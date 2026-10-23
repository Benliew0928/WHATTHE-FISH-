# Online room UI redesign

## Review findings

The old secondary screens used a mix of large opaque panels, generic connection
copy and automatic guest modals. That obscured the islands, made it hard to tell
who should act next, and treated several recoverable failures as invalid room
codes. Some loading paths lacked a useful exit; backing out of fishing skill
choice could silently pick the first offer. Station/travel cards also competed
with gameplay controls.

The redesign gives status, decisions and failures distinct behaviour: compact
status preserves exploration, explicit decisions take input temporarily, and
failures retain a reason with a recovery action. Scene panels use the world
palette while menu-only pages keep the player hub treatment.

## Presentation rules

In-world status panels use warm charcoal, a lightly translucent surface, restrained
island accents, and the existing game fonts. The scene remains visible around
them. Football, basketball, fishing and golf retain their established gameplay
HUD artwork. The account and connection pages shown outside gameplay use the
main menu's navy theme. No new raster assets, blur passes or network fields are
needed.

A waiting state should say what is happening, who acts next, and what the player
can still do. A status update does not take movement input. A decision overlay
does, and restores input when closed. Every progress indication comes from a
real phase, timer or acknowledgement; there is no estimated loading percentage.

## Implemented scope

| State | Presentation and behaviour |
| --- | --- |
| Guest after a round | Compact waiting dock names the host; exploration continues. Details can be opened and closed explicitly. |
| Host after a round | Smaller scene-facing island chooser with destination art, capacities, separate replay action, station/game landing choice and a destination-specific primary action. |
| Loading an island | Destination artwork, actual scenery/landscape preparation stage, retry on failure and a back action for offline selection. |
| Teleport preparation | Destination stays visible; the real number of prepared crew includes spectators. |
| Teleport travel/arrival | Prepare/travel/arrive steps and an expandable, two-column crew readiness list; the main scene stays visible. |
| Interrupted teleport | Persistent reason, explicit dismissal, host retry/choose-island actions and entry-failure feedback after returning to the lobby. Existing group rollback remains authoritative. |
| Host recovery | Scene-facing paused card, successor name, connection/restoration phase, longer-wait explanation and Leave room action. |
| Connection failures | Persistent, dismissible feedback outside the join page too; distinct full/locked/incompatible/unavailable/recovery messages. A generic network failure does not incorrectly mark the invitation code invalid. |
| Leave/return confirmation | Lobby host departure explains transfer; returning everyone to the lobby is distinct from leaving as a guest. Spectators are not told their departure voids a match. |
| Create/join operations | Action-specific stages and a cancellation request that waits for service/transport cleanup before allowing another operation. The entered invitation is retained. |
| Lobby readiness | Actual ready/total crew count, spectator participation explained, guest host-wait status and host-only departure action. |
| QR scanning | Camera/image-picker/scanned-code messages remain distinct from network connection progress. |
| Skyrail station and ride | Shared world-facing panel, consistent Skyrail naming, crew attendance visible to guests, real ride progress and compact scenery-preserving ride status. |
| Football/basketball setup | Existing sport skins retained; team choice, countdown and automatic assignment are explained consistently. |
| Fishing ready/skill draft | Real ready counts; chosen skill collapses into a compact summary. Back never silently selects the first skill. Required timed drafts explain auto-selection at expiry. |
| Golf finish waiting | Compact finish notice with remaining-player count; existing final countdown and standings remain available. |
| Spectating | Sport/activity, host identity while waiting and a clear start condition for spectator hosts. |
| Void/abandoned round | Post-round controls also handle voided ball matches; abandoned fishing rounds have their own explanation. |
| Account/social/startup | Account styling matches the player hub, native sign-in can be cancelled while pending, operations name their action, disabled controls keep their prior state, and startup failure advice reflects the cause. |

Four islands, one active game per room, existing sport capacities, spectator
limits, match rules, travel authority and the shared-room world model are
preserved. The UI changes add no serialized fields. The requested pull incorporates the
golf event schema at protocol **77**; every tester must use this delivered build.

## Verification and delivery

Initial UI validation on 22 October 2026, before the requested pull:

| Suite | Result and evidence under ignored Builds/ |
| --- | --- |
| Scene UI | 287 checks: all four actual islands, three screen shapes, safe bounds, text height, input restoration, fishing Back/reopen, loading Back, connection cancellation/cleanup and error copy. `OnlineUIQA/Run-20261022-205239-393/`. |
| Host/guest teleport | 534 checks: every island and landing type, guest exploration/details, replay, preparation, rollback, cancellation at multiple phases and disconnect. `RoomTeleportQA/Run-20261022-210035-161/`. |
| Player/spectator travel | 245 checks: all four islands, full Skyrail ride, match starts and spectator-host controls. `TravelMatchQA/Run-20261022-211005-466/`. |
| Host recovery | 3,928 checks with 20 real local processes: five successive handovers across lobby/match, abrupt/graceful exit, spectator fallback and new-player priority. `HostMigrationQA/Capacity-Football-20261022-210035-204/`. |
| Menus/lobby | 1,780 checks across offline and real local host/guest: readiness, permissions, seat changes, conditions, departure and return. `StorybookMenuQA/Run-20261022-210035/`. |
| Startup | 347 checks across cold/retry and warm caches at three aspects. `StartupWelcomeQA/Run-20261022-210035-117/`. |
| Island settings | 112 offline and 175 real cloud room checks for modal input, host return and guest departure. `IslandSettingsQA/Run-20261022-210035-182/` and `Run-20261022-205345-701/`. |
| Portable defaults | A complete copied player and the new script passed another 287 scene checks from a folder containing spaces, launched from an unrelated working directory with no player-path override. Evidence retained in `OnlineUIQA/PortableValidation/`. |

