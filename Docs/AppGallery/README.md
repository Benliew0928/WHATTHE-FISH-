# AppGallery release preparation

Prepared 25 October 2026 against source commit `dc0be40`. This is a preparation
pack, not evidence of store acceptance or a production-signed release.
Existing concurrent welcome/menu edits are outside this preparation.

## Owner information still required

| Item | Status |
| --- | --- |
| Legal publisher/team name and public support email | Awaiting owner |
| Launch countries, intended audience and minimum age | Malaysia confirmed; 13+ product audience proposed, not yet confirmed or an official store rating |
| Free/paid, advertising and purchases | Free download, no ads, no in-app purchases confirmed |
| Public privacy-policy host/domain | Owner has no website/domain; prepare a static page, choose hosting later |
| Verified Huawei developer account | Owner is unsure; historical app registration exists but verification must be checked in console |
| Existing production keystore and backup | None; helper prepared for private local creation later |
| Huawei Android device model and availability | No Huawei test phone available; device checks remain pending |
| Data retention periods and person handling support/reports | Awaiting owner |

Publisher identity and support email may be completed later. They block final
policy publication and store submission, not source inspection, signing workflow
preparation or deletion implementation. A custom paid domain is not inherently
needed for a public HTTPS policy page; hosting choice and URL remain pending.

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
2. **Account deletion:** client and additive backend implementation are prepared.
   Activation is pending Cloudflare authentication/deployment; no live users have
   been deleted. Native linked-account verification still requires a phone.
3. **Privacy entry point:** implemented in settings and account choices. Public
   policy URL/contact/retention remain pending; no placeholder link is advertised.
4. **Device acceptance:** real Huawei login/link/recovery and no-GMS behavior,
   camera, microphone/Bluetooth, touch and performance remain to be verified.
5. **Service readiness:** Google OAuth is recorded as Testing; decide whether to
   complete production setup or disable its public release entry. Confirm provider
   production signatures, service quotas, support and moderation responsibilities.
6. **Current release:** the newer integrated AndroidSubmission APK is
   160,144,610 bytes and contains this preparation's source. It is non-debuggable
   but still signed with Android Debug, so it is not production-signed. See
   [validation](VALIDATION.md) and [size evidence](../BUILD-SIZE.md).
7. **Known regressions:** latest build notes record legacy cart checks failing.
   Triage and verify real player impact before claiming release acceptance.

## Preparation contents

- [Store listing and reviewer draft](STORE-LISTING.md)
- [Privacy facts and policy draft](PRIVACY-DRAFT.md)
- [Signing and device acceptance plan](RELEASE-CHECKLIST.md)
- [Implemented technical workflow and activation steps](TECHNICAL-HANDOFF.md)

No app upload, submission, service deployment, live cloud-account deletion or
production signing-key creation has been performed. Isolated offline guest data
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
