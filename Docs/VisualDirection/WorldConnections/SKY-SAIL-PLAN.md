# Sky-Sail Circuit — selected world direction and implementation proposal

Selected by the user: **03 · Sky-Sail Circuit**, 27 September 2026.

Status: historical design proposal. The user subsequently authorized implementation with **one active game and one travelling party per host room**. The implemented system and current validation are recorded in [SKY-SAIL-IMPLEMENTATION.md](SKY-SAIL-IMPLEMENTATION.md). Statements below describe the earlier proposal, rather than the current repository state.

[Selected concept board](Concepts/20260927/03-sky-sail-circuit.png) · [All five studies](index.html)

## Feasibility and performance expectation

Rideable cabins connecting all four islands are feasible. A moving cabin following a fixed route, static cable geometry and modest decorative motion are a manageable design. The likely larger costs are detailed islands, water shading, foliage, shadows, texture memory and loading/activation spikes. Their actual relative costs must be measured on the chosen phone.

Keep the world visually continuous while loading and drawing only the detail needed around the player. A high-quality tropical art style is a realistic goal; matching the generated painting at a particular frame rate on unspecified phones is not a verified promise.

The user has not chosen a target phone. Proposed planning baseline: stable 30 FPS on a representative mid-range Android device; optional 60 FPS for capable devices. This baseline remains a recommendation, not an approved device specification. These imply roughly 33.3 ms and 16.7 ms per frame, with processing headroom for sustained heat and loading. Device RAM alone does not establish suitability.

## Proposed geography

Place football west, golf north, basketball east and fishing south around a broad open sea court. Treat this as the first blockout arrangement, not fixed coordinates. Adjust island rotation and height using the existing models rather than rebuilding them to match the painting's exaggerated cliffs.

Use a perimeter cable circuit: Football ↔ Golf ↔ Basketball ↔ Fishing ↔ Football. Two travel directions keep every destination within two adjacent legs and avoid crossed cables in the middle of the bay. Cabins can stop at each station, allowing the player to disembark or stay aboard. Direct point-to-point shortcuts can be considered later if journey lengths justify them.

Place a station on each shore facing the shared sea, connected to that island's existing walking paths. Cable approaches stay away from playing areas. Give each station the same timber, cream stone and teal sail-canopy family, with local signs and planting. Preserve a clear outward-facing lookout at each one.

Determine spacing from reciprocal shore views, an aerial ride camera and desired travel time. A provisional 20–40 seconds per adjacent ride segment is a pacing test, not an engineering requirement or a measured loading allowance. Longer journeys should be optional rather than forced waiting.

## World representation

Use one shared coordinate layout and three visual scales:

| Representation | Purpose | Contents |
| --- | --- | --- |
| Detailed island | Exploration and close arrival | Playable terrain, local collision, venue detail, vegetation and nearby interactions |
| Approach model | Looking down from a gondola and coming into port | Continuous coastline, terrain, roofs/bowl, large planting groups, major paths and station exterior |
| Horizon model | Views from other islands and across the route | Very simple 3D landform, main venue silhouette, arch/waterfall or palm crown, small material set |

Approach and horizon models should be derived from the actual islands and share their transforms, coastline and major landmarks. Because the gondola changes both angle and elevation, a single flat image is insufficient for the entire journey. True simplified 3D models preserve parallax; any image-based substitutes would be restricted to distances and angles where they hold up.

Fishing's lagoon and five decks become visible from above during a ride. From another shore, its outer limestone shoulders, palm grouping and shelter/arrival station must identify it.

A persistent world scene contains shared sky, sun, sea, fog, the small island models, cable routes, transit stations/cabins as needed, player/camera and world services. The current per-island sea surfaces and global lighting overrides need coordination so they do not stack or change abruptly. The current island origins, hardcoded world-space checks, spawns, cameras and recovery positions need conversion to island-local anchors before relocating the maps.

## Loading during a journey

1. **At a station:** retain the current detailed island. Prepare the selected destination's essential arrival content and check route readiness. Board while stationary, close doors, reserve a seat or standing anchor.
2. **Leaving shore:** move the cabin along the cable path. Replace the receding island's near content with matched approach/horizon models once distant enough; preserve the view when looking back.
3. **Over water:** retain the cabin and world shell, and bring in destination detail in bounded chunks. Prefer the arrival station and essential collision first. Release departure-only content when safe.
4. **Approaching:** switch visual detail at matched boundaries and suitable distances. The silhouette, lighting and coastline should remain aligned. Measure any crossfade cost; long blended overlaps are not mandatory.
5. **Docking:** only open doors and enable walking when station collision, player placement and destination state are ready. On slow loading, wait safely at departure or a controlled approach hold. On failure, report it and recover to a valid station.

Target one detailed island's visual working set in normal use, with lightweight surroundings. Short arrival/departure overlaps and queued data can temporarily increase memory; budget and measure the peak, not only the steady state.

Unity supports asynchronous scene loading, but background data loading does not by itself guarantee a hitch-free activation. Mesh/texture preparation, object activation and asset cleanup must be measured and scheduled. Asset references also matter: the current SportDefinition points directly at environment prefabs, which would need restructuring into identifiers/load handles so permanent world services do not retain every detailed island.

