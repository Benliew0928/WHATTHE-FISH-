"""Slice the supplied transparent medal sheet without redrawing its artwork."""
from pathlib import Path
import hashlib
import json
import re
import uuid
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/GolfAwards/RankMedals.png"
OUTPUT = ROOT / "Game/Assets/_Game/Resources/GolfUI/Awards"


def guid(path):
    return uuid.uuid5(uuid.NAMESPACE_URL, "whatthefish/" + path.relative_to(ROOT).as_posix()).hex


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    template = (ROOT / "Game/Assets/_Game/Resources/GolfUI/Reference/Aim.png.meta").read_text()
    entries = []
    with Image.open(SOURCE) as sheet:
        assert sheet.mode == "RGBA"
        for index in range(3):
            region = sheet.crop((sheet.width * index // 3, 0, sheet.width * (index + 1) // 3, sheet.height))
            bounds = region.getbbox()
            image = region.crop(bounds)
            target = OUTPUT / f"Place{index + 1}.png"
            image.save(target, optimize=True)
            meta = re.sub(r"(?m)^guid: .*$", "guid: " + guid(target), template)
            meta = re.sub(r"(?m)^  spriteID: .*$", "  spriteID: " + guid(target), meta)
            meta = re.sub(r"spriteBorder: \{[^}]+\}", "spriteBorder: {x: 0, y: 0, z: 0, w: 0}", meta)
            # Match GolfUIAssetImporter: 108px HUD artwork uses 256px delivery
            # textures and ASTC 6x6 on Android; the source PNG stays full size.
            meta = meta.replace("maxTextureSize: 1024", "maxTextureSize: 256")
            meta = meta.replace("textureFormat: 48", "textureFormat: 50")
            target.with_suffix(".png.meta").write_text(meta, encoding="utf-8", newline="\n")
            entries.append({"rank": index + 1, "sourceCellAlphaBounds": bounds,
                            "output": target.relative_to(ROOT).as_posix(),
                            "size": image.size, "sha256": hashlib.sha256(target.read_bytes()).hexdigest()})
    OUTPUT.with_suffix(".meta").write_text("fileFormatVersion: 2\nguid: " + guid(OUTPUT) + "\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")
    (SOURCE.parent / "delivery.json").write_text(json.dumps(entries, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(entries, indent=2))


if __name__ == "__main__":
    main()
