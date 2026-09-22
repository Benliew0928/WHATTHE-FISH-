"""Rebuild original stadium and athlete assets. Run with Blender --background --python."""
import bpy, math, os, json
from mathutils import Vector
from pathlib import Path

ROOT = Path('C:/UMPSA')
OUT = ROOT / 'Game/Assets/_Game/Art'
OUT.mkdir(parents=True, exist_ok=True)
PALETTE = {'Concrete':(.49,.55,.60,1),'Dark':(.045,.09,.14,1), 'Roof':(.82,.87,.88,1),
 'Seat':(.10,.46,.53,1), 'Trim':(.98,.58,.22,1),'White':(.94,.97,.95,1),
 'GrassA':(.13,.39,.23,1),'GrassB':(.17,.45,.27,1),'Ground':(.18,.24,.28,1),
 'Screen':(.018,.038,.07,1),'Skin':(.87,.59,.40,1),'Hair':(.075,.04,.025,1),
 'Jersey':(.12,.65,.62,1),'Shorts':(.045,.10,.18,1),'Boot':(.97,.62,.24,1)}

def reset():
    bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
    for m in list(bpy.data.materials): bpy.data.materials.remove(m)
    for name,c in PALETTE.items():
        m=bpy.data.materials.new(name); m.diffuse_color=c; m.use_nodes=True
        bs=m.node_tree.nodes.get('Principled BSDF'); bs.inputs['Base Color'].default_value=c; bs.inputs['Roughness'].default_value=.72

def finish(o,name,mat):
    o.name=name; o.data.materials.append(bpy.data.materials[mat]); return o

def box(name,loc,size,mat,bevel=0):
    sx,sy,sz=[v/2 for v in size]
    verts=[(-sx,-sy,-sz),(sx,-sy,-sz),(sx,sy,-sz),(-sx,sy,-sz),(-sx,-sy,sz),(sx,-sy,sz),(sx,sy,sz),(-sx,sy,sz)]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)])
    o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);o.location=loc
    if bevel:
        bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
        mod=o.modifiers.new('Soft edges','BEVEL'); mod.width=bevel; mod.segments=2
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return finish(o,name,mat)

def sphere(name,loc,size,mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=10,location=loc)
    o=bpy.context.object; o.scale=size; bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    for p in o.data.polygons:p.use_smooth=True
    return finish(o,name,mat)

def beam(name,a,b,r,mat,vertices=8):
    d=Vector(b)-Vector(a); mid=(Vector(a)+Vector(b))/2
    v=[(r*math.cos(i*2*math.pi/vertices),r*math.sin(i*2*math.pi/vertices),z) for z in [-d.length/2,d.length/2] for i in range(vertices)]
    faces=[tuple(range(vertices-1,-1,-1)),tuple(range(vertices,vertices*2))]+[(i,(i+1)%vertices,(i+1)%vertices+vertices,i+vertices) for i in range(vertices)]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],faces);o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);o.location=mid
    o.rotation_euler=d.to_track_quat('Z','Y').to_euler(); return finish(o,name,mat)

def line(name,coords,mat='White',r=.06):
    for a,b in zip(coords,coords[1:]):beam(name,a,b,r,mat,6)

def arc(name,c,r,start,end,mat='White',width=.06):
    line(name,[(c[0]+r*math.cos(start+(end-start)*i/64),c[1]+r*math.sin(start+(end-start)*i/64),c[2]) for i in range(65)],mat,width)

def combine_materials():
    # One mesh per material reduces draw calls. Flags remain independently togglable.
    groups={}
    for o in list(bpy.context.scene.objects):
        if o.type=='MESH':groups.setdefault(('Flags' if o.name.startswith('Flag') else o.data.materials[0].name),[]).append(o)
    for key,objects in groups.items():
        bpy.ops.object.select_all(action='DESELECT')
        for o in objects:o.select_set(True)
        bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();objects[0].name=key
        # Collapse duplicate material slots after joining thousands of instances.
        mats=list(objects[0].data.materials); unique=[]; remap={}
        for i,m in enumerate(mats):
            if m not in unique:unique.append(m)
            remap[i]=unique.index(m)
        ids=[remap[p.material_index] for p in objects[0].data.polygons]
        objects[0].data.materials.clear()
        for m in unique:objects[0].data.materials.append(m)
        for p,idx in zip(objects[0].data.polygons,ids):p.material_index=idx

def export(name,source):
    bpy.ops.wm.save_as_mainfile(filepath=str(source))
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=False,object_types={'MESH','ARMATURE'},
        axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,
        bake_anim_use_nla_strips=False,path_mode='AUTO',use_mesh_modifiers=True)

