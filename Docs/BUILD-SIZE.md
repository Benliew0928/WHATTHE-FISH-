# Build size — merged working tree, 2 October 2026

The GitHub basketball/jump updates and local football changes have been combined. No fresh APK exists for this combined source. The following sizes are historical records from separate working trees and must not be used as validation of the merged build. The last local APK measured 83,787,566 bytes and was archived before this integration; the upstream record reports 85,970,822 bytes. Neither is a measured size for the combined implementation.

The prescribed `Tools/Build/Build.ps1 -Target AndroidSubmission` was attempted after combined Windows feature checks. It failed at Gradle with `java.io.IOException: Unable to establish loopback connection`; no new APK was produced, so combined size/headroom remains unverified. Failure evidence: `Builds/build-AndroidSubmission.log` and `Builds/SizeAudit/latest/build.log`; the accompanying report is from the failed build and is not a successful size audit. The combined Windows player passed 289 checks, with per-suite evidence listed in [FOOTBALL-PROTOTYPE.md](FOOTBALL-PROTOTYPE.md).

## Upstream release record — 1 October 2026

The current Android release APK contains all four islands, Sky-Sail, the steerable character jump, basketball pickup/carry/shoot gameplay, golf equipment and two fishing rods. Its exact size is **85,970,822 bytes (85.97 MB / 81.99 MiB)**, leaving **14,029,178 bytes (14.03 MB)** to the hackathon's strict 100,000,000-byte limit.

Built at 22:08:35 Malaysia time (14:08:35 UTC) on October 1 with Unity 6000.3.20f1, IL2CPP, ARM64, minimum API 26, target API 36 and default Android ZIP compression. `AndroidSubmission` and `apksigner verify --verbose` passed (v2 signature). SHA-256: `CCE5D8AE9AF66ADE9E3DDAAC3BA7AF7D7C9FB30204A9ED397539719B3DE381CC`.

Use `Tools/Build/Build.ps1 -Target AndroidRelease` for development releases. It generates size reports and warns above the **75,000,000-byte working target**. That target remains unmet by **10,970,822 bytes (10.97 MB)**. `-Target AndroidSubmission` rejects APKs of **100,000,000 bytes or larger**; the current submission check passes. `-Target OptimizeAndroid` reapplies mobile import settings and regenerates delivery libraries from their prepared source exports.

The archive contains about **59.64 MB** of Unity scene/art/shader data and **26.33 MB** of runtime/Android overhead. Shared vegetation and material patterns, generated grass, selected render-mesh reductions and mobile compression account for the main saving. Cabin geometry and editable art masters are preserved. See [the measured optimization report and limits](APK-SIZE-AUDIT.md). Continue budgeting during development; no physical phone qualification has been performed.

The basketball gameplay revision measured **85,957,194 to 85,970,822 bytes (+13,628 bytes)**. It reuses the ball, hoops and character art, adding proximity possession, assisted shooting, recovery and protocol-12 synchronization. The final Windows gameplay suite passed **82 checks**, with **53 foundation physics/replication checks** and **40 jump regression checks** also passing. A copied-player fixture in a path with spaces passed all 61 offline gameplay checks from an unrelated working directory. See [controls, tuning and current evidence](BASKETBALL-GAMEPLAY.md).

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

## Historical local football releases

The historical local charge-kick AndroidSubmission APK is **83,787,566 bytes**, compared with **83,786,982 bytes** before this task (**+584 bytes**). It leaves **16,212,434 bytes** below the strict 100,000,000-byte limit and exceeds the 75,000,000-byte development target by **8,787,566 bytes**. Built on 1 October at 15:03 UTC; release IL2CPP ARM64. SHA-256: `B6D7794AF24D0A4ECD8B7E8B0CD5DBF1A5BE53181EC7FBC282D34A9487F916E6`. Submission size gate and APK v2 signature verification passed. Archived artifact: `Legacy/20261002-002911-200-football-balance-apk-baseline/Builds/Android/WhatTheFish-release.apk`.

