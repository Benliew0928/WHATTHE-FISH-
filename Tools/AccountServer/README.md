# Identity, player profiles and friends

## Account coins and deployment — 26 October 2026

Worker `13c03113-5f15-43ea-8df5-01f2774eda13` now serves the account wallet,
daily streak, match settlement and cosmetic purchase/ownership APIs. Apply all six
migrations in order only to a new database. On the existing database, migrations
0004–0006 were applied after confirming 0001–0003 already existed. Its
dashboard-managed schema has no Wrangler migration-history records.

Authenticated economy routes are `GET /v1/economy/wallet` and POST routes
`daily`, `equip`, `purchase`, `matches/start`, `matches/lookup`, `matches/join` and
`matches/finish` under `/v1/economy/`. The account comes from the verified bearer
token. Clients cannot submit a balance or arbitrary credit amount. The reward
ledger, daily update and whole-match settlement are atomic and retry-safe.
The shared catalog grants two starters per category; each remaining item costs
1,000 earned coins. A unique purchase receipt atomically debits and grants it.
See [store rules and upgrade behavior](../../Docs/COSMETIC-STORE.md) and
[coin rules and trust boundary](../../Docs/COIN-ECONOMY.md).

The local backend suite passes 71 tests. The deployed economy smoke passed fourteen
checks with real UGS tokens; synthetic profiles were removed afterward. Run
`node economy-smoke.mjs` only for an intentional live check. Local client testing
uses `economy-fixture.mjs` on loopback with fake identities; it is not deployable
authentication. No service permissions or paid plans were changed.

## Account deletion — activated 26 October 2026

New source adds authenticated `GET/DELETE /v1/account-deletion`. GET advertises
contract version 1; DELETE requires `{"confirm":"delete-game-account"}` and
transactionally records a hashed identity revocation marker and removes the
profile plus FK-owned social data. Regular profile API access is then rejected;
profile creation also checks the marker inside its SQL statement to close a
concurrent-write race. The client deletes Unity Authentication afterward.

The schema and route are now deployed as prerequisites of the economy release.
This activation does not establish physical-device completion of the full native
Unity account-deletion flow. Do not blindly run all migrations against the
dashboard-created database. Clients refuse to start deletion when capability is unavailable.
See [activation, retention and failure limits](../../Docs/AppGallery/TECHNICAL-HANDOFF.md).
The deletion preparation passed 48 tests, including ownership/confirmation, stale-client
recreation, idempotency, invitation cleanup and transaction failure behavior.

This service verifies Huawei Account Kit ID tokens and exchanges the verified
identity through Unity Authentication Custom ID. The Android application contains
only public configuration; Unity service credentials belong exclusively in the
Worker's encrypted secrets.

## Hosting configuration — established 20 October 2026

- Cloudflare Worker: `whatthefish-huawei-auth`, Workers Free.
- Public endpoint: `https://whatthefish-huawei-auth.benliew28262826.workers.dev/v1/huawei`.
- Google Cloud project `whatthe-fish` has billing disabled. Google sign-in uses
  Unity's Google provider directly and does not call this Worker.
- Unity project `633e314c-46d1-4b48-bbbc-2dc5f6fb653c`, environment `production`.
- Service account `whatthefish-huawei-auth`: **Player Authentication Token Issuer**
  on that project only. No organization-wide role.
- Initial Huawei adapter deployment: `ca2e8e94-8824-4354-951f-17aeb71f7dae`.
  See [the profile release record](../../Docs/PLAYER-PROFILES-PLAN.md) for the
  later ten-avatar backend deployment.
- D1 Free database `whatthefish-profiles`, binding `PROFILES_DB`.
- Android application and Google OAuth production audience have not been published.

