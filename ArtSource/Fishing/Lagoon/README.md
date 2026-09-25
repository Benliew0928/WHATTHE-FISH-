# Lagoon Island — modular fishing environment

Editable assembly: [Island_Lagoon_5P.blend](Island_Lagoon_5P.blend). Built in Blender 5.1 from the approved five-player lagoon concept. This milestone provides environment exploration only: no casting, catches, scoring, timer or bots.

## Modular source and game assets

The assembly contains an `AssetKit_EDITABLE` collection with origin-centered source modules and a `Lagoon_Assembly` collection of placed instances. The kit is hidden in viewport and renders so it does not overlap the placed environment; unhide it when editing a source. Placed copies share source mesh data. Review lights and cameras are in `Review`.

`Modules/*.blend` are appendable Blender collection libraries, one per module. Use File → Append → select the library → Collection → select its named collection. The main assembly is the file to open for an immediate full-island view.

| Module | Reuse / customization boundary |
| --- | --- |
| Island | One replaceable land module containing ten terrain sectors and inlet banks; painted path and beach map |
| PlayerStand | One wooden stand model, instanced five times; independently replaceable and recolorable |
| InletBridge | Separate timber bridge with its own collision and local origin |
| Rock_A / Rock_B / Rock_C | Three reusable sculpted limestone shapes |
| Palm_A / Palm_B | Two reusable palms |
| Flowers_Coral / Flowers_Gold / Flowers_Violet | Three independent planted flower clusters |
| Bush | Reusable shrub cluster |
| Ocean / LagoonWater / Shallows / ShoreFoam | Four independent water and shoreline modules |

Each module has its own FBX in `Game/Assets/_Game/Art/Fishing/Lagoon/`, and its own wrapper prefab in `Game/Assets/_Game/Prefabs/Fishing/Lagoon/`. `LagoonIsland.prefab` is the assembled environment; `Resources/FishingEnvironment.prefab` is the sport environment asset. The source keeps textures packed and exports portable PNGs.

## Coordinates and sockets

Blender uses metres, XY ground and Z height. Unity wrapper roots use XZ ground and Y height. The imported art child is rotated 180 degrees around Unity Y; wrapper transforms and manifest positions are already in Unity coordinates. `FishingBuilder` checks five deck floors to catch axis, scale and placement errors.

The island's origin is lagoon center at sea level. The stand's origin is the center of its shore attachment, on the walking surface. It extends 7 m along local forward and is 6 m wide. The bridge origin is its deck center and it is 13 × 4.2 m. Tree and rock modules use ground-level origins. `lagoon-manifest.json` records placement transforms, module material slots, collision descriptions, five spawn/standing sockets, and shoreline boundaries.

Stand attachment radius is 28 m, with 72-degree spacing. Spawn radius is 32 m; standing radius is 22.5 m. Stands and the bridge are at elevation 1.2 m. The inner path is approximately 4 m wide, at radius 36 m. Procedural outer shoreline is roughly 110–118 m across, with organic variation; dimensions are the implemented result rather than exact copies of perspective concept pixels.

`FishingModule` records `moduleType`, stable `slotId`, and an independent accent color. `SetAccent` uses a material property block so recoloring one stand does not change the other four. Replacing a stand later should preserve its wrapper/socket transform, usable deck extents, collision and slot identity. Island replacements will also need matching spawn, path and shoreline data. This establishes asset boundaries for future customization; no player customization menu or persistence is included yet.

## Rebuild

From the repository root, with Unity closed:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.1\blender.exe' --background --factory-startup --python-exit-code 1 --python Tools/Blender/build_fishing_lagoon.py -- --render
& 'C:\Program Files\Blender Foundation\Blender 5.1\blender.exe' --background --python-exit-code 1 --python Tools/Blender/audit_fishing_lagoon.py
Tools/Build/Build.ps1 -Target Windows
Tools/Build/Test-FishingIsland.ps1
```

The generator owns this fishing source/output family and does not regenerate golf or other sports. Save manually edited variants under different names before regeneration. Blender render references are in `Docs/VisualDirection/Fishing/Blender/`.

## Test the environment

Launch `Builds/WindowsFinal/WhatTheFish.exe`, then **Let's play → Fishing → Explore offline**. Walk the five decks, inner sandy path and inlet bridge using WASD, Shift, right mouse drag and C for camera changes. Escape leaves the lagoon. Existing touch movement and camera controls remain available. The sport defines five room places; other sports retain ten.

Dedicated Windows QA checks the menu entry, module separation, shared deck mesh, five spawn floors, all deck entrances/exits, water boundaries, bridge loop, station colors, and switching among all four sports. Source QA checks modular exports, UVs, materials, packed textures and placement data. Windows validation is not an Android performance certification.
