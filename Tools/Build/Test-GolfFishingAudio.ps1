param([ValidateSet('Golf','Fishing')][string]$Sport='Golf',[string]$PlayerPath)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $root 'Builds/GolfFishingAudioQA/Player/WhatTheFish.exe'}
$output=Join-Path $root ('Builds/GolfFishingAudioQA/'+$Sport+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $output -Force | Out-Null
$arguments=@('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-sport',$Sport,'-offline','-golfFishingAudioReview',('"'+$output+'"'),'-logFile',('"'+(Join-Path $output 'player.log')+'"'))
$process=Start-Process -FilePath $PlayerPath -ArgumentList $arguments -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
try {
 $deadline=(Get-Date).AddSeconds(150)
 do {if($process.HasExited){break};Start-Sleep -Milliseconds 500}while((Get-Date) -lt $deadline)
 $report=Join-Path $output 'results.txt'
 if(!(Test-Path -LiteralPath $report)){throw "No audio report: $output"}
 $text=Get-Content -LiteralPath $report -Raw
 if($text -match '(?m)^FAIL ' -or $text -notmatch 'GOLF_FISHING_AUDIO_COMPLETE success=True'){throw "Audio review incomplete/failed: $output"}
 if(Select-String -LiteralPath (Join-Path $output 'player.log') -Pattern 'Exception:|error CS|MissingReferenceException' -Quiet){throw "Audio runtime exception: $output"}
 Write-Output "GOLF_FISHING_AUDIO_PASS $Sport assertions=$(([regex]::Matches($text,'(?m)^PASS ')).Count) $output"
}finally {if(!$process.HasExited){Stop-Process -Id $process.Id}}
