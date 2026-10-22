# Golf cup and event polish

## Cup entry

The physical cup radius is **0.19 m** (0.38 m diameter), about 13% wider
than the previous 0.16815 m radius. The five terrain openings, lining, visible
rim, floor, scene and
environment prefab agree. The editable layout remains
[`FiveHoleCourse.json`](../Game/Assets/_Game/Sports/Golf/FiveHoleCourse.json).
The scoped `GolfCourseBuilder.ResizeCurrentCups` migration cuts the current
terrain and preserves existing tee, slope and bunker repairs. Full generation
also reads the same new layout radius.

Ordinary ground-level shots now drop in as soon as their centre crosses the
visible opening, including off-centre crossings. The ball no longer needs to
slow to a crawl or physically fall below the rim first. Capture accepts up to
**6 m/s** horizontal speed at a centre height up to **1.3 ball radii**. Continuous
segment/cylinder intersection catches crossings between physics steps without
enlarging the visible cup. Very hard shots above that speed can roll across or
rebound. Airborne flyovers, upward rebounds and centres outside the opening
remain in physics. No attraction is applied across the green.

After qualifying entry, a 0.32-second visible descent precedes the existing
authoritative score/next-tee operation. Clients receive the ordinary ball
position snapshots. Scoring, rankings, deadlines, ownership and the wire schema
remain unchanged (matching protocol 77 builds). Recovery, a new strike, leaving
the round and next-tee placement clear the local sink state.

Edit capture speed, inside-rim allowance, entry height and sink duration in
[`GolfBallPhysics.asset`](../Game/Assets/_Game/Resources/GolfBallPhysics.asset).
The capture rule is shared by the fixed-step scan and trigger callbacks.

## Find the target

During aiming, one outlined gold numbered marker identifies the player's actual
current cup, with a subtle cup halo. Offscreen, the same upright number moves
to the safe-area edge and its tip points toward the target. The roaming compass
card is hidden while aiming so it does not compete with this marker. The guide
fades after aiming ends. Other players' targets are not marked.

The native number stays sharp at different landscape sizes. The marker uses
the safe-area frame, reserves the lower control band and has no raycast target.
Reduced Motion removes its breathing animation.

## Event effects and tuning

See [current event rules](GOLF-RANDOM-EVENTS.md). No extra action buttons or
instruction panels are added. Native geometry, existing particle materials and
existing event artwork are reused; no new raster image or 3D model is needed.

[`GolfPresentation.asset`](../Game/Assets/_Game/Resources/GolfPresentation.asset)
exposes marker dimensions/world lift/fade, bounded wind streak count/opacity,
wind compass size, persistent weather edge tint,
edge sweep duration/opacity, contact ring lifetime/radius, squash strength and
event colours. Physics tuning lives separately from these cosmetic settings.

The opt-in development `-golfCupReview` checks the actual guided fast crossing,
slow descent/next tee, current-hole/edge guidance, landscape layout and real
event landings. The shorter `-golfEaseReview` covers the later tuning: hard-shot
overshoot, gentle off-centre entry, next-tee readiness, 45-second weather and
longer wind/contact visibility. Both fixtures are excluded from the release APK. Windows review
does not establish physical-phone FPS, touch quality or WAN synchronisation.

## Stronger feedback and shorter menus

Wind now uses 28 pooled streaks with twice the width, longer downwind motion,
88% peak opacity and a stronger grey-blue border that persists with slow gusts.
A 72-unit direction compass sits below the event badge, clear of its timer.
Sticky Green and Bounce Party add mint/orange edge feedback, thicker ball
outlines and 1.4-second contact rings. Rush ribbons and foot puffs are larger.
Reduced Motion suppresses edge breathing and wind streaks. Physics forces and
event durations remain unchanged by this presentation update.

Home, sport entry, Settings and Friends omit the requested promotional
subtitles. Main titles, mode buttons, navigation and actual rule explanations
remain. Offline Explore retains its existing function and name.

The opt-in `-golfClarityReview` extends the short review with a normal 4.5 m/s
actual crossing, large wind direction and clean Home/Settings/sport screens.
