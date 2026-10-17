# Five-hole golf map

The Golf island has five cups with aqua flags numbered **1–5**, putting surfaces and paired tee markers. Tee markers reuse the map's `RI_Teal` material; pennants use the approved original flag artwork and a two-sided wind shader. The two old unnumbered scenery flags have been removed; the numbered cup flags and original tee stones remain. The course is included in both the streamed `SkySail_Golf` scene and the reusable `GolfEnvironment` prefab. `GolfCourseBuilder.RemoveDecorativeFlags` filters the old flag geometry from the retained FBX master, and the Blender generator omits it on regeneration.

## V5 obstacle and water additions - 14 October 2026

The approved [full design](../ArtSource/Golf/Course/ObstacleWaterV5.png) and
[holes 3/5 detail](../ArtSource/Golf/Course/ObstacleWaterV5-detail.png) add
**47 obstacle zones / 141 kit props**, **14 rough patches**, and **six ponds**
to the retained V2 map. Totals: **225 physical props**, **20 rough areas**,
**20 unchanged sand patches**, six water footprints. All five hole/tee/route/
bypass/green/par records and old 84 prop entries remain exact.

Ponds use horizontal teal water, the existing LagoonWater ripple shader and
shared sand banks conforming to the original slope. Banks contain water without
editing original terrain mesh vertices/colliders, cups, flags, signs or fairways.
Only blades under new pond/bank footprints are cleared; mobile grass uses the
same footprint data. No new texture, model, reflection camera or global render
setting is needed. Water levels are sampled from terrain; the observed lift above
centre ground is **0.264-0.766 m** across the six retained design positions.

`GolfBall` records supported safe dry terrain/bank/tee contact positions, avoiding
water and its immediate one-metre margin. Falling or swept/guided water contact
clears the trajectory, trail, velocity and old landing-rebound state, then rests
the ball at its latest dry contact. Airborne positions do not update the anchor;
a new tee/hole seeds it normally. Existing manual/out-of-bounds recovery still
uses the last shot position. Water does not add a penalty stroke or alter hole
progress. Only the host performs recovery; existing reset-sequence snapshots snap
remote presentation without an RPC/protocol change.

`GolfObstacleAdditionBuilder.ApplyAndReview` applies missing V2/V5 entries and
pond art additively. `GolfCourseBuilder` preserves the additions on normal fresh
regeneration. Generated water/bank/rough assets and metas belong in Git. Details
and source/build limits are in [verification](VERIFICATION.md).

## Additional V2 obstacle zones � 14 October 2026

The approved 43-zone plan adds **72 physical kit props**, **19 shallow sand
patches** and **six rough areas**, retaining all 12 prior props and the prior
sand patch (84 props / 20 sand patches total). Rocks and palms reuse existing
prefabs, LODs, materials and collision. Sand/rough overlays share existing
textures and shader; rough areas use the existing rough resistance value.
No shot strength, bounce, timing, scoring or multiplayer parameter changes.

All five tee/cup coordinates, route points, bypass, widths, pars, greens,
terrain heights, numbered flags, signs and UI remain unchanged.
`GolfObstacleAdditionBuilder.ApplyAndReview` adds only missing V2 objects to
the saved scene and environment prefab; grass is cleared only under added sand.
`FiveHoleCourse.json` retains all 43 zone records and individual prop entries
for normal fresh-clone regeneration. See [validation](VERIFICATION.md).

## Current circular course — 14 October 2026

Five holes follow the island clockwise and return near the entrance. Route
length follows intended landing waypoints rather than a direct shot line.

| Hole | Area | Cup X/Z (m) | Tee X/Z (m) | Route length | Proposed par |
| --- | --- | --- | --- | ---: | ---: |
| 1 — Bunker Bend | Southwest bunker bend | −62 / −90 | −10 / −146 | 89 m | 3 |
| 2 — West Stone Garden | Western stone and sand garden | −44 / 53 | −73 / −71 | 141 m | 4 |
| 3 — Plateau Approach | Northern raised plateau | −10 / 126 | −39 / 68 | 72 m | 3 |
| 4 — Limestone Gate | Eastern stone gate | 119 / 39 | 8 / 116 | 140 m | 4 |
| 5 — Homeward Links | S-shaped southern return | 14 / −122 | 109 / 15 | 187 m | 5 |

Coordinates use the island's local metre space. Heights are sampled from the original terrain. Adjacent cup-to-tee walks are under 32 m. Pars are design targets; automated route traversal does not establish optimal scoring. Hole 2 moved 5.7 m from the proposal to a smoother nearby location. Existing bunkers, coast, structures and the northern plateau remain.

