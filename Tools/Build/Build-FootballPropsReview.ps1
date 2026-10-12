param([string]$UnityEditorPath=(Join-Path $env:ProgramFiles 'Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe'))
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$out=Join-Path $root 'Builds/FootballPropsQA'
New-Item -ItemType Directory -Path $out -Force | Out-Null
$log=Join-Path $out ('windows-build-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.log')
$process=Start-Process -FilePath $UnityEditorPath -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"'+(Join-Path $root 'Game')+'"'),'-buildTarget','Win64','-job-worker-count','4','-executeMethod','FootballPropBuild.BuildReview','-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
$process.WaitForExit()
if($process.ExitCode -ne 0){throw "Unity football props build failed: $log"}
Write-Output "FOOTBALL_PROPS_PLAYER_BUILT $log"
