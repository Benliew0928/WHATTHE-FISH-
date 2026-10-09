# Five-hole golf map

The Golf island has five new cups with aqua flags numbered **1–5**, putting surfaces and paired tee markers. Pennants and tee markers reuse the map's existing `RI_Teal` material. The previous two teal flags remain scenery; they do not define this course. The course is included in both the streamed `SkySail_Golf` scene and the reusable `GolfEnvironment` prefab.

| Hole | Area | Cup X/Z (m) | Tee X/Z (m) | Straight distance | Proposed par |
| --- | --- | --- | --- | ---: | ---: |
| 1 — Welcome Green | Southern entrance fairway | 8 / −106 | −8 / −141 | 38 m | 3 |
| 2 — Sand Garden | Western grassland beyond the bunkers | −103 / 10 | −22 / −46 | 98 m | 3 |
| 3 — Winding Fairway | Northeastern grassland | 68 / 100 | −14 / 39 | 102 m | 4 |
| 4 — Coastal Approach | Southeastern coast | 111 / −70 | 115 / 21 | 91 m | 3 |
| 5 — High Green | Northern raised green | −10 / 126 | 8 / 55 | 73 m | 4 |

Coordinates use the island's local metre space. Heights are sampled from the original terrain. Distances and pars describe this compact game layout, not a regulation course. Holes 1, 2 and 3 are **161–215 m apart**; the closest pair anywhere is approximately **82 m apart**. All new greens clear the coastal promenade, bunkers and existing palm gardens.

Cups are **0.285 m in diameter**, exactly **1.5×** the initial 0.19 m opening, and remain **0.18 m deep**. Each terrain opening has a collidable inner lining and recessed floor. Flagpoles are approximately 3 m high, with a shared aqua pennant and white numbers visible on both sides.

## Editing and regeneration

Edit [FiveHoleCourse.json](../Game/Assets/_Game/Sports/Golf/FiveHoleCourse.json), then run **WHATTHE FISH? → Golf → Place five-hole course** in Unity. **Place, audit and capture five holes** checks the actual scene/prefab colliders and repeated generation; **Audit and capture saved five holes** reviews the serialized scene in a fresh editor without regenerating geometry. Both write Unity renders to ignored `Builds/GolfPlayabilityQA/Map/`. [GolfCourseBuilder](../Game/Assets/_Game/Editor/GolfCourseBuilder.cs) also runs from the ordinary refined-island generator, so rebuilding the world retains the course.

Keep the configuration, generator, generated [course mesh/material assets](../Game/Assets/_Game/Art/Golf/FiveHoleCourse), [flag prefab](../Game/Assets/_Game/Prefabs/Golf/HoleFlag.prefab), scene, environment prefab and their `.meta` files in Git. The original Blender/FBX terrain masters remain editable and unchanged. Derived terrain sectors contain the cup openings; repeated generation restores the original terrain before cutting. Shared poles, cloth, cup floors and tee meshes reuse existing material/shader families and add no imported texture or font. **The existing terrain mesh remains the putting surface**: it retains its `Golf_Terrain` material and texture coordinates. Small local height adjustments give the immediate cup surrounds a gentle slope; a quintic falloff reconnects them to the original hillside. There is no separate green overlay, colour patch, new texture or additional terrain collider.

Grass keeps its meadow distribution outside the five greens. A small irregular fringe clears tall blades around the cups, and the remaining blades follow the local height adjustment. Android uses the same green records with the existing shared grass library; desktop uses derived grass meshes. The original macro-colour/control textures, base terrain height function and four-island travel/streaming model are retained. Existing painted fairway and practice-green colours belong to the original map and remain intact.

The 8 October playability update also adds surface-dependent rolling resistance and a finer putting charge range; see [gameplay tuning](GOLF-MINIGAME.md). Hole 4 moves 10.8 m onto a gentler coastal position; Hole 5 moves 20.4 m toward the raised plateau. Other cup and tee positions remain unchanged. The old positions are restored to continuous terrain when the generator starts from the original master.

## Current playability verification

Current [measured release and evidence](VERIFICATION.md#golf-playability-and-integrated-greens--8-october-2026) records inner slopes of 0.32–0.53 degrees and no new imported art.

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
