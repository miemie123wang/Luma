namespace Luma.Models;

public enum FieldWindowTone
{
    Good,
    Caution,
    Neutral
}

public class FieldWindowRecommendation
{
    public string Icon { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string? Detail { get; set; }
    public FieldWindowTone Tone { get; set; }
}