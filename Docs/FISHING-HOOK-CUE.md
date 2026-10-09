# Hook bite cue

A gold **!** pops above the biting fish/float, with small cream/gold ripples.
The existing **HOOK!** artwork pulses and a shrinking gold rim shows the actual
remaining hook time. The label and touch target stay fixed and usable immediately.

## Timing and interaction

- The cue starts on the local authoritative `Bite` state, after casting/waiting.
- The remaining time is `hookUntil - FishingGame.Now`, within the existing
  1.8-second window. A late snapshot gets only its real remaining time.
- Repeated snapshots do not replay the introduction. Hook, missed bite, cancel,
  focus/pause, travel, disconnection and round changes clear the cue.
- Expired snapshots disable Hook and show **MISSED BITE** inside the control.
- Strong guidance belongs to the local player. Existing shared float/rod motion
  stays visible to friends; no additional multiplayer message is introduced.

## Layout and assets

Use the existing safe-area frame and action transforms. Project the actual
water-level float through the active camera; hide the world marker when it is
hidden/offscreen while retaining button feedback. Decorations ignore touches.

Small procedural UI meshes reuse the existing UI material. No texture, font,
audio, 3D model, shader or package is added. Original masters and mobile
compression remain. The user chose the Ripple trail concept and then simplified
it to the animated button and fish **!**; the dotted path and flying badge are
omitted. Superseded task reference drafts are archived outside Unity.

Tune `Game/Assets/_Game/Resources/FishingUILayout.asset`: pop duration,
exclamation/ripple size, pulse amount, padding and reduced motion. Dimensions
are canvas design units resolved against the current action/frame scale.
Reduced motion retains the static marker and real deadline rim.

## Readable rows and controls

Scores use solid dark navy lettering without a second outline or synthetic
weight. The complete local row is blue, including when its existing milestone
flame appears; friends use full pale-blue surfaces, with whole-row gold
milestone feedback. Cancel has a full red
surface and a crisp X. Camera and Jump use native camera/jumping-person icons.
Their original input handlers and responsive hit rectangles remain in place.
Icon captions are hidden visually while retaining the existing label references.
These graphics add no imported texture, font, model or material.

Wanted details occupy one line beside the fish illustration. The larger yellow
**+3** has a two-unit navy outline and no badge background. Catch awards float
as orange text without a container, preserving the actual catch/bonus amount.
Player names use the existing heavier Cove Display face; the Lagoon Cup title
uses plain navy lettering without a separate highlighted plate. Catch/tension
labels and orange catch awards have two-unit navy outlines. Camera is 104
units wide and Jump is 120, with rounder ends and skins stretched to their
actual control frames. The camera glyph width has its own editable scale.
Ready, Start and the mode buttons also use full coloured surfaces with navy
captions, replacing the narrow coloured strips behind their lettering.

The layout asset exposes the four label sizes, outline width and utility corner
radius under **Readability and utility controls**. Existing canvas scaling
applies these design units across landscape screens; no screen-pixel placement
or imported replacement artwork is required.

## Manually adjust the timer

In Unity's Project panel, select `Assets/_Game/Resources/FishingUILayout`.
In the Inspector, **Clock Size / Y** controls the outside card height;
**Clock Badge Position / Y** controls the clock illustration;
**Clock Text Position / Y** controls the timer lettering. Smaller Y positions
move the contents down within the card. Dimensions use responsive canvas units.

The current card height is **170**, reduced from 174. Both clock row positions
are **35**, reduced from 39. The card remains anchored to the top of the safe
frame, so the combined adjustment moves the clock row down by two design units.
Crew mode keeps its additional checklist space. The saved values are in
[`FishingUILayout.asset`](../Game/Assets/_Game/Resources/FishingUILayout.asset);
[`FishingHUDLayout.cs`](../Game/Assets/_Game/Shared/UI/FishingHUDLayout.cs)
provides fallback defaults, and
[`FishingHUD.cs`](../Game/Assets/_Game/Shared/UI/FishingHUD.cs) reads the asset
positions during layout. Edit the asset first; changing only code defaults
does not override an existing saved asset. Save and rebuild standalone players.

Delivery measurements and checks are recorded in [build size](BUILD-SIZE.md),
[packing audit](APK-SIZE-AUDIT.md) and [verification](VERIFICATION.md).
