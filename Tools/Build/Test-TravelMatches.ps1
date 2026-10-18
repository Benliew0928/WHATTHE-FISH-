param([string]$PlayerPath,[int]$Port=7892)
$ErrorActionPreference='Stop'
$repoRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $repoRoot 'Builds/TravelMatchQA/Player/WhatTheFish.exe'}
$run=Join-Path $repoRoot ('Builds/TravelMatchQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$processes=@()
try {
 foreach($role in @('Host','Player','Spectator')){
  $folder=Join-Path $run $role
  New-Item -ItemType Directory -Force -Path $folder | Out-Null
  $arguments=@('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2',$(if($role -eq 'Host'){'-localHost'}else{'-localClient'}),'-port',"$Port",'-travelMatchReview',('"'+$folder+'"'),'-contentCache',('"'+(Join-Path $run 'ContentCache')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
  $processes+=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
  Start-Sleep -Seconds 2
 }
 $deadline=(Get-Date).AddMinutes(12)
 while(($processes | Where-Object {!$_.HasExited}).Count -gt 0 -and (Get-Date) -lt $deadline){Start-Sleep -Seconds 2}
 foreach($role in @('Host','Player','Spectator')){
  $folder=Join-Path $run $role
  $result=Get-Content -LiteralPath (Join-Path $folder 'results.txt') -Raw
  if($result -match '(?m)^FAIL ' -or !$result.Contains('TRAVEL_MATCH_REVIEW_COMPLETE success=True')){throw "Travel match review failed: $folder"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'Exception:|Assertion failed|Shader error|called on inactive controller' -Quiet){throw "Runtime error: $folder"}
  Write-Output "TRAVEL_MATCH_PASS $role assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count) $folder"
 }
 if($processes | Where-Object {$_.HasExited -and $_.ExitCode -ne 0}){throw 'A review player exited with an error.'}
} finally {foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id}}}
