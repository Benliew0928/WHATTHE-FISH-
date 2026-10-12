# Football sound plan

Status: **all preview sounds approved** — 12 October 2026.
Integration and build verification follow in [the delivery record](FOOTBALL-AUDIO.md).
Open the [preview guide](FOOTBALL-AUDIO-PREVIEW.md) for listening and feedback.

## What the current game needs

This plan follows the implemented [match rules](FOOTBALL-MATCH.md),
[movement and pressure](FOOTBALL-PACE-AND-PRESSURE.md),
[abilities](FOOTBALL-ABILITIES.md) and [football UI](FootballUI/README.md).
The pitch is outdoors on grass. Explore Offline has ball play and props but
does not award goals or run a formal match. Formal play has a team-selection
phase, kickoff countdown, regulation, a continuous golden-goal period when
tied, and results. Sounds must respect those modes.

| Action or state | Proposed sound and timing |
| --- | --- |
| Quick Kick release / short pass | `kick-light`, on accepted ball release; the existing Kick action supplies power, not a new pass button. |
| Charged shot | `kick-shot`, scale contact level with actual power. Compare `kick-alt` before choosing. No two-second Lock-on tone: football reaches full charge in one second and has no Lock-on prop. |
| Carried-ball dribble | `dribble-touch` at each visible foot contact. The carried ball follows the player kinematically; collision callbacks alone cannot supply this. Never bounce it audibly while standing still. |
| Trap / possession acquisition | `receive` once on new control, including Portal completion. A steal or deflection can use `deflection`; avoid playing both loudly for the same contact. |
| Free ball landing / bouncing | `turf-bounce`, strength from impact speed, a short repeat gate and near-silent small settling contacts. |
| Free ball rolling | Optional quiet `turf-roll` loop, crossfade with speed and ground contact; stop when held, airborne or reset. |
| Post / crossbar | `goal-post` only for frame contact. Two designed versions to compare, distinct from basketball rim and backboard. |
| Ball enters / brushes net | `goal-net` for the first stronger net contact, `net-brush-football` for gentle side/roof/back touches. Net noise can occur without a goal. |
| Boundary containment | Shared `boundary` once for meaningful contact. Current containment does not award throw-ins or corners. |
| Walk / run / Dash / Turbo | `grass-walk` or `grass-run` on foot plants, with different cadence and limited variation. `trail-alt` is an outdoor running comparison. |
| Turn / stop / Pressure shuffle | `grass-cut` for real footwork; Pressure is positional defense, so no continuous lock-on alarm. |
| Tackle slide | `tackle-slide` when the slide begins; about 0.43 seconds to match the current 0.425-second action. |
| Successful tackle / player contact | `deflection` for the ball, shared `body` for player collision, `turf-fall` for a grounded fall. Whiffs do not play success contact sounds. |
| Jump / landing / fabric | Shared `cloth` for takeoff, `turf-land` on actual landing. Shared `swipe` is an optional restrained action accent, not required on every input. |
| Dash / stamina | Quiet `dash-start` once when Dash becomes active; `stamina-empty` once on exhaustion. `stamina-ready` is optional and should require a previous exhausted state. No meter-tick loop. |
| Prop countdown / materialization | Shared `countdown` once per displayed second of the five-second countdown; `spawn` once when the pickup actually appears. Spatialize at the pickup. The preceding 15-second wait is silent. |
| Prop collection / replacement | Shared `pickup` once for accepted collection, including replacement of an unused held prop. |
| Team Turbo | Shared `turbo-start`, quiet `turbo-loop`, `turbo-end` across the accepted five-second effect. Do not stack a loud full loop for every teammate. |
| Portal Steal | Shared `portal-open`, `portal-in`, `portal-out`, `portal-close`, plus `receive` at feet. Follow football's own animation phases, not basketball's timings. |
| Interrupted Portal | Shared `interrupt` once on a genuine interruption; stop remaining transfer cues. |
| Wall first tap | `wall-preview` once as placement preview opens. Moving the preview should remain silent. |
| Wall Place / Cancel | `wall-place` only after accepted deployment. Cancel uses `ui-back` and preserves the held prop. Rejected placement uses low-key `ui-error`, not deployment audio. |
| Ball hits Wall | `wall-hit` on the resolved swept/physical collision, deduplicated. Wall now also blocks players; a restrained shared body contact covers player impact. |
| Wall expiry | `wall-expire` at 15 seconds. Silent cleanup on a goal/reset avoids many expiry effects together. |
| Kickoff | Shared `countdown` for the three-second countdown, then `kickoff-whistle` when both teams are released. No formal-match whistle for Explore. |
| Goal awarded | Shared `score` on the authoritative score increment, independently of net contact. The existing short net follow-through precedes the next kickoff phase. |
| Golden goal | `overtime` once when tied regulation transitions directly into play. No final whistle or kickoff restart at that transition. |
| Match end | `final-whistle`, then local `result-win`, `result-loss` or `result-draw`. The golden-goal winner ends the match; timeout without a winner is a draw. |
| Outdoor bed | Shared quiet `air`. No crowd chant, commentary or music in the main proposal; there is no authored spectator event requiring them. |

