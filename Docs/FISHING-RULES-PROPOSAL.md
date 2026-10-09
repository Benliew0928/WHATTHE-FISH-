# Lagoon Cup: fishing multiplayer rules proposal

Status: approved implementation specification, 8 October 2026, with the crew
target reduced to 20 points per starting player and **A — Lagoon Stickers**
selected. Implemented on 9 October 2026; build evidence and limits are in
[verification](VERIFICATION.md). Current controls
and modes are documented in [Lagoon Cup and Crew Catch](FISHING-MINIGAME.md). See the
[interaction and animation requirements](FISHING-UX-DIRECTION.md).

## The game players should understand

**A three-minute fishing sport for two to five friends. Choose your catch,
react to the bite, and reel without snapping the line. Compete for the Lagoon
Cup, or work together to fill the crew's basket before time runs out.**

Keep the existing compact lagoon, five coloured piers, three fish sizes and
shared group travel. Everybody in a room plays one active activity together.
Solo players use free practice. Fishing participation remains capped at five;
the host must see that limit before starting or travelling with a larger party.
Extra room members must never be silently excluded from the contest.

The original proposal's target hunt becomes a small shared catch challenge.
Its different fishing areas become the existing piers. Its collection mode
becomes a crew basket. This retains decisions and shared objectives while
keeping the first version focused on one catching mechanic.

## Common catching rules

1. Before the round, everybody confirms readiness. The host starts a visible
   three-second countdown; everyone begins with zero points and no active cast.
   Practice catches never carry into a scored round.
2. At any pier, tap or click an available visible fish to select it, then
   press **Cast**. Highlight the selected fish with a tracking ring; keep its
   size and points inside the Cast button or a compact UI card. Every pier
   offers the same three categories, so a particular starting position does
   not grant exclusive access to valuable fish. Show unavailable fish as
   occupied or recovering; do not silently substitute another target.
3. React to **BITE!** within 1.8 seconds. Hold Reel to gain catch progress;
   release to ease tension. Land the fish before the line reaches 100% tension.
4. A landed small fish earns **2 points**, medium **5**, large **10**. Larger
   fish should demand more time and better tension control. Selection changes
   require balancing; the present tuning is not proof that all three choices
   are competitive.
5. A missed bite, snapped line, cancelled cast or reel timeout earns zero. It
   does not subtract existing points. Leaving the pier cancels that catch.
6. Only one player can claim a particular fish at a time. Another player cannot
   steal an already hooked fish, cut their line or cancel their input. Caught
   fish return after seven seconds. Piers are shared; owning a pier is not a
   rule.
7. The three-minute clock continues while players move or fish. A catch counts
   only when landed and accepted by the host before the deadline. Hooking it
   before the buzzer is insufficient. At zero, unfinished catches end and
   scores lock. There is no overtime.

These rules keep pressure on timing and risk: secure a smaller catch now, or
spend time attempting a larger catch. The player should see fish availability,
points, catch progress, line tension and remaining time without opening menus.

## The shared catch challenge

Each minute has one **Wanted Catch** category, visible to everyone. During that
minute, your first landed fish in that category earns **+3 bonus points**. Your
next catches receive their normal points. An indicator shows whether you have
already collected that minute's bonus.

| Target fish | First qualifying catch | Further catches in that minute |
| --- | ---: | ---: |
| Small | 2 + 3 = 5 | 2 |
| Medium | 5 + 3 = 8 | 5 |
| Large | 10 + 3 = 13 | 10 |

Each round features every category exactly once. Start with Small -> Medium ->
Large; rotate the order for the next round. All players receive the same order
and the same change times. Show the full schedule before starting and preview
the next category near a change. A catch qualifies by its landing time, not
its cast time. A catch on a minute boundary belongs to the new minute; a catch
at the final deadline does not score.

