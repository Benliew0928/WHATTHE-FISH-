"""Derive compact tutorial sprites from retained AI-generated PNG masters."""
from pathlib import Path
import hashlib
import json
import re
import uuid
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/SportsTutorial"
OUTPUT = ROOT / "Game/Assets/_Game/Resources/IslandTutorial"


def guid(path):
    return uuid.uuid5(uuid.NAMESPACE_URL, "whatthefish/" + path.relative_to(ROOT).as_posix()).hex


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    template = (ROOT / "Game/Assets/_Game/Resources/GolfUI/Reference/Aim.png.meta").read_text()
    entries = []
    for sport in ("Football", "Basketball", "Golf"):
        master = SOURCE / (sport + ".png")
        target = OUTPUT / master.name
        with Image.open(master) as source:
            assert source.mode == "RGBA", "Keep the generated transparency"
            image = source.copy()
            image.thumbnail((768, 320), Image.Resampling.LANCZOS)
            image.save(target, optimize=True, compress_level=9)
        meta = re.sub(r"(?m)^guid: .*$", "guid: " + guid(target), template)
        meta = re.sub(r"maxTextureSize: \d+", "maxTextureSize: 1024", meta)
        # A new full-image sprite, not a shared identity with the template.
        meta = re.sub(r"(?m)^  spriteID: .*$", "  spriteID: " + guid(target), meta)
        target.with_suffix(".png.meta").write_text(meta, encoding="utf-8", newline="\n")
        entries.append({"master":master.relative_to(ROOT).as_posix(), "output":target.relative_to(ROOT).as_posix(), "width":image.width, "height":image.height, "sourceBytes":master.stat().st_size, "bytes":target.stat().st_size, "sha256":hashlib.sha256(target.read_bytes()).hexdigest()})
    folder = OUTPUT.with_suffix(".meta")
    if not folder.exists():
        folder.write_text("fileFormatVersion: 2\nguid: " + guid(OUTPUT) + "\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")
    (SOURCE / "delivery.json").write_text(json.dumps(entries, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(entries, indent=2))


if __name__ == "__main__":
    main()
