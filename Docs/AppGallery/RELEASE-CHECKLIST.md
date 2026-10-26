# AppGallery signing and acceptance checklist

All unchecked items are pending, not implied to pass by successful compilation.

## Owner decisions recorded on 26 October 2026

- [x] Personal/individual publisher: LIEW KAIY BIN, supplied by owner.
- [x] Public support/privacy email: benliew28262826@gmail.com.
- [x] Owner will handle support, reports and deletion exceptions.
- [x] Owner reports Huawei developer verification completed; not independently
      reverified in the console by this task.
- [x] Malaysia, free download, no advertising and no real-money in-app purchases confirmed.
- [x] All-ages intended audience selected; this is not an official rating.
- [x] Same features for everyone, daily owner review, no launch deadline.
- [x] Local three-language website and offline Terms/Privacy drafts prepared.
- [x] Owner separately authorized website publication; Cloudflare Pages development
      preview is live. See [deployment evidence](../../Website/DEPLOYMENT.md).
- [x] Welcome Info replaces the previous content with Terms, Privacy and Website.
- [x] Retain Google login conditionally on AppGallery acceptance and compatibility.
- [ ] Complete the children's privacy, communication and guardian-control review.
- [ ] Define support/report procedures, retention periods and deletion escalation.
- [ ] Review and adopt [proposed periods and daily procedure](OPERATIONS-PROPOSAL.md),
      then implement cleanup and verify restores/deletion against them.
- [ ] Scrub deleted participants from another host's stored match roster/receipt JSON.
- [ ] Confirm intended app/account ownership and matching publisher details.

The later website-publication request authorized the static preview deployment.
It does not authorize further backend changes, production signing or store submission.

## Signing handoff

- [x] Owner confirmed there is no existing production keystore.
- [ ] If creating a key, owner chooses its custodian and secure backup location.
- [ ] Passwords entered privately; no secrets in chat, command arguments or Git.
- [ ] Record only public release certificate fingerprints in release evidence.
- [ ] Register the final delivery fingerprint with Huawei and any retained Google
      Android OAuth client. If store re-signing is used, distinguish the upload
      certificate from the installed application's certificate.
- [ ] Apply signing after project initialization; refuse absent signing inputs.
- [ ] Verify final APK signature, signer, package, version code, minimum/target
      SDK, ARM64 native libraries, permissions and non-debuggable manifest.
- [ ] Verify upgrade installation and account continuity from a same-key release.
      Development-to-release signature changes can require uninstalling a test
      app; warn testers about unrecoverable guest data before uninstalling.

## Release artifact

This section applies to the final production release. Preparation build/test
results are recorded separately in [validation](VALIDATION.md).

- [ ] Freeze/review the intended source snapshot and preserve concurrent work.
- [ ] Build with Tools/Build/Build.ps1 -Target AndroidSubmission.
- [ ] Record actual APK bytes, SHA-256, certificate and source revision.
- [ ] Inspect Builds/SizeAudit/latest and update build-size records.
- [ ] Confirm developer event shortcuts/probes cannot run in the release.
- [ ] Keep complete validated WindowsFinal when delivering gameplay changes.
- [ ] Run Check-TaskReady.ps1; its size check does not establish APK freshness.

## Physical-device matrix

Record build hash, device/OS, network, result, steps and evidence for each row.

| Check | Required scenario | Result |
| --- | --- | --- |
| Startup | Fresh install, relaunch, existing same-key update | Pending |
| No GMS | Huawei Android phone without Google services; guest and Huawei usable | Pending |
| Huawei account | Success, cancel, link, conflict, session expiry, second-device recovery | Pending |
| QR | Camera/image, denial, cancel, invalid/expired QR, manual fallback | Pending |
| Multiplayer | Host/join, four-island travel, each sport, reconnect/host loss | Pending |
| Voice | Two physical devices, denied permission, mute, headset/Bluetooth, background | Pending |
| Social safety | Name/photo reports, blocks, invitations, operator response | Pending |
| All-ages suitability | Validate the agreed child/guardian and communication controls; no assumed official rating | Pending |
| Account deletion | All service data, retry/partial failure, relaunch, no stale recreation | Pending |
| Golf | Cart on paved paths, events, completion/ranking, existing failed-suite triage | Pending |
| Performance | Frame pacing, memory, thermals, battery and touch on target phones | Pending |
| Network | Slow/lost network, Wi-Fi/mobile change, service failure | Pending |
| Privacy | Public URL accessible, in-game link, disclosures match permissions/data | Pending |

## Console submission

- [ ] Confirm verified individual account owns the intended app; complete category
      and official content rating. Malaysia is the confirmed initial region.
- [ ] Final listing/screenshots/support/privacy fields complete.
- [ ] Asset rights and licenses reviewed; no placeholder UI or sample personal data.
- [ ] Google production audience resolved if Google sign-in remains available.
- [ ] Reviewer can access all advertised core functions.
- [ ] Owner reviews final package and listing before actual submission.

This checklist is preparation only. Do not claim official approval, physical
phone quality or native HarmonyOS support from a Windows or Unity build.
