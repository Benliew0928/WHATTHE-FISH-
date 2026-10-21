# Golf random events — current rules

Matching multiplayer protocol **77** is required for every player.
Scoring, hole progress, ball ownership and ranking rules are unchanged.

## Schedule

The Host draws one of three events every 90 seconds, with a ten-second warning
and no immediate repeat. The authority seeds each match from independent OS
random bytes instead of truncated clock ticks. The first draw gives each of the
three events equal probability; the first event can still legitimately repeat
between matches. Explicit seeds remain available to reproducible development
fixtures. Clients display the Host snapshot rather than drawing their own event.
Events affect newly accepted shots; each shot retains
its captured conditions through flight and bounces, then clears them at rest or
recovery. Aim uses the exact same conditions and trajectory calculation.

When the first player finishes five holes, the automatic random cycle stops.
An event that has already started continues to its original expiry. A warned
but unstarted event is cancelled. Island Rush starts one second after that first
finish and lasts 30 seconds. Subsequent finishers do not retrigger it. The existing
30-second finishing deadline remains unchanged. A new round resets the cycle.

| Event | Duration | Current response |
| --- | --- | --- |
| Tropical Wind | 20 seconds | Random horizontal airborne acceleration of 2.6 m/s², four times the previous 0.65. Putt has no wind acceleration. Higher flights accumulate more drift. |
| Sticky Green | 20 seconds | Swing stops at the first supported ground touchdown, with zero linear and angular velocity. Putt is identical to ordinary Putt. |
| Bounce Party | 15 seconds | Swing landing retains 80% incoming normal speed and 75% tangential speed, above the existing minimum bounce threshold. Putt is unchanged. |
| Island Rush | 30 seconds | Player movement multiplier 2.10 (+110%); cart target forward/reverse speed multiplier 1.90 (+90%). |

Moon Golf is removed from the enum, random pool, gravity modifier and F2 shortcut.
The remaining event wire IDs stay 1/3/4; retired ID 2 cannot trigger or display.
Random Swap is removed from the enum, schedule, networking payload and runtime
position-changing logic. F5 no longer triggers an event.

## Wind presentation

The shot preview, actual flight and environment use the replicated
world wind direction. Leaves and grass bend downwind with travelling gusts.
Generated grass includes per-blade root-to-tip weights: roots remain anchored,
while tips bend most. Non-Golf foliage keeps its original ambient motion.
Trunks and collision geometry remain fixed. The separate wind-direction arrow is removed.

Only the cloth of the five flags turns downwind; poles stay fixed. Existing
30 cm maximum single-sided flutter is retained, with a faster event flutter.
Wind fades in/out over half a second, and uses a continuous cosmetic clock.

Nearby sand surfaces emit a bounded pool of fine downwind grains. Grains have
horizontal launch velocity, upward lift, gravity, fading lifetimes and expire
before their estimated ground return. Emission is limited to nearby dry sand;
no water, rock or grass source is used. Existing particle materials are reused.
The world effects are cosmetic; they do not move scenery colliders or alter
other sports. Physical phone FPS has not been measured.

## HUD and testing

The lower centre control/status plaque is hidden, including the starting hint,
drag hint and cart driving hint. The central large opening numbers/GO animation
and top-left finishing countdown remain available. Original event badge art stays.

See [development hotkeys](GOLF-EVENT-HOTKEYS.md). Current focused evidence:
55 offline event assertions, 30 Host and 32 Client event assertions, including actual sticky
contacts, preview/landing agreement, live downwind particles, event timers and
final-countdown stacking. Evidence is under ignored `Builds/GolfSignEventsQA/`.
Detailed package measurements and recoverable archives are in
[verification records](VERIFICATION.md).

## Rush trail and ordinary landing

Ordinary Swing first-contact tangential retention is 0.72 (28% loss). Later
ordinary contacts keep the existing 0.92 retention. Bounce Party uses its own
0.80 normal / 0.75 tangential response; Putt remains excluded.

During Rush, grounded running golfers emit pooled world-space cyan/white and
gold ribbons, leaf chips and sparkles behind their body. Stationary players,
cart drivers, spectators and players outside Golf emit nothing. Samples fade
within 0.7 seconds. Sharp turns discard forward history; a shader plane clips
any fragment in front of the runner. Teleports clear the history.

The supplied warning artwork is retained unmodified as `WarningSheet.png`.
The Host selects at 80 seconds; live 10..01 seconds occupy the top-left badge
position before the actual event appears at 90 seconds. New-round resets and
first-finisher cancellation rules apply to this warning too.

Current random pool: Tropical Wind, Sticky Green and Bounce Party. Island Rush
retains its first-finisher trigger. The 90-second cycle and 10-second warning
remain unchanged. Matching protocol 77 players are required.
