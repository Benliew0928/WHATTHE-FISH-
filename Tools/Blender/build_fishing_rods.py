"""Derive portable fishing-rod masters and compact delivery assets from Meshy GLBs.

blender --background --python Tools/Blender/build_fishing_rods.py -- --render
Optional --root resolves a different checkout; defaults to this script's project.
"""
import argparse
import hashlib
import json
import math
import struct
import sys
from pathlib import Path

import bpy
import bmesh
import numpy as np
from mathutils import Vector, Matrix
from mathutils.bvhtree import BVHTree

SOURCES = {
    'BlueLime': ('Meshy_AI_Colorful_Fishing_Rod_1001115128_texture.glb', 10000, 3000),
    'TealOrange': ('Meshy_AI_Colorful_Fishing_Rod_1001115122_texture.glb', 14000, 4000),
}


def select(obj):
    bpy.ops.object.select_all(action='DESELECT')
    obj.hide_set(False)
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj


def audit(obj):
    mesh = obj.data
    mesh.calc_loop_triangles()
    bm = bmesh.new()
    bm.from_mesh(mesh)
    unseen = set(bm.verts)
    components = []
    while unseen:
        pending = [unseen.pop()]
        count = 0
        while pending:
            vertex = pending.pop()
            count += 1
            for edge in vertex.link_edges:
                other = edge.other_vert(vertex)
                if other in unseen:
                    unseen.remove(other)
                    pending.append(other)
        components.append(count)
    result = dict(vertices=len(mesh.vertices), triangles=len(mesh.loop_triangles),
                  nonmanifold_edges=sum(not e.is_manifold for e in bm.edges),
                  euler_characteristic=len(bm.verts)-len(bm.edges)+len(bm.faces),
                  components=sorted(components, reverse=True),
                  degenerate_triangles=sum(t.area < 1e-15 for t in mesh.loop_triangles),
                  finite_uvs=all(math.isfinite(c) for uv in mesh.uv_layers.active.data for c in uv.uv),
                  dimensions_metres=np.ptp(np.array([tuple(v.co) for v in mesh.vertices]), axis=0).tolist())
    bm.free()
    assert result['finite_uvs'] and result['degenerate_triangles'] == result['nonmanifold_edges'] == 0, result
    return result


def normalize(obj):
    select(obj)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    xyz = np.array([tuple(v.co) for v in obj.data.vertices])
    _, axes = np.linalg.eigh(np.cov(xyz.T))
    rotation = Vector(axes[:, -1]).rotation_difference(Vector((0, 0, 1)))
    for v in obj.data.vertices:
        v.co = rotation @ v.co
    xyz = np.array([tuple(v.co) for v in obj.data.vertices])
    bottom, top = xyz[:, 2].min(), xyz[:, 2].max()
    length = top-bottom
    a, b = xyz[xyz[:, 2] < bottom+length*.25], xyz[xyz[:, 2] > top-length*.25]
    if np.ptp(a[:, :2], axis=0).max() < np.ptp(b[:, :2], axis=0).max():
        for v in obj.data.vertices:
            v.co.z = -v.co.z
            v.co.x = -v.co.x
    xyz = np.array([tuple(v.co) for v in obj.data.vertices])
    bottom = xyz[:, 2].min()
    butt = xyz[xyz[:, 2] < bottom+length*.035].mean(0)
    butt[2] = bottom
    reel = xyz[(xyz[:, 2] > bottom+length*.23) & (xyz[:, 2] < bottom+length*.40)].mean(0)-butt
    roll = Matrix.Rotation(-math.pi/2-math.atan2(reel[1], reel[0]), 3, 'Z')
    for v in obj.data.vertices:
        v.co = roll @ ((v.co-Vector(butt))*(1.8/length))
    # Recalculate after rotating vertex positions; imported custom normals use the old basis.
    obj.data.normals_split_custom_set([(0, 0, 0)]*len(obj.data.loops))
    for face in obj.data.polygons:
        face.use_smooth = True
    obj.data.update()


def save_image(im, path, size=None):
    if size:
        im = im.copy()
        im.scale(*size)
    _ = im.pixels[0]
    im.filepath_raw = str(path)
    im.file_format = 'PNG'
    im.save()
    if im.packed_file:
        im.unpack(method='REMOVE')
    im.filepath = str(path)
    return im


