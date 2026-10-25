param([string]$PlayerPath,[ValidateSet('Offline','Network','All')][string]$Mode='All',[ValidateSet('Basketball','Football','All')][string]$Sport='All',[switch]$Details,[ValidateSet('en','zh-Hans','ms')][string]$Language='en')
$ErrorActionPreference='Stop'
$scoreRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$scoreArea=if($Details){'Builds/ScoreboardDetailsQA'}else{'Builds/ScoreboardQA'}
if(!$PlayerPath){$PlayerPath=Join-Path $scoreRoot ($scoreArea+'/Player/WhatTheFish.exe')}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$scoreOutput=Join-Path $scoreRoot ($scoreArea+'/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $scoreOutput -Force | Out-Null
$scoreSports=if($Sport -eq 'All'){@('Basketball','Football')}else{@($Sport)}
foreach($scoreSport in $scoreSports){
 $scoreModes=if($Mode -eq 'All'){@('Offline','Network')}else{@($Mode)}
 foreach($scoreMode in $scoreModes){
  $scoreRoles=if($scoreMode -eq 'Network'){@('host','guest')}else{@('offline')}
  $scoreProcesses=@()
  try{
   foreach($scoreRole in $scoreRoles){
    $scoreFolder=Join-Path $scoreOutput "$scoreSport-$scoreRole";New-Item -ItemType Directory -Path $scoreFolder -Force | Out-Null
    $scoreArgs="-batchmode -force-d3d11 -job-worker-count 2 -skipWelcome -guestProfile scoreboard-$scoreRole -sport $scoreSport -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -scoreboardReview `"$scoreFolder`" -logFile `"$scoreFolder/player.log`""
    $scoreArgs+=" -contentCache `"$(Join-Path $scoreRoot ($scoreArea+'/ContentCache'))`" -language $Language -localizationProfile scoreboard-review-$Language"
    if($Details){$scoreArgs+=' -scoreboardDetails'}
    if($scoreRole -eq 'offline'){$scoreArgs+=' -offline'}elseif($scoreRole -eq 'host'){$scoreArgs+=' -localHost -port 7992'}else{$scoreArgs+=' -localClient -port 7992'}
    if($scoreRole -ne 'offline'){$scoreArgs+=" -probe -expected 2 -startAt 8 -returnAt 180 -exitAfter 200 -report `"$scoreFolder/probe.txt`""}
    $scoreProcess=Start-Process -FilePath $PlayerPath -ArgumentList $scoreArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru;$null=$scoreProcess.Handle;$scoreProcesses+=$scoreProcess
    if($scoreRole -eq 'host'){Start-Sleep -Seconds 4}
   }
   $scoreDeadline=(Get-Date).AddSeconds(210)
   while((Get-Date) -lt $scoreDeadline -and @($scoreProcesses | Where-Object {!$_.HasExited}).Count){Start-Sleep -Milliseconds 500}
   if(@($scoreProcesses | Where-Object {!$_.HasExited}).Count){throw "Scoreboard review timed out: $scoreOutput"}
   foreach($scoreRole in $scoreRoles){
    $scoreFolder=Join-Path $scoreOutput "$scoreSport-$scoreRole";$scoreResults=[string](Get-Content -LiteralPath "$scoreFolder/results.txt" -Raw)
    if($scoreResults -match '(?m)^FAIL ' -or !$scoreResults.Contains('SCOREBOARD_REVIEW_COMPLETE success=True')){throw "Scoreboard review failed: $scoreFolder"}
    if(Select-String -LiteralPath "$scoreFolder/player.log" -Pattern 'Exception:|Shader error|Assertion failed' -Quiet){throw "Scoreboard runtime error: $scoreFolder"}
    Write-Output "PASS $scoreSport $scoreRole"
   }
  }finally{foreach($scoreProcess in $scoreProcesses){if(!$scoreProcess.HasExited){Stop-Process -Id $scoreProcess.Id -ErrorAction SilentlyContinue}}}
 }
}
Write-Output "SCOREBOARD_SUITE_PASS $scoreOutput"
