# Cable-car startup welcome

The selected [Cable-car Welcome concept](VisualDirection/StartupWelcome/01-cable-car-welcome.png) now serves as the game's loading and entry screen. The character, teal cabin, tropical boardwalk, logo and cream sign use the approved composition. The [raster master](../ArtSource/StartupWelcome/README.md) preserves the 3D cartoon appearance; text, buttons, loading progress and dialogs are native Unity UI. The illustration is static.

On a normal APK or Windows launch, the screen appears after the platform/Unity splash, before the packaged islands are prepared. The loading card sits immediately right of the character's legs, near the lower edge. Its progress follows actual content preparation and scene-loading stages. It reaches 100% only after the existing main menu is available. The progress card then clears and the right-hand sign reveals the entry buttons without changing the background.

- **Continue as guest** asks for a player name, saves a local guest profile, and opens the existing main menu. Returning guests see their saved name. It does not create an online session. See [player login and Huawei requirements](PLAYER-LOGIN.md).
- **Sign in with HUAWEI** opens a clearly labeled coming-soon dialog.
- **English** and **Settings** open placeholders for later development.
- **Info** shows a short description of the four-sport game.
- A content preparation failure keeps entry unavailable and offers **Try again** on the sign.

The existing main menu and gameplay remain behind an input gate until guest entry. Dialogs also block the underlying entry controls. The complete artwork fits inside the device safe area with coordinated controls; teal margins accommodate wider or taller landscape displays without stretching or cropping the character and sign. The four-island world, room model and shared travel behavior are unchanged.

## Source and reproduction

- [StartupWelcome.cs](../Game/Assets/_Game/Shared/UI/StartupWelcome.cs): responsive artwork frame, native UI, placeholders and input gate.
- [PackagedContentBoot.cs](../Game/Assets/_Game/Shared/Core/PackagedContentBoot.cs): staged progress, content integrity/cache preparation, retry and ready state.
- [StartupWelcomeBuild.cs](../Game/Assets/_Game/Editor/StartupWelcomeBuild.cs): background importer and candidate Windows build. The background uses its original 1672 × 941 dimensions, no mipmaps, clamp sampling and Android ASTC 4×4 compression.
- [StartupWelcomeReview.cs](../Game/Assets/_Game/Shared/Core/StartupWelcomeReview.cs) and [Test-StartupWelcome.ps1](../Tools/Build/Test-StartupWelcome.ps1): development-only runtime review and repeatable Windows test runner.

Run `Tools/Build/Build.ps1 -Target Windows` or `-Target AndroidSubmission` from a checkout to build the current game. The editor method `WhatTheFish.Editor.StartupWelcomeBuild.Windows` builds an isolated candidate under `Builds/StartupWelcomeQA/Player/`. Test a delivered development player with `Tools/Build/Test-StartupWelcome.ps1 -PlayerPath Builds/WindowsFinal/WhatTheFish.exe`.

Explicit `Explore-*.cmd` shortcuts keep their direct-to-sport behavior. Development review/audit flags bypass the welcome after content is ready; `-startupWelcomeReview` exercises the screen itself. These exceptions do not affect a normal phone launch. Huawei authentication, cloud account recovery, additional languages and startup settings are future work; guest names and a local guest ID now persist on the device.

## Validation and limits

The startup review exercises a forced preparation failure and retry, cold and warm caches, real pointer raycasts, all placeholder dialogs, guest entry, restored gameplay input, safe-area containment and text visibility. It captures the actual Unity UI at 1600 × 900, 1560 × 720 and 1200 × 900, including simulated screen cutouts. Runtime logs and images stay under ignored `Builds/StartupWelcomeQA/`.

The original 16 October 2026 welcome-only candidate passed 277 startup assertions, 104 cold/warm packaging assertions and 713 existing-menu assertions. Loading, ready, retry and placeholder captures were visually reviewed. The packaging checks visited all four islands. All 294 compiled source documents matched that source snapshot, and all 325 files of the complete player matched after promotion. Its `AndroidSubmission` build succeeded and passed the automatic APK gate. This candidate has since been superseded by the [guest-profile delivery and its verification](PLAYER-LOGIN.md#guest-delivery-verification--16-october-2026).

Evidence: `Builds/StartupWelcomeQA/Run-20261016-172120-659/`, `Builds/StartupWelcomeQA/Packaging/`, and `Builds/StorybookMenuQA/StartupWelcome/`. The startup runner also passed from a copied script tree containing spaces with an unrelated working directory; 19 local documentation links resolved in the alternate root. PNG inspection verified dimensions, CRCs and integrity hashes. Embedded C2PA provenance, its icon and certificate URLs were retained; they are not external image dependencies.

Build regeneration changed Golf Ball prefab YAML object IDs/order. A format-aware comparison resolved local references by named object/component and proved all 14 objects, fields and external references equivalent before restoring the original YAML. Its generated copy and comparison remain in the ignored evidence folder. Pre-existing bot, pass and HUD changes were preserved; no Git staging, commit or push was performed.

Delivery verification is recorded in `Builds/WindowsFinal/LATEST-BUILD.txt`. Phone rendering, physical cutouts, touch behavior and performance still require a physical Android test; Windows captures do not establish phone FPS. The artwork has no external image/library dependencies. Its recorded master and delivery hashes are in [integrity.json](../ArtSource/StartupWelcome/integrity.json).

The previous complete player and APK are recoverable in the ignored archive `Legacy/20261016-172454-757-startup-welcome-delivery/`. Superseded candidates and their evidence are in `Legacy/20261016-171145-450-startup-first-candidate/` and `Legacy/20261016-171742-949-startup-second-candidate/`; temporary portability copies are in `Legacy/20261016-172306-384-startup-portable-check/`. The validated candidate copy was archived after full promotion in `Legacy/20261016-172816-935-startup-promoted-candidate/`; current evidence remains under `Builds/`. Earlier rejected design drafts remain in the `20261016-162041-059-startup-cartoon-redesign` and `20261016-163251-368-startup-flat-concepts` archive batches. Each batch contains the repository archive tool's integrity/recovery manifest.
