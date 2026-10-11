# Ball-first Stadium view

Football and basketball share `Shared/Player/StadiumCamera.cs`. First person,
third person, Golf, Fishing and Sky-Sail retain their existing cameras.

The ball determines the centre of action. Presented ball positions supply a
filtered velocity estimate on both authority and guests; a guest's kinematic
Rigidbody velocity is deliberately ignored. Basketball dribble height does not
drive the camera. A free airborne ball raises the framing target, with a short
gravity forecast opening space ahead of the flight. The current ball, ground
beneath it, nearby athlete and nearby basketball rim determine camera distance.
Distance opens faster than it closes, with a bounded maximum pullback.

The local player has a small, distance-tapered influence: up to 22% while play
is grounded and 8% during flight. At long distances that influence is zero.
Only the ball has a hard viewport guard. A broad inner rectangle permits
off-centre play, while the court remains a reference for judging shot height.
Large position discontinuities clear velocity prediction; the guard immediately
keeps the new ball position visible after a reset or Portal transfer.

`StadiumCameraPresenter` runs after both sports' ball presentation. PlayerView
retains its earlier input order. Movement uses the presented camera's ground
axes; look input continues to control shot/pass/defensive aim independently.
Mode, sport, target and travel transitions clear camera history and restore
the saved projection and lens.

`StadiumPlayerIndicator` runs after the camera. It places a cyan arrow and
YOU/distance label at the safe screen edge, moving along that edge to avoid
the finished HUD graphics. Distance is measured horizontally from character
to ball. A re-entry margin prevents edge flicker. Behind-camera bearings remain
well-defined. The marker uses the existing font and procedural UI geometry,
and never receives pointer events. See [sports controls](SPORTS-HUD.md).

Build the camera review with `WhatTheFish.Editor.StadiumCameraBuild.Windows`.
Run `Tools/Build/Test-StadiumCamera.ps1` with the candidate `-PlayerPath` and
desired `-Width`/`-Height`. It checks real viewport projections, continuous
flight, actual styled-button overlap, input/mode transitions and a physical
long-range Lock-on shot. `Test-BasketballProps.ps1 -Mode Local -StadiumCamera`
adds passive camera observers to the existing three-peer ability regression.
The observers do not move players or the ball. These desktop checks do not
establish Android device appearance, touch performance or WAN latency quality.
