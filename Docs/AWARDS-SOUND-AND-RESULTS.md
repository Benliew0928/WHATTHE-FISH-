# Island awards sound and local result visibility



Fishing and Golf retain their existing ceremony timing and result designs.

`IslandAwardsAudio` presents local sound only. It caches the render-only cast's

actual foot bones and receives each actor's contact transitions directly from the phase used to pose `FishingCeremony.Podium.WalkingFoot`. Offscreen feet, the

opening shot, podium pose, large time jumps and cancelled ceremonies emit no

steps. Fishing uses attack-aligned timber steps; Golf uses attack-aligned grass steps.

The original grass recordings contain 90-105 ms of pre-contact sound; wood has

20-30 ms. Awards-only derivatives place the first strong attack within 10 ms.

Distant cast members are quieter so multiple walkers do not mask the foreground.



Fishing's 1.7–3.8 second podium rise has a quiet, faded water layer. On settling,

the existing short Cup fanfare and a recorded group cheer play once. Muted or

unfocused transitions are consumed without replaying on return. Opening Golf

results fades the celebration mix to 25%; closing restores it without restarting.

Fishing's transition retains any remaining tail quietly under its results.

Cancellation, focus loss, pause and scene shutdown stop the voices. Existing

master/SFX settings apply. The ordinary delayed result cue is suppressed while

the ceremony owns the result presentation, avoiding a second early fanfare.



The cheer recording, editable generator and provenance are in

[IslandAwards audio](../ArtSource/Audio/IslandAwards/README.md). Existing surface

step masters, waves and fanfare remain shared with the approved Golf/Fishing bank.



The Golf full ranking performs one local-row reveal after opening and laying

out the scroll viewport. Results remain in their authoritative order; subsequent

network refreshes do not pull a manually scrolled list back. Reopening reveals

the local row again. Missing local records/spectators retain the top fallback.

A gold edge marker and stronger existing blue-row rim distinguish the local

player. Fishing retains all five visible rows with its blue row and gold marker.



`IslandResultName` reserves the already localized local-player suffix, fits at a

readable native font size, then truncates only the player name at Unicode text

element boundaries. The original names and scores remain in match state; no

ranking or report identity changes. Existing English, Simplified Chinese and

Malay translations are reused; no new player-visible wording is introduced.



## Reference-style awards crowd celebration - 28 October 2026

Fishing and Golf now share a 4.65-second celebration derived from the user-supplied
reference recording. The delivery removes empty lead/tail, balances gain, filters
lightly and fades gently. The original embedded-audio MP4 is retained outside
Unity as an editable master; the obsolete standalone OGG cheer is archived.
The one-shot standing trigger, surface footsteps and 25% results-volume mix remain.
No new player-visible wording was introduced.

AndroidSubmission: **168,495,603 -> 168,656,095 bytes (+160,492)**;
remaining headroom **831,343,905 bytes** below 1,000,000,000.
SHA-256: **841920880442FDFD6D97FE832BDF7FB107010BB41DCD3EC4DC30CF0753E0D654**.
V2 signature, ZIP CRC, 1,428 exact audit entries and five-scene bundle integrity pass.
The crowd audio resource grows by 167,744 bytes; other entry deltas were reviewed.
Largest compressed entries are islands 45,941,031; IL2CPP 13,329,398;
Vivox 10,240,163; Unity 9,828,173 bytes. No art quality was reduced.

**141 runtime assertions** pass across both ceremonies in English, Chinese and Malay.
All 3,664 localization entries pass and six current result captures were reviewed.
The new PCM is confirmed inside both delivered Windows and Android resources;
its duration, gain, fades and portable regeneration pass. All 546 Windows / 385
Android compiled source hashes match. The complete 334-file Windows candidate
was copied and hash-verified; retained awards launchers bring WindowsFinal to 336
files. The one-file reduction is only a Burst DoNotShip debug text file.
Evidence: `Builds/AwardsCrowdQA/`. Physical speaker balance and phone FPS/touch
remain unverified. No Git publishing was performed.

## Awards footstep alignment and cheers - verified 28 October 2026

Awards now receive contact transitions from the phase used to pose each actor.
Awards-only grass/wood derivatives remove 90-105 ms / 20-30 ms of pre-contact
sound; all eight strong attacks begin within 10 ms. Distant walkers are quieter.
The synthetic applause is replaced by a recorded two-second CC0 group cheer;
standing triggers it once and result viewing retains the 25% celebration mix.
Existing gameplay step recordings, ceremony timing and leaderboard designs stay intact.

