# AppGallery signing and acceptance checklist

All unchecked items are pending, not implied to pass by successful compilation.

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
| Account deletion | All service data, retry/partial failure, relaunch, no stale recreation | Pending |
| Golf | Cart on paved paths, events, completion/ranking, existing failed-suite triage | Pending |
| Performance | Frame pacing, memory, thermals, battery and touch on target phones | Pending |
| Network | Slow/lost network, Wi-Fi/mobile change, service failure | Pending |
| Privacy | Public URL accessible, in-game link, disclosures match permissions/data | Pending |

## Console submission

- [ ] Publisher verification, regions, category and content rating confirmed.
- [ ] Final listing/screenshots/support/privacy fields complete.
- [ ] Asset rights and licenses reviewed; no placeholder UI or sample personal data.
- [ ] Google production audience resolved if Google sign-in remains available.
- [ ] Reviewer can access all advertised core functions.
- [ ] Owner reviews final package and listing before actual submission.

This checklist is preparation only. Do not claim official approval, physical
phone quality or native HarmonyOS support from a Windows or Unity build.
