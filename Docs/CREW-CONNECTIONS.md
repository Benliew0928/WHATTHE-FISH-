# Crew reconnection and friend invitations

## Player experience

A guest who briefly loses connection has a **30-second** grace period while
the running game reconnects automatically. The host reserves their original
member identity, participant number, player/spectator seat, team and saved
position. The room pauses together, with a visible countdown and a Leave room
action. Deliberate departures use a server acknowledgement and do not trigger
the grace period. The first interruption owns the deadline; additional missing
guests do not extend it.

Gameplay time pauses independently of NGO transport time. Network updates,
identity verification and reconnect acknowledgements continue while the match
clock and simulation are held. Existing sport snapshot restoration rebinds the
returning network object to its participant records. Scores and match phase are
preserved; in-flight actions settle through the existing sport restoration
logic. This is an in-process reconnect, not recovery after closing/restarting
the application.

If a playing participant does not return, Football and Basketball cancel the
incomplete round without a winner, Fishing abandons the round, and Golf applies
its existing departure/DNF behavior. Spectator loss does not invalidate a
playing roster. An interrupted cable/teleport journey returns the crew to the
waiting room, where everyone can ready up and travel together again. The four
islands and one active game per host room remain unchanged.

Friends can invite each other from an online waiting room. A brief notification
opens the Player Hub's Invitations page. Invitation cards show the sender's
verified public identity, sport, expiration, Join room and Dismiss actions.
Joining from another room requires a concrete Stay here / Leave & join
confirmation. Connecting can be cancelled; an unsuccessful join keeps the
invitation available until it expires. The UI uses the shared navy surfaces,
cyan accents, lime actions, Barlow display type and existing character art.

## Identity and service rules

- Network protocol **83**: all peers must run the same build.
- Reconnection requires a random, owner-only room key, its checkpointed SHA-256
  digest and the original authenticated player identity. Duplicate live members
  and outsiders cannot take a reserved place. Keys are kept through repeated
  guest interruptions and cleared on leaving; they are never saved to disk.
- Online return is complete only after the normal public-profile proof verifies
  the returning player. Host migration retains the existing election/checkpoint
  flow and carries the gameplay clock offset to the new host.
- Invitations require an accepted friendship, expire after 120 seconds and have
  a 30-second resend cooldown. Atomic limits allow at most 20 active inbox and
  outbox records. Blocking/removing a friend revokes their invitations.
- The inbox does not expose the room code. Only its recipient can resolve a
  valid invitation. Invitations are advisory: the actual Unity room still
  enforces protocol, capacity, spectator settings and the gameplay lock.
- The inbox polls while focused at 20-second intervals, with bounded failure
  backoff. Account changes clear cached invitations and notifications. No email,
  push-notification provider or arbitrary join URL is introduced.

The service implementation and deployment instructions are in
[AccountServer](../Tools/AccountServer/README.md). Apply
`0003_room_invites.sql` after the two existing profile migrations. The migration
was applied through the existing Cloudflare D1 console on 24 October 2026;
the deployed Worker version is `af0271d6-ee24-45b1-9361-1c9b854ecf51`.

## Reproduction and evidence

`Tools/Build/Build-CrewConnections.ps1` creates a complete development Windows
player under `Builds/CrewConnectionsQA/Player/`. Its optional `-ScriptsOnly`
refresh requires a complete player and cannot replace a full export after a
serialized layout change. The development-only `CrewConnectionsReview` fixture
accepts `-crewReview` with an evidence directory; it is excluded from Android
submission builds. Run:

```powershell
Tools/Build/Test-CrewConnections.ps1 -Sport Football
Tools/Build/Test-CrewConnections.ps1 -Sport Basketball -Scenario HostLoss -Port 7943
Tools/Build/Test-CrewConnections.ps1 -Sport Golf -Port 7942
Tools/Build/Test-CrewConnections.ps1 -Sport Fishing -Port 7944
Tools/Build/Test-CrewConnections.ps1 -Sport Football -Scenario Expire -Port 7945
Tools/Build/Test-CrewInvitations.ps1
```

The invitation suite creates isolated anonymous test profiles, exercises the
actual UI callbacks and online room join, and deletes its own profile records
in cleanup. It does not contact existing players. `-Online` on the reconnect
suite uses Unity Relay; test credentials are not written to evidence files.

Current service evidence: `Builds/CrewConnectionsQA/backend-tests.txt` contains
**44 passing tests**, and `invitation-cloud-smoke.txt` contains **13 live
assertions** with temporary profile cleanup. Windows validation, final source
scope and release measurements are recorded with the delivered player and
in the build-size documentation. Windows captures do not establish physical
Android touch, phone frame rate, or behavior on different mobile networks.

## Validated delivery — 24 October 2026

The complete player is in `Builds/WindowsFinal/` with its runtime, data,
supporting files and retained launchers. The Android release is
`Builds/Android/WhatTheFish-release.apk`: **113,823,708 bytes**, leaving
**886,176,292 bytes** below the strict limit. Both require network protocol
**83**. The [build-size record](BUILD-SIZE.md) and [APK audit](APK-SIZE-AUDIT.md)
record checksums, source verification and entry deltas.

The final gameplay regression set passed **109 assertions**:

- Football return: `Run-Football-Return-20261024-200810-945` (22).
- Basketball return followed by host loss:
  `Run-Basketball-HostLoss-20261024-200818-334` (26).
- Golf return: `Run-Golf-Return-20261024-200950-667` (22).
- Fishing return: `Run-Fishing-Return-20261024-200950-667` (22).
- Football grace expiry: `Run-Football-Expire-20261024-201507-724` (17).

These directories are under `Builds/CrewConnectionsQA/`. Checks cover paused
and resumed match phase, frozen gameplay clock, stable identity/seat/team,
score preservation, other connected peers, deliberate departure, grace expiry
and host migration after a guest returns. Earlier preliminary runs are
superseded: a queued physics tick could previously cancel a match before the
pause took effect. Explicit pause guards and checkpoint restoration fix that
race; the final tests assert that the round remains active after return.

`Invitations-20261024-195513-227` passed **39 live UI assertions**, covering an
accepted friendship, the actual Invite button, notification, inbox, Stay,
Leave & join, successful Unity Relay room entry and test-profile cleanup.
Desktop and 1080 × 600 landscape captures were visually reviewed. Subsequent
copy refinement for a solo host and confirmation keyboard-focus isolation are
compiled in the delivery. The final live rerun was blocked by local service
timeouts before reaching that UI, so those last refinements were code-reviewed
but not rerun through the complete online flow.

The online reconnect attempt also timed out creating its Unity lobby, before
any reconnect could be exercised. Local multi-process reconnection is verified;
live Relay reconnection and physical mobile network interruption remain
unverified. The failed run's one temporary profile was removed separately.
The earlier 13-assertion live service test and 39-assertion live invitation
test passed before connectivity failed.

The isolated delivery snapshot preserves concurrent source work and includes
the latest cinematic presentation. It excludes only the unfinished voice SDK
integration. PDB checks verify 478 Windows and 343 Android source documents;
two source differences remove voice hooks. No voice code was removed from the
primary workspace. Build outputs, diagnostic logs and captures remain ignored.

Superseded preliminary players/downloads were archived in
`Legacy/20261024-202345-901-crew-connections-preliminary/`; the reviewed previous
Windows player, APK and audit were archived in
`Legacy/20261024-203229-182-crew-connections-delivery/`. Current evidence and
the exact isolated source snapshot remain under `Builds/CrewConnectionsQA/`.
