# GTAVFix

[English](../../README.md) · **Français** · [Italiano](README.it.md) · [Deutsch](README.de.md) · [Español (España)](README.es-ES.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [简体中文](README.zh-CN.md) · [Español (Latinoamérica)](README.es-419.md) · [正體中文](README.zh-TW.md)

Un outil de complément des polices de GTA V Enhanced pour ajouter les glyphes manquants, notamment dans les titres des morceaux de Self Radio.

## Fonctionnalités

- Ajoute les glyphes disponibles pour les langues prises en charge par le jeu.
- Vérifie les polices sources, la couverture des glyphes et les ressources générées à chaque exécution.
- Sauvegarde automatiquement les ressources et permet leur restauration en un clic.

## Configuration requise et dépendances

- Windows 10 22H2 ou Windows 11 en 64 bits.
- GTA V Enhanced.
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) et ASI Loader pour Enhanced (`xinput1_4.dll` inclus dans la [distribution de l’auteur](https://www.dev-c.com/gtav/scripthookv/)).
- Désactivez BattlEye. Mode Histoire uniquement.

## Utilisation

1. Installez OpenRPF et ASI Loader pour Enhanced. Placez `OpenRPF.asi` et `xinput1_4.dll` dans le dossier du jeu.
2. Ajoutez `-nobattleye` au fichier `args.txt` du dossier du jeu.
3. Lancez `GTAVFix.exe`, sélectionnez le dossier contenant `GTA5_Enhanced.exe`, puis cliquez sur **Compléter les polices**.
4. Pour restaurer les ressources, fermez le jeu et cliquez sur **Restaurer**.

Les ressources modifiées sont installées dans `mods/update/update.rpf`. Les sauvegardes se trouvent dans `.gtavfix/backups`. Si une archive mods existe déjà, elle sert de base et sa version est vérifiée.

## Compilation

Installez le SDK .NET 10 et exécutez `./build.ps1` dans PowerShell. Les fichiers sont générés dans `artifacts/release/<version>`.

[Notes de version](../../RELEASE_NOTES.md#fr) · [Vérification](../verification.md) · [Licence MIT](../../LICENSE) · [Mentions tierces](../../THIRD-PARTY-NOTICES.md)
