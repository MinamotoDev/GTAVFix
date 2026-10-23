# GTAVFix

[English](../../README.md) · [Français](README.fr.md) · **Italiano** · [Deutsch](README.de.md) · [Español (España)](README.es-ES.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [简体中文](README.zh-CN.md) · [Español (Latinoamérica)](README.es-419.md) · [正體中文](README.zh-TW.md)

Uno strumento per completare i caratteri di GTA V Enhanced e aggiungere i glifi mancanti, ad esempio nei titoli dei brani di Self Radio.

## Funzionalità

- Aggiunge i glifi disponibili per le lingue supportate dal gioco.
- Verifica i caratteri di origine, la copertura dei glifi e le risorse generate a ogni avvio.
- Crea automaticamente un backup delle risorse e consente il ripristino con un clic.

## Requisiti di sistema e dipendenze

- Windows 10 22H2 o Windows 11 a 64 bit.
- GTA V Enhanced.
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) e ASI Loader per Enhanced (`xinput1_4.dll` incluso nella [distribuzione dell’autore](https://www.dev-c.com/gtav/scripthookv/)).
- Disattiva BattlEye. Solo modalità Storia.

## Utilizzo

1. Installa OpenRPF e ASI Loader per Enhanced. Copia `OpenRPF.asi` e `xinput1_4.dll` nella cartella del gioco.
2. Aggiungi `-nobattleye` al file `args.txt` nella cartella del gioco.
3. Avvia `GTAVFix.exe`, seleziona la cartella contenente `GTA5_Enhanced.exe` e fai clic su **Completa caratteri**.
4. Per ripristinare le risorse, chiudi il gioco e fai clic su **Ripristina backup**.

Le risorse modificate vengono installate in `mods/update/update.rpf`. I backup si trovano in `.gtavfix/backups`. Se esiste già un archivio mods, viene usato come base e ne viene verificata la versione.

## Compilazione

Installa .NET 10 SDK ed esegui `./build.ps1` in PowerShell. I file vengono generati in `artifacts/release/<versione>`.

[Note di rilascio](../../RELEASE_NOTES.md#it) · [Verifica](../verification.md) · [Licenza MIT](../../LICENSE) · [Note sulle terze parti](../../THIRD-PARTY-NOTICES.md)
