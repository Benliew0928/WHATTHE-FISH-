param([ValidateSet('Offline','Local')][string]$Mode='Offline')
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$player=Join-Path $root 'Builds/WindowsFinal/WhatTheFish.exe'
$out=Join-Path $root ('Builds/BasketballModel/20261001/'+$Mode+'-'+(Get-Date -Format 'HHmmss'))
New-Item -ItemType Directory -Path $out -Force | Out-Null
$processes=@();$folders=@()
try {
 foreach($role in $(if($Mode -eq 'Local'){@('host','guest')}else{@('offline')})){
  $folder=Join-Path $out $role;New-Item -ItemType Directory -Path $folder | Out-Null;$folders+=$folder
  $argsList=@('-batchmode','-job-worker-count','2','-probe','-sport',$(if($role -eq 'guest'){'Football'}else{'Basketball'}),'-basketballBallReview',('"'+$folder+'"'),'-report',('"'+(Join-Path $folder 'probe.txt')+'"'),'-logFile',('"'+(Join-Path $folder 'player.log')+'"'),'-exitAfter','110')
  if($role -eq 'offline'){$argsList+='-offline'}else{$argsList+=@($(if($role -eq 'host'){'-localHost'}else{'-localClient'}),'-port','7896','-expected','2','-startAt','12','-returnAt','100')}
  $processes+=Start-Process -FilePath $player -ArgumentList $argsList -WindowStyle Hidden -PassThru
  if($role -eq 'host'){Start-Sleep -Seconds 4}
 }
 $deadline=(Get-Date).AddSeconds(100)
 do {
  $done=@($folders | Where-Object { $result=Join-Path $_ 'results.txt'; (Test-Path -LiteralPath $result) -and (Select-String -LiteralPath $result -Pattern 'BASKETBALL_REVIEW_COMPLETE' -Quiet) })
  if($done.Count -eq $folders.Count){break}
  Start-Sleep -Milliseconds 500
 }while((Get-Date) -lt $deadline)
 if($done.Count -ne $folders.Count){throw 'Basketball review timed out: '+$out}
 foreach($folder in $folders){
  $result=Join-Path $folder 'results.txt';Get-Content -LiteralPath $result
  if(Select-String -LiteralPath $result -Pattern '^FAIL|success=False' -Quiet){throw 'Basketball check failed: '+$result}
  if(Select-String -LiteralPath (Join-Path $folder 'player.log') -Pattern 'NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException' -Quiet){throw 'Player exception: '+$folder}
 }
 Write-Output ('BASKETBALL_SUITE_PASS '+$out)
} finally {
 foreach($process in $processes){if(!$process.HasExited){Stop-Process -Id $process.Id}}
 Write-Output ('Evidence: '+$out)
}
