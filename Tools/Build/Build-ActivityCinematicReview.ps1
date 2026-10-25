param([string]$UnityEditorPath=(Join-Path $env:ProgramFiles 'Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe'),[switch]$ScriptsOnly)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$out=Join-Path $root 'Builds/ActivityCinematicsQA'
New-Item -ItemType Directory -Force -Path $out | Out-Null
if($ScriptsOnly -and !(Test-Path -LiteralPath (Join-Path $out 'Player/WhatTheFish.exe'))){throw 'Build the complete review player before refreshing scripts.'}
$log=Join-Path $out ('windows-build-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')+'.log')
$method=if($ScriptsOnly){'WhatTheFish.Editor.ActivityCinematicBuild.Scripts'}else{'WhatTheFish.Editor.ActivityCinematicBuild.Build'}
$process=Start-Process -FilePath $UnityEditorPath -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"'+(Join-Path $root 'Game')+'"'),'-buildTarget','Win64','-job-worker-count','4','-executeMethod',$method,'-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
$null=$process.Handle
$process.WaitForExit()
if($process.ExitCode -ne 0){throw "Unity cinematic build failed: $log"}
Write-Output "ACTIVITY_CINEMATIC_PLAYER_BUILT $log"
