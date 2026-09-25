"""RALLY / modular basketball art. Blender 5.1, metres, no external dependencies.

blender --background --python Tools/Blender/build_rally_arena.py -- --render
Prototype and live game environment are deliberately separate from this art library.
"""
import bpy, bmesh, math, json, sys
import numpy as np
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'ArtSource/Basketball/Rally'
EXPORT = ROOT / 'Game/Assets/_Game/Art/Basketball/Rally'
REVIEW = ROOT / 'Docs/VisualDirection/Basketball'
for folder in (SOURCE, EXPORT, REVIEW): folder.mkdir(parents=True, exist_ok=True)
PI = math.pi
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
for c in list(bpy.data.collections): bpy.data.collections.remove(c)
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'; scene.unit_settings.scale_length = 1

COLORS = {'Ivory':'FFF1D5','Teal':'209DA9','Aqua':'44C4CB','DeepTeal':'215666',
 'Navy':'263C59','Blue':'486981','Lavender':'8D79C9','Violet':'6559A2',
 'Coral':'EF8076','Gold':'F5C356','Lime':'ACCE62','Orange':'EF713C',
 'White':'FFF7E7','Dark':'132A3E','Concrete':'778C9F','Glass':'ABD8DE'}
def srgb(h): return [int(h[i:i+2],16)/255 for i in (0,2,4)]
def linear(c): return c/12.92 if c<=.04045 else ((c+.055)/1.055)**2.4
def rgba(name): return (*[linear(c) for c in srgb(COLORS.get(name,name))],1)

# Authored, portable texture maps. No Blender-only noise nodes in game materials.
def texture(name, array, noncolor=False):
    h,w=array.shape[:2]; im=bpy.data.images.new(name,width=w,height=h,alpha=array.ndim==3 and array.shape[2]==4)
    if noncolor: im.colorspace_settings.name='Non-Color'
    pixels=np.ones((h,w,4),np.float32)
    if array.ndim==3 and array.shape[2]==4:pixels[:]=array
    else:pixels[:,:,:3]=array[:,:,None] if array.ndim==2 else array
    im.pixels.foreach_set(pixels.ravel());im.filepath_raw=str(EXPORT/(name+'.png'))
    im.file_format='PNG';im.save();im.pack();im.use_fake_user=True;return im
y,x=np.mgrid[0:256,0:256]/256
paint=texture('Rally_PaintedSurface',np.clip(.965+.012*np.sin(x*2*PI)*np.cos(y*4*PI)+.008*np.sin(x*52*PI)*np.sin(y*48*PI),0,1))
fabric=texture('Rally_WovenFabric',np.clip(.95+.018*np.cos(x*128*PI)+.018*np.cos(y*128*PI),0,1))
N=2048; yy,xx=np.mgrid[0:N,0:N]/N
col=np.floor(xx*30); u=xx*30-col
v=yy*14+(col%3)/3; row=np.floor(v); v-=row
variation=.97+.052*np.sin(col*24.17+row*12.76)
grain=.018*np.sin(xx*3200+4*np.sin(yy*22+col))+.011*np.sin(xx*12000+np.sin(yy*37)*3)
joint=(u<.011)|(u>.989)|(v<.004)|(v>.996)
base=np.array(srgb('F3BD70'))
wood_pixels=np.clip(base[None,None,:]*(variation+grain-.11*joint)[:,:,None],0,1)
wood=texture('Rally_HoneyMaple_BaseColor',wood_pixels)
wood_rough=texture('Rally_HoneyMaple_Roughness',np.clip(.30+.05*np.sin(col*24.17+row*12.76)+.13*joint,.23,.5),True)
packed=np.zeros((N,N,4),np.float32);packed[:,:,3]=1-np.clip(.30+.05*np.sin(col*24.17+row*12.76)+.13*joint,.23,.5)
wood_mask=texture('Rally_HoneyMaple_MetallicSmoothness',packed,True);del packed
del wood_pixels,xx,yy,col,u,v,row,joint,variation,grain
materials={}; material_specs={}
def mat(role,color='Ivory',rough=.55,tex='paint',metal=0,emission=0):
    name='Rally_'+current+'_'+role
    if name in materials:return materials[name]
    m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=rgba(color)
    bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=rgba(color)
    bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=metal
    im={'paint':paint,'fabric':fabric,'wood':wood}.get(tex)
    if im:
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=im
        multiply=m.node_tree.nodes.new('ShaderNodeMixRGB');multiply.blend_type='MULTIPLY';multiply.inputs[0].default_value=1
        multiply.inputs[2].default_value=rgba(color);m.node_tree.links.new(t.outputs['Color'],multiply.inputs[1]);m.node_tree.links.new(multiply.outputs[0],bs.inputs['Base Color'])
    if tex=='wood':
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=wood_rough;m.node_tree.links.new(t.outputs['Color'],bs.inputs['Roughness'])
    if emission:
        bs.inputs['Emission Color'].default_value=rgba(color);bs.inputs['Emission Strength'].default_value=emission
    materials[name]=m;material_specs[name]={'color_srgb':srgb(COLORS.get(color,color)), 'roughness':rough,'metallic':metal,'emission':emission,'base_map':im.name+'.png' if im else None,'roughness_map':wood_rough.name+'.png' if tex=='wood' else None}
    return m

