# Golf multiplayer tee surface repair — 20 October 2026

## Scope

Repair the visible grass colour cuts around T2–T5 and restore the annotated
curved coastal walkway beside the T1 welcome approach. Preserve tee allocations,
ball/cup positions, terrace heights/colliders, routes, obstacles and shot controls.

## Changes

- T2–T5 terraces now reuse the island macro/control textures, turf/sand detail
  and normals, linear colour treatment, PBR lighting and fog. Their authored
  local UVs map back to the original island UVs; detail tiling remains in world
  coordinates. Existing five-metre edge weights receive a smooth alpha fade.
- Draw the terrace before the original transparent course dressings, without
  writing an opaque depth mask that cuts off the fairway and sand colours.
  This keeps the existing course colour bands visible across the join.
- Restore native terrain colour in the annotated welcome/coastal junction.
  Restore original CoastalPath paving and border-stone triangles there from
  the editable Structures master, retaining UVs/materials and adapting height
  to the existing graded terrain. The old straight tee spur stays removed.
- Update generators so regeneration retains the same treatment. No new textures,
  external dependencies or runtime update loops were added.

## Focused verification

Evidence is local and ignored under `Builds/GolfTeeColourQA/`.

- Reviewed 11 paired 1600×1100 actual Unity renders: T2–T5 top/near views,
  T1 overview and the annotated walkway top/near views.
- 46 focused surface assertions passed: native map references and UV phase,
  smooth edge weights, original pad support, original source triangle coverage
  (108 paving / 2,240 border-stone triangles), region exclusions and shaders.
- Scene and environment prefab bytes remain identical to the opening snapshot.
  All tee-site values remain identical. Of the 50 existing multiplayer-tee
  meshes, only the two annotated CoastalPath meshes changed.

## Delivery

- Complete tested Windows player promoted to `Builds/WindowsFinal/`; all 326
  files SHA-256 matched the candidate, including the amended delivery note.
- Existing runtime tee fixture passed 28 assertions. This checks reservations,
  distinct pads, support, water recovery and ownership; no unrelated sport tests.
- Fresh official AndroidSubmission: 93,875,514 → 93,972,974 bytes (+97,460);
  906,027,026 bytes headroom below the strict decimal-GB limit. ZIP integrity
  and independent v2 signature passed. Largest compressed entry, islands bundle,
  is 45,152,911 bytes; IL2CPP/Unity/metadata sizes remain unchanged.
- APK SHA-256: `565FFB4F3E28E6BBF696461BF76FB71E03BE0AE5214372771FA4821F1A5E9E78`.
- Reviewed prior APK/player are recoverable in local archive batches
  `Legacy/20261020-190104-358-golf-tee-colour-apk/` and
  `Legacy/20261020-190206-181-golf-tee-colour-player/`.
- All 508 C# hashes match the tested source snapshot. Restored build-generated
  CharacterIdentity cache hash, equivalent Ball prefab local IDs/order and
  temporary quality setting to opening bytes after reviewing their differences.
  No Git publishing performed for this task.

Physical-phone appearance/FPS and WAN multiplayer are not established by these
local render and support checks.

## Five-pad extension, 21 October 2026

T2–T5 now have five equal-cup-distance positions on the existing tee arc:
centre, ±4 m and ±8 m along the arc. Assignment remains centre first, then
random player/side selection at each outward tier, with exclusive reservations.
A sixth concurrent arrival queues until a pad becomes free. T1 stays unchanged.

Four derived terrace meshes extend to include the new outer pads. They retain
the native material palette, world/detail UV phase and five-metre smooth alpha
join. All five pads share a supported height. The original island masters,
T1 grading, corridor meshes, hole positions and existing obstacles are preserved.
Focused UV/colour/support records and overhead/near views are in ignored
`Builds/GolfRushFiveQA/` and `Builds/GolfTeeColourQA/five-tees/`.
