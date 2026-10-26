# Coin display and daily rewards — 26 October 2026

The home header now shows a read-only gold fish coin and account balance. A
separate Rewards button uses a gift icon and a plain red circle when a daily
reward is unclaimed. The dot contains no number and disappears after server
confirmation. The [coin master and generation prompt](../ArtSource/CoinRewards/README.md)
are retained; only the 256px derivative is included in the game.

Rewards opens a seven-day calendar with day labels and the existing coin values
(100, 200, 300, 400, 500, 600, 1,000). The claimable day is yellow, all other
cards are white. Claimed cards replace the coin illustration with a tick. The
calendar removes explanatory subtitles, received/next labels, history and the
old wallet shortcuts. Connection problems retain the last balance and provide
a read-only retry.

Daily claims are manual. Startup, background refresh, reconnect, opening the
calendar and retrying sync perform GET wallet requests. Only clicking the
current, confirmed, unclaimed day submits the daily POST. The client guards
against repeated taps and stale day snapshots; the existing server still owns
Malaysia-midnight resets, streaks, idempotent credit and account persistence.
The seven-day cycle and missed-day behavior are unchanged. Match-result coin
text is also read-only. No backend deployment or migration is needed.

New labels are in the shared English, Simplified Chinese and Bahasa Melayu
catalog. The coin, gift and ticks contain no embedded wording. Existing button
geometry, font families, colors and icon treatment are shared across languages.

Validation evidence is retained under ignored `Builds/CoinRewardsQA/`.
The first candidate exposed a shared importer treating the new coin as a plain
texture; the importer now recognizes it as a sprite and the review build rejects
a missing or incorrectly sized coin. The rejected candidate is recoverable in
`Legacy/20261026-234559-245-coin-rewards-candidate/`; its screenshots and checks
remain in `Run-1`. The review also checks actual fitted glyph bounds; raw preferred height is
not a valid clipping measurement when best-fit text is active.


## Verified delivery

- **7,236 calendar/UI assertions** across 1600x900, 1560x720, 1200x900 and
  1920x1080, all three languages. Tests exercise pointer clicks, no auto-claim,
  all seven rewards, rapid repeats, reopen persistence, rollover, missed days,
  failure/retry, red-dot visibility, glyph bounds and large balances. Screenshots
  were visually reviewed in English, Chinese and Malay, including wide framing.
- **760 wallet/store integration assertions**, using the existing loopback
  server and real client controllers across all four sports, plus **23 backend
  economy tests**. No live account or production service was modified.
- **3,664** localization entries, five fonts and **16** artwork checks pass.
  PNG inspection confirms real alpha, no metadata and no external dependencies.
  The derivative generator produces identical bytes from a relocated project
  path containing spaces while launched from an unrelated working directory.
- All **534 Windows / 383 Android** compiled source documents match. The full
  Windows resource build was followed by review-harness-only code refreshes;
  serialized layouts stayed unchanged. The complete **342-file** player is in
  `Builds/WindowsFinal/`, with every copied file hash verified and its build note
  updated. Existing trailer launchers are retained; trailer flows were not rerun.
- AndroidSubmission: **168,154,644 -> 168,186,270 bytes (+31,626)**, with
  **831,813,730 bytes** headroom. ZIP CRC, **1,410** exact size-audit entries,
  five-scene bundle integrity, v2 signature, non-debuggable package and native
  dependencies pass. The new compressed coin payload is **46,083 bytes**; no
  existing art quality reduction or unexpected large growth was found.
  See [BUILD-SIZE.md](BUILD-SIZE.md) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).

Archives created by this task:

- `Legacy/20261026-234559-245-coin-rewards-candidate/` — rejected first player.
- `Legacy/20261026-235343-587-coin-rewards-android/` — prior APK and size audit.
- `Legacy/20261026-235612-965-coin-rewards-cleanup/` — completed portability fixture
  and redundant reference copy; current proof stays under Builds.
- `Legacy/20261026-235826-271-coin-rewards-windows/` — superseded complete player.

A format-aware comparison verified the generated golf-ball prefab retains all
14 objects/components and external references; its original source representation
was restored. Other pre-existing working changes were preserved. No Git staging,
commit or push was performed. Physical phone appearance, touch and performance
remain unverified. The APK retains the project's Android Debug signing certificate.
