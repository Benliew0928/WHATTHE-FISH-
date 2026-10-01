# APK size audit — 1 October 2026

The current self-contained APK is **85,957,194 bytes (85.96 MB / 81.98 MiB)**. It includes all four islands, shared Sky-Sail travel, the steerable character jump, basketball model/physics foundation, golf equipment and two fishing rods. It leaves **14,042,806 bytes (14.04 MB) to the strict 100,000,000-byte hackathon ceiling**. No external asset download is required.

The **75 MB working target remains unmet by 10,957,194 bytes (10.96 MB)**. Measure each addition and preserve the shared reserve for gameplay, effects, props and audio.

Artifact: `Builds/Android/WhatTheFish-release.apk`. Built on 1 October at 20:40:28 Malaysia time (12:40:28 UTC) with Unity 6000.3.20f1, release IL2CPP, ARM64, minimum Android API 26 and default Android ZIP compression. `AndroidSubmission` passed the size gate; APK v2 signature verification passed.

SHA-256: `99B19C56933544AC79A2CD597673DE1840D39A5D9378C63EF6B859A739EDC8C1`.

## Jump motion revision

Measured before/after APK: **87,153,874 → 85,957,194 bytes (−1,196,680 bytes)**. The pre-revision length was read directly before another build replaced that APK; its hash was not captured. Most of this shared-tree reduction comes from the concurrent fishing optimization below. The revised animation uses **37,124 serialized bytes**, approximately **24,231 compressed bytes**, compared with the original jump's 29,292 / 17,367 bytes. It adds no character meshes, textures or external dependencies.

The revised poses coordinate the arm backswing, leg extension at physical takeoff, relaxed flight and landing absorption. The importer now follows the full 61-frame source range. The motor presents an 80 ms grounded load immediately, keeps horizontal input active and shares preparation progress through protocol 11. All **126 Windows checks** passed: 40 jump/room, 24 tackle/room and 62 turning/room checks. Side, front and airborne-reversal motion captures, plus both LODs, were inspected. The fixed-rate capture is an animation review, not a performance benchmark.

Largest contributors remain timber, character maps/mesh/animation, golf structures/terrain and stadiums. The per-asset and ZIP-entry comparison found no unexpected large dependency growth. The signed Android submission build passed; no phone was connected. Phone appearance/FPS and WAN latency remain unverified.

Evidence is under `Builds/JumpNaturalQA/`: final build/signature logs, exact APK bytes/hash, `SizeAuditFinal/`, contribution estimates, portability audit and `jump-review.mp4`. Final jump checks use `Builds/JumpQA/Run-20261001-203848/`; tackle and turn use `Run-20261001-203321/` and `Run-20261001-203333/` in their QA folders. Superseded poses/captures and the temporary portable checkout were archived to `Legacy/20261001-204131-970-jump-motion-revision/`.

## Fishing rod addition

Measured before/after APK: **85,088,760 → 85,955,058 bytes (+866,298 bytes)**. This working-tree delta includes concurrent jump refinements: the jump clip's separate ZIP estimate changed from 17,367 to 22,223 bytes. Rod art totals **981,892 uncompressed serialized bytes** and approximately **852,462 compressed bytes**. Separate asset compression estimates are not exact allocations of shared APK chunks.

The first fishing build was **87,153,874 bytes**. Reducing imported colour maps from 1024 to 512 pixels and normals from 512 to 256, using ASTC 6x6 for both, and applying Low mesh packing reduced the rod estimate from about 2.06 MB to 0.85 MB. The final APK is **1,198,816 bytes smaller** than that first pass, despite concurrent jump changes. Source GLBs, editable masters and full-resolution maps are retained outside Unity Assets. Near LODs total 24,000 triangles and distant LODs 7,000; each rod shares one material across both. The rack reuses existing timber.

All **22 Windows rod checks** passed; close, reverse, guide, distance and pier views were inspected after the final lossy import changes. The reduced maps soften extreme close detail; source reconstruction flaws remain documented in [the fishing report](FISHING-RODS.md). Largest APK contributors remain timber, character maps/mesh/animation, golf structures/terrain and stadiums. The comparison found no unexpected large dependency growth.

Evidence under `Builds/FishingRods/20261001/` includes exact APK measurements/hashes, signature verification, `SizeAuditBefore/`, `SizeAuditFinal/`, asset and ZIP-entry comparisons, portability checks and `Player-Compact/` captures. Android submission and v2 signature passed; no phone was connected. These Windows checks do not qualify Android ASTC appearance or phone performance.

## Jump and shared golf-equipment build

Measured before/after APK: **83,751,052 → 85,088,760 bytes (+1,337,708 bytes)**. The working tree also gained the separate golf-equipment integration, so this combined delta must not be attributed entirely to jumping. The new animation-only FBX shares the existing character rig, mesh and textures: **29,292 uncompressed serialized bytes**, with a **17,367-byte separate DEFLATE estimate**. Golf equipment contributes approximately **1,314,630 bytes** by the same estimator. Runtime/metadata, scene packing and ZIP chunk boundaries account for the remaining difference; per-asset estimates are not exact allocations of the APK.

The first build measured **85,817,216 bytes**. Inspection found world regeneration had reset the previously approved high compression on distant island proxies. Restoring that packing recovered **728,456 actual APK bytes**. The Sky-Sail generator now packs distant silhouettes at High and near cabin/station modules at Low; the Android release path also repairs missing packing. This restores existing delivery settings, retaining the original source geometry and authoring masters. Largest contributors remain timber, character colour/mesh/animation, golf structures/terrain and stadiums. No new jump mesh or texture ships.

