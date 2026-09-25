# Cartoon visual direction — exploration, 23 September 2026

These four images are **concept targets**, generated with the built-in imagegen tool. They are not screenshots, production textures, 3D assets, or promises of current mobile performance. Keep the existing three-sport prototype as the functional foundation while refining art one asset family at a time.

The football stadium now uses the editable modular [Sunvale asset](../../ArtSource/Football/Sunvale/README.md) in the playable game. It includes the new Blender source, ten module exports, a shared painted atlas, seat LODs and an isolated Unity review scene. [Bowl overview](Stadium/02_Bowl_Overview.png) and [architecture detail](Stadium/03_Architecture_Detail.png) are renders of that actual asset. Build and launch `Builds/WindowsFinal/SportsPrototype.exe` to test the latest integrated version.

| Area | Concept |
| --- | --- |
| Football | [football-cartoon-concept.png](football-cartoon-concept.png) |
| Basketball | [basketball-cartoon-concept.png](basketball-cartoon-concept.png) |
| Golf | [golf-cartoon-concept.png](golf-cartoon-concept.png) |
| Shared athlete | [athlete-cartoon-concept.png](athlete-cartoon-concept.png) |

## Style target

Build an **inhabitable animated sports world**, not a miniature copy of real stadiums or a realistic child. Quality should come from excellent silhouettes, intentional shape design, clean color composition, appealing animation, and art-directed light.

- **Shapes:** rounded, chunky, gently asymmetrical forms. Roofs, seats, hoops, rocks, trees, shoes and hair should share a soft sculpted language. Exaggerate for charm while retaining clear sport equipment and field markings.
- **Character:** one recognizable avatar across all sports: large head, compact body, short springy limbs, simple expressive face, grouped hair shapes, oversized shoes. Outfit silhouettes and colors change by sport; skin and hair customization remain important.
- **Textures:** painted broad color variation and selective graphic marks. Turf uses designed stripes and a few oversized tufts; wood uses simplified plank blocks; sand, stone, fabric and water have their own stylized marks. Avoid photographic scans, tiny noise, visible skin pores, dense grass blades, or real-world fabric weave.
- **Color:** rainbow color appears in coordinated zones, trims, banners, seating, props and outfits. Keep pitches and courts legible. Use a shared teal/cyan/coral/yellow/violet language with a different dominant color balance for each venue.
- **Lighting:** soft key light, generous colored bounce, simple readable shadows and restrained decorative glow. The render should feel like animation even in still images.
- **Readability:** at normal third-person distance, the athlete, goal/hoop/green, boundaries and action-relevant props must remain instantly recognizable. Texture detail should diminish with distance.

## Per-sport translation

| Foundation now | Art refinement target |
| --- | --- |
| Covered football stadium, regulation pitch, repeated seats | Sculpted roof and stands, graphic seat-color sections, illustrated grass stripes, stronger sky and light, animated-world signage and props. Keep pitch and goal proportions readable. |
| Indoor basketball arena with court, hoops, seating and selectable center logo | Simplified graphic court planks, rounded hoop assembly and seat forms, colorful tiered sections, warm pools of arena light. Keep court markings and customizable logo area clear. |
| Broad flat grass island with sand edge and sea | Build a whimsical course gradually: designed fairway/rough/green shapes, rounded bunkers, stylized foliage and layered water. The concept shows future holes and terrain; those are not in the current prototype. |

The concept images contain exploratory scenery beyond the current meshes, such as cliffs, waterfalls and detailed course features. Treat these as mood and shape references, not exact geometry to copy. The football reference also shows a shirt number; no particular number or logo is prescribed.

## Practical first art slice

1. Rebuild the shared athlete silhouette and one outfit in Blender; check it at actual third-person camera distance before adding detail.
2. Make a small material/look development kit: animated turf, painted court planks, graphic sand, layered water, molded seats, stylized skin/hair/fabric.
3. Apply that kit to one camera-facing slice of each venue, with a shared lighting and color setup in Unity URP.
4. Compare screenshots on the target Android device for color, readability and frame time before expanding to the full environments.

## Prompt set used for these references

All images were generated as `stylized-concept` with the built-in imagegen tool. Shared instruction: **“A HIGH QUALITY 3D CARTOON ANIMATED WORLD, deliberately far from reality,”** with expressive simplified geometry, chunky smooth silhouettes, sculpted surfaces, broad painted color variation, coordinated vivid teal/cyan/lime/yellow/coral/violet, soft light, and a playful game camera. Explicit exclusions: photorealism, real grass blades, realistic people, photographic materials, interface text, logos and watermarks.

- **Football:** third-person view of a recognizable covered football stadium; small stylized athlete from behind, center circle, far goal, graphic seat-color blocks, illustrated lawn stripes and oversized sparse tufts.
- **Basketball:** third-person view of a recognizable indoor court; rounded hoops, empty stepped seats, graphic honey-colored plank blocks, colorful arena sections, soft warm spotlights.
- **Golf:** elevated third-person view of a broad island; curved fairway and green, rounded bunkers, cream beach, layered cyan sea, sculpted palms and flowers, small mascot with a club at a tee.
- **Athlete:** three full-body views of the **same** large-headed mascot in association football, basketball and golf outfits. The football version includes a round black-and-white soccer ball, explicitly excluding American football. Simple face, grouped teal hair with coral/yellow accents, compact body, short limbs and oversized shoes.
