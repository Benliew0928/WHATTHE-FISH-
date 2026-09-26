"""Shared, metre-scaled coastal architecture. Called by the two venue generators.

Exports an independent detailed facade and a lightweight collision model. UV0 is
in metres / four, giving 512 texels/metre with the shared 2048px material library.
"""
import bpy, bmesh, math, json
import numpy as np
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT/'Game/Assets/_Game/Art/CoastalStadiums'
SOURCE = ROOT/'ArtSource/CoastalStadiums'
PI=math.pi

def materials():
    ART.mkdir(parents=True,exist_ok=True); SOURCE.mkdir(parents=True,exist_ok=True)
    size=2048; y,x=np.mgrid[0:size,0:size].astype(np.float32)/size
    rng=np.random.default_rng(926)
    noise=rng.random((size,size),dtype=np.float32)-.5
    broad=np.sin(x*2*PI)*np.cos(y*4*PI)+.35*np.cos(x*10*PI+y*6*PI)
    grain=np.sin(x*1500+np.sin(y*12*PI)*2)*.5+np.sin(x*3300+y*8*PI)*.22
    out={};spec=[]
    colors={'Stone':('ECD8B0',.78),'Plaster':('F4E7CB',.85),'Paving':('D8C5A4',.76),'Timber':('BC8148',.61),'Teal':('258D97',.4),'Bronze':('B9873D',.38),'Leaf':('4A923E',.75),'Flower':('E98092',.69),'Lamp':('FFE3A1',.45),'Screen':('173F4C',.45)}
    for kind,(hexcode,rough) in colors.items():
        name='Coastal_'+kind
        m=bpy.data.materials.new(name);m.use_nodes=True
        bs=m.node_tree.nodes.get('Principled BSDF')
        color=np.array([int(hexcode[i:i+2],16)/255 for i in (0,2,4)])
        height=.5+.035*broad+.05*noise
        if kind=='Timber':height=.5+.12*grain+.018*noise
        if kind in ('Stone','Paving'):
            row=np.floor(y*8);u=(x*4+(row%2)*.5)%1;v=y*8%1
            joint=(u<.008)|(u>.992)|(v<.018)|(v>.982)
            height-=joint*.17
            height+=.012*np.sin(np.floor(x*4+(row%2)*.5)*17+row*13)
        wash=.96+(height-.5)*.38
        if kind in ('Teal','Bronze','Leaf','Flower','Lamp','Screen'):wash=.98+.008*broad+.01*noise
        base=np.clip(color[None,None,:]*wash[:,:,None],0,1)
        def image(name,data,noncolor=False):
            im=bpy.data.images.get(name) or bpy.data.images.new(name,width=size,height=size,alpha=True)
            if noncolor:im.colorspace_settings.name='Non-Color'
            im.use_fake_user=True
            px=np.ones((size,size,4),np.float32);px[:,:,:data.shape[2]]=data
            im.pixels.foreach_set(px.ravel());im.filepath_raw=str(ART/(name+'.png'));im.file_format='PNG';im.save();im.pack();return im
        im=image(name+'_BaseColor',base)
        dx=(np.roll(height,-1,1)-np.roll(height,1,1))*2.5;dy=(np.roll(height,-1,0)-np.roll(height,1,0))*2.5
        normal=np.stack((-dx,-dy,np.ones_like(dx)),axis=2);normal/=np.linalg.norm(normal,axis=2)[:,:,None]
        nm=image(name+'_Normal',normal*.5+.5,True)
        mask=np.zeros((size,size,4),np.float32);mask[:,:,0]=.55 if kind=='Bronze' else .08 if kind=='Teal' else 0
        mask[:,:,3]=np.clip(1-rough+(height-.5)*.12,0,1)
        sm=image(name+'_Mask',mask,True)
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=im;m.node_tree.links.new(t.outputs['Color'],bs.inputs['Base Color'])
        t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=nm;n=m.node_tree.nodes.new('ShaderNodeNormalMap');n.inputs['Strength'].default_value=.45;m.node_tree.links.new(t.outputs['Color'],n.inputs['Color']);m.node_tree.links.new(n.outputs['Normal'],bs.inputs['Normal'])
        bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=float(mask[0,0,0])
        if kind=='Lamp':bs.inputs['Emission Color'].default_value=(*color,1);bs.inputs['Emission Strength'].default_value=.6
        out[kind]=m;spec.append({'name':name,'base_map':im.name+'.png','normal':nm.name+'.png','mask':sm.name+'.png','roughness':rough})
    (ART/'materials.json').write_text(json.dumps({'materials':spec},indent=2))
    return out

