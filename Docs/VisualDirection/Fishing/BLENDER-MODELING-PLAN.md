# Fishing island — Blender modeling plan

**Current selection:** Lagoon Island, revised for five players. The [five-player specification](LAGOON-5P-BLENDER-PLAN.md) supersedes the four-player lagoon layout and assumptions below. Retain this document for the shared asset, material, collision and export workflow; the other map layouts are exploration history.

## Scope and visual target

Three alternative map concepts for selection. The current deliverables are environment reference images and this modeling plan; no fishing Blender model has been built yet. The earlier UI mockups are superseded by the map-only views. Model one chosen layout after selection, using the common asset kit below.

Match the golf island's rounded limestone, broad sculpted palms, cream beaches, lime turf, cyan shallows, deeper blue ocean, and sparse coral/yellow/violet flowers. Use soft asymmetry, broad painted color variation and readable silhouettes. Keep paths, casting edges and water easy to see from the gameplay camera. Images establish mood and composition; this plan supplies proposed build dimensions. Images are not measured blueprints.

Assume four competitors for the initial blockout. Match duration, movement speed, camera and fishing reach are unresolved. All dimensions and geometry budgets below are starting targets to validate, not established game requirements.

## Compare the maps

| Option | Reference | Approximate land footprint | Main geometry | Modeling effort |
| --- | --- | --- | --- | --- |
| 1. Shoreline Island | [Map](map-01-shoreline-island.png) | 140 × 105 m | Low solid island, broad south beach, three outward piers, coastal loop and center shortcuts | Lowest |
| 2. Lagoon Island | [Map](map-02-lagoon-island.png) | 110 × 100 m | Horseshoe island, 45 m lagoon, four inward decks and one inlet bridge | Moderate |
| 3. Three-Cove Island | [Map](map-03-three-cove-island.png) | 150 × 130 m | Central hill, three coves, three piers, two bridges, sea arch and cascade | Highest |

Recommendation: option 2 for a focused competitive fishing venue. Its four fishing areas share visible water and use the same deck module. Option 3 offers the strongest exploration map and the richest landmark composition. Option 1 is closest to the open, grassy golf island.

## 1 — Shoreline Island

**Layout.** Orient north toward the far side of the overview. Make a rounded bean-like outline, a broad crescent beach on the south, and smaller accessible beach pockets to east and west. Keep the middle open grass. Use a low northern rise as a visual landmark, with most walking ground around 1–3 m above water and the rise around 6 m. Place three outward fishing piers at the west, east and north shores. The south beach is a fourth fishing area of comparable usefulness.

**Route.** A continuous 4 m wide coastal path connects all four areas. Two center shortcuts prevent the north shore becoming a long compulsory walk. At each fishing area, reserve at least an 8 × 6 m clear approach and a flat edge. Keep rocks and palms behind or beside the casting area. Start with four spawn anchors at comparable distances from their nearest fishing area; adjust using measured travel time.

**Blender construction.** Start with a filled irregular outline and an editable low-density quad/grid surface. Shape beach, grass and hill as one continuous terrain surface, then split the export into 8 spatial sectors with matching edge vertices. Paint the path into the terrain color map; do not stack a coplanar path mesh on top. Make one straight pier module and instance it three times. Concentrate rock dressing on the north rise and corners of the beach, leaving castable coastline open.

**Distinct asset needs.** Three piers, one simple grass hill, a small north rock group. No bridges, arch or waterfall required. This is the quickest way to establish the fishing environment's materials and scale.

## 2 — Lagoon Island

**Layout.** Build a horseshoe-shaped land ring around a roughly 45 m wide lagoon, with an 8–10 m northern inlet connecting it to the sea. Bridge the inlet to complete the walking loop. Place equal fishing decks at northwest, northeast, southeast and southwest inner shores. Start each deck at roughly 6 m wide × 7 m long. The outer land silhouette can be asymmetrical, while inner deck heights and casting distances remain comparable.

