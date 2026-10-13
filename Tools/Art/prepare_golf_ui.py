"""Derive tiny alpha Golf HUD sprites from preserved image-generation masters.

Run with a Python installation that includes Pillow. Paths resolve from this
script, so regeneration also works from an unrelated working directory.
"""

from __future__ import annotations

import hashlib
import json
from pathlib import Path

from PIL import Image, ImageOps


PROJECT_ROOT = Path(__file__).resolve().parents[2]
SOURCE_DIR = PROJECT_ROOT / "ArtSource/Golf/UI"
DELIVERY_DIR = PROJECT_ROOT / "Game/Assets/_Game/Resources/GolfUI"
ASSETS = {
    "GolfShotBadge": 256,
    "GolfCourseBadge": 128,
    "Countdown_1": 256,
    "Countdown_2": 256,
    "Countdown_3": 256,
    "Countdown_4": 256,
    "Countdown_5": 256,
    "Countdown_GO": 256,
}


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def describe(path: Path, image: Image.Image) -> dict:
    alpha = image.getchannel("A")
    return {
        "path": path.relative_to(PROJECT_ROOT).as_posix(),
        "bytes": path.stat().st_size,
        "sha256": digest(path),
        "dimensions": list(image.size),
        "mode": image.mode,
        "alpha_range": list(alpha.getextrema()),
        "alpha_bounds": list(alpha.getbbox()),
    }


def main() -> None:
    DELIVERY_DIR.mkdir(parents=True, exist_ok=True)
    records = []
    for name, edge in ASSETS.items():
        source_path = SOURCE_DIR / f"{name}-master.png"
        destination = DELIVERY_DIR / f"{name}.png"
        with Image.open(source_path) as original:
            source = original.convert("RGBA")
            if source.getchannel("A").getextrema()[0] != 0:
                raise ValueError(f"{name}: master must have a transparent background")
            # Premultiplied resizing avoids dark fringes at translucent outlines.
            if name == "Countdown_GO":
                bbox = source.getbbox()
                pad = 12
                padded_bbox = (max(0, bbox[0] - pad), max(0, bbox[1] - pad), min(source.width, bbox[2] + pad), min(source.height, bbox[3] + pad))
                cropped_source = source.crop(padded_bbox)
                target_w, target_h = 256, int(round(256 * cropped_source.height / cropped_source.width))
                fitted = ImageOps.contain(
                    cropped_source.convert("RGBa"), (target_w - 4, target_h - 4), Image.Resampling.LANCZOS
                ).convert("RGBA")
                delivery = Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0))
                delivery.paste(fitted, ((target_w - fitted.width) // 2, (target_h - fitted.height) // 2))
                raw_astc_bytes = (((target_w + 5) // 6) * ((target_h + 5) // 6)) * 16
            else:
                fitted = ImageOps.contain(
                    source.convert("RGBa"), (edge - 4, edge - 4), Image.Resampling.LANCZOS
                ).convert("RGBA")
                delivery = Image.new("RGBA", (edge, edge), (0, 0, 0, 0))
                delivery.paste(fitted, ((edge - fitted.width) // 2, (edge - fitted.height) // 2))
                raw_astc_bytes = ((edge + 5) // 6) ** 2 * 16
            delivery.save(destination, format="PNG", optimize=True, compress_level=9)
            records.append({
                "name": name,
                "master": describe(source_path, source),
                "delivery": describe(destination, delivery),
                "android_astc_6x6_raw_bytes": raw_astc_bytes,
            })
    audit = {
        "base": "repository root",
        "generation": "built-in image_gen with approved reference and transparent_background=true",
        "delivery": "Aspect-preserving Lanczos premultiplied-alpha downsample with transparent edge padding, optimized lossless PNG; Android ASTC 6x6",
        "apk_size_note": "Raw ASTC and PNG lengths are not actual APK growth; measure a fresh AndroidSubmission build.",
        "assets": records,
        "total_delivery_png_bytes": sum(r["delivery"]["bytes"] for r in records),
        "total_android_astc_6x6_raw_bytes": sum(r["android_astc_6x6_raw_bytes"] for r in records),
    }
    audit_path = SOURCE_DIR / "delivery-audit.json"
    audit_path.write_text(json.dumps(audit, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(audit, indent=2))


if __name__ == "__main__":
    main()
