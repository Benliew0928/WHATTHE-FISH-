param([string]$PlayerPath,[int]$Port=7942,[switch]$Render,[switch]$OfflineOnly,[switch]$NetworkOnly)
$ErrorActionPreference='Stop'
if($OfflineOnly -and $NetworkOnly){throw 'Choose one test filter.'}
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$taskOutput=Join-Path $taskRoot ('Builds/FishingGameplayQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskOutput | Out-Null
function Invoke-FishingProbes($Specs){
 $taskProcesses=@()
 try {
  foreach($taskSpec in $Specs){
   $taskReport=Join-Path $taskOutput ($taskSpec.name+'.txt');$taskLog=Join-Path $taskOutput ($taskSpec.name+'.log')
   $taskGraphics=if($Render -and $taskSpec.name -eq 'offline'){'-batchmode -force-d3d11 -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -fishingCapture'}else{'-batchmode -nographics'}
   $taskOptions="$taskGraphics -job-worker-count 2 -sport Fishing $($taskSpec.args) -probe -exitAfter 110 -report `"$taskReport`" -logFile `"$taskLog`""
   $taskProcesses+=Start-Process -FilePath $PlayerPath -ArgumentList $taskOptions -WindowStyle Hidden -PassThru
   if($taskSpec.name -eq 'host'){Start-Sleep -Seconds 2}
  }
  $taskDeadline=(Get-Date).AddSeconds(100)
  do {
   $taskDone=@($Specs|Where-Object{$f=Join-Path $taskOutput ($_.name+'.txt');(Test-Path -LiteralPath $f) -and (Select-String -LiteralPath $f -Pattern 'FISHING_GAMEPLAY_COMPLETE' -Quiet)})
   if($taskDone.Count -eq $Specs.Count){break}
   if(@($Specs|Where-Object{$f=Join-Path $taskOutput ($_.name+'.txt');(Test-Path -LiteralPath $f) -and (Select-String -LiteralPath $f -Pattern '^FAIL ' -Quiet)}).Count){break}
   if(@($taskProcesses|Where-Object{$_.HasExited}).Count){break}
   Start-Sleep -Milliseconds 500
  }while((Get-Date) -lt $taskDeadline)
  foreach($taskSpec in $Specs){
   $taskText=Get-Content -LiteralPath (Join-Path $taskOutput ($taskSpec.name+'.txt')) -Raw
   if($taskText -match '(?m)^FAIL ' -or !$taskText.Contains('FISHING_GAMEPLAY_COMPLETE')){throw "Incomplete/failed $($taskSpec.name): $taskOutput"}
   $taskLogText=Get-Content -LiteralPath (Join-Path $taskOutput ($taskSpec.name+'.log')) -Raw
   if($taskLogText -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException|Shader error)'){throw "Runtime exception: $($taskSpec.name)"}
   Write-Output "PASS $($taskSpec.name) assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count)"
  }
 }finally{foreach($taskProcess in $taskProcesses){if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}}
}
if(!$NetworkOnly){Invoke-FishingProbes @(@{name='offline';args='-offline -fishingGameplayAudit'})}
if(!$OfflineOnly){Invoke-FishingProbes @(@{name='host';args="-localHost -port $Port -networkFishingAudit -expected 2 -startAt 5 -returnAt 100"},@{name='client';args="-localClient -port $Port -networkFishingAudit -expected 2 -startAt 5 -returnAt 100"})}
Write-Output "FISHING_GAMEPLAY_SUITE_PASS $taskOutput"