**Route.** Use a 4 m wide inner-rim path and a bridge with 4 m clear walking width. Provide gentle approach ramps to the decks, even if the render suggests small steps. Separate spawn anchors behind the four decks. Preserve a clear view across the lagoon; place taller rock groups and palms on the outer rim. Fishable areas should be reachable from each deck at the actual rod range; the entire lagoon does not need to be castable from every position.

**Blender construction.** Create paired outer/inner outline loops, leave the north channel open and connect the remaining ring with quad strips. Add support loops for the beach shelf and gentle raised outer rim. Do not fill the lagoon with a land cap. Use a shallow seabed below a separate water surface. Split the terrain into four or eight sectors. Build one deck asset with origin at its shore attachment and rotate/instance it four times. Make one short bridge, with collision spanning smoothly between both landings.

**Distinct asset needs.** Four equal inward decks and one bridge; no waterfall or sea arch required. Model outer knolls as soft terrain or rock clusters rather than tall cliffs. Equal access and open sightlines matter more than precise radial symmetry.

## 3 — Three-Cove Island

**Layout.** Use a compact triangular land silhouette around a central hill. The southwest Palm Cove has a broad sand beach, the southeast Cascade Cove has a calm fishing pocket beside a decorative waterfall, and the north Arch Cove has a limestone arch framing the sea. Start the main hill around 14 m tall and the tallest decorative rock shapes around 20 m. Give each cove one compact pier and two open bank areas, so there are multiple places to stand.

**Route.** A 4 m wide coastal loop links all three coves. Exactly two short bridges cross rocky water cuts along the loop. Add a gradual saddle shortcut across the interior, avoiding mandatory jumps or stair climbing. Let landmarks differ visually while keeping useful fishing access comparable. Use four distributed spawn anchors on the loop and tune their travel times; with three coves, spawn fairness needs an explicit check rather than one spawn per cove.

**Blender construction.** Block out the three bays and low route before raising the central hill. Use terrain patches around the coves and hill, with continuous matching borders; avoid copying the golf island's radial mesh unchanged into concave coastlines. Build the arch from chunky tapered rock sections and a separate simplified collider. Build the waterfall as a curved ribbon mesh plus a separate small foam/splash mesh. Keep its pool/stream visually connected to the source and ocean. The waterfall is scenery beside the fishing pocket, so it does not hide fishing activity. Use one pier module three times and two length variants of the bridge module.

**Distinct asset needs.** Three piers, two bridges, a sea arch, a waterfall ribbon, a small pool and three landmark dressing groups. Fish-school locations remain runtime data; do not bake floating symbols or glowing target rings into the environment mesh.

## Shared asset kit

| Asset | Authoring approach | Initial variants |
| --- | --- | --- |
| Terrain and seabed | Editable terrain surface, spatial export sectors, shallow underwater shelf | One selected layout |
| Limestone | Rounded bevels and gentle asymmetry; broad shaded faces | 6 rocks plus 2 cliff modules |
| Palm | Curved tapered trunk and grouped broad fronds | 3 silhouettes |
| Plants | Simple bushes, sparse grass tufts and large flowers | 2 bushes, 2 tufts, 3 flower colors |
| Timber pier | Chunky plank blocks, supports and side posts; open outer casting face | 1 master, optional wider end |
| Bridge | Clear walkable deck, side ropes/posts, smoothly connected landings | 1 master, 2 lengths if needed |
| Water | Separate ocean, shallow color transition and shoreline foam | One shared set |
| Landmarks | Only those required by selected layout | Arch and cascade for option 3 |

Use linked source meshes for repeats. Keep a master kit collection separate from placed instances. Reuse or adapt the existing Tidebloom palm, rock and flower construction where practical; author a new terrain outline and fishing furniture. Do not run the golf generator to create fishing: it writes live golf outputs and the shared asset register.

## Blender scene organization and materials

Work in metres, with Blender X/Y as the plan and Z as height; set sea level to Z = 0. Use a source hierarchy such as `Fishing_Map_01` containing `Terrain`, `Rocks`, `Foliage`, `Piers`, `Bridges`, `Water`, `Landmarks`, `Collision`, `GameplayAnchors`, and `Review`. Put reusable source assets in `AssetKit`. Keep render cameras, lighting and scale proxies in `Review`, excluded from game export.

