param([string]$PlayerPath='Builds/ResponsiveLayoutQA/Player/WhatTheFish.exe',[string]$OutputPath='Builds/ResponsiveLayoutQA/Evidence',[switch]$PortraitOnly)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(![IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath=Join-Path $taskRoot $PlayerPath}
if(![IO.Path]::IsPathRooted($OutputPath)){$OutputPath=Join-Path $taskRoot $OutputPath}
if(Test-Path -LiteralPath $OutputPath){throw 'Choose a new evidence folder.'}
New-Item -ItemType Directory -Path $OutputPath | Out-Null
$arguments=@('-batchmode','-force-d3d11','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-job-worker-count','2','-skipWelcome','-guestProfile','responsive-layout-review','-offline','-sport','Football','-localizationProfile','responsive-layout-review','-responsiveLayoutReview',('"'+$OutputPath+'"'),'-contentCache',('"'+(Join-Path $taskRoot 'Builds/ResponsiveLayoutQA/ContentCache')+'"'),'-logFile',('"'+(Join-Path $OutputPath 'player.log')+'"'))
if($PortraitOnly){$arguments+='-responsivePortraitOnly'}
$process=Start-Process -FilePath $PlayerPath -ArgumentList $arguments -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
try{
 $null=$process.Handle;if(!$process.WaitForExit(600000)){throw "Review timed out: $OutputPath"}
 $result=Get-Content -LiteralPath (Join-Path $OutputPath 'results.txt') -Raw
 if($process.ExitCode -ne 0 -or $result -match '(?m)^FAIL ' -or $result -notmatch 'RESPONSIVE_LAYOUT_COMPLETE success=True'){throw "Review failed: $OutputPath"}
 if(Select-String -LiteralPath (Join-Path $OutputPath 'player.log') -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException|Shader error' -Quiet){throw "Runtime error: $OutputPath"}
 Write-Output "RESPONSIVE_LAYOUT_SUITE_PASS checks=$(([regex]::Matches($result,'(?m)^PASS ')).Count) evidence=$OutputPath"
}finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
