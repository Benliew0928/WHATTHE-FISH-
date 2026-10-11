# Basketball gameplay audio

Implemented on **11 October 2026** after the user approved the listening set.
The two-second Lock-on aim uses a lower **220–360 Hz** fundamental, reduced
upper harmonics and peak **0.064 instead of 0.16 (7.96 dB quieter)**. Duration
stays exactly 64,000 samples at 32 kHz. Unity normalization is disabled;
decoded runtime peak measured **0.06448364** after Vorbis import.

## Delivery and controls

Launch `Builds/WindowsFinal/Explore-Basketball.cmd`. Court Settings provides
Master, Court sounds, Buttons and Ambience sliders. Defaults are 80%, 80%, 65%
and 45%, respectively; existing saved Master/Buttons values are preserved.
Master applies once through the listener. Changing Court sounds also adjusts
an already-playing aim or Turbo loop. Values persist when the settings close.
Combined room protocol **43** requires matching builds on host and guests.

There are **58 approved WAVs / 43 cue families**, with **2,346,294 authoring
bytes**. Sources, licences, source hashes and processing provenance live in
[ArtSource/Audio/Basketball](../ArtSource/Audio/Basketball/README.md). Only the
selected derivatives are Unity Resources. Freesound inputs are the approved
public HQ MP3 audition copies; exporting WAV does not recover lossless detail.
Missing sounds use original deterministic synthesis, with no AI service or
runtime synthesis dependency. The original download and research links remain
in the [preview report](BASKETBALL-AUDIO-PREVIEW.md).

## Action mapping

| Action | Playback trigger |
| --- | --- |
| Held dribble and hand contact | Locally displayed dribble phase crossing, including on guests; paused during gathers, air time, inbound holds and portals |
| Loose bounce and bounce pass | Actual floor collision, scaled by normal impact speed; resting chatter and repeated immediate callbacks are suppressed |
| Backboard, rim and padding | Explicit court collider classification; board and rim remain independent contacts |
| Swish / light net brush | Actual net score/touch event; a brush does not play score confirmation |
| Dunk | Contact-timed rim and net cues, reusing the approved bank |
| Catch, pickup, pass and shot | Confirmed possession or actual ball release, rather than a button press |
| Steal and block | Light swipe at the reach; ball slap only at successful contact |
| Walking and running | Descending planted ankle in the displayed animation; no airborne footsteps |
| Shoe cuts, defensive shuffles and landing | Turning/deceleration, defensive foot plants and actual air-to-ground transition, with teleport suppression |
| Gather and cancel | Quiet clothing cue on the new shot/pass action |
| Prop countdown / spawn / pickup | Actual countdown digit, materialization and accepted collection |
| Lock-on | Deadline-synchronized two-second aim; actual release accent; immediate stop and interruption cue on cancellation |
| Turbo | Accepted activation, quiet active texture and local player's actual expiry |
| Portal Steal | Open at both ends, intake at transfer phase 0.15, arrival at 0.52, catch and closure on completed transfer |
| Out of bounds and inbound | Initial boundary crossing and completed standing inbound placement |
| Rolling / coastal air | Quiet optional texture; rolling currently follows the authoritative physics body only |
| Court buttons | Click/confirm/back/toggle/select/error/open/close palette; held action buttons click once on press, gesture choices on change |

No whistle, period buzzer or match-win sound is added to shared practice because
those rules do not currently run. Failed prop requests return a local error
cue, including a server rejection sent to the requesting owner.

## Timing and mixing

[BasketballAudio.cs](../Game/Assets/_Game/Sports/Basketball/BasketballAudio.cs)
owns one bank and a bounded pool per active court: 16 world voices, four UI
voices and dedicated aim, Turbo, ambience and rolling sources. Physical
outcomes outrank feet and fabric; UI has separate slots. Variations rotate with
small pitch offsets on short physical cues. The aim retains its exact duration.

Host-authoritative live events use reliable server-only RPCs, unique sequence
IDs and timestamps. Guests reject stale events and wait for the existing
50 ms presentation offset. A host never plays its own event twice. These cues
are not retained in snapshots, so a joining client cannot replay an old score.
Held dribbles and foot plants follow each peer's presented animation rather
than adding network traffic for every contact. A late observer of an active
aim seeks to its remaining timeline. Leaving play, disabling the court or
suspending the application stops voices and clears queued events.

World voices remain at their contact positions rather than following the ball.
Horizontal distance from the local player determines attenuation so an elevated
court camera does not silence gameplay. Partial stereo positioning adds
direction, without Doppler. Ambient air and interface sounds are non-positional.

## Import, regeneration and verification

[BasketballAudioBuild.cs](../Game/Assets/_Game/Editor/BasketballAudioBuild.cs)
enforces 58 files, mono 32 kHz Vorbis at quality 0.65, preserved authored volume
and a two-second aim. Short cues decompress on load; the three longer textures
stay compressed in memory. Soft/hard bounces reuse the same four recordings
with impact gain; the precomposed dunk and audition alternatives are excluded.

Run `python Tools/Audio/publish_basketball_audio.py` to regenerate from retained
masters without a network. A relocated fixture with spaces in its path, launched
from an unrelated working directory, reproduced all **58 WAV hashes**. See
[audio tools](../Tools/Audio/README.md).

`Tools/Build/Test-BasketballAudio.ps1` exercises real gameplay in the Windows
player, offline and as two local network peers. It checks contact cues, feet,
landing, release, score, prop collection, aim duration/level/cancellation,
Portal phases, Turbo expiry, button deduplication, volume and suspension.
Runtime traces record cue, position, gain, event ID and reason. These verify
engine playback and action timing; they are not a recorded final mix or a
perceptual listening test. The user should assess the mix in gameplay.

Current reports and release measurements are in [VERIFICATION.md](VERIFICATION.md)
and [BUILD-SIZE.md](BUILD-SIZE.md). Physical Android speaker balance, audio
latency, touch/FPS and WAN behavior remain unverified.
