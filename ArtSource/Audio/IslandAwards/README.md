# Island awards sound sources

- `CrowdReference.mp4`: user-supplied screen recording provided on 28 October 2026 to replace the awards applause/cheer. Retained unchanged as the editable audio master; only its audio stream enters the game. No CC0 claim is made for this user-supplied reference.
- `Cheers.wav`: a 4.65-second mono 32 kHz derivative of that recording. It removes the empty tail, balances gain, applies light high/low-pass filtering, and fades gently in/out. It replaces the previous short standalone cheer in both Fishing and Golf.
- `GrassPlant1-4.wav` and `WoodPlant1-4.wav`: awards-only attack-aligned derivatives of the existing approved Football grass and Golf/Fishing wooden step banks. Their original bank provenance remains applicable. Ordinary gameplay clips are unchanged.

Rebuild with [build_island_awards.py](../../../Tools/Audio/build_island_awards.py), supplying `--ffmpeg` or installing FFmpeg on PATH. All asset paths resolve from the script's repository location.
The previous standalone OGG cheer is archived. Awards retain one-shot playback and lower celebration volume when viewing results.

Reference SHA-256: `a056d1c675b5b005e76d12628d15c6bee2bb47daf6ab9ac876f98fdbb2250355`.
