using Luma.Models;

namespace Luma.Services;

public class LightPhaseService
{
    public LightPhaseInfo GetCurrentPhase(SunTimes times)
    {
        var now = string.IsNullOrEmpty(times.NowUtc)
            ? DateTime.UtcNow
            : DateTime.Parse(times.NowUtc, null, System.Globalization.DateTimeStyles.RoundtripKind);

        var nightEnd      = Parse(times.NightEnd);
        var nauticalDawn  = Parse(times.NauticalDawn);
        var dawn          = Parse(times.Dawn);
        var sunrise       = Parse(times.Sunrise);
        var sunriseEnd    = Parse(times.SunriseEnd);
        var goldenHourEnd = Parse(times.GoldenHourEnd);
        var solarNoon     = Parse(times.SolarNoon);
        var goldenHour    = Parse(times.GoldenHour);
        var sunsetStart   = Parse(times.SunsetStart);
        var sunset        = Parse(times.Sunset);
        var dusk          = Parse(times.Dusk);
        var nauticalDusk  = Parse(times.NauticalDusk);
        var night         = Parse(times.Night);

        if (now >= nightEnd && now < nauticalDawn)
            return MakePhase(LightPhase.AstronomicalDawn, "🌌", "Phase_AstronomicalDawn", 3);
        if (now >= nauticalDawn && now < dawn)
            return MakePhase(LightPhase.BlueHour, "🌃", "Phase_BlueHourMorning", 5);
        if (now >= dawn && now < sunrise)
            return MakePhase(LightPhase.Sunrise, "🌄", "Phase_SunrisePre", 5);
        if (now >= sunrise && now < sunriseEnd)
            return MakePhase(LightPhase.GoldenHourMorning, "🌅", "Phase_GoldenHourMorning", 5);
        if (now >= sunriseEnd && now < goldenHourEnd)
            return MakePhase(LightPhase.GoldenHourMorning, "🌅", "Phase_GoldenHourMorningTail", 4);
        if (now >= goldenHourEnd && now < solarNoon)
            return MakePhase(LightPhase.Morning, "☀️", "Phase_Morning", 2);
        if (now >= solarNoon && now < goldenHour)
            return MakePhase(LightPhase.Afternoon, "🌤️", "Phase_Afternoon", 2);
        if (now >= goldenHour && now < sunsetStart)
            return MakePhase(LightPhase.GoldenHourEvening, "🌇", "Phase_GoldenHourEvening", 5);
        if (now >= sunsetStart && now < sunset)
            return MakePhase(LightPhase.Sunset, "🌆", "Phase_Sunset", 5);
        if (now >= sunset && now < dusk)
            return MakePhase(LightPhase.BlueDusk, "🌃", "Phase_BlueDusk", 5);
        if (now >= dusk && now < nauticalDusk)
            return MakePhase(LightPhase.NauticalDusk, "🌙", "Phase_NauticalDusk", 3);
        if (now >= nauticalDusk && now < night)
            return MakePhase(LightPhase.AstronomicalDusk, "🌙", "Phase_AstronomicalDusk", 3);

        return MakePhase(LightPhase.Night, "⭐", "Phase_Night", 3);
    }

    private static DateTime Parse(string iso) =>
        DateTime.Parse(iso, null, System.Globalization.DateTimeStyles.RoundtripKind);

    private static LightPhaseInfo MakePhase(LightPhase phase, string icon, string keyPrefix, int rating) => new()
    {
        Phase       = phase,
        Icon        = icon,
        Name        = $"{keyPrefix}_Name",
        Description = $"{keyPrefix}_Desc",
        NextPhase   = $"{keyPrefix}_Next",
        Rating      = rating
    };
}