AndroidSubmission: **168,438,635 -> 168,495,603 bytes (+56,968)**;
remaining headroom **831,504,397 bytes** below 1,000,000,000.
SHA-256: **2F67E7CC3845E8A4DA089F756E62ECC121E6B3B988C6E748785BD673343ABD6F**.
V2 signature, ZIP CRC, all 1,428 audit entries and five-scene bundle integrity pass.
The largest entries remain islands (45,948,280 compressed bytes), IL2CPP
(13,329,398), Vivox (10,240,163) and Unity (9,828,173). Audio replacement and
contact trims account for the small growth; entry deltas are retained in evidence.
No existing art quality was reduced. The first Android attempt failed in Java's
local loopback setup; retry with an isolated temporary directory and IPv4 succeeded.

All **218 runtime assertions**, **3,664 localization entries**, eight contact-onset
checks and portable regeneration checks pass. English, Chinese and Malay result
captures were reviewed. All **546 Windows / 385 Android** compiled source hashes
match. The complete 335-file Windows player was promoted and hash-verified;
two retained awards preview launchers bring WindowsFinal to 337 files.
Evidence: `Builds/AwardsFootstepQA/`. Physical speaker balance, phone FPS/touch
and live WAN remain unverified. No Git publishing was performed.

## Previous delivery verification (before contact-alignment revision)



Focused review entry: `-awardsPolishReview <ignored-output-folder>` in a development

player, plus the existing `-islandAwardsReview` entrance regression. Validation

covers the final player.



Fishing/Golf now have synchronized visible-foot steps, a quiet Fishing podium-rise

water layer, and one fanfare/applause on settling. Celebration tails duck to 25%

under results. Full Golf rankings reveal the local row once per opening; both

result designs emphasize the local row and reserve its translated suffix when

shortening names. Hidden report controls no longer consume name space.



AndroidSubmission: **168,037,240 -> 168,438,635 bytes (+401,395)**;

remaining headroom **831,561,365 bytes** below 1,000,000,000.

SHA-256: **4C4AF911FA41807646E73E95D9D79AF5AC86D0CBE462F13CA8546942C2465FCD**. The local baseline was the earlier awards-only

snapshot. This full rebuild also includes the previously pulled account, rewards

and moderation source; its growth is not solely the new audio.



The APK passes v2 signature, ZIP CRC, all 1,412 exact audit entries,

and the five-scene bundle length/hash checks. Largest compressed entries:

islands bundle **45,947,971**, IL2CPP **13,328,811**, Vivox **10,240,163** and

Unity **9,828,173** bytes. Entry deltas were reviewed; the new 243,244-byte source

applause and the integrated resources/code explain the growth. No existing art

quality was reduced. The generator reproduces identical WAV bytes from a separate

checkout path containing spaces and an unrelated working directory.



**218 runtime assertions** pass: 141 focused audio/results assertions across

English, Simplified Chinese and Malay, plus 77 existing entrance checks.

16:9 and 4:3 captures were reviewed; all **3,664** localization entries pass.

All **546 Windows / 385 Android** compiled source documents match. The complete

335-file candidate was copied and hash-checked; two retained preview launchers

bring WindowsFinal to 337 files. Evidence: `Builds/AwardsSoundQA/`.

Phone rendering/touch/FPS, physical speaker balance and live WAN remain unverified.

Details and archives: [awards sound and results](AWARDS-SOUND-AND-RESULTS.md).







Reviewed superseded/draft APKs, player and promoted duplicate: `Legacy/20261027-212249-631-awards-sound-android/`, `Legacy/20261027-212938-008-awards-sound-draft-android/`, `Legacy/20261027-213203-582-awards-sound-windows/`, `Legacy/20261027-213401-152-awards-sound-promoted/`.



Unity regenerated Golf prefab IDs and the character library editor cache hash.

The complete 14-object/component prefab graph and all character payload fields

were verified unchanged before retaining their original source representation.

Earlier diagnostic runs remain in the evidence folder; only runs listed in

`validation.json` count toward the final 218 assertions. Capture fixtures contain

15 Golf standings / 5 Fishing results and do not represent network participants.



The final Android build, source comparisons, localization check, whitespace check and Check-TaskReady.ps1 -RequireApk pass. No staging, commit or push was performed for this task.


## Contact-alignment revision archives

- `Legacy/20261027-225534-135-awards-cheers/`: obsolete synthetic applause and meta; runtime/GUID references checked before archiving.
- `Legacy/20261027-225940-816-awards-cheers-android/`: prior APK and audit.
- `Legacy/20261027-230223-341-awards-cheers-windows/`: superseded complete Windows player.
- `Legacy/20261027-230623-871-awards-cheers-promoted/`: validated candidate after promotion and portable-regeneration fixture.

## Reference-crowd revision archives

- `Legacy/20261028-214953-237-awards-crowd-source/`: old standalone OGG and preceding delivery/generator copies.
- `Legacy/20261028-215409-551-awards-crowd-android/`: preceding APK and audit.
- `Legacy/20261028-215558-973-awards-crowd-windows/`: preceding complete Windows player.
- `Legacy/20261028-215650-828-awards-crowd-promoted/`: tested candidate after promotion and portable-regeneration fixture.
