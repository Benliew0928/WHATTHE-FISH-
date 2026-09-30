# WHATTHE FISH?

WHATTHE FISH? is an Android-first game with football, basketball, golf, and fishing environments.

AI contributors must read [AGENTS.md](AGENTS.md) for the **strictly under-100-MB APK requirement**, the 75 MB working target, visual-quality priorities and end-of-task cleanup. Before committing, run `Tools/Build/Check-TaskReady.ps1`; use `-RequireApk` when delivering an Android build. [Repository housekeeping](Docs/REPOSITORY-HYGIENE.md) explains what belongs in Git and how to recover archived files. The user stages, commits and pushes changes.

The official game logo is the **Pocket Island** design. See the [official logo and usage note](Docs/Branding/README.md).

The four islands now share the **Sky-Sail Circuit**: rideable group cabins, separate cable/tower/station modules, and distant 3D island views. Launch `Builds/WindowsFinal/Explore-SkySail.cmd` to start at a station. One host room travels together to one active island. See the [implementation guide](Docs/VisualDirection/WorldConnections/SKY-SAIL-IMPLEMENTATION.md) and [actual game gallery](Docs/VisualDirection/WorldConnections/GameReview/index.html).

The Windows player includes the selected F1/B1 **walkable coastal stadiums**: detailed cream-stone entrances, teal and timber trim, sea islands, connected spectator stairs and an open-air basketball arena. See the [actual EXE review gallery](Docs/VisualDirection/CoastalStadiums/index.html) and [implementation and validation notes](Docs/COASTAL-STADIUMS.md).

Golf and fishing now use **G2 Limestone Cove Links** and **L2 Limestone Garden Lagoon**, with detailed coastal materials, vegetation, facilities and walking routes. See the [27-view Windows gallery](Docs/VisualDirection/GolfFishingRefinement/GameReview/index.html) and [delivery evidence](Docs/VisualDirection/GolfFishingRefinement/GameReview/VALIDATION.md).

Open `Game` in Unity **6000.3.20f1**. Open `Assets/_Game/Scenes/Bootstrap.unity`, then press Play. The football stadium, open-air basketball arena and golf island are Blender assets. The shared athlete is the approved Meshy 6 Lite Rainbow Sprinter, prepared in Blender. There are no spectator NPCs.

For a fresh clone, install Git LFS and run `git lfs pull` before opening the Unity project so Blender masters, FBX models and textures are available. `Builds/` and `Legacy/` are local folders excluded from Git; teammates receive the Unity project and editable resources, then build their own Windows player with `Tools/Build/Build.ps1 -Target Windows`.

## Where things live

