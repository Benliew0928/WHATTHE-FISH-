param(
 [string]$PlayerPath='Builds/FinalOptimizationQA/Player/WhatTheFish.exe',
 [string]$OutputPath='Builds/FinalOptimizationQA/Regression',
 [string]$ContentCache='Builds/SharedContentCache',
 [ValidateSet('Core','CoreRepair','Lifecycle','Presentation','Gameplay','Body','Localization','Awards')][string]$Suite='Core',
 [ValidateSet('Tour','Fishing','Golf','Basketball','Teleport')][string[]]$LifecycleModes=@('Tour','Fishing','Golf','Basketball','Teleport'),
 [ValidateSet('Bots','Basketball','Fishing','FishingFull','FishingPolish','Golf')][string[]]$GameplayCases=@('Bots','Basketball','Fishing','FishingPolish','Golf'),
 [ValidateSet('Golf','Fishing')][string[]]$BodySports=@('Golf','Fishing')
)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
function Resolve-ReviewPath([string]$Value){if([IO.Path]::IsPathRooted($Value)){return [IO.Path]::GetFullPath($Value)};return [IO.Path]::GetFullPath((Join-Path $root $Value))}
$player=Resolve-ReviewPath $PlayerPath;$output=Resolve-ReviewPath $OutputPath;$cache=Resolve-ReviewPath $ContentCache
if(!(Test-Path -LiteralPath $player -PathType Leaf)){throw 'Build the complete candidate player first.'}
function Read-LiveReview([string]$Path){
 # Reviews append hundreds of assertions in a frame. A default ReadAllText
 # handle denies writes and can abort that coroutine on Windows.
 $stream=[IO.FileStream]::new($Path,[IO.FileMode]::Open,[IO.FileAccess]::Read,([IO.FileShare]::ReadWrite -bor [IO.FileShare]::Delete))
 $reader=[IO.StreamReader]::new($stream)
 try{return $reader.ReadToEnd()}finally{$reader.Dispose();$stream.Dispose()}
}
function Invoke-Review {
 param([string]$Name,[string[]]$Options,[string]$Marker,[int]$Width=1280,[int]$Height=720,[int]$Seconds=300,[string]$Report='results.txt',[switch]$StopAfterMarker)
 $folder=Join-Path $output $Name
 if(Test-Path -LiteralPath $folder){throw "Review evidence already exists: $folder"}
 New-Item -ItemType Directory -Path $folder -Force | Out-Null
 $resolvedOptions=@($Options | ForEach-Object {$_.Replace('{output}',$folder)})
 # Gameplay fixtures assert authored English captions. Locale reviews opt in
 # explicitly; a preceding Malay/Chinese review must not leak into this one.
 if($resolvedOptions -notcontains '-language'){$resolvedOptions+=@('-language','en')}
 $arguments=@('-batchmode','-force-d3d11','-screen-width',"$Width",'-screen-height',"$Height",'-screen-fullscreen','0','-job-worker-count','2','-contentCache',('"'+$cache+'"'),'-guestProfile','final-optimization-review','-localizationProfile','final-optimization-review','-logFile',('"'+(Join-Path $folder 'player.log')+'"'))+$resolvedOptions
 $process=Start-Process -FilePath $player -ArgumentList $arguments -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
 try {
  $null=$process.Handle
  if($StopAfterMarker){
   $deadline=[DateTime]::UtcNow.AddSeconds($Seconds);$reportFile=Join-Path $folder $Report
   do {
    if(Test-Path -LiteralPath $reportFile){$reviewText=Read-LiveReview $reportFile;if($reviewText -match '(?m)^FAIL ' -or $reviewText -match [regex]::Escape($Marker)){break}}
    if($process.HasExited){break};Start-Sleep -Milliseconds 500
   }while([DateTime]::UtcNow -lt $deadline)
  }elseif(!$process.WaitForExit($Seconds*1000)){throw "Review timed out: $Name"}
  $result=Read-LiveReview (Join-Path $folder $Report)
  if(($process.HasExited -and $process.ExitCode -ne 0) -or $result -match '(?m)^FAIL ' -or $result -notmatch [regex]::Escape($Marker)){throw "Review failed: $Name"}
  $log=Get-Content -LiteralPath (Join-Path $folder 'player.log') -Raw
  if($log -match 'NullReferenceException:|MissingReferenceException:|IndexOutOfRangeException:|InvalidOperationException:|Shader error|CONTENT_LOAD_FAILED|Some objects were not cleaned up'){throw "Runtime diagnostic: $Name"}
  $record=[ordered]@{name=$Name;checks=([regex]::Matches($result,'(?m)^PASS ')).Count;exitCode=$process.ExitCode;stoppedAfterCompletion=[bool]$StopAfterMarker;evidence=[IO.Path]::GetRelativePath($root,$folder).Replace('\','/')}
  $record | ConvertTo-Json -Compress | Add-Content -LiteralPath (Join-Path $output 'summary.jsonl')
  Write-Output "PASS $Name checks=$($record.checks)"
 }finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
}
switch($Suite){
 {$_ -in 'Core','CoreRepair','Lifecycle'} {
  if($Suite -ne 'Lifecycle'){Invoke-Review Optimization @('-offline','-sport','Football','-skipWelcome','-optimizationReview','"{output}"') 'OPTIMIZATION_REVIEW_COMPLETE success=True'}
  if($Suite -eq 'Core'){Invoke-Review Lighting @('-characterLightingReview','"{output}"') 'CHARACTER_LIGHTING_REVIEW_COMPLETE success=True'}
  foreach($mode in $LifecycleModes){
   $sport=if($mode -in @('Tour','Teleport')){'Football'}else{$mode}
   Invoke-Review "Lifecycle-$mode" @('-offline','-sport',$sport,'-sceneLifecycleReview','"{output}"','-lifecycleMode',$mode) 'SCENE_LIFECYCLE_COMPLETE success=True' -Width 844 -Height 390
  }
 }
 'Presentation' {
  Invoke-Review Responsive @('-offline','-sport','Football','-skipWelcome','-responsiveLayoutReview','"{output}"') 'RESPONSIVE_LAYOUT_COMPLETE success=True' -Width 1600 -Height 900 -Seconds 600
  foreach($case in @(@{name='desktop';w=1600;h=900},@{name='wide';w=1560;h=720},@{name='tablet';w=1200;h=900},@{name='fullhd';w=1920;h=1080})){
   Invoke-Review "FirstVisit-$($case.name)" @('-offline','-sport','Football','-skipWelcome','-firstVisitReview','"{output}"') 'FIRST_VISIT_COMPLETE success=True' -Width $case.w -Height $case.h
  }
 }
 'Gameplay' {
  if($GameplayCases -contains 'Bots'){foreach($sport in @('Football','Basketball')){Invoke-Review "Bots-$sport" @('-offline','-sport',$sport,'-programmedBotsReview','"{output}"') 'PROGRAMMED_BOTS_COMPLETE success=True'}}
  if($GameplayCases -contains 'Basketball'){Invoke-Review Basketball @('-offline','-sport','Basketball','-basketballGameplayReview','"{output}"') 'BASKETBALL_GAMEPLAY_COMPLETE success=True' -StopAfterMarker}
  if($GameplayCases -contains 'Fishing'){Invoke-Review Fishing @('-probe','-offline','-sport','Fishing','-fishingGameplayAudit','-fishingDefaultViews','-report','"{output}/results.txt"','-exitAfter','300') 'FISHING_GAMEPLAY_COMPLETE' -StopAfterMarker}
  if($GameplayCases -contains 'FishingFull'){Invoke-Review FishingFull @('-probe','-offline','-sport','Fishing','-fishingGameplayAudit','-report','"{output}/results.txt"','-exitAfter','300') 'FISHING_GAMEPLAY_COMPLETE' -StopAfterMarker}
  if($GameplayCases -contains 'FishingPolish'){Invoke-Review FishingPolish @('-probe','-offline','-sport','Fishing','-fishingPolishAudit','-fishingDefaultViews','-report','"{output}/results.txt"','-exitAfter','180') 'FISHING_POLISH_COMPLETE' -StopAfterMarker}
  if($GameplayCases -contains 'Golf'){Invoke-Review Golf @('-probe','-offline','-sport','Golf','-golfDragPowerAudit','-report','"{output}/results.txt"','-exitAfter','180') 'GOLF_DRAG_POWER_COMPLETE' -StopAfterMarker}
 }
 'Localization' {Invoke-Review Localization @('-localizationReview','"{output}"') 'LOCALIZATION_COMPLETE success=True' -Width 1600 -Height 900 -Seconds 900}
 'Body' {foreach($sport in $BodySports){Invoke-Review "Body-$sport" @('-offline','-sport',$sport,'-firstPersonBodyReview','"{output}"') 'FIRST_PERSON_BODY_COMPLETE' -Width 1600 -Height 900}}
 'Awards' {
  foreach($sport in @('Golf','Fishing')){foreach($language in @('en','zh-Hans','ms')){
   Invoke-Review "Awards-$sport-$language" @('-offline','-sport',$sport,'-language',$language,'-awardsPolishReview','"{output}"') 'AWARDS_POLISH_COMPLETE success=True' -Width 1600 -Height 900
  }}
 }
}
Write-Output "FINAL_OPTIMIZATION_SUITE_PASS $Suite"
