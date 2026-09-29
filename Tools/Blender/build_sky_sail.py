"""Deterministic, metre-scale Sky-Sail modules. Blender 5.1, no external packages."""
import bpy, sys, math, json
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
from refined_island_kit import Meshes

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Game/Assets/_Game/Art/SkySail'
SRC = ROOT/'ArtSource/SkySail'
OUT.mkdir(parents=True, exist_ok=True); SRC.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
palette = {'Timber':('B68651',.48,0), 'Teal':('18777E',.27,.18), 'Cream':('F1DFB5',.42,0),
           'Bronze':('C79855',.26,.72), 'Steel':('253D45',.3,.7), 'Cushion':('DCE7D6',.72,0),
           'Glass':('99D9DC',.12,.25), 'Lamp':('FFE1A3',.3,.15), 'Rope':('977752',.82,0)}
mats={}
for key,(h,rough,metal) in palette.items():
    m=bpy.data.materials.new('SS_'+key);m.diffuse_color=tuple(int(h[i:i+2],16)/255 for i in (0,2,4))+(1,)
    m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=m.diffuse_color;p.inputs['Roughness'].default_value=rough;p.inputs['Metallic'].default_value=metal
    mats[key]=m

def rail(m,name,a,b,r=.05,material='Bronze'):
    m.beam(name,a,b,r,material,12)

def bolts(m, x, y, z, axis='Z'):
    for a in (-.1,.1):
        for b in (-.1,.1):
            p=(x+a,y+b,z);q=(x+a,y+b,z+.035)
            m.beam('Fasteners',p,q,.037,'Bronze',8)

def rounded_rect(w,d,r,z,n=12):
    pts=[]
    for cx,cy,start in ((w/2-r,d/2-r,0),(-w/2+r,d/2-r,90),(-w/2+r,-d/2+r,180),(w/2-r,-d/2+r,270)):
        for j in range(n+1):
            a=math.radians(start+j*90/n);pts.append((cx+r*math.cos(a),cy+r*math.sin(a),z))
    return pts+[pts[0]]

def cabin_shell(m):
    # Rounded perimeter structural rings; open glazing areas preserve views.
    for z,r,mat in ((-.18,.16,'Steel'),(.04,.11,'Bronze'),(.95,.11,'Teal'),(2.85,.10,'Bronze'),(3.12,.16,'Teal')):
        m.curve('Perimeter',rounded_rect(5.8,7.4,.65,z),r,mat,12)
    m.box('Undertray',(0,0,-.23),(5.2,6.7,.35),'Steel',.15)
    for y in [i*.17-3.4 for i in range(41)]:m.box('Deck',(0,y,0),(5.32,.16,.11),'Timber',.018)
    for x in (-2.72,2.72):
        for y in (-2.9,0,2.9):
            m.box('WindowMullions',(x,y,1.8),(.13,.16,2.55),'Cream',.045)
            m.box('LowerPanels',(x,y*.79,.51),(.12,2.15,.79),'Teal',.05)
            m.box('PanelInlay',(x*1.025,y*.79,.64),(.02,1.80,.025),'Bronze',.005)
            rail(m,'Handrail',(x*.87,y-.9,1.22),(x*.87,y+.9,1.22))
    # Rear entrance is left clear for independent sliding door modules.
    for x in (-2.02,2.02):
        m.box('EntryPanel',(x,-3.42,.55),(1.24,.15,.88),'Teal',.06)
        m.box('EntryPillar',(x*.68,-3.47,1.61),(.14,.17,3.0),'Cream',.04)
    for x in (-1.7,0,1.7):m.box('FrontPanel',(x,3.4,.5),(1.55,.16,.85),'Teal',.05)
    for x in (-2.2,2.2):
        m.box('Bumper',(x,-3.57,.0),(.65,.18,.21),'Steel',.07)
        m.rock('Lamp',(x,-3.52,2.61),(.13,.1,.13),1,'Lamp',3)
    # Arched metal roof, thick fascia and warm interior soffit.
    v=[];f=[];n=36
    for y in (-3.75,3.75):
        for i in range(n+1):
            x=-3.05+6.1*i/n;v.append((x,y,3.08+.48*(1-(x/3.05)**2)))
    for i in range(n):f.append((i,i+1,i+1+n+1,i+n+1))
    m.add('Roof',v,f,'Teal',True)
    m.add('Soffit',[(x,y,z-.12) for x,y,z in v],[tuple(reversed(a)) for a in f],'Timber',True)
    for y in (-3.75,3.75):m.curve('RoofFascia',[(x,y,3.08+.48*(1-(x/3.05)**2)) for x in [-3.05+i*6.1/n for i in range(n+1)]],.085,'Cream',12)
    for y in (-2.8,0,2.8):rail(m,'RoofRib',(-2.8,y,3.12),(2.8,y,3.12),.06,'Bronze')
    m.box('SkylightTrim',(0,0,3.52),(1.4,3.4,.07),'Bronze',.15)
    m.box('Skylight',(0,0,3.565),(1.24,3.2,.035),'Glass',.12)

