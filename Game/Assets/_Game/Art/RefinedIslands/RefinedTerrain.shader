Shader "WhatTheFish/RefinedTerrain" {
 Properties {
  _BaseMap("Macro color",2D)="white"{} _Control("Surface weights",2D)="black"{}
  _Turf("Turf detail",2D)="white"{} _Sand("Sand detail",2D)="white"{}
  _TurfNormal("Turf normal",2D)="bump"{} _SandNormal("Sand normal",2D)="bump"{}
  _Tile("Tile metres",Float)=4 _BaseColor("Tint",Color)=(1,1,1,1)
 }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
 Pass {Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);TEXTURE2D(_Control);SAMPLER(sampler_Control);
 TEXTURE2D(_Turf);SAMPLER(sampler_Turf);TEXTURE2D(_Sand);SAMPLER(sampler_Sand);
 TEXTURE2D(_TurfNormal);SAMPLER(sampler_TurfNormal);TEXTURE2D(_SandNormal);SAMPLER(sampler_SandNormal);
 CBUFFER_START(UnityPerMaterial)
 float _Tile;float4 _BaseColor;
 CBUFFER_END
 struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};
 struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;float fog:TEXCOORD3;};
 V vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.n=TransformObjectToWorldNormal(a.n);o.uv=a.uv;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
 half4 frag(V i):SV_Target {
  float2 uv=i.world.xz/_Tile;half sand=SAMPLE_TEXTURE2D(_Control,sampler_Control,i.uv).r;
  half3 turf=SAMPLE_TEXTURE2D(_Turf,sampler_Turf,uv).rgb,grit=SAMPLE_TEXTURE2D(_Sand,sampler_Sand,uv).rgb;
  // Normalize by the measured PNG mean, decoded into Unity's linear colour space.
  half3 variation=lerp(turf/SRGBToLinear(half3(.409,.582,.236)),grit/SRGBToLinear(half3(.815,.711,.523)),sand);
  variation=1+(variation-1)*2.2;
  half3 albedo=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb*clamp(variation,.7,1.3)*_BaseColor.rgb;
  half3 bump=normalize(lerp(UnpackNormal(SAMPLE_TEXTURE2D(_TurfNormal,sampler_TurfNormal,uv)),UnpackNormal(SAMPLE_TEXTURE2D(_SandNormal,sampler_SandNormal,uv)),sand));
  InputData d=(InputData)0;d.positionWS=i.world;d.normalWS=normalize(i.n+float3(bump.x,0,bump.y)*.38);d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);
  d.shadowCoord=TransformWorldToShadowCoord(i.world);d.bakedGI=SampleSH(d.normalWS);d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);d.shadowMask=1;
  SurfaceData s=(SurfaceData)0;s.albedo=albedo;s.alpha=1;s.occlusion=1;s.smoothness=lerp(.16,.07,sand);s.normalTS=half3(0,0,1);
  half4 color=UniversalFragmentPBR(d,s);color.rgb=MixFog(color.rgb,i.fog);return color;
 }
 ENDHLSL }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 UsePass "Universal Render Pipeline/Lit/DepthNormals"
 }
}
