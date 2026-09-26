# Final Windows validation

Build and review completed after the retired basketball roof, rig and suspended scoreboard assets were archived. The [gallery](../index.html) contains 16 actual EXE images and two controller walkthroughs; six imagegen concepts are clearly separated.

## Results

- 128 passing offline assertions: four entrances/exits per venue, all exterior stairs up/down, concourse landings, spectator aisles in both directions, arrival and return dock routes, garden bridges, sea-fall recovery, all three camera modes at playing area/entrance/concourse, open basketball sky, and four environment switches.
- Both local host/guest route runs passed after archival: replicated entrance, island approach, stairs up/down and concourse movement; the dock-side guest also completed dock return. Server collision ownership and remote movement replication passed. Evidence: `Builds/CoastalRooms-Football-20260926-232404` and `Builds/CoastalRooms-Basketball-20260926-232407`.
- Both post-archive smoke checks passed. Basketball retained ten distinct floor-supported spawns, four logo selections, four palettes, environment switching, all three camera modes and offline movement. Football movement/cameras passed.
- Blender audits passed geometry/UV validity, packed maps, dimensions and seating LODs. Football: 68 × 105 m pitch, 3,072 seats / 32 groups. Basketball: 15.24 × 28.6512 m court, 3.048 m rim height, 1,256 seats / 48 groups.
- Unity rebuilt the complete scene after archival and checked active dependency roots plus both venue prefabs for missing scripts/materials. No runtime exceptions, shader errors or inactive-controller errors were found in the final review/smoke logs.
- Close views inspected at player height and roughly 1–2 m from the new stone/trim: texture relief, modeled joints, thresholds, stair transitions, rail connections, planting and lighting. Existing football seating remains its original design. Review-only camera framing and local-avatar visibility were corrected before the final walkthrough export.

## RTX 4050 laptop · 1920 × 1080

Normal visible Windows development player, D3D11, 4× MSAA, vSync off, uncapped. Each view has 120 warm-up and 600 measured update frames; 720 camera-render callbacks confirm that the window rendered throughout each sample. No Unity build, video encoder or other test players ran during this benchmark.

| View | Mean frame ms | P95 ms | Mean FPS | Unity graphics MB |
| --- | ---: | ---: | ---: | ---: |
| Football · exterior | 3.64 | 4.18 | 274.4 | 753 |
| Football · entrance | 5.27 | 5.91 | 189.8 | 753 |
| Football · playing area | 4.23 | 4.59 | 236.2 | 737 |
| Basketball · exterior | 3.60 | 4.06 | 277.6 | 753 |
| Basketball · entrance | 4.27 | 4.72 | 234.3 | 753 |
| Basketball · playing area | 3.66 | 4.20 | 273.5 | 740 |

All sampled views met the 16.67 ms / 60 FPS target. Peak Windows process working set: **501.1 MiB**; peak private memory: **1530.5 MiB**. Unity graphics-driver allocation is reported separately and is not total dedicated VRAM. Raw timings and OS samples: [frame-performance.txt](frame-performance.txt), [process-memory.csv](process-memory.csv). This is a short six-view sample, not a sustained thermal or Android qualification. Internet/Relay latency was not retested in this pass.

The hidden-window timing attempt skipped rendering and was discarded. `performance.txt` is a separate explicit-render/GPU-fence benchmark; its equivalent render FPS is not presented as gameplay FPS.

## Known engine limitation

The normally rendered player can terminate with Windows code `0xC0000005` during shutdown after a UI Automation client has interacted with its window. The engine log shows rendering and input shutting down; Windows Event Log identifies UnityPlayer.dll. Local release-player symbols resolve the fault to `RuntimeStatic<PlatformAccessibilityManager,0>::StaticDestroy`. It reproduced in development/release players and D3D11/D3D12. Unity documents the matching shutdown defect as UUM-146676 in its [6000.6.0b8 release notes](https://unity.com/releases/editor/beta/6000.6.0b8). This diagnosis matches the observed failure; the project remains on its existing 6000.3.20f1 editor. Accessibility support has not been removed. Normal-player exit is therefore **not a passing check**; benchmark samples completed before teardown, and batch capture/route runs exited successfully. An engine patch qualification remains follow-up work.

## Cleanup and recovery

- `Legacy/coastal-stadiums-20260926-231415`: 4,803 files, 1,169.33 MiB.
- `Legacy/coastal-stadiums-20260926-232756`: 3,462 files, 564.18 MiB.
- `Legacy/coastal-stadiums-20260926-235334`: 6,523 files, 881.81 MiB.

Each archive retains original relative paths, SHA-256 hashes, reasons, sizes, completed moves and restoration instructions. Unity assets include their `.meta` files. Current authoring sources, selected references, latest reviews, Android artifacts, historically linked reports and uncertain tools were retained. The inventory and source-load audit are in `Builds/CoastalStadiumAudit/`.

Automatic approval review blocked removal of empty archive-source folders, without a more specific reason. Those empty folders remain; no content or recovery records were lost. Git internals and Unity caches were excluded from archival.

Reproduction: `Review-CoastalStadiums.ps1 -RecordRoutes`, both `Test-CoastalStadiumRooms.ps1` sport modes, and the normal-player `-coastalFrameBenchmark` option. [Build hashes](build-evidence.json) identify the final executable content.
