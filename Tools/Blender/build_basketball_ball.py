"""Build the basketball master and mobile FBX from a portable source specification.

blender --background --python Tools/Blender/build_basketball_ball.py
Optional: -- --root <checkout> --render (outputs reviews beneath Builds/).
The Meshy GLB is retained unchanged as visual provenance, not shipped geometry.
"""
import argparse
import hashlib
import json
import math
import struct
import sys
import zlib
from pathlib import Path

import bpy
import bmesh
import numpy as np
from mathutils import Vector


def png(path, pixels):
    """Write deterministic RGB8 data; input rows start at the image bottom."""
    pixels = np.uint8(np.clip(pixels[::-1], 0, 1) * 255 + .5)
    height, width, _ = pixels.shape
    def chunk(kind, data):
        return struct.pack('>I', len(data)) + kind + data + struct.pack('>I', zlib.crc32(kind + data))
    raw = b''.join(b'\0' + row.tobytes() for row in pixels)
    path.write_bytes(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', width, height, 8, 2, 0, 0, 0)) + chunk(b'IDAT', zlib.compress(raw, 9)) + chunk(b'IEND', b''))


def surface(width):
    height = width // 2
    u, v = np.meshgrid((np.arange(width) + .5) / width, (np.arange(height) + .5) / height)
    phi, theta = u * 2 * math.pi, v * math.pi
    x, y, z = np.sin(theta) * np.cos(phi), np.sin(theta) * np.sin(phi), -np.cos(theta)
    # Two great circles and one continuous curved loop divide the ball into eight
    # panels. Project its gradient onto the sphere for approximately uniform width.
    f = y - 1.25 * (z*z - x*x)
    gx, gy, gz = 2.5*x, np.ones_like(y), -2.5*z
    dot = gx*x + gy*y + gz*z
    gradient = np.sqrt(np.maximum(gx*gx + gy*gy + gz*gz - dot*dot, .01))
    distance = np.minimum(np.minimum(np.abs(x), np.abs(z)), np.abs(f) / gradient)
    groove = 1 - np.clip((distance - .011) / .008, 0, 1)
    groove = groove*groove*(3 - 2*groove)
    # Continuous 3D cellular rubber grain has no longitude join or polar pinch.
    p = np.stack([x, y, z], -1)*65
    cell = np.floor(p)
    nearest = np.full(x.shape, 10.0)
    for a in (-1, 0, 1):
        for b in (-1, 0, 1):
            for c in (-1, 0, 1):
                q = cell + (a, b, c)
                h = q[..., 0]*127.1 + q[..., 1]*311.7 + q[..., 2]*74.7
                feature = q + np.stack([np.mod(np.sin(h+s)*43758.5453, 1) for s in (0, 19.19, 47.47)], -1)*.66 + .17
                nearest = np.minimum(nearest, np.sum((p-feature)**2, -1))
    pebble = np.exp(-nearest*7)
    height_map = .00065 * pebble * (1-groove) - .006 * groove
    orange = np.array([.83, .305, .065])
    charcoal = np.array([.075, .060, .045])
    albedo = orange * (1 + (.035*pebble)[..., None])
    albedo = albedo*(1-groove[..., None]) + charcoal*groove[..., None]
    du = (np.roll(height_map, -1, 1)-np.roll(height_map, 1, 1)) / (4*math.pi/width*np.maximum(np.sin(theta), .015))
    dv = np.gradient(height_map, math.pi/height, axis=0)
    normal = np.stack([-du, -dv, np.ones_like(du)], -1)
    normal /= np.linalg.norm(normal, axis=-1, keepdims=True)
    return albedo, normal*.5+.5


def sphere(name, segments, rings):
    verts = [(0, 0, -.12)]
    for row in range(1, rings):
        theta = math.pi*row/rings
        for col in range(segments):
            phi = 2*math.pi*col/segments
            verts.append((.12*math.sin(theta)*math.cos(phi), .12*math.sin(theta)*math.sin(phi), -.12*math.cos(theta)))
    top = len(verts); verts.append((0, 0, .12))
    faces, coords = [], []
    def ring(row, col): return 1+(row-1)*segments+col % segments
    for col in range(segments):
        a, b = col/segments, (col+1)/segments
        faces.append((0, ring(1, col+1), ring(1, col)))
        coords.append((((a+b)/2, 0), (b, 1/rings), (a, 1/rings)))
        for row in range(1, rings-1):
            faces.append((ring(row,col), ring(row,col+1), ring(row+1,col+1), ring(row+1,col)))
            coords.append(((a,row/rings), (b,row/rings), (b,(row+1)/rings), (a,(row+1)/rings)))
        faces.append((ring(rings-1,col), ring(rings-1,col+1), top))
        coords.append(((a,(rings-1)/rings), (b,(rings-1)/rings), ((a+b)/2,1)))
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(verts, [], faces); mesh.update()
    uv = mesh.uv_layers.new(name='UV0')
    for face, values in zip(mesh.polygons, coords):
        face.use_smooth = True
        for loop, value in zip(face.loop_indices, values): uv.data[loop].uv = value
    mesh.normals_split_custom_set_from_vertices([Vector(v).normalized() for v in verts])
    obj = bpy.data.objects.new(name, mesh); bpy.context.scene.collection.objects.link(obj)
    return obj


def audit(obj):
    mesh = obj.data; mesh.calc_loop_triangles()
    bm = bmesh.new(); bm.from_mesh(mesh)
    result = dict(vertices=len(mesh.vertices), triangles=len(mesh.loop_triangles),
                  nonmanifold_edges=sum(not e.is_manifold for e in bm.edges),
                  max_vertex_radius_error_metres=max(abs(v.co.length-.12) for v in mesh.vertices),
                  min_face_normal_dot=min(p.normal.dot(p.center.normalized()) for p in mesh.polygons),
                  degenerate_triangles=sum(t.area < 1e-12 for t in mesh.loop_triangles),
                  dimensions_metres=list(obj.dimensions), origin=list(obj.location), scale=list(obj.scale))
    bm.free()
    assert result['nonmanifold_edges'] == result['degenerate_triangles'] == 0
    assert result['max_vertex_radius_error_metres'] < 1e-7
    assert result['min_face_normal_dot'] > .99
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument('--render', action='store_true')
    args = parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    root = args.root.resolve(); source = root/'ArtSource/Basketball/Ball'; game = root/'Game/Assets/_Game/Art/Basketball/Ball'
    evidence = root/'Builds/BasketballModel/20261001/Blender'; evidence.mkdir(parents=True, exist_ok=True); game.mkdir(parents=True, exist_ok=True)
    original = source/'Meshy_AI_Basketball_1001104636_texture.glb'
    data = original.read_bytes(); length, kind = struct.unpack_from('<II', data, 12)
    gltf = json.loads(data[20:20+length]); assert kind == 0x4E4F534A
    assert all('uri' not in entry for entry in gltf.get('buffers', []) + gltf.get('images', [])), 'GLB has an external dependency'
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(original))
    objects = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    xyz = np.array([tuple(o.matrix_world @ v.co) for o in objects for v in o.data.vertices])
    centre = (xyz.max(0)+xyz.min(0))/2; radii = np.linalg.norm(xyz-centre, axis=1)
    source_report = dict(file=original.name, sha256=hashlib.sha256(data).hexdigest(), bytes=len(data),
                         external_dependencies=[], mesh_objects=len(objects), dimensions=(xyz.max(0)-xyz.min(0)).tolist(),
                         radial_range=[float(radii.min()),float(radii.max())],
                         images=[dict(name=i.name, size=list(i.size), packed=bool(i.packed_file)) for i in bpy.data.images])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.scene.unit_settings.system='METRIC'; bpy.context.scene.unit_settings.scale_length=1
    bpy.context.preferences.filepaths.save_version=0
    for width, folder, suffix in ((2048, source, 'Master'), (1024, game, '')):
        color, normal = surface(width)
        png(folder/('Basketball'+suffix+'_BaseColor.png'), color)
        png(folder/('Basketball'+suffix+'_Normal.png'), normal)
    material = bpy.data.materials.new('Basketball_Rubber'); material.use_nodes=True
    bsdf = material.node_tree.nodes.get('Principled BSDF'); bsdf.inputs['Roughness'].default_value=.76
    bsdf.inputs['Metallic'].default_value=0
    for suffix, target in (('BaseColor','Base Color'), ('Normal','Normal')):
        im=bpy.data.images.load(str(source/('BasketballMaster_'+suffix+'.png')))
        if suffix=='Normal': im.colorspace_settings.name='Non-Color'
        im.filepath='//'+Path(im.filepath).name
        tex=material.node_tree.nodes.new('ShaderNodeTexImage'); tex.image=im
        if suffix=='Normal':
            normal=material.node_tree.nodes.new('ShaderNodeNormalMap'); material.node_tree.links.new(tex.outputs['Color'],normal.inputs['Color']); material.node_tree.links.new(normal.outputs['Normal'],bsdf.inputs[target])
        else: material.node_tree.links.new(tex.outputs['Color'],bsdf.inputs[target])
    lods=[sphere('Basketball_LOD0',48,32),sphere('Basketball_LOD1',24,16)]
    report=dict(source=source_report, diameter_metres=.24, geometry='Rebuilt analytical sphere; Meshy appearance reference retained',
                texture_design='Eight panels; uniform shallow channels and continuous procedural rubber grain',
                lods={o.name:audit(o) for o in lods}, material_count=1,
                delivery_textures=[dict(file='Basketball_'+s+'.png',width=1024,height=512) for s in ('BaseColor','Normal')])
    for o in lods:o.data.materials.append(material)
    lods[1].hide_render=True; lods[1].hide_set(True)
    bpy.context.view_layer.objects.active=lods[0]; lods[0].select_set(True)
    bpy.ops.wm.save_as_mainfile(filepath=str(source/'Basketball.blend'))
    # FBX uses delivery maps in its own directory, avoiding external master paths.
    for node in material.node_tree.nodes:
        if node.type=='TEX_IMAGE':
            suffix='Normal' if 'Normal' in node.image.name else 'BaseColor'
            node.image.filepath=str(game/('Basketball_'+suffix+'.png'))
    bpy.ops.object.select_all(action='DESELECT')
    for o in lods:o.hide_set(False);o.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(game/'Basketball.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_scale_options='FBX_SCALE_UNITS',use_mesh_modifiers=True,bake_anim=False,path_mode='RELATIVE',embed_textures=False,add_leaf_bones=False)
    # Format-aware FBX dependency check; provenance strings are not dependencies.
    from io_scene_fbx import parse_fbx, encode_bin
    tree, version=parse_fbx.parse(str(game/'Basketball.fbx'))
    # Blender writes an absolute fallback alongside RelativeFilename. Rewrite only
    # the parsed Texture/Video path properties, preserving geometry and bindings.
    def portable(node, parent=b''):
        out=encode_bin.FBXElem(node.id)
        values=list(node.props)
        if parent in (b'Texture',b'Video') and node.id in (b'FileName',b'Filename',b'RelativeFilename'):
            values=[Path(p.decode('utf8')).name.encode('utf8') if isinstance(p,bytes) else p for p in values]
        methods=dict(B='bool',C='char',Z='int8',Y='int16',I='int32',L='int64',F='float32',D='float64',R='bytes',S='string',i='int32_array',l='int64_array',f='float32_array',d='float64_array',b='bool_array',c='byte_array')
        for kind,value in zip(node.props_type,values):getattr(out,'add_'+methods[chr(kind)])(value)
        out.elems=[portable(child,node.id) for child in node.elems]
        return out
    encode_bin.write(str(game/'Basketball.fbx'),portable(tree),version)
    checked,_=parse_fbx.parse(str(game/'Basketball.fbx'))
    def same_payload(before,after,parent=b''):
        assert before.id==after.id and before.props_type==after.props_type
        if not(parent in (b'Texture',b'Video') and before.id in (b'FileName',b'Filename',b'RelativeFilename')):
            assert all(np.array_equal(a,b) for a,b in zip(before.props,after.props)),before.id
        assert len(before.elems)==len(after.elems)
        for a,b in zip(before.elems,after.elems):same_payload(a,b,before.id)
    same_payload(tree,checked)
    refs=[]
    def walk(node):
        if node.id==b'RelativeFilename':
            for p in node.props:
                if isinstance(p,bytes):refs.append(p.decode('utf8'))
        for child in node.elems:walk(child)
    walk(checked)
    assert refs and all(not Path(p).is_absolute() and (game/p).exists() for p in refs), refs
    report['fbx_relative_dependencies']=sorted(set(refs))
    report['fbx_path_rewrite_preserved_other_payloads']=True
    report['retained_metadata_exception']='FBX exporter Original|FileName /foobar.fbx is fixed, nonfunctional provenance; no image or library depends on it.'
    # Reopen saved artifacts: a successful export alone does not prove a teammate
    # can load the native master or the FBX with its dependencies intact.
    bpy.ops.wm.open_mainfile(filepath=str(source/'Basketball.blend'))
    assert not bpy.data.libraries, 'Master depends on an external Blender library'
    master_images=[]
    for im in bpy.data.images:
        if im.source != 'FILE': continue
        assert im.filepath.startswith('//') and Path(bpy.path.abspath(im.filepath)).is_file(), im.filepath
        master_images.append(im.filepath)
    assert len(master_images)==2
    report['reopened_master_image_dependencies']=sorted(master_images)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(game/'Basketball.fbx'))
    reimported=[o for o in bpy.context.scene.objects if o.type=='MESH']
    assert len(reimported)==2
    for obj in reimported:
        obj.data.calc_loop_triangles()
        assert len(obj.data.loop_triangles)==report['lods'][obj.name]['triangles']
        assert max(abs((obj.matrix_world@v.co).length-.12) for v in obj.data.vertices)<1e-6
        assert all(math.isfinite(c) for uv in obj.data.uv_layers.active.data for c in uv.uv)
    report['fbx_reimport_verified']='Two correctly scaled, centred LODs; expected triangle counts; finite UVs'
    report['files']={p.relative_to(root).as_posix():dict(bytes=p.stat().st_size,sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in [source/'Basketball.blend',game/'Basketball.fbx',*game.glob('*.png')]}
    (source/'model-audit.json').write_text(json.dumps(report,indent=2)+'\n')
    if args.render:
        bpy.ops.wm.open_mainfile(filepath=str(source/'Basketball.blend'))
        scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=40
        scene.render.resolution_x=1200;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
        scene.world=bpy.data.worlds.new('Review world');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.16,.18,.21,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.7
        scene.view_settings.view_transform='Standard';scene.view_settings.look='None'
        for location, energy, size in (((-.35,-.35,.50),12,.45),((.4,.1,.25),7,.35)):
            light=bpy.data.lights.new('Review softbox','AREA');light.energy=energy;light.shape='DISK';light.size=size
            obj=bpy.data.objects.new(light.name,light);scene.collection.objects.link(obj);obj.location=location;obj.rotation_euler=(-obj.location).to_track_quat('-Z','Y').to_euler()
        camera=bpy.data.objects.new('Review camera',bpy.data.cameras.new('Review camera'));scene.collection.objects.link(camera);scene.camera=camera;camera.data.type='ORTHO';camera.data.ortho_scale=.31
        for label,pos in (('front',(.045,-.6,.11)),('reverse',(-.08,.6,.15))):
            camera.location=pos;camera.rotation_euler=(-camera.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(evidence/(label+'.png'));bpy.ops.render.render(write_still=True)
    print('BASKETBALL_MODEL_OK '+json.dumps(report),flush=True)


if __name__=='__main__':main()
