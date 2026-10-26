# Player profiles, friends and service capacity

Implementation record, 20 October 2026. See [sign-in setup](PLAYER-LOGIN.md) for Huawei/Google credentials and provider configuration. The Android app has not been published.

## What players can do

Guests and signed-in players use the same profile features. Your profile lets them edit a 1–16-character name, choose one of ten original character illustrations, or take/select an Android photo. Android shows a crop/zoom preview before returning a 128 × 128 thumbnail. Save profile publishes the change; cancelling the picker does not change the saved profile. Camera access is requested only for taking a photo. The gallery uses the system document picker without broad storage access.

The player hub now follows the main menu's navy surfaces, lime primary actions,
cyan accents, italic Barlow headings and angular background. A persistent side
navigation separates editing, friends, search and room players. Profiles show a
large full character illustration; square avatars use a face crop in friend
rows, room seats and gameplay nameplates. Custom photos remain square.
The ten numbered character tiles show the selected choice before saving.
Existing avatar indices 0–5 map to characters 01–06, while indices 6–9 add the
remaining four characters. No account or database migration is required.
See [the restored original artwork](../ArtSource/PlayerPortraits/README.md).

The profile has a permanent, server-issued 16-character public code. Names can repeat or change; the code identifies the person. Provider email, provider identifiers, authentication tokens and original photographs are not public profile fields.

The Friends button opens accepted friends, incoming requests, sent requests and blocked players. Players can search by name or exact code, send/cancel a request, accept/decline a received request, remove a friend, block/unblock and use one Report player action with selectable reasons and an explanation. A crossed request still requires the recipient to accept. Declined/cancelled/removed relationships have a 24-hour request cooldown. Each player has at most 150 accepted friends plus pending incoming/outgoing requests, and 200 blocked players. Reports are stored as incidents for manual operator review. A shared multilingual vocabulary now filters new names and room text chat; image classification and automatic sanctions are not implemented. See [player safety](PLAYER-SAFETY.md) for gameplay entry points, evidence limits and validation.

Lobby seats and nearby character labels show names and pictures. Tap a populated seat, nameplate or Players-list entry to open the profile. Profile edits are fetched from the service by the room host and replicated, including to late joiners. The profile payload was introduced with protocol **73**; the current combined build uses **76**, and all room participants need matching builds. Profile fields are included in host-migration snapshots. Gameplay input is blocked while the social panel is open. Friends lists refresh every 25 seconds while displayed; they are not a push-notification service. Room invitations and global online/last-seen presence are not included.

## Identity, persistence and trust

- Unity Authentication player ID is the canonical account identity for anonymous guests and linked Huawei/Google users. Linking preserves that ID, public code and friend graph. Switching to an existing account loads that account's profile instead of merging identities.
- Profile caches are isolated per local guest/account (and per development test profile). Local edits survive restarting the game. If a save cannot reach the service, it remains pending on the device and is retried when opening the profile or joining online. Stale revisions are rejected; Reload cloud explicitly discards pending edits in favour of the server record.
- Guests need their stored Unity session to recover the online identity. Clearing app data/uninstalling can lose that recovery ability. Link a provider before relying on cross-device recovery. A local guest GUID is never accepted as a cloud credential.
- HTTPS profile endpoints validate the Unity access-token RSA signature, issuer, project, environment audience, subject and expiry against Unity's fixed JWKS endpoint. Every write derives the owner from the verified token. Clients cannot choose whose profile to edit. SQL uses bound parameters and compare-and-swap revisions.
- A room peer proves its identity using a random challenge and a one-use 90-second service token bound to that room. The host checks the verified subject against the connection's member ID before exposing a public profile; invalid/missing proofs disconnect the peer. Reusable UGS access tokens are never sent through game RPCs. This is a profile identity check, not comprehensive anti-cheat: gameplay remains hosted by a player and the existing connection/migration admission model still needs a wider security audit before an adversarial public release.
- Photos contain exactly 49,152 RGB bytes, with no image headers, EXIF, location or arbitrary remote URL. Source images are decoded with bounds, oriented, cropped and downsampled on the phone; input is limited to 20 MB. The backend enforces exact thumbnail length. Photo textures use a 64-entry cache and default-avatar fallback.
- Blocking suppresses profile lookup, search and friendship in both directions and removes an existing relationship. It does not kick someone from a current room or guarantee that an already downloaded picture disappears from another device.

## Name-search algorithm

