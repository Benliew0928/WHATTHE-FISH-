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
