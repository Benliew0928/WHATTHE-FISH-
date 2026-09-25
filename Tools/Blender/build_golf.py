"""TIDEBLOOM ISLAND. Deterministic Blender source, UV textures and game export.

blender -b --python Tools/Blender/build_golf.py -- --render
Coordinates in metres: Blender X/Y are the course plan, Z is height.
The original Island.blend remains archived; this generator owns Tidebloom/ only.
"""
import bpy, bmesh, math, json, sys, random
import numpy as np
from mathutils import Vector
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
SRC=ROOT/'ArtSource/Golf/Tidebloom'
OUT=ROOT/'Game/Assets/_Game/Art/Golf/Tidebloom'
DOC=ROOT/'Docs/VisualDirection/Golf'
for p in (SRC,OUT,DOC): p.mkdir(parents=True,exist_ok=True)
random.seed(417)
PI=math.pi
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
for c in list(bpy.data.collections):bpy.data.collections.remove(c)
for m in list(bpy.data.materials):bpy.data.materials.remove(m)
for im in list(bpy.data.images):
    if im.name.startswith('Tidebloom_'):bpy.data.images.remove(im)
scene=bpy.context.scene
scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
materials={};specs=[];batches={};collections={};obstacles=[]

def rgb(h):return np.array([int(h[i:i+2],16)/255 for i in (0,2,4)])
def linear(c):return c/12.92 if c<=.04045 else ((c+.055)/1.055)**2.4
def texture(name,a):
    h,w=a.shape[:2];im=bpy.data.images.new(name,width=w,height=h,alpha=False)
    pixels=np.ones((h,w,4),np.float32);pixels[:,:,:3]=a[:,:,None] if a.ndim==2 else a
    path=str(OUT/(name+'.png'));im.pixels.foreach_set(pixels.ravel());im.filepath_raw=path;im.file_format='PNG';im.save()
    # Reload the encoded PNG so Blender and Unity sample the same sRGB image.
    bpy.data.images.remove(im);im=bpy.data.images.load(path);im.name=name;im.pack();return im
def material(name,h,tex=None,rough=.85,emission=0):
    name='TB_'+name;c=rgb(h);m=bpy.data.materials.new(name);m.use_nodes=True
    color=(*[linear(v) for v in c],1);m.diffuse_color=color
    bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=color;bs.inputs['Roughness'].default_value=rough
    if tex:
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=tex
        mult=m.node_tree.nodes.new('ShaderNodeMixRGB');mult.blend_type='MULTIPLY';mult.inputs[0].default_value=1;mult.inputs[2].default_value=color
        m.node_tree.links.new(t.outputs['Color'],mult.inputs[1]);m.node_tree.links.new(mult.outputs[0],bs.inputs['Base Color'])
    if emission:bs.inputs['Emission Color'].default_value=color;bs.inputs['Emission Strength'].default_value=emission
    materials[name]=m;specs.append(dict(name=name,color_srgb=c.tolist(),base_map=tex.name+'.png' if tex else '',roughness=rough,emission=emission));return name

# Shared analytic course plan drives both the surface and its painted color map.
def smooth(a,b,x):
    t=np.clip((x-a)/(b-a),0,1);return t*t*(3-2*t)
def radius(a):return 192+12*np.sin(3*a+.5)+7*np.sin(5*a-1)+4*np.cos(9*a)
def route(y):return 24*np.sin((y+135)/55)-5
def width(y):return 24+5*np.cos(y/38)+6*np.exp(-((y+110)/40)**2)
BUNKERS=[(-42,-112,18,11,.3),(43,-56,24,14,-.3),(-38,16,21,13,.2),(38,74,18,12,.5),(-37,127,15,10,-.5)]
def bunker_distance(x,y,b):
    cx,cy,rx,ry,a=b;u=(x-cx)*math.cos(a)+(y-cy)*math.sin(a);v=-(x-cx)*math.sin(a)+(y-cy)*math.cos(a)
    return np.sqrt((u/rx)**2+(v/ry)**2)*(1+.10*np.sin(np.arctan2(v/ry,u/rx)*3+.6))
