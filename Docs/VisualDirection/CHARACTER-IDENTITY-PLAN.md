# Ten-player character identity plan

Approved design, 14 October 2026. The implementation and its final design choices
are described in [character identity delivery notes](CHARACTER-IDENTITIES.md).
Measured releases and validation are recorded there through links to the current
build-size and verification records. Physical Android qualification remains a
separate device check.

## Accepted requirements

- Every participant, including bots, must have an individual identity that is
  obvious on the field or court. Distinct database values alone are insufficient.
- Football and basketball use **Blue** and **Red** team uniforms, up to 5v5.
  All five teammates share their dominant jersey, shorts and trim colors.
- Golf, fishing and non-match exploration retain personal appearance colors;
  they do not acquire blue/red teams from a previous sports match.
- Keep the existing character proportions, collision capsule, skeleton and
  animation library. Preserve the four islands, one active game per host room,
  and shared travel.
- The priority is gameplay recognition. A character-page redesign or full
  customization editor is not required for this first delivery.

## Recognition design

Each participant has three independently readable cues: a distinct head
silhouette, a permanent room number, and small personal accents. Team identity
controls the large clothing regions. Skin tone is personal appearance, never a
team signal or the only way to identify someone.

The first collection contains ten curated presets. The numbers below are room
identities, **not team assignments**: any five can play Blue or Red. Names are
working design labels, not replacements for player nicknames.

| Room number | Main silhouette | Secondary personal cue |
| --- | --- | --- |
| 01 | Existing broad swept quiff | Single wristband |
| 02 | Close rounded crop | Paired wristbands |
| 03 | Broad rounded curls | Narrow sock stripe |
| 04 | Tall flat-top hair | Wide sock band |
| 05 | Narrow central crest | Contrasting shoe heel |
| 06 | Low ponytail with clear rear projection | Shoe toe accent |
| 07 | Single high bun | Double wrist stripe |
| 08 | Two separated side puffs | Double sock stripe |
| 09 | Forward cap with clearly projecting brim | Small cap-side stripe |
| 10 | Bucket hat with a continuous brim | Hat band |

These are ten target silhouettes, not ten complete body models. Reuse scalp,
hair and headwear modules wherever it preserves their visible distinction.
All presets retain the approved stylized character's face/body quality. Hair
color and skin-tone palettes can vary, but the preset must remain recognizable
when its clothing and personal accents are shown in the same color.

Put large, high-contrast numbers on the back and a smaller number on the chest.
Use one shared digit atlas or generated glyph geometry; do not create ten full
jersey textures. Keep 06/09 unambiguous and never mirror the back number.

At stadium/court distances where hair and jersey digits become too small,
provide a compact screen-space number badge anchored above the player. Give
the local player an additional outline/arrow and YOU indicator. Use depth and
overlap handling so badges do not reveal players through buildings or cover
the ball, aim cues or essential HUD. Keep numbers readable in the smallest
supported viewport; adjust distance/overlap behavior from actual captures.
Do not promise distant recognition from hairstyle alone.

Blue and Red retain a simple secondary team cue, such as one versus two sleeve
bands, so color is not the only team signal. Personal accents must stay small
and must not resemble an opposing team's large uniform blocks. Existing team
labels, roster chips, score displays and applicable field markers should agree
with the same Blue/Red mapping; this does not recolor stadium decoration.

## Animation and art contract

Keep one shared rig definition and animation/controller library; every player
still owns independent transforms and playback state. Preserve bone names,
hierarchy, rest transforms, proportions, animation paths and root-motion policy.
Do not retarget nine independently generated skeletons in this phase.

The existing one-piece model needs a prepared delivery derivative:

1. Preserve the editable high-quality master and original exports. Separate
   replaceable hair without opening holes in the scalp or changing face detail.
2. Derive a shared neutral/recolorable clothing base and region mask. Recolor
   jersey/shorts independently of face, eyes, skin, hair, shoes and shading.
   A whole-character tint is not acceptable.
3. Author lightweight hair/headwear options on the existing head bone. Use rigid
   attachments where suitable; bind deforming pieces to the existing rest pose.
   No new hair simulation or animation library is required.
4. Retain both near/far body LODs and create appropriate accessory LODs. Keep
   distinctive head outlines at distance and remove hidden original hair.
5. Preserve `GolfRightGrip` on both body LODs and the `Golf grip socket`. Use
   format-aware mesh extraction/shape transfer with verified vertex mapping;
   simply exporting a replacement body can discard the grip deformation.
6. Extend first-person visibility handling to every new renderer. The current
   athlete cache gathers skinned renderers, so rigid hats must not be left
   floating in front of the camera. Check fishing's head-hiding behavior too.

Football, basketball, golf and fishing perform additional bone-based motion.
Test contact and equipment alignment even when the imported clips still play.
Body size, hand size, reach, physics radius and sports timing remain unchanged.

Keep master texture resolution and editable assets. Derive mobile masks/maps,
share textures/materials, and control shader variants. Updating one player's
color must not mutate every renderer's shared material. Choose the material
parameter implementation after checking the project's URP rendering costs.
Shared asset storage is not a promise that skinned characters render in one draw.

## Authoritative identity and team state

- The host reserves one of ten preset IDs for every active participant, humans
  and bots together. No duplicate preset or room number is allowed.
- Maintain a room-scoped participant-to-identity registry. Do not derive identity
  from client ID modulo ten: bots can share host ownership and IDs can have gaps.
