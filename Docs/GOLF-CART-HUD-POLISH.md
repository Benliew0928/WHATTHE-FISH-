# Golf cart impacts, HUD and finisher awards

## Presentation and input

- Golf leaderboard docks at the top left. Its full compact rectangle, including
  the frame, cells and empty padding, opens the full standings. Expanded scrolling
  and the existing close controls remain available.
- Room chat and quick stickers dock below the actual compact leaderboard bounds.
  They hide while full Golf standings are expanded. Other sports keep their chat
  placement. Chat/sticker launchers use the Golf HUD auxiliary fade while aiming and disable their pointer/keyboard launch actions during Aim. The floating in-game Players profile launcher is retired for all sports;
  room profile access remains available.
- Golf Settings uses the same top-right anchor and 18/16 reference-unit insets as
  Football. Event warnings and active event badges dock to the left of its actual
  scaled bounds with a 12-unit gap. Tropical Wind's compass is inline to the left
  of its event badge. Both event badges retain their original art at 75% of the former size, stacked vertically. The finishing clock docks at bottom centre at 70% of its former corner scale; its centre introduction and smooth transition remain. At full reference size the docked plate is 287.28 x 122.094 units, each numeral 35 x 35, and s scales proportionally.
- Moving the stick (or movement keys) ends the Golf shot camera with the existing
  smooth return. Accepted Jump requests use that return too. Aim turning continues
  to use the stick without cancelling aim.

## Car-to-car response

Driving sweeps identify the other cart and the contact normal. The Host computes
relative closing velocity and applies equal/opposite horizontal recoil to both
carts, including an unoccupied cart. Contacts below 0.8 m/s are ignored; each
participating cart has a 0.35-second impact cooldown. Recoil impulse is limited
and decays exponentially at 5/s, with exact integrated displacement. For closing speeds 0.8-20 m/s the initial recoil is 5-15 m/s, producing approximately 1-3 metres per car (2-6 metres combined) on clear ground. Held throttle is suppressed until recoil speed falls below 0.2 m/s (about 0.64-0.86 seconds), so it cannot immediately cancel separation. Obstacles can shorten travel. Recoil movement uses the existing bounded sweeps,
terrain/water support and clearance checks; it is not a teleport or a Rigidbody
conversion. Positions still replicate through the existing authoritative cart
snapshot. The existing CartBump feedback channel broadcasts a short gold spark
burst and impact sound. Car/car bursts contain 64 sparks, initial size 0.08-0.18 metres, lifetime 0.45-0.65 seconds and 2.8-3.1 m/s radial speed (up to about 1.5-2 metres horizontally). Gravity and the downward component affect the full 3D spread. No new image assets are added.

## Car-to-pedestrian response

The nearest swept pedestrian contact can launch a non-spectator on foot. Contacts below 1.5 m/s do not launch; each victim has a 1.2-second cooldown. Host-authoritative flight uses 3-8 m/s horizontal impulse, 4.5-7 m/s vertical impulse and the existing Jump motor gravity/collision/landing pose. Horizontal velocity decays at 1.4/s; walls damp it further. Drivers and travelling players are excluded. Existing island fall recovery remains available. Input resumes on landing; a three-second safety timeout prevents a permanent movement lock.

A hit releases Aim, resets the club pose (invalidating a queued contact), clears Host input and sends an owner-only input cancellation RPC. Existing NetworkTransform and JumpSnapshot replicate the flight. Ball ownership, progress and score rules stay unchanged. A 32-mote cyan/gold impact burst and the approved cart-bump clip at lower pitch with a quiet swish play through the existing broadcast/pool; no new sound files. This RPC and the extra cosmetic feedback enum require matching protocol 83 on every player.

## Hazard camera replication

Ball snapshots retain the hazard-launch sequence used to cancel stale pending
shots. A separate replicated `hazardReplay` flag identifies tornado launches;
crab nudges set it false and do not start a ball-follow camera. A crab nudge to an
already followed ball also returns the camera to the golfer. Room recovery restores
this flag. This wire change requires matching **protocol 83** on all peers.

## Golf honours

Only players whose score records are finished, and not DNF, enter the awards cast.
The order and displayed rank come directly from `GolfMatchState.Results()`; tied
scores keep their existing ranks. The highest-ranked finisher stands at
screen centre, second on the left, third on the right, then fourth farther left,
fifth farther right, and so on. Rank badges use gold/silver/bronze accents, a star
medal, place and player name. The group camera fits all shown golfers, and badges
follow their render-only doubles. Real player transforms and scores are unchanged.
If nobody finished, proceed to results without creating a DNF cast. Fishing's
shared awards presenter keeps its original roster, arrangement and animation.

## Build evidence

Current source/build notes and measured APK records are in [verification](VERIFICATION.md).
Task logs, source hashes and package audit are kept under ignored
`Builds/GolfCartHudPolish/` (previous delivery) and `Builds/GolfImpactLayout/` (current impacts/layout delivery). Builds do not establish actual multiplayer behaviour
or physical-device performance; this task does not add or run gameplay tests.


## Current delivery - 24 October 2026

Complete Windows and Android builds passed; package integrity, APK v2 signature,
and source-hash checks passed. Actual APK: 113,325,590 -> 113,325,786 bytes;
headroom: 886,674,214 bytes. No runtime/multiplayer/device tests were run.
See the current measured record in [verification](VERIFICATION.md).
Archives: `Legacy/20261024-005046-827-golf-impact-layout/` (previous player/APK)
and `Legacy/20261024-005919-401-golf-impact-intermediate/` (intermediate player).