class Meshes:
    def __init__(self):self.data={};self.mapping=lambda p:p;self.cache={}
    def add(self,name,verts,faces,mat,smooth=False):
        key=(name,mat);v,f,uv=self.data.setdefault(key,([],[],[]));offset=len(v)
        v.extend(self.mapping(p) for p in verts)
        for face in faces:
            f.append(tuple(offset+i for i in face))
            n=(Vector(verts[face[1]])-Vector(verts[face[0]])).cross(Vector(verts[face[2]])-Vector(verts[face[0]]))
            axes=[a for a in range(3) if a!=max(range(3),key=lambda a:abs(n[a]))]
            uv.append([(verts[i][axes[0]]/4,verts[i][axes[1]]/4) for i in face])
    def box(self,name,p,size,mat='Stone',bevel=.035):
        bevel=min(bevel,min(size)*.4);key=(*size,bevel)
        if key not in self.cache:
            bm=bmesh.new();bmesh.ops.create_cube(bm,size=1)
            for v in bm.verts:v.co=Vector(tuple(v.co[i]*size[i] for i in range(3)))
            if bevel:bmesh.ops.bevel(bm,geom=list(bm.edges),offset=bevel,segments=2,affect='EDGES')
            bm.verts.ensure_lookup_table();bm.verts.index_update();self.cache[key]=([tuple(v.co) for v in bm.verts],[tuple(v.index for v in f.verts) for f in bm.faces]);bm.free()
        v,f=self.cache[key];self.add(name,[tuple(vv[i]+p[i] for i in range(3)) for vv in v],f,mat)
    def beam(self,name,a,b,r=.035,mat='Teal',segments=10):
        d=Vector(b)-Vector(a);q=d.to_track_quat('Z','Y');mid=(Vector(a)+Vector(b))/2
        v=[tuple(mid+q@Vector((r*math.cos(i*2*PI/segments),r*math.sin(i*2*PI/segments),z))) for z in (-d.length/2,d.length/2) for i in range(segments)]
        f=[tuple(range(segments-1,-1,-1)),tuple(range(segments,segments*2))]+[(i,(i+1)%segments,(i+1)%segments+segments,i+segments) for i in range(segments)]
        self.add(name,v,f,mat)
    def petal(self,name,p,size,mat):
        key='petal'
        if key not in self.cache:
            bm=bmesh.new();bmesh.ops.create_icosphere(bm,subdivisions=2,radius=1);bm.verts.ensure_lookup_table();bm.verts.index_update()
            self.cache[key]=([tuple(v.co) for v in bm.verts],[tuple(v.index for v in f.verts) for f in bm.faces]);bm.free()
        v,f=self.cache[key];self.add(name,[tuple(p[i]+vv[i]*size[i] for i in range(3)) for vv in v],f,mat)
    def leaf(self,name,p,angle,length,width):
        v=[];steps=8
        for j in range(steps+1):
            t=j/steps;w=width*math.sin(PI*t)**.75;z=p[2]+.35*math.sin(PI*t)-.08*t
            for side in (-1,0,1):v.append((p[0]+math.cos(angle)*length*t-math.sin(angle)*w*side,p[1]+math.sin(angle)*length*t+math.cos(angle)*w*side,z+(.045 if side==0 else 0)*math.sin(PI*t)))
        f=[]
        for j in range(steps):
            a=j*3;b=a+3
            if j==0:f.extend([(a,b,b+1),(a,b+1,b+2)])
            elif j==steps-1:f.extend([(a,b,a+1),(a+1,b,a+2)])
            else:f.extend([(a,b,b+1,a+1),(a+1,b+1,b+2,a+2)])
        f+=list(tuple(reversed(face)) for face in f.copy());self.add(name,v,f,'Leaf')
    def slab(self,name,path,s0,s1,d0,d1,z0,z1,mat='Paving',steps=8):
        old=self.mapping;self.mapping=lambda p:p
        v=[path(s0+(s1-s0)*i/steps,d,z) for z in (z0,z1) for d in (d0,d1) for i in range(steps+1)]
        n=steps+1;f=[]
        for i in range(steps):f.extend([(i,i+1,n+i+1,n+i),(2*n+i,3*n+i,3*n+i+1,2*n+i+1),(i,2*n+i,2*n+i+1,i+1),(n+i,n+i+1,3*n+i+1,3*n+i)])
        f.extend([(0,n,3*n,2*n),(steps,2*n+steps,3*n+steps,n+steps)])
        self.add(name,v,f,mat);self.mapping=old
    def finish(self,sport,mats,collision=False):
        root=bpy.data.objects.new(sport+('_Collision' if collision else '_Architecture'),None);bpy.context.scene.collection.objects.link(root)
        for (name,mat),(v,f,uvs) in self.data.items():
            mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],f);mesh.update()
            layer=mesh.uv_layers.new(name='MetreUV')
            for poly,uv in zip(mesh.polygons,uvs):
                poly.use_smooth=mat in ('Leaf','Flower')
                for loop,co in zip(poly.loop_indices,uv):layer.data[loop].uv=co
            bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
            o=bpy.data.objects.new(name+'_'+mat,mesh);bpy.context.scene.collection.objects.link(o);o.parent=root
            mesh.materials.append(mats[mat]);o.hide_render=collision
        bpy.ops.object.select_all(action='DESELECT');root.select_set(True)
        for o in root.children:o.select_set(True)
        bpy.ops.export_scene.fbx(filepath=str(ART/(root.name+'.fbx')),use_selection=True,object_types={'EMPTY','MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False)
        if collision:
            for o in root.children:o.hide_set(True)
        return root

def build(ctx,sport):
    mats=materials();m=Meshes();c=Meshes();path=ctx['path'];baylen=ctx['BAY'];count=ctx['BAYS'];perim=ctx['PERIM'];football=sport=='Football'
    depth=14.5 if football else 9.;level=8.25 if football else 6.10
    # Football portals are centred on existing lower arcades; basketball entry
    # corridors follow four widened ground-level seating aisles.
    mids=[(i+.5)*baylen if football else i*baylen for i in range(count)]
    cardinal=[]
    for direction in ((0,1),(1,0),(0,-1),(-1,0)):
        best=min(range(count),key=lambda i:abs(path(mids[i],0,0)[0]*direction[1]-path(mids[i],0,0)[1]*direction[0])+(0 if path(mids[i],0,0)[0]*direction[0]+path(mids[i],0,0)[1]*direction[1]>0 else 10000))
        cardinal.append(best)
    def rail(name,a,b,z):
        # Local s/depth endpoints. Structural posts, capped bolts and crossed infill.
        length=math.dist(a,b);n=max(1,math.ceil(length/1.4))
        for h,r in ((z+.98,.052),(z+.28,.035)):
            m.beam(name,(*a,h),(*b,h),r,'Teal')
        for j in range(n+1):
            p=tuple(a[k]+(b[k]-a[k])*j/n for k in range(2))
            m.beam(name,(*p,z),(*p,z+1.02),.045,'Teal');m.box(name,(*p,z+.03),(.19,.19,.06),'Bronze',.02)
            m.box(name,(p[0],p[1]-.052,z+.82),(.065,.045,.065),'Bronze',.012)
            if j<n:
                q=tuple(a[k]+(b[k]-a[k])*(j+1)/n for k in range(2))
                m.beam(name,(*p,z+.30),(*q,z+.94),.022,'Teal');m.beam(name,(*p,z+.94),(*q,z+.30),.022,'Teal')
        # Simple invisible rail proxy follows only the solid railing.
        v=[(*p,h) for h in (z,z+1.12) for p in (a,b)]
        c.add(name+'_Rail',v,[(0,1,3,2),(2,3,1,0)],'Stone')
    def arch(name,w,d,spring,top):
        r=w/2;n=24
        v=[]
        for y in (d-.36,d+.36):
            for i in range(n+1):
                a=PI-i*PI/n;v.extend([(r*math.cos(a),y,spring+r*math.sin(a)),(r*math.cos(a),y,top)])
        k=(n+1)*2;f=[]
        for i in range(n):
            a=i*2;b=a+2;f.extend([(a,b,b+1,a+1),(k+a+1,k+b+1,k+b,k+a),(a,k+a,k+b,b),(a+1,b+1,k+b+1,k+a+1)])
        f.extend([(0,1,k+1,k),(2*n,k+2*n,k+2*n+1,2*n+1)])
        m.add(name,v,f,'Plaster');c.add(name,v,f,'Stone')
        for i in range(n):
            t=PI-i*PI/n-.008;u=PI-(i+1)*PI/n+.008
            v=[(rr*math.cos(a),d+y,spring+rr*math.sin(a)) for y in (-.43,.43) for rr,a in ((r,t),(r,u),(r+.30,u),(r+.30,t))]
            m.add(name+'_Voussoirs',v,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],'Stone')
        for side in (-1,1):
            x=side*(w/2+.30)
            m.box(name,(x,d,spring/2),(.60,.87,spring),'Stone',.06);c.box(name,(x,d,spring/2),(.60,.87,spring),bevel=0)
            m.box(name,(x,d,.18),(.88,1.12,.36),'Stone',.045)
            m.box(name,(x,d,spring),(.94,1.10,.24),'Stone',.04)
    def plants(name,x,y,z):
        m.box(name,(x,y,z+.35),(1.4,1.1,.7),'Stone',.09)
        m.box(name,(x,y,z+.70),(1.53,1.23,.13),'Stone',.035)
        m.box(name,(x,y,z+.77),(1.25,.98,.035),'Timber',0)
        for j in range(22):
            a=j*2.4;m.leaf(name,(x+math.cos(a)*.2,y+math.sin(a)*.18,z+.81+(j%3)*.06),a,.48+(j%4)*.1,.11+(j%3)*.03)
        for j in range(10):
            a=j*2.4;h=.22+(j%4)*.08;cx=x+math.cos(a)*.43;cy=y+math.sin(a)*.31
            m.beam(name,(cx,cy,z+.79),(cx,cy,z+.8+h),.018,'Leaf',6)
            for k in range(5):
                a=k*2*PI/5;m.petal(name,(cx+.105*math.cos(a),cy+.105*math.sin(a),z+.8+h),(.105,.092,.035),'Flower')
            m.petal(name,(cx,cy,z+.83+h),(.042,.042,.03),'Bronze')
    route=[]
    for i,mid in enumerate(mids):
        name='Bay_%02d'%i;mapping=lambda p,mid=mid:path(mid+p[0],p[1],p[2]);m.mapping=c.mapping=mapping
        w=min(6.4,baylen-1.1);spring=2.65 if football else 2.6
        arch(name,4.8 if football and i in cardinal else w,depth,.80 if football and i in cardinal else spring,level-.12)
        # Ground promenade and upper open balcony frame the arcade.
        for d0,d1,z0,z1 in ((depth-1,depth+4.8,-.4,-.025),(depth-1.9,depth+.5,level-.18,level)):
            m.slab(name,path,mid-baylen/2,mid+baylen/2,d0,d1,z0,z1)
            c.slab(name+'_Floor',path,mid-baylen/2,mid+baylen/2,d0,d1,z0,z1,steps=6)
        m.box(name,(0,depth,level-.1),(baylen,.98,.32),'Stone',.045)
        if not football:
            m.slab(name+'_UpperCornice',path,mid-baylen/2,mid+baylen/2,7.25,8.45,10.05,10.27,'Stone')
            m.slab(name+'_TealCoping',path,mid-baylen/2,mid+baylen/2,7.18,8.52,10.27,10.38,'Teal')
        # Coffered wood underside, real thickness and separate joints.
        for j in range(max(2,int(baylen/.3))):
            x=-baylen/2+.16+j*.3;m.box(name,(x,depth-.60,level-.36),(.27,2.1,.12),'Timber',.014)
        # Landing bridges interrupt the outer handrail rather than crossing it.
        cuts=[mids[k]+3.8+level*2.1+.7 for k in cardinal]
        intervals=[(-baylen/2,baylen/2)]
        for cut in cuts:
            local=(cut-mid+perim/2)%perim-perim/2
            result=[]
            for a,b in intervals:
                if local+.82<=a or local-.82>=b:result.append((a,b))
                else:
                    if a<local-.82:result.append((a,local-.82))
                    if b>local+.82:result.append((local+.82,b))
            intervals=result
        for a,b in intervals:rail(name,(a,depth+.42),(b,depth+.42),level)
        # Low ramped apron joins the original terrain without a 40cm kerb.
        v=[(-baylen/2,depth+4.8,-.025),(baylen/2,depth+4.8,-.025),(baylen/2,depth+7,-.42),(-baylen/2,depth+7,-.42)]
        m.add(name+'_Approach',v,[(0,1,2,3),(3,2,1,0)],'Paving');c.add(name+'_Approach',v,[(0,1,2,3),(3,2,1,0)],'Stone')
        for side in (-1,1):
            x=side*(baylen/2-.24)
            m.box(name,(x,depth,(level+.15)/2),(.62,.95,level+.15),'Stone',.06)
            c.box(name,(x,depth,level/2),(.62,.95,level),bevel=0)
            m.box(name,(x,depth,level+.20),(.98,1.18,.28),'Stone',.05)
            # Wall-mounted lantern with open frame and emissive glass core.
            m.beam(name,(x,depth+.47,3.3),(x,depth+1.0,3.3),.035,'Bronze')
            m.box(name,(x,depth+.92,2.95),(.24,.24,.43),'Lamp',.045)
            for dx in (-.14,.14):
                for dy in (-.14,.14):m.beam(name,(x+dx,depth+.92+dy,2.69),(x+dx,depth+.92+dy,3.21),.018,'Bronze',6)
            for z in (2.68,3.22):m.box(name,(x,depth+.92,z),(.38,.38,.08),'Bronze',.025)
        if i%2==0:
            for side in (-1,1):plants(name,side*(w/2+.35),depth+1.65,0)
        if i in cardinal:
            # A finished passage hides bare grandstand undersides at the four entries.
            half=2.25 if football else 1.05;roof=3.02 if football else 1.97
            for side in (-1,1):
                m.box(name+'_Tunnel',(side*(half+.13),depth/2,roof/2),(.26,depth,roof),'Plaster',.025)
                c.box(name+'_Tunnel',(side*(half+.13),depth/2,roof/2),(.26,depth,roof),bevel=0)
                for y in np.arange(.5,depth,.7):m.box(name+'_Tunnel',(side*(half+.01),float(y),.28),(.08,.68,.55),'Stone',.015)
            for y in np.arange(.15,depth,.3):m.box(name+'_Tunnel',(0,float(y),roof+.06),(half*2+.2,.28,.12),'Timber',.012)
            m.box(name+'_Sign',(0,depth+.46,6.55 if football else 5.22),(3.5,.12,.62),'Teal',.045)
            # Portal plaza: ground is continuous with the surrounding island.
            m.slab(name,path,mid-2.0,mid+2.0,-5 if football else -1.1,depth+9,-.45,-.025)
            c.slab(name+'_Entry',path,mid-2.0,mid+2.0,-5 if football else -1.1,depth+9,-.45,-.025)
            # The projecting entry plaza also needs its own flush terrain approach.
            v=[(-2,depth+9,-.025),(2,depth+9,-.025),(2,depth+11,-.42),(-2,depth+11,-.42)]
            m.add(name+'_EntryApproach',v,[(0,1,2,3),(3,2,1,0)],'Paving')
            c.add(name+'_EntryApproach',v,[(0,1,2,3),(3,2,1,0)],'Stone')
            # Exterior stair rises along the building, keeping the main tunnel open.
            run=level*2.1;start=3.8;d=depth+3.25;n=math.ceil(level/.20)
            for j in range(n):
                x=start+(j+.5)*run/n;h=(j+1)*level/n
                m.box(name,(x,d,h/2),(run/n,2.15,h),'Stone',.018)
                m.box(name,(x+run/n*.38,d,h+.007),(.035,2.08,.015),'Bronze',.004)
            # Smooth ramp proxy coincides with nosings, avoiding controller jitter.
            for j in range(n):
                a=start+run*j/n;b=start+run*(j+1)/n;za=level*j/n;zb=level*(j+1)/n
                v=[(a,d-1.075,za),(a,d+1.075,za),(b,d+1.075,zb),(b,d-1.075,zb)]
                c.add(name+'_StairRamp',v,[(0,1,2,3),(3,2,1,0)],'Stone')
            for side in (-1,1):
                for j in range(n):
                    a=start+run*j/n;b=start+run*(j+1)/n;za=level*j/n;zb=level*(j+1)/n
                    m.beam(name,(a,d+side*1.08,za+1),(b,d+side*1.08,zb+1),.052,'Teal')
                    c.add(name+'_StairSide',[(a,d+side*1.11,za),(b,d+side*1.11,zb),(b,d+side*1.11,zb+1.2),(a,d+side*1.11,za+1.2)],[(0,1,2,3),(3,2,1,0)],'Stone')
                for k in range(9):
                    x=start+run*k/8;z=level*k/8;m.beam(name,(x,d+side*1.08,z),(x,d+side*1.08,z+1),.036,'Teal')
            # Bridge at landing crosses back to upper concourse; railing has a gap here.
            m.box(name,(start+run+.7,depth+1.5,level-.12),(1.4,4.65,.24),'Paving',.025)
            c.box(name+'_Landing',(start+run+.7,depth+1.5,level-.12),(1.4,4.65,.24),bevel=0)
            stairs=[mapping((start-.7,d,.08))]+[mapping((start+run*t,d,level*t+.08)) for t in np.linspace(0,1,25)]+[mapping((start+run+.7,d,level+.08))]
            route.append({'entry':list(mapping((0,depth+7,.08))),'inside':list(mapping((0,-4.8 if football else -1,.08))),'stair_bottom':list(stairs[0]),'stair_top':list(stairs[-1]),'stair_path':[float(n) for p in stairs for n in p],'concourse':list(mapping((start+run+.7,depth-1,level+.08)))})
    # Seating access uses existing aisle positions, but collision is smooth.
    for i in range(count):
        s=i*baylen;m.mapping=c.mapping=lambda p,s=s:path(s+p[0],p[1],p[2]);name='Aisle_%02d'%i
        if football:
            for j in range(17):
                d=-4.9+(j+.5)*6.3/17;h=(j+1)*3.3/17;m.box(name,(0,d,h/2),(1.5,6.3/17,h),'Stone',.012)
            flights=[(-4.9,1.4,0,3.3),(1.4,10.4,3.3,8.25)]
        else:
            flights=[] if i in cardinal else [(.0,4.15,0,2.25)]
            flights.append((3.3,6.6,4.42,6.10))
        for a,b,z0,z1 in flights:
            v=[(-.68,a,z0),(.68,a,z0),(.68,b,z1),(-.68,b,z1)];c.add(name,v,[(0,1,2,3),(3,2,1,0)],'Stone')
    # B1 perimeter screens stand on real piers; central sky remains unobstructed.
    scoreboards=[]
    if not football:
        for i in cardinal:
            mid=mids[i]+baylen*.5;m.mapping=c.mapping=lambda p,mid=mid:path(mid+p[0],p[1],p[2]);name='PerimeterScreen_%02d'%i
            for x in (-1.95,1.95):
                m.box(name,(x,8.1,7.45),(.44,.70,2.8),'Stone',.06);m.box(name,(x,8.1,8.95),(.68,.94,.2),'Stone',.035)
            m.box(name,(0,8.1,8.15),(3.9,.40,2.0),'Teal',.10);m.box(name,(0,7.87,8.15),(3.55,.07,1.7),'Screen',.025)
            for x in (-1.78,1.78):m.box(name,(x,7.80,8.15),(.10,.08,1.80),'Bronze',.02)
            p=path(mid,7.80,8.15);q=path(mid,6.8,8.15)
            scoreboards.append({'position':[-p[0],p[2],-p[1]],'inward':[-q[0]+p[0],0,-q[1]+p[1]]})
        # Mounted light posts around the back rim, no spanning truss.
        for i in range(0,count,3):
            m.mapping=lambda p,i=i:path(mids[i]+p[0],p[1],p[2]);name='RimLight_%02d'%i
            m.beam(name,(0,8,6.1),(0,8,10.1),.07,'Teal');m.box(name,(0,7.90,10),(1.0,.32,.45),'Teal',.06)
            for x in (-.3,0,.3):m.box(name,(x,7.71,10),(.23,.04,.28),'Lamp',.025)
    m.finish(sport,mats);c.finish(sport,mats,True)
    s=((cardinal[0]+1)%count)*baylen
    if football:
        aisle_paths=[[path(s,d,z+.08) for d,z in ((12.7,8.25),(10.4,8.25),(1.4,3.3),(-4.9,0))]]
    else:
        aisle_paths=[[path(s+.9,8.4,6.18),path(s+.9,7.1,6.18),path(s,7.1,6.18),path(s,6.6,6.18),path(s,3.3,4.50)],
                     [path(s,d,z+.08) for d,z in ((-.8,0),(0,0),(4.15,2.25))]]
    data={'sport':sport,'routes':route,'aisles':[{'points':[float(v) for p in points for v in p]} for points in aisle_paths],'scoreboards':scoreboards,'main_entry_bay':cardinal[0],'entry_bays':cardinal,'concourse_height':level,'facade_depth':depth}
    (ART/(sport+'-layout.json')).write_text(json.dumps(data,indent=2))
    (SOURCE/(sport+'-layout.json')).write_text(json.dumps(data,indent=2))
    print('COASTAL_ARCHITECTURE_COMPLETE',sport)
