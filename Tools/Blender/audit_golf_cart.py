"""Format-aware dependency check for the retained golf-cart master and FBX."""
import argparse
import json
import math
import re
import zipfile
from pathlib import Path

import bpy
import bmesh
from io_scene_fbx import parse_fbx
from golf_cart_wheels import RIM_RADIUS, SEGMENTS, TREAD_ROWS, TREAD_DEPTH

parser = argparse.ArgumentParser(); parser.add_argument('--root', type=Path)
root = (parser.parse_args().root or Path(__file__).resolve().parents[2]).resolve()
source = root / 'ArtSource/Golf/Cart'
output = root / 'Builds/GolfCartQA/PortableAudit'; output.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(source / 'GolfCart.blend'))
images = []
for image in bpy.data.images:
    if image.packed_file: continue
    assert image.filepath.startswith('//'), image.name
    target = Path(bpy.path.abspath(image.filepath)).resolve()
    assert target.is_file() and target.is_relative_to(root), image.name
    images.append({'image': image.name, 'relative_path': target.relative_to(root).as_posix()})
assert not bpy.data.libraries
master=bpy.data.objects['GolfCart_MeshyMaster']
assert len(master.data.polygons)==246850
delivery=[o for o in bpy.data.objects if o.type=='MESH' and o!=master]
assert len(delivery)==15
delivery_report=json.loads((source/'delivery-audit.json').read_text())
lod_triangles=[lod['triangles'] for lod in delivery_report['lods']]
wheel_caps=[]; wheel_circles=[]; wheel_rims=[]; axle_connections=[]
for level,triangles in enumerate(lod_triangles):
    parts=[o for o in delivery if o.name.endswith('LOD'+str(level))]
    assert len(parts)==5 and sum(len(o.data.polygons) for o in parts)==triangles
    assert sum(o.name.startswith('GolfCart_Wheel_') for o in parts)==4
    body=next(o for o in parts if o.name.startswith('GolfCart_Body_'))
    for wheel in (o for o in parts if o.name.startswith('GolfCart_Wheel_')):
        sign=-1 if wheel.name.split('_')[2].endswith('L') else 1
        for x in (sign*.30,wheel.location.x+sign*.115):
            ring=[v for v in body.data.vertices if abs(v.co.x-x)<1e-5 and
                  abs(math.hypot(v.co.y-wheel.location.y,v.co.z-wheel.location.z)-.065)<1e-5]
            assert len(ring)==(10,8,6)[level], 'Missing chassis/axle connection: '+wheel.name
        axle_connections.append(dict(wheel=wheel.name,inner_anchor=sign*.30,hub_end=wheel.location.x+sign*.115,radius=.065))
    for wheel in (o for o in parts if o.name.startswith('GolfCart_Wheel_')):
        sign=-1 if wheel.name.split('_')[2].endswith('L') else 1
        cap=[f for f in wheel.data.polygons if f.center.x*sign>.174 and f.center.y**2+f.center.z**2<(RIM_RADIUS-.002)**2]
        outward=sum(f.normal.x*sign for f in cap)/len(cap)
        assert outward>.3, 'Inverted wheel hub: '+wheel.name
        wheel_caps.append(dict(name=wheel.name,outward_normal_mean=outward))
        radius=.339 if wheel.name.split('_')[2].startswith('F') else .328
        radial=[math.hypot(v.co.y,v.co.z) for v in wheel.data.vertices]
        assert max(radial)<=radius+.00001, 'Fragment outside tyre: '+wheel.name
        # The tread has intentional recessed channels. Every block's envelope
        # must still have the same radius; a groove must not turn the whole tyre
        # or the round rim into an oval or displace it from its rolling pivot.
        segments=SEGMENTS[level]
        for x in ((-.034,.006,.054,.094) if level==0 else (-.034,.094)):
            ring=[v for v in wheel.data.vertices if abs(v.co.x*sign-x)<1e-6]
            assert len(ring)==segments, 'Incomplete tyre tread ring: '+wheel.name
            heights=[math.hypot(v.co.y,v.co.z) for v in ring]
            assert sum(abs(h-radius)<1e-6 for h in heights)==segments*3//4
            assert sum(abs(h-(radius-TREAD_DEPTH))<1e-6 for h in heights)==TREAD_ROWS[level]
            assert math.hypot(sum(v.co.y for v in ring)/segments,sum(v.co.z for v in ring)/segments)<1e-6
            floors=sorted(math.atan2(v.co.z,v.co.y)%(2*math.pi) for v,h in zip(ring,heights) if h<radius-.001)
            gaps=[(floors[(i+1)%len(floors)]-floors[i])%(2*math.pi) for i in range(len(floors))]
            assert max(gaps)-min(gaps)<1e-5, 'Uneven tyre blocks: '+wheel.name
        tread=[f for f in wheel.data.polygons if all(radial[i]>radius-.0035 for i in f.vertices)]
        assert tread, 'Missing outward tread plateaus: '+wheel.name
        facing=sum((f.normal.y*f.center.y+f.normal.z*f.center.z)/math.hypot(f.center.y,f.center.z) for f in tread)/len(tread)
        assert facing>.85, 'Inverted tyre: '+wheel.name
        wheel_circles.append(dict(name=wheel.name,radius=radius,segments=segments,max_vertex_radius=max(radial),
                                  tread_rows=TREAD_ROWS[level],channel_depth=TREAD_DEPTH,tread_outward_mean=facing))
        # Check the visible rim lip as well as the tyre: a round tyre alone does
        # not prevent the retained irregular hub from wobbling during rotation.
        for x,r in ((.16,RIM_RADIUS),(.174,RIM_RADIUS-.005),(.180,RIM_RADIUS-.040),(-.10,RIM_RADIUS)):
            ring=[v for v in wheel.data.vertices if abs(v.co.x*sign-x)<1e-6 and abs(math.hypot(v.co.y,v.co.z)-r)<1e-6]
            assert len(ring)==segments, 'Incomplete rim ring: '+wheel.name
            assert math.hypot(sum(v.co.y for v in ring)/segments,sum(v.co.z for v in ring)/segments)<1e-6
            angles=sorted(math.atan2(v.co.z,v.co.y)%(2*math.pi) for v in ring)
            gaps=[(angles[(i+1)%segments]-angles[i])%(2*math.pi) for i in range(segments)]
            assert max(gaps)-min(gaps)<1e-5, 'Uneven rim circle: '+wheel.name
        bm=bmesh.new();bm.from_mesh(wheel.data)
        assert all(e.is_manifold for e in bm.edges), 'Open wheel surface: '+wheel.name
        def bead(v,x):return abs(v.co.x*sign-x)<1e-6 and abs(math.hypot(v.co.y,v.co.z)-RIM_RADIUS)<1e-6
        seam_edges=[e for e in bm.edges if any(all(bead(v,x) for v in e.verts) for x in (.16,-.10))]
        assert len(seam_edges)==2*segments
        for edge in seam_edges:
            sides=[sum(math.hypot(v.co.y,v.co.z)-RIM_RADIUS for v in f.verts) for f in edge.link_faces]
            assert min(sides)<-1e-5 and max(sides)>1e-5, 'Unjoined rim/tyre bead: '+wheel.name
        bm.free()
        rim_signature=sorted((round(v.co.x*sign,6),round(v.co.y,6),round(v.co.z,6))
                             for v,r in zip(wheel.data.vertices,radial) if r<=RIM_RADIUS+1e-6)
        reference_name='GolfCart_Wheel_FR_LOD'+str(level)
        if wheel.name==reference_name: reference_signature=rim_signature
        else:
            reference=bpy.data.objects[reference_name]
            reference_signature=sorted((round(v.co.x,6),round(v.co.y,6),round(v.co.z,6))
                                       for v in reference.data.vertices if math.hypot(v.co.y,v.co.z)<=RIM_RADIUS+1e-6)
        assert rim_signature==reference_signature, 'Unequal front/rear/left/right rim: '+wheel.name
        wheel_rims.append(dict(name=wheel.name,rim_diameter_metres=RIM_RADIUS*2,segments=segments,
                               welded_bead_edges=len(seam_edges),open_edges=0,equals_front_right=True))

