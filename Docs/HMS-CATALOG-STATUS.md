# HMS catalog and Android service availability

Checked **15 October 2026** for the WHATTHE FISH? Unity Android competition entry.

Huawei's public HMS landing page contains **57 catalog entries**. This is a complete inventory of that page, not a claim that all entries can be activated by a new Malaysian Android project. The catalog includes old product pages, hardware engines, frameworks and services closed to new users. Native HarmonyOS kits with similar names are separate platform integrations. [Official HMS catalog](https://developer.huawei.com/consumer/en/hms/)

The linked public introductions were inspected for supported platforms and retirement/access notices. A public guide or downloadable SDK does not prove that a new account can enable its cloud service. No authenticated developer project or phone was used to validate activation or Unity 6 compatibility.

## Status meanings

| Status | Meaning |
|---|---|
| Documented | A public guide describes the capability; no closure was found in the inspected introduction. Confirm region, API access, SDK compatibility and any commercial requirements before selecting it. |
| Conditional | Hardware, permissions, business qualifications, legacy interfaces or dependencies require explicit investigation. This is not a confirmed option for this game. |
| Partial retirement | The service remains documented, but specific functions have been removed. |
| Closed to new users | An explicit official restriction prevents treating it as a normal new-project option. |
| Framework | A separate application format/toolchain rather than an SDK feature to add to the current Unity APK. |

For our first integration, prefer Game Service with Account Kit, then investigate Scan Kit for room invitations. Do not add services solely to increase the kit count. The participant briefing expects an integration into the core gameplay/function; acceptance of a particular combination still needs organizer confirmation.

## App services

| Catalog entry | Status | Purpose and relevant limitation | Official source |
|---|---|---|---|
| Account Kit | Documented | Huawei ID authorization and sign-in. Game Service has an additional game-player identity step. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/introduction-0000001050048870) |
| Ads Kit / Petal Ads | Documented | Advertising and rewarded ads; publisher onboarding is a separate requirement. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/publisher-service-introduction-0000001070671805) |
| Analytics Kit | Closed to new users | Service access portal closed to new registrations on 30 June 2024. Existing projects and migration arrangements are different cases. | [Access notice](https://developer.huawei.com/consumer/en/doc/HMSCore-References/android-api-hianalytics-0000001050707170) |
| App Linking | Documented | Deep/deferred links; this is an AppGallery Connect service included in the HMS catalog. Its current guide distinguishes Android/iOS and HarmonyOS 5 or later. | [Guide](https://developer.huawei.com/consumer/en/doc/AppGallery-connect-Guides/agc-applinking-introduction-0000001054143215) |
| Awareness Kit | Conditional | Context such as headset status, ambient light and activity; capabilities vary by device and OS. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/service-introduction-0000001050171255) |
| Drive Kit | Documented | Huawei Drive file storage and management. This is separate from the removed Game Service saved-games API. Authorization and our own save format/conflict handling would still be needed. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/introduction-0000001050039630) |
| Dynamic Tag Manager | Conditional | Controls event-reporting tags. Do not assume it bypasses the Analytics registration restriction; inspect the chosen SDK's Analytics dependency and console eligibility. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/introduction-0000001050043907), [dependency example](https://developer.huawei.com/consumer/en/doc/HMS-Plugin-Guides-V1/accessing-dtm-0000001061669327-V1) |
| Game Service | Partial retirement | Launch, sign-in, updates, floating window and regional game services remain documented. Saved games were removed in May 2024; achievements, leaderboards and event reporting in September 2024. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/introduction-0000001050121216), [version history](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/version-change-history-0000001050123471) |
| Health Kit / Health Service Kit | Conditional | Authorized health/fitness data and wearable ecosystem access; not a reason to collect health data for this sports game. The old catalog introduction redirects to a missing page; use the current guide. | [Current guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/description-0000001558389985) |
| Identity Kit | Documented | User-authorized postal/contact address management. It is not game account authentication. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/introduction-0000001050040471) |
| In-App Purchases | Documented | Consumables, permanent products and subscriptions; merchant setup and purchase verification add scope. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/introduction-0000001050033062) |
| Location Kit | Documented | Device location, activity and geofences; high-precision features have separate geographic/licensing limits. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/introduction-0000001050706106) |
| Map Kit | Documented | Real-world map display and interaction. It does not generate our fictional island map. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/android-sdk-introduction-0000001061991291) |
| Navi Kit | Documented | Real-world route planning and turn-by-turn navigation. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/introduction-0000001185010752) |
| Push Kit | Documented | Cloud-to-device messages and notifications; invitations need their own server-side trigger and handling. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/service-introduction-0000001050040060) |
| Quick App | Framework | Installation-free application format; not a plug-in added to this Unity APK. | [Guide](https://developer.huawei.com/consumer/en/doc/quickApp-Guides/quickapp-doc-introduction-0000001082027756) |
| Scan Kit | Documented | QR/barcode scanning and generation. A practical candidate for supplying the existing multiplayer room code. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/service-introduction-0000001050041994) |
| Search Kit | Documented | Petal Search cloud APIs; service access and current commercial availability must be confirmed. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/introduction-0000001055591730) |
| Site Kit | Documented | Real-world place search, geocoding and time zones. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/android-sdk-about-the-service-0000001076188604) |
| Wallet Kit | Conditional | Digital passes, tickets and loyalty cards; issuer, region and business access conditions apply. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/service-introduction-0000001050042318) |

