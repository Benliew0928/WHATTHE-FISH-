# Huawei P40 device check — 29 October 2026

The existing production APK was exercised on a real remote phone through
Huawei AppGallery Connect Cloud Debugging. This extends the earlier Android
emulator checks; it does not complete the release acceptance matrix.

| Item | Observed result |
| --- | --- |
| Device | Huawei P40, ANA-NX9, EMUI 10.1.0, API 29 / Android 10, cloud Wi-Fi |
| Package | `com.umpsa.whatthefish`, 0.3.0 (1), 172,597,919 bytes |
| APK SHA-256 | `48F52571E7F92602FBCC03E423B4D8CEB2087F53EDCA4E0E728AB83D29C47146` |
| Installation/startup | Cloud portal showed Installed; launcher opened the game. Relaunch returned to the game. |
| Welcome localization | English, Simplified Chinese and Bahasa Melayu reviewed on the physical device; heading, provider/guest buttons and top controls fit. |
| Guest entry | A disposable local guest named `testp40` entered the game. The Malay name form, first-visit guide, sports menu and Golf selection fit. No developer/provider credentials were entered on the shared phone. |
| Golf | Offline scene and Malay guide loaded. Starting a round displayed standings; Aim changed to the trajectory view; dragging/releasing the power control produced a shot. The displayed stroke count became 1 and distance changed from 66 m to 64 m. |
| Huawei sign-in | Selecting Huawei returned the device to its launcher. Reopening the app resumed the account panel with a localized cancellation message. No successful provider login was established. The sampled AndroidRuntime filter showed no fatal entry; this is insufficient to diagnose the transition as either a crash or a provider/service limitation. |
| Input limitation | The remote phone's SwiftKey onboarding/account offer and floating navigation dock interfered with some taps. Optional keyboard sign-in was cancelled; no new account or agreement was accepted. Joystick attempts do not establish sustained movement or simultaneous touch acceptance. |

The welcome screen showed Huawei and guest entry; the account panel also
contained a Google option. Google/HMS service availability, provider linking,
conflicts, session expiry and account recovery remain unverified. Do not infer
absence of GMS from the visible welcome buttons alone.

QR scanning, camera/microphone permissions, two-device multiplayer and voice,
account deletion, all four sports, full round completion, upgrade continuity,
audio quality, frame pacing, memory, thermals and battery remain open. A streamed
view and a single shot do not establish phone performance or full touch quality.

An earlier P40 Pro allocation was released by the cloud service before useful
game testing. No test result is attributed to that model. The successful P40
session was explicitly released after the checks. No paid reservation, cloud
shell or ADB operation was used. The uploaded APK remains available for future
authorized tests. The session's temporary guest was local to the cloud device;
this check did not intentionally create or reset a production game identity.

Compact evidence and the device-check record are under
`Builds/AppGallerySubmissionQA/`, with `huawei-real-p40-` image names. Existing
store screenshots remain the nine original Android-emulator captures; these
new browser-context device images were not substituted into the listing.
