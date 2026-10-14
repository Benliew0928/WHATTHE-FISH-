param([string]$PlayerPath,[switch]$Local,[int]$Port=7998,[int]$Width=1280,[int]$Height=720)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $root 'Builds/BasketballControlQA/Player/WhatTheFish.exe'}
$out=Join-Path $root ('Builds/BasketballControlQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
$running=@()
try {
 foreach($role in $(if($Local){@('host','guest')}else{@('offline')})){
  $folder=Join-Path $out $role;New-Item -ItemType Directory -Force -Path $folder | Out-Null
  $arguments=@('-batchmode','-force-d3d11','-screen-width',"$Width",'-screen-height',"$Height",'-screen-fullscreen','0','-job-worker-count','2','-probe','-sport','Basketball','-basketballControlsReview',('"'+$folder+'"'),'-report',('"'+(Join-Path $folder 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'),'-exitAfter','190')
  if($role -eq 'offline'){$arguments+='-offline'}else{$arguments+=@($(if($role -eq 'host'){'-localHost'}else{'-localClient'}),'-port',"$Port",'-expected','2','-startAt','5','-returnAt','185')}
  $p=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru;$running+=@{process=$p;folder=$folder}
  if($role -eq 'host'){Start-Sleep -Seconds 2}
 }
 foreach($item in $running){if(!$item.process.WaitForExit(200000)){throw "Review timed out: $($item.folder)"}}
 foreach($item in $running){
  $result=Get-Content -LiteralPath (Join-Path $item.folder 'results.txt') -Raw
  if($result -match '(?m)^FAIL ' -or !$result.Contains('BASKETBALL_CONTROLS_COMPLETE success=True')){throw "Review failed: $($item.folder)"}
  if(Select-String -LiteralPath (Join-Path $item.folder 'player.log') -Pattern 'Exception:|Assertion failed' -Quiet){throw "Runtime diagnostic: $($item.folder)"}
  Write-Output "PASS $($item.folder) assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count)"
 }
}finally{foreach($item in $running){if(!$item.process.HasExited){Stop-Process -Id $item.process.Id}}}
