# Football UI proposal — Touchline

**Current icon/font update:** [Sports UI refresh](../SportsUI/README.md) uses polished full-colour game artwork and clean Manrope lettering in the game. This page retains the earlier layout and interaction reference.

11 October 2026 · Approved proposal. **Implemented in Unity on 12 October 2026.**

See [implementation and verification](implementation.md) for the delivered game,
measured build sizes and device-testing limits. This HTML remains the design
reference; its preview states are illustrative.

Open [the interactive prototype](proposal.html) through a local web server from
the repository root. It extends the [approved basketball UI](../BasketballUI/README.md)
with the game's existing Cove fonts, cream plates, navy edges, short shadows,
rounded action buttons and a regular eight-tooth settings gear.

The review toolbar is outside the game screen. Choose a possession state,
charging, cancellation, exhaustion or golden goal. Hold Kick and release;
drag the same finger into Cancel to stop. Use the gear to review Camera,
Controls, Rules, Stamina and the leave-pitch confirmation. The Host / Guest /
Offline selector changes the confirmation's consequence.

## Direction and layout

The pitch stays clear in the centre. A small map sits at the upper left, the
score and clock at the top centre, and Settings at the upper right. Court names,
station distance, FPS, desktop keys and benchmark text are absent from the HUD.
The venue's physical sign is part of the existing stadium capture.

Measurements use a logical height of 450 and a 16:9 width of 800. Side controls
remain anchored to their respective edges on wider phones.

| Component | Size | Placement at 800 × 450 |
| --- | --- | --- |
| Movement joystick | 108 × 108 | Left 28, bottom 30 |
| Kick / Pressure | 108 × 108 | Right 31, bottom 43 |
| Dash | 80 × 80 | Right 159, bottom 47 |
| Stamina | 72 × 7 | Directly below Dash |
| Jump | 66 × 72 | Right 20, bottom 169 |
| Tackle | 76 × 79 | Right 132, bottom 181 |
| Cancel landing | 102 × 61 | Right 199, bottom 181 |
| Map | 125 × 75 | Left 18, top 16 |
| Score strip | At least 304 × 56 | Top centre, inset 16 |
| Settings | 48 × 48 | Right 18, top 16 |
| Settings panel | 668 × 390 | Centred |

In a short landscape browser, the stage fills the available height beneath a
compact review toolbar. The menu expands horizontally to 960 logical units
(clamped to its safe region), with larger text and utility targets. This keeps
the guide readable instead of scaling down the desktop panel unchanged.
Production should use the actual safe area, including notches and gesture bars,
and enlarge invisible utility hit regions where necessary.

| Token | Value / source |
| --- | --- |
| Ink | `#123F50` |
| Cream | `#FFF9E9` |
| Paper | `#F4ECD5` |
| Kick / power | `#FFDA89` |
| Defense | `#BAEDF4` |
| Dash | `#A7E8CC` |
| Stamina fill | `#40B8B8` |
| Cancel | `#FFE2D5`, armed `#FFC5AE` |
| Titles and labels | Existing CoveDisplay |
| Body copy | Existing CoveText |

## Football-specific behavior

**Kick** is the existing single power-based action, not a new assisted pass or
shot system. A quick release is a light kick; holding for one second reaches
full power and holds there. There is no green timing window or automatic kick.
The power arc wraps around the original button; the release instruction stays
directly below it. The pitch aim should continue to use the real football aim
direction and charge. Distance to goal does not introduce a different meter or
a basketball finish action.

**Cancel** is a connected landing, not another tap button. Its faint path ends
at the outer edge of Kick's ring. Entering arms cancellation and changes the
cue to “Release to cancel”. Charge continues; sliding out restores the normal
release cue without restarting power. Only the original finger can finish the
gesture. Lifting inside the landing cancels. Focus loss, pointer cancellation,
resizing, role changes and menus clear held input. Other actions disappear while
charging, while the joystick remains independent.

The Unity implementation now uses this release-only cancellation behavior.

