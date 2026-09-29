# Mobile asset optimization — 29 September 2026

The current self-contained APK is **83,282,804 bytes (83.28 MB / 79.42 MiB)**. It includes all four islands, shared Sky-Sail travel and the existing character animations. It is **266.97 MB smaller (76.22%)** than the 350.25 MB cleanup checkpoint, leaving **16.72 MB below the 100,000,000-byte hackathon ceiling**. No external asset download is required.

The **75 MB working target remains unmet by 8.28 MB**. This provides room for gameplay, but does not establish that every future feature will fit. Measure each addition; do not postpone optimization until the APK reaches 700–800 MB.

Artifact: `Builds/Android/WhatTheFish-release.apk`. Built at 20:01 Malaysia time with Unity 6000.3.20f1, release IL2CPP, ARM64, minimum Android API 26 and default Android ZIP compression. `AndroidSubmission` passed the size gate; APK v2 signature verification passed.

SHA-256: `894EE22E3E26F6BC19E513510A9DB4669CAACE846F699F3293FC7D035DF5EEE3`.

## Measured stages

These are actual APK sizes, not source-folder sizes or uncompressed asset estimates.

| Stage | APK MB |
| --- | ---: |
| Before dependency cleanup | 354.31 |
| Dependency cleanup and lossless terrain-channel packing | 350.25 |
| Android texture settings and mesh packing | 218.19 |
| Shared vegetation and regenerated golf/fishing grass | 128.31 |
| Tighter mesh packing, code stripping and ZIP delivery | 103.03 |
| Shared material patterns | 99.27 |
| Reduced render meshes and texture-alias cleanup | 89.50 |
| Regenerated coastal meadow, compact seating and materials | 83.49 |
| GPU instance batches and final build | **83.28** |

An intermediate experiment retained duplicate texture references and was larger. The final material builder updates URP's hidden `_MainTex` alias along with `_BaseMap`, preventing that duplication.

## What changed

- **Vegetation:** 76 coastal palms and all 1,292 palm/plant/tuft placements use nine shared modules and 27 LOD meshes. Near flowers retain their source shape; palm leaflets use fewer longitudinal segments. Explicit GPU instance batches avoid thousands of individual renderer submissions.
- **Grass:** three authored patches are stored once. Golf/fishing blades follow the original analytic terrain; coastal meadows use recorded positions and blade heights. Math runs on a worker and Unity mesh uploads span frames. The room waits for preparation before becoming ready. Tiny blades no longer cast individual shadows; larger objects retain shadows. Complete blades are trimmed at golf bunker boundaries.
- **Buildings:** 387 selected render meshes in delivered coastal scenes use simplified copies. Every source vertex in an accepted simplification was checked against the reduced surface, with a maximum distance of 2.5 cm before Unity packing. This is a source-vertex check, not a bound on every surface point. Meshes shared with colliders retain their original geometry. Another 160 small seating meshes use tighter packing; no seats were removed. High mesh compression is restricted to those seating groups and distant proxies.
- **Textures:** Android uses ASTC and appropriately sized maps. Character colour and cabin timber retain 2K maps; general shared patterns use 1K, normals 512/1K, and masks generally 256. Terrain colour stays 2K; terrain controls retain exact 4K R8 data and mipmaps. Water-depth maps use 2K ASTC 4x4.
- **Materials:** 24 colours use 11 shared patterns and deduplicated normal maps. Tint conversion was compared against every source RGB pixel: maximum error below 2/255 before resizing/GPU compression. Final compressed textures are not claimed to be pixel-identical.
- **Package/code:** Medium managed stripping, IL2CPP size-oriented code generation, ARM64-only delivery and default Android ZIP compression. Code generation and ZIP loading can trade speed for size; device timing remains necessary.

The cabin, doors, hanger, towers and cables were not geometrically simplified by the building pass. Modular authoring structure remains intact. Original FBX/PNG assets and Blender masters remain available; Android substitutes derived delivery assets in build copies of scenes. Source hashes guard against stale model and colour derivatives.

