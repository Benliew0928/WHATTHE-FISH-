param([string]$PlayerPath,[ValidateSet('Offline','Network','All')][string]$Mode='All',[ValidateSet('Football','Basketball','Golf','Fishing','All')][string]$Sport='All')
$ErrorActionPreference='Stop'
$coinRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $coinRoot 'Builds/StoreQA/Player/WhatTheFish.exe'}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$coinOutput=Join-Path $coinRoot ('Builds/StoreQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $coinOutput -Force | Out-Null
$coinCases=@()
if($Mode -ne 'Network'){$coinCases+=@{sport='Football';network=$false}}
if($Mode -ne 'Offline'){foreach($coinSport in $(if($Sport -eq 'All'){@('Football','Basketball','Golf','Fishing')}else{@($Sport)})){$coinCases+=@{sport=$coinSport;network=$true}}}
foreach($coinCase in $coinCases){
 $coinCaseName=$coinCase.sport+'-'+$(if($coinCase.network){'network'}else{'offline'})
 $coinFolder=Join-Path $coinOutput $coinCaseName
 New-Item -ItemType Directory -Path $coinFolder -Force | Out-Null
 $coinProcesses=@();$coinFixture=$null
 try{
  $coinFixture=Start-Process -FilePath (Get-Command node).Source -ArgumentList 'economy-fixture.mjs' -WorkingDirectory (Join-Path $coinRoot 'Tools/AccountServer') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $coinFolder 'fixture.txt') -RedirectStandardError (Join-Path $coinFolder 'fixture-errors.txt')
  Start-Sleep -Seconds 2
  if($coinFixture.HasExited){throw 'Economy fixture did not start.'}
  $coinRoles=if(!$coinCase.network){@('host')}elseif($coinCase.sport -eq 'Basketball'){@('host','guest1','guest2','guest3')}else{@('host','guest1')}
  foreach($coinRole in $coinRoles){
   $coinEvidence=Join-Path $coinFolder $coinRole;New-Item -ItemType Directory -Path $coinEvidence -Force | Out-Null
   $coinArgs="-batchmode -force-d3d11 -job-worker-count 2 -skipWelcome -guestProfile coin-$coinRole -sport $($coinCase.sport) -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -economyReview `"$coinEvidence`" -economyRole $coinRole -logFile `"$coinEvidence/player.log`" -contentCache `"$(Join-Path $coinRoot 'Builds/StoreQA/ContentCache')`" -localizationProfile coin-$coinRole"
   if(!$coinCase.network){$coinArgs+=' -offline'}elseif($coinRole -eq 'host'){$coinArgs+=' -localHost -port 7994'}else{$coinArgs+=' -localClient -port 7994'}
   if($coinCase.network){$coinArgs+=" -probe -expected $($coinRoles.Count) -startAt 12 -returnAt 300 -exitAfter 320 -report `"$coinEvidence/probe.txt`""}
   $coinPlayer=Start-Process -FilePath $PlayerPath -ArgumentList $coinArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
   $null=$coinPlayer.Handle;$coinProcesses+=$coinPlayer
   Start-Sleep -Seconds 3
  }
  $coinDeadline=(Get-Date).AddSeconds(330)
  while((Get-Date) -lt $coinDeadline -and @($coinProcesses | Where-Object {!$_.HasExited}).Count){Start-Sleep -Milliseconds 500}
  if(@($coinProcesses | Where-Object {!$_.HasExited}).Count){throw "Economy review timeout: $coinFolder"}
  foreach($coinRole in $coinRoles){
   $coinEvidence=Join-Path $coinFolder $coinRole;$coinResults=Get-Content -LiteralPath "$coinEvidence/results.txt" -Raw
   if($coinResults -match '(?m)^FAIL ' -or $coinResults -notmatch 'ECONOMY_REVIEW_COMPLETE success=True'){throw "Economy checks failed: $coinEvidence"}
   if(Select-String -LiteralPath "$coinEvidence/player.log" -Pattern 'Exception:|Shader error|Assertion failed' -Quiet){throw "Economy runtime error: $coinEvidence"}
   Write-Output "PASS $coinCaseName $coinRole"
  }
 }finally{
  foreach($coinPlayer in $coinProcesses){if(!$coinPlayer.HasExited){Stop-Process -Id $coinPlayer.Id -ErrorAction SilentlyContinue}}
  if($coinFixture -and !$coinFixture.HasExited){Stop-Process -Id $coinFixture.Id -ErrorAction SilentlyContinue}
 }
}
Write-Output "ECONOMY_SUITE_PASS $coinOutput"
