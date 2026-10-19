# Golf mini game

Current owner-only aim/swing policy and circular address refinements: [18 October change record](GOLF-ADDRESS-AND-COUNTDOWN-20261018.md). Historical helper test descriptions below predate this policy.

## V5 controls restored - 14 October 2026

Swing uses the pre-control-redesign V5 implementation: hold to charge, release
to strike. Aim uses the original third-person framing and existing shot guide.
No retained power rail, click Swing or direct camera loft mapping is active.
Automatic Putt, all V5 obstacles/ponds and latest-dry water recovery remain.
Room protocol is restored to **49**, requiring matching builds.

## Circular course — 14 October 2026

The current five-hole layout follows a clockwise route from the southern
entrance, through the west sand/stone garden, northern plateau and east stone
gate, then returns south. See [course coordinates and obstacles](GOLF-COURSE.md).
Authored fairways retain normal fairway resistance; the new shallow sand patch
uses existing sand resistance. Stone and palm obstacles use real kit colliders.
Hole 4 offers a southern gate bypass. Shot strength, charge, rebound formulas,
scoring, match authority and camera control are retained. A full downhill shot
on Hole 4 can travel about 66 m including roll; the regression test envelope
was extended to 90 m without changing the shot model.

Enter Golf offline, or start the host's ready room, then select **Start golf match**. Only the offline player or host can start or restart a match. The existing character movement, three camera modes, golf carts and group island travel remain available. Returning to the lobby or leaving the island clears the round without producing a result.

All five existing numbered aqua flags and cups remain present and open together. Each participant gets one ball at the first existing tee and an independent target starting at Hole 1. Completing a hole places only that owner's ball at the next existing tee; characters continue moving normally. After Hole 5 the player is Finished, their target indicator disappears, and their ball is excluded from further swings.

At match start, each existing ball rests on a small wooden support at its
opening Hole 1 position. The [reference and editable generator](../ArtSource/Golf/Tee/README.md)
define its pointed stem, wood grain and shallow cup. Its static collider
supports the dynamic ball and allows normal putts and lifted shots. The
support stays at the opening position after a shot; later holes use their
existing ground placement. Wrong-hole and outside-course recovery still
restore the last shot position, including the opening cup. Round reset and
island travel clear these supports with the balls. Players share one mesh,
material and tiny texture; no new ball, scoring rule or network field is added.

## Rules and authority

- Complete holes strictly in order, 1 through 5. Cup detection checks the ball's permanent owner and that owner's current target.
- Every accepted swing adds one to the hitter's current-hole strokes and total strokes. Only the ball owner may aim at or strike it; foreign requests are rejected before a stroke is recorded.
- Progress, finish state, finish time and accepted swings belong to the ball owner. Striking another player's ball is not permitted.
- Entering another cup or crossing the shoreline restores the struck ball to its last shot position, stops its motion and preserves all scores. Recovery adds no penalty stroke. The recovery anchor updates whenever its owner hits that ball, and starts at the tee for a new hole.
- The first Hole 5 completion stores elapsed time from match start and starts one shared **30-second** countdown. Later finishers never change its deadline.
- End immediately when all participants have finished, or when that deadline is reached. The latter marks all remaining participants DNF, preserving their completed holes and total strokes.
- There is no global, single-hole, AFK or maximum-duration timer before the first finisher.
- Finished players rank ahead of DNF, by total strokes ascending, then elapsed finish time ascending. Equal strokes and equal finish time share a rank and a Tie marker. DNF remains in roster order and receives no competitive rank.

The host owns all physical simulation, scores and deadlines; offline play uses the same gameplay code. Clients send reliable owner-authorized swing requests with a selected ball, heading, charge and round revision. The host validates the hitter, active ball, range, visibility and revision. Invalid, cancelled or stale requests add no stroke. The ball snapshot is published on its owner's player object, independent of the hitter. The current room protocol is defined in [RoomService.cs](../Game/Assets/_Game/Shared/Networking/RoomService.cs); matching builds are required.

## Shot camera and buggy recall

An accepted Aim recalls the golfer's own buggy, clearing an obstructed ball.
Cancelling or completing the shot leaves the buggy recalled; use Buggy / R
to summon it again when ready. Driving camera rotation stays under manual
control, including turns and reverse; see [cart controls](GOLF-CART.md).

On release, the camera holds the aiming composition through the downswing.
Confirmed ball contact starts a 0.65-second eased transition to the actual
moving ball. The camera follows flight, touchdown and rolling, with a small
velocity lead and terrain/prop obstruction checks. After the ball settles,
it holds the landing for 0.85 seconds and eases back to the character over
0.9 seconds. The player's selected roaming mode and look angle are retained.
Right-side drag/right mouse can still adjust the view during flight.

