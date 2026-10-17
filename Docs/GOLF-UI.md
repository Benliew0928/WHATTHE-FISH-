# Charge and contact presentation — 17 October 2026

Golf now has charging ring/loop/ready cue, subtle strong-charge Swing edge glow,
local camera feedback and actual contact/landing/cup sound/particles. Older
entries below describe their prior absence. Settings and behavior are in
[GOLF-FEEDBACK.md](GOLF-FEEDBACK.md). Compact/hidden-Aim ranking, top hole card,
edge-arrow clearance and touch targets remain unchanged. Charging hint is now
`Power xx% • Release` so the percentage fits the existing sticker clearly.

# Aim target card and ranking visibility — 17 October 2026

During Aim, the existing hole/distance card scales uniformly to 72% and sits
12 UI units below the safe-area top edge. Existing measured glyph spacing and
approved artwork are preserved. The large central direction arrow is hidden.
When the cup target is inside the camera viewport, there is no direction arrow.
Offscreen/behind-camera targets use a 42-by-44-unit arrow at the safe-area edge.
The top-centre card and lower shot/Cancel/charge-hint band are kept clear;
below-view targets use a side edge above the lower controls. Exiting Aim
restores the normal card scale, position and direction arrow.

The entire leaderboard root is inactive during Aim. No compact personal rank,
completed-hole count or stroke summary is displayed or receives input. Ranking
ordering and data continue to update; leaving Aim restores the usual ranking.
Important final countdown and match-finish notices remain separate and visible.

Inspector: `Game/Assets/_Game/Resources/GolfUILayout.asset` fields
`aimTargetScale` (0.72), `aimTargetTopMargin` (12), `aimEdgeArrowSize` (42,44).
No map, physics, shot power, progression or other sport changes are introduced.

Audio inspection: Golf currently has no dedicated charge/swing/putt/contact/
landing/cup AudioSource or sound hooks; the Golf scene/prefabs contain none.
Generic menu taps and other sports have separate sound systems. Missing Golf
shot sounds are an unimplemented feature, not a Windows-only audio limitation.
This UI task does not add sound effects or alter global audio preferences.

# Golf aim focus — 17 October 2026

Entering Aim fades the course/station badge, movement joystick, jump, cart,
leave-island/start controls, local ranking summary and owned-ball marker over
0.25 seconds. Their touch targets stop receiving input immediately, including
while fading. Existing control availability remains authoritative; restoring
opacity cannot enable an unavailable action. The look region remains active.

The hole/distance indicator, shot preview, Swing/Putt, Cancel and live charge
hint remain visible. Important final-countdown/finish notices remain visible.
Both Swing and Putt use the same presentation. Cancelling or releasing a valid
shot restores the auxiliary HUD; automatic aim invalidation uses the same path.
Reduced motion skips the fade. Rapid re-entry reverses from the current opacity.
No shot rules, forces, camera, course, ranking data or other sport modes change.

Inspector: `Game/Assets/_Game/Resources/GolfUILayout.asset`, **Aim focus /
Aim Focus Fade Seconds** (`aimFocusFadeSeconds`, default 0.25; zero is immediate).
This pass adds no charge ring, edge glow, shake, particles or sound.

# Leaderboard border, status and hover polish — 17 October 2026

The gold column band retains its original labels and shape, with transparent
outside corners instead of a cream rectangle covering the blue frame. Title
art has its illustrated silhouette isolated from the sample body's cream fill
and duplicate side walls. The expanded title occupies 90% of panel width,
with a single shared outer body frame beneath it. Content inset is 20 units;
header join is 0.8 of the title height. This removes the stepped overlap.

Playing and Finished use the same complete source badge in compact and full
views, with Simple/proportional rendering. Playing is no longer nine-sliced
into a pointed shape; DNF uses the cleared orange source badge and live text.

Hover on any expanded-panel control animates the common board root to 1.018
scale, including title, frame, rows, labels and scrollbar. Leaving restores
1.0; compact/aim views do not hover-grow. Ranking child buttons have their
independent CoveFeedback disabled so the title cannot enlarge on its own.
The hover honors reduced-motion preferences and resets on focus loss.

Inspector: `GolfUILayout.asset` fields `boardPadding` (20),
`boardExpandedTitleFraction` (0.9), `boardHeaderJoin` (0.8),
`boardHoverScale` (1.018). Existing 64-unit preview rows and compact/expanded
height bounds remain. Regenerate artwork with
`python Tools/Art/prepare_golf_leaderboard.py`; source pixels remain in the
unchanged high-resolution master. Previous delivery records follow.

