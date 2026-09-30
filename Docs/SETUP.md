# Setup and build

## Tool versions and local settings

Use Unity **6000.3.20f1** and Blender **5.1.2**. Clone the repository anywhere; scripts resolve the repository root from their own location. Run documentation command examples from the repository root. Relative `-OutputPath` and `-Manifest` arguments in the review/archive scripts also resolve from that root.

Before running Blender examples, set `BLENDER_PATH` in your terminal to your installed Blender executable. If Blender is on `PATH`:

```powershell
$env:BLENDER_PATH = (Get-Command blender -ErrorAction Stop).Source
```

Otherwise assign your executable's actual path to `$env:BLENDER_PATH` locally. This setting is not stored in the repository. The examples use `& $env:BLENDER_PATH` so each teammate can use their own installation.

Unity build scripts default to Unity Hub's editor under `$env:ProgramFiles`. For another installation, pass `-UnityEditorPath` to `Build.ps1` or `-UnityEditorRoot` (the version folder containing `Editor` and `modules.json`) to `Install-Android.ps1` and `Repair-CMake.ps1`.

Project: the `Game` folder inside your clone.

Install Git LFS before cloning, or run `git lfs pull` inside an existing clone. Use Unity Hub → Add project from disk → select the clone's `Game` folder. Let Unity import assets and packages on first open; it will regenerate the ignored `Library` folder. The manifest and lock pin URP, Multiplayer Services and NGO. These packages include the Relay/Lobby integrations and Transport dependencies. Open `Assets/_Game/Scenes/Bootstrap.unity` and press Play to explore offline. The exported models, textures, materials, scene and `.meta` files are part of the repository; Blender is only needed to edit or regenerate art.

To build on Windows from a clone, run `Tools/Build/Build.ps1 -Target Windows` at the repository root. The script finds the clone automatically. If Unity is installed elsewhere, set `$env:UNITY_EDITOR_PATH` locally and pass `-UnityEditorPath $env:UNITY_EDITOR_PATH`. Install the matching Unity Editor and Windows Build Support; Android builds also require Android Build Support.

## Android toolchain

Android Build Support and the bundled SDK, NDK r27c, CMake and OpenJDK 17 are installed under this editor. `Tools/Build/Install-Android.ps1` caches official downloads in ignored `Tools/Downloads` and installs the versions declared in the editor's `modules.json`; it requires administrator rights for Program Files. Superseded local installer downloads were moved to the 29 September 2026 `Legacy/` housekeeping batch. Installed tools are unchanged; the installer downloads missing cache files when run again.

Unity Preferences → External Tools should use the installed Unity SDK/NDK/JDK. Build Settings/Build Profiles → Android. The project uses IL2CPP, ARM64, minimum API 26, landscape, package ID `com.umpsa.whatthefish`. The identifier is a development placeholder; choose your publishing ID before release.

The current APK is `Builds/Android/WhatTheFish-release.apk` (rebuild with `Tools/Build/Build.ps1 -Target AndroidRelease`). To create an optional development APK, use **WHATTHE FISH? → Build Android development APK**, or run `Tools/Build/Build.ps1 -Target Android`. Output: `Builds/Android/WhatTheFish.apk`. Development APKs use a debug signing key. Do not publish this build. Signing keys are excluded from Git.

Connect a phone with USB debugging enabled and accept its debugging prompt. From the repository root, run `adb devices`, then `adb install -r .\Builds\Android\WhatTheFish-release.apk`. Launch WHATTHE FISH?. Airplane-mode offline testing and sustained FPS measurements must be performed on the actual target phone.

## Unity cloud project — required for internet rooms

1. Sign into Unity Hub/Editor with your own account. In Project Settings → Services, create or link a **development** cloud project in your intended organisation.
2. In that project's Unity Gaming Services dashboard, enable Authentication with anonymous sign-in and enable Multiplayer Services/Lobby/Relay. Use the same environment for every build.
3. Rebuild both players after linking. The cloud project ID is build configuration, not a secret. Never commit service account credentials, signing keys or access tokens.
4. On one device choose Football, Basketball or Golf → Create internet room. Share its session code. On another internet connection enter the code and join. All players must mark ready before the host starts.
5. A room supports one host plus nine guests. The SDK maintains the session/lobby connection; Relay carries NGO traffic. Host departure ends this prototype's room. Host migration is intentionally disabled at the application layer.

The project is linked to Unity cloud project `633e314c-46d1-4b48-bbbc-2dc5f6fb653c`, for a general audience. Its Authentication/Lobby/Relay services have passed real create/join tests. The current runtime uses its default `production` environment while separate development environment setup is pending dashboard access. Offline exploration stays usable independently of cloud services. No paid plan or third-party API purchase is configured by this project.

`Tools/Build/Test-CloudRooms.ps1` exercises real services with separate anonymous test profiles. It creates temporary rooms, tests ten-player capacity and two-room isolation, and writes evidence under `Builds/CloudQA-*`. Test processes exit automatically. This uses service quota and should only be run when needed. Local transport-only testing is available through `Test-LocalRooms.ps1`.

## Rebuild art and scene

Close Unity while regenerating art, or wait for FBX export to finish before triggering asset import.

```powershell
& $env:BLENDER_PATH --background --factory-startup --python .\Tools\Blender\build_football_stadium.py
```

