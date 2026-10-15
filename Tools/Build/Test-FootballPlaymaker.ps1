param([string]$PlayerPath,[switch]$RoomOnly,[switch]$OfflineOnly,[int]$Width=1600,[int]$Height=900,[int]$Port=7998)
$ErrorActionPreference='Stop'
if($RoomOnly -and $OfflineOnly){throw 'Choose RoomOnly or OfflineOnly.'}
$playmakerRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$playmakerPlayer=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $playmakerRoot 'Builds/FootballPlaymakerQA/Player/WhatTheFish.exe'}
$playmakerRun=Join-Path $playmakerRoot ('Builds/FootballPlaymakerQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
function Invoke-PlaymakerPlayers([string[]]$Modes){
 $running=@()
 try {
  foreach($mode in $Modes){
   $folder=Join-Path $playmakerRun $mode;New-Item -ItemType Directory -Force -Path $folder | Out-Null
   $options=@('-batchmode','-force-d3d11','-screen-width',"$Width",'-screen-height',"$Height",'-screen-fullscreen','0','-job-worker-count','2','-probe','-sport','Football','-footballPlaymakerReview',('"'+$folder+'"'),'-report',('"'+(Join-Path $folder 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
   if($mode -eq 'offline'){$options+='-offline'}else{$options+=@($(if($mode -eq 'host'){'-localHost'}else{'-localClient'}),'-expected','2','-startAt','5','-returnAt','300','-port',"$Port")}
   $p=Start-Process -FilePath $playmakerPlayer -ArgumentList $options -WindowStyle Hidden -PassThru;$running+=@{process=$p;folder=$folder}
   if($mode -eq 'host'){Start-Sleep -Seconds 2}
  }
  foreach($item in $running){if(!$item.process.WaitForExit(180000)){throw "Playmaker review timed out: $($item.folder)"}}
  foreach($item in $running){
   $result=Get-Content -LiteralPath (Join-Path $item.folder 'results.txt') -Raw
   if($result -match '(?m)^FAIL ' -or !$result.Contains('FOOTBALL_PLAYMAKER_COMPLETE success=True') -or $item.process.ExitCode -ne 0){throw "Playmaker review failed: $($item.folder)"}
   if(Select-String -LiteralPath (Join-Path $item.folder 'player.log') -Pattern 'Exception:|Assertion failed|Validated centre kickoff failed' -Quiet){throw "Runtime error: $($item.folder)"}
   Write-Output "PASS $($item.folder) assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count)"
  }
 }finally{foreach($item in $running){if(!$item.process.HasExited){Stop-Process -Id $item.process.Id}}}
}
if(!$RoomOnly){Invoke-PlaymakerPlayers @('offline')}
if(!$OfflineOnly){Invoke-PlaymakerPlayers @('host','guest')}
Write-Output "FOOTBALL_PLAYMAKER_SUITE_PASS $playmakerRun"
