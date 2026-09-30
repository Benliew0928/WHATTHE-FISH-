"""Reusable G2/L2 assets. Metres, XY plan/Z up, UVs in metres/4.

No import-time scene mutation. Approved stadium assets are read-only references.
"""
import bpy, bmesh, math, json, random
import numpy as np
from pathlib import Path
from mathutils import Vector

PI=math.pi
ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/'Game/Assets/_Game/Art/RefinedIslands'
SOURCE=ROOT/'ArtSource/RefinedIslands'

def rgb(h):return np.array([int(h[i:i+2],16)/255 for i in (0,2,4)],np.float32)
def smooth(a,b,x):
    t=np.clip((x-a)/(b-a),0,1);return t*t*(3-2*t)
def point(x,y,z):return dict(x=float(x),y=float(z),z=float(y))
def save_image(name,data,folder,noncolor=False):
    h,w=data.shape[:2];im=bpy.data.images.new(name,width=w,height=h,alpha=True)
    if noncolor:im.colorspace_settings.name='Non-Color'
    rgba=np.ones((h,w,4),np.float32)
    if data.ndim==2:rgba[:,:,:3]=data[:,:,None]
    else:rgba[:,:,:data.shape[2]]=data
    im.pixels.foreach_set(rgba.ravel());im.filepath_raw=str(folder/(name+'.png'));im.file_format='PNG';im.save();im.pack();im.use_fake_user=True
    return im

def make_materials():
    folder=ART/'Shared';folder.mkdir(parents=True,exist_ok=True);SOURCE.mkdir(parents=True,exist_ok=True)
    n=2048;y,x=np.mgrid[:n,:n].astype(np.float32)/n;rng=np.random.default_rng(270926)
    noise=rng.random((n,n),dtype=np.float32)-.5
    broad=np.sin(x*2*PI+.3)*np.cos(y*4*PI)+.3*np.sin(x*12*PI-y*6*PI)
    kinds={'Stone':('D9C9A5',.81),'Paving':('D8C5A1',.83),'Timber':('BA814B',.66),'Bark':('96754E',.89),'Leaf':('4D8F3A',.76),'LeafLight':('78A84B',.74),'Turf':('6F9E40',.88),'Sand':('DDC18E',.94),'Soil':('695133',.97),'Teal':('277F88',.48),'Bronze':('A17B45',.42),'Rope':('D9C9A3',.88),'Flower':('EA929A',.72),'Gold':('EBC75D',.70)}
    mats={};specs=[]
    for kind,(hexcode,rough) in kinds.items():
        h=.5+.06*noise+.025*broad
        if kind in ('Stone','Paving'):
            # Mineral mottling and tiny pores, no giant painted masonry seams.
            h+=.02*np.sin(40*PI*x+np.sin(8*PI*y))*np.cos(38*PI*y)
            h-=.10*(noise<-.48)
        elif kind in ('Timber','Bark'):
            warp=x+.003*np.sin(y*6*PI)+.002*np.cos(y*14*PI)
            grain=np.sin(warp*512*PI)*.035+np.sin(warp*124*PI)*.055
            h+=grain
            if kind=='Bark':h+=.05*np.sin(y*64*PI+np.sin(x*8*PI))
        elif kind in ('Turf','Leaf','LeafLight'):
            h+=.055*np.sin(x*1000*PI+np.sin(y*8*PI))*np.sin(y*250*PI)
        elif kind=='Sand':h+=.007*np.sin(y*48*PI+np.sin(x*6*PI))
        elif kind=='Rope':h+=.09*np.sin((x+y*.35)*180*PI)+.025*np.sin((x-y*.2)*800*PI)
        color=rgb(hexcode)[None,None,:]*(.94+(h-.5)[:,:,None]*.65)
        base=save_image('RI_'+kind+'_BaseColor',np.clip(color,0,1),folder)
        dx=(np.roll(h,-1,1)-np.roll(h,1,1))*2.5;dy=(np.roll(h,-1,0)-np.roll(h,1,0))*2.5
        normal=np.stack((-dx,-dy,np.ones_like(dx)),2);normal/=np.linalg.norm(normal,axis=2)[:,:,None]
        nm=save_image('RI_'+kind+'_Normal',normal*.5+.5,folder,True)
        mask=np.zeros((n,n,4),np.float32);mask[:,:,0]=.60 if kind=='Bronze' else .06 if kind=='Teal' else 0
        mask[:,:,3]=np.clip(1-rough+(h-.5)*.16,0,1)
        sm=save_image('RI_'+kind+'_Mask',mask,folder,True)
        m=bpy.data.materials.new('RI_'+kind);m.use_nodes=True;m.use_fake_user=True;bs=m.node_tree.nodes.get('Principled BSDF')
        a=m.node_tree.nodes.new('ShaderNodeTexImage');a.image=base;m.node_tree.links.new(a.outputs['Color'],bs.inputs['Base Color'])
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=nm;norm=m.node_tree.nodes.new('ShaderNodeNormalMap');norm.inputs['Strength'].default_value=.42
        m.node_tree.links.new(t.outputs['Color'],norm.inputs['Color']);m.node_tree.links.new(norm.outputs['Normal'],bs.inputs['Normal'])
        bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=float(mask[0,0,0]);m.diffuse_color=(*rgb(hexcode),1)
        mats[kind]=m;specs.append(dict(name=m.name,base_map=base.name+'.png',normal=nm.name+'.png',mask=sm.name+'.png',roughness=rough,foliage=kind in ('Leaf','LeafLight','Flower','Gold'),tile_metres=4))
    # Station accent remains a distinct material slot for existing property blocks.
    accent=mats['Teal'].copy();accent.name='LG_Accent';mats['Accent']=accent
    specs.append(dict(specs[next(i for i,s in enumerate(specs) if s['name']=='RI_Teal')],name='LG_Accent'))
    (folder/'materials.json').write_text(json.dumps(dict(materials=specs),indent=2))
    return mats

