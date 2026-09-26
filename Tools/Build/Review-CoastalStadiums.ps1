param([string]$OutputPath='C:\UMPSA\Docs\VisualDirection\CoastalStadiums\GameReview',[switch]$RecordRoutes)
$ErrorActionPreference='Stop'
$reviewRoot=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null
$reviewLog=Join-Path $reviewRoot 'Builds\coastal-stadium-review.log'
$reviewArgs=@('-batchmode','-force-d3d11','-screen-width','1920','-screen-height','1080','-screen-fullscreen','0','-stadiumReview',('"{0}"' -f $OutputPath),'-logFile',('"{0}"' -f $reviewLog))
if($RecordRoutes){$reviewArgs+='-recordRoutes'}
$reviewProcess=Start-Process -FilePath (Join-Path $reviewRoot 'Builds\WindowsFinal\WhatTheFish.exe') -ArgumentList $reviewArgs -WindowStyle Hidden -PassThru
$null=$reviewProcess.Handle
if(!$reviewProcess.WaitForExit(900000)){Stop-Process -Id $reviewProcess.Id;throw 'Stadium review timed out'}
$reviewText=Get-Content -LiteralPath (Join-Path $OutputPath 'review.txt') -Raw
if($reviewProcess.ExitCode -ne 0 -or $reviewText -match '(?m)^FAIL ' -or $reviewText -notmatch 'STADIUM_REVIEW_COMPLETE'){throw 'Stadium review failed; inspect review.txt and player log'}
if(Select-String -LiteralPath $reviewLog -Pattern 'Exception:|Shader error|error CS|called on inactive controller|required resources|Missing material' -Quiet){throw 'Runtime error in stadium review'}
Write-Output "Stadium review passed. Captures: $OutputPath"
