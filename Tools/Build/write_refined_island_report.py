"""Assemble delivery documentation from verified build evidence."""
from pathlib import Path
import datetime,hashlib,json,re,sys
root=Path(__file__).resolve().parents[2]
review=root/'Docs/VisualDirection/GolfFishingRefinement/GameReview'
audit=root/'Builds/IslandRefinementAudit'
def read(p):return p.read_text(encoding='utf-8-sig')
def passed(p,marker):
    s=read(p);assert marker in s and not re.search(r'^FAIL ',s,re.M),p
    return len(re.findall(r'^PASS ',s,re.M))
def rel(p):return p.relative_to(root).as_posix()
def link(p):return '['+rel(p)+']('+p.as_posix()+')'
if '--compact' in sys.argv:
    evidence=read(review/'review.txt');assert not re.search(r'^FAIL ',evidence,re.M)
    batch=json.loads(read(audit/'archive-candidates.json'));archive=root/'Legacy'/batch['batch']
    assert len(read(archive/'completed-moves.jsonl').splitlines())==len(batch['files'])
    for sport in ['Golf','Fishing']:
        layout=json.loads(read(root/'Game/Assets/_Game/Art/RefinedIslands'/sport/'layout.json'))
        for route in layout['routes']:
            for direction in ['outbound','return']:
                assert f"PASS {sport} {route['name']} {direction}" in evidence
    performance=read(review/'performance.txt');assert 'ISLAND_BENCHMARK_COMPLETE' in performance
    removed=json.loads(read(audit/'temporary-cleanup.json'))
    build=root/'Builds/WindowsFinal'
    fingerprints={}
    for item in batch['buildFiles']:
        p=Path(item['path']);digest=hashlib.sha256(p.read_bytes()).hexdigest().upper()
        assert digest==item['sha256'],p
        fingerprints[p.name]=digest
    (review/'delivery-build-hashes.json').write_text(json.dumps(fingerprints,indent=2)+'\n')
    for item in batch['files']:
        assert hashlib.sha256((archive/item['original']).read_bytes()).hexdigest().upper()==item['sha256']
    text=f'''# G2 / L2 Windows review

Delivered {datetime.datetime.now().strftime('%Y-%m-%d %H:%M')} Asia/Kuala_Lumpur.

The user selected **G2 Limestone Cove Links** and **L2 Limestone Garden Lagoon**, then requested fewer repeated checks and less temporary storage so they could inspect the game themselves. This delivery keeps the finished environments, the existing measured evidence and one compact screenshot gallery. Repeated recording and post-cleanup build/test loops were stopped.

## Open the result

The local Windows player is at `Builds/WindowsFinal/WhatTheFish.exe`, with `Explore-Golf.cmd` and `Explore-Fishing.cmd` launchers beside it. Keep the whole WindowsFinal folder together. Git excludes `Builds/`, so teammates build their own player from the committed Unity project.
- [27 actual Windows screenshots](index.html): 14 golf and 13 fishing, including player-height terrain, vegetation, joinery, water, landmarks and close material views. These are captures of the final environment geometry; the subsequent build only corrected the optional recording camera.
- [Editable Blender masters and rebuild guide](../../../../ArtSource/RefinedIslands/README.md).

WASD moves, Shift runs, right mouse drag looks, and C changes camera mode.

## Implemented

Golf has sculpted limestone shores and sea arch, five bunkers, main fairway, tee, raised and practice greens, spring pools and waterfall, an open welcome pavilion and connected routes. Fishing has a planted limestone lagoon, five stable independently coloured modular decks, inlet bridge, shelter and continuous shore circuit.

Both use shared 2K tiling base/normal/smoothness materials, 4K terrain masks, modeled timber joints and fixings, layered vegetation, curved palms and fronds, near/mid/far vegetation and rock LODs, separate water settings and environment lighting profiles. The fishing path-height defect found during testing was corrected; the circuit now uses a joined ribbon. Existing controls, capacities, menu/profile identities and server-authoritative movement remain.

## Existing validation evidence

- Latest offline run: {len(re.findall(r'^PASS ',evidence,re.M))} passing checks, zero failures. All ten golf and eight fishing routes completed both directions. This also covered supported spawns, containment, fall recovery, independent fishing accents and all three camera modes. Its recording phase was intentionally stopped, so the run has no final completion banner; it is not represented as a completed video run.
- Fishing local host/guest: 165/165 host and 149/149 guest waypoints passed, including all five stations and the inlet bridge. Detailed logs are in the local ignored `Builds/RefinedRooms-Fishing-20260927-031608` folder.
- Earlier golf local host/guest routes and ten-player golf/five-player fishing room suites passed. Their original logs remain in Builds; no cloud test is claimed.
- Source geometry/UV/material audit passed. The Unity dependency audit found 90 obsolete golf/fishing assets unreferenced by active scenes, Resources and prefabs before archival. All four environment prefabs had their meshes, materials and scripts.
- The approved stadium regression passed earlier in this refinement. All 395 protected football/basketball source and art files matched their recorded SHA-256 baseline before the last code-only recording build.
- The final Windows build exited successfully. The EXE, managed game assembly and packaged data still match the pre-archive hashes in [delivery-build-hashes.json](delivery-build-hashes.json). Archive integrity was checked without another Unity rebuild.

## Measured performance

Visible Windows development player, RTX 4050 Laptop GPU, 1920×1080, Direct3D 11, uncapped with VSync off. Three representative views per island used 120 warm-up and 900 measured frames. Both sustained controller runs covered approximately 240 metres in 60 seconds. Mean frame times were 2.05–2.43 ms and p95 2.61–3.95 ms in this workload. These measurements precede only the optional recorder-camera correction.

Process working set was approximately 572–651 MiB; graphics-driver allocation was 1239–1271 MiB. Windows process counters were sampled by the wrapper because the embedded Mono memory counter returned zero. Exact view/run measurements, private memory, frame counts and method are in [performance.txt](performance.txt). The already documented native Unity shutdown access violation occurred after completed measurements; [the exit status](benchmark-exit.txt) is retained. This refinement does not claim to fix that separate issue.

## Cleanup

Archived {len(batch['files'])} confirmed superseded golf/fishing assets, sources, tools and old evidence ({sum(f['bytes'] for f in batch['files'])/1048576:.2f} MiB) to the local ignored `Legacy/{batch['batch']}` folder. Retired basketball ceiling, overhead lighting and scoreboard assets and their metadata are preserved in the separate local `Legacy/RallyOpenAirRetired-20260927` batch. Original paths, hashes, reasons, paired Unity metadata and restoration instructions are included. Neither archive is sent by `git push`.

Removed {sum(x['bytes'] or 0 for x in removed)/1073741824:.2f} GiB of this task's intermediate captures and automatic Blender backups. Current editable masters, selected concepts/prompts, the final gallery and concise diagnostic logs remain. No route video is delivered after the user's request to stop repeated recording/checking. Original fishing concept history, older unrelated cloud/network evidence, Android builds and shutdown investigation files were retained.

No golf-shot/fishing mechanics, NPCs, swimming, new customization UI or Android tuning were added. Final visual acceptance is left to the user's own EXE review as requested.
'''
    (review/'VALIDATION.md').write_text(text,encoding='utf-8')
    print(review/'VALIDATION.md');sys.exit(0)
