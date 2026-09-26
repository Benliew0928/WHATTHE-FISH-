"""Read-only dependency inventory; moves are performed by the native PS script."""
from pathlib import Path
import datetime,hashlib,json,re,sys
root=Path(__file__).resolve().parents[2]
audit=root/'Builds/IslandRefinementAudit'
review=root/'Docs/VisualDirection/GolfFishingRefinement/GameReview'
files={}
compact='--compact' in sys.argv
def digest(p):
    h=hashlib.sha256()
    with p.open('rb') as f:
        for chunk in iter(lambda:f.read(4*1024*1024),b''):h.update(chunk)
    return h.hexdigest().upper()
def add(p,reason):
    p=p.resolve();p.relative_to(root)
    if not p.is_file():return
    relative=p.relative_to(root).as_posix()
    assert not any(x in p.parts for x in ('.git','Library','Temp','obj','Logs','UserSettings','Legacy'))
    files[relative]=dict(original=relative,sha256=digest(p),bytes=p.stat().st_size,reason=reason)
def tree(relative,reason,exclude=()):
    p=root/relative
    for f in p.rglob('*'):
        if f.name not in exclude:add(f,reason)
    add(Path(str(p)+'.meta'),reason+'; paired folder metadata')
retired=set((audit/'retired-unity-assets.txt').read_text().splitlines())
for relative in ['Game/Assets/_Game/Art/Golf/Tidebloom','Game/Assets/_Game/Art/Fishing/Lagoon','Game/Assets/_Game/Prefabs/Fishing/Lagoon']:
    for p in (root/relative).rglob('*'):
        if p.is_file() and p.suffix!='.meta':assert p.relative_to(root/'Game').as_posix() in retired,p
    tree(relative,'Superseded by RefinedIslands; active Unity dependency graph confirms no reference')
for suffix in ('','.meta'):add(root/('Game/Assets/_Game/Art/GolfIsland.fbx'+suffix),'Superseded G2 terrain export; dependency audit confirms unreferenced')
for relative in ['ArtSource/Golf/Tidebloom','ArtSource/Fishing/Lagoon']:
    tree(relative,'Superseded Blender authoring source; editable G2/L2 masters and deterministic builders retained')
for name in ['build_golf.py','build_fishing_lagoon.py','audit_golf.py','audit_fishing_lagoon.py','export_assets.py']:
    add(root/'Tools/Blender'/name,'Retired generator/audit only targets superseded golf/fishing sources')
for p in (root/'Tools/Build').glob('Refined*.template'):add(p,'Temporary implementation staging copy; final source retained in Game/Assets')
if not compact:
    for p in (root/'ArtSource/RefinedIslands').glob('*.blend1'):add(p,'Blender previous-save backup; validated current editable .blend retained')
for name in ['GOLF-VERIFICATION.md','TIDEBLOOM-VERIFICATION.md','FISHING-VERIFICATION.md']:
    add(root/'Docs'/name,'Historical superseded island report; current validation and link replacement retained')
for relative in ['ArtSource/Golf/README.md','Docs/VisualDirection/Fishing/README.md','Docs/VisualDirection/Fishing/BLENDER-MODELING-PLAN.md','Docs/VisualDirection/Fishing/LAGOON-5P-BLENDER-PLAN.md']:
    add(root/relative,'Superseded golf/fishing authoring or integration guide; G2/L2 guide replaces active entry points')
# Keep original selected concept references at the paths recorded in PROMPTS.md.
tree('Docs/VisualDirection/Golf','Superseded golf review image; new 12-view EXE gallery retained',('08_Windows_Overview.png',))
tree('Docs/VisualDirection/Fishing/Blender','Superseded fishing review image; new 12-view EXE gallery retained',('04_Windows_Overview.png',))
for p in (root/'Builds').iterdir():
    if p.is_dir() and (p.name.startswith('GolfTidebloomQA') or re.match(r'Fishing(?:Rooms)?QA-20260926-',p.name)):
        tree(p.relative_to(root),'Superseded island test output; current graphical, route and capacity evidence retained')
for name in ['Review02','Review03','Review04','Review05','Review06','PreWaterReview','PrePathFixReview','PreRecorderReview']:
    tree('Builds/IslandRefinementAudit/'+name,'Intermediate refinement captures superseded by final verified EXE gallery')
for p in audit.glob('first-*'):add(p,'Initial island integration smoke output superseded by passing final review')
if not compact:
    metadata=json.loads((review/'video-metadata.json').read_text())
    for sport in ['Golf','Fishing']:
        assert metadata[sport]['full_decode_verified']
        assert digest(review/(sport+'_Walkthrough.mp4'))==metadata[sport]['sha256']
        tree(str((review/(sport+'_WalkthroughFrames')).relative_to(root)),'Verified continuous route video source frames; original JPEG frames retained in archive and fully decoded MP4 retained in review')
builds=[]
for relative in ['Builds/WindowsFinal/WhatTheFish.exe','Builds/WindowsFinal/WhatTheFish_Data/Managed/Assembly-CSharp.dll','Builds/WindowsFinal/WhatTheFish_Data/data.unity3d']:
    p=root/relative;assert p.is_file(),p
    builds.append(dict(path=str(p),sha256=digest(p)))
batch=dict(batch='GolfFishing-G2L2-'+datetime.datetime.now().strftime('%Y%m%d-%H%M%S'),validation=str(review/'review.txt'),dependencyAudit=str(audit/'dependency-audit-result.txt'),buildFiles=builds,files=sorted(files.values(),key=lambda f:f['original']),retainedUncertain=['Original fishing concept studies and prompt history','Older cloud/network runs and all unrelated football/basketball outputs','Android builds and Unity shutdown investigation','Selected original golf/fishing overview reference images'])
if compact:
    batch['validation']=str(audit/'PreRecorderReview/review.txt')
    batch['validationScope']='Completed bidirectional offline routes and camera checks; video recording stopped at user request to reduce repeated checking and storage.'
(audit/'archive-candidates.json').write_text(json.dumps(batch,indent=2)+'\n')
print(len(files),'files;',round(sum(f['bytes'] for f in files.values())/1048576,2),'MiB;',batch['batch'])
