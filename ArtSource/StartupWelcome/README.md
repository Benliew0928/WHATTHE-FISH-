# Cable-car welcome artwork

The current master is the user's approved 17 October 2026 refinement: light peach skin, clean sculpted hair and improved cable car, station, rail and park colors. It replaces the first [Cable-car Welcome concept](../../Docs/VisualDirection/StartupWelcome/01-cable-car-welcome.png) in the game.

- `cable-car-welcome-master.png`: the exact approved 1672 × 941 image, including the finished menu artwork. The character and scenery are not regenerated during integration.
- [Background.png](../../Game/Assets/_Game/Resources/StartupWelcome/Background.png): identical delivery source. Unity applies platform compression during import.
- `cable-car-welcome-blank-sign.png` and [BlankSign.png](../../Game/Assets/_Game/Resources/StartupWelcome/BlankSign.png): identical companion master/delivery pair, generated with only the sign contents erased. At runtime only the sign-interior rectangle `(1138, 340, 484, 320)` is sampled, with a six-pixel edge blend. Changes elsewhere in this companion image are never displayed.
- `approved-refinement-prompt.txt`: the approved complexion refinement prompt.
- `generation-prompt.txt`: the exact built-in imagegen prompt used for the blank sign layer. Its input is the current master. No CLI fallback was used.
- `integrity.json`: SHA-256 hashes and dimensions, using the repository root as the path base.

The illustration and ready-state labels/icons are raster artwork. Native Unity buttons supply real touch, pointer, keyboard focus and press feedback over the approved designs. The English, Settings and Info hit targets include their labels. The blank sign appears only during loading/retry, behind native status text; actual progress sits beside the character's legs. Guest-name and information dialogs remain native UI.

Future localization should replace the affected raster labels with localized assets or native labels over clean background regions. It should not modify the approved character to change UI text. No authentication is baked into this artwork; Huawei sign-in remains a placeholder.

Keep both high-quality masters. The approved scene is 1672 × 941; imagegen returned the blank companion at 1671 × 941. Its normalized UV patch maps into the same 1672 × 941 UI frame, without rewriting either master. The importer validates these dimensions, disables mipmaps, clamps sampling and uses Android ASTC 4×4. Review ready, loading, retry, normal, wide, tablet and simulated-cutout captures after changes. PNGs contain their own pixels and have no active external image/library dependencies; embedded generation provenance is not an external asset dependency.
