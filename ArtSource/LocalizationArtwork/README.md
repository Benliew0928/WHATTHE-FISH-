# Localized lettering backgrounds

These 16 editable high-resolution clean companions were produced with the
built-in imagegen edit tool on 25 October 2026. Original approved artwork is
unchanged. [integrity.json](integrity.json) records repository-relative source,
master and delivery paths, SHA-256 hashes and dimensions.

The game **does not substitute these complete images for the original art**.
`LocalizedArtworkSurface` samples a companion only inside reviewed lettering
regions. Original pixels supply the rest of the button, its icons and frame.
English restores the original material. Translated captions have transparent
backgrounds, matched ink/outline colors and high-resolution dynamic glyphs.
No replacement rounded panel, gray rectangle or redrawn icon is added.

Alpha-bound alignment accounts for extra transparent padding returned by image
generation. `Tools/Build/Check-LocalizationArtwork.py` validates original hashes,
identical delivery copies and alignment without modifying image pixels. Its
`--write` mode regenerates only the alignment JSON. The Unity importer limits
delivery textures to their useful display resolution, with ASTC 4x4 on Android;
the full masters remain here.

## Prompt set

Each source was supplied as the edit target in its own built-in tool call.

Leaderboard title, event badge sheet, Tornado, Crab and Warning:

> Produce a clean lettering-free companion with the same layout, aspect ratio,
> positions, proportions and composition. Remove all written words, letter
> outlines and letter shadows; remove countdown digits and their s suffix.
> Reconstruct the naturally painted glossy colored background. Do not add a
> panel, rectangle, gray fill, border or new design. Preserve every icon,
> illustration, crown, leaf, ball, clock, star and outer frame. Keep the question
> mark on the star. Preserve outside transparency. High-definition crisp art.
> No replacement text, cropping or rearrangement.

Start, Buggy, Drive, Recall, Exit, Your Ball, Finish plate, Columns, Playing,
Finished and welcome background:

> Remove written letters and their outlines/shadows, filling only erased areas
> by continuing the original painted gradients and highlights. Preserve original
> non-text icons, decorations, borders, rounded shapes and colors. No new panel,
> flat rectangle, gray patch or text. Keep the canvas layout and full objects.
> High-resolution crisp output; preserve transparency outside the UI.

The welcome exception was explicit: remove only “Ready to play”, “Sign in with
HUAWEI”, “Continue as guest”, “English”, “Settings” and “Info”; preserve the game
logo, red Huawei logo, person, globe, gear and information icons. Keep its
background opaque. The runtime uses only those six lettering regions.
