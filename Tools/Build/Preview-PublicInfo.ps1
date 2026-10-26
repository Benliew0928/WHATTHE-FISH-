param([int]$Port=8765,[string]$Python='python')
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
& $Python (Join-Path $PSScriptRoot 'Prepare-PublicInfo.py')
if($LASTEXITCODE -ne 0){throw 'Public information preparation failed.'}
Write-Output "Local preview: http://127.0.0.1:$Port/"
& $Python -m http.server $Port --bind 127.0.0.1 --directory (Join-Path $taskRoot 'Builds/PublicInfoQA/Site')
