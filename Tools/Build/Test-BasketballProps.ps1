param([ValidateSet('Offline','Local')][string]$Mode='Offline',[int]$Port=7939,[string]$PlayerPath,[switch]$NoCapture,[switch]$StadiumCamera)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'}
$out=Join-Path $root ('Builds/BasketballPropsQA/'+$Mode+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$processes=@();$folders=@()
try {
 foreach($role in $(if($Mode -eq 'Local'){@('host','guest','teammate')}else{@('offline')})){
  $folder=Join-Path $out $role;New-Item -ItemType Directory -Path $folder | Out-Null;$folders+=$folder
  $arguments=@('-batchmode','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-job-worker-count','2','-probe','-sport','Basketball','-basketballPropReview',('"'+$folder+'"'),'-report',('"'+(Join-Path $folder 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'),'-exitAfter','330')
  if($NoCapture){$arguments+='-nographics'}
  if($StadiumCamera){$arguments+=@('-stadiumCameraMonitor',('"'+(Join-Path $folder 'camera.json')+'"'))}
  if($role -eq 'offline'){$arguments+='-offline'}else{$arguments+=@($(if($role -eq 'host'){'-localHost'}else{'-localClient'}),'-port',$Port,'-expected','3','-startAt','12','-returnAt','320')}
  $processes+=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
  if($role -ne 'offline'){Start-Sleep -Seconds 2}
 }
 $deadline=(Get-Date).AddSeconds(310)
 do {
  $done=@($folders | Where-Object {$result=Join-Path $_ 'results.txt';(Test-Path -LiteralPath $result) -and (Select-String -LiteralPath $result -Pattern 'BASKETBALL_PROPS_COMPLETE' -Quiet)})
  if($done.Count -eq $folders.Count){break}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $deadline)
 if($done.Count -ne $folders.Count){throw "Basketball props timed out: $out"}
 foreach($folder in $folders){
  $result=Join-Path $folder 'results.txt';Get-Content -LiteralPath $result
  if(Select-String -LiteralPath $result -Pattern '^FAIL|success=False' -Quiet){throw "Basketball props failed: $result"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'Exception:|Look rotation viewing vector is zero|Setting linear velocity of a kinematic body' -Quiet){throw "Runtime diagnostic: $folder"}
 }
 if($StadiumCamera){foreach($folder in $folders){
  $camera=Get-Content -Raw -LiteralPath (Join-Path $folder 'camera.json') | ConvertFrom-Json
  if($camera.frames -lt 100 -or $camera.flightFrames -lt 10 -or $camera.heldFrames -lt 10 -or $camera.offscreenFrames -lt 10 -or $camera.ballFailures -ne 0 -or $camera.arrowFailures -ne 0){throw "Stadium camera monitor failed: $folder"}
  Write-Output ("STADIUM_NETWORK_CAMERA_PASS "+($camera | ConvertTo-Json -Compress))
 }}
 Write-Output "BASKETBALL_PROPS_SUITE_PASS $out"
}finally{foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id}};Write-Output "Evidence: $out"}
