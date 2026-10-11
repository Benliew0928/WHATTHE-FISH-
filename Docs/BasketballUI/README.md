# Basketball UI proposal — Hold, slide, release

11 October 2026 · Revision 2, with playtest fixes · **Approved design implemented in the basketball HUD.**

Open [the interactive proposal](proposal.html). Choose a possession or gesture
state with **Preview**; press the top-right gear for camera choices, the court
guide and leaving play. Hold Shoot or Pass and follow a faint path with the same
finger to choose a variation. Release inside Cancel to stop; sliding back keeps aiming. The preview does not
simulate basketball, move the camera, or connect to a room.

This revision keeps the accepted joystick, Pass, Jump and top-centre scoreboard
in the landscape layout. Shoot is now a round gold button with a simpler ball
illustration. Connected drag destinations replace scattered sub-buttons. Shot
timing wraps around Shoot instead of occupying a small detached panel. Scoreboard
changes for individual game modes are deferred as requested.

## Direction

Use the existing game's cream, navy, gold, sky and mint palette, with the
existing CoveDisplay and CoveText fonts. Soft rounded buttons have a dark ink
edge, cream inset rim and a short solid shadow. Small basketball illustrations
and legible action words carry meaning together. Avoid ornate frames, glowing
panels, emoji, decorative currency, and permanent instructions across the court.

The [Golf reference](../GolfUI/README.md) supplies the family resemblance.
Basketball uses a quieter treatment: less illustration around the perimeter,
smaller utility controls and more space for the action. The background in the
proposal is a real existing Windows court capture, with the proposed UI layered
over it. Its player pose is a reference photograph of gameplay, not a simulated
possession state. Embedded fonts and prop art come from the current project.
The three prop illustrations retain the existing approved design.

## Screen layout

The intended game orientation is landscape. Measurements below are screen-space
design units at **800 × 450**, equivalent to half the existing 1600 × 900 canvas.
Positions are offsets from the safe area, not from physical screen edges. Keep
the action cluster anchored to the bottom-right and the joystick bottom-left;
extra width on wide phones becomes open court space. Do not stretch buttons.

| Element | Size | Placement / purpose |
| --- | --- | --- |
| Shared score | About 230 × 46 | Top-centre, 16 from top. Shared points and made / attempted baskets. No invented team scoreboard or timer. |
| Mini-map | 102 × 62 | Top-left, 18 from left and 16 from top. Teammates are circles, opponents triangles, ball gold; local marker outlined. |
| Settings | 48 × 48 | Top-right, 18 from right and 15 from top. Gear only, with accessible name. |
| Movement stick | 108 × 108 | Bottom-left, 28 from left and 31 from bottom. Push fully to run. No separate Sprint button. |
| Shoot / Block | 106 × 106 | Bottom-right, 32 from right and 47 from bottom. Round main action; spacing reserves room for its timing arc. |
| Pass / Guard | 80 × 80 | 154 from right and 30 from bottom. Fixed secondary slot. |
| Jump / Jump Block | 64 × 64 / 68 × 68 | 23 from right and 174 from bottom. The defensive label wraps onto two lines. |
| Held prop | 80 × 80 | 137 from right and 173 from bottom. Existing illustration, short name and one-use badge. |
| Steal | 68 × 68 | 241 from right and 118 from bottom. Visible while defending; lifted clear of the reference player's head. |
| Attached timing / distance arc | Radius = half button width + 12 | Centred on the held button. 240-degree arc, open at the bottom. Its 148-wide release cue sits just below the button. |
| Drag destinations | 92 × 49 | Offsets from the held button centre: upper `(0, -145)`, diagonal `(-105, -100)`, Cancel `(-178, -22)`. All appear together during a hold. |

The review surface reflows below 480 pixels so it can be inspected in a narrow
chat. Its drag fan becomes taller to avoid the joystick; this portrait reflow is
**not** a proposed portrait gameplay mode or a production gesture distance. In the
game, use safe-area anchors and a landscape layout, with physical-phone checks
for notches, gesture bars, reach and minimum 48 logical-pixel touch targets.
The foreground player's silhouette, ball and hoop must remain unobscured in
Follow and Court view; the still image cannot prove camera framing on every
device. Re-check before accepting an implementation.

Remove the court-name heading, station distance/directions, FPS/benchmark
readout, permanent movement instructions, keyboard keycaps, standalone Camera
button and large Leave court/Free roam buttons from the playing HUD. Keep
keyboard support for testing behind the same actions without showing keys.
Keep connection failures actionable when they occur; removing the permanent
technical status does not mean suppressing real errors.

## Buttons and interaction states

