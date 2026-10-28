"""Build awards-only contact trims and recorded cheers. Requires FFmpeg on PATH or --ffmpeg."""
from pathlib import Path
import argparse, array, hashlib, json, math, subprocess, wave
ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Game/Assets/_Game/Resources/IslandAwards'

def plants():
    report = []
    for surface, folder, cue in [('Grass','FootballAudio','grass-walk'),('Wood','GolfFishingAudio','wood-steps')]:
        for take in range(1,5):
            source = OUT.parent / folder / f'{cue}-{take:02}.wav'
            with wave.open(str(source)) as f:
                assert f.getnchannels()==1 and f.getsampwidth()==2
                rate=f.getframerate(); samples=array.array('h', f.readframes(f.getnframes()))
            # The original foley has pre-contact rustling (grass: ~100 ms).
            # Align its first strong attack, without altering ordinary gameplay clips.
            window=max(1,round(rate*.005))
            energy=[sum(x*x for x in samples[i:i+window])/window for i in range(0,len(samples),window)]
            attack=next(i for i,e in enumerate(energy) if e>=max(energy)*.4)*window
            start=max(0,attack-round(.005*rate)); data=samples[start:]
            for i in range(min(round(.002*rate),len(data))):data[i]=round(data[i]*i/max(1,round(.002*rate)))
            target=OUT/f'{surface}Plant{take}.wav'
            with wave.open(str(target),'wb') as f:
                f.setparams((1,2,rate,0,'NONE','not compressed')); f.writeframes(data.tobytes())
            report.append(dict(file=str(target.relative_to(ROOT)).replace('\\','/'),trim_seconds=start/rate,sha256=hashlib.sha256(target.read_bytes()).hexdigest()))
    return report

def build(ffmpeg):
    source=ROOT/'ArtSource/Audio/IslandAwards/CrowdReference.mp4'
    target=OUT/'Cheers.wav'
    subprocess.run([ffmpeg,'-v','error','-y','-i',str(source),'-vn','-af','atrim=start=0.15:end=4.8,asetpts=PTS-STARTPTS,highpass=f=80,lowpass=f=10000,volume=3.5,afade=t=in:d=0.08,afade=t=out:st=3.85:d=0.8,alimiter=limit=0.8:level=false','-ar','32000','-ac','1','-map_metadata','-1',str(target)],check=True)
    report=plants();report.append(dict(file=str(target.relative_to(ROOT)).replace('\\','/'),sha256=hashlib.sha256(target.read_bytes()).hexdigest()))
    print(json.dumps(report,indent=2))
if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--ffmpeg',default='ffmpeg');build(parser.parse_args().ffmpeg)
