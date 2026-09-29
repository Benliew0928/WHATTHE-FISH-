"""Independent terrain reference values from the original Blender authoring math.
Run with Python + numpy; does not import bpy or regenerate art.
"""
import ast, json, math
from pathlib import Path
import numpy as np

root = Path(__file__).resolve().parents[2]
ns = dict(np=np, math=math)
for filename, names in [
    ('refined_island_kit.py', {'smooth'}),
    ('build_refined_islands.py', {'golf_radius', 'fairway', 'bunker_dist', 'golf_height', 'fish_radius', 'fish_height', 'BUNKERS'})
]:
    tree = ast.parse((root/'Tools/Blender'/filename).read_text())
    for node in tree.body:
        if isinstance(node, ast.FunctionDef) and node.name in names or isinstance(node, ast.Assign) and any(isinstance(t, ast.Name) and t.id in names for t in node.targets):
            exec(compile(ast.Module(body=[node], type_ignores=[]), filename, 'exec'), ns)
samples = []
for sport, extent in [('Golf', 220), ('Fishing', 70)]:
    fn = ns['golf_height' if sport == 'Golf' else 'fish_height']
    for x in np.linspace(-extent, extent, 81):
        for z in np.linspace(-extent, extent, 81):
            samples.append(dict(sport=sport, x=float(x), z=float(z), height=float(fn(x, z))))
out = root/'Tools/Blender/Fixtures/terrain-reference.json'
out.parent.mkdir(parents=True, exist_ok=True)
out.write_text(json.dumps(dict(samples=samples), separators=(',', ':')))
print(f'Terrain reference: {len(samples)} samples')
