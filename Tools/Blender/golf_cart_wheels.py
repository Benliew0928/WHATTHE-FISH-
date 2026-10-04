"""Separate existing Meshy wheel faces while keeping their UVs and full master."""
import math
import bpy
import bmesh
from mathutils import Vector
from mathutils import Matrix
from mathutils.bvhtree import BVHTree

# Blender +Y is the front. Selection radius includes the original tyre contour.
WHEELS = [('FL', (-.635, 1.058, .339), .40), ('FR', (.635, 1.058, .339), .40),
          ('RL', (-.635, -.845, .328), .36), ('RR', (.635, -.845, .328), .36)]
RIM_RADIUS = .190  # Equal 38 cm cream rims leave the reference's thick tyre wall.
SEGMENTS = (64, 40, 24)
TREAD_ROWS = (16, 10, 6)
TREAD_DEPTH = .006
_rim_swatch = None

def split_wheels(source, colour, level):
    pixels = list(colour.pixels); width, height = colour.size
    uv = source.data.uv_layers.active.data
    groups = [[] for _ in range(5)]
    for face in source.data.polygons:
        point = face.center; selected = 0
        for index, (_, centre, radius) in enumerate(WHEELS):
            radial = (point.y-centre[1])**2 + (point.z-centre[2])**2
            if point.x*math.copysign(1, centre[0]) <= .46 or radial >= radius**2:
                continue
            coords = sum((uv[i].uv for i in face.loop_indices), Vector((0, 0))) / len(face.loop_indices)
            pixel = (int(coords.y*height) % height * width + int(coords.x*width) % width) * 4
            if radial > .28**2 and sum(pixels[pixel:pixel+3])/3 > .32:
                continue  # Pale fender surfaces remain fixed; the hub rotates.
            selected = index+1; break
        groups[selected].append(face.index)
    assert sum(map(len, groups)) == len(source.data.polygons) and all(groups)
    parts = []
    for index, group in enumerate(groups):
        obj = source.copy(); obj.data = source.data.copy()
        bpy.context.scene.collection.objects.link(obj)
        obj.name = ('GolfCart_Body' if index == 0 else 'GolfCart_Wheel_'+WHEELS[index-1][0])+'_LOD'+str(level)
        obj.hide_set(False); obj.hide_render = False
        bm = bmesh.new(); bm.from_mesh(obj.data); bm.faces.ensure_lookup_table()
        keep = set(group)
        bmesh.ops.delete(bm, geom=[face for face in bm.faces if face.index not in keep], context='FACES')
        # Intentional seams stay inside wheel wells. Nonplanar hole filling
        # produces visible fans across the original tyre and is not appropriate.
        # Preserve the closed source's winding. Recalculating this open subset
        # flips rear hub faces inward, so Unity culls their outer pattern.
        bm.to_mesh(obj.data); bm.free(); obj.data.update()
        if index:
            pivot = Vector(WHEELS[index-1][1])
            for vertex in obj.data.vertices: vertex.co -= pivot
            obj.location = pivot
        obj.data.update(); parts.append(obj)
    assert sum(len(part.data.polygons) for part in parts) == len(source.data.polygons)
    bpy.data.objects.remove(source, do_unlink=True)
    refine_wheel_parts(parts, colour, level)
    return parts


