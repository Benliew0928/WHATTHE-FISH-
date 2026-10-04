param([string]$PlayerPath, [switch]$Render)

$ErrorActionPreference = 'Stop'
$taskRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (!$PlayerPath) { $PlayerPath = Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe' }
$taskOutput = Join-Path $taskRoot ('Builds/GolfBallPhysicsQA/Run-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskOutput | Out-Null
$taskReport = Join-Path $taskOutput 'physics.txt'
$taskLog = Join-Path $taskOutput 'physics.log'
$taskGraphics = if ($Render) { '-batchmode -force-d3d11 -screen-width 1600 -screen-height 900 -screen-fullscreen 0' } else { '-batchmode -nographics' }
$taskArgs = "$taskGraphics -job-worker-count 2 -sport Golf -offline -probe -golfBallPhysicsAudit -exitAfter 120 -report `"$taskReport`" -logFile `"$taskLog`""
$taskProcess = Start-Process -FilePath $PlayerPath -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
try {
    $taskDeadline = (Get-Date).AddSeconds(150)
    do {
        if (Test-Path -LiteralPath $taskReport) {
            $taskText = Get-Content -LiteralPath $taskReport -Raw
            if ($taskText -match '(?m)^FAIL ' -or $taskText.Contains('GOLF_BALL_PHYSICS_COMPLETE')) { break }
        }
        if ($taskProcess.HasExited) { break }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $taskDeadline)
    $taskText = Get-Content -LiteralPath $taskReport -Raw
    if ($taskText -match '(?m)^FAIL ' -or !$taskText.Contains('GOLF_BALL_PHYSICS_COMPLETE')) { throw "Failed or incomplete golf ball physics checks: $taskOutput" }
    $taskLogText = Get-Content -LiteralPath $taskLog -Raw
    if ($taskLogText -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException)') { throw "Runtime exception: $taskOutput" }
    Write-Output "PASS golf ball visibility, body isolation, rolling, slopes and swings; assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count): $taskOutput"
} finally {
    if (!$taskProcess.HasExited) { Stop-Process -Id $taskProcess.Id }
}
