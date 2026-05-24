using System.Globalization;
using System.Text;
using Luma;
using Luma.Localization;
using Luma.Models;
using Luma.Services;
using Microsoft.Extensions.Localization;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");

var options = ParseOptions(args);
var localizer = new TracingStringLocalizer(new InMemoryStringLocalizer());
var adviceService = new ShootingAdviceService(localizer);
var fieldWindowService = new FieldWindowService(localizer);
var output = new StringBuilder();

if (options.CheckInvariants)
{
    var failures = options.Set == "field-window"
        ? CheckFieldWindowInvariants(fieldWindowService, GetFieldWindowCases())
        : CheckInvariants(adviceService, GetCases(options.Set));

    output.AppendLine($"Invariant check: {options.Set}");
    output.AppendLine();
    output.AppendLine($"Cases checked: {GetCaseCount(options.Set)}");
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

if (options.Set == "field-window")
{
    foreach (var auditCase in GetFieldWindowCases())
    {
        var recommendation = fieldWindowService.GetRecommendation(auditCase.Phase, auditCase.Weather);

        output.AppendLine($"Case {auditCase.Number}: {auditCase.Name}");
        output.AppendLine($"Phase: {auditCase.Phase.Phase}");
        output.AppendLine($"Weather: {auditCase.WeatherLabel}");
        output.AppendLine($"Title: {recommendation?.Title ?? "(none)"}");
        output.AppendLine($"Summary: {recommendation?.Summary ?? "(none)"}");
        output.AppendLine($"Detail: {recommendation?.Detail ?? "(none)"}");
        AppendList(output, "Notes", recommendation?.Notes ?? [], localizer, options.ShowKeys);
        output.AppendLine();
    }

    WriteOutput(options, output.ToString());
    return;
}

foreach (var auditCase in GetCases(options.Set))
{
    localizer.ClearTrace();
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
    output.AppendLine($"Feasibility: {FormatAdviceText(advice.FeasibilityWarning, localizer, options.ShowKeys)}");
    AppendList(output, "First test shot", advice.ExposureSteps, localizer, options.ShowKeys);
    AppendList(output, "Watch first", advice.RiskWarnings, localizer, options.ShowKeys);
    AppendList(output, "If it is not working", advice.AdjustmentSteps, localizer, options.ShowKeys);
    AppendList(output, "Steps", advice.FieldSteps, localizer, options.ShowKeys);
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

static void AppendList(StringBuilder output, string heading, IReadOnlyList<string> items, TracingStringLocalizer localizer, bool showKeys)
{
    output.AppendLine($"{heading}:");

    if (items.Count == 0)
    {
        output.AppendLine("- (none)");
        return;
    }

    foreach (var item in items)
        output.AppendLine($"- {FormatAdviceText(item, localizer, showKeys)}");
}

static string FormatAdviceText(string? text, TracingStringLocalizer localizer, bool showKeys)
{
    if (string.IsNullOrWhiteSpace(text))
        return "(none)";

    if (!showKeys)
        return text;

    var keys = localizer.GetKeysForValue(text);
    return keys.Count == 0
        ? text
        : $"[{string.Join(", ", keys)}] {text}";
}

static Options ParseOptions(string[] args)
{
    var set = "high-risk";
    string? outputPath = null;
    var checkInvariants = false;
    var showKeys = false;

    for (var index = 0; index < args.Length; index++)
    {
        var arg = args[index];

        if (arg == "--check-invariants")
        {
            checkInvariants = true;
            continue;
        }

        if (arg == "--show-keys")
        {
            showKeys = true;
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

    if (set is not "high-risk" and not "regression" and not "travel-fullframe-landscape" and not "travel-aps-c-landscape" and not "travel-t6-sept-iles" and not "matrix-smoke" and not "field-window")
        throw new ArgumentException($"Unknown audit set: {set}. Use high-risk, regression, travel-fullframe-landscape, travel-aps-c-landscape, travel-t6-sept-iles, matrix-smoke, or field-window.");

    return new Options(set, outputPath, checkInvariants, showKeys);
}

static int GetCaseCount(string set) => set == "field-window"
    ? GetFieldWindowCases().Count
    : GetCases(set).Count;

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
            "tap to focus", "tap the subject", "ISO", "manual shutter", "shutter speed", "sport mode", "action mode", "burst"))
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

static InvariantFailure FailFieldWindow(FieldWindowAuditCase auditCase, string invariantName, string message) =>
    new(auditCase.Number, auditCase.Name, invariantName, message);

static IReadOnlyList<InvariantFailure> CheckFieldWindowInvariants(FieldWindowService fieldWindowService, IReadOnlyList<FieldWindowAuditCase> cases)
{
    var failures = new List<InvariantFailure>();

    foreach (var auditCase in cases)
    {
        var recommendation = fieldWindowService.GetRecommendation(auditCase.Phase, auditCase.Weather);
        var allText = GetFieldWindowText(recommendation);

        if (auditCase.Invariant == FieldWindowInvariant.OvercastLowLight)
        {
            if (recommendation == null)
            {
                failures.Add(FailFieldWindow(auditCase, "Overcast low-light recommendation", "No recommendation was produced."));
                continue;
            }

            if (recommendation.Tone != FieldWindowTone.Caution)
                failures.Add(FailFieldWindow(auditCase, "Overcast low-light tone", "Overcast blue-hour/night guidance should be cautionary, not a generic good window."));

            if (!ContainsAny(allText, "night-light", "city lights", "reflections", "silhouettes"))
                failures.Add(FailFieldWindow(auditCase, "Overcast low-light subject direction", "Overcast low-light guidance does not point toward night-light subjects."));

            if (ContainsAny(allText, "Good window now", "golden-hour color may be weak", "you do not need to wait"))
                failures.Add(FailFieldWindow(auditCase, "Overcast low-light daylight copy", "Overcast low-light guidance reused daylight overcast copy."));
        }
        else if (auditCase.Invariant == FieldWindowInvariant.OvercastDaylight)
        {
            if (recommendation == null)
            {
                failures.Add(FailFieldWindow(auditCase, "Overcast daylight recommendation", "No recommendation was produced."));
                continue;
            }

            if (recommendation.Tone != FieldWindowTone.Good)
                failures.Add(FailFieldWindow(auditCase, "Overcast daylight tone", "Daylight overcast guidance should be a good stable-light window."));

            if (!ContainsAny(allText, "soft and stable", "water", "foliage", "street details", "portraits"))
                failures.Add(FailFieldWindow(auditCase, "Overcast daylight subject direction", "Daylight overcast guidance does not keep the stable soft-light recipe."));
        }
        else if (auditCase.Invariant == FieldWindowInvariant.RainSoon)
        {
            if (recommendation == null)
            {
                failures.Add(FailFieldWindow(auditCase, "Rain soon recommendation", "No recommendation was produced for upcoming rain."));
                continue;
            }

            if (!ContainsAny(allText, "Rain risk rises", "must-have shot", "before the weather turns"))
                failures.Add(FailFieldWindow(auditCase, "Rain soon timing", "Upcoming rain guidance does not make the timing risk explicit."));
        }
        else if (auditCase.Invariant == FieldWindowInvariant.TomorrowRainHigh)
        {
            if (recommendation == null)
            {
                failures.Add(FailFieldWindow(auditCase, "Tomorrow rain recommendation", "No recommendation was produced for high rain tomorrow."));
                continue;
            }

            if (!ContainsAny(allText, "tomorrow rain chance", "today", "dry window"))
                failures.Add(FailFieldWindow(auditCase, "Tomorrow rain timing", "High rain tomorrow guidance does not make today's window explicit."));
        }
    }

    return failures;
}

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

static string GetFieldWindowText(FieldWindowRecommendation? recommendation)
{
    if (recommendation == null)
        return "";

    var parts = new List<string>
    {
        recommendation.Title,
        recommendation.Summary
    };

    if (!string.IsNullOrWhiteSpace(recommendation.Detail))
        parts.Add(recommendation.Detail);

    parts.AddRange(recommendation.Notes);

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
    "matrix-smoke" => GetMatrixSmokeCases(),
    _ => throw new ArgumentOutOfRangeException(nameof(set), set, null)
};

static IReadOnlyList<AuditCase> GetMatrixSmokeCases()
{
    var cases = new List<AuditCase>();
    var number = 1;

    foreach (var scenario in GetMatrixSmokeScenarios())
    {
        foreach (var camera in Enum.GetValues<CameraType>())
        {
            foreach (var experience in Enum.GetValues<ExperienceLevel>())
            {
                cases.Add(new AuditCase(
                    number++,
                    $"{scenario.Name} - {camera} - {experience}",
                    scenario.WeatherLabel,
                    new ShootingAdviceContext
                    {
                        Phase = scenario.Phase,
                        Weather = scenario.GetWeather(),
                        Style = scenario.Style,
                        Camera = camera,
                        Experience = experience,
                        SupportMode = scenario.SupportMode,
                        SubjectMotion = scenario.SubjectMotion
                    }));
            }
        }
    }

    return cases;
}

static IReadOnlyList<MatrixSmokeScenario> GetMatrixSmokeScenarios() =>
[
    new("Midday clear landscape handheld", LightPhase.Midday, "clear", ClearWeather, ShootingStyle.Landscape, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Midday clear moving portrait", LightPhase.Midday, "clear", ClearWeather, ShootingStyle.Portrait, CameraSupportMode.Handheld, SubjectMotion.Moving),
    new("Midday clear moving urban", LightPhase.Midday, "clear", ClearWeather, ShootingStyle.Urban, CameraSupportMode.Handheld, SubjectMotion.Moving),
    new("Midday heavy-cloud landscape", LightPhase.Midday, "heavy cloud", HeavyCloudWeather, ShootingStyle.Landscape, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Foggy morning urban", LightPhase.Morning, "fog or low visibility", FoggyWeather, ShootingStyle.Urban, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Rainy afternoon urban", LightPhase.Afternoon, "rain", RainyWeather, ShootingStyle.Urban, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Golden-hour landscape", LightPhase.GoldenHourEvening, "mixed cloud", MixedCloudWeather, ShootingStyle.Landscape, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Golden-hour moving portrait", LightPhase.GoldenHourEvening, "mixed cloud", MixedCloudWeather, ShootingStyle.Portrait, CameraSupportMode.Handheld, SubjectMotion.Moving),
    new("Sunset tripod landscape", LightPhase.Sunset, "clear", ClearWeather, ShootingStyle.Landscape, CameraSupportMode.Tripod, SubjectMotion.Still),
    new("Sunset moving urban", LightPhase.Sunset, "clear", ClearWeather, ShootingStyle.Urban, CameraSupportMode.Handheld, SubjectMotion.Moving),
    new("Blue-hour handheld landscape", LightPhase.BlueHour, "mixed cloud", MixedCloudWeather, ShootingStyle.Landscape, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Blue-hour handheld urban", LightPhase.BlueHour, "mixed cloud", MixedCloudWeather, ShootingStyle.Urban, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Blue-hour moving portrait", LightPhase.BlueHour, "mixed cloud", MixedCloudWeather, ShootingStyle.Portrait, CameraSupportMode.Handheld, SubjectMotion.Moving),
    new("Blue-dusk tripod landscape", LightPhase.BlueDusk, "heavy cloud", HeavyCloudWeather, ShootingStyle.Landscape, CameraSupportMode.Tripod, SubjectMotion.Still),
    new("Nautical dawn handheld urban", LightPhase.NauticalDawn, "mixed cloud", MixedCloudWeather, ShootingStyle.Urban, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Night handheld landscape", LightPhase.Night, "mixed cloud", MixedCloudWeather, ShootingStyle.Landscape, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Night tripod landscape", LightPhase.Night, "mixed cloud", MixedCloudWeather, ShootingStyle.Landscape, CameraSupportMode.Tripod, SubjectMotion.Still),
    new("Night handheld urban", LightPhase.Night, "mixed cloud", MixedCloudWeather, ShootingStyle.Urban, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Night tripod urban", LightPhase.Night, "mixed cloud", MixedCloudWeather, ShootingStyle.Urban, CameraSupportMode.Tripod, SubjectMotion.Still),
    new("Night handheld portrait", LightPhase.Night, "mixed cloud", MixedCloudWeather, ShootingStyle.Portrait, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Night moving portrait", LightPhase.Night, "mixed cloud", MixedCloudWeather, ShootingStyle.Portrait, CameraSupportMode.Handheld, SubjectMotion.Moving),
    new("Night-sky handheld", LightPhase.Night, "clear", ClearWeather, ShootingStyle.NightSky, CameraSupportMode.Handheld, SubjectMotion.Still),
    new("Night-sky tripod", LightPhase.Night, "clear", ClearWeather, ShootingStyle.NightSky, CameraSupportMode.Tripod, SubjectMotion.Still),
    new("Astronomical-dusk night-sky tripod", LightPhase.AstronomicalDusk, "clear", ClearWeather, ShootingStyle.NightSky, CameraSupportMode.Tripod, SubjectMotion.Still)
];

static IReadOnlyList<FieldWindowAuditCase> GetFieldWindowCases() =>
[
    new(
        1,
        "Overcast blue dusk is night-light specific",
        "heavy cloud, dry, low upcoming rain",
        MakePhaseInfo(LightPhase.BlueDusk, "Phase_BlueDusk"),
        HeavyCloudWeatherWithForecast(tomorrowRainChance: 55, hourlyProbabilities: [4, 7, 6, 5]),
        FieldWindowInvariant.OvercastLowLight),
    new(
        2,
        "Overcast afternoon remains a soft-light window",
        "heavy cloud, dry, low upcoming rain",
        MakePhaseInfo(LightPhase.Afternoon, "Phase_Afternoon"),
        HeavyCloudWeatherWithForecast(tomorrowRainChance: 20, hourlyProbabilities: [5, 8, 12, 10]),
        FieldWindowInvariant.OvercastDaylight),
    new(
        3,
        "Dry now but rain risk rises soon",
        "mixed cloud, dry now, rain risk in forecast",
        MakePhaseInfo(LightPhase.Afternoon, "Phase_Afternoon"),
        MixedCloudWeatherWithForecast(tomorrowRainChance: 35, hourlyProbabilities: [10, 25, 65, 70]),
        FieldWindowInvariant.RainSoon),
    new(
        4,
        "Dry today before high rain tomorrow",
        "mixed cloud, dry now, high rain tomorrow",
        MakePhaseInfo(LightPhase.Morning, "Phase_Morning"),
        MixedCloudWeatherWithForecast(tomorrowRainChance: 80, hourlyProbabilities: [5, 10, 12, 8]),
        FieldWindowInvariant.TomorrowRainHigh)
];

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

static WeatherInfo HeavyCloudWeatherWithForecast(int tomorrowRainChance, IReadOnlyList<int> hourlyProbabilities)
{
    var weather = HeavyCloudWeather();
    weather.TomorrowPrecipitationProbability = tomorrowRainChance;
    weather.HourlyForecast = MakeHourlyForecast(hourlyProbabilities);
    return weather;
}

static WeatherInfo MixedCloudWeatherWithForecast(int tomorrowRainChance, IReadOnlyList<int> hourlyProbabilities)
{
    var weather = MixedCloudWeather();
    weather.TomorrowPrecipitationProbability = tomorrowRainChance;
    weather.HourlyForecast = MakeHourlyForecast(hourlyProbabilities);
    return weather;
}

static IReadOnlyList<HourlyWeatherForecast> MakeHourlyForecast(IReadOnlyList<int> probabilities)
{
    var start = DateTime.Today.AddHours(14);
    return probabilities
        .Select((probability, index) => new HourlyWeatherForecast
        {
            Time = start.AddHours(index),
            CloudCover = 80,
            PrecipitationProbability = probability,
            Precipitation = 0
        })
        .ToList();
}

static LightPhaseInfo MakePhaseInfo(LightPhase phase, string keyPrefix) => new()
{
    Phase = phase,
    Icon = "",
    Name = $"{keyPrefix}_Name",
    Description = $"{keyPrefix}_Desc",
    NextPhase = $"{keyPrefix}_Next",
    Rating = 3
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

internal sealed record Options(string Set, string? OutputPath, bool CheckInvariants, bool ShowKeys);

internal sealed class TracingStringLocalizer : IStringLocalizer<SharedResource>
{
    private readonly IStringLocalizer<SharedResource> _inner;
    private readonly List<TraceEntry> _trace = [];

    public TracingStringLocalizer(IStringLocalizer<SharedResource> inner)
    {
        _inner = inner;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var localized = _inner[name];
            _trace.Add(new TraceEntry(name, localized.Value));
            return localized;
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var localized = _inner[name, arguments];
            _trace.Add(new TraceEntry(name, localized.Value));
            return localized;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        _inner.GetAllStrings(includeParentCultures);

    public void ClearTrace() => _trace.Clear();

    public IReadOnlyList<string> GetKeysForValue(string value) => _trace
        .Where(entry => entry.Value == value)
        .Select(entry => entry.Key)
        .Distinct(StringComparer.Ordinal)
        .ToList();
}

internal sealed record TraceEntry(string Key, string Value);

internal sealed record InvariantFailure(
    int CaseNumber,
    string CaseName,
    string InvariantName,
    string Message);

internal enum FieldWindowInvariant
{
    OvercastLowLight,
    OvercastDaylight,
    RainSoon,
    TomorrowRainHigh
}

internal sealed record FieldWindowAuditCase(
    int Number,
    string Name,
    string WeatherLabel,
    LightPhaseInfo Phase,
    WeatherInfo Weather,
    FieldWindowInvariant Invariant);

internal sealed record MatrixSmokeScenario(
    string Name,
    LightPhase Phase,
    string WeatherLabel,
    Func<WeatherInfo> GetWeather,
    ShootingStyle Style,
    CameraSupportMode SupportMode,
    SubjectMotion SubjectMotion);

internal sealed record AuditCase(
    int Number,
    string Name,
    string WeatherLabel,
    ShootingAdviceContext Context);