"""Build bilingual fonts; pip install fonttools brotli. Run from any directory.

The original Latin outlines, advances and line metrics remain unchanged. Noto
Sans SC supplies only codepoints absent from each original face. Masters and
licenses are retained under ArtSource/LocalizationFonts.
"""
from pathlib import Path
import tempfile
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont
from fontTools import subset

ROOT = Path(__file__).resolve().parents[2]
RES = ROOT / 'Game/Assets/_Game/Resources'
FACES = [('MenuText','Menu/CoveText.ttf',600),('MenuDisplay','Menu/CoveDisplay.ttf',900),
         ('SportText','SportsUI/Text.ttf',600),('SportDisplay','SportsUI/Display.ttf',800),('MenuHeading','SportyMenu/BarlowDisplay.ttf',800)]

def main():
    out = RES/'Localization/Fonts'
    out.mkdir(parents=True,exist_ok=True)
    with tempfile.TemporaryDirectory() as work:
        for role, original, weight in FACES:
            base = TTFont(RES/original)
            donor = instantiateVariableFont(TTFont(ROOT/'ArtSource/LocalizationFonts/NotoSansSC-Variable.ttf'),{'wght':weight},inplace=True)
            options = subset.Options();options.layout_features=[]
            sub = subset.Subsetter(options=options)
            sub.populate(unicodes=set(donor.getBestCmap())-set(base.getBestCmap()))
            sub.subset(donor)
            from fontTools.pens.ttGlyphPen import TTGlyphPen
            from fontTools.pens.recordingPen import DecomposingRecordingPen
            from fontTools.pens.transformPen import TransformPen
            from fontTools.ttLib.tables._c_m_a_p import CmapSubtable
            # Append decomposed CJK outlines without changing any original glyph.
            merged=base
            glyphs=donor.getGlyphSet();order=list(merged.getGlyphOrder());mapping=dict(base.getBestCmap())
            scale=base['head'].unitsPerEm/donor['head'].unitsPerEm
            added={}
            for cp,name in donor.getBestCmap().items():
                if cp in mapping:continue
                if name not in added:
                    targetName='cjk_'+name;pen=TTGlyphPen(None)
                    recording=DecomposingRecordingPen(glyphs);glyphs[name].draw(recording)
                    recording.replay(TransformPen(pen,(scale,0,scale*.12 if role=='MenuHeading' else 0,scale,0,0)))
                    merged['glyf'][targetName]=pen.glyph()
                    aw,lsb=donor['hmtx'][name];merged['hmtx'][targetName]=(round(aw*scale),round(lsb*scale))
                    order.append(targetName);added[name]=targetName
                mapping[cp]=added[name]
            merged.setGlyphOrder(order)
            for table in merged['cmap'].tables:
                if table.isUnicode():table.cmap={cp:name for cp,name in mapping.items() if cp<=0xffff} if table.format==4 else dict(mapping)
            if not any(t.format==12 for t in merged['cmap'].tables):
                table=CmapSubtable.newSubtable(12);table.platformID=3;table.platEncID=10;table.language=0;table.cmap=mapping
                merged['cmap'].tables.append(table)
            family='Island Bilingual '+role
            for platform,enc,lang in [(3,1,0x409),(1,0,0)]:
                for ident,value in [(1,family),(2,'Regular'),(3,family+' 1.0'),(4,family),(6,family.replace(' ',''))]:
                    merged['name'].setName(value,ident,platform,enc,lang)
            target=out/(role+'.ttf');merged.save(target)
            verified=TTFont(target);base=TTFont(RES/original)
            assert all(verified['hmtx'][verified.getBestCmap()[cp]]==base['hmtx'][glyph] for cp,glyph in base.getBestCmap().items())
            assert ord('中') in verified.getBestCmap()
            print(role,target.stat().st_size,'bytes; Latin advances preserved')

if __name__=='__main__':main()
