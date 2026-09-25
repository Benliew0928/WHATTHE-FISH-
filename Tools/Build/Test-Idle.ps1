param([int]$Port=7794)
$ErrorActionPreference='Stop'
$root=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$player=Join-Path $root 'Builds/WindowsFinal/SportsPrototype.exe'
$out=Join-Path $root ('Builds/IdleQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$processes=@()
function Start-IdleProbe([string]$Name,[string]$Options){
    $report=Join-Path $out ($Name+'.txt')
    $log=Join-Path $out ($Name+'.log')
    Start-Process -FilePath $player -ArgumentList "$Options -probe -report `"$report`" -logFile `"$log`"" -WindowStyle Hidden -PassThru
}
try {
    # Batch mode retains graphics/camera renders without creating an interactive
    # game window. Hidden non-batch players crash in Unity's GPU profiler teardown
    # on this installed editor/runtime; headless network probes are unaffected.
    $processes+=Start-IdleProbe 'gameplay' '-batchmode -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -idleAudit -exitAfter 65'
    $network="-batchmode -nographics -job-worker-count 2 -networkIdleAudit -port $Port -expected 2 -startAt 18 -returnAt 36 -exitAfter 48"
    $processes+=Start-IdleProbe 'host' "$network -localHost"
    $processes+=Start-IdleProbe 'client' "$network -localClient"
    $deadline=(Get-Date).AddSeconds(100)
    while(($processes | Where-Object {-not $_.HasExited}) -and (Get-Date) -lt $deadline){Start-Sleep -Seconds 1}
    if($processes | Where-Object {-not $_.HasExited}){throw 'Idle probes timed out'}
    foreach($name in @('gameplay','host','client')){
        $report=Get-Content -LiteralPath (Join-Path $out ($name+'.txt')) -Raw
        if($report -match '(?m)^FAIL '){throw "Failed assertions in $name"}
        $required=if($name -eq 'gameplay'){'IDLE_AUDIT_COMPLETE'}else{'NETWORK_IDLE_AUDIT_COMPLETE'}
        if(-not $report.Contains($required)){throw "Missing completion marker in $name"}
        if(-not $report.Contains('PROBE_EXIT')){throw "Missing clean exit in $name"}
    }
    foreach($process in $processes){if($process.ExitCode -ne 0){throw "Player PID $($process.Id) exit $($process.ExitCode)"}}
    Write-Output "PASS idle gameplay and two-player checks: $out"
} finally {
    foreach($process in $processes){if(-not $process.HasExited){Stop-Process -Id $process.Id}}
}