Editor and rebuilt Windows each passed 75 actual-pitch checks, and isolated host/client checks verified charge transmission. Generated IL2CPP contains the charge-aware kick methods. The batch build failed at Gradle loopback initialization; the same submission method succeeded in the editor after desktop unlock. Current evidence: `Builds/FootballPrototype/` and `Builds/SizeAudit/latest/`. No physical-phone or internet-latency qualification was performed.

The incremental build omitted packing offsets. The audit uses actual APK entries and baseline offsets only for **42 streams verified byte-identical by SHA-256**, omitting three changed streams. Largest verified art contributors remain unchanged (timber 2.25 MB, character albedo 1.99 MB, golf structures 1.98 MB, character mesh 1.57 MB, football stadium 1.56 MB estimates). No models/textures were added, and verified shared art streams retain their bytes. Mobile pipeline assignments and pruned editor resources were restored/preserved. Previous APK and audit: `Legacy/20261001-225440-511-charge-kick-apk-baseline/`; see [feature verification and archives](FOOTBALL-PROTOTYPE.md).

## Previous goal-entry release
The current no-climb, field-boundary and both-goal-entry AndroidSubmission APK is **83,786,982 bytes (83.79 decimal MB)**, **7,704 bytes larger** than the preceding measured 83,779,278-byte player-interception build. It leaves **16,213,018 bytes** below the strict 100,000,000-byte limit and remains **8,786,982 bytes above** the 75,000,000-byte development target. No current APK existed before this follow-up; the comparison is against the preserved prior measured APK.

Built on 1 October at 14:13 UTC with Unity 6000.3.20f1, release IL2CPP ARM64. SHA-256: `AAD7E0BC691678B215C7F30AC2053C4008B7F6DF9C3826EDCB2608C60E3CCA85`. The submission size gate and APK v2 signature verification passed. Artifact: `Builds/Android/WhatTheFish-release.apk`. Generated IL2CPP contains the goal-area implementation. Editor and rebuilt Windows each passed 63 actual-pitch checks; the isolated loopback rerun passed host and guest checks including both goals. One earlier guest stopped-ball timing check failed during concurrent compilation and is preserved; see [verification and archives](FOOTBALL-PROTOTYPE.md). No phone or internet-latency qualification was performed.

The prescribed batch build failed at Gradle loopback initialization; the same submission method succeeded in the native editor. Its incremental build reused player data and omitted packing offsets. The current audit therefore includes actual APK entries plus a clearly marked partial asset estimate: baseline offsets are reused only for **43 streams whose SHA-256 matches the new APK**, omitting two changed streams. The largest verified art contributors are timber texture (about 2.25 MB ZIP estimate), character albedo (1.99 MB), golf structures (1.98 MB), character mesh (1.57 MB) and football stadium (1.56 MB). Shared asset streams are byte-identical to the baseline; no new art or avoidable art growth was introduced. Current evidence is in `Builds/SizeAudit/latest/`. The mobile pipeline and pruned editor resource references were preserved.

## Previous player-interception release
The player-interception AndroidSubmission APK is **83,779,278 bytes (83.78 MB / 79.90 MiB)**, **80 bytes smaller** than the preceding 83,779,358-byte physics prototype. It leaves **16,220,722 bytes** below the strict 100,000,000-byte limit and remains **8,779,278 bytes above** the 75,000,000-byte development target.

SHA-256: `D8012E04B9E954D3C2EDF69AD4F7C6C9F7D89844ABD9C97557295B98F5FE6E92`. Built on 1 October at 10:17 UTC with Unity 6000.3.20f1, release IL2CPP ARM64, API 26 minimum / API 36 target. The submission size gate and APK v2 signature verification passed. Historical artifact (now archived): `Legacy/20261001-183311-169-no-climb-apk/Builds/Android/WhatTheFish-release.apk`. Generated Android IL2CPP output from that build contains the interception methods. Editor and rebuilt Windows checks passed, including the kicker overtaking their ball and remote-player interception; see [verification and limits](FOOTBALL-PROTOTYPE.md).

