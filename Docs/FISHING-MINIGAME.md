# Lagoon Cup and Crew Catch

Launch the complete Windows player with `Builds/WindowsFinal/Explore-Fishing.cmd`,
or choose **Let's play → Fishing → Explore offline**. To play with friends,
the host opens a Fishing room and guests join the same room using matching
protocol **73** players. The room shares one activity and travels together.
Fishing supports **two to five friends**, with solo free practice and a solo
timed practice option. Arrival assigns each active player a distinct coloured
pier; spectators land on shore. Match start waits for the whole travelling group.
The lagoon has **30 shared fish**, ten of each size. They swim across pier sectors
in a public pool; every player can aim at any reachable, unclaimed fish. The
school reuses the existing three fish models and materials. Independent seeded
wandering paths let fish turn, slow and cross the pool without orbiting its centre.

After Ready and Start, everyone sees the same three random skill cards and picks
one for the round. The host waits up to 12 seconds, then starts the ordinary
countdown. Reuse your chosen skill after its cooldown; its contextual button
sits beside Cast / Right. There are no bag or treasure controls.
The first visit opens a four-page tutorial using the same icons and meters as gameplay. The leaderboard **?** and Fishing **How to Play** reopen that shared tutorial.
See [abilities and acquisition rules](FISHING-ABILITIES.md).

## Catching

1. **Aim:** move the camera so the centre reticle is near a visible fish in
   the shared pool. The reticle turns teal when a target is available;
   tracking brackets and a cyan water ring identify it, and the Cast button
   shows its size and base points. A forgiving aim cone and stable selection
   reduce flicker as fish swim. You do not need to tap the fish itself.
2. **Cast:** tap Cast. The character turns toward the target,
   swings the rod and throws the line/float in an arc. A fish already claimed
   by someone else cannot be taken; aim at another fish or wait. Casting commits
   the highlighted fish; camera movement does not switch it during the catch.
3. **Hook:** a [gold ! cue](FISHING-HOOK-CUE.md) appears above the biting fish
   and **HOOK!** pulses, with a gold rim showing the remaining time. Tap it within **1.8 seconds** of the bite; input is available immediately.
4. **Hold the Glow:** hold the glowing Left or Right button against the fish.
   Switch with the glow; release both when tension turns red. There is no tap
   window or one-attempt sweep. The fish marker, gold band and lit button show
   the opposing direction. Correct holding winds the physical line continuously.
   At **100% tension the line snaps**. See
   [hold controls, camera and multiplayer rules](FISHING-COUNTERSTEER.md).
5. **Land:** small/medium/large fish earn **2 / 5 / 10** base points.
   Landing briefly lifts the fish, updates points and animates the ranking.
   Caught fish return after **seven seconds**.

Cancel with the Cancel button. Moving away, jumping out of casting
position, travelling or leaving the game releases the line. Misses, cancellation,
snaps and the **30-second reel timeout** award no points. Pointer release/exit does not erase a queued tap. Focus loss, pause and round
changes clear local input; the host expires each accepted short stroke without
requiring a release message. Open the sports-style gear button for three camera views for this island visit, four sound sliders, the shared tutorial and Leave island confirmation. Jump uses the same illustrated control as Basketball and hides while fishing. The old Camera and Leave HUD buttons are removed. The rope and central meter share teal/gold/orange/red tension feedback. A small line icon beside the rope hangs loose at low strain, pulls straight as tension rises and shows a warning near snapping; it has no progress fill. Eye-level view stays first person while holding either direction and allows manual looking. Long casts use a normalized spring/drag retrieval range so public-pool distance does not multiply fight duration.

The reaction window remains **1.8 seconds**. A remote Hook carries its press
time on the synchronized room clock. The host can receive that already-timely
press up to **250 ms** later; it rejects presses before the bite, at/after the
input deadline, older than 250 ms, more than 50 ms in the future, or for an old
cast/round or disconnected player. Small accepted clock skew is clamped to host
time. Host/offline misses stay immediate, and the match deadline always ends
play. This bounded receipt tolerance does not guarantee every internet delay.

**HOOKING…** acknowledges a queued press for at most **0.5 seconds**, blocking
duplicates during that wait. A retry is allowed afterward only inside the
original Hook window. The animated rim continues to show the real deadline.
Target explanations stay inside the action button: **FIND A PIER**, **FRIEND'S
FISH**, **FISH RETURNING**, **MOVE CLOSER** and **CAST PATH BLOCKED**. Missed
bites, snaps, cancellations and reel timeouts have distinct labels. These
diagnostics do not reserve fish or change the selected target.

A short sports-style awards scene precedes the completed-round card. It reuses the existing render-only characters and celebration takes; players, scores and network actors stay unchanged. Tied winners share the honours, Crew success celebrates everyone, and abandoned rounds go straight to results. Tap Show results to skip. Spectators watch the same ceremony.

