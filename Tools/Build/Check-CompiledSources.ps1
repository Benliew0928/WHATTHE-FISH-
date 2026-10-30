param([Parameter(Mandatory)][string]$PdbPath,[Parameter(Mandatory)][string]$OutputPath)
$ErrorActionPreference='Stop'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
function Resolve-SourceAuditPath([string]$Value){if([IO.Path]::IsPathRooted($Value)){return [IO.Path]::GetFullPath($Value)};return [IO.Path]::GetFullPath((Join-Path $root $Value))}
$pdb=Resolve-SourceAuditPath $PdbPath;$output=Resolve-SourceAuditPath $OutputPath
if(!$output.StartsWith((Join-Path $root 'Builds')+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Generated source audit must be under Builds/.'}
$stream=[IO.File]::OpenRead($pdb)
$provider=[System.Reflection.Metadata.MetadataReaderProvider]::FromPortablePdbStream($stream)
$records=[Collections.Generic.List[object]]::new()
try {
 $reader=$provider.GetMetadataReader()
 foreach($handle in $reader.Documents){
  $document=$reader.GetDocument($handle);$name=$reader.GetString($document.Name).Replace('\','/')
  $marker=$name.IndexOf('/Assets/',[StringComparison]::OrdinalIgnoreCase)
  if($marker -lt 0){continue}
  $relative='Game'+$name.Substring($marker)
  if(!$relative.StartsWith('Game/Assets/_Game/',[StringComparison]::Ordinal)){continue}
  $algorithm=$reader.GetGuid($document.HashAlgorithm)
  $hashName=if($algorithm -eq [guid]'8829d00f-11b8-4213-878b-770e8597ac16'){'SHA256'}elseif($algorithm -eq [guid]'ff1816ec-aa5e-4d10-87f7-6f4963833460'){'SHA1'}else{throw "Unknown source checksum algorithm: $algorithm"}
  $compiled=[Convert]::ToHexString($reader.GetBlobBytes($document.Hash))
  $source=Join-Path $root $relative
  $actual=if(Test-Path -LiteralPath $source -PathType Leaf){(Get-FileHash -LiteralPath $source -Algorithm $hashName).Hash}else{''}
  $records.Add([ordered]@{path=$relative;algorithm=$hashName;compiledHash=$compiled;currentHash=$actual;match=$compiled -eq $actual})
 }
}finally{$provider.Dispose();$stream.Dispose()}
if(!$records.Count){throw 'No project source documents found in the PDB.'}
New-Item -ItemType Directory -Force -Path (Split-Path $output -Parent) | Out-Null
$records | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $output
$mismatches=@($records | Where-Object {!$_.match})
if($mismatches.Count){throw "Compiled source mismatches: $($mismatches.path -join ', ')"}
Write-Output "COMPILED_SOURCES_MATCH documents=$($records.Count)"
