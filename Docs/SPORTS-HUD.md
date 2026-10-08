# Team controls and pocket maps

Football and basketball use team possession to choose the action buttons.
Your team holding the ball means attack; an opponent holding it or a loose
ball means defense. The action slots keep their positions. Switching roles
cancels the previous captured touch before the replacement accepts a new
press, and updates its label and availability in the same frame.

| Sport | Attack | Defense | Always available when gameplay permits |
| --- | --- | --- | --- |
| Football | Kick, hold/release F | Pressure, hold Q; Tackle, E | Dash, left Ctrl; Jump; Camera |
| Basketball | Shoot, hold/release E; Pass, hold/release Q | Block, E; Guard, hold Q; Steal, hold F | Jump / Jump Block; Camera |

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
pressure to one another. Use matching **protocol 34** players.

## Pocket maps

Both sports use a plain bordered board at bottom centre, with simple pitch
or court lines. The board is **204 × 114** design units, down from 326 × 216
(67% less area). There are no headings, legends, faces or decorative surrounds.

- **Blue `#2788ED`: your team. Red `#EC575D`: opponents.** These colours are
  relative to the local player, independent of squad letter or outfit.
- Your marker has a white ring. The ball is a smaller white dot.
- Every active player appears, including players outside the camera view.
  Off-field positions clamp to the map edge; in-transit/inactive actors hide.
- The map uses the arena's local coordinates, with the long axis horizontal.
  Markers refresh at 12.5 Hz. It never intercepts movement or look touches.
- The local football stamina meter and control hint sit above the map.
  Station directions sit below the upper-left status; the station's travel
  card moves above the map when visiting a football or basketball station.

## Button treatment

Football, basketball, golf and fishing use plain text controls in a round
cluster near the right thumb. Primary actions are 160 units across; supporting
actions are 132–138 units. Utility and Sky-Sail controls use rounded pills.
There are no button icons or decorative highlights. Short labels show the
action and keyboard key (or Hold on mobile for sustained actions).

Pressing compresses the visual to 91% and releasing gives a small bounce.
The invisible touch target stays full-sized throughout the animation, so a
finger near its edge is not cancelled by the shrinking face. Hover, keyboard
focus and disabled states also receive feedback; reduced-motion preferences
suppress scaling. Offensive shared slots use gold and defensive slots use
sky blue. Existing hold, swipe and cancel gestures remain intact.
The special Chinese cart/aim font is preserved. No imported texture, audio,
animation, shader or package is added.

## Verification entry points

`Tools/Build/Test-SportsHud.ps1 -Capture` checks all four sports, possession
changes, touch cancellation, map bounds and styling. `-Mode Local` starts
three basketball peers to verify the carrier, teammate and opponent views.
Use `-Width` and `-Height` for alternate landscape aspect ratios. The default
player is resolved relative to the script's repository, independent of the
shell's working directory. Logs and images stay in ignored `Builds/SportsHudQA/`.

See [verification](VERIFICATION.md), [APK measurements](BUILD-SIZE.md), and
[packing audit](APK-SIZE-AUDIT.md) for the measured delivery and remaining limits.
