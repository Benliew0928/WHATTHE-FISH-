# Player sign-in: guest, Huawei and Google

## Implemented — 20 October 2026

The Android game now has Huawei Account Kit and Google Credential Manager
sign-in, alongside guest play. Provider setup is configured in AppGallery,
Google and Unity; the Huawei verifier is deployed on Cloudflare Workers Free.
The Android app has **not been published**. Physical Android account success,
linking and recovery remain unverified because no test phone is available.

The subsequent profile implementation adds cloud names/pictures, room avatars,
world nameplates, profile cards and friend requests on Cloudflare D1 Free.
See [behavior, verification and service limits](PLAYER-PROFILES-PLAN.md).
It uses multiplayer protocol **73**; the original sign-in-only delivery used 72.

## Player experience

- Continue as guest works offline and preserves the existing name-entry flow.
  Names use 1–16 characters with at least one letter or number; duplicate names
  are allowed. A local GUID is not proof of online identity.
- The welcome screen and Your profile expose Account & sign-in. Huawei is
  preferred on Huawei devices; Google is preferred on other Android phones
  with available Google Play services. Google is unavailable without GMS.
  Huawei's native flow handles its service/account requirements. Guest remains
  available when a provider cannot be used.
- **Keep this profile** links a provider to the current anonymous/linked UGS
  player and preserves its ID and current local name. This is the default for
  upgrading a guest or creating the player's first linked game account.
- **Use an existing account** recovers a previously linked game account.
  It does not silently create a new account or merge two players. A provider
  already linked elsewhere produces a conflict message; choose recovery to
  switch explicitly.
- Cancellation of the native prompt preserves the current session. Failed
  account switching attempts restore the previous usable UGS session where
  available. Account changes are blocked while in a room or joining one.
- Sign-out clears the active UGS credentials and local active identity, then
  clears native provider credential state. It is not account deletion and does
  not sign the entire phone out of its Huawei or Google account.
- Windows supports guest play and the account-management UI with an Android-only
  explanation. It cannot exercise the native provider account dialogs.

UGS player ID is the canonical online identity. Online room entry restores the
cached UGS session, including linked accounts. If a saved linked profile has
lost its session, the game asks for sign-in instead of silently assigning a new
anonymous player. Recovered accounts load their cloud name and picture after
sign-in. A temporary service failure leaves their cached profile available.
Inactive account-name caches remain local and are not inherited by other players.
An unlinked guest can lose recovery access when app data or its session is lost.

## Configuration completed

| Service | Current setup |
| --- | --- |
| Huawei | App ID `119406947`, package `com.umpsa.whatthefish`, existing registered development SHA-256 fingerprint. Native Account Kit ID-token flow. |
| Google | Personal/team project `whatthe-fish`, external audience in Testing, developer account added as test user. Web and Android OAuth clients created. Billing disabled on this project. |
| Unity | Project `633e314c-46d1-4b48-bbbc-2dc5f6fb653c` (dashboard name SportsPrototype), environment `production`; Google and Custom ID enabled. |
| Backend | `whatthefish-huawei-auth` on Cloudflare Workers Free. Only project-scoped Player Authentication Token Issuer credentials; stored as encrypted Worker secrets. |

The Google Web client ID used by Credential Manager and Unity is:
`258837307348-4o39ggbqmdrjb8vlfqmmidrom29fa5qr.apps.googleusercontent.com`.
The Android OAuth client is tied to the package and development SHA-1:
`BF:C5:89:B7:8E:98:D5:0F:DF:D4:31:D4:D5:ED:CC:E0:19:AD:29:3F`.
Register a separate matching OAuth/signing configuration before changing the key.
The current APK is development-signed, not production-signed.

Google project billing was disabled after creation. The existing Firebase Payment
billing account is pay-as-you-go; its collection threshold is not a spending
cap, and its pre-existing budget applies to another project. Other projects were
not changed. Cloudflare remains Free with no payment method or paid add-on added.
Unity's existing project services remain in use; this task does not establish a
new account-wide spending cap. See [backend limits/setup](../Tools/AccountServer/README.md).

## Build setup for another checkout

Use Unity 6000.3.20f1 with Android tools. Before launching the build process, set
these **public** values in its environment; teammates must configure their own
shell rather than depend on another machine's user environment:

