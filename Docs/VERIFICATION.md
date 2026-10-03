# Verification records

## Transparent lagoon and three fish sizes — 3 October 2026

Implemented transparent moving lagoon water, a sandy basin with animated caustics, and one gold-and-teal fish in **0.4 / 0.9 / 1.8 m** variants. All sizes share the same meshes/material; the two mobile LODs contain **3,192 / 1,248 triangles**, with the **12,792-triangle editable master** retained outside Unity. Casting, catching, swimming AI and scoring are deferred. [Design and rebuild instructions](FISHING-PRESENTATION.md).

The final Windows player passes **30 rendered presentation checks**: fish lengths, shared resources and vertex colours, supported shaders, retained five fishing stands, no new colliders, one active island, three gameplay camera poses, fish visibility through water, motion at a fixed camera, and ocean-cutout cleanup/restoration across island reload. Pier, underwater, overhead, shoreline/inlet, actual game-camera and size-lineup renders were inspected. Fish sit beside the first pier; railings can occlude individual fish from some camera angles. The first-person pier view clearly shows the size gaps. Current evidence: `Builds/FishingWaterQA/Review-Delivery/`, `lagoon-final-preview.mp4`, and `Player/WhatTheFish.exe`. Raw video frames were archived after encoding the final preview.

The older fishing-route probe was corrected to wait for streaming completion, count active islands rather than retained inactive scene entries, and reacquire the lagoon after reload. It passes **50 assertions with 3 failures**: `SPAWN_FLOOR_3`, `SPAWN_FLOOR_4`, and `FULL_COASTAL_LOOP_AND_BRIDGE`. An explicit `-WithoutPresentation` control run produces the same failures and stuck coordinates **(35.87, 1.33, 2.87)**. These existing route/collision issues remain unresolved; no assertion or shoreline collider was weakened to hide them. Both reports are retained in `Builds/FishingWaterQA/RouteChecks/`.

Blender regeneration also passed from a copied checkout path containing spaces, invoked from an unrelated system temporary working directory. Reopening the master and parsing FBX dependency elements found no external image, linked-library or active file dependencies. Generator outputs, `.meta` files and integrity hashes are retained. Saved-file/exporter provenance is nonfunctional metadata; binary bytes were not blindly replaced. New Markdown links resolve within the repository. The final fishing runtime/editor/shader/mesh/material/prefab inputs match the isolated build; source hashes are in `Builds/FishingWaterQA/final-source-hashes.json`.

The review script also repeated **all 30 checks successfully** from a copied checkout path containing spaces, invoked from an unrelated system temporary directory with both player/output defaults. Its report/log are in `Builds/FishingWaterQA/PortableVerification/`. Eight new relative documentation links and both authoring/delivery integrity hashes pass. `Check-TaskReady.ps1 -RequireApk` passes, including portable text paths, metadata, LFS and publishable-file hygiene; it inspected the retained concurrent root APK separately from this task's measured package.

