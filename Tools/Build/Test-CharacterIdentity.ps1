param([string]$PlayerPath,[ValidateSet('All','Offline','Network')][string]$Mode='All',[ValidateSet('All','Football','Basketball')][string]$Sport='All',[ValidateRange(640,3840)][int]$Width=1280,[ValidateRange(360,2160)][int]$Height=720)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $root 'Builds/CharacterIdentityQA/Player/WhatTheFish.exe'}
$run=Join-Path $root ('Builds/CharacterIdentityQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
$sports=if($Sport -eq 'All'){@('Football','Basketball')}else{@($Sport)}
foreach($selected in $sports){
 $groups=@();if($Mode -ne 'Network'){$groups+=,@('offline')};if($Mode -ne 'Offline'){$groups+=,@('host','guest')}
 foreach($modes in $groups){
  $running=@()
  try{
   foreach($side in $modes){
    $folder=Join-Path $run "$selected-$side";New-Item -ItemType Directory -Force -Path $folder | Out-Null
    $arguments=@('-batchmode','-force-d3d11','-screen-width',$Width,'-screen-height',$Height,'-screen-fullscreen','0','-job-worker-count','2','-probe','-sport',$selected,'-characterIdentityReview',('"'+$folder+'"'),'-report',('"'+(Join-Path $folder 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'))
    if($side -eq 'offline'){$arguments+='-offline'}else{$arguments+=@($(if($side -eq 'host'){'-localHost'}else{'-localClient'}),'-expected','2','-startAt','5','-returnAt','300','-port','7993')}
    $process=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru
    $null=$process.Handle;$running+=@{process=$process;folder=$folder};if($side -eq 'host'){Start-Sleep -Seconds 2}
   }
   foreach($item in $running){if(!$item.process.WaitForExit(150000)){throw "Identity review timed out: $($item.folder)"}}
   foreach($item in $running){
    $result=Get-Content -LiteralPath (Join-Path $item.folder 'results.txt') -Raw
    if(($null -ne $item.process.ExitCode -and $item.process.ExitCode -ne 0) -or $result -match '(?m)^FAIL ' -or !$result.Contains('CHARACTER_IDENTITY_REVIEW_COMPLETE success=True')){throw "Identity checks failed: $($item.folder)"}
    if(Select-String -LiteralPath (Join-Path $item.folder 'player.log') -Pattern 'Exception:|Assertion failed|Shader error' -Quiet){throw "Runtime diagnostic: $($item.folder)"}
    Write-Output "PASS $($item.folder) assertions=$(([regex]::Matches($result,'(?m)^PASS ')).Count)"
   }
   if($modes.Count -eq 2){$hostRoster=Get-Content (Join-Path $running[0].folder 'roster.txt');$guestRoster=Get-Content (Join-Path $running[1].folder 'roster.txt');if(Compare-Object $hostRoster $guestRoster){throw 'Host and guest identity/team assignments disagree'};Write-Output "PASS $selected host and guest agree on all ten identities and uniforms"}
  }finally{foreach($item in $running){if(!$item.process.HasExited){Stop-Process -Id $item.process.Id}}}
 }
}
Write-Output "CHARACTER_IDENTITY_SUITE_PASS $run"
