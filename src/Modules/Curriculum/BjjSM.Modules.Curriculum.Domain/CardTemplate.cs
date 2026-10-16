using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Domain;

public sealed class CardFieldDefinition
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CardFieldSide Side { get; private set; }
    public CardFieldType FieldType { get; private set; }
    public bool IsRequired { get; private set; }
    public int SortOrder { get; private set; }

    private CardFieldDefinition()
    {
    }

    public CardFieldDefinition(
        Guid id,
        string name,
        CardFieldSide side,
        CardFieldType fieldType,
        bool isRequired,
        int sortOrder)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = (name ?? string.Empty).Trim();
        Side = side;
        FieldType = fieldType;
        IsRequired = isRequired;
        SortOrder = sortOrder;
    }
}

public sealed class CardTemplate
{
    private readonly List<CardFieldDefinition> _fields = [];

    public Guid Id { get; private set; }
    public Guid AcademyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime LastModifiedAt { get; private set; }
    public IReadOnlyCollection<CardFieldDefinition> Fields => _fields.AsReadOnly();

    private CardTemplate()
    {
    }

    private CardTemplate(
        Guid id,
        Guid academyId,
        string name,
        string description,
        IEnumerable<CardFieldDefinition> fields)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        AcademyId = academyId;
        Name = name.Trim();
        Description = (description ?? string.Empty).Trim();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        _fields.AddRange(fields.OrderBy(f => f.SortOrder));
    }

    public static Result<CardTemplate> Create(
        Guid id,
        Guid academyId,
        string name,
        string description,
        IEnumerable<CardFieldDefinition> fields)
    {
        if (academyId == Guid.Empty)
            return Result.Failure<CardTemplate>("AcademyId is required.");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<CardTemplate>("Template name is required.");

        if (fields is null)
            return Result.Failure<CardTemplate>("Template fields are required.");

        var fieldList = fields.ToList();
        var validation = ValidateFields(fieldList);
        if (validation.IsFailure)
            return Result.Failure<CardTemplate>(validation.Error);

        return Result.Success(new CardTemplate(id, academyId, name, description, fieldList));
    }

    public Result Update(
        string name,
        string description,
        IEnumerable<CardFieldDefinition> fields)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Template name is required.");

        if (fields is null)
            return Result.Failure("Template fields are required.");

        var fieldList = fields.ToList();
        var validation = ValidateFields(fieldList);
        if (validation.IsFailure)
            return validation;

        Name = name.Trim();
        Description = (description ?? string.Empty).Trim();
        _fields.Clear();
        _fields.AddRange(fieldList.OrderBy(f => f.SortOrder));
        LastModifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static Result ValidateFields(List<CardFieldDefinition> fieldList)
    {
        if (fieldList.Count < 2)
            return Result.Failure("A card template must define at least one Front field and one Back field.");

        if (fieldList.Any(f => string.IsNullOrWhiteSpace(f.Name)))
            return Result.Failure("Every template field must have a non-empty name.");

        if (!fieldList.Any(f => f.Side == CardFieldSide.Front))
            return Result.Failure("A card template must contain at least one Front field.");

        if (!fieldList.Any(f => f.Side == CardFieldSide.Back))
            return Result.Failure("A card template must contain at least one Back field.");

        return Result.Success();
    }
}
