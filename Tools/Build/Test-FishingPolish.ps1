param([string]$PlayerPath)
$ErrorActionPreference='Stop'
$taskRepo=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRepo 'Builds/WindowsFinal/WhatTheFish.exe'}
if(!(Test-Path -LiteralPath $PlayerPath -PathType Leaf)){throw 'Complete Windows player not found.'}
$taskOutput=Join-Path $taskRepo ('Builds/FishingPolishQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskOutput | Out-Null
$taskReport=Join-Path $taskOutput 'polish.txt';$taskLog=Join-Path $taskOutput 'polish.log'
$taskGraphics='-batchmode -force-d3d11 -screen-width 1600 -screen-height 900 -screen-fullscreen 0'
$taskOptions="$taskGraphics -job-worker-count 2 -sport Fishing -offline -fishingPolishAudit -probe -exitAfter 120 -report `"$taskReport`" -logFile `"$taskLog`""
$taskProcess=$null
try {
 $taskProcess=Start-Process -FilePath $PlayerPath -ArgumentList $taskOptions -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
 $taskDeadline=(Get-Date).AddSeconds(115)
 do {
  if(Test-Path -LiteralPath $taskReport){$taskText=Get-Content -LiteralPath $taskReport -Raw;if($taskText -match '(?m)^FAIL ' -or $taskText.Contains('FISHING_POLISH_COMPLETE')){break}}
  if($taskProcess.HasExited){break}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $taskDeadline)
 $taskText=Get-Content -LiteralPath $taskReport -Raw
 if($taskText -match '(?m)^FAIL ' -or !$taskText.Contains('FISHING_POLISH_COMPLETE')){throw "Incomplete/failed Fishing polish review: $taskOutput"}
 $taskLogText=Get-Content -LiteralPath $taskLog -Raw
 if($taskLogText -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException|Shader error)'){throw "Runtime exception: $taskOutput"}
 "FISHING_POLISH_SUITE_PASS assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count) $taskOutput"
}finally{if($taskProcess -and !$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}