offline=passed(review/'review.txt','ISLAND_REVIEW_COMPLETE success=True')
post=passed(audit/'PostCleanup/review.txt','ISLAND_REVIEW_COMPLETE success=True')
stadium=passed(audit/'PostCleanupStadium/review.txt','STADIUM_REVIEW_COMPLETE')
performance=read(review/'performance.txt');assert 'ISLAND_BENCHMARK_COMPLETE' in performance and 'INVALID' not in performance
benchmark_exit=read(review/'benchmark-exit.txt').strip()
exit_note='The known native shutdown fault occurred after complete measurements and is recorded separately from frame-time results.' if '-1073741819' in benchmark_exit else 'The benchmark player exited normally.'
baseline=json.loads(read(audit/'approved-worlds-comparison.json'));assert not baseline['changed']
videos=json.loads(read(review/'video-metadata.json'))
batch=json.loads(read(audit/'archive-candidates.json'))
archive=root/'Legacy'/batch['batch'];assert (archive/'completed-moves.jsonl').exists()
assert len(read(archive/'completed-moves.jsonl').splitlines())==len(batch['files'])
photos={sport:len(list(review.glob(sport+'_??_*.png'))) for sport in ['Golf','Fishing']}
rows=[]
for line in performance.splitlines():
    if 'mean_ms=' not in line:continue
    label=line.split(': samples=')[0];values=dict(re.findall(r'(\w+)=([^;]+)',line))
    rows.append('| '+label+' | '+values['mean_ms']+' | '+values['p95_ms']+' | '+values['FPS']+' | '+values['working_set_MiB']+' | '+values['graphics_MiB']+' |')
