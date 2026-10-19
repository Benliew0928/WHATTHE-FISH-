# Golf and latest main integration - 20 October 2026

Local Golf work was committed as `2bfcc5d` with the short English message:

> Polish Golf controls, multiplayer tees, and UI

Pulled `origin/main` through `99685e2` (following `43eb9d2`). Retained incoming
Fishing controls/shared lagoon, responsive networking, audio, room QR and the
MIT QR encoder, together with Golf UI, celebrations/countdown, personal ranking,
multiplayer tee allocation, T1 terrain restoration and drag/release power.
Idle Golf preview remains 30% until dragging; actual drag power drives the shot.

Only three merge conflicts occurred: the two size-record documents and the
shared protocol constant. Both parents' historical records remain in the size
documents. `PlayerView` merged automatically and retains Fishing ceremony camera
blocking and the Golf controls. `RoomService` matches the incoming implementation
with the combined protocol advanced from local 69 / incoming 71 to **72**,
because the combined build also contains the new Golf tee snapshot fields.
Everyone in a room must use matching protocol-72 builds.

Publishing uses ordinary merge history; no force push or history rewrite.
Current review/build evidence lives under ignored `Builds/GolfPublishQA/`.
Fresh delivery checks:

- Full Windows build passed; 36 focused drag/Swing/Putt checks passed.
- Real protocol-72 local host/client tee checks: 7 host + 7 client passed.
- All 327 candidate files matched the complete WindowsFinal player by SHA-256
  before its build note was amended. All 505 tested C# hashes are unchanged.
- Actual idle-preview screenshot reviewed. No fresh WAN, physical-phone input,
  FPS or scanner-camera test; incoming historical evidence is not a rerun.
- New integration documentation link resolved from a relocated fixture containing
  spaces while run from an unrelated working directory.
- Reviewed ball prefab local-ID regeneration and character cache hash restored
  to pre-build merged bytes; generated copies retained as evidence. Project
  graphics/quality settings match pre-build bytes.
- Prior protocol-69 player/APK and inspected obsolete protocol-67 QA player were
  archived via the repository archive tool to
  `Legacy/20261020-071812-929-golf-main-publish/` with recovery manifests.
- Fresh AndroidSubmission: **93,875,514 bytes**, +3,024,608 from **90,850,906**;
  strict decimal-GB headroom **906,124,486 bytes**. Budget, ZIP integrity and
  independent v2 signature checks passed. Most growth is the incoming Huawei
  native scanner, DEX and model assets; no Golf artwork was added.
- APK SHA-256: `25A3F72A8CF2764BF78DED87E5319D5127BF61EC19C32DB3F0E4538976FF2C2D`.
  Contributor inventory: `Builds/GolfPublishQA/FinalAndroidAudit/`.

Intermediate compilation/candidate-attempt logs remain diagnostic evidence;
current passing logs are `build-windows-pass.log`, `final-drag-runtime.txt` and
`FinalNetwork/{host,client}.txt`. The tested candidate is `MergedPlayer/`, not
the obsolete QA player that was archived.
The earlier protocol-71 integration report remains historical evidence only.
