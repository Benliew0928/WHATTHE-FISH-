# Remaining UI design audit

23 October 2026. Source inspected at `a653bd1`, with unrelated local prop-focus
work present. This is an inventory and design assessment, not an implementation
or a new player validation. The subsequent implementation is documented in
[Shared menu design](MENU-DESIGN.md).

## Why the team pages were missed

The previous [control-art pass](CONTROL-ART.md) covered 24 editable gameplay
controls and their Layout previews. It explicitly excluded other menus and match
UI. Football and Basketball team selection use separate button factories and
never call the shared `ControlDesigns` factory.

The visible team names are now **Blue / Red**; internal object names still include
Team A / Team B. Both solo AI selection (choose a side, then Start match) and
multiplayer selection (capacity, full state and automatic assignment countdown)
need the same redesign.

The main mismatch is the combination of flat cream/pastel surfaces, heavy dark
outlines and simple offset shadows beside the glossy illustrated controls.
Changing the button face alone will leave the surrounding dialog inconsistent.
Procedural drawing itself is not a quality defect: the newer navy menus also
use procedural surfaces, with more consistent spacing and hierarchy.

## Remaining redesign inventory

Each row is a screen family; repeated sport variants and dynamic instances are
listed together. Priority 1 means the clearest visible mismatch with the approved
control artwork. Priority 2 means supporting controls in the same older family.

| Priority | Screen / entry | Missed elements | Current source |
| --- | --- | --- | --- |
| 1 | Football: choose team after starting a match or choosing an AI match | Whole dialog, Blue / Red buttons, selection feedback, capacity/full state, countdown, solo Start match | [FootballMatchHUD.cs](../Game/Assets/_Game/Shared/UI/FootballMatchHUD.cs) |
| 1 | Basketball: choose team after starting a match or choosing an AI match | Whole dialog, Blue / Red buttons, selected state, occupancy, countdown, solo Start match | [BasketballMatchHUD.cs](../Game/Assets/_Game/Shared/UI/BasketballMatchHUD.cs) |
| 1 | Football and Basketball: pre-match / between-match HUD | Start / New match, 2v2 through 5v5 size choices, Fill / Remove bots | Same two match HUD files |
| 1 | Fishing: Choose 1 Skill | Outer modal, three selectable cards, names/descriptions, picked/waiting feedback; all six skill variants. Illustrations are already shared with the new gameplay button, but card frames are not | [FishingAbilityHUD.cs](../Game/Assets/_Game/Shared/UI/FishingAbilityHUD.cs) |
| 1 | All four sports: in-game Settings | Gear button, dialog frame, camera tiles, selection ticks, navigation, guide/Layout/back/close actions, audio sliders | [BasketballMenus.cs](../Game/Assets/_Game/Shared/UI/BasketballMenus.cs), [FootballMenus.cs](../Game/Assets/_Game/Shared/UI/FootballMenus.cs), [IslandSettingsMenu.cs](../Game/Assets/_Game/Shared/UI/IslandSettingsMenu.cs), [BasketballAudioSettings.cs](../Game/Assets/_Game/Shared/UI/BasketballAudioSettings.cs), [FootballAudioSettings.cs](../Game/Assets/_Game/Shared/UI/FootballAudioSettings.cs) |
| 1 | All four sports: leave confirmation from in-game Settings | Stay / Leave, close action and confirmation card | Same three menu files |
| 2 | Basketball and Football: in-game guides | Rules / Controls / Props or Stamina / Abilities tabs, back and close actions, content frame | BasketballMenus.cs and FootballMenus.cs |
| 2 | Golf: in-game guide | Help panel, Back to settings and close | IslandSettingsMenu.cs |
| 2 | Fishing: illustrated help sequence | Outer card, Skip / Done, Back / Next and final action. Preserve the tutorial illustrations | [FishingTutorialUI.cs](../Game/Assets/_Game/Shared/UI/FishingTutorialUI.cs) |
| 2 | Basketball and Football: ball effects / customization | Trail-choice cards, selected state and footer actions; Basketball's picker also calls the Football button helper | [BasketballCustomization.cs](../Game/Assets/_Game/Shared/UI/BasketballCustomization.cs), [BasketballTrailPicker.cs](../Game/Assets/_Game/Shared/UI/BasketballTrailPicker.cs), [FootballCustomization.cs](../Game/Assets/_Game/Shared/UI/FootballCustomization.cs), [FootballTrailPicker.cs](../Game/Assets/_Game/Shared/UI/FootballTrailPicker.cs) |
| 2 | Basketball and Football: awards / results | Show results, Box score / Match stats, Next adventure. Preserve the separate awards artwork and celebration | [BasketballMVP.cs](../Game/Assets/_Game/Shared/UI/BasketballMVP.cs), [BasketballMVPCelebration.cs](../Game/Assets/_Game/Shared/UI/BasketballMVPCelebration.cs), [FootballMVP.cs](../Game/Assets/_Game/Shared/UI/FootballMVP.cs), [FootballMVPCelebration.cs](../Game/Assets/_Game/Shared/UI/FootballMVPCelebration.cs) |
| 2 | Fishing: match setup and help access | START and small question-mark help button; leaderboard/clock surfaces also retain a separate presentation | [FishingHUD.cs](../Game/Assets/_Game/Shared/UI/FishingHUD.cs) |
| 2 | Fishing: completed round / ceremony | RESULTS reopen, close icon, CLOSE and Show results; results card uses another panel renderer | [FishingResultsUI.cs](../Game/Assets/_Game/Shared/UI/FishingResultsUI.cs), [FishingCeremony.cs](../Game/Assets/_Game/Shared/UI/FishingCeremony.cs) |
| 2 | All four sports: Layout editor tools, in-game and main menu | Cancel X, Reset, Save, toolbar-position arrow, control picker/list, size slider and resize handle. Actual editable gameplay artwork is already shared | [ControlLayoutEditor.cs](../Game/Assets/_Game/Shared/UI/ControlLayoutEditor.cs) |
| 2 | All four sports: interactive tutorial overlay | Quit, Retry, Skip, Lessons, lesson-selection rows, Back and Quit tutorial | [TutorialView.cs](../Game/Assets/_Game/Shared/UI/TutorialView.cs) |
| 2 | Spectator mode | Camera, Return / Leave room and host Start match are built by the basic AppRoot button helper; scoreboard uses a different, newer surface | [AppRoot.cs](../Game/Assets/_Game/Shared/UI/AppRoot.cs), [SpectatorHUD.cs](../Game/Assets/_Game/Shared/UI/SpectatorHUD.cs) |

