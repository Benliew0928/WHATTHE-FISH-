# AppGallery signing and acceptance checklist

All unchecked items are pending, not implied to pass by successful compilation.

## Owner decisions recorded on 26 October 2026

- [x] Personal/individual publisher: LIEW KAIY BIN, supplied by owner.
- [x] Public support/privacy email: benliew28262826@gmail.com.
- [x] Owner will handle support, reports and deletion exceptions.
- [x] Owner reports Huawei developer verification completed; the signed-in
      account and correct app/package were checked during release preparation.
- [x] Malaysia, free download, no advertising and no real-money in-app purchases confirmed.
- [x] All-ages intended audience selected; this is not an official rating.
- [x] Same features for everyone, daily owner review, no launch deadline.
- [x] Local three-language website and offline Terms/Privacy drafts prepared.
- [x] Owner separately authorized website publication; Cloudflare Pages development
      preview is live. See [deployment evidence](../../Website/DEPLOYMENT.md).
- [x] Welcome Info replaces the previous content with Terms, Privacy and Website.
- [x] Retain Google login conditionally on AppGallery acceptance and compatibility.
- [ ] Complete the children's privacy, communication and guardian-control review.
- [x] Submit owner-approved permission/configuration questions to Huawei and Unity;
      [tickets D650176 / #3623478](PROVIDER-QUESTIONS.md), answers pending.
- [ ] Define support/report procedures, retention periods and deletion escalation.
- [x] Adopt four [retention periods](OPERATIONS-PROPOSAL.md); implement and test
      cleanup/deletion/controlled restore locally (85-test backend suite passes).
- [x] Deploy the disabled retention handler and verify both production indexes.
- [x] Activate the adopted retention schedule, daily at 03:17 Malaysia time.
- [ ] Inspect first scheduled run/monitoring and verify live restore handling.
- [x] Deploy the tested scrub of deleted participants in other hosts' roster/receipt JSON.
- [ ] Verify authenticated live deletion, retries and shared-reference removal.
- [x] Confirm intended app/account ownership and matching publisher details.

The owner subsequently authorized production signing, the confirmed pre-launch
account reset and Huawei draft/upload work on 28 October. The production
certificate is registered. The owner adopted retention periods, retained all
ages and explicitly kept submission pending without a test phone, then approved
the calculated Malaysia 12+ rating. The deletion fix and retention handler are
deployed; daily cleanup is enabled, with the first run unverified. Huawei rules 8.12–8.13
establish the policy/parental-permission gap without requiring KWS specifically;
see [minimum requirements](MINIMUM-REQUIREMENTS.md).

## Signing handoff

- [x] Owner confirmed there is no existing production keystore.
- [x] Owner created the key outside Git and confirmed private backup; owner is custodian.
- [x] Signing passwords entered privately, outside chat and command arguments.
- [x] Public release certificate fingerprints recorded in release evidence and owner record.
- [x] Register the final delivery fingerprint with Huawei; owner-approved and verified.
- [ ] Verify the retained Google Android OAuth client uses the final fingerprint.
      If store re-signing is used, distinguish the upload
      certificate from the installed application's certificate.
- [x] Apply signing after project initialization; refuse absent signing inputs.
- [x] Verify final APK signature, signer, package, version code, minimum/target
      SDK, ARM64 native libraries, permissions and non-debuggable manifest.
- [ ] Verify upgrade installation and account continuity from a same-key release.
      Development-to-release signature changes can require uninstalling a test
      app; warn testers about unrecoverable guest data before uninstalling.

## Release artifact

This section applies to the final production release. Preparation build/test
results are recorded separately in [validation](VALIDATION.md).

- [ ] Freeze/review the intended source snapshot and preserve concurrent work.
- [x] Build with Tools/Build/Build.ps1 -Target AndroidSubmission.
- [x] Record actual APK bytes, SHA-256, certificate and source revision, including dirty-source limits.
- [x] Inspect Builds/SizeAudit/latest and update build-size records.
- [ ] Confirm developer event shortcuts/probes cannot run in the release.
- [x] Keep complete validated WindowsFinal with all 541 compiled runtime sources matching.
- [x] Run Check-TaskReady.ps1 with the signed candidate: 8,924 publishable files,
      1,203 LFS paths; its size check does not replace source/device acceptance.

## Physical-device matrix

Record build hash, device/OS, network, result, steps and evidence for each row.

| Check | Required scenario | Result |
| --- | --- | --- |
| Startup | Fresh install, relaunch, existing same-key update | P40 install/launch/relaunch exercised; same-key update pending. [Record](HUAWEI-CLOUD-DEVICE-2026-10-29.md) |
| No GMS | Huawei Android phone without Google services; guest and Huawei usable | Pending |
| Huawei account | Success, cancel, link, conflict, session expiry, second-device recovery | P40 provider attempt returned to launcher; reopening showed cancellation. Successful authentication and full matrix pending |
| QR | Camera/image, denial, cancel, invalid/expired QR, manual fallback | Pending |
| Multiplayer | Host/join, four-island travel, each sport, reconnect/host loss | Pending |
| Voice | Two physical devices, denied permission, mute, headset/Bluetooth, background | Pending |
| Social safety | Name/photo reports, blocks, invitations, operator response | Pending |
| All-ages suitability | Validate the agreed child/guardian and communication controls; no assumed official rating | Pending |
| Account deletion | All service data, retry/partial failure, relaunch, no stale recreation | Pending |
| Golf | Cart on paved paths, events, completion/ranking, existing failed-suite triage | P40 offline round, aiming and one touch shot exercised; full matrix pending |
| Performance | Frame pacing, memory, thermals, battery and touch on target phones | Pending |
| Network | Slow/lost network, Wi-Fi/mobile change, service failure | Pending |
| Privacy | Public URL accessible, in-game link, disclosures match permissions/data | Pending |

## Console submission

- [x] Confirm verified individual account owns the intended app; complete category
      and official content rating. Malaysia is the confirmed initial region.
- [ ] Final listing/screenshots/support/privacy fields complete.
- [ ] Asset rights and licenses reviewed; no placeholder UI or sample personal data.
- [x] Verify four packaged notices and seven packed font licences; publish
      documented rod/club credits on all twelve website pages in three languages.
      [Source review](ASSET-SOURCE-REVIEW.md) retains the actual cart/recording
      provenance and the owner's decision to keep the current assets.
- [ ] Google production audience resolved if Google sign-in remains available.
- [ ] Reviewer can access all advertised core functions.
- [x] Owner authorized submission of the existing package and listing without
      email approval; the reviewer note discloses the actual missing controls.
- [x] Submit 0.3.0 (1) for Malaysia; console shows **Submitted / Reviewing**.
- [x] Confirm release setting **Immediately once approved**.

The [submission record](SUBMITTED-2026-10-29.md) confirms review submission;
unchecked acceptance items remain open. Do not claim official approval, physical
phone quality or native HarmonyOS support from a Windows or Unity build.
