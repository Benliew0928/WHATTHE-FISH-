param([switch]$Staged)

# Read-only: inspect publishable working files, or the exact staged text blobs.
# Opaque binary/LFS payloads still require format-aware dependency validation.
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$patterns = [ordered]@{
    DrivePath = '(?i)(?<![a-z0-9:])[a-z]:[\\/]'
    HomePath = '(?i)(?<![a-z0-9])/(?:Users|home|root|mnt/[a-z]|Volumes|media)/'
    FileUrl = '(?i)\bfile:(?://|\\\\)'
    NetworkShare = '(?i)(?<![a-z0-9:\\])\\\\[a-z0-9_.-]+\\'
}
$utf8 = [Text.UTF8Encoding]::new($false, $true)
$binaryExtensions = @('.blend', '.fbx', '.png', '.jpg', '.jpeg', '.webp', '.ico', '.zip', '.7z', '.gz', '.mp4', '.wav', '.mp3', '.ogg', '.ttf', '.otf', '.pdf', '.dll', '.exe', '.glb', '.unitypackage')
$problems = [Collections.Generic.List[string]]::new()
$textCount = 0
$opaqueCount = 0

function ConvertTo-PortableText {
    param([byte[]]$Bytes)
    if ($Bytes.Length -ge 2 -and $Bytes[0] -eq 255 -and $Bytes[1] -eq 254) {
        return [Text.UnicodeEncoding]::new($false, $false, $true).GetString($Bytes, 2, $Bytes.Length - 2)
    }
    if ($Bytes.Length -ge 2 -and $Bytes[0] -eq 254 -and $Bytes[1] -eq 255) {
        return [Text.UnicodeEncoding]::new($true, $false, $true).GetString($Bytes, 2, $Bytes.Length - 2)
    }
    for ($offset = 0; $offset -lt [Math]::Min($Bytes.Length, 8192); $offset++) {
        if ($Bytes[$offset] -eq 0) { return $null }
    }
    try { return $utf8.GetString($Bytes) }
    # Legacy text encodings still contain ASCII paths and must be inspected.
    catch [Text.DecoderFallbackException] { return [Text.Encoding]::Latin1.GetString($Bytes) }
}

function Read-IndexHeader {
    param([IO.Stream]$Stream)
    $bytes = [Collections.Generic.List[byte]]::new()
    while ($true) {
        $value = $Stream.ReadByte()
        if ($value -lt 0) { throw 'Git ended before returning a staged blob header.' }
        if ($value -eq 10) { break }
        $bytes.Add([byte]$value)
    }
    return $utf8.GetString($bytes.ToArray())
}

function Read-IndexBytes {
    param([string]$Path, [Diagnostics.Process]$Process)
    $Process.StandardInput.WriteLine(':' + $Path)
    $Process.StandardInput.Flush()
    $header = Read-IndexHeader $Process.StandardOutput.BaseStream
    if ($header -notmatch '^[a-f0-9]+ blob (\d+)$') { throw "Cannot read staged file: $Path" }
    $size = [int]$Matches[1]
    $bytes = [byte[]]::new($size)
    $offset = 0
    while ($offset -lt $size) {
        $read = $Process.StandardOutput.BaseStream.Read($bytes, $offset, $size - $offset)
        if ($read -eq 0) { throw "Incomplete staged blob: $Path" }
        $offset += $read
    }
    if ($Process.StandardOutput.BaseStream.ReadByte() -ne 10) { throw "Invalid staged blob separator: $Path" }
    return ,$bytes
}

$listArguments = @('ls-files', '--cached', '-z')
if (-not $Staged) { $listArguments += @('--others', '--exclude-standard') }
$listed = @(& git -C $repoRoot @listArguments)
if ($LASTEXITCODE -ne 0) { throw 'Cannot list publishable files for the portability check.' }
$paths = @(($listed -join "`n") -split "`0" | Where-Object { $_ } | Sort-Object -Unique)
$reader = $null
try {
    if ($Staged) {
        $start = [Diagnostics.ProcessStartInfo]::new('git')
        foreach ($argument in @('-C', $repoRoot, 'cat-file', '--batch')) { $start.ArgumentList.Add($argument) }
        $start.UseShellExecute = $false
        $start.CreateNoWindow = $true
        $start.RedirectStandardInput = $true
        $start.RedirectStandardOutput = $true
        $start.RedirectStandardError = $true
        $start.StandardInputEncoding = [Text.UTF8Encoding]::new($false)
        $reader = [Diagnostics.Process]::Start($start)
        # Git expects LF-delimited object expressions; do not append Windows CR.
        $reader.StandardInput.NewLine = "`n"
    }
    foreach ($path in $paths) {
        if ([IO.Path]::GetExtension($path).ToLowerInvariant() -in $binaryExtensions) {
            $opaqueCount++
            continue
        }
        if ($Staged) {
            $bytes = Read-IndexBytes $path $reader
        } else {
            $fullPath = Join-Path $repoRoot $path
            if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) { continue }
            # Probe before reading large art files. The index contains LFS pointers.
            $stream = [IO.File]::OpenRead($fullPath)
            try {
                $prefix = [byte[]]::new([Math]::Min($stream.Length, 8192))
                [void]$stream.Read($prefix, 0, $prefix.Length)
            } finally { $stream.Dispose() }
            $unicodeBom = $prefix.Length -ge 2 -and (($prefix[0] -eq 255 -and $prefix[1] -eq 254) -or ($prefix[0] -eq 254 -and $prefix[1] -eq 255))
            # A probe may end halfway through a UTF-8 character. Decode only
            # the complete file; this probe checks binary NUL bytes instead.
            if (-not $unicodeBom -and 0 -in $prefix) { $opaqueCount++; continue }
            $bytes = [IO.File]::ReadAllBytes($fullPath)
        }
        $content = ConvertTo-PortableText $bytes
        if ($null -eq $content -or $content.StartsWith('version https://git-lfs.github.com/spec/v1')) {
            $opaqueCount++
            continue
        }
        $textCount++
        foreach ($kind in $patterns.Keys) {
            foreach ($match in [regex]::Matches($content, $patterns[$kind])) {
                $line = 1 + [regex]::Matches($content.Substring(0, $match.Index), "`n").Count
                # Report locations only; never print entire config lines or secrets.
                $problems.Add("${path}:${line}: $kind")
            }
        }
    }
} finally {
    if ($reader) {
        $reader.StandardInput.Close()
        if (-not $reader.WaitForExit(5000)) { $reader.Kill(); throw 'Staged portability reader did not exit.' }
        $readerError = $reader.StandardError.ReadToEnd()
        $readerExit = $reader.ExitCode
        $reader.Dispose()
        if ($readerExit -ne 0) { throw 'Git could not finish reading the staged text blobs.' }
    }
}
if ($problems.Count) {
    throw ("Machine-specific paths must be replaced with portable references:`n - " + ($problems -join "`n - "))
}
$scope = if ($Staged) { 'staged index' } else { 'publishable working files' }
Write-Output "PORTABLE_PATHS_OK: $scope; $textCount text files; $opaqueCount opaque/LFS files. Validate binary dependencies separately with their asset tools."
