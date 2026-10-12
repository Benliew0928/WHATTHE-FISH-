# Approved football audio masters

The user approved all preview sounds on 12 October 2026. `delivery.json` maps
the 80 approved clips to repository-relative Unity paths and SHA-256 hashes.
Only 55 new WAVs live in `FootballAudio`; 25 existing basketball WAVs are
referenced in place. Alternate football kicks and trail-running takes join
the shot and running variation banks. The runtime exposes 54 action families
because those two comparison families are merged with their matching actions.

`Raw/` retains the eight downloaded public HQ MP3 sources; `sources.json`
records authors, CC0 declarations, source/download URLs, timestamp and hashes.
Paths in `sources.json` are relative to this directory. These are audition
copies, not the original WAV downloads. `LICENSES.txt` records the source
licenses. The saved uploader-page evidence remains in the local preview.

Regenerate offline with `python Tools/Audio/publish_football_audio.py` from the
repository root. The script resolves defaults from its own location and does
not depend on the caller's working directory. It requires Python, NumPy and
imageio-ffmpeg and the existing basketball source/tools and approved bank.
Do not replace existing Unity `.meta` files when regenerating WAVs.

All procedural effects are original local synthesis. No AI service or paid
asset was used. Importers preserve authored levels and use 32 kHz mono Vorbis;
normalization stays off. Raw recordings and preview alternatives/reels are
outside Unity. The raw source and delivered WAV extensions use Git LFS.
