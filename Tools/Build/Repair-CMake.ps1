$ErrorActionPreference = 'Stop'
$cmakeDestination = 'C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\cmake\3.22.1'
New-Item -ItemType Directory -Force -Path $cmakeDestination | Out-Null
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
& tar -xf (Join-Path $root 'Tools\Downloads\cmake-3.22.1.zip') -C $cmakeDestination
if ($LASTEXITCODE -ne 0) { throw 'CMake extraction failed' }
Test-Path -LiteralPath "$cmakeDestination\bin\cmake.exe" | Out-File (Join-Path $root 'Builds\cmake-repair.txt')
