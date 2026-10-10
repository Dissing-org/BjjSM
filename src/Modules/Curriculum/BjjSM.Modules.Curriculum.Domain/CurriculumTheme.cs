using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Domain;

public sealed class CurriculumTheme
{
    private readonly List<ThemeDrillTemplate> _defaultDrills = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public string DefaultObjective { get; private set; }
    public IReadOnlyCollection<ThemeDrillTemplate> DefaultDrills => _defaultDrills.AsReadOnly();

    private CurriculumTheme()
    {
        Name = string.Empty;
        Category = string.Empty;
        DefaultObjective = string.Empty;
    }

    private CurriculumTheme(Guid id, string name, string category, string defaultObjective)
    {
        Id = id;
        Name = name;
        Category = category;
        DefaultObjective = defaultObjective;
    }

    public static Result<CurriculumTheme> Create(
        Guid id,
        string name,
        string category,
        string defaultObjective,
        IEnumerable<ThemeDrillTemplate>? drills = null)
    {
        if (id == Guid.Empty)
            return Result.Failure<CurriculumTheme>("Id is required.");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<CurriculumTheme>("Theme name is required.");

        if (string.IsNullOrWhiteSpace(category))
            return Result.Failure<CurriculumTheme>("Category is required.");

        if (string.IsNullOrWhiteSpace(defaultObjective))
            return Result.Failure<CurriculumTheme>("Default objective is required.");

        var theme = new CurriculumTheme(id, name.Trim(), category.Trim(), defaultObjective.Trim());

        if (drills is not null)
        {
            foreach (var drill in drills)
            {
                theme.AddDrill(drill);
            }
        }

        return Result.Success(theme);
    }

    public void AddDrill(ThemeDrillTemplate drill)
    {
        ArgumentNullException.ThrowIfNull(drill);
        _defaultDrills.Add(drill);
    }
}
