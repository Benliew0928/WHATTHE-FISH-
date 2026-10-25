# Shared menu design

24 October 2026. Implements the remaining families in the
[UI audit](REMAINING-UI-DESIGN-AUDIT.md), after merging `7274043` from `origin/main`.

## Appearance and editing

The remaining menus use a navy frame, cyan inner rim, cream surfaces, a restrained
upper highlight and a shallow shadow. Gold identifies a selected choice; mint is
used for continuation actions and coral for leaving. Blue and Red team cards have
distinct crests, readable occupancy/full labels and an explicit Start match action
in solo play. Fishing skill cards retain the approved illustrations and provide
more room for their titles and descriptions.

Edit [MenuDesigns.cs](../Game/Assets/_Game/Shared/UI/MenuDesigns.cs) to change the
shared menu palette, rims, shading, corners and pressed/selected/disabled states.
`MenuSurface` distinguishes buttons, cards, dialogs, slider tracks and handles.
The existing view owns its text, layout, click listeners and game state. The mesh
responds to presses while the original touch rectangle remains stationary.
Reduced motion disables press travel. Decorative crests and highlights ignore
raycasts. No new textures or duplicate sprite atlases are required.

The Basketball and Football button factories apply the design automatically.
Fishing, the illustrated island rules, interactive lessons and spectator controls
opt into the same renderer. Plates used as meters, dividers, transparent targets
and unrelated game graphics keep their own rendering.

Gameplay control artwork remains centralized in
[ControlDesigns.cs](../Game/Assets/_Game/Shared/UI/ControlDesigns.cs), including its
glossy variants. The in-game and main-menu Layout pages still display that same
live design. Menu changes belong in `MenuDesigns`; changing a gameplay control's
art belongs in `ControlDesigns`. Neither needs edits to separate Layout copies.

## Coverage

| Family | Implemented treatment |
| --- | --- |
| Football and Basketball team selection | Larger team cards, A/B crests, Blue/Red colours, selected and full states, shared confirmation button and dialog |
| Court match setup | Start/New match, bot count, Fill/Remove bots and adjacent score strips |
| Fishing skill draft | Shared outer dialog and choice cards; all six skills reuse their existing icons |
| Settings and leave confirmations, all four sports | Frames, close/back/leave actions, camera cards, selected choices and audio sliders |
| Illustrated rules, all four sports | Shared card and Skip/Back/Next/Done actions, preserving the teammate's new illustrations and entry flow |
| Older court/Golf guide builders | Their retained action helpers and frames also receive the design; normal help opens the shared illustrated rules |
| Court ball effects | Trail cards, selected state and footer actions |
| Court awards/results | Show results, statistics and Next adventure actions through the shared helpers |
| Fishing setup/results/ceremony | START, help, leaderboard frame, results card, reopen/close and Show results |
| Both Layout entry points, all four sports | Toolbar, Cancel/Reset/Save, control picker, size slider and resize handle |
| Interactive lessons | Coach card, Quit/Retry/Skip/Lessons, lesson rows and Back/Quit actions |
| Spectators | Camera, Return/Leave room and host Start match |

The newer navy main menu, player hub, chat, travel UI, illustrated welcome and
Golf leaderboard retain their established art. They were reviewed separately in
the audit and were not the unfinished flat menu family.

## Reproducing the review

From any working directory, invoke the scripts by path:

```powershell
Tools/Build/Build-MenuDesignReview.ps1
Tools/Build/Test-MenuDesign.ps1 -PlayerPath Builds/MenuDesignQA/Player/WhatTheFish.exe
Tools/Build/Test-ControlLayout.ps1 -PlayerPath Builds/MenuDesignQA/Player/WhatTheFish.exe -Sport All -DesignMutation
Tools/Build/Test-IslandRules.ps1 -PlayerPath Builds/MenuDesignQA/Player/WhatTheFish.exe
```

The first two runners resolve relative player/output paths from the repository
root. Existing runners should receive an absolute `-PlayerPath` when invoked
from another working directory. `Build-MenuDesignReview.ps1 -ScriptsOnly` refreshes
an already complete candidate player. Review fixtures require explicit command
line flags and are excluded from Android release builds.

Current captures, logs and build evidence are kept under ignored `Builds/`.
See [build size](BUILD-SIZE.md) for the measured delivery and validation limits.

## Delivered verification

The complete **327-file** Windows player payload is in `Builds/WindowsFinal/`, built on
24 October at 14:21 MYT and refreshed with final scripts at 14:49 MYT. Every
copied payload hash matched before updating its delivery note. Seven existing
relative-path trailer/highlight launchers were also retained. All **469**
compiled Windows source documents and **336** Android runtime source documents
match the current source. The build includes the merged teammate tutorials and
the concurrent sports prop implementation. Android development fixtures are
excluded; their final maintenance does not alter release runtime code.

The successful suites contain **3,373** assertions:

- **632** menu checks across all four sports, including actual EventSystem
  presses, camera choices, input blocking, team switching/confirmation, Fishing
  skill choice and landscape sizes 1600x900, 1024x768 and 1920x864.
- **1,806** Layout checks, including save/restart and design mutations reaching
  all 24 shared gameplay controls in both editor contexts.
- **125** illustrated-rules checks across all four sports; **155** interactive
  Golf lesson checks covering the shared coach, chapter list and actions.
- **174** court ceremony/results checks and **370** host/guest spectator checks.
- **111** portable-path checks from a directory containing spaces, invoked from
  an unrelated working directory with the default player path.

Rendered team cards, settings, skill choices, results, spectator actions and
the opaque lesson-list panel were visually inspected. Fishing's broader test
also passed the revised setup, rules, awards/results and all six skill-card
controls before failing at `AIM_SIX_TARGETS_PIER_0_CAMERA_2_SLOT_0`. The failure
repeated with the existing Island camera; the test's synthetic aim hit its pitch
limit and selected a neighbouring fish. This is an unresolved broader gameplay
test limit, not a passing full Fishing regression. Camera behavior was not
changed for this menu task. No physical Android touch, appearance/FPS or live
WAN validation was performed.

`Builds/MenuDesignQA/verification-runs.json` indexes the successful suites and
the retained failure. The same folder holds source proofs, APK integrity,
source snapshot and complete Windows promotion hashes. The APK is
**113,758,996 bytes** with **886,241,004 bytes** of strict-cap headroom.

Recovery batches: `Legacy/20261024-143909-542-shared-menu-android/`,
`Legacy/20261024-145351-137-shared-menu-windows/` and
`Legacy/20261024-145433-358-shared-menu-qa/`. They retain the previous deliveries,
intermediate test failures, superseded script-build logs and duplicate portable
test player. Current validation evidence remains under `Builds/`.
