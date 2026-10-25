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

The Voice button sits beside text chat and stickers. Join voice requests device
permission and connects with the microphone muted. Mic, speaker, output volume,
individual mute and speaking indicators are available in the voice panel. Up to
20 participants fit in its scrolling roster. Voice and text panels are mutually
exclusive; opening voice captures gameplay input without pausing the room.
Speaker/volume preferences persist on the device; individual mutes belong to the
current room. Microphone enablement never persists into a new room or reconnect.
A compact mute button remains reachable during travel, Golf aim mode, expanded
Golf standings or other full-screen overlays when the mic is on. Voice volume affects voice playback independently
of the game's sound effects.

Travel, settings and result overlays save only the explicit gameplay input lock.
Voice, chat and tutorial blockers are evaluated live, so dismissing one cannot
leave movement locked after an overlay or trip completes. Island rules hide the
chat controls while retaining the microphone mute shortcut.

## Phone audio

Android requests `RECORD_AUDIO` when the player joins voice. Android 12 and newer
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

## Verification and delivery — 25 October 2026

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
same online room. On each, open **Voice**, choose **Join voice**, grant the device
permissions and explicitly unmute. Check both speech directions, speaker echo,
headset routing, per-person mute and output volume. Travel together between
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
