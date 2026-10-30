# Recorded Golf cues

User-provided recordings approved for the Golf GO signal and walkway steps on
17 October 2026. Full 48 kHz stereo WAV masters retain their original decoded
AAC audio. Source recording names and SHA-256 hashes are in `provenance.json`;
all paths there are relative to this directory. Video frames are not required
to regenerate audio, so no video copy is shipped.

Public-release review, 29 October 2026: the owner does not know the recordings'
original sources. Historical approval to integrate them is not evidence of
public redistribution rights. The GO cue and all three walkway steps remain
active and packed in the current candidate. The owner subsequently explicitly
chose to retain the current effects on 29 October; replacement work is cancelled.
See [the asset-source review](../../../../Docs/AppGallery/ASSET-SOURCE-REVIEW.md).

Run `python Tools/Audio/prepare_golf_recordings.py` from the repository root,
or invoke that script from any working directory. Requires NumPy and SciPy.
It verifies masters, crops the selected ranges, downmixes, removes DC, applies
a gentle 70 Hz high-pass to footsteps, resamples to 32 kHz, normalizes to a
0.65 peak and adds 5 ms boundary ramps. GO retains its recorded pitch/timbre;
three distinct individual steps retain impact and scrape tails. The original
multi-step sequence is never played as one footstep.

Delivery files under `Game/Assets/_Game/Resources/GolfAudio/` use mono PCM on
load, avoiding codec pre-echo on short footstep transients. Imported clips are
shared assets and are not destroyed with a course. `delivery.json` records
repository-relative delivery paths, crop intervals and hashes. These WAV files
use the repository's existing Git LFS rules. Keep masters out of Unity Resources.
