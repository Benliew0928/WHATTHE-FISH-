# Room chat and fish stickers

The implementation follows the [approved visual prototype](ChatPrototype/README.md).
The same room conversation follows the crew through the lobby, island selection,
gameplay and return to the lobby. Players and spectators can send text or any of
the eight free stickers: GGWP, Laugh, Cry, Angry, Thanks, Oops, Wow and Let's go.

## Player experience

- Chat and a quick sticker button sit below the gameplay minimap; Fishing docks
  them beside its taller scoreboard so Ready and Start remain accessible. The lobby has
  its own chat button and a larger history panel on the right. There is no dimmed
  backdrop and opening history does not pause an online game.
- Enter opens the composer on desktop. Typing captures gameplay input. Send or
  close releases it; Escape closes chat before opening another game menu.
- Text appears above its sender for four seconds, and a sticker for three,
  followed by a 240 ms fade. A new message replaces that sender's old bubble.
  At most three bubbles are visible, prioritizing the local sender and recent
  messages. Messages remain in history when their bubbles are suppressed.
- Bubbles avoid fixed HUD/control regions, active HUD buttons, the open chat panel,
  player heads, the ball, Fishing's aiming reticle and each other. Offscreen or obstructed senders have no bubble;
  replays, ceremonies, recovery and travel also suppress bubbles. Lobby roster
  cards remain free of floating bubbles.
- History renders 32 messages at a time with Earlier/Newer buttons. The complete
  room timeline remains in memory. Tap a sender's name to mute/unmute them;
  muted messages stay retained but hidden. Chat preferences can hide all bubbles.
- Empty messages are ignored. Text is limited to 120 Unicode text elements and
  480 UTF-8 bytes, with literal markup and normalized control characters. Text
  and stickers share a 1.2-second send interval.

## Room lifetime and networking

`RoomChatService` belongs to the persistent `RoomService`; UI pages, sports and
network avatar objects do not own its history. Protocol **80** requires matching
players. Named NGO messages use reliable fragmented delivery on the existing
room connection. The server supplies sender identity, nickname, timestamp and
location context, and validates membership, content, sticker IDs and rate limits.
Request IDs prevent replayed requests from adding duplicate history entries.

Late joiners fetch history in bounded pages while live messages continue. Message
IDs deduplicate overlap between live delivery and snapshots. Each surviving peer
keeps received history during host recovery; the elected host serves its retained
copy to reconnecting peers. A recovery notice explicitly says earlier history may
be incomplete: messages in flight when the old host disappears cannot be guaranteed
to have reached every survivor. Accepted messages already held by a survivor are
preserved. Leaving the room clears local history and mute choices; a new room
starts empty. This is room-session history, not a cloud archive across app restarts.

## Artwork and builds

The original transparent 1254×1254 approved images and generation provenance
remain in [ChatPrototype/stickers](ChatPrototype/stickers) and
[generation-prompts.json](ChatPrototype/generation-prompts.json). Byte-identical
source copies live in `Game/Assets/_Game/Resources/RoomChat/`, relative to the
repository root. Unity imports 256×256 sprites with no mipmaps or readable CPU
copy; Android uses ASTC 4×4. `RoomChatTextureImport` preserves these settings on
fresh imports. PNG sources use the repository's Git LFS rules.

The development-only `RoomChatReview` probe and
`Tools/Build/Test-RoomChat.ps1` exercise real host, guest and spectator processes,
late-join paging, invalid messages, input focus, display sizes, sport transitions,
host recovery and new-room clearing. Runtime captures and raw evidence belong
under ignored `Builds/RoomChatQA/`. Android and Windows delivery measurements are
recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

Physical Android touch, native keyboard behavior, small-screen readability,
lossy texture appearance and phone frame rate require device validation; Windows
captures do not establish those results. Real WAN/Relay behavior also requires
an online device test beyond the local transport checks.

## Validated Windows delivery — 23 October 2026

The final candidate retains the newer shared control designs for all four sports
and both Football/Basketball filming launchers. The complete **329-file** player
is delivered in `Builds/WindowsFinal/`; its delivery note records promotion time,
verification and the prior-player archive. The final patch rebuild reused the
validated Windows scene bundle because it changed only chat code.

The final candidate passed **198 multiplayer assertions**, **95 all-island UI
assertions** and **63 host UI assertions**. Evidence is in
`Builds/RoomChatQA/Run-20261023-160511-7987/`,
`Run-20261023-160451-7986/` and `Run-20261023-160801-7988/`. Tests used real host,
guest and spectator processes launched from an unrelated working directory.
Earlier candidate checks also passed from a player path containing spaces.
The UI checks include visible guest bubbles, suppression over host setup buttons,
literal text, input capture/release, all eight sticker choices, and 1600×900,
1920×864 and 1024×768 displays. Fishing's scoreboard clearance has explicit
assertions and inspected captures. The sticker picker replaces history while
open, keeping the composer and launcher from overlapping.

All **453 compiled runtime C# documents** in the final Windows PDB matched the
checkout. The Android release's **321 compiled production documents** also matched,
including the Fishing fix and shared control designs. Evidence, source hashes,
complete player hashes, the successful build log and independent APK checks are
under `Builds/RoomChatQA/FinalReview/`. No later concurrent gameplay source was
discarded when combining the delivery. Filming and layout regression evidence
is documented separately in their respective task reports.

`Tools/Build/Test-RoomChat.ps1` defaults to the complete WindowsFinal player.
Use `-UIOnly` for host display/input checks, `-IslandUI` for Basketball, Golf and
Fishing gameplay views, or neither for multiplayer/history/recovery checks.
An explicit `-PlayerPath` can target a candidate built by
`WhatTheFish.Editor.RoomChatBuild.Build`.

Eight asset sources were verified byte-identical to their approved masters, with
transparent RGBA pixels and no PNG metadata or external dependencies. Imported
256px sprites passed runtime checks. Lossless Windows storage compression was
applied only to previous task-specific runtime caches; their SHA-256 hashes were
unchanged. It does not change delivered asset bytes or visual quality.

The final AndroidSubmission APK is **111,107,490 bytes**, leaving **888,892,510
bytes** below the strict cap. It includes the latest chat source; this is not an
older APK used to validate newer code. The task-opening APK was 110,721,046 bytes
(+386,444 overall); concurrent releases also changed during the task. The immediate
predecessor was 111,097,730 bytes (+9,760). Size attribution and largest entries
are recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

Earlier root/isolated build attempts failed during concurrent source changes and
a low-disk build attempt. Their output is not validation; the successful combined
release and independently verified source hashes supersede them. The build
wrapper reported an unset process exit code after the final Android build, while
Unity logged success and the fresh APK passed the budget, signature, ZIP,
catalog, dependency and exact audit checks.

Superseded isolated build inputs and the prior complete Windows player were
archived with the repository archive tool; see the current delivery note for
batch names. Binary hardlinks were detached and all 552 original hashes verified
before archiving the isolated inputs. Unity caches and installed dependencies
remain in place. Some earlier archive batches were removed by separate concurrent
cleanup, so they are not relied on for recovery. Current evidence remains under
`Builds/RoomChatQA/`.