modules={};batches={};current=None;mapper=lambda p:p
def module(name):
    global current,mapper
    current=name;mapper=lambda p:p
    c=bpy.data.collections.new(name);scene.collection.children.link(c)
    root=bpy.data.objects.new(name,None);c.objects.link(root)
    root['module']=name;root['units']='metres';root['customization']='Named material slots; replace whole module or individual role mesh'
    modules[name]=(c,root)

class Batch:
    def __init__(self,name):self.name=name;self.v=[];self.f=[];self.uv=[];self.mi=[];self.mats=[];self.smooth=[]
    def add(self,verts,faces,material,smooth=False,uvs=None):
        offset=len(self.v);self.v.extend([mapper(v) for v in verts])
        if material not in self.mats:self.mats.append(material)
        vs=np.array(verts);lo=vs.min(axis=0);span=np.maximum(vs.max(axis=0)-lo,1e-6)
        # The perimeter coordinate system has an outward Y axis and reverses
        # handedness. Keep authored one-sided prints facing into the arena.
        origin=Vector(mapper((0,0,0)))
        axes=[Vector(mapper(tuple(1 if j==i else 0 for j in range(3))))-origin for i in range(3)]
        mirrored=axes[0].cross(axes[1]).dot(axes[2])<0
        for fi,face in enumerate(faces):
            if mirrored:face=tuple(reversed(face))
            self.f.append(tuple(offset+i for i in face));self.mi.append(self.mats.index(material));self.smooth.append(smooth[fi] if isinstance(smooth,list) else smooth)
            n=(Vector(verts[face[1]])-Vector(verts[face[0]])).cross(Vector(verts[face[2]])-Vector(verts[face[0]]))
            axes=[a for a in range(3) if a!=max(range(3),key=lambda a:abs(n[a]))]
            self.uv.append([uvs[i] if uvs else tuple((verts[i][a]-lo[a])/span[a] for a in axes) for i in face])
    def finish(self):
        mesh=bpy.data.meshes.new(self.name);mesh.from_pydata(self.v,[],self.f);mesh.update()
        uv=mesh.uv_layers.new(name='UV0')
        for m in self.mats:mesh.materials.append(m)
        for poly,coords,mi,smooth in zip(mesh.polygons,self.uv,self.mi,self.smooth):
            poly.material_index=mi;poly.use_smooth=smooth
            for loop,co in zip(poly.loop_indices,coords):uv.data[loop].uv=co
        # Solids and authored graphics are kept in separate meshes.
        if not any(s in self.name for s in ('Graphics','Markings','Lettering','Maple','Cloth','Emblem')):
            bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
        o=bpy.data.objects.new(self.name,mesh);c,root=modules[self.name.split('__')[0]];c.objects.link(o);o.parent=root
        o['part']=self.name.split('__')[1];return o
def batch(part):
    key=current+'__'+part
    if key not in batches:batches[key]=Batch(key)
    return batches[key]
templates={}
def box(part,loc,dim,m,bevel=0):
    bevel=min(bevel,min(dim)*.44);key=(*dim,bevel)
    if key not in templates:
        bm=bmesh.new();bmesh.ops.create_cube(bm,size=1)
        for v in bm.verts:v.co=Vector(tuple(v.co[i]*dim[i] for i in range(3)))
        if bevel:bmesh.ops.bevel(bm,geom=list(bm.edges),offset=bevel,segments=3,affect='EDGES')
        bm.verts.ensure_lookup_table();bm.verts.index_update()
        templates[key]=([tuple(v.co) for v in bm.verts],[tuple(v.index for v in f.verts) for f in bm.faces]);bm.free()
    v,f=templates[key];batch(part).add([tuple(p[j]+loc[j] for j in range(3)) for p in v],f,m)
def beam(part,a,b,r,m,n=8):
    d=Vector(b)-Vector(a);q=d.to_track_quat('Z','Y');mid=(Vector(a)+Vector(b))/2
    v=[tuple(mid+q@Vector((r*math.cos(i*2*PI/n),r*math.sin(i*2*PI/n),z))) for z in (-d.length/2,d.length/2) for i in range(n)]
    f=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    batch(part).add(v,f,m,[False,False]+[True]*n)
def line(part,points,r,m,n=8):
    for a,b in zip(points,points[1:]):beam(part,a,b,r,m,n)
def circle(part,center,r,rod,m,axis='Z',steps=64,start=0,end=2*PI):
    pts=[]
    for i in range(steps+1):
        t=start+(end-start)*i/steps;a=r*math.cos(t);b=r*math.sin(t)
        p=(a,b,0) if axis=='Z' else (a,0,b)
        pts.append(tuple(center[j]+p[j] for j in range(3)))
    line(part,pts,rod,m)
def graphic(part,pts,m):batch(part).add(pts,[tuple(range(len(pts)))],m)
def marking(part,points,m,width=.0508,z=.016):
    v=[];f=[]
    for a,b in zip(points,points[1:]):
        d=Vector((b[0]-a[0],b[1]-a[1]));d.normalize();n=Vector((-d.y,d.x))*width/2;k=len(v)
        v += [(a[0]+n.x,a[1]+n.y,z),(a[0]-n.x,a[1]-n.y,z),(b[0]-n.x,b[1]-n.y,z),(b[0]+n.x,b[1]+n.y,z)];f.append((k,k+1,k+2,k+3))
    batch(part).add(v,f,m)
def ring(part,cx,cy,r,m,start=0,end=2*PI,width=.0508,z=.016):
    marking(part,[(cx+r*math.cos(start+(end-start)*i/128),cy+r*math.sin(start+(end-start)*i/128)) for i in range(129)],m,width,z)