def refine_wheel_parts(parts, colour, level):
    """Repair only the delivery wheels; keep the full Meshy master editable.

    A common circular six-spoke rim replaces the irregular hub contour. Its
    bead shares vertices with the tyre, so there is no cut-off or floating gap.
    Both materials sample clean swatches in the existing shared atlas.
    """
    pixels = list(colour.pixels); width, height = colour.size
    def shade(uv):
        i = (int(uv.y*height) % height * width + int(uv.x*width) % width)*4
        return sum(pixels[i:i+3])/3
    body = next(p for p in parts if p.name.startswith('GolfCart_Body_'))
    mesh = body.data; uv = mesh.uv_layers.active.data; remove = []
    # Discard the dark fragments left below the two rear wheel wells. Keep the
    # pale fenders, bumper, floor, axle and all geometry outside these small zones.
    for face in mesh.polygons:
        p = face.center
        if p.z >= .25: continue
        for _, centre, _ in WHEELS[2:]:
            radial = math.hypot(p.y-centre[1], p.z-centre[2])
            if p.x*math.copysign(1, centre[0]) > .49 and .338 < radial < .49:
                tex = sum((uv[i].uv for i in face.loop_indices), Vector((0, 0)))/len(face.loop_indices)
                if shade(tex) < .32: remove.append(face.index)
                break
    bm = bmesh.new(); bm.from_mesh(mesh); bm.faces.ensure_lookup_table()
    bmesh.ops.delete(bm, geom=[bm.faces[i] for i in remove], context='FACES')
    bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context='VERTS')
    bm.to_mesh(mesh); bm.free(); mesh.update()

    global _rim_swatch
    if _rim_swatch is None:
        front = next(p for p in parts if p.name.startswith('GolfCart_Wheel_FR_'))
        uv = front.data.uv_layers.active.data
        candidates = []
        for face in front.data.polygons:
            if face.center.x <= .10 or math.hypot(face.center.y, face.center.z) >= .225: continue
            candidates.extend(uv[i].uv.copy() for i in face.loop_indices)
            candidates.append(sum((uv[i].uv for i in face.loop_indices),Vector((0,0)))/len(face.loop_indices))
        # Keep the supplied cream finish, including face-interior samples
        # rather than selecting only dark UV vertices along the spoke holes.
        _rim_swatch = clean_swatch(candidates, pixels, width, height, .88, .65, 1)

    for obj in parts:
        if not obj.name.startswith('GolfCart_Wheel_'): continue
        name = obj.name.split('_')[2]; sign = -1 if name.endswith('L') else 1
        radius = .339 if name.startswith('F') else .328
        old = obj.data; old_uv = old.uv_layers.active.data
        samples = []
        for face in old.polygons:
            if math.hypot(face.center.y, face.center.z) < .27: continue
            for loop in face.loop_indices:
                tex = old_uv[loop].uv.copy()
                if shade(tex) < .25: samples.append((old.vertices[old.loops[loop].vertex_index].co.copy(), tex))
        assert samples, 'No original tyre UV samples: '+obj.name
        # Interpolating unrelated atlas UVs crosses white body/trim islands.
        # Find a dark rubber patch and map the whole new shell within that patch.
        swatch = clean_swatch([tex for _, tex in samples], pixels, width, height, .055, 0, .18)
        build_round_wheel(old, sign, radius, SEGMENTS[level], swatch, _rim_swatch, width, height)
    add_axle_supports(body, swatch, width, height, level)


def add_axle_supports(body, swatch, width, height, level):
    """Join each hub to the fixed chassis with an axle and an upright mount."""
    mesh=body.data
    tree=BVHTree.FromPolygons([v.co.copy() for v in mesh.vertices],[list(f.vertices) for f in mesh.polygons])
    bm=bmesh.new();bm.from_mesh(mesh);uv=bm.loops.layers.uv.active
    def tube(start,end,radius):
        start,end=Vector(start),Vector(end);delta=end-start
        previous=set(bm.faces)
        matrix=Matrix.Translation((start+end)*.5)@delta.to_track_quat('Z','Y').to_matrix().to_4x4()
        bmesh.ops.create_cone(bm,cap_ends=True,cap_tris=False,segments=(10,8,6)[level],
                              radius1=radius,radius2=radius,depth=delta.length,matrix=matrix)
        faces=[f for f in bm.faces if f not in previous]
        bmesh.ops.recalc_face_normals(bm,faces=faces)
        for face in faces:
            face.smooth=len(face.verts)==4
            axis=max(range(3),key=lambda i:abs(face.normal[i]));axes=[i for i in range(3) if i!=axis]
            low=[min(v.co[a] for v in face.verts) for a in axes]
            size=[max(v.co[a] for v in face.verts)-l for a,l in zip(axes,low)]
            for loop in face.loops:
                loop[uv].uv=swatch+Vector(tuple(((loop.vert.co[a]-l)/max(s,1e-8)-.5)*6/d
                                              for a,l,s,d in zip(axes,low,size,(width,height))))
        bmesh.ops.triangulate(bm,faces=faces)
    for name,centre,_ in WHEELS:
        sign=-1 if name.endswith('L') else 1
        # The end lies inside the rotating hub, including full front steering.
        anchor=Vector((sign*.30,centre[1],centre[2]))
        hub=Vector(centre)+Vector((sign*.115,0,0))
        mount=tree.find_nearest(anchor+Vector((0,0,.28)))[0]
        assert mount is not None
        mount+=(mount-anchor).normalized()*.03
        tube(anchor,hub,.065)
        tube(anchor,mount,.070)
    bm.to_mesh(mesh);bm.free();mesh.update()


