# Golf event viewing demonstrations

Tropical Wind, Bounce Party and Sticky Green now join Tornado and Crab Crossing in the existing 3.5-second viewing-camera flow. Island Rush now uses the same viewing flow with a cart pass and three chasing runners. The HUD fades out/in with the camera; Settings remains accessible.

- Tropical Wind: an oblique view of a moving golf ball with a visibly bending airborne path and wind streaks. The bend uses the accepted event wind direction, with lateral displacement increased from 3 to 6 metres.
- Bounce Party: a following oblique view of a ball falling from 8 metres (previously 4), two diminishing rebounds, orange contact ripples and existing turf impact audio.
- Sticky Green: a ball falling from 8 metres (previously 4) stops at first contact; a contracting green ring and short clinging strands show the sticky surface, with one turf impact.

`GolfEventShowcase.cs` samples the presentation path by event age and clones only the original golf ball mesh/materials. The example ball uses the real gameplay radius (0.0645 metres); the camera moves closer for legibility and has no GolfBall component, Rigidbody, collider or network identity. It never changes a real shot, player position, score, event duration or map. The temporary stage is selected on nearby clear ground; an obstructed location skips the camera rather than placing the example in geometry. Render objects are cleaned up when the presentation ends or is interrupted.

`ActivityCinematic.cs` retains activation timestamps, future/late snapshot handling and interruption rules. Aiming, charging, shot replay, driving, menus and Reduced Motion retain their existing camera protections. Existing GolfPresentation colours, golf-ball mesh/material, trail shader and sound bank are reused. No new textures/audio or scene changes are required.

Presentation timings/path coordinates are centralized in `GolfEventShowcase.Sample`; camera framing is in `GolfEventShowcase.Camera`. The existing `GolfPresentation` settings control wind, bounce and sticky colours. The demonstration is intentionally illustrative, rather than a prediction of the player's next shot.

## Island Rush and localized badges

`GolfRushShowcase.cs` creates a render-only copy of the existing cart, a seated driver and three runners on a checked dry, unobstructed lane. A continuously accelerating approach leads into a close pass, a camera turn and a view of the chase. Existing four-wheel ground support sets the cart height/tilt, while runners sample the actual floor. Distance-driven wheels/strides, moving ribbons, the existing motor loop and a pass-by sound show acceleration. Everything fits the existing 3.5-second event introduction and is disposed on return or interruption. The cast has no gameplay colliders, cart controller, Athlete or network authority.

Travel and timing are centralized in `CartTravel`, `RunnerTravel`, `Present` and `Camera`; lane selection is in `FindLane`. The cinematic is illustrative and does not spawn a usable cart or move players. Reduced Motion and existing aim/menu/camera interruption rules apply.

`GolfEventHUD.cs` uses 0.72 line spacing and per-badge text safe areas. `LocalizedArtwork` opts these captions into measured ink fitting (including outlines), preserving the original art and keeping two-line Malay labels close together. `GameLocalization.ArtworkWords` supplies explicit line breaks for Island Rush and Sticky Green as well as the existing two-line names.

## Previous delivery evidence

125 Golf assertions and 1,107 Fishing assertions passed. Golf checks include curved flight, two diminishing rebounds, static sticky position/rotation, collision-free examples, event deadline preservation, camera/HUD fades, Settings access, late snapshot handling, Reduced Motion and aim protection. Actual frames were inspected at launch, flight, contact and settled phases.

Final evidence: `Builds/ActivityCinematicsQA/Golf-20261025-214310-283/`, `Builds/ActivityCinematicsQA/Fishing-20261025-214430-706/` and `Builds/GolfEventShowcaseQA/`. Six 5-second silent simulation videos are under `Builds/GolfEventShowcaseQA/Videos/`. The earlier Golf recording is an intermediate diagnostic run; only the final run validates the delivered version.

Windows development shortcuts after a match starts: F1 Tropical Wind, F3 Sticky Green, F4 Bounce Party. Complete Windows player was promoted to `Builds/WindowsFinal/`; prior player archived in `Legacy/20261025-214716-326-golf-showcases-windows/`. Phone rendering, touch and FPS have not been tested.


## Golf Rush revision verification — 25 October 2026

The final widescreen run passes **176 Golf assertions**, including cart ground support, camera reversal, three visual-only runners, cleanup, actual ball radius, doubled drop height and unclipped Malay/Chinese badge outlines. Fishing regression passes **1,107 assertions**. The localization catalog/font check covers 3,445 entries. All 520 Windows compiled source documents match the workspace snapshot.

Current evidence: `Builds/ActivityCinematicsQA/Golf-20261025-221829-070/`, `Builds/ActivityCinematicsQA/Fishing-20261025-221317-501/` and `Builds/GolfRushShowcaseQA/`. Six final silent 30 simulation-fps clips and a combined reel are in `Builds/GolfRushShowcaseQA/Videos/`. The 3.5-second presentation is surrounded by normal gameplay in each 5-second clip. The 960x720 four-by-three run also passes all 176 Golf checks: `Builds/ActivityCinematicsQA/Golf-20261025-222123-698/`. Phone rendering/touch/FPS and cloud multiplayer remain unverified.

The complete 337-file Windows player was promoted and SHA-256 compared in `Builds/WindowsFinal/`, retaining both awards preview launchers and concurrent awards/localization work. Existing development shortcuts after starting Golf: F1 Tropical Wind, F3 Sticky Green, F4 Bounce Party and F6 Island Rush. Disable Reduced Motion to see camera travel and avoid aiming/driving during the cue.

Reviewed superseded Windows player: `Legacy/20261025-222054-487-golf-rush-windows/`. Superseded APK/audit: `Legacy/20261025-221444-200-golf-rush-android/`. Intermediate films/videos: `Legacy/20261025-221909-640-golf-rush-drafts/`; diagnostic check results remain in the current evidence folder. No Git publishing actions were taken.

Final AndroidSubmission: **166,803,592 bytes**, up **1,356** from **166,802,236**, with **833,196,408 bytes** headroom. Signature, ZIP/bundle integrity and all 366 Android source hashes pass. `Check-TaskReady.ps1 -RequireApk` and diff whitespace checks pass. See [build size record](BUILD-SIZE.md) for the APK hash and audit.
