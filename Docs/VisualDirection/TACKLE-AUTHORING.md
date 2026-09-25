# Rainbow Sprinter: football ground tackle

The tackle drops the character onto the turf, with a reclining torso, one leg reaching forward, the other folded, and a supporting arm behind. It launches immediately along body facing. Gameplay plays the authored slide at 2x speed: 4.62 metres of unobstructed travel over 0.275 seconds and 0.425 seconds including recovery. Movement input resumes during that recovery, without waiting for the clip to end. The existing responsive forward/backward locomotion remains unchanged.

## Controls and gameplay

Football exploration exposes a **Tackle** touch button above Camera; desktop also supports **Space**. Pressing starts once, holding does not repeat, and the button shows its cooldown. A tackle requires ground contact and has a 1.25-second cooldown from launch. Basketball and golf expose no tackle button and reject tackle requests.

An opponent hit by the slide receives a 0.45-second stumble and up to 0.72 metres of knockback over 0.24 seconds. Walls constrain both movements. Contacts sweep actual travelled distance, reject hits through world geometry, and affect each opponent once per slide. A short hit immunity prevents repeated stun from one contact cluster. There are no teams in this prototype, so other athletes are eligible opponents; this change does not add AI opponents, ball possession, health or ragdolls.

`FootballTackle` owns action timing, cooldown and contact resolution. `Athlete` combines action displacement with the capsule motor and resumes normal movement during recovery. The Animator's full-body **Football action** layer blends in quickly and out smoothly, above the existing idle/run and turn-expression layers. During the slide, a measured visual ground-clearance correction removes the capsule's small standing gap without changing collision or camera height. Root motion remains disabled.

Network action requests are reliable and restricted to the athlete's owner. The server validates and simulates them, including opponent contact. A separate `FootballSnapshot` carries action, start time, cooldown and sequence so remote animation follows server time. NGO protocol is **6**; all participants require matching builds. Existing movement input and locomotion snapshots retain their behavior. Local networking tests do not establish WAN latency performance.

## Editable source

`ArtSource/Shared/Characters/RainbowSprinterFootball.blend` is a protected copy of the idle source. Select `RainbowAnimationControls` and either saved action:

| Action | Frames at 60 fps | Performance |
|---|---|---|
| `Slide_Tackle` | 1–52 | Drop over 0.13 s; ground slide through 0.43 s; recover through 0.85 s. |
| `Tackle_Hit` | 1–28 | Quick backward recoil, low balance adjustment and recovery over 0.45 s. |

These are source timings. `FootballTackle.SlidePlayback = 2` compresses the slide and its travel to half duration in gameplay through normalized `FootballTime`; the saved source and FBX remain editable at their original timing. Slide speed scales with playback to retain the full tackling distance. Opponent reaction timing and the 1.25-second cooldown are unchanged. Standalone Blender previews show source timing; gameplay captures show the faster timing.

Pelvis, chest, head, arms and IK feet remain editable. The original 34-bone skeleton, bind transforms, mesh, materials and supplied Running action are retained. The slide lowers the source pelvis by 0.29 m and leans it back 58 degrees; restrained counter-rotation keeps the face readable. These are authoring values in the source model's scale, not Unity travel distances.

Polish pelvis/foot contact before head and arm follow-through. Review front, side and three-quarter views, then the gameplay camera. Avoid sinking the hand or shoe into the turf. The saved actions contain keys at each frame; curve simplification should be followed by a new contact audit. If duration changes, update Python `ACTIONS`, C# `FootballTackle` durations, and the tests together.

## Repeatable workflow

Run from `C:/UMPSA` in PowerShell:

```powershell
$blender = 'C:/Program Files/Blender Foundation/Blender 5.1/blender.exe'
$tool = 'Tools/Blender/tackle_rainbow_sprinter.py'

# Creates only when absent; preserves an existing authored source.
& $blender -b --factory-startup --python-exit-code 1 --python $tool -- setup
& $blender -b --factory-startup --python-exit-code 1 --python $tool -- audit
& $blender -b --factory-startup --python-exit-code 1 --python $tool -- export

& $blender -b --factory-startup -t 4 --python-exit-code 1 --python $tool -- preview --action Slide_Tackle --video
& $blender -b --factory-startup -t 4 --python-exit-code 1 --python $tool -- preview --action Tackle_Hit --video
python Tools/Blender/package_tackle_preview.py

& Tools/Build/Build.ps1 -Target Scene
& Tools/Build/Build.ps1 -Target Windows
& Tools/Build/Test-Tackle.ps1 -Video
& Tools/Build/Test-Turn.ps1
& Tools/Build/Test-Idle.ps1
& Tools/Build/Build.ps1 -Target Android
```

Export reads saved curves, bakes the evaluated original skeleton at 60 fps in memory, exports one action per FBX, and leaves the source intact. Output files are `RainbowSprinterSlide_Tackle.fbx` and `RainbowSprinterTackle_Hit.fbx` in `Game/Assets/_Game/Art`. Controls, meshes, materials and unrelated actions are excluded. `ProjectBuilder.Setup` imports non-looping Generic clips and assigns the layer and components to both athlete prefabs.

Run Unity's `TackleAudit.Run` through batch `-executeMethod` for the import, prefab and 30/60/120-fps travel checks. Do not launch two Unity editors against this project at once. Package captured game frames with `python Tools/Blender/package_tackle_preview.py --gameplay Builds/TackleQA/Run-<timestamp>`.

## Review evidence

Evidence lives under `Builds/TackleQA`: `tackle-contact-sheet.jpg`, repeated normal/half-speed action videos, `football-tackle-gameplay.mp4`, per-action audit JSON, `export.log`, and timestamped gameplay/host/client reports. The source contact audit found less than 1 mm of sole penetration from the existing skin weights and less than 0.05 mm ankle-target error, with all 34 bone names, parents and bind matrices preserved.

The automated gameplay checks cover the actual button input path, immediate facing-direction travel, both LODs, opponent knockback, cooldown/no queued repeats, control during recovery, full-distance misses, wall obstruction, airborne rejection, and sport gating. Local host/client checks cover both players initiating and receiving tackles. Idle and rapid direction-change regression suites also pass.

Verified on 2026-09-24: final tackle gameplay and two-player reports are in `Builds/TackleQA/Run-20260924-225514`; video frames are from `Run-20260924-224850` with identical animation/gameplay tuning. Regression reports are `Builds/TurnQA/Run-20260924-225049` and `Builds/IdleQA/Run-20260924-225049`. `TackleAudit.Run.log`, `TurnAudit.Run.log`, `build-Windows.log` and `build-Android.log` record successful audits/builds. Re-running setup preserved the existing football source byte-for-byte.

The subsequent 2x slide retiming passes gameplay and local two-player checks in `Builds/TackleQA/Run-20260924-233410`. The gameplay preview now uses those faster captures. The import audit accounts for playback speed when comparing the saved clip with gameplay duration; travel remains 4.62 m at 30, 60 and 120 fps.

Windows and Android builds are produced by the commands above. Physical Android performance, touch feel, slopes/stairs and internet latency/loss need separate device/session testing. No Android device was attached during this implementation.
