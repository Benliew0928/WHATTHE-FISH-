# Golf event development hotkeys

Available in the Unity Editor and Development Windows player. Start a Golf match,
then focus the Host window (or an offline game). Clients cannot trigger events.
Typing in a selected text input suppresses the keyboard shortcuts.

| Key | Action |
| --- | --- |
| F1 | Tropical Wind |
| F2 | Unused; Moon Golf removed |
| F3 | Sticky Green |
| F4 | Bounce Party |
| F5 | Unused; Random Swap removed |
| F6 | Island Rush immediately, for 30 seconds |
| F7 | Clear random event and Rush |
| F8 | Tornado |
| F9 | Crab Crossing |

Manual triggers restart the automatic 60-second cycle during ordinary play;
once the first player finishes, the cycle remains stopped. An airborne ball keeps
its captured shot conditions until it rests or recovers. Clearing events affects
subsequent shots; it does not rewrite the physics of a shot already in progress.
Testing Rush before a player finishes does not suppress the normal first-finisher
Rush. F7 can clear that Rush without it immediately restarting.

Run `Tools/Build/Test-Golf-Two-Players.cmd` from any working directory to open the
complete `Builds/WindowsFinal/` player twice on loopback port 7777. Both players
must Ready in the lobby; the Host enters the island and starts the game.
No probe or automatic driving is enabled by this launcher.

Focused validation: 55 offline + 30 Host + 32 Client event assertions passed.
Actual preparation-room entry/sign visibility passed 17 Host + 16 Client checks.
Evidence is under ignored `Builds/GolfSignEventsQA/`. Dispatch is exercised via
the development API; physical function-key presses were not automated. Protocol
is now 78; update every peer. Android release excludes development hotkeys.
The earlier assertion counts above describe the original shortcuts. The new
hazards have a separate focused review; see [hazard rules](GOLF-HAZARDS.md).
