"""Five-player lagoon: editable modular Blender source and independent FBX kit.

blender -b --factory-startup --python Tools/Blender/build_fishing_lagoon.py -- --render
Only writes Fishing/Lagoon output. Plan X/Y, height Z, metres.
"""
import bpy, bmesh, math, json, random, sys
import numpy as np
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT/'ArtSource/Fishing/Lagoon'
OUT = ROOT/'Game/Assets/_Game/Art/Fishing/Lagoon'
DOC = ROOT/'Docs/VisualDirection/Fishing/Blender'
for p in (SRC, OUT, DOC): p.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
for c in list(bpy.data.collections): bpy.data.collections.remove(c)
scene = bpy.context.scene
scene.unit_settings.system='METRIC'; scene.unit_settings.scale_length=1
random.seed(526); PI=math.pi
materials={}; specs=[]; modules={}; batches={}; placements=[]

def rgb(h): return np.array([int(h[i:i+2],16)/255 for i in (0,2,4)])
def linear(c): return c/12.92 if c<=.04045 else ((c+.055)/1.055)**2.4
def smooth(a,b,x):
    t=np.clip((x-a)/(b-a),0,1); return t*t*(3-2*t)
def outer(a): return 55+2.4*np.sin(3*a+.4)+1.5*np.cos(7*a)
def height(r,a):
    # A broad, level inner path; gentle grass dunes outside it, low sand at sea.
    rise=1.4+2.0*np.exp(-((r-45)/5.5)**2)*(0.65+.35*np.sin(a*5+.8)**2)
    z=1.2+(rise-1.2)*smooth(32,39,r)
    z=z*(1-smooth(outer(a)-6,outer(a)+.7,r))-.6*smooth(outer(a)-1,outer(a)+.7,r)
    return z*(smooth(26,28,r))-.6*(1-smooth(26,28,r))
def ground(x,y): return float(height(math.hypot(x,y),math.atan2(x,y)))

def texture(name,pixels):
    h,w=pixels.shape[:2]; im=bpy.data.images.new(name,width=w,height=h,alpha=False)
    rgba=np.ones((h,w,4),np.float32);rgba[:,:,:3]=pixels[:,:,None] if pixels.ndim==2 else pixels
    im.pixels.foreach_set(rgba.ravel());im.filepath_raw=str(OUT/(name+'.png'));im.file_format='PNG';im.save()
    bpy.data.images.remove(im);im=bpy.data.images.load(str(OUT/(name+'.png')));im.name=name;im.pack();return im

N=2048;yy,xx=np.mgrid[0:N,0:N]/(N-1)*140-70
rad=np.sqrt(xx*xx+yy*yy);ang=np.arctan2(xx,yy);edge=outer(ang)
pixels=np.broadcast_to(rgb('8CBF45'),(N,N,3)).copy()
variation=.025*np.sin(xx*.27)*np.sin(yy*.19)+.015*np.cos(xx*.71+yy*.15)
pixels+=variation[:,:,None]*np.array([1,.8,.25])
def paint(color,mask):
    global pixels
    pixels=pixels*(1-mask[:,:,None])+rgb(color)[None,None,:]*mask[:,:,None]
paint('F4DC9F',1-smooth(32,33.5,rad))
paint('EED39A',1-smooth(2,2.6,np.abs(rad-36)))
paint('F8E3AF',smooth(edge-5,edge-2,rad))
paint('EBCF91',(1-smooth(4.7,6.5,np.abs(xx)))*smooth(24,28,yy))
for bearing in (180,252,324,36,108):
    a=math.radians(bearing);side=np.abs(xx*math.cos(a)-yy*math.sin(a));along=xx*math.sin(a)+yy*math.cos(a)
    paint('F5DEAA',(1-smooth(2.9,3.6,side))*smooth(26,28,along)*(1-smooth(35.5,37.5,along)))
