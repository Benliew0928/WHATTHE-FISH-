# Golf event badges and T1 corridor seams

## Reference art and live time

The unmodified supplied transparent badge sheet is the active source at
`Game/Assets/_Game/Resources/GolfUI/Events/BadgeSheet.png`. Its active regions supply
Tropical Wind, Sticky Green, Bounce Party and Island Rush. The Moon Golf region
is retained only inside the shared source PNG and is no longer selected. Artwork,
event names, stopwatch illustrations and decorations use those original pixels.
The baked seconds are replaced with a sampled empty gradient and the game's
outlined live font. This permits every countdown value without baking new images.
The source sheet is retained at its original resolution on both build targets.

Badges appear at the top left, 12 HUD units beyond the actual right edge of
the Settings button (converted between their independently scaled UI spaces).
The event warning, active weather and Rush share this placement; width is
clamped to the available page space on resize. Concurrent weather and Rush occupy separate rows;
the finishing clock docks below the visible stack after its existing central
intro. Finishing numeral size stays 50 by 50 reference UI units, with proportional
seconds suffix. The separate wind-direction arrow is removed. Random Swap has been removed in the current event revision. Timers use the existing host deadlines;
the badge implementation itself does not change scoring, ranking or ownership.
Current event physics and protocol are documented in [event rules](GOLF-RANDOM-EVENTS.md).

## Local corridor repair

Only the existing annotated T1 entry-corridor mask is repaired. The paving and
border meshes share matched seam heights; duplicate projected triangles are
removed. Path vertices retain a small ground clearance to prevent intersection
and depth flicker. Native materials and UVs remain in place. Regeneration applies
the same repair after restoring the original corridor triangles.

Before/after near and overhead views, the duplicate-face report, actual player
UI captures and focused runtime results are retained under ignored
`Builds/GolfEventBadgeQA/`. No phone quality or frame-rate claim is made from the
Windows views. No Git publishing is performed by this task.

## Delivery evidence

Historical badge revision: 142 offline, 32 Host and 35 Client assertions passed.
The complete WindowsFinal player matches the tested 328-file candidate. That revision
APK was 98,078,070 bytes. See [verification records](VERIFICATION.md) for package
measurements, earlier badge evidence and recoverable archive batches.

## Ten-second warning

`Game/Assets/_Game/Resources/GolfUI/Events/WarningSheet.png` is the unmodified
1983 by 793 supplied reference. Static decorations and wording use source pixels;
the baked counter area is covered with an adjacent cream strip and live outlined
orange/yellow seconds. The source artwork is not redrawn. The card shares the
event stack and is replaced by the active event badge at its Host start deadline.

All warning values from 10s through 01s use the same rounded game font, warm
gradient and white outline, including 03s. There is no baked-counter exception. Runtime trail
geometry follows the reference's cyan/white ribbons, gold wisps, green leaves
and sparkle motifs. Visual evidence is in `Builds/GolfRushFiveQA/ValidatedOffline/`.

## Warning-to-event texture binding fix

`GolfEventBadgeGraphic.Sheet` invalidates the Canvas material and geometry when
the atlas changes. Previously only UV geometry changed, so the Canvas could
keep sampling the warning PNG after switching to an event crop. The badge index
now invalidates its own geometry; the row resets its cached timer on type changes.
Original image assets, physics, schedule, scoring and map content are unchanged.
Focused countdown-style, repeated atlas-transition and rendered comparison
evidence is retained under ignored `Builds/GolfEventHudFixQA/`. Protocol is now 76 after Moon Golf removal.

HUD correction delivery: 88 focused checks passed across two viewport sizes.
Warning-to-event rendering agrees with an explicitly refreshed Canvas, and all
four event types plus Rush use the event atlas after the warning. Complete
WindowsFinal updated; fresh APK is 98,087,150 bytes. See current
[verification records](VERIFICATION.md) for audit and recoverable archives.

Moon Golf is no longer a playable or selectable event. Its unused source-image
region remains inside the shared atlas. Current event pool and protocol 76 are
documented in [event rules](GOLF-RANDOM-EVENTS.md).
