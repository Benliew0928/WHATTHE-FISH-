# English, Simplified Chinese and Malay

The welcome screen's top-right language control offers **English**, **简体中文**
and **Bahasa Melayu**. Selection applies immediately and is saved on the
device under `ui.language`. The same choices are available in main-menu Settings and the in-game Settings
of all four sports. Welcome Settings deliberately has no duplicate language
entry: the top-right welcome language control already supplies it. Back closes the
language picker without leaving Settings or changing match/input ownership.
A fresh installation defaults to English. Language
is a presentation preference: it does not change room protocol, simulation,
shot power, account identity or other players' language.

The offline source of truth is
[`Catalog.json`](../Game/Assets/_Game/Resources/Localization/Catalog.json).
Runtime factories create `LocalizedText`, which retains the original English
caption separately from the rendered text. Serialized world labels have a
matching binding. Bitmap lettering uses the original graphic plus a shader that
samples a clean companion only inside reviewed lettering regions. Transparent
text uses the original palette/outline; no replacement plate, gray rectangle or
redrawn icon covers the design. English restores the original material. See
[artwork provenance and exact prompt sets](../ArtSource/LocalizationArtwork/README.md).
Icons without lettering remain icons. Brand names remain unchanged.

Names, chat messages, room codes and input fields are user content, not catalog
keys. Dynamic templates preserve their arguments unless an argument is explicitly
listed in `terms` as a translatable game term. Any gameplay/UI logic that reads
a caption must use `GameLocalization.Source(label)` rather than rendered text.

## Typography

English and Malay retain the original fonts. Five bilingual derivatives preserve
all original Latin outlines, advance widths and line metrics. Missing Chinese
glyphs come from Noto Sans SC at matching weights; the heading derivative also
matches the menu's slant. Existing text colors, shadows, alignment and font styles
are retained. Translated labels use bounded best-fit sizing where needed,
including complete multiline event names. Text rasterization accounts for the
actual canvas and panel scale with 2x edge sampling. The mesh is cleared after
font-atlas generation to prevent unrelated glyphs leaking into other labels.

The original Latin fonts do not contain Chinese. Identical Chinese glyph shapes
therefore cannot be promised; these are deliberately matched companion glyphs.
The editable upstream master, licenses and generator are retained. See
[font provenance](../ArtSource/LocalizationFonts/README.md).

## Maintenance and verification

- Add both translations whenever introducing a caption. Preserve `{0}` format
  arguments and rich-text tags. Prefer explicit templates for mixed user content.
- Mark player-authored text with `GameLocalization.UserContent(label)`.
- For a new bitmap containing words, add a reviewed clean companion and transparent lettering; don't
  translate filenames, enum/protocol values or resource paths.
- Install `fonttools` and `brotli` in the developer's Python environment, then run
  `python Tools/Build/Generate-LocalizationFonts.py` to regenerate fonts.
- Run `python Tools/Build/Check-Localization.py` for catalog completeness,
  placeholder parity, Chinese coverage, original glyph outlines and metrics.
- Build the Windows candidate with Unity method `LocalizationBuild.Polish`.
  Run its player with `-localizationReview <output-directory>` and isolated
  `-localizationProfile <id> -guestProfile <id>` arguments. It visits the welcome
  picker, menus and four sports, captures translated UI, starts Golf, inspects
  rankings/events, and verifies round-trip/source behavior. Outputs belong under
  ignored `Builds/LocalizationPolishQA/`. The review also switches languages through
  every Settings screen, preserves audio/input state, checks original UI properties
  and captures all 18 artwork states in three languages.
- Run `python Tools/Build/Check-LocalizationArtwork.py` to verify original artwork
  hashes, clean masters/delivery copies and UV alignment. It never edits pixels.
- A second run with the same profile and `-localizationExpected zh-Hans` verifies
  persistence across processes. `-language en|zh-Hans|ms` is an optional launch
  override; omit it for the persistence check.

Windows captures cannot certify phone touch, rendering or frame rate. Delivery
size is measured from the new APK, separately from source font sizes.

## Settings and artwork verification — 25 October 2026

The final candidate passed **2,168 checks at 1600×900**, **2,166 at 1200×900**
and **one separate-process persistence check**. Tests switch through all three
languages inside welcome, main-menu and four sport Settings, retain audio and
input ownership, restore English text/font/size and check every visible bitmap
caption for truncation. All 3,445 catalog entries and 900 Chinese glyphs passed;
original Latin outlines/metrics and all 16 original artwork hashes are unchanged.
The 503 compiled Windows source documents matched the workspace. The startup
suite passed another 1,684 assertions at four sizes through 1920×1080. All
335 files of the promoted complete Windows player matched the tested candidate.

