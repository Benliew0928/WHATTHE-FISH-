# Category B: next improvements

Source: the supplied **UXHMAC 2026 Preliminary Round Participant Briefing
Slides**, page 8 (Category B criteria) and page 4 (25% pitching / 75% experience),
plus the **Category B Preliminary Stage Sample Deck**, pages 6–10.
The supplied briefing lists criteria without individual criterion weights.
This is a proposed development order, not an official scoring prediction.

Personal control layouts directly support **8 UI/UX**, with evidence for
**9 Functionality & Stability** through persistence and input checks, and
**15 User Guide — Verified Against Build** through the matching controls guide.
They should be demonstrated as practical accessibility and comfort, rather than
claimed as the game's main innovation.

| Priority | Proposed improvement | Category B criteria | Judge-visible proof |
| --- | --- | --- | --- |
| 1 | Short interactive first-play lesson for each sport, with Skip and Replay. Ask the player to move, perform one main action and finish a tiny objective. Follow the player's customized button positions. | 7 Gameplay, 8 UI/UX, 15 User Guide | A new player completes a pass, a basket, a golf shot or a catch with minimal verbal coaching. |
| 2 | Accessibility presets: left-handed and large controls, adjustable look sensitivity, and text/shape cues alongside colour for shot timing and fishing tension. Preserve individual edits with a preview before applying a preset. | 8 UI/UX, 13 Visual & Audio | Switch presets, refine one control and demonstrate the same action without relying only on colour. |
| 3 | An “Island Passport” with clear sport mastery objectives and shared crew goals. Reward improvement and helping teammates with cosmetic recognition. | 10 Engagement, 12 Realized innovation, 14 Positive value | Complete one objective, see progress persist, and show how teamwork contributes to a shared goal. |
| 4 | A clear recovery flow for connection loss: reconnecting status, retry/cancel, and an explanation of whether the room or score can be recovered. Build on existing host migration and room behaviour. | 9 Stability, 8 UI/UX | Interrupt a test connection, recover or leave cleanly, and show no stuck controls or duplicate results. |
| 5 | A player-facing graphics/performance setting with sensible defaults, then qualify a real Android phone. Keep distant detail scalable while preserving readable controls and close character/cabin quality. | 9 Stability, 13 Visual & Audio | Record frame time, memory, loading and thermal behaviour during a full four-island session on the target phone. |

For **11 HMS Integration**, prioritize a reliable demonstration of the existing
Huawei account and QR room-joining flows on a supported phone, including cancel,
permission denial and retry. Show the connection to the actual shared game.
See [account setup](PLAYER-LOGIN.md) and [Scan Kit](HUAWEI-SCAN-KIT.md).
Additional HMS services should solve a concrete game need and have their current
SDK availability checked before implementation.

For pitching, use a compact before/after story: a new player struggles with
fixed controls → opens Layout → adjusts controls for comfort → completes the
interactive lesson → joins friends → earns shared progress. Show the same
features in the submitted APK, guide and video. Keep the four-island shared
travel model central to the concept and positive social value.
