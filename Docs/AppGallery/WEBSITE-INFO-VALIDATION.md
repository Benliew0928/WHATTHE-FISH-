# Website and Info panel preparation — 26 October 2026

This task prepares local artifacts only. No site was published, no app was
submitted, no production service was changed and no Git publishing action was
performed. The previously completed reporting/text-filtering work remains in
the shared source and is included in the new player builds.

## Changes

- A static Home, Terms, Privacy and Support/account-deletion site, in English,
  Simplified Chinese and Bahasa Melayu. All 12 pages are marked as drafts.
- Welcome Info replaces the previous sport introductions with Terms, Privacy
  and Website. Terms/Privacy open complete scrollable offline drafts. Website
  remains disabled until a real HTTPS base directory is configured.
- The policy URL resolver is shared with Settings/account Privacy and preserves
  the previous standalone URL configuration. Credentials, query/fragment base
  URLs and non-HTTPS addresses are rejected.
- Publisher/contact details, daily owner review, same features for everyone,
  and no deadline are recorded. Proposed retention is documented separately
  from currently implemented behavior.

## Website and source checks

- Generator check: 12 pages, 100 source strings in all three languages, three
  bundled document drafts, valid relative links and exact approved asset copies.
- Localization: 3,660 complete catalog entries; 1,003 Chinese characters covered
  across all five fonts, original Latin glyph metrics retained; 16 artwork checks.
- Browser: all 12 pages at 1,440- and 390-pixel widths; loaded images, no horizontal
  text/image overflow, readable navigation and document layout. Confirmed policy
  navigation, final-section anchor, back-to-top and language switching while
  retaining the selected document. These are browser viewport checks, not a
  claim about an actual Huawei phone.
- Generator verified from a copied checkout path containing spaces and an
  unrelated working directory. The disposable fixture was archived to
  `Legacy/20261026-183953-533-public-info-portable/`; the pass record remains in
  `Builds/PublicInfoQA/portable-check.txt`.

Current raw build logs, browser captures, executable checks and source hashes
are kept under ignored `Builds/PublicInfoQA/`. Current APK measurements are in
[build size](../BUILD-SIZE.md) and [the size audit](../APK-SIZE-AUDIT.md).

## Game validation and Windows delivery

The welcome-only review passed **4,634 assertions**: 1,166 at 1600x900,
1,156 at 1560x720, 1,159 at 1200x900 and 1,153 at 1920x1080. Each includes
English, Simplified Chinese and Bahasa Melayu, real pointer hit tests, loading
and error states, document scrolling, top/end text bounds, Back/Done/close,
disabled unpublished Website, URL configuration validation and unchanged room,
identity and gameplay gating. No real account was deleted or signed into.

The first review found a difference between the custom text renderer's wrapping
density and Unity's ordinary height estimate. Document paragraphs now measure
at the rendering density and align at the top, removing translated paragraph
overlap. Tests separately account for intentional offscreen paragraph culling
and verify visible glyph bounds. Superseded review evidence is preserved in
`Legacy/20261026-184834-012-public-info-initial-review/`.

The complete Windows player was built, then refreshed for code-only layout and
review corrections with unchanged serialized scene content. All **533 compiled
source documents** match current files. The **342-file** player in
`Builds/WindowsFinal/` matches the validated candidate by SHA-256. Seven existing
trailer launchers were preserved; trailer workflows were not rerun by this task.
The superseded complete player is in
`Legacy/20261026-185147-630-public-info-windows/`.

## Android delivery

The fresh AndroidSubmission APK is **168,146,796 bytes**, up 32,728 bytes from
the task's 168,114,068-byte baseline, with 831,853,204 bytes of headroom under
the project's strict limit. SHA-256:
`96EBADD627A8EA6CB02EFCF452B09D8AB954D6CA674B3F52ACA7BB3185891AFD`.
All **383 Android compiled source documents** match; v2 signature, package,
non-debuggable manifest, ZIP CRC, 1,409 audit entries, bundled scene hash/length,
required runtime types and exclusion of development review types pass.
The certificate is **Android Debug**, so this remains a test-signed preparation
package. The previous APK/audit are preserved in
`Legacy/20261026-184952-869-public-info-android/`.

The build regenerated golf-ball prefab local IDs/order. Format-aware graph
comparison verified all 14 objects/components and external references identical;
the original source representation was retained. No art quality reduction or
phone performance claim was made.

## Remaining release work

The [operations proposal](OPERATIONS-PROPOSAL.md) identifies the cross-host match
JSON deletion gap, unimplemented retention cleanup, child/guardian review and
provider-setting verification. The drafts make none of these into completed
promises. Public HTTPS hosting, production signing, physical Huawei testing,
final OAuth configuration, final store materials and content rating remain
separate release steps. No additional personal details are needed to finish
this local preparation pass.
