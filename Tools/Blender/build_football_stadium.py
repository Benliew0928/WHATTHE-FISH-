"""Sunvale stadium: deterministic, modular production meshes and Blender review scene.

Blender --background --python Tools/Blender/build_football_stadium.py
Pass -- --render to also render the three review cameras. Existing prototype is preserved.
"""
import bpy, bmesh, math, json, sys, random
import numpy as np
from pathlib import Path
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'ArtSource/Football/Sunvale'
EXPORT = ROOT / 'Game/Assets/_Game/Art/Football/Sunvale'
REVIEW = ROOT / 'Docs/VisualDirection/Stadium'
for folder in (SOURCE, EXPORT, REVIEW): folder.mkdir(parents=True, exist_ok=True)
random.seed(73)
PI = math.pi
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for collection in list(bpy.data.collections):
    if collection.name != 'Collection': bpy.data.collections.remove(collection)

# One shared painted palette atlas: deliberately broad pigments, no photographic noise.
# Values are sRGB art direction; generated image buffers use those encoded values.
COLORS = [
    ('Ivory', '#F4DEA9'), ('Limestone', '#D6BD90'), ('Chalk', '#FFF0C9'), ('ShadowStone', '#859FA4'),
    ('TealRoof', '#328A98'), ('RoofShade', '#2D687F'), ('DeepTeal', '#234E67'), ('Seafoam', '#77D9CC'),
    ('Cyan', '#39C4DF'), ('Azure', '#438CE1'), ('Violet', '#8671D5'), ('Lilac', '#AF90E3'),
    ('Coral', '#F18079'), ('Rose', '#DF6194'), ('Gold', '#F5C85C'), ('Apricot', '#EDA56E'),
    ('Leaf', '#6AA744'), ('LeafLight', '#99C957'), ('LeafShade', '#3B8261'), ('Bark', '#9A795C'),
    ('Turf', '#67AD44'), ('TurfLight', '#83BD4E'), ('TurfDark', '#4E963F'), ('FieldWhite', '#FFF5D6'),
    ('Navy', '#253F59'), ('Lamp', '#FFF4D5'), ('Path', '#A5B9B1'), ('Terrace', '#B8C5BB'),
    ('Turquoise', '#27AEAF'), ('BlueShade', '#3D70B4'), ('Clay', '#D99572'), ('Mint', '#B1D9BC')]
