# Lagoon Island — five-player Blender plan

Implementation note: the environment is now built. See the [modular source guide](../../../ArtSource/Fishing/Lagoon/README.md) for the editable assembly, 16 module libraries, exports and executable entry point. The specification below is the original design brief; the source guide records implemented dimensions and structure.

This is the selected concept direction, revised from four to five players. [Current map reference](map-02-lagoon-island-5p.png). The image is generated concept art, not a completed Blender render. This specification supersedes the four-deck lagoon proposal in the initial exploration.

## Layout

- Exactly five identical inward-facing fishing decks, arranged at 72° intervals around one shared circular lagoon.
- One continuous sandy walking loop connects all five approaches. One separate northern footbridge crosses the sea inlet; it is not a sixth fishing station.
- Proposed island footprint: approximately 120 × 110 m. Proposed lagoon diameter: 56 m. These are blockout targets, not measurements from the image.
- Each deck: 6 m wide × 7 m long, same surface elevation around 1.2 m above sea level, open casting face, and matching approach geometry.
- Use an approximately 4 m clear walking path and bridge width, plus an 8 × 6 m clear staging area behind each deck. Keep approach ramps gentle and collision smooth.
- Keep palms and higher limestone groups on the outer rim. Preserve open views of the water and other stations.
- Color accents: teal, coral, yellow, violet and blue on small post caps only. Station numbering belongs in the authoring data, not visible map graphics.

## Precise blockout placement

Use metres. Blender X is east, Y is north, Z is height; sea level is Z = 0. Lagoon center is (0, 0). Bearings below are clockwise from north. All decks point toward (0, 0). The northern inlet is at bearing 0°, halfway between the yellow and violet stations.

Set each deck origin at the midpoint of its shore attachment, radius 28 m. The deck extends 7 m inward, so its center lies at radius 24.5 m and its casting edge at radius 21 m. Keep the lagoon shoreline circular at the five attachments; make the outer coastline and decorative rock groups organic.

| Station | Accent | Bearing | Shore attachment X, Y (m) |
| --- | --- | --- | --- |
| 01 | Teal | 180° | 0.00, -28.00 |
| 02 | Coral | 252° | -26.63, -8.65 |
| 03 | Yellow | 324° | -16.46, 22.65 |
| 04 | Violet | 36° | 16.46, 22.65 |
| 05 | Blue | 108° | 26.63, -8.65 |

Generate anchors from the bearings rather than rounded table values: X = radius × sin(bearing), Y = radius × cos(bearing). Place five spawn anchors at radius 32 m, five standing anchors initially at radius 22.5 m, and five inward cast-direction anchors. Give each spawn a clear approach to its own deck. All five standing anchors use the same height and distance from the casting edge.

The image communicates the intended composition. The radial construction above is the authority for equal spacing; do not infer precise positions from perspective pixels. Test the actual avatar scale, camera distance, fishing reach and controller slope limits during blockout, then change the shared dimensions together.

## Blender asset structure

Create one editable deck master, then five linked instances with per-station accent materials. Keep geometry identical. Use the existing rounded golf rock/palm/flower language and broad painted materials. Build the land ring from inner and outer outlines with the north inlet left open, a separate seabed, separate water surface and a shallow-water transition.

Suggested collections: Terrain, Rocks, Foliage, FishingDecks, InletBridge, Water, Collision, GameplayAnchors, AssetKit and Review. Export terrain in five or ten spatial sectors; split the northern section around the inlet as needed, with matching border vertices. Keep review lights/cameras and reusable unplaced masters out of game exports.

Required modules: one terrain layout, one deck master instantiated five times, one inlet bridge, shared limestone and palm variants, flower/bush patches, ocean/shallow-water surfaces and foam strips. There is no need for a building, waterfall or additional pier.

## Fairness and collision

Equal geometry supports a fair layout but does not establish gameplay balance by itself. At integration, give all stations equivalent reachable fishing areas, fish availability and bite rules. The inlet must not give its nearest players an automatic catch-rate advantage. If fish are shared, check access from all five positions with the actual casting range rather than putting every fish at the lagoon center.

Use smooth simplified deck and bridge colliders, terrain collision and simple rock/palm obstacles. Keep casting fronts clear of posts, rails and decorative rocks. Boundary colliders that stop players entering water must not also block rod cast queries. Keep at least 3 m of clear camera/rod space behind each standing position, then verify the real camera sweep. No collision on flower petals or palm fronds.

## Build sequence

1. Block out the lagoon, five identical decks, five spawn approaches, sandy loop and inlet bridge. Add an existing-avatar scale proxy at all five stations.
2. Review from a top-down camera and all five player viewpoints. Verify exactly five stations, equal spacing/height/range, unobstructed sightlines, continuous routes and clear cast paths.
3. Finish one deck and one shore slice; validate its cartoon materials beside the golf island, then instance and dress the rest.
4. Export FBX meshes, texture PNGs, collision proxies and a layout manifest with five spawn/stand/fishing-area records. Validate Blender-to-Unity scale and axis conversion using a test marker; do not assume the golf art's 180° correction applies without checking.
5. Audit the saved master and runtime scene. Check all five stations in use together and profile representative water, vegetation and camera views on the target device.

Use the [shared modeling plan](BLENDER-MODELING-PLAN.md#shared-asset-kit) for the asset kit, material workflow and provisional performance budgets. The five-player layout here overrides the earlier four-player assumption.

Proposed later source: ArtSource/Fishing/Lagoon/Island_Lagoon_5P.blend, with a fishing-only generator/exporter and layout manifest. These are planned modeling deliverables; this redesign creates only the image and plan.

[Exact built-in imagegen prompt](LAGOON-5P-PROMPT.md).
