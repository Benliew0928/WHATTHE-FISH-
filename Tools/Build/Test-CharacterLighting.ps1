param([string]$PlayerPath='Builds/CharacterLightingQA/Player/WhatTheFish.exe',[string]$OutputPath='')
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
function Resolve-TaskPath([string]$Value){if([IO.Path]::IsPathRooted($Value)){return $Value};return Join-Path $root $Value}
$player=Resolve-TaskPath $PlayerPath
if(!$OutputPath){$OutputPath='Builds/CharacterLightingQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')}
$output=Resolve-TaskPath $OutputPath
New-Item -ItemType Directory -Force $output | Out-Null
$arguments=@('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-job-worker-count','2','-characterLightingReview',('"'+$output+'"'),'-contentCache',('"'+(Join-Path $output 'Cache')+'"'),'-logFile',('"'+(Join-Path $output 'player.log')+'"'))
$process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
try{if(!$process.WaitForExit(240000)){throw "Character lighting review timed out: $output"}}
finally{if(!$process.HasExited){Stop-Process -Id $process.Id}}
$results=Get-Content -LiteralPath (Join-Path $output 'results.txt') -Raw
if($process.ExitCode -ne 0 -or $results -match '(?m)^FAIL ' -or !$results.Contains('CHARACTER_LIGHTING_REVIEW_COMPLETE success=True')){throw "Character lighting review failed: $output"}
if(Select-String -LiteralPath (Join-Path $output 'player.log') -Pattern 'Exception:|Shader error|CONTENT_LOAD_FAILED|could not be loaded from the resource file' -Quiet){throw "Character lighting runtime error: $output"}
Write-Output "PASS character lighting: $(([regex]::Matches($results,'(?m)^PASS ')).Count) checks; screenshots and report: $output"
