param([ValidateSet('Windows','Android','AndroidRelease','Scene')][string]$Target='Windows')
$ErrorActionPreference='Stop'
$editor='C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe'
$method=switch($Target){'Windows'{'ProjectBuilder.BuildWindows'}'Android'{'ProjectBuilder.BuildAndroid'}'AndroidRelease'{'ProjectBuilder.BuildAndroidRelease'}'Scene'{'ProjectBuilder.Setup'}}
$argsList=@('-batchmode','-nographics','-quit','-projectPath','C:\UMPSA\Game','-executeMethod',$method,'-logFile',"C:\UMPSA\Builds\build-$Target.log")
if($Target -in @('Android','AndroidRelease')){$argsList+=@('-buildTarget','Android')}
if($Target -eq 'Windows'){$argsList+=@('-buildTarget','Win64')}
$process=Start-Process -FilePath $editor -ArgumentList $argsList -PassThru -WindowStyle Hidden
$process.WaitForExit()
if($process.ExitCode -ne 0){throw "Unity failed ($($process.ExitCode)). Read C:\UMPSA\Builds\build-$Target.log"}
