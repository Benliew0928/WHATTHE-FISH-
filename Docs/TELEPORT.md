# Blue-light teleport and room travel

The reusable character effect now supports real room travel and post-match
choices, alongside the offline preview. Room entry also descends from the sky after
loading; see [spectator places and initial arrivals](SPECTATORS.md). Everyone remains in the same room after
a match. The existing shared Sky-Sail journey remains available.

## After a match

After dismissing the football/basketball honours or fishing results, the next
adventure card appears. Golf opens it when the round ends. The host can choose
**Play again**, or choose an island and **Skyrail station / Game area**, then
**Teleport everyone**. Guests see a waiting card; fishing guests can mark
themselves ready for the next round. **Keep exploring** closes the card and
**Next adventure** reopens it while the completed result remains current.

Travel always includes every human in the room. Match bots are removed when
travelling, and are retained for a replay on the same island. Island cards show
playing capacity; fishing is unavailable to groups with more than five players.
Up to ten additional spectators travel with them. An active match,
another teleport or a Skyrail ride blocks new travel requests. A replay uses the
game's existing team and readiness rules; fishing Cup advances to its next round.

The small travel card remains visible while the camera moves. The host can
cancel with its button or Escape. Guests cannot choose destinations, cancel the
group's journey or start a match.

## Preview

Run `Builds/WindowsFinal/Preview-Teleport.cmd` or
`Tools/Build/Preview-Teleport.ps1`. Walk to any starting location,
then select **Station** or **Game** for an island. Enable the full-capacity option
to see simultaneous arrivals. **T** hides the preview controls; **Escape** cancels
an active preview and restores the source location. The preview can also be
launched with `WhatTheFish.exe -offline -teleportPreview`.

The preview is explicitly offline and rejects requests during an active match,
another journey or a results presentation. Normal play does not show these
development controls. It does not start a match or disconnect a room.

## Animation and camera

1. **Charge — 1.00 second:** two blue floor rings appear, with a translucent
   light column and rising spiral lines. The camera eases out to show the body,
   including when starting in first person.
2. **Absorb — 0.65 seconds:** a luminous edge dissolves the character upward
   while a small lift gives the impression of being drawn into the light.
   Hair, head and clothing use the same effect. Sports equipment is stowed
   through the existing transit state before the effect begins.
3. **Journey — 2.4–6 seconds:** the character is invisible. The camera climbs,
   travels over the visible world, and descends at the destination. The source
   and destination details remain loaded until arrival; there is no blackout.
   A temporary view of the existing cabin keeps the destination station complete
   during the approach, sharing its meshes and materials and disappearing when
   the original cabin takes over at the same position.
4. **Arrival — 1.30 seconds:** the rings and column reveal each character at
   their assigned position, then fade away.
5. **Camera return — 0.80 seconds:** the arrival view blends into the player's
   selected gameplay camera. Controls and the game HUD return.

Animation progress is capped at 50 ms per rendered update. A long loading or
rendering stall lengthens the sequence instead of jumping the camera forward;
the timings above are nominal durations at 20 FPS or higher.

The effect uses shared procedural meshes and two small shaders, without new
textures, skeletal clips, audio files or packages. Original character materials
and renderer state are restored when the effect ends or is cancelled.

## Landing positions

All coordinates in `TeleportDestinations` are relative to the destination island,
independent of the current floating world origin. The game uses the same ordered
roster for the whole operation; there is no wrapping of excess players into
occupied slots.

| Island | Capacity | Station | Game |
| --- | ---: | --- | --- |
| Football | 10 | Two rows on the boarding deck | Two rows inside the pitch, facing inward |
| Basketball | 10 | Two rows on the boarding deck | Two rows inside the court, facing inward |
| Golf | 10 | Two rows on the boarding deck | Two rows at the existing southern island arrival area |
| Fishing | 5 | One row on the boarding deck | One player at each of the five authored fishing positions |

Station slots have 1.8 m lateral spacing and 1.4 m between rows, with two
additional rows reserved for spectators. Football uses
2.4 m lateral spacing; basketball uses 1.8 m; golf uses 2 m with 3 m between rows.
Fishing slots follow the five piers around the lagoon and face the water.

Before the charge begins, every requested slot is projected onto actual walkable
ground and checked for capsule clearance and separation. If any slot is blocked,
the entire request fails while everyone remains at the source. A group with more than
five playing participants cannot teleport to fishing; spectator places are separate. Cancellation restores source positions,
colliders, input, camera and materials.

## Integration and verification

