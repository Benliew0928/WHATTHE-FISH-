# Football abilities

Implemented from the approved football prototype, 12 October 2026.
Artwork masters and regeneration: [football props](../ArtSource/Football/Props/README.md).

## Collect and activate

Abilities appear during formal football matches **and Explore Offline**. Solo
Explore starts them automatically without pressing Start Match. Online practice
still waits for a formal match and its assigned teams. The first pickup appears
ten seconds after live play starts: five quiet seconds, then a solid dark-blue
3D 5–4–3–2–1 countdown. The host or offline game chooses one
of the two fixed points at the touchline ends of halfway, 0.75 m inside the
pitch boundary. Only one pickup exists at a time and remains until collected.
Each collection starts another ten-second cycle with the warning in its final
five seconds. Each new kind differs from the last. Goals and kickoff resets
preserve the world pickup, chosen location and fixed deadline; its timer and
presentation continue during the pause. Collection resumes with live play.

Countdown digits, floating pickup models and their floor projections are **4×**
their original display size for visibility across the large pitch. Models float
fully above the turf. Pickup distance, HUD icons, deployed walls and ability
effects retain their gameplay sizes.

Walk into the floating model to collect it. Each player has one single-use slot;
collecting another prop replaces the unused one. Tap the illustrated ability
button, or press **G**, to activate. The football menu's **Abilities** page
explains the controls. Invalid requests and cancelled wall previews retain the
prop. An accepted portal is consumed even if its transfer is interrupted.

Unused props remain with their collector through goals, kickoff resets and
overtime. The next pickup replaces that one slot; props never accumulate in a
second slot. Match completion, cancellation, a new match or leaving the sport
clears inventory.
The held prop stays visible during a goal/kickoff pause and is disabled until
play resumes.

In Explore Offline, the local player uses Team A's half for ability rules
(the negative pitch-Z half), without adding a match roster or enabling scoring.
Turbo boosts the solo player; Portal can retrieve the free ball; Wall uses the
same placement and collision rules. Leaving Explore clears all effects. Starting
a formal match clears practice inventory and waits through team selection and
kickoff before restarting the match countdown.

| Ability | Behaviour |
| --- | --- |
| Team Turbo | Whole own team receives **1.5× final movement speed for 5 seconds**. Existing sprint, dribble, charge, pressure, dash and stamina rules still apply. No stacking or refreshing an active boost. |
| Portal Steal | Use while grounded in your own half, with a 0.35 m centre-line margin. Reserves the football from any other carrier or free flight. Two blue-rimmed black hex portals animate a 0.7-second transfer to the receiver's **feet**. Receiver remains planted until transfer ends. |
| Wall | First tap opens an aimable preview 2 m ahead; tap **Place** to commit, or **Cancel** to keep it. The 6 m × 5.25 m × 0.3 m wall lasts **15 seconds**. One active wall per team. Physically blocks the ball and players on both teams. Go around it or lob the ball over it. |

## Authority and edge cases

The host alone chooses spawns, arbitrates pickups, consumes inventory and changes
ball ownership. Slot revisions reject replacement races and duplicate requests;
round revisions reject delayed requests across kickoffs. Kick RPCs also include
the portal transfer revision so an old kick cannot affect a newly transferred ball.
Room protocol **52** (including approved football audio) requires matching builds
on all players. See [football audio](FOOTBALL-AUDIO.md).

Placement validates the entire rotated footprint, pitch bounds, goal areas,
world obstacles, players, football, portal endpoints and existing walls. The
wall has a pooled world-layer box collider, synchronized independently of its
visuals on every peer. Placement creates collision immediately; reset and expiry
remove it before player movement. Swept character movement blocks walking,
dashes, tackles and jumps from either team, including the placer. Previews and
floating pickups remain non-solid. The football also uses a continuous sphere
sweep against the rotated volume for fast shots, side/top contacts, dribbling
and ball reach checks. Wall contact resolves before
goal sampling, including multiple fixed updates in one render frame. The sweep
uses contact time when a wall expires during a physics step. Expanded-box
corners are conservatively rounded by the ball radius rather than using an
expensive exact sphere-versus-rounded-box solver.

Only one portal may reserve the ball at a time. Acquisition, kicks and collisions
are disabled while reserved. A departing, airborne, obstructed or tackled receiver
cancels attachment and drops the ball safely at the source, keeping the use spent.
Goal tracking resets around the transfer so teleporting cannot score along its
path. Existing goals resolve before a new portal can claim the ball. A wall stays
until expiry if its placer leaves. Goals clear active effects while retaining
held props. Match termination and travel clear inventory and effects;
regulation-to-overtime remains continuous.

## Verification and delivery

The development-only `FootballPropReview` exercises the real runtime systems and
owner RPCs. Run `Tools/Build/Build-FootballPropsReview.ps1`, then
`Tools/Build/Test-FootballProps.ps1 -Mode Explore` for the actual solo entry path,
`-Mode Offline` for a solo formal test match, and `-Mode Local` (four players).
Each run saves assertions, logs and captures under ignored `Builds/FootballPropsQA/`.
The 3D models, countdown, placement preview, deployed wall, portal and HUD have
dedicated rendered captures. Release measurements and complete Windows delivery
evidence are recorded in [VERIFICATION.md](VERIFICATION.md) and
[BUILD-SIZE.md](BUILD-SIZE.md). Local Windows testing does not establish physical
Android touch quality, phone FPS or real Internet latency behaviour.
