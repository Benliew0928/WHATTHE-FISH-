# Run only after LegacyAssetAudit.Prepare and migration of builder dependencies.
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path (Split-Path $PSScriptRoot -Parent) -Parent))
$archiveRoot=[IO.Path]::GetFullPath((Join-Path $root 'Legacy'))
if(!$archiveRoot.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Archive must stay inside workspace'}
$paths=[Collections.Generic.List[string]]::new()
foreach($relative in @(
 'ArtSource\Football\Stadium.blend','ArtSource\Basketball\Arena.blend','ArtSource\Golf\Island.blend','ArtSource\Golf\island-dimensions.json',
 'ArtSource\Legacy','Tools\Blender\build_assets.py','Tools\Blender\build_basketball.py','Tools\Blender\inspect_assets.py',
 'Builds\Windows','Builds\WindowsLatest','Builds\WindowsTest','Builds\BasketballReview')){$paths.Add($relative)}
foreach($file in Get-ChildItem -LiteralPath (Join-Path $root 'ArtSource') -Recurse -File | Where-Object {$_.Extension -match '^\.blend[0-9]+$'}){
 if(!$file.FullName.Contains('\ArtSource\Legacy\')){$paths.Add($file.FullName.Substring($root.Length+1))}
}
foreach($line in Get-Content -LiteralPath (Join-Path $root 'Builds\legacy-asset-candidates.txt')){
 if($line.StartsWith('UNUSED ')){
  $relative='Game\'+$line.Substring(7).Replace('/','\');$paths.Add($relative)
  if(Test-Path -LiteralPath (Join-Path $root ($relative+'.meta'))){$paths.Add($relative+'.meta')}
 }
}
New-Item -ItemType Directory -Path $archiveRoot -Force | Out-Null
$records=[Collections.Generic.List[object]]::new()
foreach($relative in $paths | Select-Object -Unique){
 $source=[IO.Path]::GetFullPath((Join-Path $root $relative));$destination=[IO.Path]::GetFullPath((Join-Path $archiveRoot $relative))
 if(!$source.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase) -or !$destination.StartsWith($archiveRoot+'\',[StringComparison]::OrdinalIgnoreCase)){throw "Path escaped intended roots: $relative"}
 if(!(Test-Path -LiteralPath $source)){continue}
 if(Test-Path -LiteralPath $destination){throw "Archive destination already exists: $destination"}
 $item=Get-Item -LiteralPath $source
 $bytes=if($item.PSIsContainer){(Get-ChildItem -LiteralPath $source -Recurse -File | Measure-Object -Property Length -Sum).Sum}else{$item.Length}
 New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
 Move-Item -LiteralPath $source -Destination $destination
 $records.Add([pscustomobject]@{original=$relative;archive=$destination;bytes=$bytes})
}
$records | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $archiveRoot 'archive-manifest.json')
$bytes=($records | Measure-Object -Property bytes -Sum).Sum
Write-Output "Archived $($records.Count) entries; $([Math]::Round($bytes/1e6,2)) MB. Folder: $archiveRoot"
