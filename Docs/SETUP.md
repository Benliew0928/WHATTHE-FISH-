# Setup and build

## Installed versions

Unity: `C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe`

Blender: `C:\Program Files\Blender Foundation\Blender 5.1\blender.exe` (5.1.2)

Project: `C:\UMPSA\Game`

Use Unity Hub → Add project from disk → select `C:\UMPSA\Game`. Let package import finish. The manifest and lock pin URP, Multiplayer Services and NGO. These packages include the Relay/Lobby integrations and Transport dependencies.

## Android toolchain

Android Build Support and the bundled SDK, NDK r27c, CMake and OpenJDK 17 are installed under this editor. Official downloads are cached in `Tools/Downloads`. `Tools/Build/Install-Android.ps1` installs the versions declared in the editor's `modules.json`; it requires administrator rights for Program Files.

Unity Preferences → External Tools should use the installed Unity SDK/NDK/JDK. Build Settings/Build Profiles → Android. The project uses IL2CPP, ARM64, minimum API 26, landscape, package ID `com.umpsa.sportsprototype`. The identifier is a development placeholder; choose your publishing ID before release.

The current APK is `Builds/Android/SportsPrototype-release.apk` (rebuild with `Tools/Build/Build.ps1 -Target AndroidRelease`). To create an optional development APK, use **Sports → Build Android development APK**, or run `Tools/Build/Build.ps1 -Target Android`. Output: `Builds/Android/SportsPrototype.apk`. Development APKs use a debug signing key. Do not publish this build. Signing keys are excluded from Git.

Connect a phone with USB debugging enabled and accept its debugging prompt. Run `adb devices`, then `adb install -r C:\UMPSA\Builds\Android\SportsPrototype-release.apk`. Launch SportsPrototype. Airplane-mode offline testing and sustained FPS measurements must be performed on the actual target phone.

## Unity cloud project — required for internet rooms

1. Sign into Unity Hub/Editor with your own account. In Project Settings → Services, create or link a **development** cloud project in your intended organisation.
2. In that project's Unity Gaming Services dashboard, enable Authentication with anonymous sign-in and enable Multiplayer Services/Lobby/Relay. Use the same environment for every build.
3. Rebuild both players after linking. The cloud project ID is build configuration, not a secret. Never commit service account credentials, signing keys or access tokens.
4. On one device choose Football, Basketball or Golf → Create internet room. Share its session code. On another internet connection enter the code and join. All players must mark ready before the host starts.
5. A room supports one host plus nine guests. The SDK maintains the session/lobby connection; Relay carries NGO traffic. Host departure ends this prototype's room. Host migration is intentionally disabled at the application layer.

The project is linked to the newly created **SportsPrototype** cloud project `633e314c-46d1-4b48-bbbc-2dc5f6fb653c`, for a general audience. Its Authentication/Lobby/Relay services have passed real create/join tests. The current runtime uses its default `production` environment while separate development environment setup is pending dashboard access. This is a new prototype project, not an unrelated existing project. Offline exploration stays usable independently of cloud services. No paid plan or third-party API purchase is configured by this project.

`Tools/Build/Test-CloudRooms.ps1` exercises real services with separate anonymous test profiles. It creates temporary rooms, tests ten-player capacity and two-room isolation, and writes evidence under `Builds/CloudQA-*`. Test processes exit automatically. This uses service quota and should only be run when needed. Local transport-only testing is available through `Test-LocalRooms.ps1`.

## Rebuild art and scene

