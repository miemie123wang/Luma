using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Luma.Layout;

public partial class MainLayout
{
    private bool _drawerOpen = true;

    private MudTheme _lumaTheme = new MudTheme()
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

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }
}