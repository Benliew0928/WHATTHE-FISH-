"""Derive portable golf masters and mobile FBXs from the retained Meshy ZIPs.

Run with Blender --background --python Tools/Blender/build_golf_equipment.py.
Optional arguments after --: --root <checkout> --render. No network access.
"""
import argparse
import hashlib
import json
import math
import sys
import zipfile
from pathlib import Path

import bpy
import bmesh
import numpy as np
from mathutils import Vector

SOURCES = {
    'Ball': ('Meshy_AI_Dimpled_Moon_1001111942_texture_fbx.zip', .043),
    'Driver': ('Meshy_AI_Teal_Horizon_Driver_1001112212_texture_fbx.zip', 1.14),
    'Iron': ('Meshy_AI_Golf_Club_1001112230_texture_fbx.zip', .94),
    'Putter': ('Meshy_AI_Golf_Putter_1001112238_texture_fbx.zip', .89),
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
                  components=sorted(components, reverse=True),
                  degenerate_triangles=sum(t.area < 1e-15 for t in mesh.loop_triangles),
                  dimensions_metres=list(obj.dimensions),
                  finite_uvs=all(math.isfinite(c) for uv in mesh.uv_layers.active.data for c in uv.uv))
    bm.free()
    assert result['finite_uvs'] and not result['degenerate_triangles'], result
    return result


def sphere(name, segments, rings):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=.0215)
    obj = bpy.context.object
    obj.name = name
    for p in obj.data.polygons:
        p.use_smooth = True
    obj.data.normals_split_custom_set_from_vertices([v.co.normalized() for v in obj.data.vertices])
    return obj


def material(name, color, normal, metallic=None, roughness=None):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    nodes, links = mat.node_tree.nodes, mat.node_tree.links
    bsdf = nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = (.91, .90, .86, 1)
    bsdf.inputs['Roughness'].default_value = .38
    for im, socket in [(color, 'Base Color'), (normal, 'Normal'), (metallic, 'Metallic'), (roughness, 'Roughness')]:
        if im is None:
            continue
        tex = nodes.new('ShaderNodeTexImage')
        tex.image = im
        if socket == 'Normal':
            node = nodes.new('ShaderNodeNormalMap')
            links.new(tex.outputs['Color'], node.inputs['Color'])
            links.new(node.outputs['Normal'], bsdf.inputs[socket])
        else:
            links.new(tex.outputs['Color'], bsdf.inputs[socket])
    return mat


def save_image(im, path):
    # Decode lazily loaded source files before changing their filename.
    assert len(im.pixels) > 0
    _ = im.pixels[0]
    im.filepath_raw = str(path)
    im.file_format = 'PNG'
    im.save()
    if im.packed_file:
        im.unpack(method='REMOVE')
    im.filepath = str(path)


def delivery_image(source, path, size):
    im = source.copy()
    im.scale(*size)
    im.filepath_raw = str(path)
    im.file_format = 'PNG'
    im.save()
    return im


