param([switch]$OfflineOnly,[switch]$NetworkOnly)
if($OfflineOnly -and $NetworkOnly){throw "Choose OfflineOnly or NetworkOnly"}
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskPlayer=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'
$taskOutput=Join-Path $taskRoot 'Builds/FootballPrototype'
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
if(!$NetworkOnly){$taskProcesses+=Start-BallProbe 'player-offline' '-offline -footballBallAudit -probe -exitAfter 90'}
if(!$OfflineOnly){
    $taskProcesses+=Start-BallProbe 'host' '-localHost -port 7794 -networkBallAudit -probe -expected 2 -startAt 12 -returnAt 40 -exitAfter 45'
    $taskDeadline=(Get-Date).AddSeconds(20)
    do {
        $taskHostReport=Join-Path $taskOutput 'host.txt'
        $taskListening=(Test-Path -LiteralPath $taskHostReport) -and (Select-String -LiteralPath $taskHostReport -Pattern 'count=1 connected=True' -Quiet)
        if(!$taskListening){Start-Sleep -Seconds 1}
    }while(!$taskListening -and (Get-Date) -lt $taskDeadline)
    if(!$taskListening){throw 'Host did not become ready for the client'}
    $taskProcesses+=Start-BallProbe 'client' '-localClient -port 7794 -networkBallAudit -probe -expected 2 -startAt 12 -returnAt 40 -exitAfter 45'
}
foreach($taskProcess in $taskProcesses){if(!$taskProcess.WaitForExit(100000)){throw "Football probe did not finish: $($taskProcess.Id)"}}
foreach($taskName in $(if($OfflineOnly){@('player-offline')}elseif($NetworkOnly){@('host','client')}else{@('player-offline','host','client')})){
    $taskText=Get-Content -LiteralPath (Join-Path $taskOutput "$taskName.txt") -Raw
    $taskMarker=if($taskName -eq 'player-offline'){'FOOTBALL_PHYSICS_COMPLETE'}else{'NETWORK_FOOTBALL_BALL_COMPLETE'}
    if($taskText -match 'FAIL ' -or !$taskText.Contains($taskMarker)){throw "Football verification failed: $taskName"}
    Write-Output "PASS $taskName"
}
