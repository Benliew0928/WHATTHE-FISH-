# Basketball pickup, shot physics, scoring and passing

Choose **Basketball > Explore offline**, or create a basketball room and start exploration. Walk within about one metre of the loose ball to pick it up automatically. The player dribbles automatically with synchronized hand and body motion. **Hold E** on desktop or **hold the Shoot button**, then release in the meter's green band. The needle sweeps back and forth, so holding through a missed window gives another chance. Lower power falls short; higher power adds excess force. Actual contacts decide the outcome, so a mistimed shot can still make a legitimate bank or rim bounce. **Drag the same finger into Cancel**, click Cancel, or press **X** to lower the ball without firing. **Q** or **Pass** performs a chest pass toward a teammate in front, or into open court. Move with WASD/left stick; aim toward either end using right-mouse drag/right-side touch look. At charge start the camera-facing hoop is preferred, with the nearer hoop as the fallback, and that target stays locked until release or cancellation.

The HUD shows shared practice points, made baskets / attempts, shot results and the power meter. The action slots switch with possession: Shoot/Block (E), Pass/hold Defend (Q), and a defender-only Steal (F). Space/Jump becomes Jump Block while defending. Shift/sprint and C/Camera retain their roles; holding Defend limits movement to a 3 m/s strafe. See [defense controls and contact rules](BASKETBALL-DEFENSE.md). Released shots use rigid-body flight, backspin, floor bounce, rim and backboard contacts. Both nets respond to contact and scored shots. This is shared shooting practice; team assignment, match timers and fouls are not implemented for basketball. The large architectural scoreboard remains decorative. [Basketball animation](BASKETBALL-ANIMATION.md) describes the stopping stance, running overlay, dribble, shooting and passing timelines.

## Layups and dunks

While holding Shoot, drag **up for Dunk** or **down for Layup**, then release.
An eligible finish requires **no charge or timing**. Return to the centre for a regular shot. Desktop also supports **E + Up /
Down**; release E to commit. Cancel is left and above the shoot button, with X
still available. The host validates approach, distance, grounded state and a
clear jump path. Unavailable finishes explain the reason and release a normal
shot with normal charge timing instead of cancelling.

Both finishes can start up to 4 m from the rim. Layups use a softer arc with an
85% host-side make chance. Dunks require a moving approach and are guaranteed
when unblocked. A brief planted gather retains the actual approach; it cannot
create a run-up from standing. Both have gather, takeoff,
follow-through and landing poses on the shared rig. Scoring still requires a
physical rim crossing; missed finishes rebound normally. See the
[finish rules, timings and validation](BASKETBALL-FINISHES.md).

## Charging, distance and cancellation

Charging halves the requested movement speed: walking **4 → 2 m/s**, sprinting **7 → 3.5 m/s**, with the existing acceleration and airborne momentum rules. The current bounce finishes into a two-handed aiming pose that remains visible to opponents for the whole hold. A prepared release lifts directly from that grip; cancellation lowers the ball smoothly back into the dribble. Passing can replace the charged aim. Possession loss, travel and session changes cancel charging.

A one-way needle sweep takes **0.84 s at 4 m or closer**, gradually shortening to **0.42 s at 22 m or farther**. The green band remains at **60.5–69.5%**, giving approximately **76 ms** per close-shot crossing and **38 ms** per distant-shot crossing. There is no random distance penalty or automatic shot when the meter reaches an end. Holding beyond five seconds is allowed. Distance is measured horizontally from the player to the locked basket and updated while moving; the accumulated phase stays continuous, preventing a needle jump or a reset by turning the camera.

The mobile cancel target follows the football gesture: dragging outside Shoot alone keeps tracking the original finger; entering Cancel cancels that charge, and lifting afterwards cannot shoot. Other fingers cannot release or drag-cancel it. Cancelling preserves possession and resumes normal movement. Losing focus or disabling the controls clears held input.

