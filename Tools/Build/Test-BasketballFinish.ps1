param([ValidateSet('Offline','Local')][string]$Mode='Offline',[int]$Port=7899,[string]$PlayerPath,[switch]$NoCapture,[switch]$GestureOnly,[ValidateSet(20,30,60,120)][int]$Fps=30)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'}
$out=Join-Path $root ('Builds/BasketballFinishQA/Reviews/'+$Mode+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$processes=@();$folders=@()
try {
 foreach($role in $(if($Mode -eq 'Local'){@('host','guest')}else{@('offline')})){
  $folder=Join-Path $out $role;New-Item -ItemType Directory -Path $folder | Out-Null;$folders+=$folder
  $arguments=@('-batchmode','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-probe','-sport','Basketball','-basketballFinishReview',('"'+$folder+'"'),'-report',('"'+(Join-Path $folder 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'),'-exitAfter','180','-finishRate',$Fps)
  if($NoCapture){$arguments+=@('-nographics','-finishNoCapture')}
  if($GestureOnly){$arguments+='-finishGestureOnly'}
  if($role -eq 'offline'){$arguments+='-offline'}else{$arguments+=@($(if($role -eq 'host'){'-localHost'}else{'-localClient'}),'-port',$Port,'-expected','2','-startAt','12','-returnAt','170')}
  $processes+=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
  if($role -eq 'host'){Start-Sleep -Seconds 3}
 }
 $deadline=(Get-Date).AddSeconds(210)
 do {
  $done=@($folders | Where-Object {$result=Join-Path $_ 'results.txt';(Test-Path -LiteralPath $result) -and (Select-String -LiteralPath $result -Pattern 'BASKETBALL_FINISH_COMPLETE' -Quiet)})
  if($done.Count -eq $folders.Count){break}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $deadline)
 if($done.Count -ne $folders.Count){throw "Basketball finish review timed out: $out"}
 foreach($folder in $folders){
  $result=Join-Path $folder 'results.txt';Get-Content -LiteralPath $result
  if(Select-String -LiteralPath $result -Pattern '^FAIL|success=False' -Quiet){throw "Basketball finish check failed: $result"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'Exception:|Only the server can find player|Look rotation viewing vector is zero' -Quiet){throw "Player exception: $folder"}
 }
 Write-Output "BASKETBALL_FINISH_SUITE_PASS $out"
}finally{foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id}};Write-Output "Evidence: $out"}
