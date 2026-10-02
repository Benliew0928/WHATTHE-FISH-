# Basketball pickup, shooting and passing

Choose **Basketball > Explore offline**, or create a basketball room and start exploration. Walk within about one metre of the loose ball to pick it up automatically. The player dribbles automatically with synchronized hand and body motion. **E** on desktop or the **Shoot** touch button gathers and releases one shot. **Q** or **Pass** performs a chest pass toward a teammate in front, or into open court. Holding the control does not repeat it. Move with the existing WASD/left stick controls; aim toward either end using right-mouse drag/right-side touch look. The camera-facing hoop is preferred, with the nearer hoop as the fallback. Power adjusts automatically to distance.

The button reads **Walk to ball**, **Ball held**, or **Shoot**, so possession is visible even in first person. Space/Jump, Shift/sprint and C/Camera retain their existing roles. Shots use real rigid-body flight, backspin, floor bounce, rim and backboard contacts. The existing decorative scoreboards do not count baskets. Scoring, sound and effects remain outside this interaction. [Basketball animation](BASKETBALL-ANIMATION.md) describes the stopping stance, running overlay, dribble, shooting and passing timelines.

## Tuning

The existing [ball prefab](../Game/Assets/_Game/Prefabs/Basketball/BasketballBall.prefab) has a 0.24 m diameter, 0.62 kg mass, 0.78 restitution, two mesh LODs and its existing rubber material. The component's interaction settings are defined in [BasketballInteraction.cs](../Game/Assets/_Game/Sports/Basketball/BasketballInteraction.cs); they appear on BasketballBall in the Inspector and default correctly on the existing scene and prefab instances.

| Setting | Default | Effect |
| --- | --- | --- |
| Pickup radius / height | 1.05 m / 1.65 m | Loose ball must be reachable, with no wall between ball and player |
| Pickup speed limit | 10 m/s | Fast loose balls cannot be caught automatically; ascending shots are also excluded |
| Airborne carry offset | (0, 0.80, 0.34) m | Two-handed hold while jumping; grounded dribbling uses its own timeline |
| Release / shooter grace | 0.30 / 0.90 s | Prevents immediate re-grabbing; other players become eligible sooner |
| Base arc height | 1.60 m above the higher endpoint | Additional distance-based clearance shapes the assisted arc |
| Arc per metre | 0.12 m | Gives long shots enough descent angle to clear the front rim with the finite-sized ball |
| Maximum shot speed | 24 m/s | An unsolvable shot is rejected while keeping possession |
| Backspin | 18 rad/s | Rotation around the shot's horizontal transverse axis |
| Playable recovery bounds | local x Â±10 m, z Â±17 m | Loose balls outside for 0.75 s reset; holders outside release immediately |
| Stranded timeout | 12 s | Resets a stationary ball stuck above pickup height |

Air damping is zero during the initial shot to preserve the solved arc; the existing 0.015 damping returns on first collision. Airborne shots use swept continuous collision detection to avoid speculative contacts with the thin rim. Descending below 1.8 m restores the foundation's speculative mode before floor bounce. Gravity uses the project's fixed timestep and includes the semi-implicit integration correction. Release requires an unobstructed sphere and line from the body to the hand release point. The animation finishes its current bounce and gathers before launching; trajectory calculations run at release. Jumping and movement do not add an uncontrolled velocity boost.

## Authority and lifecycle

Offline simulation belongs to the local player. In a room, only the host arbitrates proximity, possession, trajectory, physics and resets. Nearest eligible player wins; equal-distance ties use client ID. Only connected, active avatars on the court can acquire the ball. Owner-only reliable Shoot and Pass RPCs supply camera heading, never a velocity, position or target player. Host validation rejects nonholders, repeated shots, invalid headings, travel and inactive sessions.

The 20 Hz ball snapshot carries possession, queued-action state, dribble phase/cadence and shot/pass sequences. Each player also replicates the action timeline for gather and follow-through. Guests remain kinematic, follow the displayed holder when carried and interpolate host flight poses. Transition/reset sequences snap interpolation across pickup, release and reset. A disconnect, room return/restart or island unload clears possession and ignored collision pairs. The shooter collider is ignored briefly until the released sphere is clear; normal collisions then resume. One active basketball and one shared sport per host room are preserved. **The combined football and basketball network changes use protocol 14, so hosts and guests need matching builds.**

## Reproduce checks

Build a development Windows player using `Tools/Build/Build.ps1 -Target Windows`. For code-only iterations with existing generated scenes, Unity batch method `ProjectBuilder.BuildCurrentWindows` avoids regenerating art.

```powershell
Tools/Build/Test-BasketballMotion.ps1
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
