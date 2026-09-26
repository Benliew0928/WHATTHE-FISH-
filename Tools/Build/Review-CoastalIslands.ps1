param([string]$OutputPath='C:\UMPSA\Docs\VisualDirection\CoastalIslands')
$ErrorActionPreference='Stop'
$reviewRoot=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null
$reviewLog=Join-Path $reviewRoot 'Builds\coastal-review-player.log'
$reviewArgs=@('-batchmode','-screen-width','1920','-screen-height','1080','-screen-fullscreen','0','-coastalReview',('"{0}"' -f $OutputPath),'-logFile',('"{0}"' -f $reviewLog))
$reviewProcess=Start-Process -FilePath (Join-Path $reviewRoot 'Builds\WindowsFinal\WhatTheFish.exe') -ArgumentList $reviewArgs -WindowStyle Hidden -PassThru
$null=$reviewProcess.Handle
if(!$reviewProcess.WaitForExit(180000)){Stop-Process -Id $reviewProcess.Id;throw 'Coastal review timed out'}
$reviewText=Get-Content -LiteralPath (Join-Path $OutputPath 'review.txt') -Raw
if($reviewProcess.ExitCode -ne 0 -or $reviewText -match '(?m)^FAIL ' -or $reviewText -notmatch 'COASTAL_REVIEW_COMPLETE'){throw 'Coastal review failed; inspect review.txt and player log'}
if(Select-String -LiteralPath $reviewLog -Pattern 'Exception:|Shader error|error CS|called on inactive controller|required resources|Missing material' -Quiet){throw 'Runtime error in coastal review'}
Write-Output "Coastal review passed. Captures: $OutputPath"
