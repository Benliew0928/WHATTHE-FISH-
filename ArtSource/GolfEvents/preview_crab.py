"""Verify the exported FBX and render a static preview into ignored Builds/."""
from pathlib import Path
import bpy, sys, json
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
out=ROOT/(args[0] if args else "Builds/GolfHazards/crab-preview.png")
out.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(ROOT/"Game/Assets/_Game/Art/Golf/Events/CrossingCrab.fbx"))
meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
assert len(meshes)==2 and all(o.modifiers for o in meshes)
for o in meshes:o.hide_render="LOD1" in o.name
files=[i.filepath for i in bpy.data.images if i.source=="FILE"]
assert not files and not bpy.data.libraries
record={"formatAwareInspection":"Re-imported exported FBX in Blender", "meshes":[{"name":o.name,"triangles":len(o.data.polygons),"dimensions":list(o.dimensions)} for o in meshes],"activeTextureDependencies":files,"armatures":len([o for o in bpy.context.scene.objects if o.type=="ARMATURE"])}
assert record["armatures"]==1
camera=bpy.data.objects.new("Preview camera",bpy.data.cameras.new("Preview camera"));bpy.context.scene.collection.objects.link(camera);camera.location=(2.3,3.4,1.9);camera.rotation_euler=(Vector((0,.15,.3))-camera.location).to_track_quat("-Z","Y").to_euler();camera.data.type="ORTHO";camera.data.ortho_scale=2.5;bpy.context.scene.camera=camera
for name,at,power,size in [("Soft key",(3,2,5),600,4),("Soft fill",(-3,1,3),300,3)]:
 light=bpy.data.lights.new(name,"AREA");light.energy=power;light.shape="DISK";light.size=size;obj=bpy.data.objects.new(name,light);bpy.context.scene.collection.objects.link(obj);obj.location=at;obj.rotation_euler=(Vector((0,0,.3))-obj.location).to_track_quat("-Z","Y").to_euler()
scene=bpy.context.scene;scene.render.engine="BLENDER_EEVEE";scene.render.resolution_x=640;scene.render.resolution_y=640;scene.render.resolution_percentage=100;scene.render.film_transparent=True;scene.render.image_settings.file_format="PNG";scene.render.filepath=str(out);bpy.ops.render.render(write_still=True)
out.with_suffix(".json").write_text(json.dumps(record,indent=2)+"\n",encoding="utf-8")
print("CRAB_FBX_INSPECTED",json.dumps(record))
