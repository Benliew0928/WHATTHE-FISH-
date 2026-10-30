# First-visit home introduction

After a player completes login, the home menu shows a 15-step introduction once per device. A 2D character speaks while the current destination is highlighted. Tapping the screen advances; Skip or completing the last step stores the seen flag. An interrupted guide is offered again at the next login. The flag is local to the device, rather than synchronized between accounts or devices.

The guide covers hosting and joining, all four sports, shared island travel, daily rewards, coins, friends, profile, settings, venue customization and tutorials. It uses the existing player portrait and localizes all authored dialogue and controls in English, Simplified Chinese and Bahasa Melayu.

The [adaptive menu update](RESPONSIVE-ANDROID-LAYOUT.md) makes highlights follow
the actual control rectangles after reflow or a live window resize. The dialogue
and Skip placement adapt with the menu. Its original release record follows;
current delivery measurements are listed first in the build-size documentation.

Validation: `Tools/Build/Test-FirstVisit.ps1` exercised the production guide directly in a full Windows player with a compatible scripts-only update. All 2,380 checks passed across 1600×900, 1560×720, 1200×900 and 1920×1080, including pointer routing, skip, completion persistence, text clipping and overlap with the highlighted destination. Representative rendered views were reviewed in all three languages. An earlier wide-screen check exposed Skip touching the settings highlight; the final spacing change passed all sizes. Catalog/font and artwork checks passed. Provider sign-in, actual guest-login interaction and physical-phone appearance, touch and performance were not tested by this review.

The final AndroidSubmission APK was measured at 168,199,714 bytes, 13,444 bytes above the pre-task APK, leaving 831,800,286 bytes below the limit. Its ZIP CRC, 1,410 entries and v2 signature passed. See [build size](BUILD-SIZE.md) and [APK audit](APK-SIZE-AUDIT.md). The complete 342-file Windows player was promoted with matching hashes. Evidence and promotion inventory are under ignored `Builds/FirstVisitQA/`; current Android audit is under ignored `Builds/SizeAudit/latest/`.

The previous APK is in `Legacy/20261027-144532-407-first-visit-android/`; the previous Windows player is in `Legacy/20261027-150052-432-first-visit-windows/`. The build regenerated the golf ball prefab; its pre-task source bytes were restored after the build. No files were staged, committed or pushed.

Localized narration and tap sound were added in [the voice follow-up](FIRST-VISIT-AUDIO.md).
