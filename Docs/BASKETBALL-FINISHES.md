# Basketball layups and dunks

Press the existing **Shoot** button, slide **up for Dunk** or **down for Layup**,
and release. When the selected finish is eligible, **no charging or timing is
required**: the meter hides and the button says it is ready. An unblocked dunk
is a guaranteed make; a layup has an **85% make chance**, rolled by the host.
When the finish is unavailable, the meter remains visible and release performs
a **normal shot with its normal charge timing**. Sliding back to the centre
also restores a regular shot. The
selection stays stable around the threshold; another finger cannot change it.
Cancel sits to the left and above Shoot, clear of both vertical gestures.
On a keyboard, hold **E**, select with **Up / Down**, and release E. **Left /
Right** restores a regular shot; **X** cancels. Passing retains Q.

## Eligibility and fairness

The host measures the carrier's actual position, grounded state, facing and
movement. It checks the jump with swept capsules, the carried ball with swept
spheres, and the path against nearby players. Client requests supply a finish
choice and the existing bounded release timestamp, never a score, launch
velocity or jump destination.

| Rule | Layup | Dunk |
| --- | --- | --- |
| Horizontal distance from rim | 1.25–4.0 m | 1.05–4.0 m |
| Approach | Front or front diagonal, facing the rim | Same |
| Minimum movement toward rim | 1.0 m/s outside 2.0 m; close standing layup allowed | 1.6 m/s |
| Movement alignment | Within about 47 degrees of the rim direction when a run is needed | Same |
| Release timing requirement | None | None |
| Unblocked make rule | 85% host-side roll | 100% |
| Takeoff / release after gather begins | 0.24 / 0.60 s | 0.26 / 0.76 s |
| Complete action | 1.30 s | 1.55 s |

A requested finish outside these conditions falls back to a normal shot at the
locked hoop and preserves the charge phase at finger release. It does not get
a wider green band or automatic power correction. The button explains the
reason before release; only explicit Cancel discards a valid held shot.
Normal release-clearance checks can still prevent a ball spawning in a wall.

Host-observed movement provides a **0.20-second** allowance for planting just
before finger release. A stationary player cannot invent a run-up. Once a
valid request is accepted, movement brakes while a free dribble returns to the
hand; that accepted approach is retained for the gather, while grounding,
position and path clearance are rechecked. This avoids running past the rim
while waiting for the bounce. Jump, pass and repeated shot commands cannot
restart the committed action or reroll a layup. The normal steal rules remain
in place.

## Motion and the ball

The shared rig supplies a two-handed gather, opposite-foot plant, upward leg
drive, shooting-hand extension, follow-through and landing absorption. Left and
right finishes follow the approach side. A layup rolls from an upward-facing
palm into a short, soft arc. A dunk turns the palm over the ball, clears the rim,
and throws downward. Neither uses rim hanging or an airborne pause.

The actual CharacterController leaves the ground. Incremental movement sweeps
prevent a blocked body from snapping to the release destination. A collision
interrupts the guided motion; the existing jump gravity completes the fall.
Reset, possession loss and session changes cancel queued releases.

After release the ball is a normal rigid body. The existing rim and backboard
contacts, downward crossing, full-ball clearance and attempt accounting decide
the outcome. A dunk uses the perfect downward trajectory. A layup draws exactly
one uniform random value when the host commits its animation: below 0.85 takes
the make trajectory; otherwise it releases wide into a physical rebound.
The roll is independent of hold duration and power, and unavailable requests
do not consume it. There are no automatic points or corrections after release.
This keeps the ball available for future defensive blocks before it crosses
the rim. [Defensive blocks](BASKETBALL-DEFENSE.md) can now interrupt the exposed airborne carry or deflect a released shot.

## Source and validation

- [Rules, eligibility and launch](../Game/Assets/_Game/Sports/Basketball/BasketballFinish.cs)
- [Finish motor and pose](../Game/Assets/_Game/Sports/Basketball/BasketballFinishMotion.cs)
- [Gesture handling](../Game/Assets/_Game/Shared/UI/BasketballShootButton.cs)
- [Repeatable player review](../Tools/Build/Test-BasketballFinish.ps1)

Rooms require matching **protocol 29** builds so every peer uses the same
finish timelines and outcome rules. The review covers both hoops,
both hands, fallback shooting, cancellation, both layup outcomes, recovery and
host/guest input. `-GestureOnly` exercises mouse and touch through Unity's
actual input module, including off-button release, immediate finishes and
far-range normal-shot fallback. Use `-Mode Local` for two-player checks and `-Fps` for fixed-rate
motion reviews. Fixed-rate Windows captures do not measure phone performance.
Current task evidence is kept under ignored `Builds/BasketballFinishQA/`.

The original 5 October finish release and its 994 assertions are recorded in
[verification history](VERIFICATION.md#basketball-layups-and-dunks--5-october-2026).
The revised input, outcome and timing rules use current evidence under ignored
`Builds/BasketballGestureFixQA/`;
[release verification](VERIFICATION.md#untimed-finishes-and-shot-fallback--6-october-2026)
and [APK size records](BUILD-SIZE.md#untimed-finishes-and-shot-fallback--6-october-2026)
identify the latest validated delivery.
