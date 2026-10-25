# Cable-car startup welcome

The selected Cable-car Welcome design serves as the game's loading and entry screen. The current [raster master](../ArtSource/StartupWelcome/README.md) is the approved 17 October 2026 refinement, with light peach skin, clean sculpted hair and improved cable-car, station, rail and park colors. The ready screen uses that exact image, including its English, Settings and Info designs. Native Unity buttons add real input, hover, press and keyboard-focus feedback over the illustrated controls. Loading text, progress and dialogs are native UI; the illustration is static.

On a normal APK or Windows launch, the screen appears after the platform/Unity splash, before the packaged islands are prepared. The loading card sits immediately right of the character's legs, near the lower edge. Its progress follows actual content preparation and scene-loading stages. It reaches 100% only after the existing main menu is available. The progress card then clears and the right-hand sign reveals the entry buttons without changing the background.

- **Continue as guest** asks for a player name, saves a local guest profile, and opens the existing main menu. Returning guests see their saved name. It does not create an online session. See [player login and Huawei requirements](PLAYER-LOGIN.md).
- **Account sign-in** uses the existing supported Android provider flow; desktop explains that phone sign-in is unavailable.
- **Language** uses the separately implemented language selector. The Settings/Info redesign preserves that entry point.
- **Settings** opens a cream and teal welcome dialog with Master, Game sounds, Ambience, Button sounds and Reduced motion. All use the existing `MenuPreferences` values and saved keys; changes save on close or application pause. Camera, layout and sport-specific controls stay in the full settings pages after entry.
- **Info** opens a read-only introduction: four sport cards, solo/room play, shared travel, the actual application version and Done. The Credits button and its generic tool/milestone page have been removed. It never loads an island or launches a tutorial. Existing interactive tutorials remain in the signed-in main menu.
- A content preparation failure keeps entry unavailable and offers **Try again** on the sign.

The three utility touch targets include both circle and label. During preparation/retry, a small patch from a blank-sign companion image covers the ready-state sign contents; it never replaces the character or scenery. When preparation succeeds, the patch clears to reveal the approved English sign. The language feature owns its translated artwork overlays.

Settings and Info remain available during content preparation and after a loading failure. Only sign-in and guest entry require ready content. An open utility dialog stays open when loading completes, and closing it restores the appropriate entry controls. They keep the login artwork visible behind a dimmed backdrop. Close, Done and Escape dismiss the dialog. Browsing them does not create or change a sign-in identity, select a sport or join a room. Normal guest entry still requires the name dialog. Dialogs block underlying entry controls. Native rounded geometry, gradients, inset rows and large slider thumbs remain editable UI. Four transparent 1024-pixel renders now show the actual gameplay ball/fish models; [sources, capture tool and integrity](../ArtSource/WelcomeSportIcons/README.md) are retained. Golf uses cream/beige and Fishing uses light blue, with lighter tops and deeper bottoms like Football/Basketball. The entire panel fits inside the device safe frame without stretching the background. The four-island world, room model and shared travel behavior are unchanged.

Canvas scaling now drives dynamic-font rasterization at the real display density,
instead of stretching a child transform's text mesh. Rounded panel edges have a
one-display-pixel alpha fringe for smooth edges in overlay UI. The icon importer
preserves 1024×1024 resolution, transparency and high-quality mobile compression.

## Source and reproduction

### Gameplay icons and sharper welcome UI — 25 October 2026

Info now uses the existing gameplay prefabs for football, basketball, golf ball
and lagoon fish, rendered as transparent 1024×1024 images. Golf has a cream/beige
card and Fishing a light blue card; their tops are lighter than their bottoms.
Credits is removed; version and Done remain. Existing license/provenance files
are retained. See [art source and regeneration](../ArtSource/WelcomeSportIcons/README.md).

Welcome fonts now follow Canvas pixel density, and rounded panels use a one-pixel
alpha fringe. Concurrent localized-text supersampling is retained; a font atlas
rebuild could contaminate another label's shared UI mesh, so mesh clearing now
occurs after text generation, matching Unity UI's ordering. The regression review
checks that each label mesh contains exactly its own generated glyphs.

