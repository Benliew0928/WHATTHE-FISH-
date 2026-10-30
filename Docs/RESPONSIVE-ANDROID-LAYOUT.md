# Adaptive Android menu layout

The app measures its current window and `Screen.safeArea` live. Layouts do not
use a phone-model list or a whitelist of resolutions. Window resizing, rotation
and changes to cutout/system-bar insets update placement while the screen is open.

The menu background belongs to the complete overlay canvas and covers every
edge, including outside the control-safe rectangle. Controls use the largest
centred rectangle inside the reported safe area. Reserving the larger inset on
both opposing edges balances the composition for left or right camera cutouts.
Unusual freeform safe regions fall back to their actual rectangle; invalid or
empty safe-area values fall back to the window bounds.

Home uses four sport columns at landscape ratios of at least 1.6, two at
intermediate shapes and one below a 0.8 ratio. Compact headers use two rows.
Wide, shallow sport cards show complete artwork beside labels. Sport selection
uses flexible columns and stacks below a 1.2 ratio. Other menu pages retain
fitted layouts inside the same safe frame. Resizing adjusts existing controls
and preserves page/input state. First-visit highlights follow actual controls.

## Why the sides were visible or blurred

The original island strip came from parenting the menu background inside the
safe-area frame. That left the world camera visible outside it. Full-display
background parenting fixes this leak.

The first welcome adaptation fitted a fixed 16:9 illustration, then extended
unused space with a 32-by-18 copy of the scene. That deliberate downsampling
caused the blurred sides reported by the user. It has been removed.

## Sharp layered welcome

Scenery, mascot, official logo and native controls now adapt independently.
Scenery covers the whole display with proportional cropping. Foreground layers
stay complete inside the centred safe area. Landscape uses mascot/card columns;
tall portrait windows stack logo, mascot and card. Portrait tablet windows place
mascot and logo side by side above the card. Very wide windows retain a centred
content band with more scenery around it. No blurred filler or low-resolution
render texture remains.

Two layers were prepared with the built-in image generation tool from the
approved welcome reference: 1983-by-793 scenery and a transparent 1024-by-1536
mascot. Preserved masters, exact prompts, hashes and relative delivery paths are
in [the responsive art folder](../ArtSource/StartupWelcome/Responsive/README.md).
The unchanged official logo is reused. Unity derives ASTC 4-by-4 Android textures
with a 2048 limit, no mipmaps and clamped edges. New images contain no text;
native controls use the existing English, Simplified Chinese and Malay catalog.
All 16 recorded original-art hashes still pass. The original masters remain.

## Verification and visual review

[Test-ResponsiveLayout.ps1](../Tools/Build/Test-ResponsiveLayout.ps1) exercises
phone landscape ratios from 16:9 through 22:9, 32:9 desktop, the reported
2374-by-1080 shape with a 120-pixel cutout on either side, 4:3/16:10/3:2 tablets,
a foldable with top/bottom insets, square/small windows and four portrait shapes.
The matrix covers 19 window/inset scenarios in all three languages.

Current runs passed **29,215 assertions**: 21,985 primary layout checks, 4,296
additional portrait checks and 2,934 startup/guest/settings/sign-out checks.
They cover foreground separation, actual safe bounds, text clipping, real
pointer raycasts, sharp scenery and its proportions, painted display edges,
loading/retry, dialogs, cold/warm caches and live guide resizing. The unchanged
introduction retains its previous 2,380-check regression evidence.

Evidence is under ignored `Builds/SharpWelcomeQA/`: `Layout/`, `Portrait/`,
`Welcome/` and `Review/Review.md`. The review includes landscape/portrait
comparisons and ten multilingual sheets. Large shapes fit the 1920-by-1200
monitor while preserving ratio and proportional insets: OPPO renders at
1920-by-873, with approximately 97 pixels representing the source 120-pixel
cutout. Actual dimensions are asserted. Narrow portrait views crop more scenery;
their foreground layers reflow rather than fitting a landscape screenshot.

Android gameplay retains its landscape orientation. Portrait captures exercise
resizable Windows windows. No Android device was connected; physical phone
appearance, touch size, native provider sign-in and FPS remain unverified. This
matrix does not certify every manufacturer's inset reporting. Desktop previews
show Huawei; Android chooses its supported provider, including Google on OPPO.

## Measured delivery

The complete **342-file WindowsFinal** player was promoted and every file hash
matched its tested candidate. All **549 Windows / 387 Android** compiled source
documents match the validated snapshot. AndroidSubmission is **170,877,702 bytes**, up **2,663,300** from
168,214,402, with **829,122,298 bytes** of headroom below the strict decimal GB cap.
SHA-256: **CD63695743BE4A14F8E36E0FB1449C5C6B7D0540DCF4EB3F9280BA8048DA8577**.

The APK passes CRC, all 1,412 exact entry-size audit records, five-scene bundle
hash/length, v2 signature, non-debuggable package and development-helper exclusion.
Both sharp layers appear in its resource and packed-asset inventories. Existing
Android Debug signing remains in use. Localization catalog, placeholders and
five font coverage checks pass. A structured Unity YAML comparison verified all
14 golf-ball object/component graphs equivalent before restoring its original
source representation after build generation.

Source checks initially matched the current working tree. Concurrent narration
work subsequently changed `StorybookMenu.cs` and `StorybookMenu.Introduction.cs`
and added `FirstVisitNarration.cs`. This delivery is the validated layout snapshot
and excludes that later work. The two exact tested source files are retained in
`Builds/SharpWelcomeQA/SourceSnapshot/`; each matches its compiled portable-PDB
SHA-256. The subsequently changed development-only `FirstVisitReview.cs` also
has an exact tested copy. `source-snapshot-limits.json` and
`source-verification.json` distinguish
the tested snapshot from the preserved concurrent working files. The 549/387
counts describe compiled documents, not a claim that later narration is included.

The APK targets ARM64 (`arm64-v8a`) and Android API 26 or newer. Window-size
adaptation does not add x86 compatibility for Android PCs.

Previous deliveries are preserved in
`Legacy/20261027-195602-151-sharp-welcome-android/` and
`Legacy/20261027-200157-719-sharp-welcome-windows/`. Archival provides recovery;
it does not save disk space. The retained previous-task candidate was reused
for this full asset build. Current cleanup/retention is recorded in
`Builds/SharpWelcomeQA/cleanup.json`. Earlier blocked cleanup and retained output
are recorded separately in `Builds/ResponsiveLayoutQA/cleanup.json`.
Automatic approval review rejected the reviewed task-only deletion command with
"blocked by policy". No targets were deleted and no workaround was attempted.
The **592,334,532-byte** test-only cache/repeated-capture set remains. Current
sharp-welcome evidence totals approximately **716.8 MB**, including that set.
The reused **662,581,846-byte** candidate remains identical to WindowsFinal;
its previously denied removal was not retried. Existing/shared caches and
rollback archives remain in place. These figures overlap earlier retention
records and must not be added together as independent new copies.
No Git staging, commit or push was performed.

On 28 October the owner separately requested deep folder cleanup and archival.
The reviewed superseded candidate, isolated content caches and repeated captures
listed in both layout cleanup records were then moved to
`Legacy/20261028-114246-642-appgallery-folder-cleanup/`. Current final layout
and narration evidence, source proofs and delivery files remain in place.
The earlier blocked-deletion account above remains a historical record;
the later operation was recoverable archival under the new user request.
See [the current cleanup report](CLEANUP-2026-10-28.md).
