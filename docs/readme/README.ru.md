# GTAVFix

[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md) · [Español (España)](README.es-ES.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Português (Brasil)](README.pt-BR.md) · **Русский** · [简体中文](README.zh-CN.md) · [Español (Latinoamérica)](README.es-419.md) · [正體中文](README.zh-TW.md)

Инструмент дополнения шрифтов GTA V Enhanced, добавляющий недостающие глифы в текстах, например в названиях треков Self Radio.

## Возможности

- Дополняет доступные глифы для языков, поддерживаемых игрой.
- При каждом запуске проверяет исходные шрифты, покрытие глифов и созданные ресурсы.
- Автоматически создаёт резервные копии ресурсов и восстанавливает их одним нажатием.

## Системные требования и зависимости

- 64-разрядная Windows 10 22H2 или Windows 11.
- GTA V Enhanced.
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) и ASI Loader для Enhanced (`xinput1_4.dll` из [пакета автора](https://www.dev-c.com/gtav/scripthookv/)).
- Отключите BattlEye. Только сюжетный режим.

## Использование

1. Установите OpenRPF и ASI Loader для Enhanced. Поместите `OpenRPF.asi` и `xinput1_4.dll` в папку игры.
2. Добавьте `-nobattleye` в файл `args.txt` в папке игры.
3. Запустите `GTAVFix.exe`, выберите папку с `GTA5_Enhanced.exe` и нажмите **Дополнить шрифты**.
4. Для восстановления закройте игру и нажмите **Восстановить копию**.

Изменённые ресурсы устанавливаются в `mods/update/update.rpf`, а резервные копии хранятся в `.gtavfix/backups`. Если архив mods уже существует, он используется как основа с проверкой версии игры.

## Сборка

Установите .NET 10 SDK и запустите `./build.ps1` в PowerShell. Результат сохраняется в `artifacts/release/<версия>`.

[Примечания к выпуску](../../RELEASE_NOTES.md#ru) · [Проверка](../verification.md) · [Лицензия MIT](../../LICENSE) · [Уведомления о сторонних компонентах](../../THIRD-PARTY-NOTICES.md)
