# UDP receive buffer recovery

This is the complete Unity Transport 2.7.3 package embedded from Unity's package
registry. Its original license, source, documentation and metadata are retained.
The only changed upstream source is `Runtime/UDPNetworkInterface.cs`.
`package.json` retains Unity's resolved cache metadata; this note and its Unity
meta file are added. Registry archive signatures are not copied because this
embedded package intentionally contains a source patch.

When a completed UDP receive fails, its buffer must be released before skipping
the result. It never enters `ReceiveQueue`, so normal queue cleanup cannot
release it. On Windows, ICMP errors from a terminated client could exhaust the
host's receive pool and time out every other client. Rejected zero-length and
oversized completions now return their buffers for the same reason.

Unity confirmed the failed-completion fix in this
[upstream report](https://discussions.unity.com/t/unity-transport-6-6-0-udp-host-stops-receiving-after-abrupt-client-disconnect-on-windows/1736023/3).
The available 2.7.4 changelog does not include it. This narrow backport keeps the
project's existing 2.7.3 dependency set and wire behavior. It applies to desktop
and Android without modifying an ignored package cache or system firewall.

Run `Tools/Build/Test-SpectatorCapacity.ps1 -AbruptDisconnect` from the repository
with the current development player, once for each sport. The runner checks
player and spectator process termination, survivors, replacement admission and
ongoing replication past the disconnect timeout, including island exploration.
See `Docs/DISCONNECTS.md` (paths are relative to the repository root).

When adopting an official release containing the fix, compare these receive
branches, remove the embedded package, update the manifest/lock and repeat the
capacity tests and both builds. Do not silently replace this with registry 2.7.3.
