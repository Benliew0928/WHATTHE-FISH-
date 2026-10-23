# Football passing, player statistics and MVP

## Controls and passing

Tap Pass or press Space to immediately lead the eligible teammate closest to
the camera heading. Double-tap a teammate to choose them directly. Pass no
longer has a hold/aim/release gesture. Sliding from Dash onto Pass also commits
immediately; Dash can remain held. Shoot still charges, and returning to Dash
releases that shot. Switching from an aimed shot to Pass cancels the shot.

The local halo previews the selected receiver. The authoritative flight ribbon
appears for everyone after initiation, including guests and spectators. Moving
receivers collect clear passes automatically within receiving reach. Small
corrections follow normal running; sharp turns preserve the original predicted
destination. Teammates and opponents in the lane can intercept. There is no
receiver teleport or ball U-turn. See [shared passing rules](TEAMMATE-PASSING.md)
for selection, prediction, interception and multiplayer details.

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

Latest change validation: [bot start and pass presentation](BOT-START-PASS-VERIFICATION.md).