The complete Windows player passed **1,684 assertions**: 427 normal/cold and 419
each wide, tablet and Full HD, across English, Chinese and Malay. The fourth run
requested 2560×1440, but the desktop clamped the actual capture to **1920×1080**;
future runner arguments use that actual supported size. Images were visually
reviewed. PNG CRC/alpha/bounds and all **335 player file hashes** passed, with
**502 compiled source documents** matching the workspace. Current evidence:
`Builds/WelcomeIconsQA/Verified/`, integrity/source comparisons and delivery hashes.
Cloud profile saving was unavailable in the test; local guest entry and recovery
controls passed. Physical phone rendering/touch/FPS remain unverified.

Earlier failed captures and integration runs are kept as diagnostic evidence.
The test runner now uses a fresh per-suite profile to avoid colliding with older
folders also named `Checks`. A code-only interim refresh lacked a concurrently
added shader; the final player uses the newer full native payload, with matching
scene/catalog/payload hashes across its final code refresh.
Recovery archives: `Legacy/20261025-154536-430-welcome-icons-initial/` and
`Legacy/20261025-154956-584-welcome-icons-windows/`. The promoted duplicate is
archived in `Legacy/20261025-155128-209-welcome-icons-promoted/`; current evidence
remains under `Builds/WelcomeIconsQA/`.

Official AndroidSubmission also passed: **160,144,610 -> 161,335,456 bytes**;
headroom **838,664,544 bytes**. Signature v2, ZIP CRC, 1,265 entry sizes and
the five-scene bundle passed; all 361 Android source records matched. The four
icons contribute 1,172,710 compressed APK bytes. See [size audit](APK-SIZE-AUDIT.md).
Prior APK/audit recovery: `Legacy/20261025-154835-073-welcome-icons-android/`.

### Welcome modal delivery — 25 October 2026

This earlier revision introduced Settings, Info and Credits with cream/teal native rounded panels
above. The complete Windows player passed **962 rendered assertions** (326 normal,
318 wide, 318 tablet), including all three languages, preference persistence,
modal input gating, text visibility and guest entry. Final captures were visually
reviewed. All **501 Windows** and **360 Android** compiled source documents matched
the shared workspace at verification. The complete **335-file** Windows copy was
hash-verified in `Builds/WindowsFinal/`.

The official AndroidSubmission build succeeded: the actual existing APK was
**115,954,062 bytes**, and this integrated APK is **160,144,610 bytes**, leaving
**839,855,390 bytes** below the strict decimal-GB limit. Signature v2, ZIP CRC,
1,260 audited entries and the five-scene bundle integrity checks passed. This
snapshot also includes concurrent localization and privacy/account work; the
welcome redesign adds no raster assets. See [size audit](APK-SIZE-AUDIT.md) for
the integrated package contributors. This task does not certify every concurrent
feature or physical phone touch, rendering and FPS.

Evidence: `Builds/WelcomePanelsQA/FinalChecks/`, source comparisons, APK verification
and complete player hashes in that evidence folder. Earlier failed typography
captures remain in `Checks/`; `Checks2/` and `FinalChecks/` pass after reflow.
Recovery: `Legacy/20261025-144545-819-welcome-panels-first-candidate/`,
`Legacy/20261025-145145-702-welcome-panels-android/` and
`Legacy/20261025-145606-822-welcome-panels-windows/`. Superseded candidate copies
were archived after promotion in `Legacy/20261025-145849-900-welcome-panels-candidates/`;
current validation evidence remains active. No Git publishing was performed.

### Earlier shared-page delivery — 25 October 2026 (superseded design)

The complete Windows player passed **474 rendered assertions** at 1600×900,
1560×720 and 1200×900. Coverage includes all four tutorial replays/resumes,
Settings persistence, nested Back navigation, returning to welcome, and normal
guest entry afterward. Settings and tutorial-selector captures were reviewed.
Evidence: `Builds/WelcomeUtilitiesQA/verified-cases.json`, `StartupChecks2/` and
`TabletIsolated/` in that evidence folder. Two sequential tablet assertions
expected an old guest name after the preceding cloud profile test renamed it;
the independent tablet run passed. Those earlier failures remain recorded.

