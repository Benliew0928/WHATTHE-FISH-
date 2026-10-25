param([string]$PlayerPath,[ValidateSet('All','Golf','Fishing')][string]$Sport='All',[int]$Width=960,[int]$Height=540)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'}
$run=Join-Path $root ('Builds/FirstPersonBodyQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$sports=if($Sport -eq 'All'){@('Golf','Fishing')}else{@($Sport)}
foreach($selected in $sports){
 $folder=Join-Path $run $selected;New-Item -ItemType Directory -Force $folder | Out-Null
 $profile=(Split-Path $run -Leaf)+$selected
 $arguments=@('-batchmode','-force-d3d11','-screen-width',$Width,'-screen-height',$Height,'-screen-fullscreen','0','-job-worker-count','2','-offline','-sport',$selected,'-language','en','-guestProfile',$profile,'-localizationProfile',$profile,'-firstPersonBodyReview',('"'+$folder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
 $process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
 $null=$process.Handle
 try {
  if(!$process.WaitForExit(240000)){throw "Review timed out: $folder"}
  $text=Get-Content -LiteralPath (Join-Path $folder 'results.txt') -Raw
  if($process.ExitCode -ne 0 -or $text -match '(?m)^FAIL ' -or $text -notmatch 'FIRST_PERSON_BODY_COMPLETE checks=\d+ success=True'){throw "First-person review failed: $folder"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'Exception:|Assertion failed|Shader error' -Quiet){throw "Runtime diagnostic: $folder"}
  Write-Output "PASS $folder assertions=$(([regex]::Matches($text,'(?m)^PASS ')).Count)"
 }finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
}
Write-Output "FIRST_PERSON_BODY_SUITE_PASS $run"