```powershell
$env:WTF_HUAWEI_APP_ID = '119406947'
$env:WTF_GOOGLE_WEB_CLIENT_ID = '258837307348-4o39ggbqmdrjb8vlfqmmidrom29fa5qr.apps.googleusercontent.com'
$env:WTF_HUAWEI_AUTH_URL = 'https://whatthefish-huawei-auth.benliew28262826.workers.dev/v1/huawei'
./Tools/Build/Build.ps1 -Target AndroidSubmission
```

The Android postprocessor copies the native Java bridge and writes public
manifest metadata. It pins Huawei `hwid:6.14.0.300`, AndroidX Credentials `1.6.0`,
Google ID `1.2.1` and Play Services Basement `18.12.0`. The advertised Huawei
6.15.0.300 AAR was reachable but its official POM returned 404 during this build;
6.14.0.300 was the latest complete publication verified for dependency resolution.
An incremental-build fix replaces existing version declarations correctly.
Provider AAR consumer shrinker rules are retained; our JNI bridge has its own
keep rule. One missing optional Huawei installer class has a narrow warning
exception after verifying Huawei's runtime availability guard.

The APK contains the native account bridge, a non-exported account activity,
public IDs/endpoint and provider SDKs. Minimum Android API is 26. Credential
Manager dependencies add normal biometric/fingerprint permissions and a
signature-level dynamic-receiver permission; this game's sign-in flow does not
collect biometric data. Camera permission belongs to the existing QR scanner.
No broad photo/storage permission is introduced. No web-client secret or Unity
service secret is needed by the app.

## Authentication and storage boundaries

Google supplies an ID token to Unity's Google sign-in/link APIs. Huawei supplies
an ID token to the HTTPS adapter, which verifies the RSA signature, exact issuer,
audience and time claims against Huawei's fixed signing-key endpoint. It derives
the external identity server-side and calls Unity Custom ID; linking also needs
the current player's UGS access token. The app checks that linking preserves the
player ID. No force-link, arbitrary caller-supplied player ID or client-side
service credential is used.

Direct Huawei-to-Unity OIDC was not claimed as working: Huawei's discovery host
and token issuer differ, and the inspected Unity form exposed no separate
discovery URL. The Custom ID adapter is the implemented route.

No email, phone or provider profile-photo scopes are requested. Provider ID tokens
are passed through memory and are not deliberately saved or logged by this code.
**Unity Authentication's SDK persists its session token using its normal local
PlayerPrefs-backed storage.** This implementation does not add Android Keystore
protection; do not describe local credentials as encrypted by this feature.
Account hardening before public release includes reviewing this persistence,
reauthenticated deletion and privacy disclosures. The server's freshness window
is not nonce-based replay prevention. Full details are in the backend README.

## Verification and limits

- Native Android Java compilation, release shrinker/package build and the official
  AndroidSubmission build passed. APK ZIP integrity and v2 signature passed;
  public manifest values and retained account/scanner bridge classes checked.
- 25 backend tests and 7 live Worker health/rejection checks passed. Health only
  proves required configuration exists, not a real Unity credential exchange.
- 36 account UI/identity assertions at 1600×900 and 1200×900, 338 startup checks,
  12 two-peer name-replication checks and 1,057 offline menu checks passed.
  Account UI captures were visually inspected. These do not prove native login.
- The account suite also passed using its default player path from a copied
  checkout containing spaces, invoked from an unrelated working directory.
- The fresh APK is **94,676,500 bytes**, up **765,870** from **93,910,630**,
  leaving **905,323,500 bytes** below the strict 1,000,000,000-byte limit.
  See [measured build record](BUILD-SIZE.md).

Required Android checks: Huawei and Google success/cancellation; missing or
outdated services; native HTTP access to the Worker; guest linking preserving
UGS ID; already-linked conflicts; relaunch/session expiry; sign-out; recovery on
a second installation; name separation between accounts; production signing.
Phone camera/image scanning, touch layout and performance still need device
validation. Android application/OAuth production publication is outside this task.

Current evidence is under ignored `Builds/AccountSignInQA/`,
`Builds/PlayerIdentityQA/Network-20261020-161630-129/` and
`Builds/StorybookMenuQA/AccountSignIn-20261020/`. Current delivery paths are
`Builds/Android/WhatTheFish-release.apk` and the complete `Builds/WindowsFinal/`.

## Source and reproduction

