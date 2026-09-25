# Playful idle verification — 23 September 2026

The authored eight-second idle is integrated into the shared athlete controller. It replaces the empty Idle state in the character menu and offline/network athletes. The supplied sprint remains unchanged.

## Authored asset and export

- `RainbowSprinterAnimation.blend` contains editable controls, `Idle_Playful`, a stepped `Idle_Playful_Blocking` study, the original meshes, and the preserved `Running` action.
- Setup was rerun against the authored source; its SHA-256 did not change. Running action curve data matches the original master exactly (SHA-256 `7ad39c8f20c66fce84a95ad738c6c96e989781163ba142110d2c582067b4314e`).
- All 481 Blender frames passed the contact and loop audit. Endpoint position and rotation differences are zero; the largest endpoint velocity difference is 0.000190 m/s. Maximum planted toe drift is 0.0000144 m (0.0144 mm). Hip travel across the loop is 0.024 m.
- The FBX contains one take and the original 34 bones. It is 1,219,052 bytes and carries no mesh, material, or control rig. Its Unity importer preserves the top-level rig node so its paths bind to the model correctly.
- Unity checks passed for all 350 curve bindings, the eight-second loop, both prefabs, unchanged rig root transforms, actual body movement and matching endpoints. Sampled Unity/Blender bone positions differ by at most 0.0000012 m. Both LOD meshes remain within the ground-contact tolerance (lowest sampled vertex: −0.001180 m).

## Visual and gameplay checks

Multi-view renders were inspected for the near and distant LODs, including the heel-lift pose. An eight-second 30 fps frame sequence, eight-pose contact sheet, and three-loop normal/half-speed videos are available under `Builds/IdleQA`. The controls maintain soft knees and lowered arms; head, wrist and weight-shift movement stay restrained at gameplay scale. The distant LOD retains its existing simplified silhouette.

The Windows development player passed checks for moving head/hands in the menu, the authored clip in football/basketball/golf, screenshots of both LODs, first-person hiding and third-person restoration. Stopping at four sprint phases, interrupting the heel lift, four rapid restarts, and 24 seconds of idle playback passed. There was no horizontal root drift.

Two local network players connected, entered exploration, moved, returned to the waiting room, and verified animated idle on both avatars from both processes before and after exploration. No network schema or movement-speed changes were introduced. Logs contain completion markers and no failed assertions for the paired run.

Evidence: `Builds/IdleQA/blender-audit.json`, `blender-reference.json`, `preservation.json`, `unity-import-audit.txt`, `gameplay.txt`, `paired-host.txt`, and `paired-client.txt`. `Tools/Build/Test-Idle.ps1` reproduces the runtime checks into a timestamped directory.

The complete reusable runner passed in `Builds/IdleQA/Run-20260923-223831`, including seven camera captures, gameplay and network completion markers, zero failed assertions, and zero process exit codes for all three players.

## Builds and limits

Unity 6000.3.20f1 Windows and Android development builds succeeded. The Android APK is 76,002,978 bytes (76.00 MB), below the 100 MB target. The original model and run FBX files were not regenerated. Source Blend files and authoring controls are not shipped.

No physical Android device was connected (`adb devices` returned an empty device list). On-device touch, sustained frame rate and thermals remain unverified. This pass does not add terrain foot IK, separate stopping animations, facial animation, or additional idle variations; idle/run blending uses the existing locomotion clips.

During automation, launching a graphical player with a hidden interactive window caused a native shutdown access violation after all assertions completed. The installed Unity symbols resolve the fault to `ExternalGPUProfiler::GetGameViewWindowHandle`; it reproduced with both D3D11 and D3D12. The test runner therefore uses graphics-enabled batch mode, which exits cleanly and retains camera captures. Headless network players were unaffected. This changes only the test invocation, not the game's graphics settings or quit behavior.

See [Idle authoring](IDLE-AUTHORING.md) for editing and export instructions.
