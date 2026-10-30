# Minimum requirements for the Malaysia 12+ online release

**Submission update, 29 October:** the owner requested submitting the current
build without email approval. Huawei accepted the submission and shows
**Reviewing**, with the missing controls explicitly disclosed to the reviewer.
[Submission evidence](SUBMITTED-2026-10-29.md) does not establish compliance or
resolve the requirements and implementation gaps described below.

Reviewed 28–29 October 2026. The owner wants to retain younger players' online
access. KWS and an adults-only online restriction have **not** been selected.
This document separates verified platform requirements from implementation
proposals; it is not a legal opinion or AppGallery approval.

## Verified requirements and the remaining gap

| Source | What it establishes | Current game / action |
| --- | --- | --- |
| Huawei [Minor Protection](https://developer.huawei.com/consumer/en/doc/app/50104-08), 8.12–8.13; live page updated 28 September 2026 | A dedicated children's privacy policy when children's personal data is processed; parental/legal-guardian permission before minors' personal-data processing, with explicit consent for sensitive data/permissions | No recorded guardian-permission flow was found. The public policy is a draft. Resolve the acceptable permission method and publish the matching notice. The clauses do not name KWS or prescribe one verification vendor. |
| Huawei [User Privacy](https://developer.huawei.com/consumer/en/doc/app/50104-07), 7.1–7.9, 7.12, 7.16–7.19; live page updated 14 January 2026 | Accessible and accurate disclosures, a lawful basis, advance notice/consent where required, withdrawal/deletion, minimization and purpose-specific permissions | Existing links and deletion UI help, but do not prove consent or complete third-party disclosures. Delay restricted processing until permission is effective; preserve unrelated/offline functionality when permission is refused. |
| Huawei Minor Protection, 8.9 | Accessible reporting channels and timely handling | Reporting/blocking/mute and support email exist. Owner selected daily review. Operator response and deletion escalation still need acceptance testing. |
| Unity [Terms of Service](https://unity.com/legal/terms-of-service), 8.6; version reviewed 30 June 2026 | Child-data obligations depend on the project, triggered laws and provider terms. Appropriate dashboard designation/signals and authority to transmit child data may be needed; compliant verified parental consent is one route among the stated bases | Confirm the actual Authentication, Multiplayer and Vivox arrangement. Do not assume a Huawei account holder's permission covers this game's separate processing. |
| Unity [Authentication privacy](https://docs.unity.com/en-us/authentication/privacy-and-consent/overview) and [Vivox privacy](https://docs.unity.com/en-us/vivox/privacy/privacy-overview) | Identity/device/network data may be processed even when a player does not type a real name. Child-consent requirements and developer responsibilities apply as described by the services | An online guest is not data-free. Existing source can initialize Authentication before any age/guardian state. An Android microphone prompt is not proof of guardian consent. |
| Malaysian authority's [PDPA scope](https://www.pdp.gov.my/ppdpv1/en/akta/application-and-non-application-of-the-act/) and [2013 Regulations](https://www.pdp.gov.my/ppdpv1/wp-content/uploads/2024/07/Peraturan-peraturan-Perlindungan-Data-Peribadi.pdf), regulation 3 | PDPA scope and consent rules must be assessed together; under-18 guardian consent is specified where those rules apply | Free distribution and individual publication alone do not establish an exemption. Huawei's separate platform requirements still need to be met. |

The approved 12+ rating remains the content classification. It does not itself
record permission for this game's accounts, profiles, photos, chat or voice.
There is no established requirement here to buy KWS, collect identity documents
ourselves, or prohibit every under-18 player from online play.

## Smallest implementation to investigate

First ask Huawei and Unity whether a self-managed, recorded parent-permission
process is adequate for this exact game. A candidate is a private parent email
approval link plus an explicit guardian attestation and choices for the game's
online/social features. Email ownership or an attestation alone is **not** a
claim of verified parental consent; provider acceptance must be established.
Use a verification provider only if the applicable requirements need stronger
assurance or the owner chooses it.

Whichever adequate method is selected, the implementation needs these properties:

1. A minimal age/permission decision before restricted online SDK initialization
   or identity/profile creation; offline sports available while undecided.
2. Clear English, Simplified Chinese and Malay notices describing recipients,
   purposes, feature choices and withdrawal. Avoid unnecessary full birth dates
   or identity documents in the game's database.
3. Private, versioned permission records and enforceable server entitlements;
   forged requests, stale sessions or restored data must not bypass withdrawal.
4. Withdrawal and account/data deletion that work across the actual providers;
   voice, photos and other restricted features stop when permission is revoked.
5. Fresh-install/refusal/approval/revocation/account-switch/API-bypass tests,
   three-language layout review and new signed Android/complete Windows builds.

This is a design boundary, not an implemented or approved consent flow. Do not
add a parent checkbox and represent it as verified consent. Prepared provider
questions and delivery confirmations are in [the support record](PROVIDER-QUESTIONS.md):
Huawei **D650176** and Unity **#3623478** were submitted with owner authorization
on 29 October. Provider answers remain pending. No KWS account/license was added.

## Independent preparation completed

The [retention/deletion backend](RETENTION-ROLLOUT.md) is deployed with daily
cleanup enabled for 03:17 Malaysia time; its first run is unverified. The 85-test suite and Worker bundling pass; production health
and unauthenticated rejection pass. No game code, signed APK or store submission
was changed by this requirements review. Phone/Huawei service acceptance,
effective final policy, provider settings, first scheduled cleanup verification and remaining
store declarations are still release checks.
