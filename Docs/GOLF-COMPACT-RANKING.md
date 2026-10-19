# Golf compact ranking and visual refinements

> Historical release record. The leaderboard design was reverted at the user's request on 19 October 2026; see [current rollback](GOLF-RANKING-ROLLBACK.md). The separate countdown, arrow and trail fixes remain.

## Scope

- Default ranking is a 310 x 68 design-unit card at the existing upper-right anchor, showing the local avatar, ranking, player count and total strokes. Before players join it reads Leaderboard.
- Clicking the card opens the existing expanded five-column leaderboard with its original title, artwork, rows, status badges, hover feedback and scrolling. Footer or backdrop closes it. Aim hides the card and board; the final countdown remains independent.
- Countdown numeral containers are 50 x 50 at the normal corner scale. The lowercase s is 32.042 x 35.211, with proportionally tighter glyph spacing. Existing smooth transition timing is retained.
- Aim hole arrow joins the top of the hole card with a 4-unit overlap of their transparent rectangles (previously 4 units of separation), bringing the visible artwork closer without resizing it.
- The ball-centred trail has a linear width envelope from 1 at its newest point to 0 at its oldest point. No rounded cap; lifetime, colour and emission position are retained.

## Validation

Focused graphical runtime presentation checks: **38 PASS**, with no captured runtime exceptions. Screenshots and logs are in ignored `Builds/GolfCompactQA/`: rank-compact, rank-expanded, aim-tight-arrow, countdown-50 and trail-taper. These were visually inspected.
Complete Windows player built at 23:29, promoted to `Builds/WindowsFinal/`; 326 candidate files SHA-256 compared before the delivery note was amended. AndroidSubmission at 23:33:48: APK **88,550,326 -> 88,542,330 bytes (-7,996)**, headroom **911,457,670 bytes**. Independent v2 signature passed. APK SHA-256: **358B883D2286330AA147193AEF336E9619F2FEE9D15E4A51F26B51263BCFAAD8**. Audit copied into AndroidAudit/. No physical phone FPS, WAN or unrelated gameplay checks were performed.

Superseded complete player and opening APK archived in batch `20261018-233137-841-golf-compact-ranking`. Build-generated changes to 26 asset templates were restored byte-for-byte from the task-opening snapshot; prior local asset changes were retained. Historical full HUD review assumptions about the former four-row preview are superseded by this focused compact-card fixture.

Gameplay rules, map, camera controls and network schemas were not changed by this task. Existing local Golf work is retained. No Git publishing actions are part of this task.