class Meshes:
    def __init__(self):self.data={};self.cache={}
    def add(self,name,v,f,mat,smoothness=False,uv=None):
        vv,ff,tt,ss=self.data.setdefault((name,mat),([],[],[],[]));offset=len(vv);vv.extend(tuple(p) for p in v)
        for face in f:
            if len(face)<3:continue
            # Beveled n-gons often begin with three collinear rim vertices.
            # Find a real triangle before deciding the whole cap has zero area.
            origin=Vector(v[face[0]]);n=Vector((0,0,0))
            for j in range(1,len(face)-1):
                n=(Vector(v[face[j]])-origin).cross(Vector(v[face[j+1]])-origin)
                if n.length_squared>=1e-15:break
            if n.length_squared<1e-15:continue
            axes=[a for a in range(3) if a!=max(range(3),key=lambda a:abs(n[a]))]
            if mat in ('Timber','Bark'):
                axes.sort(key=lambda axis:max(v[i][axis] for i in face)-min(v[i][axis] for i in face))
            ff.append(tuple(offset+i for i in face));tt.append([uv[i] if uv else tuple(v[i][a]/4 for a in axes) for i in face]);ss.append(smoothness)
    def box(self,name,p,size,mat='Stone',bevel=.025):
        bevel=min(bevel,min(size)*.35);key=(*size,bevel)
        if key not in self.cache:
            bm=bmesh.new();bmesh.ops.create_cube(bm,size=1)
            for v in bm.verts:v.co=Vector(tuple(v.co[i]*size[i] for i in range(3)))
            if bevel:bmesh.ops.bevel(bm,geom=list(bm.edges),offset=bevel,segments=2,affect='EDGES')
            bm.verts.ensure_lookup_table();bm.verts.index_update();self.cache[key]=([tuple(v.co) for v in bm.verts],[tuple(v.index for v in f.verts) for f in bm.faces]);bm.free()
        v,f=self.cache[key];self.add(name,[tuple(p[i]+co[i] for i in range(3)) for co in v],f,mat)
    def beam(self,name,a,b,r,mat='Timber',n=10,r2=None):
        a,b=Vector(a),Vector(b);q=(b-a).to_track_quat('Z','Y');r2=r if r2 is None else r2
        v=[tuple(p+q@Vector((radius*math.cos(i*2*PI/n),radius*math.sin(i*2*PI/n),0))) for p,radius in ((a,r),(b,r2)) for i in range(n)]
        self.add(name,v,[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)],mat,True)
    def curve(self,name,pts,r,mat='Rope',n=8):
        points=[Vector(p) for p in pts];v=[];f=[]
        for j,p in enumerate(points):
            tangent=points[min(j+1,len(points)-1)]-points[max(j-1,0)];q=tangent.to_track_quat('Z','Y')
            v.extend(tuple(p+q@Vector((r*math.cos(i*2*PI/n),r*math.sin(i*2*PI/n),0))) for i in range(n))
        for j in range(len(points)-1):
            for i in range(n):a=j*n+i;b=j*n+(i+1)%n;f.append((a,b,b+n,a+n))
        f.extend([tuple(range(n-1,-1,-1)),tuple((len(points)-1)*n+i for i in range(n))]);self.add(name,v,f,mat,True)
    def rock(self,name,p,scale,seed=0,mat='Stone',sub=3):
        key=('ico',sub)
        if key not in self.cache:
            bm=bmesh.new();bmesh.ops.create_icosphere(bm,subdivisions=sub,radius=1);bm.verts.ensure_lookup_table();bm.verts.index_update()
            self.cache[key]=([tuple(v.co) for v in bm.verts],[tuple(v.index for v in f.verts) for f in bm.faces]);bm.free()
        v,f=self.cache[key]
        verts=[]
        for co in v:
            wave=1+.06*math.sin(co[0]*7+seed)*math.sin(co[1]*5+co[2]*4)+.035*math.cos(co[2]*13+seed)
            verts.append(tuple(p[i]+co[i]*scale[i]*wave for i in range(3)))
        self.add(name,verts,f,mat,True)
    def leaf(self,name,origin,angle,length,width,lift=.4,droop=.1,mat='Leaf',segments=5):
        o=Vector(origin);d=Vector((math.cos(angle),math.sin(angle),0));s=Vector((-d.y,d.x,0));v=[tuple(o)];uv=[(0,.5)]
        for j in range(1,segments):
            t=j/segments;c=o+d*(length*t)+Vector((0,0,lift*math.sin(PI*t)-droop*t*t));w=width*math.sin(PI*t)**.8
            v.extend([tuple(c-s*w),tuple(c+Vector((0,0,.045*math.sin(PI*t)))),tuple(c+s*w)]);uv.extend([(t,0),(t,.5),(t,1)])
        v.append(tuple(o+d*length+Vector((0,0,-droop))));uv.append((1,.5));f=[(0,1,2),(0,2,3)]
        for j in range(segments-2):
            a=1+j*3;b=a+3;f.extend([(a,b,b+1,a+1),(a+1,b+1,b+2,a+2)])
        a=len(v)-4;b=len(v)-1;f.extend([(a,b,a+1),(a+1,b,a+2)]);self.add(name,v,f,mat,True,uv)
    def finish(self,name,mats,collection):
        objects=[]
        for (group,material),(v,f,uvs,sm) in self.data.items():
            mesh=bpy.data.meshes.new(name+'__'+group+'_'+material);mesh.from_pydata(v,[],f);mesh.update();layer=mesh.uv_layers.new(name='MetreUV')
            for poly,uv,s in zip(mesh.polygons,uvs,sm):
                poly.use_smooth=s
                for loop,co in zip(poly.loop_indices,uv):layer.data[loop].uv=co
            # Winding is authored by each primitive. Recalculating disconnected open
            # roof/leaf faces can randomly reverse them and create visible holes.
            mesh.materials.append(mats[material]);o=bpy.data.objects.new(mesh.name,mesh);collection.objects.link(o);objects.append(o)
        return objects

