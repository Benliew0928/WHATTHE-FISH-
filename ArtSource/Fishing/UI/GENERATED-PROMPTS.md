# Lagoon Stickers production artwork

Generated with the built-in OpenAI image-generation tool on 8–9 October 2026.
Style reference: [selected concept](../../../Docs/FishingUI/Lagoon-Stickers-Reference.png).
These are six separate RGBA production assets, not a flattened gameplay HUD.
Each original is 1254 × 1254 pixels and retains real alpha transparency.
Full-resolution masters are the active source PNGs in
[Game/Assets/_Game/Resources/FishingUI](../../../Game/Assets/_Game/Resources/FishingUI).
No absolute installation paths are required. The original source art stays intact;
the importer derives 64-pixel flames, 128-pixel fish/profile/clock badges and 256-pixel frames
and reel buttons with ASTC 6×6 on Android. These six compressed textures occupy
about 84 KB before APK ZIP compression; their original PNG byte sizes are not
the shipped texture sizes.
The frame uses a reusable nine-slice border. Scores, labels and fills remain live.
The existing Cove rounded display font is reused. The same illustrated fisher
badge is shared by all rows; real names distinguish players and the local row
has a different colour. This is not a live avatar/customisation portrait.

## Exact prompts

### LagoonFrame.png

Use case: stylized-concept. Production asset: one transparent PNG of a blank layered cartoon game UI frame, square 1024x1024, intended for Unity 9-slice scaling. Image 1 is a STYLE REFERENCE ONLY: match the polished Lagoon Stickers UI in it, not the game world. Draw one clean square panel filling 86% of canvas, centered. Rounded corners radius about 12% panel width, thick dark navy/teal outer outline, cream-white raised inner rim, fine turquoise bevel, soft dark teal offset shadow. Interior is smooth cream with a subtle warm gradient, ample uninterrupted blank space. Rich glossy illustrated toy/sticker polish and softly shaded volume exactly like the reference, with crisp smooth contours. Keep the middle of every side straight and undecorated, and the centre blank, so it can be 9-sliced. No fish, splashes, characters, words, digits, symbols, background, grid or watermark. Actual transparent background outside the panel, preserve useful alpha around the shadow. The deliverable is a reusable UI asset, not a screenshot or a mockup.

### ReelButton.png

Use case: stylized-concept. Production asset: one transparent PNG round fishing action button illustration, square 1024x1024. Image 1 is only a style reference. Match its high-polish Lagoon Stickers REEL button: thick deep navy outline, layered cream-white rim, bright turquoise outer rim, glossy golden yellow face, subtle soft teal drop shadow, small turquoise water droplets attached at the lower edges. The button fills 90% of the canvas. A detailed cute toy fishing reel icon fills the UPPER HALF only: navy/cobalt circular side housings, orange wound spool, shiny pale metallic rings, turquoise handle knob, smoothly shaded highlights. Keep the LOWER HALF of the gold button clean and mostly blank for a live Unity label. Front-on, completely circular button, rich rounded toy-like illustrated volume, same coherent lighting and palette as the reference. No letters, words, digits or watermark anywhere; no game world, square card, background or UI screenshot. Actual transparent background around the entire circular button and splashes.

### FishBadge.png

Use case: stylized-concept. Production asset: one transparent PNG icon, square 1024x1024. Image 1 is a style reference only. Create the cute cyan-and-gold cartoon fish badge from the polished Lagoon Stickers HUD: a round cheerful fish looking right, golden cream face and belly, bright turquoise/blue back and fins, dark navy thick outline, big sparkling expressive eye, friendly small smiling mouth, little attached cyan water splash droplets along lower-left edge. Glossy softly shaded toy-like illustrative volume, rich clean layered highlights, crisp smooth contours, matches the exact palette and polish of the reference UI. Fish fills 85% of canvas with generous transparent margin and is fully visible, centered, horizontal. No circle background or button, no text, no numbers, no game world, no grid, no watermark. Actual alpha transparency around fish and droplets.

### Flame.png

Use case: stylized-concept. Production asset: one transparent PNG small cartoon flame accent for the Lagoon Stickers game HUD, square 1024x1024. Image 1 is a style reference only. A compact upward flickering flame with three uneven soft pointed tips, bold dark navy outer outline, rich coral-red outer fire, orange middle and luminous golden yellow inner flame. Polished glossy hand-painted sticker/game-icon volume and smooth crisp thick contour like the fire surrounding the danger gauge in the reference. One coherent flame only, centered with generous padding, fills 75% of canvas, tall about twice as high as wide. Small spark flecks may stay close to the flame. No warning sign, face, circle, frame, letters, numbers, screenshot, backdrop or watermark. True transparent alpha outside the flame.


### FisherBadge.png

Use case: stylized-concept. Production asset: one transparent PNG player profile badge illustration, square 1024x1024. Image 1 is a STYLE REFERENCE ONLY. Match the small teal-haired fisher avatar beside YOU in the Lagoon Stickers leaderboard: cheerful youthful cartoon island fisher with huge friendly brown eyes, warm peach skin, cute rounded cheeks, turquoise/teal layered wavy hair, tiny pink coral fin-like accents at the temples, white shirt neckline. Front-facing head and a little neck/shoulders, centred, fully visible. Glossy softly shaded toy/game illustration, thick clean dark navy contour, smooth detailed highlights, the same polished navy/cream/coral/teal palette as Image 1. Head fills 85% of canvas. No circular background, frame, fish, words, numbers, letters, watermark or surrounding UI. True transparent background around the head and shoulders. This is a reusable tiny UI portrait sticker, not a screenshot and not a 3D model.

### ClockBadge.png

Use case: stylized-concept. Production asset: one transparent PNG stopwatch badge for the Lagoon Stickers fishing game HUD, square 1024x1024. Image 1 is a STYLE REFERENCE ONLY. Match the charming stopwatch icon at the left of the top-centre timer in it: a chunky glossy orange-gold round stopwatch with small top push button, deep navy outer contour, creamy white clock face, rounded deep navy hour and minute hands, tiny simple navy tick marks, turquoise-blue raised outer accent and subtle toy-like shiny highlight. Tilt the stopwatch slightly left for lively sticker character; fully visible and centred, occupying 85% of the canvas. Crisp thick smooth outlines and high-quality softly shaded playful game UI illustration, same cream/navy/coral/gold/teal palette and finish as Image 1. No letters, words, digits, extra props, fish, timer panel, full HUD, watermark or coloured background. Real transparent alpha around the whole stopwatch. This is a small standalone 2D game icon, not a 3D model or screenshot.
