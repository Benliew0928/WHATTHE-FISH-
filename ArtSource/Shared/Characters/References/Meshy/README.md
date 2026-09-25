# Meshy character reference set

Generated with the built-in imagegen tool on 23 September 2026 from the approved cartoon athlete concept. These are image references, not meshes or animation data.

| File | Use |
| --- | --- |
| `athlete-front-A-pose.png` | Primary single-image input for Meshy Image to 3D |
| `athlete-front-three-quarter-A-pose.png` | Optional second single-image trial and shape review |
| `athlete-left-profile-A-pose.png` | Side-volume reference for Blender; future multi-view input |
| `athlete-back-A-pose.png` | Back/hair/shoe reference for Blender; future multi-view input |

The views are generated independently and may differ in small hair-clump details. Resolve those differences deliberately when building the canonical Blender model. Do not assume that combining them automatically produces a consistent mesh.

## Meshy workflow observed in the user's Chrome workspace

The current account showed **240 credits** at inspection time. Under **Model → Image to 3D**:

- **High Detail / Meshy 7.1** accepts a single image at Standard resolution for **20 credits** with Texture off. Texture on changes the visible estimate to **30 credits**. This is the preferred first visual reconstruction of the avatar.
- **Smart Topology / Meshy T2** accepts a single image, exposes a polygon target (4,000 by default), and showed **5 credits**. Use it as a quick geometry/deformation comparison if the High Detail shape is promising.
- The account's **Multi-view** and **A/T/Custom Pose** switches opened a Pro subscription prompt. The supplied images already show an A-pose, so generation does not depend on the Pose control.
- The form selected **CC BY 4.0** by default. The **Private** option was visible separately. Check the applicable license before using any Meshy-generated result in the released game.
- **Animate** showed preset actions including Idle, Running, Regular Jump, Kick a Soccer Ball, and Golf Drive. These are useful motion tests; the planned final cartoon actions should be reviewed and refined on the game's own rig in Blender.

Suggested first run: upload `athlete-front-A-pose.png` to **Model → Image to 3D → High Detail / Meshy 7.1**, choose Standard resolution, turn Image Enhancement off to preserve the already clean source art, leave Texture off for the initial shape test, and review the model from every angle before downloading or rigging. Do not submit all four files to Batch Images to 3D: that creates separate models, not one multi-view character.

Acceptance check: big-head silhouette and face, separate arms and legs, intact hands/shoes, plausible back and side, clear joints and garment boundaries. A good still image is insufficient if the mesh cannot bend cleanly or support swappable outfits.

## Prompt set for the image references

The front image prompt specified one full-body original high-quality 3D cartoon athlete derived from `Docs/VisualDirection/athlete-cartoon-concept.png`, centered in a symmetrical neutral A-pose on a pale seamless background. It preserved the large round head, simple teal eyes, tiny nose and smile, layered teal hair with coral/yellow locks, white-and-teal training shirt, teal shorts, socks and chunky rainbow shoes. It required visible gaps around arms and hands, distinct clothing boundaries, near-orthographic camera, soft even light, and no props, extra figures, text or photorealism. The three companion prompts rotated the same design to direct back, exact left profile and front three-quarter views while preserving proportion, colors, costume and pose.
