# AppGallery release preparation

**Current status — 29 October:** WHATTHE FISH? 0.3.0 (1) is **Submitted /
Reviewing** for Malaysia, with release set to **Immediately once approved**.
The owner requested submitting the existing build without email approval;
the reviewer note discloses the absent age/guardian controls and draft policy.
See [submission confirmation](SUBMITTED-2026-10-29.md). Pending acceptance
checks below were not certified as passed by this submission.

Technical preparation was first validated on 25 October 2026 against source
commit `dc0be40`; see [historical validation](VALIDATION.md) for its exact scope.
Owner decisions were updated on 26 October 2026. On 28 October the owner
authorized release preparation, production signing and a confirmed pre-launch
account reset. This is not evidence of store acceptance or a live release.
Current build measurements are in [build size](../BUILD-SIZE.md).

The owner reports game development finalized on 28 October. The subsequent
[folder cleanup](../CLEANUP-2026-10-28.md) preserved source and useful deliveries.
A fresh source-matched candidate is now production-signed; Huawei draft work
has begun. See [current status](PRE-SUBMISSION-STATUS.md) and
[what the owner should save](OWNER-RECORD.md).

## Owner decisions and remaining information

| Item | Status |
| --- | --- |
| Publisher | LIEW KAIY BIN; personal/individual publisher, name supplied by owner to match the Huawei developer account |
| Public support and privacy contact | benliew28262826@gmail.com, supplied by owner |
| Launch countries and intended audience | Malaysia; owner accepted the calculated **12+** rating, now saved in the Huawei draft. Parental-permission and suitability review remains open; KWS is optional |
| Free/paid, advertising and purchases | Free download, no ads, no real-money in-app purchases confirmed |
| Public privacy-policy host/domain | Cloudflare Pages development preview published at <https://whatthefish-game.pages.dev/>; three languages, game Terms/Privacy remain drafts; no custom domain required |
| Verified Huawei developer account | Owner reports verification completed; intended app ownership and final signing configuration still need checking before release |
| Production keystore and backup | Owner created the private key outside Git and confirmed backup on 28 October; public fingerprints and alias are in the owner record |
| Huawei Android device model and availability | No Huawei test phone available; device checks remain pending |
| Support, reports and deletion exceptions | Owner handles these and selected daily review; procedure drafted, no guaranteed resolution deadline |
| Data retention periods | Four periods adopted 28 October; 85-test backend suite passes. Deletion fix and daily cleanup schedule deployed, both indexes verified; first scheduled run remains unverified and mailbox cleanup is manual. See [rollout](RETENTION-ROLLOUT.md) |
| Google login | Retain if compatible with AppGallery acceptance; production configuration and physical-device checks remain pending |
| Preparation language | English discussion and internal documents; retain English, Simplified Chinese and Bahasa Melayu for player-facing content |

Policy acceptance and retention rules remain needed for the final policy. A
custom paid domain is not inherently needed for a public HTTPS policy page.
The owner separately authorized website publication on 26 October; see the
[deployment record](../../Website/DEPLOYMENT.md). The supplied email is the intended public contact, not a verified
mailbox-deliverability result.

The owner selected the same features for everyone, with no separate child mode
and no launch deadline. The welcome Info panel now opens Terms, Privacy and
Website actions. The subsequent login update configured the live HTTPS base
URL; all three actions open the localized website, whose policies remain drafts.
Bundled offline drafts remain available as fallback. See the [website workflow](../../Website/README.md)
and [retention/operations proposal](OPERATIONS-PROPOSAL.md).

## Preparation priorities after owner decisions

1. Use LIEW KAIY BIN and the confirmed contact/operator details in the policy and
   store drafts. Prepare player-facing materials in all three supported languages
   before delivery.
2. Review the all-ages audience against text/voice chat, profile photos, discovery,
   invitations, reporting and blocking. Record what child/guardian controls,
   consent and operator procedures are needed for the intended market and SDKs.
   Do not claim an official all-ages rating or silently impose a 13+ restriction.
3. Retain Google as a conditional release option. Huawei documents Google as an
   authentication provider and notes its Android Google-SDK dependency on GMS;
   this is not approval of this game's AppGallery submission. The current bridge
   checks GMS availability. Verify Huawei/guest access without GMS and Google
   success/cancel/recovery where supported, using the final signing certificate.
4. Prepare retention, deletion-support, signing, device-test and backend rollout
   procedures locally. Website hosting was separately authorized and completed;
   further backend/console changes and app submission remain separate work.

