# Golf equipment integration — 1 October 2026

The Meshy ball, driver, iron and putter are reusable Unity prefabs, placed beside the golf practice green. They are included in the streamed golf scene, its source environment prefab and the environment generator. The four-island world and shared room travel are preserved. This change adds visual equipment, not golf-shot controls.

## Open the actual game

Run `Builds/WindowsFinal/Review-Golf-Equipment.cmd`. The current complete Windows player opens on the equipment with seven review buttons and a walk-around option. Regeneration also writes the launcher under `Builds/WindowsFinal/`. The launcher uses a development-only camera; the models and placement are the same scene assets used by normal gameplay and the Android release. Captures under `Builds/GolfEquipment/20261001/Player-ColourCorrected/` are actual Windows player images, not generated mockups.

## Sources, fixes and limits

[Source documentation](../ArtSource/Golf/Equipment/README.md) and [provenance](../ArtSource/Golf/Equipment/provenance.json) retain the reference images, original downloads, account attribution and license. The Meshy UI called the model **6 Lite**, with no 6.1 Lite option. Four geometry and four PBR texture jobs used 80 credits. The unchanged ZIPs and high-quality Blender masters remain outside Unity Assets.

The ball's uneven source outline was replaced with a round 43 mm delivery sphere, with the generated dimples baked into a normal map. Shafts were aligned and scaled, club grip pivots established, and two closed mesh LODs produced per asset. The first reduced clubs showed triangular texture seams; fresh UVs and high-to-low baking corrected that. Base colour uses an unlit colour transfer so metallic pixels retain their RGB. Club ground contact is fitted against terrain beneath the head rather than the grip position.

Remaining flaws: ball dimples are oversized and irregular; club face grooves and grip rings are soft and uneven; heads are larger than realistic proportions. Desktop ambient occlusion becomes grainy at extreme ball magnification. These are suitable stylized visual props, not validated regulation equipment. Physical-phone appearance and FPS remain untested.

All four highest-detail meshes together contain **15,264 triangles**, dropping to **4,320** at distance. Clubs share one material across their LODs, with 512-pixel colour/normal maps and 256-pixel masks; the ball uses one 1024×512 normal map. Android imports use ASTC, and no source ZIPs or full-resolution masters are included in the APK.

## Validation

- All four source meshes and eight delivery meshes were inspected for connected components, closed edges, finite UVs and degenerate faces. Delivery FBXs were reimported, and saved Blender masters reopened with verified relative or packed dependencies.
- Regeneration from a separate checkout path containing spaces, launched from an unrelated working directory, passed. LOD topology and dimensions matched. Original ZIP metadata is retained as source provenance; active delivery dependencies are portable.
- The equipment suite passed all 20 checks: four models, two LODs each, supported materials, terrain contact, ball scale, one active island and unloading/reloading with the golf scene.
- The golf-island suite passed all 126 checks, covering ten spawn points, 48 shoreline directions, fairway traversal, bunker exits, camera views and island switching. Its old loaded-island assumptions were updated for the existing streamed world.
- The local-room suite passed: ten-player room, distinct spawns, full-room rejection, independent basketball room, late joins, host start/return, guest authority restrictions and host departure. No null, missing-reference or index errors were found.

Evidence: `Builds/GolfEquipment/20261001/`, `Builds/GolfG2QA-20261001-194949/` and `Builds/GolfQA-Local-20261001-193848/`. Temporary logs and captures stay ignored.

## Rebuild

Use `Tools/Blender/build_golf_equipment.py`, then Unity's `GolfEquipmentBuilder.Prepare` or `GolfEquipmentBuilder.BuildWindows`. `RefinedIslandBuilder` also calls the placement builder. Run `Tools/Build/Test-GolfEquipment.ps1`, `Tools/Build/Test-GolfIsland.ps1` and the appropriate room tests after functional changes. Build Android with `Tools/Build/Build.ps1 -Target AndroidSubmission`; use [the current size record](BUILD-SIZE.md) for measured APK bytes, not source-folder sizes.

## Measured Android release

The verified APK built at 11:59:27 UTC is **85,088,760 bytes**, compared with the **83,751,052-byte** integration baseline: **+1,337,708 bytes**. It leaves **14,911,240 bytes** to the strict 100,000,000-byte limit, while remaining **10,088,760 bytes above** the 75 MB development target. AndroidSubmission and APK v2 signature verification passed. SHA-256: `6F91AEC0E686E4820D135F6698222C69BB6969D11C0F0549CCB97E9E218CEABA`.

This shared-workspace build also includes the concurrent jump changes and restored distant-island mesh compression; the net APK delta cannot be assigned solely to golf. Golf equipment totals 2,870,024 uncompressed serialized bytes and approximately 1,314,630 compressed bytes by separate per-asset DEFLATE estimates. These estimates do not exactly allocate APK chunks. The ball normal is the largest new golf contributor at approximately 596 KB. Original source art remains outside the build; shared timber and character/environment assets remain the largest overall contributors. The initial 85,817,216-byte build was superseded after the other task restored proxy compression, saving 728,456 bytes.

The final size-audit snapshot, signature, integrity hashes, Windows player manifest and readiness result are retained under `Builds/GolfEquipment/20261001/`. No Android device was connected. The Windows review does not qualify phone visuals or performance.

Git staging, commit and push remain with the user. Archive batches under ignored `Legacy/`: `20261001-192515-467-golf-equipment-duplicate`, `20261001-193445-227-before-golf-equipment-player`, `20261001-194502-404-golf-first-pass`, `20261001-195509-003-before-golf-apk`, `20261001-195608-612-golf-equipment-qa-cleanup`, and `20261001-200406-305-golf-analysis-helper`. These preserve duplicate/superseded output, temporary extractions and the tested portability checkout; original ZIPs, current masters, delivery assets and final evidence remain active.
