using Microsoft.AspNetCore.Components;

namespace Luma.Pages;

public partial class Home : ComponentBase
{
    protected string CurrentPhaseIcon { get; set; } = "🌇";
    protected string CurrentPhaseName { get; set; } = "黄金时段";
    protected string CurrentPhaseDescription { get; set; } = "现在是拍摄的最佳时机";
    protected string NextPhase { get; set; } = "日落后蓝调时段 · 约1小时后";

    protected override void OnInitialized()
    {
        // 之后这里会根据真实时间计算光线阶段
    }
}