def clean_swatch(candidates, pixels, width, height, target, low, high):
    """Select a uniform 9x9 patch, keeping UVs clear of unrelated atlas islands."""
    swatches = []; visited = set()
    for tex in candidates:
        x, y = int(tex.x*width), int(tex.y*height)
        if (x,y) in visited or not (4<=x<width-4 and 4<=y<height-4): continue
        visited.add((x,y))
        values = [sum(pixels[((y+dy)*width+x+dx)*4:((y+dy)*width+x+dx)*4+3])/3
                  for dy in range(-4,5) for dx in range(-4,5)]
        if min(values)<low or max(values)>high: continue
        mean = sum(values)/len(values)
        swatches.append((abs(mean-target)+3*(max(values)-min(values)), Vector(((x+.5)/width,(y+.5)/height))))
    assert swatches, 'No clean wheel atlas swatch'
    return min(swatches, key=lambda item:item[0])[1]


def build_round_wheel(mesh, sign, radius, segments, rubber, silver, width, height):
    bm = bmesh.new(); tex_layer = bm.loops.layers.uv.new('UVMap')
    def projected_uv(face, swatch):
        # Face-local planar mapping gives caps and spoke bevels valid tangents.
        face.normal_update(); axis = max(range(3), key=lambda i:abs(face.normal[i]))
        axes = [i for i in range(3) if i!=axis]
        lo = [min(v.co[a] for v in face.verts) for a in axes]
        size = [max(v.co[a] for v in face.verts)-l for a,l in zip(axes,lo)]
        for loop in face.loops:
            loop[tex_layer].uv = swatch+Vector(tuple(((loop.vert.co[a]-l)/max(s,1e-8)-.5)*6/d
                                                      for a,l,s,d in zip(axes,lo,size,(width,height))))
    def lathe(profile, shades, closed=True, tread=False):
        rings=[]
        for j,(x,r) in enumerate(profile):
            ring=[]
            for i in range(segments):
                angle=2*math.pi*i/segments; sample_radius=r
                if tread and shades[j]==rubber:
                    # A single closed shell, with V-shaped channels cut into it.
                    # Four samples per block retain a round outer envelope while
                    # defining its groove floor and the two bevelled edges.
                    period=2*math.pi/TREAD_ROWS[SEGMENTS.index(segments)]
                    slot=i%4; block_angle=(i//4)*period+(0,.045,period*.5,period-.045)[slot]
                    shoulder=max(0,min(1,(r-RIM_RADIUS)/.090))
                    angle+=(block_angle-angle-abs(x-.030)*1.6)*shoulder
                    relief=max(0,min(1,(r-.290)/.035))
                    if slot==0: sample_radius-=TREAD_DEPTH*relief
                ring.append(bm.verts.new((sign*x,math.cos(angle)*sample_radius,math.sin(angle)*sample_radius)))
            rings.append(ring)
        strip_count=len(rings) if closed else len(rings)-1
        spans={};start=0
        for end in range(1,strip_count+1):
            if end==strip_count or shades[end]!=shades[start]:
                for j in range(start,end):spans[j]=(start,end-start)
                start=end
        for j in range(strip_count):
            for i in range(segments):
                nxt=(i+1)%segments;following=(j+1)%len(rings)
                face=bm.faces.new((rings[j][i],rings[j][nxt],rings[following][nxt],rings[following][i]))
                face.smooth=not (tread and profile[j][1]>.305 and shades[j]==rubber and i%4 in (0,3))
                for loop in face.loops:
                    angle=math.atan2(loop.vert.co.z,loop.vert.co.y)%(2*math.pi)
                    if i==segments-1 and angle<.001:angle=2*math.pi
                    row=0 if loop.vert in (rings[j][i],rings[j][nxt]) else 1
                    start,length=spans[j];v=(j-start+row)/length
                    # Continuous UVs within each swatch avoid splitting every
                    # profile ring into duplicate imported GPU vertices.
                    loop[tex_layer].uv=shades[j]+Vector(((angle/(2*math.pi)-.5)*6/width,(v-.5)*6/height))
        return rings
    # Rounded thick sidewalls and a broad chevron tread, following the supplied
    # reference. Both beads still share their vertices with the cream rim.
    profile=[(-.10,RIM_RADIUS),(-.116,.230),(-.119,.270),(-.096,.302),
             (-.068,radius-.009),(-.034,radius),(.006,radius),(.017,radius),
             (.026,radius-.003),(.034,radius-.003),(.043,radius),(.054,radius),
             (.094,radius),(.128,radius-.009),(.157,.302),(.180,.270),
             (.177,.230),(.16,RIM_RADIUS),(.174,RIM_RADIUS-.005),
             (.180,RIM_RADIUS-.040),(.150,RIM_RADIUS-.044),
             (-.080,RIM_RADIUS-.044),(-.10,RIM_RADIUS-.040)]
    if segments<SEGMENTS[0]:
        # Distant wheels keep the chevron shape with fewer axial profile rows.
        profile=[p for p in profile if p[0] not in (.006,.017,.026,.034,.043,.054)]
        profile.insert(6,(.030,radius-.003))
    lathe(profile,[rubber]*(len(profile)-6)+[silver]*6,tread=True)
    # Recessed black brake backing keeps spoke openings dark from oblique views.
    backing=lathe([(.060,RIM_RADIUS-.038),(.065,RIM_RADIUS-.038)],[rubber]*2,closed=False)
    for ring in backing:
        face=bm.faces.new(ring);projected_uv(face,rubber)
    hub=lathe([(.077,.070),(.177,.082),(.187,.075)],[silver]*3,closed=False)
    for ring in (hub[0],hub[-1]):
        face=bm.faces.new(ring);projected_uv(face,silver)
    # Preserve the supplied six-spoke design, with equal, lightly bevelled spokes.
    for spoke in range(6):
        existing_faces=set(bm.faces)
        angle=2*math.pi*spoke/6
        vertices=[]
        for x in (.092,.167):
            for r,tangent in ((.055,-.018),(RIM_RADIUS-.034,-.025),
                              (RIM_RADIUS-.034,.025),(.055,.018)):
                vertices.append(bm.verts.new((sign*x,r*math.cos(angle)-tangent*math.sin(angle),r*math.sin(angle)+tangent*math.cos(angle))))
        faces=[bm.faces.new([vertices[i] for i in indices])
               for indices in ((0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7))]
        edges=list({edge for face in faces for edge in face.edges})
        if segments==SEGMENTS[0]:
            bmesh.ops.bevel(bm,geom=edges,offset=.005,segments=1,affect='EDGES')
        # Bevel can recreate original planes as well as add edge faces. Assign
        # every spoke face, so a new face cannot fall back to atlas UV (0,0).
        for face in (f for f in bm.faces if f not in existing_faces):
            projected_uv(face,silver)
    # These new closed components have known geometry; normalize both mirrored
    # windings without applying this operation to the open authored body subsets.
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.normal_update()
    bm.to_mesh(mesh);bm.free();mesh.update()
