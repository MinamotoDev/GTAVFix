# GTAVFix

[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md) · [Español (España)](README.es-ES.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · **Polski** · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [简体中文](README.zh-CN.md) · [Español (Latinoamérica)](README.es-419.md) · [正體中文](README.zh-TW.md)

Narzędzie do uzupełniania czcionek GTA V Enhanced, które dodaje brakujące glify w tekstach, takich jak tytuły utworów w Self Radio.

## Funkcje

- Uzupełnia dostępne glify języków obsługiwanych przez grę.
- Przy każdym uruchomieniu sprawdza czcionki źródłowe, pokrycie glifów i wygenerowane zasoby.
- Automatycznie tworzy kopie zasobów i umożliwia przywracanie jednym kliknięciem.

## Wymagania systemowe i zależności

- 64-bitowy Windows 10 22H2 lub Windows 11.
- GTA V Enhanced.
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) i ASI Loader dla Enhanced (`xinput1_4.dll` z [pakietu autora](https://www.dev-c.com/gtav/scripthookv/)).
- Wyłącz BattlEye. Tylko tryb fabularny.

## Użycie

1. Zainstaluj OpenRPF i ASI Loader dla Enhanced. Umieść `OpenRPF.asi` oraz `xinput1_4.dll` w folderze gry.
2. Dodaj `-nobattleye` do pliku `args.txt` w folderze gry.
3. Uruchom `GTAVFix.exe`, wybierz folder zawierający `GTA5_Enhanced.exe` i kliknij **Uzupełnij czcionki**.
4. Aby przywrócić zasoby, zamknij grę i kliknij **Przywróć kopię**.

Zmodyfikowane zasoby są instalowane w `mods/update/update.rpf`. Kopie są zapisywane w `.gtavfix/backups`. Istniejące archiwum mods jest używane jako baza i sprawdzane pod kątem wersji gry.

## Kompilacja

Zainstaluj .NET 10 SDK i uruchom `./build.ps1` w PowerShell. Pliki wynikowe znajdziesz w `artifacts/release/<wersja>`.

[Informacje o wydaniu](../../RELEASE_NOTES.md#pl) · [Weryfikacja](../verification.md) · [Licencja MIT](../../LICENSE) · [Informacje o podmiotach trzecich](../../THIRD-PARTY-NOTICES.md)
