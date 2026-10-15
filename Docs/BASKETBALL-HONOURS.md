# Basketball match statistics and MVP

Competitive basketball records every participant, including bots, in VS AI
2v2 through 5v5 and host rooms. Free practice keeps its existing shared shot
score. The host owns the match ledger and replicates it to every client;
protocol 61 is required on all players.

- Points use the actual accepted basket value: two or three. The released
  shot retains its shooter if a defender deflects it into the hoop. An own
  basket adds to the attacking team's score without rewarding the opponent
  with individual points.
- A completed pass requires an intentional Pass release and a different
  teammate taking possession within five seconds. Charge, cancellation,
  dribbling, self recovery and catching a shot do not count as passes.
- An assist requires that direct receiver to score within five seconds of
  the pass release. Another player's touch, a miss/rebound, a restart or
  another pass breaks that attribution. Only the final direct passer earns
  the assist; a teammate may dribble before the shot.
- A pass interception is the first opponent contact that blocks an active
  pass. It counts once, even if the defender subsequently catches the ball.
  Blocking a shot or stealing a held ball is not a pass interception.
- MVP rating is **points + assists**. Equal ratings prefer scoring points,
  then completed passes, then a stable player ID. Thus two assists equal
  one two-point basket, and three equal one three-point basket. All match
  participants are eligible, including the losing team.

The award appears after the match is finished, including the last legal
buzzer shot or golden basket. It uses the real basketball court and seating
from an eye-level camera, an orange basketball illustration, blue/red team
accents, a large illustrated character and oversized point/assist totals.
Completed passes and pass interceptions appear beneath them. **Match stats**
shows all players; **Back to court** restores controls and the previous camera.
The completed/cancelled match releases the court's player boundary for everyone.
Each player can leave the court after closing their own MVP screen; other players
may continue reading their results. The energy boundary fades out too. Starting
a new match restores containment, including for players who previously enabled
free roam. Practice retains its existing explicit free-roam toggle.
The composition fits landscape safe areas and uses the existing ten character
portraits and basketball icon atlas without adding texture or environment copies.

Implementation: `BasketballStatistics.cs`, `BasketballMatch.cs`, the physical
release/catch/scoring hooks and `BasketballMVP.cs`. Rule checks run through
`BasketballHonoursBuild`; `Tools/Build/Test-BasketballHonours.ps1` exercises the
rendered Windows player and host/client replication. The scripted fixtures
disable bot movement to make pass and scoring attribution deterministic;
they are development-only and are excluded from the submission APK.
