# Website deployment

## Credits update — 29 October 2026

The existing Cloudflare Pages project now publishes documented fishing-rod and
golf-club attribution in English, Simplified Chinese and Bahasa Melayu on every
page. It names the creator account and Meshy, links CC BY 4.0 and describes the
derivative changes. The scope excludes the cart and current replacement ball.
The support page's pending-work summary now reflects ongoing device/privacy
acceptance. A follow-up correction records the adopted retention periods,
enabled daily cleanup and outstanding live verification in all three public
privacy drafts. Terms and privacy remain working drafts.

- Production deployment: `50ff813f-aa01-47c9-a88e-a4c1bcbea0ed`.
- Public URL: <https://whatthefish-game.pages.dev/>.
- Deployment URL: <https://50ff813f.whatthefish-game.pages.dev/>.
- Package: `Builds/WebsitePublish/whatthefish-credits-20261029.zip`, 20 files,
  **5,406,337 bytes**, SHA-256
  `8591cadeb1eaa2172fb674fb9a4f5f7cd8921549fdfeba450127eae949ba5519`.
- Cloudflare confirmed **20/20 uploaded** and **Success**. All twelve public
  pages subsequently showed the new localized credits, with no horizontal
  overflow, broken loaded images, forms or scripts at the checked desktop size.
- Three-language credits were visually reviewed at desktop and 390px phone
  width. All twelve local pages passed the phone-width checks.
- The changed retention section was additionally reviewed in all three
  languages at both sizes and verified on all three live privacy pages.
- Both generator modes pass their applicable checks. This website-only update
  writes no game resources; game catalog/documents/links modification times
  predate this update. The existing APK and Windows player remain unchanged.
- Evidence: `Builds/WebsitePublish/credits-*`. The prior deployment and original
  ZIP below remain the rollback; no duplicate project snapshot was created.

The first credits-only publication in this update was
`6dfbfc0e-6b1b-4b71-8a12-3913b678d556`; the current deployment above also corrects
the outdated retention draft. Its reproducible intermediate ZIP was reused for
the final package instead of retaining another upload copy.

## Original publication — 26 October 2026

The owner explicitly authorized publishing the website through their signed-in
Cloudflare browser. Cloudflare Pages confirmed success and the public site was
opened and checked in the browser. This is a public development preview of
WHATTHE FISH?, not a game release or final policy acceptance.

| Item | Value |
| --- | --- |
| Public site | <https://whatthefish-game.pages.dev/> |
| Project | `whatthefish-game` |
| Hosting | Cloudflare Pages, dashboard Direct Upload |
| Deployment | `18cb6168-73c6-488d-b4da-8d9bf794d532` |
| Deployment URL | <https://18cb6168.whatthefish-game.pages.dev/> |
| Cloudflare environment | Production; the content remains a development preview |
| Languages | English, Simplified Chinese, Bahasa Melayu |
| Uploaded package | 20 files; ZIP 5,403,335 bytes |
| ZIP SHA-256 | `b601b3fa9ffeb90449b8041ad81119c33c7bea154768d88bc4de72b6443635a0` |

Each language has Home, Terms, Privacy and Support/account-deletion pages.
The site identifies LIEW KAIY BIN and the owner-supplied public email. It has no
forms, sign-in, analytics scripts, advertising trackers or app download. The
website privacy section discloses Cloudflare network-data processing and links
to [Cloudflare's privacy policy](https://www.cloudflare.com/privacypolicy/).
Game policies retain their draft notice and unresolved release checks.

All pages request `noindex,nofollow`; `robots.txt` disallows crawling. These are
search-engine requests, not access controls: the site is publicly accessible.
The package includes CSP, referrer, MIME-sniffing and permissions headers in
`_headers`. No custom domain, paid upgrade or new analytics configuration was
requested or added.

## Validation and evidence

- Generator checks passed for both the original bundled documents and the
  website-only public-preview mode; all public copy has three translations.
- All 12 local and live pages checked at 1440 and 390 pixels wide. Live checks
  waited for full page load: no horizontal overflow, broken image elements,
  script elements or forms. Language tags and translated hosting links passed.
- Visually reviewed all three homepages on desktop and mobile. Screenshots,
  browser results, per-file hashes and ZIP are under ignored
  `Builds/WebsitePublish/`; `live-home-en.png` shows the public homepage.
- The game's localization catalog, offline documents and URL configuration
  were hash-compared with the pre-publication baseline and remained unchanged.
  No new APK or Windows player was necessary for this website-only change.

The website publication originally left the game Website button disabled; the
subsequent login UI update configures the published base URL and localized links.
The game configuration lives in `Resources/AppGallery/Links.json`.
Publishing this site did not submit the game, accept an official age rating,
resolve child/guardian requirements, implement retention cleanup, complete
deletion coverage, activate backend changes or create a production signing key.

For future updates, follow the [generation/upload workflow](README.md) and
[Cloudflare Direct Upload documentation](https://developers.cloudflare.com/pages/get-started/direct-upload/).
Keep upload packages restricted to the generated website directory. Never
upload the repository, credentials, game binaries or local audit evidence.