Rejected or cancelled swings cannot start a ball-follow shot. Recovery and
cup relocation return to the golfer instead of following a teleport; new
rounds, island travel and disabled cameras clear the sequence. Only the local
hitter's requested shot takes over their camera, even when striking someone
else's ball. Guests track the displayed ball snapshots, so they do not rely
on a kinematic Rigidbody's velocity. No new network field or art asset ships.

## Golden shot trail

An actual club contact starts a soft warm-gold ribbon behind the moving ball,
including putts. Its narrow ivory centre sits inside a soft golden edge.
The newest portion beside the ball is strongest; the older tail becomes paler,
thinner and transparent. Each sampled portion lives for **1 second**, including
its fade. After the ball stops, the remaining ribbon fades fully within that
same lifetime. A new swing starts a fresh ribbon, and recovery, cup progression,
round reset or island travel clears it immediately without a teleport line.
Walking, aiming and cancelled swings create no trail.

[GolfBallTrail.cs](../Game/Assets/_Game/Sports/Golf/GolfBallTrail.cs) owns the
default **1-second lifetime** and **0.18-metre ribbon width**; its public
`lifetime` and `width` fields are visible on each running ball in the Inspector.
The head/tail colours and taper are centralized in that script.
[GolfBallTrail.shader](../Game/Assets/_Game/Art/Golf/GolfBallTrail.shader)
provides the soft edge and pale centre, using the shared
[material](../Game/Assets/_Game/Resources/GolfBallTrail.mat). No texture, light,
shadow, physics force or collider is added. The host's ball snapshot includes
a strike sequence; clients start the effect once per actual hit and follow
the existing displayed ball interpolation. Matching host/client builds
are required.

## Controls and display

Use the existing stick/WASD to move and right-side drag/right mouse to look.
Walk within **3 metres** of a visible, nearly stationary ball and tap **Aim / G**.
The host checks a safe side-on stance beside that ball, including ground support,
clearance, the path to the stance and nearby players. A blocked approach cannot
teleport through a wall. When the side stance is obstructed, a safe stance toward
the approach is used instead.

Aim locks that selected ball and plants the golfer's feet. Movement, sprint,
jump and cart commands cannot move the golfer out of range. Drag right/right
mouse to adjust direction. Hold **F** (or the shot button): power repeatedly
sweeps from 0% to 100% and back over **2.6 seconds**. Release on either sweep
when the guide reaches the desired spot. Release uses the last displayed power
and heading. **Cancel / X** (or Escape while aiming) cancels any charge without
a stroke and restores walking. Dragging off the shot button cancels only the
charge, keeping the stance. An accepted shot exits aiming; ball movement/reset,
round end, menu/focus changes and travel clear stale locks. The host enforces the
lock; the UI waits for acknowledgement before enabling the shot button.

The shot button automatically reads **Putt** within **12 horizontal metres** of
that ball owner's current cup and **Swing** farther away. The host fixes this mode
when taking aim. Holding longer changes power, never shot type. Putts use
**0.35–7.5 m/s** along the ground; swings use **3–17 m/s** horizontal speed with
**2.8–8.5 m/s** vertical lift. The authority uses the striking player's own ball-to-cup distance.

A continuous mint/ivory path ends at one pulsing gold target. For Swing this is
**first touchdown**, followed by normal physical run-out. For Putt it is the
**stopping point**. The idle guide previews 30% power. No rollout diagram,
physics readout, extra mode panel, dot trail or distance estimate is displayed.
The existing procedural shader/material and one reusable mesh provide the effect;
no texture, model, audio or package was added.

`GolfShotPlan` sweeps the ball against the actual course colliders in 20 ms
samples, following turf, slope and resistance for putts. The authority follows the
same trajectory shown by the guide, then returns to ordinary Rigidbody physics
at a swing's first impact or a putt's rest. First turf contact retains 78% of
tangential speed, reducing excessive run-out after the higher arc. Other moving balls and gameplay
impulses interrupt planned travel and restore physical collision response; those
future interventions can change the result. Invalid off-course trajectories do
not claim a ground target. Clients use the host's locked origin and mode, and
receive the existing position/rotation snapshots.

The aqua arrow continuously points toward the local player's own target hole. A gold marker shows the location and distance of their own ball, including off-screen directions. The upper-right leaderboard displays player name, completed holes out of five, total strokes, status and finish time. The final countdown remains hidden until someone finishes and displays “A player has finished!”; the first finisher is not announced as the winner. Results display ranks only after settlement.

