# Player reporting and text safety

Implementation and release validation, 26 October 2026.

## Player flow

One **Report player** action replaces the separate name/photo buttons in the
player profile, reached from Friends, lobby seats and character nameplates.
Football and Basketball expose a warning triangle with an exclamation mark at the end of each human player's row in
the expanded scoreboard and final statistics. Golf exposes it beside the name
in expanded standings (also used for results). Fishing's live catch row opens a
small player card with Report player; final standings have the same report icon directly.
Self and bot report actions are hidden. Permanent profile IDs identify targets;
display names and changing ranking positions never identify report recipients.

The shared modal offers name, picture, abusive text/harassment, sexual content,
disruptive behaviour, cheating/exploits, spam/scams and Other. Details allow 500
UTF-16 units and are required for Other. Private explanations can quote abuse.
The current player and reason remain visible before submission. Cancelling
sends nothing; network failure preserves the form and permits an idempotent
retry. Success requires an acknowledged version-2 receipt. Local controls are
blocked while the form is open; room timers and other players continue.
All game-authored strings use the English, Simplified Chinese and Malay catalog.

## Vocabulary and enforcement

[TextSafety.json](../Game/Assets/_Game/Resources/TextSafety.json) is a small,
independently authored initial vocabulary, shared by the C# client and Node
backend. No external dictionary or AI model is bundled. Public collections
[LDNOOBW](https://github.com/LDNOOBW/List-of-Dirty-Naughty-Obscene-and-Otherwise-Bad-Words)
and [V2](https://github.com/LDNOOBWV2/List-of-Dirty-Naughty-Obscene-and-Otherwise-Bad-Words_V2)
are possible expansion references; preserve their respective attribution and
provenance if importing entries later. The initial vocabulary needs continuing
native-speaker feedback and additions for local slang.

Matching normalizes compatibility forms and case, removes invisible format
characters, checks common number/symbol substitutions and repeated/separated
letters, and uses letter boundaries for Latin words. Chinese phrases can match
without spaces. The matcher scans every language together, independently of UI
language. Benign names, animal words, identity terms and sexual-health phrases
are in the regression fixtures to protect against indiscriminate filtering.

New name saves are checked on the device and authenticated backend. Legacy
nickname RPCs are checked by the host. Chat is checked before local submission
and again before host broadcast; rejected messages do not enter room history.
Existing saved profiles are not automatically renamed. Room protocol is
unchanged. A modified host can still bypass host enforcement; this is not a
trusted dedicated moderation service. No voice transcription, image classifier,
contextual AI decision or automatic ban is implemented.

## Incident storage and operator review

Migration [0007](../Tools/AccountServer/migrations/0007_report_incidents.sql)
adds `report_incidents`, retaining the original `reports` table. Existing reports
are copied as legacy incidents. Their profile metadata reflects migration time;
historical photos and message evidence were never recorded and cannot be
reconstructed. New submissions capture server profile name, public code,
revision and avatar version; picture reports also retain the current RGB
thumbnail. This captures the server profile at submission time, which may differ
from a stale client display.

Abuse/sexual/spam reports may attach the last five available text messages from
the reported player in this room. These excerpts and room/sport context are
**reporter-supplied evidence**, not independently authenticated proof. No voice,
unrelated players' chat, bearer token or room join code is included. Details and
evidence are never added to public profile responses.

The API derives the reporter from the verified Unity session. Request IDs make
retries idempotent; changing a submitted payload with the same ID is rejected.
Insertion atomically enforces 20 reports per rolling day, 200 pending reports
per reporter and a 60-second cooldown for the same target. Later incidents are
separate records. Blocking a player does not prevent a report about them.

The operator reviews pending incidents in the existing private Cloudflare D1
Console/Studio. Example read-only query:

```sql
SELECT id,reason,details,context,evidence,target_name,target_code,
       target_revision,target_avatar_version,length(target_pixels) AS photo_bytes,
       created_at,status
FROM report_incidents WHERE status='pending' ORDER BY created_at LIMIT 50;
```

After reviewing evidence, set `status` to `reviewed` or `dismissed`, with
`review_note` and `reviewed_at`. Review does not itself ban, rename, hide or delete
a player. Deliberate disruption needs context; poor skill is not a violation.
Report volume alone is not a basis for punishment. There is no scheduled purge
or report expiry. Changing review status does not delete a record or its photo.
Records remain until an explicit operator deletion or cascading profile/account
deletion. Both reporter and target foreign keys cascade: deleting either profile
removes the incident and its attached evidence, including the copied thumbnail.
The legacy reports table retains its existing deletion behavior. This release
does not exempt reports from account deletion or establish a retention period.

`GET /v1/reports` advertises `{version:2}`; POST accepts `version`, `requestId`,
`target`, `reason`, `details`, `context`, and `evidence`. Legacy clients can still
POST target/reason. The new client refuses an older service contract rather
than claiming its explanation was saved.

## Validation and release

Backend: `node --test Tools/AccountServer/*.test.mjs`.
Client/render fixtures: `Tools/Build/Test-Moderation.ps1` against the complete
candidate built with `WhatTheFish.Editor.ModerationBuild.Build`. The fixture
maps simulated roster entries to test profiles; it does not claim that offline
bots are reportable in production. Development helpers are excluded from the
Android submission. Local evidence stays in ignored `Builds/ModerationQA/`.

Physical Android touch, keyboard appearance and device performance require a
connected phone and are not established by Windows render captures.

The existing D1 database received migration 0007 through its authenticated
console, with a recovery bookmark recorded before migration. The table and all
three indexes were verified. Worker version
`8cc459f0-b24a-4c19-bbb2-6f3e6f8ac7bd` passed seven live checks with two synthetic
UGS profiles: contract version, offensive name rejection without mutation,
report receipt and retry identity, changed-payload rejection, self-report
rejection, Other validation, and duplicate-incident rate limiting. Both test
profiles and their associated reports were removed afterward. Evidence is in
`Builds/ModerationQA/cloud-smoke.txt` and `report-schema-live.png`.

Final client checks: **2,734 assertions** (Football 908, Basketball 932, Golf 443,
Fishing 451), plus **1,029 scoreboard regression assertions**, **541 final Malay
basketball assertions**, and **234 real multiplayer chat assertions**. The chat
suite verifies host rejection of raw English/Chinese/Malay profanity, benign
messages, identity, spectator channels, history and host migration. The final
report run is `Builds/ModerationQA/Run-20261026-175538-101/`. Three-language form,
profile, live standings and final-result captures were reviewed at desktop and
phone proportions. The final requested icon is an outlined warning triangle
containing an exclamation mark, drawn by the existing UI mesh system.

Backend: **77 tests passed**. Six report tests also passed from a separate path
containing spaces, launched with an unrelated working directory. Localization:
**3,567 complete entries**, five font faces, **16 artwork checks**. The complete
**342-file Windows player** was promoted to `Builds/WindowsFinal/` with SHA-256
comparison; **531 compiled source documents** match the current source. The
serialized scene bundle is from the full candidate build, followed by a code-only
refresh for icon/layout changes; serialized type layouts and assets were unchanged.

AndroidSubmission succeeded: **168,106,738 -> 168,114,068 bytes (+7,330)**,
with **831,885,932 bytes** remaining below the strict 1 GB limit. SHA-256:
`47915DF3CE129D0653F8900598E2F4593681B18201ED2C2A9EE26A8CBC084116`.
The APK passes v2 signature, ZIP CRC, 1,407 exact audit entries, five-scene bundle
integrity and release-helper exclusion checks. **381 Android compiled source
documents** match. See [build size](BUILD-SIZE.md) and [size audit](APK-SIZE-AUDIT.md).


Superseded outputs are recoverable in these ignored archive batches:
- `Legacy/20261026-120351-118-moderation-first-candidate/`
- `Legacy/20261026-121851-398-moderation-icon-revision/`
- `Legacy/20261026-122155-601-moderation-android/` (prior APK and audit)
- `Legacy/20261026-175308-878-moderation-final-layout/` (superseded candidate,
  interrupted build log and portable test copy)
- `Legacy/20261026-180039-677-moderation-windows/` (prior Windows delivery)

No staging, commit or push was performed. Protocol remains **88**.

## Release and privacy handoff

Implemented report data: server-generated incident ID and creation time;
authenticated reporter ID; target ID; client request ID and payload fingerprint;
reason; optional explanation; sport/internal room context; optional last-five
text-message excerpt from the target; server snapshot of target name, public code,
profile revision and avatar version; thumbnail bytes only for a picture reason;
review status, review note and review time. Explanation is mandatory for Other.
The excerpt is bounded to 2,400 UTF-16 units, context to 160, explanation to 500.
No microphone recording or voice transcript is attached. Picture evidence is the
current stored 128×128 RGB thumbnail, not an original uploaded photograph or a
screenshot of gameplay. There is no automatic content assessment of that image.

Storage is the existing Cloudflare D1 database behind the authenticated Worker.
Player API requests require a verified Unity session; the reporter cannot be
chosen by request data. Public profile APIs do not expose reports or their
evidence. The authenticated operator dashboard can read them; no separate
moderator role system, moderation inbox UI or report-reading player API was
introduced. Review notes/status can be changed using the private D1 console.
No in-game warning, suspension, ban, forced name/avatar removal, appeal flow or
reporter outcome notification is implemented. Blocking remains a separate
player-controlled action. Retention and deletion behavior are described above.

The initial bank covers selected English, Chinese and Malay terms and common
obfuscations. It is not a contextual classifier and can miss slang, new spellings,
other languages and abusive intent, or reject benign uses of listed words.
Modified hosts can bypass chat enforcement. Names on the backend are checked
independently; existing stored names are not retroactively rewritten. Images and
voice have no automated moderation in this release.

Release decisions for the owner, not implemented policy: complete the store's
audience/rating questionnaire; choose report review cadence and escalation,
retention duration and purge process, enforcement and appeal rules; decide how
uploaded photos and voice fit the intended audience; and approve public support
and privacy wording that matches those decisions. Filtering and reporting do
not establish an all-ages rating or store approval. These notes document the
implemented behavior and remaining product decisions; they authorize no further
deployment, hosting or store submission.