def bolt(m,name,p,axis='Y',r=.045):
    a=list(p);b=list(p);b['XYZ'.index(axis)]+=.028;m.beam(name,a,b,r,'Bronze',8)

def palm(m,variant):
    height=8.2+variant*.55;lean=(.9+variant*.18,.35*math.sin(variant));pts=[]
    for j in range(29):
        t=j/28;pts.append((lean[0]*t*t,lean[1]*t*t,height*t))
    v=[];faces=[];sides=32
    for j,p in enumerate(pts):
        r=.36-.006*j
        v.extend((p[0]+r*math.cos(a),p[1]+r*math.sin(a),p[2]) for a in [i*2*PI/sides for i in range(sides)])
        if j<28:m.beam('Palm',(p[0],p[1],p[2]+.018),(p[0],p[1],p[2]+.06),r*1.045,'Timber',32)
    for j in range(28):
        for i in range(sides):a=j*sides+i;b=j*sides+(i+1)%sides;faces.append((a,b,b+sides,a+sides))
    faces.extend([tuple(range(sides-1,-1,-1)),tuple(28*sides+i for i in range(sides))]);m.add('Palm',v,faces,'Bark',True)
    top=Vector(pts[-1])
    for f in range(14):
        a=f*2*PI/14+variant*.4;length=3.6+(f%3)*.32;d=Vector((math.cos(a),math.sin(a),0));s=Vector((-d.y,d.x,0))
        points=[top+d*(length*t)+Vector((0,0,1.05*math.sin(PI*t)-1.4*t*t)) for t in np.linspace(0,1,13)]
        m.curve('Palm',points,.026,'LeafLight',6)
        for j in range(1,20):
            t=j/20;c=top+d*(length*t)+Vector((0,0,1.05*math.sin(PI*t)-1.4*t*t));span=.95*math.sin(PI*t)**.68+.07
            for side in (-1,1):m.leaf('Palm',c,a+side*1.17,span,.125,lift=.085,droop=.18,mat='Leaf' if (j+f)%3 else 'LeafLight',segments=4)
    for j in range(5):
        a=j*2*PI/5;m.rock('Palm',top+Vector((.35*math.cos(a),.35*math.sin(a),-.3)),(.24,.25,.32),j,'Bark',2)

