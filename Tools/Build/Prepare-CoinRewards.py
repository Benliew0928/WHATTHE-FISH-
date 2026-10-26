"""Derive the small transparent runtime sprite from the retained generated master."""
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
master = ROOT / 'ArtSource/CoinRewards/GoldFishCoin-master.png'
target = ROOT / 'Game/Assets/_Game/Resources/Menu/GoldFishCoin.png'
image = Image.open(master).convert('RGBA')
assert image.getextrema()[3] == (0, 255), 'Master must retain true alpha'
image.resize((256, 256), Image.Resampling.LANCZOS).save(target, optimize=True)
print(f'{target.relative_to(ROOT)}: {target.stat().st_size} bytes, 256x256 RGBA')
