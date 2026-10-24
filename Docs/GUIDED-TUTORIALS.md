# Guided sport tutorials

For the matching four-page first-visit rules cards and How to Play reference,
see [Illustrated island rules](ISLAND-RULES.md). They reuse Fishing's shared
navigation and animated examples without replacing these hands-on courses.

The main menu has a **Tutorials** entry with separate Football, Basketball,
Golf and Fishing courses. Each course loads its existing island and live HUD
in private offline practice. A room must be left before starting a tutorial.

Lessons introduce controls progressively. The current control is highlighted;
previously introduced controls remain available. Hidden controls also reject
unrevealed gameplay input. The tutorial uses each player's saved HUD layout
and the sport's existing illustrations, gestures and contextual labels.

Each lesson is completed by an observed gameplay action. **Retry** resets the
exercise, **Skip** leaves it incomplete, and **Lessons** lets players revisit
individual exercises. **Quit** and Escape return to the tutorial selection.
Opening the lesson list stops input; returning resets the current exercise.
Progress is stored locally per profile and sport using stable lesson IDs.

| Sport | Basic operations |
| --- | --- |
| All | Forward, backward, sideways movement and looking |
| Basketball | Jump, collect, aim/shoot/cancel, tap pass, select receiver, collect/use prop, guard, steal and block stance |
| Football | Dribble, aim/shoot/cancel, tap pass, select receiver, dash/recover, pressure, tackle, collect/use prop |
| Golf | Jump, start, aim, power/swing, cancel, putt, summon/drive/steer/exit/recall buggy |
| Fishing | Jump, pier, choose skill, cast, hook, countersteer, release tension, use skill, land fish and cancel |

Golf events are suppressed only during the tutorial. No tutorial controls,
practice actors or simulation overrides are enabled in ordinary games or
network rooms. The classroom helper methods require an active private tutorial.

Implementation: `TutorialCourse` defines stable steps; `TutorialSession` owns
routing, scoped practice setup and completion; `TutorialView` reveals and
highlights the actual HUD. `TutorialPractice` provides guarded exercise setup.

## Tutorial feedback and mobile wording

The coach card now keeps an explicit **TUTORIAL** label, sport, step number and
segmented progress visible throughout practice. Completed lessons fill their
segments; skipping never earns a completion segment. The current native control
gets a gently pulsing outline, with motion disabled by Gentler motion.

Each verified action shows a mint success card with a large checkmark,
**Nice work!**, the completed action and the next lesson. It remains for
2.2 seconds before advancing. A short ascending chime plays once per success;
it respects UI and master volume, application pause and focus. It is a chime,
not recorded speech. Retry and skip do not play it. Quit remains available
during feedback, and a fully learned course has a distinct completion title.

Visible control instructions use touch language on every platform, including
the Windows testing player. The audit covers tutorials, shared button captions,
main-menu control guides, sport settings/guides, kickoff, travel, and the
development teleport preview. Keyboard/mouse bindings remain operational.
Desktop-specific diagnostic strings, editor tooling, input-axis identifiers
and third-party legal notices are retained because they are not player control
instructions. The unsupported-account message now uses neutral device wording.
Existing tutorial/hint artwork was inspected; no replacement images were needed.

For isolated tutorial builds, `WTF_TUTORIAL_PLAYER` optionally overrides the
output used by `TutorialBuild.Build` and `TutorialBuild.Scripts`. Relative
overrides resolve from the Unity project directory; the default remains
`../Builds/TutorialQA/Player/WhatTheFish.exe`. Scripts-only builds require the
complete player and the matching prior packaged assets.

## Feedback verification — 23 October 2026

The feedback refresh passed **602 runtime assertions across 58 lessons**:
Basketball 155 assertions / 15 lessons, Football 147 / 14, Golf 155 / 15 and
Fishing 145 / 14. This corrects the earlier manually reported total of 59;
the course contents have not changed.

Checks cover visible mobile wording across ordinary HUDs, settings, layouts,
main-menu and sport guides; every lesson's success message and single chime;
no celebration on skip; readable success dwell time; UI/master mute; reduced
motion; coach text clipping; saved progress and quit cleanup. Rendered success,
progress and guide screens were reviewed. Phone touch feel, speaker loudness
and device performance remain unverified.

