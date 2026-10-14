param(
 [string]$PlayerPath='',
 [string]$OutputPath='',
 [string]$CachePath='',
 [switch]$Baseline
)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
function Resolve-TaskPath([string]$Value){if([IO.Path]::IsPathRooted($Value)){return $Value};return Join-Path $root $Value}
if(!$PlayerPath){$PlayerPath='Builds/PackagingQA/Player/WhatTheFish.exe'}
$player=Resolve-TaskPath $PlayerPath
if(!$OutputPath){$OutputPath='Builds/PackagingQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')}
$output=Resolve-TaskPath $OutputPath
if(!$CachePath){$CachePath=Join-Path $output 'Cache'}else{$CachePath=Resolve-TaskPath $CachePath}
$modes=if($Baseline){@('baseline')}else{@('cold','warm')}
foreach($mode in $modes){
 $folder=Join-Path $output $mode;New-Item -ItemType Directory -Force $folder | Out-Null
 $arguments=@('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-packagingReview',('"'+$folder+'"'),'-contentCache',('"'+$CachePath+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
 $clock=[Diagnostics.Stopwatch]::StartNew()
 $process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
 try{if(!$process.WaitForExit(240000)){throw "Packaging review timed out: $folder"}}
 finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
 $clock.Stop()
 $text=Get-Content (Join-Path $folder 'results.txt') -Raw
 if($process.ExitCode -ne 0 -or $text -match '(?m)^FAIL ' -or !$text.Contains('PACKAGING_REVIEW_COMPLETE success=True')){throw "Packaging review failed: $folder"}
 if(Select-String (Join-Path $folder 'player.log') -Pattern 'Exception:|Shader error|CONTENT_LOAD_FAILED|could not be loaded from the resource file' -Quiet){throw "Packaging runtime error: $folder"}
 $timings=Get-Content (Join-Path $folder 'timings.json') -Raw | ConvertFrom-Json
 if(!$Baseline -and (!$timings.packaged -or $timings.cacheHit -ne ($mode -eq 'warm'))){throw "Unexpected packaging/cache mode: $folder"}
 Write-Output "PASS $mode startup=$($timings.startupSeconds) cachePrepare=$($timings.preparationSeconds) checks=$(([regex]::Matches($text,'(?m)^PASS ')).Count) totalSeconds=$($clock.Elapsed.TotalSeconds) evidence=$folder"
}
