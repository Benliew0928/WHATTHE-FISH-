# Golf and Fishing audio plan

18 October 2026. **All sounds approved for integration.** This original plan follows the
current code and the newest gameplay records, including changes since the
Basketball/Football previews. See the [integration record](GOLF-FISHING-AUDIO.md)
for implementation and current validation status.
See the [listening preview](GOLF-FISHING-AUDIO-PREVIEW.md).

## Current gameplay and existing audio

Golf has five ordered holes, held/released power, automatic Putt near the cup,
real course collisions, water recovery, a five-number start countdown and GO,
and a summonable, driveable buggy. Completing Hole 5 starts a shared 30-second
deadline; the first finisher is not necessarily the winner. There are no current
Golf collectible combat/boost props to invent sounds for. The buggy is the
interactive equipment requiring its own feedback.

Golf already synthesizes swing, putt, turf, sand, cup, finish, charge and surface
footsteps in `GolfFeedbackSounds.cs`. `GolfFeedback.cs` synchronizes its cosmetic
events; `GolfFootsteps.cs` uses planted feet and support surfaces. Your supplied
GO and three walkway clips are already delivered. Those **four WAVs remain
byte-identical** in this audition. The removed full-power ready beep remains
excluded. Current behavior is in [Golf feedback](GOLF-FEEDBACK.md),
[rules](GOLF-MINIGAME.md) and [buggy controls](GOLF-CART.md).

Fishing uses **continuous Hold the Glow Left/Right**, rather than the superseded
timed-tap experiment. Cast leads to float entry, waiting, the 1.8-second Hook
window, a directional fight, and catch/escape/snap. Correct holding winds line;
a strong fish can pull against the reel drag. Cooling clears below 58% tension;
an unattended fight ends after 30 seconds. The six skills are Sonar, Cool Spool,
Flippers, Power Reel, Bubble Guard and Quick Lure. Shore supplies refill in 22
seconds; treasure takes six seconds to recover with a shared 28-second cooldown.
Round, three-round Cup, Crew and Wanted all need distinct state feedback.
See [controls](FISHING-COUNTERSTEER.md), [abilities](FISHING-ABILITIES.md) and
[match rules](FISHING-MINIGAME.md).

`FishingFeedback.cs` currently generates eight short feedback cues and a reel
loop for the local accepted record. It already seeds late/resumed snapshots
silently and avoids stale bite alerts. Preserve those safeguards. Some Fishing
UI calls route to `FootballAudio.PlayMenu`, whose Football context gate does not
make that a working Fishing sound hookup. Reusing files requires an explicit
Golf/Fishing UI route after approval; replace existing playback paths where
appropriate so old and new sounds do not both fire.

## Complete action coverage

The soundboard provides **81 families / 115 clips**. A family can serve several
actions with matching meaning. Its Source & processing panel records provenance,
edits and whether it is reused, recorded or generated.

| Golf action | Audition / proposed synchronization |
| --- | --- |
| Aim accepted, cancel aim, charge cancelled | Shared Select / Back; no false strike on rejected or cancelled requests |
| Holding Swing/Putt power | Very soft low charge loop; track displayed power; fade on release, cancel, focus loss or travel; no full-charge beep |
| Downswing | Two rod-swish edits as club air foley, just before contact; little or none for Putt |
| Club contacts ball | Actual driver recording; three isolated actual putter contacts; release and contact remain separate |
| Ball lands / bounces | Small dry turf, sand, stone and timber contacts; gain by normal impact speed; cooldown prevents resting chatter |
| Ball collides with another ball | One hard-ball tick per resolved pair, not one from each collider |
| Ball rolls | Quiet close loop tied to observed grounded speed; rough/sand can filter the same texture; stop on sleep/flight/teleport |
| Pond or sea entry | Water splash at water-plane crossing, followed by recovery; do not repeat on each submerged frame |
| Correct cup | Actual cup rattle, then quiet owner-only hole acknowledgement, including a helper completing another owner's ball |
| Wrong cup / out-of-course recovery | Shared soft interruption/reset cue; no invented penalty-stroke sound |
| All five holes completed | Personal completion flourish; separate from final standings |
| First finisher / last seconds / standings | Neutral first-finisher notice; restrained final countdown; settled win/loss/tie; DNF is not a successful finish |
| Opening countdown / GO | Current beep design retained in a recreated audition; supplied GO recording retained exactly |
| Buggy summon / recall | Shared spawn on successful summon; short recall whoosh, including accepted aim recalling the buggy |
| Drive / exit | Seat/fabric foley; no car-door slam for an open cart |
| Drive, coast, brake | Quiet designed electric-style motor, grass/sand tyre loop and stopping scrub; follow actual motion, not requested throttle against a wall |
| Reverse / obstacle / hard landing | Optional one-time reverse chirp; soft chassis bump only at actual energetic contacts |

Golf decorative flags do not need a permanent flap loop. If a future flagpole
gets a physical collision surface, give that contact a small metal tick then;
do not add phantom collisions or alter the course for sound. Airborne balls
also need no loud permanent whistle. The current trail is visual feedback.

