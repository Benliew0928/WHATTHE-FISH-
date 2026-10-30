param(
 [string]$PlayerPath='',
 [string]$OutputPath='',
 [string]$CachePath='',
 [switch]$Baseline,
 [switch]$UseDefaultCache,
 [switch]$CopySource,
 [switch]$RetryReview
)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
function Resolve-TaskPath([string]$Value){if([IO.Path]::IsPathRooted($Value)){return $Value};return Join-Path $root $Value}
if(!$PlayerPath){$PlayerPath='Builds/PackagingQA/Player/WhatTheFish.exe'}
$player=Resolve-TaskPath $PlayerPath
if(!$OutputPath){$OutputPath='Builds/PackagingQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')}
$output=Resolve-TaskPath $OutputPath
if($UseDefaultCache -and ($CachePath -or $RetryReview)){throw 'Default-cache review cannot override or create a recovery fixture in app data.'}
if(!$CachePath){$CachePath=Join-Path $output 'Cache'}else{$CachePath=Resolve-TaskPath $CachePath}
if($RetryReview){
 $normalizedCache=[IO.Path]::GetFullPath($CachePath)
 if(!$normalizedCache.StartsWith([IO.Path]::GetFullPath($output)+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Retry fixture must be inside the review output.'}
 if(Test-Path -LiteralPath $normalizedCache){throw 'Retry fixture already exists; use a fresh review output.'}
 New-Item -ItemType Directory -Force -Path (Split-Path $normalizedCache -Parent) | Out-Null
 [IO.File]::WriteAllBytes($normalizedCache,[byte[]]@(1))
}
$modes=if($Baseline){@('baseline')}elseif($RetryReview){@('retry')}else{@('cold','warm')}
foreach($mode in $modes){
 $folder=Join-Path $output $mode;New-Item -ItemType Directory -Force $folder | Out-Null
 $reviewFolder=if($RetryReview){$output}else{$folder}
 # The recovery fixture asserts the authored English recovery guidance. Do not
 # inherit a language preference left by a previous localization review.
 $arguments=@('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-language','en','-localizationProfile','packaging-review','-packagingReview',('"'+$reviewFolder+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
 if(!$UseDefaultCache){$arguments+=@('-contentCache',('"'+$CachePath+'"'))}
 if($CopySource){$arguments+='-contentCopySource'}
 if($RetryReview){$arguments+='-packagingRetryReview'}
 $clock=[Diagnostics.Stopwatch]::StartNew()
 $process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
 try{if(!$process.WaitForExit(240000)){throw "Packaging review timed out: $folder"}}
 finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
 $clock.Stop()
 $text=Get-Content (Join-Path $reviewFolder 'results.txt') -Raw
 if($process.ExitCode -ne 0 -or $text -match '(?m)^FAIL ' -or !$text.Contains('PACKAGING_REVIEW_COMPLETE success=True')){throw "Packaging review failed: $folder"}
 $log=Get-Content (Join-Path $folder 'player.log') -Raw
 if($log -match 'Exception:|Shader error|could not be loaded from the resource file'){throw "Packaging runtime error: $folder"}
 $expectedFailures=if($RetryReview){1}else{0}
 if(([regex]::Matches($log,'(?m)^CONTENT_LOAD_FAILED ')).Count -ne $expectedFailures){throw "Unexpected preparation failures: $folder"}
 $timings=Get-Content (Join-Path $reviewFolder 'timings.json') -Raw | ConvertFrom-Json
 if(!$Baseline -and (!$timings.packaged -or $timings.cacheHit -ne ($mode -eq 'warm'))){throw "Unexpected packaging/cache mode: $folder"}
 if(!$Baseline -and $mode -ne 'warm' -and !$log.Contains('CONTENT_SOURCE direct='+(!$CopySource))){throw "Unexpected content source mode: $folder"}
 Write-Output "PASS $mode startup=$($timings.startupSeconds) cachePrepare=$($timings.preparationSeconds) checks=$(([regex]::Matches($text,'(?m)^PASS ')).Count) totalSeconds=$($clock.Elapsed.TotalSeconds) evidence=$folder"
}