The prescribed script exited because Unity had the project open; the same `ProjectBuilder.BuildAndroidSubmission` succeeded in that editor. After an interrupted Windows preview, an intermediate build retained the desktop pipeline and measured 86,254,408 bytes. Restoring the authored `MobileURP` assignment removed that avoidable growth. The final populated packing report contains no desktop SSAO blue-noise textures or shader; the largest art contributors remain unchanged. Current audit, build log and actual ZIP-entry estimates are in `Builds/SizeAudit/latest/`. Previous and intermediate evidence was archived as recorded in the prototype guide. No art-quality reductions, phone qualification or Git publishing were performed.

## Previous basic physics prototype — 1 October 2026

The basic football physics prototype AndroidSubmission APK is **83,779,358 bytes (83.78 MB / 79.90 MiB)**, an increase of **12,420 bytes** from the same-session 83,766,938-byte static-ball baseline. It leaves **16,220,642 bytes** below the strict 100,000,000-byte limit and remains **8,779,358 bytes above** the 75,000,000-byte development target.

SHA-256: `AB2382BC2BBF0308CD63B3B24D0276F51A101AF747B5CB55143D101841E3F263`. Built on 1 October at 09:23 UTC with Unity 6000.3.20f1, release IL2CPP ARM64, API 26 minimum / API 36 target. The submission size gate and APK v2 signature verification passed. Artifact: `Builds/Android/WhatTheFish-release.apk`. Actual-pitch physics and final Windows offline/two-player loopback tests passed; no physical-phone or internet-room qualification was performed. See [football prototype verification](FOOTBALL-PROTOTYPE.md).

The prescribed build script exited because the editor had the project open. The same `ProjectBuilder.BuildAndroidSubmission` ran in that editor. Its first APK measured 84,317,854 bytes and unexpectedly retained desktop SSAO resources after Windows testing. Clearing URP's platform build-data cache reduced it by **538,496 bytes** to the final size above. The final log selects `MobileURP`; its populated packing report contains no SSAO blue-noise textures/shader. The mesh remains **550,636 serialized bytes / 473,398 estimated ZIP bytes**. The largest art contributors remain shared timber, character colour, golf structures, character mesh and football stadium geometry. Source geometry, textures and visual quality settings were not reduced for this prototype.

Current successful build log, detailed packing report, actual APK ZIP-entry sizes and per-asset estimates are in `Builds/SizeAudit/latest/`; functional and signature evidence is in `Builds/FootballPrototype/`. Static baseline and intermediate build/audit artifacts were archived in the batches recorded in the prototype guide. The transient editor build helper was removed after completion. Git publishing remains with the user.

## Previous static-ball baseline — 1 October 2026

The Android submission APK including the doubled, 44 cm centre-spot football is **83,766,938 bytes (83.77 MB / 79.89 MiB)**. It leaves **16,233,062 bytes** below the strict 100,000,000-byte limit and exceeds the 75,000,000-byte development target by **8,766,938 bytes**. Continue preserving that shared reserve.

SHA-256: `B1300F631C9B10D5B09675A1B2B9C2E35FF3B624C8D856B55C76059CC366634D`. Built on 1 October at 07:42 UTC with Unity 6000.3.20f1, release IL2CPP ARM64, API 26 minimum / API 36 target. AndroidSubmission's hard size gate and APK v2 signature verification passed. Artifact: `Builds/Android/WhatTheFish-release.apk`.

The resize changed the measured APK from **83,765,822 to 83,766,938 bytes (+1,116 bytes)**. The mesh and two materials retain their packed sizes; scaling adds no new texture or geometry asset. Against the documented 30 September APK the increase is **484,130 bytes**; that older APK was absent at the initial integration. See [football placement and verification](SOCCER-BALL.md) for source audits, preserved prior builds and limits. No physical-phone test was performed.

The initial 22 cm integration's `Tools/Build/Build.ps1 -Target AndroidSubmission` reached Gradle but failed twice at Java loopback initialization in the command runner, including a process-local IPv4 retry. Its editor build passed at 83,765,822 bytes (SHA-256 `069841C279C05282D8FA98B5DB7D2E10573D77AF8C1752C661D9F561B8B846FF`). For this resize the script was attempted while the project was open and exited with code 1; the same `ProjectBuilder.BuildAndroidSubmission` method succeeded in the existing editor. Current successful build evidence, populated packing report and ZIP-entry sizes are in `Builds/SizeAudit/latest/`.
