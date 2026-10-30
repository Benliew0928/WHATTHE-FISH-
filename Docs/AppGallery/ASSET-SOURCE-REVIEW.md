# Asset source and notice review — 29 October 2026

This review inventories available evidence for the current release candidate.
It does not mark the store's complete rights declaration as accepted. Current
game assets and APK are unchanged. Documented model credits were subsequently
published on the three-language website as recorded below.

## Verified local evidence

| Item | Evidence / result |
| --- | --- |
| Current signed APK | 172,597,919 bytes; SHA-256 `48F52571E7F92602FBCC03E423B4D8CEB2087F53EDCA4E0E728AB83D29C47146`. Opened read-only; no extraction tree or second package created. |
| Software, icons and audio notices | Four APK entries match their source files byte for byte: SportsUI, FootballAudio, GolfFishingAudio and CodecreteQr. The complete WindowsFinal delivery also has the matching QR licence. |
| Fonts | The current packed-assets report contains seven font-licence TextAssets: GolfCartLabels, Sports, Nunito, Noto, two Barlow copies and CoveFontLicense. Their source licences remain beside retained masters/derived resources. |
| Downloaded sports audio | Basketball source manifest and retained Kenney notices; Football/GolfFishing source lists record CC0 URLs and uploaders. Original procedural effects have authoring scripts. This review did not re-audition every clip or revalidate every remote uploader page. |
| Meshy rods / golf equipment | [Rods](../../ArtSource/Fishing/Rods/README.md) and [equipment](../../ArtSource/Golf/Equipment/README.md) record the creator account, displayed CC BY 4.0 licence and derivative changes. Individual job IDs were not retained. Readable attribution for the documented rods/clubs is published in all three website languages. |
| User-provided golf cart | [Source record](../../ArtSource/Golf/Cart/provenance.json) preserves the original ZIP hash and generator. On 29 October the owner confirmed that a teammate generated it. That identifies the reported creator, but the generation plan/licence and permission for public game distribution remain unconfirmed. |
| Golf GO / walkway audio | [Recording record](../../ArtSource/Audio/Golf/Recordings/provenance.json) preserves source/master hashes and historical integration approval. On 29 October the owner explicitly chose to keep the current effects. All four active derivatives are retained; replacement work is cancelled. The original-source evidence remains as recorded. |
| Generated visuals / narration | Existing authoring README/prompt/voice manifests retain provenance. They are not a blanket warranty for every reference image or user-supplied input. No new generation or provider purchase was performed. |

Compact verification: `Builds/AppGallerySubmissionQA/asset-notice-review.json`.
The APK entry-name search found no separately named model-credit/attribution
file. That search does not prove absence from opaque binary content; no complete
copyright clearance is inferred from filenames or a successful build.

## Confirmed attribution requirement and prepared wording

Meshy's current [official licensing explanation](https://help.meshy.ai/en/articles/9992001-can-i-use-meshy-assets-commercially-license-copyright-explained)
allows free-plan outputs under CC BY 4.0 with Meshy credit; paid-plan rights
depend on its stated terms and permitted inputs. [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)
calls for appropriate credit, a licence link and an indication of changes.
The existing source records, rather than an assumed current subscription, should
determine each asset's notice.

The website now publishes the following attribution, narrowed to the documented
rods and golf clubs and naming creator account `benliew28262826`. The original
prepared wording below records the source scope:

- English: Selected fishing-rod and golf-equipment models were created with
  [Meshy](https://www.meshy.ai/) under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).
  Geometry, UVs, textures, scale and pivots were adapted for WHATTHE FISH?.
- 简体中文：部分钓竿和高尔夫装备模型使用 [Meshy](https://www.meshy.ai/) 制作，依据
  [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) 授权。为适配 WHATTHE FISH?，
  已调整几何结构、UV、纹理、尺寸和轴心。
- Bahasa Melayu: Sebahagian model joran dan peralatan golf dihasilkan dengan
  [Meshy](https://www.meshy.ai/) di bawah [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).
  Geometri, UV, tekstur, skala dan titik pangsi diubah suai untuk WHATTHE FISH?.

The published website-only catalog includes all three translations, a Meshy
link, the CC BY 4.0 link and the derivative changes. All twelve live pages were
checked, with desktop/mobile layouts reviewed in each language. The offline
game catalog, documents and APK were not changed. See the
[website deployment record](../../Website/DEPLOYMENT.md#credits-update--29-october-2026).
This notice does not apply to the golf cart, current replacement ball, entire
game or unidentified assets.

## Owner decision and release preparation

On 29 October the owner instructed keeping the current assets and proceeding
with the work. Keep the cart and all four sound clips, cancel replacement work
and stop repeating the asset-choice questions. Retain existing provenance
accurately; the keep decision is not new evidence of a particular source licence.
The documented model credits are published; complete the remaining release checks.
The three-language listing screenshots are actual gameplay captures; their
source APK and original PNG hashes remain in the screenshot manifest.

## Recorded-audio trace

- [GolfStartAudio](../../Game/Assets/_Game/Sports/Golf/GolfStartAudio.cs) loads
  `GolfAudio/go` and plays it when the opening countdown ends and the match
  becomes running. The retained cue lasts 0.93 seconds.
- [GolfFootsteps](../../Game/Assets/_Game/Sports/Golf/GolfFootsteps.cs) loads the
  three `GolfAudio/Walkway` clips and cycles them for moving foot contacts on
  path/approach/paving surfaces. Their durations are 0.35, 0.32 and 0.37 seconds.
- The current packing report lists all four WAVs; their SHA-256 values still
  match the retained delivery manifest. This is source/packing evidence, not
  a new physical-device audio test.
- Two task-created audition files were offered and declined. They are disposable
  generated output; the reviewed cleanup record is
  `Builds/AppGallerySubmissionQA/cleanup-declined-audio-preview.json`. Keep all
  current game clips, approved authoring masters and original delivery manifests.
