param([string]$PlayerPath,[int]$Port=7796,[string]$ContentCache='',[string]$OutputPath='')
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
function Resolve-TeleportReviewPath([string]$Value){if([IO.Path]::IsPathRooted($Value)){return [IO.Path]::GetFullPath($Value)};return [IO.Path]::GetFullPath((Join-Path $taskRoot $Value))}
$player=if($PlayerPath){Resolve-TeleportReviewPath $PlayerPath}else{Join-Path $taskRoot 'Builds/RoomTeleportQA/Player/WhatTheFish.exe'}
$run=if($OutputPath){Resolve-TeleportReviewPath $OutputPath}else{Join-Path $taskRoot ('Builds/RoomTeleportQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))}
if(Test-Path -LiteralPath $run){throw 'Choose a new evidence folder.'}
$cache=if($ContentCache){Resolve-TeleportReviewPath $ContentCache}else{Join-Path $run 'ContentCache'}
$processes=@()
try {
 foreach($role in @('Host','Guest')) {
  $folder=Join-Path $run $role
  New-Item -ItemType Directory -Force -Path $folder | Out-Null
  $arguments=@('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2',('-local'+$(if($role -eq 'Host'){'Host'}else{'Client'})),'-port',"$Port",'-roomTeleportReview',('"'+$folder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
  # Parallel reviews of different player versions must not prune each other's
  # packaged-content cache while scenes are still streaming from those files.
  # A shared override is appropriate when both reviews use the same catalog.
  $arguments+=@('-contentCache',('"'+$cache+'"'))
  $processes+=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
  if($role -eq 'Host'){Start-Sleep -Seconds 2}
 }
 $deadline=(Get-Date).AddMinutes(15)
 while(($processes | Where-Object {!$_.HasExited}).Count -gt 0 -and (Get-Date) -lt $deadline){Start-Sleep -Seconds 2}
 foreach($role in @('Host','Guest')){
  $folder=Join-Path $run $role
  $result=Get-Content -LiteralPath (Join-Path $folder 'results.txt') -Raw
  if($result -match '(?m)^FAIL ' -or !$result.Contains('ROOM_TELEPORT_REVIEW_COMPLETE success=True')){throw "Room teleport review failed: $folder"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'GameObjects can not be made active|being destroyed|Exception:|Assertion failed|Shader error|called on inactive controller' -Quiet){throw "Runtime error: $folder"}
  Write-Output "ROOM_TELEPORT_PASS $role assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count) $folder"
 }
 if($processes | Where-Object {$_.HasExited -and $_.ExitCode -ne 0}){throw 'A review player exited with an error.'}
} finally {foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id}}}
