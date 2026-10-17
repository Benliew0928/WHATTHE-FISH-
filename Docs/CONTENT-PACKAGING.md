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
files are not treated as completed bundles. The error screen explains that free
storage should be checked before retrying.

Where StreamingAssets is a regular filesystem directory, preparation reads the
original bundle directly instead of first duplicating it into the cache. Android
APK assets still need the temporary local copy before conversion. SHA-256, length
and conversion CRC checks remain in both paths. A cache-folder file lease serializes
preparation between simultaneous launches. Owned download, partial and Unity
partial-temporary files are cleaned after failure and on the next preparation;
the original embedded bundle is never deleted.

The cache lives at `Application.persistentDataPath/PackagedContent-v1/`, keyed by
the embedded content hash. Later launches reuse it. Clearing app data or changing
the content hash requires preparation again. The APK is smaller, but installed
storage includes this additional cache; it is not a reduction in all disk usage.
After the current bundle successfully loads, the loader removes obsolete hash
caches from its own folder. Other files, preferences, saves and Unity Editor
caches are untouched. Previous complete caches are retained if the new content
fails to prepare. Do not promise Android startup or storage figures based on the
much larger Windows bundle. APK size and first-launch working space are separate
requirements.

Bootstrap loads additively. The small boot scene remains alive so runtime-created
services and development probes survive. Existing Sky-Sail streaming continues
to load one detailed island at a time, with everyone travelling together in a
host room. Programmed sports bots, controls and match rules are unchanged.

## Runtime lighting references

Do not serialize a desktop renderer feature into an island scene or environment
prefab. A player build and a separate scene bundle can each contain their own
copy of the same source ScriptableObject. Updating the bundled copy does not
update the feature used by the player's camera.

`IslandContactShading.Resolve` selects the active camera's renderer through URP's
renderer-data API. Coastal and refined island environments bind to that live
SSAO feature when activated; a renderer without SSAO (including Android's mobile
renderer) safely returns no feature. Golf/Fishing restore the prior settings on
exit, and Football/Basketball retain their interior/exterior activation rules.
Textures, materials, geometry and the lossless packaging format are unaffected.

This corrects the Windows shading regression missed by the initial packaging
asset-equality checks. Identical art bytes do not establish runtime lighting
parity. Build with Unity execute method `CharacterLightingBuild.BuildReview`,
then run `Tools/Build/Test-CharacterLighting.ps1`. The test checks live feature
identity and settings through two four-island circuits, court/field boundaries,
character movement and matching front/back captures with corrected and former
stuck settings. Inspect the captures under `Builds/CharacterLightingQA/` as well
as the assertions; pixel checks alone do not establish visual quality.

## Validation

For Windows code-only iterations after a complete build,
`FishingPolishBuild.WindowsScripts` reuses the same packaged boot scene,
bundle manifest and full-build payload. It registers the additional
StreamingAssets again and requires unchanged hashes for native player data,
catalog and scene bundle. Unity recreates StreamingAssets even for a
scripts-only build; omitting that registration produces an incomplete player.
Only use this path when serialized assets and type layouts are unchanged.
Asset, scene, rendering or serialized-field changes require the full build.
Validate and deliver the entire resulting player folder, including the bundle.

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

Use `-UseDefaultCache` to test the normal application cache without an override.
It expects a cold cache for the current catalog on the first run; it does not clear
app data automatically. `-RetryReview` creates a one-byte blocker only inside a
fresh review output, verifies the usable error screen, removes that fixture,
retries and checks cleanup and all four islands. Add `-CopySource` to exercise
the temporary-copy path used for APK assets on a desktop player. The corresponding
player arguments are development-only. Keep failure/retry and normal-path evidence
alongside isolated-cache results; an isolated warm cache cannot qualify a user's
cold launch or available storage.

`PackagedContentBuild.BuildBaselineWindows` is a development-only unpackaged
comparison build. `Test-Packaging.ps1 -Baseline -PlayerPath <baseline-player>`
can measure it. Do not use its larger APK-equivalent layout for delivery.

See [measured releases](BUILD-SIZE.md), [size audit](APK-SIZE-AUDIT.md) and
[verification](VERIFICATION.md) for current results and retained evidence.
