# T1 terrain integration repair

This record describes the earlier repair. The floating-floor boundary was later
replaced and the annotated bunker restored; see the
[current T1 repair](GOLF-T1-BUNKER-SEAMS.md).

## Scope

The multiplayer pads and authority reservations remain unchanged: ten equal-radius
T1 pads, centre first and then outward; three pads each for T2-T5. This repair is
localized to T1 ground, old entry paving and affected course surface dressings.

## Art and geometry

- Replace the separate fan, rectangular lawn and saturated brown path with one
  continuous graded ground mesh. The existing tee height and pad positions remain.
- Reuse the original Golf terrain macro/control maps, turf/sand detail and normal
  maps, metre scale, PBR lighting and fog. Terrain UVs are sampled from the actual
  original terrain colliders rather than guessing texture coordinates.
- Planting uses the original island palette and mowing/noise modulation. The
  opening has feathered grass/sand/rough joins; the rear path uses the original
  pale sand path palette. The old entry spur is reclaimed without a turf rectangle.
- Refit affected translucent fairway/sand dressings onto the graded surface and
  remove remaining small paving triangles at the reclaimed spur.
- Editable original FBX, textures and other-hole tee layouts remain available.
  No new bitmap textures or runtime per-frame terrain work are introduced.

Implementation: `GolfMultiplayerTeeBuild.PolishT1` in `GolfTeeIslandBlend.cs`, `GolfTeePaths.FinishPaths`, tee generation and the
`WhatTheFish/GolfTeeIsland` shader. The existing surface resistance masks retain
their core sand/rough intervals; colour joins feather visually across their edges.

## Evidence

Current checks and actual Unity captures are under ignored `Builds/GolfT1BlendQA/`.

- 399 editor assertions passed: all pads supported/level, distances and assignment
  unchanged; rear path ground/capsule clearance checked.
- 28 assertions passed in the fresh final Windows player with four fixture actors:
  own tee/ball alignment, Aim, simultaneous completion, unique next pads, queue and
  water return. This task did not repeat the previous real two-peer network test.
- Actual Unity top and near screenshots reviewed. Windows capture quality does
  not establish physical-phone quality or FPS.
- Opening and final authored tee-site records are identical; scene and prefab
  site records agree. The full WindowsFinal player matched all 326 candidate
  files by SHA-256 before its delivery note was amended.
- Fresh AndroidSubmission at 22:57:57 Malaysia time: 90,795,846 bytes,
  up 500,432 from 90,295,414; 909,204,154 bytes below
  the strict decimal-GB limit. Budget gate and independent v2 signature passed.
  Detailed measurements: [build size](BUILD-SIZE.md).

## Cleanup and snapshot limits

Unused old opening/spur meshes and materials were checked for scene/prefab GUID,
generator and runtime string references before removal. Their `.meta` files and
previous complete release were archived with the repository tool:

- `Legacy/20261019-224758-035-golf-t1-terrain-repair/`
- `Legacy/20261019-225139-394-golf-t1-final-junction/` (reviewed intermediate player
  and APK before the final perimeter-road junction trim).

Source masters remain intact. Build-generated character cache, equivalent Ball
prefab IDs/order and quality normalization were reviewed and restored to their
opening state; their generated versions remain in ignored task evidence. The
validated binaries include those normal build transformations and existing local
working-tree features. No Git staging, commit, push or history rewrite was done.
