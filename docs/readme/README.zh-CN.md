# GTAVFix

[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md) · [Español (España)](README.es-ES.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · **简体中文** · [Español (Latinoamérica)](README.es-419.md) · [正體中文](README.zh-TW.md)

GTA V 增强版多语言字库补全工具，用于补齐自定义电台曲名等文字中的缺失字形。

## 功能

- 补齐游戏支持语言的可用字形。
- 每次运行时验证源字库、字形覆盖和生成资源。
- 自动备份资源，支持一键还原。

## 系统要求与依赖

- 64 位 Windows 10 22H2 或 Windows 11。
- GTA V 增强版。
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) 和增强版 ASI Loader（[作者发行包](https://www.dev-c.com/gtav/scripthookv/)中的 `xinput1_4.dll`）。
- 关闭 BattlEye，仅支持故事模式。

## 使用方法

1. 安装 OpenRPF 和增强版 ASI Loader，将 `OpenRPF.asi`、`xinput1_4.dll` 放入游戏目录。
2. 在游戏目录的 `args.txt` 中添加 `-nobattleye`。
3. 运行 `GTAVFix.exe`，选择包含 `GTA5_Enhanced.exe` 的目录，点击「一键补字」。
4. 需要还原时，退出游戏并点击「还原备份」。

补字资源安装至 `mods/update/update.rpf`，备份位于 `.gtavfix/backups`。已有 mods 归档时，以该归档为基础生成，并检查其游戏版本。

## 构建

安装 .NET 10 SDK，在 PowerShell 中运行 `./build.ps1`，输出位于 `artifacts/release/<版本号>`。

[发布说明](../../RELEASE_NOTES.md#zh-cn) · [验证记录](../verification.md) · [MIT 许可证](../../LICENSE) · [第三方说明](../../THIRD-PARTY-NOTICES.md)
