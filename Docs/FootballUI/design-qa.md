# Football prototype design QA

**Proposal review: passed (11 October 2026).**

Unity implementation and release verification are now recorded in
[implementation.md](implementation.md). This page retains the original browser review.

Scope: browser prototype and visual continuity with the approved basketball
design. This is not a Unity, Android, physical-phone, networking or FPS result.

## Reference and comparison

- Source visual truth: [Basketball proposal](../BasketballUI/proposal.html),
  rendered in its Ready to play state. Source capture:
  `Builds/FootballUIProposal/basketball-source.png` (1280 × 720).
- Implementation: [Football proposal](proposal.html), On the ball state.
  `Builds/FootballUIProposal/ready-final.png` (1254 × 1004 browser viewport).
- Source game region: 900 × 506 CSS/pixel region at (190, 60).
  Football region: 1120 × 630 at (67, 104). Captures have one image pixel per
  reported CSS pixel. Both regions were normalized to 800 × 450 for comparison;
  this is a style comparison across different sports, not a pixel clone.
- Full-view paired evidence, opened together:
  `Builds/FootballUIProposal/style-comparison-final.png`.
- Focused action-button pair, also opened:
  `Builds/FootballUIProposal/button-comparison-final.png`.
- Phone browser viewport: 844 × 390; landscape stage 824 × 336. The final
  expanded phone guide is `Builds/FootballUIProposal/phone-controls-final.png`.
  The browser viewport override was reset after review.

## Findings and fixes

1. **P2, fixed — Cancel path entered the Kick illustration.** The initial
   dotted path reached the button centre, making it look like a slash across
   the ball. It now ends just outside the power ring. Compare `hold-kick.png`
   with `charge-final.png` in the evidence folder.
2. **P2, fixed — Phone menu text was too small.** Initially the 668-unit menu
   simply shrank with the pitch. Short landscape layouts now expand the panel
   horizontally, increase guide text and utility controls, and retain fixed
   header/body/footer regions. `phone-controls-final.png` confirms the copy is
   readable with a clear gap before the footer. The guide body ended at 293.70
   CSS px; the footer began at 308.65 in the checked 844 × 390 viewport.
3. **P2, fixed — Jump sat higher than the accepted button cluster.** The initial
   bottom inset of 205 was reduced to 169. Its new position is closer to Kick,
   retains separation from the power ring and matches the basketball group's
   reach more closely. The teammate hint moved left to keep that state clear.
   The final paired captures above and `defense-final.png` show the correction.
4. **P3, resolved — Camera and tackle motifs.** Stadium now uses the existing
   overhead-court illustration instead of a generic football. Tackle uses a
   shoe instead of a lightning bolt. Labels remain present in every action.

Post-fix comparison found no remaining actionable P0/P1/P2 visual issues.

## Required visual surfaces

| Surface | Result |
| --- | --- |
| Fonts / typography | Uses the exact existing CoveDisplay and CoveText files. Display labels, body copy and numerical power hierarchy are distinct; guide text does not overlap the footer. |
| Spacing / layout | Edge anchors remain stable in 16:9 and 20:9. Only relevant actions appear. Cancel uses a connected path, and release text remains attached to Kick. Phone settings use available width. |
| Colour / tokens | Cream/navy frames, gold primary action, sky defense and mint Dash follow basketball. Armed cancellation has both coral colour and explicit lift wording. |
| Images / assets | Existing game screenshot and utility vectors are reused; nine source-library icon hashes are verified. No generated stadium or baked HUD image is presented as implementation. |
| Copy / content | No desktop bindings or diagnostic HUD text. Football's power kick, stamina, pressure, team match and leave-role behavior are distinguished from basketball. |

The larger football score strip, attached stamina bar, absence of basketball
props and disabled Jump while carrying are intentional football differences.
The stationary carrier in the backdrop is illustrative in defense/loose-ball
states; it is not a live gameplay simulation.

## Interaction and integrity checks

- Browser held Kick for 650 ms: release feedback reported 66% power.
- Browser dragged from Kick into Cancel and released: “Kick cancelled · ball kept”.
- Cancel-armed preview shows lift-to-cancel and slide-back guidance. Code review
  confirms entry does not reset charge, release checks the original pointer,
  and moving out only clears the armed state. A native multi-touch return path
  still needs physical-device qualification during implementation.
- Tackle changes to a disabled cooldown and counts down from four seconds.
- All three camera choices, selected state, guide tabs and menu return work.
- Host, guest and offline leave copy was inspected separately; Host's final
  action reaches the room-preview end screen and Reset returns to the HUD.
- Opening Settings ends held gameplay input and makes the playing surface inert.
- JavaScript syntax check passed. Browser error/warning log checks returned no entries.
- Relative images/styles/scripts/fonts resolve; nine icon SHA-256 hashes match
  their vendored originals. The game backdrop matches its original bytes.
- A copied review tree under `Builds/FootballUIProposal/Portable review clone/`
  was served with a different root from an unrelated working directory. The
  page and visible assets loaded without browser errors.
- `Tools/Build/Check-TaskReady.ps1` passed. No staging, commits or publishing.

Evidence includes `asset-integrity.json`, `leave-checks.json`,
`settings-camera-final.png`, `leave-host-final.png`, `wide-cancel-final.png`,
`portable-ready.png` and `task-readiness.txt` under the ignored evidence folder.
Early captures are retained as iteration evidence; final captures are named
explicitly above.

## Implementation checklist

- Await the user's design review before modifying the football game.
- Reuse existing UI geometry and font assets; pack only needed icon artwork.
- Bind the power ring to actual football charge, and change immediate football
  cancellation to release-in-zone without changing kick tuning or authority.
- Validate multi-touch, safe areas, all camera modes, possession transitions,
  menu input blocking and role-specific exits in Unity and on a physical phone.
- Deliver fresh tested Windows and Android builds with measured APK bytes when
  gameplay implementation is requested. This prototype changes no shipped assets.
