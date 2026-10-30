# Submission preparation update — 28 October 2026

The Huawei draft now contains the production-signed `0.3.0 (1)` APK, the owner's
approved calculated **12+ Malaysia rating**, and **nine saved Android screenshots**
(three per language). App information shows its green completion message.
**The app remains Draft / Unsubmitted.** Final policy, child-access controls,
provider configuration and retention activation remain release work.

## Current Huawei state

- Version `0.3.0 (1)` and the production-signed APK remain attached to the draft.
- Huawei accepted and saved **3/8 screenshots in each of English (US), Chinese
  (PRC) and Malay**. Each language save returned **Data saved successfully**.
  The matching 512px icon loads and localized listing text remains saved. The
  previous incomplete-app-information warning is absent on the reopened version.
- The content questionnaire was prepared with mild cartoon sports/tackle impacts,
  childlike art, no blood, player communication and personal-data collection.
  Location sharing, real-money purchases and unrestricted browsing were answered
  No. Other game-authored adult/criminal content was answered No.
- The owner approved the calculated **12+ for Other Regions**, including
  Malaysia. After renewed sign-in, the questionnaire was submitted and version
  **Save Draft** returned **Data saved successfully**. The reopened version shows
  **Other Regions / 12+ / Rated 12+**. This is a saved content classification,
  not AppGallery review approval or evidence of consent controls.
- Privacy tags are partially prepared and saved. App functionality has 12 items:
  friends/contact list, user identifiers, other device identifiers, account
  information, support email addresses, images, audio, text, search keywords,
  game statistics, customer service records and IP address. Personalization has
  user identifiers, account information, images and game statistics. Huawei
  confirmed **Data saved successfully**. These are provisional declarations;
  vendor technical data, third-party disclosure and international transfers
  still require the final processing review. Unselected scenarios are not a
  completed finding that those activities never occur.
- Public privacy/terms still identify themselves as drafts. The deletion fix and
  retention handler are now deployed with both indexes verified. Daily cleanup
  is configured for 03:17 Malaysia time; the first run is unverified. Guardian
  permission remains unresolved. Do not claim a consent flow or verified first purge in a release declaration or
  merely remove the draft notice.

The [child-protection review](CHILD-PROTECTION-REVIEW.md#verified-huawei-requirements-after-the-approved-12-rating)
corrects the earlier KWS/adults-only framing. Neither option is selected. Live
Huawei rules 8.12–8.13 establish the children's-policy and parental-permission
requirements without naming KWS. The acceptable permission method and Unity
configuration still need confirmation. See [minimum requirements](MINIMUM-REQUIREMENTS.md)
and the [submitted provider questions](PROVIDER-QUESTIONS.md) (Huawei D650176,
Unity #3623478; answers pending). No KWS account/license/credential
or game behavior change was created by this review.

The public privacy and support pages returned HTTPS **200** from this PC and
still contain their draft notice and publisher identity. This does not establish
acceptance by Huawei's separate URL checker. Evidence is in
`Builds/AppGallerySubmissionQA/public-page-check.json`.

The email/customer-service entries cover voluntary support correspondence,
not an email scope requested from Huawei/Google sign-in. The contact-list entry
covers in-game friends; it does not claim access to the phone address book.
The device identifier/IP entries account for provider processing, including
[Vivox's documented metadata](https://docs.unity.com/en-us/vivox/privacy/privacy-overview).
The broad Huawei example descriptions are supplied by the console, rather than
claims that the game collects every example in each category. No new custom
player-facing wording or game binary was introduced in this update.

## Android screenshot preparation

No physical Android device is connected. Existing emulator system images are
installed, including Android 16 with ARM64 translation. An initial read-only
user AVD had insufficient free internal space; its apps/data were preserved.
A fresh minimal test AVD was created under `Builds/AppGallerySubmissionQA/Avd/`
using the existing SDK images, without copying the user AVD or project.

The exact production APK installed successfully. Package inspection reported
`com.umpsa.whatthefish`, `0.3.0`, code `1`, target SDK `36`, and ARM64. Android
accepted a cold activity start. Native window interaction and subsequent shell
readback became unresponsive, so the test emulator was stopped. This does not
establish successful gameplay, phone compatibility or performance.

The owner explicitly authorized ADB-based interaction/capture for this temporary
device on 28 October. Software-GPU attempts exited; Windows recorded host QEMU
access violations (`0xc0000005`). Host GPU with Vulkan disabled allowed the same
device and exact signed APK to run interactively. A local guest named `Player`
was used for the welcome, sports menu and offline lagoon. Provider sign-in and
online-room controls were not invoked.

Nine unedited ADB screenshots are uploaded and saved: welcome, sports menu and lagoon in each
of English, Simplified Chinese and Bahasa Melayu. All are **1280 × 720 PNGs**,
below **5 MB** each, totaling **8,020,266 bytes**. Each was visually inspected;
labels remain in bounds and artwork/button treatment is preserved across these
views. The player-entered name remains unchanged. Source package hash, image
hashes, dimensions and upload verification are in
`Builds/AppGallerySubmissionQA/store-screenshot-manifest.json`. These are actual
Android captures, without stock images, compositing or Windows renders. They
are not evidence of full gameplay/device acceptance.

The test emulator was stopped. Removal of its completed task-only output was
attempted with containment, ignored-output, no-link and process guards, but
automatic approval review rejected the command before execution, stating
**blocked by policy**. No alternative deletion was attempted. The device remains:
**7,146,240,956 logical bytes across 28 files**. Exact scope and rejection are in
`Builds/AppGallerySubmissionQA/generated-output-cleanup.json` and
`retained-test-device.json`. Original AVDs and installed SDK images are preserved.

Evidence: `Builds/AppGallerySubmissionQA/`, including the rating preview,
installation output, device status and emulator session record. Reuse this one
candidate device for subsequent attempts; remove completed reproducible device
output under the generated-output rules after retaining compact final evidence.

Saved Huawei state and browser proof are `huawei-draft-state.json`,
`rating-saved.png` and `screenshots-saved-{en,zh-Hans,ms}.png`. No final review
Submit action was taken while the owner's child-protection requirement is open.

Repository readiness checks passed against the unchanged signed candidate:
8,921 publishable files, 1,203 LFS paths and 7,718 text files. The package remains
172,597,919 bytes. No staging, commit, push or review submission was performed.
