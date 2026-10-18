"""Publish approved Golf/Fishing audio; shared WAVs stay at their existing paths.

Capture the approval once with --capture-preview. Subsequent runs restore the
exact approved WAV masters offline; they do not silently regenerate selections.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path
import shutil

ROOT = Path(__file__).resolve().parents[2]
PREVIEW = ROOT/'Builds/GolfFishingAudioPreview'
MASTERS = ROOT/'ArtSource/Audio/GolfFishing'
RESOURCES = ROOT/'Game/Assets/_Game/Resources'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def portable_evidence(source, destination):
    """Keep readable source/license evidence without executable website scripts."""
    text = source.read_text(encoding='utf-8')
    # Saved third-party scripts can contain service credentials unrelated to the
    # recording license. Offline evidence needs the page text, not executable JS.
    text = re.sub(r'<script\b[^>]*>.*?</script\s*>', '', text,
                  flags=re.IGNORECASE | re.DOTALL)
    text = re.sub(r'''((?:href|src|action)=["'])(/(?!/)[^"']*)''',
                  lambda m: m[1]+'https://freesound.org'+m[2], text)
    text = re.sub(r'<input\b[^>]*name="csrfmiddlewaretoken"[^>]*>', '', text)
    text = re.sub(r'(?m)^[ \t]+$', '', text)
    destination.write_text(text, encoding='utf-8')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture-preview', action='store_true')
    args = parser.parse_args()
    if args.capture_preview:
        if (MASTERS/'approval.json').exists():
            raise ValueError('Approval already captured; review changes before replacing it.')
        manifest = json.loads((PREVIEW/'manifest.json').read_text(encoding='utf-8'))
        sources = json.loads((PREVIEW/'sources.json').read_text(encoding='utf-8'))
        MASTERS.mkdir(parents=True, exist_ok=True)
        for record in sources['sources']:
            src = PREVIEW/record['file']
            if sha(src) != record['sha256']:
                raise ValueError('Source changed: '+record['id'])
            dst = MASTERS/record['file']
            dst.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(src, dst)
            evidence = PREVIEW/'Evidence'/('freesound-'+str(record['source_id'])+'.html' if 'source_id' in record else 'shared-sources.html')
            (MASTERS/'Evidence').mkdir(exist_ok=True)
            portable_evidence(evidence, MASTERS/'Evidence'/evidence.name)
        for item in manifest['items']:
            for variant in item['variants']:
                src = PREVIEW/variant['file']
                dst = MASTERS/variant['file']
                dst.parent.mkdir(parents=True, exist_ok=True)
                variant['sha256'] = sha(src)
                if not item.get('reuse'):
                    shutil.copy2(src, dst)
                else:
                    shared_clip = next(p for bank in ('BasketballAudio', 'FootballAudio', 'GolfAudio')
                                       for p in (RESOURCES/bank).rglob('*.wav') if sha(p) == variant['sha256'])
                    variant['file'] = '../../../'+shared_clip.relative_to(ROOT).as_posix()
        manifest['status'] = 'User approved all sounds and requested integration on 18 October 2026'
        manifest['base'] = 'Paths relative to this approval file; shared WAVs reference the original Unity assets'
        manifest.pop('reels', None)
        (MASTERS/'approval.json').write_text(json.dumps(manifest, indent=2)+'\n', encoding='utf-8')
        (MASTERS/'sources.json').write_text(json.dumps(sources, indent=2)+'\n', encoding='utf-8')
    manifest = json.loads((MASTERS/'approval.json').read_text(encoding='utf-8'))
    shared = {}
    for folder in ('BasketballAudio', 'FootballAudio', 'GolfAudio'):
        for path in (RESOURCES/folder).rglob('*.wav'):
            shared.setdefault(sha(path), path)
    files, cues = [], []
    for item in manifest['items']:
        paths = []
        for variant in item['variants']:
            if item.get('reuse'):
                dest = shared[variant['sha256']]
            else:
                src = MASTERS/variant['file']
                if sha(src) != variant['sha256']:
                    raise ValueError('Approved master changed: '+str(src.relative_to(ROOT)))
                dest = RESOURCES/'GolfFishingAudio'/src.name
                dest.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(src, dest)
            paths.append(dest.relative_to(RESOURCES).with_suffix('').as_posix())
            files.append(dict(cue=item['id'], file=dest.relative_to(ROOT).as_posix(),
                              sha256=sha(dest), shared=bool(item.get('reuse'))))
        cues.append(dict(cue=item['id'], paths=paths))
    (RESOURCES/'GolfFishingAudioBank.json').write_text(json.dumps(dict(cues=cues), indent=2)+'\n', encoding='utf-8')
    (MASTERS/'delivery.json').write_text(json.dumps(dict(base='Repository root', approval=manifest['status'], files=files), indent=2)+'\n', encoding='utf-8')
    sources = json.loads((MASTERS/'sources.json').read_text(encoding='utf-8'))
    notice = ['Golf and Fishing audio', '', manifest['status'], '',
              'Recordings: CC0-1.0, public HQ MP3 source copies. Original uploader pages:', '']
    for s in sources['sources']:
        if 'source_id' not in s:
            continue
        notice += [s['id']+' by '+s['author'], s['source_page'], s['license_url'], '']
    notice += ['Additional effects: original deterministic project synthesis.',
               'Shared sports audio: see Basketball and Football audio notices.',
               'Shared UI: Kenney Interface Sounds, CC0, https://kenney.nl/assets/interface-sounds',
               'Golf GO and walkway: retained user recordings.', '']
    (MASTERS/'LICENSES.txt').write_text('\n'.join(notice), encoding='utf-8')
    (ROOT/'Game/Assets/StreamingAssets/ThirdParty-GolfFishingAudio.txt').write_text('\n'.join(notice), encoding='utf-8')
    print(f'Published {sum(not f["shared"] for f in files)} new clips; {sum(f["shared"] for f in files)} existing clips referenced in place; {len(cues)} cues.')


if __name__ == '__main__':
    main()
