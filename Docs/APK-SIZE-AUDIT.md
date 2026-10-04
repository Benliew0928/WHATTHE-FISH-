# APK size audit — measured release records

## GitHub Golf and football integration — 5 October 2026

Fresh combined primary AndroidSubmission at **00:07:25 Malaysia time**:
**89,024,294 → 89,230,290 bytes (+205,996)**. Strict headroom is
**10,769,710 bytes**; the development target remains exceeded by
**14,230,290 bytes**. Strict gate and independent APK v2 signature pass.
Exact retained APK/audit: `Builds/GitSyncQA/Release/`; detailed comparison:
`Builds/GitSyncQA/release-audit.json`. See
[release scope and archives](BUILD-SIZE.md#github-golf-and-football-integration--5-october-2026).

Serialized assets change **155,324,117 → 155,620,521 bytes (+296,404)**.
Actual compressed ZIP entries grow **204,965 bytes**, with **1,031 bytes** of
additional signing/container overhead. Expected new upstream contributors
include lagoon surface **43,086**, shared fish mesh **40,321**, lagoon bed
**19,892**, and football net shader **9,291 estimated ZIP bytes**. The shared
28-record football pose library contributes only **1,854 estimated ZIP bytes**.
No new package or unexpected large contributor appears. Leading estimates
remain timber **2.247 MB**, character albedo **1.993 MB**, Golf structures
**1.981 MB**, character mesh **1.588 MB** and football stadium **1.556 MB**.
These estimates and serialized totals are separate from actual APK length.

## Golf swing speed 18 m/s — 4 October 2026

Fresh primary AndroidSubmission at **23:27:16 Malaysia time** measures
**89,024,798 → 89,024,294 bytes (−504)**. Strict headroom is **10,975,706 bytes**;
the development target remains exceeded by **14,024,294 bytes**. The strict
gate and independent APK v2 signature pass. Exact APK and all audit files are
retained in `Builds/GolfEighteenSpeedQA/Release/`. Release provenance, source
scope and archives are in
[BUILD-SIZE](BUILD-SIZE.md#golf-swing-speed-18-ms--4-october-2026).

Serialized assets measure **155,324,117 bytes**, **8 bytes** above the preceding
five-metre configuration. `GolfBallPhysics.asset` grows from **88 to 96 serialized
bytes**, with its estimated ZIP contribution changing from **68 to 76 bytes**.
No new art, audio, font, animation or package is added by the launch-speed fix.
Leading compressed estimates remain timber **2.247 MB**, character albedo
**1.993 MB**, Golf structures **1.981 MB**, character mesh **1.588 MB** and
football stadium **1.556 MB**. No unexpected large contributor appears.
Estimated contributions and serialized totals are separate from exact APK size.
Evidence: `Builds/GolfEighteenSpeedQA/packing-top.txt`, `release-audit.json` and
`apk-signature.txt`; current primary audit: `Builds/SizeAudit/latest/`.

## Golf meadow restored — 4 October 2026

Fresh shared primary AndroidSubmission: **89,021,758 → 89,024,882 bytes
(+3,124)**, including concurrent shot-range/cart changes. Strict headroom
**10,975,118 bytes**; development target exceeded by **14,024,882**. Gate and
independent v2 signature pass. Snapshot limits and archives:
[BUILD-SIZE](BUILD-SIZE.md#golf-meadow-restored--4-october-2026).

Serialized assets **155,324,109 bytes**, only **4 bytes** above the prior Swing
release. Android reuses the existing grass library; restoring 2,289 blades adds
no shipped mesh or texture. The 21 obsolete derivatives are proven unreferenced.
Leading compressed estimates remain timber **2.247 MB**, character albedo
**1.993 MB**, Golf structures **1.981 MB**, character mesh **1.588 MB** and
football stadium **1.556 MB**. No unexpected large contributor appears.
These estimates and serialized totals differ from exact APK length.
Fresh detailed audit/APK: `Builds/GolfGrassQA/Release/`; source scope, signature,
original-map hashes and package comparison: `Builds/GolfGrassQA/`.

## Golf cart forward-facing feet — 4 October 2026

Fresh task APK: **89,021,758 → 89,025,074 bytes (+3,316)**, built at
**22:40:48 Malaysia time**. Strict headroom **10,974,926 bytes**;
development target exceeded by **14,025,074 bytes**. AndroidSubmission,
APK v2 signature and independent packing audit pass. Serialized assets grow
only **4 bytes** from the concurrent five-metre Golf configuration; no art is
added by the feet correction. ZIP entries grow 3,314 bytes and signing/container
overhead grows 2 bytes. The largest estimates remain timber 2.247 MB, character
albedo 1.993 MB, Golf structures 1.981 MB and character mesh 1.588 MB.

Exact task packages/audit are retained in `Builds/GolfCartFeetQA/Release/`.
The newer shared APK (**89,024,882 bytes**, 22:44:23) is preserved;
its v2 signature and generated native toe-tip branch were checked. Current
shared headroom is **10,975,118 bytes**; its development-target
excess is **14,024,882 bytes**. These snapshots differ in unrelated
concurrent grass work. The initial 313-file Windows feet snapshot passed
**439 checks**. The newer complete shared Windows player (22:43:13) includes
the same feet correction and passed another **221 cart assertions**. See
[verification and scope](VERIFICATION.md#golf-cart-forward-facing-feet--4-october-2026).

## Reliable Golf Swing proximity — 4 October 2026

Fresh measured APK: **89,021,758 bytes**, **+20,774** from opening
**89,000,984**, including intervening tee/cart deliveries, and **−300** from
the immediate **89,022,058-byte** predecessor. Strict headroom is
**10,978,242 bytes**; the development target is exceeded by **14,021,758**.
AndroidSubmission and independent APK v2 signature pass.

Serialized assets remain **155,324,105 bytes**, identical to the immediate
predecessor. Every per-asset contribution matches; no extra mesh, texture,
font, audio, animation or package is shipped by this fix. Actual compressed
ZIP entries decrease **299 bytes** and container/signing overhead decreases
**1 byte**. The leading per-asset estimates remain timber **2.247 MB**,
character albedo **1.993 MB**, Golf structures **1.981 MB**, character mesh
**1.588 MB** and football stadium **1.556 MB**. No unexpected large contributor
appears. These estimates and serialized bytes are separate from APK length.

Current fresh audit: `Builds/SizeAudit/latest/`; exact retained audit and APK:
`Builds/GolfSwingQA/Release/`. Exact comparisons, signature and source scope:
`Builds/GolfSwingQA/release-audit.json`, `apk-signature.txt` and
`delivery-source-inputs.json`. See
[release and archives](BUILD-SIZE.md#reliable-golf-swing-proximity--4-october-2026)
and [feature checks](VERIFICATION.md#reliable-golf-swing-proximity--4-october-2026).


## Golf wooden opening tee — 4 October 2026

Measured APK: **89,000,984 → 89,022,058 bytes (+21,074)**. Strict-limit
headroom is **10,977,942 bytes**; the development target remains exceeded by
**14,022,058 bytes**. The reused concurrent AndroidSubmission package contains
the exact tee and gameplay sources. Its gate, fresh detailed packing analysis
and independent v2 signature pass. Retained APK/audit:
`Builds/GolfWoodTeeQA/Release/`. Primary's earlier OOM attempt produced no APK;
source comparison and reuse provenance are in
[BUILD-SIZE](BUILD-SIZE.md#golf-wooden-opening-tee--4-october-2026).

| Shared tee asset | Serialized bytes | Estimated ZIP bytes |
| --- | ---: | ---: |
| 832-triangle lathe mesh, also used for static cup collision | 21,304 | 10,933 |
| 64×128 wood grain, Android ASTC 6×6 with mipmaps | 5,516 | 4,529 |
| Instanced Lit material | 1,660 | 632 |
| Opening support prefab | 443 | 172 |
| Total | 28,923 | 16,266 |

Every player's support shares these assets. The user reference stays outside
Unity in `ArtSource/Golf/Tee/`; it is absent from the shipped asset graph. No
replacement ball, package, audio or large texture is added. The existing ball
retains its 15,998/720-triangle meshes and material. Its ground query also
recognizes the support's Ignore Raycast layer, while ordinary character/cart
terrain queries retain their original layer.

Serialized assets total **155,324,105 bytes (+29,295)**. Exact ZIP entries
grow **20,214 bytes** and container/signing overhead grows **860 bytes**;
these measurements differ from per-asset compression estimates. Concurrent
validated settling, swing-address and seated-pose code is retained. Leading
contributors remain timber **2.247 MB**, character albedo **1.993 MB**, Golf
structures **1.981 MB**, character mesh **1.588 MB**, and football stadium
**1.556 MB**. No unexpected large dependency appeared. The 75 MB target remains
outstanding; this small prop does not change that wider optimization requirement.

## Golf cart straight-knee driver — 4 October 2026

Fresh APK: **89,022,058 bytes**; **+19,204** from the recorded opening
**89,002,854**, and **+21,188** from the intervening **89,000,870-byte** contact
release. Strict-limit headroom: **10,977,942**; development-target excess:
**14,022,058**. AndroidSubmission and independent v2 signing pass.

The straight-knee pose reuses the existing character, cart and animations.
Serialized assets are **155,324,105 bytes**, **+29,099** against the contact
release. Retained concurrent tee entries account for **28,923**: mesh **21,304**,
grain **5,516**, material **1,660**, prefab **443**. Their compressed estimates
are about **10,933 / 4,529 / 632 / 172 bytes** respectively. Tee/Swing probe
MonoScript metadata adds the remaining **176 serialized bytes**; opt-in probe
code is compiled out of release. No large new dependency appears.

Actual ZIP compressed entries grow **20,501 bytes**; container/signing
overhead accounts for **687 bytes** of the immediate APK difference.
Fresh per-asset estimates retain timber **2.247 MB**, character albedo
**1.993 MB**, Golf structures **1.981 MB**, character mesh **1.588 MB**, and
football stadium **1.556 MB** as the leading entries. These estimates differ
from exact ZIP and APK lengths. Ignored performance-test resource records
are omitted from the isolated build.

Exact APK and all **11 fresh audit files**: `Builds/GolfCartSeatQA/Release/`;
shared audit: `Builds/SizeAudit/latest/`. Packing comparison, signatures,
source scope and complete-package hashes are under `Builds/GolfCartSeatQA/`.
See [delivery records](BUILD-SIZE.md#golf-cart-straight-knee-driver--4-october-2026)
and [455 checks](VERIFICATION.md#golf-cart-straight-knee-driver--4-october-2026).
Physical-phone rendering/FPS and WAN remain unverified.

## Golf slope settling and resting — 4 October 2026

Final APK **89,000,870 bytes**: **138 bytes below** opening **89,001,008**,
**114 below** the intervening Aim delivery **89,000,984**, and **1,984 below**
the interim settling candidate. Hard-limit headroom is **10,999,130**, with
**14,000,870** above the development target. AndroidSubmission, independent
v2 signature and fresh per-asset audit pass.

Serialized assets total **155,295,006 bytes (+196)**: **84 bytes** of persistent
physics settings and **112 bytes** of MonoScript metadata. No delivered art,
texture, mesh, animation, font or package is added. Actual compressed ZIP entries
fall **311 bytes**; container/signing overhead increases **173 bytes**, giving
the exact **138-byte APK reduction**. Per-asset estimates stay led by timber
**2.247 MB**, character albedo **1.993 MB**, Golf structures **1.981 MB**,
character mesh **1.588 MB**, football stadium **1.556 MB**. No unexpected large
dependency appears. Serialized bytes and per-asset estimates are not APK length.

Fresh retained audit: `Builds/GolfSettlingQA/Release/SizeAudit/`; shared audit:
`Builds/SizeAudit/latest/`. Concurrent in-progress tee, swing-review and
cart/locomotion changes are outside this original-map snapshot. See
[hash, input scope and delivery](BUILD-SIZE.md#golf-slope-settling-and-resting--4-october-2026)
and [physics and gameplay checks](VERIFICATION.md#golf-slope-settling-and-resting--4-october-2026).


## Golf 3x ball and optional Aim — 4 October 2026

Fresh APK: **89,000,984 bytes**, **+16,796** from the recorded **88,984,188-byte**
opening and **24 bytes below** the intervening grip release. Hard-limit
headroom is **10,999,016 bytes**; the development target remains exceeded by
**14,000,984 bytes**. AndroidSubmission and independent v2 signing pass.

The ball reuses its existing **15,998 / 720-triangle** LODs and material.
Prefab visual scale and physical radius grow together; no model/texture
payload is added. The shared Chinese control font grows **5,320 → 6,256 bytes**
for Aim/Cancel, with **936 serialized bytes / 728 estimated ZIP bytes** added.
The retained concurrent grip adds **15,168 serialized bytes / 13,560 estimated
ZIP bytes** to the existing character mesh, plus **123 serialized bytes** to
each character prefab. Serialized assets total **155,294,810 (+16,350)**.
Exact compressed ZIP entries grow **16,798 bytes**; container/signing overhead
changes by **−2 bytes**. These are separate from per-asset estimates.

Fresh estimates use this APK's own detailed packing offsets. Leading entries
remain timber **2.247 MB**, character albedo **1.993 MB**, Golf structures
**1.981 MB**, character mesh **1.588 MB**, and football stadium **1.556 MB**.
No unexpected large dependency appears. Exact retained APK/audit:
`Builds/GolfBallAimQA/Release/`; raw signatures, source fingerprints and
comparisons: `Builds/GolfBallAimQA/`. See
[release/delivery scope](BUILD-SIZE.md#golf-3x-ball-and-optional-aim--4-october-2026)
and [native checks](VERIFICATION.md#golf-3x-ball-and-optional-aim--4-october-2026).
Phone quality/FPS and WAN remain unverified.

## Golf closed grip and raised carry angle — 4 October 2026

Fresh APK **89,001,008 bytes**, **+16,820** from opening; strict-limit headroom
**10,998,992**, development-target excess **14,001,008**. AndroidSubmission,
independent v2 signature and fresh per-asset packing analysis pass.

Serialized assets are **155,294,810 bytes (+16,350)**, separate from compressed
APK size. The derived closed-hand shape and grip socket add **15,168 serialized
bytes** to the character mesh and **123 bytes** to each athlete prefab. The
concurrent aim-button snapshot adds **936 bytes** to the existing label font.
There is no added texture, material, renderer or package. Both original character
LODs remain; only **335 / 55** positions receive grip deltas. The original FBX
SHA-256 is unchanged, with no new binary dependency paths.

Leading compressed estimates: timber **2.247 MB**, character albedo **1.993 MB**,
Golf structures **1.981 MB**, character mesh **1.588 MB** (about **13,560 bytes**
additional), football stadium **1.556 MB**. No unexpected large dependency
appears. Actual compressed ZIP entries grow **16,822 bytes**, with **−2 bytes**
of container/signing overhead. Per-asset estimates are not exact ZIP attribution.

Retained fresh audit: `Builds/GolfGripSlopeQA/Release/SizeAudit/`; shared audit:
`Builds/SizeAudit/latest/`. See
[hash, snapshot scope and archives](BUILD-SIZE.md#golf-closed-grip-and-raised-carry-angle--4-october-2026)
and [172-check gameplay verification](VERIFICATION.md#golf-closed-grip-and-raised-carry-angle--4-october-2026).


## Golf ball visibility and free physics — 4 October 2026

Fresh APK: **88,984,188 bytes**; **+2,364** from the recorded task opening and
**+1,792** from the intervening palm-carry release. It leaves **11,015,812 bytes**
below the strict ceiling and exceeds the development target by **13,984,188**.
AndroidSubmission and independent v2 signing pass. No art, shader, font, audio
or package is added. Rolling physics, player/cart isolation, camera framing
and UI marker spacing reuse existing systems and assets.

Serialized assets total **155,278,460 bytes (+104)**: the new opt-in probe
contributes only MonoScript metadata to release packing, while its fixture code
is compiled out. Asset estimates use this APK's own fresh packing offsets.
Leading estimates remain timber **2.247 MB**, character albedo **1.993 MB**,
Golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium
**1.556 MB**. No unexpected large dependency appears. Actual compressed ZIP
entries grow **1,788 bytes** against the preceding carry release, plus **4
bytes** of container/signing overhead.

Exact retained APK/audit: `Builds/GolfBallPhysicsQA/Release/`; current shared
audit: `Builds/SizeAudit/latest/`. Hash, input scope, complete Windows delivery
and archives are in
[BUILD-SIZE](BUILD-SIZE.md#golf-ball-visibility-and-free-physics--4-october-2026).
See [measured gameplay checks](VERIFICATION.md#golf-ball-visibility-and-free-physics--4-october-2026).
Phone quality/FPS and WAN remain unverified.

## Golf palm animation — 4 October 2026

The freshly built and independently signed APK measures **88,982,396 bytes**,
**572 bytes above** the opening release, with **11,017,604 bytes** below the
strict ceiling and **13,982,396 bytes** above the development target. The right
wrist correction adds no mesh, texture, animation asset, shader, font or package.
Serialized assets remain **155,278,356 bytes**. Fresh detailed packing offsets
were analyzed against this APK itself. Leading estimates remain timber
**2.247 MB**, character albedo **1.993 MB**, Golf structures **1.981 MB**,
character mesh **1.574 MB** and football stadium **1.556 MB**; no unexpected
large dependency appeared.

Exact APK/audit: `Builds/GolfPalmCarryQA/Release/`; build logs, signature and
input comparisons: `Builds/GolfPalmCarryQA/`. The shared Android APK,
`Builds/SizeAudit/latest/` and complete `Builds/WindowsFinal/` are updated to this
validated release. See
[hash, snapshot scope and delivery status](BUILD-SIZE.md#golf-palm-animation--4-october-2026)
and [142-check gameplay validation](VERIFICATION.md#golf-palm-animation--4-october-2026).
Physical-phone quality/FPS and WAN remain unverified.

## Rounded Golf ball — 4 October 2026

Fresh APK: **89,418,634 → 88,981,824 bytes (−436,810)**, leaving **11,018,176
bytes** below the strict ceiling and **13,981,824 bytes** above the development
target. AndroidSubmission, clean packing and independent v2 signing pass.
See [exact release, input scope and recovery](BUILD-SIZE.md#rounded-golf-ball--4-october-2026).

The original 87,048-triangle rounded-dimple model is retained as an editable
master and byte-identical FBX outside Unity. Delivery uses **15,998 / 720
triangles**, low mesh compression, imported normals, no tangents and one shared
ivory material. The distant mesh preserves a round silhouette once the small
dimples are below pixel scale. No new texture, shader, font or package is
shipped. Both LODs fit the existing 43 mm sphere; no duplicate physics body or
collider is added. The old ball's normal map remains available as historical
source provenance but is no longer referenced by the current material or
included in this APK.

Serialized assets measure **155,278,356 bytes (−583,832)**. The fresh mesh
estimate is **198,847 bytes (+160,483)**, the former normal map's **596,010
estimated bytes / 699,212 serialized bytes** are absent, and material metadata
decreases **13 estimated / 16 serialized bytes**. Exact compressed ZIP entries
shrink **436,645 bytes**; container/signing overhead shrinks **165 bytes**.
All other art sizes are stable, apart from negligible compression differences.
Leading estimates remain timber 2.247 MB, character albedo 1.993 MB, Golf
structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB.
There is no unexpected large shipped dependency. These estimates were computed
from this APK's own detailed packing offsets, not the earlier cached report.

Current audit: `Builds/SizeAudit/latest/`; exact retained audit and APK:
`Builds/GolfBallRoundedQA/Release/`. Comparisons, source manifests and signing:
`Builds/GolfBallRoundedQA/`. Format-aware primary/alternate-checkout audits find
no active image/library dependency or machine path; inactive exporter SceneInfo
and saved Blender UI provenance are documented exceptions. See
[regeneration](../ArtSource/Golf/Ball/README.md) and
[functional/visual checks](VERIFICATION.md#rounded-golf-ball--4-october-2026).
Phone touch/rendering/FPS and WAN remain unverified.

## Golf club natural carry — 4 October 2026

The retained carry-task **88,981,788-byte** APK shrinks **436,846 bytes** from the opening
89,418,634-byte release. Hard-limit headroom is **11,018,212 bytes**; the 75 MB
development target remains exceeded by **13,981,788 bytes**. AndroidSubmission
and independent APK v2 signing pass. Hash, exact release paths and input scope
are in [BUILD-SIZE](BUILD-SIZE.md#golf-club-natural-carry--4-october-2026).

Natural hand carry adds no mesh, map, animation, shader, font or package. The
existing 3,600 / 1,400 / 500-triangle club and 512 / 512 / 256-pixel maps remain
unchanged. This combined snapshot preserves the concurrent rounded-ball update:
ball geometry grows **115,396 serialized / 160,483 estimated bytes**, while its
old normal map removes **699,212 serialized / 596,010 estimated bytes**. Total
serialized assets shrink **583,832 bytes** to **155,278,356**. Actual compressed
ZIP entries shrink **436,680 bytes**, plus **166 bytes** of container/signing
overhead. Fresh offsets and estimates were calculated from this APK itself.
Timber (2.247 MB), character albedo (1.993 MB), Golf structures (1.981 MB),
character mesh (1.574 MB) and football stadium (1.556 MB) remain the largest
estimates. No unexpected large dependency appeared.

Exact retained audit: `Builds/GolfClubCarryQA/Release/latest/`.
Before/after input manifests, signatures,
package deltas and the canonical Ball prefab graph comparison are in
`Builds/GolfClubCarryQA/`. Primary source stays unchanged across packaging;
Android URP registrations and local prefab IDs are generated, and the isolated
editor uses clean packing. See [whole-gait, native-view and loopback verification](VERIFICATION.md#golf-club-natural-carry--4-october-2026).
The tested complete Windows player is retained in the same release folder;
physical-phone quality/FPS and WAN remain unverified.

The newer rounded-ball delivery includes this carry implementation and supersedes
the shared APK/player. The current APK is **88,981,824 bytes**, **436,810 bytes
smaller** than the task's opening release, with **11,018,176 bytes** of strict-limit
headroom. The development target remains exceeded by **13,981,824 bytes**.
Current `Builds/SizeAudit/latest/` reports belong to that newer APK; its exact
measurements and input scope are in the rounded-ball entry above. The complete
shared Windows player also passes the final 140-check carry/Golf suite.

## Midnight Iron handheld club — 4 October 2026

The final APK measures **89,418,634 bytes**, **+630,844** from the opening **88,787,790-byte** artifact and **+455,624** from the separately validated reference-tyre release. Hard-limit headroom is **10,581,366 bytes**; the development target is still exceeded by **14,418,634 bytes**. AndroidSubmission and independent APK v2 signature verification pass. SHA-256: **49BC113AFE2C569D42CCB63154158294E6748C81AF309F0638279A71D357E679**. Exact current/retained APKs and complete matching Windows delivery are recorded in [BUILD-SIZE](BUILD-SIZE.md#midnight-iron-handheld-club--4-october-2026).

The 7,702-triangle editable master and all original 2048-pixel maps remain under ArtSource. Only **3,600 / 1,400 / 500** delivery triangles, one URP material, **512-pixel colour/normal** and a **256-pixel metallic/smoothness mask** enter Unity. Mobile formats are ASTC 6×6 / 4×4 / 8×8 respectively, with mipmaps. All maps explicitly import as Texture2D and are referenced by the material; the missing-map cubemap trial was corrected and rebuilt. Three LODs share one material; two-hand motion uses the existing avatar rig, with no shipped swing animation, additional shader, font or package. The club has no rigidbody or collider.

Fresh packing-offset analysis gives club estimates of **123,767 bytes mesh**, **129,848 colour**, **131,740 normal**, **18,413 mask** and **1,991 prefab/material/script metadata**: **405,759 bytes total**. Serialized assets measure **155,862,188 bytes**, **+919,293** from the opening snapshot; these are not compressed APK bytes. Exact ZIP-entry growth is **629,813 bytes**, with **1,031 bytes** of container/signing overhead completing the APK delta. Against the tyre release, existing asset sizes remain stable apart from eight player-prefab serialized bytes and negligible estimate changes. The new club and updated IL2CPP/metadata packing account for the incremental snapshot; no unexpected large dependency appeared. Leading estimates remain timber 2.247 MB, character albedo 1.993 MB, Golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB.

Audit files and exact current packing offsets: `Builds/SizeAudit/latest/`; retained release: `Builds/GolfClubQA/Release/latest/`. Comparisons, signing, unchanged 2,947-input capture and Windows promotion hashes: `Builds/GolfClubQA/`. Both primary and alternate-checkout format-aware audits pass: all seven Blender image dependencies and FBX texture references are relative and exist, with no external library or active machine path. Original embedded source/provenance is retained for editing; the delivery FBX has no inactive machine-path provenance fields. See [source and regeneration](../ArtSource/Golf/Club/README.md) and [functional/visual verification](VERIFICATION.md#midnight-iron-handheld-club--4-october-2026). Phone quality/FPS and WAN remain unverified.

## Golf cart reference tyres — 4 October 2026

The fresh tyre-verification APK measures **88,787,790 → 88,963,010 bytes (+175,220)**, with **11,036,990 bytes** below the strict ceiling and **13,963,010 bytes** above the development target. AndroidSubmission, clean packing and independent v2 signing pass. The exact retained APK and complete audit are in `Builds/GolfCartTreadQA/Release/`; the shared root delivery is preserved because concurrent Golf-club additions postdate this captured source. See [hash, scope and recovery](BUILD-SIZE.md#golf-cart-reference-tyres--4-october-2026).

Serialized assets are **155,187,375 bytes (+244,480)**. The cart FBX estimate rises **172,773 bytes to 909,743** for the requested rounded thick tyre and geometric chevron channels. All other per-asset estimates and cart texture hashes are unchanged, retaining one material and five renderers per LOD. Mid/far axial simplification removes **2,560 triangles** versus the full new profile. Exact compressed entries grow **175,220 bytes**, with no net container-overhead change. Leading contributors remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB; no unexpected shipped dependency or texture growth appeared. Estimates were recalculated from this APK's own packing offsets. Phone rendering/FPS and the 75 MB development goal remain unqualified.

## Golf cart rim fit and circularity — 4 October 2026

Fresh combined snapshot: **88,713,562 → 88,787,790 bytes (+74,228)**, with **11,212,210 bytes** strict-limit headroom and **13,787,790 bytes** above the development target. AndroidSubmission, complete packing and independent APK v2 signing pass. See [release hash, captured scope and recovery](BUILD-SIZE.md#golf-cart-rim-fit-and-circularity--4-october-2026).

Serialized assets are **154,942,895 bytes**; the cart mesh estimate changes **659,132 → 736,970 bytes** for circular equal rims, joined beads and concurrent chassis supports. Existing texture hashes, one shared material and five renderers per LOD are retained. Continuous UVs remove **6,288 position/UV splits** without changing wheel geometry; mid/far spokes omit **1,536 total bevel triangles**. The final combined APK is **4,560 bytes smaller** than the intermediate before that packing refinement. Exact ZIP-entry growth against the starting APK is **74,225 bytes**, with three bytes of container overhead completing the APK delta. Leading art contributors are unchanged and no new large dependency appears. Current per-asset/ZIP records are in `Builds/SizeAudit/latest/`, comparisons and raw source/functional evidence in `Builds/GolfCartRimQA/`. The 75 MB target and phone quality/FPS remain unqualified.

## Golf cart chassis connections and startup rolling — 4 October 2026

Final APK: **88,708,082 → 88,782,546 bytes (+74,464)** against the task-opening combined snapshot, or **88,787,118 → 88,782,546 bytes (−4,572)** against the preceding complete shared release. Hard-limit headroom is **11,217,454 bytes**; the development target remains exceeded by **13,782,546 bytes**. AndroidSubmission, complete clean-cache packing and independent v2 signing pass. Source consistency, release hash, tests and reviewed recovery batch are recorded in [BUILD-SIZE.md](BUILD-SIZE.md#golf-cart-chassis-connections-and-startup-rolling--4-october-2026).

The twelve checked wheel-to-chassis connections add **288 / 224 / 160 triangles** across the three LODs, preserving five active renderers and one material. No new texture, font, shader or package is shipped. The high-quality source geometry/UV hash and delivery map hashes are unchanged. Final serialized assets measure **154,942,895 bytes (−84,608)** against the preceding release, with cart mesh packing **−84,612** and the prefab field **+4**. Independent cart mesh estimate is **736,970 bytes (−1,378)**; its colour, normal, mask and material estimates remain **498,110 / 621,270 / 71,311 / 671 bytes**. Exact compressed entries shrink **4,570 bytes**: cart mesh **−2,479**, IL2CPP **−2,086** and small metadata/engine/config changes explain the result. Container/signing overhead shrinks two more bytes. Asset estimates are distinct from the actual APK measurement.

Leading estimates remain timber **2.247 MB**, character albedo **1.993 MB**, Golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected dependency or texture growth appeared. Complete current reports are in `Builds/SizeAudit/latest/`, with an exact retained audit at `Builds/GolfCartWheelContactQA/Release/latest/`; asset/entry deltas are in `Builds/GolfCartWheelContactQA/`. The latest matching player passes **203 cart assertions** and both format-aware art audits. Close/far LOD and full-steering views were inspected; phone quality/FPS remains unverified.

## Golf cart elevated steering follow — 4 October 2026

Fresh combined release: **88,708,082 → 88,787,118 bytes (+79,036)**. Hard-limit headroom: **11,212,882 bytes**; the 75,000,000-byte development target remains exceeded by **13,787,118 bytes**. AndroidSubmission, complete clean-cache packing and independent v2 signing pass. The elevated camera change adds no imported art, texture, font, shader or package. The package includes the separately refined rim/bead mesh and sand-bowl motor fix. See [hash, source consistency, build and archive](BUILD-SIZE.md#golf-cart-elevated-steering-follow--4-october-2026).

Serialized assets measure **155,027,503 bytes (+254,880)**; the cart mesh accounts for that increase and rises **79,216 estimated compressed bytes to 738,348**. Its maps and material are unchanged. Exact compressed entries grow **79,039 bytes**: cart mesh data **+78,860** and combined IL2CPP **+178** are the main contributors. Container/signing overhead falls 3 bytes. All other independently estimated assets remain unchanged. Leading estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. No unexpected large dependency or camera asset growth appeared.

All 2,925 captured inputs remain stable during packaging and match the primary workspace except the isolated clean-cache editor option. Packing records: `Builds/SizeAudit/latest/`; exact retained release/audit: `Builds/GolfCartOverheadQA/Release/`; comparisons and hashes: `Builds/GolfCartOverheadQA/`. The matching Windows player passes 175 cart and 81 Golf gameplay checks, with native elevated views inspected. Phone touch, rendering/FPS and WAN remain unverified; the 75 MB development target still requires deliberate optimization.

## Golf cart sand-bowl rim traversal — 4 October 2026

The captured vehicle-fix APK measures **88,708,270 bytes**, **+82,432** against the task-opening 88,625,838-byte package and **+188** against the complete Golf mini-game release. Hard-limit headroom is **11,291,730 bytes**; the development target is exceeded by **13,708,270 bytes**. AndroidSubmission, independent v2 signing and fresh packing/contribution analysis pass. Artifact/audit: `Builds/GolfCartRoundFixQA/Release/WhatTheFish-release.apk` and `Builds/GolfCartRoundFixQA/Release/latest/`. See [hash, captured-source scope and archive records](BUILD-SIZE.md#golf-cart-sand-bowl-rim-traversal--4-october-2026).

The ground-sweep fix adds **zero serialized asset bytes** against the preceding complete release; serialized assets remain **154,772,623 bytes**. Exact compressed entry growth is **189 bytes**, with IL2CPP **+188** and a one-byte container difference. The captured cart mesh estimate is **659,132 bytes**; its texture maps and material are unchanged. No new art, shader or package is introduced. The later parallel rim/bead geometry export is outside this measured snapshot; the shared root APK is preserved. The 169-check cart replay includes 40 forward/reverse sand-bowl crossings. Phone touch, rendering/FPS and WAN remain unverified, and the 75 MB development target remains unmet.

## Golf mini game — 4 October 2026

Fresh Golf release: **88,625,838 → 88,708,082 bytes (+82,244)**. Hard-limit headroom: **11,291,918 bytes**; the 75,000,000-byte development target remains exceeded by **13,708,082 bytes**. AndroidSubmission, complete clean-cache packing and independent APK v2 signing pass. Golf reuses course/ball art, font and native UI arrows without importing assets, shaders or packages. This combined snapshot includes separate camera/circular-tyre changes. See [hash, captured source scope, build recovery and archive](BUILD-SIZE.md#golf-mini-game--4-october-2026).

Serialized assets remain **154,772,623 bytes**, **62,952** above the task's opening baseline. The separate cart mesh estimate rises **37,071 bytes to 659,132**, with unchanged maps. Exact compressed entry growth is **82,241 bytes**, including IL2CPP **+44,396**, cart mesh data **+36,476**, metadata **+6,198** and Burst **−5,657**. Container/signing overhead adds 3 bytes. Golf's small script records describe metadata; its development probe behavior is absent from release code. No unexpected large dependency or avoidable Golf texture/font growth was found.

Largest estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. Current packing: `Builds/SizeAudit/latest/`; retained exact snapshot: `Builds/GolfMiniGameQA/Release/latest/`; baseline comparisons and input hashes: `Builds/GolfMiniGameQA/`. All 2,925 captured inputs are stable through the build. Later parallel cart-rim FBX/motor/probe edits are outside this release. Its Golf source passes 81 gameplay checks plus existing sport/cart regressions; physical-phone quality/FPS, touch and WAN remain unverified. The remaining hard-limit reserve is not treated as a development budget; the 75 MB target still needs deliberate optimization.

## Golf cart driving cameras — 3 October 2026

Fresh camera snapshot: **88,625,838 → 88,713,562 bytes (+87,724)**. Hard-limit headroom: **11,286,438 bytes**; the 75,000,000-byte development target remains exceeded by **13,713,562 bytes**. AndroidSubmission, complete packing and independent v2 signature pass. The camera change adds no asset, shader, texture or package; this APK also contains concurrent golf gameplay and cart seating/tyre updates. See [hash, captured source scope, build recovery and archive](BUILD-SIZE.md#golf-cart-driving-cameras--3-october-2026).

Serialized assets total **154,772,623 bytes (+62,952)**. The cart mesh estimate rises **622,061 → 659,132 bytes (+37,071)** for the separate circular-tyre work, with unchanged maps. Exact ZIP entry increases are **44,653 bytes** for IL2CPP, **36,476** for cart mesh data and **6,198** for metadata; overall entry growth is 87,725 bytes, one byte above the APK delta because container overhead also changes. Leading estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. No unexpected large dependency was added. Current audit: `Builds/SizeAudit/latest/`; comparison CSVs and signatures: `Builds/GolfCartCameraQA/`.

The captured camera inputs pass 128 cart and 142 golf checks. Subsequent concurrent golf ball/probe edits are outside this APK's verification scope. Phone touch, clipping/rendering/FPS and WAN remain unverified. The 75 MB target still needs deliberate optimization; this release does not treat the remaining hard-limit reserve as a development budget.

## Golf cart seating and circular tyres — 3 October 2026

The historical vehicle-validation snapshot measures **88,625,838 → 88,707,818 bytes (+81,980)**, with **11,292,182 bytes** hard-limit headroom and **13,707,818 bytes** above the development target. AndroidSubmission, complete clean-cache packing and independent APK v2 signing pass. Its APK and audit are now under `Legacy/20261004-004451-752-golf-cart-rim-delivery/Builds/GolfCartRoundFixQA/Release/`. Shared deliveries are preserved. See [hash, captured source scope, disk recovery and archive records](BUILD-SIZE.md#golf-cart-seating-and-circular-tyres--3-october-2026).

Serialized assets grow **62,952 bytes to 154,772,623**. Circular tyres and rear-well cleanup raise the cart FBX independent estimate **37,071 bytes to 659,132**, retaining existing texture hashes, material, authored hubs and the high-quality master. Mobile totals are 25,833/9,686/3,748 triangles. Exact compressed entry growth is **81,977 bytes**; IL2CPP **+44,132**, metadata **+6,198** and Unity native code **+319** include parallel gameplay/camera code. No unexpected large asset or shipped dependency appeared. Fresh contribution estimates were regenerated from this APK's own packing offsets rather than reused from an earlier report. Physical-phone quality/FPS remains unverified, and the 75 MB development target still needs optimization.

## Golf cart rear-wheel rendering and steering — 3 October 2026

Fresh APK: **88,625,346 → 88,625,838 bytes (+492)**, leaving **11,374,162 bytes** hard-limit headroom and exceeding the development target by **13,625,838 bytes**. AndroidSubmission, complete clean-cache packing and independent APK v2 signing pass. See [release hash, input comparison, testing and recovery](BUILD-SIZE.md#golf-cart-rear-wheel-rendering-and-steering--3-october-2026).

Serialized assets increase only **52 bytes to 154,709,671**. Original mesh positions, triangle totals, UVs, maps and material are retained; corrected face winding restores rear hub visibility. Independent mesh estimate changes **622,153 → 622,061 bytes (−92)**; texture estimates are unchanged. Native IL2CPP grows 556 compressed bytes, metadata/Unity native code stay unchanged, and exact total ZIP growth is 492 bytes. Leading art contributors remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. No new wheel texture, shader/package or unexpected large dependency was introduced. Current reports: `Builds/SizeAudit/latest/`; detailed comparison and signing evidence: `Builds/GolfCartHandlingQA/`. Phone quality/FPS remains unverified.

## Golf cart speed and wheels — 3 October 2026

The final corrected APK is **88,625,346 bytes**: **−131,088 bytes** against the retained 88,756,434-byte base-cart package, or **+804 bytes** against the immediately preceding 88,624,542-byte combined course/cart release. Strict-limit headroom is **11,374,654 bytes**; the development target is still exceeded by **13,625,346 bytes**. The base-cart comparison includes concurrent course refinement. See [release hashes, build provenance and verification](BUILD-SIZE.md#golf-cart-speed-and-wheels--3-october-2026).

Complete fresh packing/ZIP/contribution records are in `Builds/SizeAudit/latest/`; comparison CSVs, source hashes and signing evidence are in `Builds/GolfCartWheelsQA/`. Serialized assets remain **154,709,619 bytes**. Wheel partitioning preserves total triangles and all three texture hashes. Independent cart estimates are mesh **622,153**, normal **621,270**, colour **498,110**, mask **71,311** and material **671 bytes**. Mesh estimate reduction is **9,959 bytes**; prefab estimate increase is **2,246**. The existing five-renderer LODs share one material. No imported wheel texture or new shader/package is packed.

Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. Terrain-sector identity changes in the baseline comparison belong to the concurrent course refinement; assess their aggregate, including removed overlays and the original terrain reduction. No unexpected dependency or avoidable wheel-texture growth was found. Actual APK and independent asset estimates remain different measurements. The source master geometry/UV hash is unchanged; exporter `SceneInfo` machine-path metadata is proven inactive and all active art references resolve relatively. Phone quality/FPS remains unverified. Recovery batches are recorded in the release entry above.

## Golf course refinement and current combined release — 3 October 2026

The fresh combined APK measures **86,896,050 → 88,624,542 bytes (+1,728,492)**, leaving **11,375,458 bytes** below the strict ceiling and exceeding the development target by **13,624,542 bytes**. AndroidSubmission, complete clean-cache packing and independent APK v2 signing passed. The package includes the separate protocol 19 cart speed/wheel update. All 2,903 authored asset/package/settings hashes match, with only the isolated editor's clean-cache option differing. See [release provenance and build-process memory mitigation](BUILD-SIZE.md#golf-course-refinement-and-current-combined-release--3-october-2026).

Current records are in `Builds/SizeAudit/latest/`; comparisons and source hashes are in `Builds/GolfCourseRefinementQA/`. Serialized assets are **154,709,619 bytes (+2,598,806)**, distinct from compressed APK bytes. Independent course estimates fall **652,179 → 610,202 bytes (−41,977)**, and the original terrain estimate falls **80,439 bytes**: together **−122,416 bytes**. Five terrain sectors carry precision openings. No green overlay, old orange/putting material or baked golf grass is packed, and the course adds no imported texture or font.

Leading art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. The cart adds the main delivered dependencies: approximately **622,153** mesh, **621,270** normal, **498,110** colour and **71,311** mask bytes in independent compression estimates, plus tiny prefab/font/licence/material/script records. These separate cart additions explain the overall increase despite the course reduction. No unrelated large art growth was found.

Exact compressed ZIP entries grow **1,727,121 bytes**; signing/ZIP overhead accounts for **1,371 bytes**. Assess split-file changes in aggregate. These exact entry measurements and independent per-asset estimates describe different quantities. Actual close/distant and cup-detail views, collision, terrain colour/UV preservation and mobile grass clearing passed their map checks; phone rendering/FPS remains unverified. Unused course assets were archived with GUID/runtime/generator/provenance evidence under `Legacy/20261003-192351-292-golf-course-refine-unused/`.

## Golf cart summon and driving snapshot — 3 October 2026

The retained protocol 18 base APK measures **86,896,050 → 88,756,434 bytes (+1,860,384)**, leaving **11,243,566 bytes** under the strict ceiling and exceeding the development target by **13,756,434 bytes**. AndroidSubmission, complete clean-cache packing and the final retained file's APK v2 signing passed. Later course refinement, protocol 19, doubled speed and wheel animation remain outside this measured snapshot. See [release provenance](BUILD-SIZE.md#golf-cart-summon-and-driving-snapshot--3-october-2026).

`Builds/GolfCartQA/ReleaseBase/latest/` preserves the actual build's packing and ZIP records, with asset estimates regenerated from that APK and its packing offsets. Serialized assets are **154,919,835 bytes (+2,809,022)**. Cart estimates total **1,830,717 bytes**: mesh **632,112**, normal **621,270**, colour **498,110**, mask **71,311**, subset font **3,638**, full licence **1,975**, prefab **1,097**, material **671** and tiny script records. Script records are not compiled native-code cost. All cart LODs share one material and the three maps; Android uses ASTC 6×6 colour, 4×4 normal and 8×8 mask. The master, original ZIP, full 17.77 MB font and unused source textures are not packed.

Exact ZIP-entry growth totals **1,859,011 bytes**, with **1,373 bytes** of ZIP/signing overhead making the actual APK delta. The four main cart art entries add **1,823,308 compressed bytes**; IL2CPP adds **23,228**, metadata **3,313**, the font entry **3,746** and licence entry **2,059**. Full comparison is `Builds/GolfCartQA/base-apk-entry-deltas.csv`. Largest pre-existing art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No avoidable large asset dependency was introduced.

The FBX exporter now rewrites both direct image filenames and active nested Video `Path` properties through format-aware parsing, preserving all non-path payloads. Blender image/library dependencies and FBX texture references were checked after relocation to a path containing spaces and execution from an unrelated directory. The base master has seven existing relative images and no external libraries; the supplied original FBX embeds all four source maps. The retained relocation audit identifies only an inactive `FBXHeaderExtension/SceneInfo/Properties70/P` exporter-provenance field as the allowed historical metadata exception; active texture dependencies are relative. Evidence is `Builds/GolfCartQA/portable-relocated-base-audit.json` and `portable-font-final.txt`. Cleanup batches and functional/device limits are in [the release record](BUILD-SIZE.md#golf-cart-summon-and-driving-snapshot--3-october-2026).

## Five-hole golf map — 3 October 2026

The final APK measures **86,563,288 → 86,896,050 bytes (+332,762)**, leaving **13,103,950 bytes** under the strict ceiling and exceeding the development target by **11,896,050 bytes**. Matching-source isolated AndroidSubmission, populated clean-cache packing and APK v2 signing passed after the open main-project attempt exited. See [release provenance and validation](BUILD-SIZE.md#five-hole-golf-map--3-october-2026).

`Builds/SizeAudit/latest/` contains complete current packing offsets, ZIP entries and estimates. Serialized assets are **152,110,813 bytes (+896,940)**; they are not APK bytes. New course estimates total **652,179 bytes**: derived terrain about **474,257**, greens **159,170**, and shared cup/flag/tee meshes and materials **18,752**. The original terrain estimate falls from **1,284,955 to 964,827 bytes (−320,128)** as four sectors use precise derived geometry. Terrain cutouts remain uncompressed to retain the small cup boundaries; unused vertices are removed. Desktop grass derivatives are retired for the existing mobile grass library and are absent from the APK.

An inherited `RI_Turf_Mask.png` appeared in the first trial package: **5,592,556 serialized bytes**, approximately **147,833 independently compressed bytes**. The putting material now clears its metallic texture/keyword. This removes the dependency without altering the source texture/master or grass colour/normal detail. Actual APK savings are **149,460 bytes** against the **87,045,510-byte** trial. No new imported texture, font, model or package was added. Largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No other unexpected art growth was found.

ZIP entry comparisons include split-file boundary shifts and should be assessed in aggregate. IL2CPP grows **1,646 compressed bytes**, managed metadata **112**; Unity native code and Burst are unchanged. Total compressed entry delta is **332,593 bytes**; ZIP/signing overhead accounts for the remaining **169** APK bytes. These exact entry measurements differ from independently compressed asset estimates.

Evidence: `Builds/GolfFiveHolesQA/`. Source masters match Git HEAD; generated Unity references, geometry winding, course collision, close/distant views and mobile grass clearing were inspected. Phone rendering/FPS remains unverified. Archive batches: `Legacy/20261003-165821-340-golf-five-holes-trial-mesh/` and `Legacy/20261003-171933-414-golf-five-holes-delivery/`. Earlier audit records below retain their original scope.

## Football animation rebuild — 4 October 2026

Actual signed AndroidSubmission APK: **86,764,912 → 86,774,768 bytes (+9,856)**. Hard-limit headroom: **13,225,232 bytes**; development-target overrun: **11,774,768 bytes**. [Build provenance, source scope and SHA-256](BUILD-SIZE.md#football-animation-rebuild--4-october-2026).

The existing character mesh, skeleton, textures and imported clips are unchanged. One distance-driven gait, coordinated FK arms, contact IK and 28 editable control-space records replace the previous mixed animation clocks. The resource grows by **1,560 serialized bytes / approximately 280 independently compressed bytes**, to **13,108 / 1,854 bytes**. These estimates exclude compiled code and are not exact allocations within the APK's shared ZIP chunks.

The largest changed ZIP entries are IL2CPP **+9,778**, global metadata **+1,181**, and Burst **−1,530 bytes**. Fresh per-asset estimates retain the same leading art contributors: timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB**, and stadium **1.556 MB**. No mesh, texture, animation pack, audio or middleware was added. Shared locomotion clips remain needed by other sports; removing them would not be a safe size optimization.

Serialized assets total **151,510,277 bytes**, distinct from the actual compressed APK size. Current populated records are in `Builds/SizeAudit/latest/`, with a task copy in `Builds/FootballMotionPolishQA/FinalSizeAudit/`. The task folder retains `before.json`, `release.json`, `apk-entry-deltas.csv`, `apk-signature.txt` and source-hash verification. The 75 MB development target is still unmet; preserve the remaining reserve. See [verification](VERIFICATION.md#football-animation-rebuild--4-october-2026) for test scope and physical-device limits.

## Transparent lagoon and three fish sizes — 3 October 2026

Actual task APK: **86,574,958 → 86,757,976 bytes (+183,018)**. Hard-limit headroom: **13,242,024 bytes**; development-target overrun: **11,757,976 bytes**. Frozen-checkout AndroidSubmission and v2 signing passed. See [source/build scope and hash](BUILD-SIZE.md#transparent-lagoon-and-three-fish-sizes--3-october-2026). The concurrent root APK/audit was retained; current fishing evidence is under `Builds/FishingWaterQA/FinalSizeAudit/`.

The largest new fishing art estimates are lagoon surface **43,086**, shared fish **40,321**, sandy bed **19,892**, water shader **8,560**, fish shader **8,088**, and bed shader **5,540 bytes**. Including their materials, the group totals **125,801 independent DEFLATE-estimated bytes**. These estimates are not exact APK allocations because ZIP chunks share compression. Serialized packed assets total **151,508,417 bytes**. The authoring master stays outside the player; all three fish sizes share the two delivered meshes/material.

The final build reuses `RI_Sand_BaseColor.png`, already required by the terrain. `KeepSourceTexture` bypasses replacement in both mobile material preparation and application, with a case-insensitive tag comparison because Unity normalizes serialized boolean-looking tags. The packing report confirms `MobileMaterials/RI_Sand.png` is absent. The compact intermediate APK was **87,021,080 bytes**; final sharing removes **263,104 actual package bytes**. Earlier mesh simplification removed **80,758 bytes** without changing close/distant silhouettes. Reviewed obsolete generated bed material variants were archived only after checking scene/prefab references and the replacement path.

Combined baseline-to-final ZIP changes include IL2CPP **+34,172**, Burst **+6,703**, metadata **+4,741**, and the concurrent net shader **+9,348 bytes**. Fishing split-entry differences reflect both new art and changed chunk boundaries. Existing largest art contributors remain unchanged: timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB**, and stadium **1.556 MB**. No extra image texture, audio, imported animation or package was added for fishing. The remaining reserve is below the desired development margin; no phone performance qualification is claimed.

## Football character motion — 3 October 2026

The final signed AndroidSubmission APK measures **86,574,958 → 86,764,912 bytes (+189,954)**, leaving **13,235,088 bytes** below the strict ceiling and exceeding the working target by **11,764,912 bytes**. The comparison includes concurrent fishing and net changes. [Build provenance and hash](BUILD-SIZE.md#football-character-motion--3-october-2026).

The new 26-take resource is **11,548 serialized bytes / approximately 1,574 independently compressed bytes**. This estimate excludes its compiled runtime code and ZIP container interactions. Animation reuses the existing character, materials and run/jump/slide clips, with no imported mesh, texture, motion pack or middleware. Combined IL2CPP growth is **41,048 bytes**, metadata **4,734** and Burst **6,703**; these cannot be attributed solely to animation. The concurrent net shader adds a 9,348-byte ZIP entry. The largest remaining changed ZIP chunks are in fishing scene data.

The first successful package was 87,028,088 bytes. Rebuilding with the concurrent material-sharing correction reduced it by **263,176 bytes**, avoiding a duplicated fishing texture while retaining its shared source image. Fresh asset estimates remain led by timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No unexpected character-art growth was found. Serialized assets total **151,508,629 bytes**, not the compressed delivery size.

Current populated packing/ZIP records are in `Builds/SizeAudit/latest/`, with a preserved task copy at `Builds/FootballMotionQA/ReleaseSizeAudit/`. `release.json`, `apk-entry-deltas.csv`, `apk-signature.txt` and input manifests in the task folder record exact bytes, comparisons, v2 signing and an unchanged final build input set. [Verification](VERIFICATION.md#football-character-motion--3-october-2026) records the 1,807 checks, remaining contact tolerances and cleanup. Device quality/FPS and WAN remain unverified; 75 MB is still unmet.

## Position-aware goal net — 3 October 2026

Actual APK: **86,574,958 → 87,028,060 bytes (+453,102)**. Hard-limit headroom: **12,971,940 bytes**; development-target overrun: **12,028,060 bytes**. Main-project AndroidSubmission and v2 signing passed. See [release provenance, artifact and SHA-256](BUILD-SIZE.md#position-aware-goal-net--3-october-2026).

The net shader adds **9,348 actual ZIP bytes**, versus a **9,291-byte independent per-asset DEFLATE estimate** and **18,520 serialized bytes**. Its shared material is **112 serialized bytes**. Script records are serialization records, not compiled-code cost; the opt-in review implementation is excluded from release. The source stadium and its editable masters are preserved. The new net uses generated geometry and shader motion, without textures, clips or a cloth solver.

The combined snapshot also contains concurrent fishing and football-motion changes. Largest new art estimates: mobile sand **259,471**, lagoon surface **43,086**, fish **40,321**, lagoon bed **19,892 bytes**. IL2CPP grows **41,048**, metadata **4,734**, and Burst **6,703 compressed bytes**. Large shifts among `sharedassets4.assets.split*` entries reflect changed packed fishing content and chunk boundaries; individual split deltas are not per-asset sizes. The five largest existing art contributors remain unchanged. Serialized total: **151,858,317 bytes**.

Task baseline/release hashes, ZIP and asset deltas, full build logs, signature verification and populated packing offsets are retained in `Builds/FootballNetQA/`. The exact measured APK is retained there under `Android/`; root audit outputs may later be replaced by another build. The package remains above the 75 MB development target. No physical Android device was attached. See [verification](VERIFICATION.md#position-aware-goal-net--3-october-2026) for functional/render coverage and archived iterations.

## Interactive football boundary and twin-rail aim — 3 October 2026

Measured APK: **86,498,912 → 86,574,958 bytes (+76,046)**. Headroom: **13,425,042 bytes** below the strict ceiling; development-target overrun: **11,574,958 bytes**. The baseline is the actual older local package measured before editing, not the newer historical release documented below. The difference includes pulled code changes as well as these effects. The main-project AndroidSubmission build and APK v2 signing check passed. See [release provenance and SHA-256](BUILD-SIZE.md#interactive-football-boundary-and-twin-rail-aim--3-october-2026).

Current packing/ZIP records and per-asset estimates are in `Builds/SizeAudit/latest/`. Serialized total: **151,235,033 bytes**, not the APK allocation. New standalone shader ZIP entries cost **7,604 bytes** (boundary) and **7,056 bytes** (charge arrow), including their entry content overhead. Independent per-asset DEFLATE estimates are **7,549 / 7,002 bytes** respectively and should not be substituted for those actual entries. The wall material is 260 serialized bytes; the updated aim material is 160. Each new runtime/review script record is 112 serialized bytes, which does not measure compiled native code; the review implementation is excluded from release.

Actual ZIP growth is primarily IL2CPP **+50,982**, metadata **+11,757**, new shaders **+14,660**, Unity native code **+1,413**, offset partly by Burst **−5,225 bytes**. Existing art remains led by timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. No texture, source model, audio, animation clip or package dependency was added. Procedural geometry is created once and reused; animation needs no particle system, bloom, light or additional render pass. Preserve the remaining reserve; 75 MB is still unmet.

Baseline/release bytes and hashes, ZIP deltas, signature result, final Windows/Android logs and previews are under `Builds/FootballFeedbackQA/`; detailed [verification and cleanup](VERIFICATION.md#interactive-football-boundary-and-twin-rail-aim--3-october-2026) cover 559 checks. No phone was attached, so device rendering/FPS/touch remain unverified. Earlier sections below retain their historical source/build scope.

## GitHub integration release — 3 October 2026

The fresh combined release measures **86,532,484 → 86,563,288 APK bytes (+30,804)**, leaving **13,436,712 bytes** below the strict ceiling and exceeding the development target by **11,563,288 bytes**. The main-project AndroidSubmission attempt exited with the user's editor running; the matching-source isolated clean-cache AndroidSubmission build passed, supplied populated packing offsets and passed v2 signature verification. See [the release provenance](BUILD-SIZE.md#github-integration-release--3-october-2026).

Complete current reports are in `Builds/SizeAudit/latest/`. Serialized assets are **151,213,873 bytes (+376)**; this is not the compressed APK allocation. Largest art estimates remain timber **2.247 MB**, character albedo **1.993 MB**, golf structures **1.981 MB**, character mesh **1.574 MB** and football stadium **1.556 MB**. Entry comparison chiefly attributes growth to IL2CPP **+25,950 bytes**, metadata **+2,906**, Burst **+851** and Unity native code **+640**. The largest individual art-estimate change is **+493 bytes** for the existing golf LOD0. Two basketball MonoScript records add 208 serialized bytes; neither represents the full compiled-code cost. No new imported art/package or unexpected art growth appeared. The existing 4,220-byte mobile dither and two tiny SSAO script records remain; no desktop SSAO shader/texture collection was introduced.

ZIP/asset deltas, source hashes, signature results and the 1,061-assertion Windows/loopback summary are retained in `Builds/GitMergeQA/`. Pre-merge artifacts and completed merge backups were archived with recovery hashes under `Legacy/20261003-150444-050-git-merge-baseline/` and `Legacy/20261003-151034-738-git-merge-cleanup/`. Phone quality/FPS, physical touch and WAN latency remain unverified. Earlier audit records below retain their original source/build scope.

## Earlier release audits

The 3 October legacy-state cleanup release measures **86,530,508 → 86,532,484 bytes (+1,976)**, with **13,467,516 bytes** hard-limit headroom and a **11,532,484-byte** development-target overrun. The prescribed root AndroidSubmission build succeeded; a matching-source isolated clean-cache build supplied the complete current packing report after the root incremental report omitted offsets. The removed state adds no delivered assets or dependencies. Serialized assets remain 151,213,497 bytes. Fresh art estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. ZIP growth is chiefly IL2CPP +2,006 bytes, offset by metadata -28 bytes and tiny build-record changes. Regenerated Performance Test resource names keep their compressed sizes; no art growth was found. Current reports are in `Builds/SizeAudit/latest/`, evidence is in `Builds/FootballLegacyCleanupQA/`, and the recovery batch is `Legacy/20261003-143827-669-football-legacy-state/`. See [the measured release and validation scope](BUILD-SIZE.md#football-legacy-state-cleanup--3-october-2026).

Team A lettering adds one small runtime vector UI mesh using the existing Canvas material, with no imported art/font/shader/package. Task baseline: **86,529,684 bytes**; retained concurrent snapshot: **86,527,020 bytes (−2,664)**; newer current combined-source APK: **86,530,508 bytes (+824 against baseline)**, leaving **13,469,492 bytes** below the hard limit and exceeding the working target by **11,530,508 bytes**. Both measured APK signatures passed. The final wordmark/HUD hashes match the newer build. Fresh packing and ZIP inspection found unchanged main art estimates and no unexpected large asset growth; differences include other code revisions. The 112-byte serialized wordmark script record is about 88 independently compressed bytes, separate from native code cost. See [the detailed measurement and provenance](BUILD-SIZE.md#team-a-lettering--3-october-2026). Current evidence: `Builds/FootballTeamAWordmarkQA/`; current release audit remains `Builds/SizeAudit/latest/`.

## Goal-net collision release — 3 October 2026

Actual APK: **86,529,684 → 86,530,508 bytes (+824)** across the combined current source, leaving **13,469,492 bytes** below the strict ceiling and **11,530,508 bytes** above the development target. AndroidSubmission, populated clean-cache packing and v2 signing passed. See [the measured release and hash](BUILD-SIZE.md#goal-net-collision-release--3-october-2026); earlier pending records below remain historical. Current ZIP/asset evidence is in `Builds/SizeAudit/latest/` and `Builds/FootballGoalNetQA/`.

Eight small convex net panels are generated once at runtime: **64 vertices/96 triangles total**, with no new imported art or shader/package dependency. Art estimates remain timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. The goal-net MonoScript contributes 96 serialized bytes (about 82 independently compressed), separate from native code/metadata. ZIP changes principally comprise Unity native code -19,145 bytes, IL2CPP +15,599, metadata +5,050 and Burst -851; the comparison includes retained earlier source changes and is not an isolated net-only delta. Small Performance Test resource GUID/name changes do not add asset content. No desktop SSAO shader/texture collection or unexpected large art growth was found. The existing single mobile dither texture remains 4,220 serialized bytes. The 75 MB milestone and physical-phone qualification remain outstanding.

The 3 October 1.8x charging-arrow update changes one runtime longitudinal scale endpoint and matching verification. It adds no geometry, assets, shaders or packages. Baseline APK: **86,529,684 bytes**, headroom **13,470,316 bytes**, development target overrun **11,529,684 bytes**. Windows/AndroidSubmission attempts exited while Unity had the project open, so after-size/delta/headroom and actual rendering are **unverified**. Evidence: `Builds/FootballAim18xQA/`; latest packing estimates remain those of the earlier release. See [the pending build record](BUILD-SIZE.md#kick-arrow-charge-growth-18x--build-pending-3-october-2026).

The 3 October shorter-head follow-up increases corner rounding and compresses only the triangular head region to 80% of its prior length. It retains the 50-vertex/47-triangle runtime mesh, existing material and all shipped assets/packages. Baseline APK: **86,529,684 bytes**, with **13,470,316 bytes** hard-limit headroom; development target overrun is **11,529,684 bytes**. Fresh Windows/AndroidSubmission builds exited while the project was open, so after-size/delta/headroom and actual visuals are **unverified**. Geometry checks passed against the extracted generator with Unity API stubs. Current evidence is in `Builds/FootballShortHeadAimQA/`; the latest asset estimates belong to the earlier build. See [the pending build record](BUILD-SIZE.md#rounder-arrow-with-shorter-head--build-pending-3-october-2026).

The 3 October rounding update adds curved geometry generated once at runtime (50 vertices, 47 triangles), sharing the same material/shader and adding no imported assets or packages. Baseline APK: 86,529,684 bytes, with 13,470,316 bytes below the hard limit; the 75 MB target remains unmet. Geometry-only checks passed, but fresh Windows/AndroidSubmission attempts exited while Unity had the project open. After-size/delta, rendering and phone qualification remain **unverified**. `Builds/FootballRoundedAimQA/` retains evidence; earlier packing reports do not validate this source. See [the pending build record](BUILD-SIZE.md#rounded-charging-arrow--build-pending-3-october-2026).

The latest arrow-width change adjusts only transverse runtime scale to 80% (0.85 to 0.68 m); no asset/shader/package was added or modified. Baseline APK is 86,529,684 bytes (13,470,316 bytes below the hard limit). AndroidSubmission exited while Unity had the project open, so this source's after-size/delta and rendering remain **unverified**. Logs: `Builds/FootballAimWidthQA/`. Earlier size-audit entries do not validate this edit. See [the pending build record](BUILD-SIZE.md#solid-arrow-width--build-pending-2-october-2026).

## Attached-control snapshot audit — 2 October 2026

Actual APK: **86,505,374 → 86,529,684 bytes (+24,310)**; strict-limit headroom **13,470,316 bytes**, development-target overrun **11,529,684 bytes**. The isolated clean-cache AndroidSubmission build and v2 signature verification passed. The complete current packing report has **151,213,149 serialized bytes**, which must not be confused with the compressed APK size. `Builds/SizeAudit/latest/` contains populated packing offsets, actual ZIP entries and freshly computed asset compression estimates. `Builds/FootballControlQA/apk-entry-deltas.csv` compares the two measured APKs.

Main art estimates remain unchanged: timber 2.247 MB, character albedo 1.993 MB, golf structures 1.981 MB, character mesh 1.574 MB and football stadium 1.556 MB. The new Resources aim material is 1,760 serialized bytes, approximately 697 compressed bytes, with no imported texture or new shader family. ZIP growth is chiefly `libunity.so` +20,458 bytes and `libil2cpp.so` +4,496; generated Burst code decreases by 968 and metadata increases by 654. Other serialization/ZIP changes account for the remainder. No avoidable large art addition or desktop SSAO shader/noise collection was found. The existing single mobile pipeline dither texture contributes 4,220 serialized bytes. Tiny serialized SSAO C# type records are not the desktop SSAO shader or texture collection.

This snapshot covers attached possession and the 25%/0.65 m tuning, shared jump/tackle/whiff updates, visible restart reset and the transparent 0.8–1.44 m outline aim. **Subsequent concurrent solid-arrow and kickoff-formation edits are not included; current complete-working-tree APK freshness and their size delta remain unverified.** The older pending notes below remain historical attempt records for the corresponding source. See [the measured build/hash and scope](BUILD-SIZE.md#validated-attached-control-snapshot--2-october-2026). The 75 MB target remains an outstanding milestone; no unrelated visual-quality reduction was made. Phone quality/FPS and WAN behavior are unverified.

## Earlier concurrent attempt records

The solid arrow replaces the outline with a seven-vertex/three-triangle runtime mesh and reuses the existing transparent material. No new imported art or shader/package dependency was introduced. Baseline APK: **86,529,668 bytes**, **13,470,332 bytes** of hard-limit headroom; the 75 MB working target is exceeded by 11,529,668 bytes. Both new builds exited while Unity had the project open, so after-size, delta and source-specific headroom remain **unverified**. `Builds/FootballSolidAimQA/` retains baseline hash, earlier audit snapshot, source geometry check and build failure logs. The latest contribution report is from the earlier APK and cannot validate this source. See [the pending build record](BUILD-SIZE.md#solid-charging-arrow--build-pending-2-october-2026).

Fixed kickoff formations add fifteen explicit slot vectors to the existing match rules resource and authority-only team-slot allocation/pose validation. They preserve the ball reset anchor and all art/field geometry; no shipped dependency was introduced. The pre-change APK measured **86,529,668 bytes** with **13,470,332 bytes** hard-limit headroom, still **11,529,668 bytes** above the development target. Fresh main-project Windows and AndroidSubmission builds exited while Unity had the project open. Post-change APK bytes, delta and headroom are **unverified**; the existing APK is only a baseline. Latest contribution estimates are empty and no fresh contributor report exists for this change. Evidence is in `Builds/FootballFormationQA/`. See [the pending build record](BUILD-SIZE.md#fixed-football-kickoff-formations--build-pending-2-october-2026).

The latest aim-length correction changes only source/regression expectations to **0.8–1.44 m** (1.8 times the shortened base). No asset, shader or package change was made. Fresh APK size, delta and headroom remain **unverified**: the baseline APK was absent and new Windows/AndroidSubmission attempts exited while Unity had the project open. Current logs: `Builds/FootballKickAimLengthQA/`. A separately supplied APK now measures 86,529,668 bytes (13,470,332 bytes of headroom), but inclusion of this correction remains unverified. Its latest isolated-build audit has an empty asset-contribution CSV. See [the pending build record](BUILD-SIZE.md#kick-aim-length-clarification--build-pending-2-october-2026).

The transparent, charge-scaled aim adds a texture-free Resources material using the existing URP glass shader/keywords. The source material is 4,017 bytes; this is **not** its measured packed/APK size. Actual APK bytes, delta, headroom and shader-variant contribution remain **unverified**: no baseline APK existed and both fresh build attempts exited while Unity had the project open. `Builds/FootballKickAimChargeQA/` retains failure logs; the latest audit has no new contributor report. See [the current pending build record](BUILD-SIZE.md#charge-scaled-transparent-kick-aim--build-pending-2-october-2026). Actual Unity transparency, phone appearance/FPS and runtime checks remain pending.

The charging kick aim uses the existing ball shader, one runtime material and a five-point LineRenderer; no imported art, texture or package was added. APK bytes, delta, headroom and actual shader contribution remain **unverified** because no baseline APK existed and both fresh build attempts were blocked by the open Unity project. The latest audit has only the failure log; `Builds/FootballKickAimQA/` retains this task's logs. See [the pending build record](BUILD-SIZE.md#charging-kick-aim--build-pending-2-october-2026). Phone appearance/FPS and Unity visual execution are also unverified.

Closer-control tuning changes only the ball anchor (0.9 to 0.65 m) and possession speed multiplier (0.85 to 0.75), plus matching source defaults/checks. No model, texture, audio, animation or package was added. The pre-change APK was absent and new Windows/AndroidSubmission attempts exited while this Unity project was open. APK bytes, delta, headroom and contributor growth are **unverified**; the latest audit has only the failed log. Evidence is in `Builds/FootballControlTuningQA/`. See [the pending build record](BUILD-SIZE.md#football-closer-control-tuning--build-pending-2-october-2026).

The missed-slide penalty adds only gameplay logic: 0.8 seconds at 40% movement speed after a slide hits neither player nor ball. No shipped art, audio, animation or package was added. The pre-change APK was absent, and new Windows/AndroidSubmission attempts were blocked by the open Unity project. APK bytes, delta, headroom and contributor growth remain **unverified**. Failure logs and logic evidence are in `Builds/FootballWhiffQA/`; the latest audit has only the failed build log. See [the pending size record](BUILD-SIZE.md#football-missed-slide-penalty--build-pending-2-october-2026).

The slide ball launch follow-up reuses existing ball art and introduces one tuning field, with no additional asset/package dependencies. Its APK size is **unverified**: no pre-change APK existed, and both fresh builds exited while the project was open. `Builds/FootballBallLaunchQA/` retains failure logs; the latest audit has no new contributor report. See [the current pending-build record](BUILD-SIZE.md#slide-ball-launch--build-pending-2-october-2026).

Football jump restoration and tackle recovery changes are currently **size-unverified**. No pre-change APK was present, and the new Windows and AndroidSubmission attempts exited with code 1 while the project was open in Unity. No model, texture, animation or package was added or removed. Failed build logs are in `Builds/FootballTackleQA/`; the latest audit contains the failed attempt log, with no new contributor report. The measured figures below predate this code change. See [the pending-build record](BUILD-SIZE.md#football-jump-and-tackle-recovery--build-pending-2-october-2026).

Current APK measures **86,505,374 bytes**, **+13,844 bytes** against the preserved 86,491,530-byte match release, including the pending glass scoreboard and current flow update. It leaves **13,494,626 bytes** below the strict hard limit, but exceeds the development target by **11,505,374 bytes**. Native editor AndroidSubmission and v2 signature verification passed after the prescribed batch script exited on the open-project lock. See [BUILD-SIZE.md](BUILD-SIZE.md) for hash and build details.

The current detailed packing/ZIP comparison in `Builds/SizeAudit/latest/` and `Builds/FootballFlowQA/` found unchanged main art contributors and no added model, imported texture, animation or package. Growth is chiefly IL2CPP (+11,665 compressed bytes) and metadata (+2,157); procedural HUD textures are generated at runtime. The 72-byte serialized rules resource remains compact. Preserve the mobile pipeline and existing art packing. The working target remains an outstanding shared optimization milestone; this small gameplay revision does not justify reducing unrelated art quality. Windows flow/physics/network checks passed, but phone appearance/FPS and WAN latency remain unverified.

## Earlier football match release — 2 October 2026

The current combined-source match APK is **86,491,530 bytes**, leaving **13,508,470 bytes** below the hard limit; the 75 MB target remains unmet by **11,491,530 bytes**. No current pre-task APK was present, so the isolated match-size delta is unverified. The native editor AndroidSubmission build and APK v2 signature passed. Actual ZIP entries, populated packing offsets and per-asset compression estimates are in `Builds/SizeAudit/latest/`; see [BUILD-SIZE.md](BUILD-SIZE.md) for hash, contributor sizes and the separate historical baseline. Match rules add no art or packages and only one 72-byte serialized resource. Phone performance remains unverified.

## Historical upstream release — 1 October 2026

The upstream record measured 85,970,822 bytes. It predates the combined local football and match implementation; the following historical local and upstream records do not describe the new APK.

## Upstream basketball animation snapshot — 3 October 2026

The current combined football and basketball APK measures **86,498,912 bytes (86.50 MB / 82.49 MiB)**, leaving **13,501,088 bytes** to the strict 100,000,000-byte ceiling. It exceeds the 75 MB working target by **11,498,912 bytes**. The pre-merge basketball animation APK measured **85,993,642 bytes**; the measured increase is **505,270 bytes**. `AndroidSubmission` and APK v2 signature verification passed. Current SHA-256: `6EB19CFA79CF55571963DA49DFE687B6402E6F7A675A375B9C1503C9D0F91418`.

The packed-assets report adds a **550,636-byte uncompressed serialized football FBX**, two small football materials and script metadata. The source FBX is 1,078,236 bytes; neither figure is its exact contribution to the compressed APK. ZIP growth is concentrated in football scene asset splits: +170,672, +141,525 and a new 129,038-byte split; IL2CPP adds 20,834 bytes and managed metadata 3,679 bytes. Changes to other splits partly offset these amounts. No new texture or package dependency appeared. The large existing art contributors remain timber, character colour, golf structures, character geometry and stadiums. See `Builds/SizeAudit/latest/` for packed assets, APK entries and the successful build report.


The merged Windows player passed **367 checks** across football physics and host/guest replication, basketball motion and passing, and jump gameplay and replication. The imported FBX has no active image references, and its Blender master has no image or library dependencies. No physical Android phone or WAN test was performed. The pre-merge APK and size audit were archived locally in `Legacy/20261002-155631-852-football-merge-build-baseline/`.

The teammate's historical charge-kick APK measured **83,787,566 bytes** (+584 bytes against its goal-entry release), with **16,212,434 bytes** of hard-limit headroom. Earlier football builds measured **83,786,982** and **83,779,278 bytes**. These figures belong to a separate working tree; its asset audit, signature checks and visual quality findings are documented in [the football build history](BUILD-SIZE.md) and [football prototype](FOOTBALL-PROTOTYPE.md).

## Basketball animation release before merge — 2 October 2026

The earlier APK, now archived locally, was built on 2 October at 15:40:51 Malaysia time (07:40:51 UTC) with Unity 6000.3.20f1, release IL2CPP, ARM64, minimum Android API 26 and default Android ZIP compression. `AndroidSubmission` passed the size gate; APK v2 signature verification passed.

SHA-256: `5FD964709B5D449ED0508FDB56E287DAFA3FCD48A89540191B7FE71447BD12CF`.

## Basketball animation and passing

Measured before/after APK: **85,970,874 to 85,993,642 bytes (+22,768 bytes)**. The implementation reuses the character skeleton, both LODs, running/jumping clips, ball and all textures/materials. ZIP comparison attributes 19,490 added bytes to IL2CPP and 2,551 to managed metadata; native/archive bookkeeping accounts for the small balance. No large asset or package dependency was added. Unity retains 208 uncompressed serialized bytes of new MonoScript metadata, including a stub for the development-only review; the review implementation is compiled out of release.

The largest estimated contributors remain timber (2.247 MB), character colour (1.993 MB), golf structures (1.981 MB), character geometry (1.574 MB) and stadiums. The packed-asset comparison found no art growth. Per-asset DEFLATE estimates are approximate and do not sum exactly to APK ZIP chunks.

All **563 primary Windows player checks** passed: 220 motion checks at fixed 20/30/60/120 FPS, 95 gameplay/room checks, 53 ball-physics/replication checks, 140 jump/momentum/room regressions and 55 portable-path repeats. After correcting idle-pose equality for replication, the 56-check motion review and two-player regression passed again. The build also passed 65 editor motor checks. Close and side captures of both character LODs were inspected; dribble contact error stayed below 0.4 mm in these samples. AndroidSubmission and v2 signature verification passed. No phone was connected; Android appearance, touch ergonomics, FPS and WAN latency remain unverified.

Evidence is under `Builds/BasketballMotionQA/`: exact bytes and hashes, signature/build logs, before/final size audits, packed-asset and ZIP comparisons, per-asset estimates, fixed-rate results, portable evidence and the actual player video. Other suite locations and all five archive batches are recorded in [basketball animation](BASKETBALL-ANIMATION.md). The local-only batches contain superseded pose iterations, intermediate APKs/logs, the interrupted jump run and portable fixtures. No active art, authoring master or current build was archived. Pre-existing jump/momentum work was preserved; Git publishing remains with the user.

## Jump momentum

Measured before/after APK: **85,970,822 to 85,970,874 bytes (+52 bytes)**. No art or package dependency was added. The ZIP comparison shows IL2CPP growing by 1,430 bytes, Burst shrinking by 1,478 bytes, and small metadata/archive changes; the net APK difference is measured from the final file. Scene/art/shader data remains about 59.64 MB. The largest contributors are unchanged: timber, character maps/meshes/animation, golf structures/terrain and stadiums.

All **287 Windows player checks** passed: 140 jump/momentum/room, 24 tackle/room, 62 turning/room and 61 offline basketball gameplay checks. Another **65 editor motor checks** cover 16 headings at 20/30/60/120 FPS, release-distance consistency, bounded reversal, analog steering, reset, wall tangents and irregular-frame integration. Native player captures show forward and diagonal coasting plus steering after release. Fixed-rate captures do not establish device frame rate. Android submission and v2 signature verification passed; no phone was connected.

Evidence is under `Builds/MomentumQA/`: before/after APK bytes and hashes, build/signature logs, `SizeAuditBefore/`, `SizeAuditFinal/`, asset and ZIP comparisons, motor results, test summary and `momentum-review.mp4`. Player run locations are recorded in [jump verification](JUMP-ANIMATION.md). No assets or masters were changed, and no archive batch was needed for this update.

## Basketball gameplay revision

Measured before/after APK: **85,957,194 to 85,970,822 bytes (+13,628 bytes)**. No new mesh, texture, animation, audio or package dependency is introduced. The largest contributors remain timber (about 2.25 MB), character albedo (1.99 MB), golf structures (1.98 MB), character mesh (1.57 MB) and stadium geometry. Per-asset estimates use separate DEFLATE compression and do not sum exactly to APK ZIP chunks. Entry comparison attributes the small increase primarily to runtime code/metadata; the scene/art budget remains stable.

All **82 basketball gameplay checks**, **53 foundation checks** and **40 jump regression checks** passed on Windows. Gameplay checks include real 2-24 m rim crossings, repeated pickup after a rebound, ownership arbitration, host/guest shot input and holder disconnect/restart recovery. The portable copied-player fixture repeated all **61 offline checks** from a path with spaces and an unrelated working directory. No phone was connected; Android appearance, touch ergonomics, FPS and WAN latency remain unverified.

Current evidence is under `Builds/BasketballGameplayQA/`: APK bytes/hashes, signature, build logs, `SizeAuditBefore/`, `SizeAuditFinal/`, contribution/ZIP comparisons, gameplay captures and portability evidence. See [basketball gameplay](BASKETBALL-GAMEPLAY.md) for controls and tuning.

## Jump motion revision

Measured before/after APK: **87,153,874 → 85,957,194 bytes (−1,196,680 bytes)**. The pre-revision length was read directly before another build replaced that APK; its hash was not captured. Most of this shared-tree reduction comes from the concurrent fishing optimization below. The revised animation uses **37,124 serialized bytes**, approximately **24,231 compressed bytes**, compared with the original jump's 29,292 / 17,367 bytes. It adds no character meshes, textures or external dependencies.

The revised poses coordinate the arm backswing, leg extension at physical takeoff, relaxed flight and landing absorption. The importer now follows the full 61-frame source range. The motor presents an 80 ms grounded load immediately, keeps horizontal input active and shares preparation progress through protocol 11. All **126 Windows checks** passed: 40 jump/room, 24 tackle/room and 62 turning/room checks. Side, front and airborne-reversal motion captures, plus both LODs, were inspected. The fixed-rate capture is an animation review, not a performance benchmark.

Largest contributors remain timber, character maps/mesh/animation, golf structures/terrain and stadiums. The per-asset and ZIP-entry comparison found no unexpected large dependency growth. The signed Android submission build passed; no phone was connected. Phone appearance/FPS and WAN latency remain unverified.

Evidence is under `Builds/JumpNaturalQA/`: final build/signature logs, exact APK bytes/hash, `SizeAuditFinal/`, contribution estimates, portability audit and `jump-review.mp4`. Final jump checks use `Builds/JumpQA/Run-20261001-203848/`; tackle and turn use `Run-20261001-203321/` and `Run-20261001-203333/` in their QA folders. Superseded poses/captures and the temporary portable checkout were archived to `Legacy/20261001-204131-970-jump-motion-revision/`.

## Fishing rod addition

Measured before/after APK: **85,088,760 → 85,955,058 bytes (+866,298 bytes)**. This working-tree delta includes concurrent jump refinements: the jump clip's separate ZIP estimate changed from 17,367 to 22,223 bytes. Rod art totals **981,892 uncompressed serialized bytes** and approximately **852,462 compressed bytes**. Separate asset compression estimates are not exact allocations of shared APK chunks.

The first fishing build was **87,153,874 bytes**. Reducing imported colour maps from 1024 to 512 pixels and normals from 512 to 256, using ASTC 6x6 for both, and applying Low mesh packing reduced the rod estimate from about 2.06 MB to 0.85 MB. The final APK is **1,198,816 bytes smaller** than that first pass, despite concurrent jump changes. Source GLBs, editable masters and full-resolution maps are retained outside Unity Assets. Near LODs total 24,000 triangles and distant LODs 7,000; each rod shares one material across both. The rack reuses existing timber.

All **22 Windows rod checks** passed; close, reverse, guide, distance and pier views were inspected after the final lossy import changes. The reduced maps soften extreme close detail; source reconstruction flaws remain documented in [the fishing report](FISHING-RODS.md). Largest APK contributors remain timber, character maps/mesh/animation, golf structures/terrain and stadiums. The comparison found no unexpected large dependency growth.

Evidence under `Builds/FishingRods/20261001/` includes exact APK measurements/hashes, signature verification, `SizeAuditBefore/`, `SizeAuditFinal/`, asset and ZIP-entry comparisons, portability checks and `Player-Compact/` captures. Android submission and v2 signature passed; no phone was connected. These Windows checks do not qualify Android ASTC appearance or phone performance.

## Jump and shared golf-equipment build

Measured before/after APK: **83,751,052 → 85,088,760 bytes (+1,337,708 bytes)**. The working tree also gained the separate golf-equipment integration, so this combined delta must not be attributed entirely to jumping. The new animation-only FBX shares the existing character rig, mesh and textures: **29,292 uncompressed serialized bytes**, with a **17,367-byte separate DEFLATE estimate**. Golf equipment contributes approximately **1,314,630 bytes** by the same estimator. Runtime/metadata, scene packing and ZIP chunk boundaries account for the remaining difference; per-asset estimates are not exact allocations of the APK.

The first build measured **85,817,216 bytes**. Inspection found world regeneration had reset the previously approved high compression on distant island proxies. Restoring that packing recovered **728,456 actual APK bytes**. The Sky-Sail generator now packs distant silhouettes at High and near cabin/station modules at Low; the Android release path also repairs missing packing. This restores existing delivery settings, retaining the original source geometry and authoring masters. Largest contributors remain timber, character colour/mesh/animation, golf structures/terrain and stadiums. No new jump mesh or texture ships.

All **126 Windows checks** passed: 40 jump/room, 24 tackle/room and 62 turning/room checks, including host/guest action replication, capsule wall/ceiling collision, input reversals at 20/30/60/120 FPS, both animation LODs, and jump controls on all four islands. Both LOD jump/recovery captures were inspected. Signature verification passed after the final Android build; no phone was connected. These checks do not qualify phone frame rate, Android visual quality or WAN latency.

That checkpoint's evidence is in `Builds/JumpQA/`, including before/after bytes/hashes, signed build log, per-asset estimates, portability audit and `Run-20261001-195405/`. Tackle and turn reports use the same run suffix in their respective QA directories. See [jump authoring and controls](JUMP-ANIMATION.md). Superseded fixtures and the temporary portable checkout were archived in `Legacy/20261001-195347-370-jump-validation/`; retained evidence remains under Builds.

## Golf equipment validation

The ball and three clubs use eight closed mesh LODs, four materials and ten maps. Highest-detail geometry totals **15,264 triangles**, dropping to **4,320** at distance. Clubs use 512-pixel colour/normal maps and 256-pixel metal/smoothness masks; the ball uses a 1024×512 dimple normal. ASTC imports, derived meshes and shared rack timber keep original downloads and full-resolution masters outside the APK.

Golf art totals **2,870,024 uncompressed serialized bytes** and approximately **1,314,630 bytes** by separate per-asset DEFLATE estimates, including 596,010 for the ball normal. These estimates cannot exactly allocate shared APK chunks. All **20 equipment checks**, **126 golf-island checks** and the local-room suite passed. Seven actual player views were inspected, and the final generator passed from a checkout with spaces and an unrelated working directory. Oversized irregular dimples, soft face grooves and uneven grip accents remain visible source limitations.

See [the golf equipment report](GOLF-EQUIPMENT.md). Retained evidence under `Builds/GolfEquipment/20261001/` includes a final `SizeAudit/` snapshot, APK measurements, entry deltas, asset estimates, signature, portable dependency checks and actual Windows captures. No phone was connected; Windows views do not qualify Android appearance or performance.

## Basketball addition (earlier checkpoint)

The APK increased from **83,282,808 to 83,751,052 bytes: +468,244 bytes**. The ball uses two mesh LODs (2,976 / 720 triangles), a single shared material and two 1024×512 ASTC 6×6 maps. The original 9.09 MB GLB and high-resolution native source stay outside Unity Assets. Ball art totals **720,260 uncompressed serialized bytes**, which is a different measurement from the APK increase.

Per-asset ZIP estimates are 234,658 bytes for the normal map, 172,805 for colour, 35,436 for the FBX and 701 for the two materials. These estimates total 443,600 bytes; separate DEFLATE estimates cannot exactly allocate the APK's compressed chunks. The balance includes physics proxies, code and packing changes. Largest contributors remain timber, character maps/animations, golf structures/terrain and stadiums. No unexpected large dependency was added. Unity refreshed its Android player resource list, omitting unused SSAO resources while retaining the authoring entries.

All **53 Windows basketball checks** passed, and close/reverse/court-distance views were inspected. Both hoop collisions, 35 m/s floor/backboard shots, spin, streaming and host/guest authority were exercised. No phone was connected; Android appearance, networking and performance remain unqualified. See [the implementation report](BASKETBALL-BALL.md).

Basketball checkpoint evidence: `Builds/BasketballModel/20261001/` (`apk-before.json`, `apk-after.json`, `apk-entry-delta.csv`, `asset-zip-estimates.csv`, `apk-signature.txt`, `adb-devices.txt`). These generated reports stay ignored.

## September optimization history

The 30 September portable-path validation rebuild was **4 bytes larger** than the 29 September APK. Only asset path strings and integrity hashes changed; geometry and texture content were checked unchanged. The earlier mobile asset pass reduced the 350.25 MB cleanup checkpoint by **266.97 MB (76.22%)** before the basketball addition below.

## Measured stages

These are actual APK sizes, not source-folder sizes or uncompressed asset estimates.

| Stage | APK MB |
| --- | ---: |
| Before dependency cleanup | 354.31 |
| Dependency cleanup and lossless terrain-channel packing | 350.25 |
| Android texture settings and mesh packing | 218.19 |
| Shared vegetation and regenerated golf/fishing grass | 128.31 |
| Tighter mesh packing, code stripping and ZIP delivery | 103.03 |
| Shared material patterns | 99.27 |
| Reduced render meshes and texture-alias cleanup | 89.50 |
| Regenerated coastal meadow, compact seating and materials | 83.49 |
| GPU instance batches and final build | **83.28** |
| Basketball model and physics foundation, 1 October | **83.75** |
| Jump and shared golf-equipment working tree, 1 October | **85.09** |
| Fishing rods and concurrent jump refinements, 1 October | **85.96** |
| Revised jump motion and takeoff alignment, 1 October | **85.96** |

An intermediate experiment retained duplicate texture references and was larger. The final material builder updates URP's hidden `_MainTex` alias along with `_BaseMap`, preventing that duplication.

## What changed

- **Vegetation:** 76 coastal palms and all 1,292 palm/plant/tuft placements use nine shared modules and 27 LOD meshes. Near flowers retain their source shape; palm leaflets use fewer longitudinal segments. Explicit GPU instance batches avoid thousands of individual renderer submissions.
- **Grass:** three authored patches are stored once. Golf/fishing blades follow the original analytic terrain; coastal meadows use recorded positions and blade heights. Math runs on a worker and Unity mesh uploads span frames. The room waits for preparation before becoming ready. Tiny blades no longer cast individual shadows; larger objects retain shadows. Complete blades are trimmed at golf bunker boundaries.
- **Buildings:** 387 selected render meshes in delivered coastal scenes use simplified copies. Every source vertex in an accepted simplification was checked against the reduced surface, with a maximum distance of 2.5 cm before Unity packing. This is a source-vertex check, not a bound on every surface point. Meshes shared with colliders retain their original geometry. Another 160 small seating meshes use tighter packing; no seats were removed. High mesh compression is restricted to those seating groups and distant proxies.
- **Textures:** Android uses ASTC and appropriately sized maps. Character colour and cabin timber retain 2K maps; general shared patterns use 1K, normals 512/1K, and masks generally 256. Terrain colour stays 2K; terrain controls retain exact 4K R8 data and mipmaps. Water-depth maps use 2K ASTC 4x4.
- **Materials:** 24 colours use 11 shared patterns and deduplicated normal maps. Tint conversion was compared against every source RGB pixel: maximum error below 2/255 before resizing/GPU compression. Final compressed textures are not claimed to be pixel-identical.
- **Package/code:** Medium managed stripping, IL2CPP size-oriented code generation, ARM64-only delivery and default Android ZIP compression. Code generation and ZIP loading can trade speed for size; device timing remains necessary.

The cabin, doors, hanger, towers and cables were not geometrically simplified by the building pass. Modular authoring structure remains intact. Original FBX/PNG assets and Blender masters remain available; Android substitutes derived delivery assets in build copies of scenes. Source hashes guard against stale model and colour derivatives.

The earlier removal of Resources dependencies, desktop rendering dependencies and retired sea/cloud groups is recorded in [the historical cleanup report](APK-SIZE-CLEANUP-2026-09-29.md).

## Where space remains

The current ZIP contains **59.64 MB** of Unity scene/art/shader data and **20.80 MB** of native libraries. Managed metadata, Android code/resources and archive overhead account for the remaining **5.55 MB**. Exact per-entry values are in `Builds/SizeAudit/latest/apk-entries.csv`.

Large individual art contributors now include 2K timber and character colour maps, golf structures/terrain, character mesh/animation data and stadium geometry. Baked coastal vegetation and repeated grass no longer dominate. Derived building/seating meshes are distributed across many asset entries.

`packed-assets.csv` reports **uncompressed serialized** bytes. For example, two 4K terrain-control maps total about 44.74 MB serialized but roughly 1.8 MB in a per-asset ZIP estimate. These are not RAM measurements and must not be added to the APK size.

`Tools/Build/analyze_packed_apk.py` estimates individual ZIP contributions using packing offsets. Separate per-asset DEFLATE differs from the APK's chunk compression; actual APK length is authoritative. The mesh inventory includes all submeshes of each contributing source FBX, including unused source submeshes. It is not a simultaneous draw count.

## Validation and limits

- Signed Android submission build succeeded below 100,000,000 bytes.
- Runtime terrain math matched **13,122 independent Blender reference samples**, maximum error approximately `1.78e-15` metres.
- The final optimized Windows circuit passed **60 checks**, covering four islands, scene release, docking collision, restored walking, prepared grass and bounded instance submissions. A separate host/guest run passed **18 checks** for synchronized rides, shared destinations and everyone disembarking together.
- All **34 walking-review checks** passed, including every shore-to-deck route and return. The circuit captured 51 screenshots. Close views of gardens, timber, course surfaces, stations and cabin were inspected. Instanced vegetation was visually checked after correcting the manual camera's capture order; sampled coastal views used **89 vegetation draw submissions**.
- The preview uses Android delivery geometry with the Windows renderer and desktop texture formats. It validates geometry, colours and travel logic, **not** Android ASTC appearance, IL2CPP networking, phone RAM/FPS, thermals or transition latency. No phone was connected (`adb devices` was empty).

Local evidence: `Builds/MobileOptimization/terrain-validation.txt`, `VerifiedTravelReview`, `FinalNetworkReview`, `WalkReview`, `InstancedCaptureReview`, logs and APK checkpoints. Builds/reports are excluded from Git. The reference fixture under `Tools/Blender/Fixtures/` supports fresh-clone math checks.

Housekeeping on 29 September retained the final evidence above and moved superseded checkpoints, intermediate logs/reviews and original-setting backups to `Legacy/20260929-201920-001-repository-hygiene/`, preserving their former workspace-relative paths. See [the recovery inventory and repository policy](REPOSITORY-HYGIENE.md). The current APK was not modified.

## Continuing development

1. Run `Tools/Build/Build.ps1 -Target AndroidRelease` after meaningful asset additions. Reports update automatically; the build warns above **75 MB**.
2. Use `-Target AndroidSubmission` for the hard **under-100-MB** check. Passing it does not reserve space for every planned feature.
3. Reuse shared materials, vegetation and prop modules. Keep high-resolution masters and make deliberate mobile exports. Avoid a new 2K/4K texture set for every small prop.
4. Treat **14.00 MB** as one shared reserve for animations, effects, props, audio and fixes. Leave several MB unallocated for integration growth. Reaching 75 MB requires further measured work; no additional saving is promised from untested changes.
5. Qualify the selected minimum phone before claiming stable 30 FPS or raising visual settings.

After editing art, run `build_mobile_vegetation.py` and `build_mobile_models.py` in Blender, `build_shared_mobile_materials.py` with Python/numpy/Pillow, then `Tools/Build/Build.ps1 -Target OptimizeAndroid`. This reapplies import settings and rebuilds the delivery libraries. `-Target MobilePreview` builds optimized geometry into `Builds/WindowsMobilePreview/`; ordinary Windows builds retain authoring scene assets.