def height(x,y):
    a=np.arctan2(y,x);r=np.sqrt(x*x+y*y);q=r/radius(a)
    base=3.6+1.4*np.sin(y/70)+.75*np.cos(x/42)*np.sin(y/36)
    base+=6.8*np.exp(-((x-4)/57)**2-((y-131)/49)**2)
    base+=2*np.exp(-((x+24)/50)**2-((y+144)/38)**2)
    base+=1.6*np.sin(x/26+.3)*np.sin(y/31)*(smooth(32,68,np.abs(x-route(y))))
    # Continuous sand bowls, shallow enough to enter and leave on foot.
    for b in BUNKERS:base-=1.65*(1-smooth(.55,1.22,bunker_distance(x,y,b)))
    return base*(1-smooth(.80,.97,q))+.25*smooth(.80,.97,q)-.8*smooth(.98,1.04,q)

N=2048;yy,xx=np.mgrid[0:N,0:N].astype(np.float32);xx=xx/(N-1)*440-220;yy=yy/(N-1)*440-220
a=np.arctan2(yy,xx);q=np.sqrt(xx*xx+yy*yy)/radius(a)
rng=np.random.default_rng(417)
noise=rng.normal(0,.009,(N,N))+.005*np.sin(xx*1.1+np.sin(yy*.4))+.008*np.sin(xx*.13)*np.sin(yy*.15)
pixels=np.broadcast_to(rgb('5A9E37'),(N,N,3)).copy()
def paint(color,mask):
    global pixels
    pixels=pixels*(1-mask[:,:,None])+np.asarray(color)[None,None,:]*mask[:,:,None]
fair=1-smooth(width(yy)-.4,width(yy)+.7,np.abs(xx-route(yy)))
fair*=smooth(-166,-148,yy)*(1-smooth(128,151,yy))
fringe=1-smooth(width(yy)+3,width(yy)+5,np.abs(xx-route(yy)))
fringe*=smooth(-170,-149,yy)*(1-smooth(133,156,yy))
paint(rgb('7CAF3A'),fringe)
stripe=(np.sin((xx-route(yy))*.43)>0).astype(np.float32)
faircolor=rgb('A5D34C')[None,None,:]+stripe[:,:,None]*np.array([.045,.032,.012])
pixels=pixels*(1-fair[:,:,None])+faircolor*fair[:,:,None]
for cx,cy,rx,ry in [(4,133,29,22),(-13,-146,18,12),(-106,-72,20,15)]:
    d=np.sqrt(((xx-cx)/rx)**2+((yy-cy)/ry)**2)
    paint(rgb('7AB63C'),1-smooth(1,1.15,d));paint(rgb('BADE57'),1-smooth(.94,1,d))
    ring=(np.sin(d*42)*.004)[:,:,None];pixels+=ring*(1-smooth(.93,1,d))[:,:,None]
for b in BUNKERS:
    d=bunker_distance(xx,yy,b)
    paint(rgb('407E32'),1-smooth(1.03,1.12,d))
    paint(rgb('D0B881'),1-smooth(.94,1.02,d))
    paint(rgb('F8E4AB'),1-smooth(.76,.96,d))
    pixels+=(np.sin(d*45)*.008*(1-smooth(.8,.98,d)))[:,:,None]
paint(rgb('E9CD90'),smooth(.878,.916,q));paint(rgb('F8E5B4'),smooth(.923,.96,q))
# Meandering sandy coastal walking path connecting the practice area.
pathx=-85+13*np.sin(yy/43);path=(1-smooth(2.5,4.5,np.abs(xx-pathx)))*smooth(-146,-135,yy)*(1-smooth(105,133,yy))
paint(rgb('DFC891'),path)
pixels=np.clip(pixels+noise[:,:,None],0,1)
course=texture('Tidebloom_Course_BaseColor',pixels)
del pixels,xx,yy,a,q,fair,fringe,stripe,faircolor,noise,path,pathx
ty,tx=np.mgrid[0:512,0:512]/512
grain=np.clip(.97+rng.normal(0,.009,(512,512)),0,1)
detail=texture('Tidebloom_SurfaceGrain',grain)
rocktex=texture('Tidebloom_Limestone',np.clip(.96+.006*np.sin(tx*8*PI+np.sin(ty*4*PI))+rng.normal(0,.005,(512,512)),0,1))
waterv=.98+.008*np.sin(tx*8*PI+2*np.sin(ty*2*PI))+.008*np.sin(ty*12*PI+np.sin(tx*6*PI))
watertex=texture('Tidebloom_WaterRipples',np.clip(waterv,0,1))
falltex=texture('Tidebloom_Cascade',np.clip(.82+.13*np.sin(tx*48*PI+np.sin(ty*2*PI))+.04*np.sin(ty*4*PI),0,1))
def water_gradient(name,start,end):
    t=smooth(0,1,ty)[:,:,None];a=rgb(start)[None,None,:]*(1-t)+rgb(end)[None,None,:]*t
    return texture(name,np.clip(a+.009*np.sin(tx*16*PI+2*np.sin(ty*4*PI))[:,:,None],0,1))