| Action | Illustration | Face / behavior |
| --- | --- | --- |
| Shoot | Orange basketball, navy seams, warm highlight and three short motion marks | Pale gold, circular, largest. Clear Shoot label and small HOLD cue. |
| Pass | Moving orange ball and teal arrow | Unchanged sky button. Tap short; hold distance; drag up for Loft, upper-left for Bounce. |
| Block | Raised hand with teal cuff | Sky, same slot as Shoot. Tap standing block; swipe up and release for Jump Block. |
| Guard | Player with spread arms inside a teal shield | Mint, same slot as Pass. Hold to guard. |
| Steal | Open hand reaching under the ball | Sky. Hold to repeat; recovery restrictions remain. |
| Jump | Shoe with upward chevrons | Cream, smaller. Changes to Jump Block on defense. |
| Prop | Existing Lock-on / Turbo / Portal illustration | Navy with cream edge and gold inner line. Short label plus quantity. |
| Cancel | Cross, word and drag direction | Faint coral destination connected by a path. Drag the original finger into it; it is not a separate tappable button. |

Button silhouettes, icons and labels all carry meaning; colour alone never
identifies an action. Use the [editable SVG masters](icons/) directly when
preparing production sprites. These are actual vector sources, not painted
mockup text. Keep labels as live text.

- **Idle:** cream inner rim, navy outline, short downward shadow.
- **Pressed:** tap actions depress 3 units and scale to 96%; full touch region
  stays fixed. About 100 ms. Hold actions remain fixed so the arc and paths do not
  jump under the finger. Respect reduced motion.
- **Unavailable:** muted face, icon and outline; preserve the label and position.
  A teammate holding the ball leaves Shoot and Pass disabled. Loose-ball state
  uses the current defensive slots with unavailable actions disabled.
- **Role change:** cancel the original gesture before binding the new action.
  A finger release must never trigger the action that replaced its old slot.
- **Holding Shoot/Pass:** temporarily hide other right-side action buttons, reveal
  the connected destinations and Cancel, and keep tracking the original finger.
  This avoids another live control occupying the swipe corridor. Hiding siblings
  must not cancel their already-owned pointer inconsistently; clear their held
  input explicitly when the new gesture starts.
- **Timing:** a cream diamond crosses a green window with a check mark on the
  arc around Shoot. The green segment occupies the upper-right portion, outside
  the button face under the thumb. The actual
  host timing/window, including defensive pressure, drives the meter. The HTML
  uses a sample sweep and does not calculate a real shot outcome.
- **Finishes:** show Dunk/Layup names and eligibility. The selected path and
  destination turn mint, the destination says Release, and the timing arc hides
  for an eligible finish. If unavailable, display the
  actual reason and retain the normal shot meter; do not promise a successful
  finish. The proposal's Finish in range design control demonstrates this state;
  it does not calculate player distance or real eligibility.
- **Feedback:** short contextual messages for a score, loose ball or blocked
  ability; no permanent statistics banners. Keep cues near the action without
  covering the target. Ability progress and remaining duration should live on
  the slot when active; no full-width announcement bar.
- **Empty prop:** hide the empty action. Introduce pickup guidance once through
  the guide/onboarding, rather than a permanent “Find a prop” button.

## One continuous gesture

The destinations are landing zones, not independent buttons. Their dashed edges,
faint connecting paths, directional chevrons and Drag labels communicate the
same-finger interaction. Do not require a second tap or a second finger.

| Finger movement from the held button | Shoot | Pass |
| --- | --- | --- |
| Stay near the original button, then lift | Release the timed shot | Release a chest pass at the held distance |
| Follow the upper path, then lift | Dunk if eligible | Loft pass |
| Follow the upper-left path, then lift | Layup if eligible | Bounce pass |
| Reach the coral Cancel destination | Arm Cancel; keep holding to slide back | Arm Cancel; keep holding to slide back |
| Lift inside the coral Cancel destination | Cancel; keep the ball | Cancel; keep the ball |
| Return to the original button before lifting | Return to the timed shot | Return to a chest pass |
| Lift outside the original button and the action corridors | Cancel; keep the ball | Cancel; keep the ball |

Lift your finger appears when a variation is selected. The selected path becomes
solid, with an arrow and stronger fill; the change is not communicated by colour
alone. The release cue stays under the original button so it remains visible
when the thumb moves onto a destination. For Pass the arc fills with distance;
there is no shot timing window. Hints appear only while holding an action.

The original pointer owns the full gesture, including release outside the
button. Cancel, pointer cancellation, window focus loss, menus and possession
changes clear pending input. Lifting inside Cancel must never fire the original action. Sliding back out
before lifting restores the chosen shot or pass. In production, clear on lost pointer
capture and app suspension as well. Keep movement on the other thumb available.