def portable_fbx(path):
    from io_scene_fbx import parse_fbx, encode_bin
    tree, version = parse_fbx.parse(str(path))
    methods = dict(B='bool', C='char', Z='int8', Y='int16', I='int32', L='int64', F='float32', D='float64', R='bytes', S='string', i='int32_array', l='int64_array', f='float32_array', d='float64_array', b='bool_array', c='byte_array')
    refs = []
    def rewrite(node, parent=b''):
        out = encode_bin.FBXElem(node.id)
        values = list(node.props)
        if parent in (b'Texture', b'Video') and node.id in (b'FileName', b'Filename', b'RelativeFilename'):
            values = [Path(p.decode('utf8').replace('\\', '/')).name.encode() if isinstance(p, bytes) else p for p in values]
            refs.extend(p.decode() for p in values if isinstance(p, bytes))
        for kind, value in zip(node.props_type, values):
            getattr(out, 'add_'+methods[chr(kind)])(value)
        out.elems = [rewrite(child, node.id) for child in node.elems]
        return out
    encode_bin.write(str(path), rewrite(tree), version)
    checked, _ = parse_fbx.parse(str(path))
    def verify(before, after, parent=b''):
        assert before.id == after.id and before.props_type == after.props_type
        if not (parent in (b'Texture', b'Video') and before.id in (b'FileName', b'Filename', b'RelativeFilename')):
            assert all(np.array_equal(a, b) for a, b in zip(before.props, after.props))
        assert len(before.elems) == len(after.elems)
        for a, b in zip(before.elems, after.elems):
            verify(a, b, before.id)
    verify(tree, checked)
    assert refs and all((path.parent/p).is_file() for p in refs)
    return sorted(set(refs))


def surface_error(high, low):
    mesh = low.data
    tree = BVHTree.FromPolygons([v.co for v in mesh.vertices], [list(p.vertices) for p in mesh.polygons])
    distances = [tree.find_nearest(v.co)[3] for v in high.data.vertices]
    return dict(max_source_vertex_distance_metres=max(distances), rms_source_vertex_distance_metres=float(np.sqrt(np.mean(np.square(distances)))))


def bake_delivery(name, high, low, color, game):
    # Collapse interpolation across the source UV seams produced jagged colour
    # boundaries. Reproject source colour/normal onto a new near-mesh unwrap.
    select(low)
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=1.15, island_margin=.012)
    bpy.ops.object.mode_set(mode='OBJECT')
    bake_mat = bpy.data.materials.new(name+'_Bake')
    bake_mat.use_nodes = True
    low.data.materials.clear()
    low.data.materials.append(bake_mat)
    node = bake_mat.node_tree.nodes.new('ShaderNodeTexImage')
    bake_mat.node_tree.nodes.active = node
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.cycles.samples = 16
    scene.render.bake.use_selected_to_active = True
    scene.render.bake.cage_extrusion = .0015
    scene.render.bake.max_ray_distance = .003
    scene.render.bake.margin = 10
    source_mat = high.data.materials[0]
    nodes, links = source_mat.node_tree.nodes, source_mat.node_tree.links
    emission = nodes.new('ShaderNodeEmission')
    tex = nodes.new('ShaderNodeTexImage')
    tex.image = color
    links.new(tex.outputs['Color'], emission.inputs['Color'])
    result = []
    for suffix, kind, size in [('BaseColor', 'EMIT', 1024), ('Normal', 'NORMAL', 512)]:
        im = bpy.data.images.new(name+'_Delivery'+suffix, width=size, height=size, alpha=False)
        if suffix == 'Normal':
            im.colorspace_settings.name = 'Non-Color'
        node.image = im
        output = nodes.get('Material Output').inputs['Surface']
        links.new(emission.outputs[0] if kind == 'EMIT' else nodes.get('Principled BSDF').outputs[0], output)
        select(low)
        high.select_set(True)
        bpy.ops.object.bake(type=kind)
        result.append(save_image(im, game/(name+'_'+suffix+'.png')))
    links.new(nodes.get('Principled BSDF').outputs[0], nodes.get('Material Output').inputs['Surface'])
    return result