Twelve collidable kit obstacles add a west stone island, an east stone gate,
five palms and three low return-route rocks. Hole 4 has a southern bypass;
hole 5 routes west of the low ridge, with lofting over it as an alternative.
One new shallow sand patch uses sand rolling resistance. It is a conforming
surface, not an excavated bunker. Six feathered fairway meshes share existing
turf textures; sand shares the existing sand texture. Five tee signs share the
existing UI font and timber material. No new bitmap, font or model is added.
Greens use radius 6 m and a 10 m blend; tee patches use radius 3.5 m and a 7 m
blend. Desktop and procedural mobile grass clear the same greens and sand.

The numbered flags now use the approved teal swallowtail design with cream
trim and rounded cream numerals, a silver ball-top pole and teal bindings.
Both faces are numbered. `GolfFlagArtBuilder` generates shared editable meshes;
`GolfFlagWind` bends cloth, trim and numbers together on the GPU while fixing
the pole-side edge. Maximum tip displacement is now 0.204 m (six times the initial 0.034 m bound). The pole stays still.
Five small derived flag textures ship; the full [approved references and authoring notes](../ArtSource/Golf/Flags/README.md)
remain outside Unity. Cup dimensions and scoring rules are retained at the revised locations.

Cups are **0.285 m in diameter**, exactly **1.5×** the initial 0.19 m opening, and remain **0.18 m deep**. Each terrain opening has a collidable inner lining and recessed floor. Flagpoles are approximately 3 m high, with shared cloth geometry and reference teal/cream artwork visible on both sides.

## Editing and regeneration

For the current circuit use `GolfCircuitReview.PrepareAndReview` to regenerate,
audit scene/prefab/repeat generation, and capture views under
`Builds/GolfCircuitQA/`. `GolfCircuitReview.CaptureSavedViews` renders existing
saved content without regeneration. `GolfCircuitArtBuilder` handles shared
fairway/sand meshes, obstacles and signs. Keep all generated delivery assets
and their `.meta` files alongside the editable JSON.

Edit [FiveHoleCourse.json](../Game/Assets/_Game/Sports/Golf/FiveHoleCourse.json), then run **WHATTHE FISH? → Golf → Place five-hole course** in Unity. **Place, audit and capture five holes** checks the actual scene/prefab colliders and repeated generation; **Audit and capture saved five holes** reviews the serialized scene in a fresh editor without regenerating geometry. Both write Unity renders to ignored `Builds/GolfPlayabilityQA/Map/`. [GolfCourseBuilder](../Game/Assets/_Game/Editor/GolfCourseBuilder.cs) also runs from the ordinary refined-island generator, so rebuilding the world retains the course.

Keep the configuration, generator, generated [course mesh/material assets](../Game/Assets/_Game/Art/Golf/FiveHoleCourse), [flag prefab](../Game/Assets/_Game/Prefabs/Golf/HoleFlag.prefab), scene, environment prefab and their `.meta` files in Git. The original Blender/FBX terrain masters remain editable and unchanged. Derived terrain sectors contain the cup openings; repeated generation restores the original terrain before cutting. Shared poles, cloth, cup floors and tee meshes reuse common geometry. Five small flag delivery textures reproduce the approved references; no font is added. **The existing terrain mesh remains the putting surface**: it retains its `Golf_Terrain` material and texture coordinates. Small local height adjustments give the immediate cup surrounds a gentle slope; a quintic falloff reconnects them to the original hillside. There is no separate green overlay, colour patch, new texture or additional terrain collider.

Grass keeps its meadow distribution outside the five greens. A small irregular fringe clears tall blades around the cups, and the remaining blades follow the local height adjustment. Android uses the same green records with the existing shared grass library; desktop uses derived grass meshes. The original macro-colour/control textures, base terrain height function and four-island travel/streaming model are retained. Existing painted fairway and practice-green colours belong to the original map and remain intact.

The 8 October playability update also adds surface-dependent rolling resistance and a finer putting charge range; see [gameplay tuning](GOLF-MINIGAME.md). Hole 4 moves 10.8 m onto a gentler coastal position; Hole 5 moves 20.4 m toward the raised plateau. Other cup and tee positions remain unchanged. The old positions are restored to continuous terrain when the generator starts from the original master.

## Current playability verification