INDEX = {n:i for i,(n,_) in enumerate(COLORS)}
size=1024; tile=128
pixels=np.ones((size//2,size,4),dtype=np.float32)
yy,xx=np.mgrid[0:tile,0:tile]/tile
for i,(name,h) in enumerate(COLORS):
    base=np.array([int(h[j:j+2],16)/255 for j in (1,3,5)])
    wash=(.95+.055*yy+.018*np.sin(xx*8+yy*11)+.014*np.cos(xx*23-yy*7))
    if name in ('Cyan','Azure','Violet','Lilac','Coral','Rose','Gold','Apricot','Seafoam','Turquoise'):
        wash=.97+.03*yy+.005*np.sin(xx*8+yy*11)
    if name in ('Ivory','Limestone','Chalk','Terrace','ShadowStone'):
        wash-= .045*np.exp(-((xx-.27)**2+(yy-.55)**2)/.012)
        wash+= .035*np.exp(-((xx-.72)**2+(yy-.72)**2)/.025)
    if name.startswith('Turf'):
        wash += .027*np.sin(xx*52+np.sin(yy*19))*np.sin(yy*36)
    rgb=np.clip(base[None,None,:]*wash[:,:,None],0,1)
    row,col=divmod(i,8)
    pixels[row*tile:(row+1)*tile,col*tile:(col+1)*tile,:3]=rgb
atlas=bpy.data.images.new('Sunvale_PaintedPalette',width=size,height=size//2,alpha=False)
atlas.pixels.foreach_set(pixels.ravel()); atlas.filepath_raw=str(EXPORT/'Sunvale_PaintedPalette.png')
atlas.file_format='PNG'; atlas.save(); atlas.pack()
material=bpy.data.materials.new('Sunvale_Painted'); material.use_nodes=True
bs=material.node_tree.nodes.get('Principled BSDF'); bs.inputs['Roughness'].default_value=.78
tex=material.node_tree.nodes.new('ShaderNodeTexImage');tex.image=atlas;tex.interpolation='Linear'
material.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])

modules={}; batches={}; current=None; mapper=lambda p:p
def module(name):
    global current
    if name not in modules:
        c=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(c)
        root=bpy.data.objects.new(name,None);c.objects.link(root)
        root['module']=name;root['units']='metres';root['origin']='pitch centre / ground level'
        modules[name]=(c,root)
    current=name

class Batch:
    def __init__(self,name): self.name=name;self.v=[];self.f=[];self.uv=[];self.smooth=[]
    def add(self,verts,faces,color,smooth=False,uv_axes=None):
        offset=len(self.v);used=sorted({i for f in faces for i in f});remap={j:offset+i for i,j in enumerate(used)}
        self.v.extend([mapper(verts[j]) for j in used])
        # Per primitive planar UVs fit inside a generously padded atlas tile.
        vs=np.array(verts);lo=vs.min(axis=0);span=np.maximum(vs.max(axis=0)-lo,.00001)
        for face_index,face in enumerate(faces):
            idx=INDEX[color[face_index] if isinstance(color,list) else color];col=idx%8;row=idx//8
            self.f.append(tuple(remap[i] for i in face));self.smooth.append(smooth)
            n=(Vector(verts[face[1]])-Vector(verts[face[0]])).cross(Vector(verts[face[2]])-Vector(verts[face[0]]))
            axes=uv_axes or [a for a in range(3) if a!=max(range(3),key=lambda a:abs(n[a]))]
            self.uv.append([((col+.1+.8*(verts[j][axes[0]]-lo[axes[0]])/span[axes[0]])/8,
                             (row+.1+.8*(verts[j][axes[1]]-lo[axes[1]])/span[axes[1]])/4) for j in face])
    def finish(self):
        mesh=bpy.data.meshes.new(self.name);mesh.from_pydata(self.v,[],self.f);mesh.update()
        uv=mesh.uv_layers.new(name='PaintedPaletteUV')
        for poly,coords,smooth in zip(mesh.polygons,self.uv,self.smooth):
            poly.use_smooth=smooth
            for loop,co in zip(poly.loop_indices,coords):uv.data[loop].uv=co
        # Recalculate solids only. Open graphic faces retain authored winding;
        # recalculating coplanar lettering can turn it away from Unity's camera.
        bm=bmesh.new();bm.from_mesh(mesh);pending=set(bm.faces)
        while pending:
            seed=pending.pop();component={seed};queue=[seed]
            while queue:
                face=queue.pop()
                for edge in face.edges:
                    for neighbour in edge.link_faces:
                        if neighbour in pending:
                            pending.remove(neighbour);component.add(neighbour);queue.append(neighbour)
            if all(len(e.link_faces)==2 for f in component for e in f.edges):
                bmesh.ops.recalc_face_normals(bm,faces=list(component))
        bm.to_mesh(mesh);bm.free()
        o=bpy.data.objects.new(self.name,mesh);modules[self.name.split('__')[0]][0].objects.link(o)
        o.parent=modules[self.name.split('__')[0]][1];mesh.materials.append(material)
        o['part']=self.name.split('__',1)[1];return o

def batch(name):
    key=current+'__'+name
    if key not in batches:batches[key]=Batch(key)
    return batches[key]

templates={}
def box(part,loc,dim,color,bevel=0):
    # Thin inset panels must never bevel through their opposite face.
    bevel=min(bevel,min(dim)*.45)
    key=(*dim,bevel)
    if key not in templates:
        bm=bmesh.new();bmesh.ops.create_cube(bm,size=1)
        for v in bm.verts:v.co=Vector((v.co.x*dim[0],v.co.y*dim[1],v.co.z*dim[2]))
        if bevel:bmesh.ops.bevel(bm,geom=list(bm.edges),offset=bevel,segments=2,affect='EDGES')
        bm.verts.ensure_lookup_table();bm.verts.index_update()
        templates[key]=([tuple(v.co) for v in bm.verts],[tuple(v.index for v in f.verts) for f in bm.faces]);bm.free()
    verts,faces=templates[key]
    batch(part).add([tuple(v[j]+loc[j] for j in range(3)) for v in verts],faces,color)

def beam(part,a,b,r,color,n=8):
    d=Vector(b)-Vector(a);q=d.to_track_quat('Z','Y');mid=(Vector(a)+Vector(b))/2
    v=[tuple(mid+q@Vector((r*math.cos(i*2*PI/n),r*math.sin(i*2*PI/n),z))) for z in (-d.length/2,d.length/2) for i in range(n)]
    f=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    batch(part).add(v,f,color,True)

def sphere(part,loc,scale,color,n=10,rings=6):
    v=[(loc[0],loc[1],loc[2]-scale[2])]
    for j in range(1,rings):
        t=-PI/2+j*PI/rings
        v += [(loc[0]+scale[0]*math.cos(t)*math.cos(i*2*PI/n),loc[1]+scale[1]*math.cos(t)*math.sin(i*2*PI/n),loc[2]+scale[2]*math.sin(t)) for i in range(n)]
    v.append((loc[0],loc[1],loc[2]+scale[2]));top=len(v)-1
    f=[(0,1+(i+1)%n,1+i) for i in range(n)]
    for j in range(rings-2):
        a=1+j*n; b=a+n; f += [(a+i,a+(i+1)%n,b+(i+1)%n,b+i) for i in range(n)]
    f += [(top,top-n+i,top-n+(i+1)%n) for i in range(n)]
    batch(part).add(v,f,color,True)

def arch(part,cx,depth,base,width,spring,top,thick,color):
    # Watertight wall above a true semicircular opening, plus two legs.
    radius=width/2;N=16
    xs=[cx+radius*math.cos(PI-i*PI/N) for i in range(N+1)]
    zs=[spring+radius*math.sin(PI-i*PI/N) for i in range(N+1)]
    verts=[]
    for y in (depth-thick/2,depth+thick/2):
        for x,z in zip(xs,zs):verts.extend([(x,y,z),(x,y,top(x) if callable(top) else top)])
    k=(N+1)*2;faces=[]
    for i in range(N):
        a=i*2;b=a+2
        faces.extend([(a,b,b+1,a+1),(k+a+1,k+b+1,k+b,k+a),(a,k+a,k+b,b),(a+1,b+1,k+b+1,k+a+1)])
    faces += [(0,1,k+1,k),(2*N,k+2*N,k+2*N+1,2*N+1)]
    batch(part).add(verts,faces,color)
    for s in (-1,1):box(part,(cx+s*(radius+.28),depth,(spring+base)/2),(.56,thick,spring-base),color,.07)

def arch_trim(part,cx,depth,spring,radius,width,color):
    for i in range(16):
        t=PI-i*PI/16;u=PI-(i+1)*PI/16
        v=[(cx+r*math.cos(a),depth+y,spring+r*math.sin(a)) for y in (-.12,.12) for r,a in ((radius,t),(radius,u),(radius+width,u),(radius+width,t))]
        batch(part).add(v,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],color if i%4 else 'Chalk')

