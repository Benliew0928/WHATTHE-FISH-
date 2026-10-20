# Fishing: Hold the Glow

Current gameplay restores the pictured first **Hold & Countersteer** version:
one **LEFT** and one **RIGHT** button, the fish marker and gold direction band.
The timed-tap experiment is superseded. Matching peers require protocol **73**.
The directional holds, rod motion, splashes and landing feedback remain. Fish wander independently through one shared pool. The centre track has a larger round fish marker; a loose-to-taut rope icon and gradual orange/red rope show tension. Camera choices, audio, the tutorial and Leave island are in the shared sports-style settings menu.

## Playing

1. Aim until the reticle turns teal. Tap Cast, then tap Hook when the fish shows
   **!**, inside the existing 1.8-second bite window.
2. **Hold the glowing Left or Right button** against the fish's pull. There is
   no tap window or one-attempt sweep. Keep holding while that side is useful.
3. Switch when the glow changes sides. The title says **HOLD THE GLOW**, or
   **SWITCH SIDES** if the held direction is wrong. The fish previews the lateral
   pull; the gold band always agrees with the lit button.
4. Release both when **RELEASE TO COOL** appears. This hint stays until tension
   falls below 58%, avoiding flicker. Hold the glowing side again after cooling.
   At 100% tension the line snaps. An unattended fight ends after 30 seconds.
5. Correct holding continuously winds the physical line. Spring, drag, fish pull
   and distance determine Catch progress. Wrong-side holding builds strain without
   normal winding. The original winding law returns, with a modest 20% speed
   assist over the earlier continuous-hold baseline. A physical reel-drag limit
   lets line slip under a sudden correct-side load, leaving time to react to a
   large fish's initial surge. Accumulated strain still requires cooling, and
   wrong-side holds can snap the line. The timed-stroke clutch is removed.

The bottom card matches the reference: title, fish/gold band, then navy native
Catch/Tension captions above the two bars inside the frame. Buttons retain fixed
touch bounds and text while their artwork presses and glows. There are no early/
late penalties, timing grades, keyboard guides or extra fight buttons. Development
keyboard shortcuts remain off the mobile HUD. The same four-page tutorial and
How to Play reuse the live buttons and meter; the updated guide opens once.

## Retained presentation and multiplayer

Eye-level view stays at the player's eyes throughout hooking and Left/Right
holds; looking around remains available. Follow retains accepted-hook framing, smooth directional shoulder shifts,
obstruction handling and return to the preferred camera. Arrival selects Follow.
Island View is a manual settings choice: a wider follow at about seven metres,
with gentle pitch and collision handling. It stays behind the player while hooked. Reduced Motion retains the preferred view.
Rod grip/lean, attached line rendering, aggressive hooked-fish swimming
and gravity-driven splashes retain their current implementations. Movement,
jump, manual camera switching and travel hide while unavailable; Cancel stays
available. The chosen round skill, Fish Landed/points feedback, sound and rankings remain.

The host accepts explicit directional presses and renewals during a valid fight.
A 0.8-second safety lease refreshes while the thumb stays down; missing releases,
focus loss, cancellation and travel cannot leave an indefinite hold. A new finger
can take over; releasing an older finger cannot stop it. Timestamp, ownership,
cast/round tokens, scoring, fish claims and ability validity remain host-controlled.
Repeated commands without simulation time never create progress or points.

Round, Cup, Crew, Wanted, six skills with one choice from a shared three-card draft, 2/5/10 points, seven-second
respawns and results remain. Competitive rounds last three minutes. Crew requires
20 points times starting players, all sizes and a catch from everyone.

## Shared sports artwork and editable layout

Fishing reuses the Football/Basketball fonts and shared press animation. The
13 October refinement uses cream-bevelled aqua/gold circles for **one Left and
one Right**, with single-line **LEFT** and **RIGHT** native labels inside the
coloured discs. The guide names the required side and confirms correct holding.
Right reverses the arrow artwork horizontally without reversing its text.
Gold marks the active side; pressure warnings and restrained pulses retain
fixed touch bounds. The familiar circular illustrated reel returns for Cast/Hook.
An illustrated sliced frame contains the dark countersteer track and separate
Catch/Tension bars. Fish, fisher, flame and clock illustrations remain; Golf also
uses the clock. Ranks, actual award pops, blue local row and milestone feedback
remain unchanged.

