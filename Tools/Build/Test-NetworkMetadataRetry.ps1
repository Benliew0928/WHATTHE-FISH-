param([string]$PlayerPath)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'}
$managed=Join-Path (Split-Path (Resolve-Path -LiteralPath $PlayerPath).Path) 'WhatTheFish_Data/Managed'
$sdk=[Reflection.Assembly]::LoadFrom((Join-Path $managed 'Unity.Services.Multiplayer.dll'))
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
Add-Type 'public static class NetworkRetryCounter { public static int Attempts; public static void Failed<T>(T error) { Attempts++; } }'
[NetworkRetryCounter]::Attempts=0
$sessionType=$sdk.GetType('Unity.Services.Multiplayer.SessionHandler',$true)
$session=[Runtime.CompilerServices.RuntimeHelpers]::GetUninitializedObject($sessionType)
$sessionType.GetProperty('Host',$flags).SetValue($session,'A')
$properties=[Collections.Generic.Dictionary``2].MakeGenericType([string],$sdk.GetType('Unity.Services.Multiplayer.SessionProperty',$true))
$properties=[Activator]::CreateInstance($properties)
# An unsupported transport deterministically fails JoinNetworkAsync before native
# networking. Exercise the same failure/retry path without Unity or a live service.
$metadata='{"Network":99,"HostId":"A"}'
$property=[Activator]::CreateInstance($sdk.GetType('Unity.Services.Multiplayer.SessionProperty'),@($metadata,[Enum]::ToObject($sdk.GetType('Unity.Services.Multiplayer.VisibilityPropertyOptions'),0),[Enum]::ToObject($sdk.GetType('Unity.Services.Multiplayer.PropertyIndex'),0)))
$properties.Add('_session_network',$property)
$sessionType.GetField('Properties',$flags).SetValue($session,$properties)
$moduleType=$sdk.GetType('Unity.Services.Multiplayer.NetworkModule',$true)
$module=[Runtime.CompilerServices.RuntimeHelpers]::GetUninitializedObject($moduleType)
$moduleType.GetField('m_Session',$flags).SetValue($module,$session)
$moduleType.GetField('m_MetadataGate',$flags).SetValue($module,[Threading.SemaphoreSlim]::new(1,1))
$relay=[Runtime.CompilerServices.RuntimeHelpers]::GetUninitializedObject($sdk.GetType('Unity.Services.Multiplayer.RelayHandler',$true))
$moduleType.GetField('m_RelayHandler',$flags).SetValue($module,$relay)
$event=$moduleType.GetEvent('StartFailed',$flags)
$method=[NetworkRetryCounter].GetMethod('Failed').MakeGenericMethod($sdk.GetType('Unity.Services.Multiplayer.SessionError'))
$event.AddEventHandler($module,[Delegate]::CreateDelegate($event.EventHandlerType,$method))
$validate=$moduleType.GetMethod('ValidateNetworkPropertyAsync',$flags)
for($attempt=1;$attempt -le 2;$attempt++){
 $failed=$false
 try {[void]$validate.Invoke($module,@()).GetAwaiter().GetResult()}catch{if($_.Exception.ToString() -notmatch 'Invalid transport type 99'){throw};$failed=$true}
 if(!$failed -or [NetworkRetryCounter]::Attempts -ne $attempt){throw 'Unchanged metadata was not retried after the failed join'}
}
Write-Output 'NETWORK_METADATA_RETRY_PASS: unchanged metadata attempts the join again after failure, and explicit callers receive the error.'
