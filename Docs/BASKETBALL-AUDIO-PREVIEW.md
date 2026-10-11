# Basketball sound preview

Prepared on **11 October 2026** for the Rally basketball court. The package has
**71 clip variations across 49 cue families**, combining CC0 recordings and
original procedural effects. The user approved the sounds and requested a
softer, lower-pitched two-second Lock-on aim. That revision and 58 selected clips
are now integrated; see [gameplay audio](BASKETBALL-AUDIO.md). The assistant's
checks are technical; listening approval came from the user.

## Preview and review

The local soundboard is `Builds/BasketballAudioPreview/index.html`. Serve that
directory on localhost or open the file directly. The [audio tools README](../Tools/Audio/README.md)
contains repeatable commands. All file paths in generated manifests are relative
to the directory containing those manifests.

The soundboard offers individual playback, variation selectors, source details,
Keep and Change choices, and a downloadable feedback file. Four MP3 reels are
available under `Reels/`: an illustrative 13.2-second court sequence, court sound
examples, prop examples and the button palette. The sequence is an edited
audition, not recorded gameplay or proof of animation synchronization.

## Selected direction

| Group | Preview candidates | What to decide while listening |
| --- | --- | --- |
| Ball and basket | Four court bounces, soft and hard derivatives, one board strike, three rim hits, a net swish, light net brush and layered dunk | Whether remaining gym reflections fit the open court; whether the swish and rim ring are convincing |
| Ball handling | Generated catches, hand slap, release, block, rolling texture and score confirmation | Whether the generated contacts need replacement with real palm-and-ball recordings |
| Movement | Four recorded sneaker steps, four Kenney alternatives, four designed running steps, three recorded squeaks and three generated alternatives | Footstep weight and whether the recorded walking-shoe squeak suits athletic cuts |
| Body and clothing | Generated scuffs, jersey rustle and missed swipe; layered landing and padded impact | Keep these quiet enough to support the ball and shoes |
| Props and court | Pickup, spawn, countdown, Lock-on aim/release/interruption, Turbo start/active/end, Portal Steal open/intake/arrival/close, boundary ripple, inbound notice and optional coastal air | Distinct prop identities, restrained volume and no confusing pickup versus score cues |
| Buttons | Kenney click, confirm, back, toggle, select, error, panel open and close; existing menu-style tap alternative | Choose one coherent palette, then balance it against the current Cove menu |

Running and landing are designed from recorded walking soles plus generated
transients; they are not separate recordings of a player sprinting or landing.
The Lock-on aiming cue is exactly two seconds. The two-second Turbo texture is
an optional looping source for the five-second gameplay effect. Loop samples
play once in the individual audition player.

## Sources and provenance

The selected recorded sources are published as CC0 on their source pages:

| Role | Source | Download used |
| --- | --- | --- |
| Ball bounces | [OBXJohn 262508](https://freesound.org/people/OBXJohn/sounds/262508/) | Public HQ MP3 audition copy |
| Backboard | [PerMagnusLindborg 324247](https://freesound.org/people/PerMagnusLindborg/sounds/324247/) | Public HQ MP3 audition copy |
| Rim | [reedhos 244348](https://freesound.org/people/reedhos/sounds/244348/) | Public HQ MP3 audition copy |
| Net | [vhschool2019 716930](https://freesound.org/people/vhschool2019/sounds/716930/) | Public HQ MP3 audition copy |
| Sneakers on wood | [Greenbird11 618816](https://freesound.org/people/Greenbird11/sounds/618816/) | Public HQ MP3 audition copy |
| Shoe squeaks | [Christophersaa365 703188](https://freesound.org/people/Christophersaa365/sounds/703188/) | Public HQ MP3 audition copy |
| Button palette | [Kenney Interface Sounds](https://kenney.nl/assets/interface-sounds) | Original publisher ZIP |
| Alternative footsteps | [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds) | Original publisher ZIP |

Three additional Freesound recordings remain in `Raw/` as research alternatives:
[net variants](https://freesound.org/people/21100495/sounds/655323/),
[outdoor basketball](https://freesound.org/people/fattirewhitey/sounds/381410/),
and [shoe skids](https://freesound.org/people/shatterstars/sounds/492071/).
They are not used in the proposed bank and have not been rejected by ear.

Freesound original WAV downloads require login. The trimmed WAVs here derive
from the public MP3s and are not lossless source masters. Integration uses the
audition copies the user approved. An optional future fidelity upgrade can use
the original WAVs with the same edits and a fresh listening comparison.
The source manifest preserves creators, URLs, licence
references and downloaded-file hashes; Kenney's licence files remain with the
extracted packs.

All missing effects in this pass use original deterministic synthesis. No paid
AI service was used. If the generated catch, scuff or squeak feels artificial,
the preferred refinement is a close recording of sneakers and a basketball on
wood; AI generation remains an optional alternative described in the
[research plan](BASKETBALL-AUDIO-PLAN.md).

## Original preview verification and size

The 71 candidate WAVs are mono PCM16 at 32 kHz, totaling **2,646,848 bytes** and
**41.308 seconds** including three loop sources and all alternatives. These are
authoring-file measurements, not APK contribution estimates. Unused alternatives
and whole downloaded packs must not enter the shipped game.

Signal checks found no full-scale clipped samples in the exported bank. The
highest candidate peak is **-4.44 dBFS**. Levels are provisional and peak-based;
perceived loudness still needs listening. Some decoded MP3 sources exceed
0 dBFS, which can arise from codec reconstruction. Attenuating an export avoids
new output clipping but does not prove that the original recording is free of
distortion. No claim of repaired source distortion is made.

Browser verification covered WAV playback, a second bounce variation, the
two-second generated aim, category changes and Keep/Change toggles. Sources and
clip paths resolve locally. All four MP3 reels decoded without full-scale
overshoots. A relocated copy of the tools, in a path containing spaces and
invoked from an unrelated working directory, generated identical hashes for all
71 WAVs using the default output location. Audit records are in the ignored
preview `Evidence/` directory. These original preview checks did not validate
gameplay. Subsequent runtime and network checks are recorded in the
[integration report](BASKETBALL-AUDIO.md); phone-speaker listening remains open.

The APK at the start of this audio task was **89,200,326 bytes**, leaving
**10,799,674 bytes** to the strict 100,000,000-byte boundary. Concurrent gameplay
work subsequently produced the separate release recorded in
[BUILD-SIZE.md](BUILD-SIZE.md): the APK observed at preview verification is
**89,202,254 bytes**, with **10,797,746 bytes** to the hard boundary, and is
still above the 75 MB development target. The original preview task only added
source tools, documentation and ignored audition output. Its measurements do
not validate the subsequent gameplay integration; current player measurements
are recorded in [BUILD-SIZE.md](BUILD-SIZE.md).

Repository readiness and portable-path checks passed. No staging, commit or push
was performed. The reviewed one-off probes and relocated test fixture were
archived with the repository tool under
`Legacy/20261011-184719-937-basketball-audio-preview/`; current sounds, generators
and verification evidence remain in their active locations.
