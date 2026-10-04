"""Subset the retained OFL font for the Chinese Golf control labels."""
import argparse
import shutil
from pathlib import Path
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont
from fontTools import subset

parser=argparse.ArgumentParser(); parser.add_argument('--root',type=Path)
root=(parser.parse_args().root or Path(__file__).resolve().parents[2]).resolve()
font=TTFont(root/'ArtSource/Golf/Cart/Fonts/NotoSansSC-Source.ttf')
font=instantiateVariableFont(font,{'wght':500},inplace=True)
options=subset.Options(); options.name_IDs=['*']; options.name_legacy=True
selection=subset.Subsetter(options=options); selection.populate(text='召唤收回驾驶离开瞄准取消 [RE]')
selection.subset(font)
for record in font['name'].names:
    if record.nameID in (1,4,6,16):
        value='GolfCartControls' if record.nameID==6 else 'Golf Cart Controls'
        record.string=value.encode(record.getEncoding())
    elif record.nameID in (2,17): record.string='Medium'.encode(record.getEncoding())
output=root/'Game/Assets/_Game/Resources/GolfCartLabels.ttf'; output.parent.mkdir(parents=True,exist_ok=True)
font.save(output)
shutil.copyfile(root/'ArtSource/Golf/Cart/Fonts/OFL.txt',output.with_name('GolfCartLabels.LICENSE.txt'))
assert set(map(ord,'召唤收回驾驶离开瞄准取消')).issubset(font.getBestCmap())
print('Cart font bytes:',output.stat().st_size)
