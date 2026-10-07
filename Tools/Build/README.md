# Build and verification tools

Run examples from the repository root. Scripts resolve the project from their own location; alternate Unity installations use `-UnityEditorPath` or the editor-root argument documented in [setup](../../Docs/SETUP.md). Unity 6000.3.20f1 is required. Build output and raw evidence go under ignored `Builds/`.

| Task | Entry point |
| --- | --- |
| Complete Windows player | `./Tools/Build/Build.ps1 -Target Windows` |
| Self-contained Android delivery with APK budget gate | `./Tools/Build/Build.ps1 -Target AndroidSubmission` |
| Optional development APK | `./Tools/Build/Build.ps1 -Target Android` |
| Regenerate the integrated scene | `./Tools/Build/Build.ps1 -Target Scene` |
| Read-only commit candidate checks | `./Tools/Build/Check-TaskReady.ps1` (add `-RequireApk` for Android delivery) |
| Exact staged text portability, after the user stages | `./Tools/Build/Check-PortablePaths.ps1 -Staged` |
| Archive an explicit reviewed path list with recovery hashes | `Archive-LocalArtifacts.ps1` — [workflow](../../Docs/REPOSITORY-HYGIENE.md#archive-and-restore) |

`Install-Android.ps1` and `Repair-CMake.ps1` support the editor's Android toolchain. `Prepare-CoveFonts.py` and `Prepare-CartLabels.py` regenerate delivery assets; `analyze_packed_apk.py` inspects APK packing. Keep these with the source project.

The `Test-*.ps1` scripts are reusable feature and networking checks. Use a matching player and the script's parameters; local tests exercise host/guest authority without Relay, while cloud tests use real service quota. The passing, trajectory and stadium-camera checks remain available for the latest gameplay. See [verification](../../Docs/VERIFICATION.md) for feature coverage and device limits.

`Review-*.ps1`, `Benchmark-RefinedIslands.ps1`, the gallery builders and walkthrough encoders reproduce curated visual evidence. `check_basketball_evidence.py` checks room movement reports. These tools remain useful even though they are not part of the runtime. Art generators and audits live in [Tools/Blender](../Blender), with authoring instructions alongside the masters in `ArtSource/`.

Completed coastal/island migration inventory, special archive and historical report-writing scripts were retired on 7 October 2026. Their hash-verified local recovery batch is recorded in [repository hygiene](../../Docs/REPOSITORY-HYGIENE.md#cleanup-completed-on-7-october-2026). The general archive tool replaces those one-time operations. Current sources, package locks, tests, fixtures, selected references and Unity metadata remain versioned.
