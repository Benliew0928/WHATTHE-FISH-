# Programmed sports bots

Football and basketball use small rule-based C# controllers. There are no
trained weights, inference runtime, Python service, cloud training costs or
automatic policy replacements.

## Playing

Select either sport, choose **Play vs bots**, then choose **2v2 to 5v5**.
Select **Choose team**, pick **Blue** or **Red**, then click **Start match**.
Solo selection has no timer or team-count labels, and Start stays disabled
until a team is chosen. Bots fill the remaining places evenly, preserving
the human's choice. Start immediately opens football kickoff or basketball
play. Match length is set in Match length. In a room, the host can fill missing slots
before starting a match, or remove bots while no match is active. Humans
still choose teams; remaining places are assigned when selection expires.

**Explore offline** stays a bot-free sandbox. Returning to it clears the
solo match and its bots. Leaving the venue or shared travel removes bots.
Bots do not replace disconnected humans: a participant leaving still voids
an active match. Formal [match rules](ROOM-MATCH-RULES.md), assigned attacking
ends, scoring, restarts, timers and action restrictions apply equally to bots.

## Behavior

- Football retains the casual policy: recover, dribble, evade, pass into
  open lanes, shoot, support, cover, pressure, tackle and use collected props.
- Basketball recovers loose balls, spaces teammates, advances toward the
  assigned hoop, passes to a better-positioned teammate, charges shots,
  guards the lane to its own hoop, attempts steals or jump blocks, and collects
  and uses props through the normal ability rules.
- Both use the shared athlete movement and ball action rules. They cannot
  award themselves possession, teleport the ball, bypass cooldowns or set
  the score. Basketball shot timing has bounded error and retains the
  normal distance/pressure-based shot odds.
- Decisions are deliberately delayed, with readable defensive commitments.
  This is one casual difficulty, not a trained or adaptive opponent.
- Only the host runs room bot decisions. Replicated sport and slot IDs keep
  each bot distinct from human owners. Matching builds require protocol 52.

## Validation

Run from a PowerShell terminal, without needing PowerShell 7:

```powershell
& .\Tools\Build\Test-ProgrammedBots.ps1 -Build
```

The script resolves the repository from its own location, builds an isolated
development player and checks offline and local host/guest play for both
sports. `-OfflineOnly` and `-Sport Basketball` narrow diagnostics.
Evidence is written under ignored `Builds/ProgrammedBotsQA/`. The review
driver is development-only and requires an explicit command-line flag.
It does not run during ordinary play or ship in the Android release.

See [verification](VERIFICATION.md) and [actual APK measurements](BUILD-SIZE.md)
for the delivered build and remaining device/network qualification limits.

## Retired Training

The user retired learned AI on 14 October 2026. The ML-Agents bridge,
observation/action training contract, skill drills, teacher recording,
training/evaluation scripts, model import and inference build support are
archived outside Unity. ML-Agents, Unity Inference Engine and its App UI
dependency have been removed from the active package graph.

Local recovery archives, not remote backups:

- `Legacy/20261014-012444-568-retired-learning-source/`: source, tools,
  configs and former training documentation, including the old AI review.
- `Legacy/20261014-012855-616-retired-learning-data/`: training plans,
  demonstrations, evaluations, models, selected candidates and old players.
- `Legacy/20261014-013411-364-retired-learning-diagnostics/`: superseded
  AI calibration, training repair/merge evidence, helper scripts and players.

Each batch contains its original-path, byte-count and SHA-256 recovery
manifest. Historical release records describe their original builds, not
an active training workflow. Unity caches and the installed local Python
environment remain ignored and unreferenced, per repository instructions.

Latest change validation: [bot start and pass presentation](BOT-START-PASS-VERIFICATION.md).
