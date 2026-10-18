# Basketball final-buzzer ceremony

Basketball now follows the football honours flow with its own court staging, camera path, performances and orange/navy scoreboard. The existing MVP ranking remains points plus assists, then points, completed passes and stable player ID. Player and team statistics remain authoritative.

- A short final-buzzer dissolve introduces the winning squad at centre court, with the basket and arena behind them. The low camera arcs across staggered cheers, shooting gestures and applause.
- From 6.8 to 9.2 seconds, the camera moves between the teammates into the MVP close-up. The player acknowledges their jersey, performs a shooting follow-through and salutes the court.
- From 11.6 to 13.8 seconds, the same animated player moves into the left-side composition as the results panel appears. Points, assists, completed passes, interceptions and the full box score remain available.
- The MVP performance loops smoothly while viewing results. “Show results” and Escape/Android Back skip through a covered camera transition. Once settled, Escape/Back or “Next adventure” hands control to the existing post-match flow.

Draws celebrate both squads. If the MVP belongs to the losing side, the winners receive the team celebration before a covered transition to the individual award. Retained records can reconstruct a departed player's visual identity. The ceremony copies renderable bones, meshes and materials only; it creates no gameplay actors, colliders, network objects or animation controllers. Dismissal, a new match, leaving basketball or disabling the presentation releases the camera, renderer visibility and input block.

## Source

- [Camera and ceremony director](../Game/Assets/_Game/Shared/UI/BasketballMVPCelebration.cs)
- [Scorecard and statistics](../Game/Assets/_Game/Shared/UI/BasketballMVP.cs)
- [Editable basketball performances](../Game/Assets/_Game/Sports/Basketball/BasketballCelebrationPoses.cs)
- [Shared render-only athlete adapter](../Game/Assets/_Game/Sports/Football/FootballCelebrationActor.cs)

The basketball performances are authored in code using the existing continuous pose sampler and shared skeleton. No new external art dependencies are introduced. The football portrait assets are retained pending a separate reference audit.

## Validation

Build a complete Windows review player through Unity's `BasketballCelebrationBuild.BuildReview` entry point, then run from the repository:

```powershell
Tools/Build/Test-BasketballCelebration.ps1 -CaptureFrames
Tools/Build/Test-BasketballHonours.ps1 -PlayerPath Builds/BasketballCelebrationQA/Player/WhatTheFish.exe
Tools/Build/Build.ps1 -Target AndroidSubmission
Tools/Build/Check-TaskReady.ps1 -RequireApk
```

The ceremony review captures team, solo, result and box-score views, checks the pose loop and limb lengths/reach, verifies unchanged live actors/statistics, and covers draws, opposing/departed MVPs, skipping, dismissal and interruption. It captures compact/wide phone and tablet aspect ratios. The honours suite exercises actual passing, interceptions, assists, final baskets and replicated host/guest results.

Current logs, motion traces and captures belong under ignored `Builds/BasketballCelebrationQA/` and `Builds/BasketballHonoursQA/`. The complete tested delivery goes to `Builds/WindowsFinal/`. Windows rendering and local room tests do not establish physical Android appearance, touch performance, frame rate or WAN behavior.
