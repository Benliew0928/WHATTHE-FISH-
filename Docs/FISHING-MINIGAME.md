# Lagoon Cup and Crew Catch

Launch the complete Windows player with `Builds/WindowsFinal/Explore-Fishing.cmd`,
or choose **Let's play → Fishing → Explore offline**. To play with friends,
the host opens a Fishing room and guests join the same room using matching
protocol **38** players. The room shares one activity and travels together.
Fishing supports **two to five friends**, with solo free practice and a solo
timed practice option. Walk to the end of any of the five coloured piers.
Each pier has **six fish: two small, two medium and two large**, for **30 fish**
across the lagoon. The schools reuse the existing three fish models and materials.

## Catching

1. **Aim:** move the camera so the centre reticle is near a visible fish in
   your pier's school. The reticle turns teal when a target is available;
   tracking brackets and a cyan water ring identify it, and the Cast button
   shows its size and base points. A forgiving aim cone and stable selection
   reduce flicker as fish swim. You do not need to tap the fish itself.
2. **Cast:** tap Cast or press **F**. The character turns toward the target,
   swings the rod and throws the line/float in an arc. A fish already claimed
   by someone else cannot be taken; aim at another fish or wait. Casting commits
   the highlighted fish; camera movement does not switch it during the catch.
3. **Hook:** watch the float and pulsing **HOOK!** button, then tap it or
   press **F** within **1.8 seconds** of the bite.
4. **Reel:** hold the button or **F** to draw the fish closer. Release when
   tension turns warm/red, then resume when it eases. The catch gauge tracks
   the fish approaching the pier; the tension gauge follows line stretch and
   sustained reeling strain. **100% tension snaps the line.** The current tuning
   draws fish in slightly faster and builds strain a little more slowly. A catch
   is typically around a second shorter, varying with size, distance, surges and
   release timing; large fish still require tension management. Bites arrive
   **0.3 seconds earlier**, with the same 1.8-second Hook window.
5. **Land:** small/medium/large fish earn **2 / 5 / 10** base points.
   Landing briefly lifts the fish, updates points and animates the ranking.
   Caught fish return after **seven seconds**.

Cancel with the Cancel button or **X**. Moving away, jumping out of casting
position, travelling or leaving the game releases the line. Misses, cancellation,
snaps and the **30-second reel timeout** award no points. Pointer release/exit,
focus loss, pause and round changes end held input. Only the finger that started
reeling can release it; the host's **0.8-second renewable lease** also recovers
from a missing release message. Use Camera to switch among all three views.

## Playing together

In free practice or after a result, each player presses **Ready**. The host
chooses a mode and presses **Start** once everyone is ready. A shared
**three-second countdown** clears practice catches and begins the round.

| Mode | Goal and finish |
| --- | --- |
| Round | Highest points after **three minutes**. Equal totals share the rank/win. |
| Cup x3 | Three Round contests with the same roster. Ready again between rounds. Each round awards `N + 1 − rank` Cup points, including shared ranks; compare total Cup points, then total catch scores. Exact ties share the championship. |
| Crew | One shared basket: **20 × starting players** points, all three fish sizes across the crew, and at least one catch per player. Finish early when all conditions are met; otherwise fail at three minutes. |

Every minute has a different **Wanted** size. A player's first matching catch
landed in that minute earns **+3**, at most once per minute and **+9 per round**.
The card warns about the next size near the change. All three sizes appear once;
the starting size rotates on rematches. Wanted awards count toward the crew basket.

Crew goals are **40 / 60 / 80 / 100** for two / three / four / five players.
Everyone contributes a catch, but each person does not need to score 20
individually. All crew members earn the same completion medal: **Gold** with at
least 40 seconds left, **Silver** with at least 20, otherwise **Bronze**.
A competitive player reaching 20 continues playing until time expires.

Late arrivals wait for the next scored round. A departed competitive player's
earned score remains visible with a faded row; the contest continues. A roster
change invalidates an unfinished Cup series, so start a new Cup with the new
roster. A disconnect during the countdown or Crew round abandons that round.
A lost host ends the room's activity. Local personal bests are saved separately
by mode and player count; there is no global online ranking service.

