param([string]$PlayerPath,[int]$TimeoutSeconds=100)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$taskOutput=Join-Path $taskRoot ('Builds/GolfHUDQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskOutput | Out-Null
$taskReport=Join-Path $taskOutput 'golf-hud.txt'
$taskLog=Join-Path $taskOutput 'golf-hud.log'
$taskArguments="-batchmode -force-d3d11 -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -job-worker-count 2 -sport Golf -offline -golfHudAudit -probe -exitAfter $TimeoutSeconds -report `"$taskReport`" -logFile `"$taskLog`""
$taskProcess=$null
try {
 $taskProcess=Start-Process -FilePath $PlayerPath -ArgumentList $taskArguments -WindowStyle Hidden -PassThru
 $taskDeadline=(Get-Date).AddSeconds($TimeoutSeconds)
 do {
  if(Test-Path -LiteralPath $taskReport){
   $taskText=Get-Content -LiteralPath $taskReport -Raw
   if($taskText -match '(?m)^FAIL ' -or $taskText -match '(?m)^GOLF_HUD_COMPLETE '){break}
  }
  if($taskProcess.HasExited){break}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $taskDeadline)
 if(!(Test-Path -LiteralPath $taskReport)){throw "Golf HUD review produced no report: $taskOutput"}
 $taskText=Get-Content -LiteralPath $taskReport -Raw
 if($taskText -match '(?m)^FAIL ' -or $taskText -notmatch '(?m)^GOLF_HUD_COMPLETE '){throw "Incomplete or failed Golf HUD review: $taskOutput"}
 $taskLogText=Get-Content -LiteralPath $taskLog -Raw
 if($taskLogText -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException|Shader error)'){throw "Golf HUD runtime exception: $taskOutput"}
 $taskCaptures=@(Get-ChildItem -LiteralPath $taskOutput -Filter '*.png' -File)
 if($taskCaptures.Count -lt 8){throw "Golf HUD review is missing rendered captures: $taskOutput"}
 Write-Output "PASS golf-hud assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count) captures=$($taskCaptures.Count)"
 Write-Output "GOLF_HUD_SUITE_PASS $taskOutput"
}finally{if($taskProcess -and !$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}
