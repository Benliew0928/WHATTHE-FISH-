# Rainbow Sprinter animation workflow

## Source and performance

`ArtSource/Shared/Characters/RainbowSprinterAnimation.blend` is the editable animation source. The original `RainbowSprinter.blend`, Meshy source, and supplied `Running` action remain separate and unchanged. Do not regenerate the animation source with the Meshy export script.

`Idle_Playful` is an eight-second action at 60 fps, frames 1–481. Frame 481 closes the loop; previews play frames 1–480. It lowers the arms, softens the knees, shifts weight, takes two subtle breaths, glances sideways, and briefly raises the left heel with the toe planted. `Idle_Playful_Blocking` preserves stepped key poses for studying the performance.

## Edit in Blender

1. Open the animation source, select **RainbowAnimationControls**, and enter Pose Mode. The original game rig is hidden in the viewport but follows these controls.
2. Use **Pose controls** for pelvis, spine, chest, head, shoulders, arms, elbows, and wrists. Use **Foot placement and knee direction** for feet and knee poles. Keep **Mechanism (do not key)** hidden.
3. Select **Idle_Playful** in the Action Editor. Turn on auto-key only when intentionally changing the action. Pose at 1, 121, 241, 301, and 361; frame 481 must match frame 1. Existing intermediate keys are editable, not live procedural drivers.
4. Move `CTRL_Heel_L/R` to place each foot. Rotate `CTRL_ToePivot_L/R` to lift a heel around its planted toe. `CTRL_Foot_L/R` controls the ankle target and foot orientation; knee poles set the bend direction. Maintain slight knee flexion to avoid a straight-leg IK snap.
5. Refine body balance first, then breathing and arm follow-through, then head and wrist details. Keep hands clear of shorts in front and side views. Inspect the actual shoes, not only bone markers.
6. In the Graph Editor, use the existing Cycles modifiers and inspect the first/last tangent directions. Preserve identical endpoint values and smooth velocity. Save the file before previewing or exporting.

The control rig is a separate armature, so no control bones enter the game skeleton. Constraints on `RainbowSprinterRig` are baked only in a temporary in-memory export copy. Do not change its rest pose, names, parenting, object scale, or bind transforms. The export audit rejects changed rest transforms.

To study the supplied run, open the original character source and activate its `Running` action. Selecting it on the constrained delivery rig in the animation source would still be overridden by the idle controls.

## Repeatable commands

Run from `C:\UMPSA` in PowerShell with Blender 5.1.2:

```powershell
$blender = 'C:/Program Files/Blender Foundation/Blender 5.1/blender.exe'
$tool = 'Tools/Blender/animate_rainbow_sprinter.py'

# First-time setup only; an existing authored file is preserved byte-for-byte.
& $blender --background --factory-startup --python-exit-code 1 --python $tool -- setup

# Read saved keys and verify all 481 frames, ground contact and loop continuity.
& $blender --background --factory-startup --python-exit-code 1 --python $tool -- audit

# Multi-view stills; choose a new label to retain each review pass.
& $blender --background --factory-startup --threads 6 --python-exit-code 1 --python $tool -- preview --label review-02

# A 30 fps, eight-second rendered cycle, then three-loop videos at both speeds.
& $blender --background --factory-startup --threads 6 --python-exit-code 1 --python $tool -- preview --video --engine eevee --label final-loop
python Tools/Blender/package_idle_preview.py --label final-loop

# Export only the selected saved idle, with 60 fps baked transforms.
& $blender --background --factory-startup --python-exit-code 1 --python $tool -- export
& Tools/Build/Build.ps1 -Target Scene
```

The Python packaging command requires Pillow and imageio-ffmpeg, already available in the workspace Python. Output goes under `Builds/IdleQA`: a contact sheet, normal-speed and half-speed three-loop MP4s, and JSON audit/reference data. Preview supports `--lod LOD1`, `--frames 1,181,301`, `--size 512`, and `--action Idle_Playful_Blocking`.

Export writes `Game/Assets/_Game/Art/RainbowSprinterIdle.fbx`. It contains the original 34-bone skeleton and one baked take, without meshes, materials, controls or other actions. It never saves baked data over the editable source. Setup has no overwrite switch; use a different source filename for experimental regeneration.

## Unity and acceptance

The scene builder imports the animation as Generic with preserved hierarchy, no animation compression, and Loop Time enabled. Loop Pose correction is disabled because the authored endpoints already match. Both athlete prefabs receive it as their default Idle. Root motion stays off. Existing `Speed` and `RunPlayback` parameters drive the unchanged supplied run.

Idle → Run uses Speed > 0.2 with a fixed 0.15 s blend; Run → Idle uses Speed < 0.1 with a fixed 0.25 s blend. Destination-state transitions can interrupt either blend. No network message or gameplay speed changes are needed.

Run `IdleImportAudit.Run` through Unity batch mode after export and scene rebuilding. It checks clip bindings, eight-second duration, actual motion, bone positions against Blender, endpoint continuity, ground penetration for both LODs, unchanged root scale, and both prefab controllers. The Blender reference JSON is generated by `audit` or `export`.

Development players accept `-batchmode -probe -idleAudit -report <absolute-path> -exitAfter 65` to exercise preview motion, all three sports, both LODs, first-person hiding, sprint stop phases, the heel-lift interruption, rapid restarts and three idle cycles. Keep graphics enabled for these camera captures. Use `-networkIdleAudit` alongside the existing local host/client flags to verify each network athlete moves while idle before and after exploration. All probes are opt-in and excluded from release builds.

After building the Windows player, `& Tools/Build/Test-Idle.ps1` launches the complete gameplay and two-player check, validates completion markers and failures, and saves evidence in a timestamped directory. Use `-Port 7795` if its default local port is occupied.

Review the videos at normal and half speed as well as game screenshots. Passing numeric checks alone does not establish animation quality. Acceptance requires stable shoe contacts, no visible seam, no joint snapping or hand intersections, and responsive starts/stops without a bind-pose flash. Physical Android behavior must be checked on a connected phone separately from an APK build.

## Later animations

Duplicate an action in the Action Editor for experiments, preserving Idle_Playful. Reuse the controls and the same sequence: performance brief → stepped poses → timing → smooth curves → contact/deformation checks → bake → Unity review. Add each approved action deliberately to the exporter and controller; do not export every action automatically. Walking, sport gestures, facial animation and additional idle variations are outside this first pass.
