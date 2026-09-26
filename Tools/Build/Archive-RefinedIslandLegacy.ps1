param([string]$Manifest='C:\UMPSA\Builds\IslandRefinementAudit\archive-candidates.json',[switch]$UseCompletedRouteChecks)
$ErrorActionPreference='Stop'
$archiveWorkspace=[IO.Path]::GetFullPath((Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path)
$batch=Get-Content -LiteralPath $Manifest -Raw | ConvertFrom-Json
$qa=Get-Content -LiteralPath $batch.validation -Raw
if($qa -match '(?m)^FAIL '){throw 'Archive requires passing island checks'}
if($qa -notmatch 'ISLAND_REVIEW_COMPLETE success=True'){
 if(!$UseCompletedRouteChecks -or !$batch.validationScope){throw 'Archive requires completed passing island EXE review'}
 # The user explicitly stopped repeated recordings/checking. Require every
 # authored route in both directions before accepting the existing evidence.
 foreach($sport in @('Golf','Fishing')){
  $layout=Get-Content -LiteralPath (Join-Path $archiveWorkspace "Game\Assets\_Game\Art\RefinedIslands\$sport\layout.json") -Raw | ConvertFrom-Json
  foreach($route in $layout.routes){foreach($direction in @('outbound','return')){if($qa -notmatch ('(?m)^PASS '+[regex]::Escape("$sport $($route.name) $direction"))){throw "Missing completed route check: $sport $($route.name) $direction"}}}
 }
}
if((Get-Content -LiteralPath $batch.dependencyAudit -Raw) -notmatch 'REFINED_DEPENDENCY_AUDIT_COMPLETE'){throw 'Missing dependency audit'}
foreach($build in $batch.buildFiles){if((Get-FileHash -LiteralPath $build.path -Algorithm SHA256).Hash -ne $build.sha256){throw "Build changed since inventory: $($build.path)"}}
$archiveBatch=[IO.Path]::GetFullPath((Join-Path $archiveWorkspace ('Legacy\'+$batch.batch)))
if(!$archiveBatch.StartsWith($archiveWorkspace+'\Legacy\',[StringComparison]::OrdinalIgnoreCase)){throw 'Invalid archive batch path'}
if(Test-Path -LiteralPath $archiveBatch){throw 'Archive batch already exists'}
$verified=@()
foreach($candidate in $batch.files){
 $source=[IO.Path]::GetFullPath((Join-Path $archiveWorkspace $candidate.original))
 $destination=[IO.Path]::GetFullPath((Join-Path $archiveBatch $candidate.original))
 if(!$source.StartsWith($archiveWorkspace+'\',[StringComparison]::OrdinalIgnoreCase) -or $source.StartsWith($archiveWorkspace+'\Legacy\',[StringComparison]::OrdinalIgnoreCase) -or !$destination.StartsWith($archiveBatch+'\',[StringComparison]::OrdinalIgnoreCase)){throw "Path escaped workspace: $source"}
 if($candidate.original -match '(^|[\\/])(\.git|Library|Temp|obj|Logs|UserSettings)([\\/]|$)'){throw "Excluded cache or Git path: $source"}
 if(!(Test-Path -LiteralPath $source -PathType Leaf)){throw "Missing candidate: $source"}
 if((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $candidate.sha256){throw "Candidate changed: $source"}
 $verified+=@{source=$source;destination=$destination;record=$candidate}
}
# A complete recovery map is durable before the first move. Every source and
# destination is resolved and contained in the workspace or this new batch.
New-Item -ItemType Directory -Path $archiveBatch | Out-Null
$batch | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $archiveBatch 'archive-manifest.json')
foreach($entry in $verified){
 New-Item -ItemType Directory -Path (Split-Path $entry.destination -Parent) -Force | Out-Null
 Move-Item -LiteralPath $entry.source -Destination $entry.destination
 $entry.record | ConvertTo-Json -Depth 6 -Compress | Add-Content -LiteralPath (Join-Path $archiveBatch 'completed-moves.jsonl')
}
@'
# Restore this golf/fishing archive

archive-manifest.json records original workspace-relative paths, SHA-256, sizes,
reasons, validation evidence and the pre-archive Windows build hashes. Files keep
their original relative paths. Existing Legacy history has not been changed.

Close Unity. For each file to restore, verify its SHA-256 against the manifest,
then copy it from this batch to C:\UMPSA\<original>. Compare any newer destination
before overwriting it. Restore Unity assets and their .meta files together to
preserve GUIDs. No Git internals, generated Unity caches, approved stadium assets,
current G2/L2 sources or selected concept references were archived.

Old directories may remain empty; Unity can regenerate metadata for those empty
folders. These are not runtime dependencies. Do not restore old generators into
routine use without deliberately choosing to revert to the older environment.
'@ | Set-Content -LiteralPath (Join-Path $archiveBatch 'README.md')
$bytes=($batch.files | Measure-Object -Property bytes -Sum).Sum
"Archived $($batch.files.Count) files ($([Math]::Round($bytes/1MB,2)) MiB) to $archiveBatch"
