"""Publish the approved football bank without duplicating shared basketball WAVs.

Use --capture-preview once after approval. Later regeneration uses retained
masters offline. Keep the basketball builder and approved bank available.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
MASTERS = ROOT/'ArtSource/Audio/Football'
PREVIEW = ROOT/'Builds/FootballAudioPreview'
DELIVERY = ROOT/'Game/Assets/_Game/Resources/FootballAudio'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture-preview', action='store_true')
    args = parser.parse_args()
    MASTERS.mkdir(parents=True, exist_ok=True)
    source = PREVIEW if args.capture_preview else MASTERS
    target = MASTERS if args.capture_preview else PREVIEW
    sources = json.loads((source/'sources.json').read_text(encoding='utf-8'))
    sources['sources'] = [s for s in sources['sources'] if s['id'] != 'shared-bank']
    for record in sources['sources']:
        original = source/record['file']
        if hashlib.sha256(original.read_bytes()).hexdigest() != record['sha256']:
            raise ValueError('Source hash mismatch: '+record['id'])
        dest = target/record['file']
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(original, dest)
    (target/'sources.json').write_text(json.dumps(sources,indent=2)+'\n',encoding='utf-8')
    if not args.capture_preview:
        subprocess.run([sys.executable,str(Path(__file__).with_name('build_football_preview.py'))],check=True)
    manifest = json.loads((PREVIEW/'manifest.json').read_text(encoding='utf-8'))
    DELIVERY.mkdir(parents=True, exist_ok=True)
    approved = []
    for item in manifest['items']:
        for variant in item['variants']:
            original = PREVIEW/variant['file']
            if item.get('reuse'):
                dest = ROOT/'Game/Assets/_Game/Resources/BasketballAudio'/original.name
                if original.read_bytes() != dest.read_bytes():
                    raise ValueError('Shared approved sound differs: '+item['id'])
            else:
                dest = DELIVERY/original.name
                shutil.copy2(original,dest)
            approved.append(dict(cue=item['id'],file=dest.relative_to(ROOT).as_posix(),
                sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),sources=item['sources'],
                processing=item['processing'],duration=variant['duration'],shared=bool(item.get('reuse'))))
    (MASTERS/'delivery.json').write_text(json.dumps(dict(base='Repository root',
        approval='User: All approve; 12 October 2026',
        selection='All 80 approved clips. Alternate kick and trail steps join the shot/run variation banks; 25 shared clips referenced in place.',
        files=approved),indent=2)+'\n',encoding='utf-8')
    text=['Football audio source record','', 'New recordings are declared CC0-1.0 on their uploader pages. Public HQ MP3 audition copies are retained, not original WAV downloads.','']
    for record in sources['sources']:
        text.extend([record['id']+' — '+record['author'],record['source_page'],record['license_url'],''])
    text.extend(['Designed effects: original deterministic project synthesis.','Shared basketball effects: see ArtSource/Audio/Basketball/delivery.json.','Shared UI: Kenney Interface Sounds, CC0, https://kenney.nl/assets/interface-sounds',''])
    (MASTERS/'LICENSES.txt').write_text('\n'.join(text),encoding='utf-8')
    notice=ROOT/'Game/Assets/StreamingAssets/ThirdParty-FootballAudio.txt'
    notice.parent.mkdir(parents=True,exist_ok=True)
    notice.write_text('\n'.join(text),encoding='utf-8')
    print(f'Published {sum(not a["shared"] for a in approved)} new clips, referenced {sum(a["shared"] for a in approved)} shared clips')


if __name__ == '__main__':
    main()