- A preferred local preset is a preference. If occupied, the host assigns an
  unused preset deterministically and publishes the resolved identity.
- Keep that identity through movement, goals, baskets, restarts, rematches,
  team changes, island travel and any presentation-only replay. Do not recolor or
  renumber a player according to roster array position.
- Actual departure releases that participant's reservation without reshuffling
  anyone else. Respect the existing match-void and room-join restrictions. This
  feature does not add host migration or mid-match replacement players.
- Team selection changes only the uniform layer. Team A maps to Blue and Team B
  to Red in both sports. Use actual authoritative membership, not camera side,
  local/remote status or player number. A teamless player keeps personal colors.
- Resolve the uniform from the active sport/match state. Clear the override on
  match cancellation/exit and travel to non-team activities, including when
  previous football/basketball membership remains in another snapshot.
- Replicate small validated preset/identity fields and use existing team state.
  Clients receive initial state and later updates; do not send meshes or frame
  images over the network. Appearance requests must be owner-authorized and
  cannot claim another participant's reserved identity.
- Version the appearance schema and advance the current network protocol if
  serialization changes. Migrate the old four-field saved profile explicitly.
  Use the actual protocol at implementation time; concurrent features can change it.

Minimal UI work is limited to replacing obsolete claims that everyone shares
one look, displaying the resolved number where needed, and keeping team labels
consistent. Automatic unique assignment works without a new selection page.

## Storage baseline and budget

The inspected APK from 14 October, 18:54:34 Malaysia time is **73,446,301 bytes**,
with **1,553,699 bytes** below the 75,000,000-byte development target and
**26,553,699 bytes** to the 100,000,000-byte boundary. Submission size must remain
strictly below that boundary. See [measured releases](../BUILD-SIZE.md) and the
[packing audit](../APK-SIZE-AUDIT.md).

Its direct character assets total **6,283,868 serialized bytes**: two model LODs,
three textures, nine imported animation clips, the football pose library,
material, controller and two prefabs. The nine clips total **435,636 bytes**.
An isolated LZMA compression test of the direct assets gives **4,016,022 bytes**;
this is an estimate, not their exact share of the compressed APK. Compiled
procedural code, shared engine/shader dependencies, equipment and audio are
outside that direct asset subtotal. Evidence is retained locally in
`Builds/CharacterStorageAudit/`, with paths based at the repository root.

Aim for **no more than 500,000 additional actual APK bytes for the first complete
identity delivery**, including masks, ten presets, parts, numbers and code. This
is an engineering target, not a measured promise. Against the inspected APK it
would leave 1,053,699 bytes under the development target. Re-measure before work:
concurrent gameplay changes are not validated by this historical baseline.

Measure an early two-preset build, then all ten. If growth exceeds the target,
inspect contributors and reduce avoidable texture/mesh/shader duplication while
preserving the recognition requirements. Do not silently spend the hard-limit
reserve, claim an incomplete recolor-only pass solves the problem, or reduce
unrelated island/cabin quality to make room.

## Implementation sequence and exit criteria

1. **Prepare two contrasting presets on the current rig.** Keep one original
   quiff and one substantially different silhouette. Prove regional team
   recoloring, number placement, both LODs, retained grips and first-person hiding.
   Run their complete animation/contact checks before expanding the collection.
2. **Implement unique assignment and the two uniform layers.** Exercise human,
   bot and mixed rosters, conflicting preferences, initial synchronization,
   team changes and identity retention. Use the same resolver for solo bot play.
3. **Complete all ten presets and gameplay recognition.** Inspect every pair in
   matching Blue uniforms and again in matching Red uniforms. Add the compact
   distance badges/local indicator; do not rely on ten differently colored shirts.
4. **Validate real 5v5 presentation and existing gameplay.** Capture all ten at
   actual football and basketball camera settings, moving and stationary, front,
   rear and side, at near/mid/far distances. Verify each room number maps to one
   actor on host and guest. Check both five-player groups at the smallest supported
   phone viewport, crowding, gray/color-vision simulations, shadows and LOD changes.
   Every participant must remain identifiable from at least one visible cue.
5. **Measure and deliver.** Run relevant existing movement/room/sports tests,
   build AndroidSubmission, inspect the new size audit, and update the measured
   size/verification documents. Validate and promote the complete Windows player
   to `Builds/WindowsFinal/`, including `LATEST-BUILD.txt`. Preserve current evidence,
   archive superseded outputs with the repository tool, and run TaskReady.

Meaningful automated checks cover uniqueness/capacity, host authority, schema
migration, correct initial state, team override/reset, preserved rig/grip assets,
and inclusion of new renderers in visibility/LOD behavior. Existing turn, jump,
tackle, football motion, basketball motion, golf and fishing checks supply the
motion regression coverage; avoid tests that merely repeat preset constants.

Physical Android validation must check recognition, ASTC appearance, memory,
frame time and transitions with ten visible players. Windows captures establish
neither phone performance nor phone texture quality. If a phone is unavailable,
report that limit and keep Android qualification outstanding.

## Working-tree coordination

The planning inspection found ongoing football replay work in shared athlete,
network, camera, football and rendering files. Preserve it. Integrate changes
against the current source when implementation begins, including replay identity
and team appearance, and record the source snapshot included in each build.
Do not stage, commit or publish as part of this plan.
