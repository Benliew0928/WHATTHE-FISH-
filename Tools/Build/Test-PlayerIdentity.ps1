param([string]$PlayerPath='Builds/PlayerIdentityQA/Player/WhatTheFish.exe',[int]$Port=7997)
$ErrorActionPreference='Stop'
$identityRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$identityPlayer=if([IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath}else{Join-Path $identityRoot $PlayerPath}
$identityRun=Join-Path $identityRoot ('Builds/PlayerIdentityQA/Network-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$identityProcesses=@()
try {
 foreach($identityRole in @('Host','Guest')){
  $identityFolder=Join-Path $identityRun $identityRole
  New-Item -ItemType Directory -Force -Path $identityFolder | Out-Null
  $identityArgs=@('-batchmode','-nographics','-job-worker-count','2','-playerIdentityReview',('"'+$identityFolder+'"'),'-guestProfile',((Split-Path $identityRun -Leaf)+$identityRole),'-guestName',('"'+$identityRole+' Name"'),'-port',$Port,'-logFile',('"'+(Join-Path $identityFolder 'player.log')+'"'),$(if($identityRole -eq 'Host'){'-localHost'}else{'-localClient'}))
  $identityProcess=Start-Process -FilePath $identityPlayer -ArgumentList $identityArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
  $identityProcesses+=@{Process=$identityProcess;Folder=$identityFolder}
  if($identityRole -eq 'Host'){Start-Sleep -Seconds 3}
 }
 foreach($identityItem in $identityProcesses){
  if(!$identityItem.Process.WaitForExit(120000)){throw 'Player identity review timed out.'}
  $identityResults=Get-Content -LiteralPath (Join-Path $identityItem.Folder 'results.txt') -Raw
  if($identityItem.Process.ExitCode -ne 0 -or $identityResults -match '(?m)^FAIL ' -or !$identityResults.Contains('PLAYER_IDENTITY_COMPLETE success=True')){throw "Identity review failed: $($identityItem.Folder)"}
  if(Select-String -LiteralPath (Join-Path $identityItem.Folder 'player.log') -Pattern 'Exception:|Assertion failed' -Quiet){throw "Runtime error: $($identityItem.Folder)"}
  Write-Output "PASS $($identityItem.Folder) assertions=$(([regex]::Matches($identityResults,'(?m)^PASS ')).Count)"
 }
 if(Compare-Object (Get-Content (Join-Path $identityRun 'Host/roster.txt')) (Get-Content (Join-Path $identityRun 'Guest/roster.txt'))){throw 'Host and guest disagree on player names.'}
 Write-Output "PLAYER_IDENTITY_SUITE_PASS $identityRun"
} finally {foreach($identityItem in $identityProcesses){if(!$identityItem.Process.HasExited){Stop-Process -Id $identityItem.Process.Id}}}
