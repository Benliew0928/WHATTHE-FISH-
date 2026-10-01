# Current build size — 1 October 2026

The current Android release APK contains all four islands, Sky-Sail, the steerable character jump, basketball model/physics foundation, golf equipment and two fishing rods. Its exact size is **85,957,194 bytes (85.96 MB / 81.98 MiB)**, leaving **14,042,806 bytes (14.04 MB)** to the hackathon's strict 100,000,000-byte limit.

Built at 20:40:28 Malaysia time (12:40:28 UTC) on October 1 with Unity 6000.3.20f1, IL2CPP, ARM64, minimum API 26, target API 36 and default Android ZIP compression. `AndroidSubmission` and `apksigner verify --verbose` passed (v2 signature). SHA-256: `99B19C56933544AC79A2CD597673DE1840D39A5D9378C63EF6B859A739EDC8C1`.

Use `Tools/Build/Build.ps1 -Target AndroidRelease` for development releases. It generates size reports and warns above the **75,000,000-byte working target**. That target remains unmet by **10,957,194 bytes (10.96 MB)**. `-Target AndroidSubmission` rejects APKs of **100,000,000 bytes or larger**; the current submission check passes. `-Target OptimizeAndroid` reapplies mobile import settings and regenerates delivery libraries from their prepared source exports.

The archive contains **59.64 MB** of Unity scene/art/shader data and about **26.32 MB** of runtime/Android overhead. Shared vegetation and material patterns, generated grass, selected render-mesh reductions and mobile compression account for the main saving. Cabin geometry and editable art masters are preserved. See [the measured optimization report and limits](APK-SIZE-AUDIT.md). Continue budgeting during development; no physical phone qualification has been performed.

The jump motion revision measured **87,153,874 → 85,957,194 bytes (−1,196,680 bytes)** across the shared working tree. The reduction is mainly concurrent fishing optimization, not a jump-only saving. The revised clip contributes **37,124 serialized bytes**, approximately **24,231 compressed bytes**, while reusing all character geometry and textures. Grounded arm/leg preparation, takeoff alignment, flight and landing were inspected in actual Windows motion captures. All **126 jump, tackle and turning checks** passed. See [jump authoring and verification](JUMP-ANIMATION.md).

The fishing integration measured **85,088,760 → 85,955,058 bytes (+866,298 bytes)**, including concurrent jump refinements. Fishing art contributes approximately **852,462 compressed bytes** by separate per-asset estimates. Both rods share their materials/maps between two LODs, use 512-pixel colour and 256-pixel normal maps with ASTC 6x6, and Low mesh packing. This replaced a larger 87,153,874-byte first pass. All **22 Windows rod checks** passed and seven actual player views were inspected after optimization. Source GLBs and full-resolution masters remain outside Unity Assets. See [fishing rod analysis and review](FISHING-RODS.md).

The initial jump implementation measured **83,751,052 → 85,088,760 bytes (+1,337,708 bytes)** across the shared working tree, which also gained golf equipment during the task. This is not an isolated jump-code size delta. That clip was **29,292 serialized bytes**, approximately **17,367 compressed bytes**; golf equipment accounts for approximately **1,314,630 compressed bytes**. Separate asset compression estimates do not sum exactly to APK ZIP chunks. Restoring the existing high packing of distant island proxies saved **728,456 APK bytes** compared with the first 85,817,216-byte build, and the generator now preserves that setting. All **126 Windows jump, tackle and turn checks** passed at that checkpoint, including two-player tests; its Android submission build and v2 signature passed. See [jump design and verification](JUMP-ANIMATION.md) and [golf equipment](GOLF-EQUIPMENT.md).

The basketball addition changed the measured APK from **83,282,808 to 83,751,052 bytes (+468,244 bytes / 0.47 MB)**. It ships two compact mesh LODs, one material and 1024×512 ASTC colour/normal maps. The unchanged Meshy GLB, reference image and 2048×1024 authoring maps remain outside Unity Assets. All **53 Windows basketball checks** passed, covering bounce/spin, floor/rim/backboard contacts, scene release/return and host/guest replication. See [the basketball report](BASKETBALL-BALL.md). Pickup, dribbling, player shooting controls, scoring and phone performance are not validated by this foundation.

Before this addition, the 30 September portable-path validation build changed the APK from **83,282,804 to 83,282,808 bytes (+4 bytes)**. The September mobile asset pass had reduced the 350.25 MB cleanup checkpoint by **266.97 MB (76.22%)**. Those are historical checkpoints, not the current APK size.

## Historical baseline — 25 September 2026

The earlier Android release-mode test APK included Sunvale football, Rally basketball, Tidebloom golf and the Rainbow Sprinter animations available at that time. It predates fishing, the later island refinements and Sky-Sail; the table below is historical, not the current file at that path.

| Artifact | Exact size | Decimal MB | MiB |
| --- | ---: | ---: | ---: |
| `Builds/Android/WhatTheFish-release.apk` | 72,690,957 bytes | **72.69 MB** | 69.32 MiB |

Built at 23:16:54 Malaysia time on September 25, 2026 with Unity 6000.3.20f1. Android package: `com.umpsa.whatthefish`, version 0.3.0, ARM64, minimum API 26, target API 36. `apksigner verify --verbose` passed (v2 signature). This non-development player uses the configured test signing key; no phone installation or performance benchmark was performed during this update.

SHA-256: `7045CA6F0BCEE51F811E353EB20F6530A34A36B1782A5C0B9F628A0361A8B444`.

Build command: `Tools/Build/Build.ps1 -Target AndroidRelease`. Windows command: `Tools/Build/Build.ps1 -Target Windows`. The current Windows executable is `Builds/WindowsFinal/WhatTheFish.exe`; its data folder and DLLs must stay together. The Unity build-report total includes intermediate/debug artifacts, so the APK figure above is measured from the actual final `.apk` file.

## Legacy cleanup

`Legacy/` contains archived placeholders, unused Unity materials, Blender automatic backups, the old athlete, obsolete generator scripts, old Windows builds, superseded APKs, and a temporary clone used to verify GitHub LFS downloads. You can delete this entire archive. `Legacy/archive-manifest.json` records the original asset-migration entries; later build archives are grouped under `Legacy/Builds/BeforeBrandRename/`. The archive does not free disk space until it is deleted.

Unity dependency inspection confirmed the old Stadium/BasketballArena FBXs and archived materials are unused by current scenes and prefabs. The basketball logo variants were retained as standalone current mesh/prefab assets, and the scene builder no longer reads the placeholder. Windows and Android rebuilt successfully after archiving. Post-cleanup football/basketball smoke tests and the 126-check golf audit passed.

Current art masters, source textures, Meshy provenance, active code, game assets and current QA evidence remain outside the archive.
