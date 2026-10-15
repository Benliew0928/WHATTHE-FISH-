param([string]$PlayerPath,[int]$Port=7796)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $taskRoot 'Builds/RoomTeleportQA/Player/WhatTheFish.exe'}
$run=Join-Path $taskRoot ('Builds/RoomTeleportQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$processes=@()
try {
 foreach($role in @('Host','Guest')) {
  $folder=Join-Path $run $role
  New-Item -ItemType Directory -Force -Path $folder | Out-Null
  $arguments=@('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2',('-local'+$(if($role -eq 'Host'){'Host'}else{'Client'})),'-port',"$Port",'-roomTeleportReview',('"'+$folder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
  $processes+=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
  if($role -eq 'Host'){Start-Sleep -Seconds 2}
 }
 $deadline=(Get-Date).AddMinutes(15)
 while(($processes | Where-Object {!$_.HasExited}).Count -gt 0 -and (Get-Date) -lt $deadline){Start-Sleep -Seconds 2}
 foreach($role in @('Host','Guest')){
  $folder=Join-Path $run $role
  $result=Get-Content -LiteralPath (Join-Path $folder 'results.txt') -Raw
  if($result -match '(?m)^FAIL ' -or !$result.Contains('ROOM_TELEPORT_REVIEW_COMPLETE success=True')){throw "Room teleport review failed: $folder"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'Exception:|Assertion failed|Shader error|called on inactive controller' -Quiet){throw "Runtime error: $folder"}
  Write-Output "ROOM_TELEPORT_PASS $role assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count) $folder"
 }
 if($processes | Where-Object {$_.HasExited -and $_.ExitCode -ne 0}){throw 'A review player exited with an error.'}
} finally {foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id}}}
