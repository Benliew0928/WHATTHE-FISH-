param(
    [ValidateSet('Windows','Android','AndroidRelease','AndroidSubmission','OptimizeAndroid','MobilePreview','Scene')][string]$Target = 'Windows',
    [string]$UnityEditorPath = (Join-Path $env:ProgramFiles 'Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe'),
    [ValidateRange(0,64)][int]$JobWorkerCount = 0
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
    'AndroidSubmission' { 'ProjectBuilder.BuildAndroidSubmission' }
    'OptimizeAndroid' { 'MobileAssetOptimizer.PrepareAndBuild' }
    'MobilePreview' { 'MobileOptimizationValidation.BuildPreview' }
    'Scene' { if (Test-Path -LiteralPath (Join-Path $gamePath 'Assets/_Game/Scenes/SkySail_Football.unity')) { 'SkySailBuilder.Prepare' } else { 'ProjectBuilder.Setup' } }
}
$argsList = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"{0}"' -f $gamePath), '-executeMethod', $method, '-logFile', ('"{0}"' -f $logPath))
if ($JobWorkerCount -gt 0) { $argsList += @('-job-worker-count', $JobWorkerCount.ToString()) }
if ($Target -in @('Android', 'AndroidRelease', 'AndroidSubmission', 'OptimizeAndroid')) { $argsList += @('-buildTarget', 'Android') }
if ($Target -in @('Windows','MobilePreview')) { $argsList += @('-buildTarget', 'Win64') }

$process = Start-Process -FilePath $UnityEditorPath -ArgumentList $argsList -PassThru -WindowStyle Hidden
$process.WaitForExit()
if ($Target -in @('Android', 'AndroidRelease', 'AndroidSubmission','OptimizeAndroid')) {
    $auditPath=Join-Path $buildRoot 'SizeAudit\latest'
    New-Item -ItemType Directory -Force -Path $auditPath | Out-Null
    Copy-Item -LiteralPath $logPath -Destination (Join-Path $auditPath 'build.log')
    $reportPath=Join-Path $gamePath 'Library\LastBuild.buildreport'
    if (Test-Path -LiteralPath $reportPath) { Copy-Item -LiteralPath $reportPath -Destination (Join-Path $auditPath 'LastBuild.buildreport') }
}
if ($process.ExitCode -ne 0) { throw "Unity failed ($($process.ExitCode)). Read $logPath" }
