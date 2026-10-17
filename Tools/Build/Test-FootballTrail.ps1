param([string]$PlayerPath,[int]$Width=1280,[int]$Height=720)
$ErrorActionPreference='Stop'
$trailRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$trailPlayer=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $trailRoot 'Builds/FootballTrailQA/Player/WhatTheFish.exe'}
$trailRun=Join-Path $trailRoot ('Builds/FootballTrailQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force -Path $trailRun | Out-Null
$trailArgs=@('-batchmode','-force-d3d11','-screen-width',"$Width",'-screen-height',"$Height",'-screen-fullscreen','0','-job-worker-count','2','-probe','-sport','Football','-offline','-footballTrailReview',('"'+$trailRun+'"'),'-report',('"'+(Join-Path $trailRun 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $trailRun 'player.log')+'"'))
$trailProcess=Start-Process -FilePath $trailPlayer -ArgumentList $trailArgs -WindowStyle Hidden -PassThru
try {
 if(!$trailProcess.WaitForExit(150000)){throw 'Football trail review timed out.'}
 $trailResult=Get-Content -LiteralPath (Join-Path $trailRun 'results.txt') -Raw
 if($trailProcess.ExitCode -ne 0 -or $trailResult -match '(?m)^FAIL ' -or !$trailResult.Contains('FOOTBALL_TRAIL_COMPLETE success=True')){throw "Football trail review failed: $trailRun"}
 if(Select-String -LiteralPath (Join-Path $trailRun 'player.log') -Pattern 'Exception:|Assertion failed|Shader error' -Quiet){throw "Runtime error: $trailRun"}
 Write-Output "FOOTBALL_TRAIL_SUITE_PASS $trailRun assertions=$(([regex]::Matches($trailResult,'(?m)^PASS ')).Count)"
}finally{if(!$trailProcess.HasExited){Stop-Process -Id $trailProcess.Id}}
