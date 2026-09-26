"""F1/B1 coastal scenery only. Never reads or rewrites either stadium source.
Metres, Blender XY ground/Z up; FBX receives a 180 degree Unity correction.
Run in Blender background with --factory-startup. Deterministic seed 926.
"""
import bpy, bmesh, math, random, json
import numpy as np
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Game/Assets/_Game/Art/CoastalIslands'; SRC=ROOT/'ArtSource/CoastalIslands'
for p in (OUT,SRC): p.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
random.seed(926); rng=np.random.default_rng(926); PI=math.pi
materials={}; specs=[]; batches={}; group=''; records=[]

def rgb(h): return np.array([int(h[i:i+2],16)/255 for i in (0,2,4)])
def smooth(a,b,x):
    t=np.clip((x-a)/(b-a),0,1); return t*t*(3-2*t)
def image(name,data):
    h,w=data.shape[:2]; im=bpy.data.images.new(name,width=w,height=h,alpha=False)
    rgba=np.ones((h,w,4),np.float32); rgba[:,:,:3]=data[:,:,None] if data.ndim==2 else data
    im.pixels.foreach_set(rgba.ravel()); im.filepath_raw=str(OUT/(name+'.png')); im.file_format='PNG'; im.save()
    bpy.data.images.remove(im); im=bpy.data.images.load(str(OUT/(name+'.png'))); im.name=name; im.pack(); return im
def normal(name,height,strength):
    dx=(np.roll(height,-1,1)-np.roll(height,1,1))*strength; dy=(np.roll(height,-1,0)-np.roll(height,1,0))*strength
    v=np.stack((-dx,-dy,np.ones_like(dx)),2); v/=np.linalg.norm(v,axis=2)[:,:,None]; return image(name,v*.5+.5)
def material(name,color,tex=None,rough=.8,bump=None):
    m=bpy.data.materials.new(name); m.use_nodes=True; c=rgb(color); m.diffuse_color=(*c,1)
    bs=m.node_tree.nodes.get('Principled BSDF'); bs.inputs['Base Color'].default_value=(*c,1);bs.inputs['Roughness'].default_value=rough
    if tex:
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=tex
        mul=m.node_tree.nodes.new('ShaderNodeMixRGB');mul.blend_type='MULTIPLY';mul.inputs[0].default_value=1;mul.inputs[2].default_value=(*c,1)
        m.node_tree.links.new(t.outputs['Color'],mul.inputs[1]);m.node_tree.links.new(mul.outputs[0],bs.inputs['Base Color'])
    if bump:
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=bump;t.image.colorspace_settings.name='Non-Color'
        nm=m.node_tree.nodes.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.22
        m.node_tree.links.new(t.outputs['Color'],nm.inputs['Color']);m.node_tree.links.new(nm.outputs['Normal'],bs.inputs['Normal'])
    materials[name]=m;specs.append(dict(name=name,color_srgb=c.tolist(),base_map=tex.name+'.png' if tex else '',normal=bump.name+'.png' if bump else '',roughness=rough));return name

# Authored seamless microtexture maps: subdued, broad cartoon strokes plus fine relief.
N=1024; y,x=np.mgrid[0:N,0:N]/N; noise=rng.random((N,N))
def softnoise(n):
    lattice=rng.random((n,n));yy,xx=np.mgrid[0:N,0:N]/N*n;ix=xx.astype(int);iy=yy.astype(int);fx=xx-ix;fy=yy-iy;fx=fx*fx*(3-2*fx);fy=fy*fy*(3-2*fy)
    return (lattice[iy,ix]*(1-fx)+lattice[iy,(ix+1)%n]*fx)*(1-fy)+(lattice[(iy+1)%n,ix]*(1-fx)+lattice[(iy+1)%n,(ix+1)%n]*fx)*fy
