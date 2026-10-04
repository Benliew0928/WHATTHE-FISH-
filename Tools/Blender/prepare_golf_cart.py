"""Derive a portable golf-cart master and three mobile LODs from the retained ZIP.

Run with Blender --background --python this_file, or Python with bpy installed.
Optional arguments: --root CHECKOUT. Outputs resolve from this script, not cwd.
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
from mathutils import Vector
from build_golf_equipment import material, portable_fbx, select, save_image, delivery_image
from golf_cart_wheels import split_wheels, RIM_RADIUS, SEGMENTS, TREAD_ROWS, TREAD_DEPTH


def topology(obj):
    obj.data.calc_loop_triangles()
    bm = bmesh.new(); bm.from_mesh(obj.data)
    result = dict(vertices=len(obj.data.vertices), triangles=len(obj.data.loop_triangles),
                  nonmanifold_edges=sum(not edge.is_manifold for edge in bm.edges),
                  degenerate_triangles=sum(t.area < 1e-14 for t in obj.data.loop_triangles),
                  finite_uvs=all(math.isfinite(n) for uv in obj.data.uv_layers.active.data for n in uv.uv))
    bm.free()
    assert result['finite_uvs'] and result['degenerate_triangles'] == 0, result
    return result


def main():
    arguments = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else sys.argv[1:]
    parser = argparse.ArgumentParser(); parser.add_argument('--root', type=Path)
    root = (parser.parse_args(arguments).root or Path(__file__).resolve().parents[2]).resolve()
    source = root / 'ArtSource/Golf/Cart'; game = root / 'Game/Assets/_Game/Art/Golf/Cart'
    evidence = root / 'Builds/GolfCartQA/Art'
    for folder in (source / 'Textures', game, evidence): folder.mkdir(parents=True, exist_ok=True)
    extracted = evidence / 'Source'
    archive = source / 'Originals/Meshy_AI_Golf_Cart_1003101246_texture_fbx.zip'
    with zipfile.ZipFile(archive) as package:
        for entry in package.infolist():
            assert (extracted / entry.filename).resolve().is_relative_to(extracted.resolve())
        package.extractall(extracted)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.preferences.filepaths.save_version = 0
    bpy.ops.import_scene.fbx(filepath=str(next(extracted.rglob('*.fbx'))))
    objects = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    assert len(objects) == 1
    high = objects[0]; select(high)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    # Source front is -X. Rotate into Blender +Y; the exported marker lets Unity
    # verify forward independently of FBX axis conversion.
    high.rotation_euler.z = -math.pi / 2
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    bounds = [Vector(p) for p in high.bound_box]
    lo = Vector([min(p[i] for p in bounds) for i in range(3)])
    hi = Vector([max(p[i] for p in bounds) for i in range(3)])
    scale = 2.85 / (hi.y - lo.y)
    origin = Vector(((hi.x + lo.x) / 2, (hi.y + lo.y) / 2, lo.z))
    for vertex in high.data.vertices: vertex.co = (vertex.co - origin) * scale
    high.data.update(); high.name = 'GolfCart_MeshyMaster'
    maps = {}
    for name, suffix in {'BaseColor':'_texture.png', 'Normal':'_texture_normal.png',
                         'Metallic':'_texture_metallic.png', 'Roughness':'_texture_roughness.png'}.items():
        image = bpy.data.images.load(str(next(extracted.rglob('*' + suffix))))
        if name != 'BaseColor': image.colorspace_settings.name = 'Non-Color'
        save_image(image, source / 'Textures' / ('GolfCart_' + name + '.png')); maps[name] = image
    high.data.materials.clear()
    high.data.materials.append(material('GolfCart_Master', maps['BaseColor'], maps['Normal'], maps['Metallic'], maps['Roughness']))
    report = {'blender':bpy.app.version_string, 'source':topology(high), 'length_metres':2.85, 'lods':[],
              'wheel_shape':'Thick rounded black tyres with geometric chevron channels and equal 38 cm cream six-spoke rims; welded beads and fixed chassis axle mounts; rear-well fragments removed',
              'rim_diameter_metres':RIM_RADIUS*2, 'wheel_angular_segments':list(SEGMENTS),
              'tread_rows':list(TREAD_ROWS), 'tread_depth_metres':TREAD_DEPTH,
              'driver_hip_height_metres':1.04}
    color = delivery_image(maps['BaseColor'], game / 'GolfCart_BaseColor.png', (1024,1024))
    normal = delivery_image(maps['Normal'], game / 'GolfCart_Normal.png', (1024,1024))
    normal.colorspace_settings.name = 'Non-Color'
    metal = maps['Metallic'].copy(); metal.scale(512,512)
    rough = maps['Roughness'].copy(); rough.scale(512,512)
    mask = bpy.data.images.new('GolfCart_Mask',width=512,height=512,alpha=True)
    mask.colorspace_settings.name='Non-Color'
    m=list(metal.pixels); r=list(rough.pixels); packed=[]
    for index in range(0,len(m),4): packed.extend((m[index],0,0,1-r[index]))
    mask.pixels=packed; save_image(mask,game/'GolfCart_Mask.png')
    delivery=material('GolfCart_Delivery',color,normal,mask)
    delivery.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.38
    lods=[]
    for level, target in enumerate((24000,8000,2500)):
        obj=high.copy(); obj.data=high.data.copy(); bpy.context.scene.collection.objects.link(obj)
        obj.name='GolfCart_LOD'+str(level); select(obj)
        reduction=obj.modifiers.new('Mobile LOD reduction','DECIMATE'); reduction.ratio=target/report['source']['triangles']
        bpy.ops.object.modifier_apply(modifier=reduction.name)
        obj.data.materials.clear(); obj.data.materials.append(delivery)
        parts=split_wheels(obj,color,level)
        report['lods'].append({'level':level,'triangles':sum(topology(part)['triangles'] for part in parts),
                              'parts':[{'name':part.name,**topology(part)} for part in parts]})
        lods.extend(parts)
    markers=[]
    for name, point in [('GolfCart_Front',(0,1.15,.6)),('GolfCart_Seat',(-.32,-.20,1.04))]:
        marker=bpy.data.objects.new(name,None); bpy.context.scene.collection.objects.link(marker)
        marker.location=point; markers.append(marker)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in lods+markers: obj.select_set(True)
    bpy.context.view_layer.objects.active=lods[0]
    output=game/'GolfCart.fbx'
    bpy.ops.export_scene.fbx(filepath=str(output),use_selection=True,object_types={'MESH','EMPTY'},
                             apply_unit_scale=True,axis_forward='-Z',axis_up='Y',
                             bake_anim=False,path_mode='RELATIVE',embed_textures=False)
    report['delivery_texture_refs']=portable_fbx(output)
    # The master retains full-quality geometry, all source maps and delivery LODs.
    # Every active image dependency is relative to the saved master.
    for image in list(bpy.data.images):
        if image.packed_file and image.users==0: bpy.data.images.remove(image); continue
        if image.filepath and not image.packed_file:
            path=Path(bpy.path.abspath(image.filepath)).resolve()
            if path.is_file(): image.filepath='//'+str(Path(__import__('os').path.relpath(path,source))).replace('\\','/')
    bpy.data.orphans_purge(do_recursive=True)
    for obj in lods: obj.hide_render=True; obj.hide_set(True)
    high.hide_render=False; high.hide_set(False)
    bpy.context.scene.render.filepath='//../../../Builds/GolfCartQA/Art/master.png'
    bpy.ops.wm.save_as_mainfile(filepath=str(source/'GolfCart.blend'),check_existing=False,compress=True)
    report['dependencies']=[{'image':im.name,'path':im.filepath,'packed':bool(im.packed_file)} for im in bpy.data.images]
    for item in report['dependencies']:
        assert item['packed'] or item['path'].startswith('//'), item
    report['files']=[{'path':str(p.relative_to(root)).replace('\\','/'),'bytes':p.stat().st_size,
                      'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in [source/'GolfCart.blend',output,*sorted(game.glob('*.png'))]]
    (source/'delivery-audit.json').write_text(json.dumps(report,indent=2)+'\n')
    (evidence/'topology.json').write_text(json.dumps(report,indent=2)+'\n')
    # Comparable close and side views of the actual derived model.
    high.hide_render=True
    for obj in lods: obj.hide_render=True
    scene=bpy.context.scene; scene.render.engine='CYCLES'; scene.cycles.samples=24
    scene.render.resolution_x=1100; scene.render.resolution_y=900; scene.render.resolution_percentage=100
    world=bpy.data.worlds.new('Golf cart review'); world.use_nodes=True
    world.node_tree.nodes['Background'].inputs[0].default_value=(.55,.65,.68,1)
    world.node_tree.nodes['Background'].inputs[1].default_value=.7; scene.world=world
    bpy.ops.object.light_add(type='AREA',location=(2,-3,6)); bpy.context.object.data.energy=1100; bpy.context.object.data.size=5
    bpy.ops.object.camera_add(); camera=bpy.context.object; scene.camera=camera; camera.data.lens=48
    centre=Vector((0,0,1.25))
    for level in range(3):
        parts=[obj for obj in lods if obj.name.endswith('LOD'+str(level))]
        for obj in parts: obj.hide_render=False
        camera.location=(4,5,3.7); camera.rotation_euler=(centre-camera.location).to_track_quat('-Z','Y').to_euler()
        scene.render.filepath=str(evidence/('lod'+str(level)+'-front.png')); bpy.ops.render.render(write_still=True)
        if level==0:
            camera.location=(-4,-4,3); camera.rotation_euler=(centre-camera.location).to_track_quat('-Z','Y').to_euler()
            scene.render.filepath=str(evidence/'lod0-rear.png'); bpy.ops.render.render(write_still=True)
        for obj in parts: obj.hide_render=True
    print(json.dumps(report,indent=2),flush=True)


if __name__=='__main__': main()
