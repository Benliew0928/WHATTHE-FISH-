# Fishing round skills

Before every timed Round, Cup round or Crew game, everyone sees the **same
three distinct skill cards**. Pick **one** for the whole round. Each menu
includes both a before-casting skill and a fighting skill. Players may choose
the same card. There are no bag pickups, treasure or extra skills mid-round.

Selection lasts up to **12 seconds**. Once everyone picks, the normal
three-second countdown starts. An absent choice defaults to the first card.
Selection does not consume the three-minute match clock. Practice offers an
untimed choice. Rematches deal a fresh menu and clear cooldowns.

| Skill | Effect | Phase | Cooldown from activation |
| --- | --- | --- | --- |
| Steel Wire | Tension cannot rise above its activation value for 5s. Existing tension stays; release to cool. Silver-blue braid on the line. | Hooked | 24s |
| Power Reel | Wind 30% faster for 6s, with ordinary tension. Gold pulses on the line. | Hooked | 22s |
| Fishing Buddy | A monochrome helper reels beside you for 6s. Wind 20% faster with 20% less positive strain. | Hooked | 30s |
| Bubble Guard | Absorb one strong surge within 9s, reducing strain by 0.22. The bubble pops when consumed. | Hooked | 18s |
| Sonar | Highlight available Wanted fish for 12s. Personal in competition; shared with all connected Crew members. | Before casting | 20s |
| School Call | Attract up to three unclaimed fish toward your pier for 10s; release them smoothly over 2s. Gold bait ripples mark the location. | Before casting, at a pier | 20s |

One skill button sits beside Cast / Right. It hides in the wrong phase,
glows at full opacity when ready, dims with a clock border while recharging,
and shows a draining ring while active. It works independently of a held
Left or Right control. Your skill remains yours after activation.

Skills never give automatic catches, extra points or ownership of a fish.
School Call does not reserve or steal fish. Fishing Buddy copies the live
avatar pose, meshes and rod using a shared black-and-white shader at 82%
opacity. It has no collider, network player, independent AI or score, and
requires no extra character model or texture set.

The host validates choices, ownership, phase, timestamps, round/cast tokens,
activation and cooldown. Use matching builds (current protocol **77**). Retired pickup and
favourites actions are rejected. Disconnecting during selection abandons it;
results and island travel clear effects.

## Tutorial and camera

The entry tutorial and How to Play use the exact same
[component and copy](../Game/Assets/_Game/Shared/UI/FishingTutorialUI.cs):
**Pick 1 of 3 / Cast & Catch / Use Your Skill / Play to Win**. Examples reuse
the actual skill graphics, clock border, buttons and fishing meters. The
leaderboard **?** reopens this guide while idle.

Fishing arrival selects Follow. **Island View** remains a manual settings
choice: a wider follow view behind the player, with gentle pitch and collision
handling. It stays player anchored while hooked. Eye-level fighting and the
accepted Follow fight framing remain. See [controls](FISHING-COUNTERSTEER.md)
and [match rules](FISHING-MINIGAME.md).

The longer-duration update keeps the same cooldowns: Steel Wire 5s, Power Reel
6s, Fishing Buddy 6s, Bubble Guard up to 9s, Sonar 12s and School Call 10s plus
a 2s release. Bubble Guard still ends early when it absorbs one surge. Ability
cards, active rings, shared Sonar and School Call motion use these same times.
