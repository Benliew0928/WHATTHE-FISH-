# Self-contained island packaging

The normal Windows and Android builders embed all four islands in one lossless
scene AssetBundle. There are no remote content downloads and no removed islands.
One bundle shares dependencies across Bootstrap and the four detailed scenes.
Resource-loaded assets remain in the player. Android release builds also enable
Java minification; the existing rendering and mobile import settings are retained.

## Build and inspect

Run `Tools/Build/Build.ps1 -Target AndroidSubmission`. The build still enforces
the strict 100,000,000-byte ceiling; keep the development target at 75,000,000
bytes or less. Measure the APK itself, not the source folder or serialized report.

`PackagedContentBuild` regenerates ignored scene copies under
`Game/Assets/_Game/GeneratedPackaging/`. It applies the existing platform-specific
substitutions to the copies, leaving authored scenes and high-quality masters
intact. Do not commit these copies. A fresh clone regenerates them automatically.

The intermediate bundle and embedded catalog are under
`Builds/PackagedContent/<target>/`. Only its `Payload/` contents are added to the
player's `StreamingAssets/PackagedContent` through the build pipeline. Temporary
experiments do not belong in Unity's Resources or StreamingAssets folders.

`Builds/SizeAudit/latest/` contains two inventories:

- `packed-assets.csv` and `packed-layout.csv`: player-side serialized assets.
- `bundle-packed-assets.csv` and `bundle-packed-layout.csv`: embedded scene assets.
- `bundle-catalog.json`: compressed bundle bytes, SHA-256, CRC and all five scenes.

The APK ZIP entry report gives the actual compressed package contributions.
The Python packed-asset estimator cannot apportion the LZMA scene bundle to
individual source assets; it explicitly reports that limitation. Do not confuse
the smaller player-only inventory with the complete game's asset footprint.

## First launch and storage

The tiny entry scene displays a progress bar while local content is prepared.
It verifies the embedded LZMA bundle's SHA-256, length and CRC, then recompresses
it asynchronously into an LZ4 runtime cache. Successful temporary output is moved
into place atomically. Failed preparation offers Retry. Interrupted temporary
files are not treated as completed bundles.

The cache lives at `Application.persistentDataPath/PackagedContent-v1/`, keyed by
the embedded content hash. Later launches reuse it. Clearing app data or changing
the content hash requires preparation again. The APK is smaller, but installed
storage includes this additional cache; it is not a reduction in all disk usage.
Old hash caches are retained, so repeated development upgrades may accumulate
cache files until app data is cleared. Do not promise Android startup or storage
figures based on the much larger Windows bundle.

Bootstrap loads additively. The small boot scene remains alive so runtime-created
services and development probes survive. Existing Sky-Sail streaming continues
to load one detailed island at a time, with everyone travelling together in a
host room. Programmed sports bots, controls and match rules are unchanged.

## Validation

Build a separate review player with Unity execute method
`PackagedContentBuild.BuildWindows`, then run:

```powershell
& .\Tools\Build\Test-Packaging.ps1
& .\Tools\Build\Test-SkySail.ps1 -PlayerPath .\Builds\PackagingQA\Player\WhatTheFish.exe -OutputPath Builds/PackagingQA/Travel
& .\Tools\Build\Test-SkySail.ps1 -Network -PlayerPath .\Builds\PackagingQA\Player\WhatTheFish.exe -OutputPath Builds/PackagingQA/TravelNetwork
```

`Test-Packaging.ps1` uses a new isolated development cache by default. It checks
cold and warm boot, two four-island load/unload circuits, geometry, collision,
material support, nonblank rendered views and runtime diagnostics. Startup times
use Unity's runtime clock rather than full OS process launch time. A physical
Android device is still required to qualify first launch, installed storage,
memory peaks, touch controls and frame rate.

`PackagedContentBuild.BuildBaselineWindows` is a development-only unpackaged
comparison build. `Test-Packaging.ps1 -Baseline -PlayerPath <baseline-player>`
can measure it. Do not use its larger APK-equivalent layout for delivery.

See [measured releases](BUILD-SIZE.md), [size audit](APK-SIZE-AUDIT.md) and
[verification](VERIFICATION.md) for current results and retained evidence.
