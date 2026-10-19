# Huawei Scan Kit room invitations

The Android game generates and scans room QR invitations with Huawei Scan
Kit SDK-Plus **2.15.0.301**. Manual entry and Paste code remain available.
Windows generates compatible invitations with the MIT Codecrete encoder;
its Scan button explains that scanning requires the Android game.

## Player flow

1. Create an online room. The lobby shows its code and QR. Tap the QR for a
   larger invitation that another phone can scan.
2. On Android, open Join a room and select Scan room QR. Allow camera access.
   The scanner offers Cancel and a flashlight on supported devices.
3. A valid scan fills the room-code field. Tap Join room to use the existing
   Unity Gaming Services session and Relay connection.

An invitation is `WTFROOM:1:ROOMCODE`, with a 4–12 character uppercase
alphanumeric code. It contains no account credentials, access tokens or
personal data. Foreign QR contents are rejected and never opened as URLs.
The existing room service handles expired, full and incompatible rooms.
Everyone still shares one room across the four islands. Local test rooms do
not generate online invitations. Share room invitations only with intended
players: knowing the code grants the same joining capability as typing it.

## Implementation and rebuild

- `Game/Assets/_Game/Shared/Platform/RoomInvitation.cs` validates invitations.
- `HuaweiRoomScan.cs` bridges Unity to Android and generates desktop QR images.
- `StorybookMenu.RoomQr.cs` adds lobby QR display and scanner results to the menu.
- `Tools/Android/RoomScan/` contains the native activity and bridge. Scan Kit's
  customized RemoteView controls the camera; Unity polls a bounded result on
  its main thread when the activity returns.
- `Game/Assets/_Game/Editor/HuaweiScanBuild.cs` adds the Huawei Maven repository,
  pinned SDK dependency, Java source, camera permission, private scanner activity
  and keep rules to Unity's generated Gradle project. No custom Unity Gradle
  templates are required. Camera hardware is optional so manual joining remains
  available on devices without a camera.
- The SDK's consumer rules retain Huawei's scanner classes. The scoped
  `Tools/Android/RoomScan/proguard-rules.pro` preserves the Unity bridge and
  suppresses exact missing optional-provider classes from Huawei's networking
  dependencies. R8 remains enabled; there is no blanket warning suppression.
- `Game/Assets/ThirdParty/CodecreteQr/` contains the desktop encoder and license.
  The license is copied into delivered players.

From any working directory, invoke the repository's `Tools/Build/Build.ps1`
by its resolved path with `-Target AndroidSubmission`. Java source and settings
are regenerated each build. A first build needs access to Huawei's Maven
repository. Do not edit generated files in `Game/Library` as permanent source.

The scanner asks only for camera access, when selected. It does not offer
gallery import or request storage access. Huawei operation tracking is disabled
before encoding or opening the scanner. A scan result is not written to logs.
The game still uses its existing network services when creating or joining rooms.

## Huawei app registration

Registered on 18 October 2026 after the user signed in directly:

- Project: **WhatTheFish**.
- Android app: **WHATTHE FISH?**, Mobile phone, Game, English (US).
- Package: `com.umpsa.whatthefish`; App ID: `119406947`.
- Current development certificate SHA-256 registered:
  `6A:7B:EB:4B:C1:B0:49:BD:18:E9:95:AF:40:AC:0B:CB:57:AF:78:3B:30:8F:90:22:D9:F9:B7:F7:50:B3:BA:6C`.

The local user environment variable `WTF_HUAWEI_APP_ID` is configured for this
app. Teammates should set it before building, or set it explicitly in the build
shell. Restart already-open terminals/editors if they have an older environment.
The build inserts `com.huawei.hms.client.appid` metadata. The app ID and public
certificate fingerprint are identifiers; private signing keys and client
secrets must remain outside Git and chat.

Manage APIs did not list a Scan Kit toggle. No additional service switches were
changed. No data-processing location or cloud-service plan was enabled for this
local scanning feature. If later services require these, choose the region for
the team's intended use and complete their configuration separately.

The generic AppGallery Connect wizard offers `agconnect-services.json`. This
local Scan Kit bridge does not consume that file; add the configuration/plugin
deliberately when integrating a service that requires it.

Before distribution, register the final signing certificate if it differs from
the current Android Debug certificate, complete privacy declarations and store
details, and test on Android. The app has been registered, not submitted to or
published on AppGallery.

Huawei Account Kit player login is not part of this feature. Camera behavior
and Huawei integration on a physical phone remain unverified.

## Verification

On 18 October 2026, the full Windows player passed **38 QR assertions** and the
parallel menu suite passed **1,442 assertions** (1,053 offline, 196 host, 193
guest). The review created and closed a real UGS room; an independent ZXing
decoder read its QR image and both lobby/modal captures, plus three fixture
codes. The manual Join button and typed code remained usable after the desktop
scanner fallback. The complete player was promoted to `Builds/WindowsFinal/`,
with 327 files verified by SHA-256 before updating the release note.

The full AndroidSubmission rebuild passed with release minification enabled.
The resulting **90,543,496-byte APK** passed ZIP integrity and v2 signature
verification. Inspection confirmed camera permission, optional camera hardware,
private scanner activity, registered app ID, bridge/SDK DEX classes, native
scanner and model assets. The first failed R8 attempt and the scoped-rule repair
are retained as diagnostic evidence. See [size audit](APK-SIZE-AUDIT.md).

Previous complete player archive:
`Legacy/20261018-194458-193-scan-kit-windows-delivery/`. The build includes the
parallel menu work and pre-existing audio/network changes; these were preserved.
Windows license copying was added to the build postprocessor after the player
build, so the identical license was copied manually into this tested delivery.
Subsequent changes to Android build rules do not affect the Windows runtime.

The development player includes `-roomQrReview <output-directory>` for payload
validation, desktop encoding, manual-join availability and scanner fallback.
Add `-qrOnline` to exercise a live room's QR and enlarged invitation. Review
code is excluded from non-development Android builds. Current evidence is
kept under ignored `Builds/ScanKitQA/`.

Physical Android testing is deferred at the user's request. When a phone is
available, check permission allow/deny, cancellation, flashlight, rotation,
background/resume, invalid and expired QR codes, and joining a host on a
second device. Include a device without HMS Core to verify the SDK-Plus path.
No phone FPS, camera performance or Huawei-service acceptance is claimed.

## Sources

Checked 18 October 2026:

- [Huawei Scan Kit version history](https://developer.huawei.com/consumer/cn/doc/HMScore-Guides/android-version-change-history-0000001050043947)
- [Customized View integration](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/android-customized-view-0000001050042012)
- [Scan Kit API overview](https://developer.huawei.com/consumer/en/doc/HMSCore-References/scan-apioverview-0000001050185407)
- [Huawei official Android sample](https://github.com/HMS-Core/hms-scan-demo)
- [Codecrete QR encoder v2.1.0](https://github.com/manuelbl/QrCodeGenerator/tree/v2.1.0)
