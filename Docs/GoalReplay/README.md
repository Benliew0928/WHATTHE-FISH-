# Broadcast Slash goal replay

The second selected transition is implemented as native Unity UI. The
[approved direction](broadcast-slash.png) and [art reference](slash-plate-master.png)
stay outside the shipped Resources folders. The running game uses vector
diagonal panels, the existing official logo, existing fonts and sound bank.
No video, new texture, audio bank or package is needed for playback.

![Current illustrated in-game transition](in-game-transition.png)

The enlarged official logo and football drawings above are from the actual
Windows player at 1600×900. The same composition was checked at 844×390.

## Match flow

1. The host accepts the whole-ball goal crossing once and pauses the match clock.
2. The ball continues into the reactive net for 1.3 seconds. This captures the
   required second after the goal, with a small replication margin.
3. An aqua, coral, cream and navy slash covers the screen for the camera change.
   It lasts **1.6 seconds**: 0.3 seconds entering, a full 1.0-second still hold,
   then 0.3 seconds leaving.
   A large official logo sits in an illustrated badge beside the GOAL title and
   updated score. Native vector football, shot-trail, net and celebration drawings
   add character to the navy field; gold seams separate the panels. A fixed design
   field keeps the composition together across desktop and wide landscape phones.
4. Replay plays exactly five seconds, from crossing minus four seconds to
   crossing plus one second, at normal speed. Transition time is additional.
5. A **0.8-second** slash covers the return: 0.25 seconds entering, a 0.3-second
   hold, and 0.25 seconds leaving. Camera restoration and centre reset happen
   during the fully covered hold. The conceding team takes the centre kick;
   a winning overtime goal returns to the result instead.

The first kickoff chooses a team. After goals, possession goes to the team
that conceded, regardless of who leads the total score. Human teammates rotate
the duty in rooms; the human takes their team's restart in VS AI. An AI-only
team takes its kick automatically. Everyone else stays at their formation
position until the kick. The taker can look around a full circle, hold Kick/F
to charge and release to pass or shoot. The match clock starts on contact.

## Camera and recording

A continuous three-quarter camera follows the recorded ball and frames the
goal mouth. Its side stays fixed for the clip, preserving the direction of
the attack. Smooth position and distance changes reveal the approach and net
impact without spinning or repeated cuts. Eye level, Follow and Stadium
preferences and heading are restored after playback.

Each peer keeps a bounded 30 Hz pose history (240 slots, with a second buffer
for the frozen clip). Replay meshes share
existing geometry and materials, with recorded player bones, ball transforms,
prop visibility, local contact sounds and reactive-net impulses. Render-only
copies have no physics, Animator, gameplay scripts or network identity.
Playback cannot change the host's ball or award another point. The live world
continues recording while the saved clip is displayed, so even a second goal
immediately after kickoff has its full history. Cancellation and scene departure restore
the live renderers and camera.

Protocol 58 carries the goal timestamp, event ID and kickoff team/player with
the existing match snapshot. Host and guest builds must match. A late join or
interrupted session with incomplete local history shows an explicit unavailable
message instead of substituting another goal.
Matching builds also keep the longer entry and return timings synchronized;
older clients cannot join a protocol 58 room.

## Coverage and development checks

Shared match code covers VS AI 2v2–5v5, human rooms 1v1–5v5, and rooms filled
with bots. Explore Offline remains free practice without competitive scoring.
Run `Tools/Build/Test-FootballReplay.ps1` against the development player for
physical goals, timing, rendering, kickoff authority, camera restoration,
golden goals, cancellation and host/guest replication. Optional `-Width 844
-Height 390` exercises a compact landscape view. Current evidence lives under
ignored `Builds/FootballReplayQA/`; release measurements and qualification are
recorded in [verification](../VERIFICATION.md) and [build size](../BUILD-SIZE.md).
The longer-transition follow-up is recorded under ignored
`Builds/FootballReplayTimingQA/`, including per-frame entry/return traces that
check the hold durations and covered camera/reset changes.

The superseded browser experiment was archived through the repository tool in
`Legacy/20261014-234321-910-goal-replay-browser-draft/` after the user requested
actual game integration.
