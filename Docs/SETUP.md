# Setup and build

## Installed versions

Unity: `C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe`

Blender: `C:\Program Files\Blender Foundation\Blender 5.1\blender.exe` (5.1.2)

Project: `C:\UMPSA\Game`

Use Unity Hub → Add project from disk → select `C:\UMPSA\Game`. Let package import finish. The manifest and lock pin URP, Multiplayer Services and NGO. These packages include the Relay/Lobby integrations and Transport dependencies.

## Android toolchain

Android Build Support and the bundled SDK, NDK r27c, CMake and OpenJDK 17 are installed under this editor. Official downloads are cached in `Tools/Downloads`. `Tools/Build/Install-Android.ps1` installs the versions declared in the editor's `modules.json`; it requires administrator rights for Program Files.

Unity Preferences → External Tools should use the installed Unity SDK/NDK/JDK. Build Settings/Build Profiles → Android. The project uses IL2CPP, ARM64, minimum API 26, landscape, package ID `com.umpsa.sportsprototype`. The identifier is a development placeholder; choose your publishing ID before release.

Use **Sports → Build Android development APK**, or run `Tools/Build/Build.ps1 -Target Android`. Output: `Builds/Android/SportsPrototype.apk`. Development APKs use a debug signing key. Do not publish this build. Signing keys are excluded from Git.

Connect a phone with USB debugging enabled and accept its debugging prompt. Run `adb devices`, then `adb install -r C:\UMPSA\Builds\Android\SportsPrototype.apk`. Launch SportsPrototype. Airplane-mode offline testing and sustained FPS measurements must be performed on the actual target phone.

## Unity cloud project — required for internet rooms

1. Sign into Unity Hub/Editor with your own account. In Project Settings → Services, create or link a **development** cloud project in your intended organisation.
2. In that project's Unity Gaming Services dashboard, enable Authentication with anonymous sign-in and enable Multiplayer Services/Lobby/Relay. Use the same environment for every build.
3. Rebuild both players after linking. The cloud project ID is build configuration, not a secret. Never commit service account credentials, signing keys or access tokens.
4. On one device choose Football → Create internet room. Share its session code. On another internet connection enter the code and join. All players must mark ready before the host starts.
5. A room supports one host plus nine guests. The SDK maintains the session/lobby connection; Relay carries NGO traffic. Host departure ends this prototype's room. Host migration is intentionally disabled at the application layer.

The project is linked to the newly created **SportsPrototype** cloud project `633e314c-46d1-4b48-bbbc-2dc5f6fb653c`, for a general audience. Its Authentication/Lobby/Relay services have passed real create/join tests. The current runtime uses its default `production` environment while separate development environment setup is pending dashboard access. This is a new prototype project, not an unrelated existing project. Offline exploration stays usable independently of cloud services. No paid plan or third-party API purchase is configured by this project.

`Tools/Build/Test-CloudRooms.ps1` exercises real services with separate anonymous test profiles. It creates temporary rooms, tests ten-player capacity and two-room isolation, and writes evidence under `Builds/CloudQA-*`. Test processes exit automatically. This uses service quota and should only be run when needed. Local transport-only testing is available through `Test-LocalRooms.ps1`.

## Rebuild art and scene

Close Unity while regenerating art, or wait for FBX export to finish before triggering asset import.

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.1\blender.exe' --background --factory-startup --python C:\UMPSA\Tools\Blender\build_assets.py
```

This overwrites the generated `.blend` and FBX assets. **Save hand-edited variants under a new name before regenerating.** All modelling lives in the Blender script and editable `.blend` files. No visible Unity primitive is used. Export axes are -Z forward / Y up, metres, without leaf bones. Solid material colours currently replace bitmap textures; no external texture pack is required.

To export your hand-edited `.blend` files without rebuilding the models, run Blender with `--background --factory-startup --python C:\UMPSA\Tools\Blender\export_assets.py`. That tool exports FBX plus any image textures in the sources as PNG; it does not save changes to the source files.

In Unity choose **Sports → Rebuild prototype scene** to reconstruct the generated Bootstrap scene and prefabs. This also replaces the generated Animator Controller. Keep manual scene variations separately. Then build Windows or Android from the Sports menu.

## Git

Git LFS tracks `.blend`, `.fbx` and `.png`. Install Git LFS before cloning. Unity `.meta` files, sources, project settings, package manifest and lock belong in Git. Library, Temp, Logs, Builds, downloaded toolchains and credentials do not. No remote repository has been created or published.
