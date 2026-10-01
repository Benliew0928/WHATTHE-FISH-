param([int]$Port=7806)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'
$out=Join-Path $root ('Builds/JumpQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$processes=@()
function Start-JumpProbe([string]$Name,[string]$Options){
    $report=Join-Path $out ($Name+'.txt')
    $log=Join-Path $out ($Name+'.log')
    Start-Process -FilePath $player -ArgumentList "$Options -probe -report `"$report`" -logFile `"$log`"" -WindowStyle Hidden -PassThru
}
try {
    $processes+=Start-JumpProbe 'gameplay' '-batchmode -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -jumpAudit -exitAfter 70'
    $network="-batchmode -nographics -job-worker-count 2 -networkJumpAudit -port $Port -expected 2 -startAt 12 -returnAt 25 -exitAfter 30"
    $processes+=Start-JumpProbe 'host' "$network -localHost"
    $processes+=Start-JumpProbe 'client' "$network -localClient"
    $deadline=(Get-Date).AddSeconds(240)
    while(($processes | Where-Object {-not $_.HasExited}) -and (Get-Date) -lt $deadline){Start-Sleep -Seconds 1}
    if($processes | Where-Object {-not $_.HasExited}){throw 'Jump probes timed out'}
    foreach($name in @('gameplay','host','client')){
        $report=Get-Content -LiteralPath (Join-Path $out ($name+'.txt')) -Raw
        if($report -match '(?m)^FAIL '){throw "Failed assertions in $name : $out"}
        $required=if($name -eq 'gameplay'){'JUMP_GAMEPLAY_COMPLETE'}else{'NETWORK_JUMP_COMPLETE'}
        if(-not $report.Contains($required) -or -not $report.Contains('PROBE_EXIT')){throw "Incomplete $name : $out"}
        $log=Get-Content -LiteralPath (Join-Path $out ($name+'.log')) -Raw
        if($log -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException)'){throw "Runtime exception in $name : $out"}
    }
    foreach($process in $processes){if($process.ExitCode -ne 0){throw "Player PID $($process.Id) exit $($process.ExitCode)"}}
    Write-Output "PASS jump gameplay and two-player checks: $out"
} finally {
    foreach($process in $processes){if(-not $process.HasExited){Stop-Process -Id $process.Id}}
}
