# Fishing: Hold & Countersteer

Selected **B — Hold & Countersteer**, 12 October 2026.
[Approved concept](FishingUI/Hold-Countersteer-Reference.png) is an authoring
reference outside Unity; it is not shipped as a screen-sized texture.

Aim and Cast / **F** still select a real available fish. React to **Hook / F**
inside the existing 1.8-second bite window. After Hook, the joystick becomes
**Hold Left** and the right action becomes **Hold Right**. Hold the gold side
against the fish's pull, switch when the other side lights up, and release both
to recover tension. On a keyboard, use **A/D** or **Left/Right arrows**.
F casts/hooks only; it cannot bypass steering during the fight.

The bottom guide previews turns with a moving fish marker and shows your actual
steer. Correct steering shortens the line; wrong steering adds strain without
normal reeling progress. Catch and Tension labels sit above their side-by-side
bars inside the illustrated fight card.
At high tension both buttons receive a warm warning tint and say **Release**. At 100% the line
snaps. Bigger fish retain stronger surges and require release timing. Cancel / X
remains available. Walking and jumping are blocked during the active line;
movement, jump and camera controls visibly fade while unavailable. They recover
after landing, escape or cancellation. Rod lean and lateral fish motion follow
the same fight state seen by peers. Sharper lateral darts, short breaches and
water wakes and gravity-driven contact splashes make fish surges visible; the line follows the actual fish endpoint.
The fisher leans back and countersteers with the torso while feet remain planted
and both hands retain their authored rod contacts. These presentation motions
reuse the existing models, rig and materials.

Round, Cup, Crew, Wanted bonuses, 2/5/10 base points, seven-second respawns,
30-second fight timeout and results are unchanged. A competitive round still
ends after three minutes. Crew still requires 20 points times starting players,
all three sizes and a catch from everyone.

## Shared sports artwork and editable layout

Fishing reuses the Football/Basketball fonts and shared press animation. The
13 October refinement uses cream-bevelled aqua/gold circles for **one Left and
one Right**, with single-line **LEFT** and **RIGHT** native labels inside the
coloured discs. The guide says **HOLD THE GLOW**, avoiding repeated captions.
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

## Authority and input recovery

Matching room players require **protocol 49** because the replicated Fishing
record includes physical swim velocity/depth and [gear inventory](FISHING-ABILITIES.md),
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
WAN qualification require separate device sessions. New celebrations remain
deferred.
