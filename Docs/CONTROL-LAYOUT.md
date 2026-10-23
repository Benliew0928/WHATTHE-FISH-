# Personal control layouts

Settings → Layout is available in Basketball, Football, Golf and Fishing.
The current HUD selects the sport, including after changing islands. Main menu
Settings → Layout offers all four sports without entering a match.

Each editor dims the background and shows the game's actual button artwork,
including actions that are temporarily unavailable. Tap and drag a button to
move it. Drag its corner handle or use the size slider to resize. Thin corner
marks indicate editable bounds; the selected button has an accent outline.
There are no replacement cards or extra captions covering the controls.

The compact bar contains Cancel (X), Reset, Save, a control picker and the size
slider. Fishing's movement and left-reel controls appear one at a time; choose
the left-reel control in the picker to edit it. The small arrow moves the bar
between the top and bottom. Selecting a control behind the bar also moves the
bar out of its way. The resize handle stays inside the control's bounds.

Save stores changes and returns to settings. Cancel or Android Back/Escape
discards the draft and returns to settings. Reset restores the sport's default
layout in the draft; Save is still required. Normal settings reset does not
erase layouts.
During a game the local player's input is blocked; multiplayer timers continue.

## Scope and appearance

| Sport | Adjustable controls | Editor style |
| --- | --- | --- |
| Basketball | Move, Shoot/Guard, Pass/Steal, Jump, ability | Glossy cyan/navy frames, gold and cyan actions |
| Football | Move, Shoot/Pressure, Dash, Pass, Tackle, ability | Glossy cyan/navy frames, gold/cyan actions and lime Dash |
| Golf | Move, Jump, shot power, Aim/Cancel, Start, Buggy/Recall, Drive/Exit | Course cream and gold, golf artwork |
| Fishing | Move, Jump, Cast/Reel right, Reel left, cancel, round skill | Lagoon cream and turquoise |

Main-menu editing tools use the menu's navy surfaces and sport accents.
Buttons retain their gameplay artwork and fonts in both editor contexts.
Scoreboards, the free-look surface, settings access and setup dialogs retain
their normal layouts. Control labels describe shared actions; the game continues
to decide when each action is available.

Positions are normalized to the safe HUD frame. Controls are clamped inside it,
and the settings corner remains accessible. Size is bounded to 70–160% of each
control's responsive default. Contextual controls can intentionally share a
position; players should keep simultaneously used buttons apart.

## Persistence and implementation

Layouts are **local to this device**, separately keyed by identity and sport.
Signing into an existing account reads that account's local layouts. Linking a
guest profile copies layouts only when the account has no existing layout.
This feature does not yet synchronize preferences between devices.

`ControlLayouts` uses versioned JSON and hashed profile keys in PlayerPrefs.
Invalid documents fall back to defaults; unknown IDs are ignored by the HUD.
Editing never writes until Save.

`ControlLayoutEditor` wraps existing rectangles while preserving their normal
responsive layout and animation. Related shot guides share the wrapper. Art,
touch regions, joystick sensitivity and gesture coordinate systems therefore
move and scale together. During editing, `ControlLayoutVisuals` copies the real
HUD's graphics and transforms without gameplay handlers or listeners. Original
controls are hidden while their visual copies are shown above the dimming layer.
Copies preserve typography, image proportions and child placement. Main-menu
samples call the same `ControlDesigns.Build` factory as gameplay. They do not
start games, spawn players or create network sessions.

## Changing button designs

Every editable control in all four sports has one presentation builder. A
design edit there reaches gameplay, the in-game Layout editor and the main-menu
Layout editor after rebuilding and reopening the UI. Layout preferences store
only position and scale, so existing saved layouts retain the new artwork.
The sports keep their individual visual styles.

The approved replacement artwork and regeneration workflow are documented in
[CONTROL-ART.md](CONTROL-ART.md). Shared frames and transparent icons are reused
across sports; captions remain native text.