The previous [8 October release](VERIFICATION.md#golf-playability-and-integrated-greens--8-october-2026) records the earlier layout. Current circuit measurements and delivery limits are in the 14 October verification section.

Run [Test-GolfPlayability.ps1](../Tools/Build/Test-GolfPlayability.ps1) against the current player. It measures actual rolling distances on four surface types, putts in four directions around every cup, and three charge levels from each tee. Run the existing physics, match, island and cart checks as regressions. The editor review checks inner cup slopes, surrounding collision continuity, original UVs, unchanged outer terrain and repeat generation. Mobile geometry requires its own preview checks; Windows renders do not qualify phone performance.

## Earlier map validation

Run the course editor review, rebuild the Windows player, then run [Test-GolfIsland.ps1](../Tools/Build/Test-GolfIsland.ps1). The golf probe checks the five numbered flags, real recessed cup floors and grounded tees alongside the existing spawn, shore, fairway, bunker, camera and island-switch checks. For Android grass geometry on Windows, build with `MobileOptimizationValidation.BuildPreview` and run the same `-golfAudit` probe against that player. Windows rendering does not establish phone quality or FPS.

After feature checks, run [Build.ps1](../Tools/Build/Build.ps1) with `-Target AndroidSubmission` and inspect the actual APK and `Builds/SizeAudit/latest/`. Current measured release provenance belongs in [BUILD-SIZE.md](BUILD-SIZE.md) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).

The refined layout passed **235 editor assertions** across the scene, prefab and repeated generation, plus **78 assertions** after reopening the saved scene. Comparisons with the original terrain check 47–48 samples per green, with position differences below 0.0001 m and texture-coordinate differences below 0.000001. Flags reuse the original map material, openings measure 0.285 m, and all cups retain real recessed floors and inner-wall collision.

The desktop and Android-geometry Windows players each passed **142 golf assertions**. The later combined-source desktop run also passed **142** at `Builds/GolfG2QA-20261003-195311/`; mobile-geometry evidence is in `Builds/GolfG2QA-20261003-193449/`. The unchanged analytic terrain passed **13,122 samples**. Actual close/distant views and cup detail were inspected in running players and a freshly loaded saved scene. Current editor captures and source manifests are in `Builds/GolfCourseRefinementQA/`. These checks cover the map; separate concurrent cart updates have their own validation scope.

The removed tinted greens, orange material, unused mobile material and obsolete grass/terrain derivatives were archived after GUID, runtime-load, generator and provenance scans in `Legacy/20261003-192351-292-golf-course-refine-unused/`. The original terrain, grass, structures, macro-colour texture, aqua texture and Blender master retain their Git HEAD LFS hashes.

The final combined AndroidSubmission measures **88,624,542 bytes**, versus the **86,896,050-byte** baseline, with **11,375,458 bytes** of strict-limit headroom. The package also includes the separate cart speed/wheel changes; this increase is not the course-only cost. Course plus original-terrain compression estimates instead decrease **122,416 bytes**. All 2,903 authored build inputs match and v2 signing passes. See [release/size provenance](BUILD-SIZE.md#golf-course-refinement-and-current-combined-release--3-october-2026). The 75 MB development target and physical-phone testing remain outstanding.

The initial 3 October 2026 layout passed **160 editor assertions**, **142 mobile-geometry Windows assertions**, **142 final desktop assertions**, and **13,122 terrain-reference samples**. Its evidence is in `Builds/GolfFiveHolesQA/`; desktop captures are in `Builds/GolfG2QA-20261003-171939/`, mobile-geometry captures in `Builds/GolfG2QA-20261003-165611/`. That earlier APK measured **86,896,050 bytes** and is the refinement baseline, not verification of the revised layout. Phone appearance/FPS and shot/scoring mechanics are unverified.

The first-layout unused terrain derivative was archived in `Legacy/20261003-165821-340-golf-five-holes-trial-mesh/` after reference and generator scans. Superseded builds, the first masked-material trial and raw trial diagnostics are in `Legacy/20261003-171933-414-golf-five-holes-delivery/`. Active art masters, dependencies and Unity caches remain in place.

## Reference flags and stronger wind — 13 October 2026

All five cup flags now derive their silhouette, numbers, cream edging and fabric shading from the original supplied PNGs, instead of redrawing them as flat coloured geometry. Silver poles and teal bindings remain 3D. See the [authoring workflow](../ArtSource/Golf/Flags/README.md). The fixed edge has zero displacement; motion grows quadratically toward the free tip. Wave amplitudes are six times the prior values: maximum .204 m per side, theoretical .408 m peak-to-peak. This is a procedural cloth approximation with broader folds. Scene/prefab cup positions and colliders remain unchanged.
