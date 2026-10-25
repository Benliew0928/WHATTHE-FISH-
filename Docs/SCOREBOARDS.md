# Sports scoreboards

## Integrated localization validation — 26 October 2026

Merged teammate revision `213700d` with the approved scoreboards and venue
previews. Backgrounds retain **80% opacity**. The localization catalog now covers
scoreboard headings, possession, scoring celebrations and detail controls in
English, Simplified Chinese and Bahasa Melayu. Player and venue names remain
user content. Bounded text fitting preserves readable statistics in all three
languages.

The final combined Windows player passed **1,173 offline expansion assertions
per language**, **952 multiplayer expansion assertions**, **185 scoring/display
assertions** and **35 score-state checks**. Checks include unclipped translated
captions at 1600×900, 1024×768 and 844×390. Chinese also passed from a copied
project path containing spaces, with the player launched from an unrelated
working directory. Captures were reviewed. Venue customization passed **1,116**
assertions. The broader localization review passed **3,555** checks before the
final scoreboard fitting correction; the focused final suites cover that fix.

Evidence: `Builds/TeamSyncQA/`, `Builds/ScoreboardDetailsQA/Run-20261026-001554-776/`,
`Run-20261026-001635-723/`, `Run-20261026-001956-254/` and
`Builds/ScoreboardQA/Run-20261026-002152-236/`. The complete tested player is
delivered in `Builds/WindowsFinal/`. Physical-phone touch and FPS remain untested.

Repeat with `Tools/Build/Test-Scoreboards.ps1 -Details -Language zh-Hans
-PlayerPath Builds/WindowsFinal/WhatTheFish.exe`; language choices are `en`,
`zh-Hans` and `ms`. See [integration report](TEAM-SYNC-20261026.md) for release
and cleanup details.

Selected on 25 October 2026: **Rally Heat** for basketball (first option) and
**Touchline Live** for football (second option). The references below are design
concepts, not game captures.

- [Rally Heat reference](VisualDirection/Scoreboards/rally-heat-concept.png)
- [Touchline Live reference](VisualDirection/Scoreboards/touchline-live-concept.png)

## Expandable scoreboards

The approved expansion designs are **Rally Ledger** for basketball (first
option) and **Club Roster** for football (second option):

- [Rally Ledger reference](VisualDirection/Scoreboards/rally-ledger-concept.png)
- [Club Roster reference](VisualDirection/Scoreboards/club-roster-concept.png)

Tap anywhere inside the compact scoreboard rectangle to expand or collapse it.
Desktop players can also use Tab; Escape closes the details before opening the
island menu. The main HUD and details backgrounds use 80% alpha, while text and
numbers remain fully opaque. Physical venue screens keep their solid faces.
Expansion eases over 240 ms and respects the reduced-motion preference.

Basketball shows both teams with points, assists, passes and interceptions.
Football shows two selectable rosters, goals and assists, with all four tracked
statistics for the selected player above them. Each team list scrolls separately
when needed. The football layout leaves room for the movement and action buttons;
its selectable rows retain a 44-pixel minimum height in the reviewed layouts.
The match keeps running and movement stays available while details are open.

[ScoreboardDetails](../Game/Assets/_Game/Shared/UI/ScoreboardDetails.cs) refreshes
the existing authoritative/replicated player records every 100 ms while open.
Team scores come from the match snapshot rather than a sum of player records,
which also handles uncredited scores correctly. Shared basketball practice shows
shared points and made/attempted baskets; individual match statistics start with
a team match. A statistics reset removes stale player rows. Changing match
context, disabling the HUD or losing application focus closes the panel.
The network schema is unchanged.

The +2, +3 / THREE POINTER and GOAL animations remain active with details open;
the banner temporarily occupies the panel heading without covering its rows.
The opt-in review uses controlled statistics on real match participants so
captures and host/guest assertions are reproducible. It never runs in ordinary
play and is excluded from the Android release.

## Appearance and scoring motion

Basketball uses a charcoal face, orange trim and shot-clock tile, large warm
digits, and explicit blue/red team labels. Shared practice replaces team scores
with shared points and made/attempted baskets. Football uses the compact chalk
and deep-green broadcast strip, a lime edge, a separate clock block and clear
blue/red identity marks.

