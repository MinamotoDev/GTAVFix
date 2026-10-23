# GTAVFix

[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md) · [Español (España)](README.es-ES.md) · [日本語](README.ja.md) · **한국어** · [Polski](README.pl.md) · [Português (Brasil)](README.pt-BR.md) · [Русский](README.ru.md) · [简体中文](README.zh-CN.md) · [Español (Latinoamérica)](README.es-419.md) · [正體中文](README.zh-TW.md)

Self Radio 곡명 등에서 누락된 글리프를 추가하는 GTA V Enhanced 다국어 글꼴 보완 도구입니다.

## 기능

- 게임에서 지원하는 언어의 사용 가능한 글리프를 보완합니다.
- 실행할 때마다 원본 글꼴, 글리프 지원 범위 및 생성된 리소스를 검증합니다.
- 리소스를 자동으로 백업하고 클릭 한 번으로 복원할 수 있습니다.

## 시스템 요구 사항 및 의존성

- 64비트 Windows 10 22H2 또는 Windows 11.
- GTA V Enhanced.
- [OpenRPF 0.3](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced/) 및 Enhanced용 ASI Loader([제작자 배포 패키지](https://www.dev-c.com/gtav/scripthookv/)에 포함된 `xinput1_4.dll`).
- BattlEye를 비활성화하세요. 스토리 모드 전용입니다.

## 사용 방법

1. OpenRPF와 Enhanced용 ASI Loader를 설치하고 `OpenRPF.asi`와 `xinput1_4.dll`을 게임 폴더에 넣으세요.
2. 게임 폴더의 `args.txt`에 `-nobattleye`를 추가하세요.
3. `GTAVFix.exe`를 실행하고 `GTA5_Enhanced.exe`가 있는 폴더를 선택한 후 **글꼴 보완**을 누르세요.
4. 복원하려면 게임을 종료하고 **백업 복원**을 누르세요.

보완된 리소스는 `mods/update/update.rpf`에 설치되며 백업은 `.gtavfix/backups`에 저장됩니다. 기존 mods 아카이브가 있으면 해당 파일을 기반으로 생성하고 게임 버전을 확인합니다.

## 빌드

.NET 10 SDK를 설치하고 PowerShell에서 `./build.ps1`을 실행하세요. 결과물은 `artifacts/release/<버전>`에 저장됩니다.

[릴리스 노트](../../RELEASE_NOTES.md#ko) · [검증 기록](../verification.md) · [MIT 라이선스](../../LICENSE) · [타사 고지](../../THIRD-PARTY-NOTICES.md)
