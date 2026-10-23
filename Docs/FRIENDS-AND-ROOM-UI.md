# Friends and room UI

The Friends entry on Home, the Friends navigation item, and the Requests tab
show a red circular count of incoming friend requests. Zero hides the badge.
The count refreshes when the visible UI polls the signed-in profile service
(at most once per 25 seconds), when a friend list loads, and immediately after
accepting, declining or blocking the sender. It excludes outgoing requests and
clears on account changes. A transient service failure retains the last known
count; this is polling, not push delivery.

The room opens on its player roster. Players and Spectators have separate counted
tabs, so spectator places only appear when requested. Portrait cards show each
participant's name, place number, local identity and host/ready status. Empty
places remain selectable; occupied places open the player's profile. The room
keeps its existing ten spectator places and each island's player capacity
(five for Fishing, ten for the other sports).

The host can click Chosen spot to choose any of the four sports before departure.
Everyone receives the same destination and venue appearance, keeping the room
code and existing participants. Guests acknowledge the new choice by readying
again. Switching to Fishing is unavailable above five players; extra players
must choose spectator places first. A joining participant temporarily blocks
the change so reserved seats cannot become invalid. Changes during exploration,
travel, recovery or an existing operation are rejected.

The host has no lobby Ready button and is excluded from the readiness gate.
All other participants, including spectators, must be ready. At least one player
place must be occupied. The host still owns departure and everyone travels
together. In-game skill drafts, team selection and match rules are unchanged.

This task's delivery snapshot uses network compatibility **78**; testers must
use matching builds. The shared checkout has independently advanced to **79**
for concurrent passing work. No new art or serialized network fields were added
by this UI task. Existing portraits and sport artwork are reused.

Validation and delivery evidence is retained under ignored `Builds/LobbyRefreshQA/`.
The build uses an isolated copy of the task's source because tutorial and passing
work are concurrently active in the shared checkout. The snapshot includes the
existing online UI changes present when this task began; later concurrent work
is outside its verification scope.

The full menu suite passed **3,959 checks**: 1,306 offline, 1,830 host and
823 guest. It exercises the real UI controls for all four spot changes, stable
room identity, guest permissions, resetting readiness, starting without host
readiness, spectator tabs, shared entry/return, safe bounds and text fit.
Room captures cover 1600×900, 1200×900 and 1920×824. Synthetic notification
fixtures verify exact counts of 3 and 150 and hiding zero.
After correcting readiness-error wording and dismissal, the final host/guest
run passed another **2,654 checks** (1,831 host and 823 guest), retaining all
four spot changes and entry/return coverage. Its captures show the cleared
lobby feedback. Earlier offline/profile/capacity checks cover unchanged logic.

The real profile-service suite passed **143 checks**, including receiving a
request, rendering its count, accepting it, clearing the count immediately and
profile persistence across restarts. Its account identities were isolated and
cleaned up. The six-peer rendered capacity suite passed **21 checks**, covering
rejection above Fishing capacity, guest authority rejection, voluntary spectator
movement, seat normalization, ready resets, spectator readiness and successful
shared entry with an unready spectator host. The initial null-renderer capacity
run could not use teleport materials; the successful run uses Direct3D 11.

The complete **327-file Windows player** was promoted to `Builds/WindowsFinal/`
and every file hash compared. The superseded player is recoverable in
`Legacy/20261023-130308-189-friends-room-windows/`. The intermediate task player
is in `Legacy/20261023-130957-651-friends-room-feedback/`, and the intermediate
Android candidate is in `Legacy/20261023-130758-407-friends-room-candidate/`.
Source hashes, scene-payload
hashes and the delivered file manifest are retained with the evidence. The
scripts build helper explicitly restores packaged scenes after Unity replaces
StreamingAssets.

Evidence: `Builds/StorybookMenuQA/LobbyRefresh-Validated/`,
`Builds/LobbyRefreshQA/ProfilesFirst/`, `Builds/LobbyRefreshQA/Capacity-Rendered/`.
The first promoted player also passed another 21 six-peer checks through the
script's default player path from an unrelated working directory; see
`Builds/LobbyRefreshQA/Promoted-Portable/`. Final feedback checks are in
`Builds/StorybookMenuQA/LobbyRefresh-Feedback/`.
Physical Android touch/appearance/FPS and real WAN latency remain unverified.

AndroidSubmission completed on 23 October at 13:12:37 Malaysia time. The actual
APK is **110,666,082 bytes**, compared with **110,643,406** at task opening
(+22,676); remaining decimal-GB headroom is **889,333,918 bytes**. The official
gate, all 1,214 ZIP/audit entries, five-scene catalog hash and APK v2 signature
passed. All 587 C# snapshot input hashes stayed unchanged through packaging.
See [size measurements](BUILD-SIZE.md) and [entry audit](APK-SIZE-AUDIT.md).
The prior APK/audit are recoverable in
`Legacy/20261023-131408-137-friends-room-android/`.

The repository readiness check passes without staging, committing or pushing.
All build outputs, source snapshot copies, raw logs and captures remain ignored.