The maximum bonus is **9 points per player per round**. It creates decisions
without making a single rare spawn decide the match. For example, if Kai has
64 and Mia has 62, Mia's first wanted small catch earns 5 and puts her on 67.
Kai can respond by completing his own challenge or landing a larger fish.

## Mode 1: Lagoon Cup, competitive

**Two to five players; three minutes; highest score wins.** Everybody catches
their own fish. The catch challenge and rules are identical for all players.

Show a compact live leaderboard with your position and points needed to reach
the next position. Announce a lead change and a large catch without covering
the tension meter. Other players' catches should be visible so the competition
feels shared rather than hidden behind a final score screen.

At the buzzer, sort total points descending. Equal points share a rank and,
if first, share the win. Catch count can appear as a statistic, but does not
break ties. Do not use who caught first or host arrival order as a tie-breaker.

A single round is the default. Friends can choose a **three-round Cup** for a
longer rivalry. With a fixed roster of N players, each round awards Cup points
equal to `N + 1 - rank`: four players earn 4 / 3 / 2 / 1. Shared ranks receive
equal Cup points and skip the next place: ranks 1 / 1 / 3 / 4 earn 4 / 4 / 2 / 1.
After exactly three rounds, highest Cup total wins. If tied, compare total fish
score across those rounds; an exact tie shares the Cup. Nothing earned before
the series counts. Each round begins from zero fish score.

This rewards consistency. One extraordinary catch or strong opening round
does not end the competition. Keep the roster fixed for a Cup; if it changes,
finish the current round, retain its result, and end the incomplete series
without awarding a Cup champion. A fresh roster can start a new series.

## Mode 2: Crew Catch, cooperative

**Two to five players; one shared basket; three minutes.** Individual catches
and wanted bonuses add to the same crew total. Everybody retains control of
their own rod; teammates coordinate fish choices and available piers.

The crew must satisfy all three conditions before time runs out:

- Reach the shared point target: **20 x starting players**, or
  40 / 60 / 80 / 100 for two / three / four / five players.
- Land at least one small, one medium and one large fish across the crew.
- Every starting player must land at least one fish.

The 20-point target is the user's selected starting value; its difficulty is
not playtested yet. Show and lock it before the round. Never quietly lower it
mid-round. Competitive play still lasts three minutes; reaching 20 does not
end a competitive match.

End successfully as soon as all three conditions are met. Everyone gets the
same crew result: **Bronze** for completing it, **Silver** with at least 20
seconds remaining, **Gold** with at least 40 seconds remaining. At zero, an
incomplete goal is a loss; show the exact missing points, category or player's
first catch so the group knows what to improve.

Useful coordination sounds like: "We still need a large fish; I'll take that.
You get the wanted medium bonus," or "We have the points; help our beginner
get their first catch." Help initially means advice and dividing objectives;
two players physically reeling the same fish is a separate mechanic, outside
this first ruleset.

There is **no winner-versus-loser ranking inside a successful crew**. Show
each person's contributions positively, then the shared medal and crew best
completion time for that player count and rules version. This gives beginners
a reason to join without encouraging teammates to compete against their own
shared objective. The every-player condition also means an AFK player cannot
receive a success by doing nothing.

## Room fairness and interruptions

- Only the host starts or restarts. The ready roster locks when countdown starts;
  a roster change during countdown cancels the start. A late arrival during play
  waits for the next round; joining cannot grant a shorter
  scoring window or change the target. Travel and returning to the lobby
  abandon an unfinished round without inventing a winner.
- The host owns fish claims, input validation, bonuses, clock and results.
  Clients request actions rather than submitting points. Friends need matching
  game builds. This trusted-host model is suitable for the present party game;
  it is not a secure public ranked service.
- A departing player releases their fish. Already earned score remains visible
  with **Left**, and cannot grow. A single competitive round can finish with
  the remaining players; the incomplete Cup follows the roster-change rule.
- A crew member leaving makes the cooperative attempt incomplete: keep its
  progress visible, end it without a medal, then let the smaller roster restart
  with a newly displayed target. Do not award a success under changed rules.
