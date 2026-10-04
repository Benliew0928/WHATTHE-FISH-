"""Derive the supplied rounded golf ball's two mobile LODs and retained master.

Run with Python+bpy or Blender --background --python; accepts --root and --render.
All paths resolve from this script or an explicit checkout root, not the caller.
"""
import argparse, hashlib, json, math, re, sys
from pathlib import Path
import bpy, numpy as np
from mathutils import Vector
from io_scene_fbx import import_fbx, parse_fbx
from build_golf_equipment import audit, select, sphere

RADIUS=.0215
TARGETS=(16000,720)

def prepare(root, render=False):
    root=Path(root).resolve();source=root/'ArtSource/Golf/Ball'
    game=root/'Game/Assets/_Game/Art/Golf/Equipment';out=root/'Builds/GolfBallRoundedQA/Art'
    out.mkdir(parents=True,exist_ok=True);game.mkdir(parents=True,exist_ok=True)
    original=source/'Originals/golf-ball-rounded.fbx'
    # The supplied exporter uses a double for Visibility. Blender expects a
    # Visibility property; both describe the same value. Source stays untouched.
    original_visibility=import_fbx.elem_props_get_visibility
    def visibility(props,name,default=None):
        node=import_fbx.elem_props_find_first(props,name)
        if node is not None and node.props[1] in (b'Number',b'double'):
            value=float(node.props[4]);assert math.isfinite(value);return value
        return original_visibility(props,name,default)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.preferences.filepaths.save_version=0
    import_fbx.elem_props_get_visibility=visibility
    try:bpy.ops.import_scene.fbx(filepath=str(original))
    finally:import_fbx.elem_props_get_visibility=original_visibility
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];assert len(meshes)==1
    high=meshes[0];select(high);bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    original_data=audit(high);xyz=np.array([tuple(v.co) for v in high.data.vertices])
    center=(xyz.min(0)+xyz.max(0))/2;scale=RADIUS/np.linalg.norm(xyz-center,axis=1).max()
    for v in high.data.vertices:v.co=(v.co-Vector(center))*scale
    high.data.update();high.name='RoundedBall_Master';bpy.context.view_layer.update()
    high.data.materials.clear();mat=bpy.data.materials.new('Ivory_SoftGloss');mat.use_nodes=True
    mat.diffuse_color=(.978,.956,.910,1)
    shader=mat.node_tree.nodes.get('Principled BSDF');shader.inputs['Base Color'].default_value=mat.diffuse_color;shader.inputs['Roughness'].default_value=.30
    high.data.materials.append(mat);lods=[];report=[]
    for level,target in enumerate(TARGETS):
        if level==0:
            obj=high.copy();obj.data=high.data.copy();bpy.context.scene.collection.objects.link(obj);obj.name='Ball_LOD0';select(obj)
            modifier=obj.modifiers.new('Mobile reduction','DECIMATE');modifier.ratio=target/original_data['triangles'];bpy.ops.object.modifier_apply(modifier=modifier.name)
        else:
            # Below 1.2% screen height the dimples cannot be resolved. Preserve
            # a round silhouette instead of decimating them into angular dents.
            obj=sphere('Ball_LOD1',24,16);obj.data.materials.append(mat)
        # Quadric simplification can move vertices outside the original sphere.
        # Keep that outer envelope aligned with the unchanged physics radius.
        capped=0
        for v in obj.data.vertices:
            if v.co.length>RADIUS:v.co*=RADIUS/v.co.length;capped+=1
        for p in obj.data.polygons:p.use_smooth=True
        if level==0 and obj.data.has_custom_normals:obj.data.normals_split_custom_set([(0,0,0)]*len(obj.data.loops))
        obj.data.update();bpy.context.view_layer.update();data=audit(obj)
        assert data['nonmanifold_edges']==0 and len(data['components'])==1
        assert abs(data['triangles']-target)<=4
        points=np.array([tuple(v.co) for v in obj.data.vertices]);radii=np.linalg.norm(points,axis=1)
        assert radii.max()<=RADIUS+1e-6 and radii.max()>=RADIUS-.0001
        assert np.max(np.abs((points.min(0)+points.max(0))/2))<.0001
        assert np.linalg.det(np.array(obj.matrix_world)[:3,:3])>0
        report.append(dict(level=level,envelope_constrained_vertices=capped,radial_range=[float(radii.min()),float(radii.max())],**data));lods.append(obj)
    # The master and active delivery contain no textures or external libraries.
    for obj in lods:obj.hide_render=True;obj.hide_set(True)
    for image in list(bpy.data.images):
        assert image.users==0;bpy.data.images.remove(image)
    bpy.data.orphans_purge(do_recursive=True)
    master=source/'RoundedBall.blend';bpy.ops.wm.save_as_mainfile(filepath=str(master),compress=True,check_existing=False)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in lods:obj.hide_set(False);obj.select_set(True);obj.hide_render=False
    bpy.context.view_layer.objects.active=lods[0]
    fbx=game/'Ball.fbx'
    bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,path_mode='RELATIVE',embed_textures=False,add_leaf_bones=False)
    tree,_=parse_fbx.parse(str(fbx));historical=[]
    def dependencies(node,path=()):
        path=path+(node.id.decode(),)
        assert node.id not in (b'Texture',b'Video')
        for value in node.props:
            if isinstance(value,bytes) and re.search(rb'(?<![a-zA-Z0-9:])[A-Za-z]:[/\\]|^/(Users|home|root|mnt)/',value):
                assert path[0]=='FBXHeaderExtension' and 'SceneInfo' in path,path
                historical.append('/'.join(path))
        for child in node.elems:dependencies(child,path)
    for node in tree.elems:dependencies(node)
    summary=dict(path_base='repository root',source=original_data,radius_metres=RADIUS,normalized_master=audit(high),lods=report,external_images=0,external_libraries=0,inactive_exporter_provenance_fields=sorted(set(historical)),files=[dict(path=p.relative_to(root).as_posix(),bytes=p.stat().st_size,sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in (original,master,fbx)])
    (source/'delivery-audit.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf8')
    if render:
        high.hide_render=True;scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24
        scene.render.resolution_x=720;scene.render.resolution_y=720;scene.render.resolution_percentage=100
        scene.world=bpy.data.worlds.new('Ball review');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.10,.13,.16,1)
        for pos,energy in [((-.05,-.05,.065),.5),((.05,.03,.04),.20)]:
            bpy.ops.object.light_add(type='AREA',location=pos);bpy.context.object.data.energy=energy;bpy.context.object.data.size=.05
            bpy.context.object.rotation_euler=(-bpy.context.object.location).to_track_quat('-Z','Y').to_euler()
        bpy.ops.object.camera_add(location=(.035,-.075,.030));camera=bpy.context.object;camera.rotation_euler=(-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=.052;camera.data.clip_start=.0001;scene.camera=camera
        for obj in lods:obj.hide_render=True
        for level,obj in enumerate(lods):
            obj.hide_render=False;scene.render.filepath=str(out/('lod'+str(level)+'.png'));bpy.ops.render.render(write_still=True);obj.hide_render=True
    print('GOLF_ROUNDED_BALL_PREPARED '+json.dumps(dict(source_triangles=original_data['triangles'],lod_triangles=[d['triangles'] for d in report],fbx_bytes=fbx.stat().st_size)),flush=True)
    return summary

def main():
    args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else sys.argv[1:]
    parser=argparse.ArgumentParser();parser.add_argument('--root',type=Path);parser.add_argument('--render',action='store_true');options=parser.parse_args(args)
    prepare(options.root or Path(__file__).resolve().parents[2],options.render)

if __name__=='__main__':main()
