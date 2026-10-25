"""Encode actual Unity capture frames; paths resolve from this script's repository.

Requires imageio-ffmpeg. Pass the four successful Test-ActivityCinematic -Film
run folders with --runs. Clips are 30 captured simulation frames per second;
they demonstrate behavior, not physical-device frame rate.
"""
import argparse
import html
import json
from pathlib import Path
import subprocess
import imageio_ffmpeg

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--runs', nargs='+', required=True)
parser.add_argument('--output', default='Builds/ActivityCinematicsQA/Videos')
args = parser.parse_args()
out = ROOT / args.output
out.mkdir(parents=True, exist_ok=True)
ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
records = []
reels = []
for run_arg in args.runs:
    run = ROOT / run_arg
    result = (run / 'results.txt').read_text(encoding='utf-8-sig')
    if 'ACTIVITY_CINEMATIC_COMPLETE success=True' not in result or '\nFAIL ' in result:
        raise RuntimeError(f'Only successful reviewed runs may be encoded: {run_arg}')
    clips = []
    for row in (run / 'clips.tsv').read_text().splitlines():
        name, count, fps, camera_moved = row.split('\t')
        frames = sorted((run / name).glob('*.jpg'))
        if len(frames) != int(count):
            raise RuntimeError(f'Incomplete recording: {name}')
        movie = out / (name + '.mp4')
        subprocess.run([ffmpeg, '-hide_banner', '-loglevel', 'error', '-y',
                        '-framerate', fps, '-i', str(run / name / '%05d.jpg'),
                        '-frames:v', count, '-c:v', 'libx264', '-threads', '4',
                        '-preset', 'fast', '-crf', '20', '-pix_fmt', 'yuv420p',
                        '-movflags', '+faststart', str(movie)], check=True)
        actual, seconds = imageio_ffmpeg.count_frames_and_secs(str(movie))
        if actual != int(count) or abs(seconds - int(count) / int(fps)) > .1:
            raise RuntimeError(f'Encoded duration/count mismatch: {name}')
        records.append(dict(name=name, file=movie.name, frames=actual, seconds=seconds,
                            bytes=movie.stat().st_size, source=run.relative_to(ROOT).as_posix()))
        clips.append(movie)
    sport = clips[0].stem.split('-')[0]
    # The controlled generated names contain no shell expressions or apostrophes.
    manifest = out / (sport + '-concat.txt')
    manifest.write_text(''.join(f"file '{clip.name}'\n" for clip in clips), encoding='utf-8')
    reel = out / (sport + '-all-animations.mp4')
    subprocess.run([ffmpeg, '-hide_banner', '-loglevel', 'error', '-y', '-f', 'concat',
                    '-safe', '1', '-i', str(manifest), '-c', 'copy', '-movflags', '+faststart',
                    str(reel)], check=True)
    actual, seconds = imageio_ffmpeg.count_frames_and_secs(str(reel))
    expected = sum(r['frames'] for r in records if r['source'] == run.relative_to(ROOT).as_posix())
    if actual != expected or abs(seconds - expected / 30) > .1:
        raise RuntimeError(f'Reel duration/count mismatch: {sport}')
    reels.append(dict(sport=sport, file=reel.name, frames=actual, seconds=seconds,
                      bytes=reel.stat().st_size, clips=[c.name for c in clips]))
(out / 'manifest.json').write_text(json.dumps(records, indent=2), encoding='utf-8')
(out / 'reels.json').write_text(json.dumps(reels, indent=2), encoding='utf-8')
labels = dict(Turbo='Team Turbo', Portal='Portal Steal', Wall='Wall', LockOn='Lock-on Shot',
              SteelWire='Steel Wire', PowerReel='Power Reel', FishingBuddy='Fishing Buddy',
              BubbleGuard='Bubble Guard', Sonar='Sonar', SchoolCall='School Call',
              TropicalWind='Tropical Wind', StickyGreen='Sticky Green', BounceParty='Bounce Party',
              Tornado='Tornado', CrabCrossing='Crab Crossing', FinalRush='Final Rush')
def title(name):
    sport, key = name.split('-', 1)
    return sport + ' · ' + labels.get(key, key)
cards = ''.join(f'<article><h2>{html.escape(title(r["name"]))}</h2>'
                f'<video controls preload="metadata" playsinline src="{r["file"]}"></video>'
                f'<a href="{r["file"]}" download>Download 5-second clip</a></article>' for r in records)
(out / 'index.html').write_text('''<!doctype html><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Four-island activation animations</title><style>
body{margin:0;background:#071b29;color:#f3f2e8;font:16px system-ui;padding:32px}main{max-width:1200px;margin:auto}
h1{font-size:32px}p{color:#b6d1dc;line-height:1.6}section{display:grid;grid-template-columns:repeat(auto-fit,minmax(320px,1fr));gap:20px}
article{border:1px solid #285060;border-radius:18px;overflow:hidden;background:#0d293a;padding:16px}h2{font-size:18px}
video{width:100%;border-radius:10px;background:#000}a{color:#65e7cf;display:inline-block;padding-top:12px}
</style><main><h1>Four islands · activation animations</h1><p>Actual game-camera and HUD recordings.
Each clip shows the lead-in, activation and camera transition. Court clips show held icons before use;
Lock-on retains the normal camera. Fishing has six skill presentations. Golf announces five course events and Final Rush.
Captured at 30 simulation frames per second; this is not a phone FPS test. Clips are silent.</p><section>'''
                         + cards + '</section></main>', encoding='utf-8')
print(json.dumps(dict(clips=len(records), output=out.relative_to(ROOT).as_posix(),
                      bytes=sum(r['bytes'] for r in records)), indent=2))
