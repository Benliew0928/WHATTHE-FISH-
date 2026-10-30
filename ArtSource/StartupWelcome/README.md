# Cable-car welcome artwork

The current game uses the [responsive layered welcome](Responsive/README.md):
sharp scenery, a separate transparent mascot, the original official logo and
native localized controls. Its active textures are `Scene.png` and `Mascot.png`
under `Game/Assets/_Game/Resources/StartupWelcome/`.

The two complete illustrations below remain editable reference masters. Their
superseded Resources copies and the unused localized full-background copy were
archived during the [30 October optimization](../../Docs/FINAL-OPTIMIZATION-2026-10-30.md).
They have no remaining serialized or runtime load references and are excluded
from the delivered game. `integrity.json` now records the retained masters.

The current master is the user's approved 17 October 2026 refinement: light peach skin, clean sculpted hair and improved cable car, station, rail and park colors. It replaces the first [Cable-car Welcome concept](../../Docs/VisualDirection/StartupWelcome/01-cable-car-welcome.png) in the game.

- `cable-car-welcome-master.png`: the exact approved 1672 × 941 image, including the finished menu artwork. The character and scenery are not regenerated during integration.
- `cable-car-welcome-blank-sign.png`: the retained companion master, generated with only the sign contents erased. The former welcome sampled its sign-interior rectangle `(1138, 340, 484, 320)` with a six-pixel edge blend.
- `approved-refinement-prompt.txt`: the approved complexion refinement prompt.
- `generation-prompt.txt`: the exact built-in imagegen prompt used for the blank sign layer. Its input is the current master. No CLI fallback was used.
- `integrity.json`: SHA-256 hashes and dimensions, using the repository root as the path base.

In the former composition, illustration and ready-state labels/icons were raster artwork. Native Unity buttons supplied touch, pointer, keyboard focus and press feedback over those designs. The blank sign appeared during loading/retry. The current layered composition keeps its controls and localized text independent of the scenery.

No authentication is baked into this artwork. Current sign-in behavior is documented in the [account guide](../../Docs/PLAYER-LOGIN.md).

Keep both high-quality masters. The approved scene is 1672 × 941; imagegen returned the blank companion at 1671 × 941. Neither master was rewritten by cleanup. PNGs contain their own pixels and have no active external image/library dependencies; embedded generation provenance is not an external asset dependency.
