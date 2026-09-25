"""Package saved Blender turn renders and optional in-game captures for review."""
import argparse
from pathlib import Path

from PIL import Image, ImageDraw
import imageio_ffmpeg

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Builds/TurnQA'


def video(path, frames, fps):
    writer = imageio_ffmpeg.write_frames(str(path), frames[0].size, fps=fps,
                                         codec='libx264', quality=8, macro_block_size=1)
    writer.send(None)
    for frame in frames:
        writer.send(frame.convert('RGB').tobytes())
    writer.close()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--gameplay', type=Path)
    args = parser.parse_args()
    rows = [('Turn_Right_180', [1, 9, 17, 25, 33]),
            ('Turn_Left_180', [1, 9, 17, 25, 33]),
            ('Turn_Right_90', [1, 6, 11, 16, 22])]
    sheet = Image.new('RGB', (5*320, 3*352), '#18232d')
    draw = ImageDraw.Draw(sheet)
    for row, (action, indices) in enumerate(rows):
        for col, index in enumerate(indices):
            frame = Image.open(OUT / action / f'three-quarter-{index:04}.png').convert('RGB')
            sheet.paste(frame.resize((320, 320)), (col*320, row*352+32))
            draw.text((col*320+12, row*352+9), f'{action}  {(index-1)/60:.2f}s', fill='white')
    sheet.save(OUT / 'turn-contact-sheet.jpg', quality=92)
    frames = []
    for index in range(1, 34, 2):
        frame = Image.new('RGB', (1024, 544), '#18232d')
        draw = ImageDraw.Draw(frame)
        for col, action in enumerate(['Turn_Left_180', 'Turn_Right_180']):
            frame.paste(Image.open(OUT / action / f'three-quarter-{index:04}.png').convert('RGB'), (col*512, 32))
            draw.text((col*512+16, 10), action+' / authored turn, root facing included', fill='white')
        frames.append(frame)
    sequence = ([frames[0]]*15 + frames + [frames[-1]]*15)*3
    video(OUT / 'turns-three-passes.mp4', sequence, 30)
    video(OUT / 'turns-three-passes-half-speed.mp4', sequence, 15)
    if args.gameplay:
        # Stream captures so a long review does not retain gigabytes of images.
        paths = sorted((args.gameplay / 'frames').glob('frame-*.png'))
        if not paths:
            raise RuntimeError('No gameplay captures in '+str(args.gameplay))
        writer = imageio_ffmpeg.write_frames(str(OUT / 'forward-backward-gameplay.mp4'),
                                             Image.open(paths[0]).size, fps=30,
                                             codec='libx264', quality=8, macro_block_size=1)
        writer.send(None)
        for path in paths:
            with Image.open(path) as frame:
                writer.send(frame.convert('RGB').tobytes())
        writer.close()
    print('TURN_PREVIEW_READY', OUT)


if __name__ == '__main__':
    main()