shoretex=water_gradient('Tidebloom_ShoreGradient','8CDDCF','35BBC6')
lagoontex=water_gradient('Tidebloom_LagoonGradient','35BBC6','177DB6')
terrain=material('Course','FFFFFF',course)
rock=material('Limestone','C4BB9C',rocktex);rocklight=material('Chalk','E3D8B6',rocktex)
grass=material('Turf','71AF3D',detail);leaf=material('PalmLeaf','65AB38',detail);leaflight=material('LeafSun','ADD451',detail)
bark=material('Bark','A7814F',rocktex);barklight=material('BarkRings','C5A16B')
stem=material('Foliage','3C873F');gold=material('PetalGold','FFD34D');pink=material('PetalCoral','F9848F');purple=material('PetalOrchid','B78AE0')
white=material('Ivory','FFF1D3');flag=material('FlagCoral','F46E51');dark=material('DeepTeal','25545B')
sea=material('Ocean','177DB6',watertex,.26);lagoon=material('Lagoon','FFFFFF',lagoontex,.3);shallow=material('Shallows','FFFFFF',shoretex,.4)
pool=material('Spring','35BBC6',watertex,.32)
foam=material('Foam','DEF9E8',None,.7,.12);cascade=material('Cascade','A4EAF2',falltex,.32,.1)

class Batch:
    def __init__(self,name):self.name=name;self.v=[];self.f=[];self.uv=[];self.mi=[];self.m=[];self.s=[]
    def add(self,v,f,mat,smooth=False,uv=None):
        off=len(self.v);self.v.extend(v)
        if mat not in self.m:self.m.append(mat)
        for face in f:
            self.f.append(tuple(i+off for i in face));self.mi.append(self.m.index(mat));self.s.append(smooth)
            self.uv.append([uv[i] if uv else (v[i][0]*.11+v[i][1]*.06,v[i][2]*.16+v[i][1]*.05) for i in face])
    def finish(self):
        mesh=bpy.data.meshes.new(self.name);mesh.from_pydata(self.v,[],self.f);mesh.update()
        layer=mesh.uv_layers.new(name='UVMap')
        for mat in self.m:mesh.materials.append(materials[mat])
        for p,uv,mi,s in zip(mesh.polygons,self.uv,self.mi,self.s):
            p.material_index=mi;p.use_smooth=s
            for i,co in zip(p.loop_indices,uv):layer.data[i].uv=co
        obj=bpy.data.objects.new(self.name,mesh)
        group=self.name.split('__')[0]
        if group not in collections:
            collections[group]=bpy.data.collections.new(group);scene.collection.children.link(collections[group])
        collections[group].objects.link(obj);return obj
def batch(name):
    if name not in batches:batches[name]=Batch(name)
    return batches[name]
def beam(name,a,b,r,mat,n=8,r2=None):
    a,b=Vector(a),Vector(b);rot=(b-a).to_track_quat('Z','Y');r2=r if r2 is None else r2
    v=[tuple(c+rot@Vector((rr*math.cos(i*2*PI/n),rr*math.sin(i*2*PI/n),0))) for c,rr in [(a,r),(b,r2)] for i in range(n)]
    f=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    batch(name).add(v,f,mat,True)
ico={}
def blob(name,p,size,mat,seed=0,sub=1):
    if sub not in ico:
        bm=bmesh.new();bmesh.ops.create_icosphere(bm,subdivisions=sub,radius=1);bm.verts.ensure_lookup_table();bm.verts.index_update()
        ico[sub]=([tuple(v.co) for v in bm.verts],[tuple(v.index for v in f.verts) for f in bm.faces]);bm.free()
    v,f=ico[sub];rr=random.Random(seed)
    verts=[tuple(p[k]+co[k]*size[k]*(1+rr.uniform(-.075,.075)) for k in range(3)) for co in v]
    batch(name).add(verts,f,mat,True)
