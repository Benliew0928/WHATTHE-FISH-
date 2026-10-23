param(
 [string]$PlayerPath,
 [ValidateSet('Basketball','Football','Golf','Fishing','All')][string]$Sport='All',
 [switch]$DesignMutation
)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$taskOutput=Join-Path $taskRoot ('Builds/ControlLayoutQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
$taskSports=if($Sport -eq 'All'){@('Basketball','Football','Golf','Fishing')}else{@($Sport)}
foreach($taskSport in $taskSports){
 $taskProfile='layout'+[guid]::NewGuid().ToString('N')
 foreach($taskPhase in @('Edit','Restart')){
  $taskFolder=Join-Path $taskOutput "$taskSport-$taskPhase"
  New-Item -ItemType Directory -Path $taskFolder -Force | Out-Null
  $taskArgs="-batchmode -force-d3d11 -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -job-worker-count 2 -offline -sport $taskSport -probe -startAt 10000 -returnAt 10000 -exitAfter 240 -guestProfile $taskProfile -layoutReview `"$taskFolder`" -report `"$taskFolder/probe.txt`" -logFile `"$taskFolder/player.log`""
  if($taskPhase -eq 'Restart'){$taskArgs+=' -layoutRestart'}
  if($DesignMutation){$taskArgs+=' -layoutDesignMutation'}
  $taskProcess=Start-Process -FilePath $PlayerPath -ArgumentList $taskArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
  try{
   $taskDeadline=(Get-Date).AddSeconds(240)
   do{
    Start-Sleep -Milliseconds 500
    $taskReport=Join-Path $taskFolder 'results.txt'
    if(Test-Path -LiteralPath $taskReport){
     $taskText=Get-Content -LiteralPath $taskReport -Raw
     if($taskText -match '(?m)^FAIL '){throw "Layout assertion failed: $taskFolder"}
     if($taskText -match 'CONTROL_LAYOUT_COMPLETE'){break}
    }
   }while(!$taskProcess.HasExited -and (Get-Date) -lt $taskDeadline)
   if(!(Test-Path -LiteralPath $taskReport) -or (Get-Content -LiteralPath $taskReport -Raw) -notmatch 'CONTROL_LAYOUT_COMPLETE'){throw "Layout review incomplete: $taskFolder"}
   if((Get-Content -LiteralPath (Join-Path $taskFolder 'player.log') -Raw) -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException|Shader error)'){throw "Runtime error: $taskFolder"}
   "PASS $taskSport $taskPhase assertions=$(([regex]::Matches((Get-Content -LiteralPath $taskReport -Raw),'(?m)^PASS ')).Count)" | Tee-Object -FilePath (Join-Path $taskOutput 'results.txt') -Append
  }finally{if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id -ErrorAction SilentlyContinue}}
 }
}
"CONTROL_LAYOUT_SUITE_PASS $taskOutput"
