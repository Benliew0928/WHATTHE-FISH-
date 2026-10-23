# Golf Tornado and Crab Crossing

The random pool now contains Tropical Wind, Sticky Green, Bounce Party,
**Tornado** and **Crab Crossing**. A new event starts every **60 seconds**,
with the existing ten-second warning. Each event lasts **45 seconds**.
Island Rush retains its separate first-finisher timing. A seeded shuffle bag
covers all five events before repeating, with no repeat at bag boundaries.
A round that finishes early can end before all five appear; five starts need
five minutes. The first-finisher rule is unchanged.

Larger tornadoes cross the fairways with a 3.5-metre contact radius and warning rings. A nearby live ball
spirals upward for 1.1 seconds, then flies toward a checked dry landing point
preferably 14–20 metres from its pickup location. Narrow fairways use a shorter
checked throw rather than ignoring a ball in contact. Each ball can be caught only once by
that event, including after a recovery. A held ball cannot be aimed or hit.
Interrupted events release it safely back to normal physics.

Coral crabs scuttle sideways across the fairways, moving their eight legs and
claws. They pursue nearby unfinished balls on their own hole, including resting balls,
then strike them sideways and upward with a stronger claw action.
Each ball has a 3.5-second bump cooldown; crab model size stays the same. The crabs are visible hazards rather
than invisible static obstacles.

Neither hazard adds a stroke, grants points or affects completed players.
Balls within 3.5 metres of any cup are protected, and hazards fade out of those
safe areas. Existing water recovery, ball ownership and cup capture remain.
Illustrated event badges use the existing responsive event stack and native live
timers. No gameplay button is added.

Only the host applies hazard forces. Tornado lanes use the event seed and synchronized room time. Crab pursuit
uses replicated ball positions with local visual smoothing; only host contacts
apply forces. Ball snapshots replicate movement, held state, one-toss immunity
and hazard-flight cues. Network protocol is **79**:
all players need the updated build. A recovered host releases any interrupted
hold while retaining its immunity stamp.

The [editable crab and generator](../ArtSource/GolfEvents/README.md) use two
small skinned LODs, four solid-colour shared materials and no textures.
The tornado uses a curved 1,152-triangle condensation body with animated vapor
shading, up to 64 body puffs, 16 compact cloud puffs and 32 ground-dust particles
per nearby funnel. There is no line spiral. Distant effects are culled;
Reduced Motion keeps a static body/cloud and suppresses moving dust.
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
