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
        Contrast
    }

    private readonly IStringLocalizer<SharedResource> _localizer;

    public ShootingAdviceService(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public ShootingAdvice GetAdvice(ShootingAdviceContext context)
    {
        var primaryRisk = GetPrimaryRisk(context);
        var startingPoints = new List<string>
        {
            GetSafeStartingPoint(context),
            GetDeviceOperation(context)
        };

        var riskWarnings = new List<string> { GetRiskWarning(primaryRisk) };
        riskWarnings.AddRange(GetCaptureConditionSettings(context));

        if (context.Weather != null)
        {
            var weatherNote = GetWeatherNote(context.Weather);
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
            StartingPoints = startingPoints,
            RiskWarnings = riskWarnings,
            AdjustmentSteps = adjustmentSteps
        };
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
            yield return context.Style == ShootingStyle.Urban
                ? _localizer["Advice_Condition_Moving_Urban"]
                : _localizer["Advice_Condition_Moving_Default"];
        }
        else if (context.SupportMode == CameraSupportMode.Tripod)
        {
            yield return _localizer["Advice_Condition_Still_Tripod"];
        }
    }

    private AdviceRisk GetPrimaryRisk(ShootingAdviceContext context)
    {
        var lowLight = IsLowLight(context.Phase) || context.Style == ShootingStyle.NightSky;

        if (context.SubjectMotion == SubjectMotion.Moving || context.SupportMode == CameraSupportMode.Handheld && lowLight)
            return AdviceRisk.Blur;
        if (IsHarshLight(context.Phase) || context.Weather?.CloudCover <= 20)
            return AdviceRisk.Highlight;
        if (lowLight)
            return AdviceRisk.Noise;
        if (context.Weather?.CloudCover >= 75 || context.Weather?.Visibility is > 0 and < 5000)
            return AdviceRisk.Contrast;

        return AdviceRisk.Highlight;
    }

    private string GetRiskWarning(AdviceRisk risk) => risk switch
    {
        AdviceRisk.Blur => _localizer["Advice_Risk_Blur"],
        AdviceRisk.Highlight => _localizer["Advice_Risk_Highlight"],
        AdviceRisk.Noise => _localizer["Advice_Risk_Noise"],
        AdviceRisk.Contrast => _localizer["Advice_Risk_Contrast"],
        _ => _localizer["Advice_Risk_Highlight"]
    };

    private string GetAdjustmentStep(AdviceRisk risk) => risk switch
    {
        AdviceRisk.Blur => _localizer["Advice_Adjust_Blur"],
        AdviceRisk.Highlight => _localizer["Advice_Adjust_Highlight"],
        AdviceRisk.Noise => _localizer["Advice_Adjust_Noise"],
        AdviceRisk.Contrast => _localizer["Advice_Adjust_Contrast"],
        _ => _localizer["Advice_Adjust_Highlight"]
    };

    private string GetWeatherNote(WeatherInfo weather)
    {
        if (weather.Precipitation > 0)
            return _localizer["Advice_Weather_Rain"];
        if (weather.Visibility > 0 && weather.Visibility < 5000)
            return _localizer["Advice_Weather_Fog"];
        if (weather.CloudCover >= 75)
            return _localizer["Advice_Weather_Cloudy"];
        if (weather.CloudCover <= 20)
            return _localizer["Advice_Weather_Clear"];

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

    private static bool IsHarshLight(LightPhase phase) => phase is
        LightPhase.Morning or
        LightPhase.Midday or
        LightPhase.Afternoon;
}
