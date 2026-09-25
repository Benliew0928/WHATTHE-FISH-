# Golf island milestone — 23 September 2026

**Historical placeholder report.** The September 25 Tidebloom art replacement is documented in [TIDEBLOOM-VERIFICATION.md](TIDEBLOOM-VERIFICATION.md). The square footprint, centre spawns and flat crossing measurements below describe the archived placeholder, not the current WindowsFinal island.

Version **0.3.0** adds **ISLAND GREENS**, a flat 420 × 420 metre island, to the existing football and basketball environments. All three sports are available for offline exploration and internet rooms. Network protocol **3** requires matching builds.

## Environment and integration

The editable source is `ArtSource/Golf/Island.blend`; the Unity export is `Game/Assets/_Game/Art/GolfIsland.fbx`. Blender 5.1 generated and exported **five meshes / 469 source vertices**. The land has 30 m rounded corners and an 8 m sandy border included in the 420 m dimensions. Static turquoise shallows surround the coast; a 5,000 m sea surface extends into distance fog. There are no golf objects, terrain features, swimming or water simulation.

Unity 6000.3.20f1 imports the source and generates a separate environment prefab with ten centre spawns. Grass and sand have floor collision. **52 overlapping invisible shoreline colliders** use the PlayerBoundary layer, excluded from camera obstruction checks. Golf uses a 1,400 m far clip, fog from 700–1,300 m, and a 19 m elevated gameplay camera. Switching sports restores the existing football and basketball settings and leaves exactly one environment active.

Golf keeps its fixed name and hides stadium customization. Attempts to save golf appearance do not affect either existing sport's preferences. Athlete customization, movement, three camera modes, readiness, room capacity and host controls are shared.

## Measured runtime checks

| Check | Result |
|---|---|
| Width sprint | **59.715 seconds**, 418.003 m between positions just inside the shore |
| Length sprint | **59.724 seconds**, 418.069 m between positions just inside the shore |
| Shoreline containment | Passed 32 outward headings covering every side and rounded corner; six simulated seconds of sustained sprint per heading |
| Spawn support | All ten distinct centre spawns have floor support |
| Camera boundary behavior | Camera can remain over the sea outside the invisible player boundary |
| Environment switching | Football → Basketball → Golf passed; one active root and correct view distance each time |
| Appearance isolation | Fixed island identity retained; football and basketball saved profiles unchanged |
| Offline exploration | Movement and all three camera captures passed |
| Football regression | Offline movement and smoke sequence completed |
| Basketball regression | Spawns, boundaries, roof, four logos, sport switching, camera clearance and offline movement passed |

The sprint crossings use normal frame updates and the real athlete controller at 7 m/s. They are timed runs, not only a distance/speed calculation. The full island footprint is 420 m; the measured runs begin and end approximately one metre inside each shoreline to allow for the character capsule and invisible wall.

Evidence: `Builds/golf-audit.txt`, `Builds/golf-visual.txt`, `Builds/golf-football-regression.txt`, `Builds/golf-basketball-regression.txt`, their corresponding logs and PNG captures. All completed without failed probe assertions or null/missing-reference/index exceptions.

## Windows visual inspection

These screenshots were rendered from the actual Windows development player. The island overview, ground view, all three camera modes and a camera positioned beyond the player boundary were inspected. Sea coverage and distance fog are present; the flat grass/sand surfaces have no coplanar overlap. No surface flicker was observed in the captured views. The fog transition is broad in the elevated overview and narrow near the ground-level horizon. Displayed desktop FPS is not a mobile performance result.

![Island menu overview](Images/Golf/home.png)

![First-person shoreline](Images/Golf/shore-camera0.png)

![Third-person shoreline](Images/Golf/shore-camera1.png)

![Elevated shoreline camera](Images/Golf/shore-camera2.png)

![Camera over the sea looking toward land](Images/Golf/sea-side-camera.png)

## Multiplayer verification

Both the local suite (`Builds/GolfQA-Local-20260923-142151/`) and the live Unity Relay suite (`Builds/GolfQA-Cloud-20260923-142618/`) passed:

- Ten-player golf room, ten distinct spawns, character appearance and readiness replication.
- Guests starting on Basketball adopt Golf and ISLAND GREENS before exploration.
- All ten players move; all nine guest views receive their replicated positions.
- Eleventh-player rejection and rejection of joins while exploration is running.
- Host start and return to waiting; guest sport/appearance changes are ignored.
- A new player joining after return receives Golf and the fixed island name.
- A simultaneous two-player basketball room remains independent; its guest starts on Golf and adopts Basketball.
- Host departure returns guests in both rooms to usable menus.

The settled host/guest position comparison passed with a maximum difference of approximately **0.0174 m** in both local and Relay runs. This compares stopped players after return, and does not measure network latency. Each run contains `results.txt` and `movement-results.json`. Cloud client launches are spaced by 1.8 seconds to avoid authentication bursts. No cloud credentials or service settings were changed.

Reproduce using `Tools/Build/Test-GolfRooms.ps1 -Transport Local` or `-Transport Cloud`, then `python Tools/Build/check_basketball_evidence.py <evidence-folder>`. The existing movement checker is shared between basketball and golf.

## Development packages

| Artifact/check | Result |
|---|---|
| Windows player | `Builds/WindowsFinal/SportsPrototype.exe`; built and launched successfully |
| Windows output folder | 171,788,695 bytes, approximately 171.79 MB uncompressed |
| Android APK | `Builds/Android/SportsPrototype.apk`; built successfully |
| Actual APK size | **53,550,903 bytes / 53.55 MB**, below 100 MB |
| APK signature | `apksigner verify --verbose` passed, v2 signature, one signer |
| Package | `com.umpsa.sportsprototype` |
| Version | `0.3.0`, development version code `1` |
| Android ABI | `arm64-v8a` |
| Android compatibility | Minimum API 26 / Android 8.0; target API 36 |
| Network protocol | 3; use the same version on every player |

APK SHA-256: `F8B51B676A0CA8DE58849E394193C8AEDEB65EE6EA88B1914F5C4D399C15285B`.

Build and package evidence: `Builds/golf-blender.log`, `Builds/golf-asset-audit.json`, `Builds/build-GolfFinal.log`, `Builds/build-Android.log`, `Builds/golf-apk-signature.txt`, and `Builds/golf-apk-metadata.txt`. Both Unity logs report successful builds. Android's Unity summary includes intermediate outputs; the size above is measured from the APK itself. These are development packages; no store publishing was performed.

## Pending device checks

`adb devices` reported no connected Android device. Physical installation and launch, touch/multitouch controls, phone camera behavior, sustained mobile performance, thermals and broader screen sizes remain pending. Relay verification used multiple processes on one PC and internet connection; separate-device and separate-network testing remains pending. Existing host migration and reconnect limitations remain unchanged.
