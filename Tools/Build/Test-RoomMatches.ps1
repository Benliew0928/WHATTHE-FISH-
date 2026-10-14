param([switch]$Build,[string]$PlayerPath,[int]$Width=1600,[int]$Height=900,[switch]$OfflineOnly,[string]$UnityEditorPath=(Join-Path $env:ProgramFiles 'Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe'))
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$evidence=Join-Path $root 'Builds/RoomMatchQA'
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
if($Build){
 $log=Join-Path $evidence ('build-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.log')
 $p=Start-Process -FilePath $UnityEditorPath -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"'+(Join-Path $root 'Game')+'"'),'-buildTarget','Win64','-job-worker-count','4','-executeMethod','RoomMatchBuild.BuildReview','-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
 $p.WaitForExit();if($p.ExitCode -ne 0){throw "Room match review build failed: $log"}
}
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $evidence 'Player/WhatTheFish.exe'}
$run=Join-Path $evidence ('Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
function Run-Players([string]$Sport,[string[]]$Modes,[int]$Port){
 $running=@()
 try {
  foreach($mode in $Modes){
   $out=Join-Path $run "$Sport-$mode";New-Item -ItemType Directory -Force -Path $out | Out-Null
   $args=@('-batchmode','-force-d3d11','-screen-width',"$Width",'-screen-height',"$Height",'-screen-fullscreen','0','-job-worker-count','2','-probe','-sport',$Sport,'-roomMatchReview',('"'+$out+'"'),'-report',('"'+(Join-Path $out 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $out 'player.log')+'"'),'-exitAfter','160')
   if($mode -eq 'offline'){$args+='-offline'}else{$args+=@($(if($mode -eq 'host'){'-localHost'}else{'-localClient'}),'-expected','2','-startAt','5','-returnAt','155','-port',"$Port")}
   $process=Start-Process -FilePath $player -ArgumentList $args -WindowStyle Hidden -PassThru
   $running+=@{process=$process;folder=$out};if($mode -eq 'host'){Start-Sleep -Seconds 2}
  }
  foreach($item in $running){if(!$item.process.WaitForExit(180000)){throw "Room match review timed out: $($item.folder)"}}
  foreach($item in $running){
   $result=Join-Path $item.folder 'results.txt';$text=Get-Content -LiteralPath $result -Raw
   if($text -match '(?m)^FAIL ' -or !$text.Contains('ROOM_MATCH_COMPLETE success=True') -or $item.process.ExitCode -ne 0){throw "Room match checks failed: $($item.folder)"}
   if(Select-String -LiteralPath (Join-Path $item.folder 'player.log') -Pattern 'Exception:|Assertion failed|Look rotation viewing vector is zero' -Quiet){throw "Runtime diagnostic: $($item.folder)"}
   Write-Output "PASS $($item.folder) assertions=$(([regex]::Matches($text,'(?m)^PASS ')).Count)"
  }
 }finally{foreach($item in $running){if(!$item.process.HasExited){Stop-Process -Id $item.process.Id}}}
}
Run-Players 'Basketball' @('offline') 7992
Run-Players 'Football' @('offline') 7992
if(!$OfflineOnly){Run-Players 'Basketball' @('host','guest') 7992;Run-Players 'Football' @('host','guest') 7993}
Write-Output "ROOM_MATCH_SUITE_PASS $run"
