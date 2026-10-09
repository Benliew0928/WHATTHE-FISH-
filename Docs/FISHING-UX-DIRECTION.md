# Fishing interaction and cartoon HUD direction

Status: **A — Lagoon Stickers selected**, 8 October 2026. The user approved
[the multiplayer rules](FISHING-RULES-PROPOSAL.md) with Crew Catch set to
20 points per starting player. Implemented on 9 October 2026; see
[verification and remaining device limits](VERIFICATION.md).
The [approved reference](FishingUI/Lagoon-Stickers-Reference.png) is a generated
design preview. The live HUD combines six reusable illustrated sprites, UI
meshes and animated text. [Artwork and exact prompts](../ArtSource/Fishing/UI/GENERATED-PROMPTS.md)
record the production assets and mobile imports. The concept PNG is outside
the Unity project and is not shipped in the APK.

## Confirmed scoring

Crew Catch requires 20 x starting players, one landed fish of each size across
the crew, and at least one catch per starting player. Competitive matches
remain three-minute highest-score contests. A personal 20-point celebration
does not change that competitive finish rule or require each crew member to
individually reach 20.

## Implemented interaction

1. Near a pier, use the centre reticle to aim near a visible fish. The reticle
   turns teal when it acquires an available target; brackets, a tracking water
   ring and the Cast button identify that fish and its base points. A forgiving
   angular aim cone works across screen sizes, while selection hysteresis keeps
   swimming fish from causing rapid target changes. Camera dragging aims;
   joystick, look and UI touches must never cast by themselves. Tapping a tiny
   fish on the screen is no longer required.
2. Press Cast to commit that fish. Turn the character toward it, animate the
   rod swing and an arcing line/float toward the actual target. The host checks
   the chosen fish's availability, pier, player range and request revision.
   No invisible replacement target is allowed. Unavailable or camera-obscured
   fish are excluded from aiming. Cast commits the displayed fish, and aiming
   does not replace it while the line is active.
3. When it bites, the Hook button and float/ring pulse. The player must tap
   Hook within the existing 1.8-second window. Keep that cue readable without
   a large sentence hovering over the lagoon. Bites arrive 0.3 seconds earlier
   than the previous release; the Hook reaction window is unchanged.
4. Hold Reel to bring the fish in; release to ease tension. Show the fish
   fighting, a taut/slack line and responsive rod/hand motion. Match visual
   feedback to authoritative catch progress and tension. Do not add cinematic
   camera shake that prevents the player aiming or reading the controls. Slightly
   faster line shortening and slower strain growth shorten a typical catch by
   around a second, depending on size, range and release timing. Preserve large
   fish surges and line failure under uninterrupted reeling.
5. Landing triggers a short visible catch animation, score change and row
   feedback for every peer. Missing, cancelling and snapping have distinct
   contained button/widget feedback. They do not leave stale input held.

Each of the five piers now has six fish, two of each size, for 30 swimming fish
across the lagoon. They reuse the same three size models, materials and LODs;
no extra model download is required.

The first target is responsive arcade line behaviour using the existing rig
and fish. The present implementation uses controlled swimming and a tension model;
it is not a full fluid or flexible-rope solver. Follow-up delivery must state
which physical behaviour is actually implemented and tested.

## Top-left leaderboard

- Separate cartoon player rows with rank, real player name, avatar/identity
  marker and points. Clear local-player outline; enough room for five players.
- On an accepted score change, ease the digits, pop a reward badge beside the
  name, and bounce only that row. Display the actual catch/bonus award; +3 is
  the wanted bonus, not an invented fixed catch value.
- Sort by authoritative score. Slide rows to their new positions, preserve
  identity during movement, and show a brief up/down cue. Do not recreate or
  reshuffle rows on every frame. Equal scores retain their shared rank.
- Near a player's first 20-point milestone, use a restrained warm red/fire
  highlight. Crossing 20 gives a brief celebration and then settles. This is
  separate from the line-danger warning. In Crew Catch, show the shared total,
  locked goal and collection checklist rather than a winner-versus-loser rank.
- Timer, wanted target, Cup standing and crew goal belong in bounded UI cards.
  Keep central fish and casting space clear.

## Bottom gauges and buttons

