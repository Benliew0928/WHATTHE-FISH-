param([string]$PlayerPath,[string]$OutputPath)
$ErrorActionPreference='Stop'
$repoRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $repoRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
elseif(![IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath=Join-Path $repoRoot $PlayerPath}
if(!$OutputPath){$OutputPath=Join-Path $repoRoot ('Builds/FishingWaterQA/Review-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))}
elseif(![IO.Path]::IsPathRooted($OutputPath)){$OutputPath=Join-Path $repoRoot $OutputPath}
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null
$arguments=@('-batchmode','-offline','-sport','Fishing','-lagoonReview',('"{0}"' -f $OutputPath),'-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-logFile',('"{0}"' -f (Join-Path $OutputPath 'player.log')))
$process=Start-Process -FilePath $PlayerPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
try {
 $null=$process.Handle
 if(!$process.WaitForExit(120000)){throw "Lagoon review timed out: $OutputPath"}
 $report=Get-Content -LiteralPath (Join-Path $OutputPath 'results.txt') -Raw
 if($process.ExitCode -ne 0 -or $report -match '(?m)^FAIL ' -or $report -notmatch 'LAGOON_REVIEW_COMPLETE success=True'){throw "Lagoon review failed: $OutputPath"}
 if(Select-String -LiteralPath (Join-Path $OutputPath 'player.log') -Pattern 'Exception:|Shader error|error CS' -Quiet){throw "Runtime error in lagoon review: $OutputPath"}
 Write-Output $report
 Write-Output "Rendered evidence: $OutputPath"
} finally {if(!$process.HasExited){Stop-Process -Id $process.Id -ErrorAction SilentlyContinue}}