Scoring uses a brief eased number pulse and a sliding score banner. Two-point
baskets show **+2 / BUCKET!**; three-pointers show **+3 / THREE POINTER** with
three moving chevrons and short rising streaks. The three-point treatment lasts
1.8 seconds; the two-point and football **+1 / GOAL!** treatments last 1.25
seconds. Effects use render-frame time and do not change gameplay, the camera,
score totals or match timing. The existing football replay and MVP programmes
remain responsible for the larger stadium show. Football hides rematch controls
during a winning-goal sequence; basketball moves them below an active score banner.

## Live display integration

- [SportsScoreboardState](../Game/Assets/_Game/Shared/UI/SportsScoreboardState.cs)
  reads the replicated match snapshots and remaining-time clocks. Reads are
  cached once per sport per render frame. Basketball practice reads the ball's
  shared points, made and attempted counters.
- [SportsScoreboard](../Game/Assets/_Game/Shared/UI/SportsScoreboard.cs) supplies
  the same visual component for players, spectators and physical venue screens.
  [ScoreboardBurst](../Game/Assets/_Game/Shared/UI/ScoreboardBurst.cs) owns only
  the bounded scoring animation.
- [VenueScoreboards](../Game/Assets/_Game/Shared/UI/VenueScoreboards.cs) upgrades
  all four basketball perimeter boards at runtime, including existing saved
  island scenes. Each complete screen and its modular piers move 1.6 metres
  towards the court so the upper arcade no longer covers the score. Custom venue names
  retain their existing `StadiumSign` binding.
- [FootballStadiumBroadcast](../Game/Assets/_Game/Sports/Football/FootballStadiumBroadcast.cs)
  places Touchline Live in the upper score area of both active giant displays;
  live video, goal replays, transition wipes and post-match MVP content remain.
  The original customizable welcome sign is separate from these runtime screens.

Initial snapshots, reconnects, display reactivation, practice/match changes,
downward score corrections and rematches establish a baseline without replaying old
celebrations. Duplicate snapshots do not retrigger an effect. Catch-up changes
larger than a single valid score update digits without inventing a single shot.
The network schema and score authority are unchanged.

All panels and effects use generated UI geometry and existing fonts; there are
no added delivery textures or external art dependencies. The physical displays
remain world-space geometry with ordinary depth testing. Venue graphics and
scoring effects remain passive; the HUD scoreboard and details handle pointer input.

## Stronger backgrounds revision

The approved follow-up increases background alpha from **50% to 80%**, retaining
**20% transparency** on the compact HUD, expanded statistics and scoring banners.
Opaque text and physical venue screens are unchanged. The complete **342-file**
Windows player is updated. **293** offline runtime assertions across both sports
and three resolutions, plus **35** score-state checks, passed. All **502 Windows**
compiled source documents match. Native captures were visually reviewed; phone
touch and FPS remain unverified.

Current previews and delivery proofs: `Builds/ScoreboardOpacityQA/`.
Runtime evidence: `Builds/ScoreboardDetailsQA/Run-20261025-233024-351/`.
The preceding 50% release is recoverable from
`Legacy/20261025-233127-118-scoreboard-opacity-android/` and
`Legacy/20261025-233203-816-scoreboard-opacity-windows/`.
See [build sizes](BUILD-SIZE.md) for the new measured Android release.

## Original expansion verification and delivery

The complete **342-file** player in `Builds/WindowsFinal/` includes the approved
translucent expansions and the latest compact communication dock. **597**
expansion assertions passed across both sports offline and with a host/guest
pair. These cover the five click positions on each full rectangle, authoritative
player data and updates, selecting either football team, scrolling to the last
player, safe-area bounds, football action-button clearance, rendered numbers,
reduced motion, Back handling, focus loss and clean reset. Offline fixtures use
ten participants; network fixtures use four. Shared practice has no invented
individual statistics. All **185** existing scoring/display assertions and
**35** score-state checks also passed. The network protocol remains **87**.

Native game captures at **1600x900**, **844x390** and **1024x768** were visually
reviewed. Current previews are in `Builds/ScoreboardDetailsQA/Preview/`.
The expansion runs are `Run-20261025-231143-898` and
`Run-20261025-231527-453` under `Builds/ScoreboardDetailsQA/`; the scoring
regression run is `Builds/ScoreboardQA/Run-20261025-231659-554/`.
The last change only corrects the development football fixture to create its
expected four-player roster. Production runtime sources match the reviewed
captures. All **502 Windows** compiled source documents and the voice
configuration bridge match the retained source snapshot. The complete delivery
was hash-verified, including runtime libraries, data and launchers.

