# Predictive teammate passing

Basketball and football now commit Pass with one button tap (Q in basketball,
Space in football). Holding, dragging and lifting Pass do not charge, aim or
fire a second pass. Shooting keeps its existing charge controls. The selected
teammate is the eligible player with the smallest angle to the camera heading
in the forward hemisphere, within 30 metres; distance breaks angle ties.
Scenery blocks selection. With no eligible teammate, a tap sends a forward
practice pass. A local marker identifies the current automatic target.

Double-tapping a visible teammate still chooses that exact character. Each
press lasts at most 0.25 seconds, with releases at most 0.34 seconds apart.
A single character tap does not throw. UI hits, camera drags, long holds,
obstructed characters, opponents, stale possession and simultaneous world
fingers reject the gesture. A separate finger can keep moving the player.

The host rechecks possession, team, range, height, scenery and match state.
Basketball also rechecks the receiver at its animated release. Locked passes
predict current running velocity and travel time. Small corrections can follow
normal motion; a sharp turn, reversal, stop or large displacement permanently
returns the destination to its original release-time prediction. Correction
acceleration and heading are bounded, so the ball cannot U-turn to chase a
receiver. Rotating without changing running velocity does not break a pass.

The physical ball uses the same receiving reach for all players. The first
reachable teammate or opponent in its swept path can take it; the selected
receiver has no priority over an interceptor. Clear passes into normal reach
are collected automatically, including moving receivers. Wall contacts,
resets and possession changes end assistance. Sharp course changes can leave
the ball at the original predicted destination, as intended.

A shared ribbon appears on initiation/gather and remains during flight.
Flight state is replicated from the host and rendered on the ball, independent
of a player's controls, so remote players and spectators can see it. Catching,
interception and resets remove it. Network protocol **79** includes this state;
all room members need matching builds. Existing materials and models are reused.

## Verification

The 23 October 2026 predictive-pass checks passed **240 assertions**:
Basketball offline 69, host 40, guest 11; Football offline 70, host 39,
guest 11. Coverage includes immediate button-down commitment without holding,
angle-over-distance selection, preserved double-taps, stationary/crossing/
away/diagonal catches, a long boundary run, teammate and opponent interception,
sharp-turn fallback without reversed flight, reset cleanup, invalid receivers
and replicated host flight paths. Older protocol-78 players were rejected in
both host/client directions by protocol 79 before spawning.

All 240 checks passed again on the exact combined tutorial/passing player,
from a folder with spaces and an unrelated working directory. Final evidence
under ignored `Builds/PredictivePassQA/Portable Review/Builds/TargetPassQA/`:
`Basketball-Offline-20261023-131917-580/`,
`Football-Offline-20261023-131917-583/`,
`Basketball-Local-20261023-132018-586/` and
`Football-Local-20261023-132018-592/`.
The extended fixture refreshes the shot clock between independent scenarios;
it does not change live match rules. The court-boundary prediction now clamps
the expected destination into the playable court instead of falling back to
the runner's earlier position. These tests use scripted Windows input and local
networking, not physical Android touch or WAN latency measurements.

At the earlier passing delivery, the complete 327-file tested player was hash
verified on promotion to `Builds/WindowsFinal/`. The combined tutorial checks
passed 201 assertions across all 58 lessons. That AndroidSubmission passed at 110,706,458 bytes,
with 889,293,542 bytes of headroom. ZIP contents, exact size-audit entries,
five-scene catalog/bundle hash and APK v2 signature passed. See the
[current release record](BUILD-SIZE.md) for before/after measurements and
evidence. The prior Android/audit and Windows deliveries are recoverable from
`Legacy/20261023-132008-386-predictive-pass-tutorial-android/` and
`Legacy/20261023-132255-097-predictive-pass-tutorial-windows/`.

`Tools/Build/Test-TargetPass.ps1 -Sport Basketball -Mode Offline` runs the direct
passing checks. `-Sport Football` selects football; `-Mode Local` uses a host
and a guest. `-PlayerPath` can select a candidate player. The default resolves
from the repository's script location, regardless of the caller's directory.
Evidence is written under ignored `Builds/TargetPassQA/`.

The runtime review exercises gesture timing, cancellation, UI/model picking,
disabled remote colliders, invalid receivers, exact target dispatch, physical
receipt, moving receivers and guest authority. These synthetic pointer checks
do not establish physical Android touch feel or phone performance.

Historical verification on 21 October 2026 (before the single-tap change):

- Direct passing: **178 assertions** across both sports (basketball offline 39,
  host 40, guest 10; football offline 40, host 39, guest 10). Both running
  receivers caught physical passes. Guest checks include rejection of stale
  possession, stale rounds and unknown receiver IDs.
- Existing basketball pass suite: **30 assertions**, including held charge,
  cancellation, shot/pass conflicts and release direction.
- Football playmaker regressions: **389 offline, 91 host and 11 guest
  assertions**, including 2v2 through 5v5, pass locks, swipes, interceptions,
  assists and shots. Its obsolete portrait-only MVP check now validates the
  existing animated cast and waits for interactive results.
- Both offline suites passed with default player paths from the `Portable
  Review` folder, launched from an unrelated working directory. All **327
  player files** matched the tested candidate by SHA-256 on promotion to
  `Builds/WindowsFinal/`. Rendered gameplay and control guides were inspected.

Historical evidence: `Builds/TargetPassQA/`, including `player-manifest.json`,
`source-final.json`, local basketball run `Basketball-Local-20261021-150821-493`,
football run `Football-Local-20261021-150330-637`, and the `Portable Review`
offline results. Regressions: `Builds/BasketballPassQA/Reviews/Offline-30-20261021-150821-512/`
and `Builds/FootballPlaymakerQA/Run-20261021-151039/`.

Earlier failed runs remain as development evidence. The basketball guest
failure exposed a real preview issue: the participant dictionary is host-only.
Replicas now consult replicated teams, with exact membership revalidated by
the host. Other fixture corrections wait for arrival/countdown and keep the
open-lane movement trial clear of defenders; physical interception remains
covered separately.

Archives: `Legacy/20261021-151145-857-teammate-pass-android/` preserves the prior
APK; `Legacy/20261021-151303-191-teammate-pass-windows/` preserves the superseded
Windows player, the duplicate portable player and completed editing helpers.
No staging, commit or push was performed. Physical Android touch/FPS and
real WAN latency remain unverified.

Fresh AndroidSubmission at **15:15:58 Malaysia time** measured
**106,371,104 -> 106,372,384 bytes (+1,280)**, with **893,627,616 bytes** of
headroom. The build gate, ZIP/audit entries, five-scene bundle hash and v2
signature passed. See [the release size record](BUILD-SIZE.md).
