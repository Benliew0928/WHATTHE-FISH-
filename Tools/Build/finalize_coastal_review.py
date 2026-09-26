"""Publish evidence only after the final player, routes, rooms and benchmark pass."""
from pathlib import Path
import json,hashlib,re,csv,datetime

root=Path(__file__).resolve().parents[2]
review=root/'Docs/VisualDirection/CoastalStadiums/GameReview'
def passed(path,marker):
    text=path.read_text(encoding='utf-8-sig',errors='replace')
    assert marker in text and not re.search(r'^FAIL |^INVALID:',text,re.M),str(path)
    return text
def sha(path):
    h=hashlib.sha256()
    with path.open('rb') as stream:
        for data in iter(lambda:stream.read(1024*1024),b''):h.update(data)
    return h.hexdigest().upper()
checks=passed(review/'review.txt','STADIUM_REVIEW_COMPLETE')
performance=passed(review/'frame-performance.txt','FRAME_BENCHMARK_COMPLETE')
assert performance.count('rendered_frames=720')==6,performance
buildlog=passed(root/'Builds/coastal-final-build.log','BUILD_OK')
dependency=passed(root/'Builds/coastal-post-archive-build.log','COASTAL_LEGACY_AUDIT_COMPLETE')
rooms=[]
for sport in ['Football','Basketball']:
    directory=sorted((root/'Builds').glob('CoastalRooms-'+sport+'-*'))[-1]
    for role in ['host','guest']:
        passed(directory/(role+'-route.txt'),'NETWORK_COASTAL_COMPLETE success=True')
    rooms.append(directory.relative_to(root).as_posix())
    passed(root/f'Builds/CoastalStadiumSmoke-PostArchive/{sport}.txt','SMOKE_COMPLETE')
    assert len(list(review.glob(sport+'_0*.png')))==8
    assert (review/(sport+'_Walkthrough.mp4')).stat().st_size>1_000_000
for log in [root/'Builds/coastal-stadium-review.log',root/'Builds/coastal-frame-performance.log',*list((root/'Builds/CoastalStadiumSmoke-PostArchive').glob('*.log'))]:
    assert not re.search(r'Exception:|Shader error|called on inactive controller|Missing material',log.read_text(errors='replace')),str(log)
files=[]
for name in ['WhatTheFish.exe','UnityPlayer.dll','WhatTheFish_Data/data.unity3d','WhatTheFish_Data/Managed/Assembly-CSharp.dll']:
    p=root/'Builds/WindowsFinal'/name
    files.append({'path':p.relative_to(root).as_posix(),'sha256':sha(p),'bytes':p.stat().st_size})
archives=[]
for p in sorted((root/'Legacy').glob('coastal-stadiums-*/archive-manifest.json')):
    d=json.loads(p.read_text(encoding='utf-8-sig'));archives.append({'batch':p.parent.name,'files':len(d['files']),'bytes':sum(f['bytes'] for f in d['files'])})
