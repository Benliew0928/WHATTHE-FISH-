# Build size — measured release records

## Communication controls and audience tabs — 25 October 2026

Official AndroidSubmission: **126,891,150 -> 126,914,878 bytes (+23,728)**.
Headroom below the strict 1,000,000,000-byte limit: **873,085,122 bytes**.
SHA-256: **E4D0D105F065B6AE50041453EE86433407C8D08DF122C3DB0B8C1D7137755FF1**.
The size gate, v2 signature, ZIP CRC, all **1,250** audit entries and five-scene
bundle hash/size passed. All **360 Android** compiled source documents and the
voice configuration bridge match the tested runtime source.

The complete **342-file WindowsFinal** preserves the latest scoreboard delivery
and includes the corrected Golf settings placement. All **500 Windows** source
documents match its tested snapshot. Verification: **480** four-sport voice UI
and state assertions, **52** standalone state checks plus **52** portable repeats,
**226** chat/history checks, **69** chat UI checks and native two-client audio with
individual gain and spectator group mute. Phone acceptance remains separate.

This release adds the two-icon chat/microphone launcher, tap-to-toggle and
hold-for-settings, individual voice volume, Players/Spectators tabs, retained
chat-message roles and local spectator group mute. Protocol **87** requires
matching clients; there is still one native voice channel per room. Vector
icons add no raster textures. The delivered source snapshot also includes
concurrent venue-preview and scoreboard code; this task's publishing scope is
the communication changes.

Evidence: `Builds/CommsRedesign/`, `Builds/VoiceQA/Online-20261025-164916-031/`
and [room voice](ROOM-VOICE.md). Physical phone capture, Bluetooth routing,
speaker echo and touch/FPS remain unverified without a connected phone.
Previous Android recovery: `Legacy/20261025-164956-773-comms-redesign-android/`.
Windows recovery: `Legacy/20261025-214153-693-comms-redesign-windows/`.

## Integrated room voice and teammate merge — 25 October 2026

Completed live Vivox room voice and merged the three teammate commits through
`1b7322e`, preserving the pending crew recovery/invitations, menus, cinematics
and prop presentation work. Protocol **86** combines the recovery clock with
Golf tornado/crab snapshots. One channel follows each online room across all
four islands. Phone microphone permission, opt-in join, initially muted capture,
voice controls and accessible mute during travel/aim/rules are included.
Travel/settings/results no longer retain a closed voice panel's temporary input lock.

Complete **342-file WindowsFinal** matches the tested candidate. **493 Windows**
and **354 Android** compiled source documents match the workspace, together
with the separate configuration bridge. Verification: **340 voice UI assertions**,
**38 state checks** plus **38 portable-path repeats**, **198 text-chat/history
checks**, final **63 chat UI checks**, **23 Golf reconnect/host-loss checks** and
**44 account/invitation tests**. Two authenticated native clients received synthetic
speech both ways in one channel, with hardware capture silent and mic input muted.
Representative Football, Golf and Fishing renders were visually reviewed.

Official AndroidSubmission: **124,646,916 -> 126,880,470 bytes (+2,233,554)**;
strict decimal-GB headroom **873,119,530 bytes**. SHA-256:
**1BB034646EA0C1FE1FCEEE7597E78AD05B9B5EAF2DA536B13E4E17A0804316FF**. Size gate, v2 signature, ZIP CRC, exact entry audit and five-scene
bundle integrity passed. Vivox public connection fields are present; Test Mode
is off and the signing key is empty. The manifest has runtime microphone/audio/
Bluetooth permissions and optional microphone hardware.

Evidence: `Builds/VoiceQA/Merge-20261025/`, current `Builds/SizeAudit/latest/`,
[voice implementation and test paths](ROOM-VOICE.md). Current archives:
`Legacy/20261025-095325-407-voice-merge-android/`,
`Legacy/20261025-095442-713-voice-merge-windows/` and
`Legacy/20261025-095518-400-voice-merge-qa-cleanup/`.
Phone microphone, Bluetooth, echo, touch/FPS and mobile network handoff remain
unverified at the user's request. The broader legacy cart suite is not passing:
pre-merge and merged candidates reproduce the same 45 offline failures; network
fixture failures also remain. These are recorded, not counted as successful checks.

## Live room voice activation — 25 October 2026

Official AndroidSubmission: **124,656,400 -> 124,646,916 bytes (-9,484)**.
Headroom below the strict 1,000,000,000-byte limit: **875,353,084 bytes**.
SHA-256: **47CD77DD73442F56E03DEE43147573D8D848EAD10784B2436A6ADC9820C62F19**.

Vivox is enabled, its public connection fields are packaged, Test Mode is off and
no signing key is embedded. A live two-client native test received synthetic
speech in both directions through one channel with hardware capture silent and
input muted. An asynchronous participant-mute acknowledgement race was fixed.
The complete 342-file Windows player includes the fix and passed a fresh 79-check
Fishing UI smoke test. Physical phone validation was deferred at the user's request.

APK v2 signature, ZIP CRC, all 1,243 audit entries, the five-scene bundle, native
voice libraries and the optional-microphone/runtime-permission manifest passed.
All 348 main Android and 486 Windows compiled source documents match current code.
Largest compressed entries: islands **45,412,350 (-8,051)**, IL2CPP
**13,080,180 (-1,581)**, Vivox SDK **10,240,163 (unchanged)**, Unity **9,808,830
(unchanged)** and metadata **2,892,493 (-149)**. No art quality changes were made.

Evidence: `Builds/VoiceQA/Setup-20261025/`,
`Builds/VoiceQA/Online-20261025-003106-105/` and `Builds/SizeAudit/latest/`.
Previous APK recovery: `Legacy/20261025-003216-741-voice-setup-android/`.
See [room voice](ROOM-VOICE.md) for verification and remaining device checks.

## Room voice implementation — 24 October 2026

The official AndroidSubmission APK is **124,656,400 bytes**. Task-entry size was
**113,758,996 bytes** (+10,897,404); the immediate pre-voice delivery was
**113,823,708 bytes** (+10,832,692). Headroom below the strict 1,000,000,000-byte
limit is **875,343,600 bytes**. SHA-256:
**1B899D6FB95467550A75544355941C0CD4380F51404C2532FBDD7DEC486AC083**.

The size gate, APK v2 signature, ZIP CRC, all **1,243** audited entries and
five-scene bundle integrity passed. Both ARM64 Vivox libraries are included.
The final manifest has runtime microphone/audio/Bluetooth permissions, optional
microphone hardware and suppressed automatic startup permission prompts.
All **348** main Android source documents plus the configuration bridge match
current source; development fixtures and token-signing secrets are excluded.

Largest compressed entries: islands **45,420,401** (+13,032), IL2CPP **13,081,761**
(+230,354), Vivox SDK **10,240,163** (new), Unity **9,808,830** (-211), and metadata
**2,892,642** (+62,709). The voice SDK explains the dominant growth. No world-art
reduction or new texture payload was introduced.

**Live voice is not enabled yet:** the linked project's Vivox connection settings
are missing. The client disables Join voice until settings are fetched and the
game is rebuilt. No phone was connected; two-way audio, Bluetooth, speaker echo,
phone touch/FPS and WAN behavior remain unverified. See [room voice](ROOM-VOICE.md)
for setup and the 307 voice-UI, 63 text-chat and 38 state assertions. The complete
342-file Windows player was promoted with protocol 83 and current recovery,
invitation and cinematic code.

Evidence: `Builds/VoiceQA/apk-verification.json`, `apk-signature.txt`,
`apk-manifest-check.txt`, `apk-entry-delta.csv`, `android-sources.json`,
`android-configuration-source.json`, `AndroidAudit/`, and `Builds/SizeAudit/latest/`.
Pre-voice APK recovery: `Legacy/20261024-210250-511-room-voice-android/`.
The rejected first manifest audit is retained in
`Legacy/20261024-211009-700-room-voice-manifest-correction/`.

## Crew reconnection and friend invitations — 24 October 2026

The official AndroidSubmission build measures **113,823,708 bytes**. The
task-entry APK measured **113,758,996 bytes** (**+64,712**); the intervening
cinematics delivery measured **113,789,392 bytes** (**+34,316**). Remaining
headroom below the strict 1,000,000,000-byte limit is **886,176,292 bytes**.
SHA-256: **8F238DB7323689E891C4E82682F2C029189AE662A7EE7C6E32A66EADE5404806**.

The build gate, APK v2 signature, ZIP CRC, all **1,243** entry sizes and the
five-scene bundle's hash/size passed. All **343** compiled Android runtime
source documents match the isolated snapshot. The complete Windows delivery
contains **334 hash-verified files**, including seven retained relative-path
launchers; all **478** compiled Windows source documents match its snapshot.
Both builds use network protocol **83** and include the current cinematics.
Only the concurrent unfinished voice integration was excluded; two compiled
source documents differ from the primary workspace for those voice hooks.

Largest compressed entries, with deltas from the intervening delivery:
islands **45,407,369 (-13,529)**, IL2CPP **12,851,407 (+38,012)**, Unity
**9,809,041 (+1)** and metadata **2,829,933 (+7,984)** bytes. No unexpected
dominant growth was found. The new UI reuses existing fonts, character art and
procedural graphics; no lossy asset conversion was introduced.

See [crew connections](CREW-CONNECTIONS.md) for gameplay, invitation, security
and visual validation. The final live Relay reconnect attempt was blocked by
local network timeouts; physical Android touch/FPS remain unverified.
Evidence is under `Builds/CrewConnectionsQA/`, including
`apk-verification.json`, `apk-signature.txt`, `apk-entry-delta.csv`,
`android-sources.json`, `windows-sources.json` and
`windows-delivery-hashes.json`. Current audit: `Builds/SizeAudit/latest/`.
Superseded delivery recovery:
`Legacy/20261024-203229-182-crew-connections-delivery/`.

## Four-island activation cinematics — 24 October 2026

The official AndroidSubmission build measures **113,789,392 bytes**, compared
with the opening APK's **113,758,996 bytes**: **+30,396 bytes**. Remaining
headroom below the strict 1,000,000,000-byte limit is **886,210,608 bytes**.
SHA-256: **93BE231E589A644E5AA2B2DF33E3F8050609BB1042E8BE8B0ED7A212A1D50806**.

The size gate, APK v2 signature, ZIP CRC, all **1,243** audited entries and
five-scene bundle integrity passed. All **338** compiled Android runtime
source documents match the isolated snapshot. Cinematic runtime types are
included and development review fixtures are excluded. Existing native
scanning and account bridges remain present.

Largest compressed entries: islands **45,420,898**, IL2CPP **12,813,395**, Unity
**9,809,040**, metadata **2,821,949** bytes. Their respective changes are
**+15,674**, **+11,365**, **-1**, and **+1,618** bytes. Two new serialized entries
add **1,216** bytes. No unexpected dominant growth was found. The presentations
reuse existing skill/prop icons and procedural meshes; no new textures or
lossy world-art conversion were introduced.

The complete Windows player was promoted and all **334 files** hash-checked.
See [activation presentation](PROP-ACTIVATION-FOCUS.md) for the 18 videos,
1,968 assertions and exact snapshot scope. This protocol 82 snapshot isolates
the concurrently unfinished voice/reconnect/invitation work. Phone quality,
FPS/touch and live WAN behavior remain unverified.

Evidence: `Builds/ActivityCinematicsQA/apk-verification.json`,
`apk-signature.txt`, `apk-entry-delta.csv`, `android-sources.json`,
`AndroidAudit/`, and `Builds/SizeAudit/latest/`. Previous APK recovery:
`Legacy/20261024-194730-823-activity-cinematics-android/`.

## Shared menu design - 24 October 2026

The official AndroidSubmission build for [shared menu design](MENU-DESIGN.md)
measures **113,758,996 bytes**. The local APK at task entry was **113,344,522
bytes**: **+414,474 bytes**, leaving **886,241,004 bytes** below the strict
1,000,000,000-byte boundary. The intervening prop delivery measured
**113,756,604 bytes**, so this final refresh adds **2,392 bytes**. SHA-256:
**9AD5963361AF930ACF8111113C0E792DEAD26994E7BB88418F9BB054C749E110**.

The size gate, APK v2 signature, ZIP CRC, all **1,241** entry sizes and the
five-scene bundle's hash/size passed. All **336** compiled Android runtime
source documents match the working source. Shared menu types are present;
development review fixtures are excluded. The combined source includes teammate
merge `7274043` and the concurrent sports prop changes. This is not an isolated
measurement of the menu feature.

Largest compressed entries: islands **45,405,224**, IL2CPP **12,802,030**, Unity
**9,809,041**, metadata **2,820,331** bytes. Compared with the intervening prop
delivery, islands increased **1,489** bytes and IL2CPP **941** bytes; metadata
decreased **42** bytes. No unexpected dominant growth was found. The menu design
uses procedural meshes and existing fonts/icons, with no new textures or lossy
world-art conversion.

Evidence: `Builds/MenuDesignQA/apk-verification.json`, `apk-signature.txt`,
`apk-entry-delta.csv`, `android-sources.json`, and `AndroidAudit/`. The current
inventory also remains in `Builds/SizeAudit/latest/`. Windows interaction and
visual review coverage is documented in [Menu design](MENU-DESIGN.md).
Physical Android touch, quality/FPS and live WAN behavior remain unverified.
Previous local APK recovery: `Legacy/20261024-143909-542-shared-menu-android/`.

## Sports prop activation and awareness - 24 October 2026

The official AndroidSubmission build for [prop awareness](PROP-ACTIVATION-FOCUS.md)
measures **113,756,604 bytes**. The actual APK present when this task began was
**113,344,522 bytes**: **+412,082 bytes**, with **886,243,396 bytes** of headroom
below the strict 1,000,000,000-byte boundary. SHA-256:
**C74BD20BAA679BD9C683016CD44A1F4A64DBA9D1F4FEC455B9982BA87335A191**.

The size gate, APK v2 signature, ZIP CRC, all **1,241** audit entries and the
five-scene bundle's size/hash passed. Prop runtime types are included and
development review fixtures are excluded. The pooled effects and name ribbon
reuse existing geometry/material/icon resources; no new art or lossy conversion
was introduced for props. This combined build includes shared UI/tutorial work,
so its delta must not be attributed solely to the prop feature.

Largest compressed entries: islands **45,403,735**, IL2CPP **12,801,089**, Unity
**9,809,041**, metadata **2,820,373** bytes. The largest increases are three
serialized data entries (**147,366**, **118,855**, **110,949** bytes) and IL2CPP
(**33,361** bytes); the island bundle shrank **4,484** bytes. No unexpected
dominant contributor was found. Exact entry differences are retained in
`Builds/PropFocusQA/apk-entry-delta.csv`.

The complete Windows player was promoted to `Builds/WindowsFinal/` after
**1,477** successful assertions, including multiplayer prop presentation,
counterplay, inventory, camera/identity and tutorial integration. Current
evidence is indexed in `Builds/PropFocusQA/verification-runs.json`; APK integrity
and source evidence are in the same folder, and the full audit is in
`Builds/SizeAudit/latest/`. The prop runtime source matches the builds. Later
concurrent UI/tutorial and development fixture changes are outside the respective
snapshots; the exact paths are in `Builds/PropFocusQA/source-scope.json`. Physical
phone quality/FPS/touch and live WAN behavior remain unverified.

Previous local APK/player recovery:
`Legacy/20261024-141036-440-prop-focus-android/` and
`Legacy/20261024-142638-674-prop-focus-windows/`.

## Golf medals raised another 12cm - 25 October 2026

Raised the animated head anchor from 0.54m to 0.66m (+0.12m), and the
feet-based fallback from 1.82m to 1.94m. No new art, sound or scoring changes.
Pending earlier Golf changes are preserved. Full WindowsFinal and official
AndroidSubmission updated. APK **115,960,030 -> 115,954,062 bytes
(-5,968)**; decimal GB headroom **884,045,938 bytes**.
SHA-256: **371D7CE61858102EE27DFCC0BB3309B4797FF81A288BF1949B62B29CB0B9660B**.
ZIP CRC and both scene-bundle catalogue hashes passed; current audit is in
`Builds/SizeAudit/latest/`. Evidence: `Builds/GolfMedalRaiseAgain/`.
Previous complete player/APK/audit: `Legacy/20261025-001700-967-golf-medal-raise-again`.
No phone FPS measurement. No staging, commit or push in this task.

## Golf tornado audio and raised medals - 24 October 2026

Raised the head-tracking Golf medals by 0.12m, retaining the original artwork
and removing captions as requested. Added one shared procedural six-second,
22,050Hz mono wind loop (529,200 PCM bytes at runtime), prepared during scene
loading. Each visible tornado has one spatial voice with growth-linked volume
and pitch, linear 4-42m attenuation and existing SFX/master controls. Warning
stages remain silent; hiding, event end, disable, pause and focus loss stop the
voice. No new imported audio or texture assets were added.

Full WindowsFinal and official AndroidSubmission updated. APK **115,954,850
-> 115,960,030 bytes (+5,180)**, decimal GB headroom
**884,039,970 bytes**. SHA-256:
**BF5D599DE46BB03783D39EA09F3FD3548C29EAC7740C6315EC8E397D0B756232**. ZIP CRCs and both scene catalogue hashes passed. Current audit:
`Builds/SizeAudit/latest/`; evidence: `Builds/GolfTornadoSound/`.
Superseded APK/audit: `Legacy/20261024-235527-419-golf-tornado-audio-apk`; complete previous player:
`Legacy/20261025-000034-408-golf-tornado-audio-windows`.
No new phone FPS measurement or real multiplayer audio listening session.
Pending earlier medal changes are preserved; nothing staged, committed or
pushed for this task.

## Golf medal caption removal and head placement - 24 October 2026

Removed the extra text beneath Golf award medals. Medals now follow the
animated head with a small clearance, rather than a fixed offset above the
feet. Original medal artwork, top-three finisher selection, DNF exclusion and
scoring are unchanged. Full WindowsFinal updated; official AndroidSubmission
passed the strict decimal GB gate. APK **115,956,082 -> 115,954,850 bytes
(-1,232)**, headroom **884,045,150 bytes**. SHA-256:
**DA0AE6D2ED48A09AE36A363C079CC232DD4DCD69AA1E53C444819761BCE538EB**.

ZIP CRCs and Windows/Android scene-bundle catalogue hashes passed. Current
size audit remains in `Builds/SizeAudit/latest/`; no new artwork was added.
Evidence: `Builds/GolfMedalPosition/`. Superseded complete delivery and audit:
`Legacy/20261024-233050-868-golf-medal-position`. No phone performance claim. Changes remain uncommitted and
unpublished.

## Top-three Golf awards and merged tutorial update - 24 October 2026

Golf awards now show at most three finished players in the existing stroke/time
ranking; DNF is excluded. Exact alpha-trimmed gold/silver/bronze artwork comes
from the retained user master in `ArtSource/GolfAwards`. Three 256px delivery
textures use the existing importer; no scene geometry or scoring rule changed.
The development-only `-golfAwardsPreview` command displays three render-only
actors without changing a match; F10 replays the ceremony.

Complete 328-file WindowsFinal built at **22:59:22 Malaysia time** and the
three-actor rendered preview inspected (centre first, screen-left second,
screen-right third). The merged source includes upstream `7274043` tutorials
and local `0da78d8` Golf changes. Official AndroidSubmission passed its budget
gate, ZIP CRC, matching bundle catalogue hashes and APK v2 signature checks.
APK **115,888,100 -> 115,956,082 bytes (+67,982)**; strict decimal-GB headroom
**884,043,918 bytes**. SHA-256:
**BC7F51C09FEB7733D2D96A1561208DEF76E22D050565AF71CDAD03BE911D990D**.
The preceding merged build was 397,974 bytes above the prior 115,490,126-byte
Golf release. Current largest compressed entries are the island bundle
45,385,111, IL2CPP 12,820,493 and Unity 9,808,570 bytes. The three medal payloads
add about 66 KB compressed; no unexpected large growth was found in
`Builds/SizeAudit/latest/`.

Evidence: `Builds/GolfAwardsDelivery/` (build logs, rendered captures, package
and bundle integrity, source restoration, signature and portable generator
check). The generator reproduced identical sprites and metadata from a path
with spaces and an unrelated working directory. Superseded complete delivery:
`Legacy/20261024-225906-177-golf-awards`; earlier merge archive:
`Legacy/20261024-224006-503-golf-publish-merge`. The two accidental draft
archives were deleted manually by the user; current evidence is retained.
No new real multiplayer finish/DNF session or phone FPS measurement was run.
Build-generated prefab IDs/material provenance were reviewed and restored.

## Crab target priority and cart paving support - 24 October 2026

Crab push immunity is **5 seconds per ball shared across all crabs**. A crab
pushes only its selected ball, drops the target, retreats for 0.5 seconds toward
a checked point up to 2.5 m away, then seeks an eligible non-cooling ball. An
existing valid target keeps priority when another ball becomes ready. Without
an eligible target it resumes patrol and can reacquire a recovered ball within
the existing 16 m lane range. Chase speed and push impulse remain unchanged.
Host-owned crab motion, target/retreat state and shared cooldowns replicate at
10 Hz and survive host recovery. All peers require **protocol 85**.

Golf carts now use the authored thin Path/Approach paving ribbons as wheel and
floor support; the same ribbons no longer block the cart body sweep/overlap
checks. Paving seams stay drivable instead of trapping the body while wheels
sample the lower terrain. Border stones, rocks, columns, railings and walls keep
their collisions. No authored map, mesh, material or water rule was modified.

Complete **328-file WindowsFinal** built at **21:05:34 Malaysia time**; official
AndroidSubmission at **21:09:15**. APK **115,486,810 -> 115,490,126 bytes (+3,316)**;
strict decimal-GB headroom **884,509,874 bytes**. SHA-256:
**70B005F051BD58A39C701D46113813396CFC20D1D1F38AA69236E2980986DF6A**.
Budget gate, ZIP CRCs, matching scene-bundle catalogue hashes, compiled-code
presence and APK v2 signature passed. Largest compressed entries: island bundle
45,380,802; IL2CPP 12,806,470; Unity 9,808,570; metadata 2,818,632 bytes.
No unexpected large contributors or new art assets.

Source hashes match after restoring reviewed generated prefab-ID/material
provenance/newline noise. Existing unrelated pending work is preserved. Evidence:
ignored `Builds/GolfCrabTargetsPath/` (task diff, source hashes, package/signature
checks, build/audit logs and readiness report). No new gameplay, multiplayer or
physical-phone tests were run; driving/target behaviour needs in-game confirmation.
Previous complete player/APK: `Legacy/20261024-210033-592-golf-crab-targets-path/`.
An interrupted intermediate build log is retained in
`Legacy/20261024-210203-933-crab-target-build-draft/`.
No Git staging, commit or push.

## Tornado proximity transition, 28 m roaming and crab cooldown - 24 October 2026

Tornado roaming now uses **28 m** for spawn/waypoint sampling, movement bounds
and fade relocation. Body/haze visibility stays **50% / 40%**. Crab push immunity
is now **4 seconds per ball shared by all crabs**; the host's existing common
cooldown dictionary gates every crab before a push. Chase speed and push impulse
are unchanged. Wire protocol remains **84**.

Proximity presentation now uses a critically damped 0.6-second response, capped
per-frame integration and the rendered funnel's relocation opacity. The local
post path warms within 28 m, outside the visible 14 m tint zone, then stays
prepared for that event. Colour values and Gaussian CoC/radius animate from
neutral with full volume weight, avoiding the non-interpolating Gaussian mode
switch at tiny volume weights. A previously disabled camera's unrelated scene
tone mapping is suppressed while this neutral-to-haze path is prepared; existing
grading stays enabled on already-graded cameras. Camera options restore on exit.
UI stays sharp; no phone FPS or visual-playthrough result is claimed.

Complete **328-file WindowsFinal** built at **20:21:44 Malaysia time**; official
AndroidSubmission at **20:25:19**. APK **115,485,634 -> 115,486,810 bytes (+1,176)**;
strict decimal-GB headroom **884,513,190 bytes**. SHA-256:
**22DB85E318CED071F7187939AD027F8282F349F5B25E79DD328060E11EBA6147**.
Budget gate, ZIP CRCs, matching bundle-catalogue SHA-256 and APK v2 signature
passed. Largest compressed entries: island bundle 45,383,935; IL2CPP 12,800,680;
Unity 9,808,570; metadata 2,818,047 bytes. No unexpected asset growth.

Four game source files changed for this task. Source hashes match after restoring
reviewed generator-only prefab IDs/material provenance/newline noise; authored
maps and unrelated assets remain unchanged. Evidence: ignored
`Builds/GolfHazardSmooth28/` (task diff, source hashes, build/audit logs, package
checks and readiness report). No new gameplay, multiplayer, visual-playthrough
or physical-phone tests were run; smoothness in actual play remains unverified.
Previous complete player/APK archived in
`Legacy/20261024-201805-345-golf-hazard-smooth28/`.
No Git staging, commit or push; existing pending work is preserved.

## Tornado opacity and 35 m roaming - 24 October 2026

Tornado body visibility is now **50%**, evolving ground haze at most **40%**.
Random spawn/waypoint sampling, movement bounds and distant-ball relocation all
use the shared **35 m** radius. Body/debris animation, speed/growth, sand effects,
catch/throw rules and protocol **84** are unchanged. The body opacity changes
from 0.4 to 0.5; the fog changes from 0.2 to 0.4; other layer opacity stays 0.4.

Complete 328-file WindowsFinal built at **19:58:35 Malaysia time**; official
AndroidSubmission at **20:01:54**. APK **115,486,074 -> 115,485,634 bytes (-440)**;
strict decimal-GB headroom **884,514,366 bytes**. SHA-256:
**6CBB41BBEA8556AD134099D29F1648AE198A87A02B88762639DD8B01BCC2EA50**.
Budget gate, ZIP CRCs, matching scene-bundle catalogue hashes and APK v2 signature
passed. Largest entries: island bundle 45,379,627; IL2CPP 12,803,997; Unity
9,808,570; metadata 2,817,863 compressed bytes. No unexpected size growth.

Only three game source files changed for this tuning. Full source hashes match
after restoring reviewed generator-only prefab/material provenance noise; maps
and unrelated assets remain unchanged. Evidence: ignored
`Builds/GolfTornadoVisibility35/` (task diff, hashes, build logs, package/signature
checks and readiness report). No new gameplay, multiplayer, visual-playthrough
or physical-phone tests were run; mobile performance is unverified.
Superseded complete player/APK archived in
`Legacy/20261024-195447-572-golf-tornado-visibility35/`.
No Git staging, commit or push; earlier pending work is preserved.

## Tornado atmosphere and ball-centred roaming - 24 October 2026

Tornado body/debris now use 40% visible opacity and the evolving ground haze
at most 20%. Inward ground spirals, entrained foliage and sand rise progressively;
leaving sand clears from bottom to top. Rooted foliage bends inward. Camera
proximity smoothly adds grey/desaturation and Gaussian depth blur while overlay
UI stays sharp; previous camera settings restore on exit. Actual runtime geometry,
materials and postprocess were inspected through editor authoring renders.
These renders do not establish gameplay, multiplayer or physical-phone results.

The host now selects random dry, cup-safe points within 20 m of each unfinished
player's live ball. Positions/velocity/waypoints and sand-source history are
replicated at 10 Hz and included in host recovery. Small funnels move slowly;
eight-second growth permits component caps of 6 m/s sideways and 5 m/s forward
only at full size. Distant shots/resets use a fade relocation. Catch/throw rules,
crab parameters, ownership and scores are unchanged. All peers require protocol
**84**. See [hazard details](GOLF-HAZARDS.md).

Complete **328-file WindowsFinal** built at **19:21:19 Malaysia time** and official
AndroidSubmission at **19:26:14**. APK **113,331,282 -> 115,486,074 bytes
(+2,154,792)**; remaining strict decimal-GB headroom **884,513,926 bytes**.
SHA-256: **5895EA3158627887B0F8E8A3639ECA18A5072E9C0A00AE3BA346DCAA4C98AF85**.
Budget gate, ZIP CRC integrity and APK v2 signature passed. The roughly 2.15 MB
increase is primarily URP postprocess shader/resource data; the island bundle
is 45,380,035 compressed bytes (3,412 fewer after rebuilding). No new raster textures or changed maps.

Source a653bd1 plus preserved earlier/current uncommitted changes. Expected
source hashes match after restoring reviewed prefab-ID/provenance/newline build
noise. Evidence: ignored `Builds/GolfTornadoAtmosphere/` (authoring renders,
source hashes, logs, package manifests, signature and readiness report).
No new gameplay, multiplayer or physical-device tests were run; phone FPS and
in-game network recovery remain unverified. Previous complete player/APK and
first draft render log are archived in
`Legacy/20261024-191731-265-golf-tornado-atmosphere/`. The incomplete space-exhausted
backup is archived in `Legacy/20261024-185758-490-tornado-incomplete-backup/`.
No Git staging, commit or push.

## Reference tornado model and entry growth - 24 October 2026

Tornado sideways peak is now **6 m/s** (0.75 rad/s with the original 8 m
amplitude); forward/backward peak remains 5 m/s. Other hazard physics and crab
parameters are unchanged. [Tornado presentation](GOLF-HAZARDS.md) now uses an
editable grey-blue hollow rolled funnel with cloud folds, white wind ribbons,
16 leaves, 12 stones and 13 dust puffs. Four profiles blend continuously; the
shared event age grows the visual from 18% to 100% over eight seconds. Catch
radius and gameplay do not scale with the cosmetic growth. Reduced Motion
freezes orbit/profile cycling. Existing 70 m culling remains. Three shared
mesh/material layers, retained buffers and cached shape samples are used.

Exact [supplied PNG references](../ArtSource/GolfEvents/References/README.md)
are retained outside Unity with byte hashes; they are not packaged runtime
textures. This is a native 3D interpretation, **not a pixel-identical copy of the
2D reference sheets**. Actual runtime meshes/materials were inspected through
editor-rendered growth, four silhouettes and a close view; images and render
log are under ignored `Builds/GolfTornadoArt/Preview/` and `preview.log`.
These authoring renders do not establish in-game multiplayer or phone quality.

Complete 328-file WindowsFinal built at **18:03:06 Malaysia time**;
official AndroidSubmission at **18:08:04**. Actual APK:
**113,320,318 -> 113,331,282 bytes (+10,964)**; strict decimal-GB headroom
**886,668,718 bytes**. SHA-256:
**9B7A55B6DAB5A1F087E439CFEE2804097676C452AEFED0478B07D9E691BD6064**.
Both builds, budget gate, 1,238 ZIP entry CRCs and APK v2 signature passed.
Largest compressed contributors: islands 45,383,447; IL2CPP 12,780,941; Unity
9,808,570; metadata 2,817,263; warning art 2,343,698; event atlas 1,692,260 bytes.
Growth is small shader/code/bundle rebuilding; no unexpected large contributor.
All peers should use the updated build; wire protocol remains 83.

Source `a653bd1` plus preserved earlier and current uncommitted changes. Expected
source hashes match after restoring only reviewed prefab-ID/provenance/newline
build noise; authored maps and unrelated art remain unchanged. Evidence is under
ignored `Builds/GolfTornadoArt/`, including source integrity, package manifests,
AndroidAudit, signature and final build/render logs. **No new gameplay,
multiplayer or physical-device tests were run; phone FPS is unverified.**
Old complete Windows player, APK and reviewed earlier preview drafts are archived
in `Legacy/20261024-175949-250-golf-tornado-art/`. No Git staging, commit or push.

## Crab pursuit and tornado patrol tuning - 24 October 2026

Only the requested [hazard parameters](GOLF-HAZARDS.md) changed: crab chase
5.0 m/s; additive ball velocity 6.5 m/s horizontal and 3.5 m/s upward; per-ball
cooldown shared by every crab 2.5 seconds. Tornado component peak speeds are
9 m/s sideways and 5 m/s forward/backward, retaining 8 m / 2 m amplitudes and
all other hazard rules. Protocol remains 83; use the updated build on all peers.

Complete 328-file Windows player built at **16:13:57 Malaysia time**, then
promoted to `Builds/WindowsFinal/` after both games closed. All candidate file
hashes and the source snapshot matched before promotion. The superseded full
player is archived in `Legacy/20261024-163328-314-golf-hazard-tuning-player/`.
Official AndroidSubmission completed at **16:18:05**. Actual APK:
**113,325,786 -> 113,320,318 bytes (-5,468)**; strict-cap headroom:
**886,679,682 bytes**. SHA-256:
**105503DD1443720BB9B59AB8BFAE912B4FF5B7FAB5A252C43C78F73B9350E6E5**.
Build/budget gate, 1,238 ZIP entry CRCs and APK v2 signature passed. Largest
compressed entries: islands 45,373,230; IL2CPP 12,777,355; Unity 9,809,039;
metadata 2,816,664; warning art 2,343,698; event atlas 1,692,260 bytes.
No image/audio assets were added, and there is no unexpected large contributor.

Source base `a653bd1` plus preserved existing and current uncommitted work.
Source hashes match after restoring reviewed prefab local-ID, provenance and
newline generation noise; authored maps/assets remain unchanged. Evidence is
under ignored `Builds/GolfHazardTuning/`, including final build logs,
`AndroidAudit/`, source integrity, package entry sizes/signature and Windows
candidate manifest. No gameplay, multiplayer or device tests were run; phone
performance remains unverified. No Git staging, commit or push.
Superseded APK archive: `Legacy/20261024-161433-404-golf-hazard-tuning-apk/`.

## Cart pedestrian impacts and compact event layout - 24 October 2026

Updated [Golf impacts and HUD](GOLF-CART-HUD-POLISH.md): 64 car/car sparks
(0.08-0.18 m, 0.45-0.65 s), approximately 1-3 m recoil per cart with held-throttle
protection; Host-authoritative pedestrian launch with Aim/input cancellation,
existing jump pose/network movement and pooled impact audio/particles. Chat and
sticker launchers share the Aim fade. Event badges retain their art at 75% scale;
final countdown retains its central introduction and docks at bottom centre at
70% of the previous corner size (35 x 35 numerals). All peers need protocol 83.

Complete 328-file Windows player built at **01:02:49 Malaysia time**; official
AndroidSubmission at **01:05:55**. Actual APK **113,325,590 -> 113,325,786 bytes
(+196)**; strict decimal-GB headroom **886,674,214 bytes**. SHA-256:
**BACA422F6D6AE76EE67723E983EF4A7160703060E6861007D9889DB9AFB3ED7F**.
Build/budget gate, all 1,238 ZIP entry CRCs and APK v2 signature passed. Largest
compressed entries: islands 45,379,025; IL2CPP 12,777,030; Unity 9,809,039;
metadata 2,816,664; warning artwork 2,343,698; event atlas 1,692,260 bytes.
No new bitmap/audio assets; no unexpected large contributor. New hit methods/RPC
are present in the resulting APK metadata and absent from the superseded APK.

Base source `a653bd1` plus existing and current uncommitted changes. Source hashes
match the expected snapshot after restoring reviewed prefab local-ID/provenance/
newline generation changes. Authored maps/assets were preserved. Evidence:
ignored `Builds/GolfImpactLayout/`, including final logs, source integrity record,
Windows manifest, AndroidAudit, package sizes/entries, code presence and signature.
Compilation and packaging passed; **no new gameplay, multiplayer or device tests
were run**, and phone FPS/quality are unverified. No staging, commit or push.

Superseded complete player/APK archived in
`Legacy/20261024-005046-827-golf-impact-layout/`; intermediate player archived in
`Legacy/20261024-005919-401-golf-impact-intermediate/`. Build evidence was retained.
The first Android attempt was interrupted to include the recoil throttle guard;
the final Android build completed successfully.

## Golf cart impacts, HUD and finisher honours - 24 October 2026

Implemented [cart/HUD polish](GOLF-CART-HUD-POLISH.md): Host-authoritative recoil
for both colliding carts with existing swept/support movement checks; broadcast
CartBump sparks/audio; full-card leaderboard opening; leaderboard top-left with
chat/stickers below; Settings top-right matching Football; event notices on its
left with the wind compass to the left of the badge. Finishing clock docks below
the right event stack. The in-game Players profile launcher is retired globally.
Movement and Jump return the Golf shot camera; crab nudges no longer initiate
ball replay. A separate replicated hazard-replay flag retains tornado playback
and stale-shot invalidation; matching **protocol 82** is required on every peer.
Golf honours show only completed players, preserving score/tie ranks, centre/left/
right staging and gold/silver/bronze rank/name badges. Fishing's awards roster and
staging retain their existing behaviour. No map or image assets were changed.

Final full Windows build succeeded at **23:54:30 Malaysia time, 23 October**.
Official AndroidSubmission succeeded at **00:00:57 Malaysia time, 24 October**.
Actual local APK **110,542,380 -> 113,325,590 bytes (+2,783,210)**; strict decimal-GB
headroom **886,674,410 bytes**. Build/budget gate, ZIP CRC and independent APK v2
signature passed. SHA-256:
**331B7F6E57C448A688A535531E0B51C528DAEA5A8F5C795E0840CFA8F2169847**.
Largest compressed entries: islands **45,378,945**, IL2CPP **12,776,984**, Unity
**9,809,039**, metadata **2,816,597**, warning art **2,343,698**, event atlas
**1,692,260** bytes. The old local APK predates the latest pulled source; major
added entries are already-merged ControlArt Frames/CourtIcons/FishingIcons and
StormPuff assets. This task adds no bitmap assets; no unexpected large contributor.

Builds use base `a653bd1` plus the uncommitted task changes. Source hashes were
checked after restoring only reviewed prefab-ID/provenance/newline generation
noise; authored maps/assets match the pre-build snapshot. Evidence is under
ignored `Builds/GolfCartHudPolish/`, including both Windows logs, the final source
hash snapshot, `AndroidAudit/`, APK entry deltas, signature and package manifest.
No new gameplay, multiplayer or device tests were run; successful compilation
and packaging do not establish those results. No phone FPS/quality claim.
Superseded full player: `Legacy/20261023-234644-549-golf-cart-hud-player/`;
superseded local APK: `Legacy/20261023-235534-218-golf-cart-hud-apk/`.
No staging, commit or push.

## Shared illustrated island rules - 24 October 2026

Football, Basketball and Golf now share Fishing's four-page rules shell,
animations, native control examples, first-visit flow and Settings/How to Play
reference. See [Illustrated island rules](ISLAND-RULES.md). The new decorative
art is compressed for delivery; full-resolution masters, generation prompts
and AI asset disclosure remain in `ArtSource/SportsTutorial/`.

Official AndroidSubmission: **113,332,706 -> 113,711,916 bytes
(+379,210)**. Headroom below the strict 1,000,000,000-byte boundary:
**886,288,084 bytes**. SHA-256:
**7C5478B3B053A49410A221DCCD4239107FA81D9C9E2BC951DDFC30E529954109**.

The build/budget gate, ZIP CRC, APK v2 signature and five-scene bundle size/hash
passed. Both shared rules types and all three tutorial sprites are present;
the development review fixture is absent from release metadata. All
**620** frozen `_Game` C# source hashes still match.
The complete **327-file** Windows player passed **125** focused entry,
navigation, input-gating, replay, landscape-bounds and nonblank-render checks.
Its sixteen page captures were retained; representative pages were visually
reviewed. Every promoted payload hash matches. This is UI verification, not a
full gameplay regression or a phone/WAN test.

Largest compressed APK entries: islands **45,386,017**,
IL2CPP **12,777,349**, Unity
**9,809,038**, metadata
**2,817,658** bytes. The retained entry inventory records
all contributors; serialized Unity reports and PNG source sizes measure
different things. No world art was downscaled for this tutorial change.

Evidence: `Builds/IslandRules/Task-20261024-003827/`, including
`RuntimeReview-Final-Golf/`, `apk-after.json`, `apk-signature.txt`,
`release-content-check.json`, `AndroidAudit/` and `windows-promotion.json`.
Recovery: `Legacy/20261024-011113-440-island-rules-delivery/` (previous APK/player)
and `Legacy/20261024-012403-721-island-rules-final-player/` (superseded UI revision).
Source base: `a653bd1` plus the reviewed local tutorial changes. Android physical
touch input, appearance/FPS and live WAN behavior remain unverified.

## Local rebuild after pulling main — 23 October 2026

Pulled `main` from `00eebcd` to `a653bd1` and fetched its Git LFS assets.
The official AndroidSubmission build produced **113,332,706 bytes**, compared
with the previous local APK's **111,342,842 bytes**: **+1,989,864 bytes**.
Remaining headroom below the strict 1,000,000,000-byte limit is
**886,667,294 bytes**. SHA-256:
**8B53DDB80FBCE35BCE8A00ED3DEA7F48A521CCC18CB6CFA59E03AB875E9CA0D5**.

The build/budget gate, ZIP CRC, APK v2 signature and five-scene bundle hash/size
passed. All **617** current `_Game` C# source hashes match the frozen rebuild
snapshot. Largest compressed entries: islands **45,392,945**, IL2CPP
**12,770,545**, Unity **9,809,038**, metadata **2,816,268** bytes. New serialized
data entries account for much of the growth; the scene bundle grew **162,156**
bytes and IL2CPP **124,346** bytes. No additional source art or lossy conversion
was introduced for this rebuild. These measurements describe this local APK;
earlier records below belong to their recorded source snapshots.

Evidence: `Builds/LocalRebuild/Task-20261023-220613/`, including
`apk-after.json`, `apk-entry-growth.json`, `apk-signature.txt` and `AndroidAudit/`.
Previous local APK recovery:
`Legacy/20261023-220904-653-local-rebuild-previous-apk/`.
The matching complete **327-file** Windows player passed a brief offline Golf
startup check and normal exit, then was promoted to `Builds/WindowsFinal/` with
every file hash verified. Its superseded player is recoverable from
`Legacy/20261023-224919-836-local-rebuild-windows-player/`. No broad gameplay
regression suite was rerun for this build-only request.
Android device appearance, touch input/FPS and WAN multiplayer were not tested.

## Open type labels and activity cameras — 23 October 2026

The [approved second label design](PLAYER-IDENTITY-CAMERAS.md) is now shared
across all four sports, with **You** for the local character. Golf's Course and
Fishing's Lagoon views frame their actual activity. The complete **334-file**
Windows player was promoted with every payload hash verified. **935 assertions**
passed across focused camera/label checks, additional screen shapes, two-player
identity replication, Fishing gameplay, chat UI and court broadcasts. The
runner's default path also passed from a path containing spaces and an unrelated
working directory. All **628** current `_Game` C# files match the saved source
snapshot; **471 Windows** and **331 Android** compiled runtime source hashes match.

Official AndroidSubmission completed at **21:21 Malaysia time**. Actual APK:
**113,342,826 -> 113,344,522 bytes (+1,696)**. Remaining headroom below the strict
1,000,000,000-byte boundary: **886,655,478 bytes**. SHA-256:
**9EFC70C3405A0B4F7F57CD66E71D18178EAB37A85C0FA3178BE3E80AF0B60476**.
The build/budget gate, ZIP CRC, all **1,238** exact audit entries, five-scene
bundle hash/size, APK v2 signature, native scanner assets, seven Java bridges
and eight room-chat stickers passed. Identity/activity camera code is present;
the development review and filming types are absent from release metadata.

Largest compressed entries: islands **45,408,219** (-7,418), IL2CPP
**12,767,728** (+8,730), Unity **9,809,041** (unchanged), metadata **2,816,268**
(+309), and classes.dex **1,382,331** (unchanged). No new art, lossy conversion
or unexpected large contributor was introduced. These are APK entry sizes;
serialized asset reports are a different measurement. Phone appearance/FPS
remains unverified.

Evidence: `Builds/PlayerIdentityCameraQA/`, including the final four-sport
`Run-20261023-211139-123/`, and `Builds/SizeAudit/latest/`. Recovery:
`Legacy/20261023-212308-021-open-type-activity-cameras/`. The normal Unity cache
was reused. The complete tested Windows player is in `Builds/WindowsFinal/`.

## Golf cinematic extension and current merged game — 23 October 2026

The [full 57.3-second trailer](TRAILER-PROTOTYPE.md) now includes all four sports:
ten fast Golf shots cover the real swing, five random events and Island Rush,
with a three-second full tornado view and a close crab shot. The complete
334-file Windows player passed 316 main filming assertions and 89 Golf
cancellation assertions; all 634 C# files match the shared checkout.

Fresh official AndroidSubmission completed around **20:28 Malaysia time**.
Actual APK: **112,464,858 -> 113,342,826 bytes (+877,968)**. Remaining headroom
under the strict 1,000,000,000-byte limit: **886,657,174 bytes**. SHA-256:
**4D2731D19026318CCB13D948B67EED864B5696358BFE3E2BA7B37CBD2261246E**.
Build/budget gate, ZIP CRC, all **1,238** exact audit entries, five-scene
catalog/bundle size and hash, APK v2 signature, native scanner assets and seven
Java bridges passed. `TrailerGolf` and the other filming types are absent from
release metadata; the actual Golf hazard game code is included.

This release also brings the already-finished Golf hazards and current merged
source into Android, so its growth is not the filming fixture's size. The largest
new compressed entries are storm puffs **389,979**, Crab badge **97,529**,
Tornado badge **83,679**, and the crab mesh **67,576** bytes. The scene bundle is
**45,415,637** (+141,621), IL2CPP **12,758,998** (+25,575), Unity **9,809,041**
(+466), metadata **2,815,959** (+2,606), and classes.dex **1,382,331** (unchanged).
The shared SimpleLit shader adds **55,100** bytes. Exact asset mappings are saved
with the audit; no unexpected large contributor was found. No assets were
downscaled or reauthored for this task. Phone quality/FPS remains unverified.

Evidence: `Builds/TrailerPrototype/Revision7/`, `Review-V7/` and the current
`Builds/SizeAudit/latest/`. Recovery:
`Legacy/20261023-203017-138-trailer-golf-delivery/`. The existing filming
workspace/cache was reused rather than creating another full Unity copy.

## Trailer crowd and celebration review — 23 October 2026

The [46.9-second full review](TRAILER-PROTOTYPE.md) adds 3,072 staged animated
supporters, walking spectator shots, both sports' winning celebrations, the
live stadium screen and an unobstructed bridge ending. Golf remains deferred.
The complete 333-file Windows player retains current room chat and approved
control artwork; all 623 C# source files match the checkout at promotion.

Official AndroidSubmission completed at **18:09 Malaysia time**. Actual APK:
**111,093,062 -> 112,464,858 bytes (+1,371,796)** from this task's starting
baseline, leaving **887,535,142 bytes** below the strict 1,000,000,000-byte cap.
It is **3,284 bytes** larger than the concurrent shared-control delivery below.
SHA-256:
**C0C168E47D23AF30D931E9C086C5884D11180CE60AA9F07BB62C745EDD0D74AF**.
The build gate, ZIP CRC, all **1,224** exact audit entries, five-scene catalog/
bundle size and hash, APK v2 signature, native scanner assets and seven Java
bridges passed. Later changes affect only development-only crowd verification
and whitespace; Android gameplay code is unchanged.

The three concurrent control-art atlas entries explain **1,335,532 bytes** of
growth. Largest compressed entries: islands **45,274,016** (+29,721), IL2CPP
**12,733,423** (+3,273), Unity **9,808,575** (unchanged), metadata **2,813,353**
(+298), classes.dex **1,382,331** (unchanged). These are APK entry sizes, not
source-folder or uncompressed report sizes. The crowd uses existing art and
runtime poses. Filming types are absent from release metadata; room-chat and
shared-control types remain present. Phone appearance/FPS remains unverified.

Evidence: `Builds/TrailerPrototype/Revision6/` and `Review-V6/`. Recovery:
`Legacy/20261023-182615-960-trailer-crowd-windows/` and
`Legacy/20261023-182541-322-trailer-crowd-android/`. The existing filming
workspace/cache was reused; no additional full Unity checkout was created.

## Approved shared control artwork — 23 October 2026

The [shared control artwork](CONTROL-ART.md) replaces 16 editable presentations
with reusable glossy frames, live captions and separate illustrations; eight
existing Golf/Fishing designs are retained. All four gameplay HUDs and both
Layout entry points use the shared builders. The three RGBA masters and approved
review boards are retained outside Unity; the runtime copies and normalized
sprite manifest are included in the project.

Official AndroidSubmission completed at **17:58 Malaysia time**. Actual APK:
**111,093,062 -> 112,461,574 bytes (+1,368,512)**. Remaining strict-cap headroom:
**887,538,426 bytes**. SHA-256:
**19A83ACB878484909A83000BB45B5A04237BFA3C4F7E3C062BE122FEF8330507**.
ZIP CRC, all **1,224** exact audit entries, the five-scene catalog/bundle size
and hash, APK v2 signature and build-budget gate passed.

The three new texture entries contribute **1,335,532 compressed bytes**.
Largest entries: islands **45,270,727** (+26,432), IL2CPP **12,733,423** (+3,273),
Unity **9,808,572** (-3), metadata **2,813,360** (+305), classes.dex **1,382,331**
(unchanged). The remaining growth is small manifests and serialization. These
are measured APK entry sizes, not source PNG sizes or GPU memory. Android uses
ASTC 6×6 at the original atlas dimensions with alpha; desktop uses RGBA32.
Physical-phone appearance and performance remain unverified.

Evidence: `Builds/ApprovedControlsQA/`, including the preserved SizeAudit and
exact APK comparison. Previous APK/audit recovery:
`Legacy/20261023-175227-897-approved-controls-android/`.

## Complete cinematic trailer — 23 October 2026

The [full filming prototype](TRAILER-PROTOTYPE.md) is **41.5 seconds / 1,245
frames / 43 shots**, silent at 1920×1080 / 30 FPS. It adds group teleportation,
station/boarding shots, the Skyrail journey, shoreline scenery and the four-friend
sea-view ending to the exact approved sports clips. Golf stays deferred.
The current player passed **333 main assertions** (114 travel, 70 Football,
68 Basketball, 81 Fishing), plus teleport/riding cancellation and ending checks.
All **331 Windows payload files** matched the tested candidate. Its **617 C#
files** matched the checkout; current room chat and shared controls are retained.

Official AndroidSubmission completed at **17:16 Malaysia time**. Measured APK:
**111,098,026 -> 111,093,062 bytes (-4,964)**, leaving **888,906,938 bytes** below
the strict 1,000,000,000-byte cap. SHA-256:
**63E5E0ADC293312F1706F78086FC9CAC444374513C0B1CF139FC3310F0C9A201**.
ZIP CRC, all **1,220** exact audit entries, five-scene catalog/bundle hash and
size, APK v2 signature, native scanner assets and seven Java bridges passed.
Filming types, including the new travel director, are absent from release
metadata. Room chat and shared-control types remain present.

Largest compressed entries: islands **45,244,295** (-5,038), IL2CPP
**12,730,150**, Unity **9,808,575**, metadata **2,813,055**, classes.dex
**1,382,331** bytes (the latter four unchanged). Small serialization/compression
changes account for the net reduction; no art or lossy conversion was added.
Staged offline capture does not establish multiplayer filming or phone FPS.

Evidence: `Builds/TrailerPrototype/Revision5/` and `Review-V5/`. The existing
filming workspace/cache was reused. Recovery batches:
`Legacy/20261023-171851-869-trailer-full-windows/` and
`Legacy/20261023-172018-029-trailer-full-android/`.

## Fishing filming extension — 23 October 2026

The [sports filming prototype](TRAILER-PROTOTYPE.md) adds seven seconds of
Fishing to the exact approved Football/Basketball footage. The joined silent
1080p30 review is **24 seconds / 720 frames**. Fishing passes **81 checks**;
current-player Football/Basketball regressions pass **70/68**, for **219 total**.
The complete **330-file WindowsFinal** payload matched the tested candidate by
SHA-256. The **616-C#-file** snapshot matched the checkout at promotion and
preserves the final room-chat/Fishing placement fix and shared control designs.

Official AndroidSubmission completed around **16:34 Malaysia time**. Actual APK:
**111,107,490 -> 111,098,026 bytes (-9,464)**. Remaining headroom under the strict
1,000,000,000-byte cap: **888,901,974 bytes**. Build/budget gate, ZIP CRC, all
**1,220** audit entries, five-scene catalog/bundle hash and size, APK v2 signature,
native scanner assets and seven QR/account/profile Java bridges passed. Room-chat
and shared-control types remain present; filming types are absent from release
metadata. SHA-256:
**9E156E55DEF3E0FFA3435912BC4BD481622C11069EFD4430A19FEBBAB5B8722C**.

Largest compressed entries: islands **45,249,333** (-9,473), IL2CPP
**12,730,150** (+1), Unity **9,808,575** (+3), metadata **2,813,055** (+7), and
classes.dex **1,382,331** (unchanged) bytes. The small net reduction is dominated
by bundle serialization/compression; no art or lossy asset change was introduced.
Windows capture does not validate phone controls/FPS or actual multiplayer filming.

Evidence: `Builds/TrailerPrototype/Revision4/`, including source/player manifests,
APK verification/signature, the preserved SizeAudit and SSD allocation reports.
Current video: `Builds/TrailerPrototype/Review-V4/Football-Basketball-Fishing-Preview.mp4`.
Recovery archives: `Legacy/20261023-163205-790-trailer-fishing-windows/` (previous
complete Windows player and two camera drafts) and
`Legacy/20261023-163925-624-trailer-fishing-android/` (previous APK, mapping and audit).

## Room chat — final combined delivery, 23 October 2026

The [room chat implementation](ROOM-CHAT.md) adds one room-session timeline
across the lobby and all four islands, text, eight approved free fish stickers,
brief avatar bubbles, history, unread counts and local mute/bubble preferences.
Fishing's final chat position clears its scoreboard and protects the aiming
reticle from bubbles. The complete **329-file Windows player** retains the newer
shared control designs and both filming launchers. Its final candidate passed
**356 assertions**: 198 multiplayer/history/recovery, 95 all-island UI and 63
host UI. All **453** compiled runtime source documents matched the checkout.

The fresh combined **AndroidSubmission** release completed at **16:02:41
Malaysia time**, after the last chat code change. Its **321** compiled production
documents match the checkout, including that Fishing fix. Actual bytes:
**110,721,046 at task opening -> 111,107,490 final (+386,444)**. The immediate
predecessor was **111,097,730 (+9,760)**; concurrent releases are included, so
this is not an isolated chat-only size comparison. Remaining headroom is
**888,892,510 bytes** below the strict 1,000,000,000-byte limit. APK SHA-256:
**DF887352BE63413A31832E62F47AAA6F83B32849D8B38881AAE37212191C4B18**.

Independent checks passed: APK budget, ZIP CRC, all **1,220** exact audit entries,
five-scene catalog/bundle size and hash, APK v2 signature, native scanner assets,
all seven QR/account/profile Java bridges, chat runtime types, all eight sticker
resources, and absence of chat review/filming types from release metadata.
Largest compressed entries are islands **45,258,806**, IL2CPP **12,730,149**,
Unity **9,808,572** and metadata **2,813,048** bytes. The eight sticker entries
remain **344,607 compressed bytes**; editable transparent masters are retained.

Evidence: `Builds/RoomChatQA/FinalReview/` and final runs on ports 7986–7988.
The Windows promotion record verifies every payload file by SHA-256. The build
wrapper's unset-exit-code report is documented in the preceding release record;
Unity succeeded and the independently checked APK is fresh. Earlier failed
attempts are not treated as validation. Physical Android touch, native keyboard,
small-screen readability, phone FPS and WAN/Relay behavior remain unverified.
Prior delivery/isolated inputs are archived; the current Windows delivery note
lists the recovery batches. Unity caches and installed dependencies remain in place.

## Shared control designs — 23 October 2026

All **24** layout-editable controls use the same presentation factories in
Basketball, Football, Golf and Fishing, including both Layout editor contexts.
See [design editing and verification](CONTROL-LAYOUT.md). No new artwork or
lossy asset conversion was introduced. Existing layout preferences remain valid.

The exact complete Windows player passed **2,586 assertions** (**1,185** normal,
**1,401** with deliberate shared-design edits). All **329** payload files matched
by SHA-256 when promoted to `Builds/WindowsFinal/`, before updating its delivery
note. Its **453** compiled runtime source documents include the layout changes;
three later concurrent room-chat documents are excluded from that Windows
snapshot, as detailed in the control-layout report. The prior validated
protocol-80 room chat, stickers and filming code are retained.

Official **AndroidSubmission** succeeded; the APK was written at
**16:02:40 Malaysia time**. The actual APK changed from the immediately preceding **111,097,730** to
**111,107,490 bytes (+9,760)**. Headroom below the strict 1,000,000,000-byte
cap is **888,892,510 bytes**. The task-opening APK was **110,720,790 bytes**;
the intervening room-chat/filming releases account for much of that larger
**+386,700-byte** difference. This is not an isolated control-code size delta.

The build/budget gate, ZIP CRC, all **1,220** exact audit entries, five-scene
catalog/bundle size and SHA-256, and APK **v2 signature** passed. Shared design
classes are present in release metadata; the development mutation hook and
layout review are absent. All **321** source documents in the stripped Android
PDB matched the production checkout at verification, including the newer chat
code; this later APK is a different source snapshot from the tested Windows
player. Native QR/account/photo classes remain present. APK SHA-256:
**DF887352BE63413A31832E62F47AAA6F83B32849D8B38881AAE37212191C4B18**.

Largest compressed entries: islands **45,258,806** (+12,767), IL2CPP
**12,730,149** (-3,429), Unity **9,808,572** (-3), metadata **2,813,048** (+294),
and classes.dex **1,382,331** (unchanged) bytes. Growth is concentrated in the
regenerated scene bundle and small runtime records. No unexpected artwork
increase was found. Serialized asset totals are not APK byte measurements.

Evidence: `Builds/SharedControlDesignQA/` (normal/mutation suites, source and
player manifests, promotion hashes, portable-link checks, APK verification and
signature) and `Builds/SizeAudit/latest/`. Current deliveries remain
`Builds/WindowsFinal/` and `Builds/Android/WhatTheFish-release.apk`.
The wrapper's post-build unset-exit-code check produced a false failure; Unity
itself exited successfully and the exact APK passed independent verification.
Historical Golf probe limitations are recorded separately in the layout report.
Physical-phone touch, cutouts and FPS remain unverified.

Recovery archives: `Legacy/20261023-154823-417-shared-controls-old-windows-bundle/`,
`Legacy/20261023-155816-458-shared-controls-android/`, and
`Legacy/20261023-160046-645-shared-controls-windows/`. The failed first Windows
packaging log is retained as evidence; the tested release is the successful
full rebuild after disk space became available.

## Basketball filming extension — 23 October 2026

The [sports filming guide](TRAILER-PROTOTYPE.md) now includes a **seven-second
Basketball take** and a **17-second Football-to-Basketball edit**, both silent
at 1920×1080 / 30 FPS. The exact player passed **68 Basketball and 70 Football
assertions**. All 210/300 input frames and all **510 joined frames** decoded.
The complete **329-file WindowsFinal** payload matched the tested candidate by
SHA-256 before updating its delivery note.

Official AndroidSubmission completed at **15:40:51 Malaysia time**. Actual APK:
**110,720,790 -> 111,097,730 bytes (+376,940)**. Remaining headroom under the
strict 1,000,000,000-byte cap is **888,902,270 bytes**. Build/budget gate, ZIP
CRC, all **1,220** exact audit entries, five-scene catalog/bundle hash and size,
APK v2 signature, native scanner assets and all seven QR/account/profile Java
bridges passed. Filming types are absent from release metadata. APK SHA-256:
**A1A03A4B888BE2B782D91B956CF43221615E054FC14458745630E82800B4EEB4**.

This measured release preserves the already validated **protocol-80 room chat
and eight stickers**. The eight sticker entries account for **344,607 compressed
bytes**; their APK entry GUIDs match the RoomChat sprite metadata. Other largest
entries: islands **45,246,039** (-7,150), IL2CPP **12,733,578** (+35,036), Unity
**9,808,575** (+371), and metadata **2,812,754** (+2,207) bytes. Growth is
consistent with the chat/sticker integration relative to the protocol-79 APK.
This filming task adds no art, lossy conversion or release filming runtime.

The tested **612-C#-file** snapshot includes chat production code and filming
changes, but excludes the later concurrent control/HUD redesign. Differences
are recorded in `Builds/TrailerPrototype/Revision3/concurrent-differences.json`.
The new APK validates that snapshot, not the advancing shared checkout.
Physical phone controls/FPS, native Android sign-in and actual multiplayer
filming remain unverified by this task.

Current outputs: `Builds/WindowsFinal/`,
`Builds/Android/WhatTheFish-release.apk`, and
`Builds/TrailerPrototype/Review-V3/Football-Basketball-Preview.mp4`.
Evidence: `Builds/TrailerPrototype/Revision3/` (source/native/player manifests,
APK verification, growth, signature and SizeAudit) and
`Builds/TrailerPrototype/Review-V3/` (takes, assertions and decoded-video
records). The edit reuses the approved Football MP4; the current player also
has a separate successful Football regression capture.

Recovery archives:
`Legacy/20261023-151618-427-trailer-basketball-baseline/` (prior isolated
Football candidate),
`Legacy/20261023-153849-207-trailer-basketball-windows/` (prior complete
protocol-80 player and two superseded camera drafts), and
`Legacy/20261023-154320-666-trailer-basketball-android/` (prior APK/shared audit).

## Faster cinematic prototype, revision 2 — 23 October 2026

The revised [Football prototype](TRAILER-PROTOTYPE.md) is **10 seconds,
eight shots, 1920×1080 at 30 FPS**. Low side tracking, a feet/ball close-up
and a pitch-side goal view follow continuous gameplay across faster cuts.
The exact Windows candidate passed **70 assertions** and all **300 video
frames** decoded. The complete **328-file WindowsFinal** payload matched its
tested candidate by SHA-256 before updating the delivery note.

Official AndroidSubmission completed at **15:00:31 Malaysia time**. Actual
APK: **110,721,046 -> 110,720,790 bytes (-256)**. Remaining headroom under the
strict 1,000,000,000-byte cap: **889,279,210 bytes**. Build/budget gate, ZIP CRC,
all **1,212** exact audit entries, five-scene catalog/bundle hash and size,
APK v2 signature, native scanner assets and all seven QR/account/profile Java
bridges passed. Filming types are absent from release metadata. APK SHA-256:
**F7CEFF3593F585395D8F56EF192EB80209FC11F78802699C01FA16DCD9C57A52**.

Largest compressed entries: islands **45,253,189** (-250), IL2CPP
**12,698,542**, Unity **9,808,204**, metadata **2,810,547**, and classes.dex
**1,382,331** bytes (last four unchanged). No new art, lossy conversion or
unexpected growth was introduced.

The tested protocol-79 snapshot is the previous validated **606 C# files**
plus filming changes. Concurrent RoomChat work, including changes to
RoomMigration, RoomService, PlayerView and AppRoot, is excluded. This build
does not validate those advancing shared-checkout changes. Capture ran from
a workspace containing spaces and an unrelated working directory. Staged
offline footage and its fixed recording clock do not establish multiplayer
filming, phone FPS/touch performance or native Android sign-in behavior.

Current review: `Builds/TrailerPrototype/Review-V2/Football-Prototype.mp4`.
The original 20-second review remains for comparison. Evidence:
`Builds/TrailerPrototype/Revision2/` (source/native/player manifests,
APK verification, signature and SizeAudit), and
`Builds/TrailerPrototype/Review-V2/` (70 checks and video verification).
Complete delivery locations remain `Builds/WindowsFinal/` and
`Builds/Android/WhatTheFish-release.apk`.

Recovery archives:
`Legacy/20261023-145252-907-trailer-isolate-scope/` (accidental isolated WIP
copy; shared source preserved),
`Legacy/20261023-145847-103-trailer-v2-windows/` (prior complete player,
duplicate test player and rejected camera draft), and
`Legacy/20261023-150243-943-trailer-v2-android/` (prior APK and size audit).

## Football filming prototype — 23 October 2026

The first review milestone is a **20-second, 1920×1080, 30 FPS silent video**:
four staged offline arrivals, native Football gameplay/HUD, Portal Steal and a
physical scored goal. Golf filming and the full multi-sport/Skyrail story remain
deferred. See the [prototype guide and review video](TRAILER-PROTOTYPE.md).

The complete **328-file WindowsFinal** player matched its tested candidate by
SHA-256 before updating its delivery note. The exact player passed **49
assertions**, including cancellation/restoration in cinematic and gameplay
shots. All **600 video frames** decoded with the expected 20-second duration.
Recording ran with default script paths from a workspace containing spaces and
an unrelated working directory. All **606 C# source files** matched the shared
checkout at promotion; native Android sources were separately hash verified.
Existing inbound-clearance, tutorial and passing changes are included.

Official AndroidSubmission completed at **14:35:59 Malaysia time**. Actual APK:
**110,718,422 -> 110,721,046 bytes (+2,624)** versus the immediately preceding
inbound/tutorial release, or **+14,588** from this task's opening
110,706,458-byte APK. Remaining headroom under the strict 1,000,000,000-byte
cap is **889,278,954 bytes**. Build/budget gate, ZIP CRC, all **1,212** exact
audit entries, the five-scene bundle catalog/hash/size, APK v2 signature, native
scanner assets and all seven QR/account/profile Java bridges passed. Filming
types are absent from release metadata. APK SHA-256:
**9F58D01C8F2B4FE344311F053E7939FA96E83730B5CA93314C1BB7C1D4691F5B**.

Largest compressed entries: islands **45,253,439** (+2,624), IL2CPP
**12,698,542** (-5), Unity **9,808,204** (-1), metadata **2,810,547** (-1)
bytes. Growth is concentrated in the regenerated scene bundle; no new art or
lossy asset conversion was introduced. The initial isolated APK lacked native
bridge source dependencies and was rejected, archived and rebuilt before
promotion. It was never delivered as the current release.

Current outputs: `Builds/WindowsFinal/`,
`Builds/Android/WhatTheFish-release.apk`, and
`Builds/TrailerPrototype/Review/Football-Prototype.mp4`.
Evidence in `Builds/TrailerPrototype/`: source/native-source/player manifests,
`Review/results.txt`, `Review/video-verification.json`,
`apk-verification.json`, `apk-signature.txt`, and `SizeAudit/`.
The fixed recording clock is not a real-time performance measurement. Physical
phone touch/FPS, native Android sign-in and actual multiplayer filming remain
unverified by this task.

Recovery archives:
`Legacy/20261023-142814-248-trailer-prototype-windows/` (prior complete player
and draft outputs),
`Legacy/20261023-143314-687-trailer-rejected-android/` (rejected candidate),
and `Legacy/20261023-143654-296-trailer-prototype-android/` (prior release/audit).

## Basketball inbound clearance and tutorial feedback — 23 October 2026

The combined protocol **79** delivery adds an opponent-only inbound radius of
**7.1628 m** (25% of the full court length), preserves teammate access and
predictive passing, and includes the latest tutorial feedback/mobile wording.
See [inbound behavior and verification](BASKETBALL-INBOUND-CLEARANCE.md) and
[the tutorial report](GUIDED-TUTORIALS.md).

Official AndroidSubmission completed at **14:20 Malaysia time**. Actual APK:
**110,706,458 -> 110,718,422 bytes (+11,964)**. Remaining headroom under the
strict 1,000,000,000-byte limit: **889,281,578 bytes**. The build/budget gate,
ZIP CRC, all **1,212** exact audit entries, five-scene catalog/bundle size and
SHA-256, and APK v2 signature passed. APK SHA-256:
**D6C52B9F9DE64BE5D74D8CD02E9B9B5FFAB02551D8276D8B4073901F4CE469F8**.

Largest compressed contributors: islands **45,250,815** (+2,128), IL2CPP
**12,698,547** (+8,928), Unity **9,808,205** (unchanged), and metadata
**2,810,548** (+635) bytes. Growth is small and concentrated in compiled code.
No unexpected large contributor, new art or lossy asset conversion was introduced.

The exact complete **327-file Windows candidate** passed **816 restart
assertions** (403 offline, 405 host, 8 guest), **120 basketball passing
assertions** (69/40/11), and **602 tutorial assertions across 58 lessons**.
Restart tests used the default player path from a folder with spaces and an
unrelated working directory. All payload files matched by SHA-256 when
promoted to `Builds/WindowsFinal/`; only its delivery note was then updated.
Phone touch/performance and real WAN latency remain unverified.

The tested source snapshot contains **606 C# files**. At release audit, 603
still matched; later concurrent edits to `Editor/TrailerBuild.cs`,
`Shared/Core/TrailerDirector.cs` and `Shared/Core/TrailerDirector.Actions.cs`
(relative to `Game/Assets/_Game/`) belong to separate trailer development.
Those files are wholly editor/development-only; trailer types are absent from
the release APK's IL2CPP metadata. The Windows player retains the validated
snapshot, including the earlier trailer tooling, rather than those later edits.
All inbound, passing and tutorial production sources matched.

Current outputs: `Builds/Android/WhatTheFish-release.apk` and
`Builds/WindowsFinal/`. Evidence under `Builds/InboundClearanceQA/`:
`apk-verification.json`, `apk-signature.txt`, `SizeAudit/`,
`source-candidate.json`, `player-final-manifest.json` and
`Portable Review/Builds/BasketballRestartQA/`.
Tutorial evidence is indexed in `Builds/TutorialFeedbackQA/tutorial-results.json`.

Reviewed superseded outputs were archived in
`Legacy/20261023-141310-049-inbound-clearance-candidates/`,
`Legacy/20261023-141543-080-inbound-clearance-android/` and
`Legacy/20261023-141810-027-inbound-clearance-windows/`.
All build output and evidence remain ignored; publishing is left to the user.

## Predictive passing and guided tutorials — 23 October 2026

Combined protocol **79** delivery includes single-tap predictive passing in both
sports, all four guided tutorial courses, and the current Friends/room UI.
All **598 C# files** matched the frozen combined source snapshot after packaging.
See [passing behavior and checks](TEAMMATE-PASSING.md) and
[tutorial scope](GUIDED-TUTORIALS.md).

Official AndroidSubmission completed at **13:23 Malaysia time**. The actual APK
is **110,706,458 bytes**, up **40,376** from the immediately preceding
110,666,082-byte Friends/room delivery, or **63,052** from this task's opening
110,643,406-byte APK. Remaining headroom under the strict 1,000,000,000-byte
cap is **889,293,542 bytes**. Build/budget gate, ZIP CRC, all **1,212** exact
audit entries, five-scene catalog/bundle size and hash, and APK v2 signature
passed. SHA-256:
**CE75B2F297E4EB0019C84E6112AAC0FBB76DF2B80EBABBC5FDE6061DED7480FA**.

Largest compressed entries versus the preceding delivery: islands
**45,248,687** (-1,038), IL2CPP **12,689,619** (+36,488), Unity
**9,808,205** (unchanged), metadata **2,809,913** (+6,135) bytes. Growth is
primarily compiled gameplay/tutorial code and metadata. No unexpected large
contributor, new art or lossy asset conversion was introduced.

The complete **327-file WindowsFinal** player was verified against the tested
candidate before its delivery-note update. The exact combined player passed
**240 passing assertions**, including offline and local host/guest tests in
both sports, from a folder with spaces and an unrelated working directory.
Tutorial checks passed **201 assertions** across all **58 lessons**, including
skip/resume/retry and quit cleanup. Protocol 78 was rejected in both directions
before player spawn. Rendered controls and paths were inspected. Physical
Android touch/FPS, native Android sign-in and real WAN latency remain unverified.

Current outputs: `Builds/Android/WhatTheFish-release.apk` and the complete
`Builds/WindowsFinal/` player. Durable evidence:
`Builds/PredictivePassQA/apk-verification.json`, `apk-signature.txt`,
`SizeAudit/`, `player-final-manifest.json`, `source-candidate.json`, and
`Portable Review/Builds/TargetPassQA/`; tutorial evidence:
`Builds/TutorialQA/Run-20261023-131827-993/` and `Run-20261023-131827-994/`.
The reviewed preceding outputs were archived with hashes and recovery manifests
to `Legacy/20261023-132008-386-predictive-pass-tutorial-android/` and
`Legacy/20261023-132255-097-predictive-pass-tutorial-windows/`.

## Friends and room lobby — 23 October 2026

Task snapshot: `a374059` plus the opening online UI changes and Friends/room
changes. This isolated delivery is **protocol 78**. Concurrent tutorial/passing
work in the shared checkout is excluded; that source has advanced independently
to protocol 79. See [scope and validation](FRIENDS-AND-ROOM-UI.md).

AndroidSubmission completed at **13:12:37 Malaysia time**. Actual APK:
**110,643,406 -> 110,666,082 bytes (+22,676)**; remaining headroom under the
strict 1,000,000,000-byte cap: **889,333,918 bytes**. Build/budget gate, ZIP CRC,
all **1,214** exact audit entries, five-scene catalog/bundle hash and APK v2
signature passed. SHA-256:
**8739A502851446237D2F49C018A19204ACA54718A500D9B8E22E06744863CCF5**.

Largest compressed entries: islands **45,249,725** (+5,344), IL2CPP
**12,653,131** (+13,920), Unity **9,808,205** (unchanged), metadata
**2,803,778** (+1,773) bytes. Two generated performance-test metadata entries
account for another **1,216** compressed bytes. No unexpected large contributor,
new art or lossy asset conversion was introduced.

The complete **327-file WindowsFinal** player was hash verified. The full menu
suite passed **3,959 checks**, the final readiness-feedback host/guest run another
**2,654**, and live profile-service/restart tests **143**. Six-peer capacity,
spectator-host departure and a default-path promotion check passed **21 checks
each**. All **587 C# input hashes** remain unchanged through Android packaging.
Physical Android touch/appearance/FPS and real WAN latency remain unverified.

Evidence: ignored `Builds/LobbyRefreshQA/`, `Builds/StorybookMenuQA/LobbyRefresh-Validated/`
and `Builds/StorybookMenuQA/LobbyRefresh-Feedback/`. Current APK and audit are in
`Builds/Android/` and `Builds/SizeAudit/latest/`. Archive batches and snapshot
limits are recorded in the [task report](FRIENDS-AND-ROOM-UI.md).

## Scene UI redesign with latest golf/fishing merge — 22 October 2026

Source: **`a374059` plus uncommitted scene UI changes**. The requested pull
fast-forwarded `4b92d90` through two commits and restored local work without
conflicts. Shared testing uses **protocol 77**. In-world waiting, travel and
recovery panels now use restrained charcoal surfaces over the actual islands;
loading, readiness, cancellation, failures and leave actions have explicit
states and recovery paths. The pulled golf events/cups, fishing durations and
menu changes are included.

Official AndroidSubmission at **21:26:10 Malaysia time**: actual APK
**106,377,892 -> 110,643,406 bytes (+4,265,514)**, leaving **889,356,594 bytes**
below the strict 1,000,000,000-byte cap. The pre-pull UI APK measured
106,392,492 bytes; it was archived before rebuilding merged source. The official
build gate, ZIP integrity, all **1,212 exact audit entries**, five-scene catalog
hash and APK v2 signature checks passed. SHA-256:
**B770BF1D49948101391430951873F06A6BFFDB952B8570FE445BE0F28AD98AD3**.

Largest compressed entries: islands **45,244,381**, IL2CPP **12,639,211**,
Unity **9,808,205**, metadata **2,802,005** bytes. Their task-opening deltas are
+152,446, +57,620, -252 and +11,864 bytes. The two large new resource entries
are the pulled golf **WarningSheet (2,343,698)** and **BadgeSheet (1,692,260)**
compressed bytes, confirmed by the packed layout. These explain most growth;
no unexpected large contributor or additional lossy conversion was introduced.
The scene UI itself reuses existing art/fonts.

The complete **328-file WindowsFinal player** and its shared ZIP were hash
verified, including extracted files. The ZIP is **437,624,006 bytes**. Final
merged validation passed 287 scene checks, 534 teleport checks, 1,686 menu/lobby
checks, 18 golf cup/event checks, 2,966 fishing rule/gameplay/skill checks and
175 real cloud room checks. The extracted ZIP passed another 287 scene checks.
The final fishing reset fix is covered by the final UI/fishing runs. Earlier
merged suites retain unchanged relevant code. Before pull, 3,928 checks covered
20-person repeated host handovers; those are not claimed as protocol-77 recovery
testing. Physical Android touch/FPS, native sign-in and real WAN latency remain
unverified; startup profile persistence was unavailable and recovery UI was tested.

Evidence: ignored `Builds/OnlineUIQA/`, including `source-final.json` (584 C#
input hashes), `SizeAuditMerged/`, `apk-verification.json`, `apk-signature.txt`,
`player-manifest.json` and `zip-verification.json`. Full player/assets were built
at 21:17 and final scripts at 21:21. The source hashes remained unchanged through
Android packaging. Generator-only golf ball IDs were restored after a semantic
serialized-property comparison. See [scope, verification and archive batches](ONLINE-UI-REDESIGN.md).

## Golf event rotation, random routes and shared awards - 23 October 2026

Based on `a374059` with the reviewed Golf hazards and Golf/Fishing round
changes. Network protocol **79** requires matching builds.

Golf events start every **60 seconds**. Seeded five-event bags cover all five
before repeating and avoid repeats at bag boundaries; an early finish can
end before the five-minute rotation completes. Tornado contact radius is
3.5 metres, with larger visuals and preferred 14-20 metre dry throws. Narrow
fairways use checked shorter throws. Same-size crabs pursue nearby balls,
animate stronger claw hits and launch balls with a per-owner cooldown.
Queued club contact cannot undo a hazard launch.

Hole 1 stays first; the other four holes use one shared host-shuffled route.
The route, progress, event sequence and flight cues survive snapshots. The
cup guide projects the cup itself and handles near/behind-camera targets.
Short screen taps skip shot/hazard flight follow without stopping the ball.
Golf and Fishing reuse court models, poses and UI for skippable team/winner
awards, hiding and restoring live controls/equipment. Shared winners retain
group framing. Fishing skills have longer/stronger effects and useful cooldowns;
School Call attracts up to five free fish closer to the pier. The misleading
pre-hook arrow above Jump is removed. See [Golf rules](GOLF-HAZARDS.md) and
[Fishing skill values](FISHING-ABILITIES.md).

Fresh official AndroidSubmission APK: **111,341,614 -> 111,342,842
bytes (+1,228)**. Remaining strict decimal-GB headroom:
**888,657,158 bytes**. Budget gate, ZIP CRC, five-scene bundle/catalog
integrity and independent APK v2 signature verification passed. Android Debug
signing is retained for local testing. SHA-256: **9737CACA876F91D2D5383B142C434A49D181921B8CE2B7EE62637E62776D1434**.

Largest compressed APK entries: `assets/PackagedContent/islands.bundle` **45,230,789**; `lib/arm64-v8a/libil2cpp.so` **12,646,199**; `lib/arm64-v8a/libunity.so` **9,808,819**; `assets/bin/Data/Managed/Metadata/global-metadata.dat` **2,800,759** bytes.
The fresh size audit and entry growth were reviewed; no unexpected large
asset duplication was found. No additional character model or texture set was
needed for these round changes; prior illustrated hazard masters remain editable.

Validation: **13 asset checks**, **14 focused round-rule checks**,
**38 rendered runtime checks**, **580 current C# hashes**,
**524 art/resource/source hashes** and **326 complete Windows runtime/support
hashes**. Phone input/FPS and live WAN multiplayer remain untested. Broad
unchanged sport suites were not rerun. The temporary local-only SDK listing
workaround was restored byte-for-byte after building.

The complete tested player is in `Builds/WindowsFinal/`. Current evidence is
ignored `Builds/GolfFishingPolish/Task-20261023-172827/`. The archive batch is
recorded there in `delivery-archive.txt`. Git publication follows the user's
explicit request to push to `main`.

## Golf hazards, illustrated weather and simpler Fishing rounds - 23 October 2026

Source: `a374059` plus local changes. Tornado and Crab Crossing join the random
Golf pool. Events now start every **70 seconds**, with a ten-second warning
and 45-second duration. Host-controlled hazards preserve stroke counts, cup
safe areas and existing shot rules. Protocol is **78**; every peer needs the
updated build. The carried-ball lock survives ordinary match updates and
blocks queued club contact.

The line spiral is replaced by a curved shaded condensation body, billowing
vapor, a compact storm cloud and rotating ground dust. Golf's background clouds
are smaller and farther away, with shared travel scenery restored on exit.
Imagegen badges and vapor preserve alpha; three delivery PNGs total **411,125
bytes** before Unity packing. Full masters and exact prompts are retained.
The editable local Blender crab uses 2,168 / 1,056 triangles and four shared
solid materials. See [hazards and asset provenance](GOLF-HAZARDS.md).

Fishing uses the default three-minute Round and one host Start / Play Again
control. Ready, mode selection, Free and nickname entry are removed from the
island leaderboard. Shared skill selection/countdown and the existing fishing
mechanics remain. Results have one Close action.

Fresh official AndroidSubmission: actual APK **110,590,846 ->
111,341,614 bytes (+750,768)**. Remaining strict decimal-GB
headroom: **888,658,386 bytes**. Budget gate, ZIP CRC, five-scene
catalog/bundle hash and independent APK v2 verification passed. Existing
Android Debug signing is retained for local testing. SHA-256:
**C2B4253FB94DC383973FCA391CAB6183D531660774931305B60AA90789126477**.

Largest compressed APK entries: `assets/PackagedContent/islands.bundle` **45,238,326**; `lib/arm64-v8a/libil2cpp.so` **12,638,797**; `lib/arm64-v8a/libunity.so` **9,808,819**; `assets/bin/Data/Managed/Metadata/global-metadata.dat` **2,799,421** bytes.
The fresh size audit was reviewed; generated art is bounded and no unexpected
large asset duplication was found.

Validation: **13 editor preparation checks**, **26 focused rendered runtime
checks**, **578 current C# hashes**, **524 art/resource/source hashes** and
**326 complete Windows player/support/launcher hashes**. Physical-phone input,
FPS and WAN play remain untested. Broad unchanged sport suites were not rerun.
The installed SDK's supported local-package list avoided a stalled remote
metadata fetch; its launcher was restored byte-for-byte afterward.

Current evidence: ignored `Builds/GolfHazards/Task-20261023-135847/`.
The complete tested player is in `Builds/WindowsFinal/`. The superseded complete
player/APK is recoverable from
`Legacy/20261023-153738-660-golf-hazards-fishing-round-delivery/`.
Git publishing remains with the user.

## Golf crossing assistance, clearer events, Fishing skills and menus - 22 October 2026

Source: `03409e7` plus local changes. Ground-level cup crossings now drop in
up to **6 m/s**, with the centre anywhere inside the visible opening and up to
**1.3 ball radii** high. A swept crossing catches entries between fixed steps.
Very hard shots can still roll across; airborne and outside shots are excluded.
Cup geometry stays at the existing 0.19 m radius.

Wind now has 28 brighter, wider, longer streaks, a **72-unit direction compass**
below the badge and a stronger persistent grey-blue edge gust. Sticky/Bounce
have coloured edges, larger outlines and **1.4-second** contact rings. Rush
ribbons and foot puffs are stronger. The centre remains clear; Reduced Motion
suppresses wind streaks and edge tint. No new image, model or shader was added.

Fishing skills last **5s Steel Wire, 6s Power Reel/Buddy, up to 9s Bubble Guard,
12s Sonar and 10s School Call plus 2s release**. Cooldowns remain unchanged.
Descriptions, Sonar recipients, school motion and ripples share those times.
Requested promotional subtitles are removed from Home, Settings, Friends and
sport entry; existing navigation and gameplay modes remain.

Official AndroidSubmission completed at **2026-10-22T19:18:20+08:00**:
actual APK **110,599,710 -> 110,590,846 bytes (-8,864)**;
strict decimal-GB headroom **889,409,154 bytes**. Build/budget gate,
ZIP CRC, five-scene catalog/bundle hash and independent APK v2 signature passed.
Existing Android Debug signing is retained for local testing. SHA-256:
**37D9DFB0C8009EC607E59C0006B6FAC0F23CDF22A54344FFDAF8ADCD97FA4697**.

Largest compressed entries: islands **45,221,620** (-8,308), IL2CPP
**12,616,091** (-462), Unity **9,808,205** (unchanged), metadata **2,796,209**
(-104) bytes. No unexpected large growth or lossy art change was found.

Validation: **32 editor cup/weather/geometry checks**, **1,040 Fishing ability
checks**, **18 rendered checks** including ordinary 4.5 m/s capture, hard-shot
overshoot, lingering wind/contact feedback and clean menu screens. Windows
exited cleanly. **579 C# hashes**, **185 Golf asset hashes** and the complete
**327-file WindowsFinal** promotion match. Protocol/wire schema remain 77;
matching builds are required. Physical-phone touch/FPS and WAN multiplayer
remain untested; unchanged broad runtime suites were not rerun.

Evidence: ignored `Builds/GolfClarity/Task-20261022-184551/` (build logs, focused review/captures, source and
asset hashes, APK/signature/audit data and reviewed import cleanup).
Superseded player/APK: `Legacy/20261022-190652-271-golf-clarity-fishing-menu/`.
See [Golf rules and effects](GOLF-CUP-AND-EVENT-POLISH.md) and
[Fishing skill rules](FISHING-ABILITIES.md). Git publishing is tracked in the task final report.

## Golf easier cups and longer events - 22 October 2026

Source: `03409e7` plus the local Golf changes. Physical cup radius is now
**0.19 m** (0.38 m diameter), about **13% wider again**. Terrain, lining, rim,
floor and scene/prefab metadata agree. Gentle-entry capture increases from
**1.2 to 1.6 m/s**, with a **6.45 mm inside-rim allowance** and entry starting
at a centre height of **0.9 ball radii**. Hard shots still overshoot; outside
balls are not attracted. Existing ranking and final-countdown rules remain.

Wind, Sticky Green and Bounce Party now each last **45 seconds** (previously
20/20/15). The 90-second cycle and ten-second warning stay unchanged. Rush
remains tied to the existing finishing countdown. Wind streaks are clearer,
the arrival sweep lasts 1.2 seconds, a faint 2.5% edge tint persists, and contact
rings last 0.85 seconds. No new image/model or extra effect pool was added.

Official AndroidSubmission at **17:02:06 Malaysia time**:
actual APK **110,589,114 -> 110,599,710 bytes (+10,596)**;
strict decimal-GB headroom **889,400,290 bytes**. Build/budget gate,
ZIP integrity, five-scene catalog/bundle hash and independent APK v2 signature
verification passed. Existing Android Debug signing is retained for local tests.
SHA-256:
**49E138557CC9C3E042C79F1209E190433B9994EC4B6D3203A3715D876BF6CCF7**.

Largest compressed entries: islands **45,229,928** (+12,912), IL2CPP
**12,616,553** (-2,654), Unity **9,808,205** (unchanged), metadata **2,796,313**
(+330) bytes. No unexpected large contributor or lossy art change was found.

Focused validation: **31 editor rule/geometry checks**, **12 rendered gameplay
checks**, including actual off-centre entry, hard-shot overshoot, next-tee
readiness, all three 45-second events and lingering wind/contact feedback.
The Windows player exited cleanly. All **579 C# source hashes** and **185 Golf
asset hashes** match through both builds. Complete WindowsFinal promotion
matched **327 files**. Network wire schema/protocol **77** remain unchanged.
Physical-phone touch/FPS, WAN multiplayer and broad historical suites were not run.

Evidence: ignored `Builds/GolfEase/Task-20261022-154854/`, including
`windows-build.log`, `review.txt`, cup/effect captures, `AndroidAudit/`, actual
APK/signature measurements, continuity hashes and reviewed import cleanup.
Superseded player/APK: `Legacy/20261022-165028-349-golf-easier-cups-longer-events/`.
See [current rules and tuning](GOLF-CUP-AND-EVENT-POLISH.md).
Git staging, commit and push were not performed.

## Golf cup, target guide and event polish - 22 October 2026

Source: `03409e7` plus the local Golf changes. Cups are 18% wider with matching
terrain/lining/floor geometry. Powerful shots can cross the opening; slow valid
entry visibly sinks before scoring. A numbered aim/edge guide replaces the
competing compass while aiming. Existing event art/materials support wind
streaks/edge arrival/compass, mint Sticky Green and orange Bounce Party contacts,
and cyan/gold Rush ribbons with foot puffs. No new bitmap or model was imported.

Official AndroidSubmission at **15:27:43 Malaysia time**:
actual APK **110,557,160 -> 110,589,114 bytes (+31,954)**;
strict decimal-GB headroom **889,410,886 bytes**. Build/budget gate,
APK ZIP integrity, five-scene catalog/bundle SHA-256 and independent APK v2
signature verification passed. The existing Android Debug signer is retained
for local testing. APK SHA-256:
**2D94F4CD51EE0F2C98BB424BC5DDF8DC34AA587C420C2623DE8C47479CA497A8**.

Largest compressed entries: islands **45,217,016** (+16,407), IL2CPP
**12,619,207** (+12,557), Unity **9,808,205** (unchanged), metadata
**2,795,983** (+2,109) bytes. New presentation settings contribute 1,094 bytes.
No unexpected large contributor or lossy art change was found.

Focused verification: **22 editor rule/geometry checks** and **14 rendered
runtime checks**, including real fast guided crossing, slow sink/next-tee
readiness, numbered visible/offscreen guidance, wide landscape layout and actual
Sticky/Bounce contacts. The player exited cleanly. All **579 C# source hashes**
and **185 Golf asset hashes** match across the Windows and Android builds.
Complete WindowsFinal promotion matched **327 files**, including support/data.
Protocol **77** and the existing event wire schema remain unchanged.

Physical-phone touch/FPS, WAN multiplayer, broad historical suites and a separate
Rush foot-puff presentation check were not run. Focused previews do not establish
phone quality. Earlier opt-in fixtures for rim-touch scoring and old compass
layouts describe superseded behaviour; this task uses `-golfCupReview`.

Evidence: ignored `Builds/GolfCupEffects/Task-20261022-142302/`, including
`prepare.log`, `review.txt`, landscape/effect captures, `AndroidAudit/`,
`apk-after.json`, `apksigner.txt`, source/asset continuity and promotion hashes.
Superseded player/APK: `Legacy/20261022-151513-026-golf-cup-event-polish/`.
See [current behaviour and tuning](GOLF-CUP-AND-EVENT-POLISH.md).
Git staging, commit and push were not performed.

## Local current-main event rebuild - 22 October 2026

Rebuilt source `03409e7` for local event testing. This includes Golf's Tropical
Wind, Sticky Green and Bounce Party, plus the first-finisher Island Rush,
profiles and the other pulled updates. No gameplay code or art was edited.

Official AndroidSubmission at **13:33:23 Malaysia time**:
actual local APK **93,903,454 -> 110,557,160 bytes (+16,653,706)**;
strict decimal-GB headroom **889,442,840 bytes**. Build/budget gate,
ZIP integrity, five-scene catalog/bundle hash and independent APK v2 signature
verification passed. SHA-256:
**2E075EABE759919D243D9B6CA90A1A605CC7F326E81960BBFC8EE1B98438E7A4**.

The opening local APK predates the pulled profiles/events. Expected growth
includes ten portraits (previously measured at 11,489,098 compressed bytes) and
warning/event art (**2,343,698 / 1,692,260 bytes**). This is a rebuild of the
latest source, not an asset-only comparison. Largest compressed entries are
islands **45,200,609**, IL2CPP **12,606,650**, Unity **9,808,205** and metadata
**2,793,874** bytes. No unexpected large new contributor or lossy asset change.

Complete WindowsFinal updated; **327 files** matched the candidate before its
note was amended. Minimal verification as requested: full compilation and a
brief Golf launch/clean exit with no detected runtime exception. Detailed event,
network and physical-phone tests were not run. All **572 C# input hashes** match
through both builds. Protocol **77** requires matching room peers.

Evidence: ignored `Builds/EventRebuild/Task-20261022-125316/`, including
`AndroidAudit/`, `apk-after.json`, `apk-entry-growth.json`, `apksigner.txt`,
`launch-summary.txt`, source/promotion hashes and reviewed import cleanup.
Superseded local player/APK and the unused detailed-test draft were archived in
`Legacy/20261022-131328-164-local-event-rebuild/`. Git publishing was not performed.
For manual testing, [start a Golf round](GOLF-RANDOM-EVENTS.md): warning at
80 seconds and first random event at 90 seconds; Swing shows the weather effects.

## Golf event placement and independent match seed - 22 October 2026

Event notices now start 12 HUD units beyond the Settings button's actual right
edge, converting world corners into the Golf HUD page's coordinate space. The
10-second warning, active weather and Rush badges share this offset and clamp
their width to available space. Original badge artwork and live countdown styles
are retained. Each authority-started round uses fresh OS random bytes instead
of truncated wall-clock ticks. The first draw is equally eligible for Wind,
Sticky Green and Bounce Party; normal random repeats across rounds remain
possible. Existing no-consecutive-repeat logic, timing and protocol **77** remain.

Complete Windows candidate built successfully and all **328 files** promoted to
WindowsFinal after the user exited the old game. Official AndroidSubmission at
**00:06:25 Malaysia time**: actual APK **110,533,860 -> 110,542,380 bytes (+8,520)**;
headroom **889,457,620 bytes**. Budget gate, ZIP integrity and independent APK v2
signature passed. SHA-256:
**93DD353FD887C254C7A7E3648910B33724E4B1611A2F304D4DDBFD60D656BDEF**.
Largest compressed entries: islands **45,183,633**, IL2CPP **12,608,582**, Unity
**9,808,208**, metadata **2,793,868**, warning art **2,343,698** and event art
**1,692,260** bytes. Small code/type-data and bundle-compression variation; no
new image or map assets. Pre-build source hashes verified after removing only
reviewed prefab object-ID/provenance regeneration noise; generated packaging
scenes remain ignored output.

Evidence: ignored `Builds/GolfEventPlacementQA/` (builds, hashes, promotion,
`AndroidAudit/`, signature and measured verification). Superseded APK archived
under `Legacy/20261022-000322-019-golf-event-placement-apk/`; full old player under
`Legacy/20261022-000525-829-golf-event-placement-player/`. No new runtime,
multiplayer or physical-device testing was performed; build checks do not
establish those results. No Git staging, commit or push.

## Main pull/merge with local Golf work - 21 October 2026

Fast-forwarded `main` from `ca8dc57` to `4b92d90` (two upstream commits), then
restored all local Golf work. Both sides' size records were retained; code merged
without textual conflicts. Upstream profiles/accounts, island bots/settings,
control layouts and ball-sport updates coexist with the local Golf event, tee,
physics, slider and signage changes. Both independent branches used protocol 76
for different payloads; this combined version uses **77**. All peers must update.
No commit or push was made. The pre-merge local recovery stash is retained.

Complete WindowsFinal player rebuilt successfully at **23:38:25 Malaysia time**.
Fresh official AndroidSubmission finished at **23:43:56 Malaysia time**: actual
APK **98,073,922 -> 110,533,860 bytes (+12,459,938)**; headroom
**889,466,140 bytes** below the decimal 1 GB limit. Official budget gate, ZIP
integrity and independent APK v2 signature passed. SHA-256:
**CB698C44FF5AE03119C68425E2D891B466065E82CAC087857B279338CB970B14**.
Ten newly merged player portraits account for **11,489,098 compressed bytes**
of growth. Largest compressed entries: islands **45,178,431**, IL2CPP
**12,605,465**, Unity **9,808,208**, metadata **2,793,652**, warning art
**2,343,698**, event art **1,692,260** bytes. No unexpected large contributor.

Evidence: ignored `Builds/MergeMain77/`, including `merge-record.json`, source
hashes, full Windows build log, `AndroidAudit/`, APK entry deltas and signature.
Reviewed superseded Windows player and APK archived together under
`Legacy/20261021-233343-731-merge-main77/`. Build-only object-ID/provenance noise
was restored after semantic inspection; map source content was preserved.
No new runtime/multiplayer/device tests were run in this merge task. Earlier
protocol-76 checks are historical and do not validate this merged version.

## Golf entry signage and Moon Golf removal - 21 October 2026

Fresh official AndroidSubmission at **23:22:03 Malaysia time**: actual APK
**98,087,150 -> 98,073,922 bytes (-13,228)**; decimal-GB headroom
**901,926,078 bytes**. Official build/budget gate, ZIP integrity and independent
APK v2 signature passed. SHA-256:
**17DBC0546EAF97B284794EF27E520D4D42B6D39C24DD590F750B88FB35019B4B**.
Largest compressed entries: islands **45,186,373**, IL2CPP **12,409,027**,
Unity **9,809,652**, metadata **2,768,210**, warning art **2,343,698** and event
art **1,692,260** bytes. Expected small code/type-data reduction; no new bitmap
assets. Shared source atlas retains its unused Moon region for the other badges.
Current audit: ignored `Builds/GolfSignEventsQA/AndroidAudit/`.

Golf course world-space signs now remain enabled during teleport. Screen HUDs
and other teleport presentation keep their prior hide/restore behavior. Verified
real two-player preparation-room entry: signs visible during flight, arrival,
settling and normal exploration on **17 Host + 16 Client** checks; actual sign
captures reviewed. The Hole 1 / PAR 3 text, position and design are unchanged.
Moon removed from the enum, pool, gravity modifier, display and F2 shortcut.
Pool is Wind/Sticky/Bounce; their IDs remain 1/3/4 and retired ID 2 cannot trigger
or display. Each peer passed a 1,800-draw pool/no-repeat fixture. Random cycle,
warning and first-finisher Rush timing stay unchanged. Protocol **76** required.

**55 offline + 30 Host + 32 Client** event checks also passed, including actual
shot/preview agreement, remaining badges and replicated event conditions. Complete
WindowsFinal updated after the user exited the previous player; all **328 files**
match the validated candidate. **52 map files** retained opening hashes and
**529 C# hashes** are recorded. Equivalent generated Ball prefab IDs/order and
identity source hash restored after semantic comparison. No Git publishing.
Physical phone appearance/FPS/touch and WAN performance remain unmeasured.

Evidence: ignored `Builds/GolfSignEventsQA/Entry/`, `EventsOffline/`,
`EventNetwork/`, `map-preservation.json` and `verification.json`.
Reviewed superseded outputs are recoverable in:
`Legacy/20261021-231900-547-golf-sign-events-apk/` and
`Legacy/20261021-232224-958-golf-sign-events-player/`.

## Golf event warning HUD correction - 21 October 2026

Fresh official AndroidSubmission at **22:36:08 Malaysia time**: actual APK
**98,078,070 -> 98,087,150 bytes (+9,080)**; decimal-GB headroom
**901,912,850 bytes**. Official build/budget gate, ZIP integrity and independent
APK v2 signature passed. SHA-256:
**476397F5A7F07BFD7E227F83374DD0F2798F0C985F8879174CD235D9062DA4AF**.
Largest compressed entries: islands **45,196,617**, IL2CPP **12,412,122**,
Unity **9,809,652**, metadata **2,768,226**, warning art **2,343,698** and event
art **1,692,260** bytes. Small code/type metadata growth; both PNG assets remain
unchanged. Current audit: ignored `Builds/GolfEventHudFixQA/AndroidAudit/`.

Fixed the Canvas's stale atlas binding when warning UVs switch to active-event
UVs: texture assignment now dirties material and geometry. Removed the 03s
source-counter exception; 10s..01s share the same font, size, gradient and outline.
Badge-type transitions reset cached timer formatting. Protocol remains **75**.
No physics, scoring, scheduling or map changes; **52 map files** retained their
snapshot hashes. Focused UI validation only: **44 checks at 1600x900 + 44 at
960x540**, including every warning second, all four repeated event transitions,
and simultaneous weather/Rush. Actual rendered transition agrees with an explicit
Canvas refresh (mean RGB error below **0.11/255**). Captures reviewed.

Complete WindowsFinal updated; all **328 files** match the validated full-build
candidate. Full export was required after the component's serialized layout
changed; failed scripts-only attempt is archived. **527 C# hashes** retained.
Only generated equivalent Ball prefab local IDs and identity source hash restored
after semantic comparison. Physical phone appearance/FPS/touch and WAN remain
unmeasured. No unrelated gameplay fixtures or Git publishing were performed.

Evidence: ignored `Builds/GolfEventHudFixQA/UI-1600/`, `UI-960/`,
`render-comparison.json`, `hud-review.png`, `map-preservation.json` and
`verification.json`. Recoverable archives:
`Legacy/20261021-223324-362-golf-event-hud-fix-apk/` and
`Legacy/20261021-223416-864-golf-event-hud-fix-player/`.

## Golf five tees, warning badge and Rush trails - 21 October 2026

Fresh official AndroidSubmission at **21:59:55 Malaysia time**: actual APK
**95,696,068 -> 98,078,070 bytes (+2,382,002)**; remaining decimal-GB headroom
**901,921,930 bytes**. Build budget gate, ZIP integrity and independent APK v2
signature passed. SHA-256:
**ECBFAF7855BAFF909CD24A34E92EA24C89156F7D587E70D478A7B036F7154B89**.
The supplied full-resolution warning PNG contributes **2,343,698 compressed bytes**;
expected island-mesh/code growth accounts for the remainder. Largest compressed
entries: islands **45,189,958**, IL2CPP **12,409,729**, Unity **9,809,652**,
metadata **2,768,195**, existing badge sheet **1,692,260** bytes. Audit retained
under ignored `Builds/GolfRushFiveQA/AndroidAudit/`.

T2–T5 each have five equidistant tee pads, assigned centre then inner/outer sides.
Native turf textures, world UVs and five-metre joins are retained. T1 data and
native island masters remain unchanged. Ordinary first landing loses 28%
horizontal speed; Bounce loses 20% vertical and 25% horizontal speed, excluding
Putt. Rush lasts 30 seconds and emits fading behind-runner wind ribbons, gold
wisps, leaves and sparkles while grounded and running. Wind HUD arrow removed.
The original warning art shows live 10..01 seconds at the top left, then the
active event badge; final-clock docking below events is preserved. Protocol **75**.

**81 tee/warning/trail + 61 physics/badge + 32 Host + 35 Client assertions** and
**47 editor surface/UV/support checks** passed. Near/top tee views and actual
runtime UI/trail captures reviewed. Complete WindowsFinal matches all **328**
files of the tested player. Last Windows refresh changes a development probe's
active-row lookup only; the release APK excludes those probes/hotkeys. C# source
manifest has **525** files. Build-generated equivalent Ball prefab identifiers,
identity source hash and temporary graphics/quality noise restored against the
opening snapshot after semantic comparison. No Git publishing performed.

Reviewed superseded outputs/draft evidence are recoverable in:
`Legacy/20261021-215706-737-golf-rush-five-apk/` and
`Legacy/20261021-220639-859-golf-rush-five-player/`.
Current evidence: `Builds/GolfRushFiveQA/ValidatedOffline/`, `PhysicsAndBadges/`,
`FinalNetwork/`, `surface-audit.txt`, `tee-site-diff.json`, `verification.json`.
Physical phone appearance/FPS, touch and WAN performance remain unmeasured.

## Golf event V2 physics and wind world - 21 October 2026

Fresh AndroidSubmission at **19:26:05 Malaysia time**: actual APK
**95,667,940 -> 95,696,068 bytes (+28,128)**; decimal-GB headroom
**904,303,932 bytes**. Official build/budget gate, ZIP integrity and independent
APK v2 signature passed. SHA-256:
**3B4EB14DA9885B2B2F423F9F5EFA73C4FC2B99A20AC5F834BCD9C1C2AC6F2B44**.
Largest compressed entries: islands **45,164,378**, IL2CPP **12,403,355**,
Unity **9,809,652**, metadata **2,767,072**, retained badge atlas **1,692,260**
bytes. Expected small code/shader growth; no new bitmap assets or unrelated
art growth. Source artwork remains unchanged.

Wind acceleration is **2.6 m/s² (4x)**, with identical preview/flight conditions,
downwind leaf/grass bending, root weights, five turning flag cloths and a bounded
pool of airborne sand. Moon gravity is **50%**. Sticky Swing stops at touchdown;
Putt is unchanged. Bounce Swing retains **90% vertical / 92% tangential speed**;
Putt is unchanged. Rush gives **2.10x player / 1.90x cart speed**. Swap is removed
and F5 is unused. The first five-hole finisher stops future random draws, cancels
unstarted warnings, preserves active weather to expiry, then triggers Rush after
one second. Lower control hints hidden; central start countdown and GO retained.
Full rules: [Golf random events](GOLF-RANDOM-EVENTS.md).

**61 offline + 31 Host + 33 Client assertions** passed, including actual landing
agreement with preview, Sticky PhysX interruption, Putt equivalence, live particle
movement, downwind cloth orientation and event scheduling. Actual world/UI captures
reviewed. Complete WindowsFinal updated: **328 files** match the tested candidate;
**521 C# source hashes** and both changed shader hashes recorded. Full Windows
content build plus code-only refinements; no authored map layout edits in this
task. Protocol is now **74**: all peers must update. No phone FPS/touch, physical
keyboard automation or WAN session verification. Evidence: ignored
`Builds/GolfEventV2QA/ReleaseOffline/`, `Network/`, `AndroidAudit/` and
`verification.json`. Equivalent Ball prefab local IDs/order and the generated
character source stamp were compared and restored to their opening values.

Reviewed superseded player/probe draft archived in
`Legacy/20261021-192704-857-golf-event-v2-player/`; previous APK in
`Legacy/20261021-192211-997-golf-event-v2-apk/`. No Git publishing performed.

## Golf reference event badges and T1 seams - 21 October 2026

Fresh AndroidSubmission at **00:12:39 Malaysia time**: actual APK
**93,985,974 -> 95,667,940 bytes (+1,681,966)**; decimal-GB headroom
**904,332,060 bytes**. Build/budget gate, ZIP integrity and APK v2 signature passed.
SHA-256: **2D087D354D2D0875E7AC86537B852F64F72A033A62C11303D6E9B8167BE69E48**.
Largest compressed entries: islands **45,145,618**, IL2CPP **12,397,260**,
Unity **9,807,996**, metadata **2,766,629**, new badge atlas **1,692,260** bytes.
Expected growth is the lossless original-resolution transparent reference atlas;
no unrelated art growth. Original source PNG SHA-256:
**5729C41270DE6C4F5C35BDB4D9E3D20838CD19DBB109D7DE2C3736B0E23FC90C**.

Five original-art event badges dock top left with live host timers. Weather and
Rush stack; the existing finishing clock docks below them, retaining 50x50 digits.
Random Swap keeps a text notice because the supplied sheet has no corresponding
art. T1 entry path duplicates and intersecting seams repaired using existing UVs
and materials. Details: [Golf event badges](GOLF-EVENT-BADGES.md).

**48 offline presentation/event assertions + 38 host + 34 client checks** passed.
Actual UI and before/after near/overhead corridor captures reviewed. Complete
WindowsFinal updated: **328 files** match the validated player; **519 C# hashes**
recorded. Of 190 baseline scene/map/physics resources, **188 are unchanged**;
only the two intended entry paving/border meshes changed. Protocol stays 73.
Windows full content build plus code-only presentation refresh. No phone FPS/touch
or new WAN verification. Evidence: ignored `Builds/GolfEventBadgeQA/`, including
`FinalUI/`, `Network/`, `SeamsBefore/`, `SeamsAfter/`, `AndroidAudit/` and
`verification.json`. Build-only character source stamp, equivalent prefab IDs and
temporary AA setting were compared and restored to their opening values.

Reviewed superseded output archived in:
- `Legacy/20261020-235851-637-golf-badge-draft/`
- `Legacy/20261021-000509-115-golf-badge-ui-draft/`
- `Legacy/20261021-000602-637-golf-badge-apk/`
- `Legacy/20261021-001310-441-golf-badge-gradient-draft/`
- `Legacy/20261021-001310-650-golf-badge-player/`

No Git publishing performed.

## Golf event test hotkeys - 20 October 2026

Development Host/offline hotkeys F1-F5 select events, F6 triggers Rush, F7 clears.
Client authority is rejected. Automatic event cycle resets to 90 seconds after
manual testing. Full rules and launcher: [Golf event hotkeys](GOLF-EVENT-HOTKEYS.md).

Fresh AndroidSubmission at **23:43:50 Malaysia time**: actual APK
**93,996,258 -> 93,985,974 bytes (-10,284)**; headroom **906,014,026 bytes**.
Budget gate, ZIP integrity and APK v2 signature passed. SHA-256:
**D9A5F257FE0D2CD0CD226BCB3814162024FCFC797664FED5C9C23E78918621E1**.
Largest compressed entries: islands **45,156,019**, IL2CPP **12,397,923**,
Unity **9,807,996**, metadata **2,766,184** bytes. No new art or unexpected growth;
release code/metadata unchanged, small bundle serialization variation only.
Development hotkeys are excluded from the release Android build.

**51 offline + 38 host + 34 client assertions** passed. Complete WindowsFinal
updated: **328 files** match the validated candidate; **516 C# source hashes**
recorded; **190 scene/map/physics resource hashes** unchanged. Code-only Windows
refresh reused the previous validated content without serialized layout changes.
Protocol remains **73**. Physical function keys, phone FPS/touch and WAN were not
verified. Evidence: ignored `Builds/GolfEventHotkeyQA/`, including `verification.json`,
`Offline/`, `Network/` and `AndroidAudit/`.

Reviewed old player and APK archived as
`Legacy/20261020-234353-145-golf-hotkeys-player/` and
`Legacy/20261020-234122-030-golf-hotkeys-apk/`. No Git publishing performed.

## Golf random events and Island Rush - 20 October 2026

Fresh AndroidSubmission at **22:59:22 Malaysia time**: actual APK
**93,972,974 -> 93,996,258 bytes (+23,284)**.
Strict decimal-GB headroom: **906,003,742 bytes**. Official build/budget gate,
ZIP integrity and independent APK v2 signature passed. SHA-256:
**1BB39CAD70CAD972954E0BD3804A878A7D2C37254995143BE7D444A00C285445**.

Protocol **73** adds the host-synchronized 90-second event cycle: Tropical Wind,
Moon Golf, Sticky Green, Swing-only Bounce Party and player-only Random Swap.
Island Rush starts one second after the first five-hole finish, lasting 15 seconds.
Shot conditions match the guide, persist through motion and clear at rest/recovery.
Existing ranking, strokes, ownership, map and tee-colour repairs are preserved.
No new bitmap assets. Largest compressed entries: islands **45,166,301**,
IL2CPP **12,397,923**, Unity **9,807,996**, metadata **2,766,184** bytes.
Small growth is confined to updated bundle/code/metadata; no unexpected art growth.

**52 offline + 34 host + 32 client assertions** passed; six actual event UI
captures reviewed, including concurrent Rush/wind. Complete WindowsFinal updated;
**327 files** SHA-256 match the tested candidate. All **514 C# hashes** match;
**190 scene/map/physics asset hashes** remain unchanged. No physical-phone or
new WAN session verification. Evidence: ignored `Builds/GolfEventsQA/`, including
`ReleaseChecks/`, `ReleaseNetwork/` and `ReleaseAndroidAudit/`.
Reviewed old/intermediate deliveries and first UI drafts were archived; see the
[events design, verification and recovery record](GOLF-RANDOM-EVENTS.md).

## Golf tee colour and annotated corridor repair - 20 October 2026

Fresh AndroidSubmission at **19:03:22 Malaysia time**: actual APK
**93,875,514 -> 93,972,974 bytes (+97,460)**.
Strict decimal-GB headroom: **906,027,026 bytes**. Official build/budget
gate, ZIP integrity and independent APK v2 signature passed. SHA-256:
**565FFB4F3E28E6BBF696461BF76FB71E03BE0AE5214372771FA4821F1A5E9E78**.

T2–T5 terraces reuse native terrain maps/lighting and preserve original course
colour layers. Restore original paving and border stones only in the annotated
T1 coastal curve. No new textures or runtime logic. Largest compressed entries:
islands **45,152,911**, IL2CPP **12,389,693**, Unity **9,807,996**, metadata
**2,764,612** bytes. Growth is concentrated in the updated island bundle.

**46 surface assertions + 28 runtime tee assertions** passed; reviewed 11 paired
actual scene renders. Complete WindowsFinal updated, **326 files** SHA-256 match
the tested candidate, including its delivery note. Scene/prefab bytes, tee-site
coordinates and terrace mesh assets are unchanged. All **508 C# hashes** match.
No physical-phone FPS/appearance or new WAN validation. Evidence is ignored under
`Builds/GolfTeeColourQA/`, including `AndroidAudit/`. Reviewed previous deliveries
archived in `Legacy/20261020-190104-358-golf-tee-colour-apk/` and
`Legacy/20261020-190206-181-golf-tee-colour-player/`.
See [surface repair and verification](GOLF-TEE-COLOUR-REPAIR.md).

## Basketball block timing and visible contact — 21 October 2026

Fresh AndroidSubmission at **16:33:40 Malaysia time**: actual APK
**106,372,384 -> 106,377,892 bytes (+5,508)**, leaving **893,622,108 bytes**
below the strict 1,000,000,000-byte cap. The official build gate, ZIP integrity,
all **1,207 exact audit entries**, five-scene catalog/bundle hash and APK v2
signature verification passed. SHA-256:
**3A0BFA6F4198AE3D9EE92D99FE287A83FEAD7592864D131EF6C574792D74826B**.

Guard contests now time a real jump and extended palm against the rising shot.
Ordinary shots release at a **1.26 m** offset and require actual swept contact
before a swat. Layups/dunks use elevated contact windows; low-frame-rate jump
and ball interpolation timing are corrected. Both sports retain double-tap
teammate passing. Protocol **76** requires matching room builds.

Largest compressed entries: islands **45,091,935**, IL2CPP **12,581,591**,
Unity **9,808,457**, metadata **2,790,141** bytes. Respective opening-to-final
deltas are **+5,870**, **−473**, **0** and **+110** bytes. No unexpected large
growth, new art or lossy asset conversion was introduced.

The complete **326-file WindowsFinal player** was hash-verified on promotion.
Its full player/assets were built at **15:50:55**, with final scripts at
**16:26:44**. All **548 C# input-file hashes** were unchanged through packaging;
this includes editor/development-only inputs, not 548 runtime classes.
**909 checks** passed: 130 at 20 fps, 114 at 60 fps, 194 host/10 guest,
252 defense geometry/motion, 39 basketball/40 football direct passing and
130 at 30 fps using default paths from a folder with spaces and an unrelated
working directory. The 60 fps and geometry runs precede a test-fixture isolation
adjustment only; gameplay code matches delivery. A further 62 shot-motion checks
and the charge suite passed on an earlier candidate with unchanged shot code.
Actual contact/deflection frames were inspected. Physical Android touch/FPS
and real WAN latency remain unverified. Existing local changes are preserved.

Evidence: ignored `Builds/BlockVisualQA/`, including `apk-verification.json`,
`apk-signature.txt`, `SizeAuditFinal/`, `source-final.json`, `player-manifest.json`,
`verification.json`, `PortableValidation/` and `block-contact.png`. The current
APK and full Windows player are in their standard delivery folders.
Archives: `Legacy/20261021-162831-015-basketball-block-android/` and
`Legacy/20261021-163558-439-basketball-block-delivery/`. The latter preserves
superseded players, tuning trials and development-only geometry fixtures.
See [basketball defense](BASKETBALL-DEFENSE.md) for behavior and test details.

## Direct teammate passing — 21 October 2026

Fresh AndroidSubmission at **15:15:58 Malaysia time**: actual APK
**106,371,104 -> 106,372,384 bytes (+1,280)**, leaving **893,627,616 bytes**
below the strict 1,000,000,000-byte cap. Build gate, ZIP integrity, all **1,207
exact audit entries**, five-scene catalog/bundle hash and APK v2 signature
verification passed. SHA-256:
**816C9AC93F57AA202148CD60032E428D48C7B461514C8C9F019CC4CE994C24FC**.

Largest compressed entries: islands **45,086,065**, IL2CPP **12,582,064**,
Unity **9,808,457**, metadata **2,790,031** bytes. Relative to the opening APK,
islands changed **−12,837**, IL2CPP **+13,329**, metadata **+677**. All entries
match the retained audit; no unexpected large growth was found. No new art,
lossy conversion or build-setting changes were introduced.

The code adds exact teammate double-tap passing and bounded receiver lead for
both ball sports. Protocol **75** requires matching room builds. Existing
local work is preserved. The full WindowsFinal player was hash-verified on
promotion. Passing and regression suites passed **699 assertions** across
offline and local host/guest runs; physical Android touch/FPS and real WAN
latency remain unverified.

Evidence: `Builds/TargetPassQA/apk-verification.json`, `apk-signature.txt` and
`SizeAuditFinal/` under that same directory. Prior APK archive:
`Legacy/20261021-151145-857-teammate-pass-android/`; Windows/helper archive:
`Legacy/20261021-151303-191-teammate-pass-windows/`.
See [the measured release record](BUILD-SIZE.md) and
[passing verification details](TEAMMATE-PASSING.md).

## Teammate Fishing integration with local features — 21 October 2026

Pulled `4c3b6ce` and integrated shared round skills and Island View with the
existing local profiles, bots, settings and personal layouts. Combined protocol
**74** rejects incompatible protocol-73 builds. Local work remains uncommitted.
Fresh AndroidSubmission at **00:09:55 Malaysia time**: actual APK
**106,347,516 -> 106,371,104 bytes (+23,588)**; remaining headroom
**893,628,896 bytes** below the strict decimal-GB cap. Official build gate,
ZIP integrity, all **1,207 exact audit entries**, five-scene bundle/catalog hash
and independent APK v2 signature verification passed. Development-signed.
SHA-256: **7A91DA70BAC772873F6CCD5B2D24C74D04C16064DBB46955C9E2936852EBA455**.

Largest compressed entries: islands **45,098,902**, IL2CPP **12,568,735**,
Unity **9,808,457**, metadata **2,789,354** bytes. Islands grew **15,249**
bytes; IL2CPP grew **4,345**. No unexpected large entry growth or additional
lossy asset conversion was introduced for this integration.

Complete WindowsFinal: **327 files** SHA-256 matched before amending its note.
All **407 compiled C# documents** match integrated source. Passed **2,960 offline
Fishing, 47 host, 49 client, 72 bot Cup/rematch, 234 layout edit, 3 layout restart
and 54 offline settings assertions**, plus **2,084 Fishing rules**. Reviewed skill/layout captures. The final
settings fixture waits for draft UI closure; Fishing camera changes stay local
to the visit. No new physical Android touch/FPS, account/scanner-device or real
WAN validation. Prior source-feature records below retain their original scope.

Evidence: ignored `Builds/TeammateFishingMergeQA/`, plus referenced test runs.
Archives: `Legacy/20261020-235205-618-teammate-fishing-candidate/`,
`Legacy/20261021-000645-949-teammate-fishing-android/` and
`Legacy/20261021-000738-465-teammate-fishing-windows-retry/`.
The first Windows archive attempt was blocked by the running old player;
its manifest remains in `Legacy/20261021-000714-187-teammate-fishing-windows/`.
See [integration details](TEAMMATE-FISHING-INTEGRATION.md).

## Control layout visual correction — 20 October 2026

Fresh AndroidSubmission at **23:34 Malaysia time**: actual APK
**106,333,260 -> 106,347,516 bytes (+14,256)**, leaving **893,652,484 bytes**
below the strict 1,000,000,000-byte cap. The official build gate, ZIP integrity
and independent APK v2 signature verification passed. Development-signed;
unpublished. SHA-256:
**B05BC792C1F13A76782C816230B64A2DFBF9460CCA44B6994A608E5BD43B8B4A**.

The layout editor now uses actual gameplay graphics, native fonts and original
child placement. Oversized replacement cards and duplicate labels are removed.
A compact toolbar provides a control picker, size slider, Cancel, Reset and
Save. This is a code-only visual correction with no new binary art or lossy
asset changes. Current profile, venue and bot features remain included.

Largest compressed entries: islands **45,083,653**, IL2CPP **12,564,390**,
Unity **9,808,059**, metadata **2,788,981** bytes. Opening-to-final deltas:
IL2CPP **+16,031**, islands **−5,670**, Burst **+3,466**, metadata **+336**.
All **1,205 APK entries** match the retained size audit exactly; no unexplained
large growth was found. These are compressed entry sizes, not source sizes.

The complete **327-file Windows player** was hash-verified on promotion to
`Builds/WindowsFinal/` at 23:34 and reopened. It uses the validated full assets
from 21:58 and final scripts from 23:30. All **405 compiled C# documents**
match current source. The layout suite passed **996 assertions**, including
artwork geometry, typography, input, persistence and restarts, with default
paths in a checkout containing spaces from an unrelated working directory.
The Android package is a separate fresh build of the same task code.
Physical Android touch, cutouts, appearance and performance remain unverified.

Exact APK: `Builds/ControlLayoutVisualQA/Android/WhatTheFish-release.apk`.
Matching audit: `Builds/ControlLayoutVisualQA/SizeAuditFinal/`.
Evidence in `Builds/ControlLayoutVisualQA/` includes `apk-final.json`,
`apk-signature.txt`, `compiled-source.json`, `promoted-player-manifest.json`
and the final suite under `Portable Review/Builds/ControlLayoutQA/`.
Superseded player and iteration runs were archived to
`Legacy/20261020-233355-953-layout-visual-delivery/`; the earlier candidate is
in `Legacy/20261020-232147-645-layout-visual-old-candidate/`.
See [CONTROL-LAYOUT.md](CONTROL-LAYOUT.md) for the editor and validation scope.

## Player profile and friends visual refresh — 20 October 2026

Fresh AndroidSubmission at **22:23 Malaysia time**: actual APK
**94,765,380 -> 106,333,260 bytes (+11,567,880)** from the opening baseline,
leaving **893,666,740 bytes** below the strict 1,000,000,000-byte cap.
The official gate, ZIP integrity, native bridge checks and independent APK v2
signature verification passed. Development-signed; unpublished.
SHA-256: **5725395E6DDAC47C3E08969CD99B1E06EEDF0B8FE297807D16E620590A3CCFCB**.

Restores ten original character illustrations, with byte-identical editable
masters and runtime sources. Android imports use ASTC 4×4 at 1024×1536.
The ten portraits account for **11,489,098 compressed APK bytes**; their Unity
serialized size is **1,572,988 bytes each**, a separate measurement. The profile,
friends, search and peer views now reuse the main menu's navy, lime and cyan
theme. Camera/gallery photos and existing social actions remain available.

Largest compressed entries: islands **45,089,323**, IL2CPP **12,548,359**, Unity
**9,808,059**, metadata **2,788,645** bytes. Compared with the opening baseline,
IL2CPP grew **68,097**, metadata **10,203**, and islands shrank **461** bytes.
The matching size audit was inspected and retained in
`Builds/ProfileStyleQA/SizeAuditFinal/`. No unexplained large growth was found.
This combined build also contains concurrent bot, venue and layout work.

The complete **327-file Windows player** was hash-verified on promotion to
`Builds/WindowsFinal/` at 22:29. Full assets were built at 21:58; final scripts
at 22:16. All **404 compiled C# documents** matched current source at promotion.
Final verification passed **140 live multiplayer/restart assertions**, **36
backend tests** and **14 deployed API smoke assertions**. Captures were inspected
at 1600×900, 1200×900 and 1920×824. Physical Android camera/gallery, appearance,
touch and performance remain unverified.

Exact APK: `Builds/ProfileStyleQA/Android/WhatTheFish-release.apk`.
Evidence: `Builds/ProfileStyleQA/ReleaseNetwork/`, `apk-verification.json`,
`apk-signature.txt`, `compiled-source.json` and `promoted-player-manifest.json`.
Superseded player and reviewed temporary copies were archived to
`Legacy/20261020-222912-992-profile-ui-delivery/`.
See [PLAYER-PROFILES-PLAN.md](PLAYER-PROFILES-PLAN.md) and
[the original artwork record](../ArtSource/PlayerPortraits/README.md).

## Personal control layouts — 20 October 2026

Fresh AndroidSubmission at **22:21 Malaysia time**: actual APK
**94,765,380 -> 106,342,532 bytes (+11,577,152)** from the opening baseline.
Immediately preceding shared APK: **106,341,732 bytes**, a difference of
**+800 bytes**. Remaining headroom: **893,657,468 bytes** below the strict
1,000,000,000-byte cap. The official build gate, ZIP integrity and independent
APK v2 signature verification passed. Development-signed; unpublished.
SHA-256: **50BA89EB359FB3A074CAB5E3AEFA91E95BC42F7137C21DC8B5BC729E33A8083B**.

Adds Settings → Layout to all four sports and a four-sport chooser in main-menu
settings. Players move and resize highlighted controls over a dimmed background,
with Save, Cancel and reset. Preferences are separate per identity and sport on
the device. Main-menu styling and distinct in-game themes reuse existing art.
No binary art, world-model changes or lossy asset changes were introduced.

Largest compressed entries: islands **45,098,588**, IL2CPP **12,548,359**, Unity
**9,808,059**, metadata **2,788,645** bytes. Concurrent player portraits account
for **11,489,098 compressed bytes** of growth from the opening baseline.
Other opening deltas include IL2CPP **+68,097**, metadata **+10,203**, and islands
**+8,804** bytes. The latest audit was inspected and retained at
`Builds/ControlLayoutQA/SizeAuditFinal/`. No unexplained large art growth was
found; the entire combined increase must not be attributed to layout controls.

The final Windows candidate passed **286** layout assertions, including real
HUD input, main-menu previews, per-account storage and process restarts, using
default paths in a checkout with spaces from an unrelated working directory.
The complete shared WindowsFinal delivery has matching code/content, apart from
build/debug identifiers in `boot.config`; another **73** layout assertions passed
on that delivered copy. An earlier feature candidate passed **110** Golf/Fishing
settings assertions. Captures were inspected across three window sizes.
All layout-task source checksums match. Later concurrent `PlayerProfiles.cs`
and `SocialPanel.cs` edits are outside the tested Windows snapshot. Android is
a separate later snapshot. Physical Android touch, cutouts, appearance and
phone performance remain unverified.

Exact retained APK: `Builds/ControlLayoutQA/Android/WhatTheFish-release.apk`.
Evidence: `apk-final.json`, `apk-signature.txt`, `build-android-final.log` and
the Windows evidence under `Builds/ControlLayoutQA/`. See
[CONTROL-LAYOUT.md](CONTROL-LAYOUT.md) for checks, source limits and archive
recovery batches, and [GROUP-B-IMPROVEMENTS.md](GROUP-B-IMPROVEMENTS.md) for
the rubric-based recommendations.

## Fishing and golf AI opponents — 20 October 2026

Fresh AndroidSubmission at **22:15 Malaysia time**: actual APK
**94,765,380 -> 106,341,732 bytes (+11,576,352)**, leaving **893,658,268 bytes**
below the strict 1,000,000,000-byte cap. The official gate, ZIP integrity and
independent APK v2 signature verification passed. Development-signed, unpublished.
SHA-256: **203EF5525104ED03F09657BD97663604E36FD257E4C3C6BB7CD34C680088F6E5**.

Adds 1–4 independent opponents for Fishing Round/Cup and the existing five-hole
Golf match. Bots use normal input validation, movement, equipment and scoring.
No art assets or lossy changes were introduced by this bot work. The ten
concurrent player portraits account for **11,489,098 compressed bytes** of the
combined increase. Other deltas include IL2CPP **+69,593**, metadata **+10,242**,
and islands **+6,471** bytes. The intervening venue APK was 106,342,788 bytes.

Largest compressed entries: islands **45,096,255**, IL2CPP **12,549,855**, Unity
**9,808,059**, metadata **2,788,684** bytes. Reviewed the matching size audit and
retained it in `Builds/IslandBotsQA/SizeAuditFinal/`. The exact APK is
`Builds/IslandBotsQA/Android/WhatTheFish-release.apk`.

The complete Windows player passed **71 fishing + 46 golf** assertions, including
full matches, rematches and cleanup, using default paths from a location with
spaces and an unrelated working directory. The preceding candidate passed
**249 basketball/football** regression assertions with the same bot gameplay.
All 404 compiled C# documents matched when the final candidate was copied;
subsequent concurrent `PlayerProfiles.cs` and `SocialPanel.cs` edits are outside
that Windows snapshot. Physical Android appearance, touch and performance remain
unverified. Details and evidence: [ISLAND-BOTS.md](ISLAND-BOTS.md).

## Direct venue customization — 20 October 2026

Fresh AndroidSubmission at **22:07 Malaysia time**: actual APK
**94,765,380 -> 106,342,788 bytes (+11,577,408)**, leaving **893,657,212 bytes**
below the strict 1,000,000,000-byte cap. Official build gate, ZIP integrity and
independent APK v2 signature verification passed. Development-signed; not
published. SHA-256:
**C56E0981958B5DE3148B99EFF9BE0B5E2CFEC401EB629B836A82E5B77281630D**.

All four venue tabs open their controls directly, beside live world previews.
Golf and Fishing gain venue names, flag/pier accents, three equipment finishes
and welcome-sign styles. Changes save per sport; the host controls the shared
room look. Rack and newly held equipment share the saved finish. No new binary
art was introduced by the venue work; the four-island travel model is unchanged.

Largest compressed entries: islands **45,098,225**, IL2CPP **12,548,962**, Unity
**9,808,059**, metadata **2,788,662** bytes. The ten player portraits added by
concurrent workspace work account for **11,489,098 compressed bytes** of growth,
confirmed by mapping ZIP entries to `packed-layout.csv`. Other deltas include
IL2CPP **+68,700**, metadata **+10,220** and islands **+8,441** bytes. Reviewed
`Builds/SizeAudit/latest/` and retained the matching reports under
`Builds/VenueCustomizationQA/SizeAuditFinal/`; no unexplained large venue-art
growth or lossy asset changes. This combined APK includes concurrent work, so its
entire size increase must not be attributed to venue customization.

The complete 327-file Windows player was hash-verified on promotion to
`Builds/WindowsFinal/` and reopened. Passed **863** offline venue checks,
**202** local host/guest checks, **20** Golf equipment checks and **22** Fishing
rod checks; an additional **863** offline checks passed with default paths in a
checkout containing spaces, launched from an unrelated working directory.
Captures at 1600×900, 1280×800 and 1920×1080 were inspected. Venue C# checksums
match the Windows player. Later concurrent changes in `ControlLayoutReview.cs`,
`PlayerHubStyle.cs` and `GolfBot.cs` were absent from that Windows snapshot.
Android was built later from the shared workspace and is a different snapshot.
Physical Android appearance, touch and phone performance remain unverified.

Evidence and the retained APK: `Builds/VenueCustomizationQA/`, including
`Android/WhatTheFish-release.apk`, `apk-final.json`, `apk-signature.txt` and
`portrait-size-attribution.json`. Superseded player, APK/audit and temporary
portable copy were archived in dated venue batches under `Legacy/`; exact
recovery locations and feature details are in
[VENUE-CUSTOMIZATION.md](VENUE-CUSTOMIZATION.md).


## Player profiles and friends — 20 October 2026

Fresh AndroidSubmission at **18:17:45 Malaysia time**: actual APK
**94,676,500 -> 94,765,380 bytes (+88,880)** from the opening sign-in baseline.
An intervening settings delivery was **94,681,516 bytes**; the final combined
build adds **83,864 bytes** to that delivery. Remaining headroom is
**905,234,620 bytes** below the strict 1,000,000,000-byte cap. Official build gate,
ZIP integrity and independent APK v2 signature verification passed. SHA-256:
**A39F9619808E50619B261D5DE74C231727290441C85E8EF987221C6050CA3051**.
Development-signed; no app publication.

Adds cloud profiles for guests/linked accounts, Android camera/gallery crop,
local cache and pending saves, live name/photo badges, tappable profiles, precise
name/code search and friend request/accept/decline/cancel/remove/block/report.
Protocol **73** requires matching clients. The existing Cloudflare Free Worker
and new Free D1 database serve these features. No paid upgrade or payment method
was added. Includes the newer Golf/Fishing settings work from this workspace.

Largest compressed entries: islands **45,089,784**, IL2CPP **12,480,262**, Unity
**9,808,059**, metadata **2,778,442** bytes. Against the opening baseline, growth
is IL2CPP **68,191**, metadata **9,110**, DEX **5,759**, bundle **3,254** bytes.
Reviewed `Builds/SizeAudit/latest/` and ZIP deltas; no unexplained large art growth
or lossy asset change. Full reports are in `Builds/PlayerProfilesQA/SizeAuditFinal/`.
Native photo/account/scanner bridges survive shrinking. The photo activity and
FileProvider are non-exported, the provider exposes only its capture cache,
and no broad storage/image permission was added. Generated Golf prefab differences
were IDs/order only across 14 objects/components and were restored after comparison.

Complete WindowsFinal: **327 files** SHA-256 matched on promotion. All **387**
compiled C# documents match current source. Full assets were built at 18:02;
subsequent supported script-only builds included the final gameplay photo/tap
integration. Passed **61** real Relay profile/friend/restart checks, **35** backend
tests, **13** deployed service checks, **12** local identity, **36** account UI,
**110** Golf/Fishing settings and **183** Football identity assertions. The profile
suite and backend tests passed from a copied checkout with spaces, launched from
an unrelated directory. UI/gameplay captures were inspected. The older Basketball
identity fixture fails its match-start assumption and the same **31** checks on
both candidate and preceding player; its baseline comparison is retained.
Physical Android camera/gallery, permissions, provider sign-in and phone performance
remain unverified. Two Windows peers on one internet connection establish functional
cloud play, not peak concurrent capacity or cross-network WAN performance.

Evidence: `Builds/PlayerProfilesQA/`, including `PortableValidation/`, plus the
referenced account/identity/settings review folders. Superseded Windows player:
`Legacy/20261020-181749-893-profiles-windows/`. Earlier profile candidates were
archived separately. Historical size-record encoding/blank-line damage was repaired
only after proving every historical nonblank line matched its original content.
See [implementation and capacity](PLAYER-PROFILES-PLAN.md).

## Golf and fishing settings — 20 October 2026

Fresh AndroidSubmission at **18:02:03 Malaysia time**: actual APK
**94,676,500 -> 94,681,516 bytes (+5,016)**, leaving **905,318,484 bytes**
below the strict 1,000,000,000-byte cap. Official budget gate, ZIP integrity and
independent APK v2 signature verification passed. Development-signed; not
published. SHA-256:
**5CE7883C4AFB03A7C5888F8F6B67F23742F223D779327B304C6620E67A829AFA**.

Golf gains the shared courts-style settings gear, saved cameras and four audio
sliders, guide, Back handling and role-aware leave confirmation. Fishing shares
that implementation, sends cancellation through the blocked command reader,
restores prior input blocks, waits before automatic tutorial display and updates
countdown / Cup / Crew leave warnings. Results close settings before taking
input ownership. Room timers continue. No new artwork, lossy asset changes,
travel-model changes or protocol changes.

Largest compressed entries: islands **45,089,793**, IL2CPP **12,411,828**, Unity
**9,808,060**, metadata **2,769,727** bytes. Bundle variation is **+3,263** bytes;
metadata **+395**, IL2CPP **-243**. Reviewed ZIP deltas and
`Builds/SizeAudit/latest/`; no unexpected large art growth. Full reports are in
`Builds/IslandSettingsQA/SizeAuditFinal/`.

Passed **998 core settings assertions** across offline, local and real Unity
cloud relay sessions, including both games' host returns and guest exits and
online fishing Round, Cup and Crew. Final player repeated **110 offline** checks;
**57** additional checks passed using default paths in a checkout with spaces,
launched from an unrelated working directory. Fishing gameplay: **2,750 offline +
46 host + 49 client** assertions. Current Golf drag-power regression: **36**.
Menu, guide, inset and live-match captures were inspected. The older Golf HUD
fixture still expects pre-drag artwork; candidate and unchanged baseline fail the
same assertion. Current drag/settings checks pass.

Complete WindowsFinal promoted; all **327 files** SHA-256 matched before updating
its note. All **381** compiled C# documents match a fixed source snapshot. That
snapshot excludes concurrent PlayerProfiles / SocialPanel work; its identity
source exactly matches the earlier account delivery's compiled checksum and all
15 identity methods match its IL. The original workspace's profile changes are
preserved. This is not validation of that concurrent feature. The concurrent
work subsequently began updating the shared Windows delivery too. The matching
validated player is retained at `Builds/IslandSettingsQA/Windows/`; use it with
this task's retained APK. The newer shared delivery was preserved. Original generated
Golf prefab changes were proved to be IDs/document order only across 14 objects;
its serialization and two build-added desktop resource registrations were
restored without affecting unrelated work.

Local evidence: `Builds/IslandSettingsQA/` and
`Builds/FishingGameplayQA/Run-20261020-175152/`. Superseded Windows player:
`Legacy/20261020-180145-656-island-settings-windows/`; previous APK/audit:
`Legacy/20261020-180343-186-island-settings-android/`. Earlier task candidates were
also archived. Promoted duplicates and verified generated backups are in
`Legacy/20261020-180624-364-island-settings-promoted-copies/`. Physical Android touch/Back, appearance and phone performance are
unverified; no phone was connected. Cloud peers used one internet connection;
different-network WAN conditions remain unverified.
The exact task APK is retained at
`Builds/IslandSettingsQA/Android/WhatTheFish-release.apk` while concurrent profile
work replaces the shared Android output. Readiness can check that explicit APK
with `-RequireApk -ApkPath` without substituting another task's build.
See [settings behavior and validation](ISLAND-SETTINGS.md).

## Huawei and Google sign-in — 20 October 2026

Fresh AndroidSubmission at **16:44:24 Malaysia time**: actual APK
**93,910,630 -> 94,676,500 bytes (+765,870)**, leaving **905,323,500 bytes**
below the strict 1,000,000,000-byte cap. Official build/budget gate, ZIP integrity
and independent APK v2 signature verification passed. SHA-256:
**CBD063BE12B922A2D39F7072620D4E453FAC7DFBDDEEE06AAC4D3AAFD5FCD1C7**.
The APK uses the registered development certificate, not production signing.

Adds Huawei Account Kit, Google Credential Manager and account management/linking
UI while retaining guests and both QR camera/image joining paths. The public
Google client ID and Huawei HTTPS endpoint are present in the manifest; native
account/scanner bridges survive release shrinking. Service credentials remain in
Cloudflare encrypted secrets. No new art, texture reduction or gameplay protocol
change. The app has not been published.

Largest compressed entries: islands **45,086,530**, IL2CPP **12,412,071**, Unity
**9,808,059**, metadata **2,769,332** bytes. Main growth is Android resource table
**+347,904**, DEX **+311,971**, IL2CPP **+22,721** bytes. The island bundle changed
by **-2,243** bytes. Reviewed `Builds/SizeAudit/latest/` and retained the complete
report in `Builds/AccountSignInQA/SizeAuditFinal/`; no unexplained large art growth.
Generated Golf prefab changes were semantically identical across 14 components;
original IDs/document order were restored. One material differed only in line
endings and was restored after verification.

The complete Windows candidate has **327 files**, all SHA-256 matched on promotion
to WindowsFinal before updating its note. All **379** compiled C# source documents
matched current source. Final changes after that Windows build affect the Android
postprocessor/native dependencies and server/docs only. Validation: **36** account
assertions, **338** startup assertions, **12** two-peer name checks, **1,057** offline
menu assertions, **25** backend tests and **7** deployed Worker smoke checks.
Windows captures were inspected. Real Android provider success/linking/recovery,
QR camera/image recognition and phone performance remain unverified.

Evidence: `Builds/AccountSignInQA/`, `Builds/PlayerIdentityQA/Network-20261020-161630-129/`
and `Builds/StorybookMenuQA/AccountSignIn-20261020/`. Reviewed superseded player:
`Legacy/20261020-165230-530-account-signin-windows/`; initial candidate:
`Legacy/20261020-160733-356-account-initial-candidate/`.
The account suite also passed from a copied checkout with spaces, launched from
an unrelated working directory. Its 36 UI assertions and 25 backend tests are in
`Builds/AccountSignInQA/PortabilityReview/`. Promoted duplicates, the temporary
checkout and obsolete diagnostics were archived in
`Legacy/20261020-165507-840-account-signin-promoted-copies/`.
See [sign-in implementation and setup](PLAYER-LOGIN.md) and
[remaining profile/friends work](PLAYER-PROFILES-PLAN.md).

## Saved-image room QR scanning — 20 October 2026

Fresh AndroidSubmission at **15:35:31 Malaysia time**: actual APK
**93,906,462 -> 93,910,630 bytes (+4,168)**, leaving **906,089,370 bytes**
below the strict 1,000,000,000-byte cap. Build budget gate, ZIP integrity and
independent APK v2 signature verification passed. SHA-256:
**CC7B355C003A2F659896D0D088CBB5A31FB95CE5335A749C9AF9227645285F34**.
The APK still uses the registered development certificate, not production signing.

Adds Android Choose QR image using the system document picker and Huawei image
mode, with manual joining preserved and no broad storage permission. Largest
compressed entries: islands **45,088,773**, IL2CPP **12,389,350**, Unity
**9,807,995**, metadata **2,764,748** bytes. DEX grew **1,947** bytes; bundle
variation was **2,404** bytes. No new artwork or quality reduction. The generated
Golf prefab was semantically identical (14 objects/components); only IDs/order
changed, so original serialization was restored. One material had line-ending-only
changes and was restored too.

Complete WindowsFinal: **327 files** matched the tested player before its note
update. **41 QR/live-room assertions**, **1,057 offline menu assertions** and
**20 native policy assertions** passed; SDK API compilation and path portability
checks passed. Join layout inspected. Physical Android picker/native image decode,
camera and phone performance remain unverified. Evidence: `Builds/ScanImageQA/`
and `Builds/StorybookMenuQA/ScanImage-20261020/`. Reviewed old Windows player:
`Legacy/20261020-153542-299-scan-image-windows/`; promoted duplicate/initial compile
output: `Legacy/20261020-153706-101-scan-image-promoted-copies/`.
See [scanner behaviour](HUAWEI-SCAN-KIT.md) and the separate, unimplemented
[profile/friends plan](PLAYER-PROFILES-PLAN.md).

## Local validation of pulled main - 20 October 2026

Fast-forwarded to `ca8dc57` with protocol **72**; Git LFS integrity passed.
Fresh AndroidSubmission at **15:01:21 Malaysia time**: actual APK
**90,579,220 -> 93,906,462 bytes (+3,327,242)**, leaving
**906,093,538 bytes** below the strict decimal-GB cap. Build budget gate,
ZIP integrity and independent APK v2 signature verification passed. SHA-256:
**C7C49A7BB97963B2DC9271DA0A4EBC8507E218663C4C7D5DB22BA743D27854C7**.

Largest compressed entries: islands bundle **45,086,369**, IL2CPP
**12,389,677**, Unity **9,807,995**, metadata **2,764,610** bytes.
The islands bundle accounts for **1,932,405** bytes of growth; the update
includes Golf meshes, UI and textures. Entry deltas and packed-asset inventories
were reviewed; no additional asset-quality reductions were made locally.

Fresh complete Windows player: **327 files** SHA-256 matched the tested
candidate before updating its delivery note. Passed **36 drag/Swing/Putt,
7 host and 9 client tee assertions**; inspected the aiming capture. The initial
network probe lacked its network-mode flag and was corrected before the passing
run. No new physical Android, WAN or phone FPS validation. Generated Golf prefab
changes were verified as local IDs/document ordering only before restoring the
committed serialization; generated material changes were line endings only.
Evidence: ignored `Builds/GolfPullQA/`, including `SizeAuditFinal/`.
Superseded deliveries: `Legacy/20261020-145840-280-golf-pull-delivery/`;
old candidate: `Legacy/20261020-145226-948-golf-pull-candidate/`.

## Fishing skill-card text cleanup and publication - 20 October 2026

Removed the **BEFORE CASTING / WHILE HOOKED** card labels and the shared-choice
subtitle. Skill icons, names, descriptions, selection controls and all gameplay
rules remain. This release includes the approved shared round skills and
player-anchored Island View from the preceding record.

Fresh AndroidSubmission at **23:41:09 Malaysia time**:
actual APK **93,899,262 -> 93,903,454 bytes
(+4,192)**, with **906,096,546 bytes** below the strict
1,000,000,000-byte limit. Build/budget gate, ZIP CRC, five-scene catalog and bundle
hash, and independent APK v2 signature verification passed. SHA-256:
**99B3594EFD68CCDCFD2A3DE5D535C8A8CDAD5A7CA33F4952A01AA52E49B0B0FE**.

Largest compressed entries: islands **45,078,358**, IL2CPP **12,390,337**,
Unity **9,807,453** and metadata **2,765,147** bytes. The bundle increased by
**7,415 bytes** while IL2CPP decreased by **3,066 bytes**; no new art was added.

The complete Windows player has **327 files**, all matched to the tested candidate
before its build note was amended. The code refresh preserved the native scene,
catalog and islands bundle. **2,084 rule checks and 2,962 rendered gameplay checks**
passed; the actual card screenshot confirms the removed text and clear descriptions.
All **507 C# hashes** match through both builds. Protocol remains **73**.
No new artwork was added. Physical-phone touch/FPS and real WAN sessions were
not retested; the preceding record retains broader loopback/audio/travel evidence.

Evidence: ignored `Builds/FishingPublish/Task-20261020-232412/`, including `FinalAndroidAudit/`,
`apk-after.json`, `apk-entry-growth.json`, `apksigner.txt` and `windows-promotion.json`.
Reviewed superseded player/APK: ignored
`Legacy/20261020-233141-488-fishing-skill-card-publish/`.
See [Fishing verification](FISHING-VERIFICATION.md) and [skill rules](FISHING-ABILITIES.md).

## Fishing round skills and Island View - 20 October 2026

Fresh AndroidSubmission at **22:51:18 Malaysia time**: actual local APK
**87,580,516 -> 93,899,262 bytes (+6,318,746)**.
Strict decimal-GB headroom: **906,100,738 bytes**. The official build
and hard-limit gate, ZIP CRC, complete five-scene content catalog and independent
APK v2 signature verification passed. SHA-256:
**478B49AB5B57D85914A434A02D614F0B31E0C7A04139D5A1F9FF8E7D0EC6E084**.

This includes the pulled `ca8dc57` source plus the uncommitted Fishing task:
shared three-card selection, one reusable skill, six skills, a monochrome
render-only helper, contextual cooldown button and manual player-anchored
**Island View**. No new bitmap or character model was added; native vector
icons and a small shader reuse existing art and meshes.

The local before APK predates the incoming Golf/QR work. Largest measured entry
growth includes islands **+1,936,533**, Huawei scanner library **+936,122**,
DEX **+710,914** and scanner models **+1,026,816** bytes. This is not an
asset-only Fishing benchmark. The new APK is **23,748 bytes** larger than the
previous documented protocol-72 measurement below, which is a separate build.
Largest compressed entries are islands **45,070,943**, IL2CPP **12,393,403**,
Unity **9,807,453** and metadata **2,765,303** bytes. Serialized/source sizes
are different measurements.

The complete WindowsFinal player has **327 files**. All files matched the tested
candidate before its note was amended. **5,762 checks** passed; **507 C# source
hashes** match both builds. Protocol **73** requires matching peers. The APK uses
the configured Android Debug certificate; store signing is separate. Physical
phone touch/FPS and real internet sessions remain unverified.
Evidence: ignored `Builds/FishingDraft/Task-20261020-175936/`, including
`FinalAndroidAudit/`, `apk-after.json`, `apksigner.txt` and source/promotion hashes.
See [skill rules](FISHING-ABILITIES.md) and [verification](FISHING-VERIFICATION.md).

## Golf and latest-main protocol-72 merge - 20 October 2026

Fresh AndroidSubmission at **07:21:42 Malaysia time**: actual APK
**90,850,906 -> 93,875,514 bytes (+3,024,608)**.
Strict decimal-GB headroom: **906,124,486 bytes**. Official build/budget
gate, ZIP integrity and independent APK v2 signature passed. SHA-256:
**25A3F72A8CF2764BF78DED87E5319D5127BF61EC19C32DB3F0E4538976FF2C2D**.

Merged local Golf `2bfcc5d` with latest main `99685e2`; combined protocol **72**.
Retained Golf drag/release with 30% idle preview, tees, UI and repaired T1 terrain,
plus incoming Fishing, responsive networking, audio and room QR updates.
Most APK growth comes from the incoming Huawei scan library (**936,122** bytes),
DEX increase (**710,914**), and scanner model assets (**1,026,816**). No new Golf
artwork or unrelated quality reductions. Largest compressed entries: islands
**45,055,447**, IL2CPP **12,389,693**, Unity
**9,807,996**, metadata **2,764,612** bytes.

Complete WindowsFinal updated; **327 files** SHA-256 matched before its note
was amended. **36 drag/Swing/Putt + 7 host + 7 client tee assertions** passed.
All **505 C# source hashes** match the tested build. No new WAN session or
physical-phone touch/FPS/scanner test. Evidence: ignored `Builds/GolfPublishQA/`,
including `FinalAndroidAudit/` and `apk-entry-growth.json`.
Reviewed old deliveries archived in `Legacy/20261020-071812-929-golf-main-publish/`.
See [current integration and publication record](GOLF-MAIN-PUBLISH.md).

## Golf 30% idle preview restoration - 20 October 2026

Fresh AndroidSubmission at **06:48:37 Malaysia time**: actual APK
**90,856,642 -> 90,850,906 bytes (-5,736)**.
Strict decimal-GB headroom: **909,149,094 bytes**. Official build/budget
gate and independent APK v2 signature passed. SHA-256:
**DBDB87FC93219B0626C21F5D6DEA15F35220EC9FF2C4FB00A73BA8563B112D13**.

Restored the default 30% Swing/Putt preview before dragging, including a touch
without movement. Actual drag power and release-to-hit remain unchanged. Existing
artwork, force limits, camera and map are reused. Largest compressed entries:
islands **45,044,192**, IL2CPP **12,340,309**,
Unity **9,773,269**, metadata **2,758,580** bytes.
The small size variation is compiled code/packaging, with no new bitmap assets.

Complete WindowsFinal updated; **326 files** SHA-256 matched before its note
was amended. **36 focused runtime assertions** passed, including default Swing,
default Putt, touch without drag and zero-power tap preview checks. Actual idle
30% screenshot reviewed. No fresh WAN or physical-phone input/FPS verification.
Evidence: ignored `Builds/GolfIdlePreviewQA/`, including `FinalAndroidAudit/`.
Previous complete player/APK archive: `Legacy/20261020-064558-702-golf-idle-preview/`.
See [drag power and idle preview notes](GOLF-DRAG-POWER.md).

## Golf drag-to-release power control - 20 October 2026

Fresh AndroidSubmission at **01:03:37 Malaysia time**: actual APK
**90,502,486 -> 90,856,642 bytes (+354,156)**.
Strict decimal-GB headroom: **909,143,358 bytes**. Official build/budget
gate and independent APK v2 signature passed. SHA-256:
**B93CF6D04E7686DB60FEDDB357964134C8625590D95F6DEA99586A86B3D10075**.

Largest compressed entries: islands **45,052,418**, IL2CPP **12,337,899**, Unity
**9,773,269**, metadata **2,758,497** bytes. Growth is concentrated in the two new
slider textures. Player-only serialized assets **28,024,568 bytes** are a separate
measurement; Rail and Knob account for **756,264 serialized bytes**, not APK bytes.
Editable image masters and references remain outside Unity. Android sprites use
ASTC 4x4; no unrelated asset quality reduction or map generation was performed.

Complete WindowsFinal player rebuilt and promoted; **326 files** SHA-256 matched
before its note was amended. **32 focused runtime assertions** passed; actual
Swing zero/60% and Putt 8% screenshots reviewed. Existing aim/trajectory/landing
marker and shot physics retained. No fresh WAN or physical-phone input/FPS test.
Evidence: ignored `Builds/GolfDragPowerQA/`, including `FinalAndroidAudit/`.
See [drag power implementation and delivery](GOLF-DRAG-POWER.md).

## Golf T1 bunker restoration and seam repair - 20 October 2026

Fresh AndroidSubmission at **00:32:45 Malaysia time**: actual APK
**90,795,846 -> 90,502,486 bytes (-293,360)**.
Strict decimal-GB headroom: **909,497,514 bytes**. Official build/budget
gate and independent APK v2 signature passed. SHA-256:
**48BC2EE82B658B0BB957F4EAB814457E49CE7739F0F0B5BD713765DA3CB68481**.

Largest compressed entries: islands **45,055,476**, IL2CPP **12,338,210**, Unity
**9,773,269**, metadata **2,758,497** bytes. Removed floating T1 floor/path meshes;
joined native terrain and exact surface intersections reuse existing texture maps.
No unrelated visual quality reduction. Source-only annotated PNG references are
outside Unity. Player-only serialized assets remain **27,268,200 bytes**, a
separate measurement from APK bytes.

Complete WindowsFinal player rebuilt, tested and promoted; **326 files** matched
by SHA-256 before its note was amended. **457 editor assertions** and **28 four-player
runtime assertions** passed. Actual top, near and two low entry views reviewed;
visible dressing clearance measured **0.0248-0.0250 m**. Original tee-site records
are unchanged. No new WAN or physical-phone quality/FPS test. Evidence: ignored
`Builds/GolfT1SeamQA/`, including `FinalAndroidAudit/`.
See [localized bunker/seam repair](GOLF-T1-BUNKER-SEAMS.md).

## Golf T1 terrain integration repair - 19 October 2026

Fresh AndroidSubmission at **22:57:57 Malaysia time**: actual APK
**90,295,414 -> 90,795,846 bytes (+500,432)**.
Strict decimal-GB headroom: **909,204,154 bytes**. Official build,
budget gate and independent APK v2 signature passed. SHA-256:
**66FAFBA8DC3D85CF3F326CEF99A3AB1BE754E7F03D229DBCF126295F4BFD9017**.

Largest compressed entries: islands **45,345,669**, IL2CPP **12,341,397**, Unity
**9,773,269**, metadata **2,758,482** bytes. The ~0.50 MB increase is concentrated
in localized ground/dressing meshes and a terrain shader using existing textures.
No new bitmap textures or unrelated quality reductions. Player-only serialized
assets remain **27,268,200 bytes**, a different measurement from the actual APK.

Complete WindowsFinal player rebuilt, tested and promoted; **326 files** matched
by SHA-256 before its delivery note was amended. **399 editor assertions** and
**28 four-player runtime assertions** passed. Original pad records are unchanged
and scene/prefab records agree. Actual desktop top/near views reviewed. No new WAN
or physical-phone quality/FPS test. Evidence: ignored `Builds/GolfT1BlendQA/`,
including `FinalAndroidAudit/`. See [T1 terrain repair](GOLF-T1-TERRAIN-REPAIR.md).

## Golf centre-first multiplayer tees - 19 October 2026

Fresh AndroidSubmission at **20:42:18 Malaysia time**: actual APK
**88,552,098 -> 90,295,414 bytes (+1,743,316)**. Strict decimal-GB headroom:
**909,704,586 bytes**. Official build/budget gate and independent v2 signature passed.
SHA-256: **87BBBEB84D4D3FEDD8C9A57432801A29481020FA21D307336121940535C9DA7D**.

Largest compressed entries: islands **44,845,228**, IL2CPP **12,341,397**, Unity
**9,773,269**, metadata **2,758,482** bytes. Expected growth is concentrated in the
new derived tee ground/scenery meshes and small allocation/snapshot code. Existing
island textures and authoring masters are reused; no quality reduction. Player-only
serialized assets **27,268,200 bytes** are a separate measurement from APK bytes.

Complete Windows player rebuilt and promoted after **399 editor assertions**,
**28 four-player runtime assertions**, and **7 host + 7 client assertions**.
Evidence: ignored `Builds/GolfMultiplayerTeeQA/`, including `AndroidAudit/`.
No physical-phone quality/FPS or WAN test. Protocol **69** requires matching peers.
See [tee layout, allocation and delivery record](GOLF-MULTIPLAYER-TEES.md).

## Golf hidden-before-start and self-only preview - 19 October 2026

Fresh AndroidSubmission at **00:51:28 Malaysia time**: actual APK
**88,550,674 -> 88,552,098 bytes (+1,424)**. Strict decimal-GB headroom:
**911,447,902 bytes**. Build/budget gate and independent v2 signature passed.
SHA-256: **3F5030017C954E89FCC603559CCDC319AF3550024D5484F0BD0A6D378AE5C222**.

Largest compressed entries: islands **43,125,660**, IL2CPP **12,319,897**, Unity
**9,773,269**, metadata **2,756,280** bytes. Tiny code/serialization compression
variation; no new assets or quality reduction. Player-only serialized assets stay
**27,268,112 bytes**, a separate measurement from the actual APK size.

Complete Windows player updated with a code-only script refresh on the prior
validated full player; 326 files hashed before updating its note. **21 focused
presentation checks** and screenshot review passed. Evidence and copied audit:
ignored `Builds/GolfPersonalRankingQA/` and `AndroidAudit/`. No phone FPS/WAN test.
See [personal preview record](GOLF-PERSONAL-RANKING.md).

## Original Golf ranking restored - 19 October 2026

Fresh AndroidSubmission at **00:39:12 Malaysia time**: actual APK
**88,542,330 -> 88,550,674 bytes (+8,344)**. Strict decimal-GB headroom:
**911,449,326 bytes**. Build/budget gate and independent APK v2 signature passed.
SHA-256: **01D4B74BAA73CC9DBB323A0721E50C314D4B34892297549AD9DEE05FBB65E878**.

Reverted the simplified leaderboard designs, including unused portrait delivery
assets. Separate 50-unit countdown, close Aim arrow and tapered trail remain.
Largest compressed entries: islands **43,122,483**, IL2CPP **12,321,500**, Unity
**9,773,269**, metadata **2,756,434** bytes. Small bundle/code serialization and
compression variation; no new textures/models or unrelated quality reduction.
Player-only serialized assets **27,268,112 bytes** are not the compressed APK size.

Full tested Windows player promoted to WindowsFinal; 326 candidate files compared
before amending its note. **39 focused graphical checks** and screenshot review
passed, including synthetic eight-player preview/full scrolling list and retained
visual settings. No remote session or physical-phone FPS tests were performed.
Evidence and audit: ignored `Builds/GolfRankingRollbackQA/` and `AndroidAudit/`.
See [rollback and archives](GOLF-RANKING-ROLLBACK.md).

## Golf compact ranking and trail taper - 18 October 2026

Fresh AndroidSubmission at **23:33:48 Malaysia time**: actual APK
**88,550,326 -> 88,542,330 bytes (-7,996)**. Strict decimal-GB headroom:
**911,457,670 bytes**. Build gate and independent v2 signature verification passed.
SHA-256: **358B883D2286330AA147193AEF336E9619F2FEE9D15E4A51F26B51263BCFAAD8**.

Largest compressed entries: islands bundle **43,115,368**, IL2CPP **12,320,248**,
Unity **9,773,269**, metadata **2,756,450** bytes. No new textures/models or quality
reductions; small bundle/code variation, no unexpected asset growth. Serialized
player assets remain **27,268,112 bytes**, a different measurement from APK bytes.
Complete Windows player delivered after 38 focused presentation checks and
screenshot inspection. Evidence: ignored `Builds/GolfCompactQA/`, audit copied to
`AndroidAudit/`. No phone FPS/WAN testing. See [change record](GOLF-COMPACT-RANKING.md).

## Golf smooth countdown, circular address and pond surface - 18 October 2026

Fresh `AndroidSubmission` at **18 October 2026, 22:54:32 Malaysia time**:
actual APK **88,540,314 -> 88,550,326 bytes (+10,012)**. Strict decimal-GB
headroom: **911,449,674 bytes**. Budget gate and independent APK v2 signature
verification passed. SHA-256: **BDDB599E91780DC6DA2440DAD15B041FEC0DA0D2DB93714EA8813206999F242A**.

Largest compressed entries: islands bundle **43,121,521**, IL2CPP **12,322,109**,
Unity runtime **9,773,269**, metadata **2,756,428** bytes. The small change is
consistent with updated code, flag shader and conservative flag mesh bounds;
no textures/models or unrelated visual quality were reduced. Player-only
serialized assets remain **27,268,112 bytes**, not the compressed APK size.

Focused final Windows checks passed **141 offline / 49 host / 53 client**
assertions, including actual remote orbit and foreign-ball RPC rejection.
Evidence: ignored `Builds/GolfRefineQA/Run-20261018-224954/` and
`Builds/GolfRefineQA/AndroidAudit/`. Complete tested Windows player promoted to
`Builds/WindowsFinal/`; 325 binary/support files SHA-256 compared (delivery note
updated separately). Whole-game phone FPS and WAN play are unverified.
See [targeted change record](GOLF-ADDRESS-AND-COUNTDOWN-20261018.md).

## Golf rollback, pond crossing, rim capture and countdown - 18 October 2026

Fresh `AndroidSubmission` at **18 October 2026, 19:11:03 Malaysia time**:
actual APK **87,606,216 -> 88,540,314 bytes (+934,098)**. Remaining headroom
below the strict **1,000,000,000-byte** cap: **911,459,686 bytes**. Budget gate
and independent APK v2 signature verification passed. SHA-256:
**E2FC748B5AA0F780A37BCA070899C5C6307B7EE8D62DE23C6FA7CF17FACAB254**.

The delivered map removes all 21 moving mechanisms while retaining existing
static obstacles and six ponds. New countdown art uses 1024-pixel fixed plates,
small numeral sprites and ASTC 4x4; high-quality editable masters stay outside
Assets. No unrelated lossy art changes were made. The increase includes these
new Resources assets and code; the islands bundle decreases by 69,331 bytes.

Largest compressed entries: islands bundle **43,116,413**, IL2CPP **12,317,560**,
Unity runtime **9,773,269**, metadata **2,756,079** bytes. Player-only serialized
assets **27,268,112 bytes** are not the compressed APK size. Full entry lists and
bundle inventory are in ignored `Builds/SizeAudit/latest/`, with the task copy in
`Builds/GolfPolishQA/AndroidAudit/`.

Complete Windows player updated in `Builds/WindowsFinal/`; 325 tested binary/
support files were SHA-256 compared during promotion, excluding the updated
delivery note. Final focused offline/host/client checks passed: **85 / 42 / 44**.
Evidence: `Builds/GolfPolishQA/Run-20261018-190816/`. Windows views do not certify
physical Android rendering/FPS or WAN play. See [implementation](GOLF-POLISH.md).

## Golf physical moving mechanisms - 18 October 2026

Fresh `AndroidSubmission` at **18 October 2026, 18:10:08 Malaysia time**: actual local APK
**95,067,465 -> 87,606,216 bytes (-7,461,249)**. Remaining headroom below the
strict **1,000,000,000-byte** cap: **912,393,784 bytes**. Build and budget gate
passed; independent APK v2 signature verification passed. SHA-256:
**F06D7A236E6DC101AE6E1F2F5D6CF831AF69F5342D2C11DBE80C5F04F0C66AD6**.

The opening local APK was an older 17 October artifact, different from the
separately documented 18 October releases below. The decrease includes already
committed packaging/audio/travel changes and is not attributed to mechanisms
alone. Against the previously documented 87,569,184-byte release the total is
37,032 bytes larger; this is a record comparison, not a separate retained APK
entry diff. The exact opening APK was recovered from an existing SHA-256-identical
copy and archived for rollback.

Largest compressed entries: islands bundle **43,185,744**, IL2CPP **12,312,472**,
Unity runtime **9,773,772**, metadata **2,755,161** bytes. Twenty-one physical
mechanisms reuse existing materials/primitive meshes; there are no new textures
or imported models and no lossy changes. See [implementation](GOLF-MOVING-OBSTACLES.md).

Complete Windows player: `Builds/WindowsFinal/` (326 files matched before the
delivery note). Editor checks passed; rendered offline **2,511**, local host
**2,475** and client **2,478** assertions passed. Physical Android and WAN play
remain unverified. Audit/signatures/source-retention/copy hashes and close views:
`Builds/GolfMovingQA/`; current audit: `Builds/SizeAudit/latest/`.

Superseded player: `Legacy/20261018-180723-332-golf-moving-player/`; opening APK:
`Legacy/20261018-181348-956-golf-moving-previous-apk/`. No Git publishing.
## Combined latest-main integration — 20 October 2026

Fresh `AndroidSubmission` at **00:10:48 Malaysia time**:
**90,543,496 → 90,579,220 bytes (+35,724)**. Headroom below the strict
**1,000,000,000-byte** cap: **909,420,780 bytes**. Build, automatic budget gate,
ZIP integrity and independent APK v2 signature verification passed. SHA-256:
**506492C679C637F90CB17B974B64F3F407BA255E2A9BD5379DEEB1C14D7AEE75**.

This combines upstream Fishing commit `43eb9d2` with the local audio, movement,
goal identity, shared-menu and Huawei Scan Kit work. Protocol **71** rejects
older peers. All **4,568 Windows integration assertions** and both tested
protocol-69/71 mismatch directions passed. The complete tested player is in
`Builds/WindowsFinal/`; all 327 candidate files matched during promotion.

Growth is mainly IL2CPP (+30,357 compressed bytes), metadata (+2,581) and the
islands bundle (+2,486). Unity runtime is unchanged, with no unexpected large
asset increase or intentional visual reduction. Serialized player assets are
25,690,212 bytes; the separate bundle remains 150,702,143. These are not APK
measurements. See [integration and limits](LATEST-MAIN-INTEGRATION.md).

Current audit: `Builds/SizeAudit/latest/`; preserved opening/final audit,
entry comparison, signature, source hashes and copy integrity:
`Builds/GitSyncQA/`. Superseded delivery and audit:
`Legacy/20261020-000738-460-git-sync-delivery/`. Physical Android camera,
audio, appearance/FPS and live WAN remain unverified.

## Fishing eye-level camera and natural swimming — 19 October 2026

Fresh `AndroidSubmission` at **14:01:07 Malaysia time**:
**87,581,220 → 87,580,516 bytes (-704)**. Headroom below the strict
**1,000,000,000-byte** cap: **912,419,484 bytes**. Build, automatic budget gate,
APK ZIP integrity and independent v2 signature verification passed. SHA-256:
**8C8D4BEC11FF5F8C10AE7918A713D7F9C86D5E667117D28ED22A90AC5B850A24**.
The opening measurement is the actual prior shared-lagoon APK, rather than the
older travel-release measurement below. Signing uses the configured Android
Debug certificate; store release signing is not established by this check.

Eye-level Fishing stays first person during Left/Right holds and permits manual
looking. Thirty shared fish wander along independent smooth seeded paths. The
extra filled rope-side gauge becomes a curved-to-taut line cue with matching
rope colors and a danger warning. Rules, controls, hooked physics and existing
art are retained. No new bitmap, model, shader, audio or package was added.
Protocol is **70**; see [the release note](FISHING-NATURAL-SWIM.md).

The islands bundle is 4,684 compressed bytes smaller, IL2CPP grows 3,626 bytes
and metadata grows 350 bytes. Both serialized asset inventories are unchanged;
there is no unexpected large asset growth or deliberate visual reduction.
Detailed contributors are in [the APK audit](APK-SIZE-AUDIT.md).

**4,930 checks pass** across rules, rendered gameplay, camera/feedback/results
and local host/guest play. The complete tested Windows player is in
`Builds/WindowsFinal/` (325 runtime/support files plus its build note), with the
native data/catalog/bundle hashes preserved by the guarded script refresh.
Evidence, opening/final APK comparisons, 467 C# source hashes, signature output
and preserved audits are under `Builds/FishingNatural/Task-20261019-052154Z/`;
the current audit is also in `Builds/SizeAudit/latest/`.
The release includes existing uncommitted Fishing work above pulled `main`
`9bb55646`. Physical Android touch/appearance/FPS and WAN remain unverified.

## Huawei Scan Kit and shared room menus — 18 October 2026

Fresh `AndroidSubmission` at **19:49:58 Malaysia time**:
**87,583,188 → 90,543,496 bytes (+2,960,308)**. Headroom below the strict
**1,000,000,000-byte** cap: **909,456,504 bytes**. The full build, budget gate,
APK ZIP integrity and independent v2 signature verification passed. SHA-256:
**36A3FBC0B2DB1CC7440CAAEE0CF53B57AFF13276C08856D99EBEE5B74AF74CF6**.

This combined workspace build includes Huawei Scan Kit 2.15.0.301 and the
parallel shared-room menu refinement. Growth is mainly Huawei's ARM64 scanner,
Java dependencies and three model assets. No visual assets were reduced.
The APK contains the registered app ID, camera permission, non-exported scanner
activity and retained bridge/SDK classes. It uses the current Android Debug
certificate; store release signing remains a separate step.

The complete tested Windows player is in `Builds/WindowsFinal/`: **38 QR checks
and 1,442 menu assertions passed**. A live UGS room's lobby and enlarged QR
decoded correctly with an independent reader. Physical Android camera behavior
and phone performance remain unverified at the user's request.

See [Scan Kit integration and registration](HUAWEI-SCAN-KIT.md) and
[menu validation](MENU-REFINEMENT.md). Current contributor audit:
`Builds/SizeAudit/latest/`; before/after evidence, signatures, screenshots and
copy hashes: `Builds/ScanKitQA/`. The previous complete Windows player is in
`Legacy/20261018-194458-193-scan-kit-windows-delivery/`.

## Quieter sports audio and one-second Lock-on — 18 October 2026

Fresh `AndroidSubmission` at **17:41:05 Malaysia time**:
**87,592,176 → 87,583,188 bytes (−8,988)**. Headroom below the strict
**1,000,000,000-byte** cap: **912,416,812 bytes**. Build, budget gate, ZIP
integrity and independent APK v2 signature verification passed. SHA-256:
**845E5EA5021A247E848D757B90E9A24EDE571470CB14835A6F0AB95E690AA1A6**.

Basketball/Football use softer local-only footsteps and lower impact gain.
Basketball Lock-on now aims for one second, with matching audio and countdown.
No audio or visual assets were reduced or removed. The islands bundle shrinks
9,977 compressed bytes; IL2CPP grows 887 and metadata 99. Unity runtime is
unchanged. Serialized player/bundle totals remain 25,688,616 / 150,702,143
bytes; these are not APK measurements. No unexpected large growth was found.

The complete tested player is in `Builds/WindowsFinal/`; **737 Windows
assertions passed** across offline and multiplayer audio/prop checks. See
[implementation, measurements and limits](QUIETER-SPORTS-AND-LOCK-ON.md).
Current audit: `Builds/SizeAudit/latest/`; preserved before/after audit, ZIP
deltas, signatures, build logs, source and full-player copy hashes:
`Builds/QuietSportsAudioQA/`. Reviewed previous delivery/audit:
`Legacy/20261018-173822-247-quiet-audio-delivery/`. Physical Android audio,
rendering, performance and live WAN remain unverified.

## Football direction and guest response — 18 October 2026

Fresh `AndroidSubmission` at **16:55:46 Malaysia time**:
**87,569,184 → 87,592,176 bytes (+22,992)**. Headroom below the strict
**1,000,000,000-byte** cap: **912,407,824 bytes**. Build, budget gate, ZIP
integrity and independent APK v2 signature verification passed. SHA-256:
**9DE4A00B43E5A9B7A8AD6868F58EAD2C13FD371E8476D3D63A1E8A115A7BB988**.

Protocol 69 adds blue/red defending-end frames, aprons, boards and minimap cues.
Guest input edges send promptly, old input packets are rejected, and owned
Football/Basketball poses are presented with less buffering. Football ball
state publishes at 30 Hz. Authority, scoring and travel remain on the host.
See [the implementation and measured limits](FOOTBALL-DIRECTION-AND-LATENCY.md).

The islands bundle grows 14,266 compressed bytes, IL2CPP 5,923, Unity runtime
806 and metadata 1,606. No new texture set or lossy visual changes were made;
the contributor audit found no unexpected large growth. Serialized player
assets are 25,688,616 bytes; that is not the compressed APK measurement.

The complete validated player is in `Builds/WindowsFinal/`; the shareable ZIP
and SHA-256 sidecar are in `Builds/ResponsivenessQA/`. Windows checks passed
56 movement, 292 ball, 513 net, 245 travel/match-start, 22 goal identity and
156 capacity assertions, plus both protocol-68/69 mismatch directions.
Three-peer A/B tests with 120 ms added RTT reduced median visible movement
onset from 315.88 to 232.74 ms; shot samples did not establish an improvement.
The 12-peer test applies 2% packet loss after joining. A separate loss-during-
admission failure and the un-reproduced screenshot rendering diagnostic remain
documented limitations. Physical Android, live Relay/WAN and phone-host FPS
are unverified.

Current audit: `Builds/SizeAudit/latest/`; preserved audit, comparisons, logs,
captures, source hashes and copy/signature evidence: `Builds/ResponsivenessQA/`.
Reviewed superseded delivery and audit:
`Legacy/20261018-165242-181-football-direction-response/`.

## Shared travel and match-start arrivals — 18 October 2026

Fresh `AndroidSubmission` at **14:44:53 Malaysia time**:
**87,561,004 → 87,569,184 bytes (+8,180)**. Headroom below the strict
**1,000,000,000-byte** cap: **912,430,816 bytes**. Build, budget gate and
independent APK v2 signature verification passed. SHA-256:
**F7EAE76B30E61CDA42EAAE3E8BF9DC185965A2BDAD25D12FFC362E4BBFCE9E02**.

Network protocol 68 blocks cable-car departures throughout all four games,
places spectators in viewing areas on match start, and puts Fishing players on
their piers before publishing the countdown. Blocked arrivals reject the start
without moving the room. The station shows attendance and departure restrictions.
See [teammate testing and release sharing](TEAMMATE-TESTING.md).

The largest increase is IL2CPP code (+9,474 compressed bytes); metadata grows
1,237 bytes and the islands bundle shrinks 2,811 bytes. There are no intentional
visual asset reductions or unexpected large asset increases. This build includes
the existing shared-workspace changes.

The complete validated player is in `Builds/WindowsFinal/`; the same release ZIP
is under `Builds/TravelMatchQA/`. Windows checks passed: 245 travel/match-start,
519 teleport/rematch, 370 spectator, and 22 network rail assertions. Two additional
old/new protocol connection cases correctly rejected the incompatible client.
Current audit: `Builds/SizeAudit/latest/`; logs, before/after entry comparison,
source hashes, screenshots and signature output: `Builds/TravelMatchQA/`.
Physical Android and two-computer/WAN behavior remain unverified. The previously
recorded unrelated cart-suite failures remain documented in
[the audio release baseline](GOLF-FISHING-AUDIO.md).

## Golf and Fishing approved audio — 18 October 2026

Fresh `AndroidSubmission` at **13:20:42 Malaysia time**:
**83,309,709 → 87,561,004 bytes (+4,251,295)**. Headroom below the strict
**1,000,000,000-byte** cap: **912,438,996 bytes**. Build, budget gate and
independent APK v2 signature verification passed. SHA-256:
**A6A60BED27CB01E5AB1A3DF2E34ACCB0D3B182DA083854E2FC7ADF75D02B256F**.

All 115 approved clips are integrated: 81 new short PCM assets and 34 reused
clips. New audio contributes 4,202,996 uncompressed serialized bytes; that is
not a separate APK measurement. New audio resource entries dominate growth.
The islands bundle is **43,155,476 compressed bytes**, down 4,734; no visual
asset reduction was made. Shared-workspace changes are included, so this is
not an isolated audio-only source comparison.

The complete tested player is in `Builds/WindowsFinal/`. Task-relevant Windows
checks passed **970 assertions**; the broader cart suite retains the same 33
distinct failures as the previous player. See [audio integration](GOLF-FISHING-AUDIO.md).
No physical Android device was connected; phone audio, FPS and memory remain
unverified. Current audit: `Builds/SizeAudit/latest/`; before/after, signature,
source and copy-integrity evidence: `Builds/GolfFishingAudioQA/`.

## Sky Rail station and cabin repair — 18 October 2026

Fresh `AndroidSubmission` at **01:40:39 Malaysia time**:
**80,052,983 → 83,309,709 bytes (+3,256,726)**. Remaining headroom below the
strict **1,000,000,000-byte** limit: **916,690,291 bytes**. Full build, automated
budget gate and independent APK v2 signature verification passed. SHA-256:
**FEBF34C89F804123D49EF1576B79751615EAE86B5DAE6B6162109DE973D4D790**.

This compares the actual opening local APK with the new shared-working-tree
build. It also incorporates previously unbuilt Golf/integration changes, so
the increase cannot be attributed solely to Sky Rail. Older 95 MB records
below describe different snapshots. The new build retains station/cabin
quality, with repaired track clearance, modular trolley, interior mounts,
doors and boarding collisions. See [SKY-RAIL-REPAIR.md](SKY-RAIL-REPAIR.md).

Largest compressed entries: islands bundle **43,160,210 bytes**, IL2CPP
**12,287,550**, Unity runtime **9,773,533**, and metadata **2,750,141**.
The bundle increased **756,879 bytes**; newly included Golf assets and UI also
appear in the audit. Current reports are in `Builds/SizeAudit/latest/`, with
before/after copies, signature output and source hashes in `Builds/SkyRailQA/`.
Windows rail checks passed **237 assertions**. No Android device was connected;
phone rendering, sustained performance and memory remain unverified.

## Manual cart camera with retained feet - 17 October 2026

AndroidSubmission attempted; Gradle/JDK failed with java.io.IOException:
Unable to establish loopback connection. New APK size/growth unverified.
Retained pre-merge APK: 95,067,465 bytes; 1 GB headroom 904,932,535 bytes.
Failed log/report in Builds/SizeAudit/latest and task evidence; older CSV
reports there do not validate this source. Check-TaskReady passed; evidence
Builds/GolfManualCartCameraQA/readiness.out. No Git publishing.


User selected merged camera behavior again, while retaining the restored
pre-merge seated feet. PlayerView.cs and GolfCart.cs match c933361 exactly;
GolfCartDriverPose.cs is unchanged from the prior fix. Steering/reverse/coast
move the camera position with the cart but do not rotate the player's chosen
world-space yaw. Manual look and all camera modes remain. Cart entry no
longer forces yaw/pitch. Local-driver guard and upstream shot cameras remain.
This supersedes the camera portion of the previous cart-fix record only.
Evidence: Builds/GolfManualCartCameraQA. Historical opening APK 95,067,465
bytes; 1 GB headroom 904,932,535 bytes. No new assets or vehicle physics.


## Restore cart pose and steering camera after merge - 17 October 2026

AndroidSubmission attempted; Gradle/JDK initialization failed with
java.io.IOException: Unable to establish loopback connection. Retained
pre-merge APK remains 95,067,465 bytes; fresh cart-fix APK size/growth
unverified. Historical headroom under 1 GB: 904,932,535 bytes. Failed
log/report retained in Builds/SizeAudit/latest and task evidence; other
CSV/summary reports there still describe an earlier successful release.
No fresh contributor audit produced.


Upstream replaced the pre-merge seating foot rotations and cart camera
follow behavior. Restore GolfCartDriverPose exactly from 86d48c3. Restore
PlayerView cart heading-delta follow, third-person reverse 180-degree
SmoothDamp transition (.45 s, 180 deg/s), input sampling and lifecycle resets.
GolfCart.Apply aligns local camera heading/pitch at entry again. Camera uses
the displayed cart after replication interpolation, for both local host and
driving client. Other players cannot rotate the local view. Retain merged
menu/networking, Fishing and Golf shot-camera functionality. No assets,
vehicle physics, steering, scoring or shot parameters are changed.
Opening retained APK: 95,067,465 bytes; 1 GB headroom 904,932,535 bytes.
This pre-merge APK does not validate the fix. Evidence: Builds/GolfCartMergeFixQA.


## Golf and upstream integration - 17 October 2026

Merged-source AndroidSubmission attempted; Gradle/JDK initialization failed:
java.io.IOException: Unable to establish loopback connection. Retained APK
remains the pre-merge 95,067,465-byte file; new merged-source APK size and
growth are unverified. Historical headroom under 1 GB is 904,932,535 bytes.
Builds/SizeAudit/latest/build.log and LastBuild.buildreport show this failure;
other CSV/summary entries there still belong to the earlier successful build
and cannot validate merged content. Task failure log: Builds/GolfPublishQA.
No fresh APK contributor audit was produced. Historical largest estimated
entries include mobile timber, character albedo and Golf structure meshes.


Merged local Golf completion (86d48c3) with origin/main (9312948), including
menu/guest profiles, Fishing startup/hold controls, replay/characters and
networking. Both branches' verification and size history are retained.
Human-provided AGENTS 1 GB rule and audit workflow take precedence over the
incoming older policy wording. Local V5 obstacles/water, ball visuals and
feedback are preserved; upstream contact-shading field removal and generated
ball object-ID churn are resolved without discarding local course objects.
Local opening APK: 95,067,465 bytes; 1 GB headroom 904,932,535 bytes.
Incoming pre-merge APK record 78,456,347 bytes also describes different source.
Neither historical APK validates this new merged snapshot. Current build
and publication evidence is under ignored Builds/GolfPublishQA.


## User-recorded Golf GO and walkway audio — 17 October 2026

Fresh AndroidSubmission **2026-10-17 19:34:26 Malaysia time**:
**94,935,333 → 95,067,465 bytes (+132,132)**. Remaining headroom under the
**1,000,000,000-byte** cap: **904,932,535 bytes**. Full build, budget gate and
independent APK v2 signature verification pass. SHA-256:
**8468547CD3D98039C15E728F33C71C23AA3599FCB38507ABB808DAD7ED2C5F8B**.

GO uses the user's first recording, cropped .53-1.46 s, once at the real GO
transition. Walkway Path/Approach/Paving contacts use three individual steps
from the second recording, not the full multi-step sequence. Each planted
foot selects one take at original pitch; other surface clips, foot cadence,
countdown-number beeps, volume controls and gameplay remain unchanged.
Four mono 32 kHz PCM clips total **126,256 source WAV bytes**. Full decoded
48 kHz stereo masters and provenance remain outside Unity Resources in
`ArtSource/Audio/Golf/Recordings/`; the portable deterministic generator is
`Tools/Audio/prepare_golf_recordings.py`. No video or source masters ship.
See [GOLF-FEEDBACK.md](GOLF-FEEDBACK.md) and the source README for crop/processing.

Final complete Windows build (**19:31**) passed **216 rendered HUD/feedback
checks with 34 captures and 17 targeted shot checks**, at
`Builds/GolfHUDQA/Run-20261017-193200/` and
`Builds/GolfShotQA/Run-20261017-193200/`. Confirms GO references the recorded
asset, three short mono steps have clean boundaries and no clipping, actual
planted walkway contacts play recorded takes, and one locomotion cycle still
produces two sounds. Existing five-beep/one-GO, other surfaces, previews and
feedback checks pass. Regeneration from an unrelated working directory produces
identical WAV hashes. Physical speaker/timbre perception, phone output/FPS and
live WAN remain unverified. Existing broad-suite Hole 4 putt-rest issue remains
documented below; this task does not change ball motion or gameplay.

All **322 full Windows player files** matched SHA-256 on promotion to
`Builds/WindowsFinal/`. Previous player archived in
`Legacy/20261017-193332-031-golf-recorded-audio-player/`. Evidence, raw extraction
analysis, hashes, build logs, APK/signature and audit: `Builds/GolfRecordedAudioQA/`.
Pre-existing saved scene/UI/art work remains included as captured, not separately
qualified. No stage, commit or push was performed.

Four audio resource entries contribute 126,592 compressed bytes; IL2CPP grows
3,180 bytes. Other small ZIP/metadata shifts account for the remaining growth.
Largest art estimates remain timber 2,247,414; character albedo 1,992,754;
Golf structures 1,965,725; character mesh 1,588,031 bytes. No unexpected art
increase. WAV/source bytes and ZIP estimates are not measured APK size.

## Golf Putt preview and presentation refinements — 17 October 2026

Fresh AndroidSubmission **2026-10-17 19:13:49 Malaysia time**:
**94,934,277 → 94,935,333 bytes (+1,056)**. Remaining headroom below the
**1,000,000,000-byte** cap: **905,064,667 bytes**. Full build, budget gate and
independent APK v2 signature verification pass. SHA-256:
**00C770C9C04DB8C88BBF3AEF1BFAFE131F7BA6CCB0A9087E534AAE5381DF142C**.

GO now plays one clean continuous tone without breath noise or trill. Full
charge no longer plays a ready cue; pale-red Swing edges reach 30% opacity.
The character power meter has rounded teal trim and a highlight. Roaming's
Walk near your ball to aim plate is hidden; charge/driving hints remain.
Actual path supports select stone footsteps. Wheel marks are darker (.48
opacity); contacts emit 8 larger, more widely scattered flecks, capped at 256
particles per used cart. Marks still fade fully in 3 s. Golf ponds reuse the
Fishing water shader with .28 base opacity, ripple speed 1 and a cooler blue
tint compensating for the green bed. Material generator defaults match.
Putt display ends at the first solid obstacle instead of showing later rebound
or final rest, including default-layer objects; Swing display is unchanged.
Authoritative ball plans/physics, scoring, terrain geometry and driving are
unchanged. Details/settings: [GOLF-FEEDBACK.md](GOLF-FEEDBACK.md).

Complete final Windows candidate (full build **19:11**) passed **211 rendered
HUD/feedback checks with 34 captures and 17 targeted shot checks**, at
`Builds/GolfHUDQA/Run-20261017-191146/` and
`Builds/GolfShotQA/Run-20261017-191146/`. Covers actual welcome-path stone sound,
hidden roaming hint, 30% edge without ready cue, steady 2400 Hz GO signal,
pond shader/opacity, Putt endpoints immediately before both default/scene-layer
obstacles at different powers, and unchanged Swing endpoint. Existing foot
plant, wheel emission/expiry, local camera and contact checks also pass.
Visual review confirmed rounded meter, red edge, stronger wheel marks/flecks,
hidden idle hint and transparent blue pond. No physical speaker/phone/WAN
quality or performance is certified by these desktop checks.

All **322 complete player files** matched SHA-256 on promotion to
`Builds/WindowsFinal/`. Previous player archived to
`Legacy/20261017-191305-118-golf-refine-player/`. Evidence, build logs, source
hashes, player hashes, signed APK and audit: `Builds/GolfRefineQA/`. Failed
intermediate test-fixture lookups are retained as diagnostic evidence and were
corrected before final qualification. Earlier green pond capture was refined
and both final platforms rebuilt; intermediate APK is not final validation.

Compressed IL2CPP grows 839 bytes and metadata 131 bytes; other ZIP entries
shift slightly. Largest art estimates remain timber 2,247,414; character albedo
1,992,754; Golf structures 1,965,725; character mesh 1,588,031 bytes. No new
imported art/audio or unexpected art growth. Estimates are not APK size.
The pre-existing broad shot-suite Hole 4 putt-rest issue remains documented
below; ball motion is unchanged. Pre-existing saved scene/UI/art work remains
included as captured and is not independently qualified by these suites.

## Golf charge meter, planted footsteps and cart effects — 17 October 2026

Fresh AndroidSubmission **2026-10-17 18:12:36 Malaysia time**:
**94,926,465 → 94,934,277 bytes (+7,812)**. Remaining headroom below the
**1,000,000,000-byte** cap: **905,065,723 bytes**. Build gate and independent
APK v2 signature verification pass. SHA-256:
**B651B30566E334A12DD26DD6CF51BA369CE026EC0DED373F71BE7C905AA6002C**.

Changes: pale-red near-full Swing screen edges; character-side vertical power
meter replacing the action-button ring, yellow through 75% then blending to
pure red at full power. Footsteps use actual ankle lift/plant transitions and
observed grounded movement; the real locomotion-cycle fixture produces exactly
two sounds. Opening 5/4/3/2/1 uses one shared beep; GO uses a separate whistle.
Moving supported wheels emit small soil flecks and shallow marks fading fully
over 3 s. Parked carts emit nothing. Effects use bounded pools/shared material;
no map, ball physics, driving, scoring, animation or other-sport changes.

Final Windows candidate passed **203 rendered HUD/feedback checks with 33
captures plus 17 targeted shot checks** at
`Builds/GolfHUDQA/Run-20261017-180937/` and
`Builds/GolfShotQA/Run-20261017-180937/`. Validates real skeletal cadence,
colour thresholds/button-ring removal, five countdown beeps/one GO whistle,
wheel contact generation, parked suppression and complete 3 s mark expiry.
Actual captures show the pale-red edge, relocated meter and cart contact view.
All **322 complete player files** matched SHA-256 when promoted to
`Builds/WindowsFinal/`; full build 18:08 and final script refresh 18:09.
Previous player archived in `Legacy/20261017-181139-736-golf-polish-player/`.
Evidence, source/player hashes, APK, signature and audit: `Builds/GolfPolishQA/`.
Settings and behavior: [GOLF-FEEDBACK.md](GOLF-FEEDBACK.md).

Growth is mainly +6,119 compressed IL2CPP bytes and +1,250 metadata bytes.
Largest art estimates remain timber 2,247,414; character albedo 1,992,754;
Golf structures 1,965,725; character mesh 1,588,031 bytes. No new imported
bitmap/audio assets were added. ZIP estimates are distinct from APK size.
Physical speaker/timing perception, mobile appearance/FPS and live WAN require
manual/device testing. The pre-existing broad shot-suite Hole 4 putt-rest
failure remains documented below; this task does not change ball motion.
Pre-existing saved scene/UI/art work remains included as captured.

## Golf follow camera, footsteps and tree-side aim — 17 October 2026

Fresh AndroidSubmission **2026-10-17 17:44:37 Malaysia time**:
**94,912,749 → 94,926,465 bytes (+13,716)**; headroom under the current
**1,000,000,000-byte** cap: **905,073,535 bytes**. Build budget gate and
independent APK v2 signature verification pass. SHA-256:
**D50825359064600D0D24154D9F3908FE5C5DB6B9213FC1F817AF2C37D2870991**.

Adds a silent 512x288 live ball camera below Island Greens, shown on local
actual contact through flight/rolling, held 3 s after settling and faded over
0.65 s. Hidden views stop rendering. Adds distance-driven grass, sand, shallow
water, wood and stone footsteps, with existing master/SFX controls. Swing edge
glow is wider and reaches 14% opacity; Putt retains no edge overlay. Tree/bunker
Aim fallback searches safe alternate stances while preserving collision,
sight, walkable ground and reach checks. No map, ball physics or scoring edits.
Settings are centralized in `Game/Assets/_Game/Resources/GolfFeedback.asset`.

Final complete Windows candidate passed **196 rendered HUD/feedback checks,
32 captures and 17 targeted ball interaction checks** in
`Builds/GolfHUDQA/Run-20261017-174235/` and
`Builds/GolfShotQA/Run-20261017-174235/`. The camera test reads actual world
pixels from its render texture, not merely its enabled flag. Edge on/off
captures differ at 60 of 63 sampled edge pixels. Initial empty-camera output
was caught during visual review and fixed before promotion. All **322** player
files matched SHA-256 on promotion to `Builds/WindowsFinal/`. Previous player
and an incorrect foreground-app capture were archived in
`Legacy/20261017-174340-729-golf-follow-player/`. Correct running-game capture,
source/player hashes, build logs, signature and APK audit are in
`Builds/GolfFollowQA/`.

APK growth is mainly +13,290 compressed IL2CPP bytes and +1,460 metadata bytes,
partly offset by -1,596 Burst bytes. Largest art estimates remain unchanged:
timber 2,247,414; character albedo 1,992,754; Golf structures 1,965,725;
character mesh 1,588,031 bytes. No imported bitmap/audio assets were added;
five footstep clips are synthesized once. Estimates are not APK measurements.
Physical speaker quality, mobile appearance/FPS and live WAN remain unverified.
The known existing `HOLE_4_Putt_0.80_PUTT_RESTS_AT_GLOW` broad-suite failure
remains documented below; this task does not alter ball motion. Pre-existing
saved scene/UI/art work is included as captured, not independently qualified.

## Golf charge, impact, landing and cup feedback — 17 October 2026

Fresh AndroidSubmission **2026-10-17 17:09:17 Malaysia time**:
**94,305,339 → 94,912,749 bytes (+607,410)**. Remaining headroom below the
current **1,000,000,000-byte** cap: **905,087,251 bytes**. Full Unity build,
budget gate and independent APK v2 signature verification pass. SHA-256:
**85E1E2CCD7492CC8F1BEC020B4B77CCD8090DBAFD4D976F2BFD849615573D84D**.

Adds gold charging ring, rising charge loop, one ready sound per peak, faint
Swing edge glow, tiny charge camera push, actual-contact Swing/Putt sounds and
particles, bounded local strong-Swing kick, actual turf/sand impact sounds and
particles, successful-cup chime/particles/short label, and a distinct final-hole
cue. Putt remains stable. Reduced Motion/central shake toggle are honored.
Cancel, focus loss, pause, round reset and leaving Golf clear transients.
Host-confirmed cosmetic messages reject stale/wrong-round/nonfinite/duplicate
events; live WAN synchronization remains unverified. Ball-follow camera is
explicitly deferred. Physics, course, progression and other sports are unchanged.

The +607,410 bytes are predominantly +579,417 compressed bytes in libunity.so
(Unity particle/audio modules retained), +17,874 in libil2cpp.so, +6,135 metadata
and +2,783 for the small particle shader entry. There are no imported sound or
bitmap assets; eight cues and a charge loop are synthesized once at 24 kHz.
The capped particle pool shares one unlit material. Existing largest art ZIP
estimates remain timber 2,247,414, character albedo 1,992,754, Golf structures
1,965,725, character mesh 1,588,031 and stadium 1,556,008 bytes. These estimates
are distinct from measured APK bytes; no unexpected art growth was observed.

Complete 322-file Windows candidate (full build **16:51:20**, final script
refresh **17:06**) passed **175 rendered HUD/feedback assertions and 29 captures**
in `Builds/GolfHUDQA/Run-20261017-170636/`, plus **17 targeted actual ball
interaction assertions and one cup-success capture** in
`Builds/GolfShotQA/Run-20261017-170636/`. Every candidate file matched by hash
when promoted together to `Builds/WindowsFinal/` before updating build metadata.
Task source hashes match final Windows/Android. Saved pre-existing scene/UI/art
work is included as captured, not independently qualified by these suites.

Known pre-existing broad shot-suite failure:
`HOLE_4_Putt_0.80_PUTT_RESTS_AT_GLOW` reproduces in both the former delivered
player (`Builds/GolfShotQA/Run-20261017-165339/`) and this feedback build
(`Builds/GolfShotQA/Run-20261017-165211/`). It remains unresolved; no ball
physics changes hide it. The old near-cup fixture's .35 charge overshoots under
current speeds; fixture-only .008 charge from .65 m enters the real cup and
scores once. Final-hole cue uses a valid progress-transition fixture. See
[GOLF-FEEDBACK.md](GOLF-FEEDBACK.md) for controls, event architecture and limits.

Current exact APK/audit/signature, source hashes, complete player, reports and
baseline evidence are retained under `Builds/GolfFeedbackQA/` and the run
folders above. Superseded player archive:
`Legacy/20261017-170827-172-golf-feedback-player/` (322 files). No staging,
commit or push performed. Phone appearance/touch/FPS, physical speaker quality
and live WAN are unverified; desktop signal/screenshot tests do not certify them.

## Golf Aim target indicator and hidden ranking — 17 October 2026

Fresh AndroidSubmission **2026-10-17 16:31:38 Malaysia time**:
**94,305,115 → 94,305,339 bytes (+224)**. Headroom below the current
**1,000,000,000-byte** limit: **905,694,661 bytes**. Full build, budget gate
and independent APK v2 signature verification pass. SHA-256:
**5B1D89BA2055689694F17C12F59D07254E8154170A18DC84A896792693AB9A40**.

Aim now uses the existing hole/distance card at 72% uniform scale, 12 units
below the safe-area top. Its large central arrow is hidden. A 42x44 edge arrow
appears only for offscreen/behind-camera targets and avoids the top card and
bottom shot/Cancel/power-hint band. The whole ranking root is inactive during
Aim; no personal rank, completed-hole count or stroke summary remains visible.
Normal card/ranking return on Aim exit. Final countdown notices are preserved.
No map, physics, progression, other sport or audio changes are introduced.

Latest audit leading ZIP estimates remain timber 2,247,414, character albedo
1,992,754, Golf structures 1,965,725, character mesh 1,588,031 and stadium
1,556,008 bytes. No new art, material, font, shader or package is shipped;
there is no unexpected asset growth. Estimates are distinct from APK bytes.

The complete 322-file Windows candidate (full build 16:25:02, followed by a
script refresh for bottom-edge clearance) passes **143 rendered assertions
and 27 captures** in `Builds/GolfHUDQA/Run-20261017-162939/`. Screenshot review
caught a bottom arrow behind the hint in the initial review; final checks
explicitly cover unobstructed edge placement. Final player files matched by
hash when promoted together to `Builds/WindowsFinal/`, before metadata update.
Task source hashes match Windows/Android. Saved pre-existing scene/course/UI
work is included as captured, not independently qualified by this HUD suite.
Phone appearance/touch/FPS and WAN remain unverified.

Audio inspection found no dedicated Golf charge/swing/putt/contact/landing/
cup audio hooks or Golf scene/prefab AudioSource. Missing Golf shot sounds
are an unimplemented feature, not a Windows-only limitation. This task did
not add audio or change mute/volume preferences.

Evidence, complete player, APK, signature, source hashes and audit are in
`Builds/GolfAimIndicatorQA/`. Archive batch:
`Legacy/20261017-163035-698-golf-aim-indicator-player/` (superseded 322-file
Windows player). No staging, commit or push was performed.

## Golf aim focus and revised APK limit — 17 October 2026

The user changed the APK limit to **strictly below 1,000,000,000 bytes
(1 decimal GB)**. The former 100 MB cap and 75 MB working target are retired;
older entries below describe their historical budgets. `AGENTS.md`, the build
size gate, readiness check and editor menu now use the current requirement.

Fresh AndroidSubmission **2026-10-17 16:09:49 Malaysia time**:
**94,303,923 → 94,305,115 bytes (+1,192)**. Current-limit headroom:
**905,694,885 bytes**. Full Unity build, budget gate and independent APK v2
signature verification pass. SHA-256:
**6FD607A76E15FC820DFFABCA01FF6825A74CF14D2C35F1E081F97A25E1B0E28D**.

Aim fades auxiliary HUD over 0.25 seconds and immediately blocks its touch
targets. Hole/distance, trajectory, shot/Cancel, charge hint and final notices
remain. Cancel or shot exit restores the HUD. Both Swing and Putt are covered;
reduced motion is honored. The editable parameter is `aimFocusFadeSeconds` in
`Game/Assets/_Game/Resources/GolfUILayout.asset`. No new art/audio/package,
map edits or physics changes are introduced by this task.

The latest packing audit has no unexpected growth: leading estimated ZIP
contributors remain timber 2,247,414, character albedo 1,992,754, Golf structures
1,965,725, character mesh 1,588,031 and stadium 1,556,008 bytes. These are
estimates, separate from measured APK bytes. Serialized assets total
161,897,902 bytes; that is not APK size. No size optimization was imposed.

Complete 322-file Windows build at **16:00:13 Malaysia time** was promoted to
`Builds/WindowsFinal/` after **137 rendered assertions and 25 captures** passed
in `Builds/GolfHUDQA/Run-20261017-160034/`. Promoted files matched the validated
player before updating `LATEST-BUILD.txt`. Task source hashes match across
Windows and Android; the editor-only menu label changed after the Windows
build; a trailing blank line in that editor-only file was removed after Android
validation. Saved pre-existing scene/course/UI work is included as captured, not
independently qualified by this HUD suite. Phone appearance/touch/FPS and WAN
remain unverified.

Evidence, complete validated player, exact APK, signatures, source hashes,
first-attempt diagnostics and audit are retained in `Builds/GolfFocusQA/`.
Android initially failed Java loopback initialization. A stale invalid socket
directory in the generated Gradle cache was removed; the successful build used
a task-local long-path TEMP/TMP directory. No SDK/system settings or portable
source defaults were changed for that workaround.

Archives: `Legacy/20261017-160150-210-golf-aim-focus-player/` (superseded player)
and `Legacy/20261017-160603-010-golf-aim-focus-helpers/` (three finished helpers).
No staging, commit or push was performed.

## Leaderboard contours, status badge and whole-panel hover — 17 October 2026

Fresh AndroidSubmission **2026-10-17 15:20:37 Malaysia time**:
**94,312,363 → 94,303,923 bytes (-8,440)**. Strict-limit headroom
**5,696,077 bytes**. The 75,000,000-byte development target remains exceeded by
**19,303,923 bytes**; reserve remains limited. Budget gate and independent
APK v2 signature pass. SHA-256:
**404FCFD157884EEBB91CD6DBDAB8E4866A10D29A99830DD833A099A81412A20F**.

Only the existing Title/Columns PNGs change. Their estimated ZIP changes are
**−12,928 / +4,315 bytes** (net **−8,613**), while the layout asset estimate is
**+13 bytes**. All other sprite/packed asset contributions are unchanged;
existing leading estimates remain timber 2.247 MB, character albedo 1.993 MB,
Golf structures 1.966 MB, character mesh 1.588 MB and stadium 1.556 MB. No new
texture, master, material, shader, font or package is shipped. Delivery remains
ASTC 4×4/no mipmaps. Exact APK bytes are separate from contribution estimates.

The column band's opaque outside corners are removed without deleting its
gold pill or original labels. Title silhouette cleanup removes duplicate body
walls/cream backing; the expanded title is 90% of panel width, content inset
20 units and header join 0.8. A single body frame now remains visible beneath
the integrated badge. Compact/full Playing and Finished use the same complete
proportional source badges; no status nine-slicing. DNF retains live lettering.
Hover animates the **entire expanded board by 1.8%**, including title, frame,
rows, text and scrollbar; child-button scale feedback is disabled. Exit restores
the whole panel, and reduced motion is honored. The previous taller preview,
first-three-plus-local selection, scrolling, aim summary, game rules and timers
remain. Map, terrain, physics and other sports are unchanged.

Complete **322-file Windows player**, built **2026-10-17 15:16:55**, is promoted to
`Builds/WindowsFinal/` and matches the retained release by file hashes.
**131 rendered assertions and 24 captures** pass in
`Builds/GolfHUDQA/Run-20261017-151656/`. This includes compact status source
sprite/aspect, no independent child feedback, shared title/frame/row growth,
hover exit, reduced motion, safe bounds, existing scrolling/touch/backdrop,
start/aim/shot/finish/DNF and multi-roster checks. Screenshot inspection covers
compact blue corners and expanded header/body joining. Phone appearance,
touch/FPS and WAN remain unverified; the 25-entry fixture is presentation only.

All 14 sprites reproduce byte-identically from a path with spaces and unrelated
working directory. Column corners and title lower-wall corners have zero alpha.
The original master and other 12 sprites remain unchanged. Final source/art
hashes match across Windows/Android. Exact APK/audit, complete release, logs,
hashes and checks remain under `Builds/GolfLeaderboardPolishQA/`; pre-existing
scene/course/UI work is included as captured, not separately qualified here.

Reviewed archives: old player
`Legacy/20261017-151842-200-golf-leaderboard-polish-player/`
(322 files, 604.42 MB); redundant candidate, portable fixture and pre-edit
Title/Columns/audit snapshots
`Legacy/20261017-151852-851-golf-leaderboard-polish-candidates/`
(343 files, 607.42 MB). Active asset GUIDs are retained. No staging, commit
or push was performed.

## Taller proportional leaderboard preview — 17 October 2026

Fresh AndroidSubmission **2026-10-17 14:37:24 Malaysia time**:
**94,308,867 → 94,312,363 bytes (+3,496)**. Strict-limit headroom
**5,687,637 bytes**. The 75,000,000-byte working target remains exceeded by
**19,312,363 bytes**; reserve is limited. Budget gate and independent
APK v2 signature pass. SHA-256:
**3270E7A64F4EBFBFB3034D370E510AA0B893071B64AD4FD3F851AFDFCEA816C3**.

No image, font, shader, material, package or map asset is added or modified.
All leaderboard sprite contribution estimates remain unchanged; only layout
asset ZIP estimate changes by 3 bytes, alongside the small compiled-code APK
delta. Existing largest estimates remain timber 2.247 MB, character albedo
1.993 MB, Golf structures 1.966 MB, character mesh 1.588 MB and stadium
1.556 MB. Exact APK bytes remain distinct from contribution estimates.

The compact column band now retains the source sprite aspect ratio instead
of a fixed 30-unit height. Preview row height **40 → 64**, title width
**230 → 320**, toggle height **32 → 36**, and preview lettering is larger.
A crowded card scales uniformly as a whole to its **44% safe-height ceiling**;
rows and decorative art are no longer squeezed separately. One-player cards
remain at full scale, about 279 reference units tall. The existing first-three
plus-local selection, full scrolling ranking, native pointer behavior and aim
summary remain. Shorter layouts move Jump to the existing left-side utility
placement when needed to avoid the taller card. Other sports, ranking rules,
progress, match timers, scene layout and gameplay physics are unchanged.

Complete **322-file Windows player** matches the retained release and is
promoted to `Builds/WindowsFinal/`. Full asset build **2026-10-17 14:33:18**, final
script refresh **2026-10-17 14:34:27**. **126 rendered assertions and 23 captures**
pass in `Builds/GolfHUDQA/Run-20261017-143427/`: one-player row height,
source-proportional column art, uniform crowded-card scale, bounded rendered
height, four-player wide layout/control separation, three landscape sizes
with safe insets, existing expand/scroll/dismiss/aim/start/shot/finish/DNF
behavior, and the 25-entry presentation roster. Phone UI/touch/FPS and WAN
remain unverified. The latter fixture is not network-player qualification.

Final source and existing sprite hashes are unchanged across the Windows and
Android builds. Exact APK, audit, complete release player, hashes and logs
remain in `Builds/GolfLeaderboardHeightQA/`. Saved pre-existing scene/course/UI
work is included as captured and is not separately qualified by this HUD
review. Reviewed archives: prior player
`Legacy/20261017-143531-325-golf-leaderboard-height-player/`
(322 files, 604.41 MB), redundant tested candidate
`Legacy/20261017-143546-186-golf-leaderboard-height-candidate/`
(322 files, 604.42 MB). No staging, commit or push was performed.

## Supplied leaderboard and bounded ranking — 17 October 2026

Fresh AndroidSubmission **2026-10-17 14:16:41 Malaysia time** measures
**93,928,409 → 94,308,867 bytes (+380,458)**. Headroom below the strict
100,000,000-byte limit: **5,691,133 bytes**. The 75,000,000-byte development
target remains exceeded by **19,308,867 bytes**; reserve remains limited.
Budget gate and independent APK v2 signature pass. SHA-256:
**2C0561EF22CB974E91770692601BF82906ACAE7902CD5E10E76294E0944E4FFE**.

The 14 cropped leaderboard sprites contribute **780,672 serialized bytes**,
estimated ZIP **466,045 bytes**. Five superseded sprites/metas were proven
unused by GUID and runtime/generator searches, then archived; their previous
estimated ZIP cost was **90,096 bytes**. Net sprite estimate **375,949 bytes**
is separate from the measured APK delta. Full-resolution source stays outside
Unity; ASTC 4×4/no-mipmap delivery, shared UI material and existing fonts are
used. No new package, shader or material is introduced. Largest contributors
remain timber 2.247 MB, character albedo 1.993 MB, Golf structures 1.966 MB,
character mesh 1.588 MB and stadium 1.556 MB; unrelated packed contributions
remain unchanged in the before/after audit.

The crown/title, border, headings, portraits and status artwork derive from
the supplied original. Actual scores remain live. Default shows the existing
first three results plus the local player once; height is bounded to 34% of
the safe canvas. Title/View all opens a 78%-height maximum scrollable list
with a backdrop, scrollbar, mouse wheel and touch drag. Closing restores the
preview; aim closes any open list and shows only the local summary. The row
pool has no ten-player presentation cutoff. Scoring, ties, DNF ordering,
countdowns, hole progress, scene/world structure and other sports are unchanged.

Complete **322-file Windows player** is promoted to `Builds/WindowsFinal/`,
matching `Builds/GolfLeaderboardQA/Release/Windows/` by file hashes. Full asset
build **2026-10-17 14:11:59**, final script refresh **2026-10-17 14:13:32**; the complete
player was then validated with **113 rendered assertions and 22 screenshots**
in `Builds/GolfHUDQA/Run-20261017-141332/`. Tests include a real start/aim/shot,
three landscape resolutions with safe insets, top-three/local de-duplication,
fixed heights, actual expand/backdrop raycasts, native wheel/touch scrolling,
last-player access, green finished badges, unranked DNF and a synthetic
**25-entry presentation roster**. The latter does not qualify 25 networked
world players. Phone appearance/touch/FPS and WAN remain unverified.

All 14 final sprites reproduce byte-identically from a path with spaces and
an unrelated working directory. The retained master matches the supplied file
exactly. Final UI/asset/rules source hashes are unchanged across the Windows
and Android builds. Current logs, exact APK/audit, hashes and checks remain
under `Builds/GolfLeaderboardQA/`; saved pre-existing scene/course/UI changes
are included as captured and are not separately qualified by this HUD review.

Reviewed archives: superseded art
`Legacy/20261017-140158-238-golf-leaderboard-old-art/` (10 files, 0.17 MB),
prior player `Legacy/20261017-141425-628-golf-leaderboard-player/`
(322 files, 603.19 MB), redundant candidate/portable fixture
`Legacy/20261017-141511-811-golf-leaderboard-candidates/`
(340 files, 606.88 MB). No staging, commit or push was performed.

## Golf target spacing and supplied jump image — 17 October 2026

Fresh AndroidSubmission completed **2026-10-17 01:22:19 Malaysia time**:
**93,826,511 → 93,928,409 bytes (+101,898)**. Strict 100,000,000-byte limit
headroom **6,071,591 bytes**. The 75,000,000-byte development target remains
exceeded by **18,928,409 bytes**; available reserve is limited.
Budget enforcement and independent APK v2 signature pass. SHA-256:
**8A66A0EAE47CD8449B773DA6FF461818B95D62645CF2C22A04F7E962725DD3DD**.

Only one new delivery texture is introduced: Jump, proportional 512×372,
ASTC 4×4 without mipmaps, **191,248 serialized bytes**, estimated ZIP
**98,638 bytes**. Its exact user original is retained outside Unity. No font,
shader, material, package, map or terrain asset was added for this request.
Leading contributors remain timber (2.247 MB), character albedo (1.993 MB),
Golf structures (1.966 MB), character mesh (1.588 MB) and stadium (1.556 MB).
These contribution estimates are separate from measured APK bytes.

Complete validated Windows player: `Builds/WindowsFinal/`, **322 files**,
build **2026-10-17 01:19:53**; retained matching release and exact APK/audit in
`Builds/GolfJumpTargetQA/Release/`. **90 rendered HUD assertions and 18 captures**
pass in `Builds/GolfHUDQA/Run-20261017-011953/`, including Hole 2 at 5/132/1234 m,
equal visible gaps, camera removal, original jump image, countdown, three
landscape sizes with safe insets and existing aim/hold/cancel/release behavior.
The PNG generator reproduces identical bytes from a path with spaces and an
unrelated working directory. Phone appearance/touch/FPS and WAN are unverified.

Reviewed archives: superseded player
`Legacy/20261017-012054-786-golf-jump-target-player/` (322 files, 602.93 MB),
redundant full candidate and portable fixture
`Legacy/20261017-012143-549-golf-jump-target-candidates/` (337 files, 609.24 MB).
Current source hashes, promotion checks, logs and audit remain under
`Builds/GolfJumpTargetQA/`. Saved current scenes were built; pre-existing
uncommitted course/flag/UI changes are included as captured and not separately
qualified by these HUD checks. No Git staging, commit or push was performed.

## Original action images and startup target distance — 17 October 2026

Fresh AndroidSubmission at **00:27:46 Malaysia time** measures
**90,334,226 → 93,826,511 bytes (+3,492,285)**. The opening package was the
stale **13 October** APK; this delta includes intervening course, flag and HUD
work and is not solely this task's growth. Strict headroom **6,173,489 bytes**;
the **75,000,000-byte development target remains exceeded by 18,826,511 bytes**.
This reserve is limited. Budget enforcement and independent APK v2 signature
pass. SHA-256:
**D4AC9A3B3416AC1ADCE66B3D0C66218A61444EB2F16B8E16ED54FBF03BDBE7DC**.

The three supplied Aim First/Swing/Putt images retain their original artwork
and wording. Delivery is proportional 512-pixel ASTC 4×4 with no mipmaps;
high-resolution masters remain outside Unity. Combined serialized contribution
**779,848 bytes**, estimated ZIP **540,892 bytes**. No font, shader, material or
package is introduced. [Packing audit](APK-SIZE-AUDIT.md#original-action-images-and-startup-target-distance--17-october-2026).

Complete **322-file** Windows player, built **00:25:13**, is promoted to
`Builds/WindowsFinal/`; every file hash matches the retained release in
`Builds/GolfActionImagesQA/Release/Windows/`. **86 rendered HUD assertions** and
**15 captures** pass, including the large central countdown, upper distance-only
Hole card, all three action images, touch/hold/cancel/release and three screen
sizes. [Verification and scope](VERIFICATION.md#original-action-images-and-startup-target-distance--17-october-2026).
Phone appearance, touch, FPS and WAN remain unverified.

Reviewed archives: prior masters
`Legacy/20261017-002143-064-golf-action-image-masters/` (3 files, 4.2 MB);
superseded player `Legacy/20261017-002718-573-golf-action-images-player/`
(322 files, 601.28 MB); redundant candidate and portable regeneration fixture
`Legacy/20261017-003008-652-golf-action-images-candidates/`
(335 files, 607.79 MB). Current exact APK/audit, source hashes and checks remain
in `Builds/GolfActionImagesQA/`. No Git publishing action is performed.

## Putt smoothing reverted - 17 October 2026

AndroidSubmission attempted after source/build checks; Gradle/JDK failed
with java.io.IOException: Unable to establish loopback connection. New APK
size unverified; retained historical APK remains 90,334,226 bytes, hard
headroom 9,665,774 bytes, 75 MB working-goal excess 15,334,226 bytes.
Failed build log/report retained in Builds/SizeAudit/latest and task evidence;
no fresh compressed contributor audit produced.


User requested the pre-smoothing Putt behavior while retaining latest Swing.
Restored GolfAimInput.cs and GolfBallPhysicsSettings.cs exactly from the
pre-smoothing task backups; SHA-256 matches. Putt uses sqrt(charge) speed
mapping and 30% idle preview again. Latest maximum speeds remain Swing
horizontal 34 m/s, vertical 17 m/s and Putt 15 m/s. No resource values,
UI, map, gravity, resistance or bounce settings changed. Previous smoothing
record below is superseded. No new assets/packages.
Opening historical APK 90,334,226 bytes; hard headroom 9,665,774 bytes;
75 MB goal excess 15,334,226 bytes. Fresh release needs AndroidSubmission.
Evidence: Builds/GolfPuttRestoreQA.


## Smooth putt preview charge - 17 October 2026

AndroidSubmission attempted after source/build checks; Gradle/JDK failed:
java.io.IOException: Unable to establish loopback connection. Retained
APK remains historical 90,334,226 bytes; new code APK size/growth unverified.
Historical hard headroom 9,665,774 bytes; development target excess
15,334,226 bytes. Failed build report/log retained in Builds/SizeAudit/latest
and task evidence; no fresh packed-assets contributor audit produced.


Putt idle preview now uses 0% instead of 30%, matching pointer-down and
removing the initial guide contraction. Swing idle preview remains 30%.
GolfBallPhysicsSettings maps putt charge to SmoothStep energy, then speed:
sqrt(lerp(minimumPuttSpeed^2, maximumPuttSpeed^2, SmoothStep(0,1,charge))).
This has finite, zero endpoint speed derivatives and softer low-charge
response, while preserving 0.35/15 m/s endpoints and the 2.6-second sweep.
Preview and host travel use this same mapping; no endpoint interpolation
lag, collision bypass or visual-only trajectory is introduced. Ground
resistance, gravity, rebound, Swing settings, UI and map are unchanged.
No new shipped art/package. Opening historical APK 90,334,226 bytes,
headroom 9,665,774 bytes, 75 MB goal excess 15,334,226 bytes. New APK
requires AndroidSubmission. Evidence: Builds/GolfPuttSmoothQA.


## Golf maximum launch speeds doubled - 16 October 2026

AndroidSubmission attempted: Gradle/JDK initialization failed with
java.io.IOException: Unable to establish loopback connection. New APK size
unverified; retained APK still historical 90,334,226 bytes. Historical
headroom 9,665,774 bytes, 75 MB target excess 15,334,226 bytes. Failed log
and report retained in Builds/SizeAudit/latest and task evidence; no fresh
compressed contributor audit produced.


User-requested parameter-only adjustment: maximumSwingSpeed 17 -> 34 m/s,
maximumSwingLift 8.5 -> 17 m/s, maximumPuttSpeed 7.5 -> 15 m/s. Changed
Resources/GolfBallPhysics.asset and matching fallback fields in
GolfBallPhysicsSettings.cs. Minimum launch speeds, charge remapping, gravity,
resistance, bounce, course, UI and network authority are unchanged. Shot
preview and authoritative shot plan both read the same settings.
No new assets/packages. Opening APK remains historical 90,334,226 bytes;
hard-limit headroom 9,665,774 bytes, development-target excess 15,334,226
bytes. Fresh release size needs AndroidSubmission. Build evidence is under
ignored Builds/GolfDoublePowerQA. No new automated gameplay tests requested.


## Shot plate restore and Start Game - 16 October 2026

AndroidSubmission failed Gradle/JDK initialization: java.io.IOException:
Unable to establish loopback connection. Retained historical APK remains
90,334,226 bytes; new source APK size/growth unverified. Failed log/report
in Builds/SizeAudit/latest and Builds/GolfShotUIRestoreQA/android-build.log.
No fresh packed-assets contributor report.

UI validation: 82 assertions, 14 captures in
Builds/GolfHUDQA/Run-20261016-230547. Inspected new StartGame, restored
Aim First/Putt/Swing without caption rectangles, native power percentage,
retained Exit/Your Ball and no ball distance. Checked three resolutions,
safe insets and original hold/cancel/release UI. Retained source hashes match.
Complete 322-file player copied to WindowsFinal with matching SHA-256 before
delivery note. Old player: Legacy/20261016-230728-537-golf-shot-ui-player.
No new device/WAN/map testing.


Retain Exit/Your Ball artwork; remove live ball distance. Start Game now uses
the new original image. Aim First/Putt/Swing restore Reference/Shot, native
labels and charge percentage, without a ShotCaption overlay. Corrected
GolfReferenceArtBuilder blends lower disc colours using an ellipse feather
instead of rectangular caption fills. No gameplay/input changes.

New StartGame PNG: 97,478 bytes; raw ASTC 4x4: 77,824 bytes. Removed three
unused action sprites: 951,884 PNG bytes / 768,000 raw ASTC bytes. Masters
retained outside Unity. These asset totals do not measure APK growth.
Portable generators and .meta files retained; obsolete delivery sprites
archived with explicit reference/GUID checks to
Legacy/20261016-230149-840-golf-shot-ui-unused.
Opening historical APK remains 90,334,226 bytes; headroom 9,665,774 bytes;
75 MB development-target excess 15,334,226 bytes. Fresh size requires build.


## Original action badges - 16 October 2026

AndroidSubmission attempted after UI validation; failed Gradle/JDK
initialization with java.io.IOException: Unable to establish loopback
connection. Retained APK remains the historical 90,334,226-byte file.
New badge APK size/growth and a fresh packed-assets contributor audit are
unverified. Historical hard-limit headroom 9,665,774 bytes; 75 MB working
goal exceeded by 15,334,226 bytes. Failed report/log: Builds/SizeAudit/latest
and Builds/GolfBadgesQA/android-build.log.


Aim First, Putt, Swing, cart Exit and Your Ball use original user-supplied
PNG designs with baked lettering, proportional simple images, no redraw.
Five unchanged masters remain in ArtSource/Golf/UI/Badges; portable generator
Tools/Art/prepare_golf_badges.py and delivery audit record hashes/crops.
Five 512 px PNGs total 1,157,582 bytes; raw Android ASTC 4x4 totals
940,032 bytes. These are not measured compressed APK growth. No added
font/material/shader/package; high-resolution originals do not ship.
Live charge moves to the control hint, live ball distance below original
art. Existing arrow, near-ball hide thresholds and input rules remain.
Opening historical APK: 90,334,226 bytes; hard headroom 9,665,774 bytes;
75 MB development-target excess 15,334,226 bytes. Fresh size requires a
successful AndroidSubmission, separate from source/texture measurements.


## Original cart buttons - 16 October 2026

AndroidSubmission attempted after UI checks; Gradle/JDK failed with
java.io.IOException: Unable to establish loopback connection. APK after
attempt is still the historical 90,334,226-byte file: new button APK size
and growth are unverified. Historical headroom is 9,665,774 bytes; the
75 MB development target is exceeded by 15,334,226 bytes. Failed build
log/report retained in Builds/SizeAudit/latest and Builds/GolfCartButtonsQA.
No current packed-assets contributor report was produced.


Opening retained APK: 90,334,226 bytes, hard-limit headroom 9,665,774 bytes,
75 MB development-target excess 15,334,226 bytes. Three original-design
button sprites add 400,224 PNG bytes / 282,624 raw ASTC 4x4 bytes. Masters
remain outside Unity. Downsampled 512 px delivery preserves original pixels
at current roughly 270 px display width. No new font/material/shader/package.
These are asset sizes, not APK growth; a fresh build must be measured.
See [verification](VERIFICATION.md#original-cart-buttons---16-october-2026).


## Golf button and ball-indicator refinement - 15 October 2026

AndroidSubmission attempted for this task: Gradle/JDK initialization failed
with java.io.IOException: Unable to establish loopback connection. Retained
APK remains 90,334,226 bytes; fresh UI APK size and compressed contributor
growth are unverified. Builds/SizeAudit/latest contains the failed build log
and report, not a fresh packed-assets audit. Development budget remains
15,334,226 bytes above the 75 MB goal on the historical baseline.


Opening retained APK **90,334,226 bytes**, historical hard-limit headroom
**9,665,774 bytes**, 75 MB development target excess **15,334,226 bytes**.
New UI reuses existing reference plates/fonts and adds one small native
steering-wheel mesh, with no texture/material/shader/model/audio/package.
Resulting AndroidSubmission must be measured separately; old APK bytes do
not validate these UI changes. See
[verification](VERIFICATION.md#golf-button-and-ball-indicator-refinement---15-october-2026).


## V5 charge controls restored - 15 October 2026

Reverted post-V5 Swing controls/camera/launch changes. Existing APK at task
start is **90,334,226 bytes**; AndroidSubmission must be measured separately.
No shipped art/resource is added; redesign-only runtime UI/probe is archived.
Historical APK headroom **9,665,774 bytes** and development-target excess
**15,334,226 bytes** do not prove freshness of the restored source. Records
below for click/direct-camera controls describe superseded implementations.
See [restore verification](VERIFICATION.md#v5-charge-controls-restored---15-october-2026).

Current restored-source AndroidSubmission failed with Gradle/JDK loopback
initialization. After/delta and fresh contributors are **unverified**. Existing
historical APK remains **90,334,226 bytes** (headroom **9,665,774**, 75 MB goal
excess **15,334,226**). Complete validated Windows V5 charge player is delivered.


## Direct camera and launch angle - 14 October 2026 build limitation

Opening retained APK **90,334,226 bytes**. AndroidSubmission fails in the
existing Gradle/JDK loopback initialization, including process-local IPv4
preference. No fresh after/delta is measured for this source. Historical
hard-limit headroom **9,665,774 bytes** and 75 MB development-goal excess
**15,334,226 bytes** do not certify these changes. No texture, mesh asset,
material, shader, font, audio or package is added. Camera framing, an explicit
zero-loft flag and existing ground travel are code only. Fresh packing growth
and largest contributors remain unavailable in the failed latest audit.
See [verification](VERIFICATION.md#direct-camera-and-launch-angle---14-october-2026).


## View aim and click Swing - 14 October 2026 build limitation

Opening retained APK **90,334,226 bytes**. AndroidSubmission attempts, including
process-local IPv4 preference and two workers, fail in Gradle/JDK loopback
initialization. No fresh resulting APK or measured after/delta exists for
these controls. Historical hard-limit headroom **9,665,774 bytes** and excess
over the 75 MB development goal **15,334,226 bytes** do not validate this source.

The power rail reuses existing Golf plate/ball artwork with a circular UV mesh;
no texture, model, font, audio, shader or package is added. Loft and power share
the existing trajectory solver. Windows is validated separately; see
[verification](VERIFICATION.md#view-aim-retained-power-and-click-swing---14-october-2026).
The latest size audit contains a failed build, not a fresh contributor ranking.


## V5 Golf ponds and obstacles - 14 October 2026 build limitation

Opening APK **90,334,226 bytes**. Current AndroidSubmission failed in Gradle/JDK
loopback initialization; no fresh after/delta measurement exists. The retained
historical APK has **9,665,774 bytes** of hard-limit headroom and is **15,334,226
bytes** above the 75 MB development goal. It cannot validate this new source.

New 141 props reuse packed rock/palm meshes, LODs and materials. Fourteen rough
patches share existing turf/surface resources. Six water meshes (385 vertices
each) and banks (256 vertices each) use 16-bit shared indices and existing water
shader/sand resources; one shared pond material, no texture/reflection camera,
model/font/audio/package is added. High-quality design references stay outside
Unity. No unrelated quality reduction or dependency change was made.

`Builds/SizeAudit/latest` contains the failed current log/report, so compressed
cost and largest contributors remain unverified. Complete validated Windows
player is delivered; [verification](VERIFICATION.md#v5-golf-obstacles-ponds-and-dry-recovery---14-october-2026).


## Golf obstacle V2 — 14 October 2026 build limitation

Opening APK: **90,334,226 bytes**. Current-source AndroidSubmission failed
with Gradle/JDK `java.io.IOException: Unable to establish loopback connection`,
including a process-local IPv4 preference. No fresh APK or after/delta measurement
exists. The retained historical package has **9,665,774 bytes** of hard-limit
headroom and exceeds the 75 MB development goal by **15,334,226 bytes**;
it does not validate these obstacle additions.

Added 72 props reuse packed rock/palm meshes, LODs and materials. Nineteen sand
and six rough overlays use welded vertices and 16-bit indices with existing
textures/shader; one shared rough material and its mobile derivative are added.
No new bitmap, model, font, audio, package or unrelated quality reduction ships.
`Builds/SizeAudit/latest/` contains the failed current build log/report, so no
fresh largest-contributor ranking or compressed growth can be claimed.
Complete tested Windows player is delivered; [evidence](VERIFICATION.md#additional-golf-obstacle-v2--14-october-2026).


## Original screenshot Golf UI — 14 October 2026 build limitation

Opening APK **90,334,226 bytes**. AndroidSubmission is blocked by Gradle/JDK
loopback initialization; no fresh resulting APK or measured delta exists.
Historical headroom is **9,665,774 bytes** and development-target excess
**15,334,226 bytes**. These figures do not certify the new UI.

Twenty-three original-reference UI sprites share existing Cove fonts and add
no model/shader/package. Delivery PNGs total **743,690 bytes**; raw ASTC 4x4
blocks total **537,936 bytes**, separate from APK compression. The full
1.7 MB reference stays outside Unity. Two old 128/256 illustrations and metas
were archived after reference/load/generator checks. Inspect a fresh successful
packing audit before attributing any compressed growth. See [verification](VERIFICATION.md#original-screenshot-golf-ui--14-october-2026).

## Circular Golf course — 14 October 2026 build limitation

Opening APK: **90,334,226 bytes**; both current-source AndroidSubmission attempts failed
with Gradle/JDK `Unable to establish loopback connection`. No resulting APK
measurement exists. The retained historical APK has **9,665,774 bytes** of
strict-limit headroom and exceeds the 75 MB working goal by **15,334,226 bytes**.
It is not validation of the new course. Twelve obstacles reuse existing kit
meshes/LODs; fairway/sand reuse already packed turf/sand textures. Signs share
the existing font. Adjacent mesh cells are welded and use 16-bit indices where
possible. Unused relocated terrain/grass derivatives were archived. Fresh
compressed growth and largest contributors cannot be measured until Android
build succeeds. See [verification](VERIFICATION.md#circular-golf-course--14-october-2026).

The project owner raised the APK limit to **1,000,000,000 bytes (1 decimal GB)**
on 15 October 2026. The 75 MB development target is retired; current size-audit requirements
are defined by AGENTS.md. Older records below retain the limits applicable when they were made;
they do not define the current policy. See [AGENTS.md](../AGENTS.md).

## Main menu merge - 17 October 2026

Merged incoming `ae9dc3b` (sports menu, startup welcome, persistent guest names,
football passing and basketball restarts) with the validated Fishing hold controls
and resilient content preparation. Incoming networking is preserved, with the
combined protocol raised to **67**: both parent deliveries independently used 66
for different serialized gameplay records, so mixed builds must not connect.
Fishing keeps its current camera, rod, rope, swimming and splash presentation.
The welcome screen now uses the existing safe preparation lease, checksum and
length validation, direct desktop source read and owned temporary-file cleanup.

The complete `Builds/WindowsFinal/` player and AndroidSubmission were freshly
rebuilt from this combined source. All seven integration suites pass, reporting
**4,659 assertions/checks** across startup/profile layouts, two-player names,
offline/online sports menus, Fishing controls/network, results, interrupted
copy-source preparation/retry and basketball restarts. All **406 C# files under `Game/Assets/`**
remain unchanged between the Windows validation snapshot and Android build.
The initial probes still expected removed inline sport tabs and old loading
control names. They now use the guide hub, header Back target and the welcome's
Retry button. Storage-recovery guidance was restored on the welcome error card.
Guarded scripts-only refreshes preserved native player data, catalog and bundle
hashes. Name/menu tests passed on unchanged code before these corrections;
Fishing/results passed after the navigation correction. Startup/profile layouts
were rerun after the storage message change, followed by recovery and restarts.
Earlier failed probe evidence is retained for diagnosis.
Generated scene/prefab graphs were compared by named objects and component references.
Only Bootstrap protocol 49 -> 67 changed semantically; equivalent ID/order churn
was restored to the incoming serialization. Native player content is unchanged.
The incoming six menu/welcome PNGs pass their recorded hashes and PNG CRCs.
Rendered welcome, retry, sports-menu and hooked-Fishing captures were visually
inspected on Windows.
Documentation links also pass from a relocated folder with spaces and an
unrelated working directory. Runtime evidence is under
`Builds/GitSync/Task-20261016-200323/MenuMerge/`.

APK: **75,053,357 -> 78,456,347 bytes
(+3,402,990)**, built 2026-10-17T01:56:46 Malaysia time.
It retains **21,543,653 bytes below 100 MB**, satisfying
the task's stricter budget as well as the repository's current 1 GB gate.
The old 75 MB target has -3,456,347 bytes of reserve;
the incoming repository policy retires that target. Independent APK v2 signature,
ARM64-only libraries and embedded five-scene bundle length/SHA-256 checks pass.
APK SHA-256: `0E8979906AD38453F6F7B61D91F88F39C8C08DBC050BF2A31202F5EB0A0A094C`.

Physical Android touch, keyboard, performance, phone cutouts and WAN behavior
remain unverified. Huawei login/cloud recovery is still coming soon; the new
guest profile is local to the device. Superseded complete players/APKs were
archived with the repository tool; batch paths are in the delivery stamp.

## Main sync and Fishing integration - 16 October 2026

Fresh AndroidSubmission at **21:41:00 Malaysia time** measures
**73,459,821 -> 75,053,357 bytes (+1,593,536)**. Independent APK v2 signature verification
passes. SHA-256: `EB1E8A61B41F130AA470EE259B402B9ED8BC003D6B05D75D57E98A96DFA11377`.
The APK is **24,946,643 bytes below 100 MB**, also satisfying
the incoming repository's 1 GB gate. This is a measured APK, rather than a source
folder estimate. The older 75 MB target has -53,357 bytes
of reserve against this build; the incoming repository policy retires that target.

The combined source keeps incoming goal replay, ten character identities,
teleport, spectators and host recovery. Fishing retains Hold the Glow,
directional camera, rod/rope/splash presentation, six abilities and two favourites.
Local content startup retains checksum validation, a preparation lease, safe
desktop source handling and owned temporary-file cleanup. Delayed Fishing
results now close an already-open next-adventure card before taking input.
Peers require **protocol 66** because the combined Fishing record layout differs
from incoming protocol 65.

The complete Windows player is in `Builds/WindowsFinal/`.
**3,310 runtime assertions across nine final reports pass**, alongside the
compiled SDK regressions. Runtime C# is unchanged after Windows validation.
See [verification](VERIFICATION.md#main-sync-and-fishing-integration---16-october-2026).
Evidence: `Builds/GitSync/Task-20261016-200323/`. Physical-phone and WAN qualification remain outstanding.

## Abrupt guest disconnect recovery - 15 October 2026

Fresh AndroidSubmission at **21:54:03 Malaysia time** measures
**75,006,569 bytes** and has **924,993,431 bytes** below the new 1 GB limit.
Independent APK v2 signature verification passes. SHA-256:
`C58D93EEC18676D089ADE3BFCEAC06EB7DB38B122798FFB608B857A2C0EBC795`.

[Disconnect recovery](DISCONNECTS.md) embeds the existing Unity Transport 2.7.3
dependency with a narrow receive-buffer fix and guards restoration of a
despawned traveller. No art or quality reduction is part of this change. The
build completed before the policy update and passed the former stricter gate;
the updated readiness check validates this same APK against 1 GB. Editor-only
policy changes are separately compiled and do not change the delivered game.
The preceding transport-only candidate's optional detailed audit is retained
in [the audit history](APK-SIZE-AUDIT.md#abrupt-guest-disconnect-recovery---15-october-2026).

Current delivery evidence is under `Builds/DisconnectQA/`, with the complete
Windows player in `Builds/WindowsFinal/`. Both builds use protocol **64**.
See [verification](VERIFICATION.md#abrupt-guest-disconnect-recovery---15-october-2026)
for capacity results and the limits of local Windows testing.

## Spectator rooms and sky arrivals - 15 October 2026

Fresh AndroidSubmission at **19:58:15 Malaysia time** measures
**74,986,217 -> 75,014,337 bytes (+28,120)** against the task's starting APK.
The intervening pickup release was 75,003,257 bytes; growth from it is
**11,080 bytes**. Strict-limit headroom is **24,985,663 bytes**. The
75,000,000-byte development target is **exceeded by 14,337 bytes** and is not
achieved. The enforced hard budget and independent APK v2 signature pass.
SHA-256: `383141E730D7740B8E94CDDF5FAD2C4907AE0FB889EC9F567ED9A35BB07621BA`.

[Spectator rooms](SPECTATORS.md) reuse the same ten character models, existing
stadium seating and teleport effects. No new texture, mesh, audio, animation
clip or package is shipped. The bundle's asset-by-asset serialized comparison
changes only the network-character prefab by **24 bytes**. Native code,
metadata and small lossless packing changes explain the compressed growth;
the opening delta also includes concurrent prop-pickup work. The small
development-target excess is recorded rather than treated as spare budget.
See [the detailed audit](APK-SIZE-AUDIT.md#spectator-rooms-and-sky-arrivals---15-october-2026).

The complete validated Windows player is in `Builds/WindowsFinal/`. Current APK,
signature, source hashes, audits and test evidence are under
`Builds/SpectatorQA/`. [Verification](VERIFICATION.md#spectator-rooms-and-sky-arrivals---15-october-2026)
records 1,555 passing checks, stable normal replacements in 20/15-process rooms,
the preceding-candidate limits and the forced-process-termination UDP stress
case that was unresolved at that release, subsequently fixed by
[disconnect recovery](DISCONNECTS.md). Physical Android and WAN/Relay validation
remain outstanding.

## Forgiving prop pickups - 15 October 2026

Fresh AndroidSubmission at **19:10:43 Malaysia time** measures
**74,986,217 -> 75,003,257 bytes (+17,040)**. Strict-limit headroom is
**24,996,743 bytes**. The 75 MB development target is **exceeded by 3,257 bytes**;
it is not achieved. The enforced hard budget and independent APK v2 signature
verification pass. SHA-256:
`AAFEEDBB84D0FCD6F7374F204AED387DEB8258D6F47275E030DDAE7D3450E437`.

[Football](FOOTBALL-ABILITIES.md) and [basketball](BASKETBALL-PROPS.md) prop
pickup radius increases from **0.85 to 1.2 m**, with collection on the spawn
simulation tick for eligible waiting players. This is a shared constant and
two control-flow changes; no texture, mesh, material, audio or package was added.
The snapshot also contains concurrent room-entry/seat/spectator work. Growth is
code/metadata, while the compressed scene bundle shrinks 2,443 bytes; it is not
an isolated size measurement of the pickup fix. See [the packing audit](APK-SIZE-AUDIT.md#forgiving-prop-pickups---15-october-2026).

The complete 325-file Windows player is updated in `Builds/WindowsFinal/`.
[Verification](VERIFICATION.md#forgiving-prop-pickups---15-october-2026) records
1,021 assertions, code-equivalent candidate limits and archives. Current APK,
baseline and audits are under `Builds/PropPickupQA/`. Physical Android touch,
appearance, RAM/FPS and WAN play remain unverified.

## Room teleport, post-match choices and cabin lighting - 15 October 2026

Fresh AndroidSubmission at **18:06:41 Malaysia time** measures
**74,943,705 -> 74,986,217 bytes (+42,512)** against the task's opening APK.
It is **13,783 bytes below the 75,000,000-byte development target**, with
**25,013,783 bytes** of headroom to the strict 100,000,000-byte ceiling.
The size gate and independent APK v2 signature verification pass. SHA-256:
`6CEEDCE74FF4F2BF067340C49D65C5EE8C39F3FA965C9DE6504FB60EF0DA563B`.

[Room teleport](TELEPORT.md) now connects the reusable animation to host choices
after all four sports, with coordinated arrival and rollback. Cabin bulbs render
after ambient shading; inside and nearby cabin views avoid noisy contact shading.
The change reuses existing art, fonts and shaders, adding no textures, meshes,
audio or packages. The opening-to-final delta also includes concurrent football
swipe/pass/pressure changes; it is not an isolated feature measurement. Growth
from the intervening 74,980,797-byte release is **5,420 bytes**. The final bulb
draw-order correction costs **572 bytes** versus the preceding task candidate.

The complete validated Windows player is in `Builds/WindowsFinal/`. Current APK,
audit and verification evidence are retained under `Builds/RoomTeleportQA/`.
See [the packing audit](APK-SIZE-AUDIT.md#room-teleport-post-match-choices-and-cabin-lighting---15-october-2026)
and [verification](VERIFICATION.md#room-teleport-post-match-choices-and-cabin-lighting---15-october-2026).
The development reserve remains small. No Android device was attached; phone
appearance, touch, peak RAM and FPS remain unverified.

## Football swipe, pass guide and pressure - 15 October 2026

Fresh AndroidSubmission at **17:36:28 Malaysia time** measures
**74,943,705 -> 74,980,797 bytes (+37,092)**. Development reserve is
**19,203 bytes** below 75,000,000; strict-limit headroom is **25,019,203 bytes**.
The enforced APK budget and independent APK v2 signature verification pass.
SHA-256: `0D6C9ADDC50165A543CEF68E092B9F1C5430DEC91DD7A103479F970373F9D474`.

[Football controls and passing](FOOTBALL-PLAYMAKER.md) now support captured
swipes across all four action hitboxes, a contrasting ground-pass path and
receiver jersey label. [Pressure](FOOTBALL-PACE-AND-PRESSURE.md) retains the
25% base and adds ten percentage points per extra eligible defender, capped at
three (25/35/45%). This task adds no textures, meshes, audio or packages.
The shared build also includes concurrent room teleport/post-match/cabin work;
its growth is not an isolated measurement of the football changes.

The complete 325-file validated Windows player is in `Builds/WindowsFinal/`.
[Verification](VERIFICATION.md#football-swipe-pass-guide-and-pressure---15-october-2026)
records 658 final assertions, compact 844 x 390 captures and source snapshot
limits. The [packing audit](APK-SIZE-AUDIT.md#football-swipe-pass-guide-and-pressure---15-october-2026)
records contributors. Task APK and evidence: `Builds/FootballSwipeQA/`.
Physical Android touch, appearance, RAM/FPS and WAN play remain unverified.

## Character head refinement - 15 October 2026

Fresh AndroidSubmission at **16:54:23 Malaysia time** measures
**74,754,317 -> 74,943,705 bytes (+189,388)**. The release is **56,295 bytes
below the 75 MB development target**, with **25,056,295 bytes** of headroom to
the strict 100,000,000-byte ceiling. Budget enforcement and APK v2 signature
verification pass. SHA-256:
`07E1A1C32583F255FC7CCE28491E1AD8AEA7DC99B6CDE4F9FB6CFCF61FB731D5`.

The [refined collection](VisualDirection/CHARACTER-IDENTITIES.md#head-refinement)
fixes ear/head joins, improves scalp shading and adds sculpted hair and hat
details at both LODs. The original body, rig, textures, grips and animation clips
remain shared. Hidden triangles and unused head coordinates were removed from
delivery data, retaining visible detail. The final APK is **198,872 bytes
smaller** than the preceding 75,142,577-byte character candidate. The opening
to final delta includes concurrent match-boundary and teleport work; it is not
an isolated measurement of the head feature.

See [the packing audit](APK-SIZE-AUDIT.md#character-head-refinement---15-october-2026)
and [verification](VERIFICATION.md#character-head-refinement---15-october-2026).
The exact APK and audit snapshot are retained under `Builds/CharacterRefinementQA/`.
The small development reserve still requires care with future additions.
No physical Android device was connected; phone quality and FPS are unverified.

## Blue-light teleport preview - 15 October 2026

Fresh AndroidSubmission at **16:31:03 Malaysia time** measures
**74,756,265 -> 75,142,885 bytes (+386,620)** against the opening observed APK.
The archived comparison baseline was an intervening 74,754,317-byte release;
growth from that baseline is **388,568 bytes**. Strict-limit headroom is
**24,857,115 bytes**. This is **142,885 bytes above the 75 MB development target**;
the development target is not achieved. The hard budget and APK v2 signature pass.
SHA-256: `F28EF3C4996CA79943979B0AFC272DB8EFDD5CBF83524E0D291C434B8AC3B53F`.

The [teleport preview](TELEPORT.md) adds reusable blue-light effects, visible
camera travel and eight capacity-checked destination sets. It uses procedural
shared geometry and existing character materials/textures. Shared growth includes
concurrent character-head refinements; their packed assets reduced the interim
75,350,821-byte build to this measured release. See the [packing audit](APK-SIZE-AUDIT.md#blue-light-teleport-preview---15-october-2026).
The signed task snapshot is retained at `Builds/TeleportQA/WhatTheFish-release.apk`
with `SizeAuditFinal/`; a concurrent release may later replace the shared Android
output. Physical Android appearance, touch, peak RAM and FPS remain unverified.

## Match exit boundaries - 15 October 2026

Fresh AndroidSubmission at **15:46:57 Malaysia time** measures
**74,754,317 -> 74,810,941 bytes (+56,624)**.
Development reserve: **189,059 bytes**; strict-limit headroom:
**25,189,059 bytes**. Budget and APK v2 signature verification pass.
SHA-256: `9841BC0103E22D4096B7C0D6A733649FBC25385D81299C4C6DF3BCD2612FBF9D`.

Football and basketball release player boundaries after the final result;
closing MVP allows island exploration. Active matches restore containment.
The size delta also includes concurrent teleport code, two materials and two
shaders. This boundary fix adds no textures, meshes or audio. See the
[packing audit](APK-SIZE-AUDIT.md#match-exit-boundaries---15-october-2026) and
[verification](VERIFICATION.md) for source snapshot limits. Physical Android
appearance, touch and performance remain unverified.

## Basketball statistics and courtside MVP - 15 October 2026

Fresh AndroidSubmission at **15:25:29 Malaysia time** measures
**74,728,161 -> 74,754,317 bytes (+26,156)**.
The development reserve below 75,000,000 bytes is **245,683**;
headroom to the strict 100,000,000-byte ceiling is **25,245,683 bytes**.
The enforced APK budget and independent APK v2 signature verification pass.
SHA-256: `65788D64FE66042BAACF6DF7AEA26459640692511201C888C1ACB10CFDF4549D`.

This release adds [basketball per-player statistics and MVP](BASKETBALL-HONOURS.md)
to VS AI 2v2-5v5 and rooms. The award reuses the existing character portraits,
basketball icons and actual arena; basketball seams and team accents are UI
geometry. No texture, mesh, audio bank or environment copy was introduced.
See [the packing audit](APK-SIZE-AUDIT.md#basketball-statistics-and-courtside-mvp---15-october-2026)
and [verification](VERIFICATION.md) for measured contributors and build limits.
No physical Android device was connected; device quality and FPS are unverified.

## Football passing and illustrated MVP - 15 October 2026

Fresh AndroidSubmission at **14:15:27 Malaysia time** measures
**73,988,013 -> 74,728,161 bytes (+740,148)** against this task's opening APK.
The intervening trail release was 74,020,385 bytes; growth from that release
is **707,776 bytes**. The development reserve below 75,000,000 bytes is
**271,839 bytes**; headroom to the strict 100,000,000-byte ceiling is
**25,271,839 bytes**. The enforced size gate and independent APK v2 signature
verification pass. SHA-256:
`EB49D9B173236EA23BBF3EB1DC8F8698EEEB854AA4DDB7C6508D8033454B0026`.

This release adds the [Pass action, match statistics and illustrated MVP](FOOTBALL-PLAYMAKER.md),
Dash/Shoot gestures and local-only footsteps. Ten 1024 x 1536 PNG masters use
512-pixel ASTC 6x6 Android imports with no mipmaps or read/write copies. The
award reuses the real pitch and stadium; no duplicate environment is shipped.
The opening-to-final delta includes the concurrent trail work. Largest-entry
and asset comparisons are in [the packing audit](APK-SIZE-AUDIT.md#football-passing-and-illustrated-mvp---15-october-2026).
Windows validation and source limits are in [verification](VERIFICATION.md).
No connected Android device was available for touch, appearance or FPS testing.

## Personal football trails - 15 October 2026

Fresh AndroidSubmission at **13:39:35 Malaysia time** measures
**73,988,013 -> 74,020,385 bytes (+32,372)**. The reserve below 75,000,000 bytes
is **979,615 bytes**; headroom to the strict 100,000,000-byte ceiling is
**25,979,615 bytes**. The enforced gate and independent APK v2 signature
verification pass. SHA-256:
`6737E37E5B48912D0A496CDE0FE52E4254BF886FA2CC424C8E7A9BE77A855ABF`.

The football diameter increases from **44 to 52.8 cm**, with matching mesh,
collider and centre height. [Five personal trails](FOOTBALL-TRAILS.md) share one
procedural shader and material resource. Each viewer's saved selection applies
throughout live play and replay, independently of the kicker. No textures,
meshes, audio or packages were added for this feature. Both mesh inventories
are unchanged; see the [packing audit](APK-SIZE-AUDIT.md#personal-football-trails---15-october-2026).
The measured delta also includes concurrent passing, controls and MVP work;
it is not an isolated trail-only size benchmark.

The complete **324-file** Windows player built at **13:32:03**, protocol **60**,
was promoted to `Builds/WindowsFinal/` at **13:37:21** with every file verified.
[Verification](VERIFICATION.md#personal-football-trails---15-october-2026)
records **661 passing assertions**, actual rendered previews and source limits.
Android includes three later concurrent MVP/review source edits; this task's
production code matches the tested Windows source. Physical Android appearance
and performance remain unverified. Evidence: `Builds/FootballTrailQA/`, including
`SizeAuditBefore/`, `SizeAuditFinal/` and `apk-comparison.json`.

## Longer football transitions - 15 October 2026

Fresh AndroidSubmission at **02:34:35 Malaysia time** measures
**73,991,313 -> 73,988,013 bytes (-3,300)**. Development reserve below
75,000,000 bytes is **1,011,987 bytes**; headroom to the strict 100,000,000-byte
ceiling is **26,011,987 bytes**. The enforced gate and independent APK v2
signature verification pass. SHA-256:
`2A5FE9F9529AD344062771496CD1B9CD17C7C7E77570AAE9398E42FCB1CF4643`.

The goal card now lasts **1.6 seconds**, including a full **1.0-second hold**.
The return lasts **0.8 seconds**, with camera switches and the centre reset
under the covered holds. Replay footage remains five seconds. No assets,
textures, meshes, audio or packages were added; both serialized inventories
and mesh inventories are unchanged. See the
[audit](APK-SIZE-AUDIT.md#longer-football-transitions---15-october-2026).

The complete **324-file** Windows player built at **02:28:43**, protocol **58**,
is delivered in `Builds/WindowsFinal/`, retaining the current character identities
and earlier gameplay. Every delivered file matches the tested candidate.
[Verification](VERIFICATION.md#longer-football-transitions---15-october-2026)
records **271 passing assertions** and **16 rendered timing traces** across
VS AI and local rooms. Physical Android quality and performance remain
unverified. Evidence: `Builds/FootballReplayTimingQA/` and its `SizeAuditFinal/`.

## Ten player identities - 15 October 2026

Fresh AndroidSubmission at **02:11:35 Malaysia time** measures
**73,482,037 -> 73,991,313 bytes (+509,276)** against the preceding illustrated
replay release. Development reserve below 75,000,000 bytes is **1,008,687 bytes**;
headroom to the strict 100,000,000-byte ceiling is **26,008,687 bytes**.
The enforced APK gate and independent APK v2 signature verification pass.
SHA-256: `701974B45CE7F671149E9DA9B729E0928CF7350AAA05E6EAE0073B9E212186AA`.

Ten unique heads and room numbers share the existing body, textures and nine
animation clips. Football/basketball use Blue/Red uniforms; other activities
retain personal colors. One shared digit atlas and small head/scalp meshes
provide the new art. Number-coordinate quantization reduced the first measured
identity APK from **74,006,669 to 73,991,313 bytes (-15,356)**. Growth is
**9,276 bytes above** the plan's 500,000-byte engineering target; the working
APK target remains satisfied. No unrelated world or texture quality was cut.
See the [packing audit](APK-SIZE-AUDIT.md#ten-player-identities---15-october-2026).

The opening APK was 73,446,301 bytes. Its combined delta is +545,012 bytes,
including the intervening replay release (+35,736); it is not all attributable
to character identities. The complete **324-file** Windows player built at
**01:47:28**, protocol **57**, is delivered in `Builds/WindowsFinal/`.
It includes the latest illustrated replay and the shared working tree's prior
gameplay changes. [Verification](VERIFICATION.md#ten-player-identities---15-october-2026)
records final-player checks, earlier regressions and source limits. Physical
Android appearance and performance remain unverified.

Evidence: `Builds/CharacterIdentityQA/release-audit.json`, `character-storage-final.json`,
`apk-signature.txt`, `verification.json` and `SizeAuditFinal/`.

## Illustrated football goal replay - 15 October 2026

Fresh AndroidSubmission at **00:37:32 Malaysia time** measures
**73,446,301 -> 73,482,037 bytes (+35,736)**. Development reserve below
75,000,000 bytes is **1,517,963 bytes**; headroom to the strict
100,000,000-byte ceiling is **26,517,963 bytes**. The enforced build gate and
independent APK v2 signature verification pass. SHA-256:
`FDA8B748C4919FE05441A8A35D0E2EE320E9D4CF890A15726B6D46D099F5A216`.

The [illustrated Broadcast Slash](GoalReplay/README.md) reuses the official
logo and native vector drawings. Five seconds of recorded goal footage precede
a conceding-team centre kick with free aiming. VS AI, human rooms and rooms
filled with bots share this flow. No texture, mesh, audio bank or package was
added. Both mesh inventories and the bundle's serialized asset total are
unchanged; see the [packing audit](APK-SIZE-AUDIT.md#illustrated-football-goal-replay---15-october-2026).

The complete **324-file** Windows player built at **00:31:09** is delivered in
`Builds/WindowsFinal/`, protocol **56**, with every file hash-verified against
the tested candidate. [Verification](VERIFICATION.md#illustrated-football-goal-replay---15-october-2026)
records **360 passing final-player assertions** and separate earlier gameplay
regressions. This isolated source snapshot excludes concurrent unfinished
character-identity work in the shared checkout. Physical Android appearance,
touch and performance remain unverified. Current evidence is under
`Builds/FootballReplayQA/`, including `SizeAuditFinal/`.




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

Complete Windows player: **324 files**. **2,767 runtime checks** and **1,571 rule checks** pass.
[Verification](VERIFICATION.md#fishing-two-button-hold-the-glow-restoration---15-october-2026), [size audit](APK-SIZE-AUDIT.md#fishing-two-button-hold-the-glow-restoration---15-october-2026).
Evidence: `Builds/FishingHold/Task-20261015-201923/`. Physical-phone and WAN verification remain outstanding.

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

Complete Windows player: **324 files**. **443 runtime checks** and **1,576 rule checks** pass.
[Verification](VERIFICATION.md#local-content-startup-recovery---15-october-2026), [size audit](APK-SIZE-AUDIT.md#local-content-startup-recovery---15-october-2026).
Evidence: `Builds/LaunchRecovery/Task-20261015-165153/`. Physical-phone and WAN verification remain outstanding.

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

Complete Windows delivery: **324 files**, **2,853 passing assertions**.
[Verification](VERIFICATION.md#fishing-timed-pulls-and-directional-camera---15-october-2026), [packing audit](APK-SIZE-AUDIT.md#fishing-timed-pulls-and-directional-camera---15-october-2026).
Evidence: `Builds/FishingTiming/Task-20261015-151017/`. Physical-phone and WAN qualification remain outstanding.

## Fishing clarity, camera and catch feedback - 15 October 2026

Fresh AndroidSubmission written **14:31:37 Malaysia time** measures
**73,446,373 -> 73,442,073 bytes (-4,300)**.
Headroom to the strict 100,000,000-byte boundary: **26,557,927
bytes**. Development reserve below 75,000,000: **1,557,927 bytes**.
The enforced hard size gate and independent APK v2 signature verification pass.
SHA-256: `F39B0474F0F59EFFFA8F850482356F0FB26EE8BF976511742B18629343A00D56`.

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

Complete Windows delivery: **324 files**, **2,755 passing
assertions** across 9 final reports. [Verification](VERIFICATION.md#fishing-clarity-camera-and-catch-feedback---15-october-2026),
[packing audit](APK-SIZE-AUDIT.md#fishing-clarity-camera-and-catch-feedback---15-october-2026), [controls and camera](FISHING-COUNTERSTEER.md).
Evidence: `Builds/FishingClarity/Task-20261015-131941/`. Physical Android and WAN qualification remain outstanding.

## Local rebuild of pulled main - 14 October 2026

Fresh local AndroidSubmission written **20:45:42 Malaysia time** measures
**90,388,492 -> 73,446,373 bytes (-16,942,119)** against
the older APK on this laptop. Headroom to the strict 100,000,000-byte boundary:
**26,553,627 bytes**. Development reserve below 75,000,000:
**1,553,627 bytes**. Enforced hard limit and independent APK v2 signature pass.
SHA-256: `51723F75318AE12D75C7494F674A1A08B79DA67C2A25F119FB4933745F7C715A`.

Rebuilds pulled `main` at `4b20c6b`. No new gameplay or art change
is authored. All four islands, shared room travel, Golf camera/buggy/flags/trail,
programmed sports bots, current match/prop rules, network movement, live renderer
lighting and Fishing's existing features are retained. Protocol **55** requires
matching players. Lossless scene AssetBundle sharing and release minification
are the upstream packaging changes; this is not removal of islands or an
external content download. Installed storage also includes the expanded runtime
cache and must be measured separately on a phone.

Complete 324-file Windows player is delivered with
**3,546 passing assertions** and matching runtime hashes.
[Verification](VERIFICATION.md#local-rebuild-of-pulled-main---14-october-2026), [audit](APK-SIZE-AUDIT.md#local-rebuild-of-pulled-main---14-october-2026).
Evidence: `Builds/LocalRebuild/Task-20261014-195856/`. No physical-phone or WAN qualification is claimed.

## Golf buggy control and shot camera - 14 October 2026

Fresh AndroidSubmission at **18:54:34 Malaysia time** measures
**73,453,793 -> 73,446,301 bytes (-7,492)** against this task's opening APK.
Development reserve below 75,000,000 bytes is **1,553,699 bytes**; headroom to
the strict 100,000,000-byte ceiling is **26,553,699 bytes**. The enforced build
gate and independent APK v2 signature verification pass. SHA-256:
`E4BF71D7D11845B843582F145C0A46D773E2ABBA72B56205319FDE195BDC4DDE`.

Buggy steering and reverse preserve the player's camera angle. Accepted aiming
recalls the player's buggy until manually summoned again. Confirmed impact
starts a smooth ball-follow camera through flight and landing, then a smooth
return to the golfer. No new art, audio or shipped dependency was added.
Both mesh inventories are unchanged. The combined working-tree measurement
also includes concurrent sports prop-cycle and lighting work, so the full byte
delta is not attributable to the golf changes alone. See the
[packing audit](APK-SIZE-AUDIT.md#golf-buggy-control-and-shot-camera---14-october-2026).

The complete **324-file** Windows player built at **18:48:50** is delivered in
`Builds/WindowsFinal/`, protocol **55**, with every file hash-verified against
the tested candidate. [Verification](VERIFICATION.md#golf-buggy-control-and-shot-camera---14-october-2026)
records **632 passing assertions**, rendered camera checks and source limits.
This is `bd62f78` plus preserved uncommitted work. Physical Android appearance,
touch and performance remain unverified. Current evidence is in
`Builds/GolfCameraQA/`; its `SizeAuditFinal/` preserves the measured audit.

## Live renderer lighting fix - 14 October 2026

Fresh AndroidSubmission at **18:43:03 Malaysia time** measures
**73,453,793 -> 73,448,789 bytes (-5,004)** against this task's opening APK.
The development reserve is **1,551,211 bytes** below 75,000,000, with
**26,551,211 bytes** to the strict 100,000,000-byte ceiling. The enforced build
gate and independent APK v2 signature check pass. SHA-256:
`00F95C3C789CB3BCDF83E76A7AC8391AFABAA3EA7CAEE76B884574F356501EB5`.

Island lighting now controls the active camera's SSAO feature instead of a
bundled duplicate. The fix restores the intended Windows contact shading;
it does not reduce character textures, materials, geometry or normal shadows.
All four islands and lossless packaging remain. Both mesh inventories are
unchanged. This shared-checkout build also includes concurrent sports prop-cycle
and Golf camera work, so its byte delta is not an isolated lighting benchmark.
See the [audit](APK-SIZE-AUDIT.md#live-renderer-lighting-fix---14-october-2026).

The complete **324-file** Windows player built at **18:35:38** is delivered in
`Builds/WindowsFinal/`, protocol **55**. [Verification](VERIFICATION.md#live-renderer-lighting-fix---14-october-2026)
records **495 passing assertions**, four-island character comparisons and source
snapshot limits. A later development-only `GolfAimProbe.cs` update is outside
this Windows snapshot; delivered gameplay and lighting code match. Phone visual
quality/FPS remain unverified. Current evidence is in `Builds/CharacterLightingQA/`.

## Continuous sports prop spawning - 14 October 2026

Fresh AndroidSubmission at **18:40:11 Malaysia time** measures
**73,453,793 -> 73,444,625 bytes (-9,168)**. Development reserve below
75,000,000 bytes is **1,555,375 bytes**; headroom to the strict 100,000,000-byte
ceiling is **26,555,375 bytes**. The enforced build gate and APK v2 signature
verification pass. SHA-256:
`DA0F0EEB65B860B9E7E74C719FBF6619DEF3D26BFEBA76B33F79EF5CAF71D624`.

Both sports spawn the first pickup ten seconds after live play begins, show a
countdown only during the final five seconds, and schedule the next pickup ten
seconds after collection. An uncollected pickup and pending deadline survive
baskets, out-of-bounds inbounds, shot-clock restarts and football goals/kickoff.
Protocol **55** requires matching builds. One held slot and slower football
shots from the preceding release are retained.

The complete **324-file** Windows player built at **18:26:20** was promoted to
`Builds/WindowsFinal/` and hash-verified after **1,404 passing assertions**.
Source includes preserved uncommitted work on `bd62f78` and concurrent golf and
lighting work available at build time. Android includes later golf camera and
development-review edits; the tested sports implementation is unchanged.
See [source limits and checks](VERIFICATION.md#continuous-sports-prop-spawning---14-october-2026)
and [packing audit](APK-SIZE-AUDIT.md#continuous-sports-prop-spawning---14-october-2026).
No physical Android or WAN validation is claimed. Current evidence is in
`Builds/PropCycleQA/`, including a preserved `AndroidAudit/` snapshot.

## Persistent sports props and slower football shots - 14 October 2026

Fresh AndroidSubmission at **17:43:53 Malaysia time** measures
**73,446,145 -> 73,453,793 bytes (+7,648)**. It remains below the
75,000,000-byte development target with **1,546,207 bytes** reserved, and has
**26,546,207 bytes** of headroom to the strict 100,000,000-byte ceiling.
The enforced build gate and independent APK v2 signature check pass. SHA-256:
`18CA85CD5E90D740F9CC109C2B0810733AC953C48AB972D3DE48DEF33C674A7D`.

Both sports retain each player's unused single prop through goals and restarts;
another pickup replaces it, activation consumes it, and match end clears it.
Football shot launch speed is reduced by 20%; attached dribbling is unchanged.
Protocol **54** requires matching peers. Existing lossless island packaging and
art quality are preserved; serialized asset and mesh inventories are unchanged.
See the [packing audit](APK-SIZE-AUDIT.md#persistent-sports-props-and-slower-football-shots---14-october-2026).

The complete **323-file** Windows player built at **17:38:15** is delivered in
`Builds/WindowsFinal/`, with every file matching the tested candidate.
[Verification](VERIFICATION.md#persistent-sports-props-and-slower-football-shots---14-october-2026)
records **1,825 passing assertions**, source provenance and archive batches.
This is `bd62f78` plus uncommitted task changes. Physical Android quality,
loading, memory/FPS and WAN play remain unverified. Current evidence is in
`Builds/PropRetentionQA/` and `Builds/SizeAudit/latest/`.

## Lossless island packaging - 14 October 2026

Fresh AndroidSubmission at **16:38:21 Malaysia time** measures
**91,647,416 -> 73,446,145 bytes (-18,201,271; 19.86%)**. This achieves the
75,000,000-byte development target with **1,553,855 bytes** remaining. Headroom
to the strict 100,000,000-byte ceiling is **26,553,855 bytes**. Preserve that
reserve. The enforced build gate and independent APK v2 signature check pass.
SHA-256:
`B95490F5D3FFE178099FD8A26130CBE43B43D9F00172177858AC41EF3E4F54C9`.

All four islands remain embedded and playable offline. Shared scene content is
losslessly LZMA-packed; Android Java minification removes unused Java code.
No art resolution, geometry, animation, lighting or rendering setting was reduced.
The actual before/after APK payload comparison matches **2,022 mesh, texture and
animation objects across 823 asset/type groups**, including texture image bytes.
High-quality editable masters are retained. See [packaging](CONTENT-PACKAGING.md)
and the [detailed audit](APK-SIZE-AUDIT.md#lossless-island-packaging---14-october-2026).

First launch prepares a local LZ4 cache; this reduces APK size, not total installed
storage. Windows cold preparation and warm loading were measured separately.
All-island travel, sports gameplay and local host/guest checks are recorded in
[verification](VERIFICATION.md#lossless-island-packaging---14-october-2026).
No physical Android device is available: phone loading, quality, memory and FPS
remain unverified. Current evidence is in `Builds/PackagingQA/` and
`Builds/SizeAudit/latest/`. The complete Windows delivery includes the existing
controls and programmed bots, protocol **53**, on the preserved dirty `3a0456d`
working tree; it is not a clean commit snapshot.

## Basketball and football defense swipes - 14 October 2026

Fresh AndroidSubmission at **15:48:33 Malaysia time** measures
**91,634,440 -> 91,647,416 bytes (+12,976)**. Headroom to the strict
100,000,000-byte boundary is **8,352,584 bytes**. The 75,000,000-byte
development target remains exceeded by **16,647,416 bytes**; the existing
reserve deficit is unresolved. The enforced size gate and independent APK v2
signature verification pass. SHA-256:
`B5840A3429B40C6D251E2DC1CC9ABF33C4C907D6431E8A62551CAC1874D90B18`.

Adds basketball Guard/Steal/Block swipes, close guard effects, camera teammate
passing and manual team inbounds, plus football Pressure swipes and close
pressure tackles. The UI reuses existing artwork, fonts and geometry. No new
textures, audio, animation clips, meshes or packages are introduced. Protocol
**53** requires matching peers. All **1,788 packed mesh rows are unchanged**.
Growth is principally compiled gameplay code and metadata; see the
[packing audit](APK-SIZE-AUDIT.md#basketball-and-football-defense-swipes---14-october-2026).

The complete **322-file** Windows player, built at **15:43:27**, is delivered
in `Builds/WindowsFinal/`; its hashes match the tested candidate. See
[2,617 passing assertions and limits](VERIFICATION.md#basketball-and-football-defense-swipes---14-october-2026).
Current evidence is retained in `Builds/BasketballControlQA/` and
`Builds/SizeAudit/latest/`. This includes preserved uncommitted local sports
work on `main` `3a0456d`. Physical Android quality/FPS and WAN play are unverified.

## Programmed sports bots and training retirement - 14 October 2026

Fresh AndroidSubmission at **14:09:27 Malaysia time** measures
**91,700,774 -> 91,634,440 bytes (-66,334)**. Headroom to the strict
100,000,000-byte boundary is **8,365,560 bytes**. The 75,000,000-byte
development target remains exceeded by **16,634,440 bytes**; the reserve
deficit is unresolved. The enforced size gate and independent APK v2 signature
verification pass. SHA-256:
`63B343211DD71D6F732CBFE0F4CC6F486C44E8A57532A6A7966722D408923F97`.

Adds basketball programmed bots and solo/room setup for 2v2-5v5, retains the
casual football policy, and removes the learned training pipeline, ML-Agents,
Inference Engine and App UI. Protocol **52** requires matching builds.
No trained weights, new art or reduced visual quality are introduced.
All **1,788 packed mesh rows are unchanged**. The small APK reduction reflects
the previously stripped inference code/shaders and remaining package resources,
not a large model removal: no learned model had been shipped.

The complete **322-file** validated Windows player, built at **14:04:16**,
is delivered in `Builds/WindowsFinal/`; its hashes match the tested player.
See [1,692 passing assertions and limits](VERIFICATION.md#programmed-sports-bots-and-training-retirement---14-october-2026),
[packing audit](APK-SIZE-AUDIT.md#programmed-sports-bots-and-training-retirement---14-october-2026)
and [programmed bots](PROGRAMMED-BOTS.md). Evidence is retained under
`Builds/ProgrammedBotsQA/` and `Builds/SizeAudit/latest/`. This includes
pre-existing local sports work on `main` `3a0456d`, not a clean commit snapshot.
Physical Android quality/FPS and real WAN play remain unverified.

## Latest upstream Golf merge - 14 October 2026

Fast-forwarded `main` from `ca979d5` to `3a0456d`, including `cb0b6e2`.
Preserved local sports/AI work and protocol 51; combined both sides of release
records and retained the incoming Golf ball prefab with coherent object IDs.

Fresh AndroidSubmission at **01:16:04 Malaysia time** measures
**90,518,282 -> 91,700,774 bytes (+1,182,492)**. Headroom to the strict
100,000,000-byte boundary is **8,299,226 bytes**. The 75,000,000-byte
development target remains exceeded by **16,700,774 bytes**. The enforced
budget gate and independent APK v2 signature verification pass. SHA-256:
`4977FBE6105F4E7ADFD0A2B250FFF127C6C6A9431F238E9BA53698FD04867E52`.

Audit growth is primarily the five incoming flag textures (**315,116 serialized
bytes each**) and refined flagpole/cloth meshes (**+121,668 / +51,044 bytes**).
Serialized assets total **157,634,778 bytes**, up **1,789,632**; serialized
measurements are not compressed APK bytes. The largest compressed entries are
IL2CPP **11,837,737**, Unity **9,114,056**, managed metadata **2,689,497** and
`classes.dex` **2,356,543 bytes**. Preserve shared masters and the existing
mobile imports; the development reserve deficit remains unresolved.

Current audit is in `Builds/SizeAudit/latest/`. Before-build APK/audit and asset
comparison are retained in `Builds/PullMergeQA/`. This is a merged working-tree
build, including pre-existing uncommitted work, not a clean commit snapshot.
Physical Android appearance, FPS and WAN play are unverified. Concurrent
BasketballBot/BasketballBots scripts added after both builds are excluded from
this validated source snapshot; final readiness flags their missing metadata.

## Football radial pickup and reduced pace - 14 October 2026

Fresh AndroidSubmission built **00:48:57 Malaysia time** measures
**90,515,790 -> 90,518,282 bytes (+2,492)**. It passes the strict
100,000,000-byte limit with **9,481,718 bytes** to the boundary. The
**75,000,000-byte development target remains exceeded by 15,518,282 bytes**;
the existing reserve deficit is not resolved. APK v2 signature verification
passes. SHA-256:
`11E14C28F9E2065C473D62A0F7C52AB42F5671478693CFAF4794ABD3C2E70829`.

Includes 360-degree close football pickup, 70% human/bot locomotion and
protocol **51**, preserving the previously integrated sports/AI work.
No new art, packages or trained weights are shipped. Serialized asset bytes
and all **1,788 mesh rows** are unchanged; growth is compiled code/metadata
and package bookkeeping. See the [packing audit](APK-SIZE-AUDIT.md#football-radial-pickup-and-reduced-pace---14-october-2026)
and [803 passing gameplay assertions](VERIFICATION.md#football-radial-pickup-and-reduced-pace-14-october-2026).

The complete 344-file validated Windows player is in `Builds/WindowsFinal/`.
Evidence, hashes and archived-output manifests are in `Builds/FootballControlQA/`.
Training remains paused; no physical Android phone or WAN qualification was
performed. Windows views do not establish Android appearance or FPS.

## Training recovery and Fishing merge - 13 October 2026

Fresh AndroidSubmission at **22:44:55 Malaysia time** measures
**90,384,954 -> 90,515,790 bytes (+130,836)**, with **9,484,210 bytes** to
the strict 100,000,000-byte boundary. The **75,000,000-byte development
target is still exceeded by 15,515,790 bytes**. This is not a comfortable
development budget. APK v2 signature verification and the enforced limit pass.
SHA-256: `CA05A8BB6FB2820FC5FA478408F49716F03718AB64F4857375E5BD92799B6B01`.

Merged teammate `ca979d5` and preserved local sports/AI work; matching peers
now require protocol **50**. Incoming Fishing countersteer, skills/tutorial
art and swimming/splash work account for the asset changes. The recovery
trainer remains development-only, and no learned weights are shipped.
All 1,788 packed mesh rows are unchanged. See the [packing audit](APK-SIZE-AUDIT.md#training-recovery-and-fishing-merge---13-october-2026),
[verification](VERIFICATION.md#training-recovery-and-fishing-merge-13-october-2026)
and [one-hour training instructions](PROGRAMMED-BOTS.md#retired-training).

Complete Windows delivery and source/merge/build evidence are retained under
`Builds/WindowsFinal/` and `Builds/TrainingRepairQA/`. No Android device is
available; size/signature checks do not establish phone quality or FPS.

## Reference flags and turf bounces - 13 October 2026 build limitation

The measured opening APK is **90,334,226 bytes**. All three new AndroidSubmission
attempts failed in Gradle/JDK loopback initialization; the prior APK is retained
with its original hash. There is no successful after measurement for these
changes. Its historical headroom is **9,665,774 bytes**, and it exceeds the
75 MB development target by **15,334,226 bytes**. These values do not validate
the current reference flag textures or rebound code. Five derived flag maps use
mipmaps and Android ASTC 6x6; the full reference masters remain outside Unity.
Unused geometric numeral meshes and their material were archived. No global
rendering quality or unrelated dependency was reduced. Fresh packed APK
contributors cannot be measured until the new build succeeds. Windows checks
and complete delivery pass; see [verification](VERIFICATION.md#original-reference-flags-and-decaying-turf-bounces---13-october-2026).


## Golden Golf shot trail — 13 October 2026

Fresh primary AndroidSubmission completed at **21:25:01 Malaysia time**:
**89,230,290 → 90,334,226 bytes (+1,103,936)**. The opening root APK was the
stale **4 October** package, so this delta includes intervening game/art work
and is **not solely trail growth**. Strict-limit headroom is **9,665,774 bytes**.
The **75,000,000-byte development target remains exceeded by 15,334,226 bytes**.
The enforced budget gate and independent APK v2 signature pass. SHA-256:
**2382DFE7BEF6B1085664B5665D14F2F33B82AE4A5EA2376B9834AA6C19C82A06**.

The warm-gold, soft-edged ball trail shares one texture-free material, fades
from a strong head to a pale transparent tail, and expires each portion within
**1 second including fade**. No model, texture, audio or package is added by
this feature. Its shader/material/MonoScript add **6,504 serialized bytes** and
**3,439 estimated ZIP bytes**, separate from compiled native code and actual
APK size. See [packing audit](APK-SIZE-AUDIT.md#golden-golf-shot-trail--13-october-2026).

The complete **322-file** Windows player is delivered in `Builds/WindowsFinal/`
and retained in `Builds/GolfTrailQA/Release/Windows/`. Full content build:
**21:17:42**; final runtime/review scripts refresh: **21:21:50**. All
**288 assertions** pass: 85 rendered ball/physics/trail, 158 rendered Golf
offline, 21 host and 24 side-approach client. Existing physical shots, slope
settling, five cups, strokes and current numbered flag content remain.
Combined room protocol **47** adds the authoritative strike marker.
[Verification and source scope](VERIFICATION.md#golden-golf-shot-trail--13-october-2026).

The initial build request collided with another primary build. Its later
Gradle loopback initialization failed; the final primary retry succeeded using
process-local IPv4 preference and constrained JVM/Gradle workers. These settings
and temporary locations remain outside committed source. Phone appearance,
touch, FPS and WAN remain unverified.

Reviewed old APK/audit: `Legacy/20261013-211950-049-golf-gold-trail-old-apk/`
(13 files, 92.89 MB). Superseded Windows player:
`Legacy/20261013-212758-518-golf-gold-trail-windows/` (322 files, 596.94 MB).
Redundant candidate, isolated build wrapper and superseded initial review runs:
`Legacy/20261013-213030-065-golf-gold-trail-candidates/`
(393 files, 682.47 MB); final 288-check evidence remains current.
Exact current APK, audit, signatures, comparisons and validation evidence:
`Builds/GolfTrailQA/`. No Git staging, commit or push is performed for this task.

## Stronger Fishing motion and upstream Golf integration — 13 October 2026

Fresh AndroidSubmission written **19:47:57 Malaysia time** measures
**90,281,708 → 90,388,492 bytes (+106,784)**.
Headroom to the strict 100,000,000-byte boundary: **9,611,508 bytes**.
The **75,000,000-byte development target remains exceeded by 15,388,492 bytes**.
The enforced hard limit and independent APK v2 signature checks pass. SHA-256:
**18CC2A2F7E2354ADACAF585FCF7DCDD8F5257A61CC1EAE4D0557CDE91A1EE6EB**.

Hooked fish have stronger thrust and surface surges, with spring force, drag,
buoyancy and gravity retained. Smooth turns allow up to 200 degrees/second;
rendered travel retains its 12 m/s cap. Free fish swim continuous 3.3 × 4.2 m
loops inside each pier's casting range. Surface spray is taller and thicker;
wakes expand farther. The shared-material droplet pool remains capped at 48.
No new Fishing image, model, shader, audio or package is introduced. Earlier
catch assistance, countersteering, six skills/two favourites, tutorial, scoring
and round rules remain intact.

Merged upstream `9b4eb15`, including Golf countdown graphics, cart controls,
longer swing and integrated scenes. Incoming shared/network code was preserved;
the merge had no conflicts. Four islands, shared travel and protocol **49** remain.

Complete Windows player refreshed with **3,916 assertions
passing in ten reports**, plus the separately documented Golf cart limitation.
[Verification](FISHING-VERIFICATION.md#stronger-fishing-motion-and-upstream-golf-integration--13-october-2026),
[packing audit](APK-SIZE-AUDIT.md#stronger-fishing-motion-and-upstream-golf-integration--13-october-2026). Source/build/ZIP/signature/promotion
evidence: `Builds/FishingSurge/Task-20261013-190655/`. No new archive batch.

Physical-phone touch, appearance and sustained FPS, real WAN conditions and
celebrations remain unverified or deferred. The separate Golf cart suite still
has the **12 documented toe-direction failures** around 64 degrees; its other
209 checks and all three completion/exit markers pass. This existing limitation
is recorded in [the Golf cart notes](VERIFICATION.md#golf-cart-english-button-labels--12-october-2026).
Fishing splashes use a stylized force model, not a fluid solver.

## Fishing six skills, familiar tutorial and water contact — 13 October 2026

Fresh AndroidSubmission written **17:49:09 Malaysia time** measures
**90,186,840 → 90,281,708 bytes (+94,868)**.
Headroom to the strict 100,000,000-byte boundary: **9,718,292 bytes**.
The **75,000,000-byte development target remains exceeded by 15,281,708 bytes**.
Enforced hard limit and independent APK v2 signature checks pass. SHA-256:
**FEC057213DF9E1F1837E954107B37E7335068EBD2B7BB53F65966C1EF2E588CB**.

Adds [six skills and two favourites](FISHING-ABILITIES.md), illustrated bag,
chest and gear controls, and one shared four-page tutorial for first entry and
How to Play. Tutorial examples reuse actual icons, direction circles and
pull/Catch/Tension/duration widgets. Two small ASTC atlases replace text-heavy
gear controls. Native labels stay independent of image resolution. A limited
early winding assist makes longer catches about one to two seconds faster
(up to 2.5 seconds in the representative long-distance large-fish case).
Held Right text stays in the same layout as Left; clock/icon move up three
design units; redundant results copy is removed. Visible-body water contact
drives bounded droplets with inherited velocity and gravity.

Complete **322-file Windows player** in `Builds/WindowsFinal/`; all **321
runtime/support hashes** match the tested candidate. **3,629 checks across
six reports** pass. Full content completed **16:49:51**; final script refresh
**17:35:40**. Matching peers need **protocol 49**. The final refresh only updates
a development result assertion; gameplay/network evidence retains its exact
earlier script snapshot. [Verification](FISHING-VERIFICATION.md#fishing-six-skills-familiar-tutorial-and-water-contact--13-october-2026),
[packing details](APK-SIZE-AUDIT.md#fishing-six-skills-familiar-tutorial-and-water-contact--13-october-2026).
Physical phone touch/visuals/FPS, real WAN and celebrations remain deferred.

## Fishing hybrid gear and smooth force-driven fights — 13 October 2026

Fresh AndroidSubmission written at **14:50:32 Malaysia time** measures
**90,166,448 → 90,186,840 bytes (+20,392)**. Headroom to the strict
100,000,000-byte boundary is **9,813,160 bytes**. The **75,000,000-byte
development target remains exceeded by 15,186,840 bytes**. Enforced budget
and independent APK v2 signature checks pass. SHA-256:
**5921BF64BBF1E7042E702DB375B83D00AA72CCADAB14F1A440FD23ED22BE5391**.

Adds the approved [hybrid gear system](FISHING-ABILITIES.md): three favourites
before Ready, one held item, shared shore supplies with 22-second refills,
and six-second treasure salvage with a shared 28-second cooldown. Nine abilities
reuse existing sports meshes/materials/fonts. Single-line Left/Right/Release
captions fit inside their coloured circles; the timer is 12 units shorter.
Hooked fish now use host-owned inertia, thrust, tether forces, water drag,
neutral buoyancy and airborne gravity, with stable headings and smoothed,
capped presentation turns. Round/Cup/Crew scoring remains unchanged.

The complete **322-file Windows player** is in `Builds/WindowsFinal/`; all
**321 runtime/support hashes** match the tested candidate. **3,397 checks
across six reports** pass, including real guest gear requests and rendered
landscape/safe-area layouts. Full content completed **14:00:51**; final script
refresh completed **14:31:54**. Matching peers require **protocol 48**.
[Verification](FISHING-VERIFICATION.md#fishing-hybrid-gear-and-smooth-force-driven-fights--13-october-2026),
[packing audit](APK-SIZE-AUDIT.md#fishing-hybrid-gear-and-smooth-force-driven-fights--13-october-2026).
Physical-phone touch/visuals/FPS, real WAN and new celebrations remain deferred.
Older records below describe earlier deliveries.

## Illustrated Fishing controls and fight motion — 13 October 2026

Fresh AndroidSubmission written at **01:18:45 Malaysia time** measures
**90,107,796 → 90,166,448 bytes (+58,652)**. Headroom to the strict
100,000,000-byte boundary is **9,833,552 bytes**. The **75,000,000-byte
working target remains exceeded by 15,166,448 bytes**. The enforced limit and
independent APK v2 signature checks pass. SHA-256:
**66D80607B160AABD90619BA3A8292D6A9F21A3B280DB236A8EB08555CACCE0A1**.

One illustrated Left and one mirrored Right circle share aqua/gold states,
with native internal labels; the circular reel returns for Cast/Hook. A sliced
frame contains the turn guide and separate Catch/Tension bars. Catch timing is
slightly faster with more recovery room; fish dart/breach/wake motion and
planted torso/rod motion are stronger. Scoring, protocol 47, shared travel and
sports game rules remain unchanged. [Controls and asset workflow](FISHING-COUNTERSTEER.md).

The complete **322-file Windows player** is delivered in `Builds/WindowsFinal/`;
all **321 runtime/support hashes** match the tested candidate. **2,491 checks
across six reports** pass on the final scripts. Full content completed **00:56:55**,
final script refresh **01:05:00**. [Verification and limits](FISHING-VERIFICATION.md#illustrated-fishing-controls-and-fight-motion--13-october-2026),
[packing audit](APK-SIZE-AUDIT.md#illustrated-fishing-controls-and-fight-motion--13-october-2026).
Physical-phone visuals/touch/FPS, real WAN and new celebrations remain deferred.
Older records below describe earlier deliveries.

## Fishing Hold & Countersteer — 12 October 2026

Fresh AndroidSubmission written at **20:43:13 Malaysia time** measures
**90,139,324 → 90,107,796 bytes (-31,528)**. Headroom to the
strict 100,000,000-byte boundary is **9,892,204 bytes**.
The **75,000,000-byte development target remains exceeded by 15,107,796
bytes**. The enforced limit and independent APK v2 signature verification pass.
SHA-256: **9F2719FDD9AEA02A5467A7C1322B451E51AFFF915A7D261897AA0D0023B1E522**.

Implements [B — Hold & Countersteer](FISHING-COUNTERSTEER.md): hold the glowing
Left/Right control, switch against the fish's pull, and release to ease tension.
Sports typography/panels/press feedback replace the old frame and reel button;
walking controls are replaced during fights, and unavailable utilities fade.
Round/Cup/Crew scoring remains unchanged. Matching peers require **protocol 47**.

The complete **322-file** Windows player (321 runtime/support files plus its
build note) is delivered in `Builds/WindowsFinal/`. All runtime hashes match the
candidate. **2,442 assertions across six reports** pass on its final scripts:
Fishing offline/rendered layouts, host/guest catches/rematches, Fishing results/
feedback and Football/Basketball UI regressions. Full content build finished at
**20:18:48**, final script refresh at **20:27:02**.

[Verification and limits](FISHING-VERIFICATION.md#fishing-hold--countersteer--12-october-2026),
[packing comparison](APK-SIZE-AUDIT.md#fishing-hold--countersteer--12-october-2026).
The development target, physical-phone touch/visual/FPS, real WAN and new
celebrations remain unresolved/deferred. Older records below are historical.
## Teammate merge and training launcher (13 October 2026)

Fresh AndroidSubmission APK written at **17:31:11 Malaysia time**:
**90,242,332 -> 90,384,954 bytes (+142,622)**. Hard-boundary headroom:
**9,615,046 bytes**. The **75 MB development target remains exceeded by
15,384,954 bytes**. Budget enforcement and independent APK v2 signature
verification pass. SHA-256:
**01858A6A58EF24430126DF1DCDAD0AD5B19598C452F79B4A25535BE69E12C5FC**.

Combines teammate Golf/Fishing commit `9b4eb15` with the preserved local sports
rules, defense and AI work. Protocol is **49**. Adds a development-only skill
curriculum and [user-run local training launcher](PROGRAMMED-BOTS.md#retired-training);
trained weights remain under ignored `Builds/`, not in the APK or normal play.
The full-contract smoke model is **186,592 bytes**, separate from APK size.
Existing inference GPU/App UI exclusions remain in place. All **1,788 mesh
records** match; the incoming six countdown images account for nearly all new
serialized asset bytes. No art quality was reduced.

[Verification](VERIFICATION.md#teammate-merge-and-training-launcher-13-october-2026),
[packing audit](APK-SIZE-AUDIT.md#teammate-merge-and-training-launcher-13-october-2026).
No phone was available. Under the hard limit does not resolve the development
budget shortfall or establish phone FPS, touch quality or inference cost.

## Football training checkpoint (13 October 2026)

Fresh AndroidSubmission APK written at **16:25:08 Malaysia time**:
**90,151,502 -> 90,242,332 bytes (+90,830)**. Hard-boundary headroom:
**9,757,668 bytes**. The **75 MB development target remains exceeded by
15,242,332 bytes**. Budget enforcement and independent APK v2 signature
verification pass. SHA-256:
**2B44048F1BF45ACC546A2DA904E9B8569F893D97DFA8CF6AE01CE320ADAA3448**.

Adds selectable 2v2-5v5 football and a development-only ML-Agents bridge.
The learned prototype remains local under `Builds/`, not the normal opponent
or a shipped model. Its ONNX is 76,086 bytes; that is separate from package
overhead. Unused inference GPU shader variants and App UI player features are
excluded. All **1,788 mesh records** and existing art/audio sizes match.
[Checkpoint](PROGRAMMED-BOTS.md#retired-training),
[verification](VERIFICATION.md#football-training-checkpoint-13-october-2026),
[packing audit](APK-SIZE-AUDIT.md#football-training-checkpoint-13-october-2026).
No Android device was connected. This is within the hard limit, not a
comfortable development budget or proof of phone quality/FPS/inference cost.

## Casual football calibration (13 October 2026)

Fresh AndroidSubmission APK written at **14:48:39 Malaysia time**:
**90,152,442 -> 90,151,502 bytes (-940)**. Hard-boundary headroom:
**9,848,498 bytes**. The **75 MB development target remains exceeded by
15,151,502 bytes**. Budget enforcement and independent APK v2 signature
verification pass. SHA-256:
**7155D7C37CA0EA1D51DCD2D06A7FEC7B5864BB7CF2B3240A4033A0B6B74B17F3**.

Calibrates the programmed football policy without new art, audio, ML packages
or model weights. Serialized asset sizes and all **1,788 mesh records** match
the baseline. The small compressed-code decrease is not an asset optimization
or resolution of the development-size shortfall.
[Policy](PROGRAMMED-BOTS.md),
[verification](VERIFICATION.md#casual-football-calibration-13-october-2026),
[packing audit](APK-SIZE-AUDIT.md#casual-football-calibration-13-october-2026).
No Android device was connected; touch, visuals/FPS and AI runtime cost remain
unverified. AI training has not started.

## Football defense and tactical AI (13 October 2026)

Fresh AndroidSubmission APK written at **13:46:59 Malaysia time**:
**90,143,514 -> 90,152,442 bytes (+8,928)**. Hard-boundary headroom:
**9,847,558 bytes**. The **75 MB development target remains exceeded by
15,152,442 bytes**. Budget enforcement and independent APK v2 signature
verification pass. SHA-256:
**FA2B24E7549C796BD4480AA6CFE27BD8BF264ABCBDEEA469BECC2E44057ECD37**.

Adds assisted defense and tactical football bots using existing characters,
physics and abilities. No new art, audio, ML package or model weights. All
**1,788 mesh records** match; serialized assets add only **104 bytes** of script
metadata. [Scope and limits](PROGRAMMED-BOTS.md),
[verification](VERIFICATION.md#football-defense-and-tactical-ai-13-october-2026),
[packing audit](APK-SIZE-AUDIT.md#football-defense-and-tactical-ai-13-october-2026).
No Android device was connected. Phone controls, visuals/FPS, bot runtime cost
and WAN/Relay play remain unverified. This is below the hard limit, not a
comfortable development budget; the existing 75 MB shortfall remains open.

## Football AI foundation (13 October 2026)

Fresh AndroidSubmission APK written at **00:23:01 Malaysia time**:
**90,126,542 -> 90,143,514 bytes (+16,972)**. Hard-boundary headroom:
**9,856,486 bytes**. The **75 MB development target remains exceeded by
15,143,514 bytes**. Budget enforcement and independent APK v2 signature
verification pass. SHA-256:
**12C855C57F86D761D6BD43E39B2CD9CCF2E633FDF23CDFC5CD0AA4AB5FED97E2**.

Adds football baseline bots, Play vs AI and a development-only episode harness.
No new art, audio, ML dependencies or model weights. All **1,788 mesh records**
match the baseline. Serialized assets add **508 bytes**, separate from compressed
APK growth. [Scope and limits](PROGRAMMED-BOTS.md),
[verification](VERIFICATION.md#football-ai-foundation-13-october-2026),
[packing audit](APK-SIZE-AUDIT.md#football-ai-foundation-13-october-2026).
No Android device was connected. Phone controls, visuals/FPS, bot runtime cost
and WAN/Relay play remain unverified; the development-size shortfall is open.

## Room matches (12 October 2026)

Fresh AndroidSubmission APK written at **22:37:28 Malaysia time**:
**90,099,110 -> 90,126,542 bytes (+27,432)**. Hard-boundary headroom:
**9,873,458 bytes**. The **75 MB development target remains exceeded by
15,126,542 bytes**. Budget enforcement and independent APK v2 signature
verification pass. SHA-256:
**B22263175CBE5BAC232E341032444D73BAFC98EF11808CADBBE565DB7BA2C647**.

Adds authoritative basketball room rules, fixed attacking hoops and disconnect
cancellation in both sports. Reuses existing physics, team selection and UI art;
no new textures, meshes or audio. All **1,788 mesh records** match the baseline.
Serialized assets add **648 bytes**, separate from compressed APK growth.
[Rules](ROOM-MATCH-RULES.md), [verification](VERIFICATION.md#room-matches-12-october-2026),
[packing audit](APK-SIZE-AUDIT.md#room-matches-12-october-2026).
No Android device was connected. Phone touch, visuals/FPS, WAN latency and
the development-size shortfall remain unverified or unresolved.

## Hosted sports movement — 12 October 2026

Fresh AndroidSubmission APK written at **18:38:41 Malaysia time**:
**90,099,410 → 90,099,110 bytes (−300)**. Hard-boundary headroom:
**9,900,890 bytes**. The **75 MB development target remains exceeded by
15,099,110 bytes**. Budget enforcement and independent APK v2 signature
verification pass. SHA-256:
**4757624B75044FC54E7F7AA438924F22A52BFA265AB1D668360134B52636C5A1**.

The host's local football/basketball motor now runs once per rendered frame.
No gameplay art, sound or geometry was added. All existing packed asset sizes
and **1,788 imported mesh records** match; the new opt-in review script adds
only **112 serialized bytes** of script metadata. Its probe logic is excluded
from the release build. [Verification](VERIFICATION.md#hosted-sports-movement--12-october-2026),
[diagnosis](NETWORK-MOVEMENT.md), [packing audit](APK-SIZE-AUDIT.md#hosted-sports-movement--12-october-2026).
No Android device was connected. Phone touch, visuals/FPS and real WAN latency
remain unverified; the development-size shortfall remains open.

## Combined Football, Basketball and Fishing release — 12 October 2026

Fresh AndroidSubmission written at **18:42:22 Malaysia time** measures
**88,856,012 → 90,139,324 bytes (+1,283,312)** against the actual
Fishing-only APK present at the start of this publish task. Headroom to the
strict 100,000,000-byte boundary is **9,860,676 bytes**.
The **75,000,000-byte development target remains exceeded by 15,139,324 bytes**;
this is not comfortable room for further features. The enforced budget gate
and independent APK v2 signature verification pass. SHA-256:
**FB9AC84E23EA34EA34FEF05B9534D07BB1F3DD1C731BE8DD4E3909227D300979**.

Incoming commit `eaad195` brings Football's illustrated HUD, Team Turbo,
Portal Steal, solid Wall, enlarged pickup displays and approved audio, plus
Basketball's latest art, controls, props and audio. Its networking logic is
retained; independent Fishing feedback, results, counters and timestamped
Hook edges remain integrated. Combined **protocol 46** rejects older incompatible
room builds. Four islands, one activity per host room and shared travel remain.

The complete **322-file** Windows player is delivered in `Builds/WindowsFinal/`.
All **321 runtime/support hashes** match the tested candidate. Selected Windows
coverage passes **4,152 assertions across 22 reports**; later script refreshes
change guarded development reviews, not shipping gameplay. The full content
build finished at **18:00:21**; final scripts refresh at **18:29:53**.

[Verification and source limits](VERIFICATION.md#combined-football-basketball-and-fishing-release--12-october-2026),
[packing audit](APK-SIZE-AUDIT.md#combined-football-basketball-and-fishing-release--12-october-2026). Previous branch measurements below
remain historical records and do not validate this combined source. New Fishing
celebrations, physical-phone quality/FPS and real-world WAN qualification remain
deferred. Further base-game size reductions are still required.

## Approved football audio — 12 October 2026

Fresh AndroidSubmission APK written at **17:05:51 Malaysia time**:
**89,859,034 → 90,099,410 bytes (+240,376)**. Hard-boundary headroom:
**9,900,590 bytes**. The **75 MB development target remains exceeded by
15,099,410 bytes**. The enforced APK budget gate and independent APK v2
signature verification pass. SHA-256:
**B637556D028555FD185079575BA55D21A9B620E315F3B3283BFA6370F7D42323**.

80 approved sounds cover football, abilities, match events and buttons.
55 new mono Vorbis clips add **190,152 serialized bytes**; 25 basketball clips
are referenced in place. Existing packed asset sizes and all **1,788 imported
mesh records** remain unchanged. Editable sources stay outside Unity.
[Implementation and verification](FOOTBALL-AUDIO.md),
[packing audit](APK-SIZE-AUDIT.md#approved-football-audio--12-october-2026).
No Android device was connected; phone listening, touch, visuals and FPS remain
unverified. The existing development-target shortfall remains open.

## Physical football wall and larger spawns — 12 October 2026

Fresh AndroidSubmission APK written at **16:29:39 Malaysia time**:
**89,851,508 → 89,859,034 bytes (+7,526)**. Hard-boundary headroom:
**10,140,966 bytes**. The **75 MB development target remains exceeded by
14,859,034 bytes**. The enforced APK budget gate and independent APK v2
signature verification pass. SHA-256:
**D1A821242C0DB57BBFA560F91043F92064B0A495F9D580FE0ED198C41B268F70**.

Football walls now physically block both teams and the ball. Spawn-only models,
countdown digits and floor projections are four times larger; countdown digits
use solid dark blue. The change adds one **1,664-byte serialized** material and
small compiled code changes. Every existing packed asset size and all **1,788
imported mesh records** remain unchanged. No texture or geometry was enlarged.
[Verification](VERIFICATION.md#physical-football-wall-and-larger-spawn-displays--12-october-2026),
[packing audit](APK-SIZE-AUDIT.md#physical-football-wall-and-larger-spawns--12-october-2026).
No Android device was connected; phone touch, visuals and FPS remain unverified.

## Football Explore Offline props — 12 October 2026

Fresh AndroidSubmission APK written at **15:58:26 Malaysia time**:
**89,850,652 → 89,851,508 bytes (+856)**. Hard-boundary headroom:
**10,148,492 bytes**. The **75 MB development target remains exceeded by
14,851,508 bytes**. The enforced APK budget gate and independent APK v2
signature verification pass. SHA-256:
**08EBEEAC279E01C3F880A77AE4D31E48B5E53019B55C97079B560E7E674F725B**.

The existing football countdown, pickups and three abilities now also run in
Explore Offline without starting a match. No new art or package was added;
all packed asset sizes and 1,788 imported mesh records remain unchanged.
[Verification](VERIFICATION.md#football-props-in-explore-offline--12-october-2026),
[packing audit](APK-SIZE-AUDIT.md#football-explore-offline-props--12-october-2026).
No Android device was connected; phone touch, visuals and FPS remain unverified.

## Football abilities — 12 October 2026

Fresh AndroidSubmission APK written at **15:31:14 Malaysia time**:
**89,699,656 → 89,850,652 bytes (+150,996)**. Headroom to the strict
100,000,000-byte boundary: **10,149,348 bytes**. The **75 MB development
target remains exceeded by 14,850,652 bytes**. The enforced APK budget gate
and independent APK v2 signature verification pass. SHA-256:
**C93FED2FFC8B47CB282AA55BE290B333B0D0603DBEECC9995BBD0E81387CEEE9**.

Adds football Team Turbo, Portal Steal and a 15-second Wall, with two fixed
midfield touchline spawn points, blue countdown holograms and single-use HUD
buttons. Four combined model meshes, shared materials and one 512 × 128 ASTC
atlas provide distinct football artwork. Full-resolution concept art and SVG
masters remain outside Unity. No new runtime package, font, audio or world
texture was added. Existing art quality is preserved.

[Rules and implementation](FOOTBALL-ABILITIES.md),
[packing audit](APK-SIZE-AUDIT.md#football-abilities--12-october-2026),
[Windows verification](VERIFICATION.md#football-abilities--12-october-2026).
No Android phone is connected; physical touch, phone appearance/FPS and real
WAN latency remain unverified. The development size target is still open.

## 2D sports action icons — 12 October 2026

Fresh AndroidSubmission APK written at **14:46:53 Malaysia time**:
**89,701,824 → 89,699,656 bytes (−2,168)**. Headroom to the strict
100,000,000-byte boundary: **10,300,344 bytes**. The **75 MB development
target remains exceeded by 14,699,656 bytes**. The enforced budget gate and
independent APK v2 signature verification pass. SHA-256:
**943DAB202360DCBAFCA375E103282CB9045E2DC3940597862A980886FB463664**.

Football Tackle and Dash use simple colourful 2D player poses. Shared Jump
uses a matching leaping figure in both sports. The existing atlas dimensions,
ASTC imports, fonts and every packed asset size remain unchanged. All
**1,788 mesh records** match; full-resolution masters stay outside Unity.

[Design and sources](SportsUI/README.md),
[packing audit](APK-SIZE-AUDIT.md#2d-sports-action-icons--12-october-2026),
[Windows delivery](VERIFICATION.md#2d-sports-action-icons--12-october-2026).
No Android phone was connected; physical touch, phone appearance and FPS remain
unverified. The development target remains an open project requirement.

## Full-colour sports artwork — 12 October 2026

Fresh AndroidSubmission APK written at **02:00:19 Malaysia time**:
**89,644,388 → 89,701,824 bytes (+57,436)**. Headroom to the strict
100,000,000-byte boundary: **10,298,176 bytes**. The **75 MB development
target remains exceeded by 14,701,824 bytes**. The enforced budget gate and
independent APK v2 signature verification pass. SHA-256:
**1D3FF392479D772C1F15A283774F7B7E695C5078B9D864D0CEE67A6BD94C727B**.

Ten full-colour cartoon masters replace the monochrome gameplay glyphs.
Utility and prop icons also have colour, depth and highlights. Atlas dimensions,
ASTC imports, font subsets and all packed asset sizes remain unchanged;
high-resolution masters stay outside Unity. All **1,788 mesh records** match.
The additional APK bytes mostly reflect richer compressed texture content.

[Design and regeneration](SportsUI/README.md),
[packing audit](APK-SIZE-AUDIT.md#full-colour-sports-artwork--12-october-2026),
[Windows delivery](VERIFICATION.md#full-colour-sports-artwork--12-october-2026).
No Android phone was connected; phone appearance, physical touch and FPS remain
unverified. The development target remains an open project requirement.

## Sports icons and typography — 12 October 2026

Fresh AndroidSubmission APK written at **01:10:55 Malaysia time**:
**89,589,994 → 89,644,388 bytes (+54,394)**. Headroom to the strict
100,000,000-byte boundary: **10,355,612 bytes**. The **75 MB development
target remains exceeded by 14,644,388 bytes**. The enforced budget gate and
independent APK v2 signature verification pass. SHA-256:
**B712BAD3BB76BA8CAA1AC3691A0871B23172335DD45854813406AA6E412C3EA2**.

Both sports now use solid rounded pictograms and compact Manrope subsets.
The existing two icon atlases retain their dimensions; the shared prop/check
atlas is 512 × 128. Existing world textures, audio, animations and all
**1,788 mesh records** remain unchanged. The full font master and SVG masters
stay editable outside Unity. Licenses ship with the player and APK.

[Design and regeneration](SportsUI/README.md),
[packing audit](APK-SIZE-AUDIT.md#sports-icons-and-typography--12-october-2026),
[Windows delivery](VERIFICATION.md#sports-icons-and-typography--12-october-2026).
No Android phone was connected; phone appearance, physical touch and FPS remain
unverified. The fresh APK does not remove the need to reach the development target.

## Football touchline UI — 12 October 2026

Fresh AndroidSubmission APK written at **00:27:42 Malaysia time**:
**89,542,916 → 89,589,994 bytes (+47,078)**. Headroom to the strict
100,000,000-byte boundary: **10,410,006 bytes**. The **75 MB development
target remains exceeded by 14,589,994 bytes**. The enforced size gate and
independent APK v2 signature verification pass. SHA-256:
**8D0FE4DF010532254F5847485979D3EA80FC762772F823C2EA8C71ABF8412C03**.

The approved football controls, same-finger release-only Cancel, attached
power ring, regular settings gear, camera tiles and illustrated guide are
implemented. One 512 × 256 ASTC atlas adds **59,288 serialized bytes**;
existing fonts and utility icons are reused. The actual compressed icon
entry is **26,739 bytes**. All **1,788 mesh records** and existing world
texture, audio and animation records are unchanged. Protocol **43** remains.

[Implementation and checks](FootballUI/implementation.md),
[packing audit](APK-SIZE-AUDIT.md#football-touchline-ui--12-october-2026),
[Windows delivery](VERIFICATION.md#football-touchline-ui--12-october-2026).
No Android phone was connected; physical touch, phone appearance/FPS and WAN
qualification remain unverified.

## Fishing feedback, timing and round recap — 11 October 2026

Fresh AndroidSubmission APK written at **21:16:21 Malaysia time**:
**88,812,118 → 88,856,012 bytes (+43,894)**. Headroom to the strict
100,000,000-byte boundary is **11,143,988 bytes**. The **75,000,000-byte
development target remains exceeded by 13,856,012 bytes**. The hard budget
gate and independent APK v2 signature verification pass. SHA-256:
**8766DC73759F316754896D4CF0E11AA2D1F3290A688C593CC3DC5853824749C7**.

Fishing adds accepted-action sound, optional vibration off by default, clearer
target/failure labels, bounded remote Hook receipt tolerance, per-round recap
counters, a closable results card and a live Crew checklist. Existing compressed
art and fonts are shared. Short mono sounds are generated at runtime; no
imported texture, mesh, animation, audio or package is added. Scores, reeling
difficulty, four islands and shared travel retain their behavior. Online peers
require matching **protocol 39** builds.

The complete **318-file** Windows player is delivered in `Builds/WindowsFinal/`:
a full content build at **15:12:39**, followed by a validated scripts-only refresh
at **21:06:07**. All **317 runtime/support hashes** match the tested candidate.
Final Fishing checks pass **1,245 assertions across four reports**. Earlier
applicable shared HUD, menu and Golf reports pass another **912 assertions**;
their shared behavior was unchanged by the final Fishing copy/padding fixes.

[Gameplay and controls](FISHING-MINIGAME.md),
[packing audit](APK-SIZE-AUDIT.md#fishing-feedback-timing-and-round-recap--11-october-2026),
[verification and source limits](VERIFICATION.md#fishing-feedback-timing-and-round-recap--11-october-2026).
New celebrations and physical-phone qualification remain deferred. Windows
checks do not establish phone touch, sound/vibration quality or sustained FPS.
The development size target still requires further reductions.

## Basketball gameplay audio — 11 October 2026

Fresh AndroidSubmission APK written at **19:19:05 Malaysia time**:
**89,202,254 → 89,542,916 bytes (+340,662)**. Headroom to the strict
100,000,000-byte boundary: **10,457,084 bytes**. The **75 MB development
target remains exceeded by 14,542,916 bytes**. The enforced size gate and
independent APK v2 signature verification pass. SHA-256:
**E4704628F47E0CEFF0CAAC4B03055D4BFE648D2EEC77501D012E90F55F6EB7D9**.

This release integrates the approved 58-clip basketball bank, lowers the
two-second Lock-on aim by 7.96 dB and reduces its pitch, synchronizes physical
contacts/props/movement/UI, and adds four volume controls. Protocol **43**
requires matching room builds. Selected audio occupies **283,696 serialized
bytes** using mono 32 kHz Vorbis; raw packs and audition alternatives stay
outside Unity. All **1,788 mesh records** match the baseline; no world texture
or animation is degraded. No unexplained bulk growth was found.

[Packing audit](APK-SIZE-AUDIT.md#basketball-gameplay-audio--11-october-2026),
[Windows delivery and verification](VERIFICATION.md#basketball-gameplay-audio--11-october-2026),
[audio mapping and sources](BASKETBALL-AUDIO.md).
No Android device was connected. Phone audio latency/speakers, appearance,
touch and FPS remain unverified; final gameplay listening is for user review.

## Basketball inbound and touch fixes — 11 October 2026

Fresh AndroidSubmission APK written at **18:34:40 Malaysia time**:
**89,200,326 → 89,202,254 bytes (+1,928)**. Headroom to the strict
100,000,000-byte boundary: **10,797,746 bytes**. The **75 MB development
target remains exceeded by 14,202,254 bytes**. The enforced size gate and
independent APK v2 signature verification pass. SHA-256:
**13AA2D4A5D6AD29193677C45B58268398AB352CE9C354A119B3109133C266086**.

This release adds a regular eight-tooth gear, court-line containment, two-second
solo inbound restarts, release-only Cancel, 0.4-second pass charging and 1.5×
horizontal pass velocity. Faster bounce passes retain their catch window after
the planned rebound. Protocol **42** requires matching room builds.

The existing 512-pixel ASTC icon atlas remains **118,456 serialized bytes**.
All **1,788 mesh records** match; no world texture, animation or audio is degraded.
[Packing audit](APK-SIZE-AUDIT.md#basketball-inbound-and-touch-fixes--11-october-2026),
[verification and Windows delivery](VERIFICATION.md#basketball-inbound-and-touch-fixes--11-october-2026).
Physical Android touch latency, appearance, reach and FPS remain unverified;
ADB reported no connected device.

## Basketball mobile controls — 11 October 2026

Fresh AndroidSubmission APK written at **17:24:14 Malaysia time**:
**89,116,652 → 89,200,326 bytes (+83,674)** from the recorded
start-of-task APK. The camera release delivered during this task was
89,178,578 bytes; this final build is **21,748 bytes** larger.
The initial-to-final increase includes that concurrent release.

Headroom to the strict 100,000,000-byte boundary: **10,799,674 bytes**.
The **75 MB development target remains exceeded by 14,200,326
bytes**. The enforced budget gate and independent APK v2 signature check pass.
SHA-256: **25EC7AD13D4D565D1DF05108109185CB7608931491008CACA721445885AB5555**.

The approved mobile buttons, same-finger drag paths, attached timing arcs,
camera/settings pages and illustrated guide are implemented. One 512-pixel
ASTC 6×6 atlas uses the approved SVG masters; existing fonts and procedural
frames are reused. No model, world texture, animation or audio was degraded.
Protocol **41** and the existing props / ball-first camera are retained.

[Packing audit](APK-SIZE-AUDIT.md#basketball-mobile-controls--11-october-2026),
[implementation](BasketballUI/README.md),
[Windows delivery and verification](VERIFICATION.md#basketball-mobile-controls--11-october-2026).
Physical Android appearance, reach, touch latency/FPS and WAN qualification remain unverified.

## Ball-first Stadium camera — 11 October 2026

Fresh AndroidSubmission finished at **16:57:15 Malaysia time**. Actual APK:
**89,116,652 → 89,178,578 bytes (+61,926)**. Headroom to the strict
100,000,000-byte boundary: **10,821,422 bytes**. The **75 MB development target
remains exceeded by 14,178,578 bytes**. The enforced size gate and independent
APK v2 signature verification pass. SHA-256:
**83A0A64C50E597DB00B9B6476865309460F03861E30DC09E8931A0AEF1C90009**.

The camera and edge marker use code and procedural UI geometry. Most growth
comes from the concurrent basketball icon sheet already present during the
validated Windows build. Later, incomplete basketball UI source edits blocked
the shared workspace's Android compile. The release was therefore built using
`Tools/Build/Build.ps1 -Target AndroidSubmission` in an ignored snapshot matching
all **3,304 Game inputs** of the tested Windows source. Those later edits remain
untouched and are excluded from this release. Protocol **41** is unchanged.

[Packing audit](APK-SIZE-AUDIT.md#ball-first-stadium-camera--11-october-2026),
[Windows delivery and verification](VERIFICATION.md#ball-first-stadium-camera--11-october-2026),
[camera behaviour](STADIUM-CAMERA.md).
Physical Android appearance, touch/FPS and WAN qualification remain unverified.

## Centre-line prop countdown — 11 October 2026

Fresh AndroidSubmission finished at **15:56:46 Malaysia time**. Actual APK:
**89,113,480 → 89,116,652 bytes (+3,172)**. Strict 100,000,000-byte boundary
headroom: **10,883,348 bytes**. The **75 MB development target remains exceeded
by 14,116,652 bytes**. The enforced size gate and independent APK v2 signature
verification pass. SHA-256:
**80351DE9BCB091279A5FD0BC225267917F2B241FED38517C24318380EADF8240**.

One pickup chooses a fixed left/right centre-line location, excludes the previous
prop type, and appears after a translucent blue 3D five-second countdown.
Tiny procedural digit meshes reuse the prop effect material. No new textures,
fonts, art packages or dependencies ship. Protocol **41** is required on all peers.

[Packing audit](APK-SIZE-AUDIT.md#centre-line-prop-countdown--11-october-2026),
[Windows delivery and verification](VERIFICATION.md#centre-line-prop-countdown--11-october-2026).
Physical Android appearance, touch/FPS and WAN qualification remain unverified.

## Visible basketball props and ability tuning — 11 October 2026

Fresh AndroidSubmission finished at **14:07:11 Malaysia time**. Actual APK:
**89,109,564 → 89,113,480 bytes (+3,916)**. Headroom to the strict
100,000,000-byte boundary: **10,886,520 bytes**. The **75,000,000-byte working
target remains exceeded by 14,113,480 bytes**. The enforced size gate and
independent APK v2 signature check pass. SHA-256:
**890C714E6F9750024132AC2269C51CBC76D61C2E3C5CACE646DCFC1E796CD860**.

Spawned props preserve imported FBX scale, Team Turbo is **1.5× normal movement
for five seconds**, and Lock-on aims for **two seconds**. No art is added;
all serialized asset sizes and mesh audit rows are unchanged. Protocol **40**
prevents mixing players with older movement rules.

[Packing audit](APK-SIZE-AUDIT.md#visible-basketball-props-and-ability-tuning--11-october-2026),
[Windows delivery and verification](VERIFICATION.md#visible-basketball-props-and-ability-tuning--11-october-2026).
Physical-phone rendering, touch/FPS and WAN qualification remain unverified.

## Basketball props — 11 October 2026

Fresh AndroidSubmission finished at **13:22:20 Malaysia time**. Actual local
APK: **88,560,698 → 89,109,564 bytes (+548,866)**. Headroom to the strict
100,000,000-byte boundary is **10,890,436 bytes**. The **75,000,000-byte
working target is still exceeded by 14,109,564 bytes**. The enforced size gate
and independent APK v2 signature verification pass. SHA-256:
**E6505135809CC247834A609CEE6B29E0599B1E39DFF280378E808145C97A08DB**.

The local pre-task APK was an older protocol-37 delivery. Current source already
contained newer Fishing/Golf HUD work; its documented prior release was
88,812,118 bytes. The new APK is 297,446 bytes larger than that documented
release. The full local before/after increase must not be attributed only to props.

Three reusable meshes, three 256-pixel icons, shared materials and a procedural
portal shader add 374,449 serialized asset bytes, a separate measurement from
compressed APK growth. Editable masters and the approved concept stay outside
Unity. Protocol **39** is required on all peers.

[Ability rules and implementation](BASKETBALL-PROPS.md),
[packing audit](APK-SIZE-AUDIT.md#basketball-props--11-october-2026),
[Windows delivery and verification](VERIFICATION.md#basketball-props--11-october-2026).
Physical Android touch, appearance/FPS and WAN qualification remain unverified.

## Golf sticker HUD — 10 October 2026

Fresh AndroidSubmission APK written at **14:31:19 Malaysia time**:
**88,760,152 → 88,812,118 bytes (+51,966)**. Headroom to the strict
100,000,000-byte boundary is **11,187,882 bytes**. The **75,000,000-byte
development target remains exceeded by 13,812,118 bytes**. The hard budget
gate and independent APK v2 signature verification pass. SHA-256:
**7C8B2CEAAC5CD83F8BEBB061428876A617311C644F9504E71A0064E189D210BF**.

Golf combines A's illustrated cream/navy/gold controls, C's scorecard column
lines and B's framed hold/release hint with **!**. Native fonts, frames,
portraits, clock and utility icons reuse Fishing resources. Only two compressed
sprites are new: **256 × 256** and **128 × 128**, with **37,328 raw ASTC 6×6
bytes** and **39,116 serialized bytes** together. These are different
measurements from actual APK growth. Full-quality masters remain outside Unity.
Five-hole rules, ranking, final countdown, physics, input handlers and world
animations retain their existing behavior.

The complete **318-file** Windows player in `Builds/WindowsFinal/` was fully
built at **14:37:46** and validated before promotion. All **317 runtime/support
files** match the tested candidate and independent portable copy. Selected
final checks pass **1,549 assertions across ten reports**, plus 56 portable
launcher assertions. The eight Golf reports account for **516** assertions.

[Presentation and editable layout](GOLF-UI.md),
[packing audit](APK-SIZE-AUDIT.md#golf-sticker-hud--10-october-2026),
[verification and delivery](VERIFICATION.md#golf-sticker-hud--10-october-2026).
Physical-phone appearance, touch/FPS and separate-network qualification remain
unverified. The development size target still requires further reductions.

## Fishing timer spacing — 9 October 2026

Fresh AndroidSubmission finished at **23:31:12 Malaysia time**:
**88,758,740 → 88,760,152 bytes (+1,412)**. Strict 100,000,000-byte boundary
headroom: **11,239,848 bytes**. The 75 MB development target remains
exceeded by **13,760,152 bytes**. Budget enforcement and independent APK
v2 signature verification pass. SHA-256: **3E859930C7FCEB8DBCCE131F6B6CD95FAE11521E5543D6827E9D3197E2299928**.

The outside timer card is four canvas units shorter (**170 → 166**), and the
digits sit four units lower. The clock icon keeps its **76 × 83** size and
absolute top placement. The Wanted row stays in place, with less bottom
padding; its position is editable in the shared layout asset. The user's
existing Cancel height **70** is preserved. No art or package is added.
[Manual layout settings](FISHING-HOOK-CUE.md#manually-adjust-the-timer).

A complete full Windows player finished at **23:22:29** and is
delivered in `Builds/WindowsFinal/` (**318 files**, including 317 runtime/support
files and its note). Final rendered fishing checks pass **876 assertions**.
Protocol **38**, four islands and shared travel remain.
[Evidence and limits](VERIFICATION.md#fishing-timer-spacing--9-october-2026), [packing audit](APK-SIZE-AUDIT.md#fishing-timer-spacing--9-october-2026).
The 75 MB shortfall and physical-phone qualification remain unresolved.

## Fishing Hook cue and readable controls — 9 October 2026

Fresh AndroidSubmission completed at **22:36:40 Malaysia time**:
**88,742,452 → 88,758,740 bytes (+16,288)**. Headroom to the strict
100,000,000-byte boundary is **11,241,260 bytes**. The **75,000,000-byte
development target remains exceeded by 13,758,740 bytes**. The hard budget
gate and independent APK v2 signature verification pass. SHA-256:
**D2BC0DA872FDCF492EFCEC0A2E9D976E78D703CDE91619A693E4064E9986A77C**.

A gold **!** marks the local biting fish, the illustrated **HOOK!** control
pulses, and its shrinking rim follows the actual 1.8-second deadline. Scores
are navy, the local row is blue, Cancel is red with an X, and Camera/Jump use
narrower rounder icon controls. Wanted details fit one line with a large
yellow/navy **+3**; catch awards float as orange text without boxes. Heavier
existing name lettering, stronger gauge outlines and a plain title improve
readability. [Controls and manual timer tuning](FISHING-HOOK-CUE.md).

No texture, font, mesh, animation, audio, shader or package is added. Native
graphics reuse the existing UI material and responsive frames. Protocol **38**,
four islands and shared room travel remain. [Packing audit](APK-SIZE-AUDIT.md#fishing-hook-cue-and-readable-controls--9-october-2026).

The complete **318-file** Windows player in `Builds/WindowsFinal/` was fully
built at **22:07:12** and validated before promotion. All **317 runtime/
support files** match the tested candidate and portable copy; the note is
excluded. Final checks pass **1,061 assertions across 8 reports**.
[Verification, source differences and archives](VERIFICATION.md#fishing-hook-cue-and-readable-controls--9-october-2026).
Evidence: `Builds/FishingRippleQA/Task-20261009-162733/`.

This comparison uses the actual pre-task **88,742,452-byte** integrated-main
APK. Intermediate builds are retained as historical evidence and are not used
to validate the final typography. Physical Android rendering, touch/FPS and
separate-network play remain unverified. The 75 MB shortfall remains unresolved.

## Integrated main — 9 October 2026

Fresh AndroidSubmission completed at **15:25:34 Malaysia time**:
**90,051,054 → 88,742,452 bytes (−1,308,602)**. Headroom to the strict
100,000,000-byte boundary is **11,257,548 bytes**. The **75,000,000-byte
development target remains exceeded by 13,742,452 bytes**. Budget enforcement
and independent APK v2 signature verification pass. SHA-256:
**C7271C20F50308032F62CA6E2CE85169B467F5C72998EE05961D115B296B8A45**.

The combined source retains centre-reticle fishing, six fish per pier, clearer
Lagoon Stickers lettering and tuned reeling, alongside teammate football
pace/stamina/pressure, possession controls, sports maps, animation compression
and Golf Aim/Cancel, cycling power and the physical shot guide. Matching peers
require **protocol 38**. Four islands, shared room travel and editable art
masters remain. [Packing audit](APK-SIZE-AUDIT.md#integrated-main--9-october-2026).

A **complete full Windows build**, finished at **15:12:50**, is delivered in
`Builds/WindowsFinal/`: **318 files**, including **317 runtime/support files**
and its delivery note. Selected checks pass **1,504 assertions across 18
reports**, plus a separate five-player room integration run. Earlier short
Golf timing/distance observations failed under concurrent test load; identical
player repeats passed, and the failed evidence remains an explicit limit.
[Checks, source differences and cleanup](VERIFICATION.md#integrated-main--9-october-2026).
Evidence: `Builds/Publish/20261009-131113/`.

This comparison uses the actual local pre-merge fishing APK (**90,051,054
bytes**, protocol 35). The teammate branch's **88,560,698-byte** protocol 37
APK was a parallel branch delivery; its original records are retained below.
Neither old APK is used as validation of the combined build. Physical Android
visuals, touch/FPS and separate-network play remain unverified. The 75 MB
shortfall remains unresolved; the reserve still requires deliberate optimization.

## Fishing aim and readability — 9 October 2026

**Historical local fishing branch build (protocol 35), before main integration.**

Fresh AndroidSubmission completed at **10:58:13 Malaysia time**:
**90,047,202 → 90,051,054 bytes (+3,852)**. Headroom to the strict
100,000,000-byte boundary is **9,948,946 bytes**. The **75,000,000-byte
development target remains exceeded by 15,051,054 bytes**. The build budget
gate and independent APK v2 signature verification pass. SHA-256:
**BAA48E2531C388F8B1F0897E3CCD909C2A87EF17F52B68292387B086F420B1ED**.

The fishing HUD now uses clearer native lettering and a centre aiming reticle
with explicit Cast confirmation. Each of the five piers has six swimming fish,
reusing the existing meshes, materials and LODs. Reeling is modestly faster,
held tension grows more slowly and bites arrive 0.3 seconds sooner. No font,
texture, mesh, animation, audio or package is added. Serialized assets grow
only **24 bytes** for the editable aiming layout; APK growth is predominantly
compiled code. [Packing audit](APK-SIZE-AUDIT.md#fishing-aim-and-readability--9-october-2026).

The complete **314-file** Windows player is delivered in `Builds/WindowsFinal/`:
full assets built at **10:26:10**, final scripts compiled at **10:38:43**.
All **313 runtime/support files** match the tested candidate and portable copy;
the delivery note is excluded from that comparison. Matching peers require
protocol **35**. Final rendered fishing, walking, five-player room and portable
network checks pass. Evidence: `Builds/FishingAimQA/` and the runs linked in
[verification](VERIFICATION.md#fishing-aim-and-readability--9-october-2026).
Physical Android appearance, touch/FPS and internet latency remain untested.
The 75 MB target shortfall is unresolved; the remaining reserve is constrained.

## Lagoon Stickers fishing — 9 October 2026

Fresh AndroidSubmission completed at **01:49:38 Malaysia time**:
**89,938,934 → 90,047,202 bytes (+108,268)**. Strict-limit headroom is
**9,952,798 bytes**. The **75,000,000-byte development target remains exceeded
by 15,047,202 bytes**; this is a constrained reserve. The build budget gate
and independent APK v2 signature verification pass. SHA-256:
**01DDD02FC436ADA6B1E36B8BA0DDA9E8B82E2638DB2D6CDDAA6E097A639A98B0**.

This release adds explicit fish selection, spring/drag reeling, three-minute
Round, three-round Cup, Crew Catch (20 × starting players), Wanted bonuses and
the approved animated Lagoon Stickers HUD. Existing fish, rods, rigs and
islands are reused. Six illustrated sprites use 64/128/256-pixel Android
imports with ASTC 6×6; their texture blocks total **84,336 bytes** before APK
compression. Editable full-resolution sources are retained. No art pack,
runtime package, mesh, animation or audio is added.

The complete **314-file** Windows player is in `Builds/WindowsFinal/`, with
full assets built at **01:25:59** and final scripts compiled at **01:37:24**.
Protocol **34** is required for matching peers. Current fishing, walking,
five-player room and portable-launch checks pass. Physical Android visuals,
touch/FPS and internet latency remain untested. [Verification](VERIFICATION.md#lagoon-stickers-fishing--9-october-2026),
[packing audit](APK-SIZE-AUDIT.md#lagoon-stickers-fishing--9-october-2026).
Current evidence is under `Builds/FishingUXQA/` and `Builds/FishingGameplayQA/`.

## Golf cycling power and accurate shot guide — 9 October 2026

**Historical teammate branch build (protocol 37), before fishing integration.**

Fresh AndroidSubmission completed at **01:14:05 Malaysia time**:
**88,554,742 → 88,560,698 bytes (+5,956)**.
Strict-limit headroom is **11,439,302 bytes**. The
**75,000,000-byte development target remains exceeded by 13,560,698
bytes**. Budget enforcement and independent APK v2 signature verification pass.
SHA-256: **F0599E23425ADC27CAF3BCFF4F7249809D9B53E914C910BE8738B203F8FDA259**.

Power cycles continuously while held. The host automatically selects Putt near
the ball owner's cup and Swing farther away. A continuous mint/ivory path ends
at a gold glow: first touchdown for swings, stopping point for putts. Preview
and authoritative travel share the terrain-aware trajectory. Landing slowdown
keeps full shots bounded. No terrain, texture, model, audio or package was added.
The existing guide shader/material is reused, and the old readout is removed.
[Controls](GOLF-MINIGAME.md#controls-and-display),
[packing audit](APK-SIZE-AUDIT.md#golf-cycling-power-and-accurate-shot-guide--9-october-2026).

The complete **317-file** Windows delivery contains **316 runtime/support files**
verified against the portable test copy. Full desktop assets and scripts built at **01:22:22**. Verification totals **1,137 passing assertions
in 14 selected reports**, plus **13,122 unchanged base-terrain samples**.
Matching **protocol 37** peers required. Obsolete players and backups are in
Legacy; equipment/rod review launchers now use the complete current player.
[Evidence and cleanup](VERIFICATION.md#golf-cycling-power-and-accurate-shot-guide--9-october-2026).
No Android phone was attached; phone visuals, touch and FPS remain unverified.

## Golf aiming lock and shot guide — 8 October 2026

Fresh AndroidSubmission completed at **21:24:55 Malaysia time**:
**88,531,618 → 88,554,742 bytes (+23,124)**. Strict-limit headroom is
**11,445,258 bytes**. The **75,000,000-byte development target remains exceeded
by 13,554,742 bytes**. Budget enforcement and independent APK v2 signing pass.
SHA-256: **BFE19E496E6564D1284102CC04F701608B9645D7BE7F723B41BB681243E65B02**.

Aim now takes a safe stance within three metres, locks movement and supports
explicit Cancel. A procedural ivory/gold guide shows launch and estimated rollout.
Existing island terrain and original art remain unchanged. One reusable runtime
mesh, one shader and one material; no imported texture, model, animation or audio.
[Controls](GOLF-MINIGAME.md#controls-and-display),
[packing audit](APK-SIZE-AUDIT.md#golf-aiming-lock-and-shot-guide--8-october-2026).

The complete **316-file** Windows player is delivered in `Builds/WindowsFinal/`,
built at **21:17:53**. All **315 runtime/support files** match the tested portable
copy. Selected verification totals **811 passing assertions in 12 reports**,
plus 13,122 unchanged base-terrain samples. Matching **protocol 36** peers required.
[Evidence and archives](VERIFICATION.md#golf-aiming-lock-and-shot-guide--8-october-2026).
No physical Android device was attached; phone quality/FPS/touch remain unverified.

## Golf playability and integrated greens — 8 October 2026

Fresh AndroidSubmission completed at **19:48:25 Malaysia time**:
**88,529,938 → 88,531,618 bytes (+1,680)**. Strict-limit headroom is
**11,468,382 bytes**. The **75,000,000-byte development target remains exceeded
by 13,531,618 bytes**. Budget enforcement and independent APK v2 signing pass.
SHA-256: **1DC92C06D4E11F8A7944A23CB08E586F6309F7805B11F9941E1CD765418E8CD0**.

Grounded balls receive rolling resistance on slopes, with green/fairway/rough/sand
profiles and finer putting control. Five greens gently reshape the existing terrain;
Holes 4 and 5 move to gentler positions. Original turf, coastline and island silhouette
remain. No texture, model, animation, shader, audio or package was imported.
[Course](GOLF-COURSE.md), [physics](GOLF-MINIGAME.md),
[packing audit](APK-SIZE-AUDIT.md#golf-playability-and-integrated-greens--8-october-2026).

The complete **316-file** Windows player is delivered in `Builds/WindowsFinal/`,
built at **19:40:04**. All **315 runtime/support files** match the tested portable
copy. Selected verification totals **1,594 passing assertions in 15 reports**,
plus 13,122 base-terrain reference samples. Matching **protocol 35** peers required.
[Evidence, source limits and archives](VERIFICATION.md#golf-playability-and-integrated-greens--8-october-2026).
No physical Android device was attached; phone quality/FPS/touch remain unverified.

## Simple maps, round controls and resource cleanup — 8 October 2026

Fresh AndroidSubmission completed at **15:14:47 Malaysia time**:
**89,894,276 → 88,529,938 bytes (−1,364,338)**. Strict-limit headroom is
**11,470,062 bytes**. The **75,000,000-byte development target remains exceeded
by 13,529,938 bytes**. Budget enforcement and independent APK v2 signature
verification pass. SHA-256:
**96112520F055A49E1253A1D3EA720D60439105B3D382C06EF2F2B03CCB49199D**.

Both maps are small bordered boards. Gameplay controls are round, text-only
thumb targets with press/release animation and fixed hit areas. Team possession,
automatic basketball teams, football pace/stamina and all action gestures remain.
[HUD guide](SPORTS-HUD.md), [resource decisions](HUD-RESOURCE-CLEANUP.md),
[packing audit](APK-SIZE-AUDIT.md#simple-maps-round-controls-and-resource-cleanup--8-october-2026).

The complete player delivered in `Builds/WindowsFinal/` has **316 files**, including **315 runtime
and support files** verified against a portable copy. Full assets finished at
**15:04:47**, final scripts at **15:08:22**. Verification totals **1,801
passing assertions in 21 selected reports**.
[Delivery, source limits and archives](VERIFICATION.md#simple-maps-round-controls-and-resource-cleanup--8-october-2026).
Evidence: `Builds/SimpleHudQA/`. Matching **protocol 34** peers are required.
No physical Android device was attached; phone quality/FPS/touch remain unverified.

## Team controls, pocket maps and Cove buttons — 8 October 2026

Fresh AndroidSubmission completed at **14:12:47 Malaysia time**:
**89,878,628 → 89,894,276 bytes (+15,648)**. Strict-limit headroom is
**10,105,724 bytes**. The **75,000,000-byte development target remains exceeded
by 14,894,276 bytes**. The hard budget gate and independent APK v2 signature
verification pass. SHA-256:
**A56A4EBDA9A7F34C2FE63F249FDDFB9EED3BFC1FB43C213ED4CE0C691C4073DD**.

Football/basketball action slots now follow team possession. Both sports have
bottom-centre maps with blue teammates and red opponents. Basketball squads
alternate automatically as players join. Cove button styling covers all four
sports and Sky-Sail travel. The existing football speed/stamina changes remain.
[Controls and design](SPORTS-HUD.md),
[detailed packing audit](APK-SIZE-AUDIT.md#team-controls-pocket-maps-and-cove-buttons--8-october-2026).
Serialized assets increase **488 bytes** to **156,298,585 bytes**, separate from
the compressed APK measurement. No imported art or runtime package is added.

The complete **316-file** Windows player is in `Builds/WindowsFinal/`.
Full assets completed at **13:50:48**, final scripts at **14:09:56**.
All **315 runtime/support files** match the tested candidate and portable copy.
Selected verification totals **1,005 passing assertions in 19 reports**.
[Coverage, source-snapshot limits and archives](VERIFICATION.md#team-controls-pocket-maps-and-cove-buttons--8-october-2026).
Windows/Android manifests agree on **3,111 of 3,112 inputs**, including metadata.
The generated Golf ball prefab differs only in IDs/order; all 14 objects retain
their properties and reference topology. Original IDs were restored after
format-aware verification. Matching **protocol 34** peers are required.
No physical Android device was attached; phone qualification remains unverified.
Evidence: `Builds/SportsHudQA/`.

## Football pace, stamina and pressure — 8 October 2026

Fresh AndroidSubmission completed at **13:01:42 Malaysia time**:
**89,864,908 → 89,878,628 bytes (+13,720)**. Strict-limit headroom is
**10,121,372 bytes**. The **75,000,000-byte development target remains exceeded
by 14,878,628 bytes**. The hard budget gate and independent APK v2 signature
verification pass. SHA-256:
**27A407964D31D0F5C0BEDC707AC48715EF35B774008D23935802E17C19FA0E1A**.

Football now has 150% walking/running speed, 140% ball launch speed, stamina,
dash, directional defensive pressure and overhead bars. It reuses existing
UI and character assets. [Controls and tuning](FOOTBALL-PACE-AND-PRESSURE.md),
[detailed packing audit](APK-SIZE-AUDIT.md#football-pace-stamina-and-pressure--8-october-2026).
Serialized assets increase only **312 bytes** to **156,298,097 bytes**;
this is separate from the compressed APK measurement.

The complete **316-file** Windows player is in `Builds/WindowsFinal/`.
Full assets completed at **12:43:12**, with final scripts at **12:57:31**.
All **315 runtime/support files** match both the validated candidate and its
portable copy. Selected verification totals **1,348 passing assertions in
14 reports**. [Verification and archives](VERIFICATION.md#football-pace-stamina-and-pressure--8-october-2026).
Windows/Android manifests agree on **3,099 of 3,100 inputs, including metadata**.
The generated Golf ball prefab differs only in IDs/order; all 14 objects
retain their properties and reference topology. Its original identifiers
were restored after format-aware verification. Use matching protocol **33**
peers. No physical Android device was available; phone qualification remains
unverified. Evidence: `Builds/FootballPaceQA/`.

## Simple lagoon fishing — 7 October 2026

Fresh AndroidSubmission completed at **23:16:43 Malaysia time**:
**89,865,804 → 89,938,934 bytes (+73,130)**. The before measurement is a fresh
build of the pulled `1fcdd26` baseline. Strict-limit headroom is **10,061,066
bytes**. The **75,000,000-byte development target remains exceeded by
14,938,934 bytes**. The build budget gate and independent APK v2 signature
verification pass. SHA-256:
**AB60082F75449D149DCDD1BDBF2BD5D193153DCC7DE6F1BC5B4C7F71B69207A7**.

The lagoon now has held rods, cast/bite/hook/tension reeling, swimming fish,
shared catch points and a host-started three-minute contest. Existing fish,
rods, piers, islands and authoring masters are reused. One small untextured
line material is added; the environment reflection is generated at runtime.
Serialized assets grow **3,256 bytes** to **156,301,133 bytes**, a different
measurement from the APK. [Controls and limitations](FISHING-MINIGAME.md),
[packing audit](APK-SIZE-AUDIT.md#simple-lagoon-fishing--7-october-2026).

The complete **315-file** Windows player is delivered in `Builds/WindowsFinal/`;
its full assets were built at **22:32:43** and final scripts at **23:05:32**.
Protocol **33** is
required for matching peers. Fishing, golf controls/networking, walking and
portable launch checks pass. Final script-source/Android raw manifests match on
**3,109/3,110 inputs**; the difference is generated Golf prefab IDs/order.
The full Windows assets retain their desktop SSAO runtime resource list.
Gameplay source matches;
the original equivalent Golf prefab serialization was restored after building.
Evidence: `Builds/FishingGameplayQA/`.
[Verification](VERIFICATION.md#simple-lagoon-fishing--7-october-2026).
No physical Android device or internet-hosted room was tested. The 75 MB target
shortfall remains unresolved; the hard-limit headroom is a constrained reserve.

## Perspective broadcast cameras — 7 October 2026

Fresh AndroidSubmission completed at **18:45:24 Malaysia time**:
**89,861,552 → 89,864,908 bytes (+3,356)**. Strict-limit headroom is
**10,135,092 bytes**. The **75,000,000-byte development target remains exceeded
by 14,864,908 bytes**. The build budget gate and independent APK v2 signature
verification pass. SHA-256:
**EC2D59AAC704AC726A83D3D02DAF12AAB80C52968C7A1D9979410013C06FF00E**.

This revision replaces the earlier flat stadium cameras with perspective,
separate football/basketball angles, smooth pan and distance changes, and
basket-aware framing. At 16:9 midfield, football covers about 40% of the lawn
and basketball about 53% of the court. First person and default third person
retain their framing. [Camera design and references](STADIUM-CAMERAS.md).
No art or package is added; serialized assets remain **156,297,785 bytes**.
[Detailed packing audit](APK-SIZE-AUDIT.md#perspective-broadcast-cameras--7-october-2026).

The complete Windows player uses full assets built at **18:13:45**, with final
managed scripts built at **18:39:14**. The existing basketball loft/bounce
passes, shot odds, defense and protocol **32** are retained. Windows/Android
manifests match on **3,089 of 3,090 inputs, including metadata**. The generated
Golf prefab differs only in IDs/order; all 14 objects retain their properties
and reference topology. Evidence: `Builds/BroadcastCameraQA/`.
[Final verification](VERIFICATION.md#perspective-broadcast-cameras--7-october-2026).
No physical Android device was tested.

## Stadium camera framing — 7 October 2026

Fresh AndroidSubmission completed at **17:27:00 Malaysia time**:
**89,847,792 → 89,861,552 bytes (+13,760)**. Strict-limit headroom is
**10,138,448 bytes**. The **75,000,000-byte development target remains exceeded
by 14,861,552 bytes**. The hard budget gate and independent APK v2 signature
verification pass. SHA-256:
**E17B46A6A487C318567FB0ACC9311A0EE65C80F172420D2FB38AC5112D71113A**.

Football now uses a high horizontal stadium view covering about 40% of the
lawn at midfield; basketball uses a closer view covering about 52% of its
painted court. Field-based panning leaves edge players off centre and clear
of the HUD. First person and the default third person retain their framing.
[Camera behavior](STADIUM-CAMERAS.md),
[packing audit](APK-SIZE-AUDIT.md#stadium-camera-framing--7-october-2026).
The camera change adds no art or runtime package. This build also preserves
the workspace's existing basketball trajectory/shot-odds changes and uses
protocol **32**, so use matching Windows/Android peers. The measured delta
includes those existing changes; it is not a camera-only size comparison.

The basketball work in this same release adds [loft/bounce passes](BASKETBALL-PASSING.md)
and [continuous shot chances](BASKETBALL-SHOT-ODDS.md). Its separate feature
review passes 828 assertions on the original basketball candidate and 328
additional assertions on the delivered combined candidate. The full combined
manifest, including metadata, matches on 3,089 of 3,090 inputs; only the
equivalent generated Golf prefab differs. Manifest provenance and feature
evidence are retained under `Builds/BasketballTrajectoryQA/`.

The complete **316-file** Windows player is delivered in `Builds/WindowsFinal/`.
Its full assets were built at **17:13:54**, with final managed scripts at
**17:22:50**. Camera checks cover three landscape aspect ratios, a complete
portable copy in a path containing spaces, and an unrelated working directory.
Football match and basketball passing/trajectory regressions also pass.
Windows/Android manifests agree on **1,520 of 1,521 non-metadata** source,
asset and configuration inputs. The sole difference is regenerated Golf
prefab IDs/order; format-aware comparison confirms all 14 objects preserve
their properties and reference topology. Evidence is under
`Builds/StadiumCameraQA/`.
[Verification](VERIFICATION.md#stadium-camera-framing--7-october-2026).
No physical Android device was tested.

## Fresh clone validation — 7 October 2026

The first local AndroidSubmission build from commit `0cbecdf` completed at
**16:22:25 Malaysia time**. No local APK existed before setup. Compared with
the previous documented release, **89,803,004 → 89,800,008 bytes (-2,996)**.
Strict-limit headroom is **10,199,992 bytes**. The **75,000,000-byte development
target remains exceeded by 14,800,008 bytes**. The hard budget gate and
independent APK v2 signature verification pass. SHA-256:
**97F67F1EBD601E6A6B945439B4F75A7A4963180DCB5A0FD3A9034A31A61C1485**.

This is a local setup validation build, signed with this developer's Android
debug certificate. Package `com.umpsa.whatthefish`, version `0.3.0`, uses ARM64,
minimum API 26 and target API 36. It does not establish store publishing or
physical-phone qualification. [Packing audit](APK-SIZE-AUDIT.md#fresh-clone-validation--7-october-2026).

The complete **15:50 Windows player** is in `Builds/WindowsFinal/`, with its
data, runtime files and Golf/Fishing/Sky-Sail launchers together. The offline
Storybook menu suite passed **694 assertions**, with zero failures, including
offline gameplay handoff; its rendered menu captures were inspected. The
complete **314-file** player has a SHA-256 inventory. Both players retain
protocol **28**. These checks do not replace the historical sport suites,
multiplayer testing or physical touch/FPS/thermal checks.

No gameplay code, authored scenes, art or package pins were changed. Of
**3,050** recorded Game inputs, **3,002** stayed byte-identical during the
builds; **47** changes were line endings, and one regenerated Golf prefab
retained all properties, reference topology and **24 incoming references**.
The original files were restored after verification, leaving no tracked Game
content diff. Evidence is in `Builds/LocalSetup/` and
`Builds/StorybookMenuQA/LocalSetup-20261007/`; current packing reports are in
`Builds/SizeAudit/latest/`. Phone and internet-room tests remain unverified on
this laptop. No previous local APK was available for a binary entry comparison.

## Shared basketball passing — 7 October 2026

Fresh AndroidSubmission completed at **16:14:52 Malaysia time**:
**89,829,116 → 89,847,792 bytes (+18,676)**. Strict-limit headroom is
**10,152,208 bytes**. The **75,000,000-byte development target remains exceeded
by 14,847,792 bytes**. The build budget gate and independent APK v2 signature
verification pass. SHA-256:
**81D8491272C83470C545600BDAB1E45EE8043E6AD01F4DF1BD7AEFF562A159DE**.

Protocol **31** adds a shared teal/amber pass lane, host-measured charge,
directional chest passes and smooth held, release and cancel poses.
[Controls and behavior](BASKETBALL-PASSING.md),
[packing audit](APK-SIZE-AUDIT.md#shared-basketball-passing--7-october-2026).
One small shader and material were added; no imported animation, texture, mesh,
audio or runtime package was added.

The complete **316-file** Windows player is in `Builds/WindowsFinal/`, including
the existing portable launchers and development geometry fixture. Its full
assets were built at **15:07:32**, with final managed scripts at **16:11:57**.
It passed the portable delivery check from a path containing spaces and an
unrelated working directory. Final Windows and Android manifests agree on
**3,077 of 3,078** source/configuration inputs. The sole difference is generated
Golf prefab identifiers/order; all 14 objects retain their properties and
reference relationships. Evidence is under `Builds/BasketballPassQA/`.
[Verification](VERIFICATION.md#shared-basketball-passing--7-october-2026).
No physical Android device was tested.

## Defense motion refinement — 7 October 2026

Fresh AndroidSubmission completed at **12:58:22 Malaysia time**:
**89,825,364 → 89,829,116 bytes (+3,752)**. Strict-limit headroom is
**10,170,884 bytes**. The **75,000,000-byte development target remains exceeded
by 14,829,116 bytes**. The hard budget gate and independent APK v2 signature
verification pass. SHA-256:
**5D0E552F9FE2C55ECBE82C1689328AAF416DE3A3F09C82AA305AB7A428265822**.

Protocol **30** refines defensive hand paths, elbow guides, guard stance,
moving takeoff, interrupted dunk recovery and pose blending. Starting foot
poses now replicate to guests. No imported animation or art was added.
[Motion and testing](BASKETBALL-DEFENSE.md#motion-and-validation) and
[packing audit](APK-SIZE-AUDIT.md#defense-motion-refinement--7-october-2026).

The complete Windows player is delivered in `Builds/WindowsFinal/`, with its
development-only surface-check fixture and existing portable launchers.
The final Windows and Android source manifests agree on all release gameplay,
art, scenes and configuration. A development-only review file was subsequently
fixed to publish test checkpoint files atomically; that entire file is excluded
from the release APK. The regenerated Golf prefab preserves all properties
and reference relationships. Evidence and source manifests are retained under
`Builds/BasketballDefensePolishQA/`.
[Verification](VERIFICATION.md#defense-motion-refinement--7-october-2026).
No physical Android device was tested.

## Basketball defense — 6 October 2026

Fresh AndroidSubmission completed at **22:51:12 Malaysia time**:
**89,803,004 → 89,825,364 bytes (+22,360)**. Strict-limit headroom is
**10,174,636 bytes**. The **75,000,000-byte development target remains exceeded
by 14,825,364 bytes**. The hard budget gate and independent APK v2 signature
verification pass. SHA-256:
**BBBE8DF5E83ED48C0D9340A77758968E3B39F4D96A3FCD39CF2AB4BA893198A5**.

Protocol **29** adds pressure guarding, standing and jump blocks, possession
based controls and rig-driven defensive poses. Swept hand contact determines
success; the host checks timing, reach, direction and obstructions. It also
keeps action time aligned with catch-up physics and guest shot releases aligned
with the displayed charge sample. No imported art or runtime package was added.
[Rules and controls](BASKETBALL-DEFENSE.md) and
[packing audit](APK-SIZE-AUDIT.md#basketball-defense--6-october-2026).

The complete Windows player in `Builds/WindowsFinal/` uses the **22:46:50**
managed build and the complete **21:54:15** player asset build. All **315 files**
match the validated candidate after preserving the existing portable launchers;
only the delivery note changes. The selected feature and regression suite has
**978 passing assertions in 22 reports, zero failures**. Use matching protocol
29 Windows/Android peers.

Across final Android packaging, **3,061 of 3,062** source/asset/config inputs
remain byte-identical. Format-aware comparison proves the regenerated Golf
prefab retains every property and reference relationship. Runtime scripts,
scenes and URP settings are unchanged. Evidence, source manifests, control
captures and animation preview are under `Builds/BasketballDefenseQA/`.
[Verification and archives](VERIFICATION.md#basketball-defense--6-october-2026).
No phone was connected; physical touch, Android appearance/FPS, thermals and
WAN latency remain unverified.

## Untimed finishes and shot fallback — 6 October 2026

Fresh AndroidSubmission completed at **01:09:38 Malaysia time**:
**89,798,556 → 89,803,004 bytes (+4,448)**. Strict-limit headroom is
**10,196,996 bytes**. The **75,000,000-byte development target remains exceeded
by 14,803,004 bytes**. The hard budget gate and independent APK v2 signature
verification pass. SHA-256:
**B4052AD336371FC3120C5CB572B8F691168E161E06875F9DEB47961A3F358685**.

Protocol **28** extends eligible approaches to 4 m, removes finish timing,
guarantees an unblocked dunk and gives layups one host-side 85% make roll.
Unavailable gestures release ordinary shots with the existing charge phase.
The gather retains a valid approach while the ball returns from a bounce, and
the jump and palm rollover are retimed for the wider range. The feature reuses
the existing character rig and physical ball; no art or runtime dependency was
added. [Rules](BASKETBALL-FINISHES.md) and
[packing audit](APK-SIZE-AUDIT.md#untimed-finishes-and-shot-fallback--6-october-2026).

The complete **01:04:56 Windows player** is delivered in
`Builds/WindowsFinal/`, with **811 passing assertions and zero failures**.
All **315 files** matched the tested candidate by SHA-256 during promotion at
**01:11 Malaysia time**, before updating its delivery note. Windows and Android
use protocol **28**, requiring matching peers. Existing menu, Golf, charging
and steal work is preserved.

Across Android packaging, **3,048 of 3,050** checked source/asset/config inputs
remain byte-identical; runtime scripts and scenes are unchanged. Format-aware
comparison proves the regenerated Golf prefab retains all properties and
reference topology; the other difference is two platform URP runtime
registrations. Evidence, source manifests, preview and audit snapshots are
under `Builds/BasketballGestureFixQA/`.
[Verification and archive batches](VERIFICATION.md#untimed-finishes-and-shot-fallback--6-october-2026).
No phone was connected; Android appearance, physical touch, FPS, thermals and
WAN timing remain unverified.

## Basketball layups and dunks — 5 October 2026

Fresh AndroidSubmission completed at **21:03:16 Malaysia time**:
**89,780,060 → 89,798,556 bytes (+18,496)**. Strict-limit headroom is
**10,201,444 bytes**. The **75,000,000-byte development target remains exceeded
by 14,798,556 bytes**. The hard budget gate and independent APK v2 signature
verification pass. SHA-256:
**B338305A08FFB2EE0E9F9FE9F2CE1E6CD45C348EA87FE3B32D8EDE20D1A0F3AB**.

Protocol **27** adds up/down finish selection on the existing Shoot button,
host-validated approach and jump clearance, and procedural gather, takeoff,
layup/dunk release and landing. The ball uses the existing physical scoring
rules. All poses reuse the shared character rig; no imported art, animation,
audio, material, texture or runtime dependency was added.
[Controls and calculations](BASKETBALL-FINISHES.md) and
[packing audit](APK-SIZE-AUDIT.md#basketball-layups-and-dunks--5-october-2026).

The complete **20:57:41 Windows player** is delivered in
`Builds/WindowsFinal/`, with **994 passing assertions** across the final suites.
All **315 files** matched the tested candidate by SHA-256 during promotion at
**21:08 Malaysia time**, before the delivery note was updated. Both builds use
protocol **27** and require matching peers. The source preserves the existing
Storybook menu, Golf and basketball work.

Across Android packaging, **3,048 of 3,050** checked source/asset/config inputs
remain byte-identical. Runtime scripts and scenes are unchanged. Format-aware
comparison proves the remaining changes are equivalent Golf prefab object IDs
and platform URP registrations. Evidence, source manifests, current video and
audit snapshots are under `Builds/BasketballFinishQA/`.
[Verification and archive batches](VERIFICATION.md#basketball-layups-and-dunks--5-october-2026).
No phone was connected; Android appearance, touch, FPS, thermals and WAN timing
remain unverified.

## Basketball charging and energy boundary — 5 October 2026

Fresh AndroidSubmission completed at **17:43:26 Malaysia time**:
**89,762,748 → 89,780,060 bytes (+17,312)**. Strict-limit headroom is
**10,219,940 bytes**. The **75,000,000-byte development target remains exceeded
by 14,780,060 bytes**. The hard budget gate and independent APK v2 signature
verification pass. SHA-256:
**E17FFB1658C16209CFAADD66ECE0496E36E7F20DAE3FA37407DC03B4FCA0CDE0**.

Protocol **26** adds a looping distance-based power meter, half-speed movement
while charging, replicated aiming poses, football-style cancellation and a
translucent court barrier with player/ball ripples. The barrier shares the
football material and shader, generating only 16 vertices at runtime. No
imported art, animation, audio, texture, material or package was added.
[Controls and calculations](BASKETBALL-GAMEPLAY.md) and
[packing audit](APK-SIZE-AUDIT.md#basketball-charging-and-energy-boundary--5-october-2026).

The complete **17:38:12 Windows player** is delivered in
`Builds/WindowsFinal/`, with **575 passing assertions** and a final rendered
check of corrected scoreboard text. All **315 files** matched the tested
candidate by SHA-256 during promotion at **19:53 Malaysia time**. The previous
player was archived after the user authorized closing its running game.
The current Windows player and APK use protocol **26** and must be paired with
matching builds. Source includes the existing Storybook menu, Golf and
basketball steal work.

Across final Android packaging, **3,040 of 3,042** source/asset/config inputs
remain byte-identical. Runtime code is unchanged. The other two contain
equivalent regenerated Golf prefab IDs and platform URP registrations.
Evidence, signatures, source manifests and the delivery status are under
`Builds/BasketballChargeQA/`.
[Verification and archive batches](VERIFICATION.md#basketball-charging-and-energy-boundary--5-october-2026).
No phone was connected; Android appearance, touch, FPS, thermals and WAN timing
remain unverified.

## Refined basketball steal animation — 5 October 2026

Fresh AndroidSubmission completed at **15:31:44 Malaysia time**:
**89,758,540 → 89,762,748 bytes (+4,208)**. Strict-limit headroom is
**10,237,252 bytes**. The **75,000,000-byte development target remains exceeded
by 14,762,748 bytes**. The budget gate and independent APK v2 signature check
pass. SHA-256:
**3DFEED06576E7F06310D6C71309FF3778B2876586C552AE7BDF0712530F21371**.

This release refines the swipe and lost-ball reaction with a planted step,
weight transfer, a curved hand sweep, shoulder recoil and recovery. It adds no
imported animation, texture, mesh, material, audio or package. See the
[animation notes](BASKETBALL-ANIMATION.md#steal-and-stripped-reaction) and
[packing audit](APK-SIZE-AUDIT.md#refined-basketball-steal-animation--5-october-2026).

The complete **15:25:12 Windows player**, protocol **25**, is promoted to
`Builds/WindowsFinal/`; all **314 files** were hash-checked before updating its
delivery note. **366 assertions pass** across fixed-rate pose reviews,
portability, real input, multiplayer stealing and existing basketball motions.
Evidence and the 17.33-second gameplay/pose preview are under
`Builds/BasketballStealAnimationQA/`.
[Verification and archive batches](VERIFICATION.md#refined-basketball-steal-animation--5-october-2026).

Across Android packaging, **3,032 of 3,034** source/asset/config inputs remain
byte-identical. The other two contain equivalent regenerated Golf prefab IDs
and platform URP registrations. Runtime scripts and scenes match the tested
Windows source. No phone was connected; Android appearance, FPS, thermals and
touch feel remain unverified.

## Basketball steals and loose-ball contests — 5 October 2026

Fresh primary AndroidSubmission completed at **14:07:22 Malaysia time**:
**89,744,112 → 89,758,540 bytes (+14,428)**. Strict-limit headroom is
**10,241,460 bytes**. The **75,000,000-byte development target remains exceeded
by 14,758,540 bytes**. The hard budget gate and independent APK v2 signature
verification pass. SHA-256:
**630BA97FAF4B1F0D4C65329D24C28F9E5271F554808FA7B5F03B83592EBDAA34**.

The **protocol 25** build adds repeatable Steal/F input, authoritative reach,
facing, shielding and dribble-timing checks, a swipe and stripped reaction, and
physical unowned deflections that anyone can recover. Both poses reuse the
existing rig. No imported art, animation clip, audio, material or package was
added. See [controls and calculations](BASKETBALL-GAMEPLAY.md#stealing-and-loose-ball-contests)
and [packing audit](APK-SIZE-AUDIT.md#basketball-steals-and-loose-ball-contests--5-october-2026).

The complete tested **14:03:09 Windows player** is promoted to
`Builds/WindowsFinal/`, including all **314 files** and the existing launchers.
**517 assertions pass**, including 69 steal checks, 31 portability checks and
417 shooting/passing/physics/motion/jump regressions.
[Verification](VERIFICATION.md#basketball-steals-and-loose-ball-contests--5-october-2026) and current evidence live under
`Builds/BasketballStealQA/`. The APK is `Builds/Android/WhatTheFish-release.apk`;
the current audit is retained in that task's `FinalSizeAudit/`.

Across Android packaging, **3,028 of 3,030** checked source/asset/config inputs
remained byte-identical. Only equivalent regenerated Golf ball prefab IDs and
platform URP registrations changed. Runtime scripts and scenes match the tested
Windows source. No phone was connected; touch feel, Android rendering/FPS,
thermals and WAN timing remain unverified.

Reviewed superseded APK/audit: `Legacy/20261005-140452-410-basketball-steal-before-release/`.
Superseded complete Windows delivery: `Legacy/20261005-140636-398-basketball-steal-windows-delivery/`.
Earlier test iterations and the copied portable fixture:
`Legacy/20261005-140731-651-basketball-steal-iterations/`.
Final captures, logs, signatures, source manifests and promotion hashes remain
under `Builds/`. No staging, commit or push was performed.

## Storybook Cove menu and teammate integration — 5 October 2026

The selected first menu direction is implemented with the official logo, four
cartoon islands, complete room/setup/settings/help pages and native interaction
feedback. Pulled `26a0bf1` and `af02ea7` by fast-forward, preserving local basketball
work through a reviewed three-way merge. The final combined source includes the
concurrent basketball pace/boundary changes and uses **protocol 24**.
[Menu implementation](STORYBOOK-MENU.md) and [verification](VERIFICATION.md#storybook-cove-menu-and-teammate-integration--5-october-2026).

Fresh primary AndroidSubmission completed at **13:19:09 Malaysia time**:
**86,791,760 → 89,744,112 bytes (+2,952,352)**. This comparison starts at the actual
pre-pull APK, so its increase includes the incoming Golf integration and basketball
work as well as the menu. Strict-limit headroom: **10,255,888 bytes**. The
**75,000,000-byte development target remains exceeded by 14,744,112 bytes**.
The strict build gate and independent APK v2 signature verification pass.
SHA-256: **D0C2860AFA93F2F0F01DFAAC8F86F14DC114DC54E1B11B9E6DDBCD48C882ECBC**.

Current output is `Builds/Android/WhatTheFish-release.apk`; fresh audit is
`Builds/SizeAudit/latest/`, retained in `Builds/StorybookMenuQA/FinalSizeAudit/`.
The complete tested **13:14:48 Windows player** is in `Builds/WindowsFinal/`, with
all runtime files and the Golf, Fishing and Sky-Sail launchers. See its updated
`LATEST-BUILD.txt` and the task's `windows-promotion.json` for package hashes.

Across Android packaging, **3,020 of 3,022 source/asset/config files remained
byte-identical**. The two changes are reviewed generated state: the Golf ball
prefab has equivalent properties and references with regenerated local IDs;
URP registrations return to the pulled Android configuration. Runtime source,
scenes, menu textures and fonts remain identical. [Packing detail](APK-SIZE-AUDIT.md#storybook-cove-menu-and-teammate-integration--5-october-2026).

Superseded baseline APK/audit: `Legacy/20261005-131618-369-storybook-before-release/`.
Superseded complete Windows delivery: `Legacy/20261005-132228-234-storybook-windows-delivery/`.
Reviewed menu iterations and portable fixture are recorded in
`Builds/StorybookMenuQA/archive-iterations.txt`; the hash-matched duplicate test
player is in `Legacy/20261005-132634-167-storybook-tested-candidate/`.
Recovery stash and pre-merge
files are retained. No staging, commit or push was performed. Phone touch,
rendering, sustained FPS, thermals and different-network WAN remain unverified.

## Basketball pace and player boundaries — 5 October 2026

Fresh AndroidSubmission completed at **12:56:34 Malaysia time**: **86,791,760 → 89,260,934 bytes (+2,469,174)**. Strict-limit headroom is **10,739,066 bytes**. The **75,000,000-byte development target remains exceeded by 14,260,934 bytes**. The hard budget gate and independent APK v2 signature verification passed. SHA-256: **48EAEF354F940AFC8B0B6F375A0D07D4D7BAED4330B56C36B424CA5C72666594**.

The before value is the actual APK measured at task start, before the repository received the Golf/football integration in `af02ea7`. The tested build includes that integration and the preserved basketball physics, faster shot preparation and player boundary. Most asset growth comes from the incoming Golf equipment/course; this basketball change adds no imported art, audio or package. See [the packing comparison](APK-SIZE-AUDIT.md#basketball-pace-and-player-boundaries--5-october-2026).

The matched **protocol 23** Android and Windows players are retained in **`Builds/BasketballPaceQA/Release/`**. They were built in an isolated validation checkout to exclude concurrent unfinished Storybook menu changes. These measurements do not validate the later combined menu source or an older APK in `Builds/Android/`. The main source uses **protocol 24** for its additional menu network field and rejects connections to these protocol 23 players.

Of **277 checked script, shader, prefab and scene inputs**, 276 were unchanged through the final Android build. The regenerated Golf ball prefab differed only in local object IDs and document ordering; a canonical comparison confirmed identical properties and reference topology. Source snapshots, both release players, signature, manifests, audit and [479 passing checks](VERIFICATION.md#basketball-pace-and-player-boundaries--5-october-2026) are under `Builds/BasketballPaceQA/`. No Android device was connected; phone rendering, touch feel, sustained FPS, thermals and WAN timing remain unverified. Earlier sections describe historical builds.

## Basketball physics and net response — 4 October 2026

Measured release: **86,774,768 → 86,791,760 bytes (+16,992)**. Strict-limit headroom: **13,208,240 bytes**. The **75,000,000-byte development target remains unmet by 11,791,760 bytes**. The baseline is the actual APK measured before this task. No imported mesh, texture, animation clip, audio or dependency was added.

The main-project `Tools/Build/Build.ps1 -Target AndroidSubmission` passed with Unity 6000.3.20f1, release IL2CPP ARM64, at **23:45 Malaysia time**. Process-local TEMP/TMP used ignored `Builds/JdkTmp/`. The hard budget gate and APK v2 signature verification passed. Current output: `Builds/Android/WhatTheFish-release.apk`; retained task copy: `Builds/BasketballPhysicsQA/WhatTheFish-release.apk`. SHA-256: **861647E6075987EEF909558F86B0E76C0C590DBCEB74049694539FECD44AB626**.

The 30 cm ball scales the existing two LOD meshes. Both responsive nets generate 1,152 vertices at runtime and reuse the existing material; idle nets stop rewriting their mesh. The power meter and score HUD use existing UI rendering. ZIP growth is principally IL2CPP **+14,294** and metadata **+2,390 bytes**. The largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected art/dependency growth was found. Serialized packed assets total **151,510,681 bytes**, which is a different measurement from APK size.

All **238 checked script, shader, prefab and scene inputs** remained unchanged through the Android build. Exact measurements, signature, source manifest, per-asset estimates and ZIP comparisons are under `Builds/BasketballPhysicsQA/`, including `FinalSizeAudit/`. [Controls and calculations](BASKETBALL-GAMEPLAY.md) and [396 passing Windows checks, visual review and cleanup](VERIFICATION.md#basketball-physics-and-net-response--4-october-2026) describe validation. No Android device was connected; phone appearance, touch feel, sustained FPS, thermals and WAN timing remain unverified. Earlier sections describe historical builds.

## GitHub Golf and football integration — 5 October 2026

Combines local Golf commit `26a0bf1` with upstream `a98d75f`. Eight merge
conflicts were reviewed: both release histories and both replicated sport
layouts are retained, room protocol is **22**, and the complete five-hole Golf
scene is preserved. The upstream Golf scene differs from its baseline only in
equivalent generated local object IDs. Golf retains its **18 m/s** full-charge
launch and slope settling; upstream football motion/net/aim and lagoon fish
presentation are included.

Fresh primary AndroidSubmission completed at **00:07:25 Malaysia time**:
**89,024,294 → 89,230,290 bytes (+205,996)**. Strict-limit headroom is
**10,769,710 bytes**; the **75,000,000-byte development target remains exceeded
by 14,230,290 bytes**. The strict gate and independent APK v2 signature pass.
SHA-256: **A112A37B04DC3A73A658CE6BB197AF91081EB6F45151464E9D09D9AB2227241C**.
See [packing audit](APK-SIZE-AUDIT.md#github-golf-and-football-integration--5-october-2026).

The complete tested **00:02:12 Windows player**, all **313 files**, is promoted
to `Builds/WindowsFinal/` with updated `LATEST-BUILD.txt`. Exact Windows/APK/audit
copies are retained in `Builds/GitSyncQA/Release/`. **1,106 assertions pass**:
252 Golf, 532 football gameplay/physics/network, 292 football motion and 30
rendered lagoon. See [verification and source scope](VERIFICATION.md#github-golf-and-football-integration--5-october-2026).
Phone rendering, touch, FPS and WAN remain unverified.

Reviewed superseded APK/audit:
`Legacy/20261005-000429-153-golf-github-merge-old-apk/` (13 files, 92.65 MB).
Superseded complete Windows player:
`Legacy/20261005-001837-569-golf-github-merge-player/` (313 files, 589.53 MB).
Current conflict-review, signature, source and package-hash evidence remain in
`Builds/GitSyncQA/`. Earlier records describe their captured releases.

## Golf swing speed 18 m/s — 4 October 2026

Full charge now launches at **18 m/s horizontally**, with the existing
**1.2 m/s** minimum and **1.8 m/s** maximum lift. The preceding five-metre
distance cap is removed. Flying and fast rolling retain normal physics;
the existing gradual slope settling, resting and next-hit wake remain.
Launch and settling parameters are centralized in
[GolfBallPhysics.asset](../Game/Assets/_Game/Resources/GolfBallPhysics.asset).

Fresh primary AndroidSubmission completed at **23:27:16 Malaysia time**:
**89,024,798 → 89,024,294 bytes (−504)**. Strict headroom is
**10,975,706 bytes**. The **75,000,000-byte development target remains exceeded
by 14,024,294 bytes**. The strict gate and independent APK v2 signature pass.
SHA-256: **32A7E0B4FF36268312E7C62762BEB9FE8C67FBE6D27BC6FAC8FDE282EDEC3792**.
See [packing audit](APK-SIZE-AUDIT.md#golf-swing-speed-18-ms--4-october-2026).

The complete tested **23:20:49 Windows player**, including all **313 files**,
is promoted to `Builds/WindowsFinal/` and retained in
`Builds/GolfEighteenSpeedQA/Release/Windows/`. **250 assertions** pass: 76 ball
physics, 134 Golf offline and 20 host plus 20 client. A real full-charge contact
measures **18.00000 m/s** horizontally and travels **12.27873 m** during the
0.8-second observation from release. Charge/contact, scoring, the original
meadow and forward-facing cart feet are retained. Exact APK/audit, signatures,
source comparisons and promotion hashes are in `Builds/GolfEighteenSpeedQA/`.
See [verification](VERIFICATION.md#golf-swing-speed-18-ms--4-october-2026).
Phone rendering, touch, FPS and WAN remain unverified.

Reviewed obsolete APK/audit: `Legacy/20261004-232537-143-golf-eighteen-speed-old-apk/`
(12 files, 92.66 MB). Superseded Windows delivery:
`Legacy/20261004-232723-737-golf-eighteen-speed-delivery/` (313 files, 589.54 MB).
Redundant and superseded candidates:
`Legacy/20261004-233110-778-golf-eighteen-speed-candidates/`
(1,252 files, 2,358.14 MB). Current release evidence is retained under `Builds/`.

## Golf meadow restored — 4 October 2026

Restored **294 original desktop mesh references** and the Android meadow,
retaining all five numbered cups, flags, tees, collision openings and scoring.
Only the cup radius plus **0.10 m** is cleared, replacing the former large
green-radius clearings. The current original placements have no blades in those
tiny openings: **2,289 blades** return, restoring **137,724** original blades.
Original terrain maps and five-hole configuration hashes remain unchanged.

Opening APK **89,021,758** → fresh **89,024,882 bytes (+3,124)**, including
concurrent shot-range/cart work. Shared primary AndroidSubmission completed at
**22:44:23 Malaysia time**; strict gate and independent v2 signature pass.
Strict headroom **10,975,118 bytes**; the **75,000,000-byte development target
remains exceeded by 14,024,882 bytes**. SHA-256:
**20D100869CBFAA24F9E74F7F5CE1D16BCA0075A8B8F3FDC4F935A301272706E4**.
See [audit](APK-SIZE-AUDIT.md#golf-meadow-restored--4-october-2026).

Complete tested **22:43:13 Windows player** promoted to `Builds/WindowsFinal/`.
**354 checks** pass (78 editor, 142 rendered island, 134 rendered match), plus
desktop-reference and full runtime-meadow comparisons. Exact player/APK/audit
and evidence: `Builds/GolfGrassQA/Release/` and
[verification](VERIFICATION.md#golf-meadow-restored--4-october-2026).
Phone appearance/touch/FPS remains unverified. Snapshot includes the concurrent
5 m stroke cap and cart foot pose. The later **22:46 GolfBall settled-stroke
range guard fix** belongs to another ongoing task and is not validated by these
packages. Reviewed clone differences are editor-only clean packing, platform
URP registrations and equivalent Ball prefab IDs.

Initial disk-full incomplete player and 21 proven-unused grass derivatives:
`Legacy/20261004-223137-525-golf-grass-obsolete/` (76 files, 875.77 MB).
Companion metadata: `Legacy/20261004-223608-027-golf-grass-meta/`.
Older 22:23 player, clone derivatives and temporary editor wrappers:
`Legacy/20261004-224855-541-golf-grass-delivery/` (360 files, 592.78 MB).
Closed-cache lossless compression preserved all hashes; caches remain in place.
Git publishing remains with the user.

## Golf cart forward-facing feet — 4 October 2026

Fresh task APK: **89,021,758 → 89,025,074 bytes (+3,316)**, built at
**22:40:48 Malaysia time**. Strict headroom **10,974,926 bytes**;
development target exceeded by **14,025,074 bytes**. AndroidSubmission,
APK v2 signature and independent packing audit pass. Serialized assets grow
only **4 bytes** from the concurrent five-metre Golf configuration; no art is
added by the feet correction. ZIP entries grow 3,314 bytes and signing/container
overhead grows 2 bytes. The largest estimates remain timber 2.247 MB, character
albedo 1.993 MB, Golf structures 1.981 MB and character mesh 1.588 MB.

Exact task packages/audit are retained in `Builds/GolfCartFeetQA/Release/`.
The newer shared APK (**89,024,882 bytes**, 22:44:23) is preserved;
its v2 signature and generated native toe-tip branch were checked. Current
shared headroom is **10,975,118 bytes**; its development-target
excess is **14,024,882 bytes**. These snapshots differ in unrelated
concurrent grass work. The initial 313-file Windows feet snapshot passed
**439 checks**. The newer complete shared Windows player (22:43:13) includes
the same feet correction and passed another **221 cart assertions**. See
[verification and scope](VERIFICATION.md#golf-cart-forward-facing-feet--4-october-2026).

## Reliable Golf Swing proximity — 4 October 2026

Fresh APK: **89,000,984 → 89,021,758 bytes (+20,774)** from the task opening,
including intervening wooden-tee and cart releases. The immediate predecessor
was **89,022,058 bytes**; this Swing release is **300 bytes smaller**.
Strict-limit headroom is **10,978,242 bytes**. The **75,000,000-byte development
target remains exceeded by 14,021,758 bytes**. AndroidSubmission completed at
**21:16:14 Malaysia time**, with the strict gate and independent APK v2
signature passing. SHA-256:
**101E2ADF8BE4052299771D620B804932FF1E64F8541A3352457C9CCF0E82CCEE**.

The complete Windows player built at **21:32:44** is promoted to
`Builds/WindowsFinal/`; all **313 files** match the retained release. The APK
and all **11 audit files** also match. Current packages and exact retained
copies are under `Builds/Android/` and `Builds/GolfSwingQA/Release/`.
**560 assertions** pass on matching gameplay/assets: 104 Swing interaction,
134 Golf offline, 41 side-approach host/client, 72 ball/tee/physics and 209 cart.
See [verification](VERIFICATION.md#reliable-golf-swing-proximity--4-october-2026)
and [packing audit](APK-SIZE-AUDIT.md#reliable-golf-swing-proximity--4-october-2026).

The **2,974-input** capture retains the current scenes, art, physics, wooden
tees and cart. The final Windows-only gait probe uses a level test fixture;
the entire probe is excluded from release APK compilation. Reviewed isolated
differences are editor-only clean packing, generated URP registrations,
equivalent **14-object** Ball prefab IDs and a legacy material color
serialization difference below **1e-6**. Lit BaseColor and all other material
properties match. No new art is added by the Swing fix. Source manifests,
signature, graph comparison and promotion hashes remain in `Builds/GolfSwingQA/`.
Physical-phone touch/rendering/FPS and WAN remain unverified.

Reviewed superseded delivery: `Legacy/20261004-214618-687-golf-swing-delivery/`
(**325 files, 682.33 MB**). Candidate/redundant players:
`Legacy/20261004-214623-806-golf-swing-candidates/` (**1,252 files, 2,357.51 MB**).
The interrupted initial player and old isolated APK/audit are recoverable in
`Legacy/20261004-204653-489-golf-swing-interrupted/` and
`Legacy/20261004-211127-929-golf-swing-old-clone-apk/`. Git publishing remains
with the user.

## Golf wooden opening tee — 4 October 2026

Opening APK: **89,000,984 bytes**. Delivered measured APK: **89,022,058 bytes
(+21,074)**, with **10,977,942 bytes** of strict-limit headroom. The
**75,000,000-byte development target remains exceeded by 14,022,058 bytes**.
The four shared tee assets add **28,923 serialized bytes / 16,266 estimated
ZIP bytes**. They use one **832-triangle** mesh, one **64×128** wood-grain
texture with ASTC 6×6 Android import, and one instanced material. The existing
ball, island terrain, flags, characters and equipment are retained.
See [packing audit](APK-SIZE-AUDIT.md#golf-wooden-opening-tee--4-october-2026).

The first primary AndroidSubmission attempt failed with native out-of-memory
before producing an APK. A subsequent concurrent cart-seat AndroidSubmission
successfully built the same gameplay and tee assets at **20:56:07 Malaysia
time**. This exact fresh package is reused after a full source comparison,
packing analysis and independent APK v2 verification; no older APK is used to
validate this change. Reviewed differences are platform URP registrations,
build-only clean packing, equivalent Ball prefab local IDs, and later editor
or development-only review/probe code. SHA-256:
**C260EDEBFFF01DC214668A9B6AF72736EA7E5A57FFBE71B25675711CCC27D947**.
The optional `Tools/Build/Build.ps1 -JobWorkerCount 2` now permits a smaller
Unity worker pool on memory-constrained machines; its default remains unchanged.

The complete Windows player built at **20:57:01** passes **246 assertions**
(72 rendered ball/tee/physics checks, 134 rendered Golf/club checks, 20 host
and 20 client checks). It is promoted to `Builds/WindowsFinal/`, preserving the
latest straight-knee cart pose, slope settling, 3× ball and Aim controls.
Exact retained player: `Builds/GolfWoodTeeQA/Player/`; APK and audit:
`Builds/GolfWoodTeeQA/Release/`. The **2,974-input** source comparison and final
evidence remain under `Builds/GolfWoodTeeQA/`. Later primary `GolfClubMotion`
work and a development-only `GolfMatchProbe` edit are outside this tested
snapshot; their source changes are preserved. Physical-phone rendering/touch/FPS
and WAN remain unverified. See [verification](VERIFICATION.md#golf-wooden-opening-tee--4-october-2026).

The superseded shared player is recoverable in
`Legacy/20261004-211728-900-golf-wood-tee-delivery/` (**313 files, 589.38 MB**).
Reviewed intermediate players and compile/stance/integration/OOM diagnostics
are recorded in `Builds/GolfWoodTeeQA/archive-*.txt`. Git publishing stays with
the user.

## Golf cart straight-knee driver — 4 October 2026

Opening APK: **89,002,854 bytes**. Fresh APK: **89,022,058 bytes (+19,204)**;
the intervening gentle-contact delivery was **89,000,870 bytes (+21,188)**.
Strict-limit headroom: **10,977,942 bytes**. The **75,000,000-byte development
target remains exceeded by 14,022,058 bytes**. AndroidSubmission completed
release IL2CPP ARM64 at **20:56:07 Malaysia time**; strict size gate,
independent APK v2 signature and fresh packing analysis pass. SHA-256:
**C260EDEBFFF01DC214668A9B6AF72736EA7E5A57FFBE71B25675711CCC27D947**.

The pose change adds no art, animation, texture or package. This snapshot also
retains the current wooden starting tee and slope settling/contact behavior.
Serialized assets total **155,324,105 bytes (+29,099)** against the preceding
contact delivery; the wooden tee accounts for **28,923** of those bytes.
See [fresh packing audit](APK-SIZE-AUDIT.md#golf-cart-straight-knee-driver--4-october-2026).

The complete Windows player built at **20:47:08** passes **455 assertions**:
**209 cart**, **72 ball/physics/tee**, and **174 Golf/club/room** checks.
All **313 Windows files**, **11 audit files**, and the APK match the retained
release. Current delivery: `Builds/WindowsFinal/` and
`Builds/Android/WhatTheFish-release.apk`; exact retained player/APK/audit:
`Builds/GolfCartSeatQA/Release/`. The **2,974-input** capture retains all
gameplay sources. Later development-only GolfMatchProbe/GolfSwingProbe and
editor GolfSwingReview changes are outside this tested snapshot and compiled
out of release. Generated URP registrations and equivalent Ball prefab IDs
are reviewed; the canonical **14-object graph** matches. Ignored performance
test run records are excluded. [Native evidence and limits](VERIFICATION.md#golf-cart-straight-knee-driver--4-october-2026).

Superseded Windows: `Legacy/20261004-205455-043-golf-cart-seat-windows/`
(**313 files, 589.34 MB**); superseded Android/audit:
`Legacy/20261004-210405-328-golf-cart-seat-android/` (**13 files, 93.05 MB**).
The reviewed trial is in `Legacy/20261004-203548-973-golf-cart-seat-trial/`
(**359 files, 639.50 MB**); a concurrent-build memory failure is preserved in
`Legacy/20261004-204222-803-golf-cart-seat-oom/` (**3 files, 1.00 MB**).
Current evidence remains under `Builds/GolfCartSeatQA/`. No publishing action
was performed. Phone quality/FPS and WAN remain unverified.

## Golf slope settling and resting — 4 October 2026

Final APK: **89,001,008 → 89,000,870 bytes (−138)**. The concurrent Aim
delivery was **89,000,984 bytes** (final is **114 bytes smaller**); an interim
settling candidate was **89,002,854** (final is **1,984 bytes smaller**).
Strict-limit headroom: **10,999,130 bytes**. The **75,000,000-byte development
target remains exceeded by 14,000,870**. AndroidSubmission completed release
IL2CPP ARM64 at **20:39:41 Malaysia time**; strict size gate, independent APK
v2 signature and fresh packing audit pass.
SHA-256: **76ED88F934D6DFB4CB5CEF3A04C4A6359045BE2C136B737FE98C422DC29DA8C1**.

The complete Windows player built at **20:29:59** passes **233 assertions**
(61 rendered ball physics, 134 Golf offline, 19 host, 19 client). It is promoted
to `Builds/WindowsFinal/`; all **313 files** match the retained release. The
shared APK and all **11 audit files** match as well. Current APK:
`Builds/Android/WhatTheFish-release.apk`; retained complete player/APK/audit:
`Builds/GolfSettlingQA/Release/`.

The isolated **2,957-input** snapshot retains the existing scene/map/hole/player
structure. Concurrent wooden-tee, swing-review and cart/locomotion work remains
in primary but is outside this release. Tee-dependent probe blocks and its
new ground-query layer are excluded only in the isolated snapshot; core ball
physics otherwise matches primary, as recorded in the code comparison.
Reviewed generated differences are clean packing and equivalent Ball prefab
local IDs; its canonical **14-object graph** matches. Source manifests,
protected-asset comparison, signatures and promotion hashes remain under
`Builds/GolfSettlingQA/`. See
[61+172-check scope](VERIFICATION.md#golf-slope-settling-and-resting--4-october-2026)
and [packing analysis](APK-SIZE-AUDIT.md#golf-slope-settling-and-resting--4-october-2026).
Physical-phone touch/rendering/FPS and WAN remain unverified.

Final superseded delivery: `Legacy/20261004-204254-498-golf-settling-final-delivery/`
(**325 files, 681.62 MB**). Earlier shared delivery is in
`Legacy/20261004-202200-124-golf-settling-delivery/` (**325 files, 682.26 MB**).
Interim exact release/evidence is in
`Legacy/20261004-203148-982-golf-settling-interim/` (**336 files, 684.15 MB**).
Superseded checks and diagnostics are in
`Legacy/20261004-203654-635-golf-settling-superseded-checks/` (**59 files, 67.90 MB**)
and `Legacy/20261004-201545-660-golf-settling-trials/` (**55 files, 58.28 MB**).
Earlier candidate players are in the `golf-settling-first-player`,
`golf-settling-refinement` and `golf-settling-player-copy` batches; exact recovery
records remain in `Builds/GolfSettlingQA/archive-*.txt`. The final redundant
isolated player is recorded in `archive-final-player-copy.txt` there.
Git staging, commit and push remain with the user.

## Golf 3x ball and optional Aim — 4 October 2026

Opening APK: **88,984,188 bytes**. Final measured APK: **89,000,984 bytes
(+16,796)**; the intervening grip delivery was **89,001,008 bytes**, so this
release is **24 bytes smaller**. Strict-limit headroom is **10,999,016 bytes**.
The **75,000,000-byte development target remains exceeded by 14,000,984 bytes**.
No new ball mesh, texture, material or package is added. The existing shared
ball LODs scale to 3x together with their physics sphere; four extra control
glyphs add **936 bytes** to the existing font subset. Concurrent validated
grip/carry improvements are retained and account for most of the combined
growth. Serialized assets are **155,294,810 bytes (+16,350)**, separate from APK
length. See [packing audit](APK-SIZE-AUDIT.md#golf-3x-ball-and-optional-aim--4-october-2026).

Unity 6000.3.20f1 completed release IL2CPP ARM64 AndroidSubmission at
**19:39:23 Malaysia time**. Its strict gate and independent APK v2 signature
pass. SHA-256:
**6A46C9CD239FD8D43A7234C5FAFC9C02991146B06FD1B4B87738E78BF25FBC4D**.
Current APK: `Builds/Android/WhatTheFish-release.apk`; exact retained APK,
fresh audit and complete Windows player: `Builds/GolfBallAimQA/Release/`.

The complete Windows player built at **19:50:57 Malaysia time** is promoted
to `Builds/WindowsFinal/`. It passes **41 rendered ball/Aim/physics/cup/restart
checks** and **172 Golf/club/room checks**; **203 cart checks** also pass on the
same cart/input/camera behavior. Every delivered Windows file and the APK
match the retained package. All **2,953 primary asset/package/settings inputs**
were captured and remain unchanged after the final Windows capture. Reviewed
isolated differences are the build-only clean-cache editor option and
equivalent Ball prefab local IDs; the canonical **14-object graph** matches.
The APK's earlier development-only GolfMatchProbe diagnostic is compiled out
of release; its gameplay sources match. See
[verification scope](VERIFICATION.md#golf-3x-ball-and-optional-aim--4-october-2026).
Physical-phone touch/rendering/FPS and WAN remain unverified.

The superseded shared grip player/APK/audit is recoverable in
`Legacy/20261004-195606-285-golf-ball-aim-delivery/` (**326 files, 682.31 MB**).
Reviewed intermediate snapshots and trials are in
`Legacy/20261004-190612-519-golf-ball-aim-initial/` (**318 files, 590.81 MB**),
`Legacy/20261004-192331-749-golf-ball-aim-snapshot/` (**332 files, 619.31 MB**)
and `Legacy/20261004-194641-726-golf-ball-aim-probe-snapshot/`
(**315 files, 590.77 MB**). Current evidence and input comparisons remain under
`Builds/GolfBallAimQA/`. Git publishing remains with the user.

The redundant isolated final player is recoverable in
`Legacy/20261004-200344-604-golf-ball-aim-player-copy/`
(**313 files, 589.33 MB**); the shared and retained player remain current.
TaskReady passes using a read-only copy of the index because the pre-existing
shared `.git/index.lock` is still present. The real index hash is unchanged;
no lock removal, staging or publishing was performed.

## Golf closed grip and raised carry angle — 4 October 2026

Fresh APK: **88,984,188 → 89,001,008 bytes (+16,820)**. Strict-limit
headroom is **10,998,992 bytes**. The **75,000,000-byte development target
remains exceeded by 14,001,008 bytes**. AndroidSubmission and independent APK
v2 signing pass. Unity 6000.3.20f1 completed release IL2CPP ARM64 at **19:35:26
Malaysia time**. SHA-256:
**DAAE6E64BE696721C3344CCDBB447E505D428537EF5A81047783E61DD751A12F**.

The complete Windows player built at **19:26:59** passes **172 Golf/club/room
checks** (134 rendered offline, 19 host, 19 client). It is promoted to
`Builds/WindowsFinal/`; all **313 files** match the retained release. The shared
APK and all **11 audit files** match as well. Current APK:
`Builds/Android/WhatTheFish-release.apk`; retained player/APK/audit and source
fingerprints: `Builds/GolfGripSlopeQA/Release/` and its parent.

The **2,953 captured primary inputs** include the concurrent aim-button/3x-ball
runtime. Reviewed isolated differences are generated Android URP registrations,
build-only clean packing, semantically equivalent Ball prefab local IDs, and a
later primary edit to the independent development-only ball-physics probe.
That probe edit is outside this player and does not change release gameplay.
The Ball prefab's canonical **14-object graph** matches. Original character
FBX bytes remain unchanged; the grip is derived at import. See
[packing audit](APK-SIZE-AUDIT.md#golf-closed-grip-and-raised-carry-angle--4-october-2026)
and [native checks](VERIFICATION.md#golf-closed-grip-and-raised-carry-angle--4-october-2026).
Phone rendering/touch/FPS and WAN remain unverified.

The replaced shared delivery is recoverable in
`Legacy/20261004-193744-641-golf-grip-delivery/` (**325 files, 682.27 MB**).
Superseded diagnostics are in
`Legacy/20261004-193144-915-golf-grip-trials/` (**106 files, 107.84 MB**); the
initial diagnostic player is in
`Legacy/20261004-190323-109-golf-grip-raw-player/` (**314 files, 589.32 MB**).
The redundant isolated final player is archived under the `golf-grip-player-copy`
batch recorded in `Builds/GolfGripSlopeQA/archive-player-copy.txt`.
Git staging, commit and push remain with the user.

## Golf ball visibility and free physics — 4 October 2026

Opening APK: **88,981,824 bytes**. The intervening palm-carry delivery was
**88,982,396 bytes**; the final measured physics release is **88,984,188 bytes**
(**+2,364** from task opening, **+1,792** from the preceding delivery).
Strict-limit headroom is **11,015,812 bytes**. The **75,000,000-byte development
target remains exceeded by 13,984,188 bytes**. This update adds no mesh,
texture, material, font, audio or package. Existing shared ball LODs remain
15,998 / 720 triangles; the far silhouette now stays rendered. Serialized
assets rise only **104 bytes**, to **155,278,460**, from development-probe
MonoScript metadata; that measurement is separate from APK length.

Current APK: `Builds/Android/WhatTheFish-release.apk`; exact retained APK and
fresh detailed audit: `Builds/GolfBallPhysicsQA/Release/`.
SHA-256: **48E35D9E094FE1DA24752C66DBC96EA735AB50B982D7CCF9261C79B076CE8BE6**.
Unity 6000.3.20f1 completed release IL2CPP ARM64 at **17:30:40 Malaysia time**.
AndroidSubmission, its strict size gate, fresh per-asset analysis and
independent APK v2 signature pass. The isolated build uses clean packing and
process-local JVM/temp settings. All **2,951 asset/package/settings inputs**
are captured and primary remained unchanged. Reviewed isolated differences
are generated Android URP registrations, the build-only clean-cache editor
option and equivalent Ball prefab local IDs; its canonical **14-object graph**
matches. No unexpected large packed dependency appeared. See
[packing details](APK-SIZE-AUDIT.md#golf-ball-visibility-and-free-physics--4-october-2026).

The complete Windows player built at **17:22:06 Malaysia time** is retained in
`Builds/GolfBallPhysicsQA/Release/Windows/` and promoted to
`Builds/WindowsFinal/`. All **313 files** match; the root APK also matches its
retained hash. It passes **22 final native visibility/physics checks** with all
UI overlays captured. The same gameplay sources pass **143 Golf/club/room
checks** and **203 cart regressions**. See
[measurements and verification scope](VERIFICATION.md#golf-ball-visibility-and-free-physics--4-october-2026).
Physical-phone touch/rendering/FPS and WAN remain unverified.

The replaced shared player/APK/audit is recoverable in
`Legacy/20261004-173632-475-golf-ball-physics-delivery/` (**327 files, 682.28 MB**).
Earlier task snapshots are in
`Legacy/20261004-172548-441-golf-ball-physics-snapshot/` (**326 files, 682.25 MB**);
superseded test records/backups are in
`Legacy/20261004-173123-168-golf-ball-physics-trials/` (**68 files, 72.30 MB**).
The first diagnostic player is in
`Legacy/20261004-163507-728-golf-ball-physics-diagnostic/` (**313 files, 589.33 MB**).
Current before/after evidence, input hashes, signatures and delivery comparisons
remain under `Builds/GolfBallPhysicsQA/`. Git publishing remains with the user.
The redundant isolated final player is recoverable in
`Legacy/20261004-173844-536-golf-ball-physics-player-copy/` (**313 files, 589.35 MB**);
the current tested player remains in the shared and retained release folders.

## Golf palm animation — 4 October 2026

Fresh carry-fix APK: **88,981,824 → 88,982,396 bytes (+572)**. Strict-limit
headroom is **11,017,604 bytes**; the **75,000,000-byte development target remains
exceeded by 13,982,396 bytes**. The change preserves the animated right wrist
and curls fingers in the palm's own frame. No art, animation asset or package
was added. Serialized assets remain **155,278,356 bytes**.

Current APK: `Builds/Android/WhatTheFish-release.apk`; exact retained copy:
`Builds/GolfPalmCarryQA/Release/WhatTheFish-release.apk`.
SHA-256: **D32D66D6D9AF7A71A549ACC36E02FDC78949AA09F1FB40F59AD921B24E525EF5**.
Unity 6000.3.20f1 completed release IL2CPP ARM64 at **16:15:54 Malaysia time**.
The prescribed AndroidSubmission target, strict size gate, fresh per-asset
analysis and independent APK v2 signature pass. The isolated checkout uses
clean packing and process-local JVM/temp settings. All **2,949 inputs** were
captured. Gameplay/art match primary; a subsequently added editor-only
`GolfBallBuilder.BuildPhysicsVerification` entry point and development-only ball
physics probe are outside this snapshot.
Generated URP registrations, the clean-cache editor option and equivalent Ball
prefab local IDs are reviewed isolated differences. The fourteen-object prefab
graph and external references match.

The complete Windows player built at **16:07:57 Malaysia time** is retained at
`Builds/GolfPalmCarryQA/Release/Windows/` and passes **142 Golf checks** with native
idle/walk/run/charge views inspected. Its `LATEST-BUILD.txt` identifies the exact
snapshot. After the old player closed, the complete validated player was promoted
to `Builds/WindowsFinal/`, with all **314 files** matching the retained release.
The matching APK and audit are updated in the shared Android/SizeAudit folders.
Reviewed superseded output is recoverable in
`Legacy/20261004-162407-373-golf-palm-delivery/` (**325 files, 682.08 MB**).
Exact fresh audits, signatures, input comparisons and current
evidence are under `Builds/GolfPalmCarryQA/`; the release audit is in
`Builds/GolfPalmCarryQA/Release/latest/`. See [functional verification](VERIFICATION.md#golf-palm-animation--4-october-2026).
Physical-phone quality/FPS and WAN remain unverified. Git publishing remains
with the user.

## Rounded Golf ball — 4 October 2026

Measured current APK: **89,418,634 → 88,981,824 bytes (−436,810)**.
Strict-limit headroom is **11,018,176 bytes**; the **75,000,000-byte development
target remains exceeded by 13,981,824 bytes**. The supplied rounded ball uses
geometric dimples, one plain shared material and **15,998 / 720** delivery
triangles. Its original 87,048-triangle FBX and editable master remain outside
Unity delivery. Removing the old normal-map dependency more than offsets the
new mesh; no new texture, shader, font, package or runtime physics system is
added. The combined snapshot also includes the current natural club carry.

Current artifact: `Builds/Android/WhatTheFish-release.apk`; exact retained copy:
`Builds/GolfBallRoundedQA/Release/WhatTheFish-release.apk`. SHA-256:
**54F69C76229C96F46D79035629D3AC7D135AC9284B3165D1C90F0E346DF1EC65**.
Unity 6000.3.20f1 completed release IL2CPP ARM64 at **14:42:09 Malaysia time**.
The prescribed `Tools/Build/Build.ps1 -Target AndroidSubmission`, strict size
gate and independent APK v2 signature pass. After a local Gradle loopback
failure, the existing isolated checkout completed the same target with clean
packing and process-local JVM/temp settings. All **2,949 captured
asset/package/settings inputs** match final primary gameplay/art sources;
expected differences are Android URP registrations, the ignored isolated
editor's clean-cache option and equivalent Ball prefab local object IDs.
The canonical **14-object prefab graph** matches. Captures, comparisons and
signing evidence are under `Builds/GolfBallRoundedQA/`.

Serialized assets total **155,278,356 bytes (−583,832)**, separate from APK
length. The ball mesh's independent compressed estimate increases **38,364 →
198,847 bytes (+160,483)**; the old normal map's **596,010-byte** estimate is
absent from the fresh packed assets. Material metadata decreases 13 estimated
bytes. Exact compressed ZIP entries shrink **436,645 bytes**, with another
**165 bytes** of container/signing reduction. The leading estimates remain
timber **2.247 MB**, character albedo **1.993 MB**, Golf structures **1.981 MB**,
character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected
large dependency appeared. Complete current/retained audits are in
`Builds/SizeAudit/latest/` and `Builds/GolfBallRoundedQA/Release/latest/`.

The complete matching Windows player was built at **14:23:37 Malaysia time**
and promoted to `Builds/WindowsFinal/`, including data, DLLs and launchers. It
passes **140 Golf checks**; the unchanged ball art also passes **20 equipment
checks**, with close, normal playing and both LOD views inspected. Original
physics dimensions, five-hole flow, ownership, strokes, recovery and authority
are preserved. See [source and verification](VERIFICATION.md#rounded-golf-ball--4-october-2026).
Physical-phone touch, rendering/FPS and WAN remain unverified.

Reviewed superseded player, portable-regeneration copy and one-off helpers are
recoverable in `Legacy/20261004-143815-451-golf-ball-rounded-trials/` (**324
files, 597.19 MB**). The replaced shared player/APK/audit are in
`Legacy/20261004-144741-052-golf-ball-rounded-final/` (**326 files, 682.62 MB**).
The earlier failed probe player is in
`Legacy/20261004-142138-879-golf-ball-probe-frame/` (**313 files, 589.33 MB**).
Superseded initial/failed suite captures and build diagnostics are in
`Legacy/20261004-145331-764-golf-ball-rounded-diagnostics/` (**28 files,
27.31 MB**).
Current evidence, exact releases, active assets and editable masters are
retained; caches and unrelated work are preserved. Git staging and publishing
remain with the user. Earlier entries describe historical snapshots.

## Golf club natural carry — 4 October 2026

Measured carry-task APK: **89,418,634 → 88,981,788 bytes (−436,846)**.
Strict-limit headroom is **11,018,212 bytes**; the **75,000,000-byte development
target remains exceeded by 13,981,788 bytes**. Carry changes reuse the existing
rig, club mesh and three maps. This combined snapshot also preserves concurrent
rounded-ball work, which removes the old ball normal-map dependency; the whole
package reduction is not a benchmark of the carry code alone.

Exact retained artifact: `Builds/GolfClubCarryQA/Release/WhatTheFish-release.apk`.
SHA-256:
**CF27A930CEF35A99675AACE86221DA8764602E4CB56472A96F6BAE44AA4BF9CE**.
Unity 6000.3.20f1 completed release IL2CPP ARM64 at **14:29:19 Malaysia time**.
The prescribed AndroidSubmission target, strict size gate and independent APK
v2 signature pass. The existing isolated checkout completed clean packing with
process-local JVM/temp settings. All **2,949 primary asset/package/settings
inputs** remain unchanged during packaging. Isolated differences are the clean
cache editor option, generated Android URP registrations and regenerated Ball
prefab local IDs; its canonical fourteen-object graph and external references
match the primary asset. Runtime/art sources match the validated Windows player.

Serialized assets total **155,278,356 bytes (−583,832)**, separate from APK bytes.
The concurrent ball mesh adds **115,396 serialized / 160,483 estimated bytes**;
the old ball normal map removes **699,212 serialized / 596,010 estimated bytes**.
Exact compressed ZIP entries shrink **436,680 bytes**, with **166 bytes** less
container/signing overhead giving the measured APK delta. Leading estimates
remain timber **2.247 MB**, character albedo **1.993 MB**, Golf structures
**1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**.
Club geometry and map estimates remain unchanged; no unexpected large dependency
appeared. Its exact complete audit is retained at
`Builds/GolfClubCarryQA/Release/latest/`.

The complete Windows player, built at **14:19:25 Malaysia time**, passes **140
Golf checks (103 offline, 19 host, 18 client)** and inspected native carry/swing
views. The exact tested player is retained at `Builds/GolfClubCarryQA/Release/Windows/`.
See [gait measurements and validation scope](VERIFICATION.md#golf-club-natural-carry--4-october-2026).
The newer rounded-ball release supersedes this snapshot in the shared delivery
folders. Its **14:23:37** Windows player includes this final carry implementation
and passes the same **140 Golf checks**, in
`Builds/GolfMiniGameQA/Run-20261004-142455/`. All **313 promoted files** match its
retained player. The shared APK now measures **88,981,824 bytes** (**−436,810**
from this task's opening release), with **11,018,176 bytes** of strict-limit
headroom and **13,981,824 bytes** above the development target. Its hash and fresh
`Builds/SizeAudit/latest/` reports are recorded in the rounded-ball entry above.
The newer complete shared release is preserved under the repository's concurrent
delivery rule. Physical-phone touch, rendering/FPS and WAN remain unverified.

Reviewed obsolete isolated output is recoverable in
`Legacy/20261004-134111-108-golf-hand-carry-isolated/` (**325 files, 683.05 MB**),
and trial output in `Legacy/20261004-142547-964-golf-hand-carry-trials/`
(**44 files, 38.96 MB**). The partial delivery batch
`Legacy/20261004-143258-918-golf-hand-carry-delivery/` contains only the **12
completed APK/audit moves (93,364,352 bytes)**; its Windows entries did not move
while the older game was open. That older player was subsequently archived in
the rounded-ball final batch listed above. An unchanged empty Git index mutex
was cleared after separate observations and exclusive-access checks, with the
actual index hash unchanged. Its one-file backup is in
`Legacy/20261004-145444-646-golf-hand-carry-mutex/`; integrity evidence remains in
`Builds/GolfClubCarryQA/`. Normal readiness checks pass. No staging, commit or
push was performed.

## Midnight Iron handheld club — 4 October 2026

Measured current APK: **88,787,790 → 89,418,634 bytes (+630,844)**. Strict-limit headroom is **10,581,366 bytes**. The **75,000,000-byte development target remains exceeded by 14,418,634 bytes**. This combined snapshot includes the already validated reference cart tyres: against that separate **88,963,010-byte** release, the club/gameplay update is **+455,624 bytes**. Original high-quality meshes, the supplied ZIP and 2048-pixel source maps remain outside Unity delivery; the handheld club uses three LODs, one shared material, two 512-pixel maps and a 256-pixel packed mask. No animation asset, font, shader or package was added.

Current artifact: `Builds/Android/WhatTheFish-release.apk`; exact retained copy: `Builds/GolfClubQA/Release/WhatTheFish-release.apk`. SHA-256: **49BC113AFE2C569D42CCB63154158294E6748C81AF309F0638279A71D357E679**. Unity 6000.3.20f1 completed release IL2CPP ARM64 at **13:12:04 Malaysia time**. The prescribed `Tools/Build/Build.ps1 -Target AndroidSubmission`, strict size gate and independent APK v2 signature pass. The open primary editor and caches were preserved; the existing isolated checkout completed the same target with clean packing and process-local JVM/temp settings. All **2,947 captured asset/package/settings inputs** stayed unchanged during final packaging and match the primary workspace except the ignored isolated editor's clean-cache build option. Final texture references are confirmed in the fresh packed reports.

Serialized assets total **155,862,188 bytes (+919,293)**, separate from APK length. The club's total independent compressed contribution estimate is **405,759 bytes**: mesh **123,767**, colour **129,848**, normal **131,740**, mask **18,413**, plus prefab/material/script metadata **1,991**. Exact compressed ZIP growth is **629,813 bytes** against the opening APK, or **454,593** against the tyre release; APK container/signing overhead accounts for the remaining **1,031 bytes**. The same leading estimates remain timber **2.247 MB**, character albedo **1.993 MB**, Golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. Cart mesh growth from the opening snapshot and the club maps/mesh explain the asset increase; no unexpected large dependency appeared. Complete fresh reports: `Builds/SizeAudit/latest/` and `Builds/GolfClubQA/Release/latest/`.

The complete matching Windows player was built at **13:04:30 Malaysia time** and promoted to `Builds/WindowsFinal/` with its data, DLLs and launchers; its 313 files match the retained delivery. It passes **113 Golf assertions**. The same gameplay sources pass **203 cart**, **509 football** and **28 basketball gameplay checks**, plus two basketball connection checks, before the final texture-import-only correction. Native carry, two-hand charge/contact/follow-through, first-person and running views and three model LODs were inspected. See [verification and source scope](VERIFICATION.md#midnight-iron-handheld-club--4-october-2026). Physical-phone touch, rendering/FPS and WAN remain unverified.

Reviewed superseded shared APK/audit/player, duplicate extractions and club import/swing/test trials are recoverable in `Legacy/20261004-131836-279-golf-midnight-iron-final/` (**422 files, 763.69 MB**). Current masters, delivery assets, before/after references, exact releases and useful validation evidence remain available; caches and unrelated work were preserved. Git staging and publishing remain with the user. Earlier entries describe historical snapshots.

## Golf cart reference tyres — 4 October 2026

Measured tyre-verification APK: **88,787,790 → 88,963,010 bytes (+175,220)**. Strict-limit headroom is **11,036,990 bytes**. The **75,000,000-byte development target remains exceeded by 13,963,010 bytes**. The larger tyre wall, chevron grooves and cream six-spoke rims reuse the existing atlas/material, with no new shipped texture, shader or runtime system. Mid/far axial simplification saves **1,600 / 960 triangles** against the full new profile; final cart totals are **33,958 / 13,805 / 6,721** with five active renderers per LOD.

Exact retained artifact: `Builds/GolfCartTreadQA/Release/WhatTheFish-release.apk`. SHA-256: **F496DE0E1B37750841EB36950244CB955754DA92253B23D26AF3336BD6A56486**. Unity 6000.3.20f1 completed release IL2CPP ARM64 at **11:55:46 Malaysia time**. The prescribed AndroidSubmission target, strict size gate and independent APK v2 signing pass. The primary batch attempt was blocked by the open editor; the existing isolated checkout completed the same target with clean packing and process-local JVM/temp settings. The open editor and caches were preserved.

This is a separate captured tyre release. Cart runtime/art/prefab inputs match the primary workspace. Android's URP resource registrations are generated, and the isolated editor uses task-specific Windows output and clean-cache options. After source capture, the workspace acquired Golf-club art/editor/runtime files, a `GolfClubBuilder.Prepare` call and subsequent player/network/Golf integration, material and probe changes. Those ongoing changes are outside this APK's verification scope; use matching captured builds for its multiplayer tests. The shared root APK/audit were preserved. Captured **2,925 inputs**, final source comparisons, additions, signatures and exact packing offsets are in `Builds/GolfCartTreadQA/`; the matching tested player is retained in its `Player/` folder.

Serialized assets total **155,187,375 bytes (+244,480)**, distinct from APK length. The cart FBX independent compressed estimate grows **736,970 → 909,743 bytes (+172,773)**. All other per-asset estimates and the three existing cart texture hashes are unchanged. Exact compressed ZIP growth is **175,220 bytes**, equal to the APK delta. The same leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. The increase is explained by the requested geometric tread and profile; no unexpected large dependency appeared. Complete fresh reports are in `Builds/GolfCartTreadQA/Release/latest/`.

The final Windows suite passes **203 cart assertions**, including measured rolling radii, steering/collision, forty sand-bowl crossings and two-player replication. Close, rolling, native and three-LOD views were inspected; original master/body geometry/UVs and texture bytes are preserved, with equivalent regeneration from another checkout path. See [verification](VERIFICATION.md#golf-cart-reference-tyres--4-october-2026). Phone quality/FPS, touch and WAN remain unverified. Superseded generation/render diagnostics are recoverable in `Legacy/20261004-120128-305-golf-cart-reference-tyre-trials/` (**3 files, 0.02 MB**); current before/after references, player and release remain under ignored Builds. Git staging and publishing remain with the user. Earlier entries describe historical snapshots.

## Golf cart rim fit and circularity — 4 October 2026

Measured APK: **88,713,562 → 88,787,790 bytes (+74,228)**. Strict-limit headroom is **11,212,210 bytes**. The **75,000,000-byte development target remains unmet by 13,787,790 bytes**. The release contains the equal circular rims and welded tyre beads, together with concurrent fixed axle supports, wheel rotation calibration and driving HUD changes; the package delta is a combined snapshot.

Artifact: `Builds/Android/WhatTheFish-release.apk`. SHA-256: **A8AFCC287E25A32D92694EE21D311B30ED23CF888692B2CB2BD3C75D92818D57**. Unity 6000.3.20f1 built release IL2CPP ARM64 at **02:18:57 Malaysia time**. AndroidSubmission's strict gate and independent APK v2 signature pass. The open primary editor prevented its batch build; the same target completed in this task's isolated checkout using clean packing and process-local JVM/temp settings. The captured **2,925 input files** and source comparisons are in `Builds/GolfCartRimQA/`. Runtime/art inputs match the working source; the URP global resource asset is regenerated for Android, and only the isolated editor's output/cache options differ.

Serialized assets total **154,942,895 bytes (+170,272)**, separate from APK length. The cart FBX estimate is **659,132 → 736,970 bytes (+77,838)**. All three existing texture hashes remain unchanged, with no new material, texture, shader or package. Continuous UV strips remove **6,288 position/UV splits (21,928 → 15,640)** across the twelve wheels. Mid/far spokes omit small corner bevels, saving **768 triangles per LOD**. The final snapshot is **4,560 APK bytes smaller** than the 88,792,350-byte intermediate; that comparison also includes concurrent vehicle changes and is not an isolated UV benchmark. Fresh estimates and ZIP deltas were calculated from this APK's own packing offsets.

Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected large dependency appeared. The final Windows cart suite passes **203 assertions**, with **142 island checks** and close/LOD views also verified. See [rim geometry, source integrity and validation scope](VERIFICATION.md#golf-cart-rim-fit-and-circularity--4-october-2026). Phone touch/rendering/FPS and WAN remain unverified.

Reviewed obsolete artifacts are recoverable in three batches: `Legacy/20261004-011055-897-golf-cart-rim-trials/` (**332 files, 603.52 MB**, superseded source backups, swatch experiments and first trial player); `Legacy/20261004-022544-106-golf-cart-rim-final/` (**316 files, 589.38 MB**, intermediate player and obsolete diagnostic helpers); and `Legacy/20261004-022754-456-golf-cart-rim-release/` (**12 files, 92.74 MB**, superseded shared APK/audit). Current comparison references, final player and validation evidence remain under ignored Builds. Active assets, original masters, caches and unrelated work were preserved. Git staging and publishing remain with the user. Earlier entries describe historical snapshots.

## Golf cart chassis connections and startup rolling — 4 October 2026

Measured combined snapshot: **88,708,082 → 88,782,546 bytes (+74,464)** from the task-opening APK, including the separately refined rim/bead model. Against the immediately preceding complete shared release, this update measures **88,787,118 → 88,782,546 bytes (−4,572)**. Strict-limit headroom is **11,217,454 bytes**. The **75,000,000-byte development target remains unmet by 13,782,546 bytes**; this is still a constrained development budget.

Current APK: `Builds/Android/WhatTheFish-release.apk`, with an exact retained copy at `Builds/GolfCartWheelContactQA/Release/WhatTheFish-release.apk`. SHA-256: **2C8425FC65A741B265894D8209F749A41D827721E11CF928F3984B97C74BE599**. Unity 6000.3.20f1 completed release IL2CPP ARM64 at **02:04:52 Malaysia time on 4 October**. The prescribed `Tools/Build/Build.ps1 -Target AndroidSubmission` target, strict size gate and independent APK v2 signing pass. The user's open Unity project remains open; the existing checkout with spaces ran the same target with clean packing cache and process-local TEMP/TMP/JVM settings. All **2,925 captured asset/package/settings inputs remain unchanged during final packaging and match the primary workspace**, except the isolated editor's clean-cache build option. A concurrent art re-export during the first package was retained, synchronized and rebuilt before this final delivery. Exact APK/audit/player, source manifests, signatures and comparison records are in `Builds/GolfCartWheelContactQA/`.

Four fixed axles and upright mounts join wheel hubs to the chassis in all three LODs. Totals are **28,838 / 12,205 / 5,761 triangles**, adding only **288 / 224 / 160** support triangles. Five renderers per LOD and one shared material remain; original high-quality geometry/UVs and delivery map hashes are unchanged. No texture, font, shader or package is added. Final serialized assets measure **154,942,895 bytes (−84,608)** against the preceding release; this is separate from APK size. Cart mesh packing decreases **84,612 serialized bytes**, and its independent compressed estimate decreases **738,348 → 736,970 bytes (−1,378)**; the rolling-scale prefab field adds four serialized/estimated bytes. Cart maps and material estimates remain unchanged. Exact compressed ZIP entries shrink **4,570 bytes**, chiefly cart mesh data **−2,479** and IL2CPP **−2,086**; container/signing overhead shrinks two bytes, giving the measured **−4,572-byte APK** change. Leading estimates remain timber 2.247 MB, character albedo 1.993 MB, Golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. No unexpected large dependency or avoidable texture growth appeared. Current complete audit: `Builds/SizeAudit/latest/`; exact retained audit: `Builds/GolfCartWheelContactQA/Release/latest/`.

The matching Windows development player passes **203 cart checks (159 offline, 22 host, 22 client)**. These include 28 new checks for measured tyre radius, 10/25/50 cm starts, reverse and stationary tilt/target-speed behavior, plus existing camera, pose, forty sand-bowl crossings, collision, authority and replication checks. Format-aware art audits pass in the primary and alternate checkout, with close/far/native/full-steering views inspected. **Wheel Roll Scale = 0.85** is intentional visual calibration; 1.0 restores the tyre-circumference ratio. See [verification and exact scope](VERIFICATION.md#golf-cart-chassis-connections-and-startup-rolling--4-october-2026). Physical-phone touch, rendering/FPS and WAN remain unverified.

Reviewed superseded shared APK/audit/player, first export/package/probe, failed generation logs and source backups are recoverable in `Legacy/20261004-020924-883-golf-cart-wheel-contact/` (**378 files, 814.16 MB**). An unchanged empty Git mutex was confirmed exclusively openable across separate inspections, then removed with the index hash unchanged; its backup is in that batch. A later transient concurrent lock cleared independently, and the readiness retry passed. Active art, original masters, Unity caches and unrelated work were preserved. Git staging and publishing remain with the user. Earlier entries describe historical snapshots.

## Golf cart elevated steering follow — 4 October 2026

Measured APK: **88,708,082 → 88,787,118 bytes (+79,036)**. Strict-limit headroom is **11,212,882 bytes**. The **75,000,000-byte development target remains unmet by 13,787,118 bytes**. Elevated camera following adds no art, texture, font, shader or package. This combined snapshot also includes the separately updated cart rim/bead mesh and sand-bowl motor fix; the whole package delta is not the cost of the camera change alone.

Artifact: `Builds/Android/WhatTheFish-release.apk`, with an exact retained copy at `Builds/GolfCartOverheadQA/Release/WhatTheFish-release.apk`. SHA-256: **3FDC1DDE408244001008F76556BE825B476CDAB9F3BBC9B9F6423068D1649A7B**. Unity 6000.3.20f1 built release IL2CPP ARM64 at **01:06:24 Malaysia time on 4 October**. AndroidSubmission's strict limit and independent APK v2 signing pass. All **2,925 captured asset/package/settings inputs remain unchanged during packaging and match the primary workspace**, except the isolated editor's clean-cache build option. Source manifests/comparisons, signatures and packing comparisons are in `Builds/GolfCartOverheadQA/`.

The primary script attempt exited while the user's Unity editor held the project. The existing independent checkout completed the same `Tools/Build/Build.ps1 -Target AndroidSubmission` target with clean packing cache and process-local TEMP/TMP/JVM settings. The open editor, original caches, dependencies and unrelated changes were preserved. A full Windows build validates the currently captured art and camera sources before Android packaging.

Serialized assets total **155,027,503 bytes (+254,880)**, separate from APK bytes. The separate rim/bead model accounts for the entire serialized increase; its estimate rises **659,132 → 738,348 bytes (+79,216)**. Cart textures and material estimates remain unchanged. Exact compressed ZIP entries grow **79,039 bytes**, including cart mesh data **+78,860** and IL2CPP **+178** for the combined motor/camera code. Container/signing overhead changes by **−3 bytes**, yielding the measured APK delta. No unexpected large dependency or avoidable camera asset growth was found. Leading estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. Current complete audit is in `Builds/SizeAudit/latest/`; the exact release's audit is retained in `Builds/GolfCartOverheadQA/Release/latest/`.

The matching Windows player passes **175 cart checks** (131 offline, 22 host, 22 client) and **81 Golf gameplay checks**. Elevated cart/camera turns match **+9.20° / +9.20°**, **−8.80° / −8.80°**, and **−18.53° / −18.53°** on the driving client. Manual view offsets, three camera modes, reverse behavior, exit, collision, all real sand bowls, authority and replication pass; the elevated native view was inspected. See [evidence and scope](VERIFICATION.md#golf-cart-elevated-steering-follow--4-october-2026). Phone touch, rendering/FPS and WAN remain unverified.

Reviewed old shared APK/audit/player and task baseline/source copies are recoverable in `Legacy/20261004-011144-554-golf-cart-overhead/` (**352 files, 862.45 MB**). No active art, Unity cache or unrelated QA was archived; Git staging and publishing remain with the user. Earlier entries describe historical snapshots.

## Golf cart sand-bowl rim traversal — 4 October 2026

Vehicle-fix APK: **88,625,838 → 88,708,270 bytes (+82,432)** across this task's combined snapshot. Relative to the complete 88,708,082-byte Golf mini-game release, the rim-traversal code adds **188 bytes**. Strict-limit headroom is **11,291,730 bytes**; the **75,000,000-byte development target remains unmet by 13,708,270 bytes**. The motor fix adds no art, texture, material, shader or package.

Artifact: `Builds/GolfCartRoundFixQA/Release/WhatTheFish-release.apk`. SHA-256: **144B87D8DD87E8AA3EC1775F51EF28D999C2201205E66FCC26492CC776D6CDF4**. Unity 6000.3.20f1 built release IL2CPP ARM64 at **00:35:14 Malaysia time on 4 October**. The prescribed AndroidSubmission target and independent APK v2 signing pass. This separate delivery preserves the other task's shared APK. All **2,925 captured asset/package/settings inputs** match before packaging apart from the isolated editor's clean-cache option. A subsequent parallel **wheel-rim/bead mesh refinement at 00:40** changes the primary FBX after this snapshot; it is outside this APK and Windows player's verification scope. Motor, pose, networking, Golf rules and UI inputs still match. Source comparisons and signatures are in `Builds/GolfCartRoundFixQA/hill-*`.

Serialized assets remain **154,772,623 bytes**, with **zero asset growth against the preceding complete Golf release**. The cart's captured mesh estimate remains **659,132 bytes** and its colour/normal/mask maps and material remain unchanged. Exact compressed ZIP growth against that release is **189 bytes**, chiefly IL2CPP **+188**; container overhead accounts for the one-byte difference from total APK growth. Against the task-opening release, serialized assets grow **62,952 bytes** and exact compressed entries grow **82,430 bytes**; the circular tyres account for **37,071 estimated bytes** while concurrent Golf/camera code contributes to native growth. Leading art estimates remain timber 2.247 MB, character albedo 1.993 MB, Golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. No unexpected large shipped dependency appeared. Complete fresh packing/contribution records are in `Builds/GolfCartRoundFixQA/Release/latest/`; comparisons are in `hill-package-comparison.json` and `hill-*-entry-deltas.csv` within the task directory.

The fresh vehicle Windows player passes **169 checks** (127 offline, 21 host, 21 client), including **40 real sand-bowl crossings** in four directions and forward/reverse, plus existing walls, twelve shoreline directions, seating, wheel LODs and host/client replication. Evidence is `Builds/GolfCartQA/Run-20261004-002512/`; the complete tested player is in `Builds/GolfCartRoundFixQA/Release/Windows/`. See [reproduction and validation](VERIFICATION.md#golf-cart-sand-bowl-rim-traversal--4-october-2026). The successful retry uses the same existing isolated checkout and process-local TEMP/TMP/JVM settings described below. No new disk-space recovery was needed. Phone touch, rendering/FPS and WAN remain unverified.

The superseded seating/tyre-only release and empty stale-mutex backup are recoverable in `Legacy/20261004-004451-752-golf-cart-rim-delivery/` (**326 files, 681.49 MB**). Earlier trials/source backups remain in `Legacy/20261003-235959-283-golf-cart-round-seated-cleanup/`, with the inspection helper in `Legacy/20261003-220432-659-golf-cart-inspect-helper/`. The abandoned empty Git index lock was confirmed unchanged and exclusively openable, then removed without altering the index or history. Caches, source masters, parallel wheel work and shared deliveries were preserved. Earlier entries below describe historical captured snapshots.

## Golf mini game — 4 October 2026

Measured APK: **88,625,838 → 88,708,082 bytes (+82,244)**. Strict-limit headroom is **11,291,918 bytes**. The **75,000,000-byte development target remains unmet by 13,708,082 bytes**. Golf gameplay reuses the existing course, equipment ball meshes/materials, font and native UI geometry; it adds no imported art, texture, shader or package. This combined snapshot also includes the separate camera and circular-tyre work, so the whole APK delta is not the isolated cost of Golf rules.

Artifact: `Builds/Android/WhatTheFish-release.apk`, with an exact retained copy at `Builds/GolfMiniGameQA/Release/WhatTheFish-release.apk`. SHA-256: **B2B7B27CD2541D9EB0AC157E006081C85618143D568BE69B8747BDB723FBF6B3**. Unity 6000.3.20f1 built release IL2CPP ARM64 at **00:23:04 Malaysia time on 4 October**. AndroidSubmission's strict gate and independent APK v2 signing pass. All **2,925 captured asset/package/settings inputs remain unchanged during packaging**. Golf gameplay, player, networking and UI inputs match the workspace. A separate ongoing cart-rim task subsequently changed the cart FBX, `GolfCartMotor` and the development-only `GolfCartProbe`; those later edits are outside this captured release. The isolated editor's clean-cache option is the only intentional build-source difference. Input manifests and comparisons are in `Builds/GolfMiniGameQA/`.

The prescribed primary build attempt exited with the user's editor holding the project. The independent checkout completed the same `Tools/Build/Build.ps1 -Target AndroidSubmission` target with clean packing cache and process-local TEMP/TMP/JVM settings. An earlier Windows retry ran out of disk space; NTFS compression retained this task's copied Library, storing **8,372,093,269 logical bytes in 4,725,265,238 bytes**. Original caches, dependencies, source assets and the open editor were preserved. Compression changes neither logical source content nor APK bytes.

Serialized assets total **154,772,623 bytes (+62,952)**, separate from APK bytes. The concurrent circular-tyre mesh explains the main asset increase: **622,061 → 659,132 independently compressed estimated bytes (+37,071)**, with unchanged cart texture maps. Exact compressed ZIP entries grow **82,241 bytes**: IL2CPP **+44,396**, cart mesh data **+36,476**, metadata **+6,198**, and Burst **−5,657** are the main changes. Container/signing overhead accounts for the remaining **3 bytes**. Golf's tiny MonoScript records are metadata, not estimates of compiled code cost; development fixture behavior is absent from the release's generated C++.

Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected large dependency appeared. Complete fresh packing is in `Builds/SizeAudit/latest/`; this exact release's audit is also retained in `Builds/GolfMiniGameQA/Release/latest/`. APK entry and asset-estimate comparisons are in `Builds/GolfMiniGameQA/`.

The current Windows player passes **81 Golf gameplay, 509 football, 142 Golf map, 128 cart and 67 basketball checks**. The rendered leaderboard and target/ball indicators were inspected. See [exact scope and evidence](VERIFICATION.md#golf-mini-game--4-october-2026). Physical-phone touch, rendering/FPS and WAN remain unverified. Reviewed old player/APK/audit, task source backups, superseded probes and failed build/precision/fixture attempts are recoverable in `Legacy/20261004-002911-162-golf-mini-game/` (**382 files, 868.08 MB**). No active art or Unity cache was archived. Earlier release records below are historical snapshots.

## Golf cart driving cameras — 3 October 2026

Measured APK: **88,625,838 → 88,713,562 bytes (+87,724)**. Strict-limit headroom is **11,286,438 bytes**. The **75,000,000-byte development target remains unmet by 13,713,562 bytes**. Camera following adds no art, texture, shader or package. This combined snapshot also includes concurrent golf gameplay and cart seating/circular-tyre changes; the package increase is not the cost of camera following alone.

Artifact: `Builds/Android/WhatTheFish-release.apk`. SHA-256: **D953A218545FC7F01953901B1512ADE2958A543A8F2F4DE6EE1A8AFE27412993**. Unity 6000.3.20f1 built release IL2CPP ARM64 at **23:44:05 Malaysia time**. AndroidSubmission's strict gate and independent APK v2 signing pass. The **2,925-file input snapshot** remains unchanged inside the build checkout. Camera/vehicle inputs match the primary workspace. Two subsequent parallel changes, `GolfBall.ResetSequence` reception and the development-only `GolfMatchProbe`, are outside this captured APK; this record verifies the camera change and does not certify ongoing golf work. Input comparisons, signatures and current test evidence are in `Builds/GolfCartCameraQA/`; complete packing records are in `Builds/SizeAudit/latest/`.

Serialized assets total **154,772,623 bytes (+62,952)**, separate from APK bytes. The concurrent tyre refinement raises the cart mesh independent estimate **622,061 → 659,132 bytes (+37,071)**; its colour/normal/mask maps remain unchanged. Exact compressed entry growth is **87,725 bytes**: IL2CPP **+44,653**, cart mesh data **+36,476** and metadata **+6,198** account for most of it. The one-byte difference from total APK growth is container overhead. Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected large shipped dependency appeared.

The primary build attempt exited on the open editor. The isolated checkout completed the same script/target with clean packing cache and process-local TEMP/TMP/JVM settings. A later attempt ran out of disk space; retaining and applying NTFS compression to this task's copied Library stored **8,300,495,635 logical bytes in 4,823,223,361 bytes**. Original caches, installed dependencies and source assets were preserved; inputs were rechecked before the successful retry. Compression does not change logical APK bytes or asset content.

The validated camera player in `Builds/GolfCartCameraQA/Player/` passes **128 cart checks** and **142 golf scene checks**, with native reverse/forward/turn/elevated views inspected. See [camera measurements and exact scope](VERIFICATION.md#golf-cart-driving-cameras--3-october-2026). Phone touch, clipping/rendering/FPS and WAN remain unverified. Reviewed old APK/audit, intermediate snapshot, source backups and failed comparisons/builds are recoverable in `Legacy/20261003-235321-846-golf-cart-camera/` (**60 files, 210.06 MB**). Earlier records below are historical snapshots.

## Golf cart seating and circular tyres — 3 October 2026

Historical vehicle-validation APK: **88,625,838 → 88,707,818 bytes (+81,980)**, leaving **11,292,182 bytes** below the strict 100,000,000-byte limit. The **75,000,000-byte development target was unmet by 13,707,818 bytes**. This captured release is now recoverable at `Legacy/20261004-004451-752-golf-cart-rim-delivery/Builds/GolfCartRoundFixQA/Release/WhatTheFish-release.apk`; shared deliveries were preserved. SHA-256: **08F24EA6CFCB41F230EB340C2089E329449A5B9FED850AA0D1099E21DFCEEB07**. Unity 6000.3.20f1 built release IL2CPP ARM64 at **23:48:50 Malaysia time on 3 October**. AndroidSubmission's strict gate and independent APK v2 signature pass.

The captured **2,925-file** asset/package/settings manifest and comparison records are in `Builds/GolfCartRoundFixQA/`. Vehicle art, seat, pose and camera inputs matched the workspace at capture. Parallel changes to `GolfBall`, `GolfMatchState` and the development-only `GolfMatchProbe` during packaging are outside this APK's captured scope; this record validates the vehicle changes rather than subsequent golf rules. The isolated editor's clean-cache build option is the only intentional source difference. The primary build attempt stopped on the open editor; the existing isolated checkout completed the same `Tools/Build/Build.ps1 -Target AndroidSubmission` command after resynchronizing incomplete parallel gameplay inputs.

Serialized assets measure **154,772,623 bytes (+62,952)**, separate from APK bytes. The circular-tyre cart FBX rises **674,932 → 736,800 serialized bytes (+61,868)** and **622,061 → 659,132 independently compressed estimated bytes (+37,071)**. The existing colour/normal/mask textures and material are unchanged. Delivery geometry totals **25,833 / 9,686 / 3,748 triangles**, using 64/40/24 concentric tyre segments while retaining authored hubs and the original high-quality master. No new texture, shader or package was added. Exact compressed ZIP entry growth is **81,977 bytes**, including IL2CPP **+44,132**, metadata **+6,198** and Unity native code **+319**; these native increases also include parallel golf/camera work. Leading art estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. This snapshot's packing and contribution records are recoverable at `Legacy/20261004-004451-752-golf-cart-rim-delivery/Builds/GolfCartRoundFixQA/Release/latest/`.

A native-link retry exhausted disk space. NTFS compression of three already-reviewed, ignored historical cart archive batches freed **636,760,064 physical bytes**, preserving all **724 files' logical sizes and SHA-256 hashes**. No original Unity cache, installed dependency or source asset was removed. Process-local TEMP/TMP used `Builds/JdkTmp/`; the subsequent submission build succeeded. Recovery evidence is in `disk-recovery.json` and `compressed-archive-integrity.txt` within the task evidence directory.

The captured Windows vehicle player passes **128 assertions** (86 offline, 21 host, 21 client), including seated roof clearance, both character LODs and all cart wheel LODs. Snapshot evidence is `Builds/GolfCartQA/Run-20261003-231835/`; its tested complete player is recoverable in `Legacy/20261004-004451-752-golf-cart-rim-delivery/Builds/GolfCartRoundFixQA/Release/Windows/`. See [pose, wheel and portable-art verification](VERIFICATION.md#golf-cart-seating-and-circular-tyres--3-october-2026). Phone touch, rendering/FPS and WAN remain unverified. Superseded source backups, old delivery/audit and trial runs are recoverable in `Legacy/20261003-235959-283-golf-cart-round-seated-cleanup/` (**98 files, 184.87 MB**); an obsolete inspection helper is in `Legacy/20261003-220432-659-golf-cart-inspect-helper/`.

## Golf cart rear-wheel rendering and steering — 3 October 2026

Actual APK: **88,625,346 → 88,625,838 bytes (+492)**. Strict-limit headroom is **11,374,162 bytes**. The **75,000,000-byte development target remains unmet by 13,625,838 bytes**. The rear hub repair preserves original face winding rather than recalculating inward normals on open wheel parts. Steering increases to 120°/s with a 38° front-wheel limit, and equal stick strength gives equal straight/turning throttle. No package, texture, material or extra geometry was added.

Current artifact: `Builds/Android/WhatTheFish-release.apk`. SHA-256: **F4646C546AD3248D8AE7FA637351EE20C257114C3FDB3C66B77CAB41D51D15F2**. Unity 6000.3.20f1 built release IL2CPP ARM64 at **21:34:38 Malaysia time**. The prescribed main-project AndroidSubmission attempt exited on the open editor; the same script/target passed in the matching-source isolated checkout using clean build cache and process-local TEMP/TMP in `Builds/JdkTmp/`. The strict budget gate and independent APK v2 signing pass. All **2,890 publishable asset/package/settings hashes** match, except isolated editor output/clean-cache options. Current packing records are in `Builds/SizeAudit/latest/`; comparisons, signatures, source manifests and art checks are in `Builds/GolfCartHandlingQA/`.

Serialized assets measure **154,709,671 bytes (+52)**, separate from APK bytes. Cart mesh independent compression estimate falls **622,153 → 622,061 bytes (−92)**; colour/normal/mask estimates are unchanged at 498,110/621,270/71,311 bytes. Exact compressed ZIP growth is 492 bytes, including IL2CPP **+556**; metadata and Unity native code remain unchanged. Total mobile triangles remain 24,000/8,000/2,500 and source geometry/UV/texture hashes are unchanged. Leading art estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. No unexpected dependency or art growth appeared.

The fresh desktop player passes **90 cart assertions** (56 offline, 17 host, 17 client). Measured straight/full-turn speed is **17.000/17.000 m/s**, half-strength straight/turn is **8.500/8.500**, and reverse turning remains **8.000**. All twelve wheel/LOD hub orientations point outward; actual three-LOD views show the restored rear pattern. Evidence: `Builds/GolfCartQA/Run-20261003-212527/`. Current desktop delivery is in `Builds/WindowsFinal/`; phone touch, rendering/FPS and WAN remain unverified. Reviewed old deliveries and wrong-normal art backups are recoverable in `Legacy/20261003-213949-431-golf-cart-render-steering/`. Source masters, caches and unrelated work were preserved. Earlier records below are historical snapshots.

## Golf cart speed and wheels — 3 October 2026

Current APK: **88,625,346 bytes**, leaving **11,374,654 bytes** under the strict 100,000,000-byte ceiling. The **75,000,000-byte development target remains unmet by 13,625,346 bytes**. Task-opening APK: **86,896,050 bytes**; a concurrent base-cart release measured **88,756,434 bytes**. The final combined release is **131,088 bytes smaller than that base-cart package** and **1,729,296 bytes larger than the task-opening package**. This comparison includes concurrent course refinement, not just wheel animation. The immediately preceding combined APK was **88,624,542 bytes**; restoring the tested slope-heading stability adds **804 bytes**.

Current artifact: `Builds/Android/WhatTheFish-release.apk`. SHA-256: **6FF7DD3020809D03BE903CB50FC7ECBC87FD6E6B02BF21EAAABE69C20BDB39DC**. Unity 6000.3.20f1 produced release IL2CPP ARM64 at **20:58:58 Malaysia time**. The prescribed main-project AndroidSubmission attempt exited on the user's open editor; the same script/target passed in a matching-source isolated checkout with clean build cache for full packing offsets and process-local TEMP/TMP under `Builds/JdkTmp/`. The strict size gate and independent APK v2 signing passed. All **2,890 publishable asset/package/settings input hashes** match except the isolated editor's output/clean-cache options. The final 84 runtime C# hashes also match the successfully tested Windows player.

Serialized assets total **154,709,619 bytes**, separate from APK size. Separating the existing wheels retains **24,000 / 8,000 / 2,500 total triangles**, one shared material and unchanged colour/normal/mask textures. The cart FBX estimate falls **632,112 → 622,153 bytes (−9,959)**; its prefab estimate grows **2,246 bytes**. No new wheel texture, shader or package was introduced. The unrelated course refinement accounts for most of the remaining package reduction. Largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected large dependency was found.

The corrected player passes **87 cart assertions** and a separate **142 golf assertions**; measured forward/reverse speeds are **17.000 / −8.000 m/s**. Turning, wheel roll/reverse/stop, all three LOD views, collisions, slope parking, cameras, shared travel and two-player replication pass. Source/master UV integrity and relative Blender/FBX dependencies were checked from a second checkout and unrelated working directory. Current evidence: `Builds/GolfCartWheelsQA/`, `Builds/GolfCartQA/Run-20261003-202128/` and `Builds/GolfG2QA-20261003-202344/`; current complete packing records: `Builds/SizeAudit/latest/`. The validated desktop player is in `Builds/WindowsFinal/`. Physical-phone touch, rendering/FPS and WAN remain unverified.

An intermediate APK containing a concurrent rollback of the slope fix is recoverable in `Legacy/20261003-205405-553-golf-cart-wheels-intermediate/`. Replaced shared deliveries, old static-wheel asset backups, rejected geometry trials and superseded failed probes are in `Legacy/20261003-210419-502-golf-cart-wheels-delivery/`. Active masters, caches, dependencies and unrelated source work were preserved. Earlier records below are historical source/build snapshots with their original scope.

## Golf course refinement and current combined release — 3 October 2026

Actual APK: **86,896,050 → 88,624,542 bytes (+1,728,492)**. Strict-limit headroom is **11,375,458 bytes**. The **75,000,000-byte development target remains unmet by 13,624,542 bytes**. The five-hole refinement uses the original aqua material and terrain colours, spreads the holes across the island and enlarges cup diameter to 0.285 m. The combined release also includes the separate protocol 19 cart speed/wheel updates; the APK increase is not the cost of the course refinement alone. See [course authoring](GOLF-COURSE.md) and [cart validation](VERIFICATION.md#golf-cart-speed-and-wheels--3-october-2026).

Current artifact: `Builds/Android/WhatTheFish-release.apk`. SHA-256: **9D2AD84E7F8211681B4AFF14FE945B8B3A9F4EA18CF6ABCC91DFE840816CDAFF**. Unity 6000.3.20f1 produced release IL2CPP ARM64 at **20:28:02 Malaysia time**. AndroidSubmission's strict budget gate and independent APK v2 signing passed. All **2,903 authored asset/package/settings input hashes** match the primary workspace; the isolated editor differs only by its Android clean-cache option. Generated Performance Test resources are excluded from authoring hash comparisons. Current packing/ZIP/contribution records are in `Builds/SizeAudit/latest/`; source manifests, comparisons, renders and signing evidence are in `Builds/GolfCourseRefinementQA/`.

The prescribed main-project attempt exited on the open Unity project. The matching-source isolated checkout completed the same script/target. A later Gradle daemon exhausted native memory; the successful retry set only process-local TEMP/TMP to `Builds/JdkTmp/` and JAVA_TOOL_OPTIONS to two active processors/two Gradle workers, sequential execution, no persistent daemon and a 32 GB JVM heap base. No user or game-quality setting changed. The shorter ignored checkout path avoids Mono's long-path cache errors.

Serialized assets total **154,709,619 bytes (+2,598,806)**, a separate measurement from APK bytes. Course asset estimates fall **652,179 → 610,202 bytes (−41,977)**; the original terrain estimate falls another **80,439 bytes**, for a **122,416-byte reduction** in their combined independent compression estimates. Five precise derived terrain sectors retain small cup boundaries. Removing the coloured green meshes/materials avoids added turf textures; shared aqua, poles, numbers, floors and tees reuse existing art. Neither baked golf grass nor the removed overlays/materials is packed. Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. New cart art accounts for the principal combined-source growth; no unexpected course dependency appeared.

Course checks passed **235 editor assertions**, **78 saved-scene assertions**, **142 mobile-geometry Windows assertions**, **142 later desktop assertions**, and **13,122 terrain-reference samples**. Close/distant and cup-detail views were inspected. The map/grass inputs remain identical through later cart-only source updates; the cart's separate final tests are linked above. Physical-phone touch, quality/FPS, WAN latency and golf shot/scoring behavior remain unverified.

Unused course overlays/materials and old grass/terrain derivatives are recoverable in `Legacy/20261003-192351-292-golf-course-refine-unused/`. Superseded intermediate packages are in `Legacy/20261003-194837-732-golf-refine-first-package/` and `Legacy/20261003-200907-120-golf-refine-second-package/`. The replaced shared APK/audit/log is in `Legacy/20261003-204804-232-golf-refine-current-release/`; reviewed backups, duplicate captures and superseded diagnostics are in `Legacy/20261003-204959-483-golf-refine-task-cleanup/`. Final cleanup records are retained in the task QA folder. The map validation players remain in the ignored task checkout and its recorded run folders; the separate cart task's player outputs are preserved. No art master, installed dependency or primary Unity cache was removed. Earlier records below are historical snapshots with their original scope.

## Golf cart summon and driving snapshot — 3 October 2026

Actual APK: **86,896,050 → 88,756,434 bytes (+1,860,384)**. Strict-limit headroom is **11,243,566 bytes**. The **75,000,000-byte development target remains unmet by 13,756,434 bytes**. The supplied cart gains owner-limited summon/recall, public empty-cart driving, parked persistence and Golf-only controls. See [gameplay and art](GOLF-CART.md).

This is the validated **protocol 18 base snapshot**, retained at `Builds/GolfCartQA/ReleaseBase/WhatTheFish-release.apk`; SHA-256 **C055ED1CEFCA2E8560D2954FEF00A72E0B173E76F14E56D661DD87BD54210FEE**. Concurrent tasks subsequently changed course assets and introduced protocol 19, doubled cart speed and animated wheels. Those later inputs are outside this snapshot's build and test scope. This record does not establish freshness of the shared `Builds/Android/` APK for those inputs.

The prescribed main-project AndroidSubmission attempt exited on the user's open Unity project. The same script/target succeeded in the existing ignored isolated checkout, using its Android clean-cache option for full packing offsets and process-local TEMP/TMP under `Builds/JdkTmp/`. Unity 6000.3.20f1 produced the final release IL2CPP ARM64 APK at **19:56:01 Malaysia time**. The strict budget gate passed; the retained final file was separately verified with APK v2 signing. Complete packing, recalculated per-asset estimates and ZIP entries are in `Builds/GolfCartQA/ReleaseBase/latest/`; exact comparisons and signature evidence remain in `Builds/GolfCartQA/`.

Serialized assets total **154,919,835 bytes (+2,809,022)**, a separate measurement from compressed APK bytes. Three cart LODs retain **24,000 / 8,000 / 2,500 triangles**, with shared 1024² colour/normal and 512² mask maps in ASTC. The original 246,850-triangle master, original ZIP and full font stay outside Unity assets. The eight-glyph font subset is **5,320 source bytes**, with its full OFL licence shipped. Independently compressed cart asset estimates total **1,830,717 bytes**. Existing leading art contributors remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected large dependency appeared.

The Windows base player passed **76 cart assertions**, including owner-only recall and public guest driving, plus football, basketball, jump, golf traversal and shared travel regressions. Portable source dependencies and close/distant cart views were inspected. See [verification scope](VERIFICATION.md#golf-cart-summon-and-driving-snapshot--3-october-2026). Physical-phone touch, rendering/FPS and WAN latency remain unverified.

Reviewed obsolete players, APK/audit output and the replaced nonportable FBX are recoverable in `Legacy/20261003-192438-574-golf-cart-player-baseline/`, `Legacy/20261003-194210-309-golf-cart-apk-baseline/` and `Legacy/20261003-195112-220-golf-cart-portable-fbx/`. Superseded pre-licence packaging and completed path-rewrite/relocation fixtures are in `Legacy/20261003-202644-582-golf-cart-base-cleanup/`. Active masters, caches, installed dependencies and other tasks' outputs remain in place. Earlier entries below retain their original source/build scope.

## Five-hole golf map — 3 October 2026

Actual APK: **86,563,288 → 86,896,050 bytes (+332,762)**. Strict-limit headroom is **13,103,950 bytes**. The **75,000,000-byte development target remains unmet by 11,896,050 bytes**. Five new numbered flags, recessed cups, putting greens and tees are included in the streamed golf scene. See [course layout and authoring](GOLF-COURSE.md).

The prescribed main-project AndroidSubmission attempt exited while the user's Unity editor held the project open. The same script/target built the matching-source isolated project successfully, using its Android clean-cache option for complete packing offsets. TEMP/TMP were set only within the build processes to ignored `Builds/JdkTmp/`. Unity 6000.3.20f1 produced the final release IL2CPP ARM64 APK at **17:14:07 Malaysia time**; the strict budget gate and APK v2 signature verification passed.

Current artifact: `Builds/Android/WhatTheFish-release.apk`. SHA-256: **977703634DD39FC91717CED426B67AC424EEA49F3105E27CD5789FC12F5C471A**. Complete current packing/ZIP/contribution records are in `Builds/SizeAudit/latest/`; source hashes, signature results, captures and comparisons are in `Builds/GolfFiveHolesQA/`. All authored asset inputs match the build checkout; only the isolated editor's clean-cache build option differs. Serialized assets total **152,110,813 bytes (+896,940)**, a separate measurement from APK size.

The four derived terrain sectors and five greens use existing textures and shader families. Shared pole, pennant, cup-floor, tee and number meshes avoid repeated assets. New course assets contribute about **652,179 independently compressed bytes**, offset by a **320,128-byte** reduction in the original terrain estimate; these estimates are not exact ZIP allocation. Inspection caught an unnecessary inherited turf metallic mask: removing it reduced the trial APK from **87,045,510 to 86,896,050 bytes (−149,460)** while preserving colour/normal detail. Neither this mask nor baked golf grass is packed in the final APK. Largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**.

The editor scene/prefab review passed **160 assertions**, the mobile-geometry Windows preview passed **142 golf assertions**, and the final desktop player passed **142**. The unchanged analytic terrain function also passed **13,122 authoring samples**. Physics openings, inner walls, recessed floors, grounded tees, materials, shoreline containment, fairway/bunker traversal and island switching were checked; close and distant renders were inspected. Physical-phone touch, quality/FPS and golf shot/scoring behavior are outside this map-only validation. No imported Blender/FBX master changed. The current desktop player is retained in `Builds/WindowsFinal/`.

The obsolete first-layout terrain mesh is recoverable in `Legacy/20261003-165821-340-golf-five-holes-trial-mesh/`. Old players/APKs, the first masked-material package, root audit output and trial diagnostics are archived in `Legacy/20261003-171933-414-golf-five-holes-delivery/`. No active master, cache or installed dependency was moved. Earlier entries below are historical source/build snapshots.

## Football animation rebuild — 4 October 2026

Measured release: **86,764,912 → 86,774,768 bytes (+9,856)**. Strict-limit headroom: **13,225,232 bytes**. The **75,000,000-byte development target remains unmet by 11,774,768 bytes**. The baseline is the actual local APK recorded before this task; no new mesh, texture, imported clip, audio or package was added.

The prescribed main-project `Tools/Build/Build.ps1 -Target AndroidSubmission` passed with Unity 6000.3.20f1, release IL2CPP ARM64, at **22:49 Malaysia time**. Process-local TEMP/TMP used ignored `Builds/JdkTmp/`. The strict budget gate and APK v2 signature verification passed. Root output: `Builds/Android/WhatTheFish-release.apk`; preserved task copy: `Builds/FootballMotionPolishQA/Android/WhatTheFish-release.apk`. SHA-256: **4218128125E5ADDD4E65361CFA904A378595567438408F2FF375E36A400FEEB8**.

The rebuilt gait and FK/IK presentation share the existing character. The 28-record posture/action resource is **13,108 serialized bytes / 1,854 independently compressed bytes**, versus 11,548 / 1,574 previously. Actual ZIP changes include IL2CPP **+9,778**, metadata **+1,181** and Burst **−1,530 bytes**; resource and container changes make up the remainder. The largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected art growth or new shipped dependency appeared. Shared source clips remain used by other sports.

Fresh populated packing offsets, per-asset estimates, ZIP comparisons, signature output and exact before/after measurements are retained under `Builds/FootballMotionPolishQA/FinalSizeAudit/` and the task evidence folder. Serialized assets total **151,510,277 bytes**, a separate measurement from the APK. The 32 checked football runtime/resource/editor/review inputs are unchanged between the verified Windows motion run and Android build. Subsequent turn-probe corrections are entirely development-only and do not affect the release player.

See [motion implementation](FOOTBALL-ANIMATION.md) and [validation, visual evidence and cleanup](VERIFICATION.md#football-animation-rebuild--4-october-2026). The current short-legged character remains stylized. No physical Android device was connected; phone rendering, touch, sustained FPS and thermals remain unverified. Earlier sections describe their historical releases.

## Transparent lagoon and three fish sizes — 3 October 2026

Measured task APK: **86,574,958 → 86,757,976 bytes (+183,018)**. Strict-limit headroom: **13,242,024 bytes**. The **75,000,000-byte development target remains unmet by 11,757,976 bytes**. The delta includes concurrent football changes and is not an isolated fishing-feature cost.

The prescribed main-project AndroidSubmission attempt failed while another chat changed football serialization during compilation. The same `Tools/Build/Build.ps1 -Target AndroidSubmission` succeeded from the frozen ignored checkout at `Builds/FishingWaterQA/BuildSnapshot/`, with clean build cache for complete packing offsets. Unity 6000.3.20f1 built release IL2CPP ARM64 at **23:01 Malaysia time**; the strict APK gate and v2 signature verification passed. TEMP/TMP were set only for the process to ignored `Builds/JdkTmp/`. The fishing runtime/editor/shader/material/mesh/prefab inputs match the workspace; later unrelated football edits are outside this snapshot's validation scope.

Exact artifact: `Builds/FishingWaterQA/Android/WhatTheFish-release.apk`. SHA-256: **C44AAFA5AE3E571DFEA7E56F19D102F9A7C8334A150893813F85042C16056E3E**. The concurrently replaced root APK and `Builds/SizeAudit/latest/` were preserved; use this task-specific copy and `Builds/FishingWaterQA/FinalSizeAudit/` for these figures. Serialized packed assets total **151,508,417 bytes**, a separate measure from APK bytes.

One vertex-coloured fish FBX supplies all three sizes and two LODs. Fish/water/bed art totals **125,801 estimated compressed bytes**, with no new image texture. Reducing unnecessary basin/surface tessellation saved **80,758 actual APK bytes** in an earlier comparison. Reusing the sand already shipped by the terrain then reduced the compact intermediate package by **263,104 bytes**; the final packing report confirms the duplicate mobile sand texture is absent. Existing large art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB**, and football stadium **1.556 MB**. Editable masters are retained.

The presentation passes **30 rendered Windows checks**. The legacy fishing-route suite passes **50**, with **3 existing failures identical when the presentation is disabled**. See [validation, limits and cleanup](VERIFICATION.md#transparent-lagoon-and-three-fish-sizes--3-october-2026) and [art/rebuild details](FISHING-PRESENTATION.md). Physical-phone appearance, touch, FPS and thermals remain unverified. Earlier sections describe their own historical snapshots.

## Football character motion — 3 October 2026

Measured release: **86,574,958 → 86,764,912 bytes (+189,954)**. Strict-limit headroom: **13,235,088 bytes**. The **75,000,000-byte development target remains unmet by 11,764,912 bytes**. This is a combined working-source comparison including concurrent fishing and goal-net changes, not an isolated animation cost.

The prescribed main-project `Tools/Build/Build.ps1 -Target AndroidSubmission` passed with Unity 6000.3.20f1, release IL2CPP ARM64, at **22:58 Malaysia time**. TEMP/TMP were set only within the build process to ignored `Builds/JdkTmp/`. APK v2 signature verification passed. Output: `Builds/Android/WhatTheFish-release.apk`; the task-specific verified copy is `Builds/FootballMotionQA/WhatTheFish-release.apk`. SHA-256: **B816F2D6E5F8686D80718FBD303BD84C384AE51B7AB90A6EDF846D84BC46C334**. The final input manifest reports no changes to the checked source, shader, scene, prefab, material, settings or metadata files during the build.

The 26-take animation resource contributes **11,548 serialized bytes**, approximately **1,574 independently compressed bytes**, separate from compiled code. No character mesh, texture, imported animation clip, audio or package was added by this pass. Existing run/jump/slide clips are reused; runtime IK and the saved control-space poses add no rigging middleware. Combined native-code growth is **41,048 bytes**, metadata **4,734 bytes** and Burst **6,703 bytes**, including other working changes. Largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**.

An initial successful package measured 87,028,088 bytes. A concurrent material-sharing correction removed **263,176 bytes** on the final rebuild by avoiding a duplicated fishing texture. The final package therefore retains that optimization. Populated packing offsets and freshly calculated contribution estimates are retained in `Builds/SizeAudit/latest/` and `Builds/FootballMotionQA/ReleaseSizeAudit/`. Serialized assets total **151,508,629 bytes**, a different measurement from APK bytes. No unexpected character-art growth appeared; preserve the remaining reserve.

The motion/render and gameplay/loopback suites pass **1,807 assertions**, including four timeline rates, both LODs and copied-path verification. See [scope, measured contact tolerances and archive batches](VERIFICATION.md#football-character-motion--3-october-2026). No Android device was attached: physical-phone rendering, touch, sustained FPS and WAN latency remain unverified. Earlier sections retain their original build snapshots.

## Position-aware goal net — 3 October 2026

Measured release: **86,574,958 → 87,028,060 bytes (+453,102)**. Strict-limit headroom: **12,971,940 bytes**. The **75,000,000-byte development target remains unmet by 12,028,060 bytes**. This measures combined working source, including concurrent fishing and character-motion work, rather than an isolated net-feature cost.

The prescribed main-project `Tools/Build/Build.ps1 -Target AndroidSubmission` passed with Unity 6000.3.20f1, release IL2CPP ARM64, at 22:43 Malaysia time. TEMP/TMP were set only for that build process to ignored `Builds/JdkTmp/`. APK v2 signature verification passed. Output: `Builds/Android/WhatTheFish-release.apk`; a verified copy is retained at `Builds/FootballNetQA/Android/WhatTheFish-release.apk` so concurrent builds cannot replace this evidence. SHA-256: **FE0821E2CBD384984C5DDCC6FC6CC7198BE6C5A405018EABB5B0BE46C76C2C65**.

The net adds no imported texture, mesh, animation clip, audio or package. Each goal creates one reusable subdivided mesh and shares a procedural rope material. The net shader's actual ZIP entry costs **9,348 bytes**. Combined native-code growth is **41,048 bytes**, metadata **4,734 bytes**, and Burst **6,703 bytes**; these include other working changes. The largest new art estimates belong to concurrent fishing work: mobile sand **259,471**, lagoon surface **43,086**, fish model **40,321**, and lagoon bed **19,892 bytes**. Existing largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No avoidable net asset growth was found; retain the remaining reserve.

Detailed packing/ZIP evidence is retained under `Builds/FootballNetQA/SizeAuditAfter/`, including populated offsets and independently calculated contribution estimates. Serialized assets total **151,858,317 bytes**, a different measurement from APK size. See [net validation and cleanup](VERIFICATION.md#position-aware-goal-net--3-october-2026). No phone was connected: physical-device rendering, touch, FPS and WAN latency remain unverified. Later review-fixture adjustments are development-only and do not change the verified release implementation. Earlier sections retain their historical build scope.

## Interactive football boundary and twin-rail aim — 3 October 2026

Actual local APK: **86,498,912 → 86,574,958 bytes (+76,046)**. Strict-limit headroom is **13,425,042 bytes**. The **75,000,000-byte development target remains unmet by 11,574,958 bytes**. The pre-task local file was the older 2 October football/basketball package, despite the newer integration release recorded below; its bytes/hash were measured before editing. This delta therefore includes the latest pulled code as well as this feedback change and is not an isolated feature cost.

The prescribed main-project `Tools/Build/Build.ps1 -Target AndroidSubmission` succeeded with Unity 6000.3.20f1, release IL2CPP ARM64. TEMP/TMP were set only within that build process to ignored `Builds/JdkTmp/`; no system setting changed. The APK passed the strict size gate and v2 signature verification. Artifact: `Builds/Android/WhatTheFish-release.apk`. SHA-256: **84FB381BD32B1972BAEFDEBEB6CBE763B0DED3036DCD5F3772EFFFD140F288CC**.

The effects add no texture, imported mesh, audio, animation clip or package. The wall generates 24 vertices/12 triangles and the arrow uses a reusable four-vertex/two-triangle canvas. The two new shader ZIP entries total **14,660 bytes**. The actual ZIP delta is chiefly IL2CPP **+50,982 bytes**, metadata **+11,757**, those shaders **+14,660**, Burst **−5,225** and Unity native code **+1,413**. Existing art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**; no unexpected art growth was found. Serialized assets total **151,235,033 bytes**, a separate measurement from APK bytes. Populated packing offsets and contribution estimates accompanied this build.

The final Windows source/render checks, football gameplay and loopback suites, and copied-path repeat pass **559 assertions**. See [verification scope, evidence and archive batches](VERIFICATION.md#interactive-football-boundary-and-twin-rail-aim--3-october-2026). Build/size/signature and preview evidence is under `Builds/FootballFeedbackQA/`. No Android device was attached: physical-phone rendering, touch, FPS and WAN latency remain unverified. Earlier entries below are historical source/build snapshots.

## GitHub integration release — 3 October 2026

Actual APK: **86,532,484 → 86,563,288 bytes (+30,804)**. Strict-limit headroom is **13,436,712 bytes**. The **75,000,000-byte development target remains unmet by 11,563,288 bytes**. The merge retains local football gameplay from `571cacc` and upstream basketball animation/passing from `c9810c4`, with shared network protocol **17**. No imported art or package dependency was added during conflict resolution.

The prescribed main-project AndroidSubmission attempt exited while the user's Unity editor was running. The same script and target succeeded in the existing ignored isolated checkout, using clean build cache for complete packing offsets. Its 112 C# inputs match the workspace; only the isolated editor's Android clean-cache option differs. TEMP/TMP were set only within the build process to `Builds/JdkTmp/`. Unity 6000.3.20f1 produced a release IL2CPP ARM64 APK at **15:08:49 Malaysia time**, passed the strict budget gate and APK v2 signature verification.

Current artifact: `Builds/Android/WhatTheFish-release.apk`. SHA-256: **AFFFA3FA0DD6797B97099E36C7F8AF73AC77187DD714AA661C25BAE442279696**. Complete packing, ZIP and contribution records are in `Builds/SizeAudit/latest/`; task evidence is in `Builds/GitMergeQA/`. Serialized assets total **151,213,873 bytes (+376)**, a separate measurement from the APK. Largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. APK entry growth is chiefly IL2CPP **+25,950 bytes**, metadata **+2,906**, Burst **+851** and Unity native code **+640**. No unexpected art growth was found.

The rebuilt Windows player passed **1,061 offline/loopback assertions**, including football matches/physics/fake shots, basketball motion/passing and shared jump/tackle/turning checks. See [verification scope](VERIFICATION.md#github-integration-release--3-october-2026). Physical-phone touch, rendering/FPS and WAN latency remain unverified. Reviewed old artifacts were archived in `Legacy/20261003-150444-050-git-merge-baseline/`; completed merge backups and superseded root audit output are in `Legacy/20261003-151034-738-git-merge-cleanup/`. No active art or Unity cache was archived. All earlier entries below are historical source/build snapshots.

## Football legacy-state cleanup — 3 October 2026

Actual APK: **86,530,508 → 86,532,484 bytes (+1,976)**. Strict-limit headroom is **13,467,516 bytes**. The **75,000,000-byte development target remains unmet by 11,532,484 bytes**. This cleanup removes the unused `blockedPusher` field, writes/resets and clearing branch; no asset, shader, font, audio, animation or package was added. Interception, possession eligibility, fake shots, kick tuning and ball physics retain their existing code.

The prescribed main-project AndroidSubmission build succeeded. Its incremental report omitted asset packing offsets, so the existing ignored isolated checkout supplied a clean-cache AndroidSubmission build at **14:37 Malaysia time**, with matching runtime source hashes and complete detailed packing evidence. Only the previously documented isolated editor output/clean-cache options differ. TEMP/TMP were set only within the build processes to the ignored `Builds/JdkTmp/` path. Unity 6000.3.20f1 built release IL2CPP ARM64, passed the strict size gate and APK v2 signature verification.

Artifact: `Builds/Android/WhatTheFish-release.apk`. SHA-256: **0BA2651A04C582F5AD468356624D44AD72D5DFCE06F1CB5550CA5ED699243242**. Current complete packing/ZIP/contribution reports are in `Builds/SizeAudit/latest/`; task evidence is in `Builds/FootballLegacyCleanupQA/`. Serialized assets remain **151,213,497 bytes**, a different measurement from APK size. Largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. ZIP changes are principally IL2CPP **+2,006 bytes** and metadata **-28 bytes**, with a few tiny build-record changes. Regenerated Performance Test resource names retain the same compressed sizes; no art growth was found.

Source compilation, the active-reference scan and an exact before/after comparison passed; the change deletes only unused state. No fresh gameplay/device suite is claimed for this cleanup. See [verification scope](VERIFICATION.md#football-legacy-state-cleanup--3-october-2026). The pre-task APK/audit/source backup and superseded incremental output were archived in `Legacy/20261003-143827-669-football-legacy-state/`; active assets and Unity caches remain in place. Earlier release entries below are historical snapshots.

## Goal-net collision release — 3 October 2026

Current measured APK: **86,530,508 bytes**, versus the preserved **86,529,684-byte** pre-task package: **+824 bytes** across the current combined source. Strict-limit headroom is **13,469,492 bytes**. The **75,000,000-byte development target remains unmet by 11,530,508 bytes**; retain the reserve and continue optimization. This is an actual APK measurement, not the 151,213,497-byte serialized asset report or Unity's aggregate build-output size.

Built at **13:07 Malaysia time**, Unity 6000.3.20f1, release IL2CPP ARM64. The prescribed main-project AndroidSubmission attempt ran but failed at Gradle initialization with Java's temporary-socket loopback error. The same script/target succeeded in an ignored independent checkout after setting TEMP/TMP only for that build process to the full ignored `Builds/JdkTmp/` path; no system setting changed. Its clean-cache build supplied populated packing offsets and passed the strict budget gate. APK v2 signature verification passed.

Artifact: `Builds/Android/WhatTheFish-release.apk`. SHA-256: **5A1C2E2F612338A35F7F90C2AD80A9A842D4E3E36D432E57FA3D39D2D2E9C09C**. Current detailed packing/ZIP/contribution reports are in `Builds/SizeAudit/latest/`; task evidence is in `Builds/FootballGoalNetQA/`. The old package and superseded audit were archived in `Legacy/20261003-130812-352-football-goal-net-baseline/`. No active art or Unity cache was archived.

Both goals gain four tiny runtime convex collision meshes (64 vertices/96 triangles across both goals), using authored goal bounds and the existing roof slope. No imported model, texture, animation, audio, shader or package was added. Largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB**, and football stadium **1.556 MB**. ZIP comparison chiefly shows Unity native code **-19,145 compressed bytes**, IL2CPP **+15,599**, metadata **+5,050** and Burst **-851**, with small remaining differences; these combined-source figures cannot be attributed solely to the new net code. The 96-byte serialized goal-net script record is not its compiled-code cost. No desktop SSAO shader/texture collection was found; the existing 4,220-byte mobile dither remains.

This current runtime source includes the retained formation, rounded-arrow and fake-shot updates in addition to goal-net collision. It passes **509 Windows/loopback assertions**, documented in [VERIFICATION.md](VERIFICATION.md#goal-nets-block-athletes--3-october-2026). Earlier pending entries below are historical for their corresponding source snapshots. Phone quality/FPS, touch and WAN latency remain unverified.

## Team A lettering — 3 October 2026

Pre-task APK: **86,529,684 bytes**. The retained concurrent AndroidSubmission snapshot containing the final lettering measures **86,527,020 bytes (−2,664 bytes)**, with **13,472,980 bytes** hard-limit headroom and **11,527,020 bytes** above the development target. SHA-256: `323F6749B3041F52BDE86186199D0B1C61A16658AC1DA625C7927E217690E7BA`; APK v2 signing passed. All 110 C# input hashes matched the workspace when that external build ran. Its provenance, APK, populated packing report and estimates are preserved in `Builds/FootballTeamAWordmarkQA/ExternalBuild/`. The prescribed main-project `Tools/Build/Build.ps1 -Target AndroidSubmission` attempt also ran and exited on the open-project lock; the successful concurrent snapshot used the same AndroidSubmission method in the isolated project.

The newer current release above measures **86,530,508 bytes (+824 bytes against this task's baseline)**, with **13,469,492 bytes** hard-limit headroom and **11,530,508 bytes** above the development target. Its final HUD/wordmark source hashes match the reviewed UI and its signature was independently rechecked. The newer APK/audit was retained in place. Both deltas include other working-tree revisions and are not an isolated lettering cost.

The lettering uses a **426-vertex / 366-triangle** runtime vector UI mesh and the existing Canvas material, adding no imported image, font, shader or package. Main art estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and stadium 1.556 MB. The wordmark's 112-byte serialized script record is about 88 independently compressed bytes; this does not measure native code cost. Snapshot ZIP changes chiefly comprise Unity native code −20,322 bytes, IL2CPP +13,618, metadata +4,749 and Burst −851. No unexpected large art growth was found. The 75 MB milestone remains unmet.

Actual full-size/compact/selection/detail Unity Canvas renders passed visual review; phone appearance/FPS remain unverified. See [verification and archive batches](VERIFICATION.md#team-a-lettering--3-october-2026).

## Kick arrow charge growth 1.8x — build pending, 3 October 2026

Maximum arrow growth changes from 2.0x to **1.8x** of its base. With the existing round, 80%-length head, visible length is about **1.465–2.636 m**; longitudinal transform scale is 1.6–2.88. Charge duration remains the existing one second. No geometry, material, asset or package was added. Baseline APK: **86,529,684 bytes**, **13,470,316 bytes** hard-limit headroom, **11,529,684 bytes** above the development target. Fresh Windows/AndroidSubmission attempts exited with code 1 while Unity had the project open. Resulting APK size/delta/headroom and runtime visuals remain **unverified**. Logs and baseline: `Builds/FootballAim18xQA/`. Existing runtime assertions now check the 1.8x maximum. Earlier APK/audit reports do not validate this source.

## Rounder arrow with shorter head — build pending, 3 October 2026

The arrow's rounding trim increases from 0.055 to 0.09 normalized units. Only the region ahead of its shoulder is compressed to **80%** of the rounded head's original length; shaft vertices retain their longitudinal coordinates during compression. The existing 0.68 m width, charge scaling, white/red 50% opacity and material are retained. Resulting geometric base/full lengths are approximately **1.465/2.929 m**. No asset, shader or package was added; mesh complexity remains 50 vertices/47 triangles. Actual extracted-generator checks passed bounds, upward winding, exact fill area and head-only compression using Unity API stubs. Baseline APK: **86,529,684 bytes**, **13,470,316 bytes** hard-limit headroom, **11,529,684 bytes** above the development target. Fresh Windows/AndroidSubmission attempts exited with code 1 while Unity had the project open. After-size/delta/headroom and actual rendering remain **unverified**. Evidence: `Builds/FootballShortHeadAimQA/`. The latest audit's main estimates still describe timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and stadium 1.556 MB from the earlier build; they do not measure this source. Rebuild Windows and run `Tools/Build/Test-FootballBall.ps1`, then build AndroidSubmission and measure its APK.

## Rounded charging arrow — build pending, 3 October 2026

Corner rounding uses a once-created 50-vertex/47-triangle runtime mesh and the existing transparent material. No imported asset, shader or package was added. The actual extracted geometry generator passed compilation and bounds/winding/fill-area checks with Unity API stubs. Baseline APK: **86,529,684 bytes**, **13,470,316 bytes** of hard-limit headroom, and 11,529,684 bytes above the 75 MB development target. Both fresh Windows and prescribed AndroidSubmission attempts exited with code 1 while the project was open in Unity. After-size/delta/headroom, actual rendering and phone quality/FPS remain **unverified**. Current logs and baseline record: `Builds/FootballRoundedAimQA/`. The latest asset audit still belongs to an earlier build and does not validate this source. Rebuild Windows and run `Tools/Build/Test-FootballBall.ps1`, then build AndroidSubmission and measure the actual APK.

## Solid arrow width — build pending, 2 October 2026

Only the arrow's transverse scale changes from 0.85 to **0.68 m** (80% of the preceding width), narrowing the shaft and head together. Length remains 1.6–3.2 m with the existing charge colors. No assets/shaders/packages changed. Baseline APK: **86,529,684 bytes**, with **13,470,316 bytes** of hard-limit headroom and 11,529,684 bytes above the development target. The prescribed AndroidSubmission attempt exited with code 1 while the project was open in Unity. Resulting APK bytes/delta/headroom and rendered width are **unverified**; the existing APK does not validate this edit. Evidence: `Builds/FootballAimWidthQA/`. The latest audit describes the earlier build, not this source.

## Validated attached-control snapshot — 2 October 2026

The locally saved AndroidSubmission APK is **86,529,684 bytes (86.53 decimal MB)**. Against the preserved **86,505,374-byte** release, the combined-source increase is **24,310 bytes**. Hard-limit headroom is **13,470,316 bytes**; the **75,000,000-byte development target remains unmet by 11,529,684 bytes**. This release passes the strict under-100,000,000-byte gate. Preserve the reserve; the development target is still an optimization milestone.

This snapshot validates attached-foot possession, the user-approved **25%** slowdown and **0.65 m** anchor, kick/tackle release, visible goal reset, existing match flow, the shared jump/tackle/whiff revisions and the transparent **0.8–1.44 m LineRenderer aim**. **It is not validation of subsequent concurrent solid-arrow or kickoff-formation edits in the current working tree.** Those source-specific Windows/Android checks and APK freshness remain pending in the attempt records below. Preserve those edits; do not use this APK as evidence that they passed.

Built at **23:49 Malaysia time**, Unity 6000.3.20f1, release IL2CPP ARM64. The prescribed root AndroidSubmission attempt hit the open-project lock. The same script/target succeeded in the ignored isolated project; a final clean-cache build populated the complete detailed packing report. Only the isolated editor build options were adjusted for that clean report. Java's default temporary socket path caused initialization errors; setting **TEMP/TMP only in the build process** to the repository's ignored `Builds/JdkTmp/` resolved them, without changing system settings. The first wrapper also misread robocopy's retained exit code after a successful PowerShell build; its obsolete check was removed. The clean final script and Unity exited successfully.

Artifact: `Builds/Android/WhatTheFish-release.apk`. APK v2 signature verification passed. SHA-256: `C3D07308791957066F458FEEB608C4CF8F484343B412BF428DABCBFD85E7AE67`. Current full packing, ZIP and contribution reports are in `Builds/SizeAudit/latest/`; build/signature/delta evidence is in `Builds/FootballControlQA/`. The measured baseline is retained in `Legacy/20261002-221540-695-football-reset-baseline/`.

The largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. The texture-free aim material is **1,760 serialized bytes / approximately 697 compressed bytes**. Actual ZIP growth is chiefly Unity native code **+20,458 bytes** and IL2CPP **+4,496 bytes**, partly offset by other entries. The single mobile dither texture remains; no desktop SSAO shader/texture collection was added. Serialized totals are not APK bytes. See [the full audit](APK-SIZE-AUDIT.md).

The validated source snapshot passed **323 football match/physics/loopback assertions**, alongside **292 shared movement regression assertions**. See [evidence and current-source limits](VERIFICATION.md#attached-football-possession--2-october-2026). Phone rendering/touch/FPS and WAN latency remain unverified. Superseded players/helpers and failed audits were archived under `Legacy/20261002-233051-458-football-attached-cleanup/`; the intermediate APK/partial report and diagnostic extracts under `Legacy/20261002-234957-518-football-final-package/`. No active art, Unity caches or Git history were removed.

## Earlier concurrent build attempts

The pending records below describe their task-specific attempts and later edits. Their runtime/size claims are superseded only for the source included in the validated snapshot above; the subsequent solid-arrow and formation revisions are still unverified by this snapshot.

## Solid charging arrow — build pending, 2 October 2026

The aim now grows from **1.6 m to 3.2 m**, using a filled seven-vertex/three-triangle mesh generated once at runtime and the existing transparent material. No texture, model, shader or package asset was added. The pre-change APK measured **86,529,668 bytes**, with **13,470,332 bytes** of hard-limit headroom and **11,529,668 bytes** above the development target. Baseline bytes/hash and audit snapshot are retained in `Builds/FootballSolidAimQA/`. Fresh Windows and prescribed AndroidSubmission attempts exited with code 1 while Unity had the project open, so resulting APK bytes, delta and headroom for this source are **unverified**. Latest asset estimates still belong to the earlier build (timber 2.247 MB, character albedo 1.993 MB); they do not validate this change. A separately supplied handoff APK now measures **86,529,684 bytes** (+16 bytes against the baseline), leaving **13,470,316 bytes** below the hard limit; whether it includes this solid-arrow source is unverified, so that delta is not attributable to this task. Its hash and freshness caveat are in `Builds/FootballSolidAimQA/artifact-at-handoff.json`. Geometry checks passed, but actual rendering, Windows regression and phone quality/FPS remain pending. Rebuild Windows and run `Tools/Build/Test-FootballBall.ps1`, then build AndroidSubmission and measure its actual APK.

## Fixed football kickoff formations — build pending, 2 October 2026

Independent 1v1–5v5 slot arrays were added to the existing rules resource, with same-team host shuffling on initial and regulation goal kickoffs. The existing ball anchor, art, stadium, goals and field dimensions are preserved. No model, texture, audio, animation, shader or package was added. Pre-change APK: **86,529,668 bytes**, SHA-256 `128D39307B39BAEFA176295B7077D050E2F8C0A8A8BE000400DC67DAFC9A86B2`, leaving **13,470,332 bytes** below the hard limit and exceeding the development target by **11,529,668 bytes**. This is a baseline, not validation of the new source. Fresh main-project Windows and prescribed AndroidSubmission attempts exited with code 1 while Unity had the project open. Resulting APK bytes, delta and post-change headroom are **unverified**. `Builds/FootballFormationQA/` retains baseline measurement, failures, 60,059 passing pure rule/formation/allocation checks and compilation against installed Unity assemblies. The latest audit's asset contribution CSV has no estimates and belongs to the prior isolated build; no new growth report is available. Runtime geometry, visuals and host/client checks require a fresh match verification player and `Tools/Build/Test-FootballMatch.ps1`, followed by AndroidSubmission and actual APK measurement. See [formation configuration and limits](FOOTBALL-MATCH.md#fixed-kickoff-formations). During handoff, a separate build replaced the APK with **86,529,684 bytes**, leaving **13,470,316 bytes** below the hard limit and exceeding the development target by **11,529,684 bytes**. Inclusion of these formation changes in that artifact is unverified; the observed 16-byte difference cannot be attributed to this task. Its observed hash/size is recorded in `Builds/FootballFormationQA/observed-apk.json`.

## Kick aim length clarification — build pending, 2 October 2026

The maximum is now 1.8 times the shortened 0.8 m base, giving **0.8–1.44 m**. No assets, shaders or packages were added or changed for this correction. No APK was present before the change. Fresh Windows and AndroidSubmission attempts exited with code 1 while Unity had the project open. At handoff, a separate isolated build supplied an APK measuring **86,529,668 bytes**, with **13,470,332 bytes** of hard-limit headroom and **11,529,668 bytes** above the development target. Its inclusion of this correction is **unverified**, and no before/after delta can be attributed to this task. `Builds/FootballKickAimLengthQA/` retains the failed task-build logs. The latest audit belongs to that separate isolated build, but its contribution CSV has no asset estimates; it does not validate this correction. Rebuild Windows and run `Tools/Build/Test-FootballBall.ps1`, then build AndroidSubmission and measure the actual APK. Older renders/builds do not validate this correction.

## Charge-scaled transparent kick aim — build pending, 2 October 2026

The aim is white at 50% opacity, grows from 0.8 m to 1.44 m during charging, and becomes red at 100%. It adds one 4,017-byte **source material file** (not an APK contribution measurement), using the existing URP glass shader and transparency keywords with no textures. The authored Resources material preserves the needed transparent variant in builds. No pre-change APK existed; fresh Windows and prescribed AndroidSubmission builds exited with code 1 while Unity had the project open. Before/after APK bytes, delta, headroom, contributor growth and runtime visual checks are **unverified**. Current failure logs are in `Builds/FootballKickAimChargeQA/`; the latest size audit has only a failure log. Build Windows, run `Tools/Build/Test-FootballBall.ps1`, then rebuild AndroidSubmission and measure its actual APK before delivery.

## Charging kick aim — build pending, 2 October 2026

The charging-only direction line reuses the shipped ball shader through a runtime material and LineRenderer, with no imported texture/model or package addition. No APK existed before the change. Fresh Windows and prescribed AndroidSubmission attempts exited with code 1 while Unity had the project open, so before/after APK bytes, delta and remaining headroom are **unverified**. `Builds/FootballKickAimQA/` retains both build failures; `Builds/SizeAudit/latest/` contains only the failed log and no fresh contributor report. A new Windows player and `Tools/Build/Test-FootballBall.ps1` are required for actual aim visibility/direction/lifecycle checks, followed by AndroidSubmission and measurement of the resulting APK. Historical release sizes below do not validate this rendering change.

## Football closer-control tuning — build pending, 2 October 2026

Attached-ball offset is reduced from 0.9 m to 0.65 m, preserving the collision clearance floor. Possession movement now uses 0.75 of base speed (25% slowdown), giving 3/5.25 m/s walk/sprint targets. Source defaults, active prefab and existing runtime checks agree. No asset/package dependency was added. The pre-change APK was absent; fresh Windows and AndroidSubmission attempts exited with code 1 while Unity had the project open. Before/after APK bytes, delta and remaining headroom are **unverified**. `Builds/FootballControlTuningQA/` retains failure logs and static tuning evidence; `Builds/SizeAudit/latest/` has only the failed build log, with no new contributors report. Fresh runtime and visual checks require rebuilding Windows and running `Tools/Build/Test-FootballBall.ps1`; then run `Tools/Build/Build.ps1 -Target AndroidSubmission` and measure its APK. Historical releases below do not validate this tuning.

## Football missed-slide penalty — build pending, 2 October 2026

A slide that hits neither a player nor the ball now applies 0.8 seconds of recovery after its 0.425-second action, with movement reduced by 60% (40% of normal walk/sprint speed). This reuses locomotion and existing animation assets; no assets or packages were added. The pre-change APK was absent. Fresh Windows and prescribed AndroidSubmission builds both exited with code 1 because Unity had the project open. Before/after APK bytes, delta and remaining headroom are **unverified**. `Builds/FootballWhiffQA/` retains both failure logs and 665 passing logic assertions using the actual tackle source with Unity API stubs. `Builds/SizeAudit/latest/` contains only the failed build log, with no new contributor report to inspect. Runtime speed, collision, animation and network validation require a fresh Windows build and `Tools/Build/Test-Tackle.ps1`; APK validation requires a successful `Tools/Build/Build.ps1 -Target AndroidSubmission`. Historical size records below do not validate this change.

## Slide ball launch — build pending, 2 October 2026

Both free-ball slide contact and a tackled holder's released ball now receive 4 m/s upward velocity, reusing the existing ball and physics. No asset or package dependency was added. The pre-change release APK was absent. Fresh Windows and AndroidSubmission attempts exited with code 1 while Unity had the project open; resulting APK bytes, delta and headroom are **unverified**. Failed logs are retained in `Builds/FootballBallLaunchQA/`, and `Builds/SizeAudit/latest/` has only the failed build log, with no new contributor report. Rebuild Windows and run `Tools/Build/Test-FootballBall.ps1`, then rebuild AndroidSubmission and measure the actual APK. Historical 86,505,374-byte figures below do not validate these edits.

## Football jump and tackle recovery — build pending, 2 October 2026

Football jumping is restored. Tackle hits lock movement for 0.8 seconds, affect teammates and opponents without a fixed contact limit, and knock both sliding players down on collision. Airborne players evade hits. No assets or packages were added. Before this change, `Builds/Android/WhatTheFish-release.apk` was absent; the 86,505,374-byte release below is a historical record, not validation of these edits. Fresh Windows and AndroidSubmission builds were attempted but exited with code 1 while Unity had this project open. Player/runtime checks, resulting APK bytes, size delta and remaining headroom are **unverified**. Current build logs are retained in `Builds/FootballTackleQA/`; `Builds/SizeAudit/latest/build.log` records the unsuccessful submission attempt. Close the project in Unity, rebuild Windows and run `Tools/Build/Test-Tackle.ps1` and `Tools/Build/Test-Jump.ps1`, then rebuild AndroidSubmission and measure the actual APK before delivery.

## Last measured release

Current AndroidSubmission APK: **86,505,374 bytes (86.51 decimal MB)**, up **13,844 bytes** from the measured 86,491,530-byte match release. This delta includes the previously unbuilt glass scoreboard plus the current team-selection/flow revision. Hard-limit headroom is **13,494,626 bytes**; the 75,000,000-byte development target remains unmet by **11,505,374 bytes**. The strict under-100,000,000-byte gate passed; preserve the remaining reserve.

Built at 21:58 Malaysia time on 2 October using Unity 6000.3.20f1, release IL2CPP ARM64. The prescribed `Tools/Build/Build.ps1 -Target AndroidSubmission` exited on the open-project lock; the same `ProjectBuilder.BuildAndroidSubmission` succeeded in the current editor. APK v2 signature passed. SHA-256: `68A35BB2AC436D115E08D35F5AFC7322D120ECAA4AB035627418696A101566A8`. Artifact: `Builds/Android/WhatTheFish-release.apk`.

Fresh populated packing offsets and the actual APK show unchanged main art contributors: timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB estimates. No texture, model, animation or package was added. The UI generates three small procedural textures at runtime. ZIP comparison attributes 11,665 bytes to IL2CPP and 2,157 bytes to metadata; scene/resource changes are small. Configuration remains 72 serialized bytes (58 compressed-byte estimate), and the new selection script reference is 112 serialized bytes (72 estimated compressed bytes). No desktop SSAO/blue-noise collection was introduced. Per-asset estimates are not exact allocations of shared ZIP chunks.

Current audit: `Builds/SizeAudit/latest/`; before/after comparison and hash: `Builds/FootballFlowQA/verification.json` and `apk-entry-deltas.csv`. The rebuilt Windows player passed 294 match/network/football-physics assertions; pure rules passed 40,011. Selection/full-team and scoreboard states were visually reviewed. No physical-phone or WAN qualification was performed. The prior APK/audit is preserved under `Legacy/20261002-185628-523-scoreboard-baseline/`; obsolete prior Windows player, UI captures and one-off editing helpers are archived in the flow batches listed in [FOOTBALL-MATCH.md](FOOTBALL-MATCH.md).

## Earlier football match release — 2 October 2026

The first freshly verified APK for the combined GitHub/local source plus football match rules is **86,491,530 bytes (86.49 decimal MB)**. It leaves **13,508,470 bytes** below the strict 100,000,000-byte limit, and remains **11,491,530 bytes above** the 75,000,000-byte development target. No current APK existed immediately before this task, so a measured before/after delta for the match change alone is unavailable. The historical upstream record is 85,970,822 bytes; the 520,708-byte difference also includes the earlier local football integration and cannot be attributed solely to match code.

Built at 17:22 Malaysia time on 2 October with Unity 6000.3.20f1, release IL2CPP ARM64. `Tools/Build/Build.ps1 -Target AndroidSubmission` was attempted but exited because the editor had the project open. The same `ProjectBuilder.BuildAndroidSubmission` succeeded in that editor, including its strict size gate. APK v2 signature verification passed. SHA-256: `B45370E4425DF5CA79581244BE65E777D5AF0B564D730E1130BEE7DE8251D724`. Artifact: `Builds/Android/WhatTheFish-release.apk`.

No art, texture, animation or package was added for match rules. The only new shipped resource is the 72-byte serialized configuration. Current detailed packing offsets and the actual APK were used for contribution estimates: timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. The existing ball remains 550,636 serialized bytes. A single mobile pipeline dither blue-noise texture contributes 4,220 serialized bytes; no desktop SSAO shader or desktop blue-noise collection was introduced. Current audit: `Builds/SizeAudit/latest/`. The large terrain-control serialized counts are uncompressed data, not their APK contributions. Preserve the mobile pipeline and reserve; no visual-quality reduction was made.

Gameplay evidence and limitations are in [FOOTBALL-MATCH.md](FOOTBALL-MATCH.md). No physical-phone or WAN qualification was performed. The superseded first-pass verification player was archived under `Legacy/20261002-171536-058-football-match-player-trial/`; its reports remain as first-pass evidence.

## Upstream basketball animation snapshot — 3 October 2026

The combined football and basketball Android submission APK measures **86,498,912 bytes (86.50 MB / 82.49 MiB)**. It leaves **13,501,088 bytes (13.50 MB)** below the strict 100,000,000-byte limit and exceeds the 75,000,000-byte development target by **11,498,912 bytes**. The pre-merge basketball animation APK measured **85,993,642 bytes**; the merged build added **505,270 bytes**. These figures are from the APK files themselves.

Built on 2 October at 16:04:53 Malaysia time (08:04:53 UTC) with Unity 6000.3.20f1, release IL2CPP, ARM64, minimum API 26, target API 36 and default Android ZIP compression. `Tools/Build/Build.ps1 -Target AndroidSubmission` passed the hard size gate, and `apksigner verify --verbose` passed with a v2 signature. SHA-256: `6EB19CFA79CF55571963DA49DFE687B6402E6F7A675A375B9C1503C9D0F91418`.


The current audit is in `Builds/SizeAudit/latest/`. The football ball FBX adds **550,636 uncompressed serialized asset bytes**; this is not an APK allocation. ZIP entry growth is concentrated in the football scene asset splits, with roughly 20.8 kB of IL2CPP and 3.7 kB of metadata growth. No new texture or package dependency appeared. The rebuilt Windows player passed **367 checks** across football physics and host/guest replication, basketball motion and passing, and jump gameplay and replication. No physical Android phone or WAN test was performed.

The pre-merge APK, Windows player and size audit were preserved locally in `Legacy/20261002-155631-852-football-merge-build-baseline/`. The teammate's earlier football APK measurements and verification are retained below as history from a separate working tree.

## Basketball animation release before merge — 2 October 2026

The earlier Android release APK contained all four islands, Sky-Sail, the steerable character jump, basketball dribbling, shooting and passing animations, golf equipment and two fishing rods. Its exact size was **85,993,642 bytes (85.99 MB / 82.01 MiB)**, leaving **14,006,358 bytes (14.01 MB)** to the hackathon's strict 100,000,000-byte limit at that checkpoint.

Built at 15:40:51 Malaysia time (07:40:51 UTC) on October 2 with Unity 6000.3.20f1, IL2CPP, ARM64, minimum API 26, target API 36 and default Android ZIP compression. `AndroidSubmission` and `apksigner verify --verbose` passed (v2 signature). SHA-256: `5FD964709B5D449ED0508FDB56E287DAFA3FCD48A89540191B7FE71447BD12CF`.

Use `Tools/Build/Build.ps1 -Target AndroidRelease` for development releases. It generates size reports and warns above the **75,000,000-byte working target**. That target remains unmet by **10,993,642 bytes (10.99 MB)**. `-Target AndroidSubmission` rejects APKs of **100,000,000 bytes or larger**; the current submission check passes. `-Target OptimizeAndroid` reapplies mobile import settings and regenerates delivery libraries from their prepared source exports.

The archive contains about **59.64 MB** of Unity scene/art/shader data and **26.35 MB** of runtime/Android overhead. Shared vegetation and material patterns, generated grass, selected render-mesh reductions and mobile compression account for the main saving. Cabin geometry and editable art masters are preserved. See [the measured optimization report and limits](APK-SIZE-AUDIT.md). Continue budgeting during development; no physical phone qualification has been performed.

The basketball animation update measured **85,970,874 to 85,993,642 bytes (+22,768 bytes)**. It adds a procedural pose layer, timed dribble/shot/pass release, a Pass control and protocol-13 action replication while reusing all art. Windows checks passed at four animation rates, with multiplayer passing, ball physics, jump regression and a portable copied-player run; **65 editor motor checks** also passed. The final idle-pose replication fix passed the 56-check motion review and two-player regression. Close and side views of both character LODs were inspected. See [basketball animation and verification](BASKETBALL-ANIMATION.md). The release has no new mesh, texture, material or package dependency.

The momentum update measured **85,970,822 to 85,970,874 bytes (+52 bytes)**. It changes movement code, reusing all art and the existing protocol-12 replication. All **287 Windows player checks** and **65 motor checks** passed, covering release in eight directions and three jump phases, air steering, landing, walls, host/guest replication, tackle, turning and basketball regression. See [jump momentum and verification](JUMP-ANIMATION.md). No asset dependency growth was found.

The basketball gameplay revision measured **85,957,194 to 85,970,822 bytes (+13,628 bytes)**. It reuses the ball, hoops and character art, adding proximity possession, assisted shooting, recovery and protocol-12 synchronization. The final Windows gameplay suite passed **82 checks**, with **53 foundation physics/replication checks** and **40 jump regression checks** also passing. A copied-player fixture in a path with spaces passed all 61 offline gameplay checks from an unrelated working directory. See [controls, tuning and current evidence](BASKETBALL-GAMEPLAY.md).

The jump motion revision measured **87,153,874 → 85,957,194 bytes (−1,196,680 bytes)** across the shared working tree. The reduction is mainly concurrent fishing optimization, not a jump-only saving. The revised clip contributes **37,124 serialized bytes**, approximately **24,231 compressed bytes**, while reusing all character geometry and textures. Grounded arm/leg preparation, takeoff alignment, flight and landing were inspected in actual Windows motion captures. All **126 jump, tackle and turning checks** passed. See [jump authoring and verification](JUMP-ANIMATION.md).

The fishing integration measured **85,088,760 → 85,955,058 bytes (+866,298 bytes)**, including concurrent jump refinements. Fishing art contributes approximately **852,462 compressed bytes** by separate per-asset estimates. Both rods share their materials/maps between two LODs, use 512-pixel colour and 256-pixel normal maps with ASTC 6x6, and Low mesh packing. This replaced a larger 87,153,874-byte first pass. All **22 Windows rod checks** passed and seven actual player views were inspected after optimization. Source GLBs and full-resolution masters remain outside Unity Assets. See [fishing rod analysis and review](FISHING-RODS.md).

The initial jump implementation measured **83,751,052 → 85,088,760 bytes (+1,337,708 bytes)** across the shared working tree, which also gained golf equipment during the task. This is not an isolated jump-code size delta. That clip was **29,292 serialized bytes**, approximately **17,367 compressed bytes**; golf equipment accounts for approximately **1,314,630 compressed bytes**. Separate asset compression estimates do not sum exactly to APK ZIP chunks. Restoring the existing high packing of distant island proxies saved **728,456 APK bytes** compared with the first 85,817,216-byte build, and the generator now preserves that setting. All **126 Windows jump, tackle and turn checks** passed at that checkpoint, including two-player tests; its Android submission build and v2 signature passed. See [jump design and verification](JUMP-ANIMATION.md) and [golf equipment](GOLF-EQUIPMENT.md).

The basketball addition changed the measured APK from **83,282,808 to 83,751,052 bytes (+468,244 bytes / 0.47 MB)**. It ships two compact mesh LODs, one material and 1024×512 ASTC colour/normal maps. The unchanged Meshy GLB, reference image and 2048×1024 authoring maps remain outside Unity Assets. All **53 Windows basketball checks** passed, covering bounce/spin, floor/rim/backboard contacts, scene release/return and host/guest replication. See [the basketball report](BASKETBALL-BALL.md). Pickup, dribbling, player shooting controls, scoring and phone performance are not validated by this foundation.

Before this addition, the 30 September portable-path validation build changed the APK from **83,282,804 to 83,282,808 bytes (+4 bytes)**. The September mobile asset pass had reduced the 350.25 MB cleanup checkpoint by **266.97 MB (76.22%)**. Those are historical checkpoints, not the current APK size.

## Historical baseline — 25 September 2026

The earlier Android release-mode test APK included Sunvale football, Rally basketball, Tidebloom golf and the Rainbow Sprinter animations available at that time. It predates fishing, the later island refinements and Sky-Sail; the table below is historical, not the current file at that path.

| Artifact | Exact size | Decimal MB | MiB |
| --- | ---: | ---: | ---: |
| `Builds/Android/WhatTheFish-release.apk` | 72,690,957 bytes | **72.69 MB** | 69.32 MiB |

Built at 23:16:54 Malaysia time on September 25, 2026 with Unity 6000.3.20f1. Android package: `com.umpsa.whatthefish`, version 0.3.0, ARM64, minimum API 26, target API 36. `apksigner verify --verbose` passed (v2 signature). This non-development player uses the configured test signing key; no phone installation or performance benchmark was performed during this update.

SHA-256: `7045CA6F0BCEE51F811E353EB20F6530A34A36B1782A5C0B9F628A0361A8B444`.

Build command: `Tools/Build/Build.ps1 -Target AndroidRelease`. Windows command: `Tools/Build/Build.ps1 -Target Windows`. The current Windows executable is `Builds/WindowsFinal/WhatTheFish.exe`; its data folder and DLLs must stay together. The Unity build-report total includes intermediate/debug artifacts, so the APK figure above is measured from the actual final `.apk` file.

## Legacy cleanup

`Legacy/` contains archived placeholders, unused Unity materials, Blender automatic backups, the old athlete, obsolete generator scripts, old Windows builds, superseded APKs, and a temporary clone used to verify GitHub LFS downloads. You can delete this entire archive. `Legacy/archive-manifest.json` records the original asset-migration entries; later build archives are grouped under `Legacy/Builds/BeforeBrandRename/`. The archive does not free disk space until it is deleted.

Unity dependency inspection confirmed the old Stadium/BasketballArena FBXs and archived materials are unused by current scenes and prefabs. The basketball logo variants were retained as standalone current mesh/prefab assets, and the scene builder no longer reads the placeholder. Windows and Android rebuilt successfully after archiving. Post-cleanup football/basketball smoke tests and the 126-check golf audit passed.

Current art masters, source textures, Meshy provenance, active code, game assets and current QA evidence remain outside the archive.

## Historical local football releases

The historical local charge-kick AndroidSubmission APK is **83,787,566 bytes**, compared with **83,786,982 bytes** before this task (**+584 bytes**). It leaves **16,212,434 bytes** below the strict 100,000,000-byte limit and exceeds the 75,000,000-byte development target by **8,787,566 bytes**. Built on 1 October at 15:03 UTC; release IL2CPP ARM64. SHA-256: `B6D7794AF24D0A4ECD8B7E8B0CD5DBF1A5BE53181EC7FBC282D34A9487F916E6`. Submission size gate and APK v2 signature verification passed. Archived artifact: `Legacy/20261002-002911-200-football-balance-apk-baseline/Builds/Android/WhatTheFish-release.apk`.

Editor and rebuilt Windows each passed 75 actual-pitch checks, and isolated host/client checks verified charge transmission. Generated IL2CPP contains the charge-aware kick methods. The batch build failed at Gradle loopback initialization; the same submission method succeeded in the editor after desktop unlock. Current evidence: `Builds/FootballPrototype/` and `Builds/SizeAudit/latest/`. No physical-phone or internet-latency qualification was performed.

The incremental build omitted packing offsets. The audit uses actual APK entries and baseline offsets only for **42 streams verified byte-identical by SHA-256**, omitting three changed streams. Largest verified art contributors remain unchanged (timber 2.25 MB, character albedo 1.99 MB, golf structures 1.98 MB, character mesh 1.57 MB, football stadium 1.56 MB estimates). No models/textures were added, and verified shared art streams retain their bytes. Mobile pipeline assignments and pruned editor resources were restored/preserved. Previous APK and audit: `Legacy/20261001-225440-511-charge-kick-apk-baseline/`; see [feature verification and archives](FOOTBALL-PROTOTYPE.md).

## Previous goal-entry release
The current no-climb, field-boundary and both-goal-entry AndroidSubmission APK is **83,786,982 bytes (83.79 decimal MB)**, **7,704 bytes larger** than the preceding measured 83,779,278-byte player-interception build. It leaves **16,213,018 bytes** below the strict 100,000,000-byte limit and remains **8,786,982 bytes above** the 75,000,000-byte development target. No current APK existed before this follow-up; the comparison is against the preserved prior measured APK.

Built on 1 October at 14:13 UTC with Unity 6000.3.20f1, release IL2CPP ARM64. SHA-256: `AAD7E0BC691678B215C7F30AC2053C4008B7F6DF9C3826EDCB2608C60E3CCA85`. The submission size gate and APK v2 signature verification passed. Artifact: `Builds/Android/WhatTheFish-release.apk`. Generated IL2CPP contains the goal-area implementation. Editor and rebuilt Windows each passed 63 actual-pitch checks; the isolated loopback rerun passed host and guest checks including both goals. One earlier guest stopped-ball timing check failed during concurrent compilation and is preserved; see [verification and archives](FOOTBALL-PROTOTYPE.md). No phone or internet-latency qualification was performed.

The prescribed batch build failed at Gradle loopback initialization; the same submission method succeeded in the native editor. Its incremental build reused player data and omitted packing offsets. The current audit therefore includes actual APK entries plus a clearly marked partial asset estimate: baseline offsets are reused only for **43 streams whose SHA-256 matches the new APK**, omitting two changed streams. The largest verified art contributors are timber texture (about 2.25 MB ZIP estimate), character albedo (1.99 MB), golf structures (1.98 MB), character mesh (1.57 MB) and football stadium (1.56 MB). Shared asset streams are byte-identical to the baseline; no new art or avoidable art growth was introduced. Current evidence is in `Builds/SizeAudit/latest/`. The mobile pipeline and pruned editor resource references were preserved.

## Previous player-interception release
The player-interception AndroidSubmission APK is **83,779,278 bytes (83.78 MB / 79.90 MiB)**, **80 bytes smaller** than the preceding 83,779,358-byte physics prototype. It leaves **16,220,722 bytes** below the strict 100,000,000-byte limit and remains **8,779,278 bytes above** the 75,000,000-byte development target.

SHA-256: `D8012E04B9E954D3C2EDF69AD4F7C6C9F7D89844ABD9C97557295B98F5FE6E92`. Built on 1 October at 10:17 UTC with Unity 6000.3.20f1, release IL2CPP ARM64, API 26 minimum / API 36 target. The submission size gate and APK v2 signature verification passed. Historical artifact (now archived): `Legacy/20261001-183311-169-no-climb-apk/Builds/Android/WhatTheFish-release.apk`. Generated Android IL2CPP output from that build contains the interception methods. Editor and rebuilt Windows checks passed, including the kicker overtaking their ball and remote-player interception; see [verification and limits](FOOTBALL-PROTOTYPE.md).

The prescribed script exited because Unity had the project open; the same `ProjectBuilder.BuildAndroidSubmission` succeeded in that editor. After an interrupted Windows preview, an intermediate build retained the desktop pipeline and measured 86,254,408 bytes. Restoring the authored `MobileURP` assignment removed that avoidable growth. The final populated packing report contains no desktop SSAO blue-noise textures or shader; the largest art contributors remain unchanged. Current audit, build log and actual ZIP-entry estimates are in `Builds/SizeAudit/latest/`. Previous and intermediate evidence was archived as recorded in the prototype guide. No art-quality reductions, phone qualification or Git publishing were performed.

## Previous basic physics prototype — 1 October 2026

The basic football physics prototype AndroidSubmission APK is **83,779,358 bytes (83.78 MB / 79.90 MiB)**, an increase of **12,420 bytes** from the same-session 83,766,938-byte static-ball baseline. It leaves **16,220,642 bytes** below the strict 100,000,000-byte limit and remains **8,779,358 bytes above** the 75,000,000-byte development target.

SHA-256: `AB2382BC2BBF0308CD63B3B24D0276F51A101AF747B5CB55143D101841E3F263`. Built on 1 October at 09:23 UTC with Unity 6000.3.20f1, release IL2CPP ARM64, API 26 minimum / API 36 target. The submission size gate and APK v2 signature verification passed. Artifact: `Builds/Android/WhatTheFish-release.apk`. Actual-pitch physics and final Windows offline/two-player loopback tests passed; no physical-phone or internet-room qualification was performed. See [football prototype verification](FOOTBALL-PROTOTYPE.md).

The prescribed build script exited because the editor had the project open. The same `ProjectBuilder.BuildAndroidSubmission` ran in that editor. Its first APK measured 84,317,854 bytes and unexpectedly retained desktop SSAO resources after Windows testing. Clearing URP's platform build-data cache reduced it by **538,496 bytes** to the final size above. The final log selects `MobileURP`; its populated packing report contains no SSAO blue-noise textures/shader. The mesh remains **550,636 serialized bytes / 473,398 estimated ZIP bytes**. The largest art contributors remain shared timber, character colour, golf structures, character mesh and football stadium geometry. Source geometry, textures and visual quality settings were not reduced for this prototype.

Current successful build log, detailed packing report, actual APK ZIP-entry sizes and per-asset estimates are in `Builds/SizeAudit/latest/`; functional and signature evidence is in `Builds/FootballPrototype/`. Static baseline and intermediate build/audit artifacts were archived in the batches recorded in the prototype guide. The transient editor build helper was removed after completion. Git publishing remains with the user.

## Previous static-ball baseline — 1 October 2026

The Android submission APK including the doubled, 44 cm centre-spot football is **83,766,938 bytes (83.77 MB / 79.89 MiB)**. It leaves **16,233,062 bytes** below the strict 100,000,000-byte limit and exceeds the 75,000,000-byte development target by **8,766,938 bytes**. Continue preserving that shared reserve.

SHA-256: `B1300F631C9B10D5B09675A1B2B9C2E35FF3B624C8D856B55C76059CC366634D`. Built on 1 October at 07:42 UTC with Unity 6000.3.20f1, release IL2CPP ARM64, API 26 minimum / API 36 target. AndroidSubmission's hard size gate and APK v2 signature verification passed. Artifact: `Builds/Android/WhatTheFish-release.apk`.

The resize changed the measured APK from **83,765,822 to 83,766,938 bytes (+1,116 bytes)**. The mesh and two materials retain their packed sizes; scaling adds no new texture or geometry asset. Against the documented 30 September APK the increase is **484,130 bytes**; that older APK was absent at the initial integration. See [football placement and verification](SOCCER-BALL.md) for source audits, preserved prior builds and limits. No physical-phone test was performed.

The initial 22 cm integration's `Tools/Build/Build.ps1 -Target AndroidSubmission` reached Gradle but failed twice at Java loopback initialization in the command runner, including a process-local IPv4 retry. Its editor build passed at 83,765,822 bytes (SHA-256 `069841C279C05282D8FA98B5DB7D2E10573D77AF8C1752C661D9F561B8B846FF`). For this resize the script was attempted while the project was open and exited with code 1; the same `ProjectBuilder.BuildAndroidSubmission` method succeeded in the existing editor. Current successful build evidence, populated packing report and ZIP-entry sizes are in `Builds/SizeAudit/latest/`.