def render(name, lods, evidence):
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.cycles.samples = 24
    scene.render.resolution_x = 900
    scene.render.resolution_y = 1100
    scene.render.resolution_percentage = 100
    scene.world = bpy.data.worlds.new('Review')
    scene.world.use_nodes = True
    scene.world.node_tree.nodes['Background'].inputs[0].default_value = (.22, .24, .27, 1)
    scene.world.node_tree.nodes['Background'].inputs[1].default_value = .8
    centre = Vector((0, -.04, .9))
    for pos in [(-2, -3, 3), (2, 2, 2)]:
        light = bpy.data.lights.new('Softbox', 'AREA')
        light.energy = 200
        light.size = 3
        ob = bpy.data.objects.new(light.name, light)
        scene.collection.objects.link(ob)
        ob.location = pos
        ob.rotation_euler = (centre-ob.location).to_track_quat('-Z', 'Y').to_euler()
    camera = bpy.data.objects.new('Review camera', bpy.data.cameras.new('Review camera'))
    scene.collection.objects.link(camera)
    scene.camera = camera
    camera.data.type = 'ORTHO'
    camera.data.ortho_scale = 2.1
    for label, offset in [('front', (2, -4, .2)), ('back', (-2, 4, .2))]:
        camera.location = centre+Vector(offset)
        camera.rotation_euler = (centre-camera.location).to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = str(evidence/(name+'-delivery-'+label+'.png'))
        bpy.ops.render.render(write_still=True)
    lods[0].hide_render = True
    lods[1].hide_render = False
    scene.render.filepath = str(evidence/(name+'-distance-lod.png'))
    bpy.ops.render.render(write_still=True)


