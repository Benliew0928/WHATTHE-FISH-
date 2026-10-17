param([string]$PlayerPath='')
$ErrorActionPreference='Stop'
$teleportRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $teleportRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
elseif(![IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath=Join-Path $teleportRoot $PlayerPath}
if(!(Test-Path -LiteralPath $PlayerPath -PathType Leaf)){throw 'Build the Windows player first, or supply -PlayerPath.'}
# This launcher intentionally opens the interactive design preview for the user.
Start-Process -FilePath $PlayerPath -ArgumentList @('-offline','-teleportPreview','-screen-fullscreen','0','-screen-width','1600','-screen-height','900')