Upper-left Layup/Bounce deliberately replaces the earlier downward mapping to
avoid dragging toward the phone's bottom edge. The Unity gesture recognizer and guide now use this mapping together, preserving
gameplay authority and eligibility checks. Physical-phone reach and tolerance
still need device qualification; the browser preview alone is not multitouch validation.

## Settings and court guide

The top-right gear opens a cream panel with a quiet court dim behind it. Existing
camera modes become named choices: **Eye level** (mode 0), **Follow** (mode 1),
**Court view** (mode 2). A visible tick and filled tile mark the current choice.
Select immediately; no extra Save button. Preserve the player's choice. Do not
add nonfunctional future settings rows. This panel can grow into a category
navigation layout when real settings are added.

**Court guide** has three clear tabs, with short illustrated explanations:

- **Rules:** automatic reachable-ball pickup and dribbling; 2 and 3 points;
  rebounding and defending; shared practice, no match timer or foul penalties.
- **Controls:** movement/look, shooting/finishes, passing and defending. Only
  mobile gestures are shown. The Cancel rule is always visible on this page.
- **Props:** Lock-on's two-second stationary aim and interruption, Turbo's
  five-second 1.5× team speed, and Portal's home-half requirement. Explain one
  slot/one use/replacement and that activation requires the painted court.

Opening a menu clears held gameplay input and prevents touches passing through.
Room play continues: this is not a multiplayer pause. Returning does not restore
a pending shot/steal/guard. Use a selected-tab state, visible close button, focus
containment for testing/accessibility and a clear Back to court action.

**Free roam** remains a quiet setting action. It means stop participating and
walk around the same island, retaining room membership. It must not send anyone
to a different island or alter shared travel.

**Leave court** opens a confirmation with the actual consequence:

| Session role | Confirmation | Final action |
| --- | --- | --- |
| Host | Everyone returns to the room; practice score resets. | Return to room |
| Guest | You leave the room; others can continue. | Leave room |
| Offline | Return to sport selection; practice score resets. | Leave court |

These choices preserve the current `AppRoot.Return` behavior. Free roam uses
the current court-participation path. The prototype defaults to Host; its design
controls can show Guest and Offline. No preview action changes a live session.

## Implementation fidelity and cost

| Token | Value |
| --- | --- |
| Ink | `#123F50` |
| Cream | `#FFF9E9` |
| Paper inset | `#F4ECD5` |
| Gold accent | `#FFD47D` |
| Shoot face | `#FFDA89` |
| Sky / action | `#BAEDF4` |
| Mint / Guard | `#A7E8CC` |
| Teal illustration | `#40B8B8` |
| Coral / Cancel | `#FF9273` |
| Destructive text | `#A63438` |

Use existing Unity UI / CovePlate geometry, native fonts, reusable frames and
one small icon atlas. Use the exact vector masters and original prop icons.
Adapt the current input components to the proposed directions and pointer
ownership without replacing gameplay or network authority. Build paths and arcs
from reusable UI geometry, not full-screen textures. No new font, UI package,
blur shader or large background
texture is needed. The court image and embedded HTML resources are **proposal
material outside Game/Assets**, never runtime assets.

A proposed upper bound is a 512 × 512 ASTC 6×6 icon atlas without mipmaps:
118,336 raw block bytes, before engine metadata and APK compression. This is a
planning allowance, not a measured APK increase. Aim lower after packing and
reuse existing utility glyphs. Do not add separate baked panels for each screen.

Exact assets, palette, fonts and geometry make this design reproducible; actual
Unity parity still needs an implementation and rendered comparison. Check
landscape 16:9 and wide phones, all three camera modes, possession transitions,
multi-touch cancellation, ineligible abilities, host/guest exit and guide text
before approval of the delivered game. A browser preview does not prove phone
visual quality, touch behavior or FPS.

## Scope and evidence

This task adds only the proposal and its documentation/vector sources. It does
not change Unity code, gameplay, scenes, runtime assets, Android or Windows
delivery. Existing unrelated work is preserved and nothing is staged.

Existing APK measured before this revision: **89,116,652 bytes**, with
**10,883,348 bytes** to the strict 100,000,000-byte boundary. It exceeds the
75,000,000-byte development target by **14,116,652 bytes**. No APK was rebuilt;
that measurement describes the existing player and is not validation of the
proposed UI. See [the release record](../BUILD-SIZE.md).

Local review captures and the standalone inspection wrapper are kept under
ignored `Builds/BasketballUIProposal/`. The superseded proposal, Shoot master and
earlier review output were archived with the repository archive tool in
`Legacy/20261011-162132-474-basketball-ui-v1/`. No game assets were moved.
Browser review and repository readiness findings are recorded in the current
evidence folder. The implementation now has its own fresh AndroidSubmission,
measured size audit and complete tested Windows player; see the linked release
and verification records below.

