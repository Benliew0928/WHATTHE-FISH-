param([string]$OutputPath='')
$ErrorActionPreference='Stop'
$benchmarkRoot=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if (-not $OutputPath) { $OutputPath=Join-Path $benchmarkRoot 'Docs\VisualDirection\GolfFishingRefinement\GameReview' }
elseif (-not [IO.Path]::IsPathRooted($OutputPath)) { $OutputPath=Join-Path $benchmarkRoot $OutputPath }
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$arguments=@('-force-d3d11','-screen-width','1920','-screen-height','1080','-screen-fullscreen','0','-islandBenchmark',('"{0}\performance.txt"' -f $OutputPath),'-logFile',('"{0}\benchmark-player.log"' -f $OutputPath))
$measurementStart=[DateTime]::UtcNow
# The user requested measurement of the visible Windows player. Do not use batch
# mode or a hidden window here; the runtime verifies actual camera render counts.
$benchmarkPlayer=Start-Process -FilePath (Join-Path $benchmarkRoot 'Builds\WindowsFinal\WhatTheFish.exe') -ArgumentList $arguments -WindowStyle Normal -PassThru
$null=$benchmarkPlayer.Handle
try {
 $memory=@{};$deadline=(Get-Date).AddSeconds(600);$lastMemory=$null
 do {
  $benchmarkPlayer.Refresh()
  if(!$benchmarkPlayer.HasExited){$lastMemory=@{working_set_MiB=[Math]::Round($benchmarkPlayer.WorkingSet64/1MB,1);private_MiB=[Math]::Round($benchmarkPlayer.PrivateMemorySize64/1MB,1);sampledUTC=[DateTime]::UtcNow.ToString('O')}}
  $metricsFile=Join-Path $OutputPath 'performance.txt'
  if((Test-Path -LiteralPath $metricsFile) -and (Get-Item -LiteralPath $metricsFile).LastWriteTimeUtc -ge $measurementStart){
   foreach($line in Get-Content -LiteralPath $metricsFile){if($line -match '^(?<label>.+): samples='){
    $label=$Matches.label;if(!$memory.ContainsKey($label) -and $lastMemory){$memory[$label]=$lastMemory.Clone()}
   }}
  }
  if($benchmarkPlayer.WaitForExit(250)){break}
 }while((Get-Date) -lt $deadline)
 if(!$benchmarkPlayer.HasExited){throw 'Visible benchmark timed out'}
 $report=Get-Content -LiteralPath (Join-Path $OutputPath 'performance.txt') -Raw
 if($report -match 'INVALID' -or $report -notmatch 'ISLAND_BENCHMARK_COMPLETE'){throw 'Visible benchmark measurements incomplete'}
 "Measurement complete; process exit code: $($benchmarkPlayer.ExitCode)" | Set-Content -LiteralPath (Join-Path $OutputPath 'benchmark-exit.txt')
 # Keep completed measurements when the separately tracked native teardown
 # access violation occurs after ISLAND_BENCHMARK_COMPLETE. Never hide the exit.
 if($benchmarkPlayer.ExitCode -ne 0 -and $benchmarkPlayer.ExitCode -ne -1073741819){throw "Unexpected benchmark exit: $($benchmarkPlayer.ExitCode)"}
 if(Select-String -LiteralPath (Join-Path $OutputPath 'benchmark-player.log') -Pattern 'Exception:|Shader error|called on inactive controller' -Quiet){throw 'Runtime error during benchmark'}
 if($memory.Count -ne 8){throw 'Expected Windows memory samples for all eight measured views/runs'}
 Copy-Item -LiteralPath $metricsFile -Destination (Join-Path $OutputPath 'performance-runtime.txt') -Force
 $memory | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $OutputPath 'process-memory-samples.json')
 $lines=@('Process memory: Windows Process API, sampled within 250ms of each view/run report; original Unity/Mono output preserved in performance-runtime.txt.')
 foreach($line in ($report -split '\r?\n')){
  if($line -match '^(?<label>.+): samples='){
   $sample=$memory[$Matches.label]
   if($sample.working_set_MiB -le 0 -or $sample.private_MiB -le 0){throw 'Invalid Windows process memory sample'}
   $line=$line -replace 'working_set_MiB=\d+',('working_set_MiB='+$sample.working_set_MiB) -replace 'private_MiB=\d+',('private_MiB='+$sample.private_MiB)
  }
  $lines+=$line
 }
 $lines | Set-Content -LiteralPath $metricsFile
 if($benchmarkPlayer.ExitCode -ne 0){'Known separate native Unity shutdown access violation after completed measurements; see benchmark-exit.txt.' | Add-Content -LiteralPath $metricsFile}
 "PASS visible 1080p island benchmark: $OutputPath"
}finally{if(!$benchmarkPlayer.HasExited){Stop-Process -Id $benchmarkPlayer.Id -ErrorAction SilentlyContinue}}
