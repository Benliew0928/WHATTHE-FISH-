"""Preserve the supplied football FBX and audit its portable dependencies.

Run with Python + bpy, or Blender --background --python this-file -- --input FBX.
The source mesh and materials are retained without decimation or recolouring.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import sys

import bpy
from io_scene_fbx import parse_fbx


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", type=Path)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else sys.argv[1:])
    root = Path(__file__).resolve().parents[2]
    delivery = root / "Game/Assets/_Game/Art/Football/Ball/soccer-glossy-unity.fbx"
    master = root / "ArtSource/Football/Ball/SoccerGlossy.blend"
    evidence = root / "Builds/SoccerBall/source-audit.json"
    delivery.parent.mkdir(parents=True, exist_ok=True)
    master.parent.mkdir(parents=True, exist_ok=True)
    evidence.parent.mkdir(parents=True, exist_ok=True)
    if args.input and args.input.resolve() != delivery.resolve():
        if delivery.exists():
            raise FileExistsError("Delivery already exists; inspect before replacing it.")
        shutil.copy2(args.input, delivery)

    tree, version = parse_fbx.parse(str(delivery))
    references = []

    def inspect(node, parent=b""):
        if parent in (b"Texture", b"Video") and node.id in (b"FileName", b"Filename", b"RelativeFilename"):
            references.extend(v.decode("utf-8", errors="replace").rstrip("\0") for v in node.props if isinstance(v, bytes) and v)
        for child in node.elems:
            inspect(child, node.id)

    inspect(tree)
    # This football has two solid pigments and needs no external images.
    if references:
        raise ValueError("Unexpected active FBX image references: " + repr(references))
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(delivery), use_custom_normals=True, use_anim=False)
    points = []
    triangles = 0
    materials = {}
    for obj in bpy.context.scene.objects:
        if obj.type != "MESH":
            continue
        obj.data.calc_loop_triangles()
        triangles += len(obj.data.loop_triangles)
        points.extend(tuple(obj.matrix_world @ v.co) for v in obj.data.vertices)
        for mat in obj.data.materials:
            node = next(n for n in mat.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
            materials[mat.name] = {"linear_color": list(node.inputs["Base Color"].default_value),
                                   "roughness": float(node.inputs["Roughness"].default_value)}
    minimum = [min(p[i] for p in points) for i in range(3)]
    maximum = [max(p[i] for p in points) for i in range(3)]
    dimensions = [b - a for a, b in zip(minimum, maximum)]
    assert triangles == 46080, triangles
    assert all(.219 < d < .221 for d in dimensions), dimensions
    assert len(materials) == 2 and not bpy.data.images
    bpy.context.scene.unit_settings.system = "METRIC"
    bpy.context.scene.unit_settings.scale_length = 1
    bpy.context.preferences.filepaths.save_version = 0
    bpy.ops.wm.save_as_mainfile(filepath=str(master))
    report = {"delivery": delivery.relative_to(root).as_posix(), "master": master.relative_to(root).as_posix(),
              "sha256": hashlib.sha256(delivery.read_bytes()).hexdigest(), "fbx_version": version,
              "triangles": triangles, "dimensions_m": dimensions, "materials": materials,
              "active_image_references": references, "blend_images": len(bpy.data.images),
              "blend_libraries": len(bpy.data.libraries)}
    evidence.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report))


if __name__ == "__main__":
    main()