Starting a live round keeps the normal roaming camera. Aim temporarily uses a
ball-centred shoulder camera from any of the three roaming modes, keeping the
ball and guide visible. Cancel restores the selected roaming mode. Camera
switching during aim is ignored. Driving retains its existing camera rules.
The ball's final LOD stays rendered instead of culling at a small screen size.
The gold ball indicator keeps a screen-space gap above the actual sphere.

Live balls use one authoritative Rigidbody and sphere collider. Characters
(including late arrivals and re-enabled controllers) and carts remain excluded.
`GolfBall` uses Flying, FastRolling, SlowRolling
and Resting. Outside planned shot travel, Flying uses gravity and damping.
All supported slopes receive surface-dependent tangential resistance. Greens,
fairways, rough and sand use 1.15, 3.3, 4.5 and 8 m/s² respectively, with smooth
fringe transitions. Rolling spin tracks the reduced ground speed so it cannot
restore energy removed by turf resistance.
Below the slow threshold on supported ground, additional settling resistance ramps
up as speed falls, and tangential gravity is cancelled. The settling speed ceiling
converges smoothly to its configured band rather than snapping the entry speed.
Normal-to-ground motion is preserved. Below the stop threshold continuously for
the configured delay, the ball zeroes linear/angular velocity, disables its own
gravity and sleeps **while remaining dynamic and collidable**. New swings,
ball impacts or gameplay impulses restart its timers, gravity and normal motion.
Loss of support also restores falling. Host/offline remains authoritative;
clients reuse the existing position/rotation snapshots.

Select [GolfBallPhysics.asset](../Game/Assets/_Game/Resources/GolfBallPhysics.asset)
in the Project window to edit persistent Inspector settings for all match balls:

| Inspector field | Default | Effect |
| --- | ---: | --- |
| Minimum Swing Speed | 3 m/s | Swing horizontal speed at zero charge |
| Maximum Swing Speed | 17 m/s | Horizontal speed at full charge |
| Minimum Swing Lift | 2.8 m/s | Swing lift at zero charge |
| Maximum Swing Lift | 8.5 m/s | Swing lift at full charge |
| Landing Speed Retention | 0.78 | Tangential speed retained at a swing’s first turf impact |
| Minimum Putt Speed | 0.35 m/s | Ground speed at zero charge |
| Maximum Putt Speed | 7.5 m/s | Ground speed at full charge |
| Slow Rolling Threshold | 2 m/s | Enter extra ground resistance below this speed |
| Stop Speed Threshold | 0.25 m/s | Speed must remain below this before rest |
| Stop Delay | 0.5 s | Continuous supported low-speed time needed |
| Settling Max Speed | 1 m/s | Slow-mode speed band; entry converges smoothly |
| Rolling Resistance Strength | 1.8 m/s² | Extra ground deceleration, ramped with slowing |
| Green Resistance | 1.15 m/s² | Ground resistance on every green slope |
| Fairway Resistance | 3.3 m/s² | Ground resistance on the fairway |
| Rough Resistance | 4.5 m/s² | Ground resistance in the meadow |
| Sand Resistance | 8 m/s² | Ground resistance inside bunkers |
| External Velocity Change | 0.2 m/s | Significant new force detection during slow roll |
| New Force Grace Period | 0.12 s | Extra resistance stays off briefly after a fresh impulse |

Keep Stop Speed Threshold below Settling Max Speed and Slow Rolling Threshold.
Increase resistance first if slow rolls remain too long; increase Stop Delay if
rest happens too eagerly. Air damping stays at 0.05 linear / 0.1 angular.
New gameplay forces should call `GolfBall.ApplyGameplayImpulse` (impulse in N·s),
which clears even a nearly complete stop timer before applying the impulse.
Every new valid body contact resets the timer, including gentle ball collisions.
Significant impulses during continuing contacts and direct Rigidbody velocity
changes are detected too; stationary terrain support is not a new hit.

Start testing with a light putt ending on a gentle slope: it should roll, slow,
then remain still for at least 10 seconds. Swing again with high power to verify
immediate release and normal flight. Repeat downhill above 2 m/s to check bounded rolling, and collide
another ball with the resting one. Check both Host and Client views. The course adjustments are described in
[GOLF-COURSE.md](GOLF-COURSE.md); movement, scoreboard and countdown rules remain unchanged.

## Extension points

### Handheld Midnight Iron

