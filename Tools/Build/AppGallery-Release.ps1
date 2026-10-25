#requires -Version 7.0
param(
    [Parameter(Mandatory)][ValidateSet('Inspect','CreateKey','Sign')][string]$Action,
    [string]$ApkPath,
    [string]$KeystorePath,
    [string]$KeyAlias = 'whatthefish-release',
    [string]$CertificateName,
    [string]$ExpectedCertificateSha256,
    [string]$JavaHomePath,
    [string]$AndroidBuildToolsPath
)

# This helper never uploads, installs, builds or changes Unity signing settings.
# Passwords are prompted locally and passed only in the child process environment.
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$androidPlayer = Join-Path $env:ProgramFiles 'Unity/Hub/Editor/6000.3.20f1/Editor/Data/PlaybackEngines/AndroidPlayer'
if (-not $JavaHomePath) { $JavaHomePath = Join-Path $androidPlayer 'OpenJDK' }
if (-not $AndroidBuildToolsPath) { $AndroidBuildToolsPath = Join-Path $androidPlayer 'SDK/build-tools/36.0.0' }

function Invoke-ReleaseTool {
    param([string]$File, [string[]]$Arguments, [Security.SecureString]$Password)
    if (-not (Test-Path -LiteralPath $File -PathType Leaf)) { throw "Required tool not found: $File. Supply an explicit tool location." }
    $info = [Diagnostics.ProcessStartInfo]::new($File)
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    foreach ($argument in $Arguments) { $info.ArgumentList.Add($argument) }
    if ($Password) {
        $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
        try { $info.Environment['WTF_RELEASE_PASSWORD'] = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) }
        finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer) }
    }
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $info
    try {
        if (-not $process.Start()) { throw 'Could not start the Android release tool.' }
        $info.Environment.Remove('WTF_RELEASE_PASSWORD') | Out-Null
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $output = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0) {
            # Tool output can contain local paths; never record secret-bearing invocation state.
            throw "Release tool failed (exit $($process.ExitCode)). Check paths, certificate alias and password locally."
        }
        return $output
    } finally {
        $info.Environment.Remove('WTF_RELEASE_PASSWORD') | Out-Null
        $process.Dispose()
    }
}

