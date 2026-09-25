# Rally Court visual review

Refined modular basketball environment, based on the mood and palette of `../basketball-cartoon-concept.png`, with original geometry and graphics.

| Image | Purpose |
| --- | --- |
| [01 — Court hero](01_Court_Hero.png) | Player-height material and architectural read |
| [02 — Bowl overview](02_Bowl_Overview.png) | Full court, both hoops, two decks, roof and courtside layout |
| [03 — Hoop detail](03_Hoop_Detail.png) | Rounded backboard, rim/net, padding, support frame and badge |
| [04 — Gallery detail](04_Gallery_Detail.png) | Molded seating, arched concourse, rails, banner print and sconces |
| [05 — Unity court](05_Unity_Court.png) | Actual URP import with a simple review lighting rig |
| [06 — Windows player](06_Windows_Player.png) | Integrated Rally arena in the rebuilt WindowsFinal executable, with athlete and gameplay HUD |
| [07 — Corrected apron](07_Apron_Fixed.png) | Runtime camera-rotation check after separating the teal floor from the dark structural foundation |

Images 01–04 are Blender Cycles art renders, not gameplay captures. Image 05 uses the imported Unity assets. Image 06 is captured from the running WindowsFinal executable. Lighting is intentionally authored separately from the reusable geometry.

The editable source is `ArtSource/Basketball/Rally/Arena_Rally.blend`; the assembly prefab is `Game/Assets/_Game/Prefabs/Basketball/Rally/RallyArena.prefab`; the review scene is `Game/Assets/_Game/Scenes/RallyArenaReview.unity`.

See [the asset guide](../../../ArtSource/Basketball/Rally/README.md) for module roles, pivots, recoloring, textures, export commands, LOD counts, verification, and integration limits.