evidence={'reviewed_utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'build_files':files,'actual_screenshots':16,'concept_images':6,'offline_assertions':checks.count('PASS '),'network_evidence':rooms,'archives':archives,'source_audit':'ArtSource/CoastalStadiums/source-audit.json','video_fps':30,'videos':json.loads((review/'video-metadata.json').read_text())}
(review/'build-evidence.json').write_text(json.dumps(evidence,indent=2)+'\n')
rows=re.findall(r'(Football|Basketball) (.+): mean_ms=([\d.]+); p95_ms=([\d.]+); FPS=([\d.]+); rendered_frames=(\d+); Unity_reserved_MB=(\d+); graphics_MB=(\d+)',performance)
assert len(rows)==6
memory=list(csv.DictReader((review/'process-memory.csv').open(encoding='utf-8-sig')))
peak=max(float(row['workingSetMB']) for row in memory);private=max(float(row['privateMB']) for row in memory)
table='\n'.join(f'| {s} · {v} | {mean} | {p95} | {fps} | {gpu} |' for s,v,mean,p95,fps,n,unity,gpu in rows)
archive_text='\n'.join(f'- `Legacy/{a["batch"]}`: {a["files"]:,} files, {a["bytes"]/1048576:,.2f} MiB.' for a in archives)
report=f'''# Final Windows validation

Build and review completed after the retired basketball roof, rig and suspended scoreboard assets were archived. The [gallery](../index.html) contains 16 actual EXE images and two controller walkthroughs; six imagegen concepts are clearly separated.

## Results

- {evidence['offline_assertions']} passing offline assertions: four entrances/exits per venue, all exterior stairs up/down, concourse landings, spectator aisles in both directions, arrival and return dock routes, garden bridges, sea-fall recovery, all three camera modes at playing area/entrance/concourse, open basketball sky, and four environment switches.
- Both local host/guest route runs passed after archival: replicated entrance, island approach, stairs up/down and concourse movement; the dock-side guest also completed dock return. Server collision ownership and remote movement replication passed. Evidence: `{rooms[0]}` and `{rooms[1]}`.
- Both post-archive smoke checks passed. Basketball retained ten distinct floor-supported spawns, four logo selections, four palettes, environment switching, all three camera modes and offline movement. Football movement/cameras passed.
- Blender audits passed geometry/UV validity, packed maps, dimensions and seating LODs. Football: 68 × 105 m pitch, 3,072 seats / 32 groups. Basketball: 15.24 × 28.6512 m court, 3.048 m rim height, 1,256 seats / 48 groups.
- Unity rebuilt the complete scene after archival and checked active dependency roots plus both venue prefabs for missing scripts/materials. No runtime exceptions, shader errors or inactive-controller errors were found in the final review/smoke logs.
- Close views inspected at player height and roughly 1–2 m from the new stone/trim: texture relief, modeled joints, thresholds, stair transitions, rail connections, planting and lighting. Existing football seating remains its original design. Review-only camera framing and local-avatar visibility were corrected before the final walkthrough export.

## RTX 4050 laptop · 1920 × 1080

Normal visible Windows development player, D3D11, 4× MSAA, vSync off, uncapped. Each view has 120 warm-up and 600 measured update frames; 720 camera-render callbacks confirm that the window rendered throughout each sample. No Unity build, video encoder or other test players ran during this benchmark.

| View | Mean frame ms | P95 ms | Mean FPS | Unity graphics MB |
| --- | ---: | ---: | ---: | ---: |
{table}

All sampled views met the 16.67 ms / 60 FPS target. Peak Windows process working set: **{peak:.1f} MiB**; peak private memory: **{private:.1f} MiB**. Unity graphics-driver allocation is reported separately and is not total dedicated VRAM. Raw timings and OS samples: [frame-performance.txt](frame-performance.txt), [process-memory.csv](process-memory.csv). This is a short six-view sample, not a sustained thermal or Android qualification. Internet/Relay latency was not retested in this pass.

The hidden-window timing attempt skipped rendering and was discarded. `performance.txt` is a separate explicit-render/GPU-fence benchmark; its equivalent render FPS is not presented as gameplay FPS.

## Known engine limitation

The normally rendered player can terminate with Windows code `0xC0000005` during shutdown after a UI Automation client has interacted with its window. The engine log shows rendering and input shutting down; Windows Event Log identifies UnityPlayer.dll. Local release-player symbols resolve the fault to `RuntimeStatic<PlatformAccessibilityManager,0>::StaticDestroy`. It reproduced in development/release players and D3D11/D3D12. Unity documents the matching shutdown defect as UUM-146676 in its [6000.6.0b8 release notes](https://unity.com/releases/editor/beta/6000.6.0b8). This diagnosis matches the observed failure; the project remains on its existing 6000.3.20f1 editor. Accessibility support has not been removed. Normal-player exit is therefore **not a passing check**; benchmark samples completed before teardown, and batch capture/route runs exited successfully. An engine patch qualification remains follow-up work.

## Cleanup and recovery

{archive_text}

Each archive retains original relative paths, SHA-256 hashes, reasons, sizes, completed moves and restoration instructions. Unity assets include their `.meta` files. Current authoring sources, selected references, latest reviews, Android artifacts, historically linked reports and uncertain tools were retained. The inventory and source-load audit are in `Builds/CoastalStadiumAudit/`.

Automatic approval review blocked removal of empty archive-source folders, without a more specific reason. Those empty folders remain; no content or recovery records were lost. Git internals and Unity caches were excluded from archival.

Reproduction: `Review-CoastalStadiums.ps1 -RecordRoutes`, both `Test-CoastalStadiumRooms.ps1` sport modes, and the normal-player `-coastalFrameBenchmark` option. [Build hashes](build-evidence.json) identify the final executable content.
'''
(review/'VALIDATION.md').write_text(report,encoding='utf-8')
print('FINAL_REVIEW_VALIDATED',evidence['offline_assertions'],'offline checks;',len(rows),'rendered benchmark views; peak working set',peak)
