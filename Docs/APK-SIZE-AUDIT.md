# APK size audit — measured release records

## Fishing skill-card text cleanup and publication - 20 October 2026

Removed the **BEFORE CASTING / WHILE HOOKED** card labels and the shared-choice
subtitle. Skill icons, names, descriptions, selection controls and all gameplay
rules remain. This release includes the approved shared round skills and
player-anchored Island View from the preceding record.

Fresh AndroidSubmission at **23:41:09 Malaysia time**:
actual APK **93,899,262 -> 93,903,454 bytes
(+4,192)**, with **906,096,546 bytes** below the strict
1,000,000,000-byte limit. Build/budget gate, ZIP CRC, five-scene catalog and bundle
hash, and independent APK v2 signature verification passed. SHA-256:
**99B3594EFD68CCDCFD2A3DE5D535C8A8CDAD5A7CA33F4952A01AA52E49B0B0FE**.

Largest compressed entries: islands **45,078,358**, IL2CPP **12,390,337**,
Unity **9,807,453** and metadata **2,765,147** bytes. The bundle increased by
**7,415 bytes** while IL2CPP decreased by **3,066 bytes**; no new art was added.

The complete Windows player has **327 files**, all matched to the tested candidate
before its build note was amended. The code refresh preserved the native scene,
catalog and islands bundle. **2,084 rule checks and 2,962 rendered gameplay checks**
passed; the actual card screenshot confirms the removed text and clear descriptions.
All **507 C# hashes** match through both builds. Protocol remains **73**.
No new artwork was added. Physical-phone touch/FPS and real WAN sessions were
not retested; the preceding record retains broader loopback/audio/travel evidence.

Evidence: ignored `Builds/FishingPublish/Task-20261020-232412/`, including `FinalAndroidAudit/`,
`apk-after.json`, `apk-entry-growth.json`, `apksigner.txt` and `windows-promotion.json`.
Reviewed superseded player/APK: ignored
`Legacy/20261020-233141-488-fishing-skill-card-publish/`.
See [Fishing verification](FISHING-VERIFICATION.md) and [skill rules](FISHING-ABILITIES.md).

## Fishing round skills and Island View - 20 October 2026

Fresh AndroidSubmission at **22:51:18 Malaysia time**: actual local APK
**87,580,516 -> 93,899,262 bytes (+6,318,746)**.
Strict decimal-GB headroom: **906,100,738 bytes**. The official build
and hard-limit gate, ZIP CRC, complete five-scene content catalog and independent
APK v2 signature verification passed. SHA-256:
**478B49AB5B57D85914A434A02D614F0B31E0C7A04139D5A1F9FF8E7D0EC6E084**.

This includes the pulled `ca8dc57` source plus the uncommitted Fishing task:
shared three-card selection, one reusable skill, six skills, a monochrome
render-only helper, contextual cooldown button and manual player-anchored
**Island View**. No new bitmap or character model was added; native vector
icons and a small shader reuse existing art and meshes.

The local before APK predates the incoming Golf/QR work. Largest measured entry
growth includes islands **+1,936,533**, Huawei scanner library **+936,122**,
DEX **+710,914** and scanner models **+1,026,816** bytes. This is not an
asset-only Fishing benchmark. The new APK is **23,748 bytes** larger than the
previous documented protocol-72 measurement below, which is a separate build.
Largest compressed entries are islands **45,070,943**, IL2CPP **12,393,403**,
Unity **9,807,453** and metadata **2,765,303** bytes. Serialized/source sizes
are different measurements.

The complete WindowsFinal player has **327 files**. All files matched the tested
candidate before its note was amended. **5,762 checks** passed; **507 C# source
hashes** match both builds. Protocol **73** requires matching peers. The APK uses
the configured Android Debug certificate; store signing is separate. Physical
phone touch/FPS and real internet sessions remain unverified.
Evidence: ignored `Builds/FishingDraft/Task-20261020-175936/`, including
`FinalAndroidAudit/`, `apk-after.json`, `apksigner.txt` and source/promotion hashes.
See [skill rules](FISHING-ABILITIES.md) and [verification](FISHING-VERIFICATION.md).

## Golf and latest-main protocol-72 merge - 20 October 2026

Fresh AndroidSubmission at **07:21:42 Malaysia time**: actual APK
**90,850,906 -> 93,875,514 bytes (+3,024,608)**.
Strict decimal-GB headroom: **906,124,486 bytes**. Official build/budget
gate, ZIP integrity and independent APK v2 signature passed. SHA-256:
**25A3F72A8CF2764BF78DED87E5319D5127BF61EC19C32DB3F0E4538976FF2C2D**.

Merged local Golf `2bfcc5d` with latest main `99685e2`; combined protocol **72**.
Retained Golf drag/release with 30% idle preview, tees, UI and repaired T1 terrain,
plus incoming Fishing, responsive networking, audio and room QR updates.
Most APK growth comes from the incoming Huawei scan library (**936,122** bytes),
DEX increase (**710,914**), and scanner model assets (**1,026,816**). No new Golf
artwork or unrelated quality reductions. Largest compressed entries: islands
**45,055,447**, IL2CPP **12,389,693**, Unity
**9,807,996**, metadata **2,764,612** bytes.

Complete WindowsFinal updated; **327 files** SHA-256 matched before its note
was amended. **36 drag/Swing/Putt + 7 host + 7 client tee assertions** passed.
All **505 C# source hashes** match the tested build. No new WAN session or
physical-phone touch/FPS/scanner test. Evidence: ignored `Builds/GolfPublishQA/`,
including `FinalAndroidAudit/` and `apk-entry-growth.json`.
Reviewed old deliveries archived in `Legacy/20261020-071812-929-golf-main-publish/`.
See [current integration and publication record](GOLF-MAIN-PUBLISH.md).

## Golf 30% idle preview restoration - 20 October 2026

Fresh AndroidSubmission at **06:48:37 Malaysia time**: actual APK
**90,856,642 -> 90,850,906 bytes (-5,736)**.
Strict decimal-GB headroom: **909,149,094 bytes**. Official build/budget
gate and independent APK v2 signature passed. SHA-256:
**DBDB87FC93219B0626C21F5D6DEA15F35220EC9FF2C4FB00A73BA8563B112D13**.

Restored the default 30% Swing/Putt preview before dragging, including a touch
without movement. Actual drag power and release-to-hit remain unchanged. Existing
artwork, force limits, camera and map are reused. Largest compressed entries:
islands **45,044,192**, IL2CPP **12,340,309**,
Unity **9,773,269**, metadata **2,758,580** bytes.
The small size variation is compiled code/packaging, with no new bitmap assets.

Complete WindowsFinal updated; **326 files** SHA-256 matched before its note
was amended. **36 focused runtime assertions** passed, including default Swing,
default Putt, touch without drag and zero-power tap preview checks. Actual idle
30% screenshot reviewed. No fresh WAN or physical-phone input/FPS verification.
Evidence: ignored `Builds/GolfIdlePreviewQA/`, including `FinalAndroidAudit/`.
Previous complete player/APK archive: `Legacy/20261020-064558-702-golf-idle-preview/`.
See [drag power and idle preview notes](GOLF-DRAG-POWER.md).

## Golf drag-to-release power control - 20 October 2026

Fresh AndroidSubmission at **01:03:37 Malaysia time**: actual APK
**90,502,486 -> 90,856,642 bytes (+354,156)**.
Strict decimal-GB headroom: **909,143,358 bytes**. Official build/budget
gate and independent APK v2 signature passed. SHA-256:
**B93CF6D04E7686DB60FEDDB357964134C8625590D95F6DEA99586A86B3D10075**.

Largest compressed entries: islands **45,052,418**, IL2CPP **12,337,899**, Unity
**9,773,269**, metadata **2,758,497** bytes. Growth is concentrated in the two new
slider textures. Player-only serialized assets **28,024,568 bytes** are a separate
measurement; Rail and Knob account for **756,264 serialized bytes**, not APK bytes.
Editable image masters and references remain outside Unity. Android sprites use
ASTC 4x4; no unrelated asset quality reduction or map generation was performed.

Complete WindowsFinal player rebuilt and promoted; **326 files** SHA-256 matched
before its note was amended. **32 focused runtime assertions** passed; actual
Swing zero/60% and Putt 8% screenshots reviewed. Existing aim/trajectory/landing
marker and shot physics retained. No fresh WAN or physical-phone input/FPS test.
Evidence: ignored `Builds/GolfDragPowerQA/`, including `FinalAndroidAudit/`.
See [drag power implementation and delivery](GOLF-DRAG-POWER.md).

## Golf T1 bunker restoration and seam repair - 20 October 2026

Fresh AndroidSubmission at **00:32:45 Malaysia time**: actual APK
**90,795,846 -> 90,502,486 bytes (-293,360)**.
Strict decimal-GB headroom: **909,497,514 bytes**. Official build/budget
gate and independent APK v2 signature passed. SHA-256:
**48BC2EE82B658B0BB957F4EAB814457E49CE7739F0F0B5BD713765DA3CB68481**.

Largest compressed entries: islands **45,055,476**, IL2CPP **12,338,210**, Unity
**9,773,269**, metadata **2,758,497** bytes. Removed floating T1 floor/path meshes;
joined native terrain and exact surface intersections reuse existing texture maps.
No unrelated visual quality reduction. Source-only annotated PNG references are
outside Unity. Player-only serialized assets remain **27,268,200 bytes**, a
separate measurement from APK bytes.

Complete WindowsFinal player rebuilt, tested and promoted; **326 files** matched
by SHA-256 before its note was amended. **457 editor assertions** and **28 four-player
runtime assertions** passed. Actual top, near and two low entry views reviewed;
visible dressing clearance measured **0.0248-0.0250 m**. Original tee-site records
are unchanged. No new WAN or physical-phone quality/FPS test. Evidence: ignored
`Builds/GolfT1SeamQA/`, including `FinalAndroidAudit/`.
See [localized bunker/seam repair](GOLF-T1-BUNKER-SEAMS.md).

## Golf T1 terrain integration repair - 19 October 2026

Fresh AndroidSubmission at **22:57:57 Malaysia time**: actual APK
**90,295,414 -> 90,795,846 bytes (+500,432)**.
Strict decimal-GB headroom: **909,204,154 bytes**. Official build,
budget gate and independent APK v2 signature passed. SHA-256:
**66FAFBA8DC3D85CF3F326CEF99A3AB1BE754E7F03D229DBCF126295F4BFD9017**.

Largest compressed entries: islands **45,345,669**, IL2CPP **12,341,397**, Unity
**9,773,269**, metadata **2,758,482** bytes. The ~0.50 MB increase is concentrated
in localized ground/dressing meshes and a terrain shader using existing textures.
No new bitmap textures or unrelated quality reductions. Player-only serialized
assets remain **27,268,200 bytes**, a different measurement from the actual APK.

Complete WindowsFinal player rebuilt, tested and promoted; **326 files** matched
by SHA-256 before its delivery note was amended. **399 editor assertions** and
**28 four-player runtime assertions** passed. Original pad records are unchanged
and scene/prefab records agree. Actual desktop top/near views reviewed. No new WAN
or physical-phone quality/FPS test. Evidence: ignored `Builds/GolfT1BlendQA/`,
including `FinalAndroidAudit/`. See [T1 terrain repair](GOLF-T1-TERRAIN-REPAIR.md).

## Golf centre-first multiplayer tees - 19 October 2026

Fresh AndroidSubmission at **20:42:18 Malaysia time**: actual APK
**88,552,098 -> 90,295,414 bytes (+1,743,316)**. Strict decimal-GB headroom:
**909,704,586 bytes**. Official build/budget gate and independent v2 signature passed.
SHA-256: **87BBBEB84D4D3FEDD8C9A57432801A29481020FA21D307336121940535C9DA7D**.

Largest compressed entries: islands **44,845,228**, IL2CPP **12,341,397**, Unity
**9,773,269**, metadata **2,758,482** bytes. Expected growth is concentrated in the
new derived tee ground/scenery meshes and small allocation/snapshot code. Existing
island textures and authoring masters are reused; no quality reduction. Player-only
serialized assets **27,268,200 bytes** are a separate measurement from APK bytes.

Complete Windows player rebuilt and promoted after **399 editor assertions**,
**28 four-player runtime assertions**, and **7 host + 7 client assertions**.
Evidence: ignored `Builds/GolfMultiplayerTeeQA/`, including `AndroidAudit/`.
No physical-phone quality/FPS or WAN test. Protocol **69** requires matching peers.
See [tee layout, allocation and delivery record](GOLF-MULTIPLAYER-TEES.md).

## Golf hidden-before-start and self-only preview - 19 October 2026

Fresh AndroidSubmission at **00:51:28 Malaysia time**: actual APK
**88,550,674 -> 88,552,098 bytes (+1,424)**. Strict decimal-GB headroom:
**911,447,902 bytes**. Build/budget gate and independent v2 signature passed.
SHA-256: **3F5030017C954E89FCC603559CCDC319AF3550024D5484F0BD0A6D378AE5C222**.

Largest compressed entries: islands **43,125,660**, IL2CPP **12,319,897**, Unity
**9,773,269**, metadata **2,756,280** bytes. Tiny code/serialization compression
variation; no new assets or quality reduction. Player-only serialized assets stay
**27,268,112 bytes**, a separate measurement from the actual APK size.

Complete Windows player updated with a code-only script refresh on the prior
validated full player; 326 files hashed before updating its note. **21 focused
presentation checks** and screenshot review passed. Evidence and copied audit:
ignored `Builds/GolfPersonalRankingQA/` and `AndroidAudit/`. No phone FPS/WAN test.
See [personal preview record](GOLF-PERSONAL-RANKING.md).

## Original Golf ranking restored - 19 October 2026

Fresh AndroidSubmission at **00:39:12 Malaysia time**: actual APK
**88,542,330 -> 88,550,674 bytes (+8,344)**. Strict decimal-GB headroom:
**911,449,326 bytes**. Build/budget gate and independent APK v2 signature passed.
SHA-256: **01D4B74BAA73CC9DBB323A0721E50C314D4B34892297549AD9DEE05FBB65E878**.

Reverted the simplified leaderboard designs, including unused portrait delivery
assets. Separate 50-unit countdown, close Aim arrow and tapered trail remain.
Largest compressed entries: islands **43,122,483**, IL2CPP **12,321,500**, Unity
**9,773,269**, metadata **2,756,434** bytes. Small bundle/code serialization and
compression variation; no new textures/models or unrelated quality reduction.
Player-only serialized assets **27,268,112 bytes** are not the compressed APK size.

Full tested Windows player promoted to WindowsFinal; 326 candidate files compared
before amending its note. **39 focused graphical checks** and screenshot review
passed, including synthetic eight-player preview/full scrolling list and retained
visual settings. No remote session or physical-phone FPS tests were performed.
Evidence and audit: ignored `Builds/GolfRankingRollbackQA/` and `AndroidAudit/`.
See [rollback and archives](GOLF-RANKING-ROLLBACK.md).

## Golf compact ranking and trail taper - 18 October 2026

Fresh AndroidSubmission at **23:33:48 Malaysia time**: actual APK
**88,550,326 -> 88,542,330 bytes (-7,996)**. Strict decimal-GB headroom:
**911,457,670 bytes**. Build gate and independent v2 signature verification passed.
SHA-256: **358B883D2286330AA147193AEF336E9619F2FEE9D15E4A51F26B51263BCFAAD8**.

Largest compressed entries: islands bundle **43,115,368**, IL2CPP **12,320,248**,
Unity **9,773,269**, metadata **2,756,450** bytes. No new textures/models or quality
reductions; small bundle/code variation, no unexpected asset growth. Serialized
player assets remain **27,268,112 bytes**, a different measurement from APK bytes.
Complete Windows player delivered after 38 focused presentation checks and
screenshot inspection. Evidence: ignored `Builds/GolfCompactQA/`, audit copied to
`AndroidAudit/`. No phone FPS/WAN testing. See [change record](GOLF-COMPACT-RANKING.md).

## Golf continuous countdown, circular address and water support - 18 October 2026

Final AndroidSubmission succeeded at **22:54:32 Malaysia time, 18 October 2026**.
Actual APK: **88,550,326 bytes**, opening APK **88,540,314**; delta **+10,012**.
Headroom below the strict 1,000,000,000-byte limit: **911,449,674 bytes**.
Independent v2 signature verification passed. SHA-256:
**BDDB599E91780DC6DA2440DAD15B041FEC0DA0D2DB93714EA8813206999F242A**.

| Largest compressed entry | Bytes |
| --- | ---: |
| `assets/PackagedContent/islands.bundle` | 43,121,521 |
| `lib/arm64-v8a/libil2cpp.so` | 12,322,109 |
| `lib/arm64-v8a/libunity.so` | 9,773,269 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,756,428 |

Player-only serialized bytes remain **27,268,112**. The islands bundle increases
5,108 bytes; IL2CPP increases 4,549 and metadata 349. This is expected small
shader/code/bounds growth, with no new bitmap/model assets or unrelated quality
changes. Full current audit is in ignored `Builds/SizeAudit/latest/`, preserved
in `Builds/GolfRefineQA/AndroidAudit/`. Exact measurements and signature output:
`Builds/GolfRefineQA/final-apk.json` and `apk-signature.txt`.

The exact opening APK is archived in batch
`20261018-225557-224-golf-refine-previous-apk`. Complete superseded Windows player:
`20261018-225239-597-golf-aim-countdown-water-player`. Device visual/FPS and WAN
checks are not inferred from the Windows preview. See
[targeted implementation](GOLF-ADDRESS-AND-COUNTDOWN-20261018.md).

## Golf rollback, pond crossing, rim capture and countdown - 18 October 2026

Final `AndroidSubmission` succeeded at **19:11:03 Malaysia time, 18 October 2026**.
Actual APK **88,540,314 bytes**, versus opening **87,606,216** (**+934,098**).
Strict decimal-GB headroom: **911,459,686 bytes**. APK v2 signature passed.
SHA-256: **E2FC748B5AA0F780A37BCA070899C5C6307B7EE8D62DE23C6FA7CF17FACAB254**.

| Largest compressed entry | Bytes |
| --- | ---: |
| `assets/PackagedContent/islands.bundle` | 43,116,413 |
| `lib/arm64-v8a/libil2cpp.so` | 12,317,560 |
| `lib/arm64-v8a/libunity.so` | 9,773,269 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,756,079 |

The islands bundle shrank **69,331 bytes**, consistent with removal of all
21 authored mechanisms. New Resources countdown sprites account for the main
new asset contribution: two 1024-wide ASTC 4x4 plates and eleven small glyphs.
These are intentional quality improvements over an intermediate 256-pixel
preview that was rejected as blurry. No unrelated art reduction was used.
Player-only serialized bytes: **27,268,112**; consult the separate bundle report
for scene assets. Neither this figure nor raw source PNG sizes is APK size.

Current audit: ignored `Builds/SizeAudit/latest/`; preserved task copy:
`Builds/GolfPolishQA/AndroidAudit/`. Independent signature output and entry/hash
measurement: `Builds/GolfPolishQA/apk-signature.txt`, `final-apk.json`.

The superseded exact opening APK and unused generated mechanism sand variant
were archived in `20261018-191208-845-golf-polish-superseded`. Variant references
were checked in scenes, prefabs, materials, metadata, runtime and generators;
the original high-quality sand material remains. Device visual/FPS and WAN
validation are not claimed. See [Golf change record](GOLF-POLISH.md).

## Golf physical moving mechanisms - 18 October 2026

`AndroidSubmission` succeeded at **18 October 2026, 18:10:08 Malaysia time**. Actual APK:
**87,606,216 bytes**; strict 1 GB headroom **912,393,784 bytes**. The actual
opening local APK was **95,067,465 bytes** (-7,461,249), an older snapshot;
packaging/audio/travel changes already in the checkout are included. No claim
is made that this difference is caused solely by the moving mechanisms.
Independent APK v2 signature verification passed. SHA-256:
**F06D7A236E6DC101AE6E1F2F5D6CF831AF69F5342D2C11DBE80C5F04F0C66AD6**.

| Largest entry | Compressed bytes |
| --- | ---: |
| `assets/PackagedContent/islands.bundle` | 43,185,744 |
| `lib/arm64-v8a/libil2cpp.so` | 12,312,472 |
| `lib/arm64-v8a/libunity.so` | 9,773,772 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,755,161 |

The player-only report lists 25,688,264 uncompressed serialized asset bytes;
this is not the APK size and excludes the separate scene bundle. The 21 A-U
mechanisms add scene objects and code, with shared primitive meshes/materials.
No large new texture/model contributors or intentional visual reductions were
introduced. Compared with the previously documented 87,569,184-byte release,
the recorded total is 37,032 bytes larger; the retained byte/entry comparison
is against the actual opening 95,067,465-byte APK.

Current audit: `Builds/SizeAudit/latest/`. Preserved audit, ZIP entry deltas,
actual before/after bytes, signature output, source-retention and Windows copy
hashes: `Builds/GolfMovingQA/`. Windows validation passed 7,464 assertions across
the focused offline/local-host/client runs, including six pond returns.
No physical Android or WAN validation was performed.

An ignored task-local TEMP/TMP directory was used for the build process; no
SDK, system environment or portable source defaults were changed. A preceding
Windows bundle attempt ran out of disk space; the complete final build passed
after space recovered. NTFS lossless compression was requested for local Legacy
output only; see task evidence. The task issued no deletion of other archives.
Archives: `Legacy/20261018-180723-332-golf-moving-player/` and
`Legacy/20261018-181348-956-golf-moving-previous-apk/`.
## Combined latest-main integration — 20 October 2026

`AndroidSubmission` succeeded at **00:10:48 Malaysia time**. Actual APK:
**90,579,220 bytes**, up **35,724** from the opening local **90,543,496**.
Headroom below the strict 1,000,000,000-byte cap: **909,420,780 bytes**.
The budget gate, ZIP integrity and independent APK v2 signature passed.
SHA-256: **506492C679C637F90CB17B974B64F3F407BA255E2A9BD5379DEEB1C14D7AEE75**.

| Largest APK entry | Compressed bytes | Change |
| --- | ---: | ---: |
| `assets/PackagedContent/islands.bundle` | 43,153,964 | +2,486 |
| `lib/arm64-v8a/libil2cpp.so` | 12,351,726 | +30,357 |
| `lib/arm64-v8a/libunity.so` | 9,807,995 | 0 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,761,478 | +2,581 |

The merge retains Fishing updates and local audio, responsiveness, menu and
Huawei scanner features. There was no unexpected large growth or lossy art
change. Player assets serialize to **25,690,212 bytes** and the separate bundle
to **150,702,143**; these are different measurements from the APK length.
The Huawei SDK/registration remains configured from local environment settings.

The complete Windows player passed **4,568 integration assertions**, plus both
protocol-69/71 rejection directions. All 327 files matched the tested candidate
before its build note was updated. Physical Android and live WAN remain
unverified. See [the integration report](LATEST-MAIN-INTEGRATION.md).

Current audit: `Builds/SizeAudit/latest/`; preserved audits, entry delta and
verification: `Builds/GitSyncQA/`. Previous delivery/audit archive:
`Legacy/20261020-000738-460-git-sync-delivery/`.

## Fishing eye-level camera and natural swimming — 19 October 2026

Fresh `AndroidSubmission` succeeded at **14:01:07 Malaysia time**. Actual APK:
**87,580,516 bytes**, down **704** from the directly measured opening APK
**87,581,220 bytes**. Headroom below the strict 1,000,000,000-byte cap:
**912,419,484 bytes**. The automatic gate, ZIP CRC validation and independent
APK v2 verification passed. Signing uses the configured Android Debug
certificate. SHA-256:
**8C8D4BEC11FF5F8C10AE7918A713D7F9C86D5E667117D28ED22A90AC5B850A24**.

| Largest APK entry | Compressed bytes | Change from opening APK |
| --- | ---: | ---: |
| `assets/PackagedContent/islands.bundle` | 43,134,410 | -4,684 |
| `lib/arm64-v8a/libil2cpp.so` | 12,336,737 | +3,626 |
| `lib/arm64-v8a/libunity.so` | 9,773,270 | 0 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,757,223 | +350 |

Both serialized asset inventories match their opening measurements: player
assets **25,688,452 bytes**, scene bundle assets **150,702,139 bytes**. These
uncompressed inventories are not the actual APK size. The largest unchanged
scene entries are Fishing/Golf terrain controls (22,369,760 / 22,369,756 bytes),
their water-depth textures (5,592,564 / 5,592,560), Football's stadium mesh
(3,569,800) and the character mesh (2,940,648). The largest player entries are
the built-in Unity splash logo (2,796,376) and menu mascot (1,607,464).
No new art or lossy reduction was introduced. The small APK difference reflects
code/metadata and bundle compression, with no unexpected large entry growth.

The release retains eye-level fishing, adds smooth independent wandering to
the shared fish pool and replaces the extra rope-side filled gauge with a
loose-to-taut line cue. See [the release note](FISHING-NATURAL-SWIM.md) and
[4,930 passing checks](FISHING-VERIFICATION.md). All 467 C# files matched the
validated source snapshot before and after the Android build. Protocol is 70.

Current audit: `Builds/SizeAudit/latest/`. Before/after audit copies, source
hashes, entry deltas, logs, signature and delivery records:
`Builds/FishingNatural/Task-20261019-052154Z/`. This includes prior uncommitted
shared-lagoon work above `9bb55646`. Physical Android and WAN remain unverified;
store release signing is separate from this debug-certificate verification.

## Huawei Scan Kit and shared room menus — 18 October 2026

`AndroidSubmission` succeeded at **19:49:58 Malaysia time**. Actual APK:
**90,543,496 bytes**, up **2,960,308** from **87,583,188**. Headroom below the
strict 1,000,000,000-byte cap: **909,456,504 bytes**. ZIP integrity, the size gate
and independent APK v2 signature verification passed. SHA-256:
**36A3FBC0B2DB1CC7440CAAEE0CF53B57AFF13276C08856D99EBEE5B74AF74CF6**.

| APK entry | Compressed bytes | Change |
| --- | ---: | ---: |
| `assets/PackagedContent/islands.bundle` | 43,151,478 | −5,476 |
| `lib/arm64-v8a/libil2cpp.so` | 12,321,369 | +6,185 |
| `lib/arm64-v8a/libunity.so` | 9,807,995 | +33,922 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,758,897 | +3,255 |
| `classes.dex` | 1,062,654 | +710,914 |
| `lib/arm64-v8a/libscannative.so` | 936,122 | +936,122 |
| `assets/detect.ms` | 587,404 | +587,404 |
| `assets/corner.ms` | 333,930 | +333,930 |
| `assets/angle.ms` | 105,482 | +105,482 |

The new scanner library, three models, Java dependencies and SDK resources
explain the growth. The combined snapshot also includes the parallel menu task,
so the total delta is not a pure SDK measurement. No lossy asset changes or
unexpected large contributors were found. Player serialized assets are
**25,689,908 bytes**, with the separate bundle **150,702,143 bytes**; these are
not the APK length.

The first attempt failed R8 validation on optional networking-provider classes.
The final build retains Huawei's packaged consumer rules, keeps the Unity bridge
and applies an explicit optional-class warning list; release minification stays
enabled. Full rebuilding and APK inspection passed after this repair.

Manifest checks confirm the camera is optional, the scanner activity is private,
and app ID `119406947` is present. DEX checks confirm the bridge, activity,
ScanUtil and RemoteView survived shrinking. The APK is signed with the registered
development certificate. Device scanning and final store signing remain separate.

Current audit: `Builds/SizeAudit/latest/`. Preserved final audit, earlier APK,
entry deltas, signature, manifest and class checks: `Builds/ScanKitQA/`.
Windows: 38 QR checks plus 1,442 menu assertions passed; complete player promoted
to `Builds/WindowsFinal/`. Previous player archive:
`Legacy/20261018-194458-193-scan-kit-windows-delivery/`.
See [the integration report](HUAWEI-SCAN-KIT.md).

## Quieter sports audio and one-second Lock-on — 18 October 2026

`AndroidSubmission` succeeded at **17:41:05 Malaysia time**. Actual APK:
**87,583,188 bytes**, down **8,988** from **87,592,176**. Headroom below the
strict 1,000,000,000-byte cap: **912,416,812 bytes**. ZIP integrity and independent
APK v2 signature verification passed. SHA-256:
**845E5EA5021A247E848D757B90E9A24EDE571470CB14835A6F0AB95E690AA1A6**.

| Largest APK entry | Compressed bytes | Change |
| --- | ---: | ---: |
| `assets/PackagedContent/islands.bundle` | 43,156,954 | −9,977 |
| `lib/arm64-v8a/libil2cpp.so` | 12,315,184 | +887 |
| `lib/arm64-v8a/libunity.so` | 9,774,073 | 0 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,755,642 | +99 |

Only playback locality/gain and the Basketball Lock-on duration changed in
production code. There were no new audio/visual assets, lossy changes or
unexpected large contributors. Player assets remain **25,688,616 uncompressed
serialized bytes** and the separate bundle **150,702,143**; neither is the APK
length. Rebuild serialization/compression can change bundle bytes without a
change in those totals.

The complete validated Windows player is in `Builds/WindowsFinal/`; all **737
audio/prop assertions passed**, including three-peer one-second release and
opposing-block interruption. No phone was connected. See
[the task report](QUIETER-SPORTS-AND-LOCK-ON.md) for source snapshot and limits.
Current audit: `Builds/SizeAudit/latest/`; preserved audit, entry comparison,
signature and verification: `Builds/QuietSportsAudioQA/`. Previous delivery/audit
archive: `Legacy/20261018-173822-247-quiet-audio-delivery/`.

## Football direction and guest response — 18 October 2026

`AndroidSubmission` succeeded at **16:55:46 Malaysia time**. Actual APK:
**87,592,176 bytes**, up **22,992** from **87,569,184**. Headroom below the
strict 1,000,000,000-byte cap: **912,407,824 bytes**. Independent APK v2
verification passed with the configured Android Debug certificate. SHA-256:
**9DE4A00B43E5A9B7A8AD6868F58EAD2C13FD371E8476D3D63A1E8A115A7BB988**.

| Largest APK entry | Compressed bytes | Change |
| --- | ---: | ---: |
| `assets/PackagedContent/islands.bundle` | 43,166,931 | +14,266 |
| `lib/arm64-v8a/libil2cpp.so` | 12,314,297 | +5,923 |
| `lib/arm64-v8a/libunity.so` | 9,774,073 | +806 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,755,543 | +1,606 |

The new markers reuse primitive meshes, the existing stadium-frame material
and built-in font. There are no added textures, lossy asset reductions or
unexpected large contributors. The player report's **25,688,616 uncompressed
serialized bytes** excludes the separate scene bundle and is not the APK size.
Both ends were inspected close/wide on Windows D3D12 and D3D11.

The current audit is in `Builds/SizeAudit/latest/`; its preserved copy and actual
ZIP-entry deltas are in `Builds/ResponsivenessQA/SizeAuditFinal/` and
`Builds/ResponsivenessQA/apk-entry-delta.csv`. The same evidence folder holds
signature verification, screenshots and source/copy hashes. Superseded output
is inventoried under `Legacy/20261018-165242-181-football-direction-response/`.

Windows gameplay/capacity/visual suites passed 1,284 assertions plus both
old/new protocol mismatch directions. Protocol 69 is required on every peer.
The [latency report](FOOTBALL-DIRECTION-AND-LATENCY.md) records the measured
movement improvement, unchanged shot-latency limit, loss-during-admission
failure and rendering diagnostic that did not reproduce. Physical Android
rendering, performance and live WAN behavior remain unverified.

## Shared travel and match-start arrivals — 18 October 2026

`AndroidSubmission` succeeded at **14:44:53 Malaysia time**. Actual APK:
**87,569,184 bytes**, up **8,180** from **87,561,004**. Headroom below the
strict 1,000,000,000-byte cap: **912,430,816 bytes**. Independent APK v2
verification passed with the configured Android Debug certificate. SHA-256:
**F7EAE76B30E61CDA42EAAE3E8BF9DC185965A2BDAD25D12FFC362E4BBFCE9E02**.

| Largest APK entry | Compressed bytes | Change |
| --- | ---: | ---: |
| `assets/PackagedContent/islands.bundle` | 43,152,665 | -2,811 |
| `lib/arm64-v8a/libil2cpp.so` | 12,308,374 | +9,474 |
| `lib/arm64-v8a/libunity.so` | 9,773,267 | 0 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,753,937 | +1,237 |

Code and metadata account for the small increase. Runtime-created status UI
reuses existing assets; no new art or lossy visual changes were introduced.
The player-side report contains 25,688,056 uncompressed serialized bytes and
excludes the separate scene bundle; it is not the APK measurement.

The fresh audit is in `Builds/SizeAudit/latest/`, with a preserved copy and entry
deltas in `Builds/TravelMatchQA/`. Windows validation passed 1,156 assertions across
travel/match-start, teleport/rematch, spectator and network rail suites, plus
both protocol-67/68 mismatch directions. The full player and shareable ZIP are
described in [teammate testing](TEAMMATE-TESTING.md). This includes pre-existing
shared-workspace changes. Physical Android performance and WAN play are unverified;
the unrelated prior cart-suite baseline is recorded in
[the audio integration report](GOLF-FISHING-AUDIO.md).

## Golf and Fishing approved audio — 18 October 2026

`AndroidSubmission` succeeded at **13:20:42 Malaysia time**. Actual APK:
**87,561,004 bytes**, up **4,251,295** from **83,309,709**. Headroom below the
strict 1,000,000,000-byte cap: **912,438,996 bytes**. Independent APK v2
verification passed with the configured Android Debug certificate. SHA-256:
**A6A60BED27CB01E5AB1A3DF2E34ACCB0D3B182DA083854E2FC7ADF75D02B256F**.

| Largest APK entry | Compressed bytes |
| --- | ---: |
| `assets/PackagedContent/islands.bundle` | 43,155,476 |
| `lib/arm64-v8a/libil2cpp.so` | 12,298,900 |
| `lib/arm64-v8a/libunity.so` | 9,773,267 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,752,700 |

All **81 new GolfFishingAudio WAVs** appear in the player-side packed report,
totaling **4,202,996 uncompressed serialized bytes**. The new PCM resource
entries account for most growth; shore wash is the largest new resource
entry at **1,363,968 actual compressed-entry bytes**. Per-asset DEFLATE estimates
in the task evidence are estimates and are not substituted for actual ZIP
entry or APK byte counts. The islands bundle shrank 4,734 bytes; IL2CPP grew
11,350 bytes. No unexpected large visual asset growth or lossy visual changes
were introduced. Thirty-four approved shared audio clips are reused in place.

Before/after reports, entry deltas, asset estimates, signature output and
source hashes are in `Builds/GolfFishingAudioQA/`; the fresh audit remains in
`Builds/SizeAudit/latest/`. **970 task-relevant Windows assertions** passed.
The broader cart suite has the same 33 distinct failed assertions on the
previous player and the audio candidate; [details](GOLF-FISHING-AUDIO.md).
No Android device was connected, so physical phone audio, FPS and memory
are unverified. The build includes pre-existing shared-workspace changes.

## Sky Rail station and cabin repair — 18 October 2026

`AndroidSubmission` succeeded at **01:40:39 Malaysia time**. Actual APK:
**83,309,709 bytes**, compared with **80,052,983** at task start
(**+3,256,726**). Headroom below **1,000,000,000 bytes**: **916,690,291**.
Independent APK v2 verification passed; the configured Android Debug
certificate signs this local hackathon build. SHA-256:
**FEBF34C89F804123D49EF1576B79751615EAE86B5DAE6B6162109DE973D4D790**.

| Largest APK entry | Compressed bytes |
| --- | ---: |
| `assets/PackagedContent/islands.bundle` | 43,160,210 |
| `lib/arm64-v8a/libil2cpp.so` | 12,287,550 |
| `lib/arm64-v8a/libunity.so` | 9,773,533 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,750,141 |

The bundle grew **756,879 bytes**, Unity runtime **578,252**, and IL2CPP
**60,843**. The baseline predates existing Golf/integration work in this
shared checkout: the asset inventories show newly included Golf course meshes,
sand and Golf UI. This is not an isolated measurement of the rail repair.
No lossy texture reduction was introduced for this task.

The separate bundle inventory's largest **uncompressed serialized** assets
remain Fishing/Golf terrain control maps (**22,369,760 / 22,369,756 bytes**) and
water depth maps (**5,592,564 / 5,592,560**). These are not APK byte counts.
SkySail-path assets total **4,676,580 uncompressed serialized bytes**, including
the retained distant-island meshes; the largest revised station mesh is
`SailStation_SS_Timber.asset` at **179,380 serialized bytes**. Native Blender
masters remain outside the Unity delivery assets.

Fresh reports are in `Builds/SizeAudit/latest/`; task copies and entry deltas
are in `Builds/SkyRailQA/SizeAuditFinal/` and `apk-entry-delta.csv`. All 341
Windows runtime source documents match current source, and the recorded
gameplay/source snapshot remained unchanged during Android packaging.
[Repair and validation details](SKY-RAIL-REPAIR.md) include 237 passing checks,
eight full route journeys, local host/guest travel and native/FBX portability.
No phone was connected; APK build/size/signature success does not validate
physical Android rendering, FPS, thermal behavior or peak memory.

## Golf and upstream integration - 17 October 2026

Merged-source AndroidSubmission attempted; Gradle/JDK initialization failed:
java.io.IOException: Unable to establish loopback connection. Retained APK
remains the pre-merge 95,067,465-byte file; new merged-source APK size and
growth are unverified. Historical headroom under 1 GB is 904,932,535 bytes.
Builds/SizeAudit/latest/build.log and LastBuild.buildreport show this failure;
other CSV/summary entries there still belong to the earlier successful build
and cannot validate merged content. Task failure log: Builds/GolfPublishQA.
No fresh APK contributor audit was produced. Historical largest estimated
entries include mobile timber, character albedo and Golf structure meshes.


Merged local Golf completion (86d48c3) with origin/main (9312948), including
menu/guest profiles, Fishing startup/hold controls, replay/characters and
networking. Both branches' verification and size history are retained.
Human-provided AGENTS 1 GB rule and audit workflow take precedence over the
incoming older policy wording. Local V5 obstacles/water, ball visuals and
feedback are preserved; upstream contact-shading field removal and generated
ball object-ID churn are resolved without discarding local course objects.
Local opening APK: 95,067,465 bytes; 1 GB headroom 904,932,535 bytes.
Incoming pre-merge APK record 78,456,347 bytes also describes different source.
Neither historical APK validates this new merged snapshot. Current build
and publication evidence is under ignored Builds/GolfPublishQA.


## User-recorded Golf GO and walkway audio — 17 October 2026

Fresh AndroidSubmission **2026-10-17 19:34:26 Malaysia time**:
**94,935,333 → 95,067,465 bytes (+132,132)**. Remaining headroom under the
**1,000,000,000-byte** cap: **904,932,535 bytes**. Full build, budget gate and
independent APK v2 signature verification pass. SHA-256:
**8468547CD3D98039C15E728F33C71C23AA3599FCB38507ABB808DAD7ED2C5F8B**.

GO uses the user's first recording, cropped .53-1.46 s, once at the real GO
transition. Walkway Path/Approach/Paving contacts use three individual steps
from the second recording, not the full multi-step sequence. Each planted
foot selects one take at original pitch; other surface clips, foot cadence,
countdown-number beeps, volume controls and gameplay remain unchanged.
Four mono 32 kHz PCM clips total **126,256 source WAV bytes**. Full decoded
48 kHz stereo masters and provenance remain outside Unity Resources in
`ArtSource/Audio/Golf/Recordings/`; the portable deterministic generator is
`Tools/Audio/prepare_golf_recordings.py`. No video or source masters ship.
See [GOLF-FEEDBACK.md](GOLF-FEEDBACK.md) and the source README for crop/processing.

Final complete Windows build (**19:31**) passed **216 rendered HUD/feedback
checks with 34 captures and 17 targeted shot checks**, at
`Builds/GolfHUDQA/Run-20261017-193200/` and
`Builds/GolfShotQA/Run-20261017-193200/`. Confirms GO references the recorded
asset, three short mono steps have clean boundaries and no clipping, actual
planted walkway contacts play recorded takes, and one locomotion cycle still
produces two sounds. Existing five-beep/one-GO, other surfaces, previews and
feedback checks pass. Regeneration from an unrelated working directory produces
identical WAV hashes. Physical speaker/timbre perception, phone output/FPS and
live WAN remain unverified. Existing broad-suite Hole 4 putt-rest issue remains
documented below; this task does not change ball motion or gameplay.

All **322 full Windows player files** matched SHA-256 on promotion to
`Builds/WindowsFinal/`. Previous player archived in
`Legacy/20261017-193332-031-golf-recorded-audio-player/`. Evidence, raw extraction
analysis, hashes, build logs, APK/signature and audit: `Builds/GolfRecordedAudioQA/`.
Pre-existing saved scene/UI/art work remains included as captured, not separately
qualified. No stage, commit or push was performed.

Four audio resource entries contribute 126,592 compressed bytes; IL2CPP grows
3,180 bytes. Other small ZIP/metadata shifts account for the remaining growth.
Largest art estimates remain timber 2,247,414; character albedo 1,992,754;
Golf structures 1,965,725; character mesh 1,588,031 bytes. No unexpected art
increase. WAV/source bytes and ZIP estimates are not measured APK size.

## Golf Putt preview and presentation refinements — 17 October 2026

Fresh AndroidSubmission **2026-10-17 19:13:49 Malaysia time**:
**94,934,277 → 94,935,333 bytes (+1,056)**. Remaining headroom below the
**1,000,000,000-byte** cap: **905,064,667 bytes**. Full build, budget gate and
independent APK v2 signature verification pass. SHA-256:
**00C770C9C04DB8C88BBF3AEF1BFAFE131F7BA6CCB0A9087E534AAE5381DF142C**.

GO now plays one clean continuous tone without breath noise or trill. Full
charge no longer plays a ready cue; pale-red Swing edges reach 30% opacity.
The character power meter has rounded teal trim and a highlight. Roaming's
Walk near your ball to aim plate is hidden; charge/driving hints remain.
Actual path supports select stone footsteps. Wheel marks are darker (.48
opacity); contacts emit 8 larger, more widely scattered flecks, capped at 256
particles per used cart. Marks still fade fully in 3 s. Golf ponds reuse the
Fishing water shader with .28 base opacity, ripple speed 1 and a cooler blue
tint compensating for the green bed. Material generator defaults match.
Putt display ends at the first solid obstacle instead of showing later rebound
or final rest, including default-layer objects; Swing display is unchanged.
Authoritative ball plans/physics, scoring, terrain geometry and driving are
unchanged. Details/settings: [GOLF-FEEDBACK.md](GOLF-FEEDBACK.md).

Complete final Windows candidate (full build **19:11**) passed **211 rendered
HUD/feedback checks with 34 captures and 17 targeted shot checks**, at
`Builds/GolfHUDQA/Run-20261017-191146/` and
`Builds/GolfShotQA/Run-20261017-191146/`. Covers actual welcome-path stone sound,
hidden roaming hint, 30% edge without ready cue, steady 2400 Hz GO signal,
pond shader/opacity, Putt endpoints immediately before both default/scene-layer
obstacles at different powers, and unchanged Swing endpoint. Existing foot
plant, wheel emission/expiry, local camera and contact checks also pass.
Visual review confirmed rounded meter, red edge, stronger wheel marks/flecks,
hidden idle hint and transparent blue pond. No physical speaker/phone/WAN
quality or performance is certified by these desktop checks.

All **322 complete player files** matched SHA-256 on promotion to
`Builds/WindowsFinal/`. Previous player archived to
`Legacy/20261017-191305-118-golf-refine-player/`. Evidence, build logs, source
hashes, player hashes, signed APK and audit: `Builds/GolfRefineQA/`. Failed
intermediate test-fixture lookups are retained as diagnostic evidence and were
corrected before final qualification. Earlier green pond capture was refined
and both final platforms rebuilt; intermediate APK is not final validation.

Compressed IL2CPP grows 839 bytes and metadata 131 bytes; other ZIP entries
shift slightly. Largest art estimates remain timber 2,247,414; character albedo
1,992,754; Golf structures 1,965,725; character mesh 1,588,031 bytes. No new
imported art/audio or unexpected art growth. Estimates are not APK size.
The pre-existing broad shot-suite Hole 4 putt-rest issue remains documented
below; ball motion is unchanged. Pre-existing saved scene/UI/art work remains
included as captured and is not independently qualified by these suites.

## Golf charge meter, planted footsteps and cart effects — 17 October 2026

Fresh AndroidSubmission **2026-10-17 18:12:36 Malaysia time**:
**94,926,465 → 94,934,277 bytes (+7,812)**. Remaining headroom below the
**1,000,000,000-byte** cap: **905,065,723 bytes**. Build gate and independent
APK v2 signature verification pass. SHA-256:
**B651B30566E334A12DD26DD6CF51BA369CE026EC0DED373F71BE7C905AA6002C**.

Changes: pale-red near-full Swing screen edges; character-side vertical power
meter replacing the action-button ring, yellow through 75% then blending to
pure red at full power. Footsteps use actual ankle lift/plant transitions and
observed grounded movement; the real locomotion-cycle fixture produces exactly
two sounds. Opening 5/4/3/2/1 uses one shared beep; GO uses a separate whistle.
Moving supported wheels emit small soil flecks and shallow marks fading fully
over 3 s. Parked carts emit nothing. Effects use bounded pools/shared material;
no map, ball physics, driving, scoring, animation or other-sport changes.

Final Windows candidate passed **203 rendered HUD/feedback checks with 33
captures plus 17 targeted shot checks** at
`Builds/GolfHUDQA/Run-20261017-180937/` and
`Builds/GolfShotQA/Run-20261017-180937/`. Validates real skeletal cadence,
colour thresholds/button-ring removal, five countdown beeps/one GO whistle,
wheel contact generation, parked suppression and complete 3 s mark expiry.
Actual captures show the pale-red edge, relocated meter and cart contact view.
All **322 complete player files** matched SHA-256 when promoted to
`Builds/WindowsFinal/`; full build 18:08 and final script refresh 18:09.
Previous player archived in `Legacy/20261017-181139-736-golf-polish-player/`.
Evidence, source/player hashes, APK, signature and audit: `Builds/GolfPolishQA/`.
Settings and behavior: [GOLF-FEEDBACK.md](GOLF-FEEDBACK.md).

Growth is mainly +6,119 compressed IL2CPP bytes and +1,250 metadata bytes.
Largest art estimates remain timber 2,247,414; character albedo 1,992,754;
Golf structures 1,965,725; character mesh 1,588,031 bytes. No new imported
bitmap/audio assets were added. ZIP estimates are distinct from APK size.
Physical speaker/timing perception, mobile appearance/FPS and live WAN require
manual/device testing. The pre-existing broad shot-suite Hole 4 putt-rest
failure remains documented below; this task does not change ball motion.
Pre-existing saved scene/UI/art work remains included as captured.

## Golf follow camera, footsteps and tree-side aim — 17 October 2026

Fresh AndroidSubmission **2026-10-17 17:44:37 Malaysia time**:
**94,912,749 → 94,926,465 bytes (+13,716)**; headroom under the current
**1,000,000,000-byte** cap: **905,073,535 bytes**. Build budget gate and
independent APK v2 signature verification pass. SHA-256:
**D50825359064600D0D24154D9F3908FE5C5DB6B9213FC1F817AF2C37D2870991**.

Adds a silent 512x288 live ball camera below Island Greens, shown on local
actual contact through flight/rolling, held 3 s after settling and faded over
0.65 s. Hidden views stop rendering. Adds distance-driven grass, sand, shallow
water, wood and stone footsteps, with existing master/SFX controls. Swing edge
glow is wider and reaches 14% opacity; Putt retains no edge overlay. Tree/bunker
Aim fallback searches safe alternate stances while preserving collision,
sight, walkable ground and reach checks. No map, ball physics or scoring edits.
Settings are centralized in `Game/Assets/_Game/Resources/GolfFeedback.asset`.

Final complete Windows candidate passed **196 rendered HUD/feedback checks,
32 captures and 17 targeted ball interaction checks** in
`Builds/GolfHUDQA/Run-20261017-174235/` and
`Builds/GolfShotQA/Run-20261017-174235/`. The camera test reads actual world
pixels from its render texture, not merely its enabled flag. Edge on/off
captures differ at 60 of 63 sampled edge pixels. Initial empty-camera output
was caught during visual review and fixed before promotion. All **322** player
files matched SHA-256 on promotion to `Builds/WindowsFinal/`. Previous player
and an incorrect foreground-app capture were archived in
`Legacy/20261017-174340-729-golf-follow-player/`. Correct running-game capture,
source/player hashes, build logs, signature and APK audit are in
`Builds/GolfFollowQA/`.

APK growth is mainly +13,290 compressed IL2CPP bytes and +1,460 metadata bytes,
partly offset by -1,596 Burst bytes. Largest art estimates remain unchanged:
timber 2,247,414; character albedo 1,992,754; Golf structures 1,965,725;
character mesh 1,588,031 bytes. No imported bitmap/audio assets were added;
five footstep clips are synthesized once. Estimates are not APK measurements.
Physical speaker quality, mobile appearance/FPS and live WAN remain unverified.
The known existing `HOLE_4_Putt_0.80_PUTT_RESTS_AT_GLOW` broad-suite failure
remains documented below; this task does not alter ball motion. Pre-existing
saved scene/UI/art work is included as captured, not independently qualified.

## Golf charge, impact, landing and cup feedback — 17 October 2026

Fresh AndroidSubmission **2026-10-17 17:09:17 Malaysia time**:
**94,305,339 → 94,912,749 bytes (+607,410)**. Remaining headroom below the
current **1,000,000,000-byte** cap: **905,087,251 bytes**. Full Unity build,
budget gate and independent APK v2 signature verification pass. SHA-256:
**85E1E2CCD7492CC8F1BEC020B4B77CCD8090DBAFD4D976F2BFD849615573D84D**.

Adds gold charging ring, rising charge loop, one ready sound per peak, faint
Swing edge glow, tiny charge camera push, actual-contact Swing/Putt sounds and
particles, bounded local strong-Swing kick, actual turf/sand impact sounds and
particles, successful-cup chime/particles/short label, and a distinct final-hole
cue. Putt remains stable. Reduced Motion/central shake toggle are honored.
Cancel, focus loss, pause, round reset and leaving Golf clear transients.
Host-confirmed cosmetic messages reject stale/wrong-round/nonfinite/duplicate
events; live WAN synchronization remains unverified. Ball-follow camera is
explicitly deferred. Physics, course, progression and other sports are unchanged.

The +607,410 bytes are predominantly +579,417 compressed bytes in libunity.so
(Unity particle/audio modules retained), +17,874 in libil2cpp.so, +6,135 metadata
and +2,783 for the small particle shader entry. There are no imported sound or
bitmap assets; eight cues and a charge loop are synthesized once at 24 kHz.
The capped particle pool shares one unlit material. Existing largest art ZIP
estimates remain timber 2,247,414, character albedo 1,992,754, Golf structures
1,965,725, character mesh 1,588,031 and stadium 1,556,008 bytes. These estimates
are distinct from measured APK bytes; no unexpected art growth was observed.

Complete 322-file Windows candidate (full build **16:51:20**, final script
refresh **17:06**) passed **175 rendered HUD/feedback assertions and 29 captures**
in `Builds/GolfHUDQA/Run-20261017-170636/`, plus **17 targeted actual ball
interaction assertions and one cup-success capture** in
`Builds/GolfShotQA/Run-20261017-170636/`. Every candidate file matched by hash
when promoted together to `Builds/WindowsFinal/` before updating build metadata.
Task source hashes match final Windows/Android. Saved pre-existing scene/UI/art
work is included as captured, not independently qualified by these suites.

Known pre-existing broad shot-suite failure:
`HOLE_4_Putt_0.80_PUTT_RESTS_AT_GLOW` reproduces in both the former delivered
player (`Builds/GolfShotQA/Run-20261017-165339/`) and this feedback build
(`Builds/GolfShotQA/Run-20261017-165211/`). It remains unresolved; no ball
physics changes hide it. The old near-cup fixture's .35 charge overshoots under
current speeds; fixture-only .008 charge from .65 m enters the real cup and
scores once. Final-hole cue uses a valid progress-transition fixture. See
[GOLF-FEEDBACK.md](GOLF-FEEDBACK.md) for controls, event architecture and limits.

Current exact APK/audit/signature, source hashes, complete player, reports and
baseline evidence are retained under `Builds/GolfFeedbackQA/` and the run
folders above. Superseded player archive:
`Legacy/20261017-170827-172-golf-feedback-player/` (322 files). No staging,
commit or push performed. Phone appearance/touch/FPS, physical speaker quality
and live WAN are unverified; desktop signal/screenshot tests do not certify them.

## Golf Aim target indicator and hidden ranking — 17 October 2026

Fresh AndroidSubmission **2026-10-17 16:31:38 Malaysia time**:
**94,305,115 → 94,305,339 bytes (+224)**. Headroom below the current
**1,000,000,000-byte** limit: **905,694,661 bytes**. Full build, budget gate
and independent APK v2 signature verification pass. SHA-256:
**5B1D89BA2055689694F17C12F59D07254E8154170A18DC84A896792693AB9A40**.

Aim now uses the existing hole/distance card at 72% uniform scale, 12 units
below the safe-area top. Its large central arrow is hidden. A 42x44 edge arrow
appears only for offscreen/behind-camera targets and avoids the top card and
bottom shot/Cancel/power-hint band. The whole ranking root is inactive during
Aim; no personal rank, completed-hole count or stroke summary remains visible.
Normal card/ranking return on Aim exit. Final countdown notices are preserved.
No map, physics, progression, other sport or audio changes are introduced.

Latest audit leading ZIP estimates remain timber 2,247,414, character albedo
1,992,754, Golf structures 1,965,725, character mesh 1,588,031 and stadium
1,556,008 bytes. No new art, material, font, shader or package is shipped;
there is no unexpected asset growth. Estimates are distinct from APK bytes.

The complete 322-file Windows candidate (full build 16:25:02, followed by a
script refresh for bottom-edge clearance) passes **143 rendered assertions
and 27 captures** in `Builds/GolfHUDQA/Run-20261017-162939/`. Screenshot review
caught a bottom arrow behind the hint in the initial review; final checks
explicitly cover unobstructed edge placement. Final player files matched by
hash when promoted together to `Builds/WindowsFinal/`, before metadata update.
Task source hashes match Windows/Android. Saved pre-existing scene/course/UI
work is included as captured, not independently qualified by this HUD suite.
Phone appearance/touch/FPS and WAN remain unverified.

Audio inspection found no dedicated Golf charge/swing/putt/contact/landing/
cup audio hooks or Golf scene/prefab AudioSource. Missing Golf shot sounds
are an unimplemented feature, not a Windows-only limitation. This task did
not add audio or change mute/volume preferences.

Evidence, complete player, APK, signature, source hashes and audit are in
`Builds/GolfAimIndicatorQA/`. Archive batch:
`Legacy/20261017-163035-698-golf-aim-indicator-player/` (superseded 322-file
Windows player). No staging, commit or push was performed.

## Golf aim focus and revised APK limit — 17 October 2026

The user changed the APK limit to **strictly below 1,000,000,000 bytes
(1 decimal GB)**. The former 100 MB cap and 75 MB working target are retired;
older entries below describe their historical budgets. `AGENTS.md`, the build
size gate, readiness check and editor menu now use the current requirement.

Fresh AndroidSubmission **2026-10-17 16:09:49 Malaysia time**:
**94,303,923 → 94,305,115 bytes (+1,192)**. Current-limit headroom:
**905,694,885 bytes**. Full Unity build, budget gate and independent APK v2
signature verification pass. SHA-256:
**6FD607A76E15FC820DFFABCA01FF6825A74CF14D2C35F1E081F97A25E1B0E28D**.

Aim fades auxiliary HUD over 0.25 seconds and immediately blocks its touch
targets. Hole/distance, trajectory, shot/Cancel, charge hint and final notices
remain. Cancel or shot exit restores the HUD. Both Swing and Putt are covered;
reduced motion is honored. The editable parameter is `aimFocusFadeSeconds` in
`Game/Assets/_Game/Resources/GolfUILayout.asset`. No new art/audio/package,
map edits or physics changes are introduced by this task.

The latest packing audit has no unexpected growth: leading estimated ZIP
contributors remain timber 2,247,414, character albedo 1,992,754, Golf structures
1,965,725, character mesh 1,588,031 and stadium 1,556,008 bytes. These are
estimates, separate from measured APK bytes. Serialized assets total
161,897,902 bytes; that is not APK size. No size optimization was imposed.

Complete 322-file Windows build at **16:00:13 Malaysia time** was promoted to
`Builds/WindowsFinal/` after **137 rendered assertions and 25 captures** passed
in `Builds/GolfHUDQA/Run-20261017-160034/`. Promoted files matched the validated
player before updating `LATEST-BUILD.txt`. Task source hashes match across
Windows and Android; the editor-only menu label changed after the Windows
build; a trailing blank line in that editor-only file was removed after Android
validation. Saved pre-existing scene/course/UI work is included as captured, not
independently qualified by this HUD suite. Phone appearance/touch/FPS and WAN
remain unverified.

Evidence, complete validated player, exact APK, signatures, source hashes,
first-attempt diagnostics and audit are retained in `Builds/GolfFocusQA/`.
Android initially failed Java loopback initialization. A stale invalid socket
directory in the generated Gradle cache was removed; the successful build used
a task-local long-path TEMP/TMP directory. No SDK/system settings or portable
source defaults were changed for that workaround.

Archives: `Legacy/20261017-160150-210-golf-aim-focus-player/` (superseded player)
and `Legacy/20261017-160603-010-golf-aim-focus-helpers/` (three finished helpers).
No staging, commit or push was performed.

## Leaderboard contours, status badge and whole-panel hover — 17 October 2026

Fresh AndroidSubmission **2026-10-17 15:20:37 Malaysia time**:
**94,312,363 → 94,303,923 bytes (-8,440)**. Strict-limit headroom
**5,696,077 bytes**. The 75,000,000-byte development target remains exceeded by
**19,303,923 bytes**; reserve remains limited. Budget gate and independent
APK v2 signature pass. SHA-256:
**404FCFD157884EEBB91CD6DBDAB8E4866A10D29A99830DD833A099A81412A20F**.

Only the existing Title/Columns PNGs change. Their estimated ZIP changes are
**−12,928 / +4,315 bytes** (net **−8,613**), while the layout asset estimate is
**+13 bytes**. All other sprite/packed asset contributions are unchanged;
existing leading estimates remain timber 2.247 MB, character albedo 1.993 MB,
Golf structures 1.966 MB, character mesh 1.588 MB and stadium 1.556 MB. No new
texture, master, material, shader, font or package is shipped. Delivery remains
ASTC 4×4/no mipmaps. Exact APK bytes are separate from contribution estimates.

The column band's opaque outside corners are removed without deleting its
gold pill or original labels. Title silhouette cleanup removes duplicate body
walls/cream backing; the expanded title is 90% of panel width, content inset
20 units and header join 0.8. A single body frame now remains visible beneath
the integrated badge. Compact/full Playing and Finished use the same complete
proportional source badges; no status nine-slicing. DNF retains live lettering.
Hover animates the **entire expanded board by 1.8%**, including title, frame,
rows, text and scrollbar; child-button scale feedback is disabled. Exit restores
the whole panel, and reduced motion is honored. The previous taller preview,
first-three-plus-local selection, scrolling, aim summary, game rules and timers
remain. Map, terrain, physics and other sports are unchanged.

Complete **322-file Windows player**, built **2026-10-17 15:16:55**, is promoted to
`Builds/WindowsFinal/` and matches the retained release by file hashes.
**131 rendered assertions and 24 captures** pass in
`Builds/GolfHUDQA/Run-20261017-151656/`. This includes compact status source
sprite/aspect, no independent child feedback, shared title/frame/row growth,
hover exit, reduced motion, safe bounds, existing scrolling/touch/backdrop,
start/aim/shot/finish/DNF and multi-roster checks. Screenshot inspection covers
compact blue corners and expanded header/body joining. Phone appearance,
touch/FPS and WAN remain unverified; the 25-entry fixture is presentation only.

All 14 sprites reproduce byte-identically from a path with spaces and unrelated
working directory. Column corners and title lower-wall corners have zero alpha.
The original master and other 12 sprites remain unchanged. Final source/art
hashes match across Windows/Android. Exact APK/audit, complete release, logs,
hashes and checks remain under `Builds/GolfLeaderboardPolishQA/`; pre-existing
scene/course/UI work is included as captured, not separately qualified here.

Reviewed archives: old player
`Legacy/20261017-151842-200-golf-leaderboard-polish-player/`
(322 files, 604.42 MB); redundant candidate, portable fixture and pre-edit
Title/Columns/audit snapshots
`Legacy/20261017-151852-851-golf-leaderboard-polish-candidates/`
(343 files, 607.42 MB). Active asset GUIDs are retained. No staging, commit
or push was performed.

## Taller proportional leaderboard preview — 17 October 2026

Fresh AndroidSubmission **2026-10-17 14:37:24 Malaysia time**:
**94,308,867 → 94,312,363 bytes (+3,496)**. Strict-limit headroom
**5,687,637 bytes**. The 75,000,000-byte working target remains exceeded by
**19,312,363 bytes**; reserve is limited. Budget gate and independent
APK v2 signature pass. SHA-256:
**3270E7A64F4EBFBFB3034D370E510AA0B893071B64AD4FD3F851AFDFCEA816C3**.

No image, font, shader, material, package or map asset is added or modified.
All leaderboard sprite contribution estimates remain unchanged; only layout
asset ZIP estimate changes by 3 bytes, alongside the small compiled-code APK
delta. Existing largest estimates remain timber 2.247 MB, character albedo
1.993 MB, Golf structures 1.966 MB, character mesh 1.588 MB and stadium
1.556 MB. Exact APK bytes remain distinct from contribution estimates.

The compact column band now retains the source sprite aspect ratio instead
of a fixed 30-unit height. Preview row height **40 → 64**, title width
**230 → 320**, toggle height **32 → 36**, and preview lettering is larger.
A crowded card scales uniformly as a whole to its **44% safe-height ceiling**;
rows and decorative art are no longer squeezed separately. One-player cards
remain at full scale, about 279 reference units tall. The existing first-three
plus-local selection, full scrolling ranking, native pointer behavior and aim
summary remain. Shorter layouts move Jump to the existing left-side utility
placement when needed to avoid the taller card. Other sports, ranking rules,
progress, match timers, scene layout and gameplay physics are unchanged.

Complete **322-file Windows player** matches the retained release and is
promoted to `Builds/WindowsFinal/`. Full asset build **2026-10-17 14:33:18**, final
script refresh **2026-10-17 14:34:27**. **126 rendered assertions and 23 captures**
pass in `Builds/GolfHUDQA/Run-20261017-143427/`: one-player row height,
source-proportional column art, uniform crowded-card scale, bounded rendered
height, four-player wide layout/control separation, three landscape sizes
with safe insets, existing expand/scroll/dismiss/aim/start/shot/finish/DNF
behavior, and the 25-entry presentation roster. Phone UI/touch/FPS and WAN
remain unverified. The latter fixture is not network-player qualification.

Final source and existing sprite hashes are unchanged across the Windows and
Android builds. Exact APK, audit, complete release player, hashes and logs
remain in `Builds/GolfLeaderboardHeightQA/`. Saved pre-existing scene/course/UI
work is included as captured and is not separately qualified by this HUD
review. Reviewed archives: prior player
`Legacy/20261017-143531-325-golf-leaderboard-height-player/`
(322 files, 604.41 MB), redundant tested candidate
`Legacy/20261017-143546-186-golf-leaderboard-height-candidate/`
(322 files, 604.42 MB). No staging, commit or push was performed.

## Supplied leaderboard and bounded ranking — 17 October 2026

Fresh AndroidSubmission **2026-10-17 14:16:41 Malaysia time** measures
**93,928,409 → 94,308,867 bytes (+380,458)**. Headroom below the strict
100,000,000-byte limit: **5,691,133 bytes**. The 75,000,000-byte development
target remains exceeded by **19,308,867 bytes**; reserve remains limited.
Budget gate and independent APK v2 signature pass. SHA-256:
**2C0561EF22CB974E91770692601BF82906ACAE7902CD5E10E76294E0944E4FFE**.

The 14 cropped leaderboard sprites contribute **780,672 serialized bytes**,
estimated ZIP **466,045 bytes**. Five superseded sprites/metas were proven
unused by GUID and runtime/generator searches, then archived; their previous
estimated ZIP cost was **90,096 bytes**. Net sprite estimate **375,949 bytes**
is separate from the measured APK delta. Full-resolution source stays outside
Unity; ASTC 4×4/no-mipmap delivery, shared UI material and existing fonts are
used. No new package, shader or material is introduced. Largest contributors
remain timber 2.247 MB, character albedo 1.993 MB, Golf structures 1.966 MB,
character mesh 1.588 MB and stadium 1.556 MB; unrelated packed contributions
remain unchanged in the before/after audit.

The crown/title, border, headings, portraits and status artwork derive from
the supplied original. Actual scores remain live. Default shows the existing
first three results plus the local player once; height is bounded to 34% of
the safe canvas. Title/View all opens a 78%-height maximum scrollable list
with a backdrop, scrollbar, mouse wheel and touch drag. Closing restores the
preview; aim closes any open list and shows only the local summary. The row
pool has no ten-player presentation cutoff. Scoring, ties, DNF ordering,
countdowns, hole progress, scene/world structure and other sports are unchanged.

Complete **322-file Windows player** is promoted to `Builds/WindowsFinal/`,
matching `Builds/GolfLeaderboardQA/Release/Windows/` by file hashes. Full asset
build **2026-10-17 14:11:59**, final script refresh **2026-10-17 14:13:32**; the complete
player was then validated with **113 rendered assertions and 22 screenshots**
in `Builds/GolfHUDQA/Run-20261017-141332/`. Tests include a real start/aim/shot,
three landscape resolutions with safe insets, top-three/local de-duplication,
fixed heights, actual expand/backdrop raycasts, native wheel/touch scrolling,
last-player access, green finished badges, unranked DNF and a synthetic
**25-entry presentation roster**. The latter does not qualify 25 networked
world players. Phone appearance/touch/FPS and WAN remain unverified.

All 14 final sprites reproduce byte-identically from a path with spaces and
an unrelated working directory. The retained master matches the supplied file
exactly. Final UI/asset/rules source hashes are unchanged across the Windows
and Android builds. Current logs, exact APK/audit, hashes and checks remain
under `Builds/GolfLeaderboardQA/`; saved pre-existing scene/course/UI changes
are included as captured and are not separately qualified by this HUD review.

Reviewed archives: superseded art
`Legacy/20261017-140158-238-golf-leaderboard-old-art/` (10 files, 0.17 MB),
prior player `Legacy/20261017-141425-628-golf-leaderboard-player/`
(322 files, 603.19 MB), redundant candidate/portable fixture
`Legacy/20261017-141511-811-golf-leaderboard-candidates/`
(340 files, 606.88 MB). No staging, commit or push was performed.

## Golf target spacing and supplied jump image — 17 October 2026

Fresh AndroidSubmission completed **2026-10-17 01:22:19 Malaysia time**:
**93,826,511 → 93,928,409 bytes (+101,898)**. Strict 100,000,000-byte limit
headroom **6,071,591 bytes**. The 75,000,000-byte development target remains
exceeded by **18,928,409 bytes**; available reserve is limited.
Budget enforcement and independent APK v2 signature pass. SHA-256:
**8A66A0EAE47CD8449B773DA6FF461818B95D62645CF2C22A04F7E962725DD3DD**.

Only one new delivery texture is introduced: Jump, proportional 512×372,
ASTC 4×4 without mipmaps, **191,248 serialized bytes**, estimated ZIP
**98,638 bytes**. Its exact user original is retained outside Unity. No font,
shader, material, package, map or terrain asset was added for this request.
Leading contributors remain timber (2.247 MB), character albedo (1.993 MB),
Golf structures (1.966 MB), character mesh (1.588 MB) and stadium (1.556 MB).
These contribution estimates are separate from measured APK bytes.

Complete validated Windows player: `Builds/WindowsFinal/`, **322 files**,
build **2026-10-17 01:19:53**; retained matching release and exact APK/audit in
`Builds/GolfJumpTargetQA/Release/`. **90 rendered HUD assertions and 18 captures**
pass in `Builds/GolfHUDQA/Run-20261017-011953/`, including Hole 2 at 5/132/1234 m,
equal visible gaps, camera removal, original jump image, countdown, three
landscape sizes with safe insets and existing aim/hold/cancel/release behavior.
The PNG generator reproduces identical bytes from a path with spaces and an
unrelated working directory. Phone appearance/touch/FPS and WAN are unverified.

Reviewed archives: superseded player
`Legacy/20261017-012054-786-golf-jump-target-player/` (322 files, 602.93 MB),
redundant full candidate and portable fixture
`Legacy/20261017-012143-549-golf-jump-target-candidates/` (337 files, 609.24 MB).
Current source hashes, promotion checks, logs and audit remain under
`Builds/GolfJumpTargetQA/`. Saved current scenes were built; pre-existing
uncommitted course/flag/UI changes are included as captured and not separately
qualified by these HUD checks. No Git staging, commit or push was performed.

## Original action images and startup target distance — 17 October 2026

Signed fresh APK: **93,826,511 bytes**, **+3,492,285** against the actual stale
**90,334,226-byte 13 October** opening package. Current-source growth includes
the intervening course/flag/HUD work. Strict headroom **6,173,489 bytes**;
75 MB goal excess **18,826,511 bytes**. Budget gate and independent v2 signature
pass. Exact APK/audit: `Builds/GolfActionImagesQA/Release/`.
[Release and source limits](BUILD-SIZE.md#original-action-images-and-startup-target-distance--17-october-2026).

| New action sprite | Serialized bytes | Estimated ZIP bytes |
| --- | ---: | ---: |
| Aim First | 261,336 | 182,204 |
| Swing | 259,280 | 178,700 |
| Putt | 259,232 | 179,988 |
| Total | 779,848 | 540,892 |

Masters remain outside Unity; only proportional 512-pixel sprites ship, with
ASTC 4×4, no mipmaps and existing importer/material. Transparent padding is
trimmed and resizing uses premultiplied alpha. No extra font, shader, model,
audio or package is added by this task. Current serialized assets total
**161,105,326 bytes**, separate from APK length. Leading estimates remain
timber **2.247 MB**, character albedo **1.993 MB**, Golf structures **1.966 MB**,
character mesh **1.588 MB** and football stadium **1.556 MB**.

Comparison to the retained 13 October packing report identifies intervening
mobile sand, five numbered flag textures, course terrain meshes and UI assets;
it does not attribute those earlier changes to this feature. No unexpected
large dependency is added by the three required sprites. Detailed contributor
comparison: `Builds/GolfActionImagesQA/asset-growth.json`; signature, top entries
and exact before/after measurement remain alongside it. Further base-game
size reductions are still required; 93.83 MB is not a comfortable working target.

## Shot plate restore and Start Game - 16 October 2026

AndroidSubmission failed Gradle/JDK initialization: java.io.IOException:
Unable to establish loopback connection. Retained historical APK remains
90,334,226 bytes; new source APK size/growth unverified. Failed log/report
in Builds/SizeAudit/latest and Builds/GolfShotUIRestoreQA/android-build.log.
No fresh packed-assets contributor report.

UI validation: 82 assertions, 14 captures in
Builds/GolfHUDQA/Run-20261016-230547. Inspected new StartGame, restored
Aim First/Putt/Swing without caption rectangles, native power percentage,
retained Exit/Your Ball and no ball distance. Checked three resolutions,
safe insets and original hold/cancel/release UI. Retained source hashes match.
Complete 322-file player copied to WindowsFinal with matching SHA-256 before
delivery note. Old player: Legacy/20261016-230728-537-golf-shot-ui-player.
No new device/WAN/map testing.


Retain Exit/Your Ball artwork; remove live ball distance. Start Game now uses
the new original image. Aim First/Putt/Swing restore Reference/Shot, native
labels and charge percentage, without a ShotCaption overlay. Corrected
GolfReferenceArtBuilder blends lower disc colours using an ellipse feather
instead of rectangular caption fills. No gameplay/input changes.

New StartGame PNG: 97,478 bytes; raw ASTC 4x4: 77,824 bytes. Removed three
unused action sprites: 951,884 PNG bytes / 768,000 raw ASTC bytes. Masters
retained outside Unity. These asset totals do not measure APK growth.
Portable generators and .meta files retained; obsolete delivery sprites
archived with explicit reference/GUID checks to
Legacy/20261016-230149-840-golf-shot-ui-unused.
Opening historical APK remains 90,334,226 bytes; headroom 9,665,774 bytes;
75 MB development-target excess 15,334,226 bytes. Fresh size requires build.


## Original action badges - 16 October 2026

AndroidSubmission attempted after UI validation; failed Gradle/JDK
initialization with java.io.IOException: Unable to establish loopback
connection. Retained APK remains the historical 90,334,226-byte file.
New badge APK size/growth and a fresh packed-assets contributor audit are
unverified. Historical hard-limit headroom 9,665,774 bytes; 75 MB working
goal exceeded by 15,334,226 bytes. Failed report/log: Builds/SizeAudit/latest
and Builds/GolfBadgesQA/android-build.log.


Aim First, Putt, Swing, cart Exit and Your Ball use original user-supplied
PNG designs with baked lettering, proportional simple images, no redraw.
Five unchanged masters remain in ArtSource/Golf/UI/Badges; portable generator
Tools/Art/prepare_golf_badges.py and delivery audit record hashes/crops.
Five 512 px PNGs total 1,157,582 bytes; raw Android ASTC 4x4 totals
940,032 bytes. These are not measured compressed APK growth. No added
font/material/shader/package; high-resolution originals do not ship.
Live charge moves to the control hint, live ball distance below original
art. Existing arrow, near-ball hide thresholds and input rules remain.
Opening historical APK: 90,334,226 bytes; hard headroom 9,665,774 bytes;
75 MB development-target excess 15,334,226 bytes. Fresh size requires a
successful AndroidSubmission, separate from source/texture measurements.


## Original cart buttons - 16 October 2026

AndroidSubmission attempted after UI checks; Gradle/JDK failed with
java.io.IOException: Unable to establish loopback connection. APK after
attempt is still the historical 90,334,226-byte file: new button APK size
and growth are unverified. Historical headroom is 9,665,774 bytes; the
75 MB development target is exceeded by 15,334,226 bytes. Failed build
log/report retained in Builds/SizeAudit/latest and Builds/GolfCartButtonsQA.
No current packed-assets contributor report was produced.


Opening retained APK: 90,334,226 bytes, hard-limit headroom 9,665,774 bytes,
75 MB development-target excess 15,334,226 bytes. Three original-design
button sprites add 400,224 PNG bytes / 282,624 raw ASTC 4x4 bytes. Masters
remain outside Unity. Downsampled 512 px delivery preserves original pixels
at current roughly 270 px display width. No new font/material/shader/package.
These are asset sizes, not APK growth; a fresh build must be measured.
See [verification](VERIFICATION.md#original-cart-buttons---16-october-2026).


## Golf button and ball-indicator refinement - 15 October 2026

AndroidSubmission attempted for this task: Gradle/JDK initialization failed
with java.io.IOException: Unable to establish loopback connection. Retained
APK remains 90,334,226 bytes; fresh UI APK size and compressed contributor
growth are unverified. Builds/SizeAudit/latest contains the failed build log
and report, not a fresh packed-assets audit. Development budget remains
15,334,226 bytes above the 75 MB goal on the historical baseline.


No new texture, font, material, shader, model, audio or package. Native steering
wheel uses current UI material; ball plate uses its existing image without
nine-slice icon distortion. Historical opening APK **90,334,226 bytes**,
headroom **9,665,774**, 75 MB goal excess **15,334,226**. A fresh successful
AndroidSubmission is required to measure new compressed growth/contributors.
See [verification](VERIFICATION.md#golf-button-and-ball-indicator-refinement---15-october-2026).


## V5 charge controls restored - 15 October 2026

Restore adds no texture/model/font/audio/material/shader/package. Power rail
and redesigned input are removed; existing V5 resources remain. Historical
APK is **90,334,226 bytes**, boundary headroom **9,665,774**, development-goal
excess **15,334,226**. Fresh AndroidSubmission size/contributors require a new
successful build; historical figures cannot validate the restore. Redesign
records below remain historical. See
[restore verification](VERIFICATION.md#v5-charge-controls-restored---15-october-2026).

The restored-source AndroidSubmission attempt failed with Gradle/JDK
loopback initialization. `Builds/SizeAudit/latest` has the failed current log
and report; no fresh APK/delta/contributor ranking exists. Historical APK
bytes remain 90,334,226, with 9,665,774 headroom and 15,334,226 dev-goal excess.


## Direct camera and launch angle - 14 October 2026 build limitation

Retained historical APK **90,334,226 bytes**, boundary headroom **9,665,774**,
75 MB development-goal excess **15,334,226**. The new source's AndroidSubmission
fails in Gradle/JDK loopback initialization, so after/delta and fresh packed
contributors are unverified. No image, font, model, shader, audio, material or
package is added; ground shots share existing meshes/settings/trajectory
buffers and camera framing uses the existing camera. Current failed log is in
`Builds/SizeAudit/latest`; see
[verification](VERIFICATION.md#direct-camera-and-launch-angle---14-october-2026).


## View aim and click Swing - 14 October 2026 build limitation

No fresh APK exists after Gradle/JDK loopback initialization failures.
Retained historical APK **90,334,226 bytes**, hard-limit headroom **9,665,774**,
75 MB goal excess **15,334,226**. These measurements cannot certify the new
control/physics source. The current failed log is in `Builds/SizeAudit/latest`.

Power UI is runtime geometry and a circular UV window on existing ball art,
sharing current UI resources; no new image, font, model, audio, shader or package
ships. Aim elevation uses the existing trajectory solver and a single RPC
float. Fresh compressed cost/largest contributors remain unverified until
AndroidSubmission succeeds. See
[verification](VERIFICATION.md#view-aim-retained-power-and-click-swing---14-october-2026).


## V5 Golf ponds and obstacles - 14 October 2026 build limitation

Opening APK **90,334,226 bytes**. Current AndroidSubmission failed in Gradle/JDK
loopback initialization; no fresh after/delta measurement exists. The retained
historical APK has **9,665,774 bytes** of hard-limit headroom and is **15,334,226
bytes** above the 75 MB development goal. It cannot validate this new source.

New 141 props reuse packed rock/palm meshes, LODs and materials. Fourteen rough
patches share existing turf/surface resources. Six water meshes (385 vertices
each) and banks (256 vertices each) use 16-bit shared indices and existing water
shader/sand resources; one shared pond material, no texture/reflection camera,
model/font/audio/package is added. High-quality design references stay outside
Unity. No unrelated quality reduction or dependency change was made.

`Builds/SizeAudit/latest` contains the failed current log/report, so compressed
cost and largest contributors remain unverified. Complete validated Windows
player is delivered; [verification](VERIFICATION.md#v5-golf-obstacles-ponds-and-dry-recovery---14-october-2026).


## Golf obstacle V2 — 14 October 2026 build limitation

Opening APK: **90,334,226 bytes**. Current-source AndroidSubmission failed
with Gradle/JDK `java.io.IOException: Unable to establish loopback connection`,
including a process-local IPv4 preference. No fresh APK or after/delta measurement
exists. The retained historical package has **9,665,774 bytes** of hard-limit
headroom and exceeds the 75 MB development goal by **15,334,226 bytes**;
it does not validate these obstacle additions.

Added 72 props reuse packed rock/palm meshes, LODs and materials. Nineteen sand
and six rough overlays use welded vertices and 16-bit indices with existing
textures/shader; one shared rough material and its mobile derivative are added.
No new bitmap, model, font, audio, package or unrelated quality reduction ships.
`Builds/SizeAudit/latest/` contains the failed current build log/report, so no
fresh largest-contributor ranking or compressed growth can be claimed.
Complete tested Windows player is delivered; [evidence](VERIFICATION.md#additional-golf-obstacle-v2--14-october-2026).


## Original screenshot Golf UI — 14 October 2026 build limitation

No fresh APK exists after the Gradle/JDK loopback failure. Retained historical
APK: **90,334,226 bytes**, headroom **9,665,774 bytes**, 75 MB goal excess
**15,334,226 bytes**. `Builds/SizeAudit/latest` records a failed build, not
a fresh successful contributor ranking. It cannot certify new UI size.

The [portable source/delivery hash manifest](../ArtSource/Golf/UI/reference-delivery-audit.json)
records 23 cropped sprites: **743,690 PNG bytes**, **537,936 raw ASTC 4x4 bytes**.
No mipmaps/CPU texture copies or new font/shader/package are added. Only UI
crops ship; the full screenshot and historic masters remain outside Assets.
Four superseded illustration files were archived after GUID, resource-load,
generator and source-provenance checks. Countdown art remains unchanged.
Actual compressed APK delta and unexpected contributors remain unverified.

## Circular Golf course — 14 October 2026 build limitation

The opening and retained APK is **90,334,226 bytes**, SHA-256
`2382DFE7BEF6B1085664B5665D14F2F33B82AE4A5EA2376B9834AA6C19C82A06`.
Both AndroidSubmission attempts failed in Gradle/JDK loopback initialization; there is no
fresh APK, before/after delta or contributor ranking. `Builds/SizeAudit/latest/`
contains that failed build log/report. Historical headroom is **9,665,774
bytes**; the 75 MB development goal is exceeded by **15,334,226 bytes**.

Circuit art adds native welded fairway/sand meshes, two shared materials and
a shader, while reusing existing rock/palm LODs, turf/sand maps and UI font.
Both map textures occur in `Builds/GolfFlagDesignQA/BaselineAudit/packed-assets.csv`
at 626,420 serialized bytes each; this historical report does not measure new
APK compression. Twenty-eight unused grass/terrain derivatives and metas
were archived after reference/load/generator checks. No authoring masters or
global quality were reduced. Windows validation cannot certify phone size/FPS.

The current limit is **1 GB**, with no lower development target. Current audit
requirements are defined by [AGENTS.md](../AGENTS.md). The records below are historical evidence, not instructions
to repeat size analysis. The current measured release and its signature are recorded below and in
[build size](BUILD-SIZE.md).

## Main menu merge - 17 October 2026

Merged incoming `ae9dc3b` (sports menu, startup welcome, persistent guest names,
football passing and basketball restarts) with the validated Fishing hold controls
and resilient content preparation. Incoming networking is preserved, with the
combined protocol raised to **67**: both parent deliveries independently used 66
for different serialized gameplay records, so mixed builds must not connect.
Fishing keeps its current camera, rod, rope, swimming and splash presentation.
The welcome screen now uses the existing safe preparation lease, checksum and
length validation, direct desktop source read and owned temporary-file cleanup.

The complete `Builds/WindowsFinal/` player and AndroidSubmission were freshly
rebuilt from this combined source. All seven integration suites pass, reporting
**4,659 assertions/checks** across startup/profile layouts, two-player names,
offline/online sports menus, Fishing controls/network, results, interrupted
copy-source preparation/retry and basketball restarts. All **406 C# files under `Game/Assets/`**
remain unchanged between the Windows validation snapshot and Android build.
The initial probes still expected removed inline sport tabs and old loading
control names. They now use the guide hub, header Back target and the welcome's
Retry button. Storage-recovery guidance was restored on the welcome error card.
Guarded scripts-only refreshes preserved native player data, catalog and bundle
hashes. Name/menu tests passed on unchanged code before these corrections;
Fishing/results passed after the navigation correction. Startup/profile layouts
were rerun after the storage message change, followed by recovery and restarts.
Earlier failed probe evidence is retained for diagnosis.
Generated scene/prefab graphs were compared by named objects and component references.
Only Bootstrap protocol 49 -> 67 changed semantically; equivalent ID/order churn
was restored to the incoming serialization. Native player content is unchanged.
The incoming six menu/welcome PNGs pass their recorded hashes and PNG CRCs.
Rendered welcome, retry, sports-menu and hooked-Fishing captures were visually
inspected on Windows.
Documentation links also pass from a relocated folder with spaces and an
unrelated working directory. Runtime evidence is under
`Builds/GitSync/Task-20261016-200323/MenuMerge/`.

APK: **75,053,357 -> 78,456,347 bytes
(+3,402,990)**, built 2026-10-17T01:56:46 Malaysia time.
It retains **21,543,653 bytes below 100 MB**, satisfying
the task's stricter budget as well as the repository's current 1 GB gate.
The old 75 MB target has -3,456,347 bytes of reserve;
the incoming repository policy retires that target. Independent APK v2 signature,
ARM64-only libraries and embedded five-scene bundle length/SHA-256 checks pass.
APK SHA-256: `0E8979906AD38453F6F7B61D91F88F39C8C08DBC050BF2A31202F5EB0A0A094C`.

Physical Android touch, keyboard, performance, phone cutouts and WAN behavior
remain unverified. Huawei login/cloud recovery is still coming soon; the new
guest profile is local to the device. Superseded complete players/APKs were
archived with the repository tool; batch paths are in the delivery stamp.

Measured growth is principally the incoming menu/welcome artwork. The three new
UI textures total **3,286,535 compressed bytes**; the scene bundle grows by only **5,031 compressed bytes**.

| Largest actual compressed APK entries | Bytes |
| --- | ---: |
| `assets/PackagedContent/islands.bundle` | 42,396,169 |
| `lib/arm64-v8a/libil2cpp.so` | 12,137,139 |
| `lib/arm64-v8a/libunity.so` | 9,193,322 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,728,470 |
| `assets/bin/Data/ea98f7a1d32efd144a9ac765d6e2a920` | 1,432,392 |
| `assets/bin/Data/f1f277fb7d419384f835cb091df49952` | 1,366,171 |
| `assets/bin/Data/35850a973745b614aa108f2ab1f630a5` | 909,802 |
| `assets/bin/Data/Resources/unity default resources` | 798,971 |

## Main sync and Fishing integration - 16 October 2026

Fresh AndroidSubmission at **21:41:00 Malaysia time** measures
**73,459,821 -> 75,053,357 bytes (+1,593,536)**. Independent APK v2 signature verification
passes. SHA-256: `EB1E8A61B41F130AA470EE259B402B9ED8BC003D6B05D75D57E98A96DFA11377`.
The APK is **24,946,643 bytes below 100 MB**, also satisfying
the incoming repository's 1 GB gate. This is a measured APK, rather than a source
folder estimate. The older 75 MB target has -53,357 bytes
of reserve against this build; the incoming repository policy retires that target.

Actual compressed ZIP payload delta: **+1,590,776 bytes**;
container/signing overhead delta: **+2,760 bytes**.
ZIP entries: **795**. The bundle length and SHA-256 match
the embedded catalog, which contains Bootstrap and all four island scenes.
Only ARM64 native libraries are present.

| Largest actual compressed APK entries | Bytes |
| --- | ---: |
| `assets/PackagedContent/islands.bundle` | 42,391,138 |
| `lib/arm64-v8a/libil2cpp.so` | 12,092,950 |
| `lib/arm64-v8a/libunity.so` | 9,193,322 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,722,535 |
| `assets/bin/Data/35850a973745b614aa108f2ab1f630a5` | 909,802 |
| `assets/bin/Data/Resources/unity default resources` | 798,971 |

The growth includes the incoming character, replay, spectator and travel work.
This integration adds no image, model, recorded audio or package. Generated
Windows scene/material/proxy records were refreshed by the existing builder.
Serialized asset-report totals remain distinct from compressed APK contributions.
Current reports are in `Builds/SizeAudit/latest/`; measured ZIP and source
integrity evidence is in `Builds/GitSync/Task-20261016-200323/`.

## Abrupt guest disconnect recovery - 15 October 2026

This intermediate transport-only AndroidSubmission at **21:35:54 Malaysia time**
was superseded by the final build, which also guards teleport cleanup.
The intermediate APK is **75,006,437 bytes**:
**-7,900** from the opening **75,014,337**. Hard-limit headroom is
**24,993,563 bytes**; the development target is **6,437 bytes over**.
The enforced budget and independent APK v2 signature pass. ZIP entry count is
unchanged at **795**. SHA-256:
`9427D978B4F671657060C6A9B91DF7226355954E4586CD9B2D1E3F5C4E82A2B7`.

| Entry | Compressed bytes | Change |
| --- | ---: | ---: |
| Island bundle | 42,399,892 | -4,652 |
| IL2CPP | 12,047,024 | +99 |
| Unity runtime | 9,192,799 | 0 |
| Managed metadata | 2,718,056 | -3 |

Every bundled asset has the same serialized byte count as the opening audit.
Bundled assets remain **148,676,453 bytes** and player assets **12,240,598 bytes**;
neither figure is the compressed APK size. The net reduction is lossless packing
variation, not asset removal or reduced visual quality. There is no avoidable
new asset payload to trim in this change, and the remaining development-target
excess is explicitly recorded rather than treated as spare budget.

The existing Unity Transport 2.7.3 package is now embedded with a buffer-release
backport, retaining its license and upstream source. Its documentation and
samples do not become new game assets. The Windows DLL source hash and Android
stripped assembly both confirm the two buffer-release paths are compiled.
The capacity probe remains development-only.

Intermediate evidence: `Builds/DisconnectQA/transport-only-release-audit.json`
and `build-android-transport-only.log`. The final APK and its fresh signature
are recorded in `Builds/DisconnectQA/final-release.json`,
`WhatTheFish-release.apk`, `apk-signature.txt` and `build-android.log`.
No Android device was attached; phone rendering, memory, touch and frame rate
remain unverified.

## Spectator rooms and sky arrivals - 15 October 2026

AndroidSubmission at **19:58:15 Malaysia time** is **75,014,337 bytes**:
**+28,120** from the opening **74,986,217**, or **+11,080** from the intervening
75,003,257-byte release. Hard-limit headroom is **24,985,663**; the development
target is **14,337 bytes over**, not achieved. The size gate and independent
APK v2 signature pass. ZIP entry count remains **795**. SHA-256:
`383141E730D7740B8E94CDDF5FAD2C4907AE0FB889EC9F567ED9A35BB07621BA`.

Largest compressed entries, with changes from the opening APK:

| Entry | Compressed bytes | Change |
| --- | ---: | ---: |
| Island bundle | 42,404,544 | +6,515 |
| IL2CPP | 12,046,925 | +18,563 |
| Unity runtime | 9,192,799 | 0 |
| Managed metadata | 2,718,059 | +2,699 |

Serialized player assets total **12,240,598** bytes; serialized bundled assets
total **148,676,453** bytes. These are separate measurements from the APK.
Comparing every bundled asset with the opening audit finds only the network
character prefab changing, **10,496 -> 10,520 bytes**. Existing textures,
meshes, audio and other bundled asset contributions are unchanged. Small
lossless bundle-packing changes accompany the code/schema change. No new art
or dependency was added, and visual quality was not reduced to offset it.
Development review code is excluded from the Android release. Waiting-room
movement traffic is also removed to avoid unnecessary work for twenty peers.

Evidence: `Builds/SpectatorQA/release-audit.json`, `entry-comparison.csv`,
`bundle-asset-comparison.csv`, `SizeAuditBefore/`, `SizeAuditIntervening/`,
`SizeAuditFinal/`, `apk-signature.txt`, `build-android-final.log`, `Before.apk`
and `WhatTheFish-release.apk`. The opening delta includes concurrent pickup
work. No Android device was attached; phone quality and performance remain
unverified. See [verification](VERIFICATION.md#spectator-rooms-and-sky-arrivals---15-october-2026)
for the local UDP forced-termination limitation and tested normal behavior.

## Forgiving prop pickups - 15 October 2026

Actual AndroidSubmission at **19:10:43 Malaysia time** is **75,003,257 bytes**,
up **17,040** from the opening **74,986,217-byte** APK. Hard-limit headroom is
**24,996,743**; the development target is **3,257 bytes over**, not achieved.
The hard size gate and independent v2 signature verification pass. Entry count
is **795**, unchanged. Signed task snapshot: `Builds/PropPickupQA/WhatTheFish-release.apk`.

Largest compressed entries: island bundle **42,395,586 (-2,443)**, IL2CPP
**12,044,964 (+16,602)**, Unity **9,192,799 (unchanged)**, and managed metadata
**2,717,902 (+2,542)** bytes. Player serialized assets are **12,240,598** and
bundle serialized assets **148,676,453 bytes**; these are not the APK size.

The inspected growth is native code/metadata in the combined room-entry,
seating/spectator and pickup snapshot. The pickup change adds no asset or
shipped dependency: one existing radius constant changes and two spawn-frame
returns are removed. Development review methods are excluded from Android
release. No visual quality was reduced, and no new texture/audio payload was
introduced. The small development-target excess is recorded explicitly; the
hard-limit reserve must still be conserved for subsequent work.

Evidence: `Builds/PropPickupQA/release-audit.json`, `SizeAuditBefore/`,
`SizeAuditFinal/`, `Before.apk`, `apk-signature.txt` and `build-android-final.log`.
No Android device is connected; phone appearance and performance are unverified.

## Room teleport, post-match choices and cabin lighting - 15 October 2026

Actual signed AndroidSubmission at **18:06:41 Malaysia time** is
**74,986,217 bytes**, compared with the opening **74,943,705 bytes**
(**+42,512**). Development reserve is **13,783 bytes** and strict-limit headroom
is **25,013,783 bytes**. The enforced budget and APK v2 signature pass.
SHA-256: `6CEEDCE74FF4F2BF067340C49D65C5EE8C39F3FA965C9DE6504FB60EF0DA563B`.

Largest compressed entries are the island bundle **42,398,029** (+11,085),
IL2CPP **12,028,362** (+26,567), Unity **9,192,799** (unchanged), and managed
metadata **2,715,360 bytes** (+3,999). Serialized player assets total
**12,240,118 bytes**, and serialized bundled assets **148,676,429 bytes**;
these are separate from compressed APK size. Shared growth includes the
concurrent football controls/pass guide/pressure work.

Post-match cards reuse the existing island illustration atlas and fonts.
Teleport retains its shared procedural effects and existing character assets.
The bulb uses an existing URP unlit shader variant, drawn after the opaque
ambient pass with solid colour and depth testing. No new texture, mesh, audio,
animation clip or package was added. The final draw-order fix adds only
**572 bytes** over the preceding 74,985,645-byte task candidate.

Evidence: `Builds/RoomTeleportQA/baseline.json`, `SizeAuditBefore/`,
`SizeAuditFinal/`, `entry-comparison.csv`, `release-audit.json`,
`apk-signature.txt` and `WhatTheFish-release.apk`. Close cabin, bulb and island
views were inspected on Windows. No physical Android device was attached;
phone rendering and performance are not established by those captures.

## Football swipe, pass guide and pressure - 15 October 2026

Signed AndroidSubmission at **17:36:28 Malaysia time** is **74,980,797 bytes**,
up **37,092** from the observed **74,943,705-byte** baseline. Hard-limit
headroom: **25,019,203**; development reserve: **19,203 bytes**. The APK gate
and independent v2 signature verification pass. There are **795 ZIP entries**,
unchanged from the baseline.

Largest compressed entries: island bundle **42,392,002 (+5,058)**, IL2CPP
**12,029,011 (+27,216)**, Unity **9,192,799 (unchanged)**, managed metadata
**2,715,321 (+3,960)** bytes. Player serialized assets are **12,240,118**;
bundle serialized assets are **148,641,645**. Those serialized figures are
not compressed APK sizes. Entry comparisons show code/metadata and small
bundle growth; no new texture/audio payload was introduced by this task.

The football pass guide and lock label reuse procedural UI geometry, shared
plates and the existing sports font. Swipe routing and capped pressure are
code changes. The combined snapshot also includes concurrent room teleport,
post-match and cabin changes, so this delta cannot be attributed solely to
football. No unrelated visual quality was reduced to meet the target.

Evidence: `Builds/FootballSwipeQA/release-audit.json`, `SizeAuditBefore/`,
`SizeAuditFinal/`, `apk-signature.txt`, `Before.apk` and
`WhatTheFish-release.apk`. Physical Android quality/performance is unverified.

## Character head refinement - 15 October 2026

Signed AndroidSubmission at **16:54:23 Malaysia time** is **74,943,705 bytes**,
up **189,388** from the observed **74,754,317-byte** opening APK. Hard-limit
headroom is **25,056,295**; development reserve is **56,295 bytes**. The APK
budget gate and independent v2 signature verification pass. This release has
**795 ZIP entries**. The opening entry inventory and APK hash were recorded
before edits; comparisons include concurrent game changes.

Largest compressed entries: island bundle **42,386,944**, IL2CPP **12,001,795**,
Unity **9,192,799**, and managed metadata **2,711,361 bytes**. Compared with the
opening APK these change by **+131,484**, **+23,871**, **-229**, and **+3,727**
respectively. Player serialized assets are **12,239,518 bytes**, and bundle
serialized assets are **148,641,629 bytes**; these are not compressed APK sizes.

Generated identity `.asset` inventory rises from **199,992 to 818,128 serialized
bytes**. This includes the existing digit atlas/library and the refined meshes;
the largest mesh is the near curly head at **119,224 bytes**. The shared
CharacterHead shader is **31,804 serialized bytes**. The original body FBX's
serialized inventory stays **2,940,648 bytes**. This refinement adds no texture,
audio or animation clip, and does not duplicate body meshes or rig libraries.

The generator removes **27,671 fully buried triangles**, retaining **68,574**
across the twenty near/far attachment and scalp meshes. Medium mesh compression
preserves the inspected silhouettes. The ear mask uses an otherwise unused
negative facing coordinate; unused head number coordinates are canonicalized,
while every triangle that can contribute to the fringe keeps its height data.
This final packing reduces the preceding **75,142,577-byte** candidate by
**198,872 bytes**, without reducing textures or animation quality. Both LODs,
close-up joins and gameplay views were reviewed after packing.

Evidence: `Builds/CharacterRefinementQA/Before/`, `SizeAuditFinal/`,
`release-audit.json`, `entry-comparison.json`, `asset-comparison.json`,
`geometry-audit.json`, `apk-signature.txt` and `WhatTheFish-release.apk`.
Physical Android appearance, memory and FPS remain unverified.

## Blue-light teleport preview - 15 October 2026

Actual signed AndroidSubmission at **16:31:03 Malaysia time**:
**75,142,885 bytes**. Opening observed APK: **74,756,265**; growth: **386,620**.
Archived baseline: **74,754,317**; growth: **388,568**. Hard-limit headroom:
**24,857,115 bytes**. Development target exceeded by **142,885 bytes**.
Budget enforcement and independent APK v2 verification pass.

Largest compressed entries are the island bundle **42,586,120**, IL2CPP
**12,001,795**, Unity **9,192,799** and managed metadata **2,711,361 bytes**.
Entry count is **795**, previously 791. Player serialized assets change from
**12,190,550 to 12,239,518 bytes**; bundle serialized assets change from
**147,991,237 to 148,641,629 bytes**. These are uncompressed inventory values,
not the APK measurement.

TeleportCharacter shader occupies **38,756 serialized bytes**, TeleportLight
**8,788**, their two materials **832**, and their script metadata **488**.
The effect adds no textures, audio, animation clips, packages or stored meshes.
Short-lived material instances share the existing character textures; light
geometry is shared and generated at runtime. The temporary station cabin shares
the existing cabin meshes/materials. Source and destination detail scenes overlap
only during the preview; phone peak-memory behavior still needs device testing.

The largest shared asset growth comes from concurrent scalp/hair meshes and
the CharacterHead shader. The final shared APK is **207,936 bytes smaller** than
the interim 75,350,821-byte measurement after adopting the refined head packing.
No unrelated visual assets were removed to conceal the development-target overage.
An intervening 75,094,373-byte character release is preserved in
`Builds/TeleportQA/SizeAuditBeforeFinal/`; the final delta from it is **48,512**.
Shared changes and compression prevent assigning that entire delta to one feature.

Evidence: `Builds/TeleportQA/SizeAuditFinal/`, `asset-comparison.json`,
`entry-comparison.json`, `apk-comparison.json`, `apk-signature.txt` and the
task-specific `WhatTheFish-release.apk`. SHA-256:
`F28EF3C4996CA79943979B0AFC272DB8EFDD5CBF83524E0D291C434B8AC3B53F`.

## Match exit boundaries - 15 October 2026

Signed APK: **74,754,317 -> 74,810,941 bytes (+56,624)**,
built at **15:46:57 Malaysia time**. Hard-limit headroom: **25,189,059**;
development reserve: **189,059 bytes**. Budget and v2 signature pass.

The boundary change adds no texture, mesh or audio. Growth includes concurrent
teleport code/materials/shaders: TeleportCharacter shader 37,340 serialized bytes,
TeleportLight shader 8,788, and their materials 784 combined. The new development
check has a 104-byte script metadata record. No unused media was introduced by
this task. Both mesh inventories are unchanged; bundle serialized assets remain
147,991,237 bytes. Player serialized assets: 12,190,550 -> 12,238,054 bytes.
These uncompressed inventory measurements are distinct from APK bytes.

Largest compressed entries: islands bundle **42,259,598**, IL2CPP **11,997,463**,
Unity **9,192,799**, managed metadata **2,711,373 bytes**. Entry count: **795**,
previously 791. Evidence: `Builds/MatchExitQA/SizeAuditFinal/`,
`apk-comparison.json`, `entry-comparison.json`, `asset-comparison.json` and
`apk-signature.txt`. SHA-256: `9841BC0103E22D4096B7C0D6A733649FBC25385D81299C4C6DF3BCD2612FBF9D`.

## Basketball statistics and courtside MVP - 15 October 2026

Actual signed APK at **15:25:29 Malaysia time**:
**74,728,161 -> 74,754,317 bytes (+26,156)**.
Development reserve: **245,683 bytes**. Hard-limit headroom:
**25,245,683 bytes**. AndroidSubmission and APK v2 verification pass.

| Largest APK entries | Opening compressed bytes | Final compressed bytes |
| --- | ---: | ---: |
| Embedded five-scene bundle | 42,252,709 | 42,255,460 |
| IL2CPP ARM64 library | 11,956,088 | 11,977,924 |
| Unity ARM64 library | 9,193,028 | 9,193,028 |
| Managed metadata | 2,706,317 | 2,707,634 |

Entry count remains **791**. Player serialized assets change from
**12,190,134 to 12,190,550 bytes**; the bundle changes
from **147,991,229 to 147,991,237 bytes**.
Both mesh inventories are unchanged. These are uncompressed serialized asset
measurements, distinct from APK bytes. Growth is predominantly compiled code;
the small script metadata records and network component serialization are expected.

The screen shares the ten existing portraits and basketball icon atlas, uses
the live arena, and draws basketball markings in UI geometry. No additional
textures, meshes, audio or packages ship. No unrelated visual quality was reduced.
Evidence: `Builds/BasketballHonoursQA/SizeAuditFinal/`, `apk-comparison.json`,
`entry-comparison.json`, `asset-comparison.json` and `apk-signature.txt`.
Physical Android appearance, touch and performance remain unverified.
SHA-256: `65788D64FE66042BAACF6DF7AEA26459640692511201C888C1ACB10CFDF4549D`.

## Football passing and illustrated MVP - 15 October 2026

Actual signed APK at **14:15:27 Malaysia time**: **74,728,161 bytes**,
up **740,148** from the task-opening 73,988,013-byte build and **707,776**
from the intervening trail release. Remaining development reserve: **271,839
bytes**. Strict-limit headroom: **25,271,839 bytes**. AndroidSubmission and
independent APK v2 verification pass.

| Largest APK entries | Opening compressed bytes | Final compressed bytes |
| --- | ---: | ---: |
| Embedded five-scene bundle | 42,257,975 | 42,252,709 |
| IL2CPP ARM64 library | 11,929,212 | 11,956,088 |
| Unity ARM64 library | 9,193,097 | 9,193,028 |
| Managed metadata | 2,702,660 | 2,706,317 |

There are **791 entries**, previously 779. Ten MVP portrait textures use
512-pixel ASTC 6x6 imports with alpha and no mipmaps; high-resolution PNG
masters remain editable in Git LFS. The portraits account for the main growth.
No meshes, environment copies, audio banks or packages were added for MVP.
Player serialized assets grow **832,232 bytes**, from 11,357,902 to **12,190,134**;
the bundle grows **8 serialized bytes**, to **147,991,229**. Both mesh inventories
are unchanged. These inventories are uncompressed serialized sizes, not APK bytes.

The new screen shows the existing stadium through one pitch-level camera,
with a sprite for the winning character. The superseded render-texture model
was archived. No unrelated visual quality was reduced. Full and compact
Windows renders were inspected; Android compressed visual quality and device
performance remain unverified. Evidence: `Builds/FootballPlaymakerQA/SizeAuditFinal/`,
`apk-comparison.json`, `entry-comparison.json`, `asset-comparison.json`,
`portrait-packed-assets.json` and `apk-signature.txt`.

SHA-256: `EB49D9B173236EA23BBF3EB1DC8F8698EEEB854AA4DDB7C6508D8033454B0026`.

## Personal football trails - 15 October 2026

Actual signed APK at **13:39:35 Malaysia time**:
**73,988,013 -> 74,020,385 bytes (+32,372)**. Development reserve is **979,615
bytes** and strict-limit headroom is **25,979,615 bytes**. AndroidSubmission and
independent APK v2 verification pass. SHA-256:
`6737E37E5B48912D0A496CDE0FE52E4254BF886FA2CC424C8E7A9BE77A855ABF`.

| Largest APK entries | Opening compressed bytes | Current compressed bytes |
| --- | ---: | ---: |
| Embedded five-scene bundle | 42,257,975 | 42,250,838 |
| IL2CPP ARM64 library | 11,929,212 | 11,960,528 |
| Unity ARM64 library | 9,193,097 | 9,194,136 |
| Managed metadata | 2,702,660 | 2,706,570 |

There are **781 entries**, previously 779. The new trail shader occupies **9,960
uncompressed serialized bytes** and its resource material **112 bytes**. Five
cached runtime material variants supply lightning, blue ribbons, pink stars,
flame filaments and aurora waves; the UI previews reuse the same shader. No new
texture, mesh, audio bank, particle asset or package ships with the trails.

Player serialized assets grow **11,088 bytes**, from 11,357,902 to **11,368,990**.
The bundle grows only **8 serialized bytes**, from 147,991,221 to **147,991,229**,
in the concurrent network-player prefab change. Both mesh inventories are
unchanged. Existing ball geometry is scaled through its prefab; editable model
bytes and pigments are retained. These serialized inventory figures are not
compressed APK sizes.

Runtime code/metadata and the small shader account for the main growth; no
unexpected large dependency was found. The same working tree includes concurrent
passing, controls and MVP changes, so the full delta cannot be attributed solely
to trails. No unrelated visual quality was reduced. Windows checks include
actual close/wide views and compact landscape previews, but do not establish
phone quality, RAM or FPS; `adb devices` lists no connected device.

Evidence: `Builds/FootballTrailQA/apk-comparison.json`, `asset-comparison.json`,
`entry-comparison.json`, `apk-signature.txt`, `SizeAuditBefore/`, `SizeAuditFinal/`
and [verification](VERIFICATION.md#personal-football-trails---15-october-2026).

## Longer football transitions - 15 October 2026

Actual AndroidSubmission APK at **02:34:35 Malaysia time**:
**73,991,313 -> 73,988,013 bytes (-3,300)**. Development reserve is
**1,011,987 bytes**; hard-limit headroom is **26,011,987 bytes**.
The enforced gate and independent APK v2 signature verification pass.
SHA-256: `2A5FE9F9529AD344062771496CD1B9CD17C7C7E77570AAE9398E42FCB1CF4643`.

| Largest APK entries | Previous compressed bytes | Current compressed bytes |
| --- | ---: | ---: |
| Embedded five-scene bundle | 42,262,403 | 42,257,975 |
| IL2CPP ARM64 library | 11,928,252 | 11,929,212 |
| Unity ARM64 library | 9,193,097 | 9,193,097 |
| Managed metadata | 2,702,486 | 2,702,660 |

There are still **779 entries**. Compressed payload decreases by **3,300 bytes**,
matching the actual APK delta. The regenerated bundle is 4,428 bytes smaller;
IL2CPP grows 960 bytes and managed metadata grows 174 bytes. No art quality or
compression setting was reduced for this timing change. Both serialized asset
inventories are identical: player **11,357,902 bytes**, bundle **147,991,221
bytes**. Both mesh inventories are unchanged. These are uncompressed inventory
values, not APK measurements. No new shipped texture, geometry, audio bank or
package was introduced, and no unexpected asset growth was found to remove.

Evidence: `Builds/FootballReplayTimingQA/apk-comparison.json`,
`entry-comparison.json`, `asset-comparison.json`, `apk-signature.txt`,
`SizeAuditBefore/` and `SizeAuditFinal/`. The current audit remains in
`Builds/SizeAudit/latest/`. See
[verification](VERIFICATION.md#longer-football-transitions---15-october-2026)
for timing traces, complete player delivery and source/device limits.

## Ten player identities - 15 October 2026

Actual AndroidSubmission APK at **02:11:35 Malaysia time**:
**73,482,037 -> 73,991,313 bytes (+509,276)**. The development reserve is
**1,008,687 bytes** and hard-limit headroom is **26,008,687 bytes**.
Enforced build gate and independent APK v2 verification pass. SHA-256:
`701974B45CE7F671149E9DA9B729E0928CF7350AAA05E6EAE0073B9E212186AA`.

| Largest APK entries | Previous compressed bytes | Current compressed bytes |
| --- | ---: | ---: |
| Embedded five-scene bundle | 41,771,295 | 42,262,403 |
| IL2CPP ARM64 library | 11,910,661 | 11,928,252 |
| Unity ARM64 library | 9,193,092 | 9,193,097 |
| Managed metadata | 2,700,748 | 2,702,486 |

The bundle grows **491,108 bytes**, mainly the body's derived color/number
vertex channels plus small shared head meshes. There are **779 APK entries**
(previously 781). No duplicate character texture set or animation library was
introduced. Bundle serialized assets grow from 146,871,897 to **147,991,221**
bytes; player serialized assets are **11,357,902** bytes. These inventory values
are uncompressed and must not be presented as APK size.

The original body FBX's delivered two meshes grow **879,340 serialized bytes**
for region masks and number coordinates; source geometry, skinning, grips and
animation files are unchanged. New shared identity assets total **202,208
serialized bytes**, including **178,916 bytes** for twenty attachment/scalp
meshes and **20,612 bytes** for the single-channel digit atlas. The identity
shader is **37,628 serialized bytes**. Original character textures retain their
sizes and references. There are still **nine AnimationClips (435,636 bytes)**,
with **zero character animation sprites**.

Direct character storage, including the new identity assets/shader, is
**7,403,080 serialized bytes**. Isolated LZMA compression gives **4,499,317 bytes**;
this is an estimate, not an exact allocation within the shared-compression APK.
The prior direct subtotal was 6,283,868 bytes (isolated estimate 4,016,022).
Procedural animation code, engine/shared dependencies and held equipment are
outside these direct totals. No tenfold multiplication occurs for ten players.

The first complete identity APK was 74,006,669 bytes. Quantized jersey coordinates
and subsequent HUD/code changes reduced it **15,356 bytes** without changing body
positions or animation data. Final growth exceeds the 500,000-byte engineering
target by **9,276 bytes**, while remaining below the 75,000,000-byte working target.
No unrelated art was reduced to close this small difference.

Evidence: `Builds/CharacterIdentityQA/release-audit.json`, `character-storage-final.json`,
`BeforeReleaseAudit/`, `SizeAuditInitial/`, `SizeAuditFinal/` and `apk-signature.txt`.
The asset extraction verifies the packaged bundle's SHA-256 against its catalog.
The opening 73,446,301-byte APK predates the intervening replay release; comparison
against that opening build includes both features (+545,012 bytes).

## Illustrated football goal replay - 15 October 2026

Actual APK at **00:37:32 Malaysia time**:
**73,446,301 -> 73,482,037 bytes (+35,736)**. Development reserve is
**1,517,963 bytes** and hard-limit headroom is **26,517,963 bytes**.
AndroidSubmission and independent APK v2 verification pass. SHA-256:
`FDA8B748C4919FE05441A8A35D0E2EE320E9D4CF890A15726B6D46D099F5A216`.

| Largest APK entries | Opening compressed bytes | Current compressed bytes |
| --- | ---: | ---: |
| Embedded five-scene bundle | 41,770,179 | 41,771,295 |
| IL2CPP ARM64 library | 11,882,138 | 11,910,661 |
| Unity ARM64 library | 9,190,940 | 9,193,092 |
| Managed metadata | 2,696,491 | 2,700,748 |

There are **781 entries**, compared with 779 before. Compressed payload grows
by **35,401 bytes**, with **335 bytes** additional container/signing overhead.
Most growth is IL2CPP code (+28,523) and managed metadata (+4,257).
Player serialized assets increase **408 bytes**, from **11,356,958 to
11,357,366**, entirely four replay script records. Bundle serialized assets
remain **146,871,897 bytes** and both mesh inventories are unchanged. These
uncompressed inventory values are not APK measurements.

The larger transition logo reuses the existing sprite; football, shot-trail,
net and celebration drawings use native UI geometry. Playback shares existing
meshes, materials and audio. No shipped art/dependency growth was found to
remove, and the four-island content and lossless packaging remain intact.

Evidence: `Builds/FootballReplayQA/apk-comparison.json`, `entry-comparison.json`,
`asset-comparison.json`, `apk-signature.txt`, `SizeAuditBefore/` and
`SizeAuditFinal/`. The promoted current audit is in `Builds/SizeAudit/latest/`.
This release was built from `Builds/FootballReplayQA/SourceSnapshot` to exclude
concurrent unfinished character-identity changes. See
[verification](VERIFICATION.md#illustrated-football-goal-replay---15-october-2026)
for compiled-source checks, complete Windows delivery and device limits.




## Fishing two-button Hold the Glow restoration - 15 October 2026

Fresh AndroidSubmission written **22:14:33 Malaysia time** measures
**73,456,241 -> 73,459,821 bytes (+3,580)**.
Headroom to the strict 100,000,000-byte boundary: **26,540,179 bytes**.
Development reserve below 75,000,000: **1,540,179 bytes**.
The enforced build limit and independent APK v2 signature verification pass.
SHA-256: `79D7A6F34CF6072214C3E5975EB542B8FE078C3156D0A70C037A1BB2752AFA54`.

Fishing restores the pictured **Hold & Countersteer** controls: one Left and
one Right button. Hold the glowing direction, switch when it changes and release
both to cool the line. The fish/gold-band meter and clear navy Catch/Tension
captions return. Timed-tap gating is removed. Continuous winding uses the earlier
law with a modest 20% speed assist. Correct-side reel drag can slip under a
sudden load to leave reaction time; spring, accumulated strain and fish forces
remain, and players still release to cool.
The shared tutorial uses the same live controls and opens once for the revision.

Current camera switching and directional framing, rod grip/lean, attached rope,
fish swimming, gravity-driven splashes and landing feedback are retained. Scoring,
Wanted, Round/Cup/Crew, six abilities/two favourites, four islands and shared room
travel remain. Matching peers require protocol **57**. No texture, font, model,
shader, recorded audio, package or realtime light is added.
[Controls and simulation](FISHING-COUNTERSTEER.md).

Actual compressed ZIP payload delta: **+3,582 bytes**;
container/signature overhead delta: **-2 bytes**.
ZIP entries: **779 -> 779**.
Both player and bundle inventories are inspected. Combined mesh rows remain
**1,788 -> 1,788**, with identical geometry,
compression and bounds; no new texture is shipped. Serialized report sizes are
uncompressed measurements, not APK size contributions.

| Largest actual compressed APK entries | Bytes |
| --- | ---: |
| `assets/PackagedContent/islands.bundle` | 41,771,138 |
| `lib/arm64-v8a/libil2cpp.so` | 11,893,182 |
| `lib/arm64-v8a/libunity.so` | 9,190,935 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,698,136 |
| `assets/bin/Data/35850a973745b614aa108f2ab1f630a5` | 909,802 |
| `assets/bin/Data/Resources/unity default resources` | 798,971 |

Embedded catalog, bundle byte count, SHA-256 and five scene names match the
build audit. Forbidden desktop pipeline/SSAO/BlueNoise256 dependencies remain
absent from the combined Android inventories; development review types are
stripped. Current raw evidence is in `Builds/SizeAudit/latest/` and
`Builds/FishingHold/Task-20261015-201923/`.

## Local content startup recovery - 15 October 2026

Fresh AndroidSubmission written **17:17:47 Malaysia time** measures
**73,452,461 -> 73,456,241 bytes (+3,780)**.
Headroom to the strict 100,000,000-byte boundary: **26,543,759 bytes**.
Development reserve below 75,000,000: **1,543,759 bytes**.
The enforced build limit and independent APK v2 signature verification pass.
SHA-256: `7F39D1F12038005F6028A80600148A792394C8C1442E263DBA164358535A5FE6`.

First-launch preparation reads an embedded filesystem bundle directly,
avoiding an unnecessary compressed-source copy on Windows. APK archive assets
retain their required temporary copy. Both paths retain length/SHA-256 and
conversion-CRC checks. Preparation is serialized between launches; owned failed
and interrupted temporary files are cleaned. Obsolete hash caches are pruned
after the current content successfully loads, preserving other files and saves.
The loading screen offers a usable storage/retry message. All four islands,
the existing Fishing gameplay and protocol **56** are retained. No asset,
recorded audio, model, shader, texture, package or realtime light is added.
[Loader and storage behavior](CONTENT-PACKAGING.md#first-launch-and-storage).

Actual compressed ZIP payload delta: **+3,776 bytes**;
container/signature overhead delta: **+4 bytes**.
ZIP entries: **779 -> 779**.
Both player and bundle inventories are inspected. Combined mesh rows remain
**1,788 -> 1,788**, with identical geometry,
compression and bounds; no new texture is shipped. Serialized report sizes are
uncompressed measurements, not APK size contributions.

| Largest actual compressed APK entries | Bytes |
| --- | ---: |
| `assets/PackagedContent/islands.bundle` | 41,765,418 |
| `lib/arm64-v8a/libil2cpp.so` | 11,895,370 |
| `lib/arm64-v8a/libunity.so` | 9,190,935 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,698,087 |
| `assets/bin/Data/35850a973745b614aa108f2ab1f630a5` | 909,802 |
| `assets/bin/Data/Resources/unity default resources` | 798,971 |

Embedded catalog, bundle byte count, SHA-256 and five scene names match the
build audit. Forbidden desktop pipeline/SSAO/BlueNoise256 dependencies remain
absent from the combined Android inventories; development review types are
stripped. Current raw evidence is in `Builds/SizeAudit/latest/` and
`Builds/LaunchRecovery/Task-20261015-165153/`.

## Fishing timed pulls and directional camera - 15 October 2026

Fresh AndroidSubmission written **16:14:33 Malaysia time** measures
**73,442,073 -> 73,452,461 bytes (+10,388)**.
Headroom to the strict 100,000,000-byte boundary: **26,547,539 bytes**.
Development reserve below 75,000,000: **1,547,539 bytes**.
The enforced hard limit and independent APK v2 signature verification pass.
SHA-256: `E94EAA6B2F1C8A6FA1BB88BCC4C34532A4A7D1B5463CF249AFA24848B82AE940`.

Tap Reel when the moving circle enters gold. Every third sweep asks for a
timed Left/Right counterpull through the same controls. Each sweep allows one
attempt; holding and spam never repeat a stroke. Clear arrow cues brighten at
the tap window, with contained hit/miss feedback and no keyboard guides. The
rod, torso and fish forces respond to accepted pulls, and a restrained camera
shoulder shift keeps the opposing direction readable. A tension-sensitive reel
clutch softens short strokes under load. Host and guest receive equal fixed
stroke duration; timestamp and duplicate guards remain host-owned. The revised
tutorial reopens once and shares the live widgets with How to Play.

Scoring, Wanted, Round/Cup/Crew goals, six skills/two favourites, existing catch
feedback, all four islands and shared room travel remain. Matching peers require
protocol **56**. No new image, model, shader, recorded audio file, package or
realtime light is added. [Controls and simulation](FISHING-TIMING.md).

Actual compressed APK ZIP payload changes **+10,388 bytes**;
container/signature overhead changes **+0**.
ZIP entries: **779 -> 779**.
Both player and scene-bundle inventories are reviewed: **1,788
-> 1,788 mesh rows**, with unchanged geometry,
compression and bounds. No new shipped texture appears. Serialized asset sizes
are uncompressed measurements, not APK byte estimates.

| Largest actual APK ZIP payload | Compressed bytes |
| --- | ---: |
| `assets/PackagedContent/islands.bundle` | 41,768,150 |
| `lib/arm64-v8a/libil2cpp.so` | 11,889,293 |
| `lib/arm64-v8a/libunity.so` | 9,190,935 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,697,657 |
| `assets/bin/Data/35850a973745b614aa108f2ab1f630a5` | 909,802 |
| `assets/bin/Data/Resources/unity default resources` | 798,971 |

The embedded catalog equals the build audit; actual bundle bytes, SHA-256, CRC
and all five scene names are checked. Combined player/bundle inventories contain
no forbidden desktop pipeline/SSAO shader/BlueNoise256 dependency. Selected
development review types are stripped from release metadata. See
`Builds/SizeAudit/latest/` and `Builds/FishingTiming/Task-20261015-151017/` for ZIP/asset deltas, source and
native payload proofs, independent signature output and final review reports.

## Fishing clarity, camera and catch feedback - 15 October 2026

Fresh AndroidSubmission written **14:31:37 Malaysia time** measures
**73,446,373 -> 73,442,073 bytes (-4,300)**.
Headroom to the strict 100,000,000-byte boundary: **26,557,927
bytes**. Development reserve below 75,000,000: **1,557,927 bytes**.
The enforced hard size gate and independent APK v2 signature verification pass.
SHA-256: `F39B0474F0F59EFFFA8F850482356F0FB26EE8BF976511742B18629343A00D56`.

Fishing now has a circular accepted-input cue and gold target socket,
explicit hold/switch/release instructions with stable cooling guidance, a framed
cast/wait/hook hint, contextual utility visibility, a smooth accepted-hook camera
with return to the chosen view, and confirmed local fish/points landing feedback.
Reduced Motion retains the chosen camera and simplifies the new effects. Held
gear stays clear of the landing card. The tutorial reuses the live circular meter.
No new image, model, shader, recorded audio file or package is added. Rules,
scoring, fish forces, timing, six skills/two favourites and host authority remain
unchanged. All four islands and shared room travel remain self-contained.
Matching peers use protocol **55**.

Actual compressed ZIP payloads change **-4,293 bytes**;
container/signature overhead changes **-7**.
ZIP entries: **779 -> 779**.
Both the player and scene-bundle asset inventories are checked; their serialized
sizes are uncompressed measurements and are not APK-size estimates.
Combined mesh rows: **1788 ->
1788**, with matching geometry, compression and
bounds. No new shipped texture is present. No lossy art or geometry change is
made to pay for the UX additions.

| Largest actual APK ZIP payload | Compressed bytes |
| --- | ---: |
| `assets/PackagedContent/islands.bundle` | 41,762,019 |
| `lib/arm64-v8a/libil2cpp.so` | 11,885,289 |
| `lib/arm64-v8a/libunity.so` | 9,190,935 |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | 2,697,453 |
| `assets/bin/Data/35850a973745b614aa108f2ab1f630a5` | 909,802 |
| `assets/bin/Data/Resources/unity default resources` | 798,971 |

The actual APK embedded catalog matches the build audit. The embedded bundle
has **41,762,019 bytes** before ZIP accounting, five
scenes, matching SHA-256 and catalog CRC. Combined player/bundle inventories
contain no forbidden desktop pipeline, SSAO shader or BlueNoise256 dependency.
Release metadata excludes the selected development-only review types.
Current reports: `Builds/SizeAudit/latest/`; before/after ZIP hashes, combined
asset/mesh comparison, signature output and source proofs: `Builds/FishingClarity/Task-20261015-131941/`.

## Local rebuild of pulled main - 14 October 2026

Fresh local AndroidSubmission written **20:45:42 Malaysia time** measures
**90,388,492 -> 73,446,373 bytes (-16,942,119)** against
the older APK on this laptop. Headroom to the strict 100,000,000-byte boundary:
**26,553,627 bytes**. Development reserve below 75,000,000:
**1,553,627 bytes**. Enforced hard limit and independent APK v2 signature pass.
SHA-256: `51723F75318AE12D75C7494F674A1A08B79DA67C2A25F119FB4933745F7C715A`.

Actual APK compressed ZIP payloads change **-16,919,221 bytes**;
container/signature overhead changes **-22,898**.
APK entries: **919 -> 779**.
The embedded scene bundle measures **41,769,305 bytes** before ZIP
container accounting; its catalog lists all five scenes, byte length, SHA-256
and CRC. The APK's embedded catalog/bundle bytes are independently checked.

The baseline APK predates lossless scene-bundle sharing. Its old player-only
asset/mesh rows cannot be compared directly to the new player-only report as
proof of deleted art. Current player serialized totals **11,357,050**
and scene-bundle serialized totals **146,871,897** are separate,
uncompressed inventories; do not add them as an APK size estimate or claim the
smaller player-only inventory describes the entire game.

| Largest scene-bundle source | Uncompressed serialized bytes |
| --- | ---: |
| `Assets/_Game/Art/RefinedIslands/Fishing/Fishing_TerrainControl.png` | 22,369,760 |
| `Assets/_Game/Art/RefinedIslands/Golf/Golf_TerrainControl.png` | 22,369,756 |
| `Assets/_Game/Art/RefinedIslands/Fishing/Fishing_WaterDepth.png` | 5,592,564 |
| `Assets/_Game/Art/RefinedIslands/Golf/Golf_WaterDepth.png` | 5,592,560 |
| `Assets/_Game/Art/Football/Sunvale/Stadium.fbx` | 3,569,800 |
| `Assets/_Game/Art/RefinedIslands/Golf/Structures.fbx` | 2,617,296 |

Existing terrain control/depth assets remain major serialized contributors;
this rebuild uses the upstream lossless dependency sharing rather than changing
their art. MobileURP/MobileRenderer remain in the player and the combined player
and scene inventory is checked for forbidden desktop delivery dependencies.
Requested development review types are absent from IL2CPP release metadata.

Current bundle/player packing CSVs, actual ZIP hashes/deltas, signature result,
source snapshots and full-player promotion proof: `Builds/LocalRebuild/Task-20261014-195856/`. No new imported
art, shader, audio or package was authored by this local rebuild. Android device
appearance/FPS, first launch and installed cache storage remain unverified.

## Golf buggy control and shot camera - 14 October 2026

Actual APK at **18:54:34 Malaysia time**:
**73,453,793 -> 73,446,301 bytes (-7,492)** from this task's opening baseline.
Development reserve is **1,553,699 bytes** and hard-limit headroom is
**26,553,699 bytes**. AndroidSubmission and independent APK v2 verification pass.
SHA-256: `E4BF71D7D11845B843582F145C0A46D773E2ABBA72B56205319FDE195BDC4DDE`.

| Largest APK entries | Opening compressed bytes | Current compressed bytes |
| --- | ---: | ---: |
| Embedded five-scene bundle | 41,782,767 | 41,770,179 |
| IL2CPP ARM64 library | 11,877,472 | 11,882,138 |
| Unity ARM64 library | 9,190,940 | 9,190,940 |
| Managed metadata | 2,696,165 | 2,696,491 |

The APK retains **779 entries**. Compressed payload decreases by **7,498 bytes**;
container/signing overhead increases by **6 bytes**. Player serialized assets
increase from **11,356,742 to 11,356,958 bytes**, entirely the concurrent
`CharacterLightingReview` (+112) and `IslandContactShading` (+104) script records.
Bundle serialized assets remain **146,871,897 bytes** and both mesh inventories
are unchanged. These uncompressed inventories are not APK measurements.
The golf work adds no art, audio, texture, mesh or shipped dependency. Four-island
content and lossless packaging are retained without reducing visual quality.

This shared-checkout build also contains concurrent sports prop-cycle and
lighting work; the opening-to-final delta is a combined measurement. Compared
with the immediately preceding 73,448,789-byte APK, this build is 2,488 bytes
smaller. There is no unexpected asset growth to remove from this task.

Evidence: `Builds/GolfCameraQA/apk-comparison.json`, `entry-comparison.json`,
`asset-comparison.json`, `apk-signature.txt`, `SizeAuditBefore/` and
`SizeAuditFinal/`. The current audit is also in `Builds/SizeAudit/latest/`.
See [verification](VERIFICATION.md#golf-buggy-control-and-shot-camera---14-october-2026)
for the complete Windows delivery, source comparison and device-testing limits.

## Live renderer lighting fix - 14 October 2026

Actual APK at **18:43:03 Malaysia time**:
**73,453,793 -> 73,448,789 bytes (-5,004)** from this task's opening baseline.
Development headroom is **1,551,211 bytes** and hard-limit headroom is
**26,551,211 bytes**. AndroidSubmission and APK v2 signature verification pass.
SHA-256: `00F95C3C789CB3BCDF83E76A7AC8391AFABAA3EA7CAEE76B884574F356501EB5`.

| Largest APK entries | Opening compressed bytes | Current compressed bytes |
| --- | ---: | ---: |
| Embedded five-scene bundle | 41,782,767 | 41,772,668 |
| IL2CPP ARM64 library | 11,877,472 | 11,882,138 |
| Unity ARM64 library | 9,190,940 | 9,190,940 |
| Managed metadata | 2,696,165 | 2,696,491 |

There are **779 APK entries**. Player-side serialized assets increase by **216
bytes**, entirely two script records, to **11,356,958 bytes**. Bundle assets remain
**146,871,897 bytes** and both mesh inventories are unchanged. These uncompressed
inventories are not APK sizes. No art imports or material settings were reduced.
The combined working-tree build also contains concurrent prop-cycle/Golf work;
do not attribute its entire byte delta to the lighting fix.

Actual Windows binary inspection finds **one** `Soft contact shading` feature
in the player and **zero** in the island bundle. Scene/prefab references to the
desktop feature were removed, and regeneration no longer recreates them. The
Android renderer remains SSAO-free. The normal shading settings are preserved;
the fix corrects runtime object identity, not texture compression.

Current evidence: `Builds/CharacterLightingQA/After/SizeAudit/`,
`apk-comparison.json`, `asset-comparison.json`, `renderer-feature-inventory.json`
and `apk-signature.txt`. See [verification](VERIFICATION.md#live-renderer-lighting-fix---14-october-2026)
for character captures, complete Windows delivery and device-testing limits.

## Continuous sports prop spawning - 14 October 2026

Fresh APK at **18:40:11 Malaysia time**:
**73,453,793 -> 73,444,625 bytes (-9,168)**. Hard-limit headroom is
**26,555,375 bytes** and development reserve is **1,555,375 bytes**.
The AndroidSubmission gate and independent APK v2 verification pass.
SHA-256: `DA0F0EEB65B860B9E7E74C719FBF6619DEF3D26BFEBA76B33F79EF5CAF71D624`.

| Largest APK entries | Previous compressed bytes | Current compressed bytes |
| --- | ---: | ---: |
| Embedded five-scene bundle | 41,782,767 | 41,768,502 |
| IL2CPP ARM64 library | 11,877,472 | 11,882,138 |
| Unity ARM64 library | 9,190,940 | 9,190,940 |
| Managed metadata | 2,696,165 | 2,696,491 |

The APK retains **779 entries**. Compressed payload decreases by **9,173 bytes**;
container/signing overhead grows by **5 bytes**. Bundle compression offsets
the small code/metadata growth. Both mesh inventories are unchanged. Bundle
serialized assets remain **146,871,897 bytes**; player serialized assets grow
from **11,356,742 to 11,356,958 bytes**, solely two script metadata rows from
concurrent `CharacterLightingReview` and `IslandContactShading` work. There is
no new prop art, texture, audio or mesh. These inventories are not APK size.

The measured build includes concurrent golf/lighting source, so its byte delta
is a combined working-tree measurement. All 247 Windows compiled documents
matched captured source at build time. Subsequent source differences are the
concurrent golf camera's `PlayerView`, `GolfAimProbe`, `CharacterLightingReview`,
platform render-resource selection and generated Golf prefab IDs. Canonical
prefab comparison confirms unchanged semantics; sports prop source is unchanged.
Evidence: `Builds/PropCycleQA/apk-comparison.json`, `asset-comparison.json`,
`source-comparison.json`, `apk-signature.txt` and `AndroidAudit/`.

## Persistent sports props and slower football shots - 14 October 2026

Actual APK at **17:43:53 Malaysia time**:
**73,446,145 -> 73,453,793 bytes (+7,648)**. Development reserve is
**1,546,207 bytes**; hard-limit headroom is **26,546,207 bytes**. The
AndroidSubmission gate and independent APK v2 signature verification pass.
SHA-256: `18CA85CD5E90D740F9CC109C2B0810733AC953C48AB972D3DE48DEF33C674A7D`.

| Largest APK entries | Previous compressed bytes | Current compressed bytes |
| --- | ---: | ---: |
| Embedded five-scene bundle | 41,773,982 | 41,782,767 |
| IL2CPP ARM64 library | 11,878,577 | 11,877,472 |
| Unity ARM64 library | 9,190,940 | 9,190,940 |
| Managed metadata | 2,696,189 | 2,696,165 |

The APK still contains **779 entries**. Compressed payload grows by **7,655
bytes**; container/signing overhead decreases by **7 bytes**. There are no new
art assets or dependencies. Player-side serialized assets remain **11,356,742
bytes** and bundle-side assets remain **146,871,897 bytes**. Compared asset rows
and both player/bundle mesh inventories are unchanged. These uncompressed
inventories are distinct from the measured APK size.

All **244 compiled Windows gameplay source documents** match captured working
source. Platform-specific resource selection and generated Golf prefab local
IDs are the only subsequent build-input changes; format-aware comparison
confirms identical prefab semantics. Evidence is in `Builds/PropRetentionQA/`,
including `asset-comparison.json`, `source-comparison.json`,
`generated-prefab-equivalence.json` and `apk-signature.txt`.
See [verification and delivery limits](VERIFICATION.md#persistent-sports-props-and-slower-football-shots---14-october-2026).

## Lossless island packaging - 14 October 2026

Actual APK at **16:38:21 Malaysia time**:
**91,647,416 -> 73,446,145 bytes (-18,201,271)**. The 75 MB target is achieved
with **1,553,855 bytes** remaining; hard-limit headroom is **26,553,855 bytes**.
The AndroidSubmission gate and APK v2 signature verification pass. SHA-256:
`B95490F5D3FFE178099FD8A26130CBE43B43D9F00172177858AC41EF3E4F54C9`.

| Largest APK entries | Compressed bytes |
| --- | ---: |
| Embedded five-scene bundle | 41,773,982 |
| IL2CPP ARM64 library | 11,878,577 |
| Unity ARM64 library | 9,190,940 |
| Managed metadata | 2,696,189 |
| Golf cart serialized asset | 909,802 |
| Unity default resources | 798,971 |
| Java classes.dex | 351,740 |

The primary saving is lossless scene compression with shared dependencies,
followed by release Java minification. The earlier non-minified packaging trial
was 75,452,618 bytes and missed the target; it was not accepted as delivery.
All four islands, close props, cabin modules, animations and resource-loaded
gameplay/UI assets remain. This is not an asset-quality reduction or deferred
network download. The runtime cache is additional installed storage.

Player-side serialized assets total **11,356,742 bytes**; bundle-side serialized
assets total **146,871,897 bytes**. These are separate inventories, not APK size.
`latest/bundle-packed-assets.csv`, `bundle-packed-layout.csv`, `bundle-meshes.csv`
and `bundle-catalog.json` now accompany the normal player report. The asset ZIP
estimator explicitly warns that its per-asset estimates exclude the LZMA bundle.

UnityPy inspection of the actual opening APK, new embedded bundle and new
player-side files matched **2,022 Mesh/Texture2D/AnimationClip objects in 823
asset/type groups**, with **zero changed or missing groups**. Texture checks
also hash image bytes. This covers assets under `Assets/_Game/Art/`; shader and
material support is checked separately in rendered Windows tests. All 244
compiled Windows gameplay source documents match current source. Platform
builds only changed the desktop SSAO resource selection and generated Golf ball
local object IDs; format-aware comparison confirms unchanged prefab semantics.

Before/after package, source, payload, signature and validation evidence is in
`Builds/PackagingQA/`; current release inventories are in `Builds/SizeAudit/latest/`.
See [packaging mechanics and cache tradeoffs](CONTENT-PACKAGING.md) and
[verification](VERIFICATION.md#lossless-island-packaging---14-october-2026).
Physical Android quality/FPS, startup and WAN play remain unverified.

## Basketball and football defense swipes - 14 October 2026

Actual APK: **91,634,440 -> 91,647,416 bytes (+12,976)**, built at
**15:48:33 Malaysia time**. Headroom: **8,352,584 bytes** to the strict
100,000,000-byte boundary. Development-target excess: **16,647,416 bytes**.
The AndroidSubmission budget gate and independent APK v2 signature check pass.
SHA-256:
`B5840A3429B40C6D251E2DC1CC9ABF33C4C907D6431E8A62551CAC1874D90B18`.

There are **923 APK entries**, unchanged. Compressed payload grows **12,978
bytes**, with two fewer bytes of container/signature overhead. The principal
growth is IL2CPP **+11,789 bytes** and managed metadata **+1,114 bytes**.
Largest compressed entries are IL2CPP **11,862,414**, Unity **9,114,056**,
managed metadata **2,691,336** and `classes.dex` **2,353,575 bytes**.

Serialized assets total **157,500,474 bytes (+112)**. The only changed asset
row is the new development review script's 112-byte script record; its review
implementation is guarded by `UNITY_EDITOR || DEVELOPMENT_BUILD`. Serialized
sizes are not compressed APK sizes. All **1,788 packed mesh rows are unchanged**.
The feature shares existing control artwork, fonts, meshes and animations and
adds no packages. No texture/audio/mesh quality was reduced. The existing
75 MB development reserve deficit remains open.

All **242 compiled Windows gameplay source documents** still match current
source after Android builds. Of 3,774 captured asset/package/settings inputs,
only two differ: Android removes the desktop SSAO runtime resource selection
and regenerates Golf ball prefab object IDs. Format-aware YAML comparison
confirms identical Golf prefab semantics. The Android packing comparison
confirms unchanged art and mesh rows.

Current audit is in `Builds/SizeAudit/latest/`; before/after comparisons,
signature evidence, source checks and Windows delivery manifests are retained
under `Builds/BasketballControlQA/`. The prior APK/audit is recoverable from
`Legacy/20261014-154454-549-sports-controls-before-android/`.
See the [validation and archive record](VERIFICATION.md#basketball-and-football-defense-swipes---14-october-2026).
Physical Android appearance/FPS and real WAN play remain unverified.

## Programmed sports bots and training retirement - 14 October 2026

Actual APK: **91,700,774 -> 91,634,440 bytes (-66,334)**, built at
**14:09:27 Malaysia time**. Headroom: **8,365,560 bytes** to the strict
100,000,000-byte boundary. Development-target excess: **16,634,440 bytes**.
The enforced size gate and APK v2 signature check pass. SHA-256:
`63B343211DD71D6F732CBFE0F4CC6F486C44E8A57532A6A7966722D408923F97`.

Package entries fall from **1,030 to 923**. Compressed payload decreases
**47,952 bytes**, with **18,382 fewer bytes** of container/signature overhead.
Serialized assets decrease **134,416 bytes** to **157,500,362**, including
**116,808 bytes** of inference resources across **482 removed audit rows**.
These serialized measurements are not compressed APK bytes. All **1,788 mesh
rows are unchanged**, and no gameplay art, texture or audio quality is reduced.

Largest compressed entries: IL2CPP **11,850,625**, Unity **9,114,056**,
managed metadata **2,690,222**, and `classes.dex` **2,353,575 bytes**.
The programmed policies add small compiled code; removing the already-stripped
learning dependencies produces a modest net saving. No trained model had been
included in the opening APK. The development reserve deficit remains open.

ML-Agents, Inference Engine, App UI, their build support, training launchers,
datasets and model candidates are retired. Source/data recovery batches are
listed in [programmed bots](PROGRAMMED-BOTS.md#retired-training).
The prior APK/audit is preserved in
`Legacy/20261014-140531-191-programmed-bots-before-android/`.
Before/after APK entries, asset changes, canonical Golf prefab comparison and
signature evidence are in `Builds/ProgrammedBotsQA/`. Current audit is in
`Builds/SizeAudit/latest/`. No physical Android or WAN qualification was run.

## Latest upstream Golf merge - 14 October 2026

Fast-forwarded `main` from `ca979d5` to `3a0456d`, including `cb0b6e2`.
Preserved local sports/AI work and protocol 51; combined both sides of release
records and retained the incoming Golf ball prefab with coherent object IDs.

Fresh AndroidSubmission at **01:16:04 Malaysia time** measures
**90,518,282 -> 91,700,774 bytes (+1,182,492)**. Headroom to the strict
100,000,000-byte boundary is **8,299,226 bytes**. The 75,000,000-byte
development target remains exceeded by **16,700,774 bytes**. The enforced
budget gate and independent APK v2 signature verification pass. SHA-256:
`4977FBE6105F4E7ADFD0A2B250FFF127C6C6A9431F238E9BA53698FD04867E52`.

Audit growth is primarily the five incoming flag textures (**315,116 serialized
bytes each**) and refined flagpole/cloth meshes (**+121,668 / +51,044 bytes**).
Serialized assets total **157,634,778 bytes**, up **1,789,632**; serialized
measurements are not compressed APK bytes. The largest compressed entries are
IL2CPP **11,837,737**, Unity **9,114,056**, managed metadata **2,689,497** and
`classes.dex` **2,356,543 bytes**. Preserve shared masters and the existing
mobile imports; the development reserve deficit remains unresolved.

Current audit is in `Builds/SizeAudit/latest/`. Before-build APK/audit and asset
comparison are retained in `Builds/PullMergeQA/`. This is a merged working-tree
build, including pre-existing uncommitted work, not a clean commit snapshot.
Physical Android appearance, FPS and WAN play are unverified. Concurrent
BasketballBot/BasketballBots scripts added after both builds are excluded from
this validated source snapshot; final readiness flags their missing metadata.

## Football radial pickup and reduced pace - 14 October 2026

Actual APK: **90,515,790 -> 90,518,282 bytes (+2,492)**, built
**00:48:57 Malaysia time**. Headroom: **9,481,718 bytes** to the strict
100,000,000-byte boundary. Development-target excess: **15,518,282 bytes**.
SHA-256: `11E14C28F9E2065C473D62A0F7C52AB42F5671478693CFAF4794ABD3C2E70829`.
APK v2 signature verification and enforced size checks pass.

The package still has **1,026 entries**. Compressed payload changes **+2,493
bytes**, with **-1 byte** of container/signature overhead. Total serialized
assets stay **155,845,146 bytes**, and all **1,788 mesh rows** are unchanged.
Largest compressed entries are IL2CPP **11,833,262**, Unity **9,112,780**,
managed metadata **2,688,022** and `classes.dex` **2,356,543 bytes**. The small
increase is code/metadata rather than new assets or dependencies; there is no
avoidable art growth to remove within this gameplay change. The pre-existing
development-budget deficit remains unresolved.

Current audit: `Builds/SizeAudit/latest/`. Before/after assets, ZIP entries,
hashes and signature evidence: `Builds/FootballControlQA/`. Prior Android
output and audit are preserved in
`Legacy/20261014-004605-879-football-control-android/`.

## Training recovery and Fishing merge - 13 October 2026

Actual APK: **90,384,954 -> 90,515,790 bytes (+130,836)**, built at
**22:44:55 Malaysia time**. Headroom to the strict 100,000,000-byte boundary:
**9,484,210 bytes**. Development-target excess: **15,515,790 bytes**.
SHA-256: `CA05A8BB6FB2820FC5FA478408F49716F03718AB64F4857375E5BD92799B6B01`.
APK v2 signature verification passes. The package has **1,026 entries**;
compressed payload grows **130,152 bytes**, with another **684 bytes** of
container/signature overhead. Serialized assets grow **132,000 bytes**.

Incoming Fishing art changes are small delivery textures, not additional
high-resolution masters in the package:

| Packed source | Serialized byte delta |
| --- | ---: |
| `FishingUI/TutorialPanels.png` | +63,428 |
| `FishingUI/GearIcons.png` | +45,960 |
| `FishingUI/CounterFrame.png` | +30,928 |
| `FishingUI/SteerIdle.png` | +17,160 |
| `FishingUI/SteerLit.png` | +17,160 |
| `FishingUI/LagoonFrame.png` | -30,248 |
| `FishingUI/ReelButton.png` | -13,288 |

These paths are under `Assets/_Game/Resources/`. Remaining growth is small
layout/script metadata. All **1,788 mesh rows are unchanged**. Largest compressed
entries remain IL2CPP **11,830,766**, Unity **9,112,780**, managed metadata
**2,688,010** and `classes.dex` **2,356,543 bytes**. No candidate model or teacher
data enters game assets. Training logic is excluded from the release player.
The existing development-budget deficit remains unresolved; unrelated visual
assets were not degraded to hide it.

Current reports: `Builds/SizeAudit/latest/`. Before/after entries, packed assets,
source hashes and independent signature check: `Builds/TrainingRepairQA/`.
The pre-task APK and audit are preserved in
`Legacy/20261013-222552-227-training-recovery-before-android/`.



## Reference flags and turf bounces - 13 October 2026 build limitation

The measured opening APK is **90,334,226 bytes**. All three new AndroidSubmission
attempts failed in Gradle/JDK loopback initialization; the prior APK is retained
with its original hash. There is no successful after measurement for these
changes. Its historical headroom is **9,665,774 bytes**, and it exceeds the
75 MB development target by **15,334,226 bytes**. These values do not validate
the current reference flag textures or rebound code. Five derived flag maps use
mipmaps and Android ASTC 6x6; the full reference masters remain outside Unity.
Unused geometric numeral meshes and their material were archived. No global
rendering quality or unrelated dependency was reduced. Fresh packed APK
contributors cannot be measured until the new build succeeds. Windows checks
and complete delivery pass; see [verification](VERIFICATION.md#original-reference-flags-and-decaying-turf-bounces---13-october-2026).


## Golden Golf shot trail — 13 October 2026

Fresh signed primary APK at **21:25:01 Malaysia time**:
**89,230,290 → 90,334,226 bytes (+1,103,936)**. The opening APK was the stale
4 October release; the delta includes later combined gameplay/art, so it does
not measure this visual feature alone. Strict headroom **9,665,774 bytes**;
75 MB development-target excess **15,334,226 bytes**. Budget gate and independent
v2 signature pass. Exact package/audit: `Builds/GolfTrailQA/Release/`.
[Release, source limits and archives](BUILD-SIZE.md#golden-golf-shot-trail--13-october-2026).

| Trail entry | Serialized bytes | Estimated ZIP bytes |
| --- | ---: | ---: |
| Soft-edge unlit shader | 6,312 | 3,304 |
| Shared material | 96 | 54 |
| MonoScript metadata | 96 | 81 |
| Total | 6,504 | 3,439 |

No new mesh, texture, audio, font or package is added by the trail. Each ball
uses a bounded one-second runtime ribbon with a reused property block; it adds
no lights, shadows, collision geometry or physics forces. Native compiled code
is a separate contribution. Current serialized assets total **156,182,122 bytes**,
which is not the compressed APK size. Largest compressed estimates: timber
**2.247 MB**, character albedo **1.993 MB**, Golf structures **1.966 MB**,
character mesh **1.588 MB** and football stadium **1.556 MB**. The structures
change belongs to concurrent scenery-flag cleanup. No unexpected large trail
dependency appears. Detailed current packing: `Builds/SizeAudit/latest/`;
retained copy and top contributors: `Builds/GolfTrailQA/`.

## Stronger Fishing motion and upstream Golf integration — 13 October 2026

Fresh AndroidSubmission written **19:47:57 Malaysia time** measures
**90,281,708 → 90,388,492 bytes (+106,784)**.
Headroom to the strict 100,000,000-byte boundary: **9,611,508 bytes**.
The **75,000,000-byte development target remains exceeded by 15,388,492 bytes**.
The enforced hard limit and independent APK v2 signature checks pass. SHA-256:
**18CC2A2F7E2354ADACAF585FCF7DCDD8F5257A61CC1EAE4D0557CDE91A1EE6EB**.

Compressed ZIP payloads change **+105,751 bytes**;
container/signature overhead changes **+1,033**.
The new APK contains **919 entries**, including
**899 unchanged payload hashes**. These are measurements
of the actual APK; source PNG bytes and the tables below are different quantities.

Serialized assets change **+172,008 bytes**;
6 packed source entries change.

| Changed packed source | Serialized delta (uncompressed) |
| --- | ---: |
| `Assets/_Game/Resources/GolfUI/Countdown_5.png` | +30,660 |
| `Assets/_Game/Resources/GolfUI/Countdown_3.png` | +30,572 |
| `Assets/_Game/Resources/GolfUI/Countdown_2.png` | +30,456 |
| `Assets/_Game/Resources/GolfUI/Countdown_4.png` | +30,412 |
| `Assets/_Game/Resources/GolfUI/Countdown_1.png` | +30,344 |
| `Assets/_Game/Resources/GolfUI/Countdown_GO.png` | +19,564 |

Fishing's new motion reuses the existing shared line material and pooled
geometry. Upstream Golf graphics are included in this combined measurement;
growth is not attributed solely to Fishing. No new Fishing asset is imported.
Mesh rows: **1788 → 1788**, with
**0 differing rows**. MobileURP/MobileRenderer are
packed; no forbidden desktop pipeline/SSAO/BlueNoise256 delivery asset is found.
All requested development review types are absent from release metadata.
The package remains self-contained ARM64; no addressable download is introduced.

| Largest packed source | Serialized bytes (uncompressed) |
| --- | ---: |
| `Assets/_Game/Art/RefinedIslands/Fishing/Fishing_TerrainControl.png` | 22,369,760 |
| `Assets/_Game/Art/RefinedIslands/Golf/Golf_TerrainControl.png` | 22,369,756 |
| `Assets/_Game/Art/RefinedIslands/Fishing/Fishing_WaterDepth.png` | 5,592,564 |
| `Assets/_Game/Art/RefinedIslands/Golf/Golf_WaterDepth.png` | 5,592,560 |
| `Assets/_Game/Art/Football/Sunvale/Stadium.fbx` | 3,569,800 |
| `Built-in Texture2D: Splash Screen Unity Logo` | 2,796,376 |

These largest sources remain the principal size concern. The development
target remains unmet; this effect pass does not add textures or models to them.
Current CSVs, ZIP hashes/deltas, source manifest, signature and full-player
promotion evidence: `Builds/FishingSurge/Task-20261013-190655/`. No physical-phone FPS claim is made.

## Fishing six skills, familiar tutorial and water contact — 13 October 2026

Fresh AndroidSubmission written **17:49:09 Malaysia time** measures
**90,186,840 → 90,281,708 bytes (+94,868)**.
Headroom to the strict 100,000,000-byte boundary: **9,718,292 bytes**.
The **75,000,000-byte development target remains exceeded by 15,281,708 bytes**.
Enforced hard limit and independent APK v2 signature checks pass. SHA-256:
**FEC057213DF9E1F1837E954107B37E7335068EBD2B7BB53F65966C1EF2E588CB**.

Compressed ZIP payload totals **90,044,973 → 90,139,488 (+94,515)**.
Container/signature overhead rises **353 bytes**. **913 entries** (previously
911) include **845 unchanged payload hashes**. The two new resource entries
use **38,685 and 30,558 compressed bytes**. IL2CPP grows **15,392**, Burst
**6,616**, and metadata **2,060** compressed bytes; smaller entry changes offset
the remainder. These are actual APK measurements.

Serialized assets total **155,427,666 → 155,537,350 (+109,684)**.
Only five entries change: GearIcons **45,960**, TutorialPanels **63,428**,
and three script metadata stubs **104/96/96** bytes. The atlases are **384×256**
and **512×256**; no mipmaps, Android **ASTC 6×6**. Delivery PNGs total **299,025
bytes** before APK packing. Larger editable masters stay outside Unity.
Sonar shares the sports radar; Power Reel shares Fishing's reel. The
[portable generator and art record](../ArtSource/Fishing/UI/Learning/README.md)
regenerate identical pixels from a clone path with spaces and an unrelated cwd.

All **1,788 mesh rows** match. Existing raster/model/font/audio/shader packed
sizes remain unchanged. No new imported model, audio, shader or package is
added. Splashes share Fishing's line material; supplies, treasure and fonts
remain shared with the sports. MobileURP/MobileRenderer are packed; forbidden
Desktop/SSAO shader/BlueNoise256 assets remain absent. All **14 checked
development review types** are absent from release metadata.

The largest unchanged serialized contributors remain Fishing/Golf terrain
controls (~22.37 MB each), their water-depth textures (~5.59 MB each), and
Football Stadium.fbx (~3.57 MB). Those values are uncompressed serialized
sizes, not individual APK ZIP sizes. New UI growth was reduced by downsizing
the supporting tutorial art to 256×128 per page. The 75 MB target remains unmet.

Current evidence: `Builds/FishingLearn/Task-20261013-160232/ApkComparisonFinal/`
and `Builds/SizeAudit/latest/`. The exact baseline APK ZIP and prior size audit
were captured before replacement. No old APK is used to validate new code.

## Fishing hybrid gear and smooth force-driven fights — 13 October 2026

Fresh signed APK: **90,166,448 → 90,186,840 bytes (+20,392)**, written
**14:50:32 Malaysia time**. Hard-boundary headroom **9,813,160**; 75 MB target
overrun **15,186,840**. Enforced limit and independent APK v2 signature pass.

Compressed ZIP entries total **90,024,585 → 90,044,973 (+20,388)**;
container/signature overhead increases **4 bytes**. There are **911 entries**
in both APKs and **848 unchanged payload hashes**. IL2CPP grows **21,908
compressed bytes**, Unity **3,792**, and metadata **2,529**; small serialized
and other entry changes offset part of this growth. These are APK measurements,
separate from Unity's uncompressed serialized totals.

Serialized assets total **155,427,222 → 155,427,666 (+444)**. Only five totals
change: four script metadata entries (120, 104, 104 and 100 bytes), and 16 bytes
for the layout's two caption vectors. The new development check's MonoScript
metadata is a 104-byte stub; its executable review type is absent from release.
Existing packed raster/model/font/audio/material/shader asset sizes are unchanged.
All **1,788 mesh audit rows** match. No new raster texture, imported model,
audio, shader or package is added. Supplies share Basketball's existing Turbo
mesh, treasure parts share built-in cubes and Football materials, and effects
use the existing Fishing line material. Native labels remain sharp without
additional texture pixels. Existing sprite delivery PNGs stay at **201,479 bytes**.

MobileURP/MobileRenderer are packed; forbidden Desktop/SSAO shader/BlueNoise256
assets remain absent. All **14 checked development review types** are absent
from release metadata. The dominant terrain-control/depth assets remain the
base-game size problem; the 75 MB working target is still unmet.

Evidence: `Builds/FishingFlow/Task-20261013-133219/ApkComparisonFinal/` and
`Builds/SizeAudit/latest/`. The baseline ZIP was captured before replacement;
its retained prior release audit was recovered and verified against the exact
same APK size/SHA when missing baseline CSVs were discovered. That provenance
is recorded in `baseline-audit-provenance.json`. No current audit was substituted
for the baseline. Desktop checks do not qualify physical phone performance.

## Illustrated Fishing controls and fight motion — 13 October 2026

Fresh signed APK: **90,107,796 → 90,166,448 bytes (+58,652)**, written
**01:18:45 Malaysia time**. Headroom: **9,833,552 bytes**; 75 MB target overrun:
**15,166,448 bytes**. Enforced limit and independent v2 signature pass.
[Release record](BUILD-SIZE.md#illustrated-fishing-controls-and-fight-motion--13-october-2026).

Compressed ZIP entries total **89,966,621 → 90,024,585 bytes (+57,964)**;
container/signature overhead changes **141,175 → 141,863 (+688)**.
There are **907 → 911 entries** and **895 unchanged payload hashes**.
The four new/restored textures add **53,005 compressed ZIP bytes**:
frame **14,424**, reel **13,749**, lit steering **12,779**, idle steering **12,053**.
Left/Right share their artwork; native labels do not occupy texture pixels.
IL2CPP grows **2,400 compressed bytes**, Burst **2,292**, metadata **74**;
remaining small entry changes include serialized record IDs and layout data.

Serialized assets grow **155,344,702 → 155,427,222 bytes (+82,520)**, a separate
uncompressed Unity measurement. Only five packed totals change:

| Asset | Added serialized bytes |
| --- | ---: |
| CounterFrame, 512×128, sliced | 30,928 |
| ReelButton, 192×192, restored original GUID | 17,216 |
| SteerIdle, 192×192 | 17,160 |
| SteerLit, 192×192 | 17,160 |
| FishingUILayout fields | 56 |

Delivery PNG files total **201,479 bytes**, before Android ASTC 6×6 and ZIP
packing. All sprites are 2D, without mipmaps. A build preflight validates these
imports. Full-quality editable masters remain outside Unity; the old ornamental
LagoonFrame remains retired. No font, model, audio, shader or package is added.
All **1,788 imported-FBX mesh rows remain identical**; retained packed binary
asset sizes remain unchanged. Largest serialized contributors remain the two
terrain-control textures (about **22.37 MB each**), water-depth textures (about
**5.59 MB each**) and stadium geometry. These are not compressed APK costs and
remain base-game optimization candidates; the 75 MB target is still unmet.

MobileURP/MobileRenderer are packed; Desktop/SSAO shader/BlueNoise256 assets
are absent. All **13 checked development review types** are absent from release
metadata. Current evidence: `Builds/FishingIllustrated/Task-20261012-234621/ApkComparison/`
and `Builds/SizeAudit/latest/`. Desktop validation does not qualify a physical phone.

## Fishing Hold & Countersteer — 12 October 2026

Fresh signed APK: **90,139,324 → 90,107,796 bytes (-31,528)**,
written **20:43:13 Malaysia time**. Hard-boundary headroom: **9,892,204**;
75 MB target overrun: **15,107,796 bytes**. Independent v2 signature
and enforced hard-limit checks pass. [Release record](BUILD-SIZE.md#fishing-hold--countersteer--12-october-2026).

Compressed ZIP entries total **89,997,825 → 89,966,621
bytes (-31,204)**; container/signature overhead changes
**141,499 → 141,175 (-324)**.
There are **909 → 907 entries** and **855 unchanged payload hashes**. Added
countersteering/UI code grows IL2CPP by **8,100 compressed bytes** and metadata
by **561**. Removed sprite payloads outweigh this small code growth. No new
raster delivery asset, font, audio, model, shader or package was added.

Serialized assets fall **155,405,350 → 155,344,702 bytes (−60,648)**, a separate
uncompressed measurement. Only three packed asset totals change: removal of
LagoonFrame (**30,248**) and ReelButton (**30,504**) plus **104 bytes** of new
script metadata. Every other existing packed binary asset retains its size.
Both retired textures have zero active GUID/resource/generator references;
originals remain hash-identical in `ArtSource/Fishing/UI/Masters/`. Their source
PNGs total 3.06 MB, which is **not** the APK saving. Clock remains shared by Golf.
All **1,788 imported-FBX mesh rows remain identical**. World art/animation is
unchanged; the APK remains well above the development target.

MobileURP/MobileRenderer remain packed; Desktop/SSAO shader/BlueNoise256 assets
are absent. All **13 checked development review types** are absent from release
IL2CPP metadata. Production fight controls are present and exercised separately
in the Windows player. Fresh evidence: `Builds/FishingRedesign/Concepts-20261012-194347/ApkComparison/` and
`Builds/SizeAudit/latest/`. Physical-phone qualification remains separate.
## Teammate merge and training launcher (13 October 2026)

Fresh signed APK: **90,384,954 bytes**, **+142,622** from the measured
90,242,332-byte baseline. Headroom: **9,615,046 bytes**; development target
exceeded by **15,384,954 bytes**.
[Release and hash](BUILD-SIZE.md#teammate-merge-and-training-launcher-13-october-2026).

ZIP entries increase **1,015 -> 1,022**. Compressed entries add **141,423 bytes**;
container overhead adds **1,199 bytes**. Largest compressed entries are IL2CPP
**11,786,432**, Unity **9,108,103**, metadata **2,681,794** and Java
**2,356,543 bytes**. Serialized assets rise **155,540,258 -> 155,713,146 bytes
(+172,888)**: six incoming Golf countdown images total **172,008**, Fishing
layout **160**, and seven script records **720 bytes**. These are uncompressed
serialized sizes, not APK bytes. All **1,788 mesh records** match. New Golf art
master/delivery hashes match all **16** entries in its source audit.

No new model weights, Python dependencies, meshes or audio enter the APK.
Existing CPU-only inference shader filtering and editor-only App UI settings
were preserved while retaining the teammate's portable output-path resolver.
The incoming image growth is small and explained; the pre-existing 75 MB
shortfall remains an optimization task, not an acceptable reserve to spend.

All **234 compiled runtime source documents** match the final Windows player.
After Android, **3,720/3,722 Game input hashes** match its source snapshot. Only
URP resource registration and generated Golf prefab IDs/order differ; a
format-aware comparison proves identical Golf objects/properties/references.
Original IDs/order were restored after retaining the generated copy and proof.

Evidence is in `Builds/MergeTrainingQA/` (`Before/`, APK/asset/entry comparisons,
source proofs, art hashes, signature and device records) and
`Builds/SizeAudit/latest/`. The pre-merge APK/audit is preserved in
`Legacy/20261013-165930-274-teammate-merge-before-android/`. The first merged APK,
superseded by the tested Golf fix, is in
`Legacy/20261013-170706-721-merge-golf-validation-rebuild/`.
Signature verification passes; no Android device was attached.

## Football training checkpoint (13 October 2026)

Fresh signed APK: **90,242,332 bytes**, **+90,830** from the measured
90,151,502-byte baseline. Hard-boundary headroom: **9,757,668 bytes**;
75 MB development target exceeded by **15,242,332 bytes**.
[Release and hash](BUILD-SIZE.md#football-training-checkpoint-13-october-2026).

ZIP entries increase **908 -> 1,015**. Compressed entries add **72,428 bytes**;
container overhead adds **18,402 bytes**. Largest compressed entries: IL2CPP
**11,740,737**, Unity **9,107,612**, metadata **2,675,153**, Java **2,356,543
bytes**. The IL2CPP increase is 6,668 compressed bytes. Model weights and Python
dependencies are not included in the APK.

Serialized assets rise **155,405,854 -> 155,540,258 bytes (+134,404)**:
103 residual inference shader records total 62,504 bytes, 523 package script
records add 71,588, two project script records add 224, and generated performance
test metadata adds 88. These are uncompressed serialized sizes, not APK bytes.
All existing art/audio sizes and **1,788 mesh records** match the baseline.

An initial attempt revealed expensive unused GPU inference shader compilation
and was stopped before an APK was produced. The final build excludes all GPU
inference shader variants, while retaining CPU inference for the development
prototype. Transitive App UI is editor-only, with its player shader inclusion
disabled. Shader/resource headers remain in the package; no package-cache
files were deleted or vendored to hide their dependencies. Future GPU inference
requires revisiting this deliberate filter. No game render shader, texture,
mesh or audio quality was reduced. Existing development-budget debt remains.

All **227 compiled runtime source documents** match the final Windows player.
After Android, **3,690/3,692 Game input hashes** match the Windows snapshot;
only URP resource registration and generated Golf prefab IDs/order differ.
Format-aware comparison proves identical Golf objects/properties/references
after ID normalization; original IDs/order were restored to avoid unrelated
churn. The generated copy and proof are retained.

Evidence: `Builds/FootballTrainingQA/Before/`, `apk-comparison.json`,
`asset-comparison.json`, `entry-comparison.json`, `apk-signature.txt`,
`compiled-source.json`, `source-comparison.json`,
`generated-prefab-equivalence.json` and `Builds/SizeAudit/latest/`.
The prior APK/audit is in
`Legacy/20261013-160308-582-football-training-before-android/`;
the abandoned GPU build log is in
`Legacy/20261013-161136-171-football-unused-gpu-build/`.
APK v2 signature verification passes; no Android device was available.

## Casual football calibration (13 October 2026)

Fresh signed APK: **90,151,502 bytes**, **940 bytes smaller** than the measured
90,152,442-byte baseline. Hard-boundary headroom: **9,848,498 bytes**;
75 MB development target exceeded by **15,151,502 bytes**.
[Release and hash](BUILD-SIZE.md#casual-football-calibration-13-october-2026).

There are still **908 ZIP entries**. Compressed entries decrease **936 bytes**;
container overhead decreases **four bytes**. Largest compressed entries: IL2CPP
**11,734,069**, Unity **9,107,612**, metadata **2,674,740**, Java **2,353,575
bytes**. Native uncompressed code rises 656 bytes while its compressed entry
shrinks 1,006 bytes; do not interpret compressed-size noise as an optimization.

Serialized assets remain **155,405,854 bytes**. Every packed asset size and all
**1,788 mesh records** match the baseline. The policy continues to share the
same characters, animation, geometry, audio and abilities. No art quality,
compression setting, ML dependency or model weight changed. Development-only
calibration drills and reporting remain excluded from release. No avoidable
bulk growth was found; the pre-existing 75 MB development-target gap is open.

All **225 compiled runtime source documents** match the final Windows player.
After Android, **3,682/3,684 Game inputs** match the Windows snapshot, with only
URP resource registration and generated Golf prefab IDs/order differing.
Format-aware comparison proves equivalent Golf objects, properties and
references after ID normalization; original IDs/order were restored to avoid
unrelated churn. The generated copy and proof are retained.

Evidence: `Builds/FootballCasualQA/Before/`, `apk-comparison.json`,
`asset-comparison.json` (empty: no asset-size changes), `entry-comparison.json`,
`apk-signature.txt`, `compiled-source.json`, `source-comparison.json`,
`generated-prefab-equivalence.json` and `Builds/SizeAudit/latest/`.
The previous release/audit is in
`Legacy/20261013-144448-226-football-casual-before-android/`.
APK v2 signature verification passes; no Android device was available.

## Football defense and tactical AI (13 October 2026)

Fresh signed APK: **90,152,442 bytes**, **+8,928** from the measured
90,143,514-byte baseline. Hard-boundary headroom: **9,847,558 bytes**;
75 MB development target exceeded by **15,152,442 bytes**.
[Release and hash](BUILD-SIZE.md#football-defense-and-tactical-ai-13-october-2026).

There are still **908 ZIP entries**. Compressed entries add **8,927 bytes**;
container overhead adds **one byte**. Largest compressed entries: IL2CPP
**11,735,075**, Unity **9,107,612**, metadata **2,674,698**, Java **2,353,575
bytes**. The small increase is compiled gameplay code and metadata, not a
content pack or training dependency.

Serialized assets rise **155,405,750 -> 155,405,854 bytes (+104)**, entirely the
new navigation script record. All existing art/audio sizes and **1,788 mesh
records** match. The defense/tactical policy shares existing characters,
animation, props, geometry and ball physics. Development-only review/training
logic is excluded from release. No new model weights, duplicated delivery art
or avoidable bulk growth were introduced. Existing asset quality/compression
is unchanged; the pre-existing development-target gap remains unresolved.

All **225 compiled runtime source documents** match the final tested Windows
player. After Android, **3,682/3,684 Game input hashes** match the Windows
snapshot; only generated Golf prefab IDs/order and URP resource registration
differ. Format-aware prefab comparison proves identical objects, properties
and references after normalizing local IDs. Original IDs/order were restored
to avoid unrelated churn; the generated copy and comparison are retained.

Evidence: `Builds/FootballDefenseAIQA/Before/`, `apk-comparison.json`,
`asset-comparison.json`, `entry-comparison.json`, `apk-signature.txt`,
`compiled-source.json`, `source-comparison.json`,
`generated-prefab-equivalence.json` and `Builds/SizeAudit/latest/`.
The prior APK/audit is preserved in
`Legacy/20261013-130942-517-football-defense-ai-before-android/`.
Intermediate releases were preserved in
`Legacy/20261013-131957-747-football-defense-ai-intermediate/`,
`Legacy/20261013-133124-728-football-defense-ai-final-fixture/` and
`Legacy/20261013-133808-909-football-ai-loose-ball-recovery/`.
The final APK was rebuilt after the loose-ball fix and final functional checks;
an older APK is not being used to validate current code. APK v2 signature
verification passes; no Android device was available.

## Football AI foundation (13 October 2026)

Fresh signed APK: **90,143,514 bytes**, **+16,972** from the measured
90,126,542-byte baseline. Hard-boundary headroom: **9,856,486 bytes**;
75 MB development target exceeded by **15,143,514 bytes**.
[Release and hash](BUILD-SIZE.md#football-ai-foundation-13-october-2026).

There are still **908 ZIP entries**. Compressed entries add **16,963 bytes**;
container overhead adds **9 bytes**. Largest compressed entries: IL2CPP
**11,726,704**, Unity **9,107,612**, metadata **2,674,156**, Java **2,353,575
bytes**. Growth is small compiled gameplay code/metadata, not new content packs.

Serialized assets rise **155,405,242 -> 155,405,750 bytes (+508)**: five new
script records add 496 bytes; the network athlete record adds 12 bytes. Review
and training-driver logic is excluded from release by the development/editor
guard. All **1,788 mesh records** and existing texture, mesh, material and audio
sizes match. Bots share existing characters, animation, controls and ball
physics. No ML package, model weight, duplicated art or avoidable bulk growth
was introduced. The pre-existing 75 MB development-target gap is not resolved.

All **224 compiled runtime source documents** match the tested Windows player.
After Android, **3,680/3,682 Game input hashes** match the Windows snapshot;
only generated Golf prefab IDs/order and URP resource registration differ.
Format-aware prefab comparison proves identical objects, properties and
references after normalizing local IDs. Original IDs/order were restored to
avoid unrelated churn; the generated copy and comparison are retained.

Evidence: `Builds/FootballAIQA/Before/`, `apk-comparison.json`,
`asset-comparison.json`, `entry-comparison.json`, `apk-signature.txt`,
`compiled-source.json`, `source-comparison.json`,
`generated-prefab-equivalence.json` and `Builds/SizeAudit/latest/`.
Prior release/audit: `Legacy/20261013-001445-260-football-ai-before-android/`.
An intermediate APK was rebuilt after a development-only menu fixture update:
`Legacy/20261013-002059-797-football-ai-intermediate-apk/`.

## Room matches (12 October 2026)

Fresh signed APK: **90,126,542 bytes**, **+27,432** from the measured
90,099,110-byte baseline. Hard-boundary headroom: **9,873,458 bytes**;
75 MB development target exceeded by **15,126,542 bytes**.
[Release and hash](BUILD-SIZE.md#room-matches-12-october-2026).

There are still **908 ZIP entries**. Compressed entries increase **27,433 bytes**;
container overhead decreases **1 byte**. Largest compressed entries: IL2CPP
**11,712,289**, Unity **9,107,612**, metadata **2,672,024**, Java **2,353,575
bytes**. Growth is small compiled gameplay/network code and metadata, not art.

Serialized assets rise **155,404,594 -> 155,405,242 bytes (+648)**: six script
records add 616 bytes; the network athlete prefab's serialized record adds 32.
Review logic is excluded from release by `UNITY_EDITOR || DEVELOPMENT_BUILD`.
All **1,788 imported mesh records**, and all existing textures, meshes, materials
and audio asset sizes, match. Existing compression and visual quality are
unchanged. No unexpected bulk growth or avoidable duplicated delivery assets
were found; the pre-existing 75 MB target gap remains unresolved.

All **219 compiled runtime source documents** match the tested Windows player.
After Android, **3,668/3,670 Game input hashes** match the Windows snapshot;
only generated Golf prefab IDs/order and URP resource registration differ.
Format-aware prefab comparison proves identical objects, properties and
references after normalizing local IDs. Original IDs/order were restored to
avoid unrelated source churn; the generated copy and comparison are retained.

Evidence: `Builds/RoomMatchQA/Before/`, `apk-comparison.json`,
`asset-comparison.json`, `entry-comparison.json`, `apk-signature.txt`,
`compiled-source.json`, `source-comparison.json`,
`generated-prefab-equivalence.json` and `Builds/SizeAudit/latest/`.
Prior release/audit:
`Legacy/20261012-223506-586-room-match-before-android/`.

## Hosted sports movement — 12 October 2026

Fresh signed APK: **90,099,110 bytes**, **300 bytes smaller** than the measured
90,099,410-byte baseline. Hard-boundary headroom: **9,900,890 bytes**;
75 MB development target exceeded by **15,099,110 bytes**.
[Release and hash](BUILD-SIZE.md#hosted-sports-movement--12-october-2026).

There are still **908 ZIP entries**. Compressed entries shrink by **298 bytes**;
container overhead shrinks by **2 bytes**. Largest compressed entries: IL2CPP
**11,689,062**, Unity **9,107,612**, metadata **2,668,294**, Java **2,353,575
bytes**. The small native/metadata differences accompany the movement code
change; no unexpected bulk growth was found.

Serialized assets rise **155,404,482 → 155,404,594 bytes (+112)**, entirely
from the new review script's metadata. Review logic is guarded by
`UNITY_EDITOR || DEVELOPMENT_BUILD`. Every existing packed asset size and all
**1,788 imported mesh records** match. No new texture, mesh, material or audio
resource is shipped for this fix; existing compression and visual quality are
unchanged. The pre-existing development-target gap remains unresolved.

All **213 compiled runtime documents** match the tested Windows source.
After Android, **3,654/3,656 Game input hashes** match the Windows snapshot;
only URP resource registration and generated Golf prefab IDs/order differ.
Format-aware comparison proves the regenerated prefab has identical objects,
properties and references after normalizing local IDs. Original IDs/order were
restored to avoid unrelated source churn; the generated copy and proof are kept.

Evidence: `Builds/NetworkMovementQA/Before/`, `apk-comparison.json`,
`asset-comparison.json`, `entry-comparison.json`, `apk-signature.txt`,
`compiled-source.json`, `source-comparison.json`,
`generated-prefab-equivalence.json` and `Builds/SizeAudit/latest/`.
Prior release/audit:
`Legacy/20261012-183556-915-sports-movement-before-android/`.

## Combined Football, Basketball and Fishing release — 12 October 2026

Actual signed APK: **88,856,012 → 90,139,324 bytes (+1,283,312)**,
written at **18:42:22 Malaysia time**. Strict-boundary headroom:
**9,860,676 bytes**; 75 MB development-target overrun:
**15,139,324 bytes**. SHA-256:
**FB9AC84E23EA34EA34FEF05B9534D07BB1F3DD1C731BE8DD4E3909227D300979**. The enforced budget gate and independent APK v2 signature pass.
[Release and limits](BUILD-SIZE.md#combined-football-basketball-and-fishing-release--12-october-2026).

Compressed ZIP entries total **88,762,914 → 89,997,825 bytes
(+1,234,911)**. Container/signature overhead changes
**93,098 → 141,499 bytes
(+48,401)**. There are **640 → 909 entries**,
with **573 unchanged named payload hashes**. The largest compressed entries are
IL2CPP **11,722,581**, Unity **9,108,106**, metadata **2,674,535** and Java
**2,353,635 bytes**. IL2CPP grows **159,451 compressed bytes**; Java is unchanged.

Serialized assets change **153,775,137 → 155,405,350 bytes
(+1,630,213)**. These are uncompressed Unity
measurements, not APK bytes. The incoming asset groups account for:

| Incoming group | Added serialized bytes |
| --- | ---: |
| 58 Basketball clips | 283,696 |
| 55 new Football clips | 190,152 |
| Basketball props, icons and materials | 366,417 |
| Football procedural props, icons and materials | 470,932 |
| Sports typography and shared utility atlas | 99,660 |
| Football HUD atlas | 59,288 |
| Basketball HUD atlas | 118,456 |

The remaining **41,612 serialized bytes** are the required particles shader
(**26,840**), shared prop-effect shader (**11,960**) and script metadata
(**2,812**). Football references **25 Basketball clips in place**; they are
not duplicated. Both banks remain mono 32 kHz Vorbis at the approved levels.
All 80 Football delivery rows and the earlier Basketball delivery rows match
source hashes. Raw audio, painted masters and Blender authoring stay outside Unity.

All **1,785 prior imported-FBX mesh rows** remain identical; **three new
Basketball props** produce **1,788 rows**. Football's four procedural `.asset`
meshes are outside this FBX-only CSV; their packed paths, rendered models and
collision behavior are checked separately. No pre-existing packed binary
asset gains serialized bytes. Existing Cove fonts, Fishing sprites and Golf
badges retain their serialized sizes. No new lossy change to prior world art,
models, animation or fonts was made by this merge.

MobileURP and MobileRenderer remain packed. No Desktop pipeline asset, SSAO
shader or BlueNoise256 asset is present. Existing BlueNoise64/script metadata
remain. All **13 checked development-only review types** are absent from
release IL2CPP metadata; MonoScript filename metadata is a separate measurement.
No unexplained bulk growth was found. The development-target shortfall remains
open and is not treated as spare capacity.

Current immutable comparison, ZIP hashes, asset/mesh deltas, independent
signature output and copied Unity report are in
`Builds/Publish/20261011-225324-fishing-merge/LatestApkComparison-20261012-104347/`.
The current report is also under `Builds/SizeAudit/latest/`.

## Approved football audio — 12 October 2026

Fresh signed APK: **90,099,410 bytes**, **+240,376** from the measured
89,859,034-byte baseline. Hard-boundary headroom: **9,900,590 bytes**;
75 MB development target exceeded by **15,099,410 bytes**.
[Release and hash](BUILD-SIZE.md#approved-football-audio--12-october-2026).

ZIP entries increase **797 → 908**, adding **220,234 compressed bytes** and
**20,142 bytes** of container overhead. Largest compressed entries: IL2CPP
**11,689,490**, Unity **9,107,612**, metadata **2,668,179**, Java **2,353,575
bytes**. New audio resources, licensing, small compiled code changes and metadata
explain the increase; no unexpected bulk growth was found.

Serialized assets rise **155,214,130 → 155,404,482 bytes (+190,352)**:
55 new football clips account for **190,152 bytes**, with 200 bytes of script
references. Serialized size is distinct from compressed APK size. Every existing
packed asset size and all **1,788 imported mesh records** match the prior release.
The 25 shared basketball clips are referenced at their original resource paths,
without duplicated files. Full source recordings and generators remain outside
Unity. Imports use 32 kHz mono Vorbis at quality 0.65 with normalization disabled;
the turf loop remains compressed in memory. Existing art is unchanged.

The increase stays below the proposed 500,000-byte incremental allowance.
Bounded voices, shared resources and short clips avoid needless audio growth;
this does not resolve the project's pre-existing 75 MB development-target gap.
All **212 compiled runtime documents** match the tested Windows player.
After Android, **3,650/3,652 Game input hashes** match. Only generated Golf
prefab IDs and URP resource registration differ.

Evidence: `Builds/FootballAudioQA/Before/`, `apk-comparison.json`,
`asset-comparison.json`, `entry-comparison.json`, `mesh-comparison.json`,
`apk-signature.txt`, `compiled-source.json`, `source-comparison.json`,
`portable-regeneration.json` and `Builds/SizeAudit/latest/`.
Prior release/audit:
`Legacy/20261012-170341-319-football-audio-before-android/`.

## Physical football wall and larger spawns — 12 October 2026

Fresh signed APK: **89,859,034 bytes**, **+7,526** from the measured
89,851,508-byte baseline. Hard-boundary headroom: **10,140,966 bytes**;
75 MB development target exceeded by **14,859,034 bytes**.
[Release and hash](BUILD-SIZE.md#physical-football-wall-and-larger-spawns--12-october-2026).

ZIP entries increase **796 → 797**, adding **7,352 compressed bytes** and
**174 bytes** of container overhead. Largest compressed entries: IL2CPP
**11,665,791**, Unity **9,107,612**, metadata **2,667,024**, Java **2,353,575
bytes**. Growth comes from native/managed collision code and metadata, the new
countdown material, and small generated build-data changes. No unexpected bulk
growth was found.

Serialized assets rise **155,212,466 → 155,214,130 bytes (+1,664)**, entirely
from `FootballProps/Countdown.mat`; this measurement is distinct from APK size.
Every existing packed asset size and all **1,788 imported mesh records** match.
The larger markers reuse the existing meshes and shared URP Lit shader at
runtime scale. Two pooled box colliders and one shared physics material provide
solid walls without new geometry, textures or effect assets. Existing art and
compression quality are preserved.

All **208 compiled runtime source documents** match the tested Windows player.
After Android, **3,526/3,529 Game input hashes** match. Generated Golf prefab IDs,
URP resource registration and the new material's legacy `_Color` alias differ.
Reconstructing the material before that alias update reproduces its exact
Windows-source hash; `_BaseColor`, shader and every other material property
are unchanged. The installed URP Lit shader renders `_BaseColor`.

Evidence: `Builds/FootballPhysicalWallQA/Before/`, `apk-comparison.json`,
`asset-comparison.json`, `entry-comparison.json`, `apk-signature.txt`,
`countdown-material-comparison.json`, `compiled-source.json`,
`source-comparison.json` and `Builds/SizeAudit/latest/`.
Prior release/audit:
`Legacy/20261012-162621-528-football-solid-before-android/`.

## Football Explore Offline props — 12 October 2026

Fresh signed APK: **89,851,508 bytes**, **+856** from the measured
89,850,652-byte baseline. Hard-boundary headroom: **10,148,492 bytes**;
75 MB development target exceeded by **14,851,508 bytes**.
[Release and hash](BUILD-SIZE.md#football-explore-offline-props--12-october-2026).

The same **796 ZIP entries** add **857 compressed bytes**, with one fewer byte
of container overhead. Largest compressed entries: IL2CPP **11,661,162**, Unity
**9,106,039**, metadata **2,666,619**, Java **2,353,575 bytes**. Small compiled
code/metadata/build-data differences explain the change. Serialized assets
remain **155,212,466 bytes**, distinct from the APK measurement. Every packed
asset size and all **1,788 imported mesh records** match the prior release.
No asset, texture, audio, model, font or package was added or degraded.

All **208 compiled runtime source documents** match the tested Windows player.
After Android, **3,525/3,527 Game inputs** match; only generated Golf prefab IDs
and URP resource registration differ. The two edited runtime source files,
development review and existing football delivery assets match.

Evidence: `Builds/FootballExplorePropsQA/Before/`, `apk-comparison.json`,
`asset-comparison.json`, `entry-comparison.json`, `apk-signature.txt`,
`compiled-source.json`, `source-comparison.json` and `Builds/SizeAudit/latest/`.
Prior release/audit: `Legacy/20261012-155613-395-football-explore-before-android/`.

## Football abilities — 12 October 2026

Fresh signed APK: **89,850,652 bytes**, **+150,996** from the measured
89,699,656-byte baseline. Hard-boundary headroom: **10,149,348 bytes**;
75 MB development target exceeded by **14,850,652 bytes**.
[Release and hash](BUILD-SIZE.md#football-abilities--12-october-2026).

Compressed ZIP entries increase **147,903 bytes**, with **3,093 bytes** of
additional container overhead. Entry count rises **778 → 796**. Largest
compressed entries remain IL2CPP **11,660,115**, Unity **9,106,039**, metadata
**2,666,796** and Java **2,353,575 bytes**. Growth is attributable to the small
football prop resource set, compiled ability/network/UI code, metadata and the
shared effect shader. No unexplained bulk growth was found.

Serialized assets rise **154,741,182 → 155,212,466 bytes (+471,284)**, a
different measurement from APK size. Four new combined mesh assets, eight
coloured materials plus a translucent panel material, four prefabs and one
**30,392-byte serialized** icon atlas comprise the football art. The atlas is
512 × 128, ASTC 6×6, without mipmaps. The existing shared shader grows by only
**1,492 serialized bytes**. All **1,788 pre-existing imported mesh records**
match; the four generated mesh assets are listed separately in packed-assets.
Existing fonts, textures, animation and audio are neither enlarged nor reduced
in quality. The approved concept image and editable SVG masters do not ship.

Runtime presentation pools pickups, the two team barriers, preview, portals
and countdown digits. Models share materials and have no player colliders;
wall/portal effects use the shared procedural shader without new effect
textures. The relocation check reproduces the exact atlas hash from a path
with spaces and an unrelated working directory. Native model masters are
deterministic C# geometry with no external Blender/FBX/image dependencies.

All **208 compiled runtime source documents** match the tested Windows player.
Of **3,527 Game input files**, **3,525** match after Android: generated Golf
prefab IDs and URP resource registration differ, as in prior releases. Runtime
C#, generated football resources and UI textures match the tested snapshot.

Evidence: `Builds/FootballPropsQA/Before/`, `apk-comparison.json`,
`entry-comparison.json`, `asset-comparison.json`, `apk-signature.txt`,
`compiled-source.json`, `windows-source.json`, `source-comparison.json`,
`portability.json` and `Builds/SizeAudit/latest/`. Prior APK/audit archived in
`Legacy/20261012-152732-503-football-abilities-before-android/`.

## 2D sports action icons — 12 October 2026

Fresh signed APK: **89,699,656 bytes**, **−2,168** from the start-of-task
89,701,824-byte baseline. Hard-boundary headroom: **10,300,344 bytes**;
75 MB development target exceeded by **14,699,656 bytes**.
[Release and hash](BUILD-SIZE.md#2d-sports-action-icons--12-october-2026).

Compressed ZIP entries decrease **2,167 bytes**, with **1 fewer byte** of
container overhead. Entry count stays **778**. Largest compressed entries:
IL2CPP **11,630,842**, Unity **9,105,512**, metadata **2,662,916** and Java
**2,353,575 bytes**. The two updated icon textures and small compiled/UI build
changes account for the difference; there is no unexplained bulk growth.

Serialized assets remain **154,741,182 bytes**, a different measurement from
the APK. Every packed asset size matches the baseline, including the existing
**118,456 / 59,288 / 30,392-byte** basketball, football and shared prop atlases.
Their dimensions and ASTC 6×6 imports are unchanged. Three high-resolution
2D illustration masters replace the shoe artwork outside Unity. No font,
world texture, animation or audio asset expands or loses quality. All
**1,788 mesh records** match independent of row order.

Evidence: `Builds/FootballPoseIconQA/Before/`, `apk-comparison.json`,
`entry-comparison.json`, `asset-comparison.json`, `apk-signature.txt`,
`art-validation.json`, `portability.json` and `Builds/SizeAudit/latest/`.
All three atlas hashes reproduce exactly when regenerated from a relocated
path with spaces and an unrelated working directory. The wider Tackle sprite
omits only fully transparent rows; source alpha ranges from zero to opaque.

All **202 compiled runtime documents** match the tested Windows player.
Of **3,476 Game inputs**, **3,474** match after Android: generated Golf prefab
IDs and URP resource registration differ, as in previous builds. Runtime C#,
font and texture inputs match. Relative to the prior Windows release, only
the two icon atlases, three HUD/style scripts and generated Golf prefab IDs
change. The final APK is compared to the original release, not the intermediate
build made before the shared Jump request.

Prior APK/audit: `Legacy/20261012-143459-868-football-flat-poses-before-android/`.
Intermediate builds and draft art are indexed in the verification record.

## Full-colour sports artwork — 12 October 2026

Fresh signed APK: **89,701,824 bytes**, **+57,436** from the measured
89,644,388-byte baseline. Hard-boundary headroom: **10,298,176 bytes**;
75 MB development target exceeded by **14,701,824 bytes**.
[Release and hash](BUILD-SIZE.md#full-colour-sports-artwork--12-october-2026).

Compressed ZIP entries increase **57,442 bytes**, with **6 fewer bytes** of
container overhead. Entry count remains **778**. Three texture entries account
for **56,738 bytes** of growth; the remaining net **704 bytes** are compiled
code, metadata and build/scene data. No unexplained bulk growth was found.
Largest compressed entries: IL2CPP **11,629,529**, Unity **9,105,512**,
metadata **2,662,916**, Java **2,353,575 bytes**.

Serialized assets remain **154,741,182 bytes**, a different measurement from
the APK. Every packed asset size matches the baseline. The three UI atlases
remain **118,456 / 59,288 / 30,392 serialized bytes**, using the same 512 × 512,
512 × 256 and 512 × 128 ASTC 6×6 textures. Ten high-resolution PNG masters stay
outside Unity. Existing fonts, world textures, audio and animations are not
expanded or degraded. All **1,788 mesh records** match independent of row order.

Evidence: `Builds/ColorIconQA/Before/`, `apk-comparison.json`,
`entry-comparison.json`, `asset-comparison.json`, `apk-signature.txt`,
`portability.json` and `Builds/SizeAudit/latest/`. Regenerating all three atlases
from an unrelated working directory and a project path with spaces produces
identical hashes. All **202 compiled runtime documents** match the tested
Windows player. Of **3,476 Game inputs**, **3,474** match after Android:
generated Golf prefab IDs and URP resource registration differ, as in prior
builds. Runtime C#, fonts, texture imports and icon atlases match.

Previous APK/audit: `Legacy/20261012-015824-930-colour-icons-before-android/`.

## Sports icons and typography — 12 October 2026

Fresh signed APK: **89,644,388 bytes**, **+54,394** from the measured
89,589,994-byte baseline. Hard-boundary headroom: **10,355,612 bytes**;
75 MB development target exceeded by **14,644,388 bytes**.
[Release and hash](BUILD-SIZE.md#sports-icons-and-typography--12-october-2026).

Compressed ZIP entries increase **53,737 bytes**, plus **657 bytes** of
container overhead. Entry count is **774 → 778**. Largest compressed entries:
IL2CPP **11,628,857**, Unity **9,105,512**, metadata **2,662,875**, Java
**2,353,575 bytes**. This remains a small UI change within a project that
already exceeds its development budget; no world assets were degraded.

Serialized assets increase **154,641,426 → 154,741,182 (+99,756 bytes)**,
a different measurement from the APK. All changes are accounted for:

| New asset | Serialized bytes |
| --- | ---: |
| Display font, Manrope 800 subset | 34,690 |
| Text font, Manrope 600 subset | 34,578 |
| Shared prop/check ASTC 6×6 atlas | 30,392 |
| Shared sports UI script record | 96 |

Existing basketball/football atlases remain **118,456 / 59,288 serialized
bytes**. All other packed asset sizes match. All **1,788 mesh records** match
when compared independent of report order. No existing world texture, audio
or animation record changes. The 16,471-byte combined third-party notice is
verified byte-for-byte inside the APK; compressed delivery costs are included
in the total above.

Evidence: `Builds/SportsIconQA/Before/`, `apk-comparison.json`,
`asset-comparison.json`, `apk-signature.txt`, `portability.json` and
`Builds/SizeAudit/latest/`. Regeneration from an unrelated working directory
and a project path with spaces produces identical font and atlas hashes.
All 202 compiled runtime documents match the tested Windows player. Of 3,476
recorded Game inputs, 3,474 match after Android: generated Golf prefab IDs and
URP resource registration differ, as in previous builds. Runtime C#, fonts,
icon imports and textures match. StreamingAssets license text was copied
identically into the Windows candidate and is automatically included by fresh
Unity builds.

Previous APK/audit: `Legacy/20261012-010856-667-sports-icons-before-android/`.

## Football touchline UI — 12 October 2026

Fresh signed APK: **89,589,994 bytes**, **+47,078 bytes** from the measured
start-of-task release. Hard-boundary headroom: **10,410,006 bytes**; the
75 MB development target is exceeded by **14,589,994 bytes**.
[Release and hash](BUILD-SIZE.md#football-touchline-ui--12-october-2026).

Compressed ZIP entries increase **46,910 bytes**, with **168 bytes** of extra
container overhead. Entry count is **773 → 774**. The new football icon atlas
accounts for **26,739 compressed bytes**; IL2CPP adds **17,314**, metadata
**2,456**. Largest compressed entries remain IL2CPP **11,629,168**, Unity
**9,105,512**, metadata **2,662,664** and Java **2,353,575 bytes**. No unexplained
bulk growth was found.

Serialized assets increase **154,581,754 → 154,641,426 bytes (+59,672)**,
which is not the APK measurement. The 512 × 256 ASTC 6×6 icon atlas contributes
**59,288 bytes**; four script records contribute **384 bytes**. Runtime panels,
paths, power ring and minimap use geometry. Existing basketball utility icons
and Cove fonts are shared. The reference pitch screenshot and HTML are outside
Unity. All **1,788 mesh rows** match the baseline; no existing texture, animation
or audio record changes. The 75 MB target still needs broader project work;
this UI adds no redundant font, baked screen or large texture dependency.

Evidence: `Builds/SizeAudit/latest/`, `Builds/FootballUIQA/Before/`,
`apk-comparison.json`, `asset-comparison.json`, `apk-signature.txt` and
`portability.json`. Relocated regeneration from an unrelated working directory
reproduces the exact atlas hash. All 201 compiled runtime source documents
match the tested Windows player. Of 3,462 captured Game inputs, 3,459 match
following Android generation: Golf ball prefab IDs and URP resource registration
are regenerated; the editor-only player writer adds the football launcher and
updated release caption. The identical launcher is included in the delivered
Windows folder. Runtime code and icon imports match.

The previous APK and audit are preserved in
`Legacy/20261012-002527-083-football-ui-before-android/`.

## Fishing feedback, timing and round recap — 11 October 2026

The fresh AndroidSubmission APK was written at **21:16:21.948 Malaysia time**.
Actual signed APK size: **88,812,118 → 88,856,012 bytes (+43,894)**.
Headroom to the strict 100,000,000-byte boundary is **11,143,988 bytes**.
The 75,000,000-byte development target remains exceeded by **13,856,012 bytes**.
APK SHA-256: **8766DC73759F316754896D4CF0E11AA2D1F3290A688C593CC3DC5853824749C7**.
The hard budget gate and independent APK v2 signature verification pass.
[Release record](BUILD-SIZE.md#fishing-feedback-timing-and-round-recap--11-october-2026).

Compressed ZIP entry sums change **88,719,188 → 88,762,914 bytes (+43,726)**.
Container/signature overhead changes **92,930 → 93,098 bytes (+168)**.
The largest compressed entries are IL2CPP **11,563,130**, Unity **9,094,072**,
metadata **2,653,490** and Java **2,353,635 bytes**. The largest positive deltas
are IL2CPP **+34,322**, metadata **+7,295** and regenerated Burst native code
**+1,642 bytes**. Together these account for **43,259 bytes** of the ZIP growth.
Unity's native binary changes too, decreasing by 195 compressed bytes; Java's
DEX payload is unchanged. These compiled/native output changes are distinct
from imported game art, meshes, fonts and audio.

Serialized assets change **153,774,361 → 153,775,137 bytes (+776)**.
The editable results layout adds **160 bytes** and six MonoScript metadata
rows add **616 bytes**. No pre-existing packed asset's serialized byte count
changes: **3,188 rows remain the same**. No new or enlarged packed texture,
font, mesh, animation, material, shader or imported audio asset is reported.
The new runtime sounds are synthesized rather than shipped as an audio pack.

The six new MonoScript rows include **208 bytes** of native/source-file
metadata for the two guarded review files. The executable
`FishingPolishReview`, `FishingResultsChecks`, `FishingFeedbackChecks` and
`FishingGameplayProbe` types are absent from the release IL2CPP metadata.
Their filename metadata is not executable review code, and the size is
included in the 776-byte serialized increase.

All **1,785 mesh report rows** retain identical field values. Both Cove fonts,
all six Fishing sprites and both Golf badges retain their serialized layout,
serialized sizes and the SHA-256 of their entire decompressed packed APK
files. The two fonts total **64,916 serialized bytes**, the six Fishing sprites
**88,988**, and the two Golf badges **39,116**. This is actual packed-payload
integrity, not merely a source-folder size comparison.

The APK has **639 → 640 ZIP entries**. Two apparent added/removed GUID entries
are the unchanged PerformanceTestRunSettings/PerformanceTestRunInfo resources:
their decompressed payload hashes and compressed byte counts match exactly.
Only the results layout adds a genuine new data-file entry, **208 compressed
bytes**. Overall, **576 entries** retain the same name and payload SHA-256;
the two renamed unchanged resources are recorded separately.

MobileURP and MobileRenderer remain packed. There is no Desktop pipeline
asset, SSAO shader or BlueNoise256 asset. The pre-existing BlueNoise64 texture
and **336-byte** SSAO script metadata remain unchanged. No unexpected imported
binary growth was found. The substantial development-target shortfall remains
and is not treated as comfortable spare capacity.

Evidence is in `Builds/FishingPolishQA/Task-20261011-143503/ApkComparison-20261011-131850/`: `comparison.json`,
`apk-entry-delta.csv`, `serialized-asset-delta.csv`, `mesh-row-delta.csv`,
`retained-asset-integrity.json`, `added-removed-zip-classification.json`,
`release-type-check.json`, `apk-signature.txt` and the immutable
`AfterSizeAudit/` copy. Baseline ZIP hashes are in the task's `BeforeApkZip.json` and the
baseline audit is in `BeforeSizeAudit/`. The baseline APK is recoverable under
`Legacy/20261011-205048-934-fishing-polish-before-android/` at repository root.

This audit measures packaging and signatures. Functional evidence is in the
[verification record](VERIFICATION.md#fishing-feedback-timing-and-round-recap--11-october-2026); it does not establish physical-phone sound,
haptics, appearance, touch, FPS, thermals or separate-network quality.
New celebrations and physical-phone testing remain deferred.

## Basketball gameplay audio — 11 October 2026

Fresh signed APK: **89,542,916 bytes**, **+340,662 bytes** from the measured
start-of-integration APK. Hard-boundary headroom: **10,457,084 bytes**; the
75 MB working target remains exceeded. [Release and hash](BUILD-SIZE.md#basketball-gameplay-audio--11-october-2026).

Compressed ZIP entries increase **319,573 bytes**, and container overhead
increases **21,089 bytes**. Entry count rises **657 → 773** with the separate
audio resources. The largest new resource is coastal air at **88,320 bytes**;
the next two are Turbo and rolling textures at **14,944** and **11,456 bytes**.
IL2CPP increases **24,726 bytes**. Largest compressed entries remain IL2CPP
**11,611,854**, Unity **9,105,512**, metadata **2,660,208** and Java
**2,353,575 bytes**. No unexplained bulk growth.

Serialized assets increase **154,297,738 → 154,581,754 bytes (+284,016)**,
which is a separate measurement from compressed APK size. The 58 audio clips
account for **283,696 bytes**; three script records account for the remaining
320 bytes. Mono 32 kHz Vorbis at quality 0.65 preserves the approved mix;
normalization is disabled. Reusing bounce variants at different gains and
combining actual rim/net events avoids redundant soft/hard/dunk recordings.
All **1,788 mesh rows** match, and existing texture/animation records are
unchanged. The retained source packs, previews, reels and unused alternatives
are outside Unity and excluded from the APK.

Evidence: `Builds/SizeAudit/latest/`, `Builds/BasketballAudioQA/Before/`,
`apk-comparison.json`, `asset-comparison.json` and `apk-signature.txt`.
Source comparison matches **3,445 / 3,447 Game inputs**. Only generated Golf
ball prefab serialization and URP resource registration differ between Windows
and Android generation; runtime C#, audio bytes and import settings match.
Superseded APK/audit and successful regeneration fixture are preserved at
`Legacy/20261011-191701-160-basketball-audio-before-android/`.

## Basketball inbound and touch fixes — 11 October 2026

Fresh signed APK: **89,202,254 bytes**, **+1,928 bytes** from the measured
start-of-task APK. Hard-boundary headroom: **10,797,746 bytes**; the 75 MB
working target remains exceeded. [Hash and release](BUILD-SIZE.md#basketball-inbound-and-touch-fixes--11-october-2026).

Compressed ZIP entries increase **1,929 bytes**; container overhead decreases
one byte. Largest changes: IL2CPP **+983**, metadata **+607**, icon data **+242**
bytes. Largest compressed entries are IL2CPP **11,587,128**, Unity **9,103,972**,
metadata **2,655,751** and Java **2,353,575 bytes**. Hashed asset entry renames
have equal sizes and do not add delivery content. No unexplained bulk growth.

Serialized assets increase **154,297,626 → 154,297,738 bytes (+112)**, a
separate measurement from compressed APK size. The added record is the inbound
review script's tiny asset record; development review code is conditionally
excluded from the release. The icon atlas stays **118,456 bytes**, using the
same ASTC 6×6 import with no mipmaps or CPU copy. All **1,788 mesh rows** match
when sorted; their report order changes. No new font, package, model, texture
sheet, animation or audio is introduced. Largest existing serialized assets
remain the golf/fishing terrain control maps, not this UI update.

Evidence: `Builds/SizeAudit/latest/` and `Builds/BasketballInboundQA/Before/`,
`apk-comparison.json`, `asset-comparison.json`, `apk-signature.txt` and
`android-build.txt`. Source comparison matches **3,318 / 3,320 Game inputs**;
only generated Golf ball prefab serialization and URP resource registration
change between Windows and Android generation. Runtime C#, UI art and fonts match.

## Basketball mobile controls — 11 October 2026

Fresh signed APK: **89,200,326 bytes**. Increase from the start-of-task
APK: **83,674 bytes**; increase from the intervening camera release:
**21,748 bytes**. Hard-boundary headroom: **10,799,674 bytes**.
[Release and hash](BUILD-SIZE.md#basketball-mobile-controls--11-october-2026).
The 75 MB development target remains exceeded.

Compared with the camera release, compressed ZIP entries grow **22,089
bytes** and container overhead shrinks **341 bytes**. IL2CPP grows **25,446** and
metadata **4,390 bytes**; the icon data entry shrinks **7,534 bytes**. Largest
compressed entries are IL2CPP **11,586,145**, Unity **9,103,972**, metadata
**2,655,144** and Java **2,353,575 bytes**. Other differences are retained in
`Builds/BasketballUIQA/apk-comparison.json`.

Serialized assets change **154,528,410 → 154,297,626 bytes
(−230,784)**, a different measurement from compressed APK size. The atlas changes
**349,672 → 118,456 bytes (−231,216)** through ASTC 6×6, no mipmaps and no CPU copy.
Four tiny script records add 432 bytes. All **1,788 mesh rows** match. The packing
still excludes DesktopCoastURP, BlueNoise256 and the SSAO shader; existing package
script records are distinct from the excluded shader. No new font or UI package
is added. Frames, shadows, paths and meters use procedural geometry.

Evidence: `Builds/SizeAudit/latest/`, `Builds/BasketballUIQA/Before/`,
`PreRelease/`, `apk-comparison.json`, `apk-signature.txt`, `windows-source.json`
and `source-delta.json`. All runtime code, UI art and fonts match between the
validated Windows inputs and final Android source. Two generated inputs differ:
the Golf ball prefab and URP resource registration. These are recorded as build
source limits, not claimed to be byte-identical. Previous release/audit:
`Legacy/20261011-171516-579-basketball-ui-before-android/`.

## Ball-first Stadium camera — 11 October 2026

Fresh signed APK: **89,116,652 → 89,178,578 bytes (+61,926)**. Strict-boundary
headroom: **10,821,422 bytes**; development target exceeded by **14,178,578 bytes**.
[Release, snapshot limits and hash](BUILD-SIZE.md#ball-first-stadium-camera--11-october-2026).

Compressed ZIP entries grow **61,410 bytes**; container overhead adds 516.
Largest compressed entries: IL2CPP **11,560,699**, Unity **9,103,263**, metadata
**2,650,754**, Java **2,353,575 bytes**. The largest increase is the new icon
sheet data entry (**57,436 bytes**); IL2CPP grows **2,361 bytes**. The camera
introduces no texture, font or imported mesh. Its arrow is generated UI geometry.

Serialized assets change **154,178,410 → 154,528,410 bytes (+350,000)**. This is
**349,672 bytes** for `Resources/BasketballUI/Icons.png`, created by concurrent UI
work before the final Windows build, plus **328 bytes** of script records.
That sheet is retained so the Android snapshot matches the tested Windows
inputs; the subsequent UI source edits are excluded. All **1,788 mesh records**
match the baseline. Mobile packing excludes DesktopCoastURP, BlueNoise256 and
the SSAO shader. No model or texture was degraded for this camera change.

Evidence: `Builds/BallCameraQA/apk-comparison.json`, `apk-signature.txt`,
`BeforeAudit/`, `windows-source.json`, `snapshot-verification.json`,
`android-source-delta.json`, `generated-source-check.json`, and
`Builds/SizeAudit/latest/`. Runtime code matches across both builds; generated
Golf prefab IDs/order are semantically identical and Android restores mobile
rendering registrations. Shared concurrent source was not normalized or reset.
Prior release/audit: `Legacy/20261011-164821-353-ball-camera-before-android/`.

## Centre-line prop countdown — 11 October 2026

Fresh signed APK: **89,113,480 → 89,116,652 bytes (+3,172)**. Strict-boundary
headroom: **10,883,348 bytes**; development target exceeded by **14,116,652 bytes**.
[Release and hash](BUILD-SIZE.md#centre-line-prop-countdown--11-october-2026).

Compressed ZIP entries grow **3,174 bytes**, with two fewer bytes of container
overhead. Largest entries: IL2CPP **11,558,338**, Unity **9,103,347**, metadata
**2,650,706**, Java **2,353,575 bytes**. One serialized data entry grows 2,009
compressed bytes; Burst grows 545, metadata 541 and IL2CPP 120 bytes. Other entry
changes are recorded in `Builds/BasketballSpawnQA/apk-comparison.json`.

Serialized assets change **154,175,974 → 154,178,410 bytes (+2,436)**, entirely
the shared prop effect shader (**8,032 → 10,468 bytes**). All **1,788 mesh records**
match the baseline; only report row order differs. No textures, fonts or new
packed meshes are added. Five small digit meshes are generated once at runtime;
there is only one active pickup or countdown. Mobile packing still excludes
DesktopCoastURP, BlueNoise256 and the SSAO shader.

Evidence: `Builds/SizeAudit/latest/`, `Builds/BasketballSpawnQA/Before/`,
`apk-comparison.json` and `apk-signature.txt`. Prior APK/audit:
`Legacy/20261011-155414-437-basketball-countdown-before-android/`.

## Visible basketball props and ability tuning — 11 October 2026

Fresh signed APK: **89,109,564 → 89,113,480 bytes (+3,916)**. Strict-boundary
headroom: **10,886,520 bytes**. Development target exceeded by **14,113,480 bytes**.
[Release and hash](BUILD-SIZE.md#visible-basketball-props-and-ability-tuning--11-october-2026).

Compressed ZIP entries grow **3,920 bytes**; container/signature overhead shrinks
four bytes. IL2CPP accounts for **3,885 bytes** of growth. Largest entries:
IL2CPP **11,558,218**, Unity **9,103,347**, metadata **2,650,165**, Java
**2,353,575 bytes**. There is no new shipped art or dependency.

Serialized assets stay at **154,175,974 bytes**, with every per-asset size
unchanged. All **1,788 mesh report rows** are identical. The prop import settings
are retained; the fix preserves FBX unit scale during runtime animation.
Mobile packing still excludes DesktopCoastURP, BlueNoise256 and the SSAO shader.

Evidence: `Builds/SizeAudit/latest/` and `Builds/BasketballPropFixQA/` files
`apk-comparison.json`, `Before/` and `apk-signature.txt`. Prior APK/audit:
`Legacy/20261011-140500-474-basketball-prop-fix-before-android/`.

## Basketball props — 11 October 2026

Fresh signed APK: **88,560,698 → 89,109,564 bytes (+548,866)**. Strict-boundary
headroom is **10,890,436 bytes**; the 75 MB target remains exceeded by
**14,109,564 bytes**. The old local APK predates newer source and HUD work;
the difference from the last documented 88,812,118-byte release is 297,446 bytes.
[Release and hash](BUILD-SIZE.md#basketball-props--11-october-2026).

Compressed ZIP entries grow **543,870 bytes**, with **4,996 bytes** more
container/signature overhead. The largest entries remain IL2CPP **11,554,333**,
Unity **9,103,347**, metadata **2,650,185** and Java **2,353,575 bytes**.

Serialized assets change **153,637,593 → 154,175,974 bytes (+538,381)**.
New prop resources total **374,449 bytes**: **260,493** for the three models,
**92,956** for the three sprites, **8,032** for the portal shader and **12,968**
for shared materials. The trail also adds **26,840 bytes** of the URP unlit
particle shader. These figures are not compressed APK measurements.

The model importers use medium mesh compression and disable animation and CPU
readability. Icon importers cap Android at 256 pixels, ASTC 6×6, without mipmaps.
There are at most two world pickups, using shared meshes/materials, two reused
portal planes and one small procedural effect shader. No effects package,
portal capture cameras, full-resolution icon masters or Blender scenes ship.

All **1,785 existing mesh report rows are unchanged**; three prop meshes bring
the total to **1,788**. MobileURP is packed; no DesktopCoastURP, BlueNoise256 or
SSAO shader appears. Baseline-to-current Fishing/Golf sprite changes are the
pre-existing source/delivery gap, not part of this feature's source edits.

Evidence: `Builds/SizeAudit/latest/`, `Builds/BasketballPropsQA/apk-comparison.json`,
`retained-meshes.json`, `asset-audit.json` and `apk-signature.txt` in that task's
QA directory. The previous APK/audit is recoverable from
`Legacy/20261011-125700-224-basketball-props-before-android/`.
The hard gate passes; the development target remains unmet.

## Golf sticker HUD — 10 October 2026

Actual fresh signed APK: **88,760,152 → 88,812,118 bytes (+51,966)**.
Strict-boundary headroom: **11,187,882 bytes**. The 75 MB development target
remains exceeded by **13,812,118 bytes**.
[Release and hash](BUILD-SIZE.md#golf-sticker-hud--10-october-2026).

Compressed ZIP entries grow **51,449 bytes**, with **517 bytes** more
container/signature overhead. The largest compressed entries are IL2CPP
**11,528,808**, Unity **9,094,267**, metadata **2,646,195** and Java
**2,353,635 bytes**. The two new texture ZIP entries total **27,394 bytes**.
These are actual APK measurements, separate from source PNG lengths.

Serialized assets change **153,734,549 → 153,774,361 bytes (+39,812)**.
Two Golf sprites contribute **30,536 + 8,580 = 39,116 bytes**; the editable
layout contributes **336**, and four MonoScript metadata rows add **360**.
Binary Texture2D inspection confirms **256/128 pixels**, **ASTC 6×6**,
one mip level and no CPU readability, with **37,328 raw image bytes** total.
Full-resolution masters and the approved preview stay outside Unity and do
not ship. Existing fonts, portraits, frames, clock and icons are reused.

All **1,785 mesh report rows** are identical. Both Cove fonts and all six
Fishing sprites retain their serialized sizes and packed payload SHA-256.
MobileURP is packed, with **no DesktopCoastURP, SSAO shader or BlueNoise256
assets**. The existing **336-byte SSAO script metadata** and **4,220-byte
BlueNoise64 texture** remain unchanged; those are not new desktop effects.

Current evidence is `Builds/SizeAudit/latest/` and
`Builds/GolfHUDQA/Task-20261010-125740/AfterComparison/`: exact APK ZIP and
serialized asset deltas, texture inspection, retained-asset hashes and pipeline
checks. Independent APK v2 verification is in the task's `apk-signature.txt`.
The baseline APK/audit is recoverable in
`Legacy/20261010-141911-246-golf-hud-before-android/`.
The hard budget passes; the 75 MB development target remains unmet.

## Fishing timer spacing — 9 October 2026

Fresh signed APK: **88,758,740 → 88,760,152 bytes (+1,412)**; headroom to the
strict boundary is **11,239,848 bytes**. The 75 MB target remains exceeded
by **13,760,152 bytes**. [Release and hash](BUILD-SIZE.md#fishing-timer-spacing--9-october-2026).

Compressed ZIP entries change **+1,410 bytes**,
with signing/container overhead **+2**.
Largest compressed entries are IL2CPP **11,508,983**, Unity
**9,094,267**, metadata **2,642,676**,
and Java **2,353,635 bytes**. Serialized assets change
**153,734,541 → 153,734,549 bytes**;
this is a separate measurement from the actual APK length.

All **1785 mesh report rows** retain identical field values.
The same two Cove fonts and six compressed HUD sprites are unchanged;
no packed asset path is added or removed. MobileURP is packed, with no
unintended DesktopCoastURP/SSAO shader/blue-noise texture growth.
Current evidence is `Builds/SizeAudit/latest/` and `Builds/TimerCardQA/Task-20261009-231529/After/SizeAudit/`,
plus actual APK ZIP and serialized asset comparisons in `Builds/TimerCardQA/Task-20261009-231529/`.

## Fishing Hook cue and readable controls — 9 October 2026

Actual fresh signed APK: **88,742,452 → 88,758,740 bytes (+16,288)**.
Strict-boundary headroom is **11,241,260 bytes**; the 75 MB development
target remains exceeded by **13,758,740 bytes**.
[Release and hash](BUILD-SIZE.md#fishing-hook-cue-and-readable-controls--9-october-2026).

Compressed ZIP entries grow **16,288 bytes**;
signing/container overhead changes **+0 bytes**.
Largest compressed entries: IL2CPP **11,507,560**, Unity
native code **9,094,267**, metadata **2,642,733**
and Java **2,353,635 bytes**. These are actual APK entries,
not source-folder sizes or uncompressed Unity report totals.

Serialized assets change **153,734,377 →
153,734,541 bytes (+164)**,
a different measurement. All **1,785 mesh report rows**
remain byte-identical; the two Cove fonts and six HUD sprites are unchanged.
Sprites still total **88,988 serialized bytes**, with **84,336 raw ASTC bytes**.
The additional delivery cost comes from compiled code, small script metadata
and editable layout settings. No new art pack or runtime dependency appears; the development
target remains unmet.

A rejected intermediate APK measured **91,235,832 bytes** because an
interrupted desktop build left DesktopCoastURP selected. The audit exposed
desktop SSAO and blue-noise resources in the mobile package. Android build
entry points now explicitly require and save MobileURP before building.
The final audit verifies those unintended desktop dependencies are absent;
the rejected APK is archived as diagnostic evidence.

Current evidence: `Builds/SizeAudit/latest/`, copied to
`Builds/FishingRippleQA/Task-20261009-162733/After/SizeAudit/`, plus `release.json`, `apk-entry-delta.csv`,
`serialized-asset-delta.csv`, `asset-invariance.json` and `apk-signature.txt`.
Intermediate and original APK/player output is recoverable in the archive
batches listed in [verification](VERIFICATION.md#fishing-hook-cue-and-readable-controls--9-october-2026).

## Integrated main — 9 October 2026

Actual fresh signed APK: **90,051,054 → 88,742,452 bytes (−1,308,602)**.
Strict-boundary headroom is **11,257,548 bytes**. The 75 MB development target
remains exceeded by **13,742,452 bytes**.
[Release, hash and baseline](BUILD-SIZE.md#integrated-main--9-october-2026).
The comparison is against the local fishing branch APK; the teammate protocol
37 records below describe a separate historical branch.

Exact compressed ZIP entries shrink **1,308,454 bytes**, with another
**148 bytes** less signing/container overhead. The largest compressed entries
are IL2CPP **11,492,196 bytes**, Unity native code **9,094,267**, managed
metadata **2,641,881** and Java **2,353,635**. These are APK-entry measurements,
not source folder sizes or estimates of uncompressed assets.

Serialized assets decrease **156,394,629 → 153,734,377 bytes (−2,660,252)**,
a different measurement from compressed APK length. Seven shared character
animation imports save **2,676,924 serialized bytes**: idle **1,942,528** and
six turn/tackle clips **734,396** together. The retained import optimizer uses
conservative keyframe reduction; original FBXs and editable masters remain.
See [compression/dependency rationale](HUD-RESOURCE-CLEANUP.md).

The original Golf terrain FBX shrinks **275,780 serialized bytes**, while
three active derived terrain sectors add the same **275,780**. This substitutes
delivery geometry rather than claiming the whole source terrain was removed.
The Golf guide shader adds **7,992 serialized bytes**, its material **80**,
and shared built-in dependencies grow **7,640**. Small script/settings changes
account for the remaining net change. Fishing reuses its existing fish meshes,
LODs, fonts and compressed HUD sprites. No large new art pack or runtime
package appears in the audit. The development target is still unmet.

Current evidence: `Builds/SizeAudit/latest/` and
`Builds/Publish/20261009-131113/After/SizeAudit/`, plus `release.json`,
`apk-entry-delta.csv`, `serialized-asset-delta.csv` and `apk-signature.txt`.
The superseded local APK/audit is recoverable in
`Legacy/20261009-151219-684-publish-main-before-android/`.

## Fishing aim and readability — 9 October 2026

**Historical local fishing branch build (protocol 35), before main integration.**

Actual signed APK: **90,047,202 → 90,051,054 bytes (+3,852)**. Headroom to
the strict limit is **9,948,946 bytes**; the 75 MB development target remains
exceeded by **15,051,054 bytes**.
[Release and hash](BUILD-SIZE.md#fishing-aim-and-readability--9-october-2026).

Compressed ZIP entries grow **3,853 bytes**; container/signing overhead shrinks
**1 byte**. IL2CPP contributes **+3,325**, managed metadata **+374**, Unity
native code **+123** and remaining data **+31 bytes**. Largest compressed
entries are IL2CPP **11,450,589 bytes**, Unity **9,095,015 bytes**, metadata
**2,639,186 bytes** and Java **2,353,635 bytes**. The new reticle and extra fish
reuse existing code-native graphics and shared assets; no imported art or
runtime dependency is added.

Serialized assets grow **24 bytes**, from **156,394,605 to 156,394,629 bytes**,
solely in `FishingUILayout.asset`. This is a different measurement from APK
bytes. All **1,785 mesh report rows** are byte-identical; fonts and the six HUD
sprites are unchanged. The sprites still total **88,988 serialized bytes**,
including **84,336 bytes** of raw ASTC texture blocks. No avoidable large asset
growth was found. The development target remains unmet.

Current evidence: `Builds/SizeAudit/latest/`, copied to
`Builds/FishingAimQA/After/SizeAudit/`, plus `release.json`,
`apk-entry-delta.csv`, `serialized-asset-delta.csv` and `apk-signature.txt`.
The reviewed previous APK/audit is recoverable in
`Legacy/20261009-105020-349-fishing-aim-before-android/`.

## Lagoon Stickers fishing — 9 October 2026

Actual signed APK: **89,938,934 → 90,047,202 bytes (+108,268)**. Strict-limit
headroom is **9,952,798 bytes**; the 75 MB development target remains exceeded
by **15,047,202 bytes**. [Release and hash](BUILD-SIZE.md#lagoon-stickers-fishing--9-october-2026).

Compressed ZIP entries grow **106,897 bytes**, with **1,371 bytes** of additional
container/signing overhead. IL2CPP grows **39,653 bytes** and managed metadata
**4,806 bytes**. Largest compressed entries remain IL2CPP **11,447,264 bytes**,
Unity native code **9,094,892 bytes**, metadata **2,638,812 bytes** and Java
**2,353,635 bytes**. The detailed entry comparison is retained in
`Builds/FishingUXQA/apk-entry-delta.csv`.

Serialized assets grow **93,472 bytes** to **156,394,605 bytes**, a different
measurement from APK bytes. The six sprites account for **88,988 serialized
bytes**, including sprite/texture headers. Their raw ASTC 6×6 texture blocks
total **84,336 bytes**: 64-pixel flame, 128-pixel fish/fisher/clock badges and
256-pixel frame/reel button. Mipmaps and CPU readability are disabled; Android
import limits are saved in metadata. The importer and candidate builder enforce
these limits, while the original 1254-pixel RGBA source PNGs stay editable.
The shared custom line shader, smaller line material, layout configuration and
script metadata account for the remaining net growth.

The approved design reference is outside the Unity project. No new mesh,
animation, audio, package or large shipped dependency appears in the packing
audit. Existing fish/rod meshes, LODs, materials and Cove fonts are shared.
The 75 MB shortfall remains unresolved; additional asset packs would consume a
limited reserve. Current records: `Builds/FishingUXQA/Before/`, `After/SizeAudit/`,
`release.json`, `serialized-asset-delta.csv`, `apk-signature.txt`, source and
delivery verification. The superseded intermediate APK is recoverable in
`Legacy/20261009-015307-440-fishing-intermediate-apk/`.

## Golf cycling power and accurate shot guide — 9 October 2026

**Historical teammate branch build (protocol 37), before fishing integration.**

Actual signed APK: **88,554,742 → 88,560,698 bytes
(+5,956)**. Headroom: **11,439,302 bytes**.
The 75 MB target remains exceeded by **13,560,698 bytes**.
[Release/hash](BUILD-SIZE.md#golf-cycling-power-and-accurate-shot-guide--9-october-2026).

Serialized assets grow **1,428 bytes**, from **153,636,165 to 153,637,593**.
The reused procedural guide shader grows **1,236 serialized bytes**; two small
script records add **184 bytes**, and the shared physics settings add **8**.
Development-only test behavior is stripped from release; its MonoScript record
can remain. The guide material and all island, character, prop, texture, audio
and other rendering assets have unchanged serialized sizes.

Exact compressed ZIP entries grow **5,962 bytes**; container/signing overhead
shrinks **6 bytes**. IL2CPP adds **3,931 compressed bytes**, guide shader data
**1,044**, and metadata **822**. The largest compressed contributors remain
IL2CPP **11,406,190 bytes**, Unity native code **9,071,934**, metadata **2,632,590**,
and Java code **2,353,575**. No large new dependency or duplicate asset appeared.
The existing material loads the same guide shader; no new bitmap or model is used.
Legacy rollout/readout code and the blended charge setting were replaced in place.
The other sports' guides and editable authoring masters remain actively required.

Evidence: `Builds/GolfShotQA/before.json`, `release.json`, `packing-summary.json`,
`apk-entry-delta.csv`, `apk-signature.txt`, `asset-preservation.json`,
`source-verification.json`, `size-before/` and `size-final/`.
The earlier validation APK was archived after the final development review fix;
these figures describe the freshly rebuilt final source.

## Golf aiming lock and shot guide — 8 October 2026

Actual signed APK: **88,531,618 → 88,554,742 bytes (+23,124)**.
Hard-limit headroom: **11,445,258 bytes**. The 75 MB target remains exceeded
by **13,554,742 bytes**. [Release/hash](BUILD-SIZE.md#golf-aiming-lock-and-shot-guide--8-october-2026).

Serialized assets grow **7,116 bytes**, from **153,629,049 to 153,636,165**.
The new procedural guide shader accounts for **6,756 serialized bytes**, its
material **80 bytes**, script records **272 bytes**, and the network athlete
prefab **8 bytes**. Development test logic is compiled out of the release;
its tiny MonoScript record can still appear in the packed-asset report.
These are serialized measurements, distinct from the compressed APK.

Exact compressed ZIP entries grow **22,776 bytes**, with another **348 bytes**
of container/signing overhead. IL2CPP grows **16,605 compressed bytes**, managed
metadata **1,349**, and Unity native code **454**; Java code is unchanged.
The new shader data entry is **3,823 compressed bytes**. The new guide uses
no texture or imported mesh; a single mesh is created and reused at runtime.

All existing terrain, grass, character, structure, equipment and texture asset
serialized sizes are unchanged. The previously largest retained art contributors
remain unchanged; no large new dependency or avoidable duplicate was introduced.
The original basketball/football guides remain active in their sports. The
Chinese cart-label font remains required by both cart controls. No existing source
asset was made obsolete by replacing the old camera-only Aim behavior.

Evidence: `Builds/GolfAimQA/before.json`, `release.json`, `packing-summary.json`,
`apk-entry-delta.csv`, `apk-signature.txt`, `source-verification.json`,
`size-before/` and `size-final/`. Superseded deliveries and redundant test-player
copies were archived; [recovery batches](VERIFICATION.md#golf-aiming-lock-and-shot-guide--8-october-2026).

## Golf playability and integrated greens — 8 October 2026

Actual signed APK: **88,529,938 → 88,531,618 bytes (+1,680)**.
Hard-limit headroom: **11,468,382 bytes**. The 75 MB development target is still
exceeded by **13,531,618 bytes**. [Release/hash](BUILD-SIZE.md#golf-playability-and-integrated-greens--8-october-2026).

Total serialized assets decrease **252 bytes**, from **153,629,301 to 153,629,049**.
The combined course and original terrain decrease **372 serialized bytes**;
their independent per-asset ZIP estimates change **1,494,590 → 1,497,012 bytes**.
These estimates do not allocate Unity's shared compressed chunks exactly.
The exact compressed ZIP-entry delta is **+1,680 bytes**; container/signing
overhead is unchanged. IL2CPP grows **806 bytes**, managed metadata **258 bytes**.
Native Unity and Java code remain unchanged.

All sixteen existing terrain sectors/colliders remain. Eight sectors now reference
active derived meshes, replacing their original sector meshes in the scene. Grass
derivatives are desktop-only: **zero derived course grass assets are packed in
the APK**. Android still instantiates the existing shared patch library and applies
the same small green deformation. No new texture, imported model, material,
shader, animation, audio or runtime package was added. The original Blender/FBX
masters, textures, macro-colour patterns and travel components are unchanged.
A GUID-reference scan found no unused generated terrain/grass assets. The active
originals remain necessary for authoring and repeatable generation.

The largest retained estimates remain shared timber **2.247 MB**, character
albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.588 MB**,
and football stadium **1.556 MB**. No unexpected large contributor or avoidable
new dependency was found. The pre-existing 75 MB target shortfall remains.

Evidence: `Builds/GolfPlayabilityQA/before.json`, `release.json`,
`packing-summary.json`, `apk-entry-delta.csv`, `asset-estimates-final.csv`,
`apk-signature.txt`, `unused-derived-candidates.json`, `size-before/`, `size-final/`.

## Simple maps, round controls and resource cleanup — 8 October 2026

Actual signed APK: **89,894,276 → 88,529,938 bytes (−1,364,338)**.
Hard-limit headroom: **11,470,062 bytes**. The 75 MB development target is still
exceeded by **13,529,938 bytes**. [Release/hash](BUILD-SIZE.md#simple-maps-round-controls-and-resource-cleanup--8-october-2026).

Seven animation imports account for the major saving. Conservative keyframe
reduction lowers their editor curve key count **235,550 → 68,811**. Their packed
serialized data drops **2,676,924 bytes**; independent per-asset ZIP estimates
drop **1,528,989 → 221,381 bytes**. Estimates are not exact allocations of the
APK's shared compressed chunks. All 39 character transforms were compared at
120 Hz, including endpoints, with a maximum position difference of **0.714 mm**.
Duration and loop flags match. Original FBXs and masters are unchanged.

Total serialized assets fall **156,298,585 → 153,629,301 bytes (−2,669,284)**.
The shared built-in asset block grows **7,640 serialized bytes**; engine-selected
dependencies remain included after removing two unused forced shader entries.
No mesh, texture, audio or package was added. The largest retained art estimates
remain shared timber **2.247 MB**, character albedo **1.993 MB**, golf structures
**1.981 MB**, character mesh **1.588 MB** and football stadium **1.556 MB**.
They are active assets, not cleanup candidates.

Exact compressed ZIP entries fall **1,363,845 bytes**; signing/container overhead
falls another **493 bytes**. IL2CPP falls **4,802 bytes**, managed metadata
**224 bytes**. Unity native code and Java code are unchanged. The largest
compressed entries are IL2CPP **11,384,848 bytes**, Unity **9,071,480 bytes**,
managed metadata **2,630,161 bytes** and Java **2,353,575 bytes**.

The unused imported club material was already excluded from the old APK; its
archive is not counted as APK savings. Font licences and active runtime Resources
are retained. [Full dependency and regeneration rationale](HUD-RESOURCE-CLEANUP.md).
The development target still needs further deliberate optimization.

Evidence: `Builds/SimpleHudQA/before.json`, `release.json`, `packing-summary.json`,
`apk-entry-delta.csv`, both `asset-estimates-*.csv`, `animation-key-validation.csv`,
`material-textures.tsv`, `scene-resource-dependencies.txt`, `forced-shaders.txt`,
`unused-material-proof.json`, `apk-signature.txt`, `size-before/`, `size-final/`.

## Team controls, pocket maps and Cove buttons — 8 October 2026

Actual signed APK: **89,878,628 → 89,894,276 bytes (+15,648)**. Strict-limit
headroom: **10,105,724 bytes**. The 75 MB development target remains exceeded
by **14,894,276 bytes**. [Release and hash](BUILD-SIZE.md#team-controls-pocket-maps-and-cove-buttons--8-october-2026).

Compressed ZIP entries grow **15,645 bytes**, plus **3 bytes** of additional
container/signing overhead. IL2CPP grows **13,819 bytes**, managed metadata
**1,362 bytes**, and remaining entries **464 bytes net** after equal-sized
renamed entries cancel. Unity native code and Java code remain unchanged.
Largest compressed entries are IL2CPP **11,389,650 bytes**, Unity native code
**9,071,480 bytes**, managed metadata **2,630,385 bytes** and Java code
**2,353,575 bytes**.

Serialized assets grow **488 bytes** to **156,298,585 bytes**: 104 bytes for
football action-slot metadata and 96 bytes each for the shared possession,
button style, minimap and development-review script records. Review execution
code is excluded from release. The maps, symbols and button faces use generated
UI vertices and existing fonts. No imported mesh, texture, animation, shader,
audio or package is added. Inspection found no unexpected large contributor
or avoidable new art dependency. The existing development-target shortfall
remains unresolved; the reserve is still constrained.

Evidence under `Builds/SportsHudQA/`: `before.json`, `release.json`,
`apk-entry-delta.csv`, `packing-summary.json`, `apk-signature.txt`,
`size-before/`, `size-final/`, `source-windows.json`, `source-android.json`,
`source-verification.json`, and `golf-prefab-equivalence.json`.

## Football pace, stamina and pressure — 8 October 2026

Actual signed APK: **89,864,908 → 89,878,628 bytes (+13,720)**. Strict-limit
headroom: **10,121,372 bytes**. The 75 MB development target is exceeded by
**14,878,628 bytes**. [Release and hash](BUILD-SIZE.md#football-pace-stamina-and-pressure--8-october-2026).

Compressed ZIP entries grow **13,723 bytes**, offset by **3 bytes** less
container/signing overhead. IL2CPP grows **11,627 bytes**, managed metadata
**1,822 bytes**, and remaining entries **274 bytes net** after equal-sized
renamed entries cancel. Unity native code is unchanged. The largest compressed
entries are IL2CPP **11,375,831 bytes**, Unity native code **9,071,480 bytes**,
managed metadata **2,629,023 bytes** and Java code **2,353,575 bytes**.

Serialized assets grow **312 bytes** to **156,298,097 bytes**: 96 bytes for
football effort script metadata, 104 for its HUD, 104 for development-review
script metadata and 8 for the serialized network player. Review execution
code is excluded from release builds. There is no new imported mesh, texture,
animation, shader, audio or runtime package. Bars use existing UI graphics;
pressure and faster gait use the existing rig. No unexpected large contributor
or avoidable new art dependency was found. The pre-existing development-target
shortfall remains unresolved and the remaining reserve is constrained.

Evidence under `Builds/FootballPaceQA/`: `before.json`, `release.json`,
`apk-entry-delta.csv`, `packing-summary.json`, `apk-signature.txt`,
`size-before/`, `size-final/`, `source-windows.json`, `source-android.json`,
`source-verification.json` and `golf-prefab-equivalence.json`.

## Simple lagoon fishing — 7 October 2026

Actual signed APK: **89,865,804 → 89,938,934 bytes (+73,130)**. Strict-limit
headroom: **10,061,066 bytes**. The 75 MB development target remains exceeded
by **14,938,934 bytes**. [Release and hash](BUILD-SIZE.md#simple-lagoon-fishing--7-october-2026).

Compressed ZIP entries grow **72,951 bytes**, plus **179 bytes** of container
and signing overhead. IL2CPP grows **44,113 bytes**, Unity native code
**22,010 bytes**, managed metadata **6,805 bytes**; Burst shrinks **2,097
bytes**. Remaining entries grow **2,120 bytes** net. Equal-sized renamed
entries cancel. Largest compressed entries are IL2CPP **11,407,611 bytes**,
Unity native code **9,093,495 bytes**, managed metadata **2,634,006 bytes**
and Android Java **2,353,635 bytes**.

The final hand-grip handoff fix adds **2,396 bytes** to the intermediate
89,936,538-byte APK, with no further serialized-asset growth. Its immediate
before APK/record are retained under `Before/grip-handoff-before.*`.

Serialized assets grow **3,256 bytes** to **156,301,133 bytes**. This comprises
the untextured shared line material (**1,616**), lagoon shader (**876**), lagoon
material properties (**76**), network-player serialization (**24**) and script
metadata (**664 bytes**). Development probe execution is excluded from release.
No imported mesh, texture, animation, audio or runtime package is added. Existing
LOD meshes/materials are shared; swimming and rod poses use code and the current
rig. The six-face 128-pixel reflection is captured once, with a sky fallback
on unsupported devices. No large new dependency or avoidable art growth was found.

The existing 75 MB target shortfall remains unresolved. Authoring masters and
the current mobile art were preserved; this task does not justify spending the
remaining reserve on additional asset downloads. Independent v2 verification
passes. Evidence under `Builds/FishingGameplayQA/`: `Before/`, `After/SizeAudit/`,
`release.json`, `apk-entry-delta.csv`, `serialized-asset-delta.csv`,
`apk-signature.txt`, source manifests and source/delivery verification records.

## Perspective broadcast cameras — 7 October 2026

Actual signed APK: **89,861,552 → 89,864,908 bytes (+3,356)**. Strict-limit
headroom: **10,135,092 bytes**. The 75 MB development target is still exceeded
by **14,864,908 bytes**. [Release and hash](BUILD-SIZE.md#perspective-broadcast-cameras--7-october-2026).

Compressed ZIP entries grow **3,359 bytes**, offset by **3 bytes** less
container/signing overhead. IL2CPP grows **3,222 bytes**, Unity native code
**1,175 bytes**, and managed metadata **454 bytes**. Burst shrinks **1,478
bytes**; remaining entries shrink **14 bytes** net. Equal-sized renamed
entries cancel. No new mesh, material, texture, animation, shader, audio or
runtime package is shipped. Serialized assets are unchanged at
**156,297,785 bytes**, a separate measurement from compressed APK bytes.

Largest compressed entries remain IL2CPP **11,364,204 bytes**, Unity native
code **9,071,480 bytes**, and managed metadata **2,627,201 bytes**. No unexpected
large contributor or avoidable new asset dependency was found. The existing
75 MB development-target shortfall remains unresolved.

Evidence under `Builds/BroadcastCameraQA/`: `before.json`, `release.json`,
`apk-before-entries.csv`, `apk-entry-delta.csv`, `packing-summary.json`,
`apk-signature.txt`, `size-before/`, `size-final/`, `source-windows.csv`,
`source-android.json` and `source-verification.json`.

## Stadium camera framing — 7 October 2026

Actual signed APK: **89,847,792 → 89,861,552 bytes (+13,760)**. Hard-limit
headroom is **10,138,448 bytes**; the development target is exceeded by
**14,861,552 bytes**.
[Delivery and hash](BUILD-SIZE.md#stadium-camera-framing--7-october-2026).

Compressed ZIP entries grow **13,758 bytes**, with **2 bytes** of container
and signing growth. IL2CPP grows **10,190 bytes**, Burst **1,530 bytes**,
managed metadata **1,212 bytes**, Unity native code **325 bytes**, and the
existing basketball pass shader **253 bytes**; the remaining entries grow
**248 bytes** net. Renamed entries with equal lengths cancel.

Serialized assets grow **688 bytes** to **156,297,785 bytes**, a separate
measurement from APK size. The changes comprise existing basketball
shader/material updates and small script metadata records. The development
camera review's script metadata is 104 bytes; its execution code is excluded
from the release APK. This release includes the basketball trajectory and
shot-odds work already present in the workspace before the camera task.
The measured increase cannot be attributed solely to the camera change.

Largest compressed entries remain IL2CPP **11.361 MB**, Unity native code
**9.070 MB**, managed metadata **2.627 MB**, and Android Java **2.354 MB**.
The camera reuses the playing surfaces and introduces no mesh, texture,
animation, audio, shader, material or package. No unexpected large packing
growth or avoidable new art dependency was found. The existing 75 MB target
shortfall remains unresolved; this is still a constrained development reserve.

The basketball trajectory update reuses the existing pass material and replaces
the simple lane with one cached 130-vertex procedural ribbon. It adds no
imported mesh, texture, animation clip, audio or runtime package. Its independent
comparison confirms the same 13,760-byte combined APK increase and v2 signature.
The pass shader grows 253 compressed bytes and its material grows 4 bytes.
Before/after APK entries, source manifests and signature evidence also remain
under `Builds/BasketballTrajectoryQA/`; this is the same signed release, not a
second APK measurement.

Evidence under `Builds/StadiumCameraQA/`: `before.json`, `release.json`,
`apk-entry-delta.csv`, `packing-summary.json`, `apk-signature.txt`,
`size-before/`, `size-final/`, `source-windows.csv`, `source-android.json`,
`source-verification.json` and `inputs-since-previous-apk.json`.

## Fresh clone validation — 7 October 2026

Actual new APK: **89,800,008 bytes**, compared with the previous documented
**89,803,004 bytes (-2,996)**. Hard-limit headroom is **10,199,992 bytes**;
the 75 MB development target is exceeded by **14,800,008 bytes**.
[Build, signature and source limits](BUILD-SIZE.md#fresh-clone-validation--7-october-2026).
No previous APK existed locally, so an exact entry-by-entry delta is unavailable.

The largest compressed entries are IL2CPP **11,313,392 bytes**, Unity native
code **9,071,018 bytes**, managed metadata **2,620,296 bytes** and Java code
**2,353,635 bytes**. Serialized assets total **156,285,329 bytes**, 92 bytes
above the historical report; this is separate from compressed APK length.
No new gameplay code, asset or dependency was introduced by local setup, and
the new measured APK has no overall growth. Resources-only dependencies
include the existing Golf equipment and menu assets; that report alone is not
evidence that those assets are unused.

Current per-asset DEFLATE estimates lead with timber **2.258 MB**, Golf
structures **2.112 MB**, character albedo **2.006 MB**, character mesh
**1.718 MB**, football stadium **1.568 MB**, basketball stadium **1.373 MB**
and idle animation **1.088 MB**. These estimates do not partition shared APK
chunks exactly and are not exact before/after asset deltas. The 75 MB target
remains unmet; the existing development reserve is constrained.

Evidence: `Builds/LocalSetup/apk-before.json`, `apk-release.json`,
`apk-signature.txt`, `apk-badging.txt`, `asset-estimates.csv`,
`generated-input-verification.json`, `source-reconciliation.json` and
`windows-player-files.json`, plus `Builds/SizeAudit/latest/`. The AndroidSubmission
gate and independent v2 signature verification passed. Physical-phone testing
and fresh internet-room testing remain pending.

## Shared basketball passing — 7 October 2026

Actual signed APK: **89,829,116 → 89,847,792 bytes (+18,676)**. Hard-limit
headroom is **10,152,208 bytes**; the development target is exceeded by
**14,847,792 bytes**. [Delivery and hash](BUILD-SIZE.md#shared-basketball-passing--7-october-2026).

Compressed ZIP entries grow **18,331 bytes**, plus **345 bytes** of container
and signing overhead. IL2CPP grows **9,361 bytes**, the new pass shader uses
**7,100 compressed bytes**, its material **232 bytes**, and managed metadata
grows **1,514 bytes**. The remaining entries change by **124 bytes** net;
renamed entries with equal sizes cancel. Serialized assets grow **11,524 bytes**
to **156,297,097 bytes**, a separate measurement from APK size.

Largest compressed entries remain IL2CPP **11.351 MB**, Unity native code
**9.070 MB**, metadata **2.626 MB**, and Android Java code **2.354 MB**. The
lane uses one reused procedural quad and material instance. No texture, mesh
asset, animation clip, audio or package was added. The Windows head-surface
fixture and development review code are absent from the release player.
No unexpected large packing growth or avoidable new asset dependency was found.
The existing 75 MB development target shortfall remains unresolved.

Evidence: `Builds/BasketballPassQA/` contains `before.json`, `release.json`,
`apk-entry-delta.csv`, `packing-summary.json`, `apk-signature.txt`,
`size-before/`, `size-final/` and both source manifests.

## Defense motion refinement — 7 October 2026

Actual signed APK: **89,825,364 → 89,829,116 bytes (+3,752)**. Hard-limit
headroom is **10,170,884 bytes**; the development target is exceeded by
**14,829,116 bytes**. [Delivery and hash](BUILD-SIZE.md#defense-motion-refinement--7-october-2026).

Compressed entries grow **3,753 bytes** and signing/container overhead shrinks
**1 byte**. IL2CPP grows **3,642 bytes**, managed metadata grows **115 bytes**,
and the remaining entries shrink by **4 bytes** net. Renamed entries with the
same lengths cancel. Serialized assets remain **156,285,573 bytes**, unchanged.
The largest individual compressed entries remain IL2CPP **11.341 MB**, Unity
native code **9.070 MB**, metadata **2.624 MB** and Android Java **2.354 MB**.
No animation, texture, mesh, material, audio or runtime package was added.

The head/arm geometry index fixture is generated beside development Windows
players for surface-level animation inspection. It is not a runtime dependency
and is absent from the release APK. Review code is excluded from release
execution. The measured increase is code/metadata; no avoidable new asset
packing growth was found. The existing 75 MB target shortfall remains unresolved.

Evidence: `Builds/BasketballDefensePolishQA/` contains `before.json`,
`release.json`, `apk-entry-delta.csv`, `packing-summary.json`,
`apk-signature.txt`, `size-before/`, `size-final/` and source manifests.

## Basketball defense — 6 October 2026

Actual signed APK: **89,803,004 → 89,825,364 bytes (+22,360)**. Hard-limit
headroom is **10,174,636 bytes**; the 75 MB development target is exceeded by
**14,825,364 bytes**. [Delivery and hash](BUILD-SIZE.md#basketball-defense--6-october-2026).

Compressed ZIP entries grow **22,361 bytes**; container/signing overhead
shrinks **1 byte**. IL2CPP grows **21,345 bytes** and managed metadata grows
**3,610 bytes**. The generated Burst library shrinks **1,530 bytes**, the Unity
library shrinks **1,040 bytes**, and the net remainder shrinks **24 bytes**.
Renamed entries with equal compressed lengths cancel in the comparison.
Serialized assets grow **336 bytes** to **156,285,573 bytes**; this is separate
from the actual compressed APK measurement.

The largest individual compressed entries remain IL2CPP **11.338 MB**, Unity
native code **9.070 MB**, managed metadata **2.624 MB** and Android Java code
**2.354 MB**. Shared art is split across smaller archive entries. The packed
asset comparison shows no new texture, mesh, animation clip, material, audio
or package. Defense reuses the character rig and ball; diagnostic traces and
review fixtures are excluded from release execution. No avoidable new asset
dependency or unexpected large packing increase was found. The existing 75 MB
target shortfall remains unresolved and the development reserve is constrained.

Evidence under `Builds/BasketballDefenseQA/`: `before.json`, `release.json`,
`apk-before-entries.json`, `apk-entry-delta.csv`, `packing-summary.json`,
`apk-signature.txt`, `BeforeSizeAudit/`, `FinalSizeAudit/` and
`generated-input-verification.json`.

## Untimed finishes and shot fallback — 6 October 2026

Actual signed APK: **89,798,556 → 89,803,004 bytes (+4,448)**. Hard-limit
headroom is **10,196,996 bytes**; the 75 MB development target is exceeded by
**14,803,004 bytes**. [Build, delivery and hash](BUILD-SIZE.md#untimed-finishes-and-shot-fallback--6-october-2026).

Compressed ZIP entries grow **4,448 bytes**, with unchanged container/signing
overhead. IL2CPP grows **4,065 bytes**, managed metadata **384 bytes**, and the
net remainder shrinks **1 byte**. Two renamed entries retain their compressed
lengths. Serialized assets remain **156,285,237 bytes**, unchanged; this is
separate from the compressed APK measurement.

The rules, input feedback and retimed motions reuse the shared character rig,
meshes, materials and ball. The layup outcome is rolled once by the host and
the existing snapshot carries the motion. No imported texture, mesh, animation
clip, audio, material or package was added. Development review code is excluded
from Android release execution. The latest packed-asset and ZIP-entry audits
show no unexpected asset growth or unused new dependency to remove.

Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**,
Golf structures **1.981 MB**, character mesh **1.588 MB**, football stadium
**1.556 MB**, basketball stadium **1.348 MB** and idle animation **1.096 MB**.
These per-asset DEFLATE estimates do not partition shared APK chunks exactly.
The 75 MB development target remains unmet and the reserve is constrained.

Evidence under `Builds/BasketballGestureFixQA/`: `before.json`, `release.json`,
`apk-before-entries.json`, `apk-entry-delta.csv`, `asset-estimates.csv`,
`packing-summary.json`, `apk-signature.txt`, `BeforeSizeAudit/`,
`FinalSizeAudit/` and `generated-input-verification.json`.

## Basketball layups and dunks — 5 October 2026

Actual signed APK: **89,780,060 → 89,798,556 bytes (+18,496)**. Hard-limit
headroom is **10,201,444 bytes**; the 75 MB development target is exceeded by
**14,798,556 bytes**. [Build, delivery status and hash](BUILD-SIZE.md#basketball-layups-and-dunks--5-october-2026).

Compressed ZIP entries grow **18,494 bytes**, with **2 bytes** of additional
container/signing overhead. IL2CPP grows **16,024 bytes**, managed metadata
**2,213 bytes**, and the Unity native library **219 bytes**; the net remainder
is **38 bytes**. Renamed entries are paired through the full entry comparison.
Serialized assets grow **216 bytes** to **156,285,237 bytes**; this is separate
from the compressed APK measurement.

The new motions reuse the character skeleton, meshes, materials and existing
ball. Swept body and ball checks run when an action is accepted; held poses and
incremental collision-aware motion run during that action. Finish parameters
join the existing action snapshot, without per-frame finish RPCs. Development
review code is excluded from Android release execution. No imported texture,
mesh, animation clip, audio, material or package was introduced, and no
unexpected large asset growth or unused new dependency was found.

Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**,
Golf structures **1.981 MB**, character mesh **1.588 MB**, football stadium
**1.556 MB**, basketball stadium **1.348 MB** and the existing idle animation
**1.096 MB**. These per-asset DEFLATE estimates do not partition shared APK
chunks exactly. The 75 MB development target remains unmet; the existing
reserve is constrained despite this feature's small increase.

Evidence: `Builds/BasketballFinishQA/before.json`, `release.json`,
`apk-before-entries.json`, `apk-entry-delta.csv`, `asset-estimates.csv`,
`apk-signature.txt`, `BeforeSizeAudit/`, `FinalSizeAudit/` and
`generated-input-verification.json`.

## Basketball charging and energy boundary — 5 October 2026

Actual signed APK: **89,762,748 → 89,780,060 bytes (+17,312)**. Hard-limit
headroom is **10,219,940 bytes**; the 75 MB development target is exceeded by
**14,780,060 bytes**. [Build, delivery status and hash](BUILD-SIZE.md#basketball-charging-and-energy-boundary--5-october-2026).

Compressed ZIP entries grow **17,310 bytes**, with **2 bytes** of additional
container/signing overhead. IL2CPP grows **14,614 bytes**, managed metadata
**1,607 bytes**, and the shared boundary shader **882 bytes**. Renamed entries
with equal compressed lengths cancel in the comparison. No imported texture,
mesh, material, audio, animation clip or dependency was introduced.
Serialized assets grow **1,972 bytes** to **156,285,021 bytes**; this is not the
compressed APK measurement.

Basketball reuses football's material/shader with per-renderer tint and impact
properties. Four static quads use 16 vertices and one material; four bounded
ripples animate in the shader, whose expired pulses take a cheap early exit.
The aiming/cancel poses reuse the existing rig. Charge phase joins the existing
ball snapshot; cosmetic barrier contacts do not add RPCs. No unexpected large
asset contributor or avoidable new dependency was found.

Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**,
Golf structures **1.981 MB**, character mesh **1.588 MB**, football stadium
**1.556 MB** and basketball stadium **1.348 MB**. These DEFLATE estimates and
Unity's serialized asset totals are separate from actual APK length. The
development reserve remains constrained and the 75 MB target is unmet.

Evidence: `Builds/BasketballChargeQA/before.json`, `release.json`,
`apk-entry-delta.csv`, `asset-estimates.csv`, `packing-top.txt`,
`apk-signature.txt`, `BeforeSizeAudit/`, `FinalSizeAudit/` and
`generated-input-verification.json`.

## Refined basketball steal animation — 5 October 2026

Actual signed APK: **89,758,540 → 89,762,748 bytes (+4,208)**. Hard-limit
headroom is **10,237,252 bytes**; the 75 MB development target is exceeded by
**14,762,748 bytes**. [Build and hash](BUILD-SIZE.md#refined-basketball-steal-animation--5-october-2026).

Compressed ZIP entries grow **4,210 bytes**, while container/signing overhead
shrinks **2 bytes**. IL2CPP grows **3,906 bytes**, managed metadata **244 bytes**,
and the net remainder **60 bytes**. Two renamed entries keep their compressed
lengths. Serialized assets are **156,283,049 bytes**, up **120 bytes**; this is
not the APK measurement. No art or runtime dependency was added. The animation
uses the existing rig and character LODs; development review code is excluded
from the Android release except for small Unity script metadata.

Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**,
Golf structures **1.981 MB**, character mesh **1.588 MB**, football stadium
**1.556 MB**, and basketball stadium **1.348 MB**. There is no unexpected asset
growth or new dependency to remove from this refinement. Per-asset DEFLATE
estimates do not partition shared APK chunks exactly. The development target
remains unmet and reserve is constrained.

Evidence: `Builds/BasketballStealAnimationQA/release.json`, `apk-entry-delta.csv`,
`asset-estimates.csv`, `apk-signature.txt`, `BeforeSizeAudit/`, `FinalSizeAudit/`
and `generated-input-verification.json`.

## Basketball steals and loose-ball contests — 5 October 2026

Actual signed APK: **89,744,112 → 89,758,540 bytes (+14,428)**. Hard-limit
headroom: **10,241,460 bytes**; development-target overrun: **14,758,540 bytes**.
[Build and hash](BUILD-SIZE.md#basketball-steals-and-loose-ball-contests--5-october-2026).

Actual compressed ZIP entries grow **14,425 bytes**, with **3 bytes** of
additional container/signing overhead. IL2CPP grows **13,466 bytes**, managed
metadata **891 bytes**, and the net remainder is **68 bytes**. Two renamed data
entries preserve their compressed lengths. Serialized assets measure
**156,282,929 bytes**, only **324** more than the preceding release; this is a
different measurement from APK length. No new imported art, animation, audio or
runtime package ships. The two procedural actions reuse the existing character
rig, and the button/HUD reuse existing UI resources.

Leading estimated art contributors remain timber **2.247 MB**, character albedo
**1.993 MB**, Golf structures **1.981 MB**, character mesh **1.588 MB**, football
stadium **1.556 MB**, and basketball stadium **1.348 MB**. There is no unexpected
asset or dependency growth to remove in this task. These per-asset DEFLATE
estimates are not exact allocations of shared APK chunks. The 75 MB working
target remains unmet and the remaining hard-limit reserve is still constrained.

Evidence: `Builds/BasketballStealQA/release.json`, `apk-entry-delta.csv`,
`asset-estimates.csv`, `packing-top.txt`, `apk-signature.txt`, `BeforeSizeAudit/`
and `FinalSizeAudit/`. Runtime source is unchanged through Android packaging;
canonical inspection confirms identical properties/reference topology in the
regenerated Golf ball prefab. [Functional checks and limits](VERIFICATION.md#basketball-steals-and-loose-ball-contests--5-october-2026).

## Storybook Cove menu and teammate integration — 5 October 2026

Signed APK: **86,791,760 → 89,744,112 bytes (+2,952,352)**. Headroom below the
strict limit: **10,255,888 bytes**; development-target overrun: **14,744,112 bytes**.
The baseline predates the teammate's Golf integration; the full growth is not a
menu-only cost. The independently measured concurrent basketball/Golf build was
89,260,934 bytes, making this combined menu package **483,178 bytes larger**,
with source/build differences documented in the respective release records.
[Exact build and hash](BUILD-SIZE.md#storybook-cove-menu-and-teammate-integration--5-october-2026).

| New menu asset | Serialized bytes | Estimated ZIP bytes |
| --- | ---: | ---: |
| Shared four-island atlas | 467,980 | 307,208 |
| Official logo delivery texture | 123,784 | 74,225 |
| Cove Display font | 32,442 | 17,724 |
| Cove Text font | 32,474 | 17,645 |
| OFL license | 4,412 | 1,969 |
| Total | 661,092 | 418,771 |

The atlas imports to Android at 1024 pixels and the logo at 512, both ASTC 6×6
without mipmaps. Four sprites share one atlas. Font subsets retain 64,108 source
bytes across two weights; full editable originals and design boards stay outside
the Unity player. Panels, clouds, water and icons are runtime UI meshes; tap sound
is synthesized. No UI/animation package, new scene, full-screen bitmap per page,
imported audio or duplicate island texture is added. Venue previews reuse one
768×432 render texture and release it when closed.

Actual compressed ZIP entries grow **2,948,227 bytes**, with **4,125 bytes** of
additional signing/container overhead. Across the entire merged game, IL2CPP
grows **154,698 bytes**. The largest new contributors from the pre-pull baseline
are the Golf cart mesh and maps, followed by the menu atlas. Leading existing
art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, Golf
structures **1.981 MB**, character mesh **1.588 MB** and football stadium
**1.556 MB**. No unexpected menu dependency appears. Serialized assets total
**156,282,605 bytes**, a different measurement from compressed APK length.

Fresh populated packing offsets support these per-asset estimates; shared ZIP
chunks mean they are not exact asset allocations. Evidence:
`Builds/StorybookMenuQA/release.json`, `apk-entry-delta.csv`, `packing-top.txt`,
`apk-signature.txt`, `FinalSizeAudit/` and source hash/canonical comparisons.
The 75 MB working target is still unmet; current headroom is not spare capacity
to spend without further optimization. No lossy change to existing world art
was made for this menu. [Functional limits](VERIFICATION.md#storybook-cove-menu-and-teammate-integration--5-october-2026).

## Basketball pace and player boundaries — 5 October 2026

Actual signed APK: **86,791,760 → 89,260,934 bytes (+2,469,174)**. Hard-limit headroom: **10,739,066 bytes**; development-target overrun: **14,260,934 bytes**. The strict gate and APK v2 signature pass. [Build scope and exact hash](BUILD-SIZE.md#basketball-pace-and-player-boundaries--5-october-2026).

The measured starting APK predates the incoming Golf/football integration. The final build includes that integration plus basketball work, so the full increase must not be attributed to the timing/boundary change. No basketball mesh, texture, audio, animation pack or dependency was added. The player fence uses movement constraints without a rendered wall or physical ball collider. The final unreachable-rebound recovery fix increased the preceding diagnostic APK by **1,744 bytes**.

Leading estimated asset increases against the starting APK are Golf cart mesh **909,743**, cart normal **621,270**, cart base color **498,110**, club normal **131,740**, club base color **129,848**, club mesh **123,767**, and course sector 07 **120,273 bytes**. The old ball normal and Golf terrain estimates decreased by **596,010** and **400,567 bytes** respectively. Actual ZIP entry growth includes IL2CPP **102,662** and metadata **15,857 bytes** across the integrated game. No unexpected new basketball art or package contributor was found.

Largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, Golf structures **1.981 MB**, character mesh **1.588 MB** and football stadium **1.556 MB**. Serialized packed assets total **155,621,029 bytes**, up from **151,510,681**. These asset estimates and serialized sizes are distinct from the measured compressed APK length. The development reserve remains insufficient; being below the hard limit does not satisfy the 75 MB working target.

Evidence is retained under `Builds/BasketballPaceQA/`: `before.json`, `release.json`, `apk-entry-delta.csv`, `asset-estimate-delta.csv`, `asset-estimates.csv`, `apk-signature.txt`, `BeforeSizeAudit/` and `FinalSizeAudit/`. Matched isolated Android/Windows players are in `Release/`; concurrent menu work is excluded. [Functional checks and remaining limits](VERIFICATION.md#basketball-pace-and-player-boundaries--5-october-2026).

## Basketball physics and net response — 4 October 2026

Actual signed AndroidSubmission APK: **86,774,768 → 86,791,760 bytes (+16,992)**. Hard-limit headroom: **13,208,240 bytes**; development-target overrun: **11,791,760 bytes**. [Build provenance and SHA-256](BUILD-SIZE.md#basketball-physics-and-net-response--4-october-2026).

Ball enlargement changes prefab scale, collider radius, inertia and hand targets, retaining the original FBX, both LODs, rubber maps and editable masters. Each responsive net uses a small runtime mesh with the existing net material, and sleeps its mesh updates after the damped response. The hold/release meter, score feedback, host rules and replicated net events add no imported art, audio, middleware or animation pack.

Actual ZIP deltas are IL2CPP **+14,294 bytes**, metadata **+2,390**, and the remaining **+308 bytes** across other entries/container overhead. The largest unchanged art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. Per-asset estimates come from fresh populated packing offsets; they are not exact allocations of shared compressed chunks. Serialized asset bytes increased by **404**, to **151,510,681**, separately from APK bytes.

Evidence: `Builds/BasketballPhysicsQA/apk-before.json`, `apk-after.json`, `apk-entry-delta.csv`, `asset-estimates.csv`, `apk-signature.txt` and `FinalSizeAudit/`. **396 Windows assertions** cover physics/scoring, hold/release input, net response, player motion, host/guest state and shared jumping. No phone was connected. [Verification and remaining limits](VERIFICATION.md#basketball-physics-and-net-response--4-october-2026).

## GitHub Golf and football integration — 5 October 2026

Fresh combined primary AndroidSubmission at **00:07:25 Malaysia time**:
**89,024,294 → 89,230,290 bytes (+205,996)**. Strict headroom is
**10,769,710 bytes**; the development target remains exceeded by
**14,230,290 bytes**. Strict gate and independent APK v2 signature pass.
Exact retained APK/audit: `Builds/GitSyncQA/Release/`; detailed comparison:
`Builds/GitSyncQA/release-audit.json`. See
[release scope and archives](BUILD-SIZE.md#github-golf-and-football-integration--5-october-2026).

Serialized assets change **155,324,117 → 155,620,521 bytes (+296,404)**.
Actual compressed ZIP entries grow **204,965 bytes**, with **1,031 bytes** of
additional signing/container overhead. Expected new upstream contributors
include lagoon surface **43,086**, shared fish mesh **40,321**, lagoon bed
**19,892**, and football net shader **9,291 estimated ZIP bytes**. The shared
28-record football pose library contributes only **1,854 estimated ZIP bytes**.
No new package or unexpected large contributor appears. Leading estimates
remain timber **2.247 MB**, character albedo **1.993 MB**, Golf structures
**1.981 MB**, character mesh **1.588 MB** and football stadium **1.556 MB**.
These estimates and serialized totals are separate from actual APK length.

## Golf swing speed 18 m/s — 4 October 2026

Fresh primary AndroidSubmission at **23:27:16 Malaysia time** measures
**89,024,798 → 89,024,294 bytes (−504)**. Strict headroom is **10,975,706 bytes**;
the development target remains exceeded by **14,024,294 bytes**. The strict
gate and independent APK v2 signature pass. Exact APK and all audit files are
retained in `Builds/GolfEighteenSpeedQA/Release/`. Release provenance, source
scope and archives are in
[BUILD-SIZE](BUILD-SIZE.md#golf-swing-speed-18-ms--4-october-2026).

Serialized assets measure **155,324,117 bytes**, **8 bytes** above the preceding
five-metre configuration. `GolfBallPhysics.asset` grows from **88 to 96 serialized
bytes**, with its estimated ZIP contribution changing from **68 to 76 bytes**.
No new art, audio, font, animation or package is added by the launch-speed fix.
Leading compressed estimates remain timber **2.247 MB**, character albedo
**1.993 MB**, Golf structures **1.981 MB**, character mesh **1.588 MB** and
football stadium **1.556 MB**. No unexpected large contributor appears.
Estimated contributions and serialized totals are separate from exact APK size.
Evidence: `Builds/GolfEighteenSpeedQA/packing-top.txt`, `release-audit.json` and
`apk-signature.txt`; current primary audit: `Builds/SizeAudit/latest/`.

## Golf meadow restored — 4 October 2026

Fresh shared primary AndroidSubmission: **89,021,758 → 89,024,882 bytes
(+3,124)**, including concurrent shot-range/cart changes. Strict headroom
**10,975,118 bytes**; development target exceeded by **14,024,882**. Gate and
independent v2 signature pass. Snapshot limits and archives:
[BUILD-SIZE](BUILD-SIZE.md#golf-meadow-restored--4-october-2026).

Serialized assets **155,324,109 bytes**, only **4 bytes** above the prior Swing
release. Android reuses the existing grass library; restoring 2,289 blades adds
no shipped mesh or texture. The 21 obsolete derivatives are proven unreferenced.
Leading compressed estimates remain timber **2.247 MB**, character albedo
**1.993 MB**, Golf structures **1.981 MB**, character mesh **1.588 MB** and
football stadium **1.556 MB**. No unexpected large contributor appears.
These estimates and serialized totals differ from exact APK length.
Fresh detailed audit/APK: `Builds/GolfGrassQA/Release/`; source scope, signature,
original-map hashes and package comparison: `Builds/GolfGrassQA/`.

## Golf cart forward-facing feet — 4 October 2026

Fresh task APK: **89,021,758 → 89,025,074 bytes (+3,316)**, built at
**22:40:48 Malaysia time**. Strict headroom **10,974,926 bytes**;
development target exceeded by **14,025,074 bytes**. AndroidSubmission,
APK v2 signature and independent packing audit pass. Serialized assets grow
only **4 bytes** from the concurrent five-metre Golf configuration; no art is
added by the feet correction. ZIP entries grow 3,314 bytes and signing/container
overhead grows 2 bytes. The largest estimates remain timber 2.247 MB, character
albedo 1.993 MB, Golf structures 1.981 MB and character mesh 1.588 MB.

Exact task packages/audit are retained in `Builds/GolfCartFeetQA/Release/`.
The newer shared APK (**89,024,882 bytes**, 22:44:23) is preserved;
its v2 signature and generated native toe-tip branch were checked. Current
shared headroom is **10,975,118 bytes**; its development-target
excess is **14,024,882 bytes**. These snapshots differ in unrelated
concurrent grass work. The initial 313-file Windows feet snapshot passed
**439 checks**. The newer complete shared Windows player (22:43:13) includes
the same feet correction and passed another **221 cart assertions**. See
[verification and scope](VERIFICATION.md#golf-cart-forward-facing-feet--4-october-2026).

## Reliable Golf Swing proximity — 4 October 2026

Fresh measured APK: **89,021,758 bytes**, **+20,774** from opening
**89,000,984**, including intervening tee/cart deliveries, and **−300** from
the immediate **89,022,058-byte** predecessor. Strict headroom is
**10,978,242 bytes**; the development target is exceeded by **14,021,758**.
AndroidSubmission and independent APK v2 signature pass.

Serialized assets remain **155,324,105 bytes**, identical to the immediate
predecessor. Every per-asset contribution matches; no extra mesh, texture,
font, audio, animation or package is shipped by this fix. Actual compressed
ZIP entries decrease **299 bytes** and container/signing overhead decreases
**1 byte**. The leading per-asset estimates remain timber **2.247 MB**,
character albedo **1.993 MB**, Golf structures **1.981 MB**, character mesh
**1.588 MB** and football stadium **1.556 MB**. No unexpected large contributor
appears. These estimates and serialized bytes are separate from APK length.

Current fresh audit: `Builds/SizeAudit/latest/`; exact retained audit and APK:
`Builds/GolfSwingQA/Release/`. Exact comparisons, signature and source scope:
`Builds/GolfSwingQA/release-audit.json`, `apk-signature.txt` and
`delivery-source-inputs.json`. See
[release and archives](BUILD-SIZE.md#reliable-golf-swing-proximity--4-october-2026)
and [feature checks](VERIFICATION.md#reliable-golf-swing-proximity--4-october-2026).

## Golf wooden opening tee — 4 October 2026

Measured APK: **89,000,984 → 89,022,058 bytes (+21,074)**. Strict-limit
headroom is **10,977,942 bytes**; the development target remains exceeded by
**14,022,058 bytes**. The reused concurrent AndroidSubmission package contains
the exact tee and gameplay sources. Its gate, fresh detailed packing analysis
and independent v2 signature pass. Retained APK/audit:
`Builds/GolfWoodTeeQA/Release/`. Primary's earlier OOM attempt produced no APK;
source comparison and reuse provenance are in
[BUILD-SIZE](BUILD-SIZE.md#golf-wooden-opening-tee--4-october-2026).

| Shared tee asset | Serialized bytes | Estimated ZIP bytes |
| --- | ---: | ---: |
| 832-triangle lathe mesh, also used for static cup collision | 21,304 | 10,933 |
| 64×128 wood grain, Android ASTC 6×6 with mipmaps | 5,516 | 4,529 |
| Instanced Lit material | 1,660 | 632 |
| Opening support prefab | 443 | 172 |
| Total | 28,923 | 16,266 |

Every player's support shares these assets. The user reference stays outside
Unity in `ArtSource/Golf/Tee/`; it is absent from the shipped asset graph. No
replacement ball, package, audio or large texture is added. The existing ball
retains its 15,998/720-triangle meshes and material. Its ground query also
recognizes the support's Ignore Raycast layer, while ordinary character/cart
terrain queries retain their original layer.

Serialized assets total **155,324,105 bytes (+29,295)**. Exact ZIP entries
grow **20,214 bytes** and container/signing overhead grows **860 bytes**;
these measurements differ from per-asset compression estimates. Concurrent
validated settling, swing-address and seated-pose code is retained. Leading
contributors remain timber **2.247 MB**, character albedo **1.993 MB**, Golf
structures **1.981 MB**, character mesh **1.588 MB**, and football stadium
**1.556 MB**. No unexpected large dependency appeared. The 75 MB target remains
outstanding; this small prop does not change that wider optimization requirement.

## Golf cart straight-knee driver — 4 October 2026

Fresh APK: **89,022,058 bytes**; **+19,204** from the recorded opening
**89,002,854**, and **+21,188** from the intervening **89,000,870-byte** contact
release. Strict-limit headroom: **10,977,942**; development-target excess:
**14,022,058**. AndroidSubmission and independent v2 signing pass.

The straight-knee pose reuses the existing character, cart and animations.
Serialized assets are **155,324,105 bytes**, **+29,099** against the contact
release. Retained concurrent tee entries account for **28,923**: mesh **21,304**,
grain **5,516**, material **1,660**, prefab **443**. Their compressed estimates
are about **10,933 / 4,529 / 632 / 172 bytes** respectively. Tee/Swing probe
MonoScript metadata adds the remaining **176 serialized bytes**; opt-in probe
code is compiled out of release. No large new dependency appears.

Actual ZIP compressed entries grow **20,501 bytes**; container/signing
overhead accounts for **687 bytes** of the immediate APK difference.
Fresh per-asset estimates retain timber **2.247 MB**, character albedo
**1.993 MB**, Golf structures **1.981 MB**, character mesh **1.588 MB**, and
football stadium **1.556 MB** as the leading entries. These estimates differ
from exact ZIP and APK lengths. Ignored performance-test resource records
are omitted from the isolated build.

Exact APK and all **11 fresh audit files**: `Builds/GolfCartSeatQA/Release/`;
shared audit: `Builds/SizeAudit/latest/`. Packing comparison, signatures,
source scope and complete-package hashes are under `Builds/GolfCartSeatQA/`.
See [delivery records](BUILD-SIZE.md#golf-cart-straight-knee-driver--4-october-2026)
and [455 checks](VERIFICATION.md#golf-cart-straight-knee-driver--4-october-2026).
Physical-phone rendering/FPS and WAN remain unverified.

## Golf slope settling and resting — 4 October 2026

Final APK **89,000,870 bytes**: **138 bytes below** opening **89,001,008**,
**114 below** the intervening Aim delivery **89,000,984**, and **1,984 below**
the interim settling candidate. Hard-limit headroom is **10,999,130**, with
**14,000,870** above the development target. AndroidSubmission, independent
v2 signature and fresh per-asset audit pass.

Serialized assets total **155,295,006 bytes (+196)**: **84 bytes** of persistent
physics settings and **112 bytes** of MonoScript metadata. No delivered art,
texture, mesh, animation, font or package is added. Actual compressed ZIP entries
fall **311 bytes**; container/signing overhead increases **173 bytes**, giving
the exact **138-byte APK reduction**. Per-asset estimates stay led by timber
**2.247 MB**, character albedo **1.993 MB**, Golf structures **1.981 MB**,
character mesh **1.588 MB**, football stadium **1.556 MB**. No unexpected large
dependency appears. Serialized bytes and per-asset estimates are not APK length.

Fresh retained audit: `Builds/GolfSettlingQA/Release/SizeAudit/`; shared audit:
`Builds/SizeAudit/latest/`. Concurrent in-progress tee, swing-review and
cart/locomotion changes are outside this original-map snapshot. See
[hash, input scope and delivery](BUILD-SIZE.md#golf-slope-settling-and-resting--4-october-2026)
and [physics and gameplay checks](VERIFICATION.md#golf-slope-settling-and-resting--4-october-2026).

## Golf 3x ball and optional Aim — 4 October 2026

Fresh APK: **89,000,984 bytes**, **+16,796** from the recorded **88,984,188-byte**
opening and **24 bytes below** the intervening grip release. Hard-limit
headroom is **10,999,016 bytes**; the development target remains exceeded by
**14,000,984 bytes**. AndroidSubmission and independent v2 signing pass.

The ball reuses its existing **15,998 / 720-triangle** LODs and material.
Prefab visual scale and physical radius grow together; no model/texture
payload is added. The shared Chinese control font grows **5,320 → 6,256 bytes**
for Aim/Cancel, with **936 serialized bytes / 728 estimated ZIP bytes** added.
The retained concurrent grip adds **15,168 serialized bytes / 13,560 estimated
ZIP bytes** to the existing character mesh, plus **123 serialized bytes** to
each character prefab. Serialized assets total **155,294,810 (+16,350)**.
Exact compressed ZIP entries grow **16,798 bytes**; container/signing overhead
changes by **−2 bytes**. These are separate from per-asset estimates.

Fresh estimates use this APK's own detailed packing offsets. Leading entries
remain timber **2.247 MB**, character albedo **1.993 MB**, Golf structures
**1.981 MB**, character mesh **1.588 MB**, and football stadium **1.556 MB**.
No unexpected large dependency appears. Exact retained APK/audit:
`Builds/GolfBallAimQA/Release/`; raw signatures, source fingerprints and
comparisons: `Builds/GolfBallAimQA/`. See
[release/delivery scope](BUILD-SIZE.md#golf-3x-ball-and-optional-aim--4-october-2026)
and [native checks](VERIFICATION.md#golf-3x-ball-and-optional-aim--4-october-2026).
Phone quality/FPS and WAN remain unverified.

## Golf closed grip and raised carry angle — 4 October 2026

Fresh APK **89,001,008 bytes**, **+16,820** from opening; strict-limit headroom
**10,998,992**, development-target excess **14,001,008**. AndroidSubmission,
independent v2 signature and fresh per-asset packing analysis pass.

Serialized assets are **155,294,810 bytes (+16,350)**, separate from compressed
APK size. The derived closed-hand shape and grip socket add **15,168 serialized
bytes** to the character mesh and **123 bytes** to each athlete prefab. The
concurrent aim-button snapshot adds **936 bytes** to the existing label font.
There is no added texture, material, renderer or package. Both original character
LODs remain; only **335 / 55** positions receive grip deltas. The original FBX
SHA-256 is unchanged, with no new binary dependency paths.

Leading compressed estimates: timber **2.247 MB**, character albedo **1.993 MB**,
Golf structures **1.981 MB**, character mesh **1.588 MB** (about **13,560 bytes**
additional), football stadium **1.556 MB**. No unexpected large dependency
appears. Actual compressed ZIP entries grow **16,822 bytes**, with **−2 bytes**
of container/signing overhead. Per-asset estimates are not exact ZIP attribution.

Retained fresh audit: `Builds/GolfGripSlopeQA/Release/SizeAudit/`; shared audit:
`Builds/SizeAudit/latest/`. See
[hash, snapshot scope and archives](BUILD-SIZE.md#golf-closed-grip-and-raised-carry-angle--4-october-2026)
and [172-check gameplay verification](VERIFICATION.md#golf-closed-grip-and-raised-carry-angle--4-october-2026).

## Golf ball visibility and free physics — 4 October 2026

Fresh APK: **88,984,188 bytes**; **+2,364** from the recorded task opening and
**+1,792** from the intervening palm-carry release. It leaves **11,015,812 bytes**
below the strict ceiling and exceeds the development target by **13,984,188**.
AndroidSubmission and independent v2 signing pass. No art, shader, font, audio
or package is added. Rolling physics, player/cart isolation, camera framing
and UI marker spacing reuse existing systems and assets.

Serialized assets total **155,278,460 bytes (+104)**: the new opt-in probe
contributes only MonoScript metadata to release packing, while its fixture code
is compiled out. Asset estimates use this APK's own fresh packing offsets.
Leading estimates remain timber **2.247 MB**, character albedo **1.993 MB**,
Golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium
**1.556 MB**. No unexpected large dependency appears. Actual compressed ZIP
entries grow **1,788 bytes** against the preceding carry release, plus **4
bytes** of container/signing overhead.

Exact retained APK/audit: `Builds/GolfBallPhysicsQA/Release/`; current shared
audit: `Builds/SizeAudit/latest/`. Hash, input scope, complete Windows delivery
and archives are in
[BUILD-SIZE](BUILD-SIZE.md#golf-ball-visibility-and-free-physics--4-october-2026).
See [measured gameplay checks](VERIFICATION.md#golf-ball-visibility-and-free-physics--4-october-2026).
Phone quality/FPS and WAN remain unverified.

## Golf palm animation — 4 October 2026

The freshly built and independently signed APK measures **88,982,396 bytes**,
**572 bytes above** the opening release, with **11,017,604 bytes** below the
strict ceiling and **13,982,396 bytes** above the development target. The right
wrist correction adds no mesh, texture, animation asset, shader, font or package.
Serialized assets remain **155,278,356 bytes**. Fresh detailed packing offsets
were analyzed against this APK itself. Leading estimates remain timber
**2.247 MB**, character albedo **1.993 MB**, Golf structures **1.981 MB**,
character mesh **1.574 MB** and football stadium **1.556 MB**; no unexpected
large dependency appeared.

Exact APK/audit: `Builds/GolfPalmCarryQA/Release/`; build logs, signature and
input comparisons: `Builds/GolfPalmCarryQA/`. The shared Android APK,
`Builds/SizeAudit/latest/` and complete `Builds/WindowsFinal/` are updated to this
validated release. See
[hash, snapshot scope and delivery status](BUILD-SIZE.md#golf-palm-animation--4-october-2026)
and [142-check gameplay validation](VERIFICATION.md#golf-palm-animation--4-october-2026).
Physical-phone quality/FPS and WAN remain unverified.

## Rounded Golf ball — 4 October 2026

Fresh APK: **89,418,634 → 88,981,824 bytes (−436,810)**, leaving **11,018,176
bytes** below the strict ceiling and **13,981,824 bytes** above the development
target. AndroidSubmission, clean packing and independent v2 signing pass.
See [exact release, input scope and recovery](BUILD-SIZE.md#rounded-golf-ball--4-october-2026).

The original 87,048-triangle rounded-dimple model is retained as an editable
master and byte-identical FBX outside Unity. Delivery uses **15,998 / 720
triangles**, low mesh compression, imported normals, no tangents and one shared
ivory material. The distant mesh preserves a round silhouette once the small
dimples are below pixel scale. No new texture, shader, font or package is
shipped. Both LODs fit the existing 43 mm sphere; no duplicate physics body or
collider is added. The old ball's normal map remains available as historical
source provenance but is no longer referenced by the current material or
included in this APK.

Serialized assets measure **155,278,356 bytes (−583,832)**. The fresh mesh
estimate is **198,847 bytes (+160,483)**, the former normal map's **596,010
estimated bytes / 699,212 serialized bytes** are absent, and material metadata
decreases **13 estimated / 16 serialized bytes**. Exact compressed ZIP entries
shrink **436,645 bytes**; container/signing overhead shrinks **165 bytes**.
All other art sizes are stable, apart from negligible compression differences.
Leading estimates remain timber 2.247 MB, character albedo 1.993 MB, Golf
structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB.
There is no unexpected large shipped dependency. These estimates were computed
from this APK's own detailed packing offsets, not the earlier cached report.

Current audit: `Builds/SizeAudit/latest/`; exact retained audit and APK:
`Builds/GolfBallRoundedQA/Release/`. Comparisons, source manifests and signing:
`Builds/GolfBallRoundedQA/`. Format-aware primary/alternate-checkout audits find
no active image/library dependency or machine path; inactive exporter SceneInfo
and saved Blender UI provenance are documented exceptions. See
[regeneration](../ArtSource/Golf/Ball/README.md) and
[functional/visual checks](VERIFICATION.md#rounded-golf-ball--4-october-2026).
Phone touch/rendering/FPS and WAN remain unverified.

## Golf club natural carry — 4 October 2026

The retained carry-task **88,981,788-byte** APK shrinks **436,846 bytes** from the opening
89,418,634-byte release. Hard-limit headroom is **11,018,212 bytes**; the 75 MB
development target remains exceeded by **13,981,788 bytes**. AndroidSubmission
and independent APK v2 signing pass. Hash, exact release paths and input scope
are in [BUILD-SIZE](BUILD-SIZE.md#golf-club-natural-carry--4-october-2026).

Natural hand carry adds no mesh, map, animation, shader, font or package. The
existing 3,600 / 1,400 / 500-triangle club and 512 / 512 / 256-pixel maps remain
unchanged. This combined snapshot preserves the concurrent rounded-ball update:
ball geometry grows **115,396 serialized / 160,483 estimated bytes**, while its
old normal map removes **699,212 serialized / 596,010 estimated bytes**. Total
serialized assets shrink **583,832 bytes** to **155,278,356**. Actual compressed
ZIP entries shrink **436,680 bytes**, plus **166 bytes** of container/signing
overhead. Fresh offsets and estimates were calculated from this APK itself.
Timber (2.247 MB), character albedo (1.993 MB), Golf structures (1.981 MB),
character mesh (1.574 MB) and football stadium (1.556 MB) remain the largest
estimates. No unexpected large dependency appeared.

Exact retained audit: `Builds/GolfClubCarryQA/Release/latest/`.
Before/after input manifests, signatures,
package deltas and the canonical Ball prefab graph comparison are in
`Builds/GolfClubCarryQA/`. Primary source stays unchanged across packaging;
Android URP registrations and local prefab IDs are generated, and the isolated
editor uses clean packing. See [whole-gait, native-view and loopback verification](VERIFICATION.md#golf-club-natural-carry--4-october-2026).
The tested complete Windows player is retained in the same release folder;
physical-phone quality/FPS and WAN remain unverified.

The newer rounded-ball delivery includes this carry implementation and supersedes
the shared APK/player. The current APK is **88,981,824 bytes**, **436,810 bytes
smaller** than the task's opening release, with **11,018,176 bytes** of strict-limit
headroom. The development target remains exceeded by **13,981,824 bytes**.
Current `Builds/SizeAudit/latest/` reports belong to that newer APK; its exact
measurements and input scope are in the rounded-ball entry above. The complete
shared Windows player also passes the final 140-check carry/Golf suite.

## Midnight Iron handheld club — 4 October 2026

The final APK measures **89,418,634 bytes**, **+630,844** from the opening **88,787,790-byte** artifact and **+455,624** from the separately validated reference-tyre release. Hard-limit headroom is **10,581,366 bytes**; the development target is still exceeded by **14,418,634 bytes**. AndroidSubmission and independent APK v2 signature verification pass. SHA-256: **49BC113AFE2C569D42CCB63154158294E6748C81AF309F0638279A71D357E679**. Exact current/retained APKs and complete matching Windows delivery are recorded in [BUILD-SIZE](BUILD-SIZE.md#midnight-iron-handheld-club--4-october-2026).

The 7,702-triangle editable master and all original 2048-pixel maps remain under ArtSource. Only **3,600 / 1,400 / 500** delivery triangles, one URP material, **512-pixel colour/normal** and a **256-pixel metallic/smoothness mask** enter Unity. Mobile formats are ASTC 6×6 / 4×4 / 8×8 respectively, with mipmaps. All maps explicitly import as Texture2D and are referenced by the material; the missing-map cubemap trial was corrected and rebuilt. Three LODs share one material; two-hand motion uses the existing avatar rig, with no shipped swing animation, additional shader, font or package. The club has no rigidbody or collider.

Fresh packing-offset analysis gives club estimates of **123,767 bytes mesh**, **129,848 colour**, **131,740 normal**, **18,413 mask** and **1,991 prefab/material/script metadata**: **405,759 bytes total**. Serialized assets measure **155,862,188 bytes**, **+919,293** from the opening snapshot; these are not compressed APK bytes. Exact ZIP-entry growth is **629,813 bytes**, with **1,031 bytes** of container/signing overhead completing the APK delta. Against the tyre release, existing asset sizes remain stable apart from eight player-prefab serialized bytes and negligible estimate changes. The new club and updated IL2CPP/metadata packing account for the incremental snapshot; no unexpected large dependency appeared. Leading estimates remain timber 2.247 MB, character albedo 1.993 MB, Golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB.

Audit files and exact current packing offsets: `Builds/SizeAudit/latest/`; retained release: `Builds/GolfClubQA/Release/latest/`. Comparisons, signing, unchanged 2,947-input capture and Windows promotion hashes: `Builds/GolfClubQA/`. Both primary and alternate-checkout format-aware audits pass: all seven Blender image dependencies and FBX texture references are relative and exist, with no external library or active machine path. Original embedded source/provenance is retained for editing; the delivery FBX has no inactive machine-path provenance fields. See [source and regeneration](../ArtSource/Golf/Club/README.md) and [functional/visual verification](VERIFICATION.md#midnight-iron-handheld-club--4-october-2026). Phone quality/FPS and WAN remain unverified.

## Golf cart reference tyres — 4 October 2026

The fresh tyre-verification APK measures **88,787,790 → 88,963,010 bytes (+175,220)**, with **11,036,990 bytes** below the strict ceiling and **13,963,010 bytes** above the development target. AndroidSubmission, clean packing and independent v2 signing pass. The exact retained APK and complete audit are in `Builds/GolfCartTreadQA/Release/`; the shared root delivery is preserved because concurrent Golf-club additions postdate this captured source. See [hash, scope and recovery](BUILD-SIZE.md#golf-cart-reference-tyres--4-october-2026).

Serialized assets are **155,187,375 bytes (+244,480)**. The cart FBX estimate rises **172,773 bytes to 909,743** for the requested rounded thick tyre and geometric chevron channels. All other per-asset estimates and cart texture hashes are unchanged, retaining one material and five renderers per LOD. Mid/far axial simplification removes **2,560 triangles** versus the full new profile. Exact compressed entries grow **175,220 bytes**, with no net container-overhead change. Leading contributors remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB; no unexpected shipped dependency or texture growth appeared. Estimates were recalculated from this APK's own packing offsets. Phone rendering/FPS and the 75 MB development goal remain unqualified.

## Golf cart rim fit and circularity — 4 October 2026

Fresh combined snapshot: **88,713,562 → 88,787,790 bytes (+74,228)**, with **11,212,210 bytes** strict-limit headroom and **13,787,790 bytes** above the development target. AndroidSubmission, complete packing and independent APK v2 signing pass. See [release hash, captured scope and recovery](BUILD-SIZE.md#golf-cart-rim-fit-and-circularity--4-october-2026).

Serialized assets are **154,942,895 bytes**; the cart mesh estimate changes **659,132 → 736,970 bytes** for circular equal rims, joined beads and concurrent chassis supports. Existing texture hashes, one shared material and five renderers per LOD are retained. Continuous UVs remove **6,288 position/UV splits** without changing wheel geometry; mid/far spokes omit **1,536 total bevel triangles**. The final combined APK is **4,560 bytes smaller** than the intermediate before that packing refinement. Exact ZIP-entry growth against the starting APK is **74,225 bytes**, with three bytes of container overhead completing the APK delta. Leading art contributors are unchanged and no new large dependency appears. Current per-asset/ZIP records are in `Builds/SizeAudit/latest/`, comparisons and raw source/functional evidence in `Builds/GolfCartRimQA/`. The 75 MB target and phone quality/FPS remain unqualified.

## Golf cart chassis connections and startup rolling — 4 October 2026

Final APK: **88,708,082 → 88,782,546 bytes (+74,464)** against the task-opening combined snapshot, or **88,787,118 → 88,782,546 bytes (−4,572)** against the preceding complete shared release. Hard-limit headroom is **11,217,454 bytes**; the development target remains exceeded by **13,782,546 bytes**. AndroidSubmission, complete clean-cache packing and independent v2 signing pass. Source consistency, release hash, tests and reviewed recovery batch are recorded in [BUILD-SIZE.md](BUILD-SIZE.md#golf-cart-chassis-connections-and-startup-rolling--4-october-2026).

The twelve checked wheel-to-chassis connections add **288 / 224 / 160 triangles** across the three LODs, preserving five active renderers and one material. No new texture, font, shader or package is shipped. The high-quality source geometry/UV hash and delivery map hashes are unchanged. Final serialized assets measure **154,942,895 bytes (−84,608)** against the preceding release, with cart mesh packing **−84,612** and the prefab field **+4**. Independent cart mesh estimate is **736,970 bytes (−1,378)**; its colour, normal, mask and material estimates remain **498,110 / 621,270 / 71,311 / 671 bytes**. Exact compressed entries shrink **4,570 bytes**: cart mesh **−2,479**, IL2CPP **−2,086** and small metadata/engine/config changes explain the result. Container/signing overhead shrinks two more bytes. Asset estimates are distinct from the actual APK measurement.

Leading estimates remain timber **2.247 MB**, character albedo **1.993 MB**, Golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected dependency or texture growth appeared. Complete current reports are in `Builds/SizeAudit/latest/`, with an exact retained audit at `Builds/GolfCartWheelContactQA/Release/latest/`; asset/entry deltas are in `Builds/GolfCartWheelContactQA/`. The latest matching player passes **203 cart assertions** and both format-aware art audits. Close/far LOD and full-steering views were inspected; phone quality/FPS remains unverified.

## Golf cart elevated steering follow — 4 October 2026

Fresh combined release: **88,708,082 → 88,787,118 bytes (+79,036)**. Hard-limit headroom: **11,212,882 bytes**; the 75,000,000-byte development target remains exceeded by **13,787,118 bytes**. AndroidSubmission, complete clean-cache packing and independent v2 signing pass. The elevated camera change adds no imported art, texture, font, shader or package. The package includes the separately refined rim/bead mesh and sand-bowl motor fix. See [hash, source consistency, build and archive](BUILD-SIZE.md#golf-cart-elevated-steering-follow--4-october-2026).

Serialized assets measure **155,027,503 bytes (+254,880)**; the cart mesh accounts for that increase and rises **79,216 estimated compressed bytes to 738,348**. Its maps and material are unchanged. Exact compressed entries grow **79,039 bytes**: cart mesh data **+78,860** and combined IL2CPP **+178** are the main contributors. Container/signing overhead falls 3 bytes. All other independently estimated assets remain unchanged. Leading estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. No unexpected large dependency or camera asset growth appeared.

All 2,925 captured inputs remain stable during packaging and match the primary workspace except the isolated clean-cache editor option. Packing records: `Builds/SizeAudit/latest/`; exact retained release/audit: `Builds/GolfCartOverheadQA/Release/`; comparisons and hashes: `Builds/GolfCartOverheadQA/`. The matching Windows player passes 175 cart and 81 Golf gameplay checks, with native elevated views inspected. Phone touch, rendering/FPS and WAN remain unverified; the 75 MB development target still requires deliberate optimization.

## Golf cart sand-bowl rim traversal — 4 October 2026

The captured vehicle-fix APK measures **88,708,270 bytes**, **+82,432** against the task-opening 88,625,838-byte package and **+188** against the complete Golf mini-game release. Hard-limit headroom is **11,291,730 bytes**; the development target is exceeded by **13,708,270 bytes**. AndroidSubmission, independent v2 signing and fresh packing/contribution analysis pass. Artifact/audit: `Builds/GolfCartRoundFixQA/Release/WhatTheFish-release.apk` and `Builds/GolfCartRoundFixQA/Release/latest/`. See [hash, captured-source scope and archive records](BUILD-SIZE.md#golf-cart-sand-bowl-rim-traversal--4-october-2026).

The ground-sweep fix adds **zero serialized asset bytes** against the preceding complete release; serialized assets remain **154,772,623 bytes**. Exact compressed entry growth is **189 bytes**, with IL2CPP **+188** and a one-byte container difference. The captured cart mesh estimate is **659,132 bytes**; its texture maps and material are unchanged. No new art, shader or package is introduced. The later parallel rim/bead geometry export is outside this measured snapshot; the shared root APK is preserved. The 169-check cart replay includes 40 forward/reverse sand-bowl crossings. Phone touch, rendering/FPS and WAN remain unverified, and the 75 MB development target remains unmet.

## Golf mini game — 4 October 2026

Fresh Golf release: **88,625,838 → 88,708,082 bytes (+82,244)**. Hard-limit headroom: **11,291,918 bytes**; the 75,000,000-byte development target remains exceeded by **13,708,082 bytes**. AndroidSubmission, complete clean-cache packing and independent APK v2 signing pass. Golf reuses course/ball art, font and native UI arrows without importing assets, shaders or packages. This combined snapshot includes separate camera/circular-tyre changes. See [hash, captured source scope, build recovery and archive](BUILD-SIZE.md#golf-mini-game--4-october-2026).

Serialized assets remain **154,772,623 bytes**, **62,952** above the task's opening baseline. The separate cart mesh estimate rises **37,071 bytes to 659,132**, with unchanged maps. Exact compressed entry growth is **82,241 bytes**, including IL2CPP **+44,396**, cart mesh data **+36,476**, metadata **+6,198** and Burst **−5,657**. Container/signing overhead adds 3 bytes. Golf's small script records describe metadata; its development probe behavior is absent from release code. No unexpected large dependency or avoidable Golf texture/font growth was found.

Largest estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. Current packing: `Builds/SizeAudit/latest/`; retained exact snapshot: `Builds/GolfMiniGameQA/Release/latest/`; baseline comparisons and input hashes: `Builds/GolfMiniGameQA/`. All 2,925 captured inputs are stable through the build. Later parallel cart-rim FBX/motor/probe edits are outside this release. Its Golf source passes 81 gameplay checks plus existing sport/cart regressions; physical-phone quality/FPS, touch and WAN remain unverified. The remaining hard-limit reserve is not treated as a development budget; the 75 MB target still needs deliberate optimization.

## Golf cart driving cameras — 3 October 2026

Fresh camera snapshot: **88,625,838 → 88,713,562 bytes (+87,724)**. Hard-limit headroom: **11,286,438 bytes**; the 75,000,000-byte development target remains exceeded by **13,713,562 bytes**. AndroidSubmission, complete packing and independent v2 signature pass. The camera change adds no asset, shader, texture or package; this APK also contains concurrent golf gameplay and cart seating/tyre updates. See [hash, captured source scope, build recovery and archive](BUILD-SIZE.md#golf-cart-driving-cameras--3-october-2026).

Serialized assets total **154,772,623 bytes (+62,952)**. The cart mesh estimate rises **622,061 → 659,132 bytes (+37,071)** for the separate circular-tyre work, with unchanged maps. Exact ZIP entry increases are **44,653 bytes** for IL2CPP, **36,476** for cart mesh data and **6,198** for metadata; overall entry growth is 87,725 bytes, one byte above the APK delta because container overhead also changes. Leading estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. No unexpected large dependency was added. Current audit: `Builds/SizeAudit/latest/`; comparison CSVs and signatures: `Builds/GolfCartCameraQA/`.

The captured camera inputs pass 128 cart and 142 golf checks. Subsequent concurrent golf ball/probe edits are outside this APK's verification scope. Phone touch, clipping/rendering/FPS and WAN remain unverified. The 75 MB target still needs deliberate optimization; this release does not treat the remaining hard-limit reserve as a development budget.

## Golf cart seating and circular tyres — 3 October 2026

The historical vehicle-validation snapshot measures **88,625,838 → 88,707,818 bytes (+81,980)**, with **11,292,182 bytes** hard-limit headroom and **13,707,818 bytes** above the development target. AndroidSubmission, complete clean-cache packing and independent APK v2 signing pass. Its APK and audit are now under `Legacy/20261004-004451-752-golf-cart-rim-delivery/Builds/GolfCartRoundFixQA/Release/`. Shared deliveries are preserved. See [hash, captured source scope, disk recovery and archive records](BUILD-SIZE.md#golf-cart-seating-and-circular-tyres--3-october-2026).

Serialized assets grow **62,952 bytes to 154,772,623**. Circular tyres and rear-well cleanup raise the cart FBX independent estimate **37,071 bytes to 659,132**, retaining existing texture hashes, material, authored hubs and the high-quality master. Mobile totals are 25,833/9,686/3,748 triangles. Exact compressed entry growth is **81,977 bytes**; IL2CPP **+44,132**, metadata **+6,198** and Unity native code **+319** include parallel gameplay/camera code. No unexpected large asset or shipped dependency appeared. Fresh contribution estimates were regenerated from this APK's own packing offsets rather than reused from an earlier report. Physical-phone quality/FPS remains unverified, and the 75 MB development target still needs optimization.

## Golf cart rear-wheel rendering and steering — 3 October 2026

Fresh APK: **88,625,346 → 88,625,838 bytes (+492)**, leaving **11,374,162 bytes** hard-limit headroom and exceeding the development target by **13,625,838 bytes**. AndroidSubmission, complete clean-cache packing and independent APK v2 signing pass. See [release hash, input comparison, testing and recovery](BUILD-SIZE.md#golf-cart-rear-wheel-rendering-and-steering--3-october-2026).

Serialized assets increase only **52 bytes to 154,709,671**. Original mesh positions, triangle totals, UVs, maps and material are retained; corrected face winding restores rear hub visibility. Independent mesh estimate changes **622,153 → 622,061 bytes (−92)**; texture estimates are unchanged. Native IL2CPP grows 556 compressed bytes, metadata/Unity native code stay unchanged, and exact total ZIP growth is 492 bytes. Leading art contributors remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. No new wheel texture, shader/package or unexpected large dependency was introduced. Current reports: `Builds/SizeAudit/latest/`; detailed comparison and signing evidence: `Builds/GolfCartHandlingQA/`. Phone quality/FPS remains unverified.

## Golf cart speed and wheels — 3 October 2026

The final corrected APK is **88,625,346 bytes**: **−131,088 bytes** against the retained 88,756,434-byte base-cart package, or **+804 bytes** against the immediately preceding 88,624,542-byte combined course/cart release. Strict-limit headroom is **11,374,654 bytes**; the development target is still exceeded by **13,625,346 bytes**. The base-cart comparison includes concurrent course refinement. See [release hashes, build provenance and verification](BUILD-SIZE.md#golf-cart-speed-and-wheels--3-october-2026).

Complete fresh packing/ZIP/contribution records are in `Builds/SizeAudit/latest/`; comparison CSVs, source hashes and signing evidence are in `Builds/GolfCartWheelsQA/`. Serialized assets remain **154,709,619 bytes**. Wheel partitioning preserves total triangles and all three texture hashes. Independent cart estimates are mesh **622,153**, normal **621,270**, colour **498,110**, mask **71,311** and material **671 bytes**. Mesh estimate reduction is **9,959 bytes**; prefab estimate increase is **2,246**. The existing five-renderer LODs share one material. No imported wheel texture or new shader/package is packed.

Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. Terrain-sector identity changes in the baseline comparison belong to the concurrent course refinement; assess their aggregate, including removed overlays and the original terrain reduction. No unexpected dependency or avoidable wheel-texture growth was found. Actual APK and independent asset estimates remain different measurements. The source master geometry/UV hash is unchanged; exporter `SceneInfo` machine-path metadata is proven inactive and all active art references resolve relatively. Phone quality/FPS remains unverified. Recovery batches are recorded in the release entry above.

## Golf course refinement and current combined release — 3 October 2026

The fresh combined APK measures **86,896,050 → 88,624,542 bytes (+1,728,492)**, leaving **11,375,458 bytes** below the strict ceiling and exceeding the development target by **13,624,542 bytes**. AndroidSubmission, complete clean-cache packing and independent APK v2 signing passed. The package includes the separate protocol 19 cart speed/wheel update. All 2,903 authored asset/package/settings hashes match, with only the isolated editor's clean-cache option differing. See [release provenance and build-process memory mitigation](BUILD-SIZE.md#golf-course-refinement-and-current-combined-release--3-october-2026).

Current records are in `Builds/SizeAudit/latest/`; comparisons and source hashes are in `Builds/GolfCourseRefinementQA/`. Serialized assets are **154,709,619 bytes (+2,598,806)**, distinct from compressed APK bytes. Independent course estimates fall **652,179 → 610,202 bytes (−41,977)**, and the original terrain estimate falls **80,439 bytes**: together **−122,416 bytes**. Five terrain sectors carry precision openings. No green overlay, old orange/putting material or baked golf grass is packed, and the course adds no imported texture or font.

Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. The cart adds the main delivered dependencies: approximately **622,153** mesh, **621,270** normal, **498,110** colour and **71,311** mask bytes in independent compression estimates, plus tiny prefab/font/licence/material/script records. These separate cart additions explain the overall increase despite the course reduction. No unrelated large art growth was found.

Exact compressed ZIP entries grow **1,727,121 bytes**; signing/ZIP overhead accounts for **1,371 bytes**. Assess split-file changes in aggregate. These exact entry measurements and independent per-asset estimates describe different quantities. Actual close/distant and cup-detail views, collision, terrain colour/UV preservation and mobile grass clearing passed their map checks; phone rendering/FPS remains unverified. Unused course assets were archived with GUID/runtime/generator/provenance evidence under `Legacy/20261003-192351-292-golf-course-refine-unused/`.

## Golf cart summon and driving snapshot — 3 October 2026

The retained protocol 18 base APK measures **86,896,050 → 88,756,434 bytes (+1,860,384)**, leaving **11,243,566 bytes** under the strict ceiling and exceeding the development target by **13,756,434 bytes**. AndroidSubmission, complete clean-cache packing and the final retained file's APK v2 signing passed. Later course refinement, protocol 19, doubled speed and wheel animation remain outside this measured snapshot. See [release provenance](BUILD-SIZE.md#golf-cart-summon-and-driving-snapshot--3-october-2026).

`Builds/GolfCartQA/ReleaseBase/latest/` preserves the actual build's packing and ZIP records, with asset estimates regenerated from that APK and its packing offsets. Serialized assets are **154,919,835 bytes (+2,809,022)**. Cart estimates total **1,830,717 bytes**: mesh **632,112**, normal **621,270**, colour **498,110**, mask **71,311**, subset font **3,638**, full licence **1,975**, prefab **1,097**, material **671** and tiny script records. Script records are not compiled native-code cost. All cart LODs share one material and the three maps; Android uses ASTC 6×6 colour, 4×4 normal and 8×8 mask. The master, original ZIP, full 17.77 MB font and unused source textures are not packed.

Exact ZIP-entry growth totals **1,859,011 bytes**, with **1,373 bytes** of ZIP/signing overhead making the actual APK delta. The four main cart art entries add **1,823,308 compressed bytes**; IL2CPP adds **23,228**, metadata **3,313**, the font entry **3,746** and licence entry **2,059**. Full comparison is `Builds/GolfCartQA/base-apk-entry-deltas.csv`. Largest pre-existing art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No avoidable large asset dependency was introduced.

The FBX exporter now rewrites both direct image filenames and active nested Video `Path` properties through format-aware parsing, preserving all non-path payloads. Blender image/library dependencies and FBX texture references were checked after relocation to a path containing spaces and execution from an unrelated directory. The base master has seven existing relative images and no external libraries; the supplied original FBX embeds all four source maps. The retained relocation audit identifies only an inactive `FBXHeaderExtension/SceneInfo/Properties70/P` exporter-provenance field as the allowed historical metadata exception; active texture dependencies are relative. Evidence is `Builds/GolfCartQA/portable-relocated-base-audit.json` and `portable-font-final.txt`. Cleanup batches and functional/device limits are in [the release record](BUILD-SIZE.md#golf-cart-summon-and-driving-snapshot--3-october-2026).

## Five-hole golf map — 3 October 2026

The final APK measures **86,563,288 → 86,896,050 bytes (+332,762)**, leaving **13,103,950 bytes** under the strict ceiling and exceeding the development target by **11,896,050 bytes**. Matching-source isolated AndroidSubmission, populated clean-cache packing and APK v2 signing passed after the open main-project attempt exited. See [release provenance and validation](BUILD-SIZE.md#five-hole-golf-map--3-october-2026).

`Builds/SizeAudit/latest/` contains complete current packing offsets, ZIP entries and estimates. Serialized assets are **152,110,813 bytes (+896,940)**; they are not APK bytes. New course estimates total **652,179 bytes**: derived terrain about **474,257**, greens **159,170**, and shared cup/flag/tee meshes and materials **18,752**. The original terrain estimate falls from **1,284,955 to 964,827 bytes (−320,128)** as four sectors use precise derived geometry. Terrain cutouts remain uncompressed to retain the small cup boundaries; unused vertices are removed. Desktop grass derivatives are retired for the existing mobile grass library and are absent from the APK.

An inherited `RI_Turf_Mask.png` appeared in the first trial package: **5,592,556 serialized bytes**, approximately **147,833 independently compressed bytes**. The putting material now clears its metallic texture/keyword. This removes the dependency without altering the source texture/master or grass colour/normal detail. Actual APK savings are **149,460 bytes** against the **87,045,510-byte** trial. No new imported texture, font, model or package was added. Largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No other unexpected art growth was found.

ZIP entry comparisons include split-file boundary shifts and should be assessed in aggregate. IL2CPP grows **1,646 compressed bytes**, managed metadata **112**; Unity native code and Burst are unchanged. Total compressed entry delta is **332,593 bytes**; ZIP/signing overhead accounts for the remaining **169** APK bytes. These exact entry measurements differ from independently compressed asset estimates.

Evidence: `Builds/GolfFiveHolesQA/`. Source masters match Git HEAD; generated Unity references, geometry winding, course collision, close/distant views and mobile grass clearing were inspected. Phone rendering/FPS remains unverified. Archive batches: `Legacy/20261003-165821-340-golf-five-holes-trial-mesh/` and `Legacy/20261003-171933-414-golf-five-holes-delivery/`. Earlier audit records below retain their original scope.

## Football animation rebuild — 4 October 2026

Actual signed AndroidSubmission APK: **86,764,912 → 86,774,768 bytes (+9,856)**. Hard-limit headroom: **13,225,232 bytes**; development-target overrun: **11,774,768 bytes**. [Build provenance, source scope and SHA-256](BUILD-SIZE.md#football-animation-rebuild--4-october-2026).

The existing character mesh, skeleton, textures and imported clips are unchanged. One distance-driven gait, coordinated FK arms, contact IK and 28 editable control-space records replace the previous mixed animation clocks. The resource grows by **1,560 serialized bytes / approximately 280 independently compressed bytes**, to **13,108 / 1,854 bytes**. These estimates exclude compiled code and are not exact allocations within the APK's shared ZIP chunks.

The largest changed ZIP entries are IL2CPP **+9,778**, global metadata **+1,181**, and Burst **−1,530 bytes**. Fresh per-asset estimates retain the same leading art contributors: timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB**, and stadium **1.556 MB**. No mesh, texture, animation pack, audio or middleware was added. Shared locomotion clips remain needed by other sports; removing them would not be a safe size optimization.

Serialized assets total **151,510,277 bytes**, distinct from the actual compressed APK size. Current populated records are in `Builds/SizeAudit/latest/`, with a task copy in `Builds/FootballMotionPolishQA/FinalSizeAudit/`. The task folder retains `before.json`, `release.json`, `apk-entry-deltas.csv`, `apk-signature.txt` and source-hash verification. The 75 MB development target is still unmet; preserve the remaining reserve. See [verification](VERIFICATION.md#football-animation-rebuild--4-october-2026) for test scope and physical-device limits.

## Transparent lagoon and three fish sizes — 3 October 2026

Actual task APK: **86,574,958 → 86,757,976 bytes (+183,018)**. Hard-limit headroom: **13,242,024 bytes**; development-target overrun: **11,757,976 bytes**. Frozen-checkout AndroidSubmission and v2 signing passed. See [source/build scope and hash](BUILD-SIZE.md#transparent-lagoon-and-three-fish-sizes--3-october-2026). The concurrent root APK/audit was retained; current fishing evidence is under `Builds/FishingWaterQA/FinalSizeAudit/`.

The largest new fishing art estimates are lagoon surface **43,086**, shared fish **40,321**, sandy bed **19,892**, water shader **8,560**, fish shader **8,088**, and bed shader **5,540 bytes**. Including their materials, the group totals **125,801 independent DEFLATE-estimated bytes**. These estimates are not exact APK allocations because ZIP chunks share compression. Serialized packed assets total **151,508,417 bytes**. The authoring master stays outside the player; all three fish sizes share the two delivered meshes/material.

The final build reuses `RI_Sand_BaseColor.png`, already required by the terrain. `KeepSourceTexture` bypasses replacement in both mobile material preparation and application, with a case-insensitive tag comparison because Unity normalizes serialized boolean-looking tags. The packing report confirms `MobileMaterials/RI_Sand.png` is absent. The compact intermediate APK was **87,021,080 bytes**; final sharing removes **263,104 actual package bytes**. Earlier mesh simplification removed **80,758 bytes** without changing close/distant silhouettes. Reviewed obsolete generated bed material variants were archived only after checking scene/prefab references and the replacement path.

Combined baseline-to-final ZIP changes include IL2CPP **+34,172**, Burst **+6,703**, metadata **+4,741**, and the concurrent net shader **+9,348 bytes**. Fishing split-entry differences reflect both new art and changed chunk boundaries. Existing largest art contributors remain unchanged: timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB**, and stadium **1.556 MB**. No extra image texture, audio, imported animation or package was added for fishing. The remaining reserve is below the desired development margin; no phone performance qualification is claimed.

## Football character motion — 3 October 2026

The final signed AndroidSubmission APK measures **86,574,958 → 86,764,912 bytes (+189,954)**, leaving **13,235,088 bytes** below the strict ceiling and exceeding the working target by **11,764,912 bytes**. The comparison includes concurrent fishing and net changes. [Build provenance and hash](BUILD-SIZE.md#football-character-motion--3-october-2026).

The new 26-take resource is **11,548 serialized bytes / approximately 1,574 independently compressed bytes**. This estimate excludes its compiled runtime code and ZIP container interactions. Animation reuses the existing character, materials and run/jump/slide clips, with no imported mesh, texture, motion pack or middleware. Combined IL2CPP growth is **41,048 bytes**, metadata **4,734** and Burst **6,703**; these cannot be attributed solely to animation. The concurrent net shader adds a 9,348-byte ZIP entry. The largest remaining changed ZIP chunks are in fishing scene data.

The first successful package was 87,028,088 bytes. Rebuilding with the concurrent material-sharing correction reduced it by **263,176 bytes**, avoiding a duplicated fishing texture while retaining its shared source image. Fresh asset estimates remain led by timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected character-art growth was found. Serialized assets total **151,508,629 bytes**, not the compressed delivery size.

Current populated packing/ZIP records are in `Builds/SizeAudit/latest/`, with a preserved task copy at `Builds/FootballMotionQA/ReleaseSizeAudit/`. `release.json`, `apk-entry-deltas.csv`, `apk-signature.txt` and input manifests in the task folder record exact bytes, comparisons, v2 signing and an unchanged final build input set. [Verification](VERIFICATION.md#football-character-motion--3-october-2026) records the 1,807 checks, remaining contact tolerances and cleanup. Device quality/FPS and WAN remain unverified; 75 MB is still unmet.

## Position-aware goal net — 3 October 2026

Actual APK: **86,574,958 → 87,028,060 bytes (+453,102)**. Hard-limit headroom: **12,971,940 bytes**; development-target overrun: **12,028,060 bytes**. Main-project AndroidSubmission and v2 signing passed. See [release provenance, artifact and SHA-256](BUILD-SIZE.md#position-aware-goal-net--3-october-2026).

The net shader adds **9,348 actual ZIP bytes**, versus a **9,291-byte independent per-asset DEFLATE estimate** and **18,520 serialized bytes**. Its shared material is **112 serialized bytes**. Script records are serialization records, not compiled-code cost; the opt-in review implementation is excluded from release. The source stadium and its editable masters are preserved. The new net uses generated geometry and shader motion, without textures, clips or a cloth solver.

The combined snapshot also contains concurrent fishing and football-motion changes. Largest new art estimates: mobile sand **259,471**, lagoon surface **43,086**, fish **40,321**, lagoon bed **19,892 bytes**. IL2CPP grows **41,048**, metadata **4,734**, and Burst **6,703 compressed bytes**. Large shifts among `sharedassets4.assets.split*` entries reflect changed packed fishing content and chunk boundaries; individual split deltas are not per-asset sizes. The five largest existing art contributors remain unchanged. Serialized total: **151,858,317 bytes**.

Task baseline/release hashes, ZIP and asset deltas, full build logs, signature verification and populated packing offsets are retained in `Builds/FootballNetQA/`. The exact measured APK is retained there under `Android/`; root audit outputs may later be replaced by another build. The package remains above the 75 MB development target. No physical Android device was attached. See [verification](VERIFICATION.md#position-aware-goal-net--3-october-2026) for functional/render coverage and archived iterations.

## Interactive football boundary and twin-rail aim — 3 October 2026

Measured APK: **86,498,912 → 86,574,958 bytes (+76,046)**. Headroom: **13,425,042 bytes** below the strict ceiling; development-target overrun: **11,574,958 bytes**. The baseline is the actual older local package measured before editing, not the newer historical release documented below. The difference includes pulled code changes as well as these effects. The main-project AndroidSubmission build and APK v2 signing check passed. See [release provenance and SHA-256](BUILD-SIZE.md#interactive-football-boundary-and-twin-rail-aim--3-october-2026).

Current packing/ZIP records and per-asset estimates are in `Builds/SizeAudit/latest/`. Serialized total: **151,235,033 bytes**, not the APK allocation. New standalone shader ZIP entries cost **7,604 bytes** (boundary) and **7,056 bytes** (charge arrow), including their entry content overhead. Independent per-asset DEFLATE estimates are **7,549 / 7,002 bytes** respectively and should not be substituted for those actual entries. The wall material is 260 serialized bytes; the updated aim material is 160. Each new runtime/review script record is 112 serialized bytes, which does not measure compiled native code; the review implementation is excluded from release.

Actual ZIP growth is primarily IL2CPP **+50,982**, metadata **+11,757**, new shaders **+14,660**, Unity native code **+1,413**, offset partly by Burst **−5,225 bytes**. Existing art remains led by timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No texture, source model, audio, animation clip or package dependency was added. Procedural geometry is created once and reused; animation needs no particle system, bloom, light or additional render pass. Preserve the remaining reserve; 75 MB is still unmet.

Baseline/release bytes and hashes, ZIP deltas, signature result, final Windows/Android logs and previews are under `Builds/FootballFeedbackQA/`; detailed [verification and cleanup](VERIFICATION.md#interactive-football-boundary-and-twin-rail-aim--3-october-2026) cover 559 checks. No phone was attached, so device rendering/FPS/touch remain unverified. Earlier sections below retain their historical source/build scope.

## GitHub integration release — 3 October 2026

The fresh combined release measures **86,532,484 → 86,563,288 APK bytes (+30,804)**, leaving **13,436,712 bytes** below the strict ceiling and exceeding the development target by **11,563,288 bytes**. The main-project AndroidSubmission attempt exited with the user's editor running; the matching-source isolated clean-cache AndroidSubmission build passed, supplied populated packing offsets and passed v2 signature verification. See [the release provenance](BUILD-SIZE.md#github-integration-release--3-october-2026).

Complete current reports are in `Builds/SizeAudit/latest/`. Serialized assets are **151,213,873 bytes (+376)**; this is not the compressed APK allocation. Largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. Entry comparison chiefly attributes growth to IL2CPP **+25,950 bytes**, metadata **+2,906**, Burst **+851** and Unity native code **+640**. The largest individual art-estimate change is **+493 bytes** for the existing golf LOD0. Two basketball MonoScript records add 208 serialized bytes; neither represents the full compiled-code cost. No new imported art/package or unexpected art growth appeared. The existing 4,220-byte mobile dither and two tiny SSAO script records remain; no desktop SSAO shader/texture collection was introduced.

ZIP/asset deltas, source hashes, signature results and the 1,061-assertion Windows/loopback summary are retained in `Builds/GitMergeQA/`. Pre-merge artifacts and completed merge backups were archived with recovery hashes under `Legacy/20261003-150444-050-git-merge-baseline/` and `Legacy/20261003-151034-738-git-merge-cleanup/`. Phone quality/FPS, physical touch and WAN latency remain unverified. Earlier audit records below retain their original source/build scope.

## Earlier release audits

The 3 October legacy-state cleanup release measures **86,530,508 → 86,532,484 bytes (+1,976)**, with **13,467,516 bytes** hard-limit headroom and a **11,532,484-byte** development-target overrun. The prescribed root AndroidSubmission build succeeded; a matching-source isolated clean-cache build supplied the complete current packing report after the root incremental report omitted offsets. The removed state adds no delivered assets or dependencies. Serialized assets remain 151,213,497 bytes. Fresh art estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. ZIP growth is chiefly IL2CPP +2,006 bytes, offset by metadata -28 bytes and tiny build-record changes. Regenerated Performance Test resource names keep their compressed sizes; no art growth was found. Current reports are in `Builds/SizeAudit/latest/`, evidence is in `Builds/FootballLegacyCleanupQA/`, and the recovery batch is `Legacy/20261003-143827-669-football-legacy-state/`. See [the measured release and validation scope](BUILD-SIZE.md#football-legacy-state-cleanup--3-october-2026).

Team A lettering adds one small runtime vector UI mesh using the existing Canvas material, with no imported art/font/shader/package. Task baseline: **86,529,684 bytes**; retained concurrent snapshot: **86,527,020 bytes (−2,664)**; newer current combined-source APK: **86,530,508 bytes (+824 against baseline)**, leaving **13,469,492 bytes** below the hard limit and exceeding the working target by **11,530,508 bytes**. Both measured APK signatures passed. The final wordmark/HUD hashes match the newer build. Fresh packing and ZIP inspection found unchanged main art estimates and no unexpected large asset growth; differences include other code revisions. The 112-byte serialized wordmark script record is about 88 independently compressed bytes, separate from native code cost. See [the detailed measurement and provenance](BUILD-SIZE.md#team-a-lettering--3-october-2026). Current evidence: `Builds/FootballTeamAWordmarkQA/`; current release audit remains `Builds/SizeAudit/latest/`.

## Goal-net collision release — 3 October 2026

Actual APK: **86,529,684 → 86,530,508 bytes (+824)** across the combined current source, leaving **13,469,492 bytes** below the strict ceiling and **11,530,508 bytes** above the development target. AndroidSubmission, populated clean-cache packing and v2 signing passed. See [the measured release and hash](BUILD-SIZE.md#goal-net-collision-release--3-october-2026); earlier pending records below remain historical. Current ZIP/asset evidence is in `Builds/SizeAudit/latest/` and `Builds/FootballGoalNetQA/`.

Eight small convex net panels are generated once at runtime: **64 vertices/96 triangles total**, with no new imported art or shader/package dependency. Art estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. The goal-net MonoScript contributes 96 serialized bytes (about 82 independently compressed), separate from native code/metadata. ZIP changes principally comprise Unity native code -19,145 bytes, IL2CPP +15,599, metadata +5,050 and Burst -851; the comparison includes retained earlier source changes and is not an isolated net-only delta. Small Performance Test resource GUID/name changes do not add asset content. No desktop SSAO shader/texture collection or unexpected large art growth was found. The existing single mobile dither texture remains 4,220 serialized bytes. The 75 MB milestone and physical-phone qualification remain outstanding.

The 3 October 1.8x charging-arrow update changes one runtime longitudinal scale endpoint and matching verification. It adds no geometry, assets, shaders or packages. Baseline APK: **86,529,684 bytes**, headroom **13,470,316 bytes**, development target overrun **11,529,684 bytes**. Windows/AndroidSubmission attempts exited while Unity had the project open, so after-size/delta/headroom and actual rendering are **unverified**. Evidence: `Builds/FootballAim18xQA/`; latest packing estimates remain those of the earlier release. See [the pending build record](BUILD-SIZE.md#kick-arrow-charge-growth-18x--build-pending-3-october-2026).

The 3 October shorter-head follow-up increases corner rounding and compresses only the triangular head region to 80% of its prior length. It retains the 50-vertex/47-triangle runtime mesh, existing material and all shipped assets/packages. Baseline APK: **86,529,684 bytes**, with **13,470,316 bytes** hard-limit headroom; development target overrun is **11,529,684 bytes**. Fresh Windows/AndroidSubmission builds exited while the project was open, so after-size/delta/headroom and actual visuals are **unverified**. Geometry checks passed against the extracted generator with Unity API stubs. Current evidence is in `Builds/FootballShortHeadAimQA/`; the latest asset estimates belong to the earlier build. See [the pending build record](BUILD-SIZE.md#rounder-arrow-with-shorter-head--build-pending-3-october-2026).

The 3 October rounding update adds curved geometry generated once at runtime (50 vertices, 47 triangles), sharing the same material/shader and adding no imported assets or packages. Baseline APK: 86,529,684 bytes, with 13,470,316 bytes below the hard limit; the 75 MB target remains unmet. Geometry-only checks passed, but fresh Windows/AndroidSubmission attempts exited while Unity had the project open. After-size/delta, rendering and phone qualification remain **unverified**. `Builds/FootballRoundedAimQA/` retains evidence; earlier packing reports do not validate this source. See [the pending build record](BUILD-SIZE.md#rounded-charging-arrow--build-pending-3-october-2026).

The latest arrow-width change adjusts only transverse runtime scale to 80% (0.85 to 0.68 m); no asset/shader/package was added or modified. Baseline APK is 86,529,684 bytes (13,470,316 bytes below the hard limit). AndroidSubmission exited while Unity had the project open, so this source's after-size/delta and rendering remain **unverified**. Logs: `Builds/FootballAimWidthQA/`. Earlier size-audit entries do not validate this edit. See [the pending build record](BUILD-SIZE.md#solid-arrow-width--build-pending-2-october-2026).

## Attached-control snapshot audit — 2 October 2026

Actual APK: **86,505,374 → 86,529,684 bytes (+24,310)**; strict-limit headroom **13,470,316 bytes**, development-target overrun **11,529,684 bytes**. The isolated clean-cache AndroidSubmission build and v2 signature verification passed. The complete current packing report has **151,213,149 serialized bytes**, which must not be confused with the compressed APK size. `Builds/SizeAudit/latest/` contains populated packing offsets, actual ZIP entries and freshly computed asset compression estimates. `Builds/FootballControlQA/apk-entry-deltas.csv` compares the two measured APKs.

Main art estimates remain unchanged: timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. The new Resources aim material is 1,760 serialized bytes, approximately 697 compressed bytes, with no imported texture or new shader family. ZIP growth is chiefly `libunity.so` +20,458 bytes and `libil2cpp.so` +4,496; generated Burst code decreases by 968 and metadata increases by 654. Other serialization/ZIP changes account for the remainder. No avoidable large art addition or desktop SSAO shader/noise collection was found. The existing single mobile pipeline dither texture contributes 4,220 serialized bytes. Tiny serialized SSAO C# type records are not the desktop SSAO shader or texture collection.

This snapshot covers attached possession and the 25%/0.65 m tuning, shared jump/tackle/whiff updates, visible restart reset and the transparent 0.8–1.44 m outline aim. **Subsequent concurrent solid-arrow and kickoff-formation edits are not included; current complete-working-tree APK freshness and their size delta remain unverified.** The older pending notes below remain historical attempt records for the corresponding source. See [the measured build/hash and scope](BUILD-SIZE.md#validated-attached-control-snapshot--2-october-2026). The 75 MB target remains an outstanding milestone; no unrelated visual-quality reduction was made. Phone quality/FPS and WAN behavior are unverified.

## Earlier concurrent attempt records

The solid arrow replaces the outline with a seven-vertex/three-triangle runtime mesh and reuses the existing transparent material. No new imported art or shader/package dependency was introduced. Baseline APK: **86,529,668 bytes**, **13,470,332 bytes** of hard-limit headroom; the 75 MB working target is exceeded by 11,529,668 bytes. Both new builds exited while Unity had the project open, so after-size, delta and source-specific headroom remain **unverified**. `Builds/FootballSolidAimQA/` retains baseline hash, earlier audit snapshot, source geometry check and build failure logs. The latest contribution report is from the earlier APK and cannot validate this source. See [the pending build record](BUILD-SIZE.md#solid-charging-arrow--build-pending-2-october-2026).

Fixed kickoff formations add fifteen explicit slot vectors to the existing match rules resource and authority-only team-slot allocation/pose validation. They preserve the ball reset anchor and all art/field geometry; no shipped dependency was introduced. The pre-change APK measured **86,529,668 bytes** with **13,470,332 bytes** hard-limit headroom, still **11,529,668 bytes** above the development target. Fresh main-project Windows and AndroidSubmission builds exited while Unity had the project open. Post-change APK bytes, delta and headroom are **unverified**; the existing APK is only a baseline. Latest contribution estimates are empty and no fresh contributor report exists for this change. Evidence is in `Builds/FootballFormationQA/`. See [the pending build record](BUILD-SIZE.md#fixed-football-kickoff-formations--build-pending-2-october-2026).

The latest aim-length correction changes only source/regression expectations to **0.8–1.44 m** (1.8 times the shortened base). No asset, shader or package change was made. Fresh APK size, delta and headroom remain **unverified**: the baseline APK was absent and new Windows/AndroidSubmission attempts exited while Unity had the project open. Current logs: `Builds/FootballKickAimLengthQA/`. A separately supplied APK now measures 86,529,668 bytes (13,470,332 bytes of headroom), but inclusion of this correction remains unverified. Its latest isolated-build audit has an empty asset-contribution CSV. See [the pending build record](BUILD-SIZE.md#kick-aim-length-clarification--build-pending-2-october-2026).

The transparent, charge-scaled aim adds a texture-free Resources material using the existing URP glass shader/keywords. The source material is 4,017 bytes; this is **not** its measured packed/APK size. Actual APK bytes, delta, headroom and shader-variant contribution remain **unverified**: no baseline APK existed and both fresh build attempts exited while Unity had the project open. `Builds/FootballKickAimChargeQA/` retains failure logs; the latest audit has no new contributor report. See [the current pending build record](BUILD-SIZE.md#charge-scaled-transparent-kick-aim--build-pending-2-october-2026). Actual Unity transparency, phone appearance/FPS and runtime checks remain pending.

The charging kick aim uses the existing ball shader, one runtime material and a five-point LineRenderer; no imported art, texture or package was added. APK bytes, delta, headroom and actual shader contribution remain **unverified** because no baseline APK existed and both fresh build attempts were blocked by the open Unity project. The latest audit has only the failure log; `Builds/FootballKickAimQA/` retains this task's logs. See [the pending build record](BUILD-SIZE.md#charging-kick-aim--build-pending-2-october-2026). Phone appearance/FPS and Unity visual execution are also unverified.

Closer-control tuning changes only the ball anchor (0.9 to 0.65 m) and possession speed multiplier (0.85 to 0.75), plus matching source defaults/checks. No model, texture, audio, animation or package was added. The pre-change APK was absent and new Windows/AndroidSubmission attempts exited while this Unity project was open. APK bytes, delta, headroom and contributor growth are **unverified**; the latest audit has only the failed log. Evidence is in `Builds/FootballControlTuningQA/`. See [the pending build record](BUILD-SIZE.md#football-closer-control-tuning--build-pending-2-october-2026).

The missed-slide penalty adds only gameplay logic: 0.8 seconds at 40% movement speed after a slide hits neither player nor ball. No shipped art, audio, animation or package was added. The pre-change APK was absent, and new Windows/AndroidSubmission attempts were blocked by the open Unity project. APK bytes, delta, headroom and contributor growth remain **unverified**. Failure logs and logic evidence are in `Builds/FootballWhiffQA/`; the latest audit has only the failed build log. See [the pending size record](BUILD-SIZE.md#football-missed-slide-penalty--build-pending-2-october-2026).

The slide ball launch follow-up reuses existing ball art and introduces one tuning field, with no additional asset/package dependencies. Its APK size is **unverified**: no pre-change APK existed, and both fresh builds exited while the project was open. `Builds/FootballBallLaunchQA/` retains failure logs; the latest audit has no new contributor report. See [the current pending-build record](BUILD-SIZE.md#slide-ball-launch--build-pending-2-october-2026).

Football jump restoration and tackle recovery changes are currently **size-unverified**. No pre-change APK was present, and the new Windows and AndroidSubmission attempts exited with code 1 while the project was open in Unity. No model, texture, animation or package was added or removed. Failed build logs are in `Builds/FootballTackleQA/`; the latest audit contains the failed attempt log, with no new contributor report. The measured figures below predate this code change. See [the pending-build record](BUILD-SIZE.md#football-jump-and-tackle-recovery--build-pending-2-october-2026).

Current APK measures **86,505,374 bytes**, **+13,844 bytes** against the preserved 86,491,530-byte match release, including the pending glass scoreboard and current flow update. It leaves **13,494,626 bytes** below the strict hard limit, but exceeds the development target by **11,505,374 bytes**. Native editor AndroidSubmission and v2 signature verification passed after the prescribed batch script exited on the open-project lock. See [BUILD-SIZE.md](BUILD-SIZE.md) for hash and build details.

The current detailed packing/ZIP comparison in `Builds/SizeAudit/latest/` and `Builds/FootballFlowQA/` found unchanged main art contributors and no added model, imported texture, animation or package. Growth is chiefly IL2CPP (+11,665 compressed bytes) and metadata (+2,157); procedural HUD textures are generated at runtime. The 72-byte serialized rules resource remains compact. Preserve the mobile pipeline and existing art packing. The working target remains an outstanding shared optimization milestone; this small gameplay revision does not justify reducing unrelated art quality. Windows flow/physics/network checks passed, but phone appearance/FPS and WAN latency remain unverified.

## Earlier football match release — 2 October 2026

The current combined-source match APK is **86,491,530 bytes**, leaving **13,508,470 bytes** below the hard limit; the 75 MB target remains unmet by **11,491,530 bytes**. No current pre-task APK was present, so the isolated match-size delta is unverified. The native editor AndroidSubmission build and APK v2 signature passed. Actual ZIP entries, populated packing offsets and per-asset compression estimates are in `Builds/SizeAudit/latest/`; see [BUILD-SIZE.md](BUILD-SIZE.md) for hash, contributor sizes and the separate historical baseline. Match rules add no art or packages and only one 72-byte serialized resource. Phone performance remains unverified.

## Historical upstream release — 1 October 2026

The upstream record measured 85,970,822 bytes. It predates the combined local football and match implementation; the following historical local and upstream records do not describe the new APK.

## Upstream basketball animation snapshot — 3 October 2026

The current combined football and basketball APK measures **86,498,912 bytes (86.50 MB / 82.49 MiB)**, leaving **13,501,088 bytes** to the strict 100,000,000-byte ceiling. It exceeds the 75 MB working target by **11,498,912 bytes**. The pre-merge basketball animation APK measured **85,993,642 bytes**; the measured increase is **505,270 bytes**. `AndroidSubmission` and APK v2 signature verification passed. Current SHA-256: `6EB19CFA79CF55571963DA49DFE687B6402E6F7A675A375B9C1503C9D0F91418`.

The packed-assets report adds a **550,636-byte uncompressed serialized football FBX**, two small football materials and script metadata. The source FBX is 1,078,236 bytes; neither figure is its exact contribution to the compressed APK. ZIP growth is concentrated in football scene asset splits: +170,672, +141,525 and a new 129,038-byte split; IL2CPP adds 20,834 bytes and managed metadata 3,679 bytes. Changes to other splits partly offset these amounts. No new texture or package dependency appeared. The large existing art contributors remain timber, character colour, golf structures, character geometry and stadiums. See `Builds/SizeAudit/latest/` for packed assets, APK entries and the successful build report.


The merged Windows player passed **367 checks** across football physics and host/guest replication, basketball motion and passing, and jump gameplay and replication. The imported FBX has no active image references, and its Blender master has no image or library dependencies. No physical Android phone or WAN test was performed. The pre-merge APK and size audit were archived locally in `Legacy/20261002-155631-852-football-merge-build-baseline/`.

The teammate's historical charge-kick APK measured **83,787,566 bytes** (+584 bytes against its goal-entry release), with **16,212,434 bytes** of hard-limit headroom. Earlier football builds measured **83,786,982** and **83,779,278 bytes**. These figures belong to a separate working tree; its asset audit, signature checks and visual quality findings are documented in [the football build history](BUILD-SIZE.md) and [football prototype](FOOTBALL-PROTOTYPE.md).

## Basketball animation release before merge — 2 October 2026

The earlier APK, now archived locally, was built on 2 October at 15:40:51 Malaysia time (07:40:51 UTC) with Unity 6000.3.20f1, release IL2CPP, ARM64, minimum Android API 26 and default Android ZIP compression. `AndroidSubmission` passed the size gate; APK v2 signature verification passed.

SHA-256: `5FD964709B5D449ED0508FDB56E287DAFA3FCD48A89540191B7FE71447BD12CF`.

## Basketball animation and passing

Measured before/after APK: **85,970,874 to 85,993,642 bytes (+22,768 bytes)**. The implementation reuses the character skeleton, both LODs, running/jumping clips, ball and all textures/materials. ZIP comparison attributes 19,490 added bytes to IL2CPP and 2,551 to managed metadata; native/archive bookkeeping accounts for the small balance. No large asset or package dependency was added. Unity retains 208 uncompressed serialized bytes of new MonoScript metadata, including a stub for the development-only review; the review implementation is compiled out of release.

The largest estimated contributors remain timber (2.247 MB), character colour (1.993 MB), golf structures (1.981 MB), character geometry (1.574 MB) and stadiums. The packed-asset comparison found no art growth. Per-asset DEFLATE estimates are approximate and do not sum exactly to APK ZIP chunks.

All **563 primary Windows player checks** passed: 220 motion checks at fixed 20/30/60/120 FPS, 95 gameplay/room checks, 53 ball-physics/replication checks, 140 jump/momentum/room regressions and 55 portable-path repeats. After correcting idle-pose equality for replication, the 56-check motion review and two-player regression passed again. The build also passed 65 editor motor checks. Close and side captures of both character LODs were inspected; dribble contact error stayed below 0.4 mm in these samples. AndroidSubmission and v2 signature verification passed. No phone was connected; Android appearance, touch ergonomics, FPS and WAN latency remain unverified.

Evidence is under `Builds/BasketballMotionQA/`: exact bytes and hashes, signature/build logs, before/final size audits, packed-asset and ZIP comparisons, per-asset estimates, fixed-rate results, portable evidence and the actual player video. Other suite locations and all five archive batches are recorded in [basketball animation](BASKETBALL-ANIMATION.md). The local-only batches contain superseded pose iterations, intermediate APKs/logs, the interrupted jump run and portable fixtures. No active art, authoring master or current build was archived. Pre-existing jump/momentum work was preserved; Git publishing remains with the user.

## Jump momentum

Measured before/after APK: **85,970,822 to 85,970,874 bytes (+52 bytes)**. No art or package dependency was added. The ZIP comparison shows IL2CPP growing by 1,430 bytes, Burst shrinking by 1,478 bytes, and small metadata/archive changes; the net APK difference is measured from the final file. Scene/art/shader data remains about 59.64 MB. The largest contributors are unchanged: timber, character maps/meshes/animation, golf structures/terrain and stadiums.

All **287 Windows player checks** passed: 140 jump/momentum/room, 24 tackle/room, 62 turning/room and 61 offline basketball gameplay checks. Another **65 editor motor checks** cover 16 headings at 20/30/60/120 FPS, release-distance consistency, bounded reversal, analog steering, reset, wall tangents and irregular-frame integration. Native player captures show forward and diagonal coasting plus steering after release. Fixed-rate captures do not establish device frame rate. Android submission and v2 signature verification passed; no phone was connected.

Evidence is under `Builds/MomentumQA/`: before/after APK bytes and hashes, build/signature logs, `SizeAuditBefore/`, `SizeAuditFinal/`, asset and ZIP comparisons, motor results, test summary and `momentum-review.mp4`. Player run locations are recorded in [jump verification](JUMP-ANIMATION.md). No assets or masters were changed, and no archive batch was needed for this update.

## Basketball gameplay revision

Measured before/after APK: **85,957,194 to 85,970,822 bytes (+13,628 bytes)**. No new mesh, texture, animation, audio or package dependency is introduced. The largest contributors remain timber (about 2.25 MB), character albedo (1.99 MB), golf structures (1.98 MB), character mesh (1.57 MB) and stadium geometry. Per-asset estimates use separate DEFLATE compression and do not sum exactly to APK ZIP chunks. Entry comparison attributes the small increase primarily to runtime code/metadata; the scene/art budget remains stable.

All **82 basketball gameplay checks**, **53 foundation checks** and **40 jump regression checks** passed on Windows. Gameplay checks include real 2-24 m rim crossings, repeated pickup after a rebound, ownership arbitration, host/guest shot input and holder disconnect/restart recovery. The portable copied-player fixture repeated all **61 offline checks** from a path with spaces and an unrelated working directory. No phone was connected; Android appearance, touch ergonomics, FPS and WAN latency remain unverified.

Current evidence is under `Builds/BasketballGameplayQA/`: APK bytes/hashes, signature, build logs, `SizeAuditBefore/`, `SizeAuditFinal/`, contribution/ZIP comparisons, gameplay captures and portability evidence. See [basketball gameplay](BASKETBALL-GAMEPLAY.md) for controls and tuning.

## Jump motion revision

Measured before/after APK: **87,153,874 → 85,957,194 bytes (−1,196,680 bytes)**. The pre-revision length was read directly before another build replaced that APK; its hash was not captured. Most of this shared-tree reduction comes from the concurrent fishing optimization below. The revised animation uses **37,124 serialized bytes**, approximately **24,231 compressed bytes**, compared with the original jump's 29,292 / 17,367 bytes. It adds no character meshes, textures or external dependencies.

The revised poses coordinate the arm backswing, leg extension at physical takeoff, relaxed flight and landing absorption. The importer now follows the full 61-frame source range. The motor presents an 80 ms grounded load immediately, keeps horizontal input active and shares preparation progress through protocol 11. All **126 Windows checks** passed: 40 jump/room, 24 tackle/room and 62 turning/room checks. Side, front and airborne-reversal motion captures, plus both LODs, were inspected. The fixed-rate capture is an animation review, not a performance benchmark.

Largest contributors remain timber, character maps/mesh/animation, golf structures/terrain and stadiums. The per-asset and ZIP-entry comparison found no unexpected large dependency growth. The signed Android submission build passed; no phone was connected. Phone appearance/FPS and WAN latency remain unverified.

Evidence is under `Builds/JumpNaturalQA/`: final build/signature logs, exact APK bytes/hash, `SizeAuditFinal/`, contribution estimates, portability audit and `jump-review.mp4`. Final jump checks use `Builds/JumpQA/Run-20261001-203848/`; tackle and turn use `Run-20261001-203321/` and `Run-20261001-203333/` in their QA folders. Superseded poses/captures and the temporary portable checkout were archived to `Legacy/20261001-204131-970-jump-motion-revision/`.

## Fishing rod addition

Measured before/after APK: **85,088,760 → 85,955,058 bytes (+866,298 bytes)**. This working-tree delta includes concurrent jump refinements: the jump clip's separate ZIP estimate changed from 17,367 to 22,223 bytes. Rod art totals **981,892 uncompressed serialized bytes** and approximately **852,462 compressed bytes**. Separate asset compression estimates are not exact allocations of shared APK chunks.

The first fishing build was **87,153,874 bytes**. Reducing imported colour maps from 1024 to 512 pixels and normals from 512 to 256, using ASTC 6x6 for both, and applying Low mesh packing reduced the rod estimate from about 2.06 MB to 0.85 MB. The final APK is **1,198,816 bytes smaller** than that first pass, despite concurrent jump changes. Source GLBs, editable masters and full-resolution maps are retained outside Unity Assets. Near LODs total 24,000 triangles and distant LODs 7,000; each rod shares one material across both. The rack reuses existing timber.

All **22 Windows rod checks** passed; close, reverse, guide, distance and pier views were inspected after the final lossy import changes. The reduced maps soften extreme close detail; source reconstruction flaws remain documented in [the fishing report](FISHING-RODS.md). Largest APK contributors remain timber, character maps/mesh/animation, golf structures/terrain and stadiums. The comparison found no unexpected large dependency growth.

Evidence under `Builds/FishingRods/20261001/` includes exact APK measurements/hashes, signature verification, `SizeAuditBefore/`, `SizeAuditFinal/`, asset and ZIP-entry comparisons, portability checks and `Player-Compact/` captures. Android submission and v2 signature passed; no phone was connected. These Windows checks do not qualify Android ASTC appearance or phone performance.

## Jump and shared golf-equipment build

Measured before/after APK: **83,751,052 → 85,088,760 bytes (+1,337,708 bytes)**. The working tree also gained the separate golf-equipment integration, so this combined delta must not be attributed entirely to jumping. The new animation-only FBX shares the existing character rig, mesh and textures: **29,292 uncompressed serialized bytes**, with a **17,367-byte separate DEFLATE estimate**. Golf equipment contributes approximately **1,314,630 bytes** by the same estimator. Runtime/metadata, scene packing and ZIP chunk boundaries account for the remaining difference; per-asset estimates are not exact allocations of the APK.

The first build measured **85,817,216 bytes**. Inspection found world regeneration had reset the previously approved high compression on distant island proxies. Restoring that packing recovered **728,456 actual APK bytes**. The Sky-Sail generator now packs distant silhouettes at High and near cabin/station modules at Low; the Android release path also repairs missing packing. This restores existing delivery settings, retaining the original source geometry and authoring masters. Largest contributors remain timber, character colour/mesh/animation, golf structures/terrain and stadiums. No new jump mesh or texture ships.

All **126 Windows checks** passed: 40 jump/room, 24 tackle/room and 62 turning/room checks, including host/guest action replication, capsule wall/ceiling collision, input reversals at 20/30/60/120 FPS, both animation LODs, and jump controls on all four islands. Both LOD jump/recovery captures were inspected. Signature verification passed after the final Android build; no phone was connected. These checks do not qualify phone frame rate, Android visual quality or WAN latency.

That checkpoint's evidence is in `Builds/JumpQA/`, including before/after bytes/hashes, signed build log, per-asset estimates, portability audit and `Run-20261001-195405/`. Tackle and turn reports use the same run suffix in their respective QA directories. See [jump authoring and controls](JUMP-ANIMATION.md). Superseded fixtures and the temporary portable checkout were archived in `Legacy/20261001-195347-370-jump-validation/`; retained evidence remains under Builds.

## Golf equipment validation

The ball and three clubs use eight closed mesh LODs, four materials and ten maps. Highest-detail geometry totals **15,264 triangles**, dropping to **4,320** at distance. Clubs use 512-pixel colour/normal maps and 256-pixel metal/smoothness masks; the ball uses a 1024×512 dimple normal. ASTC imports, derived meshes and shared rack timber keep original downloads and full-resolution masters outside the APK.

Golf art totals **2,870,024 uncompressed serialized bytes** and approximately **1,314,630 bytes** by separate per-asset DEFLATE estimates, including 596,010 for the ball normal. These estimates cannot exactly allocate shared APK chunks. All **20 equipment checks**, **126 golf-island checks** and the local-room suite passed. Seven actual player views were inspected, and the final generator passed from a checkout with spaces and an unrelated working directory. Oversized irregular dimples, soft face grooves and uneven grip accents remain visible source limitations.

See [the golf equipment report](GOLF-EQUIPMENT.md). Retained evidence under `Builds/GolfEquipment/20261001/` includes a final `SizeAudit/` snapshot, APK measurements, entry deltas, asset estimates, signature, portable dependency checks and actual Windows captures. No phone was connected; Windows views do not qualify Android appearance or performance.

## Basketball addition (earlier checkpoint)

The APK increased from **83,282,808 to 83,751,052 bytes: +468,244 bytes**. The ball uses two mesh LODs (2,976 / 720 triangles), a single shared material and two 1024×512 ASTC 6×6 maps. The original 9.09 MB GLB and high-resolution native source stay outside Unity Assets. Ball art totals **720,260 uncompressed serialized bytes**, which is a different measurement from the APK increase.

Per-asset ZIP estimates are 234,658 bytes for the normal map, 172,805 for colour, 35,436 for the FBX and 701 for the two materials. These estimates total 443,600 bytes; separate DEFLATE estimates cannot exactly allocate the APK's compressed chunks. The balance includes physics proxies, code and packing changes. Largest contributors remain timber, character maps/animations, golf structures/terrain and stadiums. No unexpected large dependency was added. Unity refreshed its Android player resource list, omitting unused SSAO resources while retaining the authoring entries.

All **53 Windows basketball checks** passed, and close/reverse/court-distance views were inspected. Both hoop collisions, 35 m/s floor/backboard shots, spin, streaming and host/guest authority were exercised. No phone was connected; Android appearance, networking and performance remain unqualified. See [the implementation report](BASKETBALL-BALL.md).

Basketball checkpoint evidence: `Builds/BasketballModel/20261001/` (`apk-before.json`, `apk-after.json`, `apk-entry-delta.csv`, `asset-zip-estimates.csv`, `apk-signature.txt`, `adb-devices.txt`). These generated reports stay ignored.

## September optimization history

The 30 September portable-path validation rebuild was **4 bytes larger** than the 29 September APK. Only asset path strings and integrity hashes changed; geometry and texture content were checked unchanged. The earlier mobile asset pass reduced the 350.25 MB cleanup checkpoint by **266.97 MB (76.22%)** before the basketball addition below.

## Measured stages

These are actual APK sizes, not source-folder sizes or uncompressed asset estimates.

| Stage | APK MB |
| --- | ---: |
| Before dependency cleanup | 354.31 |
| Dependency cleanup and lossless terrain-channel packing | 350.25 |
| Android texture settings and mesh packing | 218.19 |
| Shared vegetation and regenerated golf/fishing grass | 128.31 |
| Tighter mesh packing, code stripping and ZIP delivery | 103.03 |
| Shared material patterns | 99.27 |
| Reduced render meshes and texture-alias cleanup | 89.50 |
| Regenerated coastal meadow, compact seating and materials | 83.49 |
| GPU instance batches and final build | **83.28** |
| Basketball model and physics foundation, 1 October | **83.75** |
| Jump and shared golf-equipment working tree, 1 October | **85.09** |
| Fishing rods and concurrent jump refinements, 1 October | **85.96** |
| Revised jump motion and takeoff alignment, 1 October | **85.96** |

An intermediate experiment retained duplicate texture references and was larger. The final material builder updates URP's hidden `_MainTex` alias along with `_BaseMap`, preventing that duplication.

## What changed

- **Vegetation:** 76 coastal palms and all 1,292 palm/plant/tuft placements use nine shared modules and 27 LOD meshes. Near flowers retain their source shape; palm leaflets use fewer longitudinal segments. Explicit GPU instance batches avoid thousands of individual renderer submissions.
- **Grass:** three authored patches are stored once. Golf/fishing blades follow the original analytic terrain; coastal meadows use recorded positions and blade heights. Math runs on a worker and Unity mesh uploads span frames. The room waits for preparation before becoming ready. Tiny blades no longer cast individual shadows; larger objects retain shadows. Complete blades are trimmed at golf bunker boundaries.
- **Buildings:** 387 selected render meshes in delivered coastal scenes use simplified copies. Every source vertex in an accepted simplification was checked against the reduced surface, with a maximum distance of 2.5 cm before Unity packing. This is a source-vertex check, not a bound on every surface point. Meshes shared with colliders retain their original geometry. Another 160 small seating meshes use tighter packing; no seats were removed. High mesh compression is restricted to those seating groups and distant proxies.
- **Textures:** Android uses ASTC and appropriately sized maps. Character colour and cabin timber retain 2K maps; general shared patterns use 1K, normals 512/1K, and masks generally 256. Terrain colour stays 2K; terrain controls retain exact 4K R8 data and mipmaps. Water-depth maps use 2K ASTC 4x4.
- **Materials:** 24 colours use 11 shared patterns and deduplicated normal maps. Tint conversion was compared against every source RGB pixel: maximum error below 2/255 before resizing/GPU compression. Final compressed textures are not claimed to be pixel-identical.
- **Package/code:** Medium managed stripping, IL2CPP size-oriented code generation, ARM64-only delivery and default Android ZIP compression. Code generation and ZIP loading can trade speed for size; device timing remains necessary.

The cabin, doors, hanger, towers and cables were not geometrically simplified by the building pass. Modular authoring structure remains intact. Original FBX/PNG assets and Blender masters remain available; Android substitutes derived delivery assets in build copies of scenes. Source hashes guard against stale model and colour derivatives.

The earlier removal of Resources dependencies, desktop rendering dependencies and retired sea/cloud groups is recorded in [the historical cleanup report](APK-SIZE-CLEANUP-2026-09-29.md).

## Where space remains

The current ZIP contains **59.64 MB** of Unity scene/art/shader data and **20.80 MB** of native libraries. Managed metadata, Android code/resources and archive overhead account for the remaining **5.55 MB**. Exact per-entry values are in `Builds/SizeAudit/latest/apk-entries.csv`.

Large individual art contributors now include 2K timber and character colour maps, golf structures/terrain, character mesh/animation data and stadium geometry. Baked coastal vegetation and repeated grass no longer dominate. Derived building/seating meshes are distributed across many asset entries.

`packed-assets.csv` reports **uncompressed serialized** bytes. For example, two 4K terrain-control maps total about 44.74 MB serialized but roughly 1.8 MB in a per-asset ZIP estimate. These are not RAM measurements and must not be added to the APK size.

`Tools/Build/analyze_packed_apk.py` estimates individual ZIP contributions using packing offsets. Separate per-asset DEFLATE differs from the APK's chunk compression; actual APK length is authoritative. The mesh inventory includes all submeshes of each contributing source FBX, including unused source submeshes. It is not a simultaneous draw count.

## Validation and limits

- Signed Android submission build succeeded below 100,000,000 bytes.
- Runtime terrain math matched **13,122 independent Blender reference samples**, maximum error approximately `1.78e-15` metres.
- The final optimized Windows circuit passed **60 checks**, covering four islands, scene release, docking collision, restored walking, prepared grass and bounded instance submissions. A separate host/guest run passed **18 checks** for synchronized rides, shared destinations and everyone disembarking together.
- All **34 walking-review checks** passed, including every shore-to-deck route and return. The circuit captured 51 screenshots. Close views of gardens, timber, course surfaces, stations and cabin were inspected. Instanced vegetation was visually checked after correcting the manual camera's capture order; sampled coastal views used **89 vegetation draw submissions**.
- The preview uses Android delivery geometry with the Windows renderer and desktop texture formats. It validates geometry, colours and travel logic, **not** Android ASTC appearance, IL2CPP networking, phone RAM/FPS, thermals or transition latency. No phone was connected (`adb devices` was empty).

Local evidence: `Builds/MobileOptimization/terrain-validation.txt`, `VerifiedTravelReview`, `FinalNetworkReview`, `WalkReview`, `InstancedCaptureReview`, logs and APK checkpoints. Builds/reports are excluded from Git. The reference fixture under `Tools/Blender/Fixtures/` supports fresh-clone math checks.

Housekeeping on 29 September retained the final evidence above and moved superseded checkpoints, intermediate logs/reviews and original-setting backups to `Legacy/20260929-201920-001-repository-hygiene/`, preserving their former workspace-relative paths. See [the recovery inventory and repository policy](REPOSITORY-HYGIENE.md). The current APK was not modified.

## Continuing development

1. Run `Tools/Build/Build.ps1 -Target AndroidRelease` after meaningful asset additions. Reports update automatically; the build warns above **75 MB**.
2. Use `-Target AndroidSubmission` for the hard **under-100-MB** check. Passing it does not reserve space for every planned feature.
3. Reuse shared materials, vegetation and prop modules. Keep high-resolution masters and make deliberate mobile exports. Avoid a new 2K/4K texture set for every small prop.
4. Treat **14.00 MB** as one shared reserve for animations, effects, props, audio and fixes. Leave several MB unallocated for integration growth. Reaching 75 MB requires further measured work; no additional saving is promised from untested changes.
5. Qualify the selected minimum phone before claiming stable 30 FPS or raising visual settings.

After editing art, run `build_mobile_vegetation.py` and `build_mobile_models.py` in Blender, `build_shared_mobile_materials.py` with Python/numpy/Pillow, then `Tools/Build/Build.ps1 -Target OptimizeAndroid`. This reapplies import settings and rebuilds the delivery libraries. `-Target MobilePreview` builds optimized geometry into `Builds/WindowsMobilePreview/`; ordinary Windows builds retain authoring scene assets.
