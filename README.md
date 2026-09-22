# SportsPrototype

Android-first, stadium-first student prototype. The working title is temporary.

Open `Game` in Unity **6000.3.20f1**. Open `Assets/_Game/Scenes/Bootstrap.unity`, then press Play. The football stadium and athlete are original Blender assets. There are no spectator NPCs.

## Where things live

- `Game/` — Unity URP project, runtime code, materials, prefabs, scenes and package locks.
- `ArtSource/Football/Stadium.blend` — editable stadium mesh, review camera and lighting.
- `ArtSource/Shared/Characters/Athlete.blend` — editable chibi athlete, rig, Idle/Walk/Run actions.
- `ArtSource/Basketball/`, `ArtSource/Golf/` — reserved for later sports.
- `Tools/Blender/` — deterministic asset generator and audit tool.
- `Tools/Build/` — build/environment scripts.
- `Builds/WindowsFinal/` — latest development Windows player (launch `SportsPrototype.exe`). Other Windows build folders contain earlier review builds.
- `Builds/Android/` — Android output location when Android support is installed.
- `Docs/SETUP.md` — toolchain, cloud linking and running on a phone.
- `Docs/ARCHITECTURE.md` — code boundaries and future gameplay.
- `Docs/VERIFICATION.md` — measured checks and remaining access requirements.

## Controls

Landscape: left thumb stick moves; push it fully to run. Drag on the right to look. Tap **Camera** to cycle first person, third person and elevated views. On Windows: WASD, Shift, right mouse drag, C. Escape returns to the menu/waiting room. A guest leaving the pitch leaves the room; the host can return everyone to the waiting room.

The room system uses Unity Multiplayer Services sessions, Relay, Unity Transport and Netcode for GameObjects. Offline exploration requires no Unity account or internet connection. Online rooms require your cloud project to be linked and services enabled.

This milestone is exploration, appearance customisation and networking infrastructure. Football ball physics, scoring, bots, basketball and golf gameplay are future work.
