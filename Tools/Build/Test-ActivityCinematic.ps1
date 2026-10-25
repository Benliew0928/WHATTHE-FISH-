param([ValidateSet('Football','Basketball','Fishing','Golf')][string]$Sport='Fishing',[string]$PlayerPath,[switch]$Film,[int]$Width=1280,[int]$Height=720)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){if([IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath}else{Join-Path $root $PlayerPath}}else{Join-Path $root 'Builds/ActivityCinematicsQA/Player/WhatTheFish.exe'}
$run=Join-Path $root ('Builds/ActivityCinematicsQA/'+$Sport+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force -Path $run | Out-Null
$arguments=@('-batchmode','-force-d3d11','-screen-width',$Width,'-screen-height',$Height,'-screen-fullscreen','0','-job-worker-count','2','-sport',$Sport,'-offline','-guestProfile',(Split-Path $run -Leaf),'-activityCinematicReview',('"'+$run+'"'),'-logFile',('"'+(Join-Path $run 'player.log')+'"'))
if($Film){$arguments+='-cinematicFilm'}
$process=Start-Process -FilePath $player -ArgumentList $arguments -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
$null=$process.Handle
try{
 if(!$process.WaitForExit(600000)){throw "Cinematic review timed out: $run"}
 $result=Get-Content -LiteralPath (Join-Path $run 'results.txt') -Raw
 if($process.ExitCode -ne 0 -or $result -match '(?m)^FAIL ' -or !$result.Contains('ACTIVITY_CINEMATIC_COMPLETE success=True')){throw "Cinematic review failed: $run"}
 if(Select-String -LiteralPath (Join-Path $run 'player.log') -Pattern 'Exception:|Assertion failed|Shader error|Look rotation viewing vector is zero' -Quiet){throw "Runtime error: $run"}
 Write-Output "ACTIVITY_CINEMATIC_PASS $run assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count)"
}finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
