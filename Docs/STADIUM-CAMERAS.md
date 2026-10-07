# Football and basketball stadium cameras

The Camera button (or C) cycles first person, third person and stadium.
Third person remains the default. First person and third person retain their
existing perspective, orbit, distance and obstruction checks.

Stadium mode uses perspective broadcast framing with depth, diagonal field
lines and a smooth pan toward either end. Football uses a high gantry view:
a 40-degree downward angle, 37-degree vertical lens and a gentle 38-degree
total pan across the pitch. Basketball uses a closer courtside view: a
36-degree downward angle, 42-degree lens and a stronger 64-degree pan into
the half-court. The camera stays on the same sideline through changes of
direction; it never flips across the court or tilts the horizon.
At midfield on a 16:9 screen, the rendered views cover approximately 40% of
the football lawn and 53% of the basketball court. Coverage varies with
position, the nearby basket and screen shape.

Movement anticipation and a small nearby-ball bias provide room ahead of
play. Distance eases out during transitions. Football tightens slightly toward
the goals; basketball opens enough nearby to retain both rim and player.
Position, pan and distance use
separate time-based smoothing. Jumping, dribbling height and action aim do
not shake the camera. Teleports and venue changes reset the tracking state.

The camera references football's authored `Lawn__Pitch` bounds and basketball's
painted player boundary. It constrains the perspective ground footprint to
the playing surface and lets the player move off centre near a boundary.
A visibility margin keeps the player inside the picture and above the
bottom controls. Walking beyond the venue continues to follow exploration.
Aspect-ratio compensation retains useful playing space in narrower windows.
Near-sideline basketball players sit lower in the picture; far-sideline
players can move higher, leaving more of the court visible. Nearby basket
influence fades in continuously, including around midcourt.

The movement stick and WASD follow the current camera angle. Right-side
dragging still controls basketball action aim independently. Football kicks
follow the player's facing direction. Switching out restores the prior lens
and projection. Golf, Fishing, menus and group cable-car travel retain their
existing cameras. No art, shaders, textures, audio or network payload are added.

## Visual references

The design takes cues from [Konami's eFootball Duel camera presentation](https://www.konami.com/games/us/en/topics/1756/)
and [NBA 2K Mobile gameplay imagery](https://www.pocketgamer.com/nba-2k-mobile/nba-2k-mobile-introduces-a-new-andre-iguodala-card-alongside-fixing-the-rewards/).
The first shows perspective pitch depth and describes framing that opens for
free play. The second shows a closer, angled half-court composition with the
basket visible in depth. These are visual references, not claims that either
game uses our camera parameters. No reference assets are shipped.

## Validation

`Tools/Build/Test-StadiumCamera.ps1` runs the development player's opt-in review.
It samples actual perspective rays and playing-surface points independently
to measure visible surface and coverage. Centre, edges and corners produce
full HUD captures. A continuous end-to-end traversal records camera motion,
checks player visibility and bounds pan speed, and captures a preview sequence.
The suite also checks screen-relative movement through the real motor,
jump/aim stability, view/lens restoration and outside-field exploration.
Current revision evidence belongs under ignored `Builds/BroadcastCameraQA/`;
the superseded flat-camera evidence remains under `Builds/StadiumCameraQA/`.

Windows captures verify desktop framing. Physical Android appearance, touch
and performance require device testing.


