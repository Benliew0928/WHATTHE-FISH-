# Account cosmetic store

Each category starts with its first two choices unlocked. Every remaining item
costs **1,000 coins**, once per account. The shared catalog contains **58 choices**:
30 starters and 28 paid unlocks. There are no real-money payments.

| Category | Choices | Free |
| --- | ---: | ---: |
| Football trails | 5 | Electric Bolt, Ocean Blue |
| Basketball trails | 6 | Solar Drive, Court Lightning |
| Profile avatars | 10 | Characters 01 and 02 |
| Venue palette, separately for each of four sports | 4 each | Mint, Rose |
| Basketball centre-court logo | 4 | Rally crest, Star |
| Football board message | 3 | Sunny days, Good energy |
| Golf club and fishing rod finishes, separate categories | 3 each | Original, Sea glass |
| Football/golf/fishing sign style | 2 each | Both |
| Football decorative flags | 2 | On and Off |

Player names, venue names, uploaded profile photos, controls and gameplay settings
remain freely editable. Room-assigned player numbers, hairstyles and uniforms
remain part of the existing distinct-player identity system, not store choices.

Locked buttons retain their layout beneath a dark grey overlay and a geometric
lock icon. Selecting one opens a confirmation with its name, price and current
balance. Buying requires a confirmed online wallet. Successful purchase selects
the item; profile and venue changes still use their existing Save action.
All new wording is supplied in English, Simplified Chinese and Bahasa Melayu.

## Persistence and transactions

The canonical catalog is
[`EconomyCosmetics.json`](../Game/Assets/_Game/Resources/EconomyCosmetics.json),
loaded by both Unity and the account Worker. Stable item IDs identify ownership.
`POST /v1/economy/purchase` takes only an item ID. The authenticated account and
catalog determine the owner and price; supplied prices, amounts and owner IDs
cannot change the charge.

An insert condition checks sufficient balance and existing ownership. The unique
`purchase:<item-id>` ledger key prevents repeated charges. Wallet debit and the
`cosmetic_purchase` inventory trigger execute in the same SQL statement, so a
failure rolls back both. Simultaneous purchases cannot overdraw the balance.
History displays debits with a minus sign. Account deletion cascades through
inventory and purchase receipts.

Ownership survives account recovery on another device. Purchased effects can be
used offline from that account's last synchronized version-2 cache. Purchases
cannot be made offline. Trail equipment is synchronized through the service;
venue selections are saved locally per account and sport, while their unlocks
are shared across devices. Server profile saves reject unowned avatar choices.
Venue editing remains host-controlled through the existing room appearance model.

## Upgrade

The previous coin release automatically granted all eleven trails without any
purchase. `Economy.ensure` removes those non-starter grants unless a purchase
receipt exists. Unowned local trail and venue choices fall back to the first
starter. Old wallet caches cannot grant ownership. Migration
[`0006_cosmetic_store.sql`](../Tools/AccountServer/migrations/0006_cosmetic_store.sql)
adds the atomic grant trigger and resets pre-store non-starter profile avatars.
Balances and earned reward history are retained.

Apply only migrations absent from the target database. The live database uses
manual migrations and has no Wrangler migration-history records. Migration 0006
was applied after a D1 recovery bookmark, then Worker version
`13c03113-5f15-43ea-8df5-01f2774eda13` was deployed. Recovery evidence and current
validation outputs live under ignored `Builds/StoreQA/`.

## Verification

`npm test` in `Tools/AccountServer` checks starter counts, prices, insufficient
funds, duplicate and competing purchases, rollback, account isolation, migration
of old grants, persistence, and server avatar enforcement alongside reward tests.
The opt-in live smoke test earns coins through ordinary match routes before
buying with synthetic accounts, then removes its synthetic profile records.

`Tools/Build/Test-Economy.ps1` exercises the development player against a loopback
fixture. Its offline case covers locked selectors, all four venue pages, both
trail menus, avatars, purchase confirmations, account switching and every paid
catalog item. The fixture's funding route is local-only and is absent from the
deployed Worker. Production Android excludes development review code.

The existing player-hosted match trust limitation remains: the service enforces
the economy transaction but cannot independently prove that a host-reported
match was played honestly. Physical phone testing is separate from Windows QA.

Final client validation passed 2,202 assertions across the offline store and
Football, Basketball, Golf and Fishing multiplayer. Store-specific coverage
accounts for 900 assertions, with all three languages visually reviewed.
The AndroidSubmission APK is 168,106,738 bytes; compiled source verification
matches 378 Android and 527 Windows documents. See [build size](BUILD-SIZE.md)
for measurements, build limits and retained evidence.

The complete 342-file tested Windows player is delivered in `Builds/WindowsFinal/`.
All 115 football/basketball trail regression assertions pass on its final scripts.
Superseded players, APK/audit and diagnostic captures were archived through the
repository archive tool; archive manifests are recorded under `Builds/StoreQA/`.