Sources reviewed: [current HUD](../../Game/Assets/_Game/Shared/UI/BasketballHUD.cs),
[Cove UI](../../Game/Assets/_Game/Shared/UI/CoveUI.cs),
[shared action layout](../../Game/Assets/_Game/Shared/UI/AppRoot.cs),
[defense](../BASKETBALL-DEFENSE.md), [passing](../BASKETBALL-PASSING.md),
[finishes](../BASKETBALL-FINISHES.md), [props](../BASKETBALL-PROPS.md), and
[camera modes](../../Game/Assets/_Game/Shared/Player/PlayerView.cs).


## Unity implementation

The live HUD uses the exact sixteen [SVG icon masters](icons/) in one padded
512 × 512 atlas. `Tools/Build/Generate-BasketballUI.cjs` regenerates it using
Node.js and `sharp`, installed outside the repository and exposed through
`NODE_PATH`. The script resolves its root from its own location, not the current
working directory. [atlas.json](atlas.json) records portable source paths and
SHA-256 hashes. Unity's Android import uses ASTC 6×6, no mipmaps and no CPU copy.
Frames, shadows, connected paths and arcs are UI geometry; labels reuse the
existing Cove fonts. Text is rasterized at the parent canvas density so the
half-size layout does not enlarge low-resolution glyphs.

`BasketballHUD`, `BasketballMenus`, `BasketballGestureGuide` and
`BasketballUIStyle` own layout and presentation. Existing Shoot/Pass handlers
retain the original pointer and read the actual gameplay charge phase. No
separate tap is needed at a destination. Returning to the centre restores the
base action without restarting charge. Lifting inside Cancel, lifting outside the
corridors, OS cancellation, focus loss, suspension, role changes, resizing and
opening menus cancel the owned gesture. The movement thumb stays independent.

When Dunk or Layup is unavailable, the destination shows the actual reason.
For range, the hint says “Too far for Dunk/Layup · release a timed shot”. The
same timing arc remains around Shoot with its current phase and actual green
window; the cue below says “Timed shot · green”. Eligibility is updated while
the player moves. Only an eligible finish hides timing and offers “Release”.
The existing host rules make the final decision at release.

Settings and its Rules / Controls / Props guide use fixed typographic regions,
consistent margins, a safe-area-scaled panel and a separate leave confirmation.
They clear held actions and block movement while open. The shared room continues
playing. Camera selection retains the existing saved preference. The free-roam
switch remains inside settings. PC test shortcuts remain functional but are not
printed in the playing UI.

Build the review player with `Tools/Build/Build-BasketballUIReview.ps1`, then run
`Tools/Build/Test-BasketballUI.ps1`. Its development-only harness feeds Unity's
actual touch input module, checks finger ownership and timing, and renders the
HUD and all menu pages. Fresh APK size and delivery evidence are recorded in
[BUILD-SIZE](../BUILD-SIZE.md) and [VERIFICATION](../VERIFICATION.md).

## Playtest fixes — court restarts and touch cancellation

The gear now has eight identical, evenly spaced teeth and a centred hub; its SVG
master feeds the existing atlas. Entering Cancel only arms it. A coral landing
and “Release to cancel” cue stay visible until the same finger lifts there;
sliding out restores aiming without resetting charge. Both Shoot and Pass share
this behavior. OS cancellation, focus loss and menus still abort safely.

Passing fills in 0.4 seconds and uses 150% of the former horizontal launch speed
for chest, loft and bounce, with the preview calculated from the same path.
The loft height and bounce timing stay intact; distance increases with speed.

The wall follows the 15.24 × 28.6512 m painted court. After two continuous
seconds outside, solo practice gives the player the ball just outside its first
exit point. Basket supports are cleared by choosing the nearest open position
on the same line. The HUD says “Out of bounds · pass to re-enter”; Shoot, Jump,
movement and props remain unavailable until a pass physically releases. Cancel
and Settings preserve the standing restart. Team throw-in ownership remains a
future game-mode rule; rooms with multiple players keep their recovery reset.

Build the review player with `Tools/Build/Build-BasketballInboundReview.ps1`.
Run `Tools/Build/Test-BasketballInbound.ps1` for offline practice, or add
`-HostSolo` for a network host testing alone. These checks cover all four sides,
corners, both basket supports, falls, timer re-entry, stationary restrictions,
cancellation, settings, actual pass release, and 2× charge / 1.5× pass speed.
The pass regression also checks that a repeated floor callback after a planned
bounce neither adds another bounce nor loses the faster pass catch window.
