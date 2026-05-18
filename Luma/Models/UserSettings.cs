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

public enum TimePreference
{
    EarlyBird,
    NightOwl,
    Both
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
    public ShootingStyle Style { get; set; } = ShootingStyle.Landscape;
    public TimePreference TimePreference { get; set; } = TimePreference.Both;
    public ExperienceLevel Experience { get; set; } = ExperienceLevel.Beginner;
}