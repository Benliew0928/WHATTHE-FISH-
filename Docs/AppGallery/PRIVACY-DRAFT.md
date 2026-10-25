# Privacy preparation — not for publication yet

This working draft records implementation facts and owner decisions. Processing
regions, retention, child/guardian arrangements and deletion operations remain
unresolved. Do not upload it as the public privacy
policy until those facts are completed.

Owner confirmed Malaysia as the initial market, free download, no advertising and
no real-money in-app purchases. On 26 October 2026 the owner confirmed a personal/individual
publisher named LIEW KAIY BIN, public support/privacy email
benliew28262826@gmail.com, an all-ages
intended audience and personal responsibility for support, reports and deletion
exceptions. There is no policy website or domain yet. All ages replaces the
earlier 13+ proposal; it is not an official
rating or evidence that current communication/data controls are suitable for
children. Retain Google login if compatible with AppGallery acceptance.

## All-ages preparation

- Inventory children's possible access to accounts, names/photos, player search,
  friendships, invitations, room text chat and optional voice. Assess the needed
  child/guardian controls and reporting workflow before making public claims.
- Determine applicable consent requirements for the intended Malaysian market
  and actual providers; do not invent an age threshold or claim that choosing
  an all-ages audience satisfies these requirements.
- [Vivox's privacy overview](https://docs.unity.com/en-us/vivox/privacy/privacy-overview)
  requires verified parental consent before submitting children's data where
  applicable law requires it. Review how access and any required consent would
  be handled before service initialization/data transmission.
- The owner is the designated operator; handling procedures, response targets,
  escalation and retention periods still need to be documented. Reporting and
  blocking features alone do not prove operational coverage.
- No age gate, parental-consent flow or communication restriction is added or
  certified by this documentation-only preparation.

## Data inventory

| Data / access | Purpose and recipients | Facts or follow-up |
| --- | --- | --- |
| Huawei/Google identity tokens, Unity player ID | Sign-in, linking and recovery; Huawei/Google, Unity and Huawei verification Worker | Tokens are sensitive; no email/phone/profile-photo scopes requested by the documented native flow |
| Display name, chosen avatar, optional profile picture | Player identity, search and friends; Cloudflare D1 and other authorized players | Uploaded picture is a 128x128 RGB thumbnail, not the original photo |
| Friends, blocks, reports, invitations | Social features and abuse handling; Cloudflare D1 | Reports require an operator; invitations expire after 120 seconds but expiry does not alone prove physical record deletion |
| Room membership and match state | Online play and synchronization; Unity multiplayer/Relay and room participants | Map actual retention and host-held data before final policy |
| Text chat | Communication with room participants | Verify history/cache lifetime and deletion behavior; do not promise no retention without checking |
| Microphone audio | Optional room voice; Unity Vivox and room participants | Explicit join and initially muted; verify service retention/recording settings before policy claims |
| Camera / chosen QR image | Scan a room invitation using Huawei Scan Kit | Camera permission on demand; document picker grants access only to the selected image; no broad storage permission |
| Optional profile image selection | Profile thumbnail | Verify native picker and permission behavior in the final manifest/device |
| Local settings, identity caches and session credentials | Remember preferences and account session | UGS session persistence uses its normal PlayerPrefs-backed storage; do not claim Keystore encryption |
| Coins and cosmetic selections | Remember account balance, daily streak, match rewards and owned/equipped cosmetics | Cloud wallet and transaction ledger plus local display/retry cache; account deletion removes owned records. Earned-coin cosmetic purchases are enabled; no real-money payments are offered. Retention policy still needs owner confirmation |
| Deletion progress and revocation markers | Resume deletion and prevent stale clients recreating profiles | Local journal: account ID/scope/stage without tokens. Backend: SHA-256 identity marker and request time; activated 26 October, retention policy pending |
| IP/network and SDK technical data | Connectivity, authentication, rate limiting and service operation | Verify vendor disclosures, hosting regions and retention; application logging being disabled does not mean providers process no metadata |

## Public policy structure to finish

1. **Who operates the game:** individual publisher LIEW KAIY BIN, public
   contact benliew28262826@gmail.com, and a future effective date.
2. **Information used:** the categories above, optional versus necessary features.
3. **Why information is used:** gameplay, accounts, communication, support and safety.
4. **Who receives it:** actual providers and other players, with current vendor links.
5. **Permissions and choices:** camera, microphone, Bluetooth routing and selected
   images; explain denial and how permissions can be changed in device settings.
6. **Retention:** specific operational periods and backup deletion rules, separated
   by profiles, social data, reports, chat, voice and authentication. Owner to set
   these after checking provider settings; no invented immediate-deletion promise.
7. **Access, correction and deletion:** explain the implemented process and support
   channel. The backend is activated; the client workflow still requires native
   account verification on a phone and provider retention confirmation.
8. **Children and audience:** all ages is the owner's intended audience; complete
   the child/guardian and regional-policy review above before describing actual
   protections or suitability. Official content rating remains pending.
9. **International processing/security:** confirmed hosting locations, actual
   safeguards and provider roles; avoid unsupported guarantees.
10. **Changes and contact:** version date, notice mechanism and contact address.

## Account-deletion implementation acceptance

- In-game entry with clear consequences; blocked during an active/joining room.
- Reauthenticate the correct linked account; define guest ownership verification.
- Stop voice and network activity, clear or disable stale profile writes.
- Delete Cloudflare profile/social records while authentication is still valid.
- Confirm deletion requirements for each Unity service actually used; delete UGS
  Authentication only after associated data cleanup has a recoverable outcome.
- Persist bounded retry state for partial failures without retaining raw tokens
  in a new file; do not report success while any required stage remains pending.
- Clear the deleted user's local credentials, profile/photo caches and identity.
- Do not delete the player's underlying Huawei or Google account.
- Verify deletion using disposable test identities, including network failure
  between stages, expired authorization and repeated requests.
- Align public support handling and provider backup retention with the policy.

Source references: [login](../PLAYER-LOGIN.md),
[backend](../../Tools/AccountServer/README.md), [voice](../ROOM-VOICE.md),
[scanner](../HUAWEI-SCAN-KIT.md).
