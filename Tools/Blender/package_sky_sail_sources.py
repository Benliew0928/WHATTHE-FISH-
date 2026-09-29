"""Repair/open-check the editable Sky-Sail sources without re-exporting game FBX."""
from pathlib import Path
import bpy

root=Path(__file__).resolve().parents[2]/'ArtSource'/'SkySail'
bpy.ops.wm.open_mainfile(filepath=str(root/'SkySailAssetKit.blend'))
layout={'CabinShell':(0,0,0),'CabinInterior':(10,0,0),'SlidingDoor':(19,0,0),'CabinHanger':(24,0,0),'CableTower':(34,8,0),'SailStation':(0,22,0),'Gangway':(17,22,0)}
for name,position in layout.items():
    collection=bpy.data.collections[name]
    scene=bpy.data.scenes.new(name)
    scene.unit_settings.system='METRIC'
    scene.collection.children.link(collection)
    assert len(scene.objects)>0, name+' has no linked meshes'
    bpy.data.libraries.write(str(root/(name+'.blend')),{scene},fake_user=True)
    bpy.data.scenes.remove(scene)
    if collection.name in bpy.context.scene.collection.children:
        bpy.context.scene.collection.children.unlink(collection)
    key=name+' · editable module'
    instance=bpy.data.objects.get(key)
    if instance is None:
        instance=bpy.data.objects.new(key,None)
        bpy.context.scene.collection.objects.link(instance)
    instance.instance_type='COLLECTION';instance.instance_collection=collection;instance.location=position
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(root/'SkySailAssetKit.blend'))
for name in layout:
    bpy.ops.wm.open_mainfile(filepath=str(root/(name+'.blend')))
    count=len(bpy.context.scene.objects)
    assert count>0,name+' opened empty'
    bpy.ops.wm.save_as_mainfile(filepath=str(root/(name+'.blend')))
    print('SKY_SOURCE_PASS',name,'linked objects',count)
