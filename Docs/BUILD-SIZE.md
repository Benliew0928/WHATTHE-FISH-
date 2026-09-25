# Current build size — 25 September 2026

The final Android release-mode test APK includes Sunvale football, Rally basketball, Tidebloom golf and the current Rainbow Sprinter animations.

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
