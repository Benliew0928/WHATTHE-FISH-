> Historical checkpoint before the mobile optimization pass. For the current package, see [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).

# APK size audit — 29 September 2026

Submission constraint: one APK smaller than **100,000,000 bytes**. After cleanup and a fresh Android release build, the APK is **350,248,013 bytes (350.25 MB)**. It still does not meet the constraint. The original measured APK was 354,313,157 bytes; this pass saved **4,065,144 bytes (4.07 MB, 1.15%)**.

Measured stages: 354,313,157 bytes originally; 352,899,117 after dependency/scene cleanup; 350,668,393 after packing terrain-control channels; **350,248,013** after removing the unused stone-mask input. These are measured APK differences, not estimates from source file sizes.

## Completed cleanup

- Moved four legacy environment prefabs out of `Resources` into `Prefabs/Environments`, preserving GUIDs and authoring data. Runtime uses the four additive scenes. Builders and audits now use the new paths.
- Moved the two directly referenced athlete prefabs into `Prefabs/Characters`, preserving their GUIDs and contents.
- Moved the desktop URP profile into `Settings`. Windows selects it through the build's default pipeline; editor previews resolve it directly. Android scene processing removes desktop SSAO references from build copies only.
- Removed eight permanently disabled ocean/cloud groups from the four island scenes. Sky-Sail supplies the shared ocean and clouds. The scene generator also removes these retired groups on future regeneration.
- Switched the two terrain-control maps from RGB24 to R8 **on Android only**, retaining 4096x4096 resolution and all 13 mip levels. Every red-channel sample was compared before/after and was byte-identical. The terrain shader samples only red. This removes 85.3 MiB of uncompressed texture data and saved another 2.23 MB in the APK (352,899,117 -> 350,668,393 bytes).
- Removed the unused metallic/smoothness mask reference from `RI_Stone.mat`; the stone shader samples only base color and normal textures and uses fixed smoothness. Updated the generator so it does not restore the unused reference.
- Added per-asset serialized size reports after Android builds, a warning above 95 MB, and `Tools/Build/Build.ps1 -Target AndroidSubmission`, which rejects an APK of 100,000,000 bytes or more.

The rebuilt Android report contains neither the environment prefab assets, desktop URP profile/renderer, nor the unused stone mask. There are no project-owned Resources assets remaining. No texture resolution, mesh detail, cabin asset, island, animation or gameplay feature was reduced. Editable source assets remain available.

The small saving is significant evidence: nearly all large textures and meshes belong to the live islands. Deleting source masters, review images, build caches or assets Unity already excludes would not shrink this APK. Unlisted prefabs/FBXs must not be deleted blindly: several are source inputs for generated meshes or get flattened into scenes.

## When it increased

- The 72,690,957-byte APK in `Docs/BUILD-SIZE.md` was built on 25 September at 23:16 Malaysia time. It contained the earlier football, basketball and golf environments.
- Fishing was added after that baseline (`4d15ede`, 26 September).
- The coastal stadiums and G2/L2 island art were introduced in `cffa30b` (27 September), including much larger coast/vegetation meshes and material maps.
- The 29 September Android build includes those refinements plus Sky-Sail (`78cfac1`). There is no retained intermediate APK measurement here that isolates the exact compressed-size increase at each step.

## Actual APK contents

Measured using the ZIP central directory of `Builds/Android/WhatTheFish-release.apk`:

| Contents | Bytes in APK | Decimal MB |
| --- | ---: | ---: |
| Compressed Unity game data (`data.unity3d`) | 314,452,311 | 314.45 |
| Runtime, native libraries, metadata, Android resources and ZIP overhead | 35,795,702 | 35.80 |
| Total APK | 350,248,013 | 350.25 |

The game-data archive already uses LZ4HC compression. Its ZIP entry is stored without a second compression layer. The package is ARM64 only and a release build; debug-mode and multiple CPU architectures are not the primary cause.

With the current runtime overhead, a 95 MB submission target leaves approximately **59 MB for game data**, compared with 314.45 MB currently. Reaching 95 MB requires another **255.25 MB** reduction in the complete APK.

## Asset evidence

The rebuilt Android log (`Builds/SizeAudit/latest/build.log`, "Used Assets" section) reports approximately **379.6 MiB of textures and 403.1 MiB of meshes before archive compression**, out of 788.5 MiB reported user assets. Textures were previously 470.3 MiB. These are not additional APK bytes or measured runtime RAM usage. Together textures and meshes account for about 99.3% of reported user-asset data.

Large entries include:

| Asset group | Reported uncompressed size |
| --- | ---: |
| Football coast mesh | 122.5 MiB |
| Basketball coast mesh | 85.4 MiB |
| Golf and fishing grass meshes | 71.5 MiB combined |
| Golf and fishing terrain-control maps | 42.7 MiB combined (previously 128 MiB) |
| Golf and fishing water-depth maps | 32 MiB combined |

