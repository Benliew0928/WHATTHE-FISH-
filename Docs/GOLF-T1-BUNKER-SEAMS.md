# T1 localized bunker restoration and joined terrain

## Requested scope

Restore only the annotated blue-circle area to the pre-multiplayer-tee design;
repair the block/step artefact near the entrance; move the Hole 1 / PAR 3 /
Bunker Bend sign beside T1. Keep pad allocation, other holes and gameplay rules.

## Changes

- The previous separate T1 opaque floor was offset above the original ground.
  Its cut boundary and intersections with older translucent fairway meshes could
  appear as raised zigzag blocks. It has been removed with its extra collider.
- T1 now grades existing original terrain vertices, retaining the island's exact
  triangle topology and UVs. There are no new ground patch boundaries or hanging
  mesh edges. Vertex positions/normals outside the grading footprint stay intact.
- The restored area uses the original terrain maps and original course dressing
  masters. Baked grass triangles are restored only within that footprint with
  position-based duplicate rejection. Original island-kit placements and circuit
  obstacles whose original centres lie in the circle return to their source
  layout positions. Other placement centres are not restored.
- Bunker/rough override rules are disabled inside the restored footprint so ball
  surface resistance follows the original sand and grass again.
- Narrow footpath/planting masks are evaluated per pixel using the existing island
  UV mapping and textures. This removes coarse vertex colour banding. Evaluation
  is bounded to the T1 area; no new bitmap textures or per-frame CPU work.
- The existing sign and its post move beside T1. Its world-space Canvas position
  is saved through RectTransform.anchoredPosition3D; text/design remain intact.

Implementation: GolfTeeIslandBlend.cs, GolfT1Restoration.cs,
GolfMultiplayerTeeBuild.cs, GolfTeePaths.cs, GolfTeeSite.cs and GolfTeeIsland.shader.
Exact course/terrain triangle intersections are built in GolfT1Drape.cs,
preserving original UVs, alpha masks and interpolated terrain normals. This avoids
coarse draped faces cutting through the grading fringe. Measured visible dressing
clearance is approximately 0.025 metres.
Editor verification is in GolfMultiplayerTeeReview.cs and GolfT1GroundReview.cs.

Original annotated/bug references are retained in
[ArtSource/Golf/Course/T1Restoration](../ArtSource/Golf/Course/T1Restoration/).

## Evidence and delivery

Current evidence is under ignored `Builds/GolfT1SeamQA/`. Actual Unity views include
tee-1-top.png, tee-1-near.png, tee-1-entry-low.png and tee-1-entry-low-side.png.
- 457 editor assertions passed, including original terrain topology, unchanged
  grading-exterior vertex positions, restored blue-circle colour/geometry,
  matching shared-vertex heights and visible course dressing clearance.
- Actual visible dressing clearance measured 0.0248-0.0250 m at triangle centres;
  transparent-only intersections are reported separately.
- 28 assertions passed in the final complete Windows player with four fixture
  actors: tee support/alignment, Aim, simultaneous completion, queue/reuse,
  recovery anchors, water return and ownership rejection.
- Opening/final tee-site records match in both scene and prefab. Full tested
  WindowsFinal player promoted; all 326 candidate files compared by SHA-256
  before appending the delivery note.
- Fresh AndroidSubmission at 00:32:45 Malaysia time on 20 October 2026:
  **90,502,486 bytes**, down **293,360** from **90,795,846**;
  strict decimal-GB headroom **909,497,514 bytes**. Official budget gate
  and independent APK v2 signature passed. [Measured size record](BUILD-SIZE.md).
- No new real-network/WAN or physical-phone quality/FPS test.

## Archive and source snapshot

Superseded complete Windows player/APK and three confirmed unused tee derivatives
(with their metadata) were archived using the repository archive tool:
`Legacy/20261020-003001-935-golf-t1-bunker-seams/`. Active original terrain/grass,
course surface masters and all other tee sites remain in place.

Delivery uses the current working tree, including pre-existing local work. Normal
build-only prefab IDs/cache/quality normalization are reviewed separately from
feature changes. No staging, commit, push or history rewrite is authorized here.


The build-generated character source hash, equivalent Ball prefab IDs/order and
single anti-aliasing normalization were compared with the task-opening byte
snapshots and restored. Their generated versions and reviewed differences remain
in ignored task evidence; the tested binaries contain those ordinary build
transformations. All other pre-existing local changes were preserved.