The host integrates phase and replicates it with the locked basket and action pose. Release transmits an input timestamp rather than a requested power. The host samples its own phase history with at most **250 ms** of bounded rewind, preventing an extra input frame or ordinary RPC delay from changing the displayed release power. WAN jitter beyond that allowance remains a device/network qualification item. Current rooms use **protocol 29**; all players need matching builds.

## Stealing and loose-ball contests

**Hold F** or press and hold **Steal** to attempt a swipe every **0.74 seconds**. Repeated taps also work, but cannot bypass the **0.68-second** recovery. Releasing, dragging off, losing focus, leaving play or entering Free roam stops the held input. A swipe uses a **0.14-second windup** and a contact window ending at **0.26 seconds**. Movement is reduced during a swipe/reaction; jumping cannot cancel either animation.

The host evaluates contact during that window, using the current ball position, a committed swipe heading and the chosen hand. There is no random roll. The defender must be grounded, within **1.55 m** of the carrier, within **0.65 m** vertically, and facing the ball within a **60-degree** half-cone. A three-dimensional reach test from the lowered shoulder limits contact to **0.95 m including the ball radius**. Walls, another player's body, and the carrier's torso block the reaching path. The carrier can turn, move out of reach or release a pass/shot before contact.

After those gates, contact quality is `0.45 * reach + 0.25 * alignment + 0.30 * exposure`. Reach improves from the outer limit to 0.40 m; alignment improves from the cone edge to almost straight ahead. Exposure follows `sin(pi * dribblePhase)^2`: the free bounce is more exposed than the hand-controlled endpoints. A two-handed gather/airborne hold uses exposure 0.05 and a stricter threshold. Ordinary swipes need quality **0.60**; secured grips need **0.64**. These are initial gameplay tuning values, not real-world basketball measurements. No basketball teams or foul rules have been added.

A successful strip cancels the carrier's queued shot/pass or charge, plays a **0.60-second** recoil, and releases a **physically simulated, unowned ball** from its actual contact position. The outgoing velocity combines **60% of carrier movement**, **25% of defender movement**, a bounded outward/sideways swipe, and dribble vertical velocity. Left/right contact changes the lateral deflection; a rising dribble can pop up, while a descending swipe drops into a bounce. Horizontal speed is capped at **7.5 m/s**, vertical speed at **−0.85 to 2.5 m/s**, with bounded spin. Floor, player, rim and backboard contacts then determine where it goes.

Everyone waits **0.34 seconds** before pickup; the stripped player waits **0.70 seconds**. Both participants must also finish their animation recovery. Normal closest-eligible-player pickup then decides possession, so the defender has no ownership claim and the original carrier can recover it. The HUD reports **BALL LOOSE · CHASE IT**. Simultaneous swipes are tied to the original possession and cannot strip a newly acquired ball.

[BasketballSteal.cs](../Game/Assets/_Game/Sports/Basketball/BasketballSteal.cs) centralizes the rules and authoritative release. Both poses reuse the existing rig without imported animation assets. The aiming grip counts as a secured two-handed hold. Run [Test-BasketballSteal.ps1](../Tools/Build/Test-BasketballSteal.ps1) offline and with `-Mode Local` for repeat-input, geometry, physics and two-process replication checks.

## Tuning

Player boundaries follow the painted **15.24 × 28.6512 m** court. Walking, sprinting, dribbling and jumping stop inside all four lines, allowing room for the character capsule. Diagonal movement slides along the edge. Loose balls remain physical and can leave the court. A ball wholly outside a painted line and below pickup height returns after 0.75 seconds so it cannot become stranded beyond the player's reach. Airborne excursions retain the wider recovery apron. The constraint runs in the offline/host movement path without a physical wall collider.

