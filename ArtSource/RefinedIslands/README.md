# G2 / L2 coastal environment authoring

The user selected **G2 Limestone Cove Links** and **L2 Limestone Garden Lagoon** on 27 September 2026. The new library does not write football or basketball art.

- `IslandAssetKit.blend`: reusable metre-scale palms, plants, grass, limestone, pier, bridge, welcome pavilion and shelter. LOD0 is the authoring quality; LOD1/2 are derived meshes.
- `Golf_G2.blend` and `Fishing_L2.blend`: assembled environment masters with packed material images, terrain, structures and ground-conforming grass. Higher grass LODs are hidden in Blender renders.
- `Golf_G2-layout.json` / `Fishing_L2-layout.json`: exported copies of the layout manifests used by Unity. Source generators own these files; change placements and paths in `Tools/Blender/build_refined_islands.py`, then regenerate.
- Portable Unity assets: `Game/Assets/_Game/Art/RefinedIslands/`. The `Shared/materials.json` manifest declares 2K albedo/normal/metallic-smoothness maps. Terrain macro/control maps are 4K. UVs use four metres per 2K tile; limestone uses world-space triplanar detail.
- Per-island `layout.json` defines geometry placement, capacity, spawns, routes, boundaries, review views, signs and recovery points. Game rules retain the existing sport IDs and profile names.

Run from the repository root with Unity closed, using your Blender executable path:

```powershell
& '<path to Blender 5.1>/blender.exe' --background --factory-startup --python-exit-code 1 --python Tools/Blender/build_refined_islands.py
```

For layout iteration, append `-- --reuse-kit`. To rebuild modules with cached textures, append `-- --reuse-kit --rebuild-kit`. Full generation replaces generated sources and exports; preserve any manual Blender variations under a different name first.

Unity `RefinedIslandBuildEntry.BuildWindows` updates only golf/fishing objects in the saved Bootstrap scene and rebuilds the Windows player. `Tools/Build/Build.ps1 -Target Windows` reconstructs the complete game from all current builders. The island-only entry preserves approved stadium scene objects during art iteration.

Validation entry points:

```powershell
# Source integrity, UVs, materials and closed deck/bridge geometry
& 'C:\Program Files\Blender Foundation\Blender 5.1\blender.exe' --background --factory-startup --python-exit-code 1 --python Tools/Blender/audit_refined_islands.py
# Actual EXE captures, real-controller routes, containment, cameras and switching
Tools/Build/Review-RefinedIslands.ps1 -RecordRoutes
Tools/Build/Test-RefinedIslandRooms.ps1 -Sport Golf
Tools/Build/Test-RefinedIslandRooms.ps1 -Sport Fishing
# Visible 1080p profiling, including Windows process memory and raw exit status
Tools/Build/Benchmark-RefinedIslands.ps1
```

The benchmark wrapper launches the visible player at 1920×1080. Its runtime argument `-islandBenchmark <absolute-report-path>` intentionally rejects batch mode. The wrapper samples Windows process memory and records the exit status; it recognizes the documented native shutdown fault only after complete valid measurements. Review captures and videos remain distinct from imagegen concept art.

Water and foliage animation are visual only. No fishing, golf-shot, swimming, audience or new networking mechanics are included.