The delivered player contains this task's changes on `dc0be40`, before concurrent
localization/AppGallery edits. Those later edits are preserved in the workspace
but are outside this player validation. AndroidSubmission was attempted while
the concurrent localization edit referenced unfinished methods and failed
compilation; **Android size for this feature is unverified**. The prior APK
remains **115,954,062 bytes** and is not validation of this change. Physical
Android input, rendering and performance remain untested.
The portable-path check passed. Full task readiness was blocked by the concurrent
`StartupWelcome.Language.cs` addition missing its Unity `.meta` file at check
time; that in-progress work was preserved.

Recovery archives: `Legacy/20261025-140632-935-welcome-utilities-windows/` and
`Legacy/20261025-140101-610-welcome-utilities-android/`.

- [StartupWelcome.cs](../Game/Assets/_Game/Shared/UI/StartupWelcome.cs): responsive artwork frame, native UI and modal input gate.
- [StartupWelcome.Panels.cs](../Game/Assets/_Game/Shared/UI/StartupWelcome.Panels.cs): welcome Settings/Info/Credits layout, localized introductory copy and shared preference bindings.
- [StorybookMenu.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.cs): full settings after entry.
- [StorybookMenu.Tutorials.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.Tutorials.cs): existing four-sport tutorial selector in the main menu.
- [PackagedContentBoot.cs](../Game/Assets/_Game/Shared/Core/PackagedContentBoot.cs): staged progress, content integrity/cache preparation, retry and ready state.
- [StartupWelcomeBuild.cs](../Game/Assets/_Game/Editor/StartupWelcomeBuild.cs): importer and candidate Windows build. The approved scene retains 1672 × 941 pixels; the generated 1671 × 941 blank-sign companion uses normalized UVs in that same UI frame. Both use no mipmaps, clamp sampling and Android ASTC 4×4 compression.
- [StartupWelcomeReview.cs](../Game/Assets/_Game/Shared/Core/StartupWelcomeReview.cs) and [Test-StartupWelcome.ps1](../Tools/Build/Test-StartupWelcome.ps1): development-only runtime review and repeatable Windows test runner.

Run `Tools/Build/Build.ps1 -Target Windows` or `-Target AndroidSubmission` from a checkout to build the current game. The editor method `WhatTheFish.Editor.StartupWelcomeBuild.Windows` builds an isolated candidate under `Builds/StartupWelcomeQA/Player/`. Test a delivered development player with `Tools/Build/Test-StartupWelcome.ps1 -PlayerPath Builds/WindowsFinal/WhatTheFish.exe`.

Explicit `Explore-*.cmd` shortcuts keep their direct-to-sport behavior. Development review/audit flags bypass the welcome after content is ready; `-startupWelcomeReview` exercises the screen itself. These exceptions do not affect a normal phone launch. Account sign-in, language selection and welcome settings now have their own implemented flows; guest names and a local guest ID persist on the device. Consult the account and localization documents for platform-specific verification limits.

## Validation and limits

The current startup review exercises a forced preparation failure and retry, cold and warm caches, real pointer raycasts, Settings/Info in all three supported languages, shared preference persistence, full-resolution icon loading, card gradients, Credits removal, guest entry, restored gameplay input, safe-area containment and text visibility. It captures the actual Unity UI at 1600 × 900, 1560 × 720, 1200 × 900 and 1920 × 1080, including simulated screen cutouts. Current icon/clarity evidence stays under ignored `Builds/WelcomeIconsQA/`; older startup evidence remains under its recorded task folders.

The 17 October 2026 art-refinement candidate passed **338 startup assertions** (118 normal/cold, 110 wide/warm and 110 tablet/warm). This includes clicking the English, Settings and Info labels, restoring the exact approved sign after loading, guest-name persistence and entry into the existing main menu. Actual loading, ready, retry, placeholder, wide-cutout and tablet guest-dialog captures were visually inspected. All 303 compiled C# source documents matched the working snapshot. PNG signatures, dimensions, chunk CRCs and master/delivery hashes passed, including a byte-for-byte comparison with the approved review image. Twelve relative documentation links resolved in a copied root containing spaces from an unrelated working directory. Evidence is under `Builds/StartupWelcomeRefinementQA/`, with runtime captures in `Run-1/`.

