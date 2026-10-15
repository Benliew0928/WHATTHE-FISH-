# Football passing, player statistics and MVP

## Controls

Dash is the large main thumb button. Shoot occupies the diagonal upper-left
slot and still changes to Pressure on defense. Hold Dash, slide onto Shoot
to charge, then return to Dash to release the shot while keeping Dash held.
Lifting releases the shot and ends Dash. All four action hitboxes accept captured swipes in either direction: Dash, Shoot/Pressure, Pass and Tackle (when eligible). Dash and Pressure remain held while visiting other actions. Returning to Dash releases an aimed pass or shot; switching directly between Shoot and Pass cancels the previous aim to avoid firing both. Lifting ends every hold owned by that finger. Shoot also accepts a separate finger.
Cancelling that shot does not cancel the independent Dash hold. Exhaustion,
hits, menu changes and match transitions still stop invalid input.

Pass replaces the football Jump button. Aim with the camera and tap Pass;
hold it to see a dashed directional ground-pass path, receiver ring and LOCKED # jersey number, then release. Free passes show the direction without a teammate lock. Space performs the same action
on keyboard; F remains Shoot and Left Control remains Dash. Charging a shot
while dashing preserves dash movement and stamina drain. Without Dash, the
existing charging movement penalty remains.

## Passing

Only a player controlling the ball can pass. Target selection chooses the
nearest teammate ahead of the aim heading, up to 30 metres away, inside a
lane with a 1.8-metre half-width. Scenery obstructing the route rejects that
lock. Without a teammate in the lane, Pass sends a free grounded pass in the
aim direction. Opponents remain able to block either kind of pass. The ball
is physical: there is no homing or receiver teleport. Bots use this same Pass
action when their existing policy decides to pass.

## Host-authoritative match records

Every participant, including bots, has goals, assists, successful passes and
interceptions. The host publishes these records to guests. A new match clears
the previous totals; kickoff resets clear pending ball attribution while
retaining the match totals. These are match records, not a persistent career
profile.

- **Goal:** the last player to touch the scored ball on the scoring team.
  An own goal still increases the opposing team's score, without awarding
  the defender a personal goal.
- **Successful pass:** a new Pass action received under control by another
  teammate within five seconds. Repeated contacts cannot add more passes.
  An ordinary Shoot action never starts pass or assist tracking, even if a
  teammate receives the shot.
- **Assist:** the completed direct pass to the scorer, released no more than
  five seconds before the accepted goal crossing. Another player's touch,
  a new pass, interception or reset breaks the earlier assist chain. No
  self-assists or secondary assists are awarded.
- **Interception:** an opponent physically blocks an active Pass action
  before it completes. Each pass can award at most one interception. Blocking
  a shot or tackling an already-controlled ball does not count as a pass
  interception.

MVP ranking is **2 × goals + assists**. Thus, one fewer goal and three more
assists wins. Equal points prefer more goals; equal goals and assists prefer
more completed passes. A final exact tie uses stable player ID so every
client selects the same player. Interceptions are displayed but do not change
the requested ranking.

## Presentation and sound

The final replay completes before the MVP award appears. Ten illustrated 2D
victory poses retain the roster's hairstyles, face identities, shoes and jersey
numbers. The selected player's full-body artwork fills the left side. A real
camera at 1.55 metres above the pitch shows the turf and stadium behind it;
live players are hidden locally while the award is open. A translucent ivory
panel carries large MVP, name, goal and assist type, plus completed passes and
interceptions. Match stats lists everyone's totals. Back to pitch restores
the camera, player visibility and existing match/room controls.

The portraits use the blue presentation kit from the identity references;
the player's actual team is written beside their name. High-resolution PNG
masters remain in the asset tree. Android imports cap them at 512 pixels with
ASTC 6x6; Windows uses a 1024-pixel cap. The [prompt set](../ArtSource/FootballMVP/prompts.json)
records the built-in image generation requests and all ten asset paths.

Grass walk, run and turning footsteps play only for the local player.
Shared ball contacts and match sounds remain audible. The player's own
footsteps are the only footsteps recorded into their local goal replay.

The implementation applies to VS AI 2v2–5v5, human rooms and bot-filled rooms.
Network protocol 61 requires matching builds (including basketball match statistics). Runtime validation is invoked
with `Tools/Build/Test-FootballPlaymaker.ps1`; release measurements and device
limits are recorded in [verification](VERIFICATION.md) and
[build size](BUILD-SIZE.md).