def text(body,loc,size,m,part='Lettering',flat=False):
    curve=bpy.data.curves.new('Lettering','FONT');curve.body=body;curve.align_x='CENTER';curve.align_y='CENTER';curve.size=size;curve.resolution_u=3
    o=bpy.data.objects.new('temp text',curve);scene.collection.objects.link(o);o.location=loc;o.rotation_euler=(0,0,0) if flat else (PI/2,0,0)
    bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.convert(target='MESH')
    o=bpy.context.object;v=[tuple(o.matrix_world@v.co) for v in o.data.vertices];f=[tuple(p.vertices) for p in o.data.polygons]
    batch(part).add(v,f,m);bpy.data.objects.remove(o,do_unlink=True)
def shell(part,loc,dim,m,axis):
    axes=[i for i in range(3) if i!=axis];a=dim[axes[0]]/2;b=dim[axes[1]]/2;r=min(.09,a*.5,b*.5)
    outline=[(-a+r,-b),(a-r,-b),(a,-b+r),(a,b-r),(a-r,b),(-a+r,b),(-a,b-r),(-a,-b+r)]
    v=[]
    for depth in (-dim[axis]/2,dim[axis]/2):
        for x,y in outline:
            p=list(loc);p[axis]+=depth;p[axes[0]]+=x;p[axes[1]]+=y;v.append(tuple(p))
    batch(part).add(v,[tuple(range(7,-1,-1)),tuple(range(8,16))]+[(i,(i+1)%8,(i+1)%8+8,i+8) for i in range(8)],m)

def round_panel(part,loc,dim,r,m,axis=1):
    axes=[i for i in range(3) if i!=axis];a=dim[axes[0]]/2;b=dim[axes[1]]/2;r=min(r,a,b)
    outline=[]
    for cx,cy,start in ((a-r,b-r,0),(-a+r,b-r,PI/2),(-a+r,-b+r,PI),(a-r,-b+r,3*PI/2)):
        outline += [(cx+r*math.cos(start+i*PI/12),cy+r*math.sin(start+i*PI/12)) for i in range(7)]
    n=len(outline);v=[]
    for depth in (-dim[axis]/2,dim[axis]/2):
        for x,y in outline:
            p=list(loc);p[axis]+=depth;p[axes[0]]+=x;p[axes[1]]+=y;v.append(tuple(p))
    batch(part).add(v,[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)],m,[False,False]+[True]*n)

# Rounded rectangular bowl: a compact indoor venue, comfortable sideline runoff.
X,Y,R=12.5,19.5,6.0
segments=[]
def straight(a,b):segments.append(('line',a,b,(Vector(b)-Vector(a)).length))
def corner(c,t):segments.append(('arc',c,t,R*PI/2))
straight((X,-Y+R),(X,Y-R));corner((X-R,Y-R),0)
straight((X-R,Y),(-X+R,Y));corner((-X+R,Y-R),PI/2)
straight((-X,Y-R),(-X,-Y+R));corner((-X+R,-Y+R),PI)
straight((-X+R,-Y),(X-R,-Y));corner((X-R,-Y+R),3*PI/2)
PERIM=sum(s[3] for s in segments);BAYS=24;BAY=PERIM/BAYS
def path(s,depth,z):
    s%=PERIM
    for kind,a,b,length in segments:
        if s<=length:
            t=s/length
            if kind=='line':
                tx=(b[0]-a[0])/length;ty=(b[1]-a[1])/length
                return (a[0]+(b[0]-a[0])*t+ty*depth,a[1]+(b[1]-a[1])*t-tx*depth,z)
            t=b+t*PI/2;return(a[0]+(R+depth)*math.cos(t),a[1]+(R+depth)*math.sin(t),z)
        s-=length
def rigid_bay(mid):
    p=path(mid,0,0);q=path(mid+.01,0,0);tx=(q[0]-p[0])/.01;ty=(q[1]-p[1])/.01
    return lambda v:(p[0]+tx*v[0]+ty*v[1],p[1]+ty*v[0]-tx*v[1],v[2])
def patch(part,s0,s1,d0,d1,z0,z1,m,steps=8):
    v=[path(s0+(s1-s0)*i/steps,d,z) for z in (z0,z1) for d in (d0,d1) for i in range(steps+1)]
    n=steps+1;f=[]
    for i in range(steps):f += [(i,i+1,n+i+1,n+i),(2*n+i,3*n+i,3*n+i+1,2*n+i+1),(i,2*n+i,2*n+i+1,i+1),(n+i,n+i+1,3*n+i+1,3*n+i)]
    f += [(0,n,3*n,2*n),(steps,2*n+steps,3*n+steps,n+steps)]
    batch(part).add(v,f,m)
def arch(part,width,depth,base,spring,top,thick,m,trim):
    radius=width/2;n=20;v=[]
    for y in (depth-thick/2,depth+thick/2):
        for i in range(n+1):
            t=PI-i*PI/n;x=radius*math.cos(t);z=spring+radius*math.sin(t);v.extend([(x,y,z),(x,y,top)])
    k=(n+1)*2;f=[]
    for i in range(n):
        a=i*2;b=a+2;f += [(a,b,b+1,a+1),(k+a+1,k+b+1,k+b,k+a),(a,k+a,k+b,b),(a+1,b+1,k+b+1,k+a+1)]
    f += [(0,1,k+1,k),(2*n,k+2*n,k+2*n+1,2*n+1)];batch(part).add(v,f,m)
    for s in (-1,1):box(part,(s*(radius+.19),depth,(base+spring)/2),(.38,thick,spring-base),m,.06)
    circle(part,(0,depth-thick/2-.05,spring),radius+.035,.08,trim,'Y',32,0,PI)
    for s in (-1,1):beam(part,(s*(radius+.035),depth-thick/2-.05,base),(s*(radius+.035),depth-thick/2-.05,spring),.08,trim)

