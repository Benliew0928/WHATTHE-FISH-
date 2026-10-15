# Session updates during host migration

This is the complete Unity Multiplayer Services 2.3.3 package from the resolved
registry package. License, source, metadata and package dependencies are retained.
The registry signature is omitted because this package contains a source patch.

Five upstream source files are changed: `Runtime/Multiplayer/Session/SessionHandler.cs`,
`Runtime/Multiplayer/Lobby/LobbyHandler.cs` and
`Runtime/Multiplayer/Modules/Network/NetworkModule.cs`, plus
`Runtime/Lobbies/SDK/LobbyCacher/LobbyCacher.cs` and
`Runtime/Lobbies/SDK/LobbyUpdates/Internal/LobbyChannel.cs`.
`UpdateDerivedProperties` now handles null entries in both the lobby data and
session property dictionaries. `StopNetworkAsync` stages a null session property
for deletion. A concurrent lobby event or refresh previously dereferenced that
null entry while the new host stopped the old Relay network, raising a
`NullReferenceException` before processing the rest of the session update.

This fix was reproduced in the project's real Relay host-migration smoke test;
it is a local correction, not a claim that an upstream release includes it.
The lobby handler also keeps versions monotonic across overlapping HTTP responses
and events. Stale positional player patches are skipped, and a version gap fetches
a complete lobby before applying further patches. A 20-client Relay admission test
reproduced the original out-of-range player insertion after an older response
replaced a newer player list. Full snapshots and patch predecessors now agree.
Snapshots also deliver host changes and start the elected host's lobby heartbeat,
even when the corresponding event arrives later and is skipped as stale.
Event callbacks copy the service cache's complete snapshot instead of applying
its positional diff to a second, independently refreshed player list. A
full-capacity fishing run exposed a valid host being removed by that second
application. Departures are compared by player ID, and the copied snapshot owns
its own player list. `Tools/Build/Test-LobbySnapshot.ps1` reproduces the list-order
failure against the earlier binary and passes against the corrected binary.
The service cache itself also retains complete HTTP responses instead of
reconstructing their player order through synthetic remove/insert operations.
The upstream diff can turn `[A,B,C]` into `[B,C,A]` when the server returns
`[C,B,A]`. The next indexed departure then targets the wrong person. The same
regression test reproduces this second failure and checks the following removal.
Event patches require their exact cached predecessor; gaps await a full response.
Membership events always fetch a full response. Even an in-range player index
can identify the wrong person after independently reordered lists, as reproduced
when two guests were terminated in a full room. Only the complete response's
player identities decide who has left; indexed membership hints never remove
someone directly from the local session.
Patches are applied to a temporary snapshot before publishing. Invalid player
indices cannot partially remove members or advance the cached version. The event
channel fetches a full response when an event could not be applied. A 20-peer
football run completed every handoff but exposed this invalid-index diagnostic;
the regression also checks rejection and subsequent snapshot recovery.
Redelivered versions already waiting in the event queue are ignored instead of
throwing on a duplicate key. Full snapshot reads are spaced, with a delayed retry
for service throttling; expected rate-limit and post-departure access responses
remain warnings. Programming exceptions still surface as errors. The lobby
regression reproduces the duplicate pending event against the previous binary.

Network metadata transitions are serialized. A removed network property can
still be stopping the old connection while a later event publishes its
replacement. Processing both concurrently could shut down a connection just
after it had recovered. Queued updates now read the latest property after the
preceding stop/join has completed.
If a Relay join fails, the failed metadata is cleared so the next session
refresh can retry it. Event callbacks report a retry warning without throwing
an unobserved asynchronous exception; explicit initial joins still report their
failure to the caller. A full-capacity run reproduced a Relay HTTP timeout
that previously left one survivor disconnected permanently.
`Tools/Build/Test-NetworkMetadataRetry.ps1` checks that the same metadata attempts
another join after a deterministic failure, while explicit callers still receive
the error. Both scripts read compiled player assemblies.

All other package files are unchanged. Keep this embedded package until a tested
upstream version handles property deletion during concurrent lobby updates.

Run `Tools/Build/Test-HostMigration.ps1 -Online` for each sport with a current
development player, and rebuild AndroidSubmission. Repository-relative paths
and measured results are documented in `Docs/HOST-MIGRATION.md` and
`Docs/VERIFICATION.md` (relative to the repository root).
