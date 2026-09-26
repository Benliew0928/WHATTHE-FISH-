"""G2 Limestone Cove Links / L2 Limestone Garden Lagoon, approved 27 Sep 2026.

Blender --background --factory-startup --python-exit-code 1 --python this_file
Use --reuse-kit for layout-only iteration. No approved F1/B1 files are written.
"""
import bpy,bmesh,sys,json,math,random
from pathlib import Path
import numpy as np
from mathutils import Vector
sys.path.insert(0,str(Path(__file__).resolve().parent))
from refined_island_kit import *

random.seed(927);scene=bpy.context.scene
if '--reuse-kit' in sys.argv:
    bpy.ops.wm.open_mainfile(filepath=str(SOURCE/'IslandAssetKit.blend'));scene=bpy.context.scene
    for name in ('Turf','Sand','Soil'):
        if 'RI_'+name not in bpy.data.materials:
            m=bpy.data.materials.new('RI_'+name);m.use_nodes=True;m.use_fake_user=True
            tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(ART/'Shared'/('RI_'+name+'_BaseColor.png')))
            m.node_tree.links.new(tex.outputs['Color'],m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
    mats={name:bpy.data.materials['RI_'+name] for name in ('Stone','Paving','Timber','Bark','Leaf','LeafLight','Turf','Sand','Soil','Teal','Bronze','Rope','Flower','Gold')};mats['Accent']=bpy.data.materials['LG_Accent']
    kit=json.loads((ART/'Shared'/'kit.json').read_text())['modules'];library={s['name']:(bpy.data.collections[s['name']],[o for o in bpy.data.collections[s['name']].objects if o.name.startswith('LOD0')]) for s in kit}
    if '--rebuild-kit' in sys.argv:
        for m in mats.values():m.use_fake_user=True
        for o in list(bpy.data.objects):bpy.data.objects.remove(o,do_unlink=True)
        for c in list(bpy.data.collections):bpy.data.collections.remove(c)
        kit,library=build_kit(mats)
else:
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    for c in list(bpy.data.collections):bpy.data.collections.remove(c)
    mats=make_materials();kit,library=build_kit(mats)
for col,_ in library.values():col.hide_render=True;col.hide_viewport=True

def golf_radius(a):return 192+12*np.sin(3*a+.5)+7*np.sin(5*a-1)+4*np.cos(9*a)-9*np.exp(-((np.arctan2(np.sin(a-.45),np.cos(a-.45)))/.28)**2)
def fairway(y):return 24*np.sin((y+135)/55)-5
def fair_width(y):return 24+5*np.cos(y/38)+6*np.exp(-((y+110)/40)**2)
BUNKERS=[(-42,-112,18,11,.3),(43,-56,24,14,-.3),(-38,16,21,13,.2),(38,74,18,12,.5),(-37,127,15,10,-.5)]
def bunker_dist(x,y,b):
    cx,cy,rx,ry,a=b;u=(x-cx)*math.cos(a)+(y-cy)*math.sin(a);v=-(x-cx)*math.sin(a)+(y-cy)*math.cos(a)
    return np.sqrt((u/rx)**2+(v/ry)**2)*(1+.08*np.sin(np.arctan2(v/ry,u/rx)*3+.6))
def golf_height(x,y):
    a=np.arctan2(y,x);q=np.sqrt(x*x+y*y)/golf_radius(a)
    base=3.6+1.4*np.sin(y/70)+.75*np.cos(x/42)*np.sin(y/36)
    base+=6.8*np.exp(-((x-4)/57)**2-((y-131)/49)**2)+2*np.exp(-((x+24)/50)**2-((y+144)/38)**2)
    base+=1.9*np.sin(x/26+.3)*np.sin(y/31)*smooth(35,70,np.abs(x-fairway(y)))
    for b in BUNKERS:base-=1.15*(1-smooth(.55,1.18,bunker_dist(x,y,b)))
    # Level pavilion plaza and practice green are integrated into the same surface.
    d=np.sqrt(((x+65)/10)**2+((y+145)/8)**2);base=base*smooth(1,2,d)+4.2*(1-smooth(1,2,d))
    result=base*(1-smooth(.82,.98,q))+.18*smooth(.82,.98,q)-.8*smooth(.98,1.035,q)
    result+=6.5*np.exp(-((x-151)/20)**2-((y-73)/16)**2)*(1-smooth(.94,1.01,q))
    return result
def fish_radius(a):return 55+2.4*np.sin(3*a+.4)+1.5*np.cos(7*a)
def fish_height(x,y):
    r=np.sqrt(x*x+y*y);a=np.arctan2(x,y)
    rise=1.45+1.8*np.exp(-((r-46)/5)**2)*(.65+.35*np.sin(a*5+.8)**2)
    z=1.2+(rise-1.2)*smooth(38,41,r)
    z=z*(1-smooth(fish_radius(a)-5,fish_radius(a)+.7,r))-.6*smooth(fish_radius(a)-1,fish_radius(a)+.7,r)
    plaza=np.sqrt(((x-41)/6.2)**2+((y+14)/6.3)**2);blend=1-smooth(.9,1.4,plaza);z=z*(1-blend)+1.2*blend
    return z*smooth(26,28,r)-.6*(1-smooth(26,28,r))

def export_world(name,m,materials,folder):
    col=bpy.data.collections.new(name);scene.collection.children.link(col);obs=m.finish(name,materials,col)
    bpy.ops.object.select_all(action='DESELECT')
    for o in obs:o.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(folder/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False)
    return col,obs

def make_world(sport):
    golf=sport=='Golf';code='G2' if golf else 'L2';folder=ART/sport;folder.mkdir(parents=True,exist_ok=True)
    height=golf_height if golf else fish_height;extent=220 if golf else 70;sea=-.42 if golf else -.08
    rng=random.Random(927 if golf else 928);instances=[];routes=[];views=[];boundaries=[];specials=[];signs=[];grass_mesh=Meshes()
    assembly=bpy.data.collections.new(sport+'_Assembly');scene.collection.children.link(assembly)
    def place(asset,id,x,y,z=None,yaw=0,scale=1,accent=''):
        z=float(height(x,y)) if z is None else z
        instances.append(dict(module=asset,id=id,position=point(x,y,z),yaw=yaw,scale=scale,accent=accent))
        for source in library[asset][1]:
            if asset.startswith('Grass'):
                # Bake each blade's root onto the analytic terrain, then batch by spatial cell.
                # This prevents the eight-metre source patches floating over rolling ground.
                a=-math.radians(yaw);coords=np.array([tuple(vert.co) for vert in source.data.vertices]);px=x+scale*(coords[:,0]*math.cos(a)-coords[:,1]*math.sin(a));py=y+scale*(coords[:,0]*math.sin(a)+coords[:,1]*math.cos(a))
                v=np.stack((px,py,height(px,py)+coords[:,2]*scale-.01),axis=1)
                faces=[tuple(p.vertices) for p in source.data.polygons]
                if golf:
                    # Trim complete blades out of bunker sand, including patch edges.
                    allowed=np.ones(len(v),dtype=bool)
                    for bunker in BUNKERS:allowed &= bunker_dist(px,py,bunker)>1.02
                    faces=[f for f in faces if all(allowed[i] for i in f)]
                mat=source.data.materials[0].name.replace('RI_','')
                cell='Cell_%02d_%02d'%(int((x+extent)/40),int((y+extent)/40));grass_mesh.add(cell,v,faces,mat,True)
                continue
            o=source.copy();o.data=source.data;assembly.objects.link(o);o.name=id+'__'+source.name;o.location=(x,y,z);o.rotation_euler=(0,0,-math.radians(yaw));o.scale=(scale,)*3;o.hide_render=False
    def waypoint(x,y,z=None):return point(x,y,float(height(x,y))+.12 if z is None else z)
    def route(name,xy):routes.append(dict(name=name,points=[waypoint(*p) for p in xy]))
    def view(name,pos,look,fov=65):views.append(dict(name=name,position=point(*pos),target=point(*look),fov=fov))
    # Four-kilopixel macro colour/control maps retain readable course and path masks.
    n=4096;yy,xx=np.mgrid[:n,:n].astype(np.float32)/(n-1)*(extent*2)-extent;r=np.sqrt(xx*xx+yy*yy)
    a=np.arctan2(yy,xx) if golf else np.arctan2(xx,yy);q=r/(golf_radius(a) if golf else fish_radius(a))
    pixels=np.broadcast_to(rgb('679544' if golf else '759449'),(n,n,3)).copy();sand=np.zeros((n,n),np.float32);rock=np.zeros_like(sand)
    def paint(color,mask):
        nonlocal pixels
        pixels*=1-mask[:,:,None];pixels+=rgb(color)[None,None,:]*mask[:,:,None]
    if golf:
        fair=(1-smooth(fair_width(yy)-.6,fair_width(yy)+.8,np.abs(xx-fairway(yy))))*smooth(-164,-146,yy)*(1-smooth(133,151,yy))
        paint('86B249',fair);pixels+=fair[:,:,None]*(np.sin((yy+xx*.18)*.3)>0)[:,:,None]*np.array([.015,.025,.009],np.float32)
        for x,y,rx,ry in ((4,133,29,22),(-13,-146,18,12),(-87,-117,19,14)):
            d=np.sqrt(((xx-x)/rx)**2+((yy-y)/ry)**2);paint('729D42',1-smooth(.98,1.08,d));paint('9DBC58',1-smooth(.93,.99,d))
        for b in BUNKERS:
            d=bunker_dist(xx,yy,b);paint('467737',1-smooth(.97,1.05,d));mask=1-smooth(.89,.98,d);paint('E8CF9D',mask);sand=np.maximum(sand,mask)
        coast=smooth(.88,.94,q);paint('E7D0A4',coast);sand=np.maximum(sand,coast)
        pathy=yy-6*np.exp(-((xx+64)/18)**4)*(yy<-100);pathd=np.sqrt((xx/145)**2+(pathy/156)**2);path=1-smooth(.009,.018,np.abs(pathd-1));paint('D6C29A',path);sand=np.maximum(sand,path)
    else:
        coast=np.maximum(1-smooth(31,33,r),smooth(q*.0+.88,.96,q));paint('E9D3A8',coast);sand=np.maximum(sand,coast)
        path=1-smooth(1.7,2.25,np.abs(r-36));paint('D7C29B',path);sand=np.maximum(sand,path)
        for bearing in (180,252,324,36,108):
            angle=math.radians(bearing);side=np.abs(xx*math.cos(angle)-yy*math.sin(angle));along=xx*math.sin(angle)+yy*math.cos(angle)
            approach=(1-smooth(3,3.4,side))*smooth(27,28,along)*(1-smooth(35,38,along));paint('DDC8A1',approach);sand=np.maximum(sand,approach)
    pixels+=((.012*np.sin(xx*.24)*np.sin(yy*.31)+.007*np.cos(xx*.77+yy*.19))[:,:,None])
    macro=save_image(sport+'_TerrainColor',np.clip(pixels,0,1),folder);control=save_image(sport+'_TerrainControl',np.stack((sand,rock,np.zeros_like(sand)),2),folder,True)
    del pixels,xx,yy,q,r,a,sand,rock,path
    material=bpy.data.materials.new('RI_'+sport+'_Terrain');material.use_nodes=True;tex=material.node_tree.nodes.new('ShaderNodeTexImage');tex.image=macro
    material.node_tree.links.new(tex.outputs['Color'],material.node_tree.nodes.get('Principled BSDF').inputs['Base Color']);material.diffuse_color=(.3,.5,.2,1);worldmats=dict(mats,Terrain=material)
    terrain=Meshes()
    if golf:
        rings=144;segs=384
        for sector in range(16):
            v=[];uv=[]
            for j in range(rings+1):
                for i in range(25):
                    angle=2*PI*(sector*24+i)/segs;rr=float(golf_radius(angle))*1.035*j/rings;x=rr*math.cos(angle);y=rr*math.sin(angle)
                    v.append((x,y,float(height(x,y))));uv.append(((x+extent)/(extent*2),(y+extent)/(extent*2)))
            faces=[]
            for j in range(rings):
                for i in range(24):
                    k=j*25+i
                    if j:faces.append((k,k+26,k+1))
                    faces.append((k,k+25,k+26))
            terrain.add('Sector_%02d'%sector,v,faces,'Terrain',True,uv)
    else:
        for sector in range(10):
            v=[];uv=[];rows=48;cols=36
            for j in range(rows+1):
                for i in range(cols+1):
                    u=(sector+i/cols)/10;angle=.13+(2*PI-.26)*u
                    for _ in range(5):
                        rr=26+(float(fish_radius(angle))+.8-26)*j/rows;cut=math.asin(4.5/rr);angle=cut+(2*PI-2*cut)*u
                    x=rr*math.sin(angle);y=rr*math.cos(angle);v.append((x,y,float(height(x,y))));uv.append(((x+extent)/(extent*2),(y+extent)/(extent*2)))
            faces=[(j*(cols+1)+i,j*(cols+1)+i+1,(j+1)*(cols+1)+i+1,(j+1)*(cols+1)+i) for j in range(rows) for i in range(cols)]
            terrain.add('Sector_%02d'%sector,v,faces,'Terrain',True,uv)
        for side in (-1,1):
            for j in range(30):
                y=26+j;x=side*4.5
                terrain.add('InletBank',[(x,y,float(height(x,y))),(x,y+1,float(height(x,y+1))),(x,y+1,-2),(x,y,-2)],[(0,1,2,3)],'Stone')
    terrain_col,terrain_objects=export_world('Terrain',terrain,worldmats,folder)
    structures=Meshes();water=Meshes()
    def sign(x,y,title,caption,width=3):
        z=float(height(x,y));structures.box('Wayfinding',(x,y,z+.86),(.20,.24,1.72),'Timber',.025)
        structures.box('Wayfinding',(x,y,z+1.74),(width,.18,.83),'Timber',.025)
        structures.box('Wayfinding',(x,y-.108,z+1.74),(width-.12,.045,.69),'Teal',.018)
        for side in (-1,1):
            structures.box('Wayfinding',(x+side*(width/2-.13),y-.14,z+1.74),(.09,.04,.53),'Bronze',.015)
            for dz in (-.2,.2):bolt(structures,'Wayfinding',(x+side*(width/2-.13),y-.17,z+1.74+dz),'Y',.03)
        signs.append(dict(position=point(x,y-.15,z+1.74),yaw=0,width=width-.4,title=title,caption=caption))
    # Paved ribbons follow terrain exactly; thin beveled curb blocks sit beside them.
    path_index=0
    def path(name,xy,width=3.6,curbs=True):
        nonlocal path_index
        path_index+=1;offset=.035+path_index*.015
        assert offset<=.18, 'Paving must stay within 18cm of terrain; combine adjacent segments before adding a ribbon'
        dense=[xy[0]]
        for start,end in zip(xy,xy[1:]):
            steps=max(1,math.ceil(math.hypot(end[0]-start[0],end[1]-start[1])/.65))
            dense.extend([(start[0]+(end[0]-start[0])*j/steps,start[1]+(end[1]-start[1])*j/steps) for j in range(1,steps+1)])
        xy=dense;left=[];right=[]
        for j,p in enumerate(xy):
            before=Vector(xy[j])-Vector(xy[max(0,j-1)]);after=Vector(xy[min(len(xy)-1,j+1)])-Vector(xy[j])
            if before.length<1e-5:before=after.copy()
            if after.length<1e-5:after=before.copy()
            before.normalize();after.normalize();normal=Vector((-before.y,before.x))+Vector((-after.y,after.x));normal.normalize()
            miter=normal*(width/2/max(.55,normal.dot(Vector((-after.y,after.x)))))
            left.append(Vector(p)+miter);right.append(Vector(p)-miter)
        for i,(p0,p1) in enumerate(zip(xy,xy[1:])):
            d=Vector((p1[0]-p0[0],p1[1]-p0[1]));d.normalize();side=Vector((-d.y,d.x))
            points=[left[i],left[i+1],right[i+1],right[i]]
            v=[(p.x,p.y,float(height(p.x,p.y))+offset) for p in points];structures.add(name,v,[(0,3,2,1)],'Paving')
            if curbs and i%4==0:
                for sign in (-1,1):
                    p=Vector(p0)+side*(width/2+.13)*sign;structures.rock(name,(p.x,p.y,float(height(p.x,p.y))+.09),(.28,.22,.13),i,'Stone',2)
    if golf:
        loop=[(145*math.sin(a),156*math.cos(a)) for a in np.linspace(0,2*PI,241)]
        loop=[(x,y+6*math.exp(-((x+64)/18)**4) if y<-100 else y) for x,y in loop];path('CoastalPath',loop,3.8)
        link=[(-65,-145),(-60,-145),(-50,-145),(-40,-145),(-30,-145),(-13,-145)];path('WelcomeApproach',link,4)
        practice=[(-65,-145),(-59,-145),(-56,-139),(-65,-134),(-75,-130),(-87,-117)];path('PracticeApproach',practice,3.4)
        place('GolfPavilion','G2_WelcomePavilion',-65,-145,4.24)
        sign(-69,-151,'LIMESTONE COVE','WELCOME  /  LINKS & COAST',3.7)
        sign(-81,-129,'PRACTICE GREEN','KEEP THE WALKWAY CLEAR',2.8)
        sign(128,68,'SPRING GARDEN','COASTAL VIEWPOINT',2.7)
        for i in range(90):
            a=2*PI*(i+.3)/90;r=float(golf_radius(a))*rng.uniform(.78,.88);x=r*math.cos(a);y=r*math.sin(a)
            if y<-123 and x<0:continue
            if x>140 and 58<y<107:continue
            if abs(math.sqrt((x/145)**2+(y/156)**2)-1)<.075:continue
            place('Rock_'+str(i%4),'CoastalRock_%03d'%i,x,y,float(height(x,y))-.3,rng.random()*360,rng.uniform(1.1,2.7))
        # Distinct planted limestone terraces beside the cove and waterfall.
        for side in (-1,1):
            for j in range(14):
                if side==1 and 5<=j<=12:continue
                y=28+j*6.3;rockscale=2+(j%4)*.8;path_x=145*math.sqrt(max(0,1-(y/156)**2))
                x=side*max(132+(j%3)*10,path_x+rockscale*2.5+4);z=float(height(x,y))
                place('Rock_'+str(j%4),'TerraceRock_%s_%02d'%(side,j),x,y,z-.8,j*37,2+(j%4)*.8)
                for k in range(2):place('Plant_'+str((j+k)%4),'TerracePlant_%s_%d_%d'%(side,j,k),x+(k-.5)*2,y,z+7+(j%4)*1.6,j*25,1.7)
        # Offshore stone arch, grounded foundations rather than floating blocks.
        for j in range(17):
            a=j*PI/16;structures.rock('SeaArch',(-257+24*math.cos(a),83,-1+34*math.sin(a)),(7.7,10,6.3),j,sub=4)
        for j in range(4):structures.rock('SeaArch',(-286+j*18,84,-.9),(13,15,3),j,sub=3)
        # Three connected spring terraces with thick basins and layered water ribbons.
        cascade=[(151,89,17),(163,89,16.8),(166,89,11),(176,89,10.7),(180,89,4.7),(189,89,4.5),(193,89,sea+.05)]
        for k,(x,y,z) in enumerate(cascade[:-1:2]):
            ground=float(height(x,y))-.8;top=z-2;mid=(ground+top)/2
            structures.rock('CascadeFoundation',(x,y,mid),(10.5,9,(top-ground)/2+.2),k+71,sub=4)
            # The full bowl floor must remain below the water, even at the
            # highest sculpted vertex; the separately modeled rim forms its lip.
            structures.rock('CascadeBasin',(x,y,z-2.3),(10,9,1.65),k,sub=4)
            water.add('SpringPool',[(x+math.cos(a)*8,y+math.sin(a)*6,z+.04) for a in np.linspace(0,2*PI,49)[:-1]],[tuple(range(48))],'Teal')
            for j in range(17):
                a=j*2*PI/17
                if math.cos(a)>.7:continue
                structures.rock('SpringRim',(x+8*math.cos(a),y+6*math.sin(a),z),(1.5,1.1,.65),j,sub=3)
        for j,(a,b) in enumerate(zip(cascade,cascade[1:])):
            for k in range(12):
                y0=-3.5+k*7/12;y1=-3.5+(k+1)*7/12
                water.add('Cascade',[(a[0],a[1]+y0,a[2]),(b[0],b[1]+y0,b[2]),(b[0],b[1]+y1,b[2]),(a[0],a[1]+y1,a[2])],[(0,1,2,3)],'Teal',True,[(k/12,j),(k/12,j+1),((k+1)/12,j+1),((k+1)/12,j)])
        for i in range(70):
            y=rng.uniform(-150,155);side=1 if i%2 else -1;x=float(fairway(y))+side*(float(fair_width(y))+rng.uniform(18,55))
            if any(float(bunker_dist(x,y,b))<1.5 for b in BUNKERS) or (x+65)**2+(y+145)**2<160:continue
            if abs(math.sqrt((x/145)**2+(y/156)**2)-1)<.075:continue
            place('Palm_'+str(i%4),'Palm_%03d'%i,x,y,yaw=i*71,scale=rng.uniform(1.0,1.65))
            for k in range(4):
                px=x+math.cos(k*1.57)*2;py=y+math.sin(k*1.57)*2;place('Plant_'+str(k),'PalmGarden_%d_%d'%(i,k),px,py,yaw=i*43,scale=1.3)
        for i in range(280):
            y=rng.uniform(-170,168);x=rng.uniform(-165,165)
            if abs(x-float(fairway(y)))<float(fair_width(y))+7 or math.sqrt((x/145)**2+(y/156)**2)>1.13 or abs(math.sqrt((x/145)**2+(y/156)**2)-1)<.065:continue
            if (x+65)**2+(y+145)**2<300 or (x+87)**2+(y+117)**2<650:continue
            place('Grass_'+str(i%3),'Rough_%03d'%i,x,y,yaw=i*31,scale=rng.uniform(.8,1.2))
        # Modeled pins and tee markers retain a readable golf course.
        for x,y in ((4,134),(-87,-117)):
            z=float(height(x,y));structures.beam('CourseProps',(x,y,z+.01),(x,y,z+.045),.18,'Bronze',24);structures.beam('CourseProps',(x,y,z),(x,y,z+3),.05,'Timber',12)
            flag=[(x,y,z+3),(x+1.1,y+.1,z+2.8),(x+1,y+.1,z+2.3),(x,y,z+2.25)]
            flag += [(a,b+.016,c) for a,b,c in flag]
            structures.add('CourseProps',flag,[(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)],'Teal')
        for x in (-25,-1):structures.rock('CourseProps',(x,-151,float(height(x,-151))+.18),(.45,.30,.22),0,'Teal',3)
        spawns=[waypoint(-13+(i%5-2)*2,-146+(i//5)*3) for i in range(10)];safe=spawns[0]
        route('Main fairway',[(float(fairway(y)),y) for y in range(-146,135,10)]+[(2.5,134)])
        route('Coastal promenade',loop);route('Pavilion approach',link);route('Practice green',practice)
        cascade_walk=[(130,70),(138,72),(146,74),(156,73)];path('CascadeApproach',cascade_walk,3.6);route('Cascade viewpoint',cascade_walk)
        for i,b in enumerate(BUNKERS):route('Bunker %d crossing'%(i+1),[(b[0]-b[2]*1.2,b[1]),(b[0],b[1]),(b[0]+b[2]*1.2,b[1])])
        for i in range(256):
            a=i*2*PI/256;b=(i+1)*2*PI/256;p=[]
            for angle in (a,b):
                r=float(golf_radius(angle))-.8;x=r*math.cos(angle);y=r*math.sin(angle);p.append(point(x,y,float(height(x,y))+2))
            boundaries.append(dict(a=p[0],b=p[1]))
        overview=(290,-350,250);focus=(0,5,4)
        view('01_Overview',overview,focus,48);view('02_Rear_Cove',(-270,320,155),(5,35,5),50)
        view('03_Fairway',(-12,-140,float(height(-12,-140))+1.65),(0,40,9))
        palm_ref=min((p for p in instances if p['module'].startswith('Palm')),key=lambda p:(p['position']['x']-66)**2+(p['position']['z']+98)**2)['position'];px,py=palm_ref['x'],palm_ref['z']
        view('04_Palm_Garden',(px+2.2,py-2.0,float(height(px+2.2,py-2))+1.65),(px,py,palm_ref['y']+2.2),72)
        view('05_Turf_And_Bunker',(-38,-127,float(height(-38,-127))+1.65),(-42,-112,3),68)
        view('06_Pavilion',(-56,-155,5.9),(-64,-145,6.2),65)
        view('07_Pavilion_Joinery',(-62.2,-149,5.9),(-60.5,-147.9,6.6),68)
        view('08_Practice_Green',(-90,-130,float(height(-90,-130))+1.65),(-87,-110,4.5))
        view('09_Cascade',(152,62,float(height(152,62))+1.65),(168,89,11),68)
        view('10_Shoreline',(163,-60,float(height(163,-60))+1.65),(196,-48,0))
        view('11_Rock_Detail',(148,28,float(height(148,28))+1.65),(154,28,float(height(148,28))+1.7),70)
        view('12_Raised_Green',(8,123,float(height(8,123))+1.65),(4,151,10),65)
    else:
        for side in (-1,1):
            for j in range(30):
                y=26.5+j;top=float(height(side*4.7,y))
                for k in range(max(1,math.ceil((top+.8)/.35))):
                    z=min(top-.18,-.62+k*.35)
                    structures.box('InletRetainingStone',(side*4.7,y+(k%2)*.12,z),(.46,.965,.32),'Stone',.045)
        loop=[]
        for angle in np.linspace(0,2*PI,145):
            x=36*math.sin(angle);y=36*math.cos(angle)
            if y>34 and abs(x)<7:y=36
            loop.append((x,y))
        # Leave the inlet opening to the separate bridge.
        circuit_strip=[]
        for a,b in zip(loop,loop[1:]):
            if a[1]>33 and (abs(a[0])<7 or abs(b[0])<7):
                if len(circuit_strip)>1:path('LagoonPath',circuit_strip,3.6,True)
                circuit_strip=[]
                continue
            if not circuit_strip:circuit_strip.append(a)
            circuit_strip.append(b)
        if len(circuit_strip)>1:path('LagoonPath',circuit_strip,3.6,True)
        place('InletBridge','NorthInletBridge',0,36,1.34)
        bearings=[180,252,324,36,108];colors=['24B6AE','F78685','FFD258','BC90DF','4A9FDF'];spawns=[];stands=[]
        for i,(bearing,color) in enumerate(zip(bearings,colors)):
            a=math.radians(bearing);x=28*math.sin(a);y=28*math.cos(a);place('PlayerStand','Stand_%02d'%(i+1),x,y,1.2,bearing+180,1,color)
            spawns.append(waypoint(32*math.sin(a),32*math.cos(a),1.35));stands.append(waypoint(22.5*math.sin(a),22.5*math.cos(a),1.35))
            approach=[(r*math.sin(a),r*math.cos(a)) for r in (36,34,32,30,28)];path('StandApproach',approach,5.4)
            route('Station %d approach'%(i+1),[(36*math.sin(a),36*math.cos(a),1.35),(22.5*math.sin(a),22.5*math.cos(a),1.35)])
        place('FishingShelter','L2_FishingShelter',41,-13,1.22,0)
        sign(45,-18,'LIMESTONE LAGOON','FIVE STATIONS  /  SHARED SHORE',3.5)
        shelter_route=[(32,-17),(36,-19),(41,-19),(41,-13)]
        path('ShelterApproach',shelter_route,3.6);route('Shelter approach',shelter_route)
        for i in range(44):
            a=i*2*PI/44;r=float(fish_radius(a))-rng.uniform(1.8,4);x=r*math.sin(a);y=r*math.cos(a)
            if y>0 and abs(x)<9:continue
            place('Rock_'+str(i%4),'CoastalRock_%02d'%i,x,y,float(height(x,y))-.6,i*57,rng.uniform(.65,1.35))
        for side in (-1,1):
            for j in range(8):
                x=side*(17+(j%3)*8);y=41-(j//3)*3;scale=1.5+(j%3)*.6
                radius=math.sqrt(x*x+y*y);clearance=max(1,(39.5+scale*2.5)/radius);x*=clearance;y*=clearance;z=float(height(x,y))
                if x*x+y*y>53**2:continue
                place('Rock_'+str(j%4),'LimestoneTerrace_%s_%d'%(side,j),x,y,z-.5,j*51,scale)
                place('Plant_'+str(j%4),'TerraceGarden_%s_%d'%(side,j),x,y,z+scale*3.25,j*51,1.2)
        for i in range(35):
            a=(i+.5)*2*PI/35;r=rng.uniform(42,48);x=r*math.sin(a);y=r*math.cos(a)
            if y>0 and abs(x)<10 or (x-41)**2+(y+13)**2<85:continue
            place('Palm_'+str(i%4),'Palm_%02d'%i,x,y,yaw=i*73,scale=rng.uniform(.85,1.1))
            for k in range(5):
                px=x+math.cos(k*2.4)*2;py=y+math.sin(k*2.4)*2;place('Plant_'+str(k%4),'PalmGarden_%d_%d'%(i,k),px,py,yaw=k*61,scale=rng.uniform(.85,1.3))
        for i in range(95):
            a=i*2.4;r=rng.uniform(41,48);x=r*math.sin(a);y=r*math.cos(a)
            if y>0 and abs(x)<10 or (x-41)**2+(y+13)**2<110:continue
            place('Grass_'+str(i%3),'Meadow_%03d'%i,x,y,yaw=i*47,scale=.7)
        for i in range(25):
            a=i*2.4;r=31;x=r*math.sin(a);y=r*math.cos(a)
            if y>0 and abs(x)<9 or min(abs(math.atan2(math.sin(a-math.radians(b)),math.cos(a-math.radians(b)))) for b in bearings)<.15:continue
            place('Plant_'+str(i%4),'InnerGarden_%d'%i,x,y,yaw=i*35)
        # Preserve the exact five rectangular pier notches in water containment.
        pts=[];last=math.radians(9.5);half=3;radius=28.1;opening=math.asin(half/radius)
        def arc(start,end):
            for a in np.linspace(start,end,max(2,math.ceil((end-start)*180/PI)*2)):pts.append((radius*math.sin(a),radius*math.cos(a)))
        for bearing in sorted(bearings):
            a=math.radians(bearing);arc(last,a-opening);n=Vector((math.sin(a),math.cos(a)));t=Vector((math.cos(a),-math.sin(a)))
            pts.extend([tuple(n*21-t*half),tuple(n*21+t*half),tuple(n*math.sqrt(radius*radius-half*half)+t*half)]);last=a+opening
        arc(last,math.radians(350.5))
        def boundary(a,b):boundaries.append(dict(a=point(a[0],a[1],2),b=point(b[0],b[1],2)))
        for a,b in zip(pts,pts[1:]):boundary(a,b)
        outer=[]
        for a in np.linspace(.086,2*PI-.086,257):
            r=float(fish_radius(a))-1.4;outer.append((r*math.sin(a),r*math.cos(a)))
        for a,b in zip(outer,outer[1:]):boundary(a,b)
        for side in (-1,1):
            for a,b in ((27.7,33.8),(38.2,54)):boundary((side*4.9,a),(side*4.9,b))
        boundary((-4.9,33.9),(4.9,33.9));boundary((-4.9,38.1),(4.9,38.1))
        safe=spawns[0];route('Lagoon circuit',[(x,y,1.35) for x,y in loop]);route('Inlet bridge',[(-8,36,1.35),(8,36,1.35)])
        view('01_Overview',(0,-116,97),(0,3,0),48);view('02_Rear_Inlet',(-89,87,57),(0,6,2),50)
        view('03_Station_Entrance',(4,-33,2.95),(0,-22,2.3),65)
        view('04_Deck_Joinery',(3.7,-26.5,2.9),(2.8,-25,1.9),67)
        palm_ref=min((p for p in instances if p['module'].startswith('Palm')),key=lambda p:(p['position']['x']-45)**2+(p['position']['z']+8)**2)['position'];px,py=palm_ref['x'],palm_ref['z']
        view('05_Palm_Garden',(px-2.2,py-2.0,float(height(px-2.2,py-2))+1.65),(px,py,palm_ref['y']+2.0),72)
        view('06_Lagoon_Path',(31,-18,2.85),(35,-3,2),65)
        view('07_Shelter',(37,-18,2.85),(41,-13,3),65)
        view('08_Bridge',(-10,30,3.1),(0,36,2.5),65)
        view('09_Limestone_Terraces',(12,31,2.85),(22,41,5),65)
        view('10_Shoreline',(-48,-7,2.1),(-62,0,0),65)
        view('11_Water_Detail',(0,-22,2.85),(3,-15,.1),65)
        view('12_Lagoon_Sky',(-17,-25,2.9),(8,20,7),65)
    structures_col,structure_objects=export_world('Structures',structures,worldmats,folder)
    grass_col=bpy.data.collections.new(sport+'_ConformedGrass');scene.collection.children.link(grass_col);grass_objects=grass_mesh.finish('LOD0',mats,grass_col)
    for obj in list(grass_objects):
        for level,ratio in ((1,.40),(2,.13)):
            clone=obj.copy();clone.data=obj.data.copy();clone.name=obj.name.replace('LOD0','LOD'+str(level));grass_col.objects.link(clone)
            bpy.context.view_layer.objects.active=clone;mod=clone.modifiers.new('Grass distance density','DECIMATE');mod.ratio=ratio;bpy.ops.object.modifier_apply(modifier=mod.name);clone.hide_render=True;grass_objects.append(clone)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in grass_objects:obj.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(folder/'Grass.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False)
    water_col,water_objects=export_world('Cascade',water,worldmats,folder) if golf else (None,[])
    # Shore-distance/depth map used by the same water shader across the entire sea.
    water_extent=500 if golf else 160;n=2048;y,x=np.mgrid[:n,:n].astype(np.float32)/(n-1)*water_extent*2-water_extent
    r=np.sqrt(x*x+y*y);a=np.arctan2(y,x) if golf else np.arctan2(x,y)
    if golf:distance=r-golf_radius(a)*1.011;lagoon=np.zeros_like(r)
    else:
        distance=np.maximum(r-fish_radius(a)*.995,27.6-r)
        inlet=(1-smooth(4.4,5.2,np.abs(x)))*smooth(26,28,y);distance=np.maximum(distance,inlet*np.minimum(4.5-np.abs(x),6));lagoon=1-smooth(24,32,r)
    depth=smooth(0,34 if golf else 15,distance);colors=rgb('54C8C0')[None,None,:]*(1-depth[:,:,None])+rgb('19779D')[None,None,:]*depth[:,:,None]
    save_image(sport+'_WaterColor',colors,folder);save_image(sport+'_WaterDepth',np.stack((np.clip(.5+distance/64,0,1),depth,lagoon),2),folder,True)
    data=dict(sport=sport,concept=code,name='Limestone Cove Links' if golf else 'Limestone Garden Lagoon',extent=extent,water_extent=water_extent,sea_level=sea,capacity=10 if golf else 5,spawns=spawns,stands=[] if golf else stands,safe_return=safe,instances=instances,boundaries=boundaries,routes=routes,views=views,bunkers=[point(b[0],b[1],float(height(b[0],b[1]))+.12) for b in BUNKERS] if golf else [],terrain_file='Terrain.fbx',structures_file='Structures.fbx',cascade_file='Cascade.fbx' if golf else '',source='ArtSource/RefinedIslands/'+sport+'_'+code+'.blend')
    data['signs']=signs
    (folder/'layout.json').write_text(json.dumps(data,indent=2));(SOURCE/(sport+'_'+code+'-layout.json')).write_text(json.dumps(data,indent=2))
    # Source master contains linked reusable meshes and packed textures.
    scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.4,.6,.8,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.7
    bpy.ops.object.light_add(type='SUN');sun=bpy.context.object;sun.name=sport+'_ReviewSun';sun.rotation_euler=(.7,-.4,-.6);sun.data.energy=2.5;sun.data.angle=.12
    p=views[0]['position'];t=views[0]['target'];bpy.ops.object.camera_add(location=(p['x'],p['z'],p['y']));cam=bpy.context.object;cam.name=sport+'_Overview';cam.rotation_euler=(Vector((t['x'],t['z'],t['y']))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.clip_end=4000;scene.camera=cam
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(sport+'_'+code+'.blend')),compress=True)
    for col in (assembly,terrain_col,structures_col,water_col,grass_col):
        if col:col.hide_render=True;col.hide_viewport=True
    sun.hide_render=True;cam.hide_render=True
    print('REFINED_WORLD_COMPLETE',sport,len(instances),'instances',len(routes),'routes',flush=True)

make_world('Golf');make_world('Fishing')
print('REFINED_ISLANDS_COMPLETE',flush=True)
