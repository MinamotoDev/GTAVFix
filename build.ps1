param([string]$Runtime = 'win-x64')
$ErrorActionPreference = 'Stop'
if ($Runtime -ne 'win-x64') { throw 'Only win-x64 is supported.' }
$projectRoot = $PSScriptRoot
$projectVersion = ([xml](Get-Content -LiteralPath (Join-Path $projectRoot 'Directory.Build.props') -Raw)).Project.PropertyGroup.Version
$publish = Join-Path $projectRoot 'artifacts\publish\win-x64'
$release = Join-Path (Join-Path $projectRoot 'artifacts\release') $projectVersion
New-Item -ItemType Directory -Path $release -Force | Out-Null
dotnet publish (Join-Path $projectRoot 'src\GTAVFix.App\GTAVFix.App.csproj') -c Release -r $Runtime --self-contained true -o $publish --nologo -p:UseSharedCompilation=false -p:DebugType=None -p:DebugSymbols=false -nodeReuse:false
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }
$exe = Join-Path $release 'GTAVFix.exe'
Copy-Item -LiteralPath (Join-Path $publish 'GTAVFix.exe') -Destination $exe -Force
foreach ($name in @('LICENSE','THIRD-PARTY-NOTICES.md','README.md','RELEASE_NOTES.md')) { Copy-Item -LiteralPath (Join-Path $projectRoot $name) -Destination (Join-Path $release $name) -Force }
New-Item -ItemType Directory -Path (Join-Path $release 'docs') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $projectRoot 'docs\verification.md') -Destination (Join-Path $release 'docs\verification.md') -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'docs\readme') -Destination (Join-Path $release 'docs') -Recurse -Force
$hash = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash.ToLowerInvariant()
Set-Content -LiteralPath (Join-Path $release 'SHA256SUMS.txt') -Value "$hash  GTAVFix.exe" -Encoding utf8
$files = @('GTAVFix.exe','LICENSE','THIRD-PARTY-NOTICES.md','README.md','RELEASE_NOTES.md','SHA256SUMS.txt','docs') | ForEach-Object { Join-Path $release $_ }
Compress-Archive -LiteralPath $files -DestinationPath (Join-Path $release ('GTAVFix-'+$projectVersion+'-win-x64.zip')) -Force
Write-Output "EXE: $exe"
Write-Output "SHA256: $hash"
