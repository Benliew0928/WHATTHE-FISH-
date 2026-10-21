param(
 [string]$PlayerPath,
 [ValidateSet('Offline','Network','All')][string]$Mode='All'
)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$taskOutput=Join-Path $taskRoot ('Builds/VenueCustomizationQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
function Invoke-VenueReview([string]$sport,[string[]]$roles){
 $taskProcesses=@()
 try {
  foreach($taskRole in $roles){
   $taskFolder=Join-Path $taskOutput "$sport-$taskRole"
   New-Item -ItemType Directory -Force -Path $taskFolder | Out-Null
   $taskArgs="-batchmode -force-d3d11 -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -job-worker-count 2 -skipWelcome -sport $sport -venueReview `"$taskFolder`" -logFile `"$taskFolder/player.log`""
   if($taskRole -eq 'host'){$taskArgs+=' -localHost -port 7988'}
   if($taskRole -eq 'guest'){$taskArgs+=' -localClient -port 7988'}
   $taskProcesses+=Start-Process -FilePath $PlayerPath -ArgumentList $taskArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
   if($taskRole -eq 'host'){Start-Sleep -Seconds 5}
  }
  $taskDeadline=(Get-Date).AddSeconds(230)
  do {
   $taskDone=@($roles | Where-Object {$taskResult=Join-Path $taskOutput "$sport-$_/results.txt";(Test-Path -LiteralPath $taskResult) -and (Select-String -LiteralPath $taskResult -Pattern 'VENUE_REVIEW_COMPLETE' -Quiet)})
   if($taskDone.Count -eq $roles.Count){break}
   Start-Sleep -Milliseconds 500
  }while((Get-Date) -lt $taskDeadline)
  foreach($taskRole in $roles){
   $taskFolder=Join-Path $taskOutput "$sport-$taskRole";$taskText=Get-Content -LiteralPath "$taskFolder/results.txt" -Raw
   if($taskText -match '(?m)^FAIL ' -or !$taskText.Contains('VENUE_REVIEW_COMPLETE success=True')){throw "Venue review failed/incomplete: $taskFolder"}
   if(Select-String -LiteralPath "$taskFolder/player.log" -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException|Shader error' -Quiet){throw "Runtime error: $taskFolder"}
   "PASS $sport $taskRole assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count)" | Tee-Object -FilePath "$taskOutput/results.txt" -Append
  }
 }finally{foreach($taskProcess in $taskProcesses){if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id -ErrorAction SilentlyContinue}}}
}
if($Mode -ne 'Network'){Invoke-VenueReview 'Football' @('offline')}
if($Mode -ne 'Offline'){foreach($taskSport in @('Golf','Fishing')){Invoke-VenueReview $taskSport @('host','guest')}}
"VENUE_CUSTOMIZATION_SUITE_PASS $taskOutput"
