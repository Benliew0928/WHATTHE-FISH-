param([string]$OutputPath='',[switch]$RecordRoutes,[switch]$CaptureOnly)
$ErrorActionPreference='Stop'
$reviewRoot=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if (-not $OutputPath) { $OutputPath=Join-Path $reviewRoot 'Docs\VisualDirection\GolfFishingRefinement\GameReview' }
elseif (-not [IO.Path]::IsPathRooted($OutputPath)) { $OutputPath=Join-Path $reviewRoot $OutputPath }
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$arguments=@('-batchmode','-force-d3d11','-screen-width','1920','-screen-height','1080','-screen-fullscreen','0','-islandReview',('"{0}"' -f $OutputPath),'-logFile',('"{0}\player.log"' -f $OutputPath))
if($RecordRoutes){$arguments+='-recordRoutes'}
if($CaptureOnly){$arguments+='-captureOnly'}
$player=Start-Process -FilePath (Join-Path $reviewRoot 'Builds\WindowsFinal\WhatTheFish.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru
$null=$player.Handle
if(!$player.WaitForExit(1200000)){Stop-Process -Id $player.Id;throw 'Island review timed out'}
$report=Get-Content -LiteralPath (Join-Path $OutputPath 'review.txt') -Raw
if($player.ExitCode -ne 0 -or $report -match '(?m)^FAIL ' -or $report -notmatch 'ISLAND_REVIEW_COMPLETE success=True'){throw "Island review failed: $OutputPath"}
if(Select-String -LiteralPath (Join-Path $OutputPath 'player.log') -Pattern 'Exception:|Shader error|called on inactive controller|required resources|Missing material' -Quiet){throw 'Runtime error in island review'}
Write-Output "PASS G2/L2 review: $OutputPath"
