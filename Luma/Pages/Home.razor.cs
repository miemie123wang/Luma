using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Luma.Services;
using Luma.Models;

namespace Luma.Pages;

public partial class Home : ComponentBase
{
    [CascadingParameter(Name = "UICulture")] private string UICulture { get; set; } = "";
    [Inject] private SunCalcService SunCalcService { get; set; } = default!;
    [Inject] private LightPhaseService LightPhaseService { get; set; } = default!;
    [Inject] private WeatherService WeatherService { get; set; } = default!;
    [Inject] private SettingsService SettingsService { get; set; } = default!;
    [Inject] private ShootingAdviceService ShootingAdviceService { get; set; } = default!;
    

    protected LightPhaseInfo? CurrentPhase { get; set; }
    protected GeoLocation? Location { get; set; }
    protected string LocationName { get; set; } = "";
    protected WeatherInfo? Weather { get; set; }
    protected UserSettings CurrentSettings { get; set; } = new();
    protected ShootingAdvice? Advice { get; set; }
    protected bool IsLoading { get; set; } = true;
    protected ShootingStyle SelectedShootingStyle { get; set; } = ShootingStyle.Landscape;
    protected CameraSupportMode SelectedSupportMode { get; set; } = CameraSupportMode.Handheld;
    protected SubjectMotion SelectedSubjectMotion { get; set; } = SubjectMotion.Still;
    protected string? LocationWarningMessage { get; set; }
    protected string? WeatherWarningMessage { get; set; }

    protected record OptionItem<T>(string LabelKey, T Value);

    protected readonly OptionItem<ShootingStyle>[] StyleOptions =
    [
        new("Style_Landscape", ShootingStyle.Landscape),
        new("Style_Urban", ShootingStyle.Urban),
        new("Style_Portrait", ShootingStyle.Portrait),
        new("Style_NightSky", ShootingStyle.NightSky),
    ];

    protected readonly OptionItem<CameraSupportMode>[] SupportModeOptions =
    [
        new("Advice_Support_Handheld", CameraSupportMode.Handheld),
        new("Advice_Support_Tripod", CameraSupportMode.Tripod),
    ];

    protected readonly OptionItem<SubjectMotion>[] SubjectMotionOptions =
    [
        new("Advice_Subject_Still", SubjectMotion.Still),
        new("Advice_Subject_Moving", SubjectMotion.Moving),
    ];

    protected string FormattedVisibility => Weather == null ? "" :
        Weather.Visibility >= 1000
            ? $"{(Weather.Visibility / 1000).ToString("F1", System.Globalization.CultureInfo.InvariantCulture)} km"
            : $"{Weather.Visibility.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)} m";
    protected string? ErrorMessage { get; set; }

    protected void SelectShootingStyle(ShootingStyle style)
    {
        SelectedShootingStyle = style;
        UpdateShootingAdvice();
    }

    protected void SelectSupportMode(CameraSupportMode supportMode)
    {
        SelectedSupportMode = supportMode;
        UpdateShootingAdvice();
    }

    protected void SelectSubjectMotion(SubjectMotion subjectMotion)
    {
        SelectedSubjectMotion = subjectMotion;
        UpdateShootingAdvice();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        CurrentSettings = await SettingsService.LoadAsync();
        await LoadCurrentLightAsync();
        IsLoading = false;
        StateHasChanged();
    }

    private async Task LoadCurrentLightAsync()
    {
        try
        {
            Location = await SunCalcService.GetCurrentPositionAsync();
        }
        catch (JSException ex)
        {
            ErrorMessage = GetLocationErrorMessage(ex.Message);
            return;
        }
        catch
        {
            ErrorMessage = Localizer["Error_LocationUnavailable"];
            return;
        }

        if (Location == null)
        {
            ErrorMessage = Localizer["Error_LocationUnavailable"];
            return;
        }

        try
        {
            var sunTimes = await SunCalcService.GetSunTimesAsync(Location.Lat, Location.Lng);
            if (sunTimes == null)
            {
                ErrorMessage = Localizer["Error_SunCalcUnavailable"];
                return;
            }

            CurrentPhase = LightPhaseService.GetCurrentPhase(sunTimes);
            UpdateShootingAdvice();
        }
        catch
        {
            ErrorMessage = Localizer["Error_SunCalcUnavailable"];
            return;
        }

        try
        {
            LocationName = await SunCalcService.GetLocationNameAsync(Location.Lat, Location.Lng);
            if (string.IsNullOrWhiteSpace(LocationName))
            {
                LocationName = Localizer["Location_Unknown"];
                LocationWarningMessage = Localizer["Warning_LocationNameUnavailable"];
            }
        }
        catch
        {
            LocationName = Localizer["Location_Unknown"];
            LocationWarningMessage = Localizer["Warning_LocationNameUnavailable"];
        }

        try
        {
            Weather = await WeatherService.GetCurrentWeatherAsync(Location.Lat, Location.Lng);
            if (Weather == null)
                WeatherWarningMessage = Localizer["Warning_WeatherUnavailable"];
            UpdateShootingAdvice();
        }
        catch
        {
            WeatherWarningMessage = Localizer["Warning_WeatherUnavailable"];
            UpdateShootingAdvice();
        }
    }

    private void UpdateShootingAdvice()
    {
        if (CurrentPhase == null)
            return;

        Advice = ShootingAdviceService.GetAdvice(new ShootingAdviceContext
        {
            Phase = CurrentPhase.Phase,
            Weather = Weather,
            Style = SelectedShootingStyle,
            Camera = CurrentSettings.Camera,
            Experience = CurrentSettings.Experience,
            SupportMode = SelectedSupportMode,
            SubjectMotion = SelectedSubjectMotion
        });
    }

    private LocalizedString GetLocationErrorMessage(string errorMessage)
    {
        if (errorMessage.Contains("LUMA_GEO_PERMISSION_DENIED", StringComparison.OrdinalIgnoreCase))
            return Localizer["Error_LocationDenied"];
        if (errorMessage.Contains("LUMA_GEO_UNSUPPORTED", StringComparison.OrdinalIgnoreCase))
            return Localizer["Error_GeolocationUnsupported"];
        if (errorMessage.Contains("LUMA_GEO_TIMEOUT", StringComparison.OrdinalIgnoreCase))
            return Localizer["Error_LocationTimeout"];

        return Localizer["Error_LocationUnavailable"];
    }
}