## Lagoon Stickers HUD

[A is the approved design reference](FishingUI/Lagoon-Stickers-Reference.png).
The top-left cream/navy card contains player names, cartoon avatars, shared
ranks and animated points. Catch awards pop beside the name; the base award
appears first and a Wanted **+3** follows when earned. Rows slide when another
player overtakes, with brief direction cues. Scores near 20 glow warm with a
flame; crossing the milestone celebrates briefly without ending the contest.
Crew rows show contribution checks rather than competitive ranks.

The clock/Wanted card holds the time and crew checklist. Catch and Tension
labels sit inside the bottom gauges. The fill, shine and flame accents animate;
the danger triangle and **RELEASE!** cue reinforce the red warning. Cast, Hook,
Reel, Cancel, Camera, Jump and exit controls keep their text contained.
Players can set a short room nickname before a round.

Six small illustrated sprites supply the layered frame, reel button, fish/profile/
stopwatch badges and flame. [Full-resolution source art and exact prompts](../ArtSource/Fishing/UI/GENERATED-PROMPTS.md)
are retained with deliberate 64/128/256-pixel mobile imports and ASTC 6×6 compression.
The six textures total about 84 KB before APK ZIP compression. Rounded lettering
reuses the existing Cove fonts. Smaller labels and player names use the lighter
text face; headings, timers and large scores retain the display face. Native text
uses its intended font weight, with clean dark labels and only one thin outline
on white labels, instead of stacked shadow/outline copies. Names, scores, gauge fills and interactions remain
live Unity UI. A shared illustrated fisher badge, real names and local-row colour
identify players; it is not a live character-customisation portrait.
No additional 3D model, art pack, animation download or runtime package is added.

## Simulation and presentation

The host owns targets/reservations, bites, hook deadlines, a spring/drag line
model, tension, catch progress, scoring and results. Guests send owner-authorized
actions with round/cast revisions and an explicit fish ID. The host checks
the pier, range, terrain obstruction, fish availability and current phase.

Fish schools follow bounded underwater paths using the shared room clock.
During reeling the fish's distance responds to line shortening, spring force,
mass, drag and periodic pulls. Rod/hand motion, line sag, float colour and
visible fish thrashing follow that state; landing lifts the fish briefly.
The existing two rods, three fish sizes, character rig, grip socket, meshes,
materials and LODs are reused. First person retains arms and hides the local head.

This is arcade line simulation with visual slack/taut feedback. It does not
solve flexible-rope collisions, full fluid dynamics or anatomical fish muscles.
Water retains the transparent bed, ripples, sun highlights and shoreline blend.
A **128-pixel cubemap** captures the static environment once per island load;
sky reflection is the fallback. It is not a live planar mirror of players.
The float follows the ripple equation. Existing lighting and cameras remain.

## Checks and rebuilding

`Tools/Build/Test-FishingGameplay.ps1 -Render` checks real screen-space
reticle targeting, touch/button and keyboard action paths, catch scoring, hold/release,
camera targeting, swimming containment, reflection/light availability, ranking
animations and Fishing/Golf hand-grip transitions. It also runs host/guest
target validation, Wanted scoring, an overtake, shared results and ready rematch.
Its deterministic state tests cover all fish sizes/ranges, deadline boundaries,
Cup ties/series, crew requirements/medals and roster changes.

Five-row/phone HUD captures use explicitly identified development fixtures;
they are layout/animation checks, not proof of a five-device network session.
Development fixtures and probes are excluded from the release APK. Evidence
stays under ignored `Builds/FishingGameplayQA/`, `Builds/FishingUXQA/` and
`Builds/FishingAimQA/`.
The script defaults to the complete WindowsFinal player; `-PlayerPath` accepts
another complete candidate.

A fresh AndroidSubmission must pass the strict APK gate. See
[measured build size](BUILD-SIZE.md), [packing audit](APK-SIZE-AUDIT.md) and
[verification](VERIFICATION.md). The **75,000,000-byte development target**
remains separate from the strict **100,000,000-byte submission limit**.
Windows captures cannot establish Android appearance, FPS or thermal behaviour.
Physical Android and internet-latency testing remain necessary.
