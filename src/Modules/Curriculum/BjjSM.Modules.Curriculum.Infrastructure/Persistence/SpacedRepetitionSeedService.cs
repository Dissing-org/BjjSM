using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using BjjSM.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace BjjSM.Modules.Curriculum.Infrastructure.Persistence;

public sealed class SpacedRepetitionSeedService : ISpacedRepetitionSeedService
{
    private readonly CurriculumDbContext _context;

    public SpacedRepetitionSeedService(CurriculumDbContext context)
    {
        _context = context;
    }

    public async Task EnsurePresetsAndBackfillAsync(Guid academyId, CancellationToken ct = default)
    {
        var existingTemplates = await _context.CardTemplates
            .Include(t => t.Fields)
            .Where(t => t.AcademyId == academyId)
            .ToListAsync(ct);

        if (existingTemplates.Count == 0)
        {
            var defaultTemplates = SpacedRepetitionPresets.CreateDefaultTemplates(academyId);
            _context.CardTemplates.AddRange(defaultTemplates);
            await _context.SaveChangesAsync(ct);

            var starterDecks = SpacedRepetitionPresets.CreateStarterDecks(academyId, defaultTemplates);
            _context.StudyDecks.AddRange(starterDecks);
            await _context.SaveChangesAsync(ct);
            return;
        }

        // 1. Backfill ImageUrl definitions in templates if missing
        foreach (var t in existingTemplates)
        {
            var hasImageField = t.Fields.Any(f => f.FieldType == CardFieldType.ImageUrl);
            if (!hasImageField)
            {
                var fieldDefId = t.Id == SpacedRepetitionPresets.PositionalCounterTemplateId
                    ? SpacedRepetitionPresets.PositionalImageFieldId
                    : t.Id == SpacedRepetitionPresets.SubmissionMechanicsTemplateId
                        ? SpacedRepetitionPresets.SubmissionImageFieldId
                        : SpacedRepetitionPresets.StandardImageFieldId;

                var nextSort = t.Fields.Count > 0 ? t.Fields.Max(f => f.SortOrder) + 1 : 1;
                var newFields = t.Fields.ToList();
                newFields.Add(new CardFieldDefinition(
                    fieldDefId,
                    "Technique Illustration / GIF",
                    CardFieldSide.Back,
                    CardFieldType.ImageUrl,
                    false,
                    nextSort));

                t.Update(t.Name, t.Description, newFields);
            }
        }
        await _context.SaveChangesAsync(ct);

        // 2. Non-destructively backfill technique GIFs on starter deck cards while preserving SM-2 learning progress
        var decks = await _context.StudyDecks
            .Include(d => d.Cards)
                .ThenInclude(c => c.FieldValues)
            .Where(d => d.AcademyId == academyId)
            .ToListAsync(ct);

        var refreshedTemplates = await _context.CardTemplates
            .Include(t => t.Fields)
            .Where(t => t.AcademyId == academyId)
            .ToListAsync(ct);

        var templateMap = refreshedTemplates.ToDictionary(t => t.Id);

        foreach (var deck in decks)
        {
            if (!templateMap.TryGetValue(deck.TemplateId, out var template))
                continue;

            var imageDef = template.Fields.FirstOrDefault(f => f.FieldType == CardFieldType.ImageUrl);
            if (imageDef is null)
                continue;

            foreach (var card in deck.Cards)
            {
                var frontVal = card.FieldValues
                    .Where(f => f.Side == CardFieldSide.Front)
                    .OrderBy(f => f.SortOrder)
                    .FirstOrDefault()?.Value ?? string.Empty;

                var gifPath = ResolveGifForCard(frontVal);
                if (string.IsNullOrEmpty(gifPath))
                    continue;

                var existingImageVal = card.FieldValues.FirstOrDefault(f => f.FieldType == CardFieldType.ImageUrl);
                if (existingImageVal is null)
                {
                    var updatedValues = card.FieldValues.ToList();
                    var nextSort = updatedValues.Count > 0 ? updatedValues.Max(v => v.SortOrder) + 1 : 1;
                    updatedValues.Add(new CardFieldValue(
                        Guid.NewGuid(),
                        imageDef.Id,
                        imageDef.Name,
                        CardFieldSide.Back,
                        CardFieldType.ImageUrl,
                        gifPath,
                        nextSort));

                    card.UpdateContent(card.GetTags(), updatedValues);
                }
                else if (string.IsNullOrWhiteSpace(existingImageVal.Value))
                {
                    var updatedValues = card.FieldValues.Select(v =>
                        v.Id == existingImageVal.Id
                            ? new CardFieldValue(v.Id, v.FieldDefinitionId, v.FieldName, v.Side, v.FieldType, gifPath, v.SortOrder)
                            : v).ToList();

                    card.UpdateContent(card.GetTags(), updatedValues);
                }
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<Result> ResetStarterDecksAsync(Guid academyId, CancellationToken ct = default)
    {
        var starterDeckIds = new[]
        {
            SpacedRepetitionPresets.GuardRetentionDeckId,
            SpacedRepetitionPresets.SubmissionsDeckId
        };

        var starterDecks = await _context.StudyDecks
            .Include(d => d.Cards)
                .ThenInclude(c => c.FieldValues)
            .Where(d => d.AcademyId == academyId && starterDeckIds.Contains(d.Id))
            .ToListAsync(ct);

        if (starterDecks.Count > 0)
        {
            var logs = await _context.ReviewLogs
                .Where(r => starterDeckIds.Contains(r.DeckId))
                .ToListAsync(ct);

            if (logs.Count > 0)
            {
                _context.ReviewLogs.RemoveRange(logs);
            }

            _context.StudyDecks.RemoveRange(starterDecks);
            await _context.SaveChangesAsync(ct);
        }

        var templates = await _context.CardTemplates
            .Include(t => t.Fields)
            .Where(t => t.AcademyId == academyId)
            .ToListAsync(ct);

        if (templates.Count == 0)
        {
            templates = SpacedRepetitionPresets.CreateDefaultTemplates(academyId).ToList();
            _context.CardTemplates.AddRange(templates);
            await _context.SaveChangesAsync(ct);
        }

        var freshStarterDecks = SpacedRepetitionPresets.CreateStarterDecks(academyId, templates);
        _context.StudyDecks.AddRange(freshStarterDecks);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static string? ResolveGifForCard(string frontTitle)
    {
        if (frontTitle.Contains("De La Riva", StringComparison.OrdinalIgnoreCase))
            return SpacedRepetitionPresets.TechniqueGifs.DeLaRivaSingleLegX;

        if (frontTitle.Contains("Knee Shield", StringComparison.OrdinalIgnoreCase))
            return SpacedRepetitionPresets.TechniqueGifs.KneeShieldShoulderCrunch;

        if (frontTitle.Contains("Open Guard", StringComparison.OrdinalIgnoreCase) || frontTitle.Contains("Granby", StringComparison.OrdinalIgnoreCase))
            return SpacedRepetitionPresets.TechniqueGifs.OpenGuardGranbyRecovery;

        if (frontTitle.Contains("Closed Guard", StringComparison.OrdinalIgnoreCase))
            return SpacedRepetitionPresets.TechniqueGifs.ClosedGuardHipBump;

        if (frontTitle.Contains("Rear Naked", StringComparison.OrdinalIgnoreCase))
            return SpacedRepetitionPresets.TechniqueGifs.RearNakedChoke;

        if (frontTitle.Contains("Juji-Gatame", StringComparison.OrdinalIgnoreCase) || frontTitle.Contains("Armbar", StringComparison.OrdinalIgnoreCase))
            return SpacedRepetitionPresets.TechniqueGifs.ArmbarFromMount;

        if (frontTitle.Contains("Front Triangle", StringComparison.OrdinalIgnoreCase) || frontTitle.Contains("Triangle", StringComparison.OrdinalIgnoreCase))
            return SpacedRepetitionPresets.TechniqueGifs.TriangleChokeGuard;

        if (frontTitle.Contains("Cross Ashi", StringComparison.OrdinalIgnoreCase) || frontTitle.Contains("Heel Hook", StringComparison.OrdinalIgnoreCase))
            return SpacedRepetitionPresets.TechniqueGifs.InsideHeelHook;

        return null;
    }
}
