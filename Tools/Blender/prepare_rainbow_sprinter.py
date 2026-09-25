"""Unpack the approved Meshy download and prepare Unity PBR textures.

Run with the workspace Python before export_rainbow_sprinter.py in Blender.
The original ZIP stays in ArtSource so the generated files are reproducible.
"""
from pathlib import Path
from zipfile import ZipFile

from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Shared/Characters/Meshy"
GAME = ROOT / "Game/Assets/_Game/Art/RainbowSprinter"
ZIP = SOURCE / "RainbowSprinter-source.zip"


def main():
    GAME.mkdir(parents=True, exist_ok=True)
    with ZipFile(ZIP) as archive:
        if archive.testzip() is not None:
            raise RuntimeError("The Meshy source ZIP failed its integrity check")
        entries = {Path(name).name: name for name in archive.namelist() if not name.endswith("/")}
        fbx = next((name for name in entries if name.lower().endswith(".fbx")), None)
        if fbx is None:
            raise RuntimeError("The Meshy source ZIP contains no FBX")
        (SOURCE / "RainbowSprinter-running-original.fbx").write_bytes(archive.read(entries[fbx]))
        suffixes = {
            "_texture_0.png": "albedo.png",
            "_texture_0_normal.png": "normal.png",
            "_texture_0_metallic.png": "metallic.png",
            "_texture_0_roughness.png": "roughness.png",
        }
        for suffix, output in suffixes.items():
            found = next((name for name in entries if name.endswith(suffix)), None)
            if found is None:
                raise RuntimeError(f"Missing Meshy texture: {suffix}")
            data = archive.read(entries[found])
            (SOURCE / output).write_bytes(data)
            if output in ("albedo.png", "normal.png"):
                (GAME / output).write_bytes(data)

    metallic = Image.open(SOURCE / "metallic.png").convert("L")
    roughness = Image.open(SOURCE / "roughness.png").convert("L")
    if metallic.size != roughness.size:
        raise RuntimeError("Metallic and roughness texture dimensions differ")
    black = Image.new("L", metallic.size, 0)
    packed = Image.merge("RGBA", (metallic, black, black,
                                  roughness.point(lambda value: 255 - value)))
    packed.save(GAME / "metallic_smoothness.png")
    print("RAINBOW_TEXTURES_READY", metallic.size, GAME)


if __name__ == "__main__":
    main()