module('Court')
ivory=mat('Lines','White',.36);edge=mat('Apron','Teal',.42);trim=mat('EdgeTrim','DeepTeal');woodmat=mat('Maple','FFFFFF',.3,'wood');key=mat('KeyPaint','Teal',.32)
# The foundation ends at the apron underside (-.11), never at its visible top.
# Coplanar top faces at z=0 caused teal/navy z-fighting during camera rotation.
box('Foundation',(0,0,-.34),(24,37.5,.46),trim,.20)
box('Apron',(0,0,-.055),(23.6,37.1,.11),edge,.05)
graphic('Maple',[(-7.62,-14.3256,.003),(7.62,-14.3256,.003),(7.62,14.3256,.003),(-7.62,14.3256,.003)],woodmat)
marking('Markings',[(-7.62,-14.3256),(7.62,-14.3256),(7.62,14.3256),(-7.62,14.3256),(-7.62,-14.3256)],ivory)
marking('Markings',[(-7.62,0),(7.62,0)],ivory);ring('Markings',0,0,1.8288,ivory)
for s in (-1,1):
    baseline=s*14.3256;rim=s*12.7254;free=s*8.5344
    pts=[(-2.4384,min(baseline,free),.009),(2.4384,min(baseline,free),.009),(2.4384,max(baseline,free),.009),(-2.4384,max(baseline,free),.009)]
    graphic('KeyGraphics',pts,key)
    marking('Markings',[(-2.4384,baseline),(-2.4384,free),(2.4384,free),(2.4384,baseline)],ivory)
    ring('Markings',0,free,1.8288,ivory,start=0 if s<0 else PI,end=PI if s<0 else 2*PI)
    for i in range(12):
        start=(PI if s<0 else 0)+i*PI/12;ring('Markings',0,free,1.8288,ivory,start,start+PI/24)
    radius=7.239;corner=6.7056;a=math.acos(corner/radius)
    ring('Markings',0,rim,radius,ivory,a if s<0 else PI+a,PI-a if s<0 else 2*PI-a)
    for x in (-corner,corner):marking('Markings',[(x,baseline),(x,rim-s*math.sqrt(radius**2-corner**2))],ivory)
    ring('Markings',0,rim,1.2192,ivory,0 if s<0 else PI,PI if s<0 else 2*PI)
    for x in (-2.4384,2.4384):
        for distance in (2.13,3.05,3.96,4.87):marking('Markings',[(x,baseline-s*distance),(x+(.25 if x>0 else -.25),baseline-s*distance)],ivory)
    # Apron wordmarks remain separate and replaceable.
    text('R A L L Y   C O U R T',(0,s*16.9,.019),.55,ivory,'ApronLettering',True)
    marking('Markings',[(-10.8,s*17.9),(10.8,s*17.9)],ivory,.045)

module('CenterEmblem');teal=mat('Disk','Teal',.35);gold=mat('Sunburst','Gold',.4);white=mat('Outline','White',.4)
graphic('EmblemDisk',[(1.77*math.cos(i*2*PI/96),1.77*math.sin(i*2*PI/96),.012) for i in range(96)],teal)
ring('EmblemOutline',0,0,1.76,white,width=.06,z=.018)
# Original rising-ball mark: open solar arc and three ascending bars.
ring('EmblemGraphics',0,.12,.80,gold,0,PI,.19,.021)
for x,h in ((-.62,.36),(0,.64),(.62,.93)):
    graphic('EmblemGraphics',[(x-.14,-.9,.021),(x+.14,-.9,.021),(x+.14,-.9+h,.021),(x-.14,-.9+h,.021)],gold)

module('Hoop');teal=mat('Padding','Teal',.48);aqua=mat('Piping','Aqua',.42);ivory=mat('BoardShell','Ivory',.35);glass=mat('BoardInset','Glass',.25);coral=mat('Target','Coral',.4);orange=mat('Rim','Orange',.3,metal=.3);dark=mat('Mechanism','Navy',.5,metal=.2);net=mat('Net','White',.8,'fabric');gold=mat('Badge','Gold',.4);screen=mat('Screen','Dark',.5,tex=None);led=mat('ClockDigits','Gold',.4,tex=None,emission=.7)
box('Base',(0,3.08,.20),(1.55,1.95,.40),dark,.13)
box('Padding',(0,3.08,.39),(1.62,1.98,.52),teal,.16)
for x in (-.65,.65):
    for y in (2.45,3.72):beam('Wheels',(x-.09,y,.16),(x+.09,y,.16),.13,dark,12)
# Sloped padded tower, out of the playable court.
v=[(-.67,2.25,.56),(.67,2.25,.56),(.67,3.72,.56),(-.67,3.72,.56),(-.40,2.73,2.45),(.40,2.73,2.45),(.40,3.48,2.45),(-.40,3.48,2.45)]
batch('Padding').add(v,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],teal)
for x in (-.41,.41):beam('Piping',(x*1.5,2.25,.62),(x,2.73,2.40),.035,aqua)
for x in (-.28,.28):
    beam('Support',(x,3.1,2.20),(x,.55,3.66),.105,dark,12)
    beam('Support',(x,3.12,2.9),(x,.55,3.66),.075,aqua,12)
