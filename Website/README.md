# WHATTHE FISH? website

Public development preview: <https://whatthefish-game.pages.dev/>. The owner
authorized website publication on 26 October 2026. The game remains unreleased;
its Terms and Privacy pages are explicitly working drafts. See the
[deployment record](DEPLOYMENT.md) for the uploaded package and verification.

The sources are `content.json`, `public-preview.json` and `site.css`. No service
is registered, deployed or contacted by the generator. English, Simplified Chinese and Bahasa Melayu
each have a homepage, Terms, Privacy and Support/account-deletion page.

From any working directory, invoke the repository's
`Tools/Build/Prepare-PublicInfo.py` with Python 3.10 or later. Paths resolve from the script.
The output is ignored under `Builds/PublicInfoQA/Site/`. Run the same script
with `--check` to detect stale pages, assets, bundled text or catalog entries.
`Tools/Build/Preview-PublicInfo.ps1` generates and serves it on loopback only.

For the hosted preview, run `Tools/Build/Prepare-PublicInfo.py --public-preview`,
then repeat with `--public-preview --check`. This mode applies the shared
three-language website-only catalog in `public-preview.json` and outputs
`Builds/WebsitePublish/Site/`. It never writes game resources or Unity metadata.
It includes Cloudflare hosting disclosures, security headers and a robots file.
Upload the contents of that directory (or a ZIP with `index.html` at its root)
through Cloudflare Pages, project `whatthefish-game`, only when publication is
authorized. Direct Upload does not connect or push the repository.

The generator reuses the approved logo, islands and sport-card artwork without
modifying their pixels and copies the existing Barlow font with its OFL licence.
It synchronizes new English/Chinese/Malay keys into the shared game catalog and
the offline document bundle in `Game/Assets/_Game/Resources/AppGallery/`.
Existing catalog translations remain authoritative for shared English keys.
Keep the generated document bundle and its Unity metadata in Git.

The website has no forms, analytics, external assets or authentication. Email
links open the visitor's email application and do not send anything automatically.
All pages are marked as drafts and carry `noindex,nofollow`. They have no fake
download button, effective date, invented public URL or promise of store approval.

## Game integration

The welcome Info panel contains Terms, Privacy policy and Website. The game now
configures `https://whatthefish-game.pages.dev/` as its `baseUrl`, opening all
three actions in the browser in the selected language. The Settings/account
privacy action uses the same localized policy destination. The published
documents remain development drafts; linking them does not finalize the policy.

Configure the HTTPS base directory in
`Game/Assets/_Game/Resources/AppGallery/Links.json` (`baseUrl`).
The game resolves `index.html`, `terms.html` and `privacy.html` in the selected
language (`zh/` and `ms/`, English at the root). Credentials, query strings,
fragments and non-HTTPS base URLs are rejected. An absent/invalid configuration
never opens a placeholder URL. The older standalone `PrivacyPolicyUrl.txt`
configuration remains supported when no valid base directory is supplied.

Before treating the game policy as final or using it for store submission, settle the gaps in
[the privacy preparation](../Docs/AppGallery/PRIVACY-DRAFT.md), including child
privacy, provider settings, retention operations and complete deletion coverage.
Replace draft notices and effective-date wording only after those facts are
confirmed; update the game bundle and test all three languages again. Hosting
access logs need disclosure according to the chosen provider's actual settings.
