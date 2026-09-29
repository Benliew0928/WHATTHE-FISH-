# Sky-Sail Circuit

Implemented 29 September 2026 in Unity 6000.3.20f1. The four existing islands form one visible coastal world: **Football ↔ Golf ↔ Basketball ↔ Fishing ↔ Football**. The existing games retain their original local coordinates and art.

## Play

Run `Builds/WindowsFinal/Explore-SkySail.cmd` to start beside the football station. The regular menu and sport launchers still work. Walk to a shore station, then choose either neighboring island on its travel card. Windows also supports E for the clockwise destination. A ride cruises for 44 seconds, with door closing and docking time on either end.

Look around with right mouse drag or the existing touch look control. C / Camera switches between cabin, chase and elevated panorama views. Walking resumes on the destination's boarding deck. Each stop disembarks the party; select the next leg to continue.

In a room, only the host selects the destination. Every player must be at the station. Everyone boards the same cabin, waits for every connected client to prepare the destination, and disembarks on the same island. There is no simultaneous independent game occupation. Existing destination capacities still apply: a party larger than five cannot enter Fishing and stays together at its departure station.

## Art modules

Editable sources: `ArtSource/SkySail/`. Runtime prefabs: `Game/Assets/_Game/Prefabs/SkySail/`.

| Module | Details |
| --- | --- |
| CabinShell | Rounded frame, paneled teal body, individual timber floorboards, curved roof and skylight |
| CabinInterior | Ten upholstered seats, armrests, grab poles and hanging grips |
| SlidingDoor | Independent framed glass door; two animated instances |
| CabinHanger | Bogie, cable grip, wheel hubs, spokes and roof supports |
| CableTower | Braced frame, stone foundation, ladder, service platform and sheave wheels |
| SailStation | Supported timber boardwalk, fabric canopy, benches, lanterns and local sign |
| Gangway | Reusable walkway sections with matching collision and guardrails |
| Cable spans | Four separate route prefabs generated from the same paths used by the cabin |

Modules share the existing coastal material family, including 2K timber, teal and bronze maps. Unity combines each module by material and provides a simplified distant version. The occupied cabin always retains its detailed version. Passenger avatars use fixed standing anchors and their existing idle animation; a seated animation has not been authored.

The shared ocean uses shoreline color/depth fields, shallow-water color, animated surface highlights and foam. One sky, sun and cloud field connects every view. Distant islands are simplified **3D meshes derived from the actual terrain and venue geometry**, with baked vertex colors and two levels of detail. They preserve parallax during a ride and avoid retaining the full island material libraries.

## Scene and network lifetime

Bootstrap owns the scenery, stations, cables, cabin, camera, UI and room services. Four additive scenes own the detailed game environments. `SportDefinition.environmentPrefab` no longer directly retains these detailed prefabs.

At departure, the destination scene is loaded asynchronously but kept inactive. The host waits for sequence-specific readiness acknowledgements from every client. A preparation timeout returns the group to the source station. At 27% of the ride, departure detail is replaced by its proxy, its scene unloads, and unused assets are released. At 83%, the destination detail becomes visible. Docking rebases the common scenery around the new island before restoring walking.

Only one detailed island is rendered at a time; both source and destination can be resident during preparation. This temporary memory peak still needs measurement on the target phone. Loading a whole additive scene can cause activation/upload work; asynchronous loading alone is not a guarantee of stutter-free mobile travel.

The server owns the journey sequence, route, phase and start time. Clients evaluate the same route against server time. Passenger movement is locked during transit, so avatars follow deterministic cabin anchors instead of sending per-frame cabin movement. The server restores normal network transforms at disembarkation. Joining an exploring room remains locked under the existing room rules.

`SkySailMap` defines geographic centers, ports, travel paths and arc-length lookup. The same curve drives cable geometry and cabin wheel contact. `SkySailWorld` manages the party, camera, doors, UI and travel states; `SkySailStreaming` manages island scene lifetime.

## Build and maintain

- `Tools/Build/Build.ps1 -Target Windows` rebuilds the saved Sky-Sail world and Windows player.
- `Tools/Build/Build.ps1 -Target Scene` rebuilds the world scenes without a player.
- `Tools/Build/Build.ps1 -Target AndroidRelease` builds the Android release APK.
- Blender: run `Tools/Blender/build_sky_sail.py` to regenerate the editable kit and FBX files.
- Unity: `SkySailBuilder.BuildWindows` regenerates modules, island proxies and scene assembly. `-skyReuseProxies` keeps previously baked island proxy meshes for layout/material iterations.
- Unity: `SkySailBuilder.RebuildPlayer` recompiles the saved scene arrangement after code-only changes.

For island art edits, update its `SkySail_<Sport>` additive scene and regenerate the world/proxy meshes. The old `ProjectBuilder.Setup` and `RefinedIslandBuildEntry` are legacy scene generators; use the Sky-Sail workflow for the connected world.

## Performance scope

The mobile settings retain a 30 FPS target, 0.9 render scale, 2× MSAA and short shadow range. There is no cable-chain physics simulation or volumetric cloud pass. Far islands, stations and towers use reduced geometry; distant wire thickness is stabilized in the shader. The single occupied cabin keeps its full detail on every platform.

No minimum phone has been selected, and no physical Android device is connected for qualification. The current result is a working implementation with Windows functional and visual evidence, not a verified “ultra” performance guarantee for phones. Sustained phone frame times, peak memory during preparation, thermal behavior and slow-network travel still require device testing.

[Actual game gallery and validation](GameReview/index.html) · [Editable asset guide](../../../ArtSource/SkySail/README.md) · [Selected concept](Concepts/20260927/03-sky-sail-circuit.png)
