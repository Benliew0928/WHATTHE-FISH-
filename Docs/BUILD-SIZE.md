# Current build size — 25 September 2026

The final Android release-mode test APK includes Sunvale football, Rally basketball, Tidebloom golf and the current Rainbow Sprinter animations.

| Artifact | Exact size | Decimal MB | MiB |
| --- | ---: | ---: | ---: |
| `Builds/Android/SportsPrototype-release.apk` | 72,692,133 bytes | **72.69 MB** | 69.32 MiB |

Built at 20:30:13 Malaysia time on September 25, 2026 with Unity 6000.3.20f1. Android package: `com.umpsa.sportsprototype`, version 0.3.0, ARM64, minimum API 26, target API 36. `apksigner verify --verbose` passed (v2 signature). This non-development player uses the configured test signing key; no phone installation or performance benchmark was performed during this update.

SHA-256: `7717A7BEDF0E75282BD1CD0499F2B6012C22B739F514F4721F23B6324F95AF67`.

Build command: `Tools/Build/Build.ps1 -Target AndroidRelease`. Windows command: `Tools/Build/Build.ps1 -Target Windows`. The current Windows executable is `Builds/WindowsFinal/SportsPrototype.exe`; its data folder and DLLs must stay together. The Unity build-report total includes intermediate/debug artifacts, so the APK figure above is measured from the actual final `.apk` file.

## Legacy cleanup

`C:/UMPSA/Legacy` contains 939.04 MB of archived placeholders, unused Unity materials, Blender automatic backups, the old athlete, obsolete generator scripts, old Windows builds and superseded APKs. You can delete this entire archive. `Legacy/archive-manifest.json` records original paths and sizes. This move does not free disk space until the archive is deleted.

Unity dependency inspection confirmed the old Stadium/BasketballArena FBXs and archived materials are unused by current scenes and prefabs. The basketball logo variants were retained as standalone current mesh/prefab assets, and the scene builder no longer reads the placeholder. Windows and Android rebuilt successfully after archiving. Post-cleanup football/basketball smoke tests and the 126-check golf audit passed.

Current art masters, source textures, Meshy provenance, active code, game assets and current QA evidence remain outside the archive.