round_panel('BoardShell',(0,.44,3.5052),(1.8288,.16,1.0668),.23,ivory)
round_panel('BoardInset',(0,.348,3.51),(1.55,.035,.79),.14,glass)
for x in (-.3048,.3048):box('Target',(x,.319,3.35),(.045,.032,.47),coral,.014)
for z in (3.115,3.585):box('Target',(0,.319,z),(.6546,.032,.045),coral,.014)
box('BoardBumper',(0,.34,2.977),(1.6,.25,.105),coral,.044)
box('RimMount',(0,.287,3.035),(.19,.2,.11),orange,.025)
circle('Rim',(0,0,3.048),.2476,.019,orange,steps=64)
for level in range(4):
    z0=3.024-level*.115;z1=z0-.115;r0=.23-level*.023;r1=.23-(level+1)*.023
    for i in range(16):
        t=i*2*PI/16+(level%2)*PI/16
        for sign in (-1,1):
            u=t+sign*PI/16;beam('Net',(r0*math.cos(t),r0*math.sin(t),z0),(r1*math.cos(u),r1*math.sin(u),z1),.0055,net,4)
circle('Net',(0,0,2.564),.138,.0055,net,steps=32)
mapper=lambda p:(p[0],p[1]+2.25+(p[2]-.56)*.48/1.89-.047,p[2])
circle('Badge',(0,0,1.22),.245,.035,gold,'Y',32,0,PI)
for x in (-.15,0,.15):beam('Badge',(x,0,.92),(x,0,1.05+(x+.15)*.6),.029,gold)
mapper=lambda p:p
beam('ClockMount',(0,.49,3.95),(0,.49,4.4),.055,dark)
box('ShotClock',(0,.43,4.38),(.71,.24,.46),dark,.075)
box('ShotClock',(0,.29,4.38),(.60,.035,.34),screen,.013)
text('24',(0,.264,4.38),.31,led)

module('Stadium');navy=mat('Shell','Navy',.72);blue=mat('WallPanels','Blue',.65);violet=mat('Balcony','Violet',.58);ivory=mat('ArchTrim','Ivory',.65);stone=mat('Terraces','Concrete',.75);teal=mat('Columns','DeepTeal',.55);gold=mat('Bands','Gold',.55)
for bay in range(BAYS):
    s0=bay*BAY;s1=(bay+1)*BAY;mid=(s0+s1)/2;mapper=lambda p:p;part=f'Bay_{bay:02d}'
    patch(part,s0,s1,-.5,9,-.45,-.02,navy)
    # Front rows, a walkable mid-level concourse, then an upper gallery.
    for row in range(5):patch(part,s0+.48,s1-.48,.20+row*.79,.20+(row+1)*.79,-.01,.55+row*.42,stone)
    patch(part,s0,s1,4.15,5.5,2.0,2.25,stone)
    patch(part,s0,s1,2.8,9,4.15,4.42,violet)
    patch(part,s0,s1,2.75,2.96,4.15,4.85,violet)
    patch(part,s0,s1,2.67,2.98,4.81,4.92,ivory)
    for row in range(4):patch(part,s0+.48,s1-.48,3.30+row*.82,3.30+(row+1)*.82,4.4,4.65+row*.44,stone)
    patch(part,s0,s1,6.58,8.95,5.85,6.10,stone)
    patch(part,s0,s1,8.7,9.0,0,14.0,navy)
    patch(part,s0+.09,s1-.09,8.4,8.72,10.1,13.65,blue)
    patch(part,s0,s1,8.16,8.6,9.92,10.14,violet)
    patch(part,s0,s1,8.14,8.65,13.8,14.10,gold)
    mapper=lambda p,m=mid:path(m+p[0],p[1],p[2])
    arch(part,2.30,7.78,6.1,7.75,10.05,.58,blue,ivory)
    # Mid-concourse openings read between the two seating decks.
    arch(part,1.70,5.03,2.25,3.0,4.30,.42,teal,blue)
    for side in (-1,1):
        x=side*(BAY/2-.19)
        beam(part,(x,7.77,6.1),(x,7.77,10.2),.20,teal,12)
        beam(part,(x,7.77,9.15),(x,7.77,9.40),.225,gold,12)
    # Shared 0.96m aisle at each bay boundary, 0.21/0.22m risers.
    for step in range(10):box(part,(-BAY/2,.20+(step+.5)*.395,.21+step*.21),(.96,.395,.42),stone)
    for step in range(8):box(part,(-BAY/2,3.3+(step+.5)*.41,4.43+step*.22),(.96,.41,.44),stone)

