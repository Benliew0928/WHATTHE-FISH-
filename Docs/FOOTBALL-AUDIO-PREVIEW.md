# Football audio listening preview

Prepared and **approved in full** on 12 October 2026. This document records the
original audition; see [integration](FOOTBALL-AUDIO.md) for current delivery.
The [sound plan](FOOTBALL-AUDIO-PLAN.md) covers gameplay,
props, UI, reuse, research, licensing and proposed integration timing.

## Listen and choose

Open the generated [soundboard](../Builds/FootballAudioPreview/index.html), or
serve it locally as described in [the audio tools guide](../Tools/Audio/README.md).
The current listening server uses `http://127.0.0.1:8767/`.

Start with the 18-second pitch sequence. It is an illustrative edit with time
jumps between situations, including Wall expiry; it is not gameplay capture
or proof of synchronization. The five shorter reels group related effects.
Then open the individual categories, compare variations, and mark **Keep** or
**Change**. Choices and overall notes stay in that browser; **Download my
choices** exports JSON. Feedback does not install sounds automatically.

| Category | Families | What to focus on |
| --- | ---: | --- |
| Ball & goal | 12 | Powerful kick versus alternate kick; post variants; net texture; softer touch/receive/bounce. |
| Movement & defense | 13 | Grass walk/run versus trail alternative; tackle slide and landing; optional stamina-ready cue. |
| Props | 15 | New Wall family alongside approved pickup, Turbo and Portal sounds. |
| Match & ambience | 8 | Whistle sharpness, overtime and win/loss/draw phrases; existing coastal air. |
| Buttons | 8 | The unchanged approved basketball UI palette. |

Total: **56 sound families, 80 mono WAV clips**, including **25 unchanged
approved basketball clips**. The eight newly downloaded CC0 recordings feed
the edited and layered candidates; remaining new material is local procedural
synthesis. No paid service or AI generation was used.

Preview volume starts at 55%. All players share that master control while
retaining each file's authored level. Loop samples play once; Play this section
skips loops. Stop all stops every player. No automatic sound starts on page load.

## Files and provenance

All audition output is under ignored `Builds/FootballAudioPreview/`:

- `index.html`: self-contained interface with relative audio references.
- `Clips/`: 80 32 kHz mono PCM16 WAVs, **2,906,776 bytes** in total.
- New clips alone: **1,613,038 PCM bytes**; this is not compressed APK growth.
- `Reels/`: six MP3 listening reels and their WAV masters.
- `Raw/` and `sources.json`: eight public HQ MP3 sources and verified hashes;
  approved basketball delivery provenance is also copied here.
- `manifest.json`: cue names, origins, processing, variations, paths and timing.
- `Evidence/`: saved license pages, measurements and verification results.

The [downloader](../Tools/Audio/download_football_candidates.py),
[builder](../Tools/Audio/build_football_preview.py) and
[interface template](../Tools/Audio/football_preview.html) are retained source.
They reuse the existing basketball DSP utilities rather than duplicating them.
The builder verifies source hashes and checks each shared WAV against the
approved delivery manifest before copying it byte for byte.

## Verification and limits

The exported WAV measurements report **zero full-scale clipped samples**.
All 80 WAVs and six MP3 reels decode successfully. Rebuilding a relocated source
fixture in a path containing spaces, from an unrelated working directory,
produced identical hashes for all 80 WAVs and six MP3 reels. Document links
resolve. Browser verification confirmed scene playback, variation selection,
Wall playback, Keep persistence across reload, and Stop all; test choices were
cleared afterward. The browser reported no errors or warnings, and the desktop
layout had no horizontal overflow. `Check-TaskReady.ps1` passed its repository
hygiene and portable-path checks. These results are retained in `Evidence/`.

Technical signal/format checks and browser playback do not establish subjective
sound quality, perceptual loudness balance, phone-speaker quality or gameplay
synchronization. MP3 inputs can contain prior distortion that lowering gain
cannot repair. Final listening and selection remain with the user.

This is a preview/tooling/documentation task, so no Unity rebuild or Windows
player replacement is required. The APK at task start was **89,851,508 bytes**
with **10,148,492 bytes** to the strict 100,000,000-byte boundary; it exceeded
the 75,000,000-byte development target by **14,851,508 bytes**. This existing
build does not validate future football-audio integration. No delivery growth
or device-performance claim is made by this preview.

The shared workspace changed during this task: eight existing Game inputs
differed from the initial hash snapshot, and the standard release APK was
temporarily absent at the final audio check. This task made no Game edits or
build-output changes; concurrent game work was preserved. The initial APK
measurement above is a baseline, not final delivery certification.

The completed relocation fixture was archived with the repository tool in
`Legacy/20261012-162900-519-football-audio-preview/`. Current clips, provenance
and verification evidence remain in the active preview folder. No files were
staged, committed or pushed.
