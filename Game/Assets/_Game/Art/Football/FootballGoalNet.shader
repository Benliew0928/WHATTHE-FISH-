Shader "WhatTheFish/ReactiveGoalNet" {
 Properties { _BaseColor("Rope",Color)=(.88,.94,.96,1) }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
  Cull Off
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;
   float4 _NetSize;
   float4 _Contacts[4],_Impulses[4];
  CBUFFER_END
  float3 Deform(float3 p){
   float3 offset=0;
   [unroll] for(int n=0;n<4;n++){
    float distance=length(p-_Contacts[n].xyz),age=_Time.y-_Contacts[n].w-distance/12;
    if(age>0&&age<1.8){
     float radius=max(.01,_Impulses[n].w);
     float response=(1-exp(-age*35))*exp(-age*4.5)*sin(age*12)*exp(-distance*distance/(radius*radius));
     offset+=_Impulses[n].xyz*response;
    }
   }
   float roof=_NetSize.y-_NetSize.w*saturate(p.z/_NetSize.z),side=abs(abs(p.x)-_NetSize.x);
   float support=min(min(abs(p.z),abs(p.y)),min(length(float2(side,p.y-roof)),length(float2(side,p.z-_NetSize.z))));
   float pin=smoothstep(0,1,saturate(support/.28));
   return p+offset*min(1,.55/max(.0001,length(offset)))*pin;
  }
  half Rope(float2 uv){
   float2 d=abs(frac(uv+.5)-.5),aa=max(fwidth(uv),.01);
   float2 rope=1-smoothstep(.045,.045+aa,d);
   return max(rope.x,rope.y);
  }
  struct Attributes {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
  struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;half3 normal:TEXCOORD2;half fog:TEXCOORD3;};
  Varyings vert(Attributes v){
   Varyings o;o.world=TransformObjectToWorld(Deform(v.positionOS.xyz));o.positionCS=TransformWorldToHClip(o.world);
   o.uv=v.uv;o.normal=TransformObjectToWorldNormal(v.normalOS);o.fog=ComputeFogFactor(o.positionCS.z);return o;
  }
  ENDHLSL
  Pass {
   Tags { "LightMode"="UniversalForward" }
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite On
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   half4 frag(Varyings i):SV_Target{
    half coverage=Rope(i.uv);clip(coverage-.08);
    half3 normal=normalize(cross(ddy(i.world),ddx(i.world)));
    Light light=GetMainLight();half diffuse=.5+.5*abs(dot(normal,light.direction));
    half3 color=_BaseColor.rgb*(.42+diffuse*.58*light.color);
    return half4(MixFog(color,i.fog),coverage);
   }
   ENDHLSL
  }
  Pass {
   Name "ShadowCaster"
   Tags { "LightMode"="ShadowCaster" }
   ZWrite On
   ColorMask 0
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment shadow
   half4 shadow(Varyings i):SV_Target{clip(Rope(i.uv)-.5);return 0;}
   ENDHLSL
  }
  Pass {
   Name "DepthOnly"
   Tags { "LightMode"="DepthOnly" }
   ZWrite On
   ColorMask R
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment depth
   half4 depth(Varyings i):SV_Target{clip(Rope(i.uv)-.5);return i.positionCS.z;}
   ENDHLSL
  }
 }
}
