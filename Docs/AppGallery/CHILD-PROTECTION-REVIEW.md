# Malaysia 12+ release: unresolved child permissions

Reviewed 28 October 2026. The owner retained all ages and explicitly chose to
resolve child protections before submission. This is an implementation review,
not an official content rating or a legal determination. The owner subsequently
requested proceeding with submission despite having no Huawei test phone.
That request does not establish the missing child protections. The draft is
still unsubmitted; see the [current submission attempt](SUBMISSION-ATTEMPT-2026-10-28.md).

## Current findings

- `PlayerAccount.EnsureSession` can initialize Unity services and authenticate
  an online guest; `SignIn` supports provider-linked accounts. Neither path has
  age-aware authorization or verified guardian-consent state.
- `RoomService` can create/join rooms after online initialization. Profile names,
  photos, friend search and invitations are online identity/social features.
- `VivoxRoomVoiceTransport.Connect` initializes and logs into Vivox after an
  explicit voice action. It sets capture/output privacy before login/channel
  transmission, and room voice starts muted. These are useful controls, but
  microphone permission and pressing Join do not verify guardian consent.
- Existing name/text filters, reporting, blocking and mute controls are useful;
  they do not verify age or consent, review every uploaded photo, or moderate
  every spoken message. Do not describe chat as completely safe for children.
- No verified guardian workflow, consent revocation record or server enforcement
  of child feature permissions was established by the source review.

## Design required before implementation

1. Determine the rules applicable to the Malaysian launch and the selected
   providers, including who is treated as a child and when verified parental
   consent is required. Do not substitute an invented universal 13+ threshold.
2. Select an adequate recorded guardian-permission process and establish whether
   verified parental consent is required for the actual services/processing.
   Specify minimum evidence, private handling and refusal/revocation. A checkbox,
   email address or self-declared birthday alone is not verified parental consent.
3. Gate online identity, social uploads, room communication and SDK initialization
   before restricted child data is sent. Keep a local/offline route available
   during refusal or pending verification. Scope each approved permission and
   enforce it on the service as well as in the UI.
4. Provide guardian controls and a clear default for text/voice/photo/friend
   features. Revoke access promptly when consent is withdrawn; leave voice/rooms
   and prevent stale tokens from restoring permissions. Document any age-related
   feature difference rather than promising identical unrestricted access.
5. Publish concise notices in English, Simplified Chinese and Malay, in the game
   and website. Keep only necessary consent/version/time records; never place
   identity documents, birthdays or guardian contact data in public Git/issues.
6. Verify provider processing locations, consent integration, access/deletion
   requests and backup/log retention. Complete the store questionnaire honestly,
   including user communication and uploaded profile content.

## Acceptance evidence still needed

Refusal/pending/revocation must prevent unauthorized outbound SDK requests,
identity creation and social uploads. Test fresh install, offline use, switching
devices/accounts, restored data and direct API bypasses. Verify notices and
controls in all three languages on physical Android/Huawei devices. Record
actual provider configuration; do not infer phone behavior from Windows tests.

Unity's [Vivox privacy documentation](https://docs.unity.com/en-us/vivox/privacy/privacy-overview)
requires verified parental consent where applicable and explains that the
developer controls consent handling and privacy disclosures. It also documents
provider-held identifiers and request handling. That conditional requirement
does not, by itself, certify this game's current all-ages configuration.

## Verified Huawei requirements after the approved 12+ rating

The owner accepted Huawei's calculated 12+ rating for Malaysia. This is a content
classification, not evidence of parental consent or an implemented age gate.

Clarification after the owner's question: **KWS is optional.** A KWS integration
or an adults-only online restriction is not an established mandatory binary
choice. The owner has not selected either. Establish which consent requirements
apply to this Malaysia-only, free/no-advertising/no-IAP game and its actual
provider arrangements. Individual publication or a free price alone does not
establish a personal/household exemption. Do not impose a blanket under-18 online
restriction or promise unrestricted minor access solely from the content rating.
Huawei's [AGC agreement, sections 2.8 and 5.2](https://terms1.hicloud.com/agreementservice/developer/getAgreementTemplate?agrType=1051&country=ove&language=en_us&version=2021092701)
separates rating from compliance and requires a lawful basis for personal-data
processing. Malaysia's authority explains the PDPA's
[commercial-transaction scope and exemptions](https://www.pdp.gov.my/ppdpv1/en/akta/application-and-non-application-of-the-act/).
The verified-consent condition in Vivox remains conditional on applicable law;
it is not a universal requirement to adopt KWS. Any existing platform/guardian
consent must be shown to cover the game's actual processing before relying on it.

Malaysia's published [2013 Regulations, regulation 3](https://www.pdp.gov.my/ppdpv1/wp-content/uploads/2024/07/Peraturan-peraturan-Perlindungan-Data-Peribadi.pdf)
specify parent/guardian consent for under-18 data subjects where those consent
requirements apply. Applicability must be assessed within the PDPA's scope;
the store rating alone does not answer that question. The existing Vivox
conditional verified-consent requirement also remains a provider release check.

The live official [Minor Protection rules](https://developer.huawei.com/consumer/en/doc/app/50104-08)
were checked directly on 28 October 2026 (page updated 28 September 2026).
Clause 8.12 requires a dedicated children's policy when children's personal
data is processed. Clause 8.13 requires parent/legal-guardian permission before
processing minors' personal data, and explicit consent for sensitive data and
permissions. Neither clause names KWS or prescribes a verification vendor.
These Huawei requirements must be addressed independently of an assessment of
Malaysian PDPA applicability. The separate [User Privacy rules](https://developer.huawei.com/consumer/en/doc/app/50104-07)
cover advance notice, consent where required, withdrawal, deletion and minimization.

Unity's [Terms of Service, 8.6](https://unity.com/legal/terms-of-service) require
the applicable child-data settings/signals and authority for transmitting child
data, with consent/other bases as specified. Confirm the exact project setup
rather than inferring a universal KWS requirement from the Vivox overview.

The next step is to establish an acceptable recorded permission method for this
game. A self-managed parent email approval process is a candidate to ask the
providers about; it has not been certified as adequate verification. See the
[minimum-requirements matrix](MINIMUM-REQUIREMENTS.md) and
[submitted support questions](PROVIDER-QUESTIONS.md). Huawei **D650176** and
Unity **#3623478** are awaiting substantive replies; receipt confirmations do
not establish an adequate permission method.

KWS remains an optional provider if stronger assurance is needed. Epic's
[parent-verification flow](https://dev.epicgames.com/docs/kids-web-services/parent-verification-service/pv-service-flow)
does not itself collect the game's parental consent or supply its notice.
No Epic account, product, license, credential, consent UI or game restriction
has been created here. Any implemented flow must ship all three languages,
server enforcement, withdrawal tests and fresh required player builds.
