namespace BjjSM.Client.Models;

public class Achievement
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🌟";
    public int Points { get; set; } = 10;
    public DateTime? UnlockedAt { get; set; }
    public bool IsUnlocked => UnlockedAt.HasValue;
    public string Category { get; set; } = "Talenter";
}
