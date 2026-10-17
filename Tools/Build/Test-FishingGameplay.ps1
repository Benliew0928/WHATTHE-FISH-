param([string]$PlayerPath,[int]$Port=7942,[switch]$Render,[switch]$OfflineOnly,[switch]$NetworkOnly,[ValidateRange(60,600)][int]$TimeoutSeconds=180)
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
   # Shared room entry now validates teleport shaders before handing over input.
   # A Null graphics device cannot exercise that production arrival path.
   $taskGraphics=if($taskSpec.name -ne 'offline'){'-batchmode -force-d3d11 -screen-width 640 -screen-height 360 -screen-fullscreen 0'}elseif($Render){'-batchmode -force-d3d11 -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -fishingCapture'}else{'-batchmode -nographics'}
   $taskOptions="$taskGraphics -job-worker-count 2 -sport Fishing $($taskSpec.args) -probe -exitAfter $($TimeoutSeconds+10) -report `"$taskReport`" -logFile `"$taskLog`""
   $taskProcesses+=Start-Process -FilePath $PlayerPath -ArgumentList $taskOptions -WindowStyle Hidden -PassThru
   if($taskSpec.name -eq 'host'){
    $taskHostReady=(Get-Date).AddSeconds(60)
    do{
     if($taskProcesses[-1].HasExited){throw 'Fishing host exited before opening its room.'}
     if((Test-Path -LiteralPath $taskReport) -and (Select-String -LiteralPath $taskReport -Pattern 'count=1 connected=True' -Quiet)){break}
     Start-Sleep -Milliseconds 250
    }while((Get-Date) -lt $taskHostReady)
    if(!(Test-Path -LiteralPath $taskReport) -or !(Select-String -LiteralPath $taskReport -Pattern 'count=1 connected=True' -Quiet)){throw 'Fishing host did not open its room before the guest launch.'}
   }
  }
  # Rendered tutorial, camera, both holds and island handoff captures can exceed
  # 100 seconds on integrated graphics. This is a test allowance, not match time.
  $taskDeadline=(Get-Date).AddSeconds($TimeoutSeconds)
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
if(!$OfflineOnly){Invoke-FishingProbes @(@{name='host';args="-localHost -port $Port -networkFishingAudit -expected 2 -startAt 5 -returnAt $TimeoutSeconds"},@{name='client';args="-localClient -port $Port -networkFishingAudit -expected 2 -startAt 5 -returnAt $TimeoutSeconds"})}
Write-Output "FISHING_GAMEPLAY_SUITE_PASS $taskOutput"