| Fishing action | Audition / proposed synchronization |
| --- | --- |
| Target selection / invalid cast / rod selection | Select or Error only at deliberate action edges; no reticle-hover ticking |
| Cast | Two isolated rod swishes at accepted rod release |
| Float lands | Small separate splash at entry, about 0.65 s after cast starts |
| Waiting | Shore ambience only; no repeating artificial waiting beep |
| Bite / Hook | Two bite-alert alternatives to compare; one local ! prompt; short accepted-hook line set |
| Correct continuous holding | Actual mechanical winding loop; no rhythmic tap-game grading or repeated click on lease renewals |
| Fish pull / line slip / strain | Separate drag loop during actual slip and occasional nylon creak under load |
| Required Left/Right side changes | Quiet, distinct pitches; readable in mono without relying on stereo position |
| Release to cool / cooled | Soft falling warning when the cooling state starts; quiet recovery note once below its hysteresis threshold |
| Fish breaks the surface / surges | Three splash sizes at visible water crossings, rate-limited; no voices assigned to fish species |
| Catch | Physical water/body landing plus shared confirmed-score cue; reward only once, not while digits animate |
| Wanted +3 / minute change | Separate light bonus sparkle if actually earned; subtle once-per-minute target change |
| Missed bite / escaped / line snapped | Downward bubbles for missed bite; distinct short dry snap; timeout can use shared interruption |
| Cancel / focus loss / disconnect | Back for deliberate cancellation; stop all loops; no false failure, catch or expiry cue |
| Fish respawn | Optional quiet nearby visible bubbles; distant or underwater respawns remain silent |
| Shore supply / choose item / item awarded | Shared nearby Spawn and accepted Pickup; opening the picker is UI Open, not a pickup |
| Treasure reservation / six-second recovery | Wooden/rod start cue; reuse winding loop; cancel fades it immediately |
| Treasure emerges / awarded gear | Water draining and lid clack, then shared Pickup; no points reward because treasure grants equipment |
| Sonar | Watery ping at acceptance and restrained repeats during its 8 seconds, for actual recipients |
| Cool Spool | Airy cooling activation; ordinary mechanical reel continues |
| Flippers | Reuse approved speed-boost activation; footsteps reflect actual increased speed |
| Power Reel | Mechanical engagement; restrained speed change in the existing reel loop during its four seconds |
| Bubble Guard | Bubble formation, then a separate single absorption pop when consumed by a strong surge |
| Quick Lure | Tackle jingle on activation; normal cast and float entry when consumed, without a second reward sting |
| Natural skill expiry | Shared soft expiry for the local effect; consumed Bubble Guard uses its pop; resets end silently |
| Ready / countdown / Round / Cup / Crew | Shared UI/countdown, short Fishing start/end cues, settled result palette; Cup champion has a fuller final flourish |
| Crew progress and local-best statistics | Existing catch/reward cue is sufficient; avoid extra sounds for every animated row, progress update or statistic |

| Shared movement, environment and UI | Selection |
| --- | --- |
| Grass walk/run, grass jump landing | Football's approved takes, unchanged; actual foot plants/grounded landing |
| Wood pier, sand, shallow water | New short recorded foley; source caveats exposed in audition; surface selection avoids gym squeaks outdoors |
| Stone/hard path | Golf keeps the supplied walkway takes; Fishing has a new dry contact option |
| Clothing / jump / equipment lift | Basketball's quiet cloth cue; one edge, not while holding a pose |
| Air and shoreline | Shared coastal air plus optional quiet recorded shore wash; one listener bed, not one per player |
| Action buttons | Shared Click on one physical/touch/key press; held inputs do not click every frame |
| Ready, Start, save kit, rematch, confirmed Leave | Confirm; Toggle for actual two-state changes |
| Cancel, Back, Keep playing | Back; action cancellation stays distinct from failure |
| Camera, modes, skill cards, tutorial pages | Select; no hover sound |
| Settings, guide, skill picker, results, exit dialog | Open/Close once at the actual panel transition |
| Rejected action / full inventory / occupied buggy | Error, rate-limited; decorative/disabled controls and continuous slider dragging stay silent |

## Reuse choices

**30 approved Basketball/Football clips** are copied unchanged for review: the
eight UI cues, grass walk/run, turf landing, cloth, coastal air, countdown,
spawn/pickup, interruption, speed activation/expiry, score and win/loss/tie.
The preview also retains **four approved Golf clips**: GO plus three walkway
steps. Future integration should reference these original assets, not publish
duplicate copies under each sport.

Basketball bounces, court squeaks, backboard/rim hits and net swishes are poor
matches for a small Golf ball or wooden Fishing pier. Football kicks are too
hollow for putter contact. The Basketball two-second Lock-on is not a match
for Golf's repeating held charge; the new charge bed is shorter in character,
lower in pitch and substantially quieter. Portal/Wall effects have no equivalent
current Golf/Fishing mechanic. Retain their meaning in their existing sports.

## Free sources researched and selected

