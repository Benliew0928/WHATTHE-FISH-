Shader "WhatTheFish/GolfFlagWind" {
 Properties { _FlagTime("Preview time (-1 uses game clock)",Float)=-1 _BaseMap("Approved front and back artwork",2D)="white" {} }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
  Cull Off
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
  CBUFFER_START(UnityPerMaterial)
   float _FlagTime;
  CBUFFER_END
  TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
  struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
  struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; float2 uv:TEXCOORD2; };
  float3 Wave(float3 p) {
   float time=_FlagTime<0?_Time.y:_FlagTime;
   float phase=dot(float2(unity_ObjectToWorld._m03,unity_ObjectToWorld._m23),float2(.071,.043));
   float weight=saturate((p.x-.055)/1.065);weight*=weight;
   p.z+=weight*(.144*sin(p.x*3.5-time*1.65+phase)+.060*sin(p.y*4+p.x*2.5-time*2.1+phase));
   return p;
  }
  Varyings Vert(Attributes input) {
   UNITY_SETUP_INSTANCE_ID(input);
   Varyings output;float3 p=Wave(input.positionOS.xyz);
   output.positionWS=TransformObjectToWorld(p);output.positionCS=TransformWorldToHClip(output.positionWS);
   float3 dx=Wave(input.positionOS.xyz+float3(.001,0,0))-p;
   float3 dy=Wave(input.positionOS.xyz+float3(0,.001,0))-p;
   output.normalWS=TransformObjectToWorldNormal(normalize(cross(dx,dy))*sign(input.normalOS.z));
   output.uv=input.uv;return output;
  }
  ENDHLSL
  Pass {
   Name "ForwardLit" Tags { "LightMode"="UniversalForward" }
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_instancing
   half4 Frag(Varyings input, FRONT_FACE_TYPE facing:FRONT_FACE_SEMANTIC):SV_Target {
    half4 artwork=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,float2(input.uv.x*.5+IS_FRONT_VFACE(facing,0,.5),input.uv.y));clip(artwork.a-.5);
    half3 normal=normalize(input.normalWS)*IS_FRONT_VFACE(facing,1,-1);
    Light light=GetMainLight();
    half diffuse=saturate(dot(normal,light.direction));
    // Thin two-sided fabric keeps its colour legible without self-shadow acne.
    // The separate caster still projects the waving silhouette onto the green.
    half3 lighting=SampleSH(normal)+light.color*(.22+.78*diffuse);
    return half4(artwork.rgb*lerp(half3(1,1,1),lighting,.35),1);
   }
   ENDHLSL
  }
  Pass {
   Name "ShadowCaster" Tags { "LightMode"="ShadowCaster" }
   ZWrite On ColorMask 0
   HLSLPROGRAM
   #pragma vertex ShadowVert
   #pragma fragment ShadowFrag
   #pragma multi_compile_instancing
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   float3 _LightDirection;float3 _LightPosition;
   struct ShadowOutput {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
   ShadowOutput ShadowVert(Attributes input) {
    UNITY_SETUP_INSTANCE_ID(input);
    float3 world=TransformObjectToWorld(Wave(input.positionOS.xyz));
    float3 normal=TransformObjectToWorldNormal(input.normalOS);
    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
     float3 direction=normalize(_LightPosition-world);
    #else
     float3 direction=_LightDirection;
    #endif
    float4 clip=TransformWorldToHClip(ApplyShadowBias(world,normal,direction));
    #if UNITY_REVERSED_Z
     clip.z=min(clip.z,UNITY_NEAR_CLIP_VALUE*clip.w);
    #else
     clip.z=max(clip.z,UNITY_NEAR_CLIP_VALUE*clip.w);
    #endif
    ShadowOutput output;output.positionCS=clip;output.uv=input.uv;return output;
   }
   half4 ShadowFrag(ShadowOutput input):SV_Target{clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,float2(input.uv.x*.5,input.uv.y)).a-.5);return 0;}
   ENDHLSL
  }
  Pass {
   Name "DepthOnly" Tags { "LightMode"="DepthOnly" }
   ZWrite On ColorMask 0
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment DepthFrag
   #pragma multi_compile_instancing
   half4 DepthFrag(Varyings input):SV_Target{clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,float2(input.uv.x*.5,input.uv.y)).a-.5);return 0;}
   ENDHLSL
  }
 }
}
