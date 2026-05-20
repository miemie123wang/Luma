using System.Globalization;
using Luma.Models;
using Microsoft.Extensions.Localization;

namespace Luma.Services;

public class AiPromptBuilder
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AiPromptBuilder(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public string Build(AiPromptContext context)
    {
        var lines = new List<string>
        {
            _localizer["Advice_Prompt_Intro"],
            "",
            $"{_localizer["Advice_Prompt_Time"]}: {context.CurrentTime.ToString("yyyy-MM-dd HH:mm zzz", CultureInfo.CurrentCulture)}",
            $"{_localizer["Advice_Prompt_Phase"]}: {_localizer[context.Phase.Name]} - {_localizer[context.Phase.Description]}",
            $"{_localizer["Advice_Prompt_Location"]}: {GetPromptLocation(context)}",
            $"{_localizer["Advice_Prompt_Weather"]}: {GetPromptWeather(context)}",
            $"{_localizer["Camera_Label"]}: {_localizer[$"CameraOption_{GetCameraKey(context.Settings.Camera)}"]}",
            $"{_localizer["Experience_Label"]}: {_localizer[$"Experience_{GetExperienceKey(context.Settings.Experience)}"]}",
            $"{_localizer["Style_Label"]}: {_localizer[$"Style_{GetStyleKey(context.ShootingStyle)}"]}",
            $"{_localizer["Advice_Support_Label"]}: {_localizer[GetSupportLabelKey(context.SupportMode)]}",
            $"{_localizer["Advice_Subject_Label"]}: {_localizer[GetSubjectLabelKey(context.SubjectMotion)]}"
        };

        return string.Join(Environment.NewLine, lines);
    }

    private string GetPromptLocation(AiPromptContext context)
    {
        if (context.Location == null)
            return _localizer["Location_Unknown"];

        return $"{context.LocationName} ({context.Location.Lat.ToString("F4", CultureInfo.InvariantCulture)}, {context.Location.Lng.ToString("F4", CultureInfo.InvariantCulture)})";
    }

    private string GetPromptWeather(AiPromptContext context)
    {
        if (context.Weather == null)
            return context.WeatherWarningMessage ?? _localizer["Warning_WeatherUnavailable"];

        return _localizer["Weather_Summary", context.Weather.Icon, _localizer[context.Weather.Description], context.Weather.CloudCover];
    }

    private static string GetCameraKey(CameraType camera) => camera switch
    {
        CameraType.PhoneBasic => "Phone",
        CameraType.PhonePro => "PhonePro",
        CameraType.MirrorlessAPS => "APS",
        CameraType.FullFrame => "FullFrame",
        CameraType.ActionCam => "ActionCam",
        _ => "PhonePro"
    };

    private static string GetExperienceKey(ExperienceLevel experience) => experience switch
    {
        ExperienceLevel.Beginner => "Beginner",
        ExperienceLevel.Intermediate => "Intermediate",
        ExperienceLevel.Professional => "Professional",
        _ => "Beginner"
    };

    private static string GetStyleKey(ShootingStyle style) => style switch
    {
        ShootingStyle.Landscape => "Landscape",
        ShootingStyle.Urban => "Urban",
        ShootingStyle.Portrait => "Portrait",
        ShootingStyle.NightSky => "NightSky",
        _ => "Landscape"
    };

    private static string GetSupportLabelKey(CameraSupportMode supportMode) => supportMode switch
    {
        CameraSupportMode.Handheld => "Advice_Support_Handheld",
        CameraSupportMode.Tripod => "Advice_Support_Tripod",
        _ => "Advice_Support_Handheld"
    };

    private static string GetSubjectLabelKey(SubjectMotion subjectMotion) => subjectMotion switch
    {
        SubjectMotion.Still => "Advice_Subject_Still",
        SubjectMotion.Moving => "Advice_Subject_Moving",
        _ => "Advice_Subject_Still"
    };
}

public record AiPromptContext(
    DateTimeOffset CurrentTime,
    LightPhaseInfo Phase,
    GeoLocation? Location,
    string LocationName,
    WeatherInfo? Weather,
    string? WeatherWarningMessage,
    UserSettings Settings,
    ShootingStyle ShootingStyle,
    CameraSupportMode SupportMode,
    SubjectMotion SubjectMotion);