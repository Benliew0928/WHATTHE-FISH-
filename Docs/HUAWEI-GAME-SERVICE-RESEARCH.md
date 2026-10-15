# Huawei Game Service integration and competition publishing

Research date: **14 October 2026**, reviewed **15 October 2026**, Malaysia time. Project: **WHATTHE FISH?**

Use **current Huawei Game Service initialization and game sign-in with Account Kit** as the first integration milestone. Keep the existing Unity multiplayer rooms and four-island gameplay. Consider Scan Kit for joining rooms by QR code, subject to Unity compatibility and the measured APK budget.

**Correction on 15 October:** Analytics Kit is not a general option for a new entry. Huawei's developer documentation says its service access portal closed to new registrations on 30 June 2024. Obtain explicit confirmation of an existing eligible project or special organizer access before including it. Product pages and SDK downloads alone do not establish activation availability. [Huawei Analytics access notice](https://developer.huawei.com/consumer/en/doc/HMSCore-References/android-api-hianalytics-0000001050707170)

**Do not plan Huawei achievements, leaderboards or cloud saves using old Game Service tutorials.** Huawei removed saved games in SDK 6.13.0.300 on 30 May 2024, then removed achievements, event reporting and leaderboards in SDK 6.14.0.300 on 27 September 2024. The current release notes list 6.14.0.300 and require HMS Core (APK) 6.12.0.300 or later. Installing an older wrapper does not restore an online service. [Huawei version history](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/version-change-history-0000001050123471)

This is a development proposal. The game currently has no working Huawei integration, and SDK compatibility and package growth need a fresh Android build and physical-device testing.

## Competition requirements: participant-kit review, 15 October

The supplied event is the **UMPSA x HUAWEI GameCenter Mobile App Competition 2026**. Category B covers undergraduate university, polytechnic and diploma students. The participant briefing and Category B kit now provide more specific evidence than the public poster. This section summarizes the supplied private materials; the original PDFs are retained outside Git.

Sources inspected: **UXHMAC 2026 Preliminary Round Participant Briefing Slides.pdf** (21 pages), **Category B / Preliminary Stage Sample Deck.pdf** (12 pages), and **Category B / Application Form Category B.pdf** (2 pages). The copies in the two supplied download folders have matching SHA-256 hashes. Full hashes and local extraction/render evidence are in ignored `Builds/HuaweiResearch/20261015/`; no SDK credentials or sample integration code were found in these three PDFs.

### What the kit settles

| Requirement | Evidence and consequence |
|---|---|
| HMS must serve the game | Briefing p4 says integration into core gameplay; Category B form p1 asks which feature(s)/kit(s) are used and how they support the core function. Merely installing GameCenter does not demonstrate game integration. No minimum kit count or named compulsory kit is stated. |
| Overall scoring | Briefing p4 gives **25% pitching and 75% experience**. HMS is one component of experience, alongside gameplay, UI/UX, stability and engagement; HMS alone is not worth 75%. |
| Category B criteria | Briefing p8 also assesses student creativity/ownership, visuals/audio, positive values and a **user guide verified against the build**. Prepare concise host/join, controls, travel and troubleshooting instructions matching the shipped APK. No individual HMS weighting is supplied. |
| Package formats | Briefing p5 explicitly shows **APK/RPK**. Unity APK is a documented submission format; RPK is a separate quick-game format, not an APK renamed. Confirm any format-specific publication or scoring conditions before considering changes. |
| Presentation | Briefing p6 permits **at most 40 slides**. The sample deck organizes team, concept, gameplay, innovation, technical implementation/HMS, UI/visual/audio, market/future and summary. Its technical section p8 requests technology, implementation and result, and allows discussion of other technologies/APIs. |
| Publication evidence | Sample deck p1 says the GameCenter QR box may be deleted if the game is unreleased. This strongly suggests unpublished entries are contemplated, but is not an explicit waiver of publication at every stage. |
| Monetization | Sample deck p10 labels monetization as applicable only when relevant. Neither supplied Category B document requires Ads or IAP. |
| Written summary | Category B form p2 requests a summary of at most **500 words**, covering concept, purpose, audience, gameplay and innovation. |
| Schedule | Briefing p3 lists final upload **1 November, 11:59 PM**, preliminary judging **2-9 November**, and finale **25 November**, top ten per category. The public timeline specifies UTC+8. |
| Workshops | Briefing pp10-12: **17 October 9 AM-noon** pitching; **17 October 2-6 PM** Agentic AI/Godot/MCP Android workshop; **18 October 9 AM-noon** RPK/GameCenter publishing. Attendance at all three is stated for the e-certificate. An AI workshop does not establish unrestricted AI usage; student ownership remains assessed. |
| Participant onboarding | Briefing pp13-17 describe GameCenter installation and Malaysia-region Huawei ID registration/login, including an onboarding KPI. P18 shows MyNext registration. These are distinct from integrating HMS into our APK; their mandatory status, proof, counts and deadlines are unspecified. |

