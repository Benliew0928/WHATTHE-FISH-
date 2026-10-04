param([switch]$IncludeRegression,[switch]$Render,[int]$Port=7814,[string]$PlayerPath)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/FootballMatchQA/Player/WhatTheFish.exe'}
$taskOutput=Join-Path $taskRoot ('Builds/FootballMatchQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $taskOutput | Out-Null
function Invoke-Probes($Specs){
 $taskProcesses=@()
 try {
  foreach($taskSpec in $Specs){
   $taskReport=Join-Path $taskOutput ($taskSpec.name+'.txt');$taskLog=Join-Path $taskOutput ($taskSpec.name+'.log')
   # Aim-material assertions need a graphics device. Keep the physics probe in
   # batch mode so review captures cannot resize its pointer-coordinate fixture.
   $taskGraphics=if($taskSpec.name -eq 'physics'){'-batchmode -screen-width 960 -screen-height 540 -screen-fullscreen 0'}elseif($Render){'-screen-width 960 -screen-height 540 -screen-fullscreen 0'}else{'-batchmode -nographics'}
   $taskOptions="$taskGraphics -job-worker-count 2 $($taskSpec.args) -probe -exitAfter 120 -report `"$taskReport`" -logFile `"$taskLog`""
   $taskProcesses+=Start-Process -FilePath $PlayerPath -WindowStyle Hidden -ArgumentList $taskOptions -PassThru
   if($taskSpec.name -match 'host'){Start-Sleep -Seconds 2}
  }
  $taskDeadline=(Get-Date).AddSeconds(110)
  do {
   $taskDone=@($Specs | Where-Object {$taskFile=Join-Path $taskOutput ($_.name+'.txt');(Test-Path -LiteralPath $taskFile) -and (Select-String -LiteralPath $taskFile -Pattern $_.marker -Quiet)})
   if($taskDone.Count -eq $Specs.Count){break}
   Start-Sleep -Milliseconds 500
  }while((Get-Date) -lt $taskDeadline)
  foreach($taskSpec in $Specs){
   $taskText=Get-Content -LiteralPath (Join-Path $taskOutput ($taskSpec.name+'.txt')) -Raw
   if($taskText -match '(?m)^FAIL ' -or !$taskText.Contains($taskSpec.marker)){throw "Incomplete/failed $($taskSpec.name): $taskOutput"}
   $taskLogText=Get-Content -LiteralPath (Join-Path $taskOutput ($taskSpec.name+'.log')) -Raw
   if($taskLogText -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException)'){throw "Runtime exception: $($taskSpec.name)"}
   Write-Output "PASS $($taskSpec.name) assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count)"
  }
 }finally{foreach($taskProcess in $taskProcesses){if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}}
}
Invoke-Probes @(@{name='offline';args='-offline -footballMatchAudit';marker='MATCH_COMPLETE'})
Invoke-Probes @(@{name='host';args="-localHost -port $Port -networkFootballMatchAudit -expected 2 -startAt 5 -returnAt 100";marker='NETWORK_MATCH_COMPLETE'},@{name='client';args="-localClient -port $Port -networkFootballMatchAudit -expected 2 -startAt 5 -returnAt 100";marker='NETWORK_MATCH_COMPLETE'})
if($IncludeRegression){
 Invoke-Probes @(@{name='physics';args='-offline -footballBallAudit';marker='FOOTBALL_PHYSICS_COMPLETE'})
 $taskBallPort=$Port+1
 Invoke-Probes @(@{name='ball-host';args="-localHost -port $taskBallPort -networkBallAudit -expected 2 -startAt 10 -returnAt 40";marker='NETWORK_FOOTBALL_BALL_COMPLETE'},@{name='ball-client';args="-localClient -port $taskBallPort -networkBallAudit -expected 2 -startAt 10 -returnAt 40";marker='NETWORK_FOOTBALL_BALL_COMPLETE'})
}
Write-Output "FOOTBALL_MATCH_SUITE_PASS $taskOutput"
