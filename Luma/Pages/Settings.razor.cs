using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Luma.Models;
using Luma.Services;

namespace Luma.Pages;

public partial class Settings : ComponentBase
{
    [CascadingParameter(Name = "UICulture")] private string UICulture { get; set; } = "";
    [Inject] private SettingsService SettingsService { get; set; } = default!;
    [Inject] private IStringLocalizer<SharedResource> Localizer { get; set; } = default!;

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

    protected bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        CurrentSettings = await SettingsService.LoadAsync();
        IsLoading = false;
    }

    protected async Task SaveSettings()
    {
        await SettingsService.SaveAsync(CurrentSettings);
        IsSaved = true;
        await Task.Delay(2000);
        IsSaved = false;
        StateHasChanged();
    }
}