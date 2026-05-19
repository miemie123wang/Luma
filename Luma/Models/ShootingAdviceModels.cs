namespace Luma.Models;

public enum CameraSupportMode
{
    Handheld,
    Tripod
}

public enum SubjectMotion
{
    Still,
    Moving
}

public class ShootingAdviceContext
{
    public LightPhase Phase { get; set; }
    public WeatherInfo? Weather { get; set; }
    public ShootingStyle Style { get; set; }
    public CameraType Camera { get; set; }
    public ExperienceLevel Experience { get; set; }
    public CameraSupportMode SupportMode { get; set; } = CameraSupportMode.Handheld;
    public SubjectMotion SubjectMotion { get; set; } = SubjectMotion.Still;
}

public class ShootingAdvice
{
    public string Title { get; set; } = "";
    public IReadOnlyList<string> StartingPoints { get; set; } = [];
    public IReadOnlyList<string> RiskWarnings { get; set; } = [];
    public IReadOnlyList<string> AdjustmentSteps { get; set; } = [];
}
