# Bot match start and football pass presentation - 16 October 2026

## Delivered behavior

Football and basketball Play vs bots now use Choose team -> Blue or Red -> Start match. There is no automatic ten-second start, team capacity counter, or forced initial team. Start is disabled until a side is chosen. Bots fill balanced teams while preserving that choice. Football opens its normal centre kickoff immediately; basketball enters play immediately. Room team selection retains its countdown and capacity rules.

Football passing uses a continuous world-space ribbon made with the football shooting material, with compact arrowheads and metre-spaced animated chevrons inspired by basketball. The route endpoint and receiver halo ease into and out of a lock. There is no floating LOCKED badge. Cancelling, losing possession, changing context or replay hides the guide. Existing physical ground passes, opponent interceptions and authority rules remain in effect.

## Verification

- Solo bots: 155 basketball and 94 football checks, including 2v2-5v5, both team choices, waiting past the former deadline, disabled Start before selection, immediate explicit start, balanced teams, bot actions, complete matches and return to exploration. Evidence: `Builds/ProgrammedBotsQA/Run-20261016-153047/`.
- Final player room bots: basketball host/guest 12/8 checks; football host/guest 11/8 checks. Evidence: `Builds/ProgrammedBotsQA/Run-20261016-153737/`.
- Final player football playmaker: offline 401, host 104 and guest 8 checks. Covers route locking/unlocking at 20, 60 and 120 FPS caps, all three camera views, absence of the floating badge, cancellation, possession loss, physical passing, interceptions, assists, swipes and results. Evidence and captures: `Builds/FootballPlaymakerQA/Run-20261016-153804/`.
- Compiled Windows symbols match all 292 available game source documents. The solo run used the same production gameplay/presentation sources; subsequent changes only corrected development fixture setup. Final player tests wait for actual arrivals and place unused randomized teammates outside the free-pass test lane.
- Complete 325-file Windows player promoted to `Builds/WindowsFinal/`, with every file hash checked before updating its delivery stamp. Current source checks and promotion inventory: `Builds/BotStartPassQA/`.
- Fresh `Tools/Build/Build.ps1 -Target AndroidSubmission -JobWorkerCount 4` succeeded. APK: `Builds/Android/WhatTheFish-release.apk`. Build log: `Builds/build-AndroidSubmission.log`.
- The build regenerated golf prefab local object IDs. Format-aware YAML comparison proved all 14 objects equivalent; incidental ID churn was restored to keep the source change focused. Evidence retained under `Builds/BotStartPassQA/`.

FPS caps are Windows regression settings, not measured phone performance. No physical Android-device test was performed. The complete Windows delivery retains the current repository's prior room, travel and host-recovery implementation; those unrelated systems were not exhaustively requalified by this task.

## Local recovery archives

All archives are ignored local recovery copies, with manifests and hashes:

- `Legacy/20261016-152658-828-bot-start-pass-candidate/`: first superseded task player.
- `Legacy/20261016-153356-609-bot-start-pass-fixtures/`: second superseded task player.
- `Legacy/20261016-154104-791-bot-start-pass-delivery/`: prior complete Windows and Android deliveries.

No files were staged, committed or pushed.
