"""Read-only Sky-Sail geometry/dependency audit. Run with Blender --background.

Output is repository-relative by default; --output after Blender's -- overrides it.
No external images or linked libraries are required by these editable modules.
"""
import bpy, hashlib, json, sys
from pathlib import Path
from io_scene_fbx import parse_fbx

ROOT = Path(__file__).resolve().parents[2]
args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
output = Path(args[args.index('--output') + 1]) if '--output' in args else ROOT/'Builds/SkyRailQA/source-audit.json'
modules = ('CabinShell', 'CabinInterior', 'SlidingDoor', 'CabinHanger', 'CabinTrolley', 'CableTower', 'SailStation', 'Gangway')
records = []
for name in (*modules, 'SkySailAssetKit'):
    path = ROOT/'ArtSource/SkySail'/f'{name}.blend'
    bpy.ops.wm.open_mainfile(filepath=str(path))
    assert not bpy.data.libraries, f'{name}: unexpected external library'
    assert not [im for im in bpy.data.images if im.source == 'FILE' and not im.packed_file], f'{name}: external image'
    objects = [o for o in bpy.data.objects if o.type == 'MESH']
    assert objects, f'{name}: empty source'
    points = lambda key: [v.co for o in objects if '__'+key+'_' in o.name for v in o.data.vertices]
    if name == 'CabinInterior':
        assert max(v.z for v in points('GrabPole')) > 3.38, 'Grab poles must reach soffit'
        assert points('RailCeilingMount') and points('PoleCeilingMount'), 'Missing rail/pole attachments'
    if name == 'CabinShell':
        assert points('DoorTrack') and points('BoardingSill'), 'Missing door support/threshold'
        assert points('HandrailBracket'), 'Floating handrail'
    if name == 'CableTower':
        assert max(v.x for v in points('Leg')) < -3.7, 'Mast lies in cabin lane'
        assert min(v.z for v in points('Wheel')) > 29.54, 'Sheaves must contact cable from above'
    if name == 'SailStation':
        assert all(v.y <= -2.64 or abs(v.x) > 3.24 for v in points('Boardwalk')), 'Deck intersects berth'
    records.append(dict(path=path.relative_to(ROOT).as_posix(), sha256=hashlib.sha256(path.read_bytes()).hexdigest(), meshes=len(objects), vertices=sum(len(o.data.vertices) for o in objects), external_dependencies=0))

def inspect(element):
    if element.id in (b'RelativeFilename', b'FileName', b'Filename'):
        for value in element.props:
            if isinstance(value, bytes):
                text = value.decode('utf-8', errors='replace')
                assert not text or not (Path(text).is_absolute() or ':/' in text or ':\\' in text or text.startswith('\\\\')), f'Absolute FBX dependency: {text}'
                if text: assert (fbx.parent/text).exists(), f'Missing FBX dependency: {text}'
    for child in element.elems: inspect(child)

for name in modules:
    fbx = ROOT/'Game/Assets/_Game/Art/SkySail'/f'{name}.fbx'
    tree, version = parse_fbx.parse(str(fbx))
    inspect(tree)
    records.append(dict(path=fbx.relative_to(ROOT).as_posix(), sha256=hashlib.sha256(fbx.read_bytes()).hexdigest(), fbx_version=version))
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(dict(success=True, files=records), indent=2))
print('SKY_SAIL_SOURCE_AUDIT_PASS', len(records), 'native/FBX files; no active external dependencies')
