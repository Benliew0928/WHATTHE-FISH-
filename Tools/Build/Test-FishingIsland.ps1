param([switch]$Headless)
$ErrorActionPreference='Stop'
$root=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$out=Join-Path $root ('Builds\FishingQA-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $out | Out-Null
$arguments=@('-batchmode','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-probe','-sport','Fishing','-fishingAudit','-report',"$out\fishing.txt",'-exitAfter','30','-logFile',"$out\player.log")
if($Headless){$arguments+='-nographics'}
$process=Start-Process -FilePath (Join-Path $root 'Builds\WindowsFinal\WhatTheFish.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru
$null=$process.Handle
try {
 if(!$process.WaitForExit(90000)){throw "Fishing audit exceeded 90 seconds: $out"}
 $report=Get-Content -LiteralPath "$out\fishing.txt" -Raw
 if($process.ExitCode -ne 0 -or $report -match '(?m)^FAIL ' -or $report -notmatch 'FISHING_AUDIT_COMPLETE'){throw "Fishing audit failed: $out"}
 if(Select-String -LiteralPath "$out\player.log" -Pattern 'Exception:|Shader error|error CS' -Quiet){throw "Runtime error: $out"}
 Write-Output "PASS five deck entrances/exits, bridge loop, water containment, independent modules, material accents and all four sport switches. Evidence: $out"
} finally {
 if(!$process.HasExited){Stop-Process -Id $process.Id -ErrorAction SilentlyContinue}
}