def frond(name,origin,angle,length,w,mat,lift=1.6,droop=1.9):
    o=Vector(origin);d=Vector((math.cos(angle),math.sin(angle),0));side=Vector((-d.y,d.x,0));v=[]
    for i in range(9):
        t=i/8;c=o+d*(length*t)+Vector((0,0,lift*math.sin(t*PI)-droop*t*t))
        spread=w*math.sin(PI*t)**.7*(.97 if i%2 else 1)
        v.extend([tuple(c-side*spread),tuple(c+Vector((0,0,.18*math.sin(t*PI)))),tuple(c+side*spread)])
    f=[]
    for i in range(8):
        a=i*3;b=a+3;f.extend([(a,b,b+1,a+1),(a+1,b+1,b+2,a+2)])
    # Independent back vertices prevent opposing normals from cancelling.
    nv=len(v);v+=list(v);f += [tuple(nv+j for j in reversed(face)) for face in list(f)]
    batch(name).add(v,f,mat,True)

# Triangulated continuous walkable relief, split into 16 cullable sectors.
RINGS=104;SEGS=256
for sector in range(16):
    v=[]
    for j in range(RINGS+1):
        r=j/RINGS
        for i in range(17):
            a=2*PI*(sector*16+i)/SEGS;rr=radius(a)*r;x=rr*math.cos(a);y=rr*math.sin(a)
            v.append((x,y,float(height(x,y))))
    f=[]
    for j in range(RINGS):
        for i in range(16):
            k=j*17+i
            if j>0:f.append((k,k+18,k+1))
            f.append((k,k+17,k+18))
    batch('Terrain__Walkable_%02d'%sector).add(v,f,terrain,True,[((p[0]+220)/440,(p[1]+220)/440) for p in v])

def ring(name,inner,outer,z,mat,offset=0,n=256):
    v=[]
    for d in (inner,outer):
        for i in range(n):
            a=i*2*PI/n;r=radius(a)+d;v.append((r*math.cos(a),r*math.sin(a),z+offset*math.sin(a*13)))
    uv=[(i/n,j) for j in (0,1) for i in range(n)] if mat in (shallow,lagoon) else None
    batch(name).add(v,[(i,n+i,n+(i+1)%n,(i+1)%n) for i in range(n)],mat,False,uv)
# All water surfaces share one elevation; adjacent, non-overlapping color bands.
ring('Water__Shallows',-1,15,-.42,shallow)
ring('Water__Lagoon',15,48,-.42,lagoon)
ring('Water__Ocean',48,2200,-.42,sea)
for i,(d,w) in enumerate([(0,1.1),(3.8,.38),(10,.24)]):ring('Water__Foam',d,d+w,-.38+i*.002,foam,n=384)
# Delicate offshore strokes break up the large surface without transparency.
for i in range(260):
    a=random.uniform(0,2*PI);r=random.uniform(215,680);x=math.cos(a)*r;y=math.sin(a)*r
    if r<radius(a)+19:continue
    length=random.uniform(.8,4.8)
    beam('Water__Glints',(x,y,-.36),(x+length,y+.2,-.36),random.uniform(.025,.09),foam,4)

# Coastal limestone clusters and turf-capped sea stacks.
def rock_stack(x,y,z,sx,sy,h,idx,cap=True,collision=True):
    name='Rocks__Cluster_%02d'%(idx%16)
    n=9;v=[]
    for j,(scale,zh) in enumerate([(1.02,0),(1.08,.18),(.91,.67),(.66,.94),(.27,1.03)]):
        for i in range(n):
            a=i*2*PI/n+.13*math.sin(j+idx);w=1+.09*math.sin(i*4+idx)
            v.append((x+math.cos(a)*sx*scale*w,y+math.sin(a)*sy*scale*w,z+h*zh))
    faces=[tuple(range(n-1,-1,-1)),tuple(range(4*n,5*n))]
    faces += [(j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i) for j in range(4) for i in range(n)]
    batch(name).add(v,faces,rock if idx%3 else rocklight,True)
    # Thin soft turf cushions; never spherical bushes masquerading as rocks.
    if cap:
        blob('Rocks__Turf_%02d'%(idx%16),(x,y,z+h*.98),(sx*.70,sy*.7,h*.10),grass,idx,2)
        # Soft overhanging grass fingers break the cap's rigid silhouette.
        for k in range(5):
            a=k*2*PI/5+idx;blob('Rocks__Turf_%02d'%(idx%16),(x+math.cos(a)*sx*.53,y+math.sin(a)*sy*.53,z+h*.93),(sx*.25,sy*.24,h*.095),grass,idx+k,1)
    if collision:obstacles.append(dict(x=x,y=y,z=z+h*.42,rx=sx*.85,ry=sy*.85,height=h*.92))
