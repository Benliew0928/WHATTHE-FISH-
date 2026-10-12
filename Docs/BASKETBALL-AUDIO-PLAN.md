# Basketball sound plan and source research

Research date: **11 October 2026, Malaysia time**. This document records the design
and source research. The subsequent [preview package](BASKETBALL-AUDIO-PREVIEW.md)
contains downloaded, edited and generated candidates. The user subsequently
approved them, requesting only a softer, lower-pitched Lock-on aim. See the
[implemented sound bank and action mapping](BASKETBALL-AUDIO.md). The research
and initial inventory below describe the checkout before integration.

## Recommended direction

Use recognizable recorded basketball, sneaker and net sounds, with restrained
synthetic feedback for the interface and playful sounds for the three props.
The [Rally court](../ArtSource/Basketball/Rally/README.md) has a honey-maple floor
and an open central roof. Prefer close, fairly dry recordings with a little
short reflection; a long, enclosed school-gym echo would contradict the scene.
Do not use concrete footsteps as the main court surface.

Free sources cover every major category at candidate level. Authentic sneaker
cuts, hard running, clean hand catches and consistent variations may still need
original recording. AI is an optional design tool for distinctive props and
replacement takes, not a prerequisite for completing the sound set.

**Prefer CC0 and original assets.** This project keeps delivery assets and
editable sources in Git. Some stock licences allow a finished game but forbid
sharing the audio with source files; those are a poor default for this workflow.

## Initial checkout audit, before audio integration

- No imported WAV, OGG, MP3, AIFF, FLAC or mixer files were found under
  `Game/Assets` during the audit. Basketball gameplay code has no audio playback.
- [CoveUI.cs](../Game/Assets/_Game/Shared/UI/CoveUI.cs) generates a 60 ms mono
  menu tap in `CoveSound`; `CoveButton` invokes it. Reuse or extend this palette.
- [MenuPreferences.cs](../Game/Assets/_Game/Shared/UI/MenuPreferences.cs) already
  persists Master and UI volume. Separate gameplay and ambience controls are
  proposed below, not implemented settings.
- [BasketballUIStyle.cs](../Game/Assets/_Game/Shared/UI/BasketballUIStyle.cs)
  creates ordinary Unity buttons.
  [BasketballButtonFeedback.cs](../Game/Assets/_Game/Shared/UI/BasketballButtonFeedback.cs)
  currently animates their press but does not play sound. The basketball
  interface therefore needs explicit audio coverage.
- Ball simulation belongs to the host/offline authority; guests present
  snapshots. Dribble phase, action timelines, net event sequences and prop
  deadlines provide useful synchronization points.
- Current basketball is shared practice with squad-aware props. Timed match
  periods, a basketball shot-clock rule, foul officiating and match wins are not
  reasons to add active buzzer/whistle/winner cues yet. Decorative clocks and
  scoreboards must not imply rules that do not run.

Gameplay references: [ball and court](BASKETBALL-GAMEPLAY.md),
[finishes](BASKETBALL-FINISHES.md), [defense](BASKETBALL-DEFENSE.md),
[props](BASKETBALL-PROPS.md), [mobile controls](BasketballUI/README.md).
Some historical sections describe older releases; actual source and current
prop/inbound behavior take precedence.

## Sound event inventory

P0 = first playable sound pass; P1 = polish after the main interactions work;
Future = enable only with the corresponding feature. Variation counts are
starting proposals, not separate recordings for every event. Several rows
deliberately share a small sample bank.

### Ball and basket

