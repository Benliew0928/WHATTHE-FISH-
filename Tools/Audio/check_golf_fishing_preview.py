"""Validate audition files; optionally prove offline regeneration after relocation.

All evidence and disposable fixtures stay under the ignored preview directory.
This does not validate gameplay synchronization or claim a human listening review.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import wave

import numpy as np
from build_basketball_preview import FF, SR

ROOT=Path(__file__).resolve().parents[2]


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def check(output, relocate=False):
    root=output.resolve()
    data=json.loads((root/'manifest.json').read_text(encoding='utf-8'))
    qa=json.loads((root/'Evidence/clip-qa.json').read_text(encoding='utf-8'))
    metrics={m['id']:m for m in qa['clips']}
    ids=set();files=set();loop_count=0;retained=0
    for source in data['sources']:
        path=(root/source['file']).resolve()
        assert path.is_relative_to(root) and digest(path)==source['sha256'],source['id']
        if source.get('source_id'):
            assert source['license']=='CC0-1.0'
            evidence=root/f'Evidence/freesound-{source["source_id"]}.html'
            assert 'creativecommons.org/publicdomain/zero/1.0' in evidence.read_text(encoding='utf-8')
    for item in data['items']:
        assert item['id'] not in ids,item['id']
        ids.add(item['id'])
        for variant in item['variants']:
            path=(root/variant['file']).resolve()
            assert path.is_relative_to(root) and path not in files,path
            files.add(path)
            metric=metrics[path.stem]
            assert digest(path)==metric['sha256'],path
            with wave.open(str(path),'rb') as w:
                assert (w.getframerate(),w.getnchannels(),w.getsampwidth())==(SR,1,2)
                frames=w.readframes(w.getnframes())
            x=np.frombuffer(frames,dtype='<i2').astype(float)/32768
            assert .005<len(x)/SR<30 and np.isfinite(x).all(),path
            assert .005<abs(x).max()<.9 and abs(x.mean())<.015,path
            assert not np.any(abs(x)>=.9999),path
            if 'reuse_asset' in metric:
                assert digest(ROOT/metric['reuse_asset'])==digest(path)
                retained+=1
            if item.get('loop'):
                assert abs(x[-1]-x[0])<.012,path
                audition=root/variant['preview_file']
                with wave.open(str(audition),'rb') as w:
                    assert w.readframes(w.getnframes())==frames*3,audition
                loop_count+=1
            else:
                assert max(abs(x[0]),abs(x[-1]))<.004,path
    reel_metrics=[]
    for reel in data['reels']:
        assert all(e['cue'] in ids for e in reel['timeline'])
        path=root/reel['file']
        decoded=subprocess.check_output([FF,'-v','error','-i',str(path),'-ac','1','-ar',str(SR),'-f','f32le','pipe:1'])
        x=np.frombuffer(decoded,dtype='<f4')
        assert np.isfinite(x).all() and abs(x).max()<1,path
        assert abs(len(x)/SR-reel['duration'])<.08,path
        reel_metrics.append(dict(file=reel['file'],sha256=digest(path),duration=len(x)/SR,peak=float(abs(x).max())))
    html=(root/'index.html').read_text(encoding='utf-8')
    assert '__AUDIO_MANIFEST__' not in html
    report=dict(valid=True,clip_count=len(files),cue_count=len(ids),unchanged_approved_clips=retained,
                contiguous_three_cycle_auditions=loop_count,reels=reel_metrics,
                checks='Decoding, file/source hashes, format, duration, no clipping, DC, faded edges, loop seams, source evidence, references. Not perceptual listening.')
    if relocate:
        fixture=root/'Relocated source with spaces'
        if fixture.exists():raise ValueError('Archive the prior relocation fixture before repeating this check.')
        out=fixture/'Builds/Audition'
        for folder in ['Raw','Evidence']:(out/folder).mkdir(parents=True,exist_ok=True)
        # Only raw inputs, reproducible scripts and referenced existing assets.
        for name in ['build_golf_fishing_preview.py','build_basketball_preview.py','golf_fishing_preview.html']:
            target=fixture/'Tools/Audio'/name;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(ROOT/'Tools/Audio'/name,target)
        for bank in ['Basketball','Football','Golf/Recordings']:
            rel=Path('ArtSource/Audio')/bank/'delivery.json';target=fixture/rel;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(ROOT/rel,target)
        for metric in qa['clips']:
            if 'reuse_asset' not in metric:continue
            rel=metric['reuse_asset'];target=fixture/rel;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(ROOT/rel,target)
        shutil.copy2(root/'sources.json',out/'sources.json')
        for source in data['sources']:
            shutil.copy2(root/source['file'],out/source['file'])
        result=subprocess.run([sys.executable,str(fixture/'Tools/Audio/build_golf_fishing_preview.py'),'--output',str(out)],cwd=root/'Evidence',capture_output=True,text=True,check=True)
        produced=[p for folder in ['Clips','Reels','LoopAuditions'] for p in (root/folder).glob('*') if p.is_file()]
        assert all(digest(p)==digest(out/p.relative_to(root)) for p in produced)
        report['relocation']=dict(network_used=False,all_audio_hashes_equal=True,audio_files=len(produced),
            directory='Relocated source with spaces',working_directory='Unrelated Evidence directory',stdout=result.stdout)
    (root/'Evidence/validation.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k not in ('reels','relocation')},indent=2))
    if relocate:print('OFFLINE_RELOCATION_PASS',report['relocation']['audio_files'])


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output',type=Path,default=ROOT/'Builds/GolfFishingAudioPreview')
    parser.add_argument('--relocate',action='store_true')
    args=parser.parse_args()
    check(args.output,args.relocate)