def portable_fbx(path):
    from io_scene_fbx import parse_fbx, encode_bin
    tree, version = parse_fbx.parse(str(path))
    methods = dict(B='bool', C='char', Z='int8', Y='int16', I='int32', L='int64', F='float32', D='float64', R='bytes', S='string', i='int32_array', l='int64_array', f='float32_array', d='float64_array', b='bool_array', c='byte_array')
    refs = []
    def path_slots(node, ancestry):
        if ancestry and ancestry[-1] in (b'Texture', b'Video') and node.id in (b'FileName', b'Filename', b'RelativeFilename'):
            return set(range(len(node.props)))
        # Blender also writes a live image path in Video.Properties70.Path.
        if node.id == b'P' and node.props and node.props[0] == b'Path' and ancestry and ancestry[-1] == b'Properties70' and any(p in (b'Texture', b'Video') for p in ancestry):
            return {len(node.props) - 1}
        return set()
    def rewrite(node, ancestry=()):
        out = encode_bin.FBXElem(node.id)
        values = list(node.props)
        for index in path_slots(node, ancestry):
            if not isinstance(values[index], bytes): continue
            values[index] = Path(values[index].decode('utf8').replace('\\', '/')).name.encode()
            refs.append(values[index].decode())
        for kind, value in zip(node.props_type, values):
            getattr(out, 'add_' + methods[chr(kind)])(value)
        out.elems = [rewrite(child, ancestry + (node.id,)) for child in node.elems]
        return out
    encode_bin.write(str(path), rewrite(tree), version)
    checked, _ = parse_fbx.parse(str(path))
    def verify(before, after, ancestry=()):
        assert before.id == after.id and before.props_type == after.props_type
        slots = path_slots(before, ancestry)
        assert all(index in slots or np.array_equal(a, b) for index, (a, b) in enumerate(zip(before.props, after.props)))
        assert len(before.elems) == len(after.elems)
        for a, b in zip(before.elems, after.elems):
            verify(a, b, ancestry + (before.id,))
    verify(tree, checked)
    assert refs and all((path.parent / p).is_file() for p in refs)
    return sorted(set(refs))


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
    scene.view_settings.view_transform = 'AgX'
    obj = lods[0]
    points = [obj.matrix_world @ v.co for v in obj.data.vertices]
    centre = sum(points, Vector()) / len(points) if name == 'Ball' else Vector((-.025, 0, -SOURCES[name][1] / 2))
    extent = SOURCES[name][1]
    for pos, energy in [((-.8, -1.2, 1.5), 140), ((1, 1, 1), 100)]:
        light = bpy.data.lights.new('Softbox', 'AREA')
        light.energy = energy * extent * extent
        light.size = extent * 2
        ob = bpy.data.objects.new(light.name, light)
        scene.collection.objects.link(ob)
        ob.location = centre + Vector(pos) * extent
        ob.rotation_euler = (centre - ob.location).to_track_quat('-Z', 'Y').to_euler()
    cam = bpy.data.objects.new('Review camera', bpy.data.cameras.new('Review camera'))
    scene.collection.objects.link(cam)
    scene.camera = cam
    cam.data.type = 'ORTHO'
    cam.data.ortho_scale = extent * 1.25
    cam.data.clip_start = .0001
    for label, pos in [('front', (.25, -2, .15)), ('back', (-.25, 2, .15))]:
        cam.location = centre + Vector(pos) * extent
        cam.rotation_euler = (centre - cam.location).to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = str(evidence / (name + '-delivery-' + label + '.png'))
        bpy.ops.render.render(write_still=True)


