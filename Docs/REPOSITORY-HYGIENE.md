# APK budget and commit handoff

The root [AGENTS.md](../AGENTS.md) carries the standing instructions for new agent tasks: keep the APK strictly below **100,000,000 bytes**, pursue the **75,000,000-byte working target**, preserve visual quality and leave publishing to the user. Codex discovers repository instructions through [AGENTS.md](https://learn.chatgpt.com/docs/agent-configuration/agents-md). Other agent tools must also be configured to read that file if they do not support it automatically.

## What belongs in Git

### Verified local compression — 4 October 2026

The disk-space request used standard NTFS lossless compression of reviewed
existing files. SHA-256 checks prove the contents are unchanged: archived
artifacts saved **1,501,461,838 bytes**, the closed historical Football control
cache **3,037,340,817**, the closed goal-net cache **3,089,851,766**, and the
closed Swing validation cache **73,856,948**. Total measured allocation saving:
**7,702,511,369 bytes (7.70 decimal GB)**. Caches remain in place; directory
defaults are unchanged for the cache operations. Current player/APK, source
art and installed tools remain available. Evidence and per-file hashes are in
ignored `Builds/DiskSpaceQA/`; `final-result.json` records the measured saving
and a free-space snapshot. Simultaneous builds and deliveries change total
free disk space independently of these verified per-file savings.


Keep runtime/editor code, active scenes and prefabs, Unity asset metadata, project settings, package locks, mobile delivery assets, source masters, generators, fixtures, current documentation and useful design references. Large source models and curated image/video references use the existing Git LFS rules. They are not the same as APK contents.

Build packages, caches, downloaded installers, raw logs, temporary scripts, obsolete build checkpoints, unused drafts and Blender backup versions do not belong in a normal commit. Current output stays in ignored `Builds/`; confirmed superseded files go to an ignored dated batch under `Legacy/`. Unity caches stay in place and stay ignored. Do not move an active dependency merely because it is generated or absent from one build report.

Moving files into `Legacy/` preserves local recovery, but does not free disk space, shrink existing Git history, or provide a remote backup. Keep important original source files versioned with LFS. A tracked file moved to Legacy appears as a deletion in the next commit; that is intentional for a confirmed discarded file. Existing tracked files are not excluded simply by adding an ignore rule.

## At the end of a task

For delivered game changes, build and measure:

```powershell
./Tools/Build/Build.ps1 -Target AndroidSubmission
./Tools/Build/Check-TaskReady.ps1 -RequireApk
```

For documentation or housekeeping changes, run only:

```powershell
./Tools/Build/Check-TaskReady.ps1
```

The read-only check examines the files a root-level `git add .` would include. It checks whitespace/conflict markers, unresolved merges, tracked ignored files, local artifacts, missing asset metadata, large files without LFS and the existing APK size. It warns above 75 MB and fails at 100 MB. It does not build, install hooks, stage files, verify APK freshness/signing, scan arbitrary file contents for secrets, or replace gameplay/device tests.

After reviewing the changes, the user runs:

```powershell
git add .
./Tools/Build/Check-PortablePaths.ps1 -Staged
git diff --cached --stat
git commit -m "Describe the completed task"
git push
```

Agents leave these publishing steps to the user unless explicitly asked to perform them. The pending optimization contains legitimate asset moves from Resources into Prefabs/Settings; matching `.meta` files preserve their GUIDs.

## Windows player handoff

After every gameplay or asset delivery, update the complete validated player in `Builds/WindowsFinal/`, including `WhatTheFish.exe`, its `_Data` directory, runtime DLLs, supporting folders and launchers. The regular Windows builder already uses this destination. If an isolated checkout was used, promote its tested package, archive the superseded player with the tool below and compare the copied files with the tested source package. Do not overwrite a newer concurrent delivery with an older snapshot.

Keep `LATEST-BUILD.txt` beside the executable with its build time, included feature changes, verification evidence and any ongoing changes outside that snapshot. Task-specific players and reports may remain under ignored Builds for evidence; `WindowsFinal` is the user's normal launch location. Documentation-only changes do not require rebuilding a player.

## Archive and restore

Review dependencies and the exact path list first, then archive only confirmed obsolete files:

```powershell
./Tools/Build/Archive-LocalArtifacts.ps1 `
  -RelativePath @('Builds/obsolete-review', 'ArtSource/example.blend1') `
  -Reason 'Superseded by the validated delivery; no active references' `
  -Label 'example-cleanup'
```

The script validates workspace containment and rejects links/junctions and protected cache/Git paths. It writes an inventory of original paths, lengths and SHA-256 hashes before moving anything, preserves the directory layout and records completed moves. It does not infer whether assets are unused. Include Unity `.meta` files with any asset moves.

To restore, find the original path in `Legacy/<batch>/archive-manifest.json`, verify the archived hash, and copy it back to its original workspace-relative path. Compare newer destination content first. Restore Unity assets and metadata together. Each batch includes recovery instructions; do not overwrite earlier batches.

## Cleanup completed on 29 September 2026

Batch: `Legacy/20260929-201920-001-repository-hygiene/`.

- Archived **3,140 files / 4,805,685,377 bytes (4.81 GB)**: old QA runs, superseded optimization APK checkpoints and previews, raw review logs, one Blender backup, cached tool installers, two discarded concept drafts and raw generation-job metadata.
- Only **three of those files were versioned**: the two `G1_01_Overview_draft.png` / `G2_01_Overview_draft.png` images and `generation-jobs.json` in the golf/fishing concept folder. They total 5,992,693 bytes and are now pending deletions. The other archived files were already ignored; this cleanup does not claim a 4.81 GB Git or disk-space saving.
- Retained the current Android APK, Windows players, `SizeAudit/latest/`, and final `VerifiedTravelReview`, `FinalNetworkReview`, `WalkReview` and `InstancedCaptureReview` evidence. Active source art, delivery assets, selected concepts and curated galleries remain versioned.
- All archive paths and lengths were checked; the three formerly versioned files were hash-verified after the move. The APK SHA-256 and Git index remained unchanged. Protected/out-of-workspace archive paths were rejected in validation. The commit-readiness check passed with the expected warning above the 75 MB working target.

Historical reports can mention output at its former `Builds/` path. Look up that original relative path inside this archive batch. Newly run tests can recreate the usual active output paths. The optimization result remains **83,282,804 bytes**; this housekeeping-only task did not change the game or rebuild it.

## Cleanup completed on 2 October 2026

Batch: `Legacy/20261002-162115-398-reviewed-qa-cleanup/`.

Archived **24 ignored generated files / 3,825,309 bytes (3.83 MB)** across nine reviewed paths. These were the preliminary jump run `Run-20261001-194729`, the intermediate basketball foundation run `Local-215557`, the September 29 AndroidRelease log, a stale September 25 Android build marker, and five obsolete Git/readiness snapshots. The two test runs have the same passing assertion names as their retained final replacements. No source, script, documentation or saved-summary consumer of the selected outputs was found.

The archive preserves the original directory structure. Its portable `archive-manifest.json` records every original path, archived path, byte count, SHA-256, reason and replacement. All **24 archived files** and all **3,382 retained pre-existing Builds files** were hash-verified; the **42 pre-existing modified/untracked publishable files** remained byte-identical. Relocation freed no disk space and changed no Git history.

Retained the current Android APK and complete Windows player, `SizeAudit/latest/` and `latest-apk.txt`, all documented feature evidence, the merged 367-check reports, motion frames/videos, and the separate mobile, fishing-rod and golf-equipment previews. The pre-merge stash and `Builds/MergeFootball-20261002-155106/` recovery backup remain intact. Reusable test scripts, terrain fixtures, source masters, active assets and Unity/tool caches were left in place. Uncertain local inspection/library helpers and unique dependency/LFS audit records were retained.

The existing APK remained **86,498,912 bytes**, with **13,501,088 bytes** to the strict 100,000,000-byte ceiling and **11,498,912 bytes above** the development target. SHA-256 remained `6EB19CFA79CF55571963DA49DFE687B6402E6F7A675A375B9C1503C9D0F91418`. This housekeeping task did not rebuild or rerun gameplay tests; the retained merged reports contain 367 passing checks. See [the current build record](BUILD-SIZE.md) for the release's verification limits.

The detailed keep/archive inventory, dependency checks, hash verification and readiness output are local evidence under `Builds/Housekeeping/20261002T081405Z-cleanup/`. No Git staging, commit, push, reset or merge was performed.

## Cleanup completed on 7 October 2026

Batch: `Legacy/20261007-190844-392-teammate-handoff/`.

Archived **13 files / 83,495 bytes**. Seven tracked tools were completed, one-time migration helpers: `Archive-CoastalLegacy.ps1`, `Archive-LegacyAssets.ps1`, `Archive-RefinedIslandLegacy.ps1`, `inventory_coastal_legacy.py`, `prepare_island_archive.py`, `finalize_coastal_review.py` and `write_refined_island_report.py`. Their retired source/manifests and historical build prerequisites were checked; no active build caller uses them. The general archive tool remains available. The other six files were ignored, task-specific report/preview helpers from the basketball passing, fishing presentation, menu and broadcast-camera deliveries. Their finalized reports, validation records and media remain available.

Every archived file was verified against its recorded byte count and SHA-256 after relocation. All pre-existing gameplay, assets, authoring masters, package locks, useful references, reusable tests/generators, dependency audits and Unity metadata remain intact. The 61 pre-existing modified/untracked feature files were preserved byte-for-byte. The current Android and complete Windows delivery, including its build note, also remain byte-identical. Unity and installed-tool caches stay in place. Archival changes no Git history and frees no disk space.

Updated the root README and setup guide for protocol **32**, current passing/shot-odds/camera behavior, Git LFS setup and the AndroidSubmission budget gate. [Build tool navigation](../Tools/Build/README.md) explains the retained build, test, audit and review scripts. All local Markdown links in README, Docs and ArtSource resolve. All **481** LFS files are hydrated and match their recorded SHA-256 and sizes; this verifies local content, not a fresh remote clone or opaque asset dependency paths.

After fetching `origin`, `main` and `origin/main` both identify `c167c4c`, with **zero commits ahead or behind**. The pending basketball/camera work and this cleanup are ready for user review and staging; the index remains untouched. The read-only readiness check and working-file portability check pass. After staging, the user must still run `Check-PortablePaths.ps1 -Staged` against the exact indexed content before committing.

This housekeeping task does not rebuild or retest the game. The retained measured APK is unchanged: **89,864,908 → 89,864,908 bytes**, leaving **10,135,092 bytes** below the strict ceiling and remaining **14,864,908 bytes above** the development target. Its hash is `EC2D59AAC704AC726A83D3D02DAF12AAB80C52968C7A1D9979410013C06FF00E`. [The latest build record](BUILD-SIZE.md#perspective-broadcast-cameras--7-october-2026) describes gameplay validation and the remaining phone-testing limits.

Detailed inventories, baseline hashes, archive verification, LFS checks, preservation checks and Git/readiness snapshots are local evidence under `Builds/Housekeeping/20261007-teammate-handoff/`. No staging, commit, push, reset or history rewrite was performed.