| Event | Sound and variations | When it should play | Priority |
| --- | --- | --- | --- |
| Controlled dribble, stationary/walking/running | Hollow rubber thump with a clear attack; 4 medium takes, 2 soft and 2 hard takes shared with free bounces | At the displayed ball's floor contact in the dribble cycle; cadence follows the actual motion | P0 |
| Loose ball / rebound / dropped ball | Same bounce bank with impact-dependent strength and a softer tail | Actual floor contact, including contacts after a miss; suppress resting chatter | P0 |
| Bounce pass | Same floor bank, normally stronger than a quiet dribble | At the real bounce, not at button release or predicted landing time | P0 |
| Catch / loose-ball pickup / intercepted pass | Short palm slap plus rubber/fabric handling; 3-4 takes | Confirmed possession transfer at the visible catch | P0 |
| Hand contact during dribble | Very quiet palm tick; 2-3 takes | At hand contact; optional at distance to avoid doubling the perceived bounce count | P1 |
| Gather and two-handed hold | Soft ball rub and jersey movement; 2 takes | Entering a shot/pass gather or standing inbound hold | P1 |
| Shot, chest pass, loft pass, layup release | Subtle hand release; 2-3 shared takes, optional light clothing swish | When the ball actually leaves the hands | P0 |
| Backboard strike | Firm, short panel thud distinct from floor and rim; 3 takes | Real board contact, scaled by normal impact speed | P0 |
| Rim strike / rim rattle | Rubber impact plus metallic ring; 3-4 takes | Actual rim contacts; short retrigger protection must still allow genuine double bounces | P0 |
| Clean net swish | Crisp nylon rush/snap; 3 takes | Valid downward crossing / corresponding net event at that hoop | P0 |
| Net brush without a basket | Quieter nylon rustle; 1-2 takes | Side/underside net touch; never a score confirmation | P1 |
| Dunk rim interaction | Short grip/metal rattle plus normal net sound; 2 takes | Actual dunk contact; stronger but not an explosion | P0 |
| Ball hits player, pad or support | Muted body/padded thump versus firmer structure impact; 2 shared takes each | Only when the actual contact exists; classify surfaces explicitly | P1 |
| Ball rolling to rest | Low rubber rubbing, optional short loop | Sustained rolling above a speed threshold; fade out at rest | P1 |
| Score awarded | Very short friendly confirmation, shared by 2- and 3-point scores with a small optional variation | Authoritative new score, once; separate from physical net rustle | P0 |

A bank shot can legitimately sound like board, rim, net, then a floor bounce.
Do not replace those independent contacts with one prerecorded complete shot.
A missed shot needs its actual contact sounds; it does not need a loud failure
tone on every miss. A green timing release is not proof of a made basket.

### Feet, body and defense

| Event | Sound and variations | When it should play | Priority |
| --- | --- | --- | --- |
| Walk on court | Soft sneaker sole on wood; 4 alternate takes | Visible grounded foot plants, with cadence based on locomotion | P0 |
| Run / sprint / Turbo run | Firmer sole contacts; 4 takes, sharing some walk material | Faster/heavier foot plants; Turbo follows the actual boosted movement | P0 |
| Hard stop / sharp turn / pivot | Short rubber chirp; 4-6 takes | Planted foot under braking or direction change | P0 |
| Defensive shuffle / shoe drag | Low scuff with occasional squeak; 2-3 takes | Lateral planted movement, independent from ordinary footfalls | P0 |
| Jump takeoff / layup gather | Sole push-off and quiet clothing movement | Actual takeoff / finish step; reuse footfall material | P1 |
| Landing after jump / block / dunk | Low double-foot thud; 2-3 takes | Airborne-to-grounded transition, scaled by downward speed | P0 |
| Steal attempt / block reach | Brief clothing/arm swish; 2 takes | Contact window/action animation; a missed reach has no ball slap | P1 |
| Successful strip / block | Sharp hand-on-ball slap, shared catch material | Confirmed contact and deflection | P0 |
| Carrier recoil / meaningful body bump | Restrained fabric/body thud | Confirmed reaction, not persistent overlapping capsules | P1 |
| Jersey rustle and exertion | Sparse cloth; optional original short effort sounds | Hard actions only; avoid constant breathing and repeated vocal grunts | P1 |
| Free-roam surface change | Wood, stone/concrete and grass families only where those surfaces exist | Ground probe/material beneath the foot | P1 |

Squeaks should be accents on friction events, not a loop attached to every
running player. Avoid footsteps while standing, sliding without stepping,
airborne, teleporting or moving a camera. Do not infer loudness solely from a
pressed movement key when the player is blocked by a wall.