The earlier removal of Resources dependencies, desktop rendering dependencies and retired sea/cloud groups is recorded in [the historical cleanup report](APK-SIZE-CLEANUP-2026-09-29.md).

## Where space remains

The ZIP contains **56.98 MB** of Unity scene/art/shader data and **20.75 MB** of native libraries. Managed metadata, Android code/resources and archive overhead account for the remaining **5.54 MB**. Exact per-entry values are in `Builds/SizeAudit/latest/apk-entries.csv`.

Large individual art contributors now include 2K timber and character colour maps, golf structures/terrain, character mesh/animation data and stadium geometry. Baked coastal vegetation and repeated grass no longer dominate. Derived building/seating meshes are distributed across many asset entries.

`packed-assets.csv` reports **uncompressed serialized** bytes. For example, two 4K terrain-control maps total about 44.74 MB serialized but roughly 1.8 MB in a per-asset ZIP estimate. These are not RAM measurements and must not be added to the APK size.

`Tools/Build/analyze_packed_apk.py` estimates individual ZIP contributions using packing offsets. Separate per-asset DEFLATE differs from the APK's chunk compression; actual APK length is authoritative. The mesh inventory includes all submeshes of each contributing source FBX, including unused source submeshes. It is not a simultaneous draw count.

## Validation and limits

- Signed Android submission build succeeded below 100,000,000 bytes.
- Runtime terrain math matched **13,122 independent Blender reference samples**, maximum error approximately `1.78e-15` metres.
- The final optimized Windows circuit passed **60 checks**, covering four islands, scene release, docking collision, restored walking, prepared grass and bounded instance submissions. A separate host/guest run passed **18 checks** for synchronized rides, shared destinations and everyone disembarking together.
- All **34 walking-review checks** passed, including every shore-to-deck route and return. The circuit captured 51 screenshots. Close views of gardens, timber, course surfaces, stations and cabin were inspected. Instanced vegetation was visually checked after correcting the manual camera's capture order; sampled coastal views used **89 vegetation draw submissions**.
- The preview uses Android delivery geometry with the Windows renderer and desktop texture formats. It validates geometry, colours and travel logic, **not** Android ASTC appearance, IL2CPP networking, phone RAM/FPS, thermals or transition latency. No phone was connected (`adb devices` was empty).

Local evidence: `Builds/MobileOptimization/terrain-validation.txt`, `VerifiedTravelReview`, `FinalNetworkReview`, `WalkReview`, `InstancedCaptureReview`, logs and APK checkpoints. Builds/reports are excluded from Git. The reference fixture under `Tools/Blender/Fixtures/` supports fresh-clone math checks.

Housekeeping on 29 September retained the final evidence above and moved superseded checkpoints, intermediate logs/reviews and original-setting backups to `Legacy/20260929-201920-001-repository-hygiene/`, preserving their former workspace-relative paths. See [the recovery inventory and repository policy](REPOSITORY-HYGIENE.md). The current APK was not modified.

## Continuing development

1. Run `Tools/Build/Build.ps1 -Target AndroidRelease` after meaningful asset additions. Reports update automatically; the build warns above **75 MB**.
2. Use `-Target AndroidSubmission` for the hard **under-100-MB** check. Passing it does not reserve space for every planned feature.
3. Reuse shared materials, vegetation and prop modules. Keep high-resolution masters and make deliberate mobile exports. Avoid a new 2K/4K texture set for every small prop.
4. Treat **16.72 MB** as one shared reserve for animations, effects, props, audio and fixes. Leave several MB unallocated for integration growth. Reaching 75 MB requires further measured work; no additional saving is promised from untested changes.
5. Qualify the selected minimum phone before claiming stable 30 FPS or raising visual settings.

After editing art, run `build_mobile_vegetation.py` and `build_mobile_models.py` in Blender, `build_shared_mobile_materials.py` with Python/numpy/Pillow, then `Tools/Build/Build.ps1 -Target OptimizeAndroid`. This reapplies import settings and rebuilds the delivery libraries. `-Target MobilePreview` builds optimized geometry into `Builds/WindowsMobilePreview/`; ordinary Windows builds retain authoring scene assets.