def seat_shell(part,loc,dim,color,axis):
    # Chamfered eight-point molded outline: 28 triangles, independent of bevel modifiers.
    axes=[i for i in range(3) if i!=axis];a=dim[axes[0]]/2;b=dim[axes[1]]/2;r=.075
    outline=[(-a+r,-b),(a-r,-b),(a,-b+r),(a,b-r),(a-r,b),(-a+r,b),(-a,b-r),(-a,-b+r)]
    v=[]
    for depth in (-dim[axis]/2,dim[axis]/2):
        for x,y in outline:
            p=list(loc);p[axis]+=depth;p[axes[0]]+=x;p[axes[1]]+=y;v.append(tuple(p))
    faces=[tuple(range(7,-1,-1)),tuple(range(8,16))]+[(i,(i+1)%8,(i+1)%8+8,i+8) for i in range(8)]
    batch(part).add(v,faces,color)

# Rounded rectangle arc-length parametrisation, counter-clockwise.
X,Y,R=42,61,17
segments=[]
def straight(a,b):segments.append(('line',a,b,(Vector(b)-Vector(a)).length))
def corner(c,t):segments.append(('arc',c,t,R*PI/2))
straight((X,-Y+R),(X,Y-R));corner((X-R,Y-R),0)
straight((X-R,Y),(-X+R,Y));corner((-X+R,Y-R),PI/2)
straight((-X,Y-R),(-X,-Y+R));corner((-X+R,-Y+R),PI)
straight((-X+R,-Y),(X-R,-Y));corner((X-R,-Y+R),3*PI/2)
PERIM=sum(s[3] for s in segments);BAYS=32;BAY=PERIM/BAYS
def path(s,depth,z):
    s%=PERIM
    for kind,a,b,length in segments:
        if s<=length:
            t=s/length
            if kind=='line':
                tx=(b[0]-a[0])/length;ty=(b[1]-a[1])/length
                return (a[0]+(b[0]-a[0])*t+ty*depth,a[1]+(b[1]-a[1])*t-tx*depth,z)
            angle=b+t*PI/2
            return (a[0]+(R+depth)*math.cos(angle),a[1]+(R+depth)*math.sin(angle),z)
        s-=length
    raise ValueError(s)

def patch(part,s0,s1,d0,d1,z0,z1,color,steps=6):
    # Closed curved rectangular slab; height interval z0/z1.
    verts=[path(s0+(s1-s0)*i/steps,d,z) for z in (z0,z1) for d in (d0,d1) for i in range(steps+1)]
    n=steps+1;faces=[]
    for i in range(steps):
        faces.extend([(i,i+1,n+i+1,n+i),(2*n+i,3*n+i,3*n+i+1,2*n+i+1),
                      (i,2*n+i,2*n+i+1,i+1),(n+i,n+i+1,3*n+i+1,3*n+i)])
    faces.extend([(0,n,3*n,2*n),(steps,2*n+steps,3*n+steps,n+steps)])
    batch(part).add(verts,faces,color)

