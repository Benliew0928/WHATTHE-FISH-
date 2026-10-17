# Character identity delivery notes

The ten-player collection uses distinct heads, permanent room numbers and
personal accents, with Blue/Red uniforms only in football and basketball.
Release measurements are recorded in [build size](../BUILD-SIZE.md), with
test evidence and device limits in [verification](../VERIFICATION.md).
Local captures and raw reports are under `Builds/CharacterIdentityQA/` (ignored).

## Head refinement

`CharacterHeadMesh` is the editable source for the replacement heads. The shared
scalp overlaps the jaw, neck and both ear roots; color alone no longer decides
whether the original ear geometry survives. A small texture-aware boundary mask
removes the fused model's colored hair strands at the temples in the forward,
shadow and depth passes. Skin normalization uses linear color consistently, so
the scalp and original face use the same palette.

The crop has swept, tapered locks; curls use smaller overlapping clusters; the
flat top has a level crown; the crest uses curved fins. The ponytail tapers from
its tie, the bun and twin puffs have sculpted grooves, and both hats have shaped
brims and trim. Shared vertex colors supply the details without new textures.
Both geometry LODs preserve the silhouettes and joins. The original swept quiff,
body topology, rig, grip shapes and animation clips remain shared and unchanged.
Buried triangles inside overlapping rigid pieces are removed before delivery;
visible surfaces retain their detail. Generated head meshes use medium mesh
compression. The ear mask shares the otherwise unused negative jersey-facing
value on head vertices, avoiding an extra coordinate across the whole body.
Unused head number coordinates are canonicalized for compression; fringe
triangles retain their height data. This removes packed entropy without changing
body geometry, UV0, skinning or animation.

`CharacterRefinementReview.Preview` renders all ten identities from front,
quarter, side and back at both LODs, and checks finite mesh data and scalp normals.
It explicitly selects MobileURP for consistent contact-sheet lighting; desktop
gameplay cameras are verified separately in the built player.
`CharacterRefinementReview.Build` creates a complete Windows player under
`Builds/CharacterRefinementQA/Player/`. Captures and raw evidence belong under
`Builds/CharacterRefinementQA/`, outside the shipped game.

## Asset and animation layout

The ten looks share the original Rainbow Sprinter body, both body LODs, the
34-bone rig, nine imported animation clips, controller, textures and procedural
sports motion. The original FBX and editable art master remain unchanged.
`CharacterIdentityImport` derives clothing, skin, accent and replaceable-hair
regions plus jersey-number coordinates during model import. It retains the
`GolfRightGrip` deformation on both LODs. The identity shader removes the original
hair surface for the nine attachment presets.

`CharacterIdentityBuild.Prepare` generates nine pairs of head attachments,
two shared scalp LOD meshes, a 320 x 64 single-channel digit atlas, two shared material templates and
an identity library under `Game/Assets/_Game/Art/CharacterIdentity/`. These are
active delivery assets and should be kept with their `.meta` files. The C# mesh
generator is their editable source. The normal build calls Prepare, so a clone
can regenerate them without an external art application. Generated meshes and
the atlas have no external file dependencies; material dependencies use Unity
asset GUIDs.

The game creates a small cached material per identity/team combination. It
reuses texture objects, and each player retains independent animation playback.
Rigid hair follows the existing head bone, uses the body's LOD group and is
included in first-person hiding. The implementation does not create nine body
models, texture sets or animation libraries. There are no character sprite-frame
sheets. The only new texture is the shared ten-digit atlas.

## Identity and team rules

A host-owned lease registry allocates room numbers 01-10 to humans and bots.
The initial owner preference is honored if available; collisions use a free
identity. Leaving releases a reservation without renumbering the remaining
players. Allocation is independent of client ownership, teams and roster order.
Offline bots use the same lease algorithm. The saved appearance schema migrates
legacy profiles once. Matching multiplayer builds are required; the current
combined protocol is recorded in each player's `LATEST-BUILD.txt`.

Football and basketball resolve uniforms from the active match's actual team:
A is Blue and B is Red. Personal skin, head silhouette and shoe accents remain.
Shared front/back numbers and compact depth-tested identity badges identify
players at gameplay distance. Each badge uses a small Blue/Red number tab beside
the player's name on a warm neutral plate, so team recognition stays immediate
without filling the whole label with saturated colour. The local badge adds a
small gold YOU chip; spectators use a quiet SPEC chip. Badge width follows the
actual rendered name width, including the saved 16-character name limit, and
scales down for the compact viewport. The name has its own padding before the
role chip, including for wide letters and maximum-length names.
Badges avoid the team scoreboards and separate crowded labels; football stamina
bars sit immediately below their matching badge. Basketball shot-clock and team
status stay inside its scoreboard. One/two chest bands provide another team cue.
Team selection, score labels, replay labels and minimap colors use the same
mapping. A cancelled/cleared match, menu or travel removes the uniform override;
golf and fishing use personal colors.

## Verification entry points

- `CharacterIdentityBuild.BuildReview`: complete task-specific Windows player.
- `Tools/Build/Test-CharacterIdentity.ps1`: ten-player offline and host/guest
  football and basketball checks, with rendered lineup and gameplay captures.
  `-Width 844 -Height 390` exercises the compact layout; `-PlayerPath` selects
  another complete player. Defaults resolve relative to the script's repository.
- Existing turn, jump, tackle, football/basketball motion, golf and fishing
  suites provide animation/contact regressions.
- `Tools/Build/Build.ps1 -Target AndroidSubmission`: actual release APK size gate.

Windows captures do not establish physical Android visual quality or frame rate.

## Delivery choices

The final personal detail layer uses shoe accents and coordinated skin/hair
palettes; the plan's wrist/sock patterns were illustrative options. Recognition
comes from the ten head silhouettes and unique numbers, including under shared
uniform colors. There are no per-player jersey textures or new animation clips.
Number coordinates are quantized at import to sub-millimetre precision to improve
bundle compression. Body positions, skinning and animation data are unchanged.

The original FBX and authoring master were not edited. New generated mesh/texture
assets have no external dependencies. Existing character textures are referenced
by GUID, and both character prefabs reference one shared identity library; placing
another copy under Resources would duplicate shipped texture dependencies.
