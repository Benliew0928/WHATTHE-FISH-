param([int]$Port=7796,[switch]$Video)
$ErrorActionPreference='Stop'
$root=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$player=Join-Path $root 'Builds/WindowsFinal/SportsPrototype.exe'
$out=Join-Path $root ('Builds/TurnQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$processes=@()
function Start-TurnProbe([string]$Name,[string]$Options){
    $report=Join-Path $out ($Name+'.txt')
    $log=Join-Path $out ($Name+'.log')
    Start-Process -FilePath $player -ArgumentList "$Options -probe -report `"$report`" -logFile `"$log`"" -WindowStyle Hidden -PassThru
}
try {
    $videoOption=if($Video){'-turnVideo'}else{''}
    $processes+=Start-TurnProbe 'gameplay' "-batchmode -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -turnAudit $videoOption -exitAfter 50"
    $network="-batchmode -nographics -job-worker-count 2 -networkTurnAudit -port $Port -expected 2 -startAt 15 -returnAt 32 -exitAfter 37"
    $processes+=Start-TurnProbe 'host' "$network -localHost"
    $processes+=Start-TurnProbe 'client' "$network -localClient"
    $deadline=(Get-Date).AddSeconds(240)
    while(($processes | Where-Object {-not $_.HasExited}) -and (Get-Date) -lt $deadline){Start-Sleep -Seconds 1}
    if($processes | Where-Object {-not $_.HasExited}){throw 'Turn probes timed out'}
    foreach($name in @('gameplay','host','client')){
        $report=Get-Content -LiteralPath (Join-Path $out ($name+'.txt')) -Raw
        if($report -match '(?m)^FAIL '){throw "Failed assertions in $name : $out"}
        $required=if($name -eq 'gameplay'){'TURN_GAMEPLAY_COMPLETE'}else{'NETWORK_TURN_AUDIT_COMPLETE'}
        if(-not $report.Contains($required)){throw "Missing completion marker in $name : $out"}
        if(-not $report.Contains('PROBE_EXIT')){throw "Missing clean exit in $name : $out"}
    }
    foreach($process in $processes){if($process.ExitCode -ne 0){throw "Player PID $($process.Id) exit $($process.ExitCode)"}}
    Write-Output "PASS turn gameplay and two-player checks: $out"
} finally {
    foreach($process in $processes){if(-not $process.HasExited){Stop-Process -Id $process.Id}}
}