1. Validate display names and retain their original NFC spelling. Search keys use Unicode compatibility decomposition, lowercase, accent removal and collapsed spaces/underscore/hyphen separators.
2. Recognize an exact public code first. Otherwise use indexed name ranges for exact and full-prefix matches; return 20 results per page. Duplicate names remain separate records with different codes.
3. Add indexed word-prefix candidates, then bounded typo candidates from trigram and single-character deletion indexes. Rank exact names before full-prefix, word-prefix and approximate suggestions. Adjacent letter swaps use optimal-string-alignment edit distance, so `Jonh` can find `John`.
4. Disable typo suggestions below four characters; allow one edit for lengths 4–7, two for longer queries. Candidate pools are bounded, so approximate suggestions are helpful rather than an exhaustive fuzzy-search guarantee. SQL wildcard characters are never interpreted as a user-supplied pattern.
5. Debounce typing by 400 ms, ignore stale responses and cache avatar images. Search excludes the caller, hidden profiles and blocked relationships. Renames atomically rebuild the indexes; photo-only edits do not.

## Deployed backend and no-charge boundary

The existing Cloudflare Worker `whatthefish-huawei-auth` now serves profiles/friends as well as Huawei verification. Its D1 database is `whatthefish-profiles`, bound as `PROFILES_DB`, with an Asia Pacific location hint. Both are on Free; no payment method or paid upgrade was added. Schema migrations are in [Tools/AccountServer/migrations](../Tools/AccountServer/migrations/), applied through the dashboard on 20 October. Current deployment ID: `f83871de-9bc8-48c4-ae36-61d40c395a53` (ten-avatar range update).

The Unity dashboard was inspected: **No payment method available**, and no active UGS service requiring a payment method. Google project's billing was disabled during sign-in setup. Free quota exhaustion causes service failures; it is not permission to enable a paid plan. Local/offline play remains available, while cloud profile, friend or room operations may fail. Future billing/plan changes must be reviewed separately.

## How many people and accounts?

These are different limits. A storage quota is not a tested concurrent-player capacity.

| Layer | Current limit / meaning |
| --- | --- |
| One room | Session cap 20 people. Football, Basketball and Golf allow 10 active players plus 10 spectators. Fishing allows 5 active plus 10 spectators (15 occupied places). Everyone travels together and one game is active per room. |
| Many rooms | Multiple rooms can run independently. Actual peak capacity depends on Relay, host phones, network traffic and backend load; no production load-test capacity is claimed. |
| Unity Relay free allowance | First 50 **average monthly concurrent users**, approximately 2,160,000 player-minutes in a 30-day month. This is not a hard 50-person peak limit. Bandwidth is 3 GiB per average CCU, capped at 150 GiB/month across regions. |
| Lobby free allowance | 10 GiB/month for each regional group (US/EU and Asia/Australia). |
| Authentication accounts | Authentication is free; the pricing page does not impose a fixed total registered-account count. Do not treat this as infinite capacity for profiles or social data. |
| Profile service | Workers Free: 100,000 requests/day shared across the account, 10 ms CPU/request. Huawei token verification, profiles, searches, pictures and friends consume that budget. |
| Profile database | D1 Free: 5 million rows read/day, 100,000 rows written/day; the current single database is limited to 500 MB. Account-wide 5 GB across up to 10 databases does not automatically enlarge this database. Index maintenance counts toward writes. |
| Stored profile count | No fixed account-count switch exists. At 49,152 bytes per uploaded thumbnail, the 500 MB database has room for roughly 10,000 thumbnails **before any profile/index/friend/database overhead**. Therefore the actual all-photo profile count is lower. Default-avatar profiles are much smaller. Measure real storage before expanding; this arithmetic is not an advertised capacity. |
| Per-player API throttles | 120 reads/minute and 20 writes/minute. Cloudflare rate-limit bindings are approximate and local to each location. They are abuse controls, not a global spending limit. |

For scale intuition, 100 people playing two hours daily use about 360,000 player-minutes/month, or 8.3 average CCU. That fits the connectivity-minute allowance, but it does **not** establish that the game's bandwidth or host devices can support that traffic. Similarly, 40 profile-service requests per daily player gives a request-budget ceiling of 2,500 daily players before other requests, database limits and peak load; leaving a friends list open generates one request per 25 seconds, so actual behavior matters.

