"""Retain the Midnight Iron master and derive a portable handheld mobile club.

Run with Python+bpy or Blender --background --python, optionally --root CHECKOUT.
Outputs resolve from this script, independently of the working directory.
"""
import argparse,hashlib,json,os,sys,zipfile
from pathlib import Path
import bpy,numpy as np
from mathutils import Vector
from build_golf_equipment import select,audit,material,save_image,delivery_image,portable_fbx

def main():
    args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else sys.argv[1:]
    parser=argparse.ArgumentParser();parser.add_argument('--root',type=Path)
    root=(parser.parse_args(args).root or Path(__file__).resolve().parents[2]).resolve()
    source=root/'ArtSource/Golf/Club';game=root/'Game/Assets/_Game/Art/Golf/Club';out=root/'Builds/GolfClubQA/Art'
    for p in [source/'Textures',game,out]:p.mkdir(parents=True,exist_ok=True)
    archive=source/'Originals/Meshy_AI_Midnight_Iron_1004033216_texture_fbx.zip';extracted=out/'Extracted'
    with zipfile.ZipFile(archive) as z:
        for e in z.infolist():assert (extracted/e.filename).resolve().is_relative_to(extracted.resolve())
        z.extractall(extracted)
    bpy.ops.wm.read_factory_settings(use_empty=True);bpy.context.preferences.filepaths.save_version=0
    bpy.ops.import_scene.fbx(filepath=str(next(extracted.rglob('*.fbx'))))
    high=next(o for o in bpy.context.scene.objects if o.type=='MESH');select(high)
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True);high.name='MidnightIron_Master'
    original=audit(high);lo=min(v.co.z for v in high.data.vertices);hi=max(v.co.z for v in high.data.vertices)
    grip=[v.co for v in high.data.vertices if v.co.z>hi-(hi-lo)*.10]
    centre=Vector((sum(v.x for v in grip)/len(grip),sum(v.y for v in grip)/len(grip),hi-(hi-lo)*.09))
    factor=1.02/(hi-lo)
    for v in high.data.vertices:v.co=(v.co-centre)*factor
    high.data.update()
    bottom=min(v.co.z for v in high.data.vertices);head=[v.co for v in high.data.vertices if v.co.z<bottom+.055]
    head_centre=sum(head,Vector())/len(head)
    maps={}
    for name,suffix in [('BaseColor','_texture.png'),('Normal','_texture_normal.png'),('Metallic','_texture_metallic.png'),('Roughness','_texture_roughness.png')]:
        image=bpy.data.images.load(str(next(extracted.rglob('*'+suffix))))
        if name!='BaseColor':image.colorspace_settings.name='Non-Color'
        save_image(image,source/'Textures'/('MidnightIron_'+name+'.png'));maps[name]=image
    high.data.materials.clear();high.data.materials.append(material('MidnightIron_Master',maps['BaseColor'],maps['Normal'],maps['Metallic'],maps['Roughness']))
    color=delivery_image(maps['BaseColor'],game/'MidnightIron_BaseColor.png',(512,512))
    normal=delivery_image(maps['Normal'],game/'MidnightIron_Normal.png',(512,512));normal.colorspace_settings.name='Non-Color'
    metal=maps['Metallic'].copy();metal.scale(256,256);rough=maps['Roughness'].copy();rough.scale(256,256)
    mask=bpy.data.images.new('MidnightIron_Mask',width=256,height=256,alpha=True);mask.colorspace_settings.name='Non-Color'
    m=np.asarray(metal.pixels[:]).reshape(-1,4);r=np.asarray(rough.pixels[:]).reshape(-1,4);pixels=np.zeros_like(m);pixels[:,0]=m[:,0];pixels[:,3]=1-r[:,0];mask.pixels=pixels.ravel();save_image(mask,game/'MidnightIron_Mask.png')
    delivery=material('MidnightIron_Delivery',color,normal,mask);lods=[];report=[]
    for level,target in enumerate((3600,1400,500)):
        obj=high.copy();obj.data=high.data.copy();bpy.context.scene.collection.objects.link(obj);obj.name='MidnightIron_LOD'+str(level);select(obj)
        mod=obj.modifiers.new('Mobile reduction','DECIMATE');mod.ratio=target/original['triangles'];bpy.ops.object.modifier_apply(modifier=mod.name)
        obj.data.materials.clear();obj.data.materials.append(delivery);report.append(dict(level=level,**audit(obj)));lods.append(obj)
    markers=[]
    for name,p in [('ClubGrip',(0,0,0)),('ClubTop',(0,0,.0918)),('ClubFace',(0,-.1,0)),('ClubHead',head_centre)]:
        obj=bpy.data.objects.new(name,None);bpy.context.scene.collection.objects.link(obj);obj.location=p;markers.append(obj)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in lods+markers:obj.select_set(True)
    bpy.context.view_layer.objects.active=lods[0]
    fbx=game/'MidnightIron.fbx';bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='RELATIVE',embed_textures=False)
    refs=portable_fbx(fbx)
    for image in list(bpy.data.images):
        if image.packed_file and not image.users:bpy.data.images.remove(image);continue
        if image.filepath and not image.packed_file:
            p=Path(bpy.path.abspath(image.filepath)).resolve()
            if p.is_file():image.filepath='//'+os.path.relpath(p,source).replace('\\','/')
    bpy.data.orphans_purge(do_recursive=True)
    for obj in lods:obj.hide_render=True;obj.hide_set(True)
    high.hide_render=False;scene=bpy.context.scene;scene.render.filepath='//../../../Builds/GolfClubQA/Art/master.png'
    master=source/'MidnightIron.blend';bpy.ops.wm.save_as_mainfile(filepath=str(master),compress=True,check_existing=False)
    report=dict(source=original,length_metres=1.02,grip_from_top_metres=.0918,head_marker=list(head_centre),lods=report,texture_refs=refs,
                images=[dict(name=im.name,path=im.filepath,packed=bool(im.packed_file)) for im in bpy.data.images],
                files=[dict(path=p.relative_to(root).as_posix(),bytes=p.stat().st_size,sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in [master,fbx,*sorted(game.glob('*.png'))]])
    (source/'delivery-audit.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
    (source/'provenance.json').write_text(json.dumps(dict(source='User-supplied Meshy Midnight Iron FBX archive',archive='Originals/'+archive.name,sha256=hashlib.sha256(archive.read_bytes()).hexdigest(),bytes=archive.stat().st_size),indent=2)+'\n',encoding='utf8')
    high.hide_render=True
    scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=650;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
    world=bpy.data.worlds.new('Midnight Iron review');world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.42,.47,.52,1);scene.world=world
    bpy.ops.object.light_add(type='AREA',location=(1,-2,2));bpy.context.object.data.energy=500;bpy.context.object.data.size=3
    bpy.ops.object.camera_add();camera=bpy.context.object;scene.camera=camera;camera.data.type='ORTHO';camera.data.ortho_scale=1.3
    target=Vector((0,0,-.40));camera.location=(.30,-2,.20);camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
    for i,obj in enumerate(lods):obj.hide_render=False;scene.render.filepath=str(out/('lod'+str(i)+'.png'));bpy.ops.render.render(write_still=True);obj.hide_render=True
    print(json.dumps(report,indent=2))

if __name__=='__main__':main()
