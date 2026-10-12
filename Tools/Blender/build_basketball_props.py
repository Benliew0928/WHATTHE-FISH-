"""Original editable props and matching transparent HUD renders.
Run Blender --background --python Tools/Blender/build_basketball_props.py.
All paths resolve from this script; no external images or libraries are used.
"""
import argparse, hashlib, json, math, sys
from pathlib import Path
import bpy
from mathutils import Vector

args=argparse.ArgumentParser();args.add_argument('--root',type=Path)
opts=args.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
root=opts.root or Path(__file__).resolve().parents[2]
master=root/'ArtSource/Basketball/Props';delivery=root/'Game/Assets/_Game/Resources/BasketballProps'
master.mkdir(parents=True,exist_ok=True);delivery.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
palette={'Ivory':(.95,.90,.70,1),'Gold':(1,.55,.045,1),'Orange':(.92,.24,.025,1),'Ink':(.018,.065,.075,1),'Mint':(.07,.78,.52,1),'Blue':(.02,.40,1,1),'Black':(.003,.006,.02,1)}
mats={}
for name,col in palette.items():
 m=bpy.data.materials.new('Prop'+name);m.diffuse_color=col;m.use_nodes=True
 bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=col;bs.inputs['Roughness'].default_value=.32
 if name in ('Gold','Blue'):bs.inputs['Metallic'].default_value=.3
 mats[name]=m
objects=[]
def finish(o,name,mat):
 o.name=name;o.data.materials.append(mats[mat]);objects.append(o)
 for p in o.data.polygons:p.use_smooth=True
 return o
def ball(name,pos,scale,mat):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,location=pos);o=bpy.context.object;o.scale=scale;return finish(o,name,mat)
def cube(name,pos,scale,mat,bevel=.025):
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 mod=o.modifiers.new('Soft toy edges','BEVEL');mod.width=bevel;mod.segments=3;bpy.ops.object.modifier_apply(modifier=mod.name)
 mod=o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');bpy.ops.object.modifier_apply(modifier=mod.name);return finish(o,name,mat)
def tube(name,points,radius,mat,closed=False):
 c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';c.resolution_u=1;c.bevel_depth=radius;c.bevel_resolution=2
 sp=c.splines.new('POLY');sp.points.add(len(points)-1)
 for p,v in zip(sp.points,points):p.co=(*v,1)
 sp.use_cyclic_u=closed;o=bpy.data.objects.new(name,c);bpy.context.collection.objects.link(o);bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.convert(target='MESH');o.select_set(False);return finish(o,name,mat)
def ring(name,center,radius,width,mat,plane='XZ'):
 pts=[]
 for i in range(64):
  a=i*math.tau/64;pts.append((center[0]+radius*math.cos(a),center[1]+(radius*math.sin(a) if plane=='XY' else 0),center[2]+(radius*math.sin(a) if plane=='XZ' else 0)))
 return tube(name,pts,width,mat,True)
def shape(name,coords,y,depth,mat):
 verts=[(x,y+side*depth/2,z) for side in (-1,1) for x,z in coords];n=len(coords)
 faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o)
 bpy.context.view_layer.objects.active=o;mod=o.modifiers.new('Rounded edges','BEVEL');mod.width=.012;mod.segments=2;bpy.ops.object.modifier_apply(modifier=mod.name);return finish(o,name,mat)
def basketball(center,r=.22):
 ball('Basketball',center,(r,r,r),'Orange')
 for axis in range(3):
  pts=[]
  for i in range(64):
   a=i*math.tau/64;p=[0,0,0];p[axis%3]=(r+.002)*math.cos(a);p[(axis+1)%3]=(r+.002)*math.sin(a);pts.append(tuple(p[k]+center[k] for k in range(3)))
  tube('Rubber seams',pts,.008,'Ink',True)
def lockon():
 basketball((0,0,0),.205);ring('Golden aiming ring',(0,0,0),.32,.043,'Gold')
 for i in range(4):
  a=i*math.pi/2;o=cube('Aim tick',(.32*math.cos(a),-.018,.32*math.sin(a)),(.12,.09,.055),'Ivory',.017);o.rotation_euler.y=-a
 shape('Comet tail',[(-.08,-.18),(-.28,-.28),(-.52,-.32),(-.37,-.18),(-.23,-.05)],.1,.07,'Gold')