def interior(m):
    for x in (-2.14,2.14):
        for y in (-2.4,-1.2,0,1.2,2.4):
            m.box('SeatBase',(x,y,.31),(.72,.98,.6),'Timber',.08)
            m.box('SeatCushion',(x*.99,y,.65),(.8,1.0,.17),'Cushion',.10)
            m.box('SeatBack',(x*1.14,y,1.12),(.13,1.0,.85),'Cushion',.07)
            for yy in (y-.44,y+.44):rail(m,'SeatPiping',(x-.33,yy,.74),(x+.33,yy,.74),.018,'Bronze')
    for y in (-1.8,1.8):
        rail(m,'GrabPole',(.90,y,.08),(.90,y,2.94),.048)
        m.beam('PoleFoot',(.90,y,.06),(.90,y,.1),.13,'Bronze',20)
    for x in (-1.5,1.5):
        rail(m,'OverheadRail',(x,-2.8,2.65),(x,2.8,2.65),.04)
        for y in (-1.9,0,1.9):
            m.curve('LeatherGrip',[(x+.15*math.cos(a),y,2.42+.16*math.sin(a)) for a in [i*2*math.pi/24 for i in range(25)]],.022,'Rope',8)

def door(m):
    for x in (-.61,.61):m.box('Frame',(x,0,1.46),(.095,.1,2.9),'Cream',.025)
    for z in (.12,1.0,2.85):m.box('Crossbar',(0,0,z),(1.3,.1,.095),'Bronze',.022)
    m.box('LowerPanel',(0,0,.52),(1.18,.07,.84),'Teal',.025)
    m.box('Glass',(0,0,1.91),(1.16,.025,1.74),'Glass',.008)
    rail(m,'Handle',(.39,-.10,1.15),(.39,-.10,1.65),.027)

def wheel(m,p,r=.42):
    x,y,z=p
    m.beam('Wheel',(x-.1,y,z),(x+.1,y,z),r,'Steel',32)
    m.beam('Hub',(x-.14,y,z),(x+.14,y,z),r*.35,'Bronze',24)
    for a in range(0,360,60):
        t=math.radians(a);rail(m,'Spoke',(x-.11,y,z),(x-.11,y+math.cos(t)*r*.82,z+math.sin(t)*r*.82),.026,'Bronze')

def hanger(m):
    for y in (-1.0,1.0):
        rail(m,'RoofSupport',(-1.5,y,3.45),(0,y,4.6),.09,'Steel');rail(m,'RoofSupport',(1.5,y,3.45),(0,y,4.6),.09,'Steel')
    m.box('Neck',(0,0,4.98),(.19,1.15,2.2),'Cream',.04)
    m.box('Bogie',(0,0,6.07),(.26,2.6,.16),'Teal',.04)
    for y in (-.95,.95):wheel(m,(0,y,6.17),.32)
    m.box('Grip',(0,0,6.5),(.24,.58,.10),'Bronze',.04)

