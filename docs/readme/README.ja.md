# GTAVFix

[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md) · [Español (España)](README.es-ES.md) · **日本語** · [한국어](README.ko.md) · [Polski](README.pl.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [简体中文](README.zh-CN.md) · [Español (Latinoamérica)](README.es-419.md) · [正體中文](README.zh-TW.md)

GTA V Enhanced のフォントを補完し、Self Radio の曲名などで不足するグリフを追加するツールです。

## 機能

- ゲームが対応する言語の利用可能なグリフを補完します。
- 実行のたびに元のフォント、グリフの対応範囲、生成したリソースを検証します。
- リソースを自動でバックアップし、ワンクリックで復元できます。

## システム要件と依存関係

- 64 ビット版 Windows 10 22H2 または Windows 11。
- GTA V Enhanced。
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) と Enhanced 用 ASI Loader（[作者の配布パッケージ](https://www.dev-c.com/gtav/scripthookv/)内の `xinput1_4.dll`）。
- BattlEye を無効にしてください。ストーリーモード専用です。

## 使い方

1. OpenRPF と Enhanced 用 ASI Loader をインストールし、`OpenRPF.asi` と `xinput1_4.dll` をゲームフォルダーに配置します。
2. ゲームフォルダーの `args.txt` に `-nobattleye` を追加します。
3. `GTAVFix.exe` を起動し、`GTA5_Enhanced.exe` があるフォルダーを選択して「**フォントを補完**」をクリックします。
4. 復元する場合はゲームを終了し、「**バックアップを復元**」をクリックします。

補完リソースは `mods/update/update.rpf` にインストールされ、バックアップは `.gtavfix/backups` に保存されます。既存の mods アーカイブがある場合は、それを基に生成し、ゲームのバージョンを確認します。

## ビルド

.NET 10 SDK をインストールし、PowerShell で `./build.ps1` を実行します。出力先は `artifacts/release/<バージョン>` です。

[リリースノート](../../RELEASE_NOTES.md#ja) · [検証記録](../verification.md) · [MIT ライセンス](../../LICENSE) · [サードパーティに関する表記](../../THIRD-PARTY-NOTICES.md)
