# Pitch deck review — 29 October 2026

Revision follow-up: the improved editable deck and its verification record are available in [PitchDeck/README.md](PitchDeck/README.md). The findings below describe the earlier review snapshot. The source downloaded for revision already uses **Thai Jing Heng** consistently; the revised deck preserves that spelling.

Reviewed: **WHATTHE-FISH-Pitch-Revised.pptx**, 37 slides, opened in PowerPoint for the web in Google Chrome. Every slide was visually inspected, and slide text and speaker notes were read. The online presentation was not edited.

The implementation comparison uses the current workspace, current feature documents and selected runtime sources. This is a presentation and source review, not a new gameplay, backend or device qualification. Some repository documents contain historical descriptions that conflict with newer source. Demonstrations and screenshots must match the actual submitted APK.

## Main finding

The deck explains the four sports and coastal setting, but understates how much product work surrounds them. Safety, personal controls, localization, communication, rewards and recovery provide evidence of a usable social game. Most of that evidence is missing or compressed into generic wording.

Adding one slide for every missing feature would make the pitch harder to follow. Reallocate repeated concept, inspiration, market and audio slides to a small number of concrete demonstrations. Keep detailed implementation and feature inventories in an appendix.

Preserve the useful existing strengths: actual gameplay captures, consistent rounded typography, recognizable branding, the four-island world and honest distinction between implemented features and future proposals.

## Corrections before the next presentation

| Location | Finding | Required correction |
| --- | --- | --- |
| Slides 1 and 2 | The cover says **Tai Jing Heng**, while the team slide says **Thai Jing Heng**. | Confirm the person's preferred spelling and apply it consistently. Do not infer it from generated copy. |
| Slide 14 notes | The notes explicitly say voice is not implemented. | Replace this with the current room-voice status and its device-testing limits. Voice code and documented two-client transport checks exist. |
| Slide 35 | The visible footer says there is no shop. | Distinguish the implemented store using earned coins from proposed real-money monetization. Current cosmetic purchases, account ownership and daily/match rewards exist. |
| Slide 10 notes | The event list includes Tiny Cup. | Current `GolfEventKind` contains Tropical Wind, Sticky Green, Bounce Party, Tornado and Crab Crossing. Refresh the notes from the submitted build. |
| Slide 26 notes | A known Golf low-gravity guide mismatch remains described as an outstanding correction. | Verify whether the current submitted guide still contains it. Do not carry an old defect forward as current or claim it fixed without checking. |
| Slides 14, 24–26 | Captures/notes draw on different historical builds, including older menus and control designs. | Replace captures that explain current interaction with images from one identified delivery. Historical ability-effect images can remain if their purpose is clear. |
| Slides 21, 22, 36 and 37 notes | Phone-verification wording predates the limited 29 October P40 test. | Record the limited successful startup, three-language welcome, guest entry and Golf shot. Huawei sign-in did not establish success. QR, voice, multiplayer and performance remain open. |
| Slide 31 | The Portal transfer image shows a group arrival on the football pitch, while the text/notes describe ball-transfer audio. | Use the actual ball ability effect, or relabel the image and audio explanation as group arrival. |

The current Golf touch source uses a drag-and-release power slider. Slide 10's drag wording is compatible with that source. Older hold-to-charge sections in the README/Golf history should not be used to rewrite the slide without checking current input code.

## Missing and underrepresented features

