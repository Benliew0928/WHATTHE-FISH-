# Technical preparation and remaining owner steps

## Public website preview — 26 October 2026

After the local work below, the owner separately authorized publication through
Cloudflare. The development preview is live at <https://whatthefish-game.pages.dev/>.
See [deployment and validation](../../Website/DEPLOYMENT.md). The generator's
`--public-preview` mode adds translated hosting disclosures without writing game
resources. No game URL configuration, new player build, signing or store
submission was included in this website-only deployment.

## Local website and welcome Info preparation — 26 October 2026

`Website/content.json` supplies the English, Simplified Chinese and Bahasa Melayu
drafts. `Tools/Build/Prepare-PublicInfo.py` generates 12 static pages under ignored
`Builds/PublicInfoQA/Site/`, refreshes the shared localization catalog and writes
`Resources/AppGallery/Documents.json`. `--check` validates current outputs and
relative page links. `Tools/Build/Preview-PublicInfo.ps1` serves only on loopback.
See [website authoring and preview instructions](../../Website/README.md).

The welcome Info button now offers Terms, Privacy and Website instead of sport
introductions. Terms/Privacy open bundled, scrollable preparation drafts while
the public URL is absent. Website stays disabled with explanatory text. The
Settings/account Privacy entry uses the same configured website policy URL,
with the older standalone policy URL supported for compatibility.

`Resources/AppGallery/Links.json` intentionally has an empty `baseUrl`. Later,
after separate publication authorization and policy acceptance, a real HTTPS
base directory resolves localized pages. No placeholder domain is shipped.
No hosting registration, upload, deployment or production setting was changed
by that earlier local preparation; the later website deployment is recorded above.

Owner decisions and current storage/deletion gaps are recorded in
[the operations proposal](OPERATIONS-PROPOSAL.md). Proposed retention periods
must not become public promises until implemented and tested.

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

## Backend activation — 26 October 2026

The approved account coin implementation required the existing profile API's
deletion table. Migrations **0004 and 0005** were applied after inspecting the
live schema and recording a recovery bookmark. The reviewed Worker is deployed;
see [coin deployment evidence](../COIN-ECONOMY.md) for its version and smoke checks.
This establishes backend activation, not complete native account deletion or
second-device recovery on a phone.

Existing Wrangler authorization supported Worker deployment but lacked D1 access.
The already authenticated Cloudflare dashboard applied the additive schema; no
new access grant was made. The database has no Wrangler migration-history records.
**Do not reapply migrations 0001–0005 blindly.** Inspect the schema first. Keep
revocation markers during rollback and use only disposable identities for future
deletion acceptance tests. The coin release's live smoke removed only its own
synthetic profiles, without deleting a real player's account.

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

- Final policy acceptance, retention periods and operational support/report/deletion
  procedures. Personal publisher LIEW KAIY BIN supplied public support/privacy
  email benliew28262826@gmail.com and will handle these requests.
- All-ages audience suitability, child/guardian controls and official rating;
  all ages replaces the earlier 13+ proposal without certifying those controls.
- Intended app/account ownership and final certificate registration. The owner
  reports Huawei developer identity verification completed on 26 October 2026.
- Native end-to-end deletion acceptance following the recorded backend activation.
- Huawei Android device testing, including no-GMS login/QR, microphone/Bluetooth,
  same-provider deletion verification and second-device account recovery.
- Google OAuth production configuration and no-GMS compatibility: the owner wants
  Google retained if compatible with AppGallery acceptance; approval is unverified.
- Final store materials, device screenshots and submission.

Malaysia, free download, no advertising and no real-money in-app purchases are confirmed.
English is the requested preparation/discussion language; player-visible text
continues to require English, Simplified Chinese and Bahasa Melayu. These owner
decisions alone do not authorize further production changes or store submission.
