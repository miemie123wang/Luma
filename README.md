# Luma 🌅
> 旅行摄影师的光线助手

---

## 项目简介

Luma 是一个面向旅行摄影师的光线助手 app，帮助用户在旅行期间找到最佳拍摄时间和光线条件。

目标用户是带着相机旅行、不想错过好光线但也不想研究太多的普通摄影爱好者。

### 差异化定位

| 现有工具（工具型） | Luma（助手型） |
|---|---|
| PhotoPills、The Photographer's Ephemeris、GoldenHour.One | 主动结合行程 + 天气 + 光线 |
| 告诉你"某地几点日出" | 告诉你"这次旅行哪天哪个时间段最值得早起拍" |

---

## 核心功能

### 1. 实时光线助手
- 自动获取用户当前时间和位置
- 显示当前所处光线阶段（蓝调 / 黄金时段 / 正午 / 日落等）
- 告知用户现在该怎么拍，下一个好时段是什么时候
- 根据器材类型和经验级别给出具体参数建议

### 2. 行程规划
- 输入地点 + 日期范围
- 自动分析每天的光线质量和天气状况
- 标出最佳拍摄日和时间窗口
- 卡片式展示，一眼看出哪天值得早起

---

## 用户设置

> 所有设置存储于 `localStorage`，无需账号注册。

### 器材类型
- 📱 手机（有无 Pro 模式）
- 📷 入门单反 / 微单（APS-C）
- 🎞️ 全幅相机
- 🏄 运动相机

### 拍摄类型
风景 / 城市 / 人像 / 星空夜景

### 时间偏好
早鸟（日出）/ 夜猫（日落）/ 两者都行

### 经验级别
| 级别 | 显示内容 |
|---|---|
| 🌱 入门 | 评级 + 一句话建议 |
| 📷 进阶 | 参数范围建议 |
| 🎯 专业 | 完整数据（EV 值、色温、曝光补偿建议等） |

---

## 参数建议逻辑

- 纯本地逻辑，**无需任何 API**
- 根据光线条件 × 器材类型 × 经验级别硬编码规则
- 所有建议底部统一注明：

> *以上为参考建议，实际效果因场景而异*

---

## 技术方案

| 模块 | 技术选型 | 说明 |
|---|---|---|
| 框架 | Blazor WASM | 静态部署，无需服务器 |
| UI 组件库 | MudBlazor | 深色主题，Material Design |
| 托管 | GitHub Pages | 免费，零维护 |
| 天气 + 云量 | Open-Meteo | 完全免费，无需 API Key |
| 日出日落 + 太阳方向 | SunCalc（JS 库） | 本地运行，无需 API |
| 用户数据 | localStorage | 无需账号 |
| 多语言 | 待实现 | 简体中文 / 繁体中文 / 英文 |

---

## 视觉风格

- **色调**：深色背景（夜晚 / 黄昏感）+ 暖橙色调（黄金时段）
- **风格参考**：Dark Sky / Slopes——简洁有质感
- **布局**：卡片式结果展示，每天一张，最佳拍摄日高亮显示

---

## 项目结构

```
Luma/
├── Layout/
│   ├── MainLayout.razor          # 主布局 UI
│   └── MainLayout.razor.cs       # 主布局逻辑
├── Pages/
│   ├── Home.razor                # 实时光线页面 UI
│   ├── Home.razor.cs             # 实时光线页面逻辑
│   ├── Planner.razor             # 行程规划页面 UI
│   ├── Planner.razor.cs          # 行程规划页面逻辑
│   ├── Settings.razor            # 设置页面 UI
│   └── Settings.razor.cs         # 设置页面逻辑
├── wwwroot/
│   ├── css/app.css               # 全局样式
│   ├── js/luma.js                # JS interop（SunCalc）
│   └── index.html                # 入口 HTML
├── Program.cs                    # 应用入口
└── _Imports.razor                # 全局引用
```

---

## 本地开发

### 环境要求
- .NET 9 SDK
- VS Code + C# Dev Kit 插件

### 启动项目
```bash
git clone https://github.com/miemie123wang/Luma.git
cd Luma/Luma
dotnet run
```

浏览器访问 `http://localhost:5284`

---

## MVP 范围（第一版）

- [x] 基础框架搭建（Blazor WASM + MudBlazor）
- [x] 深色主题 + 暖橙色视觉风格
- [x] 页面结构（实时光线 / 行程规划 / 设置）
- [ ] SunCalc JS interop 接入
- [ ] 实时光线阶段计算
- [ ] 获取用户地理位置
- [ ] 行程规划功能
- [ ] 用户设置 + localStorage
- [ ] Open-Meteo 天气接入
- [ ] 多语言支持（简体 / 繁体 / 英文）
- [ ] GitHub Pages 部署

**暂不做：** 拍摄点推荐、社区功能（后期 UGC）

---

## 未来扩展路线图

- [ ] 离线地图
- [ ] 拍摄点收藏和笔记
- [ ] 社区上传拍摄点（UGC）
- [ ] MAUI 版本（iOS / Android）

---

*最后更新：2026-05-18*
