# Sports audio preview tools

## Football approved bank and preview

Football's complete preview was approved. See the
[football sound plan](../../Docs/FOOTBALL-AUDIO-PLAN.md) and
[integration record](../../Docs/FOOTBALL-AUDIO.md). Regenerate offline with:

```powershell
python Tools/Audio/publish_football_audio.py
```

This verifies retained sources, regenerates the audition, publishes 55 new WAVs
and references 25 approved basketball WAVs in place. Sources and license records
are retained in `ArtSource/Audio/Football/`. Preserve existing Unity `.meta`
files. Unity imports retain authored levels with normalization disabled.

To fetch fresh candidate sources and rebuild the original audition instead:

```powershell
python Tools/Audio/download_football_candidates.py
python Tools/Audio/build_football_preview.py
python -m http.server 8767 --bind 127.0.0.1 --directory Builds/FootballAudioPreview
```

Open `http://127.0.0.1:8767/`. These scripts need the same Python dependencies
listed below and derive their default root from their own location. Both accept
`--output`; use the same destination when overriding it. They download eight
CC0 public Freesound HQ MP3 audition copies, verify hashes, and build 80 clips
across 56 families, including 25 unchanged approved basketball WAVs. The
basketball delivery manifest and its LFS-resolved Unity WAVs must be present.
The downloader and preview builder do not write Unity files. Generated audition
files stay in ignored `Builds/FootballAudioPreview/`. New effects use
deterministic local synthesis, not an AI service.

Mark Keep / Change, compare variations and export choices in the preview.
The new downloader and builder import shared utilities from the existing
basketball scripts, so keep those scripts together. Source pages and download
availability may change; review a refreshed download manifest before reuse.

## Basketball approved bank

The audition tools create a soundboard outside Unity. The approved selection
is now published into the game by `publish_basketball_audio.py`. See the
[integration record](../../Docs/BASKETBALL-AUDIO.md),
[preview report](../../Docs/BASKETBALL-AUDIO-PREVIEW.md) and
[sound plan](../../Docs/BASKETBALL-AUDIO-PLAN.md).

## Recreate the approved game bank

```powershell
python Tools/Audio/publish_basketball_audio.py
```

This verifies retained source hashes, regenerates the audition from
`ArtSource/Audio/Basketball/`, and writes 58 approved mono WAVs under
`Game/Assets/_Game/Resources/BasketballAudio/`. It needs NumPy and
imageio-ffmpeg, with no network, account or API key. Existing Unity `.meta`
files must be retained. The softer Lock-on sweep stays exactly two seconds;
Unity import disables normalization to preserve its authored level.

## Rebuild the preview

Use Python 3.10 or newer with `requests`, `numpy` and `imageio-ffmpeg` available.
The latter supplies the FFmpeg executable. No account, API key or AI subscription
is used.

From the repository root:

```powershell
python Tools/Audio/download_basketball_candidates.py
python Tools/Audio/build_basketball_preview.py
python -m http.server 8766 --bind 127.0.0.1 --directory Builds/BasketballAudioPreview
```

Open `http://127.0.0.1:8766/`. Alternatively open the generated `index.html`
directly and keep its `Clips` and `Reels` directories beside it. Browser support
for saving review choices may differ when using a local file URL.

Both Python scripts derive the default repository root from their own location,
so they also work when invoked from an unrelated working directory. `--output`
can select a different audition directory; use the same destination for both
scripts. An explicitly supplied relative override is relative to the caller.

The downloader retrieves nine CC0 Freesound audition MP3s and two original
Kenney packs. It saves source-page evidence, licences, URLs, download time and
SHA-256 hashes in the output folder. Source availability and page structure can
change. Inspect a new download manifest before treating it as the same source
revision. The builder verifies downloaded file hashes against that manifest.

The builder trims recordings, layers selected effects, synthesizes missing cues
with a fixed random seed, and writes 32 kHz mono PCM16 WAV candidates plus MP3
listening reels. Generated effects are original local synthesis, not AI output.
`manifest.json` records each cue's sources, processing, variants and file paths;
`Evidence/clip-qa.json` records signal measurements and WAV hashes.

Audition output remains under ignored `Builds/` by default. The publisher keeps
masters and licence evidence in `ArtSource/Audio/Basketball/`, and approved Unity
WAVs and `.meta` files in active source. The raw packs, soundboard, reels and unused
alternatives stay outside the Unity project and APK.

## Listening and limitations

Start with the illustrative court sequence, then compare individual variations.
The four categories cover ball and basket contacts, shoes and body movement,
props and court feedback, and UI buttons. Mark Keep or Change and use
Download my choices to export a review file. This does not install audio.

Freesound publicly exposes HQ MP3 previews; its original WAV downloads require
login. Exporting the previews as WAV does not recover the original fidelity.
Kenney files come from the original publisher ZIPs, with their licence files.
The technical checks do not constitute an auditory review, a perceptual loudness
match, device-speaker validation or a gameplay mix test.