Starting a match grants every participant the supplied Midnight Iron club.
The right hand closes around the handle using a local mitten grip shape on both
character LODs. The club follows the animated hand: near horizontal and slightly
downward at low positions, then sloping farther down behind the player when the
hand swings forward and upward. Shoulder and elbow travel retains its gait;
forearm pronation shares grip alignment to avoid a permanently palm-down wrist.
The shaft stays centred in the hand's grip opening. Charging and swinging switch
to two hands, with the grip maintained and
an arm/torso overlay on the existing rig. First person retains the gripping
arms and locally hides the head; returning to third person restores it.
The club is hidden while driving,
travelling or outside the match; restarting reuses one club per avatar.

Input eligibility uses ball proximity rather than the previous hand pose or
the golfer's exact facing angle. The grip adapts to the addressed ball.
Releasing a valid shot starts the backswing-to-contact animation; the host
starts authoritative shot travel at 0.10 seconds. Feet stay planted through contact and
control resumes during follow-through. Recovery lasts 0.74 seconds and prevents
duplicate strokes. Round changes and ball resets cancel queued contacts. Charge,
heading, round, sequence and server start time synchronize the presentation;
protocol 51 requires matching host/client players. Ball ownership, scoring,
cup order and the shared final countdown retain their existing rules.

See [source, regeneration and tuning](../ArtSource/Golf/Club/README.md).

| Source | Responsibility |
| --- | --- |
| [GolfPlayerState](../Game/Assets/_Game/Sports/Golf/GolfPlayerState.cs) | Independent target, progress, strokes, elapsed finish time and Finished/DNF flags |
| [GolfMatchState](../Game/Assets/_Game/Sports/Golf/GolfMatchState.cs) | Pure scoring, one-time countdown, end conditions and ranking, independent of scene/UI |
| [GolfMatchManager](../Game/Assets/_Game/Sports/Golf/GolfMatchManager.cs) | Host/offline authority, roster, round lifecycle, tee placement and replication |
| [GolfBall](../Game/Assets/_Game/Sports/Golf/GolfBall.cs) | Permanent owner, rigidbody, shot anchor, recovery and client pose interpolation |
| [PlayerView](../Game/Assets/_Game/Shared/Player/PlayerView.cs) | Existing input/camera modes and opt-in Golf shoulder framing; no scoring decisions |
| [GolfHoleTrigger](../Game/Assets/_Game/Sports/Golf/GolfHoleTrigger.cs) | Golf-ball-only cup entry and downward swept detection; fly-overs do not score |
| [GolfSnapshots](../Game/Assets/_Game/Shared/Networking/GolfSnapshots.cs) | Authoritative match, player and ball network snapshots |
| [GolfTargetIndicator](../Game/Assets/_Game/Shared/UI/GolfTargetIndicator.cs) | Local target and own-ball indicators |
| [GolfLeaderboardUI](../Game/Assets/_Game/Shared/UI/GolfLeaderboardUI.cs) | Read-only leaderboard, countdown and results |
| [GolfSwingButton](../Game/Assets/_Game/Shared/UI/GolfSwingButton.cs) | Aim/Cancel, automatic button label, cycling charge/release and match controls |
| [GolfAiming](../Game/Assets/_Game/Sports/Golf/GolfAiming.cs) | Authoritative stance entry, movement lock and stale-ball cleanup |
| [GolfAimInput](../Game/Assets/_Game/Shared/Player/GolfAimInput.cs) | Local aim request/acknowledgement, cancellation and preview lifecycle |
| [GolfShotPlan](../Game/Assets/_Game/Sports/Golf/GolfShotPlan.cs) | Shared swept trajectory, automatic shot rules and cycling power |
| [GolfShotPreview](../Game/Assets/_Game/Sports/Golf/GolfShotPreview.cs) | Continuous trajectory ribbon and procedural landing glow |

[GolfCourse](../Game/Assets/_Game/Sports/Golf/GolfCourse.cs) retains the map's existing serialized `GolfHole` records. In play it adds a manager, which attaches lightweight capture triggers to those five cups. No scene regeneration is required. Live balls share the existing equipment ball's meshes, material and two LODs. The [supplied rounded ball](../ArtSource/Golf/Ball/README.md) is displayed and collides at **3× its previous size: 129 mm diameter, 64.5 mm radius**. `GolfBall.VisualScale` configures this relative to the unchanged 21.5 mm source-model radius. `GolfBallBuilder` scales the visual child, preserves the prefab identity and matches its sphere collider; tee height, rolling rotation and cup entry use the same radius. The existing 285 mm cups need no enlargement. Near-view dimples and a lightweight round far silhouette share their existing meshes. Native UI arrows need no texture or shader asset. Balls ignore the character's shoreline wall layer so they can actually leave the island. Character containment remains unchanged. See [map authoring](GOLF-COURSE.md) and [carts](GOLF-CART.md).

