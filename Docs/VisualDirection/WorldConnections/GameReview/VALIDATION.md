# Sky-Sail validation

29 September 2026 · Unity 6000.3.20f1 · Windows development player, Direct3D 11.

## Passed

- Full clockwise circuit: Football → Golf → Basketball → Fishing → Football. Each leg completed preparation, boarding, its 44-second ride, docking and disembarkation.
- All eight directed route definitions meet their station berths and remain above sea level.
- The departure scene unloaded during every ride. Only the destination detail was active after docking. Walking and supported player placement were restored at all four ports.
- Real character-controller traversal in both directions on all four shore approaches, including the transition between gangway and boarding deck. Golf's deck was raised to clear the terrain. Conflicting golf/fishing rocks and planting were cleared from the station reservation.
- Two independent player processes connected over localhost. Host and guest travelled Football → Golf and Golf → Football, agreed with the host's destination, disembarked together and restored normal network transforms. The guest could not independently initiate travel.
- The Windows player and Android release APK compiled successfully. Custom ocean, distant-island, cable and sign shaders are included in the build workflow.

The report files contain the per-check results: [circuit](review.txt), [walking approaches](Approaches/review.txt), [host](Host/review.txt), [guest](Guest/review.txt).

Delivered Windows build: `Builds/WindowsFinal/WhatTheFish.exe`, built 29 September at 08:42 UTC; the complete build report records 672,008,993 bytes. Android artifact: `Builds/Android/WhatTheFish-release.apk`, built at 08:45 UTC, 354,313,157 bytes (354.31 MB), ARM64 and minimum Android API 26. Build size is not runtime memory usage.

## Visual review

The gallery contains actual 1920 × 1080 scene captures from the executable: world overview, four station views, cabin interiors and three views from each ride. The automated camera capture excludes overlay UI. Assets were checked for cabin detail, station clearance, cable alignment and consistent surrounding island views.

The editable art is split into seven Blender/FBX modules, seven module prefabs, a composed cabin prefab, and four cable-span prefabs. The cabin retains its detailed meshes throughout transit. Distant island models are derived from the existing islands rather than unrelated concept images.

All seven standalone Blender modules were opened and checked for linked scene objects. The kit master presents the modules as spaced collection instances. `Tools/Blender/package_sky_sail_sources.py` repeats that source packaging/open audit without changing runtime FBX geometry.

## Scope of the evidence

The automated player runs use batch mode and are functional/visual checks, not frame-rate benchmarks. Localhost validates synchronization logic, not internet latency, packet loss or reconnect behavior. No maximum-capacity party, slow-client timeout, interrupted ride or sustained heat test was performed in this pass.

No physical Android device is connected and no minimum phone model has been selected. APK compilation is verified; phone frame times, loading hitches, memory peaks, touch usability and thermal behavior remain unverified. The mobile configuration retains the existing 30 FPS target, 0.9 render scale, 2× MSAA and 65 m shadow range. Both source and destination detail can be resident while preparing, even though only one is rendered.

## Reproduce

```powershell
Tools/Build/Build.ps1 -Target Windows
Tools/Build/Test-SkySail.ps1
Tools/Build/Test-SkySail.ps1 -WalkOnly -OutputPath Docs/VisualDirection/WorldConnections/GameReview/Approaches
Tools/Build/Test-SkySail.ps1 -Network
Tools/Build/Build.ps1 -Target AndroidRelease
```

For a quick opening view, use `Test-SkySail.ps1 -CaptureOnly` with a separate output directory. The test runner checks successful completion and scans the player logs for runtime exceptions and shader errors.

Play with `Builds/WindowsFinal/Explore-SkySail.cmd`, or walk to a station in the normal game. The host chooses either neighboring island once everyone is at the station. Existing destination capacities still apply, and every room remains on one game island together.
