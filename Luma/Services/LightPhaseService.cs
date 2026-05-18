using Luma.Models;

namespace Luma.Services;

public class LightPhaseService
{
    public LightPhaseInfo GetCurrentPhase(SunTimes times)
    {
        var now = string.IsNullOrEmpty(times.NowUtc)
            ? DateTime.UtcNow
            : DateTime.Parse(times.NowUtc, null, System.Globalization.DateTimeStyles.RoundtripKind);

        var nightEnd = Parse(times.NightEnd);
        var nauticalDawn = Parse(times.NauticalDawn);
        var dawn = Parse(times.Dawn);
        var sunrise = Parse(times.Sunrise);
        var sunriseEnd = Parse(times.SunriseEnd);
        var goldenHourEnd = Parse(times.GoldenHourEnd);
        var solarNoon = Parse(times.SolarNoon);
        var goldenHour = Parse(times.GoldenHour);
        var sunsetStart = Parse(times.SunsetStart);
        var sunset = Parse(times.Sunset);
        var dusk = Parse(times.Dusk);
        var nauticalDusk = Parse(times.NauticalDusk);
        var night = Parse(times.Night);

        if (now >= nightEnd && now < nauticalDawn)
            return MakePhase(LightPhase.AstronomicalDawn, "🌌", "天文晨光", "天空开始有极微弱光线", "蓝调晨曦即将开始", 3);

        if (now >= nauticalDawn && now < dawn)
            return MakePhase(LightPhase.BlueHour, "🌃", "蓝调晨曦", "柔和蓝紫光线，适合城市和风景", "日出即将开始", 5);

        if (now >= dawn && now < sunrise)
            return MakePhase(LightPhase.Sunrise, "🌄", "日出前", "暖色光线从地平线升起", "黄金时段即将开始", 5);

        if (now >= sunrise && now < sunriseEnd)
            return MakePhase(LightPhase.GoldenHourMorning, "🌅", "黄金时段", "最佳拍摄时机，光线柔和温暖", "黄金时段即将结束", 5);

        if (now >= sunriseEnd && now < goldenHourEnd)
            return MakePhase(LightPhase.GoldenHourMorning, "🌅", "黄金时段尾声", "光线依然柔和，把握剩余时间", "光线将变得较硬", 4);

        if (now >= goldenHourEnd && now < solarNoon)
            return MakePhase(LightPhase.Morning, "☀️", "上午", "光线较硬，适合高对比度拍摄", "正午即将到来", 2);

        if (now >= solarNoon && now < goldenHour)
            return MakePhase(LightPhase.Afternoon, "🌤️", "下午", "光线仍较硬，等待黄金时段", "傍晚黄金时段即将开始", 2);

        if (now >= goldenHour && now < sunsetStart)
            return MakePhase(LightPhase.GoldenHourEvening, "🌇", "黄金时段", "最佳拍摄时机，光线柔和温暖", "日落即将开始", 5);

        if (now >= sunsetStart && now < sunset)
            return MakePhase(LightPhase.Sunset, "🌆", "日落时分", "暖色光线，适合剪影和风景", "蓝调时段即将开始", 5);

        if (now >= sunset && now < dusk)
            return MakePhase(LightPhase.BlueDusk, "🌃", "蓝调黄昏", "柔和蓝紫光线，城市灯光开始亮起", "天色将完全变暗", 5);

        if (now >= dusk && now < nauticalDusk)
            return MakePhase(LightPhase.NauticalDusk, "🌙", "暮色", "天空仍有余光，适合长曝光", "夜晚即将到来", 3);

        if (now >= nauticalDusk && now < night)
            return MakePhase(LightPhase.AstronomicalDusk, "🌙", "天文暮光", "天空接近全黑，星空开始显现", "完全夜晚即将到来", 3);

        return MakePhase(LightPhase.Night, "⭐", "夜晚", "适合星空和长曝光夜景拍摄", "天文晨光将在黎明前出现", 3);
    }

    private static DateTime Parse(string iso) => DateTime.Parse(iso, null, System.Globalization.DateTimeStyles.RoundtripKind);

    private static LightPhaseInfo MakePhase(LightPhase phase, string icon, string name, string desc, string next, int rating) => new()
    {
        Phase = phase,
        Icon = icon,
        Name = name,
        Description = desc,
        NextPhase = next,
        Rating = rating
    };
}