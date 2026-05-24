using Luma.Models;
using Microsoft.Extensions.Localization;
using System.Globalization;

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
        var hourlyNote = GetHourlyWindowNote(weather);

        if (!isDryNow)
        {
            var notes = new List<string>();
            if (hourlyNote != null)
                notes.Add(hourlyNote);

            return new FieldWindowRecommendation
            {
                Icon = "🌧️",
                Title = Text("FieldWindow_Rain_Title"),
                Summary = Text("FieldWindow_Rain_Summary"),
                Detail = highRainTomorrow
                    ? Text("FieldWindow_TomorrowRainStillHigh", tomorrowRainChance!.Value)
                    : null,
                Notes = notes,
                Tone = FieldWindowTone.Caution
            };
        }

        if (highCloudCover)
        {
            var notes = new List<string>
            {
                Text("FieldWindow_Overcast_Recipe"),
                Text("FieldWindow_Overcast_Settings")
            };

            if (hourlyNote != null)
                notes.Insert(0, hourlyNote);

            return new FieldWindowRecommendation
            {
                Icon = "☁️",
                Title = Text("FieldWindow_Overcast_Title"),
                Summary = Text("FieldWindow_Overcast_Summary", weather.CloudCover),
                Detail = highRainTomorrow
                    ? Text("FieldWindow_TomorrowRainHigh", tomorrowRainChance!.Value)
                    : Text("FieldWindow_Overcast_Detail"),
                Notes = notes,
                Tone = FieldWindowTone.Good
            };
        }

        if (highRainTomorrow)
        {
            var notes = new List<string>();
            if (hourlyNote != null)
                notes.Add(hourlyNote);

            return new FieldWindowRecommendation
            {
                Icon = "⏰",
                Title = Text("FieldWindow_DryBeforeRain_Title"),
                Summary = Text("FieldWindow_DryBeforeRain_Summary", tomorrowRainChance!.Value),
                Notes = notes,
                Tone = FieldWindowTone.Good
            };
        }

        return null;
    }

    private string? GetHourlyWindowNote(WeatherInfo weather)
    {
        if (weather.HourlyForecast.Count == 0)
            return null;

        var rainRisk = weather.HourlyForecast.FirstOrDefault(point =>
            point.PrecipitationProbability >= 50 || point.Precipitation > 0);

        if (rainRisk != null)
            return Text("FieldWindow_Hourly_RainRisk", rainRisk.Time.ToString("HH:mm", CultureInfo.CurrentCulture));

        var maxProbability = weather.HourlyForecast.Max(point => point.PrecipitationProbability);
        return Text("FieldWindow_Hourly_DryWindow", maxProbability);
    }

    private string Text(string key, params object[] arguments) => _localizer[key, arguments].ToString();
}