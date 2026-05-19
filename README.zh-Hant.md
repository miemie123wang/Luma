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
- 在首頁選擇當前拍攝類型，供後續參數建議使用；此選擇不寫入長期設定

### 使用者設定
所有設定儲存在 `localStorage`，無需帳號。

- 器材類型：手機、手機 Pro、APS-C、全幅、運動相機
- 經驗等級：入門、進階、專業
- 介面語言：英文、西班牙文、簡體中文、繁體中文

### 行程規劃
行程規劃頁面已建立佔位，完整規劃邏輯尚未實作。

---

## 待實作方向

### 攝影參數建議
- 純本地規則，無需額外 API
- 根據光線階段 × 器材類型 × 當前拍攝類型 × 經驗等級輸出建議
- 入門使用者顯示一句話建議，進階使用者顯示參數範圍，專業使用者顯示更完整資料

### 行程規劃
- 輸入地點與日期範圍
- 分析每日光線品質與天氣狀況
- 標出最佳拍攝日與時間區間

---

## 技術方案

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
├── Layout/        # 主版面與導覽
├── Localization/  # 記憶體本地化實作與翻譯字典
├── Models/        # 位置、光線階段、天氣與設定模型
├── Pages/         # 首頁、行程規劃與設定頁
├── Services/      # 光線階段、設定、SunCalc 與天氣服務
├── wwwroot/       # CSS、JavaScript interop、本地 SunCalc 與入口 HTML
├── Program.cs     # 應用啟動與服務註冊
└── Luma.csproj    # 專案檔
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
- [ ] 攝影參數建議規則
- [ ] 行程規劃功能
- [ ] GitHub Pages 部署

---

*最後更新：2026-05-18*
