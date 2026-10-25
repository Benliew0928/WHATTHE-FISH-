# Technical preparation and remaining owner steps

## Implemented locally

- Settings and account choices expose **Privacy & data** without requiring a
  provider login. The page describes the actual optional camera/voice features.
  It does not invent a publisher, contact address or public policy URL.
- A real HTTPS policy URL can later be supplied as a TextAsset at
  `Game/Assets/_Game/Resources/AppGallery/PrivacyPolicyUrl.txt` (with Unity metadata).
  Without it, the external policy button remains disabled. The in-game information
  is not a substitute for the final public policy.
- Account removal requires a second screen and typing `DELETE`. Linked accounts
  reverify the same Huawei/Google identity before starting; a different account is
  rejected and the previous session restored. A purely offline guest is removed
  locally without creating an online identity.
- Cloud profile/social deletion is transactional and idempotent. A SHA-256
  identity marker prevents a stale client from recreating the deleted profile.
  Other players' profiles are not removed; relationship/report/invitation records
  belonging to the deleted profile are removed through foreign-key cascades.
- Unity Authentication deletion follows confirmed profile deletion. Local
  credentials, active profile/photo cache and per-profile control layouts are then
  cleared. Underlying Huawei/Google accounts are never deleted.
- A local progress journal contains only the account ID, local profile scope and
  stage, never authorization tokens. Incomplete deletion blocks online play and
  normal account switching until retried. No success is shown for an ambiguous
  network outcome. If Authentication accepted deletion but its response was lost,
  automatic sign-in recovery may fail: support must resolve that case; do not
  promise every network failure is automatically recoverable.

## Backend activation is pending

Wrangler on this computer reports `loggedIn:false`. No production database or
Worker has been changed. The client checks server capability before deleting
anything; the old server returns a clear unavailable message. Local implementation
does not mean live account deletion is enabled.

Once the owner is ready, authenticate locally in a terminal from
`Tools/AccountServer`:

```powershell
npx wrangler login
npx wrangler whoami
```

Review the selected Cloudflare account and existing database binding before writes.
Inspect whether the new table already exists:

```powershell
npx wrangler d1 execute whatthefish-profiles --remote --command "SELECT name FROM sqlite_master WHERE type='table' AND name='account_deletions';"
```

If absent, apply **only** the new additive migration, then deploy the reviewed
Worker. Do not reapply migrations 0001–0003: historical setup used the dashboard
and did not register Wrangler migration history.

```powershell
npx wrangler d1 execute whatthefish-profiles --remote --file migrations/0004_account_deletions.sql
npm run deploy
```

Migration must precede deployment because profile APIs query the new table. The
new table does not remove existing data by itself. Keep the previous Worker version
for rollback; do not drop the table or its revocation markers during rollback.
Afterward use disposable test identities for capability, deletion, retry and
cross-client recreation checks. Never test deletion against a real player's data.

Retention for pseudonymous revocation markers, provider security records and
backups needs an explicit operator policy. Markers currently retain hash/time,
not names, photos, raw player IDs or credentials. Transient room data already
received by other participants is not remotely erased by this endpoint. Verify
Unity/Vivox service-side retention and support procedures before public release.

## Signing helper

Run `Tools/Build/AppGallery-Release.ps1` using PowerShell 7. Paths for the default
APK resolve from the script, not the current shell directory. Java and Android
build-tools locations have explicit override parameters.

Read-only inspection:

```powershell
./Tools/Build/AppGallery-Release.ps1 -Action Inspect
```

When ready to create the production key, choose an existing secure directory
outside this checkout and set `WTF_RELEASE_KEYSTORE` locally to its intended new
file. Do not put that machine path into committed files. The script refuses
existing files, repository locations and junction/symlink directories.

```powershell
./Tools/Build/AppGallery-Release.ps1 -Action CreateKey -KeystorePath $env:WTF_RELEASE_KEYSTORE -CertificateName 'WhatTheFish Release'
```

Password entry is hidden and confirmed. The PKCS12 store and key use the same
password. It is passed through the child process environment, not command-line
arguments or a saved configuration. Keep the key/password in secure backups.
Do not create a second release key for routine updates.

Build the intended source with the existing AndroidSubmission workflow, then
sign using the public SHA-256 fingerprint printed when creating the key:

```powershell
./Tools/Build/Build.ps1 -Target AndroidSubmission
./Tools/Build/AppGallery-Release.ps1 -Action Sign -KeystorePath $env:WTF_RELEASE_KEYSTORE -ExpectedCertificateSha256 $env:WTF_RELEASE_CERT_SHA256
```

Signing happens outside Unity, so project initialization cannot reset the final
signing identity. The helper snapshots the input, refuses a debuggable/wrong-package
APK, aligns it, signs it and verifies the exact expected certificate, alignment
and project size limit. It leaves the original test APK unchanged. Output and
public evidence are in a unique ignored `Builds/AppGallery/` batch. Archive reviewed
intermediate inputs with the normal archive tool after validation.

This is a signing tool, not a store-readiness approval gate. A verified signature
does not prove source freshness, correct version code, privacy or phone quality.
Production key creation/signing remains pending private owner password entry.

## Still dependent on owner or external access

- Publisher/legal name, public support email, policy hosting, age/audience choice,
  retention and moderation operator details.
- AppGallery developer identity verification and final certificate registration.
- Cloudflare local authentication and production backend activation.
- Huawei Android device testing, including no-GMS login/QR, microphone/Bluetooth,
  same-provider deletion verification and second-device account recovery.
- Google OAuth production audience if Google login remains public.
- Final store materials, device screenshots and submission.

Malaysia, free download, no advertising and no in-app purchases are confirmed.
