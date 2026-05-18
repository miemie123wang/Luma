using Microsoft.AspNetCore.Components;

namespace Luma.Pages;

public partial class Planner : ComponentBase
{
    [CascadingParameter(Name = "UICulture")] private string UICulture { get; set; } = "";
}