param([switch]$OfflineOnly,[switch]$NetworkOnly,[string]$PlayerPath)
if($OfflineOnly -and $NetworkOnly){throw "Choose OfflineOnly or NetworkOnly"}
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskPlayer=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$taskOutput=Join-Path $taskRoot ('Builds/FootballPrototype/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
function Start-BallProbe([string]$name,[string]$arguments){
    $taskReport=Join-Path $taskOutput "$name.txt"
    if(Test-Path -LiteralPath $taskReport){throw "Archive previous evidence before rerunning: $taskReport"}
    $taskLog=Join-Path $taskOutput "$name.log"
    # The offline aim assertions inspect shader material state, which needs a graphics device.
    $taskGraphics=if($name -eq 'player-offline'){''}else{'-nographics'}
    Start-Process -FilePath $taskPlayer -WindowStyle Hidden -PassThru -ArgumentList "-batchmode $taskGraphics -job-worker-count 2 $arguments -report `"$taskReport`" -logFile `"$taskLog`""
}
$taskProcesses=@()
try {
if(!$NetworkOnly){$taskProcesses+=Start-BallProbe 'player-offline' '-offline -footballBallAudit -probe -exitAfter 90'}
if(!$OfflineOnly){
    $taskProcesses+=Start-BallProbe 'host' '-localHost -port 7794 -networkBallAudit -probe -expected 2 -startAt 12 -returnAt 230 -exitAfter 240'
    # A cold packaged launch prepares the embedded scene cache before opening the room.
    $taskDeadline=(Get-Date).AddSeconds(90)
    do {
        $taskHostReport=Join-Path $taskOutput 'host.txt'
        $taskListening=(Test-Path -LiteralPath $taskHostReport) -and (Select-String -LiteralPath $taskHostReport -Pattern 'count=1 connected=True' -Quiet)
        if(!$taskListening){Start-Sleep -Seconds 1}
    }while(!$taskListening -and (Get-Date) -lt $taskDeadline)
    if(!$taskListening){throw 'Host did not become ready for the client'}
    $taskProcesses+=Start-BallProbe 'client' '-localClient -port 7794 -networkBallAudit -probe -expected 2 -startAt 12 -returnAt 230 -exitAfter 240'
}
$taskNames=if($OfflineOnly){@('player-offline')}elseif($NetworkOnly){@('host','client')}else{@('player-offline','host','client')}
$taskDeadline=(Get-Date).AddSeconds(260)
do {
    $taskComplete=@($taskNames | Where-Object {
        $taskReport=Join-Path $taskOutput "$_.txt"
        $taskMarker=if($_ -eq 'player-offline'){'FOOTBALL_PHYSICS_COMPLETE'}else{'NETWORK_FOOTBALL_BALL_COMPLETE'}
        (Test-Path -LiteralPath $taskReport) -and (Select-String -LiteralPath $taskReport -SimpleMatch $taskMarker -Quiet)
    })
    if($taskComplete.Count -eq $taskNames.Count){break}
    Start-Sleep -Milliseconds 500
}while((Get-Date) -lt $taskDeadline)
if($taskComplete.Count -ne $taskNames.Count){throw "Football probe timed out: $taskOutput"}
foreach($taskName in $taskNames){
    $taskText=Get-Content -LiteralPath (Join-Path $taskOutput "$taskName.txt") -Raw
    $taskMarker=if($taskName -eq 'player-offline'){'FOOTBALL_PHYSICS_COMPLETE'}else{'NETWORK_FOOTBALL_BALL_COMPLETE'}
    if($taskText -match 'FAIL ' -or !$taskText.Contains($taskMarker)){throw "Football verification failed: $taskName"}
    if(Select-String -LiteralPath (Join-Path $taskOutput "$taskName.log") -Pattern 'Exception:|Assertion failed' -Quiet){throw "Football runtime diagnostic: $taskName"}
    Write-Output "PASS $taskName"
}
Write-Output "FOOTBALL_BALL_SUITE_PASS $taskOutput"
}finally{foreach($taskProcess in $taskProcesses){if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}};Write-Output "Evidence: $taskOutput"}
