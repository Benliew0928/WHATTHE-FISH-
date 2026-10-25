param([string]$PlayerPath)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/VoiceQA/Player/WhatTheFish.exe'}
elseif(![IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath=Join-Path $taskRoot $PlayerPath}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$taskOutput=Join-Path $taskRoot ('Builds/VoiceQA/Online-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
$taskWave=Join-Path $taskOutput 'synthetic-voice.wav'
$taskSpeech=Start-Process -FilePath (Join-Path $env:WINDIR 'System32/WindowsPowerShell/v1.0/powershell.exe') -ArgumentList @('-NoProfile','-File',('"'+(Join-Path $PSScriptRoot 'New-VoiceTestAudio.ps1')+'"'),'-OutputPath',('"'+$taskWave+'"')) -WindowStyle Hidden -PassThru
$null=$taskSpeech.Handle;$taskSpeech.WaitForExit()
if($taskSpeech.ExitCode -ne 0 -or !(Test-Path -LiteralPath $taskWave)){throw 'Synthetic test speech could not be generated.'}
$taskRoom='voice-review-'+[guid]::NewGuid().ToString('N')
$taskPlayers=@()
try{
 foreach($role in @('A','B')){
  $folder=Join-Path $taskOutput $role;New-Item -ItemType Directory -Force -Path $folder | Out-Null
  $args=@('-batchmode','-force-d3d11','-screen-width','800','-screen-height','450','-screen-fullscreen','0','-job-worker-count','2','-offline','-guestProfile',('voice-online-'+[guid]::NewGuid().ToString('N')),'-voiceOnlineProbe',('"'+$folder+'"'),'-voiceTestRoom',$taskRoom,'-voiceInject',('"'+$taskWave+'"'),'-logFile',('"'+$folder+'/player.log"'))
  $process=Start-Process -FilePath $PlayerPath -ArgumentList $args -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru;$null=$process.Handle
  $taskPlayers+=@{process=$process;folder=$folder}
 }
 $deadline=(Get-Date).AddSeconds(210);$passed=$false
 do{
  Start-Sleep -Milliseconds 500;$states=@()
  foreach($player in $taskPlayers){
   $result=Join-Path $player.folder 'results.txt';if(Test-Path -LiteralPath $result){$text=Get-Content -LiteralPath $result -Raw;if($text.StartsWith('FAIL')){throw "Online voice failed: $($player.folder); $text"}}
   $file=Join-Path $player.folder 'state.json';if(Test-Path -LiteralPath $file){try{$states+=Get-Content -LiteralPath $file -Raw | ConvertFrom-Json}catch{}}
  }
  if($states.Count -eq 2 -and @($states | Where-Object {$_.connected -and $_.peers -eq 2 -and $_.maxRemoteEnergy -gt 0 -and !$_.micEnabled -and $_.inputMuted -and !$_.outputMuted -and $_.remoteMuted -eq 0 -and $_.transmittingChannels -eq 1}).Count -eq 2){
   if($states[0].channel -ne $states[1].channel){throw 'Peers joined different channels'}
   $passed=$true;break
  }
 }while((Get-Date) -lt $deadline -and @($taskPlayers | Where-Object {$_.process.HasExited}).Count -eq 0)
 if(!$passed){throw "Two-way native audio was not confirmed: $taskOutput"}
 foreach($player in $taskPlayers){Set-Content -LiteralPath (Join-Path $player.folder 'stop.txt') -Value 'stop'}
 foreach($player in $taskPlayers){if(!$player.process.WaitForExit(15000) -or $player.process.ExitCode -ne 0){throw "Voice player did not stop cleanly: $($player.folder)"}}
 $states | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $taskOutput 'two-way-audio.json')
 'VOICE_ONLINE_PASS: two native clients, one channel, synthetic audio received both ways, capture stayed muted.' | Set-Content -LiteralPath (Join-Path $taskOutput 'summary.txt')
 "VOICE_ONLINE_SUITE_PASS $taskOutput"
}finally{
 foreach($player in $taskPlayers){if(!$player.process.HasExited){Stop-Process -Id $player.process.Id}}
}
