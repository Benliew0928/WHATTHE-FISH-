# Basketball and football ball effects

Both sports draw a camera-facing wake from the displayed ball position. The head
is written explicitly after ball movement and replay presentation, including on
frames between network updates. World-space history does not rotate with the ball.
The continuous bright spine keeps all styles connected visually; length is capped
and the tail tapers and expires. These effects never change physics or scoring.

## Customization

- Basketball: Court settings → Ball effects. Six styles: Solar Drive, Court
  Lightning, Glacier, Rose Comet, Aurora Rush and Championship.
- Football: Pitch settings → Customize. The five existing styles retain their
  saved selections and now use the improved wake.
- Both sports expose Ball effects in the main settings and Customize in the room.
  Guests can change their personal effects without gaining venue edit authority.
- Choices are saved on the viewer's device. Everyone sees the moving ball's
  effects, using their own selected style, regardless of who possesses it.

Basketball adds twin speed accents and sparks, a wider/longer shot wake, a compact
pass wake, release rings and floor-impact rings. Held dribbles and aiming do not
emit flight trails. Catches, resets, inbound holds, portal transfers, travel and
disabled objects clear obsolete effects. Release and impact accents use the
displayed motion and existing replicated state, with no additional network events.

Reduced motion freezes animated shader detail, shortens and narrows both sports'
wakes, and suppresses basketball rings/sparks. It preserves ball tracking.

## Implementation and verification

- [SportsMotionRibbon](../Game/Assets/_Game/Shared/Core/SportsMotionRibbon.cs)
  holds at most 96 samples. Football uses one line renderer; basketball uses one
  wake and a fixed pool of four 40-segment rings. Materials are shared per style.
- [BasketballTrail](../Game/Assets/_Game/Sports/Basketball/BasketballTrail.cs)
  handles reset generations and shot/pass counters from the existing snapshots.
- [FootballTrail](../Game/Assets/_Game/Sports/Football/FootballTrail.cs) also
  supports the visual-only replay ball. Sequence snaps clear obsolete history.
- [SportsTrailBuild.BuildReview](../Game/Assets/_Game/Editor/SportsTrailBuild.cs)
  builds the complete review player under `Builds/SportsTrailQA/Player/`.
- Run [Test-BasketballTrail.ps1](../Tools/Build/Test-BasketballTrail.ps1) in Offline
  and Local modes. It checks real releases, every picker choice, per-frame head
  attachment, impact rings, catches, resets, reduced motion and guest preferences.
- Run [Test-FootballTrail.ps1](../Tools/Build/Test-FootballTrail.ps1),
  [Test-FootballBall.ps1](../Tools/Build/Test-FootballBall.ps1) and
  [Test-FootballReplay.ps1](../Tools/Build/Test-FootballReplay.ps1) with the review
  player path for kicks, room transport and replay coverage.

Current local captures, logs, source hashes and delivery notes live under ignored
`Builds/SportsTrailQA/`. Final Windows delivery is the complete
`Builds/WindowsFinal/` folder; Android is rebuilt through
`Tools/Build/Build.ps1 -Target AndroidSubmission`. Desktop rendering and local
room tests do not establish appearance, FPS or network performance on phones.
