param([string]$PlayerPath,[int]$Width=1280,[int]$Height=720)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'}
$output=Join-Path $root ('Builds/BroadcastCameraQA/Reviews/'+$Width+'x'+$Height+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force $output | Out-Null
$arguments=@('-batchmode','-force-d3d11','-screen-fullscreen','0','-screen-width',$Width,'-screen-height',$Height,'-job-worker-count','2','-stadiumCameraReview',('"'+$output+'"'),'-logFile',('"'+(Join-Path $output 'player.log')+'"'))
$process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
try {
 if(!$process.WaitForExit(300000)){throw "Camera review timed out: $output"}
 $result=Join-Path $output 'results.txt'
 if(!(Test-Path -LiteralPath $result)){throw "Camera report missing: $output"}
 $checks=Get-Content -LiteralPath $result
 Write-Output ('Camera assertions: '+@($checks | Where-Object {$_ -like 'PASS *'}).Count)
 $checks | Where-Object {$_ -like 'FAIL *' -or $_ -like 'STADIUM_CAMERA_COMPLETE*'} | Write-Output
 if($process.ExitCode -ne 0 -or !(Select-String -LiteralPath $result -Pattern 'STADIUM_CAMERA_COMPLETE success=True' -Quiet) -or (Select-String -LiteralPath $result -Pattern '^FAIL' -Quiet)){throw "Camera review failed: $output"}
 if(Select-String -LiteralPath (Join-Path $output 'player.log') -Pattern 'Exception:|Look rotation viewing vector is zero' -Quiet){throw "Player exception: $output"}
 Write-Output "STADIUM_CAMERA_SUITE_PASS $output"
}finally{if(!$process.HasExited){Stop-Process -Id $process.Id};Write-Output "Evidence: $output"}
