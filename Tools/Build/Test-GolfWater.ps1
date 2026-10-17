param([string]$PlayerPath,[int]$Port=7837,[switch]$NetworkOnly,[switch]$OfflineOnly)
$ErrorActionPreference='Stop'
if($NetworkOnly -and $OfflineOnly){throw 'Choose either NetworkOnly or OfflineOnly.'}
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$taskOutput=Join-Path $taskRoot ('Builds/GolfWaterV5QA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskOutput | Out-Null
function Invoke-GolfWaterProbes($Specs){
 $taskProcesses=@()
 try {
  foreach($taskSpec in $Specs){
   $taskReport=Join-Path $taskOutput ($taskSpec.name+'.txt');$taskLog=Join-Path $taskOutput ($taskSpec.name+'.log')
   $taskGraphics='-batchmode -nographics'
   $taskOptions="$taskGraphics -job-worker-count 2 -sport Golf $($taskSpec.args) -probe -exitAfter 120 -report `"$taskReport`" -logFile `"$taskLog`""
   $taskProcesses+=Start-Process -FilePath $PlayerPath -ArgumentList $taskOptions -WindowStyle Hidden -PassThru
   if($taskSpec.name -eq 'host'){Start-Sleep -Seconds 2}
  }
  $taskDeadline=(Get-Date).AddSeconds(110)
  do {
   $taskDone=@($Specs | Where-Object {$taskFile=Join-Path $taskOutput ($_.name+'.txt');(Test-Path -LiteralPath $taskFile) -and (Select-String -LiteralPath $taskFile -Pattern 'GOLF_WATER_COMPLETE' -Quiet)})
   if($taskDone.Count -eq $Specs.Count){break}
   if(@($Specs | Where-Object {$taskFile=Join-Path $taskOutput ($_.name+'.txt');(Test-Path -LiteralPath $taskFile) -and (Select-String -LiteralPath $taskFile -Pattern '^FAIL ' -Quiet)}).Count){break}
   if(@($taskProcesses | Where-Object {$_.HasExited}).Count){break}
   Start-Sleep -Milliseconds 500
  }while((Get-Date) -lt $taskDeadline)
  foreach($taskSpec in $Specs){
   $taskText=Get-Content -LiteralPath (Join-Path $taskOutput ($taskSpec.name+'.txt')) -Raw
   if($taskText -match '(?m)^FAIL ' -or !$taskText.Contains('GOLF_WATER_COMPLETE')){throw "Incomplete/failed $($taskSpec.name): $taskOutput"}
   $taskLogText=Get-Content -LiteralPath (Join-Path $taskOutput ($taskSpec.name+'.log')) -Raw
   if($taskLogText -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException)'){throw "Runtime exception: $($taskSpec.name)"}
   Write-Output "PASS $($taskSpec.name) assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count)"
  }
 }finally{foreach($taskProcess in $taskProcesses){if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}}
}
if(!$NetworkOnly){Invoke-GolfWaterProbes @(@{name='offline';args='-offline -golfWaterAudit'})}
$taskSide=''
if(!$OfflineOnly){Invoke-GolfWaterProbes @(@{name='host';args="-localHost -port $Port -golfWaterAudit -networkWater -expected 2 -startAt 5 -returnAt 90$taskSide"},@{name='client';args="-localClient -port $Port -golfWaterAudit -networkWater -expected 2 -startAt 5 -returnAt 90$taskSide"})}
Write-Output "GOLF_WATER_SUITE_PASS $taskOutput"
