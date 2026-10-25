# Golf results presentation

Golf awards stay on screen after their animation. They do not automatically open the ranking. The bottom-right **Show results** button opens the existing live score table as a centered modal over the awards. Its close control or shaded backdrop returns to the awards, and the results can be reopened. **Next adventure** leaves the awards through the existing shared post-match flow.

The compact score table remains available during play; after the match, it stays hidden unless explicitly expanded. Expanded rankings use a centered pivot and anchor, including whole-card hover scaling. Long tables retain their existing scroll viewport.

Implementation: `FishingCeremony.cs` applies the manual-only behavior to Golf; Fishing retains its timed ceremony. `GolfLeaderboardUI.cs` temporarily parents the existing table and backdrop into the awards safe-area canvas, restoring their original parent on close or ceremony cancellation. `GolfAwards.cs` no longer supplies an automatic results callback. Scoring, finish conditions and ranking order are unchanged.

Validation evidence is stored locally under `Builds/GolfResultsQA/` and the dated Golf run under `Builds/ActivityCinematicsQA/`.

## Validation — 25 October 2026

- 96 Golf runtime assertions at 1600×900, including no automatic ranking after 18 seconds, visible ranking artwork, pointer-accessible close control, centering, close/reopen, outside-click dismissal, and next-adventure exit. The rendered screenshot was reviewed.
- 1,107 Fishing regression assertions at 960×540 passed.
- All 518 Windows compiled source documents matched the shared workspace. The complete 335-file candidate was hash-verified after delivery to `Builds/WindowsFinal/`.
- Earlier intermediate captures show a diagnosed hidden CanvasGroup; final Golf evidence is `Builds/ActivityCinematicsQA/Golf-20261025-195951-678/`. Fishing evidence is `Builds/ActivityCinematicsQA/Fishing-20261025-200241-296/`.
- Superseded Windows player: `Legacy/20261025-200220-613-golf-manual-results-windows/`. Previous APK and audit: `Legacy/20261025-195003-319-golf-manual-results-android/`.
- Physical phone rendering, touch and frame rate remain unverified.
