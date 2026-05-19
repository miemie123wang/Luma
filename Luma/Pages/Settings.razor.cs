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
    protected bool IsSaving { get; set; } = false;
    protected string? SaveErrorMessage { get; set; }
    private int _saveVersion = 0;

    protected record OptionItem<T>(string LabelKey, T Value);

    protected readonly OptionItem<CameraType>[] CameraOptions =
    [
        new("CameraOption_Phone", CameraType.PhoneBasic),
        new("CameraOption_PhonePro", CameraType.PhonePro),
        new("CameraOption_APS", CameraType.MirrorlessAPS),
        new("CameraOption_FullFrame", CameraType.FullFrame),
        new("CameraOption_ActionCam", CameraType.ActionCam),
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

    protected async Task UpdateSetting<T>(T currentValue, T newValue, Action<T> applyValue)
    {
        if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
            return;

        applyValue(newValue);
        await SaveSettings();
    }

    private async Task SaveSettings()
    {
        var saveVersion = ++_saveVersion;
        IsSaving = true;
        IsSaved = false;
        SaveErrorMessage = null;
        StateHasChanged();

        try
        {
            await SettingsService.SaveAsync(CurrentSettings);
        }
        catch
        {
            if (saveVersion != _saveVersion)
                return;

            IsSaving = false;
            SaveErrorMessage = Localizer["Settings_SaveStatus_Failed"];
            StateHasChanged();
            return;
        }

        if (saveVersion != _saveVersion)
            return;

        IsSaving = false;
        IsSaved = true;
        StateHasChanged();

        await Task.Delay(1500);
        if (saveVersion == _saveVersion)
        {
            IsSaved = false;
            StateHasChanged();
        }
    }
}