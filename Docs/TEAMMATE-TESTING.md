# Testing the same multiplayer build

`Builds/` is ignored. Committing and pushing source does **not** upload
`Builds/WindowsFinal/`, and pulling source does not update an existing player.
An old executable/data folder continues to run its old code.

## Share the tested player

Share the complete current `Builds/WindowsFinal/` folder. Both teammates should
copy the same delivery into a new folder, keep all files and subfolders
together, and run `WhatTheFish.exe`. If packaging it as a ZIP, include the
entire folder and compare its SHA-256 after transfer. Do not replace only the
executable or combine it with an older player folder. `LATEST-BUILD.txt`
identifies the source and verification limits. The older
`Builds/TravelMatchQA/WhatTheFish-Windows-network68.zip` is historical and does
not include the current [Fishing update](FISHING-ABILITIES.md).

Use the main menu's online room flow: one person creates a room and shares its
code; the other chooses Join and enters that code. Both need internet access to
the project's Unity online services. The `Explore-*.cmd` launchers are offline
practice shortcuts. Local automated-test flags use loopback and cannot connect
two separate computers.

The waiting room shows **Network build 88**. The current combined game uses
protocol 88; use the same delivered player on every computer. Earlier protocols are rejected during connection
before entering gameplay. The generic join-failure message also asks clients to
check their code, connection and matching build; a missing network response
cannot uniquely identify a version mismatch. The protocol is a compatibility
number, not an exact hash of every source edit: two privately modified builds
with the same number are not guaranteed compatible. Sharing the same tested player
is the recommended way to eliminate that ambiguity.

## Build from the repository

After the author commits and pushes, the teammate should pull the same commit,
run `git lfs install` and `git lfs pull`, and use Unity **6000.3.20f1** with the
Windows build support installed. Open the clone's `Game` project and allow the
first import to finish. Close Unity before using the batch builder:

```powershell
./Tools/Build/Build.ps1 -Target Windows
```

The builder locates the project from the script location. Its default Unity
installation is resolved through the system Program Files location; an explicit
`-UnityEditorPath` can select another local installation. Run the resulting
`Builds/WindowsFinal/WhatTheFish.exe`, keeping the entire output folder together.
Do not assume an independently rebuilt player is the already-tested ZIP.

## Manual checks with your teammate

1. In the waiting room, confirm both see network build 88. Enter as players,
   ready up, and travel together. For Basketball, fill to 2v2 with bots.
2. Finish a round, then use Next adventure to teleport everyone to a Skyrail
   station. Start another match while a participant remains near the cabin.
3. For Football, Basketball, Golf and Fishing, verify that a running match
   disables Skyrail destinations. A rejected travel request must preserve the
   round and its progress. Fishing countdown also blocks travel.
4. For Fishing, ready both players while away from the lagoon. Starting places
   them on different piers before the countdown is presented. Check that each
   can aim/cast after the countdown.
5. Return to the waiting room and switch one person to a spectator slot. Travel
   to a station, then start a game. The spectator should move to the viewing
   area and retain their spectator badge, controls and number. With only two
   people, test Golf or add bots for Football/Basketball. A Fishing room needs
   two playing humans, so its spectator test requires a third connection.
6. Try hosting from a spectator slot. Its Start match control should show the
   roster/readiness requirement and disable during a match or journey.
7. Outside a match, leave one person away from the station. The departure card
   should show attendance and the missing person. Gather everyone, ride, and
   verify both players and spectators arrive together.
8. After a round, teleport to another island's Game area. Spectators should
   arrive in that island's viewing area. No new room or spectator role change
   should occur.
9. In Football, blue defends the blue goal and red defends the red goal. Check
   the frames, apron, BLUE GOAL/RED GOAL boards and matching minimap ends from
   both playing directions. Shoot at the other team's colour.
10. Compare guest start/stop/turn response with the previous release, then
    test charged shots, passes and tackles. The local player's movement uses
    the newest host pose, but actions still need host approval. Record the
    host/client devices and connection when reporting remaining delay. See
    [the latency investigation](FOOTBALL-DIRECTION-AND-LATENCY.md) for controlled
    measurements and the separate loss-during-admission limitation.

Automated regression entry point:
`Tools/Build/Test-TravelMatches.ps1 -PlayerPath Builds/WindowsFinal/WhatTheFish.exe`.
It runs three local clients. Existing Skyrail and spectator suites remain under
`Tools/Build/Test-SkySail.ps1` and `Tools/Build/Test-Spectators.ps1`.
These checks do not replace testing two physical computers, WAN latency or a
physical Android device.

This release preserves the existing source snapshot's unrelated gameplay work.
The previously recorded cart-suite camera/pose/steering failures are outside this
direction/response change; see [the existing baseline](GOLF-FISHING-AUDIO.md).

## Publish source after reviewing the changes

Builds, logs, captures and archives remain ignored; active source, Unity assets,
metadata and documentation remain publishable. Review the whole working tree,
including changes made before this task. The assistant leaves staging and
publishing to you:

```powershell
./Tools/Build/Check-TaskReady.ps1 -RequireApk
git diff --stat
git status --short
git add .
./Tools/Build/Check-PortablePaths.ps1 -Staged
git commit -m "Fix shared travel and match-start arrivals"
git push
```

Only commit after the staged portability check passes. Sending the Windows ZIP
is a separate step from publishing source; neither action uploads the other.