# Compact leaderboard proportions — 17 October 2026

The preview no longer flattens its column image or squeezes row heights.
The column band uses the source sprite aspect ratio, preview rows are 64
reference units (previously 40), title width is 320 (previously 230), and
preview text is larger. The close/expand footer is 36 units high.

A crowded preview scales the entire card uniformly to fit its configured
height limit; artwork keeps the same proportions in both axes. The compact
height ceiling is now 44% of the safe canvas. One-player cards stay at full
scale, about 279 units tall on the standard reference canvas. The default
first-three-plus-local selection, full scrolling ranking and aim summary are
retained. On shorter landscapes, the existing utility-button layout moves
Jump left when needed to keep the taller card clear of its touch target.

Adjust `boardPreviewRowHeight`, `boardCompactTitleWidth`,
`boardCompactHeightFraction` and `boardToggleHeight` in
`Game/Assets/_Game/Resources/GolfUILayout.asset`. `RenderedHeight` reports
actual card height after its uniform scale, for presentation checks. No art,
texture, font, material, map, scoring or ranking-rule change is required.
Previous delivery records follow.

# Supplied leaderboard style and compact ranking — 17 October 2026

The crown/title, blue/cream frames, column headings, row/cell skins, four
portrait decorations and orange/green status badges are extracted from the
supplied original. Dynamic player data is native UI; sample names, scores
and progress from the picture are not used as gameplay data. The complete
editable original is `ArtSource/Golf/UI/Leaderboard/Leaderboard-master.png`.

- Default: first three entries in the existing `GolfMatchState.Results()`
  order, plus the local player when outside those entries, without duplication.
- Click the title or **View all**: fixed-height full ranking. Mouse wheel and
  touch drag scroll its clipped content; the footer or outside backdrop closes
  it. The backdrop intercepts underlying UI touches while open.
- Aim: show only the local ordinal, progress and total strokes in a small
  summary. Leaving aim restores the compact list. New rounds also reset the
  list to compact. Match scoring/ranking/timers remain authoritative gameplay.
- The row pool grows to fit the supplied roster; it has no ten-player display
  truncation. This does not change room/player limits.

Inspector configuration: `Game/Assets/_Game/Resources/GolfUILayout.asset`.
`boardCompactHeightFraction` defaults to 0.34, expanded height to 0.78;
`boardExpandedWidth` is 720, compact title width 230, preview row height 40,
full row height 82, toggle height 32 and aiming summary height 58. Compact
rows adapt within the height cap; the full list scrolls instead of growing.
Sizes are canvas reference units. Portrait art is a decorative source icon,
selected consistently by player ID; it does not change player appearance.

Run `python Tools/Art/prepare_golf_leaderboard.py` with Pillow to reproduce
the 14 delivery sprites and integrity audit. Blank source strips replace
sample-data regions in reusable skins; original fixed title, column heading
and full-size Playing/Finished lettering are retained. No new font, shader,
material or package is required. Previous delivery records follow.

# Current Golf target and jump controls — 17 October 2026

Golf removes its camera-switch button; other sports and camera look remain
unchanged. Jump uses the complete supplied original image with proportional
fitting, native fixed touch target and the existing press feedback.

The target caption uses separate Hole, dot and distance text runs. Visible
glyph bounds determine equal gaps on both sides of the dot. All runs share
a font size and shrink together for longer distances. The flag retains its
original artwork. Adjust `targetTextLeftPadding` (100),
`targetTextRightPadding` (18), `targetCaptionGap` (12), `targetFontSize` and
`targetSize` in `Game/Assets/_Game/Resources/GolfUILayout.asset` (the Golf HUD
layout Inspector). Distances are canvas reference units. The upper Hole card
continues to show distance while the central start countdown remains intact.

Previous delivery notes follow.

# Golf reference HUD

## Original Aim First, Swing and Putt images — 17 October 2026

The action button now switches between the three newly supplied complete PNGs
using the existing aim and shot-mode state. Their baked wording, illustration,
transparent outline and round aspect are preserved; native action text and
caption patches do not cover them. While holding, live power remains visible
in the bottom control hint. Press, hold, cancel and release use the same button
and existing authority-checked handlers.

