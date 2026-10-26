# AppGallery release preparation

Technical preparation was first validated on 25 October 2026 against source
commit `dc0be40`; see [historical validation](VALIDATION.md) for its exact scope.
Owner decisions were updated on 26 October 2026. This remains preparation only,
not evidence of store acceptance, production activation or a production-signed
release. Current build measurements are in [build size](../BUILD-SIZE.md).

## Owner decisions and remaining information

| Item | Status |
| --- | --- |
| Publisher | LIEW KAIY BIN; personal/individual publisher, name supplied by owner to match the Huawei developer account |
| Public support and privacy contact | benliew28262826@gmail.com, supplied by owner |
| Launch countries and intended audience | Malaysia; all ages, including children, as the owner's intended audience. Replaces the earlier 13+ proposal; official content rating and suitability remain unverified |
| Free/paid, advertising and purchases | Free download, no ads, no real-money in-app purchases confirmed |
| Public privacy-policy host/domain | Cloudflare Pages development preview published at <https://whatthefish-game.pages.dev/>; three languages, game Terms/Privacy remain drafts; no custom domain required |
| Verified Huawei developer account | Owner reports verification completed; intended app ownership and final signing configuration still need checking before release |
| Existing production keystore and backup | None; helper prepared for private local creation later |
| Huawei Android device model and availability | No Huawei test phone available; device checks remain pending |
| Support, reports and deletion exceptions | Owner handles these and selected daily review; procedure drafted, no guaranteed resolution deadline |
| Data retention periods | Proposed in the operations document; not yet approved or implemented |
| Google login | Retain if compatible with AppGallery acceptance; production configuration and physical-device checks remain pending |
| Preparation language | English discussion and internal documents; retain English, Simplified Chinese and Bahasa Melayu for player-facing content |

Policy acceptance and retention rules remain needed for the final policy. A
custom paid domain is not inherently needed for a public HTTPS policy page.
The owner separately authorized website publication on 26 October; see the
[deployment record](../../Website/DEPLOYMENT.md). The supplied email is the intended public contact, not a verified
mailbox-deliverability result.

The owner selected the same features for everyone, with no separate child mode
and no launch deadline. The welcome Info panel now opens Terms, Privacy and
Website actions. Terms/Privacy have bundled offline drafts; Website is disabled
until a real HTTPS base URL is configured. See the [website workflow](../../Website/README.md)
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

1. **Production signing:** helper implemented outside Unity to avoid initialization
   resetting custom signing. Private owner key creation and final signing remain
   pending. See the technical handoff; do not commit keys or passwords.
2. **Account deletion:** the additive backend was activated with the account coin
   release on 26 October; see [deployment evidence](../COIN-ECONOMY.md). Native
   linked-account verification and complete device deletion still require a phone.
   Offline review also found participant IDs retained inside another host's match
   roster/receipt JSON; correct and test this before complete-deletion acceptance.
3. **Privacy entry point:** implemented in settings and account choices. Public
   contact and publisher name are supplied; public draft URL now exists. Final
   policy acceptance, in-game URL configuration and retention remain pending.
4. **Device acceptance:** real Huawei login/link/recovery and no-GMS behavior,
   camera, microphone/Bluetooth, touch and performance remain to be verified.
5. **Service readiness:** retain Google conditionally per the owner. OAuth is
   recorded as Testing; check production configuration, final signing fingerprints,
   service quotas and provider behavior. The owner handles support/moderation;
   response procedures and the all-ages safety review remain pending.
6. **Current release:** use [current size evidence](../BUILD-SIZE.md). The
   160,144,610-byte APK in [validation](VALIDATION.md) is historical. Inspection
   on 26 October found the 166,897,340-byte APK non-debuggable but signed with
   Android Debug; it is not production-signed or store-certified.
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

This preparation did not upload or submit the app, change backend services,
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
