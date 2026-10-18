# Football full-time ceremony

The football result presentation now celebrates the winning squad, isolates the MVP, and eases into the existing results card with the same animated 3D player. Goals, assists, completed passes, interceptions, ranking and MVP points keep their existing rules. Basketball has its own [courtside ceremony](BASKETBALL-CELEBRATION.md). The older portrait assets are retained pending a separate reference audit.

## Direction and timing

- A 0.65-second full-time dissolve covers the move from the gameplay camera to the pitch ceremony. The stadium reveals over another 0.85 seconds.
- The first 15 seconds celebrate the winning squad with staggered cheers, crowd acknowledgements and applause. A wide opening shot establishes the stadium, giant screens and sky fireworks, then descends into a slow arc across the formation.
- From 15 to 25 seconds, the MVP receives an individual spotlight. The camera moves between the forward wings into the solo framing during the first 2.4 seconds.
- From 25 to 27.2 seconds, the camera moves to the left-column composition while the results card fades in. Animation time continues through this move.
- The final performance repeats every 14.4 seconds: breath, chest acknowledgement, raised-arm salute, open gesture, applause and a proud recovery. Matching poses and cyclic shape-preserving tangents join the loop.
- “Show results” uses a covered, eased transition. The stats controls become interactive once the transition settles. “Next adventure” restores the previous camera and input state.

Draws celebrate both squads. An MVP from the opposing side receives the individual award after the winning team, with a covered handoff. Retained records can rebuild a departed player's kit, number and hairstyle from the identity library.

## Stadium broadcast

Two 24-metre LED displays, one above each end stand, share the stadium broadcast. During idle, team selection, kickoff and play, the top 30% shows the current Blue/Red score and match status; the lower 70% shows live ball-following footage. Three elevated views change every 14 seconds through a 2.8-second blend. Tracking includes short velocity anticipation, the pitch boundaries and high lobs. Near the camera-side touchline, a raised view and inward focus prevent a steep overhead rotation. The camera operates independently of a player's first-person, third-person or spectator view.

A goal uses the same authoritative timeline as the player replay: a full-screen diagonal goal/score wipe, the recorded four seconds before and one second after the goal, then a covered return to live coverage. The feed films the existing replay's visual copies and camera direction, without duplicating physics, audio or network events. The normal header disappears for the full-screen replay. A client without the required recorded history sees an explicit replay-unavailable goal card until the shared return. Cancelling the match restores live coverage.

During results, the screens switch to the winning team, final score, live 3D MVP footage, goals, assists, passes, interceptions and points. MVP labels remain hidden throughout normal play. The existing Sunvale structure supports the modular world-space displays; no Blender source or external textures are changed.

Eight bronze-trimmed flame pylons and four roof laser mounts decorate the stadium. During the 15-second team segment, the pylons sustain layered red/orange/gold flames and drifting embers. Six violet electric arcs connect the side pylons, twelve cyan/magenta beams sweep slowly above the end stands, and 30 staggered sky shells leave broad saturated trails. The shader preserves coloured silhouettes against the daylight sky instead of depending on white bloom. The effects rise over 0.65 seconds and ease away over the last 1.1 seconds, leaving the ten-second MVP spotlight clear. The hardware remains visible during ordinary play; celebration emissions do not run during live matches.

Seeded analytic effects share the replicated match clock. The existing snapshot deadline retains the exact finish timestamp; the rendered server clock advances the camera between physics ticks. After a winning goal the show waits until the goal replay ends. Players, guests and spectators derive the same stage and timing, including when a presentation is enabled late. Local skip or dismissal does not end the arena broadcast. A new match clears the effects and replaces the MVP studio with live pitch coverage. Leaving football or disabling the broadcast hides the hardware and releases the camera and texture.

One render texture serves both screens, capped at 20 captures per second: 1024 x 336 during live coverage, 1024 x 480 for full-screen replay, or 640 x 548 for the isolated render-only MVP. Aspect and camera changes occur under each opaque wipe. Layer 30 contains the screen geometry and is excluded from the match feed; layer 31 isolates the MVP studio. First-person body visibility is temporarily restored for a live capture and its exact renderer flags are restored immediately afterwards. Celebration effects use one bounded dynamic mesh and a procedural shader, with no per-particle objects, collisions or replicated effects. The twelve fixtures share a static mesh and two materials, which are released with the stadium. They are generated in code without changing Blender dependencies.

## Source and authoring

