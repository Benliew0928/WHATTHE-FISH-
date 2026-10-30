# First-visit speech and click audio

The fifteen guide steps now speak automatically in the selected English,
Simplified Chinese or Bahasa Melayu language. The built-in OpenAI Ballad voice
uses the recorded youthful, happy and energetic adult delivery instructions.
All forty-five files were generated using the user's OpenAI account credits.
The game plays packaged audio and contains no OpenAI credentials or generation
requests. A translated AI-generated-voice label appears below the character.

Advancing reuses the existing shared menu click effect and replaces the previous
speech immediately. Skip, completion and hiding the guide stop speech. Narration
uses UI volume and the master listener volume. Changing language restarts the
current step in the selected language; resizing the guide does not restart it.
The seen-state behavior and existing four-island room model are unchanged.

## Assets and regeneration

The lossless masters, exact scripts, prompt and hashes are in
[the narration source folder](../ArtSource/FirstVisitNarration/README.md).
The 45 mono 24 kHz WAV files total **13,835,580 bytes** and **288.2 seconds** across
all languages. Streaming WAV length headers were corrected without altering PCM
samples. The 45 Ogg derivatives total **2,384,726 source bytes**; the mobile build
uses mono Vorbis compression. All audio assets are covered by Git LFS.

Run [the preparation tool](../Tools/Audio/prepare_first_visit_narration.py) to
verify all scripts against the guide/catalog and derive the runtime assets.
This offline step spends no credits. Paths resolve from the script location.
Credentials stay outside Git and the Unity player for any future regeneration.

## Verification and delivery

[The guide regression runner](../Tools/Build/Test-FirstVisit.ps1) passed **3,496**
assertions across 1600×900, 1560×720, 1200×900 and 1920×1080 in all three languages.
Checks cover actual pointer routing and shared click-source playback, matching
speech clips, single-source narration, duration, volume/mute, live language
changes, immediate skip/exit cancellation, persistence and text/target bounds.
Representative rendered layouts were reviewed in all languages. All 45 masters
and 45 decoded Ogg files passed script/hash, duration and nonsilent-signal checks.
Catalog/font and artwork checks passed. All 541 Windows and 388 Android compiled
project-source documents match their current source files.

The complete 342-file Windows player was promoted to ignored `Builds/WindowsFinal/`
with every file hash verified, retaining the existing launchers. The new
AndroidSubmission APK is **171,975,759 bytes**, compared with **170,877,702** before
this change; headroom is **828,024,241 bytes**. ZIP CRC, all 1,502 actual entry sizes,
the five-scene bundle hash/length, v2 signature and expected non-debuggable package
checks passed. The 45 narration assets occupy **1,077,884 uncompressed
serialized bytes**, distinct from both the APK and Ogg source measurements.
Development review helpers are excluded from Android. See
[the size record](BUILD-SIZE.md) and [APK audit](APK-SIZE-AUDIT.md).

Current compact evidence is in ignored `Builds/FirstVisitAudioQA/`; the current
size audit is in ignored `Builds/SizeAudit/latest/`. The previous Android/APK audit
is in `Legacy/20261027-202155-263-first-visit-narration-android/`, and the previous
Windows player is in `Legacy/20261027-202324-184-first-visit-narration-windows/`.
The build regenerated local golf-ball prefab IDs; a format-aware comparison
verified its same fourteen-object graph and external references before restoring
its pre-task source representation. Cleanup paths and reasons are recorded in
`Builds/FirstVisitAudioQA/cleanup.json`; final masters, delivery files, evidence and
rollback archives are retained. Archiving retains recovery data and does not
itself free disk space.

This review did not perform phonetic transcription of every utterance, native
provider sign-in or physical-phone speaker, touch, visual and performance testing.
Existing Git work was preserved; no files were staged, committed or pushed.
