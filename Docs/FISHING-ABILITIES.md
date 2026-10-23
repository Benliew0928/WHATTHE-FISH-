# Fishing round skills

After the host starts a timed Round, everyone sees the **same
three distinct skill cards**. Pick **one** for the whole round. Each menu
includes both a before-casting skill and a fighting skill. Players may choose
the same card. There are no bag pickups, treasure or extra skills mid-round.

Selection lasts up to **12 seconds**. Once everyone picks, the normal
three-second countdown starts. An absent choice defaults to the first card.
Selection does not consume the three-minute match clock. Free practice does
not automatically open this selection. Rematches deal a fresh menu and clear
cooldowns. There is no per-player Ready step or in-island mode selection.

| Skill | Effect | Phase | Cooldown from activation |
| --- | --- | --- | --- |
| Steel Wire | Tension cannot rise above its activation value for 7s. Existing tension stays; release to cool. Silver-blue braid on the line. | Hooked | 20s |
| Power Reel | Wind 55% faster for 8s, with ordinary tension. Gold pulses on the line. | Hooked | 18s |
| Fishing Buddy | A monochrome helper reels beside you for 8s. Wind 45% faster with 40% less positive strain. | Hooked | 22s |
| Bubble Guard | Absorb one strong surge within 12s, reducing strain by 0.35. The bubble pops when consumed. | Hooked | 16s |
| Sonar | Highlight available Wanted fish for 16s. Personal in competition; shared with all connected Crew members. | Before casting | 18s |
| School Call | Attract up to five unclaimed fish to about 6.5 metres inside your pier for 15s; release them smoothly over 2s. Gold bait ripples mark the location. | Before casting, at a pier | 18s |

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
activation and cooldown. Use matching builds (current protocol **79**). Retired pickup and
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

The duration, description, active ring and cooldown share the same rules. School
Call brings fish in over 1.3 seconds, holds the school for 15 seconds and releases
it over 2 seconds. Fish remain public and claimable by anyone. Buddy and Power
Reel still require correct Left/Right input; skills never award a catch directly.
The misleading pre-hook arrow over Jump is removed. Hook pulses and the fish's
exclamation cue remain. Round awards reuse the court team and winner animations.
