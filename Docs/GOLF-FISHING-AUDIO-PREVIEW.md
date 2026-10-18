# Golf and Fishing listening preview

Prepared 18 October 2026. **All sounds approved by the user on 18 October 2026.**
The following describes the original listening preview. See the
[integration record](GOLF-FISHING-AUDIO.md) for subsequent game delivery.
The [complete analysis and source research](GOLF-FISHING-AUDIO-PLAN.md) describes
all current actions, reuse decisions and proposed synchronization.

Open the [generated soundboard](../Builds/GolfFishingAudioPreview/index.html),
or run the local server described in [the audio tools guide](../Tools/Audio/README.md).
The current listening address is `http://127.0.0.1:8768/`.

Start with **Golf · a short round** (23 seconds) and **Fishing · cast to catch**
(24 seconds). They are illustrative edits with time jumps, not gameplay capture
or evidence that new sounds are synchronized in the game. Seven additional
reels group contacts, buggy, fishing actions, skills, footsteps, rewards and UI.

The audition contains **115 clips / 81 families**:

- **30 unchanged approved Basketball/Football clips**, reused by meaning.
- **4 unchanged Golf recordings**: your GO and three walkway contacts.
- **81 newly edited or generated clips**, using 11 CC0 recording sources and
  original deterministic synthesis. No AI service was used.

Browse categories or search by action, compare variants, and mark **Keep** or
**Change**. Select your preferred variant before saving a choice. Notes and
choices stay in the browser; **Download my choices** exports JSON. You can also
reply in chat with sound names and requested adjustments. These controls never
modify the game. Reverse confirmation and nearby fish-respawn bubbles are
clearly marked optional.

Particularly useful listening comparisons:

- Golf hit versus Putt; the low soft charge against the unchanged recorded GO.
- Natural cup rattle, hole completion and final result as separate events.
- Buggy motor/tyres at a comfortable low level, with parked silence intended.
- Bite alert variations, winding versus drag, and the restrained tension warning.
- All six skill activations and Bubble Guard's separate absorption sound.
- Wooden pier steps for residual indoor resonance, and sand for coastal noise.

New WAVs are mono PCM16 at 32 kHz. The preview keeps authored relative levels;
its volume slider applies the same playback scaling to all cues. Individual
loop candidates have **three sample-contiguous repetitions** for judging joins;
WAV download links still point to one original loop. All 115 WAVs passed format,
decoding, finite/non-silent signal, clipping, DC, edge and hash checks. All nine
loop auditions contain exactly three copies of their source samples. The nine
MP3 reels decode without clipping. A relocation check reproduces **142 audio
file hashes** offline from a path with spaces and an unrelated working directory.

Source-page evidence and download/source hashes are in `sources.json` and
`Evidence/`. `manifest.json` records every action, variant, edit and intended
trigger. `Evidence/clip-qa.json` and `Evidence/validation.json` record checks.
Technical checks do not establish subjective sound quality, perceived loudness,
phone-speaker performance or gameplay synchronization; those require listening
approval and later integration testing.

All new audition output is ignored under `Builds/GolfFishingAudioPreview/`.
Portable download, build, validation scripts and this documentation are kept
in source. No Unity assets, imports, runtime code, WindowsFinal player or APK
were changed by this preview task. No Git staging, commit or push was performed.

Browser checks confirm both scene players, exclusive playback, individual clips,
search, selecting Bite variation 2, a progressing 6.84-second three-cycle reel,
volume UI, Keep persistence through reload and feedback-export confirmation.
The temporary test choice was removed; user decisions start empty. All **136
served page/manifest/audio URLs** returned the expected file lengths.
Screenshots and UI/HTTP evidence are retained in `Evidence/`.

The discarded combined putt/cup source, completed relocation fixture and
superseded one-time authoring helpers are recoverable in
`Legacy/20261018-032325-072-golf-fishing-audio-preview/`. Current research,
downloads, listening files and verification evidence remain available.
