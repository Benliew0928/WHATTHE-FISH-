# Illustrated island rules

Football, Basketball, Golf and Fishing use one four-page tutorial shell.
It supplies the same cream card, safe-area scaling, page fade, entry reveal,
button feedback and Next / Back / Skip / final-page flow. Gentler motion
disables the page and example animation. Labels stay native text.

The first normal player visit to each island opens its own rules popup after
arrival/travel presentation. Closing with Skip, the last button or Back saves
that island's local seen flag. Leaving during an unread popup does not mark it
seen. Football, Basketball and Golf use separate `island.rules.<sport>.v1`
flags; Fishing retains `fishing.tutorial.round-skill.v1` for compatibility.
Settings → the sport guide and the menu's How to Play instantiate the same
content and examples. Reopening begins at page one. Embedded reference pages
never change the first-visit flag.

The popup blocks local movement, actions and camera input while visible, and
clears held inputs on opening/closing. It does not pause a multiplayer room.
The host's match, other players and shared timers continue normally. Arrival,
travel and awards take priority; an unread popup is deferred. Spectator HUDs
retain their existing spectator controls.

| Sport | Four pages |
| --- | --- |
| Football | Move/pass; aim/shoot/dash; defense/props; teams, goals and golden goal |
| Basketball | Move/pass; shoot timing and gestures; defense/props; basket values and match result |
| Golf | Find ball/flag; drag power and release; events/buggy/replay; shared random route and finish countdown |
| Fishing | Choose skill; cast/hook/countersteer; use/cooldown; scoring and round result |

The examples reuse the live `ControlDesigns` artwork, Football/Basketball prop
icons, Golf drag rail/ball handle and event badges. They have no gameplay input
handlers. Fishing keeps its existing live catch/tension/skill examples.
The separate **Tutorials** hands-on courses remain available and unchanged.

Implementation: `IslandTutorialUI` owns the common shell; `SportTutorialUI`
supplies the three sports' rules/examples; `FishingTutorialUI` supplies Fishing.
`IslandRulesReview` is an explicitly enabled development-only review of entry,
navigation, replay, seen flags, input blocking and landscape bounds.
Run `Tools/Build/Test-IslandRules.ps1` against the complete development player.
It restores the original seen flags, captures all sixteen pages with the
offscreen renderer and rejects blank captures. This is a focused UI check;
it does not validate physical touch input or WAN room behavior.

Three decorative illustrations were generated using the built-in image_gen
tool. Their retained masters, exact prompts and AI disclosure are in
[Sports tutorial art](../ArtSource/SportsTutorial/README.md). The game uses
compressed derived sprites and mobile texture import settings. Native text and
animated examples remain separate from the illustrations.
