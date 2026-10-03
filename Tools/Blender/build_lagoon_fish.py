"""Author one portable, vertex-coloured lagoon fish and two mobile LODs.

Run through Blender with --background --factory-startup --python-exit-code 1
--python Tools/Blender/build_lagoon_fish.py. Optional -- --root <checkout>.
All persistent paths in the audit are relative to the repository root.
"""
import argparse
import hashlib
import json
import math
import sys
from pathlib import Path

import bpy
import bmesh
from mathutils import Vector


def colour(hex_value):
    def linear(v):
        return v / 12.92 if v < .04045 else ((v + .055) / 1.055) ** 2.4
    return tuple(linear(int(hex_value[i:i+2], 16) / 255) for i in (0, 2, 4)) + (1,)


GOLD = colour('F4B64E')
BELLY = colour('FFF0B8')
BACK = colour('347E88')
FIN = colour('E9854B')
EDGE = colour('345B70')


def mix(a, b, t):
    return tuple(x * (1-t) + y*t for x, y in zip(a, b))


def make_mesh(name, verts, faces, colors):
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    attr = mesh.color_attributes.new(name='Color', type='FLOAT_COLOR', domain='POINT')
    for item, c in zip(attr.data, colors):
        item.color = c
    mesh.color_attributes.active_color = attr
    for p in mesh.polygons:
        p.use_smooth = True
    return obj


def body(name, rings, sides):
    profile = [(-.31, .023, .033), (-.25, .065, .095), (-.13, .112, .157),
               (.03, .137, .19), (.19, .125, .167), (.33, .088, .117),
               (.43, .05, .071), (.495, .015, .023)]
    verts, faces, colors = [], [], []
    for j in range(rings+1):
        x = -.31 + .805*j/rings
        index = next((i for i in range(len(profile)-1) if profile[i+1][0] >= x), len(profile)-2)
        left, right = profile[index:index+2]
        t = (x-left[0])/(right[0]-left[0])
        t = t*t*(3-2*t)
        width = left[1]*(1-t)+right[1]*t
        height = left[2]*(1-t)+right[2]*t
        for i in range(sides):
            angle = math.tau*i/sides
            y, z = width*math.cos(angle), height*math.sin(angle)
            verts.append((x, y, z))
            up = math.sin(angle)
            c = mix(GOLD, BELLY, max(0, -up)**.6)
            c = mix(c, BACK, max(0, min(1, (up-.23)/.68)))
            # A warm lateral stripe and restrained rear scale highlights.
            if abs(up-.03) < .16:
                c = mix(c, colour('FFE4A0'), .42)
            if x < .22:
                c = mix(c, BELLY, .06*(.5+.5*math.sin(x*95+angle*8)))
            colors.append(c)
    for j in range(rings):
        for i in range(sides):
            a = j*sides+i; b = j*sides+(i+1)%sides
            faces.append((a, b, b+sides, a+sides))
    faces += [tuple(reversed(range(sides))), tuple(rings*sides+i for i in range(sides))]
    return make_mesh(name, verts, faces, colors)


def fin(name, outline, thickness, base_color=FIN):
    # Closed thin fins, with a raised central ridge and darker perimeter.
    center = Vector((0, 0, 0))
    for p in outline:
        center += Vector(p)
    center /= len(outline)
    verts = [tuple(Vector(p)+Vector((0, thickness, 0))) for p in outline]
    verts += [tuple(Vector(p)-Vector((0, thickness, 0))) for p in outline]
    verts += [tuple(center+Vector((0, thickness*2.3, 0))), tuple(center-Vector((0, thickness*2.3, 0)))]
    n = len(outline); faces = []
    for i in range(n):
        k = (i+1)%n
        faces.extend(((2*n, i, k), (2*n+1, n+k, n+i), (i, n+i, n+k, k)))
    return make_mesh(name, verts, faces, [mix(base_color, EDGE, .32)]*(n*2)+[base_color]*2)


