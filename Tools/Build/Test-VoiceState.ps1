param([string]$OutputDirectory)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$OutputDirectory){$OutputDirectory=Join-Path $taskRoot ('Builds/VoiceQA/State-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))}
elseif(![IO.Path]::IsPathRooted($OutputDirectory)){$OutputDirectory=Join-Path $taskRoot $OutputDirectory}
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
Add-Type -Path @((Join-Path $taskRoot 'Game/Assets/_Game/Shared/Networking/RoomVoiceSession.cs'),(Join-Path $taskRoot 'Game/Assets/_Game/Shared/Core/RoomVoiceChecks.cs')) -CompilerOptions '/define:DEVELOPMENT_BUILD'
$taskResults=[WhatTheFish.RoomVoiceChecks]::Run().GetAwaiter().GetResult()
$taskResults | Set-Content -LiteralPath (Join-Path $OutputDirectory 'results.txt')
"VOICE_STATE_PASS $($taskResults.Count) assertions; $OutputDirectory"
