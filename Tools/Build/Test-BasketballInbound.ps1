param([string]$PlayerPath,[int]$TimeoutSeconds=180,[switch]$HostSolo)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(-not $PlayerPath){$PlayerPath=Join-Path $root 'Builds/BasketballInboundQA/Player/WhatTheFish.exe'}
$role=if($HostSolo){'HostSolo'}else{'Offline'}
$out=Join-Path $root ('Builds/BasketballInboundQA/'+$role+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')+'-'+[Guid]::NewGuid().ToString('N').Substring(0,6))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$mode=if($HostSolo){@('-probe','-localHost','-expected','1','-startAt','4','-port','7957','-report',('"'+(Join-Path $out 'probe.jsonl')+'"'))}else{@('-offline')}
$arguments=@('-batchmode','-force-d3d11','-job-worker-count','2','-sport','Basketball','-basketballInboundReview',('"'+$out+'"'),'-screen-fullscreen','0','-screen-width','1600','-screen-height','900','-logFile',('"'+(Join-Path $out 'player.log')+'"'))+$mode
$p=Start-Process -FilePath $PlayerPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
$null=$p.Handle
if(-not $p.WaitForExit($TimeoutSeconds*1000)){$p.Kill();throw "Basketball inbound review timed out: $out"}
$p.Refresh()
$results=Get-Content -LiteralPath (Join-Path $out 'results.txt')
$results | Select-String 'FAIL|COMPLETE'
@{exitCode=$p.ExitCode;completedUtc=(Get-Date).ToUniversalTime().ToString('o');renderer='D3D11 batch capture';hostSolo=[bool]$HostSolo} | ConvertTo-Json | Set-Content (Join-Path $out 'exit.json')
if($p.ExitCode -ne 0 -or $results -match '^FAIL' -or -not ($results -match '^BASKETBALL_INBOUND_COMPLETE success=True')){throw "Basketball inbound review failed (exit $($p.ExitCode)): $out"}
if(Select-String -LiteralPath (Join-Path $out 'player.log') -Pattern 'Exception:|Failed to capture' -Quiet){throw "Basketball inbound runtime/capture failure: $out"}
Write-Output "BASKETBALL_INBOUND_REVIEW_OK: $out"
