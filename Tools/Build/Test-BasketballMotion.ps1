param([string]$OutputName=('Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss')),[ValidateSet(20,30,60,120)][int]$Fps=30,[switch]$NoCapture)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'
$out=Join-Path $root ('Builds/BasketballMotionQA/'+$OutputName)
New-Item -ItemType Directory -Force -Path $out | Out-Null
$arguments=@('-batchmode','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-probe','-sport','Basketball','-offline','-basketballMotionReview',('"'+$out+'"'),'-report',('"'+(Join-Path $out 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $out 'player.log')+'"'),'-exitAfter','120')
$arguments+=@('-motionRate',$Fps)
if($NoCapture){$arguments+=@('-nographics','-motionNoCapture')}
$process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
try {
 $deadline=(Get-Date).AddSeconds(160);$result=Join-Path $out 'results.txt'
 do {
  if((Test-Path -LiteralPath $result) -and (Select-String -LiteralPath $result -Pattern 'BASKETBALL_MOTION_COMPLETE' -Quiet)){break}
  if($process.HasExited){throw 'Player exited before completing motion checks.'}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $deadline)
 if(-not (Test-Path -LiteralPath $result)){throw 'No motion results.'}
 Get-Content -LiteralPath $result
 if(-not (Select-String -LiteralPath $result -Pattern 'BASKETBALL_MOTION_COMPLETE success=True' -Quiet)){throw "Motion review failed or timed out: $out"}
 if(Select-String -LiteralPath (Join-Path $out 'player.log') -Pattern 'Exception:|Look rotation viewing vector is zero' -Quiet){throw 'Runtime pose exception.'}
 Write-Output "BASKETBALL_MOTION_SUITE_PASS $out"
}finally{if(-not $process.HasExited){Stop-Process -Id $process.Id}}
