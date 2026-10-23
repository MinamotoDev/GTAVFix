# GTAVFix

**English** · [Français](docs/readme/README.fr.md) · [Italiano](docs/readme/README.it.md) · [Deutsch](docs/readme/README.de.md) · [Español (España)](docs/readme/README.es-ES.md) · [日本語](docs/readme/README.ja.md) · [한국어](docs/readme/README.ko.md) · [Polski](docs/readme/README.pl.md) · [Português (Brasil)](docs/readme/README.pt-BR.md) · [Русский](docs/readme/README.ru.md) · [简体中文](docs/readme/README.zh-CN.md) · [Español (Latinoamérica)](docs/readme/README.es-419.md) · [正體中文](docs/readme/README.zh-TW.md)

A multilingual font patcher for GTA V Enhanced that fills in missing glyphs in text such as Self Radio track titles.

## Features

- Adds available glyphs for the languages supported by the game.
- Validates source fonts, glyph coverage, and generated resources on every run.
- Automatically backs up resources and supports one-click restoration.

## System Requirements and Dependencies

- 64-bit Windows 10 22H2 or Windows 11.
- GTA V Enhanced.
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) and the Enhanced ASI Loader (`xinput1_4.dll` from the [author’s distribution](https://www.dev-c.com/gtav/scripthookv/)).
- Disable BattlEye. Story Mode only.

## Usage

1. Install OpenRPF and the Enhanced ASI Loader. Place `OpenRPF.asi` and `xinput1_4.dll` in the game folder.
2. Add `-nobattleye` to `args.txt` in the game folder.
3. Run `GTAVFix.exe`, select the folder containing `GTA5_Enhanced.exe`, and click **Patch fonts**.
4. To restore, exit the game and click **Restore backup**.

Patched resources are installed in `mods/update/update.rpf`. Backups are stored in `.gtavfix/backups`. If a mods archive already exists, it is used as the base and checked against the game version.

## Build

Install the .NET 10 SDK and run `./build.ps1` in PowerShell. Output is saved to `artifacts/release/<version>`.

[Release notes](RELEASE_NOTES.md#en) · [Verification](docs/verification.md) · [MIT license](LICENSE) · [Third-party notices](THIRD-PARTY-NOTICES.md)
