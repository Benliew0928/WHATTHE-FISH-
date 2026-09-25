param(
    [ValidateSet('Windows','Android','AndroidRelease','Scene')][string]$Target = 'Windows',
    [string]$UnityEditorPath = (Join-Path $env:ProgramFiles 'Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe')
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$gamePath = Join-Path $repoRoot 'Game'
$buildRoot = Join-Path $repoRoot 'Builds'
$logPath = Join-Path $buildRoot "build-$Target.log"

if (-not (Test-Path -LiteralPath $UnityEditorPath -PathType Leaf)) {
    throw "Unity Editor not found at $UnityEditorPath. Install 6000.3.20f1 or pass -UnityEditorPath."
}
New-Item -ItemType Directory -Path $buildRoot -Force | Out-Null

$method = switch ($Target) {
    'Windows' { 'ProjectBuilder.BuildWindows' }
    'Android' { 'ProjectBuilder.BuildAndroid' }
    'AndroidRelease' { 'ProjectBuilder.BuildAndroidRelease' }
    'Scene' { 'ProjectBuilder.Setup' }
}
$argsList = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"{0}"' -f $gamePath), '-executeMethod', $method, '-logFile', ('"{0}"' -f $logPath))
if ($Target -in @('Android', 'AndroidRelease')) { $argsList += @('-buildTarget', 'Android') }
if ($Target -eq 'Windows') { $argsList += @('-buildTarget', 'Win64') }

$process = Start-Process -FilePath $UnityEditorPath -ArgumentList $argsList -PassThru -WindowStyle Hidden
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw "Unity failed ($($process.ExitCode)). Read $logPath" }
