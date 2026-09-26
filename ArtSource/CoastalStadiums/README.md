# Coastal stadium architecture

The shared generator is `Tools/Blender/coastal_stadium_detail.py`, called by the active Sunvale football and Rally basketball generators. Regenerate both before importing the art library. The full editable architecture is saved in each venue's existing `.blend` source alongside its original modules.

The shared library exports independent architecture and simplified collision FBXs, ten 2048px base/normal/metallic-smoothness material sets, and route/scoreboard layout manifests. Architecture UVs use one tile per four metres (512 pixels/metre before mip selection). Mesh trim, thickness, joints, lantern frames, stair nosings, railing bolts and planter leaves supplement the texture detail.

Unity imports the shared library with `CoastalStadiumBuilder`, attaches it to each venue, and serializes route anchors for validation and future audience navigation. Audience NPCs are not implemented. Source coordinates use Blender Z-up; the layout importer converts to Unity `(-x,z,-y)`.

The six imagegen concepts and their prompts are in `Docs/VisualDirection/CoastalStadiums/Concepts`. They are references, not evidence of the shipping renderer. Actual player captures are generated separately by `Review-CoastalStadiums.ps1`.
