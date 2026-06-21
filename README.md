# 📝 待辦事項管理系統 — TodoManager

> **Windows Programming (II) 期末專題**  
> C# WinForms + SQLite 桌面應用程式

---

## 📌 專案簡介

TodoManager 是一套基於 **C# WinForms** 開發的桌面待辦事項管理工具。  
使用 **SQLite** 本地資料庫進行資料儲存，支援新增、編輯、刪除、篩選與完成狀態管理，
並提供即時統計儀表板讓使用者一目瞭然掌握進度。

### ✨ 功能特色

| 功能 | 說明 |
|------|------|
| ➕ 新增事項 | 設定標題、描述、優先等級、分類、截止日期 |
| ✏️ 編輯事項 | 雙擊或點擊按鈕即可修改所有欄位 |
| 🗑️ 刪除事項 | 單筆刪除或一鍵清除所有已完成事項 |
| ✅ 切換狀態 | 一鍵標記完成／恢復進行中 |
| 🔍 搜尋篩選 | 依關鍵字、狀態（全部/進行中/已完成/已逾期）、分類篩選 |
| 📊 統計儀表板 | 即時顯示全部、已完成、已逾期數量與完成率 |
| 💾 本地存檔 | 資料自動儲存至 SQLite 資料庫，重開程式不遺失 |
| 🔴 逾期提醒 | 逾期事項以紅色高亮顯示 |

---

## 🖥️ 執行環境

| 需求 | 版本 |
|------|------|
| 作業系統 | Windows 10 / 11 |
| .NET SDK | .NET 8.0 (Windows) |
| IDE | Visual Studio 2022+ |

---

## 🚀 快速開始

### 1. 下載專案

```bash
git clone https://github.com/<你的帳號>/TodoManager.git
cd TodoManager
```

### 2. 安裝套件

```bash
dotnet restore
```

### 3. 執行程式

```bash
dotnet run
```

或在 **Visual Studio 2022** 中開啟 `TodoManager.csproj`，按 `F5` 執行。

---

## 📂 專案結構

```
TodoManager/
├── Models/
│   └── TodoItem.cs          # 資料模型（TodoItem、Priority 列舉）
├── Data/
│   └── DatabaseHelper.cs    # SQLite CRUD 操作封裝
├── Forms/
│   ├── MainForm.cs          # 主視窗（列表、統計、工具列）
│   └── TodoEditForm.cs      # 新增／編輯對話框
├── Program.cs               # 程式進入點
├── TodoManager.csproj       # 專案設定
├── .gitignore               # 排除 bin/obj/.vs/.git
└── README.md
```

---

## 🛠️ 使用技術

- **語言**：C# 12
- **框架**：.NET 8.0 WinForms
- **資料庫**：SQLite（`System.Data.SQLite` NuGet 套件）
- **版本控制**：Git + GitHub

---

## 📄 資料儲存位置

資料庫自動儲存於：
```
%AppData%\TodoManager\todos.db
```
（例：`C:\Users\你的帳號\AppData\Roaming\TodoManager\todos.db`）

---

## 📝 Commit 規範

本專案採用語意化提交訊息：

```
feat: 新增功能
fix: 修復問題
ui: 介面調整
refactor: 重構
docs: 文件更新
```

---

## 👤 作者

- 學號：1133313
- 姓名：黃以慈
- 課程：視窗程式設計 (II) Windows Programming (II)