| Design source | What to edit |
| --- | --- |
| [ControlDesigns.Glossy.cs](../Game/Assets/_Game/Shared/UI/ControlDesigns.Glossy.cs) | Shared glossy frames, caption styling, translucent joysticks, disabled/ready states and Cancel |
| [ControlDesigns.cs](../Game/Assets/_Game/Shared/UI/ControlDesigns.cs) | Shared dimensions, movement sticks, typography helpers and factory routing |
| [ControlDesigns.Courts.cs](../Game/Assets/_Game/Shared/UI/ControlDesigns.Courts.cs) | Basketball and Football faces, illustrations, captions, hints, stamina and ability badges |
| [ControlDesigns.GolfFishing.cs](../Game/Assets/_Game/Shared/UI/ControlDesigns.GolfFishing.cs) | Golf slider, utility artwork and captions; Fishing action, countersteer, cancel and skill artwork |
| [GolfUILayout.asset](../Game/Assets/_Game/Resources/GolfUILayout.asset), [FishingUILayout.asset](../Game/Assets/_Game/Resources/FishingUILayout.asset) | Existing Golf/Fishing size and layout configuration, also read by previews |

`ControlLayoutPreview` controls sample placement only; it contains no button
drawings, typography or separate design sizes. `ControlLayoutVisuals` copies
presentation for editing and delegates menu artwork to the shared factory.
Input handlers, listeners, contextual availability and match authority remain
in the gameplay components. Runtime captions may change with gameplay state;
their base typography comes from the shared design.

`Test-ControlLayout.ps1 -DesignMutation` additionally changes the shared font
sizes, plate borders and artwork hierarchy before controls are created. It
checks those edits on real HUD controls and all four menu Layout pages, alongside
the normal movement, resizing, safe-frame, persistence and input-isolation checks.

Build a candidate with Unity method
`WhatTheFish.Editor.ControlLayoutBuild.Build`. Run
`Tools/Build/Test-ControlLayout.ps1 -PlayerPath <candidate-executable>`.
The suite uses isolated review identities, actual HUD raycasts, pointer events,
three display sizes, main-menu previews and separate process restart checks.
Current raw evidence belongs under ignored `Builds/ControlLayoutQA/`.

Physical Android multi-touch, cutout behaviour and performance require a device
run; Windows checks do not establish those results.

## Shared design validation — 23 October 2026

All **24** editable controls now use shared presentation factories in gameplay
and the main-menu samples. In-game editing copies those same designs. The old
separate preview drawings and size tables were removed; runtime captions retain
the factory's base typography. Input and contextual actions remain in their
existing gameplay components.

The complete Windows candidate passed **2,586 assertions**: **1,185** normal
layout checks and **1,401** checks with deliberate changes to shared typography,
plate borders and the artwork hierarchy. Each suite covers all four sports,
all four main-menu previews and restart persistence. Tests used the default
runner paths with the complete player in a folder containing spaces, and an
unrelated working directory. Actual/menu artwork pairs for all four sports and
the responsive captures were inspected.

Current evidence: `Builds/SharedControlDesignQA/`, including the portable review
runs under `Portable Review/Builds/ControlLayoutQA/`. All **329** complete player
files matched the tested candidate by SHA-256 when promoted to
`Builds/WindowsFinal/`, before the delivery note was replaced.

The Windows snapshot contains **453** compiled runtime C# documents. The control
design sources match the checkout. Three later concurrent room-chat files are
outside that snapshot: `RoomChatReview.cs`, `RoomChatUI.Bubbles.cs` and
`RoomChatUI.cs`. Exact differences are in
`Builds/SharedControlDesignQA/concurrent-source-differences.json`. The preceding
validated protocol-80 room chat, eight stickers and filming work are retained;
no newer validated delivery was replaced.

Two historical auxiliary Golf probes were investigated but are **not** counted
as passing evidence: `GolfSwingProbe` fails its two-ball setup before button
interaction, while `GolfHUDReview` still expects the former image-only Aim
caption. The current source already used a native Aim label before this task.
The historical Swing runner also failed to complete against the preceding
Windows delivery. This task's passing layout suites do not establish Golf
physics or physical-phone performance.