terrain_tex=texture('Lagoon_Terrain',np.clip(pixels,0,1));del xx,yy,rad,ang,edge,pixels
y,x=np.mgrid[0:512,0:512]/512
paint_tex=texture('Lagoon_Painted',np.clip(.965+.022*np.sin(x*PI*4)*np.sin(y*PI*2)+.009*np.sin(x*PI*12+y*PI*8),0,1))
wood_tex=texture('Lagoon_Wood',np.clip(.92+.045*np.sin(x*PI*12+np.sin(y*PI*2))+.02*np.sin(x*PI*30+y*3),0,1))
ripple=.90+.035*np.sin(x*PI*8+2*np.sin(y*PI*4))+.03*np.cos(y*PI*10+np.sin(x*PI*6))
ripple+=.08*(np.sin(x*PI*8+2*np.sin(y*PI*4))>.93)
water_tex=texture('Lagoon_Ripples',np.clip(ripple,0,1))
gradient=rgb('187CB4')[None,None,:]*(1-y[:,:,None])+rgb('83DDD1')[None,None,:]*y[:,:,None]
gradient_tex=texture('Lagoon_WaterGradient',np.broadcast_to(gradient,(512,512,3)).copy())
# One world-aligned painted water map across all meshes eliminates layer seams.
wy,wx=np.mgrid[0:2048,0:2048]/2047*360-180
wr=np.sqrt(wx*wx+wy*wy);wa=np.arctan2(wx,wy)
sh=np.exp(-((wr-outer(wa))/8)**2)
wp=np.broadcast_to(rgb('168BBC'),(2048,2048,3)).copy()
wp=wp*(1-sh[:,:,None])+rgb('73DCD0')[None,None,:]*sh[:,:,None]
inside=1-smooth(27,32,wr)
lag=rgb('25BED0')[None,None,:]+np.minimum(wr/28,1)[:,:,None]*np.array([.13,.06,.015])
wp=wp*(1-inside[:,:,None])+lag*inside[:,:,None]
wave=.008*np.sin(wx*.73+2*np.sin(wy*.4))+.009*np.sin(wy*1.03+np.sin(wx*.24))
wave+=.022*(np.sin(wx*.81+2*np.sin(wy*.44))>.955)
wp=np.clip(wp+wave[:,:,None],0,1);water_world=texture('Lagoon_WaterPainted',wp)
del wx,wy,wr,wa,sh,wp,inside,lag,wave

def mat(name,color,tex=None,rough=.8,emission=0):
    name='LG_'+name;m=bpy.data.materials.new(name);m.use_nodes=True
    col=(*[linear(c) for c in rgb(color)],1);m.diffuse_color=col
    bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=col;bs.inputs['Roughness'].default_value=rough
    if tex:
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=tex
        mul=m.node_tree.nodes.new('ShaderNodeMixRGB');mul.blend_type='MULTIPLY';mul.inputs[0].default_value=1;mul.inputs[2].default_value=col
        m.node_tree.links.new(t.outputs['Color'],mul.inputs[1]);m.node_tree.links.new(mul.outputs[0],bs.inputs['Base Color'])
    if emission:bs.inputs['Emission Color'].default_value=col;bs.inputs['Emission Strength'].default_value=emission
    materials[name]=m;specs.append(dict(name=name,color_srgb=rgb(color).tolist(),base_map=tex.name+'.png' if tex else '',roughness=rough,emission=emission));return name

TERRAIN=mat('Terrain','FFFFFF',terrain_tex)
STONE=mat('Limestone','C7C8AF',paint_tex);CHALK=mat('RockLight','E4DABD',paint_tex)
GRASS=mat('Grass','8DBE49',paint_tex);LEAF=mat('Leaf','61A848',paint_tex);SUNLEAF=mat('LeafLight','AED15B',paint_tex)
BARK=mat('Bark','A57A49',wood_tex);WOOD=mat('Wood','C69160',wood_tex);WOODLIGHT=mat('WoodLight','D9AA78',wood_tex);WOODDARK=mat('WoodDark','8E6244',wood_tex)
ROPE=mat('Rope','E8D6AD',paint_tex);ACCENT=mat('Accent','24B6AE',paint_tex)
GOLD=mat('Gold','FFD258',paint_tex);CORAL=mat('Coral','F78685',paint_tex);VIOLET=mat('Violet','BC90DF',paint_tex)
SEA=mat('Ocean','FFFFFF',water_world,.65);LAGOON=mat('LagoonWater','FFFFFF',water_world,.65)
SHALLOW=mat('WaterGradient','FFFFFF',water_world,.65);FOAM=mat('Foam','D5F7E8',None,.75,.12)