def tower(m):
    # 29.5m sheave elevation; braced tapering legs and an inspectable service head.
    m.box('Foundation',(0,0,0),(5.4,5.4,1.4),'Cream',.18)
    for x in (-1,1):
        for y in (-1,1):
            rail(m,'Leg',(x*2,y*2,.5),(x*.8,y*.8,27.8),.21,'Cream')
            m.box('Baseplate',(x*2,y*2,.8),(.7,.7,.13),'Steel',.045);bolts(m,x*2,y*2,.88)
    for z in (3,8,13,18,23):
        s=2-z/30*1.2;t=2-(z+5)/30*1.2
        for y in (-1,1):
            rail(m,'Brace',(-s,y*s,z),(t,y*t,z+5),.065,'Bronze');rail(m,'Brace',(s,y*s,z),(-t,y*t,z+5),.065,'Bronze')
        rail(m,'Ring',(-s,-s,z),(s,-s,z),.11,'Teal');rail(m,'Ring',(-s,s,z),(s,s,z),.11,'Teal')
    m.box('ServiceDeck',(0,0,27.65),(3.4,4.2,.18),'Steel',.07)
    for x in (-1.6,1.6):
        rail(m,'ServiceRail',(x,-2,28.7),(x,2,28.7),.045)
        for y in (-2,0,2):rail(m,'Upright',(x,y,27.7),(x,y,28.7),.045)
    m.box('Head',(0,0,28.72),(.25,5.2,.35),'Teal',.06)
    for y in (-2.1,-.7,.7,2.1):wheel(m,(0,y,29.12),.38)
    for z in [i*.34+1 for i in range(78)]:rail(m,'Ladder',(.87,-.9,z),(1.4,-.9,z),.028,'Steel')
    for x in (.87,1.4):rail(m,'LadderRail',(x,-.9,1),(x,-.9,27.5),.028,'Steel')

def station(m):
    for x in (-5.3,5.3):
        for y in (-6,0,6):
            m.box('StonePier',(x,y,-1.5),(1.0,1.0,5),'Cream',.12)
            m.box('PierCap',(x,y,.43),(1.25,1.25,.3),'Cream',.05)
    for x in [-5.85+i*.18 for i in range(66)]:m.box('Boardwalk',(x,0,.65),(.17,15,.17),'Timber',.02)
    # Boarding aisle and two tall timber portals frame the sea.
    for x in (-5.35,5.35):
        for y in (-5.8,4.8):
            m.box('ColumnShoe',(x,y,1.0),(.6,.6,.55),'Bronze',.04)
            rail(m,'Pillar',(x,y,.7),(x,y,8.0),.20,'Timber')
            rail(m,'KneeBrace',(x,y,6.6),(x*.63,y,7.8),.12,'Timber')
        rail(m,'PerimeterRail',(x,-6.8,1.82),(x,6.8,1.82),.055)
        for y in (-6.8,-3.4,0,3.4,6.8):rail(m,'RailPost',(x,y,.73),(x,y,1.85),.055)
    for y in (-5.8,4.8):rail(m,'RoofBeam',(-5.35,y,8),(5.35,y,8),.22,'Timber')
    # Tensioned double-curved fabric roof with a real underside.
    v=[];f=[];nx=24;ny=28
    for j in range(ny+1):
        y=-7+14*j/ny
        for i in range(nx+1):
            x=-6+12*i/nx;z=8.4+1.5*(x/6)**2-.55*math.sin(j*math.pi/ny);v.append((x,y,z))
    for j in range(ny):
        for i in range(nx):a=j*(nx+1)+i;f.append((a,a+1,a+nx+2,a+nx+1))
    m.add('SailCanopy',v,f,'Teal',True);m.add('CanopyUnderside',[(x,y,z-.035) for x,y,z in v],[tuple(reversed(a)) for a in f],'Cream',True)
    for x in (-6,6):m.curve('SailEdge',[(x,-7+14*j/ny,9.9-.55*math.sin(j*math.pi/ny)) for j in range(ny+1)],.045,'Bronze',12)
    # Benches, lanterns and small planted tubs, not decoration inside the boarding lane.
    for x in (-4.55,4.55):
        for y in (-3.8,3.8):
            m.box('Bench',(x,y,1.17),(.7,2.3,.16),'Timber',.055)
            for yy in (y-.8,y+.8):m.box('BenchLeg',(x,yy,.94),(.45,.12,.4),'Teal',.035)
        for y in (-5.7,4.7):
            m.box('Lantern',(x,y,4.0),(.34,.34,.55),'Bronze',.045);m.box('LanternGlass',(x,y,4.0),(.29,.29,.41),'Lamp',.025)
    m.box('SignBoard',(0,-6.0,5.7),(6.8,.19,1.0),'Teal',.07)
    for x in (-3.7,3.7):m.box('Mooring',(x,6.7,.9),(.22,.22,.5),'Bronze',.04)

