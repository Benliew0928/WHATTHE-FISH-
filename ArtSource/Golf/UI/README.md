# Golf HUD reference art

## Leaderboard contour cleanup — 17 October 2026

The current generator isolates `Leaderboard/Title` with a traced alpha outline,
removing the original cream body and duplicate lower side walls. `Columns`
retains the gold pill/lettering while its outside corners are transparent.
The other 12 delivery PNGs and the high-resolution master remain unchanged.
All 14 final sprites are reproducible from a portable path with spaces.
Playing/Finished now retain their source lettering in both UI modes, with
Simple/proportional rendering. DNF retains its cleared badge/live label.
Previous records below describe earlier deliveries.

## Supplied leaderboard — 17 October 2026

`Leaderboard/Leaderboard-master.png` is the byte-identical 1122×1402 RGBA
original supplied for the new leaderboard. `Tools/Art/prepare_golf_leaderboard.py`
creates 14 proportional sprites in `Resources/GolfUI/Leaderboard` and records
hashes/dimensions in `Leaderboard/delivery-audit.json`. Fixed decorative art
and headings retain source pixels. Reusable row/cell interiors use clean
source strips so real names, progress, strokes and time can update. Compact Playing and DNF use a cleared orange badge with live lettering for
legibility; expanded Playing/Finished retain the original baked words.
Full-resolution art stays outside Unity; delivery uses ASTC 4×4, no mipmaps.
Superseded five reference leaderboard sprites/metas are recoverable in
`Legacy/20261017-140158-238-golf-leaderboard-old-art/` at repository root;
the old HUD master remains editable. Its generator no longer emits them.

## Supplied jump button — 17 October 2026

`Badges/Jump-master.png` is the byte-identical 1402×1122 RGBA user original.
The badge generator also derives `Resources/GolfUI/Badges/Jump.png` at
512×372, preserving all nontransparent artwork and its aspect ratio. Only
transparent padding is trimmed; there is no redraw, recolour or text overlay.
The original remains editable outside Unity. `Badges/delivery-audit.json`
records source/delivery hashes, crop bounds and dimensions. Android uses the
existing ASTC 4×4 importer with no mipmaps.


## New original action images — 17 October 2026

`Badges/AimFirst-master.png`, `Swing-master.png` and `Putt-master.png` are the
three exact 682×682 RGBA originals supplied on 17 October. They replace the
prior action masters; old originals remain recoverable under the reviewed
`Legacy/20261017-002143-064-golf-action-image-masters/` batch at repository root.
Run `Tools/Art/prepare_golf_badges.py` with Python/Pillow from any directory to
produce the proportional 512-pixel sprites and their integrity audit. Current
runtime uses these complete images without a native text overlay; charge
percentage is in the bottom hint. Exit, Your Ball and Start Game retain their
existing artwork. Older records below describe earlier deliveries.

## Shot plate restore and Start Game - 16 October 2026

Exit and Your Ball retain the latest user images. Your Ball shows no distance;
its arrow and near-distance hide behavior remain. Start Game uses the new
original image with baked lettering; no native text overlay. Master: UI art
StartButton/StartGame-master.png; regenerate with
Tools/Art/prepare_golf_start_button.py (Python/Pillow).

Aim First/Putt/Swing return to Reference/Shot with native text and held charge
percentage on the button. No ShotCaption patch is instantiated. The Shot
generator clears lettering with a smoothly feathered underlying disc gradient
instead of an opaque rectangular clear. The three newer action delivery
sprites are archived outside Unity; their approved masters remain for editing.
Tools/Art/prepare_golf_badges.py now regenerates only Exit and YourBall.


## Original action badges - 16 October 2026

Aim First, Putt, Swing, cart Exit and Your Ball use the five complete
user-supplied PNG designs, including their original lettering. Masters are
in ArtSource/Golf/UI/Badges; run Tools/Art/prepare_golf_badges.py with Python
and Pillow to reproduce transparent-padding crops and proportional 512 px
delivery images. PNG hashes and alpha bounds are in Badges/delivery-audit.json
under the UI art directory. No redraw or new font/material/shader/package.

GolfHUD swaps badges using existing Aim/shot/cart state. Charge percentage
is shown in the bottom live control hint, preserving the baked badge text.
GolfTargetIndicator keeps original Your Ball artwork and displays live distance
below it; unchanged arrow and 1.5/1.8 m caption visibility thresholds remain.
Only presentation changes; existing controls and V5 hold-charge rules remain.


## Original cart buttons - 16 October 2026

The user's exact Drive, Buggy and Recall PNG masters are retained in
`CartButtons/`. Runtime uses their complete icon, lettering, border, leaves
and shading. No caption overlay or procedural steering icon is used for these
three states. Exit retains the previous native-label fallback.

Run `python Tools/Art/prepare_golf_cart_buttons.py` from the repository (or
invoke the script by path from any working directory). It trims fully
transparent margins and downsamples proportionally with premultiplied alpha
to 512 px width, with two transparent pixels of padding. Masters are unchanged.
`CartButtons/delivery-audit.json` records hashes, sizes and alpha crop bounds.
Three delivery PNGs total 400,224 bytes; raw Android ASTC 4x4 totals 282,624
bytes. Neither number is compressed APK growth. Masters remain outside Unity.
The older Reference/Buggy artwork remains needed for Exit and its generator.


## Current approved design — 14 October 2026

[ApprovedHUD-reference.png](ApprovedHUD-reference.png) is the user's complete
reference. It stays outside Unity. The current design uses its original panel,
icon, gloss, leaf and default-lettering pixels rather than generated redraws.
Only separate UI crops ship; the reference gameplay background does not ship.

Run `GolfReferenceArtBuilder.Prepare` in Unity to rebuild the delivery sprites,
or `GolfReferenceArtBuilder.PrepareAndBuild` for a complete review player.
The generator resolves source paths from `Application.dataPath`, records alpha
contours and caption patches in code, and creates independent sprites under
`Game/Assets/_Game/Resources/GolfUI/Reference/` with stable `.meta` files.
[reference-delivery-audit.json](reference-delivery-audit.json) records source and
delivery hashes, dimensions, PNG bytes and raw ASTC block bytes. Those numbers
are not actual compressed APK growth.

Default fixed headings and Aim/G, Buggy/R, Aim first, Leave island and walking
hint lettering retain original pixel patches. Live station/hole/ball distances,
player names, table values, results, Cancel, Recall/Drive/Exit and charge values
use the existing Cove fonts above clean caption interiors. Runtime values never
come from the screenshot. Nine-slice board/row artwork expands for more players;
fixed buttons preserve the original aspect. Safe-area layout moves frames and
caption patches together. Decorations do not receive pointer events.

Reference delivery uses original crop resolution, no mipmaps, no CPU
readability, clamp and bilinear filtering. Windows uses RGBA32; Android uses
ASTC 4x4 to preserve thin outlines and lettering. No new font, shader or package
is added. PNGs have no external file dependencies and use existing Git LFS rules.

## Retained historical masters and countdown

`GolfShotBadge-master.png`, `GolfCourseBadge-master.png` and
[GENERATED-PROMPTS.md](GENERATED-PROMPTS.md) retain earlier approved artwork and
provenance outside Unity. Their two superseded runtime PNGs/metas are archived;
they are not shipped by the current design.

`Tools/Art/prepare_golf_ui.py` now regenerates only the unchanged countdown
sprites from their retained masters. Run it with Pillow; paths resolve from the
script location. [delivery-audit.json](delivery-audit.json) covers those countdown
assets. It does not recreate superseded course/shot illustrations in Resources.
