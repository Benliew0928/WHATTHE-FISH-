param([switch]$Network, [switch]$CaptureOnly, [switch]$WalkOnly, [string]$OutputPath='C:\UMPSA\Docs\VisualDirection\WorldConnections\GameReview')
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$playerPath=Join-Path $taskRoot 'Builds\WindowsFinal\WhatTheFish.exe'
function Start-SkyReview([string]$Folder,[string[]]$Extra){
 New-Item -ItemType Directory -Force -Path $Folder | Out-Null
 $playerArgs=@('-batchmode','-force-d3d11','-screen-width','1920','-screen-height','1080','-screen-fullscreen','0','-skyReview',('"{0}"' -f $Folder),'-logFile',('"{0}\player.log"' -f $Folder))+$Extra
 return Start-Process -FilePath $playerPath -ArgumentList $playerArgs -WindowStyle Hidden -PassThru
}
function Assert-SkyReview($Process,[string]$Folder){
 if(-not $Process.WaitForExit(540000)){Stop-Process -Id $Process.Id;throw "Sky-Sail review timeout: $Folder"}
 $report=Get-Content -LiteralPath (Join-Path $Folder 'review.txt') -Raw
 if($report -match '(?m)^FAIL ' -or $report -notmatch 'SKY_SAIL_REVIEW_COMPLETE success=True'){throw "Sky-Sail review failed: $Folder"}
 if(Select-String -LiteralPath (Join-Path $Folder 'player.log') -Pattern 'Exception:|Shader error|called on inactive controller|Missing material' -Quiet){throw "Sky-Sail runtime error: $Folder"}
 Write-Output "PASS Sky-Sail: $Folder (exit $($Process.ExitCode))"
}
if($Network){
 $hostFolder=Join-Path $OutputPath 'Host';$guestFolder=Join-Path $OutputPath 'Guest'
 $hostProcess=Start-SkyReview $hostFolder @('-skyNetworkReview','-localHost','-port','7793')
 Start-Sleep -Seconds 2
 $guestProcess=Start-SkyReview $guestFolder @('-skyNetworkReview','-localClient','-port','7793')
 Assert-SkyReview $guestProcess $guestFolder
 Assert-SkyReview $hostProcess $hostFolder
}else{
 $extra=if($CaptureOnly){@('-skyCaptureOnly')}elseif($WalkOnly){@('-skyWalkReview')}else{@()}
 $reviewProcess=Start-SkyReview $OutputPath $extra
 Assert-SkyReview $reviewProcess $OutputPath
}
