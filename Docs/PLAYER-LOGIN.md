# Guest profiles and Huawei sign-in

Requirements agreed on 16 October 2026: guests need a player name; Huawei users should be able to restore their account on another device and upgrade an existing guest. The owner has a Huawei Developer account but has not created the app in AppGallery Connect yet.

## Available in this change

The welcome screen's **Continue as guest** opens a name dialog. Names use 1–16 characters, with at least one letter or number; spaces, hyphens and underscores are also allowed. Blank names, markup and unsupported punctuation are rejected. Duplicate display names are allowed; names are not login credentials.

A successful entry saves a random device-local guest ID and the name. Later launches prefill the name, and **Your profile** on the home menu allows editing it. Cancelling leaves the saved profile unchanged. An existing fishing nickname is offered as the initial name. Fishing's name editor uses the same profile. Room rosters and the four sports' participant records use the shared name; room names are validated again by the host.

Guest creation works offline. Online rooms still initialise Unity Authentication anonymously when needed. The local guest ID is separate from the UGS player ID and is never used as proof of account ownership. Guest storage is local and can be lost when app data is cleared. Explicit Windows Explore shortcuts and development probes retain direct entry, with a `Guest` fallback until a name is saved.

**Huawei remains a coming-soon action.** No Huawei login, token verification, account linking or cloud profile recovery is claimed by this delivery.

## What the owner needs to provide

| Item | Next action |
| --- | --- |
| AppGallery Connect app | Create an Android APK app under a project. Use the existing package name `com.umpsa.whatthefish` and game name WHATTHE FISH?. Confirm the developer account has completed identity verification. |
| Huawei app configuration | Enable Account Kit, then provide the app/project identifiers, client ID and the downloaded `agconnect-services.json`. Keep the downloaded configuration under ignored `Builds/HuaweiSetup/` for review before deciding what may be distributed. |
| Signing certificate | Choose the signing key for the test/submission APK. Register its SHA-256 fingerprint in AppGallery Connect. Separate debug and release certificates need their corresponding registered fingerprints. The fingerprint can be derived locally; passwords and private keys are not needed in chat. |
| Region and audience | Confirm the AppGallery project region and intended distribution countries. This is an Android HMS integration; a native HarmonyOS application would be a separate target. |
| Test device | Provide access to an Android device that can use HMS Core and a Huawei test account, plus a second installation for recovery tests. The owner signs into the account personally. |
| Competition constraints | Supply the hackathon's Huawei integration checklist if it specifies Game Service or other kits beyond Account Kit. These are separate from basic sign-in. |

Huawei documents the app registration, signing fingerprint, Account Kit enablement and configuration download in its [Account Kit Android codelab](https://developer.huawei.com/consumer/en/codelab/HMSAccounts/). The project already uses Unity 6000.3.20f1, Android minimum API 26 and Authentication SDK 3.7.1. No HMS Android plugin or app configuration is currently installed. Custom keystore signing is disabled. Inspection of the existing APK confirms an Android Debug certificate; its public fingerprint is recorded locally in `Builds/PlayerIdentityQA/current-signing-certificate.txt`. Use that only for matching development builds, and register the final signing certificate before testing a submission build signed with that key.

No Huawei password, app/client secret, private signing key or Unity service-account credential should be pasted into chat or committed. Any server credential needed by the chosen integration belongs in the deployment's secret store. We will inspect the downloaded app configuration before using it; a config file alone does not provide a secure account backend.

## Integration design for account recovery

Use the existing UGS player ID as the online game's durable identity. Keep an editable in-game display name separate from the Huawei account's profile name. Request only the identity scopes needed for sign-in; email, phone number and avatar are unnecessary for the current feature.

First evaluate Huawei ID-token compatibility with UGS OpenID Connect using the real registered app: issuer discovery, signing keys, token audience and device flows must work together. Unity supplies sign-in and anonymous-account linking APIs for configured OIDC providers; it does not implement the Huawei SDK flow for us. A successful guest upgrade must link to the current anonymous UGS player, preserving its ID, instead of silently signing into a new player. See [Unity OpenID Connect](https://docs.unity.com/en-us/authentication/openid-connect). Direct Huawei-to-UGS compatibility remains unverified.

If direct OIDC cannot satisfy that contract, use a small HTTPS backend: validate Huawei credentials server-side, derive identity from verified claims, and exchange it through UGS Custom ID. Linking must also prove ownership of the current UGS account. Unity's [Custom ID documentation](https://docs.unity.com/en-us/authentication/platform-signin/custom-id) requires a server-authoritative backend and service-account credentials; those cannot be shipped in the APK. Before this route is implemented, the owner must identify a hosting account/project and configure its secret store. No backend deployment or paid service has been provisioned.

Cross-device recovery also requires a cloud profile record keyed by UGS player ID. Proposed first record: schema version, display name and update revision. [Unity Cloud Save](https://docs.unity.com/en-us/cloud-save/tutorials/unity-sdk-sample) supports authenticated player data and write locks. Confirm dashboard access/configuration for the existing Unity project and enable the selected storage service. Authentication alone does not sync local names, settings or progress. Migration of sports progress and cosmetics needs an explicit inventory; this first scope is player identity and display name.

Account switching must happen outside an active room. Keep the guest if authorization is cancelled, network access fails or credentials are rejected. If a Huawei account is already linked elsewhere, explain the conflict and let the player explicitly switch to that existing account; never force-link or overwrite either profile. On sign-out, remove the active account session/cache and present the entry choice without displaying another account's cached profile. Reload the cloud name after successful recovery. No real authentication token belongs in PlayerPrefs or logs.

## Acceptance checks

Guest checks cover first entry, validation, cancellation, fishing-name migration, editing, stable ID across process restarts, and host/guest name replication. Windows tests use isolated preference keys and leave the user's normal saved profile untouched.

Huawei completion requires physical-device evidence for success, cancellation, offline/error states, missing/outdated HMS behavior, relaunch, sign-out, guest upgrade preserving the UGS ID, already-linked conflicts, and a second installation recovering the same UGS ID and cloud name. Verify the release-signing fingerprint as well as development signing, and check that logs and packaged assets contain no server secrets. Windows guest tests cannot validate those flows.

## Source and reproduction

- [PlayerIdentity.cs](../Game/Assets/_Game/Shared/Core/PlayerIdentity.cs): local identity, shared validation and name persistence.
- [StartupWelcome.cs](../Game/Assets/_Game/Shared/UI/StartupWelcome.cs) and [StorybookMenu.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.cs): guest entry and profile editor.
- [NetworkAthlete.cs](../Game/Assets/_Game/Shared/Networking/NetworkAthlete.cs): owner name replication and host sanitization.
- [PlayerIdentityBuild.cs](../Game/Assets/_Game/Editor/PlayerIdentityBuild.cs): candidate player under ignored `Builds/PlayerIdentityQA/Player/`.
- [Test-StartupWelcome.ps1](../Tools/Build/Test-StartupWelcome.ps1) and [Test-PlayerIdentity.ps1](../Tools/Build/Test-PlayerIdentity.ps1): UI/persistence and two-player identity checks.

After building a candidate, run both test scripts with `-PlayerPath Builds/PlayerIdentityQA/Player/WhatTheFish.exe`, then the existing menu checks. After promotion, use `-PlayerPath Builds/WindowsFinal/WhatTheFish.exe`. Build Android with `Tools/Build/Build.ps1 -Target AndroidSubmission`.

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
