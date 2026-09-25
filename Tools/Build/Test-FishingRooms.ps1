$ErrorActionPreference='Stop'
$root=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$out=Join-Path $root ('Builds\FishingRoomsQA-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $out | Out-Null
$player=Join-Path $root 'Builds\WindowsFinal\WhatTheFish.exe'
$processes=[Collections.Generic.List[System.Diagnostics.Process]]::new()
function Launch([string]$name,[string]$role,[string]$sport='Golf',[int]$duration=55){
 $arguments="-batchmode -nographics -job-worker-count 2 -probe -sport $sport $role -port 7906 -expected 5 -startAt 22 -returnAt 36 -report $out\$name.txt -exitAfter $duration -logFile $out\$name.log"
 $processes.Add((Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru))
}
function Await([string]$name,[string]$pattern,[int]$seconds=30){
 $deadline=(Get-Date).AddSeconds($seconds)
 do {
  $file=Join-Path $out "$name.txt"
  if((Test-Path -LiteralPath $file) -and (Select-String -LiteralPath $file -Pattern $pattern -Quiet)){return}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $deadline)
 throw "Timeout: $name / $pattern. Evidence: $out"
}
try {
 Launch 'host' '-localHost' 'Fishing'
 Await 'host' 'count=1 connected=True'
 foreach($i in 1..4){Launch "guest$i" '-localClient';Start-Sleep -Milliseconds 350}
 Await 'host' 'count=5 connected=True'
 foreach($i in 1..4){Await "guest$i" 'sport=Fishing activeEnvironments=1 world=.*LAGOON ISLAND'}
 $line=Get-Content "$out\host.txt" | Where-Object {$_ -match 'count=5 connected=True'} | Select-Object -First 1
 $positions=[regex]::Matches($line,'\(-?\d+\.\d+, -?\d+\.\d+, -?\d+\.\d+\)') | ForEach-Object {$_.Value}
 if(($positions | Sort-Object -Unique).Count -ne 5){throw 'Five distinct room spawns required'}
 Launch 'overflow' '-localClient' 'Golf' 12
 Await 'overflow' 'full \(5 players\)' 20
 Await 'guest1' 'count=5 connected=True exploring=True.*sport=Fishing' 30
 Await 'host' 'time=3[6-9].*exploring=False' 25
 Await 'guest1' 'time=3[6-9].*exploring=False' 10
 foreach($line in Get-Content "$out\host.txt" | Where-Object {$_ -match 'exploring=True'}){
  foreach($position in [regex]::Matches($line,'\(-?\d+\.\d+, (?<height>-?\d+\.\d+), -?\d+\.\d+\)')){
   if([float]$position.Groups['height'].Value -lt .3){throw "An explorer left the dry walkable surface: $line"}
  }
 }
 foreach($i in 1..4){Await "guest$i" 'PASS GUEST_CANNOT_CHANGE_SPORT' 3}
 foreach($file in Get-ChildItem -LiteralPath $out -Filter '*.log'){
  if(Select-String -LiteralPath $file.FullName -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException' -Quiet){throw "Runtime exception in $($file.Name)"}
 }
 'PASS five connected explorers, five distinct spawns, host-selected Fishing, sixth-player rejection, start/return and guest permissions.' | Set-Content -LiteralPath "$out\results.txt"
 Write-Output "PASS local fishing room integration. Evidence: $out"
} finally {
 foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id -ErrorAction SilentlyContinue}}
}
