using System.Globalization;
using System.Text;
using Luma.Localization;
using Luma.Models;
using Luma.Services;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");

var adviceService = new ShootingAdviceService(new InMemoryStringLocalizer());
var output = new StringBuilder();

foreach (var auditCase in GetHighRiskCases())
{
    var advice = adviceService.GetAdvice(auditCase.Context);

    output.AppendLine($"Case {auditCase.Number}: {auditCase.Name}");
    output.AppendLine($"Phase: {auditCase.Context.Phase}");
    output.AppendLine($"Weather: {auditCase.WeatherLabel}");
    output.AppendLine($"Style: {auditCase.Context.Style}");
    output.AppendLine($"Camera: {auditCase.Context.Camera}");
    output.AppendLine($"Experience: {auditCase.Context.Experience}");
    output.AppendLine($"Support: {auditCase.Context.SupportMode}");
    output.AppendLine($"Subject: {auditCase.Context.SubjectMotion}");
    output.AppendLine();
    output.AppendLine("Local output:");
    output.AppendLine($"Feasibility: {advice.FeasibilityWarning ?? "(none)"}");
    AppendList(output, "First test shot", advice.ExposureSteps);
    AppendList(output, "Watch first", advice.RiskWarnings);
    AppendList(output, "If it is not working", advice.AdjustmentSteps);
    AppendList(output, "Steps", advice.FieldSteps);
    output.AppendLine();
}

var outputText = output.ToString();
var outputPath = GetOutputPath(args);

if (outputPath is null)
{
    Console.Write(outputText);
}
else
{
    var directory = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(directory))
        Directory.CreateDirectory(directory);

    File.WriteAllText(outputPath, outputText, Encoding.UTF8);
    Console.WriteLine($"Advice audit output written to {outputPath}");
}

static void AppendList(StringBuilder output, string heading, IReadOnlyList<string> items)
{
    output.AppendLine($"{heading}:");

    if (items.Count == 0)
    {
        output.AppendLine("- (none)");
        return;
    }

    foreach (var item in items)
        output.AppendLine($"- {item}");
}

static string? GetOutputPath(string[] args)
{
    for (var index = 0; index < args.Length; index++)
    {
        var arg = args[index];

        if (arg == "--out" || arg == "-o")
        {
            if (index + 1 >= args.Length)
                throw new ArgumentException("Missing value for --out.");

            return args[index + 1];
        }

        const string outPrefix = "--out=";
        if (arg.StartsWith(outPrefix, StringComparison.Ordinal))
            return arg[outPrefix.Length..];
    }

    return null;
}

static IReadOnlyList<AuditCase> GetHighRiskCases() =>
[
    new(
        1,
        "Night Handheld Landscape",
        "clear or mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        2,
        "Night Sky With Tripod",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = ClearWeather(),
            Style = ShootingStyle.NightSky,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        3,
        "Night Sky Handheld",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = ClearWeather(),
            Style = ShootingStyle.NightSky,
            Camera = CameraType.PhonePro,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        4,
        "Golden Hour Moving Portrait",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.GoldenHourEvening,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Portrait,
            Camera = CameraType.PhonePro,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Moving
        }),
    new(
        5,
        "Midday Landscape",
        "clear, low cloud cover",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Professional,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        6,
        "Foggy Urban Scene",
        "fog or low visibility",
        new ShootingAdviceContext
        {
            Phase = LightPhase.BlueHour,
            Weather = FoggyWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        7,
        "Action Cam Low Light",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.ActionCam,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Moving
        })
];

static WeatherInfo ClearWeather() => new()
{
    CloudCover = 10,
    Precipitation = 0,
    Visibility = 20000,
    Temperature = 18,
    WindSpeed = 8,
    WeatherCode = 0,
    Description = "Clear",
    IsGoodForPhoto = true
};

static WeatherInfo MixedCloudWeather() => new()
{
    CloudCover = 50,
    Precipitation = 0,
    Visibility = 12000,
    Temperature = 16,
    WindSpeed = 10,
    WeatherCode = 2,
    Description = "Mixed cloud",
    IsGoodForPhoto = true
};

static WeatherInfo FoggyWeather() => new()
{
    CloudCover = 85,
    Precipitation = 0,
    Visibility = 3000,
    Temperature = 9,
    WindSpeed = 4,
    WeatherCode = 45,
    Description = "Fog",
    IsGoodForPhoto = false
};

internal sealed record AuditCase(
    int Number,
    string Name,
    string WeatherLabel,
    ShootingAdviceContext Context);