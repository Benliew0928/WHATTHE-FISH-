# Storybook Cove menu

The selected first direction is implemented as a native Unity UI. The four
illustrated islands and the original WHATTHE FISH? logo lead into private rooms,
offline exploration, conditions, venue customization, settings and controls.
The approved reference boards are in [VisualDirection/Menu/StorybookCove](VisualDirection/Menu/StorybookCove/).

## Interaction

- A menu button compresses while held, brightens on hover and has a gold focus
  ring for keyboard navigation. Release inside activates once; dragging away,
  disabling the control, losing focus or pausing cancels the press. An unrelated
  finger cannot release a held button. Holding a navigation button never repeats.
- Pages crossfade and move into place over 240 ms. Both outgoing and incoming
  pages reject input during that transition. Reduced motion removes scaling,
  floating illustrations and page movement, keeping short fades and colour cues.
- Arrival in the world uses a 320 ms fade and gates gameplay input until it ends.
  Volume sliders animate only their thumb, keeping the drag track stationary.
- Native inputs support typing, selection, paste and the mobile keyboard. Room
  codes normalize case and surrounding spaces. Empty, short, malformed and
  overlong codes cannot start a connection. Failures remain on the form for retry.
- Copy retries briefly and confirms success by reading the OS clipboard back.
  If clipboard access is unavailable, the visible room code and typing path remain
  usable, with a clear message instead of a false success notice.
- Connection requests lock controls until completion. Readiness and player counts
  refresh from live room state. Start becomes available only to the host when
  everyone is ready. Leaving as host explains that the room will close.
- Sliders change real game and menu audio volume. Camera and motion preferences
  persist on the device. Gameplay buttons also receive press/hold/release feedback;
  existing kick, basketball charge, golf swing and joystick input rules remain.

## Rooms and conditions

The host creates an invite-only room. Guests paste or type its code. Capacity is
read from the selected sport definition. The lobby exposes copy-code, ready,
conditions, host venue editing, help and departure. Departure starts exploration;
the existing in-world Start Match controls start formal football or golf matches.
There is one active sport per room, and Sky-Sail travel moves everyone together.

Football can use 3, 5 or 10 regulation minutes, with the existing one-minute
overtime, ten-second team selection and even teams of 2–10 players. A saved host
condition replicates and supplies the actual next-round clock; it resets all
ready flags. Guests can read conditions but cannot change them. When no menu
override has been chosen, the authored rule asset remains authoritative.

Basketball practice scoring, the five-hole golf rules and the exploration-only
fishing island are described accurately without nonfunctional configuration
controls. Golf and fishing venue customization remains unavailable.

## Venue editing

Football supports name, four flag/screen accent palettes, screen message, screen
content and flags. The message selector is available in Venue title screen mode.
Basketball supports name, palettes and four court logos. Edits preview the actual
venue in a single cached 768 × 432 render texture, with overview and detail views.
Save persists and publishes the
host's venue; Back offers to discard an unsaved draft. The preview texture and
camera are released on departure. No gallery or user-image upload is implied.

## Assets and rebuilding

- [Original logo](Branding/WHATTHE-FISH-Logo.png): retained master; the menu copy
  imports at a maximum 512 pixels.
- [Island atlas](../Game/Assets/_Game/Resources/Menu/CoveIslands.png): full-quality
  transparent raster retained in Git LFS. Four runtime sprites share one texture;
  Android uses a 1024-pixel ASTC 6×6 import. No baked text or buttons are in the art.
- The ocean, clouds, route, panels and icons are small native UI meshes.
- [Font provenance](../ArtSource/Menu/README.md) and [font generator](../Tools/Build/Prepare-CoveFonts.py)
  retain the source, OFL license and compact static subsets.

[StorybookMenu.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.cs) owns the screens.
[CoveUI.cs](../Game/Assets/_Game/Shared/UI/CoveUI.cs) supplies visual components and
interaction feedback. [MenuPreferences.cs](../Game/Assets/_Game/Shared/UI/MenuPreferences.cs)
contains saved preferences and the host-rule access policy. No third-party UI or
animation package is required.

Build the Windows review player with Unity's batch execute method
`WhatTheFish.Editor.StorybookMenuBuild.Windows`, then run
`Tools/Build/Test-StorybookMenu.ps1 -PlayerPath <complete-player-exe>`.
The script resolves its root from its own location, and accepts player paths with
spaces. Development probes are omitted from Android release builds. Final build
measurements and validation belong in [BUILD-SIZE](BUILD-SIZE.md),
[APK-SIZE-AUDIT](APK-SIZE-AUDIT.md) and [VERIFICATION](VERIFICATION.md).

## Integration provenance

The teammate's `26a0bf1` and `af02ea7` commits were pulled by fast-forward from
`a98d75f`. Local basketball physics, scoring, free-roam and input work was retained
through a reviewed three-way merge alongside incoming golf clubs, carts, ball
physics and match state. Protocol **24** describes the combined network layout;
all players need the same build. The recovery stash and hashed pre-merge files
are retained locally. No user change was staged, committed or pushed.
