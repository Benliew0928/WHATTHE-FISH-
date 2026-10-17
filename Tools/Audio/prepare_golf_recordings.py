"""Derive Golf GO and individual walkway steps from retained user audio masters.

Requires numpy and scipy. Defaults resolve from this script, independent of cwd.
The full stereo 48 kHz WAV masters retain the recordings' original decoded audio.
"""
import hashlib
import json
from pathlib import Path

import numpy as np
from scipy.io import wavfile
from scipy.signal import resample_poly, butter, sosfilt

ROOT = Path(__file__).resolve().parents[2]
MASTERS = ROOT / 'ArtSource/Audio/Golf/Recordings'
OUTPUT = ROOT / 'Game/Assets/_Game/Resources/GolfAudio'
CUTS = [('go', 'go-master.wav', .53, 1.46),
        ('Walkway/step-01', 'walkway-master.wav', .79, 1.14),
        ('Walkway/step-02', 'walkway-master.wav', 1.48, 1.80),
        ('Walkway/step-03', 'walkway-master.wav', 2.20, 2.57)]


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    provenance = json.loads((MASTERS / 'provenance.json').read_text('utf-8-sig'))
    for source in provenance['recordings']:
        if digest(MASTERS / source['master']).lower() != source['master_sha256'].lower():
            raise ValueError('Changed master: ' + source['master'])
    records = []
    for name, master, start, end in CUTS:
        rate, raw = wavfile.read(MASTERS / master)
        if rate != 48000 or raw.dtype != np.int16:
            raise ValueError('Expected retained 48 kHz PCM16 master: ' + master)
        data = raw.astype(np.float64) / 32768
        if data.ndim > 1:
            data = data.mean(axis=1)
        data = data[round(start * rate):round(end * rate)]
        data -= data.mean()
        if name.startswith('Walkway/'):
            data = sosfilt(butter(2, 70, fs=rate, btype='highpass', output='sos'), data)
        data = resample_poly(data, 2, 3)  # retained masters are 48 kHz
        ramp = min(160, len(data) // 8)
        data[:ramp] *= np.linspace(0, 1, ramp)
        data[-ramp:] *= np.linspace(1, 0, ramp)
        data *= .65 / max(.00001, np.max(np.abs(data)))
        dest = OUTPUT / (name + '.wav')
        dest.parent.mkdir(parents=True, exist_ok=True)
        wavfile.write(dest, 32000, np.round(data * 32767).astype(np.int16))
        records.append(dict(file=dest.relative_to(ROOT).as_posix(),
                            master=(MASTERS / master).relative_to(ROOT).as_posix(),
                            start_seconds=start, end_seconds=end,
                            sample_rate=32000, channels=1, sha256=digest(dest)))
    (MASTERS / 'delivery.json').write_text(json.dumps(dict(base='Repository root',
        clips=records), indent=2) + '\n', encoding='utf-8')
    print('Published', len(records), 'recorded Golf cues')


if __name__ == '__main__':
    main()
