param([ValidateSet('Golf','Fishing')][string]$Sport='Golf')
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$output=Join-Path $root ('Builds\RefinedRooms-'+$Sport+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $output | Out-Null
$player=Join-Path $root 'Builds\WindowsFinal\WhatTheFish.exe';$players=@();$port=if($Sport -eq 'Golf'){7904}else{7905}
try {
 foreach($role in @('host','guest')){
  $connection=if($role -eq 'host'){'-localHost'}else{'-localClient'}
  $arguments=@('-batchmode','-nographics','-job-worker-count','2','-probe','-sport',$Sport,$connection,'-port',$port,'-expected','2','-startAt','15','-returnAt','690','-exitAfter','700','-report',("$output\$role.txt"),'-islandNetworkReview',("$output\$role-route.txt"),'-logFile',("$output\$role.log"))
  $players+=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
  if($role -eq 'host'){Start-Sleep -Seconds 3}
 }
 $deadline=(Get-Date).AddSeconds(680)
 do {
  $complete=0
  foreach($role in @('host','guest')){$report=Join-Path $output "$role-route.txt";if((Test-Path -LiteralPath $report) -and (Select-String -LiteralPath $report -Pattern 'ISLAND_NETWORK_COMPLETE' -Quiet)){$complete++}}
  if($complete -eq 2){break};Start-Sleep -Seconds 1
 }while((Get-Date) -lt $deadline)
 if($complete -ne 2){throw "Island network route timeout: $output"}
 foreach($role in @('host','guest')){
  $report=Get-Content -LiteralPath (Join-Path $output "$role-route.txt") -Raw
  if($report -match '(?m)^FAIL ' -or $report -notmatch 'success=True'){throw "Island network route failed: $output"}
  if(Select-String -LiteralPath (Join-Path $output "$role.log") -Pattern 'Exception:|called on inactive controller' -Quiet){throw "Player error: $output"}
 }
 Write-Output "PASS $Sport host/guest routes. Evidence: $output"
}finally{foreach($process in $players){if(!$process.HasExited){Stop-Process -Id $process.Id -ErrorAction SilentlyContinue}}}