SEAT_PALETTES=[('Azure','Cyan','Seafoam'),('Cyan','Turquoise','Seafoam'),('Gold','Apricot','Coral'),('Coral','Rose','Apricot'),('Violet','Lilac','Azure'),('Azure','Violet','Cyan'),('Seafoam','Cyan','Turquoise'),('Gold','Apricot','Coral')]
module('Stadium')
for bay in range(BAYS):
    s0=bay*BAY;s1=(bay+1)*BAY;mid=(s0+s1)/2
    mapper=lambda p:p
    part=f'Bay_{bay:02d}_Structure'
    patch(part,s0,s1,-5,18,-.4,-.045,'Limestone')
    patch(part,s0,s1,0,1.1,3.05,3.35,'Chalk')
    patch(part,s0,s1,10.4,14.5,7.95,8.25,'Ivory')
    patch(part,s0,s1,14.0,14.5,0,8.0,'Limestone')
    for row in range(8):
        d=1.35+row*1.12;z=3.3+row*.58
        patch(part,s0+.55,s1-.55,d,d+1.12,z-.58,z,'Terrace')
    # A curved lower arcade, not a solid front slab.
    mapper=lambda p,m=mid:path(m+p[0],p[1],p[2])
    if bay in (3,11,19,27):
        # Four larger entrance portals break the repeating concourse rhythm.
        arch(part,0,.25,0,4.8,.70,3.30,.90,'Ivory')
        arch_trim(part,0,-.28,.70,2.4,.28,'Chalk')
        for side in (-1,1):
            box(part,(side*4.5,.25,1.6),(2.1,.90,3.2),'Ivory',.12)
            box(part,(side*4.5,-.26,1.9),(1.20,.08,1.45),SEAT_PALETTES[bay//4][0],.09)
    else:
        for j in range(3):
            x=(j-1)*3.25
            arch(part,x,.25,0,2.6,1.2,3.12,.65,'Ivory')
            arch_trim(part,x,-.12,1.2,1.30,.20,'Limestone')
    for side in (-1,1):
        x=side*(BAY/2-.25)
        cheek=[(x+dx,y,z) for dx in (-.22,.22) for y,z in ((.5,0),(10.5,0),(10.5,8.0),(.5,3.25))]
        batch(part).add(cheek,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],'Ivory')
        for step in range(16):
            box(part,(side*(BAY/2-.23),1.35+step*.56,3.05+step*.29),(.96,.56,.26),'Chalk')
        beam(part,(side*(BAY/2-.23),1.5,4.0),(side*(BAY/2-.23),10,8.4),.045,'DeepTeal')
    # Upper promenade arcade and carved impost blocks.
    arch(part,0,13.6,8.25,BAY-1.15,10.15,lambda x:12.65+3.4*math.sin(PI*(x/BAY+.5))**.72+.035*10.1,.8,'Ivory')
    arch_trim(part,0,13.13,10.15,(BAY-1.15)/2,.28,'Chalk')
    for side in (-1,1):
        box(part,(side*(BAY/2-.30),13.6,10.16),(1.18,1.22,.30),'Chalk',.07)
        box(part,(side*(BAY/2-.30),13.6,8.4),(1.20,1.22,.32),'Limestone',.07)
    # Promenade railing between seats and arcade.
    beam(part,(-BAY/2,11.6,8.9),(BAY/2,11.6,8.9),.07,'Seafoam')
    for k in range(9):beam(part,(-BAY/2+k*BAY/8,11.6,8.2),(-BAY/2+k*BAY/8,11.6,8.9),.035,'DeepTeal')
    # Barrel-vault canopy with actual thickness; teal outside, cool underside.
    roof=f'Bay_{bay:02d}_Canopy'
    N=16;v=[]
    for layer in (0,.24):
        for depth in (3.5,16.4):
            for i in range(N+1):
                x=-BAY/2+i*BAY/N
                z=12.65+3.4*math.sin(PI*i/N)**.72+.035*(depth-3.5)+layer
                v.append((x,depth,z))
    n=N+1
    roof_faces=[];roof_colors=[]
    for i in range(N):
        roof_faces.append((i,i+1,n+i+1,n+i));roof_colors.append('RoofShade')
        roof_faces.append((2*n+i,3*n+i,3*n+i+1,2*n+i+1));roof_colors.append('TealRoof')
        for end in (0,n):
            roof_faces.append((end+i,end+i+2*n,end+i+1+2*n,end+i+1));roof_colors.append('Ivory')
        # Chunky cream scalloped fascia follows the vault.
        x=-BAY/2+i*BAY/N;xn=x+BAY/N
        z=12.65+3.4*math.sin(PI*i/N)**.72
        zn=12.65+3.4*math.sin(PI*(i+1)/N)**.72
        beam(roof,(x,3.42,z),(xn,3.42,zn),.20,'Chalk')
    for i in (0,N):
        roof_faces.append((i,n+i,3*n+i,2*n+i));roof_colors.append('Ivory')
    batch(roof).add(v,roof_faces,roof_colors,True,(0,1))
    for side in (-1,1):
        beam(roof,(side*BAY/2,3.45,12.7),(side*BAY/2,16.5,13.15),.16,'Limestone')
        # Sweeping canopy support forks leave the front view open.
        beam(part,(side*(BAY/2-.28),11.0,7.9),(side*BAY/2,9.6,12.8),.18,'Ivory')
        beam(part,(side*BAY/2,9.6,12.8),(side*BAY/2,3.6,12.7),.15,'Ivory')
    # Molded seats, three coordinated tones per zone, kept separate from architecture.
    colors=SEAT_PALETTES[bay//4]
    for row in range(8):
        for col in range(12):
            x=(col-5.5)*.71;d=1.63+row*1.12;z=3.3+row*.58
            c=colors[(col//4+row//3)%3]
            seat=f'Bay_{bay:02d}_Seats_LOD0'
            seat_shell(seat,(x,d,z+.25),(.60,.55,.14),c,2)
            seat_shell(seat,(x,d+.22,z+.57),(.60,.14,.60),c,1)
            box(seat,(x,d,z+.10),(.18,.22,.2),'DeepTeal')
            # Far tier uses one solid chamfered seat silhouette.
            box(f'Bay_{bay:02d}_Seats_LOD1',(x,d+.10,z+.40),(.57,.40,.58),c)
    # Embedded oversized stone accents, concentrated under the arcade.
    for j in range(7):
        x=random.uniform(-BAY/2+.7,BAY/2-.7)
        box(part,(x,-.105,random.uniform(2.65,2.98)),(.20+random.random()*.25,.045,.12),'Limestone',.025)

module('Banners')
for bay in range(0,BAYS,2):
    mid=(bay+.5)*BAY;mapper=lambda p,m=mid:path(m+p[0],p[1],p[2])
    c=SEAT_PALETTES[bay//4][0]
    beam(f'Flags_{bay:02d}',(-1.0,3.10,14.7),(1.0,3.10,14.7),.07,'Gold')
    # Draped cloth silhouette with a shallow chevron hem.
    verts=[(-.90,3.08,14.65),(.90,3.08,14.65),(.86,2.99,12.05),(0,2.94,11.73),(-.86,2.99,12.05)]
    batch(f'Flags_{bay:02d}').add(verts,[(0,1,2,3,4),(4,3,2,1,0)],c)
    # Sunburst badge, original venue motif, modeled so it survives export.
    v=[(0,2.91,13.25)]+[(math.sin(i*PI/8)*(.48 if i%2==0 else .28),2.9,13.25+math.cos(i*PI/8)*(.48 if i%2==0 else .28)) for i in range(16)]
    # The curved-bay mapping reflects the local XY frame, so +Y winding faces inward.
    batch(f'Flags_{bay:02d}').add(v,[(0,1+i,1+(i+1)%16) for i in range(16)],'Chalk')

module('Landscape')
for bay in range(BAYS):
    mid=(bay+.5)*BAY;mapper=lambda p,m=mid:path(m+p[0],p[1],p[2])
    part=f'Planters_{bay//4:02d}'
    if bay%2==0:
        box(part,(0,11.9,8.55),(3.3,1.3,.6),'Limestone',.18)
        for j in range(4):sphere(part,(-1.1+j*.75,11.9,9.04+random.random()*.2),(.85,.70,.75),('Leaf','LeafLight','LeafShade')[j%3])
    # Trees outside the stadium, individually removable landscape module.
    if bay%2==1:
        x=random.uniform(-1,1);d=19.8;h=random.uniform(14,18)
        beam(part,(x,d,0),(x-.2,d,h*.78),.40,'Bark')
        for j in range(4):sphere(part,(x+math.sin(j*2)*1.5,d+math.cos(j*2)*1.3,h-j*.8),(3.0,2.8,3.1),('Leaf','LeafLight','LeafShade','Leaf')[j])
    # Ivy gathered on piers, not scattered over playable turf.
    if bay%3==0:
        for j in range(8):sphere(part,(-BAY/2+.5+.22*math.sin(j),-.25,3.1-j*.31),(.3,.20,.42),'Leaf' if j%2 else 'LeafShade',8,4)

module('Perimeter')
for bay in range(BAYS):
    mid=(bay+.5)*BAY;mapper=lambda p,m=mid:path(m+p[0],p[1],p[2])
    # Low cushions preserve the goal sightline and sit outside the runoff.
    c=SEAT_PALETTES[bay//4][0]
    box(f'Boards_{bay//4:02d}',(0,-1.5,.48),(BAY-.30,.45,.95),'Ivory',.15)
    for j in (-1,0,1):box(f'Boards_{bay//4:02d}',(j*2.9,-1.75,.49),(2.65,.06,.70),c if j else 'Gold',.025)

module('Floodlights')
for i,s in enumerate([PERIM*.15,PERIM*.35,PERIM*.65,PERIM*.85]):
    mapper=lambda p,m=s:path(m+p[0],p[1],p[2]);part=f'Tower_{i:02d}'
    box(part,(0,15.1,.45),(2.3,2.2,.9),'Limestone',.24)
    for x in (-.6,.6):
        beam(part,(x,15.1,.8),(x,15.1,20),.16,'Seafoam')
        beam(part,(x,15.1,17.7),(x*2,15.1,19.4),.13,'Gold')
    box(part,(0,15.1,20.25),(5.2,.85,3.8),'Ivory',.38)
    box(part,(0,14.63,20.25),(4.68,.12,3.26),'DeepTeal',.32)
    for x in (-1.43,0,1.43):
        for z in (19.5,21):
            beam(part,(x,14.49,z),(x,14.29,z),.59,'Gold',16)
            sphere(part,(x,14.24,z),(.49,.12,.49),'Lamp',16,8)

module('Scoreboard')
mapper=lambda p:p
box('Frame',(0,74.8,20),(15,.95,5.0),'Ivory',.40)
# A small face bevel must stay below half the panel thickness.
box('Display',(0,74.0,20),(13.9,.20,3.95),'DeepTeal',.06)
for x in (-5,5):beam('Supports',(x,75,8),(x,75,18.4),.23,'Seafoam')
def text_mesh(text,loc,size,name):
    curve=bpy.data.curves.new(name,'FONT');curve.body=text;curve.align_x='CENTER';curve.align_y='CENTER';curve.size=size;curve.extrude=0;curve.bevel_depth=0;curve.resolution_u=4
    o=bpy.data.objects.new(name,curve);modules[current][0].objects.link(o);o.location=loc;o.rotation_euler=(PI/2,0,0)
    bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.convert(target='MESH');o=bpy.context.object
    verts=[tuple(o.matrix_world@v.co) for v in o.data.vertices];faces=[tuple(p.vertices) for p in o.data.polygons]
    batch('Lettering').add(verts,faces,'Chalk');bpy.data.objects.remove(o,do_unlink=True)
text_mesh('S U N V A L E',(0,73.82,20.6),1.35,'Venue wordmark')
text_mesh('F O O T B A L L   C L U B',(0,73.82,19.2),.45,'Club line')
sphere('Crest',(0,74.7,24),(1.75,.5,1.75),'Gold',24,12)
for i in range(12):
    t=i*PI/6
    beam('Crest',(math.sin(t)*2.04,74.7,24+math.cos(t)*2.04),(math.sin(t)*2.55,74.7,24+math.cos(t)*2.55),.12,'Gold')

module('Dugouts');mapper=lambda p:p
for side in (-1,1):
    # Sideline shelters beyond the playable touchline, with eight bucket seats each.
    centre=(side*38.7,side*14,0)
    angle=-PI/2 if side>0 else PI/2
    mapper=lambda p,c=centre,a=angle:(c[0]+p[0]*math.cos(a)-p[1]*math.sin(a),c[1]+p[0]*math.sin(a)+p[1]*math.cos(a),p[2])
    part='Shelter_East' if side>0 else 'Shelter_West'
    box(part,(0,.15,.1),(6.8,2.2,.20),'Ivory',.1)
    box(part,(0,1.05,1.05),(6.8,.16,1.9),'Seafoam',.08)
    for x in (-3.2,3.2):beam(part,(x,.9,.15),(x,.9,2.45),.09,'Gold')
    box(part,(0,.1,2.45),(7.1,2.5,.22),'TealRoof',.1)
    box(part,(0,-1.10,2.42),(7.1,.16,.33),'Chalk',.07)
    for i in range(8):
        x=(i-3.5)*.76
        seat_shell(part,(x,.2,.58),(.63,.57,.16),'Azure' if side>0 else 'Coral',2)
        seat_shell(part,(x,.44,.88),(.63,.14,.66),'Azure' if side>0 else 'Coral',1)
        box(part,(x,.2,.34),(.24,.30,.40),'DeepTeal')

# Separate lawn and reusable goal. These are supporting game-scale modules.
module('Lawn');mapper=lambda p:p
box('TurfBase',(0,0,-.20),(78,115,.36),'TurfDark',.1)
for i in range(14):box('Pitch',(0,-52.5+(i+.5)*7.5,-.014),(68,7.5,.028),'TurfLight' if i%2 else 'Turf')
def marking(points,width=.12):
    v=[];f=[]
    for a,b in zip(points,points[1:]):
        d=Vector((b[0]-a[0],b[1]-a[1]));d.normalize();n=Vector((-d.y,d.x))*width/2;k=len(v)
        v.extend([(a[0]+n.x,a[1]+n.y,.018),(a[0]-n.x,a[1]-n.y,.018),(b[0]-n.x,b[1]-n.y,.018),(b[0]+n.x,b[1]+n.y,.018)]);f.append((k,k+1,k+2,k+3))
    batch('Markings').add(v,f,'FieldWhite')
def circle(cx,cy,r,start=0,end=2*PI):marking([(cx+r*math.cos(start+(end-start)*i/96),cy+r*math.sin(start+(end-start)*i/96)) for i in range(97)])
marking([(-34,-52.5),(34,-52.5),(34,52.5),(-34,52.5),(-34,-52.5)]);marking([(-34,0),(34,0)]);circle(0,0,9.15)
sphere('Markings',(0,0,.018),(.12,.12,.003),'FieldWhite')
for s in (-1,1):
    for w,d in ((40.32,16.5),(18.32,5.5)):marking([(-w/2,s*52.5),(-w/2,s*(52.5-d)),(w/2,s*(52.5-d)),(w/2,s*52.5)])
    sphere('Markings',(0,s*41.5,.018),(.12,.12,.003),'FieldWhite')
    a=math.asin(5.5/9.15);circle(0,s*41.5,9.15,PI+a if s==1 else a,2*PI-a if s==1 else PI-a)
    for x in (-34,34):
        start=PI if x>0 and s>0 else (PI/2 if x>0 else (3*PI/2 if s>0 else 0))
        circle(x,s*52.5,1,start,start+PI/2)

module('Goal');mapper=lambda p:p
# Canonical origin: centre of goal line, facing -Y; 7.32 x 2.44 m clear opening.
for x in (-3.72,3.72):
    beam('Frame',(x,0,0),(x,0,2.5),.06,'FieldWhite',12)
    beam('Frame',(x,0,2.5),(x,2.2,2.05),.035,'FieldWhite')
    beam('Frame',(x,2.2,2.05),(x,2.2,.06),.035,'FieldWhite')
    beam('Frame',(x,0,.06),(x,2.2,.06),.035,'FieldWhite')
beam('Frame',(-3.72,0,2.5),(3.72,0,2.5),.06,'FieldWhite',12)
beam('Frame',(-3.72,2.2,.06),(3.72,2.2,.06),.035,'FieldWhite')
for i in range(38):
    x=-3.66+i*7.32/37
    beam('Net',(x,0,2.44),(x,2.2,2.01),.009,'FieldWhite',4)
    beam('Net',(x,2.2,2.01),(x,2.2,.06),.009,'FieldWhite',4)
for j in range(11):
    z=.06+j*1.95/10;beam('Net',(-3.66,2.2,z),(3.66,2.2,z),.009,'FieldWhite',4)
    y=j*2.2/10;beam('Net',(-3.66,y,2.44-y*.43/2.2),(3.66,y,2.44-y*.43/2.2),.009,'FieldWhite',4)
    for x in (-3.66,3.66):beam('Net',(x,0,z),(x,2.2,z*(2.01/2.44)),.009,'FieldWhite',4)
for x in (-3.66,3.66):
    for j in range(1,11):
        y=j*2.2/10;beam('Net',(x,y,.06),(x,y,2.44-y*.43/2.2),.009,'FieldWhite',4)

module('CornerFlags')
for s in (-1,1):
    for x in (-34,34):
        beam('Poles',(x,s*52.5,0),(x,s*52.5,1.55),.022,'Chalk')
        box('Flags',(x+.18,s*52.5,1.36),(.36,.025,.28),'Coral',.012)

# Write meshes only once after procedural construction.
objects=[b.finish() for b in batches.values()]
for o in objects:
    if 'LOD1' in o.name:o.hide_render=True;o.hide_set(True)
    o.data.calc_loop_triangles()
report={'name':'Sunvale Football Club','source':'Stadium_Sunvale.blend','units':'metres',
        'pitch':[68,105],'bays':BAYS,'seats':BAYS*8*12,'materials':1,'atlas':[1024,512],
        'modules':{},'coordinate_system':'Blender X width, Y length, Z up; FBX -Z forward / Y up',
        'notes':['Prototype stadium is preserved.','Unity performance requires device profiling.','Review lights and cameras are excluded from all exports.']}
for name,(collection,root) in modules.items():
    children=[o for o in objects if o.parent==root]
    report['modules'][name]={'meshes':len(children),'triangles_lod0':sum(len(o.data.loop_triangles) for o in children if 'LOD1' not in o.name),
                           'triangles_lod1_seats':sum(len(o.data.loop_triangles) for o in children if 'LOD1' in o.name),
                           'file':name+'.fbx','pivot':[0,0,0]}
    bpy.ops.object.select_all(action='DESELECT');root.select_set(True)
    for o in children:o.hide_set(False);o.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(EXPORT/(name+'.fbx')),use_selection=True,object_types={'EMPTY','MESH'},
        axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='RELATIVE',use_mesh_modifiers=True)
    for o in children:
        if 'LOD1' in o.name:o.hide_set(True)

# Assemble two goal instances in the source without changing the reusable export pivot.
goalroot=modules['Goal'][1];goalroot.location=(0,52.56,0)
copyroot=bpy.data.objects.new('Goal_South',None);modules['Goal'][0].objects.link(copyroot);copyroot.location=(0,-52.56,0);copyroot.rotation_euler.z=PI
for o in [o for o in objects if o.parent==goalroot]:
    duplicate=o.copy();duplicate.data=o.data;modules['Goal'][0].objects.link(duplicate);duplicate.parent=copyroot

# Review-only apron, lighting and cameras. Never exported as stadium geometry.
review=bpy.data.collections.new('REVIEW_ONLY');bpy.context.scene.collection.children.link(review)
def to_review(o):
    for c in list(o.users_collection):c.objects.unlink(o)
    review.objects.link(o)
bpy.ops.mesh.primitive_plane_add(size=2000,location=(0,0,-.44));o=bpy.context.object;o.name='Review ground';to_review(o)
m=bpy.data.materials.new('Review sage');m.diffuse_color=(.19,.32,.22,1);o.data.materials.append(m)
world=bpy.data.worlds.new('Soft blue daylight');bpy.context.scene.world=world;world.use_nodes=True
world.node_tree.nodes['Background'].inputs[0].default_value=(.32,.60,.95,1);world.node_tree.nodes['Background'].inputs[1].default_value=.8
bpy.ops.object.light_add(type='SUN',location=(0,0,40));sun=bpy.context.object;to_review(sun);sun.name='Warm afternoon';sun.rotation_euler=(.38,-.48,-.45);sun.data.energy=3;sun.data.angle=.12
bpy.ops.object.light_add(type='AREA',location=(0,-25,45));fill=bpy.context.object;to_review(fill);fill.data.energy=100000;fill.data.shape='DISK';fill.data.size=65
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True
scene.render.resolution_x=1600;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX';scene.view_settings.look='AgX - Medium High Contrast';scene.view_settings.exposure=.4
cameras=[]
for name,loc,look,lens in [
    ('01_Pitch_View',(13,-19,2.8),(-3,56,8),24),
    ('02_Bowl_Overview',(115,-145,112),(0,0,3),46),
    ('03_Architecture_Detail',(24,28,4.4),(40,37,9),28)]:
    bpy.ops.object.camera_add(location=loc);cam=bpy.context.object;to_review(cam);cam.name=name;cam.data.lens=lens;cam.data.clip_end=1500
    cam.rotation_euler=(Vector(look)-cam.location).to_track_quat('-Z','Y').to_euler();cameras.append(cam)
scene.camera=cameras[0]
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':area.spaces.active.region_3d.view_perspective='CAMERA'
scene['Design']='SUNVALE / warm carved limestone, sea-glass canopy, zoned jewel seating, sunburst textiles'
scene['Export']='Ten replaceable FBX modules; shared 1024 x 512 painted atlas. Goal origin is goal-line centre in its FBX.'
bpy.ops.object.select_all(action='DESELECT')
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'Stadium_Sunvale.blend'))
(SOURCE/'stadium-manifest.json').write_text(json.dumps(report,indent=2)+'\n')
print('SUNVALE_EXPORT_COMPLETE',json.dumps(report))
if '--render' in sys.argv:
    for cam in cameras:
        scene.camera=cam;scene.render.filepath=str(REVIEW/(cam.name+'.png'));bpy.ops.render.render(write_still=True)
    print('SUNVALE_RENDERS_COMPLETE')
