# Palm Shore coastal scenery

Set `BLENDER_PATH` for your installation as described in [Setup](../../Docs/SETUP.md); run commands from the repository root.

F1 football and B1 basketball island surroundings. Open `PalmShore_Islands.blend`; enable one of `Football_Exterior` and `Basketball_Exterior` at a time. Both assemblies use their venue centre as origin and are authored in metres, XY ground / Z up. The existing stadiums are deliberately not copied into this source.

The generator's independent scenery FBXs are corrected by a 180° Unity root rotation, matching the project's Blender import convention. The reusable cloud model retains its imported transform inside a separate scale/placement object.

Rebuild geometry with:

```powershell
& $env:BLENDER_PATH --background --factory-startup --python Tools/Blender/build_coastal_islands.py
```

This overwrites this generated coastal source and exports only. Preserve a separate copy before making manual edits. Neither stadium source is read or modified.

Re-run the source audit with `Tools/Blender/audit_coastal_islands.py` in background Blender. See `Docs/COASTAL-ISLANDS.md` for integration, review and scope.

The `Path` values in `venue-baseline.json` and `venue-preservation-audit.json` are relative to the repository root. The audit resolves them from its script location.
