param(
 [string]$PlayerPath,
 [ValidateSet('Offline','Local','Cloud','All')][string]$Transport='Offline',
 [ValidateSet('Golf','Fishing','Both')][string]$Sport='Both',
 [ValidateSet('Host','Guest','Both')][string]$ExitRole='Both',
 [ValidateSet('Round','Cup','Crew')][string]$FishingMode='Round'
)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(!$PlayerPath){$PlayerPath=Join-Path $taskRoot 'Builds/WindowsFinal/WhatTheFish.exe'}
$PlayerPath=(Resolve-Path -LiteralPath $PlayerPath).Path
$taskOutput=Join-Path $taskRoot ('Builds/IslandSettingsQA/Run-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
$taskSports=if($Sport -eq 'Both'){@('Golf','Fishing')}else{@($Sport)}
$taskTransports=if($Transport -eq 'All'){@('Offline','Local','Cloud')}else{@($Transport)}
$taskExits=if($ExitRole -eq 'Both'){@('Host','Guest')}else{@($ExitRole)}
$taskProcesses=[Collections.Generic.List[Diagnostics.Process]]::new()
function Start-SettingsPlayer([string]$folder,[string]$role,[string]$mode,[string]$sportName,[string]$exitName,[string]$networkArgs){
 New-Item -ItemType Directory -Path (Join-Path $folder $role) -Force | Out-Null
 $taskPeer=Join-Path $folder $role
 $taskArgs="-batchmode -force-d3d11 -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -job-worker-count 2 -sport $sportName -probe -expected 2 -startAt 8 -returnAt 10000 -exitAfter 180 -islandSettingsAudit -settingsExit $exitName -settingsFishingMode $FishingMode -settingsSync `"$folder`" -settingsReport `"$taskPeer/settings.txt`" -report `"$taskPeer/probe.txt`" -logFile `"$taskPeer/player.log`" $networkArgs"
 $taskProcess=Start-Process -FilePath $PlayerPath -ArgumentList $taskArgs -WorkingDirectory $env:TEMP -WindowStyle Hidden -PassThru
 $taskProcesses.Add($taskProcess)
 return $taskProcess
}
function Wait-Settings([string]$folder,[string]$role,[string]$pattern,[int]$seconds=150){
 $taskDeadline=(Get-Date).AddSeconds($seconds);$taskReport=Join-Path $folder "$role/settings.txt"
 do {
  if(Test-Path -LiteralPath $taskReport){$taskText=Get-Content -LiteralPath $taskReport -Raw;if($taskText -match '(?m)^FAIL '){throw "Settings review failed: $taskReport"};if($taskText -match $pattern){return}}
  Start-Sleep -Milliseconds 400
 }while((Get-Date) -lt $taskDeadline)
 throw "Settings review timed out: $taskReport / $pattern"
}
try {
 foreach($taskMode in $taskTransports){foreach($taskSport in $taskSports){
  $taskScenarios=if($taskMode -eq 'Offline'){@('Offline')}else{$taskExits}
  foreach($taskExit in $taskScenarios){
   $taskFolder=Join-Path $taskOutput "$taskMode-$taskSport-$FishingMode-$taskExit"
   New-Item -ItemType Directory -Path $taskFolder -Force | Out-Null
   $taskActive=@()
   if($taskMode -eq 'Offline'){$taskActive+=Start-SettingsPlayer $taskFolder 'offline' $taskMode $taskSport 'Offline' '-offline';$taskRoles=@('offline')}
   else {
    $taskRoles=@('host','guest');$taskPort=7976
    if($taskMode -eq 'Cloud'){
     $taskCode=Join-Path $taskFolder 'room-code.txt'
     $taskActive+=Start-SettingsPlayer $taskFolder 'host' $taskMode $taskSport $taskExit "-cloudHost -codeFile `"$taskCode`" -authProfile settings$([guid]::NewGuid().ToString('N').Substring(0,16))"
     $taskDeadline=(Get-Date).AddSeconds(150)
     do {if((Test-Path -LiteralPath $taskCode) -and (Get-Item -LiteralPath $taskCode).Length -gt 0){break};Start-Sleep -Milliseconds 400}while((Get-Date) -lt $taskDeadline)
     if(!(Test-Path -LiteralPath $taskCode)){throw "Cloud room creation failed: $taskFolder"}
     $taskJoin="-cloudCode $((Get-Content -LiteralPath $taskCode -Raw).Trim()) -authProfile settings$([guid]::NewGuid().ToString('N').Substring(0,16))"
    }else{
     $taskActive+=Start-SettingsPlayer $taskFolder 'host' $taskMode $taskSport $taskExit "-localHost -port $taskPort"
     $taskHostReport=Join-Path $taskFolder 'host/probe.txt';$taskDeadline=(Get-Date).AddSeconds(80)
     do {if((Test-Path -LiteralPath $taskHostReport) -and (Select-String -LiteralPath $taskHostReport -Pattern 'count=1 connected=True' -Quiet)){break};Start-Sleep -Milliseconds 400}while((Get-Date) -lt $taskDeadline)
     if(!(Test-Path -LiteralPath $taskHostReport) -or !(Select-String -LiteralPath $taskHostReport -Pattern 'count=1 connected=True' -Quiet)){throw "Local host did not open its room: $taskFolder"}
     $taskJoin="-localClient -port $taskPort"
    }
    $taskActive+=Start-SettingsPlayer $taskFolder 'guest' $taskMode $taskSport $taskExit $taskJoin
   }
   foreach($taskRole in $taskRoles){Wait-Settings $taskFolder $taskRole '(?m)^ISLAND_SETTINGS_COMPLETE '}
   foreach($taskRole in $taskRoles){
    $taskPeer=Join-Path $taskFolder $taskRole;$taskText=Get-Content -LiteralPath "$taskPeer/settings.txt" -Raw
    if((Get-Content -LiteralPath "$taskPeer/player.log" -Raw) -match '(?m)(NullReferenceException|MissingReferenceException|IndexOutOfRangeException|InvalidOperationException|Shader error)'){throw "Runtime exception: $taskPeer"}
    "PASS $taskMode $taskSport $taskExit $taskRole assertions=$(([regex]::Matches($taskText,'(?m)^PASS ')).Count)" | Tee-Object -FilePath (Join-Path $taskOutput 'results.txt') -Append
   }
   foreach($taskProcess in $taskActive){if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id -ErrorAction SilentlyContinue}}
  }
 }}
 "ISLAND_SETTINGS_SUITE_PASS $taskOutput"
}finally{foreach($taskProcess in $taskProcesses){if(!$taskProcess.HasExited){Stop-Process -Id $taskProcess.Id -ErrorAction SilentlyContinue}}}
