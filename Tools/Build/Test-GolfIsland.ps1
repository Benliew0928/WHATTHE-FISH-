param([switch]$Headless)
$ErrorActionPreference='Stop'
$root=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$out=Join-Path $root ('Builds\GolfTidebloomQA-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $out | Out-Null
$argsList=@('-batchmode','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-probe','-sport','Golf','-golfAudit','-report',"$out\island.txt",'-exitAfter','24','-logFile',"$out\player.log")
if($Headless){$argsList+='-nographics'}
$p=Start-Process -FilePath (Join-Path $root 'Builds\WindowsFinal\SportsPrototype.exe') -ArgumentList $argsList -WindowStyle Hidden -PassThru
$null=$p.Handle
try {
 if(!$p.WaitForExit(90000)){throw 'Golf audit exceeded 90 seconds'}
 $report=Get-Content -LiteralPath "$out\island.txt" -Raw
 if($p.ExitCode -ne 0 -or $report -match '(?m)^FAIL ' -or $report -notmatch 'GOLF_AUDIT_COMPLETE'){throw "Golf audit failed. Evidence: $out"}
 if(Select-String -LiteralPath "$out\player.log" -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException' -Quiet){throw "Runtime exception. Evidence: $out"}
 Write-Output "PASS Golf source integration, ten spawns, 48 shoreline directions, fairway, bunkers, camera and environment switching. Evidence: $out"
} finally {
 if(!$p.HasExited){Stop-Process -Id $p.Id -ErrorAction SilentlyContinue}
}
