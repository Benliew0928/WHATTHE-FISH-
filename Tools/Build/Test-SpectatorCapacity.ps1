param([string]$PlayerPath,[ValidateSet('Football','Basketball','Golf','Fishing')][string]$Sport='Football',[int]$Port=7810,[switch]$AbruptDisconnect)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$run=Join-Path $taskRoot ('Builds/DisconnectQA/Capacity-'+$Sport+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$players=if($Sport -eq 'Fishing'){5}else{10}
$total=$players+10
$running=[Collections.Generic.List[object]]::new()
$active=[Collections.Generic.List[string]]::new()
$processByName=@{}
$script:checks=0
$script:round=0
function Launch([string]$Name,[switch]$HostPeer,[switch]$Rejected){
 $folder=Join-Path $run $Name
 New-Item -ItemType Directory -Force -Path $folder | Out-Null
 $arguments=@('-batchmode','-force-d3d11','-screen-width','320','-screen-height','180','-screen-fullscreen','0','-job-worker-count','1',$(if($HostPeer){'-localHost'}else{'-localClient'}),'-port',"$Port",'-sport',$Sport,'-roomCapacityProbe',('"'+$folder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
 $process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
 $running.Add($process);$processByName[$Name]=$process
 if(!$Rejected){$active.Add($Name)}
}
function Read-Roster([string]$Name){
 $file=Join-Path $run "$Name/roster.txt"
 if($processByName[$Name].HasExited){throw "Unexpected player exit: $Name in $run"}
 # A replacement can race a Windows file open. Retry the read, but never accept old evidence.
 for($attempt=0;$attempt -lt 40;$attempt++){
  try {
   if([IO.File]::Exists($file)){
    $stream=[IO.File]::Open($file,[IO.FileMode]::Open,[IO.FileAccess]::Read,([IO.FileShare]::ReadWrite -bor [IO.FileShare]::Delete))
    $reader=[IO.StreamReader]::new($stream)
    try {$state=$reader.ReadToEnd()}finally{$reader.Dispose()}
    $written=[IO.File]::GetLastWriteTimeUtc($file)
    if($written.Year -gt 2000){
     if(((Get-Date).ToUniversalTime()-$written).TotalSeconds -gt 8){throw "Stale peer heartbeat: $Name in $run"}
     return $state
    }
   }
  } catch [IO.IOException] { }
  Start-Sleep -Milliseconds 50
 }
 return ''
}
function Await-Roster([string]$Name,[string]$Pattern,[int]$Seconds=100){
 $deadline=(Get-Date).AddSeconds($Seconds)
 while((Get-Date) -lt $deadline){
  if((Read-Roster $Name) -match $Pattern){$script:checks++;return}
  Start-Sleep -Milliseconds 500
 }
 throw "Capacity timeout: $Name / $Pattern in $run"
}
function Assert-All([int]$Playing,[int]$Watching){
 foreach($name in $active){Await-Roster $name "count=$($Playing+$Watching) players=$Playing spectators=$Watching unique=True connected=True error=\r?\n"}
}
function Command([string]$Name,[string]$Value){
 $target=Join-Path $run "$Name/command.txt";$temporary=$target+'.pending'
 [IO.File]::WriteAllText($temporary,$Value)
 for($attempt=0;$attempt -lt 40;$attempt++){
  try {
   # Publish a closed file; never truncate a command while the Unity probe reads it.
   if([IO.File]::Exists($target)){[IO.File]::Replace($temporary,$target,[NullString]::Value)}else{[IO.File]::Move($temporary,$target)}
   return
  } catch [IO.IOException] {Start-Sleep -Milliseconds 50}
 }
 throw "Could not publish command for $Name"
}
function Prove-Replication {
 $script:round++
 # Every survivor sends an owner RPC; every peer must receive every resulting network value.
 $tokens=@();$index=0
 foreach($name in $active){$token="R$($script:round)N$index";$tokens+=$token;Command $name "nick:$token";$index++}
 foreach($name in $active){foreach($token in $tokens){Await-Roster $name (':'+$token+'(?:,|/)') 25}}
}
function Stable([int]$Playing,[int]$Watching){
 $deadline=(Get-Date).AddSeconds(35)
 while((Get-Date) -lt $deadline){
  foreach($name in $active){
   if((Read-Roster $name) -notmatch "count=$($Playing+$Watching) players=$Playing spectators=$Watching unique=True connected=True error=\r?\n"){throw "Survivor lost its room: $name in $run"}
   $script:checks++
  }
  Start-Sleep -Seconds 2
 }
 Prove-Replication
}
function Select-Guest([bool]$Spectator){
 foreach($name in $active){
  if($name -eq 'Host'){continue}
  $state=Read-Roster $name
  if($state -match 'localSeat=(\d+)'){
   $seat=[int]$Matches[1]
   if(($seat -ge 10 -and $seat -lt 20) -eq $Spectator){return $name}
  }
 }
 throw 'No eligible guest to remove.'
}
function Remove-Guest([string]$Name){
 $before=Read-Roster $Name
 Add-Content -LiteralPath (Join-Path $run 'departures.txt') -Value ((Get-Date -Format o)+" abrupt=$AbruptDisconnect peer=$Name`n"+$before)
 if($AbruptDisconnect){Stop-Process -Id $processByName[$Name].Id}else{Set-Content -LiteralPath (Join-Path $run "$Name/leave") -Value 'Leave normally.'}
 [void]$active.Remove($Name)
}
try {
 Launch 'Host' -HostPeer
 Await-Roster 'Host' 'count=1 players=1 spectators=0 unique=True connected=True'
 for($i=1;$i -lt $total;$i++){
  Launch "Guest-$i"
  # Finish each island load/admission before launching another complete Unity player.
  # This keeps startup memory spikes from turning a single-machine room test into an OS paging test.
  Await-Roster "Guest-$i" 'local=\d+ localSeat=(?:[0-9]|1[0-9]) exploring=False'
 }
 Assert-All $players 10
 Launch 'Overflow' -Rejected
 Await-Roster 'Overflow' 'connected=False error=This room has no open places'
 Assert-All $players 10
 Prove-Replication
 foreach($role in @('Player','Spectator')){
  Remove-Guest (Select-Guest ($role -eq 'Spectator'))
  $remainingPlayers=$players-$(if($role -eq 'Player'){1}else{0})
  $remainingSpectators=10-$(if($role -eq 'Spectator'){1}else{0})
  Assert-All $remainingPlayers $remainingSpectators
  Launch "Replacement-$role"
  Assert-All $players 10
  Stable $players 10
  Write-Output "CAPACITY_RECOVERY sport=$Sport role=$role peers=$total"
 }
 # Start the real full room, including its load barrier and cloud arrival. Then lose both roles together.
 Command 'Host' 'explore:true:1'
 foreach($name in $active){Await-Roster $name 'exploring=True travel=Complete transit=False' 120}
 Prove-Replication
 Remove-Guest (Select-Guest $false)
 Remove-Guest (Select-Guest $true)
 Assert-All ($players-1) 9
 foreach($name in $active){Await-Roster $name 'exploring=True travel=Complete transit=False'}
 Stable ($players-1) 9
 Command 'Host' 'explore:false:2'
 foreach($name in $active){Await-Roster $name 'exploring=False'}
 Launch 'Replacement-Island-Player';Start-Sleep -Milliseconds 500
 Launch 'Replacement-Island-Spectator'
 Assert-All $players 10
 Prove-Replication
 foreach($log in Get-ChildItem -LiteralPath $run -Filter player.log -Recurse){
  if(Select-String -LiteralPath $log.FullName -Pattern 'Exception:|Assertion failed|CONTENT_LOAD_FAILED' -Quiet){throw "Runtime diagnostic: $($log.FullName)"}
 }
 "SPECTATOR_CAPACITY_PASS sport=$Sport peers=$total overflowRejected=True playerReplacement=True spectatorReplacement=True islandDoubleDeparture=True survivorsReplicate=True abrupt=$AbruptDisconnect checks=$script:checks" | Set-Content (Join-Path $run 'result.txt')
 Get-Content (Join-Path $run 'result.txt')
 Write-Output $run
} finally {foreach($process in $running){if(!$process.HasExited){Stop-Process -Id $process.Id}}}
