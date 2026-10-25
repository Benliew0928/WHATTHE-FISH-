param(
    [string]$PlayerPath='Builds/StartupWelcomeQA/Player/WhatTheFish.exe',
    [string]$OutputPath=''
)
$ErrorActionPreference='Stop'
$startupRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
function Resolve-StartupPath([string]$Value){if([IO.Path]::IsPathRooted($Value)){return $Value};return Join-Path $startupRoot $Value}
$startupPlayer=Resolve-StartupPath $PlayerPath
if(!$OutputPath){$OutputPath='Builds/StartupWelcomeQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')}
$startupOutput=Resolve-StartupPath $OutputPath
$startupCache=Join-Path $startupOutput 'ContentCache'
$startupProfile='welcome'+[guid]::NewGuid().ToString('N')
if(Test-Path -LiteralPath $startupOutput){throw 'Choose a new output directory so the first run has a genuinely cold cache.'}
New-Item -ItemType Directory -Path $startupOutput | Out-Null
$startupCases=@(
    @{Name='cold-1600x900';Width=1600;Height=900;Extra=@('-welcomeFailOnce')},
    @{Name='wide-1560x720';Width=1560;Height=720;Extra=@('-expectWarmWelcome')},
    @{Name='tablet-1200x900';Width=1200;Height=900;Extra=@('-expectWarmWelcome')},
    @{Name='fullhd-1920x1080';Width=1920;Height=1080;Extra=@('-expectWarmWelcome')}
)
foreach($startupCase in $startupCases){
    $startupFolder=Join-Path $startupOutput $startupCase.Name
    New-Item -ItemType Directory -Path $startupFolder | Out-Null
    $startupLog=Join-Path $startupFolder 'player.log'
    $startupArgs=@('-batchmode','-force-d3d11','-screen-width',$startupCase.Width,'-screen-height',$startupCase.Height,'-screen-fullscreen','0','-job-worker-count','2','-language','en','-localizationProfile',$startupProfile,'-guestProfile',$startupProfile,'-startupWelcomeReview',('"'+$startupFolder+'"'),'-contentCache',('"'+$startupCache+'"'),'-logFile',('"'+$startupLog+'"'))+$startupCase.Extra
    $startupProcess=Start-Process -FilePath $startupPlayer -ArgumentList $startupArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
    try {
        if(!$startupProcess.WaitForExit(240000)){throw "Startup review timed out: $startupFolder"}
        $startupResult=Get-Content -LiteralPath (Join-Path $startupFolder 'results.txt') -Raw
        if($startupProcess.ExitCode -ne 0 -or $startupResult -match '(?m)^FAIL ' -or !$startupResult.Contains('STARTUP_WELCOME_COMPLETE success=True')){throw "Startup review failed: $startupFolder"}
        $startupLogText=Get-Content -LiteralPath $startupLog -Raw
        if($startupLogText -match 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException|Shader error'){throw "Startup runtime exception: $startupFolder"}
        Write-Output "PASS $($startupCase.Name) checks=$(([regex]::Matches($startupResult,'(?m)^PASS ')).Count) evidence=$startupFolder"
    } finally {if(!$startupProcess.HasExited){Stop-Process -Id $startupProcess.Id}}
}
Write-Output "STARTUP_WELCOME_SUITE_PASS $startupOutput"