The current game does not implement offside, penalty shootouts, foul decisions,
half-time breaks, corners or throw-ins. Those sounds are not part of this pass.

## Every UI interaction

Reuse the eight approved UI families; keep these local and non-spatial.
An input press and an accepted world action are separate events. Suppress
redundant accents when a confirmation immediately produces a strong prop cue.

| Family | Existing football UI uses |
| --- | --- |
| `ui-click` | Kick, Dash, Pressure, Tackle, Jump and prop press, once per press rather than every held frame. |
| `ui-confirm` | Start match, accepted Place and confirmed Leave; avoid doubling Wall deployment. |
| `ui-back` | Cancel kick, cancel Wall preview, Back and Keep playing. |
| `ui-toggle` | Actual setting toggles; continuous slider adjustments need no repeated ticks. |
| `ui-select` | Camera mode, guide tab, match option and Team A/B selection when the choice changes. |
| `ui-error` | Rejected team/action requests; never repeat an alarm while holding a disabled control. |
| `ui-open` | Settings, guide, team-selection or confirmation panel opens. |
| `ui-close` | Panel closes and play resumes. Avoid also playing Back for the same close. |

Football uses its own button factory and reuses some basketball visual feedback,
but that does not establish football audio routing. The later integration must
cover both factories and touch/keyboard action entry points. A shared mixer can
expose Master, pitch effects, buttons and ambience consistently with basketball;
this preview does not change the already implemented UI.

## Reuse decisions

**25 families / 25 WAVs** are copied byte for byte from the approved basketball
delivery for this preview: `boundary`, `body`, `cloth`, `swipe`, `countdown`,
`spawn`, `pickup`, `turbo-start`, `turbo-loop`, `turbo-end`, `portal-open`,
`portal-in`, `portal-out`, `portal-close`, `interrupt`, `score`, `air`, and the
eight `ui-*` families above. Original records remain in
[the approved manifest](../ArtSource/Audio/Basketball/delivery.json).

Both sports reference the same shared audio assets. Duplicating
these WAVs in another Unity Resources folder would waste the APK reserve.
Basketball hardwood steps, sneaker squeaks, bounces, hand catches, swishes,
rim/backboard impacts and Lock-on do not fit football's surface or actions.

## Free-source research and selected downloads

The following uploader pages identify their sounds as **CC0**. The downloader
verifies that declaration and saves the page, source URL, timestamp and SHA-256
under the ignored preview directory. Inputs are public HQ MP3 audition copies;
the original Freesound WAV downloads require login. WAV export cannot restore
lossy source detail. These are source observations, not a claim of legal title
beyond the uploader's declaration.

