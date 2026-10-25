# Fishing and Golf podium ceremonies

Both ceremonies retain real match scoring and use render-only character copies.
The scene, actors and temporary renderer visibility changes are removed/restored
on completion, skip, cancellation, travel or component destruction. Winners use
their actual character identity; the preview intentionally reuses the local
avatar for three sample placements.

## Selection

- Golf: existing ranked results, first three finished players, DNF excluded.
- Fishing Solo/round: descending round score, owner ID only stabilizes equal
  scores. Show up to three players and retain the existing shared ranks.
- Fishing final Cup: cup points, then cumulative catch score, then stable ID;
  retain the existing Cup ranks. Earlier Cup rounds use round scores.
- Fishing Crew: show the entire crew without competitive ranking badges.
- Tied ranks share the same platform level with separated standing positions.
  No floating rank medals or place captions are drawn above the characters.
  Fewer than three winners leave unused podium levels empty.

## Sequence

Fishing uses a common stage at the lagoon's actual centre and water height,
oriented toward pier 0 for every viewer. A moving wide pool shot introduces soft
grey mist; the stage rises under it and the mist disperses. From 4.8 seconds,
winners enter the extended central wooden bridge in sequence. Golf uses the
same single-file entrance from 1.5 seconds, with ground height sampled along
the grass approach. First place leads, followed by the remaining finishers.

Both entrances move at 0.82 metres/second with 1.85 metres between players
(approximately 2.26 seconds between entrances). The 0.92-metre gait cycle is
calculated from travelled distance; stance feet cancel root motion rather than
sliding to an independent animation clock. A tracking shot follows the line,
then a brief dissolve skips the remaining walk/climb. Three winners reach their
platforms at 12 seconds in Fishing and 8.7 seconds in Golf. Five-member Crew
extends its entrance until about 15 seconds so everyone is seen on the bridge.

Once seated, the camera travels through a 36-degree left-to-right arc over
8 seconds, gently approaching the podium. Quintic easing has zero velocity and
acceleration at both ends: slow departure, faster middle, gradual arrival. The
camera then holds its final composition. Fishing lasts approximately 21 seconds;
Golf remains open until Next adventure. Show results opens the existing ranking
over Golf's podium without ending the ceremony.

The top-centre title panel uses its original 1100 x 124 layout and vertical
position. The podium's physical numerals use a depth-tested font shader, so
characters in front correctly hide them. Localization font/atlas changes retain
that depth behavior. The original floating badge artwork is retained as a source
asset but is not shown by these ceremonies.

Reduced motion skips the walk, smoke and camera cuts, directly presenting the
fixed podium composition (Fishing approximately 8 seconds; Golf retains its manual results/exit flow). Existing Show results
and Golf Next adventure controls remain available.

## Local preview and reproduction

Launch the complete development Windows player from the repository root:

```bat
start "" "Builds\WindowsFinal\WhatTheFish.exe" -sport Fishing -offline -fishingAwardsPreview
start "" "Builds\WindowsFinal\WhatTheFish.exe" -sport Golf -offline -golfAwardsPreview
```

F10 replays after the preview ends. Preview mode never creates synthetic
scores or finishes a live room. `-islandAwardsReview <output-folder>` runs the
presentation checks and screenshots; add `-podiumFilm` for 30 fps source frames.
`WhatTheFish.Editor.IslandAwardsBuild.Windows` regenerates the assets and builds
the test player under ignored `Builds/IslandAwardsMotionQA/Player/`.

The complete Windows delivery also includes `Preview-Fishing-Awards.cmd` and
`Preview-Golf-Awards.cmd` beside the executable. These resolve the executable
relative to themselves and work from an unrelated working directory.

## Current motion revision verification — 25 October 2026

All **180 runtime assertions** passed across English, Simplified Chinese and
Bahasa Melayu (27 Golf + 33 Fishing per language). Checks cover single-file
order, queue spacing, complete queue framing, actual skeletal foot planting,
depth-tested digits after localization, no floating medals, eased camera speed,
left-to-right travel, shared ranks, five-player Crew, reduced-motion camera,
input/renderer restoration and Golf's manual results flow. Measured horizontal
stance-foot drift was approximately 7.7 mm while the actor moved 16.4 cm.

Full 1280x720 captures were reviewed in all three languages. The current silent
videos show 510 Golf frames / 17 seconds and 630 Fishing frames / 21 seconds.
They are deterministic choreography recordings, not real-time phone FPS tests.
The normal settled sweep lasts 8 seconds; a five-person Crew compresses that
sweep to about 5 seconds so it still decelerates before the ceremony ends.

Evidence: `Builds/IslandAwardsMotionQA/` contains language captures, the two
`*-podium-preview.mp4` files, runtime summary, APK/signature checks and source/
delivery hash comparisons. All 510 Windows and 365 Android compiled documents
matched at initial verification; all 334 candidate files matched WindowsFinal before
notes/preview launchers were added. AndroidSubmission is **166,802,236 bytes**;
see [BUILD-SIZE.md](BUILD-SIZE.md) for the measured baseline and headroom.

Superseded complete player: `Legacy/20261025-215707-063-awards-motion-windows/`.
Preceding APK/audit: `Legacy/20261025-215311-800-awards-motion-android/`.
Intermediate reviewed candidates are retained in the `awards-motion-framing`
and `awards-title-review` archive batches; the promoted duplicate is retained
in `Legacy/20261025-220013-521-awards-motion-promoted/`. No publishing or history
changes were performed.

Source snapshot limit at final handoff: all awards-specific compiled sources
still match. Subsequent concurrent edits to ActivityCinematic, GolfEventHUD,
GolfEventShowcase, GameLocalization and LocalizedArtwork (plus the Windows-only
ActivityCinematicReview) postdate initial validation. Final comparison found
6 Windows / 5 Android differences in those files. They are preserved in the
workspace but are not claimed as tested by this awards delivery.

## Previous delivery verification — 25 October 2026

The final Windows candidate passed **117 runtime assertions** across English,
Simplified Chinese and Bahasa Melayu: three cast identities, no added gameplay
colliders, correct entry order, podium foot positions, fixed settled camera,
real player preservation, cleanup, shared ranks/medal separation, five-member
Crew, reduced motion and the existing manual Golf results flow.

Full 1280x720 game renders were reviewed in all three languages. Silent 30 fps
presentation captures contain 510 Golf frames (17 seconds) and 630 Fishing
frames (21 seconds). These deterministic captures demonstrate choreography;
they are not a real-time device frame-rate benchmark. The preview uses the
local avatar as the three sample winners, while live matches resolve each
winner's actual character identity.

Current evidence is under ignored `Builds/IslandAwardsQA/`: `FilmGolf/`,
`FilmFishing/`, language-specific runs, the two `*-podium-preview.mp4` files,
source comparisons and the complete Windows delivery hash manifest.
All **509 Windows** compiled source documents matched at verification.
All **335 candidate files** matched the delivered copy before delivery notes
and the two preview launchers were added. The shared source includes the
concurrent first-person body, welcome/loading, localization and manual Golf
results work; these changes were preserved.

The previous complete Windows player is recoverable from
`Legacy/20261025-205129-978-island-awards-windows/`. Superseded visual checks are
retained in the `island-awards-refinement` and `island-awards-tie-review` archive
batches. See the current [APK size record](BUILD-SIZE.md) for release bytes.

Reference provenance and editable master: [IslandAwards art](../ArtSource/IslandAwards/README.md).
Device performance, touch and cloud multiplayer synchronization require actual
device/network qualification; Windows renders do not establish those results.
