"""Derive mobile PNGs from retained imagegen masters. Requires Pillow."""
from pathlib import Path
from PIL import Image
import json, hashlib

here=Path(__file__).resolve().parent
root=here.parents[1]
dest=root/'Game/Assets/_Game/Resources/GolfUI/Events'
dest.mkdir(parents=True,exist_ok=True)
records=[]
for name,width in [('TornadoBadge',768),('CrabBadge',768),('StormPuff',512)]:
    source=here/(name+'-master.png')
    image=Image.open(source).convert('RGBA')
    assert image.getextrema()[3][0]==0, 'Transparent master required'
    size=(width,round(image.height*width/image.width))
    image=image.resize(size,Image.Resampling.LANCZOS)
    if name!='StormPuff':
        image=image.quantize(colors=256,method=Image.Quantize.FASTOCTREE,dither=Image.Dither.NONE)
    output=dest/(name+'.png')
    image.save(output,optimize=True)
    assert Image.open(output).convert('RGBA').getextrema()[3][0]==0
    records.append(dict(master=source.relative_to(here).as_posix(),delivery=output.relative_to(root).as_posix(),size=size,bytes=output.stat().st_size,sha256=hashlib.sha256(output.read_bytes()).hexdigest()))
(here/'image-delivery.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
print(json.dumps(records,indent=2))
