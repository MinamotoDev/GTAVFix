# GTAVFix

[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md) · [Español (España)](README.es-ES.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · **Português (Brasil)** · [Русский](README.ru.md) · [简体中文](README.zh-CN.md) · [Español (Latinoamérica)](README.es-419.md) · [正體中文](README.zh-TW.md)

Ferramenta de complementação de fontes do GTA V Enhanced que adiciona glifos ausentes em textos, como os títulos das músicas da Self Radio.

## Recursos

- Complementa os glifos disponíveis dos idiomas compatíveis com o jogo.
- Verifica as fontes de origem, a cobertura de glifos e os recursos gerados a cada execução.
- Faz backup automático dos recursos e permite restaurá-los com um clique.

## Requisitos do sistema e dependências

- Windows 10 22H2 ou Windows 11 de 64 bits.
- GTA V Enhanced.
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) e ASI Loader para Enhanced (`xinput1_4.dll` do [pacote do autor](https://www.dev-c.com/gtav/scripthookv/)).
- Desative o BattlEye. Apenas modo História.

## Como usar

1. Instale o OpenRPF e o ASI Loader para Enhanced. Coloque `OpenRPF.asi` e `xinput1_4.dll` na pasta do jogo.
2. Adicione `-nobattleye` ao arquivo `args.txt` na pasta do jogo.
3. Execute `GTAVFix.exe`, selecione a pasta que contém `GTA5_Enhanced.exe` e clique em **Completar fontes**.
4. Para restaurar os recursos, feche o jogo e clique em **Restaurar backup**.

Os recursos modificados são instalados em `mods/update/update.rpf`. Os backups ficam em `.gtavfix/backups`. Se já houver um arquivo mods, ele será usado como base e sua versão será verificada.

## Compilação

Instale o SDK do .NET 10 e execute `./build.ps1` no PowerShell. Os arquivos são gerados em `artifacts/release/<versão>`.

[Notas da versão](../../RELEASE_NOTES.md#pt-br) · [Verificação](../verification.md) · [Licença MIT](../../LICENSE) · [Avisos de terceiros](../../THIRD-PARTY-NOTICES.md)
