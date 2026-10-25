# Welcome sport icons

These are transparent studio renders of the existing gameplay prefabs, not
replacement ball/fish models or newly generated illustrations. Editable masters:

- [Football](../../Game/Assets/_Game/Prefabs/Football/SoccerBall.prefab)
- [Basketball](../../Game/Assets/_Game/Prefabs/Basketball/BasketballBall.prefab)
- [Golf](../../Game/Assets/_Game/Prefabs/Golf/Ball.prefab)
- [Fishing](../../Game/Assets/_Game/Prefabs/Fishing/MediumLagoonFish.prefab)

The [Unity capture tool](../../Game/Assets/_Game/Editor/WelcomeSportIconBuild.cs)
renders the highest-detail prefab mesh with its existing materials in a temporary
empty scene. It disables lower LODs and fish tail motion on temporary instances;
the source prefabs, mesh files, materials and gameplay are unchanged.

Run Unity 6000.3.20f1 with graphics enabled and editor method
`WelcomeSportIconBuild.Capture`, or use **WHATTHE FISH? / Welcome / Render sport
icons**. `CaptureAndBuild` also builds a Windows review player. The editor-only
SRP initialization bridge is pinned to this Unity version and fails explicitly
if that API changes. Invalid empty or magenta captures fail before being written.

The PNGs live in
[`Game/Assets/_Game/Resources/WelcomeSportIcons/`](../../Game/Assets/_Game/Resources/WelcomeSportIcons/).
Each is a 1024×1024 RGBA render with transparent background and 4× MSAA.
The original PNGs retain full resolution; desktop uses uncompressed RGBA and
Android uses high-quality ASTC 4×4. Mipmaps and resizing are disabled, with clamp
and bilinear filtering. Four static images are reused by Info; no extra scene or
camera runs while the player browses Info. PNGs contain no external dependencies.

The panel displays them in 84-unit square frames, preserving the capture aspect
ratio. Golf uses a cream/beige gradient and Fishing a light blue water gradient.
The other two card colors are retained. Welcome text is rasterized at the canvas's
actual display scale, and native panel edges use a one-pixel alpha fringe rather
than a blurred bitmap. Version and Done remain; Credits has been removed from Info.

Current output hashes are in [integrity.json](integrity.json). Local screenshots,
capture logs, build logs and checks belong under ignored `Builds/WelcomeIconsQA/`.
