param([Parameter(Mandatory)][string]$PreviousPlayerPath,[string]$PlayerPath,[int]$Port=7894)
$ErrorActionPreference='Stop'
$repoRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$current=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $repoRoot 'Builds/TravelMatchQA/Player/WhatTheFish.exe'}
$previous=(Resolve-Path -LiteralPath $PreviousPlayerPath).Path
$run=Join-Path $repoRoot ('Builds/TravelMatchQA/Compatibility-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$results=@()
foreach($direction in @('CurrentHost','PreviousHost')){
 $running=@()
 try {
  $case=Join-Path $run $direction
  foreach($role in @('Host','Guest')){
   $folder=Join-Path $case $role
   New-Item -ItemType Directory -Force -Path $folder | Out-Null
   $usesCurrent=($direction -eq 'CurrentHost') -eq ($role -eq 'Host')
   $player=if($usesCurrent){$current}else{$previous}
   $arguments=@('-batchmode','-force-d3d11','-screen-width','640','-screen-height','360','-screen-fullscreen','0','-job-worker-count','1',$(if($role -eq 'Host'){'-localHost'}else{'-localClient'}),'-port',"$Port",'-roomCapacityProbe',('"'+$folder+'"'),'-contentCache',('"'+(Join-Path $folder 'ContentCache')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
   $running+=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
   if($role -eq 'Host'){Start-Sleep -Seconds 2}
  }
  $deadline=(Get-Date).AddSeconds(100)
  $hostLog=Join-Path $case 'Host/player.log'
  do {Start-Sleep -Seconds 2; $mismatch=(Test-Path -LiteralPath $hostLog) -and (Select-String -LiteralPath $hostLog -SimpleMatch 'NetworkConfig mismatch' -Quiet)} while(!$mismatch -and (Get-Date) -lt $deadline)
  if(!$mismatch){throw "No protocol rejection observed: $case"}
  Start-Sleep -Seconds 4
  $hostRoster=Get-Content -LiteralPath (Join-Path $case 'Host/roster.txt') -Raw
  $guestRoster=Get-Content -LiteralPath (Join-Path $case 'Guest/roster.txt') -Raw
  if($hostRoster -notmatch '^count=1 ' -or $guestRoster -notmatch '^count=0 ' -or $guestRoster -notmatch 'connected=False'){throw "Mismatched client was not cleanly rejected: $case"}
  if($direction -eq 'PreviousHost' -and $guestRoster -notmatch 'same game build'){throw 'Current client did not explain the matching-build requirement.'}
  $results+=[pscustomobject]@{direction=$direction;rejected=$true;hostPlayers=1;guestPlayers=0}
  Write-Output "COMPATIBILITY_PASS ${direction}: incompatible client rejected before player spawn"
 } finally {foreach($process in $running){if(!$process.HasExited){Stop-Process -Id $process.Id}}}
 $Port++
}
$results | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $run 'results.json')
