# Luma 🌅
> 旅行攝影師的光線助手

---

## 專案簡介

Luma 是一個面向旅行攝影師的光線助手應用，幫助使用者在旅途中判斷當前光線階段、天氣條件，以及下一段適合拍攝的時間窗口。

目標使用者是在旅途中帶著相機、想抓好光但不想做太多研究的攝影愛好者。

---

## 目前功能

### 即時光線助手
- 自動取得瀏覽器目前位置
- 使用 SunCalc 計算日出、日落、藍調時段、黃金時段等太陽時間
- 依照目前時間判斷光線階段，並顯示名稱、說明、下一階段與 1-5 星拍攝評級
- 使用 Open-Meteo 顯示雲量、降水、天氣、溫度、風速與能見度
- 使用 OpenStreetMap Nominatim 反向地理編碼顯示城市 / 區縣名稱
- 支援高海拔提示
- 在首頁選擇目前拍攝類型、拍攝方式（手持 / 三腳架）和主體狀態（靜態 / 運動）；這些選擇不寫入長期設定

### 本地拍攝建議
- 使用純本地規則邏輯，無需 AI API 或額外付費服務
- 根據目前光線、天氣、拍攝類型、器材、經驗等級、拍攝方式和主體狀態產生建議
- 使用模組化建議結構：可行性提示、第一張測試、先注意的風險、如果不對先調什麼、入門者步驟
- 第一張測試會給出更保守的起始參數：相機使用者包含 ISO、光圈、快門、曝光補償；手機使用者包含鏡頭、模式、曝光操作和穩定方式
- 夜晚 / 弱光會先依照光線階段和手持 / 三腳架分流，避免夜晚給出白天參數；手持夜景會優先提示找穩定支撐
- 針對手機、手機 Pro、APS-C、全幅和運動相機使用不同操作語言
- 經驗等級會影響操作模式和說明深度：入門偏 A/Av 與保守範圍，進階開始引入 A/Av 或 M 模式，專業偏 RAW、手動控制、包圍曝光和明確取捨
- 文件已依功能分類整理，拍攝建議設計記錄於 [docs/advice/design.md](docs/advice/design.md)，AI 審查流程記錄於 [docs/advice/audit.md](docs/advice/audit.md)

### Copy AI Prompt
- 建議卡片右上角提供 `Copy AI prompt` 按鈕
- 一鍵複製目前時間、光線階段、地點、天氣、器材、經驗、拍攝類型、手持 / 三腳架和主體狀態
- Prompt 只複製現場上下文，不複製本地硬編碼建議，避免外部 AI 被本地規則帶偏
- 複製狀態會短暫顯示並自動消失

### 使用者設定
所有設定儲存在 `localStorage`，無需帳號。

- 器材類型：手機、手機 Pro、APS-C、全幅、運動相機
- 經驗等級：入門、進階、專業
- 介面語言：英文、西班牙文、簡體中文、繁體中文

### 行程規劃
行程規劃頁面已建立佔位，完整規劃邏輯尚未實作。

---

## 後續方向

### 拍攝建議下一步
- 增加鏡頭焦段：廣角 / 標準 / 長焦
- 增加鏡頭最大光圈：f/1.8 / f/2.8 / f/4 / f/5.6
- 增加 RAW / JPEG 偏好
- 根據距離日出 / 日落的剩餘時間進一步調整建議
- 將城市、人像、星空等類型繼續拆細，例如街拍、建築、車流光軌、單人 / 多人 / 兒童人像、月相和光污染

### 行程規劃
- 輸入地點與日期範圍
- 分析每日光線品質與天氣狀況
- 標出最佳拍攝日與時間區間

---

## 技術方案

完整文件入口見 [docs/README.md](docs/README.md)。

| 模組 | 技術選型 | 說明 |
|---|---|---|
| 框架 | Blazor WebAssembly / .NET 9 | 靜態前端應用 |
| UI 元件庫 | MudBlazor | 深色 Material Design 介面 |
| 天氣 + 雲量 | Open-Meteo | 免費，無需 API Key |
| 日出日落 + 光線階段 | 本地 SunCalc + C# 服務 | JS 計算太陽時間，C# 判斷光線階段 |
| 定位 | Browser Geolocation API | 透過 JS interop 取得位置 |
| 地名 | OpenStreetMap Nominatim | 反向地理編碼；失敗時降級為座標顯示 |
| 使用者資料 | localStorage | 無需帳號 |
| 多語言 | 自訂 `IStringLocalizer` + `Translations.cs` | 英文 / 西班牙文 / 簡體中文 / 繁體中文 |
| 託管 | GitHub Pages（計畫） | 靜態部署 |

---

## 專案結構

```text
Luma/
├── Components/    # 功能專用 UI 子元件
├── Layout/        # 主版面與導覽
├── Localization/  # 記憶體本地化實作與翻譯字典
├── Models/        # 位置、光線階段、天氣、設定與建議模型
├── Pages/         # 首頁、行程規劃與設定頁
├── Services/      # 光線階段、設定、拍攝建議、SunCalc 與天氣服務
├── wwwroot/       # CSS、JavaScript interop、本地 SunCalc 與入口 HTML
├── Program.cs     # 應用啟動與服務註冊
└── Luma.csproj    # 專案檔

tools/
└── Luma.LocalizationCheck/ # 本地化 key 與佔位符校驗工具
```

根目錄的 `global.json` 將 SDK 鎖定到 .NET 9，避免本機預設 .NET 10 SDK 造成建置差異。

---

## 本地開發

需求：
- .NET 9 SDK
- VS Code + C# Dev Kit

啟動專案：
```powershell
git clone https://github.com/miemie123wang/Luma.git
cd Luma/Luma
dotnet run
```

預設開發網址為 `http://localhost:5284`。

建置檢查：
```powershell
cd Luma/Luma
dotnet build
```

本地化校驗：
```powershell
cd Luma
dotnet run --project tools/Luma.LocalizationCheck/Luma.LocalizationCheck.csproj
```

此檢查會確認所有語言擁有同一組翻譯 key，並校驗 `{0}`、`{1}` 等格式化佔位符是否一致。

---

## MVP 狀態

- [x] Blazor WASM + MudBlazor 基礎框架
- [x] 深色主題與暖橙色視覺風格
- [x] 即時光線 / 行程規劃 / 設定頁面結構
- [x] SunCalc JS interop
- [x] 即時光線階段計算
- [x] 瀏覽器地理定位
- [x] 使用者設定 + localStorage
- [x] Open-Meteo 天氣整合
- [x] 英文 / 西班牙文 / 簡體中文 / 繁體中文本地化
- [x] 本地規則式拍攝建議
- [x] Copy AI Prompt
- [ ] 行程規劃功能
- [ ] GitHub Pages 部署

---

*最後更新：2026-05-19*
