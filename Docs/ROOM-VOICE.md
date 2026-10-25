# Room voice

## Design and implementation

One online Multiplayer Services session maps to one non-positional Vivox audio
channel. The channel name is a SHA-256-derived identifier from the stable session
ID, independent of the host, room invitation code, selected sport and loaded scene.
Players and spectators share it across all four islands. Game packets continue
using NGO/Relay; audio uses Vivox's native transport and processing.

The pinned dependency is `com.unity.services.vivox` 16.12.1. Authentication uses the
existing Unity Authentication identity and UGS-managed tokens. No token-signing
secret belongs in the game or repository. The linked Unity project must have
Vivox enabled. An opaque channel name and client membership filtering are not a
server-side channel access policy; stricter admission requires a trusted token
service that verifies session membership before issuing scoped Vivox tokens.

`RoomVoiceService` lives on the persistent `RoomService`. `RoomVoiceSession`
serializes connect/disconnect operations and discards stale join completions.
Network errors retry with backoff, stopping after five failed attempts until the
player retries. Room departure immediately mutes capture and playback, including
while an SDK operation is pending. A new room starts with voice disconnected.

The launcher has two vector icon buttons: chat and microphone. A short mic tap
toggles capture; from a disconnected state it requests permission and joins with
the explicit intent to speak. A second tap cancels a pending unmute. Holding for
half a second opens voice settings without changing capture; right-click also
opens settings. Dragging off, losing focus or backgrounding cancels the gesture.
The microphone icon is slashed/coral when off and mint when transmitting.

Voice settings use Players and Spectators tabs. The room join/leave action has a
speaker icon and an explicit label. Joining through this action starts muted.
The scrollable roster shows in-game names, including the local player's name,
and room members who have not joined voice. Tap a member's name for their volume
slider; the adjacent speaker/slashed-speaker button mutes or unmutes them locally.
Volume ranges from 0–100%, independently for each member. Zero volume is silent.
The Spectators tab has a bulk mute that includes late arrivals and role changes;
unmuting the group preserves each member's previous volume and individual mute.
Unknown roles stay silent while group mute is enabled until the game confirms
their seat. Last known roles survive transient avatar respawns/host recovery.

Both roles share the same native channel. Group and individual controls affect
only the listener using them, not other members' settings. Voice/text panels are
mutually exclusive; opening voice captures gameplay input without pausing the
room. Speaker/output-volume preferences persist on the device; individual
volumes, group mute and individual mutes belong to the current room. Microphone
enablement never persists into a new room or reconnect. Protocol **87** includes
the authoritative player/spectator role on each chat message.
A compact microphone icon remains reachable during travel, Golf aim mode, expanded
Golf standings or other full-screen overlays when the mic is on. Voice volume affects voice playback independently
of the game's sound effects.
During gameplay, the expanded panel stays beneath the launchers in a 560 by 260
canvas-unit area. Room controls occupy its left column and the member list
scrolls on the right. Selecting a member scrolls their volume control into view.
Spectator mute and any permission/error actions stay inside the same panel;
room controls scroll when additional status content needs space. Fishing's
launchers also sit beneath its leaderboard. The lobby retains its taller layout.

Travel, settings and result overlays save only the explicit gameplay input lock.
Voice, chat and tutorial blockers are evaluated live, so dismissing one cannot
leave movement locked after an overlay or trip completes. Island rules hide the
chat controls while retaining the microphone mute shortcut.

## Phone audio

Android requests `RECORD_AUDIO` when the player joins voice or taps the mic. Android 12 and newer
also request `BLUETOOTH_CONNECT`. The permission handler allows continuing when
microphone access is granted and Bluetooth is denied; actual routing still needs
physical-device validation. Permanent microphone denial offers a device settings action.
The generated manifest marks microphone hardware optional and suppresses Unity's
automatic startup permission dialog. Existing camera features request their own
permissions at the point of use.

