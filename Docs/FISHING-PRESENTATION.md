# Fishing presentation — transparent lagoon and fish sizes

This stage covers water and fish art. Casting, biting, catching, scores, rounds and fishing rules are deferred.

## Water

The fishing lagoon has a dedicated transparent surface, small continuous wave displacement, overlapping moving ripples, view-dependent sky highlights and animated caustics on a sandy basin. The existing shared ocean remains outside the lagoon. A `LagoonPresentation` component opens the opaque ocean only while the detailed fishing island is active; unloading the island restores the original ocean.

The basin supplies real underwater geometry where the original island was an open ring. It reuses the exact sand texture already shipped by the terrain. The `KeepSourceTexture` material tag tells the Android material pipeline to retain that shared image instead of introducing a second colour-pattern texture. The surface requires no extra scene colour/depth render, reflection camera, texture animation sequence or package. Its transparent edge joins the sea under the shore and through the inlet.

The five fishing stations, four islands, shared travel, collision boundaries and room model remain in place. This is an attractive visual approximation, not a fluid simulation.

## One fish, three sizes

The three prefabs share one gold-and-teal fish design, one vertex-colour material and the same two imported meshes. They differ only in uniform visual scale:

| Prefab | Nose-to-tail length | Relative length |
| --- | ---: | ---: |
| SmallLagoonFish | 0.4 m | 1 |
| MediumLagoonFish | 0.9 m | 2.25 |
| LargeLagoonFish | 1.8 m | 4.5 |

The editable master has 12,792 triangles; mobile LODs have 3,192 and 1,248 triangles. Fish colours, fins, eyes, mouth and gill markings are authored geometry/vertex colours; there are no fish image textures. Gentle shader tail motion is cosmetic. The three sizes are placed underwater beside the first pier for comparison, without swimming AI, capture interactions or network state.

The native master and source audit are in [LagoonFish](../ArtSource/Fishing/LagoonFish/README.md). Prefabs are under `Game/Assets/_Game/Prefabs/Fishing/`; their common FBX and material are under `Game/Assets/_Game/Art/Fishing/Fish/`.

## Rebuild and inspect

Run these examples from the repository root, using `BLENDER_PATH` as documented in [Setup](SETUP.md):

```powershell
& $env:BLENDER_PATH --background --factory-startup --python-exit-code 1 --python Tools/Blender/build_lagoon_fish.py
```

In Unity run **WHATTHE FISH? → Fishing → Prepare transparent lagoon and fish**. The matching batch entry is `LagoonPresentationBuilder.Prepare`; `LagoonPresentationBuilder.BuildWindows` also builds the current saved game. Preparation updates the fishing scene and environment prefab without rebuilding the other islands. `RefinedIslandBuilder` preserves the presentation when the fishing environment is regenerated.

`Tools/Build/Test-LagoonPresentation.ps1` runs the actual Windows player with `-lagoonReview`. Its captures, shader checks, size/sharing checks, water animation/visibility measurements and island reload checks are written beneath ignored `Builds/FishingWaterQA/`. Review those images, including the ordinary pier view, alongside the numeric checks. Desktop inspection does not establish phone transparency quality, touch input, FPS or thermals.

The older `Test-FishingIsland.ps1` now waits for streamed islands and ignores unloaded entries when counting active islands. Its optional `-WithoutPresentation` control run disables the new visual group before checking the existing routes; it does not change the shoreline colliders or relax the route assertions.

Run `Tools/Build/Build.ps1 -Target AndroidSubmission` after changes to delivery assets. See [build size](BUILD-SIZE.md) for the latest measured release and its exact validation scope.

## Validation — 3 October 2026

The final Windows presentation passes **30 checks** with pier/close/distant/shoreline and actual game-camera renders. The size lineup is clearly distinguishable from the pier; individual railings can still occlude fish as the camera moves. The existing island-route probe has **50 passes and 3 failures**, reproduced unchanged with the visual presentation disabled. Those route issues remain open; see [verification details and cleanup](VERIFICATION.md#transparent-lagoon-and-three-fish-sizes--3-october-2026).

The measured frozen-source Android APK is **86,757,976 bytes**, **183,018 bytes above the actual pre-task package**, leaving **13,242,024 bytes** below the strict ceiling. It remains **11,757,976 bytes above the development target**. AndroidSubmission, v2 signature verification and packing audit passed; phone rendering/performance remain unverified. The task-specific APK and current video are `Builds/FishingWaterQA/Android/WhatTheFish-release.apk` and `Builds/FishingWaterQA/lagoon-final-preview.mp4`. The concurrently produced root APK was preserved. See [size provenance](BUILD-SIZE.md#transparent-lagoon-and-three-fish-sizes--3-october-2026).
