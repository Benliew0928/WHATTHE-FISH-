param([switch]$RequireApk)

# Read-only check of what the user could publish with git add . from the repo root.
# This does not stage files, build the game, or establish APK freshness.
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
& (Join-Path $PSScriptRoot 'Check-PortablePaths.ps1')
$problems = [Collections.Generic.List[string]]::new()
function Invoke-RepoGit {
    param([string[]]$Arguments)
    $result = @(& git -C $repoRoot -c core.safecrlf=false @Arguments)
    if ($LASTEXITCODE -ne 0) { throw "Git check failed: $($Arguments -join ' ')" }
    return $result
}

foreach ($diffArgs in @(@('diff','--check'), @('diff','--cached','--check'))) {
    & git -C $repoRoot -c core.safecrlf=false @diffArgs
    if ($LASTEXITCODE -ne 0) { $problems.Add('Git whitespace/conflict-marker check failed.') }
}
if (@(Invoke-RepoGit @('ls-files','--unmerged')).Count) { $problems.Add('Unresolved merge entries exist.') }
$paths = @(Invoke-RepoGit @('-c','core.quotepath=false','ls-files','--cached','--others','--exclude-standard') | Sort-Object -Unique)
$files = @($paths | Where-Object { Test-Path -LiteralPath (Join-Path $repoRoot $_) -PathType Leaf })
$fileSet = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($path in $files) { [void]$fileSet.Add($path) }

foreach ($path in @(Invoke-RepoGit @('ls-files','--cached','--ignored','--exclude-standard'))) {
    if ($fileSet.Contains($path)) { $problems.Add("Already tracked despite ignore rule: $path") }
}
$artifactPattern = '(^|/)(Builds|Legacy|__pycache__)/|^Tools/Downloads/|^Game/(Library|Temp|obj|Logs|UserSettings)/|\.(apk|aab|log|tmp|bak|pyc|pyo|keystore|jks)$|\.blend[0-9]+$|(^|/)\.env($|\.)'
foreach ($path in $files) {
    if ($path -match $artifactPattern) { $problems.Add("Local artifact or credential file would be published: $path") }
    if ($path.StartsWith('Game/Assets/')) {
        if ($path.EndsWith('.meta')) {
            $assetPath = $path.Substring(0, $path.Length - 5)
            if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $assetPath))) { $problems.Add("Orphan Unity metadata: $path") }
        } elseif (-not $fileSet.Contains($path + '.meta')) {
            $problems.Add("Missing publishable Unity metadata: $path.meta")
        }
    }
}

# Ask Git which files use LFS; preserve its attribute rules rather than guessing.
$attributes = @(
    # Windows pipelines append CRLF; --stdin would treat CR as part of a filename.
    # Bounded argument batches also stay below the Windows command-line limit.
    for ($offset = 0; $offset -lt $files.Count; $offset += 64) {
        $last = [math]::Min($offset + 63, $files.Count - 1)
        & git -C $repoRoot -c core.quotepath=false check-attr filter -- @($files[$offset..$last])
        if ($LASTEXITCODE -ne 0) { throw 'Cannot read Git attributes.' }
    }
)
$lfsFiles = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($line in $attributes) {
    if ($line -match '^(.*): filter: lfs$') { [void]$lfsFiles.Add($Matches[1]) }
}
if ($lfsFiles.Count) {
    & git -C $repoRoot lfs version
    if ($LASTEXITCODE -ne 0) { $problems.Add('Git LFS is required for the project binary assets.') }
}
foreach ($path in $files) {
    $item = Get-Item -LiteralPath (Join-Path $repoRoot $path) -Force
    if ($item.Length -ge 50000000 -and -not $lfsFiles.Contains($path)) {
        $problems.Add("Large file without LFS ($($item.Length) bytes): $path")
    }
}

$auditSource = Get-Content -LiteralPath (Join-Path $repoRoot 'Game/Assets/_Game/Editor/BuildSizeAudit.cs') -Raw
if ($auditSource -notmatch 'LimitBytes\s*=\s*(\d+)') { throw 'Cannot read the APK limit from BuildSizeAudit.cs.' }
$limitBytes = [long]$Matches[1]
if ($limitBytes -ne 100000000) { $problems.Add('The hard APK limit must remain 100,000,000 bytes.') }
if ($auditSource -notmatch 'TargetBytes\s*=\s*(\d+)') { throw 'Cannot read the working target from BuildSizeAudit.cs.' }
$targetBytes = [long]$Matches[1]
$apkPath = Join-Path $repoRoot 'Builds/Android/WhatTheFish-release.apk'
if (Test-Path -LiteralPath $apkPath -PathType Leaf) {
    $apk = Get-Item -LiteralPath $apkPath
    Write-Output ('Existing APK: {0} bytes ({1:F2} MB); headroom: {2:F2} MB; modified UTC: {3:o}' -f $apk.Length, ($apk.Length / 1e6), (($limitBytes - $apk.Length) / 1e6), $apk.LastWriteTimeUtc)
    if ($apk.Length -ge $limitBytes) { $problems.Add('APK violates the strict under-100-MB limit.') }
    elseif ($apk.Length -gt $targetBytes) { Write-Warning ('APK is above the {0:F2} MB working target. Preserve room for gameplay.' -f ($targetBytes / 1e6)) }
    Write-Output 'APK freshness, signature, gameplay and phone performance require separate build/test evidence.'
} elseif ($RequireApk) {
    $problems.Add('No release APK exists. Build AndroidSubmission before handing off an Android release.')
} else {
    Write-Output 'No local APK: package size is unverified (acceptable for documentation-only work or a fresh clone).'
}

# Dry-run also checks Git can enumerate the additions/removals without changing its index.
$dryRun = @(Invoke-RepoGit @('add','--dry-run','--all','--','.'))
Write-Output "Inspected $($files.Count) publishable files; $($lfsFiles.Count) LFS paths; $($dryRun.Count) pending add/remove operations."
if ($problems.Count) { throw ("Task readiness failed:`n - " + ($problems -join "`n - ")) }
Write-Output 'TASK_READY: repository hygiene checks passed; staging, commit and push remain with the user.'
