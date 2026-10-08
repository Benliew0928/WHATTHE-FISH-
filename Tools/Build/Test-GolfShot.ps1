param([string]$PlayerPath,[switch]$Render)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$taskOutput=Join-Path $taskRoot ('Builds/GolfShotQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskOutput | Out-Null
$taskReport=Join-Path $taskOutput 'shot.txt';$taskLog=Join-Path $taskOutput 'shot.log'
$taskGraphics=if($Render){'-batchmode -force-d3d11 -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -golfShotCapture'}else{'-batchmode -nographics'}
$taskOptions="$taskGraphics -job-worker-count 2 -sport Golf -offline -golfShotAudit -probe -exitAfter 200 -report `"$taskReport`" -logFile `"$taskLog`""
$taskProcess=Start-Process -FilePath $PlayerPath -ArgumentList $taskOptions -WindowStyle Hidden -PassThru
try {
 $taskDeadline=(Get-Date).AddSeconds(180)
 do {
  if((Test-Path -LiteralPath $taskReport) -and (Select-String -LiteralPath $taskReport -Pattern 'GOLF_SHOT_COMPLETE|^FAIL ' -Quiet)){break}
  if($taskProcess.HasExited){break}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $taskDeadline)
 $taskText=Get-Content -LiteralPath $taskReport -Raw
 if($taskText -match '(?m)^FAIL ' -or !$taskText.Contains('GOLF_SHOT_COMPLETE')){throw "Incomplete/failed Shot: $taskOutput"}
 $taskLogText=Get-Content -LiteralPath $taskLog -Raw
 if($taskLogText -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException)'){throw 'Shot runtime exception'}
 Write-Output "GOLF_SHOT_SUITE_PASS assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count) $taskOutput"
}finally{if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}