The configured Cloudflare plan has a 100,000-request daily allowance and a
10 ms CPU allowance per request. The profile database was added on Free.
No paid upgrade or payment method was added. Free-plan exhaustion can make
online features unavailable; offline guest play remains available.
The Huawei binding's 60 requests/minute/IP limit is
approximate and local to each Cloudflare location, not a global spending cap.
Check the account plan before future changes. See
[Workers pricing](https://developers.cloudflare.com/workers/platform/pricing/)
and [rate-limit semantics](https://developers.cloudflare.com/workers/runtime-apis/bindings/rate-limit/).

## Profile API

All `/v1/me`, `/v1/players`, `/v1/photos`, `/v1/search`, `/v1/friends`,
`/v1/blocks`, `/v1/reports`, `/v1/invites` and `/v1/room-proof` endpoints require the caller's
Unity access token in the Bearer header. The verifier checks signature, issuer,
project, environment and lifetime; it never accepts a player ID as authentication.
Public profile responses contain no email, provider ID or credentials.

- `GET/PUT /v1/me`: read/save the caller's profile. PUT includes name, avatar
  0–9, expected revision (0 to create), and optional photo. `photo:""` clears
  the photo; omitting it retains the current one. Photos are exactly 128×128
  top-to-bottom RGB24 bytes in Base64, not original image files.
- `GET /v1/players/:id`, `/v1/photos/:id/:avatarVersion`: public profile and
  authenticated binary thumbnail reads, subject to blocks.
- `GET /v1/search?q=...&offset=...`: ranked exact/prefix/typo search, 20/page.
- `GET /v1/friends`: accepted, incoming and outgoing records. POST accepts
  target plus action `request`, `accept`, `decline`, `cancel` or `remove`.
- Blocks use GET/POST/DELETE. `GET /v1/reports` advertises version 2;
  POST accepts target, reason, details, context, evidence and requestId.
  Reasons are `name`, `photo`, `abuse`, `sexual`, `behaviour`, `cheating`,
  `spam`, `other`; Other requires details. Details allow 500 UTF-16 units.
  Reports capture server profile metadata (and thumbnail for picture reports),
  enforce retry identity and rate limits, and require manual operator review.
  Apply migration `0007_report_incidents.sql` before deploying this version.
  See [player safety](../../Docs/PLAYER-SAFETY.md) for the flow and review query.
- Name saves reject listed offensive terms using the shared EN/ZH/MS vocabulary
  in `Game/Assets/_Game/Resources/TextSafety.json` (relative to repository root).
  `node moderation-smoke.mjs` tests the deployed contract with temporary profiles.
- `GET/POST /v1/invites`: list received room invitations or send one to an
  accepted friend. Invitations expire after 120 seconds, allow one pending
  invitation per pair and enforce a 30-second resend cooldown and 20-entry
  inbox/outbox limits. The inbox does not disclose the room code.
- `POST /v1/invites/resolve`: the recipient resolves a still-valid invitation
  immediately before joining. `POST /v1/invites/respond` accepts `declined`
  or `joined` from its recipient, or `cancelled` from its sender. Removing
  a friend or blocking them revokes their invitations. Room admission still
  checks the actual host's capacity, gameplay state and protocol version.
- POST room-proof issues a one-use 90-second token bound to room/challenge;
  POST room-proof/verify consumes it and returns the authenticated profile.
- `DELETE /v1/me`, body `{"confirm":"delete-profile"}`, removes the caller's
  profile and cascading social data. This is not UGS/provider account deletion
  and is not exposed as a complete account-deletion flow in the game.

Read/write limits are 120/20 requests per minute per authenticated player.
There are 150 combined friends/pending requests, 200 blocks and 200 retained
reports per reporter. The service uses prepared SQL, transactional index
triggers and optimistic profile revisions. A block does not eject room players.

Migrations `0001_profiles.sql`, `0002_profile_limits.sql` and
`0003_room_invites.sql` were applied via the
Cloudflare dashboard. They must be applied in order to a new database before
binding/deploying it. The current dashboard-applied database does not have
Wrangler migration-history records; do not blindly reapply the initial migration.
Do not use `server.mjs` as a local profile server: it implements the Huawei
adapter only. Use Wrangler's local D1 runtime for profiles.

Run `npm test` for the local suites. `node profile-smoke.mjs` is an **opt-in live**
test: it creates two anonymous Unity identities and temporary cloud profiles,
checks ownership/photo/search/friend/proof behavior, and deletes its own profile
records in `finally`. UGS anonymous identities may remain; credentials are
memory-only. See [game behavior and capacity](../../Docs/PLAYER-PROFILES-PLAN.md).

`node invitation-smoke.mjs` is a separate opt-in live invitation test. It
creates three temporary anonymous players, exercises friend access, recipient
isolation, expiry metadata, cooldowns and block revocation, then deletes its
own profiles in `finally`. It never messages an existing player's friends.
The invitation release passed 44 local tests and 13 live assertions on
24 October 2026; service version `af0271d6-ee24-45b1-9361-1c9b854ecf51`.

## Reproduce and deploy

Requires Node.js 22 or newer. From this directory:

```powershell
npm ci
npm test
npm run check:worker
npx wrangler login
```

Use the approved account; Wrangler needs account/user read and Workers Scripts
write access. On a shared Windows machine, prefer Wrangler's `--use-keyring`
option with its supported OS keyring dependency installed. Do not commit its
local authorization files. The existing developer machine uses encrypted
Wrangler credentials with the key in Windows Credential Manager.

Set `CLOUDFLARE_ACCOUNT_ID` in the shell for the selected account. Review the
public project/environment identifiers in [wrangler.jsonc](wrangler.jsonc).
Create a Unity service account with only the project-scoped role above. Enter
the two values privately into Wrangler's prompts:

```powershell
npx wrangler secret put UNITY_SERVICE_KEY_ID
npx wrangler secret put UNITY_SERVICE_SECRET
npm run deploy
npx wrangler secret list
```

Run these commands from this directory so Wrangler's configuration and credential
storage are resolved consistently. Never place secret values in command
arguments, committed JSON, screenshots or build environment variables. Key
rotation means replacing the Worker secrets and then revoking the old Unity key.

For local development, copy the public configuration values into environment
variables and supply the two secrets privately. `npm start` runs the Node server
on loopback port 8787; `npm run dev` uses Wrangler. An ignored `.dev.vars` file
can supply Wrangler development values, but is plaintext local storage; do not
use or retain production secrets there unnecessarily. `node_modules/`,
`.wrangler/`, and `.dev.vars*` are ignored.

## API and trust boundary

`GET /health` returns `{"status":"ok"}` when required configuration is present.
It does **not** prove that the Unity key is valid or that a real account can log in.

`POST /v1/huawei`, `Content-Type: application/json`:

```json
{"idToken":"<fresh Huawei ID token>","link":true,"accessToken":"<current UGS player access token>"}
```

For account recovery use `link:false` and omit `accessToken`. Successful responses
contain the Unity `playerId`, `accessToken` and `sessionToken`. These are private
authentication credentials, not public profile data.

- Verify RSA signature against the fixed Huawei JWKS endpoint, exact issuer
  `https://accounts.huawei.com`, app audience, subject and validity times.
  Require an ID token issued within ten minutes, with 30 seconds of clock skew.
  The native Huawei flow requests a fresh ID token.
- Derive Custom ID from the verified issuer/app/subject; callers cannot choose a
  player ID or an arbitrary Custom ID.
- Linking passes the current UGS access token to Unity, which verifies ownership
  and rejects conflicts. Recovery uses `signInOnly` to avoid accidentally
  creating a replacement account.
- No force-link or profile merge. No client secret or service key is returned.
- JSON body maximum 32 KiB, fixed upstream URLs, ten-second upstream timeouts,
  redirects refused, bounded response checks, generic errors and no-store headers.
- Browser `Origin` requests are rejected. This is not device attestation:
  non-browser callers can omit Origin and still must pass identity verification.
- Tokens are bearer credentials. A stolen valid token can be replayed during its
  validity window; this version does not implement a per-login nonce/replay store.
  Do not describe the rate limiter or freshness check as replay prevention.
- Worker logs/traces and Wrangler usage metrics are disabled; application code
  does not log requests, tokens, identities, vendor error bodies or secrets.

The dependency-free `auth.mjs` uses Node crypto supported by the Worker's
`nodejs_compat` flag. `server.mjs` is a local development harness, not a second
public production service.

## Verification and remaining limits

`npm test` passes 71 tests across authentication, profiles/social data, deletion,
economy rules, transaction handling and the actual Worker HTTP boundary.
No real account is created by these tests.

From the repository root, a deployed smoke check can be run with:

```powershell
node Tools/AccountServer/smoke.mjs https://whatthefish-huawei-auth.benliew28262826.workers.dev Builds/AccountSignInQA/deployed-worker-checks.json
```

All seven checks passed against the deployed version. A locally signed forged
token must return 401 even when it names a current public Huawei key. The probe
uses an application User-Agent; Cloudflare blocked a generic Python urllib
User-Agent with error 1010 during an earlier diagnostic.

Physical-device success, native HTTP behavior, Unity credential exchange with a
real Huawei token, linking conflicts and second-device recovery remain unverified.
The full device matrix and Android configuration are in
[PLAYER-LOGIN.md](../../Docs/PLAYER-LOGIN.md).

Primary references:
[Unity Custom ID](https://docs.unity.com/en-us/authentication/platform-signin/custom-id),
[Workers Node crypto](https://developers.cloudflare.com/workers/runtime-apis/nodejs/crypto/),
[Huawei signing keys](https://oauth-login.cloud.huawei.com/oauth2/v3/certs).
