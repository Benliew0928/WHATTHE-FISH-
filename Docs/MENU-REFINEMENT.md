# Shared rooms and sport-tab menus

## Design

The home page keeps the approved navy, lime and teal palette, Barlow display
font, official logo and four sport illustrations. A shared **Create room / Join
room** strip sits above the sport cards. Each sport opens two choices: Explore
offline and Versus bots. Bots remain available for Football and Basketball;
Golf and Fishing clearly explain their current limitation.

Create room chooses a starting island, rather than implying a separate room
system for each sport. One code works across the four islands. Existing host
authority, shared travel and one active game per room remain unchanged.

Customize venue and How to play use a single horizontal sport selector:
basketball, football, golf ball and fish. Inactive icons are grey; the selected
icon has its sport colour, a matching title and an underline. Venue browsing
shows one illustration and description, with an explicit Edit venue action for
supported sports. Existing live venue previews, save and discard are retained.
Guide browsing changes only the displayed guide, not the active world sport.
The illustrated Fishing tutorial remains available within the tabbed page.

## Room

The left player board retains numbered player and spectator places. The right
column separates sharing from readiness and departure. The invitation panel
reserves a 168 by 168 design-unit square for Huawei HMS room QR invitations.
A concurrent Scan Kit integration connected this slot to `Show room QR` during
validation; that work is preserved. Local test rooms display **Online room
only** instead of an invalid QR invitation. Room codes and Copy code remain
available. HMS encoding/scanning belongs to that concurrent integration, not
the menu redesign itself.

Seat occupancy, names, ready states and spectator permission update existing
controls in place. The room page, invitation, ready controls, QR placeholder
and open dialog survive player movement. The former roster-signature-triggered
whole-page rebuild and fade are removed. Seat assignment still uses the same
server-authoritative RPCs and capacity/permission checks.

## Verification and delivery

The final Windows interaction review passed **1,442 assertions**: 1,053
offline, 196 host and 193 guest, with no reported runtime exceptions. It ran
against the complete combined player in `Builds/ScanKitQA/Player/`.

Checks cover room actions on the home page, sport modes, all four guide tabs,
supported venue editors and save/discard, room-code validation and clipboard
fallback, text fit and pointer hits, several landscape window sizes, stable
seat changes on both peers (including a live confirmation dialog), readiness,
shared departure and return. Actual Windows screenshots were inspected for
the home page, sport choices, venue tabs, all guides, creation and room layout.
Phone rendering, touch, FPS and live WAN seat changes are not established by
these desktop tests. Huawei camera/scanning validation is recorded separately.

The initial run preserved in `MenuRefinement/` had two obsolete QR-placeholder
name assertions after concurrent integration replaced that slot with the
working QR control. The corrected `MenuRefinementFinal/` rerun passed. The
artwork now retains its original proportions in the shorter home cards.


Evidence is kept under `Builds/MenuRefinementQA/` and
`Builds/StorybookMenuQA/MenuRefinementFinal/`.
The review gallery is `Builds/MenuRefinementQA/review.html`. Build output is ignored. The shared
working tree contains earlier audio, movement and networking changes; this
release includes that snapshot, rather than isolating the menu change.

The superseded initial menu player was archived with the repository archive
tool to `Legacy/20261018-194010-228-menu-refinement-initial-player/`.
Its initial validation logs and captures remain available as diagnostic evidence.
The combined delivery is coordinated by the concurrent Huawei Scan Kit task;
its release record will supply Android bytes and the final player promotion.

## Handoff limits

The initial combined `AndroidSubmission` attempt failed at
`:launcher:minifyReleaseWithR8` on Huawei optional networking classes. The Scan
Kit task repaired the scoped shrinking rules and the full rebuild succeeded at
**19:49:58 Malaysia time**. The original diagnostic is preserved as
`Builds/MenuRefinementQA/failed-android-build.log`.

The combined APK is **90,543,496 bytes**, up **2,960,308** from the opening
**87,583,188-byte** build, with **909,456,504 bytes** of headroom below the strict
1,000,000,000-byte limit. ZIP integrity and independent v2 signature verification
passed. This includes the Huawei SDK as well as menu changes; see
[the measured size audit](APK-SIZE-AUDIT.md) and
[Scan Kit verification](HUAWEI-SCAN-KIT.md). Physical Android validation remains
outstanding.

The complete tested combined Windows player was promoted to
`Builds/WindowsFinal/` at approximately **19:45 Malaysia time** after the
running game released its files. All **326 player files** (excluding the
release-note text) match the tested `Builds/ScanKitQA/Player/` by SHA-256;
see `Builds/MenuRefinementQA/final-player-integrity.json`.
The prior final player was archived by the Huawei task in
`Legacy/20261018-194458-193-scan-kit-windows-delivery/`.

No Git staging, commits or publishing were performed by this task.