- [PlayerAccount.cs](../Game/Assets/_Game/Shared/Core/PlayerAccount.cs): UGS session/link/recovery coordination.
- [PlayerIdentity.cs](../Game/Assets/_Game/Shared/Core/PlayerIdentity.cs): local name and identity separation.
- [AndroidAccountSignIn.cs](../Game/Assets/_Game/Shared/Platform/AndroidAccountSignIn.cs): native bridge and capability ordering.
- [AccountPanel.cs](../Game/Assets/_Game/Shared/UI/AccountPanel.cs): account choices and errors.
- [Native Java](../Tools/Android/AccountSignIn/AccountActivity.java), [Android build configuration](../Game/Assets/_Game/Editor/AccountSignInBuild.cs) and [backend setup](../Tools/AccountServer/README.md).
- [Test-AccountSignIn.ps1](../Tools/Build/Test-AccountSignIn.ps1): backend tests and isolated Windows account UI review. Use `-PlayerPath Builds/WindowsFinal/WhatTheFish.exe` after promotion.
- [Startup checks](../Tools/Build/Test-StartupWelcome.ps1) and [two-peer identity checks](../Tools/Build/Test-PlayerIdentity.ps1).

Primary references: [Huawei Account Kit](https://developer.huawei.com/consumer/en/codelab/HMSAccounts/),
[Google Credential Manager](https://developer.android.com/identity/sign-in/credential-manager-siwg-implementation),
[Unity Google](https://docs.unity.com/en-us/authentication/platform-signin/google),
[Unity Custom ID](https://docs.unity.com/en-us/authentication/platform-signin/custom-id).

The following is a historical guest-only delivery record; its setup limitations
apply to 16 October, not the current sign-in implementation.

## Guest delivery verification — 16 October 2026

The final Windows candidate passed 443 startup/name/profile assertions at 1600 × 900, 1560 × 720 and 1200 × 900. Separate launches retained the same guest ID and prefilled name. Actual guest-entry and profile captures were visually inspected. The existing offline menu review passed 730 assertions; the room/menu review passed 283 assertions after exercising shared travel and returning to the lobby.

The initial and final player snapshots each passed the 12-assertion two-peer name test. For the final player, that test ran using its default relative player path in a copied checkout containing spaces, launched from the system temporary directory. Both peers agreed on initial names and renamed rosters. Local documentation links were checked. The tests use development-only preference namespaces, leaving the user's normal name and ID untouched.

Two test assumptions were corrected: hidden InputField placeholders are excluded from rendered-text checks, and the existing room review waits for the shared arrival sequence instead of trying to exit after a fixed three seconds. Neither correction changes production travel or rendering. The first failed run is retained as diagnostic evidence.

All 296 source documents represented in the Windows assembly's debug information matched the working source. All 325 files of the candidate player matched after full promotion to `Builds/WindowsFinal/`, before updating its delivery stamp. The snapshot includes the pre-existing startup-welcome, bot/start, football pass and HUD changes; those edits were preserved. No Git staging, commit or push was performed.

Current evidence:

- `Builds/StartupWelcomeQA/Run-20261016-184157-073/`: final startup/profile checks and captures.
- `Builds/StorybookMenuQA/PlayerIdentity-20261016/offline/`: offline regression checks.
- `Builds/StorybookMenuQA/PlayerIdentity-recheck-20261016/`: final host/client room checks.
- `Builds/PlayerIdentityQA/PortabilityNetwork/`: final identity/portability checks and agreed rosters.
- `Builds/PlayerIdentityQA/`: build logs, compiled-source proof and complete player manifest.

The previous complete player/APK are recoverable from `Legacy/20261016-184343-472-player-identity-delivery/`. The first candidate is in `Legacy/20261016-183848-988-player-identity-first-candidate/`. Promoted duplicate players and the temporary checkout are in `Legacy/20261016-184506-300-player-identity-promoted-copies/`. All moves used the repository archive tool; current test evidence stays under `Builds/`.

A fresh `AndroidSubmission` build succeeded and passed the automatic APK gate. Independent APK signature verification passed; the package still uses the existing Android debug certificate. The source snapshot remained unchanged through the Android build. Generated Golf Ball prefab YAML was compared by named object/component references: all 14 objects and fields were equivalent, so the original serialization was restored. The generated copy and semantic comparison remain in the task's evidence folder.

No physical Android device was attached; phone keyboard/touch behavior, phone performance and all real Huawei flows remain unverified. Huawei app registration, signing configuration, cloud-profile storage and account-linking integration are still required. Current delivery is `Builds/Android/WhatTheFish-release.apk` and the complete `Builds/WindowsFinal/`; its `LATEST-BUILD.txt` records the source scope and checks.