- Separate Catch and Tension gauges, with labels inside the bars or directly
  beside them. Keep free-floating instructional paragraphs out of the playing
  view. Put brief state text inside the action button or a contained card.
- Catch fill eases smoothly with a subtle shine; its destination remains the
  replicated progress. Tension responds promptly so animation smoothing does
  not hide approaching line failure.
- Tension changes from safe teal/green to amber, then coral/red. Add a warning
  shape as well as colour. Use small flame/spark accents near high tension,
  restrained pulsation at critical tension and a clear release cue in the
  button. Keep all effects away from the numbers and catch target.
- Cast / Hook / Reel use one clear context-sensitive primary control, with a
  separate Cancel. Distinct press, release, disabled and success feedback.
  Preserve the pointer that owns reeling; other fingers cannot release it.
- End holding on pointer release, cancellation, focus loss, travel, disconnect
  and round changes. Interaction correctness takes priority over decoration.

## Selected concept

Three image-generator previews used the actual lagoon screenshot as their base.
The user selected **A: Lagoon Stickers**, with rounded cream stickers, navy/teal
outlines and cheerful coral/yellow accents. Preview images illustrate animation
in a still image; they are not runtime captures. Generation prompts remain
local review evidence in the ignored `Legacy/20261009-010106-563-fishing-lagoon-stickers/`
batch; the selected reference remains publishable under `Docs/FishingUI/`.

| Option | Direction |
| --- | --- |
| A: Lagoon Stickers | Rounded cream stickers, teal outlines, cheerful coral/yellow accents |
| B: Treasure Dock | Warm cartoon timber, rope details, cream plaques and nautical gauges |
| C: Splash Arcade | Bold comic shapes, navy/teal contrast, coral controls and energetic sparks |

A is recreated as responsive UI. Keep effect intensity modest, widgets compact at
phone scale, and text inside their controls.

Text stays native Unity UI rather than being baked into compressed artwork.
The existing display font already has its intended heavy weight, so additional
bold styling is removed. Dark labels use clean ink without shadow/outline copies;
white labels use one thin navy outline. Smaller labels and player names reuse the
lighter Cove text face. This improves letter counters and contrast without larger
font assets or higher-resolution sprites.

The live HUD is contained by the existing safe-area frame. Ranking uses the
top-left corner, return uses the top-right, and movement/action controls use
the bottom corners. Gauges use the bottom centre on wide screens and move
above the controls on compact screens; the clock moves below the return
button. Layout updates when the frame changes, rather than using a fixed
screen position. Margin, spacing, breakpoint, fallback scale and control sizes
are editable in `Game/Assets/_Game/Resources/FishingUILayout.asset` through
the Unity Inspector, alongside the reticle anchor/size and acquire/release angles.
Local dimensions inside a widget are design units,
scaled by the canvas; they are not a single device's screen pixels.

Android remains landscape only. The stopwatch sticker pulses on the second
beat, strengthens near the deadline, and sits beside the live time. The cream
Wanted panel contains an illustrated fish, its size, and a separate gold +3
badge. The badge confirms a claimed bonus with OK; an upcoming target receives
a brief preview and pulse. Crew requirements use a separate contained row.

## Verification and delivery

Check fish selection in all three cameras, desktop and touch input ownership,
wrong/occupied/out-of-range targets, cancellation and deadline boundaries.
Verify host/guest selection, catching, bonus awards, rankings, crew goals and
Cup results. Render real score changes, row reordering, high-tension release
and a full catch, including repeated rounds and island travel.

Reuse existing fish, rods and character assets. Avoid new imported art packs
or UI middleware. Matching multiplayer players now require protocol **38**.
The fresh integrated release measures **88,742,452 bytes**, versus
90,051,054 before merging the teammate updates (**−1,308,602**), with
**11,257,548 bytes** to the strict 100,000,000-byte boundary. The
75,000,000-byte development target remains exceeded by **13,742,452 bytes**.
[Measured build record](BUILD-SIZE.md) and [packing audit](APK-SIZE-AUDIT.md)
cover the current reticle, fish count, tuning and integrated gameplay.
The complete validated player is in `Builds/WindowsFinal/`; see
[verification](VERIFICATION.md).
A Windows render does not qualify phone performance or real internet latency.
