# Golf and fishing settings

Golf now has an always available gear at the upper left of its safe HUD frame,
clear of the start control and upper-right ranking card. Fishing keeps its gear
at the upper right. Both use `IslandSettingsMenu`, with the existing courts'
cream plates, navy outlines, icon atlas, fonts, press feedback and shared audio
preferences. No new bitmap artwork is required.

The menu provides three roaming cameras (Golf saves the preference; Fishing applies it for the island visit), Master / Game sounds / Buttons /
Ambience sliders, a sport guide, return to play, and a separate leave confirmation.
Golf's guide describes the current five-hole course, Aim, drag/release power,
automatic Putt selection, ranking card and buggy. Fishing opens its existing
illustrated guide. A first-visit fishing tutorial waits until settings close.

Escape or Android Back cancels a golf aim first. Otherwise it opens settings;
inside a guide or leave confirmation it returns to settings, and inside settings
it closes the menu. Closing and reopening retains preferences. Repeated Open
calls cannot capture the menu's own input block. Opening and closing clear touch
pads and look delta, cancel pending shots and restore the prior input block.
Golf's modal sorts above the ranking card and independently sorted finish clock.

Settings block only the local player's input. They never pause simulation, a
countdown or a shared room. Fishing's blocked command reader now sends the real
Cancel / ReelStop command, allowing authority to release the cast instead of
leaving the host with stale input. New fishing actions, golf aiming, swings and
buggy requests respect the input block. Settings close when sport context is
lost or shared results take over.

## Leaving

| Context | Confirmed action |
| --- | --- |
| Offline | Return to sport selection and reset the current practice/game. |
| Online host | Return everyone to the existing waiting room and end the game. |
| Golf guest | Leave the room; remaining players can continue. |
| Fishing guest, normal Round | Leave the room; remaining players can continue. |
| Fishing guest, countdown | End the match for everyone. |
| Fishing guest, Cup | Remaining players can continue; the Cup result becomes invalid. |
| Fishing guest, active Crew Catch | End Crew Catch for everyone. |

These are existing room/game rules. The settings confirmation describes them;
it does not add a new protocol or change room authority, capacity or travel.
Leaving is unavailable while travel or teleport owns the transition.

## Validation

Completed on 20 October 2026: **998 core settings assertions** across offline,
local and real cloud rooms; normal Round, Cup and Crew fishing were covered.
The final candidate repeated 110 offline checks, and a further 57 passed using
default script/player paths from a checkout with spaces and an unrelated working
directory. Fishing gameplay passed 2,750 offline and 95 host/client assertions;
the current Golf drag regression passed 36. Rendered menu, guide, inset and
live-match captures were inspected.

The old Golf HUD fixture expects the action artwork removed by the drag-power
change; both the unchanged baseline and candidate fail that same assertion. Its
current replacement checks pass. The fishing fixture now lets the post-match
card release input before injecting a bite in the next synthetic round. The
cloud development launcher waits for the requested island to load before room
creation, avoiding a race with the default Football scene.

The Windows/APK delivery uses a fixed snapshot excluding concurrently developed
PlayerProfiles / SocialPanel features. Its identity source matches the earlier
account delivery's compiled checksum exactly. The original workspace's ongoing
profile changes remain untouched; this task does not validate them.

`Tools/Build/Test-IslandSettings.ps1` exercises the actual HUD, raycast targets,
command reader, saved preferences, responsive layouts, guides, live match clocks
and confirmed exits. It accepts Offline, Local and Cloud transports, host/guest
exit scenarios and Round/Cup/Crew fishing modes. Runtime fixtures are opt-in and
excluded from release builds. Evidence stays in ignored `Builds/IslandSettingsQA/`.

Release measurements and completed verification are recorded in
[BUILD-SIZE.md](BUILD-SIZE.md) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).
Physical Android touch, Back-button ergonomics, appearance and phone performance
require device verification; Windows captures do not establish those results.

The matching validated task deliveries are in `Builds/IslandSettingsQA/Windows/` and
`Builds/IslandSettingsQA/Android/WhatTheFish-release.apk` (94,681,516 bytes).
The complete player was first promoted to `Builds/WindowsFinal/`. Concurrent
profile work subsequently began updating that shared folder and the shared
Android output. The newer concurrent delivery is preserved. Use this task's
retained Windows/APK pair together. This task-specific
copy retains the exact verified settings release. Readiness supports
`-RequireApk -ApkPath Builds/IslandSettingsQA/Android/WhatTheFish-release.apk`;
relative overrides resolve from the script repository, including when launched
from an unrelated working directory. Source, reports and
compiled checksum evidence remain under ignored `Builds/IslandSettingsQA/`.
Reviewed promoted copies and generated backups were archived to
`Legacy/20261020-180624-364-island-settings-promoted-copies/`.
