# Room chat and stickers — design review

Approved on 23 October 2026. The Unity implementation is documented in
[Room chat and fish stickers](../ROOM-CHAT.md). The browser artifact below remains
the original design prototype; its screenshots are not runtime test evidence.

23 October 2026. This is a working local interaction prototype over actual game
captures, prepared for visual review. It is **not integrated into Unity or the
multiplayer transport**. The background players and camera are still images.
The preview controls outside the game frame are review tools, not proposed HUD.

Open [the prototype](index.html) in a browser. It works directly from disk, or
through any local static server serving the repository. All resources are local
and relative. No account, network service, CDN or API key is required. The font
uses the existing repository's `Game/Assets/_Game/Resources/Menu/CoveText.ttf`.

## Try it

- Open Chat, type a message and send. Its full text enters room history; a short
  version appears above the local player. Enter sends and Escape closes chat.
- Tap the fish button for eight quick stickers. A tap sends and closes the tray.
- Switch Room lobby, Football and Basketball; the same text/sticker history stays.
- Receive a message demonstrates the unread badge. Opening chat clears it.
- Replay bubbles demonstrates the expiry. Initial examples are held for review.
- Hide bubbles suppresses character overlays while messages still enter history.
- The gallery below the game also sends a sticker for quick visual comparison.

Messages are simulated locally. The prototype retains the timeline for this
browser tab using session storage where supported, including on reload. It is
not a persistent account history. Example bot messages are fixtures; the game
bots do not currently send chat. The room capture contains two test profiles
named Ben; Guest is the preview's label for the other participant.

## Proposed interaction

| Element | Design |
| --- | --- |
| Text bubble | Cream surface, dark teal text and outline; maximum two lines; 4 seconds plus a 240 ms fade. Full text stays in history. |
| Sticker bubble | Transparent artwork with cream cutout outline; 88 px square at the 1600 × 900 reference canvas; 3 seconds plus fade. |
| Sender | In gameplay, one active bubble per player; a new message replaces that player's previous bubble. |
| History | One room timeline with sender, time and location label. Scene changes add a quiet divider. Text and stickers use the same timeline. |
| Gameplay entry | Small Chat button plus fish shortcut below the minimap; closed by default, no dimming layer. |
| Gameplay panel | 362 × 310 reference pixels; fits between the minimap and movement control in these captures. |
| Lobby panel | Larger right-hand panel, opened on demand. Closing restores access to the invitation and travel controls underneath. |
| Text composition | 120 character prototype limit, empty messages ignored, literal text rather than interpreted markup. |
| Sending | Free sticker use; brief 1.2 second prototype interval to avoid repeated taps. No currency or unlock gate. |

The 1600 × 900 reference frame scales as a whole in this review. This is not an
Android dp layout. Production needs minimum 44 dp touch targets, readable mobile
text, safe-area/IME handling and separate compact landscape/large-screen layouts.
Typing captures keyboard movement only while the field has focus. Sending or
closing returns input to gameplay; online play does not pause.

Lobby messages use the shared history instead of floating over the compact
roster cards, which would obscure player/spectator tabs. Floating character
bubbles appear in the game. Text and stickers can be sent in either place.

## Keep the play area clear in Unity

Anchor to the player's head/nameplate in screen space with a small gap; do not
enlarge a distant sticker until it dominates the avatar. Reserve rectangles for
the scoreboard, minimap, controls, ball/aim indicators and nearby character heads.
Resolve overlaps with a short positional adjustment; suppress a bubble when no
safe position exists. Keep the message in history regardless. Limit simultaneous
visible bubbles to three, prioritizing the local player and recent nearby sends.
Never stack a column of messages over one player. Cull behind-camera and occluded
players, avoid edge-pinned bubbles, and suppress during replays/cinematic travel.
Give players Hide bubbles and sender mute controls in the production chat menu.

The preview uses manually placed anchors for its fixed captures. It does **not**
implement projection, collision avoidance, occlusion or mobile keyboard behavior.
Football's GGWP example was moved above the remote player's nameplate to avoid
covering an adjacent avatar. That is visual placement evidence, not a runtime
collision test.

## One room across all four islands — implementation plan

Attach a room-scoped chat service to the existing persistent room lifecycle,
alongside `RoomService`, rather than to each sport scene. Keep the current model:
one active game per host room and everyone travelling together. Lobby, travel,
football, basketball, golf, fishing and spectators read the same timeline.

