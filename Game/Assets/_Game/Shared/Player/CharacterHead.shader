Shader "WhatTheFish/CharacterHead" {
 Properties {
  _BaseColor("Hair or skin",Color)=(.1,.07,.05,1)
  _DetailColor("Accessory trim",Color)=(.85,.63,.28,1)
  _Smoothness("Smoothness",Range(0,1))=.3
 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
  CBUFFER_START(UnityPerMaterial)
  half4 _BaseColor,_DetailColor;half _Smoothness;
  CBUFFER_END
  struct Attributes{float4 positionOS:POSITION;float3 normalOS:NORMAL;half4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
  struct Varyings{float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;half4 color:COLOR;half fog:TEXCOORD2;half3 vertexLighting:TEXCOORD3;};
  Varyings Vert(Attributes v){UNITY_SETUP_INSTANCE_ID(v);Varyings o;o.positionWS=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.normalWS=TransformObjectToWorldNormal(v.normalOS);o.color=v.color;o.fog=ComputeFogFactor(o.positionCS.z);o.vertexLighting=VertexLighting(o.positionWS,o.normalWS);return o;}
  half4 Frag(Varyings i):SV_Target{
   half3 normal=normalize(i.normalWS);
   InputData input=(InputData)0;input.positionWS=i.positionWS;input.normalWS=normal;input.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);input.shadowCoord=TransformWorldToShadowCoord(i.positionWS);input.bakedGI=SampleSH(normal);input.vertexLighting=i.vertexLighting;input.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);input.shadowMask=half4(1,1,1,1);
   SurfaceData surface=(SurfaceData)0;surface.albedo=lerp(_BaseColor.rgb,_DetailColor.rgb,i.color.a)*i.color.rgb;surface.smoothness=_Smoothness;surface.normalTS=half3(0,0,1);surface.occlusion=1;surface.alpha=1;
   half4 result=UniversalFragmentPBR(input,surface);result.rgb=MixFog(result.rgb,i.fog);return result;
  }
  half4 Depth(Varyings i):SV_Target{return i.positionCS.z;}
  half4 Normals(Varyings i):SV_Target{return half4(normalize(i.normalWS),0);}
  ENDHLSL
  Pass {
   Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma multi_compile_instancing
   #pragma target 3.0
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
   #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
   #pragma multi_compile_fog
   ENDHLSL
  }
  Pass {
   Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ZWrite On ColorMask 0
   HLSLPROGRAM
   #pragma multi_compile_instancing
   #pragma vertex Shadow
   #pragma fragment Depth
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   float3 _LightDirection;float3 _LightPosition;
   Varyings Shadow(Attributes v){Varyings o=Vert(v);float3 direction=_LightDirection;
   #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
    direction=normalize(_LightPosition-o.positionWS);
   #endif
    o.positionCS=TransformWorldToHClip(ApplyShadowBias(o.positionWS,o.normalWS,direction));
   #if UNITY_REVERSED_Z
    o.positionCS.z=min(o.positionCS.z,UNITY_NEAR_CLIP_VALUE);
   #else
    o.positionCS.z=max(o.positionCS.z,UNITY_NEAR_CLIP_VALUE);
   #endif
    return o;}
   ENDHLSL
  }
  Pass {
   Name "DepthOnly" Tags {"LightMode"="DepthOnly"} ZWrite On ColorMask R
   HLSLPROGRAM
   #pragma multi_compile_instancing
   #pragma vertex Vert
   #pragma fragment Depth
   ENDHLSL
  }
  Pass {
   Name "DepthNormals" Tags {"LightMode"="DepthNormals"} ZWrite On
   HLSLPROGRAM
   #pragma multi_compile_instancing
   #pragma vertex Vert
   #pragma fragment Normals
   ENDHLSL
  }
 }
}
