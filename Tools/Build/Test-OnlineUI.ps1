param([string]$PlayerPath)
$ErrorActionPreference='Stop'
$repoRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $repoRoot 'Builds/OnlineUIQA/Player/WhatTheFish.exe'}
$run=Join-Path $repoRoot ('Builds/OnlineUIQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $run -Force | Out-Null
$arguments=@('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-onlineUIReview',('"'+$run+'"'),'-contentCache',('"'+(Join-Path $run 'ContentCache')+'"'),'-logFile',('"'+(Join-Path $run 'player.log')+'"'))
$process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
try {
 $deadline=(Get-Date).AddMinutes(8)
 while(!$process.HasExited -and (Get-Date) -lt $deadline){Start-Sleep -Seconds 2}
 $result=Get-Content -LiteralPath (Join-Path $run 'results.txt') -Raw
 if($result -match '(?m)^FAIL ' -or !$result.Contains('ONLINE_UI_REVIEW_COMPLETE success=True')){throw "Online UI review failed or incomplete: $run"}
 if(Select-String -LiteralPath (Join-Path $run 'player.log') -Pattern 'Exception:|Assertion failed|Shader error' -Quiet){throw "Runtime error: $run"}
 if(!$process.HasExited -or $process.ExitCode -ne 0){throw "Review player did not exit successfully: $run"}
 Write-Output "ONLINE_UI_PASS assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count) $run"
}finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
