param([string]$PlayerPath,[string]$OutputDirectory)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(-not $PlayerPath){$PlayerPath=Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'}
if(-not $OutputDirectory){$OutputDirectory=Join-Path $root ('Builds/GolfEquipment/20261001/Player-'+(Get-Date -Format 'HHmmss'))}
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$argsList=@('-batchmode','-screen-width','1600','-screen-height','1100','-screen-fullscreen','0','-offline','-sport','Golf','-golfEquipmentReview',('"'+$OutputDirectory+'"'),'-logFile',('"'+(Join-Path $OutputDirectory 'player.log')+'"'))
$process=Start-Process -FilePath $PlayerPath -ArgumentList $argsList -WindowStyle Hidden -PassThru
try {
 if(!$process.WaitForExit(120000)){throw 'Golf equipment review timed out.'}
 $results=Get-Content -LiteralPath (Join-Path $OutputDirectory 'results.txt') -Raw
 if($process.ExitCode -ne 0 -or $results -match '(?m)^FAIL ' -or $results -notmatch 'GOLF_EQUIPMENT_REVIEW_COMPLETE success=True'){throw 'Golf equipment review failed.'}
 if((Get-ChildItem -LiteralPath $OutputDirectory -Filter '*.png').Count -ne 7){throw 'Expected seven actual player captures.'}
 if(Select-String -LiteralPath (Join-Path $OutputDirectory 'player.log') -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException' -Quiet){throw 'Player exception.'}
 Write-Output $results
 Write-Output 'GOLF_EQUIPMENT_SUITE_PASS'
} finally {
 if(!$process.HasExited){Stop-Process -Id $process.Id}
 Write-Output ('Evidence: '+$OutputDirectory)
}