| Priority | Feature and current support | Deck treatment | Recommended audience-visible evidence |
| --- | --- | --- | --- |
| Essential | Offensive-text detection for new names and room chat. A shared vocabulary covers selected English, Chinese and Malay terms, with normalization for common obfuscations. Device/host/backend enforcement varies by surface. | Absent. | Show a safe rejection example beside a benign accepted message. Explain that rejected room text does not enter history. |
| Essential | Reporting and player controls: report reasons/details, entry points from profiles and sport standings, blocks, text muting and voice muting. | Absent. | Show where a player opens Report player and the reason form. Explain manual review without implying automatic punishment. |
| Essential | Personal control position and size for all four sports. Drag to move, handle/slider to resize, Save/Cancel/Reset. Sizes are bounded to 70–160% of the responsive default. | Slide 25 discusses contextual controls but never demonstrates customization. | Show the same HUD before editing, during resizing and after Save. Demonstrate persistence after reopening. |
| Essential | Layouts are stored locally per identity and sport, and accessible from main-menu and in-game settings. Tutorials use the saved HUD positions. | Absent. | Adjust one sport, enter its tutorial, and show that the highlight follows the moved control. State that layouts do not sync across devices. |
| Essential | English, Simplified Chinese and Bahasa Melayu throughout the game, including authored labels/artwork treatment. | No dedicated coverage. | Show the same meaningful menu/HUD in all three languages with equivalent styling. A live switch is stronger than a flag icon. |
| Essential | Daily rewards, eligible match coins, persistent account balance and cosmetic ownership. | Absent; slide 35 incorrectly says no shop. | Claim a daily reward, select a locked cosmetic, show its cost, unlock it and show the result. Keep real-money revenue as future work. |
| High | Room voice through Vivox, individual volume/mute and spectator-group mute. Room text has player/spectator histories and eight stickers. | Profiles/text/stickers briefly mentioned; voice contradicted in notes. | Show communication in the actual game and one clear mute action. Distinguish synthetic desktop transport checks from physical microphone validation. |
| High | Friend invitations through the in-app inbox and a 30-second in-process guest reconnection grace flow. | Not explained. | Invite a friend from a waiting room. For recovery, show the pause/countdown and return or clean failure, with exact supported limits. |
| High | Hands-on tutorials, separate from four-page island rules, plus a first-home introduction with localized narration. | Slide 26 only shows the rules card. | Show an action completing a lesson and the voiced menu introduction. Make the three learning surfaces easy to distinguish. |
| High | Responsive welcome/menu placement based on current window/safe area, including cutouts, and reduced-motion/audio settings. | Largely hidden in notes. | Use wide-phone and tablet comparisons, identify desktop previews, and demonstrate reduced motion. Avoid a universal-device claim. |
| High | Venue customization in all four sports, with previews, cosmetic choices and host-shared appearance. | Only a small button in a home screenshot. | Show one live edit and the same appearance on a second client. Keep cosmetic ownership separate from local venue selection persistence. |
| Supporting | Expandable scoreboards, per-player match statistics, results and celebrations. | Visible incidentally but not explained. | Expand a scoreboard and point out a useful team contribution such as an assist. |
| Supporting | Offline exploration and programmed Football/Basketball bots. | One generic line on slide 24. | Explain precisely which modes work offline and which sports support bots. Do not imply equivalent bots for Golf/Fishing. |
| Supporting | Fishing's choice of one skill from three shared options, scored three-minute rounds, Wanted bonus and skill cooldown. | Generic skill wording, with important rules only in notes. | Show the shared skill choice and a short catch-to-points sequence. Validate currently exposed modes rather than promoting retained legacy Cup/Crew fields. |

Safety wording must stay precise. This is a vocabulary-based filter, not contextual AI moderation. It can miss abuse or reject benign wording. There is no automated voice/image moderation or automatic ban. A modified player host can bypass chat enforcement. Reporting supports an operator's review rather than guaranteeing an outcome.

## Review of every slide

