param(
 [string]$PlayerPath,
 [ValidateSet('Basketball','Football','Golf','Fishing','All')][string]$Sport='All'
)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$taskOutput=Join-Path $taskRoot ('Builds/TutorialQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')+'-'+[guid]::NewGuid().ToString('N').Substring(0,6))
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
$taskSports=if($Sport -eq 'All'){@('Basketball','Football','Golf','Fishing')}else{@($Sport)}
foreach($taskSport in $taskSports){
 $taskProfile='layout'+[guid]::NewGuid().ToString('N')
 foreach($taskPhase in @('Lessons')){
  $taskFolder=Join-Path $taskOutput "$taskSport-$taskPhase"
  New-Item -ItemType Directory -Path $taskFolder -Force | Out-Null
  $taskArgs="-batchmode -force-d3d11 -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -job-worker-count 2 -offline -sport $taskSport -probe -startAt 10000 -returnAt 10000 -exitAfter 420 -guestProfile $taskProfile -tutorialReview `"$taskFolder`" -report `"$taskFolder/probe.txt`" -logFile `"$taskFolder/player.log`""
  if($taskPhase -eq 'Restart'){$taskArgs+=' -layoutRestart'}
  $taskProcess=Start-Process -FilePath $PlayerPath -ArgumentList $taskArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
  try{
   $taskDeadline=(Get-Date).AddSeconds(420)
   do{
    Start-Sleep -Milliseconds 500
    $taskReport=Join-Path $taskFolder 'results.txt'
    if(Test-Path -LiteralPath $taskReport){
     $taskText=Get-Content -LiteralPath $taskReport -Raw
     if($taskText -match '(?m)^FAIL '){throw "Tutorial assertion failed: $taskFolder"}
     if($taskText -match 'TUTORIAL_COMPLETE'){break}
    }
   }while(!$taskProcess.HasExited -and (Get-Date) -lt $taskDeadline)
   if(!(Test-Path -LiteralPath $taskReport) -or (Get-Content -LiteralPath $taskReport -Raw) -notmatch 'TUTORIAL_COMPLETE'){throw "Tutorial review incomplete: $taskFolder"}
   if((Get-Content -LiteralPath (Join-Path $taskFolder 'player.log') -Raw) -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException|Shader error)'){throw "Runtime error: $taskFolder"}
   "PASS $taskSport $taskPhase assertions=$(([regex]::Matches((Get-Content -LiteralPath $taskReport -Raw),'(?m)^PASS ')).Count)" | Tee-Object -FilePath (Join-Path $taskOutput 'results.txt') -Append
  }finally{if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id -ErrorAction SilentlyContinue}}
 }
}
"TUTORIAL_SUITE_PASS $taskOutput"