def gangway(m):
    for y in [i*.19-2.9 for i in range(31)]:m.box('Plank',(0,y,0),(5.0,.18,.16),'Timber',.018)
    for x in (-2.4,2.4):
        rail(m,'Rail',(x,-3,1.1),(x,3,1.1),.045)
        for y in (-3,0,3):rail(m,'Post',(x,y,0),(x,y,1.1),.055,'Cream')

specs=[]
for name,fn in [('CabinShell',cabin_shell),('CabinInterior',interior),('SlidingDoor',door),('CabinHanger',hanger),('CableTower',tower),('SailStation',station),('Gangway',gangway)]:
    c=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(c);m=Meshes();fn(m);obs=m.finish(name,mats,c)
    bpy.ops.object.select_all(action='DESELECT')
    for o in obs:o.select_set(True)
    bpy.context.view_layer.objects.active=obs[0]
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False)
    triangles=sum(len(p.vertices)-2 for o in obs for p in o.data.polygons)
    specs.append(dict(name=name,triangles=triangles,meshes=len(obs)))
    # Each source is standalone and editable as well as included in the kit master.
    module_scene=bpy.data.scenes.new(name)
    module_scene.unit_settings.system='METRIC'
    module_scene.collection.children.link(c)
    bpy.data.libraries.write(str(SRC/(name+'.blend')),{module_scene},fake_user=True)
    bpy.data.scenes.remove(module_scene)
# Keep the master easy to inspect: source modules retain their export origin,
# while collection instances arrange them across an assembly workspace.
layout={'CabinShell':(0,0,0),'CabinInterior':(10,0,0),'SlidingDoor':(19,0,0),'CabinHanger':(24,0,0),'CableTower':(34,8,0),'SailStation':(0,22,0),'Gangway':(17,22,0)}
for name,position in layout.items():
    collection=bpy.data.collections[name]
    bpy.context.scene.collection.children.unlink(collection)
    instance=bpy.data.objects.new(name+' · editable module',None)
    instance.instance_type='COLLECTION';instance.instance_collection=collection;instance.location=position
    bpy.context.scene.collection.objects.link(instance)
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(SRC/'SkySailAssetKit.blend'))
for name in layout:
    bpy.ops.wm.open_mainfile(filepath=str(SRC/(name+'.blend')))
    bpy.ops.wm.save_as_mainfile(filepath=str(SRC/(name+'.blend')))
bpy.ops.wm.open_mainfile(filepath=str(SRC/'SkySailAssetKit.blend'))
(OUT/'modules.json').write_text(json.dumps(dict(modules=specs,materials=[dict(name='SS_'+k,color=v[0],roughness=v[1],metallic=v[2]) for k,v in palette.items()]),indent=2))
print('SKY_SAIL_ASSETS_COMPLETE',json.dumps(specs))
