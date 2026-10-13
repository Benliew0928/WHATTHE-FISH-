# Numbered reference hole flags

The five unmodified approved masters are retained in `References/hole-1.png`
through `hole-5.png`. `GolfFlagArtBuilder` samples their flag faces into five
1024 x 384 front/back delivery textures, preserving the original silhouette,
cream trim, numerals and painted fabric highlights. Rear numerals are reversed
independently of the physical silhouette, with background repair around the ink.
Delivery textures use mipmaps and Android ASTC 6x6. Keep the masters editable;
no full-size master is imported into Unity.

The shared subdivided cloth has 40 x 28 cells. The shader clips the reference
alpha in colour, shadow and depth passes. The silver pole and teal bindings
have smooth normals and remain stationary. The cloth displacement envelope is
`((x - .055) / 1.065)^2`, clamped to [0,1]: zero at the pole, one at the tip.
Two travelling waves have amplitudes .144 and .060 metres, six times the former
.024 and .010 metres. Maximum tip displacement is therefore .204 m, with a
.408 m theoretical peak-to-peak bound. At half width the envelope is .25.
Broader folds reduce sharp creases. This is a procedural wind approximation,
not a mass/spring cloth solver; it does not simulate collisions or tearing.

Run `GolfFlagArtReview.PrepareAndReview` for saved-scene/prefab updates, course
checks, all five front/back views and two timed wind captures. Preview time
returns to the game clock before saving. Evidence stays in ignored
`Builds/GolfFlagBounceQA/`.
