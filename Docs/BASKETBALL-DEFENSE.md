# Basketball defense

Basketball has automatically alternating squads, assigned by stable network
join ID. The carrier's whole team sees attack controls; the other team sees
defense. A loose or released ball shows defense by default. Only the carrier
can shoot/pass, and teammates cannot steal, block or pressure one another.
Released-shot/pass defense still excludes the originating squad. Free roam
and travel clear inputs. Scoring remains shared court practice, without foul
penalties or a timed team match. See [team controls and maps](SPORTS-HUD.md).

## Controls

The button positions stay fixed and switch handlers with the possession role.

| Slot | Attacking team | Defending team |
| --- | --- | --- |
| Main / E | Hold Shoot; up Dunk, down Layup | Tap Block; drag up and release for Jump Block |
| Secondary / Q | Hold Pass, aim with camera, release to throw | Hold Guard |
| Third / F | Hidden | Hold Steal |
| Jump / Space | Ordinary jump | Immediate Jump Block |

Shoot and Pass are hidden from defenders. Steal, Block and Guard are hidden
from the attacking team. During loose-ball recovery the defensive slots remain
visible; actions requiring possession are disabled. Movement, jump, camera
and court controls remain available. A finger belongs to the action it started:
role changes cancel that gesture, and releasing it cannot activate a newly
assigned button. The block gesture tracks its original finger outside the
button; guard and steal holds cancel when dragged off. Focus loss, disabled
controls and session changes clear input.

The [shared pass lane](BASKETBALL-PASSING.md) shows the carrier's direction and
charge to all players. Defenders can use that cue to anticipate an interception.

## Guarding and pressure

Hold Defend to face a nearby carrier and strafe at 3 m/s. Guarding overrides
sprint, including a fully pushed touch stick. Turning is limited to 360 degrees
per second, and facing assistance acquires the ball within 3.5 m and a forward
area. The player controls every step; the guard never moves automatically
toward an opponent. The camera remains independent. A block started while
guarding commits the body's facing, so moving the camera does not pull the
reach away from the opponent.

Body collision contains a drive. It does not apply a proximity slowdown,
teleport, possession transfer or automatic stun. Guarding pressure narrows the
ordinary shot's green half-window from 0.045 toward 0.025. Pressure combines
distance (0.75–2.4 m), facing and position between shooter and hoop. A wall
removes pressure, and the strongest valid defender determines the capped
value. Perfect 0.65 power remains accurate. The host records pressure alongside
charge phase and freezes the tolerance at the bounded release timestamp; the
replicated meter displays the authoritative pressure.

Release uses the timestamp of the displayed charge sample when that sample is
ahead of the client's buffered server clock. The host still bounds the request
to its own history; a visible green release is not evaluated at an older phase.

Unblocked finishes retain their existing rules: a dunk uses its guaranteed
trajectory and a layup uses the single 85% host roll. Neither receives a second
random defense roll.

## Blocking and recovery

Standing Block loads and reaches through a committed point. Its active window
is 0.12–0.30 s, with 0.70 s total recovery. The current dribble or incoming
flight is predicted once at the start; the hand does not chase subsequent
changes. Low dribbles may be knocked loose when exposed, while the body shields
the far side. Steal retains its separate low swipe and dribble-exposure rules.

Jump Block has a 0.16 s load, a 2.20 m physical root rise, an active hand window
of 0.22–0.88 s and a 1.30 s action. This stylized jump reaches the existing dunk
height. It uses gravity, bounded air steering and ordinary ceiling/floor
contacts. Takeoff carries movement but has no target-seeking displacement.
Early or late jumps can miss. Standing, jumping and stealing cannot cancel
one another to bypass recovery.

The host sweeps the ball and the blocking palm between physics samples using
the 0.15 m ball radius and 0.12 m hand radius. Contacts require the active
window, actual height/reach, a forward approach, and a clear path past scenery
and player torsos. Rig-calibrated shoulder positions and arm lengths bound the
same palm path used by the visible pose. Clients cannot supply a success flag,
target victim or outgoing velocity. A play identifier rejects stale requests;
simultaneous contacts resolve by earliest contact and then client ID.

Networked physics extrapolates the basketball clock from the last synchronized
rendered frame for each fixed step. This keeps the active hand window, carried
ball and physical jump together when Unity runs several catch-up steps during
a slow frame; the rendered pose still uses the synchronized network time.

Before release, a block cancels the queued shot or finish and releases the
ball at contact. The attacker recoils and continues falling under gravity.
After release, a block deflects the existing ball; the shot remains live and a
ricochet can still score. A block does not award possession. Pickup waits at
least 0.34 s; a dispossessed carrier waits 0.70 s, and both participants must
finish their action recovery. A blocked gather does not create a shot attempt.

Released shots descending inside a 0.65 m horizontal radius above the rim's
full-ball clearance plane are protected from late swats. Carried dunks remain
blockable before release. Scoring processes flight up to contact first, so a
completed basket cannot be erased by a later contact in the same physics step.
This is an arcade protection rule, not a full goaltending referee.

## Motion and validation

The motions reuse the existing rig and procedural IK: a lowered guard stance,
short lateral steps, shoulder-led standing reach, one raised blocking hand,
balancing off-hand, takeoff load, landing absorption and a high-ball recoil.
They add no imported animation, mesh, texture, material or runtime package.

The 7 October refinement fits the large-head, short-arm rig. Guard hands sit
below the face. A high contest rises outside the cheek with the other hand
lowered; it does not sweep inward across the eyes. Reach is measured to the
wrist before adding the palm offset. Hands turn with the body, and a stable
rearward elbow guide avoids bend-direction flips. Recovery blends completed
local poses into the basketball running/crouching pose. Moving takeoff and
interrupted finishes retain the starting feet; those foot poses are replicated
so guests do not snap to a generic jump stance.

A defender must put the outside hand into the shot's path. A ball behind the
defender's head is not within that hand's reach simply because it is close to
the player's centre. The active windows and contact radii remain unchanged.

- [Rules and host contact resolution](../Game/Assets/_Game/Sports/Basketball/BasketballDefense.cs)
- [Guard, block and jump poses](../Game/Assets/_Game/Sports/Basketball/BasketballDefenseMotion.cs)
- [Input ownership](../Game/Assets/_Game/Shared/Player/BasketballDefenseInput.cs)
- [Context buttons](../Game/Assets/_Game/Shared/UI/BasketballDefenseButton.cs)
- [Repeatable defense review](../Tools/Build/Test-BasketballDefense.ps1)

Run the defense review offline at 20/30/60/120 FPS and with `-Mode Local` for two
processes. `-PlayerPath` selects a task candidate; omitting it uses the complete
WindowsFinal player. Captures and reports are written under ignored
`Builds/BasketballDefenseQA/`. Existing finish, charge, steal, physics,
gameplay and jump reviews cover regressions. Rooms require matching
**protocol 32** builds. Windows captures do not qualify physical phone touch,
Android appearance/FPS, thermals or internet latency.

Development Windows builds write `defense-geometry.json` next to the player.
This generated test fixture contains head triangle and arm vertex indices for
both existing LODs. The review CPU-skins the character and checks the hands and
forearms against the head surface, including hair, with a 3 mm penetration
tolerance. It also checks joint continuity, grounded feet, hand/contact
agreement, guard transitions, turns, running jump blocks and blocked airborne
dunks. Front, side and oblique captures accompany rendered offline runs.
Use `-PassOnly` for the added pass hold, push and cancel surface checks.
The fixture is external to the game data and is not included in the release
APK. Keep it with a development Windows player when running this review.
