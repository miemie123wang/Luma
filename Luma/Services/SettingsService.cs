using System.Text.Json;
using Luma.Models;
using Microsoft.JSInterop;

namespace Luma.Services;

public class SettingsService
{
    private readonly IJSRuntime _js;
    private const string Key = "luma_settings";

    public SettingsService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<UserSettings> LoadAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", Key);
            if (!string.IsNullOrEmpty(json))
                return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
        }
        catch { }
        return new UserSettings();
    }

    public async Task SaveAsync(UserSettings settings)
    {
        var json = JsonSerializer.Serialize(settings);
        await _js.InvokeVoidAsync("localStorage.setItem", Key, json);
    }
}