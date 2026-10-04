param([int]$Port=8204,[string]$PlayerPath,[switch]$Headless)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'}
$out=Join-Path $root ('Builds/GolfCartQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$processes=@()
function Start-CartProbe([string]$Name,[string[]]$Options){
 $report=Join-Path $out ($Name+'.txt');$log=Join-Path $out ($Name+'.log')
 Start-Process -FilePath $PlayerPath -ArgumentList ($Options+@('-probe','-report',('"'+$report+'"'),'-logFile',('"'+$log+'"'))) -WindowStyle Hidden -PassThru
}
try {
 $offline=@('-batchmode','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-offline','-sport','Golf','-cartAudit','-exitAfter','90')
 if($Headless){$offline+='-nographics'}
 $processes+=Start-CartProbe 'offline' $offline
 $network=@('-batchmode','-nographics','-job-worker-count','2','-sport','Golf','-networkCartAudit','-port',"$Port",'-expected','2','-startAt','10','-returnAt','60','-exitAfter','65')
 $processes+=Start-CartProbe 'host' ($network+'-localHost');$processes+=Start-CartProbe 'client' ($network+'-localClient')
 $deadline=(Get-Date).AddSeconds(180)
 while(($processes | Where-Object {-not $_.HasExited}) -and (Get-Date) -lt $deadline){Start-Sleep -Seconds 1}
 if($processes | Where-Object {-not $_.HasExited}){throw "Golf cart probes timed out: $out"}
 foreach($name in @('offline','host','client')){
  $report=Get-Content -LiteralPath (Join-Path $out ($name+'.txt')) -Raw
  $marker=if($name -eq 'offline'){'CART_OFFLINE_COMPLETE'}else{'CART_NETWORK_COMPLETE'}
  if($report -match '(?m)^FAIL ' -or !$report.Contains($marker) -or !$report.Contains('PROBE_EXIT')){throw "Failed or incomplete $name : $out"}
  $log=Get-Content -LiteralPath (Join-Path $out ($name+'.log')) -Raw
  if($log -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException)'){throw "Runtime exception in $name : $out"}
 }
 foreach($process in $processes){if($process.ExitCode -ne 0){throw "Player exit $($process.ExitCode): $out"}}
 Write-Output "PASS golf cart UI, driving/collision, parking, owner security, two-player replication and disconnect: $out"
} finally {foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id}}}
