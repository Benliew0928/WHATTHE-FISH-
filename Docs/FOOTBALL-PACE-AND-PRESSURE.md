# Football pace, stamina and pressure

Football locomotion is **70% of the previous pace**, equally for human players,
room guests and bots. Walking is **4.2 m/s** and running **7.35 m/s**.
Dribbling retains its 75% possession multiplier, giving **3.15 / 5.5125 m/s**;
charging without Dash moves at **2.52 m/s**. Dash keeps its normal movement
while charging a shot. Dash and Team Turbo multiply these reduced
speeds once. Other sports, jump physics and tackle/hit impulses retain their
tuning. Kick launch speeds are **13.44-24.64 m/s** depending on charge,
20% below the previous 16.8-30.8 m/s. Only the shot launch receives this new
multiplier; dribbling continues to attach the ball to the player's forward foot.

## Receiving

A free, low ball attaches within **1.2 m horizontally around the player's
body**, regardless of facing: front, sides and rear all work. Once received,
it follows the player's forward foot. This applies to Explore Offline,
Play vs AI and host-authoritative room matches.

Walls, ability barriers, airborne players, active tackles/falls and high or
fast balls still prevent acquisition. A nearby opponent cannot take an
already-held ball by proximity alone. After kicking or being involved in a
tackle release, that player must leave the pickup area and approach again;
this prevents immediate recapture of their own release. Ball speed and this
fresh-approach rule are unchanged.

## Controls

Dash occupies the main thumb slot. Shoot and Pressure share the smaller
upper-left slot; Tackle appears only on defense. Pass replaces Jump. Hold Dash,
slide diagonally onto Shoot, then return to Dash to release without ending the
dash. See [passing and MVP](FOOTBALL-PLAYMAKER.md) and the current
[team controls and pocket maps](SPORTS-HUD.md). The approved
[Touchline UI](FootballUI/implementation.md) places stamina directly below
Dash and moves camera/guidance/leaving into the top-right settings gear.
The desktop keys below are testing shortcuts, not on-screen button captions.

| Action | Windows | Touch | Behavior |
| --- | --- | --- | --- |
| Run | Hold Shift | Push stick fully | Faster base movement, no stamina cost |
| Dash | Hold left Ctrl while moving | Hold Dash while moving | Runs 45% faster than the new run speed |
| Pressure | Hold Q | Hold Pressure | Automatically follows and guards an opposing carrier |
| Tackle | E | Tackle | Existing slide challenge and miss recovery |
| Shoot | Hold/release F | Hold/release Shoot, or swipe from Dash | Charge, aim and cancel; Dash continues |
| Pass | Space | Aim and tap Pass; hold to preview | Nearest teammate within the aim lane |

Each player has a bar above their head. A second stamina meter remains visible
for the local player in first person. Green shows available stamina, amber
shows dash, blue shows pressure and red shows exhaustion. Bars hide during
travel, blocked match actions, behind the camera and behind world geometry.

## Stamina

A full bar contains 100 stamina. Moving dash consumes **32 per second**, so a
continuous burst lasts approximately **3.1 seconds**. Dash speed is
**10.6575 m/s**, or **7.993125 m/s** while dribbling. Stamina starts recovering
after **0.6 seconds** without dash, at **15 per second**, including during
ordinary movement and pressure. Falling and blocked match phases cancel Dash;
charging a shot does not. Standalone Dash still cancels during jumping/tackling.
Holding Dash while stationary costs
nothing. Exhaustion requires releasing dash and recovering at least **25**
stamina before another burst; keeping it held cannot generate automatic bursts.
Round resets and teleports restore a full bar.

## Pressure

Pressure is available to a defender against an opposing carrier. During free
practice, any other player can defend. During matches, teammates cannot
pressure one another. It does not automatically steal possession: tackle and
positioning remain the means of winning the ball.

Within **8 m** and clear line of sight, holding Pressure automatically follows
the carrier, faces them and targets a **1.35 m** goal-side marking gap.
The defender keeps **70%** of their already-reduced movement speed: ordinary
chase is **5.145 m/s**. Within **2.4 m**, a close, facing defender slows the
carrier's total horizontal movement by **25%**, including sideways movement,
dash and Turbo. Each additional eligible defender adds 10 percentage points, capped at three defenders: **25%, 35%, 45%**. It does not multiply the base penalty by the defender count; a fourth defender adds no further slowdown. Each defender must independently satisfy the distance, facing, opposition and line-of-sight checks; leaving removes that stack immediately. Walls, height
differences, facing away, losing possession and action recovery remove the
effect. The captured pressure hold survives the defender's jump and tackle, and Dash may run simultaneously with Pressure using the same stamina costs and slowdown. A close (at most 1.55 m), clear pressure tackle faces the carrier and dislodges the ball through the normal hit/release rules. The tackle
cooldown remains **1.5 seconds**, with the existing **0.8 m** contact sweep.

The host computes movement, stamina and containment. Guests send only action
intent and receive quantized stamina plus dash/pressure/exhaustion state.
Protocol **55** requires matching Windows and Android peers. Input expiry,
focus loss, pointer release/cancellation, match resets and travel clear held actions. Leaving Pressure to reach a swipe target retains its owning finger; leaving the standalone Dash button still cancels Dash.

The feature uses existing UI images, fonts and the character rig, with a
smoothed defensive lean and faster distance-driven gait. It adds no imported
art, textures, audio, animation clips, shaders or runtime packages.

## Verification

`Tools/Build/Test-FootballEffort.ps1` runs offline checks; use `-Mode Local`
for host/guest checks and `-Capture` for rendered control views. Evidence is
stored under ignored `Builds/FootballPaceQA/`. The script resolves its default
player relative to its repository, independently of the caller's directory.
See [release measurements](BUILD-SIZE.md), [packing audit](APK-SIZE-AUDIT.md)
and [verification record](VERIFICATION.md) for the delivered build's results.

Learned AI has been retired. The [programmed bots](PROGRAMMED-BOTS.md) use
these same pickup and movement rules. Older checkpoints, reports and training
tools are recoverable from local Legacy archives, not part of active gameplay.
