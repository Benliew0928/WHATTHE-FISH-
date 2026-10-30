# AppGallery submission — 29 October 2026

WHATTHE FISH? **0.3.0 (1)** has been submitted to Huawei AppGallery. After the
final confirmation, the console displayed **Submitted / Reviewing**. This state
was verified again by reopening the version page, around 20:00 Malaysia time.
It is not an approval or a public release.

| Item | Confirmed value |
| --- | --- |
| App / package | `119406947` / `com.umpsa.whatthefish` |
| Console version | `v2064031945490198016` |
| Region | Malaysia only |
| Rating | Other Regions 12+ |
| Price | Free; no advertising or real-money in-app purchases |
| Publication setting | **Immediately once approved** |
| Submitted APK | `Builds/AppGallery/9f15be30251541e8a180e1f03bb6bad7/WhatTheFish-AppGallery.apk` |
| Bytes / SHA-256 | **172,597,919** / `48F52571E7F92602FBCC03E423B4D8CEB2087F53EDCA4E0E728AB83D29C47146` |
| Signer | Production SHA-256 `8409C94B295A81D8F653AAFC7015D50DA85169E9EBC423723DAB11838F9BD826`; non-debuggable |
| Listing | Existing English, Simplified Chinese and Malay copy, with three screenshots each |
| Policy / rights URLs | <https://whatthefish-game.pages.dev/privacy> / <https://whatthefish-game.pages.dev/support> |

The owner expressly requested submitting and publishing the current app without
adding email approval. No age gate, guardian flow, game asset or client binary
was added or changed. The existing pending child/privacy/provider/device checks
were not represented as passed. Support tickets remain separate from review.

The provider-disclosure and cross-border privacy scenarios were completed with
the twelve existing data-inventory categories: contact/friends list, user IDs,
other device identifiers, profile/account information, voluntary support email,
images, voice, text, player-search terms, game statistics, support records and IP
addresses. The existing functionality/personalization scenarios were retained.
These categories include service-provider processing; they do not assert that
the game requests provider email/password/contact-book scopes. Huawei's broad
example descriptions are its supplied category text.

The saved reviewer note states exactly:

> Guest play is available. For online rooms, use two devices: host a room and join its code. QR and voice are optional. This build has no age check or guardian/email approval flow. Linked Terms/Privacy pages remain working drafts; these controls are not claimed as implemented.

The final dialog confirmed version, default language and release-region count.
It presented no new agreement or guardian-compliance attestation. The final
Submit action was taken under the owner's explicit submission authorization.
The console then locked the release fields and showed **Reviewing**. Its
separate Integration Check history showed **Completed / Passed** for 0.3.0;
this is not final AppGallery approval or proof that every feature passed tests.

## Evidence and remaining scope

- `Builds/AppGallerySubmissionQA/current-submitted-reviewing.png` shows the
  submitted state and Malaysia selection.
- `current-submit-confirmation.png` and `current-integration-check-passed.png`
  retain the final confirmation and separate integration-check result.
- `current-submission-apk-inspection.json` rechecks package, version, certificate,
  permissions, size and non-debuggable status. All **388** recorded compiled
  Android source hashes still match the workspace.
- `current-submission-result.json` records the observed console state and
  reviewer disclosure; readiness evidence is retained beside it.

No staging, commit, push, new Unity build, signing-key rotation or paid service
change occurred. Public release still depends on Huawei approval. A rejection
or requested correction must be handled from its actual review comments; do not
interpret submission acceptance as resolution of the recorded privacy gaps.