[BasketballBoundaryVisual.cs](../Game/Assets/_Game/Sports/Basketball/BasketballBoundaryVisual.cs) draws a **1.8 m** translucent energy wall with teal/gold flow, geometric weave and lit rails. Four panels use only **16 vertices**, sharing football's existing material/shader with property overrides. Player contact and a ball crossing create expanding, fading ripples; four recent pulses can overlap. Swept crossing detection catches fast balls, while reset sequence changes suppress teleport effects. Each peer evaluates its presented actors/ball, adding no contact RPCs. No imported texture, mesh or material was added, and the wall does not block ball or camera physics.

Choose **Free roam** beneath the practice score to deliberately leave play, release possession and walk to the Sky-Sail station. **Play basketball** enables the boundary again once the player is on court. Arrivals outside the court can walk in normally. Free roam excludes pickup and shot/pass actions, is controlled by the owner through the host in rooms, and resets on session/scene transitions. The regular Leave court / Return to room control retains its session-exit behavior. See [BasketballCourt.cs](../Game/Assets/_Game/Sports/Basketball/BasketballCourt.cs).

The existing [ball prefab](../Game/Assets/_Game/Prefabs/Basketball/BasketballBall.prefab) has a **0.30 m diameter**, enlarged 25% for the stylized character, with a matching 0.15 m sphere collider and adjusted hand contacts. It retains the 0.62 kg gameplay mass, 0.78 restitution, both existing mesh LODs and rubber material. The 0.24 m authoring meshes remain unchanged; their prefab visual scale is 1.25. The interaction settings are defined in [BasketballInteraction.cs](../Game/Assets/_Game/Sports/Basketball/BasketballInteraction.cs) and authored on the prefab.

| Setting | Default | Effect |
| --- | --- | --- |
| Pickup radius / height | 1.05 m / 1.65 m | Loose ball must be reachable, with no wall between ball and player |
| Pickup speed limit | 10 m/s | Fast loose balls cannot be caught automatically; ascending shots are also excluded |
| Airborne carry offset | (0, 0.80, 0.34) m | Two-handed hold while jumping; grounded dribbling uses its own timeline |
| Release / shooter grace | 0.30 / 0.90 s | Prevents immediate re-grabbing; other players become eligible sooner |
| Base arc height | 1.60 m above the higher endpoint | Additional distance-based clearance shapes the assisted arc |
| Arc per metre | 0.18 m | Gives the enlarged ball enough descent angle on long shots |
| Maximum shot speed | 24 m/s | An unsolvable shot is rejected while keeping possession |
| Backspin | 18 rad/s | Rotation around the shot's horizontal transverse axis |
| Playable recovery bounds | local x ±10 m, z ±17 m | Loose balls outside for 0.75 s reset; holders outside release immediately |
| Stranded timeout | 12 s | Resets a stationary ball stuck above pickup height |
| Outside painted line near floor | 0.75 s | Returns loose apron rebounds once the entire sphere is outside the line and below pickup height |
| One-way meter sweep / ideal fraction | 0.84–0.42 s / 0.65 | Distance changes speed; the needle repeats in both directions through a 0.605–0.695 green band |
| Charging movement multiplier | 0.5 | Applies to walking and sprinting; cancelling restores normal speed |
| Shot release / full action | 0.308 / 0.686 s | Charge, gather, turning and follow-through take 30% less time; passing keeps its existing timing |
| Unresolved shot timeout | 8 s | Marks an unresolved attempt missed without adding a second outcome |

Air damping is zero during the initial shot to preserve the solved arc; the existing 0.015 damping returns on first collision. Airborne shots use swept continuous collision detection to avoid speculative contacts with the thin rim. Descending below 1.8 m restores the foundation's speculative mode before floor bounce. Gravity uses the project's fixed timestep and includes the semi-implicit integration correction. Release requires an unobstructed sphere and line from the body to the hand release point. The animation finishes its current bounce and gathers before launching; trajectory calculations run at release. Jumping and movement do not add an uncontrolled velocity boost.

## Launch calculation

For release position `s`, rim target `q`, horizontal distance `d`, gravity magnitude `g` and fixed timestep `dt`:

```text
apex = max(s.y, q.y) + 1.6 + 0.18*d
up = sqrt(2*g*(apex - s.y))
flightTime = up/g + sqrt(2*(apex - q.y)/g)
ideal horizontal velocity = (q - s).horizontal / flightTime
ideal vertical velocity = up + g*dt/2
```

The last term compensates for Unity's semi-implicit fixed-step integration. For horizontal player-to-basket distance `d`, the sweep period is `0.84 / (1 + smoothstep(clamp((d - 4) / 18, 0, 1)))`. Holding integrates `phase += elapsed / period`, and normalized power is `pingpong(phase, 1)`. The green band has no penalty. Outside it, signed error is `sign(p - 0.65) * max(0, abs(p - 0.65) - 0.045)`: horizontal speed is multiplied by `1 + 0.8*error` and vertical speed by `1 + 0.14*error`. This is deterministic arcade assistance; the ball is never moved toward the target during flight.

The body starts from a held, stationary state, so the equivalent launch impulse is **`J = mass * velocity`** in newton-seconds. The implementation assigns this initial velocity once; it does not apply a frame-dependent force every Update. A force applied over a chosen interval would be `F = J / interval`. Nonfinite inputs, impossible arcs, speeds above 24 m/s and obstructed hand releases are rejected while retaining possession.

## Score, miss and net response

[BasketballScoring.cs](../Game/Assets/_Game/Sports/Basketball/BasketballScoring.cs) owns one outcome per released shot. [BasketballHoop.cs](../Game/Assets/_Game/Sports/Basketball/BasketballHoop.cs) sweeps between successive physics positions, so a fast ball can cross both detection planes in one step.

- A downward crossing of the rim centre plane must fit the complete sphere inside the opening. It then has to continue until the entire ball clears the rim's lower plane. Upward approaches, net side entry, rim rests and an incomplete bounce back out cannot score.
- Rim and backboard collisions leave the attempt live. A make is awarded once; subsequent floor bounces and net contact cannot add points. Shots are resolved as missed at the first floor contact, interception/pickup, boundary recovery or eight-second timeout. Loose rebounds remain physical and can be retrieved.
- Two/three-point value uses the shooter's ground position at release relative to the selected hoop. It follows the authored 7.239 m arc, 6.7056 m straight corner lines and 0.0508 m line width; standing on the line counts as two. This is an avatar-position approximation, not a separate referee for each foot or jump takeoff.
- Each net uses 80 pinned/rest nodes and 1,152 rendered vertices, sharing the existing white net material. A damped ripple reacts to side touches and rim hits; a score stretches and flares the lower net while its upper attachments remain fixed. Modest velocity damping below the rim gives soft resistance; this is a procedural approximation, not full cloth collision.
- A net at rest stops rewriting its mesh. No imported mesh, texture, animation, audio or physics package was added. The authored net remains in the editable hoop; its static renderer is replaced at runtime. Session exit clears the practice totals, while ordinary ball recovery preserves them.

## Authority and lifecycle

Offline simulation belongs to the local player. In a room, only the host arbitrates proximity, possession, trajectory, physics, scoring and resets. Nearest eligible player wins; equal-distance ties use client ID. Only connected, active avatars on the court can acquire the ball. Owner-only reliable charge begin/cancel/release RPCs let the host choose the target and integrate hold phase; guests supply a camera heading and bounded release timestamp, never a velocity, power, position, score or target player. Release without a recorded hold, nonholders, repeated shots, invalid headings, travel and inactive sessions are rejected. The client meter starts immediately and follows the authoritative phase once received; timing under WAN latency or severe frame hitches still needs device/network qualification.

The 20 Hz ball snapshot carries possession, queued-action state, dribble phase/cadence, shot/pass sequences, score state and the latest timestamped event for each net. Guests play the host's net events without simulating independent goals. Each player also replicates the action timeline for gather and follow-through. Guests remain kinematic, follow the displayed holder when carried and interpolate host flight poses. Transition/reset sequences snap interpolation across pickup, release and reset. A disconnect, room return/restart or island unload clears possession and ignored collision pairs. The shooter collider is ignored briefly until the released sphere is clear; normal collisions then resume. One active basketball and one shared sport per host room are preserved. **Host and guest must use matching builds.**