Close Unity while regenerating art, or wait for FBX export to finish before triggering asset import.

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.1\blender.exe' --background --factory-startup --python C:\UMPSA\Tools\Blender\build_football_stadium.py
```

This rebuilds the football stadium only. **Save hand-edited variants under a new name before regenerating.** The venue assets use editable Blender files. Export axes are -Z forward / Y up, metres, without leaf bones.

To export the hand-edited Tidebloom golf `.blend` without rebuilding it, run Blender with `--background --factory-startup --python C:\UMPSA\Tools\Blender\export_assets.py`. It does not overwrite the Rainbow Sprinter.

To regenerate the current character from the preserved Meshy ZIP, run `python C:\UMPSA\Tools\Blender\prepare_rainbow_sprinter.py`, then run Blender with `--background --factory-startup --python C:\UMPSA\Tools\Blender\export_rainbow_sprinter.py`. This produces the editable master, a static skinned model FBX with two LODs, a separate FBX containing only the supplied run take, and the texture maps used by Unity. The two exports share the same rig. Character appearance controls are paused for this one-piece mesh.

The authored idle is maintained separately in `ArtSource/Shared/Characters/RainbowSprinterAnimation.blend`. Run `Tools/Blender/animate_rainbow_sprinter.py -- export` through Blender to export saved animation edits before rebuilding the scene. Initial setup, controls, preview videos and validation are documented in [Idle authoring](VisualDirection/IDLE-AUTHORING.md). The Meshy regeneration command does not update this animation source.

In Unity choose **Sports → Rebuild prototype scene** to reconstruct the generated Bootstrap scene and prefabs. This also replaces the generated Animator Controller. Keep manual scene variations separately. Then build Windows or Android from the Sports menu.

## Basketball arena

Generate only the basketball art with:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.1\blender.exe' --background --factory-startup --python C:\UMPSA\Tools\Blender\build_rally_arena.py
```

Import the regenerated modules using the Unity menu **Sports/Basketball/Build Rally art library**, then run `Tools/Build/Build.ps1 -Target Scene`, followed by Windows and Android builds. Keep hand-edited Blender variants separately before regeneration. Scene rebuilding configures all three environments, the four logo references, spawn points, cameras and lighting.

Choose Basketball → Stadium customisation to change the arena name, accent palette and center logo. In a room only the host has this control. Return to the waiting room to edit settings, then start again. Logo selection persists locally and appears for joining guests. No image upload is included.

Run `Tools/Build/Test-BasketballRooms.ps1 -Transport Local` for local checks or `-Transport Cloud` for real Relay checks. The latter creates temporary ten-player basketball and two-player football rooms using the linked prototype cloud project. Processes and evidence are isolated per run. Run `python Tools/Build/check_basketball_evidence.py <evidence-folder>` to compare actual movement and replicated positions. Use the same new build for every participant.

## Golf island

Generate only the island with:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.1\blender.exe' --background --factory-startup --python C:\UMPSA\Tools\Blender\build_golf.py
```

Run `Tools/Build/Build.ps1 -Target Scene`, then build Windows and Android. The scene builder configures all three sports. The golfer uses the shared athlete and controls; holding Shift or pushing the thumb stick fully runs at 7 m/s. Golf has a fixed name and no stadium customization controls.

Use `-sport Golf -probe -golfAudit -report <absolute-path> -exitAfter 145` on the Windows development player to check the current Tidebloom shoreline, spawn support, fairway traversal and bunker exits. Prefer `Tools/Build/Test-GolfIsland.ps1` for the complete check. `-sport Golf -probe -smoke` captures the island overview, shore and all three cameras. Probes run only when explicitly requested by command-line flags.

Run `Tools/Build/Test-GolfRooms.ps1 -Transport Local` or `-Transport Cloud` to check a ten-player golf room alongside a two-player basketball room. Cloud launches are spaced to avoid authentication bursts. Run `python Tools/Build/check_basketball_evidence.py <golf-evidence-folder>` to compare movement and settled host/guest positions; this existing checker supports either sport.

The football tackle update uses network protocol 6. See `Docs/VisualDirection/TACKLE-AUTHORING.md` for the Tackle button, Space shortcut, authoring commands and gameplay checks. Every participant must use matching builds. See `Docs/GOLF-VERIFICATION.md` for the historical golf package checks and unverified phone tests.

The editable turn source is `ArtSource/Shared/Characters/RainbowSprinterTurns.blend`. Export saved turns with `Tools/Blender/turn_rainbow_sprinter.py`, then rebuild the scene and players. [Turn authoring](VisualDirection/TURN-AUTHORING.md) covers controls, timing, commands and the forward/backward gameplay checks. The motor responds to forward/backward input every simulation tick while smoothing body facing independently; it never waits for a turn clip to finish.

## Git

Git LFS tracks `.blend`, `.fbx`, `.png` and source `.zip` files. Install Git LFS before cloning. Unity `.meta` files, sources, project settings, package manifest and lock belong in Git. Library, Temp, Logs, Builds, downloaded toolchains and credentials do not. `Legacy/` is a local archive excluded from Git and can be deleted after review. No remote repository has been created or published.
