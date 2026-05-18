using MudBlazor;

namespace Luma.Layout;

public partial class MainLayout
{
    private bool _drawerOpen = true;

    private readonly MudTheme _lumaTheme = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#f5a623",
            Background = "#0d0d0d",
            Surface = "#1a1a1a",
            TextPrimary = "#f0f0f0",
        },
        PaletteDark = new PaletteDark()
        {
            Primary = "#f5a623",
            Background = "#0d0d0d",
            Surface = "#1a1a1a",
            TextPrimary = "#f0f0f0",
            AppbarBackground = "#1a1a1a",
        }
    };

    private string CurrentLanguageLabel => CurrentLanguage switch
    {
        "es"      => "🇪🇸 ES",
        "zh-Hans" => "🇨🇳 简",
        "zh-Hant" => "🇹🇼 繁",
        _         => "🇺🇸 EN"
    };

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }
}