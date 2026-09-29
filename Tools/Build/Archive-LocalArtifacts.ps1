param(
    [Parameter(Mandatory)][string[]]$RelativePath,
    [Parameter(Mandatory)][string]$Reason,
    [ValidatePattern('^[a-zA-Z0-9-]+$')][string]$Label = 'task-cleanup'
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
    $sources.Add([pscustomobject]@{source=$source;destination=$destination;original=$normalized;directory=$item.PSIsContainer})
}
$records = [Collections.Generic.List[object]]::new()
foreach ($source in $sources) {
    $entries = if ($source.directory) { @(Get-ChildItem -LiteralPath $source.source -Recurse -Force) } else { @(Get-Item -LiteralPath $source.source -Force) }
    foreach ($entry in $entries) {
        Assert-NoLink $entry.FullName
        $relative = $entry.FullName.Substring($repoRoot.Length + 1).Replace('\','/')
        if ($relative -match '(^|/)(\.git|Legacy|Library|Temp|obj|Logs|UserSettings)(/|$)') { throw "Protected nested path: $relative" }
        if ($entry.PSIsContainer) { continue }
        $records.Add([pscustomobject]@{original=$relative;bytes=$entry.Length;sha256=(Get-FileHash -LiteralPath $entry.FullName -Algorithm SHA256).Hash})
    }
}
# Save the complete recovery inventory before the first move. Existing archives are untouched.
New-Item -ItemType Directory -Path $batchRoot -Force | Out-Null
@{createdUtc=[DateTime]::UtcNow.ToString('o');reason=$Reason;workspace=$repoRoot;files=@($records.ToArray())} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $batchRoot 'archive-manifest.json')
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
