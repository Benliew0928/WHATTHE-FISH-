# Golf Tornado and Crab Crossing

The random pool now contains Tropical Wind, Sticky Green, Bounce Party,
**Tornado** and **Crab Crossing**. A new event starts every **60 seconds**,
with the existing ten-second warning. Each event lasts **45 seconds**.
Island Rush retains its separate first-finisher timing. A seeded shuffle bag
covers all five events before repeating, with no repeat at bag boundaries.
A round that finishes early can end before all five appear; five starts need
five minutes. The first-finisher rule is unchanged.

One tornado roams near each unfinished player's live ball, with a 3.5-metre contact radius and warning rings. A nearby live ball
spirals upward for 1.1 seconds, then flies toward a checked dry landing point
preferably 14–20 metres from its pickup location. Narrow fairways use a shorter
checked throw rather than ignoring a ball in contact. Each ball can be caught only once by
that event, including after a recovery. A held ball cannot be aimed or hit.
Interrupted events release it safely back to normal physics.
The host chooses dry, cup-safe random waypoints within 28 metres of that ball,
not a fixed fairway route. Acceleration and per-axis limits scale with the shared
eight-second growth: 18% initially, rising to 6 m/s sideways and 5 m/s forward
at full size. On a distant shot/reset, the old funnel fades out and a new position
fades in near the ball; it does not visibly rush across the map. Invalid spawn
areas wait for a valid dry point. Finished/DNF players have no tornado.

Coral crabs scuttle sideways across the fairways, moving their eight legs and
claws. They pursue nearby unfinished balls on their own hole, including resting balls,
at a chase speed of 5.0 m/s, then add 6.5 m/s horizontally and 3.5 m/s upward
to the ball's existing velocity with a stronger claw action.
Each ball has a shared 5-second bump cooldown across all crabs; crab model size stays the same. The crabs are visible hazards rather
than invisible static obstacles. Each crab locks its current eligible target until it
becomes invalid; a newly available ball does not steal that target. After pushing,
the crab drops that ball and retreats for 0.5 seconds toward a checked point up
to 2.5 m away, then selects the nearest eligible non-cooling ball on its hole.
Idle crabs resume patrol and can reacquire the original ball after its five-second
shared immunity expires. Contacts only push the crab's selected target.
Host-owned motion, target/retreat state and per-ball immunity are replicated and
included in host recovery; guests interpolate positions without selecting targets.

Neither hazard adds a stroke, grants points or affects completed players.
Balls within 3.5 metres of any cup are protected, and hazards fade out of those
safe areas. Existing water recovery, ball ownership and cup capture remain.
Illustrated event badges use the existing responsive event stack and native live
timers. No gameplay button is added.

Only the host applies hazard forces. Tornado positions, velocity, waypoints, PRNG state and 6.4 seconds of sand history
are host-owned and replicated at 10 Hz with local render smoothing. Crab pursuit
uses replicated ball positions with local visual smoothing; only host contacts
apply forces. Ball snapshots replicate movement, held state, one-toss immunity
and hazard-flight cues. Network protocol is **85**:
all players need the updated build. A recovered host releases any interrupted
hold while retaining its immunity stamp.

The [editable crab and generator](../ArtSource/GolfEvents/README.md) use two
small skinned LODs, four solid-colour shared materials and no textures.
The tornado follows the [supplied reference sheets](../ArtSource/GolfEvents/References/README.md)
as editable native 3D geometry: grey-blue helical folds, a rolled hollow mouth,
two wind ribbons, 16 entrained leaves, 12 stones and evolving soft ground haze.
The body uses 50% visible opacity; contact haze uses at most 40% and changes
position/shape rather than forming a solid skirt. Overlap can raise accumulated
screen opacity. Four profiles blend over six seconds each, growing 18% to 100%
over eight seconds using shared event age. Catch radius, carry duration, throw,
immunity, crab parameters and scores remain unchanged.

