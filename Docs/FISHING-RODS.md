# Fishing rod integration — 1 October 2026

The blue/lime and teal/orange Meshy rods are independent Unity prefabs placed in a timber rack beside the first fishing pier's centre lane. The same models appear in the streamed fishing island, its environment prefab and the environment generator. Five player stands, four islands and shared room travel are preserved. This is a visual integration; it does not add casting, fish, pickup or reel animation.

## View in the game

Run `Builds/FishingRods/Preview/Review-Fishing-Rods.cmd` to open the retained Windows preview. Seven buttons show both rods, each reel, their reverse sides, guide loops, the distance LOD and the pier context. The walk-around button restores the normal player camera. Normal gameplay and Android use the same placed assets; only the review controls are development-only.

Actual player captures are under `Builds/FishingRods/20261001/Player-Compact/`. These use Unity's Windows renderer, not Meshy or a generated mockup. They do not establish Android texture appearance or phone FPS.

## Analysis and changes

[Original downloads, references, attribution and authoring masters](../ArtSource/Fishing/Rods/README.md) remain outside Unity Assets. Meshy 6 Lite was the user-approved model; two mesh and two texture jobs used 40 credits.

The source meshes are closed, with finite UVs and no degenerate triangles. The blue rod has one connected component and the teal rod has two. Source differences include soft grip and reel details, uneven guide loops, a shortened blue crank and a second crank-like shape on the teal reverse side. Those shape differences remain visible for review. Initial reduction introduced triangular colour seams; fresh UVs and high-to-low emission/normal baking corrected that additional defect.

Both props are 1.8 m long with butt pivots and grip/tip markers. The near meshes total 24,000 triangles (10,000 and 14,000); the distance meshes total 7,000. Component counts and Euler characteristics survive reduction. Before Unity packing, maximum sampled source-vertex distance is below 0.7 mm for the near LODs; this is not a bound on every surface point or on the packed runtime mesh. The [machine-readable audit](../ArtSource/Fishing/Rods/model-audit.json) contains exact measurements and integrity hashes.

Each rod uses one instanced URP material shared by its LODs, a 512-pixel colour map and a 256-pixel normal map at runtime. Android uses ASTC 6x6 for both. Source export PNGs retain higher resolution, and Unity applies Low mesh packing. The rack reuses the island's timber material; its simple colliders stay clear of the pier centre lane. Thin rod meshes have no player-blocking mesh colliders.

## Reproduction and portability

1. Run Blender with `--background --python Tools/Blender/build_fishing_rods.py -- --render`.
2. Run Unity method `FishingRodBuilder.Prepare`, or `FishingRodBuilder.BuildWindows` to prepare and build the review player.
3. Run `Tools/Build/Test-FishingRods.ps1`.
4. Run `Tools/Build/Build.ps1 -Target AndroidSubmission` and inspect the resulting APK and size audit.

The generator's default project root follows its own location. Regeneration was also exercised from a separate path containing spaces, with an unrelated working directory. Saved Blender masters use relative or packed images, delivery FBXs have verified relative texture references, and both formats were reopened. Fixed FBX exporter provenance is retained as nonfunctional metadata; active absolute dependencies are not permitted.

## Verification

The final Windows suite passed all **22 checks**, covering both rods, LOD assignments, materials and textures, dimensions, rack support, attachment markers, five independent player stands, a clear walking lane, one active island, and release/reload without duplication. Seven captured views cover close reels and guides, reverse surfaces, distance LODs and the pier. Close views were rechecked after the final texture and mesh packing changes. The lower-resolution maps soften detail at extreme magnification while preserving the inspected silhouettes and guide openings.

All retained source and derived file hashes match their audit. Original GLBs were not changed. The independent portability regeneration reproduced topology and dimensions and reopened all active dependencies. No physical phone was connected, so Android ASTC appearance, RAM/FPS, thermals and input remain unqualified. The rods have no multiplayer interaction state or fishing controls to validate in this visual integration.

## Measured Android delivery

The final `AndroidSubmission` build and APK v2 signature check passed. The measured APK changed from **85,088,760 to 85,955,058 bytes (+866,298 bytes)**, leaving **14,044,942 bytes** below the strict 100,000,000-byte limit. The 75,000,000-byte development target remains unmet by **10,955,058 bytes**. This shared-working-tree delta includes concurrent jump refinements; the rods' separate compressed asset estimate is **852,462 bytes**, from **981,892 uncompressed serialized bytes**.

The first fishing APK measured 87,153,874 bytes. Final texture sizing, ASTC settings and Low mesh packing reduced the APK by 1,198,816 bytes from that pass, with close and distant Windows views rechecked. The size comparison found no unexpected large dependencies; original GLBs and full-resolution authoring maps are not shipped.

Final APK: `Builds/Android/WhatTheFish-release.apk`, built at 20:26:21 Malaysia time on 1 October. SHA-256: `47683A954327E7369C1A662EC7B27FFC1179CF761C04DB3F56C264F9EF608DB0`. Exact measurements, signature, asset estimates, entry deltas and a protected `SizeAuditFinal/` snapshot are under `Builds/FishingRods/20261001/`. See [the current size baseline](BUILD-SIZE.md).

A subsequent shared jump build at 20:30:50 Malaysia time replaced the canonical APK with **85,957,206 bytes**, a total increase of **868,446 bytes** over the pre-integration baseline and **14,042,794 bytes** of hard-limit headroom. Its Android submission log and v2 signature were verified; rod asset estimates remain unchanged. SHA-256: `217F63822B8929DE6FE4DD4223A5856E440E8704B5108C59C0B6C8FCB4EF61FE`. Its measurement and audit are retained as `apk-shared-final.json` and `SizeAuditSharedFinal/`. The fishing preview remains the separately retained, tested compact player.

Repository readiness with `-RequireApk`, portable text paths, local documentation links and `git diff --check` passed. Git status and the complete diff summary were reviewed, preserving concurrent basketball, golf and jump work. Format-aware binary dependency validation is described above.

## Housekeeping

Current source, references, masters, import settings, prefabs, generator, review code and documentation are publishable. Logs, comparison images, the retained interactive player and size evidence remain under ignored `Builds/FishingRods/`. Nothing was staged, committed or pushed.

Superseded output and temporary work were moved with the repository's archive tool into these ignored recovery batches:

- `Legacy/20261001-201150-431-before-fishing-rods-apk/`: pre-integration APK.
- `Legacy/20261001-201253-271-fishing-rods-first-pass/`: first UV pass, temporary inspection helper and completed alternate-checkout test.
- `Legacy/20261001-201456-228-fishing-rods-preview-debug/`: copied compiler debug text.
- `Legacy/20261001-201714-086-fishing-rods-first-apk/`: first, larger fishing APK.
- `Legacy/20261001-202013-217-fishing-rods-size-pass/`: superseded Windows preview, captures and first APK audit.

Each batch contains sizes, SHA-256 hashes and original relative recovery paths. Current compact-player evidence remains under `Builds/FishingRods/20261001/Player-Compact/`.