Screens from both aspect ratios were reviewed, including all 18 localized bitmap
states in English, Chinese and Malay, Settings, the picker, welcome, HUD,
rankings and event banners. Evidence stays in `Builds/LocalizationPolishQA/`.
The initial iterations exposed background seams and clipped second lines; these
were fixed before the final runs. Superseded captures are retained as evidence,
not presented as final validation.

Final Android size and complete Windows delivery are recorded in
[the measured release record](BUILD-SIZE.md) and
`Builds/WindowsFinal/LATEST-BUILD.txt`. Physical Android rendering, touch and FPS
remain unverified. No Git staging, commit or push was performed.

The preceding complete player is recoverable in
`Legacy/20261025-163042-576-localization-polish-windows/`; the promoted duplicate
candidate in `Legacy/20261025-163102-340-localization-polish-candidate/`.
The earlier APK/audit is in `Legacy/20261025-162352-398-localization-polish-android/`.
The validator also passed in a copied root containing spaces, launched from an
unrelated working directory; five documentation links resolved. That temporary
copy is archived in `Legacy/20261025-163156-972-localization-polish-portability/`.

## Layout regression coverage

[`AGENTS.md`](../AGENTS.md) requires every task to supply all three translations for new or
changed player-visible wording, including lettering embedded in artwork.
Player-authored names/chat and protocol identifiers remain unchanged.

The layout review captures selected guest-name text, every page of all four
sport tutorials, settings and gameplay in all three languages. It audits actual
rendered glyph bounds, clipped lines and sibling-label overlap, rather than
assuming a translation with complete glyphs automatically fits. Golf target
label spacing is measured from translated glyph edges, preserving the original
12-unit gaps and safe card padding. Fishing status/bonus labels fit their own
slots from their translated text. Editable InputField text retains Unity's
native geometry contract so selections and carets align with the visible text.
Tutorial overlays use a dedicated modal sorting canvas above the scoreboard
and final countdown; embedded menu tutorials retain their parent ordering.

Welcome Settings has no duplicate language entry; main-menu and all sport
Settings retain theirs. The language's native name is always `Bahasa Melayu`.
Selected camera cards retain their border and checkmark without the yellow
underline. The five Golf tutorial event illustrations use the same localized
lettering and original palettes as their gameplay banners.

The regression scenes also exercise Fishing round, cup and crew result sheets.
Long Malay status labels wrap inside their existing cells, and the cup catch
heading reserves its actual wrapped height. Event seconds fit both axes,
including their outline, with clearance below the upcoming-event title.


## Layout correction delivery — 25 October 2026

Final runs in `Builds/LocalizationLayoutQA/Final960`, `Final1600` and
`FinalTablet` passed 3,314 assertions each, with no recorded glyph overflow,
truncation or sibling-label overlap findings. `Persistence` passed the restart
assertion: **9,943 total**. All four sports, their 16 tutorial pages, settings,
Golf event artwork and Fishing round/cup/crew result sheets were exercised in
English, Simplified Chinese and Bahasa Melayu. Targeted screenshots were also
visually reviewed. `WelcomeFinal` passed **1,652** startup assertions at
1600×900, 1560×720, 1200×900 and 1920×1080.

All 3,445 catalog entries/format tokens, 900 Chinese glyphs and 16 original-art
hashes passed. All 335 delivered Windows files match the tested candidate, and
504 Windows / 361 Android source documents match their compiled PDB records.
The complete current player is `Builds/WindowsFinal/`; APK bytes, hashes and
recoverable archive batches are in [the release record](BUILD-SIZE.md).
Intermediate `First960` and `Second960` captures record issues fixed before the
final runs and are retained as evidence, not final validation. Phone rendering,
touch and FPS remain unverified. No Git staging, commit or push was performed.

## Results, headings and tee signs — 25 October 2026

Translated Golf leaderboard headings share one fitted font size: the largest
common size that fits every column, including outline padding. Caption regions
are laid out before measuring when the ranking expands or collapses. Original
English bitmap artwork remains unchanged. All five tee signs explicitly retain
two lines: hole/par together, then the translated course name; each line is
measured to fit the original sign width.

Fishing results use a modal sorting canvas and raycaster above the entire HUD,
including Settings. The shade dims and intercepts the gear until results close.
Golf's awards Show results button opens the expanded leaderboard; the next
adventure flow waits until ranking closes.

The current runtime checks additionally assert equal translated header sizes,
two complete lines on all five signs, blocked/restored Settings pointer hits,
and the actual Golf awards button callback. Evidence is in
`Builds/EventPresentationQA/`. Event camera and HUD fading rules are documented
in [activation presentation](PROP-ACTIVATION-FOCUS.md).


Room communication integration adds the new connect/retry/leave, microphone state, volume and spectator actions to the shared catalog. Roster names stay user content while the self suffix follows the selected language. Three-language caption bounds and literal names are included in `RoomVoiceReview.LocalizedPanels`; the current catalog has 3,456 entries.
