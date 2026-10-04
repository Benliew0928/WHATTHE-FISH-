# Golf mini game

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
- Every accepted swing adds one to the hitter's current-hole strokes and total strokes. Hitting another player's ball never transfers ownership or directly adds a stroke to its owner.
- A helper can complete the ball owner's target. Progress, finish state and finish time belong to that owner; the swing belongs to the helper.
- Entering another cup or crossing the shoreline restores the struck ball to its last shot position, stops its motion and preserves all scores. Recovery adds no penalty stroke. The recovery anchor updates whenever anybody hits that ball, and starts at the tee for a new hole.
- The first Hole 5 completion stores elapsed time from match start and starts one shared **30-second** countdown. Later finishers never change its deadline.
- End immediately when all participants have finished, or when that deadline is reached. The latter marks all remaining participants DNF, preserving their completed holes and total strokes.
- There is no global, single-hole, AFK or maximum-duration timer before the first finisher.
- Finished players rank ahead of DNF, by total strokes ascending, then elapsed finish time ascending. Equal strokes and equal finish time share a rank and a Tie marker. DNF remains in roster order and receives no competitive rank.

The host owns all physical simulation, scores and deadlines; offline play uses the same gameplay code. Clients send reliable owner-authorized swing requests with a selected ball, heading, charge and round revision. The host validates the hitter, active ball, range, visibility and revision. Invalid, cancelled or stale requests add no stroke. The ball snapshot is published on its owner's player object, independent of the hitter. Room protocol is **21**; older builds cannot join this build.

## Controls and display

Use the existing stick/WASD to move and right-side drag/right mouse to look. Approach a ball, hold **Swing / F** to charge, then release. Short charges putt; stronger charges lift the ball. Dragging off the touch button cancels that charge. F is reserved for golf swings during a live round; station travel buttons continue to use the existing group-travel checks.

Swing becomes available within **1.1 metres horizontally** of a visible ball,
including close, side and rear approaches. Pressing Swing turns the golfer
toward that ball while keeping the feet in place. Camera aim still determines
the shot direction. The selected ball stays locked for the whole charge;
leaving its range cancels the charge even if another ball is nearby. The
charge duration remains 1.3 seconds. Swing strength now ranges from a 1.2 m/s
putt to an **18 m/s horizontal launch**, with up to 1.8 m/s lift at full charge.
The previous five-metre range cap is removed. Flight and fast rolling use normal
physics; the existing slow rolling, slope settling and resting rules determine
when the ball stops. Travel depends on charge, terrain, impacts and rolling
resistance. Launch parameters are grouped in the persistent Golf Ball Physics
Inspector asset below.

The aqua arrow continuously points toward the local player's own target hole. A gold marker shows the location and distance of their own ball, including off-screen directions. The upper-right leaderboard displays player name, completed holes out of five, total strokes, status and finish time. The final countdown remains hidden until someone finishes and displays “A player has finished!”; the first finisher is not announced as the winner. Results display ranks only after settlement.

Starting a live round keeps the normal centred third-person camera. Tap
**瞄准** beside Camera to enter the existing closer shoulder view, with the
character on the left; tap **取消瞄准** to return. This is a local camera toggle
and is not required to Swing. Switching camera modes, driving, leaving or
restarting clears it. `PlayerView.golfShoulderOffset` and `golfViewDistance`
retain the previous aiming angle and distance. First-person, elevated views
and driving retain their existing camera rules. The
ball's final LOD stays rendered instead of culling at a small screen size. The
gold indicator uses a screen-space gap above the ball so its arrow does not
cover the actual sphere, including at different camera distances.

Live balls use one authoritative Rigidbody and sphere collider. Characters
(including late arrivals and re-enabled controllers) and carts remain excluded.
Only ball motion changes: `GolfBall` now uses Flying, FastRolling, SlowRolling
and Resting. Flying keeps the existing gravity, damping and launch velocity.
Fast slopes remain free; existing near-level rolling resistance is retained.
Below the slow threshold on supported ground, extra tangential resistance ramps
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
| Minimum Swing Speed | 1.2 m/s | Horizontal speed at zero charge |
| Maximum Swing Speed | 18 m/s | Horizontal speed at full charge |
| Maximum Swing Lift | 1.8 m/s | Vertical speed at full charge |
| Slow Rolling Threshold | 2 m/s | Enter extra ground resistance below this speed |
| Stop Speed Threshold | 0.25 m/s | Speed must remain below this before rest |
| Stop Delay | 0.5 s | Continuous supported low-speed time needed |
| Settling Max Speed | 1 m/s | Slow-mode speed band; entry converges smoothly |
| Rolling Resistance Strength | 1.8 m/s² | Extra ground deceleration, ramped with slowing |
| Level Rolling Resistance | 1.35 m/s² | Existing normal rolling on nearly level ground |
| Level Ground Degrees | 0.5° | Existing near-level tolerance |
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
immediate release and normal flight. Repeat downhill above 2 m/s, and collide
another ball with the resting one. Check both Host and Client views. No map,
terrain, hole, character-movement, scoreboard or countdown settings are changed.

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
applies ball velocity at 0.10 seconds. Feet stay planted through contact and
control resumes during follow-through. Recovery lasts 0.74 seconds and prevents
duplicate strokes. Round changes and ball resets cancel queued contacts. Charge,
heading, round, sequence and server start time synchronize the presentation;
protocol 21 requires matching host/client players. Ball ownership, scoring,
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
| [GolfSwingButton](../Game/Assets/_Game/Shared/UI/GolfSwingButton.cs) | Touch charge/cancel, local Aim display and start/restart commands; no scoring decisions |

[GolfCourse](../Game/Assets/_Game/Sports/Golf/GolfCourse.cs) retains the map's existing serialized `GolfHole` records. In play it adds a manager, which attaches lightweight capture triggers to those five cups. No scene regeneration is required. Live balls share the existing equipment ball's meshes, material and two LODs. The [supplied rounded ball](../ArtSource/Golf/Ball/README.md) is displayed and collides at **3× its previous size: 129 mm diameter, 64.5 mm radius**. `GolfBall.VisualScale` configures this relative to the unchanged 21.5 mm source-model radius. `GolfBallBuilder` scales the visual child, preserves the prefab identity and matches its sphere collider; tee height, rolling rotation and cup entry use the same radius. The existing 285 mm cups need no enlargement. Near-view dimples and a lightweight round far silhouette share their existing meshes. Native UI arrows need no texture or shader asset. Balls ignore the character's shoreline wall layer so they can actually leave the island. Character containment remains unchanged. See [map authoring](GOLF-COURSE.md) and [carts](GOLF-CART.md).

## Verification

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
