# Fishing learning art

Generated with the built-in image generator in transparent-background mode.
The existing Fishing clock and Football UI icon atlas were visual references.
These full-resolution PNG masters stay outside Unity. Runtime code shares the
sports Sonar radar and existing Fishing ReelButton rather than duplicating them.

The icon prompt requested a transparent, text-free **three-column/two-row**
atlas of glossy cartoon game icons with thick navy contours and aqua/gold
accents: shore bag; treasure chest with a float; icy spool with stopwatch;
orange sprint boot/flipper; fish in a bubble shield; quick gold lure with a
stopwatch and speed streaks. Icons occupy separate evenly padded cells.

The tutorial prompt requested a transparent, text-free **two-column/two-row**
atlas of four glossy cartoon mini-scenes: a turquoise-haired fisher choosing
two glowing gear icons; rod and leaping fish with opposite direction arrows;
shore bag/treasure and sonar/bubble effects; two friends celebrating their
fish basket beside a trophy and stopwatch. Use the same navy, cream, aqua
and gold finish as the game, with separate cells and no baked instructions.

Run `Tools/Build/Generate-FishingLearningUI.py` with Python/Pillow from any
directory. It slices each cell, mechanically downsamples, preserves alpha
and saves optimized PNGs. It does not repaint or replace the source art.
`delivery.json` uses repository-relative paths based on the repository root
and records dimensions, bytes and source/delivery SHA-256 hashes.

Delivery atlases: GearIcons **384 × 256**, TutorialPanels **512 × 256**.
The importer disables mipmaps and uses Android ASTC 6×6. Labels remain native
font rendering, independent of image resolution. Tutorial examples also
instantiate the same runtime widgets used in the live Fishing HUD.
