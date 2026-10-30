"""Verify the actual self-contained APK against its exact Unity size audit."""
import argparse
import csv
import hashlib
import json
import re
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apk', default='Builds/Android/WhatTheFish-release.apk')
    parser.add_argument('--audit', default='Builds/SizeAudit/latest')
    parser.add_argument('--report', default='Builds/FinalOptimizationQA/apk-integrity.json')
    args = parser.parse_args()
    apk, audit, report = (ROOT / value for value in (args.apk, args.audit, args.report))
    assert report.resolve().is_relative_to((ROOT / 'Builds').resolve()), 'Report must be under Builds/'
    size_source = (ROOT / 'Game/Assets/_Game/Editor/BuildSizeAudit.cs').read_text()
    limit = int(re.search(r'LimitBytes\s*=\s*(\d+)', size_source)[1])
    size = apk.stat().st_size
    assert 0 < size < limit, (size, limit)
    with apk.open('rb') as stream:
        checksum = hashlib.file_digest(stream, 'sha256').hexdigest()
    expected = {r['entry']: (int(r['compressed_bytes']), int(r['uncompressed_bytes']))
                for r in csv.DictReader((audit / 'apk-entries.csv').open(encoding='utf-8-sig'))}
    with zipfile.ZipFile(apk) as package:
        assert package.testzip() is None, 'APK ZIP CRC failed'
        entries = package.infolist()
        assert len(entries) == len({e.filename for e in entries}), 'Duplicate ZIP entries'
        actual = {e.filename: (e.compress_size, e.file_size) for e in entries if not e.is_dir()}
        assert actual == expected, 'APK contents differ from the measured build audit'
        catalog = json.loads(package.read('assets/PackagedContent/catalog.json'))
        assert catalog == json.loads((audit / 'bundle-catalog.json').read_text()), 'Catalog audit differs'
        bundle_name = 'assets/PackagedContent/' + catalog['bundle']
        with package.open(bundle_name) as stream:
            bundle_hash = hashlib.file_digest(stream, 'sha256').hexdigest()
        assert actual[bundle_name][1] == catalog['bytes'] and bundle_hash == catalog['sha256'], 'Scene bundle integrity failed'
        assert {Path(s).stem for s in catalog['scenes']} == {'Bootstrap', 'SkySail_Fishing', 'SkySail_Golf', 'SkySail_Football', 'SkySail_Basketball'}, 'Four-island scene set changed'
        abis = sorted({e.filename.split('/')[1] for e in entries if e.filename.startswith('lib/')})
        assert abis == ['arm64-v8a'], abis
        metadata = package.read('assets/bin/Data/Managed/Metadata/global-metadata.dat')
        reviews = ['OptimizationReview', 'SceneLifecycleReview', 'FirstPersonBodyReview', 'FishingGameplayProbe', 'FirstVisitReview', 'ResponsiveLayoutReview']
        assert all(name.encode() not in metadata for name in reviews), 'Development reviews entered the release'
    result = {'success': True, 'apk': apk.resolve().relative_to(ROOT).as_posix(), 'bytes': size,
              'limitBytes': limit, 'headroomBytes': limit-size, 'sha256': checksum,
              'zipCrcVerified': True, 'exactAuditEntries': len(actual), 'abis': abis,
              'bundle': catalog, 'developmentReviewsExcluded': reviews,
              'largestCompressedEntries': [{'entry': key, 'compressedBytes': value[0], 'uncompressedBytes': value[1]}
                                           for key, value in sorted(actual.items(), key=lambda p: p[1][0], reverse=True)[:20]]}
    report.parent.mkdir(parents=True, exist_ok=True)
    report.write_text(json.dumps(result, indent=2)+'\n', encoding='utf-8')
    print(f'APK_AUDIT_PASS bytes={size} headroom={limit-size} entries={len(actual)} sha256={checksum}')


if __name__ == '__main__':
    main()