def plant(m,variant):
    for j in range(14):
        a=j*2.4;m.leaf('Plant',(math.cos(a)*.15,math.sin(a)*.15,0),a,.65+(j%4)*.17,.12+(j%3)*.02,lift=.25+(j%3)*.1,droop=.08,mat='Leaf' if j%3 else 'LeafLight')
    if variant<3:
        for j in range(5):
            a=j*2.4;x=math.cos(a)*.38;y=math.sin(a)*.38;z=.55+(j%3)*.13;m.beam('Plant',(x,y,0),(x,y,z),.018,'Leaf',6)
            for k in range(5):
                a=k*2*PI/5;m.rock('Plant',(x+math.cos(a)*.105,y+math.sin(a)*.105,z),(.13,.105,.035),k,'Gold' if variant==1 else 'Flower',2)
            m.rock('Plant',(x,y,z+.025),(.055,.055,.045),j,'Gold',2)

def limestone(m,variant):
    # Rounded but visibly bedded limestone: modeled strata, undercuts, and planted crowns.
    levels=[(-.4,1.05),(0,1.10),(.22,1.04),(.30,.97),(.9,.94),(1.04,1.0),(1.13,.91),(1.75,.87),(1.87,.93),(1.98,.82),(2.6,.76),(2.72,.81),(2.82,.67),(3.18,.61),(3.32,.49)]
    count=48;v=[];f=[]
    for j,(z,r) in enumerate(levels):
        for i in range(count):
            a=i*2*PI/count;wave=1+.06*math.sin(a*3+variant)+.035*math.sin(a*7+variant*.4)
            v.append((math.cos(a)*r*(2.2+variant*.17)*wave,math.sin(a)*r*1.85*wave,z*(1+variant*.10)+.045*math.sin(a*4+variant)))
    for j in range(len(levels)-1):
        for i in range(count):a=j*count+i;b=j*count+(i+1)%count;f.append((a,b,b+count,a+count))
    f.append(tuple(range(count-1,-1,-1)));f.append(tuple((len(levels)-1)*count+i for i in range(count)));m.add('Rock',v,f,'Stone',True)

def grass(m,variant):
    rng=random.Random(810+variant)
    # An eight-metre patch; foliage placement masks keep this out of paths.
    for j in range(220):
        x=rng.uniform(-3.7,3.7);y=rng.uniform(-3.7,3.7)
        for k in range(5):
            a=rng.random()*2*PI;h=rng.uniform(.09,.26);w=rng.uniform(.015,.027);d=Vector((math.cos(a),math.sin(a),0));s=Vector((-d.y,d.x,0));o=Vector((x,y,0))
            v=[o-s*w,o+s*w,o+d*h*.22+s*w*.5+Vector((0,0,h*.6)),o+d*h*.22-s*w*.5+Vector((0,0,h*.6)),o+d*h*.45+Vector((0,0,h))]
            m.add('Grass',v,[(0,1,2,3),(3,2,4)],'Leaf' if j%3 else 'LeafLight',True)

