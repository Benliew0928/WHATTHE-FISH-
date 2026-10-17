"""Copy approved 2D menu masters and record portable source integrity.

Run with Python 3 from any working directory. No imaging library is required;
Unity slices the card atlas and handles platform texture compression.
"""
from pathlib import Path
import hashlib
import json
import shutil
import struct

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "ArtSource/SportyMenu"
DELIVERY = ROOT / "Game/Assets/_Game/Resources/SportyMenu"
PAIRS = (("SportCards-master.png", "SportCards.png"), ("Mascot-master.png", "Mascot.png"))


def main():
    DELIVERY.mkdir(parents=True, exist_ok=True)
    records = []
    for master, asset in PAIRS:
        source, destination = ART / master, DELIVERY / asset
        data = source.read_bytes()
        if data[:8] != b"\x89PNG\r\n\x1a\n":
            raise ValueError(f"Expected PNG: {master}")
        width, height = struct.unpack(">II", data[16:24])
        if master.startswith("SportCards") and (width, height) != (1536, 1024):
            raise ValueError("The four-card atlas must keep its original quadrant layout")
        shutil.copyfile(source, destination)
        for path in (source, destination):
            records.append(dict(path=path.relative_to(ROOT).as_posix(), width=width, height=height,
                                sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    (ART / "integrity.json").write_text(json.dumps(dict(base="repository root", files=records), indent=2)+"\n", encoding="utf-8")
    print("Sports menu masters and delivery assets verified")


if __name__ == "__main__":
    main()
