param([string]$PlayerPath)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'}
$managed=Join-Path (Split-Path (Resolve-Path -LiteralPath $PlayerPath).Path) 'WhatTheFish_Data/Managed'
$sdk=[Reflection.Assembly]::LoadFrom((Join-Path $managed 'Unity.Services.Multiplayer.dll'))
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
$lobbyType=$sdk.GetType('Unity.Services.Lobbies.Models.Lobby',$true)
$playerType=$sdk.GetType('Unity.Services.Lobbies.Models.Player',$true)
$handlerType=$sdk.GetType('Unity.Services.Multiplayer.LobbyHandler',$true)
$cacheType=$sdk.GetType('Lobbies.SDK.LobbyCacher.LobbyCacher',$true)
$serviceType=$sdk.GetType('Unity.Services.Lobbies.Internal.WrappedLobbyService',$true)
$patcherType=$sdk.GetType('LobbyPatcher',$false)
if(!$patcherType){$patcherType=$sdk.GetTypes() | Where-Object Name -eq 'LobbyPatcher'}
function New-Lobby([int]$Version,[string[]]$Ids){
 $lobby=[Runtime.CompilerServices.RuntimeHelpers]::GetUninitializedObject($lobbyType)
 $listType=[Collections.Generic.List``1].MakeGenericType($playerType)
 $players=[Activator]::CreateInstance($listType)
 foreach($id in $Ids){$player=[Runtime.CompilerServices.RuntimeHelpers]::GetUninitializedObject($playerType);$playerType.GetProperty('Id').SetValue($player,$id);$players.Add($player)}
 $lobbyType.GetProperty('Id').SetValue($lobby,'snapshot-fixture')
 $lobbyType.GetProperty('HostId').SetValue($lobby,'A')
 $lobbyType.GetProperty('Version').SetValue($lobby,$Version)
 $lobbyType.GetProperty('Players').SetValue($lobby,$players)
 return $lobby
}
function Require([bool]$Condition,[string]$Message){if(!$Condition){throw $Message};Write-Output "PASS $Message"}

# HTTP snapshots and the service cache can have independently ordered lists.
# A patch based on [X,A,B,C] must never remove A from [A,B,C,X].
$cache=[Activator]::CreateInstance($cacheType,@('A'))
$authoritative=New-Lobby 11 @('A','B','C','R')
[void]$cacheType.GetMethod('AddLobbyCache',$flags).Invoke($cache,@('snapshot-fixture',$authoritative))
$service=[Runtime.CompilerServices.RuntimeHelpers]::GetUninitializedObject($serviceType)
$serviceType.GetField('m_LobbyCacher',$flags).SetValue($service,$cache)
$handler=[Runtime.CompilerServices.RuntimeHelpers]::GetUninitializedObject($handlerType)
$handlerType.GetField('m_LobbyService',$flags).SetValue($handler,$service)
$handlerType.GetProperty('Lobby',$flags).SetValue($handler,(New-Lobby 10 @('A','B','C','X')))
$patch=$patcherType.GetMethod('GetLobbyDiff',$flags).Invoke($null,@((New-Lobby 10 @('X','A','B','C')),$authoritative))
$handlerType.GetMethod('OnLobbyChanged',$flags).Invoke($handler,@($patch))
$state=$handlerType.GetProperty('Lobby',$flags).GetValue($handler)
Require (($state.Players.Id -join ',') -eq 'A,B,C,R') 'Positional cache updates preserve the correct surviving members'
Require ($state.HostId -eq 'A') 'The valid replacement host remains present'
Require (![object]::ReferenceEquals($state,$authoritative)) 'Handler and cache own different lobby snapshots'
$authoritative.Players.RemoveAt(0)
Require (($state.Players.Id -join ',') -eq 'A,B,C,R') 'Later cache mutations cannot alter the session roster'
$handlerType.GetMethod('ApplyLobbySnapshot',$flags).Invoke($handler,@((New-Lobby 9 @('X'))))
Require ($handlerType.GetProperty('Lobby',$flags).GetValue($handler).Version -eq 11) 'An older HTTP response cannot replace the recovered lobby'

