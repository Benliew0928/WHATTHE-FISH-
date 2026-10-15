# Room spectators and sky arrivals

Every room has two independent rows: up to ten players (five on fishing) and
ten spectators. Everyone joins with the same room code. An open player place
is assigned first; when those are full, a spectator place is assigned if the
host permits it. The waiting-room buttons let each person move to an empty
numbered place in either row. Moving clears their ready flag. Occupied places
cannot be stolen, and only the owner can change their place.

The host's **Spectators: allowed / closed** button controls spectator admission.
Closing an occupied spectator row is rejected with an explanation; existing
guests are never silently evicted or moved. Places and admission settings are
fixed during travel and exploration. As with the existing room flow, new joins
wait until the host returns the room to its waiting menu. The cloud session
reserves twenty connections so a fishing room can later visit another sport;
server approval still enforces five players plus ten spectators on fishing.
Pending connection approvals reserve a place before the character spawns, so
simultaneous arrivals cannot claim the same number. Waiting-room avatars do not
send movement commands or publish timestamp-only locomotion snapshots.

Numbers 01–10 select the same existing character models in each row. Player 01
and spectator 01 can coexist. Spectators retain personal colours and an **S**
prefix on their world badges. No character meshes, maps or animation clips are
duplicated. When travelling to fishing, any player place above 05 is reassigned
to an available place in the five-player row. The group must contain at most
five playing participants; spectators do not consume that capacity.

## Watching and movement

Football and basketball spectator arrivals are evenly spaced by arc length
around the existing stadium seating promenade, immediately behind the seats.
They face the playing area and can walk or run around the stands, use the
stairs and explore outside the court. The capsule is swept against the actual
authored playing rectangle, expanded for body clearance, at every height.
Corner approaches, large time steps and falls cannot pass through the court.
Falling off the island returns a spectator to their own assigned location.

Spectators use the normal walking animation, a look pad, a movement stick and
first-person, chase or elevated cameras. Their HUD shows match scores, a waiting
status, camera and room controls. A host who chooses a spectator place retains
the start-match and shared-travel authority. Spectators cannot jump, tackle,
kick, acquire a ball, shoot, use sports props, summon a buggy, swing or fish.
Their capsules collide with scenery but do not obstruct players or balls.
Only playing participants enter teams, bot-fill counts, fishing piers and
golf rounds. Fishing spectators arrive around the lagoon; golf spectators
arrive near the existing southern arrival area.

## Entry and shared travel

**Travel together** first waits for everyone to ready up, then uses the shared
teleport loading barrier. After all peers prepare the island and validate every
landing capsule, each camera begins near the clouds and descends smoothly to
its own character's position. The sky starting direction varies reproducibly
between entries. Characters remain hidden during flight, then appear through
the existing blue-light reveal. Camera return and movement unlock wait for
every peer. A slow frame lengthens the flight rather than skipping it.

Players land in the island's existing game positions, spectators in their
numbered watching positions. Post-match teleport keeps its departure effect
and supports both rows. Station arrivals use distinct rows on the existing
deck, with the last row inside the rear edge. Skyrail travel carries everyone
in four cabin rows when needed and uses the same safe deck layout to disembark
the whole room. The four-island world and single active game per
host are unchanged.

Blocked landings, cancellation, loading timeout or a changed roster restore
the group. A cancelled initial entry returns to the waiting room. Protocol
**65** is required on every peer; see [disconnect recovery](DISCONNECTS.md) and
[automatic host succession](HOST-MIGRATION.md).

## Source and checks

- [Room slots and landing layout](../Game/Assets/_Game/Shared/Core/RoomSeats.cs)
- [Server movement and court exclusion](../Game/Assets/_Game/Shared/Player/SpectatorMovement.cs)
- [Room authority](../Game/Assets/_Game/Shared/Networking/NetworkAthlete.cs)
- [Shared teleport](../Game/Assets/_Game/Shared/Core/RoomTeleport.cs)
- [Animation and camera](../Game/Assets/_Game/Shared/Core/TeleportSequence.cs)
- [Runtime review](../Game/Assets/_Game/Shared/Core/SpectatorReview.cs)
- [Review runner](../Tools/Build/Test-Spectators.ps1)

Build the development candidate with `SpectatorBuild.Build`, then run
`Tools/Build/Test-Spectators.ps1`. Output belongs in ignored
`Builds/SpectatorQA/`. The full-capacity fixture uses replicated network actors
inside a two-process room. `Tools/Build/Test-SpectatorCapacity.ps1` separately
checks admission and overflow with twenty local processes (fifteen on fishing).
It also checks player/spectator replacements and simultaneous island departures;
`-AbruptDisconnect` forcibly terminates those processes and verifies surviving
peers keep exchanging messages beyond the timeout. `SpectatorBuild.CheckLandings` checks forty spectator
places and eighty station places against the actual island and station scenery.
Neither is a twenty-device or phone performance measurement.

Delivery validation and APK measurements are recorded after the completed runs
in [verification](VERIFICATION.md) and [build size](BUILD-SIZE.md).
