using Luma.Models;
using Microsoft.Extensions.Localization;

namespace Luma.Services;

public class ShootingAdviceService
{
    private enum AdviceRisk
    {
        Blur,
        Highlight,
        Noise,
        Contrast,
        NightSky
    }

    private readonly IStringLocalizer<SharedResource> _localizer;

    public ShootingAdviceService(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public ShootingAdvice GetAdvice(ShootingAdviceContext context)
    {
        var primaryRisk = GetPrimaryRisk(context);
        var feasibilityWarning = GetFeasibility(context);
        var exposureSteps = new List<string>
        {
            GetExposure(context)
        };

        if (!ShouldKeepFirstTestFocused(context))
        {
            exposureSteps.Add(GetSafeStartingPoint(context));
            exposureSteps.Add(GetDeviceOperation(context));
        }

        var riskWarnings = new List<string> { GetRiskWarning(primaryRisk) };
        riskWarnings.AddRange(GetCaptureConditionSettings(context));

        if (context.Weather != null)
        {
            var weatherNote = GetWeatherNote(context, context.Weather);
            if (!string.IsNullOrEmpty(weatherNote))
                riskWarnings.Add(weatherNote);
        }

        var adjustmentSteps = new List<string>
        {
            GetAdjustmentStep(primaryRisk),
            GetExperienceNote(context.Experience)
        };

        return new ShootingAdvice
        {
            Title = _localizer["Advice_Title"],
            FeasibilityWarning = feasibilityWarning,
            ExposureSteps = exposureSteps,
            RiskWarnings = riskWarnings,
            AdjustmentSteps = adjustmentSteps,
            FieldSteps = GetFieldSteps(context).ToList()
        };
    }

    private string? GetFeasibility(ShootingAdviceContext context)
    {
        var lowLight = IsLowLight(context.Phase) || context.Style == ShootingStyle.NightSky;

        if (context.Camera == CameraType.ActionCam && context.Phase == LightPhase.Night && context.SubjectMotion == SubjectMotion.Moving)
            return _localizer["Advice_Feasibility_ActionCamNightMoving"];

        if (context.Experience == ExperienceLevel.Beginner && context.SubjectMotion == SubjectMotion.Moving)
            return _localizer["Advice_Feasibility_BeginnerMoving"];

        if (lowLight && context.SupportMode == CameraSupportMode.Handheld &&
            context.Style is ShootingStyle.Landscape or ShootingStyle.NightSky)
            return _localizer["Advice_Feasibility_HandheldNight"];

        if (context.Style == ShootingStyle.NightSky && context.SupportMode != CameraSupportMode.Tripod)
            return _localizer["Advice_Feasibility_NightSkyNeedsTripod"];

        return null;
    }

    private static bool ShouldKeepFirstTestFocused(ShootingAdviceContext context)
    {
        var lowLight = IsLowLight(context.Phase) || context.Style == ShootingStyle.NightSky;

        return lowLight &&
            context.SupportMode == CameraSupportMode.Handheld &&
            context.Style is ShootingStyle.Landscape or ShootingStyle.NightSky;
    }

    private string GetExposure(ShootingAdviceContext context)
    {
        return context.Camera switch
        {
            CameraType.PhoneBasic or CameraType.PhonePro => GetPhoneExposure(context),
            CameraType.ActionCam => GetActionCamExposure(context),
            CameraType.MirrorlessAPS or CameraType.FullFrame => GetCameraExposure(context),
            _ => GetPhoneExposure(context)
        };
    }

    private string GetCameraExposure(ShootingAdviceContext context)
    {
        var blueHour = IsBlueHour(context.Phase);
        var lowLight = IsLowLight(context.Phase) || context.Style == ShootingStyle.NightSky;
        var harshLight = IsHarshLight(context.Phase) || context.Weather?.CloudCover <= 20;
        var fullFrame = context.Camera == CameraType.FullFrame;
        var cameraAssumption = fullFrame
            ? _localizer["Advice_Assumption_FullFrame"]
            : _localizer["Advice_Assumption_APS"];

        var mode = context.Experience == ExperienceLevel.Beginner
            ? _localizer["Advice_Params_Mode_Beginner"]
            : context.Experience == ExperienceLevel.Intermediate
                ? _localizer["Advice_Params_Mode_Intermediate"]
                : _localizer["Advice_Params_Mode_Professional"];

        if (context.Style == ShootingStyle.NightSky && context.SupportMode == CameraSupportMode.Tripod)
            return _localizer["Advice_Exposure_Camera_NightSkyTripod", mode, fullFrame ? "ISO 1600" : "ISO 3200", fullFrame ? "f/2.8" : "f/3.5", "10s", cameraAssumption];

        if (lowLight && context.SupportMode == CameraSupportMode.Tripod)
            return _localizer["Advice_Exposure_Camera_LowLightTripod", mode, "ISO 200", context.Style == ShootingStyle.Landscape ? "f/5.6" : "f/4", "2s", cameraAssumption];

        if (blueHour && context.SupportMode == CameraSupportMode.Handheld)
            return _localizer["Advice_Exposure_Camera_BlueHourHandheld", mode, fullFrame ? "ISO 800-1600" : "ISO 1600-3200", fullFrame ? "f/2.8-f/4" : "f/3.5-f/5.6", context.SubjectMotion == SubjectMotion.Moving ? "1/250s" : "1/80s", cameraAssumption];

        if (lowLight)
            return _localizer["Advice_Exposure_Camera_LowLightHandheld", mode, fullFrame ? "ISO 3200" : "ISO 6400", fullFrame ? "f/2.8-f/4" : "f/3.5-f/5.6", "1/60s", cameraAssumption];

        if (context.SubjectMotion == SubjectMotion.Moving)
            return _localizer["Advice_Exposure_Camera_Moving", mode, "ISO 400", GetDaylightAperture(context, fullFrame), "1/500s", cameraAssumption];

        if (harshLight)
            return _localizer["Advice_Exposure_Camera_Harsh", mode, "ISO 100", GetDaylightAperture(context, fullFrame), "1/250s", "-0.3 EV", cameraAssumption];

        return _localizer["Advice_Exposure_Camera_Daylight", mode, "ISO 100", GetDaylightAperture(context, fullFrame), "1/125s", cameraAssumption];
    }

    private static string GetDaylightAperture(ShootingAdviceContext context, bool fullFrame)
    {
        return context.Style switch
        {
            ShootingStyle.Portrait when fullFrame => "f/1.8-f/4",
            ShootingStyle.Portrait => "f/2.8-f/5.6",
            ShootingStyle.Landscape => "f/8-f/11",
            ShootingStyle.Urban => "f/4-f/8",
            _ => "f/4-f/8"
        };
    }

    private string GetPhoneExposure(ShootingAdviceContext context)
    {
        var lowLight = IsLowLight(context.Phase) || context.Style == ShootingStyle.NightSky;
        var harshLight = IsHarshLight(context.Phase) || context.Weather?.CloudCover <= 20;
        var proPhone = context.Camera == CameraType.PhonePro;

        var lens = context.Style switch
        {
            ShootingStyle.Portrait when proPhone => _localizer["Advice_Params_Phone_Lens_PortraitPro"],
            ShootingStyle.Urban when proPhone => _localizer["Advice_Params_Phone_Lens_UrbanPro"],
            _ => _localizer["Advice_Params_Phone_Lens_Default"]
        };

        var mode = lowLight
            ? _localizer["Advice_Params_Phone_Mode_LowLight"]
            : harshLight
                ? _localizer["Advice_Params_Phone_Mode_Harsh"]
                : _localizer["Advice_Params_Phone_Mode_Default"];

        var exposure = harshLight
            ? _localizer["Advice_Params_Phone_Exposure_Harsh"]
            : lowLight
                ? _localizer["Advice_Params_Phone_Exposure_LowLight"]
                : _localizer["Advice_Params_Phone_Exposure_Default"];

        var stability = context.SupportMode == CameraSupportMode.Tripod
            ? _localizer["Advice_Params_Phone_Stability_Tripod"]
            : context.SubjectMotion == SubjectMotion.Moving
                ? _localizer["Advice_Params_Phone_Stability_Moving"]
                : _localizer["Advice_Params_Phone_Stability_Handheld"];

        return _localizer["Advice_Params_Phone", lens, mode, exposure, stability];
    }

    private string GetActionCamExposure(ShootingAdviceContext context)
    {
        var lowLight = IsLowLight(context.Phase) || context.Style == ShootingStyle.NightSky;
        var harshLight = IsHarshLight(context.Phase) || context.Weather?.CloudCover <= 20;

        var frameRate = context.SubjectMotion == SubjectMotion.Moving ? "4K 60fps" : "4K 30fps";
        var exposure = harshLight ? "-0.5 EV" : lowLight ? "0 EV, avoid fast motion" : "0 EV";
        var stabilization = context.SupportMode == CameraSupportMode.Tripod ? "stabilization low/off on tripod" : "stabilization on";

        return _localizer["Advice_Params_ActionCam", frameRate, exposure, stabilization];
    }

    private IEnumerable<string> GetFieldSteps(ShootingAdviceContext context)
    {
        if (context.Experience != ExperienceLevel.Beginner)
            yield break;

        if ((IsLowLight(context.Phase) || context.Style == ShootingStyle.NightSky) && context.SupportMode == CameraSupportMode.Handheld)
        {
            yield return _localizer["Advice_Step_FindSupport"];
            yield return _localizer["Advice_Step_Timer"];
            yield return _localizer["Advice_Step_TestShot"];
            yield break;
        }

        if (context.Style == ShootingStyle.NightSky || context.SupportMode == CameraSupportMode.Tripod)
        {
            yield return _localizer["Advice_Step_TripodFrame"];
            yield return _localizer["Advice_Step_Timer"];
            yield return _localizer["Advice_Step_TestShot"];
            yield break;
        }

        yield return _localizer["Advice_Step_SafeShot"];
        yield return _localizer["Advice_Step_CheckScreen"];
    }

    private string GetSafeStartingPoint(ShootingAdviceContext context)
    {
        var lowLight = IsLowLight(context.Phase) || context.Style == ShootingStyle.NightSky;
        var harshLight = IsHarshLight(context.Phase);

        return context.Style switch
        {
            ShootingStyle.Portrait when harshLight => _localizer["Advice_Start_Portrait_Harsh"],
            ShootingStyle.Portrait => _localizer["Advice_Start_Portrait_Default"],
            ShootingStyle.Landscape when lowLight => _localizer["Advice_Start_Landscape_LowLight"],
            ShootingStyle.Landscape => _localizer["Advice_Start_Landscape_Default"],
            ShootingStyle.Urban when lowLight => _localizer["Advice_Start_Urban_LowLight"],
            ShootingStyle.Urban => _localizer["Advice_Start_Urban_Default"],
            ShootingStyle.NightSky => _localizer["Advice_Start_NightSky"],
            _ => _localizer["Advice_Start_Landscape_Default"]
        };
    }

    private string GetDeviceOperation(ShootingAdviceContext context)
    {
        var lowLight = IsLowLight(context.Phase) || context.Style == ShootingStyle.NightSky;
        var harshLight = IsHarshLight(context.Phase);

        return context.Camera switch
        {
            CameraType.PhoneBasic when lowLight => _localizer["Advice_Device_PhoneBasic_LowLight"],
            CameraType.PhoneBasic when harshLight => _localizer["Advice_Device_PhoneBasic_Harsh"],
            CameraType.PhoneBasic => _localizer["Advice_Device_PhoneBasic_Default"],
            CameraType.PhonePro when lowLight => _localizer["Advice_Device_PhonePro_LowLight"],
            CameraType.PhonePro when harshLight => _localizer["Advice_Device_PhonePro_Harsh"],
            CameraType.PhonePro => _localizer["Advice_Device_PhonePro_Default"],
            CameraType.MirrorlessAPS when lowLight => _localizer["Advice_Device_APS_LowLight"],
            CameraType.MirrorlessAPS when harshLight => _localizer["Advice_Device_APS_Harsh"],
            CameraType.MirrorlessAPS => _localizer["Advice_Device_APS_Default"],
            CameraType.FullFrame when lowLight => _localizer["Advice_Device_FullFrame_LowLight"],
            CameraType.FullFrame when harshLight => _localizer["Advice_Device_FullFrame_Harsh"],
            CameraType.FullFrame => _localizer["Advice_Device_FullFrame_Default"],
            CameraType.ActionCam when lowLight => _localizer["Advice_Device_ActionCam_LowLight"],
            CameraType.ActionCam when harshLight => _localizer["Advice_Device_ActionCam_Harsh"],
            CameraType.ActionCam => _localizer["Advice_Device_ActionCam_Default"],
            _ => _localizer["Advice_Device_PhonePro_Default"]
        };
    }

    private IEnumerable<string> GetCaptureConditionSettings(ShootingAdviceContext context)
    {
        if (context.SupportMode == CameraSupportMode.Tripod)
        {
            yield return context.Style == ShootingStyle.NightSky
                ? _localizer["Advice_Condition_Tripod_NightSky"]
                : _localizer["Advice_Condition_Tripod_Default"];
        }
        else if (IsLowLight(context.Phase) || context.Style == ShootingStyle.NightSky)
        {
            yield return _localizer["Advice_Condition_Handheld_LowLight"];
        }

        if (context.SubjectMotion == SubjectMotion.Moving)
        {
            yield return context.Camera is CameraType.PhoneBasic or CameraType.PhonePro or CameraType.ActionCam
                ? _localizer["Advice_Condition_Moving_AutoDevice"]
                : context.Style == ShootingStyle.Urban
                ? _localizer["Advice_Condition_Moving_Urban"]
                : _localizer["Advice_Condition_Moving_Default"];
        }
        else if (context.SupportMode == CameraSupportMode.Tripod && context.Style != ShootingStyle.NightSky)
        {
            yield return _localizer["Advice_Condition_Still_Tripod"];
        }
    }

    private AdviceRisk GetPrimaryRisk(ShootingAdviceContext context)
    {
        var lowLight = IsLowLight(context.Phase) || context.Style == ShootingStyle.NightSky;

        if (context.Style == ShootingStyle.NightSky && context.SupportMode == CameraSupportMode.Tripod)
            return AdviceRisk.NightSky;
        if (context.SubjectMotion == SubjectMotion.Moving)
            return AdviceRisk.Blur;
        if (context.Weather?.CloudCover >= 75 || context.Weather?.Visibility is > 0 and < 5000)
            return AdviceRisk.Contrast;
        if (context.SupportMode == CameraSupportMode.Handheld && lowLight)
            return AdviceRisk.Blur;
        if (IsHarshLight(context.Phase) || context.Weather?.CloudCover <= 20)
            return AdviceRisk.Highlight;
        if (lowLight)
            return AdviceRisk.Noise;

        return AdviceRisk.Highlight;
    }

    private string GetRiskWarning(AdviceRisk risk) => risk switch
    {
        AdviceRisk.Blur => _localizer["Advice_Risk_Blur"],
        AdviceRisk.Highlight => _localizer["Advice_Risk_Highlight"],
        AdviceRisk.Noise => _localizer["Advice_Risk_Noise"],
        AdviceRisk.Contrast => _localizer["Advice_Risk_Contrast"],
        AdviceRisk.NightSky => _localizer["Advice_Risk_NightSky"],
        _ => _localizer["Advice_Risk_Highlight"]
    };

    private string GetAdjustmentStep(AdviceRisk risk) => risk switch
    {
        AdviceRisk.Blur => _localizer["Advice_Adjust_Blur"],
        AdviceRisk.Highlight => _localizer["Advice_Adjust_Highlight"],
        AdviceRisk.Noise => _localizer["Advice_Adjust_Noise"],
        AdviceRisk.Contrast => _localizer["Advice_Adjust_Contrast"],
        AdviceRisk.NightSky => _localizer["Advice_Adjust_NightSky"],
        _ => _localizer["Advice_Adjust_Highlight"]
    };

    private string GetWeatherNote(ShootingAdviceContext context, WeatherInfo weather)
    {
        if (weather.Precipitation > 0)
            return _localizer["Advice_Weather_Rain"];
        if (weather.Visibility > 0 && weather.Visibility < 5000)
            return _localizer["Advice_Weather_Fog"];
        if (weather.CloudCover >= 75)
            return _localizer["Advice_Weather_Cloudy"];
        if (weather.CloudCover <= 20)
            return context.Style == ShootingStyle.NightSky ? "" : _localizer["Advice_Weather_Clear"];

        return _localizer["Advice_Weather_Mixed"];
    }

    private string GetExperienceNote(ExperienceLevel experience) => experience switch
    {
        ExperienceLevel.Beginner => _localizer["Advice_Experience_Beginner"],
        ExperienceLevel.Intermediate => _localizer["Advice_Experience_Intermediate"],
        ExperienceLevel.Professional => _localizer["Advice_Experience_Professional"],
        _ => _localizer["Advice_Experience_Beginner"]
    };

    private static bool IsLowLight(LightPhase phase) => phase is
        LightPhase.Night or
        LightPhase.AstronomicalDawn or
        LightPhase.NauticalDawn or
        LightPhase.BlueHour or
        LightPhase.BlueDusk or
        LightPhase.NauticalDusk or
        LightPhase.AstronomicalDusk;

    private static bool IsBlueHour(LightPhase phase) => phase is
        LightPhase.BlueHour or
        LightPhase.BlueDusk;

    private static bool IsHarshLight(LightPhase phase) => phase is
        LightPhase.Morning or
        LightPhase.Midday or
        LightPhase.Afternoon;
}
