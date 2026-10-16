# Verification records

## Main sync and Fishing integration - 16 October 2026

Incoming main `c64bacb` is combined with the preserved local Fishing and startup
work. Incoming networking and embedded packages are retained; RoomService differs
only by the required protocol increment to **66**. Matching peers must rebuild.
Fishing uses continuous Left/Right holds, with the retained camera, rod, rope and
water presentation. Its delayed result card now takes pointer/controller priority
over the shared next-adventure card.

| Final local Windows check | Passing assertions |
| --- | ---: |
| Rendered Fishing gameplay and native UI | 2,362 |
| Rendered Fishing host / guest, including abilities and rematch | 46 / 49 |
| Feedback, results and delayed-card pointer regression | 178 |
| Spectators and shared four-island travel, host / guest | 223 / 147 |
| Character identities, Football / Basketball | 122 / 122 |
| Archive-source copy and interrupted-startup retry | 61 |

These nine reports contain **3,310 runtime assertions**. Lobby snapshot regressions
also pass 11 checks; unchanged-metadata network retry passes against the compiled
embedded SDK. The 1,571 Fishing rule checks are included in the offline count,
and separately pass during the guarded script refresh.

The full Windows build succeeds. Later code refreshes preserve its native data,
catalog and complete island-bundle hashes. The last production adjustment only
closes an overlapping next-adventure card when Fishing results open; it adds no
serialized fields or assets. The source comparison after Android reports no
runtime C# changes. The complete tested player remains in `Builds/WindowsFinal/`.

Initial harness failures are retained: the old 65-second probe expired during
cold cache preparation; headless clients could not validate arrival shaders;
the guest was launched before the host was ready; and simulated gear input ran
during an arrival-page transition. The harness now waits for a live room, uses
rendered clients, waits for arrival and active controls, and follows the native
Keep exploring action before rematching. The results-pointer regression exposed
and now covers the actual delayed-card overlap fixed in this merge.

Separate loader measurements on this laptop were **71.64 s cold / 4.95 s warm**,
including **67.92 s / 1.18 s** cache preparation. This is Windows first-launch
evidence, not phone timing. The copy/retry path also completes with one expected
failure, preserves unrelated files and cleans interrupted preparation files.

Fresh AndroidSubmission at **21:41:00 Malaysia time** measures
**73,459,821 -> 75,053,357 bytes (+1,593,536)**. Independent APK v2 signature verification
passes. SHA-256: `EB1E8A61B41F130AA470EE259B402B9ED8BC003D6B05D75D57E98A96DFA11377`.
The APK is **24,946,643 bytes below 100 MB**, also satisfying
the incoming repository's 1 GB gate. This is a measured APK, rather than a source
folder estimate. The older 75 MB target has -53,357 bytes
of reserve against this build; the incoming repository policy retires that target.

Evidence: `Builds/GitSync/Task-20261016-200323/`, with the final report folders recorded in its QA logs.
The spectator run uses two actual local Windows processes and synthetic seat
fixtures; it does not establish fifteen/twenty physical clients. Full host
succession and WAN behavior were not rerun in this sync. Physical Android touch,
appearance, first-launch storage and sustained FPS remain unverified.
Superseded local deliveries were archived in the `20261016-200853-871-git-sync-windows`
and `20261016-212327-589-git-sync-apk` batches.

## Automatic host succession - 16 October 2026

[Host recovery](HOST-MIGRATION.md) now preserves the room when its host leaves,
closes the game or loses the connection. The lowest occupied player number takes
over; if no players remain, the lowest spectator number takes over. Room code,
places, identities, selected island and the latest settled match checkpoint survive.
Both deliveries use protocol **65**, including the earlier spectator, shared cloud
arrival and abrupt guest-disconnect corrections.

Real Unity Relay/session tests use separate authenticated Windows processes:

| Room | Capacity | Passing checks | Run under `Builds/HostMigrationQA/` |
| --- | ---: | ---: | --- |
| Football, final binary | 10 + 10 | 5,200 | `Capacity-Football-20261016-020421-875` |
| Fishing, final binary | 5 + 10 | 3,765 | `Portable workspace/Builds/HostMigrationQA/Capacity-Fishing-20261016-021048-134` |
| Basketball, earlier SDK correction | 10 + 10 | 3,929 | `Capacity-Basketball-20261016-003547-299` |
| Golf, earlier SDK correction | 10 + 10 | 3,929 | `Capacity-Golf-20261016-004311-026` |

Each run exercises five handoffs: abrupt host loss in the waiting room and during
the match, graceful departure to the first spectator, abrupt spectator-host loss,
then a newly joined player taking priority over older spectator connections.
The final Football and Fishing runs also terminate an ordinary player and a
spectator, verify the survivors and refill both places before host migration.
Every survivor sends an owner RPC that every other survivor must observe;
identities, places and code are compared before/after. Each handoff includes a
35-second stability observation, with fresh heartbeat files required throughout.
The final log scans contain no programming exceptions, assertions or content-load
failures. Recoverable service rate limits/post-departure responses remain warnings.

Full-capacity testing exposed SDK races involving null property deletion, stale
HTTP versions, independently reordered player lists, invalid indexed events,
duplicate queued events and overlapping network metadata transitions. The
embedded [Multiplayer Services patch](../Game/Packages/com.unity.services.multiplayer/LOCAL-PATCH.md)
changes five source files; all 1,332 upstream files are retained with their license.
Eleven deterministic lobby checks and the failed-metadata retry regression pass
against the final compiled DLL. Earlier binaries reproduce the specific failures;
failed runs and their diagnostics remain alongside the corrected runs.

All **393 game source files** are unchanged since the Basketball/Golf passes and
local transport regressions; the later corrections are confined to the SDK.
The shared teleport regression passes **519 checks** in
`Builds/RoomTeleportQA/Run-20261016-011113-713/`. Golf-cart offline, network
authority, driving and disconnect checks pass in
`Builds/GolfCartQA/Run-20261016-011158/`. The final SDK is exercised at both room
capacities above; Basketball/Golf were not rerun after the final SDK correction.

The complete Windows player was built at **02:03:41 Malaysia time**. All **325
files** match the tested candidate before updating `LATEST-BUILD.txt` in
`Builds/WindowsFinal/`. All **292 compiled game source documents**, the five
patched Multiplayer SDK files and the Transport receive-buffer correction match
current source. The portable Fishing run and both SDK tests use default paths
from an unrelated working directory and a copied player in a directory containing
spaces. The regenerated golf prefab's fourteen YAML objects match the original
semantically; local IDs differ. URP resources are regenerated per build platform.

A fresh AndroidSubmission APK was written at **02:07:12 Malaysia time**. Its APK v2
signature verifies; all **213 inspected compiled source documents** match current
source, including host recovery, protocol 65 and both UDP buffer-release paths.
The automatic **1 GB** gate passes. Exact artifact hashes, compiled proofs,
capacity results and source limitations are in
`Builds/HostMigrationQA/verification.json` and `android-delivery.json`.

All live peers used one Windows PC and internet connection. No Android device was
connected: physical phone rendering, controls, performance and mixed-device/WAN
behavior remain unverified. Recovery includes a reconnection pause and restores
the latest settled checkpoint; short ball flight may settle, and a host lost in
transit returns the room to its last settled destination.

Superseded deliveries and completed duplicate players were archived using the
repository archive tool, including `host-migration-previous-windows`,
`host-migration-previous-apk` and `Legacy/20261016-021857-573-host-migration-completed`. Current builds and validation
evidence remain under ignored `Builds/`; active assets and unrelated changes are
preserved. Git staging, committing and pushing remain with the user.

## Abrupt guest disconnect recovery - 15 October 2026

[Disconnect recovery](DISCONNECTS.md) returns failed UDP receive buffers to the
host's pool, preventing one terminated guest from exhausting reception for the
whole room. Teleport restoration skips a departed character's despawned network
transform, preserving the remaining room's camera, controls and visibility.
Both deliveries use protocol **64** with the embedded Transport 2.7.3 backport.

Real independent Windows processes passed these abrupt-disconnect runs:

| Room | Players + spectators | Checks | Run under `Builds/DisconnectQA/` |
| --- | ---: | ---: | --- |
| Football | 10 + 10 | 3,583 | `Capacity-Football-20261015-215345-452` |
| Fishing | 5 + 10 | 2,241 | `Capacity-Fishing-20261015-215849-423` |
| Basketball | 10 + 10 | 3,581 | `Capacity-Basketball-20261015-220346-252` |
| Golf | 10 + 10 | 3,601 | `Capacity-Golf-20261015-221800-383` |

Total: **13,006 passing capacity checks**.

Each run checks unique places, overflow rejection, separate player/spectator
process termination and replacement, full-room cloud arrivals, simultaneous
loss of both guest roles on the island, continued exploration and lobby refill.
Survivors are continuously checked for **35 seconds** beyond each recovery,
longer than the existing 30-second transport timeout. Every peer sends an owner
RPC and every other peer must observe its changed nickname; stale roster files
cannot count as a live connection.

The shared-teleport regression also passes **519 checks (290 host + 229 guest)**,
including departure during travel and restored host controls. Evidence is in
`Builds/RoomTeleportQA/Run-20261015-215108-707/`; earlier failed runs remain
available alongside their corrected reruns. The preceding football run also
survived 1,536 empty and 1,536 oversized UDP datagrams.

The portable Golf rerun passes after correcting a development-only
command-file read race. The first attempt stopped one test coroutine when
Windows temporarily hid a replaced file; no game peer had left its room.
`probe-only-change.json` proves that all **288 other compiled game source
documents** match the three other successful capacity runs and shared-teleport
regression. The probe is excluded from the Android release. This harness
correction does not change gameplay. Golf ran from an unrelated working
directory using the copied runner's default player path, under a directory
containing spaces.

The complete final Windows player was built at **22:17:42 Malaysia time** and
promoted to `Builds/WindowsFinal/`. All **325 files** were checked against the
tested candidate before writing its updated `LATEST-BUILD.txt`. All **289
compiled game source documents** match current files. The unchanged golf-ball
generator's fourteen YAML objects were compared by named object/component;
only local IDs differ. URP resources are regenerated for the build platform.
`Builds/DisconnectQA/verification.json` records the final evidence and limits.

Fresh AndroidSubmission at **21:54:03 Malaysia time** is **75,006,569 bytes**;
APK v2 signature verification passes. Stripped-assembly inspection confirms
both receive-buffer release paths and both teleport authority/spawn guards.
The project owner subsequently raised the limit to **1 GB** and retired the
75 MB target and routine audit requirement; editor compilation and the updated
readiness check pass. See [build size](BUILD-SIZE.md) for the exact APK hash.

These are local Windows process tests. Physical Android touch, rendering,
memory, frame rate and WAN/Relay behavior remain unverified. The game remains
host-authoritative: closing the host still closes the room; host migration is
not included. Existing spectator, island and travel behavior is retained.

The previous delivery is recoverable in
`Legacy/20261015-222446-516-disconnect-previous-delivery/`. Superseded task
players and completed helpers are in the `disconnect-probe-duplicate`,
`disconnect-transport-only`, `disconnect-audit-duplicates`,
`disconnect-command-reader` and `20261015-222450-863-disconnect-delivery-duplicates`
dated archive batches. Current test evidence and releases remain under ignored
`Builds/`. Readiness and portable-path checks pass; unrelated changes are
preserved, and nothing is staged, committed or pushed by this task.

## Spectator rooms and sky arrivals - 15 October 2026

[Room spectators](SPECTATORS.md) add ten independent numbered watching places,
host admission control, stadium arrivals and court exclusion. Players and
spectators enter together through the loading barrier and cloud descent.
Station teleport and cable-car disembarkation now fit the complete room on
the existing deck. No additional character or environment payload is shipped.

**1,555 passing checks** across the final and documented preceding candidates:

- Final spectator walkthrough: **223 host + 147 guest**. Includes permissions,
  independent model numbers, delayed name refresh, full replicated rosters,
  game/station landings on all four islands, court boundaries at 20/30/60/120 Hz,
  a 250 ms movement step, server-driven spectator input, sporting-action
  exclusion, host-as-spectator, initial sky flights, shared travel, a delayed
  guest frame, cancellation, restored controls and normal guest departure.
- Editor geometry: **120** checks against actual island and station scenery,
  covering forty spectator places and eighty station places.
- Teleport animation/camera regression: **492**; shared room teleport:
  **290 host + 229 guest**; cable-car routes and disembarkation: **54**.

Separate real-process admission runs passed **10 players + 10 spectators**
and **5 players + 10 spectators**. Every peer agrees on unique places; overflow
is rejected, normal departure frees a place and a replacement joins. Every
remaining peer is checked again after **35 seconds** of stability. These are
local Windows processes, not a twenty-device performance measurement.

The final complete player contains **325 files**. All **289 compiled source
documents** match the working code. A copy under a path containing spaces
launched, connected and exited from an unrelated working directory. Evidence
and exact run paths are in `Builds/SpectatorQA/verification.json`, with source
hashes, screenshots and logs alongside it. The complete delivery is
`Builds/WindowsFinal/`; its stamp records the final release measurement.

Capacity and broader teleport/skyrail regressions used candidate 10. Its only
subsequent production-code difference is adding nickname data to the room-label
refresh signature; `candidate-10-equivalence.json` verifies reconstruction
against the compiled source hash. The final spectator review uses candidate 12
and additionally checks that refresh. The development cancellation fixture now
waits for both peers to observe flight before cancelling. The unchanged golf
ball preparer regenerates prefab-local IDs between builds; it validates the
same named LODs, source meshes, scale and collider. Earlier failures and their
corrected reruns remain in ignored evidence folders.

**Limit at this release, subsequently fixed by [disconnect recovery](DISCONNECTS.md):**
forcibly terminating a guest process in the high-count local UDP stress runs
sometimes caused other peers to time out. Normal leave/rejoin and the extended
stability checks passed at that time. No claim
is made that this demonstrates WAN/Relay resilience. Physical Android touch,
rendering, memory and frame rate are also unverified.

Cleanup preserves active art, source, authoring masters and other pending work.
Thirteen reviewed historical complete players (4,214 files, about 6.28 GB) were
archived in `Legacy/20261015-193741-961-superseded-validated-players/`.
Superseded task candidates, one-off editing scripts and the previous delivery
have separate dated archive batches, each with hashes and recovery paths.
Current validation evidence remains under ignored `Builds/`. No files are
staged, committed or pushed by this task.

Fresh AndroidSubmission is **75,014,337 bytes**, with **24,985,663 bytes** of
hard-limit headroom and a **14,337-byte** development-target excess. The build
size gate and independent APK v2 signature pass; see [build size](BUILD-SIZE.md).
The previous delivery archive is
`Legacy/20261015-195620-513-spectator-final-delivery/`.
`Check-TaskReady.ps1 -RequireApk` passes: **4,747 publishable files**, including
**702 LFS paths**, with no staged changes. The working-text portability check
passes. A format-aware Unity YAML comparison also confirms that the Windows
and Android golf-ball prefab changes are local-ID/order changes only.

## Forgiving prop pickups - 15 October 2026

Both sports use a **1.2 m horizontal radius**, previously 0.85 m. There is no
facing gate. The host/offline simulation now falls through into pickup on the
spawn tick, so a grounded player already waiting there receives the item
immediately after the full countdown. Existing nearest-player arbitration,
wall/height checks, transit/jump/action restrictions, replacement inventory and
ten-second respawn cadence remain intact.

**1,021 assertions pass:** football **209 offline + 242 host + 24 guest + 13
teammate + 14 rival**; basketball **211 offline + 235 host + 38 guest + 35
teammate**. Added cases test the exact spawn tick at the centre and 1.1 m from
eight approach directions with different relative facings, no early or duplicate
grant, 1.3 m exclusion, obstruction/removal and height. Broader suites cover
countdowns, random rotation, inventory replacement/revisions, movement boosts,
portal transfers, physical walls, goals, manual kickoff and room replication.

Results and their exact paths are in `Builds/PropPickupQA/verification.json`.
Football evidence: `Builds/FootballPropsQA/Offline-20261015-185318-171/` and
`Local-20261015-185318-223/`. Basketball evidence:
`Builds/BasketballPropsQA/Offline-20261015-185608-667/` and
`Local-20261015-190319-153/`. The corrected final basketball room run uses the
promoted player. Preceding football/basketball-offline candidates have identical
pickup production-source PDB hashes (`pickup-code-equivalence.json`). All three
pickup production files match the final player. A concurrently edited
`SpectatorReview.cs` differs from the compiled source; it is an unrelated
editor/development-only review. Source/provenance records are retained under
`Builds/PropPickupQA/`. These prop tests are not a full spectator-mode audit.

Old fixtures assumed instant goal-effect cleanup, automatic kickoff and an
unadvanced countdown after room-entry travel. They now wait for entry, respect
replay/manual kickoff, and establish a fresh basketball cycle for its quiet-time
assertion. Earlier failures and corrected passing runs remain as evidence.
A public visibility correction to AppRoot.Label lets the concurrent spectator
HUD compile. No unrelated source was reverted.

All **325 delivered files** matched the complete tested candidate at promotion;
see `Builds/PropPickupQA/promoted-files.json` and
`Builds/WindowsFinal/LATEST-BUILD.txt`. Fresh AndroidSubmission at **19:10:43
Malaysia time** is **75,003,257 bytes**, versus **74,986,217 (+17,040)**.
Headroom: **24,996,743 bytes**; development target exceeded by **3,257 bytes**.
Budget enforcement and independent APK v2 signature verification pass. See
[build size](BUILD-SIZE.md) and [packing audit](APK-SIZE-AUDIT.md).
Physical Android touch, visual quality, RAM/FPS and WAN behavior are unverified.

Archives: `Legacy/20261015-184637-687-prop-pickup-goal-fixture/` and
`Legacy/20261015-185912-087-prop-pickup-cycle-fixture/` retain superseded
candidates; `Legacy/20261015-190617-555-prop-pickup-android/` retains the baseline
APK/audit; `Legacy/20261015-190808-064-prop-pickup-delivery/` retains the previous
complete Windows player. Current evidence stays under ignored `Builds/`.
No staging, commit or push was performed.

## Room teleport, post-match choices and cabin lighting - 15 October 2026

The complete tested player was promoted to `Builds/WindowsFinal/`, including
runtime libraries, data, packaged islands, support folders and launchers.
Protocol **62** requires matching players. The final **325-file** candidate
matched the relocated player byte for byte before promotion. The relocated
test used a path containing spaces and ran from the system temporary directory,
with the test script resolving its default player and output paths from itself.

- **519 final host/guest assertions** (290 host, 229 guest): all eight routes,
  visible charge/absorption/travel/arrival, separate replicated landing slots,
  host-only requests and replays, stale/out-of-phase acknowledgement rejection,
  all four post-match and replay flows, fishing readiness, keep exploring/reopen,
  text rendering at 1280 x 720 and 960 x 540, a 650 ms guest render stall, blocked
  destination rollback, cancellation before departure/during absorption/after
  arrival, and a departing guest. No runtime exceptions or shader errors.
- **492 animation regression assertions** on the integrated candidate before
  subsequent UI/lighting polish: maximum-capacity sets for all eight destinations,
  separation/support/clearance, six-player fishing rejection, renderer restoration,
  cancellation, first-person/broadcast camera restoration and a render hitch.
  The animation/destination runtime sources remain unchanged since that run.
- **18 final Skyrail host/guest assertions**: two normal shared rides, guest
  authority restriction, agreed island, all players disembarking together and
  restored network transforms. Both players exited successfully.
- Inspected host/guest post-match screens and cabin interior, bulb and exterior
  captures across all four islands. The bulb close-up is clean after moving its
  solid draw after the fullscreen ambient-shading pass. Cabin views retain normal
  sun shadows while removing the coarse contact-shading speckle.

Final evidence: `Builds/RoomTeleportQA/FinalNetwork/`, `SkyrailRegression/`,
`CabinBefore/`, `portable-player-integrity.json`, `final-source-snapshot.json`
and `final-source-delta.json`. The runtime sources did not change after the final
Windows compilation; the only subsequent source change was the editor helper
that preserves the already-delivered teleport preview launcher on future builds.
Earlier animation evidence: `Builds/TeleportQA/Run-20261015-174056-368/`.
The portable iteration initially requested another trip while cancellation was
still releasing a scene; the harness now waits for that cleanup, as the real UI
does through its disabled travel button. Iteration reports are retained.

Fresh AndroidSubmission at **18:06:41 Malaysia time** passes its size gate and
independent v2 signature check: **74,943,705 -> 74,986,217 bytes (+42,512)**,
**25,013,783 bytes** hard-limit headroom and **13,783 bytes** development reserve.
See [the size record](BUILD-SIZE.md#room-teleport-post-match-choices-and-cabin-lighting---15-october-2026).
No Android device was attached. Physical touch, phone quality/RAM/FPS and WAN
Relay travel remain unverified; network tests use two local transport processes.

The reviewed superseded complete delivery and portable workspace were archived
with hashes and recovery paths in
`Legacy/20261015-180652-951-room-teleport-delivery/`. Earlier task candidates are
in the `174408-250-room-teleport-candidate`, `175113-214-room-teleport-ui-polish`
and `175943-593-room-teleport-bulb-order` batches under the same date. No active
game art was archived. Superseded APK copies and their intermediate audit are
in `Legacy/20261015-181015-105-room-teleport-apk-cleanup/`, with their hashes also
retained in `Builds/RoomTeleportQA/intermediate-apks.json`. Git publishing remains
with the user.

## Football swipe, pass guide and pressure - 15 October 2026

The complete final Windows player passes **658 assertions**: passing/swipes
**374 offline + 102 host + 8 guest**, effort/pressure **141 offline + 33 room**.
Final passing runs use **844 x 390**, with captures of locked/free passes and
all three cameras. Screenshots were inspected: the dark-edged dashed path,
receiver ring and mint **LOCKED #** plate remain clear, including first person.
The label is placed below the receiver ring to avoid the scoreboard. Earlier
1600 x 900 gameplay suites passed before this label placement correction.

Checks cover Dash-to-Pass/Shoot/Pressure/Tackle; Pass/Shoot switching without
firing both; Pressure/Tackle origins; an unavailable visible action initiating
a valid swipe; unrelated fingers; release and cancellation. Actual defenders
produce **25%, 35%, 45%, 45%** carrier penalties for one through four eligible
defenders, and leaving removes stacks. Existing wall, facing, elevation,
Turbo, dash, stamina, tackle and replicated host/guest checks pass.

All **325 files** in `Builds/WindowsFinal/` matched the tested candidate at
promotion. Evidence: `Builds/FootballSwipeQA/promoted-files.json`,
`playmaker-final-tests.txt`, `effort-final-offline-tests.txt`,
`effort-final-room-tests.txt`; captures and detailed results are in
`Builds/FootballPlaymakerQA/Run-20261015-173147/` and
`Builds/FootballPaceQA/Reviews/Offline-20261015-173147-316/` and
`Local-20261015-173216-124/`.

PDB checksums confirm every football task source matches the final player.
Of **282 compiled source documents**, four concurrent files subsequently
changed: `TeleportSequence.cs`, `NetworkAthlete.cs`, `AppRoot.cs`, and `PostMatchFlow.cs`. The player
contains the earlier compiling protocol-62 integration; these football tests
do not validate later teleport changes. Android builds a later shared source
snapshot, including concurrent room/cabin work. All football task source hashes
remain unchanged. Source records are `compiled-source.json`, `windows-source.json`,
`android-source.json` and their change reports under `Builds/FootballSwipeQA/`.
Two narrow compile fixes in the concurrent work wrapped the PlayAgain callback
and used GolfPlayerState.PlayerId in its review fixture; no other work was reverted.

Fresh AndroidSubmission at **17:36:28 Malaysia time**:
**74,943,705 -> 74,980,797 bytes (+37,092)**. Development reserve **19,203**;
strict-limit headroom **25,019,203 bytes**. Enforced size and independent APK v2
signature verification pass; see [build size](BUILD-SIZE.md) and
[packing audit](APK-SIZE-AUDIT.md). No Android device is connected; touch,
phone appearance, peak RAM, FPS and WAN behavior remain unverified.

Archives: `Legacy/20261015-172821-302-football-swipe-label/` (superseded
candidate), `Legacy/20261015-173323-045-football-swipe-android/` (baseline
APK/audit), and `Legacy/20261015-173422-252-football-swipe-delivery/`
(superseded complete Windows player). Current evidence remains under ignored
`Builds/`. No staging, commit or push was performed.

## Character head refinement - 15 October 2026

Complete **324-file Windows player**, built **16:41:18 Malaysia time**, delivered
in `Builds/WindowsFinal/`. Runtime, data, supporting folders and launchers were
promoted together and hash-checked. `LATEST-BUILD.txt` records the delivered
snapshot and test limits. The ear roots now overlap the shared scalp, ear
masking preserves skin, and linear-color shading matches the face. Nine
replacement hairstyles/hats have refined shape and detail; the original quiff
and all ten identity/team rules remain intact.

The final player passes **1,356 assertions**: identity/teams/multiplayer **620**,
compact 844x390 identity views **244**, and teleport **492**, plus two complete
host/guest roster comparisons. Final runs:

- `Builds/CharacterIdentityQA/Run-20261015-164836/`
- `Builds/CharacterIdentityQA/Run-20261015-165055/`
- `Builds/TeleportQA/Run-20261015-164846-822/`

Before the final equivalent head-coordinate packing, the refined geometry
passed **4,059 assertions** in total, including football motion **303**,
basketball motion **62**, golf grip/swing **106**, and fishing **2,232**.
Those **2,703 motion/contact assertions** were not rerun after coordinate-only
packing; body positions, rig, grip shape and animations did not change.
`Builds/CharacterRefinementQA/verification.json` maps every suite to its run.

All twenty generated meshes pass finite-data/normal checks. Eight contact
sheets cover ten heads, four angles and two LODs (**80 views**). Ear joins,
hair silhouettes and hats were inspected close up; football/basketball lineups
and live gameplay cameras were checked in the Windows player. Contact sheets
explicitly select MobileURP for repeatable lighting after desktop builds.
Pixel comparisons show small differences after final coordinate packing; no
claim of pixel-identical output is made. Desktop orthographic SSAO comparison
captures are retained separately. These previews do not establish phone quality.

All **278 compiled source documents match** the delivered runtime. Of **3,967
recorded source/asset inputs**, **3,963 match** after Android packaging. The
four differing records are URP global bookkeeping, Graphics/Quality platform
selection and generated Golf Ball prefab serialization; exact hashes are in
`post-android-source-changes.json`. The snapshot includes preserved concurrent
passing/MVP, basketball honours, post-match exploration and teleport work.
Matching multiplayer protocol **61** builds are required. This is a working-tree
snapshot, not a clean commit or a full regression of every concurrent feature.

Fresh AndroidSubmission is **74,943,705 bytes** (opening **74,754,317**;
**+189,388**), leaving **25,056,295** bytes below the hard limit and **56,295**
below the development target. Budget and v2 signature checks pass. Exact APK,
size inventory, source proof and capture evidence are under ignored
`Builds/CharacterRefinementQA/`. No Android device was attached; physical
appearance, touch, peak RAM, FPS and WAN behavior remain unverified.

The reviewed superseded Windows player is recoverable from
`Legacy/20261015-165538-607-character-refinement-final/`. Earlier task iterations
are in the `character-head-iteration`, `character-packing`,
`character-refinement-delivery` and `character-unused-coordinates` archive
batches, with explicit hash inventories. No active source/authoring asset was
archived. No staging, commit or push was performed.

## Blue-light teleport preview - 15 October 2026

The complete **324-file Windows player**, built **16:27:35 Malaysia time**,
is retained in `Builds/WindowsFinal/`. It is the newer concurrent character
refinement player; its runtime and asset files were hash-checked against the
adopted candidate and preserved. The teleport launcher and verification metadata
were added. All **492 teleport assertions pass** in
`Builds/TeleportQA/Run-20261015-162739-485/`.

Coverage includes maximum-capacity groups at all eight destinations, the
one-second charge, disappearance/reappearance, original materials, valid ground,
pair separation, blocked positions, excess capacity, cancellation before and
after the world-origin change, selected-camera restoration and scene cleanup.
A deliberate **650 ms rendering stall** during cross-island travel passes the
camera-continuity check. Animation progress is capped per update, so a stall
extends the journey instead of skipping ahead.

An identical gameplay assembly passed **492 additional assertions** from a
complete relocated player and script under a path with spaces, launched from an
unrelated working directory. Evidence:
`Builds/TeleportQA/Portable Checkout/Builds/TeleportQA/Run-20261015-162428-996/`.
The five-check recording uses the same assembly and character meshes; charge,
absorption, visible flight, station approach and reappearance frames were inspected.
See `Builds/TeleportQA/Teleport-Preview.mp4` and `Film/`. Earlier shared Sky-Sail
regression passed **18 host/guest checks**; subsequent changes only refine the
offline teleport preview. Host teleport controls and network synchronization
are not implemented in this version.

All **278 compiled source documents match**. Of **3,967 recorded inputs**,
**3,965 match** after Android packaging; URP build bookkeeping and Golf Ball
serialization differ. The working tree includes preserved concurrent changes,
not a clean commit snapshot. Source proof, full delivery hashes and test records
are under `Builds/TeleportQA/`.

Fresh AndroidSubmission: **75,142,885 bytes**, opening observed **74,756,265**,
growth **386,620**, hard-limit headroom **24,857,115**, and **142,885 above**
the 75 MB development target. Budget and APK v2 verification pass. The signed
task snapshot and audit are retained under `Builds/TeleportQA/`; later concurrent
releases may replace the shared Android output. No Android device was attached;
phone appearance, touch, peak RAM and FPS remain unverified.

Current evidence is retained under ignored `Builds/`. Superseded task players,
recordings and APKs were archived through the repository archive tool. The final
metadata/relocated-player batch is
`Legacy/20261015-163420-725-teleport-delivery-evidence/`; the preceding candidate
is in `Legacy/20261015-163125-994-teleport-newest-player/`. The previous shared
Windows delivery was already archived by the concurrent character task in
`Legacy/20261015-163211-981-character-refinement-delivery/`. See [the feature
record](TELEPORT.md#delivery-evidence) for the remaining task batches. No Git
publishing actions were performed.

## Match exit boundaries - 15 October 2026

Complete **324-file Windows player**, built **15:39:18 Malaysia time**, delivered
in `Builds/WindowsFinal/` with a full hash manifest. Football/basketball finished
or cancelled matches release the player boundary and hide its energy visual;
each player's Back button restores movement for island exploration. Active
matches restore containment. Football now constrains players without possession
as well as the carrier. Basketball retains inbound exceptions and practice's
explicit free-roam option. No protocol layout change; current protocol is 61.

**1,127 runtime assertions and 36 pure rule checks pass** on that player:
football offline/host/guest **358/102/8**, basketball **559/92/8**. Tests cover all
four edges for every participant, VS AI 2v2-5v5, active containment, post-result
release, preserved jump displacement, successive matches, actual Back buttons
and guest replicated state. Existing passes, assists, scoring and MVP checks
remain passing. Evidence: `Builds/MatchExitQA/verification.json`,
`Builds/FootballPlaymakerQA/Run-20261015-153947` and
`Builds/BasketballHonoursQA/Run-20261015-153947`.

AndroidSubmission: **74,810,941 bytes**, opening **74,754,317**,
change **+56,624**, strict-limit headroom **25,189,059** and
reserve below 75 MB **189,059**. Budget and v2 signature pass.
Physical Android touch, appearance/FPS and WAN testing remain unverified.

All boundary source files match compiled bytes. Of 278 Windows PDB source
records, 274 match at the final check. Concurrent TeleportPreview, TeleportReview,
TeleportSequence and CharacterIdentity scalp-colour changes are newer than this
Windows snapshot and are preserved, not validated here. Android compiled a newer
teleport snapshot; later character-art changes are not certified by this task.
The 3,961 recorded input hashes have six differences after packaging: Unity's
URP/Golf serialization and concurrent CharacterIdentity build/import, runtime and
shader-input files. See `compiled-source.json`, `concurrent-source-changes.json`
and `source-comparison.json` under `Builds/MatchExitQA/`. This is preserved shared
uncommitted work on HEAD `27470897`, not a clean commit snapshot.

The superseded complete player was archived in
`Legacy/20261015-154752-273-match-exit-windows/`. Current evidence remains under
ignored `Builds/MatchExitQA/`, including `delivery.json`, `windows-manifest.json`,
`SizeAuditFinal/` and `task-ready.txt`. No staging, commits or pushes.

## Basketball statistics and courtside MVP - 15 October 2026

The complete Windows player built at **15:22:07 Malaysia time**, protocol **61**,
adds [per-player basketball statistics and an arena MVP](BASKETBALL-HONOURS.md).
The full tested player is delivered in `Builds/WindowsFinal/`; its exact file
manifest and promotion time are in `Builds/BasketballHonoursQA/windows-delivery.json`
and `windows-manifest.json`. Existing football and shared-world changes are retained.

**146 assertions pass on the final player, plus 21 pure rule checks**:

- 111 relocated-player checks cover VS AI 2v2-5v5, both team colours, actual
  intercepted/completed passes, cancelled input, assisted two/three-point
  baskets, all-player records, MVP rank, readable glyphs, hidden player tags,
  court-level camera and input restoration. Captures were inspected at 844x390.
- 28 host and 7 guest checks cover reliable guest pass input, authoritative
  completion and assist credit, identical replicated records and MVP. Final
  1600x900 captures were inspected. Room clients require matching protocol 61.
- Before the final badge-visibility polish, 98 basketball gameplay checks
  passed, including shooting, rebounds, boundaries and island lifecycle; 141
  programmed-bot checks passed, including complete unassisted matches and
  filled rooms. 248 MVP/portability checks passed on that same-gameplay build.
  The final code change hides identity tags during basketball MVP and adds
  its assertion. The final honours suite was rerun; unrelated suites were not.

Rules cover exact/expired five-second assists, two/three-point attribution,
shot rebounds, deflected shots, intermediate teammate deflections, pass
interceptions counted once, last-pass attribution, self recovery, rematch
reset, own baskets, stable IDs, weighted ties and accepted buzzer baskets.
The new script and complete player ran from a different directory with spaces
and an unrelated working directory, using their default relative paths.

Fresh AndroidSubmission: **74,754,317 bytes**, change **+26,156**,
hard-limit headroom **25,245,683**, development reserve **245,683**.
Budget and APK v2 signature checks pass. No Android device was connected.

Source evidence matches all **272 compiled source documents**. Of **3,937**
recorded input hashes, **3,935** remain exact after Android packaging. Unity
regenerated the Golf Ball prefab's internal serialization IDs and URP global
settings build bookkeeping; these differences are recorded in the task evidence.
This is HEAD `27470897` plus preserved shared uncommitted work, not a clean
commit snapshot. Android and Windows include the same basketball gameplay source.
Eight concurrent teleport source/shader files and metadata appeared after the
recorded snapshot, during the final packaging run. They are preserved and are
outside this task's validated Windows delivery; their APK inclusion is not
asserted. `final-source-check.json` records those additions separately, with
no changes to the 272 compiled source documents at that check.

Evidence: `Builds/BasketballHonoursQA/verification.json`, `Run-20261015-152330`,
`Portable Checkout/Builds/BasketballHonoursQA/Run-20261015-152226`,
`compiled-source.json`, `windows-source.json`, `source-comparison.json`,
`generated-input-diff.txt`, `apk-comparison.json`, `SizeAuditFinal/` and
`task-ready.txt`. Supporting results are in `Builds/BasketballGameplayQA/Offline-20261015-151231`
and `Builds/ProgrammedBotsQA/Run-20261015-151401`.

Archives: `20261015-151921-755-basketball-honours-drafts` retains the first
disposable portability player and completed one-use edit helper. The superseded
Windows player and final disposable portability player are archived in the
batch recorded by `Builds/BasketballHonoursQA/windows-archive.txt`. Current
results and screenshots remain under ignored `Builds/`.

Limits: physical Android touch, appearance, memory/FPS and WAN play are not
validated. Portraits retain their shared blue illustration kit; team colour is
shown in the award ribbon, rail and text. No staging, commits or pushes.

## Football passing and illustrated MVP - 15 October 2026

The complete **324-file** Windows player built at **14:19:27 Malaysia time**, promoted at **14:25:29** with all file hashes verified, protocol
**60**, includes Dash/Shoot gestures, Pass replacing Jump, local-only footsteps,
host-authored goals/assists/passes/interceptions and ten illustrated MVP poses.
The MVP uses the real pitch and stadium at eye level, with larger data on an
ivory panel. [Feature rules and artwork](FOOTBALL-PLAYMAKER.md) describe the
five-second direct assist and weighted MVP ranking.

**733 runtime assertions pass on this final player**, plus **15 pure rule checks**:

| Check | Passing assertions |
| --- | ---: |
| VS AI 2v2–5v5, controls, passing, stats, portraits and camera | 102 |
| Host / guest pass RPC, stats and MVP | 38 / 7 |
| Offline match rules, phases and travel | 260 |
| Host / guest match, draw MVP dismissal and rematch | 17 / 17 |
| Football physics | 241 |
| Ball networking: host / guest | 30 / 21 |

The same production behavior also passed 81 programmed-bot assertions, 289
replay/kickoff assertions and 102 checks from a relocated complete player.
The relocated folder contains spaces; the runner used its default player path
from an unrelated working directory. Later rebuilds corrected only development
checks: a single glyph has four vertices, and the match probe must dismiss the
new MVP modal before testing movement. The backwards-pass release correction
was verified by an actual guest-to-host pass and subsequent assisted goal.

Full rendered captures at **1600 × 900**, **1280 × 720** and **844 × 390** were
inspected. All ten portrait sprites load. Camera height, live-player hiding,
input restoration and personal footsteps are checked. Shots received by a
teammate never increment passes; interception is counted once per blocked Pass.

The measured APK is **74,728,161 bytes**, versus 73,988,013 at task start:
**+740,148 bytes**, **25,271,839 bytes** hard-limit headroom and **271,839 bytes**
development reserve. AndroidSubmission and APK v2 signature checks pass.
[Size audit](APK-SIZE-AUDIT.md) includes the ten compressed portrait imports.
Android was packaged before the final development-only match-probe correction;
that probe is excluded from AndroidSubmission. Production gameplay and artwork
are included. At build validation, all 268 compiled source documents and 3,927 inputs matched.
Final hygiene removed only trailing blank lines from two C# files; 266 compiled
documents remain byte-identical and the two EOF-only changes are proven in
`final-whitespace-cleanup.json`. This is HEAD `27470897` plus shared uncommitted
work, including the concurrent trail/customization changes, not a clean commit.

Evidence: `Builds/FootballPlaymakerQA/verification.json`, `Run-20261015-142000/`,
`Run-20261015-142039/`, `Portable Checkout/`, `compiled-source.json`,
`source-comparison.json`, `portrait-assets.json`, `SizeAuditFinal/`,
`apk-comparison.json` and `apk-signature.txt`. Match regression evidence is
`Builds/FootballMatchQA/Run-20261015-142000/`; supporting bot and replay runs are
`Builds/ProgrammedBotsQA/Run-20261015-140703/` and
`Builds/FootballReplayQA/Run-20261015-140714/`.

The superseded render-only MVP model was archived in
`Legacy/20261015-135511-342-football-mvp-2d/`. Windows promotion records and the
reviewed superseded-player archive are recorded in
`Builds/FootballPlaymakerQA/windows-delivery.json` and `windows-archive.txt`.
The prior player and disposable portability copy are recoverable in
`Legacy/20261015-142523-374-football-playmaker-windows/`.
No staging, commits or pushes were performed. Physical Android appearance,
touch ergonomics, memory/FPS and WAN behavior remain unverified; no device
was connected.

## Personal football trails - 15 October 2026

The complete **324-file** Windows player built at **13:32:03 Malaysia time**,
protocol **60**, was promoted to `Builds/WindowsFinal/` at **13:37:21**. All
player data, DLLs, launchers and the release note match the tested candidate.
The football is **52.8 cm across**, including its collider. Five locally selected
trails and animated previews are available in Football Customize and the pitch
settings. Guests can choose their own effect without editing the host's venue.
See [the feature guide](FOOTBALL-TRAILS.md).

**661 assertions pass on this player:**

| Final-player check | Passing assertions |
| --- | ---: |
| Five trails, dimensions, selection, reset and fade | 40 |
| Same checks from relocated complete player | 40 |
| Offline football physics and controls | 241 |
| Ball networking: host / guest | 30 / 21 |
| Replay and kickoff: offline / host / guest | 199 / 73 / 17 |

Final trail views and menus were checked at **844 x 390**. Earlier **1280 x 720**
views of the same effects were also inspected. Five real kicks exercise every
shader design, close/wide framing, shared materials, saved preferences, invalid
selection fallback, stationary fade, reset clearing and disabled history.
The complete relocated player ran through the script's default path from a
folder containing spaces and an unrelated working directory.

Host and guest use different local selections while both kick the same shared
ball through real network input. Ownership, reception, dribbling, collisions,
goals and boundary checks pass. The host's Rose Sparkle and guest's Aurora
remain distinct through replay. VS AI 2v2-5v5, human rooms, bot-filled rooms,
consecutive/golden goals, all camera preferences, transition timing, cancellation
and the guest's aimed restart pass. Render-only replay copies contain no
colliders, rigidbodies or network objects; live trail history is suppressed.

`Builds/FootballTrailQA/verification.json` records the eight report paths and
hashes. Current images are in `Run-20261015-133214-522/`; portable results are in
`Portable Checkout/Builds/FootballTrailQA/Run-20261015-133322-380/`. Physics and
replay reports are under `Builds/FootballPrototype/Run-20261015-133214-517/` and
`Builds/FootballReplayQA/Run-20261015-133214/`. Earlier failed fixture runs are
retained separately and are not included in the final passing count.

The build is `2747089` plus preserved shared working-tree changes. All **269
compiled source documents** matched at recording. After Android packaging,
**3,900 of 3,905** input files match. Differences are platform URP resources,
generated Golf Ball local identifiers and three later concurrent files:
`FootballMVP.cs`, `FootballMVPModel.cs` and its development-only
`FootballPlaymakerReview.cs`. All code implementing this task matches. The
Windows player contains the concurrent Playmaker snapshot at build time; later
MVP refinements are outside that snapshot. This task does not claim full
validation of unrelated concurrent features. Source proofs and complete file
hashes are in `compiled-source.json`, `compiled-source-comparison.json`,
`source-comparison.json` and `windows-manifest.json`.

Fresh AndroidSubmission at **13:39:35** measures **74,020,385 bytes**, up **32,372
bytes** from the opening APK, with **25,979,615 bytes** hard-limit headroom and
**979,615 bytes** development reserve. APK v2 signature verification passes.
See the [size record](BUILD-SIZE.md#personal-football-trails---15-october-2026)
and [audit](APK-SIZE-AUDIT.md#personal-football-trails---15-october-2026).
No physical Android device is connected. Android appearance, touch, loading,
memory/FPS and WAN play remain unverified.

Reviewed obsolete players/output were archived with byte/hash recovery records:

- `Legacy/20261015-132142-706-football-trail-candidate/` - first candidate and log.
- `Legacy/20261015-132905-788-football-trail-check-fixes/` - superseded candidate,
  duplicate portable player and log; rendered reports remain under Builds.
- `Legacy/20261015-133604-229-football-trail-apk/` - previous APK and size audit.
- `Legacy/20261015-133713-491-football-trail-windows/` - previous complete Windows
  release and disposable duplicate portability player.

No staging, commit, push or history rewrite was performed.

## Longer football transitions - 15 October 2026

The complete **324-file** Windows player built at **02:28:43 Malaysia time** is
delivered in `Builds/WindowsFinal/`, protocol **58**. Every data file, library,
launcher and release note matches the tested candidate. The current ten
character identities, Blue/Red teams and prior sports/world changes are retained.

**271 assertions passed on this player**: **187 offline, 69 host and 15 guest**
at **844×390**. Tests used the default runner path from a relocated complete
player, a directory containing spaces and an unrelated working directory.
Coverage includes VS AI 2v2–5v5, human rooms, rooms filled with bots, all three
camera preferences, consecutive goals, golden goals, cancellation, held props,
conceding-side restart, arbitrary kickoff heading, AI kickoff and the guest's
real reliable kick RPC. Recorded footage remains exactly four seconds before
the goal and one second after it; scoring and match time stay protected.

**Sixteen rendered timing traces** independently measure eight **1.6-second
entries with 1.0-second holds**, and eight **0.8-second returns with 0.3-second
holds**. Entry movement takes 0.3 seconds on each side; return movement takes
0.25 seconds on each side. Measurements use transitions between rendered
samples on the simulation clock. Every camera switch and centre reset occurs
while the card is stationary and fully covering the field; the card clears
afterward. The large-logo entry and return captures were inspected.

Reports and captures are in
`Builds/FootballReplayTimingQA/Portable Checkout/Builds/FootballReplayQA/Run-20261015-022915/`.
`verification.json` records report hashes; `observed-timing.json` indexes the
sixteen CSV traces. `compiled-source.json` confirms all **256 compiled gameplay
source documents** match. Of **3,871 Windows build inputs**, **3,869** match
after Android packaging; the two differences are platform URP resources and
regenerated Golf Ball local identifiers. The build is `bd62f78` plus preserved
shared uncommitted work, not a clean commit snapshot. `windows-manifest.json`
and `windows-delivery.json` record complete delivery verification.

Fresh AndroidSubmission at **02:34:35** measures **73,988,013 bytes**, down
**3,300 bytes**, with **26,011,987 bytes** hard-limit headroom and **1,011,987
bytes** development reserve. APK v2 signature verification passes. See the
[size record](BUILD-SIZE.md#longer-football-transitions---15-october-2026)
and [audit](APK-SIZE-AUDIT.md#longer-football-transitions---15-october-2026).
No physical Android device is connected; Android appearance, touch, loading,
memory/FPS and WAN play remain unverified.

Reviewed superseded output was archived with byte/hash recovery inventories:

- `Legacy/20261015-023221-315-football-replay-timing-apk/` — preceding APK and audit.
- `Legacy/20261015-023542-600-football-replay-timing-windows/` — preceding complete Windows player and disposable duplicate portability player.

Current reports, captures, tested candidate and build evidence remain under
`Builds/FootballReplayTimingQA/`. No staging, commit, push or history rewrite
was performed.

## Ten player identities - 15 October 2026

The complete **324-file** player built at **01:47:28 Malaysia time** is delivered
in `Builds/WindowsFinal/`, with protocol **57**. Ten unique room identities apply
to humans and bots; actual football/basketball membership controls Blue/Red
uniforms. Other activities use personal colors. Both body LODs, the existing rig,
grips and animation library remain shared. Illustrated goal replay and kickoff,
four islands and the prior sports/travel work are included.

**972 final-player assertions pass**, plus two host/guest roster comparisons:

| Final-player check | Passing assertions |
| --- | ---: |
| Ten-player football: offline / host / guest | 122 / 95 / 93 |
| Ten-player basketball: offline / host / guest | 122 / 95 / 93 |
| Jump: gameplay / host / client | 132 / 3 / 5 |
| Tackle: gameplay and two-player network | 212 |

Identity checks run at **844 x 390** from a complete portable player under a path
with spaces, launched from an unrelated working directory. They cover colliding
preferences, full capacity, departure/reuse, schema migration, ten unique leases,
5v5 team membership, correct material colors, shared textures, preserved grips,
all accessory LODs, first-person hiding, network retention and personal-color
restoration. Host and guest independently agree on all ten number/team mappings.
Rendered gameplay, front/back lineups and close views of all ten presets were
inspected. Scoreboard exclusion prevents number badges hiding under the HUD;
football stamina bars share badge placement. Desktop checks do not establish
physical-phone legibility or performance.

Earlier identity candidates passed **3,077 additional regression assertions**:
turn 62, football motion 303, basketball motion 62, basketball pass 30, golf
swing 106, fishing 2,232, football effort 137 and football replay/kickoff 145.
These precede only final number-coordinate quantization, HUD placement or test
fixture changes; they are recorded separately from final-player counts. Earlier
1280 x 720 and compact identity runs also pass, but are not added to this total.

Legacy test repairs: probes wait for packaged scene startup; fishing's turn check
expects visible held-rod hands and a hidden head/accessories; jump checks the
current Tackle label; tackle LOD fixtures settle feet and move the loose ball
away, and use the existing 2.94 m/s miss-recovery speed. Basketball motion checks
full-strength IK reach after its existing 0.22-second pass gather; held-ball,
joint-continuity and release checks still cover the gather. Baseline comparison
reproduced that old reach-check failure. No gameplay timing/physics was changed
to satisfy these assertions. Failed intermediate evidence is retained locally.

All **256 compiled gameplay source documents** match the shared checkout;
all **372 recorded C#/shader/include files** still match after Android packaging.
This is `bd62f78` plus preserved uncommitted work, not a clean commit snapshot.
Unity normalizes the current quality AA field to the existing URP 2x setting;
build-generated Golf Ball local identifiers/platform resources may differ.
New identity assets have no external binary dependencies and use Unity GUID
references. Original FBX/Blender sources remain unchanged.

Fresh AndroidSubmission and independent APK v2 verification pass at
**73,991,313 bytes**, +509,276 from the preceding replay APK. Development reserve
is **1,008,687 bytes**; hard-limit headroom is **26,008,687 bytes**.
The shell's additional `$LASTEXITCODE` wrapper produced a false failure because
it followed a PowerShell script rather than a native command; Unity's final exit
is 0, the build report is Succeeded, and the gate records WITHIN_TARGET for these
exact APK bytes. [Build size](BUILD-SIZE.md#ten-player-identities---15-october-2026)
and [asset audit](APK-SIZE-AUDIT.md#ten-player-identities---15-october-2026) contain
the complete measurements.

Current evidence is indexed with report hashes in
`Builds/CharacterIdentityQA/verification.json`, with source hashes, rendered
captures, `release-audit.json`, signature output and `SizeAuditFinal/` beside it.
The superseded complete Windows player and reviewed intermediate players/captures
were archived with the repository tool in
`Legacy/20261015-021139-868-character-identities/` (1,031 files). Current source,
generated assets and their metadata remain publishable; build output is ignored.

Physical Android appearance, touch, memory/FPS, loading and WAN play remain
unverified; no phone was connected. No staging, commit or push was performed.

## Illustrated football goal replay - 15 October 2026

The complete **324-file** Windows player built at **00:31:09 Malaysia time**
is delivered in `Builds/WindowsFinal/`. Every file, including data, libraries,
launchers and release note, matches the tested candidate. It uses protocol
**56** and retains four islands, shared room travel and earlier sports,
prop-cycle, golf-camera and contact-shading work.

**360 passing assertions on the final illustrated player**, indexed with
report hashes in `Builds/FootballReplayQA/verification.json`:

| Final-player check | Passing assertions |
| --- | ---: |
| Replay and kickoff, offline at 1600×900 | 145 |
| Replay and kickoff, offline at 844×390 | 145 |
| Human and bot-filled rooms, host at 844×390 | 55 |
| Human and bot-filled rooms, guest at 844×390 | 15 |

Coverage includes VS AI 2v2–5v5, physical goal crossings, exactly four seconds
before and one second after the goal, rapid consecutive goals, camera-mode
restoration, ball framing, golden-goal results, cancellation, retained props,
conceding-side centre possession, 360-degree aiming, AI kickoff, invalid/stale
action rejection and the real guest reliable kick RPC. Render-only copies
contain no colliders, Animators or network objects. Playback cannot rescore
the goal or consume the remaining match clock.

Actual goal cards, replay approach/impact and returned views were inspected at
both resolutions. The [current illustrated card](GoalReplay/in-game-transition.png)
shows the enlarged official logo, football, shot trail and net drawings. Compact
tests ran using the default player path from a relocated directory containing
spaces and an unrelated working directory. Final evidence is in
`Builds/FootballReplayQA/Final Portable/Builds/FootballReplayQA/Run-20261015-003200/`
and `Builds/FootballReplayQA/Run-20261015-003447/`.

Separate earlier gameplay regressions passed **575 football match/physics
assertions** (`Builds/FootballMatchQA/Run-20261015-000823/`) and **81 programmed-bot
assertions** (`Builds/ProgrammedBotsQA/Run-20261015-002359/`). These preceded the
final artwork revision and are not counted as final-player assertions. An
earlier compact guest fixture cleared the host's accepted restart before the
guest observed it; keeping that fixture live for one second fixed the test
race. The final host and guest both observe the accepted angled kick.

Build source is the isolated `Builds/FootballReplayQA/SourceSnapshot`, based on
`bd62f78` plus preserved prior changes and this task. All **252 compiled gameplay
source documents** match that snapshot. Of **3,804 recorded Windows inputs**,
**3,802** still match after Android generation; platform URP resource selection
and regenerated Golf Ball local identifiers are the two differences. Evidence
is in `SourceSnapshot/Builds/FootballReplayQA/compiled-source.json` and
`windows-source.json`, plus top-level `source-comparison.json`,
`windows-manifest.json` and `windows-delivery.json` in the task evidence folder.
The shared checkout's concurrent character-identity implementation is preserved
but intentionally excluded from this delivery and these test claims. This is
not a clean commit snapshot.

AndroidSubmission at **00:37:32** is **73,482,037 bytes**, up **35,736**, with
**26,517,963 bytes** hard-limit headroom and **1,517,963 bytes** development
reserve. APK v2 signature verification passes. See the
[size record](BUILD-SIZE.md#illustrated-football-goal-replay---15-october-2026)
and [audit](APK-SIZE-AUDIT.md#illustrated-football-goal-replay---15-october-2026).
No phone is connected. Physical Android appearance, touch, loading, memory/FPS
and WAN play remain unverified.

Reviewed superseded output was archived with byte/hash recovery inventories:

- `Legacy/20261015-003935-425-football-replay-delivery/` — prior complete Windows player, pre-artwork APK and audit.
- `Legacy/20261015-003655-116-football-replay-preliminary-players/` — preliminary review players and relocation helper.
- `Legacy/20261015-002802-896-football-replay-design-cleanup/` — superseded boards and completed one-off editing helpers.
- `Legacy/20261014-234321-910-goal-replay-browser-draft/` — obsolete browser prototype.

Current captures, reports, the tested source snapshot and approved references
remain available. No staging, commit, push or history rewrite was performed.




## Fishing two-button Hold the Glow restoration - 15 October 2026

Fresh AndroidSubmission written **22:14:33 Malaysia time** measures
**73,456,241 -> 73,459,821 bytes (+3,580)**.
Headroom to the strict 100,000,000-byte boundary: **26,540,179 bytes**.
Development reserve below 75,000,000: **1,540,179 bytes**.
The enforced build limit and independent APK v2 signature verification pass.
SHA-256: `79D7A6F34CF6072214C3E5975EB542B8FE078C3156D0A70C037A1BB2752AFA54`.

Fishing restores the pictured **Hold & Countersteer** controls: one Left and
one Right button. Hold the glowing direction, switch when it changes and release
both to cool the line. The fish/gold-band meter and clear navy Catch/Tension
captions return. Timed-tap gating is removed. Continuous winding uses the earlier
law with a modest 20% speed assist. Correct-side reel drag can slip under a
sudden load to leave reaction time; spring, accumulated strain and fish forces
remain, and players still release to cool.
The shared tutorial uses the same live controls and opens once for the revision.

Current camera switching and directional framing, rod grip/lean, attached rope,
fish swimming, gravity-driven splashes and landing feedback are retained. Scoring,
Wanted, Round/Cup/Crew, six abilities/two favourites, four islands and shared room
travel remain. Matching peers require protocol **57**. No texture, font, model,
shader, recorded audio, package or realtime light is added.
[Controls and simulation](FISHING-COUNTERSTEER.md).

**2,767 passing runtime assertions across seven final reports:**

Evidence paths below are relative to the repository root.

| Review | Passes | Evidence |
| --- | ---: | --- |
| Fishing holds, camera, feedback and results | 176 | `Builds/FishingPolishQA/Run-20261015-215205/polish.txt` |
| Fishing rendered gameplay | 2360 | `Builds/FishingGameplayQA/Run-20261015-215227/offline.txt` |
| Fishing host | 45 | `Builds/FishingGameplayQA/Run-20261015-215401/host.txt` |
| Fishing client | 47 | `Builds/FishingGameplayQA/Run-20261015-215401/client.txt` |
| Golf offline | 70 | `Builds/GolfAimQA/Run-20261015-221544/offline.txt` |
| Golf host | 35 | `Builds/GolfAimQA/Run-20261015-220024/host.txt` |
| Golf client | 34 | `Builds/GolfAimQA/Run-20261015-220024/client.txt` |

All **1,571 Fishing rule assertions** also pass during the final Windows refresh.
Coverage includes valid directional holds, immediate release, renewed safety
leases, stale-release rejection, pointer takeover, all fish sizes over four cast
ranges, landing/scoring/Wanted, tension, tutorial/native glyph bounds at landscape
sizes, camera return and Reduced Motion, actual fish movement, physical water
contact/splash, rod grip/line endpoints, catch feedback, results/rematch, and
host/guest replication. Golf offline/network checks cover the shared controls.
Rendered captures were visually reviewed for both buttons, meter spacing and
retained camera/rope. Natural surface contacts are recorded during catches.
Because a fast catch need not breach, a separate controlled body-surface crossing
exercises the production splash observer and its real bounded gravity arcs;
it does not call the burst implementation or change product swimming/forces.

Complete **324-file** Windows player is promoted together to
`Builds/WindowsFinal/`; **323 runtime/support files** match the tested
candidate hashes. Full native build: `2026-10-15T07:38:05.3822437Z`; code refresh:
`2026-10-15T13:48:32.0493990Z`. Native player data, catalog and scene-bundle hashes
are unchanged. A dedicated source proof confirms that the current camera block,
rod, fish, splash, startup loader and serialized Fishing wire-record declarations
are unchanged by this task. Only the input/simulation/UI methods and review
fixtures change. Runtime C# is unchanged after final Windows QA. Generated scene
copies, canonical Golf Ball graph/IDs, desktop SSAO resource registrations and
configured text endings are checked separately. Source is main `4b20c6b` plus
preserved unstaged work; the commit alone does not contain this delivery.

Reviewed previous complete player is archived with the repository tool in
`Legacy/20261015-221826-672-fishing-hold-player/Builds/WindowsFinal/`. The previous APK is retained in the
separate fishing-hold-apk archive batch, with its initial SHA and ZIP inventory.
No staging, commit or push is performed.

Physical Android touch, appearance, first-launch storage, memory peaks, sustained
FPS and WAN play remain unverified. Windows captures render the real overlays
through the game camera; they do not qualify a phone. Full evidence: `Builds/FishingHold/Task-20261015-201923/`.

## Local content startup recovery - 15 October 2026

Fresh AndroidSubmission written **17:17:47 Malaysia time** measures
**73,452,461 -> 73,456,241 bytes (+3,780)**.
Headroom to the strict 100,000,000-byte boundary: **26,543,759 bytes**.
Development reserve below 75,000,000: **1,543,759 bytes**.
The enforced build limit and independent APK v2 signature verification pass.
SHA-256: `7F39D1F12038005F6028A80600148A792394C8C1442E263DBA164358535A5FE6`.

First-launch preparation reads an embedded filesystem bundle directly,
avoiding an unnecessary compressed-source copy on Windows. APK archive assets
retain their required temporary copy. Both paths retain length/SHA-256 and
conversion-CRC checks. Preparation is serialized between launches; owned failed
and interrupted temporary files are cleaned. Obsolete hash caches are pruned
after the current content successfully loads, preserving other files and saves.
The loading screen offers a usable storage/retry message. All four islands,
the existing Fishing gameplay and protocol **56** are retained. No asset,
recorded audio, model, shader, texture, package or realtime light is added.
[Loader and storage behavior](CONTENT-PACKAGING.md#first-launch-and-storage).

The reported player log fails while writing the converted bundle, not while
running Fishing. At investigation the drive had **727,629,824 bytes** free.
The old Windows preparation needed at least **778,366,109 bytes** of working
files: a **295,364,236-byte** copied source plus a **483,001,873-byte** runtime
cache, before filesystem/Unity overhead. Direct file preparation removes the
source-copy requirement. These Windows numbers do not estimate phone storage.

The original exact source/catalog successfully loads all four islands when its
previously validated cache is restored. Three obsolete app-generated hash caches
are then removed, reclaiming **1,449,005,567 bytes**; saves, preferences, source
assets and Editor caches are preserved. Verified identical review/runtime caches
share disk blocks through hard links rather than occupying repeated copies.

**443 passing runtime assertions across six final reports:**

| Review | Passes | Evidence |
| --- | ---: | --- |
| normal cold | 52 | `Builds/LaunchRecovery/Task-20261015-165153/DefaultCache/cold/results.txt` |
| normal warm | 52 | `Builds/LaunchRecovery/Task-20261015-165153/DefaultCache/warm/results.txt` |
| portable copy and retry | 61 | `Builds/LaunchRecovery/Task-20261015-165153/Portable Checkout With Spaces/Builds/PortableRetryQA/results.txt` |
| concurrent first | 52 | `Builds/LaunchRecovery/Task-20261015-165153/Concurrent/first/results.txt` |
| concurrent second | 52 | `Builds/LaunchRecovery/Task-20261015-165153/Concurrent/second/results.txt` |
| native Fishing | 174 | `Builds/FishingPolishQA/Run-20261015-170611/polish.txt` |

The normal cold/warm test uses the actual application cache without
`-contentCache`. Cold preparation is **55.83 seconds**, including concurrent
source hashing for evidence; warm preparation is **1.24 seconds**. The portable
retry test deliberately blocks its private cache with a one-byte file, verifies
the error screen and Retry button, then completes conversion through the copied
source path and checks owned temporary/old-cache cleanup and unrelated-file
preservation. It uses a directory with spaces, the tool's default player path
and a working directory outside the checkout. Exactly one conversion occurs
when two players start concurrently against a fresh cache. The native Fishing
run omits batch mode and starts through the normal cache without preparation
errors. All **1,576** existing Fishing rule assertions pass during the build.

Complete **324-file** Windows player is promoted together to
`Builds/WindowsFinal/`; **323 runtime/support files** match the tested
candidate hashes. Full native build: `2026-10-15T07:38:05.3822437Z`; code refresh:
`2026-10-15T08:59:55.1521879Z`. Native data, catalog and scene-bundle hashes are
unchanged; only loader and development-review methods changed. No serialized
component fields are added. Runtime C# is unchanged after final Windows QA.
Platform-generated scene copies, canonical Golf Ball graph/IDs, desktop SSAO
resource registrations and configured working-copy text endings are checked
separately. Source is main `4b20c6b` plus preserved unstaged work; the commit alone
does not contain this delivery.

Reviewed previous complete player is archived with the repository tool in
`Legacy/20261015-194101-883-launch-recovery-player/Builds/WindowsFinal/`. The previous APK is retained in the
separate launch-recovery-apk archive batch, with its initial SHA and ZIP inventory.
No staging, commit or push is performed.

Physical Android first-launch storage, memory peaks, touch, appearance, sustained
FPS and WAN play remain unverified. Desktop tests of the copied-source path do
not qualify a real phone. Full evidence: `Builds/LaunchRecovery/Task-20261015-165153/`.

## Fishing timed pulls and directional camera - 15 October 2026

Fresh AndroidSubmission written **16:14:33 Malaysia time** measures
**73,442,073 -> 73,452,461 bytes (+10,388)**.
Headroom to the strict 100,000,000-byte boundary: **26,547,539 bytes**.
Development reserve below 75,000,000: **1,547,539 bytes**.
The enforced hard limit and independent APK v2 signature verification pass.
SHA-256: `E94EAA6B2F1C8A6FA1BB88BCC4C34532A4A7D1B5463CF249AFA24848B82AE940`.

Tap Reel when the moving circle enters gold. Every third sweep asks for a
timed Left/Right counterpull through the same controls. Each sweep allows one
attempt; holding and spam never repeat a stroke. Clear arrow cues brighten at
the tap window, with contained hit/miss feedback and no keyboard guides. The
rod, torso and fish forces respond to accepted pulls, and a restrained camera
shoulder shift keeps the opposing direction readable. A tension-sensitive reel
clutch softens short strokes under load. Host and guest receive equal fixed
stroke duration; timestamp and duplicate guards remain host-owned. The revised
tutorial reopens once and shares the live widgets with How to Play.

Scoring, Wanted, Round/Cup/Crew goals, six skills/two favourites, existing catch
feedback, all four islands and shared room travel remain. Matching peers require
protocol **56**. No new image, model, shader, recorded audio file, package or
realtime light is added. [Controls and simulation](FISHING-TIMING.md).

Complete Windows delivery: **324 files**, including
**323 runtime/support files** hash-verified against the
tested candidate. The complete folder is promoted to `Builds/WindowsFinal/`.
Full native build UTC: `2026-10-15T07:38:05.3822437Z`; final code refresh UTC:
`2026-10-15T07:57:30.0162606Z`. Native data, catalog and scene-bundle hashes
remain identical through the code refreshes. Source is main `4b20c6b` plus the
preserved unstaged preceding clarity work and this local timing revision; the
commit alone does not contain the delivered feature.

**2,853 passing assertions across 9 final reports:**

| Review | Passes | Evidence |
| --- | ---: | --- |
| Packaging cold | 52 | `Builds/FishingTiming/Task-20261015-151017/Packaging/cold/results.txt` |
| Packaging warm | 52 | `Builds/FishingTiming/Task-20261015-151017/Packaging/warm/results.txt` |
| Golf offline | 70 | `Builds/GolfAimQA/Run-20261015-155841/offline.txt` |
| Fishing clarity and results | 174 | `Builds/FishingPolishQA/Run-20261015-155916/polish.txt` |
| Fishing offline | 2,343 | `Builds/FishingGameplayQA/Run-20261015-155934/offline.txt` |
| Fishing host | 46 | `Builds/FishingGameplayQA/Run-20261015-160103/host.txt` |
| Fishing client | 47 | `Builds/FishingGameplayQA/Run-20261015-160103/client.txt` |
| Golf host | 35 | `Builds/GolfAimQA/Run-20261015-160142/host.txt` |
| Golf client | 34 | `Builds/GolfAimQA/Run-20261015-160142/client.txt` |

Fishing covers all 30 fish at four ranges, timed success/miss/wrong side, one
attempt per sweep, bounded original-clock input, equal guest stroke duration,
wire serialization, same-frame touch down/up, no held repeat, camera shifts and
return, Reduced Motion, safe-frame layouts, native labels, tutorial reuse,
skills, scoring, claims and round results. Golf checks shared camera/control
regressions; cold/warm packaging covers all islands. Packaging reports precede the final HUD-label and development-probe corrections.
Their protected native payload and bootstrap/loading code are unchanged. The
remaining seven reports use the final refreshed player. Review wrappers only
adjust their relative root and share one explicit runtime cache; original
script hashes and exact changes are recorded in evidence.

Initial force tuning snapped lines too quickly; the reel clutch and final
calibration pass all catch fixtures. The first rendered run stopped at the old
Jump-visibility expectation, which was corrected without changing gameplay.
An intermediate screenshot wait also invalidated the frame-sampling fixture;
removing that extra yield restores zero measured line-endpoint error. Labels
move inside their bars after inspecting the rendered overlap. Those attempts
and final reports are retained in ignored evidence. Selected
real frames are inspected; batch captures render overlay canvases through a
camera and do not independently qualify native overlay compositing.

Runtime C# is unchanged between final Windows QA and Android. All14 generated
Golf Ball objects and GUID references match the incoming graph; equivalent
identifier drift is restored with a format-aware proof. Full native Windows
source includes two desktop resource registrations beyond Android; exact bytes
are reconstructed and checked. Project/Quality settings differ only in proved
configured working-copy line endings. The entire tested player is moved together,
avoiding a redundant runtime copy. The reviewed previous complete player is
archived by the repository tool in `Legacy/20261015-161920-551-fishing-timing/Builds/WindowsFinal/`.
No staging, commit or push is performed.

Physical Android touch, appearance, sustained FPS, installed/cache storage and
real internet multiplayer remain unverified. Windows automated events and
renders do not qualify a phone. Evidence: `Builds/FishingTiming/Task-20261015-151017/`.

## Fishing clarity, camera and catch feedback - 15 October 2026

Fishing now has a circular accepted-input cue and gold target socket,
explicit hold/switch/release instructions with stable cooling guidance, a framed
cast/wait/hook hint, contextual utility visibility, a smooth accepted-hook camera
with return to the chosen view, and confirmed local fish/points landing feedback.
Reduced Motion retains the chosen camera and simplifies the new effects. Held
gear stays clear of the landing card. The tutorial reuses the live circular meter.
No new image, model, shader, recorded audio file or package is added. Rules,
scoring, fish forces, timing, six skills/two favourites and host authority remain
unchanged. All four islands and shared room travel remain self-contained.
Matching peers use protocol **55**.

Complete **324-file Windows player** refreshed in
`Builds/WindowsFinal/`; all **323 runtime/support hashes** match
the tested candidate, plus `LATEST-BUILD.txt`. Full content build UTC:
**2026-10-15T05:54:57.7990599Z**.
Final code-only refresh UTC: **2026-10-15T06:13:25.9603889Z**, with matching full
native-data, catalog and scene-bundle hashes. Native inputs are retained in
the full-content snapshot; final compiled code inputs have their own snapshot.
Source: `main` at `4b20c6b` plus local,
unstaged Fishing changes; the commit alone does not contain this feature.
Exact source inputs and runtime hashes are retained with the evidence.

**2,755 assertions pass across 9 final reports**:

| Review | Passes | Local evidence |
| --- | ---: | --- |
| Golf offline | 70 | `Builds/GolfAimQA/Run-20261015-141519/offline.txt` |
| Fishing clarity and results | 164 | `Builds/FishingPolishQA/Run-20261015-141556/polish.txt` |
| Fishing offline | 2247 | `Builds/FishingGameplayQA/Run-20261015-141612/offline.txt` |
| Fishing host | 50 | `Builds/FishingGameplayQA/Run-20261015-141738/host.txt` |
| Fishing client | 51 | `Builds/FishingGameplayQA/Run-20261015-141738/client.txt` |
| Golf host | 35 | `Builds/GolfAimQA/Run-20261015-141817/host.txt` |
| Golf client | 34 | `Builds/GolfAimQA/Run-20261015-141817/client.txt` |
| Packaging cold | 52 | `Builds/FishingClarity/Task-20261015-131941/Packaging/cold/results.txt` |
| Packaging warm | 52 | `Builds/FishingClarity/Task-20261015-131941/Packaging/warm/results.txt` |

Rendered Fishing controls, directions, caught points and camera views were
inspected. Checks include actual cast/hook/steer/catch input, inactive controls
having no pointer targets, narrow safe frames, accepted award totals, stale and
repeated snapshots, cooling hysteresis, held-gear separation, cancellation from
first-person/overhead and Reduced Motion. First attempts are retained in the
task record; a hidden-Cancel parent lookup in the check was corrected and a
held-gear/card overlap was fixed before the final build. An initial rendered
Golf guest did not observe the ball-following phase; final rerun evidence is
listed above. No Golf gameplay code was changed.
An initial guest Hook did not enter Reeling; an unchanged-code repeat passed,
as did the final host/guest suite. The quick build initially omitted additional
StreamingAssets; the native hash guard stopped it, exact matching payload files
were restored, and the corrected build processor passed before final QA.

Runtime C# hashes are unchanged between the final Windows QA and Android.
Generated packaging scene copies vary by platform. All14 Golf Ball objects and
external GUID references match the incoming graph; regenerated IDs are restored
only after field-aware proof. The full Windows content source has two desktop
SSAO registrations beyond Android; the final code-only build retains the same
native player data. Combined player/bundle mesh inventories
match the baseline. Full source, native-graph, embedded-bundle and promotion
proofs: `Builds/FishingClarity/Task-20261015-131941/`.

Fresh AndroidSubmission written **14:31:37 Malaysia time** measures
**73,446,373 -> 73,442,073 bytes (-4,300)**.
Headroom to the strict 100,000,000-byte boundary: **26,557,927
bytes**. Development reserve below 75,000,000: **1,557,927 bytes**.
The enforced hard size gate and independent APK v2 signature verification pass.
SHA-256: `F39B0474F0F59EFFFA8F850482356F0FB26EE8BF976511742B18629343A00D56`.

Windows checks do not qualify Android phone touch, appearance, sustained FPS,
first startup or installed/cache storage. Local host/guest tests do not establish
WAN behavior. Formal round-win celebrations are not part of this change.
Reviewed previous complete player archived with the repository tool under
`Legacy/20261015-144256-647-fishing-clarity/Builds/WindowsFinal`, as required by the current AGENTS.md.
Sizes, SHA-256 hashes and recovery paths are recorded. Current evidence stays
under ignored Builds. No Git stage, commit or push requested.

## Local rebuild of pulled main - 14 October 2026

Complete Windows player refreshed in `Builds/WindowsFinal/`: all
**323 runtime/support file hashes** match the tested candidate,
plus `LATEST-BUILD.txt` (**324 files**). Full content build UTC:
**2026-10-14T12:13:45.2423825Z**. The delivery includes the embedded catalog and scene bundle.

Rebuilds pulled `main` at `4b20c6b`. No new gameplay or art change
is authored. All four islands, shared room travel, Golf camera/buggy/flags/trail,
programmed sports bots, current match/prop rules, network movement, live renderer
lighting and Fishing's existing features are retained. Protocol **55** requires
matching players. Lossless scene AssetBundle sharing and release minification
are the upstream packaging changes; this is not removal of islands or an
external content download. Installed storage also includes the expanded runtime
cache and must be measured separately on a phone.

**3,546 assertions pass in 17 reports**, with no failures
or runtime exceptions in those suites:

| Review | Passes | Local evidence |
| --- | ---: | --- |
| Packaging cold | 52 | `Builds/LocalRebuild/Task-20261014-195856/Packaging/cold/results.txt` |
| Packaging warm | 52 | `Builds/LocalRebuild/Task-20261014-195856/Packaging/warm/results.txt` |
| Character lighting | 102 | `Builds/LocalRebuild/Task-20261014-195856/Lighting/results.txt` |
| Golf offline | 70 | `Builds/GolfAimQA/Run-20261014-201726/offline.txt` |
| Golf host | 35 | `Builds/GolfAimQA/Run-20261014-201726/host.txt` |
| Golf client | 34 | `Builds/GolfAimQA/Run-20261014-201726/client.txt` |
| Fishing offline | 2232 | `Builds/FishingGameplayQA/Run-20261014-201859/offline.txt` |
| Fishing host | 45 | `Builds/FishingGameplayQA/Run-20261014-202017/host.txt` |
| Fishing client | 46 | `Builds/FishingGameplayQA/Run-20261014-202017/client.txt` |
| Basketball match offline | 166 | `Builds/RoomMatchQA/Run-20261014-202055/Basketball-offline/results.txt` |
| Basketball match host | 157 | `Builds/RoomMatchQA/Run-20261014-202055/Basketball-host/results.txt` |
| Basketball match guest | 100 | `Builds/RoomMatchQA/Run-20261014-202055/Basketball-guest/results.txt` |
| Basketball bots | 124 | `Builds/ProgrammedBotsQA/Run-20261014-202254/Basketball-offline/results.txt` |
| Football match offline | 88 | `Builds/RoomMatchQA/Run-20261014-202055/Football-offline/results.txt` |
| Football match host | 91 | `Builds/RoomMatchQA/Run-20261014-202055/Football-host/results.txt` |
| Football match guest | 89 | `Builds/RoomMatchQA/Run-20261014-202055/Football-guest/results.txt` |
| Football bots | 63 | `Builds/ProgrammedBotsQA/Run-20261014-202254/Football-offline/results.txt` |

Selected island, character-lighting, Fishing, Golf and sports-bot renders were
inspected. Cold Windows startup took **55.16 s**, including **52.69 s** preparing
the cache; warm startup took **3.14 s** on this laptop. These do not qualify
Android startup, touch, visual quality, sustained FPS or installed storage.
Local host/guest tests do not establish real WAN behavior. Other development
suites were not rerun merely because they exist.

Runtime C# hashes remain unchanged between Windows QA and Android. Generated
packaging scene copies vary by platform. Both Windows and Android Golf Ball
prefab graphs preserve all14 objects and external GUID targets relative to the
pulled index; regenerated IDs were restored only after field-aware comparison.
Two desktop SSAO runtime resource registrations explain the platform settings
difference; reconstructing them matches the exact tested Windows bytes and
the Android graph equals the pulled index. Evidence: `Builds/LocalRebuild/Task-20261014-195856/`.

Fresh local AndroidSubmission written **20:45:42 Malaysia time** measures
**90,388,492 -> 73,446,373 bytes (-16,942,119)** against
the older APK on this laptop. Headroom to the strict 100,000,000-byte boundary:
**26,553,627 bytes**. Development reserve below 75,000,000:
**1,553,627 bytes**. Enforced hard limit and independent APK v2 signature pass.
SHA-256: `51723F75318AE12D75C7494F674A1A08B79DA67C2A25F119FB4933745F7C715A`.

Prior Windows files were replaced in place under the user's no-backup preference;
only their metadata inventory is retained. No new archive batch. Build output
stays ignored; this rebuild creates no Git commit or push. Fresh measured-size
documentation remains available for user review.

The first Android attempt failed when Gradle hashed a transient `libgame.so.tmp`
file after it disappeared. Its diagnostics are retained as `android-attempt-1.log`
and `FailedAndroidAudit-1/` in the task evidence. A cached retry succeeded;
runtime C# did not change between attempts. The older local APK was not treated
as validation while the first attempt was failing.

## Golf buggy control and shot camera - 14 October 2026

The complete **324-file** Windows player built at **18:48:50 Malaysia time** is
delivered in `Builds/WindowsFinal/`. Every file, including data, runtime libraries,
launchers and the release note, matches the tested candidate. The player retains
protocol **55**, concurrent sports prop-cycle and lighting changes, four islands
and shared room travel. Source is `bd62f78` plus preserved uncommitted work.

**632 passing assertions**, indexed with report hashes in
`Builds/GolfCameraQA/verification.json`:

| Check | Passing assertions |
| --- | ---: |
| Buggy offline / host / guest | 165 / 25 / 25 |
| Five-hole shot and putt checks, cycling all camera modes | 209 |
| Rendered aim, recall and shot camera, offline / host / guest | 70 / 35 / 34 |
| Cache-prepared runner, host / guest | 35 / 34 |

Buggy checks cover manual yaw/pitch through entry, steering and reverse in all
three camera modes. Aim checks cover a buggy parked over the ball, authoritative
recall before the accepted stance, rejected summoning while aiming, cancellation
leaving the buggy recalled, and later manual summoning. The shot camera waits
for confirmed impact, follows the presented ball through flight and run-out,
holds briefly at rest and returns smoothly with the player's camera angle/mode
preserved. Interruption and input/result clearing also exercise the smooth return.

Rendered samples keep the ball inside the central 90% of the viewport for
**219/219 offline, 389/389 host and 224/224 guest frames**. The continuity checks
pass, and flight, landing and return captures were visually inspected. The aim
probe now uses the actual tee height and current power-sweep timing; its former
low fixture could embed the ball in the wooden cup. The runner prepares the
packaged-content cache before timed checks so cold extraction does not consume
their gameplay timeout.

The final aim and runner suites used their default player path from a relocated
directory containing spaces and an unrelated working directory. **43 local
documentation links** were checked in that fixture. Reports, logs and captures
remain in `Builds/GolfCameraQA/PortableFinalEvidence/`; cart and shot reports are
in `Builds/GolfCartQA/Run-20261014-185122/` and
`Builds/GolfShotQA/Run-20261014-185122/`. The final explicit-player-path
normalization is a script-only follow-up; the tested default path is unchanged.

All **247 compiled gameplay source documents** match current source. Of **3,792**
captured Windows build inputs, **3,790** still match after Android generation;
the differences are platform URP resource selection and the generated Golf Ball
prefab. These are recorded source-snapshot limits, not a clean commit snapshot.
Evidence includes `compiled-source.json`, `windows-source.json`,
`source-comparison.json`, `windows-manifest.json` and `windows-delivery.json`.

AndroidSubmission at **18:54:34** is **73,446,301 bytes**, down **7,492 bytes** from
the opening APK, with **26,553,699 bytes** hard-limit headroom and **1,553,699
bytes** development reserve. APK v2 signature verification passes. See the
[size record](BUILD-SIZE.md#golf-buggy-control-and-shot-camera---14-october-2026)
and [audit](APK-SIZE-AUDIT.md#golf-buggy-control-and-shot-camera---14-october-2026).
Physical Android appearance, touch, loading, memory/FPS and real WAN play remain
unverified; no phone was connected. Windows tests do not establish phone quality.

The superseded complete Windows delivery is recoverable in
`Legacy/20261014-185618-106-golf-camera-windows/`. Reviewed preliminary players
and disposable portability fixtures were archived with their size/hash manifests:

- `Legacy/20261014-182626-171-golf-camera-candidate/`
- `Legacy/20261014-183721-533-golf-camera-second-candidate/`
- `Legacy/20261014-184237-621-golf-camera-portable-fixture/`
- `Legacy/20261014-184314-014-golf-camera-shared-candidate/`
- `Legacy/20261014-185704-673-golf-camera-final-fixture/`

Current validation evidence remains under `Builds/`. No staging, commit, push
or history changes were performed; unrelated working-tree changes are preserved.

## Live renderer lighting fix - 14 October 2026

The complete **324-file** Windows player built at **18:35:38 Malaysia time** is
delivered in `Builds/WindowsFinal/`. Every delivered binary/data/launcher file
matches the tested candidate; the final build note adds release evidence.
Protocol **55** and the concurrent sports prop-cycle/Golf camera source are
retained. This is `bd62f78` plus the preserved uncommitted working tree.

**495 passing assertions**:

| Check | Passing assertions |
| --- | ---: |
| Four-island live shading, two circuits, boundaries and character captures | 102 |
| Cold/warm packaging, all islands and render/material/collision checks | 104 |
| Programmed basketball/football bots, offline | 124 / 63 |
| Default test paths from an unrelated directory and a path containing spaces | 102 |

Evidence is in `Builds/CharacterLightingQA/FinalReview/`, `Packaging/`,
`PortableCheck/`, and `Builds/ProgrammedBotsQA/Run-20261014-183752/`.
Front/back character views were inspected on Football, Basketball, Golf and
Fishing. Matching captures temporarily restore the former stuck SSAO settings:
these reproduce the excessive dark hair/crease patches and speckles, while the
corrected captures retain the intended lighter contact shading and normal
shadows. The fixture freezes the character pose; captures are not animation/FPS
measurements. Original materials and texture bindings are checked unchanged.

The live renderer is controlled directly through URP's camera renderer-data API.
Court/field activation boundaries, Golf/Fishing intensity/radius, restoration
across island changes and walking remain correct. Binary inspection confirms one
player-side feature and no bundled duplicate. The build dependency check rejects
island dependencies on the desktop renderer asset. Mobile has no SSAO feature.

All **247 compiled source documents** matched when validated. A later independent
change to development-only `GolfAimProbe.cs` is not included in this Windows
snapshot; it only runs with `-golfAimAudit` and is excluded from Android release.
The other **246 documents**, including all gameplay/lighting code, still match.
See `compiled-source.json` and `delivery-source-snapshot.json` in the task evidence.

Cold Windows startup was **29.22 s**, including **28.02 s** cache preparation;
warm startup was **1.30 s**, including **0.17 s** cache validation. These are
Unity-runtime measurements on a shared development machine, not phone or
controlled performance benchmarks. AndroidSubmission measures **73,448,789 bytes**
and passes APK v2 signature verification; see [size](BUILD-SIZE.md#live-renderer-lighting-fix---14-october-2026).
Physical Android appearance, touch, memory/FPS and real WAN play remain untested.
Existing Fishing reflection depth-buffer warnings are outside this reference fix.

The superseded delivery/failed initial diagnostic is recoverable in
`Legacy/20261014-184351-733-character-lighting-delivery/`. Duplicate players,
preliminary review, alternate-path fixture and test-only caches are archived in
`Legacy/20261014-184548-350-character-lighting-cleanup/`. Current screenshots,
logs, inventories, signature and delivery hashes remain under the task evidence.
No staging, commit, push or history changes were performed.

## Continuous sports prop spawning - 14 October 2026

The complete **324-file** Windows player built at **18:26:20 Malaysia time**
was delivered to `Builds/WindowsFinal/` and hash-compared with the tested
candidate. Protocol **55** requires matching peers. This is `bd62f78` plus
preserved uncommitted work, including concurrent golf/lighting code available
at build time. All **247 compiled gameplay documents** matched captured source.

**1,404 passing assertions**, all run against that final player, are indexed in
`Builds/PropCycleQA/verification.json`:

| Check | Passing assertions |
| --- | ---: |
| Football props, offline | 176 |
| Football props, rendered four-player room | 260 |
| Basketball props, rendered host/guest/teammate | 277 |
| Both sports' offline and host/guest competitive matches | 691 |

Coverage includes the first ten-second deadline, quiet initial five seconds,
visible 5–4–3–2–1 countdown, ten-second respawn after collection, one uncollected
world pickup, replacement of a held item, and replicated inventory/activation.
Basketball checks prove that a pickup can materialize during an inbound pause,
that baskets retain its deadline, and that out-of-bounds and shot-clock restarts
retain the same world object and location. Football checks prove that ball
resets preserve pending deadlines and the same pickup stays visible through
goal and kickoff. Match end/cancellation still clears props. Existing ball,
ability, scoring and team rules pass alongside these additions.

A fixture initially sampled the first timer before the next live physics step;
the corrected fixture waits for that step, and all final match suites pass.
An initial build overlapped incomplete golf source and one Android launch lost
the shared-project startup race; neither is presented as release validation.
The subsequent full Windows and Android builds succeeded.

Fresh AndroidSubmission at **18:40:11** measures **73,444,625 bytes**, down
**9,168 bytes**. APK v2 verification passes; hard-limit headroom is **26,555,375
bytes**, and development reserve is **1,555,375 bytes**. Both mesh inventories
are unchanged. See [size](BUILD-SIZE.md#continuous-sports-prop-spawning---14-october-2026)
and [packing audit](APK-SIZE-AUDIT.md#continuous-sports-prop-spawning---14-october-2026).

Source limits: later concurrent edits to `PlayerView`, `GolfAimProbe` and
`CharacterLightingReview` are present in Android but absent from this Windows
snapshot. Sports prop code is identical; this task's tests do not certify those
other features. Android also changes platform resources and generated Golf
prefab IDs; canonical comparison confirms unchanged prefab semantics. Physical
Android quality, loading, memory/FPS and WAN behavior remain unverified.

Reviewed superseded output is archived under `Legacy/20261014-*-prop-cycle-*/`.
The previous Windows player is in `20261014-183030-237-prop-cycle-windows`, and
the previous APK/audit is in `20261014-183123-339-prop-cycle-before-android`.
Initial candidates, preliminary tests, fixture failure and build-race evidence
have their own batches. Archive records and current final evidence remain in
`Builds/PropCycleQA/`. No Git staging, commits or pushes were performed.

## Persistent sports props and slower football shots - 14 October 2026

The full packaged Windows player built at **17:38:15 Malaysia time** is delivered
in `Builds/WindowsFinal/`. All **323 files** match the tested candidate, including
data, runtime libraries, embedded scenes and launchers. `LATEST-BUILD.txt` records
the changes and limits. Source is `bd62f78` plus uncommitted task changes;
protocol **54** requires matching peers.

**1,825 passing assertions** are indexed in
`Builds/PropRetentionQA/verification.json`:

| Check | Passing assertions |
| --- | ---: |
| Football props, offline and rendered four-player room | 420 |
| Basketball props, rendered host/guest/teammate | 275 |
| Final competitive match and inventory lifecycle | 681 |
| Football shot speed and attached dribbling, offline and host/guest | 165 |
| Football network ball and shot probe | 43 |
| Final portable default runner and offline ball physics | 241 |

Checks cover one-slot replacement and consumption, retained items across goals,
basketball inbounds/out/shot-clock restarts, football kickoff and stranded-ball
recovery, and clearing at match completion/cancellation. Rendered captures verify
the held prop stays visible and disabled during restart pauses. Football shot
launch is exactly 80% of its previous speed at weak, half and full charge;
existing attached dribbling behavior remains covered.

All **244 compiled gameplay documents** match captured working source. The final
full build reran match lifecycle and the default ball runner. Earlier rendered
prop, effort and network checks use equivalent gameplay source; only the
development match fixture's guest-receipt wait changed before the final build.
The runner also passes from a checkout path containing spaces with an unrelated
working directory. Android changes platform resource selection and generated
Golf prefab IDs; canonical YAML comparison confirms unchanged prefab semantics.
Provenance and delivery manifests are retained in `Builds/PropRetentionQA/`.

Fresh AndroidSubmission measures **73,453,793 bytes**, up **7,648 bytes**, with
**26,546,207 bytes** hard-limit headroom and **1,546,207 bytes** development
reserve. APK v2 verification passes; asset and mesh inventories are unchanged.
See [measured size](BUILD-SIZE.md#persistent-sports-props-and-slower-football-shots---14-october-2026)
and [packing audit](APK-SIZE-AUDIT.md#persistent-sports-props-and-slower-football-shots---14-october-2026).

Reviewed superseded players, APK/audit output, incomplete build attempts and
temporary copied checkout were archived with the repository tool under
`Legacy/20261014-*-prop-retention-*/`. The prior complete Windows player is in
`20261014-174836-819-prop-retention-windows`; the prior APK is in
`20261014-174134-473-prop-retention-before-android`. Task archive records remain
in `Builds/PropRetentionQA/archive-*.txt`; current evidence remains available.
No Git staging, commits or pushes were performed. Physical Android appearance,
loading, memory/FPS and WAN play remain unverified.

## Lossless island packaging - 14 October 2026

The complete **324-file** Windows player built at **16:33:17 Malaysia time**
is delivered in `Builds/WindowsFinal/`. Every file was hash-compared with the
tested candidate before enriching `LATEST-BUILD.txt` with the delivery record.
All **244 compiled gameplay source documents** still match current source.
This includes preserved local sports work on `main` `3a0456d`, protocol **53**,
not a clean commit snapshot. Android platform resource selection and generated
Golf ball local IDs are the only post-Windows input differences; prefab semantics
are verified unchanged.

**3,917 passing assertions** are indexed with report hashes in
`Builds/PackagingQA/verification.json`:

| Check | Passing assertions |
| --- | ---: |
| Cold/warm boot and two four-island loading circuits | 104 |
| Relocated player, space-containing path and corrupt-cache recovery | 104 |
| Full four-leg offline Sky-Sail circuit | 54 |
| Local host/guest shared travel and return | 18 |
| Basketball/football programmed bots, 2v2-5v5 and full matches | 224 |
| Both sports' room rules, offline and host/guest | 666 |
| Rendered fishing gameplay and local multiplayer | 2,323 |
| Golf offline and host/guest matches | 204 |
| Golf rolling, materials, putts and charge distances | 121 |
| Basketball local defense, directional passing and manual inbounds | 99 |

The new packaging review also checks geometry, collision, shader support,
nonblank images, missing-shader magenta and clean return from Fishing. Close
props, cabin interiors, distant islands and all four sports were inspected in
rendered captures. The package comparison matched **2,022 serialized mesh,
texture and animation objects**, including texture image bytes, across **823
asset/type groups**, with zero changed or missing groups. Existing high-quality
masters and mobile delivery settings are unchanged.

### Loading measurements

Measurements use Unity's runtime clock on Windows, not full OS process startup.
The baseline was the same working source with normal unpackaged scenes.

| Windows measurement | Seconds |
| --- | ---: |
| Unpackaged baseline reaches home | 0.812 |
| Packaged first launch reaches home | 21.401 |
| First-launch cache preparation portion | 20.550 |
| Packaged cached launch reaches home | 0.954 |
| Cached preparation portion | 0.140 |
| Subsequent detailed-island load/reload range | 0.251-0.518 |

The desktop LZMA bundle is about 295 MB and produces a **482,965,362-byte**
runtime cache. Android's embedded bundle is only **41,773,982 bytes**; these
desktop timings and cache size must not be presented as Android measurements.
The final sampled maximum startup frame gap was below 0.07 seconds, and sampled
Unity allocations peaked at 176.5/195.9 MB in cold/warm review runs. These are
not OS working-set measurements, device memory limits or FPS certification.
Evidence: `MeasuredTiming/`, `BaselineTiming/`, `PortableRecovery/` and
`ReleaseTravel/` under `Builds/PackagingQA/`.

### Repairs and limits

The packaging trial exposed a Fishing shutdown null mesh, now guarded without
changing visible gameplay. The loading bar was changed to a plain UI rectangle
after a missing built-in sprite diagnostic. The basketball bot action fixture
now performs the manual inbound for the opponent it deliberately disables;
full bot matches independently verify scoring and restarts through normal play.
One heavily concurrent fishing network run failed the three-second favourites
acknowledgement check; the isolated host/guest rerun passed **45/46** assertions.
The failed trace is retained rather than hidden. No production fishing RPC or
match logic was changed to make that test pass.

Fresh AndroidSubmission at **16:38:21** is **73,446,145 bytes**, down from
**91,647,416**, with **1,553,855 bytes** below the 75 MB target and **26,553,855**
to the strict 100 MB boundary. Build enforcement, APK v2 signing, all five
catalog scenes and separate bundle/player inventories pass. No physical Android
device is attached: first-launch time, installed cache storage, appearance,
touch controls, frame rate and WAN networking remain unqualified on phones.

### Cleanup and publishing

Reviewed superseded players and prototype caches were moved with recovery
hashes, preserving QA reports, screenshots, baseline APK, current delivery,
authoring masters, reusable tools and installed Unity/dependency caches:

- `Legacy/20261014-162906-367-packaging-review-player/`: intermediate candidate.
- `Legacy/20261014-163244-772-packaging-obsolete-players/`: 12 older test players.
- `Legacy/20261014-164257-579-packaging-prototype-cleanup/`: unpackaged comparison,
  relocated test copy and superseded temporary caches; copied recovery evidence
  stays in `Builds/PackagingQA/PortableRecovery/`.
- `Legacy/20261014-164354-934-packaging-windows-delivery/`: complete prior delivery.

All **5,144 archived files (10,552,958,308 bytes)** were hash-verified after moving.
Archival does not free disk space or rewrite Git history. Generated packaging
scene copies and all build output are ignored, and normal builds regenerate the
copies from versioned sources. Portable-path and `Check-TaskReady.ps1 -RequireApk`
checks passed: **4,511 publishable files**, **689 LFS paths**, **139 pending
add/remove operations**, including preserved earlier work. Nothing is staged,
committed or pushed by this task.

## Basketball and football defense swipes - 14 October 2026

The complete **322-file** Windows player built at **15:43:27 Malaysia time**
is delivered in `Builds/WindowsFinal/`. All copied files match the tested
player's SHA-256 manifest. Matching peers use **protocol 53**. This includes
preserved local sports work on `main` `3a0456d`, not a clean commit snapshot.

Basketball uses a main Guard button, immediate left-swipe Steal without ending
Guard, and an upward release-to-Block path. Guard auto-faces the carrier. Within
1.25 m both players retain 70% speed; a guarded steal connects after windup,
the ordinary-shot meter runs 30% faster, and scoring chance loses 20 percentage
points. Passing follows camera heading, highlights the selected teammate in
mint, and retains the lock within two player widths. Competitive scores and
boundary exits require a stationary teammate pass from outside the line;
opponents stay in their own half until release. Basketball now requires 2v2-5v5
so an inbounder always has a teammate; host bot filling remains available.

Football swaps Dash and Tackle positions. Pressure remains held while swiping
to Dash, Jump or Tackle and back. Auto-follow/facing survives the defender's own
actions, and a clear pressure tackle within 1.55 m dislodges the ball. Existing
slowdown, stamina, cooldown and recovery numbers are retained.

**2,617 selected assertions pass**:

| Coverage | Assertions |
| --- | ---: |
| New basketball controls, offline portable run and host/guest | 191 |
| Basketball touch UI, labels and screen layouts | 434 |
| Football touch UI and persistent Pressure swipe paths | 745 |
| Competitive match rules, offline and host/guest | 666 |
| Basketball network passing regression | 33 |
| Basketball defense, animation continuity and physical contacts | 246 |
| Football effort, offline and host/guest | 162 |
| Basketball bots, full match and host/guest | 140 |

The new basketball fixture verifies both 30% movement multipliers, the actual
host shot penalty, faster meter, repeated held steals, missed steals, Block,
menu cancellation, camera selection/band exit, physical receipt at all four
court edges, exact restart placement, paused clocks and centre restrictions.
Peers receive the selected teammate and simultaneous guard/steal state.
Football uses injected EventSystem touches to exercise Pressure through dash,
jump/landing, missed tackle, return-to-centre and a repeated successful tackle.
The unattended basketball bot match scored eight points with ten shots and
20 passes, including manual restarts.

Reviewed nonblank 844x390, 1280x720 and larger Windows captures of the controls,
pass badge and inbounds. The final basketball UI checks include immediate
caption/art agreement during possession changes. Earlier regressions exposed
a pressure target lost during jumping, a one-frame defensive-caption delay,
and test fixtures retaining disabled pickup or assuming the old inbounder;
these are corrected. The final match fixture verifies actual teammate receipt
before waiting for a shot-clock turnover.

All **242 compiled gameplay source documents** match their captured source.
The final control/UI/match suites ran on the delivered player. Earlier passing,
defense, football and bot evidence remains applicable: the subsequent source
changes are limited to immediate basketball slot refresh and two review
fixtures. The exact four-file comparison is saved in
`Builds/BasketballControlQA/test-source-provenance.json`.

The new runner's default paths also pass from a copied checkout folder with
spaces, launched from an unrelated temporary working directory. That is a
portable execution check using a complete copied player, not a fresh Unity
clone build. Current results, captures, manifests and source comparisons remain
under `Builds/BasketballControlQA/` and the suite folders listed in its
`verification.json`. See [APK measurements](BUILD-SIZE.md) and
[packing audit](APK-SIZE-AUDIT.md) for the fresh AndroidSubmission build.
Physical Android appearance/FPS and real WAN play remain unverified.

The repository archive tool recorded recoverable hashes and paths for these
ignored batches:

- `Legacy/20261014-154149-011-sports-controls-candidate/`: earlier candidate.
- `Legacy/20261014-154454-549-sports-controls-before-android/`: prior APK/audit.
- `Legacy/20261014-154704-340-sports-controls-windows/`: prior Windows delivery.
- `Legacy/20261014-154733-860-sports-controls-cleanup/`: superseded task players,
  copied portability checkout, corrected failing fixtures and editing scripts.

No active art was removed. No staging, commits, pushes or history changes were
performed. Existing unrelated local work is preserved.

## Programmed sports bots and training retirement - 14 October 2026

Basketball now has programmed bots for recovery, dribbling, passing, charged
shooting at assigned hoops, teammate spacing, marking, steals, jump blocks and
collected props. Both sports offer Play vs bots for **2v2 through 5v5** and
host-controlled room filling. Explore offline clears solo bots and formal rules.
The existing casual football policy is retained. Room bot identities are
sport-specific and distinct from human owners; matching peers use protocol 52.

**1,692 selected assertions pass**:

| Coverage | Assertions |
| --- | ---: |
| Programmed bots: offline sizes, actions, full matches, host/guest | 221 |
| Competitive match rules: offline and host/guest | 603 |
| Basketball passing: offline and host/guest | 64 |
| Basketball defense and pose/contact checks | 246 |
| Basketball props | 178 |
| Football effort: offline and host/guest | 162 |
| Football props | 162 |
| Movement: both sports, offline/host, 30/60/120 FPS and two cameras | 56 |

Basketball's final unattended match recorded **11 shots, 12 passes and 12
points**. A separate fixture verifies a human physically receiving a bot's
pass and a defender entering the actual guard pose. Football scored through
normal kicks at **both assigned ends**, resumed both restarts, and completed
an unattended match with one goal and 16 kicks. A goalless draw is also a valid
football result; the full-match check does not require every match to score.
Casual reaction delays and ordinary shot/movement rules apply.

Reviewed nonblank **844x390** basketball setup and 5v5 gameplay captures;
controls and the scoreboard fit. Room checks exercise live **2v2** host/guest
play, ownership, carrier resolution, filling restrictions and clean bot removal.
Offline checks cover all four supported sizes. This does not establish 5v5
multi-device performance. A 55-assertion portability fixture ran the script's
default paths from a checkout path with spaces and an unrelated temporary
working directory, using a copied complete player; it is not a fresh Unity
clone build claim.

The pass regression exposed a test timing issue: the observer checked loft
curvature before its bend snapshot finished converging. The review now waits
for the same bend threshold that it asserts; the host/guest rerun passes.
Earlier bot test fixtures placed the receiver behind a carrier after its
normal dribble delay; the corrected fixture keeps the receiver ahead.
Superseded diagnostic runs and the portable player fixture are preserved in
`Legacy/20261014-141156-525-programmed-bots-superseded-qa/`.

All **241** compiled gameplay source documents match the captured working
source. Android's build changes only four recorded input hashes: the selected
URP/graphics/quality settings and regenerated Golf prefab local IDs/order.
Canonical Golf prefab semantics and all **1,788** packed mesh rows are unchanged.
The complete **322-file** tested Windows player is promoted to
`Builds/WindowsFinal/`, with every delivered hash verified and its build note
updated. The reviewed prior 344-file player is recoverable from
`Legacy/20261014-141202-880-programmed-bots-windows/`.

The learned bridge, observations, external action injection, teacher/drill
recording, launchers, models and datasets are archived; ML-Agents, Inference
Engine and App UI are removed from the active graph. Source reference/GUID
checks found no serialized dependencies on retired scripts. No learned runtime
or model files appear in the delivered Windows player. Installed local tools
and caches stay ignored per repository instructions. See
[archive batches and active policy](PROGRAMMED-BOTS.md).

Fresh signed AndroidSubmission: **91,634,440 bytes**, **66,334 fewer** than the
opening package, with **8,365,560 bytes** to the 100 MB boundary. The **75 MB
development target is still exceeded by 16,634,440 bytes**. Size and signature
pass; physical phone appearance/FPS and real WAN play remain unverified.
Evidence: `Builds/ProgrammedBotsQA/verification.json`, selected `Run-*` folders,
suite logs, source proofs, manifest and APK comparisons. See
[release measurement](BUILD-SIZE.md#programmed-sports-bots-and-training-retirement---14-october-2026)
and [packing audit](APK-SIZE-AUDIT.md#programmed-sports-bots-and-training-retirement---14-october-2026).

## Latest upstream Golf merge - 14 October 2026

Fast-forward `ca979d5` -> `3a0456d` includes incoming trail, reference flags and
ball-physics work while retaining existing local sports/AI changes. Five stash
conflicts were resolved: both sets of release records are kept; protocol 51
remains active; regenerated Golf ball object IDs follow the incoming prefab.
The temporary recovery stash is retained, and all working changes are unstaged.

Fresh AndroidSubmission succeeds at **91,700,774 bytes**, **+1,182,492** from
90,518,282; hard-limit headroom **8,299,226 bytes**, development-target excess
**16,700,774 bytes**. Independent APK v2 verification passes. Detailed audit
and SHA-256 are in [build sizes](BUILD-SIZE.md#latest-upstream-golf-merge---14-october-2026).

Complete freshly built Windows player: `Builds/WindowsFinal/`.
Rendered ball physics **98 PASS**, rendered shots **188 PASS**, rendered offline
match **158 PASS**. Captures checked show the gold flight trail, readable close
hole-5 flag and putt guide. Headless offline match fails
`ADDRESS_HANDS_EXTEND_FORWARD_CLEAR_OF_TORSO` in both the new player and the
archived pre-merge player; this existing headless pose limitation is unresolved.
The corresponding rendered match passes. No phone or WAN qualification.

Evidence: `Builds/PullMergeQA/`; reports/captures:
`Builds/GolfBallPhysicsQA/Run-20261014-011906/`,
`Builds/GolfShotQA/Run-20261014-011906/`,
`Builds/GolfMiniGameQA/Run-20261014-011945/` (rendered pass),
`Builds/GolfMiniGameQA/Run-20261014-011906/` (headless failure),
`Builds/GolfMiniGameQA/Run-20261014-012006/` (same baseline failure).
Prior complete Windows player (344 files) archived with integrity manifest:
`Legacy/20261014-011526-139-pull-merge-windows-baseline/`.
The earlier `Legacy/20261014-011231-655-pull-merge-windows-baseline/` attempt
recorded its inventory but moved nothing because the player was running.

Initial readiness, portable paths, Git whitespace and LFS integrity checks pass.
Final readiness fails because concurrent work added `BasketballBot.cs` and
`BasketballBots.cs` without their Unity metadata after both builds. Those later
additions are preserved and excluded from this validated source snapshot; their
owning task must complete metadata and validation. Git publishing remains with
the user.

## Football radial pickup and reduced pace (14 October 2026)

Free, low footballs now attach inside the existing **1.2 m horizontal radius**
from any direction, without a facing cone or forward-foot proximity test.
Receiving cannot cross a wall, including placing a rear pickup across a wall
in front of the receiver. Existing fast/high-ball, grounded/action, ownership,
ability barrier and fresh-approach exclusions remain. Football locomotion is
**70% of its preceding pace** for human players, guests and bots. Walk/run/dash
are **4.2 / 7.35 / 10.6575 m/s**; dribbling, charging, Turbo and pressure use
the reduced base. Other sports, kick speed, jump physics and tackle/hit impulses
are unchanged. Rooms require matching **protocol 51** builds.

**803 assertions passed across 12 reports**:

| Suite | Passed | Coverage |
| --- | ---: | --- |
| Football ball | 241 | 32 pickup bearing/facing combinations, outside radius, wall/high/airborne/fast-ball rejection, release/ownership, kick/collision/net/field-edge behavior |
| Rendered effort | 134 | Actual speed, dash, charge, pressure/Turbo, tackles and touch controls |
| Effort host/guest | 33 | Rear pickup replication, reduced guest speed, pressure and input expiry |
| Rendered football abilities | 162 | Turbo speeds, portals, walls, inventory and match reset |
| AI offline/host/guest | 177 | Equal human/bot walk/run/dash at 30/60/120 Hz, rear pickup, tactics, room authority and full matches |
| Football/basketball movement | 56 | Offline/host, chase/stadium, 30/60/120 FPS: zero stationary movement frames, football 7.35 m/s, basketball unchanged at 7 m/s |

Two 120-second 2v2 bot matches completed legally, **0-1** and **0-0**, with
**27 kicks and 23 completed passes** combined. Longest stationary-ball spells
were approximately **1.20 s** and **1.18 s**. This is a gameplay regression
sample, not a human difficulty study or learned-policy qualification.
Explore Offline remains bot-free; Play vs AI retains its separate 2v2-5v5
selector and match rules.

Evidence index: `Builds/FootballControlQA/verification.json`. It lists exact
passing report paths. Rendered close/stadium views are retained under
`Builds/FootballPaceQA/Reviews/Offline-20261014-003828-646/`. The initial ball
diagnostic expected the former interception position rather than newly attached
possession; the corrected test checks either a stopped physical interception
or actual receiver ownership at the foot. A headless ability run cannot validate
shader material state; the passing ability suite was rerun with Direct3D.

Complete Windows player built **00:42:32 Malaysia time**, promoted to
`Builds/WindowsFinal/`, with **344 files hash-verified** against the candidate.
All **242 compiled C# documents** match the working source. The final rebuild
only isolates the AI speed-test runners from other players; prior ball,
effort and ability reports use identical gameplay source. No earlier delivery
is presented as validation of new gameplay. Superseded delivery is preserved
in `Legacy/20261014-004711-225-football-control-windows/`; prior Android output
and audit in `Legacy/20261014-004605-879-football-control-android/`.

Windows-to-Android source hashing reports **3,744/3,746 matching inputs**
(excluding the generated performance-run record from mismatch detection).
The two differing generated inputs are the URP global settings asset, whose
post-Android content has no Git diff, and the Golf ball prefab. Structural
inspection proves the prefab differs from the repository version only in
local IDs and object ordering; its generated bytes are retained, not blindly
reverted over pre-existing work. No gameplay script differs between builds.

Training remains paused. No cloud spending, new learning run, candidate
promotion or automatic bot replacement occurred. Existing checkpoints and
reports are retained but need evaluation under the new mechanics before
further training. No phone or real-WAN validation was available. See
[pickup/pace rules](FOOTBALL-PACE-AND-PRESSURE.md) and the fresh
[APK measurement](BUILD-SIZE.md#football-radial-pickup-and-reduced-pace---14-october-2026).

## Training recovery and Fishing merge (13 October 2026)

Fast-forwarded **9b4eb15 -> ca979d5** and merged the uncommitted sports/AI
work back in. All **129 pre-merge local files remain**; the retained snapshot
and per-file comparison are under `Builds/TrainingRepairQA/Before/` and
`local-preservation.json`. Recovery stash
`ada0af0a2681715bd794316aba57015a8f4f859d` is retained as a backup, not something
to reapply. No feature commit or push was made; the index is left unstaged.
Protocol **50** separates this combined contract from both branches' protocol 49.

Selected gameplay checks pass **3,884 assertions across 24 reports**, with
no targeted runtime exceptions in **21 logs**: room Basketball/Football,
football AI, network Pressure, Fishing gameplay/polish, Golf cart and Storybook
menus. The final Windows build passes **80 editor room-rule checks**. All
**242 compiled runtime documents** match the final source snapshot. Comparison
with the preceding player isolates the last two changed runtime files to
development-only teacher recording/masking; normal gameplay did not change.
The complete player is delivered together in `Builds/WindowsFinal/`.

The trainer now mixes full-contract collection/shooting/passing/defense,
uses bounded progress rewards, per-team outcome metrics and qualification
gates before 2v2-5v5 advancement. Teacher state/action pairs are captured
together rather than using the stock recorder's preceding action. The pinned
MA-POCA cloning targets account for its clipped Gaussian action scale.
**19 Python regression tests** cover pairing, final sample retention, invalid
actions, action-scale round-trip, gates, deadline status and checkpoint selection.

Teacher recording and ML-Agents loader round-trips pass for moving-defense
skills and all four team sizes, including Team B. The 40 moving-skill teacher
trials have **37 successes**, with zero rejected actions. Windows PowerShell
launches a relocated script tree containing spaces from an unrelated working
directory; the one-hour dry run and six real legacy evaluation trials pass.
This is a script portability check with an explicit player override, not a
fresh full Unity clone build. No cloud training or automatic bot replacement.

The [archived first four-hour run review](PROGRAMMED-BOTS.md#retired-training) remains the
baseline. New bounded pilot results are retained under
`Builds/FootballTrainingQA/Plans/recovery-paired-pilot/`; the user-run one-hour
experiment is not started by this task. See [training instructions](PROGRAMMED-BOTS.md#retired-training).

The diagnostic pilot used **9:50 of a 12-minute cap**, including a deliberate
pause/resume to repair action scaling; this is not a clean training benchmark.
Its foundation gate scored collection **1/20**, shooting **15/20**, passing
**16/20**, stationary defense **16/20**. Correctly, it did not advance.
Independent moving-defense checks scored collection **3/20**, shooting
**17/20**, passing **16/20**, defense **0/20**; corresponding random scores
were **1/20, 8/20, 7/20, 0/20**. This establishes useful shooting/passing
learning in narrow fixtures, not overall football competence. Recovery and
moving defense remain blockers for learned-bot deployment.
The four final 60-second 2v2 games were all losses, with **0-26 aggregate
goals**. The selected ONNX is **186,592 bytes**. It is retained for diagnosis,
not installed as a normal opponent.

The complete prior Windows delivery is archived in
`Legacy/20261013-225109-906-training-recovery-windows/`; superseded diagnostic
players are in `Legacy/20261013-222154-004-training-recovery-old-player/` and
`Legacy/20261013-225245-024-training-recovery-pre-pairing-player/`.
Generated timer telemetry was moved to
`Legacy/20261013-225508-085-training-recovery-player-telemetry/`. All **344
remaining delivery files** match the validated player by SHA-256.

Fresh signed AndroidSubmission: **90,515,790 bytes**, **9,484,210 bytes** to
the hard boundary and **15,515,790 bytes over** the development target.
[Size record](BUILD-SIZE.md#training-recovery-and-fishing-merge---13-october-2026).
Generated Golf ball prefab IDs were normalized only after structural
equivalence checks; source content and active references are unchanged.
Physical-phone performance, touch/appearance, WAN/Relay and human AI difficulty
remain unqualified. Desktop inference timing is not phone performance.


## Teammate merge and training launcher (13 October 2026)

`main` fast-forwarded **eaad195 -> 9b4eb15** and the uncommitted sports/AI work
was restored and merged. All **105** pre-merge local files were preserved;
94 matched ignoring line endings and 11 required combined content. Further
changes in this phase are recorded below. Recovery stash
`76abbc3c7987d41e8e19cab457c3c7853be40a18` remains as a backup, not something to
apply again. No local feature commit or push was made. Teammate Golf/Fishing
work and both sides' historical release records are retained. Protocol is
**49** to cover the combined bot and Fishing Hook RPC contracts.

Selected checks pass **3,835 assertions across 35 reports**, with no targeted
runtime exceptions in **32 logs**, plus **80 editor pure room-rule checks**:
room rules, 5v5 solo/host/guest AI, tackle/Pressure, movement, Golf matches/HUD/
swing/cart/shot guide, Fishing gameplay/results and the Storybook menu. An
extra cart check caught imported-axis foot rotation in the incoming pose; it
now preserves the relaxed 22-degree pitch using the actual toe direction.
The near-cup putt fixture now accounts for the teammate's square-root power
mapping. Both initially failing suites pass: **221 cart + 188 shot checks**.
Rendered Golf seated feet, Golf HUD and compact Fishing results were inspected.
The teammate's normal five-second Golf startup countdown is bypassed by its
existing diagnostic mode, so these probes do not qualify its normal visuals.

The final Windows player was built **17:26:13 Malaysia time**. All **234
compiled runtime documents** match final source. Initial merged checks used
the preceding build: **228/233** source documents match final, with differences
only in the subsequently tested Golf pose/probes and development trainer.
Golf-fix validation has **231/233** unchanged documents; the remaining changes
are development-only learning code. Later room/AI regressions have **233/234**
identical documents; only the skill drill's programmed pass receiver changed.
That final fixture passed 30 teacher trials: **10/10 shooting, 10/10 passing,
8/10 defense**, **475 accepted actions, zero rejected**. These teacher results
validate the fixture, not learned competence.

The [live-progress local launcher](PROGRAMMED-BOTS.md#retired-training) has eight passing
Python contract tests. A final-player short run trained/exported and loaded
the same **186,592-byte** full-contract policy through mixed skills and **2v2,
3v3, 4v4, 5v5**, then evaluated random/candidate skills and both team sides
against casual bots (2v2 A and 5v5 B). All action masks were respected. The
smoke candidate won neither match and passed none of its three skill trials;
it is explicitly not a competent opponent. Stop-file pause and resume
preserved the run and consumed budget. Two additional 25-second worker tests
stopped at their time limits and saved at **1,024 -> 1,792 steps**, proving
actual checkpoint resume. The four-hour run was **not** performed.

The launcher dry run and a six-trial real evaluation also pass from copied
scripts under a checkout path with spaces, launched from an unrelated working
directory. Python/player locations were explicit overrides; this is not a full
independent Unity clone build. Portable-path and final task-readiness checks
pass, including the measured APK budget check. The index remains empty.

Current evidence: `Builds/MergeTrainingQA/` (source preservation/comparisons,
gameplay summary, `SkillTeacherFinal/`, `BudgetStop/`, APK audits and hashes),
`Builds/FootballTrainingQA/Plans/skills-final-resume-smoke/`, its five model
folders under `Models/`, and the timestamped gameplay runs listed in
`gameplay-verification.json`. Earlier merged bridge and collection-model reload
checks passed: ten agents, 176 observations and explicit reset; the prior
diagnostic collection model reproduced **64/100** with no rejected actions.

The complete **345-file** final player is delivered in `Builds/WindowsFinal/`,
with data, DLLs, launchers, build note and all files hash-verified. The previous
player is in `Legacy/20261013-173638-686-teammate-ai-merge-windows/`; intermediate
players/APK were archived in the `merge-golf-validation-rebuild`,
`training-skills-old-validated` and `merge-golf-tested-player` dated batches.
The superseded intermediate player and first smoke weights are in
`Legacy/20261013-173957-931-merge-training-intermediate/`; final smoke weights
and all current validation evidence remain available.
No active art was archived. The fresh signed APK is **90,384,954 bytes**;
[size/headroom](BUILD-SIZE.md#teammate-merge-and-training-launcher-13-october-2026).

User choices are settled: up to four hours locally, casual/fun first, normal
mid-to-high-tier Android, no test phone yet. No further product decision is
needed to run this football curriculum. Default bots remain programmed;
learned-policy promotion and basketball learning are later gates. Phone
touch/FPS/memory/heat/inference, WAN/Relay, human difficulty and broader skill
generalization remain unverified. The 75 MB development-size shortfall and
stadium-camera roof occlusion remain open.

## Football training checkpoint (13 October 2026)

Selectable solo and host-filled **2v2, 3v3, 4v4 and 5v5** are implemented,
with larger-team support/cover spacing. The casual programmed bots remain the
default. Explore Offline and the established match/defense rules are unchanged.
The [training checkpoint](PROGRAMMED-BOTS.md#retired-training) describes the separate
development-only bridge and learned collection prototype, not trained football
tactics or basketball AI.

The final Windows player was built **16:16:40 Malaysia time**. **1,503 gameplay
assertions pass**, with no targeted diagnostics in **22 runtime logs**:

- Final 5v5 compact 844x390 solo, host and guest: **215** assertions, including
  four full 180-second regulation matches (seeds 1729-1732), physical shooting,
  tactical/ability drills, capacity, authority, disconnects and cleanup.
- Final 2v2 desktop 1600x900: **113**. Final headless solo/host/guest at 2v2,
  3v3 and 4v4: **136, 138 and 140**. Menu checks click all size choices and
  the actual Start button, verify the ten-player roster, then switch to 2v2.
- Offline/network defense regression: **158**. Basketball and football room
  rule regression: **603**. Also **80 editor pure-rule checks** pass.

The four final 5v5 matches finish **1-1, 1-0, 1-0, 1-0** with **210 accepted
kicks, 172 receptions, five physical goals, 115 body tackle hits and seven
ability uses**. The longest stationary-ball interval is **1.16 seconds**.
These are small automated samples, not proof of perceived human difficulty.
Compact menu/live and desktop captures were inspected; ten athletes render,
the field is nonblank, and the team-size controls fit without overlap.

Defense/room regressions used the preceding player. Compiled-document
comparison proves **223/227** documents identical to the final player; the
four differences are the AI-review fixture, learner, trainer bridge and setup
menu. Those changes were tested on the final player. All **227 final compiled
runtime documents** match current source. Complete **345-file** delivery was
copied and hash-verified into `Builds/WindowsFinal/`, including its updated note,
data, DLLs and launchers. Protocol remains **48**, with no wire-format change.

Final-player Python protocol tests pass with 4, 6, 8 and 10 agents, 176-float
full-match observations, action masks and explicit resets. A 2,400-step 5v5
bridge test also crosses two natural match endings, producing **20 terminal
agent notifications**. It uses random legal actions, not a learned policy;
its poor match activity is not offered as a gameplay-quality result.

The selected learned collection model uses 8 projected floats and 2 movement
actions. **1,000 teacher demonstrations + 20,004 PPO/behavioral-cloning steps**
produce a **76,086-byte ONNX**, converted to **76,144 bytes** for Unity.
Independent confirmation seed 23001 collects **72/100**, versus random **34/100**.
Diagnostic seed 22001 collects **64/100**, versus **22/100**, reproduced by
loading the model in the final player. The diagnostic seed was also used for
an earlier rejected prototype, not treated as a wholly fresh holdout.
The prototype is weaker than its teacher and does not pass, shoot or defend.

Resume loads checkpoint step 20,004, preserves the original run logs and
exports byte-identical ONNX (SHA-256
`19FB001C6A2EEDC73CB2050027C93404F13E9694A4071E77CF3D931899EBE79B`).
A copied evaluation script/model under a checkout path with spaces runs from
an unrelated working directory. This exercises repository-root resolution and
quoted model/player arguments; it is not a complete independent Unity clone
build. No trained weights were added to active Unity assets or normal play.

Evidence: final AI runs `Builds/FootballAIQA/Run-20261013-162202/`, `162631`,
`163002`, `163126` and `163251`; defense reviews `Offline-20261013-161222-650`
and `Local-20261013-161230-547` under `Builds/FootballPaceQA/Reviews/`;
`Builds/RoomMatchQA/Run-20261013-161232/`; and `Builds/FootballTrainingQA/`
for `gameplay-verification.json`, source/delivery hashes, `Selected/`,
`Models/collect-relative-v1/`, `checkpoint-verification.json`, timestamped bridge
results and evaluations. Final CPU inference/reload is in
`Evaluation-20261013-162328/`; independent learned/random evaluations are
`Evaluation-20261013-161436/` and `Evaluation-20261013-161448/`.

The previous complete Windows player is archived in
`Legacy/20261013-163533-565-football-training-windows/`. Superseded training
players and rejected full-vector experiments are preserved in
`Legacy/20261013-163602-919-football-training-superseded/`. No active game art
was archived. The fresh signed APK is **90,242,332 bytes**;
[size/headroom and audit](BUILD-SIZE.md#football-training-checkpoint-13-october-2026).
Task-readiness and portable-path checks pass; nothing was staged, committed
or pushed. No Android device was connected. Phone touch/FPS/inference cost,
WAN/Relay and human playtesting remain unverified; the development-size
shortfall and stadium-camera roof occlusion remain. Stop at this checkpoint
before lengthy full-match learning or replacing the default opponent.

## Casual football calibration (13 October 2026)

The approved casual target is implemented as `football-casual-v1`, shared by
solo teammates/opponents and host-controlled room bots. Attacking decisions
remain 0.18-0.25 seconds; off-possession decisions are 0.28-0.38 seconds. Bots
mark for at least 0.65 seconds before a slide, space slide commitments by
3.5-4.25 seconds, and wait one second before a defensive Portal/Wall response
to a new carrier. Close clear shots retain priority and existing aim accuracy.
Human controls, 1.5-second tackle cooldown, Pressure penalties, room rules and
Explore Offline are unchanged. No learned-AI training or trainer integration
was started. [Current policy](PROGRAMMED-BOTS.md).

Same-seed all-bot comparison, four 180-second regulation matches per policy
(8119-8122), passes the limits in `Tools/Build/Compare-FootballCalibration.ps1`:

| Metric | Previous tactical baseline | Final casual |
| --- | ---: | ---: |
| Total elapsed seconds, including restarts/overtime | 740.25 | 751.33 |
| Accepted kicks | 96 | 143 |
| Teammate receptions | 79 | 98 |
| Physical goals | 7 | 8 |
| Body tackle hits per elapsed minute | 20.83 | 12.06 |
| Idle time / elapsed time | 13.39% | 7.27% |

Body-hit rate is **42% lower**, without losing kicking, passing or scoring in
this sample. Final scores are **0-0, 2-3, 1-1, 0-1**. This is a small automated
comparison, not a human difficulty rating, target win rate or proof of fun.
Receptions are not a pass-quality grade. Physics and pickup spawning are not
bit-identical, even when tactical seeds match.

Four additional full-length seeds **9041-9044**, not used to choose the tuning,
finish **0-0, 1-0, 1-2, 1-2**: **108 kicks, 83 receptions, seven goals,
158 body hits and six ability uses**. Idle time is 56.24/739.93 seconds;
the longest stationary interval is **2.04 seconds**. All eight final long
episodes retain at least six kicks, a reception and no 12-second stall.
The short desktop smoke has one decisive result and three legal draws.

The first casual candidate slowed all decisions and failed the kicking-rate
comparison (49 kicks versus 96). It was rejected, not relabelled as passing.
Restoring quicker team-in-possession decisions produced the final result.
The comparison thresholds were not relaxed. Candidate evidence and source
hashes are retained in `Legacy/20261013-143809-827-football-casual-first-calibration/`.

Windows player built **14:39:38 Malaysia time**. **1,304 assertions pass**:
AI desktop/offline/host/guest **158**, compact 844x390 **96**, two four-match
long batches **284**, offline/network defense **163**, both sports' room rules
**603**. **80 editor pure-rule checks** also pass. No targeted diagnostics in
**15 current runtime logs**. Drills verify bounded decision cadence, quicker
attack decisions, initial marking delay, post-tackle breathing room, re-entry
reset, delayed abilities, legal physical tackles, mirrored physical shooting,
passing/wall lanes, navigation, recovery, input expiry, authority and cleanup.

Defense and room regressions ran on the first casual candidate. Their modules
are source-identical in the final player: **223/225 compiled documents** match,
with only the bot policy and its AI-review fixture changed afterward. All
**225 final compiled documents** match current source. The complete **322-file**
player was copied to `Builds/WindowsFinal/` and hash-checked before updating its
delivery note. Protocol remains **48**; no network wire contract changed.

Evidence: `Builds/FootballAIQA/Run-20261013-142743/` (previous-player baseline),
`Run-20261013-144032/` (comparison), `Run-20261013-144043/` (desktop/network),
`Run-20261013-144120/` (compact), `Run-20261013-144244/` (additional seeds),
and `Builds/FootballCasualQA/` for the comparison, held-out summary, source
proofs, full regression inventory and delivery hashes. The comparison script
also produces an identical report from a copied checkout path containing
spaces, launched from an unrelated working directory. Compact live and guest
captures were inspected; gameplay assets/rendering were not changed.

The previous complete Windows player is preserved in
`Legacy/20261013-144659-671-football-casual-windows/`. The previous APK/audit is
in `Legacy/20261013-144448-226-football-casual-before-android/`. Fresh release
measurements are in [BUILD-SIZE.md](BUILD-SIZE.md#casual-football-calibration-13-october-2026).
No staging, commit or push was performed.

Limits remain: human playtesting for perceived difficulty, Android touch/FPS/
runtime cost, WAN/Relay behavior, the pre-existing 75 MB development-size gap,
and stadium-camera roof occlusion near some touchlines. Basketball AI is not
part of this football calibration. Stop here before the learned-AI phase.

## Football defense and tactical AI (13 October 2026)

The [defense and tactical policy](PROGRAMMED-BOTS.md) add a 1.5-second
tackle cooldown, a wider 0.8-metre sweep, ball-first steals and automatic
goal-side Pressure. Defender movement is 70%; the guarded carrier's complete
horizontal movement is 75%, including Dash/Turbo, without stacked penalties.
Room rules, assigned goals, no free throws/penalty kicks and isolated Explore
Offline remain intact. Football bots now pass, cover, use abilities and navigate
walls through normal actions. They are programmed, not trained or skill-rated.

Windows player built **13:40:14 Malaysia time**. **2,032 assertions pass** in
the selected current evidence, with no targeted runtime diagnostics in **28 logs**:

- AI desktop/offline/host/guest: **136**; compact 844x390: **77**; two headless
  180-second regulation episodes: **90**. Includes legal physical shooting,
  actual reacquisition after kick lockout, all four pitch edges, pass/shot lanes,
  hunter/cover roles, wall detours, all three abilities, contract version 2,
  input expiry, authority, guest replication, cancellation and cleanup.
- Pressure/effort: **163**, offline and host/guest. Covers 30/60/120 Hz,
  run/Dash/Turbo penalties, automatic follow, release, visibility/range, travel,
  teammates, no stacking, touch controls and the exact tackle cooldown boundary.
- Tackle regression: **210**; football match/ball physics: **535**; abilities:
  **162**; both sports' room rules: **603**; hosted/offline movement: **56**.
  The host remains 10.5 m/s in football and 7 m/s in basketball without stalls.

**80 editor pure-rule checks** also pass. Reused non-AI regressions precede the
last bot-policy and AI-review changes: **223/225 compiled source documents**
are identical between candidates; only `FootballBot.cs` and `FootballAIReview.cs`
differ. All **225 compiled documents** in the final player match current source.
The complete **322-file** player was promoted to `Builds/WindowsFinal/` and
hash-checked before updating its delivery note. Protocol **48** requires matching
players. No phone performance claim is made from these Windows checks.

Four 45-second desktop seeds (1729-1732) all draw 0-0: **27 accepted kicks,
25 teammate receptions, 67 body tackle hits and one ability use**. Held-out
180-second seeds 8119/8120 finish **1-0 and 0-0**, with **45 kicks, 30 receptions,
117 body tackle hits and two ability uses** combined. Stationary-ball time is
**22.52/181.74** and **20.12/190.66 seconds**. Idle time is counted only in live
play; total elapsed time includes restarts and overtime where applicable.
Receptions are not a tactical pass-quality rating.
These are functional smoke metrics, not proof of strong attacking play.

An intermediate long run scored but then left the ball stationary for **152.48
seconds**. Trace inspection found loose-ball back-pass turning and a recovery
condition that watched the player's speed. Both were corrected. A targeted
physical lockout-recovery drill and a per-episode stationary-time ceiling of
50% now cover that failure. Short matches permit legal draws; the full-length
evaluation batch must demonstrate scoring. Seeded decisions do not imply
bit-identical physics or ability-spawn randomness across runs/platforms.

Current AI evidence: `Builds/FootballAIQA/Run-20261013-134105/`,
`Run-20261013-134115/` and `Run-20261013-134156/`. The full regression inventory,
runtime scan, compiled-source comparison and delivery hashes are under
`Builds/FootballDefenseAIQA/`; its `verification.json` names every reused run.
Compact live and default third-person defense captures were inspected. The
stadium camera can still be occluded by the roof near some touchlines; that
camera issue is not fixed in this phase. Android touch, visuals/FPS, AI runtime
cost and WAN/Relay play still need device/network testing.

Previous complete player: `Legacy/20261013-134402-136-football-defense-ai-windows/`.
Superseded failures/benchmarks are preserved in
`Legacy/20261013-133153-346-football-defense-ai-diagnostics/` and
`Legacy/20261013-133808-909-football-ai-loose-ball-recovery/`, not counted as
current passing validation. Superseded passing AI traces/captures are in
`Legacy/20261013-134647-331-football-ai-superseded-passes/`.
Earlier/intermediate APKs are retained in dated
`football-defense-ai-before-android`, `football-defense-ai-intermediate` and
`football-defense-ai-final-fixture` archive batches. No Git publishing actions
were taken. Final APK measurements are recorded in
[BUILD-SIZE.md](BUILD-SIZE.md#football-defense-and-tactical-ai-13-october-2026).
Human difficulty calibration is the next gate, before claiming AI readiness.

## Football AI foundation (13 October 2026)

The [football AI foundation](PROGRAMMED-BOTS.md) adds a separate 2v2
Play vs AI mode, host-added room bots, stable non-human identities and a
development-only reset/observation/action/report interface. Explore Offline
stays isolated. The policy is programmed, not trained, and is not yet a
validated strong opponent. Basketball AI and ability-aware tactics are pending.

Windows player built **00:19:28 Malaysia time**. **2,278 assertions pass** across
the current evidence, including repeated pure rules in the room suite:

- AI desktop host/guest/offline: **84**, covering balanced teams, identities,
  normal-rule starts, physical play, server authority, replication, input expiry,
  stale/duplicate action rejection, disconnect cancellation and lobby cleanup.
- Compact 844x390 AI: **64**. Headless AI: **64**. A copied player/test script in
  a path containing spaces, launched from an unrelated working directory:
  **64**. Desktop and compact menu/live/result captures were inspected.
- Existing room matches: **603**, covering both sports' competitive rules,
  exploration separation, restarts, fixed goals, scoring and guest behavior.
- Existing football match/physics: **533**, including physical kicks, goals,
  formations, resets and network synchronization.
- Menu regression: **848**, including button hit targets/text fitting, settings,
  conditions, customization, offline entry and host/guest lobby controls.
- Two-process Sky-Sail travel: **18**, covering the human group going to Golf
  and back to Football, shared destination and restored network transforms.

The four desktop evaluation seeds produce **18 accepted physical kicks,
5 goals and 15 teammate receptions** across four completed short matches.
One is a scoreless draw. These are smoke metrics, not evidence of strong
opposition or a calibrated difficulty tier. The same seed choices aid repeatable
tests, but cross-platform/frame-schedule bitwise physics determinism is not claimed.
There is no trainer bridge, learned model or complete training pipeline yet.

**80 editor pure-rule assertions** also pass. No targeted runtime diagnostics
in **23 current logs**. Corrected early issues included the room prefab choice,
pre-spawn network-variable writes and an endpoint-only movement fixture that
mistook a post-goal reset for no movement. The older menu review expected the
superseded feedback component/protocol; it now checks the current sports
feedback and configured protocol. Superseded failures are archived, not counted.

Current evidence: `Builds/FootballAIQA/Run-20261013-001930/`,
`Run-20261013-001505/`, `Run-20261013-002159/`, `HeadlessPortable/`, `SkySail/`,
`verification.json`, `runtime-diagnostics.json`, `build-20261013-001754.log`,
`Builds/RoomMatchQA/Run-20261013-002035/`,
`Builds/FootballMatchQA/Run-20261013-002209/` and
`Builds/StorybookMenuQA/Run-20261013-002025/`.
Compact/portable/travel tests precede the final development-only menu-review
fixture update; gameplay source is unchanged between those candidates.

The complete player is promoted to **Builds/WindowsFinal/**. All **322 files**
match the tested candidate before updating the delivery note. All **224 compiled
source documents** match. Of **3,682 Game inputs**, 3,680 match after Android;
the generated prefab and URP-registration differences are recorded in the
[size audit](APK-SIZE-AUDIT.md#football-ai-foundation-13-october-2026).
Protocol **47** requires matching builds. Existing room rules and the hosted
movement fix are retained.

AndroidSubmission and independent APK v2 verification pass: **90,126,542 ->
90,143,514 bytes (+16,972)**, with **9,856,486 bytes** hard-limit headroom.
The 75 MB development target is exceeded by **15,143,514 bytes**. No Android
device was connected: phone input/visuals/FPS, bot runtime cost and WAN/Relay
latency remain unverified. Automated bot matches do not replace human playtests.

Archives: `Legacy/20261013-001445-260-football-ai-before-android/`,
`Legacy/20261013-002059-797-football-ai-intermediate-apk/`,
`Legacy/20261013-002432-126-football-ai-diagnostics/` and
`Legacy/20261013-002444-485-football-ai-windows/`.
Intermediate passing regressions are archived in
`Legacy/20261013-002634-395-football-ai-old-regressions/`.
No staging, commit or push.

## Room matches (12 October 2026)

The [competitive rules phase](ROOM-MATCH-RULES.md) is implemented. Basketball
has host-authoritative team scores, fixed attacking hoops, 3/5/10-minute matches,
20-second possession clocks, protected two-second restarts, buzzer resolution
and 60-second golden-basket overtime. Both sports reject ordinary offline match
starts and immediately void an active match when a participant leaves. Explore
Offline remains practice/exploration. No free throws, penalty kicks or AI added.

Validated Windows player built **22:30:35 Malaysia time**. **1,492 assertions
pass**, including repeated pure rules in each room-review process:

- Room-match desktop suite: **603** across basketball/football offline isolation
  and two-process host/guest runs. Covers fixed hoops at four headings per team,
  locked teams, score ownership, physical 2/3-point baskets, possession/restart
  clocks, boundary ownership, abilities/input gating, stale network requests,
  overtime, disconnect cancellation, rematches and shared-travel rejection.
- Compact 844x390 suite: **217**, with rendered selection, live play, score
  restart, shot-clock turnover and final-result captures. Also inspected 1600x900
  captures. The canvas, court and character visuals render without new overlaps.
- Existing football match/physics suite: **533**, including whole-ball goals,
  both resets, net impacts, overtime/results, formations and host/guest sync.
- Existing basketball gameplay: **98 offline + 41 host/guest**, including physical
  shots, catches, passes, input RPCs, boundary fencing, solo inbound, Free roam,
  island exits/returns and practice-mode disconnect recovery.

**80 pure assertions** also pass in the editor before building. No targeted
runtime diagnostics in **17 logs**. Earlier failed fixtures were corrected:
the boundary launch must be dynamic, and team choices must arrive before the
selection deadline. Their superseded evidence is archived, not counted as passes.

Current evidence:
`Builds/RoomMatchQA/Run-20261012-223039/`,
`Builds/RoomMatchQA/Run-20261012-223302/`,
`Builds/RoomMatchQA/verification.json`, `runtime-diagnostics.json`,
`pure-rules.txt`, `build-20261012-222912.log`,
`Builds/FootballMatchQA/Run-20261012-223150/`,
`Builds/BasketballGameplayQA/Offline-20261012-223226/` and
`Builds/BasketballGameplayQA/Local-20261012-223335/`.

The complete player was promoted to **Builds/WindowsFinal/**. All **322 files**
matched the tested candidate before updating its delivery note; all 321 remaining
runtime/support files retain those hashes. All **219 compiled source documents**
match. Of **3,670 Game inputs**, 3,668 match after Android; only regenerated Golf
prefab IDs/order and URP resource registration differ. Prefab equivalence passes,
and original IDs/order were restored. Protocol **46** requires matching builds.

AndroidSubmission and independent APK v2 verification pass: **90,099,110 ->
90,126,542 bytes (+27,432)**, with **9,873,458 bytes** hard-limit headroom.
The 75 MB development target is still exceeded by **15,126,542 bytes**.
[Full size audit](APK-SIZE-AUDIT.md#room-matches-12-october-2026).
No device was connected: actual Android touch, visuals, FPS and WAN/Relay latency
remain unverified. Scene-level network runs used two players; balanced 2-10-player
rosters are covered by pure rules, not full 10-device playtests. AI training still
needs its environment/agent harness and evaluation; this phase does not claim a
trained agent or training-ready pipeline.

Archives: `Legacy/20261012-223506-586-room-match-before-android/`,
`Legacy/20261012-224000-225-room-match-windows/` and
`Legacy/20261012-224056-343-room-match-diagnostics/`.
Existing hosted-movement changes were retained. No staging, commit or push.

## Hosted sports movement — 12 October 2026

The host's own football/basketball character now samples and simulates movement
per rendered frame, matching Explore Offline. Previously the server-owned
character stepped at 50 Hz without host-side NetworkTransform interpolation.
At controlled 60 FPS the original player has 10 stationary movement frames per
second; at 120 FPS it has 70. The fix has zero in both sports and both chase and
stadium views at 30/60/120 FPS. Sprint pace stays 10.5 m/s in football and 7 m/s
in basketball. [Diagnosis, scope and reproduction](NETWORK-MOVEMENT.md).

The Windows player built at **18:05:21 Malaysia time** is delivered in
`Builds/WindowsFinal/`, with all **322 files** verified against the tested
candidate before updating its delivery note. **321 runtime/support files**
remain identical. Start `WhatTheFish.exe` to host a room, or the existing
Explore launchers for offline play. Protocol **45** is unchanged.

| Suite | Passing assertions | Evidence under Builds |
| --- | ---: | --- |
| Final frame cadence, both sports, offline/host | 56 | `NetworkMovementQA/Fixed-20261012-180526/` |
| Final football props, four rendered participants | 226 | `FootballPropsQA/Local-20261012-183226-340/` |
| Final basketball props, three rendered participants | 275 | `BasketballPropsQA/Local-20261012-183332-318/` |
| Final football audio, host/client | 65 | `FootballAudioQA/Local-20261012-183532-836/` |
| Football stamina, Dash, pressure and input expiry | 26 | `FootballPaceQA/Reviews/Local-20261012-174622-656/` |
| Basketball shots, passing, possession and reconnect | 41 | `BasketballGameplayQA/Local-20261012-174622/` |
| Football match, scoring, collisions and ball physics | 533 | `FootballMatchQA/Run-20261012-175157/` |

**622 assertions** pass on the final player, plus **600** on the initial fixed
player. Compiled-source comparison confirms only three development review probes
changed between those players; every gameplay source matches. No targeted
runtime diagnostics occur in **23 retained logs**. The final source proof covers
**213 compiled runtime documents**. Evidence and full delivery hashes are in
`Builds/NetworkMovementQA/`.

Early headless ability reviews failed shader/material assertions and were not
counted as visual passes. A loaded rendered run also failed a possession fixture;
the isolated run passed all possession and network-wall cases. Both original and
fixed players exposed the accelerated countdown test's screenshot-timing issue.
The final football review observes all five digits over real elapsed time;
basketball samples the receiving hand before screenshot readback can outlast
the portal. Both corrected rendered suites pass. Close countdown and portal
captures were inspected. These corrections change review probes only.

Frame-cadence measurements use Unity capture time, not an FPS benchmark.
Local multi-process network checks do not establish real WAN latency behavior.
Android size/signature and device limits are recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

Fresh AndroidSubmission measures **90,099,110 bytes (−300)**, with
**9,900,890 bytes** to the hard boundary. It remains **15,099,110 bytes** above
the development target. Budget and APK v2 signature checks pass. Existing
packed assets and all 1,788 mesh records match. No phone is connected. After
Android, 3,654/3,656 Game input hashes match the Windows snapshot; the two
generated-state differences are documented in [the audit](APK-SIZE-AUDIT.md#hosted-sports-movement--12-october-2026).

Reviewed superseded output and failed diagnostics are recoverable in:

- `Legacy/20261012-174217-734-sports-movement-baseline/`
- `Legacy/20261012-180046-302-sports-movement-initial/`
- `Legacy/20261012-183518-994-sports-movement-diagnostics/`
- `Legacy/20261012-183556-915-sports-movement-before-android/`
- `Legacy/20261012-183650-909-sports-movement-windows/`

## Original reference flags and decaying turf bounces - 13 October 2026

The prior flag redraw lost the supplied outline, rounded digits and fabric
highlights. `GolfFlagArtBuilder` now derives five 1024 x 384 front/back textures
from the retained originals, with alpha clipping, mipmaps and Android ASTC 6x6.
The silver pole/bindings remain native 3D, with corrected proportions and smooth
normals. Ten superseded numeral meshes and the unused old wind material were
archived after GUID and generator checks; the scene, prefab and rebuilding path
all reference the new artwork. See [flag authoring](../ArtSource/Golf/Flags/README.md).
Front appearance follows the reference pixels; game lighting, perspective,
texture filtering and animated folds mean a rendered frame is not pixel-identical
to the original illustration.

`GolfFlagWind.Wave` increases both displacement coefficients sixfold:
.024/.010 -> .144/.060 metres. The fixed hoist stays still; the squared horizontal
envelope reaches one at the free tip. Maximum displacement bound is .204 m,
theoretical peak-to-peak .408 m. Broader travelling folds replace sharp creases.
This is a procedural wind approximation, not a full cloth collision simulation.
The cloth mesh bounds cover the stronger displacement; alpha is respected by
colour, shadow and depth passes. Preview times return to -1 before saving.

The guided swing formerly projected away all incoming normal speed at touchdown.
`GolfShotPlan.Build` now uses `GolfBallPhysicsSettings.LandingVelocity` to reflect
it with .46 restitution, retaining the existing .78 first-landing tangent factor.
`GolfBall.OnCollisionEnter` handles later static turf impacts with .46 normal
restitution and .92 tangent retention, using pre-solver incoming velocity.
`GroundMotion` waits until a descending contact falls below the .55 m/s cutoff
before treating it as rolling support. This drains energy and avoids perpetual
micro-bounces. Putt plans, launch/charge rules, scoring, cups, authority, existing
snapshot fields and cameras are preserved. Bounces intentionally extend run-out
past the first touchdown guide target.

Final full Windows build: **22:42:45 Malaysia time** (14:42:45 UTC). The complete
321-file tested candidate is promoted to `Builds/WindowsFinal/`; every copied
file hash matched before updating the delivery note. Runtime protocol remains
47, including the existing concurrent golden shot trail. Verification:

- 83 saved-map assertions pass: cup positions/collision, five numbered flags,
  shared course and scene references.
- All five front/back views and oblique wind captures were generated. All front
  numerals, all rear numerals and oblique wind were visually inspected; rear repair removed
  the mirrored-digit seam/ghost. Wind snapshots differ by 225,714 pixels.
- 98 rendered ball-physics assertions pass, including 1 m/3 m drops, guided
  charges .08/.50/1.00, multiple decaying bounces, unchanged first touchdown,
  supported rolling/rest, slopes, gameplay forces, trail and all five cups.
  Recorded outgoing drop speeds: 1 m, 2.0511 -> .8614 m/s; 3 m,
  3.3635 -> 1.4313 -> .6049 m/s. Guided swing bounce counts: 2/3/6; the
  full shot also leaves the raised test platform and meets lower terrain.
- 158 rendered offline match assertions, 22 host and 25 rendered side-approach
  client assertions pass. New checks observe authoritative multiple rebounds
  and replicated descent-to-upward reversal without a reset/teleport.
- 142 rendered island assertions pass: the new two-renderer flag layout, cups,
  shoreline approaches, fairway, bunkers, camera and streamed environment switching.
- 121 playability assertions pass: four surface resistances, all five cups in
  four putting directions and three tee charge levels. Total: **649 assertions**,
  plus the flag image/wind audit. This is Windows/localhost evidence, not phone
  appearance/FPS or real WAN qualification.

Evidence: `Builds/GolfFlagBounceQA/`, `Builds/GolfBallPhysicsQA/Run-20261013-223930/`,
`Builds/GolfMiniGameQA/Run-20261013-223930/`, and
`Builds/GolfPlayabilityQA/Run-20261013-223930/`, and
`Builds/GolfG2QA-20261013-224412/`. The final rear-face polish only changes
texture derivation: final `Assembly-CSharp.dll` SHA-256 exactly matches the
22:38 player used for the repeated physics/match/playability suites; the final
delivery also passes a fresh 142-check rendered island run. The absent extra
file compared with the earlier 322-file candidate is an obsolete Burst
DoNotShip text diagnostic, not a runtime or launcher dependency.

AndroidSubmission was invoked three times, including the final delivery source, and failed during
Gradle/JDK initialization: `Unable to establish loopback connection`, caused by
`UnixDomainSockets.connect0: Invalid argument: connect`. A process-local test
that made the socket directory unavailable allowed JDK's TCP fallback and the
standalone Gradle help task succeeded, but the Unity build retry still failed.
No installed JDK or global Java settings were modified. Therefore new APK bytes
and Android visual quality remain **unverified**. The retained previous APK is
unchanged at **90,334,226 bytes**, SHA-256
`2382DFE7BEF6B1085664B5665D14F2F33B82AE4A5EA2376B9834AA6C19C82A06`:
9,665,774 bytes to the strict 100 MB boundary, 15,334,226 bytes above the 75 MB
development goal. It does not validate the new reference textures or rebounds.
Latest failed audit/logs are retained; older successful contributors remain
separately in `Builds/GolfFlagDesignQA/BaselineAudit/`.

Archives: `Legacy/20261013-221944-380-golf-reference-flags/` (22 obsolete art/meta
files), `Legacy/20261013-222802-010-golf-reference-bounce-windows/` (superseded
complete player and first trial),
`Legacy/20261013-224525-736-golf-reference-bounce-final/` (all superseded player
trials and the older promoted player), and `Legacy/20261013-223345-444-golf-reference-bounce-evidence/` (redundant baseline APK copy and first physics run). No Git stage, commit or push was performed.


## Teal and cream numbered wind flags — 13 October 2026

All five cup flags now follow the approved references: teal swallowtail cloth,
cream border and rounded cream numerals, a satin-silver ball-top pole and teal
bindings. Both faces carry the correct number. `GolfFlagArtBuilder` generates
native editable geometry and updates the streamed scene and reusable environment
prefab. `GolfCourseBuilder` uses it on regeneration. Original cup positions,
collision and scoring geometry remain. The five source PNG references are kept
under `ArtSource/Golf/Flags/References/`, outside the delivered Unity assets.

`GolfFlagWind` moves the cloth, trim and numeral vertices together. Its two
slow waves have a combined maximum tip displacement of **0.034 m**; the
pole-side edge has zero displacement and the pole/bindings remain stationary.
The cloth casts a waving shadow, and avoids receiving thin-surface self-shadow
artifacts to keep the numerals legible. Mesh bounds include the deformation.
There is no cloth simulation, per-frame CPU mesh upload or new raster texture.

Final editor evidence is in `Builds/GolfFlagDesignQA/`: ten front/back close
captures, two timed oblique wind views and `design-audit.txt`. The wind views
change **214,041 pixels**; all five front faces and a rear face were visually
inspected. The existing course audit passes **83 assertions**. Final rendered
player run `Builds/GolfMiniGameQA/Run-20261013-213326/` passes **158 assertions**,
including the live five-hole progression and existing procedural golfer pose.
The shader preview override is restored to **-1 (game clock)** before saving.

The cloth/numeral geometry welds duplicate vertices, uses 16-bit indices and
packed vertex colours. Raw mesh source text across pole, cloth and ten number
faces falls **1,165,912 → 381,447 bytes**. This is a source measurement, not an
APK-size measurement. The three shared materials and shared cloth/pole meshes
avoid five complete texture copies. Superseded previews are archived in
`Legacy/20261013-210925-268-golf-flag-first-preview/`,
`Legacy/20261013-211226-158-golf-flag-polish-preview/`,
`Legacy/20261013-212259-324-golf-flag-before-weld/` and
`Legacy/20261013-212910-655-golf-flag-pole-preview/`.

The complete Windows player built at **21:32 Malaysia time** was promoted with
**322 file hashes** matching the tested candidate. It includes the existing
golden shot-trail runtime and room protocol **47**. The older concurrent player
and first flag player are archived in
`Legacy/20261013-214349-182-golf-wind-flags/`. This is a working-source snapshot,
not a clean-commit build; later unrelated changes are outside its validation.

The opening on-disk APK was the stale **89,230,290-byte** October 5 package.
A concurrent shot-trail release produced **90,334,226 bytes** at 21:25 while
this task was in progress. That intervening growth cannot be attributed solely
to flag artwork. It and its audit were preserved in
`Legacy/20261013-213631-964-golf-flag-before-release/` before the final flag build.
Final-source AndroidSubmission attempts, including process-local IPv4/JVM
options, still failed with Gradle's `Unable to establish loopback connection`.
The valid **90,334,226-byte** prior APK was restored byte-for-byte; it is not
validation of the final optimized meshes. Its hard-limit headroom is
**9,665,774 bytes**, and its development-target overrun is **15,334,226 bytes**.
The final Android size and physical-phone appearance remain unverified.
`Builds/SizeAudit/latest/` holds fresh failure evidence; the previous release's
contributor CSVs are retained separately in `Builds/GolfFlagDesignQA/BaselineAudit/`.

## Golden Golf shot trail — 13 October 2026

**288 assertions pass** on the complete current Windows player: 85 rendered
ball/tee/physics/trail, 158 rendered Golf offline, 21 host and 24 side-approach
client. Evidence: `Builds/GolfBallPhysicsQA/Run-20261013-212235/` and
`Builds/GolfMiniGameQA/Run-20261013-212235/`. Current visual capture:
`Builds/GolfTrailQA/gold-shot-trail.png`. Full content build **21:17:42 Malaysia
time**, final gameplay/review scripts refresh **21:21:50**.

Real qualified shots emit the shared gold/ivory soft-edge ribbon along actual
ball movement. Head/tail colour, transparent taper, no shadows, a fresh second
shot, gradual stopped fade, complete one-second expiry and immediate teleport
cleanup pass. Stopped midpoint fade measures **0.5166**; after expiry the renderer
is disabled, and reset reports **zero points**. The actual rendered flight was
inspected. The client detects the host-authorized strike sequence and displays
the ribbon; score ownership, cancelled swings, stale rounds, independent cup
progress and authoritative DNF remain checked. The room protocol is **47**.

An initial stop fixture used a kinematic flag which the match manager's normal
`ApplyLive` step immediately restored to dynamic. The guarded review now freezes
that temporary body for the visual-only fade sample and restores its constraints
afterward. Reset verification also exposed immediate automatic trail sampling;
clearing now disables the empty renderer after placement. Initial reports are
retained in the reviewed task archives, and the complete final suites pass.

All **222 runtime sources** match the tested isolated snapshot. Current Golf
scene/environment graphs (**2,275 / 2,342 objects**) and the **14-object** Ball
prefab match primary despite regenerated local IDs. Current numbered flag
artwork and scenery-flag cleanup are retained. Reviewed build differences are
the isolated editor-only build wrapper, desktop pipeline registration and
generated IDs. The complete **322-file** delivery and exact fresh signed APK/audit
are retained in `Builds/GolfTrailQA/Release/`, with promotion hashes and source
comparisons beside them. `Builds/WindowsFinal/LATEST-BUILD.txt` records scope.

Fresh primary AndroidSubmission at **21:25:01** measures **90,334,226 bytes**;
budget gate, independent v2 signature and populated packing audit pass. The
one-second fade probe is excluded from release gameplay compilation. Physical
phone appearance, touch, FPS and WAN remain unverified. See
[size, provenance and archives](BUILD-SIZE.md#golden-golf-shot-trail--13-october-2026).
Git publishing was not requested for this feature.

## Remove obsolete Golf scenery flags — 13 October 2026

The two unnumbered teal flags in the original Golf scenery are removed from
`SkySail_Golf` and the `GolfEnvironment` prefab. The five numbered cup flags
remain. Old flag bases, timber poles and teal pennants shared `CourseProps`
material batches with the original tee stones. `GolfCourseBuilder` filters
only the two old flag footprints, updates the matching visual/collision meshes,
and retains the tee stones' positions, materials and attributes. This removes
**296 triangles** from each authored map representation; the surviving teal
mesh has **520 vertices**. The retained FBX master remains editable.
The Blender generator now omits those scenery pins, and course regeneration
applies the same cleanup to older source art.

`GolfDecorativeFlagReview` recorded unchanged references to all **five** hole
flags during removal and checked five flags in the prefab. The existing saved
course audit passes **83 assertions**, including hole flags, cups, terrain,
tees and surface continuity. Rendered evidence is preserved in
`Builds/GolfFlagRemovalQA/Map/`; the numbered fifth-hole flag was visually
inspected. Rendered player run `Builds/GolfMiniGameQA/Run-20261013-204245/`
passes **158 assertions**, including live five-hole progression.

The first Windows build stalled during resource import after saving the map
and rendered audit. Its batch process was stopped, and a new non-rendering
editor build completed successfully. Logs and source inventory remain under
`Builds/GolfFlagRemovalQA/`. Gameplay cameras, scoring and ball physics were
not edited.

The complete Windows candidate built at **20:41 Malaysia time** was promoted
to `Builds/WindowsFinal/`, with **322 copied files** matching SHA-256 before
the note update. The reviewed prior player is archived in
`Legacy/20261013-204439-499-golf-scenery-flags/`. The build uses the current
uncommitted workspace, including unrelated pre-existing changes.

AndroidSubmission was attempted and failed in Gradle initialization with
`Unable to establish loopback connection`. The actual on-disk APK before/after
remains the stale October 5 output, **89,230,290 bytes**, with **10,769,710 bytes**
to the hard boundary and **14,230,290 bytes** above the development target.
This is not an Android build of the flag removal. Fresh size and physical-phone
appearance are unverified. The latest size-audit log/report are fresh failure
evidence, while the APK contributor CSVs still describe the old build.
Repository readiness and whitespace checks pass. No Git publishing was done.

## Golf cart English button labels — 12 October 2026

`AppRoot` and `GolfCartButton` now display **Buggy**, **Recall**, **Drive** and
**Exit** for the four cart states. Desktop R/E hints remain. The controls use
the existing Cove display font because the old Chinese subset does not contain
the required Latin letters; no new font asset is added. `GolfCartProbe`'s
existing label and font assertions are updated to match the requested wording.

`Builds/GolfCartQA/Run-20261012-214905/` passes the four button-state checks,
English glyph coverage and recall label reset. The complete cart suite is
**not fully passing**: seated-toe direction checks fail at about 64 degrees
in offline and network poses. The previous WindowsFinal player also reproduces
the same failures in `Builds/GolfCartLabelsQA/baseline.txt`; these are pre-existing
pose issues, not a label regression. Both network runs reach completion.
The existing driving pose is outside this wording change's scope.

The suite records **163 offline passes / 8 toe failures**, **23 host passes /
2 toe failures**, and **23 client passes / 2 toe failures**. The complete Windows
candidate built at **21:48 Malaysia time** was promoted to `Builds/WindowsFinal/`;
**318 copied files** match SHA-256 before note updates. Prior player and trial
build are archived in `Legacy/20261012-215214-980-golf-cart-labels/`.
The build includes the existing uncommitted workspace snapshot.

AndroidSubmission was attempted and again failed with Gradle's loopback
connection error. The stale October 5 APK remains **89,230,290 bytes** before
and after, with **10,769,710 bytes** to the hard boundary; it exceeds the
development target by **14,230,290 bytes**. This change's Android size is
unverified. The latest size audit's failure log/report are fresh, while APK
contributor CSVs still describe the older APK. No new asset was added.

## Golf forward gaze during swing — 12 October 2026

The head previously inherited the complete 65-degree torso turn. In
`GolfClubMotion.LateUpdate`, its animated world rotation is now captured before
the procedural spine/chest adjustment and restored with only **8%** of chest
yaw and **15%** of address pitch. This keeps the forward gaze with at most
**5.2 degrees** of yaw and **2.7 degrees** of nod. Head position still follows
the neck naturally. The bone restore path retains ordinary carry/walk animation;
arm geometry, grip targets, swing timing and gameplay are unchanged.

Rendered run `Builds/GolfMiniGameQA/Run-20261012-212731/` passes **158 assertions**.
The new full-backswing and three-strength continuous head checks pass, with
maximum combined deviation **5.858 degrees**. The existing arm reach, grip,
contact and recovery checks also pass. The full-charge front screenshot was
inspected: torso/arms turn sideways while the face remains directed forward.
Build/test evidence is under `Builds/GolfHeadQA/`.

Network run `Builds/GolfMiniGameQA/Run-20261012-212809/` passes **21 host /
22 client** assertions. The complete Windows player built at **21:27 Malaysia
time** was promoted to `Builds/WindowsFinal/`, with **318 copied files** matching
SHA-256 before its note update. The previous player is archived in
`Legacy/20261012-212909-820-golf-head/`. This is the current uncommitted workspace
snapshot, including unrelated pre-existing changes.

AndroidSubmission again failed with Gradle's `Unable to establish loopback
connection`. APK before/after is the stale **89,230,290-byte** October 5 output:
**10,769,710 bytes** below the hard boundary and **14,230,290 bytes** above the
development target. Fresh APK size and phone appearance remain unverified.
The latest audit log/report show the failed attempt; APK contributor CSVs
still describe the older build, not this change.

## Golf extended backswing — 12 October 2026

The user's new recording shows both hands retracting to the chest while Swing
is held. The old `shoulder + (0.12, 0.08, 0.12)` backswing endpoint caused this:
full-charge wrist reach was about **57% / 30%** of left/right arm length,
with elbow flexion about **111 / 145 degrees**. Correct address placement and
palm orientation alone could not fix the separate charge endpoint.

`GolfClubMotion.SwingArcGrip` replaces that endpoint with an outward arc.
The shoulder turn follows the same signed phase as the hands. Each wrist's
measured arm length and rotated palm offset define a sphere for the shared
right-grip position; the left sphere also includes the actual grip-marker
separation. The closest point on their intersection circle fits both hands
to a common club pose. Reach blends continuously from the approved address
to **96%** at full backswing, retaining soft elbows. The modest upward arc
keeps the hands below the large cartoon head. Contact fitting happens before
the chest turn so it does not resize the club to compensate for a backswing.
The elbow pole now follows the current swing phase, including recovery.

The existing 0.10-second contact and 0.74-second swing duration remain. The
last 0.30 seconds now ease back to address before the busy state ends.
Grip markers, opposing palm normals, Aim activation and procedural two-bone
IK remain; ball physics, charge power, trajectories, scores and authority
were not changed by this revision.

`GolfMatchProbe` adds full-backswing reach/lateral-extension assertions,
front/side charge captures and exact-clock pose sweeps at **8%, 50%, 100%**
shot strength. The sweep evaluates the real procedural pose every 10 ms
without issuing a shot. Rendered evidence
`Builds/GolfMiniGameQA/Run-20261012-211558/` passes **154 assertions**.
Each sweep has grip error approximately **0.00001 m**, contact error rounded
to **0.00000 m**, and minimum arm reach **93%**. Maximum 10 ms movement
across both palms/elbows is **19.63 / 36.49 / 68.02 mm** for those strengths;
the last recovery samples stay below **12.72 mm** per step. These are fixed
animation-frame sweeps, supplemented by the existing live shot tests, not
phone frame-rate measurements.

At full backswing both wrist reaches are **96%** with about **33 degrees**
of elbow flexion; half-charge reaches are about **94% / 97%**. Front, side
and oblique captures show the hands extended to the golfer's right, below
the shoulders. Address, carry, charge, contact, follow-through and recovery
checks pass. Evidence and build logs are retained under
`Builds/GolfBackswingQA/`; extracted user-video frames are in its `Video/`.

Final network evidence `Builds/GolfMiniGameQA/Run-20261012-211639/` passes
**21 host / 22 client** assertions. The complete Windows player built at
**21:15 Malaysia time** was promoted to `Builds/WindowsFinal/`; all **318**
copied files matched SHA-256 before the delivery-note update. The old running
player was closed under the user's prior authorization. Its complete directory,
two superseded trial players and the temporary decoder dependency are archived
in `Legacy/20261012-211800-416-golf-backswing/`. The delivery includes the existing
uncommitted workspace snapshot and unrelated pre-existing changes.

AndroidSubmission was retried on the final source and again failed with
Gradle's `Unable to establish loopback connection`. APK before/after remains
**89,230,290 bytes** (old October 5 output), leaving **10,769,710 bytes** to
the strict limit and exceeding the development target by **14,230,290 bytes**.
No fresh Android size or phone appearance is validated. The latest size-audit
failure log/report are current, while the APK contributor CSV is stale;
its largest compressed entries remain IL2CPP **11,193,548** and Unity
**9,066,688 bytes**, not measurements of the new source.

## Golf opposing palm orientation — 12 October 2026

The user approved the arm placement and requested only palm roll: right palm
faces the golfer's left, and left palm faces the golfer's right. The previous
two-hand pose used the club's local X axis, which points up/down in address.
`GolfClubMotion` now uses opposing club-face normals (local +Z / -Z), including
the wrist-offset calculation in `ContactGrip`. The same club-relative normals
follow the backswing and follow-through. Grip markers, arm preference, elbow
poles, torso lean, carry orientation, Aim behavior and gameplay are unchanged.
The existing fit makes a small adjustment for the rotated palm-to-wrist offset.

Rendered offline evidence `Builds/GolfMiniGameQA/Run-20261012-205330/` passes
**146 assertions**, including the new palm-direction regression. Right palm
dot golfer-left is **0.9994**; left palm dot golfer-right is **1.0000**.
Address and contact grip/head-target errors round to **0.0000 m**. Address
left/right wrist reach is **92.7% / 98.0%**, and hand height above hips is
**0.145 / 0.106 m**. The original forward-arm and settled-pose checks pass;
maximum settled palm movement is **2.03 mm**. Front, side and oblique address
captures were inspected. Carry, charge 0/50/100%, contact, follow-through,
recovery and Aim exit checks also pass. Network run
`Builds/GolfMiniGameQA/Run-20261012-205403/` passes **21 host / 22 client**
assertions; this is same-machine network validation, not separate-device QA.

The full tested Windows player was promoted to `Builds/WindowsFinal/` and
**318 copied files** matched SHA-256. The previous player is archived in
`Legacy/20261012-205457-848-golf-palms/`. The candidate was built at 00:35
Malaysia time and validated/promoted at 20:53–20:55; its source is the existing
uncommitted workspace snapshot, including unrelated pre-existing changes.

AndroidSubmission was attempted again and failed in Gradle initialization with
`java.io.IOException: Unable to establish loopback connection`. Before/after
APK size remains **89,230,290 bytes**, leaving **10,769,710 bytes** to the hard
boundary and exceeding the development target by **14,230,290 bytes**. This
October 5 APK is stale; the palm fix has no validated Android build or phone
preview. `Builds/SizeAudit/latest/` contains fresh failure logs/report, but its
APK CSVs remain stale (largest entries: IL2CPP 11,193,548 bytes and Unity
9,066,688 bytes compressed); they cannot establish this revision's size delta.
`Check-TaskReady.ps1` and `git diff --check` pass. No Git publishing was done.

## Golf drawn arm and club reference — 12 October 2026

This revision supersedes the preceding address fit. The user's annotated side
view requires the arms to extend ahead of the torso and the club to stand more
upright. The prior waist-relative preference kept hands too close to the body;
independent left-marker sliding also collapsed the hand separation. Reach and
finger-axis checks alone did not establish the requested silhouette.

`GolfClubMotion.ContactGrip` now starts from the measured shoulder midpoint,
average arm length and a forward/downward hand preference. Continuous local
refinement evaluates both wrist targets using the same palm normals and offsets
as runtime IK, penalizes unreachable targets and hands falling back toward the
torso, and retains the authored relative grip-marker positions. The full visual
club is uniformly fitted to the head-to-grip distance, instead of moving hands
down the shaft. At address its scale is approximately 69% of authored size;
the carry scale and marker positions are restored outside the two-hand pose.
The original arm lengths, procedural IK, shot physics and authority are retained.
The torso leans 18 degrees. Existing Aim, charge and swing states continue to
activate the shared local/remote pose.

The graphics-enabled run `Builds/GolfMiniGameQA/Run-20261012-002114/` passes
**145 assertions**. Added regression checks cover hand position ahead of the
shoulders, low hands, shaft inclination, distinct grip positions and settled
pose stability. Address palms are **0.219 m ahead of the shoulder midpoint**;
shaft inclination is **26.63 degrees from vertical**; maximum settled palm
movement per sampled frame is **1.49 mm**. Left/right wrist reaches are
**93.7% / 98.0%**, hand heights above the hip joint **0.158 / 0.117 m**,
and reported grip and head-target errors round to **0.0000 m**. Contact wrist
reaches are **98.0% / 96.5%**, with the same rounded-zero errors. Captures include
front, side, oblique, charge, contact, follow-through and carry views.
These measurements establish the revised silhouette and attachment, not literal
collinearity of both arms with one shaft or physical-phone performance.

Network evidence `Builds/GolfMiniGameQA/Run-20261012-002158/` passes **21 host /
22 client assertions**. With explicit user authorization, the old Windows
player was closed and the complete tested candidate promoted to
`Builds/WindowsFinal/`; **318 copied files** were independently hash compared.
The superseded player and temporary source backup are recoverable in
`Legacy/20261012-002315-447-golf-red-frame/`. The build includes the existing
uncommitted workspace changes; it is not a clean-commit build.

AndroidSubmission was attempted for this revision but Gradle again failed with
`Unable to establish loopback connection`. The APK before and after the attempt
remains **89,230,290 bytes**, with **10,769,710 bytes** to the hard boundary;
this is a stale baseline, **not validation of the revised code**. A fresh APK
size and phone appearance remain unverified. The failed build evidence is under
`Builds/SizeAudit/latest/` and `Builds/GolfRedFrameQA/`.

## Golf address pose and Aim stance — 11 October 2026

`GolfClubMotion` derives the contact grip from measured arm geometry,
waist/shoulder positions and the club's head-to-`rightGrip` distance. It uses
the physical head-to-handle axis for both hand rotations, so fingers follow the
visible shaft rather than an authored marker rotation. During the two-hand pose,
the real grip markers move down the shaft by the measured club-length/arm-span
difference, capped at 25% of club length. The left wrist then receives the
smallest additional along-shaft shift needed to reach 92% of its actual
two-bone length. This corrects the asymmetric left palm offset without moving
the club root, club head or right-hand target. One-handed carry restores the
authored markers unchanged.

Aim drives the same two-hand address for local and replicated players before
charging. The address stance is taken at 64% of the sanctioned golf reach,
which places the compact golfer over the lower handle without changing swing,
ball, score or authority code. The address elbow pole keeps the residual bend
behind the torso, so the forearms project down the shaft rather than flaring
away from it. `GolfMatchProbe` records both grip errors, club-head target
distance, reachability, wrist reach fractions, finger/forearm-to-shaft angles,
elbow angles and hand height.

The latest graphics-enabled offline run passed all **141 checks**, including
carry, Aim/address, 0/50/99% charge, contact, follow-through, recovery and
carry grip-skin checks. Both grip errors and club-head target error were
**0 mm**. At address, both wrists were reachable at **92% left / 98% right**
of their actual arm lengths; finger axes were **0 degrees** from the shaft;
forearm-to-shaft angles were **22 degrees left / 28 degrees right**; and hands
were **0.10–0.11 m** above the hip joint. At contact, reach was **92% / 95%**
with zero grip and head-target error. Front, side and gameplay-camera captures
are in `Builds/GolfMiniGameQA/Run-20261011-235555/`.

The final host/client suite passed **21 host and 20 client assertions**,
including replicated Aim address before charge, local address, both-hand charge
and remote shot authority. Evidence: `Builds/GolfMiniGameQA/Run-20261011-235628/`.

A complete Windows candidate with the updated grip markers was built at
`Builds/GolfAddressPoseQA/Player/`. It could not be promoted to
`Builds/WindowsFinal/` because the previous
`WhatTheFish.exe` was still running and Windows denied moving the locked player
directory. The running process was left untouched. The AndroidSubmission build
was retried after the final pose change and again reached Gradle but failed with
`java.io.IOException: Unable to establish loopback connection`. The APK remains
the pre-task **89,230,290-byte** artifact; its **10,769,710-byte** hard-limit
headroom is a baseline measurement only and does not verify this change. No
fresh Android size or signature result is claimed.

## Combined Football, Basketball and Fishing release — 12 October 2026

The newest pulled commit is `eaad195` (Football props, UI and audio). All incoming
networking/multiplayer logic is retained. Only independent Fishing timestamped
Hook edges extend its networking file; combined **protocol 46** distinguishes
the new Fishing recap wire schema from both previous builds. Matching builds
are required. Four islands, one activity per room and shared travel remain.

Full Windows content build finished **18:00:21 Malaysia time**
(`2026-10-12T10:00:21.3354160Z`). Successful scripts refreshes for guarded
countdown/menu reviews retain every scene, asset and runtime file; the final
refresh finished **18:29:53** (`2026-10-12T10:29:53.2206829Z`). The complete
candidate was promoted to `Builds/WindowsFinal/`: **321 runtime/support files
plus LATEST-BUILD.txt, 322 total**. All 321 hashes match the tested source player.
Only `boot.config`, `Assembly-CSharp.dll` and `Assembly-CSharp.pdb` differ from
the full content build. Launch `Explore-Fishing.cmd` or `Explore-Football.cmd`.

Selected relevant reports contain **4,152 counted PASS assertions across
22 reports**, zero FAIL lines and successful completion:

| Suite | Passing assertions | Current evidence under Builds |
| --- | ---: | --- |
| Fishing gameplay | 1,099 | `FishingGameplayQA/Run-20261012-180140/offline.txt`; `FishingGameplayQA/Run-20261012-180140/host.txt`; `FishingGameplayQA/Run-20261012-180140/client.txt` |
| Fishing feedback/results | 146 | `FishingPolishQA/Run-20261012-180334/polish.txt` |
| Football UI | 756 | `FootballUIQA/Run-20261012-180348-401/results.txt` |
| Basketball UI | 434 | `BasketballUIQA/Run-20261012-180411-152/results.txt` |
| Football audio offline | 49 | `FootballAudioQA/Offline-20261012-180434-938/offline/results.txt` |
| Football audio local | 65 | `FootballAudioQA/Local-20261012-180523-653/host/results.txt`; `FootballAudioQA/Local-20261012-180523-653/guest/results.txt` |
| Football Explore abilities | 162 | `FootballPropsQA/Explore-20261012-180630-021/offline/results.txt` |
| Football four-player abilities | 225 | `FootballPropsQA/Local-20261012-181824-344/host/results.txt`; `FootballPropsQA/Local-20261012-181824-344/guest/results.txt`; `FootballPropsQA/Local-20261012-181824-344/teammate/results.txt`; `FootballPropsQA/Local-20261012-181824-344/rival/results.txt` |
| Basketball audio local | 55 | `BasketballAudioQA/Local-20261012-182015-061/host/results.txt`; `BasketballAudioQA/Local-20261012-182015-061/guest/results.txt` |
| Football match | 291 | `FootballMatchQA/Run-20261012-182054/offline.txt`; `FootballMatchQA/Run-20261012-182054/host.txt`; `FootballMatchQA/Run-20261012-182054/client.txt` |
| Golf HUD | 56 | `GolfHUDQA/Run-20261012-182127/golf-hud.txt` |
| Storybook menus | 699 | `StorybookMenuQA/Run-20261012-182953/offline/results.txt` |
| Stadium camera | 115 | `BroadcastCameraQA/Reviews/1280x720-20261012-182412-282/results.txt` |


Counts come from `PASS ` report lines. Fishing's guest report has 34 such lines
although its terminal counter says 32; the two recap assertions are present.
Actual local Fishing/audio tests use two processes; Football ability ownership,
Wall collision, Portal transfer, scoring and cleanup use four actual processes.
The four-player Football pass uses a **D3D11 host at 640×360 and three headless
peers**. Shader/material checks run on the rendered host; peer visual quality
is not established. The solo Explore pass separately renders at 1600×900 and
checks natural five-second countdown timing and all ability models.

The native Football/Basketball UI reviews verify their current illustrated
controls and touch ownership. The older generic SportsHUD review predates
these skins and is excluded from current qualification. A menu review now
accepts the native press-feedback component as well as Cove feedback, retaining
the requirement on every active button. The countdown review samples each
deliberate digit at a fresh relative deadline after rendering; natural timing
is tested separately. The camera fixture clears the legitimate stranded-ball
inbound state before testing ordinary movement. All these changes are guarded
by `UNITY_EDITOR || DEVELOPMENT_BUILD`; shipping rules are unchanged.

Earlier headless/rendered countdown and stale menu failures remain diagnostic
evidence and are excluded. No failed/incomplete run is counted as a pass.
Reports span the full build and later development-review refreshes: this is
**not a claim that every report ran against the exact final DLL**. Shipping
source and full serialized content are unchanged between these review refreshes.
Both current native settings captures were inspected for clear text and contained
controls. Automated audio checks verify event/playback state, not human listening.

The Windows source manifest covers **3,668 Game inputs** across Assets,
Packages and ProjectSettings, including binary assets and metadata. Editable
masters and Tools outside Game are not covered by that manifest. Post-Android
source differences are recorded separately; regenerated Golf prefab IDs are
restored only after all **14 complete object graphs and external references**
match the exact incoming indexed prefab. No player bytes are changed by that
source-only restoration. Unity's platform-resource registration is generated
metadata, not a gameplay source change.

Fresh AndroidSubmission written **18:42:22 Malaysia time** measures
**90,139,324 bytes**, with **9,860,676 bytes** to the strict
100,000,000-byte boundary and **15,139,324 bytes** over the 75 MB
development target. Budget and independent APK v2 signature checks pass.
[Size and SHA-256](BUILD-SIZE.md#combined-football-basketball-and-fishing-release--12-october-2026),
[packing comparison](APK-SIZE-AUDIT.md#combined-football-basketball-and-fishing-release--12-october-2026). All 13 checked executable
review types are absent from the release metadata.

Current manifests, proof of retained incoming networking, 22-report hashes,
failed diagnostics and APK comparison are under
`Builds/Publish/20261011-225324-fishing-merge/`. Earlier protocol44 candidate,
Fishing-only APK/audit and complete Windows player are recoverable in
`Legacy/20261012-175411-814-combined44-player-superseded/`,
`Legacy/20261012-131303-811-fishing-basketball-before-android/` and
`Legacy/20261012-183239-088-latest-sports-fishing-windows-delivery/`.
The superseded review draft is in
`Legacy/20261012-184043-253-superseded-combined44-review-draft/`.
Reviewed archives were losslessly compressed with every manifest hash verified.
No file content was deleted or Git history rewritten.

New Fishing celebrations and physical-phone testing remain deferred. Windows
captures do not establish phone touch, sound/vibration, graphics, FPS/heat or
WAN latency. The 75 MB development goal remains unmet.

## Approved football audio — 12 October 2026

All preview sounds are approved and integrated: **80 clips / 54 runtime action
families**, including 25 shared basketball clips. Kicks, contacts, movement,
abilities, match transitions and UI feedback follow resolved gameplay or the
presented pose/state. The Sound page exposes the four shared volume preferences.
Network protocol **45** requires matching builds.

The complete Windows player built at **17:01:39 Malaysia time** is delivered in
`Builds/WindowsFinal/`. **322 files** match the tested candidate before updating
`LATEST-BUILD.txt`; **321 runtime/support files** remain identical. Launch
`Builds/WindowsFinal/Explore-Football.cmd`. The existing solid Wall and enlarged
spawn displays are included.

| Suite | Passing assertions | Evidence under Builds |
| --- | ---: | --- |
| Football audio, actual offline actions | 49 | `FootballAudioQA/Offline-20261012-170153-895/` |
| Host/client audio events and presentation | 65 | `FootballAudioQA/Local-20261012-170153-913/` |
| Explore football props, solid Wall and spawn displays | 162 | `FootballPropsQA/Explore-20261012-170351-716/` |
| Shared basketball audio regression | 35 | `BasketballAudioQA/Offline-20261012-170351-715/` |
| Football UI layout and controls, initial audio build | 756 | `FootballUIQA/Run-20261012-165705-468/` |

**311 assertions** pass on the delivered player. The **756 UI assertions** use
the initial audio build before contact/presentation refinements; final UI layout
sources are unchanged. The final Sound page was separately rendered and visually
checked. No targeted errors occur in six retained runtime logs. The client
receives **21 unique live event IDs** with no duplicates or stale drops.
Tests include physical frame/net/turf contacts, whole-ball scoring, stationary
silence, held-button deduplication, Portal phases, Turbo muting/expiry, Wall
impact/expiry, continuous overtime, final whistle and win/loss/draw results.

An initial frame-contact test exposed pitch containment preceding the PhysX
callback. The correction checks actual post/crossbar geometry and uses the same
repeat gate; it changes audio classification only. The failed diagnostic run is
archived and the corrected offline/network cases pass.

All **80 WAV hashes** regenerate offline from a path with spaces and unrelated
working directory. **212 compiled runtime source documents** match the current
source. After Android, **3,650/3,652 Game inputs** match; generated Golf prefab IDs
and URP resource registration differ. Complete delivery and source hashes are
retained under `Builds/FootballAudioQA/`.

Fresh AndroidSubmission measures **90,099,410 bytes (+240,376)** with
**9,900,590 bytes** to the hard boundary. It remains **15,099,410 bytes** above
the 75 MB development target. Budget and APK v2 signature checks pass. New clips
add 190,152 serialized bytes; shared clips are not duplicated and every existing
packed asset size and all 1,788 mesh records match. See [size](BUILD-SIZE.md),
[audit](APK-SIZE-AUDIT.md) and [implementation/archive list](FOOTBALL-AUDIO.md).
No connected phone; device listening, touch, visuals/FPS and WAN latency remain
unverified. Automated audio checks confirm triggering/playback state, not human
listening quality. No Git staging, commit or push was performed.

## Physical football wall and larger spawn displays — 12 October 2026

The 15-second wall now has a solid world-layer box collider on every peer.
Both teams, including the placer, collide with it. Immediate placement, expiry,
reset and travel cleanup run independently of the rendered model. Character
movement retains its swept collision; ball motion retains continuous wall
sweeps before goal sampling. Placement checks the full player capsule, including
airborne players, before creating collision.

Countdown digits and pickup displays are four times their original size, with
matching larger floor projections. The countdown uses an opaque, depth-writing
dark-blue material. Enlarged pickups sit fully above the turf; pickup distance,
HUD icons, deployed wall dimensions and other ability effects are unchanged.

The complete Windows player built at **16:20:48 Malaysia time** is delivered in
`Builds/WindowsFinal/`. **321 files** match the tested player before updating
`LATEST-BUILD.txt`; the **320 runtime/support files** remain identical. Protocol
remains **44**. Relaunch `Builds/WindowsFinal/Explore-Football.cmd` for solo play.

**1,660 assertions pass on this player**:

| Suite | Assertions | Evidence under Builds |
| --- | ---: | --- |
| Actual Explore Offline, spawn displays and all abilities | 162 | `FootballPropsQA/Explore-20261012-162159-053/` |
| Four-player abilities, wall collision and owner RPCs | 225 | `FootballPropsQA/Local-20261012-162251-688/` |
| Football touch controls and multi-viewport UI | 740 | `FootballUIQA/Run-20261012-162251-695/` |
| Match, scoring, football physics and network regressions | 533 | `FootballMatchQA/Run-20261012-162428/` |

Collision cases cover walking, dash, tackle, jumping, a 250 ms simulation hitch,
reverse approach, rotated walls, walking around the ends, actual 15-second expiry
and immediate reset. A guest's real network dash stops at its own wall, and the
opposing host player also stops. Ball tests cover dribbling, low kicks, lobs,
side passes, rotated rebounds and a 2,000 m/s same-step wall/goal crossing.
Natural Explore countdown, non-repeating pickups, fixed halfway anchors,
unchanged collection distance and four-times display scale also pass.

Rendered countdown, three enlarged pickup designs, deployed wall and ability
guide captures were inspected. The diagnostic close countdown camera sees the
back of the digit; it does not drive the player-facing billboard orientation.
The default centre-pitch stadium captures have the sideline beacon outside the
viewport and are not evidence of full-pitch camera coverage.

`Builds/FootballPhysicalWallQA/` records assertions, source hashes, full delivery
hashes and release comparisons. All **208 compiled runtime source documents**
match. Only one small shared opaque material was added; geometry and textures
were reused. Fresh AndroidSubmission measures **89,859,034 bytes (+7,526)**,
with **10,140,966 bytes** to the hard boundary; the 75 MB development target is
still exceeded by **14,859,034 bytes**. APK v2 signature verification passes.
After Android, **3,526/3,529 Game inputs** match: generated Golf prefab IDs,
URP resource registration and the countdown material's unused legacy colour
alias differ. The rendered base colour, shader and remaining material properties
match; reconstruction reproduces the exact Windows material source hash.
See [release size](BUILD-SIZE.md) and [packing audit](APK-SIZE-AUDIT.md).
Historical delivery records below describe their earlier source snapshots.

Superseded output is recoverable in
`Legacy/20261012-162621-528-football-solid-before-android/` and
`Legacy/20261012-162731-407-football-solid-windows/`. No Android device was
connected. Physical phone touch, visuals/FPS and real WAN latency remain
unverified. Git staging, commits and publishing remain with the user.

## Football props in Explore Offline — 12 October 2026

The initial ability implementation only ran in formal matches, so ordinary
Explore Offline showed no countdown or pickup. Solo Explore now enables the
same ability simulation automatically while the match stays Idle. The local
player uses Team A for ability rules without changing the match roster or
turning on scoring. Online practice continues to wait for confirmed teams.
The five-second hologram, two fixed touchline locations, different consecutive
kinds, pickup replacement and all three ability rules are unchanged.

The complete Windows player built at **15:53:33 Malaysia time** was promoted to
`Builds/WindowsFinal/`. All **321 files** matched the tested player by SHA-256
before updating the release note; **320 runtime/support files** remain identical.
The delivery still uses protocol **44**. Relaunch `Explore-Football.cmd` from
that directory to use the updated offline flow.

**1,061 assertions pass on this player**:

| Suite | Assertions | Evidence under Builds |
| --- | ---: | --- |
| Actual Explore Offline entry, countdown, collection and abilities | 133 | `FootballPropsQA/Explore-20261012-155355-333/` |
| Four-player formal match abilities and cleanup | 188 | `FootballPropsQA/Local-20261012-155452-084/` |
| Football touch controls and multi-viewport UI | 740 | `FootballUIQA/Run-20261012-155502-355/` |

The Explore test never starts a match while checking all three abilities. It
observes the actual initial five-second countdown without advancing its clock,
collects the natural first pickup, activates it through the HUD, checks the
remaining spawn alternatives and exercises Turbo, Portal and Wall in practice.
It then checks transition into team selection, rejection of old practice
requests, clean resumption of practice and exit during a pending portal.
Rendered first-pickup and practice wall-preview captures were inspected. The
immediate `explore-held-prop` diagnostic capture occurs before the HUD's next
LateUpdate; use `wall-preview.png` for the rendered ability button in practice.

Source and delivery evidence is under `Builds/FootballExplorePropsQA/`:
`verification.json`, `windows-delivery.json`, `compiled-source.json`,
`windows-source.json` and `changed-game-inputs.json`. All **208 compiled runtime
source documents** match. Only the ability eligibility/team fallback, one guide
sentence and the development-only review changed in runtime C#; no new asset
or package was added. Earlier formal-match-only release notes below are historical.

Fresh AndroidSubmission is **89,851,508 bytes (+856)**, with **10,148,492 bytes**
of hard-boundary headroom. All packed asset sizes match the baseline. The
75 MB development target remains exceeded by **14,851,508 bytes**. APK v2
signature and budget gate pass; `Check-TaskReady.ps1 -RequireApk` passes.
[Size audit](APK-SIZE-AUDIT.md#football-explore-offline-props--12-october-2026).
After Android, 3,525/3,527 Game inputs match; only generated Golf prefab IDs and
URP resource registration differ from Windows. Source and football art match.

Prior APK/audit archive:
`Legacy/20261012-155613-395-football-explore-before-android/`.
Prior complete Windows player archive:
`Legacy/20261012-155653-400-football-explore-windows/`.
Existing unrelated changes are preserved. Git staging, commit and push remain
with the user. No connected Android phone or real WAN test was available.

## Football abilities — 12 October 2026

Implemented the approved [football abilities](FOOTBALL-ABILITIES.md): 1.5× team
Turbo for 5 seconds, own-half Portal Steal to the feet, and a 15-second placed
wall. Added distinct floating football models, illustrated HUD buttons,
placement preview/confirmation/cancel, team timers and an Abilities guide.
Two fixed halfway touchline locations use random sides, different consecutive
kinds and blue translucent 3D countdowns. Practice has no pickups.

The complete Windows player built at **15:25:11 Malaysia time** is now in
`Builds/WindowsFinal/`. All **321 files** matched the tested
`Builds/FootballPropsQA/Player/` by SHA-256 before replacing `LATEST-BUILD.txt`
with the delivery record; the **320 runtime/support files** remain identical.
The previous delivery was compared against the task-start record before
replacement. Use matching **protocol 44** players for every room participant.

**1,762 assertions pass on this exact player**, with no test-suite runtime
exceptions:

| Suite | Assertions | Evidence under Builds |
| --- | ---: | --- |
| Four-player football abilities | 188 | `FootballPropsQA/Local-20261012-152539-474/` |
| Football UI and touch gestures | 740 | `FootballUIQA/Run-20261012-152639-309/` |
| Football match, goals, ball physics and network ball | 533 | `FootballMatchQA/Run-20261012-152649/` |
| Network dash, pressure and stamina | 26 | `FootballPaceQA/Reviews/Local-20261012-152722-255/` |
| Rendered three-player basketball abilities | 275 | `BasketballPropsQA/Local-20261012-153312-161/` |

Football checks cover every countdown digit, both spawn alternatives, replacement
and stale requests, actual 1.5× walking/sprinting/dashing/dribbling/charging,
five-second Turbo expiry, team scope, invalid and simultaneous portals,
interrupted transfers, one controller, wall preview cancellation, bounds and
overlap rejection, high-speed/rotated/side/top/dribble contacts, ball reach through
walls, player passage, actual 15-second expiry and contact timing at expiry.
Wall contact precedes same-step goals; an already crossed goal wins over a
portal. Goals clear inventory/effects, restart countdowns and invalidate old
round requests. Offline implementation runs passed 121 and then 125 assertions
before the final goal-specific additions; those are retained as development
evidence and are not included in the final 1,762 count.

Inspected actual rendered captures for all three models, the hologram, blue/black
portal, wall preview, full-height deployed wall, HUD and illustrated guide.
The final basketball run also verifies the existing two-second lock-on, Turbo,
portal, rendered materials and countdowns. The first basketball run used
`-NoCapture`, which cannot satisfy its existing shader-support assertions. A
rendered run concurrent with Android compilation missed one sampled countdown
digit during capture. Rerunning the unchanged player with rendering after the
build passed all 275 assertions; both earlier logs remain in the task evidence.

All **208 compiled runtime source documents** match their PDB hashes. After
Android, **3,525 of 3,527 Game inputs** match the tested Windows snapshot; only
generated Golf prefab IDs and URP resource registration differ. Runtime code,
football meshes/materials/prefabs and icon textures match. The icon generator
also reproduces its exact output from a relocated path containing spaces with
an unrelated working directory. New art uses code-native geometry and SVGs,
with no external binary model dependencies.

Fresh signed AndroidSubmission: **89,699,656 → 89,850,652 bytes (+150,996)**;
strict hard-boundary headroom **10,149,348 bytes**. APK budget gate, APK v2
signature and `Check-TaskReady.ps1 -RequireApk` pass. The 75 MB development
target remains exceeded by **14,850,652 bytes**.
[Packing audit and hash](APK-SIZE-AUDIT.md#football-abilities--12-october-2026).
Evidence index: `Builds/FootballPropsQA/verification.json`, `windows-delivery.json`,
`compiled-source.json`, `source-comparison.json`, `portability.json`,
`apk-signature.txt` and `task-ready.txt`.

Archives: prior APK/audit in
`Legacy/20261012-152732-503-football-abilities-before-android/`; previous complete
Windows player and completed relocation fixture in
`Legacy/20261012-153553-759-football-abilities-windows/`. No active game asset
was removed. Existing unrelated workspace changes were preserved; no staging,
commit or push was performed.

No Android device is connected. Physical touch, phone appearance/FPS and real
WAN latency remain unverified; local rendered multiplayer runs do not establish
those results.

## 2D sports action icons — 12 October 2026

Delivered the complete Windows player at **Builds/WindowsFinal/**, compiled
**14:43:22 Malaysia time** (2026-10-12T06:43:22Z). All **320 runtime/support
files** match the tested candidate by SHA-256, excluding the release note.
The executable, Data directory, DLLs, support folders and launchers stay
together. Use `Explore-Football.cmd` or `Explore-Basketball.cmd`.

Tackle shows a simple 2D cartoon player sliding along the ground; Dash shows
a forward sprint with golden speed streaks. Shared Jump uses a matching
leaping figure with upward chevrons and a turquoise/orange lift trail in
both sports. The wider Tackle sprite excludes empty transparent rows, and
Jump sits above its label with clear spacing. Fonts, button positions,
input behavior, timing and gameplay rules remain unchanged.

**1,162 assertions pass across two fresh final-player UI reports**:

- Football: **728**, `Builds/FootballUIQA/Run-20261012-144334-266/`.
- Basketball: **434**, `Builds/BasketballUIQA/Run-20261012-144334-294/`.

Unity touch-module sequences exercise independent fingers, drag/cancel/release,
suspension, settings, role changes and far-finish timing. Font-family, glyph,
text-fit and slider checks pass. Captures cover 1600 × 900, 1280 × 720,
1560 × 720 and 1600 × 1200. Final football and basketball defense captures
were inspected for pose clarity and label separation. These Windows previews
do not establish physical phone appearance, touch latency or frame rate.

Fresh AndroidSubmission: **89,701,824 → 89,699,656 bytes (−2,168)**,
**10,300,344 bytes** below the hard boundary and **14,699,656 bytes** over the
development target. Budget enforcement and independent APK v2 verification
pass. [Packing audit](APK-SIZE-AUDIT.md#2d-sports-action-icons--12-october-2026).
ADB reports no connected device. Network protocol remains **43**; WAN
qualification and physical phone testing remain unverified.

Evidence: `Builds/FootballPoseIconQA/verification.json`, `windows-delivery.json`,
`compiled-source.json`, `source-comparison.json`, `game-input-changes.json`,
`portability.json` and `art-validation.json`. All 202 runtime documents match
the compiled player; the two generated asset differences are documented above.

Reviewed archive batches:

- Replaced shoe masters and old review player:
  `Legacy/20261012-142456-168-football-pose-icons-sources/`.
- Superseded 3D draft art, prompts and validation:
  `Legacy/20261012-143243-928-football-pose-icons-3d-draft/`.
- Prior APK/audit: `Legacy/20261012-143459-868-football-flat-poses-before-android/`.
- Prior full-colour player: `Legacy/20261012-143532-297-football-flat-poses-windows/`.
- Jump shoe master and intermediate validation/builds:
  `Legacy/20261012-144031-237-sports-flat-poses-before-jump/`.
- Final superseded Windows player:
  `Legacy/20261012-144511-651-sports-flat-poses-windows/`.
- Temporary relocation trees:
  `Legacy/20261012-142831-692-football-pose-icons-portability/`,
  `Legacy/20261012-143349-024-football-flat-poses-portability/`,
  `Legacy/20261012-144140-276-sports-flat-poses-portability/`.

Active masters, prompts and deterministic atlas inputs remain publishable.
No staging, commit or push was performed.

## Full-colour sports artwork — 12 October 2026

Delivered the complete Windows player at **Builds/WindowsFinal/**, compiled
**01:54:28 Malaysia time** (2026-10-11T17:54:28Z). All **320 runtime/support
files** match the tested candidate by SHA-256, excluding the release note.
The executable, Data directory, DLLs, support folders and launchers stay
together. Launch with `Explore-Football.cmd` or `Explore-Basketball.cmd`.

Both sports now show full-colour cartoon artwork with consistent shading,
highlights and clear silhouettes. Utility and prop icons gain colour and
depth. Unavailable controls preserve the illustration's colour with a softer
alpha fade. Manrope typography, button placements, connected drag paths,
release-only cancellation and gameplay behavior remain unchanged.

**1,162 assertions pass across two fresh final-player UI reports**:

- Football: **728**, `Builds/FootballUIQA/Run-20261012-015453-298/`.
- Basketball: **434**, `Builds/BasketballUIQA/Run-20261012-015453-298/`.

Actual Unity touch-module sequences cover independent fingers, drag/cancel/
release, suspension, settings, role changes and far-finish timing. Font-family,
glyph coverage, text fitting, slider travel and round thumb checks pass.
Captures at 1600 × 900, 1280 × 720, 1560 × 720 and 1600 × 1200 cover gameplay,
settings and guides. Inspected active and unavailable controls, the new gear,
camera cards and typography in the actual Windows player.

Fresh AndroidSubmission: **89,644,388 → 89,701,824 bytes (+57,436)**,
**10,298,176 bytes** below the hard boundary and **14,701,824 bytes** over the
development target. Budget enforcement and independent APK v2 verification
pass. [Packing audit](APK-SIZE-AUDIT.md#full-colour-sports-artwork--12-october-2026).
ADB reports no connected device; physical phone touch, appearance/FPS and WAN
qualification remain unverified. Network protocol remains **43**.

Evidence is indexed in `Builds/ColorIconQA/verification.json`,
`windows-delivery.json`, `compiled-source.json`, `source-comparison.json` and
`portability.json`. All 202 runtime source documents match the compiled player;
the two generated Game asset differences after Android are documented in the audit.

Reviewed archives:

- Retired monochrome gameplay SVGs and relocation sandbox:
  `Legacy/20261012-015503-596-colour-icons-retired-glyphs/`.
- Prior APK/audit: `Legacy/20261012-015824-930-colour-icons-before-android/`.
- Prior complete Windows player:
  `Legacy/20261012-020004-602-colour-icons-windows-delivery/`.

The retained utility geometry, licenses, previous proposals and all active
masters remain publishable. No staging, commit or push was performed.

## Sports icons and typography — 12 October 2026

Delivered complete Windows player at **Builds/WindowsFinal/**, compiled
**01:06:24 Malaysia time** (2026-10-11T17:06:24Z), with the matching bundled
license notice. **320 runtime/support files** match the tested candidate by
SHA-256, excluding the release note. The executable, Data directory, DLLs,
support folders and launchers stay together. Launch with
`Explore-Football.cmd` or `Explore-Basketball.cmd`.

Both sports use solid Material Symbols Rounded pictograms and Manrope display/
text weights, including HUD labels, scores, menus, guides and player indicators.
Basketball settings avoid fractional downscaling at the base resolution; round
volume knobs stay inside their tracks and clear of the labels. Existing button
placements, drag paths, release-only Cancel and gameplay rules remain.

**1,162 assertions pass across two fresh final-player UI reports**:

- Football: **728**, `Builds/FootballUIQA/Run-20261012-010710-372/`.
- Basketball: **434**, `Builds/BasketballUIQA/Run-20261012-010710-377/`.

Actual Unity touch-module sequences cover independent fingers, drag/cancel/
release, suspension, settings, role changes and far-finish timing. Added checks
verify every visible HUD/menu/guide glyph and the shared font family. All four
volume controls are dragged from zero to full and restored; their thumbs remain
circular within their tracks. Captures reviewed at 1600 × 900, 1280 × 720,
1560 × 720 and 1600 × 1200 cover gameplay, settings and guidance without text
truncation. These are Windows D3D11 captures, not a phone performance claim.

Fresh AndroidSubmission: **89,589,994 → 89,644,388 bytes (+54,394)**,
**10,355,612 bytes** below the hard boundary, **14,644,388 bytes** over the
development target. Budget enforcement and independent APK v2 verification
pass. [Packing audit](APK-SIZE-AUDIT.md#sports-icons-and-typography--12-october-2026).
ADB reports no connected device; physical phone touch, appearance/FPS and WAN
qualification remain unverified. Network protocol remains **43**.

Evidence is indexed in `Builds/SportsIconQA/verification.json`,
`windows-delivery.json`, `compiled-source.json`, `source-comparison.json` and
`portability.json`. All 202 runtime source documents match the compiled player;
two generated Game asset differences after Android are documented in the audit.

Reviewed archives:

- Initial candidate: `Legacy/20261012-010444-582-sports-icons-first-review/`.
- Superseded captures and relocation sandbox: `Legacy/20261012-011046-690-sports-icons-review-drafts/`.
- Prior APK/audit: `Legacy/20261012-010856-667-sports-icons-before-android/`.
- Prior complete Windows player: `Legacy/20261012-011322-811-sports-icons-windows-delivery/`.

The older running test window was closed normally before replacement. An earlier
archive attempt was blocked by that open process and moved no source directory;
its local failure record is retained. No staging, commit or push was performed.

## Football touchline UI — 12 October 2026

Delivered the complete **00:21:15 Malaysia-time** validated Windows player to
`Builds/WindowsFinal/`. All **319 runtime/support files** match the reviewed
candidate by SHA-256; `LATEST-BUILD.txt` is authored separately. Launch with
`Builds/WindowsFinal/Explore-Football.cmd`. The previous basketball audio,
props, inbound fixes and other sports remain included. Protocol **43** requires
matching room builds. [Implementation](FootballUI/implementation.md).

**1,705 passing assertions, zero failures across 11 final-candidate reports**:

- Football UI: **680**, `Builds/FootballUIQA/Run-20261012-002220-476/`.
  Actual Unity input-module touches cover independent joystick/Kick fingers,
  Cancel entry/exit/release, full-power hold and launch, wrong-pointer rejection,
  OS cancellation, suspension, resize, possession changes and menus. Captures
  cover defense, loose ball, exhaustion, all match phases, camera, guide and
  leave confirmation. Text fit and bounds are checked at 1600×900, 1280×720,
  1560×720 and 1600×1200. The initial review caught a clipped roster message
  and overly opaque map/joystick; these were fixed and rendered again.
- Match, physics and host/guest ball regression: **533**,
  `Builds/FootballMatchQA/Run-20261012-002242/` (260 + 16 + 15 + 199 + 26 + 17).
- Football effort: **72 offline + 17 host + 9 guest**,
  `Builds/FootballPaceQA/Reviews/Offline-20261012-002242-428/` and
  `Local-20261012-002259-825/`. Stamina, exhaustion/release, pressure, confirmed
  team identity and expiry of remote held input remain correct.
- Basketball UI regression: **394**,
  `Builds/BasketballUIQA/Run-20261012-003033-029/`.

`Builds/FootballUIQA/verification.json` records each result hash. Delivery
hashes, 201 matching compiled-source checks, source snapshots, atlas portability,
APK comparison and v2 signature output are alongside it. The fresh APK is
**89,589,994 bytes**, **+47,078**, with **10,410,006 bytes** of hard-limit
headroom. The 75 MB target remains exceeded. See [size record](BUILD-SIZE.md).

Source limits: Android regenerates Golf prefab IDs and URP registration; the
editor-only player writer adds a football launcher/release caption after the
Windows runtime was compiled. All runtime source hashes match, and the manually
added launcher matches the new generator. These three differences are recorded
in `source-comparison.json`. No device was connected: physical Android touch,
appearance, FPS and WAN are unverified. Rendered tests use D3D11 batch capture.

Reviewed archives:

- `Legacy/20261012-001606-135-football-ui-initial/`: initial compile-only player.
- `Legacy/20261012-001952-186-football-ui-visual-refinement/`: superseded UI candidate.
- `Legacy/20261012-002527-083-football-ui-before-android/`: previous APK/audit and successful relocated generator fixture.
- `Legacy/20261012-003204-034-football-ui-windows-delivery/`: complete superseded WindowsFinal player.

Current evidence stays under ignored Builds. No staging, commit or push was performed.

## Fishing feedback, timing and round recap — 11 October 2026

The Windows candidate was fully built at **15:12:39 Malaysia time**
(`2026-10-11T07:12:39.5353337Z`). After later nonserialized C# fixes, a
successful **scripts-only refresh** retained the complete player data. That
refresh wrote `Assembly-CSharp.dll` at **20:41:21.115** and `boot.config`
at **20:41:15.317**; its successful Unity log closed at **20:41:38.945**.
After the two result-wording fixes and final Crew padding correction, the
final successful scripts refresh finished at **21:06:07.774 Malaysia time**
(`2026-10-11T13:06:07.7741024Z`). Each refresh reuses the full 15:12 player
content; no second full serialized-content build is claimed.

The **seven selected relevant reports** below contain **2,157 counted PASS
assertions and zero FAIL lines**. The four Fishing reports follow the final
21:06 refresh. The three shared HUD/menu/Golf reports ran before the last
Fishing-only wording and Crew padding changes; their underlying shared code
is unchanged. This is relevant combined coverage, **not a claim that all
seven reports ran against the exact final snapshot**:

| Coverage | Counted assertions | Evidence |
| --- | ---: | --- |
| Final rendered Fishing gameplay, host rules, input timing, recap counters, targeting, Hook cues, all piers/cameras, native typography and landscape safe frames | 1,031 | `Builds/FishingGameplayQA/Run-20261011-210855/offline.txt` |
| Final actual local host: authoritative fishing, bonuses, replicated recap and clean rematch | 34 | `Builds/FishingGameplayQA/Run-20261011-210701/host.txt` |
| Final actual local guest: authoritative fishing, bonuses, replicated recap and clean rematch | 34 | `Builds/FishingGameplayQA/Run-20261011-210701/client.txt` |
| Final feedback/audio contracts, actual cue polling, contained target/failure labels, pending Hook handling, results modal, Crew glyphs inside the colored clock interior, native text, landscape captures and Ready/Close/Reopen routing | 146 | `Builds/FishingPolishQA/Run-20261011-210624/polish.txt` |
| Earlier shared Football/Basketball/Golf/Fishing controls, gestures, maps and native HUD skins | 157 | `Builds/SportsHudQA/Reviews/Offline-20261011-204513-007/offline/results.txt` |
| Earlier shared storybook menu, settings, help/rules, room flows and supported landscape layouts | 699 | `Builds/StorybookMenuQA/Run-20261011-204937/offline/results.txt` |
| Earlier existing Golf HUD, native text, hold/release/cancel, scorecard, countdown and tied/DNF results | 56 | `Builds/GolfHUDQA/Run-20261011-205052/golf-hud.txt` |

Counts were independently obtained from report lines beginning `PASS `, rather
than copied from older builds. The Fishing client report has **34 PASS lines**
although its terminal completion counter says `checks=32`; the recap checks
are present in the report. This counter discrepancy does not introduce FAIL
lines or invalidate the observed assertions.
The additional headless offline report in `Run-20261011-210701/` is retained
as supplementary evidence and excluded from this selected total.

A later wording correction uses `ROSTER CHANGED` for a join or departure,
and `EVERYONE CAUGHT · CREW GOALS INCOMPLETE` when the point basket is met
but another Crew requirement is missing. Its successful intermediate
**scripts-only refresh** finished at **20:57:30.774 Malaysia time**
(`2026-10-11T12:57:30.7743055Z`). Its focused rendered Fishing polish rerun,
`Builds/FishingPolishQA/Run-20261011-205738/polish.txt`, contains **146 PASS
lines, zero FAIL lines, and `checks=146`**. This includes two new meaningful
fixtures for the met basket with a missing fish size, and the roster-change
label covering both joins and departures. The final 21:06 Polish rerun retains
both fixtures and the same 146-count result, with a stronger Crew interior
check replacing the earlier geometry-only assertion.

Visual review of the 20:57
1600 × 900 Crew capture found that the white contribution label reached the
pale bottom border even though its text geometry existed. The new fix uses
the existing layout's **26-unit padding** below the Crew strip, expands the
clock accordingly, and preserves the timer and Wanted header position.
The stronger landscape fixture checks each actual rendered Crew glyph
vertex in clock coordinates against its **20-unit illustrated border**,
in addition to native typography, safe-frame and cross-group separation
checks. The successful `build-windows-crew-padding.log` refresh is followed
by the final rendered 146-check Polish report and reviewed 1600 × 900
capture, where the contribution label is clear inside the colored clock.
No serialized asset changed for this correction. The fresh
Android build was paused for the wording correction and then this visual
fix; neither interrupted attempt is a delivered APK.

The selected results treatment is a centered native sticker card after the
round. It shows frozen standings, catch counts by size, Wanted bonus points,
missed bites, snapped lines, cancellations, timeouts, local personal bests,
Cup totals and unmet Crew conditions. Wanted bonuses remain included in the
normal round score; the recap does not add them again. Close immediately
restores walking; Results reopens the same snapshot. Ready & Close queues
the existing Ready action once, and an already-ready player is not toggled
unready. Keyboard/controller focus and navigation stay inside the open card;
closing, a new round and context reset clear the owned selection. Delayed
local-player records retry before a round is marked shown.

The live Crew clock contains fish-size checkmarks and identifies the next
fisher needing a catch. Native Cove lettering reserves its actual line metrics
instead of clipping the first line in short text rectangles. Existing artwork,
fonts and frames are reused; no new texture or effects package is introduced
for results. New celebration effects remain deferred.

Rendered results captures cover **1600 × 900, 1280 × 960, 1920 × 900,
1280 × 720 and 844 × 390**, with asymmetric insets. Crew captures cover
**1600 × 900, 1280 × 960, 1920 × 900 and 844 × 390**. The **five-row standings,
delayed-record and missing-contributor scenarios are synthetic presentation
fixtures**. They establish UI/state behavior, not five-device multiplayer.
The separate host/guest Fishing reports use actual local network processes,
including authoritative awards and replicated recap/rematch data. Desktop
844 × 390 review is a phone-shaped Windows viewport, not a physical Android
test. Physical-phone rendering, touch, vibration, sustained FPS/heat and WAN
latency/disconnect qualification remain unverified. Sound cue polling and
generated audio are checked; perceptual listening quality is not established
by assertion counts alone.

**Player integrity and source limits:** the saved V3 runtime manifest contains
**317 runtime/support files plus `LATEST-BUILD.txt` (318 total)**. Comparison
against the measured scripts-refreshed candidates, including final V6, independently
finds changes in exactly
`WhatTheFish_Data/boot.config`,
`WhatTheFish_Data/Managed/Assembly-CSharp.dll` and
`WhatTheFish_Data/Managed/Assembly-CSharp.pdb`. All other player files retain
their V3 SHA-256. Evidence is `windows-v3-runtime-files.json` and
`scripts-refresh-runtime-delta.json`, `copy-fix-runtime-delta.json` and
`crew-padding-runtime-delta.json` under
`Builds/FishingPolishQA/Task-20261011-143503/`. The final
`windows-v6-runtime-files.json` contains all **317 runtime/support files**,
excluding the delivery note; all 317 hashes were independently matched to
the complete candidate.

The **281-entry** source snapshots cover **253 C# files**, the Fishing results
layout, Bootstrap scene, packages and project settings. They are deliberately
narrow manifests: they do **not** hash every Unity asset, shader, texture,
mesh, material, `.meta` file or tool, and are not proof of a complete
all-source build snapshot. V3 versus final
`windows-source-inputs-v6-crew-padding.json` changes exactly **five manifested
C# paths**: `FishingPolishBuild.cs`, `FishingGameplayProbe.cs`,
`FishingResultsChecks.cs`, `FishingHUD.cs` and `FishingResultsUI.cs`.
`windows-source-post-v6.json` records no mismatches across those 281 narrow
inputs after the refresh. The final Fishing reports above follow this
refresh; the three unchanged shared-code reports are explicitly earlier.

Generated Golf Ball text was restored only after a format-aware Unity YAML
comparison proved all **14 objects**, component/child order, mesh/material/LOD/
collider fields and GUID-linked external references equivalent. Only local
file-ID assignment and document order differed. The restoration preserves
player files and the Git index. Evidence is
`GeneratedDrift/generated-drift-audit.json` and
`ball-graph-equal-restoration.json` in the same primary task folder. The audited
URP global settings matched the baseline at that inspection; the subsequent
fresh Android packing audit below independently confirms the mobile pipeline.

Early candidate failures remain diagnostic evidence: malformed initial new
metadata prevented import; a pending-Hook fixture exposed its first failure;
later native title/label rectangles exposed truncation from Cove line metrics.
The passing rendered reports follow the import, pending-input and
geometry fixes. The first scripts refresh failed to compile a development
mesh-read helper; the corrected refresh succeeds. Earlier failed logs and
captures are retained. The 20:57 focused checks preserved the strict
readability and gameplay requirements but missed contribution text crossing
the illustrated border. Screenshot review caught that defect; the final
26-unit padding and actual 20-unit clock-interior glyph checks pass across
all four Crew aspects. This extends the earlier own-label geometry check
without weakening it.

Fresh AndroidSubmission written at **21:16:21 Malaysia time** is
**88,856,012 bytes (+43,894 from 88,812,118)**. Strict-boundary headroom is
**11,143,988 bytes**; the **75 MB target is exceeded by 13,856,012 bytes**.
Budget enforcement and independent APK v2 signature verification pass.
[Release and hash](BUILD-SIZE.md#fishing-feedback-timing-and-round-recap--11-october-2026),
[packing audit](APK-SIZE-AUDIT.md#fishing-feedback-timing-and-round-recap--11-october-2026).
Evidence is `ApkComparison-20261011-131850/` in the primary task and
`Builds/SizeAudit/latest/`. All 1,785 mesh rows are unchanged; the mobile
pipeline ships, without unintended desktop rendering assets. Executable
review types are absent from release IL2CPP metadata, although Unity retains
208 bytes of MonoScript asset metadata for two review sources.

The complete candidate was promoted to `Builds/WindowsFinal/` at **21:12:15**.
All **317 runtime/support SHA-256 hashes** match the tested V6 manifest, with
`LATEST-BUILD.txt` bringing the folder to **318 files**. Evidence is
`windows-promotion-integrity.json`; the delivery note records changes,
validation, APK measurement and source limits. The previous delivery note
was checked against the task baseline before replacement to preserve any
newer concurrent delivery.

**Post-build editor-only exception:** the new QA helper's `Windows()` output
now resolves explicitly from `Application.dataPath`. That path-only edit
occurred after the APK/player builds; it is excluded from shipping runtime.
`post-android-source-delta.json` identifies exactly that one changed input;
the other 280 narrow-manifest inputs match. Compiling the actual helper with
minimal Unity API stubs passes **11 output-path checks**, using two differently
named project roots with spaces and an unrelated working directory.
`Builds/FishingPolishQA/primary/PortableHelper/` holds the evidence. This is a
helper-path check, not a complete fresh-clone Unity build.

The final format-aware Android inspection in
`GeneratedDriftAfterAndroid/generated-drift-audit.json` again finds identical
14-object Ball graphs and baseline URP definitions. Its accompanying
`ball-graph-equal-restoration.json` records the reviewed local-ID restoration.
Neither player nor APK bytes were modified by these source restorations.

Reviewed superseded players and the baseline APK/audit are recoverable in
these ignored archive batches, with original paths, sizes and SHA-256:

- `Legacy/20261011-145908-710-fishing-polish-first-candidate/`
- `Legacy/20261011-150807-369-fishing-polish-font-candidate/`
- `Legacy/20261011-205048-934-fishing-polish-before-android/`
- `Legacy/20261011-211135-879-fishing-polish-windows-delivery/`

Current validation evidence remains under ignored `Builds/`. Changes are left
unstaged and unpublished; staging, committing and pushing remain with the user.

## Basketball gameplay audio — 11 October 2026

Delivered the complete **19:11:47 Malaysia-time** Windows player to
`Builds/WindowsFinal/`. All **318 runtime/support files** match the tested
candidate by SHA-256; `LATEST-BUILD.txt` is authored separately. Launch with
`Builds/WindowsFinal/Explore-Basketball.cmd`. Existing approved UI, props,
inbound rules and ball-first camera are included. Protocol **43** requires
matching host/guest builds.

The user-approved 58-clip bank is synchronized with court/ball contacts,
possession, releases, scoring, footsteps/landings, props and UI. The Lock-on
aim stays two seconds with a lower pitch and **7.96 dB** reduction. Import
normalization is disabled and runtime decoded peak is **0.06448364**. The
court settings now expose Master, Court sounds, Buttons and Ambience.
[Action mapping, mixing and provenance](BASKETBALL-AUDIO.md).

**167 passing assertions in four final-player reports**:

- Audio offline: 35 passes, `Builds/BasketballAudioQA/Offline-20261011-191520-609/`.
- Audio local host/guest: 35 + 20 passes,
  `Builds/BasketballAudioQA/Local-20261011-191602-259/`. The guest plays 34 unique
  authoritative event IDs, without duplicates or stale/overflow drops.
- Inbound and menu regression: 77 passes,
  `Builds/BasketballInboundQA/Offline-20261011-191650-937-c35b4e/`.
  Rendered court settings were inspected: labels, volume values, handles and
  existing controls fit. The original touch and inbound behavior remains intact.

`Test-BasketballAudio.ps1` drives actual pickups, pass release, falling floor
contacts, backboard/rim collisions, moving feet, jump landing, prop collection,
Lock-on release/score/cancel, portal intake/arrival/close, Turbo expiry,
button deduplication, ongoing-aim mute, master mute and application suspension.
The initial offline run caught a net-prefix lookup also selecting the net-brush
clip; exact name matching fixes this, and the final bank has exactly 58 clips.
The final build log is `Builds/BasketballAudioQA/build-20261011-191022.log`.

A relocated source fixture with spaces in its path and an unrelated caller
directory reproduced all **58 delivery WAV hashes** without network access.
`portable-delivery.json`, `verification.json`, complete player manifests and
source snapshots are retained in `Builds/BasketballAudioQA/`.

Fresh AndroidSubmission and independent APK v2 verification pass:
**89,202,254 → 89,542,916 bytes (+340,662)**, with **10,457,084 bytes** of
hard-limit headroom. The 75 MB target remains exceeded by **14,542,916 bytes**.
[Size/hash](BUILD-SIZE.md#basketball-gameplay-audio--11-october-2026) and
[asset audit](APK-SIZE-AUDIT.md#basketball-gameplay-audio--11-october-2026).
**3,445 / 3,447 Game inputs** match between tested Windows and Android
generation; only generated Golf prefab serialization and URP registration
differ. Runtime code, approved audio and import settings match.

These are rendered Windows engine/event checks, not a recorded final mix or
a perceptual audition by the assistant. User gameplay listening, physical
Android speakers/latency, touch/FPS and WAN behavior remain unverified; ADB
reported no connected device. The earlier native non-batch Unity shutdown
issue was not newly qualified by these batch-mode reviews.

Reviewed archives:
`Legacy/20261011-185526-060-basketball-audio-lock-revision/` preserves the old
Lock-on preview and affected reels;
`Legacy/20261011-191701-160-basketball-audio-before-android/` preserves the
previous APK/audit and regeneration fixture;
`Legacy/20261011-192036-297-basketball-audio-windows-delivery/` preserves the
complete superseded Windows player. No staging, commit or push was performed.

## Basketball inbound and touch fixes — 11 October 2026

Delivered the complete **18:30:28 Malaysia-time** Windows player to
`Builds/WindowsFinal/`, including the Data directory, runtime DLLs, support
folders and launchers. All **317 runtime/support files** match the tested
candidate and portable copy; `LATEST-BUILD.txt` is authored separately.
Launch with `Builds/WindowsFinal/Explore-Basketball.cmd`.

- Regular, symmetric eight-tooth Settings gear, from the SVG master and existing atlas.
- Wall, movement containment and ball rules use the actual 15.24 × 28.6512 m court.
  Two continuous seconds outside at any height starts a solo standing inbound at
  the first crossing. Re-entry before the deadline resets the timer. Basket
  supports use the nearest clear spot on the same line. Shooting, jumping,
  moving and props stay blocked until a pass physically releases; Cancel and
  Settings preserve the restart. Offline and a network host alone use this rule;
  multiplayer team throw-in ownership is deferred and those rooms retain recovery resets.
- Entering Cancel arms it; only lifting there cancels. Sliding back preserves
  the hold and charge phase. The coral cue explains the release action.
- Pass charge fills in 0.4 s; horizontal velocity is 1.5× at every charge level
  for chest, loft and bounce. Preview and real launch share the plan. A repeated
  floor contact during the CCD transition no longer clears the clean bounce's
  catchable-pass state or produces a second bounce. Wall interceptions remain physical.

**1,357 passing assertions in nine reports**, listed in
`Builds/BasketballInboundQA/verification.json`. Final-player checks include
77 offline and 77 solo-host inbound checks, 378 touch/layout checks repeated
from a portable copy launched with an unrelated working directory, 140
trajectory/shot-odds checks and 31 local host/guest pass checks. Earlier candidate
checks cover 98 gameplay and 178 props assertions; subsequent gameplay changes
were the inbound ownership guard and bounce-contact fix, both covered again by
the final-player suites. Test commands are `Test-BasketballInbound.ps1` (with
optional `-HostSolo`), `Test-BasketballUI.ps1`, `Test-BasketballTrajectory.ps1`,
`Test-BasketballPass.ps1`, `Test-BasketballGameplay.ps1` and `Test-BasketballProps.ps1`
under `Tools/Build/`. Use the rendered props test for material support checks.

Rendered gear/settings, armed Cancel, court edges and basket-stand restarts were
inspected. Menus and guide text fit at 1280×720, 1560×720 and 1600×1200. The
input-module suite exercises actual touch IDs, independent movement fingers,
foreign-finger rejection, sliding back, release in Cancel, outside release,
OS cancellation, pause and timed far-finish fallback. The inbound suite covers
all edges, both supports, corners, falls, stationary restrictions and actual release.

Fresh AndroidSubmission and independent APK v2 verification pass:
**89,200,326 → 89,202,254 bytes (+1,928)**, with **10,797,746 bytes** of hard-limit
headroom. The 75 MB development target remains exceeded by **14,202,254 bytes**.
[Size and hash](BUILD-SIZE.md#basketball-inbound-and-touch-fixes--11-october-2026).
No physical Android device was connected; phone appearance, touch reach/latency,
FPS and WAN behavior remain unverified. Rendered reviews use D3D11 batch mode;
ordinary interactive shutdown was not newly qualified against the existing
Unity native shutdown issue.

`player-manifest.json`, `delivery-manifest.json`, `windows-source.json` and
`source-delta.json` preserve complete-file and source evidence. **3,318 / 3,320**
Game inputs match after Android generation; exceptions are generated Golf ball
prefab serialization and URP resource registration. Runtime code, art and fonts match.
The bounce fault trace is retained at
`Builds/BasketballPassQA/Reviews/Local-30-20261011-182718-354/`.

Reviewed archives:
`Legacy/20261011-181955-468-basketball-inbound-first-review/`,
`Legacy/20261011-182517-688-basketball-inbound-catch-review/`,
`Legacy/20261011-182851-612-basketball-inbound-bounce-fix/`,
`Legacy/20261011-183217-747-basketball-inbound-before-android/`, and
`Legacy/20261011-183818-785-basketball-inbound-windows-delivery/`, and
`Legacy/20261011-184034-287-basketball-inbound-cleanup/`.
The Windows delivery batch contains the complete superseded player; cleanup
contains superseded captures, logs and duplicate tested player copies. Current task
validation remains under ignored `Builds/`; no game asset was archived.
No staging, commit or push was performed.

## Basketball mobile controls — 11 October 2026

Implemented the approved [basketball mobile UI](BasketballUI/README.md) with the
original icon masters, palette, button sizes and safe-area placement. The HUD
removes court/FPS/station-distance/key labels and places camera, free roam,
leaving and the Rules / Controls / Props guide behind the top-right gear.
Far Dunk/Layup shows its actual reason and retains the current timed-shot arc;
only an eligible finish hides timing. Main controls stay fixed during a hold.

**1,283 passing assertions across eight reports** cover:

- Unity's actual touch input module: two fingers, ownership, upper/diagonal
  paths, returning to centre, immediate Cancel, outside release, OS cancellation,
  suspension and menus during a hold. Both far finishes release exactly one
  ordinary shot using the displayed phase; eligible finishes hide the arc.
- Settings and guide text bounds, camera choices, menu input blocking, offscreen
  marker suppression, safe-screen controls at 1280×720, 1560×720 and 1600×1200.
  Actual rendered attack, defense, gesture and menu captures were reviewed.
- Existing charge (47), finish gesture (76), pass trajectory (139), defense
  (246), and two-player local pass (19 host / 12 guest) checks. Finish/pass
  regressions preceded the final text/tint and menu-marker presentation polish;
  the final 372-assertion UI suite passed twice on the delivered build.
- A complete portable copy ran from a directory with spaces and an unrelated
  working directory. All **318 candidate files** matched before promotion;
  **317 runtime/support files** remain identical after authoring the delivery note.
  Portable atlas regeneration also produced exactly the same PNG hash. All
  sixteen SVG source hashes match and have no external dependencies.

The complete player was promoted to `Builds/WindowsFinal/`; start
`Explore-Basketball.cmd`. Build time: **17:20:29 Malaysia time**. The final
AndroidSubmission is **89,200,326 bytes**, with **10,799,674 bytes** to the
strict limit and a valid APK v2 signature. [Full size record](BUILD-SIZE.md#basketball-mobile-controls--11-october-2026).
The 75 MB development target remains exceeded. Existing protocol-41 props and
ball-first camera changes are included; unrelated working changes are preserved.

Source comparison matches **3,314 of 3,316 inputs** after Android generation.
Only the generated Golf ball prefab and URP resource registration differ;
all runtime C#, UI sprites, fonts and other inputs match. No runtime source was
edited after the final build. Evidence: `Builds/BasketballUIQA/verification.json`,
`player-manifest.json`, `delivery-manifest.json`, `windows-source.json`,
`source-delta.json`, `Run-20261011-172108-637/`, `PortableEvidence/`,
`portable-atlas.txt`, `apk-comparison.json`, `apk-signature.txt`, and the individual
regression report paths listed in `verification.json`.

Physical Android appearance, thumb reach, latency/FPS and WAN qualification are
unverified; no device was connected. Initial hidden non-batch capture runs hit
the repository's documented native Unity shutdown fault after completing their
checks. Final rendered reviews use D3D11 batch capture and exit with code 0.
This does not newly qualify ordinary interactive-player shutdown; see the
[existing engine limitation](VisualDirection/CoastalStadiums/GameReview/VALIDATION.md).

Reviewed archives:

- `Legacy/20261011-171516-579-basketball-ui-before-android/`: prior APK/audit.
- `Legacy/20261011-171652-905-basketball-ui-windows-delivery/`: previous camera player.
- `Legacy/20261011-172204-938-basketball-ui-final-polish/`: superseded UI builds,
  captures, diagnostics and duplicate portable output.
- `Legacy/20261011-172615-057-basketball-ui-cleanup/`: redundant portable wrapper /
  captures and preliminary hygiene report after successful promotion.

Active assets and the approved design reference remain publishable. No staging,
commit, push or history changes were performed. Final repository checks are
recorded in `Builds/BasketballUIQA/task-ready.txt`.

## Ball-first Stadium camera — 11 October 2026

The complete Windows player in `Builds/WindowsFinal/` was built at **16:45:40
Malaysia time**. All **317 runtime/support files** match the validated candidate
and portable copy. The camera follows the ball, leads moving play, retains the
court under high arcs and considers nearby basketball rims. The local athlete
has a small nearby influence and no distant pull. A cyan YOU edge arrow shows
the horizontal distance to the ball, avoids the finished HUD graphics, and never
captures touches. [Behaviour and implementation](STADIUM-CAMERA.md).

**617 assertions pass across six final reports:** 114 camera checks each at
1280×720, 1600×720 and portable 1024×768; 202 host, 38 opponent and 35 teammate
checks in the existing basketball ability suite. The passive network camera
observers recorded **15,546 frames**, including **3,349 airborne frames** and
**1,814 offscreen-character frames**, with **zero** ball-visibility or missing-
arrow failures. The rendered run uses real replicated shots, catches and Portal
transfers. A preliminary run without graphics could not validate material
support; the complete rendered rerun passes.

Camera checks cover centre/edge/corner ball positions, nearby and distant player
framing, continuous arcs in both directions, court reference beneath flights,
teleport/reset recovery, held-dribble stability, a real approximately 9-metre
high Lock-on arc and successful scoring, movement remapping through the motor,
first/third-person restoration, sport re-entry and disable cleanup. Arrow checks
cover behind-camera bearings, hiding on re-entry, touch transparency and actual
styled-button overlap. Full-HUD captures were inspected at the tested layouts.
The portable copy runs from a path containing spaces, with default root resolution
and an unrelated working directory.

Source scope: ongoing basketball UI edits appeared after the tested Windows
build and initially prevented a shared-workspace Android compile. A snapshot
matched all **3,304 Game input hashes** from the Windows build; the APK was built
there. A concurrent icon sheet already present at Windows build time is included;
later UI source edits are preserved in the workspace but excluded. All camera
and gameplay code matches between the delivered players. Generated Golf prefab
IDs/order are semantically equivalent; mobile rendering registrations differ as
expected. Protocol **41** and the existing centre-line prop countdown are retained.

Fresh AndroidSubmission at **16:57:15** measures **89,178,578 bytes**, up **61,926**,
with **10,821,422 bytes** of boundary headroom. Budget and independent APK v2
signature checks pass. The 75 MB development target remains exceeded.
[Release hash and packing evidence](BUILD-SIZE.md#ball-first-stadium-camera--11-october-2026).
Physical Android appearance, touch/FPS, actual-device networking and WAN latency
qualification are not established by these desktop runs.

Current evidence: `Builds/BallCameraQA/verification.json`, `player-manifest.json`,
`PortableEvidence/1024x768-20261011-164703-878/`, the source/snapshot records and
build logs; `Builds/BroadcastCameraQA/Reviews/1280x720-20261011-164601-230/` and
`1600x720-20261011-164601-238/`; and
`Builds/BasketballPropsQA/Local-20261011-164800-007/`.
The retained ignored snapshot preserves reproducibility without changing the
concurrent workspace. No staging, commit or push was performed.
`Tools/Build/Check-TaskReady.ps1 -RequireApk` passes; evidence is
`Builds/BallCameraQA/task-ready.txt`. The read-only check does not certify the
newer concurrent UI source as part of this tested build.

Previous Windows delivery: `Legacy/20261011-165211-231-ball-camera-windows-delivery/`.
Prior APK/audit: `Legacy/20261011-164821-353-ball-camera-before-android/`.
Superseded visual iterations and diagnostic runs:
`Legacy/20261011-163953-153-ball-camera-first-review/`,
`Legacy/20261011-164405-544-ball-camera-marker-review/`, and
`Legacy/20261011-165209-423-ball-camera-test-diagnostics/`.
The duplicate portable runtime and superseded helper/preliminary check are in
`Legacy/20261011-165900-007-ball-camera-cleanup/`; current portable evidence is retained.

## Centre-line prop countdown — 11 October 2026

Complete Windows player built at **15:49:52 Malaysia time**, delivered in
`Builds/WindowsFinal/`. All **317 runtime/support files** match the tested
candidate and independent portable copy, with the delivery note as file 318.
**453 assertions across four reports pass** on this same final player:

| Coverage | Assertions | Evidence |
| --- | ---: | --- |
| Portable solo props and countdown | 178 | `Builds/BasketballSpawnQA/PortableEvidence/Offline-20261011-155027-958/` |
| Host | 202 | `Builds/BasketballPropsQA/Local-20261011-155209-139/host/` |
| Opponent client | 38 | `Builds/BasketballPropsQA/Local-20261011-155209-139/guest/` |
| Teammate client | 35 | `Builds/BasketballPropsQA/Local-20261011-155209-139/teammate/` |

Both fixed centre-line apron points clear architecture. Each model renders at
both points. Solo and all three local peers observe **5, 4, 3, 2, 1**, with
centred, extruded digit meshes and an unchanged announced side/deadline. No prop
renders or awards inventory early. First spawn takes five seconds; subsequent
respawning retains the twenty-second interval, including the final five-second
warning. A player waiting on the location receives the prop after the countdown.
Sixteen accelerated host cycles per solo/host run never repeat the prior type;
both possible random choices also exclude it. Disabling the arena clears its
pending countdown, and re-entry starts a fresh one. Existing Turbo, Lock-on,
Portal, physical scoring, interruption, duplicate RPC and session checks pass.

Close and court-distance countdown captures were visually inspected. The final
solo `offline/countdown-5.png` through `countdown-1.png` and `countdown-court.png`
show the real world hologram; `spawned-*.png` shows actual pickup instances.
Asymmetric digits use their mesh bounds for centring. The portable run used a
path containing spaces and launched the copied test script with default root
resolution from an unrelated working directory. No runtime exceptions occurred.
`Builds/BasketballSpawnQA/verification.json` and `player-manifest.json` index
the evidence. No gameplay source changed after the final build. Generated Golf
prefab IDs/order and platform rendering registration were normalized after
semantic verification, recorded in `generated-source-cleanup.json`.

Fresh AndroidSubmission completed at **15:56:46**, **89,116,652 bytes**, with
**10,883,348 bytes** strict-boundary headroom and a valid APK v2 signature.
The **75 MB development target remains unmet**. All peers require **protocol 41**.
Previous Windows player and completed portable fixture:
`Legacy/20261011-155507-931-basketball-countdown-delivery/`. Earlier visual
reviews, including the detected digit-alignment failure, are recoverable in
`Legacy/20261011-154940-861-basketball-countdown-intermediate/`. Previous APK:
`Legacy/20261011-155414-437-basketball-countdown-before-android/`.
Physical Android appearance, touch/FPS, actual-device late joins/disconnects
and separate-network latency remain unverified.

## Visible basketball props and ability tuning — 11 October 2026

Complete Windows player built at **14:01:46 Malaysia time** and promoted to
`Builds/WindowsFinal/`. All **317 runtime/support files** match the tested
candidate and independent portable copy; the delivery also contains its build
note. Fresh checks pass **298 assertions across five reports**, all using this
same player, with no gameplay source changes after the build.

| Coverage | Assertions | Evidence |
| --- | ---: | --- |
| Props, solo | 102 | `Builds/BasketballPropsQA/Offline-20261011-140201-204/` |
| Props, three local peers | 149 | `Builds/BasketballPropsQA/Local-20261011-140305-580/` |
| Ordinary charge, touch cancellation and boundaries, portable copy | 47 | `Builds/BasketballPropFixQA/PortableEvidence/Run-20261011-140436/` |

The old pickup animation replaced imported scale 100 with 0.95, shrinking the
models by 100 times. Actual pickups now preserve that scale before applying the
0.95 presentation factor. All three types exercise the real spawn renderer in
both halves, including replacement and random reset. Mesh bounds, materials
and floor clearance pass. The solo report's `spawned-LockOn.png`,
`spawned-Turbo.png` and `spawned-Portal.png` were visually inspected. These are
live spawn instances, unlike the separately staged gallery in the earlier record.

Solo Turbo controller and travel speeds measure **6 / 10.5 m/s**, exactly 1.5×
normal walk/sprint. Local teammate travel measures approximately **5.98 / 10.51
m/s**, while the opponent stays at approximately **4.09 / 7.01 m/s**. Five-second
expiry restores normal movement. Lock-on's deadline is two seconds; physical
release is **2.028 s solo / 2.030 s host**, within one simulation-step allowance.
Stationary aiming, spent interrupted shots, ordinary blocks, long-range scoring,
portal races, duplicate RPCs and session cleanup pass.

The complete portable copy ran from a directory containing spaces, using the
copied charge test's default root resolution from an unrelated working directory.
`Builds/BasketballPropFixQA/verification.json` indexes reports and manifest
agreement. Generated Golf prefab IDs/order and platform rendering registrations
were normalized after semantic verification; see `generated-source-cleanup.json`.

Fresh AndroidSubmission completed at **14:07:11**, **89,113,480 bytes**, with
**10,886,520 bytes** strict-boundary headroom and a valid APK v2 signature.
The **75 MB development target remains unmet**. All peers require **protocol 40**.
The old complete Windows player and portable fixture are archived in
`Legacy/20261011-140604-130-basketball-prop-fix-delivery/`; current evidence stays
under `Builds/`. Physical Android appearance, touch/FPS, actual-device
disconnects/late joins and separate-network latency remain unverified.

## Basketball props — 11 October 2026

The complete Windows player was built at **13:16:35 Malaysia time** and
promoted to `Builds/WindowsFinal/`. All **317 runtime/support files** match
the tested candidate and independent portable copy, with a separate delivery
note making **318 files** in the folder. Selected checks pass **812
assertions across 14 reports**:

| Coverage | Assertions | Evidence |
| --- | ---: | --- |
| Props, portable solo | 66 | `Builds/BasketballPropsQA/PortableFinalEvidence/Offline-20261011-131742-047/` |
| Props, three local peers | 93 | `Builds/BasketballPropsQA/Local-20261011-131837-344/` |
| Court boundaries, recovery, ordinary shots and island lifecycle | 98 | `Builds/BasketballGameplayQA/Offline-20261011-125824/` |
| Shot charge, touch cancellation and boundary effects | 47 | `Builds/BasketballChargeQA/Reviews/Run-20261011-132030/` |
| Guard, block and interrupted finishes, two local peers | 74 | `Builds/BasketballDefenseQA/Reviews/Local-30-20261011-130046-192/` |
| Passing and authoritative ordinary-shot probability, two local peers | 50 | `Builds/BasketballTrajectoryQA/Reviews/Local-30-20261011-130131-960/` |
| Ordinary steals, two local peers | 42 | `Builds/BasketballStealQA/Local-20261011-130200-662/` |
| Layups, dunks, fallbacks and physical recovery | 185 | `Builds/BasketballFinishQA/Reviews/Offline-20261011-132107-637/` |
| Rendered shared HUD, four sports | 157 | `Builds/SportsHudQA/Reviews/Offline-20261011-131040-166/` |

The final prop runs cover both teams' opposing-hoop selection, real scoring
from corners and behind the backboard, one-click three-second aim, immediate
consumption, ordinary-block interruption, normal speed restoration after five
seconds, teammate-only boosts, replacement revisions, duplicate owner RPCs,
home-half gating, simultaneous portal requests, opponent/teammate ownership,
in-flight interception, receiver departure and lifecycle cleanup. Receiving
hands follow the actual portal destination. A gallery stages all three models
for visual review; live play spawns at most two pickups.

`Builds/BasketballPropsQA/Local-20261011-131837-344/host/` contains the final
model gallery, ability HUD, aiming and portal captures. The portable run used
an independent directory containing spaces and launched its copied test script
from an unrelated working directory, with default project-root resolution.
Its current evidence is copied to `PortableFinalEvidence/` in the task QA folder.
Blender regeneration from a second root also preserves canonical geometry;
format-aware inspection found no external image/library dependencies or active
absolute FBX paths. `asset-audit.json` records those checks.

Source-snapshot limits: props, charge and finish reports use the final player.
Shared HUD checks used the 13:06 player; general gameplay, defense, passing and
ordinary-steal checks used the 12:57 player. Subsequent production edits were
limited to prop-specific opposing-hoop selection and portal receiving hands,
covered by the final runs. No further gameplay edits followed the final build.
Build-generated Golf prefab ID/order churn was removed only after comparing its
component graph; desktop SSAO runtime registrations were restored to the mobile
source configuration. Each platform builder prepares its intended feature set.
`generated-source-cleanup.json` records this normalization.

Fresh AndroidSubmission completed at **13:22:20**, with APK **89,109,564 bytes**,
**10,890,436 bytes** below the strict boundary. Independent APK v2 verification
passes. The **75 MB development target remains unmet**. The pre-task local APK
predated current source; [build size](BUILD-SIZE.md#basketball-props--11-october-2026)
and [packing audit](APK-SIZE-AUDIT.md#basketball-props--11-october-2026) distinguish
that baseline from the last documented release. Protocol **39** is required.

The latest intermediate Windows delivery and completed fixtures are archived in
`Legacy/20261011-132229-805-basketball-props-final-delivery/`. The original Windows
release and earlier review runs are in
`Legacy/20261011-131152-728-basketball-props-delivery/`; the original APK is in
`Legacy/20261011-125700-224-basketball-props-before-android/`. Additional completed
portable fixtures and the intermediate APK are in the task's `130700` and
`131524` archive batches. Current validation evidence remains under `Builds/`.

Physical Android appearance, touch/FPS, real-device disconnect/late-join behavior
and separate-network latency qualification remain unverified. These are Windows
executable and local multi-process checks, not a guarantee against every bug.

## Golf sticker HUD — 10 October 2026

The complete final Windows player was fully built at **14:37:46 Malaysia
time**. Selected final checks pass **1,549 assertions across ten reports**:

| Coverage | Assertions | Evidence |
| --- | ---: | --- |
| Native HUD, actual raycast routing, hold/release/cancel, ten-row scorecard, typography, dividers, landscape safe frames, countdown and tied/DNF results | 56 | `Builds/GolfHUDQA/Run-20261010-143850/` |
| Aim stance, cancellation, guide, original input semantics and local host/guest | 96 | `Builds/GolfAimQA/Run-20261010-144227/` |
| Five-hole progression, strokes, ranking, finish times, countdown and local host/guest match | 177 | `Builds/GolfMiniGameQA/Run-20261010-144304/` |
| Physical shot profiles and guide behavior | 187 | `Builds/GolfShotQA/Run-20261010-144408/` |
| Shared Football/Basketball/Golf/Fishing controls, original gestures, maps and native skins | 157 | `Builds/SportsHudQA/Reviews/Offline-20261010-144444-352/` |
| Rendered Fishing gameplay, controls, Hook cue, all piers/cameras, rules, typography and landscapes | 876 | `Builds/FishingGameplayQA/Run-20261010-144503/` |

The Golf captures show A's cream/navy/gold treatment, C's table divider lines
and B's framed **! Hold, then release** cue. Reviewed layouts include 16:9,
4:3 and wide landscape with asymmetric safe-area insets. Native fonts render
all captions; illustrations contain no baked text. The complete local row is
blue. Existing power cycling, stroke/ranking rules, club/cart animation,
camera/jump eligibility and input handlers remain. Ten-row table fixtures
are synthetic UI data, not ten-device multiplayer qualification.

The complete tested directory was moved into `Builds/WindowsFinal/`, with
**317 runtime/support files plus its note (318 total)**. Every runtime/support
SHA-256 matches the tested candidate. An independent complete player copy
under a different root containing spaces also matches all hashes. The new
Golf launcher passes another **56 assertions/eight captures** using its default
script-relative player path from an unrelated temporary working directory.
Portable artwork regeneration also preserves both delivery PNG hashes.
Evidence is under `Builds/GolfHUDQA/Task-20261010-125740/`, including
`runtime-integrity.json`, `portable-player-integrity.json`,
`portable-generator-check.json`, `validation-index.json` and manifests.

**Source and failed observations:** the old shared Fishing diagnostic required
glyphs from intentionally disabled Camera/Jump labels. Both the prior player
and first candidate fail those same two checks. The corrected development-only
review filters active/enabled text; actual Fishing behavior is unchanged.
AndroidSubmission excludes this review, and both review types are absent from
the release IL2CPP metadata. Production feature/art inputs match across builds.
The post-build source manifest identifies the single excluded diagnostic edit;
it is not a complete pre-build input snapshot. Generated Golf Ball IDs/order
were restored only after all **14 Unity objects** and external references
matched. The mobile URP runtime list was restored after the desktop build added
two required SSAO containers; the tested Windows player retains its desktop
resources. Other URP definitions/settings are unchanged.

A short Aim shot-distance observation failed in
`Builds/GolfAimQA/Run-20261010-143921/`; the same unchanged final player repeat
passes. No cause is established. A Fishing Hook pulse observation failed while
Android was building in `Builds/FishingGameplayQA/Run-20261010-142430/`; the
final isolated run passes all 876 checks. Those failed reports remain diagnostic
evidence. Early HUD captures also exposed a clipped **!**, an unwanted shine
line and a ball-caption overlap; the final player includes the visual fixes.

Fresh AndroidSubmission is **88,812,118 bytes (+51,966)**, with **11,187,882
bytes** below the strict boundary. Budget enforcement and independent v2
signature verification pass. The 75 MB target remains exceeded by
**13,812,118 bytes**. [Release](BUILD-SIZE.md#golf-sticker-hud--10-october-2026),
[packing audit](APK-SIZE-AUDIT.md#golf-sticker-hud--10-october-2026).
Physical Android rendering, touch/FPS and separate-network qualification remain
unverified; Windows review does not establish those results.

Superseded players and the baseline APK/audit are recoverable in these reviewed
ignored archive batches with original paths, sizes and hashes:

- `Legacy/20261010-140923-350-golf-hud-initial-player/`
- `Legacy/20261010-141911-246-golf-hud-before-android/`
- `Legacy/20261010-143242-960-golf-hud-before-fixture-fix/`
- `Legacy/20261010-144857-949-golf-hud-windows-delivery/`
- `Legacy/20261010-145426-948-golf-hud-portable-player/`

Implementation was initially handed back unstaged. The user subsequently
authorized publishing these Golf HUD changes directly to `main`.

## Fishing timer spacing — 9 October 2026

The complete new Windows player was fully built at **23:22:29
Malaysia time**. Rendered fishing checks pass **876 assertions** in
`Builds/FishingGameplayQA/Run-20261009-232300/`: gameplay inputs, all piers/cameras, Round/Cup/Crew states,
timer/Wanted typography, four landscape resolutions and safe-area insets.
Captured timer, Wanted and Crew views were reviewed. The clock illustration
retains 76 × 83 design units; responsive canvas scaling remains in use.
This layout refinement introduces no multiplayer message or physics change.

The complete tested candidate directory was moved into `Builds/WindowsFinal/`,
retaining all **317 runtime/support files**; the delivery note makes the complete
folder **318 files**. A post-promotion SHA-256 manifest is retained in
`runtime-manifest.json`; no before/after file-hash comparison is claimed.
The final Android source manifest covers **3,233 inputs** and matches current
Game inputs. The Windows/Android source snapshots differ in exactly
**2 verified platform/generated files**:
`Game/Assets/UniversalRenderPipelineGlobalSettings.asset`, `Game/Assets/_Game/Prefabs/Golf/Ball.prefab`.
These differences are audited in `source-verification.json`: two desktop SSAO
registrations and Golf prefab IDs/order with all 14 objects equivalent.
Glass/lamp material source bytes and gameplay code match across platforms.
Generated scene/cabin and packed Golf LOD ordering is reviewed against the
pre-task commit in `generated-output-equivalence.json`; decoded geometry,
attributes and reference graphs remain equivalent.

Fresh AndroidSubmission finished at **23:31:12**: **88,760,152 bytes**,
with **11,239,848 bytes** of strict-boundary headroom. The budget gate
and APK v2 signature check pass. The 75 MB target remains exceeded by
**13,760,152 bytes**. [Release](BUILD-SIZE.md#fishing-timer-spacing--9-october-2026),
[packing](APK-SIZE-AUDIT.md#fishing-timer-spacing--9-october-2026). Physical Android appearance, touch/FPS
and separate-network play remain unverified for this delivery; desktop checks
do not establish those results. No new phone or actual network qualification
is claimed for this layout-only refinement.

Superseded player/APK/audit output is recoverable in these reviewed ignored
archive batches, with original paths, sizes and hashes:

- `Legacy/20261009-232406-908-timer-card-before-android/`
- `Legacy/20261009-232539-244-timer-card-windows-delivery/`

Current evidence stays under `Builds/TimerCardQA/Task-20261009-231529/`. The manual Cancel height 70 was
preserved. Git publication follows the user's existing authorization to merge
and push these fishing UI refinements directly to main.

## Fishing Hook cue and readable controls — 9 October 2026

Final protocol **38** player checks pass **1,061 assertions across
8 selected reports**:

| Coverage | Assertions | Evidence |
| --- | ---: | --- |
| Rendered fishing rules, all 30 fish across five piers/three cameras, touch/keyboard, bite deadline/lifecycle, readable controls, score animation, layouts and grip | 876 | `Builds/FishingGameplayQA/Run-20261009-220744/` |
| Host/guest actual bites, hook timing, awards, reservations and rematch | 52 | `Builds/FishingGameplayQA/Run-20261009-221255/` |
| Three-peer shared sports controls, possession, maps and original input roles | 81 | `Builds/SportsHudQA/Reviews/Local-20261009-221259-529/` |
| Portable copied player, script-relative defaults from an unrelated working directory | 52 | `Builds/FishingRippleQA/Task-20261009-162733/Portable Review/Builds/FishingGameplayQA/Run-20261009-221439/` |

Rendered views confirm navy score digits, the complete blue local row through
milestones, solid red Cancel/X, native Camera/Jump symbols, a single-line
Wanted label with naked yellow/navy **+3**, orange catch awards without boxes,
plain title lettering and stronger catch/tension outlines. Decorations ignore
raycasts; pointer targets and existing callbacks remain. Camera/Jump skins
stretch with their narrower responsive frames. Lifecycle checks cover late,
expired and repeated snapshots, focus/pause, offscreen cameras, round changes,
foreign players, travel and reduced motion. No actionable runtime exceptions
were found in these final reports.

Earlier rank animation fixtures in `Run-20261009-202413/` and
`Run-20261009-203628/` observed two updates in one rendered frame. The fixture
now crosses a frame boundary before reading geometry, rank and score pulses;
the final run passes. Those failed and intermediate reports remain ignored
diagnostic evidence; no gameplay score correction is claimed for that fix.
The intermediate `Run-20261009-214457/` passes its landscape typography cases
but fails a forced portrait Wanted glyph check. Portrait is disabled by the
product's orientation settings. Final coverage uses four landscape resolutions
(844 × 390, 1024 × 768, 1920 × 810 and 1280 × 720), including safe-area insets.

**Delivery and source:** the final Windows player was fully built at
**22:07:12 Malaysia time**, with **318 complete files**, including
**317 runtime/support files** and its note. File-by-file SHA-256 comparison
confirms the tested candidate, promoted `Builds/WindowsFinal/` and portable
copy match. The source manifests cover **3,233 Game inputs**; all current
inputs match the final Android snapshot. Windows has exactly five verified
platform/generated/editor differences: two desktop SSAO runtime registrations,
regenerated Golf ball IDs/order with all **14 objects** equivalent,
GraphicsSettings and QualitySettings
select the desktop pipeline for the Windows snapshot and MobileURP for
Android; ProjectBuilder additionally contains the Android-only mobile
pipeline safeguard added after the Windows build. Windows build methods and
all gameplay code match the tested full Windows snapshot. Glass and lamp
material source bytes are also exact matches. Evidence:
`source-verification.json`, `runtime-integrity.json`,
`portable-integrity.json` and manifests under `Builds/FishingRippleQA/Task-20261009-162733/`.

Regenerated scene IDs/order and packed mesh ordering were also reviewed against
the pre-task commit: **7,411 scene/cabin objects** retain their property and
reference graphs; Golf LOD keeps exactly the same decoded positions, winding,
normals, colours and bounds across **111,302 triangles**. A raw submesh index
range changed with vertex reordering; decoded geometry and attributes are
unchanged. Format-aware proof: `generated-output-equivalence.json`.

Fresh AndroidSubmission completed at **22:36:40**: **88,758,740 bytes**,
up **16,288** from the actual pre-task APK. Strict-boundary headroom is
**11,241,260 bytes**; budget enforcement and independent APK v2 signing
pass. The 75 MB target remains exceeded by **13,758,740 bytes**.
[Release](BUILD-SIZE.md#fishing-hook-cue-and-readable-controls--9-october-2026), [packing audit](APK-SIZE-AUDIT.md#fishing-hook-cue-and-readable-controls--9-october-2026).
Physical Android appearance, touch/multitouch, sustained FPS and
separate-network play remain untested. Desktop/loopback evidence does not
establish those results.

Reviewed superseded output, discarded task drafts and duplicate validation
players are recoverable in these ignored batches, each with sizes, hashes and
original recovery paths:

- `Legacy/20261009-163904-596-hook-alert-design-refinement/`
- `Legacy/20261009-201759-878-hook-alert-before-windows/`
- `Legacy/20261009-203128-363-hook-alert-timer-refinement/`
- `Legacy/20261009-203655-886-hook-alert-before-android/`
- `Legacy/20261009-204003-839-hook-alert-review-seed/`
- `Legacy/20261009-204805-206-hook-alert-promote-tested/`
- `Legacy/20261009-210558-418-hook-alert-hud-refinement-apk/`
- `Legacy/20261009-210852-689-hook-alert-before-hud-code/`
- `Legacy/20261009-211729-105-hook-alert-final-hud-promotion/`
- `Legacy/20261009-212238-725-hook-alert-portable-refresh/`
- `Legacy/20261009-213331-261-hook-alert-typography-refinement/`
- `Legacy/20261009-214852-745-hook-alert-setup-surface-refinement/`
- `Legacy/20261009-215900-330-hook-alert-friend-surface-refinement/`
- `Legacy/20261009-221118-755-hook-alert-final-readability-delivery/`
- `Legacy/20261009-222045-463-hook-alert-portable-validation-complete/`
- `Legacy/20261009-222840-324-hook-alert-desktop-dependency-rejection/`

Current player/APK, raw validation reports and source/asset audit evidence stay
under ignored `Builds/`. Active assets, editable masters and approved design
references remain publishable. No Git history is rewritten.

## Integrated main — 9 October 2026

The combined protocol **38** player retains fishing reticle selection and
Lagoon Stickers, teammate football pace/stamina/pressure and possession maps,
shared control styling, conservative animation compression, and Golf aiming,
cycling power and terrain-aware shot travel. Protocol 35 fishing and protocol
37 teammate releases below remain **historical parallel branch deliveries**;
the following evidence comes from the newly combined complete player.

**1,504 passing assertions across 18 selected reports**:

| Coverage | Assertions | Evidence |
| --- | ---: | --- |
| Rendered fishing, 471 rules, 90 aiming combinations, all 30 fish, touch/keyboard, HUD/layout, grip handoff | 660 | `Builds/FishingGameplayQA/Run-20261009-151341/` |
| Four-sport HUD, possession, maps, touch targets and native captures | 167 | `Builds/SportsHudQA/Reviews/Offline-20261009-151640-666/` |
| Football pace, stamina, dash, pressure and rendered controls | 77 | `Builds/FootballPaceQA/Reviews/Offline-20261009-151739-841/` |
| Golf repeated power sweeps and actual shot/guide contact across five holes | 187 | `Builds/GolfShotQA/Run-20261009-152220/` |
| Golf Aim/Cancel, locked stance and touch release | 50 | `Builds/GolfAimQA/Run-20261009-153031/` |
| Golf ball/tee physical regression | 74 | `Builds/GolfBallPhysicsQA/Run-20261009-153050/` |
| Fishing pier spawns, entrances/exits, walking, shoreline and sport changes | 53 | `Builds/FishingQA-20261009-152429/` |
| Five network suites, listed below | 236 | `Builds/Publish/20261009-131113/qa-assertion-summary.json` |

The network reports cover:

- Fishing: **21 host / 21 guest**, `Builds/FishingGameplayQA/Run-20261009-151628/`.
- Football effort: **17 host / 9 guest**, `Builds/FootballPaceQA/Reviews/Local-20261009-151729-857/`.
- Three-player sports HUD: **30 host / 25 guest / 26 guest**, `Builds/SportsHudQA/Reviews/Local-20261009-151804-506/`.
- Golf aiming: **23 host / 23 guest**, `Builds/GolfAimQA/Run-20261009-151841/`.
- Golf match: **20 host / 21 guest**, `Builds/GolfMiniGameQA/Run-20261009-151918/`.

The separate five-player room run passes capacity rejection, distinct dry
spawns, host-selected Fishing, ordered exploration/return for all participants,
eight guest-permission assertions and runtime exception scanning:
`Builds/FishingRoomsQA-20261009-152844/`. This is room integration coverage,
rather than five simultaneous human fishing catches.

Eleven native HUD/football captures were reviewed at 1280 × 720. Fishing keeps
its illustrated controls and contained readable text; possession maps, action
roles and stamina cues render in the other sports without missing text or
material overlap in those views. Review summary:
`Builds/Publish/20261009-131113/ui-offline-summary.json`.

**Observed limits:** the initial Golf shot run
`Builds/GolfShotQA/Run-20261009-151622/` missed a short contact/travel observation;
Golf Aim `Builds/GolfAimQA/Run-20261009-152409/` failed its shot-distance/preview
observation. They ran under concurrent test load. Identical-player repeats
listed above pass, without a gameplay-code fix. The failures remain diagnostic
evidence and a limit on repeatability under that load. The earlier room test
`Builds/FishingRoomsQA-20261009-152516/` compared unsynchronized process clocks;
its runner now checks each participant's state order, and the final room run
passes. No game-network fix is claimed for that harness correction.

**Delivery and source:** full Windows assets/scripts built at **15:12:50**
Malaysia time; **318 files** in `Builds/WindowsFinal/`, including **317 runtime/
support files** and `LATEST-BUILD.txt`. The production manifests cover **3,231
Game inputs**. Current source matches all **3,231 Android snapshot inputs**.
Windows has exactly four format-aware verified differences: URP adds only two
desktop SSAO runtime registrations; Sky-Sail glass uses `_SrcBlend=5` on Windows
and `_SrcBlend=1` with premultiplied transparency on Android; the lamp retains
the same colour while Windows adds `_EMISSION` and Android normalizes its keyword
list; generated Golf ball IDs/order differ while all **14 objects** retain their
canonical properties/reference topology. No code, geometry or other input
differs. These are explicit platform/generated exceptions; the raw visual source
snapshots are not byte-identical. Evidence: `source-verification.json`,
`golf-generated-equivalence.json` and the manifests under
`Builds/Publish/20261009-131113/`. Desktop captures do not establish phone quality.

The complete **318-file** Windows delivery was also copied to a portable path
with spaces and verified file-by-file with SHA-256. From an unrelated working
directory, the copied fishing test resolved its own project/player defaults and
passed **21 host / 21 guest** assertions. This additional portable run is
separate from the 1,504 selected assertions above. Evidence:
`Builds/Publish/20261009-131113/delivery-verification.json`, `test-portable.txt`
and `Portable Review/Builds/FishingGameplayQA/Run-20261009-154217/`.

Fresh AndroidSubmission completed at **15:25:34**: **88,742,452 bytes**, down
**1,308,602** from the local pre-merge APK; strict-boundary headroom is
**11,257,548 bytes**. APK v2 signing and the hard budget gate pass. The 75 MB
development target remains exceeded by **13,742,452 bytes**.
[Size and hash](BUILD-SIZE.md#integrated-main--9-october-2026),
[packing audit](APK-SIZE-AUDIT.md#integrated-main--9-october-2026).
Physical Android launch, touch/multitouch, sustained FPS and separate-network
play remain untested in this task.

Reviewed superseded complete Windows and Android output is recoverable in:

- `Legacy/20261009-131943-199-publish-main-before-windows/`
- `Legacy/20261009-151219-684-publish-main-before-android/`

Current validation evidence stays under `Builds/Publish/20261009-131113/`
and the linked run folders. Active source, assets, metadata and editable masters
remain available; builds and archives remain ignored.

## Fishing aim and readability — 9 October 2026

**Historical local fishing branch build (protocol 35), before main integration.**

The [fishing controls](FISHING-MINIGAME.md) now use a centre reticle and Cast
confirmation, with six fish per pier (two of each size, 30 across five piers).
All three cameras aim at visible fish bodies; terrain obstruction, range,
claims and respawns remain enforced. The selected target stays fixed while a
Cast request awaits the host. Matching peers require protocol **35**.

Native HUD text no longer combines an extra bold style with stacked shadows
and outlines. Smaller labels use the existing text font; white lettering has
one restrained outline. No font or raster asset is added. Responsive landscape
layouts and asymmetric safe areas are covered by captures and assertions.

Reeling speed is **1.48 / 1.05 / 0.73 m/s** for small/medium/large fish; held
strain grows 4% more slowly and the bite wait is 0.3 seconds shorter. At an 8 m
cast, the deterministic controlled comparison measures:

| Fish | Before, cast to land | After | Difference |
| --- | ---: | ---: | ---: |
| Small | 7.72 s | 6.56 s | −1.16 s |
| Medium | 12.45 s | 10.99 s | −1.46 s |
| Large | 20.16 s | 19.18 s | −0.98 s |

Timing varies with range, surges and release decisions. The **471 rule checks**
include 120 controlled catches and 40 continuously held large-fish cases that
still snap the line; this preserves the need to manage tension. Comparison
evidence is in `Builds/FishingAimQA/reeling-evidence.json` and its CSV records.

The complete **314-file** Windows player in `Builds/WindowsFinal/` uses full
assets built at **10:26:10** and final scripts at **10:38:43 Malaysia time**.
All **313 runtime/support files** match both the tested candidate and portable
copy; only `LATEST-BUILD.txt` is excluded from hashing.

- Final rendered fishing: **660 passing offline assertions**, including the
  471 rule checks, all **90 pier/camera/fish aiming combinations**, all 30 fish
  swimming/containment, native typography, camera obstruction, claimed and
  respawning targets, pending-Cast ownership, asymmetric safe-area aiming,
  keyboard/touch actions, completed catches, animated ranks/Wanted/tension,
  layout checks and Fishing/Golf/Fishing grip handoff.
  Evidence: `Builds/FishingGameplayQA/Run-20261009-104400/`.
- Two-player fishing: **21 host / 21 guest assertions** in
  `Builds/FishingGameplayQA/Run-20261009-103459/`; the subsequent change only
  adjusts leaderboard name spacing. The final complete player also passes
  **21 host / 21 guest** in a folder with spaces using the default runner,
  launched from an unrelated working directory:
  `Builds/FishingAimQA/Portable Review/Builds/FishingGameplayQA/Run-20261009-105114/`.
- Walking/controller regression: **53 assertions**, covering five pier
  spawns, entrances/exits, containment, the bridge/coastal circuit and four
  sport switches. Evidence: `Builds/FishingQA-20261009-104034/`.
- Five-player local room: supported spawns, host-selected Fishing, sixth-player
  rejection, shared start/return and guest permissions pass.
  Evidence: `Builds/FishingRoomsQA-20261009-104742/`. This checks room
  integration, rather than five simultaneous fishing catches on five devices.

An initial final-player run made concurrently with walking/capture checks
failed the transient `RANK_REORDER_AND_SCORE_POP` animation assertion:
`Builds/FishingGameplayQA/Run-20261009-104039/`. The identical final binary
passed all 660 assertions when run alone at 10:44. Both records are retained;
the repeat does not establish a gameplay fix for the concurrent timing issue.
Phone-sized frames and five-row ranks include identified UI fixtures, while
selected-fish and completed-catch captures use actual gameplay. A portrait
Windows resize remains a stress check; Android stays landscape only.

Source manifests cover **3,135 inputs including metadata**. Final Windows
scripts, Android and restored publishable source differ only in generated Golf
prefab IDs/order; canonical inspection proves all **14 objects** retain their
properties and reference topology. The original pre-task serialization is
restored. Full Windows assets predate the final HUD/input/probe code edits,
which are included in the final script compilation. Their desktop SSAO runtime
resource list differs from Android by exactly two reconstructed resource IDs;
authored settings match HEAD/Android. Source and complete-delivery proofs are
in `Builds/FishingAimQA/source-verification.json` and
`delivery-verification.json`. No later gameplay change is omitted.

Fresh AndroidSubmission: **90,047,202 → 90,051,054 bytes (+3,852)**, leaving
**9,948,946 bytes** to the hard boundary. Budget and independent v2 signature
checks pass; the **75 MB development target is exceeded by 15,051,054 bytes**.
[Measured release/hash](BUILD-SIZE.md#fishing-aim-and-readability--9-october-2026),
[packing audit](APK-SIZE-AUDIT.md#fishing-aim-and-readability--9-october-2026).
Physical Android appearance, actual phone touch/FPS, internet latency and human
balance/retention remain unverified.

Reviewed obsolete trials, the prior APK/audit and the prior complete player
are archived with recovery manifests in:
`Legacy/20261009-102009-186-fishing-tuning-trials/`,
`Legacy/20261009-105020-349-fishing-aim-before-android/` and
`Legacy/20261009-110826-067-fishing-aim-final-player/`.
Current validation evidence remains under ignored `Builds/`. Publishing is left
to the user; unrelated workspace changes are preserved.
The final readiness dry run initially encountered an empty, stale Git lock.
No Git process was running and exclusive access succeeded; the lock is preserved
with a recovery record in `Builds/FishingAimQA/lock-recovery.json`. The Git index
hash is unchanged. Portable document links also pass from the folder with spaces.

## Lagoon Stickers fishing — 9 October 2026

The [approved fishing rules](FISHING-RULES-PROPOSAL.md) and
[Lagoon Stickers direction](FISHING-UX-DIRECTION.md) are implemented. Competitive
Round/Cup use three-minute scoring; Crew Catch needs 20 × starting players,
all three fish sizes and a catch from everyone. Wanted awards +3 once per player
per minute. Host authority, explicit chosen fish, ready/countdown, ties,
disconnects, rematches and round/cast tokens are covered; matching peers need
protocol **34**. [Controls and physical limits](FISHING-MINIGAME.md).

The complete **314-file** Windows player in `Builds/WindowsFinal/` uses full
assets built at **01:25:59** and final scripts compiled at **01:37:24** Malaysia
time. All **313 runtime/support files** match the tested candidate and portable
copy; only `LATEST-BUILD.txt` is excluded from hash comparison.

- Current rendered fishing: **147 offline report assertions**, including rule
  boundaries, all sizes/ranges, real touch/button and keyboard action paths,
  supported piers, swimming, hand grip, three cameras, reflection/light,
  selection after menu return, Wanted confirmation/preview, live score digits,
  animated overtakes, tension gauges, safe-area/layout checks and Golf handoff.
  Evidence: `Builds/FishingGameplayQA/Run-20261009-013910/`.
- Current two-player fishing: **21 host / 21 guest report assertions**, covering
  ready/start, chosen-target validation, Wanted catch scoring, overtakes,
  replicated deadlines/results and clean rematch, in the same run.
- Portable complete player/default runner, in a folder with spaces and launched
  from an unrelated working directory: **21 host / 21 guest report assertions**.
  Evidence: `Builds/FishingUXQA/Portable Review/Builds/FishingGameplayQA/Run-20261009-014416/`.
- Island/controller walking: **53 assertions**, including all five supported
  spawns, deck entrances/exits, containment, bridge/coastal circuit, accents
  and four sport switches. Evidence: `Builds/FishingQA-20261009-012648/`.
- Five-player local room: distinct supported spawns, host-selected Fishing,
  sixth-player rejection, shared start/return and guest permissions pass.
  Evidence: `Builds/FishingRoomsQA-20261009-013917/`. This is room integration,
  rather than five simultaneous networked fishing catches.
- Golf regression passed **135 rendered offline / 20 host / 20 guest** assertions
  on the earlier candidate in `Builds/GolfMiniGameQA/Run-20261009-004355/`.
  Subsequent changes affect fishing UI, menu text/layout and development probes;
  Golf gameplay/assets are unchanged. Final-player Fishing/Golf/Fishing grip
  and first-person handoff checks also pass.

Real captures include the stopwatch/Wanted card, held rod and completed catches.
The five-row leaderboard, extreme tension, phone-sized frames and Crew checklist
also use explicitly identified development fixtures. These demonstrate UI
layout/animation, not a five-device fishing session. Android is landscape only;
a portrait Windows resize is a stress check, not an enabled mobile orientation.
The UI is anchored inside the safe-area frame with Inspector-editable layout
settings. Six transparent sprites have verified 64/128/256-pixel delivery limits.

Initial checks exposed an arrival-readiness race in the probe, an obsolete menu
label expectation, a fixed floor-height assumption and a scenic route crossing
the later Sky-Sail gangway railing. Readiness now waits for gameplay control;
the button stays disabled during arrival. Walking tests validate the actual
paved floor and continuously walk the shoreward bypass around the railing.
The original obstruction also reproduced with lagoon art disabled; no collider
or island geometry was removed. A low-angle test target was hidden by the deck;
the aiming check now tilts the look camera toward the water and waits for the
actual visible fish. Terrain obstruction checks remain active. Newly inspected
help/rules text is current and clipped menu headings were corrected.

Source manifests cover **3,135 inputs including metadata**. Final Windows script
source and Android match on **3,134**; the only raw difference is regenerated
Golf prefab IDs/order. Canonical inspection proves all **14 objects** preserve
properties/reference topology; the equivalent original serialization was
restored, making the publishable source match all final Windows script inputs.
The full Windows asset snapshot differs from final scripts in the later menu
and development-probe edits, plus URP's stripped runtime list. The exact full
Windows URP hash is reconstructed by adding only the two desktop SSAO resource
IDs; all authored settings match Android/HEAD. Artwork PNGs retain alpha and
contain no ancillary metadata/dependency paths. Records are under
`Builds/FishingUXQA/`, including source/delivery verification and signature proof.

Fresh AndroidSubmission: **89,938,934 → 90,047,202 bytes (+108,268)**, with
**9,952,798 bytes** strict-limit headroom. Budget and independent v2 signature
checks pass. The **75 MB development target is exceeded by 15,047,202 bytes**.
[Measured release/hash](BUILD-SIZE.md#lagoon-stickers-fishing--9-october-2026),
[packing audit](APK-SIZE-AUDIT.md#lagoon-stickers-fishing--9-october-2026).
Physical Android appearance, actual phone touch/FPS, internet latency and
human retention/balance remain unverified.

Reviewed obsolete files are archived with size/hash recovery inventories:
`Legacy/20261009-003423-261-fishing-ui-portrait/` (unused portrait experiment),
`Legacy/20261009-010106-563-fishing-lagoon-stickers/` (older player, discarded
concepts and superseded runs), `Legacy/20261009-014243-715-fishing-final-player/`
(superseded player copies) and `Legacy/20261009-015307-440-fishing-intermediate-apk/`.
The approved reference, editable art, active generators/metadata and current
validation evidence are preserved. No Git staging, commit or push was performed.

## Golf cycling power and accurate shot guide — 9 October 2026

**Historical teammate branch build (protocol 37), before fishing integration.**

Delivered automatic Putt/Swing modes, repeated power sweeps and one accurate
landing/stopping guide. Aim/Cancel, stationary stance, scoring and four-island
travel remain intact. Near the ball owner's current cup (12 horizontal metres)
the button reads Putt; farther away it reads Swing. Only the button names the
mode. Releasing freezes the last displayed power and heading. A continuous
mint/ivory ribbon and pulsing gold target replace the dashed rollout and readout.

The same swept trajectory drives the preview and authoritative shot. Putts
follow turf and stop at the glow; swings end their guide at first touchdown and
then roll physically. First turf impact retains 78% of tangential speed. A new
ball impact or gameplay impulse interrupts planned travel and restores ordinary
Rigidbody response. Future interference can change the outcome. No island art,
terrain, model, texture or audio changes were made in this iteration.

**1,137 passing assertions in 14 selected reports**, recorded in
`Builds/GolfShotQA/selected-checks.json`:

| Coverage | Evidence |
| --- | --- |
| All five holes, two modes, rendered path vs live ball, cycling/descending release, distance boundary, collision/impulse/reset, actual putt into cup | `Builds/GolfShotQA/Run-20261009-012252/` |
| Final portable player, path with spaces, unrelated working directory | `Builds/GolfAimQA/Run-20261009-012254/` |
| Final mobile geometry, rendered Putt/Swing captures | `Builds/GolfShotQA/Run-20261009-011310/` |
| Offline Aim, host/client mode replication and actual putt stopping target | `Builds/GolfAimQA/Run-20261009-012254/` |
| Close stance, touch release, club contact and two-hand grip | `Builds/GolfSwingQA/Run-20261009-005547/` |
| Match scoring, ownership, cups, deadlines, host/client | `Builds/GolfMiniGameQA/Run-20261009-005549/` |
| Rendered ball/tee, physical slopes, rest, forces and launch | `Builds/GolfBallPhysicsQA/Run-20261009-010437/` |
| All-sport HUD roles and touch targets | `Builds/SportsHudQA/Reviews/Offline-20261009-012513-195/` |
| Surface resistance and actual full shots across all five holes | `Builds/GolfPlayabilityQA/Run-20261009-005408/` |
| Retained golf equipment and fishing rod review tools | `Builds/GolfShotQA/equipment-desktop-final/`, `rods-desktop-final/` |

The selected shot cases report path/contact error rounded to **0.00000 m**;
assertion bounds are **0.08 m for the live path** and **0.02 m at contact**.
Host/client putts also report **0.00000 m** final error (0.08 m assertion bound).
Full-charge shots travel **37.88–62.07 m** across the five authored tees and
settle in **4.22–6.58 seconds**, without recovery. Mobile terrain validation
checks **13,122** unchanged base samples (maximum error **1.78e−15 m**).
Windows captures do not establish phone FPS, visuals or touch behavior.

Earlier iterations exposed a test entering aim before the menu transition,
an obsolete charge-dependent putt expectation, excessive full-shot run-out,
and a retained equipment review checking the old ball radius. The test waits
for control, checks distance-based modes, measures the actual shot path, and
uses the current radius; landing slowdown bounds run-out. Failed iteration
logs remain diagnostics, not final validation.

**Delivery:** full Windows assets and scripts **01:22:22**, final
AndroidSubmission **01:14:05**, all on 9 October 2026 Malaysia time. The complete
`Builds/WindowsFinal/` player has **317 files**, including **316 matching runtime/
support hashes**. Current equipment/rod review launchers are included, and the
builder now regenerates them. Unity's explicitly marked non-runtime Burst text
disassembly is archived. Desktop/mobile game assemblies match. Protocol **37**.
Fresh APK **88,560,698 bytes**, **11,439,302 bytes** of strict-limit
headroom; the 75 MB development target is still exceeded. Signature v2 passes;
no Android device attached. [Size details](BUILD-SIZE.md#golf-cycling-power-and-accurate-shot-guide--9-october-2026).

Final source hashes match **all 3,186 inputs** captured before the complete
final desktop build. All Android production inputs are unchanged; the only
later source difference is removal of a temporary editor-only scripts-build
helper. A package-size audit caught that scripts-only builds can reuse the last
mobile asset cache. The provisional player was archived and replaced with the
fresh full desktop build; no scripts-only update followed it. Final desktop
runtime/support files total **593,742,855 bytes** (not an APK measurement).

The generated URP runtime-registration list is restored to its pre-build source.
A format-aware YAML check confirms all 14 ball-prefab objects retain properties/
reference topology; original generated IDs/order restored. Source and delivery
proof is in `source-verification.json`, `prefab-equivalence.json`,
`runtime-manifest.json` and `delivery-verification.json`.

Cleanup moved eight reviewed obsolete development players (**4,764,942,952 bytes**),
superseded deliveries/APKs, task player copies and source backups to recovery
batches below. Active source, editable masters, shared materials, needed Unity
metadata, current captures/reports and installed caches remain. The old golf
readout and obsolete blended charge setting were removed in place; no new art
asset was made obsolete. Publishable candidates contain no builds or archives.

- `Legacy/20261009-005752-310-golf-shot-obsolete-players/`
- `Legacy/20261009-005852-461-golf-shot-old-mobile/`
- `Legacy/20261009-010100-561-golf-shot-old-android/`
- `Legacy/20261009-010510-771-golf-shot-debug-sidecars/`
- `Legacy/20261009-011209-356-golf-shot-validation-apk/`
- `Legacy/20261009-011438-598-golf-shot-old-windows/`
- `Legacy/20261009-011848-903-golf-shot-task-copies/`
- `Legacy/20261009-012641-109-golf-shot-replaced-cache-player/`
- `Legacy/20261009-012858-139-golf-shot-final-player-copies/`

`Check-TaskReady.ps1 -RequireApk` checks the final publishable candidate set.
Git staging, commit, push and history remain untouched. After staging, the user
should run `Tools/Build/Check-PortablePaths.ps1 -Staged` before committing.

## Golf aiming lock and shot guide — 8 October 2026

Delivered a dedicated golf aiming interaction and procedural shot guide:

- Aim within 3 m of a visible, nearly stationary ball. The host validates safe
  ground, stance clearance, travel path and other players before placing feet.
- Movement, sprint, jump and cart input cannot break the lock. Direction and
  charge remain adjustable. Cancel / X or Escape cancels without a stroke;
  release swings once. Another nearby ball cannot change the selected ball.
- Ivory flight arc, dashed gold rollout estimate and endpoint rings update with
  the same launch profile as the shot. The readout labels distance as estimated.
- Shot camera supports all roaming camera modes, with a clear side-on stance.
  Ball reset/movement, focus/menu changes, travel and round cleanup release locks.
- Terrain, original art, existing physics tuning and four-island travel remain.

**811 passing assertions across 12 selected reports**, plus **13,122 independent
base-terrain samples** (maximum error 1.78e−15 m). The complete selection and
counts are in `Builds/GolfAimQA/selected-checks.json`:

| Coverage | Current evidence |
| --- | --- |
| Desktop Aim, input lock, cancel, release, host/client | `Builds/GolfAimQA/Run-20261008-211859/` |
| Close approaches, club contact and two-hand grips | `Builds/GolfSwingQA/Run-20261008-211859/` |
| Offline scoring, cups, host/client ownership and deadline | `Builds/GolfMiniGameQA/Run-20261008-212004/` |
| Rendered ball visibility, slopes, rest, impulses and launch | `Builds/GolfBallPhysicsQA/Run-20261008-212004/` |
| All-sport HUD roles, touch targets and control styles | `Builds/SportsHudQA/Reviews/Offline-20261008-211900-034/` |
| Actual course charge distances and directional putts | `Builds/GolfPlayabilityQA/Run-20261008-212817/` |
| Rendered mobile geometry Aim and putt/chip captures | `Builds/GolfAimQA/Run-20261008-212317/` |
| Portable path with spaces, launched from unrelated working directory | `Builds/GolfAimQA/Run-20261008-212553/` |

The first iterations exposed a disabled-camera HUD null reference and an
insufficient contact margin in the initial stance. The final code guards the
inactive view and uses a safe 0.85 m stance; both regressions pass. Earlier failed
logs and iteration captures remain diagnostic evidence, not delivery validation.

**Delivery:** full Windows build at **21:17:53 Malaysia time**, all 315 runtime
and support files verified against both the tested candidate and portable copy,
then promoted together to `Builds/WindowsFinal/`. The 316th file is the updated
`LATEST-BUILD.txt`. AndroidSubmission completed **21:24:55** and independently
verified APK v2 signing. Matching protocol **36** peers are required.

**APK:** **88,531,618 → 88,554,742 bytes (+23,124)**; hard-limit headroom
**11,445,258 bytes**. The 75 MB development target remains exceeded by
**13,554,742 bytes**. [Hash and release](BUILD-SIZE.md#golf-aiming-lock-and-shot-guide--8-october-2026),
[packing analysis](APK-SIZE-AUDIT.md#golf-aiming-lock-and-shot-guide--8-october-2026).

**Source evidence:** manifests were recorded before Windows, before mobile
preview, before Android and after Android/final restoration. Pre-Windows/final
match **3,180 of 3,182 inputs**. Differences are generated URP global settings
(final matches the repository baseline) and generated golf-ball prefab object
IDs/order. A format-aware comparison proves all 14 prefab objects retain their
properties and reference topology across baseline, Windows, mobile and Android;
original IDs were restored. All runtime source hashes are unchanged after the
validated Windows build. Desktop/mobile-preview `Assembly-CSharp.dll` hashes
match: `819EE36C31E59FC44D32DF8645230235C5BF4CE662F282442488CA5862B14451`.
Evidence: `source-verification.json`, `prefab-equivalence.json`,
`delivery-verification.json` and `runtime-manifest.json` under `Builds/GolfAimQA/`.

**Cleanup:** reviewed obsolete output was archived with hashes and recovery paths:

- `Legacy/20261008-212035-784-golf-aim-old-mobile/` — previous mobile preview.
- `Legacy/20261008-212300-046-golf-aim-android/` — previous signed APK/output.
- `Legacy/20261008-212908-471-golf-aim-windows/` — previous complete Windows player.
- `Legacy/20261008-213022-394-golf-aim-player-copies/` — verified redundant candidate/portable players.

Existing editable masters and active source assets remain; this change makes no
old source asset obsolete. No staging, commit, push, reset or history rewrite.
The repository readiness check and portable text scan pass. No physical Android
device was attached; phone FPS, touch and visual quality remain unverified.
The rollout guide is a terrain-aware estimate, not an exact collision simulation.

## Golf playability and integrated greens — 8 October 2026

Delivered the complete **316-file** Windows player in `Builds/WindowsFinal/`.
Full build completed at **19:40:04 Malaysia time**; every one of its **315
runtime/support files** matches the validated candidate and portable copy.
The portable copy passed the golf playability suite from a path containing spaces
while invoked from an unrelated working directory. Launchers and runtime folders
are retained together. Matching **protocol 35** peers are required.

The final change applies surface-dependent resistance to every supported slope,
keeps airborne motion free of ground resistance, synchronizes rolling spin, and
reserves the first 45% of charge for 0.65–4.5 m/s putts. Maximum shot speed is
17 m/s with 5.2 m/s vertical lift. Scoring, permanent ball ownership, countdown,
cart behaviour and group island travel remain intact.

Five small greens are shaped inside the existing terrain, with the original
material and UVs. Inner collider slopes measure **0.32–0.53 degrees**; sampled
height changes at the greens stay below **0.25 m**. The near-cup plane transitions
smoothly into the surrounding contours. Outer samples match the original terrain
within **0.05 mm**. Hole 4 moves from (107, -80) to (111, -70); Hole 5 moves from
(-6, 146) to (-10, 126), in island-local X/Z metres. The original cup locations
are restored when generation starts from the FBX master. No duplicate ground
surface, new terrain collider, texture or imported art is added. Desktop and mobile
grass use the same green shapes and restrained irregular clearing.

Selected evidence contains **1,594 PASS assertions across 15 reports**, plus
**13,122** unchanged analytic base-terrain samples. It covers scene/prefab/repeat
and saved-scene geometry, actual ball visibility/flight/collision/rest, all five
tees at three charges, twenty directional putts, four surface types, offline and
host/client scoring, carts, shoreline containment, fairway/bunker traversal,
mobile geometry, and portable delivery. The exact report list is in
`Builds/GolfPlayabilityQA/selected-checks.json`.

On the actual course, 3 m/s putts travelled **3.18–3.46 m**, settled in
**2.22–2.36 s**, and stayed still. Full shots from Holes 1–5 respectively travelled
**64.06 / 36.36 / 43.19 / 37.09 / 37.37 m**, settling in **4.26–6.84 s**.
Greater charge produced greater distance at every tee; none of these shots needed
forced recovery. These are measured trials, not universal distance guarantees.
A full shot deliberately overhits the short opening hole.

Close views of all five greens and distant overviews were inspected in Unity;
mobile grass and course views were inspected in the Android-geometry Windows
player. Captures are in `Builds/GolfPlayabilityQA/Map/` and
`Builds/GolfG2QA-20261008-194522/`. No physical Android device was attached;
phone touch, appearance, FPS and thermals are unverified.

Fresh AndroidSubmission completed at **19:48:25** and passes the hard budget
and independent v2 signature check: **88,531,618 bytes**, **+1,680 bytes**,
**11,468,382 bytes** of hard-limit headroom. The 75 MB target remains exceeded.
[Size and packing](BUILD-SIZE.md#golf-playability-and-integrated-greens--8-october-2026).

Source manifests were captured after the desktop build/mobile preparation and
before Android, rather than before the first desktop import. Final/pre-Android
hashes match **3,166 of 3,170 inputs**. Differences are generated URP settings,
ball-material serialization, ball-prefab identifiers/order, and restoration of
legacy Ultra MSAA after URP synchronized it from the unchanged pipeline. All 14
ball-prefab objects preserve their properties and reference topology. Desktop and
mobile-preview runtime assembly hashes match exactly. No gameplay code changed
after the final builds. These source-snapshot limits are recorded in
`Builds/GolfPlayabilityQA/source-verification.json` and `LATEST-BUILD.txt`.

Earlier diagnostic runs are retained: the known headless match pose check sampled
before LateUpdate; the rendered run passed all 135 offline assertions. A D3D12
island wrapper reported failure after its assertions completed; the final D3D11
rendered mobile and headless island runs passed. The island test now explicitly
uses D3D11, consistent with the other rendered review tools.

Cleanup found no unreferenced generated course terrain/grass assets; active art
and editable masters remain in Git. Reviewed superseded output and redundant
player copies were archived with sizes, SHA-256 and recovery paths in:

- `Legacy/20261008-194208-872-golf-playability-old-preview/`
- `Legacy/20261008-194618-455-golf-playability-android/`
- `Legacy/20261008-195623-742-golf-playability-windows/`
- `Legacy/20261008-195716-953-golf-playability-player-copies/`

Current evidence stays in ignored `Builds/`. No staging, commit, push, reset or
history changes were performed. Readiness checks include the actual APK budget;
they do not substitute for device testing.

## Simple maps, round controls and resource cleanup — 8 October 2026

The new plain HUD and resource cleanup pass **1,801 assertions in
21 selected reports**. The exact index is
`Builds/SimpleHudQA/selected-checks.json`.

- Rendered HUD at 1280×720, 1600×720 and 1024×768: all four sports, small map
  bounds, text-only circular controls, no button overlaps, visible press/release
  feedback, fixed hit targets and pointer ownership. Reviewed football,
  basketball, golf, fishing and station captures.
- Three basketball peers: carrier, teammate, opponent and loose-ball roles;
  team colours, alternating join teams and cancellation on possession changes.
- Football motion/source-clip comparison, networking, stamina/pressure,
  match authority, ball physics, kick charge/cancel and interaction regression.
- Network basketball defense and loft/bounce pass regressions.
- Rendered golf match: **135 passes**, including first-person club/head pose,
  retained materials, grip, swing and ball behavior. An initial headless run
  sampled the pose before LateUpdate and failed that visibility assertion;
  the rendered rerun passes. Both records are preserved.
- Complete 315-file runtime/support set matches a portable copy, which passed
  the HUD suite when launched from an unrelated directory and a path with spaces.
- Seven compressed clips compare all 39 transforms at 120 Hz against original
  uncompressed imports. Timing and loop flags match; largest position difference
  is **0.714 mm**. Runtime motion, turns, tackles and falls pass.

Full Windows assets finished at **15:04:47**, final scripts at **15:08:22**.
The recorded Windows and pre-Android snapshots agree on **3,113 retained input
hashes**. The Windows snapshot was taken after its build and removal of the
unused material folder's metadata; that removal does not affect runtime assets.
Android's build
processor subsequently regenerated 13 mobile material assets and Golf ball
prefab identifiers. Those mobile variants belong to the Android rendering path;
Windows uses the desktop materials. Golf's **14 objects** have equivalent
properties and reference topology despite changed IDs/order. Original prefab
IDs were restored after format-aware comparison. No gameplay edit followed the
final APK. Source manifests and comparisons are in `Builds/SimpleHudQA/`.

Fresh AndroidSubmission: **88,529,938 bytes**, verified APK v2 signature,
**11,470,062 bytes** hard-limit headroom. [Measurements](BUILD-SIZE.md#simple-maps-round-controls-and-resource-cleanup--8-october-2026).
The 75 MB development target remains unmet. No physical phone was attached;
phone rendering, touch, FPS/thermals and WAN latency are unverified. Protocol **34**.

Delivery and archive details are recorded in `Builds/WindowsFinal/LATEST-BUILD.txt`
and `Builds/SimpleHudQA/delivery-verification.json`. The repository is left
unstaged; no commit, push, reset or history rewrite is performed.

The complete **316-file** Windows delivery is now in `Builds/WindowsFinal/`;
all **315 runtime/support files** match both tested copies. Current evidence is
retained under `Builds/`. Recovery batches:

- `Legacy/20261008-150142-537-simple-hud-unused-material/` and
  `Legacy/20261008-150842-655-simple-hud-empty-folder/`: unused imported material
  and its empty folder/metadata.
- `Legacy/20261008-151201-889-simple-hud-android/`: previous APK and size audit.
- `Legacy/20261008-152539-903-simple-hud-windows-delivery/`: complete previous
  Windows player, replaced after the user closed it. The earlier locked attempt
  at `Legacy/20261008-152013-477-simple-hud-windows/` retained an inventory only;
  it moved no files.
- `Legacy/20261008-152502-293-simple-hud-qa-cleanup/` and
  `Legacy/20261008-152653-895-simple-hud-player-copies/`: superseded captures,
  editing helper/log and duplicate tested player copies.

## Team controls, pocket maps and Cove buttons — 8 October 2026

Delivered the complete Windows player and a freshly built AndroidSubmission.
[Behavior and controls](SPORTS-HUD.md). Windows full assets finished at
**13:50:48**, final scripts at **14:09:56** Malaysia time. All **315 runtime and
support files** match the validated candidate and its portable copy; the
316th file is the delivery note.

Selected results: **1,005 PASS, zero selected failures, 19 reports**.
The exact report list is `Builds/SportsHudQA/selected-checks.json`.

- Final HUD checks at **1280×720**, **1600×720**, and **1024×768** cover all four
  sports, a station card, role swaps, same-frame labels/eligibility, map bounds,
  all active players, disabled controls and cancellation of a held touch on
  turnover. Captures were visually reviewed. Safe-edge anchoring keeps controls
  at the sides on wider phones and labels at the top on taller tablets.
- A three-peer basketball review covers the carrier, same-team noncarrier,
  opponent and loose-ball views. Teammates see offensive slots but cannot
  shoot, steal or block another teammate's ball. Local-relative map colours
  use the same squad relation as the rules.
- Football match/offline/network/physics regressions: **260 + 16 + 15 + 199 +
  26 + 17** assertions. Charge/cancel gestures, two-finger isolation, overtime,
  attached possession, speeds, field boundaries and replicated ball state pass.
  The overtime fixture now explicitly acquires the ball before beginning a
  charge, matching the new possession requirement.
- Football effort offline and host/guest checks retain faster movement, stamina,
  dash, pressure, input expiry, teammate rejection and travel/reset cancellation.
- Basketball host/guest defense and passing regressions pass.
- The final Windows player also passed from a complete copy in a path containing
  spaces, launched through the review script from an unrelated working directory.
  Hashes confirm that the candidate, portable copy and delivered player agree.

The broad gameplay regressions ran before the final change to **AppRoot HUD
anchoring only**. The final three-aspect HUD checks, portable review and
three-peer basketball UI review ran after that change. This source limit is
recorded in `regression-source-delta.json`; it does not change gameplay rules.
No gameplay code changed after Android compilation. Windows/Android manifests
match on **3,111/3,112 inputs**, including metadata. The only difference is the
generated Golf ball prefab: all **14 objects** have equivalent properties and
reference topology after remapping IDs. Original identifiers were restored
after verification, preventing an unrelated generated diff.

The fresh signed APK at **14:12:47** is **89,894,276 bytes**, up **15,648 bytes**
from **89,878,628**; hard-limit headroom is **10,105,724 bytes**. The **75 MB**
development target is still exceeded by **14,894,276 bytes**. APK v2 signature
verification and the AndroidSubmission budget gate pass. See
[measured size](BUILD-SIZE.md#team-controls-pocket-maps-and-cove-buttons--8-october-2026) and
[packing audit](APK-SIZE-AUDIT.md#team-controls-pocket-maps-and-cove-buttons--8-october-2026).

Current evidence is retained in ignored `Builds/SportsHudQA/`, with referenced
football/basketball review reports in their existing ignored QA folders.
Superseded output and temporary edit scripts were archived with explicit lists:

- `Legacy/20261008-141028-211-sports-hud-android/` — previous APK, packing audit and build log.
- `Legacy/20261008-141221-288-sports-hud-windows/` — previous complete Windows player.
- `Legacy/20261008-141654-902-sports-hud-cleanup/` — duplicate tested players, superseded HUD captures and completed edit helpers.

No physical Android device was attached. Phone touch, appearance, FPS/thermals,
and WAN latency remain unverified. No Git staging, commit, push, reset or history
rewrite was performed.

## Football pace, stamina and pressure — 8 October 2026

[Football controls and rules](FOOTBALL-PACE-AND-PRESSURE.md) now include 150%
walking/running, 140% ball launches, host-owned stamina and dash, and directional
pressure against opposing carriers. The complete **316-file** Windows player
is delivered in `Builds/WindowsFinal/`; full assets completed **12:43:12** and
final managed scripts **12:57:31 Malaysia time**. All **315 runtime/support
files** match the candidate and portable copy, including launchers and the
existing basketball review fixture. The build note is the remaining file.

Selected evidence has **1,348 passes across 14 reports, zero failures**:

| Check | Passing assertions |
| --- | ---: |
| New effort rules, real movement/ball physics, touch holds and visible HUD | 77 |
| Complete portable player, default script path, spaces and unrelated working directory | 77 |
| Host/guest dash, stamina replication, pressure, teammate immunity and expired input | 25 |
| Football rig, fast gait, dash dribble, floor clearance, recovery and both LODs at 30/60 FPS | 606 |
| Offline and network match rules | 290 |
| Football physics, kicks, possession, boundaries and nets | 199 |
| Final host/guest football ball replication | 43 |
| Basketball shared passing regression | 31 |

Reports are indexed in `Builds/FootballPaceQA/selected-checks.json`.
Feature tuning and older test-expectation failures are retained as iteration
evidence; the final network ball run updates the old speed expectations.
The final assembly changes only those development-only assertions after the
passing motion build. No gameplay code changed after APK packaging.

The effort rules test 20/30/60/120 Hz drain and recovery, stationary holds,
exhaustion/release gating, charging cancellation and pressure precedence.
Runtime checks measure walking at **6 m/s**, running at **10.5 m/s**, dash at
**15.225 m/s** and dash dribbling at **11.419 m/s**. Partial/full kicks are
checked at their new speeds. Pressure tests cover facing, distance, elevation,
world obstruction, sideways escape, retreat and non-stacking containment.
Host/guest checks use the actual command RPC and replicated state, including
an intentionally silent guest. Pointer identity, pointer exit, menu/reset and
travel cancellation are checked.

Rendered captures cover third person, first person and stadium cameras at
16:9, 20:9 and 4:3. Overhead bars clear the head and separate in crowded views;
first person retains the local meter. Final 30/60 FPS motion checks retain
bone lengths, bounded reach and floor clearance, without relaxing existing
joint-speed or contact-error thresholds. These are Windows checks, not phone
FPS claims. ADB listed no connected physical device. Android touch, appearance,
performance/thermals and WAN latency remain unverified.

Fresh signed AndroidSubmission completed **13:01:42**. The measured APK is
**89,878,628 bytes**, up **13,720**, leaving **10,121,372 bytes** below the hard
limit; the 75 MB development target is still exceeded. The budget gate and
independent v2 signature verification pass. [Size audit](APK-SIZE-AUDIT.md#football-pace-stamina-and-pressure--8-october-2026).
Protocol **33** is required on all peers. Windows/Android inputs agree on
**3,099/3,100 hashes**; format-aware comparison proves the generated Golf ball
prefab's 14 objects and reference relationships are equivalent. Original
prefab IDs were restored to avoid unrelated generated churn.

The previous Windows player, APK and size audit were archived in
`Legacy/20261008-125934-707-football-pace-delivery/`. Duplicate test players and
temporary edit scripts were archived in
`Legacy/20261008-130544-128-football-pace-cleanup/`. Current reports, captures, hashes and source manifests
remain under ignored `Builds/`. No Git publishing action was performed.

## Simple lagoon fishing — 7 October 2026

The [simple fishing game](FISHING-MINIGAME.md) reuses the existing five piers,
two rod styles, three fish sizes and character grip socket. Free fishing starts
on arrival; the host can begin a three-minute points contest. The host owns
reservations, bite windows, progress, tension, deadlines and rewards. Owner-only
actions carry round/cast tokens; peers must use protocol **33**.

The complete **315-file** player in `Builds/WindowsFinal/` uses full assets built
at **22:32:43** and final managed scripts built at **23:05:32**. Its
**314 runtime/support files** match both the tested candidate
and the portable copy; only the delivery note is excluded from hash comparison.
Final checks pass:

- Fishing rendered offline: **70 report assertions**, including all three fish
  sizes, claim contention, stale/duplicate actions, disconnect/lease/deadline
  boundaries, five supported piers, bounded swimming, touch catch/release/cancel,
  keyboard action flow, hand alignment, three camera modes, light, reflection
  and Fishing/Golf/Fishing transitions without losing the shared hand grip.
- Fishing two-player local room: **8 host / 12 guest report assertions**, covering
  owner scoring, replicated catches/results, guest restrictions and shared restart.
- Portable default-path runner in a folder with spaces, launched from an unrelated
  working directory: **8 host / 12 guest report assertions**.
- Golf regression: **135 rendered offline** and **20 host / 20 guest report
  assertions**. The earlier graphics-free offline check failed while reading the
  first-person pose before `LateUpdate`; its rendered counterpart passes.
- Sky-Sail walking: **34 checks**, including the actual controller traversing
  each island's shore, gangway and boarding deck in both directions, on the
  earlier complete candidate. Traversal code/assets were unchanged by the final
  grip fix; the final rendered transition checks cover the affected handoff.

Close-up hand grip, fishing poses and lagoon camera captures are retained in
`Builds/FishingGameplayQA/Run-20261007-230635/`. The reflection uses explicit
URP render requests, six faces captured once, mipmaps and restrained distortion;
it reflects static scenery rather than moving players. This is arcade line
tension and bounded fish movement, not a fluid or flexible-rope simulation.

Source manifests include **3,110 inputs**. Final Windows script-source/Android
match on **3,109**; the difference is generated Golf prefab IDs/order.
The full Windows asset-source snapshot differs from the final script-source
snapshot in the two subsequently updated code files, generated Golf IDs/order
and URP's stripped runtime resource list. Asset/shader content is unchanged.
The exact full-asset Windows URP hash was reconstructed by adding
only the two desktop SSAO resource IDs to the Android/HEAD runtime list. All
authored settings match. The regenerated Golf prefab preserves all **14 objects'
properties and reference topology** against the retained original. Restoring
its original IDs/order is the sole final-source change after Android building;
gameplay code is unchanged. Records: `source-verification.json`,
`urp-source-verification.json` and `golf-generated-equivalence.json`.

The golf tee generator now avoids rewriting identical PNG bytes, resolving a
Windows mapped-file failure during the full build. The wood texture is unchanged;
both full Windows and Android builds pass. The previous setup-only player,
initial discarded draft and one-use integration helper are recoverable under
`Legacy/20261007-224018-816-fishing-gameplay/`. The preliminary fishing delivery,
portable copy and first-release receipts are recoverable under
`Legacy/20261007-230923-510-fishing-grip-handoff/`. An empty stale Git index lock
was removed after confirming no Git process was running; the index hash remained
unchanged. No Git publishing was performed.

Fresh AndroidSubmission completed at **23:16:43**. Actual APK size is
**89,938,934 bytes (+73,130)**, with **10,061,066 bytes** below the strict limit.
The 75 MB development target remains exceeded by **14,938,934 bytes**. Budget
and independent APK v2 signature checks pass. [Size and hash](BUILD-SIZE.md#simple-lagoon-fishing--7-october-2026).
Current evidence remains under ignored `Builds/FishingGameplayQA/`; golf reports
are under `Builds/GolfMiniGameQA/Run-20261007-232303/`.
No physical phone, WAN room, sustained FPS or thermal performance was qualified.

## Perspective broadcast cameras — 7 October 2026

The revised [stadium cameras](STADIUM-CAMERAS.md) use different perspective
angles for football and basketball, continuous pan and distance changes,
field-aware boundaries and screen-relative movement. Basketball includes
nearby rim framing from the baseline and corners, with influence fading in
smoothly near midcourt. First/third-person framing and lenses are restored
when leaving stadium mode; Golf and disabling the view are also checked.

The complete **316-file** player is delivered in `Builds/WindowsFinal/`.
Full assets were built at **18:13:45** and final scripts at **18:39:14**.
The final candidate passes **1,016 assertions in 10 reports**, with no
selected failures:

- Camera reviews: **139 each** at 1280x720, 1600x720 and 960x720.
- Portable player in a path containing spaces: **139** at 1024x768.
- Copied launcher under a separate root containing spaces, using its default
  player path from an unrelated working directory: **139** at 1024x768.
- Football match: **259 offline, 16 host, 15 client**.
- Basketball shared loft/bounce passing: **31 host/guest**.

The camera suite measures actual perspective playing-surface coverage and
viewport occupancy at centre, edges and corners, checks player/HUD separation
and nearby-rim visibility, and records continuous travel through both ends.
It checks movement through the real motor, stable framing while jumping and
changing action aim, projection/lens restoration and outside-field exploration.
At 16:9 midfield, measured coverage is **40.04% football / 53.03% basketball**.
The final motion runs keep the player visible without camera cuts. Rendered
full-HUD captures were inspected; the 16-second preview is an automated
framing sweep, not a phone-performance recording.

All **315 runtime/support files** match the validated portable copy; the
updated delivery note is excluded. Windows/Android manifests match on
**3,089/3,090 inputs including metadata**. Format-aware comparison confirms
the sole regenerated Golf prefab retains all 14 objects' properties and
reference relationships. The existing basketball features and protocol **32**
remain included. No gameplay source changed after the APK build.

Fresh AndroidSubmission completed at **18:45:24**. The APK grows
**89,861,552 → 89,864,908 bytes (+3,356)**, leaving **10,135,092 bytes** below
the hard limit. The 75 MB development target remains exceeded by **14,864,908
bytes**. Budget and independent v2 signature checks pass; serialized assets
are unchanged. [Size and signature hash](BUILD-SIZE.md#perspective-broadcast-cameras--7-october-2026).

Current evidence: `Builds/BroadcastCameraQA/checks.json`, `Reviews/`,
`PortableReview/`, `stadium-camera-preview.mp4`, `preview.json`,
`validated-player-hashes.csv`, `delivery-verification.json`, source manifests,
release/audit records and build logs. Regression evidence is in
`Builds/FootballMatchQA/Run-20261007-184256/` and
`Builds/BasketballPassQA/Reviews/Local-30-20261007-184329-038/`.
Superseded player/APK, reviewed iterations and the duplicate portable player
were archived with recovery hashes in
`Legacy/20261007-184908-013-broadcast-camera-delivery/`.
No physical Android appearance, touch, FPS/thermal or WAN testing was performed.
Git publishing remains with the user.

## Loft and bounce passes; shot chances — 7 October 2026

Protocol **32** adds [vertical pass selection](BASKETBALL-PASSING.md), a shared
curved ribbon and [continuous distance/timing shot odds](BASKETBALL-SHOT-ODDS.md).
Up selects blue loft; down selects orange bounce; the centre restores teal
chest passing. A separate Cancel target sits left of Pass. Keyboard Q with
Up/Down selects the equivalent paths; X cancels. The original finger owns the
gesture, and reliable release carries its final pass type.

The original 17:07 basketball candidate passes **828 assertions in 14 reports**,
with zero failures. Checks cover touch selection, lateral cancel, fixed-step
physical trajectories, actual floor contact, wall interruption, replicated
curves, descending loft/bounce catches, host-only shot outcomes, duplicate
release rejection, defense, charge, physics, dunk/layup and session flow.
Chest/loft preview error was **0.000 m**; bounce error was at most **0.072 m**
in the tested trajectories. Each odds suite checks 400,000 seeded samples,
monotonic range/timing behavior and nine physical make/miss branches, including
perfect close misses and imperfect full-court makes. Deflections and valid
downward hoop crossings still determine actual scores.

Held, cancelled and released chest/loft/bounce poses were reviewed visually and
against both head LODs at 20/120 FPS. **1,831 head-surface samples** had zero
hand/forearm penetrations beyond the 3 mm tolerance. Joint continuity checks
pass. The motion keeps the gather below the face and smoothly returns to the
dribble. A 16.2-second preview is retained at
`Builds/BasketballTrajectoryQA/loft-bounce-passing.mp4`.

The subsequent camera build contains the same basketball changes. Its final
17:22:50 managed player passes **328 additional assertions in four reports**:
offline trajectory/odds, host/guest authority and a complete portable copy in
a path containing spaces, invoked from an unrelated working directory with
the test script's default player location. All **315 gameplay/runtime files**
match `Builds/WindowsFinal/`; only its delivery note is excluded. The complete
316-file player and 17:27 AndroidSubmission include both features. The earlier
rendered animation preview and broad regressions predate the camera change;
the four combined-build reports are recorded separately.

The APK is **89,861,552 bytes**, **+13,760** over the 89,847,792-byte baseline,
with **10,138,448 bytes** of hard-limit headroom. It remains **14,861,552 bytes**
above the development target. This is the same combined release recorded below.
Independent signature and packing checks pass. The composed Windows manifest
uses the final camera source/configuration snapshot and the earlier metadata
snapshot, with provenance recorded explicitly: **3,089 of 3,090 inputs** match
Android. The sole difference is generated Golf prefab IDs/order; all 14 objects
retain their properties and reference relationships. No runtime source changed
after the release. Phone touch, appearance, FPS/thermals and WAN latency remain
unverified.

Evidence under `Builds/BasketballTrajectoryQA/`: `checks.json`,
`combined-checks.json`, original/combined player hashes, `release.json`,
`generated-input-verification.json`, `source-manifest-provenance.json`,
packing/signature audits, reports and captures. Reviewed superseded players,
iteration captures and helpers were archived with recovery hashes under
`Legacy/20261007-172822-647-basketball-trajectory-iterations`,
`Legacy/20261007-173123-245-basketball-trajectory-players` and
`Legacy/20261007-173258-941-basketball-trajectory-portable`.
No stage, commit, push, reset or history rewrite was performed.

## Stadium camera framing — 7 October 2026

[Camera design and controls](STADIUM-CAMERAS.md) and
[measured build](BUILD-SIZE.md#stadium-camera-framing--7-october-2026).

Rendered development-player reviews at **1280×720, 1600×720 and 960×720**
pass **107 assertions each**. They cover both fields at the centre, every
edge and every corner; actual ground-ray measurements confirm football's
40% and basketball's 52% midfield coverage. Goal-to-goal alignment remains
horizontal, players remain visible and off centre near lines, and the playing
surface retains the majority of the viewport. Full HUD captures were reviewed
to keep the player clear of the controls. Jump height and action aim do not
bob or rotate the stadium view. Screen-relative input is tested through the
real player motor. First/third-person framing, Golf perspective, menu projection
restoration and walking outside the field pass.

The complete portable Windows player also passes the camera review from a
path containing spaces and an unrelated temporary working directory. All
316 candidate files match the portable copy by SHA-256. The complete player
was then promoted to `Builds/WindowsFinal/`; delivery verification compares
every file, allowing only the updated `LATEST-BUILD.txt` note. Existing
portable launchers and the development geometry fixture are preserved.
The delivered player passes another 107-assertion camera review from an
unrelated temporary working directory. Across the six rendered camera reviews
and six selected regression reports, **1,102 assertions pass with zero failures**.

Selected regressions pass: football match **259 offline, 16 host and 15 client
assertions**; basketball shared passing **31 host/guest assertions**; and
basketball trajectory/shot-odds **139 offline assertions**. Camera rendering
uses Windows D3D11. No phone was tested, so Android appearance, physical touch,
FPS/thermals and WAN latency remain unverified.

The build preserves all pre-existing uncommitted work and uses protocol **32**.
This task changes no network payload. Final Windows/Android source manifests
match on **1,520 of 1,521 non-metadata inputs**. The regenerated Golf prefab
is the sole byte difference; all 14 objects have equal properties and reference
relationships after canonicalizing internal identifiers. All release gameplay,
scenes, art and configuration match. No gameplay code changed after the APK.

Evidence: camera reports/captures, source manifests, packing/signature audits
and delivery hashes are in `Builds/StadiumCameraQA/`; regressions are in
`Builds/FootballMatchQA/Run-20261007-172516/`,
`Builds/BasketballPassQA/Reviews/Local-30-20261007-172105-904/` and
`Builds/BasketballTrajectoryQA/Reviews/Offline-30-20261007-172948-311/`.

Reviewed obsolete outputs were archived with hash/recovery manifests under
`Legacy/20261007-172411-858-stadium-camera-apk`,
`Legacy/20261007-172617-581-stadium-camera-iterations` and
`Legacy/20261007-173003-580-stadium-camera-windows` and
`Legacy/20261007-173400-301-stadium-camera-candidates`. Current validation evidence
remains under ignored `Builds/`. No Git publishing action was performed.

## Shared basketball passing — 7 October 2026

Implemented [charged directional passing](BASKETBALL-PASSING.md) with a teal
court lane, flowing chevrons, amber charge accents and a brief release pulse.
The host replicates intent to every peer. Q/touch hold charges 4–10 m over
0.8 seconds; camera heading aims, release throws, and X/drag-to-Cancel retains
possession. Launch direction matches the lane without hidden receiver snapping.
Protocol **31** includes pass intent and captured starting arm poses.

The character finishes its dribble, gathers below the chin, pushes with both
hands and blends back into movement. Complete local arm poses remove elbow
flips; stable recovery offsets avoid wrist quaternion flips. Camera changes
turn the hands with the body. Cancel blends back to the actual dribble pose.

The selected suite has **365 passing assertions in 12 reports, zero failures**:

- Rendered offline controls at 60 FPS: quick tap, full charge cap, aiming,
  release direction, cancel, original-finger ownership, mutually exclusive
  shot/pass charging, stale/nonfinite requests and possession loss.
- Pass hold, push and cancel geometry at 20 and 120 FPS, both head LODs:
  **935 sampled poses**, zero hand/forearm vertices inside the head beyond
  the 3 mm tolerance. Arm and joint continuity bounds pass.
- Rendered host/guest pass cues and steering, authoritative release and
  noncolliding guest physics. Separate gameplay checks cover short catches,
  return passes, longer catches, scoring, free roam and reconnect/session flow.
- Rendered defense checks cover pressure, standing/dribble/shot/pass blocks,
  missed reaches, shielding, walls, both-hoop jump blocks and role switching.
  Separate host/guest finish checks preserve dunk, layup and host make/miss rules.
- The complete player was copied to a path with spaces and tested from an
  unrelated working directory using the script's default player location.
  This final run also checks cancellation after release but before input
  dispatch, same-frame shot/pass exclusion and active aim before possession loss.

The review caught and corrected a shader compilation error, gather/recovery
snaps and an aim-turn hand snap. Cancelling a queued release now also clears
the host's aim, and queued shot/pass releases cannot race each other.
A cancellation fixture now holds a real ball
and waits for pickup to settle. Network fixtures wait for observed charge and
shot gather, use velocity-based pass interception placement, and place shot/
layup defenders clearly inside the existing contact reach. Earlier boundary
misses remain documented; gameplay contact radii and defense success rules
were not relaxed to satisfy the tests. Concise iteration diagnostics remain
under `Builds/BasketballPassQA/Iterations/`.

Fresh AndroidSubmission: **89,847,792 bytes**, **+18,676** over the prior APK;
hard-limit headroom **10,152,208 bytes**. APK v2 signature and size gate pass.
The 75 MB development target remains exceeded by **14,847,792 bytes**.
[Packing details](APK-SIZE-AUDIT.md#shared-basketball-passing--7-october-2026).

Delivered the complete **316-file** Windows player in `Builds/WindowsFinal/`:
full assets built **15:07:32**, final scripts **16:11:57 Malaysia time**.
Its hash inventory matches the validated portable copy; only the delivery note
was subsequently updated. Windows/Android manifests agree on all gameplay,
scene, art and configuration inputs except regenerated Golf prefab IDs/order;
format-aware comparison confirms all 14 objects and reference relationships.

Current evidence is under `Builds/BasketballPassQA/`: `checks.json`, source and
player manifests, size audits, signature result, `basketball-passing.mp4` and
`passing-preview.jpg`. Superseded players, duplicate delivery output and
reviewed draft captures were archived in
`Legacy/20261007-155538-088-basketball-passing` (**2,176 files, 2,092.72 MB**).
The provisional APK and its audit were archived in
`Legacy/20261007-161247-302-basketball-pass-final` (11 files, 93.25 MB).
The redundant final portable player/test copy was archived in
`Legacy/20261007-161434-693-basketball-pass-portable` (321 files, 595.26 MB),
after preserving its passing report and complete hash inventory.
Active source/art remains in place. No Git staging or publishing was performed.
Physical Android appearance, touch, FPS/thermals and WAN latency remain untested.

## Defense motion refinement — 7 October 2026

Refined the guard, standing/high blocks, jump blocks and blocked-shot recovery
on the existing short-arm, large-head character. Hands rise outside the face,
the opposite hand balances lower, wrist-based reach avoids double clamping,
and a rearward elbow guide removes bend flips. Whole local poses blend into
the normal basketball crouch/run. Captured feet ease through moving takeoff
and blocked dunk recovery, including on guests. Protocol **30** is required.
No imported animation, texture or mesh was added.

The selected suite contains **895 passing assertions across 11 reports**, with
zero failures. It covers 20/30/60/120 FPS motion; both hands and LODs; guard
entry/release, turns, running jump blocks and airborne dunk interruptions;
actual two-process standing, pass, shot, layup and dunk blocks; role/button
switching; pressure; host authority; finish, charge and steal regressions.
The final player also passes from a directory containing spaces, launched
from an unrelated working directory with the test script's default player
path. Detailed report paths are in `Builds/BasketballDefensePolishQA/checks.json`.

Across **5,764 sampled poses**, the CPU-skinned hand and forearm vertices have
**zero head-surface penetrations exceeding 3 mm**. The same geometric check
reproduces the original clipping in both jump blocks and high contests. The
fixture accounts for the imported renderer's scale and checks world dimensions;
the head mesh includes hair. Running shoe tips and arm continuity use separate
bounds so normal running does not conceal an elbow flip. Front, side and
oblique views were reviewed, including normal and half-speed playback.

Current evidence under `Builds/BasketballDefensePolishQA/` includes
`defense-refined.mp4`, `before-after.jpg`, `preview.json`, `checks.json`,
`PortableReview/`, the APK audit/signature and source/player hash manifests.
The development-only host/guest review now publishes immutable command files
atomically after a Windows file-sharing race was reproduced. That final test
harness fix is excluded from the release APK; every release gameplay script,
scene, art asset and configuration agrees between the delivered Windows and
Android source snapshots. The Golf prefab's regenerated local IDs retain the
same properties and reference graph.

Fresh signed AndroidSubmission: **89,825,364 → 89,829,116 bytes (+3,752)**;
hard-limit headroom **10,170,884 bytes**. The 75 MB development target remains
exceeded by **14,829,116 bytes**. [Size record](BUILD-SIZE.md#defense-motion-refinement--7-october-2026).
The full validated Windows player, runtime files, development geometry fixture
and portable launchers are promoted together to `Builds/WindowsFinal/`.
No physical Android device, phone FPS/thermals or WAN connection was tested.
Git staging, commits and pushes remain with the user.

Archive batch `Legacy/20261007-130618-782-defense-motion-polish` contains the
superseded complete Windows player and reviewed intermediate builds/captures:
4,626 files, 2,957.97 MB. Its manifest records recovery paths and SHA-256 hashes.
Current comparison evidence, final reports and complete delivery stay under
ignored `Builds/`; no active game asset or authoring master was archived.

## Basketball defense — 6 October 2026

[Implemented controls and success rules](BASKETBALL-DEFENSE.md) cover pressure
guarding, grounded hand blocks and physical jump blocks. The motions reuse
the shared rig with lowered stance, alternating lateral steps, shoulder-led
reach, a balancing arm, takeoff load and landing absorption. The host resolves
contact from the same bounded palm path used by the pose.

The selected final feature and regression suite contains **978 passing
assertions in 22 reports, zero failures**, inventoried in
`Builds/BasketballDefenseQA/checks.json`. It covers:

- Offline defense at 20/30/60 FPS, both hands, both character LODs, continuous
  joints, palm/contact agreement, real jump height and grounded recovery.
- Rendered host/guest guarding, limited strafe speed, pressure and wall
  occlusion; dribble, ordinary shot and released-pass blocks; torso shielding,
  range misses, repeated inputs, stale requests, reset and possession changes.
- Owner RPC jump blocks against layups and dunks at both baskets, with
  interrupted releases occurring at most once and both players landing.
- Existing shot charging, physical scoring and nets, passes, steals, finishes,
  mouse/touch gesture routing, free roam, disconnect, room restart and shared
  jump behavior.
- Two final multiplayer gameplay runs confirming the guest's displayed
  release power matches host evaluation within **0.00001** and **0.00004**.

Slow rendered capture exposed a network timing defect: Netcode updates its
clock after Unity's catch-up physics. Extrapolating the action clock at each
fixed step keeps jump movement, held-ball motion and block windows together.
A further regression exposed the case where a received charge sample is ahead
of the guest's buffered clock. Release now uses that displayed sample's time,
while retaining the host's bounded timestamp validation. Earlier failed
fixtures and diagnostics were archived; they are not counted as passing runs.
The final network corrections were checked separately from the unchanged
offline pose and physics checks.

Evidence and presentation:

- `Builds/BasketballDefenseQA/defense-review.mp4`: sampled gameplay and clear
  jump-form capture at normal and half speed; `defense-storyboard.jpg` provides
  representative stance, reach and recovery frames.
- `Reviews/Local-30-20261006-223631-615/host/` under that evidence folder holds
  the rendered contact trace, motion samples and attacker/defender UI captures.
- `PortableEvidence/` and `portable-test.txt` record the review script running
  from a path containing spaces, with an unrelated working directory and its
  default complete-player path. This checks script/runtime path resolution,
  not a fresh-clone Unity build.
- The complete Windows player was promoted to `Builds/WindowsFinal/` at
  **22:54–22:55 Malaysia time**. All **315 files**, including preserved portable
  launchers, match the candidate hashes apart from `LATEST-BUILD.txt`.
- Fresh signed AndroidSubmission: **89,825,364 bytes**, **+22,360 bytes**, with
  **10,174,636 bytes** of hard-limit headroom. The 75 MB target remains exceeded.
  [Size and signature](BUILD-SIZE.md#basketball-defense--6-october-2026).
- Final Android packaging preserves 3,061/3,062 source inputs byte-for-byte;
  the regenerated Golf prefab's 14 objects retain all properties and reference
  topology. Runtime scripts and scenes are identical across packaging.

Reviewed local archives, all under ignored `Legacy/`:
`20261006-222935-294-basketball-defense-apk`,
`20261006-224938-264-basketball-defense-interim-apk`,
`20261006-225026-615-basketball-defense-portability`,
`20261006-225403-689-basketball-defense-windows` and
`20261006-225606-927-basketball-defense-iterations`. Manifests retain recovery
paths, sizes and hashes. Current APK, complete player and useful validation
evidence remain under `Builds/`. No game asset was archived.

Protocol **29** requires matching peers. No phone was connected; physical
Android touch, appearance, FPS, thermals and WAN timing remain unverified.
Git publishing remains with the user.

## Untimed finishes and shot fallback — 6 October 2026

The failed-looking up/down gesture was an unavailable finish entering the old
cancel/recovery path. Mouse and touch routing through Unity's input module
was intact. A rejected finish now releases an ordinary shot with the original
charge phase. Ready finishes hide the timing meter: **up is an unblocked 100%
dunk; down is a layup with one host-side 85% make roll**. Eligibility extends
to 4 m and checks grounding, approach, actual run-up and jump clearance.
The gather brakes while the ball returns from a bounce and keeps a short
planting allowance. [Rules and motion design](BASKETBALL-FINISHES.md).

The complete **01:04:56 Malaysia time**, protocol **28** Windows player passed
**811 assertions with zero failures** across the final retained suites:

| Suite | Passes | Evidence |
| --- | ---: | --- |
| Both hoops/hands, LOD1, standing layup, immediate release, wider approaches, planting grace, blocked jumps, reset, duplicate commands and both physical layup outcomes at 20 and 30 FPS | 368 | `Builds/BasketballFinishQA/Reviews/Offline-20261006-010545-675/`, `Offline-20261006-010546-202/` |
| Mouse/touch through StandaloneInputModule at 120 FPS: off-button release, immediate ready finishes, cancellation, gesture ownership and far-range shot fallback with preserved power | 76 | `Builds/BasketballFinishQA/Reviews/Offline-20261006-010546-584/` |
| Host dunk, guest layup make/miss, replicated pose, physical score and host-only outcome authority | 24 | `Builds/BasketballFinishQA/Reviews/Local-20261006-010544-866/` |
| Complete copied player and default launcher paths with spaces, invoked from an unrelated working directory; gesture checks at 20 FPS | 76 | `Builds/BasketballGestureFixQA/PortableEvidence/` |
| Basketball physics, power, scoring, net response and HUD | 81 | `Builds/BasketballPhysicsQA/FinishRules-20261006/` |
| Looping charge, aiming, cancellation and court boundary regression | 46 | `Builds/BasketballChargeQA/Reviews/FinishRules-20261006/` |
| Shared jump, recovery and two-player movement regression | 140 | `Builds/JumpQA/Run-20261006-010721/` |

`Builds/BasketballGestureFixQA/checks.json` records every final report. The
input tests supply mouse and touch states through a BaseInput override to the
actual Unity input module; they do not establish physical phone touch behavior.
The portability fixture verifies script-relative defaults and the complete
copied player, rather than a fresh-clone Unity build.

The **26.8-second** `Builds/BasketballGestureFixQA/finish-rules-review.mp4`
shows an untimed dunk, a successful layup, a wider planted approach and the
layup miss branch, each at normal and half speed. The inspected storyboard
shows gather, takeoff, release and landing. Final raw frames and telemetry
remain in the 30 FPS report folder. Maximum sampled held-ball palm error
during an active finish is **2.1 cm**; sampled architecture overlap is zero.
Each successful action releases once and recovers to the ground. The ready
control capture hides the timing bar and identifies the 85% layup rule;
fallback captures retain the ordinary meter and explain the normal shot.
Controlled random seeds verify both physical outcomes and exactly one host
roll, not an empirical sample estimate of the configured 85% probability.
No defensive block mechanic is added; released balls remain physical.

Fresh AndroidSubmission completed at **01:09:38 Malaysia time**, passing the
strict budget gate and independent APK v2 signature verification:
**89,798,556 → 89,803,004 bytes (+4,448)**, with **10,196,996 bytes** of hard-limit
headroom. The 75 MB development target remains exceeded by **14,803,004 bytes**.
[Packing audit and hash](BUILD-SIZE.md#untimed-finishes-and-shot-fallback--6-october-2026).
The existing Android Debug signing identity is retained. No phone was
connected; Android appearance, physical touch, FPS, thermals and WAN timing
remain unverified.

Across Android packaging, **3,048 of 3,050** source/asset/config inputs remain
byte-identical; runtime scripts and scenes are unchanged. Format-aware
comparison confirms the regenerated Golf prefab's 14-object graph retains all
properties and reference topology. The other difference removes two SSAO
entries from the Android URP runtime registration list while preserving their
definitions. Evidence is in `windows-source.json`, `android-source.json` and
`generated-input-verification.json` under `Builds/BasketballGestureFixQA/`.

At **01:11 Malaysia time**, the complete tested player was promoted to
`Builds/WindowsFinal/`. All **315 files** matched the candidate by SHA-256 before
updating `LATEST-BUILD.txt`. The delivery preserves the existing menu, Golf,
charging and steal work. The previous delivery note was checked before
replacement; no newer concurrent delivery was overwritten. Promotion and
final file hashes are retained under `Builds/BasketballGestureFixQA/`.
Rooms require matching protocol **28** builds.

Reviewed superseded local output was archived with recovery manifests:

- `Legacy/20261006-003811-039-basketball-gesture-diagnostic/` — initial diagnostic player, 313 files, 593.55 MB.
- `Legacy/20261006-010732-484-basketball-finish-rules-apk/` — previous APK, 1 file, 89.80 MB.
- `Legacy/20261006-011150-658-basketball-finish-rules-windows/` — previous complete Windows player, 315 files, 593.55 MB.
- `Legacy/20261006-011943-417-basketball-finish-rules-iterations/` — superseded reviews and duplicate player fixtures, 2,313 files, 2,183.96 MB.

Current validation, reproduction evidence, media, source snapshots and size
audits remain under ignored `Builds/`. No active art, authoring master or Unity
cache was archived. No staging, commit, push or history rewrite was performed.
Final readiness evidence is `Builds/BasketballGestureFixQA/task-ready.txt`.

## Basketball layups and dunks — 5 October 2026

The existing Shoot control selects **Dunk on an upward drag**, **Layup on a
downward drag**, and a normal shot on return to centre. Keyboard players hold
**E** and choose with the arrow keys. The host validates possession, grounding,
distance, approach, actual movement and clearance before committing the
gather, jump, release and landing. Wider timing windows reward a valid close
approach; every basket still requires the free physical ball to cross the rim.
[Controls, rules and animation design](BASKETBALL-FINISHES.md).

The complete **20:57:41 Malaysia time**, protocol **27** Windows player passed
**994 assertions with zero failures** across the final retained suites:

| Suite | Passes | Evidence |
| --- | ---: | --- |
| Both hoops/hands, LOD1, standing layup, motion contacts, approach rejection, blocked jumps, reset, duplicate commands, gesture selection and mistimed rebound at 20, 30 and 120 FPS | 384 | `Builds/BasketballFinishQA/Reviews/Offline-20261005-205814-781/`, `Offline-20261005-205814-782/`, `Offline-20261005-205814-786/` |
| Host dunk, guest layup, replicated pose, physical score and authority | 17 | `Builds/BasketballFinishQA/Reviews/Local-20261005-205919-063/` |
| Copied player and default launcher paths containing spaces, invoked from an unrelated working directory | 128 | `Builds/BasketballFinishQA/PortableEvidence/` |
| Basketball physics, power, scoring, net response and HUD | 81 | `Builds/BasketballPhysicsQA/Finish-Final/` |
| Offline shooting, passing, boundaries, travel and recovery | 98 | `Builds/BasketballGameplayQA/Offline-20261005-205920/` |
| Existing dribble, shot, pass and character motion regression at 20 FPS | 62 | `Builds/BasketballMotionQA/Finish-Final/` |
| Host/guest steals, shielding, loose-ball contests and charge cancellation | 38 | `Builds/BasketballStealQA/Local-20261005-210043-085/` |
| Looping charge, aiming, cancellation and court boundary regression | 46 | `Builds/BasketballChargeQA/Reviews/Finish-Final/` |
| Shared jump, recovery and two-player movement regression | 140 | `Builds/JumpQA/Run-20261005-210042/` |

The result registry is `Builds/BasketballFinishQA/checks.json`. The portability
fixture verifies script-relative defaults and the copied complete player; it
is not a fresh-clone Unity build.

Actual rendered gather, plant, takeoff, ball release and landing were inspected
at both hoops and with both hands. The **26.93-second**
`Builds/BasketballFinishQA/finish-review.mp4` shows four finish sequences at
normal and half speed; `finish-storyboard.jpg` and the final 30 FPS run retain
the frame evidence and contact telemetry. The final controls capture keeps
Cancel clear of the up/down drag route. Sampled held-ball architecture overlap
was zero, grounded recovery completed, and each successful action produced
exactly one physical release/attempt. The largest sampled palm error in the
30 FPS finish review was **2.1 cm** on the deliberately mistimed dunk. Fixed
capture rates and Windows renders do not establish phone performance or touch
ergonomics. No new defensive action is included.

Fresh AndroidSubmission completed at **21:03:16 Malaysia time**, passing the
strict budget gate and independent APK v2 signature verification:
**89,780,060 → 89,798,556 bytes (+18,496)**, with **10,201,444 bytes** of hard-limit
headroom. The 75 MB development target remains exceeded by **14,798,556 bytes**.
[Packing audit and hash](BUILD-SIZE.md#basketball-layups-and-dunks--5-october-2026).
The existing Android Debug signing identity is retained. No phone was
connected; Android appearance, touch, FPS, thermals and WAN timing remain
unverified.

Across Android packaging, **3,048 of 3,050** source/asset/config inputs remained
byte-identical; runtime scripts and scenes are unchanged. Format-aware
comparison confirms the regenerated Golf prefab's 14-object graph retains all
properties and reference topology. The other difference removes two SSAO
entries from the Android URP runtime registration list while preserving their
definitions. Evidence is in `windows-source.json`, `android-source.json` and
`generated-input-verification.json` under `Builds/BasketballFinishQA/`.

At **21:08 Malaysia time**, the complete tested player was promoted to
`Builds/WindowsFinal/`. All **315 files** matched the candidate by SHA-256
before updating `LATEST-BUILD.txt`. The source preserves the existing menu,
Golf, charging and steal work. The former delivery's timestamp was checked
before replacement; no newer concurrent delivery was overwritten.
`Builds/BasketballFinishQA/windows-promotion.json` records the promotion and
`candidate-player-hashes.json` retains the tested-file hashes. Rooms require
matching protocol **27** builds.

Reviewed superseded local output was archived with recovery manifests:

- `Legacy/20261005-210031-681-basketball-finishes-apk/` — previous APK, 1 file, 89.78 MB.
- `Legacy/20261005-210752-218-basketball-finishes-windows/` — previous complete Windows player, 315 files, 593.50 MB.
- `Legacy/20261005-211139-959-basketball-finishes-iterations/` — earlier captures/reviews and duplicate player fixtures, 3,796 files, 3,081.83 MB.

Current tests, media, source snapshots and size audits remain under ignored
`Builds/`. No active art, authoring master or Unity cache was archived. No
staging, commit, push or history rewrite was performed. Final readiness evidence
is retained in `Builds/BasketballFinishQA/task-ready.txt`.

## Basketball charging and energy boundary — 5 October 2026

The power needle now loops through repeated release windows, accelerating with
distance to a basket locked at charge start. Moving changes the integrated
phase continuously. Charging halves walking/sprint speed and gathers the ball
into a visible two-handed aiming pose, replicated to opponents. Drag-to-Cancel,
clicking Cancel or **X** lowers it back into the dribble without firing. The
translucent teal/gold court perimeter uses football's energy pattern with
expanding, fading player/ball contact ripples.
[Rules, calculations and controls](BASKETBALL-GAMEPLAY.md).

**575 assertions pass**, with no failing assertions in the retained suites:

| Suite | Passes | Evidence |
| --- | ---: | --- |
| Charging, half-speed movement, loops, target lock, cancellation, aim contacts and barrier | 46 | `Builds/BasketballChargeQA/Reviews/Final/` |
| Same new review through its default launcher in a path with spaces, from an unrelated working directory | 46 | `Builds/BasketballChargeQA/PortableReview/` |
| Physics, power, scoring, net response and final rendered HUD | 81 | `Builds/BasketballPhysicsQA/Charge-Delivery/` |
| Offline input, shots at both hoops, boundaries, travel and recovery | 98 | `Builds/BasketballGameplayQA/Offline-20261005-172820/` |
| Host/guest charging, visible remote aim, cancellation, shots, scoring and passes | 39 | `Builds/BasketballGameplayQA/Local-20261005-172921/` |
| Two-player stealing, secured charge, deflection and charge cancellation on strip | 38 | `Builds/BasketballStealQA/Local-20261005-172631-779/` |
| Existing motions and both character LODs at the 20 FPS review cap | 62 | `Builds/BasketballMotionQA/Charge-Final-20/` |
| Shared football boundary, kick feedback and cancellation regression | 25 | `Builds/FootballFeedbackQA/Run-20261005-172632/` |
| Shared jump and two-player movement regression | 140 | `Builds/JumpQA/Run-20261005-173045/` |

Measured charging walk speed is **4.00 → 2.00 m/s**. The moving aiming palm
stayed within the 4 cm contact tolerance (measured error rounded to **0.000 m**),
with a maximum sampled ball movement of **0.034 m/frame**. At 4, 12 and 22 m,
four successive green-window opportunities were observed without auto-fire.
Holding beyond five seconds remained active, and a later-cycle green release
scored. The authoritative release power matched the displayed value within
0.018, using host phase history rather than a client-supplied power.

Rendered close aim, player contact, ball ripple/fade, the whole court and the
power/cancel HUD were inspected. A premature capture initially sampled before
the procedural LateUpdate pose; the review now captures at end of frame and
asserts real palm contact. Visual review also found four malformed separator
characters in the existing scoreboard; they were corrected and the final
Windows player reran all 81 physics/HUD checks. The remaining gameplay source
is unchanged from the other passing suites. Football retained its translucent
look and unobstructed goal openings. These Windows observations do not certify
physical-phone visuals, touch or frame rate.

Fresh AndroidSubmission completed at **17:43:26 Malaysia time**, passing the
strict budget gate and independent APK v2 verification:
**89,762,748 → 89,780,060 bytes (+17,312)**, with **10,219,940 bytes** of hard-limit
headroom. The 75 MB working target remains exceeded by **14,780,060 bytes**.
[Packing audit and hash](BUILD-SIZE.md#basketball-charging-and-energy-boundary--5-october-2026).
Across packaging, 3,040 of 3,042 checked inputs remain identical. Canonical
comparison proves the regenerated Golf prefab has equivalent properties and
reference topology; the other change removes desktop-only URP registrations.

The complete **17:38:12 protocol 26 Windows player** is delivered under
`Builds/WindowsFinal/`, including the basketball launcher. After the user
authorized closing the old game, promotion completed at **19:53 Malaysia time**.
All **315 files** matched the tested candidate by SHA-256; the complete previous
314-file protocol 25 player was archived. Evidence is retained in
`Builds/BasketballChargeQA/windows-promotion.json`. Current source preserves
the menu, Golf and earlier basketball work. No staging, commit or push was
performed.

Reviewed obsolete output was moved to these archive batches:

- `Legacy/20261005-173200-338-basketball-charge-before-release/` — 11 files, 93.15 MB.
- `Legacy/20261005-174031-057-basketball-charge-diagnostic-apk/` — 21 files, 96.58 MB.
- `Legacy/20261005-174313-257-basketball-charge-iterations/` — 377 files, 629.94 MB.
- `Legacy/20261005-195311-178-basketball-charge-windows-delivery/` — 314 files, 593.45 MB.

Current captures, players, checks, source manifests and package audit remain
under `Builds/`. The earlier failed Windows archive attempt created only an
inventory; the successful delivery archive above supersedes that attempt.
No active art, authoring master or Unity cache was archived.
`Check-TaskReady.ps1 -RequireApk` passed, including the portable-path and Git LFS
checks. The publishable candidate set and Git diff were reviewed; nothing was
staged. The complete Windows delivery and its updated build note are verified.

## Refined basketball steal animation — 5 October 2026

The **15:25:12 Malaysia time**, protocol **25** Windows player passed
**366 assertions with zero failures**. The complete player is promoted to
`Builds/WindowsFinal/`; a hash comparison verified all **314 files**, including
the preserved SkySail launcher, before the delivery note was updated.
Results are indexed by `Builds/BasketballStealAnimationQA/check-counts.json`.

| Check | Passing assertions |
| --- | ---: |
| Challenge poses at fixed 20, 30, 60 and 120 FPS | 188 |
| Copied player/default paths, checkout path with spaces, unrelated working directory | 47 |
| Dribble, run, stop, shoot, pass, jump, LOD and release regression | 62 |
| Real-time held/released steal input and geometry/deflection rules | 31 |
| Two-player host/guest stealing and reactions | 38 |
| **Total** | **366** |

Pose coverage includes left/right hands, low/high contacts, mirrored victim
reactions, movement, repeated swipes, full recovery and the lower-detail model.
The largest sampled planted-support-foot drift is **1.6 mm**; bone lengths
remain fixed and arm reach correction stays within the configured bound.
The fixed-clock fixture schedules repeated poses on simulation time; the
separate input suite tests actual UI hold/release using its real-time clock.
Close and side frames were inspected. The **17.33-second**
`Builds/BasketballStealAnimationQA/steal-animation-preview.mp4` includes actual
two-player ball knockaways at normal speed and half speed, plus isolated pose
review. Captures are review evidence, not an FPS benchmark.

Current evidence: `Release-20/`, `Release-30/`, `Release-60/`, `Release-120/`,
`PortableEvidence/`, `promotion-hashes.json` and `windows-source.json` under
`Builds/BasketballStealAnimationQA/`; basketball motion regression under
`Builds/BasketballMotionQA/StealAnimationRelease-30/`; steal checks under
`Builds/BasketballStealQA/Offline-20261005-152726-201/` and
`Local-20261005-152713-115/`.

Fresh **15:31:44** AndroidSubmission passes the hard budget and independent v2
signature check: **89,758,540 → 89,762,748 bytes**, with **10,237,252 bytes** of
headroom. The 75 MB working target remains exceeded by **14,762,748 bytes**.
The Android build preserves **3,032 of 3,034** recorded inputs exactly;
format-aware text comparison confirms that the regenerated Golf prefab retains
its properties and references, and URP returns to the repository's Android
settings. Runtime scripts and scenes match the tested Windows source. No phone
was connected; physical-device appearance/FPS, thermals, touch feel and WAN
latency are unverified.

Reviewed local archives, with sizes, hashes and recovery paths:

- `Legacy/20261005-151802-842-basketball-steal-animation-iteration/`
- `Legacy/20261005-152321-289-basketball-steal-animation-refinement/`
- `Legacy/20261005-152652-548-basketball-steal-animation-snapshot/`
- `Legacy/20261005-152845-910-basketball-steal-animation-before-release/`
- `Legacy/20261005-153142-243-basketball-steal-animation-windows-delivery/`
- `Legacy/20261005-153421-641-basketball-steal-animation-evidence-cleanup/`

## Basketball steals and loose-ball contests — 5 October 2026

The final **protocol 25**, **14:03:09 Malaysia time** Windows player passed
**517 assertions with zero failures** and was promoted in full to
`Builds/WindowsFinal/`. Package hash comparison covers **314 files**. Current
results are indexed by `Builds/BasketballStealQA/check-counts.json`:

| Check | Passing assertions |
| --- | ---: |
| Steal controls and geometry, offline | 31 |
| Stealing, host / guest | 32 / 6 |
| Steal script in a path with spaces, unrelated working directory | 31 |
| Existing basketball gameplay, offline / host / guest | 98 / 22 / 14 |
| Basketball physics, scoring and net response | 81 |
| Basketball motion at simulated 20 FPS | 62 |
| Shared jump/momentum, offline / host / guest | 132 / 3 / 5 |

Steal checks exercise held-button repeats, repeated-request recovery, touch
ownership, release/drag/focus/disable cancellation, jumping and Free roam
exclusion, close/far/height/facing/shielded contacts, dribble exposure and secure
grips, walls, escape during windup, possession reset, host/guest owner input,
both replicated animations, real body collisions, loose-ball velocity/spin,
shot-charge cancellation, pickup grace and recovery by the original carrier.
Velocity checks cover both players' movement, opposite swipe hands, rising vs
falling dribbles, speed bounds and translated/rotated contact geometry.

Final two-player renders and telemetry are in
`Builds/BasketballStealQA/Local-20261005-140328-419/host/`.
Frames around contacts **92** and **107** show the swipe/reaction and loose-ball
flight. The maximum recorded arm target correction after clamping was **2 mm**;
this is an IK limit metric, not proof of exact palm/ball surface coincidence.
The final in-game HUD screenshot is
`Builds/BasketballGameplayQA/Offline-20261005-140328/offline/carry-controls.png`.
Close frames and control placement were visually inspected. Early checks found
a touch-test source mismatch and a jump request tested before simulation;
the final fixtures exercise real pointer ownership and the simulated jump.
Review also corrected outgoing player collisions and fitted palm targets.

The [fresh signed AndroidSubmission](BUILD-SIZE.md#basketball-steals-and-loose-ball-contests--5-october-2026) measures
**89,758,540 bytes**, up **14,428**, with **10,241,460 bytes** below the hard limit.
The 75 MB development target remains exceeded. Android packaging preserved
**3,028 of 3,030** checked inputs byte-for-byte; the two exceptions are reviewed
platform URP registrations and equivalent generated Golf prefab IDs. C# source
and scenes match the final Windows validation snapshot. Source manifests,
canonical verification, the APK signature and both packing audits are retained.

Superseded APK/audit and complete Windows deliveries were archived with hashes
and recovery paths in `Legacy/20261005-140452-410-basketball-steal-before-release/`
and `Legacy/20261005-140636-398-basketball-steal-windows-delivery/`.
Intermediate tests/logs and the portable fixture are in
`Legacy/20261005-140731-651-basketball-steal-iterations/`; its passing results
remain in `Builds/BasketballStealQA/PortablePathEvidence/`.
The hash-matched duplicate candidate is retained in
`Legacy/20261005-141102-934-basketball-steal-tested-candidate/`.
`Check-TaskReady.ps1 -RequireApk` passes, including the portable-text, Unity
metadata, LFS, whitespace and candidate-file checks; its output is retained in
`Builds/BasketballStealQA/task-ready.txt`. No binary art or active asset dependency was edited for the feature. Existing
nonfunctional art-provenance exceptions are unchanged. Git publication remains
with the user. Physical-phone touch, Android appearance/FPS/thermals and WAN
latency remain unverified; local Windows captures do not qualify them.

## Storybook Cove menu and teammate integration — 5 October 2026

The first selected direction is now the native game menu: illustrated island
selection, Create/Join, live lobby, real match conditions, football/basketball
venue editing, settings and all four sport help pages. All navigation buttons
have hold compression, release feedback, cancellation, disabled/focus states and
one-shot activation. Sliders animate their thumb without shifting their input
track. Pages crossfade/slide with input gated, and gameplay arrival fades in.
Reduced motion keeps colour/focus feedback while suppressing movement.
[Behaviour and source guide](STORYBOOK-MENU.md).

**1,658 feature assertions pass** across the following retained Windows suites:

| Suite | Passing assertions | Evidence |
| --- | ---: | --- |
| Final offline menu, pointer/keyboard states, inputs, layout, settings, preview/save/discard | 694 | `Builds/StorybookMenuQA/FinalReview/offline/` |
| Final host/guest lobby, conditions, permissions, ready resets, departure/return | 138 (70/68) | `Builds/StorybookMenuQA/FinalReview/host/`, `client/` |
| Football gameplay, clock, physics and network regressions | 532 | `Builds/FootballMatchQA/Run-20261005-125217/` |
| Basketball physics, scoring, charging and net response | 81 | `Builds/BasketballPhysicsQA/StorybookMerge/` |
| Basketball host/guest input, possession, scoring, free roam and permissions | 38 | `Builds/BasketballGameplayQA/Local-20261005-130919/` |
| Golf offline and host/guest match regression | 175 | `Builds/GolfMiniGameQA/Run-20261005-130812/` |

The final menu player includes the last visual, slider, focus and clipboard
refinements. Earlier sport suites cover unchanged gameplay sources; the menu
refinements did not alter those mechanics. Combined network layout is **protocol
24**. The pulled Golf/cart work and preserved local basketball work both pass.
No conflict markers or unmerged index entries remain; the pre-merge recovery
stash and hashed files are retained. Nothing was staged, committed or pushed.

Two fresh anonymous players also used the **real Unity cloud session/Relay**
service to create/join one private room, become ready, depart and return together,
then leave. Evidence: `Builds/StorybookMenuQA/CloudReview/` and `cloud-final.txt`.
Both players shared one internet connection; this is not different-network WAN
or Android-device qualification. Create/join completed without connection errors.
On host deletion, the guest logged a handled `lobby not found` warning during
cleanup; both players disconnected and returned to offline state.

All final copy/paste and typing checks pass. Earlier Windows clipboard writes
intermittently returned empty, reproduced separately by PowerShell outside Unity;
the final OS roundtrip succeeded. Copy retries briefly and announces success only
after readback; failure leaves the displayed room code and typing fallback usable.
Earlier failure evidence is retained in `ClipboardInvestigation/` without recording
user clipboard contents. Android clipboard/keyboard behaviour still needs a phone.

Rendered captures were visually reviewed for home, lobby, conditions, actual
football screen/basketball court previews and settings. Bounds, button raycasts,
label width and text height pass. Actual Windows sizes checked: **1600×900,
1280×800 and 1920×1080**. The requested 2340×1080 window was clamped to 1920×1080
by this display, so ultra-wide support is not claimed as tested. Two-player
network review passes another **138 assertions** from a copied default player tree
containing spaces, launched from an unrelated working directory. The font
generator produces identical hashes from that portable fixture. Evidence:
`PortableNetwork/`, `portable-menu.txt` and `portable-font-check.json`.

Fresh AndroidSubmission and APK v2 signature pass: **89,744,112 bytes**, with
**10,255,888 bytes** hard-limit headroom; the 75 MB development target is unmet.
Source manifests cover 3,022 files, with only equivalent generated Golf IDs and
platform URP registration changes. [Size and delivery](BUILD-SIZE.md#storybook-cove-menu-and-teammate-integration--5-october-2026)
and [packing audit](APK-SIZE-AUDIT.md#storybook-cove-menu-and-teammate-integration--5-october-2026).
No physical Android device was connected. Phone touch feel, appearance, sustained
FPS, thermals and WAN timing remain unverified.

The complete tested player, including all runtime DLLs/data and existing launchers,
was promoted to **`Builds/WindowsFinal/`**. Updated `LATEST-BUILD.txt`, file-hash
promotion evidence, current screenshots, test logs and audit remain under `Builds/`.
Reviewed obsolete outputs were archived with recovery inventories; see
`archive-baseline.txt`, `archive-windows.txt`, `archive-iterations.txt` and `archive-candidate.txt` in
`Builds/StorybookMenuQA/`. No active art or master was removed.

## Basketball pace and player boundaries — 5 October 2026

Basketball charging and shot preparation now take **30% less time**: full charge **1.20 → 0.84 s**, ideal release **0.78 → 0.546 s**, shot animation release **0.44 → 0.308 s**, and full shot action **0.98 → 0.686 s**. Queued dribble preparation and authored shot poses use the same time scale. Pass timing and the green power band are retained. [Controls and implementation](BASKETBALL-GAMEPLAY.md).

An invisible movement boundary keeps the complete player capsule inside the painted court during basketball play. Input and velocity projection preserve smooth sliding along sidelines, including diagonal movement. The ball can leave the court and remains physical. A loose ball stranded beyond the painted boundary near the floor returns after **0.75 s**, while airborne apron excursions retain the wider recovery envelope. **Free roam** deliberately releases the boundary and ball possession for station travel; **Play basketball** restores play. Host authority, replicated free-roam state, arrivals and session resets are covered.

**479 assertions passed** with no failing assertions:

| Suite | Passing assertions | Retained evidence |
| --- | ---: | --- |
| Offline input, charging, shots, boundaries, sliding, free roam and rebound recovery | 98 | `Builds/BasketballPaceQA/QA/Offline/` |
| Host/guest possession, charge, scoring, net events and court/free-roam state | 36 (22 host, 14 guest) | `Builds/BasketballPaceQA/QA/Local/` |
| Motion timing, dribble, hand contact and both character LODs at 30/20 FPS caps | 124 (62 each) | `Builds/BasketballPaceQA/QA/Motion-30/`, `QA/Motion-20/` |
| Shot physics, faster power timing, score/miss decisions, nets and portable launch | 81 | `Builds/BasketballPaceQA/QA/Physics-Portable/` |
| Shared jumping and two-player regression | 140 | `Builds/BasketballPaceQA/QA/Jump/` |

The rendered motion review checked gather/release hand contact, first-/third-person views, green meter feedback, sidelines and deliberate free-roam exit. Maximum measured action hand reach error was **25 mm**. Observed frame-sampled release/recovery times stayed within the expected render/fixed-step allowance around the new authored times. Net displacements in the physics samples reached approximately **6.3–7.4 cm**, with fixed upper attachments and a return to rest. Deliberately weak/strong shots missed; well-timed two/three-pointers scored. The portable suite ran from a copied tree containing spaces, launched from an unrelated working directory.

Tests exposed two issues that were fixed before release: repeated loss of diagonal sliding speed when constraining velocity alone, and a loose ball resting on an unreachable apron inside the old wider recovery envelope. The final offline/network rerun includes both fixes and additional airborne/apron recovery cases. The earlier motion/physics/jump checks cover unchanged timing and shared mechanics; the final Windows player contains all fixes.

During work the primary checkout received `af02ea7` and concurrent Storybook menu changes. Preserved basketball work was applied to that integration and tested in an isolated checkout, excluding the unfinished menu. The matched tested players in **`Builds/BasketballPaceQA/Release/` use protocol 23**. The main source uses **protocol 24** for its later menu network field. These results do not certify the combined unfinished menu build, and an older primary APK is not evidence for the new source.

The prescribed AndroidSubmission build passed at **12:56:34 Malaysia time**: **86,791,760 → 89,260,934 bytes**, leaving **10,739,066 bytes** below the strict limit and exceeding the 75 MB working target by **14,260,934 bytes**. APK v2 signature verification passed. Of 277 checked inputs, 276 remained unchanged during the build; the Golf ball prefab regenerated equivalent local IDs/document order only, confirmed by canonical property/reference comparison. [Detailed size audit](APK-SIZE-AUDIT.md#basketball-pace-and-player-boundaries--5-october-2026). No physical phone was connected; Android touch, appearance, FPS, thermals and WAN timing remain unverified.

Reviewed temporary logs, superseded test runs and the completed portable fixture were archived to **`Legacy/20261005-125501-976-basketball-pace/`** (**368 files, 607.76 MB**) with hashes and recovery paths. Current release/evidence remain under `Builds/BasketballPaceQA/`. No active art or authoring master was archived. The isolated build passed `Check-TaskReady.ps1 -RequireApk`; the primary checkout also passed repository hygiene, with its APK freshness separately qualified above. Git staging, commit and push remain with the user.

## Basketball physics and net response — 4 October 2026

The basketball now has a 30 cm visual/physical diameter (25% larger), fitted palm contacts, a hold/release power meter, deterministic ballistic launches, shared 2/3-point practice scoring, missed-shot outcomes and responsive nets. [Implementation, equations and controls](BASKETBALL-GAMEPLAY.md).

**396 Windows assertions passed** across these retained suites:

| Suite | Passing assertions | Local evidence |
| --- | ---: | --- |
| Shot physics, power, scoring, net response and portable execution | 79 | `Builds/BasketballPhysicsQA/FinalReview/` |
| Offline shooting, rebound pickup, collision, recovery and streaming | 67 | `Builds/BasketballGameplayQA/Offline-20261004-233337/` |
| Host/guest charge edges, scoring, net events, passes and lifecycle | 32 (20 host, 12 guest) | `Builds/BasketballGameplayQA/Local-20261004-233711/` |
| Dribble, enlarged-ball palm contact, gather, release and both character LODs | 56 | `Builds/BasketballMotionQA/Physics-Initial/` |
| Ball drop, spin, both rims and 35 m/s floor/backboard contacts | 22 | `Builds/BasketballModel/20261001/Offline-233712/` |
| Shared jump/input and two-player regression | 140 | `Builds/JumpQA/Run-20261004-234309/` |

The focused suite exercises complete downward passage, upward and side rejection, fast crossings, incomplete rim exits, three-point/corner lines, foreign-pointer release rejection, touch cancellation, clean timed two/three-pointers at both hoops, deliberately short/long shots, timeout, one outcome per attempt and session clearing. Scored nets reached approximately **5.6–6.3 cm** displacement in the measured samples; their upper attachments stayed fixed, and the mesh returned to rest. Outside-net touch moved the net without awarding a basket. Host and guest agreed on two three-pointers and received the authoritative net events. The regression suite also retains real descending rim crossings at **2, 5, 10, 18 and 24 m**.

Rendered Windows views of the larger ball beside the character, dribble/contact poses, first-/third-person and elevated cameras, the green power meter, both scored nets and exterior net touch were inspected. The rig retained bounded hand reach and contact errors; the two-metre drop and 35 m/s floor/backboard probes remained above the floor. These observations do not certify physical-phone visuals, touch or frame rate. The existing scoreboard architecture is decorative; the new HUD tracks shared practice, without basketball teams, a match clock or foul rules. Net response is a bounded procedural approximation with soft resistance, not full cloth simulation.

An early test found that the Unity hoop's inward court axis is positive local Z; the three-point calculation was corrected. Synchronous screenshot readback inside a timed hold then skewed the test's release, so UI capture was separated from the actual timing assertions. The corrected suite passed from a copied player/script tree named `Portable Basketball Physics`, launched from an unrelated temporary working directory. Its evidence was preserved in `FinalReview/` before archiving the fixture. All **137 checked documentation links** resolved. New scripts derive their root from their own location. Source FBX, Blender and texture bytes were not changed; their previously recorded nonfunctional provenance exceptions remain as documented in [the model report](BASKETBALL-BALL.md).

The prescribed AndroidSubmission build passed at **23:45 Malaysia time**: **86,774,768 → 86,791,760 bytes**, with **13,208,240 bytes** of hard-limit headroom. The 75 MB development target is still exceeded by **11,791,760 bytes**. APK v2 verification passed; 238 checked script/shader/prefab/scene inputs did not change during that build. [Size audit and exact artifact hash](BUILD-SIZE.md#basketball-physics-and-net-response--4-october-2026). Windows player: `Builds/WindowsFinal/`; Android player: `Builds/Android/WhatTheFish-release.apk`. Protocol **19** requires matching builds. No Android device was attached; phone performance and WAN hold timing remain unverified.

Reviewed failed trials, superseded build logs and the completed portable fixture were moved with the archive tool to **`Legacy/20261004-234732-924-basketball-physics/`** (339 files, 594.35 MB), with hashes and recovery paths. No active art or authoring master was archived. Existing menu-design work was preserved. `Check-TaskReady.ps1 -RequireApk` passed, including publishable path scanning and Git/LFS hygiene; evidence is in `Builds/BasketballPhysicsQA/task-ready.txt`. These checks are read-only; Git staging, commit and push remain with the user.

## GitHub Golf and football integration — 5 October 2026

The combined Windows player built at **00:02:12 Malaysia time** passes
**1,106 assertions with no failures or runtime exceptions**:

- **252 Golf**: 76 rendered ball/tee/physics, 135 rendered offline and 20 host
  plus 21 side-approach client. Evidence:
  `Builds/GolfBallPhysicsQA/Run-20261005-000517/` and
  `Builds/GolfMiniGameQA/Run-20261005-000518/`.
- **532 football gameplay/physics/network**: 259 offline, 16 match host,
  15 match client, 199 physics and 26 ball host plus 17 ball client. Evidence:
  `Builds/FootballMatchQA/Run-20261005-000742/`.
- **292 football motion** at the 60 timeline rate, including both LODs,
  saved takes, bounded rigs, stale snapshots, reset and allocation checks.
  Evidence: `Builds/FootballMotionQA/Run-20261005-000743-227/`.
- **30 rendered lagoon**: shared three-size fish, transparent water, all three
  gameplay camera modes, animated water/caustics and reload/cutout cleanup.
  Evidence: `Builds/GitSyncQA/Lagoon/`.

Golf's actual full-charge contact remains **18.00000 m/s** horizontally and
travels **12.27873 m** during the 0.8-second observation from release. Slow-roll
settling, slope rest/wake, five-hole progression and selected-ball strokes
remain. Room protocol **22** covers both Golf and football replicated layouts
in the runtime service and Bootstrap scene. The eight reviewed conflicts keep
both features and release histories; normalized upstream Golf scene content
matches its baseline, so the complete local five-hole scene is retained.
Conflict variants, decisions and comparisons are in `Builds/GitSyncQA/`.

All **113 runtime source files** match the tested isolated build. Reviewed
build differences are its editor-only build wrapper, generated desktop
pipeline registration, equivalent **14-object** Ball prefab IDs and a legacy
material colour difference below **1e-6**. The complete **313-file** player is
promoted to `Builds/WindowsFinal/` and retained with the fresh APK/audit in
`Builds/GitSyncQA/Release/`; package hashes are recorded in the task evidence.
Fresh primary AndroidSubmission at **00:07:25** measures **89,230,290 bytes**,
with strict gate, APK v2 signature and packing audit passing. See
[size and archives](BUILD-SIZE.md#github-golf-and-football-integration--5-october-2026).
Physical-phone rendering, touch, FPS and WAN remain unverified. Earlier
documented fishing-route issues were not part of this integration's changes.

## Golf swing speed 18 m/s — 4 October 2026

**250 assertions pass** on the Windows player built at **23:20:49 Malaysia
time**: **76 rendered ball/tee/physics**, **134 rendered Golf/club offline**,
**20 host** and **20 client**. Evidence:
`Builds/GolfEighteenSpeedQA/PhysicsFinal-20261004-232216/`,
`MatchFinal-20261004-232217/` and `NetworkFinal-20261004-232217/`.

The real qualified full-charge swing measures its first horizontal launch
velocity at **18.00000 m/s**. The ball travels **12.27873 m** during the
0.8-second observation from release, exceeding the former five-metre cap
without an extra stroke or reset. A later **18.19571 m/s** horizontal peak is
normal landing energy transfer and is measured separately from launch.
Flying and high-speed rolling have no global speed clamp. Existing slow-roll
resistance, slope settling, stop delay, resting, strike/force/collision wake,
wooden tees, all five cups, restart and host authority checks pass.

All **313 Windows player files** are promoted to `Builds/WindowsFinal/` and
retained in `Builds/GolfEighteenSpeedQA/Release/Windows/`. The final manager
includes the concurrent Unity-null trigger initialization correction.
Golf runtime/configuration, meadow source, scene and environment prefab match
the tested isolated source. Reviewed isolated build differences are the
editor-only output path, generated pipeline registrations and equivalent
prefab IDs. The later editor-start review helper does not ship in the player.
Source comparisons and package hashes: `Builds/GolfEighteenSpeedQA/`.

Fresh primary AndroidSubmission completed at **23:27:16 Malaysia time**:
**89,024,294 bytes**, strict gate and independent APK v2 signature pass.
The initial-launch probe is excluded from release APK compilation.
Physical-phone appearance, touch, FPS and WAN remain unverified. See
[size, headroom and archives](BUILD-SIZE.md#golf-swing-speed-18-ms--4-october-2026).

## Golf meadow restored — 4 October 2026

**354 checks** pass on the restored Windows player built at **22:43:13 Malaysia
time**: 78 saved-course editor, 142 rendered island, 134 rendered Golf match.
Five numbered flags, recessed cup floors, grounded tees, 48 shoreline directions,
fairway/bunkers, camera/environment switching and five-hole progression remain.
Independent comparisons verify **294 original mesh references** and the exact
**137,724-blade** pre-course runtime meadow; **2,289 blades** return to the
former large clearings. Original terrain-map/configuration hashes are unchanged.

Evidence: `Builds/GolfGrassQA/`, `Builds/GolfG2QA-20261004-224336/` (five hole
views/overview), `Builds/GolfMiniGameQA/Run-20261004-224347/`.
Exact complete player/APK/audit: `Builds/GolfGrassQA/Release/`; promoted player:
`Builds/WindowsFinal/`. Fresh shared primary AndroidSubmission at **22:44:23**:
**89,024,882 bytes**, gate/v2 signature/packing checks pass. Reviewed isolated
differences are editor-only clean packing, URP registrations and equivalent
Ball prefab IDs. Later **22:46 GolfBall settled-stroke guard changes** are outside
this captured release. Phone rendering/touch/FPS and WAN remain unverified.
Initial disk-full output, old delivery and obsolete grass are archived; see
[release records](BUILD-SIZE.md#golf-meadow-restored--4-october-2026).

## Golf cart forward-facing feet — 4 October 2026

Seated feet now align the imported toe-tip segments with the cart front, rather
than inheriting the standing foot angle or using the sloping ankle-to-toe-base
segment. Both knees stay straight and the seated hip/hand targets are retained.
Native entry, turning and both character LOD captures were reviewed. **439 checks
pass**: 221 cart offline/host/client, 134 Golf match and 84 ball physics checks.
Both loopback peers report forward-facing toes and straight knees; walking and
jump recover after leaving the cart.

The complete **313-file** Windows player built at **22:23:26 Malaysia time** is
initially promoted to `Builds/WindowsFinal/` and retained in `Builds/GolfCartFeetQA/Release/Windows/`. A later complete shared player, built **22:43:13**, includes the same feet source and concurrent grass work; it is preserved and passes another **221 cart assertions** at `Builds/GolfCartQA/Run-20261004-225224/`. Native entry/turn/LOD foot captures were reviewed again, and both loopback peers report zero-degree forward toe alignment. The earlier 439-check snapshot remains separate.
Evidence: `Builds/GolfCartQA/Run-20261004-222342/`,
`Builds/GolfMiniGameQA/Run-20261004-222431/` and
`Builds/GolfBallPhysicsQA/Run-20261004-222345/`.
The 2,974-input source snapshot includes the contemporaneous five-metre Golf
swing settings. Later unrelated GolfCourseBuilder and MobileIslandGrass changes
are outside this snapshot. Generated 14-object Ball prefab IDs are equivalent;
legacy material `_Color` changes by less than 0.000001 while `_BaseColor` matches.
The isolated editor uses a clean build cache. No art asset is added by this fix.

Fresh task AndroidSubmission built **22:40:48 Malaysia time** after recovering
from a disk-space failure. APK **89,021,758 → 89,025,074 bytes (+3,316)**;
strict headroom **10,974,926 bytes**, development-target excess **14,025,074 bytes**.
APK v2 signature and fresh packing analysis pass. SHA-256:
**4A5719AFBB1A825E1A7C4AC92CF6B00BA8CF70CF29B05D34538F65AA673AA91E**. Serialized asset delta is 4 bytes (Golf configuration);
all other per-asset estimates match. No art is added by this pose correction.
The exact APK/audit is retained in `Builds/GolfCartFeetQA/Release/`.

A newer primary-workspace APK, built **22:44:23**, is preserved at
`Builds/Android/WhatTheFish-release.apk`: **89,024,882 bytes**,
strict headroom **10,975,118 bytes**, SHA-256
**20D100869CBFAA24F9E74F7F5CE1D16BCA0075A8B8F3FDC4F935A301272706E4**. Its v2 signature and generated native code confirm the same
toe-tip correction. Later unrelated grass generator/scene/prefab changes and
archived grass assets are outside this task's native gameplay snapshot.
Phone visual quality/FPS remains untested. The predecessor APK/audit was
archived by the concurrent task in
`Legacy/20261004-224157-371-golf-five-metre-old-delivery/`; it was not overwritten
with the older isolated task build.

Reviewed first-attempt output is archived in
`Legacy/20261004-221446-653-golf-cart-feet-trial/`; the superseded shared Windows
player is archived in `Legacy/20261004-222648-806-golf-cart-feet-windows/`,
with the manifest recorded in `Builds/GolfCartFeetQA/archive-windows.txt`. Ignored historical candidates were
compressed without changing their paths or bytes. Git publishing remains with
the user.

## Reliable Golf Swing proximity — 4 October 2026

The strict arm-extension check was the cause of the gray Swing button. On the
actual rig, the original predicate accepted **0/12 angles at 0.05, 0.25 and
0.50 metres**, and only **1/12 at 0.75 and 1.0 metres**. Eligibility now uses a
**1.1-metre horizontal radius**, supported height, grounded state and terrain
visibility. Swing addresses the selected ball; camera heading still sets shot
direction. Charge selection stays locked. Strength, timing and physics remain
unchanged.

**560 assertions passed on matching gameplay and assets:**

- **104 rendered Swing checks**, including 72 distance/angle combinations at
  0.05–1.08 metres, actual UI raycast, touch hold/release, body turn with planted
  feet, selected-ball lock, lost-target cancellation, wall/transit/jump rejection
  and five very-close/side contact poses with both palms attached. Evidence:
  `Builds/GolfSwingQA/Run-20261004-213536/`.
- **134 rendered Golf offline checks**, preserving rules, strokes, cups,
  grip/idle/walk/run, first person, cancellation, cart stow/re-equip, restart
  and island switching. Evidence:
  `Builds/GolfMiniGameQA/Run-20261004-213536/`.
- **41 side-approach network checks**: 20 host and 21 rendered client, including
  automatic stance after transform interpolation, reliable charge/swing,
  other-owner ball striking, actual authoritative movement, owner progression,
  wrong-cup recovery and the real 30-second countdown/DNF. Measured client
  stance error is **0.000 degrees**. Evidence:
  `Builds/GolfMiniGameQA/Run-20261004-210915/`.
- **72 rendered ball/tee/physics checks** and **209 cart checks** (163 rendered
  offline, 23 host, 23 client), with identical gameplay/assets to the delivered
  player. Evidence: `Builds/GolfBallPhysicsQA/Run-20261004-211139/` and
  `Builds/GolfCartQA/Run-20261004-211139/`.

Two preceding gait-capture runs missed a low running-hand frame while the
fixture travelled on the real terrain. Resetting each gait to the level
fixture makes the intended animation check pass; no runtime animation changes
were needed. Failure reports remain in `Builds/GolfMiniGameQA/`. The initial
packaging attempt was interrupted under disk pressure; its incomplete player
is archived, and the delivered player is a successful complete rebuild.

The final complete Windows player is built at **21:32:44 Malaysia time** and
promoted to `Builds/WindowsFinal/`. All **313 Windows files**, the fresh APK
and **11 audit files** match `Builds/GolfSwingQA/Release/`. AndroidSubmission
built at **21:16:14**, **89,021,758 bytes**, with strict size and independent
signature checks passing. The **2,974-input** capture matches production
gameplay and authored scene/model inputs. The later gait-fixture change lives
entirely in the development-only probe, excluded from release APK compilation;
canonical Ball prefab/material comparisons and build-only differences are
recorded in `Builds/GolfSwingQA/`.

Run [Test-GolfSwing.ps1](../Tools/Build/Test-GolfSwing.ps1) with `-Render` for
the interaction matrix; [Test-GolfMatch.ps1](../Tools/Build/Test-GolfMatch.ps1)
supports `-OfflineOnly -Render` and `-NetworkOnly -SideSwing -Render` for these
cases. Physical-phone touch/rendering/FPS and WAN remain unverified. Git was
not staged, committed or pushed. See
[size and recovery batches](BUILD-SIZE.md#reliable-golf-swing-proximity--4-october-2026).

## Golf wooden opening tee — 4 October 2026

The complete **20:57:01 Malaysia time** Windows player passes **246 checks**:
72 rendered ball/tee/physics/camera/cup/restart checks at
`Builds/GolfBallPhysicsQA/Run-20261004-210427/`, and 174 Golf/club/room checks at
`Builds/GolfMiniGameQA/Run-20261004-210503/` (134 rendered offline, 20 host,
20 client). This exact player is promoted to `Builds/WindowsFinal/` and retained
in `Builds/GolfWoodTeeQA/Player/`. Package hashes and source snapshots are under
`Builds/GolfWoodTeeQA/`. The identical cart/seat gameplay also retains the
concurrent **209-check** cart evidence at `Builds/GolfCartQA/Run-20261004-204749/`.

Native checks cover a separate fixed concave wooden support, shared small
grain/material, existing dynamic ball stability, real low-charge Swing and
one stroke, support remaining fixed, outside-course and wrong-cup recovery,
all five cups, unchanged later-hole placement, clean restart, and host/client
supports for both owners. Ground-query and collision checks keep characters
and carts isolated from this tiny support. Actual close, normal camera and
cup-only captures are retained; the reference ball is never imported or used
as the live ball. Existing camera, three-times ball radius, flight, slopes,
settling, force wakeup, owner-based completion and 30-second countdown checks
also pass.

The first AndroidSubmission attempt hit native memory exhaustion. The fresh
concurrent **20:56:07** AndroidSubmission APK already contains the exact tee,
match, ball, cart and other gameplay sources and assets, so it is reused after
comparing all **2,974** captured inputs and independently verifying its APK v2
signature and detailed packing. The six reviewed differences are generated
URP registrations, an editor-only clean packing option, equivalent Ball
prefab child IDs, editor GolfSwingReview and development-only GolfMatchProbe/
GolfSwingProbe changes. Release probe logic is compiled out. APK bytes are
**89,022,058**, with **10,977,942** bytes of strict-limit headroom; the 75 MB
development target remains exceeded. Exact APK/audit:
`Builds/GolfWoodTeeQA/Release/`. This is fresh matching-source evidence; the
failed primary build and older APK are not counted as successful validation.
Physical-phone touch/rendering/FPS and WAN remain unverified.

The previous shared player is in
`Legacy/20261004-211728-900-golf-wood-tee-delivery/`. Reviewed trials, source
backups and failed compile/stance/OOM diagnostics are listed by the task's
archive records. No staging, commit, push or history changes were performed.

After the final player was captured and tested, independent primary
`GolfClubMotion` work and a development-only `GolfMatchProbe` edit continued.
Those later source edits are preserved and outside this player/APK snapshot;
this record does not validate or deliver that ongoing work. The tee and its
physics/placement/assets remain identical to the tested snapshot.

## Golf cart straight-knee driver — 4 October 2026

The current Windows snapshot passes **455 assertions**: **209 cart** checks
(163 rendered offline, 23 host, 23 client), **72** rendered ball/physics/wooden
tee checks, and **174** Golf/club/room checks (134 rendered offline, 20 host,
20 client). Evidence: `Builds/GolfCartQA/Run-20261004-204749/`,
`Builds/GolfBallPhysicsQA/Run-20261004-204749/`, and
`Builds/GolfMiniGameQA/Run-20261004-204749/`.

The former bent-leg ankle IK target is replaced only in the seated leg overlay.
Both actual bone segments align forward and slightly upward without changing
their lengths. Hip and wheel-hand targets remain. Measured knee angles are
**179.9802–180°**, ankle positions about **0.30 m forward / 0.02 m above** the
hip marker, and sampled baked shoe-region vertices at least **0.232 m forward**.
The complete avatar top stays at or below **2.27233 m**, leaving **5.77 cm**
beneath the **2.33 m** roof underside. Native close views of both character LODs
show straight legs and shoes ahead of/above the seat cushion. Entry, driving
turns and the remote guest pose pass on both peers. Exit restores walking,
jump and walking cameras. Speed, steering, rolling, collisions, ownership,
recall, terrain traversal and island travel checks also pass.

The first straight-knee trial retained too much downward leg angle for this
short rig. Its failed clearance checks and images are archived in
`Legacy/20261004-203548-973-golf-cart-seat-trial/`. An overlapping-build texture
allocation exhausted system memory; diagnostics are in
`Legacy/20261004-204222-803-golf-cart-seat-oom/`. The isolated retry completed
after the other batch editors finished. No gameplay settings were changed
to work around that build failure.

Fresh release AndroidSubmission, strict APK gate and independent v2 signature
pass at **89,022,058 bytes**. The shared full Windows player, APK and audit
match the exact retained packages under `Builds/GolfCartSeatQA/Release/`.
The **2,974-input** snapshot includes the current wooden tee and settling/contact
runtime. Later development-only probe and editor review changes are outside
this tested snapshot and compiled out of release; gameplay sources match.
Generated URP registrations and equivalent Ball prefab local IDs are reviewed,
with the **14-object graph** matching. Raw source comparisons and package hashes
are under `Builds/GolfCartSeatQA/`. [Measured delivery and archives](BUILD-SIZE.md#golf-cart-straight-knee-driver--4-october-2026).
Phone touch/rendering/FPS and WAN remain unverified.

TaskReady with `-RequireApk` passes using a read-only copy of the Git index.
The pre-existing shared `.git/index.lock` is preserved; the real index hash
is unchanged. No staging, lock removal, commit or push was performed.

## Golf slope settling and resting — 4 October 2026

Final Windows sources pass **233 assertions**: **61** rendered ball-physics
checks and **172** Golf/club/room checks (134 offline, 19 host, 19 client).
Evidence: `Builds/GolfBallPhysicsQA/Run-20261004-203111/` and
`Builds/GolfMiniGameQA/Run-20261004-203306/`. The complete tested player is
retained under `Builds/GolfSettlingQA/Release/Windows/`.

On temporary test-only 5°/15° plates, a 1.8 m/s shot enters slow rolling at
**1.7793 / 1.9049 m/s**, decelerates without downhill re-acceleration, and rests
after **0.520 s** continuously below the configured 0.25 m/s threshold (0.5 s
plus fixed-step rounding). Position remains within **1 mm** over the subsequent
2 s, with zero linear/angular velocity. The resting body remains dynamic and
collidable, with its gravity disabled. Fresh strikes immediately restore gravity
and clear timers; 5 m/s downhill shots remain above 4.5 m/s after 0.5 s.

Coverage includes gentle new ball contact resetting a nearly complete stop timer,
ball-to-ball wakeup, an impulse just before the old stop timer
would expire, a tiny direct Rigidbody impulse, loss of support, and a fresh strike
issued **after settling in the same FixedUpdate**. Flight is compared against a
separate Rigidbody with the original 0.05 linear damping: velocity differs by less
than 0.002 m/s, including that same-step strike. Settling cancels tangential gravity
in velocity rather than queuing a force that could survive a later Strike.
A level 3 m/s roll reaches **2.412 m/s at 0.6 s** and stops normally. Qualified
8%/80% Swing peaks remain **4.174 / 31.271 m/s**, with charged height gain
**1.994 m**. All five existing cups, stroke counts, next tees, results and restart
checks pass. Body/cart collision exclusions and existing Host/Client snapshot
behavior are preserved.

All **2,430 existing scene/art/terrain/environment-prefab inputs** compared to
the task capture are unchanged. Production changes are limited to GolfBall,
its new persistent physics settings script/asset, ball verification and docs.
Concurrent wooden-tee work is preserved in the main workspace but excluded from
this isolated delivery; its dependent development-probe block is excluded only
in the isolated snapshot; its tee-layer addition to the ground query is likewise
excluded there. One concurrent rendered run missed the transient low-hand capture
window; the unchanged final player passed the separate retry, recorded above. No map, slope, hole, player movement, leaderboard,
countdown or other-sport behavior is changed by this task.

[Inspector defaults and manual test steps](GOLF-MINIGAME.md) are maintained with
the Golf guide. [APK measurement and delivery records](BUILD-SIZE.md#golf-slope-settling-and-resting--4-october-2026)
record the exact release scope. Physical-phone rendering/touch/FPS and WAN remain
unverified; Windows results are not device-performance measurements.

## Golf 3x ball and optional Aim — 4 October 2026

Final Windows build: **19:50:57 Malaysia time**. **41 native checks** pass in
`Builds/GolfBallPhysicsQA/Run-20261004-195210/`: both existing ball LODs and the
single collider use **129 mm diameter / 64.5 mm radius**; match startup remains
centred; the Chinese Aim/Cancel button enters the previous shoulder framing
and returns; first-person/elevated modes and round resets clear Aim. Real tee
captures include every UI overlay and prove visible ball pixels while aiming,
without moving the ball or forcing its initial LOD.

Physics remains measured: bodies and driven carts displace the resting ball
**0.00000 m**. Level roll slows **3.000 → 2.318 m/s at 0.6 s**, then stops.
Sleeping balls roll downhill on 5°/15° slopes and reach **0.592/1.757 m/s at
one second**. Qualified 8%/80% swings peak at **4.174/31.271 m/s**; the charged
ball rises **1.994 m**. The larger sphere enters each of the five actual cups,
preserves the stroke count, returns supported at each next tee, completes the
match and restarts with zero strokes and Aim off.

The same final player passes **172 Golf/club/room assertions**: **134 rendered
offline, 19 host and 19 client**, in
`Builds/GolfMiniGameQA/Run-20261004-195211/`. This includes actual live
idle/walk/run grip views, the new radius in swing fixtures, cup order, ownership,
recovery, touch input, the real 30-second finish deadline, DNF, restart,
cameras and sport cleanup. The **203 cart regressions** in
`Builds/GolfCartQA/Run-20261004-191514/` pass on identical cart/input/camera
behavior before later development-only cup/diagnostic changes. Earlier
rendered grip checks exposed stale skinning after manually restored poses;
the shared fixture now observes live poses and the final full suite passes.

All **2,953 primary inputs** remain unchanged after final capture. Isolated
build-only clean packing and Ball prefab local IDs are reviewed; its canonical
**14-object graph** matches. The release APK predates only an independent
development-only GolfMatchProbe diagnostic correction, which is compiled out
of release; gameplay matches. Font regeneration from a checkout with spaces
and unrelated working directory preserves all required glyphs and licensing.
No Computer Use or Git publishing was used.

The complete player and fresh **89,000,984-byte** APK/audit are retained in
`Builds/GolfBallAimQA/Release/` and promoted to the shared WindowsFinal, Android
and SizeAudit folders; all delivered files match. The APK strict gate and v2
signature pass. See [size/headroom and recoverable archives](BUILD-SIZE.md).
Physical-phone touch/rendering/FPS and WAN remain unverified.

Final hygiene checks pass across **3,402 publishable files and 468 LFS paths**.
The pre-existing shared `.git/index.lock` blocks the default Git dry run;
TaskReady was rerun with a read-only copied index. The real index hash remains
unchanged and the existing lock is preserved. Diff/stat/status were reviewed.

## Golf closed grip and raised carry angle — 4 October 2026

The complete Windows player passes **172 Golf/club/room assertions**: 134
rendered offline, 19 host and 19 client. Evidence and native captures:
`Builds/GolfMiniGameQA/Run-20261004-192709/`. The live high/low frames were
inspected directly; manually restoring multiple bone poses in one rendered
frame can reuse Unity's skinning cache and is not used for these checks.

The source character has no weighted finger joints. Import now derives a
local closed-mitten blend shape for both existing character LODs, plus a grip
socket. The right grip marker is on the shaft axis; the equipped shape is
released when driving, travelling or clearing the round. Original FBX, atlas,
rig hierarchy and unrelated vertices are preserved. Palm weights transition
into the hand bone to stabilize the cavity as the wrist moves.

Idle and low-hand carry remain 12 degrees down/back. The highest walk/run
phase reaches **52.71 / 52.72 degrees**, with a minimum **0.059 m** head
clearance above foot level. Native arm travel remains: right/left forward-back
ranges **0.379/0.511 m** walking and **0.378/0.511 m** running. Right-hand angular
sway is **45.1 / 49.3 degrees**. Shaft-to-socket error stays below 1 mm. All six
idle/walk/run high/low grip poses pass actual skinned-triangle enclosure checks:
6–8 of eight radial rays hit the hand, with minimum cavity distances of
**8.4–8.6 mm**. Both near/far character LODs receive the closed shape. Full-body,
close-hand, charge, contact and follow-through captures were reviewed.

The existing two-hand charge/contact, ball impulse, cooldown, cart stow/reequip,
five-hole progress, results and sport switching checks pass. Host/client use
protocol 21. The tested snapshot also contains the concurrently developed aim
button and 3x ball runtime; a later change to the independent development-only
ball-physics probe is outside this snapshot. That probe difference does not
change shipped gameplay. Runtime inputs and generated prefab equivalence are
recorded under `Builds/GolfGripSlopeQA/`.

Tuning and regeneration are documented in
[the club source guide](../ArtSource/Golf/Club/README.md). Windows checks do not
establish physical-phone touch, rendering or FPS, or WAN behavior. Exact APK
measurement and delivery/archive records are in
[BUILD-SIZE](BUILD-SIZE.md#golf-closed-grip-and-raised-carry-angle--4-october-2026).

## Golf ball visibility and free physics — 4 October 2026

Native reproduction at the real Hole 1 tee found a correctly spawned, grounded
43 mm ball. The former rear camera hid it behind the avatar at swing distance,
and the gold indicator could cover the small rendered sphere. Live-Golf
third-person framing now uses a closer shoulder position; the indicator keeps
a screen-space gap above the ball, and the last ball LOD no longer culls.
First-person, elevated and driving camera rules are preserved.

The final Windows player passes **22 dedicated checks** in
`Builds/GolfBallPhysicsQA/Run-20261004-172433/`. Actual native images with all UI
visible prove ball pixels at default 16° and 25° pitch, without moving the ball
or forcing its LOD before the initial captures. A real sprint through the ball
moves the player about **5.04 m** and the ball **0.00000 m**. Late/re-enabled
character controllers and a moving cart collider also cannot push it.

On level ground, a **3.000 m/s** ball slows to **2.315 m/s after 0.6 seconds**
and stops. A sleeping ball on a **5° slope** accelerates from **0.242 to
0.592 m/s**, travelling **0.306 m** in one second; on **15°**, it accelerates
from **0.718 to 1.758 m/s**, travelling **0.907 m**. Both remain supported.
Qualified **8% / 80% swings** produce peaks of **4.210 / 31.271 m/s**; the
charged ball rises **1.994 m** in the observation period. Each swing counts
once. Tests use gravity/contact physics and measured motion, not force values.

The complete five-hole/club/room suite passes **143 assertions: 105 offline,
19 host and 19 client**, in `Builds/GolfMiniGameQA/Run-20261004-170433/`.
It verifies helper ownership, cup order, recovery, touch input, animation,
the actual 30-second finish deadline, DNF, restart and sport cleanup.
Existing charge tests now use a temporary level collider on both peers;
assuming a free ball stayed still on a real incline incorrectly cancelled
charge after downhill rolling was restored. No gameplay ground is replaced.
The **203 cart regressions** pass in
`Builds/GolfCartQA/Run-20261004-165059/` on identical gameplay sources before
these development-only fixture corrections, including three camera modes,
reverse follow, driving collisions and two-player replication.

The complete tested Windows snapshot is retained in
`Builds/GolfBallPhysicsQA/Release/Windows/`, built at **17:22:06 Malaysia time**.
The development capture fixture renders every overlay Canvas; the shared
capture helper previously selected only one and could omit the Golf HUD.
All **2,951 asset/package/settings
inputs** are captured; the generated Ball prefab's **14-object graph** is
equivalent to primary despite regenerated local IDs. No model, texture,
package, room protocol or scoring rules were added or replaced. Physical-phone
touch/rendering/FPS and WAN remain unverified. See
[physics, controls and repeatable checks](GOLF-MINIGAME.md) and
[release size and delivery](BUILD-SIZE.md).

The complete **313-file** player is promoted to `Builds/WindowsFinal/`, and the
matching fresh **88,984,188-byte** APK/audit is promoted to the shared Android
and SizeAudit folders. All player files and the APK match the retained release.
The reviewed preceding shared release is recoverable in
`Legacy/20261004-173632-475-golf-ball-physics-delivery/`; superseded trials and
earlier snapshots are documented in BUILD-SIZE. No Git staging or publishing
was performed.

The default physics-check launcher also passes **22 checks** against the
promoted `WindowsFinal` player from an unrelated system temporary directory,
in `Builds/GolfBallPhysicsQA/Run-20261004-174344/`; the checkout path contains
spaces. Normal readiness passes portable paths but encounters the existing
shared Git index mutex during its dry run. The same full read-only check passes
using an index copy: **3,394 publishable files, 466 LFS paths, 271 pending
operations**. The actual index SHA is unchanged and its shared lock is
preserved. Readiness and index evidence are under `Builds/GolfBallPhysicsQA/`.

## Golf palm animation — 4 October 2026

Single-hand carry preserves the right wrist's full original idle/walk/run
rotation along with the shoulder and elbow. Club orientation is stabilized at
the moving palm, and fingers curl using the actual palm normal. This removes
the counter-rotation that previously forced the palm down and folded the wrist.
Two-hand charge/contact/follow-through still use the existing blended arm pose.

The complete Windows player passes **142 Golf assertions: 105 offline, 19 host
and 18 client**, in `Builds/GolfMiniGameQA/Run-20261004-160852/`. Across complete
walking/running samples, additional wrist deviation is **0.000°**, while palm
rotation changes **82.604° / 112.407°** with the arm animation. Grip error stays
below **1 mm**. Right-hand forward/back ranges are **0.4615 / 0.4615 m**, compared
with **0.5114 / 0.5105 m** on the left. Club backward alignment remains at least
**0.978** and downward pitch stays **9.38–18.00° / 9.40–18.00°**. Idle wrist
deviation is also zero with **1.195°** natural hand sway.

Native idle, walk, run and two-hand charge views were inspected. The suite also
checks contact timing, strokes, all five holes, recovery, restart, cart stow,
sport cleanup and two-player authoritative replication. Evidence and the exact
tested player are retained under `Builds/GolfPalmCarryQA/`. No art, animation
asset or package was added. Physical-phone rendering/FPS and WAN remain
unverified. See [carry behavior](GOLF-MINIGAME.md#handheld-midnight-iron) and
[tuning](../ArtSource/Golf/Club/README.md).

Normal readiness passes portable-path scanning but its Git dry-run encounters
an existing shared index mutex. The full check passes using a temporary index
copy: **3,393 publishable files, 466 LFS paths and 270 pending operations**.
The actual index SHA stays unchanged and the shared mutex is preserved. The
obsolete one-file copy is archived in
`Legacy/20261004-162243-882-golf-palm-readiness/`; scope and checks remain under
`Builds/GolfPalmCarryQA/`. No staging, commit or push was performed. The complete
new player and APK are retained in the release folder and promoted to the shared
WindowsFinal/Android directories after the older game closed. All **314 Windows
files** match the retained player. The older shared release is recoverable in
`Legacy/20261004-162407-373-golf-palm-delivery/` (**325 files, 682.08 MB**).

## Rounded Golf ball — 4 October 2026

The supplied rounded-dimple model replaces the shared `Ball.fbx` and existing
`Ball.prefab` visual used by both practice equipment and live match balls. The
prefab and model asset identities remain intact. The original 87,048-triangle
FBX is retained byte-for-byte, with an editable Blender master and **15,998 /
720-triangle** delivery LODs. Geometric dimples share one ivory material without
the previous ball textures. The centred mesh fits the existing **21.5 mm**
sphere collider; mass, damping, swing/contact timing, ownership, authority,
stroke rules, five-hole progress, recovery and final countdown are unchanged.
Normal builds and the equipment generator preserve this replacement. See
[source, regeneration and dependency audit](../ArtSource/Golf/Ball/README.md).

The final captured Windows player passes **140 Golf assertions: 103 offline,
19 host and 18 client**, in `Builds/GolfMiniGameQA/Run-20261004-142455/`.
Checks cover the actual new mesh/material on live balls, centring, one physics
body/collider, charged contact, helper ownership, all five real cups, wrong-cup
and shoreline recovery, restart, completion/countdown, sport switching, cart
stow, current club carry and two-player authoritative replication. **20
equipment checks** also pass in `Builds/GolfBallRoundedQA/Equipment/`, including
practice placement, both LODs, material support and streamed cleanup/reentry.
Native close-ball, normal playing, equipment and both model LOD views were
inspected. The animated carry fixture samples after LateUpdate so it measures
the rendered hand and club pose together.

Format-aware audits find no image or external-library dependency. Regeneration
from a checkout path containing spaces and an unrelated working directory
preserves canonical positions, UVs and winding; pole custom-normal variation
is bounded below 0.0002. Only inactive exporter SceneInfo provenance and saved
Blender UI history are exempt historical metadata. Reports, hashes and captured
inputs are under `Builds/GolfBallRoundedQA/`. Physical-phone touch, rendering/FPS
and WAN remain unverified. Final package measurements and recovery records are
in [BUILD-SIZE](BUILD-SIZE.md#rounded-golf-ball--4-october-2026).

## Golf club natural carry — 4 October 2026

Idle, walking and running retain the right shoulder/elbow's original animation,
with the club attached to the moving palm instead of a fixed hip-space target.
The wrist compensates for the long shaft and fingers remain curled around the
grip. Default pitch is 12° down with a bounded 6–18° animated variation; side
sway stays within 12° of behind the player. This prevents the original wrist
animation from swinging the shaft upright or forward. Charging and swinging
still blend into two-hand IK and return to the moving hand afterwards. See
[carry tuning](../ArtSource/Golf/Club/README.md) and [game behavior](GOLF-MINIGAME.md#handheld-midnight-iron).

The retained carry-task Windows player passes **140 Golf checks: 103 offline,
19 host and 18 client**, in `Builds/GolfMiniGameQA/Run-20261004-142055/`.
The entire sampled walking/running cycle keeps the head behind the player
(minimum horizontal back alignment **0.978**), with downward pitch
**9.38–18.00° walking / 9.46–18.00° running**. Right-palm forward/back ranges
measure **0.3893 / 0.3888 m**, compared with **0.5114 / 0.5108 m** for the left
hand. The club stays on the right palm with measured grip error below 1 mm.
Idle head sway is **0.0125 m** and pitch **16.23–16.48°**. Carry/idle/walk/run,
two-hand charge/contact/follow-through and first-person captures were inspected.
The suite also checks strokes, helper ownership, contact timing, charge cancel,
recovery, restart, driving stow, sport cleanup and real two-player loopback.
Concurrent rounded-ball checks are included. Physical-phone touch/rendering/FPS
and WAN remain unverified.

The shared `Builds/WindowsFinal/` now contains the newer **14:23:37** rounded-ball
player, including this final carry implementation. It passes **140 checks** in
`Builds/GolfMiniGameQA/Run-20261004-142455/`, including the whole-gait backward
direction and grip assertions. All **313 delivered files** match its exact
retained player. This newer complete delivery is preserved; the older tested
carry player remains in `Builds/GolfClubCarryQA/Release/Windows/`. The matching
shared Android APK is **88,981,824 bytes**, with **11,018,176 bytes** below the
strict ceiling. Source captures, promotion hashes, signatures and archive scope
are recorded in [BUILD-SIZE](BUILD-SIZE.md#golf-club-natural-carry--4-october-2026).

## Midnight Iron handheld club — 4 October 2026

Every participant receives the supplied textured club when a Golf round starts. Walking carries it in the right hand with its head above the ground; running tilts it toward the body while the free arm retains its gait. Charging, impact and follow-through use both hands, curled fingers and the existing avatar rig. First-person hands remain visible and the locally hidden head restores in third person. Driving/travel/round cleanup hides the club; restarts reuse one instance. Feet stay planted through contact, the ball receives one authoritative impulse at **0.10 seconds**, and **0.74-second** recovery prevents duplicate strokes. Reach validation rejects impossible contacts. Round/ball resets cancel pending impacts. Reliable charge/heading and server-clock swing poses synchronize host and clients; **protocol 21 requires matching builds**. Existing ownership, helper completion, scores, hole order and countdown rules remain intact. See [controls and tuning](GOLF-MINIGAME.md#handheld-midnight-iron).

The fresh textured Windows player passes **113 Golf assertions: 84 offline, 15 host, 14 client**, in `Builds/GolfMiniGameQA/Run-20261004-130434/`. Checks cover every player's club, three LODs/one material, all three correctly sized Texture2D maps, no prop physics, hand-marker grip errors below 2.5 cm, reachable two-hand charge/impact/follow-through, first-person restoration, carry floor clearance, running tilt/free arm, charge cancellation, delayed single ball impulse, movement/jump/duplicate gates, reset cancellation, driving stow and instance reuse. Actual two-player loopback verifies reliable remote charge, shared swing timing, strokes/owner progress, rejected stale rounds and the existing thirty-second replicated DNF. Native carry, charge, first-person, contact, follow-through and running captures were inspected; near/mid/far model renders are in `Builds/GolfClubQA/Art/`.

The same final gameplay sources pass **203 cart assertions** (`Builds/GolfCartQA/Run-20261004-124646/`), **509 football match/physics/network assertions** (`Builds/FootballMatchQA/Run-20261004-124646/`) and **28 basketball gameplay checks plus two connection checks** (`Builds/BasketballGameplayQA/Local-20261004-124647/`). These regressions precede only the final club texture import/material correction and added map assertion; final Golf validation uses the corrected textured player. No physical-phone touch, rendering/FPS or WAN claim follows from these Windows tests.

The format-aware art audit passes in the primary workspace and from the alternate checkout with spaces, invoked from an unrelated working directory. It confirms the **7,702-triangle** editable master, **3,600 / 1,400 / 500** LODs, 1.02 m length, finite UVs, single connected components, closed meshes, no degenerates, original-archive/hash integrity and all seven relative Blender image dependencies. The delivery FBX's three texture references are relative; no external libraries or active machine paths remain. Source maps/original ZIP are retained outside Unity. Reports: `Builds/GolfClubQA/PortableAudit/` and `Builds/GolfClubQA/AlternateCheckout/`; see [source guide](../ArtSource/Golf/Club/README.md).

AndroidSubmission completes release IL2CPP ARM64 with a **89,418,634-byte** APK and **10,581,366 bytes** of hard-limit headroom; independent v2 signing passes. The 75 MB development target remains unmet. All **2,947 captured asset/package/settings inputs** remain stable during packaging and match the primary workspace except the isolated editor's clean-cache option. The user's editor and original caches remain open/in place. The complete tested Windows player, built at **13:04:30 Malaysia time**, is promoted to `Builds/WindowsFinal/` with updated `LATEST-BUILD.txt`; all **313 files** match the retained `Builds/GolfClubQA/Release/Windows/`. Current APK/audit and exact retained releases are documented in [BUILD-SIZE](BUILD-SIZE.md#midnight-iron-handheld-club--4-october-2026).

Reviewed superseded output and trial evidence are recoverable in `Legacy/20261004-131836-279-golf-midnight-iron-final/` (**422 files, 763.69 MB**). An unchanged empty Git index mutex was observed across separate inspections, confirmed exclusively openable and cleared with the actual index SHA unchanged; its backup is in that archive. Active sources, assets, masters, current evidence, caches and unrelated work were preserved. Git staging, commits and publishing remain with the user.

## WindowsFinal handoff — 4 October 2026

The complete reference-tyre player built at **11:43:59 Malaysia time** is promoted to `Builds/WindowsFinal/`, including executable, data, runtime DLLs, supporting folders and Golf/Fishing launchers. It is the same snapshot that passes **203 cart assertions**, retaining the later Golf-club source exclusions recorded below. `LATEST-BUILD.txt` states its feature, build time, test evidence, matching APK and snapshot limits. The standing Windows delivery requirement is recorded in [AGENTS.md](../AGENTS.md#windows-testing-player-delivery) and [repository hygiene](REPOSITORY-HYGIENE.md#windows-player-handoff).

The reviewed prior chassis-wheel player is recoverable in `Legacy/20261004-122716-284-windows-final-reference-tyres/` (**313 files, 588.95 MB**). All **313 copied files** match the tested source byte-for-byte, apart from the deliberately updated build note. The copied player passes an offline Golf startup and normal-exit check from the final directory. Current evidence is in `Builds/WindowsFinalHandoff/20261004-122709/`. Source/runtime/assets and APKs are unchanged by this handoff, so no new Unity build or Android-size claim is made. No computer-use or Git publishing action is performed.

The normal readiness check passes portable-path scanning but remains blocked at Git's dry-run by the pre-existing shared `index.lock`. The full check passes using a temporary index copy: **3,383 publishable files, 464 LFS paths and 252 pending operations**. The real index hash is unchanged and the shared lock is preserved. `readiness-normal.txt`, `readiness-index-copy.txt` and `readiness-index-scope.json` record this limit in the handoff evidence directory.

## Golf cart reference tyres — 4 October 2026

The delivery wheels follow the user-approved [side](../ArtSource/Golf/Cart/References/Wheel-side.png) and [tread](../ArtSource/Golf/Cart/References/Wheel-tread.png) references: thick rounded black sidewalls, recessed chevron channels and cream six-spoke rims with a broad central cap. All four rims are **0.38 m diameter** and centred on their rolling pivots. Tyre radii remain **0.339 m front / 0.328 m rear**. The beads share vertices with the rim, and tread channels are part of a closed shell. Every tread block retains the centred circular outer envelope; maximum groove depth is **6 mm**. All twelve wheel meshes pass outward winding, groove depth/spacing, circular/equal rims, closed-surface and welded-bead checks, alongside existing chassis connections.

Mobile totals are **33,958 / 13,805 / 6,721 triangles**, preserving five renderers per LOD and one existing atlas/material. Near/mid/far wheels have **16/10/6 tread rows**; the latter two use fewer axial samples and omit spoke bevels. This removes **1,600 / 960 triangles** against using the full new profile at those distances. No new texture, shader, runtime component or network message is added. The full **246,850-triangle** master, all three body meshes and all three delivery texture byte hashes are unchanged. All sixteen canonical geometry/UV/normal hashes agree after regeneration in a checkout containing spaces, invoked from outside it. Blender image/library and FBX texture dependencies pass format-aware portability checks; retained exporter provenance is inactive.

The matching Windows player passes **203 cart assertions (159 offline, 22 host, 22 client)** at `Builds/GolfCartQA/Run-20261004-114611/`. Checks cover measured wheel radii, 10/25/50 cm starts, reverse, stationary settling, front-only steering, existing 17/8 m/s limits, collision, forty sand-bowl crossings, seat/camera behavior, ownership, travel and two-player replication. Actual side, steering and three-LOD views were inspected, together with close tyre/rolled-rim renders. Evidence, source manifests, map/body integrity and portable regeneration records are in `Builds/GolfCartTreadQA/`. Vehicle gameplay, collision hull and camera logic are unchanged.

The exact **88,963,010-byte** APK passes AndroidSubmission and independent v2 signing and is retained with its full audit in `Builds/GolfCartTreadQA/Release/`. Cart runtime/art/prefab inputs match; generated URP registrations, isolated build-output/cache flags and later Golf-club art/editor/runtime additions and player/network/Golf/material integration changes are recorded in the source comparisons. Those ongoing changes are outside this release's scope; multiplayer verification uses matching captured builds. The shared root APK/audit are preserved. See [size/headroom, capture and recovery](BUILD-SIZE.md#golf-cart-reference-tyres--4-october-2026). No computer-use operation or Git publishing was performed. Phone touch/rendering/FPS and WAN remain unverified.

`Check-TaskReady.ps1 -RequireApk` passes portability and content checks, but its normal Git dry-run is blocked by the pre-existing shared `index.lock`. The same script passes with a temporary copy of the existing index: **3,383 publishable files, 464 LFS paths and 250 pending operations**. The real index hash is unchanged and the shared lock is preserved. Evidence is in `task-ready.txt`, `task-ready-index-copy.txt` and `readiness-index-scope.json` under the task directory. Its default-APK check sees the retained older shared package; freshness, signing and size for this task are established separately by the exact retained release above. Git publishing through the normal index remains blocked while that lock exists.

## Golf cart rim fit and circularity — 4 October 2026

The old delivery had a round tyre around an irregular, disconnected original hub, with smaller rear rims. All four delivery wheels now use **0.47 m six-spoke rims**, centred on their rolling pivots. The front/rear tyre beads share mesh edges with the rim. All **twelve wheel meshes** pass closed-surface, outward-winding, equal front/rear/left/right geometry, concentric lip/tread and uniformly spaced angular-ring checks. Tyre radii remain **0.339 m front / 0.328 m rear**, with **64/40/24 segments**. Retained concurrent fixed axle supports add **288/224/160 triangles** to the body LODs; final totals are **28,838/12,205/5,761** with the same five renderers and shared atlas/material.

The original **246,850-triangle** master, original body surfaces/UVs and all three texture bytes are preserved. This task's geometry/UV integrity hash for the full source is `633717016cb39339837685def044918982fa940decb5e32fe87952f4adaf8ca0`. UV strip packing preserves all twelve wheel geometries while reducing position/UV entries **21,928 → 15,640**. Regeneration and format-aware Blender/FBX dependency checks pass from a checkout containing spaces, invoked outside that checkout. All sixteen meshes retain equivalent canonical surface/UV/normal associations; two mirrored near meshes may differ in raw index ordering after BMesh beveling. Only inactive exporter provenance is retained as historical path metadata.

The final Windows player passes **203 cart assertions (159 offline, 22 host, 22 client)** at `Builds/GolfCartQA/Run-20261004-021152/`. Tests cover forward/reverse wheel movement, 10/25/50 cm starts, stationary body settling, rendered tyre radii, steering, 17/8 m/s limits, ownership, parking, seating, collision, all sand-bowl exits, cameras, scene/travel restrictions and two-player replication. The independent **142-assertion** island regression is at `Builds/GolfG2QA-20261004-005845/`; its course/environment inputs are unchanged by this model refinement. Actual side, turning and three-LOD player views were inspected. These builds retain parallel overhead-camera following, 0.85 visual wheel-roll calibration, supports and driving-HUD changes; this task does not redesign those systems.

The fresh **88,787,790-byte** release passes the strict AndroidSubmission gate and v2 signing. Captured inputs, source comparisons, body/map integrity, packing estimates and before/after evidence are in `Builds/GolfCartRimQA/`. Android regenerates its URP resource registrations, while runtime and art match the working source. Phone touch/rendering/FPS and WAN are unverified. [BUILD-SIZE](BUILD-SIZE.md#golf-cart-rim-fit-and-circularity--4-october-2026) records size/headroom and recovery. No computer-use operation, staging, commit or push was performed.

## Golf cart chassis connections and startup rolling — 4 October 2026

The four wheel hubs now have fixed axles and upright mounts reaching into the chassis. These are merged into the existing body meshes for all three LODs, preserving five active renderers and one shared material per LOD. Totals are **28,838 / 12,205 / 5,761 triangles**, an increase of **288 / 224 / 160**; the original 246,850-triangle source geometry/UV hash and all three delivery texture hashes are unchanged. The format-aware art audit validates twelve axle connections, tyre/rim circles, matching rims, outward normals, welded bead edges, closed wheel surfaces and all active relative dependencies. It also passes from the existing checkout with spaces, launched from an unrelated working directory. The current FBX has no retained machine-path provenance fields. Reports and low-angle/full-steering views are in `Builds/GolfCartWheelContactQA/`; standard close LOD views remain in `Builds/GolfCartQA/Art/`.

Wheel rotation follows the displayed ground path using vehicle heading and ground tangent. Body tilt or vertical settling cannot add rotation while stationary; blocked motion and summon/rebase discontinuities retain their existing protections. The default **Wheel Roll Scale = 0.85** intentionally slows the visual rotation 15%; **1.0** restores the exact tyre-circumference ratio. This does not change cart speed, ground clearance or authoritative movement. Configuration and regeneration are explained in [GOLF-CART.md](GOLF-CART.md) and [the source guide](../ArtSource/Golf/Cart/README.md).

The matching Windows development player passes **203 cart checks: 159 offline, 22 host and 22 client**, including **28 new observable wheel assertions**. A 10 cm start produces **14.36269°** on the front wheels versus **14.36617°** expected at the configured scale; 25/50 cm starts and all four reverse paths pass the 0.5° tolerance. A 15° body pitch plus 25 cm vertical settling leaves wheels stationary, as does a target speed of 17 m/s without displacement. Rendered radii measure **0.3390006 / 0.3280007 m**, agreeing with the configured 0.339/0.328 m. Existing wall/shore containment, forty real sand-bowl crossings, seat pose, camera modes, steering, ownership, interpolation and two-player loopback replication pass. Evidence: `Builds/GolfCartQA/Run-20261004-015517/`. Native near/far LOD and steering views were inspected in addition to the Blender mount views. Phone touch, mobile rendering/FPS and WAN are not validated by this Windows run.

The matching final **88,782,546-byte** release APK passes AndroidSubmission and independent v2 signing, with **11,217,454 bytes** of strict-limit headroom. All 2,925 captured inputs remain stable during final packaging and match the primary workspace except the isolated editor clean-cache option. The first package was superseded after a concurrent model re-export; final art, Windows probes and Android package use the same current FBX. Current APK/audit/player are under the standard ignored delivery paths, with an exact retained copy in `Builds/GolfCartWheelContactQA/Release/`. See [measurements and source consistency](BUILD-SIZE.md#golf-cart-chassis-connections-and-startup-rolling--4-october-2026). Superseded task/shared outputs and source backups are recoverable in `Legacy/20261004-020924-883-golf-cart-wheel-contact/` (378 files, 814.16 MB). No active source asset, cache or unrelated work was archived; staging and publishing remain with the user.

## Golf cart elevated steering follow — 4 October 2026

The elevated driving camera now uses the same displayed-heading delta as first-person and third-person, preserving manual look offset. Left steering turns the view left and right steering turns it right. Its 58° downward angle and 10 m distance remain unchanged. Reverse-facing smoothing remains specific to third-person. Presentation still runs after cart interpolation and applies only to the local driver; normal walking, exit and island-switch camera behavior remain intact. See [controls and camera behavior](GOLF-CART.md).

The fresh Unity 6000.3.20f1 Windows player passes **175 cart checks: 131 offline, 22 host and 22 client**, including the existing sand-bowl traversal checks. Actual elevated measurements are **+9.20° cart / +9.20° camera** on right input and **−8.80° / −8.80°** on left input. The driving client's interpolated left turn measures **−18.53° / −18.53°**; the host's own overhead view does not rotate with that remote driver. Checks also cover manual drag/offset, switching between all three modes, existing first/third-person reverse transitions, exit, movement/jump restoration, authority, collisions and cleanup. The native elevated capture was inspected. Evidence: `Builds/GolfCartQA/Run-20261004-005627/`; build/input records: `Builds/GolfCartOverheadQA/`.

The same player passes **81 Golf gameplay checks** in `Builds/GolfMiniGameQA/Run-20261004-005628/`, including independent progress, owner-based helper completion, incorrect cups/out-of-bounds recovery, local indicators, stroke/time results and the real shared 30-second countdown ending in replicated DNF. No new art or package is introduced by camera following. The fresh **88,787,118-byte** AndroidSubmission and independent v2 signature pass; all **2,925 captured inputs** remain stable and match the workspace apart from the isolated clean-cache editor option. Physical-phone touch, rendering/FPS and WAN remain unverified. The measured release and archive are recorded in [BUILD-SIZE](BUILD-SIZE.md#golf-cart-elevated-steering-follow--4-october-2026).

## Golf cart sand-bowl rim traversal — 4 October 2026

The reported blockage concerns driving out of the sand bowl. The pre-fix player reproduces four failures on two real exits: Bunker 4 westbound stops at **32.867 / 43.2 m**, and Bunker 5 eastbound at **26.633 / 36 m**, with equivalent reverse failures. PhysX returns **zero-distance, horizontal-normal** hits against the terrain sectors when the level body sweep starts inside an uphill triangle. Four-wheel support accepts the same ground. The motor now excludes terrain from its obstacle sweep and retains wheel-based slope, height and sea-level validation, solid-prop/cart collision and shoreline boundaries.

The fresh Windows player passes **169 assertions** (**127 offline, 21 host, 21 client**) at `Builds/GolfCartQA/Run-20261004-002512/`. This includes **40 full traversals** of all five real sand bowls, east/west/north/south, driving forwards and in reverse. The formerly blocked exits now reach **43.067 / 43.2 m** and **35.983 / 36 m**. Existing large-step wall collision, twelve shoreline directions, slope parking, 17/8 m/s limits, seated roof/footwell clearance, both character LODs, circular wheel LODs, cameras, recall, travel, ownership and two-player replication also pass without runtime exceptions. Actual side/rear player views retain round, clean tyres and the corrected seated pose.

Reproduction, diagnostics, source comparisons and build evidence are in `Builds/GolfCartRoundFixQA/hill-*`. This code change adds no art, texture, material or package. The **88,708,270-byte** APK passes AndroidSubmission and independent v2 signing. All 2,925 captured inputs match before packaging apart from the isolated editor's clean-cache option; later parallel rim/bead geometry is outside this captured APK/Windows player. The tested deliveries and fresh packing are at `Builds/GolfCartRoundFixQA/Release/`. Portable Blender/FBX and original-master integrity evidence from the seating/tyre entry below applies to the captured art. Physical-phone controls, rendering/FPS and WAN remain unverified. See [bytes, source scope and recovery](BUILD-SIZE.md#golf-cart-sand-bowl-rim-traversal--4-october-2026). Superseded deliveries and the stale-lock backup are archived in `Legacy/20261004-004451-752-golf-cart-rim-delivery/`. An abandoned, unchanged empty Git mutex was proven unheld and removed; repository readiness then passed. No Git staging or publishing was performed.

## Golf mini game — 4 October 2026

The five-hole game extends the existing island, shared athlete commands, camera, room authority and ball art. All five cups remain open. Player progress, stroke ownership, owner-based completion, recovery, countdown, DNF and ranking live in gameplay classes; the leaderboard and indicators read state. See [rules, controls and class responsibilities](GOLF-MINIGAME.md).

The final Unity 6000.3.20f1 Windows player passes **81 Golf checks: 58 offline, 12 host and 11 client**, including two existing room-readiness checks. Evidence is `Builds/GolfMiniGameQA/Run-20261004-001418/`. A real owner RPC hits another player's ball, preserves ownership and charges only the hitter. The host simulates actual motion and cup entry; the owner advances while the helper's target remains unchanged. Both peers observe different targets, wrong-cup reset poses, continued play after the first finish, and replicated DNF after an actual 30-second wait. Offline checks cover all four incorrect cups, shoreline recovery without a penalty, downward cup capture versus fly-over, all five active triggers, target/ball indicators, the presented reverse-camera bearing, touch hold/release/cancel, clean round/sport transitions, results by strokes then time, exact ties and early all-finished termination. Five hundred fractional-clock cases prevent an initial `00:31` display.

The rendered view was inspected with the five leaderboard column labels, local target arrow/distance, own-ball marker and existing movement/cart controls. Runtime balls share the two authored LODs without cloning nested equipment LOD groups. The original equipment display still reports its pre-existing nested-LOD warning; the new live balls have one group and no duplicate registration.

Regression results are **509 football checks**, **142 existing Golf map checks**, **128 cart checks** (86 offline, 21 host, 21 client) and **67 basketball interaction checks**. Reports are `Builds/FootballMatchQA/Run-20261003-231025/`, `Builds/GolfG2QA-20261003-231234/`, `Builds/GolfCartQA/Run-20261004-000952/` and `Builds/GolfMiniGameQA/Basketball-Offline-20261004-001024/`. Existing character movement, football matches, basketball pickup/shoot/recovery, cart authority/cameras and island switching pass. The final Golf suite also ran with default paths from a second checkout containing spaces, while invoked from an unrelated working directory.

Windows validation used a full build followed by scripts-only updates for Golf fixes and UI labels. A later development-only cart terrain fixture is outside this Windows player's captured scope; no new terrain behavior is claimed. Android packaging resynchronized the complete source snapshot; its **2,925 captured inputs stayed unchanged**. The fresh **88,708,082-byte** release passes AndroidSubmission and independent v2 signing. Subsequent parallel cart-rim FBX/motor/probe updates are outside that captured APK. Physical-phone touch, rendering/FPS and WAN remain unverified. Release bytes, input hashes, signing and cleanup are recorded in [BUILD-SIZE](BUILD-SIZE.md#golf-mini-game--4-october-2026).

## Golf cart driving cameras — 3 October 2026

First-person and third-person views follow the displayed cart heading one-to-one while preserving manual look offset. Third-person reverse input smoothly turns the view backwards; forward input smoothly returns it forwards. First-person never adds the reverse turn. Elevated view keeps its world heading through turns and reverse. Camera presentation updates after client cart interpolation, applies only to the local driver, and clears after exit, sport switching and travel. Inspector settings and behavior are described in [GOLF-CART.md](GOLF-CART.md).

The fresh Windows player passes **128 cart assertions** (**86 offline, 21 host, 21 client**) at `Builds/GolfCartQA/Run-20261003-231510/`. With 20% right steering, cart/camera changes both measure **+9.20°**; with 20% left steering, both measure **−8.80°**, in each following camera mode. Manual 20° offsets and an additional 13° look drag remain intact. First-person reverse at **−8.000 m/s** adds **0.00°** rear-view offset. Third-person reverse measures **10.59°** after the first 0.15 s and **179.89°** after settling; forward return measures **170.11°** initially and **0.11°** after settling. The driving client's interpolated cart/camera turn both measure **8.97°**, while another player's turn does not rotate the host's walking camera. Original 17/8 m/s limits, wheel motion, collisions, shoreline, seating, exits, recall, ownership and scene/travel restrictions pass.

An independent **142-assertion** golf traversal/camera/environment regression passes at `Builds/GolfG2QA-20261003-230821/`. Native first-person/third-person reverse, forward, turning and elevated captures were inspected. Measurements sample cart and camera together after LateUpdate; the first attempt mixed a current cart pose with the previous camera frame and produced false comparison failures. The final fixture fixes sampling without relaxing bounds or changing runtime following. Current build/source/test records are in `Builds/GolfCartCameraQA/`; Android measurements are recorded in [BUILD-SIZE.md](BUILD-SIZE.md). The combined source also retains concurrent golf rules and cart seating/tyre refinements. No physical-phone touch, clipping/FPS or WAN verification is claimed.

The **88,713,562-byte** Android camera snapshot passes the submission gate and independent v2 signature. Its 2,925 captured asset/package/settings inputs are unchanged in the isolated checkout; camera and vehicle files match the primary workspace. Later parallel golf-ball reset reception and development-probe edits are outside this APK's scope. The tested Windows camera player is in `Builds/GolfCartCameraQA/Player/`; the original shared Windows player is preserved for the other task. Superseded output, initial sampling failures and the full-disk build attempt are archived in `Legacy/20261003-235321-846-golf-cart-camera/`. The successful retry retained original caches/dependencies and compressed only this task's copied Library without changing contents. No Git publishing or computer-use interaction occurred.

## Golf cart seating and circular tyres — 3 October 2026

The fixed ankle target exceeded the sprinter's short leg reach, clamping knees almost straight. Seating now derives reachable ankle targets from thigh/shin lengths and restores the neutral forward foot orientation. The hip marker moves from 1.10 to **1.04 m**. Actual player checks measure knees around **90.75° / 90.78°** and the complete baked avatar top at **2.272 m**, beneath the **2.33 m** roof-collider underside. Both character LODs and host/client seating pass; walking and jumping restore after exit.

All twelve wheel meshes have concentric circular tread rings, with radii **0.339 m front / 0.328 m rear** and 64/40/24 angular segments across LODs. The art audit rejects vertices outside the tyre radius, uneven tread spacing and inward tyre/hub normals. Dark fragments below both rear wheel wells are removed. Delivery counts are **25,833 / 9,686 / 3,748 triangles**; the full 246,850-triangle source, source UVs and three delivered texture hashes remain unchanged. Geometry/UV integrity hash: `963e86508f7ef7a5d9f63f15495dc9f9908b0097b36facae360868fbdfa5616a`.

The combined-source cart replay in `Builds/GolfCartQA/Run-20261003-231835/` passes **128 assertions** (86 offline, 21 host, 21 client), including entry, moving/turning pose, both character LODs, footwell/roof clearance, owner restrictions, collision, shore containment, cameras, travel, recall and pose restoration. It also includes concurrent camera work: sampled car/camera turns match at 9.21° right and about 8.80° left, and first-person reverse retains a 0.00° heading offset. The camera fixture now samples both completed presentation poses in LateUpdate; coroutine timers previously mixed the current cart with the preceding camera frame. Actual side/front/rear and three cart LOD views were inspected; close left/right rear-wheel renders are in `Builds/GolfCartRoundFixQA/`. Active Blender/FBX references resolve in an alternate checkout containing spaces, including execution from an unrelated working directory. The format-aware audit permits only inactive `FBXHeaderExtension/SceneInfo` exporter provenance as historical path metadata. Android/combined-source verification is recorded in [BUILD-SIZE.md](BUILD-SIZE.md); phone rendering/FPS/touch and WAN remain unverified.

The historical **88,707,818-byte** vehicle APK passes AndroidSubmission and independent v2 signing. Its 2,925 captured inputs include the tested vehicle changes; parallel edits to `GolfBall`, `GolfMatchState` and `GolfMatchProbe` during packaging are outside this snapshot. The superseded Windows player and APK/audit are now recoverable in `Legacy/20261004-004451-752-golf-cart-rim-delivery/Builds/GolfCartRoundFixQA/Release/`, preserving the other task's shared deliveries. See [measured size, source scope and recovery](BUILD-SIZE.md#golf-cart-seating-and-circular-tyres--3-october-2026). Superseded backups and trial runs are archived in `Legacy/20261003-235959-283-golf-cart-round-seated-cleanup/`; the obsolete inspection helper is in `Legacy/20261003-220432-659-golf-cart-inspect-helper/`. No Git staging or publishing was performed.

## Golf cart rear-wheel rendering and steering — 3 October 2026

The original rear hub faces had been flipped inward by recalculating normals on extracted open wheel meshes. Unity culled their outer surface, exposing a broken pattern. The extraction helper now preserves original face winding; all **12 wheel/LOD hub orientation checks** pass, and actual Windows views show the original rear spoke pattern in all three LODs. Vertex positions, triangle budgets (24,000/8,000/2,500), texture hashes and the high-quality source geometry/UV hash stay unchanged. The regular art audit now rejects inverted hubs. Relative Blender/FBX dependencies pass in the main and alternate checkout.

Steering is **120°/s maximum** instead of 72°/s and front-wheel steering is **38° maximum** instead of 28°. Throttle uses stick magnitude with existing forward/reverse intent, so diagonal steering does not reduce drive speed. The real Unity player measures **17.000 m/s straight and turning**, **8.500 m/s for both half-stick cases**, and **8.000 m/s reversing through a turn**. The normalized turning input produces **72.56° heading change in 0.8 seconds** and **28.72° displayed front-wheel steering** (maximum input X gives the full 38°).

The fresh Windows player passes **90 cart assertions**: **56 offline**, **17 host**, **17 client**. Existing rolling/reverse/stop, steering return, collision sweeps, slope parking, shoreline, UI, seat/exit controls, cameras, sport switching, ownership and two-player steering/pose replication pass. Cart speed limits remain 17/8 m/s; protocol remains 19 with no packet layout change. Evidence: `Builds/GolfCartQA/Run-20261003-212527/` and `Builds/GolfCartHandlingQA/`. The source-only course/grass inputs retain the earlier golf-suite coverage; no new course change or physical-phone touch/rendering/FPS/WAN verification is claimed. Fresh **88,625,838-byte** AndroidSubmission, complete packing and v2 signing pass; all 2,890 publishable asset/package/settings hashes match except isolated editor output/clean-cache options. Measurements and recovery records are in [BUILD-SIZE.md](BUILD-SIZE.md#golf-cart-rear-wheel-rendering-and-steering--3-october-2026).

## Golf course refinement — 3 October 2026

Five aqua numbered flags reuse the original map material. Holes 1/2/3 are 160.6–214.6 m apart across the south, west and northeast, with hole 4 on the southeast coast and hole 5 on the northern high ground. Cup diameter is 0.285 m, 1.5× the initial opening. The original terrain itself supplies the surrounding grass; separate colour overlays are removed. Both streamed scene and reusable prefab retain five real recessed cups, grounded tees, sixteen terrain sectors and the shared grass-clearing metadata. See [coordinates and authoring](GOLF-COURSE.md).

Unity 6000.3.20f1 passed **235 assertions** across the scene, prefab and repeat generation, and **78** after reopening the saved scene. Checks cover material identity, pair separation, enlarged physical openings, inner-wall/floor collision, nearby solid ground and source-terrain positions/normals/UVs. There are 47–48 terrain samples per green; position errors remain below 0.0001 m and UV errors below 0.000001. Actual saved-scene and running-player near/distant views were inspected. Six original terrain/grass/structures/colour/aqua/Blender masters match Git HEAD LFS hashes.

Desktop runs at `Builds/GolfG2QA-20261003-192847/` and `Builds/GolfG2QA-20261003-195311/` each passed **142** assertions; Android-geometry Windows evidence at `Builds/GolfG2QA-20261003-193449/` passed **142**, with **13,122** unchanged analytic terrain samples. Test defaults resolved from their script location while invoked from an unrelated temporary directory; player/checkouts contain spaces. Later cart-only inputs did not change the course/grass configuration or generated map assets. These golf checks do not claim cart behavior coverage; its final 87-assertion validation is documented separately below.

The final combined AndroidSubmission passed its strict gate and independent v2 signing at **88,624,542 bytes**. All 2,903 authored asset/package/settings inputs match apart from the isolated Android clean-cache option. Build/source/signature/packing evidence is in `Builds/GolfCourseRefinementQA/` and `Builds/SizeAudit/latest/`; see [measured release and unresolved phone/development-budget limits](BUILD-SIZE.md#golf-course-refinement-and-current-combined-release--3-october-2026). No physical-phone touch, quality/FPS, WAN or golf shot/scoring validation is claimed.

## Golf cart speed and wheels — 3 October 2026

The existing cart now drives at 2× its original forward/reverse speeds and animates the four supplied wheels. Unity 6000.3.20f1 passed **87 cart assertions**: **53 offline**, **17 host** and **17 client**. Actual player measurements are **17.000 m/s forward** (24.435 m in the 1.5-second probe) and **−8.000 m/s reverse** (8.054 m in 1.2 seconds). Measured rear-wheel rotation is **4,247.1° forward** and **−1,384.6° reverse**, following travelled distance. Checks cover all four axle pivots/three LODs, front steering, straight rear axles, stationary wheels, steering return, swept wall collisions under large simulation steps, slope parking and twelve shoreline directions. Existing buttons, seated pose, exit walking/jumping, all camera modes, owner restrictions, other-island restrictions, shared travel and disconnect cleanup also pass.

The two-player loopback run verifies host authority, a guest driving the host's cart, replicated front-wheel steering and seated pose on both peers. Protocol **19** includes the steering angle. A separate **142-assertion headless golf run** passes the existing cups, numbered flags, tees, spawns, shoreline/fairway/bunker collision, cameras and environment switching. Actual Windows captures of turning and all three cart LODs were inspected. No physical-phone touch, rendering/FPS or WAN behavior is claimed.

Cart evidence: `Builds/GolfCartQA/Run-20261003-202128/`; golf report: `Builds/GolfG2QA-20261003-202344/`; task build/input/art evidence: `Builds/GolfCartWheelsQA/`. Format-aware Blender/FBX checks pass both in the main checkout and from an unrelated working directory against a second checkout whose path contains spaces. The high-quality 246,850-triangle source stays intact; mobile totals remain 24,000/8,000/2,500 triangles with one body plus four wheel parts per LOD, unchanged maps and relative active dependencies. Recorded FBX `SceneInfo` exporter provenance is inactive metadata, not an external asset dependency.

Validation caught and fixed connected FBX instances rejecting wheel reparenting; the final prefab generator unpacks the model before assigning axle pivots and checks each assignment. An earlier simultaneous second graphics-player run crashed in D3D12; final cart rendering ran alone and the separate golf collision suite used headless mode. A concurrent source rollback removed slope-heading stability after the Windows build; the tested motor and slope regression were restored, with exact original tested hashes, before rebuilding Android. All 84 tested runtime source hashes match the final workspace. The final **88,625,346-byte** APK passes AndroidSubmission, complete packing and independent v2 signing; 2,890 publishable asset/package/settings hashes match except isolated editor output/clean-cache options. Only the corrected build's passing evidence is claimed. Android packaging and recovery records are in [BUILD-SIZE.md](BUILD-SIZE.md#golf-cart-speed-and-wheels--3-october-2026).

## Golf cart summon and driving snapshot — 3 October 2026

The initial protocol 18 cart player passes **76 assertions**: **44 offline, 16 host and 16 client**, including room-readiness markers. Reports and final close/distant camera/LOD captures are in `Builds/GolfCartQA/Run-20261003-192340/`. The probe exercises the actual Chinese summon/recall and drive/leave buttons, plus pointer-driven joystick input. It verifies one cart per player, blocked duplicate/invalid placement, Golf-only availability, owner-only recall, public empty-cart driving, single-seat occupancy, distance rejection, forward/reverse/steering/braking, obstacles, twelve shoreline directions, seat alignment, three camera modes, parked persistence, safe occupied recall, driver locomotion restoration, sport switching/return and owner disconnect cleanup. Host/client checks confirm authoritative actions and replicated guest driving of another player's cart. The script also ran from an unrelated temporary directory against a player path containing spaces.

The same base player passed these regressions with no failed assertions or runtime exceptions:

| Suite | Passing assertions | Evidence |
| --- | ---: | --- |
| Football match, physics, fake shots and ball replication | 509 | `Builds/FootballMatchQA/Run-20261003-194404/` |
| Basketball local host/client gameplay | 30 | `Builds/BasketballGameplayQA/Local-20261003-193027/` |
| Shared jump/momentum | 140 | `Builds/JumpQA/Run-20261003-193807/` |
| Five-hole golf traversal and scene switching | 142 | `Builds/GolfG2QA-20261003-193039/` |
| Shared Football → Golf → Football travel | 18 | `Builds/GolfCartQA/TravelRegression/` |

Football's physics probe now consistently uses batch mode with graphics enabled. A preceding headless run could not validate the transparent aim material; a non-batch capture run changed the simulated pointer fixture's viewport and produced a false fake-shot failure. The final 509-check run retains the graphics device without that viewport resize. Normal light/full kicking, cancellation and existing measured ball speeds all pass. Sport force, charge and ball-physics tuning were not altered by the cart integration.

Blender topology/delivery LOD checks, seated pose and close/distant player renders passed. The final path-only FBX correction was checked with typed format-aware comparison; non-path mesh/texture payloads are preserved. Relocated base art dependencies and font generation pass from a path containing spaces and an unrelated working directory. Full source masters remain editable outside Unity. The eight Chinese glyphs, renamed subset family and shipped OFL licence were verified.

The prescribed main AndroidSubmission attempt exited on the open project; the same script and target succeeded in the isolated checkout. The final **88,756,434-byte** release APK, complete audit and separately verified v2 signature are retained as `Builds/GolfCartQA/ReleaseBase/`. See [size and archive records](BUILD-SIZE.md#golf-cart-summon-and-driving-snapshot--3-october-2026). This snapshot predates concurrent course refinement and protocol 19 speed/wheel work; it does not certify those later source edits or shared-root APK freshness. Phone touch, phone quality/FPS and WAN latency remain unverified. No Git staging, commit or publishing was performed for this task.

The final `Check-TaskReady.ps1 -RequireApk` check passes **3,324 publishable files / 452 LFS paths**. Documentation links resolve, `git diff --stat` and `git status --short` were reviewed, and unrelated course/speed/wheel changes remain in place. This hygiene check reports the shared-root APK's size only; the immutable base artifact and separate signature/audit evidence establish this task's measured release. Readiness evidence is `Builds/GolfCartQA/readiness-final.txt`.

## Five-hole golf map — 3 October 2026

The map adds five independent recessed cups, orange numbered flags, putting greens and tee markers following the entrance/left bunker/central fairway/coast/raised-green layout. Existing teal flags remain scenery. The streamed scene and environment prefab both retain the course, and the refined-island generator places it on regeneration. Golf shot controls, scoring, shared travel and room protocol are unchanged. See [course authoring](GOLF-COURSE.md).

Unity 6000.3.20f1 passed **160 editor assertions** across the actual scene, environment prefab and repeated generation: five unique flags/tees, sixteen retained terrain sectors, open terrain at each cup, recessed floors, inner-wall collisions, nearby solid ground, upward green normals, terrain clearance and complete material dependencies. Native mesh inspection confirms all **3,021 putting-surface triangles** face upward and retain complete circular areas. Original terrain/grass/structures FBXs and the Golf G2 Blender master exactly match Git HEAD.

The Windows build with Android geometry processors passed **142 golf assertions** with five real cups and cleared greens. Its 49 golf grass cells/147 meshes prepared and appeared in the running player. The final desktop player also passed **142 assertions**, including ten spawns, 48-direction shoreline containment, full fairway traversal, all bunker exits, camera behavior and Football/Basketball/Golf switching. Actual close/distant renders and five-hole views were inspected. The analytic grass-height function passed **13,122 independent samples**, with maximum error **1.7763568394002505e−15 m**. The test script ran from an unrelated temporary working directory against a player path with spaces.

Evidence and final source hashes: `Builds/GolfFiveHolesQA/`; desktop run: `Builds/GolfG2QA-20261003-171939/`; mobile-geometry run: `Builds/GolfG2QA-20261003-165611/`. The mobile geometry/collision run predates the material-only removal of an unused turf mask; final editor/desktop rendering and the Android audit cover that removal. All authored release inputs match the workspace, apart from the isolated editor's Android clean-cache option. Complete-source AndroidSubmission and APK v2 signing passed with **86,896,050 bytes**. See [size and archive provenance](BUILD-SIZE.md#five-hole-golf-map--3-october-2026). No physical-phone rendering/FPS, touch, golf scoring or shot-controller validation is claimed.

## Football animation rebuild — 4 October 2026

Rebuilt football presentation on the existing Rainbow Sprinter model: one distance-driven gait, coordinated shoulders/elbows/wrists, world-space support feet, gradual starts/stops/turns, swing-phase dribble contact, complete-pose transitions, jump/landing, directional falls and interrupted recovery. The 28 saved records are compact posture/action timelines combined with procedural motion, not 28 imported mocap clips. [Implementation and reproduction](FOOTBALL-ANIMATION.md).

The final checked source passes **2,116 assertions** across the following suites. Repeated rates and scenarios are included in that total; it is not a count of distinct animations or a quality score.

| Suite | Passing assertions |
| --- | ---: |
| Motion at 20 / 30 / 60 / 120 timeline FPS, both LODs | 292 / 292 / 296 / 292 (1,172) |
| Rendered football match, physics and host/guest replication | 532 |
| Shared jump gameplay and loopback | 140 |
| Shared tackle gameplay and loopback | 210 |
| Final shared turning gameplay and loopback | 62 |

Across the four motion runs, maximum high-weight dribble contact error is **2.13 cm**, maximum sampled rendered floor penetration is **1.35 cm**, and maximum live local joint speed is **2,637.72 degrees/second**, below the unchanged 2,700-degree regression ceiling. Walking records **17 / 28 / 59 / 123** consecutive support samples with zero horizontal displacement at the trace's recorded precision. Low-rate sampling can miss the short sprint stance; zero slip without support samples is not evidence of a good plant. Bone-length error remains below 0.1 mm. Ten athletes allocate **zero steady-frame pose bytes**; measured Windows pose work peaks at **0.711–0.809 ms**, excluding Animator, physics and rendering. This is not phone FPS.

Actual Unity renders were inspected for locomotion, dribbling, cuts, stops, jumps, slides, directional falls and recovery, plus the two LOD pose sheets. The final video is **800 × 688, 60 FPS, 86.4 seconds**, with normal and half-speed playback and original shared clips for comparison. The exporter now retains chronological repeated stages and uses the recorded capture rate. Current footage, sheets and trace: `Builds/FootballMotionQA/Run-20261004-224406-424/`. Other final rates are indexed by `Builds/FootballMotionPolishQA/final-runs.json`; numeric summaries are in `motion-results-summary.json` and the run-local `continuity.json` files. The playable Windows build remains under `Builds/WindowsFinal/`.

Gameplay evidence: football `Builds/FootballMatchQA/Run-20261004-224456/`, jump `Builds/JumpQA/Run-20261004-224456/`, tackle `Builds/TackleQA/Run-20261004-224611/`, and final turning `Builds/TurnQA/Run-20261004-225849/`. The first turning run failed two Golf LOD1 cases after accumulated route travel; a previous-player control and isolated rerun passed those cases. The rerun exposed a separate obsolete base-clip assertion during a football hand-off. The development-only turn probe now samples the final displayed pose after animation evaluation, checks the football rig/weight/finite feet or an active/incoming shared clip, and repeats the same starting route for each LOD. Travel, speed, state-change, first-person and replication assertions remain. The corrected final suite passes; initial reports and comparison evidence are retained.

Both Python review helpers passed with default root/run selection from a copied checkout path containing spaces and an unrelated working directory. No character mesh, rig, texture, material, imported clip or package changed. The new `FootballGait.cs` file has Unity metadata; the pose resource is retained as active source. The 32 recorded football source/resource inputs match the tested Windows motion and Android build; the later turn-probe edit is wholly excluded from nondevelopment players. Its hash is recorded separately. Portable path and task-readiness checks passed without staging or committing files.

AndroidSubmission, the strict size gate and APK v2 signing passed: **86,764,912 → 86,774,768 bytes (+9,856)**, leaving **13,225,232 bytes** hard-limit headroom. The **75 MB development target remains unmet**. The preserved APK is `Builds/FootballMotionPolishQA/Android/WhatTheFish-release.apk`; [size audit and exact build scope](BUILD-SIZE.md#football-animation-rebuild--4-october-2026) record its hash and contributors.

This is a stylized short-legged character at unchanged gameplay speeds. Numerical gates and frame inspection do not certify human biomechanics, every self-intersection or a subjective "top-class" rating. No Android device was connected, so physical-phone appearance, touch, sustained FPS, thermals and WAN playback remain unverified.

Reviewed superseded captures, failed iterations/logs and the completed portability fixture were archived with hashes and recovery paths in `Legacy/20261004-225208-764-football-motion-polish-iterations/`. Current review evidence, player/APK, active source, authoring masters and Unity caches remain. No Git staging, commit, push or history rewrite was performed.

## Transparent lagoon and three fish sizes — 3 October 2026

Implemented transparent moving lagoon water, a sandy basin with animated caustics, and one gold-and-teal fish in **0.4 / 0.9 / 1.8 m** variants. All sizes share the same meshes/material; the two mobile LODs contain **3,192 / 1,248 triangles**, with the **12,792-triangle editable master** retained outside Unity. Casting, catching, swimming AI and scoring are deferred. [Design and rebuild instructions](FISHING-PRESENTATION.md).

The final Windows player passes **30 rendered presentation checks**: fish lengths, shared resources and vertex colours, supported shaders, retained five fishing stands, no new colliders, one active island, three gameplay camera poses, fish visibility through water, motion at a fixed camera, and ocean-cutout cleanup/restoration across island reload. Pier, underwater, overhead, shoreline/inlet, actual game-camera and size-lineup renders were inspected. Fish sit beside the first pier; railings can occlude individual fish from some camera angles. The first-person pier view clearly shows the size gaps. Current evidence: `Builds/FishingWaterQA/Review-Delivery/`, `lagoon-final-preview.mp4`, and `Player/WhatTheFish.exe`. Raw video frames were archived after encoding the final preview.

The older fishing-route probe was corrected to wait for streaming completion, count active islands rather than retained inactive scene entries, and reacquire the lagoon after reload. It passes **50 assertions with 3 failures**: `SPAWN_FLOOR_3`, `SPAWN_FLOOR_4`, and `FULL_COASTAL_LOOP_AND_BRIDGE`. An explicit `-WithoutPresentation` control run produces the same failures and stuck coordinates **(35.87, 1.33, 2.87)**. These existing route/collision issues remain unresolved; no assertion or shoreline collider was weakened to hide them. Both reports are retained in `Builds/FishingWaterQA/RouteChecks/`.

Blender regeneration also passed from a copied checkout path containing spaces, invoked from an unrelated system temporary working directory. Reopening the master and parsing FBX dependency elements found no external image, linked-library or active file dependencies. Generator outputs, `.meta` files and integrity hashes are retained. Saved-file/exporter provenance is nonfunctional metadata; binary bytes were not blindly replaced. New Markdown links resolve within the repository. The final fishing runtime/editor/shader/mesh/material/prefab inputs match the isolated build; source hashes are in `Builds/FishingWaterQA/final-source-hashes.json`.

The review script also repeated **all 30 checks successfully** from a copied checkout path containing spaces, invoked from an unrelated system temporary directory with both player/output defaults. Its report/log are in `Builds/FishingWaterQA/PortableVerification/`. Eight new relative documentation links and both authoring/delivery integrity hashes pass. `Check-TaskReady.ps1 -RequireApk` passes, including portable text paths, metadata, LFS and publishable-file hygiene; it inspected the retained concurrent root APK separately from this task's measured package.

The first main-project Android build failed amid concurrent football source edits. Frozen-checkout AndroidSubmission, strict budget enforcement and v2 signature verification then passed: **86,757,976 bytes**, **13,242,024 bytes** hard-limit headroom. The task APK/audit is retained separately; the concurrently produced root APK was preserved. The **75 MB development target remains unmet**. See [exact baseline/delta, SHA-256 and build scope](BUILD-SIZE.md#transparent-lagoon-and-three-fish-sizes--3-october-2026). This does not validate later football edits. Physical-phone appearance, touch, sustained FPS and thermals remain unverified.

Reviewed superseded previews, intermediate packages/audits and completed portability fixtures were archived with hashes/recovery paths in `Legacy/20261003-230218-819-fishing-presentation-final-cleanup/` and `Legacy/20261003-230828-261-fishing-portable-validation/`. Earlier task batches are `Legacy/20261003-221450-901-fishing-water-baseline/`, `Legacy/20261003-223827-819-fishing-water-failed-audit/`, `Legacy/20261003-224342-327-fishing-water-shared-sand/`, and `Legacy/20261003-225548-346-fishing-water-sand-final/`. Current evidence, active art and Unity caches remain. No staging, commit, push or history rewrite was performed.

## Position-aware goal net — 3 October 2026

Both goals deform at the authoritative ball's actual contact location, using incoming direction and normal impact speed. Back, sides and sloping roof share a continuous response with fixed frame/ground attachments. Scoring keeps the approved nominal 0.65-second ball follow-through before kickoff; a golden goal decides the result immediately and then freezes at the net. See [implementation, limits and reproduction](FOOTBALL-PROTOTYPE.md#position-aware-goal-net--3-october-2026).

The final source passes **1,558 assertions** across two rendered net runs and the rendered football suite:

- **513 net checks**, repeated as **513 copied-path checks**: 224 sampled panel locations across both goals, 12 real PhysX shot cases, varied speeds/directions, high/low impacts, roof/back continuity, pinned supports, bounded overlapping waves, rest/separating rejection, deduplication, invalid event rejection, goal movement and reactivation cleanup. Six 36-frame physical-shot sequences were rendered and visually inspected; `Builds/FootballNetQA/goal-net-impact.gif` and `.mp4` show the localized deformation.
- **532 football checks**: offline matches **259**, match host **16**, match guest **15**, physics/possession/fake shots **199**, network ball host **26**, network ball guest **17**. These verify net contact before reset, paused match clock, no duplicate scoring/new kicks, immediate golden-goal result, post-impact freeze, host/guest impact agreement and rejection of guest-authored collisions, plus retained gameplay coverage.

Initial collision traces exposed an inverted collider-relative velocity vector; the callback now converts it to the ball's incoming direction. Actual contact positions/directions pass after correction. Development-only fixture repairs read the displayed cancel-rectangle position after layout, wait for streamed sport activation rather than a fixed one-second delay, and allow numeric tolerance for the kickoff deadline. A headless shader-property run is not rendering qualification; final gameplay checks ran with graphics enabled. These fixture edits do not change the release game. The seven relevant runtime/material/shader files match the inputs verified before the Android build; hashes and comparison are retained in the task evidence.

Current net evidence: `Builds/FootballNetQA/Run-20261003-223537/` and `PortableVerification/Net/`. Complete final gameplay/loopback evidence: `Builds/FootballNetQA/PortableVerification/Match/` (original run `Run-20261003-225034`). The repeat used a copied checkout path containing spaces, invoked from an unrelated system working directory. The current review player is `Builds/FootballNetQA/Player/WhatTheFish.exe`; `ProjectBuilder.BuildFootballNetReview` rebuilds it with fast development compression and the configured delivery render pipeline. A concurrent shared Windows build supplied the earlier desktop preview; later dedicated builds verified the final fixtures.

Main-project AndroidSubmission and APK v2 signature verification passed: **86,574,958 → 87,028,060 bytes**, leaving **12,971,940 bytes** below the hard limit. The 75 MB development target remains unmet. The exact tested release is retained at `Builds/FootballNetQA/Android/WhatTheFish-release.apk`, with matching logs, baseline, populated packing audit and ZIP/asset comparison. See [size provenance](BUILD-SIZE.md#position-aware-goal-net--3-october-2026). No phone was connected; physical-device rendering/FPS/touch and WAN latency remain unverified. This is visual net deformation over existing rigid collision surfaces, not soft-body ball/net physics.

Superseded task players, failed preview iterations and completed portable fixtures were archived with hashes/recovery paths in `Legacy/20261003-223749-588-football-net-iterations/` and `Legacy/20261003-225549-095-football-net-validation-cleanup/`. Current evidence, editable source and Unity caches remain in place. No Git staging, commit, push or history rewrite was performed by this task.

## Football character motion — 3 October 2026

Implemented the [complete current-control animation set](FOOTBALL-ANIMATION.md): 26 saved control-space takes, existing run/jump/slide clips, bounded limb IK, actual shoe-to-ball placement, directional fall selection and synchronized charge/kick/cancel presentation. Protocol **18** requires matching players. Movement, possession, kick release, tackle restrictions and hit/miss deadlines retain their authoritative rules.

The Windows development builds pass **1,807 assertions** across the following suites. This total includes repetitions at the four timeline rates; it is not a count of distinct scenarios.

| Suite | Passing assertions |
| --- | ---: |
| Football motion at 20/30/60/120 timeline FPS, including both LODs | 748 (187 each) |
| Football match, physics and host/guest ball/presentation replication | 530 |
| Shared jumping, offline and host/guest | 140 |
| Shared tackling, offline and host/guest, including fall variants | 210 |
| Rapid turning, offline and host/guest | 62 |
| Idle visibility, interruptions and host/guest playback | 61 |
| Basketball motion regression | 56 |

Every saved take is sampled at 31 normalized times on both LODs. Maximum measured limb-length drift is 0.001 mm; maximum reach clamping is about 2 cm. Intended shoe-to-ball-surface contact errors peak at **3.43 cm across rates** (30-rate control/fast/cut: **2.2/3.1/1.8 cm**). Maximum sampled skinned-surface floor penetration is **1.3 cm**. These slightly exceed the original design's 3 cm/1 cm targets and are disclosed rather than relabelled as exact contact. The large shoes, short legs and large head retain their original proportions. Front/side pose sheets and ordered transition frames were inspected; the actual-player video includes normal and half-speed playback.

The ten-athlete pose-layer workload preserves rig lengths and reports **zero steady per-frame managed allocations** at all four rates. Its maximum summed pose cost was **0.507 ms** in the 30-rate Windows capture, with **3.127 ms** the highest maximum among the four runs. This measures only the pose layer, excluding Animator, rendering and much of gameplay; it is not phone FPS or a whole-match performance qualification.

Current motion evidence is in `Builds/FootballMotionQA/Run-20261003-223534-834/` (video, sheets, PNG frames, metrics and results), `Run-20261003-223955-854/` (20), `Run-20261003-224057-732/` (120) and `PortableVerification/` (60). The copied test script resolved its default player/output paths from a `Portable Checkout` directory containing spaces while invoked from an unrelated system temporary directory. The fixture was archived after keeping its reports. `test-summary.json` records all regression report paths. Football match/ball evidence: `Builds/FootballMatchQA/Run-20261003-224838/`.

Earlier attempts found and corrected a scaled-mesh measurement error, real fall ground clipping, excessive get-up reach and pose-layer allocations. An idle regression ran into an obstruction in the final enumerated fishing venue; its locomotion phase now runs on the open football pitch, and all 61 checks pass. A rendered goal-reset assertion exposed deadline rounding; diagnostics were retained and the concurrent net review adds a small comparison tolerance. The unchanged runtime match passed both headless and rendered suites. These fixes do not relax animation input timing or gameplay rules.

The prescribed AndroidSubmission build, measured bytes, signature and packing evidence are recorded in [BUILD-SIZE.md](BUILD-SIZE.md#football-character-motion--3-october-2026) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md#football-character-motion--3-october-2026). Current task-specific APK/audit copies are retained under `Builds/FootballMotionQA/` to preserve provenance when other chats rebuild the shared output. The review code is excluded from release. No source Blender/FBX, binary art dependency, package or character texture was changed for this animation pass. Physical-phone touch, rendering, sustained performance, thermals and WAN behavior remain unverified.

Superseded fitting runs and the completed portable fixture were archived in `Legacy/20261003-225412-407-football-motion-cleanup/`. The first successful APK was superseded by a build using a concurrent material-sharing correction and is recoverable in `Legacy/20261003-225615-892-football-motion-first-apk/`. Superseded baseline/audit copies are in `Legacy/20261003-230020-132-football-motion-release-cleanup/`; their bytes and hashes remain in the current task report. No active art, Unity cache or installed dependency was archived. Staging, commit and push remain with the user.

## Interactive football boundary and twin-rail aim — 3 October 2026

Inspected/fetched `origin/main` at `0d314fd`; the checkout already contained the latest merge. The existing authored pitch containment and goal openings drive the new visual boundary. Ball physics, possession, kicks, match rules and networking remain unchanged. [Implementation and tuning](FOOTBALL-PROTOTYPE.md#animated-boundary-and-charging-feedback--3-october-2026).

The rebuilt Windows player passes **559 assertions**: 241 offline match checks, 28 host/guest match checks, 199 football physics/input checks, 41 host/guest ball checks, 25 focused feedback checks and 25 copied-player portability repeats. The focused checks cover both open goal mouths, exact boundary alignment, absence of new colliders, a real free-ball impact ripple, no repeated ripple from a resting ball, both compiled shaders, monotonic charge growth, maximum charge hold, all three camera modes, cancellation, release and mesh reuse. The last shader-only refinement turns the interior chevrons toward the arrow tip; the final capture run repeats all 25 feedback checks.

Close wall, impact sequence, distant pitch, goal-opening, charge sequence and first/third-person/overhead camera renders were inspected. The final shader build log contains no shader compilation errors or warnings. Camera captures render actual gameplay camera poses into a render target; they do not include the screen-space HUD. The review harness uses the same completion-marker/process-lifetime convention as the other repository suites. Earlier forced-exit review experiments encountered a native shutdown error after completing their assertions; those experimental captures/logs are archived and are not a graceful-exit qualification.

Current evidence: `Builds/FootballFeedbackQA/Run-20261003-214141/`, `build-final-windows.log`, `football-feedback.mp4`, `football-feedback.gif` and `PortableVerification/`; gameplay/loopback evidence: `Builds/FootballMatchQA/Run-20261003-213934/`. A copied player and copied test script ran successfully from a `Portable Checkout` path containing spaces, invoked from an unrelated system working directory with the default player/output resolution. The fixture was archived after retaining its reports.

No source model, Blender image/library link or FBX dependency changed. New delivery assets are text shaders/materials with relative Unity GUID references; generated geometry is runtime-only. The review implementation is excluded from release. Windows renders do not qualify physical-phone touch, Android appearance/FPS, thermals or WAN latency. APK bytes and audit results are recorded in [BUILD-SIZE.md](BUILD-SIZE.md) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).

Reviewed superseded outputs are recoverable in these ignored archive batches: `Legacy/20261003-212342-227-football-feedback-preview/`, `Legacy/20261003-212431-424-football-feedback-baseline/`, `Legacy/20261003-213433-994-football-feedback-design-revision/`, `Legacy/20261003-213735-250-football-feedback-shader-check/` and `Legacy/20261003-214348-184-football-feedback-final-cleanup/`. No active art, Unity cache or installed dependency was archived. Git staging/commit/push remain with the user.

## GitHub integration release — 3 October 2026

The merge combines local football revision `571cacc` with upstream basketball revision `c9810c4`. Football match/team state, possession, fake-shot cancellation, charging aim, tackle contact and goal-net collision are retained. Basketball dribble/shoot/pass poses, reliable pass requests and shared jump momentum are retained. Protocol **17** consistently covers both replicated layouts in the runtime service and Bootstrap scene. The obsolete football `blockedPusher` state and `Push` collision path were not reintroduced.

Unity 6000.3.20f1 rebuilt the Windows development player in the ignored isolated checkout. All **112 C# input hashes** match the workspace, apart from the separately documented editor build option used for Android clean-cache reporting. The combined player passed **1,061 assertions, with no failures or runtime exceptions**:

| Suite | Passing assertions |
| --- | ---: |
| Football match, physics and host/client ball replication | 509 |
| Basketball motion at 30 FPS, including both character LODs | 56 |
| Basketball two-player gameplay and passing | 30 |
| Shared jump/momentum, offline and host/client | 140 |
| Shared tackle, offline and host/client | 208 |
| Shared turning, offline and host/client | 62 |
| Basketball script from a path with spaces and an unrelated temporary working directory | 56 |

Test summaries, input manifests and build diagnostics are in `Builds/GitMergeQA/`. Football evidence is in `Builds/FootballMatchQA/Run-20261003-150002/`; the other suite paths are recorded in `test-summary.csv`. Selected basketball front/side frames for both LODs and the football restart view were inspected. Synthetic gameplay/control checks and Windows captures do not qualify physical-phone touch, rendering/FPS or WAN latency.

The prescribed main-project AndroidSubmission attempt exited while the user's editor was running. The same script/target succeeded in the isolated project with clean-cache packing evidence and a v2-signed **86,563,288-byte** release APK. Its source manifest remains identical to the tested player. Size, headroom, growth analysis and archive batches are in [the current release record](BUILD-SIZE.md#github-integration-release--3-october-2026). Git LFS integrity and indexed portable-path checks passed; final repository readiness is checked before the user-authorized commit and push.

## Football legacy-state cleanup — 3 October 2026

The unused `blockedPusher` field, assignments/resets and fixed-update clearing branch are removed. A full runtime C# compilation passes with only the existing review-script camera-name warnings. Active source/scripts contain no remaining references. An exact comparison against the pre-task source confirms that only this unused state and its cleanup are deleted: interception, kick eligibility, waking the ball, tuning, fake shots and `mustApproach` retain their existing code. No new gameplay or phone test suite is claimed for this cleanup; the gameplay snapshot below predates it.

The prescribed AndroidSubmission build succeeds. Its incremental packing report has no asset offsets, so a clean-cache build in the existing ignored isolated checkout supplies complete current packing evidence. Runtime source hashes match the workspace; only the previously documented isolated editor output/clean-cache options differ. Logs, the source manifest, narrow source diff, ZIP deltas and signature result are in `Builds/FootballLegacyCleanupQA/`. See [the measured release record](BUILD-SIZE.md#football-legacy-state-cleanup--3-october-2026).

The pre-task package/audit/source backup and superseded incremental output are recoverable from `Legacy/20261003-143827-669-football-legacy-state/`.

## Goal nets block athletes — 3 October 2026

The current-source Unity 6000.3.20f1 Windows player passes **509 assertions**: **199** football physics/control checks, **241** offline match checks, **28** host/client match checks and **41** host/client ball checks. This includes **57 new offline net assertions** covering both goals' four solid world-layer panels, walking/sprinting/sliding against left/right/back nets from inside and outside, open-mouth entry/exit and jumps into the sloping roof. Existing tests still verify football entry/retention/return through both goals, all four unladen pitch exits, attached possession, kick/slide physics, visible restart reset, charge cancellation, goal rules, selection, overtime and match results. Host tests additionally simulate a remote athlete sprinting into all six side/back faces and both peers verify its replicated stop pose. Actual current running/kick measurements remain **4.000/7.000 m/s unladen walk/run**, **5.250 m/s controlled run**, **12.003/22.001 m/s light/full kicks** and **8.000 m/s slide ball speed**.

Evidence: `Builds/FootballGoalNetQA/` contains build logs, physics/ball reports, signatures, source/asset hash manifests, ZIP deltas and a summary. Match reports are in `Builds/FootballMatchQA/Run-20261003-130119/`. An independent ignored checkout with a path containing spaces supplied the verification player and clean-cache AndroidSubmission build. Runtime source hashes still match the workspace; the only editor-source exception is its isolated output/clean-cache build options. Two ignored Performance Test resource metadata GUIDs are regenerated by Unity; publishable assets otherwise match. No Computer Use, Git staging or publishing was used. Windows/loopback checks do not qualify phone touch/FPS or WAN behavior. The current APK and recovery batch are recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

## Football fake shot — 3 October 2026

The rebuilt Windows player passes **142 offline football assertions** and **31 host/client loopback assertions**. An additional **19 gesture checks** compile the actual `KickButton` with the extracted `PlayerView` charge methods against Unity API stubs. Current evidence and the source hash manifest are in `Builds/FootballFakeShotQA/`. The isolated build uses the current runtime source, including the rounded arrow and 1.8x charge scaling; the earlier snapshot scope below remains historical.

The real Unity integration probe injects pointer events into Kick and the left joystick. It verifies same-finger cancellation, exclusion of other fingers, immediate aim hiding, retained controller/kinematic possession without releasing the ball, turning/dribbling after cancellation, no delayed kick on release, no restart by sliding back, a fresh press, normal light/full release commands and cancellation when the release lands inside the cancel region. Existing physics checks still measure approximately **12.003 m/s light**, **22.001 m/s full** and **8.000 m/s tackle**. No ball force, speed, charge curve or physics setting changed for this feature. Loopback checks cover existing authoritative ball behavior; physical phone multi-touch, phone quality/FPS and WAN remain unverified.

The original `-nographics` offline run stopped at the existing shader-material assertion. A run with Direct3D12 passed white/50% alpha, transparent surface, depth-write disabled and queue 3000, along with all gameplay assertions. The offline test runner now retains its graphics device in batch mode; network-only probes still use `-nographics`. The final scripted offline run passed. Rendered captures are also kept in the task evidence folder.

The main project was open in Unity, so builds used the existing ignored isolated project without closing the user's editor. Source hashes match the workspace. Superseded isolated APK/audit output was archived in `Legacy/20261003-002925-125-fake-shot-isolated-baseline/`; the failed Null-graphics evidence is recoverable from `Legacy/20261003-003427-835-fake-shot-null-graphics/`. See [the build-size record](BUILD-SIZE.md) for the Android outcome.

## Team A lettering — 3 October 2026

The top football Team A label now uses overlapping vector capitals with an orange gradient, coral outline and red marks matching the supplied sketch. Unity 6000.3.20f1 compiled the updated source and rendered the real Canvas at 1600 x 900 and 844 x 390, including team selection and enlarged details. The inspected previews preserve A counters, left-to-right overlap, clear score separation and the existing controls. Rendering ran from the ignored isolated checkout whose path contains spaces, with outputs resolved from `Application.dataPath` rather than the caller's working directory. The user's open editor was left running.

Evidence: `Builds/FootballTeamAWordmarkQA/Visuals/`, `wordmark-capture.log` and `validated-inputs.csv`. This UI change adds no gameplay logic or imported art. Actual Android packaging is recorded in [BUILD-SIZE.md](BUILD-SIZE.md); Windows editor renders do not validate physical-phone touch, rendering/FPS or the other working-tree gameplay changes. Existing gameplay reports below retain their original source scope.

The required main AndroidSubmission script attempt exited on the open-project lock. A concurrent successful isolated AndroidSubmission build included all final UI inputs (110 matching C# hashes); its 86,527,020-byte APK and complete audit were retained with explicit external provenance under `ExternalBuild/`. The newer current goal-net release is 86,530,508 bytes; its HUD/wordmark hashes match the reviewed UI and its APK v2 signature was independently rechecked. Measurement, signature and contribution comparison evidence is in `Builds/FootballTeamAWordmarkQA/verification.json`, `package-comparison.json`, `apk-entry-deltas.csv`, `asset-estimate-deltas.csv` and both signature logs. These measurements include other working-tree changes.

First-pass previews were archived with hashes and recovery paths to `Legacy/20261003-003046-063-football-team-a-first-review/`; the transient failed editor-startup log is in `Legacy/20261003-003137-731-football-team-a-capture-startup/`. Final previews, measured artifacts and current evidence remain under Builds. No staging, commit, push or history change was performed.

## Attached football possession — 2 October 2026

**Snapshot scope:** this record validates attached possession and the transparent 0.8–1.44 m outline aim. Other tasks subsequently changed the filled arrow and kickoff formation while these checks/builds ran. Those edits are retained, but are not included in this tested player/APK; full current-working-tree compilation, runtime checks and Android freshness are unverified by this record. `Builds/FootballControlQA/verified-runtime-source-manifest.csv` records the actual isolated source that produced the artifacts. The measured APK is [documented separately](BUILD-SIZE.md#validated-attached-control-snapshot--2-october-2026).

The rebuilt Windows player passed **323 match/physics/loopback assertions** after the latest transparent charge-aim/1.44 m length updates. Its unchanged shared movement source also passed **292 regression assertions (615 total, no failures)**. Current football evidence: `Builds/FootballMatchQA/Run-20261002-234406/` and `Builds/FootballControlQA/suite-current.txt`; shared movement evidence: `Builds/FootballControlQA/MovementVerified/`. The preceding 321-check run is retained under `Builds/FootballMatchQA/Run-20261002-232617/`. Unity 6000.3.20f1 built this combined working tree in the ignored `Builds/FootballControlQA/Isolated Workspace/` project, whose path contains spaces, without operating or closing the user's editor. No Computer Use was used.

Possession checks cover exclusive kinematic attachment, unchanged unladen speeds, the user-approved **25%** possession slowdown, no compounded penalty, blocked jump/slide requests, moving and stationary 180°/90°/270° turns, separate charging speed, light/full kick release, spatial re-approach, immediate reception by another eligible athlete, successful versus missed/ball-only tackles, duplicate contact suppression, field limits, reset, despawn and travel. Loopback host/client checks verify ownership, exact replicated foot anchoring after a 180° turn, actual tackle release, both goal areas and shared scene return. Only the host simulates free-ball physics.

| Measured or runtime-checked quantity | Result |
| --- | ---: |
| Unladen walk / sprint | 4.000 / 7.000 m/s |
| Attached walk / sprint | 3.000 / 5.250 m/s |
| Charging motor speed | at most 2.410 m/s; target 2.400 |
| Light / full release speed, including 0.25 m/s lift | 12.003 / 22.001 m/s |
| Tackled holder's ball horizontal release / lift | 8.000 / 4.000 m/s |
| Sampled maximum attached presentation gap | 0.0000 m |
| Client settled 180° turn foot gap | 0.0000 m |

The shared regression suite passed **34 jump, 202 tackle and 56 turning checks**, including all four environments, both character LODs, the full 0.8-second knockdown lock, mutual slide knockdown, airborne evasion, 40 simultaneous path victims without duplicate hits, and missed-slide recovery at 30/60/120 Hz. Both active athlete prefabs explicitly serialize the 4-second cooldown and 0.8-second recovery. The recovery tests check actual sprint travel near 2.8 m/s, followed by normal speed restoration.

Actual Windows RenderTexture captures of acquired/turned/kicked possession, charging aim and both goal restarts were inspected; the ball stays visible at restart and held at the feet after reversal. The first diagnostic runs exposed fixture issues: capture frames had already advanced a kicked ball outside its exclusion area, and a destroyed test wall persisted until frame end. These fixtures were corrected before the successful run. Physical Android touch, device rendering/FPS and WAN latency remain unverified. Fresh Android packaging evidence is recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

Football kickoff, full-ball goal detection, score, restart, regulation deadlines and golden-goal overtime are described in [FOOTBALL-MATCH.md](FOOTBALL-MATCH.md). The preceding flow revision passed 294 rebuilt Windows offline/loopback/physics assertions and 40,011 pure-rules assertions: capacity-limited 10-second team selection, seamless overtime with live actions and ball momentum, and movement after the immutable final result. HUD selection/full-team, regulation, goal flash, overtime and finish captures were inspected. Physical Android and WAN checks remain pending.

The centre-spot football integration is recorded in [SOCCER-BALL.md](SOCCER-BALL.md), with scene placement, model/material audits and build status. Balanced dribbling, light/charged kicking, limited slide contacts, physical rolling and host-authoritative control/ball synchronization are described in [FOOTBALL-PROTOTYPE.md](FOOTBALL-PROTOTYPE.md).

The current golf milestone is documented in [GOLF-VERIFICATION.md](GOLF-VERIFICATION.md), including updated Windows/Android artifacts, room tests and remaining device checks. The preceding basketball milestone is recorded in [BASKETBALL-VERIFICATION.md](BASKETBALL-VERIFICATION.md).

The records below are the **earlier football baseline from 22 September 2026**. Its APK size/hash and statements that basketball is unavailable describe that earlier build, not the current version.

## Completed

- Android ARM64 IL2CPP development APK built successfully: **52,083,383 bytes (52.1 MB / 49.7 MiB)**, below the 100 MB file target. APK metadata confirmed minimum API 26 and target API 36. Android `apksigner verify` passed with a v2 signature. This is a historical build/package check, not a physical-device launch test; the current package ID and APK are documented in [BUILD-SIZE.md](BUILD-SIZE.md).
- APK SHA-256: `CC512A1431171CD4965C9EAFCD66BD8A315F87BAC0F3405F487D80F77C77F04E`.
- New general-audience Unity cloud project linked in saved project settings. Real anonymous authentication, session creation, session-code joining and Relay transport passed with two Windows processes. Movement, readiness, character appearances and changed stadium settings replicated; host start and return reached the guest. Evidence: `Builds/cloud-host2.txt`, `Builds/cloud-guest2.txt`.
- The reported camera conflict came from the diagnostic `-viewScreen` code overriding the view each frame. That override was removed. The rebuilt interactive player visibly switched from first-person to third-person using the Camera button and displayed 60 FPS on this laptop. Phone drag/multitouch remains unverified.
- Real cloud capacity/isolation test: Room A held ten players and rejected an eleventh with `lobby is full`; Room B simultaneously held two players with independent phase and movement. Joining Room B during exploration returned `lobby is locked`. A stale code returned `lobby not found`. Host departure in the two-client cloud test produced `connected=False`, `exploring=False` and the friendly connection-closed message. Evidence: `Builds/CloudQA-20260922-225455/` and `Builds/cloud-guest2.txt`. Reproduce using `Tools/Build/Test-CloudRooms.ps1`.

- Unity 6000.3.20f1 project imports and C# compilation pass. URP 17.3.0, Multiplayer Services 2.3.3 and NGO 2.13.3 compile together against the installed editor.
- Original stadium and athlete export from Blender 5.1.2. Stadium inspected in a Blender render from elevated and pitch-level views. Both assets imported into Unity and used in the Windows player.
- Stadium FBX reduced from approximately 12.1 MB to 1.93 MB by simplifying repeated seat geometry. Final editable sources remain outside Unity. No spectator NPCs or crowd audio added.
- Windows development player builds and launches. Visual inspection performed on the actual native game window: main menu, character customiser, third-person and elevated stadium views. All three camera positions also exercised by the offline probe.
- The football baseline Windows development build output was approximately 170.1 MB uncompressed. This includes the Windows engine/runtime and is not an Android APK size estimate.
- Offline movement probe moved the avatar 4.8 m; no internet/UGS initialisation was needed. Chibi Idle/Walk/Run clips imported and the runtime Animator evaluated. The on-screen FPS indicator displayed 30 FPS on this laptop; this is **not** a phone performance measurement.
- Character colour selections made in the customiser persisted across player launches. The changeable hair tuft and skin/hair/outfit preset controls are implemented.
- Two independent NGO/Transport loopback rooms ran simultaneously on ports 7777 and 7778. Room A contained ten players, Room B two. Their player counts and transforms remained separate.
- Room A rejected an eleventh process with `This room is full (10 players).`
- Server-owned spawn positions were distinct. Movement was simulated by the host and matched across client logs; character appearance IDs and ready state replicated.
- Host start changed every client's exploration phase. Host return changed every client back to the waiting phase. A later join attempt during exploration was rejected.
- Host process exit disconnected guests and returned the application from exploration to menu state (`connected=False`, `exploring=False`). A disconnect reason was available to the UI.
- A separate two-process test replicated a changed host stadium preset (`TEST 7780`, lilac palette, design 2, team welcome, flags off) to the guest during exploration. Evidence: `Builds/preset-host.txt` and `Builds/preset-client.txt`.

Evidence: `Builds/NetworkQA/*.txt` and `*.log`, `Builds/smoke.txt`, `Builds/scene-audit.txt`, build logs and Blender review PNGs. These generated evidence files are excluded from Git. The reproducible local room test is `Tools/Build/Test-LocalRooms.ps1`. Development probes run only with explicit `-probe` arguments.

## Still awaiting access / not passed

| Requirement | Status |
|---|---|
| Android development APK | Passed build, signature and package checks; physical launch pending. |
| Physical Android launch, touch/multitouch, camera clipping | Not tested on a phone. `adb devices` returned no device. Desktop checks do not prove these. |
| Sustained 30 FPS on Android and APK below 100 MB | APK size passes at 52.1 MB. Sustained Android FPS and thermal behaviour require a phone. |
| Internet sessions across different networks | Real cloud/Relay tests passed on this PC's connection. Two separate physical networks still need testing. A separate named development environment remains pending dashboard access; builds currently use the new prototype project's default environment. |
| Invalid cloud codes, cloud-full room errors, network outages | Stale-code, full-room, locked-room and host-disconnect paths passed via live services. Broader network outage/recovery testing remains pending. |
| Ten users through Relay and simultaneous cloud sessions | Passed using independent Windows processes on this PC. Different physical devices/networks remain pending. |
| Stadium preset replication over internet/Relay | Passed with two Windows processes using real Relay; separate physical networks remain pending. |
| Broad device/aspect-ratio coverage | Pending. UI uses landscape scaling and safe-area anchors; test phones/tablets before publishing. |
| Huawei achievements/results | Explicit placeholder only. HMS SDK registration/integration is a later task. |

## Known prototype limits

- No football ball, scoring, timers, bots, spectator population, basketball arena or golf island.
- No host migration, client prediction, reconnect/resume or dedicated servers.
- One static full-size stadium. Seats are consolidated by material, so editing individual seats requires Blender editing/regeneration rather than runtime object manipulation.
- Stadium signs use runtime world-space UI. Font legibility and face orientation should be reviewed at close range on the phone before release.
- The startup art uses solid colours, not image textures. It does not depend on an external asset pack.
- Native desktop drag automation delivered pointer-down but no subsequent drag events in the touch-pad trace. Actual multitouch dragging still needs testing on the phone; this check has not been counted as passed.
- Scene rebuild and art regeneration intentionally replace generated outputs; save manual variants separately.