Vivox handles native phone capture and device routing, with noise suppression,
automatic gain control and software echo cancellation enabled. App backgrounding
leaves voice and immediately silences capture; returning reconnects with mic off.
Changing audio devices or losing mobile app focus also turns the mic off. Permission
revocation is checked before continuing capture. No recording feature is enabled.

## Compact in-game communication dock — 25 October 2026

Official AndroidSubmission: **126,914,878 -> 126,913,482 bytes
(-1,396)**. Headroom below the strict 1,000,000,000-byte limit:
**873,086,518 bytes**. SHA-256:
**482B3F2D4E21103D2CB2864C43B4B1DFB930686C8C2BD6BF00F60EB272AC7030**.
The size gate, v2 signature, ZIP CRC, all **1,250** audit entries and five-scene
bundle integrity passed. All **360 Android** compiled source documents and the
voice configuration bridge match the tested source.

The complete **342-file Windows player** is promoted to `Builds/WindowsFinal/`.
Every delivered file matches the tested candidate. All **500** compiled Windows
source documents match; **496** remain byte-identical to the preceding combined
release, preserving its concurrent scoreboard and venue work. The retained
source ZIP records the complete build snapshot, beyond this task's commit scope.

The four-sport suite passed **708 assertions**: Golf 186, Fishing 176, Football
173 and Basketball 173. Checks cover every expanded view's bounds, joystick
clearance, actual UI raycasts, group mute, last-member scrolling/volume and all
previous gesture/travel/background behaviors. All eight stickers remain inside
the compact picker. Text-chat UI checks passed **69 assertions**. Representative
1600x900, 1920x864 and 1024x768 native renders were visually reviewed. Voice-panel
input isolation is retained; opening text history permits gameplay until typing.

Evidence: `Builds/VoiceQA/UI-20261025-223156-638/`,
`Builds/VoiceQA/UI-20261025-223156-643/`,
`Builds/RoomChatQA/Run-20261025-223324-7998/` and `Builds/CommsDock/`.
Physical phone touch, native keyboard, microphone/headset behavior and FPS remain
unverified. Native voice transport was unchanged; the earlier two-client audio
acceptance is recorded below.

Superseded output recovery: `Legacy/20261025-222956-032-comms-dock-candidate/`,
`Legacy/20261025-223324-406-comms-dock-android/` and
`Legacy/20261025-223605-172-comms-dock-windows/`. Completed editing drafts were
archived in `Legacy/20261025-223850-246-comms-dock-drafts/`.

## Earlier two-button redesign verification — 25 October 2026

The complete **342-file Windows player** in `Builds/WindowsFinal/` contains the
redesign. All **500 Windows** compiled game source documents and the separate
voice configuration bridge match the retained tested source. Comparison with
the newer scoreboard delivery confirms its **487 non-communication source
documents** are preserved; only the communication code differs. The delivery
snapshot includes concurrent venue/scoreboard work, which is outside this task's
communication-only publishing scope.

The final four-sport suite passed **480 assertions**: Football 116, Basketball
116, Golf 129 and Fishing 119. It checks tap/hold/cancel behavior, two launchers,
separate member lists, individual volume, group mute, prior mute preservation,
travel, rules, Golf aim/standings, pause/resume and exit. Layouts fit 1600x900,
1920x864 and 1024x768; representative renders were visually reviewed. Golf's
active-round standings exposed an overlap, corrected by placing settings beside
the standings when necessary. The review waits for stable arrival before testing
rules and releases synthetic short taps within one frame to avoid build-load
timing artifacts. The state suite passed **52 checks**, with **52** repeated from
a path containing spaces and an unrelated working directory.

Two authenticated native clients received synthetic speech both ways through
one channel, with maximum remote energy **0.6749178 / 0.6354777**. The native
participant checks confirmed 25% gain at -12 dB, spectator mute, restored gain
and unmute. Hardware capture stayed forced silent and microphone input muted.
Text/history checks passed **226 assertions**, followed by **69** UI checks;
message roles survived seat changes, paging and host recovery.

