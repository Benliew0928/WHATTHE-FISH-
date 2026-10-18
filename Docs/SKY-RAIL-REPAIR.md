# Sky Rail clearance and cabin assembly — 18 October 2026

The repair preserves the four-island circuit, one active destination per room,
shared group travel, and the existing island coordinates and scenery.

## Problems corrected

- The old curve climbed immediately out of the berth, carrying the suspension
  through the station canopy. Both ends now have a 16 m straight, level track
  before the climb begins. The same curve generates the visible cables.
- The old arrival heading turned the cabin 180 degrees inside the station.
  The cabin now turns gradually over open water and backs into the berth with
  its doorway facing the boarding apron. A separate trolley pitches with the
  cable while the cabin remains upright; the suspension swivels in yaw.
- Tower legs and service platforms occupied the cabin lane. The mast now sits
  6 m beside the cable, with a cantilever and sheaves contacting it from above.
  Towers sit beyond the station's seaward approach, with separate branches for
  its two destinations; the Fishing–Football link no longer doubles back in a
  tight bend immediately outside the station.
- The station's solid deck intersected the undertray. The new U-shaped deck
  leaves a clear berth, with matching collision, a cabin threshold and a solid
  walking floor. Twenty disembark positions sit on the boarding apron.
- Open doors overlapped the rear body panels. Their tracks now sit outside the
  panels; a crossbar no longer blocks the doorway. Bumpers sit below the doors.
- Grab poles and overhead rails ended below the ceiling, grips floated below
  their rails, and straight roof ribs did not follow the arched soffit. These
  parts now have matching attachment geometry. Handrails have support brackets.
- The skylight was a flat panel laid over an unbroken curved roof. It now fills
  a real opening and follows the roof's curve, with a bronze perimeter frame.
- Passenger standing anchors have more clearance from seats and grab poles.
- Fishing's platform is raised above the coastal mound, its approach follows
  the ground, and the shoreline containment no longer cuts across its doorway.
  Runtime grass excludes the station footprints on all four islands.

## Sources and regeneration

Editable modules remain in [ArtSource/SkySail](../ArtSource/SkySail/README.md).
`CabinTrolley` is a separate module with its origin at the cable contact.
The [Blender generator](../Tools/Blender/build_sky_sail.py) exports relative FBX
dependencies. The [source audit](../Tools/Blender/audit_sky_sail.py) opens all
nine native sources and parses all eight FBX files, checks critical mounts and
clearances, and rejects active absolute or missing dependencies.

`SkySailBuilder.RefreshRailGeometry` updates the rail modules, route meshes and
Bootstrap assembly and Fishing's shore approach without regenerating other
island content or horizon proxies. `BuildRailReview` also creates a complete candidate under
`Builds/SkyRailQA/Player/`. The regular full world generator contains the same
geometry and collision rules.

## Verification

Evidence belongs under ignored `Builds/SkyRailQA/`. The geometry review cooks
the delivered LOD0 station/tower meshes into temporary collision probes and
sweeps body/suspension envelopes at 2,001 positions on each of eight directed
routes. It also checks route reversibility, heading continuity, all twenty
landing positions at each station, and actual controller traversal through the
open doorway. Close interior, doorway, ceiling, departure, tower and arrival
captures supplement these checks.

Run `Tools/Build/Test-SkySail.ps1 -GeometryOnly`, `-BothDirections`, `-WalkOnly`
and `-Network` with explicit `-OutputPath` under `Builds/` and `-PlayerPath`
pointing to the candidate. The both-directions run executes eight complete
rides; the local-room run checks shared host/guest journeys and disembarkation.

### Results

| Check | Result | Evidence under `Builds/SkyRailQA/` |
| --- | --- | --- |
| Editor mesh clearance and heading continuity | 20 passed | `editor-clearance.txt` |
| Rendered geometry, passenger anchors, supported landings and door traversal | 71 passed; 44 captures | `Verified-Geometry/` |
| Shore approach, station entry and exit on all four islands | 34 passed | `Final-Walk/` |
| Eight complete journeys, both directions around the circuit | 90 passed | `Final-Circuit/` |
| Local host/guest shared travel to Golf and back | 22 passed | `Verified-Network/Host/`, `Verified-Network/Guest/` |
| Native Blender and FBX dependencies | All 17 files passed | `source-audit.json` |
| Regeneration at a different path containing spaces, from an unrelated working directory | Passed | `portable-source-audit.json` |

