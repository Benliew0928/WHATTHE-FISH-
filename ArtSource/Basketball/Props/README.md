# Courtside props

`BasketballProps.blend` contains the editable original meshes, shared flat-colour
materials and icon lighting. There are no external image or library dependencies.
The approved concept sheet is `Approved-concept.png`; it is a design reference
and is not shipped. The three `*-icon-master.png` images retain full resolution.

Regenerate with Blender:

```text
blender --background --python Tools/Blender/build_basketball_props.py
```

The generator resolves the repository from its own location, or accepts
`-- --root <checkout>`. It writes delivery FBXs and matching PNGs to
`Game/Assets/_Game/Resources/BasketballProps/`. FBXs use relative path mode and
plain materials with no image references. `integrity.json` records paths
relative to the repository root, hashes, geometry counts and dependency status.

In Unity, **WHATTHE FISH? > Basketball > Prepare props** remaps shared URP
materials, disables animation/readability on the models, and configures 256-pixel
ASTC 6x6 Android icons without mipmaps. The checked-in delivery assets and metadata
are sufficient for a fresh-clone build; Blender is only needed for editing or
regeneration. The runtime handles bobbing, rotation, halos and portal effects.

Do not ship the master Blender scene or full-resolution review images.
