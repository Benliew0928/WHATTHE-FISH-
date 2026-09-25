# Sunvale football stadium

An original stadium inspired by the shape, color and material language of `Docs/VisualDirection/football-cartoon-concept.png`. The venue uses a warm limestone arcade, a scalloped teal canopy, coordinated seating zones and a sunburst identity. This is now the playable football environment in Bootstrap and the WindowsFinal player. The original placeholder is archived at `Legacy/ArtSource/Football/Stadium.blend`.

## Review

- Open `Stadium_Sunvale.blend`. The source opens on the pitch camera; the other cameras show the full bowl and an architectural close-up. Switch the viewport to Material Preview to see the packed painted atlas.
- Unity scene: `Game/Assets/_Game/Scenes/SunvaleStadiumReview.unity`.
- Complete Unity assembly: `Game/Assets/_Game/Prefabs/Football/Sunvale/SunvaleStadium.prefab`.
- Rendered views: `Docs/VisualDirection/Stadium/`. Views 04 and 05 are actual Unity renders with temporary desktop capture settings (4× MSAA, 4K soft shadows). Views 01–03 are Blender Cycles renders. The existing mobile pipeline settings are restored after capture.

## Modules

Each module has its own Blender collection and FBX, plus a Unity prefab. The source keeps structural, canopy and seating meshes separate within each of 32 bays so a local edit does not require detaching the whole bowl.

| Module | Contents |
| --- | --- |
| Stadium | Lower arcade, four wider entrance portals, terraces, stairs, rails, upper arcade, canopy and 3,072 seats |
| Lawn | 68 × 105 m marked pitch and a 78 × 115 m runoff base |
| Goal | One reusable frame and complete cord net; 7.32 × 2.44 m clear opening |
| Perimeter | Low colored boundary cushions |
| Banners | Hanging cloth silhouettes and modeled sunburst badges |
| Landscape | Trees, promenade planters and ivy |
| Floodlights | Four sculpted towers with six lenses each |
| Scoreboard | Framed venue sign, removable lettering and sun crest |
| CornerFlags | Four corner poles and flags |
| Dugouts | Two sideline shelters with bucket seats |

All modules except Goal use pitch center at ground level as their pivot. Assemble at position zero, rotation zero and scale one. Goal's FBX pivot is the middle of the goal line; the source assembly places two instances. In Unity, place the north instance at `(0, 0, -52.56)` and the south at `(0, 0, 52.56)` with a 180° Y rotation. The supplied assembly already does this. Blender coordinates convert to Unity as `(-x, z, -y)`.

## Materials and geometry

The shared 1024 × 512 sRGB atlas is an original procedural painted palette: soft stone washes, clean molded seat colors, teal roofing and broad grass variation. Its 32 padded swatches are UV-mapped into the actual mesh. The texture is packed into the blend and supplied as a PNG alongside the FBXs. Unity uses a single shared URP Lit material with roughness represented by smoothness 0.22. No Blender-only procedural shaders are needed at runtime.

Seats have separate near and far meshes per bay. LOD1 is hidden in the Blender review and included in the FBX. The Unity builder replaces Unity's inferred model-wide LOD with 32 independent LOD groups. Both levels must not be enabled manually together. The assembly uses simple lawn, goal-frame and runoff-boundary colliders; spectator aisles are visual geometry, not a finished traversable navigation area.

The roof has closed, outward-facing shells for correct backface culling in Unity. Lettering and banner graphics retain authored front-face winding. Review cameras, lights and the large ground plane are excluded from every FBX.

`stadium-manifest.json` records source triangle counts; `unity-import-audit.txt` records imported geometry, pitch dimensions, goal count, collider count and LOD checks. `blender-source-audit.json` checks finite geometry, UV bounds, closed canopy shells, packed texture and front-facing lettering. Run `Tools/Blender/audit_football_stadium.py` in background Blender to repeat those source checks. One material does not mean one draw call: the separate bays intentionally retain useful culling boundaries. Android frame time, memory and visual quality have not yet been measured on a device.

## Rebuild

From the repository root in PowerShell:

```powershell
& 'C:/Program Files/Blender Foundation/Blender 5.1/blender.exe' --background --factory-startup --python Tools/Blender/build_football_stadium.py -- --render
```

Then use **Sports → Football → Build Sunvale review assets** in Unity, or run `SunvaleStadiumBuilder.Build` in batch mode. This creates the module prefabs and isolated review scene. It does not rebuild Bootstrap, replace `FootballEnvironment.prefab`, or alter the sport selector.

To update the playable game, run `Tools/Build/Build.ps1 -Target Windows`. The Windows build command now runs `ProjectBuilder.Setup` first, which assembles Sunvale into Bootstrap and refreshes `FootballEnvironment.prefab`, then overwrites `Builds/WindowsFinal/SportsPrototype.exe` and its companion data. Always launch that path for the latest Windows build. `LATEST-BUILD.txt` beside it records the successful build time; the small Unity launcher executable can retain its original timestamp even when the game data is rebuilt.

The generator overwrites this Sunvale source and its exports. Keep a separate working copy before making hand edits. For hand-edited modules, export the relevant collection's mesh objects and root empty, using FBX -Z forward / Y up and no cameras or lights. Keep the atlas beside the exports.

## Integration scope

The lawn and goal remain separate supporting assets so their own art passes can happen later. The reference's cliffs, waterfall, bridge and giant football monument are outside this stadium asset. The playable environment replaces the fixed venue lettering with the existing customizable title and screen text. Flag controls toggle the banner and corner-flag modules; team colors tint those accents while preserving the authored seating zones and shared atlas. `SunvaleStadiumView` handles this without the old stadium's material remapper.