for i in range(80):
    a=i*2*PI/80;r=radius(a)*random.uniform(.80,.87);x=r*math.cos(a);y=r*math.sin(a)
    if y<-125 and abs(x)<70:continue
    rock_stack(x,y,float(height(x,y))-.7,random.uniform(4,9),random.uniform(3,7),random.uniform(4,11),i)
for i,(x,y,sx,sy,h) in enumerate([(122,98,18,15,37),(146,105,13,12,25),(120,131,13,16,29),(-149,50,15,13,24),(-132,72,10,11,15),(196,118,11,12,20),(-216,23,12,10,16)]):
    rock_stack(x,y,float(height(x,y)) if math.hypot(x,y)<180 else -1,sx,sy,h,100+i)
# A real open arch in a separate offshore islet, assembled from tapered stone blocks.
for j in range(13):
    a=j*PI/12;x=-262+29*math.cos(a);z=3+42*math.sin(a)
    blob('Landmarks__SeaArch',(x,84,z),(9,12,8.5),rocklight,j,2)
    if j in (4,5,6,7,8):blob('Landmarks__ArchTurf',(x,84,z+7),(8,11,2.1),grass,j,2)
for i,(x,y,h) in enumerate([(-285,85,9),(-241,83,9),(-274,99,13),(-329,134,16),(282,194,22),(310,224,13),(-120,350,26),(170,386,43)]):rock_stack(x,y,-3,12,10,h,200+i,collision=False)

# Cascading spring: a pool nestled among the east rocks, then a broad ribbon to sea.
# Plan follows the outside of the walkable course, avoiding a hidden gameplay barrier.
cx,cy=154,89
blob('Landmarks__PoolBasin',(cx,cy,6),(15,13,7.4),rock,42,2)
v=[(cx,cy,14.6)]+[(cx+12*math.cos(i*2*PI/48),cy+10*math.sin(i*2*PI/48),14.6) for i in range(48)]
batch('Water__SpringPool').add(v,[(0,i+1,(i+1)%48+1) for i in range(48)],pool)
points=[(154,89,14.65),(166,89,14.4),(176,89,12),(182,89,7),(185,89,1),(192,89,-.29)]
v=[]
for x,y,z in points:v.extend([(x,y-4,z),(x,y+4,z)])
uv=[(j,i*.7) for i in range(len(points)) for j in (0,1)]
batch('Water__Cascade').add(v,[(2*i,2*i+2,2*i+3,2*i+1) for i in range(len(points)-1)],cascade,True,uv)
for i in range(7):
    off=-3.5+i*1.13
    for p1,p2 in zip(points,points[1:]):beam('Water__CascadeFoam',(p1[0],p1[1]+off,p1[2]+.06),(p2[0],p2[1]+off,p2[2]+.06),.06+(i%3)*.035,foam,5)
for i in range(16):blob('Water__Splash',(190+random.uniform(-3,6),89+random.uniform(-6,6),-.1),(random.uniform(.5,1.7),random.uniform(.5,1.5),random.uniform(.15,.5)),foam,i,1)
# Spring rim hides the water disk's edge and grounds it in the limestone basin.
for i in range(22):
    a=i*2*PI/22
    if abs(math.sin(a))<.36 and math.cos(a)>0:continue
    blob('Landmarks__SpringRim',(cx+12*math.cos(a),cy+10*math.sin(a),14.35),(1.9,1.7,.8),rocklight,i,2)

def palm(x,y,h,idx):
    z=float(height(x,y));name='Palms__Grove_%02d'%(idx%12);lean=Vector((math.sin(idx)*h*.19,math.cos(idx)*h*.13,0))
    pts=[Vector((x,y,z))+lean*(t*t)+Vector((0,0,h*t)) for t in np.linspace(0,1,9)]
    for i in range(8):
        rr=(.48-.022*i)*h/11;beam(name,pts[i],pts[i+1],rr,bark,8,rr*.94)
        beam(name,pts[i],pts[i]+Vector((0,0,.085)),rr*1.045,barklight,8)
    top=pts[-1]
    for i in range(9):frond(name,top,i*2*PI/9+idx,h*.49,h*.10,leaf if i%3 else leaflight,lift=h*.14,droop=h*.15)
    for i in range(3):blob(name,tuple(top+Vector((math.cos(i*2.1)*.4,math.sin(i*2.1)*.4,-.3))),(.36,.36,.42),bark,idx+i,1)
    obstacles.append(dict(x=x,y=y,z=z+h*.38,rx=.47*h/11,ry=.47*h/11,height=h*.8))
    return top