References: [Huawei authentication providers](https://developer.huawei.com/consumer/en/doc/AppGallery-connect-Guides/agc-auth-introduction-0000001053732605)
and [Vivox privacy, including children's privacy](https://docs.unity.com/en-us/vivox/privacy/privacy-overview).
Vivox requires verified parental consent before submitting children's data where
applicable law requires it; the project's controls and provider settings still
need review. No child-safety implementation is certified by this document update.

## Verified foundation

- App: WHATTHE FISH?; package `com.umpsa.whatthefish`; Huawei app ID `119406947`.
- Huawei Account Kit and Scan Kit integration, backend identity verification,
  room QR generation, camera scanning and saved-image scanning exist.
- Guest play and non-GMS fallback are documented in [player login](../PLAYER-LOGIN.md).
- Unity Authentication/Multiplayer/Vivox and Cloudflare Workers/D1 are used.
- Scan Kit registration records are in [Huawei scanning](../HUAWEI-SCAN-KIT.md).
- AndroidSubmission builds a non-development IL2CPP APK and applies the project's
  strict 1,000,000,000-byte ceiling. This ceiling is a project requirement, not
  a claim about AppGallery's current package limit.

## Release blockers

1. **Signing continuity:** the current APK is production-signed and its public
   certificate is registered with Huawei. Preserve the owner's private backup;
   retained Google OAuth and same-key update acceptance still need checks.
2. **Account deletion:** the shared-match roster/receipt scrub is deployed and
   locally tested. Native linked-account deletion, retry/recovery and complete
   authenticated service acceptance remain unverified; see [rollout](RETENTION-ROLLOUT.md).
3. **Privacy and minors:** links are implemented in settings/account choices. Public
   contact and publisher name are supplied; public draft URL now exists. Final
   policy acceptance and retention activation remain pending. Huawei 8.12–8.13
   require a children's policy and parent permission. Select an adequate method
   after the [provider clarification](PROVIDER-QUESTIONS.md); KWS is optional.
4. **Device acceptance:** real Huawei login/link/recovery and no-GMS behavior,
   camera, microphone/Bluetooth, touch and performance remain to be verified.
5. **Service readiness:** retain Google conditionally per the owner. OAuth is
   recorded as Testing; check production configuration, final signing fingerprints,
   service quotas and provider behavior. The owner handles support/moderation;
   response procedures and the all-ages safety review remain pending.
6. **Current release:** the source-matched, production-signed APK is
   **172,597,919 bytes**, attached to the saved Huawei draft. Earlier debug-signed
   and 160,144,610-byte preparation packages are historical. See
   [current size evidence](../BUILD-SIZE.md) and the pre-submission status;
   compilation/source matching does not establish phone acceptance.
7. **Regression acceptance:** historical preparation notes recorded legacy cart
   test failures. Reconcile them with current tests and player impact before
   claiming release acceptance; do not assume either a fix or a current failure.

## Preparation contents

- [Store listing and reviewer draft](STORE-LISTING.md)
- [Privacy facts and policy draft](PRIVACY-DRAFT.md)
- [Signing and device acceptance plan](RELEASE-CHECKLIST.md)
- [Implemented technical workflow and activation steps](TECHNICAL-HANDOFF.md)
- [Website and Info panel](../../Website/README.md)
- [Public website deployment](../../Website/DEPLOYMENT.md)
- [Daily operations and proposed retention](OPERATIONS-PROPOSAL.md)
- [Website and Info validation](WEBSITE-INFO-VALIDATION.md)
- [Current pre-submission status](PRE-SUBMISSION-STATUS.md)
- [Minimum child-permission requirements](MINIMUM-REQUIREMENTS.md)
- [Submitted provider questions and ticket references](PROVIDER-QUESTIONS.md)
- [Asset-source and packaged-notice review](ASSET-SOURCE-REVIEW.md)
- [Real Huawei P40 checks and remaining device limits](HUAWEI-CLOUD-DEVICE-2026-10-29.md)

The initial 25 October preparation did not upload or submit the app, change backend services,
delete a live player account or create a production signing key. The subsequent
website-only deployment is recorded separately above. Isolated offline guest data
was deleted by a test. A disposable signing-test key was created outside the
repository, used only on a fixture APK, then removed; it is not a release key.
See [validation and delivery evidence](VALIDATION.md) for the tested scope.

## Official references

- [Huawei distribution overview](https://developer.huawei.com/consumer/en/appgallery):
  store information, distribution choices and privacy-policy URL.
- [Huawei HMS preparation](https://developer.huawei.com/consumer/en/codelab/HMSPreparation/):
  app registration and signing fingerprint configuration.
- [Android release preparation](https://developer.android.com/studio/publish/preparing):
  release configuration and signed packages.
- [Unity account deletion](https://docs.unity.com/en-us/authentication/delete-accounts):
  deleting Authentication does not substitute for deleting associated service data.

Verify region-specific requirements and current console fields at submission.
Android APK distribution does not imply native HarmonyOS NEXT support.