The first main-project Android build failed amid concurrent football source edits. Frozen-checkout AndroidSubmission, strict budget enforcement and v2 signature verification then passed: **86,757,976 bytes**, **13,242,024 bytes** hard-limit headroom. The task APK/audit is retained separately; the concurrently produced root APK was preserved. The **75 MB development target remains unmet**. See [exact baseline/delta, SHA-256 and build scope](BUILD-SIZE.md#transparent-lagoon-and-three-fish-sizes--3-october-2026). This does not validate later football edits. Physical-phone appearance, touch, sustained FPS and thermals remain unverified.

Reviewed superseded previews, intermediate packages/audits and completed portability fixtures were archived with hashes/recovery paths in `Legacy/20261003-230218-819-fishing-presentation-final-cleanup/` and `Legacy/20261003-230828-261-fishing-portable-validation/`. Earlier task batches are `Legacy/20261003-221450-901-fishing-water-baseline/`, `Legacy/20261003-223827-819-fishing-water-failed-audit/`, `Legacy/20261003-224342-327-fishing-water-shared-sand/`, and `Legacy/20261003-225548-346-fishing-water-sand-final/`. Current evidence, active art and Unity caches remain. No staging, commit, push or history rewrite was performed.

## Position-aware goal net — 3 October 2026

Both goals deform at the authoritative ball's actual contact location, using incoming direction and normal impact speed. Back, sides and sloping roof share a continuous response with fixed frame/ground attachments. Scoring keeps the approved nominal 0.65-second ball follow-through before kickoff; a golden goal decides the result immediately and then freezes at the net. See [implementation, limits and reproduction](FOOTBALL-PROTOTYPE.md#position-aware-goal-net--3-october-2026).

The final source passes **1,558 assertions** across two rendered net runs and the rendered football suite:

- **513 net checks**, repeated as **513 copied-path checks**: 224 sampled panel locations across both goals, 12 real PhysX shot cases, varied speeds/directions, high/low impacts, roof/back continuity, pinned supports, bounded overlapping waves, rest/separating rejection, deduplication, invalid event rejection, goal movement and reactivation cleanup. Six 36-frame physical-shot sequences were rendered and visually inspected; `Builds/FootballNetQA/goal-net-impact.gif` and `.mp4` show the localized deformation.
- **532 football checks**: offline matches **259**, match host **16**, match guest **15**, physics/possession/fake shots **199**, network ball host **26**, network ball guest **17**. These verify net contact before reset, paused match clock, no duplicate scoring/new kicks, immediate golden-goal result, post-impact freeze, host/guest impact agreement and rejection of guest-authored collisions, plus retained gameplay coverage.

Initial collision traces exposed an inverted collider-relative velocity vector; the callback now converts it to the ball's incoming direction. Actual contact positions/directions pass after correction. Development-only fixture repairs read the displayed cancel-rectangle position after layout, wait for streamed sport activation rather than a fixed one-second delay, and allow numeric tolerance for the kickoff deadline. A headless shader-property run is not rendering qualification; final gameplay checks ran with graphics enabled. These fixture edits do not change the release game. The seven relevant runtime/material/shader files match the inputs verified before the Android build; hashes and comparison are retained in the task evidence.

Current net evidence: `Builds/FootballNetQA/Run-20261003-223537/` and `PortableVerification/Net/`. Complete final gameplay/loopback evidence: `Builds/FootballNetQA/PortableVerification/Match/` (original run `Run-20261003-225034`). The repeat used a copied checkout path containing spaces, invoked from an unrelated system working directory. The current review player is `Builds/FootballNetQA/Player/WhatTheFish.exe`; `ProjectBuilder.BuildFootballNetReview` rebuilds it with fast development compression and the configured delivery render pipeline. A concurrent shared Windows build supplied the earlier desktop preview; later dedicated builds verified the final fixtures.

Main-project AndroidSubmission and APK v2 signature verification passed: **86,574,958 → 87,028,060 bytes**, leaving **12,971,940 bytes** below the hard limit. The 75 MB development target remains unmet. The exact tested release is retained at `Builds/FootballNetQA/Android/WhatTheFish-release.apk`, with matching logs, baseline, populated packing audit and ZIP/asset comparison. See [size provenance](BUILD-SIZE.md#position-aware-goal-net--3-october-2026). No phone was connected; physical-device rendering/FPS/touch and WAN latency remain unverified. This is visual net deformation over existing rigid collision surfaces, not soft-body ball/net physics.

Superseded task players, failed preview iterations and completed portable fixtures were archived with hashes/recovery paths in `Legacy/20261003-223749-588-football-net-iterations/` and `Legacy/20261003-225549-095-football-net-validation-cleanup/`. Current evidence, editable source and Unity caches remain in place. No Git staging, commit, push or history rewrite was performed by this task.

## Football character motion — 3 October 2026

Implemented the [complete current-control animation set](FOOTBALL-ANIMATION.md): 26 saved control-space takes, existing run/jump/slide clips, bounded limb IK, actual shoe-to-ball placement, directional fall selection and synchronized charge/kick/cancel presentation. Protocol **18** requires matching players. Movement, possession, kick release, tackle restrictions and hit/miss deadlines retain their authoritative rules.

The Windows development builds pass **1,807 assertions** across the following suites. This total includes repetitions at the four timeline rates; it is not a count of distinct scenarios.

| Suite | Passing assertions |
| --- | ---: |
| Football motion at 20/30/60/120 timeline FPS, including both LODs | 748 (187 each) |
| Football match, physics and host/guest ball/presentation replication | 530 |
| Shared jumping, offline and host/guest | 140 |
| Shared tackling, offline and host/guest, including fall variants | 210 |
| Rapid turning, offline and host/guest | 62 |
| Idle visibility, interruptions and host/guest playback | 61 |
| Basketball motion regression | 56 |

Every saved take is sampled at 31 normalized times on both LODs. Maximum measured limb-length drift is 0.001 mm; maximum reach clamping is about 2 cm. Intended shoe-to-ball-surface contact errors peak at **3.43 cm across rates** (30-rate control/fast/cut: **2.2/3.1/1.8 cm**). Maximum sampled skinned-surface floor penetration is **1.3 cm**. These slightly exceed the original design's 3 cm/1 cm targets and are disclosed rather than relabelled as exact contact. The large shoes, short legs and large head retain their original proportions. Front/side pose sheets and ordered transition frames were inspected; the actual-player video includes normal and half-speed playback.

The ten-athlete pose-layer workload preserves rig lengths and reports **zero steady per-frame managed allocations** at all four rates. Its maximum summed pose cost was **0.507 ms** in the 30-rate Windows capture, with **3.127 ms** the highest maximum among the four runs. This measures only the pose layer, excluding Animator, rendering and much of gameplay; it is not phone FPS or a whole-match performance qualification.

Current motion evidence is in `Builds/FootballMotionQA/Run-20261003-223534-834/` (video, sheets, PNG frames, metrics and results), `Run-20261003-223955-854/` (20), `Run-20261003-224057-732/` (120) and `PortableVerification/` (60). The copied test script resolved its default player/output paths from a `Portable Checkout` directory containing spaces while invoked from an unrelated system temporary directory. The fixture was archived after keeping its reports. `test-summary.json` records all regression report paths. Football match/ball evidence: `Builds/FootballMatchQA/Run-20261003-224838/`.

Earlier attempts found and corrected a scaled-mesh measurement error, real fall ground clipping, excessive get-up reach and pose-layer allocations. An idle regression ran into an obstruction in the final enumerated fishing venue; its locomotion phase now runs on the open football pitch, and all 61 checks pass. A rendered goal-reset assertion exposed deadline rounding; diagnostics were retained and the concurrent net review adds a small comparison tolerance. The unchanged runtime match passed both headless and rendered suites. These fixes do not relax animation input timing or gameplay rules.

The prescribed AndroidSubmission build, measured bytes, signature and packing evidence are recorded in [BUILD-SIZE.md](BUILD-SIZE.md#football-character-motion--3-october-2026) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md#football-character-motion--3-october-2026). Current task-specific APK/audit copies are retained under `Builds/FootballMotionQA/` to preserve provenance when other chats rebuild the shared output. The review code is excluded from release. No source Blender/FBX, binary art dependency, package or character texture was changed for this animation pass. Physical-phone touch, rendering, sustained performance, thermals and WAN behavior remain unverified.

Superseded fitting runs and the completed portable fixture were archived in `Legacy/20261003-225412-407-football-motion-cleanup/`. The first successful APK was superseded by a build using a concurrent material-sharing correction and is recoverable in `Legacy/20261003-225615-892-football-motion-first-apk/`. Superseded baseline/audit copies are in `Legacy/20261003-230020-132-football-motion-release-cleanup/`; their bytes and hashes remain in the current task report. No active art, Unity cache or installed dependency was archived. Staging, commit and push remain with the user.

## Interactive football boundary and twin-rail aim — 3 October 2026

Inspected/fetched `origin/main` at `0d314fd`; the checkout already contained the latest merge. The existing authored pitch containment and goal openings drive the new visual boundary. Ball physics, possession, kicks, match rules and networking remain unchanged. [Implementation and tuning](FOOTBALL-PROTOTYPE.md#animated-boundary-and-charging-feedback--3-october-2026).

The rebuilt Windows player passes **559 assertions**: 241 offline match checks, 28 host/guest match checks, 199 football physics/input checks, 41 host/guest ball checks, 25 focused feedback checks and 25 copied-player portability repeats. The focused checks cover both open goal mouths, exact boundary alignment, absence of new colliders, a real free-ball impact ripple, no repeated ripple from a resting ball, both compiled shaders, monotonic charge growth, maximum charge hold, all three camera modes, cancellation, release and mesh reuse. The last shader-only refinement turns the interior chevrons toward the arrow tip; the final capture run repeats all 25 feedback checks.

Close wall, impact sequence, distant pitch, goal-opening, charge sequence and first/third-person/overhead camera renders were inspected. The final shader build log contains no shader compilation errors or warnings. Camera captures render actual gameplay camera poses into a render target; they do not include the screen-space HUD. The review harness uses the same completion-marker/process-lifetime convention as the other repository suites. Earlier forced-exit review experiments encountered a native shutdown error after completing their assertions; those experimental captures/logs are archived and are not a graceful-exit qualification.

Current evidence: `Builds/FootballFeedbackQA/Run-20261003-214141/`, `build-final-windows.log`, `football-feedback.mp4`, `football-feedback.gif` and `PortableVerification/`; gameplay/loopback evidence: `Builds/FootballMatchQA/Run-20261003-213934/`. A copied player and copied test script ran successfully from a `Portable Checkout` path containing spaces, invoked from an unrelated system working directory with the default player/output resolution. The fixture was archived after retaining its reports.

No source model, Blender image/library link or FBX dependency changed. New delivery assets are text shaders/materials with relative Unity GUID references; generated geometry is runtime-only. The review implementation is excluded from release. Windows renders do not qualify physical-phone touch, Android appearance/FPS, thermals or WAN latency. APK bytes and audit results are recorded in [BUILD-SIZE.md](BUILD-SIZE.md) and [APK-SIZE-AUDIT.md](APK-SIZE-AUDIT.md).

Reviewed superseded outputs are recoverable in these ignored archive batches: `Legacy/20261003-212342-227-football-feedback-preview/`, `Legacy/20261003-212431-424-football-feedback-baseline/`, `Legacy/20261003-213433-994-football-feedback-design-revision/`, `Legacy/20261003-213735-250-football-feedback-shader-check/` and `Legacy/20261003-214348-184-football-feedback-final-cleanup/`. No active art, Unity cache or installed dependency was archived. Git staging/commit/push remain with the user.

## GitHub integration release — 3 October 2026

The merge combines local football revision `571cacc` with upstream basketball revision `c9810c4`. Football match/team state, possession, fake-shot cancellation, charging aim, tackle contact and goal-net collision are retained. Basketball dribble/shoot/pass poses, reliable pass requests and shared jump momentum are retained. Protocol **17** consistently covers both replicated layouts in the runtime service and Bootstrap scene. The obsolete football `blockedPusher` state and `Push` collision path were not reintroduced.

Unity 6000.3.20f1 rebuilt the Windows development player in the ignored isolated checkout. All **112 C# input hashes** match the workspace, apart from the separately documented editor build option used for Android clean-cache reporting. The combined player passed **1,061 assertions, with no failures or runtime exceptions**:

| Suite | Passing assertions |
| --- | ---: |
| Football match, physics and host/client ball replication | 509 |
| Basketball motion at 30 FPS, including both character LODs | 56 |
| Basketball two-player gameplay and passing | 30 |
| Shared jump/momentum, offline and host/client | 140 |
| Shared tackle, offline and host/client | 208 |
| Shared turning, offline and host/client | 62 |
| Basketball script from a path with spaces and an unrelated temporary working directory | 56 |

Test summaries, input manifests and build diagnostics are in `Builds/GitMergeQA/`. Football evidence is in `Builds/FootballMatchQA/Run-20261003-150002/`; the other suite paths are recorded in `test-summary.csv`. Selected basketball front/side frames for both LODs and the football restart view were inspected. Synthetic gameplay/control checks and Windows captures do not qualify physical-phone touch, rendering/FPS or WAN latency.

The prescribed main-project AndroidSubmission attempt exited while the user's editor was running. The same script/target succeeded in the isolated project with clean-cache packing evidence and a v2-signed **86,563,288-byte** release APK. Its source manifest remains identical to the tested player. Size, headroom, growth analysis and archive batches are in [the current release record](BUILD-SIZE.md#github-integration-release--3-october-2026). Git LFS integrity and indexed portable-path checks passed; final repository readiness is checked before the user-authorized commit and push.

## Football legacy-state cleanup — 3 October 2026

The unused `blockedPusher` field, assignments/resets and fixed-update clearing branch are removed. A full runtime C# compilation passes with only the existing review-script camera-name warnings. Active source/scripts contain no remaining references. An exact comparison against the pre-task source confirms that only this unused state and its cleanup are deleted: interception, kick eligibility, waking the ball, tuning, fake shots and `mustApproach` retain their existing code. No new gameplay or phone test suite is claimed for this cleanup; the gameplay snapshot below predates it.

The prescribed AndroidSubmission build succeeds. Its incremental packing report has no asset offsets, so a clean-cache build in the existing ignored isolated checkout supplies complete current packing evidence. Runtime source hashes match the workspace; only the previously documented isolated editor output/clean-cache options differ. Logs, the source manifest, narrow source diff, ZIP deltas and signature result are in `Builds/FootballLegacyCleanupQA/`. See [the measured release record](BUILD-SIZE.md#football-legacy-state-cleanup--3-october-2026).

The pre-task package/audit/source backup and superseded incremental output are recoverable from `Legacy/20261003-143827-669-football-legacy-state/`.

## Goal nets block athletes — 3 October 2026

The current-source Unity 6000.3.20f1 Windows player passes **509 assertions**: **199** football physics/control checks, **241** offline match checks, **28** host/client match checks and **41** host/client ball checks. This includes **57 new offline net assertions** covering both goals' four solid world-layer panels, walking/sprinting/sliding against left/right/back nets from inside and outside, open-mouth entry/exit and jumps into the sloping roof. Existing tests still verify football entry/retention/return through both goals, all four unladen pitch exits, attached possession, kick/slide physics, visible restart reset, charge cancellation, goal rules, selection, overtime and match results. Host tests additionally simulate a remote athlete sprinting into all six side/back faces and both peers verify its replicated stop pose. Actual current running/kick measurements remain **4.000/7.000 m/s unladen walk/run**, **5.250 m/s controlled run**, **12.003/22.001 m/s light/full kicks** and **8.000 m/s slide ball speed**.

Evidence: `Builds/FootballGoalNetQA/` contains build logs, physics/ball reports, signatures, source/asset hash manifests, ZIP deltas and a summary. Match reports are in `Builds/FootballMatchQA/Run-20261003-130119/`. An independent ignored checkout with a path containing spaces supplied the verification player and clean-cache AndroidSubmission build. Runtime source hashes still match the workspace; the only editor-source exception is its isolated output/clean-cache build options. Two ignored Performance Test resource metadata GUIDs are regenerated by Unity; publishable assets otherwise match. No Computer Use, Git staging or publishing was used. Windows/loopback checks do not qualify phone touch/FPS or WAN behavior. The current APK and recovery batch are recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

## Football fake shot — 3 October 2026

The rebuilt Windows player passes **142 offline football assertions** and **31 host/client loopback assertions**. An additional **19 gesture checks** compile the actual `KickButton` with the extracted `PlayerView` charge methods against Unity API stubs. Current evidence and the source hash manifest are in `Builds/FootballFakeShotQA/`. The isolated build uses the current runtime source, including the rounded arrow and 1.8x charge scaling; the earlier snapshot scope below remains historical.

The real Unity integration probe injects pointer events into Kick and the left joystick. It verifies same-finger cancellation, exclusion of other fingers, immediate aim hiding, retained controller/kinematic possession without releasing the ball, turning/dribbling after cancellation, no delayed kick on release, no restart by sliding back, a fresh press, normal light/full release commands and cancellation when the release lands inside the cancel region. Existing physics checks still measure approximately **12.003 m/s light**, **22.001 m/s full** and **8.000 m/s tackle**. No ball force, speed, charge curve or physics setting changed for this feature. Loopback checks cover existing authoritative ball behavior; physical phone multi-touch, phone quality/FPS and WAN remain unverified.

The original `-nographics` offline run stopped at the existing shader-material assertion. A run with Direct3D12 passed white/50% alpha, transparent surface, depth-write disabled and queue 3000, along with all gameplay assertions. The offline test runner now retains its graphics device in batch mode; network-only probes still use `-nographics`. The final scripted offline run passed. Rendered captures are also kept in the task evidence folder.

The main project was open in Unity, so builds used the existing ignored isolated project without closing the user's editor. Source hashes match the workspace. Superseded isolated APK/audit output was archived in `Legacy/20261003-002925-125-fake-shot-isolated-baseline/`; the failed Null-graphics evidence is recoverable from `Legacy/20261003-003427-835-fake-shot-null-graphics/`. See [the build-size record](BUILD-SIZE.md) for the Android outcome.

## Team A lettering — 3 October 2026

The top football Team A label now uses overlapping vector capitals with an orange gradient, coral outline and red marks matching the supplied sketch. Unity 6000.3.20f1 compiled the updated source and rendered the real Canvas at 1600 x 900 and 844 x 390, including team selection and enlarged details. The inspected previews preserve A counters, left-to-right overlap, clear score separation and the existing controls. Rendering ran from the ignored isolated checkout whose path contains spaces, with outputs resolved from `Application.dataPath` rather than the caller's working directory. The user's open editor was left running.

Evidence: `Builds/FootballTeamAWordmarkQA/Visuals/`, `wordmark-capture.log` and `validated-inputs.csv`. This UI change adds no gameplay logic or imported art. Actual Android packaging is recorded in [BUILD-SIZE.md](BUILD-SIZE.md); Windows editor renders do not validate physical-phone touch, rendering/FPS or the other working-tree gameplay changes. Existing gameplay reports below retain their original source scope.

The required main AndroidSubmission script attempt exited on the open-project lock. A concurrent successful isolated AndroidSubmission build included all final UI inputs (110 matching C# hashes); its 86,527,020-byte APK and complete audit were retained with explicit external provenance under `ExternalBuild/`. The newer current goal-net release is 86,530,508 bytes; its HUD/wordmark hashes match the reviewed UI and its APK v2 signature was independently rechecked. Measurement, signature and contribution comparison evidence is in `Builds/FootballTeamAWordmarkQA/verification.json`, `package-comparison.json`, `apk-entry-deltas.csv`, `asset-estimate-deltas.csv` and both signature logs. These measurements include other working-tree changes.

First-pass previews were archived with hashes and recovery paths to `Legacy/20261003-003046-063-football-team-a-first-review/`; the transient failed editor-startup log is in `Legacy/20261003-003137-731-football-team-a-capture-startup/`. Final previews, measured artifacts and current evidence remain under Builds. No staging, commit, push or history change was performed.

## Attached football possession — 2 October 2026

**Snapshot scope:** this record validates attached possession and the transparent 0.8–1.44 m outline aim. Other tasks subsequently changed the filled arrow and kickoff formation while these checks/builds ran. Those edits are retained, but are not included in this tested player/APK; full current-working-tree compilation, runtime checks and Android freshness are unverified by this record. `Builds/FootballControlQA/verified-runtime-source-manifest.csv` records the actual isolated source that produced the artifacts. The measured APK is [documented separately](BUILD-SIZE.md#validated-attached-control-snapshot--2-october-2026).

The rebuilt Windows player passed **323 match/physics/loopback assertions** after the latest transparent charge-aim/1.44 m length updates. Its unchanged shared movement source also passed **292 regression assertions (615 total, no failures)**. Current football evidence: `Builds/FootballMatchQA/Run-20261002-234406/` and `Builds/FootballControlQA/suite-current.txt`; shared movement evidence: `Builds/FootballControlQA/MovementVerified/`. The preceding 321-check run is retained under `Builds/FootballMatchQA/Run-20261002-232617/`. Unity 6000.3.20f1 built this combined working tree in the ignored `Builds/FootballControlQA/Isolated Workspace/` project, whose path contains spaces, without operating or closing the user's editor. No Computer Use was used.

Possession checks cover exclusive kinematic attachment, unchanged unladen speeds, the user-approved **25%** possession slowdown, no compounded penalty, blocked jump/slide requests, moving and stationary 180°/90°/270° turns, separate charging speed, light/full kick release, spatial re-approach, immediate reception by another eligible athlete, successful versus missed/ball-only tackles, duplicate contact suppression, field limits, reset, despawn and travel. Loopback host/client checks verify ownership, exact replicated foot anchoring after a 180° turn, actual tackle release, both goal areas and shared scene return. Only the host simulates free-ball physics.

| Measured or runtime-checked quantity | Result |
| --- | ---: |
| Unladen walk / sprint | 4.000 / 7.000 m/s |
| Attached walk / sprint | 3.000 / 5.250 m/s |
| Charging motor speed | at most 2.410 m/s; target 2.400 |
| Light / full release speed, including 0.25 m/s lift | 12.003 / 22.001 m/s |
| Tackled holder's ball horizontal release / lift | 8.000 / 4.000 m/s |
| Sampled maximum attached presentation gap | 0.0000 m |
| Client settled 180° turn foot gap | 0.0000 m |

The shared regression suite passed **34 jump, 202 tackle and 56 turning checks**, including all four environments, both character LODs, the full 0.8-second knockdown lock, mutual slide knockdown, airborne evasion, 40 simultaneous path victims without duplicate hits, and missed-slide recovery at 30/60/120 Hz. Both active athlete prefabs explicitly serialize the 4-second cooldown and 0.8-second recovery. The recovery tests check actual sprint travel near 2.8 m/s, followed by normal speed restoration.

Actual Windows RenderTexture captures of acquired/turned/kicked possession, charging aim and both goal restarts were inspected; the ball stays visible at restart and held at the feet after reversal. The first diagnostic runs exposed fixture issues: capture frames had already advanced a kicked ball outside its exclusion area, and a destroyed test wall persisted until frame end. These fixtures were corrected before the successful run. Physical Android touch, device rendering/FPS and WAN latency remain unverified. Fresh Android packaging evidence is recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

Football kickoff, full-ball goal detection, score, restart, regulation deadlines and golden-goal overtime are described in [FOOTBALL-MATCH.md](FOOTBALL-MATCH.md). The preceding flow revision passed 294 rebuilt Windows offline/loopback/physics assertions and 40,011 pure-rules assertions: capacity-limited 10-second team selection, seamless overtime with live actions and ball momentum, and movement after the immutable final result. HUD selection/full-team, regulation, goal flash, overtime and finish captures were inspected. Physical Android and WAN checks remain pending.

The centre-spot football integration is recorded in [SOCCER-BALL.md](SOCCER-BALL.md), with scene placement, model/material audits and build status. Balanced dribbling, light/charged kicking, limited slide contacts, physical rolling and host-authoritative control/ball synchronization are described in [FOOTBALL-PROTOTYPE.md](FOOTBALL-PROTOTYPE.md).

The current golf milestone is documented in [GOLF-VERIFICATION.md](GOLF-VERIFICATION.md), including updated Windows/Android artifacts, room tests and remaining device checks. The preceding basketball milestone is recorded in [BASKETBALL-VERIFICATION.md](BASKETBALL-VERIFICATION.md).

The records below are the **earlier football baseline from 22 September 2026**. Its APK size/hash and statements that basketball is unavailable describe that earlier build, not the current version.

## Completed

- Android ARM64 IL2CPP development APK built successfully: **52,083,383 bytes (52.1 MB / 49.7 MiB)**, below the 100 MB file target. APK metadata confirmed minimum API 26 and target API 36. Android `apksigner verify` passed with a v2 signature. This is a historical build/package check, not a physical-device launch test; the current package ID and APK are documented in [BUILD-SIZE.md](BUILD-SIZE.md).
- APK SHA-256: `CC512A1431171CD4965C9EAFCD66BD8A315F87BAC0F3405F487D80F77C77F04E`.
- New general-audience Unity cloud project linked in saved project settings. Real anonymous authentication, session creation, session-code joining and Relay transport passed with two Windows processes. Movement, readiness, character appearances and changed stadium settings replicated; host start and return reached the guest. Evidence: `Builds/cloud-host2.txt`, `Builds/cloud-guest2.txt`.
- The reported camera conflict came from the diagnostic `-viewScreen` code overriding the view each frame. That override was removed. The rebuilt interactive player visibly switched from first-person to third-person using the Camera button and displayed 60 FPS on this laptop. Phone drag/multitouch remains unverified.
- Real cloud capacity/isolation test: Room A held ten players and rejected an eleventh with `lobby is full`; Room B simultaneously held two players with independent phase and movement. Joining Room B during exploration returned `lobby is locked`. A stale code returned `lobby not found`. Host departure in the two-client cloud test produced `connected=False`, `exploring=False` and the friendly connection-closed message. Evidence: `Builds/CloudQA-20260922-225455/` and `Builds/cloud-guest2.txt`. Reproduce using `Tools/Build/Test-CloudRooms.ps1`.

- Unity 6000.3.20f1 project imports and C# compilation pass. URP 17.3.0, Multiplayer Services 2.3.3 and NGO 2.13.3 compile together against the installed editor.
- Original stadium and athlete export from Blender 5.1.2. Stadium inspected in a Blender render from elevated and pitch-level views. Both assets imported into Unity and used in the Windows player.
- Stadium FBX reduced from approximately 12.1 MB to 1.93 MB by simplifying repeated seat geometry. Final editable sources remain outside Unity. No spectator NPCs or crowd audio added.
- Windows development player builds and launches. Visual inspection performed on the actual native game window: main menu, character customiser, third-person and elevated stadium views. All three camera positions also exercised by the offline probe.
- The football baseline Windows development build output was approximately 170.1 MB uncompressed. This includes the Windows engine/runtime and is not an Android APK size estimate.
- Offline movement probe moved the avatar 4.8 m; no internet/UGS initialisation was needed. Chibi Idle/Walk/Run clips imported and the runtime Animator evaluated. The on-screen FPS indicator displayed 30 FPS on this laptop; this is **not** a phone performance measurement.
- Character colour selections made in the customiser persisted across player launches. The changeable hair tuft and skin/hair/outfit preset controls are implemented.
- Two independent NGO/Transport loopback rooms ran simultaneously on ports 7777 and 7778. Room A contained ten players, Room B two. Their player counts and transforms remained separate.
- Room A rejected an eleventh process with `This room is full (10 players).`
- Server-owned spawn positions were distinct. Movement was simulated by the host and matched across client logs; character appearance IDs and ready state replicated.
- Host start changed every client's exploration phase. Host return changed every client back to the waiting phase. A later join attempt during exploration was rejected.
- Host process exit disconnected guests and returned the application from exploration to menu state (`connected=False`, `exploring=False`). A disconnect reason was available to the UI.
- A separate two-process test replicated a changed host stadium preset (`TEST 7780`, lilac palette, design 2, team welcome, flags off) to the guest during exploration. Evidence: `Builds/preset-host.txt` and `Builds/preset-client.txt`.

Evidence: `Builds/NetworkQA/*.txt` and `*.log`, `Builds/smoke.txt`, `Builds/scene-audit.txt`, build logs and Blender review PNGs. These generated evidence files are excluded from Git. The reproducible local room test is `Tools/Build/Test-LocalRooms.ps1`. Development probes run only with explicit `-probe` arguments.

## Still awaiting access / not passed

| Requirement | Status |
|---|---|
| Android development APK | Passed build, signature and package checks; physical launch pending. |
| Physical Android launch, touch/multitouch, camera clipping | Not tested on a phone. `adb devices` returned no device. Desktop checks do not prove these. |
| Sustained 30 FPS on Android and APK below 100 MB | APK size passes at 52.1 MB. Sustained Android FPS and thermal behaviour require a phone. |
| Internet sessions across different networks | Real cloud/Relay tests passed on this PC's connection. Two separate physical networks still need testing. A separate named development environment remains pending dashboard access; builds currently use the new prototype project's default environment. |
| Invalid cloud codes, cloud-full room errors, network outages | Stale-code, full-room, locked-room and host-disconnect paths passed via live services. Broader network outage/recovery testing remains pending. |
| Ten users through Relay and simultaneous cloud sessions | Passed using independent Windows processes on this PC. Different physical devices/networks remain pending. |
| Stadium preset replication over internet/Relay | Passed with two Windows processes using real Relay; separate physical networks remain pending. |
| Broad device/aspect-ratio coverage | Pending. UI uses landscape scaling and safe-area anchors; test phones/tablets before publishing. |
| Huawei achievements/results | Explicit placeholder only. HMS SDK registration/integration is a later task. |

## Known prototype limits

- No football ball, scoring, timers, bots, spectator population, basketball arena or golf island.
- No host migration, client prediction, reconnect/resume or dedicated servers.
- One static full-size stadium. Seats are consolidated by material, so editing individual seats requires Blender editing/regeneration rather than runtime object manipulation.
- Stadium signs use runtime world-space UI. Font legibility and face orientation should be reviewed at close range on the phone before release.
- The startup art uses solid colours, not image textures. It does not depend on an external asset pack.
- Native desktop drag automation delivered pointer-down but no subsequent drag events in the touch-pad trace. Actual multitouch dragging still needs testing on the phone; this check has not been counted as passed.
- Scene rebuild and art regeneration intentionally replace generated outputs; save manual variants separately.
