"""Derive small transparent delivery sprites; keep illustrated masters untouched.

Requires Pillow. Resolve every default from this script, independently of cwd.
The paired left/right controls share each source via a runtime horizontal flip.
"""
from pathlib import Path
import hashlib
import json
import uuid
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / "Game/Assets/_Game/Resources/FishingUI"
SOURCES = {
    "SteerIdle": ("ArtSource/Fishing/UI/Illustrated/SteerIdle.png", (192, 192)),
    "SteerLit": ("ArtSource/Fishing/UI/Illustrated/SteerLit.png", (192, 192)),
    "CounterFrame": ("ArtSource/Fishing/UI/Illustrated/CounterFrame.png", (512, 128)),
    "ReelButton": ("ArtSource/Fishing/UI/Masters/ReelButton.png", (192, 192)),
}

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    DEST.mkdir(parents=True, exist_ok=True)
    records = []
    for name, (relative, size) in SOURCES.items():
        source = ROOT / relative
        image = Image.open(source).convert("RGBA")
        bounds = image.getchannel("A").getbbox()
        if not bounds:
            raise ValueError(f"Empty alpha in {relative}")
        # Technical trimming/downsampling only. Creative work remains in masters.
        if name == "CounterFrame":
            image = image.crop(bounds).resize(size, Image.Resampling.LANCZOS)
        else:
            image.thumbnail(size, Image.Resampling.LANCZOS)
        output = DEST / f"{name}.png"
        image.save(output, optimize=True, compress_level=9)
        metadata = output.with_suffix(".png.meta")
        if not metadata.exists():
            metadata.write_text(f"fileFormatVersion: 2\nguid: {uuid.uuid4().hex}\n", encoding="utf-8")
        records.append({"source": relative, "sourceSha256": sha(source),
                        "delivery": str(output.relative_to(ROOT)).replace("\\", "/"),
                        "sha256": sha(output), "pixels": list(image.size),
                        "pngBytes": output.stat().st_size})
    record = ROOT / "ArtSource/Fishing/UI/Illustrated/delivery.json"
    record.write_text(json.dumps({"base": "repository root", "assets": records}, indent=2)+"\n", encoding="utf-8")
    print(f"DELIVERY_PNG_BYTES={sum(r['pngBytes'] for r in records)}")

if __name__ == "__main__":
    main()