def module(name,collision='none',boxes=None):
    global current
    current=name;c=bpy.data.collections.new(name);scene.collection.children.link(c)
    modules[name]=dict(collection=c,objects=[],collision=collision,boxes=boxes or [])
class Batch:
    def __init__(self,name):self.name=current+'__'+name;self.mod=current;self.v=[];self.f=[];self.uv=[];self.m=[];self.mi=[];self.sm=[]
    def add(self,v,f,material,smooth=False,uv=None):
        off=len(self.v);self.v.extend(v)
        if material not in self.m:self.m.append(material)
        for face in f:
            self.f.append(tuple(off+i for i in face));self.mi.append(self.m.index(material));self.sm.append(smooth)
            if uv:self.uv.append([uv[i] for i in face])
            else:
                normal=(Vector(v[face[1]])-Vector(v[face[0]])).cross(Vector(v[face[2]])-Vector(v[face[0]]))
                axes=[k for k in range(3) if k!=max(range(3),key=lambda k:abs(normal[k]))]
                self.uv.append([tuple(v[i][k]*.22 for k in axes) for i in face])
    def finish(self):
        mesh=bpy.data.meshes.new(self.name);mesh.from_pydata(self.v,[],self.f);mesh.update();uv=mesh.uv_layers.new(name='UVMap')
        for m in self.m:mesh.materials.append(materials[m])
        for p,co,mi,sm in zip(mesh.polygons,self.uv,self.mi,self.sm):
            p.material_index=mi;p.use_smooth=sm
            for idx,coord in zip(p.loop_indices,co):uv.data[idx].uv=coord
        obj=bpy.data.objects.new(self.name,mesh);modules[self.mod]['collection'].objects.link(obj);modules[self.mod]['objects'].append(obj);return obj
def batch(name):
    key=current+'__'+name
    if key not in batches:batches[key]=Batch(name)
    return batches[key]
def beam(name,a,b,r,material,n=10,r2=None):
    a,b=Vector(a),Vector(b);rot=(b-a).to_track_quat('Z','Y');r2=r if r2 is None else r2
    v=[tuple(c+rot@Vector((rr*math.cos(i*2*PI/n),rr*math.sin(i*2*PI/n),0))) for c,rr in ((a,r),(b,r2)) for i in range(n)]
    f=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    batch(name).add(v,f,material,True)
def box(name,p,size,material,bevel=.055):
    # Apply a real bevel to reusable timber pieces before batching.
    bm=bmesh.new();bmesh.ops.create_cube(bm,size=1)
    for v in bm.verts:v.co=Vector(tuple(v.co[i]*size[i] for i in range(3)))
    if bevel:bmesh.ops.bevel(bm,geom=list(bm.edges),offset=bevel,segments=2,affect='EDGES')
    bm.verts.ensure_lookup_table();bm.verts.index_update()
    verts=[tuple(v.co+Vector(p)) for v in bm.verts];faces=[tuple(v.index for v in f.verts) for f in bm.faces]
    batch(name).add(verts,faces,material);bm.free()
ico={}
def blob(name,p,size,material,seed=0,sub=2):
    if sub not in ico:
        bm=bmesh.new();bmesh.ops.create_icosphere(bm,subdivisions=sub,radius=1);bm.verts.ensure_lookup_table();bm.verts.index_update()
        ico[sub]=([tuple(v.co) for v in bm.verts],[tuple(v.index for v in f.verts) for f in bm.faces]);bm.free()
    v,f=ico[sub];rr=random.Random(seed)
    verts=[tuple(p[k]+co[k]*size[k]*(1+rr.uniform(-.065,.065)) for k in range(3)) for co in v]
    batch(name).add(verts,f,material,True)
