param([string]$PlayerPath,[int]$Port=7859,[switch]$NetworkOnly)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/GolfPolishQA/Player/WhatTheFish.exe'}
$taskOutput=Join-Path $taskRoot ('Builds/GolfPolishQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskOutput | Out-Null
function Invoke-PolishProbes($Specs){
 $taskProcesses=@()
 try{
  foreach($taskSpec in $Specs){
   $taskReport=Join-Path $taskOutput ($taskSpec.name+'.txt');$taskLog=Join-Path $taskOutput ($taskSpec.name+'.log')
   $taskArgs="-batchmode -job-worker-count 2 -sport Golf $($taskSpec.args) -probe -golfPolishAudit -exitAfter 150 -report `"$taskReport`" -logFile `"$taskLog`""
   $taskProcesses+=Start-Process -FilePath $PlayerPath -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
   if($taskSpec.name -eq 'host'){Start-Sleep -Seconds 2}
  }
  $taskDeadline=(Get-Date).AddSeconds(140)
  do{
   $taskDone=@($Specs|Where-Object{$taskFile=Join-Path $taskOutput ($_.name+'.txt');(Test-Path -LiteralPath $taskFile) -and (Select-String -LiteralPath $taskFile -Pattern 'GOLF_POLISH_COMPLETE' -Quiet)})
   if($taskDone.Count -eq $Specs.Count){break}
   if(@($Specs|Where-Object{$taskFile=Join-Path $taskOutput ($_.name+'.txt');(Test-Path -LiteralPath $taskFile) -and (Select-String -LiteralPath $taskFile -Pattern '^FAIL ' -Quiet)}).Count){break}
   if(@($taskProcesses|Where-Object{$_.HasExited}).Count){break}
   Start-Sleep -Milliseconds 500
  }while((Get-Date) -lt $taskDeadline)
  foreach($taskSpec in $Specs){
   $taskFile=Join-Path $taskOutput ($taskSpec.name+'.txt');if(!(Test-Path -LiteralPath $taskFile)){throw "Missing $($taskSpec.name) report: $taskOutput"}
   $taskText=Get-Content -LiteralPath $taskFile -Raw
   if($taskText -match '(?m)^FAIL ' -or !$taskText.Contains('GOLF_POLISH_COMPLETE')){throw "Incomplete/failed $($taskSpec.name): $taskOutput"}
   $taskLogText=Get-Content -LiteralPath (Join-Path $taskOutput ($taskSpec.name+'.log')) -Raw
   if($taskLogText -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException)'){throw "Runtime exception in $($taskSpec.name): $taskOutput"}
   Write-Output "PASS $($taskSpec.name) assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count)"
  }
 }finally{foreach($taskProcess in $taskProcesses){if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}}
}
if(!$NetworkOnly){Invoke-PolishProbes @(@{name='offline';args='-offline -polishPictures'})}
Invoke-PolishProbes @(@{name='host';args="-localHost -port $Port -polishNetwork -expected 2 -startAt 9999 -returnAt 9999"},@{name='client';args="-localClient -port $Port -polishNetwork -expected 2 -startAt 9999 -returnAt 9999"})
Write-Output "GOLF_POLISH_SUITE_PASS $taskOutput"
