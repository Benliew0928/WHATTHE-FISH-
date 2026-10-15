param([string]$PlayerPath,[int]$Width=1280,[int]$Height=720)
$ErrorActionPreference='Stop'
$teleportRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$teleportPlayer=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $teleportRoot 'Builds/TeleportQA/Player/WhatTheFish.exe'}
$teleportRun=Join-Path $teleportRoot ('Builds/TeleportQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force -Path $teleportRun | Out-Null
$teleportArgs=@('-batchmode','-force-d3d11','-screen-width',"$Width",'-screen-height',"$Height",'-screen-fullscreen','0','-job-worker-count','2','-offline','-teleportPreview','-teleportReview',('"'+$teleportRun+'"'),'-logFile',('"'+(Join-Path $teleportRun 'player.log')+'"'))
$teleportProcess=Start-Process -FilePath $teleportPlayer -ArgumentList $teleportArgs -WindowStyle Hidden -PassThru
try {
 if(!$teleportProcess.WaitForExit(600000)){throw 'Teleport review timed out.'}
 $teleportResult=Get-Content -LiteralPath (Join-Path $teleportRun 'results.txt') -Raw
 if($teleportProcess.ExitCode -ne 0 -or $teleportResult -match '(?m)^FAIL ' -or !$teleportResult.Contains('TELEPORT_REVIEW_COMPLETE success=True')){throw "Teleport review failed: $teleportRun"}
 if(Select-String -LiteralPath (Join-Path $teleportRun 'player.log') -Pattern 'Exception:|Assertion failed|Shader error' -Quiet){throw "Runtime error: $teleportRun"}
 Write-Output "TELEPORT_SUITE_PASS $teleportRun assertions=$(([regex]::Matches($teleportResult,'(?m)^PASS ')).Count)"
}finally{if(!$teleportProcess.HasExited){Stop-Process -Id $teleportProcess.Id}}
