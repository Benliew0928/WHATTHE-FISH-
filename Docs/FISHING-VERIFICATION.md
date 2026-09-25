# Fishing lagoon environment — verification

Verified 26 September 2026 in the Windows development player. Environment exploration only; no fishing game logic or player-facing customization editor was added.

## Delivered assets

- Editable Blender assembly: `ArtSource/Fishing/Lagoon/Island_Lagoon_5P.blend`.
- Sixteen appendable Blender module libraries in `ArtSource/Fishing/Lagoon/Modules/`.
- Sixteen independent FBX modules, portable textures and import manifest in `Game/Assets/_Game/Art/Fishing/Lagoon/`.
- Sixteen module prefabs and the assembled `LagoonIsland.prefab` in `Game/Assets/_Game/Prefabs/Fishing/Lagoon/`.
- The Island module contains ten terrain sectors and two closed inlet banks. Five PlayerStand instances share a mesh, retain independent accent colors and expose stable slot IDs. Bridge, palms, rocks, flowers, bush and water are separate modules.
- The generated kit contains 44,512 unique source triangles and 314,572 triangles across placed instances before foliage distance culling. Counts are source geometry, not measured draw calls or an Android performance claim.

## Checks passed

`Tools/Blender/audit_fishing_lagoon.py` reopens the saved master and verifies all module exports/libraries, finite vertices, UV0, assigned materials, packed textures, five equal-radius stand attachments, five distinct accents, five spawn/standing anchors and one bridge. Evidence: `Builds/fishing-blender-audit.txt`.

`Tools/Build/Build.ps1 -Target Windows` completed successfully with Unity 6000.3.20f1. The import builder additionally checked the five deck floors after Blender-to-Unity axis conversion. Evidence: `Builds/build-Windows.log` and `Builds/fishing-unity-import-audit.txt`.

`Tools/Build/Test-FishingIsland.ps1` passed in the executable. It verifies Fishing appears in the menu, one active environment, five spawns/capacity, independent stand modules with shared mesh data, all 16 module types, supported shaders, all five spawn floors, deck entry/exit/front containment, both side edges of every deck, a full walking circuit through the inlet bridge, inner shoreline/inlet containment, independent accent colors, all four sport switches and offline operation. Evidence: `Builds/FishingQA-20260926-023214/`.

`Tools/Build/Test-FishingRooms.ps1` passed with local transport: one host and four guests, distinct spawns, host-selected Fishing received by guests initially on Golf, sixth-player rejection at five places, host start and return, guest permission boundaries, and moving explorers staying above water. Evidence: `Builds/FishingRoomsQA-20260926-023214/`. This did not contact Relay or certify internet/Android operation. Network protocol was incremented to 7 so older builds without Fishing are not compatible room peers.

Visual review covered the actual Blender renders and Windows screenshots of the whole island, third-person stand, bridge and sport menu. Water fog is evaluated per pixel to avoid a color seam between the large ocean plane and near-shore modules. Deck boundaries trace rectangular corners, including the sideways approach where the initial radial approximation admitted a strip of water.

## Run it

Launch [WhatTheFish.exe](../Builds/WindowsFinal/WhatTheFish.exe), then **Let's play → Fishing → Explore offline**. [Explore-Fishing.cmd](../Builds/WindowsFinal/Explore-Fishing.cmd) opens directly in the lagoon. WASD moves, Shift sprints, right mouse drag looks, C changes camera, and Escape leaves.

[Windows overview](VisualDirection/Fishing/Blender/04_Windows_Overview.png) · [Player stand](VisualDirection/Fishing/Blender/05_Windows_Player_Stand.png) · [Bridge](VisualDirection/Fishing/Blender/06_Windows_Bridge.png) · [Modular source guide](../ArtSource/Fishing/Lagoon/README.md).
