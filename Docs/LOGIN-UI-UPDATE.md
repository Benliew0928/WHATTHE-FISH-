# Login UI and sign-out — 26 October 2026

The welcome audio sliders now fill from the inner left border of the recessed
rail. The teal fill covers the rail's interior height and follows the selected
volume; the thumb keeps its existing size and travel padding.

`Resources/AppGallery/Links.json` now points at the published development site,
`https://whatthefish-game.pages.dev/`. Welcome Info opens Terms, Privacy and
Website in the current language. Settings/account Privacy uses the same policy
destination. All three policy pages returned HTTP 200 during verification.
The website documents remain working drafts.

From the home menu, choose Settings → Sign out, then confirm. The game returns
to the ready login screen and blocks underlying gameplay. Cancel keeps the
current session open. The existing account-panel sign-out also returns to login.
Leave an online room first; existing room, account-operation and pending-deletion
guards remain in force. Sign-out does not delete a game account. Guest sign-out
retains the device guest identity and recoverable anonymous session; linked
sign-out clears its authentication token and active account selection as before.
Guest entry works again after returning to login. Offline launch shortcuts do
not bypass an explicit sign-out.

New confirmation copy uses the shared English, Simplified Chinese and Bahasa
Melayu catalog. No artwork or phone-quality settings changed.

Validation evidence and current build logs are retained under
`Builds/LoginUIQA/`. The final run passes **2,772 assertions** across four screen
sizes and all three languages, including actual pointer clicks, cancellation,
logout/re-entry and text bounds. Visually reviewed welcome sliders and the new
Settings/confirmation layouts. All **533 Windows / 383 Android** compiled source
documents match; **3,661** catalog entries, five fonts and **16** artwork checks
pass. The complete **342-file** player was promoted to `Builds/WindowsFinal/`
with every copied file hash verified. Repository readiness with `-RequireApk`
passes; nothing was staged, committed or pushed.

Archived the previous complete player under
`Legacy/20261026-211517-921-login-ui-windows/` and the previous APK/audit under
`Legacy/20261026-205233-115-login-ui-android/`. A format-aware comparison confirmed
all 14 golf-ball prefab objects/components and external references unchanged
after build generation; restored its original source representation.

Build-size and player-delivery results are recorded in
[BUILD-SIZE.md](BUILD-SIZE.md) and `Builds/WindowsFinal/LATEST-BUILD.txt`.
Physical Huawei sign-in, browser handoff and phone performance require device
testing; Windows checks cannot establish those results.