Five ground wind arcs contract inward, with surrounding debris spiralling inward
and upward. Rooted grass/leaves bend toward the nearest two funnels using the
existing foliage shader; trees are not uprooted. Visible sand source coverage
feeds a host-sampled history: grains and soft dust rise progressively over 2.8
seconds. Leaving sand clears the bottom first, then the upper levels. Ground
haze and circles sample terrain height at 5 Hz. The nearest storm adds a smooth
local grey/desaturated Gaussian depth blur over 14 to 2 metres; overlay UI stays
sharp and camera settings restore on exit. Proximity tint follows a critically damped
0.6-second response and each funnel's relocation fade. The post path warms outside
the visible zone and holds through the event; Gaussian CoC/radius and colour
values fade continuously from neutral instead of switching at a volume threshold. Both pipeline generators retain the
URP postprocess resources; passes are enabled only while the local effect is
visible. Phone performance has not been measured.

Six shared-material mesh layers retain their buffers; no per-leaf objects,
colliders or new raster textures are added. Existing 70 m culling remains.
Reduced Motion freezes orbit/profile cycling, disables foliage pull and reduces
camera effect intensity, retaining gradual entry growth. Original supplied PNGs
are retained outside the Unity project; a live 3D asset cannot be a pixel-identical
copy of a 2D illustration. The runtime mesh/material authoring render can be
regenerated using the Golf/Render tornado art preview editor menu.
The generated badges and vapor sprite total 411,125 source PNG bytes; their
[masters, exact prompts and AI disclosure](../ArtSource/GolfEvents/IMAGE-PROMPTS.md)
are retained. Golf's distant cloud ring is centred farther from the course and
scaled down; shared travel scenery restores on exit.

Development Windows testing: start a match and press **F8** for Tornado,
**F9** for Crab Crossing, or **F7** to clear events. These shortcuts are absent
from the Android release. See [other event shortcuts](GOLF-EVENT-HOTKEYS.md).

## Course, camera and awards

Hole 1 always opens the round. The host shuffles holes 2–5 once per round;
everyone shares that order and visits each hole once. The route is replicated
and retained across host recovery. Flag numbers identify physical holes, while
the leaderboard counts completed holes separately.

The aim badge projects the real cup rather than a raised world point. Targets
behind or too close to the camera use a downward side cue rather than pointing
into the sky. A short screen tap skips shot/hazard flight follow and smoothly
returns to the golfer; the actual ball continues moving. Dragging remains look
input. Golf and Fishing awards share the existing court character models, team
poses, winner pose, responsive title and Show results control. The twelve-second
sequence supports skipping and Reduced Motion. No extra character art is added.


## Parameter tuning delivery - 24 October 2026

Windows candidate and Android builds succeeded; no gameplay, multiplayer or
physical-device tests were run. The complete player is now in `Builds/WindowsFinal/`; the previous player
was archived in `Legacy/20261024-163328-314-golf-hazard-tuning-player/`. APK: 113,325,786 -> 113,320,318 bytes, headroom 886,679,682.
Current source hashes, logs and package checks: ignored `Builds/GolfHazardTuning/`.
Measured details and delivery status: [verification](VERIFICATION.md).
Old APK: `Legacy/20261024-161433-404-golf-hazard-tuning-apk/`.


## Tornado art delivery - 24 October 2026

Sideways peak is 6 m/s; forward/backward remains 5 m/s. The supplied shape and
growth sheets guide the native hollow-funnel geometry and four morphing profiles;
eight-second entry growth is cosmetic only. Actual mesh/material authoring
renders were reviewed. WindowsFinal and AndroidSubmission compiled successfully;
no gameplay, multiplayer or phone tests were run. APK: 113,320,318 -> 113,331,282
bytes; headroom 886,668,718. Evidence: ignored `Builds/GolfTornadoArt/`.
Old player/APK/preview drafts: `Legacy/20261024-175949-250-golf-tornado-art/`.
See [measured release details](VERIFICATION.md). Perspective, lighting and native
mesh shading differ from the 2D art; pixel-identical reference matching is not
claimed.
