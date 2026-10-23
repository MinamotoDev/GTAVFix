# GTAVFix

[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md) · [Español (España)](README.es-ES.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [简体中文](README.zh-CN.md) · [Español (Latinoamérica)](README.es-419.md) · **正體中文**

GTA V 增強版多語言字庫補全工具，用於補齊自訂電台曲名等文字中缺少的字形。

## 功能

- 補齊遊戲支援語言的可用字形。
- 每次執行時驗證來源字庫、字形涵蓋範圍及產生的資源。
- 自動備份資源，支援一鍵還原。

## 系統需求與相依套件

- 64 位元 Windows 10 22H2 或 Windows 11。
- GTA V 增強版。
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) 和增強版 ASI Loader（[作者發行套件](https://www.dev-c.com/gtav/scripthookv/)中的 `xinput1_4.dll`）。
- 關閉 BattlEye，僅支援故事模式。

## 使用方式

1. 安裝 OpenRPF 和增強版 ASI Loader，將 `OpenRPF.asi`、`xinput1_4.dll` 放入遊戲目錄。
2. 在遊戲目錄的 `args.txt` 中加入 `-nobattleye`。
3. 執行 `GTAVFix.exe`，選擇包含 `GTA5_Enhanced.exe` 的目錄，按下「一鍵補字」。
4. 需要還原時，結束遊戲並按下「還原備份」。

補字資源安裝至 `mods/update/update.rpf`，備份位於 `.gtavfix/backups`。已有 mods 封存檔時，會以該封存檔為基礎產生資源，並檢查其遊戲版本。

## 建置

安裝 .NET 10 SDK，在 PowerShell 中執行 `./build.ps1`，輸出位於 `artifacts/release/<版本號>`。

[版本說明](../../RELEASE_NOTES.md#zh-tw) · [驗證紀錄](../verification.md) · [MIT 授權條款](../../LICENSE) · [第三方聲明](../../THIRD-PARTY-NOTICES.md)
