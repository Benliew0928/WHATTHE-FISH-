"""Factor authored colour maps into shared patterns and material tints.

Factors the original encoded-colour formulas. Originals are left untouched.
Requires numpy and Pillow; output is ordinary sRGB PNG + a mapping manifest.
"""
import ast, json, hashlib
from pathlib import Path
import numpy as np
from PIL import Image

root=Path(__file__).resolve().parents[2]
art=root/'Game/Assets/_Game/Art'
out=art/'MobileMaterials';out.mkdir(exist_ok=True)
def encode(v):return np.where(v<=.0031308,v*12.92,1.055*np.maximum(v,0)**(1/2.4)-.055)
def decode(v):return np.where(v<=.04045,v/12.92,((v+.055)/1.055)**2.4)
def save(name,v):
    p=np.rint(np.clip(encode(v),0,1)*255).astype(np.uint8)
    Image.fromarray(p).save(out/(name+'.png'))
    return p.astype(np.float32)/255

rows=[];groups={};normal_groups={}
for family,filename,folder,variable in [
    ('RI','refined_island_kit.py','RefinedIslands/Shared','kinds'),
    ('Coastal','coastal_stadium_detail.py','CoastalStadiums','colors')
]:
    tree=ast.parse((root/'Tools/Blender'/filename).read_text())
    node=next(n for n in ast.walk(tree) if isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id==variable for t in n.targets))
    specs=ast.literal_eval(node.value)
    for kind,(hexcode,rough) in specs.items():
        # The saved source PNGs contain encoded RGB pigment times wash.
        # Reconstruct the scalar pattern, then fit its material tint in linear
        # lighting space and validate against every original source pixel.
        colour=np.array([int(hexcode[i:i+2],16)/255 for i in (0,2,4)],np.float32)
        name=family+'_'+kind
        source=art/folder/(name+'_BaseColor.png')
        pixels=np.array(Image.open(source).convert('RGB'),dtype=np.float32)/255
        category=(('Mineral' if kind in ('Stone','Paving') else 'Foliage' if kind in ('Leaf','LeafLight','Turf') else kind if kind in ('Timber','Bark','Sand','Rope') else 'Plain') if family=='RI' else ('Masonry' if kind in ('Stone','Paving') else 'Timber' if kind=='Timber' else 'Plain' if kind in ('Teal','Bronze','Leaf','Flower','Lamp','Screen') else 'Plaster'))
        pattern=family+'_'+category
        if pattern not in groups:
            channel=int(np.argmax(colour))
            wash=pixels[:,:,channel]/colour[channel]/1.125
            # Brightest source pigments do not clip for these authored washes.
            groups[pattern]=save(pattern,decode(np.repeat(wash[:,:,None],3,axis=2)))
        shared=decode(groups[pattern]);original=decode(pixels)
        tint=np.sum(shared*original,axis=(0,1),dtype=np.float64)/np.sum(shared*shared,axis=(0,1),dtype=np.float64)
        actual=encode(shared*tint)
        error=np.abs(np.clip(actual,0,1)-pixels)*255
        if float(np.max(error))>2.6:raise ValueError(f'{name}: source is not a shared pigment pattern, max error {np.max(error)}')
        normal=art/folder/(name+'_Normal.png')
        digest=hashlib.sha256(np.array(Image.open(normal)).tobytes()).hexdigest()
        normal_groups.setdefault(digest,str(normal.relative_to(root/'Game')).replace('\\','/'))
        rows.append(dict(material=name,source=str(source.relative_to(root/'Game')).replace('\\','/'),sourceHash=hashlib.sha256(source.read_bytes()).hexdigest(),pattern='Assets/_Game/Art/MobileMaterials/'+pattern+'.png',tint=[float(v) for v in encode(tint)],normal=normal_groups[digest],maxPixelError=float(np.max(error)),meanPixelError=float(np.mean(error))))
(out/'material-map.json').write_text(json.dumps(dict(materials=rows),indent=2))
print(f'SHARED_MATERIALS {len(rows)} pigments, {len(groups)} shared patterns, {len(normal_groups)} unique normal maps; max RGB error {max(r["maxPixelError"] for r in rows):.3f}/255')
