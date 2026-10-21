param([string]$PlayerPath='Builds/PlayerProfilesQA/Player/WhatTheFish.exe',[string]$OutputPath='')
$ErrorActionPreference='Stop'
$profilesRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$profilesPlayer=if([IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath}else{Join-Path $profilesRoot $PlayerPath}
if(!$OutputPath){$OutputPath='Builds/PlayerProfilesQA/Network-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')}
$profilesRun=if([IO.Path]::IsPathRooted($OutputPath)){$OutputPath}else{Join-Path $profilesRoot $OutputPath}
$profilesProcesses=@()
New-Item -ItemType Directory -Force -Path $profilesRun | Out-Null
try {
 foreach($profilesRole in @('Host','Guest')){
  $profilesFolder=Join-Path $profilesRun $profilesRole
  New-Item -ItemType Directory -Force -Path $profilesFolder | Out-Null
  $profilesIsolation='profiles'+(Get-Date -Format 'MMddHHmmssfff')+$profilesRole
  $profilesArgs=@('-batchmode','-force-d3d11','-screen-width',$(if($profilesRole -eq 'Host'){1600}else{1200}),'-screen-height','900','-screen-fullscreen','0','-job-worker-count','2',
   '-guestProfile',$profilesIsolation,'-authProfile',$profilesIsolation,'-profilesRole',$profilesRole,'-profilesShared',('"'+$profilesRun+'"'),'-profilesReview',('"'+$profilesFolder+'"'),'-logFile',('"'+(Join-Path $profilesFolder 'player.log')+'"'))
  $profilesProcess=Start-Process -FilePath $profilesPlayer -ArgumentList $profilesArgs -WorkingDirectory $env:TEMP -PassThru -WindowStyle Hidden
  $profilesProcesses+=@{Process=$profilesProcess;Folder=$profilesFolder;Arguments=$profilesArgs}
 }
 foreach($profilesItem in $profilesProcesses){
  if(!$profilesItem.Process.WaitForExit(300000)){throw 'Player profile review timed out.'}
  $profilesResult=Get-Content -LiteralPath (Join-Path $profilesItem.Folder 'results.txt') -Raw
  if($profilesItem.Process.ExitCode -ne 0 -or $profilesResult -match '(?m)^FAIL ' -or !$profilesResult.Contains('PLAYER_PROFILES_COMPLETE success=True')){throw "Profile review failed: $($profilesItem.Folder)"}
  if(Select-String -LiteralPath (Join-Path $profilesItem.Folder 'player.log') -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException' -Quiet){throw "Runtime error: $($profilesItem.Folder)"}
  Write-Output "PASS $($profilesItem.Folder) assertions=$(([regex]::Matches($profilesResult,'(?m)^PASS ')).Count)"
 }
 foreach($profilesItem in $profilesProcesses){
  $profilesCacheFolder=Join-Path $profilesItem.Folder 'Restart'
  New-Item -ItemType Directory -Force -Path $profilesCacheFolder | Out-Null
  $profilesCacheArgs=@($profilesItem.Arguments)
  $profilesCacheArgs[[Array]::IndexOf($profilesCacheArgs,'-profilesReview')+1]='"'+$profilesCacheFolder+'"'
  $profilesCacheArgs[[Array]::IndexOf($profilesCacheArgs,'-logFile')+1]='"'+(Join-Path $profilesCacheFolder 'player.log')+'"'
  $profilesCacheArgs+=@('-profilesMode','cache')
  $profilesCacheProcess=Start-Process -FilePath $profilesPlayer -ArgumentList $profilesCacheArgs -WorkingDirectory $env:TEMP -PassThru -WindowStyle Hidden
  try {
   if(!$profilesCacheProcess.WaitForExit(150000)){throw 'Profile restart review timed out.'}
   $profilesCacheResult=Get-Content -LiteralPath (Join-Path $profilesCacheFolder 'results.txt') -Raw
   if($profilesCacheProcess.ExitCode -ne 0 -or $profilesCacheResult -match '(?m)^FAIL ' -or !$profilesCacheResult.Contains('PLAYER_PROFILES_COMPLETE success=True')){throw "Profile persistence failed: $profilesCacheFolder"}
   Write-Output "PASS profile restart assertions=$(([regex]::Matches($profilesCacheResult,'(?m)^PASS ')).Count)"
  }finally{if(!$profilesCacheProcess.HasExited){Stop-Process -Id $profilesCacheProcess.Id}}
 }
 Write-Output "PLAYER_PROFILES_SUITE_PASS $profilesRun"
}finally{foreach($profilesItem in $profilesProcesses){if(!$profilesItem.Process.HasExited){Stop-Process -Id $profilesItem.Process.Id}}}
