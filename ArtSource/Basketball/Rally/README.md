# Rally Court — modular basketball art

Set `BLENDER_PATH` for your installation as described in [Setup](../../../Docs/SETUP.md); run commands from the repository root.

`Arena_Rally.blend` is the refined, editable source. It contains the complete assembled arena, two linked hoop instances, named module collections, five Rally maps plus the shared coastal maps, four review cameras, and a separate `REVIEW_ONLY` lighting collection. The original prototype is archived at `Legacy/ArtSource/Basketball/Arena.blend`.

The design takes the reference's warm timber, rounded sporting equipment, colorful seating, teal/coral/violet palette, arched gallery, and warm lights as direction. The rising-ball emblem, arena proportions, perimeter fixtures, banners, and furniture are original geometry and artwork. No concept-image projection is used.

## Modules

Each row has its own FBX and Unity prefab. All modules use metres and unit scale.

| Module | Contents | Customization |
| --- | --- | --- |
| Court | Honey maple, teal apron and keys, court markings, apron lettering | Wood texture, paint colors, markings, wordmark |
| CenterEmblem | Rising-ball disk, outline and bars | Swap the emblem, recolor each role |
| Hoop | Rounded backboard, padding, frame, wheels, orange rim, diamond net, shot clock | Padding, target, board, rim, net, badge; replace whole assembly |
| Stadium | Two-tier structural bowl, terrace stairs, arched concourses, wall panels | Architecture colors; individual bay meshes |
| LowerSeating | 680 molded seats, 24 near/far bay pairs | Six independent seat colors |
| UpperSeating | 576 molded seats, 24 near/far bay pairs | Separate six-color palette from lower seating |
| Railings | Balcony, promenade and aisle rails | Enamel and uprights |
| PerimeterPads | Rounded courtside cushion blocks | Four independent cushion colors |
| Banners | 12 cloth panels with ball motifs and diagonal prints | Fabric and print colors; replace individual banners |
| CourtsideFurniture | Folding team chairs, scorer table, monitors and coolers | Home/visitor upholstery, equipment and frames |

## Placement and scale

- Blender: X is court width, Y is court length, Z is up. Court centre is `(0,0,0)`, finished floor height is zero; decorative paint is 3–21 mm above it.
- The dark structural foundation ends at `z=-0.11 m`, touching the underside of the teal apron. Its top must never be raised to the finished floor at zero: coplanar foundation/apron faces caused camera-dependent color flicker. The source audit and Windows scene builder validate this separation.
- Court: **15.24 × 28.6512 m**. Rim height: **3.048 m**, clear internal diameter: **0.4572 m**. These preserve the existing project's proportions.
- Every FBX uses the court-centre pivot except `Hoop.fbx`, whose origin is the rim centre projected onto the floor. Its support extends along local +Y and it faces -Y.
- Blender north hoop: `(0,12.7254,0)`, Z rotation 0°. South: `(0,-12.7254,0)`, Z rotation 180°.
- This Unity FBX conversion maps Blender `(x,y,z)` to Unity `(-x,z,-y)`. North Unity placement is `(0,0,-12.7254)`; south is `(0,0,12.7254)` with Y rotation 180°.
- Source hoop meshes are linked so editing the canonical geometry updates both ends. Duplicate the mesh/material data first to customize only one end in Blender. In Unity, duplicate the relevant `.mat` and assign it to just one hoop instance.

## Editing and exporting

Open the source in Blender. Select the corresponding collection and role mesh. Each material is named `Rally_<Module>_<Role>`; materials are not shared between unrelated modules. In the Shader Editor, textured materials expose their tint on the Multiply node's second color input. Untextured materials use Principled Base Color. This preserves texture detail when recoloring.

To export hand edits without rebuilding:

```powershell
& $env:BLENDER_PATH --background --python Tools/Blender/export_rally_arena.py
```

Retain the module collections, empty roots, material names and seat `LOD0`/`LOD1` suffixes. The exporter reads changes to the existing named material roles. If adding a new role or changing texture assignments, update the manifest/import specification as part of that change. The Unity metallic/smoothness map is a separate packed map; if editing the wood roughness image, repack its inverse into the alpha channel of that map.

To regenerate the procedural design (this **overwrites** edits to the refined source and its exports):

```powershell
& $env:BLENDER_PATH --background --python Tools/Blender/build_rally_arena.py -- --render
```

Use `export_rally_arena.py` for this asset library; `export_assets.py` now exports only the current golf source.

## Unity library

FBX files, PNG maps and module-specific URP/Lit material assets live at `Game/Assets/_Game/Art/Basketball/Rally/`. The library contains 10 individual module prefabs plus `Game/Assets/_Game/Prefabs/Basketball/Rally/RallyArena.prefab`. Open `Game/Assets/_Game/Scenes/RallyArenaReview.unity` for the assembled engine review.

Reimport/rebuild through **Sports → Basketball → Build Rally art library**, or run `RallyArenaBuilder.Build` in Unity batch mode. This regenerates the module prefabs, materials and review scene from the FBX/manifest. Make persistent art changes in Blender or the generator; save alternate Unity colors as separate materials/prefab variants.

`Bootstrap` and `BasketballEnvironment` now instantiate this library through `BasketballBuilder`. `Builds/WindowsFinal/WhatTheFish.exe` includes the replacement. The integration retains the four logo indices (Rally crest, star, lightning bolt, shield), saved palettes, arena-name signs, and the existing host-shared appearance model. Palette 0 restores the authored multi-color seating; other palettes tint the seats and replace the teal trim. Three legacy logo meshes are copied into standalone assets, so the old arena geometry is not instantiated in the game.

The runtime environment adds matched architecture collision, walkable entrances and stairs, courtside furniture colliders, coastal daylight and controlled fill, and live arena-name text on four supported perimeter scoreboard panels. The central court has no ceiling or suspended lighting rig. Match scoring, ball physics and animated nets remain future work.

## Rendering and budget

- 1,256 seats and 48 bay-level LOD groups; 40 lower seats were removed to widen four entrance passages. Near/far geometry remains shared per bay.
- Existing court wood uses a 2048px map. Approachable coastal structure uses shared 2048px base, normal and metallic/smoothness maps at 512 pixels/metre before mip selection.
- `CoastalStadiumBuilder` adds cream arcades, modeled trim, planters, four exterior stairs and supported scoreboards. Architecture and simplified collision are separate FBXs under `Art/CoastalStadiums`; the source collections are saved in this same Blender file.
- Continuous runoff walls, roof collision, ceiling, central scoreboard and overhead rig are retired. Structural meshes and simplified route collision support the existing player controller. Daylight is active inside and outside.
- Current measured Windows performance, geometry audits and close-view evidence are documented in `Docs/COASTAL-STADIUMS.md`. Historical indoor triangle totals and lighting assumptions do not describe this coastal build. Android tuning is deferred.

## Verification

`blender-source-audit.json` checks the saved source's geometry, UVs, floor normals, independent module materials, exported files, packed maps, court size, two rim placements, and all 48 near/far seat pairs. `unity-import-audit.txt` checks actual imported scale, material remapping, rims, LOD groups, triangle totals and colliders.

Review images are in `Docs/VisualDirection/Basketball/`: court hero, bowl overview, hoop detail, gallery detail, and an actual Unity capture. Blender's area-lit renders are art previews; the Unity image shows the imported asset under simpler engine preview lighting.

