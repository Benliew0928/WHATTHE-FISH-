"""Download a bounded CC0 audition set; no Unity assets are modified.

Usage: python Tools/Audio/download_basketball_candidates.py [--output PATH]
Requires requests. Output defaults relative to this script's repository root.
Freesound public HQ MP3s are audition copies, not original WAV masters.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import re
import zipfile
import requests

ROOT = Path(__file__).resolve().parents[2]
SOUNDS = [
    ('dribble', 'OBXJohn', 262508),
    ('backboard', 'PerMagnusLindborg', 324247),
    ('rim', 'reedhos', 244348),
    ('swish', 'vhschool2019', 716930),
    ('net_variants', '21100495', 655323),
    ('outdoor', 'fattirewhitey', 381410),
    ('sneakers', 'Greenbird11', 618816),
    ('squeak', 'Christophersaa365', 703188),
    ('skid', 'shatterstars', 492071),
]
PACKS = ['interface-sounds', 'impact-sounds']

def get(url):
    response = requests.get(url, timeout=60)
    response.raise_for_status()
    return response

def digest(data):
    return hashlib.sha256(data).hexdigest()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, default=ROOT/'Builds/BasketballAudioPreview')
    args = parser.parse_args()
    target = args.output.resolve()
    raw = target/'Raw'
    evidence = target/'Evidence'
    raw.mkdir(parents=True, exist_ok=True)
    evidence.mkdir(parents=True, exist_ok=True)

    def download_sound(spec):
        name, author, sid = spec
        page_url = f'https://freesound.org/people/{author}/sounds/{sid}/'
        page = get(page_url).text
        if 'creativecommons.org/publicdomain/zero/1.0' not in page:
            raise ValueError(f'CC0 not present on {page_url}')
        matches = re.findall(r'https://cdn\.freesound\.org/previews/[^\s"<>]+-hq\.mp3', page)
        if not matches:
            raise ValueError(f'No public HQ audition link on {page_url}')
        url = matches[0]
        audio = get(url).content
        filename = f'Raw/{name}-{sid}-hq.mp3'
        (target/filename).write_bytes(audio)
        (evidence/f'freesound-{sid}.html').write_text(page, encoding='utf-8')
        record = dict(id=name, author=author, source_id=sid, source_page=page_url,
                      download_url=url, license='CC0-1.0',
                      license_url='https://creativecommons.org/publicdomain/zero/1.0/',
                      source_quality='Public HQ MP3 audition copy; original WAV requires login',
                      file=filename, bytes=len(audio), sha256=digest(audio))
        print(f'{name}: {len(audio)} bytes (public HQ audition MP3)', flush=True)
        return record

    def download_pack(name):
        page_url = f'https://kenney.nl/assets/{name}'
        page = get(page_url).text
        if 'CC0' not in page:
            raise ValueError(f'CC0 not present on {page_url}')
        matches = re.findall(r'https://kenney\.nl/[^\s\'"<>]+\.zip', page)
        if not matches:
            raise ValueError(f'No pack download on {page_url}')
        url = matches[0]
        content = get(url).content
        filename = f'Raw/kenney-{name}.zip'
        (target/filename).write_bytes(content)
        (evidence/f'kenney-{name}.html').write_text(page, encoding='utf-8')
        extract_dir = (raw/f'kenney-{name}').resolve()
        with zipfile.ZipFile(target/filename) as archive:
            for info in archive.infolist():
                output_path = (extract_dir/info.filename).resolve()
                if not output_path.is_relative_to(extract_dir):
                    raise ValueError('Unsafe archive path')
            archive.extractall(extract_dir)
        print(f'{name}: {len(content)} bytes (original CC0 pack)', flush=True)
        return dict(id=name, author='Kenney', source_page=page_url, download_url=url,
                    license='CC0-1.0', license_url='https://creativecommons.org/publicdomain/zero/1.0/',
                    source_quality='Original publisher pack', file=filename,
                    bytes=len(content), sha256=digest(content))

    # Network reads are independent. The final evidence record is written once.
    with ThreadPoolExecutor(max_workers=4) as pool:
        jobs = [pool.submit(download_sound, s) for s in SOUNDS]
        jobs += [pool.submit(download_pack, p) for p in PACKS]
        records = [job.result() for job in jobs]
    manifest = dict(base='Paths relative to this manifest directory',
                    downloaded_at_utc=datetime.now(timezone.utc).isoformat(), sources=records)
    (target/'sources.json').write_text(json.dumps(manifest, indent=2)+'\n', encoding='utf-8')

if __name__ == '__main__':
    main()