def turbo():
 # Toe points left; both sides carry a lightning bolt and layered wing.
 ball('Cushioned sole',(-.045,0,-.21),(.42,.18,.105),'Ivory');ball('Mint outsole',(-.045,0,-.258),(.40,.175,.046),'Mint')
 ball('Rounded toe',(-.20,0,-.095),(.27,.16,.14),'Ivory');ball('Shoe body',(.10,0,-.005),(.235,.155,.245),'Mint')
 ball('Ankle cuff',(.18,0,.15),(.155,.148,.12),'Ivory');ball('Inner cuff',(.18,0,.195),(.104,.100,.06),'Ink')
 tongue=cube('Tongue',(-.04,-.005,.065),(.18,.24,.11),'Ivory',.04);tongue.rotation_euler.y=.3
 for z in (-.02,.04,.10):tube('Laces',[(-.13,-.135,z),(-.13,0,z+.025),(-.13,.135,z)],.012,'Ivory')
 for side in (-1,1):
  shape('Lightning badge',[(.045,.06),(-.04,-.055),(.035,-.045),(-.015,-.17),(.15,-.01),(.07,-.015)],side*.155,.018,'Gold')
  for j in range(3):
   wing=ball('Wing feather',(.28+j*.037,side*(.18+j*.014),.06+j*.082),(.17-j*.018,.031,.06),'Ivory');wing.rotation_euler.y=-.43-j*.08
def portal():
 ring('Portal frame',(0,0,0),.31,.042,'Blue');ring('Inner light',(0,-.025,0),.267,.012,'Ivory')
 ball('Black portal centre',(0,.035,0),(.268,.035,.268),'Black');basketball((-.045,-.11,-.065),.14)
 for i in range(4):
  a=i*math.pi/2+.3;ball('Orbit mote',(.40*math.cos(a),0,.40*math.sin(a)),(.021,.021,.045),'Blue')
 tube('Suction curl',[(.27,-.03,-.14),(.25,-.13,-.24),(.11,-.22,-.22),(-.01,-.21,-.15)],.014,'Blue')

models=[]
for name,fn in [('LockOn',lockon),('Turbo',turbo),('Portal',portal)]:
 objects=[];fn();bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();model=bpy.context.object;model.name=name
 bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 # Portable FBX contains only geometry and plain colours, no image dependencies.
 bpy.ops.export_scene.fbx(filepath=str(delivery/(name+'.fbx')),use_selection=True,object_types={'MESH'},add_leaf_bones=False,bake_anim=False,path_mode='RELATIVE',axis_forward='-Z',axis_up='Y')
 models.append(model);model.hide_render=True

scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=48;scene.cycles.use_denoising=True
scene.render.resolution_x=scene.render.resolution_y=512;scene.render.resolution_percentage=100;scene.render.film_transparent=True
scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA';scene.view_settings.view_transform='Standard'
scene.world.color=(.23,.23,.23)
bpy.ops.object.camera_add(location=(1.05,-3.2,1.25));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=1.05;scene.camera=cam
for loc,energy,size in [((-3,-4,5),450,4),((3,-2,2),250,3),((0,3,3),400,3)]:
 bpy.ops.object.light_add(type='AREA',location=loc);light=bpy.context.object;light.data.energy=energy;light.data.shape='DISK';light.data.size=size;light.rotation_euler=(-light.location).to_track_quat('-Z','Y').to_euler()
for model in models:
 model.hide_render=False;scene.render.filepath=str(master/(model.name+'-icon-master.png'));bpy.ops.render.render(write_still=True)
 # Same quality master retained outside Unity; Android importer delivers at 256.
 (delivery/(model.name+'.png')).write_bytes((master/(model.name+'-icon-master.png')).read_bytes());model.hide_render=True
for m in models:m.hide_render=False
scene.render.filepath='//renders/'
bpy.context.preferences.filepaths.save_version=0
assert all(not image.filepath or image.filepath.startswith('//') for image in bpy.data.images if image.name not in ('Render Result','Viewer Node'))
assert not list(bpy.data.libraries)
bpy.ops.wm.save_as_mainfile(filepath=str(master/'BasketballProps.blend'))
report={'base':'repository root','dependencies':[],'models':[{'name':m.name,'vertices':len(m.data.vertices),'triangles':sum(len(p.vertices)-2 for p in m.data.polygons),'materials':len(m.data.materials)} for m in models],'files':{str(p.relative_to(root)).replace('\\','/'):{'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in list(delivery.glob('*.fbx'))+list(delivery.glob('*.png'))+[master/'BasketballProps.blend']}}
(master/'integrity.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('BASKETBALL_PROPS_ART_OK',json.dumps(report['models']))
