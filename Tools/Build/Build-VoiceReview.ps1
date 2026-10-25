param([string]$UnityEditorPath=(Join-Path $env:ProgramFiles 'Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe'),[switch]$ScriptsOnly)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskOutput=Join-Path $taskRoot 'Builds/VoiceQA'
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
if($ScriptsOnly -and !(Test-Path -LiteralPath (Join-Path $taskOutput 'Player/WhatTheFish.exe'))){throw 'Build the complete voice review player before refreshing scripts.'}
$taskLog=Join-Path $taskOutput ('windows-build-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')+'.log')
$taskMethod=if($ScriptsOnly){'WhatTheFish.Editor.RoomVoiceBuild.Scripts'}else{'WhatTheFish.Editor.RoomVoiceBuild.Windows'}
$taskProcess=Start-Process -FilePath $UnityEditorPath -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"'+(Join-Path $taskRoot 'Game')+'"'),'-buildTarget','Win64','-job-worker-count','2','-executeMethod',$taskMethod,'-logFile',('"'+$taskLog+'"')) -WindowStyle Hidden -PassThru
$null=$taskProcess.Handle
$taskProcess.WaitForExit()
if($taskProcess.ExitCode -ne 0){throw "Unity voice review build failed: $taskLog"}
"VOICE_PLAYER_BUILT $taskLog"
