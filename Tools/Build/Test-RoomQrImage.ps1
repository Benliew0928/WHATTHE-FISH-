param(
    [string]$UnityEditorPath=(Join-Path $env:ProgramFiles 'Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe'),
    [string]$ScanKitAar
)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskOutput=Join-Path $taskRoot 'Builds/ScanImageQA/Native'
New-Item -ItemType Directory -Force $taskOutput | Out-Null
$androidRoot=Join-Path (Split-Path $UnityEditorPath) 'Data/PlaybackEngines/AndroidPlayer'
$taskJdk=Join-Path $androidRoot 'OpenJDK/bin'
$androidJar=(Get-ChildItem (Join-Path $androidRoot 'SDK/platforms') -Recurse -Filter android.jar | Sort-Object FullName -Descending | Select-Object -First 1).FullName
if(!$ScanKitAar){
    $gradleRoot=if($env:GRADLE_USER_HOME){$env:GRADLE_USER_HOME}else{Join-Path ([Environment]::GetFolderPath('UserProfile')) '.gradle'}
    $ScanKitAar=(Get-ChildItem (Join-Path $gradleRoot 'caches/modules-2/files-2.1/com.huawei.hms/scanplus/2.15.0.301') -Filter '*.aar' -Recurse | Select-Object -First 1).FullName
}
if(!$androidJar -or !$ScanKitAar){throw 'Build Android once to resolve the SDK, or supply -ScanKitAar.'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive=[IO.Compression.ZipFile]::OpenRead($ScanKitAar)
try {[IO.Compression.ZipFileExtensions]::ExtractToFile($archive.GetEntry('classes.jar'),(Join-Path $taskOutput 'scanplus.jar'),$true)}finally{$archive.Dispose()}
$classes=Join-Path $taskOutput 'classes'
New-Item -ItemType Directory -Force $classes | Out-Null
$classpath=$androidJar+';'+(Join-Path $taskOutput 'scanplus.jar')
$sources=@((Get-ChildItem (Join-Path $taskRoot 'Tools/Android/RoomScan') -Filter '*.java').FullName)+@((Get-ChildItem (Join-Path $taskRoot 'Tools/Android/RoomScanTests') -Filter '*.java').FullName)
& (Join-Path $taskJdk 'javac.exe') -cp $classpath -d $classes $sources
if($LASTEXITCODE -ne 0){throw 'Native Scan Kit API compilation failed.'}
& (Join-Path $taskJdk 'java.exe') -cp ($classes+';'+$classpath) com.umpsa.whatthefish.scan.RoomQrImageDecoderTest | Tee-Object -FilePath (Join-Path $taskOutput 'results.txt')
if($LASTEXITCODE -ne 0){throw 'Room QR image policy checks failed.'}