All **126 Windows checks** passed: 40 jump/room, 24 tackle/room and 62 turning/room checks, including host/guest action replication, capsule wall/ceiling collision, input reversals at 20/30/60/120 FPS, both animation LODs, and jump controls on all four islands. Both LOD jump/recovery captures were inspected. Signature verification passed after the final Android build; no phone was connected. These checks do not qualify phone frame rate, Android visual quality or WAN latency.

That checkpoint's evidence is in `Builds/JumpQA/`, including before/after bytes/hashes, signed build log, per-asset estimates, portability audit and `Run-20261001-195405/`. Tackle and turn reports use the same run suffix in their respective QA directories. See [jump authoring and controls](JUMP-ANIMATION.md). Superseded fixtures and the temporary portable checkout were archived in `Legacy/20261001-195347-370-jump-validation/`; retained evidence remains under Builds.

## Golf equipment validation

The ball and three clubs use eight closed mesh LODs, four materials and ten maps. Highest-detail geometry totals **15,264 triangles**, dropping to **4,320** at distance. Clubs use 512-pixel colour/normal maps and 256-pixel metal/smoothness masks; the ball uses a 1024×512 dimple normal. ASTC imports, derived meshes and shared rack timber keep original downloads and full-resolution masters outside the APK.

Golf art totals **2,870,024 uncompressed serialized bytes** and approximately **1,314,630 bytes** by separate per-asset DEFLATE estimates, including 596,010 for the ball normal. These estimates cannot exactly allocate shared APK chunks. All **20 equipment checks**, **126 golf-island checks** and the local-room suite passed. Seven actual player views were inspected, and the final generator passed from a checkout with spaces and an unrelated working directory. Oversized irregular dimples, soft face grooves and uneven grip accents remain visible source limitations.

See [the golf equipment report](GOLF-EQUIPMENT.md). Retained evidence under `Builds/GolfEquipment/20261001/` includes a final `SizeAudit/` snapshot, APK measurements, entry deltas, asset estimates, signature, portable dependency checks and actual Windows captures. No phone was connected; Windows views do not qualify Android appearance or performance.

## Basketball addition (earlier checkpoint)

The APK increased from **83,282,808 to 83,751,052 bytes: +468,244 bytes**. The ball uses two mesh LODs (2,976 / 720 triangles), a single shared material and two 1024×512 ASTC 6×6 maps. The original 9.09 MB GLB and high-resolution native source stay outside Unity Assets. Ball art totals **720,260 uncompressed serialized bytes**, which is a different measurement from the APK increase.

Per-asset ZIP estimates are 234,658 bytes for the normal map, 172,805 for colour, 35,436 for the FBX and 701 for the two materials. These estimates total 443,600 bytes; separate DEFLATE estimates cannot exactly allocate the APK's compressed chunks. The balance includes physics proxies, code and packing changes. Largest contributors remain timber, character maps/animations, golf structures/terrain and stadiums. No unexpected large dependency was added. Unity refreshed its Android player resource list, omitting unused SSAO resources while retaining the authoring entries.

All **53 Windows basketball checks** passed, and close/reverse/court-distance views were inspected. Both hoop collisions, 35 m/s floor/backboard shots, spin, streaming and host/guest authority were exercised. No phone was connected; Android appearance, networking and performance remain unqualified. See [the implementation report](BASKETBALL-BALL.md).

Basketball checkpoint evidence: `Builds/BasketballModel/20261001/` (`apk-before.json`, `apk-after.json`, `apk-entry-delta.csv`, `asset-zip-estimates.csv`, `apk-signature.txt`, `adb-devices.txt`). These generated reports stay ignored.

## September optimization history

The 30 September portable-path validation rebuild was **4 bytes larger** than the 29 September APK. Only asset path strings and integrity hashes changed; geometry and texture content were checked unchanged. The earlier mobile asset pass reduced the 350.25 MB cleanup checkpoint by **266.97 MB (76.22%)** before the basketball addition below.

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
| Basketball model and physics foundation, 1 October | **83.75** |
| Jump and shared golf-equipment working tree, 1 October | **85.09** |
| Fishing rods and concurrent jump refinements, 1 October | **85.96** |
| Revised jump motion and takeoff alignment, 1 October | **85.96** |

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

The current ZIP contains **59.64 MB** of Unity scene/art/shader data and **20.77 MB** of native libraries. Managed metadata, Android code/resources and archive overhead account for the remaining **5.55 MB**. Exact per-entry values are in `Builds/SizeAudit/latest/apk-entries.csv`.

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
4. Treat **14.04 MB** as one shared reserve for animations, effects, props, audio and fixes. Leave several MB unallocated for integration growth. Reaching 75 MB requires further measured work; no additional saving is promised from untested changes.
5. Qualify the selected minimum phone before claiming stable 30 FPS or raising visual settings.

After editing art, run `build_mobile_vegetation.py` and `build_mobile_models.py` in Blender, `build_shared_mobile_materials.py` with Python/numpy/Pillow, then `Tools/Build/Build.ps1 -Target OptimizeAndroid`. This reapplies import settings and rebuilds the delivery libraries. `-Target MobilePreview` builds optimized geometry into `Builds/WindowsMobilePreview/`; ordinary Windows builds retain authoring scene assets.
