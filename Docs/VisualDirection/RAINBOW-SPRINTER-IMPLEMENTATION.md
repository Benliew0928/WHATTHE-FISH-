# Rainbow Sprinter character integration — 23 September 2026

## Asset and behavior

- The Meshy 6 Lite ZIP is preserved at `ArtSource/Shared/Characters/Meshy/RainbowSprinter-source.zip` (SHA-256 `D2587ED52B4A3FAE77B32DD3F5089356B8B9A4D466F0AE573AD3ED8E3040E3B2`).
- `ArtSource/Shared/Characters/RainbowSprinter.blend` keeps the original 189,412-triangle mesh, a 99,998-triangle close LOD, a 11,998-triangle distant LOD, the 34-bone rig and Meshy's 41-frame running action.
- Unity uses `RainbowSprinter.fbx` as the static skinned model and `RainbowSprinterRun.fbx` for the supplied run. The authored eight-second `Idle_Playful` comes from `RainbowSprinterIdle.fbx` and replaces standing in the bind pose. Walking and sprinting still play the supplied run at different speeds. Root motion remains off.
- The three 2K Unity textures are albedo, normal, and packed metallic/smoothness. The character uses a dedicated URP Lit material.
- Offline and network athlete prefabs use the same model in football, basketball and golf. First-person mode hides the complete local mesh because the head is part of the same skinned mesh.
- The character menu shows one Rainbow Sprinter preview and pauses skin, hair, outfit and hairstyle controls. The four saved appearance fields and network format remain intact for later modular work.
- The superseded prototype athlete FBX, Blender file and five character-only materials are held under `Legacy/ArtSource/Legacy/PrototypeAthlete/`. Automatic approval review blocked direct deletion, so this archive keeps removal from active Unity assets reversible. Venue materials shared with football remain in place.

## Verification

- Blender re-import found two skinned LOD meshes, 34 bones and exactly one 41-frame running action. Front still and running renders were reviewed. Unity's import audit sampled the run at multiple times and confirmed stable character bounds.
- Windows player screenshots confirm a complete still character preview, the run visible in third person, a return to bind pose after stopping, and a clear first-person view. The character is visible in basketball and golf third-person screenshots. Their existing smoke checks passed.
- A local two-player room reached two connected players, entered exploration, synchronized movement, and returned to the waiting room. Appearance IDs differed in the probe while both players used the same character asset.
- Windows and Android development builds succeeded for this milestone. That earlier Android APK was **75,989,689 bytes**, below the 100 MB target; see [the current build size](../BUILD-SIZE.md).
- `adb devices` found no physical phone. Sustained Android frame rate, thermals, touch behavior and on-device visual quality remain unverified.

## Next art pass

The editable idle and reusable control rig now live in `RainbowSprinterAnimation.blend`, separate from the Meshy regeneration source. See [Idle authoring](IDLE-AUTHORING.md) for editing, preview, export, import and gameplay checks. The verification bullets above describe the earlier model integration; idle-specific evidence is recorded in the idle verification document.

The current mesh is one textured body. Hair, face, skin and outfit changes require new masks or separately authored and skinned parts. The preserved high-detail source is the reference for that work; the current lower LOD has visible simplification up close and is intended only for distance.