Evidence: `Builds/TutorialFeedbackQA/tutorial-results.json`,
`mobile-copy-audit.txt`, `source-final.json`, `scripts-3.log`, and these final
run folders under `Builds/TutorialQA/`:

- `Run-20261023-141218-856-41961d/` — Basketball
- `Run-20261023-141218-865-f70b48/` — Football
- `Run-20261023-141218-876-14c9a8/` — Golf
- `Run-20261023-141218-885-5c17f0/` — Fishing

The validated complete candidate is `Builds/TutorialFeedbackQA/Player/`.
The combined release is delivered in `Builds/WindowsFinal/`. The official
AndroidSubmission build completed at 14:20 Malaysia time: **110,706,458 →
110,718,422 bytes**, an increase of **11,964 bytes**, leaving **889,281,578
bytes** below the strict 1,000,000,000-byte cap. The largest compressed entries
remain the island bundle, IL2CPP, Unity and metadata; no new raster/audio assets
or unexpected large contributor were added. See [the release audit](BUILD-SIZE.md).
The Windows payload is the tested scripts-3 snapshot; subsequent unrelated
developer filming-tool edits are outside that snapshot and are excluded from
release Android. Gameplay/tutorial sources match the validated candidate.
`Check-TaskReady.ps1 -RequireApk` passed after delivery: 7,836 publishable files
and 1,018 LFS paths inspected. The final Windows player was reopened and
responded normally. Final readiness, APK measurement and reopen evidence are
retained in `Builds/TutorialFeedbackQA/`. No Git files were staged or committed.

Superseded feedback runs were archived with recovery hashes under
`Legacy/20261023-141407-120-tutorial-feedback-iterations/`; interim build logs,
manifests and the one-time edit helper are in
`Legacy/20261023-141551-870-tutorial-feedback-build-iterations/`.

## Initial tutorial verification

Evidence paths below are relative to the repository root.

Run `Tools/Build/Test-Tutorial.ps1 -PlayerPath <development-player>` to exercise
the four courses. The runner uses disposable guest profiles, records action
assertions and screenshots under ignored `Builds/TutorialQA/`, and checks
profile isolation, progressive controls, chapter navigation and active-gesture
cleanup on exit. This is Windows runtime verification; phone touch feel and
frame rate still need Android device testing.

The combined Windows candidate passed **201 tutorial assertions** on 23 October
2026: Basketball 51, Football 48, Golf 53 and Fishing 49. All 58 lesson actions
completed. Skipping leaves a lesson incomplete; retry, resume, per-profile
isolation and quitting during active gestures also passed. Captures were
reviewed for live button artwork, legible titles and unobstructed gameplay.
Current detailed evidence is in `Builds/TutorialQA/Run-20261023-131827-993/`
and `Builds/TutorialQA/Run-20261023-131827-994/`. The successful script build is
`Builds/TutorialQA/scripts-4.log`; the complete candidate is retained alongside
its source fingerprint in `Builds/TutorialQA/`.

The official combined AndroidSubmission build succeeded at 13:23 Malaysia time.
The APK is **110,706,458 bytes**, compared with **110,643,406** at tutorial task
opening and **110,666,082** immediately before the combined build. Headroom is
**889,293,542 bytes**. ZIP integrity, all 1,212 audit entries, the five-scene
bundle/catalog hash and APK v2 signature passed. See [release size and audit](BUILD-SIZE.md).
The complete tested Windows payload is delivered in `Builds/WindowsFinal/`.

The tutorial runner also passed 53 Golf assertions using its default player
path from `Builds/TutorialQA/Portable Check/`, while launched from an unrelated
working directory. Its folder name contains spaces. Final readiness with
`-RequireApk` passed. No files were staged or committed.

Earlier QA captures were archived with the repository tool to
`Legacy/20261023-132034-611-tutorial-qa-iterations/`. The shared delivery archived
the superseded APK/audit and Windows player under
`Legacy/20261023-132008-386-predictive-pass-tutorial-android/` and
`Legacy/20261023-132255-097-predictive-pass-tutorial-windows/` respectively.
The official builder regenerated Golf Ball prefab local object IDs; review
confirmed the same LOD mesh assignments, materials, transforms and collider.
No new art or lossy asset conversion is part of this tutorial change.
The disposable portable-check player was archived after verification to
`Legacy/20261023-132733-234-tutorial-portable-player/`; its test evidence remains
under `Builds/TutorialQA/Portable Check/Builds/TutorialQA/`.
