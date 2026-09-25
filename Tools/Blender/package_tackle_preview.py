"""Create tackle review media from saved Blender and optional player captures."""
import argparse
from pathlib import Path
from PIL import Image, ImageDraw
import imageio_ffmpeg

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Builds/TackleQA'


def video(path, frames, fps):
    writer = imageio_ffmpeg.write_frames(str(path), frames[0].size, fps=fps, codec='libx264', quality=8, macro_block_size=1)
    writer.send(None)
    for frame in frames:
        writer.send(frame.convert('RGB').tobytes())
    writer.close()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--gameplay', type=Path)
    args = parser.parse_args()
    sheet = Image.new('RGB', (1600, 1056), '#15292b')
    draw = ImageDraw.Draw(sheet)
    for row, view in enumerate(['three-quarter','side','front']):
        for col, frame in enumerate([1,6,13,22,52]):
            im = Image.open(OUT/'Slide_Tackle'/f'{view}-{frame:04}.png').convert('RGB')
            sheet.paste(im.resize((320,320)), (col*320,row*352+32))
            draw.text((col*320+12,row*352+10),f'{view} / {(frame-1)/60:.2f}s',fill='white')
    sheet.save(OUT/'tackle-contact-sheet.jpg',quality=92)
    for action, end in [('Slide_Tackle',52),('Tackle_Hit',28)]:
        frames = [Image.open(OUT/action/f'three-quarter-{f:04}.png').convert('RGB') for f in range(1,end,2)]
        sequence = ([frames[0]]*10+frames+[frames[-1]]*15)*3
        video(OUT/(action+'-preview.mp4'), sequence, 30)
        video(OUT/(action+'-half-speed.mp4'), sequence, 15)
    if args.gameplay:
        paths = sorted((args.gameplay/'frames').glob('frame-*.png'))
        if not paths:
            raise RuntimeError('No recorded gameplay frames')
        writer = imageio_ffmpeg.write_frames(str(OUT/'football-tackle-gameplay.mp4'),Image.open(paths[0]).size,
                                             fps=30,codec='libx264',quality=8,macro_block_size=1)
        writer.send(None)
        for path in paths:
            with Image.open(path) as frame:
                writer.send(frame.convert('RGB').tobytes())
        writer.close()
    print('TACKLE_PREVIEW_READY',OUT)


if __name__=='__main__':
    main()
