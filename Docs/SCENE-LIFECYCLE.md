# Scene cleanup across all four games

Unity's non-null object checks do not mean that an object can still be activated:
an arena and its UI can remain addressable while destruction is already underway.
Cleanup must release owned resources without starting normal gameplay restoration.

[SceneLifetime](../Game/Assets/_Game/Shared/Core/SceneLifetime.cs) records application
shutdown and islands being unloaded by
[SkySailStreaming](../Game/Assets/_Game/Shared/Core/SkySailStreaming.cs). Normal
presentation restoration checks that the target scene is still available. The
unload marker is established before Unity's destruction callbacks and released
after unloading, including failed operations. It is reset for every play session.

The runtime lifecycle audit found and fixed:

- Football stadium destruction reactivated the original arena screen. Cleanup
  now only releases its owned displays, camera, render texture and fireworks.
- Fishing game/presentation disabling restored the fish comparison GameObject.
  Normal idle/reset still restores it; disable/unload cleanup does not.
- Fishing results disabling called normal Close, which reopened another widget.
  Lifecycle hiding cannot reopen results, including delayed UI updates. The
  guard checks both the shared UI scene and its bound fishing arena, since they
  have different lifetimes.
- Shared teleport disabling could roll back the sport, reactivate islands and
  rebuild HUDs during shutdown. Teardown releases effects and references without
  scene/UI restoration or room reports. Live cancellation retains rollback,
  input/camera restoration and normal shared travel behavior.

Golf club/cart cleanup, basketball hoop/net and ball cleanup, replay renderer
restoration, camera and lighting teardown were audited as well. Their existing
cleanup does not reactivate GameObjects; renderer and pose restoration remain.
This change preserves those behaviors and tests their actual island unloads.

Build the complete review player with `StadiumTeardownBuild.BuildReview`, then run:

```powershell
Tools/Build/Test-StadiumTeardown.ps1
Tools/Build/Test-SceneLifecycle.ps1
Tools/Build/Test-RoomTeleport.ps1 -PlayerPath <review-player>
Tools/Build/Build.ps1 -Target AndroidSubmission
Tools/Build/Check-TaskReady.ps1 -RequireApk
```

The lifecycle suite visits and unloads all four islands, checks normal fishing
reset/results versus disable, runs golf with live equipment, holds a basketball,
tests teleport rollback after arrival and quits during fishing results, golf,
basketball and a committed teleport. The stadium suite also quits with an active
MVP feed. Launchers inspect logs and shutdown assertions after the process exits,
so errors raised after a completion marker still fail validation.

Each review run uses its own development content cache. Host and guest share
that run's cache, while reviews of other player versions cannot prune bundles
that the run is still streaming. These caches and logs remain under `Builds/`.

Current logs, source hashes and delivery evidence stay under ignored
`Builds/StadiumTeardownQA/`. Device appearance/performance and WAN tests require
separate evidence.
