# Sports menu artwork

These are the approved energetic 2D home-menu illustrations. They are separate from the welcome scene and the official logo. The menu uses the existing [official logo](../../Game/Assets/_Game/Resources/Menu/WhatTheFishLogo.png), unchanged.

- `SportCards-master.png`: 1536 × 1024 atlas. Top left Fishing, top right Basketball, bottom left Football, bottom right Golf. Each equal quadrant is sliced into a Unity sprite at runtime.
- `Mascot-master.png`: transparent orange sports mascot, used beside the home heading. It is decorative and does not replace the brand mark.
- `generation-prompts.json`: original built-in image-generation prompts for the approved illustrations.
- `integrity.json`: SHA-256 and dimensions, with paths relative to the repository root.
- [Delivery assets](../../Game/Assets/_Game/Resources/SportyMenu): identical source pixels. The importer preserves dimensions, disables mipmaps, clamps edges, and uses ASTC 4×4 on Android. Windows keeps the source uncompressed.

Regenerate with `python Tools/Art/Sync-SportyMenu.py`; it resolves the project from its own location and works from an unrelated working directory. No external image dependencies or filesystem references are embedded in these PNGs. Preserve the high-quality masters.

The display font is unmodified Barlow Semi Condensed ExtraBold Italic from [Google Fonts](https://github.com/google/fonts/tree/main/ofl/barlowsemicondensed). Its SIL Open Font License is included in `Barlow-OFL.txt`. Body copy reuses the existing Cove text font. The font is bundled locally; the game has no online font dependency.