def build(name, spec, root, do_render):
    filename, near_count, far_count = spec
    source = root/'ArtSource/Fishing'
    masters = source/'Rods'
    textures = masters/'Textures'
    game = root/'Game/Assets/_Game/Art/Fishing/Rods'
    evidence = root/'Builds/FishingRods/20261001'
    for folder in [masters, textures, game, evidence]:
        folder.mkdir(parents=True, exist_ok=True)
    original = source/filename
    data = original.read_bytes()
    length, kind = struct.unpack_from('<II', data, 12)
    gltf = json.loads(data[20:20+length])
    assert kind == 0x4E4F534A and all('uri' not in x for x in gltf.get('buffers', [])+gltf.get('images', []))
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.preferences.filepaths.save_version = 0
    bpy.ops.import_scene.gltf(filepath=str(original), merge_vertices=True)
    objects = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    assert len(objects) == 1
    high = objects[0]
    original_audit = audit(high)
    high.name = name+'_MeshyMaster'
    normalize(high)
    mat = high.data.materials[0]
    nodes = mat.node_tree.nodes
    bsdf = nodes.get('Principled BSDF')
    color = bsdf.inputs['Base Color'].links[0].from_node.image
    normal = bsdf.inputs['Normal'].links[0].from_node.inputs['Color'].links[0].from_node.image
    master_images = list(bpy.data.images)
    for i, im in enumerate(master_images):
        suffix = 'BaseColor' if im == color else 'Normal' if im == normal else 'MetallicRoughness'
        im.name = name+'_'+suffix
        save_image(im, textures/(im.name+'.png'))
    delivery_color = save_image(color, game/(name+'_BaseColor.png'), (1024, 1024))
    delivery_normal = save_image(normal, game/(name+'_Normal.png'), (512, 512))
    delivery = bpy.data.materials.new(name+'_Delivery')
    delivery.use_nodes = True
    dnodes, links = delivery.node_tree.nodes, delivery.node_tree.links
    shader = dnodes.get('Principled BSDF')
    shader.inputs['Roughness'].default_value = .48
    shader.inputs['Metallic'].default_value = 0
    tex = dnodes.new('ShaderNodeTexImage')
    tex.image = delivery_color
    links.new(tex.outputs['Color'], shader.inputs['Base Color'])
    tex = dnodes.new('ShaderNodeTexImage')
    tex.image = delivery_normal
    normal_node = dnodes.new('ShaderNodeNormalMap')
    normal_node.inputs['Strength'].default_value = .5
    links.new(tex.outputs['Color'], normal_node.inputs['Color'])
    links.new(normal_node.outputs['Normal'], shader.inputs['Normal'])
    lods, reports = [], {}
    for i, target in enumerate([near_count, far_count]):
        basis = high if i == 0 else lods[0]
        obj = basis.copy()
        obj.data = basis.data.copy()
        bpy.context.scene.collection.objects.link(obj)
        obj.name = name+'_LOD'+str(i)
        select(obj)
        modifier = obj.modifiers.new('Mobile reduction', 'DECIMATE')
        basis.data.calc_loop_triangles()
        modifier.ratio = target/len(basis.data.loop_triangles)
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        current = audit(obj)
        assert current['euler_characteristic'] == original_audit['euler_characteristic'], 'A ring opening was lost'
        assert len(current['components']) == len(original_audit['components'])
        current.update(surface_error(high, obj))
        assert current['max_source_vertex_distance_metres'] < (.008 if i == 0 else .02)
        if i == 0:
            old_color, old_normal = delivery_color, delivery_normal
            delivery_color, delivery_normal = bake_delivery(name, high, obj, color, game)
            for node in delivery.node_tree.nodes:
                if node.type == 'TEX_IMAGE':
                    node.image = delivery_color if node.image == old_color else delivery_normal
            bpy.data.images.remove(old_color)
            bpy.data.images.remove(old_normal)
        obj.data.materials.clear()
        obj.data.materials.append(delivery)
        lods.append(obj)
        reports[obj.name] = current
    high.hide_render = True
    high.hide_set(True)
    lods[1].hide_render = True
    lods[1].hide_set(True)
    # Master uses relative high-quality maps and packed delivery previews.
    for im in master_images:
        im.filepath = '//Textures/'+Path(im.filepath).name
    for im in [delivery_color, delivery_normal]:
        im.pack()
        im.filepath = '//Packed/'+im.name+'.png'
    bpy.ops.wm.save_as_mainfile(filepath=str(masters/(name+'.blend')))
    for im, suffix in [(delivery_color, 'BaseColor'), (delivery_normal, 'Normal')]:
        im.filepath = str(game/(name+'_'+suffix+'.png'))
    bpy.ops.object.select_all(action='DESELECT')
    for obj in lods:
        obj.hide_set(False)
        obj.select_set(True)
    fbx = game/(name+'.fbx')
    bpy.ops.export_scene.fbx(filepath=str(fbx), use_selection=True, object_types={'MESH'}, axis_forward='-Z', axis_up='Y', apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, path_mode='RELATIVE', embed_textures=False, add_leaf_bones=False)
    refs = portable_fbx(fbx)
    if do_render:
        render(name, lods, evidence)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(fbx))
    imported = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    assert len(imported) == 2
    for obj in imported:
        assert audit(obj)['triangles'] == reports[obj.name]['triangles']
        points = [obj.matrix_world@v.co for v in obj.data.vertices]
        assert abs(max(v.z for v in points)-min(v.z for v in points)-1.8) < .008
    bpy.ops.wm.open_mainfile(filepath=str(masters/(name+'.blend')))
    assert not bpy.data.libraries
    assert all(im.filepath.startswith('//') and (im.packed_file or Path(bpy.path.abspath(im.filepath)).exists()) for im in bpy.data.images)
    return dict(source_file=original.relative_to(root).as_posix(), source_bytes=len(data), source_sha256=hashlib.sha256(data).hexdigest(), source=original_audit, source_external_dependencies=[], length_metres=1.8, pivot='butt cap; rod rises along Unity +Y; original slight shaft curvature retained', lods=reports, fbx_relative_dependencies=refs, fbx_other_payload_preserved=True, master_reopen_verified=True, fbx_reimport_verified=True)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument('--render', action='store_true')
    args = parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    root = args.root.resolve()
    report = {'path_base': 'repository root', 'models': {}}
    for name, spec in SOURCES.items():
        report['models'][name] = build(name, spec, root, args.render)
        print('FISHING_ROD_OK '+name, flush=True)
    source = root/'ArtSource/Fishing/Rods'
    game = root/'Game/Assets/_Game/Art/Fishing/Rods'
    report['files'] = {p.relative_to(root).as_posix(): dict(bytes=p.stat().st_size, sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in [*source.glob('*.blend'), *source.glob('Textures/*.png'), *game.glob('*.fbx'), *game.glob('*.png')]}
    report['retained_metadata_exception'] = 'Blender exporter fixed Original|FileName /foobar.fbx is nonfunctional provenance. All active delivery texture dependencies are relative siblings; master maps are relative or packed.'
    (source/'model-audit.json').write_text(json.dumps(report, indent=2)+'\n')
    print('FISHING_RODS_COMPLETE', flush=True)


if __name__ == '__main__':
    main()
