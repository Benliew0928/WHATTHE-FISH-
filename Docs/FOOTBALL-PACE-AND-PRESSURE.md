# Football pace, stamina and pressure

Football movement is 150% of the previous tuning. Walking is **6 m/s** and
running **10.5 m/s**. Dribbling retains its 75% possession multiplier, giving
**4.5 / 7.875 m/s**; charging moves at **3.6 m/s**. Other sports retain their
movement tuning. Kick launch speeds are 140%: **16.8–30.8 m/s** depending on
charge. Slide ball impulses and the controllable-ball speed threshold use the
same ball multiplier. An attached ball follows its carrier exactly.

## Controls

Kick and Pressure now share one possession-aware action slot; Tackle appears
only on defense. Dash remains available in either role. See the current
[team controls and pocket maps](SPORTS-HUD.md).

| Action | Windows | Touch | Behavior |
| --- | --- | --- | --- |
| Run | Hold Shift | Push stick fully | Faster base movement, no stamina cost |
| Dash | Hold left Ctrl while moving | Hold Dash while moving | Runs 45% faster than the new run speed |
| Pressure | Hold Q | Hold Pressure | Faces a nearby carrier and contains movement toward the defender |
| Tackle | E | Tackle | Existing slide challenge and miss recovery |
| Kick | Hold/release F | Hold/release Kick | Existing charge, aim and cancel gesture |

Each player has a bar above their head. A second stamina meter remains visible
for the local player in first person. Green shows available stamina, amber
shows dash, blue shows pressure and red shows exhaustion. Bars hide during
travel, blocked match actions, behind the camera and behind world geometry.

## Stamina

A full bar contains 100 stamina. Moving dash consumes **32 per second**, so a
continuous burst lasts approximately **3.1 seconds**. Dash speed is
**15.225 m/s**, or **11.419 m/s** while dribbling. Stamina starts recovering
after **0.6 seconds** without dash, at **15 per second**, including during
ordinary movement and pressure. Charging a kick, tackling, falling, jumping
and blocked match phases cancel dash. Holding dash while stationary costs
nothing. Exhaustion requires releasing dash and recovering at least **25**
stamina before another burst; keeping it held cannot generate automatic bursts.
Round resets and teleports restore a full bar.

## Pressure

Pressure is available to a defender against an opposing carrier. During free
practice, any other player can defend. During matches, teammates cannot
pressure one another. It does not automatically steal possession: tackle and
positioning remain the means of winning the ball.

The defender strafes at **6.6 m/s** and faces the carrier when within six
metres with a clear line of sight. Within **2.4 m**, a close, facing defender
reduces the carrier's movement component toward them by up to **60%**.
Sideways escape and retreat keep their speed. Only the strongest containment
applies, so multiple defenders cannot stack a complete movement lock. Walls,
height differences, facing away, losing possession and action recovery remove
the effect. Pressure takes precedence over simultaneous dash input.

The host computes movement, stamina and containment. Guests send only action
intent and receive quantized stamina plus dash/pressure/exhaustion state.
Protocol **34** requires matching Windows and Android peers. Input expiry,
focus loss, pointer exit, match resets and travel clear held actions.

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
