# First-visit narration masters

These 45 mono, 24 kHz, 16-bit PCM WAV masters were generated with the user's
OpenAI credits for the fifteen guide steps in English, Simplified Chinese and
Bahasa Melayu. The selected built-in voice is `ballad`, with instructions for a
young adult, happy and energetic guide. The custom voice designer did not save
a voice; no custom voice ID is used.

[manifest.json](manifest.json) records the exact scripts, model, voice, speaking
instructions, source hashes, PCM hashes, durations and derived asset hashes.
Paths in that file are relative to this directory unless identified as
repository-relative delivery paths. The original streaming WAV downloads used
unknown-length RIFF/data headers. The preparation script corrected those length
headers through a WAV parser/writer without changing any PCM sample bytes.
Original download hashes remain in the manifest.

Run `python Tools/Audio/prepare_first_visit_narration.py` from a checkout with
NumPy and imageio-ffmpeg installed to validate all masters against the current
guide and localization catalog, then derive the Ogg files under
`Game/Assets/_Game/Resources/FirstVisitNarration/`. The script resolves the
repository from its own location and works from an unrelated working directory.
It does not contact OpenAI or spend credits. Existing validated derivatives are
reused. Keep both the WAV masters and game delivery assets through Git LFS.

For regeneration, use the recorded model, voice, speed and instructions with the
recorded script for each clip in the OpenAI Text to Speech playground or Speech
API. Supply credentials outside Git and outside Unity. The game plays packaged
files and needs neither an OpenAI connection nor an API key. The in-game guide
identifies the narration as AI-generated in all three supported languages.