- [Presentation and camera](../Game/Assets/_Game/Shared/UI/FootballMVPCelebration.cs)
- [Existing results data and layout](../Game/Assets/_Game/Shared/UI/FootballMVP.cs)
- [Render-only character rig](../Game/Assets/_Game/Sports/Football/FootballCelebrationActor.cs)
- [Default authored performances](../Game/Assets/_Game/Sports/Football/FootballCelebrationPoses.cs)
- [Editable motion library](../Game/Assets/_Game/Resources/FootballMotionLibrary.asset)
- [Shared stadium clock and live screens](../Game/Assets/_Game/Sports/Football/FootballStadiumCelebration.cs)
- [Live director, scoreboard and goal-replay screen layout](../Game/Assets/_Game/Sports/Football/FootballStadiumBroadcast.cs)
- [Procedural fountains and sky shells](../Game/Assets/_Game/Sports/Football/FootballStadiumFireworks.cs)
- [Modular flame and laser fixtures](../Game/Assets/_Game/Sports/Football/FootballStadiumEffectsRig.cs)

The motion builder adds missing celebration takes without replacing existing authored takes. Rendering copies contain no gameplay actors, colliders, network objects or animation controllers. The live match stays authoritative. Camera ownership, hidden renderers and blocked local input are released on dismissal, a new match, leaving football, or disabling the presentation.

## Verification

Build the complete stadium review player with Unity's `FootballStadiumShowBuild.Windows` entry point, then run:

```powershell
Tools/Build/Test-FootballCelebration.ps1 -CaptureFrames -PlayerPath <review-player>
Tools/Build/Test-FootballStadiumShow.ps1
Tools/Build/Test-FootballMatch.ps1 -PlayerPath <review-player>
Tools/Build/Test-FootballReplay.ps1 -PlayerPath <review-player>
Tools/Build/Build.ps1 -Target AndroidSubmission
Tools/Build/Check-TaskReady.ps1 -RequireApk
```

The review records the complete ceremony and loop, unchanged statistics, bone lengths/reach, identity, five-player winners, both teams in a draw, an opposing or departed MVP, skip, dismissal and interruption. It captures 16:9, wide-phone and 4:3 compositions. Paths default relative to the script's repository, not the shell's working directory. Current captures, traces and reports belong under ignored `Builds/FootballCelebrationQA/`; the delivered player belongs under ignored `Builds/WindowsFinal/`.

The match and replay room fixtures wait for replicated arrival before starting. Room match tests require graphics for the shared entry's teleport materials; `-RoomOnly` reruns that portion. Replay fixtures explicitly confirm the solo team for their golden-goal and cancellation rounds.

The stadium show fixture runs a host, another player and a spectator together. It compares replicated finish times, revisions and MVP identity, checks both stage boundaries, captures the screen from the spectator stand, verifies a locally dismissed result leaves the world show running, and checks cleanup on a new round. Its default candidate and evidence live under `Builds/FootballStadiumShowQA/`.

For the live broadcast, build `FootballStadiumShowBuild.WindowsBroadcast`, then run `Tools/Build/Test-FootballBroadcast.ps1 -CaptureFrames`. Its candidate, screen captures and camera trace live under ignored `Builds/FootballBroadcastQA/`. The fixture checks idle/kickoff/live layouts, all three angles, boundary and lob framing, a full angle cycle along the near touchline, first-person visibility, a physical goal, the full-screen replay and return, missing history, cancellation, score reset and resource cleanup. Run the stadium show, replay and teardown launchers with this candidate's `-PlayerPath` as well. The three-member stadium fixture checks live footage, the goal wipe and replay on host, player and spectator before exercising the ceremony.

Windows rendering checks do not establish physical Android quality or frame rate. Device and WAN validation must be reported separately.

For daylight effects, build `FootballStadiumShowBuild.WindowsEffects` and run [Test-FootballStadiumEffects.ps1](../Tools/Build/Test-FootballStadiumEffects.ps1) with `-CaptureFrames`. Its candidate, timed preview frames and wide/ground/screen views live under ignored `Builds/FootballStadiumEffectsQA/`. It checks continuous emission through the team segment, the 15-second cutoff, deterministic catch-up, bounded geometry, permanent fixtures outside the pitch, normal-match silence and disable/re-enable cleanup. Run the celebration, stadium room and teardown reviews against the same candidate before delivering it.

Stadium destruction only releases objects owned by the broadcast. It does not
reactivate the original arena screen, which may already be undergoing destruction
when an island unloads or the application exits. Build `StadiumTeardownBuild.BuildReview`
and run [Test-StadiumTeardown.ps1](../Tools/Build/Test-StadiumTeardown.ps1) to exercise
live-feed disable/re-enable, a new round, football-to-basketball switching, arena
unloading/reloading and quitting during an active broadcast. The launcher checks
the full log after process exit so teardown errors cannot escape the result check.