| Slide | Assessment | Recommended action |
| --- | --- | --- |
| 1 — WHATTHE FISH? | Strong recognizable logo, but “Multiplayer sports” is too broad and the large white panel is underused. | Add a specific one-line premise, such as a continuing group session across four coastal sports. Keep the cover brief. |
| 2 — Team | Dense and early. Repeated “Project experience” wording consumes space, and the name spelling differs from the cover. | Shorten to names and verified contribution areas. Move after the product evidence unless the required template fixes its position. |
| 3 — Game background & inspiration | The everyday-break problem is understandable. The MOBA comparison introduces a different genre without evidence that it is the relevant alternative. | Focus on organizing friends with limited shared time and different preferences. Treat this as a hypothesis until playtests support it. |
| 4 — Core concept & theme | Useful real scene and shared-room explanation. A single fishing-island overview does not demonstrate connected play or camera options. | Show the four-island arrangement or a group arriving, and foreground one continuing room with one active game. |
| 5 — Objectives | Repeats slides 3, 4, 7 and 15. “Made for mobile” has little visible mobile evidence. | Merge the strongest objective into the opening. Use this slot for a missing product demonstration. |
| 6 — Gameplay & key features | Good world overview. The right-side labels are disconnected from their map locations and add little beyond naming the sports. | Label each island on the image and identify the shared travel connections. Avoid implying four simultaneous games. |
| 7 — Gameplay loop | One of the clearest conceptual slides. The process is readable but text-only. | Keep and support with a short real journey: join, travel, play, result/rematch. Mention invitation and capacity limits in backup material. |
| 8 — Football | Useful current-action explanation, but the image shows practice/charging rather than a compelling team moment. | Use a pass, goal or ability sequence. Include match objective, supported participation and default duration only if verified against the submitted build. |
| 9 — Basketball | Clear screenshot of contextual finish controls. Four paragraphs require substantial narration. | Use a short play sequence to show pass, finish and defense. Separate team-match scoring from shared-practice scoring. |
| 10 — Golf | The slide explains five holes and hazards but the image does not clearly demonstrate the power gesture. Notes include an outdated event. | Show Aim and drag/release power, plus a clearly visible hazard. Refresh notes from current source. Golf may run longer than a brief break. |
| 11 — Fishing | Readable scene, but “different skills” understates the shared draft, scoring and timed competition. | Show skill selection, Hold the Glow and the confirmed catch score. Keep input instruction concise. |
| 12 — Skills & surprises | Six small stills make effects hard to read, especially subtle portals/boosts. Several captions are vague. | Select the most legible examples, crop to the action and show the effect in motion. Keep a full skill inventory in the appendix. |
| 13 — Skills & surprises | More distinctive examples than slide 12, but another six-image grid creates the same reading burden. | Combine the best of 12/13 into one or two demonstrations. Explain how a shared event changes a decision or rematch. |
| 14 — Playing with friends | The cabin is a useful social image, but it does not prove profiles, chat or spectating. Voice notes are wrong for current source. | Show actual communication, invitations and spectator UI. Use the cabin as the travel evidence. Add a separate safety demonstration. |
| 15 — Positive value | Plausible design intent, largely restating objectives. No evidence of measured social benefit. | Link value to observed behavior: passing, assisting, welcoming a newcomer and staying together. Keep relationship claims as intended outcomes. |
| 16 — Design inspirations | Clear references and attribution, but it arrives late and gives other games prominent visual attention. | Shorten or move to appendix. Explain only a design decision the audience needs to understand. |
| 17 — Design approach | Mostly repeats slide 16 in a dense table. It describes adaptation rather than proving differentiation. | Merge with 16 or replace with the game's own distinguishing experience and evidence. |
| 18 — Technical development | Credible tools, but reading out a stack offers limited player value. Acronyms assume technical familiarity. | Keep concise in a technical appendix or associate each technology with the game function it enables. |
| 19 — Technical Architecture | Information-rich and useful for technical questions. Too many arrows and small labels for quick projection. Raster artwork makes correction difficult. | Use a simplified main view showing clients, host, Relay and profile service. Keep the identity/data detail in an editable appendix diagram. Preserve Relay's correct routing role. |
| 20 — Technical challenges | Honest but broad. “Rough 3D art” is unspecific and the table lacks acceptance criteria. | Name the observed issue, impact and next check. Combine with slide 36. Do not frame already implemented controls/recovery as wholly future work. |
| 21 — HMS Account Kit | Explains a flow without showing the player experience. Successful native verification remains a material gap. | Pair implementation with actual device evidence and its current status. Do not imply the P40 sign-in succeeded. |
| 22 — HMS Scan Kit | Clear flow, but another four-card process and no real scan evidence. | Show host QR, guest scan and confirmed room arrival when verified. Keep manual code entry and denied-permission behavior in backup material. |
| 23 — Other technologies & APIs | Overlaps heavily with slides 18/19/21. | Fold into the architecture appendix and retain only technology details that explain product behavior. |
| 24 — UI/UX, visual & audio design | Three levels of heading weaken hierarchy. Its old home capture omits rewards/current onboarding. | Retitle around navigation. Use the current menu and one specific path to play. Add localization/adaptive UI evidence elsewhere. |
| 25 — Controls and feedback | Solid Fishing example but represents feedback rather than the personal-control feature the team has built. | Retain a brief action-feedback example and allocate a dedicated slide/demo to moving, resizing and saving controls. |
| 26 — Learning the game | Demonstrates reference rules, not action-based tutorial completion. New home introduction/narration is absent. | Clearly show home introduction, island rules and hands-on lessons as different experiences. Demonstrate a lesson using a customized HUD. |
| 27 — Visual identity & environment design | Actual stadiums are a strength. Both images are distant views, so craftsmanship and playable scale are hard to judge. | Pair an overview with a close player view, using a real device capture when qualified. |
| 28 — Environment design | More similar distant island images, repeating slides 4/6/27. | Combine environment coverage and use the recovered slot for safety or language support. Show close detail or readable play spaces. |
| 29 — Audio & animation | Static stills cannot demonstrate synchronized sound. The celebration image repeats earlier coverage. | Merge with 31 and play a short clip with clear action and sound. Include reduced-motion/audio controls where relevant. |
| 30 — Character design | Attractive close view. Recognition claims would be stronger with multiple actual identities/team views. | Show contrasting players, numbers and team colors. Avoid implying freely selectable hairstyles/outfits that the current store does not offer. |
| 31 — Animation, effects & sound | Duplicates 29. Portal image/text mismatch undermines the example; a technical footer adds density. | Match each sound to the pictured event and use a short audible example. Move AudioSource/bank details to notes. Simplify inconsistent headings. |
| 32 — Market potential & future development | Actually about target players; no market sizing or validation is shown. Audience appears very late. | Move audience near slide 3 and label the slide accordingly. Support demand with measured playtest findings when available. |
| 33 — Market position | Repeats the opening and slide 32. Short-session promise needs the Golf exception already present only in notes. | Consolidate positioning into one clear proposition and explain the combination the current game delivers. |
| 34 — Competitive landscape | Names alternatives, but does not compare relevant choices. Competitor art dominates again. | Use a compact evidence-based comparison of the specific experience being pitched. Research current competitor claims before rewriting. Avoid unsupported superiority checkmarks. |
| 35 — Strengths & business potential | Mixes technical strength, creative value and speculative revenue. “No shop” is now false. | Show implemented earned coins and cosmetic unlocks. Separately label real-money business options as proposals. Avoid implying paid gameplay advantages or proven revenue. |
| 36 — Future development | Broad and overlapping with slide 20. More islands is premature as the leading message when core device qualification remains incomplete. | Prioritize current-device acceptance, responsiveness and new-player playtests, then polish. Treat expansion as a later option while preserving the current four-island scope. |
| 37 — Thank you | Consistent closing artwork but no clear next action. | End with a concrete demonstration/playtest invitation and the memorable product premise. Use a verified download link/QR only when access and destination are ready. |

