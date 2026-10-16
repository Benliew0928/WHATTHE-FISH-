# Sports home menu

The post-login home uses the official WhatTheFish logo, a navy sports theme, four illustrated sport cards, and separate global utilities. The welcome/loading scene is unchanged.

## Navigation

Home → Fishing / Basketball / Football / Golf → Create room, Join room, Explore offline, or Versus bots. Back returns to Home before choosing a different sport. Create room has a fixed sport; room joining still follows the host's island and conditions.

Customize Venue opens one hub listing all four venues. Football and Basketball open their existing working editors; Fishing and Golf visibly say customization is coming soon. Saved designs remain per sport. Unsaved changes retain the existing discard confirmation and guests cannot edit a host's venue.

How to Play opens a separate four-sport guide hub. Reading another sport's guide does not change the active environment. Profile and Settings remain in the header. The profile retains access to personal football ball trails when Football is selected. Bot play is enabled only for Football and Basketball, matching the existing game systems.

## Implementation

- [StorybookMenu.Sporty.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.Sporty.cs): menu-only palette, native Unity controls, card art, home, sport pages and global hubs.
- [StorybookMenu.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.cs): existing room, settings, profile, authority and customization flows.
- [Art masters and provenance](../ArtSource/SportyMenu/README.md).
- [SportyMenuBuild.cs](../Game/Assets/_Game/Editor/SportyMenuBuild.cs): asset import settings and isolated complete Windows candidate build.

The 1600 × 900 layout scales within the game's safe-area canvas, including wider phones and tablets. Buttons preserve pointer release activation, drag cancellation, keyboard focus, connection locks and reduced motion. In-world UI keeps its existing theme.

## Verification

Build a candidate with Unity batch method `WhatTheFish.Editor.SportyMenuBuild.Windows`, then run `Tools/Build/Test-StorybookMenu.ps1 -PlayerPath Builds/SportyMenuQA/Player/WhatTheFish.exe`. The suite checks rendered layout, card navigation, official branding, input feedback, all guides, editor persistence/discard, bot setup, offline entry and host/guest room readiness and travel. Also run startup and player-identity suites against the same candidate.

Current validation captures and logs belong under ignored `Builds/SportyMenuQA/` and `Builds/StorybookMenuQA/`. Delivery requires a fresh AndroidSubmission build and promotion of the complete validated Windows candidate to `Builds/WindowsFinal/`, with a current build stamp. Windows rendering does not establish Android device frame rate or touch quality.

## Delivered validation — 16 October 2026

The delivered complete Windows player contains the approved 2D sports design and the existing official logo. Full Windows assets were built at 12:35 UTC, with the final C# correction at 12:39 UTC. AndroidSubmission completed at 12:45 UTC and passed its automatic gate. All 432 C#/shader hashes remained stable through Android packaging. The current Windows delivery's 324 runtime/support files match the tested candidate exactly; `LATEST-BUILD.txt` is maintained separately. The concurrent player-nameplate delivery and its evidence were preserved.

- Menu: **1,272 passing assertions** (989 offline, 143 host, 140 guest), including all sport cards, global hubs, text fitting, pointer targets, back navigation, settings, rules, editor discard/save, offline arrival and room readiness/travel.
- Welcome/login: **443 passing checks** across 1600 × 900, wide 1560 × 720 and tablet 1200 × 900, including simulated safe-area constraints and guest profile editing.
- Player identity: **12 passing host/guest checks** and matching replicated names.
- Football bot entry, 2v2–5v5 setup, physical actions, complete unattended match and cleanup: **94 passing assertions**, on the delivered Windows player (`Builds/ProgrammedBotsQA/Run-20261016-204919/`).
- Rendered home, sport pages, venue hub/editor, guides, room forms, settings and profile captures were inspected. Initial disabled renderer tint after login was corrected. The tablet capture checker now uses a prime sampling stride, avoiding a false blank-image result from repeatedly sampling six columns.
- Art regeneration was verified in a checkout path containing spaces, launched from an unrelated working directory. PNGs have no external dependencies; font licensing and Unity import metadata are included. Generated Golf prefab ID/order churn was removed only after comparing every named object, component property and reference in the YAML graph.

Evidence: `Builds/SportyMenuQA/`, `Builds/StorybookMenuQA/Sporty-Final/`, and `Builds/PlayerIdentityQA/Network-20261016-204015-411/`.

**Limits:** no physical Android device check; desktop rendering does not establish phone appearance, touch response or FPS. OS clipboard roundtrip was unavailable in the hidden Windows review; typed room-code entry and validation passed. Fishing/Golf bots and venue editing remain explicitly unavailable.

A broader unattended Basketball bot simulation is **not qualified** by this menu delivery. Its setup, team sizes, physical shot/pass/guard fixtures and cleanup passed, but one run completed without scoring and a quiet repeat scored 10 points then hit its 100-second timeout with 12 match seconds remaining during inbound collection. No basketball gameplay code was changed by this menu task. Diagnostics remain in `Builds/ProgrammedBotsQA/Run-20261016-204107/` and `Run-20261016-204607/`; do not describe those simulations as passing.

Superseded preliminary captures, logs and the temporary portability clone were archived with recovery hashes to `Legacy/20261016-204617-415-sporty-menu-review/`. The previous APK is in `Legacy/20261016-204118-053-sporty-menu-android/`. The concurrent nameplate delivery already archived the superseded complete Windows player to `Legacy/20261016-204255-566-character-nameplates-windows/`.

Later concurrent player-nameplate/HUD edits are being built separately and are outside this qualification. They are preserved in the workspace. These results qualify the recorded menu source snapshot; all menu-task code still matches it. The build stamp records this source-snapshot limit.
