"""Colour and light the retained geometric utility glyphs; no bitmap editing."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Docs/SportsUI/icons"
OUTPUT = ROOT / "Docs/SportsUI/color-icons"
OUTPUT.mkdir(parents=True, exist_ok=True)

PALETTES = {
    "book": ("#87EFFF", "#228ECD", "#205892"),
    "close": ("#FFB59C", "#EF7657", "#BA423C"),
    "court": ("#A4EFCE", "#2AAFA5", "#187790"),
    "exit": ("#FFCE9A", "#F39156", "#BF5050"),
    "eye": ("#A1F5F5", "#38B6CE", "#227CB6"),
    "follow": ("#FFF0A5", "#F5B849", "#D18030"),
    "gear": ("#9BF3FF", "#37B9D5", "#287BA6"),
    "move": ("#9AECF6", "#3CAEC7", "#267392"),
    "points": ("#FFF3B0", "#FFC750", "#DD8925"),
    "hand-swipe-left": ("#FFE2B6", "#F6B77F", "#D88452"),
    "target": ("#BAF7FA", "#49C4D6", "#2875B7"),
    "flag-checkered": ("#AEF7EF", "#39BEAB", "#2475A2"),
    "users-three": ("#B8E5FF", "#569EE3", "#396BB5"),
    "lock-on": ("#FFF1A6", "#FDB74F", "#D88332"),
    "turbo": ("#FFF8A0", "#FFD04D", "#F39824"),
    "portal": ("#D6C8FF", "#A585EC", "#6553B8"),
}

for name, (light, face, dark) in PALETTES.items():
    source = (SOURCE / (name + ".svg")).read_text(encoding="utf-8")
    paths = "".join(re.findall(r"<path\b[^>]+/>", source))
    # Material's 960-unit viewBox maps into a 120-unit illustration with a
    # small extrusion, dark blue contour and restrained top-light reflection.
    accent = ""
    if name == "gear":
        accent = '<circle cx="480" cy="-480" r="165" fill="none" stroke="#FFD06E" stroke-width="48"/><circle cx="480" cy="-480" r="139" fill="#FFF1D0" stroke="#287B9A" stroke-width="20"/>'
    elif name in ("follow", "target", "lock-on"):
        accent = '<circle cx="480" cy="-480" r="112" fill="url(#blue)" stroke="#215E86" stroke-width="18"/><ellipse cx="451" cy="-522" rx="48" ry="25" fill="#D6FAFF" opacity=".85"/>'
    elif name == "portal":
        accent = '<circle cx="480" cy="-480" r="173" fill="url(#blue)" stroke="#C6FAFC" stroke-width="20"/><ellipse cx="437" cy="-530" rx="68" ry="30" fill="#D6FAFF" opacity=".6"/>'
    elif name == "book":
        accent = '<path d="M620-801h92v245l-46-35-46 35Z" fill="#FFC356" stroke="#C18638" stroke-width="12" stroke-linejoin="round"/>'
    elif name == "points":
        accent = '<path d="m480-674 30 61 67 10-49 47 12 68-60-32-60 32 12-68-49-47 67-10Z" fill="#FAF0BE" stroke="#D19433" stroke-width="12" stroke-linejoin="round"/>'
    svg = f'''<svg xmlns="http://www.w3.org/2000/svg" width="128" height="128" viewBox="0 0 128 128">
<defs><linearGradient id="face" x1="0" y1="0" x2=".65" y2="1"><stop stop-color="{light}"/><stop offset=".38" stop-color="{face}"/><stop offset="1" stop-color="{dark}"/></linearGradient><linearGradient id="blue" x1="0" y1="0" x2=".5" y2="1"><stop stop-color="#92F2FC"/><stop offset=".5" stop-color="#40BFD8"/><stop offset="1" stop-color="#2477AD"/></linearGradient></defs>
<g transform="translate(6 3) scale(.12) translate(0 960)" stroke-linejoin="round">
<g transform="translate(0 29)" fill="{dark}" stroke="#224E6A" stroke-width="26">{paths}</g>
<g fill="url(#face)" stroke="#24637F" stroke-width="15">{paths}</g>
{accent}</g></svg>
'''
    (OUTPUT / (name + ".svg")).write_text(svg, encoding="utf-8")

# Selection check intentionally remains a tintable mask on a coloured badge.
(OUTPUT / "check.svg").write_text((SOURCE / "check.svg").read_text(encoding="utf-8").replace("#123F50", "#FFFFFF"), encoding="utf-8")
print("SPORTS_UTILITY_COLOUR_OK:", len(PALETTES), "coloured utility icons + check mask")
