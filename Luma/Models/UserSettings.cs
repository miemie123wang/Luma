namespace Luma.Models;

public enum CameraType
{
    PhoneBasic,
    PhonePro,
    MirrorlessAPS,
    FullFrame,
    ActionCam
}

public enum ShootingStyle
{
    Landscape,
    Urban,
    Portrait,
    NightSky
}

public enum ExperienceLevel
{
    Beginner,
    Intermediate,
    Professional
}

public class UserSettings
{
    public CameraType Camera { get; set; } = CameraType.PhonePro;
    public ExperienceLevel Experience { get; set; } = ExperienceLevel.Beginner;
    public string Language { get; set; } = "en";
}