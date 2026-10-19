# Latest-main integration — 20 October 2026

Historical protocol-71 record. The current Golf/Fishing merge uses protocol
**72**; see [current publishing record](GOLF-MAIN-PUBLISH.md).

Pulled `origin/main` commit `43eb9d2` (Fishing controls, shared lagoon and natural
swimming) above the previous local base `9bb5564`. Preserved the existing local
audio/one-second Lock-on, responsiveness/goal markers, shared-room menus and
Huawei Scan Kit QR changes. Both sides' historical size/validation records are
retained. Current sharing instructions use the complete player folder instead
of an older protocol-specific ZIP.

The combined room protocol is **71**. Protocol 69 belongs to the older local
release, and protocol 70 to the standalone Fishing update. Everyone joining a
room must use the same combined build. Match authority, four islands and shared
travel remain unchanged.

The merge was a fast-forward to the remote commit followed by reapplication of
a recovery stash containing all unpublished local files. Conflicts were limited
to three documentation files and the protocol constant. The generated player
builder retained both responsive character setup and the new Fishing release
description.

Integration evidence is under ignored `Builds/GitSyncQA/`. Before rebuilding,
the actual local APK measured **90,543,496 bytes**; older Fishing-only release
measurements are different snapshots. A relocated fixture verified relative
documentation links and both review-builder path modes from a path
containing spaces while launched from an unrelated working directory.

The complete protocol-71 player was built and copied to `Builds/WindowsFinal/`;
all **327 files** matched the tested candidate before its build note was updated.

Windows integration checks passed:

| Review | Passed checks |
| --- | ---: |
| Fishing rendered gameplay, rules and local host/guest | 2,749 offline, 46 host, 49 guest |
| Fishing presentation/camera/results | 192 |
| Shared menus | 1,053 offline, 196 host, 193 guest |
| Basketball audio, one-second aim and host/guest events | 59 |

Room QR generation/manual-join/fallback checks passed **31 assertions**, bringing
the total above to **4,568**. Both directions of
protocol-69/71 mismatch rejected the incompatible client before spawning. No
live online room was created by this integration test; existing live QR/WAN
evidence remains in the original reports. Relevant output and test-run paths
are recorded in `Builds/GitSyncQA/`.

Reviewed archives:

- `Legacy/20261019-235948-781-git-sync-candidate/`: old audio-only candidate.
- `Legacy/20261020-000738-460-git-sync-delivery/`: previous local final Windows
  player, APK and contributor audit.

Fresh Android submission completed at **00:10:48 Malaysia time**:
**90,543,496 → 90,579,220 bytes (+35,724)**, leaving **909,420,780 bytes** under
the strict 1,000,000,000-byte cap. Build, size gate, ZIP integrity and independent
APK v2 signature verification passed. SHA-256:
**506492C679C637F90CB17B974B64F3F407BA255E2A9BD5379DEEB1C14D7AEE75**.
IL2CPP grows 30,357 compressed bytes, metadata 2,581 and the bundle 2,486;
Unity runtime is unchanged. No unexpected large contributor growth was found.

All **487 C# file hashes** still match the tested Windows snapshot. Unity's
Golf ball prefab rewrite was compared against the opening recovery snapshot:
all 14 object/component documents are identical after resolving local IDs.
Its opening bytes were restored, preserving the user's original pending
prefab change; the generated copy and comparison remain in the evidence.
No authoring asset content or existing local source work was discarded.

The relocated path/link check passed **549 relative links** and both default/
script-refresh path modes. `Check-TaskReady.ps1 -RequireApk` passed, including
portable paths, metadata/LFS and repository hygiene checks. The user explicitly
authorized committing and pushing this integration. Build output and recovery
archives remain ignored; source publication does not upload those binaries.
Physical Android camera/audio/appearance/FPS and live WAN remain unverified.