def frond(name,p,a,length,width,material,lift=1,droop=1):
    d=Vector((math.cos(a),math.sin(a),0));side=Vector((-d.y,d.x,0));o=Vector(p);v=[]
    for j in range(10):
        t=j/9;c=o+d*length*t+Vector((0,0,lift*math.sin(PI*t)-droop*t*t));w=width*math.sin(PI*t)**.65
        v.extend([tuple(c-side*w),tuple(c+Vector((0,0,.16*math.sin(PI*t)))),tuple(c+side*w)])
    f=[]
    for j in range(9):a=j*3;b=a+3;f.extend([(a,b,b+1,a+1),(a+1,b+1,b+2,a+2)])
    nv=len(v);v+=v.copy();f += [tuple(nv+k for k in reversed(face)) for face in f.copy()]
    batch(name).add(v,f,material,True)
def curve(name,points,r,material):
    for a,b in zip(points,points[1:]):beam(name,a,b,r,material,6)

# Island is one replaceable module containing ten continuous mesh sectors.
module('Island',collision='mesh')
for sector in range(10):
    v=[];uv=[];rows=32;cols=24
    for j in range(rows+1):
        t=j/rows
        for i in range(cols+1):
            u=(sector+i/cols)/10;a=.13+(2*PI-.26)*u
            for _ in range(4):
                r=26+(float(outer(a))+.8-26)*t;cut=math.asin(4.5/r);a=cut+(2*PI-2*cut)*u
            x=r*math.sin(a);y=r*math.cos(a);v.append((x,y,float(height(r,a))));uv.append(((x+70)/140,(y+70)/140))
    f=[]
    for j in range(rows):
        for i in range(cols):k=j*(cols+1)+i;f.append((k,k+1,k+cols+2,k+cols+1))
    batch('Terrain_%02d'%sector).add(v,f,TERRAIN,True,uv)
# Closed inlet banks, extending below the sea rather than leaving a paper-thin cut.
for side in (-1,1):
    v=[];uv=[];f=[]
    for j in range(33):
        y=25.7+j*(31/32);x=side*4.5;z=ground(x,y)
        v.extend([(x,y,z),(x,y,-2.5),(x+side*.3,y,-2.5),(x+side*.3,y,z)]);uv.extend([((x+70)/140,(y+70)/140)]*4)
    for j in range(32):
        for k in range(4):f.append((j*4+k,j*4+(k+1)%4,(j+1)*4+(k+1)%4,(j+1)*4+k))
    f.extend([(3,2,1,0),(128,129,130,131)])
    batch('InletBank_'+str(side)).add(v,f,CHALK)

module('PlayerStand',boxes=[dict(center=[0,-.16,3.5],size=[6,.32,7])])
# All local assets use Blender +Y as the socket's forward direction, top at Z=0.
for j in range(14):box('Planks',(0,(j+.5)*.5,-.13),(6,.48,.26),WOOD if j%3 else WOODLIGHT)
for x in (-2.3,2.3):box('Bearers',(x,3.5,-.40),(.25,7,.30),WOODDARK)
for x in (-2.85,2.85):
    for y in (.35,3.5,6.65):
        beam('Posts',(x,y,-2.3),(x,y,1.12),.18,BARK,12,.16)
        beam('Caps',(x,y,.92),(x,y,1.18),.205,ACCENT,12)
        for z in (.73,.81):
            pts=[(x+.19*math.cos(i*2*PI/16),y+.19*math.sin(i*2*PI/16),z) for i in range(17)];curve('RopeWrap',pts,.028,ROPE)
    for ya,yb in ((.35,3.5),(3.5,6.65)):
        curve('SideRopes',[(x,ya+(yb-ya)*t,.78-.20*math.sin(PI*t)) for t in np.linspace(0,1,9)],.045,ROPE)

