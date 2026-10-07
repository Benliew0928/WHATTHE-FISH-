# F1/B1 coastal stadiums — Windows

The Windows player at `Builds/WindowsFinal/WhatTheFish.exe` contains the walkable coastal redesign. The [review gallery](VisualDirection/CoastalStadiums/index.html) separates actual executable captures and walkthroughs from the six imagegen concepts and saved prompts.

## Delivered design

Football retains the 68 × 105 m pitch, goals, 3,072 seats and 32 seating LOD groups. Its exterior now has cream stone/plaster arcades, finished entrance tunnels, timber soffits, teal rails, bronze fixings, lanterns, signs and planted terraces. Four entrances connect to the island; four exterior stairs join the upper concourse. Small changes to aisle openings, railings and support positions provide controller clearance without changing the seating plan.

Basketball retains its 15.24 × 28.6512 m court, hoops at ±12.7254 m and 3.048 m rim height. The ceiling, suspended scoreboard and overhead lighting rig are removed. Cream arcades, teal coping, warmer structural finishes and four supported perimeter scoreboard panels frame an unobstructed central sky. Forty lower-tier seats were removed to open four passages; 1,256 seats and all 48 seating LOD groups remain. Four logos, palette indices, saved names, reset and host-shared appearance use the existing customization system.

The existing F1/B1 island footprints are preserved. Sculpted clouds, animated sea, textured terrain, palms, gardens, docks and bridges remain. Matched ground/structure collision and visible approach ramps join the arrival dock, island paths, entrances, playing area, stairs and concourses. The current controller and server-authoritative movement are retained. Falling below the sea threshold returns an athlete to that environment's entrance. Swimming, NPC crowds and match gameplay are outside this pass.

## Close-view assets

Ten shared 2048² base-color, normal and metallic/smoothness sets cover stone, plaster, paving, timber, enamel, metal and decorative details. New architecture uses one tile per four metres: approximately 512 pixels/metre before mip selection. Beveled masonry, arch joints, thick trim, stair nosings, railing connections, bolts and planter foliage are modeled. Existing 4K island terrain maps and 2K court wood are retained.

Shared materials enable instancing; architecture is combined by bay/material, and existing seat LODs remain active. The architecture source audit counts about 502k football and 296k basketball visual triangles, plus about 14k/11k simplified collision triangles. This is in addition to the original venues and the detailed island scenery. New architecture does not yet have separate distant mesh LODs; the existing LODs and shared materials are the current optimization. Android tuning is deferred.

Desktop URP uses 4× MSAA, soft daylight shadows, four cascades and exterior contact shading. Basketball daylight and controlled fill replace its indoor lighting. Contact shading is restricted to exterior areas to preserve court-marking clarity.

## Rebuild and validation

- Editable venue sources: `ArtSource/Football/Sunvale/Stadium_Sunvale.blend` and `ArtSource/Basketball/Rally/Arena_Rally.blend`.
- Shared generator: `Tools/Blender/coastal_stadium_detail.py`, invoked by both venue generators. Layout manifests preserve entry, stair, concourse and future audience-route anchors.
- Unity attachment/import: `CoastalStadiumBuilder` and `CoastalIslandBuilder`. `CoastalStadiumBuilder.RebuildAll` refreshes both art libraries and builds Windows; `Tools/Build/Build.ps1 -Target Windows` rebuilds from current exports.
- `Review-CoastalStadiums.ps1 -RecordRoutes` runs actual controller routes, captures eight 1080p views per sport and records 720p frame sequences. It checks entrances/exits, curved stairs, landings, seating aisles, dock return, garden bridges, safe return, cameras, open sky and all four environment switches.
- `Test-CoastalStadiumRooms.ps1 -Sport Football` and `-Sport Basketball` run local host/guest route checks using the existing network input and replication. These are localhost checks, not a new internet/Relay latency qualification.
- `-probe -smoke -sport Basketball` validates logo selection, palette application, spawns, camera clearance and offline movement. The football smoke check covers movement and camera modes.
- `-coastalFrameBenchmark <report>` runs the normal rendered Windows player at 1920 × 1080, with 120 warm-up and 600 measured frames for each exterior, entrance and playing-area view. Do not pass `-batchmode` for this benchmark. The separate explicit-render timing in `performance.txt` is a GPU-synchronized offscreen measurement, not gameplay FPS.

Current raw evidence is in `Docs/VisualDirection/CoastalStadiums/GameReview/`; build hashes identify the executable content that was reviewed. `VALIDATION.md` in the same review directory records the final measured results and cleanup batch.

## Archive policy

`CoastalLegacyAudit` checks all Unity scene, active prefab and Resources dependency roots before identifying the retired roof/rig/scoreboard library. The completed migration used `inventory_coastal_legacy.py` and `Archive-CoastalLegacy.ps1` to inventory dependencies, verify hashes and archive confirmed candidates after the passing EXE review. Those one-time migration helpers were archived during the [7 October repository cleanup](REPOSITORY-HYGIENE.md#cleanup-completed-on-7-october-2026); they are no longer active build tools. Future reviewed cleanup uses [Archive-LocalArtifacts.ps1](../Tools/Build/Archive-LocalArtifacts.ps1).

Unity assets travel with their `.meta` files. Each batch retains relative paths, SHA-256 hashes, sizes, reasons, completed-move records and restoration instructions. Git internals, Unity-generated caches, current sources, rebuild tools, selected concepts and latest review evidence are excluded. Rebuild and executable checks are repeated after archival.
