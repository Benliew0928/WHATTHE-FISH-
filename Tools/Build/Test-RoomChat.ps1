param([string]$PlayerPath,[int]$Port=7982,[switch]$UIOnly,[switch]$IslandUI)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$run=Join-Path $taskRoot ('Builds/RoomChatQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+$Port)
$peers=@{};$script:sequence=0;$script:checks=0
function Launch([string]$Name,[switch]$HostPeer){
 $folder=Join-Path $run $Name;New-Item -ItemType Directory -Force $folder | Out-Null
 $argsList=@('-batchmode','-force-d3d11','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-job-worker-count','2',$(if($HostPeer){'-localHost'}else{'-localClient'}),'-port',"$Port",'-roomChatProbe',('"'+$folder+'"'),'-contentCache',('"'+(Join-Path $taskRoot 'Builds/RoomChatQA/ContentCache')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
 $peers[$Name]=Start-Process -FilePath $player -ArgumentList $argsList -WindowStyle Hidden -PassThru
}
function State([string]$Name){
 if($peers[$Name].HasExited){throw "Unexpected exit: $Name"}
 $file=Join-Path $run "$Name/state.json";if(![IO.File]::Exists($file)){return $null}
 try{return [IO.File]::ReadAllText($file) | ConvertFrom-Json}catch [System.IO.IOException]{return $null}
}
function Await([string]$Name,[scriptblock]$Condition,[string]$Label,[int]$Seconds=120){
 $deadline=(Get-Date).AddSeconds($Seconds)
 while((Get-Date) -lt $deadline){$s=State $Name;if($s -and (& $Condition $s)){$script:checks++;Add-Content (Join-Path $run 'checks.txt') "PASS $Name $Label";return $s};Start-Sleep -Milliseconds 300}
 throw "Timeout: $Name $Label. Evidence: $run"
}
function Command([string]$Name,[string]$Value,[int]$Seconds=120){
 $script:sequence++;$valueWithId="$script:sequence|$Value";$target=Join-Path $run "$Name/command.txt";$temp=$target+'.pending'
 [IO.File]::WriteAllText($temp,$valueWithId);if([IO.File]::Exists($target)){[IO.File]::Replace($temp,$target,[NullString]::Value)}else{[IO.File]::Move($temp,$target)}
 if($Value -eq 'quit'){return}
 [void](Await $Name {param($s) $s.command -eq $valueWithId -and !$s.busy} $Value $Seconds)
}
function Has([string]$Name,[string]$Text){[void](Await $Name {param($s) @($s.messages | Where-Object text -eq $Text).Count -eq 1} "exactly one '$Text'")}
try {
 Launch host -HostPeer;[void](Await host {param($s)$s.ready} 'host chat ready' 240)
 if($IslandUI){
  Command host 'nick:Harbor';$room=(State host).room
  foreach($sport in @('Basketball','Golf','Fishing')){
   Command host "sport:$sport";[void](Await host {param($s)$s.sport -eq $sport -and $s.ready} "ready for $sport" 180)
   Command host 'explore:true' 180;[void](Await host {param($s)$s.exploring -and $s.ready} "entered $sport" 180)
   Start-Sleep -Seconds 20;Command host "text:Hello from $sport";Command host 'ui-review';Command host 'tray';Command host "capture:$sport-history"
   [void](Await host {param($s)$s.room -eq $room} "same room in $sport")
   Command host 'explore:false' 120;[void](Await host {param($s)!$s.exploring -and $s.ready} "returned from $sport" 120)
  }
  Command host 'leave-new-room';Command host 'quit';Start-Sleep -Seconds 2
 } elseif($UIOnly){
  Command host 'nick:Harbor';Command host 'typing';Command host 'ui-review';Command host 'tray';Command host 'explore:true' 180
  [void](Await host {param($s)$s.exploring -and $s.ready} 'entered gameplay' 180)
  Command host 'bubbles';Command host 'ui-review';Command host 'tray';Command host 'capture:game-history';Command host 'explore:false' 120
  Command host 'leave-new-room';Command host 'quit';Start-Sleep -Seconds 2
 } else {
 Command host 'nick:Harbor';Command host 'text:Hello from the room';Has host 'Hello from the room'
 Launch guest;[void](Await guest {param($s)$s.ready -and $s.count -eq 1} 'late join receives history' 240)
 Command guest 'nick:Coral';Command guest 'typing';Command guest 'text:<b>GG</b>';Has host '<b>GG</b>'
 Start-Sleep -Milliseconds 1300;Command guest 'rate';Start-Sleep -Milliseconds 1300;Command guest 'invalid';Command guest 'dedup';Command guest 'mute:Hello from the room'
 foreach($sticker in @('ggwp','laugh','cry','angry','thanks','oops','wow','lets-go')){Start-Sleep -Milliseconds 1300;Command guest "sticker:$sticker";[void](Await host {param($s)@($s.messages | Where-Object sticker -eq $sticker).Count -eq 1} "sticker $sticker replicated")}
 Command host 'burst-history' 70
 Launch spectator;[void](Await spectator {param($s)$s.ready -and $s.count -gt 32} 'late join receives multiple history pages' 240)
 Command spectator 'nick:Reef';Command spectator 'seat:10';[void](Await spectator {param($s)$s.spectator} 'spectator seat')
 Command spectator 'text:Spectators can chat too';Has host 'Spectators can chat too';Has guest 'Spectators can chat too'
 Command guest 'ui-review';Command guest 'capture:lobby-history'
 $before=State host;$originalRoom=$before.room
 foreach($sport in @('Basketball','Golf','Fishing','Football')){Command host "sport:$sport";[void](Await guest {param($s)$s.sport -eq $sport -and $s.ready -and $s.room -eq $originalRoom -and $s.count -eq $before.count} "shared timeline survives $sport selection" 180)}
 Command host 'explore:true' 180;[void](Await guest {param($s)$s.exploring -and $s.ready} 'entered gameplay' 180)
 Start-Sleep -Seconds 4;Command guest 'bubbles';Command guest 'ui-review';Command guest 'tray';Command guest 'capture:game-history';Has host "Nice pass! Let's go!"
 Command host 'explore:false' 120;[void](Await guest {param($s)!$s.exploring -and $s.ready} 'returned to lobby' 120)
 $count=(State host).count
 Stop-Process -Id $peers.host.Id
 [void](Await guest {param($s)$s.host -and $s.ready -and !$s.recovering -and $s.count -eq $count -and $s.room -eq $originalRoom} 'host migration retains room and full history' 180)
 [void](Await spectator {param($s)$s.ready -and !$s.recovering -and $s.count -eq $count -and $s.room -eq $originalRoom} 'survivor reconnect retains history' 180)
 Command guest 'text:Still the same crew';Has spectator 'Still the same crew';Command spectator 'text:And all our messages';Has guest 'And all our messages'
 Command spectator 'capture:after-host-recovery';Command spectator 'quit';Start-Sleep -Seconds 2
 Command guest 'leave-new-room';Command guest 'quit';Start-Sleep -Seconds 2
 }
 $failures=Get-ChildItem $run -Filter results.txt -Recurse | Select-String '^FAIL '
 if($failures){throw ($failures | Out-String)}
 $errors=Get-ChildItem $run -Filter player.log -Recurse | Select-String 'Exception:|Assertion failed|error CS|Shader error'
 if($errors){throw ($errors | Select-Object -First 8 | Out-String)}
 $total=@(Get-ChildItem $run -Filter results.txt -Recurse | Select-String '^PASS ').Count+$script:checks
 "ROOM_CHAT_COMPLETE assertions=$total" | Set-Content (Join-Path $run 'complete.txt')
 Write-Output "$run : $total assertions passed"
} finally {foreach($p in $peers.Values){if(!$p.HasExited){Stop-Process -Id $p.Id -ErrorAction SilentlyContinue}}}
