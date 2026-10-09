param([string]$PlayerPath,[int]$Port=7868,[switch]$Render,[switch]$NetworkOnly,[switch]$OfflineOnly)
$ErrorActionPreference='Stop'
if($NetworkOnly -and $OfflineOnly){throw 'Choose either NetworkOnly or OfflineOnly.'}
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$taskOutput=Join-Path $taskRoot ('Builds/GolfAimQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskOutput | Out-Null
function Invoke-GolfProbes($Specs){
 $taskProcesses=@()
 try {
  foreach($taskSpec in $Specs){
   $taskReport=Join-Path $taskOutput ($taskSpec.name+'.txt');$taskLog=Join-Path $taskOutput ($taskSpec.name+'.log')
   $taskGraphics=if($Render -and ($taskSpec.name -ne 'host')){'-batchmode -force-d3d11 -screen-width 1600 -screen-height 900 -screen-fullscreen 0 '}else{'-batchmode -nographics'}
   $taskOptions="$taskGraphics -job-worker-count 2 -sport Golf $($taskSpec.args) -probe -exitAfter 100 -report `"$taskReport`" -logFile `"$taskLog`""
   $taskProcesses+=Start-Process -FilePath $PlayerPath -ArgumentList $taskOptions -WindowStyle Hidden -PassThru
   if($taskSpec.name -eq 'host'){Start-Sleep -Seconds 2}
  }
  $taskDeadline=(Get-Date).AddSeconds(90)
  do {
   $taskDone=@($Specs | Where-Object {$taskFile=Join-Path $taskOutput ($_.name+'.txt');(Test-Path -LiteralPath $taskFile) -and (Select-String -LiteralPath $taskFile -Pattern 'GOLF_AIM_COMPLETE' -Quiet)})
   if($taskDone.Count -eq $Specs.Count){break}
   if(@($Specs | Where-Object {$taskFile=Join-Path $taskOutput ($_.name+'.txt');(Test-Path -LiteralPath $taskFile) -and (Select-String -LiteralPath $taskFile -Pattern '^FAIL ' -Quiet)}).Count){break}
   if(@($taskProcesses | Where-Object {$_.HasExited}).Count){break}
   Start-Sleep -Milliseconds 500
  }while((Get-Date) -lt $taskDeadline)
  foreach($taskSpec in $Specs){
   $taskText=Get-Content -LiteralPath (Join-Path $taskOutput ($taskSpec.name+'.txt')) -Raw
   if($taskText -match '(?m)^FAIL ' -or !$taskText.Contains('GOLF_AIM_COMPLETE')){throw "Incomplete/failed $($taskSpec.name): $taskOutput"}
   $taskLogText=Get-Content -LiteralPath (Join-Path $taskOutput ($taskSpec.name+'.log')) -Raw
   if($taskLogText -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException)'){throw "Runtime exception: $($taskSpec.name)"}
   Write-Output "PASS $($taskSpec.name) assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count)"
  }
 }finally{foreach($taskProcess in $taskProcesses){if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}}
}
if(!$NetworkOnly){Invoke-GolfProbes @(@{name='offline';args='-offline -golfAimAudit'})}
if(!$OfflineOnly){Invoke-GolfProbes @(@{name='host';args="-localHost -port $Port -golfAimAudit -expected 2 -startAt 5 -returnAt 90"},@{name='client';args="-localClient -port $Port -golfAimAudit -expected 2 -startAt 5 -returnAt 90"})}
Write-Output "GOLF_AIM_SUITE_PASS $taskOutput"
