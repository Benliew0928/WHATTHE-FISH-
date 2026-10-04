"""Package real Unity motion-review frames; does not generate or retime game assets."""
import argparse
import csv
import os
from pathlib import Path

import imageio_ffmpeg
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--run', type=Path)
    parser.add_argument('--fps', type=int, help='Override recorded capture rate (legacy runs only)')
    args = parser.parse_args()
    runs = sorted((ROOT / 'Builds/FootballMotionQA').glob('Run-*'))
    run = args.run.resolve() if args.run else runs[-1]
    frames = sorted((run / 'frames').glob('*.png'))
    rate_file = run / 'capture-rate.txt'
    fps = int(rate_file.read_text()) if rate_file.exists() else (args.fps or 30) / 2
    if not frames:
        raise RuntimeError('This run has no rendered frames.')
    font_path = Path(os.environ.get('WINDIR', '')) / 'Fonts/arial.ttf'
    font = ImageFont.truetype(str(font_path), 22) if font_path.is_file() else ImageFont.load_default(size=22)
    small = ImageFont.truetype(str(font_path), 16) if font_path.is_file() else ImageFont.load_default(size=16)
    metrics = {int(row['frame']): row for row in csv.DictReader((run / 'motion.csv').open())}
    stages = {}
    sequences = []
    for frame in frames:
        number = int(frame.name.split('-', 1)[0])
        label = metrics[number]['stage']
        stages.setdefault(label, []).append(frame)
        if not sequences or sequences[-1][0] != label:
            sequences.append((label, []))
        sequences[-1][1].append(frame)
    output = run / 'football-motion-review.mp4'
    writer = imageio_ffmpeg.write_frames(str(output), (800, 688), fps=fps,
                                        codec='libx264', pix_fmt_in='rgb24', pix_fmt_out='yuv420p', quality=8)
    writer.send(None)
    for label, files in sequences:
        if label.startswith('take-'):
            continue
        for slow in (False, True):
            for frame in files:
                canvas = Image.new('RGB', (800, 688), '#17212b')
                canvas.paste(Image.open(frame).convert('RGB'), (0, 48))
                draw = ImageDraw.Draw(canvas)
                draw.text((18, 10), label.replace('-', ' ').title(), font=font, fill='white')
                draw.text((620, 13), 'Half speed' if slow else 'Normal speed', font=small, fill='white')
                for _ in range(2 if slow else 1):
                    writer.send(canvas.tobytes())
    writer.close()
    for lod in (0, 1):
        chosen = [(label, files[len(files) // 2]) for label, files in stages.items()
                  if label.startswith('take-') and label.endswith(f'lod{lod}')]
        sheet = Image.new('RGB', (1200, ((len(chosen) + 3) // 4) * 270), '#f0f2f4')
        draw = ImageDraw.Draw(sheet)
        for i, (label, frame) in enumerate(chosen):
            im = Image.open(frame).convert('RGB').resize((300, 240))
            x, y = i % 4 * 300, i // 4 * 270
            sheet.paste(im, (x, y))
            draw.text((x + 8, y + 244), label[5:].rsplit('-lod', 1)[0], font=small, fill='#17212b')
        sheet.save(run / f'football-takes-lod{lod}.jpg', quality=92)
    print(output.relative_to(ROOT) if output.is_relative_to(ROOT) else output)


if __name__ == '__main__':
    main()
