# Basketball

Current editable master: `Rally/Arena_Rally.blend`. Read `Rally/README.md` for the modular art pipeline. Regenerate with `Tools/Blender/build_rally_arena.py`, export saved edits with `Tools/Blender/export_rally_arena.py`, then import using `RallyArenaBuilder.Build` and rebuild the playable scene.

The three alternative centre logos have independent mesh and prefab assets under `Game/Assets/_Game/Art/Basketball/Rally/LogoVariants`. Scene rebuilding no longer reads the old placeholder arena.

The placeholder source, FBX and unused materials are archived under `Legacy/` (relative to the repository root), outside Unity Assets and safe to delete with that archive.

## Basketball ball — 1 October 2026

The user-downloaded [Meshy GLB](Ball/Meshy_AI_Basketball_1001104636_texture.glb) is retained unchanged, together with the [input image](Ball/basketball-meshy-reference.png) and [generation/provenance record](Ball/meshy-provenance.json). Meshy 6 Lite produced a textured 10,330-triangle remesh. Its irregular radius and uneven seams made it an appearance reference for a rebuilt ball rather than the delivered geometry. The source's displayed CC BY 4.0 license is retained in the record.

The editable [Basketball.blend](Ball/Basketball.blend) uses a centred, 24 cm spherical mesh with regular seams and procedural rubber grain. Its two relative 2048×1024 master maps remain beside it. The Unity FBX contains **2,976 / 720 triangle LODs**, with one material and two 1024×512 delivery maps. Android imports them as ASTC 6×6. The original GLB and master maps stay outside Unity Assets.

Regenerate with Blender 5.1 using [build_basketball_ball.py](../../Tools/Blender/build_basketball_ball.py). The default repository root comes from the script's location; `-- --root <checkout>` overrides it and `-- --render` creates local review images. Regeneration replaces the derived master and delivery files, so preserve intentional hand edits before running it. [model-audit.json](Ball/model-audit.json) records geometry checks, hashes, reopened master dependencies and an FBX reimport. The exporter retains a fixed, nonfunctional Original/FileName provenance string; active image paths are relative and verified to exist.

Run the Unity menu **WHATTHE FISH? → Basketball → Prepare ball and physics**, or batch method `BasketballBallBuilder.Prepare`, after regeneration. The reusable prefab is [BasketballBall.prefab](../../Game/Assets/_Game/Prefabs/Basketball/BasketballBall.prefab). It is included once in the streamed basketball arena and environment prefab, with physical hoop/backboard contacts. Movement, bounce, spin and guest interpolation are runtime components; the rigid ball has no skeleton. An empty effects anchor supports later effects without baking them into the texture.

See [the integration and validation report](../../Docs/BASKETBALL-BALL.md) for runtime behavior and remaining gameplay work, and [the current measured APK budget](../../Docs/BUILD-SIZE.md). Pickup, dribbling, player shooting controls and scoring are a subsequent gameplay stage.
