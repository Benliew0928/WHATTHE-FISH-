$ErrorActionPreference = 'Stop'
$editorRoot = 'C:\Program Files\Unity\Hub\Editor\6000.3.20f1'
$cacheRoot = 'C:\UMPSA\Tools\Downloads'
New-Item -ItemType Directory -Force -Path $cacheRoot | Out-Null
$modules = Get-Content -LiteralPath "$editorRoot\modules.json" -Raw | ConvertFrom-Json -AsHashtable
$selected = $modules | Where-Object { $_.id -match '^android|^cmake' }
foreach ($m in $selected) {
    $file = Join-Path $cacheRoot ($m.id + [IO.Path]::GetExtension(([uri]$m.url).AbsolutePath))
    $expectedSize = if($m.downloadSize -is [System.Collections.IDictionary]) { [long]$m.downloadSize.value } else { [long]$m.downloadSize }
    if ((Test-Path -LiteralPath $file) -and (Get-Item -LiteralPath $file).Length -ne $expectedSize) {
        throw "Cached download is incomplete: $file. Remove that single file before retrying."
    }
    if (!(Test-Path -LiteralPath $file)) {
        Write-Output "Downloading $($m.id)"
        & curl.exe -L --fail --retry 3 --silent --show-error -o $file $m.url
        if ($LASTEXITCODE -ne 0) { throw "Download failed: $($m.id)" }
    }
    if ((Get-Item -LiteralPath $file).Length -ne $expectedSize) { throw "Unexpected download length: $file" }
    if ($m.integrity -like 'md5-*') {
        $md5 = [System.Security.Cryptography.MD5]::Create()
        $stream = [IO.File]::OpenRead($file)
        try { $actual = [Convert]::ToBase64String($md5.ComputeHash($stream)) } finally { $stream.Dispose(); $md5.Dispose() }
        if ($actual -ne $m.integrity.Substring(4)) { throw "Integrity check failed: $file" }
    }
    if ($m.id -eq 'android') {
        if (!(Test-Path -LiteralPath "$editorRoot\Editor\Data\PlaybackEngines\AndroidPlayer\UnityEditor.Android.Extensions.dll")) {
            Write-Output 'Installing Android playback engine'
            $p = Start-Process -FilePath $file -ArgumentList @('/S',"/D=$editorRoot") -Wait -PassThru -WindowStyle Hidden
            if ($p.ExitCode -ne 0) { throw "Android installer exit $($p.ExitCode)" }
        }
        continue
    }
    $dest = $m.destination.Replace('{UNITY_PATH}', $editorRoot)
    if ($m.id -eq 'cmake-3.22.1') { $dest = "$editorRoot/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/cmake/3.22.1" }
    New-Item -ItemType Directory -Force -Path $dest | Out-Null
    & tar -xf $file -C $dest
    if ($LASTEXITCODE -ne 0) { throw "Extraction failed: $($m.id)" }
    if ($m.renameFrom) {
        $from = $m.renameFrom.Replace('{UNITY_PATH}', $editorRoot)
        $to = $m.renameTo.Replace('{UNITY_PATH}', $editorRoot)
        $resolvedFrom = [IO.Path]::GetFullPath($from)
        $resolvedTo = [IO.Path]::GetFullPath($to)
        if (!$resolvedFrom.StartsWith($editorRoot) -or !$resolvedTo.StartsWith($editorRoot)) { throw 'Invalid module destination' }
        if ($m.id -eq 'android-ndk-r27c') {
            Get-ChildItem -LiteralPath $from | Move-Item -Destination $to -Force
        } elseif (Test-Path -LiteralPath $from) {
            Move-Item -LiteralPath $from -Destination $to -Force
        }
    }
    Write-Output "Installed $($m.id)"
}
Write-Output 'Android tools ready'
