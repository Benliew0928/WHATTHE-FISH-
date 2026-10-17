# Basketball and football room matches

## Modes

Explore Offline remains an untimed sandbox for testing controls, props and island
travel. Its normal UI cannot start a competitive match. Explicit development
probes may create fixtures without an online room.

Both sports also offer **Play vs bots**, a separate 2v2-5v5 solo match with
these same rules. Hosts can fill room rosters with programmed bots before
starting. See [programmed bots](PROGRAMMED-BOTS.md).

A room travels together to an island, warms up, then its host starts the match.
Basketball accepts 4-10 players (2v2 to 5v5) so every inbounder has a teammate; hosts can fill with bots. Football accepts 2-10 players (1v1 to 5v5). Team selection lasts
10 seconds; unselected players fill shuffled available seats. Accepted choices
are preserved and full teams reject extra players. Teams lock before the
three-second opening countdown. Both sports default to three playing minutes;
the host may choose three, five or ten minutes before everyone readies up.

Only the host simulates scoring, possession, restarts and results. Guests receive
snapshots and send owner-authorized inputs. Protocol 67 requires matching builds.
There is one active match in the room. Travel is blocked until that match finishes
or is voided; the four-island shared-travel model is retained.

## Basketball

- Team A defends the negative court-Z end and attacks the positive end. Team B
  does the reverse. Court-local hoop positions determine ends, not the imported
  North/South names, world orientation, camera heading or the player's facing.
  Normal shots, finishes and Lock-on all target that team's attacking basket.
- Two points inside/on the arc; three outside. A physical pass/deflection into
  either basket awards two to the team attacking that basket. Normal shots use
  the shooter's feet at release to determine value. Upward/side entry does not
  score, and repeated contact cannot duplicate a basket.
- The opening ball is at centre with mirrored team formations. First possession
  starts a 20-second shot clock. Passing or recovering your own team's loose ball
  does not reset it. Opponent possession or a shooting-rim contact resets it.
- A legal shot released before shot-clock expiry can finish its flight. An
  expired-clock miss/airball awards the other team possession. A queued shot or
  charge still in the player's hands at expiry is not a released shot.
- After a basket, the conceding team inbounds just outside its baseline, 2 m
  to the right of its hoop as viewed from the court. This is closer to the stand
  than the midpoint to the corner. Out-of-bounds restarts retain the exact exit
  point, shifting only along that same line if architecture blocks standing.
  Shot-clock violations use a sideline restart.
- Restarts preserve player positions and have no global freeze or formation reset.
  Inbound duty rotates in stable player order separately for each team. The
  assigned player runs to a clear spot outside the line and collects the ball;
  their movement is automatic until arrival. After a basket, scoring players
  outside their own half automatically run back and regain control individually
  on reaching it. Players already in their half keep control. The receiving
  teammates remain free to move and jump throughout. A small top HUD notice
  explains either automatic run to the affected player.
- Scoring players stay in their half until the inbound pass. The inbounder stays
  planted outside the line, cannot jump/shoot/use abilities and must pass to a
  teammate. Free-direction or outward inbounds are rejected. Both clocks stay
  paused until the physical pass releases; that release unlocks centre crossing
  and starts the 20-second clock. Unused props remain in each player's single
  slot; another pickup replaces them. Active ability effects clear on restart.
  Inventory clears when the match ends or is cancelled.
- A shot physically released before the match buzzer can resolve afterward.
  Players cannot take a new shot or rebound after the buzzer. Flight resolution
  is bounded to eight seconds from release, including stuck balls. A ball still
  held/charging at the buzzer receives no extension.
- A tied regulation period enters 60 seconds of golden-basket overtime. The
  first basket wins; no basket produces a draw. A tying buzzer basket grants the
  conceding team the normal restart before overtime. Finished results cannot
  accept more points.
- There are no free throws, foul stoppages, halves or shootouts. Existing steal,
  guard and block restrictions still apply. Free roam cannot be enabled during
  a competitive match.

## Football

The existing [football match rules](FOOTBALL-MATCH.md) remain: a whole-ball goal
is one point, including an own goal for the opposing side. A goal gets 0.65 seconds
of net follow-through, then players and ball reset for a three-second centre-ball
restart. Both teams are released together. Resets do not consume playing time.
Unused props survive goals and kickoff resets in their collector's single slot;
another pickup replaces them. Goals clear active effects. Match completion,
cancellation or travel clears held inventory.

A tied regulation period continues directly into 60-second golden-goal overtime;
the first goal wins, otherwise the result is a draw. A goal must complete before
the relevant deadline; there is no airborne-ball grace period. Pitch containment
keeps the ball in play without corners or throw-ins. No penalties, free kicks,
offsides, halves or shootouts are added.

## Departures and match isolation

If any participant departs during selection, countdown, live play or a restart,
the host immediately marks the match void, clears pending input and active ball
actions, and prevents further scoring. The visible score is retained without
awarding a winner. A new match needs a fresh valid even roster and clears all
scores. Host departure instead uses [automatic host succession](HOST-MIGRATION.md)
to restore the surviving roster and match checkpoint under the next numbered
participant. Replacement bots are not added. Returning to the lobby clears the
current match.

Ball and input revisions prevent late previous-round network requests from
executing after a reset. Selection requests also carry their selection revision.
Offline practice scoring, solo boundary recovery and exploration are separate
from the competitive state.

## Implementation and verification

- [Basketball match state](../Game/Assets/_Game/Sports/Basketball/BasketballMatchState.cs)
  holds deadline, possession, scoring and outcome rules without scene dependencies.
- [Basketball match controller](../Game/Assets/_Game/Sports/Basketball/BasketballMatch.cs)
  owns roster, fixed hoops, opening formation, inbound turns and snapshot publication.
- [Individual restart movement](../Game/Assets/_Game/Sports/Basketball/BasketballRestartMovement.cs)
  runs affected players through the normal character motor and replicates their
  control locks. Inbound turns and collection state survive room checkpoint recovery.
- [Football match controller](../Game/Assets/_Game/Sports/Football/FootballMatch.cs)
  checks the locked roster before scoring and every active match step.
- [Pure rule checks](../Game/Assets/_Game/Shared/Core/RoomMatchRuleChecks.cs) and
  [runtime review](../Game/Assets/_Game/Shared/Core/RoomMatchReview.cs) cover the
  competitive flow separately from existing sport mechanics regression suites.

Run `Tools/Build/Test-RoomMatches.ps1 -Build` from any working directory by using
its path in this checkout. Results, rendered desktop/mobile-aspect captures and
raw logs belong under ignored `Builds/RoomMatchQA/`. This command builds a complete
development Windows player in `Builds/RoomMatchQA/Player/` and tests offline
isolation plus two-process host/guest rooms. `-Width 844 -Height 390 -OfflineOnly`
checks the compact HUD aspect. These are desktop checks, not phone qualification.

The major match-rules phase is implemented and desktop-validated. See
[verification](VERIFICATION.md#room-matches-12-october-2026) and
[measured build size](BUILD-SIZE.md#room-matches-12-october-2026). Phone and WAN
qualification remain open. Both sports now have programmed bots; learned AI
training has been retired and its source and artifacts archived locally.

`Tools/Build/Test-BasketballRestart.ps1 -Build` builds and checks the continuous
basket flow; `-Local` repeats it with a host and guest. The fixture uses physical
rim crossings, tests a full rotation on both teams, detects reset teleports,
checks individual control return and captures both automatic-movement notices.
Evidence belongs under `Builds/BasketballRestartQA/`.
