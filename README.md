# WHATTHE FISH?

WHATTHE FISH? is an Android-first game with football, basketball, golf, and fishing environments.

Open `Game` in Unity **6000.3.20f1**. Open `Assets/_Game/Scenes/Bootstrap.unity`, then press Play. The football stadium, indoor basketball arena and golf island are Blender assets. The shared athlete is the approved Meshy 6 Lite Rainbow Sprinter, prepared in Blender. There are no spectator NPCs.

## Where things live

- `Game/` — Unity URP project, runtime code, materials, prefabs, scenes and package locks.
- `ArtSource/Football/Sunvale/Stadium_Sunvale.blend` — current modular football stadium, painted atlas, review cameras and lighting. The placeholder source is archived under `Legacy/ArtSource/Football/`.
- `ArtSource/Shared/Characters/RainbowSprinter.blend` — editable character master with original mesh, two game LODs, rig and supplied run.
- `ArtSource/Shared/Characters/Meshy/RainbowSprinter-source.zip` — preserved Meshy download and texture provenance.
- `ArtSource/Basketball/Rally/Arena_Rally.blend` — current modular basketball arena, textured court, rounded hoops, colorful seating, gallery, banners and lighting. The placeholder source is archived under `Legacy/ArtSource/Basketball/`.
- `ArtSource/Golf/Tidebloom/Island_Tidebloom.blend` — sculpted tropical golf island with striped fairway, five sand bowls, palms, flower gardens, sea arch and waterfall. The placeholder source is archived under `Legacy/ArtSource/Golf/`.
- `Tools/Blender/` — deterministic asset generator and audit tool.
- `Tools/Build/` — build/environment scripts.
- `Builds/WindowsFinal/` — latest development Windows player (launch `WhatTheFish.exe`), including the Sunvale football stadium, Rally basketball arena and Tidebloom golf island. `Tools/Build/Build.ps1 -Target Windows` refreshes the playable scene and replaces this build; `LATEST-BUILD.txt` records its build time. Superseded Windows players are archived under `Legacy/Builds/`.
- `Legacy/` — removable archive of old placeholders, unused materials, backups and superseded builds; see its archive manifest.
- `Builds/Android/WhatTheFish-release.apk` — current Android test APK, 72.69 MB, ARM64 / Android 8.0+, including all three refined environments.
- `Docs/SETUP.md` — toolchain, cloud linking and running on a phone.
- `Docs/ARCHITECTURE.md` — code boundaries and future gameplay.
- `Docs/VERIFICATION.md` — measured checks and remaining access requirements.
- `Docs/BUILD-SIZE.md` — measured package sizes and the asset-size budget for future work.
- `Docs/VisualDirection/RAINBOW-SPRINTER-IMPLEMENTATION.md` — current character asset, runtime behavior and verification.

## Controls

Fishing environment: launch `Builds/WindowsFinal/WhatTheFish.exe` and choose **Let's play → Fishing → Explore offline**, or use `Builds/WindowsFinal/Explore-Fishing.cmd`. The lagoon has five separate wooden stands, a connected sandy loop and an inlet bridge. This is environment exploration only. The [modular Blender source guide](ArtSource/Fishing/Lagoon/README.md) describes the 16 separate asset modules, source libraries and Unity prefabs prepared for future customization.

Landscape: left thumb stick moves; push it fully to run. Drag on the right to look. Tap **Camera** to cycle first person, third person and elevated views. On Windows: WASD, Shift, right mouse drag, C. Escape returns to the menu/waiting room. A guest leaving an environment leaves the room; the host can return everyone to the waiting room.

The room system uses Unity Multiplayer Services sessions, Relay, Unity Transport and Netcode for GameObjects. Offline exploration requires no Unity account or internet connection. Online rooms require your cloud project to be linked and services enabled.

The linked Unity cloud project supports internet rooms through Relay; create/join has been tested. Read `Docs/VERIFICATION.md` for the test coverage and phone checks still pending. Launch the Windows executable normally, without diagnostic arguments, for interactive play.

This milestone is exploration, character art and networking infrastructure. The Rainbow Sprinter is one shared look for now; character appearance controls are paused until the model has separate, swappable parts. Football and basketball ball physics, scoring, bots, and golf gameplay are future work. Choose Basketball → Explore offline to walk the court. Arena customization provides four center logos, four accent palettes, a name, and a reset control. Hosts share their selection with guests; custom-image importing is deferred.

Choose Golf → Explore offline to visit ISLAND GREENS. Sprint shore to shore in approximately one minute in either direction. Invisible shoreline boundaries keep players on the island. Golf environment customization and golf gameplay are deferred.

For the current Windows basketball replacement, see `Docs/RALLY-INTEGRATION.md`. Golf milestone results remain in `Docs/GOLF-VERIFICATION.md`; the prior basketball milestone is recorded in `Docs/BASKETBALL-VERIFICATION.md`.