PALMS=[(-42,-155,12),(26,-150,14),(-57,-126,10),(60,-101,14),(-62,-66,13),(65,-9,12),(-52,51,13),(63,112,14),(42,152,11),(-37,156,12),(-108,-89,12),(-120,-63,11)]
for i in range(42):
    a=random.uniform(0,2*PI);r=radius(a)*random.uniform(.64,.79);PALMS.append((r*math.cos(a),r*math.sin(a),random.uniform(9,16)))
PALMS=[(x,y,h) for x,y,h in PALMS if not any(((x-o['x'])/(o['rx']+2))**2+((y-o['y'])/(o['ry']+2))**2<1 for o in obstacles)]
for i,(x,y,h) in enumerate(PALMS):palm(x,y,h,i)

def plant(x,y,s,idx,flower=False):
    z=float(height(x,y))+.03;name='Garden__Patch_%02d_%02d'%(int((x+220)//40),int((y+220)//40))
    for i in range(5):frond(name,(x,y,z),i*2*PI/5+idx,s*.9,s*.20,stem if i%2 else leaf,lift=s*.65,droop=s*.05)
    if flower:
        for j in range(3):
            xx=x+math.cos(j*2.1)*s*.3;yy=y+math.sin(j*2.1)*s*.3;zz=z+s*(.45+j*.1)
            beam(name,(xx,yy,z),(xx,yy,zz),s*.025,stem,5)
            col=[gold,pink,purple][idx%3]
            for k in range(5):
                a=k*2*PI/5;blob(name,(xx+math.cos(a)*s*.18,yy+math.sin(a)*s*.18,zz),(s*.17,s*.17,s*.095),col,k,1)
            blob(name,(xx,yy,zz+.065*s),(s*.12,s*.12,s*.10),gold,0,1)
for i,(x,y,h) in enumerate(PALMS):
    for j in range(5):
        a=j*2*PI/5;plant(x+math.cos(a)*2.2,y+math.sin(a)*2.2,random.uniform(1.15,2.0),i*5+j,True)
for i in range(130):
    y=random.uniform(-155,155);side=1 if i%2 else -1;x=float(route(y))+side*(float(width(y))+random.uniform(8,17))
    if min(float(bunker_distance(x,y,b)) for b in BUNKERS)<1.4:continue
    plant(x,y,random.uniform(.45,1.1),i,True)

# Readable, deliberately small course furniture. No objects obstruct the fairway.
def pin(x,y,idx):
    z=float(height(x,y));name='Course__Flags'
    # dark inset cup with ivory rim
    beam(name,(x,y,z-.03),(x,y,z+.035),.21,dark,24)
    beam(name,(x,y,z),(x,y,z+4.1),.065,white,10)
    v=[(x,y,z+4),(x+1.6,y+.20,z+3.65),(x+1.1,y+.05,z+3.12),(x,y,z+3.05)]
    batch(name).add(v+v,[(0,1,2,3),(7,6,5,4)],flag)
pin(4,134,0);pin(-105,-71,1)
for x in (-25,-1):
    y=-151;z=float(height(x,y));blob('Course__TeeMarkers',(x,y,z+.3),(.55,.4,.33),white,1,2)
    blob('Course__TeeMarkers',(x,y,z+.32),(.28,.30,.35),flag,1,1)
# Arrival path rope fence frames the initial view; openings remain generous.
for side in (-1,1):
    pts=[]
    for i in range(6):
        x=-13+side*(23+i*.7);y=-163+i*5;z=float(height(x,y))
        beam('Course__RopePosts',(x,y,z),(x,y,z+1.35),.16,bark,9)
        blob('Course__RopePosts',(x,y,z+1.35),(.2,.2,.14),barklight,i,1);pts.append((x,y,z+1))
    for p1,p2 in zip(pts,pts[1:]):
        prev=p1
        for j in range(1,7):
            t=j/6;p=tuple(p1[k]*(1-t)+p2[k]*t-(.24*math.sin(t*PI) if k==2 else 0) for k in range(3));beam('Course__Ropes',prev,p,.045,white,5);prev=p

# Cloud sculptures and a distant silhouette put the island into a bright seascape.
for i in range(14):
    a=i*2*PI/14;r=random.uniform(550,900);x=r*math.cos(a);y=r*math.sin(a);z=random.uniform(100,150)
    for j in range(5):blob('Backdrop__Clouds',(x+(j-2)*13,y,z+math.sin(j)*5),(20,12,9+(j%3)*4),white,i+j,2)
for i in range(9):rock_stack(-40+i*19,560+math.sin(i)*15,-5,30,28,42+58*math.sin(i*PI/8),300+i,collision=False)

objects=[b.finish() for b in batches.values()]
for o in objects:
    o['authoring']='Tidebloom / metre scale / UV0 portable materials'
bpy.ops.object.select_all(action='DESELECT')
for o in objects:o.select_set(True)
bpy.context.view_layer.objects.active=objects[0]
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Game/Assets/_Game/Art/GolfIsland.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='STRIP',use_mesh_modifiers=True)

shore=[]
for i in range(192):
    a=2*PI*i/192;r=float(radius(a))-.6;x=r*math.cos(a);y=r*math.sin(a);shore.append(dict(x=x,y=float(height(x,y)),z=y))
spawns=[dict(x=-13+(n%5-2)*2,y=float(height(-13+(n%5-2)*2,-146+(n//5)*3))+1,z=-146+(n//5)*3) for n in range(10)]
manifest=dict(name='Tidebloom Island',materials=specs,shoreline=shore,spawns=spawns,
    obstacles=[dict(x=o['x'],y=o['z'],z=o['y'],rx=o['rx'],rz=o['ry'],height=o['height']) for o in obstacles],
    palms=len(PALMS),bunkers=len(BUNKERS),source='ArtSource/Golf/Tidebloom/Island_Tidebloom.blend',
    meshes=len(objects),vertices=sum(len(o.data.vertices) for o in objects),triangles=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in objects))
(OUT/'unity-import.json').write_text(json.dumps(manifest,indent=2)+'\n')
(SRC/'island-manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
(ROOT/'Builds/golf-asset-audit.json').write_text(json.dumps({k:v for k,v in manifest.items() if k not in ('materials','shoreline','obstacles')},indent=2)+'\n')

# Review cameras and lighting are source-only and never exported to the game.
scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.32,.58,.8,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.8
bpy.ops.object.light_add(type='SUN');sun=bpy.context.object;sun.name='Warm coastal sun';sun.rotation_euler=(.5,-.4,-.65);sun.data.energy=2.8;sun.data.angle=.15
def camera(name,pos,look,lens):
    bpy.ops.object.camera_add(location=pos);cam=bpy.context.object;cam.name=name;cam.rotation_euler=(Vector(look)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=lens;cam.data.clip_end=4000;return cam
views=[('01_Island_Overview',(340,-440,330),(0,10,0),42),('02_Fairway',(8,-184,31),(0,35,7),29),('03_Cascade',(232,12,49),(139,101,16),44),('04_Garden',(-45,-173,11),(-30,-141,8),43)]
cams=[camera(*v) for v in views];scene.camera=cams[0]
scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True
scene.render.resolution_x=1600;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
bpy.ops.wm.save_as_mainfile(filepath=str(SRC/'Island_Tidebloom.blend'))
register=ROOT/'ArtSource/asset-register.json';data=json.loads(register.read_text());data.update({'GolfIsland.fbx':'Golf/Tidebloom/Island_Tidebloom.blend','golf_refined_source':'Golf/Tidebloom/Island_Tidebloom.blend','golf_refined_manifest':'Golf/Tidebloom/island-manifest.json','golf_refined_exports':'Game/Assets/_Game/Art/Golf/Tidebloom/','island_metres':[round(max(p[k] for p in shore)-min(p[k] for p in shore),2) for k in ('x','z')]});register.write_text(json.dumps(data,indent=2)+'\n')
print('TIDEBLOOM_COMPLETE',manifest['meshes'],manifest['vertices'],manifest['triangles'],flush=True)
if '--render' in sys.argv:
    for cam,(name,*_) in zip(cams,views):
        scene.camera=cam;scene.render.filepath=str(DOC/(name+'.png'));bpy.ops.render.render(write_still=True)
