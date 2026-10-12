# Football ability artwork

The approved concept is retained in [Approved-concept.png](Approved-concept.png)
with its generation record in [concept-prompts.json](concept-prompts.json).
It is a design reference, not a texture shipped in the game.

The actual editable 3D masters are the deterministic geometry in
[FootballPropBuild.cs](../../../Game/Assets/_Game/Editor/FootballPropBuild.cs).
The generator creates a mint captain's armband with gold chevrons, a violet
hexagonal portal with a miniature football, and an orange barrier projector.
The deployed barrier is 6 m wide, 5.25 m high and 0.3 m deep. Shared coloured
materials and an animated cyan panel shader give these their final appearance.
There are no external model, image, font, library or FBX texture dependencies.

[Turbo.svg](Turbo.svg), [Portal.svg](Portal.svg) and [Wall.svg](Wall.svg) are the
original, editable 2D button illustrations. They deliberately use football
silhouettes and colours distinct from the basketball props.

From any working directory, run `node <repository>/Tools/Build/Generate-FootballProps.cjs`.
The script resolves the repository from its own location. It needs the `sharp`
Node package through ordinary module resolution or the developer's `NODE_PATH`.
It produces the committed 512 × 128 transparent icon atlas in
`Game/Assets/_Game/Resources/FootballProps/Icons.png` and refreshes
[atlas.json](atlas.json), whose paths are relative to the repository root.
No machine-specific dependency path is stored in source.

`FootballPropBuild.Prepare` runs from the normal Unity build entry point. It
preserves asset GUIDs while regenerating the four combined meshes, prefabs,
materials and mobile atlas import settings. The generated delivery assets and
their Unity metadata belong in Git so a clone can run immediately. Meshes share
materials; the atlas uses ASTC 6×6 on Android without mipmaps. The runtime pools
the three pickup models, two team walls, placement preview, hologram digits
and portal planes instead of allocating them for every spawn.

Spawn-only presentation uses 4× scale and an opaque dark-blue countdown material.
Prefab geometry, HUD icons and deployed effect dimensions retain their original
sizes. The deployed wall has a separately pooled solid world collider; previews
and pickup meshes remain non-solid. No extra texture or mesh is needed for this
display-size change.
