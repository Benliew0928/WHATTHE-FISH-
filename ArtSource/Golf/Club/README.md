# Midnight Iron handheld club

`Originals/` retains the user's Meshy FBX ZIP unchanged. The original contains
four embedded 2048-pixel maps and a 7,702-triangle mesh. `MidnightIron.blend`
keeps that editable mesh, normalized to 1.02 metres, plus three delivery LODs.
All seven active image dependencies are relative to the master; there are no
external Blender libraries. The original archive and inactive FBX exporter
SceneInfo provenance may retain historical machine paths. These are checked
with format-aware tooling and are not active delivery texture references.

From the repository root, run with Python plus bpy:

```powershell
python Tools/Blender/prepare_golf_club.py
python Tools/Blender/audit_golf_club.py
```

Pass `--root CHECKOUT` for another clone. The default root comes from the
script's location. Regeneration reads the original archive and replaces the
derived master/delivery files; keep manual master variations separately before
regenerating. Meshes, textures, masters, original ZIP and Unity `.meta` files
belong in Git/LFS; renders and audit logs belong under ignored `Builds/`.

Delivery lives in [the Unity club folder](../../../Game/Assets/_Game/Art/Golf/Club).
The near/mid/far meshes contain 3,600 / 1,400 / 500 triangles. Colour and normal
maps are 512 pixels, with a 256-pixel metallic/smoothness mask. One URP material
serves all three LODs. There are no club colliders or rigidbodies.

[GolfClubBuilder](../../../Game/Assets/_Game/Editor/GolfClubBuilder.cs) creates
the [resource prefab](../../../Game/Assets/_Game/Resources/GolfClub.prefab),
sets mobile import formats and aligns the origin to the grip. Its hand markers
are 6.8 cm apart along the handle. Change marker offsets/LOD distances in the
builder, then run **WHATTHE FISH? → Golf → Prepare handheld club**. The build
target also runs this preparation step. Length, texture sizes and LOD triangle
budgets are centralized in `prepare_golf_club.py`; refresh integrity hashes by
regenerating before running the audit.

[GolfClubMotion](../../../Game/Assets/_Game/Sports/Golf/GolfClubMotion.cs)
contains the animated-hand carry attachment, two-hand IK and the backswing/contact/
follow-through path. `ContactTime` also controls the authoritative ball impulse;
`Duration` controls swing recovery. The ball impulse remains host/offline owned.
Participants equip one reused club per avatar when a Golf round starts. Clearing
the round, changing sports, travelling and driving hide the club.

`carryDownAngle` sets the low-hand pitch (default 12°), and
`carryRaisedDownAngle` sets the raised-hand pitch (default 55°). The angle blends
with the actual hand lift during locomotion. `carryGroundClearance` (default 0.06 m) caps the pitch when necessary to keep the head above foot level. `carryWristSway` controls the small
side-angle contribution (default 0.18), bounded to 12°. Preview component values
during play, then change source defaults to retain the adjustment. Shoulder and
elbow travel follows the existing gait. Forearm pronation shares the grip
alignment so the hand can follow the sloping shaft without remaining palm-down.
Two-hand IK blends in for charging/swinging and releases afterwards.

The character's mitten vertices have no effective finger-joint weights. The
[grip importer](../../../Game/Assets/_Game/Editor/GolfGripPoseBuilder.cs) derives
the local `GolfRightGrip` blend shape on both existing character LODs and adds a
`Golf grip socket` at its opening. This leaves the source character FBX, rig,
textures and all unrelated vertices intact. Palm weights fade into the hand bone to keep the grip cavity stable under wrist rotation. The club's right marker lies on the
shaft centre, and runtime alignment keeps that axis through the actual hand
opening. The grip shape is enabled only while equipped and released for driving,
travel and round cleanup. A fresh Unity import regenerates it; the editor menu
**WHATTHE FISH? → Golf → Prepare gripping hand** explicitly reimports it.

User-approved carry references: [low hand](References/Carry-low.png) and
[raised hand with the requested shaft region](References/Carry-raised.png).
These source references remain outside Unity delivery.
