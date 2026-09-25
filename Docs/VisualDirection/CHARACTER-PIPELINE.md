# Character production plan — 23 September 2026

## Decision

Build **one canonical cartoon athlete** in an editable Blender file, with **one shared skeleton**. Treat Meshy or Tripo image-to-3D output as optional shape exploration. Any generated mesh must pass deformation, customization, and Unity import checks before becoming the master asset. Codex/Astra can help create and refine Blender geometry, materials, rig, exports, and Unity integration; it is not a one-click 3D character generator.

The three character views in `athlete-cartoon-concept.png` are outfits for the **same avatar**, not three independent characters.

## Asset structure

```text
AthleteRoot (one scale, pivot and collision capsule)
  Skeleton (one stable bone hierarchy for every sport)
  BodyAndHead (skin-tone zones, face options)
  HairSlot (interchangeable styles; color parameter)
  TopSlot (shirts, jerseys, polos; shared skeleton)
  BottomSlot (shorts and other bottoms; shared skeleton)
  ShoesSlot (interchangeable pairs; shared skeleton)
  AccessorySlots (cap, wristbands, etc.; attached to named bones)
```

- **Recolor** skin, hair, eyes and outfit zones using material parameters or masks. Keep visible cartoon color patches crisp. Avoid baking one complete color scheme into each mesh.
- **Swap** hairstyles, shirts, shorts, shoes, hats and accessories as separate parts. Every deforming clothing part must bind to the same rest pose and skeleton. Non-deforming small accessories can attach to head/hand bones.
- **Adjust face** through a modest number of designed eye, brow, nose and mouth options, with blend shapes only where smooth shape changes are truly needed. Keep the face readable from third-person distance.
- **Body shape** can have a few controlled variants later, but keep a stable animation skeleton and collision envelope initially. Test every shape against all outfits.
- **Animations** all target the canonical rig: idle, walk, run, jump/land, then football, basketball and golf actions. Validate elbows, knees, shoulders, hips, shoe contact and garment clipping before expanding the animation library.
- **Unity data** stores small appearance IDs and color values; it does not duplicate full meshes per color or send meshes over networking. A versioned appearance schema will be needed when the current four options grow.

## Current art phase

The approved Meshy 6 Lite Rainbow Sprinter is the shared avatar across all three sports. The downloaded ZIP is preserved under `ArtSource/Shared/Characters/Meshy/`, and `RainbowSprinter.blend` holds its original 189k-triangle mesh, a 100k near LOD, a 12k distant LOD, a 34-bone rig and the supplied running action. No new motion was authored in this phase. The mesh is one piece with one baked color texture, so the skin, hair, outfit and hairstyle controls are paused. The four-field `CharacterAppearance` data and network messages remain available for the future modular character. First-person view hides the whole local mesh until a separate head mesh exists.

The current artist-approved look is a foundation, not the final modular master. Its source mesh and textures are retained so clothing, hair, skin and face can be rebuilt as separate authored parts later.

The first authored animation pass adds `RainbowSprinterAnimation.blend`: an editable control rig and the eight-second playful idle, exported onto the existing skeleton. Its workflow is documented in [Idle authoring](IDLE-AUTHORING.md). The original Meshy model source and supplied run remain preserved.

## First acceptance test

Produce one neutral A-pose master in front, side, back and three-quarter views. Build one body, two hair silhouettes and two shirts on the same rig. Import it into Unity and exercise idle, run and jump while changing skin tone, hair color, hairstyle and shirt color. Inspect the result from the actual third-person and first-person cameras on desktop and Android. Only then scale up outfits, facial options and sport-specific motions.