The downloader verifies CC0 on each individual uploader page, saves that HTML
as evidence, and hashes the downloaded file. These are publicly offered **HQ MP3
audition copies**, not lossless original WAV downloads; conversion to WAV cannot
restore lost detail. Original Freesound downloads require login. No account,
purchase or paid API was used.

| Recording | Author and primary source | Intended use |
| --- | --- | --- |
| Driver hitting golf ball | [Scottrex05](https://freesound.org/people/Scottrex05/sounds/593483/) | Full swing contact |
| Putt | [170129](https://freesound.org/people/170129/sounds/408260/) | Three separate putter contacts |
| Golf ball into hole | [inbeeld](https://freesound.org/people/inbeeld/sounds/21878/) | Cup rattle |
| Fishing rod swish | [Mrthenoronha](https://freesound.org/people/Mrthenoronha/sounds/371313/) | Rod cast; trimmed club-air foley |
| Spinning reel | [tosha73](https://freesound.org/people/tosha73/sounds/509902/) | Winding / treasure recovery loop |
| Fly-fishing reel fast pull | [paulprit](https://freesound.org/people/paulprit/sounds/507070/) | Line slipping against drag |
| Nylon line friction | [kyles](https://freesound.org/people/kyles/sounds/450849/) | Quiet loaded-line creak |
| Water drop / splash | [bxyorna](https://freesound.org/people/bxyorna/sounds/410335/) | Float, object, fish and wading edits |
| Gentle ocean waves | [kkenny101](https://freesound.org/people/kkenny101/sounds/852826/) | Shore ambience |
| Footsteps on wood | [sillygrizzlies](https://freesound.org/people/sillygrizzlies/sounds/635058/) | Pier steps / wood contacts; indoor source, shortened tail |
| Sand steps beside sea | [Daphne_in_Wonderland](https://freesound.org/people/Daphne_in_Wonderland/sounds/383488/) | Sand steps / granular contacts; some original sea texture |

[pfranzen's combined putt/cup](https://freesound.org/people/pfranzen/sounds/512505/)
was considered, then replaced with its separately sourced contact/cup inputs so
they can follow actual gameplay timing. That discarded candidate is archived.
[BigSoundBank's fishing reel](https://bigsoundbank.com/fishing-reel-fast-s1435.html)
is another CC0 option; the selected winding/drag recordings provide separate
mechanical roles. Broad Golf-cart search results included paid libraries and
aggregators. No suitable isolated free cart motor was selected from those results.

The eight shared UI files retain their earlier
[Kenney Interface Sounds](https://kenney.nl/assets/interface-sounds) provenance.
The supplied Golf recordings retain their own user-provided provenance and are
not labelled CC0. All exact source URLs, licenses, downloaded bytes, edits and
hashes are in the preview manifests.

## Generated gaps and mix direction

Use original deterministic local synthesis for the soft Golf charge, motor and
tyre textures, small-ball contacts, warning/recovery tones, six-skill accents,
wooden lid/seat accents and new musical confirmations. These are **generated
effects, not AI output or claimed real-world recordings**. This keeps timings
editable and avoids account/subscription dependencies. An AI service remains an
optional later alternative if a selected generated sound fails the listening
review; none is needed to fill the current preview.

Keep natural contacts in front, UI short, ambience quiet, and charge/reel/motor
loops below important alerts. The new charge has a 0.065 peak versus a 0.48
Golf hit; these are sample peaks, not a perceptual loudness guarantee. Retained
GO and walkway levels are preserved for comparison. Avoid stretching alerts
into long jingles or stacking score, bonus and result flourishes simultaneously.
Duck ambience/reel under bite and critical tension information. No commentary,
crowd loop, continuous music or imaginary door/prop sounds are proposed.

## Integration plan after approval

1. Preserve the existing accepted-event/pose scheduling. Replace only the selected
   cue sources; wire the currently missing collision, buggy, skill and UI events.
2. Golf contact/cup outcomes follow host confirmation and the local ball owner;
   new impacts need pair/speed repeat gates. Pose-based feet stay tied to rendered
   planting. Buggy audio follows displayed travel, not throttle intent alone.
3. Fishing uses accepted phase/sequence/item changes. Guard against skipped
   snapshots and late/resumed records; never replay old casts or expired ! cues.
   Keep action hints local; only nearby physical splashes/steps are positional.
4. Master, SFX, UI and Ambience gains should apply to existing and new sources,
   including active loops. Context exit, pause, focus loss, round reset, travel
   and disconnect stop loops and clear pending cues. Use bounded voice pools and
   retain important alerts when incidental contacts overlap.
5. Validate actual gameplay offline and with host/client, then deliver a complete
   WindowsFinal player and fresh AndroidSubmission APK. The current limit is
   strictly **1,000,000,000 bytes**; the former 75 MB / 100 MB budgets are retired.
   Keep editable masters outside Unity and reference shared delivery clips.

For this approval phase, no game, Unity import, Windows player or APK is changed.
The existing APK was recorded as **83,309,709 bytes**, solely as an opening
baseline; it is not validation of this unintegrated audition.
