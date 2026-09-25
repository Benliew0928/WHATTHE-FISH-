# Rally Windows integration — 25 September 2026

The WindowsFinal player now uses Rally Court in Basketball. Launch `Builds/WindowsFinal/WhatTheFish.exe`, then choose **Let's play → Basketball → Explore offline**. Use **Stadium customisation** to switch logos, colors or the name.

The build completed at 09:33:14 UTC / 17:33:14 Malaysia time. `LATEST-BUILD.txt` records the included environments. Unity's launcher executable can retain its original file date; the updated game content is in `WhatTheFish_Data/data.unity3d` and `Managed/Assembly-CSharp.dll`.

## Integration

- `BasketballBuilder` instantiates the modular Rally assembly in `Bootstrap` and saves the connected `BasketballEnvironment` resource prefab.
- The interior menu camera stays below the canopy. Updated camera/player bounds match the new court, with canopy, scorer-table and team-chair proxies.
- Four logo indices are preserved: Rally crest, star, lightning bolt, shield. Only the three legacy graphic meshes are copied; the placeholder architecture is not instantiated at runtime.
- Existing saved palettes tint the seating and court/hoop trim with per-renderer property blocks. Reset restores the original authored palette without changing shared material assets. Arena names appear on all four scoreboard faces.
- Lighting belongs to the active environment; sport switching leaves exactly one environment active. Football retains Sunvale.

## Checks on the rebuilt executable

Evidence directory: `Builds/RallyIntegration-20260925-173406/`.

| Check | Result |
| --- | --- |
| Windows build | Passed, 198,366,211 bytes reported by Unity |
| Modular court/stadium and 48 seating LOD groups | Passed |
| Ten distinct spawns, all with floor support | Passed |
| Four arena boundaries and overhead camera bound | Passed |
| Four logo selections and four palette applications | Passed |
| Third-person wall clearance | Passed |
| First-person, third-person, elevated views | Captured and inspected |
| Offline movement | Passed, 4.6 m |
| Football smoke regression | Passed |
| Local two-player host/guest | Passed; guest started on Football and adopted Basketball, host palette 2 and logo 2 |
| Host start and return to waiting | Passed on both clients |
| Guest cannot change sport or logo | Passed |
| Runtime exceptions / failed probe assertions | None found in this run |

[Actual Windows player screenshot](VisualDirection/Basketball/06_Windows_Player.png).

Assembly-CSharp.dll SHA-256: `C243B93D174E0F07355C5870162DFBB45C7198F1B7267337A879A92817AE2571`.

This is a Windows integration check, not an Android performance qualification or a new Relay stress test. Score values and shot clocks remain decorative; ball physics and match gameplay are unchanged.

## Court surround flicker correction — 17:42 Malaysia time

The teal apron and dark foundation were both authored with top faces at height zero. This caused depth-buffer z-fighting: rotating the camera changed which color appeared. The foundation now ends at -0.11 m, at the underside of the apron; the finished court and collision floor remain at zero. The Blender source and generator are corrected, and both the source audit and scene builder reject overlapping floor layers.

The executable was rebuilt at 09:42:40 UTC / 17:42:40 Malaysia time. Runtime evidence is in `Builds/RallyFloorQA-20260925-174327/`: the imported clearance check passed, four complete camera orbits produced 16 captures covering all margins, and the basketball smoke checks passed. Inspected opposing-side captures show a continuous teal surround without the alternating navy patches. See [the corrected apron](VisualDirection/Basketball/07_Apron_Fixed.png).
