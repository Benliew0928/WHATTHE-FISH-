"""Derive the sports HUD's static, compact OFL fonts. Requires fontTools."""
from pathlib import Path
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont
from fontTools import subset

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/SportsUI/Fonts/Manrope-Source.ttf"
OUTPUT = ROOT / "Game/Assets/_Game/Resources/SportsUI"
OUTPUT.mkdir(parents=True, exist_ok=True)
for weight, label in [(600, "Text"), (800, "Display")]:
    font = instantiateVariableFont(TTFont(SOURCE), {"wght": weight}, inplace=True)
    options = subset.Options()
    options.name_IDs = [0, 1, 2, 3, 4, 5, 6, 13, 14]
    sub = subset.Subsetter(options=options)
    sub.populate(unicodes=list(range(32, 256)) + list(range(0x2000, 0x2070)) + list(range(0x2190, 0x2200)))
    sub.subset(font)
    for record in font["name"].names:
        names = {1: "Cove Sport " + label, 2: "Regular", 3: "CoveSport-" + label,
                 4: "Cove Sport " + label, 6: "CoveSport-" + label}
        if record.nameID in names:
            record.string = names[record.nameID].encode(record.getEncoding())
    target = OUTPUT / (label + ".ttf")
    font.recalcTimestamp = False
    font.save(target)
    print(target.relative_to(ROOT), target.stat().st_size)
