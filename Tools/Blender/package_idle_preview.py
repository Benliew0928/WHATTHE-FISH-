"""Make a pose sheet and normal/half-speed three-loop videos from Blender previews."""
import argparse
import subprocess
from pathlib import Path

from PIL import Image, ImageDraw
import imageio_ffmpeg

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--label", default="final-loop")
    args = parser.parse_args()
    folder = ROOT / "Builds/IdleQA" / args.label
    frames = sorted(folder.glob("three-quarter-*.png"))
    if len(frames) != 240:
        raise RuntimeError(f"Expected 240 rendered frames, found {len(frames)}")
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
    source = folder / "frames.txt"
    source.write_text("".join(f"file '{p.as_posix()}'\nduration 0.033333333333\n" for p in frames))
    cycle = folder / "cycle.mp4"
    subprocess.run([ffmpeg,"-y","-loglevel","error","-f","concat","-safe","0","-i",str(source),
                    "-an","-r","30","-frames:v","240","-c:v","libx264","-crf","18","-pix_fmt","yuv420p",str(cycle)],check=True)
    for label, scale in [("idle-three-loops",1),("idle-half-speed",2)]:
        subprocess.run([ffmpeg,"-y","-loglevel","error","-stream_loop","2","-i",str(cycle),
                        "-vf",f"setpts={scale}*PTS","-an","-r","30","-c:v","libx264","-crf","18",
                        "-pix_fmt","yuv420p","-movflags","+faststart",str(folder.parent/(label+".mp4"))],check=True)
    selected = [0,30,60,90,120,150,180,210]
    sheet = Image.new("RGB",(4*320,2*350),(245,245,240))
    draw = ImageDraw.Draw(sheet)
    for i,index in enumerate(selected):
        x,y=(i%4)*320,(i//4)*350
        sheet.paste(Image.open(frames[index]).convert("RGB").resize((320,320)),(x,y))
        draw.text((x+12,y+328),f"{index/30:.1f} seconds",fill=(35,45,50))
    sheet.save(folder.parent/"idle-contact-sheet.jpg",quality=94)
    print("IDLE_REVIEW_PACK_READY",folder.parent)


if __name__ == "__main__":
    main()
