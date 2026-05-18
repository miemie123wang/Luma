using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Luma;
using Luma.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped<SunCalcService>();
builder.Services.AddScoped<LightPhaseService>();
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddScoped<SettingsService>();

builder.Services.AddLocalization();
builder.Services.AddSingleton<Microsoft.Extensions.Localization.IStringLocalizer<SharedResource>,
    Luma.Localization.InMemoryStringLocalizer>();

var host = builder.Build();

// Try to read persisted language from localStorage (settings) and set culture before start
try
{
	var js = host.Services.GetRequiredService<IJSRuntime>();
	var settingsJson = await js.InvokeAsync<string>("localStorage.getItem", "luma_settings");
	if (!string.IsNullOrEmpty(settingsJson))
	{
		using var doc = JsonDocument.Parse(settingsJson);
		if (doc.RootElement.TryGetProperty("Language", out var langElem))
		{
			var lang = langElem.GetString();
			if (!string.IsNullOrEmpty(lang))
			{
				var culture = new CultureInfo(lang);
				CultureInfo.DefaultThreadCurrentCulture = culture;
				CultureInfo.DefaultThreadCurrentUICulture = culture;
			}
		}
	}
}
catch { }

await host.RunAsync();