The scoreboard/clock/banners adjacent to these controls should be reviewed as
part of each screen composition. They are display surfaces rather than missing
clickable buttons. Team selection deserves a full dialog treatment with clearly
distinct blue/red identities, readable selected/full states and one primary
confirmation action. The skill draft deserves matching illustrated card frames
with enough space for the descriptions.

## Other screens inspected: separate systems, not automatically low quality

These were also outside the gameplay-control pass. Their use of a different
factory does not by itself justify replacing their current designs.

| Screen family | Controls covered by inspection | Assessment / source |
| --- | --- | --- |
| Main menu and sport pages | Sport choices, Play/Explore/tutorial routes, profile/friends/settings, Back | Navy menu family in [StorybookMenu.Sporty.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.Sporty.cs); retain as a reference or integrate its tokens deliberately |
| AI setup | Team size, Choose team, match length, island opponent counts and Fishing mode | [StorybookMenu.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.cs); this navy setup page is distinct from the older in-game team dialog |
| Rooms and lobby | Create/join, code/paste/scan QR, spectators, ready/travel, location picker, QR dialog, leave confirmation | [StorybookMenu.Lobby.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.Lobby.cs), [StorybookMenu.RoomQr.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.RoomQr.cs), StorybookMenu.cs |
| Menu settings, conditions and Layout chooser | Camera, motion/vibration, duration, reset/save/done, sport chooser | StorybookMenu.cs and [StorybookMenu.Layout.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.Layout.cs); editor toolbar is separately flagged above |
| Venue customization and tutorial hub | Palette/logo/equipment/sign/flags, preview views, reset/save, tutorial replay | [StorybookMenu.Venues.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.Venues.cs), [StorybookMenu.Tutorials.cs](../Game/Assets/_Game/Shared/UI/StorybookMenu.Tutorials.cs) |
| Profile, friends and accounts | Avatar/photo/profile actions, friend requests/search/pagination/block/report, provider choices, cancellation/sign-out | [PlayerHubStyle.cs](../Game/Assets/_Game/Shared/UI/PlayerHubStyle.cs), [SocialPanel.cs](../Game/Assets/_Game/Shared/UI/SocialPanel.cs), [AccountPanel.cs](../Game/Assets/_Game/Shared/UI/AccountPanel.cs); newer navy family |
| Chat and stickers | Open/close/options, send, sticker choices, history paging, mute and bubble preferences | [RoomChatUI.cs](../Game/Assets/_Game/Shared/UI/RoomChatUI.cs); newer player-hub buttons with local colour/font overrides |
| Next adventure, travel and recovery | Play again, sport cards, station/game destination, Take everyone, Keep exploring, Details, cancel/retry/dismiss, recovery Leave room | [WorldUI.cs](../Game/Assets/_Game/Shared/UI/WorldUI.cs), [PostMatchFlow.cs](../Game/Assets/_Game/Shared/UI/PostMatchFlow.cs), [RoomTeleportStatus.cs](../Game/Assets/_Game/Shared/UI/RoomTeleportStatus.cs), [RoomRecoveryUI.cs](../Game/Assets/_Game/Shared/UI/RoomRecoveryUI.cs); newer dark world-facing family |
| Welcome and loading | Illustrated sign-in/guest/utility buttons; guest-name modal, retry, dismiss | [StartupWelcome.cs](../Game/Assets/_Game/Shared/UI/StartupWelcome.cs); independent illustrated/gradient design, not the flat court-button family |
| Golf leaderboard | Expand/collapse, title hit region, outside dismissal, scrolling | [GolfLeaderboardUI.cs](../Game/Assets/_Game/Shared/UI/GolfLeaderboardUI.cs); uses dedicated illustrated leaderboard assets; transparent hit regions are intentional |
| Player identity badges | Clickable portraits/identity presentation | [CharacterIdentityBadges.cs](../Game/Assets/_Game/Shared/UI/CharacterIdentityBadges.cs); do not turn portrait hit regions into generic action buttons |

