# Bilingual UI font provenance

The original Latin fonts remain the authoritative source for English/Malay
outlines and metrics. Noto Sans SC supplies only missing codepoints.

Upstream: [Google Fonts Noto Sans SC](https://github.com/google/fonts/tree/main/ofl/notosanssc).
Downloaded 25 October 2026. License: [SIL OFL 1.1](OFL.txt).
The complete variable master is retained in this folder; it has no external file
dependencies. Deliverable TTFs are self-contained Unicode fonts, not references
to an installed system font.

Generator: [Generate-LocalizationFonts.py](../../Tools/Build/Generate-LocalizationFonts.py).
Latin inputs: `Game/Assets/_Game/Resources/` (repository-relative base).
Nunito menu faces, Manrope sports faces, and Barlow Semi Condensed heading are
preserved. Derivative family names are `Island Bilingual <role>`.
Noto weights: MenuText/SportText 600, MenuDisplay 900, SportDisplay/MenuHeading 800.
Heading CJK outlines use a 0.12 shear to match the italic menu heading.
Copies of all four OFL notices accompany the delivery fonts under
`Game/Assets/_Game/Resources/Localization/Fonts/`.

SHA-256 (paths relative to repository root):

- `ArtSource/LocalizationFonts/NotoSansSC-Variable.ttf`: `a3041811a78c361b1de50f953c805e0244951c21c5bd412f7232ef0d899af0da`
- `Game/Assets/_Game/Resources/Localization/Fonts/MenuDisplay.ttf`: `698e4a605397575dbad1bfd71feec5d3f519cd594dc1473d4c134d1d16b6330c`
- `Game/Assets/_Game/Resources/Localization/Fonts/MenuHeading.ttf`: `19da283c8bb1958f225111308f910e2634f7574b2aeffb5b841d72c09aa486ce`
- `Game/Assets/_Game/Resources/Localization/Fonts/MenuText.ttf`: `7209e9a231d7873b93f69e6ed89bce2cbe713d81f49cfbc201c64a57435adb30`
- `Game/Assets/_Game/Resources/Localization/Fonts/SportDisplay.ttf`: `0c10e905474226be0a733ce57644ab490e0fa16ac305ce3129bf9994d14f0cb3`
- `Game/Assets/_Game/Resources/Localization/Fonts/SportText.ttf`: `5e6cf32e518c81362929fd5db7a0c8ae6dc3304dff2a3773b708c07344a469eb`
