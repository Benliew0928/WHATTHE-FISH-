"""Validate the offline catalog and bilingual fonts. Requires fonttools.

Run from any working directory. No Unity installation is needed.
"""
import json
import re
from pathlib import Path
from fontTools.ttLib import TTFont
from fontTools.pens.recordingPen import RecordingPen

ROOT = Path(__file__).resolve().parents[2]
RES = ROOT / 'Game/Assets/_Game/Resources'
entries = json.loads((RES / 'Localization/Catalog.json').read_text(encoding='utf-8'))['entries']
assert len({e['en'] for e in entries}) == len(entries), 'Duplicate English keys'
for entry in entries:
    for language in ('zh', 'ms'):
        assert entry[language].strip(), (entry['en'], language)
        assert sorted(re.findall(r'\{\d+\}', entry['en'])) == sorted(re.findall(r'\{\d+\}', entry[language])), entry
    assert all('{%d}' % term in entry['en'] for term in entry.get('terms', [])), entry
characters = {ord(c) for e in entries for c in e['zh'] if '\u2e80' <= c <= '\uffef'}
faces = [('MenuText', 'Menu/CoveText.ttf'), ('MenuDisplay', 'Menu/CoveDisplay.ttf'),
         ('SportText', 'SportsUI/Text.ttf'), ('SportDisplay', 'SportsUI/Display.ttf'),
         ('MenuHeading', 'SportyMenu/BarlowDisplay.ttf')]
for role, original in faces:
    base = TTFont(RES / original)
    localized = TTFont(RES / 'Localization/Fonts' / (role + '.ttf'))
    a, b = base.getBestCmap(), localized.getBestCmap()
    assert not characters - set(b), (role, characters - set(b))
    ga, gb = base.getGlyphSet(), localized.getGlyphSet()
    for codepoint, name in a.items():
        assert base['hmtx'][name] == localized['hmtx'][b[codepoint]], (role, codepoint, 'metrics')
        before, after = RecordingPen(), RecordingPen()
        ga[name].draw(before)
        gb[b[codepoint]].draw(after)
        assert before.value == after.value, (role, codepoint, 'outline')
    for field in ('ascent', 'descent', 'lineGap'):
        assert getattr(base['hhea'], field) == getattr(localized['hhea'], field), (role, field)
    print(f'PASS {role}: {len(a)} original glyphs unchanged; {len(characters)} Chinese characters covered')
print(f'PASS {len(entries)} complete catalog entries and format tokens')
