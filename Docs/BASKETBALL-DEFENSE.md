# Basketball defense

Competitive basketball uses selected, balanced teams; warm-up uses join-ID squads.
The carrier's team sees attacking controls. Only the carrier can shoot or pass.
Teammates cannot steal, block or apply guard pressure to one another. See
[match rules](ROOM-MATCH-RULES.md) and [team controls](SPORTS-HUD.md).

## Controls

| Touch slot | Attack | Defense |
| --- | --- | --- |
| Main | Hold Shoot; finish/cancel paths | Hold Guard; swipe up and release for Block |
| Lower-left secondary | Hold Pass | Steal; swipe here from Guard to steal immediately |
| Jump | Jump | Jump Block |

While holding Guard, returning to its centre re-arms the left steal path.
A missed steal retains guard during its animation and recovery; another swipe
can be made when the action is ready. Leaving the button does not release the
captured finger. Lifting, cancellation, menus, focus loss, travel or a possession
change clears it. Another finger cannot release it or fire its actions.
Desktop retains Q for Guard, E for Block, F for Steal and Space for Jump Block.

## Guarding and pressure

Guard automatically turns toward the carrier, including from behind, at the
existing 360 degrees per second. Camera aim and movement remain independent.
Within **1.25 m** with clear sight and matching elevation, both players keep
**70%** of their movement speed. Multiple defenders do not stack the slowdown.
At that distance, a guarded steal succeeds after its windup if the opponent
still has the ball and remains close; walls and other intervening bodies still
block it. The ball becomes a physical loose ball with the existing pickup grace.

A guarded ordinary shot's meter advances **30% faster**. The green band's width
stays unchanged. The host subtracts **20 percentage points** from the usual
range/timing chance and clamps at zero. It records pressure with charge history
so a bounded release timestamp uses the pressure visible at that time. The
replicated meter uses the same integrated phase and period.

Unblocked layups and dunks retain their existing finish rules. Unavailable
finishes that fall back to a timed shot receive the normal guarded-shot penalty.

## Blocking and recovery

Standing Block loads and reaches through a committed point. Its active window
is 0.05–0.46 s, with 0.70 s total recovery. The current dribble or incoming
flight is predicted once at the start; the hand does not chase subsequent
changes. Low dribbles may be knocked loose when exposed, while the body shields
the far side. Unguarded steals retain their low swipe and dribble-exposure rules.

Jump Block has a 0.16 s load, a 2.20 m physical root rise, an active hand window
of 0.16–0.98 s and a 1.30 s action. This stylized jump reaches the existing dunk
height. It uses gravity, bounded air steering and ordinary ceiling/floor
contacts. Takeoff carries movement but has no target-seeking displacement.
Early or late jumps can miss. Standing, jumping and stealing cannot cancel
one another to bypass recovery.

The host sweeps the ball and the blocking palm between physics samples using
the 0.15 m ball radius and a forgiving 0.30 m hand radius. Contacts require the active
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
