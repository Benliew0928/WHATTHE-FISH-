# Fishing — map-only concepts and Blender plan

## Implemented modular environment

The five-player lagoon now has an editable [Blender assembly](../../../ArtSource/Fishing/Lagoon/Island_Lagoon_5P.blend), 16 separate module libraries and FBX/prefab exports. See the [source and build guide](../../../ArtSource/Fishing/Lagoon/README.md). The Windows player includes Fishing → Explore offline. This is environment-only work, without fishing gameplay. Actual Blender asset renders are in [Blender/](Blender/); the imagegen pictures below remain concept references.

## Current direction — five-player Lagoon Island

The user selected the lagoon concept and requested five players. [Approved map-only concept](map-02-lagoon-island-5p.png) has five inward fishing decks and one separate inlet bridge. See the [five-player Blender plan](LAGOON-5P-BLENDER-PLAN.md) for the original specification and [imagegen prompt](LAGOON-5P-PROMPT.md) for its generation brief. The implemented source guide above records the actual module structure.

## Earlier map exploration

The current direction is **environment design only**: island maps and a Blender modeling plan. These three aerial images supersede the earlier gameplay UI mockups below. Generated with the built-in imagegen tool, they contain no interface or characters and are concept references rather than renders of completed Blender assets.

| Map | Layout | Proposed footprint |
| --- | --- | --- |
| [1 — Shoreline Island](map-01-shoreline-island.png) | Open grassy island, broad beach, three outward piers and connected walking paths | 140 × 105 m |
| [2 — Lagoon Island](map-02-lagoon-island.png) | Central saltwater lagoon, four inward fishing decks and one inlet bridge | 110 × 100 m |
| [3 — Three-Cove Island](map-03-three-cove-island.png) | Three distinct coves, central hill, three piers, two bridges, arch and cascade | 150 × 130 m |

Dimensions are proposed blockout targets, not measurements from the images. See the [Blender modeling plan](BLENDER-MODELING-PLAN.md) for each layout, the reusable asset kit, scene organization, materials, collision, export and review stages. [Map prompts](MAP-PROMPTS.md) record the exact imagegen edit prompts.

The earlier options below are retained as exploration history. The selected direction is now the five-player Lagoon Island linked above.

## Earlier exploration — superseded UI mockups

Generated 26 September 2026 with the built-in imagegen tool. Exploratory UI and environment mockups for selection, not implemented gameplay or production assets. The existing cartoon visual direction informs the chunky silhouettes, shared athlete, cream panels, teal controls, rainbow accents, and island scenery. Full generation prompts are in [PROMPTS.md](PROMPTS.md).

All three propose a timed multiplayer match where each successfully landed fish adds one to the catch count. Most fish wins. Four players and the displayed names, counts, and timers are illustrative.

| Concept | Proposed experience | UI emphasis |
| --- | --- | --- |
| [1 — Shoreline Sprint](01-shoreline-sprint.png) | Roam beaches and piers, choose a spot, cast and catch. Closest to the golf island's relaxed third-person presentation. | Large cast action, movement joystick, vertical standings and personal catch total. |
| [2 — Lagoon Arena](02-lagoon-arena.png) | Competitors fish from four nearby decks around a saltwater lagoon. Focus on fishing skill with opponents in view. | Horizontal score tiles and a reel timing meter. |
| [3 — Hotspot Hunt](03-hotspot-hunt.png) | Move around the island to find active fish schools; choose when to stay and when to run to another cove. | Minimap, school markers and alerts, sprint and cast actions. |

Recommendation: Hotspot Hunt makes the island layout part of the competition. Lagoon Arena offers the most contained first implementation; Shoreline Sprint offers the most relaxed presentation. Select a direction before detailed mechanics or implementation.

The generated scenery and UI are visual proposals. Final geometry, school markers, map consistency, labels, controls, and interaction states should be resolved during implementation.
