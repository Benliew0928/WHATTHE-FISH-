# Verification record — 22 September 2026

## Completed

- Unity 6000.3.20f1 project imports and C# compilation pass. URP 17.3.0, Multiplayer Services 2.3.3 and NGO 2.13.3 compile together against the installed editor.
- Original stadium and athlete export from Blender 5.1.2. Stadium inspected in a Blender render from elevated and pitch-level views. Both assets imported into Unity and used in the Windows player.
- Stadium FBX reduced from approximately 12.1 MB to 1.93 MB by simplifying repeated seat geometry. Final editable sources remain outside Unity. No spectator NPCs or crowd audio added.
- Windows development player builds and launches. Visual inspection performed on the actual native game window: main menu, character customiser, third-person and elevated stadium views. All three camera positions also exercised by the offline probe.
- Offline movement probe moved the avatar 4.8 m; no internet/UGS initialisation was needed. Chibi Idle/Walk/Run clips imported and the runtime Animator evaluated. The on-screen FPS indicator displayed 30 FPS on this laptop; this is **not** a phone performance measurement.
- Character colour selections made in the customiser persisted across player launches. The changeable hair tuft and skin/hair/outfit preset controls are implemented.
- Two independent NGO/Transport loopback rooms ran simultaneously on ports 7777 and 7778. Room A contained ten players, Room B two. Their player counts and transforms remained separate.
- Room A rejected an eleventh process with `This room is full (10 players).`
- Server-owned spawn positions were distinct. Movement was simulated by the host and matched across client logs; character appearance IDs and ready state replicated.
- Host start changed every client's exploration phase. Host return changed every client back to the waiting phase. A later join attempt during exploration was rejected.
- Host process exit disconnected guests and returned the application from exploration to menu state (`connected=False`, `exploring=False`). A disconnect reason was available to the UI.
- A separate two-process test replicated a changed host stadium preset (`TEST 7780`, lilac palette, design 2, team welcome, flags off) to the guest during exploration. Evidence: `Builds/preset-host.txt` and `Builds/preset-client.txt`.

Evidence: `Builds/NetworkQA/*.txt` and `*.log`, `Builds/smoke.txt`, `Builds/scene-audit.txt`, build logs and Blender review PNGs. These generated evidence files are excluded from Git. The reproducible local room test is `Tools/Build/Test-LocalRooms.ps1`. Development probes run only with explicit `-probe` arguments.

## Still awaiting access / not passed

| Requirement | Status |
|---|---|
| Android development APK | Not produced: Android Build Support is absent. Official installer downloaded, but Windows cancelled its administrator prompt. User must complete the Hub module installation. |
| Physical Android launch, touch/multitouch, camera clipping | Not tested on a phone. `adb devices` returned no device. Desktop checks do not prove these. |
| Sustained 30 FPS on Android and APK below 100 MB | Not measured. Requires the Android build and target phone; no compliance claim is made. |
| Internet sessions across different networks | Not tested. Unity cloud project is unlinked; Authentication/Lobby/Relay account setup is required. |
| Invalid cloud codes, cloud-full room errors, network outages | Local validation and UI handling are implemented, but live service failure scenarios remain unverified. |
| Ten users through Relay and simultaneous cloud sessions | Not tested. Loopback tests verify the game/transport logic only. |
| Stadium preset replication over internet/Relay | Changed-preset replication passed in local NGO/Transport testing; internet verification remains pending. |
| Broad device/aspect-ratio coverage | Pending. UI uses landscape scaling and safe-area anchors; test phones/tablets before publishing. |
| Huawei achievements/results | Explicit placeholder only. HMS SDK registration/integration is a later task. |

## Known prototype limits

- No football ball, scoring, timers, bots, spectator population, basketball arena or golf island.
- No host migration, client prediction, reconnect/resume or dedicated servers.
- One static full-size stadium. Seats are consolidated by material, so editing individual seats requires Blender editing/regeneration rather than runtime object manipulation.
- Stadium signs use runtime world-space UI. Font legibility and face orientation should be reviewed at close range on the phone before release.
- The startup art uses solid colours, not image textures. It does not depend on an external asset pack.
- Scene rebuild and art regeneration intentionally replace generated outputs; save manual variants separately.
