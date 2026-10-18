param([string]$PlayerPath,[ValidateSet('Tour','Fishing','Golf','Basketball','Teleport')][string[]]$Mode=@('Tour','Fishing','Golf','Basketball','Teleport'))
$ErrorActionPreference='Stop'
$lifecycleRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$lifecyclePlayer=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $lifecycleRoot 'Builds/StadiumTeardownQA/Player/WhatTheFish.exe'}
$lifecycleRun=Join-Path $lifecycleRoot ('Builds/StadiumTeardownQA/Lifecycle-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
foreach($lifecycleMode in $Mode){
 $lifecycleFolder=Join-Path $lifecycleRun $lifecycleMode;New-Item -ItemType Directory -Force -Path $lifecycleFolder | Out-Null
 $lifecycleSport=if($lifecycleMode -in @('Tour','Teleport')){'Football'}else{$lifecycleMode}
 $lifecycleArguments=@('-batchmode','-force-d3d11','-job-worker-count','2','-offline','-sport',$lifecycleSport,'-sceneLifecycleReview',('"'+$lifecycleFolder+'"'),'-lifecycleMode',$lifecycleMode,'-screen-fullscreen','0','-screen-width','844','-screen-height','390','-logFile',('"'+(Join-Path $lifecycleFolder 'player.log')+'"'))
 $lifecycleArguments+=@('-contentCache',('"'+(Join-Path $lifecycleRun 'ContentCache')+'"'))
 $lifecycleProcess=Start-Process -FilePath $lifecyclePlayer -ArgumentList $lifecycleArguments -WindowStyle Hidden -PassThru
 try {
  if(!$lifecycleProcess.WaitForExit(240000)){throw "Lifecycle review timed out: $lifecycleFolder"}
  $lifecycleResults=Get-Content -LiteralPath (Join-Path $lifecycleFolder 'results.txt') -Raw
  if($lifecycleProcess.ExitCode -ne 0 -or $lifecycleResults -match '(?m)^FAIL ' -or !$lifecycleResults.Contains('SCENE_LIFECYCLE_COMPLETE success=True') -or !$lifecycleResults.Contains('PASS shutdown guard armed before object destruction')){throw "Lifecycle checks failed: $lifecycleFolder"}
  if(Select-String -LiteralPath (Join-Path $lifecycleFolder 'player.log') -Pattern 'GameObjects can not be made active|being destroyed|Exception:|Assertion failed|Shader error|Some objects were not cleaned up' -Quiet){throw "Lifecycle runtime diagnostic: $lifecycleFolder"}
  Write-Output "SCENE_LIFECYCLE_PASS $lifecycleMode assertions=$(([regex]::Matches($lifecycleResults,'(?m)^PASS ')).Count) $lifecycleFolder"
 }finally{if(!$lifecycleProcess.HasExited){Stop-Process -Id $lifecycleProcess.Id}}
}