The official AndroidSubmission is **126,953,970 bytes**, up **40,488** from
**126,913,482**, with **873,046,030 bytes** of strict-cap headroom. Signature,
CRC, all **1,250** audit entries, five-scene bundle integrity and all **361**
compiled Android source documents passed. Runtime scoreboard types are present;
development probes are excluded. See [build sizes](BUILD-SIZE.md) and
[APK audit](APK-SIZE-AUDIT.md).

Repeat expansion checks with
`Tools/Build/Test-Scoreboards.ps1 -Details -PlayerPath Builds/WindowsFinal/WhatTheFish.exe`.
Omit `-Details` to run the existing score/display suite. Both runners launch from
an unrelated working directory and resolve their defaults from the script.
Desktop aspect-ratio previews do not verify physical-phone touch, appearance or
FPS. Current source, package and file-integrity proofs remain in
`Builds/ScoreboardDetailsQA/`.

Recovery batches:
`Legacy/20261025-231730-555-scoreboard-details-android/` and
`Legacy/20261025-232011-253-scoreboard-details-windows/`.

## Original scoreboard verification

The authoritative snapshot/model checks and opt-in executable
review live in [ScoreboardBuild](../Game/Assets/_Game/Editor/ScoreboardBuild.cs),
[ScoreboardReview](../Game/Assets/_Game/Shared/Core/ScoreboardReview.cs) and
[Test-Scoreboards.ps1](../Tools/Build/Test-Scoreboards.ps1). Current local evidence
is under `Builds/ScoreboardQA/`.

Windows validation passed **35** state/score-event checks and **185** runtime
assertions across basketball and football, offline and with a host/guest pair.
This includes all physical displays, both teams scoring, dedicated three-point
motion, animation completion, duplicate suppression, reactivation, reset, custom
basketball venue names, winning-score controls, and 844x390, 1600x900 and
1024x768 layouts. The full suite passed again on the combined player containing
the completed communication updates. All **500** compiled Windows source
documents match the workspace. Actual HUD and venue captures were visually
reviewed, including the corrected arcade sightlines. Earlier **38** football
broadcast checks passed on the isolated scoreboard player: a physical goal
updates the score, recorded replay plays, live coverage returns, and teardown
releases the display resources. The broadcast code is unchanged in the combined
player. These tests do not establish physical-phone appearance, touch or FPS.

The complete **342-file** combined player is in `Builds/WindowsFinal/`; all files
were hash-verified. It retains venue customization, the updated communication
controls and protocol **87**. Use matching Windows/Android builds. The final
combined scoreboard run is `Builds/ScoreboardQA/Run-20261025-214404-694/`.
Source proofs, APK integrity checks and the delivered-file inventory are the
`combined-*` records in `Builds/ScoreboardQA/`.

Repeat the runtime suite against the delivered player with
`Tools/Build/Test-Scoreboards.ps1 -PlayerPath Builds/WindowsFinal/WhatTheFish.exe`.

The task began with a **126,880,470-byte** APK. The intermediate venue release
was **126,891,150 bytes**, and an intermediate communication release was
**126,917,874 bytes**. The final combined official AndroidSubmission is
**126,914,878 bytes**: **+34,408** from task entry, **-2,996** from the immediate
pre-release APK, with **873,085,122 bytes** below the strict limit. These deltas
include concurrent communication and venue work, not only scoreboards.
SHA-256: **E4D0D105F065B6AE50041453EE86433407C8D08DF122C3DB0B8C1D7137755FF1**.
The v2 signature, ZIP CRC, all **1,250** audit entries, five-scene bundle integrity
and all **360** compiled Android source documents passed. See
[build sizes](BUILD-SIZE.md) and [APK audit](APK-SIZE-AUDIT.md).

Early iteration used `Builds/ScoreboardQA/Tested Snapshot/` to avoid concurrent
serialized-type edits. Its **498** source documents and final offline checks
passed from a path containing spaces, with players launched from the system
temporary directory. That isolated player has been superseded by the combined
delivery. Earlier failed attempts remain as diagnostic evidence.

Superseded complete players were archived in
`Legacy/20261025-165158-133-scoreboard-windows/`,
`Legacy/20261025-214310-873-scoreboard-winning-polish/` and
`Legacy/20261025-214648-489-scoreboard-combined-windows/`.
The superseded isolated candidate and its successful 126,901,238-byte Android
build are in `Legacy/20261025-214747-770-scoreboard-isolated-output/`;
their logs, audit and source snapshot remain under `Builds/ScoreboardQA/`.