Superseded art snapshots are recoverable in `Legacy/20261017-161809-818-startup-refined-art/`; the prior Android delivery was archived before rebuilding in `Legacy/20261017-162157-571-startup-refinement-android/`. The old active texture GUID is retained; no scene, prefab or runtime resource reference was removed. The imagegen blank-sign layer is sampled only inside the sign, keeping the approved character and environment intact in every state.

The refinement was delivered as a complete 325-file Windows player in `Builds/WindowsFinal/`; every file matched the tested candidate during promotion. The previous player is recoverable in `Legacy/20261017-162525-062-startup-refinement-windows/`. A fresh `AndroidSubmission` build completed successfully and passed the automatic APK gate and APK Signature Scheme v2 verification. Delivery time and snapshot limits are in `Builds/WindowsFinal/LATEST-BUILD.txt`. No Android device was connected, so physical phone rendering, input and performance remain unverified. This build includes the shared workspace's existing football-ceremony and basketball work; this task's regression checks cover startup and guest entry, not every sport feature.

The Golf Ball prefab was already modified at task start. After build regeneration, a format-aware comparison proved all 14 objects, fields and external references equivalent to that task-start snapshot before restoring only the generated YAML ID/order changes. The unrelated initial changes were preserved. `Builds/StartupWelcomeRefinementQA/golf-prefab-semantic-check.json` records that comparison. Current logs, captures and integrity evidence remain in `Builds/`; promoted candidate copies and temporary portability copies are archived using the repository tool, with recovery batches recorded in `Builds/StartupWelcomeRefinementQA/archive-cleanup.txt`.

The original 16 October 2026 welcome-only candidate passed 277 startup assertions, 104 cold/warm packaging assertions and 713 existing-menu assertions. Loading, ready, retry and placeholder captures were visually reviewed. The packaging checks visited all four islands. All 294 compiled source documents matched that source snapshot, and all 325 files of the complete player matched after promotion. Its `AndroidSubmission` build succeeded and passed the automatic APK gate. This candidate has since been superseded by the [guest-profile delivery and its verification](PLAYER-LOGIN.md#guest-delivery-verification--16-october-2026).

Evidence: `Builds/StartupWelcomeQA/Run-20261016-172120-659/`, `Builds/StartupWelcomeQA/Packaging/`, and `Builds/StorybookMenuQA/StartupWelcome/`. The startup runner also passed from a copied script tree containing spaces with an unrelated working directory; 19 local documentation links resolved in the alternate root. PNG inspection verified dimensions, CRCs and integrity hashes. Embedded C2PA provenance, its icon and certificate URLs were retained; they are not external image dependencies.

Build regeneration changed Golf Ball prefab YAML object IDs/order. A format-aware comparison resolved local references by named object/component and proved all 14 objects, fields and external references equivalent before restoring the original YAML. Its generated copy and comparison remain in the ignored evidence folder. Pre-existing bot, pass and HUD changes were preserved; no Git staging, commit or push was performed.

Delivery verification is recorded in `Builds/WindowsFinal/LATEST-BUILD.txt`. Phone rendering, physical cutouts, touch behavior and performance still require a physical Android test; Windows captures do not establish phone FPS. The artwork has no external image/library dependencies. Its recorded master and delivery hashes are in [integrity.json](../ArtSource/StartupWelcome/integrity.json).

The previous complete player and APK are recoverable in the ignored archive `Legacy/20261016-172454-757-startup-welcome-delivery/`. Superseded candidates and their evidence are in `Legacy/20261016-171145-450-startup-first-candidate/` and `Legacy/20261016-171742-949-startup-second-candidate/`; temporary portability copies are in `Legacy/20261016-172306-384-startup-portable-check/`. The validated candidate copy was archived after full promotion in `Legacy/20261016-172816-935-startup-promoted-candidate/`; current evidence remains under `Builds/`. Earlier rejected design drafts remain in the `20261016-162041-059-startup-cartoon-redesign` and `20261016-163251-368-startup-flat-concepts` archive batches. Each batch contains the repository archive tool's integrity/recovery manifest.