- `Game/` — Unity URP project, runtime code, materials, prefabs, scenes and package locks.
- `ArtSource/Football/Sunvale/Stadium_Sunvale.blend` — current modular football stadium, painted atlas, review cameras and lighting. The placeholder source is archived under `Legacy/ArtSource/Football/`.
- `ArtSource/Shared/Characters/RainbowSprinter.blend` — editable character master with original mesh, two game LODs, rig and supplied run.
- `ArtSource/Shared/Characters/Meshy/RainbowSprinter-source.zip` — preserved Meshy download and texture provenance.
- `ArtSource/Basketball/Rally/Arena_Rally.blend` — current modular basketball arena, textured court, rounded hoops, colorful seating, gallery, banners and lighting. The placeholder source is archived under `Legacy/ArtSource/Basketball/`.
- `ArtSource/RefinedIslands/` — editable G2 golf and L2 fishing masters, shared asset kit and measured layouts. Superseded golf/fishing sources are preserved in the dated `Legacy/GolfFishing-G2L2-*` batch.
- `ArtSource/SkySail/` — editable cabin, interior, doors, hanger, tower, station and gangway masters. Matching Unity prefabs are in `Game/Assets/_Game/Prefabs/SkySail/`.
- `Tools/Blender/` — deterministic asset generator and audit tool.
- `Tools/Build/` — build/environment scripts.
- `Builds/WindowsFinal/` — latest Windows review player (launch `WhatTheFish.exe`), including the approved coastal football/basketball venues and G2/L2 islands. `Tools/Build/Build.ps1 -Target Windows` refreshes the playable scene and replaces this build; `LATEST-BUILD.txt` records its build time. Superseded Windows players are archived under `Legacy/Builds/`.
- `Legacy/` — removable archive of old placeholders, unused materials, backups and superseded builds; see its archive manifest.
- `Builds/Android/WhatTheFish-release.apk` — optimized Android release APK built 30 September 2026, **83.28 MB**, ARM64 / Android 8.0+, including all four islands and Sky-Sail. It leaves **16.72 MB** below the hackathon limit; the 75 MB working target remains unmet. See [the optimization report](Docs/APK-SIZE-AUDIT.md). Physical-phone qualification is pending.
- `Builds/WindowsMobilePreview/` — preview of mobile delivery geometry, built with `Tools/Build/Build.ps1 -Target MobilePreview`. Its desktop renderer and texture formats do not substitute for phone testing.
- `Docs/SETUP.md` — toolchain, cloud linking and running on a phone.
- `Docs/ARCHITECTURE.md` — code boundaries and future gameplay.
- `Docs/VERIFICATION.md` — measured checks and remaining access requirements.
- `Docs/BUILD-SIZE.md` — measured package sizes and the asset-size budget for future work.
- `Docs/VisualDirection/RAINBOW-SPRINTER-IMPLEMENTATION.md` — current character asset, runtime behavior and verification.
- `Docs/Branding/README.md` — official WHATTHE FISH? logo and naming guidance.

## Controls

Fishing environment: launch `Builds/WindowsFinal/WhatTheFish.exe` and choose **Let's play → Fishing → Explore offline**, or use `Builds/WindowsFinal/Explore-Fishing.cmd`. Golf has its own `Explore-Golf.cmd` launcher. The lagoon has five separate coloured decks, a connected shore circuit, inlet bridge and open shelter. This is environment exploration only. The [Blender source guide](ArtSource/RefinedIslands/README.md) describes the current editable masters, shared modules, layouts and Unity assets.

Landscape: left thumb stick moves; push it fully to run. Drag on the right to look. Tap **Camera** to cycle first person, third person and elevated views. On Windows: WASD, Shift, right mouse drag, C. Escape returns to the menu/waiting room. A guest leaving an environment leaves the room; the host can return everyone to the waiting room.

The room system uses Unity Multiplayer Services sessions, Relay, Unity Transport and Netcode for GameObjects. Offline exploration requires no Unity account or internet connection. Online rooms require your cloud project to be linked and services enabled.

The linked Unity cloud project supports internet rooms through Relay; create/join has been tested. Read `Docs/VERIFICATION.md` for the test coverage and phone checks still pending. Launch the Windows executable normally, without diagnostic arguments, for interactive play.

This milestone is exploration, character art and networking infrastructure. The Rainbow Sprinter is one shared look for now; character appearance controls are paused until the model has separate, swappable parts. Football and basketball ball physics, scoring, bots, and golf gameplay are future work. Choose Basketball → Explore offline to walk the court. Arena customization provides four center logos, four accent palettes, a name, and a reset control. Hosts share their selection with guests; custom-image importing is deferred.

Choose Golf → Explore offline to visit ISLAND GREENS. Sprint shore to shore in approximately one minute in either direction. Invisible shoreline boundaries keep players on the island. Golf environment customization and golf gameplay are deferred.

For the current Windows coastal redesign, see `Docs/COASTAL-STADIUMS.md`. `Docs/RALLY-INTEGRATION.md` records the earlier indoor-arena milestone. Golf milestone results remain in `Docs/GOLF-VERIFICATION.md`; the prior basketball milestone is recorded in `Docs/BASKETBALL-VERIFICATION.md`.
