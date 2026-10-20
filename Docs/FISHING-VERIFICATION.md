# Fishing verification records

## Published skill-card text cleanup - 20 October 2026

Removed the phase labels and shared-choice subtitle from the three-card menu.
Icons, skill names, effect descriptions and pick/countdown controls remain.
The rendered screenshot was reviewed; landscape layouts, ability buttons,
cooldown behavior, catching, controls and camera checks passed.

| Fresh review | Passes | Local evidence |
| --- | ---: | --- |
| rules and guarded Windows refresh | 2084 | `Builds/FishingPublish/Task-20261020-232412/windows-refresh.log` |
| rendered offline gameplay | 2962 | `Builds/FishingGameplayQA/Run-20261020-232929/offline.txt` |

Screenshot: `Builds/FishingGameplayQA/Run-20261020-232929/offline.round-skill-choice.png`.
Complete WindowsFinal promotion matched all **327 files** before amending its note.
Scene/catalog/bundle hashes remained unchanged; all **507 C# source hashes** match
the fresh APK. AndroidSubmission: **93,903,454 bytes**, **906,096,546 bytes**
below the strict limit. ZIP, packaged catalog/hash and APK v2 signature passed.
See [current size record](BUILD-SIZE.md) and [audit](APK-SIZE-AUDIT.md).

The opening fetch found no newer remote commits beyond `ca8dc57`; this publication
includes the approved round-skills changes below plus the final text cleanup.
Protocol remains **73**. Earlier broader loopback/audio/travel validation is
recorded below. Physical-phone touch/FPS and real WAN sessions remain unverified.
Current evidence and source/promotion hashes: ignored `Builds/FishingPublish/Task-20261020-232412/`.
Superseded player/APK archive: `Legacy/20261020-233141-488-fishing-skill-card-publish/`.

## Shared round skills and Island View - 20 October 2026

Source is `ca8dc57` plus the uncommitted Fishing changes, protocol **73**.
The same three cards are offered to everyone; each player selects one reusable
skill. There are no bag/treasure controls. A contextual button shows readiness,
active duration and cooldown. The monochrome helper shares the avatar meshes
and live pose without adding gameplay actors. Arrival selects Follow; manual
Island View stays behind the player. Eye-level controls and the accepted hold,
rope and swimming behavior remain.

**5,762 checks passed:**

| Review | Passes | Local evidence |
| --- | ---: | --- |
| rules | 2084 | `Builds/FishingDraft/Task-20261020-175936/windows-refresh-3.log` |
| rendered gameplay | 2961 | `Builds/FishingGameplayQA/Run-20261020-222615/offline.txt` |
| camera and results | 192 | `Builds/FishingPolishQA/Run-20261020-222820/polish.txt` |
| host | 47 | `Builds/FishingGameplayQA/Run-20261020-222853/host.txt` |
| guest | 49 | `Builds/FishingGameplayQA/Run-20261020-222853/client.txt` |
| audio | 184 | `Builds/GolfFishingAudioQA/Fishing-20261020-223036-570/results.txt` |
| travel Host | 117 | `Builds/TravelMatchQA/Run-20261020-223102-465/Host/results.txt` |
| travel Player | 64 | `Builds/TravelMatchQA/Run-20261020-223102-465/Player/results.txt` |
| travel Spectator | 64 | `Builds/TravelMatchQA/Run-20261020-223102-465/Spectator/results.txt` |

The complete Windows candidate was promoted to `Builds/WindowsFinal/` with
all **327 files** SHA-256 matched before amending `LATEST-BUILD.txt`.
The last code-only refresh guarded the native scene, catalog and islands-bundle
hashes. All **507 C# source hashes** remain identical through the Android build.
Rendered reviews cover skill choice at several landscape sizes, all six skill
buttons/effects, independent held input, cooldown/phase visibility, all piers and
camera modes, real catches, UI feedback and results. Loopback peers cover shared
selection, validated skill requests, scoring and rematches. Three peers cover
player/spectator placement, draft/countdown travel restrictions and room travel.
The aiming fixture explicitly re-aims through empty water when a neighbouring
fish is held by the existing selection hysteresis; production aiming is retained.

Fresh APK: **93,899,262 bytes**, with **906,100,738 bytes**
under the strict decimal-GB limit. ZIP CRC, five-scene catalog/hash and independent
v2 signature verification passed. See [size record](BUILD-SIZE.md) and
[packing audit](APK-SIZE-AUDIT.md). Debug-certificate signing does not establish
store-release signing. No physical phone touch/visual/FPS or real WAN session
was performed.