function Assert-ExternalKeyPath {
    param([string]$Value)
    if (-not $Value -or -not [IO.Path]::IsPathFullyQualified($Value)) { throw 'Provide an absolute keystore path outside the repository.' }
    $full = [IO.Path]::GetFullPath($Value)
    if ($full.Equals($repoRoot,[StringComparison]::OrdinalIgnoreCase) -or
        $full.StartsWith($repoRoot + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) {
        throw 'Keep the signing keystore outside the repository, including its ignored folders.'
    }
    # Resolve the existing parent so a junction cannot redirect the key into this checkout.
    $parent = Get-Item -LiteralPath ([IO.Path]::GetDirectoryName($full))
    for ($node = $parent; $null -ne $node; $node = $node.Parent) {
        if (($node.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'Choose a keystore directory without junctions or symbolic links.' }
    }
    if ((Test-Path -LiteralPath $full) -and ((Get-Item -LiteralPath $full).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw 'The keystore must be a regular file, not a symbolic link.'
    }
    return $full
}

$java = Join-Path $JavaHomePath 'bin/java.exe'
$keytool = Join-Path $JavaHomePath 'bin/keytool.exe'
$signer = Join-Path $AndroidBuildToolsPath 'lib/apksigner.jar'
$zipalign = Join-Path $AndroidBuildToolsPath 'zipalign.exe'
$aapt = Join-Path $AndroidBuildToolsPath 'aapt.exe'
if ($KeyAlias -notmatch '^[a-zA-Z0-9][a-zA-Z0-9_-]{0,63}$') { throw 'Use a simple nonempty key alias.' }

if ($Action -eq 'CreateKey') {
    $keyFile = Assert-ExternalKeyPath $KeystorePath
    if (Test-Path -LiteralPath $keyFile) { throw 'Keystore already exists. It will not be overwritten or rotated.' }
    if (-not $CertificateName -or $CertificateName -notmatch '^[a-zA-Z0-9 ._-]{1,100}$') {
        throw 'Provide a CertificateName using letters, numbers, spaces, dots, underscores or hyphens. This public certificate label is not your AppGallery publisher profile.'
    }
    $password = Read-Host 'New keystore password (at least 12 characters; keep a secure backup)' -AsSecureString
    $confirm = Read-Host 'Repeat keystore password' -AsSecureString
    try {
        if ($password.Length -lt 12) { throw 'Use at least 12 characters.' }
        $one = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
        $two = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($confirm)
        try {
            if ([Runtime.InteropServices.Marshal]::PtrToStringBSTR($one) -cne [Runtime.InteropServices.Marshal]::PtrToStringBSTR($two)) { throw 'Passwords do not match.' }
        } finally {
            [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($one)
            [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($two)
        }
        $null = Invoke-ReleaseTool $keytool @('-genkeypair','-storetype','PKCS12','-keystore',$keyFile,'-alias',$KeyAlias,
            '-keyalg','RSA','-keysize','3072','-validity','10000','-dname',"CN=$CertificateName, C=MY",
            '-storepass:env','WTF_RELEASE_PASSWORD','-keypass:env','WTF_RELEASE_PASSWORD') $password
        $details = Invoke-ReleaseTool $keytool @('-list','-v','-keystore',$keyFile,'-alias',$KeyAlias,'-storepass:env','WTF_RELEASE_PASSWORD') $password
        Write-Output $details
        Write-Output 'KEY_CREATED: Back up the keystore and password securely before using this key. Do not upload either to Git or chat.'
    } finally { $password.Dispose(); $confirm.Dispose() }
    exit 0
}

if (-not $ApkPath) { $ApkPath = Join-Path $repoRoot 'Builds/Android/WhatTheFish-release.apk' }
$inputApk = (Resolve-Path -LiteralPath $ApkPath).Path
$inputInfo = Get-Item -LiteralPath $inputApk
if ($inputInfo.Length -ge 1000000000) { throw 'APK exceeds the project release ceiling.' }
$badging = Invoke-ReleaseTool $aapt @('dump','badging',$inputApk)
if ($badging -notmatch "package: name='com\.umpsa\.whatthefish'") { throw 'Unexpected Android package name.' }
$certificates = Invoke-ReleaseTool $java @('-jar',$signer,'verify','--verbose','--print-certs',$inputApk)
$fingerprint = [regex]::Match($certificates,'Signer #1 certificate SHA-256 digest: ([0-9a-fA-F]+)').Groups[1].Value.ToUpperInvariant()
if ($fingerprint.Length -ne 64) { throw 'Cannot determine signing fingerprint.' }
$debuggable = $badging -match '(?m)^application-debuggable'
$debugCertificate = $certificates -match '(?i)CN=Android Debug' -or $fingerprint -eq '6A7BEB4BC1B049BD18E995AF40AC0BCB57AF783B308F9022D9F9B7F750B3BA6C'
if ($Action -eq 'Inspect') {
    [ordered]@{
        bytes=$inputInfo.Length
        sha256=(Get-FileHash -LiteralPath $inputApk -Algorithm SHA256).Hash
        certificateSha256=$fingerprint
        knownDebugCertificate=$debugCertificate
        debuggable=$debuggable
        manifestSummary=($badging -split "`r?`n" | Where-Object { $_ -match '^(package:|sdkVersion:|targetSdkVersion:|native-code:|uses-permission:)' })
        storeReadiness='NOT_CERTIFIED: inspection does not verify source freshness, privacy, device behavior or AppGallery approval.'
    } | ConvertTo-Json -Depth 4
    exit 0
}

if ($debuggable) { throw 'Refusing to sign a debuggable APK for AppGallery.' }
$keyFile = Assert-ExternalKeyPath $KeystorePath
if (-not (Test-Path -LiteralPath $keyFile -PathType Leaf)) { throw 'Production keystore does not exist. Create it privately first.' }
$expected = ($ExpectedCertificateSha256 -replace ':','').Trim().ToUpperInvariant()
if ($expected -notmatch '^[0-9A-F]{64}$') { throw 'Supply the production SHA-256 fingerprint obtained from CreateKey, with or without colons.' }
if ($expected -eq '6A7BEB4BC1B049BD18E995AF40AC0BCB57AF783B308F9022D9F9B7F750B3BA6C') { throw 'The registered development certificate cannot be the production signer.' }
$outputDir = Join-Path $repoRoot ('Builds/AppGallery/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $outputDir | Out-Null
$aligned = Join-Path $outputDir 'aligned-input.apk'
$candidate = Join-Path $outputDir 'unverified-candidate.apk'
$snapshot = Join-Path $outputDir 'source-input.apk'
Copy-Item -LiteralPath $inputApk -Destination $snapshot
$snapshotHash = (Get-FileHash -LiteralPath $snapshot -Algorithm SHA256).Hash
# Inspect the frozen bytes too: a concurrent Unity build may replace the source.
$frozenBadging = Invoke-ReleaseTool $aapt @('dump','badging',$snapshot)
if ($frozenBadging -notmatch "package: name='com\.umpsa\.whatthefish'" -or $frozenBadging -match '(?m)^application-debuggable') { throw 'Frozen input is not the expected non-debuggable game APK.' }
$password = Read-Host 'Production keystore password (entered locally only)' -AsSecureString
try {
    $null = Invoke-ReleaseTool $zipalign @('-P','16','4',$snapshot,$aligned)
    $null = Invoke-ReleaseTool $java @('-jar',$signer,'sign','--ks',$keyFile,'--ks-key-alias',$KeyAlias,
        '--ks-pass','env:WTF_RELEASE_PASSWORD','--key-pass','env:WTF_RELEASE_PASSWORD',
        '--v4-signing-enabled','false','--out',$candidate,$aligned) $password
    $verified = Invoke-ReleaseTool $java @('-jar',$signer,'verify','--verbose','--print-certs',$candidate)
    $actual = [regex]::Match($verified,'Signer #1 certificate SHA-256 digest: ([0-9a-fA-F]+)').Groups[1].Value.ToUpperInvariant()
    if ($actual -ne $expected -or $verified -match '(?i)CN=Android Debug') { throw 'Signer does not match the expected production identity. Candidate is not a verified release.' }
    $null = Invoke-ReleaseTool $zipalign @('-c','-P','16','4',$candidate)
    $size = (Get-Item -LiteralPath $candidate).Length
    if ($size -ge 1000000000) { throw 'Signed APK exceeds the project ceiling.' }
    $final = Join-Path $outputDir 'WhatTheFish-AppGallery.apk'
    Move-Item -LiteralPath $candidate -Destination $final
    [ordered]@{
        apk='WhatTheFish-AppGallery.apk'; bytes=$size
        sha256=(Get-FileHash -LiteralPath $final -Algorithm SHA256).Hash
        inputSha256=$snapshotHash
        certificateSha256=$actual; verifiedUtc=[DateTime]::UtcNow.ToString('o')
        scope='Signature and alignment only; source freshness and store/device acceptance remain separate.'
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $outputDir 'signature-record.json') -Encoding utf8
    Write-Output "SIGNATURE_VERIFIED: $final"
    Write-Output 'Register this delivery certificate in AppGallery, then complete device and release checks before upload.'
} finally { $password.Dispose() }
