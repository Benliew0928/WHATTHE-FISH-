param([string]$PlayerPath,[switch]$CaptureFrames,[int]$TimeoutSeconds=240)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $root 'Builds/FootballCelebrationQA/Player/WhatTheFish.exe'}
$out=Join-Path $root ('Builds/FootballCelebrationQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$options=@('-batchmode','-force-d3d11','-job-worker-count','2','-offline','-sport','Football','-footballCelebrationReview',('"'+$out+'"'),'-screen-fullscreen','0','-screen-width','1600','-screen-height','900','-logFile',('"'+(Join-Path $out 'player.log')+'"'))
if($CaptureFrames){$options+='-celebrationFrames'}
$process=Start-Process -FilePath $PlayerPath -ArgumentList $options -WindowStyle Hidden -PassThru
$null=$process.Handle
if(!$process.WaitForExit($TimeoutSeconds*1000)){$process.Kill();throw "Celebration review timed out: $out"}
$process.Refresh()
$results=Get-Content -LiteralPath (Join-Path $out 'results.txt')
$results | Select-String 'FAIL|COMPLETE'
if($process.ExitCode -ne 0 -or $results -match '^FAIL' -or !($results -match '^FOOTBALL_CELEBRATION_COMPLETE success=True')){throw "Celebration review failed: $out"}
if(Select-String -LiteralPath (Join-Path $out 'player.log') -Pattern 'Exception:|Failed to capture|Look rotation viewing vector is zero' -Quiet){throw "Runtime error: $out"}
Write-Output "FOOTBALL_CELEBRATION_REVIEW_OK: $out"