Evidence index: `Builds/FishingDraft/Task-20261020-175936/validation-summary.json`,
`release-sources.json`, `windows-promotion.json`, `apk-after.json`,
`FinalAndroidAudit/`, `portable-output.txt`, `portable-links.json` and
`import-restoration.json`. The shared C# path resolver was checked with an output
path containing spaces and an unrelated working directory; 63 documentation
links resolved in a sparse clone with spaces. Import cleanup compared the Golf
prefab by its full named component graph and retained external references, and
proved the character library change was only an importer hash.

Reviewed superseded candidates/deliveries and the completed path fixture were
archived, with manifests, in ignored:

- `Legacy/20261020-184854-436-fishing-draft-review/`
- `Legacy/20261020-223551-295-fishing-round-skills-delivery/`
- `Legacy/20261020-225313-393-fishing-skills-portability-fixture/`

This record describes validation before the publication refresh above.

## Eye-level camera, natural swimming and rope cue — 19 October 2026

The current delivery is described in [Fishing camera and swimming](FISHING-NATURAL-SWIM.md).
The complete `Builds/WindowsFinal/` player contains 325 runtime/support files
plus `LATEST-BUILD.txt`. The code-only refresh retained identical native scene,
catalog and islands-bundle hashes from the prior complete validated player.

**4,930 checks pass** across the current feature reviews:

| Review | Passes | Local evidence |
| --- | ---: | --- |
| Fishing rules and guarded Windows build | 1,891 | `Builds/FishingNatural/Task-20261019-052154Z/windows-final-build.log` |
| Rendered gameplay and all 30 natural fish paths | 2,752 | `Builds/FishingGameplayQA/Run-20261019-134457/offline.txt` |
| Camera, rope cue, feedback and results | 192 | `Builds/FishingPolishQA/Run-20261019-134933/polish.txt` |
| Fishing host | 46 | `Builds/FishingGameplayQA/Run-20261019-135002/host.txt` |
| Fishing guest | 49 | `Builds/FishingGameplayQA/Run-20261019-135002/client.txt` |

Eye-level checks press both directions and inject manual look input; the camera
stays at eye position. Screenshots at 20%, 60% and 90% tension confirm the rod,
line, curved-to-taut cue and warning are visible. Captures wait until after the
rod's LateUpdate pose; an earlier capture rendered before the head was hidden.
Follow/Island fighting views, cancel return and reduced motion also pass.

Swimming checks sample three minutes of each fish's route, verify finite bounded
speed and acceleration, broad pool coverage and changes of angular direction,
and confirm that sampling order cannot change the shared path. Real rendered
casts/catches, held-label layout, local host/guest claims, abilities, results and
rematches pass. The rendered gameplay report precedes only a development capture
timing correction; the final Windows scripts passed the complete polish and
network suites afterward.

Evidence index and source hashes are in
`Builds/FishingNatural/Task-20261019-052154Z/`. The release includes existing
uncommitted shared-lagoon work above pulled `main` at `9bb55646`; protocol is 70.
For measured Android output, see [Build size](BUILD-SIZE.md) and
[APK audit](APK-SIZE-AUDIT.md). Physical Android and WAN behavior remain unverified.

Older release records follow.

## Stronger Fishing motion and upstream Golf integration — 13 October 2026

Complete fresh Windows player delivered to `Builds/WindowsFinal/`:
**321 runtime/support hashes** match the tested candidate,
plus `LATEST-BUILD.txt` (**322 files**).
Full content completed UTC **2026-10-13T11:32:40.8512971Z**; this is a complete
content build, with all teammate scene changes included.

Hooked fish have stronger thrust and surface surges, with spring force, drag,
buoyancy and gravity retained. Smooth turns allow up to 200 degrees/second;
rendered travel retains its 12 m/s cap. Free fish swim continuous 3.3 × 4.2 m
loops inside each pier's casting range. Surface spray is taller and thicker;
wakes expand farther. The shared-material droplet pool remains capped at 48.
No new Fishing image, model, shader, audio or package is introduced. Earlier
catch assistance, countersteering, six skills/two favourites, tutorial, scoring
and round rules remain intact.

Merged upstream `9b4eb15`, including Golf countdown graphics, cart controls,
longer swing and integrated scenes. Incoming shared/network code was preserved;
the merge had no conflicts. Four islands, shared travel and protocol **49** remain.

**3,916 assertions pass in ten fully passing reports.**
The separate cart reports contain 209 passes and 12 known failures:

