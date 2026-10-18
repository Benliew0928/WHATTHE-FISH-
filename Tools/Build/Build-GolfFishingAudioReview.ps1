param([string]$UnityEditorPath=(Join-Path $env:ProgramFiles 'Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe'),[switch]$ScriptsOnly)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$output=Join-Path $root 'Builds/GolfFishingAudioQA'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$log=Join-Path $output ('build-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.log')
$method=if($ScriptsOnly){'GolfFishingAudioBuild.BuildScripts'}else{'GolfFishingAudioBuild.BuildReview'}
$process=Start-Process -FilePath $UnityEditorPath -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"'+(Join-Path $root 'Game')+'"'),'-buildTarget','Win64','-job-worker-count','4','-executeMethod',$method,'-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
$process.WaitForExit()
if($process.ExitCode -ne 0){throw "Unity audio build failed: $log"}
Write-Output "GOLF_FISHING_AUDIO_PLAYER_BUILT $log"