latest=[]
for pattern,name in [('GolfG2QA-*','Golf graphical integration'),('FishingQA-20260927-*','Fishing graphical integration'),('GolfQA-Local-20260927-*','Ten-player golf room'),('FishingRoomsQA-20260927-*','Five-player fishing room'),('RefinedRooms-Golf-*','Golf host/guest route'),('RefinedRooms-Fishing-*','Fishing host/guest route')]:
    p=max((root/'Builds').glob(pattern),key=lambda p:p.stat().st_mtime)
    latest.append('- '+name+': '+link(p))
build=root/'Builds/WindowsFinal'
build_hashes={p.name:hashlib.sha256(p.read_bytes()).hexdigest().upper() for p in [build/'WhatTheFish.exe',build/'WhatTheFish_Data/Managed/Assembly-CSharp.dll',build/'WhatTheFish_Data/data.unity3d']}
(review/'delivery-build-hashes.json').write_text(json.dumps(build_hashes,indent=2)+'\n')
text=f'''# G2 / L2 Windows delivery

Verified {datetime.datetime.now().strftime('%Y-%m-%d %H:%M')} Asia/Kuala_Lumpur. Selected directions: **G2 Limestone Cove Links** and **L2 Limestone Garden Lagoon**. The images in this folder are actual Windows player captures; the imagegen concepts remain in the separate Concepts folder.

## Delivered

- Windows player: {link(build/'WhatTheFish.exe')}. Keep the complete WindowsFinal folder together. Explore-Golf.cmd and Explore-Fishing.cmd launch directly into each island.
- {photos['Golf']} golf and {photos['Fishing']} fishing original 1920×1080 images, including player-height surfaces, vegetation, joinery, shores and sky. The last ground-material view looks down from approximately 1–2 metres.
- Continuous controller walkthroughs: golf {videos['Golf']['seconds']:.1f}s and fishing {videos['Fishing']['seconds']:.1f}s, 1280×720 at 30fps. Both files were fully decoded and their frame counts verified. These diagnostic routes drive the existing controller through connected waypoints, with no teleports within a recording.
- Editable Blender masters, shared asset kit, measured layout manifests, portable textures, Unity prefabs and deterministic generators: {link(root/'ArtSource/RefinedIslands/README.md')}.

## Environment work

Golf retains ten supported spawns, its tee, main fairway, raised green, practice green and five walkable bunkers. Limestone terraces, the sea arch, spring basins and waterfall define the coast. An open timber/cream-stone pavilion connects to the practice route and promenade.

Fishing retains five stable independently coloured station slots with the same interchangeable deck mesh, clear standing areas, an inlet bridge and continuous island circuit. Planted limestone, palm gardens and an open shelter establish the L2 identity.

Both use shared 2K base/normal/metallic-smoothness maps, 4K terrain masks, tiled close-view surfaces, modeled beveled timber and masonry, metal fittings, rope lashings, multi-part palms and layered planting. World-tiled terrain/stone target approximately 512 source pixels/metre; texture density varies on curved and scaled modules. Vegetation and rock meshes have three LOD levels; grass is terrain-conformed and batched into spatial cells. Water/wind animation remains visual only.

The environment lighting profile sets sky, daylight, fog, contact shading and close-detail shadow bias. Original stadium shadow settings are restored when leaving either island. Profile/menu names, sport IDs, capacities, controller/network interfaces and approved stadium customization remain intact.

## Validation

- Final capture run: **{offline} passing checks**, zero failures. All ten golf routes and eight fishing routes traversed outbound and return, using the real controller; tests cover every deck, bridge, pavilion/shelter approaches, bunkers, elevation changes, spawn support, shoreline containment, fall recovery and three camera modes.
- Post-cleanup rebuilt-player review: **{post} passing checks**, zero failures.
- Football/basketball post-cleanup regression: **{stadium} passing checks**, including playing dimensions, logos/palettes, entrances, walking and environment switching. All **395** protected authoring/art files match their pre-refinement SHA-256 baseline.
- Source geometry audit checks finite vertices, UVs, materials, exports and closed deck/bridge geometry. Unity dependency audit checks active scenes, Resources and non-retired prefabs, including meshes/materials/missing scripts for all four worlds.

{chr(10).join(latest)}

Local host/guest routes also test server-authoritative movement and replicated remote motion. The standard room suites verify ten/five capacity, overflow rejection, start/return, guest permissions and room isolation. Cloud services were not required for this local regression pass.

## Visible Windows performance

RTX 4050 Laptop GPU, 1920×1080, Direct3D 11, development player, 4× MSAA, uncapped and VSync off. Each fixed view discards 120 warm-up frames and measures 900 frames; each island also has a 60-second controller walk. The runtime counts rendered camera frames, and the player window is visible. Results reflect this laptop and workload, not every possible hardware configuration.

| View/run | Mean ms | p95 ms | Mean FPS | Process working set MiB | Graphics allocation MiB |
|---|---:|---:|---:|---:|---:|
{chr(10).join(rows)}

Full private/reserved-memory and frame-count details: [performance.txt](performance.txt). Windows process memory is sampled within 250ms of each measurement because the embedded Mono counter returned zero; the original runtime output is retained. Graphics allocation is Unity's graphics-driver allocation counter, not total dedicated VRAM across all processes. Benchmark exit status: {benchmark_exit}. {exit_note}

## Visual review

The review covers terrain detail, foliage roots, close timber/rope/metal work, rock scale and texture tiling, station colours, facility supports, path joins, shoreline colour, the cascade and LOD changes during walking. Defects corrected during review included clipped roof caps, floating grass/path segments, waterfall foundations, grass inside bunker sand, rough contact shadows, overlapping path joins and one-sided flags. A later host/guest test caught accumulating paving elevation on the segmented fishing circuit; the circuit is now one joined ribbon with a bounded surface offset and was retested. The actual captures are the visual acceptance evidence; concept renders do not prove runtime quality.

## Archive and recovery

Archived **{len(batch['files'])} confirmed superseded files**, **{sum(f['bytes'] for f in batch['files'])/1048576:.1f} MiB**, in {link(archive)}. Every entry records its original relative path, SHA-256, size and reason, with paired Unity .meta files and restoration instructions. Existing Legacy history is unchanged. The project rebuilt and passed dependency, island and stadium checks after these moves.

Retained uncertain/historical material: original fishing concept/prompt studies, older cloud/network evidence, Android builds, the shutdown investigation, and the two original golf/fishing overview images cited by the selected concept prompts. Unrelated active work, Git internals and Unity-generated caches were excluded.

## Scope limits

Exploration only: no golf-shot or fishing mechanics, swimming, NPCs or new customization UI. Android tuning is deferred. The previously documented Unity native shutdown issue remains a separate investigation; this work does not claim to fix it. Build fingerprints are saved in [delivery-build-hashes.json](delivery-build-hashes.json).
'''
(review/'VALIDATION.md').write_text(text,encoding='utf-8')
print(review/'VALIDATION.md')
