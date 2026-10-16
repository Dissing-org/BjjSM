using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using Microsoft.EntityFrameworkCore;

namespace BjjSM.Modules.Curriculum.Infrastructure.Persistence;

public sealed class SpacedRepetitionRepository : ISpacedRepetitionRepository
{
    private readonly CurriculumDbContext _context;

    public SpacedRepetitionRepository(CurriculumDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CardTemplate>> GetTemplatesAsync(Guid academyId, CancellationToken ct = default)
    {
        return await _context.CardTemplates
            .AsNoTracking()
            .Include(t => t.Fields)
            .Where(t => t.AcademyId == academyId)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<CardTemplate?> GetTemplateByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.CardTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task AddTemplateAsync(CardTemplate template, CancellationToken ct = default)
    {
        await _context.CardTemplates.AddAsync(template, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateTemplateAsync(CardTemplate template, CancellationToken ct = default)
    {
        var existingFieldIds = await _context.CardFieldDefinitions
            .AsNoTracking()
            .Where(f => EF.Property<Guid>(f, "CardTemplateId") == template.Id)
            .Select(f => f.Id)
            .ToHashSetAsync(ct);

        foreach (var entry in _context.ChangeTracker.Entries<CardFieldDefinition>())
        {
            if (entry.State == EntityState.Modified && !existingFieldIds.Contains(entry.Entity.Id))
            {
                entry.State = EntityState.Added;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteTemplateAsync(Guid id, CancellationToken ct = default)
    {
        var template = await _context.CardTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (template is not null)
        {
            _context.CardTemplates.Remove(template);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<StudyDeck>> GetDecksAsync(Guid academyId, CancellationToken ct = default)
    {
        return await _context.StudyDecks
            .AsNoTracking()
            .Include(d => d.Cards)
                .ThenInclude(c => c.FieldValues)
            .Where(d => d.AcademyId == academyId)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);
    }

    public async Task<StudyDeck?> GetDeckByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.StudyDecks
            .Include(d => d.Cards)
                .ThenInclude(c => c.FieldValues)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task AddDeckAsync(StudyDeck deck, CancellationToken ct = default)
    {
        await _context.StudyDecks.AddAsync(deck, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateDeckAsync(StudyDeck deck, CancellationToken ct = default)
    {
        var existingCardIds = await _context.Flashcards
            .AsNoTracking()
            .Where(c => c.DeckId == deck.Id)
            .Select(c => c.Id)
            .ToHashSetAsync(ct);

        var existingValueIds = await _context.CardFieldValues
            .AsNoTracking()
            .Where(v => existingCardIds.Contains(EF.Property<Guid>(v, "FlashcardId")))
            .Select(v => v.Id)
            .ToHashSetAsync(ct);

        foreach (var entry in _context.ChangeTracker.Entries<Flashcard>())
        {
            if (entry.State == EntityState.Modified && !existingCardIds.Contains(entry.Entity.Id))
            {
                entry.State = EntityState.Added;
            }
        }

        foreach (var entry in _context.ChangeTracker.Entries<CardFieldValue>())
        {
            if (entry.State == EntityState.Modified && !existingValueIds.Contains(entry.Entity.Id))
            {
                entry.State = EntityState.Added;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteDeckAsync(Guid id, CancellationToken ct = default)
    {
        var deck = await _context.StudyDecks
            .Include(d => d.Cards)
                .ThenInclude(c => c.FieldValues)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        if (deck is not null)
        {
            var logs = await _context.ReviewLogs.Where(r => r.DeckId == id).ToListAsync(ct);
            if (logs.Count > 0)
            {
                _context.ReviewLogs.RemoveRange(logs);
            }

            _context.StudyDecks.Remove(deck);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task AddReviewLogAsync(ReviewLog log, CancellationToken ct = default)
    {
        await _context.ReviewLogs.AddAsync(log, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ReviewLog>> GetReviewLogsAsync(Guid academyId, DateTime? sinceUtc = null, CancellationToken ct = default)
    {
        var deckIds = await _context.StudyDecks
            .AsNoTracking()
            .Where(d => d.AcademyId == academyId)
            .Select(d => d.Id)
            .ToListAsync(ct);

        var query = _context.ReviewLogs
            .AsNoTracking()
            .Where(r => deckIds.Contains(r.DeckId));

        if (sinceUtc.HasValue)
        {
            query = query.Where(r => r.ReviewedAt >= sinceUtc.Value);
        }

        return await query
            .OrderByDescending(r => r.ReviewedAt)
            .ToListAsync(ct);
    }
}
