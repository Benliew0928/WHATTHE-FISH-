# Automatic room host succession

When the host leaves, closes the game or loses its connection, the lowest
occupied **player** number becomes host. If no players remain, the lowest
occupied **spectator** number becomes host. This ordering uses the current
room places, including places chosen in the menu, rather than join order.
The new host keeps their existing place and character. A spectator host
retains the room and travel controls without becoming a playing participant.

The room displays a reconnecting overlay while the connection is restored.
The same room code, numbered places, identities, nicknames, appearances,
readiness, spectator permission and selected island are retained. No one has
to rejoin with a different role or create a replacement room. Normal admission
limits remain ten players (five for fishing) plus ten spectators. Unknown
participants cannot enter while recovery is in progress.

## Connection and state recovery

[RoomMigration](../Game/Assets/_Game/Shared/Networking/RoomMigration.cs) keeps
the latest replicated checkpoint on every peer, sampled twice a second from
the host's heartbeat. Stale heartbeat data does not advance the saved match
clock. A persistent member identity and participant number survive NGO's new
connection IDs, so statistics, ball ownership and cart references cannot be
assigned to a different person after reconnecting. The next participant number
is also replicated, preventing reuse of a departed participant's identity.

Unity's session service can initially elect any survivor. That coordinator
transfers session ownership to the lowest eligible room place before starting
the replacement game host. The elected owner creates a new Relay allocation
in the existing session; the other session members follow its updated network
metadata. A custom network handler serializes NGO shutdown and startup.
Graceful host departure transfers ownership before leaving the session. The
new host removes the disconnected host's lingering session membership; ordinary
disconnected guests are also removed, freeing places for replacements. Failed
service cleanup retries without closing the surviving room. Only the last
participant deletes the session.

The embedded [Multiplayer Services 2.3.3 correction](../Game/Packages/com.unity.services.multiplayer/LOCAL-PATCH.md)
handles a property being deleted while lobby updates arrive. The unpatched
SDK dereferenced that null entry during a real Relay handoff. It also prevents
older HTTP responses and positional event patches from overwriting newer
player lists, while preserving host-change events and lobby heartbeats. Network
events copy the service cache's complete snapshot and resolve departures by
player identity; their positional patches are never replayed against a second
list whose order may differ.
The service cache also preserves the complete server-provided player order;
reconstructing reordered lists from index differences could target a healthy
host when a later player left. Event patches require the exact prior version.
Membership events always fetch a complete player list before deciding who left;
a valid positional index alone is insufficient to remove a room member.
Invalid indexed events leave the complete cache unchanged and fetch a fresh
snapshot, so a failed update cannot partially remove room members.
Repeated event versions are deduplicated while a snapshot request is pending.
Snapshot reads are spaced and retry service throttling without removing peers.
Network metadata changes are processed sequentially so overlapping stop/join operations
cannot interrupt the replacement connection. Late host notifications matching
the connected game host do not begin another recovery. The dependency version
is retained. Failed Relay joins can retry the same allocation metadata on the
next session refresh after a temporary service timeout. The corrections
are included in fresh-clone builds.

NGO has no built-in GameObjects state migration. The game explicitly restores
its [network variables](../Game/Assets/_Game/Shared/Networking/NetworkAthleteMigration.cs),
character positions and [sport state](../Game/Assets/_Game/Shared/Networking/RoomGameRecovery.cs).
The server restores both its time system and tick clock before accepting
reconnections, preventing connection-approval deadlines from expiring when
the old game time is restored. Each returning owner acknowledges their new
character before play resumes. A missing participant cannot hold the room
paused indefinitely.

Scores, teams, statistics and golf/fishing progress are recovered from the
checkpoint. Short-lived ball possession/flight presentation may settle during
the handoff; this is checkpoint recovery, not deterministic replay. A host lost
during a journey restores the last settled room/island checkpoint, loading
that island before reconnecting. Travel can then be requested again.

The disconnect detection and service election take time; this is an automatic
reconnection pause, not an uninterrupted server connection. If the service
cannot recover the room, a bounded timeout gives the participant an error.
Protocol **65** requires matching updated builds on every device.

## Reproduce

Build a development candidate with `ProjectBuilder.BuildHostMigrationVerification`
or use the complete `Builds/WindowsFinal/` player, then run:

```powershell
Tools/Build/Test-HostMigration.ps1 -Sport Football -Online -AbruptGuests
Tools/Build/Test-HostMigration.ps1 -Sport Basketball -Online
Tools/Build/Test-HostMigration.ps1 -Sport Golf -Online
Tools/Build/Test-HostMigration.ps1 -Sport Fishing -Online
Tools/Build/Test-LobbySnapshot.ps1
Tools/Build/Test-NetworkMetadataRetry.ps1
```

`-PlayerPath` selects another complete player. `-Port` selects the local test
port. Online runs use separate authentication profiles and real Relay/session
services. `-AbruptGuests` first kills a playing guest and spectator, then verifies
the survivors and refills both places. The runner launches twenty independent processes (fifteen for fishing),
kills hosts in the waiting room and on the island, verifies identity/seat/code
retention and match continuity, exercises repeated succession and graceful
departure, then removes all players to test spectator fallback. A later player
join verifies that a player takes precedence over older spectator connections.

Every survivor sends an owner RPC and every other peer must see the resulting
replicated value. Fresh probe heartbeats and 35-second observation periods
prevent stale roster files from counting as successful recovery. Evidence is
kept under ignored `Builds/HostMigrationQA/`; measured outcomes and delivery
limitations belong in [verification](VERIFICATION.md).