### Props, boundaries and session events

| Event | Proposed sound identity | Trigger and listening audience | Priority |
| --- | --- | --- | --- |
| Prop spawn countdown | Soft holographic ticks, one sample pitched modestly; optional brighter last tick | Each new displayed countdown second near the spawn, never replay missed ticks on joining | P0 |
| Prop materializes | Short bright shimmer/pop | Once at the authoritative spawn position after zero | P0 |
| Prop collected / existing item replaced | Small pickup chime, 2 variations | World pickup cue plus quiet owner confirmation; avoid playing two identical full-volume copies | P0 |
| Prop unavailable / invalid activation | Muted rejection tick | Local requester only, rate-limited; item remains held where gameplay says so | P0 |
| Lock-on Shot starts | Focus click followed by a light rising tone | Accepted activation and the existing two-second stationary aim | P0 |
| Lock-on Shot fires | Compact release accent, then ordinary ball flight/contact audio | Actual automatic release, never an early score cue | P0 |
| Lock-on interrupted | Brief broken-tone/downward chirp | Confirmed block or possession loss; stop aiming tone immediately | P0 |
| Team Turbo starts | Warm upward zip / airy burst | Accepted activation; affected teammates receive a small local cue | P0 |
| Team Turbo active | Optional very quiet airy texture, with normal faster footfalls | Only while the five-second boost is active; avoid one loud loop per teammate | P1 |
| Team Turbo ends | Short fading/downward breath | Actual expiry; stop on session transitions | P0 |
| Portal Steal opens | Rounded spatial opening whoosh | At both real portal positions when transfer is accepted | P0 |
| Ball enters / exits portal | Suction dip at source, rounded pop at destination, normal catch | Align to the existing 0.7-second transfer animation | P0 |
| Portal closes / transfer aborts | Small tail or softened collapse | End or cancellation of transfer; release all loop voices | P1 |
| Player touches energy boundary | Soft energy ripple | Contact onset only, with cooldown; no continuous buzzing while pushing | P1 |
| Ball crosses court boundary | Optional subdued boundary ripple | Actual crossing; the wall is permeable to balls, so do not play a solid-wall impact | P1 |
| Ball recovery / standing inbound | Small reset/inbound notification | Confirmed recovery after the existing timeout, once; keep the visible instructions | P0 |
| Enter practice / leave / free-roam / travel | Shared short UI transition palette | State change; fade basketball ambience and stop abilities when leaving | P0 |
| Room join / ready / start / connection lost | Shared interface confirmation or attention cue | Real room event, not repeated network snapshots | P1 |
| Period/shot-clock buzzer, referee whistle, win/lose | Future dedicated cues | Only after those basketball rules/match states exist | Future |

### Every button and other UI interaction

Cover accepted interactions with **one shared palette of about 6-8 cues**, not
one unique file per button. All UI sounds are local 2D audio and obey UI volume.

| Controls | Cue behavior |
| --- | --- |
| Settings, guide, Rules/Controls/Props tabs, camera choices, close/back, Stay, Leave court, Free roam / Play basketball | One quiet click on accepted activation; navigation can use a softer back/close variation |
| Shoot/Block, Pass/Defend, Steal, Jump, Sprint, prop ability | One soft press cue at valid gesture start; gameplay sound follows the actual action |
| Held Shoot/Pass/Defend/Steal/Sprint | No repeating UI clicks while held; held-steal gameplay swishes follow real attempts |
| Dunk/Layup/pass-mode gesture changes | Optional tiny tick only when selection changes; no sound for every drag sample |
| Cancel target | Small cue when deliberately armed and a softer cancellation cue on release; rate-limit repeated boundary crossings |
| Toggles | One change click, optionally two pitches for on/off |
| Sliders | Sparse ticks on meaningful increments, or a short sound preview on release; never per-frame clicking |
| Disabled controls / blocked action | No normal confirmation; optional quiet rejected-action tick where the UI accepts a request and explains rejection |
| Mouse/keyboard/controller focus | Optional quiet focus tick; distinguish focus from activation and avoid pointer-hover chatter |
| Keyboard/controller activation | Same accepted-action cue as touch/mouse; route through common actions to prevent duplicate playback |
| Joystick and camera drag | Silent during continuous movement; movement itself produces world sounds |