### Conflicting submission limits

Briefing p6 says a **maximum five-minute demo**, mandatory **YouTube upload**, and English subtitles for non-English videos. However, the poster embedded on that same page still says **three minutes within 60 MB**; the [public Category B poster](https://www.uxhmac.com/img/banners/category_b.png) also says three minutes, 60 MB and game files up to 100 MB. Do not silently treat the newer text as resolving the conflict. A three-minute video fits both duration limits while clarification is pending; confirm whether a separate video file and the 60 MB cap still apply alongside YouTube.

Our repository's self-contained APK requirement remains **strictly below 100,000,000 bytes**, with a development target of 75,000,000 bytes or less. Ask whether the organizer's 100 MB applies to the APK alone or all game-file uploads. The briefing does not authorize exceeding our project budget.

### Questions ready to ask the organizer

1. **Core integration:** "Our Unity multiplayer game uses Huawei game sign-in, and we are considering Scan Kit so players can scan a host's QR code to join a room. Would this satisfy your requirement to integrate HMS into the game's core function? Is any particular kit or minimum number of kits required?"
2. **Scoring:** "How is HMS integration assessed within the 75% experience score? Do you reward useful integration and reliability, the number of kits, or particular capabilities?"
3. **APK versus RPK:** "Page 5 accepts APK and RPK, while the publishing workshop focuses on RPK. Can our Unity Android APK enter and be judged on the same basis, without conversion to an RPK?"
4. **Publication:** "The Category B sample deck lets an unreleased game omit its GameCenter QR code. Can you confirm whether public GameCenter or AppGallery publication is optional for the preliminary round and the finale, and whether any developer verification is still required?"
5. **Video and file limits:** "The briefing says five minutes and YouTube, while the poster says three minutes and 60 MB. Which requirements apply to Category B? Is the 100 MB limit for the APK itself or all game files together?"
6. **Currently available services:** "Huawei's documentation says Analytics Kit is closed to new registrations, and Game Service removed achievements, leaderboards and saved games. Which currently available kits do you recommend for a new Malaysian project, and is any special service access supplied by the competition?"
7. **Unity support:** "Will the workshop provide a supported Unity integration example or contact for current HMS SDKs? Our project uses Unity 6, IL2CPP and ARM64."
8. **Judging setup:** "Which Android phones and HMS Core versions will judges use? Will internet access, Huawei test accounts and enough devices for a live multiplayer demonstration be available?"
9. **Other online services:** "Our multiplayer rooms and networking use Unity services. May we keep those while integrating Huawei services for player identity and room invitations?"
10. **KPI and registration:** "Are the GameCenter/Huawei ID KPI and MyNext registration mandatory for each team member? What counts, evidence and deadlines are required, and do they affect eligibility or judging?"
11. **AI and ownership:** "The workshop includes AI-assisted game development. What AI-generated code or assets are allowed, and what disclosure or proof of student contribution should we submit?"
12. **User guide:** "What format and length should the user guide have, and where should we upload it so judges can verify it against the game?"

For WHATTHE FISH?, retain the current Unity room system and investigate Account Kit/Game Service plus Scan Kit as a small, visible integration. Organizer acceptance of that combination is **not yet established**. The [full HMS catalog status audit](HMS-CATALOG-STATUS.md) separates publicly documented capabilities from retired features and restricted access; a marketing listing is not proof of new-project availability.

## What the Huawei products mean

| Product | Role in this project |
|---|---|
| HMS Core | Huawei's collection of device and service SDK capabilities, with a separate installed runtime on supported Android devices. |
| Game Service | Game launch initialization, Huawei game sign-in, floating window, game update, addiction prevention and GameTurbo Engine integration. |
| Account Kit | Supplies Huawei ID authorization used before Game Service obtains the game player identity. |
| AppGallery Connect | Developer console for the app record, API activation, certificate fingerprints, configuration, testing and release management. |
| AppGallery | Store through which the approved Android release reaches users. |
| GameCenter | Huawei's game ecosystem and the event partner; do not treat the event name as a separate multiplayer SDK requirement. |

The current Game Service introduction lists the supported game capabilities above and supports Huawei and non-Huawei phones/tablets subject to the platform/runtime requirements. Regional availability follows Account Kit. Its February 2026 notice explicitly confirms the retired features, despite the older marketing overview still advertising them. Use the development guide and release notes for implementation decisions. [Current service introduction](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/introduction-0000001050121216)

For this Android competition, test Android/EMUI-compatible devices. Confirm the judging OS rather than assuming all Huawei devices run this APK; native HarmonyOS development is a separate platform path.

## How the services fit the current game

The inspected project uses Unity **6000.3.20f1**, IL2CPP, ARM64 and minimum Android API **26**. Its development package is `com.umpsa.whatthefish`. Custom Android Gradle templates are currently disabled and there is no `Game/Assets/Plugins/Android` integration directory.

[Models.cs](../Game/Assets/_Game/Shared/Core/Models.cs) contains an unused `IPlatformGameServices` boundary with only `Available` and `ReportResult(string, int)`. `HuaweiServicesPlaceholder` returns false and logs that integration is not configured. There are no runtime calls wiring that placeholder into authentication or results. [RoomService.cs](../Game/Assets/_Game/Shared/Networking/RoomService.cs) separately initializes Unity Gaming Services, signs in anonymously, and creates or joins Multiplayer Services sessions with Relay. NGO owns the host and client gameplay connection.

The integration should add a Huawei account layer while preserving **four islands, one active game per host room, and everyone travelling together**. Game Service's current capability list supplies no replacement for our physics synchronization, Relay transport or room authority. Replacing that working multiplayer system would add unrelated implementation and testing work.

```mermaid
flowchart TD
    A[Android game launch] --> B[Game privacy agreement]
    B --> C[Huawei Game Service initialization]
    C --> D[Account Kit authorization]
    D --> E[Game Service getGamePlayer]
    E --> F[Huawei identity and profile]
    F --> G[Existing game menu]
    G --> H[Unity authentication and MPS room]
    H --> I[Relay and NGO host authority]
    I --> J[Four islands and shared travel]
    J --> K[Final sport results]
    K --> L[Local progress]
    G -. optional .-> M[Scan Kit room QR]
    M --> H
```

**Huawei game identity and Unity multiplayer identity remain separate.** A Huawei `openId` is not automatically a Unity Authentication credential. Initially, keep Unity's existing transport identity and associate Huawei identity with the local profile. Any persistent account linking or cross-device progress should use a backend that verifies Huawei authorization and applies an explicit mapping. Never transmit authorization tokens through ordinary room snapshots.

## Recommended feature scope

| Priority | Feature | Benefit and implementation boundary |
|---|---|---|
| First | Game launch initialization and Huawei game sign-in | A real supported HMS integration with visible authorization and game player retrieval. Display a signed-in profile state in the menu. |
| First | Lifecycle and failure handling | Handle consent rejection, cancellation, expired authorization, missing/old HMS Core, network loss and app resume without freezing or repeated pop-ups. |
| Next, if useful | Scan Kit | Scan a host's room QR code and pass its room code to the existing join flow. |
| Optional game feature | Local achievement book | Examples: first catch, finish five Golf cups, win a Football or Basketball match, visit four islands. These are our own features, not Huawei achievement APIs. |
| Later | Backend progress and public rankings | Needs persistence, score validation, abuse controls and additional operational scope. Existing room rankings can remain local to the current match. |
| Later | GameTurbo Engine | Investigate only with real Huawei performance measurements and any required Huawei API permissions. |

Scan Kit supports QR/barcode scanning and generation. A proposed room invitation contains a format/version marker and the existing room code. Validate the scanned value before passing it to `RoomService.Join`; do not execute arbitrary URLs or include Huawei authorization credentials in invitations. Retain manual code entry for players without camera permission. This changes the invitation input, while the existing multiplayer transport remains responsible for joining. [Current Scan Kit guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/service-introduction-0000001050041994)

Count local progress once per finalized match and per participating human player. Attach it to the authoritative finish transition in `FootballMatch`, `BasketballMatch`, `GolfMatchManager` and `FishingGame`, after reviewing each sport's final-result behavior. Separate online human matches from bot matches and practice. Host authority reduces accidental client errors but does not make a player-hosted score tamper-proof; public competitive rankings would need stronger validation. Keep global rankings out of the first competition milestone unless the rules demand them.

## Development approach

### Choose a Unity integration route

| Route | Assessment |
|---|---|
| A workshop-supported Unity wrapper | Prefer it if Huawei can demonstrate current Game Service APIs on Unity 6, ARM64 and the project's actual Gradle toolchain. Pin its version and inspect its dependency list. |
| Existing general HMS Unity plugins | Useful references, but do not import their complete samples or assume current API support. EvilMindDevs lists Unity 2019–2021 support; Unity's Huawei sample says Unity 2020 or higher. Neither statement establishes tested compatibility with this Unity 6 project or removal of obsolete APIs. |
| A small Android Java/Kotlin bridge | Recommended fallback: expose only the current launch, authorization, player retrieval and lifecycle calls through a C# adapter. It gives control over dependency scope, but requires native callback and Android lifecycle work. |

These compatibility limits come from the maintainers' documentation. [EvilMindDevs plugin](https://github.com/EvilMindDevs/hms-unity-plugin), [Unity Huawei sample](https://github.com/Unity-Technologies/HuaweiServiceSample)

Unity 6.3 supports calling Java/Kotlin plug-ins from C# through its Android JNI APIs. Use `AndroidJavaClass`/`AndroidJavaObject` and an explicit callback bridge; return Unity UI work to Unity's main thread. Verify which launch Activity Unity generates before adapting `onActivityResult`, Application initialization and pause/resume behavior. A bridge can be Java source or an AAR; it does not require another game engine. [Unity 6.3 Android plug-in documentation](https://docs.unity3d.com/6000.3/Documentation/Manual/android-plugins-java-code-from-c-sharp.html)

### Register and configure the app

1. Choose a durable developer account owned by the team or institution, register as a Huawei developer and complete identity verification.
2. Create an AppGallery Connect project and an **Android / Game** app. Finalize the publishing package identifier before configuring SDK credentials.
3. Register the **SHA-256 signing certificate fingerprint** in Project settings. Cover the certificate of the APK actually being tested and the eventual distributed signing certificate.
4. Enable **Game Service** and **Account Kit** in Manage APIs. Add Scan Kit only if selected for the scope.

Huawei documents the verified developer, Game category, fingerprint and API activation prerequisites. [AppGallery Connect preparations](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/config-agc-0000001281025916)

Huawei's pre-release checklist requires a package ending in `.huawei` or `.HUAWEI` **for a joint operations game**. Settle whether that arrangement applies before choosing between the current development ID and a distinct Huawei publishing variant; do not rename the project solely because Huawei sponsors the competition. The same checklist checks certificate/signature consistency, launch order, lifecycle and resource shrinking. [Game pre-release checklist](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/pre-release-check-0000001050121544)

Keep keystores, passwords and client secrets outside Git and outside the shipped client. Treat `agconnect-services.json` as application configuration, inspect its contents, and supply it through the selected integration's documented build process. The downloadable configuration is not a place to put server credentials.

### Integrate the Android dependencies

Add Huawei's Maven repository and the current Game Service/Account dependencies to Unity's generated Gradle structure through maintained templates or a deterministic postprocessor. The current guide names `com.huawei.hms:game:6.14.0.300` and `com.huawei.hms:hwid`; obtain a compatible Account Kit and AGConnect plugin version when implementing, then pin them. Its Android Studio instructions place `agconnect-services.json` in the app module. A Unity wrapper may copy it from a different source location, so inspect the exported Gradle project rather than blindly copying an old `StreamingAssets` tutorial. [HMS SDK integration guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/integrate-as-sdk-0000001050435953)

Keep Unity's supported SDK/NDK/JDK/Gradle combination. Old codelab values are examples, not instructions to downgrade Unity 6. Select only Account and Game Service initially, with their required transitive dependencies. Do not add IAP merely because an example includes it. Audit merged manifests, native libraries, duplicate classes, shrinker rules and final DEX/resources. A smaller direct bridge is a hypothesis until the actual APK is measured.

### Implement the supported launch and sign-in sequence

At game launch, present our privacy agreement when applicable, then initialize the Huawei SDK. Register Huawei's Application lifecycle support and call `JosAppsClient.init(AppParams)`. Initialize before sign-in and handle the Huawei consent screen. Rejected consent must not be treated as successful initialization. Do not retry endlessly on network failure. [Game initialization guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/game-start-0000001050123475)

After initialization succeeds, authorize using Account Kit with the **game scope** and an authorization code. Attempt silent authorization, then show interactive authorization when appropriate. After authorization succeeds, call `PlayersClient.getGamePlayer()`. Use the `openId` from that **Game Service Player**, not the Account Kit authorization object. The current guide gates game entry on successful player retrieval and describes server verification using the authorization code. Cancellation must not start another automatic sign-in loop. [Game sign-in guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/game-login-0000001050121526)

For the AppGallery game route, implement those gates explicitly. Keep Windows development playable with an unavailable platform adapter. Confirm the intended Android offline/guest policy with Huawei before retaining an entry path that bypasses the documented game authorization gate. A network error or denied consent must never produce a fake signed-in state.

Expand the platform boundary into asynchronous initialization and sign-in operations, a small identity object, explicit states such as initializing/authorized/error, and a capability query. Wire it into `AppRoot` and the menu. Do not implement the existing `ReportResult` method by calling removed Huawei ranking APIs. Keep local progress and optional QR invitations as separate responsibilities.

For verified production identities, use a backend to exchange/verify authorization and compare the returned game identifier. Never embed a client secret in the APK, and do not claim a client-provided ID has been server-verified. Mainland identity verification, addiction prevention and `savePlayerRole` have additional regional requirements; the current sign-in guide says role reporting is available only in the Chinese mainland. Start with the competition's intended Malaysia distribution and resolve any expansion separately.

## Testing and APK budget

At the start of this research, `Builds/Android/WhatTheFish-release.apk` measured **91,700,774 bytes**, matching the opening [release record](BUILD-SIZE.md). That baseline leaves **8,299,226 bytes to the 100,000,000-byte boundary**, so an integer-byte addition of at most **8,299,225** would still satisfy the strict rule. It exceeds the **75,000,000-byte development target by 16,700,774 bytes**.

The artifact changed during this documentation task: the first readiness run found no APK at that path, and the subsequent run observed **91,634,440 bytes**, with **8,365,560 bytes to the hard boundary** and **16,634,440 bytes above the working target**. This task did not build or functionally validate that replacement; the difference of **-66,334 bytes** is not an HMS integration delta. Neither observation validates an HMS-integrated build.

First produce an initialization/sign-in spike with no new art, ads, IAP or cloud database dependencies. Measure the final APK delta, inspect `Builds/SizeAudit/latest/`, remove unintended dependencies, and investigate supported resource/language filtering and shrinking. Preserve mandatory Huawei resources and test release builds after shrinking. Continue asset/dependency optimization toward the 75 MB working target; the remaining 8.3 MB is not a comfortable development reserve.

| Check | Required evidence |
|---|---|
| First install and authorization | A real Huawei phone shows privacy/authorization and successful `getGamePlayer`; profile state matches the current user. |
| Return launch and account changes | Silent authorization and renewal behave correctly; switching/revoking authorization cannot retain the previous profile. |
| Failure states | Denied consent, canceled sign-in, old/missing HMS Core and network loss leave responsive, truthful UI with deliberate retry. |
| Android lifecycle | Background/foreground, authorization Activity return and any floating window behave correctly; no repeated dialogs or callback loss. |
| Existing gameplay | Host and guest can join, travel together, play and finish all four sports; only one match runs per room. |
| Release packaging | IL2CPP ARM64, signing fingerprint, config, merged manifest and release shrinker behavior pass on the exact submission APK. |
| Optional QR invitations | A real device scans the host's QR and joins the intended room; invalid codes and denied camera permission have usable fallbacks. |
| Performance and size | Physical-phone touch, appearance, FPS/thermal behavior, WAN play and exact APK bytes are recorded. |

After feature checks, run `Tools/Build/Build.ps1 -Target AndroidSubmission`, inspect the audit and update the size records. Promote the complete validated Windows player to `Builds/WindowsFinal/` for a delivered gameplay change, including its build record. Run `Tools/Build/Check-TaskReady.ps1 -RequireApk`. Windows tests can verify the fallback adapter and gameplay; they do not establish Huawei phone compatibility or performance.

## Publishing to AppGallery

Huawei's Android release flow covers the APK, release territories, localized listing/assets, age rating, privacy information, AI content declaration, review information and release scheduling. Mobile APK uploads need 64-bit support. Mainland game publication approval is a separate regional requirement. Huawei states review usually takes **one to three working days**, which is not a guaranteed completion time. [Android APK release guide](https://developer.huawei.com/consumer/en/doc/distribution/app/agc-help-releaseapkrpk-0000001106463276)

Prepare the following release material alongside implementation:

1. A tested, signed release APK with the final identifier, increasing version code, matching certificate configuration and recorded hash.
2. The game name, description, icon, screenshots and contact details, consistent with the shipped game. Check whether the punctuation in WHATTHE FISH? is accepted in the listing.
3. Accurate age-rating answers and privacy disclosures covering Huawei account/services, Unity multiplayer, and optional camera-based QR scanning. Provide a reachable privacy policy and a practical contact/process for data rights.
4. Asset licenses/provenance and an accurate AI content declaration for generated assets or services. Match declarations to the actual package and selected territories.
5. Reviewer instructions covering authorization, solo play, host/join and four-island travel. Keep backend services available during review and supply test access when requested.
6. Open testing or public release as required by the organizer, leaving time for rejection fixes and another review.

The review guidelines require accurate information, stable functionality, usable test access, valid contacts and an available service during review. [AppGallery review guidelines](https://developer.huawei.com/consumer/en/doc/50104-overview)

Do not equate competition submission, an uploaded draft, approval for testing and a public store release. Each has different evidence. A signed self-contained competition APK remains subject to our strict byte limit even if store distribution uses another format.

## Proposed schedule and judging demonstration

| Malaysia dates | Proposed milestone |
|---|---|
| 14–15 October | Confirm registration, account ownership, kit acceptance, publication status, package suffix and judging devices. |
| 16–18 October | Attend the workshop; prove current initialization/sign-in on Unity 6 and a phone; measure SDK growth. |
| 19–23 October | Finish menu/account state, lifecycle, identity handling and existing-room regression checks. Add QR invitations only if justified. |
| 24–27 October | Complete phone QA and store materials; submit the required testing/release version early. |
| 28–31 October | Resolve review feedback, freeze the tested APK and prepare slides and the compressed demo. |
| 1 November | Submit the exact APK, presentation and video before 11:59 PM UTC+8. |

This schedule is a team planning estimate, not a Huawei delivery promise. Developer verification, native wrapper compatibility, device availability, SDK size and any required store approval are the critical dependencies.

Suggested three-minute demo: show Huawei authorization and the signed-in state; create/join a room, using Scan Kit if implemented; show shared island travel and a short sport sequence; finish with final results. Allocate about 25 seconds to account integration, 35 seconds to room joining, 100 seconds to the game and 20 seconds to the value proposition. Present real integration evidence and label local achievements/rankings accurately.

## Research verification

The competition poster and current Huawei documentation were read directly, including rendered document pages where web extraction omitted their content. The retirement finding is corroborated by both the current service introduction and SDK version history. Repository inspection covered platform stubs, multiplayer initialization, Android settings and the existing APK. No runtime code, SDK package, signing setup or build setting was changed for this research; an HMS-integrated APK size remains **unverified** until implementation and a fresh build.
