param([string]$PlayerPath)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
elseif(![IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath=Join-Path $taskRoot $PlayerPath}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$taskOutput=Join-Path $taskRoot ('Builds/IslandRulesQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')+'-'+[guid]::NewGuid().ToString('N').Substring(0,6))
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
$taskProfile='rules'+[guid]::NewGuid().ToString('N')
$taskArgs=@('-batchmode','-force-d3d11','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-job-worker-count','2','-offline','-sport','Football','-probe','-startAt','10000','-returnAt','10000','-exitAfter','1100','-guestProfile',$taskProfile,'-islandRulesReview',('"'+$taskOutput+'"'),'-report',('"'+$taskOutput+'/probe.txt"'),'-logFile',('"'+$taskOutput+'/player.log"'))
$taskProcess=Start-Process -FilePath $PlayerPath -ArgumentList $taskArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
$null=$taskProcess.Handle
try {
 $taskDeadline=(Get-Date).AddSeconds(900)
 $taskReport=Join-Path $taskOutput 'results.txt'
 do {
  Start-Sleep -Milliseconds 500
  if(Test-Path -LiteralPath $taskReport){
   $taskText=Get-Content -LiteralPath $taskReport -Raw
   if($taskText -match '(?m)^FAIL '){throw "Island rules assertion failed: $taskOutput"}
   if($taskText -match 'ISLAND_RULES_PASS'){break}
  }
 } while(!$taskProcess.HasExited -and (Get-Date) -lt $taskDeadline)
 if(!(Test-Path -LiteralPath $taskReport) -or (Get-Content -LiteralPath $taskReport -Raw) -notmatch 'ISLAND_RULES_PASS'){throw "Island rules review incomplete: $taskOutput"}
 if(!$taskProcess.WaitForExit(10000) -or $taskProcess.ExitCode -ne 0){throw "Island rules player did not exit normally: $taskOutput"}
 if((Get-Content -LiteralPath (Join-Path $taskOutput 'player.log') -Raw) -match 'NullReferenceException|MissingReferenceException|InvalidOperationException|CONTENT_LOAD_FAILED'){throw "Runtime error: $taskOutput"}
 Get-Content -LiteralPath $taskReport -Tail 1
 "Evidence: $taskOutput"
} finally {if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}
