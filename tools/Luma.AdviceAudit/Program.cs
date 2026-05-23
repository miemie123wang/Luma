using System.Globalization;
using System.Text;
using Luma.Localization;
using Luma.Models;
using Luma.Services;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");

var adviceService = new ShootingAdviceService(new InMemoryStringLocalizer());
var options = ParseOptions(args);
var output = new StringBuilder();

if (options.CheckInvariants)
{
    var failures = CheckInvariants(adviceService, GetCases(options.Set));

    output.AppendLine($"Invariant check: {options.Set}");
    output.AppendLine();

    if (failures.Count == 0)
    {
        output.AppendLine("No invariant failures found.");
    }
    else
    {
        output.AppendLine($"Failures: {failures.Count}");
        output.AppendLine();

        foreach (var failure in failures)
        {
            output.AppendLine($"Case {failure.CaseNumber}: {failure.CaseName}");
            output.AppendLine($"Invariant: {failure.InvariantName}");
            output.AppendLine($"Issue: {failure.Message}");
            output.AppendLine();
        }
    }

    WriteOutput(options, output.ToString());
    Environment.ExitCode = failures.Count == 0 ? 0 : 1;
    return;
}

output.AppendLine($"Audit set: {options.Set}");
output.AppendLine();

foreach (var auditCase in GetCases(options.Set))
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

WriteOutput(options, output.ToString());

