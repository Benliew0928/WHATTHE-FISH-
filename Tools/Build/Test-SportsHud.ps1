param([ValidateSet('Offline','Local')][string]$Mode='Offline',[string]$PlayerPath,[switch]$Capture,[int]$Width=1280,[int]$Height=720,[int]$Port=7963)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'}
$output=Join-Path $root ('Builds/SportsHudQA/Reviews/'+$Mode+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force -Path $output | Out-Null
$processes=@();$folders=@()
try {
 foreach($role in $(if($Mode -eq 'Local'){@('host','guest1','guest2')}else{@('offline')})){
  $folder=Join-Path $output $role;New-Item -ItemType Directory -Path $folder | Out-Null;$folders+=$folder
  $arguments=@('-batchmode','-screen-width',$Width,'-screen-height',$Height,'-screen-fullscreen','0','-job-worker-count','2','-probe','-sportsHudReview',('"'+$folder+'"'),'-report',('"'+(Join-Path $folder 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'),'-exitAfter','160')
  if($Capture){$arguments+='-hudCapture'}else{$arguments+='-nographics'}
  if($role -eq 'offline'){$arguments+='-offline'}else{$arguments+=@('-sport','Basketball',$(if($role -eq 'host'){'-localHost'}else{'-localClient'}),'-port',$Port,'-expected','3','-startAt','12','-returnAt','155')}
  $processes+=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
  if($Mode -eq 'Local'){Start-Sleep -Seconds 3}
 }
 $deadline=(Get-Date).AddSeconds(180)
 do {
  $done=@($folders | Where-Object {$result=Join-Path $_ 'results.txt';(Test-Path -LiteralPath $result) -and (Select-String -LiteralPath $result -Pattern 'SPORTS_HUD_COMPLETE' -Quiet)})
  if($done.Count -eq $folders.Count){break}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $deadline)
 if($done.Count -ne $folders.Count){throw "Sports HUD review timed out: $output"}
 foreach($folder in $folders){
  $result=Join-Path $folder 'results.txt';Get-Content -LiteralPath $result
  if(Select-String -LiteralPath $result -Pattern '^FAIL|success=False' -Quiet){throw "Sports HUD review failed: $result"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'Exception:|Look rotation viewing vector is zero' -Quiet){throw "Player exception: $folder"}
 }
 Write-Output "SPORTS_HUD_SUITE_PASS $output"
}finally{foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id}};Write-Output "Evidence: $output"}
