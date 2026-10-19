# Golf drag power artwork

Approved placement and control references are `reference-placement.png` and
`reference-detail.png`. The built-in imagegen tool produced the two transparent
layers, retained here at full resolution. Runtime copies live in
`Game/Assets/_Game/Resources/GolfUI/PowerSlider/` (from repository root).
Unity imports Rail at up to 1024 px and Knob at up to 512 px, with ASTC 4x4
for Android. PNG files are self-contained; no external image dependencies.

## Rail prompt (built-in tool, reference-detail.png as edit reference)

Create one production Unity UI sprite: the EXACT vertical power meter RAIL in this reference, isolated on true transparent background. Preserve shiny turquoise blue outer rounded frame, cream/gold interior side tick marks, green top graduating yellow middle orange red bottom, green leaf decorations at bottom. REMOVE the golf ball knob completely, REMOVE orange rays around knob, REMOVE ALL white downward chevrons, reconstruct continuous smooth colored track beneath. No grass, no background, no text, no new decorations. Straight front-on vertical symmetric meter, full object centered with tight ~3% padding, same proportions as reference rail (width including leaves about 0.53 of total height). Save image file. This is a rail only sprite; knob and animated arrows will be separate runtime UI.

## Knob prompt (built-in tool, reference-detail.png as edit reference)

Production Unity UI sprite on TRUE transparent background: isolate only the circular golf ball slider KNOB from this reference. Exactly round white silver dimpled golf ball with glossy cyan blue thick circular rim and dark navy outline, same cheerful glossy painted game art. Include three small orange yellow accent rays with white borders on EACH side of knob as in reference. No rail, no frame, no green grass, no leaves, no chevrons, no text, no other elements. Front-on circle (not oval), centered with minimal transparent padding. Whole sprite width including symmetric orange accents about 2.0 times height; circle occupies central 50% width. Save image file.

The arrows are separate native UI chevrons, so each can fade after 0.5 seconds.
No static chevrons or ball are baked into the rail.
