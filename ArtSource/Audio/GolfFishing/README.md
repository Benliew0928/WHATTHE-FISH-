# Approved Golf and Fishing sound bank

The user approved all preview candidates and requested integration on 18 October
2026. `approval.json` records the exact reviewed variants and SHA-256 hashes.
`Clips/` retains the 81 new delivery masters; `Raw/`, `sources.json`, `Evidence/`
and `LICENSES.txt` retain the 11 CC0 recording sources and uploader evidence.
These recordings are public HQ MP3 copies, not lossless original downloads.
Other new effects are deterministic local synthesis, without an AI service.

`delivery.json` uses paths relative to the repository root. It references 30
existing Basketball/Football WAVs and four existing user-recorded Golf WAVs in
place. Those shared assets remain owned by their original banks. The generated
Unity resource index uses paths relative to Unity's Resources directories.

Restore the exact approved files offline with:

```powershell
python Tools/Audio/publish_golf_fishing_audio.py
```

The script locates the project from its own path. Keep LFS files resolved.
Unity generates and retains `.meta` files; the publisher preserves them. The
importer disables normalization and imports the 81 new clips as mono 32 kHz PCM
to retain quiet levels and sample-contiguous short loops. Shared clips preserve
their original import settings. Source WAV bytes are not the APK measurement.

The listening page and reels remain in ignored `Builds/GolfFishingAudioPreview/`.
Retained source-page evidence resolves website-root links to Freesound URLs for
offline viewing and omits executable website scripts and transient anonymous form
tokens. This also removes embedded third-party service credentials from the
publishable evidence. License and uploader
text is preserved; untouched downloads remain in the local preview evidence.
See the [action/source plan](../../../Docs/GOLF-FISHING-AUDIO-PLAN.md) and
[integration record](../../../Docs/GOLF-FISHING-AUDIO.md).
