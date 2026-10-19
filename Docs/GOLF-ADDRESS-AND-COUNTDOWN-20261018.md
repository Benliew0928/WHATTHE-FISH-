# Golf address orbit, continuous countdown and pond support

## Targeted changes

- `GolfFinalCountdown.Show` initializes the introduction against the shared host
  deadline, then uses unscaled render time for its 0.5-second hold and 0.8-second
  smooth transition. The host still owns countdown expiry. A separate nested
  Canvas isolates the badge batch; no per-frame sibling reordering. Sprites and
  numeral geometry update only when the integer second changes; the clock mesh
  refreshes only while its angle changes. The steady corner numeral containers
  are 65 x 65 UI design units; s is proportional (41.655 x 45.775). Responsive
  narrow screens may scale the whole card down further. All seconds, including
  30, use the same matched numeral sprites to avoid a baked-to-dynamic size jump.
- `GolfFlagWind.Wave` coefficients are 0.211764706 and 0.088235294 m, giving a
  0.30 m one-sided bound at the free edge, 0.60 m peak-to-peak bound. The quadratic
  pinning envelope and original wave periods remain. Existing flag mesh Z bounds
  expanded to +/-0.36 m; texture and geometry payloads are unchanged.
- `GolfCartMotor.WheelFloor/Surface` use the actual pond-bank mesh height and
  the water plane inside the footprint. The crest-to-water join is smooth.
  Four wheel heights determine chassis tilt. Body sweeps delegate only these
  banks to wheel support; rocks, other carts and sea restrictions remain.
  Public walking/exit Floor remains the real terrain query.
- `GolfBallTrail` still emits at the displayed ball transform origin. Removing
  the rounded forward cap and tapering the newest section to zero width prevent
  the head protruding past the ball while the camera rotates.
- `GolfTargetIndicator` retains the same bearing arrow in Aim, scaled with the
  existing Aim card scale and positioned above that card. The separate offscreen
  arrow is suppressed to avoid two conflicting direction arrows.
- `GolfAiming.AimOrbitDisplacement` follows a bounded 120-degree/s circular arc,
  retaining the initial side-on stance radius. Mouse/touch look yaw and horizontal
  keyboard/stick input can rotate the aim. Normal CharacterController movement
  applies the displacement and the existing NetworkTransform replicates it.
  Full controller/skin clearance stops at obstructions instead of sliding off
  the circle or jumping across a blocked route. The ball is never repositioned.
- `AimEligible`, `CanStrike`, aim validation and pending-contact validation require
  the actor's match player ID to equal the ball owner. Candidate selection,
  charge/aim RPCs and authoritative swings share this rule. Network schemas,
  charge curve, shot strength and trajectories are unchanged.

## Verification

Focused evidence and fresh build records are under ignored `Builds/GolfRefineQA/`.
The existing `GolfPolishProbe` is extended by the uniquely named partial
`GolfMotionRefineProbe`; the older unrelated `GolfRefineChecks` is retained.

Tests cover continuous rendering with a frozen network timestamp, fixed digit
sizes, steady-state allocation counts, local circular stance/radius and blocked
arcs, charging during orbit, eight camera directions for centered trail emission,
all six ponds at water height and in both directions, own-ball aim, actual remote
orbit replication, forged foreign-ball RPC rejection, and existing rim scoring,
celebration, countdown and local host/client behavior. Final checks passed: **141 offline / 49 host / 53 client** in
`Run-20261018-224954/`. The complete WindowsFinal player is updated, with 325
binary/support files compared by SHA-256. AndroidSubmission: **88,550,326 bytes**,
v2 signature verified, **911,449,674 bytes** headroom. See
[verification](VERIFICATION.md) and [size audit](APK-SIZE-AUDIT.md).

No physical Android visual/FPS or WAN-play claim is made from desktop evidence.
