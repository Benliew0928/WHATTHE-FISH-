# Basketball milestone — 22 September 2026

This is the historical version 0.2.0 record. Current build locations now contain version 0.3.0; see [GOLF-VERIFICATION.md](GOLF-VERIFICATION.md) for current sizes, checksums and test results.

## Delivered

Version **0.2.0** adds the indoor basketball arena to the existing football prototype. It includes the hardwood NBA-proportioned court, both hoop assemblies, enclosed roof and trusses, light fixtures, seating bowl, aisles, tunnels, benches, scorer's table, scoreboard, shot clocks, banners and boards. Four original center logos and four accent palettes are selected through arena customization and shared by the host.

The editable source is `ArtSource/Basketball/Arena.blend`; regenerate only basketball using `Tools/Blender/build_basketball.py`. The source audit reports **20 merged meshes / 36,050 vertices**. Logo meshes remain separate. The runtime probe observed **43 active renderers**, including the athlete. Football model files were not regenerated.

This is an exploration milestone. There is no ball, shooting, scoring, match timer or spectator simulation. Scoreboards and shot clocks are decorative. Image importing remains deferred; the logo catalog is replaceable and currently contains basketball crest, star, lightning bolt and shield.

## Build and package checks

| Artifact/check | Result |
|---|---|
| Windows development player | Passed build and runtime launch: `Builds/WindowsFinal/WhatTheFish.exe` |
| Windows output folder | 171,703,253 bytes, approximately 171.7 MB uncompressed |
| Android development APK | Passed build for this historical milestone; archived locally |
| APK size | **53,464,235 bytes / 53.46 MB**, below the 100 MB target |
| APK signature | `apksigner verify --verbose` passed, v2 signature |
| Package/version | Earlier development identifier, version name `0.2.0`, development version code `1` |
| Android compatibility | ARM64 only; minimum API 26 / Android 8.0; target API 36 |
| Network compatibility | NGO protocol 2; all participants must use this new build |

APK SHA-256: `5BEE86B9E3B2819CCDC15F99BA5950D17D5550F47D5315211D970907CFBCF325`.

Evidence: `Builds/build-Windows.log`, `Builds/build-Android.log`, `Builds/basketball-apk-signature.txt` and `Builds/basketball-apk-metadata.txt`. Unity's Android build-summary byte count includes intermediate outputs; the APK size above is measured from the actual package file. These are development packages, not store submissions.

## Runtime and visual checks

- Actual Windows-player renders inspected at court level, in all three camera modes, from the roof-facing view and in the customization preview. Court surface overlap and downward-facing logo polygons found during review were corrected.
- All four logos render individually above the hardwood and below the court markings. Arena-name input no longer overlaps its placeholder.
- Ten distinct spawn points have floor support. All four arena boundary directions and the enclosed roof have collision. Third-person camera clamps inside the wall; the local athlete's head hides when camera clearance becomes too small.
- Offline movement measured **4.8 m**. Basketball and football smoke probes both completed without failed assertions. Switching sports leaves exactly one environment active.
- Saving logo 3, exiting, and launching a new process verified persistence. The test restored logo 0 afterwards. Football and basketball retain separate preference keys.

Evidence: `Builds/basketball-release.txt`, `Builds/basketball-release.*.png`, `Builds/football-verified.txt`, `Builds/football-verified.*.png` and `Builds/logo-restart.txt`. The screenshots are rendered from the running Windows player. The displayed desktop FPS is not a phone measurement.

## Multiplayer checks

The local suite passed in `Builds/BasketballQA-Local-20260922-234602/`. The live Unity Relay suite is recorded in `Builds/BasketballQA-Cloud-20260922-235311/`.

Both suites exercise ten-player basketball and a simultaneous two-player football room, including:

- Ten distinct spawns; guests entering from the opposite sport adopt the host's environment and center logo.
- Character appearance, readiness and movement replication; independent room state.
- Eleventh-player rejection, joining during exploration rejection, host start and return to waiting.
- Guest attempts to change the sport or host logo are ignored.
- A new guest after return receives basketball and the host's selected logo.
- Host departure ends the room and returns guests to usable UI.

Movement comparison passed for all ten players and all nine guest views. After movement stopped, maximum reported host/guest position difference was approximately **0.0142 m** in both local and Relay runs. This is a settled-state consistency check, not a measurement of latency or prediction quality.

The first Relay attempt hit HTTP 429 while authenticating clients in a burst. The test harness now spaces cloud-client launches by 1.8 seconds; the subsequent run reached all ten players. No service credentials or cloud settings were changed.

Reproduce with `Tools/Build/Test-BasketballRooms.ps1 -Transport Local` or `-Transport Cloud`, followed by `python Tools/Build/check_basketball_evidence.py <evidence-folder>`. Review each run's `results.txt` and `movement-results.json`.

## Still pending

`adb devices` reports no connected phone. Physical Android installation/launch, touch and multitouch, phone camera checks, sustained 30 FPS, thermal behavior and broader aspect ratios are unverified. Relay processes ran on one PC/connection; different physical devices and internet connections remain unverified. Existing Huawei integration, host migration and reconnect limitations still apply.
