"""Publish approved clips; keep original source packs outside Unity.

First capture: --capture-preview. Subsequent fresh-clone regeneration uses only
the retained masters; no network or credentials are needed.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import zipfile

ROOT = Path(__file__).resolve().parents[2]
MASTERS = ROOT / 'ArtSource/Audio/Basketball'
PREVIEW = ROOT / 'Builds/BasketballAudioPreview'
DELIVERY = ROOT / 'Game/Assets/_Game/Resources/BasketballAudio'
EXCLUDED = {'wood-alt', 'squeak-designed', 'ui-soft', 'ball-soft', 'ball-hard', 'dunk'}

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture-preview', action='store_true')
    args = parser.parse_args()
    if args.capture_preview:
        MASTERS.mkdir(parents=True, exist_ok=True)
        sources = json.loads((PREVIEW/'sources.json').read_text())
        for record in sources['sources']:
            dest = MASTERS/record['file']
            dest.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(PREVIEW/record['file'], dest)
        shutil.copy2(PREVIEW/'sources.json', MASTERS/'sources.json')
        for name in ('interface-sounds', 'impact-sounds'):
            license_file = next((PREVIEW/'Raw'/f'kenney-{name}').rglob('License.txt'))
            shutil.copy2(license_file, MASTERS/f'kenney-{name}-license.txt')
    else:
        sources = json.loads((MASTERS/'sources.json').read_text())
        for record in sources['sources']:
            original = MASTERS/record['file']
            if hashlib.sha256(original.read_bytes()).hexdigest() != record['sha256']:
                raise ValueError('Master hash mismatch: '+record['id'])
            dest = PREVIEW/record['file']
            dest.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(original, dest)
            if dest.suffix == '.zip':
                directory = dest.with_suffix('').resolve()
                with zipfile.ZipFile(dest) as archive:
                    if any(not (directory/i.filename).resolve().is_relative_to(directory) for i in archive.infolist()):
                        raise ValueError('Archive path outside extraction directory')
                    archive.extractall(directory)
        shutil.copy2(MASTERS/'sources.json', PREVIEW/'sources.json')
        subprocess.run([sys.executable, str(Path(__file__).with_name('build_basketball_preview.py'))], check=True)
    manifest = json.loads((PREVIEW/'manifest.json').read_text())
    DELIVERY.mkdir(parents=True, exist_ok=True)
    approved = []
    for item in manifest['items']:
        if item['id'] in EXCLUDED:
            continue
        for variant in item['variants']:
            source = PREVIEW/variant['file']
            dest = DELIVERY/source.name
            shutil.copy2(source, dest)
            approved.append(dict(cue=item['id'],file=dest.relative_to(ROOT).as_posix(),
                sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),sources=item['sources'],
                processing=item['processing'],duration=variant['duration']))
    (MASTERS/'delivery.json').write_text(json.dumps(dict(base='Repository root',
        approval='User approved preview, with softer/lower two-second Lock-on aim',
        lock_aim='220-360 Hz fundamental; peak 0.064, 7.96 dB below first preview; duration unchanged',
        reused='Soft/hard bounces use gain on the same four takes. Dunk uses contact-timed rim and net.',
        files=approved),indent=2)+'\n')
    print(f'Published {len(approved)} clips; {sum((ROOT/a["file"]).stat().st_size for a in approved)} WAV bytes')

if __name__ == '__main__':
    main()
