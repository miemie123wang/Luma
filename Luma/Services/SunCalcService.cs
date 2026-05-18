using Microsoft.JSInterop;
using Luma.Models;

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