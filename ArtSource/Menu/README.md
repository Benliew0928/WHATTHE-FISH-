# Storybook Cove assets

The user selected the first refined direction, Storybook Cove. Approved boards:
[home](../../Docs/VisualDirection/Menu/StorybookCove/01-storybook-cove-home.png),
[rooms](../../Docs/VisualDirection/Menu/StorybookCove/01-storybook-cove-rooms.png),
[details](../../Docs/VisualDirection/Menu/StorybookCove/01-storybook-cove-details.png).

## Island illustration

The built-in ImageGen tool generated a transparent four-island atlas from the
approved home board as a style/subject reference on 5 October 2026. The full
1254 × 1254 RGBA source is retained as
[CoveIslands.png](../../Game/Assets/_Game/Resources/Menu/CoveIslands.png).
Unity derives the mobile texture without overwriting the source. The original
brand logo is unchanged; its menu import is size-limited separately.

The prompt is retained in [island-prompt.txt](island-prompt.txt). The generated
sheet has no UI labels; layout, text, navigation and interaction are native code.

## Fonts

Source: [Google Fonts Nunito](https://github.com/google/fonts/tree/main/ofl/nunito),
downloaded from the official repository on 5 October 2026. The full variable
master is [Fonts/Nunito-Source.ttf](Fonts/Nunito-Source.ttf), with its
[OFL license](Fonts/OFL.txt).

`Tools/Build/Prepare-CoveFonts.py` uses fontTools to instantiate weights 600 and
900, retain the supported Latin and UI punctuation glyphs, and name the derived
fonts **Cove Rounded Text** and **Cove Rounded Display**. This avoids retaining
the source's reserved family name on a modified font. Both delivery files and
the license are in `Game/Assets/_Game/Resources/Menu/`; a clone does not need
Python to build. Run the generator only when changing font coverage or weights.

No active asset reference depends on a local user directory or generated-image
cache. PNG and TTF sources use Git LFS.
