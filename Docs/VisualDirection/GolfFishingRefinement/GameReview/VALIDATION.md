# G2 / L2 Windows review

Delivered 2026-09-27 03:38 Asia/Kuala_Lumpur.

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

- Latest offline run: 174 passing checks, zero failures. All ten golf and eight fishing routes completed both directions. This also covered supported spawns, containment, fall recovery, independent fishing accents and all three camera modes. Its recording phase was intentionally stopped, so the run has no final completion banner; it is not represented as a completed video run.
- Fishing local host/guest: 165/165 host and 149/149 guest waypoints passed, including all five stations and the inlet bridge. Detailed logs are in the local ignored `Builds/RefinedRooms-Fishing-20260927-031608` folder.
- Earlier golf local host/guest routes and ten-player golf/five-player fishing room suites passed. Their original logs remain in Builds; no cloud test is claimed.
- Source geometry/UV/material audit passed. The Unity dependency audit found 90 obsolete golf/fishing assets unreferenced by active scenes, Resources and prefabs before archival. All four environment prefabs had their meshes, materials and scripts.
- The approved stadium regression passed earlier in this refinement. All 395 protected football/basketball source and art files matched their recorded SHA-256 baseline before the last code-only recording build.
- The final Windows build exited successfully. The EXE, managed game assembly and packaged data still match the pre-archive hashes in [delivery-build-hashes.json](delivery-build-hashes.json). Archive integrity was checked without another Unity rebuild.

## Measured performance

Visible Windows development player, RTX 4050 Laptop GPU, 1920×1080, Direct3D 11, uncapped with VSync off. Three representative views per island used 120 warm-up and 900 measured frames. Both sustained controller runs covered approximately 240 metres in 60 seconds. Mean frame times were 2.05–2.43 ms and p95 2.61–3.95 ms in this workload. These measurements precede only the optional recorder-camera correction.

Process working set was approximately 572–651 MiB; graphics-driver allocation was 1239–1271 MiB. Windows process counters were sampled by the wrapper because the embedded Mono memory counter returned zero. Exact view/run measurements, private memory, frame counts and method are in [performance.txt](performance.txt). The already documented native Unity shutdown access violation occurred after completed measurements; [the exit status](benchmark-exit.txt) is retained. This refinement does not claim to fix that separate issue.

## Cleanup

Archived 377 confirmed superseded golf/fishing assets, sources, tools and old evidence (125.92 MiB) to the local ignored `Legacy/GolfFishing-G2L2-20260927-032531` folder. Retired basketball ceiling, overhead lighting and scoreboard assets and their metadata are preserved in the separate local `Legacy/RallyOpenAirRetired-20260927` batch. Original paths, hashes, reasons, paired Unity metadata and restoration instructions are included. Neither archive is sent by `git push`.

Removed 2.31 GiB of this task's intermediate captures and automatic Blender backups. Current editable masters, selected concepts/prompts, the final gallery and concise diagnostic logs remain. No route video is delivered after the user's request to stop repeated recording/checking. Original fishing concept history, older unrelated cloud/network evidence, Android builds and shutdown investigation files were retained.

No golf-shot/fishing mechanics, NPCs, swimming, new customization UI or Android tuning were added. Final visual acceptance is left to the user's own EXE review as requested.
