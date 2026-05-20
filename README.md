# Luma 🌅
> 旅行摄影师的光线助手

---

## 项目简介

Luma 是一个面向旅行摄影师的光线助手 app，帮助用户在旅行期间判断当前光线阶段、天气条件和下一段适合拍摄的时间窗口。

目标用户是带着相机旅行、不想错过好光线但也不想研究太多的普通摄影爱好者。

### 差异化定位

| 现有工具（工具型） | Luma（助手型） |
|---|---|
| PhotoPills、The Photographer's Ephemeris、GoldenHour.One | 主动结合位置 + 天气 + 光线 |
| 告诉你“某地几点日出” | 告诉你“现在是什么光线，接下来什么时候更值得拍” |

---

## 当前功能

### 实时光线助手
- 自动获取浏览器当前位置
- 使用 SunCalc 计算日出、日落、蓝调时段、黄金时段等太阳时间
- 根据当前时间判断光线阶段，并显示名称、说明、下一阶段和 1-5 星拍摄评级
- 调用 Open-Meteo 显示当前云量、降水、天气、温度、风速和能见度
- 使用 OpenStreetMap Nominatim 反向地理编码显示城市 / 区县名称
- 支持高海拔提示
- 在首页选择当前拍摄类型、拍摄方式（手持 / 三脚架）和主体状态（静态 / 运动），这些选择不写入长期设置

### 本地拍摄建议
- 纯本地规则式逻辑，无需 AI API 或额外付费服务
- 根据当前光线、天气、拍摄类型、器材、经验级别、拍摄方式和主体状态生成建议
- 使用模块化建议结构：可行性提示、第一张测试、先注意的风险、如果不对先调什么、初学者步骤
- 第一张测试会给出更保守的起始参数：相机用户包含 ISO、光圈、快门、曝光补偿；手机用户包含镜头、模式、曝光操作和稳定方式
- 夜晚 / 弱光会先按光线阶段和手持 / 三脚架分流，避免夜晚给出白天参数；手持夜景会优先提示找稳定支撑
- 对手机、手机 Pro、APS-C、全幅和运动相机使用不同的操作语言
- 经验级别会影响操作模式和说明深度：入门偏 A/Av 与保守范围，进阶开始引入 A/Av 或 M 档，专业偏 RAW、手动控制、包围曝光和明确取舍
- 文档按功能分类整理，拍摄建议设计记录在 [docs/advice/design.md](docs/advice/design.md)，AI 审查流程记录在 [docs/advice/audit.md](docs/advice/audit.md)

### Copy AI Prompt
- 首页建议卡片右上角提供 `Copy AI prompt` 按钮
- 一键复制当前时间、光线阶段、地点、天气、器材、经验、拍摄类型、手持 / 三脚架和主体状态
- Prompt 只复制现场上下文，不复制本地硬编码建议，避免外部 AI 被本地规则带偏
- 复制状态会短暂显示并自动消失

### 用户设置
所有设置存储于 `localStorage`，无需账号注册。

- 器材类型：手机、手机 Pro、APS-C、全幅、运动相机
- 经验级别：入门、进阶、专业
- 界面语言：英文、西班牙文、简体中文、繁体中文

### 行程规划
行程规划页面已占位，完整规划逻辑尚未实现。

---

## 后续方向

### 拍摄建议下一步
- 增加镜头焦段：广角 / 标准 / 长焦
- 增加镜头最大光圈：f/1.8 / f/2.8 / f/4 / f/5.6
- 增加 RAW / JPEG 偏好
- 根据距离日出 / 日落的剩余时间进一步调整建议
- 将城市、人像、星空等类型继续拆细，例如街拍、建筑、车流光轨、单人 / 多人 / 儿童人像、月相和光污染

### 行程规划
- 输入地点和日期范围
- 自动分析每天的光线质量和天气状况
- 标出最佳拍摄日和时间窗口
- 卡片式展示，一眼看出哪天值得早起或等待日落

---

## 技术方案

完整文档入口见 [docs/README.md](docs/README.md)。

| 模块 | 技术选型 | 说明 |
|---|---|---|
| 框架 | Blazor WebAssembly / .NET 9 | 静态前端应用，无需自建服务器 |
| UI 组件库 | MudBlazor | 深色主题，Material Design 组件 |
| 天气 + 云量 | Open-Meteo | 免费，无需 API Key |
| 日出日落 + 光线阶段 | 本地 SunCalc + C# 服务 | 浏览器端计算太阳时间，C# 判断当前光线阶段 |
| 定位 | Browser Geolocation API | 通过 JS interop 获取当前位置 |
| 地名 | OpenStreetMap Nominatim | 反向地理编码；失败时降级为坐标显示 |
| 用户数据 | localStorage | 无需账号 |
| 多语言 | 自定义 `IStringLocalizer` + `Translations.cs` | 英文 / 西班牙文 / 简体中文 / 繁体中文 |
| 托管 | GitHub Pages（计划） | 静态部署 |

