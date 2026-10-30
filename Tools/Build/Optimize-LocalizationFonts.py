"""Remove optional glyph names without changing any font rendering table.

Keeps every codepoint, outline, hint, metric and shaping table. Run from any
directory; generated validation records belong under ignored Builds/.
"""
import argparse
import hashlib
import json
from pathlib import Path
from fontTools.ttLib import TTFont

ROOT = Path(__file__).resolve().parents[2]
FONTS = ROOT / 'Game/Assets/_Game/Resources/Localization/Fonts'


def optimize(path):
    before = path.read_bytes()
    font = TTFont(path, recalcTimestamp=False)
    tables = {tag: font.getTableData(tag) for tag in font.keys() if tag != 'GlyphOrder'}
    font['post'].formatType = 3.0
    temporary = path.with_suffix('.optimized.tmp')
    try:
        font.save(temporary)
        with TTFont(temporary, recalcTimestamp=False) as verified:
            assert set(verified.keys()) == set(font.keys()), path
            for tag, data in tables.items():
                actual = verified.getTableData(tag)
                if tag == 'post':
                    # Format and the glyph-name payload are optional. All
                    # remaining post header fields, including slant, stay exact.
                    assert data[4:32] == actual[4:32], (path, tag)
                elif tag == 'head':
                    # The whole-font checksum changes when post shrinks.
                    assert data[:8] + data[12:] == actual[:8] + actual[12:], (path, tag)
                else:
                    assert data == actual, (path, tag)
        after = temporary.read_bytes()
        assert len(after) <= len(before), path
        if after != before:
            temporary.replace(path)
        return {'path': path.relative_to(ROOT).as_posix(), 'beforeBytes': len(before),
                'afterBytes': len(after), 'beforeSha256': hashlib.sha256(before).hexdigest(),
                'afterSha256': hashlib.sha256(after).hexdigest(),
                'unchangedRenderingTables': sorted(set(tables) - {'head', 'post'})}
    finally:
        font.close()
        temporary.unlink(missing_ok=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--report', default='Builds/FinalOptimizationQA/font-tables.json')
    args = parser.parse_args()
    report = ROOT / args.report
    assert report.resolve().is_relative_to((ROOT / 'Builds').resolve()), 'Report must be under Builds/'
    records = [optimize(p) for p in sorted(FONTS.glob('*.ttf'))]
    report.parent.mkdir(parents=True, exist_ok=True)
    report.write_text(json.dumps(records, indent=2) + '\n', encoding='utf-8')
    print(f'PASS {len(records)} fonts: {sum(x["beforeBytes"] - x["afterBytes"] for x in records):,} source bytes removed; rendering tables identical')


if __name__ == '__main__':
    main()