| Review | Passes | Known failures | Evidence |
| --- | ---: | ---: | --- |
| Fishing gameplay | 2232 | 0 | `Builds/FishingGameplayQA/Run-20261013-193335/offline.txt` |
| Fishing host | 45 | 0 | `Builds/FishingGameplayQA/Run-20261013-193505/host.txt` |
| Fishing guest | 46 | 0 | `Builds/FishingGameplayQA/Run-20261013-193505/client.txt` |
| Fishing results/feedback | 146 | 0 | `Builds/FishingPolishQA/Run-20261013-193543/polish.txt` |
| Basketball UI | 434 | 0 | `Builds/BasketballUIQA/Run-20261013-193555-638/results.txt` |
| Football UI | 756 | 0 | `Builds/FootballUIQA/Run-20261013-193618-344/results.txt` |
| Golf offline match | 158 | 0 | `Builds/GolfMiniGameQA/Run-20261013-193638/offline.txt` |
| Golf host | 21 | 0 | `Builds/GolfMiniGameQA/Run-20261013-193638/host.txt` |
| Golf guest | 22 | 0 | `Builds/GolfMiniGameQA/Run-20261013-193638/client.txt` |
| Golf HUD | 56 | 0 | `Builds/GolfHUDQA/Run-20261013-193748/golf-hud.txt` |
| Golf cart offline | 163 | 8 | `Builds/GolfCartQA/Run-20261013-193800/offline.txt` |
| Golf cart host | 23 | 2 | `Builds/GolfCartQA/Run-20261013-193800/host.txt` |
| Golf cart guest | 23 | 2 | `Builds/GolfCartQA/Run-20261013-193800/client.txt` |

All 30 schools are checked across a complete swim cycle for expanded movement,
cast reach and lagoon containment. Existing force fixtures still verify finite,
bounded inertial movement with rising/falling motion. Actual catches verify
speed/turn caps, attached line endpoints, real large-fish water contact,
bounded droplets and visible gravitational arcs. Rendered splash, aiming,
catch/held-caption, skill/tutorial and Golf HUD captures were inspected.
No runtime exceptions were found in the reports. Source hashes did not change
in runtime C# between Windows QA and Android; build-time Golf prefab ID drift
was audited through all14 objects and external GUID references, then restored
to the exact incoming merged index text. Two URP runtime resource registrations
differ by platform; reconstructing those two lines matches the exact tested
Windows bytes, while Android's complete settings graph equals the incoming
index. Snapshot/audit details stay in `Builds/FishingSurge/Task-20261013-190655/`.

Fresh AndroidSubmission written **19:47:57 Malaysia time** measures
**90,281,708 → 90,388,492 bytes (+106,784)**.
Headroom to the strict 100,000,000-byte boundary: **9,611,508 bytes**.
The **75,000,000-byte development target remains exceeded by 15,388,492 bytes**.
The enforced hard limit and independent APK v2 signature checks pass. SHA-256:
**18CC2A2F7E2354ADACAF585FCF7DCDD8F5257A61CC1EAE4D0557CDE91A1EE6EB**.

