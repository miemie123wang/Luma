using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Luma.Services;
using Luma.Models;

namespace Luma.Pages;

public partial class Home : ComponentBase
{
    [Inject] private SunCalcService SunCalcService { get; set; } = default!;
    [Inject] private LightPhaseService LightPhaseService { get; set; } = default!;
    [Inject] private WeatherService WeatherService { get; set; } = default!;
    

    protected LightPhaseInfo? CurrentPhase { get; set; }
    protected GeoLocation? Location { get; set; }
    protected string LocationName { get; set; } = "";
    protected WeatherInfo? Weather { get; set; }
    protected bool IsLoading { get; set; } = true;
    protected string? ErrorMessage { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        try
        {
            Location = await SunCalcService.GetCurrentPositionAsync();
            if (Location != null)
            {
                var sunTimes = await SunCalcService.GetSunTimesAsync(Location.Lat, Location.Lng);
                if (sunTimes != null)
                    CurrentPhase = LightPhaseService.GetCurrentPhase(sunTimes);

                LocationName = await SunCalcService.GetLocationNameAsync(Location.Lat, Location.Lng);
                Weather = await WeatherService.GetCurrentWeatherAsync(Location.Lat, Location.Lng);
            }
        }
        catch (Exception)
        {
            ErrorMessage = Localizer["Error_LocationDenied"];
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
}