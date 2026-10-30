# Final optimization and delivery — 30 October 2026

The combined game retains four islands, one active game per host room, shared
travel, the approved artwork and current controls. Network protocol remains
**88**. This delivery includes the previously pending localized onboarding,
narration, responsive welcome, branding and documentation work, and the incoming
awards audio/results commit `a83c6e6`.

## Runtime changes

- Shared Football/Basketball team counts iterate the existing dictionary
  directly. Nested movement, ability, snapshot and HUD queries avoid captured
  LINQ predicates while returning the same counts.
- Football wall navigation uses stack storage and resolves wall liveness once
  per synchronous query. **4,096** seeded results match the original algorithm
  component for component, including blocked starts and expired walls.
- Basketball hoop lookup uses a lifecycle registry with the original active
  hierarchy and disabled-component semantics. Scene unloading releases entries.
- Chat bubble layout returns immediately when there are no bubbles and reuses
  its bounds array. Fishing rod binding iterates the unchanged active roster
  directly.
- Localization skips unchanged labels while retaining immediate font changes,
  locale refreshes, player content and sport-dependent captions. CJK detection
  uses a direct character scan.
- Welcome layout and artwork state update when their inputs change. Percentage
  strings and progress geometry update only when their displayed values change.
  Retry resets the percentage correctly.
- The packaged loader applies the existing game frame cap during preparation:
  60 on desktop and 30 on mobile. Bootstrap retains its normal frame policy.

Paired Windows microbenchmarks, with collection paused while measuring managed
heap growth, are below. A known 64 KiB allocation calibrates the counter. These
measure isolated operations; they do not establish phone FPS or WAN latency.

| Operation | Calls | Previous ms | Optimized ms | Previous heap growth | Optimized |
| --- | ---: | ---: | ---: | ---: | ---: |
| Team count | 10,000 | 2.575 | 0.619 | 2,076,672 bytes | 0 |
| Repeated label assignment | 10,000 | 0.436 | 0.176 | 0 | 0 |
| Wall navigation | 1,000 | 79.517 | 47.080 | 442,368 bytes | 0 |
| Hoop lookup | 1,000 | 25.993 | 0.348 | 45,056 bytes | 0 |

The audit also reviewed scene preparation, cache integrity/retry, network input
dispatch, authoritative state, bot decisions, pooled audio/effects, resource
dependencies and mobile import/build settings. Existing immediate input edges,
network rates, physics, shot calculations, animation timing, grass preparation
and approved rendering settings are retained.

Cold/warm startup measurements use the same desktop, a fresh matching cache for
the cold case, then the same cache for the warm case. Each is one observation,
not a statistical performance guarantee:

| Desktop startup | Opening player | Final player |
| --- | ---: | ---: |
| Cold, including content preparation | 22.497 s | 22.575 s |
| Warm, prepared content cache | 1.688 s | 1.462 s |
| Cold content preparation alone | 20.976 s | 21.127 s |
| Warm content preparation alone | 0.298 s | 0.235 s |

Cold startup is effectively unchanged. First launch still performs lossless
bundle preparation; subsequent launches reuse it. These results do not establish
a substantial startup-speed improvement. Both final runs pass **52** checks
across two circuits of all four islands, with collision and shader/render checks.

## Package size and asset quality

AndroidSubmission: **172,446,376 → 168,026,291 bytes**: **4,420,085 bytes smaller**
(*2.56%*). Remaining headroom below the strict 1,000,000,000-byte limit:
**831,973,709 bytes**.

SHA-256: `D5B87B329DC447F87DC84A33CA300476E752F53F01C34F85EB26A983B13FF92D`.

Three replaced full-background textures were still entering the APK through
Resources. Serialized GUID references, runtime loads, generators and provenance
were reviewed before their delivery copies and metadata were archived. Current
layered scenery, mascot, official logo, native controls and all reference masters
remain available. The localized former background is explicitly retired in its
provenance record; its master hashes remain verified.