def deck(m,bridge=False):
    if bridge:
        for j in range(26):m.box('Deck',(-6.25+j*.5,0,-.13),(.48,4.2,.26),'Timber',.018)
        ends=[(-6,-1.85),(-3,-1.85),(0,-1.85),(3,-1.85),(6,-1.85),(-6,1.85),(-3,1.85),(0,1.85),(3,1.85),(6,1.85)]
        for y in (-1.6,1.6):m.box('Deck',(0,y,-.48),(13,.28,.5),'Timber',.02)
        links=[((x,y),(x+3,y)) for y in (-1.85,1.85) for x in (-6,-3,0,3)]
    else:
        for j in range(14):
            m.box('Deck',(0,(j+.5)*.5,-.13),(6,.48,.26),'Timber',.018)
            for x in (-2.45,0,2.45):bolt(m,'Deck',(x,(j+.5)*.5,.005),'Z',.025)
        ends=[(x,y) for x in (-2.8,2.8) for y in (.4,3.5,6.6)]
        for x in (-2.25,2.25):m.box('Deck',(x,3.5,-.46),(.28,7,.44),'Timber')
        links=[((x,a),(x,b)) for x in (-2.8,2.8) for a,b in ((.4,3.5),(3.5,6.6))]
    for x,y in ends:
        m.box('Deck',(x,y,-.75),(.53,.53,1.9),'Stone',.055);m.box('Deck',(x,y,.55),(.34,.34,1.5),'Timber',.035)
        m.box('Deck',(x,y,1.27),(.46,.46,.16),'Teal' if bridge else 'Accent',.04)
        for z in (.19,.92):
            m.box('Deck',(x,y-.18,z),(.35,.045,.18),'Bronze',.016);bolt(m,'Deck',(x,y-.22,z))
        for dz in (0,.07,.14):m.curve('Deck',[(x+.24*math.cos(a),y+.24*math.sin(a),.76+dz) for a in np.linspace(0,2*PI,19)],.035,'Rope',6)
    for a,b in links:
        pts=[(a[0]+(b[0]-a[0])*t,a[1]+(b[1]-a[1])*t,.90-.20*math.sin(PI*t)) for t in np.linspace(0,1,17)]
        m.curve('Deck',pts,.045,'Rope',8)
        # Three fine helices supply actual close-view twisted rope relief.
        for phase in range(3):m.curve('Deck',[(p[0]+.018*math.sin(j*1.8+phase*2*PI/3),p[1]+.018*math.cos(j*1.8+phase*2*PI/3),p[2]+.018*math.sin(j*1.8+phase*2*PI/3)) for j,p in enumerate(pts)],.012,'Rope',5)

def pavilion(m,small=False):
    width=7 if small else 10;depth=4 if small else 7;roof=3.9 if small else 4.8
    m.box('Pavilion',(0,0,-.16),(width+1,depth+1,.32),'Paving',.04)
    for x in (-width/2,width/2):
        for y in (-depth/2,depth/2):
            for j in range(4):m.box('Pavilion',(x,y,j*.23+.115),(.72,.72,.218),'Stone',.035)
            m.box('Pavilion',(x,y,.96),(.86,.86,.16),'Stone',.035)
            m.box('Pavilion',(x,y,(roof+1)/2),(.36,.36,roof-1),'Timber',.035)
            for z in (1.12,roof-.3):m.box('Pavilion',(x,y-.205,z),(.37,.045,.28),'Bronze',.012);bolt(m,'Pavilion',(x,y-.24,z),r=.045)
    for y in (-depth/2,depth/2):
        m.box('Pavilion',(0,y,roof),(width+.6,.26,.32),'Timber',.035)
        for x in (-width/2,width/2):m.beam('Pavilion',(x,y,roof-.9),(x-math.copysign(1,x),y,roof),.11,'Timber',8)
    # Curved segmented teal canopy with a timber underside and actual thickness.
    for i in range(24):
        x0=-width/2-.65+(width+1.3)*i/24;x1=-width/2-.65+(width+1.3)*(i+1)/24
        h0=roof+.2+.85*math.cos(x0/(width/2+.65)*PI/2);h1=roof+.2+.85*math.cos(x1/(width/2+.65)*PI/2)
        v=[(x0,-depth/2-.7,h0),(x1,-depth/2-.7,h1),(x1,depth/2+.7,h1),(x0,depth/2+.7,h0)]
        m.add('Pavilion',v,[(0,1,2,3)],'Teal');m.add('Pavilion',[(x,y,z-.14) for x,y,z in v],[(3,2,1,0)],'Timber')
        for y in (-depth/2-.7,depth/2+.7):m.beam('Pavilion',(x0,y,h0-.04),(x1,y,h1-.04),.10,'Teal')
        if i%3==0:m.beam('Pavilion',(x0,-depth/2-.7,h0+.018),(x0,depth/2+.7,h0+.018),.018,'Teal',6)
    for x in (-width*.3,width*.3):
        m.box('Pavilion',(x,depth*.30,.48),(width*.32,.48,.12),'Timber',.03)
        m.box('Pavilion',(x,depth*.30+.23,.84),(width*.32,.12,.5),'Timber',.025)
        for xx in (x-width*.12,x+width*.12):m.box('Pavilion',(xx,depth*.30,.22),(.12,.35,.44),'Bronze',.02)
    if small:
        for j in range(2):
            m.box('Pavilion',(-width*.36+j*.8,depth*.05,.28),(.7,.55,.56),'Timber',.03)
            for z in (.10,.43):m.box('Pavilion',(-width*.36+j*.8,depth*.05-.285,z),(.70,.04,.08),'Bronze',.01)

