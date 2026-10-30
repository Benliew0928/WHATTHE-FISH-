# Owner record — keep for future releases

**29 October submission:** WHATTHE FISH? 0.3.0 (1) is **Reviewing** in Huawei,
Malaysia only, configured to release immediately once approved. The owner
requested submission without adding email approval; the actual reviewer note
discloses that no age/guardian flow exists and policy pages are still drafts.
See [the confirmed submission](SUBMITTED-2026-10-29.md). It is not public yet.

Recorded 28 October 2026. This file contains public identifiers and operating
notes. Keep passwords, private keys, recovery codes and service secrets in your
private password manager or encrypted backup instead.

## The item you must back up

**Production signing key:** `%LOCALAPPDATA%/WhatTheFish/ReleaseSigning/WhatTheFish-release.p12`

- Alias: `whatthefish-release`.
- Password: the password you entered privately; it is not stored in this record.
- Owner confirmed the key and password were backed up on 28 October 2026.
- Keep an encrypted backup in a separate secure location. A copy only on this
  computer is vulnerable to disk loss. Keep a private note of the backup location.
- Reuse this key for future releases. Do not create a new key for routine updates.
- Keep the key out of the game folder, Git, public uploads and chat. AppGallery
  receives the signed APK; the private signing key is not part of that APK.

Public certificate details:

| Item | Value |
| --- | --- |
| Certificate owner | `CN=LIEW KAIY BIN, C=MY` |
| Algorithm | RSA 3072 / SHA256withRSA |
| Created | 28 October 2026 |
| Valid until | 15 March 2054 |
| SHA-256 | `84:09:C9:4B:29:5A:81:D8:F6:53:AA:FC:70:15:D5:0D:A8:51:69:E9:EB:C4:23:72:3D:AB:11:83:8F:9B:D8:26` |
| SHA-1 | `3B:B1:6D:AA:E7:CF:19:3F:54:71:7E:46:5A:2C:2F:41:43:09:CC:B9` |

Fingerprints are public identifiers, not passwords. If store re-signing is
introduced later, record the installed application's certificate separately.

## App and service identifiers

| Item | Value |
| --- | --- |
| Game / store name | WHATTHE FISH? |
| Personal publisher | LIEW KAIY BIN |
| Public support and privacy email | benliew28262826@gmail.com |
| Android package | `com.umpsa.whatthefish` |
| Prepared version | `0.3.0` / version code `1` |
| Huawei App ID | `119406947` |
| Huawei project | WhatTheFish / `101653523865366415` |
| Unity project display name | SportsPrototype — this is the WHATTHE FISH? project |
| Unity project ID | `633e314c-46d1-4b48-bbbc-2dc5f6fb653c` |
| Unity production environment | `19215999-e921-4fb8-b162-6e2561b1b864` |
| Cloudflare Pages project | `whatthefish-game` |
| Cloudflare account ID | `5ca8adf5bf1877cb5cae3f96869edcd0` |
| Cloudflare Worker | `whatthefish-huawei-auth` |
| Cloudflare D1 database | `whatthefish-profiles` / `ef48165d-6530-49e3-ab47-cc27a7e74623` |

Keep access to the owner accounts for Huawei, Unity, Cloudflare, Google OAuth
and the source repository, including their private recovery methods. Do not
replace or delete the existing Unity `whatthefish-huawei-auth` service account:
it supports the game's Huawei sign-in.

