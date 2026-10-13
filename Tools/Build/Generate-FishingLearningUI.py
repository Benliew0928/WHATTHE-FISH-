"""Downsample generated masters into two small alpha atlases (requires Pillow).

All paths resolve from this file; run from any working directory.
"""
from pathlib import Path
import hashlib
import json
import uuid
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Fishing/UI/Learning"
DEST = ROOT / "Game/Assets/_Game/Resources/FishingUI"

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    records = []
    for name, grid, cell in (("GearIcons", (3, 2), (128, 128)),
                             ("TutorialPanels", (2, 2), (256, 128))):
        source = SOURCE / (name + ".png")
        image = Image.open(source).convert("RGBA")
        output_image = Image.new("RGBA", (grid[0]*cell[0], grid[1]*cell[1]))
        for row in range(grid[1]):
            for column in range(grid[0]):
                tile = image.crop((column*image.width//grid[0], row*image.height//grid[1],
                                   (column+1)*image.width//grid[0], (row+1)*image.height//grid[1]))
                # Mechanical atlas slicing/resizing only; preserve the source art.
                tile.thumbnail((cell[0]-8, cell[1]-8), Image.Resampling.LANCZOS)
                output_image.paste(tile, (column*cell[0]+(cell[0]-tile.width)//2,
                                         row*cell[1]+(cell[1]-tile.height)//2))
        output = DEST / (name + ".png")
        output_image.save(output, optimize=True, compress_level=9)
        meta = output.with_suffix(".png.meta")
        if not meta.exists():
            meta.write_text(f"fileFormatVersion: 2\nguid: {uuid.uuid4().hex}\n", encoding="utf-8")
        records.append({"source": source.relative_to(ROOT).as_posix(), "sourceSha256": sha(source),
                        "delivery": output.relative_to(ROOT).as_posix(), "sha256": sha(output),
                        "pixels": list(output_image.size), "pngBytes": output.stat().st_size})
    (SOURCE / "delivery.json").write_text(json.dumps({"base": "repository root", "assets": records}, indent=2)+"\n", encoding="utf-8")
    print(json.dumps(records, indent=2))

if __name__ == "__main__":
    main()