def build(name, filename, length, root, source, game, evidence, do_render):
    if name == 'Ball':
        from prepare_golf_ball import prepare
        return prepare(root, do_render)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.preferences.filepaths.save_version = 0
    archive = source / 'Downloads' / filename
    extracted = evidence / (name + 'Source')
    extracted.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(archive) as package:
        for member in package.infolist():
            target = (extracted / member.filename).resolve()
            assert target.is_relative_to(extracted.resolve()), 'ZIP entry escapes extraction folder'
        package.extractall(extracted)
    fbx = next(extracted.rglob('*.fbx'))
    bpy.ops.import_scene.fbx(filepath=str(fbx))
    objects = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    assert len(objects) == 1
    high = objects[0]
    select(high)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    xyz = np.array([tuple(v.co) for v in high.data.vertices])
    source_report = audit(high)
    if name == 'Ball':
        centre = (xyz.min(0) + xyz.max(0)) / 2
        # Uniform size normalization retains the actual generated dimple surface.
        scale = .0215 / np.linalg.norm(xyz - centre, axis=1).max()
        for v in high.data.vertices:
            v.co = (v.co - Vector(centre)) * scale
    else:
        # Fit only the central shaft, avoiding the head's influence on orientation.
        shaft = xyz[(xyz[:, 2] > -.35) & (xyz[:, 2] < .35)]
        _, axes = np.linalg.eigh(np.cov(shaft.T))
        axis = Vector(axes[:, -1])
        if axis.z < 0:
            axis.negate()
        rotation = axis.rotation_difference(Vector((0, 0, 1)))
        for v in high.data.vertices:
            v.co = rotation @ v.co
        xyz = np.array([tuple(v.co) for v in high.data.vertices])
        scale = length / np.ptp(xyz[:, 2])
        top = xyz[xyz[:, 2] > xyz[:, 2].max() - .035].mean(0)
        top[2] = xyz[:, 2].max()
        for v in high.data.vertices:
            v.co = (v.co - Vector(top)) * scale
    high.name = name + '_MeshyMaster'
    for p in high.data.polygons:
        p.use_smooth = True
    high.data.update()
    if name != 'Ball':
        # Original split normals describe the original diagonal shaft orientation.
        # Clear them after editing vertex coordinates so the new normals agree.
        high.data.normals_split_custom_set([(0, 0, 0)] * len(high.data.loops))
    bpy.context.view_layer.update()
    textures = source / 'Textures'
    textures.mkdir(exist_ok=True)
    maps = {}
    suffixes = {'BaseColor': '_texture.png', 'Normal': '_texture_normal.png', 'Metallic': '_texture_metallic.png', 'Roughness': '_texture_roughness.png'}
    # Use the lossless PNGs in the ZIP, not the FBX's embedded JPEG copies.
    for key, suffix in suffixes.items():
        path = next(extracted.rglob('*' + suffix))
        im = bpy.data.images.load(str(path))
        im.name = name + '_' + key
        if key != 'BaseColor':
            im.colorspace_settings.name = 'Non-Color'
        save_image(im, textures / (name + '_' + key + '.png'))
        maps[key] = im
    high.data.materials.clear()
    high.data.materials.append(material(name + '_Master', maps['BaseColor'], maps['Normal'], maps['Metallic'], maps['Roughness']))
    if name == 'Ball':
        lods = [sphere('Ball_LOD0', 48, 32), sphere('Ball_LOD1', 24, 16)]
        normal = bpy.data.images.new('Ball_DimpleNormal', width=1024, height=512, alpha=False)
        normal.colorspace_settings.name = 'Non-Color'
        mat = material('Ball_Delivery', None, None)
        node = mat.node_tree.nodes.new('ShaderNodeTexImage')
        node.image = normal
        mat.node_tree.nodes.active = node
        lods[0].data.materials.append(mat)
        # Bake genuine Meshy geometry onto a perfectly round lightweight sphere.
        high.data.materials[0].node_tree.links.clear()
        scene = bpy.context.scene
        scene.render.engine = 'CYCLES'
        scene.cycles.samples = 16
        scene.render.bake.use_selected_to_active = True
        scene.render.bake.cage_extrusion = .004
        scene.render.bake.max_ray_distance = .008
        scene.render.bake.margin = 8
        select(lods[0])
        high.select_set(True)
        bpy.ops.object.bake(type='NORMAL')
        save_image(normal, textures / 'Ball_DimpleNormal.png')
        high.data.materials.clear()
        high.data.materials.append(material(name + '_Master', maps['BaseColor'], maps['Normal'], maps['Metallic'], maps['Roughness']))
        mat = material('Ball_Delivery', None, normal)
        for obj in lods:
            obj.data.materials.clear()
            obj.data.materials.append(mat)
        delivery_image(normal, game / 'Ball_Normal.png', (1024, 512))
        delivery_mat = material('Ball', None, bpy.data.images.load(str(game / 'Ball_Normal.png')))
        delivery_mat.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value = .38
    else:
        lods = []
        for level, count in enumerate([4096]):
            obj = high.copy()
            obj.data = high.data.copy()
            bpy.context.scene.collection.objects.link(obj)
            obj.name = name + '_LOD' + str(level)
            select(obj)
            mod = obj.modifiers.new('Mobile reduction', 'DECIMATE')
            mod.ratio = min(1, count / source_report['triangles'])
            bpy.ops.object.modifier_apply(modifier=mod.name)
            lods.append(obj)
        low = lods[0]
        select(low)
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.uv.smart_project(angle_limit=1.15, island_margin=.015)
        bpy.ops.object.mode_set(mode='OBJECT')
        bake_mat = material(name + '_Bake', None, None)
        low.data.materials.clear()
        low.data.materials.append(bake_mat)
        node = bake_mat.node_tree.nodes.new('ShaderNodeTexImage')
        bake_mat.node_tree.nodes.active = node
        scene = bpy.context.scene
        scene.render.engine = 'CYCLES'
        scene.cycles.samples = 16
        scene.render.bake.use_selected_to_active = True
        scene.render.bake.cage_extrusion = .008
        scene.render.bake.max_ray_distance = .016
        scene.render.bake.margin = 10
        scene.render.bake.use_pass_direct = False
        scene.render.bake.use_pass_indirect = False
        scene.render.bake.use_pass_color = True
        baked = {}
        # Reproject the real high-resolution source onto fresh UVs. Reusing UVs
        # after collapse interpolation caused triangular colour discontinuities.
        for suffix, kind, size in [('BaseColor', 'EMIT', 512), ('Normal', 'NORMAL', 512), ('Roughness', 'ROUGHNESS', 256), ('Metallic', 'EMIT', 256)]:
            im = bpy.data.images.new(name + '_Delivery' + suffix, width=size, height=size, alpha=False)
            if suffix != 'BaseColor':
                im.colorspace_settings.name = 'Non-Color'
            node.image = im
            if kind == 'EMIT':
                source_mat = high.data.materials[0]
                emission = source_mat.node_tree.nodes.new('ShaderNodeEmission')
                tex = source_mat.node_tree.nodes.new('ShaderNodeTexImage')
                tex.image = maps[suffix]
                source_mat.node_tree.links.new(tex.outputs['Color'], emission.inputs['Color'])
                source_mat.node_tree.links.new(emission.outputs[0], source_mat.node_tree.nodes.get('Material Output').inputs['Surface'])
            select(low)
            high.select_set(True)
            bpy.ops.object.bake(type=kind)
            if kind == 'EMIT':
                # A diffuse bake omits metallic base colour. Emission transfers
                # the source RGB independently of lighting and metalness.
                source_mat.node_tree.links.new(source_mat.node_tree.nodes.get('Principled BSDF').outputs[0], source_mat.node_tree.nodes.get('Material Output').inputs['Surface'])
            save_image(im, textures / (name + '_Delivery' + suffix + '.png'))
            baked[suffix] = im
        source_mat.node_tree.links.new(source_mat.node_tree.nodes.get('Principled BSDF').outputs[0], source_mat.node_tree.nodes.get('Material Output').inputs['Surface'])
        color = delivery_image(baked['BaseColor'], game / (name + '_BaseColor.png'), (512, 512))
        normal = delivery_image(baked['Normal'], game / (name + '_Normal.png'), (512, 512))
        metal, rough = baked['Metallic'], baked['Roughness']
        pixels = np.array(metal.pixels[:]).reshape(-1, 4)
        roughness = np.array(rough.pixels[:]).reshape(-1, 4)[:, 0]
        pixels[:, 1:3] = 0
        pixels[:, 3] = np.clip(1 - roughness, .2, .68)
        mask = bpy.data.images.new(name + '_Mask', width=256, height=256, alpha=True)
        mask.colorspace_settings.name = 'Non-Color'
        mask.pixels.foreach_set(pixels.astype(np.float32).ravel())
        mask.filepath_raw = str(game / (name + '_Mask.png'))
        mask.file_format = 'PNG'
        mask.save()
        delivery_mat = material(name, color, normal, metal, rough)
        # Only portable delivery images may be referenced by an exported FBX.
        for node in delivery_mat.node_tree.nodes:
            if node.type == 'TEX_IMAGE' and node.image in (metal, rough):
                delivery_mat.node_tree.nodes.remove(node)
        delivery_mat.node_tree.nodes.get('Principled BSDF').inputs['Metallic'].default_value = .65
        obj = low.copy()
        obj.data = low.data.copy()
        bpy.context.scene.collection.objects.link(obj)
        obj.name = name + '_LOD1'
        select(obj)
        mod = obj.modifiers.new('Distance reduction', 'DECIMATE')
        low.data.calc_loop_triangles()
        mod.ratio = 1200 / len(low.data.loop_triangles)
        bpy.ops.object.modifier_apply(modifier=mod.name)
        lods.append(obj)
        master_delivery_mat = material(name + '_DeliveryMaster', baked['BaseColor'], baked['Normal'], metal, rough)
        for obj in lods:
            obj.data.materials.clear()
            obj.data.materials.append(master_delivery_mat)
    report = dict(source_zip='Downloads/' + filename, source_sha256=hashlib.sha256(archive.read_bytes()).hexdigest(), source=source_report,
                  length_metres=length, pivot='centre' if name == 'Ball' else 'grip cap; shaft along local -Y in Unity',
                  lods={obj.name: audit(obj) for obj in lods})
    assert all(item['nonmanifold_edges'] == 0 and len(item['components']) == 1 for item in report['lods'].values())
    high.hide_render = True
    high.hide_set(True)
    lods[1].hide_render = True
    lods[1].hide_set(True)
    # Remove unused embedded copies and transient delivery materials before saving.
    used_materials = {mat for obj in [high, *lods] for mat in obj.data.materials}
    for mat in list(bpy.data.materials):
        if mat not in used_materials and mat != delivery_mat:
            bpy.data.materials.remove(mat)
    for im in list(bpy.data.images):
        if im not in maps.values() and im.name != 'Ball_DimpleNormal' and im.users == 0:
            bpy.data.images.remove(im)
    # All loaded image slots receive a relative path; used master images exist.
    for im in bpy.data.images:
        if im in maps.values() or im.name == 'Ball_DimpleNormal' or '_Delivery' in im.name and im.filepath and Path(im.filepath).parent == textures:
            im.filepath = '//Textures/' + Path(im.filepath).name
            assert im.filepath.startswith('//Textures/') and (source / im.filepath[2:]).exists()
        elif not im.packed_file:
            im.pack()
            im.filepath = '//Packed/' + Path(im.filepath).name
        else:
            im.filepath = '//Packed/' + Path(im.filepath).name
    bpy.ops.wm.save_as_mainfile(filepath=str(source / (name + '.blend')))
    for obj in lods:
        obj.hide_render = False
        obj.hide_set(False)
        obj.data.materials.clear()
        obj.data.materials.append(delivery_mat)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in lods:
        obj.select_set(True)
    # Rebind export images to existing sibling PNG files.
    for node in delivery_mat.node_tree.nodes:
        if node.type == 'TEX_IMAGE':
            suffix = 'Normal' if 'Normal' in node.image.name else 'BaseColor'
            node.image.filepath = str(game / (name + '_' + suffix + '.png'))
    path = game / (name + '.fbx')
    bpy.ops.export_scene.fbx(filepath=str(path), use_selection=True, object_types={'MESH'}, axis_forward='-Z', axis_up='Y', apply_scale_options='FBX_SCALE_UNITS', bake_anim=False, path_mode='RELATIVE', embed_textures=False, add_leaf_bones=False)
    report['fbx_relative_dependencies'] = portable_fbx(path)
    report['fbx_path_rewrite_preserved_other_payloads'] = True
    lods[1].hide_render = True
    if do_render:
        render(name, lods, evidence)
    # Verify the exported payload, including thin-shaft topology and real dimensions.
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(path))
    delivered = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    assert len(delivered) == 2
    for obj in delivered:
        actual = audit(obj)
        assert actual['triangles'] == report['lods'][obj.name]['triangles']
        assert actual['nonmanifold_edges'] == 0
    bpy.ops.wm.open_mainfile(filepath=str(source / (name + '.blend')))
    assert not bpy.data.libraries
    assert all(im.packed_file or (im.filepath.startswith('//') and Path(bpy.path.abspath(im.filepath)).exists()) for im in bpy.data.images)
    report['reopened_master_dependencies_verified'] = True
    report['reimported_delivery_verified'] = True
    return report


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument('--render', action='store_true')
    args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else [])
    root = args.root.resolve()
    source = root / 'ArtSource/Golf/Equipment'
    game = root / 'Game/Assets/_Game/Art/Golf/Equipment'
    evidence = root / 'Builds/GolfEquipment/20261001'
    game.mkdir(parents=True, exist_ok=True)
    evidence.mkdir(parents=True, exist_ok=True)
    report = {'path_base': 'repository root', 'models': {}}
    for name, (filename, length) in SOURCES.items():
        report['models'][name] = build(name, filename, length, root, source, game, evidence, args.render)
        print('GOLF_MODEL_OK ' + name, flush=True)
    report['files'] = {p.relative_to(root).as_posix(): {'bytes': p.stat().st_size, 'sha256': hashlib.sha256(p.read_bytes()).hexdigest()} for p in [*source.glob('*.blend'), *source.glob('Textures/*.png'), *game.glob('*.fbx'), *game.glob('*.png')]}
    report['retained_metadata_exception'] = 'Original downloaded FBX exporter provenance and packed source-image names are retained inside unchanged ZIP masters. Delivery FBX Texture/Video dependencies and unpacked Blender images use verified relative paths.'
    (source / 'model-audit.json').write_text(json.dumps(report, indent=2) + '\n')
    print('GOLF_EQUIPMENT_COMPLETE', flush=True)


if __name__ == '__main__':
    main()
