using Microsoft.JSInterop;

namespace Luma.Services;

public class SunCalcService
{
    private readonly IJSRuntime _js;

    public SunCalcService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<SunTimes?> GetSunTimesAsync(double lat, double lng)
    {
        return await _js.InvokeAsync<SunTimes>("lumaJS.getSunTimes", lat, lng);
    }

    public async Task<GeoLocation?> GetCurrentPositionAsync()
    {
        return await _js.InvokeAsync<GeoLocation>("lumaJS.getCurrentPosition");
    }

    public async Task<string> GetLocationNameAsync(double lat, double lng)
    {
        return await _js.InvokeAsync<string>("lumaJS.getLocationName", lat, lng);
    }
}

public class SunTimes
{
    public string Sunrise { get; set; } = "";
    public string SunriseEnd { get; set; } = "";
    public string GoldenHourEnd { get; set; } = "";
    public string SolarNoon { get; set; } = "";
    public string GoldenHour { get; set; } = "";
    public string SunsetStart { get; set; } = "";
    public string Sunset { get; set; } = "";
    public string Dusk { get; set; } = "";
    public string NauticalDusk { get; set; } = "";
    public string Night { get; set; } = "";
    public string Nadir { get; set; } = "";
    public string NightEnd { get; set; } = "";
    public string NauticalDawn { get; set; } = "";
    public string Dawn { get; set; } = "";
    public string NowUtc { get; set; } = "";
}

public class GeoLocation
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}