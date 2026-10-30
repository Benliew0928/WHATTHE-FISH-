"""Validate lossless narration masters and derive mobile Ogg files.

Requires numpy and imageio-ffmpeg. Resolves paths from this script, not cwd.
Generation settings and exact localized scripts are recorded with the masters.
"""
from pathlib import Path
import hashlib
import io
import json
import re
import subprocess
import uuid
import wave

import imageio_ffmpeg
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
MASTERS = ROOT / 'ArtSource/FirstVisitNarration'
DELIVERY = ROOT / 'Game/Assets/_Game/Resources/FirstVisitNarration'


def sha(data):
    return hashlib.sha256(data).hexdigest()


def folder_meta(folder):
    meta = folder.with_name(folder.name + '.meta')
    if not meta.exists():
        meta.write_text('fileFormatVersion: 2\nguid: ' + uuid.uuid4().hex +
                        '\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n'
                        '  userData: \n  assetBundleName: \n  assetBundleVariant: \n', encoding='utf-8')


def main():
    manifest = json.loads((MASTERS / 'manifest.json').read_text(encoding='utf-8'))
    source = (ROOT / 'Game/Assets/_Game/Shared/UI/StorybookMenu.Introduction.cs').read_text(encoding='utf-8')
    scripts = re.findall(r'new IntroductionStep\("((?:[^"\\]|\\.)*)"', source)
    catalog = {e['en']: e for e in json.loads((ROOT / 'Game/Assets/_Game/Resources/Localization/Catalog.json').read_text(encoding='utf-8'))['entries']}
    expected = {(f'{i:02d}', lang): catalog[text][key] for i, text in enumerate(scripts, 1)
                for lang, key in [('en', 'en'), ('zh-Hans', 'zh'), ('ms', 'ms')]}
    clips = {(c['id'], c['language']): c for c in manifest['clips']}
    assert len(scripts) == 15 and len(clips) == len(manifest['clips']) == 45
    assert clips.keys() == expected.keys(), 'All fifteen steps in all three languages are required'
    DELIVERY.mkdir(parents=True, exist_ok=True)
    folder_meta(DELIVERY)
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
    total_seconds = 0
    for identity, clip in sorted(clips.items()):
        assert clip['script'] == expected[identity], ('Script mismatch', identity)
        path = MASTERS / clip['path']
        assert path.resolve().is_relative_to(MASTERS.resolve())
        raw = path.read_bytes()
        assert sha(raw) == clip['sha256'], ('Changed master', identity)
        with wave.open(io.BytesIO(raw)) as reader:
            channels, rate, width = reader.getnchannels(), reader.getframerate(), reader.getsampwidth()
            assert (channels, rate, width) == (1, 24000, 2), identity
            samples = reader.readframes(len(raw) // (channels * width))
        # Speech streaming WAVs use unknown-length RIFF/data sentinels. Rewrite
        # the container through the WAV parser/writer; preserve every PCM byte.
        output = io.BytesIO()
        with wave.open(output, 'wb') as writer:
            writer.setnchannels(channels)
            writer.setframerate(rate)
            writer.setsampwidth(width)
            writer.writeframes(samples)
        normalized = output.getvalue()
        if normalized != raw:
            clip['downloadSha256'] = sha(raw)
            path.write_bytes(normalized)
            clip['sha256'] = sha(normalized)
        pcm = np.frombuffer(samples, dtype='<i2').astype(np.float64) / 32768
        duration = len(pcm) / rate
        peak, rms = float(np.max(np.abs(pcm))), float(np.sqrt(np.mean(pcm * pcm)))
        assert .3 < duration < 30 and peak > .05 and rms > .005, ('Invalid/silent speech', identity)
        total_seconds += duration
        clip.update(seconds=round(duration, 3), sampleRate=rate, channels=channels,
                    pcmSha256=sha(samples), peak=round(peak, 5), rms=round(rms, 5))
        target = DELIVERY / clip['language'] / (clip['id'] + '.ogg')
        target.parent.mkdir(exist_ok=True)
        folder_meta(target.parent)
        if not target.exists() or sha(target.read_bytes()) != clip.get('deliverySha256'):
            subprocess.run([ffmpeg, '-hide_banner', '-loglevel', 'error', '-y', '-i', str(path),
                            '-c:a', 'libvorbis', '-q:a', '6', '-ac', '1', '-ar', '24000', str(target)], check=True)
        clip['delivery'] = target.relative_to(ROOT).as_posix()
        clip['deliverySha256'] = sha(target.read_bytes())
        meta = target.with_name(target.name + '.meta')
        if not meta.exists():
            meta.write_text('fileFormatVersion: 2\nguid: ' + uuid.uuid4().hex + '''
AudioImporter:
  externalObjects: {}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 1
    sampleRateSetting: 2
    sampleRateOverride: 24000
    compressionFormat: 1
    quality: 0.8
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {}
  forceToMono: 1
  normalize: 0
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData:
  assetBundleName:
  assetBundleVariant:
''', encoding='utf-8')
    manifest['normalization'] = 'WAV length headers corrected losslessly; PCM samples unchanged. Mobile derivatives use mono 24 kHz Ogg Vorbis quality 6.'
    (MASTERS / 'manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    report = {'clips': len(clips), 'languages': 3, 'totalSeconds': round(total_seconds, 3),
              'masterBytes': sum(p.stat().st_size for p in MASTERS.rglob('*.wav')),
              'deliverySourceBytes': sum(p.stat().st_size for p in DELIVERY.rglob('*.ogg'))}
    evidence = ROOT / 'Builds/FirstVisitAudioQA'
    evidence.mkdir(exist_ok=True)
    (evidence / 'audio-integrity.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print('NARRATION_ASSETS_PASS', json.dumps(report))


if __name__ == '__main__':
    main()
