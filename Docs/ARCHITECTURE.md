# Architecture and extension points

## Runtime boundaries

| Area | Responsibility |
|---|---|
| Shared/Core | SportDefinition, SessionConfig, cosmetic data, persistence, development probes |
| Shared/Player | CharacterController motor, shared PlayerCommand input, Blender rig/material customisation, cameras |
| Shared/UI | Main menu, profile, sport selection, waiting room, stadium settings, touch controls |
| Shared/Networking | MPS room lifecycle, Relay setup, capacity/phase approval, NGO players and authoritative movement |
| Shared/Platform | Reserved for Android/Huawei services; IPlatformGameServices currently has an explicitly unavailable adapter |
| Sports/Football | Stadium visuals and signage; football gameplay is deferred |
| Sports/Basketball, Sports/Golf | SportDefinition assets marked unavailable; no placeholder environments |

`AppRoot` composes the prototype and owns navigation. `RoomService` is the `IRoomService` implementation. `ISportMode` and `IPlayerCommandSource` provide boundaries for future loaders/rules and bot commands; they are not implementations of the deferred sports. All current players explore one static Football scene. There is no dynamic stadium resizing.

## Network flow

1. UGS initialises only when an online action is selected. Anonymous authentication provides a player identity.
2. MPS creates a private session with ten places and Relay networking, or joins by session code.
3. NGO approves connections only while waiting and below ten connected players.
4. Each owner sends a clamped movement vector, heading and sprint flag at 30 Hz. Only the host simulates CharacterController movement on the fixed timestep. Stale input expires after 0.25 s. Clients receive server NetworkTransform updates and interpolate them. There is no client prediction yet, so high latency can affect responsiveness.
5. Each player's server-written cosmetic and readiness NetworkVariables update the waiting room and athlete. RPC ownership restricts players to their own avatar.
6. The host player's server-written stadium preset and phase variables are shared world state. The host locks the MPS session before starting exploration. NGO approval provides a second phase/capacity gate.
7. Returning to the waiting room unlocks joining. Explicit host exit deletes the session. Disconnect/session deletion/host replacement returns clients to usable UI. The application does not elect a new gameplay host.

Only cosmetic IDs, short text, flags, movement and state are transmitted. The bundled FBX meshes are never sent across the room connection. Shared world authority assumes a trusted player host; anti-cheat, prediction and dedicated servers are future work.

## Art/performance

Standard pitch: 68 × 105 metres. Four covered stands use repeated seats, terrace blocks, stair treads, posts and trusses. Meshes are consolidated by shared material during export to reduce renderers/draw calls. The generator retains reusable module functions and linked seat geometry during assembly; the final `.blend` is an editable combined mesh by material. Flags remain separately togglable.

The athlete uses one small rig, three looping actions, one collision capsule and two hair silhouettes (classic cap or cap plus tuft). Appearance never changes collision size. Collider meshes exist only on walkable/structural stadium surfaces. First-person view hides head renderers. Third/elevated camera casts against the stadium layer to avoid walls.

URP uses a directional light, ambient fill, restricted shadow distance, 2× MSAA and a 30 FPS frame target. Device FPS and thermal behaviour are measured requirements, not guaranteed by those settings. No audience, crowd audio, downloaded media or custom mesh uploads are included.

## Next milestones

- Add a sport-loading service and scene/content mapping when the second sport is built.
- Football/basketball rule modules: 1v1–5v5 team assignment, host settings, timed rounds, ball authority, scoring and a bot IPlayerCommandSource.
- Golf: separate walking/aiming/shot flow and island terrain authored in Blender.
- Huawei: implement authentication/game services and achievements/results behind IPlatformGameServices after checking competition and AppGallery requirements. Current builds do **not** claim HMS integration.
- Improve high-latency movement with input sequencing, client prediction and reconciliation before competitive play.
- Add accessibility scaling, remappable desktop controls and broader Android aspect-ratio testing.