# A full response can reorder surviving members. Reconstructing [C,B,A] from
# [A,B,C] with the SDK's synthetic index diff used to produce [B,C,A], so a later
# "remove index 0" event could remove the healthy B host instead of departed C.
$orderedCache=[Activator]::CreateInstance($cacheType,@('B'))
[void]$cacheType.GetMethod('AddLobbyCache',$flags).Invoke($orderedCache,@('snapshot-fixture',(New-Lobby 20 @('A','B','C'))))
$update=$cacheType.GetMethod('UpdateLobbyCache',$flags,$null,[Type[]]@([string],$lobbyType),$null)
[void]$update.Invoke($orderedCache,@('snapshot-fixture',(New-Lobby 21 @('C','B','A'))))
$cacheArgs=[object[]]@('snapshot-fixture',$null)
[void]$cacheType.GetMethod('TryGetLobbyCache',$flags).Invoke($orderedCache,$cacheArgs)
Require (($cacheArgs[1].Players.Id -join ',') -eq 'C,B,A') 'HTTP snapshots preserve authoritative player order in the service cache'
$removal=$patcherType.GetMethod('GetLobbyDiff',$flags).Invoke($null,@((New-Lobby 21 @('C','B','A')),(New-Lobby 22 @('B','A'))))
$patchUpdate=$cacheType.GetMethod('UpdateLobbyCache',$flags,$null,[Type[]]@([string],$sdk.GetType('Unity.Services.Lobbies.ILobbyChanges',$true)),$null)
[void]$patchUpdate.Invoke($orderedCache,@('snapshot-fixture',$removal))
[void]$cacheType.GetMethod('TryGetLobbyCache',$flags).Invoke($orderedCache,$cacheArgs)
Require ($cacheArgs[1].Version -eq 21 -and ($cacheArgs[1].Players.Id -join ',') -eq 'C,B,A') 'Indexed membership events wait for an authoritative snapshot'
[void]$update.Invoke($orderedCache,@('snapshot-fixture',(New-Lobby 22 @('B','A'))))
[void]$cacheType.GetMethod('TryGetLobbyCache',$flags).Invoke($orderedCache,$cacheArgs)
Require (($cacheArgs[1].Players.Id -join ',') -eq 'B,A') 'The complete departure response removes the departed player and keeps the host'
$changesType=$sdk.GetType('Unity.Services.Lobbies.LobbyPatcherChanges',$true)
$invalid=[Activator]::CreateInstance($changesType,@(23))
[void]$changesType.GetMethod('NameChange',$flags).Invoke($invalid,@('must-not-commit'))
[void]$changesType.GetMethod('PlayerConnectionInfoChange',$flags).Invoke($invalid,@(99,'stale'))
[void]$patchUpdate.Invoke($orderedCache,@('snapshot-fixture',$invalid))
[void]$cacheType.GetMethod('TryGetLobbyCache',$flags).Invoke($orderedCache,$cacheArgs)
Require ($cacheArgs[1].Version -eq 22 -and !$cacheArgs[1].Name -and ($cacheArgs[1].Players.Id -join ',') -eq 'B,A') 'Invalid indexed patches preserve all prior fields and version'
[void]$update.Invoke($orderedCache,@('snapshot-fixture',(New-Lobby 23 @('B','A','R'))))
[void]$cacheType.GetMethod('TryGetLobbyCache',$flags).Invoke($orderedCache,$cacheArgs)
Require ($cacheArgs[1].Version -eq 23 -and ($cacheArgs[1].Players.Id -join ',') -eq 'B,A,R') 'A complete response recovers after a rejected patch'
# A redelivered event must not be inserted twice while the first awaits HTTP.
$channelType=$sdk.GetType('Unity.Services.Lobbies.Internal.LobbyChannel',$true)
$channel=[Runtime.CompilerServices.RuntimeHelpers]::GetUninitializedObject($channelType)
$queueType=[Collections.Generic.SortedList``2].MakeGenericType([int],$sdk.GetType('Unity.Services.Lobbies.ILobbyChanges',$true))
$queue=[Activator]::CreateInstance($queueType)
$pending=[Activator]::CreateInstance($changesType,@(30))
$queue.Add(30,$pending)
$channelType.GetField('eventProcessQueue',$flags).SetValue($channel,$queue)
$channelType.GetField('eventLock',$flags).SetValue($channel,[object]::new())
$task=$channelType.GetMethod('HandleLobbyChanges',$flags).Invoke($channel,@($pending,$null))
[void]$task.GetAwaiter().GetResult()
Require ($queue.Count -eq 1 -and [object]::ReferenceEquals($queue[30],$pending)) 'Duplicate pending events are processed once without losing the queued version'
Write-Output 'LOBBY_SNAPSHOT_PASS'
