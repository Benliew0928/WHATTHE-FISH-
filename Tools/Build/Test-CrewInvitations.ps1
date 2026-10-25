param([string]$PlayerPath)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskPlayer=if($PlayerPath){if([IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath}else{Join-Path $taskRoot $PlayerPath}}else{Join-Path $taskRoot 'Builds/CrewConnectionsQA/Player/WhatTheFish.exe'}
$taskRun=Join-Path $taskRoot ('Builds/CrewConnectionsQA/Invitations-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$taskProcesses=@{};$script:assertions=0;$prefix='invite'+[guid]::NewGuid().ToString('N').Substring(0,12)
function Read-Peer([string]$Name){for($i=0;$i -lt 8;$i++){try{return Get-Content -LiteralPath (Join-Path $taskRun "$Name/crew.json") -Raw|ConvertFrom-Json}catch{Start-Sleep -Milliseconds 30}};return $null}
function Wait-Peer([string]$Name,[scriptblock]$Check,[int]$Seconds=80){$end=(Get-Date).AddSeconds($Seconds);while((Get-Date)-lt $end){if($taskProcesses[$Name].HasExited){throw "Player exited: $Name"};$v=Read-Peer $Name;if($v -and (& $Check $v)){$script:assertions++;return $v};Start-Sleep -Milliseconds 250};throw "Timed out: $Name $Check in $taskRun"}
function Command([string]$Name,[string]$Value){$command=$Value+':'+[guid]::NewGuid().ToString('N');[IO.File]::WriteAllText((Join-Path $taskRun "$Name/crew-command.txt"),$command);$null=Wait-Peer $Name {param($v) $v.result -eq ('OK '+$command) -or $v.result.StartsWith('ERROR '+$command)};if((Read-Peer $Name).result.StartsWith('ERROR ')){throw (Read-Peer $Name).result}}
function Click([string]$Name,[string]$Control){$null=Wait-Peer $Name {param($v) @($v.controls|Where-Object {$_.name -eq $Control -and $_.enabled}).Count -gt 0};Command $Name ('click:'+$Control)}
function Capture([string]$Name,[string]$Label){Command $Name ('capture:'+$Label);$end=(Get-Date).AddSeconds(15);while(!(Test-Path (Join-Path $taskRun "$Name/ui-$Label.png")) -and (Get-Date)-lt $end){Start-Sleep -Milliseconds 200};if(!(Test-Path (Join-Path $taskRun "$Name/ui-$Label.png"))){throw "Capture missing: $Name $Label"}}
try{
 foreach($name in @('Host','Guest')){
  $folder=Join-Path $taskRun $name;New-Item -ItemType Directory -Force -Path $folder|Out-Null
  $argsList=@('-batchmode','-force-d3d11','-screen-fullscreen','0','-screen-width','1600','-screen-height','900','-job-worker-count','1','-skipWelcome','-sport','Football','-guestProfile',($prefix+$name),'-crewReview',('"'+$folder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
  $taskProcesses[$name]=Start-Process -FilePath $taskPlayer -ArgumentList $argsList -WindowStyle Hidden -PassThru
  $null=Wait-Peer $name {param($v) $null -ne $v.self} 120
  Command $name cloud
 }
 $hostId=(Wait-Peer Host {param($v) ![string]::IsNullOrWhiteSpace($v.self.id)}).self.id;$guestId=(Wait-Peer Guest {param($v) ![string]::IsNullOrWhiteSpace($v.self.id)}).self.id
 Command Host ('friend-request:'+$guestId);Command Guest ('friend-accept:'+$hostId)
 Command Host host-online;Command Guest host-online
 $hostRoom=(Wait-Peer Host {param($v) $v.connected -and $v.host -and $v.peers.Count -eq 1}).code
 $guestRoom=(Wait-Peer Guest {param($v) $v.connected -and $v.host -and $v.peers.Count -eq 1}).code
 Command Host friends
 Click Host ('Invite friend '+(Read-Peer Guest).self.code)
 $null=Wait-Peer Host {param($v) ($v.ui -join ' ') -like '*Invitation sent*'}
 Capture Host sent
 Command Guest refresh
 $null=Wait-Peer Guest {param($v) $v.invites -eq 1 -and ($v.ui -join ' ') -like '*invited you*'}
 Capture Guest notification
 Click Guest 'View new room invitation'
 $null=Wait-Peer Guest {param($v) ($v.ui -join ' ') -like '*ROOM INVITATIONS*'}
 Capture Guest inbox-desktop
 Command Guest 'resize:1080:600';Start-Sleep -Seconds 1;Capture Guest inbox-landscape
 $join=((Read-Peer Guest).controls|Where-Object {$_.name -like 'Join invitation *'}|Select-Object -First 1).name
 Click Guest $join
 $null=Wait-Peer Guest {param($v) ($v.ui -join ' ') -like '*A NEW ADVENTURE*'}
 $null=Wait-Peer Guest {param($v) @($v.controls|Where-Object {$_.name -eq 'Close social panel' -and !$_.enabled}).Count -eq 1}
 Capture Guest confirmation
 Click Guest 'Stay in current room'
 $null=Wait-Peer Guest {param($v) $v.connected -and $v.host -and $v.code -eq $guestRoom}
 Click Guest $join;Click Guest 'Confirm invitation join'
 $joined=Wait-Peer Guest {param($v) $v.connected -and !$v.host -and $v.code -eq $hostRoom -and $v.peers.Count -eq 2 -and $v.invites -eq 0} 100
 $null=Wait-Peer Host {param($v) $v.peers.Count -eq 2}
 foreach($name in $taskProcesses.Keys){if(Select-String -LiteralPath (Join-Path $taskRun "$name/player.log") -Pattern 'NullReferenceException|MissingReferenceException|InvalidOperationException|CONTENT_LOAD_FAILED|Shader error' -Quiet){throw "Runtime error in $name"};$script:assertions++}
 $joined|ConvertTo-Json -Depth 10|Set-Content (Join-Path $taskRun 'joined.json')
 $record=[pscustomobject]@{Passed=$true;Assertions=$script:assertions;Run=$taskRun.Substring($taskRoot.Length+1).Replace('\','/');Flow='Friend button > notification > inbox > stay/leave confirmation > online room'}
 $record|ConvertTo-Json|Set-Content (Join-Path $taskRun 'result.json');$record|ConvertTo-Json -Compress
}finally{
 foreach($name in @('Guest','Host')){if($taskProcesses.ContainsKey($name) -and !$taskProcesses[$name].HasExited){try{Command $name leave;Command $name delete-profile}catch{Write-Warning "Test-profile cleanup needs review: $name in $taskRun"};Stop-Process -Id $taskProcesses[$name].Id -Force}}
}
