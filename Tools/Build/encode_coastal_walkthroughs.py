"""Encode and fully decode-check the EXE route frames before archival."""
from pathlib import Path
import hashlib,json,subprocess
import imageio_ffmpeg

root=Path(__file__).resolve().parents[2]
review=root/'Docs/VisualDirection/CoastalStadiums/GameReview'
ffmpeg=imageio_ffmpeg.get_ffmpeg_exe()
metadata={}
for sport in ['Football','Basketball']:
    frames=sorted((review/(sport+'_WalkthroughFrames')).glob('*.jpg'))
    assert frames and all(p.stem==f'{i:05d}' for i,p in enumerate(frames))
    movie=review/(sport+'_Walkthrough.mp4')
    subprocess.run([ffmpeg,'-hide_banner','-loglevel','error','-y','-framerate','30','-i',str(frames[0].parent/'%05d.jpg'),'-frames:v',str(len(frames)),'-c:v','libx264','-threads','4','-preset','fast','-crf','20','-pix_fmt','yuv420p','-movflags','+faststart',str(movie)],check=True)
    count,seconds=imageio_ffmpeg.count_frames_and_secs(str(movie))
    assert count==len(frames),(sport,count,len(frames))
    metadata[sport]={'frames':count,'fps':30,'seconds':seconds,'resolution':[1280,720],'bytes':movie.stat().st_size,'sha256':hashlib.sha256(movie.read_bytes()).hexdigest().upper(),'full_decode_verified':True}
    print(sport,count,'frames;',seconds,'seconds; full decode passed',flush=True)
(review/'video-metadata.json').write_text(json.dumps(metadata,indent=2)+'\n')
