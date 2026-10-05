param(
    [string]$PlayerPath,
    [ValidateSet('Offline','Network','All')][string]$Mode='All',
    [int]$Port=7943,
    [switch]$Windowed,
    [string]$OutputName=('Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$taskOutput=Join-Path $taskRoot ('Builds/StorybookMenuQA/'+$OutputName)
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
function Invoke-MenuReview($roles){
    $taskProcesses=@()
    try {
        foreach($taskRole in $roles){
            $taskFolder=Join-Path $taskOutput $taskRole
            New-Item -ItemType Directory -Force -Path $taskFolder | Out-Null
            $taskArgs=@('-batchmode','-force-d3d11','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-job-worker-count','2','-coveReview',('"'+$taskFolder+'"'),'-coveRole',$taskRole,'-logFile',('"'+(Join-Path $taskFolder 'player.log')+'"'))
            if($Windowed){$taskArgs=@($taskArgs | Where-Object {$_ -ne '-batchmode'})}
            if($taskRole -ne 'offline'){$taskArgs+=@($(if($taskRole -eq 'host'){'-localHost'}else{'-localClient'}),'-port',$Port)}
            $taskProcesses+=Start-Process -FilePath $PlayerPath -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
            if($taskRole -eq 'host'){Start-Sleep -Seconds 3}
        }
        $taskDeadline=(Get-Date).AddSeconds(220)
        do {
            $taskDone=@($roles | Where-Object {$taskResult=Join-Path $taskOutput ($_+'/results.txt');(Test-Path -LiteralPath $taskResult) -and (Select-String -LiteralPath $taskResult -Pattern 'COVE_REVIEW_COMPLETE' -Quiet)})
            if($taskDone.Count -eq $roles.Count){break}
            if(@($taskProcesses | Where-Object {$_.HasExited}).Count){break}
            Start-Sleep -Milliseconds 500
        }while((Get-Date) -lt $taskDeadline)
        foreach($taskRole in $roles){
            $taskFolder=Join-Path $taskOutput $taskRole
            $taskText=Get-Content -LiteralPath (Join-Path $taskFolder 'results.txt') -Raw
            if($taskText -match '(?m)^FAIL ' -or !$taskText.Contains('COVE_REVIEW_COMPLETE success=True')){throw "Menu review failed/incomplete: $taskFolder"}
            if(Select-String -LiteralPath (Join-Path $taskFolder 'player.log') -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException' -Quiet){throw "Runtime exception: $taskFolder"}
            Write-Output "PASS $taskRole assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count)"
            foreach($taskLimit in [regex]::Matches($taskText,'(?m)^LIMIT .+$')){Write-Output $taskLimit.Value.Trim()}
        }
    }finally{foreach($taskProcess in $taskProcesses){if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id}}}
}
if($Mode -ne 'Network'){Invoke-MenuReview @('offline')}
if($Mode -ne 'Offline'){Invoke-MenuReview @('host','client')}
Write-Output "STORYBOOK_MENU_SUITE_PASS $taskOutput"
