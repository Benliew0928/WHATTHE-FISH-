# Repository cleanup and publication — 21 October 2026

The reviewed publication includes player profiles/friends and ten original
portraits; Huawei/Google sign-in and native photo support; island bots, venue
customization and settings; personal control layouts for all four sports;
QR screenshot decoding; teammate double-tap passing in Basketball/Football;
and the revised basketball block timing, contact and animation. The current
room protocol is **76**. Historical feature records retain their original
build/test scope.

## Recoverable local cleanup

Archived **16 superseded complete Windows test players** and **41 old standalone
APK copies**: **5,257 files, 11,922,518,024 bytes**. The archive batch is
`Legacy/20261021-201146-963-publish-cleanup/`. Its `archive-manifest.json` records original relative paths,
sizes and SHA-256 hashes; `completed-moves.txt` records the reviewed top-level
moves. Historical reports that reference those copies can recover them from
the same original-relative-path location inside this batch.

The current `Builds/WindowsFinal/` player, `Builds/Android/WhatTheFish-release.apk`,
validation reports/captures, source snapshots, Unity caches and installed
dependencies are retained. No active game asset or authoring master was removed.
The archive and build output remain ignored; archiving moves these files without
claiming to reclaim their disk space.

## Publication checks

- All **548 C# input-file hashes** matched the validated delivery snapshot
  before publication cleanup. The final check found one extra blank line at
  the end of `ProfilePhoto.cs`; only that trailing whitespace was removed.
  The other **547 hashes** remain identical, with no C# behavior change.
- All **326 complete Windows player files** match the promotion manifest.
- All **10 portrait master/delivery pairs** match their integrity hashes;
  original PNG masters and game copies are published through Git LFS.
- The local account/profile service suite passed **36 tests** and its Worker
  deployment dry-run bundled successfully. No live service deployment occurred.
- Native QR sources compiled and **20 image/code policy checks** passed.
- The changed-file credential-pattern scan reported no findings. Local service
  dependencies, Wrangler state and secret files remain ignored.
- The latest gameplay record retains **909 passing checks** and the separate
  shot-motion/charge regressions. Physical Android touch/FPS and real WAN
  latency remain unverified.

The current APK is unchanged at **106,377,892 bytes**, SHA-256
`3A0BFA6F4198AE3D9EE92D99FE287A83FEAD7592864D131EF6C574792D74826B`, with
**893,622,108 bytes** below the strict decimal-GB cap. This task changes
housekeeping, documentation and the whitespace-only correction; the previous
measured release is not a
new Unity rebuild. See [the measured release record](BUILD-SIZE.md).

Current publication evidence is kept locally under `Builds/PublishQA/`; its
`latest.txt` identifies the active run. Pre-commit working-tree readiness and
post-stage portable-path checks inspect the publishable candidate set. Server
API documentation now reflects avatar indices 0–9 and distinguishes its initial
Huawei deployment from the later profile release.
