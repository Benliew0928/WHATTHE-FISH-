param([string]$PlayerPath,[ValidateSet('Football','Basketball','Golf','Fishing','All')][string]$Sport='All')
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/ModerationQA/Player/WhatTheFish.exe'}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$run=Join-Path $taskRoot ('Builds/ModerationQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$sports=if($Sport -eq 'All'){@('Football','Basketball','Golf','Fishing')}else{@($Sport)}
foreach($item in $sports){
 $folder=Join-Path $run $item;New-Item -ItemType Directory -Force -Path $folder | Out-Null
 $argsList=@('-batchmode','-force-d3d11','-job-worker-count','2','-skipWelcome','-guestProfile','moderation-review','-sport',$item,'-offline','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-moderationReview',('"'+$folder+'"'),'-logFile',('"'+$folder+'/player.log"'),'-contentCache',('"'+(Join-Path $taskRoot 'Builds/ModerationQA/ContentCache')+'"'),'-localizationProfile','moderation-review')
 $process=Start-Process -FilePath $PlayerPath -ArgumentList $argsList -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru;$null=$process.Handle
 try{
  if(!$process.WaitForExit(240000)){throw "Review timed out: $folder"}
  $result=Get-Content -LiteralPath (Join-Path $folder 'results.txt') -Raw
  if($process.ExitCode -ne 0 -or $result -match '(?m)^FAIL ' -or !$result.Contains('MODERATION_REVIEW_COMPLETE success=True')){throw "Review failed: $folder"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException|ArgumentException' -Quiet){throw "Runtime exception: $folder"}
  Write-Output "PASS $item assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count) evidence=$folder"
 }finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
}