[Packing comparison](APK-SIZE-AUDIT.md#stronger-fishing-motion-and-upstream-golf-integration--13-october-2026). The previous player was replaced
in place under the user's no-backup preference; only its metadata inventory is
retained. No new archive batch is created. User explicitly requested publishing
this task to `main`.

Physical-phone touch, appearance and sustained FPS, real WAN conditions and
celebrations remain unverified or deferred. The separate Golf cart suite still
has the **12 documented toe-direction failures** around 64 degrees; its other
209 checks and all three completion/exit markers pass. This existing limitation
is recorded in [the Golf cart notes](VERIFICATION.md#golf-cart-english-button-labels--12-october-2026).
Fishing splashes use a stylized force model, not a fluid solver.

## Fishing six skills, familiar tutorial and water contact — 13 October 2026

Delivered the complete player to `Builds/WindowsFinal/`: **321 runtime/support
hashes** match the tested candidate, plus `LATEST-BUILD.txt` (**322 files**).
The preceding player was replaced in place under the user's no-backup request;
a metadata-only inventory records its hashes. No new archive batch is created.
Source, build, comparison and promotion evidence stays under ignored
`Builds/FishingLearn/Task-20261013-160232/`.

**3,629 passing checks across six reports:**

| Review | Passing checks | Evidence |
| --- | ---: | --- |
| Fishing rendered gameplay | 2,202 | `Builds/FishingGameplayQA/Run-20261013-172747/offline.txt` |
| Local host | 45 | `Builds/FishingGameplayQA/Run-20261013-172944/host.txt` |
| Local guest | 46 | `Builds/FishingGameplayQA/Run-20261013-172944/client.txt` |
| Results, Crew and feedback | 146 | `Builds/FishingPolishQA/Run-20261013-173556/polish.txt` |
| Basketball UI | 434 | `Builds/BasketballUIQA/Run-20261013-173706-859/results.txt` |
| Football UI | 756 | `Builds/FootballUIQA/Run-20261013-173730-867/results.txt` |

Checks cover first-entry tutorial/Skip/Next/Back, all four pages at landscape
phone/tablet/desktop sizes, native text bounds, live icon/meter reuse, shared
How to Play content and unobstructed sport tabs. Gear tests cover the six-item
roster, exactly two favourites, only two acquisition choices, held-item phase
rules, nearby shared Sonar, expiry, single-consumption, reservations and NGO
wire round trips. Real host/guest kit, supply and use RPCs pass. Direction
captions remain contained during real press feedback and release.

All 120 fish/range fixtures land correctly; continuous large-fish winding still
snaps, and no-input fights still time out. Representative small catches save
0.3–0.7 seconds, medium 0.6–1.4, large 1.2–2.5. Live rendered large-fish catches
emit surface-contact splashes. Droplets inherit motion, follow a gravitational
arc, disappear at water contact/expiry and stay in a 48-slot pool. Mesh bounds,
rather than the submerged model pivot, determine surface contact. Initial
positions are set immediately. Fish turn/speed caps, rod grip and line endpoint
remain verified. Existing Round/Cup/Crew/Wanted scoring and shared travel remain.

Full Windows content completed **16:49:51**, final script refresh **17:35:40**
Malaysia. Fishing gameplay/network ran on scripts 6; scripts 7 changes only
the development results assertion for the requested shorter Cup warning.
Production gameplay/UI/effect sources are unchanged. Results/feedback and both
sports UI reviews ran on scripts 7. Post-Android comparison confirms **3,691
of 3,692 Game input files unchanged**; only Golf's regenerated Ball IDs differ.
Its **14-object canonical graph** and references were audited equal, then the
reviewed indexed text was restored. Meshes are unchanged. Git is unstaged.

Fresh AndroidSubmission **90,186,840 → 90,281,708 (+94,868)** bytes,
hard-boundary headroom **9,718,292**, 75 MB target overrun **15,281,708**.
Enforced budget, APK v2 signature, mobile pipeline and release-type exclusions
pass. See [measured size](BUILD-SIZE.md) and [packing audit](APK-SIZE-AUDIT.md).

Limits: an earlier local network hook attempt was rejected; the clean final
host/guest run passed. Hook timing/grace rules were not changed. Phone
appearance/touch/FPS, real WAN and new celebrations remain unverified/deferred.
Swimming and splashes are stylized force-driven presentation, not a fluid solver.

## Fishing hybrid gear and smooth force-driven fights — 13 October 2026

The complete updated player is in `Builds/WindowsFinal/`; launch
`Explore-Fishing.cmd`. [Gear rules](FISHING-ABILITIES.md),
[countersteering/layout](FISHING-COUNTERSTEER.md).

| Final-player report | Passing checks |
| --- | ---: |
| Fishing offline rules and rendered UX | 1,981 |
| Fishing local host | 39 |
| Fishing local guest | 41 |
| Fishing results and audio/input feedback | 146 |
| Basketball UI regression | 434 |
| Football UI regression | 756 |
| Total | **3,397** |

All nine items collect and activate under host rules. Tests cover exactly three
valid favourites, kit locking after Ready, shared cooldowns, one-slot inventory,
stale/wrapped item requests, ability expiry, nearby crew recipients, salvage
reservation/timeout/cancellation, no free scores or fish claims, and nondefault
gear/physical-state NGO wire round-trips. The real guest command queue/RPC path
sets favourites, claims an in-range supply, consumes Sonar and receives the
host's deadlines; a stale pickup message is rejected. Existing catches, bonuses,
rank changes and clean rematches pass for both peers.

Rendered checks exercise all nine gear descriptions without clipping, native
selection/save/acquisition/activation/cancellation buttons, caption glyphs inside
the coloured circular faces, the reduced-height timer, landscape layouts and
asymmetric safe-frame insets. The gear regions participate in overlap and
safe-frame checks. Empty gear hides during fights and the chooser hides aim markers.
Fish force integration stays finite and continuous; measured live motion in the
rendered runs peaks around **2.7 m/s** and **140 degrees/s**, with **zero sampled
rod endpoint error** against the corresponding rendered fish pose. This is a
stylized physical model, not a fluid simulation or physical-phone FPS result.

Evidence: `Builds/FishingFlow/Task-20261013-133219/selected-qa.json` and the
referenced six reports/captures. Full Windows content completed **14:00:51**;
final script refresh **14:31:54**. The 3,682-input source snapshot has no runtime
code changes after QA/Android building. The sole generated Golf Ball local-ID
change has an equal **14-object graph**, verified references, and is restored
to the reviewed indexed text. All **321 runtime/support hashes** match the
complete delivered player, plus its current build note.

Fresh AndroidSubmission: **90,166,448 → 90,186,840 (+20,392) bytes**;
hard-boundary headroom **9,813,160**; 75 MB target overrun **15,186,840**.
Independent v2 signature and enforced size checks pass. Matching peers require
**protocol 48**; Football/Basketball gameplay/request payloads are retained.
The complete Windows player was replaced in place per the user's no-backup
preference; a metadata-only old inventory is retained. No new archive batch
or Git publishing action is performed.

Physical phone touch/visuals/FPS, real WAN and new celebrations remain deferred.
Windows/local-loopback evidence does not establish them. Older sections below
record previous deliveries.

## Illustrated Fishing controls and fight motion — 13 October 2026

The illustrated controls, faster/easier tuning and stronger fight presentation
are implemented in the complete `Builds/WindowsFinal/` player. Start
`Explore-Fishing.cmd`. [Behavior, layouts and artwork](FISHING-COUNTERSTEER.md).

| Final-script report | Passing checks |
| --- | ---: |
| Fishing offline authority and rendered UX | 1,085 |
| Fishing local host | 35 |
| Fishing local guest | 35 |
| Fishing results, feedback and input recovery | 146 |
| Basketball UI regression | 434 |
| Football UI regression | 756 |
| **Total** | **2,491** |

Coverage retains all 30 fish at four ranges, correct/wrong steering, releases,
large-fish continuous-hold snapping, timestamp/lease recovery, scoring, Cup/Crew,
real keyboard/pointer catches and multiplayer rematches. New checks cover shared
illustrated left/right artwork with correct mirror direction and native contained
labels, small sprites, stronger torso lean with authored rod contacts, and the
side-by-side navy progress labels. Actual D3D11 captures were inspected at
1600×900, 844×390, 1024×768, 1920×810 and 1280×720, with five rows and asymmetric
safe insets. Medium representative catches finish about 1–1.5 seconds sooner;
large fish still require deliberate steering/release timing. Hand contact stays
within the existing 2 mm review tolerance. No new models/materials are used.

Full content completed **00:56:55 Malaysia**, final scripts **01:05:00**.
All **321 runtime/support hashes** match the promoted complete player; its build
note is the 322nd file. **3,673/3,674 Game inputs** remain byte-identical after
Android. The sole difference is generated Golf Ball local IDs: its 14-object graph
and external targets were checked against the merged index and the exact indexed
text restored. Runtime C# is unchanged after final QA.

Fresh AndroidSubmission **90,166,448 bytes**, **+58,652** from the task's actual
90,107,796-byte baseline, passes the enforced hard limit and independent APK v2
signature. Headroom is **9,833,552 bytes**; the 75 MB target is exceeded by
**15,166,448 bytes**. Imported mesh rows and retained packed binary sizes are
unchanged. [Packing comparison](APK-SIZE-AUDIT.md#illustrated-fishing-controls-and-fight-motion--13-october-2026).

Evidence: `Builds/FishingIllustrated/Task-20261012-234621/selected-qa.json`,
`promotion-integrity.json`, `windows-tested-inputs.json`,
`post-android-source-comparison.json`, `portable-art-check.json`,
`LatestMergedIndexBall/` and `ApkComparison/`. Final captures:
`Builds/FishingGameplayQA/Run-20261013-010506/`. Earlier checks found an exact
binary-boundary fixture issue, excessively quick first easing, an inactive-gauge
observation, and a cubemap import causing missing artwork. These were corrected.
A low-space full build left an incomplete compressed data file despite reporting
success; the launch check rejected it. The completed rebuild and final reports
supersede that output, and the builder now rejects a missing/temporary data file.

Recovery batches created before the user's later no-backup instruction:
`Legacy/20261013-000707-691-fishing-illustrated-old-candidate`,
`20261013-002718-982-fishing-illustrated-preliminary-qa` and
`20261013-004234-742-fishing-illustrated-incomplete-player`.
Reviewed archived players were losslessly compressed with unchanged SHA-256
payloads to provide build space. The user subsequently requested deletion of
obsolete backups; automatic approval review blocked bulk and individual deletion.
No listed backup was deleted. `DELETE-OBSOLETE-PLAYERS.txt` records reviewed
manual-cleanup paths. The complete current player is replaced in place per the
user's no-backup request, with a metadata-only prior inventory; active source,
masters, current APK/player and validation evidence are preserved.

**Limits:** physical-phone visuals/touch/FPS and real WAN remain unqualified;
no new celebrations. Headless local peers do not validate their rendered motion.
No Git staging, commit or push is performed.

## Fishing Hold & Countersteer — 12 October 2026

Selected B is implemented; [controls, authority and editable layout](FISHING-COUNTERSTEER.md).
The complete Windows player is in `Builds/WindowsFinal/`; start with
`Explore-Fishing.cmd`. Matching room peers require **protocol 47**.

| Exact final-candidate report | Passing checks |
| --- | ---: |
| Fishing offline, authority rules and rendered UX | 1,038 |
| Fishing local host | 34 |
| Fishing local guest | 34 |
| Fishing results, feedback, Crew and input recovery | 146 |
| Basketball shared UI regression | 434 |
| Football shared UI regression | 756 |
| **Total** | **2,442** |

Checks cover correct/wrong steering, release recovery, stale/future input,
lease recovery, obsolete one-button rejection, all 30 fish at four ranges,
unchanged scoring/Cup/Crew, native pointer ownership, gameplay catches,
rank/award feedback, fish/rod/hand presentation, bite cues, four landscape
resolutions, five-player rows, asymmetric safe areas, focus/pause/travel,
results and multiplayer rematches. D3D11 captures at 1600×900, 844×390,
1024×768, 1920×810 and 1280×720 were inspected for layout/readability.
Network peers use headless Windows simulation; this does not prove their visuals.

Full serialized content build finished **20:18:48 Malaysia**, final script
refresh **20:27:02**. All 321 promoted runtime/support hashes match the tested
candidate; the 322nd file is the updated build note. **3,665/3,666 Game inputs**
match after Android. The only remaining byte difference from the captured
Windows source is Golf Ball's generated local IDs; its 14-object graph and all
external targets were verified equivalent, then its exact indexed text restored.
No runtime C# changed after these passing reports.

Fresh AndroidSubmission **90,107,796 bytes**, **−31,528** from baseline, passes
the strict limit and independent v2 signature; headroom **9,892,204 bytes**.
The 75 MB target remains exceeded by **15,107,796 bytes**. All imported mesh
rows and every retained packed binary asset size are unchanged.

Current evidence: `Builds/FishingRedesign/Concepts-20261012-194347/selected-qa.json`, `promotion-integrity.json`,
`windows-tested-inputs.json`, `post-android-source-comparison.json`,
`unused-ui-proof.json`, `LatestMergedIndexBall/` and `ApkComparison/`.
Actual captures remain in `Builds/FishingGameplayQA/Run-20261012-202703/`.
A preliminary transient steering assertion failed before being corrected to
compare sustained progress/strain; its superseded run is archived.

Recovery batches are under ignored `Legacy/`: `20261012-201352-453-fishing-countersteer-obsolete-ui`,
`20261012-203456-247-fishing-countersteer-before-android`, and
`20261012-203732-495-fishing-countersteer-windows-delivery`. Editable high-quality
UI masters and the approved B design reference remain in publishable source.
No Git staging/commit/push is performed for this task.

**Limits:** no physical Android device or real-world WAN qualification, no
phone FPS/touch claims, and no new celebration system. Audio/vibration behavior
checks do not replace human listening or tactile review.

The selected golf and fishing refinement is implemented in the Windows player.

- Review: [actual EXE gallery](VisualDirection/GolfFishingRefinement/GameReview/index.html)
- Evidence and scope: [delivery report](VisualDirection/GolfFishingRefinement/GameReview/VALIDATION.md)
- Editable sources: [authoring guide](../ArtSource/RefinedIslands/README.md)
- Superseded version: local archive `Legacy/GolfFishing-G2L2-20260927-032531/` (excluded from Git); [archive and restoration guide](REPOSITORY-HYGIENE.md)
