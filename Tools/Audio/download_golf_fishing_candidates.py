"""Download CC0 audition sources for Golf/Fishing without changing Unity.

Public Freesound HQ MP3 copies are used; these are not original WAV masters.
Requires requests. Existing verified downloads are reused unless --refresh.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
import json
from pathlib import Path
import re

from download_basketball_candidates import get, digest

ROOT = Path(__file__).resolve().parents[2]
SOUNDS = [
    ('golf_drive', 'Scottrex05', 593483, 'Actual driver striking a golf ball'),
    ('golf_cup', 'inbeeld', 21878, 'Actual golf ball entering a cup at one metre'),
    ('putt', '170129', 408260, 'Recorded putter contacts; select isolated takes'),
    ('rod', 'Mrthenoronha', 371313, 'Bamboo fishing-rod swishes; trim a single cast'),
    ('reel', 'tosha73', 509902, 'Spinning fishing reel'),
    ('drag', 'paulprit', 507070, 'Fly-fishing reel with line pulled out; drag/slip layer'),
    ('line', 'kyles', 450849, 'Nylon line friction and tension foley'),
    ('splash', 'bxyorna', 410335, 'Small water splash / drop'),
    ('waves', 'kkenny101', 852826, 'Gentle ocean wash; quiet shore ambience candidate'),
    ('wood', 'sillygrizzlies', 635058, 'Sneaker footsteps on indoor hardwood; shortened as pier-footstep foley'),
    ('sand', 'Daphne_in_Wonderland', 383488, 'Footsteps on sand beside the sea; retain short isolated grains'),
]


def download(output, refresh=False):
    root = output.resolve()
    for name in ('Raw', 'Evidence'):
        (root/name).mkdir(parents=True, exist_ok=True)
    prior = {}
    if (root/'sources.json').is_file():
        prior = {r['id']: r for r in json.loads((root/'sources.json').read_text(encoding='utf-8'))['sources']}

    def one(spec):
        key, author, sid, description = spec
        if not refresh and key in prior and prior[key].get('source_id') == sid:
            record = prior[key]
            path = (root/record['file']).resolve()
            if path.is_relative_to(root) and path.is_file() and digest(path.read_bytes()) == record['sha256']:
                print(key+': verified cached source', flush=True)
                return record
        url = f'https://freesound.org/people/{author}/sounds/{sid}/'
        page = get(url).text
        if 'creativecommons.org/publicdomain/zero/1.0' not in page:
            raise ValueError('CC0 not verified: '+url)
        links = re.findall(r'https://cdn\.freesound\.org/previews/[^\s"<>]+-hq\.mp3', page)
        if not links:
            raise ValueError('Public HQ preview unavailable: '+url)
        data = get(links[0]).content
        relative = f'Raw/{key}-{sid}-hq.mp3'
        (root/relative).write_bytes(data)
        (root/f'Evidence/freesound-{sid}.html').write_text(page, encoding='utf-8')
        print(f'{key}: {len(data)} bytes', flush=True)
        return dict(id=key, author=author, source_id=sid, description=description,
                    source_page=url, download_url=links[0], license='CC0-1.0',
                    license_url='https://creativecommons.org/publicdomain/zero/1.0/',
                    source_quality='Public HQ MP3 audition copy; original download requires login',
                    file=relative, bytes=len(data), sha256=digest(data))

    with ThreadPoolExecutor(max_workers=4) as pool:
        records = list(pool.map(one, SOUNDS))
    (root/'sources.json').write_text(json.dumps(dict(
        base='Paths relative to this manifest directory',
        downloaded_at_utc=datetime.now(timezone.utc).isoformat(), sources=records),
        indent=2)+'\n', encoding='utf-8')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, default=ROOT/'Builds/GolfFishingAudioPreview')
    parser.add_argument('--refresh', action='store_true')
    args = parser.parse_args()
    download(args.output, args.refresh)