Public pages: [website](https://whatthefish-game.pages.dev/),
[privacy](https://whatthefish-game.pages.dev/privacy),
[terms](https://whatthefish-game.pages.dev/terms),
[support and privacy requests](https://whatthefish-game.pages.dev/support).
The public policies remain drafts until the remaining release decisions and
implementation are complete.

## Current files to keep

- [Production-signed APK](../../Builds/AppGallery/9f15be30251541e8a180e1f03bb6bad7/WhatTheFish-AppGallery.apk):
  **172,597,919 bytes**, SHA-256
  `48F52571E7F92602FBCC03E423B4D8CEB2087F53EDCA4E0E728AB83D29C47146`.
- [Signature record](../../Builds/AppGallery/9f15be30251541e8a180e1f03bb6bad7/signature-record.json)
  and [release verification](../../Builds/AppGalleryReleaseQA/).
- [Store screenshot manifest](../../Builds/AppGallerySubmissionQA/store-screenshot-manifest.json)
  and its nine original Android PNGs (three per language). All nine were
  uploaded and saved to Huawei; retain the originals for later listing updates.
- The complete `Builds/WindowsFinal/` folder, including the data folder, DLLs
  and launchers. The executable alone is not the game.
- Editable source, Unity assets and metadata, authoring masters, package locks,
  build tools and tests. These are needed to maintain and reproduce the game.
- Recovery archives listed in [the cleanup report](../CLEANUP-2026-10-28.md).
  `Legacy/` stores recovery material and does not free disk space.

For every later Android update: use a higher version code than every uploaded
version, build AndroidSubmission, sign with the same key, verify the certificate
and package, test the signed build on devices, then update the release record.
The normal Unity build currently uses development signing; the final
[signing helper](../../Tools/Build/AppGallery-Release.ps1) applies production
signing afterward with a private password prompt.

## Reset and launch status

The approved pre-launch reset deleted **567 Unity Authentication identities**
and **42 Cloudflare profiles**. Unity's final API listing was empty; Cloudflare
profiles and related game-data tables were empty. **567 minimal hashed
revocation markers** remain to prevent old sessions recreating deleted profiles.
The temporary reset service account and key were removed; the revoked key
returned HTTP 401. Local PC identities were not cleared because automatic
approval review blocked that local operation.

This APK is a release candidate, not proof of AppGallery approval or a live
store release. Keep the [current submission status](PRE-SUBMISSION-STATUS.md)
with this record. Physical-device acceptance, final policy and children's
communication/privacy arrangements, retention activation, Google OAuth
production settings and remaining store declarations remain checks. The owner
approved the calculated 12+ rating for Malaysia; it is now saved and verified
on the reopened Huawei version. Nine Android screenshots are uploaded and saved.
The owner plans daily support/report review and adopted the four retention
periods in [the operating plan](OPERATIONS-PROPOSAL.md). The deletion fix and
retention handler are deployed with both indexes verified. Daily cleanup is
enabled for 03:17 Malaysia time; the first run remains unverified. See
[rollout/version/rollback record](RETENTION-ROLLOUT.md).
The periods are not yet a published service
commitment. The owner originally retained all ages and requested resolving child
protections first. They later requested proceeding with submission and accepted
the calculated 12+ rating. That rating does not itself implement consent or
guardian controls; final privacy/child-protection acceptance remains open.

Huawei rules 8.12–8.13 require a children's policy and parental permission for
minors' data; they do not require KWS specifically. The [minimum requirements](MINIMUM-REQUIREMENTS.md)
record the current gap. Neither KWS nor an adults-only online restriction was
selected. One new D1 profile existed during the later rollout and was left
untouched; the earlier reset is not a claim that the database stays empty.

Keep the [provider support references](PROVIDER-QUESTIONS.md): Huawei **D650176**
and Unity **#3623478**, submitted with owner authorization on 29 October 2026.
They ask which permission evidence and service settings are adequate for the
Malaysia 12+ launch. Replies remain pending; these tickets do not submit the app
or approve a consent design.

Asset-source clarification on 29 October: the golf cart was generated by the
owner's teammate. The owner subsequently explicitly chose to keep the current
cart and Golf GO/walkway effects and continue release preparation. Replacement
work is cancelled; do not repeat the asset-choice questions. Existing provenance
remains recorded accurately. See [the source review](ASSET-SOURCE-REVIEW.md).

On 29 October, the exact signed APK was tested on Huawei's real cloud P40
(ANA-NX9, EMUI 10.1.0/API 29). Three-language welcome layouts, local guest,
Malay menus and a Golf round/aim/shot were exercised; the device was released.
Successful Huawei authentication, multiplayer, voice and full performance
acceptance remain open. See [the device record](HUAWEI-CLOUD-DEVICE-2026-10-29.md).
Documented rod/club attribution is published on the existing three-language
Cloudflare website. Its terms and privacy notice remain working drafts.
