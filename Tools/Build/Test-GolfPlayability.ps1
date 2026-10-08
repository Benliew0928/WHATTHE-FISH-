param([string]$PlayerPath)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$taskOutput=Join-Path $taskRoot ('Builds/GolfPlayabilityQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskOutput | Out-Null
$taskReport=Join-Path $taskOutput 'playability.txt'
$taskLog=Join-Path $taskOutput 'player.log'
$taskOptions="-batchmode -nographics -job-worker-count 2 -sport Golf -offline -probe -golfPlayabilityAudit -exitAfter 1800 -report `"$taskReport`" -logFile `"$taskLog`""
$taskProcess=Start-Process -FilePath $PlayerPath -ArgumentList $taskOptions -PassThru -WindowStyle Hidden
try {
 $taskDeadline=(Get-Date).AddSeconds(180)
 do {
  if(Test-Path $taskReport){$taskText=Get-Content $taskReport -Raw;if($taskText -match '(?m)^FAIL |GOLF_PLAYABILITY_COMPLETE'){break}}
  if($taskProcess.HasExited){break}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $taskDeadline)
 $taskText=Get-Content $taskReport -Raw
 if($taskText -match '(?m)^FAIL ' -or $taskText -notmatch 'GOLF_PLAYABILITY_COMPLETE'){throw "Golf playability incomplete/failed: $taskOutput"}
 if((Get-Content $taskLog -Raw) -match 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException'){throw "Runtime error: $taskOutput"}
 Write-Output "PASS course rolling, material resistance, directional putts and charge distances: $taskOutput"
}finally{if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}
