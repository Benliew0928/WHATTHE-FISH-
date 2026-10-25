param([string]$PlayerPath,[ValidateSet('Football','Basketball','Golf','Fishing')][string]$Sport='Football',[int]$Port=7941,[ValidateSet('Return','Expire','HostLoss')][string]$Scenario='Return',[switch]$Online)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskPlayer=if($PlayerPath){[IO.Path]::GetFullPath($PlayerPath)}else{Join-Path $taskRoot 'Builds/CrewConnectionsQA/Player/WhatTheFish.exe'}
$taskRun=Join-Path $taskRoot ('Builds/CrewConnectionsQA/Run-'+$Sport+'-'+$Scenario+'-'+$(if($Online){'Relay-'}else{''})+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$taskProcesses=@{};$script:assertions=0
function Read-Peer([string]$Name){for($i=0;$i -lt 8;$i++){try{return Get-Content -LiteralPath (Join-Path $taskRun "$Name/crew.json") -Raw | ConvertFrom-Json}catch{Start-Sleep -Milliseconds 30}};return $null}
function Wait-Peer([string]$Name,[scriptblock]$Check,[int]$Seconds=70){
 $end=(Get-Date).AddSeconds($Seconds)
 while((Get-Date)-lt $end){if($taskProcesses[$Name].HasExited){throw "Player exited: $Name in $taskRun"};$value=Read-Peer $Name;if($value -and (& $Check $value)){$script:assertions++;return $value};Start-Sleep -Milliseconds 250}
 throw "Timed out waiting for $Name ($Check) in $taskRun"
}
function Command([string]$Name,[string]$Value){[IO.File]::WriteAllText((Join-Path $taskRun "$Name/crew-command.txt"),$Value)}
function Check([bool]$Condition,[string]$Message){if(!$Condition){throw $Message};$script:assertions++}
try{
 $count=if($Sport -eq 'Basketball'){4}else{2}
 for($index=0;$index -lt $count;$index++){
  $name=if($index -eq 0){'Host'}else{'Guest'+$index};$folder=Join-Path $taskRun $name;New-Item -ItemType Directory -Force -Path $folder|Out-Null
  $arguments=@('-batchmode','-force-d3d11','-screen-fullscreen','0','-screen-width','960','-screen-height','540','-job-worker-count','1','-skipWelcome','-sport',$Sport,'-port',"$Port",'-guestProfile',('crew'+$Port+$index),'-crewReview',('"'+$folder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
  if($Online){if($index -eq 0){$arguments+='-onlineHost'}else{$arguments+=@('-onlineJoin',(Read-Peer Host).code)}}else{$arguments+=$(if($index -eq 0){'-localHost'}else{'-localClient'})}
  $taskProcesses[$name]=Start-Process -FilePath $taskPlayer -ArgumentList $arguments -WindowStyle Hidden -PassThru
  $null=Wait-Peer $name {param($v) $v.connected -and $v.peers.Count -ge 1 -and (!$Online -or $v.code -match '^[A-Z0-9]{4,12}$')} 120
 }
 $null=Wait-Peer Host {param($v) $v.peers.Count -eq $count}
 Start-Sleep -Seconds 2
 Command Host explore
 $null=Wait-Peer Host {param($v) $v.exploring -and !$v.travelling -and $v.peers.Count -eq $count}
 $null=Wait-Peer Guest1 {param($v) $v.exploring -and !$v.travelling}
 Command Host start
 if($Sport -eq 'Football'){$null=Wait-Peer Host {param($v) $v.phase -eq 'Kickoff'};Start-Sleep -Seconds 4;Command Host kickoff}
 $null=Wait-Peer Host {param($v) $v.phase -in @('Regulation','Playing')} 100
 $before=Read-Peer Host;$guestBefore=$before.peers|Where-Object member -eq (Read-Peer Guest1).member
 Check ($null -ne $guestBefore) 'Missing original guest identity.'
 $before|ConvertTo-Json -Depth 20|Set-Content (Join-Path $taskRun 'before.json')
 Command Guest1 $(if($Scenario -eq 'Expire'){'drop:35'}else{'drop:8'})
 $paused=Wait-Peer Host {param($v) $v.paused -and $v.seconds -gt 0}
 if($count -gt 2){$null=Wait-Peer Guest2 {param($v) $v.paused -and $v.seconds -gt 0} 5}
 Start-Sleep -Seconds 2
 $still=Read-Peer Host
 Check ([math]::Abs($still.clock-$paused.clock)-lt .25) 'Match clock advanced during guest reconnection.'
 Check ($still.timeScale -eq 0) 'Gameplay simulation did not pause.'
 Check ($still.phase -eq $before.phase) 'The round changed phase during the grace period.'
 $paused|ConvertTo-Json -Depth 20|Set-Content (Join-Path $taskRun 'paused.json')
 if($Scenario -eq 'Expire'){
  $resumed=Wait-Peer Host {param($v) !$v.paused -and $v.peers.Count -eq ($count-1)} 45
  Check ($resumed.timeScale -eq 1) 'Simulation did not resume after expiry.'
  if($Sport -in @('Football','Basketball')){Check ($resumed.phase -eq 'Cancelled') 'An incomplete team round did not cancel after the grace period.'}
 }else{
 $resumed=Wait-Peer Host {param($v) !$v.paused -and $v.peers.Count -eq $count} 45
 $null=Wait-Peer Guest1 {param($v) !$v.paused -and $v.connected -and $v.peers.Count -eq $count}
 $guestAfter=$resumed.peers|Where-Object member -eq $guestBefore.member
 Check ($guestAfter.stable -eq $guestBefore.stable) 'Participant ID changed after reconnect.'
 Check ($guestAfter.seat -eq $guestBefore.seat) 'Reserved seat was not restored.'
 Check ($guestAfter.football -eq $guestBefore.football -and $guestAfter.basketball -eq $guestBefore.basketball) 'Team changed after reconnect.'
 Check ($resumed.timeScale -eq 1) 'Simulation did not resume.'
 Check ($resumed.phase -eq $before.phase) 'The original active round was not restored.'
 if($Sport -eq 'Football'){Check (($resumed.football|ConvertFrom-Json).scoreA -eq ($before.football|ConvertFrom-Json).scoreA) 'Football score changed.'}
 if($Sport -eq 'Basketball'){Check (($resumed.basketball|ConvertFrom-Json).scoreA -eq ($before.basketball|ConvertFrom-Json).scoreA) 'Basketball score changed.'}
 if($Scenario -eq 'HostLoss'){
  Stop-Process -Id $taskProcesses['Host'].Id -Force
  $migrated=Wait-Peer Guest1 {param($v) $v.host -and !$v.paused -and $v.peers.Count -eq ($count-1)} 120
  Check ($migrated.timeScale -eq 1) 'New host did not resume simulation.'
  $returned=$migrated.peers|Where-Object member -eq $guestBefore.member
  Check ($returned.stable -eq $guestBefore.stable -and $returned.seat -eq $guestBefore.seat) 'Host migration lost the previously reconnected guest identity.'
  if($count -gt 2){$null=Wait-Peer Guest2 {param($v) !$v.paused -and $v.peers.Count -eq ($count-1)}}
  $migrated|ConvertTo-Json -Depth 20|Set-Content (Join-Path $taskRun 'migrated.json')
 }else{
  Command Guest1 leave
  $null=Wait-Peer Host {param($v) !$v.paused -and $v.peers.Count -eq ($count-1)} 12
  Check (!(Read-Peer Host).paused) 'Voluntary departure incorrectly held the room.'
 }
 }
 $resumed|ConvertTo-Json -Depth 20|Set-Content (Join-Path $taskRun 'resumed.json')
 foreach($name in $taskProcesses.Keys){$log=Join-Path $taskRun "$name/player.log";Check (!(Select-String -LiteralPath $log -Pattern 'NullReferenceException|MissingReferenceException|InvalidOperationException|CONTENT_LOAD_FAILED|Shader error' -Quiet)) "Runtime error in $name"}
 $record=[pscustomobject]@{Sport=$Sport;Scenario=$Scenario;Relay=[bool]$Online;Assertions=$script:assertions;Passed=$true;Run=$taskRun.Substring($taskRoot.Length+1).Replace('\','/')}
 $record|ConvertTo-Json|Set-Content (Join-Path $taskRun 'result.json')
 $record|ConvertTo-Json -Compress
}finally{
 if($Online){foreach($name in @($taskProcesses.Keys|Sort-Object)){
  if(!$taskProcesses[$name].HasExited){try{if((Read-Peer $name).connected){Command $name leave;$null=Wait-Peer $name {param($v) !$v.connected} 30};Command $name delete-profile;$null=Wait-Peer $name {param($v) $v.result -eq 'OK delete-profile'} 25}catch{Write-Warning "Temporary profile cleanup needs review: $name in $taskRun"}}
 }}
 foreach($process in $taskProcesses.Values){if(!$process.HasExited){Stop-Process -Id $process.Id -Force}}
}