def export_asset(name,m,mats,lods=True,boxes=None,collision='none'):
    col=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(col);objects=m.finish('LOD0',mats,col)
    all_objects=list(objects)
    if lods:
        for level,ratio in ((1,.60 if name.startswith('Palm') else .40),(2,.30 if name.startswith('Palm') else .16)):
            for o in objects:
                clone=o.copy();clone.data=o.data.copy();clone.name=o.name.replace('LOD0','LOD'+str(level));col.objects.link(clone)
                bpy.context.view_layer.objects.active=clone;mod=clone.modifiers.new('Distant simplification','DECIMATE');mod.ratio=ratio
                bpy.ops.object.modifier_apply(modifier=mod.name);clone.hide_render=True;all_objects.append(clone)
    path=ART/'Shared'/(name+'.fbx');bpy.ops.object.select_all(action='DESELECT')
    for o in all_objects:o.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False,path_mode='RELATIVE')
    tris=lambda obs:sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in obs)
    return dict(name=name,file=name+'.fbx',collision=collision,boxes=boxes or [],lods=lods,triangles=tris(objects)),col,objects

def build_kit(mats):
    specs=[];library={}
    for name,fn,lods,boxes in [(f'Palm_{i}',lambda m,i=i:palm(m,i),True,[dict(center=[.4,3.5,0],size=[.65,7,.65])]) for i in range(4)]+[(f'Plant_{i}',lambda m,i=i:plant(m,i),True,[]) for i in range(4)]+[(f'Grass_{i}',lambda m,i=i:grass(m,i),True,[]) for i in range(3)]+[(f'Rock_{i}',lambda m,i=i:limestone(m,i),True,[]) for i in range(4)]+[('PlayerStand',lambda m:deck(m),False,[dict(center=[0,-.16,3.5],size=[6,.32,7])]),('InletBridge',lambda m:deck(m,True),False,[dict(center=[0,-.18,0],size=[13,.36,4.2])]),('GolfPavilion',lambda m:pavilion(m),False,[dict(center=[0,-.16,0],size=[11,.32,8])]),('FishingShelter',lambda m:pavilion(m,True),False,[dict(center=[0,-.16,0],size=[8,.32,5])])]:
        m=Meshes();fn(m);spec,col,objects=export_asset(name,m,mats,lods,boxes,'rock' if name.startswith('Rock') else 'boxes' if boxes else 'none');specs.append(spec);library[name]=(col,objects)
    (ART/'Shared'/'kit.json').write_text(json.dumps(dict(modules=specs),indent=2))
    source_path=SOURCE/'IslandAssetKit.blend'
    # Keep packed and external texture references portable in saved masters.
    for image in bpy.data.images:
        if image.source=='FILE' and image.filepath:
            image.filepath=bpy.path.relpath(bpy.path.abspath(image.filepath),start=str(source_path.parent))
    bpy.ops.wm.save_as_mainfile(filepath=str(source_path),relative_remap=False,compress=True)
    return specs,library

