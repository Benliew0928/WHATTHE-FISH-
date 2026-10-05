param([string]$OutputName=('Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss')),[ValidateSet(20,30,60,120)][int]$Fps=30,[switch]$NoCapture,[string]$PlayerPath)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'}
$out=Join-Path $root ('Builds/BasketballStealAnimationQA/'+$OutputName)
New-Item -ItemType Directory -Force -Path $out | Out-Null
$arguments=@('-batchmode','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-probe','-sport','Basketball','-offline','-basketballChallengeReview',('"'+$out+'"'),'-report',('"'+(Join-Path $out 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $out 'player.log')+'"'),'-exitAfter','150','-motionRate',$Fps)
if($NoCapture){$arguments+=@('-nographics','-motionNoCapture')}
$process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
try {
 $deadline=(Get-Date).AddSeconds(170);$result=Join-Path $out 'results.txt'
 do {
  if((Test-Path -LiteralPath $result) -and (Select-String -LiteralPath $result -Pattern 'BASKETBALL_CHALLENGE_COMPLETE' -Quiet)){break}
  if($process.HasExited){throw 'Player exited before completing challenge animation checks.'}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $deadline)
 if(-not (Test-Path -LiteralPath $result)){throw 'No challenge animation results.'}
 Get-Content -LiteralPath $result
 if(-not (Select-String -LiteralPath $result -Pattern 'BASKETBALL_CHALLENGE_COMPLETE success=True' -Quiet)){throw "Challenge animation review failed or timed out: $out"}
 if(Select-String -LiteralPath (Join-Path $out 'player.log') -Pattern 'Exception:|Look rotation viewing vector is zero' -Quiet){throw 'Runtime pose exception.'}
 Write-Output "BASKETBALL_CHALLENGE_SUITE_PASS $out"
}finally{if(-not $process.HasExited){Stop-Process -Id $process.Id}}