module('InletBridge',boxes=[dict(center=[0,-.18,0],size=[13,.36,4.2])])
for i in range(26):box('Deck',(-6.25+i*.5,0,-.13),(.48,4.2,.26),WOOD if i%3 else WOODLIGHT)
for y in (-1.6,1.6):box('Beams',(0,y,-.43),(13,.25,.45),WOODDARK)
for y in (-2,2):
    for x in (-6,-3,0,3,6):beam('Posts',(x,y,-2.7),(x,y,1.18),.17,BARK,12)
    for x in (-6,-3,0,3):curve('Ropes',[(x+3*t,y,.95-.20*math.sin(PI*t)) for t in np.linspace(0,1,9)],.05,ROPE)

for i in range(3):
    module('Rock_'+chr(65+i),collision='mesh')
    # Broad tapered limestone faces, matching Tidebloom's sculpted rock language.
    n=9;v=[];h=3.1+i*.3;sx=1.6-i*.1;sy=1.45+i*.1
    for j,(scale,z) in enumerate(((1.03,-.25),(1.08,.25),(.93,.72),(.77,.93),(.59,1))):
        for k in range(n):
            a=k*2*PI/n+.055*math.sin(j+i);w=1+.09*math.sin(k*4+i)
            v.append((math.cos(a)*sx*scale*w,math.sin(a)*sy*scale*w,z*h))
    f=[tuple(range(n-1,-1,-1)),tuple(range(4*n,5*n))]
    f += [(j*n+k,j*n+(k+1)%n,(j+1)*n+(k+1)%n,(j+1)*n+k) for j in range(4) for k in range(n)]
    batch('Stone').add(v,f,STONE if i%2 else CHALK,True)
    blob('GrassCap',(0,0,h*.98),(sx*.73,sy*.73,.21),GRASS,i,2)
    for k in range(5):
        a=k*2*PI/5;blob('GrassCap',(math.cos(a)*sx*.57,math.sin(a)*sy*.57,h*.94),(sx*.25,sy*.25,.23),GRASS,i+k,2)

for variant in range(2):
    module('Palm_'+chr(65+variant),boxes=[dict(center=[.2,2.8,0],size=[.6,5.6,.6])])
    pts=[(.65*(j/9)**1.5,.18*math.sin(j*.2),j*.62) for j in range(10)]
    for j in range(9):beam('Trunk',pts[j],pts[j+1],.30-j*.015,BARK,10,.29-j*.015)
    top=pts[-1]
    for j in range(9):frond('Crown',top,j*2*PI/9+variant*.5,3.25+(j%3)*.23,.65,LEAF if j%2 else SUNLEAF,1.25,1.05)
    for j in range(3):blob('Coconuts',(top[0]+.24*math.cos(j*2.1),top[1]+.24*math.sin(j*2.1),top[2]-.30),(.22,.22,.27),GOLD,j,2)

for name,color in (('Flowers_Coral',CORAL),('Flowers_Gold',GOLD),('Flowers_Violet',VIOLET)):
    module(name)
    for i in range(6):frond('Leaves',(0,0,.05),i*PI/3,1,.28,LEAF,.55,.03)
    for j in range(3):
        px=(j-1)*.40;py=.3*math.sin(j*2);pz=.65+(j%2)*.25;beam('Stems',(px,py,0),(px,py,pz),.045,LEAF,6)
        for k in range(5):a=k*2*PI/5;blob('Petals',(px+.23*math.cos(a),py+.23*math.sin(a),pz),(.25,.20,.12),color,k,2)
        blob('Centers',(px,py,pz+.08),(.16,.16,.12),GOLD,0,2)
