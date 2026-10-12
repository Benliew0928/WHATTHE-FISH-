param([ValidateSet('Offline','Local')][string]$Mode='Offline',[int]$Port=7983,[string]$PlayerPath)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(-not $PlayerPath){$PlayerPath=Join-Path $root 'Builds/FootballAudioQA/Player/WhatTheFish.exe'}
$out=Join-Path $root ('Builds/FootballAudioQA/'+$Mode+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$processes=@();$folders=@()
try {
 foreach($role in $(if($Mode -eq 'Local'){@('host','guest')}else{@('offline')})){
  $folder=Join-Path $out $role;New-Item -ItemType Directory -Path $folder -Force | Out-Null;$folders+=$folder
  $arguments=@('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-sport','Football','-footballAudioReview',('"'+$folder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
  if($role -eq 'offline'){$arguments+='-offline'}else{$arguments+=@('-probe',$(if($role -eq 'host'){'-localHost'}else{'-localClient'}),'-port',$Port,'-expected','2','-startAt','10','-returnAt','180','-report',('"'+(Join-Path $folder 'probe.txt')+'"'))}
  $processes+=Start-Process -FilePath $PlayerPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
  if($role -eq 'host'){Start-Sleep -Seconds 2}
 }
 $deadline=(Get-Date).AddSeconds(170)
 do {
  $done=@($folders | Where-Object {Test-Path (Join-Path $_ 'results.txt')} | Where-Object {Select-String -LiteralPath (Join-Path $_ 'results.txt') -Pattern 'FOOTBALL_AUDIO_COMPLETE' -Quiet})
  if($done.Count -eq $folders.Count){break}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $deadline)
 if($done.Count -ne $folders.Count){throw "Audio review timed out: $out"}
 foreach($folder in $folders){
  Get-Content -LiteralPath (Join-Path $folder 'results.txt')
  if(Select-String -LiteralPath (Join-Path $folder 'results.txt') -Pattern '^FAIL|success=False' -Quiet){throw "Audio review failed: $folder"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'Exception:|error CS|Error:|MissingReferenceException' -Quiet){throw "Audio runtime diagnostic: $folder"}
 }
 Write-Output "FOOTBALL_AUDIO_SUITE_PASS $out"
}finally{foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id}};Write-Output "Evidence: $out"}
