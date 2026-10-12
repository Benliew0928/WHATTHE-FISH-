# Team controls and pocket maps

Basketball and football now share [full-colour cartoon artwork and Manrope
typography](SportsUI/README.md) across controls, scores, settings and guides.
The existing cream/pastel frames and gesture layouts remain. Basketball volume
thumbs are round and stay inside their tracks, leaving label space clear.

Football and basketball use team possession to choose the action buttons.
Your team holding the ball means attack; an opponent holding it or a loose
ball means defense. The action slots keep their positions. Switching roles
cancels the previous captured touch before the replacement accepts a new
press, and updates its label and availability in the same frame.

| Sport | Attack | Defense | Always available when gameplay permits |
| --- | --- | --- | --- |
| Football | Kick: hold, slide, release | Hold Pressure; tap Tackle | Dash; Jump; held ability; camera inside Settings |
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
pressure to one another. Use matching **protocol 46** players, including the
[approved football audio](FOOTBALL-AUDIO.md).

Football's illustrated ability slot sits beside Dash. Tap (or press G) for
Team Turbo or Portal Steal; Wall opens a placement preview with Place and Cancel.
The slot is single-use and replaced by a new pickup. Team effect timers appear
below the scoreboard. See [football abilities](FOOTBALL-ABILITIES.md) for the
five-second spawn countdown, team restrictions and wall collision rules.

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

Football uses a **125 × 75** rounded navy map at the upper left of its
800 × 450 design space. Basketball uses a
**102 × 62** rounded navy court map at the top-left of its 800 × 450 layout.
Neither map has headings, legends or portraits.

- **Blue `#2788ED`: your team. Red `#EC575D`: opponents.** These colours are
  relative to the local player, independent of squad letter or outfit.
- Your marker has a white ring. The ball is white in football and gold in
  basketball. Opponents use triangles; teammates use circles.
- Every active player appears, including players outside the camera view.
  Off-field positions clamp to the map edge; in-transit/inactive actors hide.
- The map uses the arena's local coordinates, with the long axis horizontal.
  Markers refresh at 12.5 Hz. It never intercepts movement or look touches.
- Football stamina sits directly below Dash. Permanent station distance and
  control hints are removed; travel controls still appear when visiting a station.

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


Football implements the approved [Touchline design](FootballUI/README.md):
108-unit round gold Kick / sky Pressure, 80-unit mint Dash with its stamina
meter, cream Jump and sky Tackle. A regular top-right gear opens Camera,
Controls / Rules / Stamina and a separate leave confirmation. Labels contain
no desktop keys. The score strip keeps actual phases, team choice and host start.

Holding Kick shows a 270-degree power arc and a faint path to a coral landing.
The original finger keeps the charge; only release in Cancel cancels. Sliding
back keeps power, and full charge holds without auto-fire. Joystick input stays
independent. Menus, interrupted touches, resizing and role changes clear holds.
Existing football aim rails and launch rules remain intact. Frames and feedback
reuse native geometry; one 512 × 256 atlas adds the approved football icons.
The fixed hit rectangle does not move when its artwork depresses.


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

Current illustrated controls are verified with `Tools/Build/Test-FootballUI.ps1`
and `Tools/Build/Test-BasketballUI.ps1`, using `-PlayerPath` for the intended
player. Football match/physics and effort suites cover gameplay and local
host/guest behavior. Their source-relative paths work independently of the
caller's directory; logs and captures stay under ignored Builds.

The older generic-skin expectations in `Test-SportsHud.ps1` predate both
illustrated court HUDs. Use the current dedicated reviews for visual qualification.

See [verification](VERIFICATION.md), [APK measurements](BUILD-SIZE.md), and
[packing audit](APK-SIZE-AUDIT.md) for the measured delivery and remaining limits.