Current evidence under `Builds/VoiceQA/`:

- `UI-20261025-213953-176/Football/`, `UI-20261025-213953-188/Basketball/`,
  `UI-20261025-213953-208/Golf/` and `UI-20261025-213953-223/Fishing/`.
- `Online-20261025-164916-031/`: native gain/mute and two-way synthetic audio.

Text evidence: `Builds/RoomChatQA/Run-20261025-164512-7995/` and
`Run-20261025-165029-7996/`. Source proofs, portable checks, package audit and
recovery records are in `Builds/CommsRedesign/`. The tested source ZIP retains
the complete compiled source snapshot. Physical phone microphone, Bluetooth,
echo, touch/FPS and mobile network handoff remain unverified without a connected
phone. See [the measured release record](BUILD-SIZE.md).

Final AndroidSubmission: **126,891,150 -> 126,914,878 bytes (+23,728)**;
**873,085,122 bytes** of strict-cap headroom. Its signature, ZIP CRC, all **1,250**
audit entries and five-scene bundle integrity passed. All **360 Android** compiled
source documents and the configuration bridge match the final runtime, including
the Golf placement fix. Superseded Windows output is recoverable under
`Legacy/20261025-214153-693-comms-redesign-windows/`; superseded UI evidence is in
`Legacy/20261025-214316-348-comms-redesign-qa/`. Additional draft and APK archive
batch paths are retained in `Builds/CommsRedesign/*-archive.txt`.

## Previous delivery before the UI redesign — 25 October 2026

Vivox is enabled for the linked Unity project. Public server/domain/issuer
settings are included in fresh-clone builds, Test Mode is off and the signing
key is empty. The complete **342-file Windows player** is in
`Builds/WindowsFinal/`; all files match its tested candidate. All **493 Windows**
and **354 Android** compiled source documents and the configuration bridge
match the workspace. Protocol **86** includes the teammate changes through
`1b7322e` plus the preserved local recovery, invitation and presentation features.

The final four-sport voice UI checks passed **340 assertions** (Football 81,
Basketball 81, Golf 94, Fishing 84). They cover the 20-person roster, volume and
individual mutes, speaking indicators, input capture, rules, travel, Golf aim,
expanded standings, settings, background/reconnect and leaving. The state suite
passed **38 checks**, repeated from a path with spaces and an unrelated working
directory. Text-chat/history regression passed **198 checks** and the final UI
repeat passed **63**; Golf guest-return/host-loss passed **23**. The account and
invitation suite passed **44 tests**. Representative 1600x900 Football, 1024x768
Golf and 1920x864 Fishing captures were visually reviewed.

Two isolated native clients authenticated and received synthetic speech both
ways in one shared channel. Remote energy reached **0.6800642 / 0.6021030**.
Each transmitted to exactly one channel, remote participants were unmuted,
hardware capture stayed forced silent and input remained muted. Both left cleanly.
This tests the real Vivox transport; it does not substitute for phone microphone
capture or a conversation in a live Multiplayer Services room.

An early native test exposed a participant mute acknowledgement race. The adapter
now reconciles against the SDK's actual mute state, and the native check requires
both remote unmute and received audio. UI regression also exposed the risk of
saving transient voice/chat input locks during travel; explicit locks are now
saved separately. The final checks acknowledge first-visit rules before aiming,
and verify mute access while those rules are open.

Actual APK: **124,646,916 -> 126,880,470 bytes (+2,233,554)**, leaving
**873,119,530 bytes** under the strict cap. SHA-256:
**1BB034646EA0C1FE1FCEEE7597E78AD05B9B5EAF2DA536B13E4E17A0804316FF**. Official AndroidSubmission, v2 signature, ZIP CRC,
1,250-entry audit and five-scene bundle integrity passed. Native
ARM64 Vivox libraries, optional microphone hardware and required permissions are
present. Development probes and signing credentials are excluded. The initial
local task-entry APK was 113,758,996 bytes; the immediate pre-voice delivery was
113,823,708 bytes. The current delta also includes teammate rendering changes.