The bilingual heading font drops **425,336 source bytes** of optional glyph
names. Every rendering table, including all Unicode mappings, outlines, hints,
metrics and shaping, is byte-identical. The only other change is the required
whole-font checksum. The generator preserves this format. All five faces retain
their original Latin outlines/metrics and every supported Chinese codepoint.
No new lossy texture, mesh, animation or audio conversion was introduced.

The APK passes ZIP CRC, all **1,538** exact build-audit entries, five-scene bundle
hash/length, v2 signature, 16 KiB alignment and ARM64 checks. Package/version is
`com.umpsa.whatthefish`, `0.3.0` / `1`, minimum SDK 26, target SDK 36. It is a
non-development build signed with the existing **Android Debug test certificate**.
The production-signed AppGallery package and private release key are preserved;
this test APK is not a store update signed with that production certificate.

Largest compressed entries: islands **45,955,307**, IL2CPP **13,345,403**, Vivox
**10,240,163**, Unity **9,828,173** bytes. The scene bundle changes by -3,735;
IL2CPP grows by 7,858 with the combined runtime; Vivox and Unity are unchanged.
Removed background entries and optional font metadata account for the reduction.

## Verification

- **545 Windows / 390 Android** compiled project-source documents match their
  current source. Development review classes are absent from APK metadata.
- **53** optimization checks include allocation calibration, all three languages,
  immediate font reassignment, raw player names, context-sensitive captions,
  navigation equivalence, hoop selection/lifecycle and loading retry state.
- **25,482** responsive/first-visit checks cover the affected screens in English,
  Simplified Chinese and Malay; **3,541** additional localization checks cover
  current menus, all four gameplay HUDs, tutorials, settings and artwork.
- **3,682** catalog entries and format tokens pass; all five font faces and the
  retained artwork provenance/alignment pass. Representative renders were reviewed.
- Bot suites: **94 Football / 155 Basketball** assertions; Basketball gameplay
  **98**; default-view Fishing gameplay **2,657**; Fishing feedback/results **189**;
  current Golf drag input **36**; first-person rendering **505 Golf / 483 Fishing**;
  awards/audio/local result visibility **141** across both games and all languages.
- Four-island lighting: **102** checks. Lifecycle/shutdown: **68** checks across
  Tour, Fishing, Golf, Basketball and teleport cancellation.
- Cold/warm packaging **104** checks and blocked-directory/copy-source recovery
  **61** checks pass. Retry completes preparation and preserves unrelated cache
  files; obsolete caches are removed only after successful loading.
- Three real loopback peers pass movement/action checks through **60 ms delay
  each way, ±8 ms jitter and 2% injected loss** after joining. This is a controlled
  local network test, not an internet/Relay or phone measurement.
- Two connected players pass **293 host / 241 guest** assertions across all four
  island destinations, stations, cabin travel, results/rematches, blocked landings,
  cancellation boundaries and departure recovery. The host remains in its room
  after the departing guest is removed.
- Account-server **85** tests and Worker dry-run pass. No server deployment or
  store submission was performed by this task.
- Minimal fixtures verify script-root resolution, a different path containing
  spaces and an unrelated working directory. Publishable text and the staged
  blobs receive the repository portability checks before commit.

Current compact evidence is under `Builds/FinalOptimizationQA/`; the current APK
inventory is under `Builds/SizeAudit/latest/`. The native Windows executable,
Unity runtime, serialized player data, catalog and scene bundle retain identical
hashes through code-only review refreshes. The regenerated Golf ball prefab has
14 graph-equivalent objects and unchanged properties/external references.
`verified-regressions.json` identifies **32 passing cases / 34,303 assertions**
with hashes of their result files. Server tests, the localization/font/artwork
audits and the separate injected-latency run are additional to that count.

### Existing limits and corrected test assumptions

The full Fishing diagnostic fails
`AIM_SIX_TARGETS_PIER_0_CAMERA_2_SLOT_0` in both the pre-task and optimized players.
The manual Island View camera and its pitch/reticle behavior are preserved.
`FishingFull` remains available in the final runner; normal regression uses the
Eye-level/Follow scope explicitly. This does not qualify the failing Island View
case as passing.