A message envelope should carry a stable room-session identifier, host-assigned
sequence, sender's authenticated/player identity, display-name snapshot, UTC
timestamp, location context, message kind, and either bounded plain text or a
whitelisted sticker ID. Use the existing reliable networking channel. Transmit
the sticker ID, never the PNG bytes. Validate membership, payload size and the
send interval on the host; show pending/failed state until acknowledgement and
deduplicate retries by a client message ID.

Retain the room's complete timeline for its lifetime and page older messages in
the history UI; a small rendering window must not silently delete older messages.
New/returning room members request a snapshot plus messages since their last
sequence. Host migration must transfer the timeline and sequence watermark as
part of room recovery. If history recovery fails, show an explicit interruption
marker rather than implying completeness. A new room uses a new session key and
starts an empty history. Leaving clears that room's local view; cross-room or
permanent account history is outside this proposed scope.

Before implementation delivery: test host/guest text and every sticker, full
history across all four sports, late join, reconnect, host migration, duplicate
acknowledgements, invalid IDs, input focus, full-screen/compact layouts, mute,
overlapping avatars and touch keyboard. Then run AndroidSubmission, inspect its
size audit, and deliver the complete validated WindowsFinal player.

## Artwork

Eight original matching 2D orange fish reactions: GGWP, Laugh, Cry, Angry, Thanks,
Oops, Wow and Let's go. Mint sports headband, clean dark teal contours, cream
cutout border, expressive cel shading and transparent backgrounds. All source
images are 1254 × 1254 RGBA PNGs under [stickers/](stickers/), retained at their
generated resolution for future edits. This is a raster master set, not vector
source or a production texture atlas. PNGs follow the repository's Git LFS rule.

Created with the built-in image generation tool. The exact final prompts and
reference relationships are in [generation-prompts.json](generation-prompts.json).
The angry variant was regenerated as one clean silhouette after reviewing edge
noise in earlier versions. Both rejected workspace variants were archived with
hashes by the repository archive tool in
`Legacy/20261023-135603-290-chat-sticker-drafts/` (ignored, relative to repo root).

## Capture provenance

The following existing validation captures were copied without pixel editing.
Paths below are relative to the repository root. They are visual references,
not fresh validation of this prototype in the game.

| Local reference | Original capture |
| --- | --- |
| [Football](reference/football.png) | `Builds/TargetPassQA/Football-Offline-20261023-130930-066/offline/target-view.png` |
| [Basketball](reference/basketball.png) | `Builds/TargetPassQA/Basketball-Offline-20261021-150545-630/offline/target-view.png` |
| [Lobby](reference/lobby.png) | `Builds/StorybookMenuQA/LobbyRefresh-Feedback/host/room-Football-host.png` |

## Validation and delivery limits

Browser interaction checks cover typed messages, sticker sends, history through
scene changes/reload, unread clearing, quiet mode, expiry, quick-tray dismissal,
literal markup handling and image loading. Source syntax is checked with Node.
RGBA dimensions and alpha ranges are recorded in the ignored
`Builds/ChatPrototypeQA/sticker-audit.json`. Review screenshots and task-readiness
output also live under `Builds/ChatPrototypeQA/` relative to the repository root.

Final visual evidence: `football-review.png`, `lobby-room.png`,
`basketball-picker.png`, `sticker-sheet.png`, and `overview.png` in that folder.
The prototype also loaded with all images present from the copied
`Builds/ChatPrototypeQA/Portable Review/` layout, including its space-containing
path. Its local font dependency was copied in the same repository-relative
position. No absolute dependency is embedded in the HTML, CSS, JS or PNGs.
Superseded and clipped review captures were archived with the repository tool in
`Legacy/20261023-140659-111-chat-prototype-review-drafts/`.

No game code, Unity asset, build configuration, APK or Windows player was changed
by this design task. The opening APK measured **110,706,458 bytes**, with
**889,293,542 bytes** of headroom to the cap. That existing APK does not validate
this prototype. No Unity build was required for these documentation/design-only
files. Real multiplayer, Android touch/appearance/FPS and dynamic world-space
behavior remain unverified. Unrelated workspace changes were preserved; nothing
was staged, committed or pushed.
