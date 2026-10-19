# Shared lagoon and sports-style controls

This records the 18 October release. The current camera, swimming and rope
feedback are described in [the current release note](FISHING-NATURAL-SWIM.md)
and [the Fishing guide](FISHING-MINIGAME.md).

The Fishing island keeps Hold the Glow: hold the lit Left or Right button,
switch with the glow, and release both to cool the line. Hook timing, points,
Wanted, Round, Cup, Crew, six abilities and two favourites remain unchanged.

- The top-right gear uses the Basketball/Football plate, gear artwork and press
  animation. It offers three saved camera views, the same four audio preferences,
  the existing Fishing tutorial and a confirmation before leaving the island.
  The separate Camera and Leave HUD buttons are hidden. Jump reuses Basketball's
  illustrated button and disappears while a line is active.
- A larger centre track separates the round fish marker from the small input
  dot. The gold direction band still agrees with the lit control. Rope colour,
  centre tension fill and the small rope-side percentage gauge use one gradual
  teal/gold/orange/red scale. The badge stays inside the safe frame and above the
  bottom controls; it does not intercept touch input.
- Thirty fish migrate continuously through the common pool. There are ten of
  each size. A fish ID has no pier ownership; the host still grants one claim
  at a time and enforces range, visibility, cooldown and cast/round tokens.
  Long casts normalize the spring/drag solver to a maximum 12 m retrieval range
  and restore world distances for presentation. Near casts retain their winding
  law. This bounds fight duration without changing directional holds or scores.
- Room entry and Game-area travel reuse the existing synchronized sky descent
  and blue arrival effect. Room seats give active players separate piers;
  spectators land on shore. Match start waits for travel and resolves the entire
  roster's landing positions before moving anyone. Station destinations remain
  available for shared Sky-Sail travel; starting Fishing places players at piers.
- A short awards camera reuses the sports' render-only character clones and
  Basketball celebration takes before opening the existing results card. Tied
  winners share honours; Cup uses its existing tie-break; Crew success celebrates
  everyone. Failed Crew rounds thank the crew, and abandoned rounds skip awards.
  Spectators can watch. Show results skips the animation. Context changes cancel
  the camera and restore input/renderers without awarding points.

Matching multiplayer builds require network protocol **69**. No new raster art,
3D model, audio recording, package or shader is introduced by this change.

## Verification and run paths

A fresh complete Windows player is installed in `Builds/WindowsFinal/`.
The source starts from `9bb5564` with the shared-lagoon changes left uncommitted.
Verification completed **5,222** assertions/checks:

- 1,891 authoritative rules checks, including public claims and 5.5–40 m fights.
- 2,689 rendered gameplay/input/layout checks and 178 results/feedback checks.
- 95 host/guest checks, including cross-pier claims, shared scores and rematch.
- 245 three-peer travel checks across four sports, including distinct Fishing
  piers, shore spectators and all-or-nobody placement when a pier is blocked.
- 20 Fishing/teleport lifecycle and shutdown checks.
- 104 cold/warm packaging checks. On this laptop, the final isolated check
  reached the world in about 45 seconds cold and 3 seconds warm. These are
  desktop measurements, not mobile performance claims.

The full Windows build includes the updated serialized HUD layout. Subsequent
code-only refreshes retain verified identical native player data, catalog and
island bundle hashes. The final refresh only corrects a development packaging
check: disabled pooled effects may wait for a material until first activation;
assigned shaders and visible renderers are still checked. Basketball gameplay
is unchanged. The held-ability position follows the larger meter's actual height
and its landing-card separation check passes.

The fresh AndroidSubmission build passes its automatic under-1-GB gate and APK
Signature Scheme v2 verification. All 467 recorded C# source hashes remained
unchanged during the build. The previous APK is archived and is not used as
validation of these changes.

After building, run `Builds/WindowsFinal/Explore-Fishing.cmd`.
The self-contained Android output is `Builds/Android/WhatTheFish-release.apk`.
These outputs and raw evidence are local and ignored by Git; a fresh clone must
build them first.

The task's build logs and source records are under `Builds/FishingSharedLagoon/`.
The completed reports are indexed by `validation-completed.json`; complete
Windows hashes are in `windows-player-integrity.json`. An initial three-peer
test exceeded its cold preparation allowance during concurrent compilation.
The travel rerun uses the same prepared content key through an ignored harness;
independent cold/warm packaging checks then passed without concurrent builds.
The previous players are preserved in these ignored archive batches:
`Legacy/20261018-222602-809-fishing-shared-lagoon-apk` and
`Legacy/20261018-225001-933-fishing-shared-lagoon-windows`.
Superseded generated test caches are in
`Legacy/20261018-225722-177-fishing-shared-lagoon-test-caches`; their reports
remain with the task's evidence.
Android build/signature evidence is in `android-submission.log`,
`android-signature.txt` and `android-delivery.json`. Build-generated character
hashes and Golf prefab IDs were restored to the pulled source after a
format-aware equivalence check, recorded in `final-import-restoration.json`.
The production Fishing gameplay probe checks public-pool claim races and catch
duration across 5.5–40 m casts, saved settings, unchanged directional controls,
shared-school containment/continuity, and ceremony skip/context cleanup.
Rendered and host/guest reports are generated by `Test-FishingGameplay.ps1`;
the results/feedback suite is `Test-FishingPolish.ps1`.

Windows checks do not establish physical-phone touch quality, sustained mobile
frame rate or wide-area multiplayer reliability. See the current delivery note
for the checks actually completed for the latest player and APK.
