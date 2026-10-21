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
| Basketball | Move, Shoot/Guard, Pass/Steal, Jump, ability | Courtside cream and orange |
| Football | Move, Shoot/Pressure, Dash, Pass, Tackle, ability | Pitch cream and lime |
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
samples use the gameplay artwork, fonts and native control dimensions; they do
not start games, spawn players or create network sessions.

Build a candidate with Unity method
`WhatTheFish.Editor.ControlLayoutBuild.Build`. Run
`Tools/Build/Test-ControlLayout.ps1 -PlayerPath <candidate-executable>`.
The suite uses isolated review identities, actual HUD raycasts, pointer events,
three display sizes, main-menu previews and separate process restart checks.
Current raw evidence belongs under ignored `Builds/ControlLayoutQA/`.

Physical Android multi-touch, cutout behaviour and performance require a device
run; Windows checks do not establish those results.

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
