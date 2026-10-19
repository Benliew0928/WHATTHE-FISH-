# Quieter sports audio and one-second Lock-on — 18 October 2026

Basketball and Football now play walking/running sounds only for the local
player. Remote players' shoe cuts, shuffles, jump/landing, gather and dash
movement accents are also silent where those cues exist. Ball contacts and
successful defensive actions still provide shared feedback.

Playback gain relative to the previous mix:

| Sound | Basketball | Football |
| --- | ---: | ---: |
| Local walk/run | 28% | 28% |
| Shoe cuts/shuffles/fabric | 22% | 22% |
| Landing/body/reach and related movement accents | 35% | 35% |
| Floor/board/rim or turf/post/deflection/boundary/Wall impacts | 40% | 40% |
| Hand/catch/release/block or dribble/receive/kicks, plus rolling | 45% | 45% |

The saved SFX and Master controls still apply. Important match, prop, score,
menu and ambience levels retain their existing mix. Gain reductions happen
once during source playback, including Football replay; recorded/network event
gains and authority are unchanged. No audio files, authoring masters or visual
assets were reduced or removed.

Basketball Lock-on spends its charge and plants the player for **one second**,
then releases the assisted shot. The authoritative deadline, HUD progress and
ground halo use `BasketballPropRules.AimSeconds`. The guide says one second.
The quiet aim sound plays the final second of its approved two-second source
at the original pitch and seeks ahead for delayed presentation. Losing
possession or a successful opposing block can still interrupt the aim without
refunding the item.

The existing four-island model, shared travel, scoring and host authority
remain. These changes use the existing protocol 69 packet layouts; share the
same complete updated player with all testers.

## Verification and delivery

The complete tested player was promoted to `Builds/WindowsFinal/` at **17:38
Malaysia time**. All **326 files** matched the candidate; only the explanatory
`LATEST-BUILD.txt` was subsequently changed. Use `Explore-Basketball.cmd` or
`Explore-Football.cmd` in that complete folder.

**737 Windows assertions passed**:

| Review | Offline | Local multiplayer |
| --- | ---: | ---: |
| Basketball audio | 37 | 59 (host and guest) |
| Football audio | 52 | 70 (host and guest) |
| Basketball props | 211 | 308 (host, opponent and teammate) |

Actual Lock-on release measured **1.0193 seconds offline** and **1.0304 seconds
with three peers**. The one-second authoritative deadline, planted movement,
long-range scoring, ordinary opposing block, cancelled aim and no-refund
behavior passed. Aim audio sought to the final second, stopped on release and
cancel, and respected live muting. Local footsteps remained audible; stationary
guests heard no host footsteps. Playing-source checks bounded footsteps below
0.203 and main impacts below 0.501 before listener Master gain. Contact events,
unique live RPC IDs, no stale drops and suspend cleanup also passed.

The full candidate completed at 17:29. Subsequent script refreshes changed only
the two audio review fixtures and their editor helper; all production C# hashes
still match the full candidate. Older fixtures assumed incomplete arrivals,
automatic kickoffs, earlier pickup materialization and shorter replay resets.
Their failed runs are retained as diagnostics; the corrected fixtures follow
the current gameplay and all final runs above passed. The existing prop review
now checks the one-second deadline and release window. The script refresh option
requires a full candidate first and is only appropriate for code changes with
unchanged serialized assets/type layouts.

Current task evidence is under ignored `Builds/QuietSportsAudioQA/`, including
`checks-final.json`, full/refresh logs, source comparisons and copy hashes.
Audio and prop fixture output stays under `Builds/BasketballAudioQA/`,
`Builds/FootballAudioQA/` and `Builds/BasketballPropsQA/` at the run paths recorded
in that manifest. `Build-BasketballAudioReview.ps1 -ScriptsOnly` performs the
guarded code-only refresh after a full review build.

Fresh `AndroidSubmission` completed at **17:41:05 Malaysia time**:
**87,592,176 → 87,583,188 bytes (−8,988)**, leaving **912,416,812 bytes** below
the strict **1,000,000,000-byte** cap. Build, automatic budget gate, ZIP integrity
and independent APK v2 signature verification passed. SHA-256:
**845E5EA5021A247E848D757B90E9A24EDE571470CB14835A6F0AB95E690AA1A6**.
The contributor comparison found no unexpected large growth. The bundle
shrinks 9,977 compressed bytes; IL2CPP grows 887 and metadata 99. No delivery
audio or visual assets were changed. See [the audit](APK-SIZE-AUDIT.md).

Build generation rewrote the Golf ball prefab's local object IDs and document
order. A format-aware comparison of all 14 Unity YAML documents confirmed
identical component data and references after resolving local IDs by object
name/component type. That prefab was clean at task start and was restored to
its original bytes; the generated copy, metadata and comparison remain in the
task evidence. No active asset content was removed. Platform build settings
returned to their opening state.

Reviewed superseded output was archived with recovery hashes:

- `Legacy/20261018-172542-261-quiet-audio-initial/`: initial candidate before the
  added one-second Lock-on request.
- `Legacy/20261018-173822-247-quiet-audio-delivery/`: prior final Windows player,
  APK and size audit.

Existing unrelated working-tree changes were preserved and included in both
deliveries. No Git publishing actions were taken. No Android device was
connected. Automated event/source checks do not establish subjective listening
balance, phone speaker quality, physical Android FPS or live WAN behavior.

Final repository readiness (`Check-TaskReady.ps1 -RequireApk`) and portable-path
checks passed at 17:43 Malaysia time. Evidence: `Builds/QuietSportsAudioQA/readiness.out`.
All Git changes remain unstaged for the user.
