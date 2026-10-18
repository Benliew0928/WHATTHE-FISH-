param([string]$PlayerPath,[switch]$CaptureFrames)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $root 'Builds/FootballStadiumEffectsQA/Player/WhatTheFish.exe'}
$out=Join-Path $root ('Builds/FootballStadiumEffectsQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$options=@('-batchmode','-force-d3d11','-job-worker-count','2','-offline','-sport','Football','-stadiumEffectsReview',('"'+$out+'"'),'-screen-fullscreen','0','-screen-width','1600','-screen-height','900','-logFile',('"'+(Join-Path $out 'player.log')+'"'))
if($CaptureFrames){$options+='-effectsFrames'}
$process=Start-Process -FilePath $PlayerPath -ArgumentList $options -WindowStyle Hidden -PassThru
try {
 $null=$process.Handle
 if(!$process.WaitForExit(180000)){throw "Effects review timed out: $out"}
 $process.Refresh();$results=Get-Content -LiteralPath (Join-Path $out 'results.txt')
 if($process.ExitCode -ne 0 -or $results -match '^FAIL' -or !($results -match '^STADIUM_EFFECTS_COMPLETE success=True')){throw "Effects review failed: $out"}
 if(Select-String -LiteralPath (Join-Path $out 'player.log') -Pattern 'Exception:|Shader error|Assertion failed|Look rotation viewing vector is zero' -Quiet){throw "Runtime error: $out"}
 Write-Output "STADIUM_EFFECTS_REVIEW_OK: $out checks=$(@($results -match '^PASS').Count)"
}finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
