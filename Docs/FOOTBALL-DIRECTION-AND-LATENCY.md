# Football ends and multiplayer response — 18 October 2026

Blue (team A) defends the negative pitch-Z end; red (team B) defends positive Z.
Each goal now has a matching frame, an apron outside the goal line, short
touchline ribbons and two low boards labelled BLUE GOAL or RED GOAL. The pocket
map uses the same colours. These identify the **defending** side: shoot at the
other colour. Markers follow the authored pitch through island rebasing and
unloading. They reuse meshes, fonts and materials, add no collision, and leave
white field markings, goal openings and scoring dimensions intact.

## Why guests felt delayed

The previous movement path was input → host simulation → replicated transform
→ interpolation → the guest's camera. A guest did not predict their own motion.
The host moved on its own render frame, so it avoided the round trip entirely.
The same host-authority rule applies to kicks, possession and scoring.

Source inspection found additional avoidable delays:

- `NetworkTransform` had its legacy interpolation buffer **and** a second
  smoothing pass with a 0.1-second time constant. That is a response filter,
  not a fixed 100 ms wait. It lengthened start/stop/turn settling.
- Transform deltas were reliable, so a lost packet could hold newer poses
  behind retransmissions on that ordered channel.
- Input sampling waited for a periodic send even on a fresh movement edge.
  Resetting the timer discarded the remainder on render rates not divisible
  by 30. Unsequenced application input could accept an older UDP command.
- Football reset its 0.05-second publish timer on a 0.02-second physics
  clock, producing a 0.06-second interval (about 16.7 Hz), followed by another
  0.05-second ball blend. A new kick could wait for that publish window.

These are code-confirmed contributors, not a measurement of the teammates'
actual Relay route, Wi-Fi or devices. The Relay SDK already uses automatic
region selection; no evidence justifies forcing every room into one region.

## Changes

Network protocol **69** requires everyone to use the same new build.
`NetworkResponsiveness` retains the one-tick interpolation buffer for other
players but removes the second smoothing pass. `ResponsiveNetworkTransform`
presents the owned Football/Basketball player's newest accepted host pose with
a short 25 ms smoothing time constant, bypassing the clock-based remote-player
buffer. It handles partial-axis deltas and snaps authoritative teleports. It
falls back to ordinary interpolation during travel, recovery and spectating.
This is faster authoritative presentation, not prediction. Moving deltas use unreliable delivery;
NGO retains reliable teleports and periodic axis repairs. Position and rotation
thresholds are 1 cm and 0.25 degrees instead of 1 mm and 0.01 degrees.
This reduces tiny updates without changing physical movement.

Movement starts, stops, meaningful direction changes and held-action changes
send immediately. Ordinary input retains a 30 Hz heartbeat, with bounded timer
carry and no burst catch-up after a hitch. Sequence numbers reject duplicate
or reordered input, including counter wrap. Existing timeout, ownership,
round, travel and spectator checks remain.

Football publishes at 30 Hz with a carried timer remainder, publishes changed
ball sequences on the next physics step, and blends over one 30 Hz interval.
Discrete actions remain reliable, and only the host decides whether they
succeed. No client supplies authoritative positions, scores or launch velocity.

Unity's [NetworkTransform documentation](https://docs-multiplayer.unity3d.com/netcode/current/components/networktransform/)
describes interpolation's latency tradeoff and unreliable delta recovery. The
installed package source was checked for the exact legacy-smoothing behaviour.

## Scope and further work

These changes remove avoidable buffering; **they do not eliminate the network
round trip**. Immediate own-player motion over a slow WAN requires a larger
prediction/reconciliation change: retain numbered local inputs, simulate a
side-effect-free movement model locally, acknowledge host snapshots, replay
unacknowledged inputs, and correct collisions/teleports/possession transitions.
Shots can anticipate animation and sound immediately, but ball flight must be
reconciled and rejected shots must unwind cleanly. Calling the current full
`Athlete.Simulate` on guests would also run ball/action side effects and is not
a safe shortcut. That architecture is not part of this release.

At capacity, host work and outgoing replicated state grow with the number of
participants and recipients. A 12-process local test exercises ten player
places and two spectators; it cannot qualify a phone host, a live Relay route,
or sustained device FPS. An actual teammate retest should record host/client
platform, ping, packet loss, and whether the remaining wait is movement, ball
release, or frame stutter.

## Reproduction and evidence

Build the complete candidate with Unity method `ResponsivenessBuild.Build`.
Use `ResponsivenessBuild.Scripts` only after that full build for source-only
review iteration. The candidate is under `Builds/ResponsivenessQA/Player/`.

Run `python Tools/Build/test_responsiveness.py --players 3 --delay-ms 60
--jitter-ms 8`, with `--baseline` for the old buffering/rates in the same build.
Repeat with `--loss 0.02 --loss-after-join` and `--players 12`. This isolates
gameplay loss from admission; omit `--loss-after-join` to impair admission too.
A real loopback UDP proxy delays
each direction, adds deterministic jitter/loss, and connects separate Unity
processes. It records six start/stop trials per playing peer, visible kick and
animation response, connection assertions and actual datagram counts. Capacity
runs also record the existing transport queue/RTT/retransmission diagnostics.
This comparison isolates networking settings; the baseline still includes the
new input sequence guard and cosmetic markers.

