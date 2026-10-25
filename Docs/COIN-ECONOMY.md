# Account coins and cosmetic ownership

The wallet belongs to the authenticated Unity player ID and is stored in the
existing Cloudflare D1 profile database. The phone cache is display-only: editing
PlayerPrefs cannot change the cloud balance. Linking the existing guest identity
keeps its wallet; recovering that linked identity restores the same wallet.
Different accounts have separate balances, reward history and equipped trails.
An unlinked anonymous identity cannot be recovered after its credentials are lost.

## Reward rules

| Source | Coins |
| --- | ---: |
| Consecutive daily sign-in, days 1–7 | 100, 200, 300, 400, 500, 600, 1,000 |
| Football/basketball winner | 100 per eligible player |
| Football/basketball loss or draw | 30 per eligible player |
| Golf/fishing first, second, third | 200, 100, 50 |

The first online session claims the daily reward automatically. A day resets at
00:00 Malaysia time (UTC+8), using server time. Missing a calendar day restarts
at day 1 without removing coins. The day after day 7 starts a new seven-day
cycle. A complete cycle awards 3,100 coins; this is a rolling streak rather than
a Monday–Sunday calendar week.

Rewards require an online human roster, registered before completion, with every
participant confirming the server ticket using their own account. Practice,
offline play, bots, spectators, cancelled/abandoned matches and golf DNF earn
no coins. Team games require balanced teams; basketball requires at least four
humans. Golf supports two to ten humans; fishing supports two to five. Fishing single rounds pay once;
a three-round cup pays only at the final standings. Crew mode has no reward yet.
Golf orders by strokes then finish time; fishing orders by score, and cups by
cup points then cup score. Exact ties share competition rank (1, 1, 3).

## Wallet and customization

The home menu coin button opens the wallet. Results provide a second entry and
show the confirmed award. The wallet displays the seven-day schedule, reset
countdown, recent transactions, connection state and account-linking shortcut.
All player-facing additions use English, Simplified Chinese and Bahasa Melayu.

The [cosmetic store](COSMETIC-STORE.md) now provides two free choices per category
and permanent 1,000-coin unlocks for the remaining trails, avatars and venue
cosmetics. Purchase receipts, balances and ownership persist per account.
Trail equipment synchronizes online; owned choices remain usable from the last
synchronized account cache offline. There are no real-money payments.

## Implementation and failure handling

- `wallets` stores balance, revision and daily streak; `coin_transactions` is
  the unique reward ledger. A trigger updates the wallet in the same transaction.
- Daily keys use the server calendar day. Match keys use a server-generated
  match ID, preventing duplicate credit across retries, reconnects and devices.
- Match start captures the roster and mode. Each player joins once. Only the
  registered host can submit the result; the service calculates the reward.
  A conflicting second result is rejected. Settlement updates all wallets and
  the final receipt atomically. Cancellation and four-hour expiry pay nothing.
- The host keeps an account-scoped local retry queue. A completed match without
  a confirmed ticket can recover an already accepted start, but cannot register
  an offline result as a new match. A restart cancels unfinished queued games.
- The replicated result state prompts guests to refresh after settlement.
  Failed synchronization shows a retry state; it does not invent a local credit.
- `cosmetic_inventory` and `cosmetic_equipment` track owned and equipped items.
  Account deletion cascades through cloud wallet, ledger and cosmetic records,
  and clears that identity's local wallet, retry queue and trail preferences.
- Network protocol is **88**. All room members must use a compatible build.

This is a player-hosted game. Authentication, immutable receipts and participant
confirmation protect ownership and duplicate awards; they do not independently
prove gameplay, room membership, score accuracy or absence of collusion. A
modified host and cooperating accounts can fabricate a result. Before coins have
commercial value, use trusted match validation and a defined abuse policy. The
ten-second server minimum is only a basic invalid-result guard.

## Initial backend activation — 26 October 2026

Applied `0004_account_deletions.sql` (required by the existing profile source)
and `0005_economy.sql` to the existing `whatthefish-profiles` database. Earlier
migrations were already present. The dashboard-created database still has no
Wrangler migration-history records: do not reapply all migrations blindly.

Deployed Worker version `0c627dee-5252-4a4f-b6aa-69c8bf9c0607`. Real UGS token
smoke checks verified daily idempotency, account isolation, registered team
settlement, repeated completion, saved equipment and subsequent wallet reads.
Synthetic profile records were removed afterward; test credentials were never
saved. Anonymous UGS identities may remain, as in the existing profile smoke test.

Local verification uses `npm test` in `Tools/AccountServer`. The opt-in
`node Tools/AccountServer/economy-smoke.mjs` creates temporary live profiles.
`Tools/Build/Test-Economy.ps1` uses a loopback-only fixture and opt-in development
player probes to exercise real game controllers, network replication and UI.
The fixture and fake-account transport are excluded from the Android release.

Current evidence is under ignored `Builds/CoinQA/`. Physical-phone appearance,
touch/FPS and native linked-account recovery on a second device require device
validation; Windows automation does not establish those results.

## Delivered validation

- **65 backend tests**: day boundaries, seven-day rollover, missed days,
  concurrent/repeated credit, account isolation, saved ownership/equipment,
  wins/losses/draws, ranked modes, ties, DNF, capacity, participant authorization,
  cancellation/expiry, rollback and deletion cascade.
- **Eight live-service checks** with real UGS account tokens, repeated after the
  final deployment. These establish the deployed D1/API behavior, not native
  Google/Huawei device recovery.
- **1,111 client assertions** in `Run-20261026-011756-560`: offline UI plus
  two-player football, four-player basketball, two-player golf and two-player
  fishing cup. Actual controllers and network replication are used; the review
  supplies deterministic final scores. Cup rounds retain one ticket and pay no
  intermediate award.
- **643 final assertions** in `Run-20261026-012328-863`: final UI text/glyph
  visibility, all three languages, menu access, saved selections and four-player
  settlement after the final UI/spectator-status corrections. Home, wallet,
  results and both trail menus were visually reviewed. Captures use a rendered
  Windows 1600×900 viewport.
- **308 lifecycle assertions** in `Run-20261026-013014-347`: account deletion
  clears its six local economy keys while preserving another wallet cache,
  followed by the unchanged three-language wallet, trail and home-menu checks.
- **115 trail regression assertions**: football 50, basketball 65. Evidence:
  `Builds/FootballTrailQA/Run-20261026-012455-105/` and
  `Builds/SportsTrailQA/Offline-20261026-012528-112/`.
- **524/524 Windows** and **376/376 Android** compiled source documents match
  the final source. The complete **342-file** Windows player is in
  `Builds/WindowsFinal/`. Its full build was followed by a validated scripts-only
  refresh; serialized assets/type layouts were unchanged during that refresh.
- AndroidSubmission is **166,942,348 bytes**, +45,008 from the previous release,
  with 833,057,652 bytes headroom. Signature, ZIP CRC, 1,405 audit entries,
  five-scene bundle integrity and development-code exclusion passed. See
  [measured size](BUILD-SIZE.md) and [package audit](APK-SIZE-AUDIT.md).

Reviewed superseded output was archived with the repository archive tool:
`Legacy/20261026-011603-316-coin-system-android/`,
`Legacy/20261026-012617-736-coin-system-windows/`, and
`Legacy/20261026-012709-745-coin-system-qa/`. Current evidence remains in
`Builds/CoinQA/`; Git publishing is left to the user.

The intermediate packages before the final account-cache cleanup are retained in
`Legacy/20261026-013042-218-coin-lifecycle-android/` and
`Legacy/20261026-013138-806-coin-lifecycle-windows/`.