grain=.50+.05*np.sin(2*PI*x*13+1.8*np.sin(y*2*PI*2))+.014*np.sin(x*2*PI*43+np.sin(y*2*PI*3)*3)+.012*noise
wood=image('Coast_WoodGrain',np.clip(grain*.55+.52,0,1)); wood_n=normal('Coast_WoodNormal',grain,1.2)
stone_h=.5+.22*(softnoise(5)-.5)+.12*(softnoise(19)-.5)+.05*(softnoise(61)-.5)+.014*noise
stone=image('Coast_LimestoneWash',np.clip(stone_h*.65+.47,0,1)); stone_n=normal('Coast_StoneNormal',stone_h,2)
sand_h=.4+.10*np.sin(y*PI*30+1.2*np.sin(x*PI*6))+.10*noise
detail=image('Coast_GroundDetail',np.clip(.5+(sand_h-.5)*.16,0,1)); detail_n=normal('Coast_GroundNormal',sand_h,1.8)
leaf_h=.75+.1*np.sin(x*PI*24+y*PI*12)+.08*np.sin(y*PI*2)
leaf=image('Coast_LeafVeins',np.clip(leaf_h,0,1));leaf_n=normal('Coast_LeafNormal',leaf_h,1.5)
STONE=material('Coast_Limestone','EDDAB4',stone,.89,stone_n)
ROCKSHADE=material('Coast_StoneShade','C8BA9C',stone,.9,stone_n)
MOSS=material('Coast_Moss','81A646',leaf,.9,leaf_n)
WOOD=material('Coast_HoneyTimber','BC8753',wood,.65,wood_n)
WOOD2=material('Coast_LightTimber','D5A974',wood,.7,wood_n)
BARK=material('Coast_PalmBark','997046',wood,.92,wood_n)
LEAF=material('Coast_PalmLeaf','4E973C',leaf,.72,leaf_n)
LIME=material('Coast_LeafSun','91BD49',leaf,.8,leaf_n)
DARK=material('Coast_LeafShade','347142',leaf,.9,leaf_n)
GRASS=material('Coast_Grass','83AA40',leaf,.9)
ROPE=material('Coast_Rope','E7D1A0',wood,.92,wood_n)
TEAL=material('Coast_Enamel','319D9B',stone,.4)
IRON=material('Coast_Bolt','58666A',None,.42)
PINK=material('Coast_CoralFlowers','F48F89',None,.72)
GOLD=material('Coast_GoldFlowers','F4C657',None,.72)
LILAC=material('Coast_LilacFlowers','B69ADD',None,.7)
SHELL=material('Coast_Shell','F8E9CB',stone,.6)
CLOUD=material('Coast_Cloud','FFFFFF',None,1)

class Batch:
    def __init__(self,name):self.name=name;self.v=[];self.f=[];self.uv=[];self.m=[];self.mi=[];self.sm=[]
    def add(self,v,f,mat,sm=True,uv=None):
        offset=len(self.v);self.v.extend(v)
        if mat not in self.m:self.m.append(mat)
        for face in f:
            self.f.append(tuple(offset+i for i in face));self.mi.append(self.m.index(mat));self.sm.append(sm)
            if uv:self.uv.append([uv[i] for i in face])
            else:
                norm=(Vector(v[face[1]])-Vector(v[face[0]])).cross(Vector(v[face[2]])-Vector(v[face[0]]))
                axes=[k for k in range(3) if k!=max(range(3),key=lambda k:abs(norm[k]))]
                self.uv.append([tuple(v[i][k]*.42 for k in axes) for i in face])
    def finish(self):
        mesh=bpy.data.meshes.new(self.name);mesh.from_pydata(self.v,[],self.f);mesh.update();uv=mesh.uv_layers.new(name='UVMap')
        for m in self.m:mesh.materials.append(materials[m])
        for p,co,mi,sm in zip(mesh.polygons,self.uv,self.mi,self.sm):
            p.material_index=mi;p.use_smooth=sm
            for idx,t in zip(p.loop_indices,co):uv.data[idx].uv=t
        ob=bpy.data.objects.new(self.name,mesh);bpy.context.scene.collection.objects.link(ob);return ob
