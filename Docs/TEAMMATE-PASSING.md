# Direct teammate passing

Basketball and football support double-tapping a visible teammate's character
while possessing the ball. Windows also accepts two left clicks. Both taps must
hit the same teammate: each press lasts at most 0.25 seconds and the releases
are at most 0.34 seconds apart. A single tap does not throw. Existing Pass
buttons, camera lane selection, basketball charge/loft/bounce and keyboard
controls remain available.

The gesture works across the gameplay viewport, including the left side away
from controls. UI hits, camera drags, long holds, simultaneous world fingers,
opponents, obstructed characters, and a changed possession cancel or reject it.
A separate finger can keep holding the movement stick. Opening a menu, losing
focus, travelling or losing possession clears the gesture. A queued shot or a
held pass takes priority over a new direct pass.

The host resolves the requested receiver identity and rechecks possession,
team, distance (30 metres), height, obstruction and match state. Invalid
requests do not fall back to another teammate. Basketball validates again at
the animation's release; a receiver who becomes invalid leaves the ball with
the passer. Remote model picking works when remote physics colliders are
disabled. The nearest visible character blocks selection of one behind it.

Locked passes predict the receiver's current movement with a bounded lead
(at most 1.2 seconds or 6 metres). Basketball chooses direct-pass power from
distance and launches its usual chest-pass trajectory. Football retains a
grounded rolling pass. These remain physical balls: opponents can intercept,
and a receiver who changes direction after release can miss the ball.

The reliable receiver-ID RPC was introduced with network protocol **75**.
The current basketball block update uses **76**; all room members need matching builds. No new art, persistent settings or world-model changes
are required.

## Verification

`Tools/Build/Test-TargetPass.ps1 -Sport Basketball -Mode Offline` runs the direct
passing checks. `-Sport Football` selects football; `-Mode Local` uses a host
and a guest. `-PlayerPath` can select a candidate player. The default resolves
from the repository's script location, regardless of the caller's directory.
Evidence is written under ignored `Builds/TargetPassQA/`.

The runtime review exercises gesture timing, cancellation, UI/model picking,
disabled remote colliders, invalid receivers, exact target dispatch, physical
receipt, moving receivers and guest authority. These synthetic pointer checks
do not establish physical Android touch feel or phone performance.

Validated on 21 October 2026:

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

Current evidence: `Builds/TargetPassQA/`, including `player-manifest.json`,
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
