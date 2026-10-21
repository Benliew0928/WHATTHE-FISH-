param(
    [string]$PlayerPath='Builds/AccountSignInQA/Player/WhatTheFish.exe',
    [string]$OutputPath=''
)
$ErrorActionPreference='Stop'
$accountRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$accountPlayer=if([IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath}else{Join-Path $accountRoot $PlayerPath}
if(!$OutputPath){$OutputPath='Builds/AccountSignInQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')}
$accountOutput=if([IO.Path]::IsPathRooted($OutputPath)){$OutputPath}else{Join-Path $accountRoot $OutputPath}
New-Item -ItemType Directory -Path $accountOutput | Out-Null
& node --test (Join-Path $accountRoot 'Tools/AccountServer/auth.test.mjs') (Join-Path $accountRoot 'Tools/AccountServer/worker.test.mjs') *> (Join-Path $accountOutput 'backend-tests.txt')
if($LASTEXITCODE -ne 0){throw 'Authentication backend tests failed.'}
foreach($accountSize in @(@{Width=1600;Height=900},@{Width=1200;Height=900})){
    $accountCase="$($accountSize.Width)x$($accountSize.Height)"
    $accountFolder=Join-Path $accountOutput $accountCase
    New-Item -ItemType Directory -Path $accountFolder | Out-Null
    $accountArgs=@('-batchmode','-force-d3d11','-screen-width',$accountSize.Width,'-screen-height',$accountSize.Height,'-screen-fullscreen','0','-job-worker-count','2','-guestProfile',('account'+(Split-Path $accountOutput -Leaf)+$accountCase),'-accountSignInReview',('"'+$accountFolder+'"'),'-logFile',('"'+(Join-Path $accountFolder 'player.log')+'"'))
    $accountProcess=Start-Process -FilePath $accountPlayer -ArgumentList $accountArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
    try {
        if(!$accountProcess.WaitForExit(180000)){throw "Account review timed out: $accountCase"}
        $accountResult=Get-Content -LiteralPath (Join-Path $accountFolder 'results.txt') -Raw
        if($accountProcess.ExitCode -ne 0 -or $accountResult -match '(?m)^FAIL ' -or !$accountResult.Contains('ACCOUNT_REVIEW_COMPLETE success=True')){throw "Account review failed: $accountCase"}
        if(Select-String -LiteralPath (Join-Path $accountFolder 'player.log') -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException' -Quiet){throw "Runtime exception: $accountCase"}
        Write-Output "PASS account $accountCase assertions=$(([regex]::Matches($accountResult,'(?m)^PASS ')).Count)"
    } finally {if(!$accountProcess.HasExited){Stop-Process -Id $accountProcess.Id}}
}
Write-Output "ACCOUNT_SIGNIN_SUITE_PASS $accountOutput"
