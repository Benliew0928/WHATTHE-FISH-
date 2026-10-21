# Teammate Fishing integration — 21 October 2026

Remote main advanced from `ca8dc57` to `4c3b6ce`. The teammate's shared three-card
round draft, reusable contextual skill, monochrome helper and behind-player
Island View are retained. Existing local profiles, social/account UI, venue
customization, bots, QR improvements and personal layouts are preserved.

## Adaptations

- Fishing's shared island settings retain visit-local camera changes and the
  teammate's Follow-on-arrival behavior. Golf continues saving its preference.
- Personal layouts expose the new round-skill button in live play and main-menu
  previews. Retired supplies/salvage controls are absent. Unchosen skill controls
  use a representative icon and their normal action-adjacent default position.
- Offline bots select one of the shared offered skills during drafting, and the
  existing Cup/rematch loop continues. Crew remains a human cooperative mode.
- Match start/practice guards include both drafting and countdown, preserving
  bot rosters and offline reset behavior. Leave warnings reflect skill choice.
- Skill RPCs preserve ownership checks and independent directional holds; local
  input-blocking guards are retained. Combined protocol is 74 because local
  profile replication and incoming skill state differ from protocol-73 builds.

## Validation and delivery

Full Windows build, followed by validated script refreshes, promoted as a complete
327-file player. All 407 compiled C# document hashes match current source.
Fishing gameplay: 2,960 offline + 47 host + 49 client assertions. Bots: 72;
layout: 234 edit + 3 restart; offline settings: 54. All 2,084 Fishing rule checks passed. Rendered captures cover skill
choice, camera controls and the adapted live layout. Initial failures exposed
camera-persistence incompatibility and a draft-overlay timing assumption; both
were corrected and the affected reviews passed.

The source opening snapshot is retained in an ignored safety stash and evidence;
all 143 originally untracked files remain present. No task source was committed
or pushed. The generated Golf prefab was compared against the opening snapshot
as a full named component graph, and restored to its opening bytes after proving
only local IDs/document order changed. Original local asset edits were preserved.

Fresh APK bytes, SHA-256, packing audit, archive batches and hardware limits are
in [build size](BUILD-SIZE.md) and [APK audit](APK-SIZE-AUDIT.md).
Evidence index: ignored `Builds/TeammateFishingMergeQA/`; runtime reviews in
`Builds/FishingGameplayQA/Run-20261021-000219/`,
`Builds/IslandBotsQA/Run-20261021-000219/`,
`Builds/ControlLayoutQA/Run-20261021-000338-871/` and
`Builds/IslandSettingsQA/Run-20261021-000607-869/`.