- Presentation: `Game/Assets/_Game/Shared/Player/TeleportEffect.cs`.
- Destinations: `Game/Assets/_Game/Shared/Core/TeleportDestinations.cs`.
- Shared presentation: `Game/Assets/_Game/Shared/Core/TeleportSequence.cs`.
- Room authority and barriers: `Game/Assets/_Game/Shared/Core/RoomTeleport.cs`.
- Post-match choices: `Game/Assets/_Game/Shared/UI/PostMatchFlow.cs`.
- Preview: `Game/Assets/_Game/Shared/Core/TeleportPreview.cs`.
- Candidate builder: `RoomTeleportBuild.Build`.
- Runtime suites: `Tools/Build/Test-Teleport.ps1` and `Tools/Build/Test-RoomTeleport.ps1`.
- Room evidence belongs under ignored `Builds/RoomTeleportQA/`; preview evidence
  remains under `Builds/TeleportQA/`.

Room protocol **64** includes a host-owned request and an owner-authorized
acknowledgement on each player. A stable ordered roster is checked throughout.
Each peer loads the destination and validates all slots before acknowledging;
the host schedules departure after everyone is ready. Travel pauses at the
approach until every peer reaches it, and controls return only after every peer
finishes arriving. Network transforms are suspended during presentation and
the server publishes the final poses before movement resumes. Stale or
out-of-phase acknowledgements cannot advance another trip.

A failed landing plan, changed roster, disconnected host, cancelled request or
90-second phase timeout restores the source poses, materials, camera, controls
and network transforms. Matching protocol builds are required.

## Cabin lighting

Cabin bulbs use a shared warm unlit material, and do not cast or receive shadows.
They render after the opaque ambient-shading pass and before glass, with solid
colour and depth testing, so the fullscreen ambient pass cannot darken them.
The cabin camera temporarily suppresses screen-space contact shading inside and
near the small cabin, covering first- and third-person views, then restores the
renderer feature after that camera finishes.
Normal material lighting and sun shadows remain on the interior and seats;
outdoor cameras retain the island's configured contact shading. The modules,
meshes, textures and stations remain shared. `SkySailBuilder` preserves the bulb
material when the modules are regenerated.

Windows visual evidence does not establish Android appearance, RAM use or FPS.

## Room integration delivery

At this checkpoint, the complete player was delivered to `Builds/WindowsFinal/`; use its normal launcher
for room play, or `Preview-Teleport.cmd` for the offline animation preview.
The final two-player room suite passes **519 assertions**, and normal shared
Skyrail travel passes **18**. The earlier integrated animation suite passes
**492**, covering the full island capacities. Final UI and cabin captures are
in `Builds/RoomTeleportQA/FinalNetwork/`. The player was also verified from a
relocated folder with spaces in its path, with all 325 files matching.

The fresh signed APK is **74,986,217 bytes**: **42,512** above the opening
observed APK, **25,013,783** below the hard ceiling and **13,783** below the
development target. See [verification](VERIFICATION.md#room-teleport-post-match-choices-and-cabin-lighting---15-october-2026)
for test coverage, source-snapshot details and remaining device/WAN limits.

## Earlier preview delivery

At the earlier preview delivery, the complete player was in `Builds/WindowsFinal/`, with
`Preview-Teleport.cmd` and `LATEST-BUILD.txt`. The newer character-refinement
player was retained after checking all runtime/assets against the adopted
candidate; no newer delivery was replaced with an older snapshot.

The player passes 492 teleport assertions; an identical gameplay assembly passes
another 492 from a relocated complete player. The cross-island test injects a
650 ms rendering stall. The final five-check recording is
`Builds/TeleportQA/Teleport-Preview.mp4`. See [verification](VERIFICATION.md#blue-light-teleport-preview---15-october-2026)
for coverage, source limits and supporting shared Sky-Sail checks.

The fresh signed APK is **75,142,885 bytes**: **386,620** above the opening
observed build, **24,857,115** below the hard limit, and **142,885** above the
development target. Shared growth includes concurrent character-head refinement.
The task's signed snapshot and size audit remain under `Builds/TeleportQA/`.
See [the size record](BUILD-SIZE.md#blue-light-teleport-preview---15-october-2026).

Reviewed superseded output was archived under these local `Legacy/` batches:

- `20261015-153945-664-teleport-first-preview`
- `20261015-154650-587-teleport-apk-baseline`
- `20261015-154831-151-teleport-camera-refinement`
- `20261015-155629-320-teleport-intervening-apk`
- `20261015-160047-708-teleport-station-continuity`
- `20261015-161209-071-teleport-shader-rebuild`
- `20261015-162024-883-teleport-hitch-refinement`
- `20261015-162821-857-teleport-final-apk`
- `20261015-163125-994-teleport-newest-player`
- `20261015-163420-725-teleport-delivery-evidence`

Each batch includes the archive tool's recovery manifest and hashes. Failed
iteration logs and current evidence are retained in ignored `Builds/TeleportQA/`.
