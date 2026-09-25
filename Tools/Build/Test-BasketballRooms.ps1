param([ValidateSet('Local','Cloud')][string]$Transport='Local')
$ErrorActionPreference='Stop'
$root=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$player=Join-Path $root 'Builds\WindowsFinal\WhatTheFish.exe'
$runId=Get-Date -Format 'yyyyMMdd-HHmmss'
$out=Join-Path $root "Builds\BasketballQA-$Transport-$runId"
New-Item -ItemType Directory -Path $out | Out-Null
$processes=[Collections.Generic.List[System.Diagnostics.Process]]::new()
$byName=@{}
$results=[Collections.Generic.List[string]]::new()
function Launch([string]$name,[string]$role,[int]$expected,[int]$seconds,[string]$sport='Football',[int]$start=80,[int]$return=105){
 $arguments="-batchmode -nographics -job-worker-count 2 -probe -sport $sport $role -authProfile bb$runId$name -expected $expected -startAt $start -returnAt $return -report $out\$name.txt -exitAfter $seconds -logFile $out\$name.log"
 $process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
 $processes.Add($process)
 $byName[$name]=$process
}
function Await([string]$name,[string]$pattern,[int]$seconds=80){
 $deadline=(Get-Date).AddSeconds($seconds)
 do {
  $file=Join-Path $out "$name.txt"
  if((Test-Path -LiteralPath $file) -and (Select-String -LiteralPath $file -Pattern $pattern -Quiet)){return}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $deadline)
 throw "Timed out: $name / $pattern. Evidence: $out"
}
function Pass([string]$message){$results.Add("PASS $message");Write-Output "PASS $message"}
try {
 if($Transport -eq 'Cloud'){
  Launch 'Ahost' "-cloudHost -codeFile $out\Acode.txt -presetProbe -leaveAfter 165" 10 170 'Basketball' 90 112
  Await 'Ahost' 'CLOUD_ROOM code=[A-Z0-9]+ connected=True'
  $codeA=(Get-Content "$out\Acode.txt" -Raw).Trim();$joinA="-cloudCode $codeA"
 }else{
  Launch 'Ahost' '-localHost -port 7887 -presetProbe -leaveAfter 165' 10 170 'Basketball' 90 112
  Await 'Ahost' 'count=1 connected=True';$joinA='-localClient -port 7887'
 }
 # Guests deliberately start on Football; room authority must select Basketball.
 foreach($i in 1..9){$role=$joinA;if($i -eq 9){$role+=' -leaveAfter 118'};Launch "Aguest$i" $role 10 200;Start-Sleep -Milliseconds $(if($Transport -eq 'Cloud'){1800}else{350})}
 Await 'Ahost' 'count=10 connected=True'
 foreach($i in 1..9){Await "Aguest$i" 'sport=Basketball activeEnvironments=1 world=.*"logo":2' 25}
 Pass 'Ten-player basketball room; host sport and logo received by all nine guests'
 $line=Get-Content "$out\Ahost.txt" | Where-Object {$_ -match 'count=10 connected=True'} | Select-Object -Last 1
 $positions=[regex]::Matches($line,'\(-?\d+\.\d+, -?\d+\.\d+, -?\d+\.\d+\)') | ForEach-Object {$_.Value}
 if(($positions|Sort-Object -Unique).Count -ne 10){throw 'Player spawn positions are not distinct'}
 Pass 'Ten distinct player spawn positions'
 Launch 'Overflow' $joinA 10 20
 Await 'Overflow' 'full' 30;Pass 'Eleventh player rejected as full'
 # A second, simultaneous football room must keep its own state.
 if($Transport -eq 'Cloud'){
  Launch 'Bhost' "-cloudHost -codeFile $out\Bcode.txt -leaveAfter 95" 2 100 'Football' 20 60
  Await 'Bhost' 'CLOUD_ROOM code=[A-Z0-9]+ connected=True'
  $codeB=(Get-Content "$out\Bcode.txt" -Raw).Trim();$joinB="-cloudCode $codeB"
 }else{
  Launch 'Bhost' '-localHost -port 7888 -leaveAfter 95' 2 100 'Football' 20 60
  Await 'Bhost' 'count=1 connected=True';$joinB='-localClient -port 7888'
 }
 Launch 'Bguest' $joinB 2 155 'Basketball'
 Await 'Bguest' 'count=2 connected=True exploring=True.*sport=Football'
 Pass 'Separate football room adopts Football and starts independently'
 Launch 'Locked' $joinB 2 18
 Await 'Locked' 'locked|Exploration has started' 25;Pass 'Joining during exploration rejected'
 Await 'Aguest1' 'count=10 connected=True exploring=True.*sport=Basketball' 110
 Pass 'Basketball host starts all players'
 Await 'Ahost' 'time=11[4-9].*exploring=False' 60
 Await 'Aguest1' 'time=11[4-9].*exploring=False' 35
 Pass 'Basketball host returns guests to waiting room'
 foreach($name in @('Aguest1','Bguest')){Await $name 'PASS GUEST_CANNOT_CHANGE_LOGO' 5;Await $name 'PASS GUEST_CANNOT_CHANGE_SPORT' 5}
 Pass 'Guest attempts to change sport or logo are ignored'
 # Free one place, then check a new guest receives basketball and its logo after return.
 Await 'Ahost' 'time=1[234][0-9].*count=9 connected=True exploring=False' 35
 Launch 'LateGuest' $joinA 10 65 'Football'
 Await 'LateGuest' 'count=10 connected=True exploring=False.*sport=Basketball.*"logo":2' 25
 Pass 'Joining after return receives basketball and the host logo'
 Await 'Aguest1' 'connected=False exploring=False error=.+ sport=' 65
 Await 'Bguest' 'connected=False exploring=False error=.+ sport=' 65
 Pass 'Host departure returns both sets of guests to usable UI'
 foreach($name in @('Ahost','Aguest1','Bhost','Bguest')){
  if(Select-String -LiteralPath "$out\$name.log" -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException' -Quiet){throw "Runtime exception in $name"}
 }
 Pass 'No null, missing-reference, or index errors in host/guest logs'
} finally {
 foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id -ErrorAction SilentlyContinue}}
 $results | Set-Content -LiteralPath (Join-Path $out 'results.txt')
 Write-Output "Evidence: $out"
}
