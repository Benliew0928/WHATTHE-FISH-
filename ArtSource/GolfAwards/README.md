# Golf podium medals

`RankMedals.png` is the user-supplied transparent master sheet, retained intact.
Run `python Tools/Art/prepare_golf_awards.py` from any working directory to
extract its three alpha-trimmed cells into the Golf UI Resources folder.
The generator preserves every original pixel; it does not redraw or recolour
the medals. `delivery.json` records the crop bounds and delivery hashes.

The ceremony shows at most three finishers using the existing stroke/time
ranking, including existing ties; DNF players never join the cast. First place
is centred, second is screen-left and third is screen-right. Each medal follows
the animated head with a small clearance. There is no extra caption below the
artwork. Other sports keep their current awards.

## Development preview

Launch the development Windows player with
`-offline -sport Golf -golfAwardsPreview -screen-fullscreen 0`.
It opens the real ceremony with three render-only copies of the local character
and the three medals. Press **F10** to replay once the ceremony has finished.
No match scores, progress or network records are changed. This preview is
compiled only for the Editor and development players.
