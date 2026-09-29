# Instructions for every task in this repository

## APK budget is a product requirement

- The self-contained hackathon APK must be **strictly below 100,000,000 bytes** (100 decimal MB, not 100 MiB). Never call an oversized build submission-ready.
- Aim for **75,000,000 bytes or less during development**, reserving space for gameplay, animation, effects, props and audio. Being under 100 MB is not a reason to spend the remaining reserve carelessly. The current baseline is in `Docs/BUILD-SIZE.md`; do not assume the working target has already been achieved.
- Read `Docs/BUILD-SIZE.md` and, when changing assets/rendering/build settings, `Docs/APK-SIZE-AUDIT.md`. Record the existing APK bytes before changes. Measure the resulting APK itself; source-folder sizes and uncompressed Unity reports are different measurements.
- For changes that can affect the delivered game, run `Tools/Build/Build.ps1 -Target AndroidSubmission` after the appropriate feature checks. It builds and enforces the hard limit. Inspect `Builds/SizeAudit/latest/` for the largest contributors and unexpected growth. Report before/after bytes and remaining headroom, and update the size documentation after a new measured release. If the build cannot run, state that size is unverified; do not present an older APK as validation of new code. Documentation/housekeeping-only changes do not require rebuilding Unity.
- Optimize continuously. Prefer removing unused shipped dependencies, sharing meshes/materials/textures, instancing, appropriate LODs, mobile import formats, and deliberate audio/animation compression. Investigate and reduce avoidable growth within the task; do not defer all optimization to the end or accept 90–95 MB as a comfortable development target.
- Preserve the intended visual quality: cabin interiors, close props, island silhouettes, animation and travel should remain attractive. Keep editable high-quality masters; derive mobile delivery assets. Check affected close/distant views and gameplay after lossy changes. Never claim phone quality/FPS from a Windows preview alone.
- Preserve the current world model: four islands, one active game per host room, everyone travels together. Keep cabin, cables, towers and stations modular.

## Repository hygiene after every task

- Leave the workspace ready for the user to run `git add .`, commit and push themselves. **Do not stage, commit, push, reset, or rewrite history unless explicitly requested.** Preserve unrelated changes already in the workspace.
- Keep active source, Unity assets and their `.meta` files, package locks, generators, fixtures, current documentation and useful approved design references in Git. Generated delivery assets used by the build also belong in Git unless the normal fresh-clone build reliably regenerates them.
- Put temporary build output, raw logs, captures and audit data under ignored `Builds/`. Keep the current APK/player and current validation evidence there. Never place archives or experiments under `Game/Assets/Resources` or `StreamingAssets`.
- Move confirmed obsolete assets, superseded output, discarded drafts and backups to **`Legacy/<dated-task-batch>/<original-relative-path>`**, outside the Unity project. `Legacy/` stays ignored. Use `Tools/Build/Archive-LocalArtifacts.ps1` with an explicit list of reviewed paths. It records sizes, SHA-256 hashes and recovery paths before moving. Do not overwrite earlier archives.
- Prove a game asset is unused before archiving it: check scene/prefab/material references, string-based runtime loads, editor/build generators and source provenance. Absence from a packed-assets report alone is insufficient. Move Unity assets with their `.meta` files and fix active references. Keep Unity caches and installed dependencies in place; ignoring them is sufficient.
- Update `.gitignore` for newly introduced local-only output. Ignore rules do not remove files already tracked. Check the actual candidate set; do not blanket-ignore active art or remove authoring masters just to make Git smaller. Existing Git history is not rewritten by cleanup.
- Use Git LFS for the binary types in `.gitattributes`. A clean clone with LFS must retain enough files to build and edit the game. Keep secrets/signing keys outside commits.

## Before handing the task back

1. Run checks appropriate to the change and fix actionable failures. Check the APK budget when the game changed.
2. Archive obsolete files created by the task; keep current evidence and update affected documentation/links.
3. Run `Tools/Build/Check-TaskReady.ps1` (add `-RequireApk` when delivering an Android build). Review `git diff --stat` and `git status --short`. The script is read-only; it does not certify APK freshness or replace functional/device testing.
4. State what changed, verification, measured APK size/headroom when relevant, and any unresolved limits. Mention the archive batch if files moved. Leave all Git publishing actions to the user.
