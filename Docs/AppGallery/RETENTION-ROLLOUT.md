# Retention and deletion rollout

Owner adopted the four periods on 28 October 2026. The deletion fix and retention
handler are deployed, and both indexes are verified. On **29 October the daily
schedule was activated for 03:17 Malaysia time (19:17 UTC)** using the adopted
periods. The first scheduled invocation has not yet been verified. The signed
APK is unchanged by this backend-only rollout.

## Prepared behavior

- `retention.mjs`: removes match records 30 days after their fixed `expires_at`;
  participant rows cascade, while wallets/coin history remain intact.
- Closed reports (`reviewed` or `dismissed`, with a recorded `reviewed_at`): clear
  free-text details/context/excerpts, snapshot photos and review notes at 30 days;
  delete case metadata at 90 days. Open cases and cases without a closure date
  remain for the owner's daily review. Migrated legacy markers expire only with
  their corresponding closed case.
- Account deletion also scrubs the player's ID from other hosts' roster and
  receipt JSON. Host replay keys and other wallets remain. A changed active
  roster cancels rewards instead of paying against incomplete participants.
- All changes in each cleanup or account-deletion batch are transactional.
  Failures roll back rather than claiming deletion. Repeated runs are safe.
- `account_deletions` hashes are never purged by this job. They protect against
  stale tokens and restored profiles. No arbitrary hash expiry is introduced.
- Support email remains a manual owner responsibility: delete identifying
  mail/attachments 90 days after resolution, keeping anonymous bug notes separately.

## Checks completed

The complete local backend suite passes **85 tests**, including eight new
retention/deletion tests for cutoff boundaries, pending cases, unrelated wallets,
legacy markers, repeat runs, rollback and controlled restore/reapplication.
Worker dry-run bundling passes. Evidence is in
`Builds/AppGalleryReleaseQA/retention-tests.txt` and `retention-worker-build.txt`;
the rollout rerun is in `Builds/AppGallerySubmissionQA/requirements-backend-tests.txt`
and `requirements-worker-build.txt`.
No smoke script that creates real Unity accounts was run after the approved reset.

## Initial disabled deployment — 28 October 2026

- Worker version: `c01a3545-86dc-4721-864d-2e1c986117db`, 100% traffic.
  Deployment `8ecc792b-3f91-4702-83ce-8d48f1361f98`,
  created `2026-10-28T15:45:55.968219Z`.
- Previous live version / rollback pointer:
  `8cc459f0-b24a-4c19-bbb2-6f3e6f8ac7bd`. This is a Worker code rollback,
  not a database restore; do not resurrect deleted profiles.
- Existing Cloudflare CLI access deployed the Worker. Its D1 scope was absent;
  the already signed-in D1 dashboard applied exactly migration
  `0008_retention_indexes.sql`. Both index names and definitions were read back.
  No access scope was expanded and the dashboard-managed database was not
  subjected to the full historical migration sequence.
- Health returned HTTP 200; unauthenticated deletion capability returned HTTP
  401 / `invalid_session`. These checks establish availability and an auth
  boundary, not a live authenticated deletion or provider restore test.
- One profile, zero reports, zero matches and 567 revocation hashes were present
  before the index migration. The profile appeared after the earlier reset;
  its origin was not established and it was left untouched. The post-migration
  read returned the same row counts, with two retention indexes instead of zero.
- `ENABLE_RETENTION_CLEANUP=false`; no daily trigger, manual purge, public cleanup
  endpoint, identity-creating live test or additional privileged credential was
  added during this rollout.

Rollout evidence is under `Builds/AppGallerySubmissionQA/`: deployment snapshots,
health results, `retention-indexes-verified.png` and `backend-rollout.json`.

## Daily schedule activated — 29 October 2026

- Worker version `f1d264ca-32c7-4bae-a3f5-6c7fcf887a6c`, deployment
  `a8381c9e-b594-4137-b10d-3cb946bbd440`, 100% traffic; created
  `2026-10-28T18:15:50.922673Z` / 29 October 02:15 MYT.
- `ENABLE_RETENTION_CLEANUP=true`; one daily trigger, `17 19 * * *`.
  Wrangler confirmed both Worker deployment and trigger deployment. The complete
  85-test suite and Worker bundling passed for activation. Health returned 200;
  unauthenticated deletion returned 401 / `invalid_session` afterward.
- Pre-activation read: two indexes, one profile, zero reports, zero matches,
  567 revocation hashes and zero records eligible for each retention cutoff.
  Profiles, wallets, open cases and revocation hashes are outside this purge.
- No public purge endpoint, new credential, expanded CLI scope, paid plan or
  identity-creating live smoke was introduced. Existing private log settings
  remain unchanged. Support-mail deletion remains the owner's manual operation.
- Cloudflare documents [UTC scheduling and propagation](https://developers.cloudflare.com/workers/configuration/cron-triggers/).
  First expected run: **29 October 03:17 MYT**. Check Settings → Trigger Events →
  View events for the actual outcome; configuring a trigger is not evidence
  that its scheduled handler has completed successfully.

Evidence: `Builds/AppGallerySubmissionQA/retention-activation.json`, activation
test/bundle/deploy output, deployment snapshots and post-deployment health JSON.
The earlier disabled deployment remains the designated immediate code rollback.
To stop cleanup, deploy `ENABLE_RETENTION_CLEANUP=false` **and** an explicit
`triggers.crons=[]`; omitting the trigger field can preserve an existing schedule.
Do not restore database backups to roll back this configuration.

## Acceptance remaining

1. Verify updated authenticated deletion using approved disposable identities,
   including shared-match references, retries and revocation.
2. Inspect the first scheduled result, monitor failed runs, and verify boundary
   counts. A daily schedule can run up to a day after a cutoff; the final policy
   must describe the actual cadence. Check provider limits before a large backlog.
3. Complete live provider restore acceptance and the final children/provider
   arrangements. Publish matching English, Simplified Chinese and Malay notices
   that describe the effective processing and support handling.

There is no public cleanup endpoint. Do not expose a secret-bearing operator URL
or use ordinary player credentials to trigger bulk cleanup.

## Restore without resurrecting deleted profiles

Keep the service closed to player traffic throughout the restore. Preserve the
current minimal revocation set separately before restoring older database data;
an old backup alone may predate a deletion marker. Merge the preserved markers
back, without overwriting newer requests. Enumerate restored profile IDs, compare
their deletion hashes with that set and reapply the normal transactional deletion
for each match. This also scrubs references in other hosts' restored records.
Verify no revoked profile or embedded ID remains before reopening traffic.
Do not restore the pre-launch account-reset database bookmark as routine rollback.

The automated fixture models retained markers plus restored old profile/shared
match rows and verifies that stale writes are blocked and reapplication removes
them. It is not a claim that a live provider restore has been tested. A full live
restore rehearsal requires its own reviewed maintenance window and evidence.