- If the host disconnects, the attempt is abandoned. Host migration and
  competitive reconnect recovery are outside this first version.

## Reasons to play again

The intended loop is **practice -> short round -> close result -> rematch**.
Support instant rematches in the same room, voluntary three-round Cups and
crew medal attempts. Show a player's own improvement in catch accuracy and
score, and the crew's faster completion, using local records initially.

Competitive rankings belong to the current room or Cup. Public/global ratings,
matchmaking and permanent ranked leagues are outside this first version.

Keep all rods mechanically equal. There is no stronger paid rod, stat grind,
loot economy or requirement to unlock fair competitive equipment. The first
retention experiment is whether friends voluntarily ask for another round,
not whether a currency bar can make them stay.

This sits alongside the other sports' skill challenges: football's teamwork
and scoring, basketball's timing and possession, and golf's precision and
ranking. Their present implementation differs: [Football](FOOTBALL-MATCH.md)
has formal team matches; [Basketball](BASKETBALL-GAMEPLAY.md) remains shared
practice; [Golf](GOLF-MINIGAME.md) already has independent progress and ranking.
A championship spanning all four islands is a later agreed feature, not part
of this fishing proposal. Group travel and one active activity are retained.

## Delivery scope and assets

| Rule or feature | Current status |
| --- | --- |
| Five piers, three fish sizes, cast/hook/reel, tension and 2/5/10 points | Implemented baseline |
| Three-minute host contest, shared wins, host-owned catches | Implemented baseline |
| Ready countdown and player-selected fish category | Proposed; current casting chooses automatically |
| Wanted Catch and minute bonus | Proposed |
| Three-round Cup and Cup points | Proposed; friends can track a series manually first |
| Crew Catch target, category checklist and shared medals | Proposed |
| Persistent personal/crew records and extra result statistics | Proposed; local first |

Build and balance category selection plus Wanted Catch first. Then test the
shared Crew Catch goal. Automate Cup standings and local records only after
friends enjoy the individual rounds. Do not add species, biome quests, ability
loadouts or a legendary-fish event to this version.

**Required new 3D models: none.** Reuse the existing fish, two rod appearances,
character, five piers and lagoon. Wanted labels, basket checkboxes and medals
use live text and UI meshes. The selected HUD adds six shared, compressed 2D
sprites. Additional scenery, fish models, animation packs and effects libraries
are unnecessary for these rules.

The APK before this implementation was **89,938,934 bytes**, with **10,061,066 bytes** to the strict
100,000,000-byte boundary. It was **14,938,934 bytes above** the 75,000,000-byte
development target. Avoidable shipped size still needs reduction. See
[measured size and build limits](BUILD-SIZE.md) for the new release's measured
delta; needing no new models does not mean zero build growth.

## Playtest decisions before calling the rules ready

Test two-, three-, four- and five-player rooms, including a first-time player
and a player comfortable with the reel mechanic. Observe whether:

- A new player can land a fish within the first minute after the tutorial.
- There is a reason to choose each size. If large-only play dominates regardless
  of the target and deadline, adjust reel difficulty/recovery or points before
  shipping selection. Never assume the current 2/5/10 tuning already does this.
- A minute bonus is readable and worth considering without deciding every
  winner. Players understand why a catch did or did not qualify.
- Crew members discuss who catches what, and new groups can plausibly complete
  the goal. Tune the quota and medal thresholds from real completion times.
- Friends voluntarily request a rematch. If they only fish in silence at
  separate piers, the social interaction still needs work; extra progression
  screens are not evidence that the catching became fun.

Any implemented follow-up must verify real host/guest rules, phone controls and
performance, build a fresh AndroidSubmission APK under the hard limit, inspect
its size audit, and update the complete Windows player under the repository's
delivery instructions. No retention or phone-performance claim follows from
this design document alone.