def batch(name):
    key=group+'__'+name
    if key not in batches:batches[key]=Batch(key)
    return batches[key]
def beam(name,a,b,r,mat,n=10,r2=None):
    a,b=Vector(a),Vector(b);q=(b-a).to_track_quat('Z','Y');r2=r if r2 is None else r2
    v=[tuple(c+q@Vector((rr*math.cos(i*2*PI/n),rr*math.sin(i*2*PI/n),0))) for c,rr in ((a,r),(b,r2)) for i in range(n)]
    f=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    batch(name).add(v,f,mat)
def box(name,p,size,mat,bevel=.05):
    bm=bmesh.new();bmesh.ops.create_cube(bm,size=1)
    for v in bm.verts:v.co=Vector(tuple(v.co[i]*size[i] for i in range(3)))
    if bevel:bmesh.ops.bevel(bm,geom=list(bm.edges),offset=bevel,segments=3,affect='EDGES')
    bm.verts.ensure_lookup_table();bm.verts.index_update();batch(name).add([tuple(v.co+Vector(p)) for v in bm.verts],[tuple(v.index for v in f.verts) for f in bm.faces],mat,False);bm.free()
ico={}
def blob(name,p,size,mat,seed=0,sub=3):
    if sub not in ico:
        bm=bmesh.new();bmesh.ops.create_icosphere(bm,subdivisions=sub,radius=1);bm.verts.ensure_lookup_table();bm.verts.index_update()
        ico[sub]=([tuple(v.co) for v in bm.verts],[tuple(v.index for v in f.verts) for f in bm.faces]);bm.free()
    v,f=ico[sub];v2=[]
    for c in v:
        irregular=1+.07*math.sin(c[0]*5+seed)*math.cos(c[1]*4+seed*.2)+.045*math.cos(c[2]*7+seed)
        v2.append(tuple(p[k]+c[k]*size[k]*irregular for k in range(3)))
    batch(name).add(v2,f,mat)
def rope(name,pts,r=.055):
    for a,b in zip(pts,pts[1:]):beam(name,a,b,r,ROPE,8)
def frond(name,origin,angle,length,width,mat,lift=1.1,droop=1.5):
    o=Vector(origin);d=Vector((math.cos(angle),math.sin(angle),0));s=Vector((-d.y,d.x,0));v=[];uv=[];steps=18
    for j in range(steps+1):
        t=j/steps;center=o+d*length*t+Vector((0,0,lift*math.sin(PI*t)-droop*t*t));w=width*math.sin(PI*t)**.65
        for k in (-1,0,1):
            v.append(tuple(center+s*w*k+Vector((0,0,(1-abs(k))*.13*math.sin(PI*t)))));uv.append(((k+1)/2,t))
    f=[]
    for j in range(steps):
        a=j*3;b=a+3;f.extend([(a,b,b+1,a+1),(a+1,b+1,b+2,a+2)])
    nv=len(v);v+=v.copy();uv+=uv.copy();f += [tuple(nv+k for k in reversed(face)) for face in f.copy()]
    batch(name).add(v,f,mat,True,uv)
    return o,d,s
