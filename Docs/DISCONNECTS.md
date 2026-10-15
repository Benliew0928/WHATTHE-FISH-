# Room disconnect recovery

A departing guest releases only their numbered place. Players and spectators
use the same connection lifecycle. A graceful close is processed immediately;
an unreachable or killed guest is removed after the existing 30-second
transport timeout. Surviving participants retain their room and roles. In a
waiting room, another guest can take the free place. After exploration starts,
the existing admission lock remains in effect until the host returns everyone
to the waiting room.

The game now supports [automatic host succession](HOST-MIGRATION.md): the lowest
occupied player number takes over, with spectator numbers as the fallback.
An interrupted group teleport restores the surviving participants.

## Cause and fix

The host could stop receiving all UDP traffic after a peer process disappeared.
Failed native receives kept their packet buffers acquired. Because those
results bypass the normal receive queue, queue cleanup never released them.
Repeated socket errors could exhaust the pool, leaving a listening host that
could neither hear healthy guests nor accept replacements.

The embedded [Unity Transport package](../Game/Packages/com.unity.transport/LOCAL-PATCH.md)
returns these completed buffers immediately, including discarded empty or
oversized packets. Unity confirmed the failed-completion fix in its
[upstream bug discussion](https://discussions.unity.com/t/unity-transport-6-6-0-udp-host-stops-receiving-after-abrupt-client-disconnect-on-windows/1736023/3).
The project retains version 2.7.3 and its dependencies with this narrow source
backport. It does not rely on changes to `Library/PackageCache`, a larger timeout
or system firewall settings. Protocol **65** keeps this delivery paired with
matching clients and hosts.

Full-capacity Relay testing also found unnecessary reliable traffic from default
basketball and golf ball snapshots on every avatar. Unity's [quaternion rotation
operator](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/quaternion/op-equality)
compares the dot product against one; two default zero quaternions compare unequal.
The snapshots now use component equality, so inactive balls stop being marked
dirty every network tick. The capacity probe checks this default-state invariant.

Travel cleanup also checks that each network transform is still spawned and
authoritative before publishing its restored position. NGO can despawn a
departing guest before Unity destroys the character. Trying to teleport that
object in the interim used to throw and interrupt restoration of the room's
camera and controls. Local presentation cleanup still runs for surviving
objects, and a departed object's network transform is skipped.

## Reproduce and verify

Build a candidate with `ProjectBuilder.BuildDisconnectVerification` through
Unity batch mode, or use the complete current `Builds/WindowsFinal/` player.
Run from any working directory with a suitable script path:

```powershell
Tools/Build/Test-SpectatorCapacity.ps1 -Sport Football -AbruptDisconnect
Tools/Build/Test-SpectatorCapacity.ps1 -Sport Basketball -AbruptDisconnect
Tools/Build/Test-SpectatorCapacity.ps1 -Sport Golf -AbruptDisconnect
Tools/Build/Test-SpectatorCapacity.ps1 -Sport Fishing -AbruptDisconnect
```

`-PlayerPath` selects a candidate and `-Port` avoids an occupied local port.
Each runner launches real independent host/client processes: twenty for each
court/golf room or fifteen for fishing. It verifies unique player/spectator
places, overflow rejection, forcibly kills a playing guest and then a watching
guest, and fills each vacancy. It starts the entire room through the real
loading and teleport barrier, kills both roles together on the island, checks
the survivors remain exploring, and returns to the lobby to refill both places.

After every departure/replacement cycle it waits longer than the transport
timeout. Complete, continuously refreshed probe files distinguish a live peer
from a stale roster. Every survivor submits an owner RPC and every peer must
observe every resulting nickname value, testing traffic in both directions.
Logs retain transport disconnect reasons. Omitting `-AbruptDisconnect` runs the
same scenarios through normal room departure. The probe is excluded from
non-development builds.

Evidence stays under ignored `Builds/DisconnectQA/`. These tests establish local
Windows process behavior, not physical Android or WAN/Relay performance.
Measured delivery results are recorded in [verification](VERIFICATION.md) and
[build size](BUILD-SIZE.md).
