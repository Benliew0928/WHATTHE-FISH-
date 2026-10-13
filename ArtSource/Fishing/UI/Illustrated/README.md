# Illustrated fight controls

These PNGs are full-quality editable image masters outside Unity. Built-in
image generation used the approved [B mockup](../../../../Docs/FishingUI/Hold-Countersteer-Reference.png)
as a style reference. No CLI/API fallback was used.

- `SteerIdle.png`: cream-bevelled aqua round Left control, large navy arrow,
  blank lower area for native Left/Hold text, genuine transparent padding.
- `SteerLit.png`: the same silhouette in gold/orange with restrained glow.
- `CounterFrame.png`: blank wide cream/navy/cyan frame with repeatable edges.
- The Cast/Hook reel master remains in `../Masters/ReelButton.png`.

Runtime uses one Left and one Right; the Right artwork is horizontally
reversed. Aqua and gold are two states of each button. Native text remains
inside each circle, independent of the artwork reversal and compression.

Run `python Tools/Build/Generate-FishingIllustratedUI.py` from any working
directory, supplying the script path as needed. It requires Pillow and resolves
the repository root from its own location. It derives 192-pixel button sprites
and a 512×128 sliced frame, preserves transparency, and records source/delivery
SHA-256 hashes in `delivery.json`. Unity imports Android sprites as ASTC 6×6,
without mipmaps; native labels do not become low-resolution bitmap text.
The build preflight reimports these four files and verifies their Sprite/ASTC
settings before collecting Windows or Android content, including fresh clones
that initially import textures before compiling the postprocessor.

## Prompt set

**Idle:** Create one isolated round Left fishing steering button on transparent
background, matching the B mockup. Deep navy outline/lower shadow, thick creamy
bevel, glossy aqua centre, chunky navy left chevron in upper half, lower third
blank for native text. Orthographic circle, fixed silhouette, transparent padding;
no letters, numbers, scene or watermark. Preserve detail at mobile size.

**Lit edit:** Preserve the idle button silhouette, arrow, bevel, shadow and
padding exactly. Change only the cyan centre to luminous gold/yellow with orange
shading and white-gold highlights, restrained glow contained in the canvas. Keep
the blank text area and real transparency; add no text or extra objects.

**Frame:** Create one isolated blank wide rounded cream countersteer panel,
deep navy border/lower shadow, cyan inset rim, tiny gold corner glints and subtle
bevel. Spacious blank interior with straight repeatable edges for slicing. Match
the mockup's illustrated depth; no text, icons, fish, bars, grid or scene.
