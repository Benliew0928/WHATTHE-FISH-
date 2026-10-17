"""Prepare the user's original cart button PNGs without redrawing their content.

Run from any directory with Python/Pillow. High-resolution originals remain in
ArtSource; only transparent padding is trimmed before premultiplied downsampling.
"""
from pathlib import Path
import hashlib
import json
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Golf/UI/CartButtons"
OUTPUT = ROOT / "Game/Assets/_Game/Resources/GolfUI/CartButtons"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    records = []
    for name in ("Drive", "Buggy", "Recall"):
        source = SOURCE / f"{name}-master.png"
        target = OUTPUT / f"{name}.png"
        with Image.open(source) as original:
            image = original.convert("RGBA")
            bounds = image.getchannel("A").getbbox()
            if not bounds or image.getchannel("A").getextrema()[0] != 0:
                raise ValueError(f"{name} requires original transparent artwork")
            crop = image.crop(bounds)
            size = (508, round(crop.height * 508 / crop.width))
            resized = crop.convert("RGBa").resize(size, Image.Resampling.LANCZOS).convert("RGBA")
            delivery = Image.new("RGBA", (512, size[1] + 4))
            delivery.paste(resized, (2, 2))
            delivery.save(target, optimize=True)
            records.append({
                "name": name, "master": source.relative_to(ROOT).as_posix(),
                "master_sha256": digest(source), "master_size": list(image.size),
                "alpha_crop": list(bounds), "delivery": target.relative_to(ROOT).as_posix(),
                "delivery_sha256": digest(target), "delivery_size": list(delivery.size),
                "delivery_png_bytes": target.stat().st_size,
                "android_astc_4x4_raw_bytes": ((delivery.width + 3) // 4) * ((delivery.height + 3) // 4) * 16,
            })
    audit = {"base": "repository root", "method": "Original pixels; transparent-padding crop and proportional premultiplied-alpha Lanczos downsample. No redraw, recolour or caption replacement.", "apk_note": "PNG and raw ASTC lengths are not measured APK growth.", "assets": records}
    (SOURCE / "delivery-audit.json").write_text(json.dumps(audit, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(audit, indent=2))


if __name__ == "__main__":
    main()
