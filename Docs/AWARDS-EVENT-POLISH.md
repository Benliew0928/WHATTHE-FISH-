# Awards and Golf event presentation polish

Updated 25 October 2026.

## Delivered behavior

- Fishing retains the podium rising from the lagoon but no longer creates the grey reveal mist. The bridge continues the original 0.33 m plank pitch, alternating timber palette, nails and rope-wrapped posts out beyond the entrance cameras. Its deck ends at approximately 36.9 m from the podium origin. The visible connection and walking surface retain the existing design.
- Golf's establishing and single-file entrance shots share a fixed framing envelope. They move forward smoothly, without backing away when another winner appears. The existing seated celebration sweep and reduced-motion hold remain.
- Golf, Fishing, Football and Basketball provide a localized bottom-left **Next adventure** action. Fishing also retains that action on its completed-round results card; Football and Basketball no longer duplicate the action on the right-hand scorecard. The shared post-match flow retains host/guest restrictions.
- Tropical Wind's demonstration ball rebounds using `GolfBallPhysicsSettings.LandingVelocity`: the first landing loses the same horizontal speed as gameplay, subsequent bounces lose energy, and turf contact sounds accompany the visual contacts. Only the demonstration path changed; actual ball physics and scoring remain unchanged.
- Island Rush's render-only runners use the actual imported `Running` clip, evaluated through a manual animation graph. They reuse the live `GolfRushTrail` renderer, colours, leaves, glints, foot puffs and behind-player clipping. Preview animation graphs and world-space trail meshes are released with the cast.

## Verification

The newest shared Windows candidate was used for final validation so that concurrent venue collision work was retained. All **514 Windows** and **368 Android** compiled source documents matched at validation. The later shared AndroidSubmission rebuild (15:53 UTC) retains all awards/event changes and passes the same APK integrity checks. At final handoff, one Windows-only venue review helper differs from the tested player; all awards/event runtime sources still match. Further concurrent venue validation is outside this task.

| Check | Result |
| --- | --- |
| Final shared Golf awards | 30 assertions passed |
| Final shared Fishing awards, ties and Crew | 38 assertions passed |
| Final shared Golf event/camera/results regression | 181 assertions passed |
| Island awards layouts in English, Simplified Chinese and Malay | 204 assertions passed across six runs |
| Football ceremony and three-language ending navigation | 93 assertions passed |
| Basketball ceremony and three-language ending navigation | 99 assertions passed |
| Shared localization catalog/font coverage | 3,456 complete entries passed |
| Existing localization artwork integrity | 16 original artwork hashes passed |

Checks include monotonic entrance camera motion, retained podium rise with no mist, bridge extent, single-file spacing, planted-foot stability, rank depth testing, input restoration, bottom-left pointer access and translated text bounds. Event checks verify real damping values, actual running-bone motion, live-trail reuse and no vertices in front of runners. Screenshots and frame sequences were visually reviewed, including the bridge connection, running gait and all affected language layouts.

Evidence and four MP4 previews are under `Builds/AwardsEventPolishQA/`. `SharedGolf/`, `SharedFishing/` and `SharedEvents/` exercise the final shared candidate. `FilmGolf/`, `FilmFishing/` and `Events-GolfV2/` preserve the motion recordings. Court navigation sources are unchanged between their checked candidate and the final delivery.

The complete **335-file** candidate copy was SHA-256 verified, and the two existing awards-preview launchers were retained, yielding the **337-file** player in `Builds/WindowsFinal/`. Use `Preview-Golf-Awards.cmd` or `Preview-Fishing-Awards.cmd` there for local visual-only previews.

## APK and preservation

AndroidSubmission APK: **166,808,788 → 167,993,556 bytes**, an increase of **1,184,768 bytes**. Headroom under the strict 1,000,000,000-byte cap: **832,006,444 bytes**. SHA-256: `8EE1178E78BE6CCC4BBF46C7D55821D5C8D7EAA68F2EACD32684E010FA16EA1A`.

APK v2 signature, ZIP CRC, all 1,405 audited entries and the five-scene bundle hash/size pass. The largest increases are the shared island bundle (+585,135 compressed bytes, including the concurrent venue collision work) and the extended Fishing bridge's rope mesh (+440,374), nail mesh (+57,630) and timber mesh (+32,766). The existing batched materials are reused; no new textures or lossy artwork changes were introduced.

Preserved archives:

- `Legacy/20261025-234152-931-awards-event-polish-android/`: previous APK and size audit.
- `Legacy/20261025-234402-673-awards-event-polish-draft/`: superseded first Windows candidate; its test evidence remains under `Builds/`.
- `Legacy/20261025-234841-026-awards-event-polish-windows/`: superseded complete Windows delivery.
- `Legacy/20261025-235257-860-awards-event-polish-candidate/`: revision-2 test player, superseded by the newer shared candidate delivered to `WindowsFinal/`; recordings and checks remain under `Builds/`.

The procedural bridge meshes contain no new external image/library dependencies. Unrelated venue collision source/assets were preserved. No staging, commit or push was performed for this task. Physical Android device rendering, touch/FPS and live network room acceptance remain unverified by these local checks.