static void WriteOutput(Options options, string outputText)
{
    if (options.OutputPath is null)
    {
        Console.Write(outputText);
        return;
    }

    var directory = Path.GetDirectoryName(options.OutputPath);
    if (!string.IsNullOrEmpty(directory))
        Directory.CreateDirectory(directory);

    File.WriteAllText(options.OutputPath, outputText, Encoding.UTF8);
    Console.WriteLine($"Advice audit output written to {options.OutputPath}");
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

static Options ParseOptions(string[] args)
{
    var set = "high-risk";
    string? outputPath = null;
    var checkInvariants = false;

    for (var index = 0; index < args.Length; index++)
    {
        var arg = args[index];

        if (arg == "--check-invariants")
        {
            checkInvariants = true;
            continue;
        }

        if (arg == "--out" || arg == "-o")
        {
            if (index + 1 >= args.Length)
                throw new ArgumentException("Missing value for --out.");

            outputPath = args[++index];
            continue;
        }

        const string outPrefix = "--out=";
        if (arg.StartsWith(outPrefix, StringComparison.Ordinal))
        {
            outputPath = arg[outPrefix.Length..];
            continue;
        }

        if (arg == "--set")
        {
            if (index + 1 >= args.Length)
                throw new ArgumentException("Missing value for --set.");

            set = args[++index];
            continue;
        }

        const string setPrefix = "--set=";
        if (arg.StartsWith(setPrefix, StringComparison.Ordinal))
        {
            set = arg[setPrefix.Length..];
            continue;
        }

        throw new ArgumentException($"Unknown argument: {arg}");
    }

    if (set is not "high-risk" and not "regression" and not "travel-fullframe-landscape" and not "travel-aps-c-landscape" and not "travel-t6-sept-iles")
        throw new ArgumentException($"Unknown audit set: {set}. Use high-risk, regression, travel-fullframe-landscape, travel-aps-c-landscape, or travel-t6-sept-iles.");

    return new Options(set, outputPath, checkInvariants);
}

static IReadOnlyList<InvariantFailure> CheckInvariants(ShootingAdviceService adviceService, IReadOnlyList<AuditCase> cases)
{
    var failures = new List<InvariantFailure>();

    foreach (var auditCase in cases)
    {
        var advice = adviceService.GetAdvice(auditCase.Context);
        var allText = GetAdviceText(advice);
        var firstRisk = advice.RiskWarnings.FirstOrDefault() ?? "";
        var firstExposure = advice.ExposureSteps.FirstOrDefault() ?? "";

        if (auditCase.Context.Camera == CameraType.PhoneBasic && ContainsAny(allText,
            "ISO", "shutter speed", "aperture", "depth of field", "f/"))
        {
            failures.Add(Fail(auditCase, "PhoneBasic manual controls", "PhoneBasic advice contains manual camera control language."));
        }

        if (auditCase.Context.Camera == CameraType.ActionCam && ContainsAny(allText,
            "tap to focus", "tap the subject", "manual ISO", "manual shutter", "shutter speed", "sport mode", "action mode", "burst"))
        {
            failures.Add(Fail(auditCase, "ActionCam device language", "ActionCam advice contains phone/manual-camera motion or control language."));
        }

        if (auditCase.Context.Camera == CameraType.ActionCam && auditCase.Context.SubjectMotion == SubjectMotion.Moving &&
            !ContainsAny(allText, "video", "high-frame-rate", "stabilization"))
        {
            failures.Add(Fail(auditCase, "ActionCam moving workflow", "ActionCam moving-subject advice does not mention video, high-frame-rate capture, or stabilization."));
        }

        if (auditCase.Context.SupportMode == CameraSupportMode.Tripod && IsLowLight(auditCase.Context) &&
            Contains(allText, "1/focal length"))
        {
            failures.Add(Fail(auditCase, "Tripod low-light handheld rule", "Tripod low-light advice contains handheld-only 1/focal length guidance."));
        }

        if (auditCase.Context.Style == ShootingStyle.NightSky &&
            (Contains(firstRisk, "highlight") || Contains(firstExposure, "protect highlights")))
        {
            failures.Add(Fail(auditCase, "NightSky daylight highlight lead", "NightSky advice leads with daylight highlight-protection language."));
        }

        if (auditCase.Context.Phase == LightPhase.Night && auditCase.Context.Style == ShootingStyle.Landscape &&
            auditCase.Context.SupportMode == CameraSupportMode.Tripod && Contains(firstRisk, "highlight"))
        {
            failures.Add(Fail(auditCase, "Night tripod landscape leading risk", "Night tripod landscape leads with highlight risk instead of noise, focus, stability, or long exposure."));
        }

        if (auditCase.Context.Experience == ExperienceLevel.Beginner && auditCase.Context.Phase == LightPhase.Night &&
            auditCase.Context.SupportMode == CameraSupportMode.Handheld && IsManualCamera(auditCase.Context.Camera) &&
            string.IsNullOrWhiteSpace(advice.FeasibilityWarning))
        {
            failures.Add(Fail(auditCase, "Beginner handheld night feasibility", "Beginner handheld night manual-camera advice has no feasibility warning."));
        }
    }

    return failures;
}

static InvariantFailure Fail(AuditCase auditCase, string invariantName, string message) =>
    new(auditCase.Number, auditCase.Name, invariantName, message);

static string GetAdviceText(ShootingAdvice advice)
{
    var parts = new List<string>();

    if (!string.IsNullOrWhiteSpace(advice.FeasibilityWarning))
        parts.Add(advice.FeasibilityWarning);

    parts.AddRange(advice.ExposureSteps);
    parts.AddRange(advice.RiskWarnings);
    parts.AddRange(advice.AdjustmentSteps);
    parts.AddRange(advice.FieldSteps);

    return string.Join("\n", parts);
}

static bool ContainsAny(string text, params string[] values) => values.Any(value => Contains(text, value));

static bool Contains(string text, string value) => text.Contains(value, StringComparison.OrdinalIgnoreCase);

static bool IsLowLight(ShootingAdviceContext context) => context.Phase is
    LightPhase.Night or
    LightPhase.AstronomicalDawn or
    LightPhase.NauticalDawn or
    LightPhase.BlueHour or
    LightPhase.BlueDusk or
    LightPhase.NauticalDusk or
    LightPhase.AstronomicalDusk ||
    context.Style == ShootingStyle.NightSky;

static bool IsManualCamera(CameraType camera) => camera is CameraType.MirrorlessAPS or CameraType.FullFrame;

static IReadOnlyList<AuditCase> GetCases(string set) => set switch
{
    "high-risk" => GetHighRiskCases(),
    "regression" => GetRegressionCases(),
    "travel-fullframe-landscape" => GetTravelFullFrameLandscapeCases(),
    "travel-aps-c-landscape" => GetTravelApsCLandscapeCases(),
    "travel-t6-sept-iles" => GetTravelT6SeptIlesCases(),
    _ => throw new ArgumentOutOfRangeException(nameof(set), set, null)
};

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

static IReadOnlyList<AuditCase> GetRegressionCases() =>
[
    new(
        1,
        "Blue Hour Handheld Landscape",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.BlueHour,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        2,
        "Golden Hour Professional Portrait",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.GoldenHourMorning,
            Weather = ClearWeather(),
            Style = ShootingStyle.Portrait,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Professional,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        3,
        "Sunset Moving Urban Beginner",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Sunset,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Moving
        }),
    new(
        4,
        "Night Tripod Urban",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        5,
        "Blue Dusk Tripod Landscape",
        "heavy cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.BlueDusk,
            Weather = HeavyCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Professional,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        6,
        "Professional Night Sky Tripod",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = ClearWeather(),
            Style = ShootingStyle.NightSky,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Professional,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        7,
        "Basic Phone Daylight Landscape",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.PhoneBasic,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        8,
        "Basic Phone Blue Hour Urban",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.BlueHour,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.PhoneBasic,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        9,
        "Basic Phone Moving Portrait",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.GoldenHourEvening,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Portrait,
            Camera = CameraType.PhoneBasic,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Moving
        }),
    new(
        10,
        "Phone Pro Night Sky Tripod",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = ClearWeather(),
            Style = ShootingStyle.NightSky,
            Camera = CameraType.PhonePro,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        11,
        "Phone Pro Rainy Night Urban",
        "rain",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = RainyWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.PhonePro,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        12,
        "Phone Pro Moving Sunset Portrait",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Sunset,
            Weather = ClearWeather(),
            Style = ShootingStyle.Portrait,
            Camera = CameraType.PhonePro,
            Experience = ExperienceLevel.Professional,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Moving
        }),
    new(
        13,
        "Action Cam Midday Moving Urban",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = ClearWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.ActionCam,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Moving
        }),
    new(
        14,
        "Action Cam Golden Hour Moving Scene",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.GoldenHourEvening,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.ActionCam,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Moving
        }),
    new(
        15,
        "Action Cam Night Still Urban",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.ActionCam,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        16,
        "Foggy Morning Urban",
        "fog or low visibility",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Morning,
            Weather = FoggyWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        17,
        "Rainy Blue Hour Portrait",
        "rain",
        new ShootingAdviceContext
        {
            Phase = LightPhase.BlueHour,
            Weather = RainyWeather(),
            Style = ShootingStyle.Portrait,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Professional,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        18,
        "Heavy Cloud Midday Landscape",
        "heavy cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = HeavyCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        19,
        "Harsh Clear Moving Portrait",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = ClearWeather(),
            Style = ShootingStyle.Portrait,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Moving
        }),
    new(
        20,
        "Mixed Cloud Afternoon Moving Urban",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Afternoon,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.PhonePro,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Moving
        }),
    new(
        21,
        "Night Handheld Portrait",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Portrait,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        22,
        "Night Tripod Landscape",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Professional,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        23,
        "Nautical Dawn Handheld Urban",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.NauticalDawn,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        24,
        "Beginner Tripod Sunset Landscape",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Sunset,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.PhoneBasic,
            Experience = ExperienceLevel.Beginner,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        })
];

