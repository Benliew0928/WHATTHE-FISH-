param([string]$PlayerPath,[int]$Port=7834)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $root 'Builds/FootballStadiumShowQA/Player/WhatTheFish.exe'}
$run=Join-Path $root ('Builds/FootballStadiumShowQA/Room-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$processes=@()
try {
 foreach($role in @('host','guest','watcher')){
  $folder=Join-Path $run $role;New-Item -ItemType Directory -Path $folder -Force | Out-Null
  $options=@('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2',$(if($role -eq 'host'){'-localHost'}else{'-localClient'}),'-port',"$Port",'-sport','Football','-stadiumShowReview',('"'+$folder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
  if($role -eq 'watcher'){$options+='-stadiumWatcher'}
  $p=Start-Process -FilePath $PlayerPath -ArgumentList $options -WindowStyle Hidden -PassThru;$null=$p.Handle;$processes+=$p
  Start-Sleep -Seconds 2
 }
 foreach($p in $processes){if(!$p.WaitForExit(240000)){throw 'Stadium room review timed out'}}
 $clocks=@()
 foreach($role in @('host','guest','watcher')){
  $folder=Join-Path $run $role;$report=Get-Content (Join-Path $folder 'results.txt') -Raw
  if($report -match '(?m)^FAIL ' -or !$report.Contains('STADIUM_SHOW_COMPLETE success=True')){throw "Failed stadium show: $folder"}
  if(Select-String -Path (Join-Path $folder 'player.log') -Pattern 'GameObjects can not be made active|being destroyed|Exception:|Shader error|Assertion failed' -Quiet){throw "Runtime diagnostic: $folder"}
  $clocks+=Get-Content (Join-Path $folder 'clock.json') -Raw | ConvertFrom-Json
  Write-Output "STADIUM_SHOW_PASS $role checks=$(([regex]::Matches($report,'(?m)^PASS ')).Count)"
 }
 if(@($clocks.finishedAt | Select-Object -Unique).Count -ne 1 -or @($clocks.mvp | Select-Object -Unique).Count -ne 1 -or @($clocks.revision | Select-Object -Unique).Count -ne 1){throw 'Peers disagree on ceremony clock or MVP'}
 Write-Output "STADIUM_SHOW_SHARED_CLOCK_OK $run"
}finally{foreach($p in $processes){if(!$p.HasExited){Stop-Process -Id $p.Id}}}
