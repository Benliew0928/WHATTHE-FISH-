param([string]$PlayerPath,[ValidateSet('All','Football','Basketball','Golf','Fishing')][string]$Sport='All',[ValidateRange(1024,65531)][int]$Port=8935)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/VoiceQA/Player/WhatTheFish.exe'}
elseif(![IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath=Join-Path $taskRoot $PlayerPath}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$taskOutput=Join-Path $taskRoot ('Builds/VoiceQA/UI-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$taskSports=if($Sport -eq 'All'){@('Football','Basketball','Golf','Fishing')}else{@($Sport)}
$taskPort=$Port
foreach($taskSport in $taskSports){
 $taskFolder=Join-Path $taskOutput $taskSport
 New-Item -ItemType Directory -Force -Path $taskFolder | Out-Null
 $taskArgs=@('-batchmode','-force-d3d11','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-job-worker-count','2','-localHost','-port',$taskPort,'-sport',$taskSport,'-probe','-startAt','10000','-returnAt','10000','-exitAfter','240','-guestProfile',('voice'+[guid]::NewGuid().ToString('N')),'-voiceReview',('"'+$taskFolder+'"'),'-report',('"'+$taskFolder+'/probe.txt"'),'-logFile',('"'+$taskFolder+'/player.log"'))
 $taskProcess=Start-Process -FilePath $PlayerPath -ArgumentList $taskArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
 $null=$taskProcess.Handle
 try{
  $taskDeadline=(Get-Date).AddSeconds(240);$taskReport=Join-Path $taskFolder 'results.txt'
  do{Start-Sleep -Milliseconds 500;if(Test-Path -LiteralPath $taskReport){$taskText=Get-Content -LiteralPath $taskReport -Raw;if($taskText -match '(?m)^FAIL '){throw "Voice UI review failed: $taskFolder"};if($taskText -match 'VOICE_UI_PASS'){break}}}while(!$taskProcess.HasExited -and (Get-Date) -lt $taskDeadline)
  if(!(Test-Path -LiteralPath $taskReport) -or (Get-Content -LiteralPath $taskReport -Raw) -notmatch 'VOICE_UI_PASS'){throw "Voice UI review incomplete: $taskFolder"}
  if(!$taskProcess.WaitForExit(10000) -or $taskProcess.ExitCode -ne 0){throw "Voice player did not exit normally: $taskFolder"}
  if((Get-Content -LiteralPath (Join-Path $taskFolder 'player.log') -Raw) -match 'NullReferenceException|MissingReferenceException|InvalidOperationException|CONTENT_LOAD_FAILED'){throw "Voice runtime error: $taskFolder"}
  "$taskSport $(Get-Content -LiteralPath $taskReport -Tail 1)" | Tee-Object -FilePath (Join-Path $taskOutput 'summary.txt') -Append
 }finally{if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}
 $taskPort++
}
"VOICE_UI_SUITE_PASS $taskOutput"
