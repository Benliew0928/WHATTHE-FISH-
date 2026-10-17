param([switch]$Build,[switch]$Local,[string]$PlayerPath,[int]$Width=1600,[int]$Height=900,[string]$UnityEditorPath=(Join-Path $env:ProgramFiles 'Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe'))
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$evidence=Join-Path $root 'Builds/BasketballRestartQA'
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
if($Build){
 $log=Join-Path $evidence ('build-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.log')
 $p=Start-Process -FilePath $UnityEditorPath -ArgumentList @('-batchmode','-nographics','-quit','-projectPath',('"'+(Join-Path $root 'Game')+'"'),'-buildTarget','Win64','-job-worker-count','4','-executeMethod','BasketballRestartBuild.BuildReview','-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
 $p.WaitForExit();if($p.ExitCode -ne 0){throw "Restart player build failed: $log"}
}
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $evidence 'Player/WhatTheFish.exe'}
$run=Join-Path $evidence ('Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$running=@()
try {
 foreach($role in $(if($Local){@('host','guest')}else{@('offline')})){
  $out=Join-Path $run $role;New-Item -ItemType Directory -Force -Path $out | Out-Null
  $arguments=@('-batchmode','-force-d3d11','-screen-width',"$Width",'-screen-height',"$Height",'-screen-fullscreen','0','-job-worker-count','2','-probe','-sport','Basketball','-basketballRestartReview',('"'+$out+'"'),'-report',('"'+(Join-Path $out 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $out 'player.log')+'"'))
  if($role -eq 'offline'){$arguments+='-offline'}else{$arguments+=@($(if($role -eq 'host'){'-localHost'}else{'-localClient'}),'-expected','2','-startAt','5','-returnAt','240','-port','7991')}
  $process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
  $running+=@{process=$process;folder=$out};if($role -eq 'host'){Start-Sleep -Seconds 2}
 }
 foreach($item in $running){if(!$item.process.WaitForExit(180000)){throw "Restart review timeout: $($item.folder)"}}
 foreach($item in $running){
  $result=Get-Content -LiteralPath (Join-Path $item.folder 'results.txt') -Raw
  if($item.process.ExitCode -ne 0 -or $result -match '(?m)^FAIL ' -or !$result.Contains('BASKETBALL_RESTART_COMPLETE success=True')){throw "Restart review failed: $($item.folder)"}
  if(Select-String -LiteralPath (Join-Path $item.folder 'player.log') -Pattern 'Exception:|Assertion failed|Look rotation viewing vector is zero' -Quiet){throw "Runtime error: $($item.folder)"}
  Write-Output "PASS $($item.folder) assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count)"
 }
}finally{foreach($item in $running){if(!$item.process.HasExited){Stop-Process -Id $item.process.Id}}}
Write-Output "BASKETBALL_RESTART_SUITE_PASS $run"
