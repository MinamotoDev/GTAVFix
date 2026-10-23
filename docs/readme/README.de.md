# GTAVFix

[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · **Deutsch** · [Español (España)](README.es-ES.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [简体中文](README.zh-CN.md) · [Español (Latinoamérica)](README.es-419.md) · [正體中文](README.zh-TW.md)

Ein Tool zum Ergänzen fehlender Glyphen in GTA V Enhanced, etwa in Musiktiteln von Self Radio.

## Funktionen

- Ergänzt verfügbare Glyphen für die vom Spiel unterstützten Sprachen.
- Prüft bei jedem Durchlauf die Quellschriften, die Glyphenabdeckung und die erzeugten Ressourcen.
- Sichert Ressourcen automatisch und stellt sie mit einem Klick wieder her.

## Systemanforderungen und Abhängigkeiten

- Windows 10 22H2 oder Windows 11 mit 64 Bit.
- GTA V Enhanced.
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) und der Enhanced ASI Loader (`xinput1_4.dll` aus der [Distribution des Autors](https://www.dev-c.com/gtav/scripthookv/)).
- BattlEye deaktivieren. Nur Story-Modus.

## Verwendung

1. Installieren Sie OpenRPF und den Enhanced ASI Loader. Kopieren Sie `OpenRPF.asi` und `xinput1_4.dll` in den Spielordner.
2. Fügen Sie `-nobattleye` zur Datei `args.txt` im Spielordner hinzu.
3. Starten Sie `GTAVFix.exe`, wählen Sie den Ordner mit `GTA5_Enhanced.exe` und klicken Sie auf **Schriften ergänzen**.
4. Zum Wiederherstellen beenden Sie das Spiel und klicken auf **Backup wiederherstellen**.

Ergänzte Ressourcen werden unter `mods/update/update.rpf` installiert. Backups liegen in `.gtavfix/backups`. Ein vorhandenes mods-Archiv dient als Grundlage und wird auf die passende Spielversion geprüft.

## Kompilieren

Installieren Sie das .NET 10 SDK und führen Sie `./build.ps1` in PowerShell aus. Die Ausgabe liegt unter `artifacts/release/<Version>`.

[Versionshinweise](../../RELEASE_NOTES.md#de) · [Prüfprotokoll](../verification.md) · [MIT-Lizenz](../../LICENSE) · [Drittanbieterhinweise](../../THIRD-PARTY-NOTICES.md)
