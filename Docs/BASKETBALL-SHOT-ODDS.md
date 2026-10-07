# Basketball shot chances

The host makes one random outcome choice for each released normal shot. Good
timing improves the chance; it does not guarantee a basket. Poor timing retains
a small chance. Dunk and layup rules remain separate.

## Distance and timing

The distance is measured horizontally from the shooter's feet to the locked
basket at release. The following uncontested values are anchors, interpolated
continuously between distances. Being just over a painted line does not cause
a sudden probability change; corner threes use their actual shorter distance.

| Distance | Best timing | Worst timing |
| --- | ---: | ---: |
| Close, up to 4 m | 95% | 8% |
| Three-point arc, about 7.26 m | 80% | 4% |
| Half court, about 13 m | 30% | 1% |
| Full court, 26 m and beyond | 5% | 0.2% |

Timing quality is 1 throughout the current green window and decreases to 0
at either end of the power meter. Early and late error are normalized by the
available meter span on that side. The chance is:

`worst + (best - worst) × quality²`

For example, power 0.50 instead of the ideal 0.65 gives roughly 3.5% at full
court. Worse releases fall further. Guard pressure narrows the green window
and reduces the resulting chance by up to 25%. The HUD shows the current
estimated chance while charging.

## Physical and network behavior

Clients send input and the release timestamp. They never supply the result,
random number, probability or launch velocity. The host evaluates its measured
power, release distance and pressure, then selects the launch trajectory once.
Duplicate requests cannot reroll a queued shot.

A selected make launches toward the rim. A selected miss launches off target,
with larger short/long error on worse timing. Both remain physical balls:
there is no steering in flight, invisible basket rejection or score award from
the random number alone. Defenders, rim contacts and later deflections can
change the eventual outcome. A basket still requires a valid downward hoop
crossing. These percentages describe the initial unblocked attempt rather
than a promise about subsequent collisions.

Long heaves use a steeper descent to give the ball clearance over the front
rim. Basket selection also penalizes a basket approached from behind its
backboard, so a full-court attempt can target the far basket correctly.

`BasketballShotOdds.cs` holds the tuning curve. Development checks exercise
all anchors, monotonic timing/range behavior, seeded sampling, physical misses
with perfect timing and physical makes with poor timing. Outcome injection
for those branch checks is excluded from release builds.

See [passing controls](BASKETBALL-PASSING.md) for the shared input changes.

## Delivered validation — 7 October 2026

Protocol 32 is included in the complete `Builds/WindowsFinal/` player and the
17:27 Malaysia-time AndroidSubmission, together with the stadium camera update.
The shot tests cover continuous distance/timing interpolation, 400,000 seeded
samples per run, perfect-power physical misses, poor-power physical makes,
full-court trajectories and exactly one authoritative outcome per release.
Host/guest tests also give clients conflicting injected outcomes and verify
that only the host's choice controls the physical result.

The original basketball candidate passed 828 assertions across 14 reports.
The newer combined camera candidate subsequently passed the offline and
two-player trajectory suites. Original evidence and combined-build checks are
kept separately under `Builds/BasketballTrajectoryQA/`; the earlier animation
preview predates the camera change. Android packaging and signature checks pass;
physical phone touch, appearance, performance and WAN latency remain untested.
