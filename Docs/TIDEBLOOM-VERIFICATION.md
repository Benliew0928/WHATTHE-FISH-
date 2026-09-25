# Tidebloom golf environment — 25 September 2026

The current `Builds/WindowsFinal/WhatTheFish.exe` contains Tidebloom Island. Choose **Golf → Explore offline**, or launch with `-sport Golf -offline`. WASD moves, Shift runs, right mouse drag looks, and C cycles the three cameras.

## Delivered art

- Editable master: `ArtSource/Golf/Tidebloom/Island_Tidebloom.blend`; the original placeholder master is preserved.
- 123 named meshes, 398,146 source triangles, 21 mapped URP materials and seven portable PNG texture maps.
- Sculpted continuous terrain, a winding striped fairway, five walkable sand bowls, raised main green, side practice green, coral flags and rope-lined tee.
- 47 palms, spatially batched flower/leaf gardens, turf-capped limestone formations, offshore sea arch, background islets and cloud sculptures.
- Gradient shallows, animated water texture coordinates, spring pool, cascade and foam geometry.
- Sixteen terrain collision sectors, seventeen limestone/basin mesh colliders, 47 simplified palm colliders and 192 player-only shoreline segments. Shore barriers are excluded from camera obstruction checks.
- Source renders `Docs/VisualDirection/Golf/01`–`04`, Unity overview `05`, and Windows player captures `06`–`08`.

## Verification

The Windows development player rebuilt successfully on Unity 6000.3.20f1. `Builds/WindowsFinal/LATEST-BUILD.txt` records the exact build time. An Android release rebuild was subsequently requested along with legacy cleanup; see `Docs/BUILD-SIZE.md` for the final package measurement.

`Tools/Build/Test-GolfIsland.ps1` exercises the actual player, and accepts `-Headless` for a graphics-free repeat. The final player passed **126 checks**: ten distinct supported spawns, valid materials, terrain collision, environment switching, profile isolation, 48 shoreline rays and 48 sustained outward sprint attempts, full fairway traversal with the real character controller, all five bunker exits, shoreline camera behavior and offline operation. Containment is measured against the coast at the player's final heading because the controller can slide along its curved segments.

Evidence: `Builds/GolfTidebloomQA-20260925-201221/island.txt` and `Builds/GolfTidebloomQA-20260925-201359/island.txt`. Both contain 126 PASS records and no FAIL records. Graphical interactive probes exposed an existing native `UnityPlayer.dll` access violation during application shutdown after the checks completed. The same exit failure was reproduced in the archived pre-Tidebloom WindowsLatest player and with both Direct3D 11 and 12. Batch-mode player shutdown succeeds; the automated runner now uses batch mode with graphics enabled, or `-Headless` without graphics. This is not a terrain assertion failure, and changing the graphics API alone did not fix it. Normal interactive shutdown remains an engine issue to investigate separately.

`Tools/Build/Test-GolfRooms.ps1 -Transport Local` passed all **11 room checks**, including ten players, distinct spawns, capacity rejection, host-controlled start/return, an independent basketball room, guest authority restrictions, late join and host departure. Evidence: `Builds/GolfQA-Local-20260925-201221/results.txt`. No null, missing-reference or index errors were found in the checked host/guest logs. Internet Relay was not retested for this art update.

`Tools/Blender/audit_golf.py` validates the saved source: packed/exported textures, finite geometry, UVs, material slots, upward nondegenerate terrain faces and per-mesh 16-bit vertex limits. `Builds/golf-source-validation.json` reports no errors.

## Scope

The organic coast and terrain replace the old square footprint; the placeholder's exact 60-second straight crossing measurements are historical. This is an explorable environment asset update. Cups and flags are visual course dressing; shot input, ball simulation and scoring remain future gameplay work. Desktop captures do not constitute a target Android performance benchmark.

Post-archive final validation: the Windows build at 20:25:10 local time rebuilt with the old venue assets and materials outside Unity Assets. `Builds/GolfTidebloomQA-20260925-202807/island.txt` passed 126 checks in the graphical batch player with exit code 0. `Builds/PostLegacyQA/Football.txt` and `Basketball.txt` completed smoke checks, including all four basketball logos, with exit code 0. The final release-mode APK passed signature verification and is 72.69 MB; see `Docs/BUILD-SIZE.md`.