module('Bush')
for i in range(5):blob('Leaves',((i%3-1)*.45,(i//3)*.42,.5),(.65,.57,.62),LEAF if i%2 else SUNLEAF,i,2)

module('Ocean')
batch('Surface').add([(-800,-800,-.08),(800,-800,-.08),(800,800,-.08),(-800,800,-.08)],[(0,1,2,3)],SEA,False,[((-800+180)/360,(-800+180)/360),((800+180)/360,(-800+180)/360),((800+180)/360,(800+180)/360),((-800+180)/360,(800+180)/360)])
module('LagoonWater')
v=[(0,0,.015)]+[(29*math.sin(i*2*PI/192),29*math.cos(i*2*PI/192),.015) for i in range(192)]
uv=[(.5,.5)]+[((p[0]+180)/360,(p[1]+180)/360) for p in v[1:]]
batch('Surface').add(v,[(0,1+(i+1)%192,1+i) for i in range(192)],LAGOON,False,uv)
module('Shallows')
for inner in (False,True):
    v=[];uv=[];f=[];N=192
    for j in range(9):
        t=j/8
        for i in range(N+1):
            a=i*2*PI/N;r=18+t*10 if inner else float(outer(a))+13-t*15
            px=r*math.sin(a);py=r*math.cos(a);v.append((px,py,.025));uv.append(((px+180)/360,(py+180)/360))
    for j in range(8):
        for i in range(N):k=j*(N+1)+i;face=(k,k+1,k+N+2,k+N+1);f.append(face if inner else tuple(reversed(face)))
    batch('Inner' if inner else 'Outer').add(v,f,SHALLOW,False,uv)
module('ShoreFoam')
for inner in (False,True):
    for offset,width in ((0,.18),(.65,.07)):
        v=[];f=[]
        for i in range(385):
            a=.19+(2*PI-.38)*i/384;r=(27 if inner else float(outer(a)))+offset+.10*math.sin(a*43)
            for rr in (r-width,r+width):v.append((rr*math.sin(a),rr*math.cos(a),.075))
        for i in range(384):
            if i%13>10:continue
            k=i*2;f.append((k,k+2,k+3,k+1))
        batch('Lagoon' if inner else 'Outer').add(v,f,FOAM)

objects=[b.finish() for b in batches.values()]
# Correct normals algorithmically; module collision relies on upward terrain faces.
for obj in objects:
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
    obj['module']=obj.name.split('__')[0];obj['units']='metres'
    obj['editable']='Independent module; named material roles; local attachment origin'

# Export each module at origin. Imported art receives the same 180-degree plan
# correction verified by the existing golf pipeline; wrapper/anchors stay unrotated.
for name,data in modules.items():
    bpy.ops.object.select_all(action='DESELECT')
    for obj in data['objects']:obj.select_set(True)
    bpy.context.view_layer.objects.active=data['objects'][0]
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='STRIP',use_mesh_modifiers=True)
    (SRC/'Modules').mkdir(exist_ok=True)
    # Save via a fresh filename before replacing this generator-owned library;
    # Blender's Windows backup rename can fail for an existing library file.
    library=SRC/'Modules'/(name+'.blend');fresh=SRC/'Modules'/(name+'.writing.blend')
    bpy.data.libraries.write(str(fresh),{data['collection']},fake_user=True)
    fresh.replace(library)

assembly=bpy.data.collections.new('Lagoon_Assembly');scene.collection.children.link(assembly)
def place(mod,id,x=0,y=0,z=0,yaw=0,scale=1,accent=''):
    empty=bpy.data.objects.new(id,None);assembly.objects.link(empty);empty.location=(x,y,z);empty.rotation_euler.z=-math.radians(yaw);empty.scale=(scale,)*3
    for source in modules[mod]['objects']:
        obj=source.copy();obj.data=source.data;assembly.objects.link(obj);obj.parent=empty;obj.location=(0,0,0)
        if accent and mod=='PlayerStand' and 'Caps' in obj.name:
            for i,slot in enumerate(obj.material_slots):
                if slot.material and slot.material.name=='LG_Accent':
                    m=slot.material.copy();m.name='ReviewAccent_'+id;m.diffuse_color=(*[linear(c) for c in rgb(accent)],1)
                    for node in m.node_tree.nodes:
                        if node.type=='MIX_RGB':node.inputs[2].default_value=m.diffuse_color
                    slot.link='OBJECT';slot.material=m
    placements.append(dict(module=mod,id=id,position=dict(x=x,y=z,z=y),yaw=yaw,scale=scale,accent=accent))
    return empty
for name in ('Island','Ocean','LagoonWater','Shallows','ShoreFoam'):place(name,name)
place('InletBridge','NorthInletBridge',0,36,1.2)
bearings=[180,252,324,36,108];colors=['24B6AE','F78685','FFD258','BC90DF','4A9FDF'];spawns=[];stands=[]
for i,(bearing,color) in enumerate(zip(bearings,colors)):
    a=math.radians(bearing);x,y=28*math.sin(a),28*math.cos(a)
    place('PlayerStand','Stand_%02d'%(i+1),x,y,1.2,bearing+180,accent=color)
    spawns.append(dict(x=32*math.sin(a),y=1.35,z=32*math.cos(a)))
    stands.append(dict(x=22.5*math.sin(a),y=1.35,z=22.5*math.cos(a)))

# Decorative groups sit outside the walking loop. Clear the inlet and five approaches.
for i in range(64):
    a=2*PI*(i+.25)/64;r=float(outer(a))-random.uniform(1.8,4.7);x,y=r*math.sin(a),r*math.cos(a)
    if y>0 and abs(x)<9:continue
    place('Rock_'+chr(65+i%3),'CoastRock_%02d'%i,x,y,ground(x,y)-.7,random.uniform(0,360),random.uniform(1.05,2.1))
for side in (-1,1):
    for j in range(7):
        y=29+j*4.1
        if 32<y<40:continue
        x=side*6.1;place('Rock_'+chr(65+j%3),'InletRock_%s_%d'%(side,j),x,y,ground(x,y)-.65,j*41,.8)
# Break the perfect inner ring with small natural headlands between clear stations.
for i in range(5):
    bearing=216+i*72
    if bearing%360==0:continue
    a=math.radians(bearing);x,y=30.8*math.sin(a),30.8*math.cos(a)
    for j in range(3):
        xx=x+(j-1)*1.6*math.cos(a);yy=y-(j-1)*1.6*math.sin(a)
        place('Rock_'+chr(65+j),'InnerHeadland_%d_%d'%(i,j),xx,yy,.15,j*39,.7 if j!=1 else 1.05)
    place('Flowers_Coral','InnerFlowers_%d'%i,33*math.sin(a),33*math.cos(a),1.2,i*60,1)
for i in range(28):
    a=2*PI*(i+.5)/28;r=random.uniform(41,48);x,y=r*math.sin(a),r*math.cos(a)
    if y>0 and abs(x)<8:continue
    place('Palm_'+chr(65+i%2),'Palm_%02d'%i,x,y,ground(x,y),random.uniform(0,360),random.uniform(.8,1.25))
    for j in range(3):
        xx=x+2*math.cos(j*2.1);yy=y+2*math.sin(j*2.1)
        place(['Flowers_Coral','Flowers_Gold','Flowers_Violet'][j],'Garden_%02d_%d'%(i,j),xx,yy,ground(xx,yy),j*100,random.uniform(1,1.45))
for i in range(35):
    a=i*2*PI/35;r=39.5;x,y=r*math.sin(a),r*math.cos(a)
    if y>0 and abs(x)<8:continue
    place('Bush','Bush_%02d'%i,x,y,ground(x,y),i*47,.8)
# Small distant sea rocks; independent reusable modules, no new combined scenery mesh.
for i in range(13):
    a=i*2*PI/13;r=75+random.uniform(0,20)
    place('Rock_'+chr(65+i%3),'SeaRock_%02d'%i,r*math.sin(a),r*math.cos(a),-1,random.uniform(0,360),random.uniform(1.0,2.3))

# A connected containment polygon follows inner shore, steps onto each stand,
# and continues around the outer shore / bridge edges. Keep it out of camera layer.
boundaries=[]
def segment(a,b):boundaries.append(dict(a=dict(x=a[0],y=2,z=a[1]),b=dict(x=b[0],y=2,z=b[1])))
inner_points=[];radius=28.1;half=3;edge=21
# The union of ring land and rectangular piers is not star-shaped. Trace each
# rectangular notch explicitly; a radial approximation leaves water beside decks.
opening=math.asin(half/radius);last=math.radians(9.5)
def shore_arc(start,end):
    for a in np.linspace(start,end,max(2,math.ceil((end-start)*180/PI)*2)):
        inner_points.append((radius*math.sin(a),radius*math.cos(a)))
for bearing in sorted(bearings):
    a=math.radians(bearing);shore_arc(last,a-opening)
    n=Vector((math.sin(a),math.cos(a)));t=Vector((math.cos(a),-math.sin(a)))
    inner_points.extend([tuple(n*edge-t*half),tuple(n*edge+t*half),tuple(n*math.sqrt(radius*radius-half*half)+t*half)])
    last=a+opening
shore_arc(last,math.radians(350.5))
for a,b in zip(inner_points,inner_points[1:]):segment(a,b)
outer_points=[]
for i in range(257):
    a=.086+(2*PI-.172)*i/256;r=float(outer(a))-1.4;outer_points.append((r*math.sin(a),r*math.cos(a)))
for a,b in zip(outer_points,outer_points[1:]):segment(a,b)
# Channel banks, interrupted by the walkable north bridge.
for side in (-1,1):
    for ya,yb in ((27.7,33.8),(38.2,54)):
        segment((side*4.9,ya),(side*4.9,yb))
segment((-4.9,33.9),(4.9,33.9));segment((-4.9,38.1),(4.9,38.1))

module_specs=[]
for name,data in modules.items():
    obs=data['objects'];module_specs.append(dict(name=name,collision=data['collision'],boxes=data['boxes'],meshes=len(obs),triangles=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in obs)))
manifest=dict(name='Lagoon Island',capacity=5,modules=module_specs,materials=specs,placements=placements,spawns=spawns,stands=stands,bearings=bearings,boundaries=boundaries,source='ArtSource/Fishing/Lagoon/Island_Lagoon_5P.blend',unique_triangles=sum(m['triangles'] for m in module_specs))
for p in (OUT/'unity-import.json',SRC/'lagoon-manifest.json'):p.write_text(json.dumps(manifest,indent=2)+'\n')

# Keep origin-centered source modules in a hidden library collection, fully editable.
kit=bpy.data.collections.new('AssetKit_EDITABLE');scene.collection.children.link(kit)
for data in modules.values():scene.collection.children.unlink(data['collection']);kit.children.link(data['collection'])
kit.hide_render=True;kit.hide_viewport=True
review=bpy.data.collections.new('Review');scene.collection.children.link(review)
def move_review(obj):
    for col in list(obj.users_collection):col.objects.unlink(obj)
    review.objects.link(obj)
scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.40,.65,.9,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.65
bpy.ops.object.light_add(type='SUN');sun=bpy.context.object;sun.name='CoastalSun';sun.rotation_euler=(.5,-.4,-.6);sun.data.energy=2.8;sun.data.angle=.14;move_review(sun)
views=[('01_Lagoon_Overview',(0,-151,152),(0,0,0),48),('02_Player_Stand',(3,-33,4.7),(0,4,3),25),('03_Bridge',(17,26,10),(0,36,2),38)]
cams=[]
for name,pos,focus,lens in views:
    bpy.ops.object.camera_add(location=pos);cam=bpy.context.object;cam.name=name;cam.rotation_euler=(Vector(focus)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=lens;cam.data.clip_end=1800;move_review(cam);cams.append(cam)
scene.camera=cams[0];scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.render.resolution_x=1600;scene.render.resolution_y=1000;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX'
bpy.ops.wm.save_as_mainfile(filepath=str(SRC/'Island_Lagoon_5P.blend'))
print('LAGOON_COMPLETE modules=%d instances=%d unique_triangles=%d'%(len(modules),len(placements),manifest['unique_triangles']),flush=True)
if '--render' in sys.argv:
    for cam,(name,*_) in zip(cams,views):scene.camera=cam;scene.render.filepath=str(DOC/(name+'.png'));bpy.ops.render.render(write_still=True)