## Reproduce checks

Build a development Windows player using `Tools/Build/Build.ps1 -Target Windows`. For code-only iterations with existing generated scenes, Unity batch method `ProjectBuilder.BuildCurrentWindows` avoids regenerating art.

```powershell
Tools/Build/Test-BasketballMotion.ps1
Tools/Build/Test-BasketballFinish.ps1
Tools/Build/Test-BasketballFinish.ps1 -Mode Local
Tools/Build/Test-BasketballCharge.ps1
Tools/Build/Test-BasketballPhysics.ps1
Tools/Build/Test-BasketballGameplay.ps1 -Mode Offline
Tools/Build/Test-BasketballGameplay.ps1 -Mode Local
Tools/Build/Test-BasketballBall.ps1 -Mode Offline
Tools/Build/Test-BasketballBall.ps1 -Mode Local
Tools/Build/Test-Jump.ps1
Tools/Build/Build.ps1 -Target AndroidSubmission
Tools/Build/Check-TaskReady.ps1 -RequireApk
```

The gameplay review exercises real players, command input, physical rim crossings, blocked pickup, recovery, restarts and two-process ownership/replication. It is development-only and does not ship in the Android release. Evidence goes under ignored `Builds/BasketballGameplayQA/`. The earlier ball review explicitly disables automatic pickup to isolate its contact/material tests. Exact current APK measurements are recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

## Earlier gameplay validation, 1 October 2026

These counts describe the earlier instant-release implementation. Current animation verification is recorded in [BASKETBALL-ANIMATION.md](BASKETBALL-ANIMATION.md).

- Final gameplay: **82 passes** (61 offline, 12 host, 9 guest). Actual descending rim crossings passed at 2, 5, 10, 18 and 24 m on the north hoop and 5 m on the south hoop. The player retrieved a real rebound for another shot. Evidence: `Offline-20261001-220542/` and `Local-20261001-220536/` beneath `Builds/BasketballGameplayQA/`.
- Foundation: **53 passes** across offline and two-player bounce/spin/floor/rim/backboard/replication checks. Evidence is under `Builds/BasketballModel/20261001/Offline-220649/` and `Local-221208/`.
- Shared jump/input regression: **40 passes**, including two-player jump/air-steering checks; `Builds/JumpQA/Run-20261001-220540/`.
- Portability: all **61 offline checks** repeated from a copied player and script in `Builds/Portable Basketball Fixture/`, invoked from an unrelated temporary directory. The result is retained in `Builds/BasketballGameplayQA/PortablePathEvidence/`. Changed Markdown links resolve; publishable text path scanning passed.
- Android: `AndroidSubmission` and APK v2 signature verification passed. **85,957,194 to 85,970,822 bytes**, an increase of **13,628 bytes**, with **14,029,178 bytes** below the hard limit. The 75 MB working target remains unmet by **10,970,822 bytes**. No new art or runtime package was added.

The first restricted editor invocation failed to obtain a licence; normal editor access resolved it. Early shot tests exposed thin-rim speculative contacts and stale imported defaults; swept airborne contacts and explicit prefab settings resolved both. Final checks above use the corrected Windows player. First-person, third-person, elevated and shot camera renders were inspected; these offscreen captures do not establish phone performance or reproduce every screen-space UI overlay.

Superseded attempts, intermediate build logs and the completed portable fixture were archived to `Legacy/20261001-221235-304-basketball-gameplay/` with sizes, SHA-256 hashes and recovery paths. No source art, active game asset or current release was archived. Git staging, commit and push remain with the user.

Windows probes and screenshots do not qualify physical-phone rendering, touch ergonomics, frame rate, thermals or WAN latency. Those require device testing.