def sphere(name, location, scale, tint, segments):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=max(6, segments//2), location=location)
    obj = bpy.context.object;obj.name=name;obj.scale=scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    attr=obj.data.color_attributes.new(name='Color', type='FLOAT_COLOR', domain='POINT')
    for c in attr.data:c.color=tint
    for p in obj.data.polygons:p.use_smooth=True
    return obj


def assemble(name, rings, sides, eye_segments):
    objects=[body('Body', rings, sides)]
    objects.append(fin('Forked tail', [(-.28,0,.026),(-.50,0,.178),(-.463,0,.067),
                  (-.423,0,0),(-.463,0,-.067),(-.50,0,-.178),(-.28,0,-.026)], .008))
    objects.append(fin('Dorsal fin', [(.22,0,.142),(.095,0,.287),(-.055,0,.266),
                  (-.22,0,.115),(-.10,0,.139)], .006, BACK))
    objects.append(fin('Anal fin', [(.02,0,-.175),(-.06,0,-.239),(-.21,0,-.112)], .005))
    for side in (-1,1):
        objects.append(fin('Pectoral fin',[(.20,side*.104,-.03),(.04,side*.215,-.125),
                       (-.035,side*.18,-.083),(.11,side*.11,-.025)], .004))
        objects.append(sphere('Eye rim',(.354,side*.075,.05),(.043,.017,.044),colour('EFCE7B'),eye_segments))
        objects.append(sphere('Eye white',(.359,side*.087,.053),(.032,.011,.034),colour('FFF5D9'),eye_segments))
        objects.append(sphere('Eye pupil',(.371,side*.096,.055),(.020,.008,.024),colour('132F3D'),eye_segments))
        objects.append(sphere('Eye glint',(.375,side*.103,.067),(.006,.003,.007),colour('FFFFFF'),max(8,eye_segments//2)))
        # Gill is a narrow dark curved strip seated on each side of the head.
        curve=[]
        for j in range(9):
            z=-.075+j*.02;x=.254-.035*math.sin(math.pi*j/8)
            width=.126*math.sqrt(max(.1,1-(z/.155)**2))
            curve.extend(((x-.003,side*(width+.002),z),(x+.003,side*(width+.002),z)))
        faces=[(j*2,j*2+1,j*2+3,j*2+2) for j in range(8)]
        gill=make_mesh('Gill marking',curve,faces,[colour('A15F32')]*len(curve))
        objects.append(gill)
    objects.append(sphere('Small mouth',(.496,0,-.005),(.005,.016,.006),colour('875139'),max(8,eye_segments//2)))
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:obj.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.object.join();obj=bpy.context.object;obj.name=name
    # Blender -Y becomes Unity +Z; all three prefabs use this same one-metre fish.
    obj.rotation_euler.z=-math.pi/2
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
    return obj


def main():
    args=argparse.ArgumentParser();args.add_argument('--root',type=Path)
    opts=args.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    root=(opts.root or Path(__file__).resolve().parents[2]).resolve()
    source=root/'ArtSource/Fishing/LagoonFish';delivery=root/'Game/Assets/_Game/Art/Fishing/Fish'
    source.mkdir(parents=True,exist_ok=True);delivery.mkdir(parents=True,exist_ok=True)
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    bpy.context.preferences.filepaths.save_version=0
    mat=bpy.data.materials.new('LagoonFish_VertexColor');mat.use_nodes=True
    nodes=mat.node_tree.nodes;bsdf=nodes.get('Principled BSDF');vc=nodes.new('ShaderNodeVertexColor');vc.layer_name='Color'
    mat.node_tree.links.new(vc.outputs['Color'],bsdf.inputs['Base Color']);bsdf.inputs['Roughness'].default_value=.34
    master=assemble('LagoonFish_Master',64,48,32)
    lod0=assemble('LagoonFish_LOD0',30,24,16);lod1=assemble('LagoonFish_LOD1',16,12,8)
    record={"base":"repository root","length_metres":1.0,"prefab_lengths_metres":[.4,.9,1.8],"meshes":[]}
    for obj in (master,lod0,lod1):
        obj.data.materials.clear();obj.data.materials.append(mat)
        obj.data.calc_loop_triangles()
        assert all(math.isfinite(c) for v in obj.data.vertices for c in v.co)
        assert all(t.area>1e-12 for t in obj.data.loop_triangles)
        record['meshes'].append(dict(name=obj.name,vertices=len(obj.data.vertices),triangles=len(obj.data.loop_triangles)))
    bpy.ops.object.select_all(action='DESELECT')
    for obj in (lod0,lod1):obj.select_set(True)
    fbx=delivery/'LagoonFish.fbx'
    bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',
        bake_anim=False,add_leaf_bones=False,path_mode='RELATIVE',use_mesh_modifiers=True,colors_type='LINEAR',bake_space_transform=True)
    lod0.hide_set(True);lod0.hide_render=True;lod1.hide_set(True);lod1.hide_render=True
    bpy.ops.wm.save_as_mainfile(filepath=str(source/'LagoonFish.blend'))
    # Format-aware validation of saved native dependencies and FBX external paths.
    bpy.ops.wm.open_mainfile(filepath=str(source/'LagoonFish.blend'))
    assert not bpy.data.libraries
    assert not [i for i in bpy.data.images if i.source=='FILE' and not i.packed_file]
    from io_scene_fbx import parse_fbx
    tree,_=parse_fbx.parse(str(fbx))
    dependencies=[]
    def inspect(node):
        if node.id in (b'Filename',b'RelativeFilename',b'FileName'):
            dependencies.extend(p.decode('utf8') for p in node.props if isinstance(p,bytes))
        for child in node.elems:inspect(child)
    inspect(tree)
    assert not dependencies, dependencies
    record['external_dependencies']=dependencies
    record['sha256']={p.relative_to(root).as_posix():hashlib.sha256(p.read_bytes()).hexdigest()
                      for p in (fbx,source/'LagoonFish.blend')}
    (source/'model-audit.json').write_text(json.dumps(record,indent=2)+'\n',encoding='utf8')
    print('LAGOON_FISH_EXPORT_OK '+json.dumps(record['meshes']))


if __name__=='__main__':main()
