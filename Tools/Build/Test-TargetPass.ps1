param([ValidateSet('Basketball','Football')][string]$Sport='Basketball',[ValidateSet('Offline','Local')][string]$Mode='Offline',[string]$PlayerPath,[int]$Port=8057)
$ErrorActionPreference='Stop'
$passRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$passPlayer=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $passRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$passRun=Join-Path $passRoot ('Builds/TargetPassQA/'+$Sport+'-'+$Mode+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$running=@()
try {
 foreach($role in $(if($Mode -eq 'Local'){@('host','guest')}else{@('offline')})){
  $folder=Join-Path $passRun $role;New-Item -ItemType Directory -Force -Path $folder | Out-Null
  $arguments=@('-batchmode','-force-d3d11','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-job-worker-count','2','-probe','-sport',$Sport,'-targetPassReview',('"'+$folder+'"'),'-report',('"'+(Join-Path $folder 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
  if($role -eq 'offline'){$arguments+='-offline'}else{$arguments+=@($(if($role -eq 'host'){'-localHost'}else{'-localClient'}),'-expected','2','-startAt','8','-returnAt','250','-port',$Port)}
  $process=Start-Process -FilePath $passPlayer -ArgumentList $arguments -WindowStyle Hidden -PassThru;$running+=@{process=$process;folder=$folder}
  if($role -eq 'host'){Start-Sleep -Seconds 2}
 }
 $deadline=(Get-Date).AddSeconds(200)
 while(@($running | Where-Object {!$_.process.HasExited}).Count -gt 0 -and (Get-Date) -lt $deadline){Start-Sleep -Milliseconds 500}
 foreach($item in $running){
  if(!$item.process.HasExited){throw "Target pass review timed out: $($item.folder)"}
  $result=Get-Content -LiteralPath (Join-Path $item.folder 'results.txt') -Raw
  if($item.process.ExitCode -ne 0 -or $result -match '(?m)^FAIL ' -or !$result.Contains('TARGET_PASS_COMPLETE success=True')){throw "Target pass review failed: $($item.folder)"}
  if(Select-String -LiteralPath (Join-Path $item.folder 'player.log') -Pattern 'Exception:|Assertion failed' -Quiet){throw "Player exception: $($item.folder)"}
  Write-Output "PASS $($item.folder) assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count)"
 }
}finally{foreach($item in $running){if(!$item.process.HasExited){Stop-Process -Id $item.process.Id}};Write-Output "Evidence: $passRun"}
