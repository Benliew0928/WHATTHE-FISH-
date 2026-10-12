# Football touchline UI — implementation

**Current icon/font update:** [Sports UI refresh](../SportsUI/README.md) uses polished full-colour game artwork and clean Manrope lettering in the game. This page retains the earlier layout and interaction reference.

12 October 2026. Approved reference: [prototype](proposal.html), [design specification](README.md).

The game uses the approved cream plates, navy edges, Cove fonts, action icons,
regular gear, attached power arc and connected cancellation landing. Logical
800 × 450 placements match the proposal. Controls anchor to the safe edges;
wide phones expand the settings panel and use larger guide body text.

Kick / Pressure share the main slot. Dash has its own stamina bar; Jump and
Tackle retain football eligibility and cooldowns. Support controls disappear
while charging. The minimap moves to the upper left and the score remains at
the top centre, with actual team selection, host start, kickoff, regulation,
goal reset, golden goal and results. The roster gate still requires even teams.
Permanent venue, FPS, station-distance and desktop-key labels are absent.

The original finger owns Kick until release. Dragging into Cancel only arms it;
sliding out preserves charge. Release inside the landing cancels without a
ball launch; release elsewhere kicks. Charge reaches full after one second,
holds there and never fires automatically. Focus loss, suspension, canceled
touches, menus, resizing and possession changes clear the gesture. A second
finger can continue to use the joystick. Football's existing pitch aim and
launch algorithm remain in use at every distance.

Settings contains Eye level / Follow / Stadium, Controls / Rules / Stamina / Abilities,
and a separate leave confirmation with host, guest and offline consequences.
Menus clear held input and block local commands; room simulation continues.
The Rules page uses the configured match duration rather than assuming every
room uses three minutes. The illustrated ability slot supports
[football props](../FOOTBALL-ABILITIES.md) in formal matches and Explore Offline.

## Runtime assets and regeneration

Eight approved Phosphor SVGs become one 512 × 256 atlas, with four-pixel padding,
no mipmaps, Android ASTC 6×6 and no CPU-readable copy. Gear, camera, book, movement,
jump, close and exit icons reuse the existing basketball atlas. Panels, paths,
rings, map and meters are mesh geometry. No screenshot, HTML or new font ships.
The original MIT masters and source hashes remain in [icons](icons/sources.json).

Run `node Tools/Build/Generate-FootballUI.cjs` with sharp available through
`NODE_PATH`, then Unity's `FootballUIBuild.Prepare` to apply import settings.
The generator resolves the repository from its own location. Regeneration in
a relocated directory containing spaces, invoked from an unrelated working
directory, reproduced the atlas SHA-256:
`6CD2A9D0012E9164780E848FA948DFF4E340E85DE5601503B8615E8B5C404BE6`.
The source PNG is 21,564 bytes; this is not the serialized or APK measurement.

## Verification

Use `Tools/Build/Test-FootballUI.ps1 -PlayerPath <review-player>` for actual
Unity input-module touch sequences, state checks, text-fit assertions and
rendered captures. Review evidence is under ignored `Builds/FootballUIQA/`.
The dedicated football match/ball and effort suites cover the existing rules,
physics, stamina and local host/guest behavior.

Automated touches and D3D11 captures qualify the Windows player. They do not
establish physical phone reach, touch latency, Android appearance/FPS or WAN
behavior. Android device status and final build measurements are recorded
with the completed release below.

## Delivered release

The complete Windows player is in `Builds/WindowsFinal/`; run
`Explore-Football.cmd`. Its 319 runtime/support files match the tested candidate.
The Unity build was produced at 00:21:15 Malaysia time on 12 October 2026.

Final verification: **1,705 passing assertions** across football UI (680),
match/physics/network (533), effort offline/host/guest (98), and basketball UI
regression (394). All tested text fits at 16:9, wide-phone and 4:3 resolutions.
[Detailed evidence and archives](../VERIFICATION.md#football-touchline-ui--12-october-2026).

The fresh AndroidSubmission is **89,589,994 bytes**, up **47,078** from
**89,542,916**, with **10,410,006 bytes** below the 100 MB boundary. It remains
**14,589,994 bytes** over the 75 MB development target. APK v2 verification
passes. The atlas uses 59,288 serialized / 26,739 compressed APK bytes; these
are distinct from its PNG source size. [Packing audit](../APK-SIZE-AUDIT.md#football-touchline-ui--12-october-2026).

No Android device was attached. Physical phone touch, appearance/FPS and WAN
remain unverified. Only generated Golf prefab IDs, URP resource registration
and an editor-only launcher writer differ from the Windows source snapshot;
all 201 compiled runtime source documents match. All Git publishing is left
to the user.
