param([ValidateSet('Offline','Local')][string]$Mode='Offline',[int]$Port=7957,[string]$PlayerPath,[int]$Width=1280,[int]$Height=720)
$ErrorActionPreference='Stop'
$trailRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$trailPlayer=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $trailRoot 'Builds/SportsTrailQA/Player/WhatTheFish.exe'}
$trailRun=Join-Path $trailRoot ('Builds/SportsTrailQA/'+$Mode+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force -Path $trailRun | Out-Null
$trailProcesses=@();$trailFolders=@()
try {
 foreach($role in $(if($Mode -eq 'Local'){@('host','guest')}else{@('offline')})){
  $folder=Join-Path $trailRun $role;New-Item -ItemType Directory -Path $folder | Out-Null;$trailFolders+=$folder
  $arguments=@('-batchmode','-force-d3d11','-screen-width',"$Width",'-screen-height',"$Height",'-screen-fullscreen','0','-job-worker-count','2','-probe','-sport','Basketball','-basketballTrailReview',('"'+$folder+'"'),'-report',('"'+(Join-Path $folder 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'),'-exitAfter','160')
  if($role -eq 'offline'){$arguments+='-offline'}else{$arguments+=@($(if($role -eq 'host'){'-localHost'}else{'-localClient'}),'-port',$Port,'-expected','2','-startAt','12','-returnAt','150')}
  $trailProcesses+=Start-Process -FilePath $trailPlayer -ArgumentList $arguments -WindowStyle Hidden -PassThru
  if($role -eq 'host'){Start-Sleep -Seconds 3}
 }
 foreach($process in $trailProcesses){if(!$process.WaitForExit(180000)){throw 'Basketball trail review timed out.'}}
 foreach($folder in $trailFolders){
  $result=Get-Content -LiteralPath (Join-Path $folder 'results.txt') -Raw
  if($result -match '(?m)^FAIL ' -or !$result.Contains('BASKETBALL_TRAIL_COMPLETE success=True')){throw "Basketball trail review failed: $folder"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'Exception:|Assertion failed|Shader error' -Quiet){throw "Runtime error: $folder"}
  Write-Output "BASKETBALL_TRAIL_SUITE_PASS $folder assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count)"
 }
}finally{foreach($process in $trailProcesses){if(!$process.HasExited){Stop-Process -Id $process.Id}}}
