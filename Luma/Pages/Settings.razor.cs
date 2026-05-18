using Microsoft.AspNetCore.Components;
using Luma.Models;
using Luma.Services;

namespace Luma.Pages;

public partial class Settings : ComponentBase
{
    [Inject] private SettingsService SettingsService { get; set; } = default!;

    protected UserSettings CurrentSettings { get; set; } = new();
    protected bool IsSaved { get; set; } = false;

    protected record OptionItem<T>(string Label, T Value);

    protected readonly OptionItem<CameraType>[] CameraOptions =
    [
        new("📱 手机", CameraType.PhoneBasic),
        new("📱 手机 Pro", CameraType.PhonePro),
        new("📷 APS-C", CameraType.MirrorlessAPS),
        new("🎞️ 全幅", CameraType.FullFrame),
        new("🏄 运动相机", CameraType.ActionCam),
    ];

    protected readonly OptionItem<ShootingStyle>[] StyleOptions =
    [
        new("🏔️ 风景", ShootingStyle.Landscape),
        new("🏙️ 城市", ShootingStyle.Urban),
        new("👤 人像", ShootingStyle.Portrait),
        new("⭐ 星空", ShootingStyle.NightSky),
    ];

    protected readonly OptionItem<TimePreference>[] TimeOptions =
    [
        new("🌅 早鸟", TimePreference.EarlyBird),
        new("🌇 夜猫", TimePreference.NightOwl),
        new("✨ 两者", TimePreference.Both),
    ];

    protected readonly OptionItem<ExperienceLevel>[] ExperienceOptions =
    [
        new("🌱 入门", ExperienceLevel.Beginner),
        new("📷 进阶", ExperienceLevel.Intermediate),
        new("🎯 专业", ExperienceLevel.Professional),
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
        IsSaved = true;
        await Task.Delay(2000);
        IsSaved = false;
        StateHasChanged();
    }
}