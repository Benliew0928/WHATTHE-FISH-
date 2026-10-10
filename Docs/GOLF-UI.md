# Golf sticker HUD

The selected design combines [A's Golf stickers](GolfUI/Approved-Reference.png),
C's table dividers and B's framed hold/release hint, with the requested **!**
inside its navy badge. This changes presentation only. Five-hole progression,
stroke counting, tied ranks, DNF, the shared final countdown, aim stance,
Swing/Putt selection, power cycling, shot guide, ball physics, club animation,
cart controls and shared island travel keep their existing rules.

## Screens and controls

- The course badge contains the existing station distance. The old duplicate
  floating station caption is hidden on Golf; travel captions remain available.
- The upper-right scorecard retains Player, Progress, Strokes, Status and Finish
  time. Column lines divide the native text. Your entire row is blue; other rows
  alternate cream/mint. Up to ten players use a more compact row height.
- Numeric ranks appear in settled results only. Equal strokes and finish times
  retain shared ranks; DNF remains unranked.
- The existing final countdown uses Fishing's clock illustration. It appears
  after the first finisher, with **A player has finished!**. There is no timer
  before that event.
- The illustrated shot button keeps a live Swing/Putt caption and held power.
  Aim becomes a red Cancel button with an X. Camera and Jump use native icons
  and remain disabled while aiming; carts remain hidden then.
- The framed hint uses a native **!** and **Hold, then release** while aiming.
  Walking/cart/journey captions reflect the existing state.
- Button press/release feedback keeps its previous scale and bounce. A fixed
  transparent root receives pointer events; artwork, text and decorative frames
  do not intercept them. The right-side look pad retains its original reference
  region through responsive anchors.

## Editable layout and delivery artwork

Open `Game/Assets/_Game/Resources/GolfUILayout.asset` in Unity's Inspector to
adjust margins, gaps, frame sizes, table rows, column font sizes, target marker
positions and countdown placement. Controls use anchored layout frames inside
the existing safe area and CanvasScaler. This is a landscape HUD.

Native Cove fonts render all captions and values. Images contain no baked text.
Frames, portraits, Camera/Jump/Close symbols and the countdown clock reuse
Fishing resources. Only two Golf sprites are new: a **256 x 256** shot badge
and a **128 x 128** course badge. Their source PNGs total **110,527 bytes**;
Android uses ASTC 6x6 without mipmaps or CPU readability. The raw texture blocks
total **37,328 bytes**, which is a different measurement from actual APK growth.

[Full-quality masters, generation prompts, hashes and regeneration](../ArtSource/Golf/UI/README.md)
remain outside the Unity project. `Tools/Art/prepare_golf_ui.py` derives the
small runtime PNGs without changing the masters.

## Validation

The 10 October 2026 full Windows player passes **516 assertions across eight
Golf reports**: native HUD/input routing, aiming, offline and local host/guest
matches, plus physical shot/guide behavior. Rendered review covers 16:9, 4:3
and wide landscape with safe-area insets, ten-row fixtures, held power,
conditional countdown and tied results/DNF. See
[the delivery, failed observations and limits](VERIFICATION.md#golf-sticker-hud--10-october-2026).

Build a complete review player with Unity method
`GolfHUDReviewBuild.BuildCandidate`, then run
`Tools/Build/Test-GolfHUD.ps1 -PlayerPath Builds/GolfHUDQA/Player/WhatTheFish.exe`.
The explicit development-only review checks live button routing, charge/cancel
and release, ten-row typography, landscape layouts and safe-area insets, the
conditional countdown and tied results/DNF. Its extra table rows are synthetic
UI fixtures, not evidence of a real ten-device room.

Run the existing Golf Aim, Match and Shot suites against that same player for
gameplay regression checks. Rebuild AndroidSubmission and consult
[measured APK releases](BUILD-SIZE.md) and [packing audits](APK-SIZE-AUDIT.md).
Windows captures do not qualify Android rendering, phone touch or FPS.