The first full Windows attempt ran out of disk space while packaging. The
successful retry is the tested release. Recovery archives:
`Legacy/20261023-154823-417-shared-controls-old-windows-bundle/` and
`Legacy/20261023-160046-645-shared-controls-windows/`. Android measurements and
its recovery record are in [BUILD-SIZE.md](BUILD-SIZE.md).

## Visual correction and validation — 20 October 2026

The oversized placeholder cards were replaced with actual gameplay artwork.
Live editors copy graphic components and original transforms, never gameplay
scripts or button listeners. Main-menu samples reuse the game's button assets,
fonts, plate geometry, joystick-circle generator and native design sizes.
The game buttons' labels are not restyled to the toolbar's font.

The final complete Windows player passed **996 assertions**: Basketball 238,
Football 244, Golf 247 and Fishing 255, plus three restart checks per sport.
Checks include original typography, child geometry, input-free visual copies,
no duplicate captions, selecting contextual controls, hitboxes, movement,
resizing, cancellation, saving and restart persistence. The default-path script
ran from an unrelated working directory in a checkout with spaces. Screens at
1600×900, 1280×720, 1920×900 and 1024×768 were visually inspected.

Final evidence is in
`Builds/ControlLayoutVisualQA/Portable Review/Builds/ControlLayoutQA/Run-20261020-233032-300/`.
All **405** compiled C# documents match current source. The complete **327-file**
player was hash-verified when promoted to `Builds/WindowsFinal/` and reopened.
The final build retains the current profile, venue and bot work; this suite
validates control layouts and does not replace those features' own tests.

Superseded output is recoverable from
`Legacy/20261020-232147-645-layout-visual-old-candidate/` and
`Legacy/20261020-233355-953-layout-visual-delivery/`.
The latter contains the previous complete Windows player and the two reviewed
iteration runs. Current evidence remains under `Builds/ControlLayoutVisualQA/`.
Android package measurement is recorded in [BUILD-SIZE.md](BUILD-SIZE.md).
No phone was attached; physical Android touch, cutouts and performance still
require device validation.

## Original implementation validation — 20 October 2026

The final Windows candidate passed **286 assertions**: Basketball 70, Football
70, Golf 67 and Fishing 67, plus three restart checks per sport. The suite used
default script paths in a checkout containing spaces and launched the game from
an unrelated working directory. It covers real HUD raycasts and gestures,
relocated joystick response, shot-guide alignment, resizing, bounds, pointer
ownership, draft cancellation, resets, account separation, guest linking,
invalid preferences and all four main-menu previews. Captures at 1280×720,
1920×900 and 1024×768, the main chooser and sport editors were inspected.

Evidence:
`Builds/ControlLayoutQA/Portable Final Review/Builds/ControlLayoutQA/Run-20261020-221036-345/`.
An earlier feature candidate also passed the existing **110** Golf/Fishing
settings checks in `Builds/IslandSettingsQA/Run-20261020-215847-419/`.

The shared complete player in `Builds/WindowsFinal/` was refreshed by the
concurrent island-bots delivery. Its game code and content match the tested
layout candidate: 326 of 327 files matched before delivery notes were updated;
only `boot.config` build/debug connection identifiers differed. A further
**73** Basketball layout/restart assertions passed on that delivered player.
See `Builds/ControlLayoutQA/delivered-player-comparison.json` and
`Builds/WindowsFinal/LATEST-BUILD.txt`.

All layout-task C# documents match compiled source checksums. Later concurrent
changes to `PlayerProfiles.cs` and `SocialPanel.cs` were outside that Windows
snapshot; the layout suite does not certify those other features.
Android is a later, separate snapshot; its measured package and audit are in
[BUILD-SIZE.md](BUILD-SIZE.md) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).

Earlier task output was archived in
`Legacy/20261020-220842-556-control-layout-first-player/` and
`Legacy/20261020-221720-158-control-layout-delivery/`. The latter contains the
six completed layout-only moves recorded in `completed-moves.txt`; its proposed
Windows player move was blocked by a file lock and did not occur.
The independent island-bots delivery archived the superseded complete player.
Superseded APK/audit backups are in
`Legacy/20261020-222310-956-control-layout-apk-backup/`; the final APK and its
size/signature evidence remain under `Builds/ControlLayoutQA/`.
