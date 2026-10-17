# Hosted sports movement

Current football pace is **7.35 m/s running** following the 14 October 2026
30% locomotion reduction; basketball remains **7 m/s**. The cadence probe now
checks those values. See [football pickup and pace](FOOTBALL-PACE-AND-PRESSURE.md).
The measurements below describe the earlier host-cadence fix at its original
tuning, not the current football speed.

## Host frame cadence — 12 October 2026

The original host moved its own athlete in `NetworkAthlete.FixedUpdate`, at the
project's approximately 50 Hz physics cadence. Explore Offline instead calls
the same athlete motor once per rendered frame. Netcode's server-authoritative
`NetworkTransform` interpolates received positions on clients, but skips
interpolation on the authoritative host. Its prefab's `Interpolate` setting
therefore did not smooth the host's own movement.

The instrumented original player reproduces this without another participant
or internet transport. At a controlled 60 FPS, both football and basketball
have **10 stationary rendered frames per second**, followed by larger movement
steps. At 30 FPS there are no stationary frames, but distance alternates between
one and two physics steps. At 120 FPS there are **70 stationary frames** per
second. Average running speeds remain correct: football **10.5 m/s**, basketball
**7 m/s**. Basketball shares the cause even when it is not noticeable during
ordinary play; its smaller travel per step can make the effect less apparent.

The fix applies only to the host's own football/basketball character. It accepts
local movement every frame and runs the existing authoritative motor with that
frame's duration, matching offline play. The host no longer also runs that
character in FixedUpdate. The motor retains bounded collision substeps, wall
collision, stamina, ball interactions and action validation. Remote participants
continue to simulate on the server's fixed clock and receive interpolated
network transforms. Physics settings, guest input rate, running speeds and room
protocol **45** are unchanged. Golf, fishing and travel rules retain their
existing timing paths.

| Render cadence | Original host stationary frames / second | Fixed host | Offline |
| --- | ---: | ---: | ---: |
| 30 FPS | 0, but uneven distances | 0, even distances | 0, even distances |
| 60 FPS | 10 | 0 | 0 |
| 120 FPS | 70 | 0 | 0 |

These results cover both sports and both chase/stadium views. The fixed host
matches offline movement without changing the 10.5 m/s football or 7 m/s
basketball sprint pace. This addresses the reported host-local symptom. Remote
player prediction and internet-latency compensation are separate concerns;
guest gameplay is checked with local host/client regression suites.

## Reproducing the measurement

`Tools/Build/Test-NetworkMovement.ps1 -Build` builds the development player in
`Builds/NetworkMovementQA/Player/` and checks both sports offline and in a solo
hosted room, using chase and stadium views at 30, 60 and 120 FPS. CSVs record each
rendered displacement, frame duration, speed and stationary-frame count. The
probe is command-line opt-in and absent from release code. `-Baseline` records
cadence without enforcing the new smoothness assertions; use the original
instrumented player for a before comparison.

Unity capture time makes this a deterministic simulation/render cadence test.
It is not a phone performance benchmark or a measurement of internet latency.
Before results are in `Builds/NetworkMovementQA/Baseline-20261012-174018/`;
the original instrumented player is recoverable in
`Legacy/20261012-174217-734-sports-movement-baseline/`.
Final movement results are in `Builds/NetworkMovementQA/Fixed-20261012-180526/`.
All 24 sport/mode/camera/rate combinations have zero stationary movement frames;
56 assertions check authority, expected pace, even movement and stopping.

The rendered ability reviews now avoid advancing or sampling short-lived
effects after synchronous screenshot readback: football observes a fresh,
unaccelerated five-second countdown, and basketball measures the receiving hand
before capturing the portal. These are development-only review changes. Failed
diagnostic runs are retained separately from the passing delivery evidence;
headless graphics assertions do not count as visual validation.

Final validation and delivery are recorded in [VERIFICATION.md](VERIFICATION.md)
and [BUILD-SIZE.md](BUILD-SIZE.md).
