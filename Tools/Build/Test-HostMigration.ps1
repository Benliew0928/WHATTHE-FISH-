param([string]$PlayerPath,[ValidateSet('Football','Basketball','Golf','Fishing')][string]$Sport='Football',[int]$Port=7830,[switch]$Online,[switch]$AbruptGuests)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$run=Join-Path $taskRoot ('Builds/HostMigrationQA/Capacity-'+$Sport+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$players=if($Sport -eq 'Fishing'){5}else{10}
$total=$players+10
$running=[Collections.Generic.List[object]]::new()
$active=[Collections.Generic.List[string]]::new()
$processByName=@{}
$script:checks=0
$script:round=0
$script:profilePrefix='m'+[guid]::NewGuid().ToString('N').Substring(0,8)
function Launch([string]$Name,[switch]$HostPeer,[switch]$Rejected){
 $folder=Join-Path $run $Name
 New-Item -ItemType Directory -Force -Path $folder | Out-Null
 $arguments=@('-batchmode','-force-d3d11','-screen-width','320','-screen-height','180','-screen-fullscreen','0','-job-worker-count','1',$(if($Online){if($HostPeer){'-onlineHost'}else{'-onlineJoin'}}elseif($HostPeer){'-localHost'}else{'-localClient'}),'-port',"$Port",'-sport',$Sport,'-roomCapacityProbe',('"'+$folder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
 if($Online){if(!$HostPeer){$at=[Array]::IndexOf($arguments,'-onlineJoin');$arguments=@($arguments[0..$at])+@($script:roomCode)+@($arguments[($at+1)..($arguments.Length-1)])};$arguments+=@('-authProfile',($script:profilePrefix+$Name.Replace('-','')))}
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
function Await-Roster([string]$Name,[string]$Pattern,[int]$Seconds=160){
 $deadline=(Get-Date).AddSeconds($Seconds)
 while((Get-Date) -lt $deadline){
  if((Read-Roster $Name) -match $Pattern){$script:checks++;return}
  Start-Sleep -Milliseconds 500
 }
 throw "Capacity timeout: $Name / $Pattern in $run"
}
function Assert-All([int]$Playing,[int]$Watching){
 foreach($name in $active){
  Await-Roster $name "count=$($Playing+$Watching) players=$Playing spectators=$Watching unique=True connected=True error=\r?\n"
  # NGO can admit the avatar before the asynchronous session join has returned.
  if($Online){Await-Roster $name ('code='+[regex]::Escape($script:roomCode)+'\r?\n')}
 }
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

function Snapshot {
 $result=@{}
 foreach($name in $active){
  $state=Read-Roster $name
  if($state -notmatch 'localSeat=(\d+)'){throw "Missing seat: $name"}
  $seat=[int]$Matches[1]
  $line=[regex]::Match($state,"(?m)^member=(\S+) stable=(\d+) seat=$seat name=([^\r\n]+?) position=(.+)$")
  if(!$line.Success){throw "Missing persistent identity: $name"}
  $result[$name]=@{Seat=$seat;Member=$line.Groups[1].Value;Stable=$line.Groups[2].Value;Name=$line.Groups[3].Value;Code=[regex]::Match($state,'code=([^\r\n]+)').Groups[1].Value}
 }
 return $result
}
function Depart([string]$Name,[switch]$Graceful){
 Add-Content (Join-Path $run 'departures.txt') ((Get-Date -Format o)+" peer=$Name graceful=$Graceful`n"+(Read-Roster $Name))
 if($Graceful){Set-Content (Join-Path $run "$Name/leave") 'Leave normally.'}else{Stop-Process -Id $processByName[$Name].Id}
 [void]$active.Remove($Name)
}
function Handoff([string]$HostName,[switch]$Graceful,[switch]$Exploring,[switch]$PlayingMatch){
 $before=Snapshot
 $before | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $run ($HostName+'-handoff-before.json'))
 $expected=($active | Where-Object {$_ -ne $HostName} | Sort-Object {$before[$_].Seat} | Select-Object -First 1)
 $seat=$before[$expected].Seat
 Depart $HostName -Graceful:$Graceful
 foreach($name in $active){Await-Roster $name "recovering=False hostSeat=$seat(?: |\r?\n)"}
 if($PlayingMatch){foreach($name in $active){Await-Roster $name 'match=(Regulation|Kickoff|Overtime|Restart|Playing|FinalCountdown)' 15}}
 $after=Snapshot
 $after | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $run ($HostName+'-handoff-after.json'))
 foreach($name in $active){
  foreach($key in @('Seat','Member','Stable','Name','Code')){if($after[$name][$key] -ne $before[$name][$key]){throw "Changed $key for $name from [$($before[$name][$key])] to [$($after[$name][$key])]"};$script:checks++}
  Await-Roster $name ('host='+$(if($name -eq $expected){'True'}else{'False'})+' recovering=False')
  if($Exploring){Await-Roster $name 'exploring=True travel=Idle transit=False'}
 }
 $p=@($active | Where-Object {$after[$_].Seat -lt 10}).Count
 $s=$active.Count-$p
 Assert-All $p $s
 Stable $p $s
 Write-Host "HOST_HANDOFF_PASS sport=$Sport online=$Online next=$expected seat=$seat peers=$($active.Count) graceful=$Graceful"
 return $expected
}
try {
 Launch 'Host' -HostPeer
 Await-Roster 'Host' 'count=1 players=1 spectators=0 unique=True connected=True'
 if($Online){Await-Roster 'Host' 'code=[A-Z0-9]{6}';$state=Read-Roster 'Host';if($state -notmatch 'code=(\w+)'){throw 'Missing online room code'};$script:roomCode=$Matches[1]}
 for($i=1;$i -lt $total;$i++){
  Launch "Guest-$i"
  if($Online -and $i+1 -lt $total){Launch "Guest-$($i+1)"}
  Await-Roster "Guest-$i" 'local=\d+ localSeat=(?:[0-9]|1[0-9]) exploring=False'
  if($Online -and $i+1 -lt $total){$i++;Await-Roster "Guest-$i" 'local=\d+ localSeat=(?:[0-9]|1[0-9]) exploring=False'}
 }
 Assert-All $players 10
 Prove-Replication
 if($AbruptGuests){
  $before=Snapshot
  $guest=$active | Where-Object {$_ -ne 'Host' -and $before[$_].Seat -lt 10} | Sort-Object {$before[$_].Seat} -Descending | Select-Object -First 1
  $spectator=$active | Where-Object {$before[$_].Seat -ge 10} | Sort-Object {$before[$_].Seat} -Descending | Select-Object -First 1
  Depart $guest;Depart $spectator
  Assert-All ($players-1) 9;Stable ($players-1) 9
  Launch 'Guest-Refill';Await-Roster 'Guest-Refill' 'local=\d+ localSeat=(?:[0-9]|1[0-9]) exploring=False'
  Launch 'Spectator-Refill';Assert-All $players 10;Prove-Replication
  Write-Host "ABRUPT_GUEST_REFILL_PASS sport=$Sport online=$Online capacity=$total"
 }
 $handoff=Handoff 'Host';$current=@($handoff)[-1]
 Launch 'Replacement';Assert-All $players 10;Prove-Replication
 Command $current 'explore:true:1'
 foreach($name in $active){Await-Roster $name 'exploring=True travel=Complete transit=False' 150}
 Command $current 'match:start'
 foreach($name in $active){Await-Roster $name 'match=(TeamSelection|Countdown|Playing|Regulation|Kickoff)' 40}
 Start-Sleep -Seconds 16
 $handoff=Handoff $current -Exploring -PlayingMatch;$current=@($handoff)[-1]
 # Remove the other player seats while retaining the host and all spectators.
 $state=Snapshot
 foreach($name in @($active)){if($name -ne $current -and $state[$name].Seat -lt 10){Depart $name -Graceful}}
 Assert-All 1 10
 Prove-Replication
 $handoff=Handoff $current -Graceful -Exploring;$current=@($handoff)[-1]
 Assert-All 0 10
 $handoff=Handoff $current -Exploring;$current=@($handoff)[-1]
 Assert-All 0 9
 Command $current 'explore:false:2'
 foreach($name in $active){Await-Roster $name 'exploring=False'}
 Launch 'New-Player';Assert-All 1 9;Prove-Replication
 # A newly joined player takes precedence over every spectator, including an older connection.
 $handoff=Handoff $current -Graceful;$current=@($handoff)[-1]
 Await-Roster $current 'localSeat=0 exploring=False'
 foreach($log in Get-ChildItem -LiteralPath $run -Filter player.log -Recurse){if(Select-String -LiteralPath $log.FullName -Pattern 'Exception:|Assertion failed|CONTENT_LOAD_FAILED' -Quiet){throw "Runtime diagnostic: $($log.FullName)"}}
 "HOST_MIGRATION_PASS sport=$Sport online=$Online capacity=$total abruptGuests=$AbruptGuests lobby=True island=True repeated=True spectatorFallback=True graceful=True newPlayerPriority=True checks=$script:checks" | Set-Content (Join-Path $run 'result.txt')
 Get-Content (Join-Path $run 'result.txt');Write-Output $run
} finally {foreach($process in $running){if(!$process.HasExited){Stop-Process -Id $process.Id}}}