## Avoid counting hidden or already corrected controls

- Football wall-placement Cancel starts through the old helper, but immediately
  calls `ControlDesigns.SkinWallCancel`; it already received the art update.
- Basketball/Football abilities and Fishing's equipped skill use ControlDesigns.
  Fishing's **selection cards** are the remaining exception.
- Golf/Fishing's former standalone camera and return controls are explicitly
  hidden. Their active settings/leave routes are included above.
- PostMatchFlow still creates a Fishing ready button but explicitly hides it.
  Current FishingHUD creates START and help; the old Round/Cup/Crew/Ready controls
  visible in earlier captures are not current in-game setup buttons.
- `GameButtonStyle` is not a universal theme hook. Its fallback call in AppRoot
  does not apply to the four named sports. Restyling that class alone would miss
  the active team/menu controls.

## Shared-design recommendation

Extend the shared design with reusable **action button, choice card, dialog,
icon button, tab and slider** presentations. Team cards need blue/red variants;
skill cards need the existing illustrations; normal/pressed/selected/disabled/full
states must remain legible. Use the approved glossy rims and accents where they
fit the screen, while retaining live text and fixed touch targets.

Route the old Basketball/Football helpers, island menus, Fishing-specific
helpers and Layout toolbar through that layer. Preserve gameplay/network action
handlers. Do not globally replace BasketballPlate: it also renders scoreboards,
meters, dividers and other non-button graphics, and callers directly access its
colour/radius/edge fields. A migration needs to handle those dependencies.

The newer StorybookMenu, PlayerHubStyle and WorldUI families should share tokens
and components where appropriate. They should not receive an automatic blanket
art replacement simply because they are outside ControlDesigns.

## Evidence and limits

The audit traced runtime UI constructors and callers, including Button/CoveButton,
the named style wrappers, native button components, tutorial controls and spectator
setup. Source inspection found no additional button constructors in the sports,
player or world folders outside the shared UI layer (review/editor fixtures are
excluded from the player inventory).

Existing captures were visually inspected:

- `Builds/FishingGameplayQA/Run-20261023-180100/offline.round-skill-choice.png`
- `Builds/FishingGameplayQA/Run-20261023-180100/offline.sports-settings.png`
- `Builds/RoomMatchQA/Run-20261014-182638/Basketball-offline/selection.png`

These are historical local evidence, not newly captured frames of the current
player. The older Basketball screenshot says Team A/B; current source says
Blue/Red. The Fishing capture predates the teammate's removal of in-game Ready
and mode controls. The retained button/card construction was checked against
current source. Screens marked from source alone still need live visual review
before implementation, including rare online, spectator and failure states.

No gameplay, assets or build settings changed in this audit; no Unity rebuild
or APK freshness claim is made. Existing unrelated work remains untouched.
