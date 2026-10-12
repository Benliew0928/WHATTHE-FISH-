# Sports icons and typography

The current basketball and football HUDs use **full-colour cartoon artwork**:
orange basketballs, blue-and-ivory footballs, turquoise player figures, warm skin tones
and gold-trimmed shields. Clean silhouettes, consistent shading and highlights
make the controls feel like game items. **Manrope** remains at weight 800 for
action labels and headings, 600 for instructions, status and guide copy.
Cream faces, navy edges and pastel button colours retain the island palette.

## Design and implementation

- Ten illustrated masters cover the balls, player poses, pass arrow,
  defensive hands and shields. Tackle uses a simple 2D cartoon player sliding
  on one hip with an extended leg reaching the football. Dash uses the same
  flat 2D figure in a forward sprint with golden acceleration streaks.
  Broad colour blocks and simple faces keep all three poses legible at small
  sizes. Jump shows a person leaping upward,
  with golden chevrons and a turquoise/orange lift trail; it is shared by both
  sports. These figures have no realistic character detail.
  Action names stay visible beside the artwork. The wide Tackle sprite omits
  transparent rows so the full sliding pose fits clearly above its label.
- Unavailable actions retain their original colour with a softer alpha fade.
  Their button face and status text communicate availability without turning
  the illustration grey.
- Regular utility shapes use restrained colour, depth and highlights: a blue
  and gold gear, blue guidebook, coral exit and coloured camera choices.
  Gold Lock-on and Boost accents and a purple Portal also appear in the guides.
  The selected-camera check remains a white mask for contrast.
- Labels use a geometric sans with a clear distinction between headings and
  instructions. Fonts are rasterized at full canvas density, and both sports'
  overhead/offscreen indicators use the same text family.
- Basketball settings stay at their native scale at 1600 × 900. Volume knobs
  are circular and contained inside their rails, with clear space below labels.
- Button positions, sizes, colours, gesture paths, release-only cancellation,
  shot timing and gameplay rules retain their previous behavior.
- The existing 128-pixel-cell atlases keep their dimensions: basketball
  512 × 512, football 512 × 256, shared props/check 512 × 128. Full-colour props
  render without an additional colour tint. Three-dimensional pickups keep
  their existing art. High-resolution masters are outside Unity and do not ship.

## Editable sources and regeneration

[Painted PNG masters](../../ArtSource/SportsUI/Painted/) were generated using
the built-in image-generation tool. The complete [prompt set](../../ArtSource/SportsUI/Painted/prompts.json)
records the base style, each current subject and retained refinements; the
[master manifest](../../ArtSource/SportsUI/Painted/masters.json) records sizes,
transparency and integrity hashes. Keep these editable delivery sources in LFS.

[Utility sources and hashes](sources.json) record the retained SVG geometry.
The paths are from [Google Material Symbols](https://github.com/google/material-design-icons)
under [Apache 2.0](LICENSE-ICONS.txt). The utility generator adds colour gradients,
contours, shallow depth and small accents, saving editable SVGs in
[color-icons](color-icons/). Superseded monochrome gameplay glyphs have been
archived locally; older proposal artwork remains with its original proposals.
The retained [font master](../../ArtSource/SportsUI/Fonts/Manrope-Source.ttf) is
from [Google Fonts' Manrope family](https://github.com/google/fonts/tree/main/ofl/manrope),
under its [OFL license](../../ArtSource/SportsUI/Fonts/OFL.txt).
Derived subsets use the family name **Cove Sport**. No system font installation
or network access is needed to regenerate the committed delivery assets.
Both licenses also ship in
[StreamingAssets](../../Game/Assets/StreamingAssets/ThirdParty-SportsUI.txt).

Run with Python + fontTools and Node.js + sharp (dependencies outside Git):

```powershell
python Tools/Build/Prepare-SportsFonts.py
python Tools/Build/Generate-SportsUtilityIcons.py
node Tools/Build/Generate-SportsUI.cjs
```

The generators resolve the project from their script locations, including
when invoked from an unrelated working directory. Node can resolve sharp
through the caller's `NODE_PATH`. Unity's `SportsUIBuild.Prepare` applies
bilinear, no-mipmap, ASTC 6×6 Android import settings. The font outputs total
68,472 bytes before APK compression; the full editable master stays outside
the Unity project.

## Validation

Current evidence and release measurements are recorded in
[verification](../VERIFICATION.md), [build size](../BUILD-SIZE.md) and
[the APK audit](../APK-SIZE-AUDIT.md). Local captures and the tested candidate
are under ignored `Builds/ColorIconQA/`. Windows captures validate layout;
they do not qualify physical phone rendering, touch latency or frame rate.
