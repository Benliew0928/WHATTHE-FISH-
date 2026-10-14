param([switch]$Baseline,[string]$PlayerPath,[string]$UnityEditorPath=(Join-Path $env:ProgramFiles 'Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe'),[switch]$Build)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$evidence=Join-Path $root 'Builds/NetworkMovementQA'
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
if($Build){
 $log=Join-Path $evidence ('windows-build-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.log')
 $p=Start-Process -FilePath $UnityEditorPath -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"'+(Join-Path $root 'Game')+'"'),'-buildTarget','Win64','-job-worker-count','4','-executeMethod','NetworkMovementBuild.BuildReview','-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
 $p.WaitForExit();if($p.ExitCode -ne 0){throw "Movement review build failed: $log"}
}
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $evidence 'Player/WhatTheFish.exe'}
$run=Join-Path $evidence ($(if($Baseline){'Baseline-'}else{'Fixed-'})+(Get-Date -Format 'yyyyMMdd-HHmmss'))
foreach($sport in @('Football','Basketball')){foreach($mode in @('offline','host')){
 $out=Join-Path $run "$sport-$mode";New-Item -ItemType Directory -Force -Path $out | Out-Null
 $args=@('-batchmode','-force-d3d11','-screen-width','960','-screen-height','540','-screen-fullscreen','0','-job-worker-count','2','-probe','-sport',$sport,'-movementReview',('"'+$out+'"'),'-report',('"'+(Join-Path $out probe.txt)+'"'),'-logFile',('"'+(Join-Path $out player.log)+'"'),'-exitAfter','120')
 if($Baseline){$args+='-movementBaseline'}
 if($mode -eq 'offline'){$args+='-offline'}else{$args+=@('-localHost','-expected','1','-startAt','3','-returnAt','115','-port','7989')}
 $p=Start-Process -FilePath $player -ArgumentList $args -WindowStyle Hidden -PassThru
 try {if(!$p.WaitForExit(120000)){throw "Movement review timed out: $out"};$result=Join-Path $out results.txt;if(!(Test-Path $result)){throw "No movement result: $out"};Get-Content $result;if($p.ExitCode -ne 0 -or !(Select-String -LiteralPath $result -Pattern 'MOVEMENT_REVIEW_COMPLETE success=True' -Quiet)){throw "Movement failed: $out"};if(Select-String -LiteralPath (Join-Path $out player.log) -Pattern 'Exception:|Assertion failed|Look rotation viewing vector is zero' -Quiet){throw "Runtime diagnostic: $out"}}
 finally {if(!$p.HasExited){Stop-Process -Id $p.Id}}
}}
Write-Output "MOVEMENT_SUITE_PASS $run"
