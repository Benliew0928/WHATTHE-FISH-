# Golf and Fishing approved audio integration

The user approved all 115 clips across 81 cue families on 18 October 2026 and
requested gameplay integration. [Research and action mapping](GOLF-FISHING-AUDIO-PLAN.md)
and the [listening preview](GOLF-FISHING-AUDIO-PREVIEW.md) retain the review history.

## Assets and reuse

81 new WAVs total 4,186,548 source bytes. Thirty Basketball/Football clips and
four recorded Golf clips are referenced at their original Resources paths.
No shared WAV is duplicated in the delivered bank. `GolfFishingAudioBank.json`
maps semantic cues to all approved variants. Variations rotate without pitch
randomization; Left/Right guidance and tyre surfaces select their specific take.

The new assets preserve the approved samples and relative levels, with mono
32 kHz PCM imports and normalization disabled. Quiet loops use the authored
sample-contiguous joins. Shared assets retain their existing import settings.
The retained [masters and provenance](../ArtSource/Audio/GolfFishing/README.md)
can restore exact reviewed WAVs offline; no network service or API key is needed.

## Runtime behavior

Golf's existing host-confirmed cosmetic event channel handles swing, putt,
landing and cup events. It now also carries hard-surface, timber, ball-pair,
water-entry, recovery and actual buggy collision cues. Invalid, stale, other-round and duplicate events
remain rejected. Club contact stays synchronized to the actual strike point.
The original GO and three walkway recordings remain unchanged. Foot planting
keeps its existing gait timing and chooses approved surface takes.

Golf rolling follows observed grounded ball movement. Buggy sounds follow
summon/recall, driver changes, observed travel, reverse and stopping. The first
finisher notice, last five seconds and settled result remain distinct from an
individual cup. Stationary carts and resting balls stop their movement loops.
Generic vertical support collisions use a hard contact even when the collider
has no material keyword. Tree and palm contacts use timber foley. The shot
regression distinguishes obstacle impacts from a putt's predicted resting point.

Fishing's local accepted-record cursor preserves silent initial, late-join and
resumed baselines. Cast, float entry, bite, hook, catch, miss and snap use the
approved assets. Correct-side holds wind the reel; increasing paid-out line
uses drag, with a short continuity allowance between network snapshots. Tension
creaks, cooling warnings and recovery have separate thresholds. Visible surface
crossings trigger splashes. Pickups, salvage start/emergence, all six skills,
Sonar recipients, Bubble Guard consumption, natural expiry, catch points and
Wanted bonuses follow observed state changes. Round, Cup and Crew outcomes use
the corresponding result cues.

Shared buttons route to the approved UI bank, including held action controls,
drawers and result pages. Keyboard activation is supported. Audio uses the
existing master, SFX, UI and ambience preferences. Island loops and cues stop
on focus loss, pause, travel and component disable. Imported clips are retained
as shared Unity assets instead of being destroyed with an island instance.

## Verification and delivery

The complete Windows player was promoted to `Builds/WindowsFinal/`, with
**326 copied files verified by SHA-256**. Full player build completed at
13:09 MYT; final Unity script iteration completed at 13:17 MYT, retaining
that full build's matching packaged scene payload. All 461 runtime/editor
source-file hashes were checked against the validated snapshot.

**970 task-relevant assertions passed:**

| Check | Assertions | Evidence under `Builds/` |
| --- | ---: | --- |
| Golf approved bank, contacts, button deduplication, lifecycle | 159 | `GolfFishingAudioQA/Golf-20261018-131909-802` |
| Fishing bank, accepted actions, six skills, final catch and lifecycle | 186 | `GolfFishingAudioQA/Fishing-20261018-131913-944` |
| Actual Golf shot paths, contacts, cups and cancelled inputs | 225 | `GolfShotQA/Run-20261018-131737` |
| Fishing controls, camera, results and glyphs | 178 | `FishingPolishQA/Run-20261018-131203` |
| Golf HUD and camera checks | 222 | `GolfHUDQA/Run-20261018-131824` |

All 115 approval hashes match. Offline publication from a relocated folder
with spaces and an unrelated working directory also reproduces the same bank.
Rendered Golf and Fishing captures were inspected. Black captures from the
small audio-only harness are not used as visual evidence. Task build logs,
signature results, source inventories and readiness output are retained under
`Builds/GolfFishingAudioQA/`.

Fresh AndroidSubmission at **13:20:42 MYT**: **83,309,709 → 87,561,004 bytes**
(**+4,251,295**), leaving **912,438,996 bytes** below the strict 1 GB limit.
The automated cap and independent APK v2 signature verification passed.
[Measured size](BUILD-SIZE.md) and [contributor audit](APK-SIZE-AUDIT.md) record
the details. No physical Android device was connected, so phone sound mix,
FPS, thermal behavior and memory remain unverified.

No Git staging, commit or push is performed by this task. Existing unrelated
working-tree changes are preserved and will be included in the shared-source
build; total APK growth cannot be attributed solely to audio.
Readiness found a retained 133,432,314-byte character-source `Idle_Playful.anim`
outside Unity's active Assets tree. An exact-path LFS rule was added to preserve
that existing master while meeting the repository's large-file check. Its
contents were not edited or staged. Verified stale empty Git locks were also
removed only after confirming that no Git process held them.

The superseded Windows player was archived with recovery hashes in
`Legacy/20261018-132008-029-golf-fishing-audio-windows`. Intermediate players
are in the `20261018-125859-074-golf-fishing-audio-first-player` and
`20261018-130524-549-golf-fishing-audio-regression-player` batches. Completed
one-time helpers and the first relocated fixture were archived in
`Legacy/20261018-131557-972-golf-fishing-audio-cleanup`. Approved source masters,
publishers, reusable checks, Unity assets and metadata remain active.
The redundant candidate player and final offline fixture/helper are retained
in `Legacy/20261018-132207-878-golf-fishing-audio-delivered`; WindowsFinal is
the current active player.

The broader cart suite reports the same **33 distinct failed assertions** in
the previous WindowsFinal player and the audio candidate: camera expectations,
seated toe pose, speed/steering expectations and some bunker traversals. These
pre-existing failures are outside this audio change. Comparison evidence is
`Builds/GolfFishingAudioQA/cart-baseline-differences.json`, with full baseline
and candidate reports in the recorded GolfCartQA runs. This is not a claim that
all existing cart behavior passes validation.