**Pressure** replaces Kick in the same main slot on defense. **Tackle** becomes
a separate tap action above-left, with its four-second cooldown printed on the
button. Pressure contains a nearby carrier; it does not automatically steal.
On a loose ball, Pressure is unavailable but Tackle remains. With a teammate
carrying, Kick stays in place but is disabled. Jump is disabled while carrying.

**Dash** keeps its own mint button and attached stamina meter. It spends stamina
only while moving, recovers after release, and requires release plus one-quarter
of the bar after exhaustion. The preview uses the existing 32-per-second drain,
15-per-second recovery and 0.6-second recovery delay. The “Out of stamina” review
state deliberately freezes at 10% so its presentation can be inspected.

The score strip keeps team letters and a YOU badge, not colour alone. The
minimap uses blue teammates, coral opponents, a white-ring local player and a
white ball. Golden goal changes the clock label and warms the score strip.

## Settings and guidance

- **Camera:** Eye level, Follow and Stadium map to existing modes 0, 1 and 2.
  A tick, outline and filled tile show selection. No extra Save step.
- **Pitch guide:** Controls, Rules, Stamina and Abilities have concise illustrated copy.
  The Rules page describes the existing three-minute regulation, one-minute
  golden goal, team selection, kickoff and boundary behavior. Explore Offline
  and formal matches include the approved
  [football abilities](../FOOTBALL-ABILITIES.md).
- **Leave pitch:** Host returns everyone to the room and ends the match. Guest
  leaves the room while others continue. Offline returns to sport selection.
  Back/Keep playing is separate from the leave action.

Menus clear held input, block the playing controls and contain focus. The
match continues behind them; this is not a multiplayer pause. Camera and guide
choices remain selected while the prototype is open. Future working settings
can use the left navigation; no inactive future rows are shown.

## Assets and implementation fidelity

The actual font files and shared basketball utility SVGs are referenced with
portable relative URLs. Football/action additions are nine unmodified
[Phosphor duotone icons](https://phosphoricons.com/), with source URLs and SHA-256
hashes in [icons/sources.json](icons/sources.json) and their [MIT license](icons/LICENSE.txt).
The ball, shoe, shield and movement motifs are paired with action words.

[assets/pitch.png](assets/pitch.png) is an unchanged 1600 × 900 game capture from
the existing football possession review, originally
`Builds/FootballMatchQA/Run-20261003-224838/control-acquired.png`. It is a static
backdrop, not a new stadium, renderer or game simulation. The camera setting,
joystick and action feedback can be reviewed, but the character, ball and 3D
camera do not move. Score, timer and map positions are illustrative.

Implementation can reuse BasketballUIStyle/CovePlate, the existing font assets,
an atlas of these exact icon paths and geometry for rings/paths. Do not ship the
HTML, stadium screenshot or a screenshot of the HUD as runtime UI. Preserve the
existing football authority, eligibility, stamina and match rules. Implementation verification includes Unity visual comparison, gameplay tests,
multi-touch input-module checks, Windows delivery and an AndroidSubmission audit.

## Review and scope

See [design QA](design-qa.md) for visual findings and browser checks. Current
captures and local review output are under ignored `Builds/FootballUIProposal/`.
Unused proposal candidates were archived with the repository tool in
`Legacy/20261011-233301-050-football-ui-proposal/`; no game assets were moved.

The proposal was originally documentation-only. The approved runtime integration
is recorded separately in [implementation.md](implementation.md); the HTML and
reference pitch screenshot remain outside Unity.

Sources: [football actions](../../Game/Assets/_Game/Shared/UI/FootballActionSlot.cs),
[Kick input](../../Game/Assets/_Game/Shared/UI/KickButton.cs),
[match rules](../FOOTBALL-MATCH.md), [pace and pressure](../FOOTBALL-PACE-AND-PRESSURE.md),
[football physics](../FOOTBALL-PROTOTYPE.md), and [sports HUD](../SPORTS-HUD.md).
