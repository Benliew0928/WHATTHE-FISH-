param([string]$OutputName=('Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss')),[string]$PlayerPath)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'}
$out=Join-Path $root ('Builds/BasketballPhysicsQA/'+$OutputName)
New-Item -ItemType Directory -Path $out -Force | Out-Null
$arguments=@('-batchmode','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-probe','-sport','Basketball','-offline','-basketballPhysicsReview',('"'+$out+'"'),'-report',('"'+(Join-Path $out 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $out 'player.log')+'"'),'-exitAfter','160')
$process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
try {
 $deadline=(Get-Date).AddSeconds(170);$result=Join-Path $out 'results.txt'
 do {
  if((Test-Path -LiteralPath $result) -and (Select-String -LiteralPath $result -Pattern 'BASKETBALL_PHYSICS_COMPLETE' -Quiet)){break}
  if($process.HasExited){throw 'Player exited before completing basketball physics checks.'}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $deadline)
 if(-not (Test-Path -LiteralPath $result)){throw 'No basketball physics results.'}
 Get-Content -LiteralPath $result
 if(-not (Select-String -LiteralPath $result -Pattern 'BASKETBALL_PHYSICS_COMPLETE success=True' -Quiet)){throw "Physics review failed or timed out: $out"}
 if(Select-String -LiteralPath (Join-Path $out 'player.log') -Pattern 'Exception:|Look rotation viewing vector is zero' -Quiet){throw 'Basketball runtime exception.'}
 Write-Output "BASKETBALL_PHYSICS_SUITE_PASS $out"
}finally{if(-not $process.HasExited){Stop-Process -Id $process.Id}}