This rebuilds the football stadium only. **Save hand-edited variants under a new name before regenerating.** The venue assets use editable Blender files. Export axes are -Z forward / Y up, metres, without leaf bones.

Golf and fishing now share the G2/L2 library. See [Refined island authoring](../ArtSource/RefinedIslands/README.md) for the editable masters and deterministic export workflow.

To regenerate the current character from the preserved Meshy ZIP, run `python .\Tools\Blender\prepare_rainbow_sprinter.py`, then run Blender with `--background --factory-startup --python .\Tools\Blender\export_rainbow_sprinter.py` from the repository root. This produces the editable master, a static skinned model FBX with two LODs, a separate FBX containing only the supplied run take, and the texture maps used by Unity. The two exports share the same rig. Character appearance controls are paused for this one-piece mesh.

The authored idle is maintained separately in `ArtSource/Shared/Characters/RainbowSprinterAnimation.blend`. Run `Tools/Blender/animate_rainbow_sprinter.py -- export` through Blender to export saved animation edits before rebuilding the scene. Initial setup, controls, preview videos and validation are documented in [Idle authoring](VisualDirection/IDLE-AUTHORING.md). The Meshy regeneration command does not update this animation source.

In Unity choose **WHATTHE FISH? → Rebuild game scene** to reconstruct the generated Bootstrap scene and prefabs. This also replaces the generated Animator Controller. Keep manual scene variations separately. Then build Windows or Android from the WHATTHE FISH? menu.

## Basketball arena

Generate only the basketball art with:

```powershell
& $env:BLENDER_PATH --background --factory-startup --python .\Tools\Blender\build_rally_arena.py
```

Import the regenerated modules using the Unity menu **WHATTHE FISH?/Basketball/Build Rally art library**, then run `Tools/Build/Build.ps1 -Target Scene`, followed by Windows and Android builds. Keep hand-edited Blender variants separately before regeneration. Scene rebuilding configures all four environments, the four logo references, spawn points, cameras and lighting.

Choose Basketball → Stadium customisation to change the arena name, accent palette and center logo. In a room only the host has this control. Return to the waiting room to edit settings, then start again. Logo selection persists locally and appears for joining guests. No image upload is included.

Run `Tools/Build/Test-BasketballRooms.ps1 -Transport Local` for local checks or `-Transport Cloud` for real Relay checks. The latter creates temporary ten-player basketball and two-player football rooms using the linked Unity cloud project. Processes and evidence are isolated per run. Run `python Tools/Build/check_basketball_evidence.py <evidence-folder>` to compare actual movement and replicated positions. Use the same new build for every participant.

## Golf and fishing islands

The selected G2 Limestone Cove Links and L2 Limestone Garden Lagoon use the shared detailed asset kit. Their current sources, materials, manifests and rebuild instructions are in [Refined island authoring](../ArtSource/RefinedIslands/README.md).

```powershell
& $env:BLENDER_PATH --background --factory-startup --python-exit-code 1 --python Tools/Blender/build_refined_islands.py
Tools/Build/Build.ps1 -Target Windows
```

For layout iteration, append `-- --reuse-kit` to Blender. Unity method `RefinedIslandBuildEntry.BuildWindows` rebuilds just these two environments into the saved scene and Windows player, preserving the stadium scene objects. Windows is the current performance target; Android tuning is deferred.

All four environments use the shared controller: WASD moves, Shift runs, right mouse drag looks, and C cycles camera modes. Golf retains ten player slots; fishing retains five independently coloured replaceable stations. Both preserve their existing fixed menu names. Swimming, golf shots, fishing mechanics and NPCs are outside this environment pass.

Run `Tools/Build/Review-RefinedIslands.ps1 -RecordRoutes` for actual Windows captures and route checks. The two `Test-RefinedIslandRooms.ps1 -Sport Golf/Fishing` runs check local host/guest movement. `Test-GolfIsland.ps1` and `Test-FishingIsland.ps1` retain the additional shore and module checks; their layout-dependent expectations now use the current manifests. `Test-GolfRooms.ps1 -Transport Local` and `Test-FishingRooms.ps1` check room capacities and authority. All diagnostics require explicit command-line opt-in.

The football tackle update uses network protocol 6. See `Docs/VisualDirection/TACKLE-AUTHORING.md` for the Tackle button, Space shortcut, authoring commands and gameplay checks. Every participant must use matching builds. See `Docs/GOLF-VERIFICATION.md` for the historical golf package checks and unverified phone tests.

The editable turn source is `ArtSource/Shared/Characters/RainbowSprinterTurns.blend`. Export saved turns with `Tools/Blender/turn_rainbow_sprinter.py`, then rebuild the scene and players. [Turn authoring](VisualDirection/TURN-AUTHORING.md) covers controls, timing, commands and the forward/backward gameplay checks. The motor responds to forward/backward input every simulation tick while smoothing body facing independently; it never waits for a turn clip to finish.

## Git

Git LFS tracks `.blend`, `.fbx`, `.png` and source `.zip` files. Install Git LFS before cloning. Unity `.meta` files, sources, project settings, package manifest and lock belong in Git. Library, Temp, Logs, Builds, downloaded toolchains and credentials do not. `Legacy/` is a local archive excluded from Git and can be deleted after review. The GitHub remote is `https://github.com/Benliew0928/WHATTHE-FISH-`.
