param(
    [Parameter(Mandatory)][string[]]$RelativePath,
    [Parameter(Mandatory)][string]$Reason,
    [ValidatePattern('^[a-zA-Z0-9-]+$')][string]$Label = 'task-cleanup',
    # Explicit opt-in for completed, unregistered project copies under Builds.
    # This never permits archiving the active project or a Git worktree.
    [string[]]$CompletedSnapshotRoot = @()
)

# Only archive an explicit, reviewed list. This script never decides an asset is unused.
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$legacyRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot 'Legacy'))
$batchRoot = [IO.Path]::GetFullPath((Join-Path $legacyRoot ((Get-Date -Format 'yyyyMMdd-HHmmss-fff') + '-' + $Label)))
if (-not $batchRoot.StartsWith($repoRoot + '\Legacy\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid archive destination.' }
if (Test-Path -LiteralPath $batchRoot) { throw 'Archive batch already exists.' }

function Assert-NoLink {
    param([string]$Path)
    $cursor = $Path
    while ($cursor -and $cursor -ne $repoRoot) {
        if (Test-Path -LiteralPath $cursor) {
            if ((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Links/junctions cannot be archived: $cursor" }
        }
        $cursor = Split-Path $cursor -Parent
    }
}
Assert-NoLink $legacyRoot
$snapshotRoots = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($relative in $CompletedSnapshotRoot) {
    if ([IO.Path]::IsPathRooted($relative)) { throw 'Snapshot roots must be repository-relative.' }
    $snapshot = [IO.Path]::GetFullPath((Join-Path $repoRoot $relative))
    if (-not $snapshot.StartsWith($repoRoot + '\Builds\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Completed snapshots must be under Builds.' }
    foreach ($required in @('Game/Assets', 'Game/ProjectSettings', 'Game/Packages/manifest.json')) {
        if (-not (Test-Path -LiteralPath (Join-Path $snapshot $required))) { throw "Incomplete snapshot: $relative" }
    }
    Assert-NoLink $snapshot
    [void]$snapshotRoots.Add($snapshot)
}
if ($snapshotRoots.Count) {
    foreach ($process in @(Get-CimInstance Win32_Process)) {
        if ($process.Name -notmatch '^(Unity|UnityShaderCompiler|WhatTheFish|blender|ffmpeg)\.exe$' -or -not $process.CommandLine) { continue }
        foreach ($snapshot in $snapshotRoots) {
            if ($process.CommandLine.Replace('/', '\').IndexOf($snapshot, [StringComparison]::OrdinalIgnoreCase) -ge 0) { throw 'A process is using a selected snapshot.' }
        }
    }
}
$sources = [Collections.Generic.List[object]]::new()
foreach ($relative in ($RelativePath | Sort-Object -Unique)) {
    if ([IO.Path]::IsPathRooted($relative)) { throw "Use a workspace-relative path: $relative" }
    $source = [IO.Path]::GetFullPath((Join-Path $repoRoot $relative))
    if (-not $source.StartsWith($repoRoot + '\', [StringComparison]::OrdinalIgnoreCase)) { throw "Source escapes workspace: $relative" }
    $normalized = $source.Substring($repoRoot.Length + 1).Replace('\','/')
    if ($normalized -match '(^|/)(\.git|Legacy|Library|Temp|obj|Logs|UserSettings)(/|$)' -or $normalized -notmatch '^(Builds|Tools|Docs|ArtSource|Game/Assets)/') { throw "Protected or invalid source: $relative" }
    Assert-NoLink $source
    $item = Get-Item -LiteralPath $source -Force
    foreach ($entry in $sources) {
        if ($source.StartsWith($entry.source + '\', [StringComparison]::OrdinalIgnoreCase) -or $entry.source.StartsWith($source + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Do not supply overlapping archive paths.' }
    }
    $destination = [IO.Path]::GetFullPath((Join-Path $batchRoot $normalized))
    if (-not $destination.StartsWith($batchRoot + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Destination escapes archive batch.' }
    $sources.Add([pscustomobject]@{source=$source;destination=$destination;original=$normalized;directory=$item.PSIsContainer;snapshot=$snapshotRoots.Contains($source)})
}
foreach ($snapshot in $snapshotRoots) {
    if (-not @($sources | Where-Object { $_.source -eq $snapshot }).Count) { throw 'Each completed snapshot must also be an explicit archive source.' }
}
$records = [Collections.Generic.List[object]]::new()
$sha256 = [Security.Cryptography.SHA256]::Create()
foreach ($source in $sources) {
    Write-Output "Inventory and SHA-256: $($source.original)"
    $pending = [Collections.Generic.Stack[IO.FileSystemInfo]]::new()
    $pending.Push((Get-Item -LiteralPath $source.source -Force))
    while ($pending.Count) {
        $entry = $pending.Pop()
        # Check every entry once, without repeatedly walking its ancestors.
        if ($entry.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Links/junctions cannot be archived: $($entry.FullName)" }
        $relative = $entry.FullName.Substring($repoRoot.Length + 1).Replace('\','/')
        if ($relative -match '(^|/)\.git(/|$)' -or (-not $source.snapshot -and $relative -match '(^|/)(Legacy|Library|Temp|obj|Logs|UserSettings)(/|$)')) { throw "Protected nested path: $relative" }
        if ($entry -is [IO.DirectoryInfo]) {
            foreach ($child in $entry.EnumerateFileSystemInfos()) { $pending.Push($child) }
            continue
        }
        $stream = [IO.File]::OpenRead($entry.FullName)
        # Directory entries for hard links can retain stale lengths. Read the
        # authoritative length from the same open handle used for the hash.
        try { $length = $stream.Length; $hash = [BitConverter]::ToString($sha256.ComputeHash($stream)).Replace('-', '') }
        finally { $stream.Dispose() }
        $records.Add([pscustomobject]@{original=$relative;bytes=$length;sha256=$hash})
    }
}
$sha256.Dispose()
# Save the complete recovery inventory before the first move. Existing archives are untouched.
New-Item -ItemType Directory -Path $batchRoot -Force | Out-Null
@{createdUtc=[DateTime]::UtcNow.ToString('o');reason=$Reason;pathBase='repository root';completedSnapshotRoots=@($CompletedSnapshotRoot);files=@($records.ToArray())} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $batchRoot 'archive-manifest.json')
foreach ($source in $sources) {
    New-Item -ItemType Directory -Path (Split-Path $source.destination -Parent) -Force | Out-Null
    Move-Item -LiteralPath $source.source -Destination $source.destination
    $source.original | Add-Content -LiteralPath (Join-Path $batchRoot 'completed-moves.txt')
}
@'
# Local recovery archive

archive-manifest.json records original workspace-relative paths, sizes and SHA-256.
To restore a file, verify its hash and copy it to its original path in the workspace.
Compare any newer destination before replacing it. Restore Unity assets with their
.meta files. completed-moves.txt records completed top-level moves. No history was
rewritten. This ignored archive is local only; it is not a remote backup.
'@ | Set-Content -LiteralPath (Join-Path $batchRoot 'README.md')
$bytes = ($records | Measure-Object -Property bytes -Sum).Sum
Write-Output "Archived $($records.Count) files ($([math]::Round($bytes/1e6,2)) MB) to $batchRoot"