Name repeated assets and placed instances consistently, for example `Pier_A`, `Pier_A_01`, `COL_Pier_A_01`, `Spawn_01`, `FishingArea_01` and `ShoreBoundary_01`. Use attachment-point origins for piers/bridges, base origins for rocks/palms, and a stable map origin for terrain sectors. Apply object scale before final export and retain editable masters for modifiers.

Start with a 2048 px terrain color map and shared 1024–2048 px prop atlases as provisional budgets. Use broad color gradients, simple plank marks and oversized foliage forms. Separate turf, sand, stone, wood, foliage and water material families while sharing textures. Pack textures into the master and also export PNGs. Match the actual runtime material in engine; Blender procedural nodes or offline water appearance will not automatically transfer. Water animation and fish movement belong to runtime work, separate from the static map model.

## Collision, visibility and provisional budgets

- Model clear paths at least 4 m wide; reserve about 3 m behind fishing positions for the avatar, rod and camera, then adjust to the real camera sweep. Use gentle ramps initially, aiming below 15 degrees until checked against the controller.
- Use simplified terrain collision, box-like pier/bridge collision and capsule/low-detail rock obstacles. Decorative leaves and flowers need no collision. Keep smooth collider transitions at bridge and pier landings.
- Define shoreline boundaries independently from the visual water. Keep invisible player barriers out of the rod's cast query, using dedicated layers when implemented. Whether swimming is supported is unresolved; do not assume the water surface is walkable.
- Initial whole-map LOD0 budget: about 150k–250k environment triangles excluding avatars, fish and distant backdrop. This is a planning target, not a performance guarantee. First measure a dressed slice on the target device, then revise.
- Add lower-detail rock, palm and landmark versions; batch or instance foliage in spatial patches. Cull decorative distant clusters. Keep terrain sectors independently cullable. Limit overlapping transparent water/foam layers and inspect their cost in engine.

## Build order and review gates

1. **Measured blockout:** model only coast, terrain heights, paths, fishing pads, bridges and simple water. Add scale proxies matching the existing avatar. Export an early review scene and check every fishing area can be reached, views stay clear, paths have no snag points, and spawn-to-water travel times are comparable.
2. **One finished slice:** finish one beach, one pier, a rock group, a palm group and the shallow-water transition. Compare it beside the golf environment under equivalent lighting. Resolve style, camera scale and runtime water before dressing the full island.
3. **Complete layout:** replicate the kit, dress all routes, and add only the selected map's landmarks. Keep fishing edges uncluttered. Review from a top-down camera, an aerial camera and actual third-person fishing positions.
4. **Game export:** provide FBX meshes, PNG textures, named material slots, collision proxies, spawn/fishing-area anchors and a layout manifest. Verify scale and axis conversion using an asymmetrical test marker before final export. Tidebloom currently needs a 180-degree art-child correction after FBX import; verify the chosen fishing export pipeline rather than applying or dropping that correction blindly.
5. **Delivery audit:** reopen the saved master, check packed textures, normals, UVs, transforms, collection names, exported bounds and manifest consistency. Validate connected routes, dry spawns, cast clearance and unobstructed camera views. Profile the representative dressed scene on the target device before calling it production-ready.

Proposed later deliverables: `ArtSource/Fishing/<SelectedMap>/Island_<SelectedMap>.blend`, a reusable kit collection, a deterministic fishing-only build/export script if procedural generation is chosen, a layout manifest, exported game meshes/textures, a Unity review scene, and top-down/aerial/three gameplay-position renders. These are planned files, not files created by this concept task.

## Local pipeline references

- [Tidebloom authoring guide](../../../ArtSource/Golf/Tidebloom/README.md)
- [Existing golf generator](../../../Tools/Blender/build_golf.py)
- [Golf Unity builder](../../../Game/Assets/_Game/Editor/GolfBuilder.cs)
- [Shared visual direction](../README.md)
- [Map generation prompts](MAP-PROMPTS.md)
