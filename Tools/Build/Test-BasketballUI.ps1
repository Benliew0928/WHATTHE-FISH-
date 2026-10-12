param([string]$PlayerPath,[int]$TimeoutSeconds=180)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(-not $PlayerPath){$PlayerPath=Join-Path $root 'Builds/BasketballUIQA/Player/WhatTheFish.exe'}
$out=Join-Path $root ('Builds/BasketballUIQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$p=Start-Process -FilePath $PlayerPath -ArgumentList @('-batchmode','-force-d3d11','-job-worker-count','2','-sport','Basketball','-offline','-basketballUIReview',('"'+$out+'"'),'-screen-fullscreen','0','-screen-width','1600','-screen-height','900','-logFile',('"'+(Join-Path $out 'player.log')+'"')) -WindowStyle Hidden -PassThru
$null=$p.Handle
if(-not $p.WaitForExit($TimeoutSeconds*1000)){$p.Kill();throw "Basketball UI review timed out: $out"}
$p.Refresh()
$results=Get-Content -LiteralPath (Join-Path $out 'results.txt')
$results | Select-String 'FAIL|COMPLETE'
@{exitCode=$p.ExitCode;completedUtc=(Get-Date).ToUniversalTime().ToString('o');renderer='D3D11 batch capture'} | ConvertTo-Json | Set-Content (Join-Path $out 'exit.json')
if($p.ExitCode -ne 0 -or $results -match '^FAIL' -or -not ($results -match '^BASKETBALL_UI_COMPLETE success=True')){throw "Basketball UI review failed (exit $($p.ExitCode)): $out"}
if(Select-String -LiteralPath (Join-Path $out 'player.log') -Pattern 'Exception:|Failed to capture' -Quiet){throw "Basketball UI runtime/capture failure: $out"}
Write-Output "BASKETBALL_UI_REVIEW_OK: $out"
