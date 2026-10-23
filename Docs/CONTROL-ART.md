# Shared control artwork

The approved glossy control family is built from reusable transparent frames,
separate illustrations and live Unity text. Cyan/navy rims, gold action faces,
cyan support faces and a lime Dash face follow the retained Golf and Fishing
artwork. Joystick centres remain translucent so the player can see the world.

## Scope

| Sport | Updated controls | Retained controls |
| --- | --- | --- |
| Basketball | Move, Shoot/Guard, Pass/Steal, Jump, Turbo/Lock-on/Portal | Other menus and match UI |
| Football | Move, Shoot/Pressure, Dash, Pass, Tackle, Wall/Turbo/Lock-on/Portal; wall-placement Cancel | Other menus and match UI |
| Golf | Move | Jump, power, Aim/Cancel, Start, Buggy/Recall, Drive/Exit |
| Fishing | Move, Jump, Cancel, all six round-skill illustrations | Cast/Reel right and Reel left |

This replaces 16 of the 24 editable control presentations. The eight retained
presentations still use the same shared factory as their Layout previews.
The Fishing skills are Steel Wire, Power Reel, Fishing Buddy, Bubble Guard,
Sonar and School Call. Dynamic captions, charge, cooldown, availability,
stamina and ability progress continue to come from gameplay.

## Editing

Change the presentation in [ControlDesigns.Glossy.cs](../Game/Assets/_Game/Shared/UI/ControlDesigns.Glossy.cs)
or the sport builders linked in [CONTROL-LAYOUT.md](CONTROL-LAYOUT.md).
All gameplay HUDs and main-menu samples use these builders. In-game Layout
copies the live graphics. Rebuild and reopen the UI to see source changes in
all three contexts. Saved layouts contain position and scale only.

The retained masters and approved review boards are in
[ArtSource/Shared/Controls](../ArtSource/Shared/Controls/).
`Frames.png`, `CourtIcons.png` and `FishingIcons.png` contain 31 reusable sprites;
`atlas.json` defines their normalized rectangles. Captions are deliberately
separate from artwork so percentages and contextual labels remain legible.

Run `python Tools/Art/prepare_control_art.py` with Pillow installed after editing
the masters or rectangles. The script resolves the repository from its own
location, copies the original RGBA pixels into `Resources/ControlArt`, writes
the runtime manifest and refreshes source SHA-256 records. Commit the masters,
runtime copies, manifests and Unity `.meta` files together. No runtime asset
depends on the original generation location. PNG metadata contains no external
file dependencies; the retained images contain no embedded metadata keys.

Unity loads three shared textures and caches the named sprites. Android uses
ASTC 6×6 with alpha and no mipmaps, matching the retained Fishing artwork's
importer; desktop uses RGBA32. Import size permits the
original atlas dimensions. Existing scene, prefab and equipment assets are
preserved. No legacy artwork has been archived without proving it unused.

## Interaction

Buttons keep fixed transparent touch targets. Press feedback moves only the
shared visual group; holding or sliding a finger does not move the hit area.
Disabled art is dimmed, a ready Basketball guard receives a green tint, and
Football Tackle displays its remaining cooldown in the main caption.
The Fishing skill keeps its radial cooldown and native availability rules.

Use `Tools/Build/Test-ControlLayout.ps1` for the four gameplay HUDs and both
Layout entry points, including responsive positions, saved preferences, sprite
identity and real raycasts. Add `-DesignMutation` to verify that edits to the
shared presentation reach every context. Captured Windows frames are visual
evidence, not physical-phone touch or performance validation.

## Validation — 23 October 2026

The full Windows build and rendered review cover all four gameplay HUDs,
in-game Layout and every main-menu Layout page. Normal and deliberate
shared-design mutation runs passed **3,396 assertions** combined, including
sprite identity, dynamic captions, fixed press targets, real raycasts,
1024×768 / 1280×720 / 1600×900 / 1920×900 layouts and saved-layout restarts.
The mutation run changes shared typography and adds a visual marker, proving
that all consumers receive a design edit. The state gallery includes all six
Fishing skills and court action/ability variants.

The current targeted-pass reviews passed **69 Basketball / 70 Football**
assertions. The rendered Fishing gameplay suite passed **2,968 assertions**,
including updated checks for the new Jump and Cancel artwork and unchanged
touch routing. Evidence is in `Builds/ApprovedControlsQA/`; rendered layouts
are in `Builds/ControlLayoutQA/Run-20261023-175201-184/` and
`Run-20261023-180100-199/`. The final Fishing review is
`Builds/FishingGameplayQA/Run-20261023-180100/`.

Two older broad fixtures remain red: BasketballControls reports eight
passing/inbound failures and FootballProps Explore reports five room/match
transition failures. Each produced exactly the same failures and pass counts
on the previous delivered player (151 and 196 passes respectively). The
comparison JSON files preserve both lists. They are not claimed as passing
regressions; no gameplay rule was changed to accommodate those fixtures.

AndroidSubmission passed the strict byte limit, exact ZIP/audit comparison,
five-scene catalog and APK signature checks; see [BUILD-SIZE.md](BUILD-SIZE.md).
After that release build, development-only review checks were updated for the
new artwork and a whitespace-only source cleanup was applied. These changes
do not alter Android gameplay. Windows delivery includes the revised probes.