def palm(name,x,y,z,height,angle):
    bend=Vector((math.cos(angle),math.sin(angle),0))*height*.16;points=[]
    for j in range(19):
        t=j/18;p=Vector((x,y,z))+bend*t*t+Vector((0,0,height*t));points.append(p)
    for j in range(18):
        radius=.32*(1-.40*j/18);beam(name,points[j],points[j+1],radius,BARK,12,radius*.98)
        a=points[j+1];beam(name,a-Vector((0,0,.035)),a+Vector((0,0,.035)),radius*1.12,WOOD2,12)
    top=points[-1]
    for j in range(9):
        a=angle+j*2*PI/9;length=height*random.uniform(.48,.61)
        d=Vector((math.cos(a),math.sin(a),0));s=Vector((-d.y,d.x,0))
        # Individual paired leaflets, each with a curved centre ridge, no flat star canopy.
        mid=[]
        for t in np.linspace(0,1,15):mid.append(top+d*length*t+Vector((0,0,1.25*math.sin(PI*t)-1.55*t*t)))
        for aa,bb in zip(mid,mid[1:]):beam(name,aa,bb,.027,LIME,6)
        for k in range(1,13):
            t=k/14;c=top+d*length*t+Vector((0,0,1.25*math.sin(PI*t)-1.55*t*t))
            ln=.9*math.sin(PI*t)**.55*(height/8)
            for side in (-1,1):frond(name,c,a+side*1.05,ln,.115,LEAF if (k+j)%3 else LIME,.13,.27)
    for j in range(4):blob(name,tuple(top+Vector((math.cos(j*1.6)*.3,math.sin(j*1.6)*.3,-.3))),(.20,.22,.26),BARK,j,2)
def planting(name,x,y,z,scale=1):
    for j in range(8):
        a=j*PI/4;frond(name,(x,y,z),a,.9*scale,.25*scale,DARK if j%3==0 else LEAF,.45*scale,.12*scale)
    for j in range(6):
        a=j*2.4;px=x+math.cos(a)*.42*scale;py=y+math.sin(a)*.42*scale;h=random.uniform(.4,.8)*scale
        beam(name,(px,py,z),(px,py,z+h),.025,DARK,6)
        for k in range(5):
            aa=k*2*PI/5;blob(name,(px+.13*scale*math.cos(aa),py+.13*scale*math.sin(aa),z+h),(.12*scale,.10*scale,.05*scale),[PINK,GOLD,LILAC][j%3],k,2)
        blob(name,(px,py,z+h+.025),(.065*scale,.065*scale,.055*scale),GOLD,0,2)
def tuft(name,x,y,z,s):
    for k in range(7):frond(name,(x,y,z),k*2.4,s*random.uniform(.4,.8),s*.047,GRASS if k%2 else LIME,s*.35,s*.08)
def meadow(name,x,y,z,s):
    for k in range(7):
        a=k*2.4;d=Vector((math.cos(a),math.sin(a),0));side=Vector((-d.y,d.x,0));o=Vector((x,y,z));h=s*random.uniform(.5,1)
        v=[tuple(o-side*.035),tuple(o+side*.035),tuple(o+d*h*.23+Vector((0,0,h*.55))+side*.026),tuple(o+d*h*.23+Vector((0,0,h*.55))-side*.026),tuple(o+d*h*.56+Vector((0,0,h)))]
        batch(name).add(v,[(0,1,2,3),(3,2,4),(3,2,1,0),(4,2,3)],GRASS if k%3 else LIME)

def outline(a,rx,ry):return 1+.035*np.sin(3*a+.4)+.023*np.cos(7*a)
def ground(x,y,rx,ry):
    a=np.arctan2(y/ry,x/rx);r=np.sqrt((x/rx)**2+(y/ry)**2)/outline(a,rx,ry)
    base=-.42+.75*np.exp(-((r-.76)/.115)**2)*(.7+.3*np.sin(5*a)**2)
    return base*(1-smooth(.85,1.035,r))-3.6*smooth(.85,1.035,r)
