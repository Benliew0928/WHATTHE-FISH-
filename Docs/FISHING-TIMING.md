# Fishing: timed pulls (historical experiment)

**Superseded:** current gameplay restores [Hold the Glow](FISHING-COUNTERSTEER.md).
Keep the two Left/Right buttons and hold the lit direction; current camera and
rope presentation remain. The timed-tap design below is retained as history.

The 15 October revision replaces continuous Hold & Countersteer. Round, Cup,
Crew, scoring, Wanted bonuses and the six-skill/two-favourite system remain.

## Touch controls

1. Aim with the camera until the reticle turns teal. Tap the illustrated Cast
   button. When the fish shows **!**, tap Hook within 1.8 seconds.
2. The fight opens one moving circle and a gold target zone. Tap **Reel** while
   the circle is inside gold. Each sweep permits one attempt. Holding does not
   repeat a pull, and tapping rapidly cannot repair a missed sweep.
3. Every third sweep is a sideways fish surge. The same right control becomes
   **Right**, and **Left** appears at the opposite thumb. A bouncing arrow and
   brighter button identify the opposing pull: fish left, pull right; fish
   right, pull left. Time that directional tap in the same gold zone.
4. A good tap flashes teal and performs a short rod/reel stroke. A centre hit
   earns a slightly longer stroke. A mistimed or wrong-side tap flashes coral
   with brief contained feedback. Skip a sweep to let line tension recover.
   At 100% tension the line still snaps; an unattended fight ends after 30 seconds.
5. Landing shows the accepted points, fish badge, ranking change and short
   sound. These effects never award points themselves.

There is no third fight button or keyboard instruction in the mobile HUD or
shared tutorial. Development keyboard shortcuts remain available in the
Windows player. A pointer-up does not erase a queued tap or interrupt its
accepted short stroke. Focus, pause and context changes clear local input;
accepted strokes have a finite host deadline, rather than a renewable hold.

## View and presentation

Movement, jump, manual camera switching and travel controls hide while the
line is active. Jump also hides at a fishing pier so the Cast guidance arrow
points directly at its control. Before casting, camera dragging and the
preferred camera modes remain available.

An accepted Hook smoothly frames the character, rod and fish. Directional
sweeps gently shift the shoulder toward the counterpull, with a small angle
change; screen-left/right are preserved. Accepted directional strokes visibly
swing the rod and torso and apply an opposing lateral force to the fish's
existing spring/drag model. The water ring turns gold during accepted strokes.
A tension-sensitive reel clutch softens winding under load; it does not move the
fish or award catch progress. No extra realtime light, image, model, audio asset
or effects package is added.
Reduced Motion retains the preferred camera and suppresses guidance bouncing.
Finishing or cancelling returns to the saved view without changing it.

The revised tutorial appears once for existing installations too. Its entry
popup and How to Play use the same view and content.

The circle, arrows, buttons and tutorial example use shared runtime widgets.
Layout stays within the safe-area frames; sizes and camera settings remain
editable in `Game/Assets/_Game/Resources/FishingUILayout.asset`.

## Host authority

Matching multiplayer builds require protocol **56**. The host evaluates the
tap's original synchronized press time, with at most 250 ms transit and 50 ms
future-clock tolerance clamped to host time. Old round/cast, repeated beat,
nonfinite, stale and future presses are rejected. The host grants the same
fixed stroke duration on receipt for local and remote players; duplicates
cannot extend it. A valid miss uses that sweep's attempt and adds a small amount
of strain. Scoring and fish claims
remain host-owned. This tolerance is not a guarantee for every internet delay.

The circle sweeps in 1.65 seconds, reversing direction on each sweep. Gold spans
the middle 32% of the track; its centre 13% is the perfect region. A good stroke
lasts 1.02 seconds and a perfect stroke 1.12 seconds. Bigger fish retain stronger
line forces and surface surges. The camera and graphic do not decide scores.

Physical-phone touch, appearance, performance and real internet multiplayer
still require device testing; Windows automation cannot qualify them.
