$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$player=Join-Path $root 'Builds\WindowsFinal\WhatTheFish.exe'
$runId=Get-Date -Format 'yyyyMMdd-HHmmss'
$out=Join-Path $root "Builds\CloudQA-$runId"
New-Item -ItemType Directory -Path $out | Out-Null
function Launch-Probe([string]$name,[string]$role,[int]$expected,[int]$seconds){
    Start-Process -FilePath $player -ArgumentList "-batchmode -nographics -job-worker-count 2 -probe $role -authProfile qa$runId$name -expected $expected -report $out\$name.txt -exitAfter $seconds -logFile $out\$name.log" -WindowStyle Hidden -PassThru | Select-Object Id
}
function Await-Report([string]$name,[string]$pattern,[int]$seconds=50){
    $deadline=(Get-Date).AddSeconds($seconds)
    do {
        $file=Join-Path $out "$name.txt"
        if((Test-Path -LiteralPath $file) -and (Select-String -LiteralPath $file -Pattern $pattern -Quiet)){return}
        Start-Sleep -Seconds 1
    }while((Get-Date) -lt $deadline)
    throw "Timed out waiting for $name / $pattern. Inspect $out"
}
# Expected 11 deliberately keeps Room A waiting so its capacity error can be distinguished from a locked room.
Launch-Probe 'Ahost' "-cloudHost -codeFile $out\Acode.txt" 11 140
Await-Report 'Ahost' 'CLOUD_ROOM code=[A-Z0-9]+ connected=True'
$codeA=(Get-Content "$out\Acode.txt" -Raw).Trim()
foreach($i in 1..9){Launch-Probe "Aclient$i" "-cloudCode $codeA" 10 150; Start-Sleep -Milliseconds 500}
Await-Report 'Ahost' 'count=10 '
Launch-Probe 'Aoverflow' "-cloudCode $codeA" 10 25
Launch-Probe 'Bhost' "-cloudHost -codeFile $out\Bcode.txt" 2 95
Await-Report 'Bhost' 'CLOUD_ROOM code=[A-Z0-9]+ connected=True'
$codeB=(Get-Content "$out\Bcode.txt" -Raw).Trim()
Launch-Probe 'Bclient' "-cloudCode $codeB" 2 130
Await-Report 'Bhost' 'count=2 '
Await-Report 'Bhost' 'exploring=True'
Launch-Probe 'Blocked' "-cloudCode $codeB" 2 20
Write-Output "Cloud test evidence: $out. Probes close automatically. These processes share one internet connection; different-network phone testing is separate."
