namespace Luma.Models;

public enum LightPhase
{
    Night,
    AstronomicalDawn,
    NauticalDawn,
    BlueHour,
    GoldenHourMorning,
    Sunrise,
    Morning,
    Midday,
    Afternoon,
    GoldenHourEvening,
    Sunset,
    BlueDusk,
    NauticalDusk,
    AstronomicalDusk
}

public class LightPhaseInfo
{
    public LightPhase Phase { get; set; }
    public string Icon { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string NextPhase { get; set; } = "";
    public int Rating { get; set; } // 1-5 拍摄评级
}
