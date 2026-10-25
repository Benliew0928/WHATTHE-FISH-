# Venue solid collision maintenance

## Collision policy

`VenueCollisionBuild` repairs the five shipping scenes and runs before Windows and
Android packaging. Stadium/coast/refined-island/rail generators also apply the
same rules. Repeating the bake is idempotent. Save open scenes before running the
editor menu **WHATTHE FISH? / World / Repair venue collisions**.

- Football: low-detail seating, canopy, floodlight towers, perimeter boards,
  dugouts, scoreboard/banner supports and landscape planters; solid stone, timber and teal facade parts.
- Basketball: low-detail upper/lower seating, railings, perimeter pads,
  courtside furniture, hoop bases/supports and solid facade/screen parts.
  The net, rim and backboard retain their existing gameplay physics.
- Both coastal islands: shoreline rocks, garden outcrops, dock ropes and palm
  trunks. Palm collision extracts only the bark submesh; foliage stays decorative.
- Golf/Fishing: reuse LOD1 bark for bent palms and remove the old straight support
  boxes. Existing pavilion columns, shelters, bridges, terrain and rock colliders
  remain in place.
- Shared Sky-Sail world: solid station timber/frame/roof and cable tower structure.
  Existing berth guards, moving cabin floor/walls/doors and gangways remain intact.

Each new static MeshCollider lives on a `Venue solid collision` child on layer 8,
independent of renderer LOD visibility. Existing player and camera collision queries
therefore see the same surfaces. Concave geometry preserves arches, seat aisles and
entrances. Existing smooth stair ramps remain; do not replace entire structures
with their axis-aligned bounds. Do not add colliders to water, distant island
proxies, grass, foliage or gameplay equipment through this bake.

Generated mesh assets include compact bark-only subsets under
`Game/Assets/_Game/Art/VenueCollision/`. Their names identify source mesh GUID,
local file ID and selected submesh indices. They are regenerated deterministically
and contain only positions/indices; source FBX art and visual materials are
unchanged. The `Walkway_` copies lower complete basketball balcony column-cap
blocks by 0.2 m, keeping the visible cap and its collision aligned while making
the landing passable with the existing 0.3 m player step height. Their UVs, normals,
colors and bevels are preserved. Football spectator steps retain their existing
smooth ramp collider instead of overlapping it with individual visual step boxes. All other proxies reuse existing source meshes, including low-detail
seats. No runtime mesh reads or runtime collider generation is needed.

## Verification

`VenueCollisionBuild.BuildVerification` builds a complete development player at
`Builds/VenueCollisionQA/Player/` and writes surface fixtures next to it. Launch
that player with `-offline -sport Football` (or Basketball/Golf/Fishing) and
`-venueCollisionReview <output-folder>`; the output folder must be directly below
`Builds/VenueCollisionQA/` so it can read those fixtures.

The opt-in development review checks shipped surface raycasts, actual player
sprint contacts, collision persistence across LOD switches, four stadium entrances,
external stairs, spectator aisles, garden bridges, authored Golf/Fishing routes
and the active station boarding apron. Logs and rendered captures stay in ignored
`Builds/VenueCollisionQA/`. Windows checks do not certify physical-phone FPS or
live cloud multiplayer.

## Validated delivery — 26 October 2026

Final complete desktop build: 6,288 passing assertions (Football 1,891,
Basketball 2,226, Golf 1,597, Fishing 574). Evidence is under
`Builds/VenueCollisionQA/Final-*/`. The review checks 5,369 mesh surfaces,
144 real sprint contacts and 144 camera sweeps, plus jump/slide contacts and
normal routes. For Golf/Fishing, old preview waypoints are projected to free
nearby ground and walked around existing five-hole rocks/gangway guards;
these intentional obstacles are not treated as missing passageways.

| Scene | Dedicated solid proxies |
| --- | ---: |
| Bootstrap stations/towers | 36 |
| Football | 279 |
| Basketball | 322 |
| Golf | 173 |
| Fishing | 30 |

All 56 generated mesh assets are referenced by active scenes. The full desktop
player was rebuilt and tested with desktop rendering; shader/material resources
are included in the complete player. 335 candidate files were copied and
hash-verified, with two prior preview launchers retained in `Builds/WindowsFinal/`.
The APK is 167,993,556 bytes; signed payload and five-scene bundle checks pass.
See [measured build record](BUILD-SIZE.md) for the exact hash and size audit.
No source assets or gameplay tuning outside this collision policy were discarded;
concurrent awards/event source and art changes are preserved.

Reviewed superseded outputs were archived with `Archive-LocalArtifacts.ps1`:

- `Legacy/20261025-233621-865-venue-collision-first-pass/`
- `Legacy/20261025-234605-023-venue-collision-review-v2/`
- `Legacy/20261025-234830-761-venue-collision-camera-fixture/`
- `Legacy/20261025-234844-486-venue-collision-android-backup/`
- `Legacy/20261025-235115-442-venue-collision-windows/`
- `Legacy/20261025-235728-025-venue-collision-desktop-rebuild/`
- `Legacy/20261026-000415-368-venue-collision-final-desktop/`

The current validation evidence remains in ignored `Builds/VenueCollisionQA/`.
Phone hardware rendering/touch/FPS and live cloud multiplayer need separate
acceptance testing; local Windows collision checks do not establish those results.