| Source | Selection and limitations |
| --- | --- |
| [Soccer kick — bittermelonheart](https://freesound.org/people/bittermelonheart/sounds/555042/) | Main actual football kick. Full-power variants use small resampling changes of one take, not three different recordings. |
| [Soccer ball kick — Luisa_Sanchez](https://freesound.org/people/Luisa_Sanchez/sounds/813402/) | Second actual kick, short-pass/receive variation and comparison. |
| [Soccer kick foley — padpad](https://freesound.org/people/padpad/sounds/515908/) | Author used pillow impacts. Clearly labeled as a foley comparison, not a real ball recording. |
| [Grass footsteps — Fission9](https://freesound.org/people/Fission9/sounds/521587/) | Hiking-boot and grass layers. Four isolated steps form the walking/running base; not recorded football cleats. |
| [Trail running — Luisa_Sanchez](https://freesound.org/people/Luisa_Sanchez/sounds/816841/) | Alternative outdoor running texture; compare the grit with the grass design. |
| [Soccer net — Luisa_Sanchez](https://freesound.org/people/Luisa_Sanchez/sounds/816989/) | Actual net recorded with a contact microphone. May sound unusually close or rigid; needs listening approval. |
| [Metal pipe impact — Anthousai](https://freesound.org/people/Anthousai/sounds/406556/) | Pipe struck with wood, layered with a football kick for a goal-frame approximation. The second post variation is synthesized. |
| [Sports whistle — Musik-Fan](https://freesound.org/people/Musik-Fan/sounds/631849/) | One whistle edited into a short kickoff and short–short–long final whistle; reduced output level. |

Existing UI comes from [Kenney Interface Sounds](https://kenney.nl/assets/interface-sounds)
(CC0). Shared prop, body/fabric, score and air effects are original local synthesis.

Other search results were less suitable: [volivieri's soccer recording](https://freesound.org/people/volivieri/sounds/37157/)
uses attribution licensing; [this kick-labelled recording](https://freesound.org/people/purchasing102/sounds/521825/)
is a ball dropped onto wood; [this whistle](https://freesound.org/people/patobottos/sounds/345800/)
and [this end whistle](https://freesound.org/people/AFFRefTony/sounds/455491/)
have noncommercial licensing. They are not downloaded into this pack.
A suitable free direct goal-post recording was not verified in this search,
so the preview labels its designed alternatives honestly.

## Missing sounds: generation and alternatives

Original deterministic DSP supplies Wall preview/deployment/expiry, stamina and
match-status cues, and the hollow goal-frame comparison. Turf bounce, cut,
slide, landing/fall and Wall impact combine simple synthesis with recorded
layers. This costs no service credits and permits exact edits to duration,
pitch and level. The turf roll is a quiet generated loop.

If an approved candidate still needs more realism, record a real ball bouncing
on grass, studs planting/turning on turf, and a ball against a hollow goal frame.
Capture several clean isolated takes plus permission from the recordist. These
would be better masters than repeatedly processing an MP3 preview.

[ElevenLabs Sound Effects](https://elevenlabs.io/sound-effects) is an optional
AI replacement route, not used for these clips. Its official
[publishing guidance](https://help.elevenlabs.io/hc/en-us/articles/13313564601361-Can-I-publish-the-content-I-generate-on-the-platform)
states that the free plan lacks a commercial license; paid-plan output has
different rights. Do not assume a free AI download is suitable for game release.
Potential prompts, if requested later: “single football striking a hollow metal
goalpost outdoors, short resonant clank, no voices, no music” or “one football
bounce on short damp turf, close dry recording, no crowd.” Compare any result
against the current candidates and retain its generation/license record.

## Integration after approval

1. Keep the approved choices, retain their sources and licenses outside Unity,
   and import short mono delivery clips. All sounds were approved; the alternate
   kick and running takes join their corresponding variation banks.
2. Share the existing bank and output mixer. Use pooled positional sources for
   pitch events, local 2D UI/results, and bounded quiet loops. Do not normalize
   imports: that would undo deliberately soft authored levels.
3. Wire accepted actions to audio events. Important sites include
   `FootballBall.TryKick`, possession/interception and tackle resolution,
   `FootballNetSurface` contact handling, `FootballProps` transfer phases and
   `WallRebound`, `FootballMatch` transitions, and the presentation foot gait.
   Deduplicate collision callbacks, swept/fallback contacts, host replication
   and locally predicted feedback; sustained overlap must not retrigger hits.
4. Portal lasts 0.7 seconds. Start cues follow its actual visible phases: source
   shrink starts around phase 0.14, destination growth around 0.54, receive at
   phase 1, and visual closure by about 1.26. Wall starts only on accepted
   placement and lasts 15 seconds. Validate this against actual gameplay.
5. Test offline, host/client, join/reset, interrupted props, menu navigation and
   touch/keyboard input. Verify no scoring cues in Explore, no false final
   whistle entering overtime, and no extra kicks on hold/cancel/rejection.
6. Build AndroidSubmission, inspect its packing audit, test and promote the
   complete Windows player, and update measured size/delivery evidence.

Start with an **incremental compressed delivery target of 500,000 bytes or less**
for the selected new football sounds. This is an engineering target, not a
measured APK result. Audition PCM sizes are not APK sizes. The existing APK is
89,851,508 bytes and already exceeds the 75 MB development target; sharing clips,
dropping alternatives and deliberate compression remain necessary.
