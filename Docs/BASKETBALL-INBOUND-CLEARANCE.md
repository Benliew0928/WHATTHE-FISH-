# Basketball inbound clearance

During a match restart, opponents must stay outside a circle centred on the
reserved inbound position. Its radius is **25% of the full painted court
length**: 28.6512 m × 0.25 = **7.1628 m**. Each defender's capsule radius and
skin width are added to their centre-distance limit so their body stays out
of the protected area. Receiving teammates are exempt.

Protection starts when the inbound position is reserved, including the
collector's walk to the ball. Opponents already inside walk to the nearest
legal part of the circle within the court. They retain scenery collisions;
the existing stalled-runner recovery can temporarily pass through player
bodies, restoring only its own collision exceptions afterwards. The existing
return-to-half movement after a basket is preserved.

The pass becomes available when the inbounder is ready and opponents have
cleared the circle. The playing clock stays stopped until physical release.
Once outside, opponents can move around the circle, but
walking, sprinting, jumping and swept movement cannot cross into it. The
restriction is enforced on the host using the existing replicated restart
state; no new network payload is needed. The HUD explains automatic clearance
and shows `CLEARING SPACE` when the inbounder is waiting for defenders.

The zone ends at the physical release of the inbound pass. Normal defending
and interception then resume. Cancelling the match clears automatic movement
and collision exceptions. Restoring a room recomputes the clearance from its
saved inbounder and touchline position. Solo practice without teams is unchanged.

Single-tap camera selection and double-tap teammate passing continue to use
the [predictive passing rules](TEAMMATE-PASSING.md).

## Verification

`Tools/Build/Test-BasketballRestart.ps1` exercises the court restart behavior;
add `-Local` for host/guest checks or `-PlayerPath` to use a complete candidate.
The expanded fixture covers all four touchlines and a corner, teammate access,
continuous opponent clearance, sprint/jump and swept re-entry rejection,
release-time removal, catches, clock preservation and room restoration.
Current task evidence is kept under ignored `Builds/InboundClearanceQA/`.

On 23 October 2026, the final combined Windows player passed **816 restart
assertions**: offline 403, host 405 and guest 8. All five clearance scenarios
passed, along with the existing scoring, collector rotation, body-block
recovery and cancellation checks. Basketball passing regressions passed
**120 assertions** (69 offline, 40 host, 11 guest). The coordinated tutorial
checks passed **602 assertions across 58 lessons**. The restart runner used
its default path from a folder with spaces and an unrelated working directory.

Final restart evidence is under `Builds/InboundClearanceQA/Portable Review/Builds/BasketballRestartQA/`:
`Run-20261023-141345-719/` (offline) and `Run-20261023-141345-688/` (host/guest).
Passing evidence: `Builds/TargetPassQA/Basketball-Offline-20261023-141345-726/`
and `Basketball-Local-20261023-141422-374/` under the same root. Tutorial evidence
is indexed by `Builds/TutorialFeedbackQA/tutorial-results.json`.

Initial failures exposed fixture timing assumptions: starting before world
arrival finished and carrying the guest's test input between independent
scoring setups. The fixture now waits for arrival and clears stale commands;
live movement and match rules were not weakened. Rendered gameplay was checked;
physical phone touch/performance and real WAN latency remain unverified.

## Delivery

The complete 327-file tested player is in `Builds/WindowsFinal/`; all payload
files matched the candidate by SHA-256 before its delivery note was updated.
The combined source fingerprint covers 606 C# files. See the
[release size record](BUILD-SIZE.md) for the Android package measurement and
integrity checks. Manifests, signatures and current audit data are retained
under `Builds/InboundClearanceQA/`.

Reviewed superseded outputs were archived with sizes, hashes and recovery
paths in `Legacy/20261023-141310-049-inbound-clearance-candidates/`,
`Legacy/20261023-141543-080-inbound-clearance-android/` and
`Legacy/20261023-141810-027-inbound-clearance-windows/`. Current test evidence
remains in `Builds/`. No staging, commit or push was performed.