def file_nodes(tree):
    objects = next(n for n in tree.elems if n.id == b'Objects')
    return [n for n in objects.elems if n.id in (b'Texture', b'Video')]

fbx = root / 'Game/Assets/_Game/Art/Golf/Cart/GolfCart.fbx'
tree, _ = parse_fbx.parse(str(fbx)); refs = set()
historical_fields = []
def inspect_metadata(node, ancestry=()):
    path = ancestry + (node.id.decode('utf8'),)
    for value in node.props:
        if not isinstance(value, bytes) or node.id == b'Content': continue
        if not re.search(rb'(?<![a-zA-Z0-9:])[A-Za-z]:[/\\]|^/(Users|home|root|mnt)/', value): continue
        assert path[0] == 'FBXHeaderExtension' and 'SceneInfo' in path, 'Unexpected active FBX path field: ' + '/'.join(path)
        historical_fields.append('/'.join(path))
    for child in node.elems: inspect_metadata(child, path)
for node in tree.elems: inspect_metadata(node)
for node in file_nodes(tree):
    for child in node.elems:
        if child.id not in (b'FileName', b'Filename', b'RelativeFilename'): continue
        for value in child.props:
            name = value.decode('utf8'); assert name == Path(name.replace('\\', '/')).name
            assert (fbx.parent / name).is_file(); refs.add(name)
assert len(refs) == 3

# Original export is a retained archive, not an active Unity dependency. Check
# its embedded payloads rather than treating historical provenance as delivery.
with zipfile.ZipFile(source / 'Originals/Meshy_AI_Golf_Cart_1003101246_texture_fbx.zip') as package:
    entry = next(n for n in package.namelist() if n.endswith('.fbx'))
    original = output / 'original.fbx'; original.write_bytes(package.read(entry))
original_tree, _ = parse_fbx.parse(str(original)); nodes = file_nodes(original_tree)
videos = [n for n in nodes if n.id == b'Video']
embedded = [any(c.id == b'Content' and any(len(v) > 0 for v in c.props) for c in n.elems) for n in videos]
assert videos and all(embedded), 'Original archive depends on external images'
report = dict(master_images=images, delivery_texture_refs=sorted(refs),
              original_embedded_images=len(videos), original_requires_external_images=False,
              master_meshes=16, master_external_libraries=0, delivery_lod_triangles=lod_triangles,
              inactive_exporter_provenance_fields=sorted(set(historical_fields)), wheel_caps=wheel_caps, wheel_circles=wheel_circles, wheel_rims=wheel_rims, axle_connections=axle_connections)
(output / 'audit.json').write_text(json.dumps(report, indent=2) + '\n')
print(json.dumps(report, indent=2))
