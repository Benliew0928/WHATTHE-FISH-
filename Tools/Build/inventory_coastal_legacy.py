"""Inventory authored project files; propose only demonstrably superseded outputs.
Does not move files. Requires Unity dependency audit and passing current EXE QA.
"""
from pathlib import Path
import hashlib,json,datetime,re,os
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Builds/CoastalStadiumAudit';OUT.mkdir(parents=True,exist_ok=True)
QA=ROOT/'Docs/VisualDirection/CoastalStadiums/GameReview/review.txt'
assert QA.exists() and 'STADIUM_REVIEW_COMPLETE' in QA.read_text() and not re.search(r'^FAIL ',QA.read_text(),re.M)
def digest(p):
    h=hashlib.sha256()
    with p.open('rb') as f:
        for b in iter(lambda:f.read(1024*1024),b''):h.update(b)
    return h.hexdigest().upper()
candidates={}
def add(p,why):
    if p.is_dir():
        for child in p.rglob('*'):
            if child.is_file():add(child,why)
    elif p.exists():candidates[p]=why
for line in (OUT/'retired-unity-assets.txt').read_text().splitlines():
    p=ROOT/'Game'/line;add(p,'Retired enclosed basketball roof, suspended scoreboard or lighting; absent from active Unity dependency roots.')
    add(Path(str(p)+'.meta'),'Unity GUID metadata paired with retired asset.')
for p in (ROOT/'ArtSource').rglob('*'):
    if re.search(r'\.blend\d+$',p.name) and p.with_suffix('.blend').exists():add(p,'Automatic Blender backup; current editable source and validated exports retained.')
for p in (ROOT/'Builds').glob('CoastalStadiumQA-*'):add(p,'Intermediate stadium review superseded by final EXE review.')
video_metadata=QA.parent/'video-metadata.json'
if video_metadata.exists():
    videos=json.loads(video_metadata.read_text())
    for sport,video in videos.items():
        movie=QA.parent/(sport+'_Walkthrough.mp4');frames=QA.parent/(sport+'_WalkthroughFrames')
        if frames.exists() and digest(movie)==video['sha256'] and len(list(frames.glob('*.jpg')))==video['frames']:
            add(frames,'Raw capture sequence; verified complete MP4 and frame-count/hash metadata retained in latest review.')
groups={}
retained=[]
documentation='\n'.join(p.read_text(encoding='utf-8',errors='replace') for p in (ROOT/'Docs').rglob('*.md'))+(ROOT/'README.md').read_text(encoding='utf-8')
for p in (ROOT/'Builds').iterdir():
    if not p.is_dir():continue
    match=re.fullmatch(r'(.+)-(\d{8}-\d{6})',p.name)
    if match:groups.setdefault(match[1],[]).append(p)
for family,paths in groups.items():
    for p in sorted(paths,key=lambda x:x.name)[:-1]:
        if p.name in documentation:retained.append({'path':p.relative_to(ROOT).as_posix(),'reason':'Referenced historical validation evidence; retained to preserve documentation links.'})
        else:add(p,'Older run in repeated QA family '+family+'; newest family evidence retained.')
inventory=[]
skip={'Library','Temp','Obj','Logs','UserSettings','.git','Legacy','__pycache__','Downloads'}
for top in ('ArtSource','Docs','Tools','Game','Builds'):
    for parent,dirs,files in os.walk(ROOT/top):
        dirs[:]=[d for d in dirs if d not in skip]
        for name in files:
            p=Path(parent)/name
            inventory.append({'path':p.relative_to(ROOT).as_posix(),'bytes':p.stat().st_size,'decision':'ARCHIVE' if p in candidates else 'KEEP','reason':candidates.get(p,'Active source, configuration, tool, dependency, reference, or retained evidence; uncertain items preserved.')})
build=ROOT/'Builds/WindowsFinal/WhatTheFish_Data/data.unity3d'
for p in ROOT.iterdir():
    if p.is_file():inventory.append({'path':p.name,'bytes':p.stat().st_size,'decision':'KEEP','reason':'Project root documentation or version-control configuration.'})
manifest={'batch':'coastal-stadiums-'+datetime.datetime.now().strftime('%Y%m%d-%H%M%S'),'validation':str(QA),'buildFile':str(build),'buildHash':digest(build),'files':[]}
for p,why in sorted(candidates.items()):manifest['files'].append({'original':p.relative_to(ROOT).as_posix(),'sha256':digest(p),'bytes':p.stat().st_size,'reason':why})
(OUT/'project-inventory.json').write_text(json.dumps(inventory,indent=2))
(OUT/'archive-candidates.json').write_text(json.dumps(manifest,indent=2))
(OUT/'retained-for-review.json').write_text(json.dumps(retained+[
 {'path':'Builds/Android','reason':'Latest Android artifacts are outside the Windows tuning scope.'},
 {'path':'Tools/Build/Archive-LegacyAssets.ps1 and Game/Assets/_Game/Editor/LegacyAssetAudit.cs','reason':'Earlier migration/recovery tools may still be needed; kept rather than inferred obsolete.'},
 {'path':'Docs/VisualDirection/CoastalIslands and SeaIslandConcepts','reason':'Selected references and earlier island evidence retained.'}
],indent=2))
print('Inventoried',len(inventory),'files; archive candidates',len(candidates),'bytes',sum(f['bytes'] for f in manifest['files']))
