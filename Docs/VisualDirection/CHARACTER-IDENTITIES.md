# Character identity delivery notes

The ten-player collection uses distinct heads, permanent room numbers and
personal accents, with Blue/Red uniforms only in football and basketball.
Release measurements are recorded in [build size](../BUILD-SIZE.md), with
test evidence and device limits in [verification](../VERIFICATION.md).
Local captures and raw reports are under `Builds/CharacterIdentityQA/` (ignored).

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
legacy profiles once; protocol 57 requires matching multiplayer builds.

Football and basketball resolve uniforms from the active match's actual team:
A is Blue and B is Red. Personal skin, head silhouette and shoe accents remain.
Shared front/back numbers and compact depth-tested number badges identify
players at gameplay distance. The local badge adds YOU and a white outline.
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
