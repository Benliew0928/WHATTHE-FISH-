"""Build shared delivery vegetation; never overwrite the authored island masters.

Blender --background --factory-startup --python-exit-code 1 --python this_file
The original deterministic coast generator is replayed with geometry calls
replaced by placement records, consuming the exact same random numbers.
"""
import ast, copy, json, math, random
from pathlib import Path
import bpy, bmesh
import numpy as np
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Game/Assets/_Game/Art/MobileVegetation'
OUT.mkdir(parents=True,exist_ok=True)
source=(ROOT/'Tools/Blender/build_coastal_islands.py').read_text()
tree=ast.parse(source)
functions={n.name:n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef))}
ns=dict(bpy=bpy,bmesh=bmesh,math=math,random=random,np=np,Vector=Vector,PI=math.pi,ico={},group='',batches={},materials={})
for n in tree.body:
    if isinstance(n,ast.Assign) and isinstance(n.value,ast.Call) and isinstance(n.value.func,ast.Name) and n.value.func.id=='material':
        ns[n.targets[0].id]=n.value.args[0].value
for name,node in functions.items():
    if name!='landscape':exec(compile(ast.Module(body=[node],type_ignores=[]),'<coastal-kit>','exec'),ns)

placements=[];coastal_grass=[]
def record(kind,name,x,y,z,scale,angle=0):
    placements.append(dict(sport=ns['group'],cluster=ns['group']+'__'+name,kind=kind,
        variant=len(placements)%({'Palm':4,'Plant':3,'Tuft':2}[kind]),
        position=dict(x=x,y=z,z=y),scale=scale,yaw=-math.degrees(angle)))
def palm(name,x,y,z,height,angle):
    record('Palm',name,x,y,z,height/12,angle)
    for _ in range(9):random.uniform(.48,.61)
def planting(name,x,y,z,scale=1):
    record('Plant',name,x,y,z,scale)
    for _ in range(6):random.uniform(.4,.8)
def tuft(name,x,y,z,scale):
    record('Tuft',name,x,y,z,scale)
    for _ in range(7):random.uniform(.4,.8)
def meadow(name,x,y,z,scale):
    coastal_grass.append(dict(sport=ns['group'],cell=name,position=dict(x=x,y=z,z=y),heights=[scale*random.uniform(.5,1) for _ in range(7)]))
original={key:ns[key] for key in ('palm','planting','tuft','meadow','blob','box','beam','rope')}
ns.update(palm=palm,planting=planting,tuft=tuft,meadow=meadow,blob=lambda *a,**k:None,box=lambda *a,**k:None,beam=lambda *a,**k:None,rope=lambda *a,**k:None)
landscape=copy.deepcopy(functions['landscape'])
start=next(i for i,n in enumerate(landscape.body) if isinstance(n,ast.Assign) and isinstance(n.targets[0],ast.Name) and n.targets[0].id=='count')
end=next(i for i,n in enumerate(landscape.body) if isinstance(n,ast.Assign) and isinstance(n.targets[0],ast.Name) and n.targets[0].id=='objects')
landscape.body=ast.parse('global group\ngroup=sport').body+landscape.body[start:end]
ast.fix_missing_locations(landscape)
exec(compile(ast.Module(body=[landscape],type_ignores=[]),'<coastal-placements>','exec'),ns)
random.seed(926)
ns['landscape']('Football',132,160,185)
ns['landscape']('Basketball',70,83,105)
(OUT/'coast-placements.json').write_text(json.dumps(dict(instances=placements),separators=(',',':')))

# Reuse the original leaf/trunk/flower authoring functions with fewer segments
# along each tiny leaflet. Keep all nine fronds and all paired leaflets.
ns.update(original)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
specs=json.loads((ROOT/'Game/Assets/_Game/Art/CoastalIslands/coast-manifest.json').read_text())['materials']
for spec in specs:
    material=bpy.data.materials.new(spec['name']);material.diffuse_color=(*spec['color_srgb'],1)
    ns['materials'][spec['name']]=material
counts=[]
for kind,variants in [('Palm',4),('Plant',3),('Tuft',2)]:
    for variant in range(variants):
        for lod,segments in enumerate((8,4,2)):
            node=copy.deepcopy(functions['frond'])
            for n in ast.walk(node):
                if isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='steps' for t in n.targets):n.value=ast.Constant(segments)
            ast.fix_missing_locations(node);exec(compile(ast.Module(body=[node],type_ignores=[]),'<mobile-frond>','exec'),ns)
            random.seed(940+variant);ns['group']='Mobile';ns['batches']={}
            name=f'{kind}_{variant}_LOD{lod}'
            if kind=='Palm':ns['palm'](name,0,0,0,12,0)
            elif kind=='Plant':ns['planting'](name,0,0,0,1)
            else:ns['tuft'](name,0,0,0,1)
            ob=next(iter(ns['batches'].values())).finish();ob.name=name;ob.data.name=name
            # The rounded flowers need more geometry nearby; reduce only their
            # distant representations, keeping the near flower mesh intact.
            if kind=='Plant' and lod:
                bpy.context.view_layer.objects.active=ob
                mod=ob.modifiers.new('Distant flower shape','DECIMATE');mod.ratio=.55 if lod==1 else .25
                bpy.ops.object.modifier_apply(modifier=mod.name)
            counts.append(dict(name=name,triangles=sum(len(p.vertices)-2 for p in ob.data.polygons)))
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.fbx(filepath=str(OUT/'CoastalVegetation.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='STRIP')

# The exact three source grass patches, stored once rather than baked hundreds
# of times. Runtime applies the authored placement and terrain height per blade.
kit=ast.parse((ROOT/'Tools/Blender/refined_island_kit.py').read_text())
grass=next(n for n in kit.body if isinstance(n,ast.FunctionDef) and n.name=='grass')
exec(compile(ast.Module(body=[grass],type_ignores=[]),'<source-grass>','exec'),ns)
prototypes=[]
class BladeWriter:
    def __init__(self):self.blades=[]
    def add(self,name,vertices,faces,material,smooth):
        self.blades.append(dict(p=[dict(x=float(v.x),y=float(v.z),z=float(v.y)) for v in vertices],light=material=='LeafLight'))
for variant in range(3):
    writer=BladeWriter();ns['grass'](writer,variant);prototypes.append(dict(blades=writer.blades))
(OUT/'grass-prototypes.json').write_text(json.dumps(dict(prototypes=prototypes,coastal=coastal_grass),separators=(',',':')))
(OUT/'vegetation-manifest.json').write_text(json.dumps(dict(placements=len(placements),palms=sum(p['kind']=='Palm' for p in placements),meshes=counts,source='Tools/Blender/build_coastal_islands.py; deterministic seed 926'),indent=2))
print('MOBILE_VEGETATION_COMPLETE',len(placements),'placements;',sum(c['triangles'] for c in counts),'shared triangles across all LODs',flush=True)
