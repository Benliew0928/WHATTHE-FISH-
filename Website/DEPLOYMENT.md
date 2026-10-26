# Website deployment — 26 October 2026

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