Several older fixtures assumed the retired Cove court skin, cycling Golf power,
collapsed head bones, the former eye position, immediate island readiness, or
numeric Golf hole order. Current dedicated reviews exercise drag input and the
approved shader head mask. Updated test expectations retain the face offset and
bone scale. Cancellation pose restoration is checked synchronously, with control
availability checked again after the next physics frame. Golf network fixtures
use the current shuffled route and honor the start countdown.
Disconnect checks wait for asynchronous player removal. The blocked cache-directory
fixture checks the current retry guidance; it does not diagnose a full disk from
a file/directory collision. These are review-only changes, excluded from Android.

No physical Android device was connected. Phone appearance, touch, memory peaks,
speaker balance, battery use and FPS remain unverified. The source and Windows
checks do not certify those device properties.

## Folder cleanup and handoff

The opening audit recorded **8,930 publishable files / 3,580,321,051 logical
bytes** and hashes of all current source and deliveries. It used a source
manifest, one reused candidate player, and compatible shared content caches.

Archive batches:

- `Legacy/20261030-135556-190-final-optimization-unused-welcome/`: six obsolete
  delivery files; all retained masters remain in Git.
- `Legacy/20261030-151131-288-final-optimization-android/`: designated previous
  APK, mapping and complete current size audit, **19 files**.
- `Legacy/20261030-161131-427-final-optimization-duplicate-pitch/`: one
  **45,130,125-byte** root presentation duplicate. Its SHA-256 equals the retained,
  linked editable deck in `Docs/PitchDeck/`; no unique slide content was removed.
- `Legacy/20261030-161236-256-final-optimization-windows/`: designated previous
  complete **342-file / 663,677,819-byte** Windows player. Every opening file still
  matched before promotion, preventing replacement of a newer concurrent build.

All **368 archived payload files / 904,451,852 bytes** were verified against their
archive manifests after moving. The archive tool now accepts explicitly reviewed
individual root-level office/PDF documents as well as its existing folder scopes;
it still rejects repository-root, protected-folder and link/junction moves.

Archival preserves recovery and does not free SSD space. Main Unity caches,
Git/LFS data, authoring masters, pre-existing installed dependencies, the paused
character workspace and existing archives remain in place. Current release and
validation output stays ignored under Builds.

The complete final Windows player is **342 files / 649,582,418 bytes**, promoted to
`Builds/WindowsFinal/`. Missing portable preview launchers from the prior player
were retained. Every promoted file matches the tested candidate; native and
serialized payload hashes are unchanged through the review-only script refreshes.
`LATEST-BUILD.txt` records build time, included work, evidence and source limits.

The requested teammate ZIP is
`Builds/Deliveries/WhatTheFish-Windows-2026-10-30.zip`: **512,145,392 bytes**.
SHA-256: `2050343EB0ED063627DEC3990BA0B50170684367EC87B85C0E9C0626A4455851`.
ZIP CRC, the complete entry set, every uncompressed size and every SHA-256 match
the promoted player. Extract the complete `WhatTheFish-Windows/` folder before
launching `WhatTheFish.exe` or the included launchers.

After promotion and ZIP verification, **946 task-created files /
2,573,319,975 logical bytes** of reproducible candidates, isolated caches, the
minimal portability fixture, dry-run output and repeated/intermediate captures
were removed. `cleanup-result.json` records each exact target and reason. Targets
were resolved inside the task's ignored Builds folder; links/junctions and running
dependencies were rejected before removal. The designated rollbacks, current
deliveries, all result logs/timings and **175** representative screenshots remain.
The final QA folder is approximately **177 MB**; the requested ZIP intentionally
duplicates the current player in compressed form. Logical removed bytes are not
a measured SSD-allocation saving.

Intentionally retained pre-existing large folders include the paused character
authoring workspace (about 16.68 GB), main Unity Library (14.14 GB), Git/LFS data
(7.29 GB), AppGallery device-test dependencies (7.16 GB), older RoomChat test
dependencies (2.64 GB) and Football training environment (1.37 GB). They are
authoring sources, caches or installed dependencies rather than reviewed obsolete
deliveries; this task does not delete them or existing recovery archives.