This setup is suitable for a controlled playable test and competition demonstration once phone checks pass. It is not yet a measured public-production capacity promise. Verify Huawei/Google sign-in and camera/gallery permissions on actual devices, complete AppGallery privacy/deletion/reviewer requirements, provide an operator process for reported content, and load-test representative room sizes and bandwidth before public release. Google OAuth remains in its configured testing state. Google documents an exception to the usual test-user allowlist when only basic identity scopes are requested, as this game does; do not label this game as having a universal 100-account cap. Native sign-in and public release configuration still need verification. See [Google OAuth app states](https://developers.google.com/identity/protocols/oauth2/production-readiness/overview).

## Player hub refresh verification — 20 October 2026

Current UI evidence is under ignored `Builds/ProfileStyleQA/`. All **140 final
two-player/restart checks**, **36 backend tests** and **14 deployed-service
checks** passed. The live Unity Relay run covers cloud photos, all ten bundled
portraits, selecting/saving characters 09 and 10, replicated profile changes,
friend-request acceptance through the UI, friend/request tabs, clearing search,
shared travel and opening profiles from gameplay. Both independent restarts
restore their own chosen character and identity. Temporary cloud profiles are
removed after the checks.

The final rendered screens were reviewed at **1600 × 900**, **1200 × 900** and
**1920 × 824**. The twin-puff portrait has its own framing so both puffs remain
visible. Edited names and avatar selections show an unsaved-changes message
until Save profile completes. Original art hashes remain unchanged; the
restoration script also passed from a copied checkout with spaces and an
unrelated working directory.

The backend accepts indices 0–9 on the existing Free Worker/D1 deployment
`f83871de-9bc8-48c4-ae36-61d40c395a53`, with no database migration or paid service.
The Windows candidate uses the complete combined 21:58 asset build and a
supported scripts refresh at 22:16. All **404** compiled C# source documents
match the checked workspace. See [BUILD-SIZE.md](BUILD-SIZE.md) for measured
Android output and final delivery records. Physical Android input, camera,
gallery, permissions and performance remain unverified.

## Initial profile feature verification — 18:17 delivery

Original feature evidence lives under ignored `Builds/PlayerProfilesQA/`. All **35 backend tests**, **13 deployed-service smoke checks** and **61 two-player/restart checks** passed. The final two-player test used real Unity Relay, verified photo/name replication, friendship, shared travel and tapping a gameplay nameplate. It also ran from a copied checkout with spaces and an unrelated working directory. Separate launches restored each player's own saved name, photo and identity. Temporary cloud profiles were removed; credentials were not printed.

At that delivery, all 327 Windows player files matched the tested candidate on promotion to `Builds/WindowsFinal/`, and all 387 compiled C# source documents matched the then-current source. Additional checks passed: 12 local identity, 36 account UI, 110 Golf/Fishing settings and 183 Football character-identity assertions. The older Basketball character fixture failed its match-start assumption and 31 resulting checks identically on that candidate and the preceding player; that original record is not a passing result. Evidence includes `basketball-baseline-comparison.json`.

To repeat the opt-in real-service review, use `Tools/Build/Test-PlayerProfiles.ps1 -PlayerPath Builds/WindowsFinal/WhatTheFish.exe`. It creates isolated test identities, exercises friendship and removes its own temporary profiles. Anonymous Unity authentication records may remain. Backend tests run with Node 22 or newer using `npm test` in `Tools/AccountServer`.

Fresh AndroidSubmission at 18:17:45 Malaysia time produced a **94,765,380-byte APK**. The build gate, ZIP integrity, APK v2 signature and release native-bridge checks passed. The non-exported photo activity and scoped FileProvider are present; no broad image/storage permission was added. See [BUILD-SIZE.md](BUILD-SIZE.md). A Windows photo-picker explanation does not validate the Android camera or gallery. Original image quality, native permission dialogs, process recreation, provider sign-in and physical-phone FPS need device testing.

## Primary sources checked 20 October 2026

- [Unity service pricing](https://unity.com/products/gaming-services/pricing) and [free-tier billing behavior](https://docs.unity.com/en-us/services/pricing-and-billing).
- [Workers limits](https://developers.cloudflare.com/workers/platform/limits/), [D1 limits](https://developers.cloudflare.com/d1/platform/limits/), [D1 quotas and pricing](https://developers.cloudflare.com/d1/platform/pricing/).
- [Unity player-token JWKS API](https://docs.unity.com/en-us/oas-player-auth-jwks/1.0.0) and [authentication sessions](https://docs.unity.com/en-us/authentication/how-authentication-works).