## Narrative and visual weaknesses

### Too much repetition, too little evidence

Slides 3–5, 7, 15, 32 and 33 repeat friends, short breaks and shared play. Slides 16/17 repeat inspiration. Slides 18/19/23 repeat technology. Slides 20/36 repeat future work. Slides 29/31 repeat audio/effects. Consolidating these creates room for missing features without expanding the deck.

At an illustrative 20 seconds per slide, 37 slides already consume 12 minutes 20 seconds before a live demo or questions. This is timing arithmetic, not a known competition time limit. The final slide count should follow the actual allotted slot and mandatory rubric/template.

### The template competes with the game

The consistent beach palette and rounded font suit the brand. However, the same beach photograph, upper-right palm, white cards and accent bars recur on nearly every slide. Several slides reserve a large upper band while shrinking the game evidence below it. The result resembles a long illustrated report.

Keep the visual identity but vary composition by purpose: a large gameplay image for a mechanic, a short sequence for an interaction, a compact table for comparison and a minimal view for architecture. Reduce repeated title/subtitle layers and use more of the canvas for the game. Preserve required template attribution.

### Screenshots do not always support the nearby claim

The cabin proves travel better than it proves friend requests. A still cannot prove sound. An empty practice court does not prove teamwork. Distant islands do not demonstrate touch comfort. Use an image or clip that visibly establishes each claim, with one or two focused annotations and readable controls.

