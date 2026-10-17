# Personal football trails

The football is **52.8 cm in diameter**, 20% larger than the previous 44 cm.
The visual mesh, sphere collider and centre-spot height agree. The original
editable model and pigments are retained. `SoccerBallBuilder` uses the same
diameter when regenerating the prefab or stadium.

Choose **Football → Customize**, or open the pitch settings and choose
**Customize** during play. Room guests can open Customize without gaining
permission to edit the host's venue. A selected card is highlighted and ticked;
changes apply immediately and save on the device.

| Design | Appearance |
| --- | --- |
| Electric Bolt | Forked blue-violet lightning with a bright core |
| Ocean Blue | Two flowing blue/cyan ribbons |
| Rose Sparkle | Pink/lilac glow with four-point stars |
| Solar Flame | Gold/orange moving flame filaments |
| Aurora | Interwoven mint and violet waves |

The choice is the **viewer's preference**, never the kicker's equipment.
Each device renders the shared football using its own selection while anyone
dribbles, passes, shoots or deflects it. Replay copies use that same local choice.
No trail preference, particle state or effect ownership is sent over the network.
The local preference key is `football.localTrail`; absent or invalid values use
Electric Bolt. Venue appearance remains the host's shared setting.

One procedural shader and one resource material supply all five designs and the
animated selection previews. Five cached material variants are shared locally;
there are no new textures, audio clips, meshes, packages or network messages.
The trail samples displayed motion, including interpolated guest poses and
attached dribbles. It stops emitting when stationary, fades within 0.55 seconds,
and shortens at higher speed. Reset, teleport, portal, replay handoff and island
travel clear or suppress its history. Reduced motion stops the shader animation.

## Reproduce the checks

- Build the candidate with Unity's `FootballTrailBuild.BuildReview` method.
  Its complete player is written to `Builds/FootballTrailQA/Player/`.
- Run `Tools/Build/Test-FootballTrail.ps1` for actual rendered swatches, menu
  selection, five real kicks, dimensions, stationary fade and reset checks.
  `-Width 844 -Height 390` exercises a compact landscape layout.
- Run `Tools/Build/Test-FootballBall.ps1 -PlayerPath <candidate-exe>` for
  physics and host/guest kicks with different local selections.
- Run `Tools/Build/Test-FootballReplay.ps1 -PlayerPath <candidate-exe>` for
  replay and kickoff regressions, including each viewer's replay trail.
- Build Android with `Tools/Build/Build.ps1 -Target AndroidSubmission`, then
  inspect `Builds/SizeAudit/latest/` and run `Check-TaskReady.ps1 -RequireApk`.

Paths above are relative to the repository; script defaults resolve from the
script location. Generated evidence stays under ignored `Builds/`. The measured
release and platform limits are recorded in [BUILD-SIZE.md](BUILD-SIZE.md),
[APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md) and [VERIFICATION.md](VERIFICATION.md).
