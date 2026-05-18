using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Luma.Models;
using Luma.Services;

namespace Luma.Pages;

public partial class Settings : ComponentBase
{
    [Inject] private SettingsService SettingsService { get; set; } = default!;
    [Inject] private IStringLocalizer<SharedResource> Localizer { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    protected UserSettings CurrentSettings { get; set; } = new();
    protected bool IsSaved { get; set; } = false;

    protected record OptionItem<T>(string LabelKey, T Value);

    protected readonly OptionItem<CameraType>[] CameraOptions =
    [
        new("CameraOption_Phone", CameraType.PhoneBasic),
        new("CameraOption_PhonePro", CameraType.PhonePro),
        new("CameraOption_APS", CameraType.MirrorlessAPS),
        new("CameraOption_FullFrame", CameraType.FullFrame),
        new("CameraOption_ActionCam", CameraType.ActionCam),
    ];

    protected readonly OptionItem<ShootingStyle>[] StyleOptions =
    [
        new("Style_Landscape", ShootingStyle.Landscape),
        new("Style_Urban", ShootingStyle.Urban),
        new("Style_Portrait", ShootingStyle.Portrait),
        new("Style_NightSky", ShootingStyle.NightSky),
    ];

    protected readonly OptionItem<TimePreference>[] TimeOptions =
    [
        new("Time_EarlyBird", TimePreference.EarlyBird),
        new("Time_NightOwl", TimePreference.NightOwl),
        new("Time_Both", TimePreference.Both),
    ];

    protected readonly OptionItem<ExperienceLevel>[] ExperienceOptions =
    [
        new("Experience_Beginner", ExperienceLevel.Beginner),
        new("Experience_Intermediate", ExperienceLevel.Intermediate),
        new("Experience_Professional", ExperienceLevel.Professional),
    ];

    protected readonly OptionItem<string>[] LanguageOptions =
    [
        new("Language_en", "en"),
        new("Language_zh-Hans", "zh-Hans"),
        new("Language_zh-Hant", "zh-Hant"),
    ];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        CurrentSettings = await SettingsService.LoadAsync();
        StateHasChanged();
    }

    protected async Task SaveSettings()
    {
        await SettingsService.SaveAsync(CurrentSettings);
        // persist a quick culture key and reload so localization takes effect immediately
        try
        {
            await JS.InvokeVoidAsync("blazorCulture.set", CurrentSettings.Language, true);
        }
        catch { }
        IsSaved = true;
        await Task.Delay(2000);
        IsSaved = false;
        StateHasChanged();
    }
}