## Verification

The [Shot accuracy suite](../Tools/Build/Test-GolfShot.ps1) measures actual ball
positions against the rendered guide mesh on all five holes, for both modes
and multiple powers. It exercises repeated charge cycles, descending release,
mode distance boundaries, collision/impulse interruption, reset and an actual
putt into a cup. Use `-PlayerPath` and `-Render` for captures. The Aim suite also
checks replicated Putt mode and host/client stopping targets.

The [Swing interaction suite](../Tools/Build/Test-GolfSwing.ps1) accepts
`-PlayerPath` and `-Render`. It checks readiness from all directions, touch
raycasts, automatic stance, selected-ball locking, cancellation and very-close
contact grips. The Golf match suite also accepts `-OfflineOnly` and
`-NetworkOnly -SideSwing -Render` to verify the rendered client's stance and
authoritative shot. See the
[measured Swing release](VERIFICATION.md#reliable-golf-swing-proximity--4-october-2026).


From the repository root, run `Tools/Build/Test-GolfMatch.ps1` against a fresh development Windows player; `-PlayerPath` selects another player, `-Render` captures the offline view, and `-NetworkOnly` runs the real host/client checks independently of offline presentation checks. The script resolves default paths from its own location, so it also runs from an unrelated working directory. Tests opt in through `-golfMatchAudit` and `-networkGolfMatchAudit`; their behavior is compiled only for Editor/development builds. A release can retain tiny MonoScript metadata records without the fixture code.

The suite covers independent progress, invalid and duplicate cups, helper strokes/owner completion, real rigidbody motion and cup triggers, shoreline recovery, touch charge/cancel, round resets, local indicators, stroke/time ranking and exact ties. Two real local players verify owner RPCs, different targets, continued countdown play and an actual 30-second wait ending in replicated DNF. Feature checks are followed by existing football, basketball, golf map and cart regressions as applicable.

Run `Tools/Build/Test-GolfBallPhysics.ps1 -Render` against a fresh development
player for the actual-tee visibility and physics checks. `-PlayerPath` selects
an isolated player; omit `-Render` for headless checks. The opt-in fixture does
not relocate the ball or force a LOD before capturing the real tee. It then
checks the 3× model/collider, centred startup, Aim/Cancel button, camera resets
and actual ball pixels with the UI visible, then uses a temporary collider
inside the course to measure body/cart isolation,
level-ground deceleration, settling/rest/wake on 5°/15° slopes and actual qualified
putt/charged-swing motion. It also drops the larger ball into all five existing
cups and checks next-tee support, finish and restart. Reports and native captures go under ignored
`Builds/GolfBallPhysicsQA/`; the fixture is absent from release gameplay.

Current task evidence is under ignored `Builds/GolfMiniGameQA/`. Release size and device-testing limits are recorded in [BUILD-SIZE](BUILD-SIZE.md) and [VERIFICATION](VERIFICATION.md). Windows validation does not establish physical-phone touch, quality/FPS or WAN behavior.

## Aiming interaction verification

Run `Tools/Build/Test-GolfAim.ps1 -Render` against a fresh development player.
It exercises real Aim and Swing controls offline and in a host/client room:
entry beyond the old swing range, safe stance, movement/jump/cart lock, direction
changes, charge cancellation, restored walking, pointer ownership, single-stroke
release, camera modes, stale-ball resets, obstruction, all five authored tees and
round cleanup. Rendered captures include the full HUD and both putt/chip guides.
Evidence is retained under `Builds/GolfAimQA/`. No test fixture ships in release
play, and Windows rendering does not qualify physical Android performance.

## Decaying turf bounces - 13 October 2026

A lofted guided swing still ends at its first swept touchdown target. Instead of deleting incoming normal velocity, `GolfBallPhysicsSettings.LandingVelocity` returns the surface tangent with the existing first-landing retention (.78), plus normal restitution (.46). Subsequent authoritative static-turf contacts retain .92 of tangent speed and .46 of incoming normal speed. Normal bounce energy is at most .2116 of the previous impact before gravity/drag losses. Impacts below .55 m/s transition to the existing rolling/settling code, preventing tiny perpetual hops. Ground support cannot prematurely consume a qualifying descending bounce. Free drops use the same contact response; putt plans retain their existing rolling calculation. The ball collider has no material restitution, so the response is applied once.

Only the host/offline authority simulates these contacts. Existing position/rotation snapshots display rebounds on remote clients. No RPC or protocol field, charge rule, launch speed, scoring, cup progression or camera control is added. Airborne run-out is deliberately longer because the requested bounces preserve some vertical energy.
