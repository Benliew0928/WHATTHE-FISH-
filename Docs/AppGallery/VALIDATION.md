# AppGallery preparation validation — 25 October 2026

This records local preparation, not store acceptance or production activation.

## Final integrated delivery

A newer shared-workspace build completed while the isolated build was being
checked. It includes all seven account/privacy source files: their compiled
document hashes match current source for both Android and Windows. The complete
newer WindowsFinal was preserved, and the privacy tests were rerun on that actual
player: **26 assertions passed**, with final captures reviewed. Evidence:
`Builds/AppGalleryPrep/Privacy-final-1600x900/`, `Privacy-final-1200x900/` and
`final-feature-source-verification.json`.

Official integrated AndroidSubmission: **115,954,062 -> 160,144,610 bytes**
(+44,190,548), leaving **839,855,390 bytes** below the project ceiling. SHA-256:
`7583A8C83C939255171EBE73C872D4B918D64C836B47761ABE5C7D9539CFF905`.
Signature inspection confirms ARM64, minimum API 26, target API 36, a
non-debuggable manifest and **Android Debug signing**. Production signing remains
pending. Development privacy/event/award probes are absent from the Android
compiled-source list. The independent signing inspection is in
`final-signing-inspection.json`.

Integrated build evidence: `Builds/WelcomePanelsQA/apk-verification.json`,
`android-source-comparison.json`, `pre-delivery-source-comparison.json`, and
`Builds/SizeAudit/latest/`. ZIP CRC, 1,260 audit entries and all five scene bundles
were checked there. Largest payloads include islands (45,378,025 bytes), IL2CPP
(13,164,466 compressed bytes), Vivox (10,240,163) and Unity (9,808,754).
Five localization font payloads total 33,127,106 compressed bytes. The growth
also includes concurrent localization/voice work; it is not attributed solely
to account deletion. Current size documents already record this integrated build.

## Completed checks

- 48 Node backend tests passed, including deletion ownership/confirmation,
  idempotency, stale-token recreation prevention, transactional failure and
  cascading profile/invitation cleanup. Worker dry-run bundling passed.
- Cloudflare CLI reports not authenticated. No production migration or deployment
  was attempted. The client checks server capability before initiating deletion;
  the existing backend therefore fails closed instead of partially deleting data.
- Windows privacy review: 13 assertions at each of 1600x900 and 1200x900,
  **26 passing assertions**. Settings/account access, disabled absent policy URL,
  typed confirmation, empty-input protection, cancellation and isolated offline
  guest cleanup passed. Overview and confirmation images were visually reviewed.
- A disposable fixture key verified the external signing workflow, exact public
  certificate matching, APK alignment/signature checks and preservation of the
  original APK. The key was removed; fixture APK copies were archived in
  `Legacy/20261025-143121-309-appgallery-signing-fixture`.
- Read-only inspection was also invoked from an unrelated working directory;
  refusal of a key location inside the repository was checked.

Evidence is ignored locally under `Builds/AppGalleryPrep/`: backend logs,
`signing-inspection.json`, `disposable-signing-test.json`,
`Privacy-1600x900-verified/`, `Privacy-1200x900-verified/`,
`windows-snapshot.log` and `windows-delivery-hashes.json`.

## Reproducibility and limits

Two shared-workspace Windows attempts did not produce the delivery: the first
found a malformed new metadata GUID (fixed), and the second encountered a
concurrent localization type-layout change. The successful player uses a frozen
copy recorded in `snapshot-manifest.json`, under `Snapshot/`, based on `dc0be40`
plus then-current working files. Later localization work is not included or
certified. Generated-only changes from the failed shared build were compared
semantically with the pre-task copies and restored; unrelated work was retained.

The first privacy test had an overlong test authentication-profile name. Its
fixture was shortened; both clean reruns passed. This was not a real-account
deletion attempt. Failed-run logs remain separate from the passing evidence.

The complete old Windows player was archived in
`Legacy/20261025-144302-774-appgallery-windows`. Its replacement was copied and
hash-verified in full, not by replacing only the executable.
The older isolated candidate and failed Android intermediate output were then
archived in `Legacy/20261025-150226-419-appgallery-superseded-candidate`; logs,
source snapshot and current evidence remain under ignored Builds.

Both isolated official AndroidSubmission attempts compiled game code but failed
in Gradle's Java AF_UNIX loopback setup. A standalone Gradle help check passed
using JDK's TCP fallback; Unity's subsequent build still failed. Those attempts
are not counted as successful APK validation. The successful newer integrated
build above supersedes them. Its source hashes were checked rather than assuming
an unrelated APK represented these changes.

Real linked-provider deletion, interruption recovery on native accounts, backend
activation, public privacy-policy hosting, production signing/fingerprint
registration and physical Huawei testing remain pending. A lost Authentication
deletion response can require operator assistance; see the technical handoff.
Existing legacy Golf cart-test failures have not been reclassified as passing.
No Git staging, commit or push was performed.
