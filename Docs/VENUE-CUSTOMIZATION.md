# Venue customization

Every sport tab in **Customize venue** opens its controls immediately. The page
keeps the controls beside a live world preview, with Reset look and Save look
always visible. Saving stays on the same page. Back and switching sports protect
unsaved changes with a discard confirmation.

| Sport | Available choices | Preview views |
| --- | --- | --- |
| Football | Venue name; Mint/Rose/Lilac/Gold accent; venue-title or team-welcome screen; three board messages; decorative flags | Overview, big screen, flags |
| Basketball | Venue name; four seat/court-trim palettes; Rally crest, Star, Lightning or Shield centre logo | Overview, centre court, seats and trim, sign |
| Golf | Venue name; four numbered-course-flag accents; Original, Sea glass or Copper club finish; venue-name or team-welcome sign | Island, clubs, flags, sign |
| Fishing | Venue name; four pier accents; Original, Sea glass or Copper rod finish; venue-name or team-welcome sign | Island, rods, piers, sign |

Selecting a choice focuses the part it changes. Football messages and colours
show the big screen; toggling flags shows a decorative banner. Basketball logos show
centre court, and palettes show the seats and court trim. Editing a venue name
shows its world sign in every sport. Save keeps the selected preview view.
The preview texture uses the display frame's aspect ratio so the scene is not
stretched. Its camera is independent of the last gameplay camera.

The football broadcast owns the replacement giant screens during play. Outside
coverage, and while the venue editor is visible, the editable stadium sign is
restored. Previously, broadcast initialization disabled that sign permanently,
so changing its stored text left the preview blank. The switch happens during
normal presentation, preserving safe island unload and application teardown.

Golf and Fishing reuse the authored island, equipment meshes and textures.
Finishes tint the rack and held equipment, including equipment created after
saving. Golf flag recolouring preserves neutral numbers/outlines and the existing
wind animation. The default restores the authored flag fabric. Fishing changes
the existing modular pier accent slots. Welcome signs retain their positions.

The choices are cosmetic: they do not change clubs' shot power, rod/reel
performance, fishing skills, fish populations, scoring, terrain, holes, collision,
water, travel routes or the four-island/shared-room model.

Designs are saved separately for each sport on the device. In a room, the host
controls the current island's look and saves it for every peer. Guests can inspect
the controls and previews; editing is disabled and the controls refresh when the
host saves. Sport tabs are locked to the room's shared island. Existing travel
controls still move the room together.

## Implementation

The existing `StadiumAppearance` payload is retained. For Golf and Fishing,
`palette` selects island accents, `design` selects the equipment finish and
`screen` selects the welcome-sign style. The other fields keep their existing
meaning in Football and Basketball. Island preferences use separate `venue.Golf`
and `venue.Fishing` keys. Reset restores a draft and requires Save to persist.

`IslandVenueStyle` applies the island's presentation. `VenueEquipmentFinish`
uses renderer property blocks, preserving shared materials and source artwork.
New held clubs and rods read the current island appearance. No external image
dependencies or newly generated binary art are required.

## Validation

### Preview correction — 25 October 2026

The corrected Windows player passed **1,116 offline assertions**, **452 local
host/guest assertions across all four sports**, **38 football broadcast/replay
checks** and **26 stadium teardown checks**. The offline suite also passed all
**1,116 assertions** from a checkout path containing spaces, with an unrelated
working directory. Preview tests hide each target briefly and compare rendered
pixels, so a hidden sign or invisible logo can no longer pass through object
activation checks alone. Pointer tests cover saving while a name field is active.

The complete **342-file** Windows delivery retains the existing launchers;
all **334 candidate files other than the delivery note** were hash-verified.
All **493 compiled source documents** match the retained tested snapshot. This
snapshot includes the preview fixes and excludes the concurrent unfinished
scoreboard redesign. Its source changes remain in the primary workspace.

The official AndroidSubmission build passed at **126,891,150 bytes**, up
**10,680 bytes** from **126,880,470**, with **873,108,850 bytes** of headroom.
Its v2 signature, ZIP CRC, **1,252** audit entries and five-scene bundle integrity
passed. All **354 Android compiled source documents** match the same retained
snapshot. SHA-256: **85D456E1006E637D65115E4F9199CF6A3E1B8148301BE6D08EC32B00DE40DF52**.
Existing voice, crew invitations and protocol 86 are retained. The audit and
entry deltas are recorded in [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).

Evidence: `Builds/VenuePreviewQA/`,
`Builds/VenueCustomizationQA/Run-20261025-160959-473/`,
`Builds/FootballBroadcastQA/Run-20261025-160959-529/` and
`Builds/StadiumTeardownQA/Run-20261025-161211-497/`.
The previous complete Windows player is retained in
`Legacy/20261025-161338-555-venue-preview-windows/`.
The prior APK/audit is in `Legacy/20261025-162457-880-venue-preview-android/`;
a reviewed temporary generated-prefab backup is in
`Legacy/20261025-161654-559-venue-preview-generated/`.
Phone appearance, touch and FPS remain unverified.

### Running the checks

Run `Tools/Build/Test-VenueCustomization.ps1` against a development Windows
player. It exercises direct access to all four editors, real preview rendering,
live visual targets (including rendered pixel contributions), save/discard/re-entry, sport switching, control raycasts and
text layout, held equipment, and all four sports' host-to-guest replication with
guest write rejection. Captures and reports belong in ignored
`Builds/VenueCustomizationQA/`.

The Windows editor entry point is
`WhatTheFish.Editor.VenueCustomizationBuild.Build`. It writes a complete candidate
to `Builds/VenueCustomizationQA/Player/`. The final tested player must be promoted
to `Builds/WindowsFinal/`. Android delivery uses
`Tools/Build/Build.ps1 -Target AndroidSubmission`; actual measured size and
validation limits are recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

The 20 October 2026 Windows candidate passed **863** offline editor assertions,
**202** local host/guest assertions, **20** Golf equipment checks and **22**
Fishing rod checks. An additional **863** offline assertions passed with the
script's default player path in a copied checkout containing spaces, launched
from an unrelated working directory. Captures at 1600×900, 1280×800 and 1920×1080
were inspected. The requested 2340×1080 window was limited to 1920×1080 by the
desktop, so this is not an ultrawide-device validation.

The complete 327-file Windows candidate was hash-verified on promotion. All
venue-task C# sources match the compiled debug checksums. The shared workspace
also contained concurrent work: at verification, later changes in
`ControlLayoutReview.cs`, `PlayerHubStyle.cs` and `GolfBot.cs` were absent from
this Windows snapshot. The venue checks do not validate those unrelated features.
Local evidence is under `Builds/VenueCustomizationQA/`, including the two
`Run-20261020-*` folders, `GolfEquipment/`, `FishingRods/`, `PortableEvidence/`
and the compiled-source/promotion manifests.

The older Windows player was archived in
`Legacy/20261020-220000-318-venue-customization-windows/`. The previous APK/audit
is in `Legacy/20261020-215935-485-venue-customization-android/`. The temporary
portable checkout was archived in `Legacy/20261020-215835-509-venue-portability-copy/`
after retaining its evidence. An earlier Windows archive attempt stopped before
moving any file because the user was running that player; it retains only a
recovery manifest. The player was replaced after the user authorized closing and
reopening it.

Physical Android appearance, touch input and phone performance remain unverified.