Charge audio is optional and local. A subtle progress texture can accompany
charging, but do not make a harsh beep on every meter sweep. If an accessible
green-window cue is added, tie it to the same displayed phase and test device
latency; the current narrow timing windows cannot be assumed usable over
Bluetooth audio. Audio must supplement the visual meter and messages.

### Atmosphere

Use a quiet coastal air/surf bed appropriate to the island, with short
reflections near the court. A sparse bird call is optional. Add crowd murmur,
claps or cheers only if the scene actually represents an audience; the current
practice court should not sound like a packed professional arena. Crowd audio
must not contain unrelated dribbling, whistles or recognizable commentary that
players could mistake for live actions. Music and announcer dialogue are
separate optional work, with their own size allowance.

## Free source shortlist

These are **candidates for audition**, not claims that recordings match in tone,
noise level or acoustic space. A free Freesound account is needed for the
original downloads on the pages that show a login requirement. Use the original
files rather than recording their web previews. The CC0 designation below was
checked per linked sound, not inferred for the entire website.

| Need | Direct source | Published licence / format | Suitability and editing work |
| --- | --- | --- | --- |
| Dribble, hand/floor contacts and bank/net alternatives | [OBXJohn: shooting and dribbling (WAV)](https://freesound.org/people/OBXJohn/sounds/262508/) | CC0; 44.213 s, WAV, 48 kHz, 16-bit stereo | Prefer this lossless version over its MP3 equivalent. Cut individual contacts. Author reports gym-light hum; indoor echo may be too wet for this court. Hand contacts need listening confirmation. |
| Backboard | [PerMagnusLindborg: Basketball backboard](https://freesound.org/people/PerMagnusLindborg/sounds/324247/) | CC0; 0.616 s, WAV, 48 kHz, 24-bit stereo | Especially direct candidate. Check attack, panel material and tail against our board. |
| Rim | [reedhos: basketball](https://freesound.org/people/reedhos/sounds/244348/) | CC0; 7.347 s, WAV, 44.1 kHz, 16-bit mono | Author explicitly recorded a ball hitting the rim several times on a phone. Extract separate hits; check clipping/background. |
| Clean swish | [vhschool2019: Basketball Swish](https://freesound.org/people/vhschool2019/sounds/716930/) | CC0; 0.5 s, WAV, 44.1 kHz, 24-bit stereo | Compact first candidate; recorded on iPad. Check whether trimming preserves the full tail. |
| Net variations | [21100495: basketball through hoop](https://freesound.org/people/21100495/sounds/655323/) | CC0; 11.604 s, WAV, 48 kHz, 24-bit stereo | Several perspectives; extract matching close takes rather than mixing all perspectives. |
| Outdoor alternate | [fattirewhitey: Outdoor Basketball Shot](https://freesound.org/people/fattirewhitey/sounds/381410/) | CC0; 12.844 s, WAV, 44.1 kHz, 16-bit stereo | Nylon-net candidate; includes asphalt bounces, wooden board and road noise. Do not use its asphalt contacts as the maple-court default. |
| Wood-court walking | [Greenbird11: Sneakers](https://freesound.org/people/Greenbird11/sounds/618816/) | CC0; 42.631 s, WAV, 48 kHz, 24-bit mono | Actual sneakers on wood. Cut isolated footsteps; hard running still needs separate audition/recording. |
| Shoe friction | [Christophersaa365: Shoe Squeak](https://freesound.org/people/Christophersaa365/sounds/703188/) | CC0; 21.240 s, WAV, 48 kHz, 24-bit stereo | Squeaky walking shoes; not proven to be an athletic pivot on maple. Accept only if it sounds convincing in context. |
| Designed skid fallback | [shatterstars: Rubber Skid](https://freesound.org/people/shatterstars/sounds/492071/) | CC0; 85.303 s, MP3, 44.1 kHz stereo | Shoe rubbing a wet tire, so this is a designed layer rather than authentic court foley. Use sparingly or record a real court skid. |
| Menu/buttons | [Kenney Interface Sounds](https://kenney.nl/assets/interface-sounds), [UI Audio](https://kenney.nl/assets/ui-audio) | CC0; 100 and 50 files respectively | Select a small coherent set; compare with the existing generated menu tap before replacing it. |
| Footstep/impact alternatives | [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds) | CC0; 130-file pack | Audition material-specific candidates; generic impacts are fallback layers, not proof of basketball authenticity. |
| Props and countdowns | [Kenney Digital Audio](https://kenney.nl/assets/digital-audio), [Sci-fi Sounds](https://kenney.nl/assets/sci-fi-sounds) | CC0; 60 and 70 files respectively | Edit/layer a small selection for pickup, focus, boost and portal signatures. Do not import whole packs. |

[CC0's official deed](https://creativecommons.org/publicdomain/zero/1.0/) permits
copying, modification and distribution, including commercial use, without
permission. Keep provenance and optional credits anyway. CC0 does not establish
that every uploader recorded everything they claim; audition for unrelated
music/speech and keep only plausible original recordings.

Other useful libraries, with a different delivery tradeoff:

| Source | What was found | Licence / recommendation |
| --- | --- | --- |
| [Mixkit basketball collection](https://mixkit.co/free-sound-effects/basketball/) | 18 listed effects, including floor bounces, catches, hand contact, net and buzzer | Its [SFX licence](https://mixkit.co/license/modal/sfxFree/) explicitly allows commercial games but forbids distributing sounds on their own or with source files. Good finished-product option; not the default for this source-controlled asset workflow. Its [FAQ](https://mixkit.co/free-sound-effects/) says attribution is not required. |
| [ZapSplat: backboard/rim recording](https://www.zapsplat.com/music/basket-ball-hit-backboard-rim/) | A specific Blastwave FX board/rim candidate | [Standard licence](https://www.zapsplat.com/license-type/standard-license/) allows incorporated game use; Basic requires credit and restricts standalone sharing. Use only with a compatible asset-sharing arrangement. Do not assume paid access removes every redistribution restriction. |
| [Sonniss GDC licence](https://sonniss.com/gdc-bundle-license/) | General fallback for foley, atmosphere and designed effects; no exact basketball file verified here | Commercial project use without attribution, with standalone redistribution restrictions. Avoid downloading huge bundles merely to find a handful of clips. |

Reject broadcast/NBA/YouTube rips and meme soundboards with unclear rights.
Avoid noncommercial-only samples for a project that may later be published or
sold. If choosing CC BY instead of CC0, record the exact licence version,
creator, source and modifications, and include the required credits.

## AI and other ways to fill gaps

| Method | Cost/licence finding on research date | Best use here |
| --- | --- | --- |
| Original foley recording | No stock fee with existing equipment; record the team's own performance and keep contributor permission/provenance | Most dependable route to consistent shoes, running, ball handling, catches, board/rim variations and matching court acoustics |
| [jsfxr](https://sfxr.me/) or simple original synthesis | Its [feature comparison](https://pro.sfxr.me/features-and-pricing) lists free WAV export and unrestricted commercial use | Clicks, countdowns, rejected actions, pickups, score pips and small prop tones. Soften the retro character for this visual style. The current game already demonstrates generated UI audio. |
| [Stable Audio](https://stability.ai/stable-audio) hosted generation | Current [licensing guidance](https://kb.stability.ai/knowledge-base/stable-audio-commerical-and-usage-licensing) explicitly permits commercial embedded game audio, including the Free plan, subject to its terms. Free generation quotas were not verified. | First AI trial for custom portal, turbo, focus and ambience textures; re-check the account's terms at generation and save the version/date |
| Stability self-hosted models | [Core-model list](https://stability.ai/core-models) and [Community licence information](https://stability.ai/license) govern the exact model; commercial registration and revenue eligibility apply. Do not assume every similarly named model has identical terms. | Offline authoring if local setup is worthwhile; no model or generation dependency belongs in the APK |
| [ElevenLabs Sound Effects](https://elevenlabs.io/sound-effects) | Main SFX page and [publishing guidance](https://help.elevenlabs.io/hc/en-us/articles/13313564601361-Can-I-publish-the-content-I-generate-on-the-platform) say Free is noncommercial; paid generation has commercial rights under the applicable terms, excluding Beta restrictions | Convenient optional paid fallback. Do not treat its marketing collection pages saying “free” as commercial clearance. Upgrading later does not retroactively clear previously free-generated outputs according to the help page. |

Generate offline, edit, then ship only the accepted audio. AI can produce
unwanted extra hits, silence, music, changing timbre or inconsistent tails;
request one event per output and audition repeated playback. A whole generated
basketball scene cannot follow unpredictable physics. For simple clicks and
beeps, parameterized synthesis is easier to control than prompt generation.

Suggested original prompts (set duration separately if supported):

- **Ball:** “One isolated basketball bounce on a polished maple court. Close
  perspective, firm rubber attack and hollow body, short natural decay. No
  voices, footsteps, music, crowd or additional bounces. About 0.5 seconds.”
- **Shoe:** “One short athletic sneaker pivot on polished maple. Quick rubber
  chirp with a subtle sole scuff, close and dry, no footstep sequence, no voices
  or music. About 0.3 seconds.”
- **Board:** “One basketball striking a rigid backboard, rounded panel thud with
  a short frame vibration. Close isolated foley, no glass breaking, no rim
  strike, no crowd or bounce sequence. About 0.5 seconds.”
- **Lock-on:** “Friendly sports-game focusing energy, a gentle rising tone that
  lasts two seconds and resolves in a tiny click. No gunfire, speech, music or
  explosion.” Produce interruption and release separately.
- **Turbo:** “Playful short air-powered acceleration burst for a colourful
  basketball game. Light upward whoosh, clean compact finish, no motor engine,
  speech or music. About 0.5 seconds.”
- **Portal:** “Small magical sports-game portal opening with a rounded airy
  whoosh and soft resonant pulse. Friendly, close, no horror drone, voices or
  explosion. About 0.4 seconds.” Produce entry and exit separately so they can
  follow the transfer timing.

For original recording, use the same ball, shoes and court where practical.
Record 8-12 separated takes per action with silence between them: low/medium/
hard bounces, catches, hand slaps, shoe walks, runs, cuts and landings, plus
board/rim/net contacts. Capture room tone. Use a fixed mic position, leave peak
headroom and disable automatic processing if the recorder allows it. Preserve
the original WAV masters; cut variations and gently clean rumble/hum rather
than making every impact identical. A phone can supply auditions; recording
quality still needs listening checks.

## Integration design

This is proposed implementation work, not changes made by this research task.

| Existing location | Appropriate hook and caveat |
| --- | --- |
| [BasketballInteraction.cs](../Game/Assets/_Game/Sports/Basketball/BasketballInteraction.cs) | Use presented `DribblePhase` contact crossings for carried dribbles. Held balls do not provide ordinary dynamic floor collisions. In `OnCollisionEnter`, capture/classify impact before early returns such as bounce-pass handling. |
| [BasketballBall.cs](../Game/Assets/_Game/Sports/Basketball/BasketballBall.cs) | Host/offline owns physical impacts. Guests need event data; their kinematic ball cannot independently reproduce all contact sounds. |
| [BasketballHoop.cs](../Game/Assets/_Game/Sports/Basketball/BasketballHoop.cs) and [BasketballScoring.cs](../Game/Assets/_Game/Sports/Basketball/BasketballScoring.cs) | Reuse net kinds/sequences for visual synchronization; new score confirmation must follow authoritative scoring. The latest net snapshot is not a lossless log of every rim impact. |
| [BasketballMotion.cs](../Game/Assets/_Game/Sports/Basketball/BasketballMotion.cs), [Athlete.cs](../Game/Assets/_Game/Shared/Player/Athlete.cs) | Foot-plant/landing presentation and accepted action phases drive foley; do not attach an arbitrary running loop to input. |
| [BasketballProps.cs](../Game/Assets/_Game/Sports/Basketball/BasketballProps.cs) | Accepted inventory/effect revisions and deadlines drive pickup, two-second aim, five-second Turbo and 0.7-second transfer; cancellation must stop sounds. |
| Basketball UI builders and gesture handlers | One common UI feedback entry per accepted interaction; keep the same-finger hold/cancel semantics and avoid duplicate onClick/pointer handlers. |

Implementation rules:

1. Keep one audio listener. Use local 2D UI/status cues and positioned mono
   world sounds at the ball, hoop, feet and portals. Tune audibility for eye,
   follow and elevated court cameras; a higher camera should not silence the
   sport. Disable or strongly reduce Doppler unless listening proves it helps.
2. Distinguish floor, board, rim, net, padding and player materials with explicit
   authored tags/components. Map normal impact speed to volume and sample tier;
   ignore tiny resting contacts. Starting retrigger protection of 40-80 ms per
   contact family is a tuning proposal, not a verified constant.
3. Randomize sample choice without immediately repeating the same take. Keep
   pitch variation small, roughly 3-5%, and avoid applying it to an ability's
   timing-critical countdown. Real variation is better than extreme pitch shifts.
4. Pool sources. Start with roughly 16 world voices and four UI/status voices,
   then profile on Android. Cull quiet distant footsteps first; preserve nearby
   ball, net, score and meaningful ability cues. Cap concurrent squeaks.
5. Use host-issued unique event IDs for authoritative contacts/results. Send
   compact IDs, positions, time, material and strength, not audio bytes. Schedule
   remote sounds against the presented timeline. Keep a short bounded event
   buffer or suitable event transport so multiple contacts between snapshots
   are not overwritten. Critical score/prop events need reliable delivery and
   deduplication; frequent impact events can tolerate bounded loss.
6. Reconstruct regular dribbles/footsteps locally from synchronized presentation.
   Seed history on joining and resets; never burst-play old bounces, scores or
   countdown ticks. A host must not hear its own event again through replication.
7. Play local UI press immediately; confirm world outcomes only when accepted.
   Predictive foley, if used later, must reconcile without replaying a second
   catch/activation. Room rejection must not sound like successful prop use.
8. Keep game SFX, ambience, UI and optional music on independent gain paths under
   Master. A mute must affect generated audio too. Avoid applying Master twice.
   Preserve existing saved preferences while adding new controls.
9. Opening settings in a network room does not pause the shared world. Decide
   local ducking deliberately; do not use a global gameplay pause as an audio
   shortcut. Stop/fade loops on unload, disconnect, travel, cancellation and
   application suspend; resume without stale event playback.
10. Set relative levels in play: ball first, feet lower, squeaks as occasional
    accents, ambience quieter. Let net and score remain readable without loud
    music/crowd. Keep headroom when multiple players and props overlap. All
    important information retains its visible counterpart.

## APK and asset budget

The existing `Builds/Android/WhatTheFish-release.apk` was inspected directly:
**89,200,326 bytes**, matching the latest measured record in
[BUILD-SIZE.md](BUILD-SIZE.md). The difference to the strict 100,000,000-byte
boundary is **10,799,674 bytes**; equality would fail. The development target
is already exceeded by **14,200,326 bytes**. That reserve also needs to cover
other gameplay, art and animation.

Proposed audio allocation: aim for **800,000-1,500,000 additional APK bytes**
for the basketball SFX package, with an investigation/reduction gate above
**2,000,000 bytes**. These are planning targets, not measured build results or
permission to spend all remaining headroom. Seek offsetting reductions where
possible without damaging visual/audio quality.

- Plan approximately 60-80 short edited variations across the event families,
  with extensive reuse, and at most one or two compact ambience/effect loops.
  Start with core contacts, locomotion, props and UI before ambience/voices.
- Preserve full-quality editable masters outside the Unity delivery tree.
  Store only approved trims in the game's referenced audio bank. Avoid importing
  complete packs, alternate exports, long recording sessions or unused music.
- Use mono for positioned foley. Trial 32 kHz delivery for impacts/steps and
  retain 44.1/48 kHz where net/squeak detail needs it. Check phone speakers and
  headphones before accepting any reduction.
- For orientation only, 50 seconds of 32 kHz mono 16-bit PCM is 3,200,000 bytes
  before headers. Compressed clips, serialized assets, decoded RAM and final
  APK ZIP entries are different measurements.
- Unity's [Audio Clip documentation](https://docs.unity3d.com/6000.3/Documentation/Manual/class-AudioClip.html)
  supports PCM for short high-quality cues, ADPCM for frequent noisy impacts,
  and Vorbis for longer compressed content. Try ADPCM for footsteps/bounces;
  compare against Vorbis/PCM on both size and audible attack. Preload short
  latency-sensitive cues; reserve streaming for genuinely long material.
  Check decompressed RAM and audio CPU separately from APK bytes.
- Use one modest shared reflection treatment if needed; do not add a large
  middleware package or convolution library for this task. Keep AI generation
  and its dependencies entirely outside runtime.

For each accepted source, record author/title, original page and download date,
exact licence URL/version, original checksum, intended events, trim/processing,
derived checksum and attribution. Use repository-relative paths in manifests.
Retain licence evidence and original recordings only where their terms permit.
Add appropriate audio extensions to the team's Git LFS rules when audio is
actually introduced; the current `.gitattributes` does not list audio formats.
No attributes or asset files were changed during this research.

## Production and verification order

1. Audition the linked CC0 core recordings against the current court and choose
   a coherent small bank. Record missing catches, athletic cuts and running
   where candidates do not match. Reject long echoes, clipping, speech and
   persistent hum that cannot be cleaned without damage.
2. Integrate bounces, hand catches, board, rim, net, footfalls, landings and UI.
   Verify dribble contacts and physics-driven contacts use different event paths
   without duplicating sounds.
3. Add the three distinct prop identities, the actual countdown/inbound events,
   and interruption/expiry behavior. Audition synthetic versions before spending
   time or credits on AI.
4. Add restrained atmosphere and optional cloth/rolling detail only after the
   main mix is readable. Test repeated actions for fatigue, not just one clip.
5. Offline checks: soft/hard bounces, dribble while standing/walking/running,
   charge/cancel, each pass type, catches, steals/blocks, board-rim-net chains,
   clean swish, side-net touch, dunk/layup/landing, boundary and inbound, all
   props including rejection/interruption, every UI gesture, mute and resume.
6. Host/guest checks: matching outcomes, no doubled host sounds, guest impacts
   audible, distinct quick successive rim contacts, late join without replay,
   interrupted props stopping on all peers, disconnect/rejoin and shared travel.
7. Device listening: real Android speaker, wired headphones where available,
   Bluetooth latency, each camera mode, many moving players and overlapping
   props. Check clipping, missing attacks, DSP/CPU, memory and pause/resume.
   A Windows playback check cannot qualify phone sound latency or performance.
8. After implementation, run the relevant existing feature checks and
   `Tools/Build/Build.ps1 -Target AndroidSubmission`. Compare actual APK bytes,
   inspect `Builds/SizeAudit/latest/`, and update measured size/audit records.
   Deliver the complete tested Windows player to `Builds/WindowsFinal/`, with
   current evidence and `LATEST-BUILD.txt`, following the repository's archive
   workflow for superseded output. Run `Check-TaskReady.ps1 -RequireApk` for
   that build delivery. Leave Git publishing to the user.

This research task changes documentation only. It requires no new Unity build
or Windows player and does not validate unrelated working-tree gameplay edits.
The existing APK is a baseline measurement, not evidence for future audio work.
