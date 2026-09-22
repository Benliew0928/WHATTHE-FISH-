$ErrorActionPreference='Stop'
$player='C:\UMPSA\Builds\WindowsFinal\SportsPrototype.exe'
$out='C:\UMPSA\Builds\NetworkQA'
New-Item -ItemType Directory -Force -Path $out | Out-Null
function Launch-Probe([string]$name,[string]$role,[int]$port,[int]$expected,[int]$seconds){
    $report=Join-Path $out "$name.txt"
    if(Test-Path -LiteralPath $report){Remove-Item -LiteralPath $report}
    Start-Process -FilePath $player -ArgumentList "-batchmode -nographics -job-worker-count 2 -$role -port $port -probe -expected $expected -report $report -exitAfter $seconds -logFile $out\$name.log" -WindowStyle Hidden -PassThru | Select-Object Id
}
function Await-Report([string]$name,[string]$pattern,[int]$seconds=45){
    $deadline=(Get-Date).AddSeconds($seconds)
    do {
        $file=Join-Path $out "$name.txt"
        if((Test-Path -LiteralPath $file) -and (Select-String -LiteralPath $file -Pattern $pattern -Quiet)){return}
        Start-Sleep -Seconds 1
    }while((Get-Date) -lt $deadline)
    throw "Timed out waiting for $name / $pattern"
}
Launch-Probe 'roomA-host' 'localHost' 7777 10 80
Await-Report 'roomA-host' 'count=1 '
foreach($i in 1..9){Launch-Probe "roomA-client$i" 'localClient' 7777 10 95}
Await-Report 'roomA-host' 'count=10 '
Launch-Probe 'roomA-overflow' 'localClient' 7777 10 20
Launch-Probe 'roomB-host' 'localHost' 7778 2 80
Await-Report 'roomB-host' 'count=1 '
Launch-Probe 'roomB-client' 'localClient' 7778 2 95
Await-Report 'roomA-host' 'exploring=True'
Launch-Probe 'roomA-locked' 'localClient' 7777 10 20
Write-Output 'Both rooms launched. Probes close themselves. Inspect NetworkQA logs for outcomes.'
