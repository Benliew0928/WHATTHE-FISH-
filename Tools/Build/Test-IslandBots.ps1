param(
 [string]$PlayerPath,
 [ValidateSet('All','Fishing','Golf')][string]$Sport='All'
)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $root 'Builds/IslandBotsQA/Player/WhatTheFish.exe'}
$player=(Resolve-Path -LiteralPath $PlayerPath).Path
$run=Join-Path $root ('Builds/IslandBotsQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
$sports=if($Sport -eq 'All'){@('Fishing','Golf')}else{@($Sport)}
foreach($selected in $sports){
 $folder=Join-Path $run $selected
 New-Item -ItemType Directory -Force -Path $folder | Out-Null
 $arguments=@('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-probe','-offline','-sport',$selected,'-islandBotsReview',('"'+$folder+'"'),'-report',('"'+(Join-Path $folder 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
 $process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
 try {
  if(!$process.WaitForExit(900000)){throw "Island bots review timed out: $folder"}
  $result=Get-Content -LiteralPath (Join-Path $folder 'results.txt') -Raw
  if($process.ExitCode -ne 0 -or $result -match '(?m)^FAIL ' -or !$result.Contains('ISLAND_BOTS_COMPLETE success=True')){throw "Island bots review failed: $folder"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'Exception:|Assertion failed|Look rotation viewing vector is zero' -Quiet){throw "Runtime diagnostic: $folder"}
  Write-Output "PASS $selected assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count) $folder"
 }finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
}
