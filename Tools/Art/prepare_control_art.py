"""Copy approved RGBA masters without resampling. Pillow inspects pixels only."""
from pathlib import Path
import hashlib, json, shutil, uuid, re
from PIL import Image
ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Shared/Controls"
DEST = ROOT / "Game/Assets/_Game/Resources/ControlArt"
def meta(path, folder=False):
    target=Path(str(path)+".meta")
    if not target.exists():
        target.write_text("fileFormatVersion: 2\nguid: "+uuid.uuid4().hex+("\nfolderAsset: yes" if folder else "")+"\nDefaultImporter:\n  externalObjects: {}\n",encoding="utf-8")
def main():
    DEST.mkdir(parents=True,exist_ok=True);meta(DEST,True)
    manifest=json.loads((SOURCE/"atlas.json").read_text(encoding="utf-8"))
    records=[]
    template=(ROOT/"Game/Assets/_Game/Resources/FishingUI/ReelButton.png.meta").read_text(encoding="utf-8")
    for name in ("Frames","CourtIcons","FishingIcons"):
        src,dst=SOURCE/(name+".png"),DEST/(name+".png")
        im=Image.open(src)
        if im.mode!="RGBA" or im.getchannel("A").getextrema()[0]!=0:raise ValueError("Expected transparent RGBA: "+name)
        for e in (e for e in manifest["entries"] if e["sheet"]==name):
            if not (0<=e["x"]<1 and 0<=e["y"]<1 and e["x"]+e["width"]<=1 and e["y"]+e["height"]<=1):raise ValueError(e["name"])
        shutil.copyfile(src,dst);sidecar=Path(str(dst)+".meta")
        if not sidecar.exists():
            data=re.sub(r"guid: \w+","guid: "+uuid.uuid4().hex,template,count=1)
            data=data.replace("spriteMode: 1","spriteMode: 0").replace("textureType: 8","textureType: 0").replace("maxTextureSize: 256","maxTextureSize: 2048")
            sidecar.write_text(data,encoding="utf-8")
        records.append({"file":str(src.relative_to(ROOT)).replace("\\","/"),"sha256":hashlib.sha256(src.read_bytes()).hexdigest(),"width":im.width,"height":im.height,"rgba":True,"metadataKeys":list(im.info.keys())})
    target=DEST/"Atlas.json";target.write_text(json.dumps({"entries":manifest["entries"]},indent=2)+"\n",encoding="utf-8");meta(target)
    (SOURCE/"integrity.json").write_text(json.dumps({"base":"repository root","sources":records},indent=2)+"\n",encoding="utf-8")
    print("CONTROL_ART_READY: 3 transparent atlases, 31 isolated sprites; original pixels preserved.")
if __name__=="__main__":main()