def stadium():
    reset()
    box('Foundation',(0,0,-.65),(124,166,1),'Ground')
    box('Pitch',(0,0,-.12),(68,105,.24),'GrassA')
    for i in range(10):
        if i%2:box('MowingStripe',(0,-52.5+(i+.5)*10.5,.005),(68,10.5,.012),'GrassB')
    z=.035
    line('Touchlines',[(-34,-52.5,z),(-34,52.5,z),(34,52.5,z),(34,-52.5,z),(-34,-52.5,z)])
    line('Halfway',[(-34,0,z),(34,0,z)]);arc('CentreCircle',(0,0,z),9.15,0,2*math.pi)
    sphere('CentreSpot',(0,0,z),(.13,.13,.025),'White')
    for s in [-1,1]:
        y=s*52.5
        for w,d in [(40.32,16.5),(18.32,5.5)]:
            line('PenaltyBox',[(-w/2,y,z),(-w/2,y-s*d,z),(w/2,y-s*d,z),(w/2,y,z)])
        sphere('PenaltySpot',(0,y-s*11,z),(.14,.14,.025),'White')
        # Penalty arc outside the penalty area.
        start=math.asin(5.5/9.15)
        if s<0:arc('PenaltyArc',(0,y+11,z),9.15,start,math.pi-start)
        else:arc('PenaltyArc',(0,y-11,z),9.15,math.pi+start,2*math.pi-start)
        gy=y+s*.3
        for x in [-3.66,3.66]:
            beam('Goalpost',(x,gy,0),(x,gy,2.44),.065,'White',12)
            line('GoalFrame',[(x,gy,2.44),(x,gy+s*2.1,1.9),(x,gy+s*2.1,0)],'White',.045)
        beam('Crossbar',(-3.66,gy,2.44),(3.66,gy,2.44),.065,'White',12)
        for i in range(25):
            x=-3.66+i*7.32/24
            line('Net',[(x,gy,2.44),(x,gy+s*2.1,1.9),(x,gy+s*2.1,.05)],'Roof',.009)
        for j in range(9):
            beam('Net',(-3.66,gy+s*2.1,j*.2375),(3.66,gy+s*2.1,j*.2375),.009,'Roof',4)
        for x in [-34,34]:
            beam('FlagPole',(x, y,0),(x,y,1.65),.023,'White')
            box('FlagsCorner',(x+.2,y,1.48),(.4,.025,.3),'Trim')
    # Lower perimeter separates spectators from the pitch.
    for s in [-1,1]:
        box('Perimeter',(s*39,0,.65),(.3,116,1.3),'Concrete')
        for y in range(-54,55,9):box('Advertising',(s*38.7,y,.7),(.12,8.6,.85),'Trim',.04)
        if s<0:
            for x in [-21,21]:box('EndWall',(x,s*59,.65),(37,.3,1.3),'Concrete')
        else:box('EndWall',(0,s*59,.65),(79,.3,1.3),'Concrete')
        for x in range(-33,34,11):box('Advertising',(x,s*58.7,.7),(10.5,.12,.85),'Seat',.04)
    # Reusable sculpted seat mesh, linked across the stands.
    seat=box('SeatMaster',(0,0,0),(.48,.43,.11),'Seat')
    # A chamfered back silhouette uses 12 vertices instead of a dense bevel.
    contour=[(-.235,0),(.235,0),(.235,.35),(.165,.45),(-.165,.45),(-.235,.35)]
    v=[(x,y,z) for y in [.12,.22] for x,z in contour]
    faces=[tuple(range(5,-1,-1)),tuple(range(6,12))]+[(i,(i+1)%6,(i+1)%6+6,i+6) for i in range(6)]
    mesh=bpy.data.meshes.new('ReusableSeatBack');mesh.from_pydata(v,[],faces)
    back=bpy.data.objects.new('SeatBack',mesh);bpy.context.collection.objects.link(back);finish(back,'SeatBack','Seat')
    bpy.ops.object.select_all(action='DESELECT');seat.select_set(True);back.select_set(True)
    bpy.context.view_layer.objects.active=seat;bpy.ops.object.join();seat_mesh=seat.data; bpy.data.objects.remove(seat,do_unlink=True)
    def stand(label,center,length,angle):
        # Local x is across the stand, local y recedes from the field.
        ca,sa=math.cos(angle),math.sin(angle)
        def world(x,y,z):return(center[0]+x*ca-y*sa,center[1]+x*sa+y*ca,z)
        def block(n,x,y,z,sz,mat):
            o=box(n,world(x,y,z),sz,mat);o.rotation_euler.z=angle;return o
        bays=int(length/10)
        for row in range(12):
            dep=row*.85;h=1.3+row*.47
            # Two entries in each stand, with terraces bridging above them.
            for bay in range(bays):
                x=-length/2+5+bay*10
                if row<4 and bay in [bays//3,2*bays//3]:continue
                block('Terrace',x,dep,h-.22,(10,.85,.44),'Concrete')
                for col in range(14):
                    o=bpy.data.objects.new('Seat',seat_mesh);bpy.context.collection.objects.link(o)
                    o.location=world(x-4.35+col*.64,dep,h+.12);o.rotation_euler.z=angle
            for bay in range(bays+1):
                x=-length/2+bay*10
                for step in [0,1]:block('AisleStep',x,dep+step*.42,h-.21+step*.235,(.9,.43,.22),'Roof')
        for bay in range(bays+1):
            x=-length/2+bay*10
            beam('RoofColumn',world(x,10,0),world(x,10,16),.16,'Dark')
            beam('RoofRafter',world(x,10,15),world(x,-2,13.4),.13,'Dark')
            beam('RoofBrace',world(x,10,11.5),world(x,2,13.94),.08,'Roof')
            beam('AisleRail',world(x,0,2.2),world(x,9,7.2),.035,'Dark')
        roof=block('Canopy',0,4.5,14.45,(length+1,13.5,.22),'Roof');roof.rotation_euler.x=.13
        block('Fascia',0,-2.3,13.55,(length+1,.25,.8),'Seat')
        block('RearWall',0,10.8,4.8,(length,.45,9.6),'Concrete')
        for bay in range(bays):
            x=-length/2+5+bay*10
            block('ExitHeader',x,10.5,2.5,(2.4,.15,.5),'Trim')
            if bay%2==0:
                block('LightHousing',x,-1.8,13.25,(2.8,.6,.4),'Dark')
                block('Floodlight',x,-1.85,13,(2.5,.45,.06),'White')
    stand('North',(0,63),100,0);stand('South',(0,-63),100,math.pi)
    stand('East',(43,0),120,-math.pi/2);stand('West',(-43,0),120,math.pi/2)
    # Technical areas and tunnel: access through the gap in the south wall.
    for x in [-12,12]:
        box('DugoutFloor',(x,-56,.15),(8,2,.3),'Concrete')
        box('DugoutRoof',(x,-56,2.4),(8.4,2.6,.18),'Seat',.08)
        for dx in [-4,4]:beam('DugoutSupport',(x+dx,-56.7,0),(x+dx,-56.7,2.35),.05,'Dark')
        box('Bench',(x,-56.3,.6),(7,.55,.2),'Roof',.08)
    for x in [-2,2]:box('TunnelWall',(x,-60,1.6),(.4,5,3.2),'Dark')
    box('TunnelTop',(0,-60,3.3),(4.4,5,.35),'Dark')
    # Elevated screen faces the pitch; runtime UI is mounted over its face.
    box('ScreenFrame',(0,72,11),(18,1,8),'Dark',.25)
    box('ScreenFace',(0,71.45,11),(17,.08,7),'Screen')
    for x in [-6,6]:beam('ScreenSupport',(x,73,0),(x,73,10),.22,'Dark')
    for s in [-1,1]:
        for x in [-39,39]:
            beam('FlagMast',(x,s*60,0),(x,s*60,9),.07,'Roof')
            box('FlagsBanner',(x+1,s*60,7.8),(2,.04,2),'Trim')
    combine_materials()
    export('Stadium',ROOT/'ArtSource/Football/Stadium.blend')
    # Author a review camera and lighting only in the Blender source.
    bpy.ops.object.light_add(type='SUN',location=(0,0,40));bpy.context.object.rotation_euler=(.5,-.45,-.4);bpy.context.object.data.energy=3
    bpy.context.scene.world.color=(.35,.4,.5)
    bpy.ops.object.camera_add(location=(100,-125,85));cam=bpy.context.object
    cam.rotation_euler=(Vector((0,0,3))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=42
    scene=bpy.context.scene;scene.camera=cam;scene.render.engine='CYCLES';scene.cycles.samples=16
    scene.render.resolution_x=1440;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
    scene.render.filepath=str(ROOT/'Builds/stadium-blender.png')
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'ArtSource/Football/Stadium.blend'))
    bpy.ops.render.render(write_still=True)
    cam.location=(15,-40,1.7);cam.rotation_euler=(Vector((0,35,8))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=24
    scene.render.filepath=str(ROOT/'Builds/stadium-pitch-blender.png');bpy.ops.render.render(write_still=True)

def character():
    reset();parts=[]
    def part(o,bone):parts.append((o,bone));return o
    part(sphere('Torso',(0,0,.88),(.28,.19,.34),'Jersey'),'Spine')
    part(box('Shorts',(0,0,.61),(.46,.34,.25),'Shorts',.08),'Hips')
    part(sphere('Head',(0,0,1.47),(.36,.30,.37),'Skin'),'Head')
    part(sphere('HairShort',(0,.035,1.69),(.37,.30,.18),'Hair'),'Head')
    part(sphere('HairTuft',(0,.09,1.81),(.18,.19,.19),'Hair'),'Head')
    for s in [-1,1]:
        part(sphere('Ear',(s*.34,0,1.46),(.065,.06,.105),'Skin'),'Head')
        part(sphere('Eye',(s*.125,-.285,1.5),(.04,.022,.057),'Dark'),'Head')
        part(sphere('EyeShine',(s*.125-.012,-.306,1.519),(.012,.007,.016),'White'),'Head')
        part(sphere('Cheek',(s*.2,-.26,1.40),(.06,.014,.025),'Trim'),'Head')
        side='L' if s<0 else 'R'
        part(sphere('Sleeve'+side,(s*.32,0,1.03),(.13,.16,.17),'Jersey'),'Arm'+side)
        part(sphere('ArmMesh'+side,(s*.37,0,.87),(.085,.10,.18),'Skin'),'Arm'+side)
        part(sphere('Hand'+side,(s*.39,0,.72),(.105,.11,.105),'Skin'),'Arm'+side)
        part(sphere('LegMesh'+side,(s*.14,0,.38),(.105,.115,.24),'Skin'),'Leg'+side)
        part(sphere('Sock'+side,(s*.14,0,.23),(.108,.118,.12),'White'),'Leg'+side)
        part(sphere('Shoe'+side,(s*.14,-.055,.105),(.13,.21,.105),'Boot'),'Leg'+side)
    part(sphere('Nose',(0,-.306,1.42),(.045,.045,.045),'Skin'),'Head')
    part(box('Smile',(0,-.289,1.345),(.085,.02,.015),'Dark',.006),'Head')
    bpy.ops.object.armature_add();rig=bpy.context.object;rig.name='AthleteRig';bpy.ops.object.mode_set(mode='EDIT')
    bones=rig.data.edit_bones;bones.remove(bones[0])
    defs=[('Hips',(0,0,.5),(0,0,.75),None),('Spine',(0,0,.75),(0,0,1.15),'Hips'),('Head',(0,0,1.15),(0,0,1.8),'Spine'),
        ('ArmL',(-.30,0,1.08),(-.39,0,.7),'Spine'),('ArmR',(.30,0,1.08),(.39,0,.7),'Spine'),
        ('LegL',(-.14,0,.60),(-.14,0,.1),'Hips'),('LegR',(.14,0,.60),(.14,0,.1),'Hips')]
    for name,head,tail,parent in defs:
        b=bones.new(name);b.head=head;b.tail=tail
        if parent:b.parent=bones[parent]
    bpy.ops.object.mode_set(mode='OBJECT')
    for o,b in parts:
        group=o.vertex_groups.new(name=b);group.add(list(range(len(o.data.vertices))),1,'REPLACE')
        mod=o.modifiers.new('Rig','ARMATURE');mod.object=rig;o.parent=rig
    rig.animation_data_create()
    for name,amplitude in [('Idle',.025),('Walk',.38),('Run',.65)]:
        action=bpy.data.actions.new(name);rig.animation_data.action=action
        for f in range(1,34,4):
            phase=(f-1)/32*2*math.pi
            for bn in rig.pose.bones:
                bn.rotation_mode='XYZ';value=0
                if 'Leg' in bn.name:value=math.sin(phase)*amplitude*(1 if bn.name.endswith('L') else -1)
                if 'Arm' in bn.name:value=math.sin(phase)*amplitude*(-1 if bn.name.endswith('L') else 1)
                bn.rotation_euler=(value,0,0);bn.keyframe_insert('rotation_euler',frame=f)
            rig.pose.bones['Hips'].location.z=abs(math.sin(phase))*(.015 if name=='Idle' else .04)
            rig.pose.bones['Hips'].keyframe_insert('location',frame=f)
        action.use_fake_user=True
    rig.animation_data.action=bpy.data.actions['Idle'];bpy.context.scene.frame_set(1)
    export('Athlete',ROOT/'ArtSource/Shared/Characters/Athlete.blend')

stadium();character()
(ROOT/'ArtSource/asset-register.json').write_text(json.dumps({'Stadium.fbx':'Football/Stadium.blend','Athlete.fbx':'Shared/Characters/Athlete.blend','generator':'Tools/Blender/build_assets.py','units':'metres','pitch_metres':[68,105]},indent=2))
print('SPORTS_ASSETS_COMPLETE')
