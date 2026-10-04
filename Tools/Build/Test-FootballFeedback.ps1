param([string]$PlayerPath)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$taskOutput=Join-Path $taskRoot ('Builds/FootballFeedbackQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
$taskLog=Join-Path $taskOutput 'player.log'
$taskStart=[Diagnostics.ProcessStartInfo]::new()
$taskStart.FileName=$PlayerPath
$taskStart.UseShellExecute=$false
$taskStart.CreateNoWindow=$true
$taskStart.WindowStyle=[Diagnostics.ProcessWindowStyle]::Hidden
foreach($taskArgument in @('-offline','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-footballFeedbackReview',$taskOutput,'-logFile',$taskLog)){$taskStart.ArgumentList.Add($taskArgument)}
$taskProcess=[Diagnostics.Process]::Start($taskStart)
try {
 $taskDeadline=(Get-Date).AddSeconds(100)
 $taskReport=Join-Path $taskOutput 'results.txt'
 do {
  $taskResult=if(Test-Path -LiteralPath $taskReport){Get-Content -LiteralPath $taskReport -Raw}else{''}
  if($taskResult.Contains('FOOTBALL_FEEDBACK_COMPLETE') -or $taskProcess.HasExited){break}
  Start-Sleep -Milliseconds 250
 }while((Get-Date) -lt $taskDeadline)
 if($taskResult -match '(?m)^FAIL ' -or !$taskResult.Contains('FOOTBALL_FEEDBACK_COMPLETE success=True')){throw "Feedback review failed/incomplete: $taskOutput"}
 if((Get-Content -LiteralPath $taskLog -Raw) -match '(?m)(NullReferenceException|MissingReferenceException|Shader error|ERROR: Shader)'){throw "Rendering/runtime error: $taskOutput"}
}finally{
 # Match the other player suites: the harness owns process lifetime after completion.
 if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}
 $taskProcess.Dispose()
}
Write-Output "FOOTBALL_FEEDBACK_PASS $taskOutput"
