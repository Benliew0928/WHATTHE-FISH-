# Golf original leaderboard restored - 19 October 2026

Follow-up: [personal preview behavior](GOLF-PERSONAL-RANKING.md) retains this
restored art, but hides ranking before Start game and shows only self while closed.

The user reverted all simplified leaderboard designs. The original upper-right
preview is restored, including title, five-column headings, the first three
players plus the local player when outside that group, and the View all footer.
Clicking the title/footer opens the unchanged full scrollable leaderboard.
The left-corner self-only card, rank-only chip, generated portrait runtime loads
and associated importer/generator changes have been removed.

The independent finish countdown and its smooth animation remain. Numerals stay
50 x 50 with a proportional s. The closer Aim arrow/card spacing and the
ball-centred thick-to-thin trail remain, as these are separate requested fixes.

Unused portrait assets/metas, generator/metas, portrait provenance and the
undelivered self-only design note were reference-checked and archived in batch
`20261019-002430-539-golf-ranking-design-reverted`. Prior test evidence stays under
ignored Builds. No history reset, staging, commit or push was used.

## Validation and delivery

Focused restored-preview/full-list, Aim hide/restore, 50-unit countdown and trail
checks: **39 PASS**, no captured runtime exceptions. Actual single-player preview,
synthetic eight-player preview/full-list and countdown screenshots inspected.

Complete WindowsFinal updated; 326 candidate files SHA-256 compared before its
delivery note was amended. Full build with a final developer-check script refresh.
Fresh AndroidSubmission at **00:39:12 Malaysia time, 19 October 2026**: APK
**88,542,330 -> 88,550,674 bytes (+8,344)**; headroom **911,449,326 bytes**.
Independent v2 signature passed. SHA-256:
**01D4B74BAA73CC9DBB323A0721E50C314D4B34892297549AD9DEE05FBB65E878**.

Superseded player/opening APK archive batch:
`20261019-003722-859-golf-original-ranking-player`.
Build-generated changes to 26 known asset templates were restored from their
clean task snapshot, preserving earlier local Golf asset changes.
Evidence: ignored `Builds/GolfRankingRollbackQA/`. Phone FPS/WAN testing is outside
these focused presentation checks. Two preliminary review-fixture issues (reserved
offline player IDs and legitimately culled offscreen list text) were corrected;
they did not require further runtime UI changes.
