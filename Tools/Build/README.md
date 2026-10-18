# Build and verification tools

Run examples from the repository root. Scripts resolve the project from their own location; alternate Unity installations use `-UnityEditorPath` or the editor-root argument documented in [setup](../../Docs/SETUP.md). Unity 6000.3.20f1 is required. Build output and raw evidence go under ignored `Builds/`.

| Task | Entry point |
| --- | --- |
| Complete Windows player | `./Tools/Build/Build.ps1 -Target Windows` |
| Travel guards, match-start spectator arrivals and Fishing piers | Unity method `TravelMatchBuild.Build`, then `./Tools/Build/Test-TravelMatches.ps1`; three real local clients; [teammate testing](../../Docs/TEAMMATE-TESTING.md) |
| Reject an older multiplayer build in both host/guest directions | `./Tools/Build/Test-BuildCompatibility.ps1 -PreviousPlayerPath <older-player-executable> -PlayerPath Builds/WindowsFinal/WhatTheFish.exe`; two local connections, protocol rejection and current-client guidance |
| Football team celebration, solo MVP and looping results | Unity method `FootballCelebrationBuild.BuildReview`, then `./Tools/Build/Test-FootballCelebration.ps1 -CaptureFrames`; [direction and checks](../../Docs/FOOTBALL-CELEBRATION.md) |
| Football giant-screen broadcast, fireworks and spectator timing | Unity method `FootballStadiumShowBuild.Windows`, then `./Tools/Build/Test-FootballStadiumShow.ps1` (host, player and spectator); [stadium broadcast](../../Docs/FOOTBALL-CELEBRATION.md#stadium-broadcast) |
| Football live stadium footage, 30/70 scoreboard and full-screen goal replay | Unity method `FootballStadiumShowBuild.WindowsBroadcast`, then `./Tools/Build/Test-FootballBroadcast.ps1 -CaptureFrames`; run the stadium-show and replay suites with this candidate's `-PlayerPath` |
| Football daylight flames, electric arcs, lasers and sky fireworks | Unity method `FootballStadiumShowBuild.WindowsEffects`, then `./Tools/Build/Test-FootballStadiumEffects.ps1 -CaptureFrames`; run ceremony, stadium-show and teardown reviews with this candidate's `-PlayerPath` |
| Basketball team celebration, shooting MVP pose and courtside results | Unity method `BasketballCelebrationBuild.BuildReview`, then `./Tools/Build/Test-BasketballCelebration.ps1 -CaptureFrames`; [direction and checks](../../Docs/BASKETBALL-CELEBRATION.md) |
| Basketball swipe-selected automatic blocks | Unity method `BasketballControlsBuild.BuildSwipeGuardReview`, then `Test-BasketballControls.ps1 -PlayerPath Builds/BasketballBlockSwipeQA/Player/WhatTheFish.exe` and `-Local` |
| Basketball continuous scoring, rotating inbounds and individual movement hints | `./Tools/Build/Test-BasketballRestart.ps1 -Build` then `-Local` (candidate under `Builds/BasketballRestartQA/Player/`; `-PlayerPath` can select a delivered player) |
| Startup artwork, real loading progress, retry, placeholders and guest entry | `./Tools/Build/Test-StartupWelcome.ps1 -PlayerPath Builds/WindowsFinal/WhatTheFish.exe` (development player; cold/warm cache, three landscape sizes and simulated cutouts) |
| All-island character lighting and active renderer regression | `./Tools/Build/Test-CharacterLighting.ps1 -PlayerPath Builds/WindowsFinal/WhatTheFish.exe` (development player; saves matching character captures) |
| Self-contained Android delivery with APK budget gate | `./Tools/Build/Build.ps1 -Target AndroidSubmission` |
| Optional development APK | `./Tools/Build/Build.ps1 -Target Android` |
| Full-capacity host succession, including spectator fallback | `./Tools/Build/Test-HostMigration.ps1 -Sport Football -Online` (repeat for Basketball, Golf and Fishing; omit `-Online` for local transport) |
| Lobby snapshot ordering regression against the compiled SDK | `./Tools/Build/Test-LobbySnapshot.ps1` (optional `-PlayerPath` selects a candidate) |
| Retry unchanged metadata after a failed network join | `./Tools/Build/Test-NetworkMetadataRetry.ps1` (deterministic failure; no live service required) |
| Regenerate the integrated scene | `./Tools/Build/Build.ps1 -Target Scene` |
| Fishing results, Crew checklist, target feedback and sound lifecycle | `./Tools/Build/Test-FishingPolish.ps1` (requires a development Windows player with graphics) |
| Approved Golf/Fishing audio candidate player | `./Tools/Build/Build-GolfFishingAudioReview.ps1` |
| Approved resources, button deduplication, contacts, fishing skills and sound lifecycle | `./Tools/Build/Test-GolfFishingAudio.ps1 -Sport Golf` and `-Sport Fishing` (optional `-PlayerPath`) |
| Read-only commit candidate checks | `./Tools/Build/Check-TaskReady.ps1` (add `-RequireApk` for Android delivery) |
| Exact staged text portability, after the user stages | `./Tools/Build/Check-PortablePaths.ps1 -Staged` |
| Archive an explicit reviewed path list with recovery hashes | `Archive-LocalArtifacts.ps1` — [workflow](../../Docs/REPOSITORY-HYGIENE.md#archive-and-restore) |

`Install-Android.ps1` and `Repair-CMake.ps1` support the editor's Android toolchain. `Prepare-CoveFonts.py` and `Prepare-CartLabels.py` regenerate delivery assets; `analyze_packed_apk.py` inspects APK packing. Keep these with the source project.

`Build-GolfFishingAudioReview.ps1 -ScriptsOnly` is an iteration option after its
complete player build. It retains the matching packaged scene payload and
rebuilds scripts through Unity. Use the normal full command after asset changes.

The `Test-*.ps1` scripts are reusable feature and networking checks. Use a matching player and the script's parameters; local tests exercise host/guest authority without Relay, while cloud tests use real service quota. The passing, trajectory and stadium-camera checks remain available for the latest gameplay. See [verification](../../Docs/VERIFICATION.md) for feature coverage and device limits.

The [sports home menu](../../Docs/SPORTS-MENU.md) uses Unity build method `WhatTheFish.Editor.SportyMenuBuild.Windows` for a complete candidate in `Builds/SportyMenuQA/Player`. Its `Scripts` method is only for C# iteration after that full build and restores the matching packaged content. Verify with `Test-StorybookMenu.ps1`, `Test-StartupWelcome.ps1`, `Test-PlayerIdentity.ps1` and `Test-ProgrammedBots.ps1 -OfflineOnly`, passing the candidate player path. Promote the entire validated player, then use the normal AndroidSubmission build for Android delivery.

`Review-*.ps1`, `Benchmark-RefinedIslands.ps1`, the gallery builders and walkthrough encoders reproduce curated visual evidence. `check_basketball_evidence.py` checks room movement reports. These tools remain useful even though they are not part of the runtime. Art generators and audits live in [Tools/Blender](../Blender), with authoring instructions alongside the masters in `ArtSource/`.

Completed coastal/island migration inventory, special archive and historical report-writing scripts were retired on 7 October 2026. Their hash-verified local recovery batch is recorded in [repository hygiene](../../Docs/REPOSITORY-HYGIENE.md#cleanup-completed-on-7-october-2026). The general archive tool replaces those one-time operations. Current sources, package locks, tests, fixtures, selected references and Unity metadata remain versioned.
