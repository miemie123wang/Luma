using System.Globalization;
using Luma.Localization;
using Luma.Models;
using Luma.Services;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");

var adviceService = new ShootingAdviceService(new InMemoryStringLocalizer());

foreach (var auditCase in GetHighRiskCases())
{
    var advice = adviceService.GetAdvice(auditCase.Context);

    Console.WriteLine($"Case {auditCase.Number}: {auditCase.Name}");
    Console.WriteLine($"Phase: {auditCase.Context.Phase}");
    Console.WriteLine($"Weather: {auditCase.WeatherLabel}");
    Console.WriteLine($"Style: {auditCase.Context.Style}");
    Console.WriteLine($"Camera: {auditCase.Context.Camera}");
    Console.WriteLine($"Experience: {auditCase.Context.Experience}");
    Console.WriteLine($"Support: {auditCase.Context.SupportMode}");
    Console.WriteLine($"Subject: {auditCase.Context.SubjectMotion}");
    Console.WriteLine();
    Console.WriteLine("Local output:");
    Console.WriteLine($"Feasibility: {advice.FeasibilityWarning ?? "(none)"}");
    WriteList("First test shot", advice.ExposureSteps);
    WriteList("Watch first", advice.RiskWarnings);
    WriteList("If it is not working", advice.AdjustmentSteps);
    WriteList("Steps", advice.FieldSteps);
    Console.WriteLine();
}

static void WriteList(string heading, IReadOnlyList<string> items)
{
    Console.WriteLine($"{heading}:");

    if (items.Count == 0)
    {
        Console.WriteLine("- (none)");
        return;
    }

    foreach (var item in items)
        Console.WriteLine($"- {item}");
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