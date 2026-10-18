param([string]$PlayerPath)
$ErrorActionPreference='Stop'
$teardownRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$teardownPlayer=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $teardownRoot 'Builds/StadiumTeardownQA/Player/WhatTheFish.exe'}
$teardownRun=Join-Path $teardownRoot ('Builds/StadiumTeardownQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force -Path $teardownRun | Out-Null
$teardownArguments=@('-batchmode','-force-d3d11','-job-worker-count','2','-offline','-sport','Football','-stadiumTeardownReview',('"'+$teardownRun+'"'),'-screen-fullscreen','0','-screen-width','844','-screen-height','390','-logFile',('"'+(Join-Path $teardownRun 'player.log')+'"'))
$teardownArguments+=@('-contentCache',('"'+(Join-Path $teardownRun 'ContentCache')+'"'))
$teardownProcess=Start-Process -FilePath $teardownPlayer -ArgumentList $teardownArguments -WindowStyle Hidden -PassThru
try {
 if(!$teardownProcess.WaitForExit(180000)){throw "Stadium teardown timed out: $teardownRun"}
 $teardownResult=Get-Content -LiteralPath (Join-Path $teardownRun 'results.txt') -Raw
 if($teardownProcess.ExitCode -ne 0 -or $teardownResult -match '(?m)^FAIL ' -or !$teardownResult.Contains('STADIUM_TEARDOWN_COMPLETE success=True')){throw "Stadium teardown failed: $teardownRun"}
 if(Select-String -LiteralPath (Join-Path $teardownRun 'player.log') -Pattern 'GameObjects can not be made active|being destroyed|Exception:|Assertion failed|Shader error' -Quiet){throw "Stadium teardown runtime diagnostic: $teardownRun"}
 Write-Output "STADIUM_TEARDOWN_PASS $teardownRun assertions=$(([regex]::Matches($teardownResult,'(?m)^PASS ')).Count)"
}finally{if(!$teardownProcess.HasExited){Stop-Process -Id $teardownProcess.Id}}
