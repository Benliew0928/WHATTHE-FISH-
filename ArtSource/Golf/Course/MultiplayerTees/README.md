# Multiplayer tee proposals

Approved plan snapshots drawn over the existing V5 map:

- `T1-inland-proposal.png` and `T1-proposal.json`: inward grass tee and rear path.
- `T2-T5-three-pad-proposal.png` and `T2-T5-proposal.json`: three-pad secondary tees.

The later centre-first instruction requires a physical centreline pad at T1.
The implemented ten-pad offsets are 0, -3, +3, -6, +6, -9, +9, -12, +12, -15
metres along the equal-distance arc. This supersedes the ten symmetric positions
straddling the centreline in the first sketch. On T2–T5 the offsets are 0, -4,
+4 metres. Assignment randomizes players and equal-distance side ties.

The JSON files are retained proposals, not runtime configuration. Active
regeneration is in `Game/Assets/_Game/Editor/GolfMultiplayerTeeBuild.cs` relative
to the repository root. See [implementation notes](../../../../Docs/GOLF-MULTIPLAYER-TEES.md).
