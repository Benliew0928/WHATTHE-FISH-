# Rainbow Sprinter: responsive turning while moving

Current gameplay prioritizes stick response: every active input moves in its requested direction on the next simulation tick, even while the body is turning. Facing smooths independently. Speed eases in over 0.10 seconds and out within approximately 0.14 seconds; reversals retain speed. The original 4 m/s movement and 7 m/s sprint limits remain unchanged. There is no committed turn, queued direction or animation completion gate.

The earlier turn-before-run implementation looked good but could trap alternating input in repeated stationary pivots. It is superseded by the behavior below.

## Performance and source

`ArtSource/Shared/Characters/RainbowSprinterTurns.blend` is a protected copy of the idle authoring source. It contains four editable actions on `RainbowAnimationControls`: `Turn_Left_90`, `Turn_Right_90` (frames 1–22, 0.35 seconds), and `Turn_Left_180`, `Turn_Right_180` (frames 1–34, 0.55 seconds), all at 60 fps. Matching actions ending in `_Facing` belong to the `TurnFacing` empty. Select both corresponding actions when editing a turn.

The character first settles, leads with the head and chest, steps the outside foot, carries the other foot around, and finishes with a small outside-foot adjustment. Soft knees and pelvis movement keep the short legs within reach. Arm motion counterbalances the turn. This is a non-looping transition, so its endpoint does not return to its initial world orientation.

The `TurnFacing` empty supplies world yaw in the Blender preview. Export removes this yaw from the evaluated skeleton: Unity's motor owns player facing and horizontal travel. Root motion remains disabled. The original character source, idle source, supplied Running action, skeleton hierarchy and bind transforms are preserved.

## Repeatable commands

Run from the repository root in PowerShell:

```powershell
$blender = 'C:/Program Files/Blender Foundation/Blender 5.1/blender.exe'
$tool = 'Tools/Blender/turn_rainbow_sprinter.py'

# Only creates a source if it does not exist; never overwrites saved authoring.
& $blender -b --factory-startup --python-exit-code 1 --python $tool -- setup

# Read and validate all four saved actions, then export four skeleton-only FBXs.
& $blender -b --factory-startup --python-exit-code 1 --python $tool -- audit
& $blender -b --factory-startup --python-exit-code 1 --python $tool -- export

# Each preview reads saved curves; no animation regeneration.
& $blender -b --factory-startup -t 4 --python-exit-code 1 --python $tool -- preview --action Turn_Left_180 --video
& $blender -b --factory-startup -t 4 --python-exit-code 1 --python $tool -- preview --action Turn_Right_180 --video
& $blender -b --factory-startup -t 4 --python-exit-code 1 --python $tool -- preview --action Turn_Right_90
python Tools/Blender/package_turn_preview.py

& Tools/Build/Build.ps1 -Target Scene
& Tools/Build/Build.ps1 -Target Windows
& Tools/Build/Test-Turn.ps1 -Video
& Tools/Build/Test-Idle.ps1
& Tools/Build/Build.ps1 -Target Android
```

Run Unity's `TurnAudit.Run` and `IdleImportAudit.Run` through `-executeMethod` in batch mode after rebuilding the scene. Do not run two Unity editors against this project simultaneously. To package captured gameplay, run `python Tools/Blender/package_turn_preview.py --gameplay Builds/TurnQA/Run-<timestamp>`. Blender stills, contact sheets, repeated normal/half-speed previews, numeric audits and game captures go under `Builds/TurnQA`.

Export bakes the saved evaluated action onto the existing 34-bone skeleton at 60 fps in memory, exports only that skeleton and its one take, then discards the baked copy. It does not save over the authoring file or regenerate keyframes. Exported filenames are `RainbowSprinterTurn_<Left|Right>_<90|180>.fbx` in the existing Unity art directory.

## Editing and timing contract

Use the idle guide for control selection and posing. The turn actions contain editable keys at every frame; simplify curves carefully if desired. Refine pelvis balance before head/arms and inspect all views. Preserve foot contact and the existing skeleton. Feet have 5 mm of authoring clearance to accommodate sole deformation from the supplied skin weights.

The authoring audit uses a turn-only two-bone leg correction to verify full-body preview placements at intermediate angles such as 120 and 150 degrees. Gameplay does not run this planted-foot correction: the running legs continue moving. Its footprint schedule matches Python `step()` and C# `TurnFootPlacement.FootStep()`: outside-foot steps at 12–35% and 70–96%; inside-foot step at 35–70%. Root yaw uses smoothstep from 15–82% of the performance. If changing foot placements or timing, update both functions and the facing curve, then rerun export and contact checks. Upper-body curve edits need only export/rebuild. The export audit deliberately rejects saved foot curves that violate this shared contact contract.

## Runtime behavior

`LocomotionMotor` separates translation from facing. Translation uses the latest camera-relative input direction every tick, with only scalar speed easing. It never averages opposing velocity vectors, which would erase rapid up/down input. Facing uses a damped angle toward the latest direction; opposite input immediately cancels obsolete rotational momentum. Releasing input cancels facing immediately and briefly eases speed to zero. Stick noise below 0.15 remains ignored.

Turning is now a presentation label based on angular error (enter above 60 degrees, leave below 8), not a movement phase that waits for a duration. Its pose progress follows actual facing, not elapsed clip time. Repeated or interrupted input does not restart any movement delay.

The base Animator remains in Idle/Run, driven by actual speed. The saved turn actions contribute only spine, neck and head motion through a masked `Turn expression` layer at up to 0.65 weight. Running hips, legs and arm swing remain active. No base transition enters the full-body `Turn` state during gameplay; that state remains available to the authoring audit. The runtime no longer applies planted turn-foot IK. Existing idle/run blends and root-motion settings are preserved. Camera yaw remains independent of body yaw.

The atomic `LocomotionSnapshot` still carries the presentation phase, speed and facing information. Remote turn expression follows interpolated body yaw. Input RPCs and server authority are unchanged: network transport still adds its ordinary latency, but animation adds no movement lock. The subsequent football tackle update advances network protocol to **6**; see `TACKLE-AUTHORING.md`. Older clients cannot join these rooms. Every participant needs a matching build.

## Review limits

Inspect real-time and half-speed previews plus actual start/stop gameplay, not only numeric tests. The standalone full-body authoring audit handles level surfaces; it is not a general terrain/stair IK system. Internet latency/loss and physical Android performance require separate testing. Local two-player checks establish replication behavior on this machine, not mobile or WAN performance.
