# Basketball passing

Hold, aim and release follow the football controls. Basketball adds a live
curved trajectory: teal for chest, blue for loft and orange for bounce, with
flowing chevrons, amber charge accents and a short release pulse. Every player
sees the host's pass intent. Match protocol: **32**; use matching players.

## Controls

| Action | Keyboard / mouse | Touch |
| --- | --- | --- |
| Short chest pass | Tap Q | Tap Pass |
| Charge distance | Hold Q, up to 0.8 seconds | Hold Pass |
| Loft | While holding Q, press Up | Swipe upward from Pass |
| Bounce | While holding Q, press Down | Swipe downward from Pass |
| Return to chest | Left or Right arrow | Return near the original press point |
| Aim horizontally | Right mouse drag | Drag the camera area with another finger |
| Throw | Release Q | Release the original Pass finger |
| Cancel | X | Slide left onto the separate Cancel target |

A longer vertical swipe increases loft height or makes the bounce delivery
reach the floor sooner. The original finger retains ownership; another finger cannot change
or release its pass. Vertical selection has a center dead zone and hysteresis.
Cancel sits to the left of Pass, outside the up/down swipe corridor.
Full charge stays ready until release; it never fires automatically.

Shooting and passing cannot charge together, including queued releases in the
same input frame. Possession loss, travel, focus loss and leaving play cancel
the gesture. The Q/button slot becomes Defend when another player holds the
ball; an old gesture cannot trigger the new role's action.

## Trajectory and motion

Chest and loft passes range from **4 to 10 m**. Loft takes a slower, higher
route, rising up to about 2.8 m above the release point at full charge and
maximum loft. The recipient can catch the descending ball.

Bounce passes range from **3 to 7.5 m**, with a shorter receiving distance and
a low return. The preview includes the first floor contact and rebound. A
clean floor contact applies the same planned rebound velocity as the preview,
with 0.78 vertical restitution and 15% horizontal speed loss. This applies
once, after actual contact; a wall or block cancels it. There is no position
teleport or in-flight homing.

The ribbon uses the same ballistic plan and fixed-step gravity correction as
the host's launch. It shows an unobstructed receiving path, not a guaranteed
catch. Players and court objects can intercept or deflect the ball. Horizontal
heading follows the displayed curve without hidden receiver selection.

The player completes its dribble, gathers into a chest pocket and pushes with
both hands. Loft releases slightly higher; bounce slightly lower. Hands stay
below the face and recovery blends into movement. Holding halves movement
speed. Release freezes direction, charge and pass type through the gather.

## Implementation and review

- `BasketballPassing.cs` owns authoritative begin, aim, cancel and release.
  Reliable owner RPCs carry the edges; movement input carries heading and
  bend. Possession serials reject stale requests. The host measures charge.
- `BasketballPassPath.cs` supplies launch, curve sampling and the first clean
  floor rebound. `BasketballPassVisual.cs` reuses a small procedural ribbon
  and one unlit material, with no imported texture or animation.
- `Test-BasketballTrajectory.ps1` checks touch selection/cancel, physical flight,
  host/guest curve replication and the new shot odds. The defense surface
  review checks chest, loft and bounce pushes against both character LODs.

Examples from `Tools/Build/` (scripts resolve their own project root):

```powershell
./Test-BasketballTrajectory.ps1 -Mode Offline
./Test-BasketballTrajectory.ps1 -Mode Local
./Test-BasketballDefense.ps1 -Mode Offline -PassOnly -NoCapture -Fps 120
```

Generated results stay under ignored `Builds/`. See the
[shot chance design](BASKETBALL-SHOT-ODDS.md), [verification record](VERIFICATION.md)
and [measured release size](BUILD-SIZE.md) for delivery and device-testing limits.