Refresh screenshots from one submitted build. Identify desktop previews versus device captures. Preserve the actual UI rather than fabricating a better-looking screen. Keep English, Chinese and Malay versions readable and stylistically consistent in any new comparison.

### Player benefit needs to lead implementation

“NGO and MPS” is less memorable than friends staying in one room as they change sports. The technical explanation can then substantiate that experience. Control resizing supports comfort; language support lets a local group choose its UI; reporting/muting gives players practical responses to abuse. These features support product quality without needing to be pitched as unique inventions.

### Validation is absent from the visible story

Do not fill a pitch with assertion counts. Show a compact validation summary tied to the identified build: what gameplay was checked, what real-device evidence exists, what remains open and what the next playtest will measure. Automated tests, desktop captures, synthetic audio transport and physical-device tests answer different questions.

The 29 October Huawei P40 check is useful limited evidence, not a four-sport performance certification. It established startup, localized welcome, guest entry and a Golf shot. Successful Huawei login, QR, physical voice behavior, two-device multiplayer, full rounds and performance were not established.

## Proposed main sequence

This 18-slide sequence is a content map for an extended pitch, not a requirement to retain that count. For a short slot, combine sports/skills coverage and put technical/commercial detail in the appendix. Keep any mandatory submission sections even if they are backup slides.

| New order | Purpose | Existing material |
| --- | --- | --- |
| 1 | Product premise and logo | 1, 4, 33 |
| 2 | Intended players and their shared-time problem | 3, 32 |
| 3 | One room, four islands, shared journey | 4, 6, 7 |
| 4 | Football and Basketball in action | 8, 9 |
| 5 | Golf and Fishing in action | 10, 11 |
| 6 | Abilities and events that change the round | Best examples from 12, 13 |
| 7 | Personal control position and size | New evidence replacing repeated concept content |
| 8 | Learning through actual actions | 26 plus hands-on tutorial/home introduction |
| 9 | Three-language and adaptive interface | New evidence |
| 10 | Staying connected: invitations, text, voice and spectators | 14 plus current communication UI |
| 11 | Text safety and player reporting/mute/block | New evidence |
| 12 | Earned rewards and cosmetic expression | Corrected 35 plus calendar/store/venue preview |
| 13 | Coastal visual identity and synchronized sound | 27–31, concise clip |
| 14 | Architecture and the two integrated HMS kits | Simplified 19, essential 21/22 |
| 15 | Validation, recovery and device limits | New summary, specific issues from 20 |
| 16 | Positioning and next playtest | 34 plus real evaluation plan |
| 17 | Team and prioritized next milestone | 2, consolidated 20/36 |
| 18 | Closing and concrete demo/playtest invitation | 37 |

Appendix: individual sport rules/capacities, full abilities, detailed architecture, toolkit inventory, inspiration attribution, rewards eligibility, store options, recovery edge cases, HMS evidence and commercial proposals. Include completed-versus-proposed labels where they affect interpretation.

## Demonstrations to prepare

