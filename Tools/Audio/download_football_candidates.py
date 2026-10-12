"""Download the football audition sources, keeping all output outside Unity.

Requires requests. Public Freesound HQ MP3 previews are audition copies;
original WAV downloads require login. Only sources verified as CC0 are accepted.
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
    ('kick', 'bittermelonheart', 555042, 'Actual soccer ball kick'),
    ('kick_alt', 'Luisa_Sanchez', 813402, 'Actual soccer ball kick; comparison take'),
    ('pillow', 'padpad', 515908, 'Pillow impacts used by author as soccer kick foley'),
    ('grass', 'Fission9', 521587, 'Hiking boot footsteps layered with grass movement by author'),
    ('trail', 'Luisa_Sanchez', 816841, 'Trail running; comparison rather than grass-cleat claim'),
    ('net', 'Luisa_Sanchez', 816989, 'Soccer net recorded with a contact microphone'),
    ('pipe', 'Anthousai', 406556, 'Metal pipe struck by wooden rod; goal-frame design layer'),
    ('whistle', 'Musik-Fan', 631849, 'Single blown sports whistle'),
]


def download(output):
    root = output.resolve()
    for name in ('Raw', 'Evidence'):
        (root/name).mkdir(parents=True, exist_ok=True)

    def one(spec):
        key, author, sid, description = spec
        page_url = f'https://freesound.org/people/{author}/sounds/{sid}/'
        page = get(page_url).text
        if 'creativecommons.org/publicdomain/zero/1.0' not in page:
            raise ValueError('CC0 not verified: '+page_url)
        links = re.findall(r'https://cdn\.freesound\.org/previews/[^\s"<>]+-hq\.mp3', page)
        if not links:
            raise ValueError('No public HQ preview: '+page_url)
        data = get(links[0]).content
        relative = f'Raw/{key}-{sid}-hq.mp3'
        (root/relative).write_bytes(data)
        (root/f'Evidence/freesound-{sid}.html').write_text(page, encoding='utf-8')
        print(f'{key}: {len(data)} bytes', flush=True)
        return dict(id=key, author=author, source_id=sid, description=description,
                    source_page=page_url, download_url=links[0], license='CC0-1.0',
                    license_url='https://creativecommons.org/publicdomain/zero/1.0/',
                    source_quality='Public HQ MP3 audition copy; original WAV requires login',
                    file=relative, bytes=len(data), sha256=digest(data))

    with ThreadPoolExecutor(max_workers=4) as pool:
        records = list(pool.map(one, SOUNDS))
    (root/'sources.json').write_text(json.dumps(dict(
        base='Paths relative to this manifest directory',
        downloaded_at_utc=datetime.now(timezone.utc).isoformat(), sources=records),
        indent=2)+'\n', encoding='utf-8')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, default=ROOT/'Builds/FootballAudioPreview')
    download(parser.parse_args().output)
