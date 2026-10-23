# GTAVFix

[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md) · **Español (España)** · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [简体中文](README.zh-CN.md) · [Español (Latinoamérica)](README.es-419.md) · [正體中文](README.zh-TW.md)

Herramienta para completar las fuentes de GTA V Enhanced y añadir los glifos que faltan en textos como los títulos de las canciones de Self Radio.

## Funciones

- Añade los glifos disponibles para los idiomas compatibles con el juego.
- Verifica las fuentes originales, la cobertura de glifos y los recursos generados en cada ejecución.
- Crea copias de seguridad automáticas de los recursos y permite restaurarlos con un clic.

## Requisitos del sistema y dependencias

- Windows 10 22H2 o Windows 11 de 64 bits.
- GTA V Enhanced.
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) y ASI Loader para Enhanced (`xinput1_4.dll` de la [distribución del autor](https://www.dev-c.com/gtav/scripthookv/)).
- Desactiva BattlEye. Solo modo Historia.

## Uso

1. Instala OpenRPF y ASI Loader para Enhanced. Coloca `OpenRPF.asi` y `xinput1_4.dll` en la carpeta del juego.
2. Añade `-nobattleye` a `args.txt` en la carpeta del juego.
3. Ejecuta `GTAVFix.exe`, selecciona la carpeta que contiene `GTA5_Enhanced.exe` y pulsa **Completar fuentes**.
4. Para restaurar los recursos, cierra el juego y pulsa **Restaurar copia**.

Los recursos modificados se instalan en `mods/update/update.rpf`. Las copias se guardan en `.gtavfix/backups`. Si ya existe un archivo mods, se utiliza como base y se comprueba su versión.

## Compilación

Instala el SDK de .NET 10 y ejecuta `./build.ps1` en PowerShell. Los archivos se generan en `artifacts/release/<versión>`.

[Notas de la versión](../../RELEASE_NOTES.md#es-es) · [Verificación](../verification.md) · [Licencia MIT](../../LICENSE) · [Avisos de terceros](../../THIRD-PARTY-NOTICES.md)