seat_count=0
for upper in (False,True):
    module('UpperSeating' if upper else 'LowerSeating')
    palette=[mat('Seats_'+c,c,.43) for c in ('Aqua','Coral','Lavender','Gold','Lime','Teal')];support=mat('SeatMounts','DeepTeal',.66)
    for bay in range(BAYS):
        mapper=lambda p,m=(bay+.5)*BAY:path(m+p[0],p[1],p[2]);near=f'Bay_{bay:02d}_Seats_LOD0';far=f'Bay_{bay:02d}_Seats_LOD1'
        for row in range(4 if upper else 5):
            d=(3.65 if upper else .52)+row*(.82 if upper else .79);z=(4.65 if upper else .55)+row*(.44 if upper else .42)
            for col in range(6):
                x=(col-2.5)*.57;c=palette[((bay//2)+(2 if upper else 0))%6];seat_count+=1
                round_panel(near,(x,d,z+.24),(.49,.47,.13),.10,c,2)
                round_panel(near,(x,d+.20,z+.51),(.49,.12,.55),.11,c,1)
                box(near,(x,d,z+.10),(.16,.22,.20),support)
                box(far,(x,d+.08,z+.37),(.48,.39,.55),c)

module('Railings');rail=mat('Enamel','Aqua',.42,metal=.15);dark=mat('Uprights','DeepTeal',.5,metal=.1)
for bay in range(BAYS):
    mapper=lambda p,m=(bay+.5)*BAY:path(m+p[0],p[1],p[2]);part=f'Bay_{bay:02d}'
    for d,z in ((2.77,5.27),(6.78,6.86)):
        line(part,[(-BAY/2+i*BAY/8,d,z) for i in range(9)],.042,rail)
        line(part,[(-BAY/2+i*BAY/8,d,z-.4) for i in range(9)],.025,dark)
        for i in range(9):beam(part,(-BAY/2+i*BAY/8,d,z-.74),(-BAY/2+i*BAY/8,d,z),.025,dark)
    for d0,d1,z0,z1 in ((.45,3.7,1.6,3.32),(3.6,6.25,5.7,7.1)):
        beam(part,(-BAY/2,d0,z0),(-BAY/2,d1,z1),.037,rail)
        for f in (0,.5,1):
            d=d0+(d1-d0)*f;z=z0+(z1-z0)*f;beam(part,(-BAY/2,d,z-.82),(-BAY/2,d,z),.027,dark)

module('PerimeterPads');padcolors=[mat('Cushion_'+c,c,.58) for c in ('Teal','Coral','Lavender','Gold')];seam=mat('Seams','Ivory',.55)
for bay in range(BAYS):
    mapper=lambda p,m=(bay+.5)*BAY:path(m+p[0],p[1],p[2])
    for i in (-1,0,1):box(f'Bay_{bay:02d}',(i*1.05,-.32,.43),(1.01,.36,.86),padcolors[(bay//3+(i==0))%4],.105)
    # Gaps align to spectator aisles and preserve module boundaries.

module('Banners');cloths=[mat(c,c,.86,'fabric') for c in ('Teal','Coral','Lavender')];gold=mat('Print','Gold',.8,'fabric');cream=mat('LightPrint','Ivory',.8,'fabric');pole=mat('Rods','Gold',.5,metal=.25)
for bay in range(0,BAYS,2):
    mapper=rigid_bay((bay+.5)*BAY);c=cloths[(bay//2)%3];part=f'Banner_{bay:02d}'
    beam(part,(-.97,7.5,13.47),(.97,7.5,13.47),.04,pole)
    # Slight cloth bow, closed thin sheet for correct backface appearance.
    pts=[(-.84,7.43,13.4),(.84,7.43,13.4),(.84,7.30,10.62),(0,7.25,10.37),(-.84,7.30,10.62)]
    batch(part+'Cloth').add(pts+[(x,y+.028,z) for x,y,z in pts],[(4,3,2,1,0),(5,6,7,8,9)]+[(i,(i+1)%5,(i+1)%5+5,i+5) for i in range(5)],c)
    # Rising-ball brand motif, instead of copying the concept's lightning emblem.
    circle(part,(0,7.21,12.23),.46,.038,gold,'Y',36)
    beam(part,(-.46,7.21,12.23),(.46,7.21,12.23),.025,gold)
    beam(part,(0,7.21,11.77),(0,7.21,12.69),.025,gold)
    graphic(part+'Graphics',[(-.84,7.18,10.62),(.84,7.18,11.38),(.84,7.18,11.72),(-.84,7.18,10.96)],gold)
    graphic(part+'Graphics',[(-.84,7.17,11.03),(.84,7.17,11.79),(.84,7.17,11.95),(-.84,7.17,11.19)],cream)

module('LightingRig');steel=mat('Truss','Blue',.47,metal=.35);housing=mat('Housing','Navy',.48,metal=.2);gold=mat('Bezels','Gold',.42,metal=.3);lamp=mat('Lenses','White',.3,tex=None,emission=4)
# Scoreboard support bridge spans the open oculus and lands on the roof ring.
for y in (-1.3,1.3):
    for z in (14.8,15.3):beam('ScoreboardBridge',(-8,y,z),(8,y,z),.065,steel)
    for x in range(-8,8):beam('ScoreboardBridge',(x,y,14.8),(x+1,y,15.3),.034,steel)
for bay in range(BAYS):
    mapper=lambda p,m=(bay+.5)*BAY:path(m+p[0],p[1],p[2]);part=f'Bay_{bay:02d}'
    for d,z in ((.1,13.2),(.7,13.2),(.4,13.8)):line(part,[(-BAY/2+i*BAY/6,d,z) for i in range(7)],.055,steel)
    for j in range(4):
        a=-BAY/2+j*BAY/4;b=a+BAY/4;beam(part,(a,.1,13.2),(b,.4,13.8),.032,steel);beam(part,(a,.7,13.2),(b,.4,13.8),.032,steel)
    for x in (-.67,.67):
        beam(part,(x,.40,13.2),(x,.40,12.88),.045,steel)
        beam(part,(x,.4,12.86),(x,.24,12.54),.22,housing,16)
        beam(part,(x,.235,12.53),(x,.20,12.46),.225,gold,16)
        beam(part,(x,.196,12.45),(x,.182,12.42),.183,lamp,16)
    # Balcony portholes and warm concourse sconces.
    beam(part,(0,2.67,4.48),(0,2.53,4.48),.175,gold,16)
    beam(part,(0,2.52,4.48),(0,2.49,4.48),.135,lamp,16)
    for x in (-1.6,1.6):
        box(part,(x,7.32,8.25),(.28,.22,.65),housing,.09)
        box(part,(x,7.17,8.28),(.15,.055,.36),lamp,.025)

module('Ceiling');navy=mat('AcousticPanels','Navy',.9);blue=mat('PanelRibs','Blue',.65);warm=mat('InsetPanels','Gold',.78);ivory=mat('SkylightFrame','Ivory',.58)
for bay in range(BAYS):
    mapper=lambda p:p;s0=bay*BAY;s1=(bay+1)*BAY
    # Open central oculus: lights sit below the roof and remain readable from court.
    patch(f'Panel_{bay:02d}',s0+.025,s1-.025,-5,9.15,14.25,14.5,navy)
    patch(f'Panel_{bay:02d}',s0+.08,s1-.08,-4.85,-4.60,14.10,14.30,ivory)
    patch(f'Panel_{bay:02d}',s0+.30,s1-.30,-4.35,-1.2,14.15,14.26,warm)
    mapper=lambda p,m=(bay+.5)*BAY:path(m+p[0],p[1],p[2])
    beam(f'Panel_{bay:02d}',(-BAY/2,-4.9,14.1),(-BAY/2,8.9,14.1),.06,blue)

module('Scoreboard');dark=mat('Display','Dark',.4,tex=None);teal=mat('Casing','DeepTeal',.5);cream=mat('Edge','Ivory',.42);coral=mat('Accent','Coral',.48);gold=mat('Digits','Gold',.4,tex=None,emission=.6);white=mat('Labels','White',.5,tex=None,emission=.25)
box('Body',(0,0,10.2),(3.9,3.9,2.2),teal,.22)
box('Crown',(0,0,11.38),(4.12,4.12,.22),cream,.10)
box('Base',(0,0,9.02),(4.12,4.12,.22),coral,.10)
for face in range(4):
    a=face*PI/2;mapper=lambda p,a=a:(p[0]*math.cos(a)-p[1]*math.sin(a),p[0]*math.sin(a)+p[1]*math.cos(a),p[2])
    box('Displays',(0,-1.977,10.20),(3.54,.08,1.75),dark,.035)
    text('HOME     AWAY',(0,-2.027,10.77),.19,white)
    text('00 : 00',(0,-2.030,10.17),.62,gold)
    text('R A L L Y  /  C O U R T',(0,-2.028,9.54),.14,white)
    beam('Suspension',(-1.3,1.3,11.4),(-1.3,1.3,15),.03,teal)
mapper=lambda p:p

module('CourtsideFurniture');teal=mat('Upholstery','Teal',.52);coral=mat('VisitorUpholstery','Coral',.52);ivory=mat('Frames','Ivory',.52);navy=mat('Equipment','Navy',.56);glass=mat('Screens','Glass',.4,tex=None);gold=mat('Accent','Gold',.5)
for side in (-1,1):
    # Portable team chairs face across the court. Two groups leave the scorer's lane clear.
    mapper=lambda p,s=side:(s*(10.1+p[1]),-s*p[0],p[2])
    for y in (-7,7):
        for j in range(5):
            x=y+(j-2)*.68;c=teal if side<0 else coral
            shell('TeamSeats',(x,.10,.54),(.56,.55,.14),c,2);shell('TeamSeats',(x,.33,.92),(.56,.14,.7),c,1)
            for dx in (-.19,.19):
                beam('ChairFrames',(x+dx,-.09,.05),(x+dx,.23,.55),.026,ivory)
                beam('ChairFrames',(x+dx,.35,.05),(x+dx,-.10,.54),.026,ivory)
    box('Cooler',(3.65,.05,.42),(.62,.58,.84),navy,.07)
    box('Cooler',(3.65,.05,.86),(.66,.63,.12),ivory,.04)
    for j in range(3):beam('Bottles',(3.45+j*.18,.05,.93),(3.45+j*.18,.05,1.18),.057,gold,10)
mapper=lambda p:(-10.1-p[1],p[0],p[2])
box('ScorerTable',(0,0,.64),(4.2,1.05,1.28),navy,.12)
box('TableTop',(0,0,1.3),(4.42,1.18,.12),ivory,.05)
box('TableFront',(0,-.546,.67),(3.95,.06,.91),teal,.027)
text('R A L L Y',(0,-.585,.72),.34,ivory)
for x in (-1.3,0,1.3):
    box('MonitorStand',(x,.12,1.43),(.24,.23,.22),navy,.03)
    box('Monitors',(x,.13,1.66),(.56,.12,.36),navy,.047)
    box('MonitorScreens',(x,.058,1.66),(.47,.022,.27),glass,.009)

# Build compact meshes, export reusable origins, then assemble hoop instances.
objects=[b.finish() for b in batches.values()]
report={'name':'Rally Court','source':'Arena_Rally.blend','units':'metres','court_metres':[15.24,28.6512],
 'rim_height':3.048,'rim_inner_diameter':.4572,'seats':seat_count,'modules':{},'materials':material_specs,
 'coordinate_system':'Blender X width, Y length, Z up; FBX -Z forward, Y up. Unity maps (x,y,z) to (-x,z,-y).',
 'placements':{'Hoop_North':{'module':'Hoop','blender_position':[0,12.7254,0],'rotation_z_degrees':0},'Hoop_South':{'module':'Hoop','blender_position':[0,-12.7254,0],'rotation_z_degrees':180}},
 'notes':['Review cameras/lights are not exported.','Court top is z=0; markings are layered 3–21 mm above.','Seat LODs must not be rendered simultaneously.','Scoreboard digits are decorative replaceable meshes, not game logic.','Textures are original deterministic authored maps; reference image is not projected onto geometry.','Android performance requires device profiling.']}
for name,(c,root) in modules.items():
    children=[o for o in objects if o.parent==root]
    for o in children:o.data.calc_loop_triangles()
    report['modules'][name]={'file':name+'.fbx','pivot':[0,0,0], 'pivot_description':'Rim centre projected onto floor; hoop faces -Y' if name=='Hoop' else 'Court centre at floor height',
       'meshes':len(children),'triangles_lod0':sum(len(o.data.loop_triangles) for o in children if not o.name.endswith('LOD1')),
       'triangles_lod1':sum(len(o.data.loop_triangles) for o in children if not o.name.endswith('LOD0'))}
    bpy.ops.object.select_all(action='DESELECT');root.select_set(True)
    for o in children:o.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(EXPORT/(name+'.fbx')),use_selection=True,object_types={'EMPTY','MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='RELATIVE',use_mesh_modifiers=True)
    for o in children:
        if o.name.endswith('LOD1'):o.hide_render=True;o.hide_set(True)
root=modules['Hoop'][1];root.location=(0,12.7254,0);root.name='Hoop_North'
south=bpy.data.objects.new('Hoop_South',None);modules['Hoop'][0].objects.link(south);south.location=(0,-12.7254,0);south.rotation_euler.z=PI
for o in [o for o in objects if o.parent==root]:
    copy=o.copy();copy.data=o.data;modules['Hoop'][0].objects.link(copy);copy.parent=south

review=bpy.data.collections.new('REVIEW_ONLY');scene.collection.children.link(review)
def to_review(o):
    for c in list(o.users_collection):c.objects.unlink(o)
    review.objects.link(o)
def light(name,loc,target,energy,color,size):
    bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.name=name;to_review(o)
    o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler();o.data.energy=energy;o.data.color=color;o.data.shape='DISK';o.data.size=size
world=bpy.data.worlds.new('Rally soft environment');scene.world=world;world.use_nodes=True
world.node_tree.nodes['Background'].inputs[0].default_value=(.27,.39,.55,1);world.node_tree.nodes['Background'].inputs[1].default_value=.4
light('Oculus daylight',(0,0,18),(0,0,0),5500,(.78,.89,1),13)
for s in (-1,1):
    for y in (-11,11):light('Warm court wash',(s*8,y,12),(0,y*.55,0),1500,(1,.80,.58),7)
for bay in range(0,BAYS,2):
    p=path((bay+.5)*BAY,3.5,11.2);target=path((bay+.5)*BAY,4.0,3)
    light('Gallery warm fill',p,target,320,(1,.77,.50),3.5)
scene.render.engine='CYCLES';scene.cycles.samples=48;scene.cycles.use_denoising=True
try:
    prefs=bpy.context.preferences.addons['cycles'].preferences;prefs.compute_device_type='OPTIX';prefs.get_devices()
    for d in prefs.devices:d.use=d.type=='OPTIX'
    scene.cycles.device='GPU'
except Exception:pass
scene.render.resolution_x=1600;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX';scene.view_settings.look='AgX - Medium High Contrast';scene.view_settings.exposure=-.15
cameras=[]
for name,loc,look,lens in [
 ('01_Court_Hero',(8,-13,3.3),(-1,12,5.0),22),
 ('02_Bowl_Overview',(16,-23,12),(0,1,2.8),20),
 ('03_Hoop_Detail',(3.6,7.2,2.65),(0,14.0,2.2),34),
 ('04_Gallery_Detail',(7,1,4.5),(17,8,7.5),28)]:
    bpy.ops.object.camera_add(location=loc);cam=bpy.context.object;cam.name=name;to_review(cam);cam.data.lens=lens;cam.data.clip_end=250
    cam.rotation_euler=(Vector(look)-cam.location).to_track_quat('-Z','Y').to_euler();cameras.append(cam)
scene.camera=cameras[0]
scene['Design']='RALLY / honey maple, teal cushions, coral and lavender terraces, ivory arch trim, rising-ball graphics'
scene['Modules']='13 independent FBX modules; each role has its own recolorable material. Hoop is instantiated twice.'
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':area.spaces.active.region_3d.view_perspective='CAMERA'
bpy.ops.object.select_all(action='DESELECT')
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'Arena_Rally.blend'))
(SOURCE/'arena-manifest.json').write_text(json.dumps(report,indent=2)+'\n')
# A copy beside the FBX files allows a self-contained Unity import.
(EXPORT/'arena-manifest.json').write_text(json.dumps(report,indent=2)+'\n')
(EXPORT/'unity-import.json').write_text(json.dumps({'materials':[dict(name=n,**s) for n,s in material_specs.items()], 'modules':list(modules)},indent=2)+'\n')
print('RALLY_EXPORT_COMPLETE',len(modules),'modules',seat_count,'seats',flush=True)
if '--render' in sys.argv:
    for cam in cameras:
        scene.camera=cam;scene.render.filepath=str(REVIEW/(cam.name+'.png'));bpy.ops.render.render(write_still=True)
    print('RALLY_RENDER_COMPLETE',flush=True)