Physical phone microphone, Bluetooth routing, speaker echo, touch/FPS and mobile
network interruption remain unverified: the user requested continuing without a
connected phone. Broader legacy cart QA is also **not passing**. The pre-merge and
merged candidates reproduced the same 45 offline failures; the network fixtures
also failed. See `cart-baseline-comparison.json` and the retained GolfCartQA runs.
Those broader tests do not certify this delivery's cart gameplay.

Current evidence under `Builds/VoiceQA/`:

- `Merge-20261025/`: merge preservation, source proofs, package audit and recovery records.
- `UI-20261025-094855-797/Football/`: 81 checks; final timing-only fixture refinement leaves runtime unchanged.
- `UI-20261025-095259-165/Basketball/`, `UI-20261025-095259-166/Golf/`,
  `UI-20261025-095259-159/Fishing/`: remaining final voice UI checks.
- `Online-20261025-094435-764/`: live two-way synthetic audio; the transport is unchanged by later UI refinements.
- `Setup-20261025/`: service activation proof and earlier release records.

Text evidence: `Builds/RoomChatQA/Run-20261025-093750-7988/` and
`Run-20261025-094855-7990/`. Recovery:
`Builds/CrewConnectionsQA/Run-Golf-HostLoss-20261025-093344-171/`.
The complete superseded player/APK and intermediate checks were archived with
hash inventories; batch paths are recorded in `Merge-20261025/*-archive.txt`.
Repository checks run before the user-authorized commit and push.

Reproduce with `Test-VoiceState.ps1`, `Build-VoiceReview.ps1`, `Test-VoiceUI.ps1`
and `Test-VoiceOnline.ps1` in `Tools/Build/`; `Test-VoiceUI.ps1 -Port` supports
independent concurrent sport reviews. Android uses
`Tools/Build/Build.ps1 -Target AndroidSubmission`.

## Service setup and phone acceptance

The current linked project already has Vivox enabled and its public connection
settings are included in the repository. If linking a different Unity Dashboard
project, enable Vivox there and fetch the replacement settings through Unity
Editor **Project Settings > Services > Vivox**. Keep **Test Mode off** and rebuild
both players: Unity embeds the public fields in its generated services
configuration. A build without them disables Join voice. Do not paste the
dashboard's signing key into client settings or source. If activation
requires an account agreement or billing choice, the project owner must complete
that step. A failed voice connection leaves the game room usable.

For a phone acceptance check, install the current APK and put two devices in the
same online room. On each, hold the mic icon, choose **Join room voice**, grant
device permissions and explicitly unmute. Also test a short mic tap from the
disconnected state. Check both speech directions, speaker echo, headset routing,
per-person mute/volume, spectator group mute and output volume. Travel together between
islands and check that the channel stays connected. Background one phone, return,
and check that it reconnects muted. Deny/revoke microphone permission and confirm
that capture cannot be enabled until permission is restored. Leave the room and
confirm that its audio stops. Check a different room separately to confirm audio
separation. Built-in microphone and Bluetooth behavior require physical devices;
desktop simulation cannot certify them.

## SDK references

- [Unity Authentication integration](https://docs.unity.com/en-us/vivox-unity/developer-guide/vivox-unity-sdk-basics/sign-in-with-authentication-package)
- [Android runtime permissions](https://docs.unity.com/en-us/vivox-unity/developer-guide/android/request-runtime-permissions)
- [Echo cancellation](https://docs.unity.com/en-us/vivox-unity/developer-guide/acoustic-echo-cancellation)
- [Access tokens and channel authorization](https://docs.unity.com/en-us/vivox-unity/access-token-guide/access-token-guide-toc)
- [Synthetic audio injection and microphone mute behavior](https://docs.unity.com/en-us/vivox-core/developer-guide/channels/audio/core-audio-injection)
