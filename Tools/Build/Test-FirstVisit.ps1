param([string]$PlayerPath='Builds/FirstVisitQA/Player/WhatTheFish.exe',[string]$OutputPath='Builds/FirstVisitQA/Evidence',[string]$ContentCache='Builds/FirstVisitQA/ContentCache')
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(![IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath=Join-Path $taskRoot $PlayerPath}
if(![IO.Path]::IsPathRooted($OutputPath)){$OutputPath=Join-Path $taskRoot $OutputPath}
if(![IO.Path]::IsPathRooted($ContentCache)){$ContentCache=Join-Path $taskRoot $ContentCache}
if(Test-Path -LiteralPath $OutputPath){throw 'Choose a new evidence folder.'}
New-Item -ItemType Directory -Path $OutputPath | Out-Null
foreach($case in @(@{name='desktop';w=1600;h=900},@{name='wide';w=1560;h=720},@{name='tablet';w=1200;h=900},@{name='fullhd';w=1920;h=1080})){
 $folder=Join-Path $OutputPath $case.name;New-Item -ItemType Directory -Path $folder | Out-Null
 $arguments=@('-batchmode','-force-d3d11','-screen-width',$case.w,'-screen-height',$case.h,'-screen-fullscreen','0','-job-worker-count','2','-skipWelcome','-guestProfile','first-visit-review','-offline','-sport','Football','-localizationProfile','first-visit-review','-firstVisitReview',('"'+$folder+'"'),'-contentCache',('"'+$ContentCache+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
 $process=Start-Process -FilePath $PlayerPath -ArgumentList $arguments -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
 try{
  $null=$process.Handle;if(!$process.WaitForExit(240000)){throw "Review timed out: $folder"}
  $result=Get-Content -LiteralPath (Join-Path $folder 'results.txt') -Raw
  if($process.ExitCode -ne 0 -or $result -match '(?m)^FAIL ' -or $result -notmatch 'FIRST_VISIT_COMPLETE success=True'){throw "Review failed: $folder"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException' -Quiet){throw "Runtime error: $folder"}
  Write-Output "PASS $($case.name) checks=$(([regex]::Matches($result,'(?m)^PASS ')).Count)"
 }finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
}
Write-Output "FIRST_VISIT_SUITE_PASS $OutputPath"
