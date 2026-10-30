# Support, reporting and retention plan

Prepared 26 October; retention periods adopted by the owner on 28 October 2026:
match details 30 days after expiry, closed report metadata 90 days, report
evidence 30 days after closure and support email 90 days after resolution.
The owner retained all ages and requested resolving child protections first;
submission remains pending. A later real cloud P40 check covers startup, guest
entry and one Golf shot; full device acceptance remains open. Other rows below remain
operating proposals unless separately confirmed.

**The deletion fix and retention handler are deployed, with both indexes
verified. Daily cleanup is configured at 03:17 Malaysia time; first scheduled
execution remains unverified. Final public policies remain drafts.**
See [retention rollout and restore procedure](RETENTION-ROLLOUT.md).

LIEW KAIY BIN handles support, privacy and player reports through
benliew28262826@gmail.com and the private report records. The owner selected
daily review and the same features for everyone, including children. There is
no launch deadline. No separate child mode was requested or implemented.

## Recommended retention schedule

| Record | Proposed rule | Work required before adopting it |
| --- | --- | --- |
| Profile, friendships, blocks, wallet and owned cosmetics | Keep while the account is maintained; remove associated personal records on verified account deletion. Do not silently expire paid-for effort or balances because an account is inactive. | Verify deletion across every store and provider; settle any justified minimum anti-abuse records separately. |
| Detailed match rosters and results | Remove identifying match detail 30 days after fixed expiry, as adopted. Keep only the minimum account-linked award ledger needed to prevent duplicate rewards while the account exists. | Handler and daily schedule deployed; shared-roster/receipt deletion fix deployed. Complete authenticated live acceptance and verify the first scheduled run. |
| Invitations and transient join proofs | Delete expired/resolved records within 24 hours; consumed proofs as soon as no longer needed. | Current 120-second invite and four-hour match expiry are logical states, not physical deletion. Implement cleanup and verify retries. |
| Closed report case metadata | Delete 90 days after closure. Reassess open cases at least every 30 days. | Record closure/review dates and an explicit, minimal investigation hold when justified; prevent indefinite unattended cases. |
| Report photos and chat excerpts | Delete 30 days after closure; open cases remain subject to daily review. A future hold exception needs explicit design/disclosure. | Handler separates evidence expiry from metadata; daily schedule enabled, first run unverified. Restrict private access. |
| Support email | Delete identifiable correspondence and attachments 90 days after resolution; retain anonymous bug notes separately when useful. | Owner performs mailbox cleanup and records any justified hold. Do not copy credentials or unnecessary identity documents into a case log. |
| Ordinary room text | Session memory only; clear local history on leaving/new room. Reported excerpts follow report rules. | Current implementation follows room travel/host migration; it cannot remotely erase copies other players already received. |
| Voice | No application recording/transcription. Confirm provider metadata retention and request handling. | Verify actual Vivox project settings and any consent obligations before claiming suitability for children. |
| Deletion revocation markers | Keep the minimum hash/time required to prevent stale credentials or restores recreating a deleted profile; review periodically. | Choose a safe expiry only after verifying credential lifetimes, restore procedures and re-creation defenses. An arbitrary 30/90-day purge would be unsafe. |
| Backups and provider logs | Use the actual provider's configured expiry, with restricted restoration and reapplication of deletions. | Record real plan, regions and settings; do not promise instantaneous backup erasure. |

## Daily operating routine

1. Read the support inbox and private unresolved reports. Prioritize credible
   threats, child-safety concerns, account compromise and deletion failures.
2. Review only necessary evidence. Reports contain reporter-supplied snapshots,
   not authenticated proof; do not punish solely because a report exists.
   The current feature adds no automatic ban or completed enforcement console.
3. Track case ID, received/reviewed date, owner, brief decision and next step in
   a private operator record. Keep access limited to the owner. Avoid copying
   names, images or message excerpts into public issues or ordinary Git files.
4. Reply through the existing support conversation where available. Daily review
   is an operating plan, not a guarantee that every case receives a response or
   resolution within one day. There is no in-game report-outcome notification yet.
5. For deletion failures, verify account ownership without passwords, inspect
   the interrupted stage, retry safely, and escalate provider-held data using
   the relevant provider procedure. Preserve minimal revocation defenses.
6. Close resolved cases, assign an evidence expiry and review held/open cases.
   Before launch, implement and test the chosen purge rules with disposable
   fixtures, plus restore-and-reapply-deletion tests.

## Concrete gaps found in the current implementation

- The deployed deletion fix removes a deleted participant from another host's
  roster/receipt while preserving the host's replay-prevention key and wallet.
  A changed active match is cancelled for rewards. Transaction failure and a
  controlled restore/reapply scenario pass locally. Authenticated live deletion
  and provider restore acceptance remain unverified.
- The deployed scheduled handler clears closed-case evidence at 30 days and
  metadata at 90 days, and deletes match records 30 days after expiry. It is
  enabled by the deployment flag and has no public HTTP trigger. Open/undated cases remain
  for operator review. A future investigation-hold exception needs an explicit
  design and disclosure; the code contains no hidden hold bypass.
- No verified guardian-consent flow or age-aware initialization was found.
  The owner's all-ages intention does not establish an official rating or
  satisfy provider/child-privacy requirements. Resolve this before public release.
- Processing regions, backup plan, provider logs, request handling and real
  Huawei device behavior still need verification. The daily retention schedule
  is configured; provider consent settings were not changed by that activation.

## Provider references checked during preparation

- [Unity Authentication account deletion](https://docs.unity.com/en-us/authentication/delete-accounts)
  deletes the Authentication account; associated service data needs separate handling.
- [Vivox privacy overview](https://docs.unity.com/en-us/vivox/privacy/privacy-overview)
  describes default metadata retention up to 30 days and ephemeral voice,
  provider-assisted access/deletion requests, and applicable parental-consent
  requirements. The game's room text uses its own multiplayer flow; do not apply
  Vivox Text retention to that flow automatically.
- [Cloudflare D1 limits](https://developers.cloudflare.com/d1/platform/limits/)
  list Time Travel recovery windows of 7 days on Free and 30 days on Paid.
  The actual project plan and any other exports remain to be confirmed.

The [local website](../../Website/README.md) and bundled documents deliberately
identify these as preparation drafts. Once operations and provider settings are
confirmed, revise all three language versions together before publishing a final notice.
