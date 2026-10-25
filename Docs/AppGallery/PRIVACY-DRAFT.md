# Privacy preparation — not for publication yet

This working draft records implementation facts. Publisher identity, contact,
audience, service regions, retention and deletion operations remain unresolved.
Do not upload it as the public privacy policy until those facts are completed.

Owner confirmed Malaysia as the initial market, free download, no advertising and
no in-app purchases. There is currently no policy website or domain. Publisher and public
support email can be supplied later. A 13+ audience is a proposal, not an approved
age restriction or platform rating. Do not add an age claim to the public policy
until the audience and corresponding controls are agreed.

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
| Deletion progress and revocation markers | Resume deletion and prevent stale clients recreating profiles | Local journal: account ID/scope/stage without tokens. New backend: SHA-256 identity marker and request time; production deployment and retention policy pending |
| IP/network and SDK technical data | Connectivity, authentication, rate limiting and service operation | Verify vendor disclosures, hosting regions and retention; application logging being disabled does not mean providers process no metadata |

## Public policy structure to finish

1. **Who operates the game:** legal publisher, public contact, effective date.
2. **Information used:** the categories above, optional versus necessary features.
3. **Why information is used:** gameplay, accounts, communication, support and safety.
4. **Who receives it:** actual providers and other players, with current vendor links.
5. **Permissions and choices:** camera, microphone, Bluetooth routing and selected
   images; explain denial and how permissions can be changed in device settings.
6. **Retention:** specific operational periods and backup deletion rules, separated
   by profiles, social data, reports, chat, voice and authentication. Owner to set
   these after checking provider settings; no invented immediate-deletion promise.
7. **Access, correction and deletion:** explain the implemented process and support
   channel. The prepared client/backend workflow still requires deployment,
   native-account verification and provider retention confirmation.
8. **Children and audience:** owner-approved audience and applicable regional policy;
   do not default to an invented age gate or market the social game to children.
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