static IReadOnlyList<AuditCase> GetTravelFullFrameLandscapeCases() =>
[
    new(
        1,
        "Daylight Clear Landscape Handheld",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Morning,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        2,
        "Midday Harsh Landscape Handheld",
        "clear, low cloud cover",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        3,
        "Golden Hour Landscape Handheld",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.GoldenHourEvening,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        4,
        "Sunset Tripod Landscape",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Sunset,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        5,
        "Blue Hour Landscape Handheld",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.BlueHour,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        6,
        "Foggy Blue Hour Landscape Handheld",
        "fog or low visibility",
        new ShootingAdviceContext
        {
            Phase = LightPhase.BlueHour,
            Weather = FoggyWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        7,
        "Night Landscape Tripod",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        8,
        "Night Urban Tripod With Mixed City Light",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        9,
        "Night Sky Tripod",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = ClearWeather(),
            Style = ShootingStyle.NightSky,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        10,
        "Heavy Cloud Landscape Handheld",
        "heavy cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = HeavyCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.FullFrame,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        })
];

static IReadOnlyList<AuditCase> GetTravelApsCLandscapeCases() =>
[
    new(
        1,
        "Canon EOS T6 Daylight Clear Landscape Handheld",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Morning,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        2,
        "Canon EOS T6 Midday Harsh Landscape Handheld",
        "clear, low cloud cover",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        3,
        "Canon EOS T6 Golden Hour Landscape Handheld",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.GoldenHourEvening,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        4,
        "Canon EOS T6 Sunset Tripod Landscape",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Sunset,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        5,
        "Canon EOS T6 Blue Hour Landscape Handheld",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.BlueHour,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        6,
        "Canon EOS T6 Foggy Blue Hour Landscape Handheld",
        "fog or low visibility",
        new ShootingAdviceContext
        {
            Phase = LightPhase.BlueHour,
            Weather = FoggyWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        7,
        "Canon EOS T6 Night Landscape Tripod",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        8,
        "Canon EOS T6 Night Urban Tripod With Mixed City Light",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        9,
        "Canon EOS T6 Night Sky Tripod",
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
        10,
        "Canon EOS T6 Heavy Cloud Landscape Handheld",
        "heavy cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = HeavyCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        })
];

static IReadOnlyList<AuditCase> GetTravelT6SeptIlesCases() =>
[
    new(
        1,
        "Sept-Iles Coastal Landscape Heavy Cloud Handheld",
        "heavy cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = HeavyCloudWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        2,
        "Sept-Iles Foggy Blue Hour Coast Handheld",
        "fog or low visibility",
        new ShootingAdviceContext
        {
            Phase = LightPhase.BlueHour,
            Weather = FoggyWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        3,
        "Sept-Iles Golden Hour Harbour Street Handheld",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.GoldenHourEvening,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        4,
        "Sept-Iles Moving Street Scene Afternoon",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Afternoon,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Moving
        }),
    new(
        5,
        "Sept-Iles Rainy Street Handheld",
        "rain",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Afternoon,
            Weather = RainyWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        6,
        "Sept-Iles Golden Hour Casual Portrait",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.GoldenHourEvening,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Portrait,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        7,
        "Sept-Iles Heavy Cloud Casual Portrait",
        "heavy cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = HeavyCloudWeather(),
            Style = ShootingStyle.Portrait,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        8,
        "Sept-Iles Sunset Coast Tripod",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Sunset,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        9,
        "Sept-Iles Night Harbour Lights Tripod",
        "mixed cloud",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Night,
            Weather = MixedCloudWeather(),
            Style = ShootingStyle.Urban,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Tripod,
            SubjectMotion = SubjectMotion.Still
        }),
    new(
        10,
        "Sept-Iles Clear Midday Shoreline Handheld",
        "clear",
        new ShootingAdviceContext
        {
            Phase = LightPhase.Midday,
            Weather = ClearWeather(),
            Style = ShootingStyle.Landscape,
            Camera = CameraType.MirrorlessAPS,
            Experience = ExperienceLevel.Intermediate,
            SupportMode = CameraSupportMode.Handheld,
            SubjectMotion = SubjectMotion.Still
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

static WeatherInfo HeavyCloudWeather() => new()
{
    CloudCover = 95,
    Precipitation = 0,
    Visibility = 10000,
    Temperature = 14,
    WindSpeed = 12,
    WeatherCode = 3,
    Description = "Heavy cloud",
    IsGoodForPhoto = false
};

static WeatherInfo RainyWeather() => new()
{
    CloudCover = 90,
    Precipitation = 2.5,
    Visibility = 7000,
    Temperature = 12,
    WindSpeed = 18,
    WeatherCode = 61,
    Description = "Rain",
    IsGoodForPhoto = false
};

internal sealed record Options(string Set, string? OutputPath, bool CheckInvariants);

internal sealed record InvariantFailure(
    int CaseNumber,
    string CaseName,
    string InvariantName,
    string Message);

internal sealed record AuditCase(
    int Number,
    string Name,
    string WeatherLabel,
    ShootingAdviceContext Context);