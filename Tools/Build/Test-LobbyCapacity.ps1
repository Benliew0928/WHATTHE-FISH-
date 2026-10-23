param([string]$PlayerPath,[int]$Port=8033,[string]$OutputName=('Capacity-'+(Get-Date -Format 'yyyyMMdd-HHmmss')))
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$taskOutput=Join-Path $taskRoot ('Builds/LobbyRefreshQA/'+$OutputName)
New-Item -ItemType Directory -Force $taskOutput | Out-Null
$taskProcesses=@()
function Read-State($index){$file=Join-Path $taskOutput "$index/roster.txt";if(Test-Path $file){try{return Get-Content $file -Raw}catch{return ''}}return ''}
function Check($ok,$label){if(!$ok){throw $label};"PASS $label" | Tee-Object -FilePath (Join-Path $taskOutput 'results.txt') -Append}
function Until($condition,$label,$seconds=65){$deadline=(Get-Date).AddSeconds($seconds);while(!( & $condition ) -and (Get-Date) -lt $deadline){Start-Sleep -Milliseconds 400};Check (& $condition) $label}
function Command($index,$value){Set-Content -LiteralPath (Join-Path $taskOutput "$index/command.txt") -Value $value}
try{
 for($i=0;$i -lt 6;$i++){
  $folder=Join-Path $taskOutput "$i";New-Item -ItemType Directory -Force $folder | Out-Null
  $args=@('-batchmode','-force-d3d11','-screen-width','800','-screen-height','450','-screen-fullscreen','0','-job-worker-count','1','-contentCache',('"'+(Join-Path $taskOutput 'ContentCache')+'"'),'-roomCapacityProbe',('"'+$folder+'"'),'-port',$Port,'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
  $args+=if($i -eq 0){'-localHost'}else{'-localClient'}
  $taskProcesses+=Start-Process -FilePath $PlayerPath -ArgumentList $args -WindowStyle Hidden -PassThru
  if($i -eq 0){Start-Sleep -Seconds 4}
 }
 Until { (Read-State 0) -match 'count=6 players=6 spectators=0 unique=True' } 'Six real peers share unique player places'
 Command 0 'unready:host'
 Until { (Read-State 0) -match 'lobbyReady=True readyGuests=5' } 'Host ready flag is excluded from gate'
 Command 0 'spot:Fishing'
 Until { Test-Path (Join-Path $taskOutput '0/spot-result.txt') } 'Capacity decision returned'
 Check ((Get-Content (Join-Path $taskOutput '0/spot-result.txt') -Raw) -match 'changed=False.*5 player places') 'Six players cannot switch to five-player Fishing'
 Command 1 'spot:Golf'
 Until { Test-Path (Join-Path $taskOutput '1/spot-result.txt') } 'Guest decision returned'
 Check ((Get-Content (Join-Path $taskOutput '1/spot-result.txt') -Raw) -match 'changed=False.*Only the host') 'Guest service rejects spot change'
 Command 5 'seat:10'
 Until { (Read-State 0) -match 'players=5 spectators=1' } 'Extra player voluntarily becomes spectator'
 Command 0 'spot:Golf'
 Until { (Read-State 0) -match 'sport=Golf' } 'Host can change after a rejected choice'
 Command 0 'spot:Fishing'
 Until { @(0..5 | Where-Object { (Read-State $_) -notmatch 'sport=Fishing' }).Count -eq 0 } 'All six peers load Fishing in the same room' 100
 Check ((Read-State 0) -match 'lobbyReady=False readyGuests=0') 'Spot change clears every guest readiness'
 Check ((Read-State 0) -match 'players=5 spectators=1 unique=True') 'Fishing preserves spectator and normalizes five player places'
 Command 0 'seat:11'
 Until { (Read-State 0) -match 'players=4 spectators=2.*' -and (Read-State 0) -match 'hostSeat=11' } 'Host can watch from a spectator place'
 foreach($i in 1..4){Command $i 'ready:after-spot'}
 Until { (Read-State 0) -match 'lobbyReady=False readyGuests=4' } 'Unready spectator still blocks departure'
 Command 5 'ready:spectator'
 Until { (Read-State 0) -match 'lobbyReady=True readyGuests=5' } 'All guests ready permits an unready spectator host'
 Command 0 'explore:true'
 Until { @(0..5 | Where-Object { (Read-State $_) -notmatch 'exploring=True travel=Complete' }).Count -eq 0 } 'Spectator host starts shared Fishing entry for all six peers' 100
 foreach($i in 0..5){Check (!(Select-String (Join-Path $taskOutput "$i/player.log") -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException' -Quiet)) "No runtime exceptions on peer $i"}
 'LOBBY_CAPACITY_SUITE_PASS' | Tee-Object -FilePath (Join-Path $taskOutput 'results.txt') -Append
}finally{foreach($p in $taskProcesses){if(!$p.HasExited){Stop-Process -Id $p.Id}}}