## Playing together

In free practice or after a result, each player presses **Ready**. The host
chooses a mode and presses **Start** once everyone is ready. A shared
three-card selection clears practice catches; the **three-second countdown** then begins the round.

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
roster. A disconnect during skill selection, the countdown or a Crew round abandons that round.
Host loss triggers [automatic room recovery](HOST-MIGRATION.md), with the lowest
occupied player number taking over, then the lowest spectator number if no
players remain. The roster-change rules above still apply. Local personal bests are saved separately
by mode and player count; there is no global online ranking service.

## Sports-style Fishing HUD

[Hold the Glow](FISHING-COUNTERSTEER.md) uses the continuous Left/Right hold
mechanic; the timed-tap experiment is superseded.
The top-left cream/navy card contains player names, cartoon avatars, shared
ranks and animated points. Catch awards pop beside the name; the base award
appears first and a Wanted **+3** follows when earned. Rows slide when another
player overtakes, with brief direction cues. Scores near 20 glow warm with a
flame; crossing the milestone celebrates briefly without ending the contest.
Crew rows show contribution checks rather than competitive ranks.

The clock/Wanted card holds the time and crew checklist. Catch and Tension
labels sit inside two side-by-side bottom gauges, with a thin navy outline for contrast.
The fill, shine and flame accents animate;
the danger triangle reinforces high tension. Release both sides to recover. Cast,
Hook, Left/Right, Cancel and Jump keep their labels contained. Camera and Leave
island are available through settings. The contextual skill button uses a clock border.
Players can set a short room nickname before a round.

The current [hold-control HUD](FISHING-COUNTERSTEER.md) reuses the sports
fonts and press animation. The circular Cast/Hook reel illustration returns;
Left and Right share aqua/gold circular artwork with their own native labels.
A small sliced illustrated frame surrounds the native fish-pull track and
progress bars. The four existing compressed delivery PNGs total 201,479
bytes before APK packing. Editable masters stay outside Unity. Round controls
use fixed safe-area touch frames. Sharper fish darts/breaches and stronger rod
and torso motion reuse existing models and materials; feet remain planted.

## Results, Crew checklist and sound

A centered cream/navy results card opens once at the end of a scored round.
It freezes final standings, shared ranks, round points and applicable Cup totals.
Your summary shows catches by size, Wanted bonus points, missed bites, snapped
lines, cancelled casts, reel timeouts and the existing local best. The host
replicates these per-round counters and clears them for the next round.
Abandoned rounds and invalid Cups are labelled explicitly.

**READY & CLOSE** uses the existing Ready action; **CLOSE** or Escape restores
walking. **RESULTS** reopens the same completed round. While open, the card
blocks movement, camera input and underlying buttons without pausing the room.
Editable layout settings are in
`Game/Assets/_Game/Resources/FishingResultsLayout.asset`.

Crew's live timer card shows the basket, three fish-size badges/checkmarks and
the next player who needs a catch, with a count of any other missing contributors.
The results layout's `padding` also reserves space inside the clock's pale
border so the contribution line stays on the coloured background. The existing
target and completion rules remain unchanged.

Accepted local actions play short cast, splash, bite, Hook, landing, miss and
snap sounds. Correct directional holds add a quiet ratchet loop and rising strain gives a
warning. Cues suppress duplicate/stale snapshots and stop on focus loss, pause,
hold lease expiry or island exit. They obey the shared sound sliders; current
Golf/Fishing audio includes the approved shared bank.

**Settings → Fishing vibration** is off by default. Android uses short native
touch feedback and respects the device touch-feedback setting. Reduced motion
disables fishing vibration. Device audibility and tactile quality still need
physical-phone testing. Arrival and round awards use the shared travel and
ceremony presentation.

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
reticle targeting, touch/button and keyboard action paths, catch scoring, timed taps/counterpulls,
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

`Tools/Build/Test-FishingPolish.ps1` adds rendered results/Crew, target
explanation, modal-input, Hook pending and sound-lifecycle checks. State checks
cover receipt ages and deadline boundaries, clock skew, nonfinite times,
reservation expiry, stale casts/rounds, cancellation and disconnects. The
gameplay suite also verifies production sound callbacks and replicated recap
counters. Additional evidence stays under ignored `Builds/FishingPolishQA/`.

A fresh AndroidSubmission must pass the strict APK gate. See
[measured build size](BUILD-SIZE.md), [packing audit](APK-SIZE-AUDIT.md) and
[verification](VERIFICATION.md). The **75,000,000-byte development target**
remains separate from the strict **100,000,000-byte submission limit**.
Windows captures cannot establish Android appearance, FPS or thermal behaviour.
Physical Android and internet-latency testing remain necessary.
