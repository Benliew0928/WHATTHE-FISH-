"""Slice the supplied leaderboard artwork into reusable, live-data UI pieces.

Run from any working directory with Python/Pillow. Originals stay outside Unity.
No generated names, scores or sample progress are shipped in reusable row skins.
"""
from pathlib import Path
import hashlib
import json
import math
from PIL import Image, ImageChops, ImageDraw, ImageOps

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Golf/UI/Leaderboard/Leaderboard-master.png"
OUTPUT = ROOT / "Game/Assets/_Game/Resources/GolfUI/Leaderboard"


def repeated_column(image, left, right, sample):
    """Replace sample data with an untouched blank vertical strip of the same row."""
    image = image.copy()
    image.paste(image.crop((sample, 0, sample + 1, image.height)).resize(
        (right - left, image.height)), (left, 0))
    return image


def rounded(image, radius):
    mask = Image.new("L", image.size)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, image.width-1, image.height-1),
                                          radius=radius, fill=255)
    image = image.copy()
    image.putalpha(mask)
    return image


def main():
    original = Image.open(SOURCE).convert("RGBA")
    OUTPUT.mkdir(parents=True, exist_ok=True)
    records = []

    def save(name, image, width):
        size = (width, round(image.height * width / image.width))
        image = image.convert("RGBa").resize(size, Image.Resampling.LANCZOS).convert("RGBA")
        path = OUTPUT / (name + ".png")
        image.save(path, optimize=True)
        records.append(dict(name=name, size=list(size), png_bytes=path.stat().st_size,
                            sha256=hashlib.sha256(path.read_bytes()).hexdigest()))

    # Keep the illustrated badge, not the cream rectangle/outer body walls
    # behind it. Those walls belong to the single resizable frame below.
    title = original.crop((0, 118, 1122, 500))
    mask = Image.new("L", (1122*4, 382*4))
    silhouette = [(0, 0), (1122, 0), (1122, 245), (1105, 270),
                  (1097, 295), (1089, 320), (1075, 360),
                  (1030, 378), (960, 382), (910, 376),
                  (875, 366), (835, 349), (285, 349), (250, 366),
                  (210, 377), (170, 382), (100, 378), (60, 366),
                  (35, 348), (28, 320), (23, 295), (18, 270), (0, 245)]
    ImageDraw.Draw(mask).polygon([(x*4, y*4) for x, y in silhouette], fill=255)
    mask = mask.resize(title.size, Image.Resampling.LANCZOS)
    title.putalpha(ImageChops.multiply(title.getchannel("A"), mask))
    # The sample's cream body is not part of the illustrated badge outline.
    for y in range(270, title.height):
        for x in range(title.width):
            red, green, blue, alpha = title.getpixel((x, y))
            if red > 235 and green > 215 and blue > 150 and red > green+2 and green > blue+8:
                title.putpixel((x, y), (red, green, blue, 0))
    save("Title", title, 1024)
    # Only the gold pill and its original labels remain; transparent corners
    # must not paint over the outer blue frame.
    save("Columns", rounded(original.crop((40, 506, 1083, 609)), 51), 1024)

    # Reuse the original lower border and an empty horizontal strip for the
    # resizable cream/blue frame, keeping the source border highlights intact.
    bottom = original.crop((0, 1170, 1122, 1282))
    for y in range(1170, 1237):
        # Keep the supplied rounded outer frame; clear only its cream interior.
        inset = math.ceil(125-math.sqrt(max(0, 90**2-(y-1147)**2)))
        bottom.paste(original.crop((inset, 612, 1122-inset, 613)),
                     (inset, y-1170))
    frame = original.crop((0, 612, 1122, 613)).resize((1122, 400))
    frame.paste(ImageOps.flip(bottom), (0, 0))
    frame.paste(bottom, (0, 288))
    save("Frame", frame, 512)

    for name, box in [("LocalRow", (38, 619, 1087, 770)),
                      ("Row", (39, 780, 1087, 926))]:
        row = original.crop(box)
        blank = row.crop((306, 0, 307, row.height)).resize(row.size)
        interior = Image.new("L", row.size)
        ImageDraw.Draw(interior).rounded_rectangle(
            (12, 12, row.width-13, row.height-13), radius=40, fill=255)
        row = Image.composite(blank, row, interior)
        row = rounded(row, 50)
        save(name, row, 512)

    for name, box in [("LocalCell", (360, 654, 529, 742)),
                      ("Cell", (360, 808, 529, 895))]:
        cell = repeated_column(original.crop(box), 24, 148, 24)
        save(name, rounded(cell, 22), 128)

    playing = rounded(original.crop((697, 652, 876, 740)), 43)
    finished = rounded(original.crop((697, 958, 886, 1047)), 43)
    save("Playing", playing, 192)
    save("Finished", finished, 192)
    # DNF is a real runtime state absent from the sample: clear only the word,
    # interpolating adjacent clean source pixels, and draw its live label in UI.
    pending = playing.copy()
    for x in range(27, 157):
        for y in range(23, 75):
            a, b = playing.getpixel((90, 18)), playing.getpixel((90, 79))
            f = (y-18)/61
            pending.putpixel((x, y), tuple(round(v+(w-v)*f) for v, w in zip(a, b)))
    save("Pending", pending, 192)

    boxes = [(57, 637, 191, 762), (58, 791, 192, 918),
             (58, 944, 192, 1073), (58, 1096, 192, 1222)]
    for i, box in enumerate(boxes):
        save("Avatar"+str(i), rounded(original.crop(box), 35), 128)

    audit = dict(base="repository root", master=SOURCE.relative_to(ROOT).as_posix(),
                 master_sha256=hashlib.sha256(SOURCE.read_bytes()).hexdigest(),
                 master_size=list(original.size),
                 method="Original artwork crops, traced title silhouette and transparent column corners, blank-strip repetition for data-free nine-slice skins, proportional premultiplied-alpha downsample. Playing/Finished lettering retained. Sample player data replaced by runtime UI.",
                 assets=records)
    (SOURCE.parent / "delivery-audit.json").write_text(json.dumps(audit, indent=2)+"\n", encoding="utf-8")
    print(json.dumps(audit, indent=2))


if __name__ == "__main__":
    main()