Use additive island scenes or separately loadable asset groups with explicit ownership and release. Addressables is a candidate for managing that lifetime, not a dependency currently installed in the project. Unloading scene objects alone is not proof that texture/mesh memory has been freed. Shared materials stay owned by the world; island-specific assets need proper release and verified memory recovery.

## Making the cabins genuinely rideable

Build fixed, sagging cable paths and move cabins along them at a controlled speed. Match the hanger's path to the visible cable. Wheels, doors, cable motion accents and gentle cabin sway can be animated. Full rope-chain physics is unnecessary for this intended experience.

For the first version, board a stopped cabin using an interaction. Attach the character to a seat/standing anchor, allow free camera looking, and suspend normal ground locomotion until docking. This supports a real visible journey while avoiding unstable moving-platform walking. Walking around inside a cabin can be evaluated separately.

Keep nearby cabins detailed; simplify distant cabins and passengers. Use restrained window transparency or open sides, inexpensive baked/environment reflections, and route-aware cable detail so thin cables do not shimmer or disappear distractingly.

## Mobile quality priorities

Preserve the visual qualities players notice first: shoreline composition, recognizable islands, rich but coherent colors, clean cabin/station models, readable water, consistent sun and haze, and smooth motion.

Spend texture and geometry detail near the camera. Reduce repeated distant seats, small foliage, material count, shadow distance and unnecessary transparency. Use shared materials and compatible batching/instancing where appropriate. Author mobile texture compression and mip levels deliberately; verify appearance on device.

The sea occupies much of the ride view. Start with one efficient ocean treatment, controlled normal/foam animation, depth-color transitions near shores and inexpensive reflections. Avoid assuming that full-scene real-time reflections, several overlapping transparent water layers or extensive screen effects are required for attractive water.

Use simple distant lighting, baked/static shading where it suits the chosen fixed daylight, and local real-time shadows where they improve contact. Quality tiers should preserve art direction while scaling expensive secondary effects. The final visual target should be approved using real Android captures.

## Multiplayer implications

Current networking uses one host-owned WorldSport and activates the same sport for the entire room. It does not yet support players freely occupying different islands.

Recommended first playable slice: one offline route. Next, support a group travelling together with host-authoritative boarding, departure time, route, seat occupancy and docking. Clients derive smooth cabin movement from the shared route and synchronized time; do not transmit rope physics.

Independent travel requires per-player island/transit state, rules for which players and objects each client receives, loading-readiness coordination, destination capacity checks, and simulation of every occupied island. A phone acting as host may need lightweight collision/gameplay data for several occupied islands even when it renders only its own location. This is a separate scaling cost from graphics.

Fishing currently has a five-player environment limit while the room supports up to ten. Travel and arrival rules need to define whether the limit concerns fishing participation or island occupancy; a ten-player party cannot silently be treated as five valid fishing slots.

## Proposed implementation order

1. **World blockout:** place the four existing islands, simple horizon/approach models, station anchors and static routes. Inspect views from all four shores and along the full route.
2. **One complete offline ride:** implement boarding, motion, loading, docking and safe recovery for a single demanding pair such as football and golf. Keep the other two visible as distant models.
3. **Phone qualification:** measure current islands and the complete ride, including arrival/departure peaks, rapid return journeys, looking backward and sideways, loading stalls, memory release and a sustained 15–20 minute heat run. Include scene activation in frame-time analysis.
4. **Finish all four stations/routes:** extend the proven system and refine scenery, cabin materials, sea and sound. Recheck views at actual phone resolution.
5. **Multiplayer travel:** group travel first; independent simultaneous island occupation after capacity, authority and memory requirements are settled.

Do not build all four detailed cable stations before the loading and ride prototype proves that the desired view and performance can coexist.

## Evidence inspected

- SportEnvironmentController.Activate toggles roots with SetActive; all four environments remain part of the current Bootstrap composition.
- SportDefinition contains a direct environmentPrefab reference plus spawn/camera/environment settings.
- AppRoot sets a 30 FPS mobile target. NetworkAthlete and AppRoot use one host-owned WorldSport.
- MobileURP currently specifies 2× MSAA, render scale 0.9 and 65 m shadow distance; desktop coastal overrides explicitly skip mobile.
- The stadium report gives roughly 3.60–5.27 ms mean frame times on an RTX 4050 Windows development player at 1080p.
- Golf/fishing reports give roughly 2.05–2.43 ms means in their Windows workload. These are not Android measurements.
- The recorded Android APK is from 25 September and predates the latest 27 September G2/L2 delivery; its size and successful build do not qualify the current proposed world on phones.

Some older architecture prose describes earlier flat/indoor environments. Current source and the latest art delivery reports were used where they differ.

## Unity references

- [Unity 6.3: asynchronous scene loading](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html)
- [Unity 6.3: scene unloading and the asset-memory caveat](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.SceneManager.UnloadSceneAsync.html)
- [Unity 6.3: URP performance settings](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/configure-for-better-performance.html)
- [Unity 6.3: level of detail](https://docs.unity3d.com/6000.3/Documentation/Manual/LevelOfDetail.html)
- [Unity 6.3: Profiler target selection](https://docs.unity3d.com/6000.3/Documentation/Manual/ProfilerWindow.html)