Assets under `Art/SkySail` account for approximately **12.97 MiB before compression**, including distant island meshes and cabin/station/cable meshes. Reused island material assets are counted under their existing art folders. This is not a measured compressed-size delta, but it shows the new Sky-Sail folder is a small part of the serialized asset payload.

The coast FBX importers currently use `meshCompression: 0`. Apart from the two cleaned terrain-control maps, refined-island textures generally have no active Android platform override. The football coast FBX contains about **3.13 million triangles** and basketball about **2.19 million**, counting the imported meshes, not simultaneous on-screen triangles. Their palm-garden meshes alone contain about **4.68 million triangles combined**. One football palm cluster has 246,304 triangles. This is the first geometry target.

The coastal/refined 2K material masks mostly have constant RGB channels and only small smoothness variation in alpha. They are candidates for smaller Android maps or a more compact shader representation, but their repeated values already compress well in the archive. Their large uncompressed numbers must not be treated as guaranteed APK savings.

Unity includes all Resources assets even when no scene references them: [Unity 6.3 Resources documentation](https://docs.unity3d.com/6000.3/Documentation/Manual/LoadingResourcesatRuntime.html). Moving an obsolete prefab out of Resources only saves its uniquely retained dependencies; it does not automatically remove another copy of every shared mesh or texture.

Large uncompressed maps can compress very well. The figures above identify candidates, not promised APK savings. Each change needs a measured rebuild.

## Reduction order

1. **Scenery geometry:** replace repeated baked grass/palm geometry with reusable meshes and placement data; apply mesh compression and remove unnecessary vertex data where supported. Preserve coastline and landmark silhouettes, player collision and cabin detail.
2. **Android texture settings:** size control/depth/mask maps according to their actual data needs; test suitable mobile compression and smaller normal/material maps. Keep more resolution for the cabin and other close surfaces. Preserve original source textures and desktop settings.
3. **Build dependencies — completed:** removed unused environment prefabs, retired scene scenery, desktop-only rendering dependencies and an unused material input from Android's included graph. Combined with lossless terrain-channel packing, actual saving: 4.07 MB.
4. **Build gate — implemented:** rebuild, inspect the same views and routes, and fail the submission build when the final APK is 100 MB or larger. Target 90–95 MB for margin. Ordinary release builds remain available for development but explicitly warn when over budget.

Do this continuously, starting with the palm/coast geometry before adding further detailed scenery. Do not allow the shipping APK to grow to 700–800 MB and rely on a final compression pass. Source `.blend` files and textures can remain large outside the package. Preserve cabin/interior quality and important silhouettes; use shared palm/grass meshes, compact placement data, carefully checked mesh compression and Android-specific texture imports for the delivered game. Mesh compression is lossy and needs comparison, not a blanket maximum setting: [Unity mesh compression guidance](https://docs.unity.com/en-us/engine/6000.6/manual/analysis/graphics-performance-profiling/compressing-mesh-data-optimization/types-of-mesh-data-compression).

An initial planning budget (not a measured achievable result) is 36 MB runtime/Android overhead, 25 MB textures, 22 MB meshes, 5 MB audio and 7 MB scenes/shaders/animation/other: **95 MB total**. Measure actual archive changes as each group is optimized; per-asset serialized sizes are not additive compressed allocations.

All four islands and the shared ride remain the intended scope. The exact visual compromises and achievable size have not been measured yet. This plan assumes a self-contained APK, with no external asset downloads.

Scene streaming and distant LODs improve rendering/memory behavior, but do not remove all four islands from a self-contained APK. Fitting the package needs a separate asset-size pass.

## Evidence and validation

- APK built 29 September 2026 at 18:14 Malaysia time; release IL2CPP, ARM64, Unity 6000.3.20f1, LZ4HC. `apksigner verify --verbose` passed (v2 signature).
- SHA-256: `3D6D4460D2EB573E84F1D796E0493A9ADE684661AE35A549ECF52A002FCC332B`.
- Dependency cleanup checked every build scene for missing scripts, materials and meshes. A serialized scene comparison found only the eight retired groups removed and their parent child lists changed; all other scene objects were unchanged.
- All seven relocated assets and their GUID files were compared with their originals; contents were unchanged.
- Windows rebuilt after scene/pipeline cleanup and passed all **54 checks** of the full Football -> Golf -> Basketball -> Fishing -> Football travel circuit. This covers boarding, riding, scene unloading, docking, disembarking collision and restored walking. All 21 screenshots were captured; cabin and golf-station views were inspected. Later terrain import changes affect Android only; the stone-mask removal does not change shader output.
- The Android terrain validation compared **44,739,242 red-channel samples** across the two maps and all 13 mip levels: zero changes. Source PNGs, default/desktop texture imports, filtering and shader code were unchanged.
- Detailed before/after asset CSVs, mesh/triangle inventories, build logs, ZIP entry sizes and the cleanup manifest are in `Builds/SizeAudit/`. These local generated reports are excluded from Git; this document records the durable conclusions.
- Physical-phone installation, RAM, frame rate and transition latency remain unmeasured. This pass does not establish phone performance or a final sub-100 MB art configuration.
