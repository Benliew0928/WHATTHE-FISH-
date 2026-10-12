# Team controls and pocket maps

Football and basketball use team possession to choose the action buttons.
Your team holding the ball means attack; an opponent holding it or a loose
ball means defense. The action slots keep their positions. Switching roles
cancels the previous captured touch before the replacement accepts a new
press, and updates its label and availability in the same frame.

| Sport | Attack | Defense | Always available when gameplay permits |
| --- | --- | --- | --- |
| Football | Kick, hold/release F | Pressure, hold Q; Tackle, E | Dash, left Ctrl; Jump; Camera |
| Basketball | Shoot / Pass: hold, slide, release | Block; hold Guard / Steal | Jump / Jump Block; camera inside Settings |

Attack teammates see offensive controls, but only the carrier can shoot or
pass the ball. Defense controls stay visible for a loose ball, with actions
that require a carrier disabled. This does not introduce air steals or let a
freshly switched slot reinterpret an already-held finger.

Football uses the match's confirmed teams. Before a match, unassigned practice
players treat each other as opponents. Basketball automatically alternates
squads A and B by stable network join ID. Teams do not reshuffle when someone
leaves; all peers derive the same assignment. Solo practice remains playable.
Basketball's shared practice score remains shared; a team score/timed basketball
match is outside this HUD change. Teammates cannot steal, block or apply shot
pressure to one another. Use matching **protocol 44** players.

## Pocket maps

Stadium camera follows the ball in football and basketball. It leads moving
passes slightly, follows shot height and opens the view for an approaching arc.
Nearby players and basketball rims contribute to framing; a distant local
player cannot drag the view away from play. Camera pan remains independent of
action aim, and movement follows the presented camera direction.

When your character leaves the screen, a cyan arrow and **YOU** label appear
at the safe screen edge. The distance is the ground distance from your character
to the ball. The marker avoids HUD panels, does not intercept touches, and hides
on re-entry, camera-mode changes, travel and menus. A small re-entry margin
prevents flickering at the screen boundary. The arrow uses native UI geometry
and the existing font; it adds no bitmap assets. See the
[camera behaviour and checks](STADIUM-CAMERA.md).

Football uses a **204 × 114** board at bottom centre. Basketball uses a
**102 × 62** rounded navy court map at the top-left of its 800 × 450 layout.
Neither map has headings, legends or portraits.

- **Blue `#2788ED`: your team. Red `#EC575D`: opponents.** These colours are
  relative to the local player, independent of squad letter or outfit.
- Your marker has a white ring. The ball is white in football and gold in
  basketball. Basketball opponents use triangles; teammates use circles.
- Every active player appears, including players outside the camera view.
  Off-field positions clamp to the map edge; in-transit/inactive actors hide.
- The map uses the arena's local coordinates, with the long axis horizontal.
  Markers refresh at 12.5 Hz. It never intercepts movement or look touches.
- The local football stamina meter and control hint sit above the map.
  Station directions sit below the upper-left status; the station's travel
  card moves above the map when visiting a football or basketball station.

## Button treatment

Basketball implements the approved [illustrated mobile design](BasketballUI/README.md):
round gold Shoot, sky Pass, cream Jump, navy prop slot and illustrated defensive
slots. Controls use the existing Cove fonts, cream inset rims and short navy
shadows. One 512-pixel atlas contains the approved SVG illustrations. Keyboard
shortcuts are retained for testing but are not displayed. The top-right gear
opens camera settings, the Rules / Controls / Props guide, free roam and leaving.
Court name, FPS, station distance and permanent instructions are removed.

Hold Shoot or Pass, then follow the faint connected upper or upper-left path
with the same finger. Cancel is a release destination, not a second button. Entering it arms the coral
cue; sliding back keeps aiming, and only lifting inside cancels. A timed
shot arc stays attached to Shoot. Far or otherwise unavailable Dunk/Layup keeps
the actual timing phase and explains the fallback; only eligible finishes hide
it. The other movement thumb stays usable. Menus clear pending actions and block
local input, while room play continues.


Football uses plain text controls in a round
cluster near the right thumb. Primary actions are 160 units across; supporting
actions are 132–138 units. Utility and Sky-Sail controls use rounded pills.
 Short labels show the
action and keyboard key (or Hold on mobile for sustained actions).

Pressing compresses the visual to 91% and releasing gives a small bounce.
The invisible touch target stays full-sized throughout the animation, so a
finger near its edge is not cancelled by the shrinking face. Hover, keyboard
focus and disabled states also receive feedback; reduced-motion preferences
suppress scaling. Offensive shared slots use gold and defensive slots use
sky blue. Existing hold, swipe and cancel gestures remain intact.
The special Chinese cart/aim font is preserved. This generic control skin uses
existing fonts and generated UI geometry.

Golf uses the selected **A** cream/navy/gold stickers, **C** scorecard column
lines and **B** framed hold/release hint with a native **!**. Its illustrated
Swing/Putt button retains the existing live power caption and press/release
feedback; Aim becomes red Cancel/X. Camera/Jump use shared native icons, and
the complete local scorecard row is blue. Five-hole rules, rankings, the
conditional final countdown, shot guide, input handlers and cart animations
remain unchanged. New artwork is compressed to 256/128-pixel sprites; frames,
fonts, portraits and clock reuse Fishing resources. See
[Golf presentation and editable layout](GOLF-UI.md).

Fishing retains its **Lagoon Stickers** HUD: illustrated frames and reel button,
native lettering, centre aiming reticle, animated rankings and catch/tension
gauges. Cast/Hook/Reel labels follow the fishing state. Its responsive landscape
frames and fixed touch targets preserve pointer ownership while the visual face
animates. Existing compressed sprites, fish models and fonts are reused. See
[fishing controls](FISHING-MINIGAME.md) and [the selected UI direction](FISHING-UX-DIRECTION.md).

## Basketball ability slot

Tap the icon above Steal, or press G, to use the held prop. The icon remains
visible with a progress ring during forced aiming or portal reception. The
caption explains unavailable actions. Team Turbo lasts five seconds.
See [rules, counterplay and multiplayer behavior](BASKETBALL-PROPS.md).

## Verification entry points

`Tools/Build/Test-SportsHud.ps1 -Capture` checks all four sports, possession
changes, touch cancellation, map bounds and styling. `-Mode Local` starts
three basketball peers to verify the carrier, teammate and opponent views.
Use `-Width` and `-Height` for alternate landscape aspect ratios. The default
player is resolved relative to the script's repository, independent of the
shell's working directory. Logs and images stay in ignored `Builds/SportsHudQA/`.

See [verification](VERIFICATION.md), [APK measurements](BUILD-SIZE.md), and
[packing audit](APK-SIZE-AUDIT.md) for the measured delivery and remaining limits.
