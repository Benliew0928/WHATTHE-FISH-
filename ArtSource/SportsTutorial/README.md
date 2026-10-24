# Island tutorial illustrations

The three original Football, Basketball and Golf PNG illustrations were made
with the built-in image_gen tool on 24 October 2026. They are AI-generated
submission assets; disclose them together with the existing Fishing artwork.
The exact prompts and tool provenance are in [prompts.json](prompts.json).
They are decorative illustrations, not screenshots of the game characters.

The editable full-resolution PNG masters retain alpha. Derive the compact
768-by-320-or-smaller RGBA game sprites with:

```powershell
python Tools/Art/prepare_island_tutorials.py
```

Regeneration needs Python with Pillow. Normal Unity builds use the retained
delivery sprites and do not need this Python step.

| Sport | Full-resolution master | Game sprite |
| --- | --- | --- |
| Football | [Football.png](Football.png) | [Compressed sprite](../../Game/Assets/_Game/Resources/IslandTutorial/Football.png) |
| Basketball | [Basketball.png](Basketball.png) | [Compressed sprite](../../Game/Assets/_Game/Resources/IslandTutorial/Basketball.png) |
| Golf | [Golf.png](Golf.png) | [Compressed sprite](../../Game/Assets/_Game/Resources/IslandTutorial/Golf.png) |

The script resolves paths from its own location. It preserves transparency and
uses lossless PNG compression after resampling. Unity's existing sprite import
profile supplies mobile texture compression. Source and derived sizes/hashes
are recorded in [delivery.json](delivery.json). Native text, real controls and
animated read-only examples are drawn separately by the shared tutorial view.
