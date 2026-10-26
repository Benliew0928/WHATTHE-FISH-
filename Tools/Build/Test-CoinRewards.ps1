param([string]$PlayerPath='Builds/CoinRewardsQA/Player/WhatTheFish.exe',[string]$OutputPath='', [switch]$Single)
$ErrorActionPreference='Stop'
$rewardRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(![IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath=Join-Path $rewardRoot $PlayerPath}
if(!$OutputPath){$OutputPath='Builds/CoinRewardsQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')}
if(![IO.Path]::IsPathRooted($OutputPath)){$OutputPath=Join-Path $rewardRoot $OutputPath}
if(Test-Path -LiteralPath $OutputPath){throw 'Choose a new evidence folder.'}
New-Item -ItemType Directory -Path $OutputPath | Out-Null
$rewardCases=@(@{name='desktop-1600x900';w=1600;h=900},@{name='wide-1560x720';w=1560;h=720},@{name='tablet-1200x900';w=1200;h=900},@{name='fullhd-1920x1080';w=1920;h=1080})
if($Single){$rewardCases=@($rewardCases[0])}
foreach($rewardCase in $rewardCases){
 $rewardFolder=Join-Path $OutputPath $rewardCase.name;New-Item -ItemType Directory -Path $rewardFolder | Out-Null
 $rewardArgs=@('-batchmode','-force-d3d11','-screen-width',$rewardCase.w,'-screen-height',$rewardCase.h,'-screen-fullscreen','0','-job-worker-count','2','-skipWelcome','-guestProfile','calendar-review','-offline','-sport','Football','-localizationProfile','calendar-review','-coinRewardsReview',('"'+$rewardFolder+'"'),'-contentCache',('"'+(Join-Path $rewardRoot 'Builds/CoinRewardsQA/ContentCache')+'"'),'-logFile',('"'+(Join-Path $rewardFolder 'player.log')+'"'))
 $rewardProcess=Start-Process -FilePath $PlayerPath -ArgumentList $rewardArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
 try{
  $null=$rewardProcess.Handle
  if(!$rewardProcess.WaitForExit(240000)){throw "Rewards review timeout: $rewardFolder"}
  $rewardText=Get-Content -LiteralPath (Join-Path $rewardFolder 'results.txt') -Raw
  if($rewardProcess.ExitCode -ne 0 -or $rewardText -match '(?m)^FAIL ' -or $rewardText -notmatch 'COIN_REWARDS_COMPLETE success=True'){throw "Rewards review failed: $rewardFolder"}
  if(Select-String -LiteralPath (Join-Path $rewardFolder 'player.log') -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException|Shader error' -Quiet){throw "Rewards runtime error: $rewardFolder"}
  Write-Output "PASS $($rewardCase.name) checks=$(([regex]::Matches($rewardText,'(?m)^PASS ')).Count)"
 }finally{if(!$rewardProcess.HasExited){Stop-Process -Id $rewardProcess.Id}}
}
Write-Output "COIN_REWARDS_SUITE_PASS $OutputPath"
