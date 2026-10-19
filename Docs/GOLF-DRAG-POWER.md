# Golf drag-to-release power control

Replaces the round hold-to-charge action button with a vertical green-to-red
power rail in the lower-right, below Jump. Click/touch the golf ball handle,
pull downward for power, then release to strike. Drag upward to reduce power.
The initial and post-shot handle position is the green top (zero). Before any
drag, the existing preview displays 30% power; touching the handle without
dragging also keeps that preview. During a drag it follows the selected power.
Tapping at zero does not strike. No charge cycling or time-based power growth remains.
The former keyboard F hold does not charge Golf shots.

The same input drives existing Swing and automatic near-hole Putt. Existing
ball physics, force limits, camera controls, aim lock, preview trajectory and
landing marker are preserved. Power is sampled by the existing preview and
passed through the existing owner/round/authority-validated shot command.
No map, tee allocation, obstacle or network serialization changes.

Only the initiating pointer can adjust/release a drag. Pointer capture continues
outside the rail. Cancelling Aim, disabling the control, losing focus or pausing
cancels without firing. A released drag fires once, then resets. The former
secondary charge meter by the character is hidden to avoid duplicate power UI.

Idle chevrons appear in downward order at 0.16-second intervals; each has a
0.5-second sine opacity envelope, repeating on a 1.3-second cycle. They hide
while dragging. They use native UI geometry; no per-frame object creation.

## Source and artwork

- `Game/Assets/_Game/Shared/UI/GolfSwingButton.cs`: drag control and hints.
- `Game/Assets/_Game/Shared/UI/GolfHUD.cs`: layout and instructions.
- `Game/Assets/_Game/Shared/Player/PlayerView.cs`: selected power and release.
- `Game/Assets/_Game/Shared/Player/GolfAimInput.cs`: existing preview uses power.
- [Artwork masters, references and exact prompts](../ArtSource/Golf/UI/DragPower/README.md).

## Verification and delivery

- Windows build passed; 32 focused runtime assertions passed in
  `Builds/GolfDragPowerQA/runtime.txt`. This exercises actual PlayerView input,
  UI pointer handlers, preview, authority stroke count and near-hole Putt.
- Reviewed `swing-zero.png`, `swing-sixty.png`, `putt-eight.png`; rail fits safe
  bounds and separates from other active controls. Existing landing marker and
  trajectory are visible. Mouse/touch pointer ownership is checked by the fixture;
  no physical-phone touch/FPS or new WAN session was tested.
- Complete player promoted to `Builds/WindowsFinal/`; 326 files matched by SHA-256
  before amending `LATEST-BUILD.txt`.
- Fresh official AndroidSubmission: **90,856,642 bytes**, +354,156 from
  **90,502,486**. Remaining strict decimal-GB headroom **909,143,358**. Independent
  v2 signature verified. APK SHA-256:
  `B93CF6D04E7686DB60FEDDB357964134C8625590D95F6DEA99586A86B3D10075`.
- Current audit copied to `Builds/GolfDragPowerQA/FinalAndroidAudit/`. The two new
  sprites account for 756,264 serialized bytes; this is not compressed APK growth.
- Reviewed build-only ball prefab local-ID regeneration and character sourceHash
  changes restored from task-opening bytes; generated versions retained as
  evidence. Project graphics/quality settings match opening bytes.
- Prior complete player and APK archived with recovery manifest under
  `Legacy/20261020-010042-973-golf-drag-power/`. No active source art was archived.
- Player includes the existing uncommitted workspace plus this feature, rather
  than a clean committed snapshot. No staging, commit or push was performed.


Final readiness: `Check-TaskReady.ps1 -RequireApk` passed (7,446 publishable
files); `git diff --check` passed. Final status/diff records are retained in the
ignored evidence folder. Compared against the task-opening diff, no existing
scene, environment prefab or map-source change was introduced by this task.

## Idle preview correction - 20 October 2026

Idle Swing and Putt preview again use 30%, matching the earlier hold-to-charge
presentation. Grabbing the handle without moving it also keeps the 30% preview.
The first drag switches the preview to the actual slider power (including zero
if dragged back to the top). The handle still starts at zero, and a zero-power
tap still does not strike. Preview-only default power is never queued as a shot.

36 focused runtime assertions passed, including four new checks of idle Swing,
idle Putt, holding without dragging and returning after a zero-power tap.
The new screenshot is `Builds/GolfIdlePreviewQA/swing-idle-thirty.png`; other
current evidence is under that same ignored folder. Complete WindowsFinal was
updated and 326 files SHA-256 matched before its note was amended.

The prior complete player and APK are recoverable under
`Legacy/20261020-064558-702-golf-idle-preview/`. No assets or map data were edited.
Fresh AndroidSubmission APK: **90,850,906 bytes**, versus **90,856,642**
previously (-5,736); strict decimal-GB headroom **909,149,094** bytes.
Official build/budget and independent v2 signature passed. SHA-256:
`DBDB87FC93219B0626C21F5D6DEA15F35220EC9FF2C4FB00A73BA8563B112D13`. Current audit is retained under
`Builds/GolfIdlePreviewQA/FinalAndroidAudit/`.
Reviewed build-only ball local-ID and character hash regeneration were restored
from opening bytes; project graphics/quality settings match opening bytes.
No Git staging, commit or push; no physical-phone or WAN verification.