The initial release retains 553 C# source-file hashes in
`OnlineUIQA/PreMergeRelease/source-final.json`. The merged release has 584 inputs
in `OnlineUIQA/source-final.json`; these include editor/test inputs.
Recovery screenshots in the focused fixture inject UI state; the separate
20-process suite tests actual host recovery. Captures of travel failure wait for
presentation restoration rather than enabling previously hidden canvases.
Existing fixtures were updated for the current player hub, bots on every island,
read-only guest venue inspection, the fishing draft and modal deferral.

## Requested pull and shared test package

After the initial checks, `git pull --no-rebase origin main` fast-forwarded
`4b92d90` to **`a374059`**, incorporating `03409e7` and `a374059`. Git restored
the local UI changes cleanly via autostash; the existing stash list is preserved.
UI changes remain uncommitted and unstaged. The pull adds the latest golf cup,
events, fishing durations and shorter menu copy; the integration retains both
the scene UI redesign and those changes. Working-file recovery data is in
`OnlineUIQA/BeforePull/`.

The complete merged Windows player is delivered in `Builds/WindowsFinal/`, with
its runtime DLLs, data and launchers. The shared ZIP contains that entire player.
The final merged player passed:

- **287** scene UI checks, including loading exits, cancellation and fishing Back/reopen (`OnlineUIQA/Run-20261022-212128-423/`).
- **534** host/guest teleport checks on protocol 77 (`RoomTeleportQA/Run-20261022-211756-978/`).
- **1,686** offline/host/guest menu and lobby checks (`StorybookMenuQA/Run-20261022-211756/`).
- **18** rendered golf cup/event/menu checks (`OnlineUIQA/Merged-GolfClarity-20261022-211757/`).
- **2,966** rendered fishing rule, gameplay, draft, skill, cooldown and camera checks (`FishingGameplayQA/Run-20261022-212128/`).
- **175** real cloud room checks for host return and guest leave (`IslandSettingsQA/Run-20261022-212238-626/`).

The fishing suite exposed a persistent practice-dismissal flag. It now resets
when the round or player changes, so a fresh practice round opens its new skill
choices. The final UI and full fishing suites cover this fix. Other merged
suites precede that scoped change; their relevant source is unchanged.

Full merged Windows assets/player were built at **21:17**, with final scripts
at **21:21**, Malaysia time. Promotion compared all **328 files**, including
the testing guide and updated build record, and verified the packaged scene
bundle hash. The ZIP is **437,624,006 bytes** and every archived/extracted file
matches the delivered player. It contains one `WhatTheFish/` folder; extract all
and run `WhatTheFish.exe`. Both testers must use this protocol-77 package.
SHA-256: **30F261BF73FE5D69D3BE35133B5E25EE77F3D73F94CEEF7758E881931FEC807D**.

The ZIP and SHA-256 sidecar are in `Builds/WindowsPackages/`; full archive
verification is in `OnlineUIQA/zip-verification.json`. The extracted ZIP passed
another **287** UI checks from a path containing spaces
(`OnlineUIQA/Run-20261022-212506-990/`).

AndroidSubmission completed at **21:26:10 Malaysia time**. The actual APK grew
from **106,377,892** to **110,643,406 bytes**, leaving **889,356,594 bytes**
below the strict cap. Integrity, all **1,212** audit entries, scene hashes and
APK v2 signature passed. Most growth is the pulled golf event artwork. See [release measurements](BUILD-SIZE.md)
and [APK audit](APK-SIZE-AUDIT.md) for the final measured package.

Physical Android touch/FPS, OS clipboard interaction, native provider sign-in
and real WAN latency remain unverified. The profile service did not save during
the startup runs; those runs verified retained drafts, recovery feedback and
restored controls, not successful cloud profile persistence. Real cloud room
creation, joining, travel and leave checks passed separately.

Superseded players and the opening APK are recoverable in:

- `Legacy/20261022-203806-544-online-ui-first-candidate/`
- `Legacy/20261022-204828-644-online-ui-reviewed-candidate/`
- `Legacy/20261022-210605-307-online-ui-android/`
- `Legacy/20261022-210820-839-online-ui-windows/`
- `Legacy/20261022-211030-113-online-ui-portable-copy/`
- `Legacy/20261022-211402-905-online-ui-before-merge/`
- `Legacy/20261022-211610-371-online-ui-helper-cleanup/`
- `Legacy/20261022-212314-673-online-ui-prepull-apk/`
- `Legacy/20261022-212408-040-online-ui-merged-delivery/`
- `Legacy/20261022-212758-673-online-ui-validation-copies/`

Current captures, suite results, source hashes and release verification belong
under ignored `Builds/OnlineUIQA/` and the suite folders listed above. Git staging,
commit and push remain with the user.
