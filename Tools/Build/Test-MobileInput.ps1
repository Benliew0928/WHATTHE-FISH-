param(
 [string]$PlayerPath='Builds/ActivityCinematicsQA/Player/WhatTheFish.exe',
 [string]$OutputPath='Builds/MobileAuditQA/Input',
 [string]$ContentCache='Builds/SharedContentCache',
 [ValidateSet('All','Touch','Click')][string]$Review='All',
 [string[]]$Sports=@('Football','Basketball','Fishing','Golf'),
 [string[]]$Languages=@('en','zh-Hans','ms')
)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
function Resolve-MobilePath([string]$value){if([IO.Path]::IsPathRooted($value)){return $value};return Join-Path $root $value}
$player=Resolve-MobilePath $PlayerPath;$output=Resolve-MobilePath $OutputPath;$cache=Resolve-MobilePath $ContentCache
if(!(Test-Path -LiteralPath $player)){throw 'Build the complete Windows review player first.'}
New-Item -ItemType Directory -Force -Path $output | Out-Null
foreach($sport in $Sports){foreach($language in $Languages){foreach($kind in $(if($Review -eq 'All'){@('Touch','Click')}else{@($Review)})){
 $name="$sport-$language-$kind";$folder=Join-Path $output $name
 if(Test-Path -LiteralPath $folder){throw "Evidence already exists: $folder"}
 New-Item -ItemType Directory -Path $folder | Out-Null
 $flag=if($kind -eq 'Touch'){'-touchCameraReview'}else{'-mobileInteractionReview'}
 $marker=if($kind -eq 'Touch'){'TOUCH_CAMERA_COMPLETE'}else{'MOBILE_INTERACTION_COMPLETE'}
 $width=if($language -eq 'ms'){960}else{1280};$height=if($language -eq 'ms'){720}else{582}
 $options=@('-batchmode','-force-d3d11','-screen-width',$width,'-screen-height',$height,'-screen-fullscreen','0','-job-worker-count','2','-offline','-skipWelcome','-sport',$sport,'-language',$language,'-guestProfile',('mobile-'+$name),'-localizationProfile',('mobile-'+$name),'-contentCache',('"'+$cache+'"'),$flag,('"'+$folder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
 $p=Start-Process -FilePath $player -ArgumentList $options -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
 try{
  $null=$p.Handle;if(!$p.WaitForExit(180000)){throw "Mobile input review timed out: $name"}
  $result=Get-Content -LiteralPath (Join-Path $folder 'results.txt') -Raw
  if($p.ExitCode -ne 0 -or $result -match '(?m)^FAIL ' -or $result -notmatch ($marker+' checks=\d+ success=True')){throw "Mobile input review failed: $name"}
  $log=Get-Content -LiteralPath (Join-Path $folder 'player.log') -Raw
  if($log -match 'NullReferenceException:|MissingReferenceException:|IndexOutOfRangeException:|InvalidOperationException:|Shader error'){throw "Runtime exception: $name"}
  $count=([regex]::Matches($result,'(?m)^PASS ')).Count
  [ordered]@{name=$name;checks=$count;width=$width;height=$height;evidence=[IO.Path]::GetRelativePath($root,$folder).Replace('\','/')} | ConvertTo-Json -Compress | Add-Content (Join-Path $output 'summary.jsonl')
  Write-Output "PASS $name checks=$count"
 }finally{if(!$p.HasExited){Stop-Process -Id $p.Id}}
}}}
Write-Output 'MOBILE_INPUT_SUITE_PASS'
