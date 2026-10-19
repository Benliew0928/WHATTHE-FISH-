# Golf leaderboard visibility and personal preview

Retain the original upper-right leaderboard art and five-column table. Hide the
board and its input targets before Start game (Idle), and after clearing/leaving
the match. Once started, collapsed ranking shows only the local player by stable
PlayerId, preserving their actual rank, name, progress, strokes, status and finish
time. Other players appear only in the existing expanded list.

Collapsed View all footer is removed together with its reserved height. The
existing title button opens the complete list. Its original header, rows, scroll
view, close footer, backdrop and hover behavior remain. Aim still hides ranking.
No gameplay rules, physics, camera, map, assets or network schemas change.

## Validation / delivery

Focused graphical presentation checks: **21 PASS**, no captured runtime exceptions.
Reviewed actual idle, personal row, local eighth-place and expanded-list captures
in ignored `Builds/GolfPersonalRankingQA/`. Only related UI was checked; no map,
gameplay, remote session or phone FPS testing was performed.

Complete WindowsFinal updated; 326 candidate files SHA-256 compared before its
delivery note was updated. Fresh AndroidSubmission at **00:51:28 Malaysia time,
19 October 2026**: **88,550,674 -> 88,552,098 bytes (+1,424)**, with **911,447,902
bytes** headroom under the strict decimal-GB cap. Independent v2 signature passed.
APK SHA-256: **3F5030017C954E89FCC603559CCDC319AF3550024D5484F0BD0A6D378AE5C222**.
Audit copied to `AndroidAudit/`. Superseded complete player and opening APK archive:
`20261019-004926-308-golf-personal-ranking`.

The Windows candidate starts as a complete copy of the prior validated player,
with a code-only script refresh. No authored scene, prefab, resources or
serialized component layouts were changed by this task. The complete candidate
was promoted, including its data, runtime DLLs, supporting folders and launchers.
Build-generated changes to 26 known asset templates were restored from the task
snapshot. Earlier unrelated local Golf work is retained. No Git staging, commit
or push was performed.
