# Football audio integration

Current locality rule: grass walk, run and turning footsteps play only for the
local player. Opponent and teammate footsteps are silent on that client.
Shared ball contacts, tackles and match sounds retain their existing behavior.

All sounds approved and integrated on 12 October 2026. The complete validated
Windows player is in `Builds/WindowsFinal/`; launch `Explore-Football.cmd` there.
The [original analysis](FOOTBALL-AUDIO-PLAN.md) and
[listening preview](FOOTBALL-AUDIO-PREVIEW.md) retain the research and limitations.

## Delivery

80 approved clips serve 54 runtime action families. The two preview comparison
families join the power-kick and running variation banks. 55 new WAVs are in
`Game/Assets/_Game/Resources/FootballAudio/`; 25 shared basketball WAVs remain
at their existing paths. The reference and hash manifest is
[`ArtSource/Audio/Football/delivery.json`](../ArtSource/Audio/Football/delivery.json).
Sources, licensing, generator and offline regeneration instructions are retained
with the [masters](../ArtSource/Audio/Football/README.md).

The football importer uses 32 kHz mono Vorbis at quality 0.65, preserves authored
levels by disabling normalization, and keeps the turf loop compressed in memory.
Short events decompress on load. Playback uses a bounded pool of 16 pitch voices,
four reserved UI voices and three quiet loops; movement has lower priority than
contacts, props and match cues. Master, Pitch sounds, Buttons and Ambience share
the existing saved mixer preferences through a new Sound page in Pitch settings.

## Synchronization

Kicks occur on accepted release; receives, deflections, turf/post/net/Wall
contacts and pickups come from resolved outcomes. Wall's swept and physical
contact paths share a repeat gate. Net collisions retain their physical/fallback
deduplication. The solid Wall also gives quiet player-impact feedback.

Gait plants and visible foot-to-ball touches drive movement sounds on each peer.
Stationary possession is silent. Jump, tackle, hit, Dash and stamina sounds
follow presented state changes. Quiet roll/air/Turbo loops stop when their
context ends or the application suspends.

Portal audio follows football's 0.7-second transfer and visible shrink/grow/
closure phases. Accepted deployment, natural expiry and cancellation have
distinct cues. A reset does not play a pile of expiry sounds. Formal-match
score, countdown, kickoff, continuous overtime, final whistle and local results
follow match transitions; Explore has no scoring or match whistles.

Authoritative contacts use live, sequenced RPCs with a bounded queue and stale
event rejection, so entering an existing match does not replay past hits.
Initial presentation snapshots seed state. **Room protocol 51** includes the
football audio event RPC; all players in a room need the updated build.

UI factories and held-action pointer feedback share the approved button palette.
A held button emits one press sound, not a sound each frame. Keyboard action
edges also receive local feedback. Rejected props and team choices use the
error cue. Volume sliders change levels without repeated button ticks.

## Verification and build sizes

The Windows player built at **17:01:39 Malaysia time** includes the physical
Wall and enlarged spawn displays from the preceding delivery. All **322 files**
matched the tested candidate before updating its build note; **321 runtime and
support files** remain identical. Keep the whole player folder together.

**311 assertions pass on the delivered player**: football audio offline 49,
host/client audio 65, football Explore props 162 and basketball audio 35.
The guest receives 21 unique live event IDs without duplicates or stale drops.
Tests exercise accepted kick release, visible gait/dribble contacts, stationary
silence, turf/frame/net contacts, actual scoring, ability phases/expiry, held
button deduplication, active-loop muting, overtime and result timing.

An additional **756 football UI assertions** pass on the initial audio build,
before the goal-frame contact and presentation refinements. UI layout sources
are unchanged in the final build. The final Sound page was rendered and visually
checked separately. No targeted runtime errors occur in the six retained logs.
Automated assertions establish event timing and playback state, not subjective
listening quality; the approved preview provides the listening reference.

The initial test found that pitch containment could resolve a front goal-frame
contact before PhysX reported it. A geometry check against the actual post and
crossbar colliders now provides the same contact cue and shares the collision
repeat gate. Football physics and goal rules are unchanged.

All **80 source WAV hashes** reproduce offline from a relocated path containing
spaces and an unrelated working directory. All **212 compiled runtime source
documents** match. After Android, **3,650/3,652 Game input hashes** match; only
generated Golf prefab IDs and URP resource registration differ.

Fresh AndroidSubmission: **89,859,034 → 90,099,410 bytes (+240,376)**.
Hard-boundary headroom is **9,900,590 bytes**. The APK passes the enforced budget
and v2 signature checks, but remains **15,099,410 bytes above the 75 MB development
target**. All 55 new clips occupy **190,152 serialized bytes**, a measurement
distinct from APK size. Shared clips are not duplicated; existing packed asset
sizes and all 1,788 imported mesh records match. No phone was connected;
Android touch, visuals/FPS, on-device listening and real WAN latency are unverified.

Evidence is retained under `Builds/FootballAudioQA/`, with related regression
paths in `verification.json`. See [release size](BUILD-SIZE.md),
[packing audit](APK-SIZE-AUDIT.md) and [verification record](VERIFICATION.md).
Reviewed superseded output and scratch files are recoverable in these ignored
archive batches:

- `Legacy/20261012-170341-319-football-audio-before-android/`
- `Legacy/20261012-170457-442-football-audio-regeneration/`
- `Legacy/20261012-170941-825-football-audio-windows/`
- `Legacy/20261012-170958-708-football-audio-cleanup/`

Active sources, licenses, generators and Unity metadata remain publishable.
No files were staged, committed or pushed.
