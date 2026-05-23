using Luma.Models;
using Microsoft.Extensions.Localization;

namespace Luma.Services;

public class FieldWindowService
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public FieldWindowService(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public FieldWindowRecommendation? GetRecommendation(LightPhaseInfo? phase, WeatherInfo? weather)
    {
        if (phase == null || weather == null)
            return null;

        var tomorrowRainChance = weather.TomorrowPrecipitationProbability;
        var isDryNow = weather.Precipitation <= 0;
        var highCloudCover = weather.CloudCover >= 85;
        var highRainTomorrow = tomorrowRainChance >= 60;

        if (!isDryNow)
        {
            return new FieldWindowRecommendation
            {
                Icon = "🌧️",
                Title = Text("FieldWindow_Rain_Title"),
                Summary = Text("FieldWindow_Rain_Summary"),
                Detail = highRainTomorrow
                    ? Text("FieldWindow_TomorrowRainStillHigh", tomorrowRainChance!.Value)
                    : null,
                Tone = FieldWindowTone.Caution
            };
        }

        if (highCloudCover)
        {
            return new FieldWindowRecommendation
            {
                Icon = "☁️",
                Title = Text("FieldWindow_Overcast_Title"),
                Summary = Text("FieldWindow_Overcast_Summary", weather.CloudCover),
                Detail = highRainTomorrow
                    ? Text("FieldWindow_TomorrowRainHigh", tomorrowRainChance!.Value)
                    : Text("FieldWindow_Overcast_Detail"),
                Tone = FieldWindowTone.Good
            };
        }

        if (highRainTomorrow)
        {
            return new FieldWindowRecommendation
            {
                Icon = "⏰",
                Title = Text("FieldWindow_DryBeforeRain_Title"),
                Summary = Text("FieldWindow_DryBeforeRain_Summary", tomorrowRainChance!.Value),
                Tone = FieldWindowTone.Good
            };
        }

        return null;
    }

    private string Text(string key, params object[] arguments) => _localizer[key, arguments].ToString();
}