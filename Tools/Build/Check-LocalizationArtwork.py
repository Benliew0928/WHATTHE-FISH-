"""Validate clean artwork provenance and regenerate its UV alignment metadata.

Requires Pillow. Run from any directory; --write updates Alignment.json only.
This measures image alpha bounds and hashes; it never edits image pixels.
"""
import hashlib
import json
import sys
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
records = json.loads((ROOT / 'ArtSource/LocalizationArtwork/integrity.json').read_text())
entries = []

def alpha_bounds(image):
    x, y, right, bottom = image.getchannel('A').point(lambda a: 255 if a > 127 else 0).getbbox()
    return x / image.width, 1 - bottom / image.height, (right - x) / image.width, (bottom - y) / image.height

for record in records:
    source, master, delivery = (ROOT / record[key] for key in ('source', 'master', 'delivery'))
    assert hashlib.sha256(source.read_bytes()).hexdigest() == record['sourceSha256'], source
    assert hashlib.sha256(master.read_bytes()).hexdigest() == record['cleanSha256'], master
    assert delivery.read_bytes() == master.read_bytes(), delivery
    with Image.open(source) as image:
        original = alpha_bounds(image.convert('RGBA'))
    with Image.open(master) as image:
        clean = alpha_bounds(image.convert('RGBA'))
    sx, sy = clean[2] / original[2], clean[3] / original[3]
    entries.append({'name': delivery.stem, 'uv': {'x': clean[0] - original[0] * sx,
                   'y': clean[1] - original[1] * sy, 'z': sx, 'w': sy}})

alignment = ROOT / 'Game/Assets/_Game/Resources/Localization/Artwork/Alignment.json'
expected = {'entries': entries}
if '--write' in sys.argv:
    alignment.write_text(json.dumps(expected, indent=2) + '\n')
else:
    assert json.loads(alignment.read_text()) == expected, 'Artwork alignment is stale'
print(f'PASS {len(records)} original art hashes, clean masters, delivery copies and alpha-bound alignments')
