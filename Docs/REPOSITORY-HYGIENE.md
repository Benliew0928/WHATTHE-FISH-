# APK budget and commit handoff

The root [AGENTS.md](../AGENTS.md) carries the standing instructions for new agent tasks: keep the APK strictly below **100,000,000 bytes**, pursue the **75,000,000-byte working target**, preserve visual quality and leave publishing to the user. Codex discovers repository instructions through [AGENTS.md](https://learn.chatgpt.com/docs/agent-configuration/agents-md). Other agent tools must also be configured to read that file if they do not support it automatically.

## What belongs in Git

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
git diff --cached --stat
git commit -m "Describe the completed task"
git push
```

Agents leave these publishing steps to the user unless explicitly asked to perform them. The pending optimization contains legitimate asset moves from Resources into Prefabs/Settings; matching `.meta` files preserve their GUIDs.

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
