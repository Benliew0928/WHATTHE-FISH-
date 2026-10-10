# Golf HUD illustration sources

The user approved [option A](../../../Docs/GolfUI/Approved-Reference.png),
with option C's scorecard divider lines and option B's hold/release cue.
These two transparent illustrations provide the illustrated A style;
table separators, the cue, labels, shot power and button state remain native UI.
No words or percentages are baked into the art.

`GolfShotBadge-master.png` and `GolfCourseBadge-master.png` are the unmodified,
full-quality built-in image-generation results. The precise prompt set is in
[GENERATED-PROMPTS.md](GENERATED-PROMPTS.md). Keep masters outside Unity's
`Assets` tree, so the high-resolution source PNGs do not ship in the game.

Regenerate both delivery images with a Python installation that includes Pillow:

```sh
python Tools/Art/prepare_golf_ui.py
```

The script resolves the repository from its own location, retains aspect ratio,
resizes in premultiplied alpha with Lanczos, pads transparent edges and saves
optimized PNGs. It can run from an unrelated working directory. It also refreshes
[delivery-audit.json](delivery-audit.json), which records dimensions, transparency,
byte sizes and SHA-256 hashes. Delivery assets are checked in for a fresh clone.

| Illustration | Editable master | Unity delivery | Delivery PNG bytes |
| --- | --- | --- | ---: |
| Golf shot | 1254 × 1254 RGBA | 256 × 256 RGBA | 91,848 |
| Tropical course | 1476 × 1066 RGBA | 128 × 128 RGBA | 18,679 |

Both runtime images live in
[`Game/Assets/_Game/Resources/GolfUI`](../../../Game/Assets/_Game/Resources/GolfUI).
[`GolfUIAssetImporter.cs`](../../../Game/Assets/_Game/Editor/GolfUIAssetImporter.cs)
keeps them as full-rectangle sprites with no mipmaps, no CPU readability, clamp
wrapping and bilinear filtering. Standalone uses RGBA32 at delivery resolution;
Android uses ASTC 6×6. The two Android images together have **37,328 raw texture
bytes**; this is a different measurement from APK growth. Only a fresh measured
AndroidSubmission build validates the final APK budget.

The illustration PNGs have no external image, library or texture dependencies.
The image-generation source files and approved preview remain covered by the
repository's existing PNG Git LFS rule. No new font, shader, mesh or package is
introduced by these illustrations.
