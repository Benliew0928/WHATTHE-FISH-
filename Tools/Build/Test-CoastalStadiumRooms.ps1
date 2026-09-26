param([ValidateSet('Football','Basketball')][string]$Sport='Football')
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$output=Join-Path $root ('Builds\CoastalRooms-'+$Sport+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $output | Out-Null
$player=Join-Path $root 'Builds\WindowsFinal\WhatTheFish.exe'
$players=@();$port=if($Sport -eq 'Football'){7894}else{7895}
try {
 foreach($role in @('host','guest')){
  $connection=if($role -eq 'host'){'-localHost'}else{'-localClient'}
  $arguments=@('-batchmode','-nographics','-job-worker-count','2','-probe','-sport',$Sport,$connection,'-port',$port,'-expected','2','-startAt','12','-returnAt','290','-exitAfter','300','-report',("$output\$role.txt"),'-coastalNetworkReview',("$output\$role-route.txt"),'-logFile',("$output\$role.log"))
  $players+=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
  if($role -eq 'host'){Start-Sleep -Seconds 3}
 }
 $deadline=(Get-Date).AddSeconds(285)
 do {
  $complete=0
  foreach($role in @('host','guest')){
   $report=Join-Path $output "$role-route.txt"
   if((Test-Path -LiteralPath $report) -and (Select-String -LiteralPath $report -Pattern 'NETWORK_COASTAL_COMPLETE' -Quiet)){$complete++}
  }
  if($complete -eq 2){break}
  Start-Sleep -Seconds 1
 }while((Get-Date) -lt $deadline)
 if($complete -ne 2){throw "Coastal network route timeout: $output"}
 foreach($role in @('host','guest')){
  $report=Get-Content -LiteralPath (Join-Path $output "$role-route.txt") -Raw
  if($report -match '(?m)^FAIL ' -or $report -notmatch 'success=True'){throw "Coastal network route failed: $output"}
  if(Select-String -LiteralPath (Join-Path $output "$role.log") -Pattern 'Exception:|called on inactive controller' -Quiet){throw "Player error: $output"}
 }
 Write-Output "PASS $Sport host/guest coastal routes. Evidence: $output"
}finally{
 foreach($process in $players){if(!$process.HasExited){Stop-Process -Id $process.Id -ErrorAction SilentlyContinue}}
}
