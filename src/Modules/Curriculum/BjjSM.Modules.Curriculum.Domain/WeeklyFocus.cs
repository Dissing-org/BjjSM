namespace BjjSM.Modules.Curriculum.Domain;

public sealed class WeeklyFocus
{
    private readonly List<TechniqueDrill> _techniques = [];

    public Guid Id { get; private set; }
    public int WeekNumber { get; private set; }
    public string Theme { get; private set; }
    public IReadOnlyCollection<TechniqueDrill> Techniques => _techniques.AsReadOnly();

    // Required by EF Core
    private WeeklyFocus()
    {
        Theme = string.Empty;
    }

    internal WeeklyFocus(Guid id, int weekNumber, string theme)
    {
        Id = id;
        WeekNumber = weekNumber;
        Theme = theme;
    }

    internal void UpdateTheme(string theme)
    {
        Theme = theme;
    }

    internal void AddTechnique(TechniqueDrill technique)
    {
        _techniques.Add(technique);
    }

    internal bool UpdateTechnique(Guid techniqueId, string title, TechniqueCategory category, string? notes, string? url, int sortOrder)
    {
        var technique = _techniques.FirstOrDefault(t => t.Id == techniqueId);
        if (technique is null)
            return false;

        technique.Update(title, category, notes, url, sortOrder);
        return true;
    }

    internal void RemoveTechnique(Guid techniqueId)
    {
        _techniques.RemoveAll(t => t.Id == techniqueId);
    }
}
