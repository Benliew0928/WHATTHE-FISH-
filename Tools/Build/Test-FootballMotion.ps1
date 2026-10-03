param([ValidateSet(20,30,60,120)][int]$Fps=30,[switch]$NoCapture,[string]$PlayerPath)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$taskOutput=Join-Path $taskRoot ('Builds/FootballMotionQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $taskOutput | Out-Null
$taskArgs=@('-batchmode','-screen-width','960','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-probe','-offline','-footballMotionReview',('"'+$taskOutput+'"'),'-motionRate',$Fps,'-report',('"'+(Join-Path $taskOutput 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $taskOutput 'player.log')+'"'),'-exitAfter','180')
if($NoCapture){$taskArgs+=@('-nographics','-motionNoCapture')}
$taskProcess=Start-Process -FilePath $PlayerPath -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
try {
 $taskDeadline=(Get-Date).AddSeconds(240);$taskResults=Join-Path $taskOutput 'results.txt'
 do {
  if((Test-Path -LiteralPath $taskResults) -and (Select-String -LiteralPath $taskResults -Pattern 'FOOTBALL_MOTION_COMPLETE' -Quiet)){break}
  if($taskProcess.HasExited){throw 'Player exited before completing football motion review.'}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $taskDeadline)
 Get-Content -LiteralPath $taskResults
 if(!(Select-String -LiteralPath $taskResults -Pattern 'FOOTBALL_MOTION_COMPLETE success=True' -Quiet)){throw "Football motion review failed: $taskOutput"}
 if(Select-String -LiteralPath (Join-Path $taskOutput 'player.log') -Pattern 'Exception:|Look rotation viewing vector is zero' -Quiet){throw 'Runtime pose exception.'}
 Write-Output "FOOTBALL_MOTION_SUITE_PASS $taskOutput"
}finally{if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}