1. **Control comfort:** open Layout, move one button, resize it, save, reopen and complete the relevant tutorial action. Show that size/position persist and that the lesson highlight follows the control. Use a test identity and preserve the preferred layout afterward.
2. **Text safety:** show a benign message accepted and an offensive test fixture rejected with a localized explanation. Open a report form and explain reason/details, mute and block. Avoid actually reporting an uninvolved real player during a pitch.
3. **Language and onboarding:** switch the same surface between English, Chinese and Malay, then show one narrated introduction step or hands-on lesson. Keep voice narration separate from player voice chat in the explanation.
4. **Social continuity:** invite/join a room, use a text/sticker or voice control, travel together, finish a round and continue with the same room. Show recovery only within its supported conditions.
5. **Rewards and expression:** claim an eligible daily reward or show a confirmed match reward, unlock a cosmetic using earned coins and show the changed appearance. Use a prepared account with sufficient balance. Do not present a test-only funding route as normal gameplay.

Use short recorded backups from the same build for timing and connectivity problems. Capture actual game state and sound. Do not use edited footage to imply a device test or service success that did not occur.

## Evidence to collect next

- First-time group playtest: recruitment method, participant/group count, date, build and device list.
- Time to enter the first shared round, tutorial completion and tasks requiring presenter help.
- Whether players can explain the objective and perform the main action without verbal coaching.
- Control-edit success, language preference and clarity of feedback.
- Whether the same group chooses a rematch or another sport, with a clear denominator.
- Full-session device frame pacing, memory, loading and thermal behavior, separately from streamed screenshot quality.
- Supported-device HMS login and QR flow, including cancellation/permission-denial behavior.

Leave unavailable results blank or label them planned. Test assertion counts are not evidence of audience demand, retention or a healthy community. A cosmetic economy is not evidence of revenue.

## Implementation references

These sources support the feature comparison. Their dated validation records should be interpreted within their own source/build limits.

- [Player reporting and text safety](PLAYER-SAFETY.md), [runtime matcher](../Game/Assets/_Game/Shared/Core/TextSafety.cs) and [chat enforcement](../Game/Assets/_Game/Shared/Networking/RoomChatService.cs)
- [Personal control layouts](CONTROL-LAYOUT.md) and [size/persistence source](../Game/Assets/_Game/Shared/UI/ControlLayouts.cs)
- [Localization](LOCALIZATION.md), [adaptive menu/welcome](RESPONSIVE-ANDROID-LAYOUT.md) and [saved motion/audio preferences](../Game/Assets/_Game/Shared/UI/MenuPreferences.cs)
- [Room voice](ROOM-VOICE.md), [room chat](ROOM-CHAT.md) and [crew reconnection/invitations](CREW-CONNECTIONS.md)
- [Account coins](COIN-ECONOMY.md), [daily rewards UI](COIN-REWARDS-UI.md), [cosmetic store](COSMETIC-STORE.md), [store runtime](../Game/Assets/_Game/Shared/Core/CosmeticStore.cs) and [wallet runtime](../Game/Assets/_Game/Shared/Core/PlayerEconomy.cs)
- [Home introduction](FIRST-VISIT-GUIDE.md), [localized narration](FIRST-VISIT-AUDIO.md) and [hands-on tutorials](GUIDED-TUTORIALS.md)
- [Venue customization](VENUE-CUSTOMIZATION.md) and [scoreboards](SCOREBOARDS.md)
- [Current Golf touch gesture](../Game/Assets/_Game/Shared/UI/GolfSwingButton.cs) and [current Golf events](../Game/Assets/_Game/Sports/Golf/GolfEvents.cs)
- [Limited Huawei P40 device check](AppGallery/HUAWEI-CLOUD-DEVICE-2026-10-29.md)

## Task scope and handoff

Only this review document was added. No presentation content, gameplay, assets, account data or service settings were changed. No players/APKs, isolated copies, media downloads or capture archives were generated. Existing unrelated workspace work was preserved. No Git staging, commit or push was performed.
