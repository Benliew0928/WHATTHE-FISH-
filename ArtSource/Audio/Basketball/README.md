# Basketball audio sources

The user approved the first 71-variation listening set on 11 October 2026,
requesting a softer, lower-pitched two-second Lock-on aim. The delivery selection
contains 58 files. Soft/hard bounces share the main bounce takes at different
gains; dunks use separately timed rim and net contacts. Alternative footsteps,
synthetic squeaks and the existing menu tap stay outside the shipped bank.

`Raw/` preserves the downloaded CC0 source recordings and Kenney publisher ZIPs.
These are editable source material, not Unity Resources. Freesound files are
the public HQ MP3 audition copies the user approved, not original WAV masters.
No fidelity recovery is implied by exporting edited PCM WAVs. The two Kenney
licence files are retained beside this document. All source URLs, creators,
download date, CC0 references and original SHA-256 values are in `sources.json`.

`delivery.json` uses paths relative to the repository root and records the
approved derivative hashes, durations and processing. Original procedural
effects are authored in [build_basketball_preview.py](../../../Tools/Audio/build_basketball_preview.py).
They do not require an AI model, paid API or runtime synthesis dependency.

To recreate the preview from these retained masters and publish the approved
clips, run this from the repository root:

```powershell
python Tools/Audio/publish_basketball_audio.py
```

Dependencies are Python, NumPy, and imageio-ffmpeg. This command needs no network.
It reconstructs ignored audition output under `Builds/BasketballAudioPreview/`
and writes the 58 delivery WAVs to `Game/Assets/_Game/Resources/BasketballAudio/`.
Keep existing Unity `.meta` files when regenerating. Unity import preserves
authored levels, uses mono 32 kHz Vorbis at quality 0.65, preloads short cues and
keeps the three longer textures compressed in memory.

The revised aim has a 220-360 Hz main sweep, reduced upper harmonics and a peak
of 0.064 instead of 0.16 (7.96 dB lower), retaining exactly 64,000 samples.
Regeneration uses a fixed random seed. Retained source packs include the
research alternatives so the original preview generator remains reproducible;
unused recordings are never imported into Unity.
