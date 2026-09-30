# Current build size — 30 September 2026

The current Android release APK contains all four islands, Sky-Sail and the existing character animations. Its exact size is **83,282,808 bytes (83.28 MB / 79.42 MiB)**, leaving **16.72 MB** below the hackathon's 100,000,000-byte limit. The mobile asset pass reduced the 350.25 MB cleanup checkpoint by **266.97 MB (76.22%)**.

Built at 09:49 UTC on September 30 with Unity 6000.3.20f1, IL2CPP, ARM64, minimum API 26, target API 36 and default Android ZIP compression. `apksigner verify --verbose` passed (v2 signature). SHA-256: `2196D57FEB20EB3164C628E4FE11C79EA96C38A9A9B04EBDD0B01B412568C700`.

Use `Tools/Build/Build.ps1 -Target AndroidRelease` for development releases. It generates size reports and warns above the **75,000,000-byte working target**. That target remains unmet by 8.28 MB. `-Target AndroidSubmission` rejects APKs of **100,000,000 bytes or larger**; the current submission check passes. `-Target OptimizeAndroid` reapplies mobile import settings and regenerates delivery libraries from their prepared source exports.

The archive contains 56.98 MB of Unity scene/art/shader data and about 26.30 MB of runtime/Android overhead. Shared vegetation and material patterns, generated grass, selected render-mesh reductions and mobile compression account for the main saving. Cabin geometry and editable art masters are preserved. See [the measured optimization report and limits](APK-SIZE-AUDIT.md). Continue budgeting during development; no physical phone qualification has been performed.

The 30 September portable-path validation build changed the APK from **83,282,804 to 83,282,808 bytes (+4 bytes)**. Blender texture references and the affected FBX path properties now resolve from each asset location; geometry, UVs, transforms, material bindings and texture content were preserved. `AndroidSubmission` and APK v2 signature verification passed. Windows gameplay and physical-phone checks were not rerun for this housekeeping task.

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
