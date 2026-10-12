param([ValidateSet('Explore','Offline','Local')][string]$Mode='Offline',[int]$Port=7953,[string]$PlayerPath,[switch]$NoCapture)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=if($PlayerPath){(Resolve-Path -LiteralPath $PlayerPath).Path}else{Join-Path $root 'Builds/FootballPropsQA/Player/WhatTheFish.exe'}
$out=Join-Path $root ('Builds/FootballPropsQA/'+$Mode+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$processes=@();$folders=@()
try {
 foreach($role in $(if($Mode -eq 'Local'){@('host','guest','teammate','rival')}else{@('offline')})){
  $folder=Join-Path $out $role;New-Item -ItemType Directory -Path $folder | Out-Null;$folders+=$folder
  $arguments=@('-batchmode','-force-d3d11','-screen-width','1600','-screen-height','900','-screen-fullscreen','0','-job-worker-count','2','-probe','-sport','Football','-footballPropReview',('"'+$folder+'"'),'-report',('"'+(Join-Path $folder 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'),'-exitAfter','280')
  if($NoCapture){$arguments+='-nographics'}
  if($Mode -eq 'Explore'){$arguments+='-explorePropsOnly'}
  if($role -eq 'offline'){$arguments+='-offline'}else{$arguments+=@($(if($role -eq 'host'){'-localHost'}else{'-localClient'}),'-port',$Port,'-expected','4','-startAt','12','-returnAt','270')}
  $p=Start-Process -FilePath $player -ArgumentList $arguments -WindowStyle Hidden -PassThru;$null=$p.Handle;$processes+=$p
  if($role -ne 'offline'){Start-Sleep -Seconds 2}
 }
 $deadline=(Get-Date).AddSeconds(260)
 do {$done=@($folders | Where-Object {$result=Join-Path $_ 'results.txt';(Test-Path -LiteralPath $result) -and (Select-String -LiteralPath $result -Pattern 'FOOTBALL_PROPS_COMPLETE' -Quiet)});if($done.Count -eq $folders.Count){break};Start-Sleep -Milliseconds 500}while((Get-Date) -lt $deadline)
 if($done.Count -ne $folders.Count){throw "Football props timed out: $out"}
 foreach($folder in $folders){
  $result=Join-Path $folder 'results.txt';Get-Content -LiteralPath $result | Select-String '^FAIL|COMPLETE';
  if(Select-String -LiteralPath $result -Pattern '^FAIL|success=False' -Quiet){throw "Football props failed: $result"}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'Exception:|Look rotation viewing vector is zero|Setting linear velocity of a kinematic body' -Quiet){throw "Runtime diagnostic: $folder"}
  Write-Output ("PASS "+(Split-Path $folder -Leaf)+" assertions="+(@(Select-String -LiteralPath $result -Pattern '^PASS').Count))
 }
 Write-Output "FOOTBALL_PROPS_SUITE_PASS $out"
}finally{foreach($process in $processes){$process.Refresh();if(!$process.HasExited){Stop-Process -Id $process.Id}};Write-Output "Evidence: $out"}