All **237 assertions** passed. The passing player logs contain no reviewed
runtime exceptions, shader errors or inactive-controller errors. Close and
distant Windows captures were visually inspected, including cabin ceiling,
doorway, station berth, tower passages and arrival. Earlier diagnostic runs
remain as evidence; the table identifies the final passing runs.

The complete Windows candidate was rebuilt from the saved scenes and content.
Two subsequent script refreshes changed only `SkySailReview.cs` and
`SkySailGeometryReview.cs`: the network fixture now waits for the existing
room-entry teleport to finish, and the terrain sweep excludes the invisible
shoreline containment layer that flying cabins intentionally cross. These
refreshes did not alter gameplay scripts or serialized assets. Geometry and
network suites ran on the final candidate; circuit and walking suites ran on
the preceding candidate with identical gameplay code and packaged island
content. `fixture-only-script-delta.json` records this comparison. All **341**
runtime source documents in the final player's PDB match the working source.

This is a shared working-tree build, including the unrelated changes already
present at task start; it is not a clean committed release. Initial status and
compiled-source verification are retained with the task evidence. No Git
staging, commits or publishing were performed.

The mesh sweep tests delivered LOD0 geometry at discrete positions with body
and suspension envelopes. It is regression coverage, not a proof against every
possible rendering artifact. Mobile rendering, sustained phone frame times,
thermal behavior and peak memory still need physical-device validation. The
network test uses two local processes; WAN latency, reconnects and a live
20-player session were not tested. Landing support and passenger spacing were
checked analytically for parties up to twenty, respecting existing sport caps.

## Delivery

The complete, hash-verified Windows player is in `Builds/WindowsFinal/`:
**325 files, 509,736,401 bytes**, including data, runtime libraries, supporting
folders and launchers. Use `Explore-SkySail.cmd` to start beside the Football
station. `LATEST-BUILD.txt` records the exact source and validation limits.
`windows-promoted-files.json` preserves the delivered per-file hashes. The
previous player's delivery marker was checked against the task-start hash
before replacement; no newer concurrent delivery was overwritten.

Fresh `Tools/Build/Build.ps1 -Target AndroidSubmission` succeeded on
**18 October 2026 at 01:40:39 Malaysia time**. The actual APK changed from
**80,052,983 to 83,309,709 bytes (+3,256,726)**, leaving **916,690,291 bytes**
below the strict decimal 1 GB limit. Its APK v2 signature verifies. SHA-256:
`FEBF34C89F804123D49EF1576B79751615EAE86B5DAE6B6162109DE973D4D790`.
The opening local file differs from historical Golf records in the size
documents. This build also includes previously unbuilt shared-tree changes;
its growth is not an isolated measurement of Sky Rail.

The fresh contributor audit was reviewed and copied into the task evidence.
The islands bundle is the largest APK entry at **43,160,210 bytes**; the audit
also identifies the newly included Golf course/UI assets. See
[BUILD-SIZE.md](BUILD-SIZE.md) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).
No Blender masters, archives or experiments were added to the APK.

All obsolete files below were moved with the repository archive tool, which
records original paths, byte counts and SHA-256 hashes. Current validation
logs, captures, source hashes and before/after size audits remain under
`Builds/SkyRailQA/`.

| Archive batch under `Legacy/` | Reviewed contents |
| --- | --- |
| `20261018-012156-565-sky-rail-unused-mesh` | Old hanger teal mesh and its metadata, after reference/GUID/generator checks proved it unused by the separate trolley assembly |
| `20261018-013943-792-sky-rail-source-temp` | Blender temporary save file; final editable masters and dependencies audited |
| `20261018-014246-771-sky-rail-delivery` | Complete superseded Windows player |
| `20261018-014318-488-sky-rail-candidates` | Superseded task players, portable regeneration clone and opening APK recovery copy |

Native source/exporter provenance or saved UI history may retain historical
paths; the format-aware audit found no external Blender image/library
dependencies and no absolute or missing FBX dependency references. No binary
search-and-replace was used. New temporary evidence stays ignored, including
Blender intermediate `.blend@` saves; active editable sources and generated
delivery assets remain publishable with their Unity metadata and LFS rules.
