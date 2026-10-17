# Approved Golf obstacle V2 layout

[ObstacleV2.png](ObstacleV2.png) is the approved 43-zone design reference.
Orange outlines retain the first proposal; magenta outlines add 21 zones.
The editable [course layout](../../../Game/Assets/_Game/Sports/Golf/FiveHoleCourse.json)
records `obstacleZones`, terrain-conforming sand/rough footprints and individual
shared-kit rock/palm instances. Positions use Golf island local metres.

Implementation is additive: existing hole/tee/route/bypass/green/flag/sign/terrain
records remain. Each rock/palm zone is populated with three reused kit props;
rough zones also retain the original meadow and existing rough-resistance value.
Sand uses the existing surface shader/texture/resistance without excavating terrain.
The reference is authoring-only and is not shipped in Unity.

## Approved V5 obstacle and pond reference

[Full plan](ObstacleWaterV5.png), [holes 3/5 detail](ObstacleWaterV5-detail.png),
and [editable proposal coordinates](obstacle-water-v5-proposal.json) retain the
V3/V4 proposals and the final 4 + 8 additions on holes 3 and 5. The dry D markers
are illustrations only; runtime anchors track each ball's real safe dry contacts.
V5 adds 47 zones / 141 kit props and 14 rough patches to V2, plus six small ponds.
These authoring references are not shipped in Unity.

Water uses the existing LagoonWater shader with an opaque pond material and
small gentle ripples. Shared conforming sand banks contain flat water on the
existing slope; the original terrain meshes remain unchanged. Generated meshes,
materials and their metas live in the active course art folder. No new texture,
reflection camera, model dependency or network protocol is introduced.
