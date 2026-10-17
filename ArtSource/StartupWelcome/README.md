# Cable-car welcome artwork

The master preserves the user-approved [Cable-car Welcome concept](../../Docs/VisualDirection/StartupWelcome/01-cable-car-welcome.png). The illustration is a static 3D-style background; progress, text, buttons, dialogs and input are native Unity UI.

- `cable-car-welcome-master.png`: full-resolution raster master, with the presentation strip and baked controls removed. The original character, scene and logo remain.
- [Background.png](../../Game/Assets/_Game/Resources/StartupWelcome/Background.png): identical delivery source. Unity performs platform compression during import; keep the master unchanged.
- `generation-prompt.txt`: exact built-in image generation edit prompt. The reference is the approved concept above. No external image/library dependencies are embedded in either PNG.
- `integrity.json`: SHA-256 hashes and dimensions; paths use the repository root as their base.

After a deliberate art revision, copy the approved master to the delivery source, refresh the integrity record, and let `StartupWelcomeTextureImport` apply the same import settings on every clone. Preserve the character and sign placement: live controls use the artwork's 1672 × 941 coordinate frame. Review the normal, wide, tablet and simulated-cutout captures before delivery.

Generation used the built-in image tool. It was an edit of the approved concept, not a regeneration of the game scene. The master contains no active filesystem references.
