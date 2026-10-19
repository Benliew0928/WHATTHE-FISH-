# Golf multiplayer tee grounds

T1 appearance was subsequently integrated with the original island terrain; see
[the current T1 bunker/seam repair](GOLF-T1-BUNKER-SEAMS.md) for current art and delivery.

## Layout

The approved inward T1 proposal and the T2–T5 three-pad proposal are retained in
[the authoring reference folder](../ArtSource/Golf/Course/MultiplayerTees/).
These are design references. The generated `GolfCourse.teeSites` and
`GolfMultiplayerTeeBuild` are the implemented geometry and regeneration source.

- T1: ten pads, three-metre arc spacing, 64 metres horizontally from H1. Its
  centre moves inward from the old entrance-road location. A centre pad now lies
  on the centreline, followed by alternating three-metre offsets to either side;
  ten pads necessarily leave one more outer pad on one side.
- T2–T5: three pads per hole, four-metre arc spacing, centred on the previous tee.
  Each pad on a given hole has the same horizontal distance to its cup.
- Pads use a common level ground per site with a five-metre transition into the
  original terrain. Derived ground meshes have real colliders; turf is not only
  a screen overlay. Original island masters are retained.
- T1 has a rear path at radii 74–77 metres from H1. The old entrance spur is
  reclaimed. Opening sand lies at radii 39–44 metres, rough at 23–27 metres.
  Surface resistance follows the same geometric masks as the rendered surfaces.
- Cups, ponds, the other-hole routes, shot strength, swing timing, scoring and
  camera controls retain their existing rules.

## Assignment and ownership

`GolfTeeReservations` owns reservations on the authority. It selects the nearest
free pad to the centreline, randomizes ties between equal-distance sides and
randomizes players arriving in the same physics batch. Initial entrants are
shuffled together. Existing assignments are not recomputed from roster order.

Three simultaneous arrivals use the centre and opposite side pads. With a full
three-pad tee, subsequent arrivals wait without a live visible ball. The HUD
shows `Waiting for a free tee`. Once a safe pad is available, earlier arrival
batches have priority. Queueing does not teleport players or restrict their
walking controls. The wait is for placement of their next ball.

A reservation is retained until the ball, player, last-shot origin and last-dry
anchor have all cleared the pad by five metres and the player is no longer in a
swing. This intentionally protects a tee that could still be used by water or
out-of-bounds recovery. Usually it is freed after a subsequent shot away from the
tee. Progressing to the next hole, finishing or leaving also releases occupancy.

The server checks physical ball/player clearance before reusing a free pad.
Snapshots carry reservation, queue and recovery-anchor state; clients present
the authoritative ball positions. Room recovery restores these records before
enabling balls. Own-ball-only striking remains in place.

## Validation and delivery

Current task evidence is kept under ignored `Builds/GolfMultiplayerTeeQA/`.
Opening measured release APK: **88,552,098 bytes**. Fresh AndroidSubmission at
20:42:18 Malaysia time on 19 October 2026: **90,295,414 bytes**, increase
**1,743,316 bytes**, strict decimal-GB headroom **909,704,586 bytes**. The official
budget gate and independent APK v2 signature passed. SHA-256:
**87BBBEB84D4D3FEDD8C9A57432801A29481020FA21D307336121940535C9DA7D**.

Final checks: 399 editor assertions, 28 four-player runtime assertions and seven
assertions each in real local host/client processes. Editor captures include
top and near views of each tee. Runtime checks include three simultaneous hole
completions, unique next-hole placement, full-site queue, safe centre reuse,
water return and protected recovery anchors. Real peers verified authoritative
slot/position snapshots. No WAN, physical-phone FPS/quality, or actual
host-migration session was tested.

The complete final Windows player is in `Builds/WindowsFinal/`. The old delivered
player/APK were archived in `Legacy/20261019-203005-332-golf-multiplayer-tees/`.
The same-task intermediate Windows player was superseded after final path checks
and archived in `Legacy/20261019-203940-208-golf-tees-final-path/`. Both archive
batches are local and ignored; the archive tool recorded hashes/recovery paths.

Three build-only authoring changes were restored after review: a character
source-hash cache, equivalent Golf Ball prefab IDs/order, and quality normalization.
These were clean at task opening. Feature sources and pre-existing user changes
were retained. Raw records are in `restored-build-noise.json` in the evidence folder.

Equal distances do not prove identical shot difficulty: approach angles differ.
Terrain, actual support and start occupancy are checked separately. Windows
captures do not establish physical-phone quality or performance.
