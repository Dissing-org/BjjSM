using BjjSM.SharedKernel;

namespace BjjSM.Modules.Memberships.Domain;

public sealed class Practitioner
{
    private readonly List<Graduation> _graduations = [];

    public Guid Id { get; private set; }
    public Guid AcademyId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public PractitionerRole Roles { get; private set; }
    public Rank InitialRank { get; private set; }
    public Rank CurrentRank { get; private set; }
    public IReadOnlyCollection<Graduation> Graduations => _graduations.AsReadOnly();

    // Required by EF Core
    private Practitioner()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        InitialRank = default!;
        CurrentRank = default!;
    }

    private Practitioner(
        Guid id,
        Guid academyId,
        string firstName,
        string lastName,
        string email,
        PractitionerRole roles,
        Rank initialRank)
    {
        Id = id;
        AcademyId = academyId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Roles = roles;
        InitialRank = initialRank;
        CurrentRank = initialRank;
    }

    public static Result<Practitioner> Create(
        Guid academyId,
        string firstName,
        string lastName,
        string email,
        PractitionerRole roles,
        Rank initialRank)
    {
        if (academyId == Guid.Empty)
            return Result.Failure<Practitioner>("AcademyId cannot be empty.");

        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<Practitioner>("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<Practitioner>("Last name is required.");

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Practitioner>("Email is required.");

        var practitioner = new Practitioner(
            Guid.NewGuid(),
            academyId,
            firstName.Trim(),
            lastName.Trim(),
            email.Trim().ToLowerInvariant(),
            roles,
            initialRank);

        return Result.Success(practitioner);
    }

    public Result Promote(Rank newRank, DateTime awardedAt, Guid awardedBy, string? notes)
    {
        if (awardedAt > DateTime.UtcNow)
        {
            return Result.Failure("Award date cannot be in the future.");
        }

        if (awardedBy == Guid.Empty)
        {
            return Result.Failure("Awarding instructor identifier is required.");
        }

        var graduation = new Graduation(Guid.NewGuid(), AcademyId, newRank, awardedAt, awardedBy, notes);
        _graduations.Add(graduation);

        RecalculateCurrentRank();
        return Result.Success();
    }

    public Result CorrectGraduation(Guid graduationId, Rank correctedRank, DateTime awardedAt, Guid awardedBy, string? notes)
    {
        if (awardedAt > DateTime.UtcNow)
        {
            return Result.Failure("Award date cannot be in the future.");
        }

        var graduation = _graduations.FirstOrDefault(g => g.Id == graduationId);
        if (graduation is null)
        {
            return Result.Failure($"Graduation with Id '{graduationId}' was not found.");
        }

        graduation.UpdateDetails(correctedRank, awardedAt, awardedBy, notes);
        RecalculateCurrentRank();

        return Result.Success();
    }

    public Result RemoveGraduation(Guid graduationId)
    {
        var graduation = _graduations.FirstOrDefault(g => g.Id == graduationId);
        if (graduation is null)
        {
            return Result.Failure($"Graduation with Id '{graduationId}' was not found.");
        }

        _graduations.Remove(graduation);
        RecalculateCurrentRank();

        return Result.Success();
    }

    private void RecalculateCurrentRank()
    {
        var latestGraduation = _graduations
            .OrderByDescending(g => g.AwardedAt)
            .FirstOrDefault();

        CurrentRank = latestGraduation?.Rank ?? InitialRank;
    }
}
