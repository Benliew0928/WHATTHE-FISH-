# Fishing and golf versus bots

From the sport menu, choose **Versus bots**, select **1–4 bot opponents**, and
start. This is an offline free-for-all with one human and at most four bots.
Fishing offers the existing three-minute Round and three-round Cup. Golf uses
the existing five-hole course, stroke ranking, and 30-second finish countdown.
Cooperative Crew Catch remains available for human room play.

Bots choose one of the same three offered round skills during Fishing drafting.
The human retains the skill-choice overlay and normal countdown.

## Integration

`IslandBots` owns the explicit offline roster and removes it on leaving,
changing island, starting travel, joining a room, or returning to exploration.
Bot IDs occupy a separate range from the local human. Existing names, character
appearances, ball ownership, scoreboards and rematches use those identities.
Online room capacity, travel, host authority and human rosters are unchanged.

`FishingBot` responds at short, staggered intervals. It chooses available fish
by expected points and the current wanted bonus, reacts to a bite, renews
directional reel holds, and releases the line to cool tension. Commands enter
through the normal athlete input and `FishingGame.Execute` validation.
Bots ready themselves between Cup rounds; the human controls the next start.

`GolfBot` walks with the normal athlete motor. Its bounded, incremental surface
search uses ground, obstacle and water queries instead of a baked navigation
mesh. It enters the existing legal aim stance and evaluates candidates with
`GolfShotPlan`, including terrain break for putts and estimated run-out for
approaches. Queries are spread over frames. Charging, club contact, flight,
rolling, cup triggers, water recovery, strokes and results remain normal game
logic. No bot writes scores, completes holes directly, or teleports to its ball.
The normal match arrival and aim-stance placement still apply equally to bots.

Minor prop additions can use the existing ground/obstacle collision layers.
Water and course changes should update `GolfCourse` as they already must for
human physics. Re-run the autonomous match suite after changing these.

## Verification

`Tools/Build/Test-IslandBots.ps1` runs the opted-in development player review.
It covers setup, every opponent count, unique IDs, rejected invalid counts,
cleanup, autonomous fishing Cup rounds, golf ball ownership, complete golf
matches against an idle human, and rematches. It writes results, traces and
captures beneath ignored `Builds/IslandBotsQA/`.

Build a complete candidate with the Unity execute method `IslandBotsBuild.Build`.
`IslandBotsBuild.Scripts` refreshes that candidate after code-only iteration using
the repository's packaged-content helper. The test runner resolves its default
paths from the script location and accepts an explicit `-PlayerPath`.

Physical Android performance and touch validation require a device. Desktop
autonomous tests do not establish phone frame rates.

## Validated delivery — 20 October 2026

The complete Windows candidate passed **71 fishing** and **46 golf** assertions
in a path containing spaces, launched from an unrelated working directory using
the runner's default paths. All four anglers earned catches in every Cup round
and in a subsequent standalone Round. Two golfers completed all five holes in
the final run; the normal 30-second finish timer classified the others as DNF.
The winning bot used 13 strokes. Setup, play, awards and leaderboard captures
were inspected. Existing basketball and football suites also passed **155**
and **94** assertions on the preceding candidate with the same bot gameplay.

Evidence: `Builds/IslandBotsQA/PortableValidation/Run-20261020-221111/`,
`Builds/ProgrammedBotsQA/Run-20261020-220218/`, and the source and promotion
manifests in `Builds/IslandBotsQA/`. The complete player is delivered in
`Builds/WindowsFinal/`. It includes concurrent venue, controls and profile work.
All 404 compiled C# documents matched when the final candidate was copied.
Subsequent edits to `PlayerProfiles.cs` and `SocialPanel.cs` belong to concurrent
profile work and are outside this Windows snapshot; see
`Builds/IslandBotsQA/final-compiled-source.json`. No later bot changes are omitted.

The official AndroidSubmission build completed at **22:15 Malaysia time**.
The actual APK is **106,341,732 bytes**, compared with the opening
**94,765,380-byte** baseline; remaining headroom is **893,658,268 bytes**.
The build gate, ZIP integrity and independent APK v2 signature checks passed.
The combined size increase primarily comes from concurrent portrait artwork;
these bots add no art assets. The exact tested release and its reports are
retained in `Builds/IslandBotsQA/Android/` and `SizeAuditFinal/`.
See [BUILD-SIZE.md](BUILD-SIZE.md) for the measured release record.

The superseded complete player is archived at
`Legacy/20261020-221821-053-island-bots-windows/`. Duplicate test players and the
semantically identical generated golf prefab backup are archived at
`Legacy/20261020-221942-262-island-bots-validation-copies/`. Each archive has
recovery paths and SHA-256 hashes. The earlier `221706-425-island-bots-windows`
attempt only wrote its inventory because the running preview held the folder
open; it moved no files. Current captures, logs, manifests and release files
remain under `Builds/IslandBotsQA/`.