def landscape(sport,rx,ry,extent):
    global group
    group=sport;before=set(batches)
    # Four-kilopixel continuous terrain color, world-scale detail normals in Unity.
    n=4096; yy,xx=np.mgrid[0:n,0:n].astype(np.float32)/(n-1)*extent*2-extent
    aa=np.arctan2(yy/ry,xx/rx);rr=np.sqrt((xx/rx)**2+(yy/ry)**2)/outline(aa,rx,ry)
    variation=.027*np.sin(xx*.083+np.sin(yy*.11)*2)+.019*np.cos(yy*.071+np.sin(xx*.053)*3)+rng.random((n,n),dtype=np.float32)*.042
    grass=rgb('88AD48')[None,None,:]+variation[:,:,None]
    sand=rgb('E8D4AA')[None,None,:]+variation[:,:,None]*.42
    blend=smooth(.80,.86,rr+.006*np.sin(xx*1.3)*np.sin(yy*1.1))
    col=grass*(1-blend[:,:,None])+sand*blend[:,:,None]
    path=np.exp(-((rr-.68)/.022)**8)
    # Broad dock approach joins the loop, plus four entry spokes.
    approach=(1-smooth(2.5,3.7,np.abs(xx)))*smooth(.59,.69,rr)*(1-smooth(.91,.96,rr))*(yy<0)
    paths=np.maximum(path,approach)
    pathcol=rgb('CDBB98')[None,None,:]+variation[:,:,None]*.38
    col=col*(1-paths[:,:,None])+pathcol*paths[:,:,None]
    tex=image(sport+'_GroundColor',np.clip(col,0,1));TERRAIN=material(sport+'_Terrain','FFFFFF',tex,.92)
    del yy,xx,aa,rr,variation,grass,sand,blend,col,path,approach,paths,pathcol
    for sec in range(16):
        v=[];uv=[];rows=86;cols=24
        for j in range(rows+1):
            r=1.065*j/rows
            for i in range(cols+1):
                a=(sec+i/cols)*2*PI/16;o=float(outline(a,rx,ry));x=rx*r*math.cos(a)*o;y=ry*r*math.sin(a)*o
                v.append((x,y,float(ground(x,y,rx,ry))));uv.append(((x+extent)/(2*extent),(y+extent)/(2*extent)))
        f=[]
        for j in range(rows):
            for i in range(cols):k=j*(cols+1)+i;f.append((k,k+1,k+cols+2,k+cols+1))
        # XY angular grid winds counter-clockwise along angle then radial -> reverse for +Z.
        batch('Terrain_%02d'%sec).add(v,[tuple(reversed(fa)) for fa in f],TERRAIN,True,uv)
    # A continuous promenade with individually rounded inset pavers at a human scale.
    circumference=PI*2*math.sqrt((rx*rx+ry*ry)/2)*.68
    for j in range(int(circumference/1.25)):
        a=j/int(circumference/1.25)*2*PI;o=float(outline(a,rx,ry));x=rx*.68*o*math.cos(a);y=ry*.68*o*math.sin(a);z=float(ground(x,y,rx,ry))+.035
        # Sparse staggered stepping accents embedded flush with path.
        if j%3==0:blob('Paving_%02d'%(j%16),(x,y,z),(.65,.45,.095),STONE,j,2)
    # Natural clustered planting: varied sizes, open sight lines, no uniform ring of props.
    count=52 if sport=='Football' else 32
    for j in range(count):
        a=(j+.35)*2*PI/count;r=random.choice([.75,.78,.59]);o=float(outline(a,rx,ry));x=rx*r*math.cos(a)*o;y=ry*r*math.sin(a)*o
        # Venue footprints remain empty, including stadium's existing landscape module.
        if abs(x)<(64 if sport=='Football' else 27) and abs(y)<(85 if sport=='Football' else 35):continue
        if y<0 and abs(x)<7:continue
        z=float(ground(x,y,rx,ry));name='PalmGarden_%02d'%(j%16)
        palm(name,x,y,z,random.uniform(10,15) if sport=='Football' else random.uniform(8,12),a+random.uniform(-.8,.8))
        for k in range(4):
            xx=x+math.cos(k*1.9)*random.uniform(1.1,2.6);yy=y+math.sin(k*1.9)*random.uniform(1.1,2.6)
            planting(name,xx,yy,float(ground(xx,yy,rx,ry)),random.uniform(.8,1.25))
        for k in range(12):
            xx=x+random.uniform(-3.5,3.5);yy=y+random.uniform(-3.5,3.5);tuft(name,xx,yy,float(ground(xx,yy,rx,ry)),random.uniform(.35,.7))
    # Shore boulders form occasional headlands, alternating with broad clean beaches.
    for cluster in range(14):
        a=cluster*2*PI/14+.18;r=.885 if cluster%3 else .95
        for j in range(random.randint(3,6)):
            aa=a+random.uniform(-.035,.035);o=float(outline(aa,rx,ry));xx=rx*r*math.cos(aa)*o+random.uniform(-2,2);yy=ry*r*math.sin(aa)*o+random.uniform(-2,2)
            if yy<0 and abs(xx)<8:continue
            s=random.uniform(.75,4.1);z=float(ground(xx,yy,rx,ry));name='CoastRocks_%02d'%cluster
            blob(name,(xx,yy,z+s*.35),(s,s*.8,s*.95),STONE if j%3 else ROCKSHADE,j+cluster*9,3)
            if j%3==0:blob(name,(xx,yy,z+s*1.13),(s*.55,s*.49,s*.14),MOSS,j,2)
    for j in range(6200 if sport=='Football' else 3400):
        a=random.uniform(0,2*PI);r=random.uniform(.54,.83)
        if .647<r<.713:continue
        o=float(outline(a,rx,ry));x=rx*r*math.cos(a)*o;y=ry*r*math.sin(a)*o
        if abs(x)<(65 if sport=='Football' else 28) and abs(y)<(86 if sport=='Football' else 36):continue
        if y<0 and abs(x)<6:continue
        meadow('Meadow_%02d'%int(a/(2*PI)*16),x,y,float(ground(x,y,rx,ry)),random.uniform(.14,.39))
    # Pebbles, ribbed shells and small grass clumps reward close cameras.
    for j in range(520 if sport=='Football' else 340):
        a=random.uniform(0,2*PI);r=random.uniform(.82,.96);o=float(outline(a,rx,ry));x=rx*r*math.cos(a)*o;y=ry*r*math.sin(a)*o;z=float(ground(x,y,rx,ry))
        if z<-1.55:continue
        name='BeachDetails_%02d'%int(a/(2*PI)*16);s=random.uniform(.04,.17)
        blob(name,(x,y,z+.035),(s,s*.7,s*.38),SHELL if j%4==0 else STONE,j,2)
        if j%4==0:
            for k in range(5):
                a2=k*.24+.9;beam(name,(x,y,z+.05),(x+math.cos(a2)*s,y+math.sin(a2)*s,z+.06),.009,SHELL,5)
    # Solid stone-edged stair approach, timber planks, grain, fasteners, rope lashings.
    dockstart=-ry*.93*float(outline(-PI/2,rx,ry));deck=-.7
    for j in range(6):box('ArrivalStairs',(0,dockstart+3.0-j*.55,-.47-j*.065),(6.4,.61,.20),STONE,.06)
    for j in range(40):
        y=dockstart-j*.48;box('DockPlanks',(0,y,deck),(5.7,.452,.22),WOOD2 if j%4==0 else WOOD,.038)
        for x in (-2.30,2.30):
            for yy in (y-.14,y+.14):beam('DockFasteners',(x,yy,deck+.108),(x,yy,deck+.12),.035,IRON,8)
    end=dockstart-39*.48
    for x in (-2,2):box('DockBearers',(x,(dockstart+end)/2,deck-.35),(.3,20,.5),BARK,.06)
    for x in (-2.65,2.65):
        for j in range(7):
            y=dockstart-j*3.1;beam('DockPiles',(x,y,-5),(x,y,deck+1.05),.19,BARK,14,.16)
            beam('DockCaps',(x,y,deck+.96),(x,y,deck+1.14),.205,TEAL,14)
            for k in range(4):
                z=deck+.60+k*.052;rope('DockLashing',[(x+.192*math.cos(a),y+.192*math.sin(a),z) for a in np.linspace(0,2*PI,21)],.023)
            if j<6:
                for ht in (.40,.79):rope('DockRailing',[(x,y-t*3.1,deck+ht-.18*math.sin(PI*t)) for t in np.linspace(0,1,18)],.045)
    # Beach access bridge over a shallow tidal swale beside the arrival garden.
    bridge_x=rx*.47;bridge_y=-ry*.66
    for j in range(24):
        t=j/23;x=bridge_x+(t-.5)*11;z=.2+.55*math.sin(PI*t)
        box('GardenBridge',(x,bridge_y,z),(.44,3.2,.20),WOOD2 if j%4==0 else WOOD,.04)
    for y in (bridge_y-1.48,bridge_y+1.48):
        for j in range(5):
            x=bridge_x-5.5+j*2.75;z=.2+.55*math.sin(PI*j/4);beam('GardenBridge',(x,y,-1),(x,y,z+1.1),.12,BARK,12)
        rope('GardenBridge',[(bridge_x-5.5+t*11,y,1.18+.55*math.sin(PI*t)) for t in np.linspace(0,1,40)],.075)
    # Sand berm anchors behind B1, not cliffs: preserve the selected low island profile.
    for j in range(4):
        x=(-.23+j*.10)*rx;y=ry*.77;z=float(ground(x,y,rx,ry));s=5+j%3
        blob('GardenOutcrop',(x,y,z+s*.35),(s,s*.85,s),STONE,j+74,3)
    objects=[batches[k].finish() for k in batches if k not in before]
    bpy.ops.object.select_all(action='DESELECT')
    for ob in objects:ob.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.export_scene.fbx(filepath=str(OUT/(sport+'_Coast.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='STRIP')
    col=bpy.data.collections.new(sport+'_Exterior');bpy.context.scene.collection.children.link(col)
    for ob in objects:
        for c in list(ob.users_collection):c.objects.unlink(ob)
        col.objects.link(ob)
    records.append(dict(sport=sport,rx=rx,ry=ry,extent=extent,meshes=len(objects),triangles=sum(sum(len(p.vertices)-2 for p in ob.data.polygons) for ob in objects),dockStart=dockstart))
    col.hide_render=sport=='Basketball';col.hide_viewport=sport=='Basketball'
    print('COAST_EXPORTED',records[-1],flush=True)

landscape('Football',132,160,185)
landscape('Basketball',70,83,105)

# Smooth connected volumetric clouds, a reusable editable mesh (no billboard cards).
group='Sky';old=set(batches)
for j,(p,s) in enumerate([((0,0,0),(5,3,1.8)),((-3,0,.8),(3,2.8,2.1)),((.2,0,2),(3.5,3,3)),((3.7,0,.7),(3.1,2.4,2)),((1,1.5,.3),(4,2.4,1.8))]):blob('Cumulus',p,s,CLOUD,j,3)
ob=batches['Sky__Cumulus'].finish();bpy.context.view_layer.objects.active=ob
rem=ob.modifiers.new('Unified soft cloud silhouette','REMESH');rem.mode='VOXEL';rem.voxel_size=.27;rem.use_smooth_shade=True;bpy.ops.object.modifier_apply(modifier=rem.name)
sm=ob.modifiers.new('Soft cloud lobes','SMOOTH');sm.factor=1.3;sm.iterations=4;bpy.ops.object.modifier_apply(modifier=sm.name)
bpy.ops.object.select_all(action='DESELECT');ob.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'Cumulus.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='STRIP')
ob.hide_render=True;ob.hide_viewport=True
manifest=dict(materials=specs,islands=records,detail='Coast_GroundDetail.png',detailNormal='Coast_GroundNormal.png',scope='Exterior only; venue sources and prefabs untouched; no traversal added.')
(OUT/'coast-manifest.json').write_text(json.dumps(manifest,indent=2));(SRC/'coast-manifest.json').write_text(json.dumps(manifest,indent=2))
bpy.context.scene.unit_settings.system='METRIC'
bpy.ops.wm.save_as_mainfile(filepath=str(SRC/'PalmShore_Islands.blend'))
print('COAST_SOURCE_COMPLETE',flush=True)
