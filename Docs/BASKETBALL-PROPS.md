# Basketball props

Basketball uses one held prop slot per player. Walk into a floating courtside
pickup to equip it; a new pickup replaces the unused item. Every pickup grants
one use. Tap the illustrated ability button or press **G** to activate it.
Picking up another prop later grants another use. Invalid actions retain the
item. The player who collects an item controls its activation.

The pickup area is a **1.2 m horizontal radius**, increased from 0.85 m, and
works from every facing direction. A grounded player waiting at the spawn
point receives the prop on the same simulation tick it appears, after the
full countdown. Walls, jumping, transit and action locks still prevent pickup.
The host awards one item to the nearest eligible player; there are no duplicate
grants when several players overlap the area.

Unused props stay with their collector through baskets, out-of-bounds inbounds,
shot-clock restarts and overtime. Collecting another item replaces the single
held slot; there is no stored second item. Activation spends the item once.
Match completion, cancellation, a new match or leaving the sport clears inventory.
Restarts still cancel active boosts, aiming and transfers.

## Abilities

| Prop | Activation | Effect and counterplay |
| --- | --- | --- |
| Lock-on Shot | One click while holding the ball, grounded inside the painted court | Immediately spends the item and plants the player for two seconds. Automatically shoots at the opposing basket. No hold or timing meter is required. A nearby opponent's normal Block interrupts the aim; losing possession also cancels it. An interrupted item is never refunded. |
| Team Turbo | One click inside the court | The collector and their active teammates move at 150% of normal walking/sprinting speed (1.5×) for five seconds. Jump height and action restrictions are unchanged. An existing boost prevents another activation without spending that second item. |
| Portal Steal | One click in the collector's home half | Both blue-rimmed, black-centred portals open together. The current ball shrinks into its source portal and emerges beside the receiver's hand over 0.7 seconds. Works on opponents, teammates, loose balls and shots/passes in flight. |

Lock-on follows an assisted, steep arc with a centred descent. Normal shots
retain their probability and physical trajectory. Powered shots still require
a real downward hoop crossing for points; blocks, physical contacts and Portal
Steal remove the assistance. The existing two-/three-point calculation applies.

Squad A defends the negative-Z end of the arena; squad B defends the positive-Z
end. Lock-on chooses the opposite hoop by position, independent of imported
art names. These sides are fixed for the session. Solo practice uses A. Portal activation
requires the player's feet to be at least 0.35 m inside the home half and
inside the painted-court margin. The ball itself can be anywhere in the arena.
Already scored baskets cannot be undone. Self-stealing is disabled.

The game retains shared basketball practice scoring. These abilities do not
introduce a timed team match or change the four-island, shared-travel model.

## Courtside placement

Two fixed arena-local locations sit just inside the sidelines at the ends of
the centre line: **(-7.00, 0.08, 0)** and **(7.00, 0.08, 0)**. There is one
active pickup at a time. The host randomly chooses left or right for each
spawn and samples one of the two types different from the previous spawn.
The side may repeat; the prop type cannot repeat consecutively within a session.

Before every spawn, a translucent blue hologram at the selected position counts
**5, 4, 3, 2, 1**, then disappears as the prop materializes. The numbers are real
extruded meshes with scan lines and a blue projection ring. They face the local
camera while retaining their depth. No pickup is visible or collectible early.
Players may wait at the announced location; normal host pickup arbitration
applies after materialization.

The first pickup appears ten seconds after live play starts: five quiet seconds,
then the five-second warning. Every collection starts the same ten-second cycle.
An uncollected pickup remains indefinitely; no additional pickup spawns beside it.
The deadline is fixed even if a host update arrives late. Deadline and location
replicate in snapshots, so clients and late arrivals use the same remaining time.
Baskets, out-of-bounds inbounds and shot-clock restarts preserve the same world
pickup and pending deadline. The timer and presentation continue during these
pauses; collection waits until inbound play resumes. Only a new match or leaving
the session starts a fresh cycle; match completion/cancellation clears it.

The original painted court remains 15.24 by 28.6512 m. Player containment and
the energy wall now follow those exact painted lines. The relocated pickups
remain reachable without Free roam. Activation requires being inside the court;
a standing inbound player must pass before using a prop. Pickups rotate and bob above coloured halos.
The animated transform preserves each FBX root's imported unit scale and rotation,
so the actual spawned mesh keeps the same readable size as the authored model.

## Authority and lifecycle

- The host chooses spawns and the closest eligible collector; equal distances
  use stable network player IDs. Clients never award their own items.
- Every slot change has a new revision. Owner-only requests must match it,
  preventing duplicate clicks or delayed requests from using a replacement.
- Portal transfer reserves the existing ball and clears pending shots, passes,
  steals and scoring attempts. Only one transfer can run at a time. A second
  request retains its item. A departing/invalid receiver releases a loose ball.
- Forced aiming and receiving lock movement, jumping and other ball actions.
  Session changes clear inventory, boosts and transfers. Free-roaming and
  travelling actors cannot collect or activate props.
- **Protocol 46** is required on all peers.

## Art and verification

[Editable art and regeneration](../ArtSource/Basketball/Props/README.md).
Models and matching transparent icons use a shared palette; the portals use
one small procedural shader, without scene-capture cameras or an effects pack.

Build a full development candidate with
`Tools/Build/Build-BasketballPropsReview.ps1`. Run
`Tools/Build/Test-BasketballProps.ps1 -Mode Offline` and `-Mode Local`, passing
`-PlayerPath` when validating a candidate. Evidence belongs in ignored
`Builds/BasketballPropsQA/`. Development injection hooks do not ship in the
Android release and cannot be invoked over the network.

The review exercises all three types through the real spawn renderer on both
sides, checking visible mesh bounds above the floor and capturing each model.
It checks all five visible countdown digits, uncollectible warning states,
ten-second respawning, repeated host transitions without consecutive types,
architecture clearance, multiplayer agreement and disabling/re-entering the arena.
It measures controller speed and distance travelled for walking and sprinting:
6 / 10.5 m/s during Turbo, versus 4 / 7 m/s normally. Local multiplayer also
checks the boosted teammate and unaffected opponent. The Lock-on deadline,
stationary aim and actual release are checked against two seconds.

Release measurements and qualification limits belong in
[build size](BUILD-SIZE.md), [packing audit](APK-SIZE-AUDIT.md) and
[verification](VERIFICATION.md).