Goal captures use `-offline -sport Football -goalIdentityReview <output-folder>`.
The review checks end ownership, frame tint, labels and collision, then captures
both ends close and wide. Output and raw logs stay under ignored `Builds/`.

## Measured response

The final gameplay binary was compared against its development-only baseline
switch. Each direction had 60 ms added delay and up to 8 ms jitter. These are
rendered-transform measurements, not physical keyboard-to-screen measurements.
Each playing guest performed six start/stop trials in a clear lane.

| Three peers, guest median | Previous settings | New settings |
| --- | ---: | ---: |
| First 3 cm of movement, no loss | 315.88 ms | 232.74 ms |
| First 25 cm of movement, no loss | 399.20 ms | 282.59 ms |
| Stop settling, no loss | 667.90 ms | 366.88 ms |
| First 3 cm, 2% loss after joining | 333.36 ms | 216.44 ms |
| First 25 cm, 2% loss after joining | 400.11 ms | 282.98 ms |
| Stop settling, 2% loss after joining | 692.04 ms | 366.72 ms |

The no-loss movement-onset reduction is approximately 26%; stopping settles
45% sooner. Guest shot visibility was approximately 233 ms in both no-loss
variants. The small shot sample and variability **do not establish a shooting
response improvement**. The faster ball publisher is implemented, but host
round-trip and reliable ball-state delivery still dominate this measurement.

The 12-peer test passed with ten players moving concurrently and two spectators,
120 ms added round-trip delay, jitter and 2% loss after admission. Guest medians
were 233.15 ms to first movement, 299.50 ms to 25 cm, and 400.08 ms to settle
after release (worst settling trial 666.58 ms). All 12 remained connected through
the test; sampled host transport queues were empty at completion. The proxy
forwarded 30,146 datagrams / 7,411,268 payload bytes over the 57-second run and
dropped 547 datagrams. This is a short local test, not a sustained mobile-host
or WAN capacity guarantee. Runtime transport retransmissions still occurred.

Final A/B evidence directories under `Builds/ResponsivenessQA/`:

- No loss: `20261018-163637-baseline-3peers`, `20261018-163504-fixed-3peers`.
- Loss after joining: `20261018-164103-baseline-3peers`, `20261018-164227-fixed-3peers`.
- Capacity: `20261018-164356-fixed-12peers`.

One separate run with loss enabled **during admission** did not admit one of
the three peers (`20261018-163827-fixed-3peers`). The host recorded a remote
disconnect; the missing peer never spawned. Its cause is not isolated or fixed
by this change. Keeping the failed evidence and explicitly separating the
after-join tests avoids claiming that unreliable-network admission is solved.

## Visual and regression validation

Both goal ends were checked close and wide using the default Windows D3D12
renderer and D3D11. Each renderer passed 11 identity/colour/label/collision
assertions; captures are in `Builds/ResponsivenessQA/Goals-default/` and
`Goals-d3d11/`. The screenshot's `Fake or uninitialized surface` / `Not inside a
Renderpass` messages did not reproduce in either run or the delayed-peer tests.
The saved prior player logs also lacked those messages. No graphics-backend
change is presented as a fix for this unresolved rendering diagnostic.

The older movement-cadence harness began when exploration first became true,
before the room's arrival animation released its input lock. Its initial host
run therefore measured locked input as zero movement. The review now waits for
the existing `ReadyToReviewSport` condition; production movement is unchanged
by that harness correction. Original failed evidence is retained.

Final Windows regressions passed: 56 movement-cadence assertions (both sports,
offline and host, 30/60/120 FPS), 292 football-ball assertions, 513 net assertions,
245 travel/match-start assertions across all four islands, and both protocol
68/69 mismatch directions. The 12-peer run passed 156 assertions. The only
source change after the latency tests was the development-only movement-review
readiness correction; source hashes document that boundary.

The full Windows player was built at 16:35 MYT and its review scripts refreshed
at 16:49 MYT. All player data, runtime DLLs, launchers and supporting folders
are delivered together in `Builds/WindowsFinal/`; the complete ZIP and SHA-256
sidecar are in `Builds/ResponsivenessQA/`. See [teammate testing](TEAMMATE-TESTING.md).
Superseded player/APK/audit output and the temporary path-check clone were
archived with inventories in
`Legacy/20261018-165242-181-football-direction-response/`.

Actual Android APK bytes and entry-size changes are recorded in
[build size](BUILD-SIZE.md) and [the APK audit](APK-SIZE-AUDIT.md): the 16:55:46
MYT `AndroidSubmission` passed at **87,592,176 bytes** (+22,992), leaving
912,407,824 bytes below the strict one-billion-byte cap. APK signature and ZIP
integrity verification passed. No physical Android device was connected.