Four delivery PNGs total **201,479 bytes** before Android packing; this is not
their APK cost. Buttons are 192×192 and the sliced frame is 512×128, with Android
ASTC 6×6 and no mipmaps. Labels remain native sharp text. Left/Right share their
normal/lit textures; no font, shader, audio, model or runtime package is added.
Keep [editable masters and the portable generator](../ArtSource/Fishing/UI/Illustrated/README.md)
outside Unity. The now-unused native arrow implementation is removed.

Tune `Game/Assets/_Game/Resources/FishingUILayout.asset`: `actionSize` sets
Cast/Hook, `fightControlSize` sets both fight circles, and `gaugeSize` sets the
fight card. `fightLabelPosition`/`fightLabelSize` keep captions inside the circles.
`guideSize`/`guidePosition`, `progressSize`, `catchPosition`,
`tensionPosition` and `fightTitlePosition` place the card's contents. The title
and track receive the standard `gap` inset below the illustrated bevel. Margins, gaps, clock
positions and typography settings remain editable. Layout uses the existing
safe-area frame and design units, with landscape fit constraints. Press/pulse
effects animate artwork inside fixed touch rectangles. Both direction captions
stay in their live layout position during a press, including the former Cast
button after it becomes Right. A limited winding assist during the first five
seconds makes catches modestly faster without changing bite or score rules. Reduced motion suppresses
the extra breathing animation while retaining colour/direction and deadline cues.

## Stronger fights and wider swimming — 13 October 2026

Free fish follow continuous ellipses spanning 3.3 metres toward/away from the
pier and 4.2 metres sideways. Each school remains inside its own casting range.
Hooked fish have stronger lateral thrust and surface surges, with spring force,
water drag, buoyancy and airborne gravity still controlling their movement.
Presentation retains a 12 m/s travel cap and smooth turns capped at 200 degrees
per second. Catch assistance, bite windows, countersteering, tension and score
rules are unchanged by this motion pass.

Surface contact produces taller, thicker spray and wider wakes. Spray inherits
fish velocity and falls under gravity; droplets disappear on water contact or
after one second. The shared-material pool remains capped at 48, with four
droplets per burst for Reduced Motion and twelve otherwise. No new image,
model, shader, audio or runtime dependency is needed for these effects. This
is a stylized force model, rather than a fluid simulation.

## Authority and input recovery

The earlier clarity release used **protocol 55** because the replicated Fishing
record includes physical swim velocity/depth and [Fishing skill state](FISHING-ABILITIES.md),
alongside signed steering, its accepted time and fish pull. Fishing
extends its action enum; Football/Basketball request payloads and rules remain
unchanged. The host owns pull, spring/drag, strain, distance and scores.
Guests submit a direction with the existing owner-only RPC, round/cast revision
and synchronized press time. Old releases, invalid timestamps and obsolete
one-button ReelStart actions are rejected. A renewable 0.8-second lease clears
held input if messages stop. Switching thumb ownership prevents an old finger's
release from cancelling a newer side. Focus/pause, cancellation, results, island
exit, disconnected players and round changes retain their existing cleanup.

## Cleanup and verification

The ornamental LagoonFrame remains archived with its `.meta` after checking
GUID/resource/build-generator references. The approved ReelButton is restored,
derived from its retained master with its original GUID. Superseded complete
players and draft evidence use the repository archive tool. Security, timing,
scoring and development review fixtures are retained.

Fresh player checks and measured APK results are recorded in
[Fishing verification](FISHING-VERIFICATION.md), [build size](BUILD-SIZE.md)
and [packing audit](APK-SIZE-AUDIT.md). Physical-phone touch/visual/FPS and real
WAN qualification require separate device sessions. Arrival and round awards use
the shared sports presentation.
