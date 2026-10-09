# Lagoon Stickers

[Lagoon-Stickers-Reference.png](Lagoon-Stickers-Reference.png) is the user-selected
A concept for the [fishing HUD](../FISHING-UX-DIRECTION.md), approved on
8 October 2026. It was generated with OpenAI image generation using an existing
Windows lagoon capture as visual context. It illustrates the intended look,
including animation cues, and is not a screenshot of working controls.

The runtime recreates this direction with [six reusable illustrated sprites](../../ArtSource/Fishing/UI/GENERATED-PROMPTS.md),
small UI meshes and live text/animation. This design reference stays outside
`Game/Assets/` and adds no APK texture cost; the separate production sprites
have mobile import sizes. Discarded concepts and original concept prompts
remain local review/archive artifacts under ignored `Builds/` and `Legacy/`.

The 9 October hook cue uses a gold **!** above the biting fish and animated Hook
artwork. The user simplified the original Ripple trail choice by removing its
dotted path and travelling badge. See [runtime direction and tuning](../FISHING-HOOK-CUE.md).

The subsequent readability pass uses navy score digits, a complete blue local
row, full red Cancel with X, and native Camera/Jump symbols. These procedural
elements retain the existing input frames and introduce no production texture.
Wanted now uses one line and a large yellow/navy +3, while catch awards use
plain orange text with a stronger navy outline. Full-colour score rows and
match-setup buttons replace the narrow highlights behind names. Camera and
Jump are narrower rounded controls, with a slimmer camera glyph.
