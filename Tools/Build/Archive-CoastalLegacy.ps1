param([string]$Manifest='C:\UMPSA\Builds\CoastalStadiumAudit\archive-candidates.json')
$ErrorActionPreference='Stop'
$archiveWorkspace=[IO.Path]::GetFullPath((Split-Path (Split-Path $PSScriptRoot -Parent) -Parent))
$batch=Get-Content -LiteralPath $Manifest -Raw | ConvertFrom-Json
$qa=Get-Content -LiteralPath $batch.validation -Raw
if($qa -match '(?m)^FAIL ' -or $qa -notmatch 'STADIUM_REVIEW_COMPLETE'){throw 'Archive requires completed passing EXE validation'}
if((Get-FileHash -LiteralPath $batch.buildFile -Algorithm SHA256).Hash -ne $batch.buildHash){throw 'Build changed since dependency inventory'}
$archiveBatch=[IO.Path]::GetFullPath((Join-Path $archiveWorkspace ('Legacy\'+$batch.batch)))
if(!$archiveBatch.StartsWith($archiveWorkspace+'\Legacy\',[StringComparison]::OrdinalIgnoreCase)){throw 'Invalid archive batch path'}
if(Test-Path -LiteralPath $archiveBatch){throw 'Archive batch already exists; previous history will not be overwritten'}
$verified=@()
foreach($candidate in $batch.files){
 $source=[IO.Path]::GetFullPath((Join-Path $archiveWorkspace $candidate.original))
 $destination=[IO.Path]::GetFullPath((Join-Path $archiveBatch $candidate.original))
 if(!$source.StartsWith($archiveWorkspace+'\',[StringComparison]::OrdinalIgnoreCase) -or $source.StartsWith($archiveWorkspace+'\Legacy\',[StringComparison]::OrdinalIgnoreCase) -or !$destination.StartsWith($archiveBatch+'\',[StringComparison]::OrdinalIgnoreCase)){throw "Path escaped archive workspace: $source"}
 if(!(Test-Path -LiteralPath $source -PathType Leaf)){throw "Missing candidate: $source"}
 if((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $candidate.sha256){throw "Candidate changed: $source"}
 $verified+=@{source=$source;destination=$destination;record=$candidate}
}
New-Item -ItemType Directory -Path $archiveBatch | Out-Null
# Save the complete recovery map before moving the first file.
$batch | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $archiveBatch 'archive-manifest.json')
$completed=[Collections.Generic.List[object]]::new()
foreach($entry in $verified){
 New-Item -ItemType Directory -Path (Split-Path $entry.destination -Parent) -Force | Out-Null
 Move-Item -LiteralPath $entry.source -Destination $entry.destination
 $completed.Add($entry.record)
 $entry.record | ConvertTo-Json -Depth 6 -Compress | Add-Content -LiteralPath (Join-Path $archiveBatch 'completed-moves.jsonl')
}
$completed | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $archiveBatch 'completed-moves.json')
@'
# Restore this archive

Each entry in archive-manifest.json records its original workspace-relative path,
SHA-256, size and reason. Files retain their original relative path in this batch.
To restore, close Unity, verify the archived SHA-256, then copy the file to its
original path. Do not overwrite newer files without comparing them. Unity assets
must be restored together with their .meta files to preserve GUIDs. Existing
archive batches and Legacy/archive-manifest.json were not modified.
'@ | Set-Content -LiteralPath (Join-Path $archiveBatch 'README.md')
$bytes=($completed | Measure-Object -Property bytes -Sum).Sum
Write-Output "Archived $($completed.Count) files ($([Math]::Round($bytes/1MB,2)) MB) to $archiveBatch"
