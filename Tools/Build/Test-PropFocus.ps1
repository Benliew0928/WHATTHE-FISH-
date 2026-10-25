param([ValidateSet('Offline','Local')][string]$Mode='Offline',[ValidateSet('Football','Basketball')][string]$Sport='Basketball',[string]$PlayerPath,[int]$Port=7983,[int]$Width=1600,[int]$Height=900,[switch]$SpectatorHost)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){if([IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath}else{Join-Path $root $PlayerPath}}else{Join-Path $root 'Builds/PropFocusQA/Player/WhatTheFish.exe'}
$run=Join-Path $root ('Builds/PropFocusQA/'+$Mode+'-'+$Sport+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$items=@()
try {
 foreach($role in $(if($Mode -eq 'Local'){@('Host','Player','Spectator')}else{@('Offline')})){
  $folder=Join-Path $run $role;New-Item -ItemType Directory -Force -Path $folder | Out-Null
  $arguments=@('-batchmode','-force-d3d11','-screen-width',$Width,'-screen-height',$Height,'-screen-fullscreen','0','-job-worker-count','2','-sport',$Sport,'-guestProfile',((Split-Path $run -Leaf)+$role),'-propFocusReview',('"'+$folder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
  if($Mode -eq 'Offline'){$arguments+='-offline'}else{$arguments+=@($(if($role -eq 'Host'){'-localHost'}else{'-localClient'}),'-port',$Port)}
  if($SpectatorHost){$arguments+=@('-spectatorHost','yes')}
  $process=Start-Process -FilePath $player -ArgumentList $arguments -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru;$null=$process.Handle
  $items+=@{Process=$process;Folder=$folder};if($Mode -eq 'Local'){Start-Sleep -Seconds 2}
 }
 $deadline=(Get-Date).AddMinutes(5)
 while(($items | Where-Object {!$_.Process.HasExited}).Count -and (Get-Date) -lt $deadline){Start-Sleep -Milliseconds 500}
 foreach($item in $items){
  if(!$item.Process.HasExited){throw "Prop focus review timeout: $($item.Folder)"}
  $result=Get-Content -LiteralPath (Join-Path $item.Folder 'results.txt') -Raw
  if($item.Process.ExitCode -ne 0 -or $result -match '(?m)^FAIL ' -or !$result.Contains('PROP_FOCUS_COMPLETE success=True')){throw "Prop focus review failed: $($item.Folder)"}
  if(Select-String -LiteralPath (Join-Path $item.Folder 'player.log') -Pattern 'Exception:|Assertion failed|Shader error|Look rotation viewing vector is zero' -Quiet){throw "Runtime error: $($item.Folder)"}
  Write-Output "PASS $($item.Folder) assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count)"
 }
 Write-Output "PROP_FOCUS_SUITE_PASS $run"
}finally{foreach($item in $items){if(!$item.Process.HasExited){Stop-Process -Id $item.Process.Id}}}
