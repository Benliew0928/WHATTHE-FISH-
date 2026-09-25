# Turnaround verification — 2026-09-24

Historical evidence for the original movement-locking implementation. Superseded by [Responsive movement verification](RESPONSIVE-MOVEMENT.md); stationary pivots are no longer gameplay acceptance criteria.

## Implemented behavior

Forward movement accelerates smoothly. Backward input from standing plays a turn before travel begins. Reversing a run brakes, steps around, then accelerates in the new direction. Four authored left/right 90/180-degree actions blend to intermediate angles; the motor controls yaw and displacement, with root motion disabled. The shared controller and prefabs cover preview, offline and network athletes.

## Evidence

| Check | Result |
|---|---|
| Blender export | Four actions, one take per FBX, original 34-bone hierarchy/rest transforms retained; skeleton only |
| Blender contacts | Largest ankle target error 0.034 mm; lowest evaluated mesh vertex approximately -4.26 mm relative to authoring floor |
| Saved source protection | Calling setup on the existing `RainbowSprinterTurns.blend` left its SHA-256 unchanged |
| Motor | 36 standing/moving reversal scenarios at 30/60/120 fps and signed 120/150/180-degree directions passed; zero horizontal pivot travel |
| Input changes | Early cancel, committed release, queued reversal, sprint release, stick dead zone, airborne guard and gradual forward acceleration passed |
| Unity import and foot correction | Eight signed angles sampled through the turn; maximum target error 0.251 mm; lowest sampled vertex -4.52 mm; both LOD meshes checked |
| Network snapshot | Stable phase timestamp; unchanged idle does not dirty/resend the snapshot |
| Built gameplay | 24 direction cases across football, basketball, golf and both LODs; turn before run, stationary pivot, foot targets, idle return and first-person hiding passed |
| Local two-player session | Host and client each observed three turns per actor; no running speed during the pivots; clean exits |
| Idle regression | Preview motion, all sports/LODs, first-person hiding, stops at four run phases, heel-lift interruption, rapid restarts, three idle loops, no root drift and two-player idle passed |
| Windows / Android | Development player and APK built successfully; physical Android testing not performed |

`Builds/TurnQA/editor-audit.txt` contains motor/import measurements. Blender reports are `Turn_<side>_<angle>-audit.json`. The final gameplay/network run is `Builds/TurnQA/Run-20260924-124402`; the recorded gameplay pass is `Run-20260924-123607`. The idle regression pass is `Builds/IdleQA/Run-20260924-124223`.

The idle stop test now allows 0.50 seconds for the new 0.14-second brake, existing 0.25-second idle blend and frame scheduling. Its former 0.40-second deadline failed despite the intended braking behavior; the updated timing passes all four stop phases. Rapid restart checks retain their original timings.

## Visual artifacts

- `Builds/TurnQA/forward-backward-gameplay.mp4`: captured basketball gameplay with standing and running reversals, both LODs.
- `Builds/TurnQA/turns-three-passes.mp4` and `turns-three-passes-half-speed.mp4`: paired left/right authored previews, repeated with endpoint holds for review.
- `Builds/TurnQA/turn-contact-sheet.jpg`: authored 90/180-degree poses.
- `Builds/TurnQA/gameplay-transition-sheet.jpg`: run → brake → turn → run sampled from the real player.

Rendered poses and gameplay transition frames were inspected for collapsed shoulders, hand/body intersections, knee direction and foot placement. The final contact pass moves the pelvis over the stepping feet; the initial blocking pass had overextended the short legs and was replaced. Saved experimental copies remain under the ignored QA directory. Numeric thresholds allow small sole deformation from the existing skin weights; they do not imply mathematically zero penetration.

## Limits

No phone was attached in `adb devices`. APK build success is not a physical-device performance or touch-control result. This pass uses a local two-player connection, not WAN latency/loss testing. The leg correction targets level sports surfaces; uneven-terrain/stair adaptation is outside this pass. Existing sprint animation, gameplay speed limits and input RPCs remain unchanged. Replicated locomotion state requires matching **protocol 4** builds for every participant.

See [Turn authoring](TURN-AUTHORING.md) for the editable source, repeatable commands and the shared Blender/Unity foot timing contract.