## Graphics

| Catalog entry | Status | Purpose and limitation | Official source |
|---|---|---|---|
| Accelerate Kit | Conditional | Native multithreading/performance capabilities; some are tied to Huawei/Kirin hardware. | [Guide](https://developer.huawei.com/consumer/en/doc/graphics-Guides/introduction-0000001050980807) |
| AR Engine | Conditional | Augmented reality; device/model support must be verified. | [Guide](https://developer.huawei.com/consumer/en/doc/graphics-Guides/introduction-0000001050130900) |
| Computer Graphics Kit | Conditional | Rendering framework and GPU extensions; not an automatic Unity URP upgrade. | [Guide](https://developer.huawei.com/consumer/en/doc/graphics-Guides/introduction-0000001050197938) |
| GameTurbo Engine | Conditional | Cooperation between a game and the OS for workload/resource adjustment; Huawei permissions and device testing are needed. | [Guide](https://developer.huawei.com/consumer/en/doc/HMSCore-Guides/gamekit-introduction-0000001050121538), [getting started](https://developer.huawei.com/consumer/fr/doc/hmscore-guides/gamekit-get-started-0000001079314102) |
| Scene Kit | Conditional | Separate lightweight 3D renderer; its existence does not establish an appropriate integration into our Unity renderer. | [Guide](https://developer.huawei.com/consumer/en/doc/graphics-Guides/service-introduction-0000001050163355) |

## Media

These are documented media capabilities, not necessary dependencies for ordinary Unity sound effects or video playback. Older published OS/device limits must be checked against the intended test phones.

| Catalog entry | Status | Purpose | Official source |
|---|---|---|---|
| Audio Editor Kit | Conditional | Audio editing, voice effects and other audio processing. | [Guide](https://developer.huawei.com/consumer/en/doc/Media-Guides/introduction-0000001153026881) |
| Audio Engine | Conditional | Hardware-dependent audio enhancement and headset monitoring. | [Guide](https://developer.huawei.com/consumer/en/doc/Media-Guides/introduction_services-0000001053333356) |
| Audio Kit | Conditional | Audio playback and decoding. | [Guide](https://developer.huawei.com/consumer/en/doc/Media-Guides/introduction-0000001050749665) |
| AV Pipeline Kit | Conditional | Audio/video processing pipelines and plugins. | [Guide](https://developer.huawei.com/consumer/en/doc/Media-Guides/introduction-0000001156025439) |
| Camera Engine | Conditional | Huawei camera modes and capture capabilities. | [Guide](https://developer.huawei.com/consumer/en/doc/Media-Guides/3180101) |
| Image Kit | Conditional | Image editing, effects and layouts. | [Guide](https://developer.huawei.com/consumer/en/doc/Media-Guides/service-introduction-0000001050199011) |
| Panorama Kit | Conditional | Interactive panoramic-image viewing on supported phones. | [Guide](https://developer.huawei.com/consumer/en/doc/Media-Guides/introduction-0000001050042264) |
| Video Editor Kit | Documented | Video editing and screen-recording SDKs; inspect the individual SDK rather than importing the full suite. | [Guide](https://developer.huawei.com/consumer/en/doc/Media-Guides/introduction-0000001101263902) |
| Video Engine | Conditional | Hardware-dependent video encoding and color processing. | [Guide](https://developer.huawei.com/consumer/en/doc/Media-Guides/service-introduction-0000001064620641) |
| Video Kit | Conditional | Video playback and HDR-related SDKs. | [Guide](https://developer.huawei.com/consumer/en/doc/Media-Guides/introduction-0000001050439577) |
| WisePlay DRM | Conditional | Digital media copyright protection and license-based playback. | [Guide](https://developer.huawei.com/consumer/en/doc/Media-Guides/introduction-0000001050041933) |

## Artificial intelligence

| Catalog entry | Status | Purpose and limitation | Official source |
|---|---|---|---|
| ML Kit | Documented | Text, speech, translation, vision and model-related capabilities. Select and verify the exact on-device/cloud feature; this does not supply sports bots or automatically train them. | [Guide](https://developer.huawei.com/consumer/en/doc/hiai-Guides/service-introduction-0000001050040017) |
| CANN | Conditional | Listed within Huawei's hardware AI ecosystem; not a general cloud NPC service. Verify the specific platform/toolchain. | [Catalog destination](https://developer.huawei.com/consumer/en/hiai#Foundation) |
| HUAWEI HiAI Engine | Conditional | Device AI capabilities; exact current hardware/SDK access needs confirmation. | [Catalog destination](https://developer.huawei.com/consumer/en/hiai#Engine) |
| HUAWEI HiAI Service | Conditional | HiAI ecosystem service entry; the shared destination and model-library documentation do not establish new-project eligibility for a specific API. | [Catalog destination](https://developer.huawei.com/consumer/en/hiai#Service) |

## Smart devices

| Catalog entry | Status | Purpose and limitation | Official source |
|---|---|---|---|
| CaaS Engine | Conditional | MeeTime-based calling and device collaboration. | [Guide](https://developer.huawei.com/consumer/en/doc/connectivity-Guides/virtualized-devices-calling-introduction-0000001051089010) |
| Cast Engine | Conditional | Screen collaboration; the inspected guide describes app support as coming soon, so it is not evidence of a generally available new game API. | [Guide](https://developer.huawei.com/consumer/en/doc/connectivity-Guides/introduction-0000001052079408) |
| DeviceVirtualization Engine | Conditional | Uses external device components as virtual phone components. | [Guide](https://developer.huawei.com/consumer/en/doc/connectivity-Guides/service-overview-0000001051608750) |
| OneHop Engine | Conditional | NFC-based device interaction and supported app continuation scenarios. | [Guide](https://developer.huawei.com/consumer/en/doc/connectivity-Guides/OneHop-introduction) |
| Share Engine | Conditional | Huawei Share file transfer; the inspected architecture depends on Nearby Service, whose new-developer registration is closed. Confirm access before planning an integration. | [Guide](https://developer.huawei.com/consumer/en/doc/connectivity-Guides/introduction-0000001062275178) |
| Wear Engine | Conditional | Communication between phone apps and supported wearables. | [Guide](https://developer.huawei.com/consumer/en/doc/connectivity-Guides/service-introduction-0000000000018585) |

## Security

| Catalog entry | Status | Purpose and limitation | Official source |
|---|---|---|---|
| FIDO | Conditional | FIDO2 and biometric authentication; supported authenticators/devices matter. | [Guide](https://developer.huawei.com/consumer/en/doc/Security-Guides/introduction-0000001051069988) |
| Keyring | Conditional | Credential storage/sharing; the published device matrix has OS upper bounds that require compatibility investigation. | [Guide](https://developer.huawei.com/consumer/en/doc/Security-Guides/introduction-0000001133141068) |
| Safety Detect | Conditional | Device/app/URL and related security checks; validate individual API eligibility. This is not a complete anti-cheat system. | [Guide](https://developer.huawei.com/consumer/en/doc/Security-Guides/introduction-0000001050156325) |
| DataSecurity Engine | Conditional | Secure storage/file-protection capabilities; its catalog link leads to older version history, which does not establish current new-project access. | [Linked version history](https://developer.huawei.com/consumer/en/doc/Security-Guides/sdk-version-0000001117302118) |
| LocalAuthentication Engine | Conditional | Device facial authentication on supported Huawei devices. | [Guide](https://developer.huawei.com/consumer/en/doc/Security-Guides/introduction-0000001051219741) |

## System and connectivity

| Catalog entry | Status | Purpose and limitation | Official source |
|---|---|---|---|
| 5G Modem Kit | Conditional | Specialized modem/network measurements on supported Huawei chipsets. | [Guide](https://developer.huawei.com/consumer/en/doc/system-Guides/introduction-0000001079685514) |
| HEM Kit | Conditional | Enterprise device deployment and management. | [Guide](https://developer.huawei.com/consumer/en/doc/system-Guides/introduction-0000001058328675) |
| MDM Engine | Conditional | Enterprise device/app management; Huawei authorizes eligible enterprise apps and API scenarios. | [Guide](https://developer.huawei.com/consumer/en/doc/system-Guides/introduction-0000001053109486) |
| Nearby Service | Closed to new users | New-developer registration stopped on 10 September 2024; existing activated developers are a separate case. | [Access notice](https://developer.huawei.com/consumer/en/doc/system-Guides/nearby-applications-0000001059239781) |
| Network Kit | Documented | HTTP/network request and upload/download capabilities. It does not replace NGO/Relay real-time game synchronization. | [Guide](https://developer.huawei.com/consumer/en/doc/system-Guides/network-introduction-0000001050440045) |
| Wireless Kit | Conditional | Device-specific wireless QoE and network scheduling capabilities. | [Guide](https://developer.huawei.com/consumer/en/doc/system-Guides/introduction-0000001050749815) |

## Related AppGallery Connect services

AppGallery Connect has a separate catalog. Do not describe every AGC backend tool as an HMS Core kit, or assume it automatically satisfies the competition's integration criterion.

- **Crash:** the official support page says new Android and iOS apps are not supported. [Restriction](https://developer.huawei.com/consumer/en/doc/AppGallery-connect-Guides/agc-crash-support-0000001117507136)
- **APM:** the official guide says new Android, iOS and web apps are not supported. [Restriction](https://developer.huawei.com/consumer/en/doc/development/AppGallery-connect-Guides/agc-apm-android-stopapm-0000001052807287)
- **Remote Configuration:** has a separate Android service guide; confirm availability in the team's project and organizer acceptance before selecting it. [Guide](https://developer.huawei.com/consumer/en/doc/appgallery-connect-Guides/agc-remoteconfig-android-0000001056347093)

Other infrastructure such as Auth Service, Cloud DB, Cloud Storage and Cloud Functions needs its own service/region/plan assessment. This inventory does not certify those services or cover native HarmonyOS's separate kit catalog.

## Selection for the competition

1. Prove Game Service initialization and game sign-in with Account Kit on the actual judging platform.
2. Ask whether Scan Kit room invitations satisfy the core-function requirement, then measure a minimal Android integration if accepted.
3. Add monetization, hardware, health, AI or media SDKs only when the game needs the capability and its activation, compatibility and APK growth have been verified.
4. Do not plan around Analytics, Nearby, new-app AGC Crash/APM, or removed Game Service features without explicit applicable access confirmation.

See [the game integration and briefing analysis](HUAWEI-GAME-SERVICE-RESEARCH.md) for project-specific implementation, submission requirements and remaining organizer questions.