---

## 视觉风格

- **色调**：深色背景（夜晚 / 黄昏感）+ 暖橙色调（黄金时段）
- **风格参考**：Dark Sky / Slopes，简洁、有质感
- **布局**：移动端优先，卡片式展示当前光线、天气和位置

---

## 项目结构

```
Luma/
├── Components/
│   └── Home/                       # 首页专用 UI 子组件
├── Layout/
│   └── MainLayout.razor          # 主布局、导航和语言切换
├── Localization/
│   ├── InMemoryStringLocalizer.cs # 自定义本地化实现
│   └── Translations.cs            # 多语言文案字典
├── Models/                        # 地理位置、光线阶段、天气、设置和建议模型
├── Pages/
│   ├── Home.razor                 # 实时光线页面 UI
│   ├── Home.razor.cs              # 实时光线页面逻辑
│   ├── Planner.razor              # 行程规划页面 UI（占位）
│   ├── Planner.razor.cs           # 行程规划页面逻辑（占位）
│   ├── Settings.razor             # 设置页面 UI
│   └── Settings.razor.cs          # 设置页面逻辑
├── Services/
│   ├── LightPhaseService.cs       # 光线阶段判断
│   ├── AiPromptBuilder.cs         # Copy AI Prompt 文本生成
│   ├── SettingsService.cs         # localStorage 设置读写
│   ├── ShootingAdviceService.cs   # 本地规则式拍摄建议
│   ├── SunCalcService.cs          # SunCalc / 定位 / 地名 JS interop
│   └── WeatherService.cs          # Open-Meteo 天气数据
├── wwwroot/
│   ├── css/app.css                # 全局样式
│   ├── js/blazorCulture.js        # 语言持久化
│   ├── js/luma.js                 # JS interop
│   ├── lib/suncalc/               # 本地 SunCalc 依赖
│   └── index.html                 # 入口 HTML
├── Program.cs                     # 应用入口和服务注册
├── Luma.csproj                    # 项目文件
└── _Imports.razor                 # 全局 Razor 引用

tools/
├── Luma.AdviceAudit/              # 拍摄建议高风险场景输出生成器
└── Luma.LocalizationCheck/        # 本地化 key 和占位符校验工具
```

根目录的 `global.json` 将 SDK 锁定到 .NET 9，避免本机默认 .NET 10 SDK 造成构建差异。

---

## 本地开发

### 环境要求
- .NET 9 SDK
- VS Code + C# Dev Kit 插件

### 启动项目
```powershell
git clone https://github.com/miemie123wang/Luma.git
cd Luma/Luma
dotnet run
```

默认开发地址为 `http://localhost:5284`。

### 构建检查
```powershell
cd Luma/Luma
dotnet build
```

### 本地化校验
```powershell
cd Luma
dotnet run --project tools/Luma.LocalizationCheck/Luma.LocalizationCheck.csproj
```

此检查会确认所有语言拥有同一组翻译 key，并校验 `{0}`、`{1}` 等格式化占位符是否一致。

### 拍摄建议 audit 输出
```powershell
cd Luma
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj
```

输出较长时，可以直接写入文件：

```powershell
dotnet run --project .\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj -- --out .\docs\advice\generated\high-risk-output.md
```

外部 review 结果也建议保存到同一目录，例如 `docs/advice/generated/high-risk-review.md`，方便后续直接读取和整理。

`docs/advice/generated/` 是本地审阅产物目录，不需要提交。

如果当前终端已经在 `Luma/Luma` app 目录内，请使用：

```powershell
dotnet run --project ..\tools\Luma.AdviceAudit\Luma.AdviceAudit.csproj
```

---

## MVP 范围（第一版）

- [x] 基础框架搭建（Blazor WASM + MudBlazor）
- [x] 深色主题 + 暖橙色视觉风格
- [x] 页面结构（实时光线 / 行程规划 / 设置）
- [x] SunCalc JS interop 接入
- [x] 实时光线阶段计算
- [x] 获取用户地理位置
- [x] 用户设置 + localStorage
- [x] Open-Meteo 天气接入
- [x] 多语言支持（英文 / 西班牙文 / 简体中文 / 繁体中文）
- [x] 本地规则式拍摄建议逻辑
- [x] Copy AI Prompt
- [ ] 行程规划功能
- [ ] GitHub Pages 部署

**暂不做：** 拍摄点推荐、社区功能（后期 UGC）

---

## 未来扩展路线图

- [ ] 离线地图
- [ ] 更多拍摄类型（食物、建筑、野生动物、视频）
- [ ] 更细的器材上下文（焦段、最大光圈、防抖、RAW / JPEG）
- [ ] 拍摄点收藏和笔记
- [ ] 社区上传拍摄点（UGC）
- [ ] MAUI 版本（iOS / Android）

---

*最后更新：2026-05-19*