The upper target card always displays `HOLE n • distance m`, including startup.
The existing large central 5–1/GO countdown and movement lock remain unchanged.
Masters: [UI/Badges](../ArtSource/Golf/UI/Badges/). Rebuild mobile sprites with
[prepare_golf_badges.py](../Tools/Art/prepare_golf_badges.py); it trims transparent
padding and downsamples proportionally to 512 pixels using premultiplied alpha.
Existing Android ASTC 4×4 import and no-mipmap settings are reused. No new font,
material, shader or package is added. Earlier entries below are historical.

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

Drive, Buggy and Recall now use the three complete user-supplied designs,
including original lettering. GolfHUD swaps proportional simple sprites from
GolfUI/CartButtons by the existing cart state. Native labels are disabled for
these designs; Exit keeps its previous fallback. Touch targets, listeners,
keyboard input, cart availability and gameplay remain unchanged. Editable
masters and regeneration instructions: [UI art](../ArtSource/Golf/UI/README.md).


## Button refinement - 15 October 2026

Start game is a single-line label. Buggy, Recall, Drive, Exit, Aim and Cancel
show words without keyboard hints; keyboard input remains enabled. Drive/Exit
uses a native steering wheel, while Buggy/Recall retains the buggy image.
Camera is above Jump, which remains above Swing in the normal right column.
The ball chip uses its original aspect ratio and native glyph-ink fitting
inside a 192 x 38 box. Below 1.5 m, only the unchanged ball arrow remains;
the caption returns at 1.8 m. This is a UI refinement of the restored V5
hold-charge controls. Evidence: [verification](VERIFICATION.md#golf-button-and-ball-indicator-refinement---15-october-2026).


## Approved screenshot design — 14 October 2026

The current UI follows the user's [original reference](../ArtSource/Golf/UI/ApprovedHUD-reference.png):
cream panels with cyan gloss rims, teal leaderboard crown/leaves, flag target
chip and gold arrow, ball chip and outlined gold pointer, yellow Buggy, coral
Aim, original Jump/Camera illustrations, gold club action badge and blue-rimmed
bottom hint/Leave panels. Artwork is extracted from original pixels. Fixed
headings and default captions retain reference lettering; values remain live.

Only presentation code/assets change. Existing button events, keyboard keys,
aim/charge timing, club pose, cart logic, scoring, authority and world content
remain. Aim displays Cancel/X after entry. Buggy retains Recall/Drive/Exit
states. Swing/Putt and held power remain live. Disabled controls retain their
eligibility and fixed transparent touch targets; decoration cannot intercept
input. Camera/Jump are disabled and cart buttons hidden during aim as before.

The leaderboard expands the same artwork for up to ten players and preserves
all five columns, tied ranks, DNF and conditional final countdown. Local row
highlight and other-player tint remain readable. Hole and ball indicators read
actual local target/owner state; their arrows follow camera bearings. The ball
caption fits its frame and avoids scorecards. No screenshot numbers drive data.

## Editing and generation

Edit `Game/Assets/_Game/Resources/GolfUILayout.asset` for dimensions, margins,
fonts, rows and marker positions. Fixed artwork scales in its original aspect;
board/rows use nine-slice expansion. Caption patch transforms update after
responsive layout and safe-area insets.

`GolfReferenceArtBuilder.Prepare` extracts alpha contours, clears live-caption
interiors and derives small original-lettering patches. Full source reference
and historical masters remain outside Assets. Keep generated PNGs, metas,
configuration and code in Git. See [source and hash audit](../ArtSource/Golf/UI/README.md).
Reference sprites use Android ASTC 4x4, no mipmaps, and no CPU readability.
The previous two illustration sprites were removed after GUID/runtime/generator
reference checks; countdown art remains unchanged. No new font/package ships.

## Focused validation

Build the full saved-scene player with `GolfReferenceArtBuilder.PrepareAndBuild`,
then run `Tools/Build/Test-GolfHUD.ps1 -PlayerPath <player>`.
The opt-in UI review captures single-player reference layout, ten-row scorecards,
Aim/Cancel, held power, landscape ratios and safe-area insets, final countdown
and results. Pointer tests cover button routing/cancel/release because those
controls were reskinned. Synthetic score rows only exercise presentation; this
is not a ten-device network test. Actual rendered screenshots are compared to
the approved art. No map, trajectory or complete gameplay suite is required for
this presentation-only change.

Complete Windows player delivery, focused evidence and Android size/build limits
are recorded in [verification](VERIFICATION.md), [size](BUILD-SIZE.md) and
[packing audit](APK-SIZE-AUDIT.md). Windows rendering does not certify phone FPS.
